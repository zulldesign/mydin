using System;
using System.IO;
using System.Web.UI;
using Bitrix.CommunicationUtility;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.Services;
using System.Collections.Generic;
using System.Web;
using Bitrix.Modules;
using Bitrix.Components.Editor;

namespace Bitrix.Blog.Components
{
	public partial class BlogPostComponent : BXComponent
	{
		private string blogSlug;
		private int? postId;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private Exception fatalException;
		private bool templateIncluded;
		private BXBlog blog;
		private BXBlogPost post;
		private PostInfo postInfo;
		private BXBlogPostChain bbcodeProcessor;
		private BXBlogPostHtmlProcessor htmlProcessor;
		private BXBlogPostFullHtmlProcessor fullHtmlProcessor;
		private IBXBlogPostProcessor processor;
		private int maxWordLength = -1;
		private BXBlogAuthorization auth;
		private BXParamsBag<object> replace;

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
		public string BlogSlug
		{
			get
			{
				return blogSlug ?? (blogSlug = Parameters.GetString("BlogSlug"));
			}
		}
		public int PostId
		{
			get
			{
				return (postId ?? (postId = Parameters.GetInt("PostId"))).Value;
			}
		}

		public int CategoryId
		{
			get { return Parameters.GetInt("CategoryId", 0); }
		}

		public BXBlog Blog
		{
			get
			{
				return blog;
			}
			set
			{
				blog = value;
			}
		}
		public BXBlogPost BlogPost
		{
			get
			{
				return post;
			}
			set
			{
				post = value;
			}
		}
		public PostInfo Post
		{
			get
			{
				return postInfo;
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
		public string PostEditUrlTemplate
		{
			get
			{
				return Parameters.GetString("PostEditUrlTemplate");
			}
			set
			{
				Parameters["PostEditUrlTemplate"] = value;
			}
		}

		public string RssUrlTemplate
		{
			get
			{
				return Parameters.GetString("RssUrlTemplate");
			}
			set
			{
				Parameters["RssUrlTemplate"] = value;
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
		public string SearchTagsUrlTemplate
		{
			get
			{
				return Parameters.GetString("SearchTagsUrlTemplate");
			}
			set
			{
				Parameters["SearchTagsUrlTemplate"] = value;
			}
		}
		public bool EnableVotingForPost
		{
			get
			{
				return Parameters.GetBool("EnableVotingForPost", false);
			}
			set
			{
				Parameters["EnableVotingForPost"] = value.ToString();
			}
		}
		private IList<string> rolesAuthorizedToVote = null;
		public IList<string> RolesAuthorizedToVote
		{
			get 
			{
				return this.rolesAuthorizedToVote ?? (this.rolesAuthorizedToVote = Parameters.GetListString("RolesAuthorizedToVote")); 
			}
			set 
			{ 
				Parameters["RolesAuthorizedToVote"] = BXStringUtility.ListToCsv(this.rolesAuthorizedToVote = value ?? new List<string>()); 
			}
		}

		public BXBlogAuthorization Auth
		{
			get
			{
				return auth;
			}
		}

		internal IBXBlogPostProcessor Processor
		{
			get
			{
				if (processor == null)
				{
					if (post.ContentType == BXBlogPostContentType.FullHtml)
						processor = FullHtmlProcessor;
					else if (post.ContentType == BXBlogPostContentType.FilteredHtml)
						processor = HtmlProcessor;
					else
						processor = BBCodeProcessor;
				}
				return processor;
			}
		}

		internal BXBlogPostChain BBCodeProcessor
		{
			get
			{
				if (bbcodeProcessor == null)
				{
					bbcodeProcessor = new BXBlogPostChain(blog);
					bbcodeProcessor.MaxWordLength = MaxWordLength;
					bbcodeProcessor.HideCut = false;
				}
				return bbcodeProcessor;
			}
		}

		internal BXBlogPostHtmlProcessor HtmlProcessor
		{
			get
			{
				if (htmlProcessor == null)
				{
					htmlProcessor = new BXBlogPostHtmlProcessor();
					htmlProcessor.HideCut = false;
				}
				return htmlProcessor;
			}
		}

		internal BXBlogPostFullHtmlProcessor FullHtmlProcessor
		{
			get
			{
				if (fullHtmlProcessor == null)
				{
					fullHtmlProcessor = new BXBlogPostFullHtmlProcessor();
					fullHtmlProcessor.HideCut = false;
				}
				return fullHtmlProcessor;
			}
		}


		public event EventHandler<BXBlogCutTagEventArgs> RenderBeginCut
		{
			add
			{
				Processor.RenderBeginCut += value;
			}
			remove
			{
				Processor.RenderBeginCut -= value;
			}
		}
		public event EventHandler<BXBlogCutTagEventArgs> RenderEndCut
		{
			add
			{
				Processor.RenderEndCut += value;
			}
			remove
			{
				Processor.RenderEndCut -= value;
			}
		}

		public void Delete()
		{
			if (fatalError != ErrorCode.None)
				return;

			BXPrincipal user = BXPrincipal.Current;
			int userId = BXIdentity.Current.Id;

			if (auth.CanDeleteThisPost)
			{
				post.Delete();
				RedirectTemplateUrl(BlogUrlTemplate, replace);
				return;
			}

			Fatal(ErrorCode.UnauthorizedDeletePost);
		}
		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.FatalPostNotFound:
					return GetMessage("Error.PostNotFound");
				case ErrorCode.UnauthorizedViewPost:
					return GetMessage("Error.UnauthorizedView");
				case ErrorCode.UnauthorizedDeletePost:
					return GetMessage("Error.UnauthorizedDelete");
				default:
					return GetMessage("Error.Unknown");
			}
		}

		private BXParamsBag<object> GetReplaceParameters()
		{
			BXParamsBag<object> replace = new BXParamsBag<object>();
			replace["BlogId"] = blog.Id;
			replace["BlogSlug"] = blog.Slug;
			replace["PostId"] = post.Id;
			DateTime date = post.DatePublished;
			replace["PostYear"] = date.Year.ToString("0000");
			replace["PostMonth"] = date.Month.ToString("00");
			replace["PostDay"] = date.Day.ToString("00");
			replace["UserId"] = post.AuthorId;
			return replace;
		}
		private List<TagInfo> ParseTags(string tags, BXParamsBag<object> parameters)
		{
			List<TagInfo> result = new List<TagInfo>();
			if (!BXStringUtility.IsNullOrTrimEmpty(tags))
			{
				string[] splits = tags.Split(TagInfo.TagSeparators, StringSplitOptions.RemoveEmptyEntries);
				foreach (string s in splits)
					if (!BXStringUtility.IsNullOrTrimEmpty(s))
					{
						TagInfo info = new TagInfo();
						info.TagHtml = Encode(s);
						parameters["SearchTags"] = s.Trim().ToLowerInvariant();
						string template = SearchTagsUrlTemplate;
						info.TagHref = !BXStringUtility.IsNullOrTrimEmpty(template) ? Encode(ResolveTemplateUrl(template, parameters)) : "";
						result.Add(info);
					}
			}
			return result;
		}
		private void Fatal(ErrorCode code)
		{
			if (code == ErrorCode.FatalException)
				throw new InvalidOperationException("Use method with Exception argument");
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

			fatalError = ErrorCode.FatalException;
			fatalException = ex;
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}
		

		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;
			try
			{
				if (post == null)
				{
					if (PostId < 0)
					{
						Fatal(ErrorCode.FatalPostNotFound);
						return;
					}
					var filter = new BXFilter(new BXFilterItem(BXBlogPost.Fields.Id, BXSqlFilterOperators.Equal, PostId),
							new BXFilterItem(BXBlogPost.Fields.Blog.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

					if(CategoryId > 0)
						filter.Add(new BXFilterItem(BXBlogPost.Fields.Blog.Categories.Category.Id, BXSqlFilterOperators.Equal, CategoryId));
					BXBlogPostCollection posts = BXBlogPost.GetList(
						filter,
						null,
						new BXSelectAdd(BXBlogPost.Fields.Blog, BXBlogPost.Fields.Author, BXBlogPost.Fields.Author.User, BXBlogPost.Fields.Author.User.Image),
						null
					);

					if (posts.Count > 0)
						post = posts[0];
				}

				if (post == null)
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return;
				}

				if (blog == null)
					blog = post.Blog;

				if (blog == null || !string.IsNullOrEmpty(BlogSlug) && !string.Equals(blog.Slug, BlogSlug, StringComparison.InvariantCultureIgnoreCase))
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return;
				}

				auth = new BXBlogAuthorization(blog, post);

				if (!auth.CanReadThisPost)
				{
					Fatal(ErrorCode.UnauthorizedViewPost);
					return;
				}

				string title = post.TextEncoder.Decode(post.Title);
				replace = GetReplaceParameters();
				postInfo = new PostInfo();
				postInfo.Post = post;
				postInfo.AuthorNameHtml = BXWordBreakingProcessor.Break(post.TextEncoder.Decode(post.AuthorName), MaxWordLength, true);
				postInfo.TitleHtml = BXWordBreakingProcessor.Break(title, MaxWordLength, true);
				postInfo.processor = Processor;
				postInfo.PostViewHref = Encode(ResolveTemplateUrl(PostViewUrlTemplate, replace));
				postInfo.PostEditHref = Encode(ResolveTemplateUrl(PostEditUrlTemplate, replace));
				postInfo.RssHref = Encode(ResolveTemplateUrl(RssUrlTemplate, replace));
				postInfo.UserProfileHref = Encode(ResolveTemplateUrl(UserProfileUrlTemplate, replace));
				postInfo.BlogHref = Encode(ResolveTemplateUrl(BlogUrlTemplate, replace));
				postInfo.Tags = ParseTags(post.TextEncoder.Decode(post.Tags), replace);
				replace.Remove("SearchTags");

				ComponentCache["UsersBannedToVote"] = post.AuthorId.ToString();

				//Set page title
				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null && !IsComponentDesignMode)
				{
					bool setPageTitle = Parameters.GetBool("SetPageTitle", true);
					bool setMasterTitle = Parameters.GetBool("SetMasterTitle", setPageTitle);

					if (setPageTitle && !string.IsNullOrEmpty(title))
						bitrixPage.Title = Encode(title);
					if (setMasterTitle)
						bitrixPage.MasterTitleHtml = postInfo.TitleHtml;
				}
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}

		//protected override void OnLoad(EventArgs e)
		//{
		//    base.OnLoad(e);
		//}

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
			int userId = BXIdentity.Current.Id;
			//if (fatalError == ErrorCode.None && (userId == 0 || userId != post.AuthorId))
			//{
			//    post.ViewCount++;
			//    post.Save();
			//}
			if (fatalError == ErrorCode.None && (userId == 0 || userId != post.AuthorId))
				BXBlogPost.IncrementViewCount(post.Id);
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = BXBlogModuleConfiguration.GetComponentGroup();

			BXCategory mainCategory = BXCategory.Main,
				urlCategory = BXCategory.UrlSettings,
				additionalSettingsCategory = BXCategory.AdditionalSettings,
				votingCategory = new BXCategory(GetMessage("Category.Voting"), "Voting", 220);

			ParamsDefinition.Add("ThemeCssFilePath", new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/style.css", mainCategory));
			ParamsDefinition.Add("ColorCssFilePath", new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", mainCategory));

			ParamsDefinition.Add("BlogSlug", new BXParamText(GetMessageRaw("Param.BlogSlug"), "", mainCategory));
			ParamsDefinition.Add("PostId", new BXParamText(GetMessageRaw("Param.PostId"), "", mainCategory));
			ParamsDefinition["CategoryId"] = new BXParamSingleSelection(GetMessageRaw("Param.CategoryId"), "", mainCategory);
			ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));
			ParamsDefinition.Add("SetPageTitle", new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory));
			ParamsDefinition.Add("SetMasterTitle", new BXParamYesNo(GetMessageRaw("Param.SetMasterTitle"), Parameters.GetBool("SetPageTitle", true), additionalSettingsCategory));

			ParamsDefinition.Add("EnableVotingForPost", new BXParamYesNo(GetMessageRaw("Param.EnableVotingForPost"), false, votingCategory));
			ParamsDefinition.Add("RolesAuthorizedToVote", new BXParamMultiSelection(GetMessageRaw("Param.RolesAuthorizedToVote"), string.Empty, votingCategory));

			ParamsDefinition.Add("BlogUrlTemplate", new BXParamText(GetMessageRaw("Param.BlogUrlTemplate"), "Blog.aspx?blog=#BlogSlug#", urlCategory));
			ParamsDefinition.Add("UserProfileUrlTemplate", new BXParamText(GetMessageRaw("Param.UserProfileUrlTemplate"), "User.aspx?user=#UserId#", urlCategory));
			ParamsDefinition.Add("PostViewUrlTemplate", new BXParamText(GetMessageRaw("Param.PostViewUrlTemplate"), "Post.aspx?post=#PostId#", urlCategory));
			ParamsDefinition.Add("PostEditUrlTemplate", new BXParamText(GetMessageRaw("Param.PostEditUrlTemplate"), "PostEdit.aspx?post=#PostId#", urlCategory));
			ParamsDefinition.Add("RssUrlTemplate", new BXParamText(GetMessageRaw("Param.RssUrlTemplate"), "PostRss.aspx?post=#PostId#", urlCategory));
			ParamsDefinition.Add("SearchTagsTemplate", new BXParamText(GetMessageRaw("Param.SearchTagsTemplate"), "Tags.aspx?tags=#SearchTags#", urlCategory));
			ParamsDefinition.Add(BXParametersDefinition.Search);
		}

		protected override void LoadComponentDefinition()
		{
			IList<BXParamValue> rolesValues = ParamsDefinition["RolesAuthorizedToVote"].Values;
			rolesValues.Clear();
			foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
			{
				if (string.Equals(r.RoleName, "Guest", StringComparison.Ordinal))
					continue;
				rolesValues.Add(new BXParamValue(r.Title, r.RoleName));
			}

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

		public override void ProcessMessage(BXCommand cmd, bool executionContext, string executionVirtualPath)
		{
			if (cmd.Action == "Bitrix.Search.ProvideUrl")
			{
				if (cmd.Parameters.GetString("moduleId") != "blog")
					return;

				if (string.IsNullOrEmpty(BlogSlug) || cmd.Parameters.GetString("param1") != BlogSlug)
					return;

				string postIdString = cmd.Parameters.GetString("itemId");
				int postId;
				if (string.IsNullOrEmpty(postIdString) || !postIdString.StartsWith("p") || !int.TryParse(postIdString.Substring(1), out postId))
					return;

				BXParamsBag<object> replaceItems = new BXParamsBag<object>();
				replaceItems.Add("BlogId", cmd.Parameters.GetString("itemGroup"));
				replaceItems.Add("BlogSlug", cmd.Parameters.GetString("param1"));
				replaceItems.Add("PostId", postId);

				Uri cur = BXSefUrlManager.CurrentUrl;
				Uri parent = new Uri(cur, BXSite.GetUrlForPath(executionVirtualPath, null, cmd.SiteId));
				string relUrl = ResolveTemplateUrl(PostViewUrlTemplate, replaceItems);
				Uri rel;
				if (Uri.TryCreate(parent, relUrl, out rel))
					relUrl = rel.ToString();

				cmd.AddCommandResult("bitrix:blog@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, relUrl));
			}
		}

		public enum ErrorCode
		{
			None = 0,
			Fatal = 1,
			Unauthorized = 2,
			FatalComponentNotExecuted = Fatal | (0 << 2),
			FatalException = Fatal | (1 << 2),
			FatalPostNotFound = Fatal | (2 << 2),
			UnauthorizedViewPost = Unauthorized | (0 << 2),
			UnauthorizedDeletePost = Unauthorized | (1 << 2)
		}
		public class PostInfo
		{
			internal IBXBlogPostProcessor processor;
			private string authorNameHtml = string.Empty;
			private string titleHtml = string.Empty;
			private string postEditHref = string.Empty;
			private string userProfileHref = string.Empty;
			private string blogHref = string.Empty;
			private string postViewHref = string.Empty;
			private string rssHref = string.Empty;
			private BXBlogPost post;
			private List<TagInfo> tags;

			public BXBlogPost Post
			{
				get
				{
					return post;
				}
				internal set
				{
					post = value;
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
					authorNameHtml = value ?? string.Empty;
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
					titleHtml = value ?? string.Empty;
				}
			}
			public string PostEditHref
			{
				get
				{
					return postEditHref;
				}
				internal set
				{
					postEditHref = value ?? string.Empty;
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
					userProfileHref = value ?? string.Empty;
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
					blogHref = value ?? string.Empty;
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
					postViewHref = value ?? string.Empty;
				}
			}
			public string RssHref
			{
				get
				{
					return rssHref;
				}
				internal set
				{
					rssHref = value ?? string.Empty;
				}
			}
			public List<TagInfo> Tags
			{
				get
				{
					return tags;
				}
				internal set
				{
					tags = value;
				}
			}

			internal PostInfo()
			{
			}

			public void RenderPostContent(HtmlTextWriter writer)
			{
				processor.Process(post.Content, writer);
			}
			public string GetPostContent()
			{
				return processor.Process(post.Content);
			}
		}
		public class TagInfo
		{
			internal static readonly char[] TagSeparators = new char[] { ',' };
			private string tagHtml;
			private string tagHref;

			public string TagHtml
			{
				get
				{
					return tagHtml;
				}
				internal set
				{
					tagHtml = value;
				}
			}
			public string TagHref
			{
				get
				{
					return tagHref;
				}
				internal set
				{
					tagHref = value;
				}
			}
		}
	}

	public class BlogPostTemplate : BXComponentTemplate<BlogPostComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & BlogPostComponent.ErrorCode.Fatal) != 0)
				BXError404Manager.Set404Status(Response);
		}
		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			base.Render(writer);
		}
	}
}

