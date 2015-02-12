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
using Bitrix.Configuration;
using Bitrix.CommunicationUtility.Rating;

namespace Bitrix.Blog.Components
{
	public partial class BlogPostsComponent : BXComponent
	{
		private List<PostInfo> posts = new List<PostInfo>();
		private bool templateIncluded;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private Exception fatalException;
		private int maxWordLength = -1;
		private BXBlogPostChain bbcodeProcessor;
		private BXBlogPostHtmlProcessor htmlProcessor;
		private BXBlogPostFullHtmlProcessor fullHtmlProcessor;
		private Sorting sortBy;
		private int periodDays = -1;
		private bool hideCut;

		public Sorting SortBy
		{
			get
			{
				if (sortBy == 0)
				{
					sortBy = Sorting.ByDate;
					string paramSort = Parameters.GetString("SortBy", String.Empty);
					if (!String.IsNullOrEmpty(paramSort))
					{
						try
						{
							sortBy = (Sorting)Enum.Parse(typeof(Sorting), paramSort);
						}
						catch { }
					}
				}

				return sortBy;
			}
		}
		public List<PostInfo> Posts
		{
			get { return posts; }
		}

		/// <summary>
		/// Запрос количества элеметов
		/// </summary>
		public int PostCount
		{
			get
			{
				object val;
				return ComponentCache.TryGetValue("PostCount", out val) ? (int)val : posts != null ? posts.Count : 0;
			}
		}

		/// <summary>
		/// Запрос наличия элементов
		/// </summary>
		public bool HasPosts
		{
			get { return PostCount > 0; }
		}


		/// <summary>
		/// Включить фильтрацию по пользовательским свойствам сообщений
		/// </summary>
		public bool FilterByPostCustomProperty
		{
			get 
			{
				return Parameters.GetBool("FilterByPostCustomProperty", false);
			}
			set 
			{
				Parameters["FilterByPostCustomProperty"] = value.ToString();
			}
		}

		public BXParamsBag<object> PostCustomPropertyFilterSettings
		{
			get 
			{
				return BXParamsBag<object>.FromString(Parameters.GetString("PostCustomPropertyFilterSettings", string.Empty));
			}
			set 
			{
				Parameters["PostCustomPropertyFilterSettings"] = value.ToString();
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
		public string CategoryBlogListUrlTemplate
		{
			get
			{
				return Parameters.GetString("CategoryBlogListUrlTemplate", "");
			}
			set
			{
				Parameters["CategoryBlogListPageUrlTemplate"] = value;
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
		public string Tags
		{
			get
			{
				return Parameters.GetString("Tags", string.Empty);
			}
			set
			{
				Parameters["Tags"] = value ?? string.Empty;
			}
		}

		public string BlogSlug
		{
			get
			{
				return Parameters.GetString("BlogSlug", string.Empty);
			}
			set
			{
				Parameters["BlogSlug"] = value ?? string.Empty;
			}
		}

		public int CategoryId
		{
			get { return Parameters.GetInt("CategoryId", 0); }
			set { Parameters["CategoryId"] = value.ToString(); }
		}
		string categoryViewUrl;

		public string CategoryViewUrl
		{
			get
			{
				if (categoryViewUrl != null)
					return categoryViewUrl;

				var replace = new BXParamsBag<object>();

				replace.Add("CategoryId", CategoryId);
				return categoryViewUrl = ResolveTemplateUrl(CategoryBlogListUrlTemplate, replace);
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
		public int PeriodDays
		{
			get
			{
				return (periodDays != -1) ? periodDays : (periodDays = Math.Max(0, Parameters.GetInt("PeriodDays", 30)));
			}
			set
			{
				periodDays = Math.Max(0, value);
				Parameters["PeriodDays"] = periodDays.ToString();
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

		public bool HideCut
			{
				get { return hideCut; }
				set 
				{
					hideCut = value;
					if (bbcodeProcessor != null)
						bbcodeProcessor.HideCut = value;
					if (htmlProcessor != null)
						htmlProcessor.HideCut = value;
					if (fullHtmlProcessor != null)
						fullHtmlProcessor.HideCut = value;
				}
			}
		internal BXBlogPostChain BBCodeProcessor
		{
			get
			{
				if (bbcodeProcessor == null)
				{
					bbcodeProcessor = new BXBlogPostChain();
					bbcodeProcessor.MaxWordLength = MaxWordLength;
					bbcodeProcessor.HideCut = HideCut;
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
					htmlProcessor.HideCut = HideCut;
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
					fullHtmlProcessor.HideCut = HideCut;
					fullHtmlProcessor.RenderHideCut += delegate(object sender, BXBlogCutTagEventArgs args) { if (RenderHideCut != null) RenderHideCut(sender, args); };
				}
				return fullHtmlProcessor;
			}
		}

		public event EventHandler<BXBlogCutTagEventArgs> RenderHideCut;

		private bool tagsMode = false;
		protected bool TagsMode
		{
			get { return this.tagsMode; }
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
				var bp = BXBlogUserPermissions.GetBlogsForPosts(BXIdentity.Current.Id, BXBlogPostPermissionLevel.Read, true, Page.Items, null);					
				if (IsCached(pagingParams, BXPrincipal.Current.IsCanOperate(BXBlog.Operations.PostEdit), bp.Hash))
				{
					SetTemplateCachedData();
					return;
				}

				using (BXTagCachingScope scope = BeginTagCaching())
				{
					if(CategoryId > 0)
					{
						BXBlogCategory category = BXBlogCategory.GetById(CategoryId);
						if(category == null)
							Fatal(new Exception(string.Format("Could not find category: '{0}'!", CategoryId)));

						category.PrepareForTagCaching(scope);
					}
					else
						scope.AddTag(BXBlogCategory.CreateSpecialTag(BXCacheSpecialTagType.All));

																
					bool hasBlogs = bp.Exclude || bp.Ids.Count > 0;
					
					// init tag filter
					this.tagsMode = Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search") && !BXStringUtility.IsNullOrTrimEmpty(Tags);
					IBlogPostsComponentTagFilter tagFilter = null;
					if (this.tagsMode)
					{
						try
						{
							tagFilter = (IBlogPostsComponentTagFilter)LoadControl("tagfilter.ascx");
						}						
						catch
						{
							tagsMode = false;
						}						
					}
					if (this.tagsMode)
						tagFilter.InitFilter(DesignerSite, Tags, BlogSlug, bp.Ids, bp.Exclude, CategoryId);
											
					BXFilter postFilter = null;
					if (!this.tagsMode && hasBlogs)
					{
						postFilter = new BXFilter(
							new BXFilterItem(BXBlogPost.Fields.IsPublished, BXSqlFilterOperators.Equal, true),
							new BXFilterItem(BXBlogPost.Fields.DatePublished, BXSqlFilterOperators.LessOrEqual, DateTime.Now),
							new BXFilterItem(BXBlogPost.Fields.Blog.Active, BXSqlFilterOperators.Equal, true),
							new BXFilterItem(BXBlogPost.Fields.Blog.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
						);

						if (bp.Ids.Count > 0)
						{
							IBXFilterItem blogFilter = new BXFilterItem(BXBlogPost.Fields.Blog.Id, BXSqlFilterOperators.In, bp.Ids);
							if (bp.Exclude)
								blogFilter = new BXFilterNot(blogFilter);
							postFilter.Add(blogFilter);
						}							

						if (CategoryId > 0)
							postFilter.Add(new BXFilterItem(BXBlogPost.Fields.Blog.Categories.CategoryId, BXSqlFilterOperators.Equal, CategoryId));

						if (PeriodDays > 0)
							postFilter.Add(new BXFilterItem(BXBlogPost.Fields.DatePublished, BXSqlFilterOperators.GreaterOrEqual, DateTime.Now.AddDays(-PeriodDays)));

						if (SortBy == Sorting.ByViews)
							postFilter.Add(new BXFilterItem(BXBlogPost.Fields.ViewCount, BXSqlFilterOperators.Greater, 0));
						else if (SortBy == Sorting.ByComments)
							postFilter.Add(new BXFilterItem(BXBlogPost.Fields.CommentCount, BXSqlFilterOperators.Greater, 0));

						BXParamsBag<object> customPropertyFilterSettings = FilterByPostCustomProperty ? PostCustomPropertyFilterSettings : null;
						if (customPropertyFilterSettings != null && customPropertyFilterSettings.Count > 0) 
						{
							BXCustomFieldCollection postCustomFields = BXBlogPost.CustomFields;
							foreach (KeyValuePair<string, object> kv in customPropertyFilterSettings)
							{ 
								BXCustomField postCustomField;
								if (!postCustomFields.TryGetValue(kv.Key, out postCustomField))
									continue;
								postFilter.Add(new BXFilterItem(BXBlogPost.Fields.GetCustomField(postCustomField.Name), postCustomField.Multiple ? BXSqlFilterOperators.In : BXSqlFilterOperators.Equal, kv.Value));
							}
						}
					}

					bool legal;
					BXParamsBag<object> pagingReplace = new BXParamsBag<object>();
					pagingReplace["SearchTags"] = this.tagsMode ? Tags : null;
					pagingReplace["BlogSlug"] = BlogSlug;
					BXQueryParams queryParams = PreparePaging(
						pagingParams,
						delegate()
						{
							if (!hasBlogs)
								return 0;
							return this.tagsMode ? tagFilter.Count() : BXBlogPost.Count(postFilter);
						},
						pagingReplace,
						out legal
					);

					if (!legal)
					{
						Fatal(ErrorCode.FatalInvalidPage);
						return;
					}

					IList<int> tagIds = null;
					if (this.tagsMode)
					{
						tagIds = tagFilter.GetPostIdList(
							(queryParams != null && queryParams.AllowPaging) 
							? new BXPagingOptions(queryParams.PagingStartIndex, queryParams.PagingRecordCount)
							: null
						);
						postFilter = new BXFilter(
							new BXFilterItem(BXBlogPost.Fields.Id, BXSqlFilterOperators.In, tagIds)
						);

						if (CategoryId > 0)
							postFilter.Add(new BXFilterItem(BXBlogPost.Fields.Blog.Categories.CategoryId, BXSqlFilterOperators.Equal, CategoryId));

						queryParams.AllowPaging = false;
					}


					BXOrderBy postOrderBy = null;
					if (!this.tagsMode)
					{
						postOrderBy = new BXOrderBy();
						if(SortBy == Sorting.ByVotingTotals)
							BXRatingDataLayerHelper.AddSortingByVotingTotalValue(BXBlogPost.Fields.CustomFields.DefaultFields, postOrderBy, BXOrderByDirection.Desc);

						else if (SortBy == Sorting.ByViews)
							postOrderBy.Add(BXBlogPost.Fields.ViewCount, BXOrderByDirection.Desc);
						else if (SortBy == Sorting.ByComments)
							postOrderBy.Add(BXBlogPost.Fields.CommentCount, BXOrderByDirection.Desc);

						postOrderBy.Add(BXBlogPost.Fields.DatePublished, BXOrderByDirection.Desc);
						postOrderBy.Add(BXBlogPost.Fields.Id, BXOrderByDirection.Desc);
					}

					BXSelect postSelect = new BXSelectAdd(
						BXBlogPost.Fields.Blog,
						BXBlogPost.Fields.Author,
						BXBlogPost.Fields.Author.User,
                        BXBlogPost.Fields.Author.User.Image.StorageId,
						BXBlogPost.Fields.Author.User.Image.Folder,
						BXBlogPost.Fields.Author.User.Image.FileName,
						//BXBlogPost.Fields.Author.User.Image.ContentType,
						BXBlogPost.Fields.Author.User.Image.Width,
						BXBlogPost.Fields.Author.User.Image.Height
					);

					if (SortBy == Sorting.ByVotingTotals)
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

					if (this.tagsMode)
					{
						postCollection.Sort(delegate (BXBlogPost a, BXBlogPost b)
						{
							return tagIds.IndexOf(a.Id) - tagIds.IndexOf(b.Id);
						});
					}

					BXParamsBag<object> replace = new BXParamsBag<object>();

					ComponentCache["PostCount"] = postCollection.Count;
					ComponentCache["DefaultBlogId"] = postCollection.Count > 0 ? postCollection[0].BlogId : 0;
					BXParamsBag<object>[] cachedPostData = new BXParamsBag<object>[postCollection.Count];
					for (int i = 0; i < postCollection.Count; i++)
					{
						BXBlogPost post  = postCollection[i];
						replace["PostYear"] = post.DatePublished.Year.ToString();
						replace["PostMonth"] = post.DatePublished.Month.ToString();
						replace["PostDay"] = post.DatePublished.Day.ToString();
						replace["PostId"] = post.Id;
						replace["UserId"] = post.AuthorId;
						replace["BlogId"] = post.Blog.Id;
						replace["BlogSlug"] = post.Blog.Slug;

						PostInfo postInfo = new PostInfo(this);
						postInfo.Post = post;
						postInfo.Auth = new BXBlogAuthorization(post.Blog, post);
						postInfo.AuthorNameHtml = BXWordBreakingProcessor.Break(post.TextEncoder.Decode(post.AuthorName), MaxWordLength, true);
						postInfo.TitleHtml = BXWordBreakingProcessor.Break(post.TextEncoder.Decode(post.Title), MaxWordLength, true);
						postInfo.BlogHref = Encode(ResolveTemplateUrl(BlogUrlTemplate, replace));
						postInfo.PostViewHref = Encode(ResolveTemplateUrl(PostViewUrlTemplate, replace));
						postInfo.PostRssHref = Encode(ResolveTemplateUrl(PostRssUrlTemplate, replace));
						postInfo.UserProfileHref = Encode(ResolveTemplateUrl(UserProfileUrlTemplate, replace));
						postInfo.VotingTotalValue = BXRatingDataLayerHelper.GetVotingTotalValue(post);
						postInfo.VotingTotalVotes = BXRatingDataLayerHelper.GetVotingTotalVotes(post);
						
						if (postInfo.Auth.CanEditPost)
							postInfo.PostEditHref = Encode(ResolveTemplateUrl(PostEditUrlTemplate, replace));

						postInfo.Tags = ParseTags(post.TextEncoder.Decode(post.Tags), replace);
						posts.Add(postInfo);

						BXParamsBag<object> cachedPost = new BXParamsBag<object>();
						cachedPost["Id"] = post.Id;
						cachedPost["AuthorId"] = post.AuthorId;

						cachedPostData[i] = cachedPost;
					}

					ComponentCache["PostData"] = cachedPostData;

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

		private void SetTemplateCachedData()
		{
			BXPublicPage bitrixPage = Page as BXPublicPage;
			bool setPageTitle = Parameters.GetBool("SetPageTitle", true);
			bool setMasterTitle = Parameters.GetBool("SetMasterTitle", setPageTitle);
			if (bitrixPage != null && (setPageTitle || setMasterTitle))
			{
				string title;
				if (this.tagsMode)
					title = Tags;
				else if (SortBy == Sorting.ByViews)
					title = GetMessageRaw("PageTitle.MostViewed");
				else if (SortBy == Sorting.ByComments)
					title = GetMessageRaw("PageTitle.MostCommented");
				else
					title = GetMessageRaw("PageTitle.NewPosts");

				if (setMasterTitle)
					bitrixPage.MasterTitleHtml = Encode(title);

				if (setPageTitle)
					bitrixPage.BXTitle = title;
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
			ParamsDefinition["SortBy"] = new BXParamSingleSelection(GetMessageRaw("Param.SortBy"), Sorting.ByDate.ToString(), mainCategory);
			ParamsDefinition["PeriodDays"] = new BXParamText(GetMessageRaw("Param.PeriodDays"), "30", mainCategory);
			
			if (Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search"))
				ParamsDefinition["Tags"] = new BXParamText(GetMessageRaw("Param.Tags"), "", mainCategory);

			ParamsDefinition["BlogSlug"] = new BXParamText(GetMessageRaw("Param.BlogSlug"), "", mainCategory);
			ParamsDefinition["CategoryId"] = new BXParamSingleSelection(GetMessageRaw("Param.CategoryId"), "", mainCategory);
			ParamsDefinition["BlogUrlTemplate"] = new BXParamText(GetMessageRaw("Param.BlogUrlTemplate"), "Blog.aspx?blog=#blogSlug#", urlCategory);
			ParamsDefinition["UserProfileUrlTemplate"] = new BXParamText(GetMessageRaw("Param.UserProfileUrlTemplate"), "User.aspx?user=#userId#", urlCategory);
			ParamsDefinition["PostViewUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostViewUrlTemplate"), "Post.aspx?post=#postId#", urlCategory);
			ParamsDefinition["PostEditUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostEditUrlTemplate"), "PostEdit.aspx?post=#postId#", urlCategory);
			ParamsDefinition["PostRssUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostRssUrlTemplate"), "PostRss.aspx?post=#PostId#", urlCategory);
			ParamsDefinition["SearchTagsUrlTemplate"] = new BXParamText(GetMessageRaw("Param.SearchTagsUrlTemplate"), "Tags.aspx?tags=#SearchTags#&searchBlog=#BlogSlug#", urlCategory);

			ParamsDefinition["MaxWordLength"] = new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory);
			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);
			ParamsDefinition["SetMasterTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetMasterTitle"), Parameters.GetBool("SetPageTitle", true), additionalSettingsCategory);

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			BXCategory customFieldCategory = BXCategory.CustomField;

			ParamsDefinition.Add("FilterByPostCustomProperty",
				new BXParamYesNo(
					GetMessageRaw("Param.FilterByPostCustomProperty"), 
					false,
					customFieldCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, "FilterByPostCustomProperty", "FilterByPostCustomProperty", string.Empty)
					)
				);

			ParamsDefinition.Add("PostCustomPropertyFilterSettings",
				new BXParamCustomFieldFilter(
					GetMessageRaw("Param.PostCustomPropertyFilterSettings"),
					string.Empty,
					customFieldCategory,
					BXBlogModuleConfiguration.PostCustomFieldEntityId,
					new ParamClientSideActionGroupViewMember(ClientID, "PostCustomPropertyFilterSettings", new string[] { "FilterByPostCustomProperty" })
					)
				);

			ParamsDefinition.Add("EnableVotingForPost", new BXParamYesNo(GetMessageRaw("Param.EnableVotingForPost"), false, votingCategory));
			ParamsDefinition.Add("RolesAuthorizedToVote", new BXParamMultiSelection(GetMessageRaw("Param.RolesAuthorizedToVote"), string.Empty, votingCategory));

			//Cache
			ParamsDefinition.Add(BXParametersDefinition.Cache);
		}
		protected override void LoadComponentDefinition()
		{
			IList<BXParamValue> sortByValues = ParamsDefinition["SortBy"].Values;
			if (sortByValues.Count > 0)
				sortByValues.Clear();
			sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.Date"), Sorting.ByDate.ToString()));
			sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.Views"), Sorting.ByViews.ToString()));
			sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.Comments"), Sorting.ByComments.ToString()));
			sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.VotingTotals"), Sorting.ByVotingTotals.ToString()));

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
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}

			AbortCache();
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

			AbortCache();
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
		public enum Sorting
		{
			ByDate = 1,
			ByViews,
			ByComments,
			ByVotingTotals
		}
		public class PostInfo
		{
			private BXBlogPost post;
			private string postViewHref = string.Empty;
			private string postEditHref = string.Empty;
			private string postRssHref = string.Empty;
			private string userProfileHref = string.Empty;
			private string blogHref = string.Empty;
			private string authorNameHtml = string.Empty;
			private string titleHtml = string.Empty;
			private BlogPostsComponent component;
			private BXBlogAuthorization auth;
			private List<TagInfo> tags;
			private double votingTotalValue = 0D;
			private int votingTotalVotes = 0;

			public BXBlogPost Post
			{
				get { return post; }
				internal set { post = value; }
			}
			public string PostViewHref
			{
				get { return postViewHref; }
				internal set { postViewHref = value ?? string.Empty; }
			}
			public string PostEditHref
			{
				get { return postEditHref; }
				internal set { postEditHref = value ?? string.Empty; }
			}

			public string PostRssHref
			{
				get { return postRssHref; }
				internal set { postRssHref = value ?? string.Empty; }
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
			
			public string GetContentHtml()
			{
				return component.GetProcessor(post).Process(post.Content);
			}
			public string GetPreviewHtml(int length)
			{
				string preview = component.GetProcessor(post).ConvertToText(post.Content);
				preview = BXStringUtility.StripOffSimpleTags(preview);

				if (length > 0 && preview.Length > length)
					preview = preview.Substring(0, length) + " ...";

				return BXWordBreakingProcessor.Break(preview, component.MaxWordLength, true);
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

			public BXBlogAuthorization Auth
			{
				get { return auth; }
				internal set { auth = value; }
			}

			internal PostInfo(BlogPostsComponent component)
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

	public class BlogPostsTemplate : BXComponentTemplate<BlogPostsComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & BlogPostsComponent.ErrorCode.Fatal) != 0)
				BXError404Manager.Set404Status(Response);
		}
		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			base.Render(writer);
		}
	}
	public interface IBlogPostsComponentTagFilter
	{
		void InitFilter(string siteId, string tags, string blogSlugs, List<int> ids, bool exclude, int categoryId);
		int Count();
		IList<int> GetPostIdList(BXPagingOptions paging);
	}
}
