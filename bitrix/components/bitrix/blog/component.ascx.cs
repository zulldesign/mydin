using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Modules;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using System.Web.Configuration;
using Bitrix.Security;

namespace Bitrix.Blog.Components
{
	public partial class BlogComponent : BXComponent
	{
		private static readonly bool IsSearch = Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search");

		//PROPERTIES
		private string BlogVariable
		{
			get
			{
				return Parameters.GetString("BlogVariable", "blog");
			}
			set
			{
				Parameters["BlogVariable"] = value;
			}
		}
		private string CategoryVariable
		{
			get
			{
				return Parameters.GetString("CategoryVariable", "category");
			}
			set
			{
				Parameters["CategoryVariable"] = value;
			}
		}
		private string CommentVariable
		{
			get
			{
				return Parameters.GetString("CommentVariable", "");
			}
			set
			{
				Parameters["CommentVariable"] = value;
			}
		}
		private string PostVariable
		{
			get
			{
				return Parameters.GetString("PostVariable", "");
			}
			set
			{
				Parameters["PostVariable"] = value;
			}
		}
		private string ActionVariable
		{
			get
			{
				return Parameters.GetString("ActionVariable", "");
			}
			set
			{
				Parameters["ActionVariable"] = value;
			}
		}
		private string PageVariable
		{
			get
			{
				return Parameters.GetString("PageVariable", "");
			}
			set
			{
				Parameters["PageVariable"] = value;
			}
		}
		private string UserVariable
		{
			get
			{
				return Parameters.GetString("UserVariable", "");
			}
			set
			{
				Parameters["UserVariable"] = value;
			}
		}
		private string TagsVariable
		{
			get
			{
				return Parameters.GetString("TagsVariable", "tags");
			}
			set
			{
				Parameters["TagsVariable"] = value ?? string.Empty;
			}
		}
        private bool SortBlogPostsByVotingTotals
        {
            get { return Parameters.GetBool("SortBlogPostsByVotingTotals", false); }
            set { Parameters["SortBlogPostsByVotingTotals"] = value.ToString(); }

        }
		public bool ShouldSetPageTitle
		{
			get
			{
				return Parameters.GetBool("SetPageTitle");
			}
			set
			{
				Parameters["SetPageTitle"] = value.ToString();
			}
		}
		public string PageTitle
		{
			get
			{
				return ComponentCache.GetString("PageTitle");
			}
		}

        public bool DisplayMenu
        {
            get { return Parameters.GetBool("DisplayMenu", true); }
            set { Parameters["DisplayMenu"] = value.ToString(); }
        }

		public bool DisplaySidebar
		{
			get { return Parameters.GetBool("DisplaySidebar", true); }
			set { Parameters["DisplaySidebar"] = value.ToString(); }
		}

		public bool AllowMetaWeblogApi
		{
			get { return IsSearch && Parameters.GetBool("AllowMetaWeblogApi", false); }
			set { Parameters["AllowMetaWeblogApi"] = value.ToString(); }
		}

		public int CategoryId
		{
			get
			{
				return Parameters.GetInt("CategoryId");
			}
		}

		//METHODS
		public void SetPageTitle(string titleText)
		{
			if (titleText == null)
				return;

			BXPublicPage bitrixPage = Page as BXPublicPage;
			if (bitrixPage != null && !IsComponentDesignMode)
				bitrixPage.BXTitle = Encode(titleText);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
            Parameters["PagingBoundedBorderSize"] = "1";

            ComponentCache["PostsPerPage"] = Parameters.GetString("PostsPerPage", "10");
            ComponentCache["BlogsPerPage"] = Parameters.GetString("BlogsPerPage", "20");
            ComponentCache["CommentsPerPage"] = Parameters.GetString("CommentsPerPage", "30");
            ComponentCache["MainPagePostCount"] = Parameters.GetString("MainPagePostCount", "6");
            ComponentCache["MainPageBlogCount"] = Parameters.GetString("MainPageBlogCount", "6");
            ComponentCache["DisplayMenu"] = DisplayMenu;
			ComponentCache["DisplaySidebar"] = DisplaySidebar;
            ComponentCache["PopularPostsSortBy"] = SortBlogPostsByVotingTotals ? "ByVotingTotals" : "ByViews";

			ComponentCache["CategoryId"] = Parameters.GetString("CategoryId", "0");

            string page = EnableSef ? PrepareSefMode() : PrepareNormalMode();

			bool setMasterTitle = Parameters.GetBool("SetPageTitle", true);
			if (setMasterTitle && Parameters.GetBool("DontSetPostsMasterTitle", false))
			{
				if (page == "posts" && ComponentCache.GetString("PublishMode") == "Published" || page == "post")
				{
					var bxPage = Page as BXPublicPage;
					if (bxPage != null)
						bxPage.MasterTitleHtml = "";
					setMasterTitle = false;
				}
			}
			ComponentCache["SetMasterTitle"] = setMasterTitle;

            IncludeComponentTemplate(page);
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = BXBlogModuleConfiguration.GetComponentGroup();

            BXCategory urlCategory = BXCategory.UrlSettings,
                mainCategory = BXCategory.Main,
                additionalSettingsCategory = BXCategory.AdditionalSettings,
                sefCategory = BXCategory.Sef,
                customFieldCategory = BXCategory.CustomField,
                votingCategory = new BXCategory(GetMessage("Category.Voting"), "Voting", 220);

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			ParamsDefinition["PagingTemplate"].DefaultValue = "customizable";
			ParamsDefinition["PagingTitle"].DefaultValue = GetMessageRaw("DefaultPagingTitle");
			ParamsDefinition["PagingMaxPages"].DefaultValue = "5";
			ParamsDefinition.Remove("PagingRecordsPerPage");

			ParamsDefinition.Add(BXParametersDefinition.Sef);
			ParamsDefinition.Add(BXParametersDefinition.Search);

			ParamsDefinition["ColorCss"] = new BXParamSingleSelectionWithText(GetMessageRaw("Param.ColorCss"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", mainCategory);

            ParamsDefinition["AllowToAjustBlogGroups"] =
                new BXParamYesNo(
                    GetMessageRaw("Param.AllowToAjustBlogGroups"),
                    true,
                    mainCategory,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "AllowToAjustBlogGroups", "NonObligatoryBlogGroupsBinding", "ObligatoryBlogGroupsBinding")
                    );
            ParamsDefinition["ObligatoryBlogGroups"] =
                new BXParamMultiSelection(
                    GetMessageRaw("Param.ObligatoryBlogGroups"),
                    string.Empty,
                    mainCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "ObligatoryBlogGroups", new string[] { "ObligatoryBlogGroupsBinding" })
                    );
            ParamsDefinition["MaxBlogGroups"] =
                new BXParamText(
                    GetMessageRaw("Param.MaxBlogGroups"),
                    "1",
                    mainCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "MaxBlogGroups", new string[] { "NonObligatoryBlogGroupsBinding" })
                    );
            ParamsDefinition["AvailableBlogGroups"] =
                new BXParamMultiSelection(
                    GetMessageRaw("Param.AvailableGroups"),
                    string.Empty,
                    mainCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "AvailableBlogGroups", new string[] { "NonObligatoryBlogGroupsBinding" })
                    );

			ParamsDefinition["CategoryId"] = new BXParamSingleSelection(GetMessageRaw("Param.CategoryId"), "", mainCategory);

			ParamsDefinition["SlugIgnoreList"] = new BXParamText(GetMessageRaw("Param.SlugIgnoreList"), "admin;blog;blogs;users;user;search;posts;rss;tags;new", mainCategory);
			ParamsDefinition["MinSlugLength"] = new BXParamText(GetMessageRaw("Param.MinSlugLength"), "3", mainCategory);
			ParamsDefinition["MaxSlugLength"] = new BXParamText(GetMessageRaw("Param.MaxSlugLength"), "10", mainCategory);

            ParamsDefinition["AvailableBlogCustomFieldsForAuthor"] =
                new BXParamMultiSelection(
                    GetMessageRaw("Param.AvailableBlogCustomFieldsForAuthor"),
                    string.Empty,
                    customFieldCategory
                    );

            ParamsDefinition["AvailableBlogCustomFieldsForModerator"] =
                new BXParamMultiSelection(
                    GetMessageRaw("Param.AvailableBlogCustomFieldsForModerator"),
                    string.Empty,
                    customFieldCategory
                    );

            ParamsDefinition["AvailablePostCustomFieldsForAuthor"] =
                new BXParamMultiSelection(
                    GetMessageRaw("Param.AvailablePostCustomFieldsForAuthor"),
                    string.Empty,
                    customFieldCategory
                    );

            ParamsDefinition["AvailablePostCustomFieldsForModerator"] =
                new BXParamMultiSelection(
                    GetMessageRaw("Param.AvailablePostCustomFieldsForModerator"),
                    string.Empty,
                    customFieldCategory
                    );

			// Query string variable names
			ParamsDefinition["BlogVariable"] = new BXParamText(GetMessageRaw("Param.BlogVariable"), "blog", sefCategory);
			ParamsDefinition["CategoryVariable"] = new BXParamText(GetMessageRaw("Param.CategoryVariable"), "category", sefCategory);
			ParamsDefinition["CommentVariable"] = new BXParamText(GetMessageRaw("Param.CommentVariable"), "comment", sefCategory);
			ParamsDefinition["PostVariable"] = new BXParamText(GetMessageRaw("Param.PostVariable"), "post", sefCategory);
			ParamsDefinition["ActionVariable"] = new BXParamText(GetMessageRaw("Param.ActionVariable"), "act", sefCategory);
			ParamsDefinition["PageVariable"] = new BXParamText(GetMessageRaw("Param.PageVariable"), "page", sefCategory);
			ParamsDefinition["UserVariable"] = new BXParamText(GetMessageRaw("Param.UserVariable"), "user", sefCategory);
			ParamsDefinition["TagsVariable"] = new BXParamText(GetMessageRaw("Param.TagsVariable"), "tags", sefCategory);

			// Sef templates
			ParamsDefinition["NewPostTemplate"] = new BXParamText(GetMessageRaw("Param.NewPostTemplate"), "/#BlogSlug#/new/", sefCategory);
			ParamsDefinition["PostListTemplate"] = new BXParamText(GetMessageRaw("Param.PostListTemplate"), "/#BlogSlug#/", sefCategory);
			ParamsDefinition["PostListPageTemplate"] = new BXParamText(GetMessageRaw("Param.PostListPageTemplate"), "/#BlogSlug#/?page=#PageId#", sefCategory);
			ParamsDefinition["DraftPostListTemplate"] = new BXParamText(GetMessageRaw("Param.DraftPostListTemplate"), "/#BlogSlug#/draft/", sefCategory);
			ParamsDefinition["DraftPostListPageTemplate"] = new BXParamText(GetMessageRaw("Param.DraftPostListPageTemplate"), "/#BlogSlug#/draft/?page=#PageId#", sefCategory);
			ParamsDefinition["PostTemplate"] = new BXParamText(GetMessageRaw("Param.PostTemplate"), "/#BlogSlug#/#PostId#/", sefCategory);
			ParamsDefinition["PostPageTemplate"] = new BXParamText(GetMessageRaw("Param.PostPageTemplate"), "/#BlogSlug#/#PostId#/?page=#PageId###comments", sefCategory);
			ParamsDefinition["PostEditTemplate"] = new BXParamText(GetMessageRaw("Param.PostEditTemplate"), "/#BlogSlug#/#PostId#/edit/", sefCategory);
			ParamsDefinition["CommentReadTemplate"] = new BXParamText(GetMessageRaw("Param.CommentReadTemplate"), "/#BlogSlug#/#PostId#/#CommentId#/##comment#CommentId#", sefCategory);
			ParamsDefinition["CommentEditTemplate"] = new BXParamText(GetMessageRaw("Param.CommentEditTemplate"), "/#BlogSlug#/#PostId#/#CommentId#/edit/", sefCategory);
			ParamsDefinition["UserProfileTemplate"] = new BXParamText(GetMessageRaw("Param.UserProfileTemplate"), "/users/#UserId#/", sefCategory);
			ParamsDefinition["UserProfileEditTemplate"] = new BXParamText(GetMessageRaw("Param.UserProfileEditTemplate"), "/users/#UserId#/edit/", sefCategory);
			ParamsDefinition["NewBlogTemplate"] = new BXParamText(GetMessageRaw("Param.NewBlogTemplate"), "/new/", sefCategory);
			ParamsDefinition["BlogEditTemplate"] = new BXParamText(GetMessageRaw("Param.BlogEditTemplate"), "/#BlogSlug#/edit/", sefCategory);
			ParamsDefinition["BlogListTemplate"] = new BXParamText(GetMessageRaw("Param.BlogListTemplate"), "/blogs/", sefCategory);
			ParamsDefinition["BlogListPageTemplate"] = new BXParamText(GetMessageRaw("Param.BlogListPageTemplate"), "/blogs/?page=#PageId#", sefCategory);
			ParamsDefinition["CategoryBlogListTemplate"] = new BXParamText(GetMessageRaw("Param.CategoryBlogListTemplate"), "/blogs/#CategoryId#/", sefCategory);
			ParamsDefinition["CategoryBlogListPageTemplate"] = new BXParamText(GetMessageRaw("Param.CategoryBlogListPageTemplate"), "/blogs/#CategoryId#/?page=#PageId#", sefCategory);
			
			ParamsDefinition["NewPostListTemplate"] = new BXParamText(GetMessageRaw("Param.NewPostListTemplate"), "/posts/", sefCategory);
			ParamsDefinition["NewPostListPageTemplate"] = new BXParamText(GetMessageRaw("Param.NewPostListPageTemplate"), "/posts/?page=#PageId#", sefCategory);
			ParamsDefinition["PopularPostListTemplate"] = new BXParamText(GetMessageRaw("Param.PopularPostListTemplate"), "/posts/popular/", sefCategory);
			ParamsDefinition["PopularPostListPageTemplate"] = new BXParamText(GetMessageRaw("Param.PopularPostListPageTemplate"), "/posts/popular/?page=#PageId#", sefCategory);
			ParamsDefinition["DiscussPostListTemplate"] = new BXParamText(GetMessageRaw("Param.DiscussPostListTemplate"), "/posts/discuss/", sefCategory);
			ParamsDefinition["DiscussPostListPageTemplate"] = new BXParamText(GetMessageRaw("Param.DiscussPostListPageTemplate"), "/posts/discuss/?page=#PageId#", sefCategory);
			if (IsSearch)
			{
                ParamsDefinition["SearchTagsTemplate"] = new BXParamText(GetMessageRaw("Param.SearchTagsTemplate"), "/posts/search/?tags=#SearchTags#", sefCategory);
                ParamsDefinition["SearchTagsPageTemplate"] = new BXParamText(GetMessageRaw("Param.SearchTagsPageTemplate"), "/posts/search/?tags=#SearchTags#&page=#PageId#", sefCategory);

                ParamsDefinition["SearchBlogTagsTemplate"] = new BXParamText(GetMessageRaw("Param.SearchBlogTagsTemplate"), "/#BlogSlug#/search/?tags=#SearchTags#", sefCategory);
                ParamsDefinition["SearchBlogTagsPageTemplate"] = new BXParamText(GetMessageRaw("Param.SearchBlogTagsPageTemplate"), "/#BlogSlug#/search/?tags=#SearchTags#&page=#PageId#", sefCategory);

				ParamsDefinition["BlogMetaWeblogApiTemplate"] = new BXParamText(GetMessageRaw("Param.BlogMetaWeblogApiTemplate"), "/#BlogSlug#/metaweblog/", sefCategory);
				ParamsDefinition["BlogMetaWeblogManifestTemplate"] = new BXParamText(GetMessageRaw("Param.BlogMetaWeblogManifestTemplate"), "/#BlogSlug#/wlwmanifest.xml", sefCategory);
			}

            ParamsDefinition["RssAllPostsTemplate"] = new BXParamText(GetMessageRaw("Param.RssAllPostsTemplate"), "/rss/", sefCategory);
			ParamsDefinition["RssBlogPostsTemplate"] = new BXParamText(GetMessageRaw("Param.RssBlogPostsTemplate"), "/#BlogSlug#/rss/", sefCategory);
            ParamsDefinition["RssBlogPostCommentsTemplate"] = new BXParamText(GetMessageRaw("Param.RssBlogPostCommentsTemplate"), "/#BlogSlug#/#PostId#/rss/", sefCategory);

			//Additional options
			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);
			ParamsDefinition["DontSetPostsMasterTitle"] = new BXParamYesNo(GetMessageRaw("Param.DontSetPostsMasterTitle"), false, additionalSettingsCategory);
            ParamsDefinition["DisplayMenu"] = new BXParamYesNo(GetMessageRaw("Param.DisplayMenu"), true, additionalSettingsCategory);
			ParamsDefinition["DisplaySidebar"] = new BXParamYesNo(GetMessageRaw("Param.DisplaySidebar"), true, additionalSettingsCategory);
			
			ParamsDefinition["MaxWordLength"] = new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory);
			ParamsDefinition["PeriodDays"] = new BXParamText(GetMessageRaw("Param.PeriodDays"), "30", additionalSettingsCategory);

			ParamsDefinition["PostsPerPage"] = new BXParamText(GetMessageRaw("Param.PostsPerPage"), "10", additionalSettingsCategory);
			ParamsDefinition["BlogsPerPage"] = new BXParamText(GetMessageRaw("Param.BlogsPerPage"), "20", additionalSettingsCategory);
			ParamsDefinition["CommentsPerPage"] = new BXParamText(GetMessageRaw("Param.CommentsPerPage"), "30", additionalSettingsCategory);

			ParamsDefinition["MainPagePostCount"] = new BXParamText(GetMessageRaw("Param.MainPagePostCount"), "6", additionalSettingsCategory);
			ParamsDefinition["MainPageBlogCount"] = new BXParamText(GetMessageRaw("Param.MainPageBlogCount"), "6", additionalSettingsCategory);

			if (IsSearch)
				ParamsDefinition["AllowMetaWeblogApi"] = new BXParamYesNo(GetMessageRaw("Param.AllowMetaWeblogApi"), false, additionalSettingsCategory);

            ParamsDefinition.Add("EnableVotingForPost", new BXParamYesNo(GetMessageRaw("Param.EnableVotingForPost"), false, votingCategory));
            ParamsDefinition.Add("EnableVotingForComment", new BXParamYesNo(GetMessageRaw("Param.EnableVotingForComment"), false, votingCategory));
            ParamsDefinition.Add("RolesAuthorizedToVote", new BXParamMultiSelection(GetMessageRaw("Param.RolesAuthorizedToVote"), "User", votingCategory));
            ParamsDefinition.Add("SortBlogPostsByVotingTotals", new BXParamYesNo(GetMessageRaw("Param.SortBlogPostsByVotingTotals"), false, votingCategory));

			BXCategory guestsCategory = new BXCategory(GetMessageRaw("Category.Guests"), "guest_parameters", mainCategory.Sort + 100);
			ParamsDefinition["GuestEmail"] = new BXParamSingleSelection(GetMessageRaw("Param.GuestEmail"), "required", guestsCategory);
			ParamsDefinition["GuestCaptcha"] = new BXParamYesNo(GetMessageRaw("Param.GuestCaptcha"), true, guestsCategory);

			ParamsDefinition["UserProfileUrl"] = new BXParamText(GetMessageRaw("Param.UserProfileUrl"), "", urlCategory);
			ParamsDefinition["UserProfileEditUrl"] = new BXParamText(GetMessageRaw("Param.UserProfileEditUrl"), "", urlCategory);

            //File limitations
            int maxRequestLength = MaxRequestLength;
            ParamsDefinition.Add("MaximalFileSizeInKBytes", new BXParamText(maxRequestLength <= 0 ? GetMessageRaw("Param.MaximalFileSizeInKBytes") : string.Format(GetMessageRaw("Param.MaximalFileSizeInKBytesAdd"), maxRequestLength.ToString()), "1024", additionalSettingsCategory));
            ParamsDefinition.Add("MaximalAllFilesSizeInKBytes", new BXParamText(GetMessageRaw("Param.MaximalAllFilesSizeInKBytes"), "8192", additionalSettingsCategory));

            //Image limitations
            ParamsDefinition.Add("MaximalImageWidthInPixels", new BXParamText(GetMessageRaw("Param.MaximalImageWidthInPixels"), "0", additionalSettingsCategory));
            ParamsDefinition.Add("MaximalImageHeightInPixels", new BXParamText(GetMessageRaw("Param.MaximalImageHeightInPixels"), "0", additionalSettingsCategory));

            //Post Form Settings
            ParamsDefinition.Add("PostCutThreshold", new BXParamText(GetMessageRaw("Param.PostCutThreshold"), "0", additionalSettingsCategory));


            //Cache
            ParamsDefinition.Add(BXParametersDefinition.Cache);
		}
		protected override void LoadComponentDefinition()
		{
			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			string csaId = ClientID;

			ParamsDefinition["EnableSEF"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(csaId, "EnableSEF", "Sef", "NonSef");
			ParamsDefinition["SEFFolder"].ClientSideAction = new ParamClientSideActionGroupViewMember(csaId, "SEFFolder", new string[] { "Sef" });

			ParamsDefinition["SetPageTitle"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(csaId, "SetPageTitle", "PageTitleOn", "PageTitleOff");
			ParamsDefinition["DontSetPostsMasterTitle"].ClientSideAction = new ParamClientSideActionGroupViewMember(csaId, "DontSetPostsMasterTitle", new string[] { "PageTitleOn" });

			foreach (string id in new string[] {
				"BlogVariable", "CategoryVariable", "CommentVariable", "PostVariable", 
				"ActionVariable",  "PageVariable", "UserVariable", "TagsVariable"
			})
				ParamsDefinition[id].ClientSideAction = new ParamClientSideActionGroupViewMember(csaId, id, new string[] { "NonSef" });

			List<string> ids = new List<string>(new string[] {
				"NewPostTemplate", "PostListTemplate", "PostListPageTemplate", "DraftPostListTemplate", "DraftPostListPageTemplate",
				"PostTemplate", "PostPageTemplate", "PostEditTemplate",
				"CommentReadTemplate", "CommentEditTemplate",
				"UserProfileTemplate", "UserProfileEditTemplate",
				"NewBlogTemplate", "BlogEditTemplate", "BlogListTemplate", "BlogListPageTemplate",
				"CategoryBlogListTemplate", "CategoryBlogListPageTemplate",
				"NewPostListTemplate", "NewPostListPageTemplate", 
				"PopularPostListTemplate", "PopularPostListPageTemplate", 
				"DiscussPostListTemplate", "DiscussPostListPageTemplate",
				"RssAllPostsTemplate", "RssBlogPostsTemplate", "RssBlogPostCommentsTemplate"
			});
			if (IsSearch)
			{
				ids.Add("SearchTagsTemplate");
				ids.Add("SearchTagsPageTemplate");
                ids.Add("SearchBlogTagsTemplate");
                ids.Add("SearchBlogTagsPageTemplate");
				ids.Add("BlogMetaWeblogApiTemplate");
				ids.Add("BlogMetaWeblogManifestTemplate");
			}

			foreach (string id in ids)
				ParamsDefinition[id].ClientSideAction = new ParamClientSideActionGroupViewMember(csaId, id, new string[] { "Sef" });


			DirectoryInfo info = new DirectoryInfo(BXPath.MapPath("~/bitrix/components/bitrix/blog/templates/.default/themes/"));
			if (info.Exists)
			{
				List<BXParamValue> values = new List<BXParamValue>();
				foreach (DirectoryInfo sub in info.GetDirectories())
				{
					if (!File.Exists(Path.Combine(sub.FullName, "style.css")))
						continue;

					string themeTitle = BXLoc.GetMessage("~/bitrix/components/bitrix/blog/templates/.default/themes/" + sub.Name + "/description", "Title");
					if (String.IsNullOrEmpty(themeTitle))
						themeTitle = sub.Name;

					values.Add(new BXParamValue(themeTitle, VirtualPathUtility.Combine("~/bitrix/components/bitrix/blog/templates/.default/themes/", sub.Name + "/style.css")));
				}
				ParamsDefinition["ColorCss"].Values = values;
			}

			ParamsDefinition["GuestEmail"].Values = new List<BXParamValue>(new BXParamValue[] {
				new BXParamValue(GetMessageRaw("GuestEmail.Hide"), ""),
				new BXParamValue(GetMessageRaw("GuestEmail.Show"), "visible"),
				new BXParamValue(GetMessageRaw("GuestEmail.Require"), "required")
			});

            //AvailableBlogGroups, ObligatoryBlogGroups
            IList<BXParamValue> availableBlogGroupsValues = ((BXParamMultiSelection)ParamsDefinition["AvailableBlogGroups"]).Values;
            if (availableBlogGroupsValues.Count > 0)
                availableBlogGroupsValues.Clear();

			IList<BXParamValue> categoryIds = ((BXParamSingleSelection)ParamsDefinition["CategoryId"]).Values;
			if (categoryIds.Count > 0)
				categoryIds.Clear();

            IList<BXParamValue> obligatoryBlogGroupsValues = ((BXParamMultiSelection)ParamsDefinition["ObligatoryBlogGroups"]).Values;
            if (obligatoryBlogGroupsValues.Count > 0)
                obligatoryBlogGroupsValues.Clear();

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
                availableBlogGroupsValues.Add(value);
                obligatoryBlogGroupsValues.Add(value);
				categoryIds.Add(value);
            }
            //---
            //AvailablePostCustomFieldsForAuthor, AvailablePostCustomFieldsForModerator
            IList<BXParamValue> availablePostCustomFieldsForAuthor = ((BXParamMultiSelection)ParamsDefinition["AvailablePostCustomFieldsForAuthor"]).Values;
            if (availablePostCustomFieldsForAuthor.Count > 0)
                availablePostCustomFieldsForAuthor.Clear();

            IList<BXParamValue> availablePostCustomFieldsForModerator = ((BXParamMultiSelection)ParamsDefinition["AvailablePostCustomFieldsForModerator"]).Values;
            if (availablePostCustomFieldsForModerator.Count > 0)
                availablePostCustomFieldsForModerator.Clear();

            BXCustomFieldCollection postFields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.PostCustomFieldEntityId);
            foreach (BXCustomField field in postFields)
            {
                BXParamValue param = new BXParamValue(field.TextEncoder.Decode(field.EditFormLabel), field.Name);
                availablePostCustomFieldsForAuthor.Add(param);
                availablePostCustomFieldsForModerator.Add(param);
            }
            //---
            //AvailableBlogCustomFieldsForAuthor, AvailableBlogCustomFieldsForModerator
            IList<BXParamValue> availableBlogCustomFieldsForAuthor = ((BXParamMultiSelection)ParamsDefinition["AvailableBlogCustomFieldsForAuthor"]).Values;
            if (availableBlogCustomFieldsForAuthor.Count > 0)
                availableBlogCustomFieldsForAuthor.Clear();

            IList<BXParamValue> availableBlogCustomFieldsForModerator = ((BXParamMultiSelection)ParamsDefinition["AvailableBlogCustomFieldsForModerator"]).Values;
            if (availableBlogCustomFieldsForModerator.Count > 0)
                availableBlogCustomFieldsForModerator.Clear();

            BXCustomFieldCollection blogFields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.BlogCustomFieldEntityId);
            foreach (BXCustomField field in blogFields)
            {
                BXParamValue param = new BXParamValue(field.TextEncoder.Decode(field.EditFormLabel), field.Name);
                availableBlogCustomFieldsForAuthor.Add(param);
                availableBlogCustomFieldsForModerator.Add(param);
            }
            //---
            //RolesAuthorizedToVote
            IList<BXParamValue> rolesValues = ParamsDefinition["RolesAuthorizedToVote"].Values;
            rolesValues.Clear();
            foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
            {
                if (string.Equals(r.RoleName, "Guest", StringComparison.Ordinal))
                    continue;
                rolesValues.Add(new BXParamValue(r.Title, r.RoleName));
            }
            //---
		}
		public override void ProcessMessage(BXCommand cmd, bool executionContext, string executionVirtualPath)
		{
			if (cmd.Action == "Bitrix.Search.ProvideUrl")
			{
				if (cmd.Parameters.GetString("moduleId") != "blog")
					return;

				string postIdString = cmd.Parameters.GetString("itemId");
				int postId;
				if (string.IsNullOrEmpty(postIdString) || !postIdString.StartsWith("p") || !int.TryParse(postIdString.Substring(1), out postId))
					return;

				string url;
				if (EnableSef)
				{
					BXParamsBag<object> replaceItems = new BXParamsBag<object>();
					replaceItems.Add("BlogId", cmd.Parameters.GetString("itemGroup"));
					replaceItems.Add("BlogSlug", cmd.Parameters.GetString("param1"));
					replaceItems.Add("PostId", postId);

					string template = CombineLink(Parameters.GetString("SEFFolder", ""), Parameters.GetString("PostTemplate"));
					url = BXSefUrlUtility.MakeLink(template, replaceItems);
				}
				else
					url = String.Format("{0}?{1}={2}", BXSite.GetUrlForPath(executionVirtualPath, null, cmd.SiteId), PostVariable, postId);
				
				cmd.AddCommandResult("bitrix:blog@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, url));
			}
		}

		private string PrepareSefMode()
		{
			string sefFolder = Parameters.GetString("SEFFolder", "");

			ComponentCache["IndexUrlTemplate"] = CombineLink(sefFolder, "");
			ComponentCache["NewPostUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("NewPostTemplate"));
			ComponentCache["PostListUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostListTemplate"));
			ComponentCache["PostListPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostListPageTemplate"));
			ComponentCache["DraftPostListUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("DraftPostListTemplate"));
			ComponentCache["DraftPostListPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("DraftPostListPageTemplate"));
			ComponentCache["PostUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostTemplate"));
			ComponentCache["PostPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostPageTemplate"));
			ComponentCache["PostEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostEditTemplate"));
			ComponentCache["CommentReadUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("CommentReadTemplate"));
			ComponentCache["CommentEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("CommentEditTemplate"));
			ComponentCache["UserProfileUrlTemplate"] = GetExternalUrlTemplate("UserProfileUrl") ?? CombineLink(sefFolder, Parameters.GetString("UserProfileTemplate"));
			ComponentCache["UserProfileEditUrlTemplate"] = GetExternalUrlTemplate("UserProfileEditUrl") ?? CombineLink(sefFolder, Parameters.GetString("UserProfileEditTemplate"));
			ComponentCache["NewBlogUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("NewBlogTemplate"));
			ComponentCache["BlogEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("BlogEditTemplate"));
			ComponentCache["BlogListUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("BlogListTemplate"));
			ComponentCache["BlogListPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("BlogListPageTemplate"));
			ComponentCache["CategoryBlogListUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("CategoryBlogListTemplate"));
			ComponentCache["CategoryBlogListPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("CategoryBlogListPageTemplate"));
			ComponentCache["NewPostListUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("NewPostListTemplate"));
			ComponentCache["NewPostListPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("NewPostListPageTemplate"));
			ComponentCache["PopularPostListUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PopularPostListTemplate"));
			ComponentCache["PopularPostListPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PopularPostListPageTemplate"));
			ComponentCache["DiscussPostListUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("DiscussPostListTemplate"));
			ComponentCache["DiscussPostListPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("DiscussPostListPageTemplate"));

			
			ComponentCache["BlogAbsoluteUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostListTemplate"));

			if (IsSearch)
			{
				ComponentCache["SearchTagsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("SearchTagsTemplate"));
				ComponentCache["SearchTagsPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("SearchTagsPageTemplate"));
                ComponentCache["SearchBlogTagsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("SearchBlogTagsTemplate"));
                ComponentCache["SearchBlogTagsPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("SearchBlogTagsPageTemplate"));

				if (AllowMetaWeblogApi)
				{
					ComponentCache["BlogMetaWeblogApiUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("BlogMetaWeblogApiTemplate"));
					ComponentCache["BlogMetaWeblogManifestUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("BlogMetaWeblogManifestTemplate"));
				}
			}

            ComponentCache["RssAllPostsTemplate"] = CombineLink(sefFolder, Parameters.GetString("RssAllPostsTemplate"));
			ComponentCache["RssBlogPostsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("RssBlogPostsTemplate"));
            ComponentCache["RssBlogPostCommentsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("RssBlogPostCommentsTemplate"));

			if (CategoryId > 0)
				ComponentCache["CategoryId"] = CategoryId.ToString();

			return PrepareSefPage();
		}

		private string PrepareSefPage()
		{
			BXParamsBag<string> sefMap = new BXParamsBag<string>();

			sefMap.Add("post-edit.new", Parameters.GetString("NewPostTemplate"));
			sefMap.Add("post-edit.edit", Parameters.GetString("PostEditTemplate"));
			sefMap.Add("blog", Parameters.GetString("PostListTemplate"));
			sefMap.Add("blog.page", Parameters.GetString("PostListPageTemplate"));
			sefMap.Add("blog.draft", Parameters.GetString("DraftPostListTemplate"));
			sefMap.Add("blog.draft.page", Parameters.GetString("DraftPostListPageTemplate"));
			sefMap.Add("post", Parameters.GetString("PostTemplate"));
			sefMap.Add("post.page", Parameters.GetString("PostPageTemplate"));
			sefMap.Add("post.comment", Parameters.GetString("CommentReadTemplate"));
			//sefMap.Add("comment.edit", Parameters.GetString("CommentEditTemplate"));
			sefMap.Add("profile-view", Parameters.GetString("UserProfileTemplate"));
			sefMap.Add("profile-edit", Parameters.GetString("UserProfileEditTemplate"));
			sefMap.Add("blogs", Parameters.GetString("BlogListTemplate"));
			sefMap.Add("blogs.page", Parameters.GetString("BlogListPageTemplate"));
			sefMap.Add("blogs.category", Parameters.GetString("CategoryBlogListTemplate"));
			sefMap.Add("blogs.category.page", Parameters.GetString("CategoryBlogListPageTemplate"));
			sefMap.Add("blog-edit.new", Parameters.GetString("NewBlogTemplate"));
			sefMap.Add("blog-edit.edit", Parameters.GetString("BlogEditTemplate"));

			sefMap.Add("posts.new", Parameters.GetString("NewPostListTemplate"));
			sefMap.Add("posts.new.page", Parameters.GetString("NewPostListPageTemplate"));
			sefMap.Add("posts.popular", Parameters.GetString("PopularPostListTemplate"));
			sefMap.Add("posts.popular.page", Parameters.GetString("PopularPostListPageTemplate"));
			sefMap.Add("posts.discuss", Parameters.GetString("DiscussPostListTemplate"));
			sefMap.Add("posts.discuss.page", Parameters.GetString("DiscussPostListPageTemplate"));

			if (IsSearch)
			{
				sefMap.Add("search.tags", Parameters.GetString("SearchTagsTemplate"));
				sefMap.Add("search.tags.page", Parameters.GetString("SearchTagsPageTemplate"));
                sefMap.Add("search.blog.tags", Parameters.GetString("SearchBlogTagsTemplate"));
                sefMap.Add("search.blog.tags.page", Parameters.GetString("SearchBlogTagsPageTemplate")); 
				
				if (AllowMetaWeblogApi)
				{
					sefMap.Add("metaweblog", Parameters.GetString("BlogMetaWeblogApiTemplate")); 
					sefMap.Add("wlwmanifest", Parameters.GetString("BlogMetaWeblogManifestTemplate")); 
				}
			}

            sefMap.Add("rss.posts", Parameters.GetString("RssAllPostsTemplate"));
			sefMap.Add("rss.blog.posts", Parameters.GetString("RssBlogPostsTemplate"));
            sefMap.Add("rss.blog.post.comments", Parameters.GetString("RssBlogPostCommentsTemplate"));

			string code = BXSefUrlUtility.MapVariable(Parameters.GetString("SEFFolder", ""), sefMap, ComponentCache, "index", null, null);

            if (IsSearch && ComponentCache.ContainsKey("BlogSlug"))
            {
                ComponentCache["SearchTagsUrlTemplate"] = ComponentCache["SearchBlogTagsUrlTemplate"];
                ComponentCache["SearchTagsPageUrlTemplate"] = ComponentCache["SearchBlogTagsPageUrlTemplate"];
            }

			string page;
			string action;
			int i = code.IndexOf('.');
			if (i == -1)
			{
				page = code;
				action = string.Empty;
			}
			else
			{
				page = code.Remove(i);
				action = code.Substring(i + 1);
			}

			switch (page)
			{
				case "blog":
					switch (action)
					{
						case "draft":
						case "draft.page":
							ComponentCache["PagingIndexTemplate"] = ComponentCache["DraftPostListUrlTemplate"];
							ComponentCache["PagingPageTemplate"] = ComponentCache["DraftPostListPageUrlTemplate"];
							ComponentCache["PublishMode"] = "Draft";
							break;
						default:
							ComponentCache["PagingIndexTemplate"] = ComponentCache["PostListUrlTemplate"];
							ComponentCache["PagingPageTemplate"] = ComponentCache["PostListPageUrlTemplate"];
							ComponentCache["PublishMode"] = "Published";
							break;
					}
					break;

				case "post-edit":
					ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostUrlTemplate"];
					break;
				case "blog-edit":
					ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostListUrlTemplate"];
					break;
				case "blogs":
					switch (action)
					{
						case "category":
						case "category.page":
							ComponentCache["PagingIndexTemplate"] = ComponentCache["CategoryBlogListUrlTemplate"];
							ComponentCache["PagingPageTemplate"] = ComponentCache["CategoryBlogListPageUrlTemplate"];
							break;
						default:
							ComponentCache["PagingIndexTemplate"] = ComponentCache["BlogListUrlTemplate"];
							ComponentCache["PagingPageTemplate"] = ComponentCache["BlogListPageUrlTemplate"];
							break;
					}
					break;
				case "posts":
					switch (action)
					{
						case "popular":
						case "popular.page":
							ComponentCache["PagingIndexTemplate"] = ComponentCache["PopularPostListUrlTemplate"];
							ComponentCache["PagingPageTemplate"] = ComponentCache["PopularPostListPageUrlTemplate"];
                            ComponentCache["PostsSortBy"] = SortBlogPostsByVotingTotals ? "ByVotingTotals" : "ByViews";
							break;
						case "discuss":
						case "discuss.page":
							ComponentCache["PagingIndexTemplate"] = ComponentCache["DiscussPostListUrlTemplate"];
							ComponentCache["PagingPageTemplate"] = ComponentCache["DiscussPostListPageUrlTemplate"];
							ComponentCache["PostsSortBy"] = "ByComments";
							break;
						default:
							ComponentCache["PagingIndexTemplate"] = ComponentCache["NewPostListUrlTemplate"];
							ComponentCache["PagingPageTemplate"] = ComponentCache["NewPostListPageUrlTemplate"];
							ComponentCache["PostsSortBy"] = "ByDate";
							Parameters["PeriodDays"] = "0";
							break;
					}
					break;
				case "profile-view":
				case "profile-edit":
					ComponentCache["ProfileFields"] = GetUserProfileFields();
					if (page == "profile-edit")
					{
						string url = GetExternalUrlTemplate("UserProfileEditUrl");
						if (url != null)
							RedirectTemplateUrl(url, ComponentCache);

						ComponentCache["PageTitle"] = GetMessageRaw("PageTitle.EditProfile");
						ComponentCache["RequiredProfileFields"] = GetRequiredUserProfileFields();
					}
					else
					{
						string url = GetExternalUrlTemplate("UserProfileUrl");
						if (url != null)
							RedirectTemplateUrl(url, ComponentCache);

						ComponentCache["PageTitle"] = GetMessageRaw("PageTitle.ViewProfile");
					}
					break;
                case "rss":
                    {
                        if (string.Equals(action, "blog.post.comments", StringComparison.Ordinal))
                        {
                            ComponentCache["RssStuffType"] = "Comment";
                            ComponentCache["RssFiltrationType"] = CategoryId > 0 ? "CategoryId" : "BlogSlug";
                            ComponentCache["RssItemUrlTemplate"] = ComponentCache["CommentReadUrlTemplate"];
                        }
                        else
                        {
                            ComponentCache["RssStuffType"] = "Post";
                            ComponentCache["RssFiltrationType"] =  CategoryId > 0 ? "CategoryId" : string.Equals(action, "blog.posts", StringComparison.Ordinal) ? "BlogSlug" : "None";
                            ComponentCache["RssItemUrlTemplate"] = ComponentCache["PostUrlTemplate"];
                        }
                    }
                    break;
				case "search":
					ComponentCache["PostsSortBy"] = "ByDate";
                    ComponentCache["PagingIndexTemplate"] = ComponentCache["SearchTagsUrlTemplate"];
                    ComponentCache["PagingPageTemplate"] = ComponentCache["SearchTagsPageUrlTemplate"];
					break;
			}

			return page;
		}

		private string PrepareNormalMode()
		{
			string filePath = BXSefUrlManager.CurrentUrl.AbsolutePath;
			string blogVar = UrlEncode(BlogVariable);
			string postVar = UrlEncode(PostVariable);
			string commentVar = UrlEncode(CommentVariable);
			string actionVar = UrlEncode(ActionVariable);
			string pageVar = UrlEncode(PageVariable);
			string tagsVar = UrlEncode(TagsVariable);

			ComponentCache["IndexUrlTemplate"] = filePath;
			ComponentCache["NewPostUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#&{2}=newpost", filePath, blogVar, actionVar);
			ComponentCache["PostListUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#", filePath, blogVar);
			ComponentCache["PostListPageUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#&{2}=#PageId#", filePath, blogVar, pageVar);
			ComponentCache["DraftPostListUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#&{2}=draft", filePath, blogVar, actionVar);
			ComponentCache["DraftPostListPageUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#&{2}=draft&{3}=#PageId#", filePath, blogVar, actionVar, pageVar);
			ComponentCache["BlogEditUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#&{2}=edit", filePath, blogVar, actionVar);

			ComponentCache["PostUrlTemplate"] = String.Format("{0}?{1}=#PostId#", filePath, postVar);
			ComponentCache["PostPageUrlTemplate"] = String.Format("{0}?{1}=#PostId#&{2}=#PageId###comments", filePath, postVar, pageVar);
			ComponentCache["PostEditUrlTemplate"] = String.Format("{0}?{1}=#PostId#&{2}=edit", filePath, postVar, actionVar);
			ComponentCache["CommentReadUrlTemplate"] = String.Format("{0}?{1}=#PostId#&{2}=#CommentId###comment#CommentId#", filePath, postVar, commentVar);
			ComponentCache["CommentEditUrlTemplate"] = String.Format("{0}?{1}=#PostId#&{2}=#CommentId#&{3}=edit", filePath, postVar, commentVar, actionVar);

			ComponentCache["UserProfileUrlTemplate"] = GetExternalUrlTemplate("UserProfileUrl") ?? String.Format("{0}?{1}=#UserId#", filePath, UrlEncode(UserVariable));
			ComponentCache["UserProfileEditUrlTemplate"] = GetExternalUrlTemplate("UserProfileEditUrl") ?? String.Format("{0}?{1}=#UserId#&{2}=edit", filePath, UrlEncode(UserVariable), actionVar);

			ComponentCache["NewBlogUrlTemplate"] = String.Format("{0}?{1}=newblog", filePath, actionVar);
			ComponentCache["BlogListUrlTemplate"] = String.Format("{0}?{1}=blogs", filePath, actionVar);
			ComponentCache["BlogListPageUrlTemplate"] = String.Format("{0}?{1}=blogs&{2}=#PageId#", filePath, actionVar, pageVar);

			ComponentCache["CategoryBlogListUrlTemplate"] = String.Format("{0}?{1}=#CategoryId#", filePath, UrlEncode(CategoryVariable));
			ComponentCache["CategoryBlogListPageUrlTemplate"] = String.Format("{0}?{1}=#CategoryId#&{2}=#PageId#", filePath, UrlEncode(CategoryVariable), pageVar);

			ComponentCache["NewPostListUrlTemplate"] = String.Format("{0}?{1}=newposts", filePath, actionVar);
			ComponentCache["NewPostListPageUrlTemplate"] = String.Format("{0}?{1}=newposts&{2}=#PageId#", filePath, actionVar, pageVar);
			ComponentCache["PopularPostListUrlTemplate"] = String.Format("{0}?{1}=popular", filePath, actionVar);
			ComponentCache["PopularPostListPageUrlTemplate"] = String.Format("{0}?{1}=popular&{2}=#PageId#", filePath, actionVar, pageVar);
			ComponentCache["DiscussPostListUrlTemplate"] = String.Format("{0}?{1}=discuss", filePath, actionVar);
			ComponentCache["DiscussPostListPageUrlTemplate"] = String.Format("{0}?{1}=discuss&{2}=#PageId#", filePath, actionVar, pageVar);
			if (IsSearch)
			{
                ComponentCache["SearchTagsUrlTemplate"] = String.Format("{0}?{1}=search&{2}=#SearchTags#", filePath, actionVar, tagsVar);
                ComponentCache["SearchTagsPageUrlTemplate"] = String.Format("{0}?{1}=search&{2}=#SearchTags#&{3}=#PageId#", filePath, actionVar, tagsVar, pageVar);
                ComponentCache["SearchBlogTagsUrlTemplate"] = String.Format("{0}?{1}=search&{2}=#SearchTags#&{3}=#BlogSlug#", filePath, actionVar, tagsVar, blogVar);
                ComponentCache["SearchBlogTagsPageUrlTemplate"] = String.Format("{0}?{1}=search&{2}=#SearchTags#&{3}=#BlogSlug#&{4}=#PageId#", filePath, actionVar, tagsVar, blogVar, pageVar);

				if(AllowMetaWeblogApi)
				{
					ComponentCache["BlogMetaWeblogApiUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#&{2}=metaweblog", filePath, blogVar, actionVar);
					ComponentCache["BlogMetaWeblogManifestUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#&{2}=wlwmanifest", filePath, blogVar, actionVar);
				}
			}

            ComponentCache["RssAllPostsTemplate"] = String.Format("{0}?{1}=rss", filePath, actionVar);
			ComponentCache["RssBlogPostsUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#&{2}=rss", filePath, blogVar, actionVar);
            ComponentCache["RssBlogPostCommentsUrlTemplate"] = String.Format("{0}?{1}=#BlogSlug#&{2}=#PostId#&{3}=rss", filePath, blogVar, postVar, actionVar);

			ComponentCache["BlogAbsoluteUrlTemplate"] = ComponentCache["PostListUrlTemplate"];


			if (CategoryId > 0)
				ComponentCache["CategoryId"] = CategoryId.ToString();

			return PrepareNormalPage();
		}
		private string PrepareNormalPage()
		{
			string blogSlug = (Request.QueryString[BlogVariable] ?? String.Empty).ToLowerInvariant();
			int? commentId = ReadQueryInt(CommentVariable);
			int? categoryId = ReadQueryInt(CategoryVariable);
			int? postId = ReadQueryInt(PostVariable);
			int? pageId = ReadQueryInt(PageVariable);
			int? userId = ReadQueryInt(UserVariable);

			string action = (Request.QueryString[ActionVariable] ?? String.Empty).ToLowerInvariant();

			ComponentCache["BlogSlug"] = blogSlug;
			ComponentCache["CategoryId"] = categoryId.ToString();
			ComponentCache["CommentId"] = commentId.ToString();
			ComponentCache["PostId"] = postId.ToString();
			ComponentCache["PageId"] = pageId.ToString();

            if (IsSearch && !string.IsNullOrEmpty(blogSlug))
            {
                ComponentCache["SearchTagsUrlTemplate"] = ComponentCache["SearchBlogTagsUrlTemplate"];
                ComponentCache["SearchTagsPageUrlTemplate"] = ComponentCache["SearchBlogTagsPageUrlTemplate"];
            }

            if (IsSearch && string.Equals("search", action, StringComparison.Ordinal))
            {
                ComponentCache["PostsSortBy"] = "ByDate";
                ComponentCache["SearchTags"] = Request.QueryString[TagsVariable];

                ComponentCache["PagingIndexTemplate"] = ComponentCache["SearchTagsUrlTemplate"];
                ComponentCache["PagingPageTemplate"] = ComponentCache["SearchTagsPageUrlTemplate"];
                return "search";
            }

			if (!String.IsNullOrEmpty(blogSlug))
			{
				switch (action)
				{
					case "newpost":
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostUrlTemplate"];
						return "post-edit";
					case "edit":
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostListUrlTemplate"];
						return "blog-edit";
					case "draft":
						ComponentCache["PagingIndexTemplate"] = ComponentCache["DraftPostListUrlTemplate"];
						ComponentCache["PagingPageTemplate"] = ComponentCache["DraftPostListPageUrlTemplate"];
						ComponentCache["PublishMode"] = "Draft";
						return "blog";
                    case "rss":
                        {
                            ComponentCache["RssFiltrationType"] = CategoryId >0 ? "CategoryId": "BlogSlug";
                            if (postId.HasValue)
                            {
                                ComponentCache["RssStuffType"] = "Comment";
                                ComponentCache["RssItemUrlTemplate"] = ComponentCache["CommentReadUrlTemplate"];
                            }
                            else
                            {
                                ComponentCache["RssStuffType"] = "Post";
                                ComponentCache["RssItemUrlTemplate"] = ComponentCache["PostUrlTemplate"];
                            }
                            return "rss";
                        }
					case "metaweblog":
						if (!AllowMetaWeblogApi)
							goto default;
						return "metaweblog";
					case "wlwmanifest":
						if (!AllowMetaWeblogApi)
							goto default;
						return "wlwmanifest";
					default:
						ComponentCache["PagingIndexTemplate"] = ComponentCache["PostListUrlTemplate"];
						ComponentCache["PagingPageTemplate"] = ComponentCache["PostListPageUrlTemplate"];
						ComponentCache["PublishMode"] = "Published";
						return "blog";
				}
			}
			else if (postId != null)
			{
				switch (action)
				{
					case "edit":
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostUrlTemplate"];
						return "post-edit";
					default:
						return "post";
				}
			}
			else if (commentId != null)
			{
				switch (action)
				{
					//case "edit":
					//	return "comment-edit";
					default:
						return "post";
				}
			}
			else if (userId != null)
			{
				ComponentCache["UserId"] = userId.ToString();
				ComponentCache["ProfileFields"] = GetUserProfileFields();
				string url;
				switch (action)
				{
					case "edit":
						url = GetExternalUrlTemplate("UserProfileEditUrl");
						if (url != null)
							RedirectTemplateUrl(url, ComponentCache);

						ComponentCache["RequiredProfileFields"] = GetRequiredUserProfileFields();
						ComponentCache["PageTitle"] = GetMessageRaw("PageTitle.EditProfile");
						return "profile-edit";
					default:
						url = GetExternalUrlTemplate("UserProfileUrl");
						if (url != null)
							RedirectTemplateUrl(url, ComponentCache);

						ComponentCache["PageTitle"] = GetMessageRaw("PageTitle.ViewProfile");
						return "profile-view";
				}
			}
			else if (categoryId != null)
			{
				ComponentCache["PagingIndexTemplate"] = ComponentCache["CategoryBlogListUrlTemplate"];
				ComponentCache["PagingPageTemplate"] = ComponentCache["CategoryBlogListPageUrlTemplate"];
				return "blogs";
			}

			switch (action)
			{
				case "newblog":
					ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostListUrlTemplate"];
					return "blog-edit";
				case "blogs":
					ComponentCache["PagingIndexTemplate"] = ComponentCache["BlogListUrlTemplate"];
					ComponentCache["PagingPageTemplate"] = ComponentCache["BlogListPageUrlTemplate"];
					return "blogs";
				case "newposts":
					ComponentCache["PagingIndexTemplate"] = ComponentCache["NewPostListUrlTemplate"];
					ComponentCache["PagingPageTemplate"] = ComponentCache["NewPostListPageUrlTemplate"];
                    ComponentCache["PostsSortBy"] = "ByDate";
					Parameters["PeriodDays"] = "0";
					return "posts";
				case "popular":
					ComponentCache["PagingIndexTemplate"] = ComponentCache["PopularPostListUrlTemplate"];
					ComponentCache["PagingPageTemplate"] = ComponentCache["PopularPostListPageUrlTemplate"];
                    ComponentCache["PostsSortBy"] = SortBlogPostsByVotingTotals ? "ByVotingTotals" : "ByViews";
					return "posts";
				case "discuss":
					ComponentCache["PagingIndexTemplate"] = ComponentCache["DiscussPostListUrlTemplate"];
					ComponentCache["PagingPageTemplate"] = ComponentCache["DiscussPostListPageUrlTemplate"];
					ComponentCache["PostsSortBy"] = "ByComments";
					return "posts";
                case "rss":
                    ComponentCache["RssFiltrationType"] = CategoryId > 0 ? "CategoryId": "None";
                    ComponentCache["RssStuffType"] = "Post";
                    ComponentCache["RssItemUrlTemplate"] = ComponentCache["PostUrlTemplate"];
                    return "rss";
			}

			return "index";
		}

		private string GetExternalUrlTemplate(string parameter)
		{
			string val = Parameters.GetString(parameter);
			return !BXStringUtility.IsNullOrTrimEmpty(val) ? val : null;
		}
		private string GetUserProfileFields()
		{
			return BXStringUtility.ListToCsv(new string[] {
				"Image",
				"DisplayName",
				"@" + GetMessageRaw("FieldGroup.PersonalData"),
				"BirthdayDate",
				"Gender",
				"LastName",
				"FirstName",
				"SecondName"
			});
		}
		private static string GetRequiredUserProfileFields()
		{
			return string.Empty;
		}
		private int? ReadQueryInt(string param)
		{
			string[] values = Request.QueryString.GetValues(param);
			if (values == null || values.Length == 0)
				return null;
			int i;
			if (!int.TryParse(values[0], out i))
				return null;
			return i;
		}
		private void Redirect(string url)
		{
			if (url.StartsWith("?"))
			{
				UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
				uri.Query = url.Substring(1);
				url = uri.ToString();
			}
			Response.Redirect(url);
		}

        private static int? maxRequestLength = null;
        private static int MaxRequestLength
        {
            get 
            {
                if (maxRequestLength.HasValue)
                    return maxRequestLength.Value;
                try
                {
                    HttpRuntimeSection httpRuntime = WebConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
                    maxRequestLength = httpRuntime != null ? httpRuntime.MaxRequestLength : -1;
                }
                catch (Exception /*exc*/) 
                {
                    maxRequestLength = -1;
                }
                return maxRequestLength.Value;
            }
        }
	}

	public class BlogTemplate : BXComponentTemplate<BlogComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			string css = Parameters.GetString("ColorCss");
			if (!BXStringUtility.IsNullOrTrimEmpty(css))
			{
				try
				{
					css = BXPath.ToVirtualRelativePath(css);
					if (BXSecureIO.FileExists(css))
						BXPage.RegisterStyle(css);
				}
				catch
				{
				}
			}
		}
	}
}
