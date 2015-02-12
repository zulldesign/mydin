using System;
using System.Collections.Generic;
using System.Threading;

using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using System.Web.UI;
using Bitrix.CommunicationUtility;
using System.Globalization;
using Bitrix.Components.Editor;

namespace Bitrix.Blog.Components
{
	public partial class BlogCommentsComponent : BXComponent
	{
		private List<CommentInfo> comments = new List<CommentInfo>();
		private bool templateIncluded;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private Exception fatalException;
		private int maxWordLength = -1;
		private BXBlogCommentChain processor;

		public List<CommentInfo> Comments
		{
			get
			{
				return comments;
			}
		}
		/// <summary>
		/// Запрос количества элеметов
		/// </summary>
		public int CommentCount
		{
			get
			{
				object val;
				return ComponentCache.TryGetValue("CommentCount", out val) ? (int)val : comments != null ? comments.Count : 0;
			}
		}

		/// <summary>
		/// Запрос наличия элементов
		/// </summary>
		public bool HasComments
		{
			get { return CommentCount > 0; }
		}

		/// <summary>
		/// Ид автора комментариев
		/// (будут выбраны только комментарии указанного автора)
		/// </summary>
		public int AuthorId
		{
			get { return Parameters.GetInt("AuthorId", 0); }
			set { Parameters["AuthorId"] = value.ToString(); }
		}

		public ErrorCode FatalError
		{
			get
			{
				return fatalError;
			}
		}
		public Exception FatalException
		{
			get
			{
				return fatalException;
			}
		}
		public string BlogUrlTemplate
		{
			get
			{
				return Parameters.GetString("BlogUrlTemplate");
			}
			set
			{
				Parameters["BlogUrlTemplate"] = value;
			}
		}
		public string UserProfileUrlTemplate
		{
			get
			{
				return Parameters.GetString("UserProfileUrlTemplate");
			}
			set
			{
				Parameters["UserProfileUrlTemplate"] = value;
			}
		}
		public string PostViewUrlTemplate
		{
			get
			{
				return Parameters.GetString("PostViewUrlTemplate");
			}
			set
			{
				Parameters["PostViewUrlTemplate"] = value;
			}
		}
		public string CommentUrlTemplate
		{
			get
			{
				return Parameters.GetString("CommentUrlTemplate");
			}
			set
			{
				Parameters["CommentUrlTemplate"] = value;
			}
		}
		public int MaxWordLength
		{
			get
			{
				return (maxWordLength != -1) ? maxWordLength : (maxWordLength = Math.Max(0, Parameters.GetInt("MaxWordLength", 15)));
			}
			set
			{
				maxWordLength = Math.Max(0, value);
				Parameters["MaxWordLength"] = maxWordLength.ToString();
			}
		}
		public int CategoryId
		{
			get
			{
				return Parameters.GetInt("CategoryId");
			}
		}

		internal BXBlogCommentChain Processor
		{
			get
			{
				return processor;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;

			try
			{
				if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.BlogPublicView))
				{
					Fatal(ErrorCode.UnauthorizedViewBlog);
					return;
				}

				BXPagingParams pagingParams = PreparePagingParams();
				var bp = BXBlogUserPermissions.GetBlogsForPostsAndComments(BXIdentity.Current.Id, BXBlogPostPermissionLevel.Read, BXBlogCommentPermissionLevel.Read, true, Page.Items, null);
				if (IsCached(pagingParams, bp.Hash))
				{
					SetTemplateCachedData();
					return;
				}

				using (BXTagCachingScope scope = BeginTagCaching())
				{
					if (CategoryId > 0)
					{
						BXBlogCategory category = BXBlogCategory.GetById(CategoryId);
						if (category == null)
							Fatal(new Exception(string.Format("Could not find category: '{0}'!", CategoryId)));

						category.PrepareForTagCaching(scope);
					}
					else
						scope.AddTag(BXBlogCategory.CreateSpecialTag(BXCacheSpecialTagType.All));

					/* check group permissions */					
					IBXFilterItem blogFilter = null;
					if (bp.Ids.Count > 0)
					{
						blogFilter = new BXFilterItem(BXBlogComment.Fields.Blog.Id, BXSqlFilterOperators.In, bp.Ids);
						if (bp.Exclude)
							blogFilter = new BXFilterNot(blogFilter);
					}


					BXBlogCommentCollection commentsCollection = null;
					if (!bp.Exclude && bp.Ids.Count == 0)
					{
						bool legal;
						BXQueryParams queryParams = PreparePaging(
							pagingParams,
							() => 0,							
							new BXParamsBag<object>(),
							out legal
						);

						if (!legal)
							Fatal(ErrorCode.FatalInvalidPage);
						else
							commentsCollection = new BXBlogCommentCollection();
					}
					else 
						commentsCollection = GetComments(pagingParams, blogFilter);

					if (commentsCollection == null)
						return;


					BXParamsBag<object> replace = new BXParamsBag<object>();

					processor = new BXBlogCommentChain();
					processor.EnableImages = false;
					processor.MaxWordLength = MaxWordLength;
					ComponentCache["CommentCount"] = commentsCollection.Count;
					foreach (BXBlogComment comment in commentsCollection)
					{
						replace["CommentId"] = comment.Id;
						replace["PostYear"] = comment.Post.DatePublished.Year.ToString();
						replace["PostMonth"] = comment.Post.DatePublished.Month.ToString();
						replace["PostDay"] = comment.Post.DatePublished.Day.ToString();
						replace["PostId"] = comment.Post.Id;
						replace["BlogId"] = comment.Blog.Id;
						replace["BlogSlug"] = comment.Blog.Slug;
						replace["UserId"] = comment.AuthorId;
						replace["AuthorId"] = comment.AuthorId;

						CommentInfo CommentInfo = new CommentInfo(this);
						CommentInfo.Comment = comment;
						CommentInfo.AuthorNameHtml = BXWordBreakingProcessor.Break(comment.TextEncoder.Decode(comment.AuthorName), MaxWordLength, true);
						CommentInfo.TitleHtml = BXWordBreakingProcessor.Break(comment.TextEncoder.Decode(comment.Post.Title), MaxWordLength, true);
						CommentInfo.BlogHref = Encode(ResolveTemplateUrl(BlogUrlTemplate, replace));
						CommentInfo.CommentHref = Encode(ResolveTemplateUrl(CommentUrlTemplate, replace));
						CommentInfo.PostViewHref = Encode(ResolveTemplateUrl(PostViewUrlTemplate, replace));
						CommentInfo.UserProfileHref = Encode(ResolveTemplateUrl(UserProfileUrlTemplate, replace));

						if (comment.AuthorBlog != null && CheckAuthorBlog(comment.AuthorBlog))
						{
							replace["BlogId"] = comment.AuthorBlog.Id;
							replace["BlogSlug"] = comment.AuthorBlog.Slug;
							CommentInfo.AuthorBlogHref = Encode(ResolveTemplateUrl(BlogUrlTemplate, replace));
						}
						else if (comment.AuthorId > 0)
						{
							CommentInfo.AuthorBlogHref = Encode(ResolveTemplateUrl(UserProfileUrlTemplate, replace));
						}

						comments.Add(CommentInfo);
					}

					if (!templateIncluded)
					{
						templateIncluded = true;
						IncludeComponentTemplate();
					}

					SetTemplateCachedData();
				}
			}
			catch (Exception ex)
			{
				Fatal(ex);
			}
		}

		private BXBlogCommentCollection GetComments(BXPagingParams pagingParams, IBXFilterItem customFilter)
		{
			BXFilter filter = new BXFilter(
				new BXFilterItem(BXBlogComment.Fields.MarkedForDelete, BXSqlFilterOperators.Equal, false),
				new BXFilterItem(BXBlogComment.Fields.Post.IsPublished, BXSqlFilterOperators.Equal, true),
				new BXFilterItem(BXBlogComment.Fields.Post.DatePublished, BXSqlFilterOperators.LessOrEqual, DateTime.Now),
				new BXFilterItem(BXBlogComment.Fields.Blog.Active, BXSqlFilterOperators.Equal, true),
				new BXFilterItem(BXBlogComment.Fields.Blog.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite),
				new BXFilterItem(BXBlogComment.Fields.IsApproved, BXSqlFilterOperators.Equal, true)
			);

			if (customFilter != null)
				filter.Add(customFilter);

			if (CategoryId > 0)
				filter.Add(new BXFilterItem(BXBlogComment.Fields.Blog.Categories.Category.Id, BXSqlFilterOperators.Equal, CategoryId));

			int authorId = AuthorId;
			if (authorId > 0)
				filter.Add(new BXFilterItem(BXBlogComment.Fields.Author.Id, BXSqlFilterOperators.Equal, authorId));

			bool legal;
			BXQueryParams queryParams = PreparePaging(
				pagingParams,
				delegate()
				{
					return BXBlogComment.Count(filter);
				},
				new BXParamsBag<object>(),
				out legal
			);

			if (!legal)
			{
				Fatal(ErrorCode.FatalInvalidPage);
				return null;
			}

			return BXBlogComment.GetList(
				filter,
				new BXOrderBy(new BXOrderByPair(BXBlogComment.Fields.Id, BXOrderByDirection.Desc)),
				new BXSelectAdd(
					BXBlogComment.Fields.Blog,
					BXBlogComment.Fields.Post,
					BXBlogComment.Fields.Author,
					BXBlogComment.Fields.Author.User,
					BXBlogComment.Fields.Author.User.Image,
					BXBlogComment.Fields.AuthorBlog.Id,
					BXBlogComment.Fields.AuthorBlog.Slug,
					BXBlogComment.Fields.AuthorBlog.Categories.Category.Sites.SiteId
				),
				queryParams
			);
		}

		private void SetTemplateCachedData()
		{
			BXPublicPage bitrixPage = Page as BXPublicPage;
			if (bitrixPage != null && !IsComponentDesignMode && Parameters.Get<bool>("SetPageTitle", true))
				bitrixPage.BXTitle = BXTextEncoder.HtmlTextEncoder.Decode(GetMessageRaw("PageTitle"));
		}

		private bool CheckAuthorBlog(BXBlog blog)
		{
			foreach (BXBlogCategory c in blog.Categories)
				if (Array.IndexOf(c.GetSiteIds(), DesignerSite) != -1)
					return true;
			return false;
		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			foreach (string param in new string[] { Parameters.GetString("ThemeCssFilePath"), Parameters.GetString("ColorCssFilePath") })
			{
				if (BXStringUtility.IsNullOrTrimEmpty(param))
					continue;
				string path = param;
				try
				{
					path = BXPath.ToVirtualRelativePath(path);
					if (BXSecureIO.FileExists(path))
						BXPage.RegisterStyle(path);
				}
				catch
				{
				}
			}
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Group = BXBlogModuleConfiguration.GetComponentGroup();

			BXCategory mainCategory = BXCategory.Main;
			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

			ParamsDefinition["ThemeCssFilePath"] = new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/style.css", mainCategory);
			ParamsDefinition["ColorCssFilePath"] = new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", mainCategory);
			ParamsDefinition["AuthorId"] = new BXParamText(GetMessageRaw("Param.AuthorId"), "0", mainCategory);
			ParamsDefinition["CategoryId"] = new BXParamSingleSelection(GetMessageRaw("Param.CategoryId"), "", mainCategory);

			ParamsDefinition["BlogUrlTemplate"] = new BXParamText(GetMessageRaw("Param.BlogUrlTemplate"), "Blog.aspx?blog=#blogSlug#", urlCategory);
			ParamsDefinition["UserProfileUrlTemplate"] = new BXParamText(GetMessageRaw("Param.UserProfileUrlTemplate"), "User.aspx?user=#userId#", urlCategory);
			ParamsDefinition["PostViewUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostViewUrlTemplate"), "Post.aspx?post=#postId#", urlCategory);
			ParamsDefinition["CommentUrlTemplate"] = new BXParamText(GetMessageRaw("Param.CommentUrlTemplate"), "Post.aspx?post=#PostId#&comment=#CommentId###comment#CommentId#", urlCategory);

			ParamsDefinition["MaxWordLength"] = new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory);
			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			//Cache
			ParamsDefinition.Add(BXParametersDefinition.Cache);
		}
		protected override void LoadComponentDefinition()
		{
			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			IList<BXParamValue> categoryIds = ((BXParamSingleSelection)ParamsDefinition["CategoryId"]).Values;
			if (categoryIds.Count > 0)
				categoryIds.Clear();

			BXBlogCategoryCollection categories = BXBlogCategory.GetList(
			   new BXFilter(new BXFilterItem(BXBlogCategory.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)),
			   new BXOrderBy(new BXOrderByPair(BXBlogCategory.Fields.Name, BXOrderByDirection.Asc)),
			   new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlogCategory.Fields.Id, BXBlogCategory.Fields.Name),
			   null,
			   BXTextEncoder.EmptyTextEncoder
			   );
			categoryIds.Add(new BXParamValue(GetMessage("Kernel.All"), ""));
			foreach (BXBlogCategory category in categories)
			{
				BXParamValue value = new BXParamValue(category.Name, category.Id.ToString());
				categoryIds.Add(value);
			}

		}

		private void Fatal(ErrorCode code)
		{
			if (code == ErrorCode.FatalException)
				throw new InvalidOperationException("Use method with Exception argument");

			AbortCache();

			fatalError = code;
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}
		private void Fatal(Exception ex)
		{
			if (ex == null)
				throw new ArgumentNullException("ex");

			AbortCache();

			fatalError = ErrorCode.FatalException;
			fatalException = ex;
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}

		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.UnauthorizedViewBlog:
					return GetMessage("Error.UnauthorizedViewBlog");
				case ErrorCode.FatalInvalidPage:
					return GetMessage("Error.FatalInvalidPage");
				default:
					return GetMessage("Error.Unknown");
			}
		}

		//NESTED CLASSES
		public class CommentInfo
		{
			private BXBlogComment comment;
			private string postViewHref;
			private string userProfileHref;
			private string blogHref;
			private string commentHref;
			private string authorBlogHref;
			private string authorNameHtml;
			private string contentHtml;
			private string titleHtml;
			private BlogCommentsComponent component;

			public BXBlogComment Comment
			{
				get
				{
					return comment;
				}
				internal set
				{
					comment = value;
				}
			}
			public BXBlogPost Post
			{
				get
				{
					return comment.Post;
				}
			}
			public BXBlog Blog
			{
				get
				{
					return comment.Blog;
				}
			}
			public BXBlogUser Author
			{
				get
				{
					return comment.Author;
				}
			}
			public string PostViewHref
			{
				get
				{
					return postViewHref;
				}
				internal set
				{
					postViewHref = value;
				}
			}
			public string UserProfileHref
			{
				get
				{
					return userProfileHref;
				}
				internal set
				{
					userProfileHref = value;
				}
			}
			public string AuthorBlogHref
			{
				get
				{
					return authorBlogHref;
				}
				internal set
				{
					authorBlogHref = value;
				}
			}
			public string BlogHref
			{
				get
				{
					return blogHref;
				}
				internal set
				{
					blogHref = value;
				}
			}
			public string CommentHref
			{
				get
				{
					return commentHref;
				}
				internal set
				{
					commentHref = value;
				}
			}
			public string AuthorNameHtml
			{
				get
				{
					return authorNameHtml;
				}
				internal set
				{
					authorNameHtml = value;
				}
			}
			public string TitleHtml
			{
				get
				{
					return titleHtml;
				}
				internal set
				{
					titleHtml = value;
				}
			}
			public string ContentHtml
			{
				get
				{
					if (contentHtml == null)
						contentHtml = component.Processor.Process(comment.Content);
					return contentHtml;
				}
			}
			public string GetPreviewHtml(int length)
			{
				string preview = component.Processor.StripBBCode(comment.Content);
				preview = BXStringUtility.StripOffSimpleTags(preview);

				if (length > 0 && preview.Length > length)
					preview = preview.Substring(0, length) + " ...";

				return BXWordBreakingProcessor.Break(preview, component.MaxWordLength, true);
			}

			internal CommentInfo(BlogCommentsComponent component)
			{
				this.component = component;
			}
		}
		public enum ErrorCode
		{
			None = 0,
			Fatal = 1,
			Unauthorized = 2,

			FatalInvalidPage = Fatal | 4,
			FatalException = Fatal | 8,
			FatalComponentNotExecuted = Fatal | 16,

			UnauthorizedViewBlog = Unauthorized | 4,
		}
	}

	public class BlogCommentsTemplate : BXComponentTemplate<BlogCommentsComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & BlogCommentsComponent.ErrorCode.Fatal) != 0)
				BXError404Manager.Set404Status(Response);
		}
		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			base.Render(writer);
		}
	}
}
