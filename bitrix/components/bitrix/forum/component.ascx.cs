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
using System.Text;
using Bitrix.Security;

namespace Bitrix.Forum.Components
{
	public partial class ForumComponent : BXComponent
	{
		static readonly string[] IncludeAreaExts = new string[] { ".ascx", ".html", ".txt", ".htm" };
		private static readonly bool IsSearch = Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search");
		private List<int> availableForums;
		private bool? enableSearch;

		//PROPERTIES
		private string ForumVariable
		{
			get
			{
				return Parameters.GetString("ForumVariable", "forum");
			}
			set
			{
				Parameters["ForumVariable"] = value;
			}
		}
		private string TopicVariable
		{
			get
			{
				return Parameters.GetString("TopicVariable", "topic");
			}
			set
			{
				Parameters["TopicVariable"] = value;
			}
		}
		private string TopicsVariable
		{
			get
			{
				return Parameters.GetString("TopicsVariable", "");
			}
			set
			{
				Parameters["TopicsVariable"] = value;
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
		public string SefFolder
		{
			get { return Parameters.Get<string>("SEFFolder", String.Empty); }
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
		public List<int> AvailableForums
		{
			get
			{
				return availableForums ?? (availableForums = Parameters.GetListInt("AvailableForums"));
			}
		}

		public bool EnableRss
		{
			get { return Parameters.Get<bool>("EnableRss", true); }
		}
		public bool ShowNavigation
		{
			get
			{
				return Parameters.GetBool("ShowNavigation");
			}
			set
			{
				Parameters["ShowNavigation"] = value.ToString();
			}
		}
		public bool ShowMenu
		{
			get
			{
				return Parameters.GetBool("ShowMenu");
			}
			set
			{
				Parameters["ShowMenu"] = value.ToString();
			}
		}
		public int MaxWordLength
		{
			get
			{
				return Parameters.GetInt("MaxWordLength");
			}
			set
			{
				Parameters["MaxWordLength"] = value.ToString();
			}
		}
		public bool EnableSearch
		{
			get
			{
				return (enableSearch ?? (enableSearch = (IsSearch && Parameters.GetBool("EnableSearch", true)))).Value;
			}
			set
			{
				enableSearch = IsSearch && value;
				Parameters["EnableSearch"] = value.ToString();
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
		public override void ProcessMessage(BXCommand cmd, bool executionContext, string executionVirtualPath)
		{
			if (String.Equals(cmd.Action, "Bitrix.Search.ProvideUrl", StringComparison.OrdinalIgnoreCase))
				ProvideSearchUrl(cmd, executionContext, executionVirtualPath);
			else if (String.Equals(cmd.Action, "Bitrix.Main.GeneratePublicMenu", StringComparison.OrdinalIgnoreCase))
				GeneratePublicMenu(cmd, executionContext, executionVirtualPath);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Parameters["PagingBoundedBorderSize"] = "1";
			ComponentCache["ShowNavigation"] = ShowNavigation;
			ComponentCache["ShowMenu"] = ShowMenu;
			ComponentCache["MaxWordLength"] = MaxWordLength;

			string page = EnableSef ? PrepareSefMode() : PrepareNormalMode();

			IncludeComponentTemplate(page);
		}
		protected void Page_PreRender(object sender, EventArgs e)
		{
			if (!EnableRss)
				return;
			object rssLinkUrlObj;
			string rssLinkUrl;
			if (ComponentCache.TryGetValue("RssLinkUrl", out rssLinkUrlObj) && !string.IsNullOrEmpty((rssLinkUrl = rssLinkUrlObj as string)))
			{
				object titleObj;
				ComponentCache.TryGetValue("RssLinkTitle", out titleObj);
				string title = titleObj as string;
				if (!string.IsNullOrEmpty(title))
					BXPage.RegisterLink("alternate", "application/rss+xml", rssLinkUrl, new KeyValuePair<string, string>("title", title));
				else
					BXPage.RegisterLink("alternate", "application/rss+xml", rssLinkUrl);
			}
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = new BXComponentGroup("forum", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);

            BXCategory urlCategory = BXCategory.UrlSettings,
                mainCategory = BXCategory.Main,
                additionalSettingsCategory = BXCategory.AdditionalSettings,
                sefCategory = BXCategory.Sef;
                //votingCategory = new BXCategory(GetMessage("Category.Voting"), "Voting", 220);

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			ParamsDefinition["PagingTemplate"].DefaultValue = "customizable";
			ParamsDefinition["PagingTitle"].DefaultValue = GetMessageRaw("DefaultPagingTitle");
			ParamsDefinition["PagingRecordsPerPage"].DefaultValue = "25";
			ParamsDefinition["PagingShowOne"].DefaultValue = "true";
			ParamsDefinition["PagingMaxPages"].DefaultValue = "3";

			ParamsDefinition.Add(BXParametersDefinition.Sef);
			if (IsSearch)
			{
				ParamsDefinition.Add(BXParametersDefinition.Search);
				ParamsDefinition["EnableSearch"] = new BXParamYesNo(GetMessageRaw("Param.EnableSearch"), true, BXCategory.SearchSettings, new ParamClientSideActionGroupViewSwitch(ClientID, "EnableSearch", "EnableSearch", "DisableSearch"));
			}
			if (!String.IsNullOrEmpty(DesignerSite))
				ParamsDefinition.Add(BXParametersDefinition.Menu(DesignerSite));

			ParamsDefinition["ColorCss"] = new BXParamSingleSelectionWithText(GetMessageRaw("Param.ColorCss"), "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css", mainCategory);
			ParamsDefinition["AvailableForums"] = new BXParamMultiSelection(GetMessageRaw("Param.AvailableForums"), "", mainCategory);

			// Query string variable names
			ParamsDefinition["ForumVariable"] = new BXParamText(GetMessageRaw("Param.ForumVariable"), "forum", sefCategory);
			ParamsDefinition["TopicVariable"] = new BXParamText(GetMessageRaw("Param.TopicVariable"), "topic", sefCategory);
			ParamsDefinition["TopicsVariable"] = new BXParamText(GetMessageRaw("Param.TopicsVariable"), "topics", sefCategory);
			ParamsDefinition["PostVariable"] = new BXParamText(GetMessageRaw("Param.PostVariable"), "post", sefCategory);
			ParamsDefinition["ActionVariable"] = new BXParamText(GetMessageRaw("Param.ActionVariable"), "act", sefCategory);
			ParamsDefinition["PageVariable"] = new BXParamText(GetMessageRaw("Param.PageVariable"), "page", sefCategory);
			ParamsDefinition["UserVariable"] = new BXParamText(GetMessageRaw("Param.UserVariable"), "user", sefCategory);

			// Sef templates
			ParamsDefinition["NewTopicTemplate"] = new BXParamText(GetMessageRaw("Param.NewTopicTemplate"), "/#i:ForumId#/new/", sefCategory);
			ParamsDefinition["MoveTopicsTemplate"] = new BXParamText(GetMessageRaw("Param.MoveTopicsTemplate"), "/move/?topics=#MoveTopicsIds#", sefCategory);
			ParamsDefinition["TopicListTemplate"] = new BXParamText(GetMessageRaw("Param.TopicListTemplate"), "/#i:ForumId#/", sefCategory);
			ParamsDefinition["TopicListPageTemplate"] = new BXParamText(GetMessageRaw("Param.TopicListPageTemplate"), "/#i:ForumId#/?page=#PageId#", sefCategory);
			ParamsDefinition["TopicTemplate"] = new BXParamText(GetMessageRaw("Param.TopicTemplate"), "/#i:ForumId#/#i:TopicId#/", sefCategory);
			ParamsDefinition["TopicPageTemplate"] = new BXParamText(GetMessageRaw("Param.TopicPageTemplate"), "/#i:ForumId#/#i:TopicId#/?page=#PageId#", sefCategory);
			ParamsDefinition["TopicReplyTemplate"] = new BXParamText(GetMessageRaw("Param.TopicReplyTemplate"), "/#i:ForumId#/#i:TopicId#/reply/", sefCategory);
			ParamsDefinition["TopicMoveTemplate"] = new BXParamText(GetMessageRaw("Param.TopicMoveTemplate"), "/#i:ForumId#/#i:TopicId#/move/", sefCategory);
			ParamsDefinition["TopicEditTemplate"] = new BXParamText(GetMessageRaw("Param.TopicEditTemplate"), "/#i:ForumId#/#i:TopicId#/edit/", sefCategory);
			ParamsDefinition["PostReadTemplate"] = new BXParamText(GetMessageRaw("Param.PostReadTemplate"), "/#i:ForumId#/#i:TopicId#/#i:PostId#/##post#PostId#", sefCategory);
			ParamsDefinition["PostQuoteTemplate"] = new BXParamText(GetMessageRaw("Param.PostQuoteTemplate"), "/#i:ForumId#/#i:TopicId#/#i:PostId#/quote/", sefCategory);
			ParamsDefinition["PostEditTemplate"] = new BXParamText(GetMessageRaw("Param.PostEditTemplate"), "/#i:ForumId#/#i:TopicId#/#i:PostId#/edit/", sefCategory);
			ParamsDefinition["UserProfileTemplate"] = new BXParamText(GetMessageRaw("Param.UserProfileTemplate"), "/users/#i:UserId#/", sefCategory);
			ParamsDefinition["UserProfileEditTemplate"] = new BXParamText(GetMessageRaw("Param.UserProfileEditTemplate"), "/users/#i:UserId#/edit/", sefCategory);
			ParamsDefinition["UserSubscriptionsUrlTemplate"] = new BXParamText(GetMessageRaw("Param.UserSubscriptionsTemplate"), "/subscriptions/", sefCategory);
			ParamsDefinition["UserSubscriptionsPageUrlTemplate"] = new BXParamText(GetMessageRaw("Param.UserSubscriptionsPageTemplate"), "/subscriptions/?page=#PageId#", sefCategory);

			ParamsDefinition["UserPostsTemplate"] = new BXParamText(GetMessageRaw("Param.UserPostsTemplate"), "/users/#i:UserId#/posts/", sefCategory);
			ParamsDefinition["UserPostsPageTemplate"] = new BXParamText(GetMessageRaw("Param.UserPostsPageTemplate"), "/users/#UserId#/posts/?page=#PageId#", sefCategory);
			ParamsDefinition["ForumRulesTemplate"] = new BXParamText(GetMessageRaw("Param.ForumRulesTemplate"), "/rules/", sefCategory);
			ParamsDefinition["ForumHelpTemplate"] = new BXParamText(GetMessageRaw("Param.ForumHelpTemplate"), "/help/", sefCategory);
			ParamsDefinition["ActiveTopicsUrlTemplate"] = new BXParamText(GetMessageRaw("Param.ActiveTopicsUrlTemplate"), "/active/", sefCategory);
			ParamsDefinition["ActiveTopicsPageUrlTemplate"] = new BXParamText(GetMessageRaw("Param.ActiveTopicsPageUrlTemplate"), "/active/?page=#PageId#", sefCategory);
			ParamsDefinition["UnAnsweredTopicsUrlTemplate"] = new BXParamText(GetMessageRaw("Param.UnAnsweredTopicsUrlTemplate"), "/unanswered/", sefCategory);
			ParamsDefinition["UnAnsweredTopicsPageUrlTemplate"] = new BXParamText(GetMessageRaw("Param.UnAnsweredTopicsPageUrlTemplate"), "/unanswered/?page=#PageId#", sefCategory);
			if (IsSearch)
			{
				ParamsDefinition["SearchUrlTemplate"] = new BXParamText(GetMessageRaw("Param.SearchUrlTemplate"), "/search/", sefCategory);
				ParamsDefinition["SearchPageUrlTemplate"] = new BXParamText(GetMessageRaw("Param.SearchPageUrlTemplate"), "/search/?page=#PageId#", sefCategory);
			}

			//Sef templates for rss
			ParamsDefinition["RssTopicsAllTemplate"] = new BXParamText(GetMessageRaw("Param.RssTopicsAllTemplate"), "/rss/topics/", sefCategory);
			ParamsDefinition["RssTopicsFromForumTemplate"] = new BXParamText(GetMessageRaw("Param.RssTopicsFromForumTemplate"), "/#i:ForumId#/rss/topics/", sefCategory);
			ParamsDefinition["RssPostsFromTopicTemplate"] = new BXParamText(GetMessageRaw("Param.RssPostsFromTopicTemplate"), "/#i:ForumId#/#TopicId#/rss/", sefCategory);

			if (IsSearch)
			{
				// for search component
				ParamsDefinition["SearchStringVariable"] = new BXParamText(GetMessageRaw("Param.SearchStringVariable"), "query", sefCategory, new ParamClientSideActionGroupViewMember(ClientID, "SearchStringVariable", new string[] {"EnableSearch"}));
				ParamsDefinition["DateIntervalVariable"] = new BXParamText(GetMessageRaw("Param.DateIntervalVariable"), "period", sefCategory, new ParamClientSideActionGroupViewMember(ClientID, "DateIntervalVariable", new string[] {"EnableSearch"}));
				ParamsDefinition["ForumsVariable"] = new BXParamText(GetMessageRaw("Param.ForumsVariable"), "forums", sefCategory, new ParamClientSideActionGroupViewMember(ClientID, "ForumsVariable", new string[] {"EnableSearch"}));
				ParamsDefinition["SortingVariable"] = new BXParamText(GetMessageRaw("Param.SortingVariable"), "sort", sefCategory, new ParamClientSideActionGroupViewMember(ClientID, "SortingVariable", new string[] {"EnableSearch"}));
			}

			//Info pages
			ParamsDefinition["ForumRulesUrl"] = new BXParamText(GetMessageRaw("Param.ForumRulesUrl"), "", urlCategory);
			ParamsDefinition["ForumHelpUrl"] = new BXParamText(GetMessageRaw("Param.ForumHelpUrl"), "", urlCategory);
			ParamsDefinition["UserProfileUrl"] = new BXParamText(GetMessageRaw("Param.UserProfileUrl"), "", urlCategory);
			ParamsDefinition["UserProfileEditUrl"] = new BXParamText(GetMessageRaw("Param.UserProfileEditUrl"), "", urlCategory);

			//Additional options
			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);
			ParamsDefinition["MaxWordLength"] = new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory);
			ParamsDefinition["ForumListTitle"] = new BXParamText(GetMessageRaw("Param.ForumListTitle"), GetMessageRaw("Param.ForumListTitle.DefaultValue"), additionalSettingsCategory);
			ParamsDefinition["ShowNavigation"] = new BXParamYesNo(GetMessageRaw("Param.ShowNavigation"), true, additionalSettingsCategory);

			ParamsDefinition["ShowMenu"] = new BXParamYesNo(GetMessageRaw("Param.ShowMenu"), true, additionalSettingsCategory, new ParamClientSideActionGroupViewSwitch(ClientID, "ShowMenu", "MenuOn", String.Empty));
			ParamsDefinition["GuestMenuTextTemplate"] = new BXParamMultilineText(GetMessageRaw("Param.GuestMenuTextTemplate"), GetMessageRaw("Param.GuestMenuTextTemplate.Default"), additionalSettingsCategory, new ParamClientSideActionGroupViewMember(ClientID, "GuestMenuTextTemplate", new string[] { "MenuOn" }));
			ParamsDefinition["UserMenuTextTemplate"] = new BXParamMultilineText(GetMessageRaw("Param.UserMenuTextTemplate"), GetMessageRaw("Param.UserMenuTextTemplate.Default"), additionalSettingsCategory, new ParamClientSideActionGroupViewMember(ClientID, "UserMenuTextTemplate", new string[] { "MenuOn" }));

			BXCategory guestsCategory = new BXCategory(GetMessageRaw("Category.Guests"), "guest_parameters", mainCategory.Sort + 100);
			ParamsDefinition["GuestEmail"] = new BXParamSingleSelection(GetMessageRaw("Param.GuestEmail"), "required", guestsCategory);
			ParamsDefinition["GuestCaptcha"] = new BXParamYesNo(GetMessageRaw("Param.GuestCaptcha"), true, guestsCategory);

            //ParamsDefinition.Add("EnableVotingForTopic", new BXParamYesNo(GetMessageRaw("Param.EnableVotingForTopic"), false, votingCategory));
            //ParamsDefinition.Add("EnableVotingForPost", new BXParamYesNo(GetMessageRaw("Param.EnableVotingForPost"), false, votingCategory));
            //ParamsDefinition.Add("RolesAuthorizedToVote", new BXParamMultiSelection(GetMessageRaw("Param.RolesAuthorizedToVote"), "User", votingCategory));
            //ParamsDefinition.Add("SortTopicsByVotingTotals", new BXParamYesNo(GetMessageRaw("Param.SortTopicsByVotingTotals"), false, votingCategory));

			//RSS
			ParamsDefinition.Add(BXParametersDefinition.Rss);
		}
		protected override void LoadComponentDefinition()
		{
			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			BXForumCollection forums = BXForum.GetList(
				new BXFilter(
					new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
				),
				new BXOrderBy(
					new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)
				),
				new BXSelect(BXForum.Fields.Id, BXForum.Fields.Name),
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			ParamsDefinition["AvailableForums"].Values = forums.ConvertAll<BXParamValue>(delegate(BXForum input)
			{
				return new BXParamValue(input.Name, input.Id.ToString());
			});

			string csaId = ClientID;

			ParamsDefinition["EnableSEF"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(csaId, "EnableSEF", "Sef", "NonSef");
			ParamsDefinition["SEFFolder"].ClientSideAction = new ParamClientSideActionGroupViewMember(csaId, "SEFFolder", new string[] { "Sef" });

			foreach (string id in new string[] {
				"ForumVariable", "TopicVariable", "TopicsVariable", "PostVariable", 
				"ActionVariable",  "PageVariable", "UserVariable"
			})
				ParamsDefinition[id].ClientSideAction = new ParamClientSideActionGroupViewMember(csaId, id, new string[] { "NonSef" });

			string[] ids = new string[] {
                "NewTopicTemplate", "MoveTopicsTemplate", "TopicListTemplate", "TopicListPageTemplate", 
				"TopicTemplate", "TopicPageTemplate", "TopicReplyTemplate", "TopicMoveTemplate", 
				"TopicEditTemplate", "PostReadTemplate",  "PostQuoteTemplate", "PostEditTemplate", 
				"UserProfileTemplate", "UserProfileEditTemplate", "ForumRulesTemplate", "ForumHelpTemplate", 
                "RssTopicsAllTemplate", "RssTopicsFromForumTemplate", "RssPostsFromTopicTemplate","UserPostsTemplate",
                "UserPostsPageTemplate", "UserSubscriptionsUrlTemplate", "UserSubscriptionsPageUrlTemplate",
                "ActiveTopicsUrlTemplate","UnAnsweredTopicsUrlTemplate","UnAnsweredTopicsPageUrlTemplate",
                "ActiveTopicsPageUrlTemplate"};

			foreach (string id in ids)
				ParamsDefinition[id].ClientSideAction = new ParamClientSideActionGroupViewMember(csaId, id, new string[] { "Sef" });

			
			if (IsSearch)
			{
				foreach (string id in new string[] {"SearchUrlTemplate", "SearchPageUrlTemplate"})
					ParamsDefinition[id].ClientSideAction = new ParamClientSideActionGroupViewMember(csaId, id, new string[] { "EnableSearch", "Sef" }, ParamClientSideActionGroupViewMemberDisplayCondition.And);
			}


			DirectoryInfo info = new DirectoryInfo(BXPath.MapPath("~/bitrix/components/bitrix/forum/templates/.default/themes/"));
			if (info.Exists)
			{
				List<BXParamValue> values = new List<BXParamValue>();
				foreach (DirectoryInfo sub in info.GetDirectories())
				{
					if (!File.Exists(Path.Combine(sub.FullName, "style.css")))
						continue;

					string themeTitle = BXLoc.GetMessage("~/bitrix/components/bitrix/forum/templates/.default/themes/" + sub.Name + "/description", "Title");
					if (String.IsNullOrEmpty(themeTitle))
						themeTitle = sub.Name;

					values.Add(new BXParamValue(themeTitle, VirtualPathUtility.Combine("~/bitrix/components/bitrix/forum/templates/.default/themes/", sub.Name + "/style.css")));
				}
				ParamsDefinition["ColorCss"].Values = values;
			}

			ParamsDefinition["GuestEmail"].Values = new List<BXParamValue>(new BXParamValue[] {
				new BXParamValue(GetMessageRaw("GuestEmail.Hide"), ""),
				new BXParamValue(GetMessageRaw("GuestEmail.Show"), "visible"),
				new BXParamValue(GetMessageRaw("GuestEmail.Require"), "required")
			});

            //RolesAuthorizedToVote
            //IList<BXParamValue> rolesValues = ParamsDefinition["RolesAuthorizedToVote"].Values;
            //rolesValues.Clear();
            //foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
            //{
            //    if (string.Equals(r.RoleName, "Guest", StringComparison.Ordinal))
            //        continue;
            //    rolesValues.Add(new BXParamValue(r.Title, r.RoleName));
            //}
		}

		private void ProvideSearchUrl(BXCommand cmd, bool executionContext, string executionVirtualPath)
		{
			if (cmd.Parameters.GetString("moduleId") != "forum")
				return;

			int forumId = cmd.Parameters.GetInt("itemGroup");
			if (AvailableForums.Count > 0 && !AvailableForums.Contains(forumId))
				return;

			int postId = cmd.Parameters.GetInt("itemId");
			if (postId < 0)
				return;

			int topicId = cmd.Parameters.GetInt("param1");

			string url;
			if (EnableSef)
			{
				BXParamsBag<object> replaceItems = new BXParamsBag<object>();
				replaceItems.Add("ForumId", forumId);
				replaceItems.Add("TopicId", topicId);
				replaceItems.Add("PostId", postId);

				url = BXSefUrlUtility.MakeLink(CombineLink(Parameters.GetString("SefFolder"), Parameters.GetString("PostReadTemplate")), replaceItems);
			}
			else
			{
				url = string.Format("{0}?{1}={2}", BXSite.GetUrlForPath(executionVirtualPath, null, cmd.SiteId), UrlEncode(PostVariable), postId);
			}

			cmd.AddCommandResult("bitrix:forum@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, url));
		}
		private void GeneratePublicMenu(BXCommand cmd, bool executionContext, string executionVirtualPath)
		{
			string sefFolder = Parameters.GetString("SEFFolder", "");

			//Совпадает ли тип меню в параметрах компонента с типом, который запрашивает система.
			if (!Parameters.Get("GenerateMenuType", "left").Equals(cmd.Parameters.Get<string>("menuType"), StringComparison.InvariantCultureIgnoreCase))
				return;

			//Генерируем меню только для тех адресов, которые выводит сам компонент.
			if (!EnableSef && !BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath).Equals(cmd.Parameters.Get<string>("uriPath"), StringComparison.InvariantCultureIgnoreCase))
				return;
			else if (EnableSef && !cmd.Parameters.Get<string>("uriPath").StartsWith(Bitrix.IO.BXPath.ToVirtualRelativePath(sefFolder.TrimStart('\\', '/')) + "/", StringComparison.InvariantCultureIgnoreCase))
				return;

			BXParamsBag<object> request = new BXParamsBag<object>();
			if (EnableSef)
			{
				BXParamsBag<string> sefMap = GetSefMap();
				//MapVariable(sefFolder, sefMap, request, "index", BXUri.ToRelativeUri(cmd.Parameters.GetString("uri")));

				Uri url = new Uri(BXUri.ToAbsoluteUri(cmd.Parameters.Get<string>("uri")));
				BXSefUrlUtility.MapVariable(SefFolder, sefMap, request, "index", url, null);
			}
			else
			{
				BXParamsBag<string> variableAlias = new BXParamsBag<string>();
				variableAlias["ForumId"] = ForumVariable;
				variableAlias["TopicId"] = TopicVariable;
				variableAlias["PostId"] = PostVariable;
				variableAlias["PageId"] = PageVariable;
				MapVariable(variableAlias, request, BXUri.ToRelativeUri(cmd.Parameters.Get<string>("uri")));
			}

			int topicId = 0, postId = 0, forumId = 0;
			forumId = request.GetInt("ForumId", forumId);
			topicId = request.GetInt("TopicId", topicId);
			postId = request.GetInt("PostId", postId);

			string parentLevelUri = null;
			Dictionary<string, BXPublicMenu.BXLoadMenuCommandParameter> menuTree = new Dictionary<string, BXPublicMenu.BXLoadMenuCommandParameter>();

			if (topicId > 0)
			{
				BXFilter topicFilter = new BXFilter();
				topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Id, BXSqlFilterOperators.Equal, topicId));
				topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Active, BXSqlFilterOperators.Equal, true));
				topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));
				if (AvailableForums.Count > 0)
					topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.In, AvailableForums));

				if (forumId > 0)
					topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.Equal, forumId));

				BXForumTopicCollection topics = BXForumTopic.GetList(topicFilter, null, new BXSelectAdd(BXForumTopic.Fields.Forum), null);
				if (topics.Count > 0)
				{
					BXParamsBag<object> replace = new BXParamsBag<object>();
					replace["ForumId"] = topics[0].Forum.Id;
					replace["ForumCode"] = topics[0].Forum.Code;
					parentLevelUri = MakeMenuUri(executionVirtualPath, replace);
				}
			}
			else if (postId > 0)
			{
				BXFilter postFilter = new BXFilter();
				postFilter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Equal, postId));
				postFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Active, BXSqlFilterOperators.Equal, true));
				postFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));
				if (AvailableForums.Count > 0)
					postFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Id, BXSqlFilterOperators.In, AvailableForums));

				if (forumId > 0)
					postFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Id, BXSqlFilterOperators.Equal, forumId));

				BXForumPostCollection posts = BXForumPost.GetList(postFilter, null, new BXSelectAdd(BXForumPost.Fields.Forum), null);
				if (posts.Count > 0)
				{
					BXParamsBag<object> replace = new BXParamsBag<object>();
					replace["ForumId"] = posts[0].Forum.Id;
					replace["ForumCode"] = posts[0].Forum.Code;
					parentLevelUri = MakeMenuUri(executionVirtualPath, replace);
				}
			}
			else
			//if (parentLevelUri == null)
			{
				BXFilter forumsFilter = new BXFilter();
				forumsFilter.Add(new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true));
				forumsFilter.Add(new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));
				if (AvailableForums.Count > 0)
					forumsFilter.Add(new BXFilterItem(BXForum.Fields.Id, BXSqlFilterOperators.In, AvailableForums));

				BXForumCollection forums = BXForum.GetList(
					forumsFilter,
					new BXOrderBy(
						new BXOrderByPair(BXForum.Fields.Category.Sort, BXOrderByDirection.Asc),
						new BXOrderByPair(BXForum.Fields.Category.Id, BXOrderByDirection.Asc),
						new BXOrderByPair(BXForum.Fields.Sort, BXOrderByDirection.Asc),
						new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)
					),
					null,
					null,
					BXTextEncoder.EmptyTextEncoder
				);

				string rootForumUrl = EnableSef ? sefFolder : BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath);
				List<BXPublicMenuItem> menuItems = new List<BXPublicMenuItem>();
				BXParamsBag<object> replace = new BXParamsBag<object>();
				int sort = 1;
				foreach (BXForum forum in forums)
				{
					BXPublicMenuItem menuItem = new BXPublicMenuItem();
					menuItem.Title = forum.Name;

					replace["ForumId"] = forum.Id;
					replace["ForumCode"] = forum.Code;

					string url = MakeMenuUri(executionVirtualPath, replace);
					menuItem.Links.Add(url);
					menuItem.ConditionType = ConditionType.Group;
					menuItem.Condition = GetMenuItemPermission(forum.Id);
					menuItem.Sort = sort++;
					menuItems.Add(menuItem);
					menuTree.Add(url, new BXPublicMenu.BXLoadMenuCommandParameter(null, true, rootForumUrl));
				}
				menuTree.Add(rootForumUrl, new BXPublicMenu.BXLoadMenuCommandParameter(menuItems, true, null));
			}

			if (menuTree.Count > 0)
				cmd.AddCommandResult("bitrix:forum@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, menuTree));
			else if (parentLevelUri != null)
			{
				BXPublicMenu.BXLoadMenuCommandParameter menuResult = new BXPublicMenu.BXLoadMenuCommandParameter(null, true, parentLevelUri);
				cmd.AddCommandResult("bitrix:forum@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, menuResult));
			}
		}
		private string GetMenuItemPermission(int forumId)
		{
			string permissionCacheKey = "forum-" + forumId + "-menu-permission";
			string menuItemPermisson;

			if ((menuItemPermisson = (string)BXCacheManager.MemoryCache.Get(permissionCacheKey)) != null)
				return menuItemPermisson;

			menuItemPermisson = String.Empty;
			StringBuilder menuItemRoles = new StringBuilder();

			BXRoleCollection forumRoles = BXRoleManager.GetAllRolesForOperation(
				BXForum.Operations.ForumPublicRead, BXForumModuleConfiguration.Id, forumId.ToString()
			);

			foreach (BXRole role in forumRoles)
			{
				//Если доступно всем
				if (role.RoleName == "Guest")
				{
					menuItemRoles = null;
					break;
				}
				//Если доступно группе User, значит достаточно проверить только для этой группы
				else if (role.RoleName == "User")
				{
					menuItemRoles = null;
					menuItemPermisson = "User";
					break;
				}
				else
				{
					menuItemRoles.Append(role.RoleName);
					menuItemRoles.Append(";");
				}
			}

			if (menuItemRoles != null && menuItemRoles.Length > 0)
			{
				menuItemRoles.Append("Admin");
				menuItemPermisson = menuItemRoles.ToString();
			}
			BXCacheManager.MemoryCache.Insert(permissionCacheKey, menuItemPermisson);

			return menuItemPermisson;
		}
		private string MakeMenuUri(string executionVirtualPath, BXParamsBag<object> replaceItems)
		{
			string url;

			if (EnableSef)
			{
				if (SefFolder == "/" || SefFolder == String.Empty)
					url = BXPath.TrimStart(Parameters.GetString("TopicListTemplate"));
				else
					url = BXPath.Trim(SefFolder) + "/" + BXPath.TrimStart(Parameters.GetString("TopicListTemplate"));
				//url = BXPath.Combine(BXPath.Trim(SefFolder), Parameters.GetString("TopicListTemplate"));
				url = BXSefUrlUtility.MakeLink(url, replaceItems);
			}
			else
				url = BXSefUrlUtility.MakeLink(String.Format("{0}?{1}=#ForumId#", BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath), UrlEncode(ForumVariable)), replaceItems);

			return url;
		}

		private string PrepareSefMode()
		{
			string sefFolder = Parameters.GetString("SEFFolder", "");
			ComponentCache["IndexUrlTemplate"] = CombineLink(sefFolder, "");
			ComponentCache["NewTopicUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("NewTopicTemplate"));
			ComponentCache["MoveTopicsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("MoveTopicsTemplate"));
			ComponentCache["TopicListUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicListTemplate"));
			ComponentCache["TopicListPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicListPageTemplate"));
			ComponentCache["TopicUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicTemplate"));
			ComponentCache["TopicPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicPageTemplate"));
			ComponentCache["TopicReplyUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicReplyTemplate"));
			ComponentCache["TopicMoveUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicMoveTemplate"));
			ComponentCache["TopicEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicEditTemplate"));
			ComponentCache["PostReadUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostReadTemplate"));
			ComponentCache["PostQuoteUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostQuoteTemplate"));
			ComponentCache["PostEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("PostEditTemplate"));
			ComponentCache["UserProfileUrlTemplate"] = GetExternalUrlTemplate("UserProfileUrl") ?? CombineLink(sefFolder, Parameters.GetString("UserProfileTemplate"));
			ComponentCache["UserProfileEditUrlTemplate"] = GetExternalUrlTemplate("UserProfileEditUrl") ?? CombineLink(sefFolder, Parameters.GetString("UserProfileEditTemplate"));
			ComponentCache["UserSubscriptionsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("UserSubscriptionsUrlTemplate"));
			ComponentCache["UserSubscriptionsPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("UserSubscriptionsPageUrlTemplate"));

			ComponentCache["ForumRulesUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("ForumRulesTemplate"));
			ComponentCache["ForumHelpUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("ForumHelpTemplate"));

			ComponentCache["ActiveTopicsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("ActiveTopicsUrlTemplate"));
			ComponentCache["UnAnsweredTopicsUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("UnAnsweredTopicsUrlTemplate"));
			ComponentCache["ActiveTopicsPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("ActiveTopicsPageUrlTemplate"));
			ComponentCache["UnAnsweredTopicsPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("UnAnsweredTopicsPageUrlTemplate"));

			ComponentCache["RssTopicsAllTemplate"] = CombineLink(sefFolder, Parameters.GetString("RssTopicsAllTemplate", string.Empty));
			ComponentCache["RssTopicsFromForumTemplate"] = CombineLink(sefFolder, Parameters.GetString("RssTopicsFromForumTemplate", string.Empty));
			ComponentCache["RssPostsFromTopicTemplate"] = CombineLink(sefFolder, Parameters.GetString("RssPostsFromTopicTemplate", string.Empty));

			if (EnableSearch)
			{
				ComponentCache["SearchUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("SearchUrlTemplate", string.Empty));
				ComponentCache["SearchPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("SearchPageUrlTemplate", string.Empty));
			}

			ComponentCache["UserPostsReadUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("UserPostsTemplate", string.Empty));


			ComponentCache["ForumRulesUrl"] = GetExternalUrl("ForumRulesUrl") ?? ComponentCache.GetString("ForumRulesUrlTemplate");
			ComponentCache["ForumHelpUrl"] = GetExternalUrl("ForumHelpUrl") ?? ComponentCache.GetString("ForumHelpUrlTemplate");

			string page = PrepareSefPage();

			BXParamsBag<object> replace = new BXParamsBag<object>();
			object forumId;
			if (ComponentCache.TryGetValue("ForumId", out forumId))
				replace.Add("ForumId", forumId);
			object forumCode;
			if (ComponentCache.TryGetValue("ForumCode", out forumCode))
				replace.Add("ForumCode", forumCode);
			object topicId;
			if (ComponentCache.TryGetValue("TopicId", out topicId))
				replace.Add("TopicId", topicId);
			object postId;
			if (ComponentCache.TryGetValue("PostId", out postId))
				replace.Add("PostId", postId);
			object UserId;
			if (ComponentCache.TryGetValue("UserId", out UserId))
				replace.Add("UserId", UserId);

			ComponentCache["RssTopicsAllUrl"] = ResolveTemplateUrl(ComponentCache["RssTopicsAllTemplate"] as string, replace);
			ComponentCache["RssTopicsFromForumUrl"] = ResolveTemplateUrl(ComponentCache["RssTopicsFromForumTemplate"] as string, replace);
			ComponentCache["RssPostsFromTopicUrl"] = ResolveTemplateUrl(ComponentCache["RssPostsFromTopicTemplate"] as string, replace);
			ComponentCache["UserPostsPageTemplate"] = CombineLink(sefFolder, Parameters.GetString("UserPostsPageTemplate", String.Empty));
			ComponentCache["UserPostsTemplate"] = CombineLink(sefFolder, Parameters.GetString("UserPostsTemplate", String.Empty));


			if (string.Equals(page, "form", StringComparison.Ordinal))
				ComponentCache["FormHeaderLinks"] = GetFormHeaderLinks();
			else if (string.Equals(page, "index", StringComparison.Ordinal))
				ComponentCache["IndexFooterLinks"] = GetIndexFooterLinks();
			else if (string.Equals(page, "forum", StringComparison.Ordinal))
				ComponentCache["ForumHeaderLinks"] = GetForumHeaderLinks();
			else if (string.Equals(page, "topic", StringComparison.Ordinal))
				ComponentCache["TopicHeaderLinks"] = GetTopicHeaderLinks();
			return page;

		}
		private BXParamsBag<string> GetSefMap()
		{
			BXParamsBag<string> sefMap = new BXParamsBag<string>();
			sefMap.Add("form.add", Parameters.GetString("NewTopicTemplate", ""));
			sefMap.Add("move", Parameters.GetString("MoveTopicsTemplate", ""));
			sefMap.Add("forum", Parameters.GetString("TopicListTemplate", ""));
			sefMap.Add("forum.page", Parameters.GetString("TopicListPageTemplate", ""));
			sefMap.Add("topic", Parameters.GetString("TopicTemplate", ""));
			sefMap.Add("topic.page", Parameters.GetString("TopicPageTemplate", ""));
			sefMap.Add("form.reply", Parameters.GetString("TopicReplyTemplate", ""));
			sefMap.Add("move.topic", Parameters.GetString("TopicMoveTemplate", ""));
			sefMap.Add("form.topic", Parameters.GetString("TopicEditTemplate", ""));
			sefMap.Add("topic.post", Parameters.GetString("PostReadTemplate", ""));
			sefMap.Add("form.quote", Parameters.GetString("PostQuoteTemplate", ""));
			sefMap.Add("form.post", Parameters.GetString("PostEditTemplate", ""));
			sefMap.Add("profileview", Parameters.GetString("UserProfileTemplate", ""));
			sefMap.Add("profileedit", Parameters.GetString("UserProfileEditTemplate", ""));
			sefMap.Add("info.rules", Parameters.GetString("ForumRulesTemplate", ""));
			sefMap.Add("info.help", Parameters.GetString("ForumHelpTemplate", ""));
			sefMap.Add("subscriptions", Parameters.GetString("UserSubscriptionsUrlTemplate", ""));
			sefMap.Add("subscriptions.page", Parameters.GetString("UserSubscriptionsPageUrlTemplate", ""));
			sefMap.Add("rss.topicsAll", Parameters.GetString("RssTopicsAllTemplate", ""));
			sefMap.Add("rss.topicsOfForum", Parameters.GetString("RssTopicsFromForumTemplate", ""));
			sefMap.Add("rss.postsOfTopic", Parameters.GetString("RssPostsFromTopicTemplate", ""));
			sefMap.Add("postlist", Parameters.GetString("UserPostsTemplate", ""));
			sefMap.Add("activetopics", Parameters.GetString("ActiveTopicsUrlTemplate", ""));
			sefMap.Add("unansweredtopics", Parameters.GetString("UnAnsweredTopicsUrlTemplate", ""));
			if (EnableSearch)
				sefMap.Add("search", Parameters.GetString("SearchUrlTemplate", ""));
			return sefMap;
		}
		private string PrepareSefPage()
		{
			BXParamsBag<string> sefMap = GetSefMap();
			string code = BXSefUrlUtility.MapVariable(Parameters.GetString("SEFFolder", ""), sefMap, ComponentCache, "index", null, null);
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

			if (string.Equals(page, "rss", StringComparison.Ordinal) && !EnableRss)
			{
				page = "index";
				action = string.Empty;
			}

			switch (page)
			{
				case "form":
					switch (action)
					{
						case "quote":
							ComponentCache["ParentPostId"] = ComponentCache["PostId"];
							ComponentCache["PostId"] = string.Empty;
							ComponentCache["FormMode"] = "add";
							ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostReadUrlTemplate"];
							break;
						case "post":
							ComponentCache["FormMode"] = "edit";
							ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostReadUrlTemplate"];
							break;
						case "reply":
							ComponentCache["PostId"] = string.Empty;
							ComponentCache["FormMode"] = "add";
							ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostReadUrlTemplate"];
							break;
						case "topic":
							ComponentCache["PostId"] = string.Empty;
							ComponentCache["FormMode"] = "edit";
							ComponentCache["RedirectUrlTemplate"] = ComponentCache["TopicUrlTemplate"];
							break;
						case "add":
							ComponentCache["PostId"] = string.Empty;
							ComponentCache["FormMode"] = "add";
							ComponentCache["RedirectUrlTemplate"] = ComponentCache["TopicUrlTemplate"];
							break;
					}
					break;
				case "move":
					switch (action)
					{
						case "topic":
							ComponentCache["RedirectUrlTemplate"] = ComponentCache["TopicListUrlTemplate"];
							ComponentCache["MoveTopicsIds"] = ComponentCache["TopicId"];
							break;
						default:
							ComponentCache["RedirectUrlTemplate"] = ComponentCache["TopicListUrlTemplate"];
							break;
					}
					break;
				case "profileview":
				case "profileedit":

					if (page == "profileedit")
					{
						ComponentCache["ProfileFields"] = GetUserEditProfileFields();

						string url = GetExternalUrlTemplate("UserProfileEditUrl");
						if (url != null)
							RedirectTemplateUrl(url, ComponentCache);
						ComponentCache["PageTitle"] = GetMessageRaw("PageTitle.EditProfile");
						ComponentCache["RequiredProfileFields"] = GetRequiredUserProfileFields();
					}
					else
					{
						ComponentCache["ProfileFields"] = GetUserViewProfileFields();
						string url = GetExternalUrlTemplate("UserProfileUrl");
						if (url != null)
							RedirectTemplateUrl(url, ComponentCache);
						ComponentCache["PageTitle"] = GetMessageRaw("PageTitle.ViewProfile");
					}
					break;
				case "info":
					switch (action)
					{
						case "rules":
							PrepareInfoUrl(GetMessageRaw("PageTitle.Rules"), "ForumRulesUrl", "/info/rules.ascx");
							break;
						case "help":
							PrepareInfoUrl(GetMessageRaw("PageTitle.Help"), "ForumHelpUrl", "/info/help.ascx");
							break;
					}
					break;
				case "rss":
					{
						switch (action)
						{
							case "topicsAll":
								{
									//новые темы форумов
									ComponentCache["ForumRssStuffType"] = "Topic";
									ComponentCache["ForumRssFiltrationType"] = "None";
									//если нужно переопределить описания
									//ComponentCache["ForumRssFeedTitle"] = "";
									//ComponentCache["ForumRssFeedDescription"] = "";
									ComponentCache["ForumRssFeedUrlTemplate"] = CombineLink(SefFolder, null);
									ComponentCache["ForumRssFeedItemUrlTemplate"] = CombineLink(SefFolder, Parameters.GetString("TopicTemplate"));
                                    ComponentCache["ForumRssFeedCacheMode"] = Parameters.GetString("RssCacheMode", "None");
								}
								break;
							case "topicsOfForum":
								{
									//новые темы форума
									ComponentCache["ForumRssStuffType"] = "Topic";
									ComponentCache["ForumRssFiltrationType"] = "ForumId";
									ComponentCache["ForumRssForumId"] = ComponentCache["ForumId"];
									//если нужно переопределить описания
									//ComponentCache["ForumRssFeedTitle"] = "";
									//ComponentCache["ForumRssFeedDescription"] = "";
									ComponentCache["ForumRssFeedUrlTemplate"] = CombineLink(SefFolder, Parameters.GetString("TopicListTemplate"));
									ComponentCache["ForumRssFeedItemUrlTemplate"] = CombineLink(SefFolder, Parameters.GetString("TopicTemplate"));
                                    ComponentCache["ForumRssFeedCacheMode"] = Parameters.GetString("RssCacheMode", "None");
								}
								break;
							case "postsOfTopic":
								//новые сообщения в теме форума
								ComponentCache["ForumRssStuffType"] = "Post";
								ComponentCache["ForumRssFiltrationType"] = "TopicId";
								ComponentCache["ForumRssTopicId"] = ComponentCache["TopicId"];
								//если нужно переопределить описания
								//ComponentCache["ForumRssFeedTitle"] = "";
								//ComponentCache["ForumRssFeedDescription"] = "";
								ComponentCache["ForumRssFeedUrlTemplate"] = CombineLink(SefFolder, Parameters.GetString("TopicTemplate"));
								ComponentCache["ForumRssFeedItemUrlTemplate"] = CombineLink(SefFolder, Parameters.GetString("PostReadTemplate"));
                                ComponentCache["ForumRssFeedCacheMode"] = "None"; //не кешируем из-за возможно большого кол-ва файлов
								break;
						}
						ComponentCache["ForumRssFeedItemQuantity"] = Parameters.GetString("RssItemQuantity", "20");
						ComponentCache["ForumRssFeedCacheDuration"] = Parameters.GetString("RssCacheDuration", "30");
						ComponentCache["ForumRssFeedTtl"] = Parameters.GetString("RssFeedTtl", "60");
					}
					break;
				case "postlist":
					break;
				case "subscriptions":
					break;
				case "unansweredtopics":
					break;
				case "activetopics":
					break;
				case "search":
					break;
			}

			return page;
		}

		private string PrepareNormalMode()
		{
			string forumVar = UrlEncode(ForumVariable);
			string topicVar = UrlEncode(TopicVariable);
			string postVar = UrlEncode(PostVariable);
			string actionVar = UrlEncode(ActionVariable);
			string pageVar = UrlEncode(PageVariable);
			string userVar = UrlEncode(UserVariable);
			string filePath = BXSefUrlManager.CurrentUrl.AbsolutePath;

			ComponentCache["IndexUrlTemplate"] = filePath;
			ComponentCache["NewTopicUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=new", filePath, forumVar, actionVar);
			ComponentCache["MoveTopicsUrlTemplate"] = String.Format("{0}?{1}=#MoveTopicsIds#&{2}=move", filePath, UrlEncode(TopicsVariable), actionVar);
			ComponentCache["TopicListUrlTemplate"] = String.Format("{0}?{1}=#ForumId#", filePath, forumVar);
			ComponentCache["TopicListPageUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=#PageId#", filePath, forumVar, pageVar);
			ComponentCache["TopicUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=#TopicId#", filePath, forumVar, topicVar);
			ComponentCache["TopicPageUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=#TopicId#&{3}=#PageId#", filePath, forumVar, topicVar, pageVar);
			ComponentCache["TopicReplyUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=#TopicId#&{3}=reply", filePath, forumVar, topicVar, actionVar);
			ComponentCache["TopicMoveUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=#TopicId#&{3}=move", filePath, forumVar, topicVar, actionVar);
			ComponentCache["TopicEditUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=#TopicId#&{3}=edit", filePath, forumVar, topicVar, actionVar);
			ComponentCache["PostReadUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=#PostId###post#PostId#", filePath, forumVar, postVar);
			ComponentCache["PostQuoteUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=#PostId#&{3}=quote", filePath, forumVar, postVar, actionVar);
			ComponentCache["PostEditUrlTemplate"] = String.Format("{0}?{1}=#ForumId#&{2}=#PostId#&{3}=edit", filePath, forumVar, postVar, actionVar);
			ComponentCache["UserProfileUrlTemplate"] = GetExternalUrlTemplate("UserProfileUrl") ?? String.Format("{0}?{1}=#UserId#", filePath, UrlEncode(UserVariable));
			ComponentCache["UserProfileEditUrlTemplate"] = GetExternalUrlTemplate("UserProfileEditUrl") ?? String.Format("{0}?{1}=#UserId#&{2}=edit", filePath, UrlEncode(UserVariable), actionVar);
			ComponentCache["UserSubscriptionsUrlTemplate"] = String.Format("{0}?{1}=subscriptions", filePath, actionVar);
			ComponentCache["UserSubscriptionsPageUrlTemplate"] = String.Format("{0}?{1}=subscriptions&{2}=#PageId#", filePath, actionVar, pageVar);

			if (EnableSearch)
			{
				ComponentCache["SearchUrlTemplate"] = String.Format("{0}?{1}=search", filePath, actionVar);
				ComponentCache["SearchPageUrlTemplate"] = String.Format("{0}?{1}=search&{2}=#PageId#", filePath, actionVar, pageVar);
			}

			ComponentCache["ForumRulesUrlTemplate"] = String.Format("{0}?{1}=rules", filePath, actionVar);
			ComponentCache["ForumHelpUrlTemplate"] = String.Format("{0}?{1}=help", filePath, actionVar);

			ComponentCache["ActiveTopicsUrlTemplate"] = String.Format("{0}?{1}=activetopics", filePath, actionVar);
			ComponentCache["UnAnsweredTopicsUrlTEmplate"] = GetExternalUrl("UnAnsweredTopicsUrl") ?? String.Format("{0}?{1}=unansweredtopics", filePath, actionVar);

			ComponentCache["ActiveTopicsPageUrlTemplate"] = String.Format("{0}?{1}=activetopics&{2}=#PageId#", filePath, actionVar, pageVar);
			ComponentCache["UnAnsweredTopicsPageUrlTemplate"] = String.Format("{0}?{1}=UnAnsweredtopics&{2}=#PageId#", filePath, actionVar, pageVar);

			ComponentCache["ForumRulesUrl"] = GetExternalUrl("ForumRulesUrl") ?? ComponentCache.GetString("ForumRulesUrlTemplate");
			ComponentCache["ForumHelpUrl"] = GetExternalUrl("ForumHelpUrl") ?? ComponentCache.GetString("ForumHelpUrlTemplate");

			ComponentCache["UserPostsTemplate"] = String.Format("{0}?{1}=#UserId#&{2}=viewposts", filePath, userVar, actionVar);
			ComponentCache["UserPostsReadUrlTemplate"] = String.Format("{0}?{1}=#UserId#&{2}=viewposts", filePath, userVar, actionVar);
			ComponentCache["UserPostsPageTemplate"] = String.Format("{0}?{1}=#UserId#&{2}=viewposts&page=#PageId#", filePath, userVar, actionVar);
			string page = PrepareNormalPage();

			BXParamsBag<object> replace = new BXParamsBag<object>();
			object forumId;
			if (ComponentCache.TryGetValue("ForumId", out forumId))
				replace.Add("ForumId", forumId);

			object topicId;
			if (ComponentCache.TryGetValue("TopicId", out topicId))
				replace.Add("TopicId", topicId);
			object postId;
			if (ComponentCache.TryGetValue("PostId", out postId))
				replace.Add("PostId", postId);

			ComponentCache["RssTopicsAllUrl"] = ResolveTemplateUrl(String.Format("{0}?{1}=rss_topics", filePath, actionVar), replace);
			ComponentCache["RssTopicsFromForumUrl"] = ResolveTemplateUrl(String.Format("{0}?{1}=#ForumId#&{2}=rss_topics", filePath, forumVar, actionVar), replace);
			ComponentCache["RssPostsFromTopicUrl"] = ResolveTemplateUrl(String.Format("{0}?{1}=#ForumId#&{2}=#TopicId#&{3}=rss_posts", filePath, forumVar, topicVar, actionVar), replace);


			if (string.Equals(page, "form", StringComparison.Ordinal))
				ComponentCache["FormHeaderLinks"] = GetFormHeaderLinks();
			else if (string.Equals(page, "index", StringComparison.Ordinal))
				ComponentCache["IndexFooterLinks"] = GetIndexFooterLinks();
			else if (string.Equals(page, "forum", StringComparison.Ordinal))
				ComponentCache["ForumHeaderLinks"] = GetForumHeaderLinks();
			else if (string.Equals(page, "topic", StringComparison.Ordinal))
				ComponentCache["TopicHeaderLinks"] = GetTopicHeaderLinks();
			return page;
		}
		private string PrepareNormalPage()
		{
			int? forumId = ReadQueryInt(ForumVariable);
			int? topicId = ReadQueryInt(TopicVariable);
			int? postId = ReadQueryInt(PostVariable);
			int? pageId = ReadQueryInt(PageVariable);
			string action = (Request.QueryString[ActionVariable] ?? string.Empty).ToLowerInvariant();

			ComponentCache["ForumId"] = forumId.ToString();
			ComponentCache["TopicId"] = topicId.ToString();
			ComponentCache["PostId"] = postId.ToString();
			ComponentCache["PageId"] = pageId.ToString();

			if ((string.Equals(action, "rss_topics", StringComparison.Ordinal) || string.Equals(action, "rss_posts", StringComparison.Ordinal))
				&& !EnableRss)
				action = string.Empty;

			if (postId != null)
			{
				switch (action)
				{
					case "quote":
						ComponentCache["ParentPostId"] = ComponentCache["PostId"];
						ComponentCache["PostId"] = string.Empty;
						ComponentCache["FormMode"] = "add";
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostReadUrlTemplate"];
						return "form";
					case "edit":
						ComponentCache["FormMode"] = "edit";
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostReadUrlTemplate"];
						return "form";
					default:
						return "topic";
				}
			}

			if (topicId != null)
			{
				switch (action)
				{
					case "reply":
						ComponentCache["PostId"] = string.Empty;
						ComponentCache["FormMode"] = "add";
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["PostReadUrlTemplate"];
						return "form";
					case "edit":
						ComponentCache["PostId"] = string.Empty;
						ComponentCache["FormMode"] = "edit";
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["TopicUrlTemplate"];
						return "form";
					case "move":
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["TopicListUrlTemplate"];
						ComponentCache["MoveTopicsIds"] = ComponentCache["TopicId"];
						return "move";
					case "rss_posts":
					case "rss_topics": //игнорируется
						//новые сообщения в теме форума
						ComponentCache["ForumRssStuffType"] = "Post";
						ComponentCache["ForumRssFiltrationType"] = "TopicId";
						ComponentCache["ForumRssTopicId"] = topicId.Value;
						//если нужно переопределить описания
						//ComponentCache["ForumRssFeedTitle"] = "";
						//ComponentCache["ForumRssFeedDescription"] = "";
						ComponentCache["ForumRssFeedUrlTemplate"] = ComponentCache["TopicUrlTemplate"];
						ComponentCache["ForumRssFeedItemUrlTemplate"] = ComponentCache["PostReadUrlTemplate"];

						ComponentCache["ForumRssFeedItemQuantity"] = Parameters.GetString("RssItemQuantity", "20");
                        ComponentCache["ForumRssFeedCacheMode"] = "None"; //не кешируем из-за возможно большого кол-ва файлов
						ComponentCache["ForumRssFeedCacheDuration"] = Parameters.GetString("RssCacheDuration", "30");
						ComponentCache["ForumRssFeedTtl"] = Parameters.GetString("RssFeedTtl", "60");
						return "rss";
					default:
						return "topic";
				}
			}

			if (forumId != null)
			{
				switch (action)
				{
					case "new":
						ComponentCache["PostId"] = string.Empty;
						ComponentCache["FormMode"] = "add";
						ComponentCache["RedirectUrlTemplate"] = ComponentCache["TopicUrlTemplate"];
						return "form";
					case "rss_topics":
					case "rss_posts":
						//новые темы форума
						ComponentCache["ForumRssStuffType"] = "Topic";
						ComponentCache["ForumRssFiltrationType"] = "ForumId";
						ComponentCache["ForumRssForumId"] = forumId.Value;
						//если нужно переопределить описания
						//ComponentCache["ForumRssFeedTitle"] = "";
						//ComponentCache["ForumRssFeedDescription"] = "";
						ComponentCache["ForumRssFeedUrlTemplate"] = ComponentCache["TopicListUrlTemplate"];
						ComponentCache["ForumRssFeedItemUrlTemplate"] = ComponentCache["TopicUrlTemplate"];

						ComponentCache["ForumRssFeedItemQuantity"] = Parameters.GetString("RssItemQuantity", "20");
						ComponentCache["ForumRssFeedCacheMode"] = Parameters.GetString("RssCacheMode", "None");
						ComponentCache["ForumRssFeedCacheDuration"] = Parameters.GetString("RssCacheDuration", "30");
						ComponentCache["ForumRssFeedTtl"] = Parameters.GetString("RssFeedTtl", "60");
						return "rss";
					default:
						return "forum";
				}
			}

			int? userId = ReadQueryInt(Parameters["UserVariable"]);
			if (userId != null)
			{
				ComponentCache["UserId"] = userId.ToString();

				string url;
				switch (action)
				{
					case "edit":
						url = GetExternalUrlTemplate("UserProfileEditUrl");
						if (url != null)
							RedirectTemplateUrl(url, ComponentCache);
						ComponentCache["RequiredProfileFields"] = GetRequiredUserProfileFields();
						ComponentCache["PageTitle"] = GetMessageRaw("PageTitle.EditProfile");
						ComponentCache["ProfileFields"] = GetUserEditProfileFields();
						return "profileedit";
					case "viewposts":
						Results["UserId"] = userId;
						return "postlist";
					default:
						url = GetExternalUrlTemplate("UserProfileUrl");
						if (url != null)
							RedirectTemplateUrl(url, ComponentCache);
						ComponentCache["PageTitle"] = GetMessageRaw("PageTitle.ViewProfile");
						ComponentCache["ProfileFields"] = GetUserViewProfileFields();
						return "profileview";
				}
			}
			switch (action)
			{
				case "move":
					ComponentCache["RedirectUrlTemplate"] = ComponentCache["TopicListUrlTemplate"];
					ComponentCache["MoveTopicsIds"] = Request.QueryString[TopicsVariable];
					return "move";
				case "rules":
					PrepareInfoUrl(GetMessageRaw("PageTitle.Rules"), "ForumRulesUrl", "/info/rules.ascx");
					return "info";
				case "help":
					PrepareInfoUrl(GetMessageRaw("PageTitle.Help"), "ForumHelpUrl", "/info/help.ascx");
					return "info";
				case "activetopics":
					return "activetopics";
				case "unansweredtopics":
					return "unansweredtopics";
				case "subscriptions":
					return "subscriptions";
				case "search":
					return EnableSearch ? "search" : "index";
				case "rss_topics":
				case "rss_posts":
					//новые темы форумов
					ComponentCache["ForumRssStuffType"] = "Topic";
					ComponentCache["ForumRssFiltrationType"] = "None";
					//если нужно переопределить описания
					//ComponentCache["ForumRssFeedTitle"] = "";
					//ComponentCache["ForumRssFeedDescription"] = "";
					ComponentCache["ForumRssFeedUrlTemplate"] = ComponentCache["IndexUrlTemplate"];
					ComponentCache["ForumRssFeedItemUrlTemplate"] = ComponentCache["TopicUrlTemplate"];

					ComponentCache["ForumRssFeedItemQuantity"] = Parameters.GetString("RssItemQuantity", "20");
					ComponentCache["ForumRssFeedCacheMode"] = Parameters.GetString("RssCacheMode", "None");
					ComponentCache["ForumRssFeedCacheDuration"] = Parameters.GetString("RssCacheDuration", "30");
					ComponentCache["ForumRssFeedTtl"] = Parameters.GetString("RssFeedTtl", "60");
					return "rss";
			}

			return "index";
		}

		private bool IsRssAllowedForCurrentForum()
		{
			object forumIdObj;
			if (!ComponentCache.TryGetValue("ForumId", out forumIdObj))
				return false;

			int forumId = 0;
			try
			{
				forumId = Convert.ToInt32(forumIdObj);
			}
			catch (Exception /*exc*/)
			{
			}

			if (forumId <= 0)
				return false;

			BXRoleCollection roles = BXRoleManager.GetAllRolesForOperation(BXForum.Operations.ForumPublicRead, BXForumModuleConfiguration.Id, forumId.ToString());
			foreach (BXRole role in roles)
				if (string.Equals(role.RoleName, "Guest", StringComparison.InvariantCulture))
					return true;
			return false;
		}

		private string GetFormHeaderLinks()
		{
			List<BXParamsBag<object>> urls = new List<BXParamsBag<object>>();
			urls.Add(PrepareLink(
				GetMessageRaw("UrlTitle.Rules"),
				ComponentCache.GetString("ForumRulesUrl"),
				"forum-option-rules"
			));

			urls.Add(PrepareLink(
				"BBCode",
				ComponentCache.GetString("ForumHelpUrl"),
				"forum-option-bbcode"
			));

			return BXSerializer.Serialize(urls);
		}

		private string GetIndexFooterLinks()
		{
			if (!EnableRss)
				return string.Empty;
			string rssTopicsAllUrl = ComponentCache.GetString("RssTopicsAllUrl");
			List<BXParamsBag<object>> urls = new List<BXParamsBag<object>>();
			urls.Add(PrepareLink(
				GetMessageRaw("UrlTitle.RssTopicsAll"),
				rssTopicsAllUrl,
				"forum-footer-rss"
			));

			ComponentCache["RssLinkUrl"] = rssTopicsAllUrl;
			ComponentCache["RssLinkTitle"] = GetMessageRaw("UrlTitle.RssTopicsAll");
			return BXSerializer.Serialize(urls);
		}

		private string GetForumHeaderLinks()
		{
			if (!EnableRss || !IsRssAllowedForCurrentForum())
				return string.Empty;

			string rssTopicsFromForumUrl = ComponentCache.GetString("RssTopicsFromForumUrl");
			List<BXParamsBag<object>> urls = new List<BXParamsBag<object>>();
			urls.Add(PrepareLink(
				"RSS",
				rssTopicsFromForumUrl,
				"forum-option-feed"
			));
			ComponentCache["RssLinkUrl"] = rssTopicsFromForumUrl;
			ComponentCache["RssLinkTitle"] = GetMessageRaw("UrlTitle.RssForumTopics");
			return BXSerializer.Serialize(urls);
		}

		private string GetTopicHeaderLinks()
		{
			if (!EnableRss || !IsRssAllowedForCurrentForum())
				return string.Empty;

			List<BXParamsBag<object>> urls = new List<BXParamsBag<object>>();
			/*
			urls.Add(PrepareLink(
				GetMessageRaw("UrlTitle.Rules"),
				ComponentCache.GetString("ForumRulesUrl"),
				"forum-option-rules"
			));

			urls.Add(PrepareLink(
				"BBCode",
				ComponentCache.GetString("ForumHelpUrl"),
				"forum-option-bbcode"
			));
			*/
			string rssPostsFromTopicUrl = ComponentCache.GetString("RssPostsFromTopicUrl");
			urls.Add(PrepareLink(
				"RSS",
				rssPostsFromTopicUrl,
				"forum-option-feed"
			));
			ComponentCache["RssLinkUrl"] = rssPostsFromTopicUrl;
			return BXSerializer.Serialize(urls);
		}

		private string GetUserEditProfileFields()
		{
			return BXStringUtility.ListToCsv(new string[] {
				"Image",
				"DisplayName",
				"BitrixForum_Signature",
				"BitrixForum_Posts",
				"BitrixForum_OwnPostNotification",
				"@" + GetMessageRaw("FieldGroup.PersonalData"),
				"BirthdayDate",
				"Gender",
				"LastName",
				"FirstName",
				"SecondName"
			});
		}
		private string GetUserViewProfileFields()
		{
			return BXStringUtility.ListToCsv(new string[] {
				"Image",
				"DisplayName",
				"BitrixForum_Signature",
				"BitrixForum_Posts",
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

		private static BXParamsBag<object> PrepareLink(string title, string url, string cssClass)
		{
			BXParamsBag<object> result = new BXParamsBag<object>();

			result["url"] = url;
			result["title"] = title;
			result["class"] = cssClass;

			return result;
		}
		private string GetExternalUrlTemplate(string parameter)
		{
			string val = Parameters.GetString(parameter);
			return !BXStringUtility.IsNullOrTrimEmpty(val) ? val : null;
		}
		private string GetExternalUrl(string urlParameter)
		{
			string url;
			return ProcessInfoUrl(Parameters.GetString(urlParameter), out url) ? url : null;
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
		private bool ProcessInfoUrl(string url, out string target)
		{
			if (BXStringUtility.IsNullOrTrimEmpty(url))
			{
				target = null;
				return false;
			}

			url = url.Trim();
			if (url.StartsWith("~/"))
			{
				bool appRel = false;
				try
				{
					appRel = VirtualPathUtility.IsAppRelative(url);
				}
				catch
				{
				}
				if (appRel)
				{
					string ext = VirtualPathUtility.GetExtension(url);
					if (ext != null && Array.IndexOf(IncludeAreaExts, ext) != -1)
					{
						target = url;
						return false;
					}
				}
				url = ResolveUrl(url);
			}
			target = url;
			return true;
		}
		private void PrepareInfoUrl(string title, string parameterName, string urlPart)
		{
			string url;
			if (!ProcessInfoUrl(Parameters.GetString(parameterName), out url))
			{
				ComponentCache["InfoFilePath"] = url ?? (ComponentRoot + urlPart);
				ComponentCache["InfoAllowEditing"] = (url != null);
				ComponentCache["PageTitle"] = title;
				return;
			}
			Redirect(url);
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


	}

	public class ForumTemplate : BXComponentTemplate<ForumComponent>
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
