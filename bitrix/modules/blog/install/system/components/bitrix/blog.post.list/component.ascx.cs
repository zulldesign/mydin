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
using Bitrix.CommunicationUtility.Rating;
using System.Collections.Specialized;
using System.Web;

namespace Bitrix.Blog.Components
{
	public partial class BlogPostListComponent : BXComponent
	{
		private BXBlogAuthorization auth;
		private BXBlog blog;
		private string blogSlug;
		private List<PostInfo> posts = new List<PostInfo>();
		//private bool templateIncluded;
		private PublishMode publishMode;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private Exception fatalException;
		private BXParamsBag<object> replace;
		private int maxWordLength = -1;
		private BXBlogPostChain bbcodeProcessor;
		private BXBlogPostHtmlProcessor htmlProcessor;
		private BXBlogPostFullHtmlProcessor fullHtmlProcessor;
		private bool isFilterSet = false;

		public BXBlogAuthorization Auth
		{
			get { return auth; }
		}
		public BXBlog Blog
		{
			get { return blog; }
		}
		public string BlogSlug
		{
			get
			{
				return (blogSlug ?? (blogSlug = Parameters.GetString("BlogSlug")));
			}
		}

		string blogRssHref;
		public string BlogRssHref
		{
			get
			{
				if (blogRssHref != null)
					return blogRssHref;

				var param = Parameters.GetString("RssBlogPostsTemplate", "");
				var replace = new BXParamsBag<object>();
				replace["BlogSlug"] = BlogSlug;
				replace["CategoryId"] = CategoryId;
				return blogRssHref = ResolveTemplateUrl(param, replace);
			}
		}

		public int CategoryId
		{
			get { return Parameters.GetInt("CategoryId", 0); }
		}

		public List<PostInfo> Posts
		{
			get { return posts; }
		}
		public PublishMode PostPublishMode
		{
			get
			{
				if (publishMode == 0)
				{
					publishMode = PublishMode.Published;
					string paramMode = Parameters.GetString("PublishMode", String.Empty);
					if (!String.IsNullOrEmpty(paramMode))
					{
						try
						{
							publishMode = (PublishMode)Enum.Parse(typeof(PublishMode), paramMode);
						}
						catch { }
					}
				}

				return publishMode;
			}
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
		public string PostRssUrlTemplate
		{
			get
			{
				return Parameters.GetString("PostRssUrlTemplate");
			}
			set
			{
				Parameters["PostRssUrlTemplate"] = value;
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
		public bool SortByVotingTotals
		{
			get
			{
				return Parameters.GetBool("SortByVotingTotals", false);
			}
			set
			{
				Parameters["SortByVotingTotals"] = value.ToString();
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
		public bool IsOwner
		{
			get { return blog != null && blog.OwnerId == BXIdentity.Current.Id; }
		}
		public bool IsFilterSet
		{
			get { return isFilterSet; }
		}
		public int FilterYear
		{
			get
			{
				return Parameters.GetInt("FilterByYear", 0);
			}
		}
		public int FilterMonth
		{
			get
			{
				return Parameters.GetInt("FilterByMonth", 0);
			}
		}
		public int FilterDay
		{
			get
			{
				return Parameters.GetInt("FilterByDay", 0);
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
					bbcodeProcessor.HideCut = true;
					bbcodeProcessor.RenderHideCut += delegate(object sender, BXBlogCutTagEventArgs args) { if (RenderHideCut != null) RenderHideCut(sender, args); };
				}
				return bbcodeProcessor;
			}
		}

		internal IBXBlogPostProcessor GetProcessor(BXBlogPost post)
		{			
			if (post.ContentType == BXBlogPostContentType.FullHtml)
				return FullHtmlProcessor;
			if (post.ContentType == BXBlogPostContentType.FilteredHtml)
				return HtmlProcessor;		
			return BBCodeProcessor;
		}

		internal BXBlogPostHtmlProcessor HtmlProcessor
		{
			get
			{
				if (htmlProcessor == null)
				{
					htmlProcessor = new BXBlogPostHtmlProcessor();
					htmlProcessor.HideCut = true;
					htmlProcessor.RenderHideCut += delegate(object sender, BXBlogCutTagEventArgs args) { if (RenderHideCut != null) RenderHideCut(sender, args); };
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
					fullHtmlProcessor.HideCut = true;
					fullHtmlProcessor.RenderHideCut += delegate(object sender, BXBlogCutTagEventArgs args) { if (RenderHideCut != null) RenderHideCut(sender, args); };
				}
				return fullHtmlProcessor;
			}
		}

		public event EventHandler<BXBlogCutTagEventArgs> RenderHideCut;

		private BXPagingParams? _pagingParams = null;
		protected BXPagingParams InternalPagingParams
		{
			get { return (_pagingParams ?? (_pagingParams = PreparePagingParams())).Value; }
		}

		protected void IncludeComponentTemplateIfNeed()
		{
			if (IsRenderedFromCache || ComponentTemplate != null)
				return;
			IncludeComponentTemplate();
		}

		public string GetOperationHref(string operation, int postId)
		{
			return GetOperationHref(operation, postId, true);
		}

		public string GetOperationHref(string operation, int postId, bool addCsrfToken)
		{
			NameValueCollection query = HttpUtility.ParseQueryString(Bitrix.Services.BXSefUrlManager.CurrentUrl.Query);
			query.Set(PostIdParameter, postId.ToString());
			query.Set(OperationParameter, operation);
			if (addCsrfToken)
				BXCsrfToken.SetToken(query);
			else
				query.Remove(BXCsrfToken.TokenKey);

			UriBuilder uri = new UriBuilder(Bitrix.Services.BXSefUrlManager.CurrentUrl);
			uri.Query = query.ToString();
			return Encode(uri.Uri.ToString());
		}

		private const string PostIdParameter = "_id";
		private const string OperationParameter = "_action";

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			int postId;
			if (string.Equals(Request.QueryString[OperationParameter], "delete", StringComparison.Ordinal)
				&& BXCsrfToken.CheckTokenFromRequest(Request.QueryString)
				&& int.TryParse(Request.QueryString[PostIdParameter], out postId)
				&& postId > 0)
				DeletePost(new int[] { postId });
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			fatalError = ErrorCode.None;

			if (IsCached(BXPrincipal.Current.GetAllRoles(true), InternalPagingParams))
			{
				SetTemplateCachedData();
				return;
			}

			using (BXTagCachingScope scope = BeginTagCaching())
			{
				if (EnableVotingForPost && SortByVotingTotals)
					scope.AddTag(BXRatingVoting.CreateSpecialTag(BXCacheSpecialTagType.All));
				try
				{
					if (BXStringUtility.IsNullOrTrimEmpty(BlogSlug) || !BXBlog.SlugRegex.IsMatch(BlogSlug))
					{
						Fatal(ErrorCode.FatalBlogNotFound);
						return;
					}
					
					BXFilter filter = new BXFilter(
						new BXFilterItem(BXBlog.Fields.Slug, BXSqlFilterOperators.Equal, BlogSlug),
						new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
					);

					if (CategoryId > 0)
						filter.Add(new BXFilterItem(BXBlog.Fields.Categories.CategoryId, BXSqlFilterOperators.Equal, CategoryId));

					BXBlogCollection blogCollection = BXBlog.GetList(
						filter,
						null,
						new BXSelectAdd(
							BXBlog.Fields.Owner.User,
							BXBlog.Fields.Owner.User.Image.Folder,
							BXBlog.Fields.Owner.User.Image.FileName,
							//BXBlog.Fields.Owner.User.Image.ContentType,
							BXBlog.Fields.Owner.User.Image.Width,
							BXBlog.Fields.Owner.User.Image.Height
						),
						null,
						null
					);
					

					if (blogCollection.Count == 0)
					{
						Fatal(ErrorCode.FatalBlogNotFound);
						return;
					}

					blog = blogCollection[0];
					blog.PrepareForTagCaching(scope);

					auth = new BXBlogAuthorization(blog);
					if (!auth.CanReadThisBlog)
					{
						Fatal(ErrorCode.UnauthorizedViewBlog);
						return;
					}
					bool ownDrafts = false;

					if (PostPublishMode == PublishMode.Draft && !auth.CanReadThisBlogDrafts)
					{
						if (auth.CanCreatePost && auth.UserId != 0)
							ownDrafts = true;
						else
						{
							Fatal(ErrorCode.UnauthorizedViewBlog);
							return;
						}
					}

					ComponentCache["BlogName"] = blog.TextEncoder.Decode(blog.Name);

					if (PostPublishMode == PublishMode.All && !auth.CanReadThisBlogDrafts)
					{
						if (auth.CanCreatePost && auth.UserId != 0)
							ownDrafts = true;
						else
							publishMode = PublishMode.Published;
					}

					replace = new BXParamsBag<object>();
					replace["BlogId"] = blog.Id;
					replace["BlogSlug"] = blog.Slug;

					BXFilter postFilter = new BXFilter(new BXFilterItem(BXBlogPost.Fields.Blog.Id, BXSqlFilterOperators.Equal, blog.Id));

					DateTime from = new DateTime();
					DateTime to = new DateTime();
					if (Parameters.GetBool("FilterByDate", false))
					{
						try
						{
							if (FilterYear > 0 && FilterMonth > 0 && FilterDay > 0)
							{
								from = new DateTime(FilterYear, FilterMonth, FilterDay);
								to = from.AddDays(1);
								isFilterSet = true;
							}
							else if (FilterYear > 0 && FilterMonth > 0)
							{
								from = new DateTime(FilterYear, FilterMonth, 1);
								to = from.AddMonths(1);
								isFilterSet = true;
							}
							else if (FilterYear > 0)
							{
								from = new DateTime(FilterYear, 1, 1);
								to = from.AddYears(1);
								isFilterSet = true;
							}

							if (to > DateTime.Now && PostPublishMode == PublishMode.Published)
								to = DateTime.Now;
						}
						catch
						{
						}
					}

					if (isFilterSet)
					{
						postFilter.Add(new BXFilterItem(BXBlogPost.Fields.DatePublished, BXSqlFilterOperators.GreaterOrEqual, from));
						postFilter.Add(new BXFilterItem(BXBlogPost.Fields.DatePublished, BXSqlFilterOperators.Less, to));
					}

					if (PostPublishMode == PublishMode.Draft)
					{						
						postFilter.Add(new BXFilterOr(
							new BXFilterItem(BXBlogPost.Fields.IsPublished, BXSqlFilterOperators.Equal, false),
							new BXFilterItem(BXBlogPost.Fields.DatePublished, BXSqlFilterOperators.Greater, DateTime.Now)
						));
						if (ownDrafts)
							postFilter.Add(new BXFilterItem(BXBlogPost.Fields.Author.Id, BXSqlFilterOperators.Equal, auth.UserId));
					}
					else if (PostPublishMode == PublishMode.Published)
					{
						postFilter.Add(new BXFilterItem(BXBlogPost.Fields.IsPublished, BXSqlFilterOperators.Equal, true));
						if (!isFilterSet)
							postFilter.Add(new BXFilterItem(BXBlogPost.Fields.DatePublished, BXSqlFilterOperators.LessOrEqual, DateTime.Now));
					}
					else if (PostPublishMode == PublishMode.All && ownDrafts)
					{
						postFilter.Add(new BXFilterOr(
							!isFilterSet	
							? (IBXFilterItem)new BXFilterItem(BXBlogPost.Fields.IsPublished, BXSqlFilterOperators.Equal, true)
							: new BXFilter(
								new BXFilterItem(BXBlogPost.Fields.IsPublished, BXSqlFilterOperators.Equal, true),
								new BXFilterItem(BXBlogPost.Fields.DatePublished, BXSqlFilterOperators.LessOrEqual, DateTime.Now)
							),
							new BXFilterItem(BXBlogPost.Fields.Author.Id, BXSqlFilterOperators.Equal, auth.UserId)
						));						
					}

					bool legal;
					BXPagingParams pagingParams = PreparePagingParams();
					BXQueryParams queryParams = PreparePaging(
						pagingParams,
						delegate()
						{
							return BXBlogPost.Count(postFilter);
						},
						replace,
						out legal
					);

					if (!legal)
					{
						Fatal(ErrorCode.FatalInvalidPage);
						return;
					}

					BXOrderBy postOrderBy = new BXOrderBy();
					if (EnableVotingForPost && SortByVotingTotals)
						BXRatingDataLayerHelper.AddSortingByVotingTotalValue(BXBlogPost.Fields.CustomFields.DefaultFields, postOrderBy, BXOrderByDirection.Desc);

					postOrderBy.Add(BXBlogPost.Fields.DatePublished, BXOrderByDirection.Desc);
					postOrderBy.Add(BXBlogPost.Fields.Id, BXOrderByDirection.Desc);

					BXSelect postSelect = new BXSelectAdd(
							BXBlogPost.Fields.Author,
							BXBlogPost.Fields.Author.User,
							BXBlogPost.Fields.Author.User.Image.Folder,
							BXBlogPost.Fields.Author.User.Image.FileName,
						//BXBlogPost.Fields.Author.User.Image.ContentType,
							BXBlogPost.Fields.Author.User.Image.Width,
							BXBlogPost.Fields.Author.User.Image.Height
							);

					if (EnableVotingForPost)
					{
						BXRatingDataLayerHelper.AddTotalValueToSelection(postSelect, BXBlogPost.Fields.CustomFields.DefaultFields);
						BXRatingDataLayerHelper.AddTotalVotesToSelection(postSelect, BXBlogPost.Fields.CustomFields.DefaultFields);
					}

					BXBlogPostCollection postCollection = BXBlogPost.GetList(
						postFilter,
						postOrderBy,
						postSelect,
						queryParams
					);

					BXParamsBag<object>[] cachedPostData = new BXParamsBag<object>[postCollection.Count];

					for (int i = 0; i < postCollection.Count; i++)
					{
						BXBlogPost post = postCollection[i];

						PostInfo postInfo = new PostInfo(this);

						replace["PostYear"] = post.DatePublished.Year.ToString("0000");
						replace["PostMonth"] = post.DatePublished.Month.ToString("00");
						replace["PostDay"] = post.DatePublished.Day.ToString("00");
						replace["PostId"] = post.Id;
						replace["UserId"] = post.AuthorId;

						postInfo.Post = post;

						BXBlogAuthorization postAuth = new BXBlogAuthorization(blog, post);
						postInfo.Auth = postAuth;
						postInfo.AuthorNameHtml = BXWordBreakingProcessor.Break(post.TextEncoder.Decode(post.AuthorName), MaxWordLength, true);
						postInfo.TitleHtml = BXWordBreakingProcessor.Break(post.TextEncoder.Decode(post.Title), MaxWordLength, true);
						postInfo.BlogHref = Encode(ResolveTemplateUrl(BlogUrlTemplate, replace));

						if (postAuth.CanEditThisPost)
							postInfo.PostEditHref = Encode(ResolveTemplateUrl(PostEditUrlTemplate, replace));

						postInfo.PostViewHref = Encode(ResolveTemplateUrl(PostViewUrlTemplate, replace));
						postInfo.PostRssHref = Encode(ResolveTemplateUrl(PostRssUrlTemplate, replace));
						postInfo.UserProfileHref = Encode(ResolveTemplateUrl(UserProfileUrlTemplate, replace)); ;
						postInfo.Tags = ParseTags(post.TextEncoder.Decode(post.Tags), replace);
						postInfo.VotingTotalValue = BXRatingDataLayerHelper.GetVotingTotalValue(post);
						postInfo.VotingTotalVotes = BXRatingDataLayerHelper.GetVotingTotalVotes(post);
						posts.Add(postInfo);

						BXParamsBag<object> cachedPost = new BXParamsBag<object>();
						cachedPost["Id"] = post.Id;
						cachedPost["AuthorId"] = post.AuthorId;

						cachedPostData[i] = cachedPost;
					}

					ComponentCache["PostData"] = cachedPostData;

					IncludeComponentTemplateIfNeed();
					SetTemplateCachedData();
				}
				catch (Exception ex)
				{
					Fatal(ex);
				}
			}
		}

		private void SetTemplateCachedData()
		{
			BXPublicPage bitrixPage = Page as BXPublicPage;
			if (bitrixPage != null && !IsComponentDesignMode)
			{
				bool setPageTitle = Parameters.GetBool("SetPageTitle", true);
				bool setMasterTitle = Parameters.GetBool("SetMasterTitle", setPageTitle);

				if (PostPublishMode == PublishMode.Draft)
				{
					if (setPageTitle)
						bitrixPage.Title = GetMessage("PageTitle.Drafts");
				}
				else
				{
					string title = ComponentCache.GetString("BlogName");
					if (setMasterTitle)
						bitrixPage.MasterTitleHtml = BXWordBreakingProcessor.Break(title, MaxWordLength, true);
					if (setPageTitle)
						bitrixPage.Title = Encode(title);
				}
			}
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

			BXCategory mainCategory = BXCategory.Main,
				urlCategory = BXCategory.UrlSettings,
				additionalSettingsCategory = BXCategory.AdditionalSettings,
				votingCategory = new BXCategory(GetMessage("Category.Voting"), "Voting", 220);

			ParamsDefinition["ThemeCssFilePath"] = new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/style.css", mainCategory);
			ParamsDefinition["ColorCssFilePath"] = new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", mainCategory);
			ParamsDefinition["BlogSlug"] = new BXParamText(GetMessageRaw("Param.BlogSlug"), "", mainCategory);
			ParamsDefinition["PublishMode"] = new BXParamSingleSelection(GetMessageRaw("Param.PublishMode"), PublishMode.Published.ToString(), mainCategory);
			ParamsDefinition["CategoryId"] = new BXParamSingleSelection(GetMessageRaw("Param.CategoryId"), "", mainCategory);
			string clientSideActionGroupViewId = ClientID;
			ParamsDefinition["FilterByDate"] = new BXParamYesNo(GetMessageRaw("Param.FilterByDate"), false, mainCategory, new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId, "FilterByDate", "Filter", "NonFilter"));
			ParamsDefinition["FilterByYear"] = new BXParamSingleSelection(GetMessageRaw("Param.FilterByDate.Year"), String.Empty, mainCategory, null, new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "FilterByYear", new string[] { "Filter" }));
			ParamsDefinition["FilterByMonth"] = new BXParamSingleSelection(GetMessageRaw("Param.FilterByDate.Month"), String.Empty, mainCategory, null, new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "FilterByMonth", new string[] { "Filter" }));
			ParamsDefinition["FilterByDay"] = new BXParamSingleSelection(GetMessageRaw("Param.FilterByDate.Day"), String.Empty, mainCategory, null, new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "FilterByDay", new string[] { "Filter" }));

			ParamsDefinition["BlogUrlTemplate"] = new BXParamText(GetMessageRaw("Param.BlogUrlTemplate"), "Blog.aspx?blog=#blogSlug#", urlCategory);
			ParamsDefinition["UserProfileUrlTemplate"] = new BXParamText(GetMessageRaw("Param.UserProfileUrlTemplate"), "User.aspx?user=#userId#", urlCategory);
			ParamsDefinition["PostViewUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostViewUrlTemplate"), "Post.aspx?post=#postId#", urlCategory);
			ParamsDefinition["PostRssUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostRssUrlTemplate"), "PostRss.aspx?post=#PostId#", urlCategory);
			ParamsDefinition["PostEditUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostEditUrlTemplate"), "PostEdit.aspx?post=#postId#", urlCategory);
			ParamsDefinition["SearchTagsUrlTemplate"] = new BXParamText(GetMessageRaw("Param.SearchTagsUrlTemplate"), "Tags.aspx?tags=#SearchTags#", urlCategory);

			ParamsDefinition["MaxWordLength"] = new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory);
			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);
			ParamsDefinition["SetMasterTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetMasterTitle"), Parameters.GetBool("SetPageTitle", true), additionalSettingsCategory);

			ParamsDefinition.Add(
				"EnableVotingForPost",
				new BXParamYesNo(
					GetMessageRaw("Param.EnableVotingForPost"),
					false,
					votingCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, "EnableVotingForPost", "EnableVoting", string.Empty)
					)
			   );
			ParamsDefinition.Add(
				"RolesAuthorizedToVote",
				new BXParamMultiSelection(
					GetMessageRaw("Param.RolesAuthorizedToVote"),
					string.Empty,
					votingCategory,
					null,
					new ParamClientSideActionGroupViewMember(ClientID, "RolesAuthorizedToVote", new string[] { "EnableVoting" })
					)
				);

			ParamsDefinition.Add(
				"SortByVotingTotals",
				new BXParamYesNo(
					GetMessageRaw("Param.SortByVotingTotals"),
					false,
					votingCategory,
					new ParamClientSideActionGroupViewMember(ClientID, "SortByVotingTotals", new string[] { "EnableVoting" })
					)
				);

			ParamsDefinition.Add(BXParametersDefinition.Cache);
			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

		}
		protected override void LoadComponentDefinition()
		{
			ParamsDefinition["PublishMode"].Values.Add(new BXParamValue(GetMessageRaw("Param.PublishMode.Published"), PublishMode.Published.ToString()));
			ParamsDefinition["PublishMode"].Values.Add(new BXParamValue(GetMessageRaw("Param.PublishMode.Draft"), PublishMode.Draft.ToString()));
			ParamsDefinition["PublishMode"].Values.Add(new BXParamValue(GetMessageRaw("Param.PublishMode.All"), PublishMode.All.ToString()));

			ParamsDefinition["FilterByYear"].Values.Add(new BXParamValue(GetMessageRaw("Option.SelectYear"), String.Empty));
			for (int i = 2008; i <= DateTime.Now.Year + 1; i++)
				ParamsDefinition["FilterByYear"].Values.Add(new BXParamValue(i.ToString(), i.ToString()));


			CultureInfo ci = CultureInfo.CurrentCulture;
			DateTimeFormatInfo dtf = ci.DateTimeFormat;
			ParamsDefinition["FilterByMonth"].Values.Add(new BXParamValue(GetMessageRaw("Option.NotSelected"), String.Empty));
			for (int i = 0; i < 12; i++)
				ParamsDefinition["FilterByMonth"].Values.Add(new BXParamValue(dtf.MonthNames[i], (i + 1).ToString()));

			ParamsDefinition["FilterByDay"].Values.Add(new BXParamValue(GetMessageRaw("Option.NotSelected"), String.Empty));
			for (int i = 1; i <= 31; i++)
				ParamsDefinition["FilterByDay"].Values.Add(new BXParamValue(i.ToString(), i.ToString()));

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

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
			AbortCache();
			IncludeComponentTemplateIfNeed();
		}
		private void Fatal(Exception ex)
		{
			if (ex == null)
				throw new ArgumentNullException("ex");

			fatalError = ErrorCode.FatalException;
			fatalException = ex;
			AbortCache();
			IncludeComponentTemplateIfNeed();
		}

		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.FatalBlogNotFound:
					return GetMessage("Error.FatalBlogNotFound");
				case ErrorCode.FatalInvalidPage:
					return GetMessage("Error.FatalInvalidPage");
				case ErrorCode.UnauthorizedViewBlog:
					return GetMessage("Error.UnauthorizedViewBlog");
				case ErrorCode.UnauthorizedDeletePost:
					return GetMessage("Error.UnauthorizedDeletePost");
				default:
					return GetMessage("Error.Unknown");
			}
		}
		public void DeletePost(IEnumerable<int> ids)
		{
			if (!(fatalError == ErrorCode.None || fatalError == ErrorCode.FatalComponentNotExecuted))
				return;

			try
			{
				BXPrincipal user = BXPrincipal.Current;
				int userId = BXIdentity.Current.Id;


				BXBlog b = !string.IsNullOrEmpty(BlogSlug) ? BXBlog.GetBySlug(BlogSlug) : null;
				if (b == null)
				{
					Fatal(ErrorCode.FatalBlogNotFound);
					return;
				}

				BXFilter filter = new BXFilter();
				filter.Add(new BXFilterItem(BXBlogPost.Fields.Id, BXSqlFilterOperators.In, ids));
				filter.Add(new BXFilterItem(BXBlogPost.Fields.Blog.Id, BXSqlFilterOperators.Equal, b.Id));

				foreach (BXBlogPost p in BXBlogPost.GetList(filter, null))
				{
					if (!new BXBlogAuthorization(b, p).CanDeleteThisPost)
					{
						Fatal(ErrorCode.UnauthorizedDeletePost);
						return;
					}
					p.Delete();

					NameValueCollection query = HttpUtility.ParseQueryString(Bitrix.Services.BXSefUrlManager.CurrentUrl.Query);
					query.Remove(PostIdParameter);
					query.Remove(OperationParameter);
					query.Remove(BXCsrfToken.TokenKey);

					Response.Redirect(query.Count > 0 ? string.Concat(BXSefUrlManager.CurrentUrl.AbsolutePath, "?", query.ToString()) : BXSefUrlManager.CurrentUrl.AbsolutePath, true);
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}
		}

		//NESTED CLASSES
		public class PostInfo
		{
			private BXBlogPost post;
			private string postViewHref = string.Empty;
			private string postRssHref = string.Empty;
			private string postEditHref = string.Empty;
			private string userProfileHref = string.Empty;
			private string blogHref = string.Empty;
			private string authorNameHtml = string.Empty;
			private string titleHtml = string.Empty;
			private BlogPostListComponent component;
			private BXBlogAuthorization auth;
			private List<TagInfo> tags;
			private double votingTotalValue = 0D;
			private int votingTotalVotes = 0;

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
			public string PostViewHref
			{
				get { return postViewHref; }
				internal set { postViewHref = value ?? string.Empty; }
			}
			public string PostRssHref
			{
				get { return postRssHref; }
				internal set { postRssHref = value ?? string.Empty; }
			}
			public string PostEditHref
			{
				get { return postEditHref; }
				internal set { postEditHref = value ?? string.Empty; }
			}
			public string UserProfileHref
			{
				get { return userProfileHref; }
				internal set { userProfileHref = value ?? string.Empty; }
			}
			public string BlogHref
			{
				get { return blogHref; }
				internal set { blogHref = value ?? string.Empty; }
			}
			public string AuthorNameHtml
			{
				get { return authorNameHtml; }
				internal set { authorNameHtml = value ?? string.Empty; }
			}
			public string TitleHtml
			{
				get { return titleHtml; }
				internal set { titleHtml = value ?? string.Empty; }
			}
			public BXBlogAuthorization Auth
			{
				get { return auth; }
				internal set { auth = value; }
			}
			public List<TagInfo> Tags
			{
				get { return tags; }
				internal set { tags = value; }
			}


			public double VotingTotalValue
			{
				get { return votingTotalValue; }
				internal set { votingTotalValue = value; }
			}

			public int VotingTotalVotes
			{
				get { return votingTotalVotes; }
				internal set { votingTotalVotes = value; }
			}

			public string GetContentHtml()
			{
				return component.GetProcessor(post).Process(post.Content);
			}

			internal PostInfo(BlogPostListComponent component)
			{
				this.component = component;
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
		public enum PublishMode
		{
			All = 1,
			Draft,
			Published
		}
		public enum ErrorCode
		{
			None = 0,
			Fatal = 1,
			Unauthorized = 2,

			FatalBlogNotFound = Fatal | 4,
			FatalInvalidPage = Fatal | 4 | 8,
			FatalException = Fatal | 8,
			FatalComponentNotExecuted = Fatal | 16,

			UnauthorizedViewBlog = Unauthorized | 4,
			UnauthorizedDeletePost = Unauthorized | 8
		}
	}

	public class BlogPostListTemplate : BXComponentTemplate<BlogPostListComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & BlogPostListComponent.ErrorCode.Fatal) != 0)
				BXError404Manager.Set404Status(Response);
		}
		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			base.Render(writer);
		}
	}
}
