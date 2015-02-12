using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Configuration;

using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Modules;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;

namespace Bitrix.Blog.Components
{
	public partial class BlogPersonalComponent : BXComponent
	{
		private static readonly bool IsSearch = Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search");
		private string blogSlug;

		//PROPERTIES
		public string BlogSlug
		{
			get
			{
				return (blogSlug ?? (blogSlug = Parameters.GetString("BlogSlug")));
			}
			set
			{
				Parameters["BlogSlug"] = blogSlug = value;
			}
		}
		public bool DisplayMenu
		{
			get { return Parameters.GetBool("DisplayMenu", true); }
			set { Parameters["DisplayMenu"] = value.ToString(); }
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
		public bool AllowMetaWeblogApi
		{
			get { return IsSearch && Parameters.GetBool("AllowMetaWeblogApi", false); }
			set { Parameters["AllowMetaWeblogApi"] = value.ToString(); }
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
			ComponentCache["CommentsPerPage"] = Parameters.GetString("CommentsPerPage", "30");
			ComponentCache["DisplayMenu"] = DisplayMenu;

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

			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory mainCategory = BXCategory.Main;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;
			BXCategory sefCategory = BXCategory.Sef;
			BXCategory customFieldCategory = BXCategory.CustomField;

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			ParamsDefinition["PagingTemplate"].DefaultValue = "customizable";
			ParamsDefinition["PagingTitle"].DefaultValue = GetMessageRaw("DefaultPagingTitle");
			ParamsDefinition["PagingMaxPages"].DefaultValue = "5";
			ParamsDefinition.Remove("PagingRecordsPerPage");

			ParamsDefinition.Add(BXParametersDefinition.Sef);
			ParamsDefinition.Add(BXParametersDefinition.Search);

			ParamsDefinition["ThemeCssFilePath"] = new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/style.css", mainCategory);
			ParamsDefinition["ColorCssFilePath"] = new BXParamSingleSelectionWithText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", mainCategory);
			ParamsDefinition["BlogSlug"] = new BXParamText(GetMessageRaw("Param.BlogSlug"), "", mainCategory);

			ParamsDefinition["AvailableBlogCustomFields"] = new BXParamMultiSelection(GetMessageRaw("Param.AvailableBlogCustomFields"), "", customFieldCategory);
			ParamsDefinition["AvailablePostCustomFields"] = new BXParamMultiSelection(GetMessageRaw("Param.AvailablePostCustomFields"), "", customFieldCategory);

			// Query string variable names
			ParamsDefinition["CommentVariable"] = new BXParamText(GetMessageRaw("Param.CommentVariable"), "comment", sefCategory);
			ParamsDefinition["PostVariable"] = new BXParamText(GetMessageRaw("Param.PostVariable"), "post", sefCategory);
			ParamsDefinition["ActionVariable"] = new BXParamText(GetMessageRaw("Param.ActionVariable"), "act", sefCategory);
			ParamsDefinition["PageVariable"] = new BXParamText(GetMessageRaw("Param.PageVariable"), "page", sefCategory);
			ParamsDefinition["TagsVariable"] = new BXParamText(GetMessageRaw("Param.TagsVariable"), "tags", sefCategory);

			// Sef templates
			ParamsDefinition["NewPostTemplate"] = new BXParamText(GetMessageRaw("Param.NewPostTemplate"), "/new-post", sefCategory);
			ParamsDefinition["PostsPageTemplate"] = new BXParamText(GetMessageRaw("Param.PostsPageTemplate"), "?page=#PageId#", sefCategory);
			ParamsDefinition["DraftsTemplate"] = new BXParamText(GetMessageRaw("Param.DraftsTemplate"), "/drafts/", sefCategory);
			ParamsDefinition["DraftsPageTemplate"] = new BXParamText(GetMessageRaw("Param.DraftsPageTemplate"), "/drafts/?page=#PageId#", sefCategory);
			ParamsDefinition["PostTemplate"] = new BXParamText(GetMessageRaw("Param.PostTemplate"), "/#PostId#/", sefCategory);
			ParamsDefinition["PostPageTemplate"] = new BXParamText(GetMessageRaw("Param.PostPageTemplate"), "/#PostId#/?page=#PageId###comments", sefCategory);
			ParamsDefinition["PostEditTemplate"] = new BXParamText(GetMessageRaw("Param.PostEditTemplate"), "/#PostId#/edit", sefCategory);
			ParamsDefinition["CommentReadTemplate"] = new BXParamText(GetMessageRaw("Param.CommentReadTemplate"), "/#PostId#/#CommentId#/##comment#CommentId#", sefCategory);
			ParamsDefinition["CommentEditTemplate"] = new BXParamText(GetMessageRaw("Param.CommentEditTemplate"), "/#PostId#/#CommentId#/edit", sefCategory);
			ParamsDefinition["BlogEditTemplate"] = new BXParamText(GetMessageRaw("Param.BlogEditTemplate"), "/settings", sefCategory);

			if (IsSearch)
			{
				ParamsDefinition["SearchTagsTemplate"] = new BXParamText(GetMessageRaw("Param.SearchTagsTemplate"), "/tags/?#SearchTags#", sefCategory);
				ParamsDefinition["SearchTagsPageTemplate"] = new BXParamText(GetMessageRaw("Param.SearchTagsPageTemplate"), "/tags/?#SearchTags#&page=#PageId#", sefCategory);

				ParamsDefinition["MetaWeblogApiTemplate"] = new BXParamText(GetMessageRaw("Param.MetaWeblogApiTemplate"), "/metaweblog", sefCategory);
				ParamsDefinition["MetaWeblogManifestTemplate"] = new BXParamText(GetMessageRaw("Param.MetaWeblogManifestTemplate"), "/wlwmanifest.xml", sefCategory);
			}

			ParamsDefinition["RssPostsTemplate"] = new BXParamText(GetMessageRaw("Param.RssPostsTemplate"), "/rss", sefCategory);
			ParamsDefinition["RssPostCommentsTemplate"] = new BXParamText(GetMessageRaw("Param.RssPostCommentsTemplate"), "/#PostId#/rss", sefCategory);

			//Additional options
			ParamsDefinition["DisplayMenu"] = new BXParamYesNo(GetMessageRaw("Param.DisplayMenu"), true, additionalSettingsCategory);
			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);
			ParamsDefinition["DontSetPostsMasterTitle"] = new BXParamYesNo(GetMessageRaw("Param.DontSetPostsMasterTitle"), false, additionalSettingsCategory);
			
			
			ParamsDefinition["MaxWordLength"] = new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory);
			ParamsDefinition["PostsPerPage"] = new BXParamText(GetMessageRaw("Param.PostsPerPage"), "10", additionalSettingsCategory);
			ParamsDefinition["CommentsPerPage"] = new BXParamText(GetMessageRaw("Param.CommentsPerPage"), "30", additionalSettingsCategory);

			if (IsSearch)
				ParamsDefinition["AllowMetaWeblogApi"] = new BXParamYesNo(GetMessageRaw("Param.AllowMetaWeblogApi"), false, additionalSettingsCategory);

			BXCategory guestsCategory = new BXCategory(GetMessageRaw("Category.Guests"), "guest_parameters", mainCategory.Sort + 100);
			ParamsDefinition["GuestEmail"] = new BXParamSingleSelection(GetMessageRaw("Param.GuestEmail"), "required", guestsCategory);
			ParamsDefinition["GuestCaptcha"] = new BXParamYesNo(GetMessageRaw("Param.GuestCaptcha"), true, guestsCategory);

			ParamsDefinition["UserProfileUrl"] = new BXParamText(GetMessageRaw("Param.UserProfileUrl"), "", urlCategory);

			//File limitations
			int maxRequestLength = MaxRequestLength;
			ParamsDefinition.Add("MaximalFileSizeInKBytes", new BXParamText(maxRequestLength <= 0 ? GetMessageRaw("Param.MaximalFileSizeInKBytes") : string.Format(GetMessageRaw("Param.MaximalFileSizeInKBytesAdd"), maxRequestLength.ToString()), "1024", additionalSettingsCategory));
			ParamsDefinition.Add("MaximalAllFilesSizeInKBytes", new BXParamText(GetMessageRaw("Param.MaximalAllFilesSizeInKBytes"), "8192", additionalSettingsCategory));

			//Image limitations
			ParamsDefinition.Add("MaximalImageWidthInPixels", new BXParamText(GetMessageRaw("Param.MaximalImageWidthInPixels"), "0", additionalSettingsCategory));
			ParamsDefinition.Add("MaximalImageHeightInPixels", new BXParamText(GetMessageRaw("Param.MaximalImageHeightInPixels"), "0", additionalSettingsCategory));
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
				"CommentVariable", "PostVariable", 
				"ActionVariable",  "PageVariable", "TagsVariable"
			})
				ParamsDefinition[id].ClientSideAction = new ParamClientSideActionGroupViewMember(csaId, id, new string[] { "NonSef" });

			List<string> ids = new List<string>(new string[] {
				"NewPostTemplate", "PostsPageTemplate", "DraftsTemplate", "DraftsPageTemplate",
				"PostTemplate", "PostPageTemplate", "PostEditTemplate",
				"CommentReadTemplate", "CommentEditTemplate",
				"BlogEditTemplate", 
				"RssPostsTemplate", "RssPostCommentsTemplate"
			});
			if (IsSearch)
			{
				ids.Add("SearchTagsTemplate");
				ids.Add("SearchTagsPageTemplate");
				ids.Add("MetaWeblogApiTemplate");
				ids.Add("MetaWeblogManifestTemplate");
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
				ParamsDefinition["ColorCssFilePath"].Values = values;
			}

			ParamsDefinition["GuestEmail"].Values = new List<BXParamValue>(new BXParamValue[] {
				new BXParamValue(GetMessageRaw("GuestEmail.Hide"), ""),
				new BXParamValue(GetMessageRaw("GuestEmail.Show"), "visible"),
				new BXParamValue(GetMessageRaw("GuestEmail.Require"), "required")
			});



			//AvailablePostCustomFields
			List<BXParamValue> availablePostCustomFields = ParamsDefinition["AvailablePostCustomFields"].Values;
			if (availablePostCustomFields.Count > 0)
				availablePostCustomFields.Clear();

			BXCustomFieldCollection postFields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.PostCustomFieldEntityId);
			foreach (BXCustomField field in postFields)
				availablePostCustomFields.Add(new BXParamValue(field.TextEncoder.Decode(field.EditFormLabel), field.Name));


			//AvailableBlogCustomFields
			List<BXParamValue> availableBlogCustomFields = ParamsDefinition["AvailableBlogCustomFields"].Values;
			if (availableBlogCustomFields.Count > 0)
				availableBlogCustomFields.Clear();

			BXCustomFieldCollection blogFields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.BlogCustomFieldEntityId);
			foreach (BXCustomField field in blogFields)
			{
				BXParamValue param = new BXParamValue(field.TextEncoder.Decode(field.EditFormLabel), field.Name);
				availableBlogCustomFields.Add(param);
			}

		}
		public override void ProcessMessage(BXCommand cmd, bool executionContext, string executionVirtualPath)
		{
			if (cmd.Action == "Bitrix.Search.ProvideUrl")
			{
				if (cmd.Parameters.GetString("moduleId") != "blog" || cmd.Parameters.GetString("param1") != BlogSlug)
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

			ComponentCache["PostsUrlTemplate"] = CombineLink(sefFolder, "");
			ComponentCache["PostsPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostsPageTemplate"));
			ComponentCache["NewPostUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("NewPostTemplate"));
			ComponentCache["DraftsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("DraftsTemplate"));
			ComponentCache["DraftsPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("DraftsPageTemplate"));
			ComponentCache["PostUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostTemplate"));
			ComponentCache["PostPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostPageTemplate"));
			ComponentCache["PostEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostEditTemplate"));
			ComponentCache["CommentReadUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("CommentReadTemplate"));
			ComponentCache["CommentEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("CommentEditTemplate"));
			ComponentCache["UserProfileUrlTemplate"] = GetExternalUrlTemplate("UserProfileUrl");
			ComponentCache["BlogEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("BlogEditTemplate"));

			if (IsSearch)
			{
				ComponentCache["SearchTagsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("SearchTagsTemplate"));
				ComponentCache["SearchTagsPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("SearchTagsPageTemplate"));

				if (AllowMetaWeblogApi)
				{
					ComponentCache["MetaWeblogApiUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("MetaWeblogApiTemplate"));
					ComponentCache["MetaWeblogManifestUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("MetaWeblogManifestTemplate"));
				}
			}

			ComponentCache["RssPostsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("RssPostsTemplate"));
			ComponentCache["RssPostCommentsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("RssPostCommentsTemplate"));

			return PrepareSefPage();
		}
		private string PrepareSefPage()
		{
			BXParamsBag<string> sefMap = new BXParamsBag<string>();

			sefMap.Add("edit.new", Parameters.GetString("NewPostTemplate"));
			sefMap.Add("edit.edit", Parameters.GetString("PostEditTemplate"));
			sefMap.Add("posts.page", Parameters.GetString("PostsPageTemplate"));
			sefMap.Add("posts.draft", Parameters.GetString("DraftsTemplate"));
			sefMap.Add("posts.draft.page", Parameters.GetString("DraftsPageTemplate"));
			sefMap.Add("post", Parameters.GetString("PostTemplate"));
			sefMap.Add("post.page", Parameters.GetString("PostPageTemplate"));
			sefMap.Add("post.comment", Parameters.GetString("CommentReadTemplate"));
			//sefMap.Add("comment.edit", Parameters.GetString("CommentEditTemplate"));
			sefMap.Add("settings", Parameters.GetString("BlogEditTemplate"));

			if (IsSearch)
			{
				sefMap.Add("tags", Parameters.GetString("SearchTagsTemplate"));
				sefMap.Add("tags.page", Parameters.GetString("SearchTagsPageTemplate"));

				if (AllowMetaWeblogApi)
				{
					sefMap.Add("metaweblog", Parameters.GetString("MetaWeblogApiTemplate"));
					sefMap.Add("wlwmanifest", Parameters.GetString("MetaWeblogManifestTemplate"));
				}
			}

			sefMap.Add("rss.posts", Parameters.GetString("RssPostsTemplate"));
			sefMap.Add("rss.comments", Parameters.GetString("RssPostCommentsTemplate"));

			string code = BXSefUrlUtility.MapVariable(Parameters.GetString("SEFFolder", ""), sefMap, ComponentCache, "posts", null, null);
			ComponentCache["BlogSlug"] = BlogSlug;

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
				case "posts":
					switch (action)
					{
						case "draft":
						case "draft.page":
							ComponentCache["PagingIndexTemplate"] = ComponentCache["DraftsUrlTemplate"];
							ComponentCache["PagingPageTemplate"] = ComponentCache["DraftsPageUrlTemplate"];
							ComponentCache["PublishMode"] = "Draft";
							break;
						default:
							ComponentCache["PagingIndexTemplate"] = ComponentCache["PostsUrlTemplate"];
							ComponentCache["PagingPageTemplate"] = ComponentCache["PostsPageUrlTemplate"];
							ComponentCache["PublishMode"] = "Published";
							break;
					}
					break;

				case "edit":
					ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostUrlTemplate"];
					break;
				case "settings":
					ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostsUrlTemplate"];
					break;
				case "rss":
					ComponentCache["RssFiltrationType"] = "BlogSlug";
					switch (action)
					{
						case "comments":
							ComponentCache["RssStuffType"] = "Comment";
							ComponentCache["RssFeedUrlTemplate"] = ComponentCache["PostUrlTemplate"];
							ComponentCache["RssItemUrlTemplate"] = ComponentCache["CommentReadUrlTemplate"];
							break;
						default:
							ComponentCache["RssStuffType"] = "Post";
							ComponentCache["RssFeedUrlTemplate"] = ComponentCache["PostsUrlTemplate"];
							ComponentCache["RssItemUrlTemplate"] = ComponentCache["PostUrlTemplate"];
							break;
					}
					break;
				case "tags":
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
			string postVar = UrlEncode(PostVariable);
			string commentVar = UrlEncode(CommentVariable);
			string actionVar = UrlEncode(ActionVariable);
			string pageVar = UrlEncode(PageVariable);
			string tagsVar = UrlEncode(TagsVariable);

			ComponentCache["PostsUrlTemplate"] = filePath;
			ComponentCache["PostsPageUrlTemplate"] = String.Format("{0}?{1}=#PageId#", filePath, pageVar);
			ComponentCache["NewPostUrlTemplate"] = String.Format("{0}?{1}=new", filePath, actionVar);
			ComponentCache["DraftsUrlTemplate"] = String.Format("{0}?{1}=drafts", filePath, actionVar);
			ComponentCache["DraftsPageUrlTemplate"] = String.Format("{0}?{1}=drafts&{2}=#PageId#", filePath, actionVar, pageVar);
			ComponentCache["BlogEditUrlTemplate"] = String.Format("{0}?{1}=settings", filePath, actionVar);
			ComponentCache["UserProfileUrlTemplate"] = GetExternalUrlTemplate("UserProfileUrl");
			ComponentCache["PostUrlTemplate"] = String.Format("{0}?{1}=#PostId#", filePath, postVar);
			ComponentCache["PostPageUrlTemplate"] = String.Format("{0}?{1}=#PostId#&{2}=#PageId###comments", filePath, postVar, pageVar);
			ComponentCache["PostEditUrlTemplate"] = String.Format("{0}?{1}=#PostId#&{2}=edit", filePath, postVar, actionVar);
			ComponentCache["CommentReadUrlTemplate"] = String.Format("{0}?{1}=#PostId#&{2}=#CommentId###comment#CommentId#", filePath, postVar, commentVar);
			ComponentCache["CommentEditUrlTemplate"] = String.Format("{0}?{1}=#PostId#&{2}=#CommentId#&{3}=edit", filePath, postVar, commentVar, actionVar);

			if (IsSearch)
			{
				ComponentCache["SearchTagsUrlTemplate"] = String.Format("{0}?{1}=#SearchTags#", filePath, tagsVar);
				ComponentCache["SearchTagsPageUrlTemplate"] = String.Format("{0}?{1}=#SearchTags#&{2}=#PageId#", filePath, tagsVar, pageVar);

				if (AllowMetaWeblogApi)
				{
					ComponentCache["MetaWeblogApiUrlTemplate"] = String.Format("{0}?{1}=metaweblog", filePath, actionVar);
					ComponentCache["MetaWeblogManifestUrlTemplate"] = String.Format("{0}?{1}=wlwmanifest", filePath, actionVar);
				}
			}

			ComponentCache["RssPostsUrlTemplate"] = String.Format("{0}?{1}=rss", filePath, actionVar);
			ComponentCache["RssPostCommentsUrlTemplate"] = String.Format("{0}?{1}=#PostId#&{2}=rss", filePath, postVar, actionVar);

			return PrepareNormalPage();
		}
		private string PrepareNormalPage()
		{
			int? commentId = ReadQueryInt(CommentVariable);
			int? postId = ReadQueryInt(PostVariable);
			int? pageId = ReadQueryInt(PageVariable);

			string action = (Request.QueryString[ActionVariable] ?? String.Empty).ToLowerInvariant();

			ComponentCache["BlogSlug"] = BlogSlug;
			ComponentCache["CommentId"] = commentId.ToString();
			ComponentCache["PostId"] = postId.ToString();
			ComponentCache["PageId"] = pageId.ToString();

			if (IsSearch && Request.QueryString[TagsVariable] != null)
			{
				ComponentCache["PostsSortBy"] = "ByDate";
				ComponentCache["SearchTags"] = Request.QueryString[TagsVariable];

				ComponentCache["PagingIndexTemplate"] = ComponentCache["SearchTagsUrlTemplate"];
				ComponentCache["PagingPageTemplate"] = ComponentCache["SearchTagsPageUrlTemplate"];
				return "tags";
			}



			if (commentId != null)
			{
				switch (action)
				{
					//case "edit":
					//	return "comment-edit";
					default:
						return "post";
				}
			}

			if (postId != null)
			{
				switch (action)
				{
					case "edit":
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostUrlTemplate"];
						return "edit";
					case "rss":
						ComponentCache["RssFiltrationType"] = "BlogSlug";
						ComponentCache["RssStuffType"] = "Comment";
						ComponentCache["RssFeedUrlTemplate"] = ComponentCache["PostUrlTemplate"];
						ComponentCache["RssItemUrlTemplate"] = ComponentCache["CommentReadUrlTemplate"];
						return "rss";
					default:
						return "post";
				}
			}


			switch (action)
			{
				case "new":
					ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostUrlTemplate"];
					return "edit";
				case "settings":
					ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostsUrlTemplate"];
					return "settings";
				case "drafts":
					ComponentCache["PagingIndexTemplate"] = ComponentCache["DraftsUrlTemplate"];
					ComponentCache["PagingPageTemplate"] = ComponentCache["DraftsPageUrlTemplate"];
					ComponentCache["PublishMode"] = "Draft";
					return "posts";
				case "rss":
					ComponentCache["RssFiltrationType"] = "BlogSlug";
					ComponentCache["RssStuffType"] = "Post";
					ComponentCache["RssFeedUrlTemplate"] = ComponentCache["PostsUrlTemplate"];
					ComponentCache["RssItemUrlTemplate"] = ComponentCache["PostUrlTemplate"];
					return "rss";
				case "metaweblog":
					if (!AllowMetaWeblogApi)
						goto default;
					return "metaweblog";
				case "wlwmanifest":
					if (!AllowMetaWeblogApi)
						goto default;
					return "wlwmanifest";
				default:
					ComponentCache["PagingIndexTemplate"] = ComponentCache["PostsUrlTemplate"];
					ComponentCache["PagingPageTemplate"] = ComponentCache["PostsPageUrlTemplate"];
					ComponentCache["PublishMode"] = "Published";
					return "posts";
			}
		}

		private string GetExternalUrlTemplate(string parameter)
		{
			string val = Parameters.GetString(parameter);
			return !BXStringUtility.IsNullOrTrimEmpty(val) ? val : null;
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

	public class BlogPersonalTemplate : BXComponentTemplate<BlogPersonalComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			string css = Parameters.GetString("ThemeCssFilePath");
			if (!BXStringUtility.IsNullOrTrimEmpty(css))
			{
				try
				{
					BXPage.RegisterStyle(BXPath.ToVirtualRelativePath(css));
				}
				catch
				{
				}
			}

			css = Parameters.GetString("ColorCssFilePath");
			if (!BXStringUtility.IsNullOrTrimEmpty(css))
			{
				try
				{
					BXPage.RegisterStyle(BXPath.ToVirtualRelativePath(css));
				}
				catch
				{
				}
			}
		}
	}
}
