using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Services;
using Bitrix.IBlock;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using System.Collections.Generic;
using Bitrix;
using Bitrix.DataTypes;
using Bitrix.Components.Editor;
using Bitrix.Modules;
using Bitrix.Security;
using System.Text;
using Bitrix.IO;
using System.Collections.Specialized;

namespace Bitrix.IBlock.Components
{
	public partial class CatalogueComponent : BXComponent
	{
        public enum ActiveFilter
        {
            Active = 1,
            NotActive,
            All
        }

		public int IBlockId
		{
			get { return Parameters.Get<int>("IBlockId"); }
		}

		public string SectionIdVariable
		{
			get { return Parameters.Get<string>("SectionIdVariable", "section_id"); }
		}

		public string ElementIdVariable
		{
			get { return Parameters.Get<string>("ElementIdVariable", "element_id"); }
		}

		public string PageIdVariable
		{
			get { return Parameters.Get<string>("PageIdVariable", "page"); }
		}

		public string ShowAllVariable
		{
			get { return Parameters.Get<string>("ShowAllVariable", "showall"); }
		}

		public string SefFolder
		{
			get { return Parameters.Get<string>("SEFFolder", String.Empty); }
		}

		public string ElementDetailTemplate
		{
			get { return Parameters.Get<string>("ElementDetailTemplate", "/#SectionId#/item-#ElementId#/"); }
		}

		public string ElementListTemplate
		{
			get { return Parameters.Get<string>("ElementListTemplate", "/#SectionId#/"); }
		}

		public string CommonListPageTemplate
		{
			get { return Parameters.Get<string>("CommonListPageTemplate", "/page-#pageId#"); }
		}

		public string CommonListShowAllTemplate
		{
			get { return Parameters.Get<string>("CommonListShowAllTemplate", "/all"); }
		}

		public string SectionListPageTemplate
		{
			get { return Parameters.Get<string>("SectionListPageTemplate", "/#SectionId#/page-#pageId#"); }
		}

		public string SectionListShowAllTemplate
		{
			get { return Parameters.Get<string>("SectionListShowAllTemplate", "/#SectionId#/all"); }
		}

        private string elementDetailCommentReadTemplate = null;
        public string ElementDetailCommentReadTemplate
        {
            get { return this.elementDetailCommentReadTemplate ?? (this.elementDetailCommentReadTemplate = Parameters.Get("ElementDetailCommentReadTemplate", "/#SectionId#/item-#ElementId#/comment-#CommentId#/")); }
        }

        private string elementDetailCommentOperationTemplate = null;
        public string ElementDetailCommentOperationTemplate
        {
            get { return this.elementDetailCommentOperationTemplate ?? (this.elementDetailCommentOperationTemplate = Parameters.Get("ElementDetailCommentOperationTemplate", "/#SectionId#/item-#ElementId#/comment-#CommentId#/act-#Operation#/")); }
        }

        private string elementDetailCommentPageTemplate = null;
        public string ElementDetailCommentPageTemplate
        {
            get { return this.elementDetailCommentPageTemplate ?? (this.elementDetailCommentPageTemplate = Parameters.Get("ElementDetailCommentPageTemplate", "/#SectionId#/item-#ElementId#/page-#PageID#/")); }
        }

        private string comparisonResultTemplate = null;
        public string ComparisonResultTemplate
        {
            get { return this.comparisonResultTemplate ?? (this.comparisonResultTemplate = Parameters.Get("ComparisonResultTemplate", "/compare")); }
        }

        private string commentVariable = null;
        public string CommentVariable
        {
            get { return this.commentVariable ?? (this.commentVariable = Parameters.Get("CommentVariable", "comment")); } 
        }

        private string commentOperationVariable = null;
        public string CommentOperationVariable
        {
            get { return this.commentOperationVariable ?? (this.commentOperationVariable = Parameters.Get("CommentOperationVariable", "act")); } 
        }

        private string commentPageVariable = null;
        public string CommentPageVariable
        {
            get { return this.commentPageVariable ?? (this.commentPageVariable = Parameters.Get("CommentPageVariable", "page")); } 
        }
		public string RootSectionTitle
		{
			get { return Parameters.GetString("RootSectionTitle", ""); }
		}

		private string menuItemPermisson = String.Empty;
		public string MenuItemPermisson
		{
			set { menuItemPermisson = value; }
			get { return menuItemPermisson; }
		}

        private bool? allowComments = null;
        /// <summary>
        /// Разрешить комментарии
        /// </summary>
        public bool AllowComments
        {
            get { return (this.allowComments ?? (this.allowComments = Parameters.GetBool("AllowComments", false))).Value; }
            set { Parameters["AllowComments"] = (this.allowComments = value).Value.ToString(); }
        }

		private int? commentsForumId = null;
		public int CommentsForumId
		{
			get { return (this.commentsForumId ?? (this.commentsForumId = Parameters.GetInt("CommentsForumId", 0))).Value; }
			set { Parameters["CommentsForumId"] = (this.commentsForumId = value).Value.ToString(); }
		}

		private bool AboutCommentsShow()
		{
			return AllowComments && CommentsForumId > 0;
		}

        private string CommentCustomPropertyName
        {
            get { return "_BXCommentForumTopicID"; }
        }

        private bool? allowComparison = null;
        /// <summary>
        /// Разрешить сравнение элементов
        /// </summary>
        public bool AllowComparison
        {
            get { return (this.allowComparison ?? (this.allowComparison = Parameters.GetBool("AllowComparison", false))).Value; }
            set { Parameters["AllowComparison"] = (this.allowComparison = value).Value.ToString(); }
        }

		protected void Page_Load(object sender, EventArgs e)
		{
            if (AboutCommentsShow())
                ComponentCache["_BXCommentCustomProperty"] = CommentCustomPropertyName;

			string page = "index";
			if (EnableSef)
			{
				BXParamsBag<string> urlToPage = new BXParamsBag<string>();

				urlToPage["index.page"] = CommonListPageTemplate;
				urlToPage["index.all"] = CommonListShowAllTemplate;
				urlToPage["list"] = ElementListTemplate;
				urlToPage["list.page"] = SectionListPageTemplate;
				urlToPage["list.all"] = SectionListShowAllTemplate;
				urlToPage["detail"] = ElementDetailTemplate;
                if(AllowComparison)
                    urlToPage["compare"] = ComparisonResultTemplate;

                BXParamsBag<object> replaceItems = new BXParamsBag<object>();
                replaceItems["SectionId"] = string.Empty;

                if (AboutCommentsShow())
                {
                    urlToPage["detail.comment.read"] = ElementDetailCommentReadTemplate;
                    urlToPage["detail.sectionless.comment.read"] = MakeLink(ElementDetailCommentReadTemplate, replaceItems).Replace("//", "/");
                    urlToPage["detail.comment.operation"] = ElementDetailCommentOperationTemplate;
                    urlToPage["detail.sectionless.comment.operation"] = MakeLink(ElementDetailCommentOperationTemplate, replaceItems).Replace("//", "/");
                    urlToPage["detail.comment.page"] = ElementDetailCommentPageTemplate;
                    urlToPage["detail.sectionless.comment.page"] = MakeLink(ElementDetailCommentPageTemplate, replaceItems).Replace("//", "/");
                }

				//string code = MapVariable(SefFolder, urlToPage, ComponentCache, "index");
				string code = BXSefUrlUtility.MapVariable(SefFolder, urlToPage, ComponentCache, "index", null, null);

				int position = code.IndexOf('.');
				if (position > 0)
					page = code.Substring(0, position);
				else
					page = code;

				ParseVariableAliases(ComponentCache);

				if (page == "list" && (ComponentCache.Get<int>("SectionId") > 0 || !String.IsNullOrEmpty(ComponentCache.GetString("SectionCode"))))
				{
					
					ComponentCache["PagingIndexTemplate"] = CombineLink(SefFolder, ElementListTemplate);
					ComponentCache["PagingPageTemplate"] = CombineLink(SefFolder, SectionListPageTemplate);
					ComponentCache["PagingShowAllTemplate"] = CombineLink(SefFolder, SectionListShowAllTemplate);
				}
				else
				{
					ComponentCache["PagingIndexTemplate"] = CombineLink(SefFolder, String.Empty);
					ComponentCache["PagingPageTemplate"] = CombineLink(SefFolder, CommonListPageTemplate);
					ComponentCache["PagingShowAllTemplate"] = CombineLink(SefFolder, CommonListShowAllTemplate);

                    if (string.Equals(page, "detail", StringComparison.Ordinal) && AboutCommentsShow())
                    {
                        string action = ComponentCache.Get("Operation", string.Empty);
                        if (!string.IsNullOrEmpty(action))
                            ComponentCache["CommentOperation"] = action;

                        string pageId = ComponentCache.Get("PageID", string.Empty);
                        if (!string.IsNullOrEmpty(pageId))
                            ComponentCache["CommentPage"] = pageId;

                        int elementId = ComponentCache.GetInt("ElementId", 0),
                            sectionId = ComponentCache.GetInt("SectionId", 0);
                        if (elementId > 0)
                        {
                            BXIBlockElementCollection elementCol = BXIBlockElement.GetList(
                                new BXFilter(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId)),
                                null,
                                new BXSelect(BXSelectFieldPreparationMode.Normal, BXIBlockElement.Fields.CreatedBy),
                                null
                                );
                            if (elementCol.Count > 0)
                                ComponentCache["CommentOriginAuthorId"] = elementCol[0].CreatedBy;
                        }

                        replaceItems["SectionId"] = sectionId.ToString();
                        replaceItems["ElementId"] = elementId.ToString();

                        ComponentCache["CommentReadUrlTemplate"] = CombineLink(SefFolder, MakeLink(ElementDetailCommentReadTemplate, replaceItems)).Replace("//", "/");
                        ComponentCache["CommentOperationUrlTemplate"] = CombineLink(SefFolder, MakeLink(ElementDetailCommentOperationTemplate, replaceItems)).Replace("//", "/");
                        ComponentCache["CommentReturnUrl"] = ComponentCache["CommentPageIndexTemplate"] = CombineLink(SefFolder, MakeLink(ElementDetailTemplate, replaceItems)).Replace("//", "/");
                        ComponentCache["CommentPageTemplate"] = CombineLink(SefFolder, MakeLink(ElementDetailCommentPageTemplate, replaceItems)).Replace("//", "/");
                    }
				}

				ComponentCache["IndexTemplate"] = CombineLink(SefFolder, String.Empty);
				ComponentCache["ShowAll"] = (code == "index.all" || code == "list.all" ? "y" : "n");
				ComponentCache["SectionElementListUrl"] = CombineLink(SefFolder, ElementListTemplate);
				ComponentCache["ElementDetailUrl"] = CombineLink(SefFolder, ElementDetailTemplate);
				ComponentCache["CompareResultUrl"] = CombineLink(SefFolder, ComparisonResultTemplate);
			}
			else
			{
				int sectionId = 0, elementId = 0;
                if(AllowComparison && string.Equals(Request["compare"], "Y", StringComparison.OrdinalIgnoreCase))
                    page = "compare";
				else if (Request[ElementIdVariable] != null && int.TryParse(Request[ElementIdVariable], out elementId) && elementId > 0)
					page = "detail";
				else if (Request[SectionIdVariable] != null && int.TryParse(Request[SectionIdVariable], out sectionId) && sectionId > 0)
					page = "list";
				else
					page = "index";

				BXParamsBag<string> variableAlias = new BXParamsBag<string>();
				variableAlias["SectionId"] = SectionIdVariable;
				variableAlias["ElementId"] = ElementIdVariable;
				variableAlias["PageId"] = PageIdVariable;
				variableAlias["ShowAll"] = ShowAllVariable;

				MapVariable(variableAlias, ComponentCache);

				string filePath = BXSefUrlManager.CurrentUrl.AbsolutePath;
				if (page == "list")
				{
					ComponentCache["PagingIndexTemplate"] = String.Format("{0}?{1}={2}", filePath, SectionIdVariable, sectionId);
					ComponentCache["PagingPageTemplate"] = String.Format("{0}?{1}={2}&{3}=#PageId#", filePath, SectionIdVariable, sectionId, PageIdVariable);
					ComponentCache["PagingShowAllTemplate"] = String.Format("{0}?{1}={2}&{3}=y", filePath, SectionIdVariable, sectionId, ShowAllVariable);
				}
				else
				{
					ComponentCache["PagingIndexTemplate"] = filePath;
					ComponentCache["PagingPageTemplate"] = String.Format("{0}?{1}=#PageId#", filePath, PageIdVariable);
					ComponentCache["PagingShowAllTemplate"] = String.Format("{0}?{1}=y", filePath, ShowAllVariable);

                    if (page == "detail" && AboutCommentsShow())
                    {
                        string action = Request.QueryString[CommentOperationVariable];
                        if (!string.IsNullOrEmpty(action))
                            ComponentCache["CommentOperation"] = action;

                        string comment = Request.QueryString[CommentVariable];
                        if (!string.IsNullOrEmpty(comment))
                            ComponentCache["CommentId"] = comment;

                        string commentPage = Request.QueryString[CommentPageVariable];
                        if (!string.IsNullOrEmpty(commentPage))
                            ComponentCache["CommentPage"] = commentPage;

                        if (elementId > 0)
                        {
                            BXIBlockElementCollection elementCol = BXIBlockElement.GetList(
                                new BXFilter(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId)),
                                null,
                                new BXSelect(BXSelectFieldPreparationMode.Normal, BXIBlockElement.Fields.CreatedBy),
                                null
                                );
                            if (elementCol.Count > 0)
                                ComponentCache["CommentOriginAuthorId"] = elementCol[0].CreatedBy;
                        }

                        ComponentCache["CommentReadUrlTemplate"] = string.Format("{0}?{1}={2}&{3}={4}&{5}=#CommentId#", filePath, SectionIdVariable, sectionId, ElementIdVariable, elementId, CommentVariable);
                        ComponentCache["CommentOperationUrlTemplate"] = string.Format("{0}?{1}={2}&{3}={4}&{5}=#CommentId#&{6}=#Operation#", filePath, SectionIdVariable, sectionId, ElementIdVariable, elementId, CommentVariable, CommentOperationVariable);
                        ComponentCache["CommentReturnUrl"] = string.Format("{0}?{1}={2}&{3}={4}", filePath, SectionIdVariable, sectionId, ElementIdVariable, elementId);
                        ComponentCache["CommentPageIndexTemplate"] = ComponentCache["CommentReturnUrl"];
                        ComponentCache["CommentPageTemplate"] = string.Format("{0}?{1}={2}&{3}={4}&{5}=#PageID#", filePath, SectionIdVariable, sectionId, ElementIdVariable, elementId, CommentPageVariable);
                    }
				}

				ComponentCache["IndexTemplate"] = filePath;
				ComponentCache["SectionElementListUrl"] = String.Format("{0}?{1}=#SectionId#", filePath, SectionIdVariable);
				ComponentCache["ElementDetailUrl"] = String.Format("{0}?{1}=#SectionId#&{2}=#ElementId#", filePath, SectionIdVariable, ElementIdVariable);
                ComponentCache["CompareResultUrl"] = String.Format("{0}?compare=Y", filePath);
			}
			
			if (page == "index" && Parameters.Get<bool>("ShowTopElements", false))
				page = "index_top";

			string rootSectionTitle = RootSectionTitle;
			if (string.IsNullOrEmpty(rootSectionTitle))
				rootSectionTitle = Page.Title;
			ComponentCache["RootSectionTitle"] = rootSectionTitle;

			IncludeComponentTemplate(page);
		}

		public override void ProcessMessage(BXCommand cmd, bool executionContext, string executionVirtualPath)
		{
			#region Search
			if (cmd.Action == "Bitrix.Search.ProvideUrl")
			{
                string moduleId = cmd.Parameters.Get("moduleId", string.Empty);
                if (string.Equals(moduleId, "forum", StringComparison.OrdinalIgnoreCase)
                    && AboutCommentsShow()
                    && BXModuleManager.IsModuleInstalled("Forum"))
                {
                    //Поиск по комментариям к эл-там каталога
                    int blockId = IBlockId;
                    int forumId = cmd.Parameters.GetInt("itemGroup", 0);
                    int postId = cmd.Parameters.GetInt("itemId", 0);
                    int topicId = cmd.Parameters.GetInt("param1");


                    if (blockId == 0 || !string.Equals(moduleId, "forum", StringComparison.OrdinalIgnoreCase) || forumId <= 0 || postId <= 0)
                        return;

                    BXSchemeFieldBase commentCpField =  BXIBlockElement.Fields.CustomFields[blockId].GetFieldByKey(CommentCustomPropertyName);
                    if (commentCpField == null)
                        return;

                    BXFilter f = new BXFilter(
                            new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                            new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, "Y"),
                            new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, blockId),
                            new BXFilterItem(commentCpField, BXSqlFilterOperators.Equal, topicId)
                            );

                    string siteId = DesignerSite;
                    if (!string.IsNullOrEmpty(siteId))
                        f.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.Sites.SiteId, BXSqlFilterOperators.Equal, siteId));

                    BXIBlockElementCollection c = BXIBlockElement.GetList(f, null);
                    if (c.Count == 0)
                        return;

                    BXIBlockElement el = c[0];
                    //формируем постоянную ссылку на комментарий
                    string commentUrl = string.Empty;
                    if (!EnableSef)
                        commentUrl = string.Format("{0}?{1}={2}&{3}={4}#comment{4}", BXSite.GetUrlForPath(executionVirtualPath, null), ElementIdVariable, el.Id.ToString(), CommentVariable, postId.ToString());
                    else
                    {
                        BXParamsBag<object> commentSefParams = new BXParamsBag<object>();
                        commentSefParams["SectionId"] = string.Empty;
                        commentSefParams["ElementId"] = el.Id.ToString();
                        commentSefParams["CommentId"] = postId.ToString();
                        string t = ElementDetailCommentReadTemplate;
                        if (t.IndexOf("##") < 0)
                            t = string.Concat(t, "##comment#CommentId#");
                        commentUrl = BXSefUrlUtility.MakeLink(CombineLink(SefFolder, t), commentSefParams).Replace("//", "/");
                    }
                    cmd.AddCommandResult(string.Concat("bitrix:forum@", executionVirtualPath), new BXCommandResult(BXCommandResultType.Ok, commentUrl));
                    return;
                }

				if (cmd.Parameters.Get<string>("moduleId") != "iblock")
					return;

				if (IBlockId < 0 || cmd.Parameters.Get<int>("itemGroup") != IBlockId)
					return;

				int elementId = cmd.Parameters.Get("itemId", -1);
				if (elementId < 0)
					return;

				BXFilter elementFilter = new BXFilter(
					new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
					new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
					new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId)
				);

				BXIBlockElementCollection element = BXIBlockElement.GetList(elementFilter, null);
				if (element != null && element.Count < 1)
					return;

				BXIBlockElement iblockElement = element[0];
				int replaceSectionId = 0;
				if (iblockElement.Sections.Count != 0)
					replaceSectionId = iblockElement.Sections[0].SectionId;

				string url;
				if (EnableSef)
				{
					string replaceSectionCode = String.Empty;
					if (ElementDetailTemplate.Contains("#SECTION_CODE#") || ElementDetailTemplate.Contains("#SectionCode#"))
					{
						BXIBlockSection elementSection = BXIBlockSection.GetById(replaceSectionId);
						if (elementSection != null)
							replaceSectionCode = elementSection.Code;
					}

					BXParamsBag<object> replaceItems = new BXParamsBag<object>();
					replaceItems.Add("IblockId", iblockElement.IBlockId);
					replaceItems.Add("IBLOCK_ID", iblockElement.IBlockId);
					replaceItems.Add("ELEMENT_ID", iblockElement.Id);
					replaceItems.Add("ElementId", iblockElement.Id);
					replaceItems.Add("ElementCode", iblockElement.Code);
					replaceItems.Add("ELEMENT_CODE", iblockElement.Code);
					replaceItems.Add("SectionId", replaceSectionId);
					replaceItems.Add("SECTION_ID", replaceSectionId);
					replaceItems.Add("SectionCode", replaceSectionCode);
					replaceItems.Add("SECTION_CODE", replaceSectionCode);

					//url = MakeLink(SefFolder, ElementDetailTemplate, replaceItems);
					url = CombineLink(SefFolder, BXSefUrlUtility.MakeLink(ElementDetailTemplate, replaceItems));
				}
				else
				{
					url = String.Format("{0}?{1}={2}&{3}={4}", BXSite.GetUrlForPath(executionVirtualPath, null), SectionIdVariable, replaceSectionId, ElementIdVariable, elementId);
				}

				cmd.AddCommandResult("bitrix:catalogue@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, url));
			}
			#endregion

			if (cmd.Action == "Bitrix.Main.GeneratePublicMenu")
			{
				//return;
				//Совпадает ли тип меню в параметрах компонента с типом, который запрашивает система.
				if (!Parameters.Get("GenerateMenuType", "left").Equals(cmd.Parameters.Get<string>("menuType"), StringComparison.InvariantCultureIgnoreCase))
					return;

				//Генерируем меню только для тех адресов, которые выводит сам компонент.
				if (!EnableSef && !BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath).Equals(cmd.Parameters.Get<string>("uriPath"), StringComparison.InvariantCultureIgnoreCase))
					return;
				else if (EnableSef && !cmd.Parameters.Get<string>("uriPath").StartsWith(Bitrix.IO.BXPath.ToVirtualRelativePath(SefFolder.TrimStart('\\', '/')) + "/", StringComparison.InvariantCultureIgnoreCase))
					return;

				if (IBlockId < 0)
					return;

				BXParamsBag<object> request = new BXParamsBag<object>();
				if (EnableSef)
				{
					BXParamsBag<string> urlToPage = new BXParamsBag<string>();
					urlToPage["index.page"] = CommonListPageTemplate;
					urlToPage["index.all"] = CommonListShowAllTemplate;
					urlToPage["list"] = ElementListTemplate;
					urlToPage["list.page"] = SectionListPageTemplate;
					urlToPage["list.all"] = SectionListShowAllTemplate;
					urlToPage["detail"] = ElementDetailTemplate;

					//MapVariable(SefFolder, urlToPage, request, "index", BXUri.ToRelativeUri(cmd.Parameters.Get<string>("uri")));
					Uri url = new Uri(BXUri.ToAbsoluteUri(cmd.Parameters.Get<string>("uri")));
					BXSefUrlUtility.MapVariable(SefFolder, urlToPage, request, "index", url, null);

					ParseVariableAliases(request);
				}
				else
				{
					BXParamsBag<string> variableAlias = new BXParamsBag<string>();
					variableAlias["SectionId"] = SectionIdVariable;
					variableAlias["ElementId"] = ElementIdVariable;
					variableAlias["PageId"] = PageIdVariable;
					variableAlias["ShowAll"] = ShowAllVariable;

					MapVariable(variableAlias, request, BXUri.ToRelativeUri(cmd.Parameters.Get<string>("uri")));
				}

				#region Menu Item Permission
				string permissionCacheKey = "catalogue-" + IBlockId.ToString() + "-menu-permission";
				if ((menuItemPermisson = (string)BXCacheManager.MemoryCache.Get(permissionCacheKey)) == null)
				{
					menuItemPermisson = String.Empty;
					StringBuilder menuItemRoles = new StringBuilder();

					BXRoleCollection iblockRoles = BXRoleManager.GetAllRolesForOperation("IBlockRead", "iblock", IBlockId.ToString());
					foreach (BXRole role in iblockRoles)
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
						menuItemPermisson = menuItemRoles.ToString();

					BXCacheManager.MemoryCache.Insert(permissionCacheKey, menuItemPermisson);
				}
				#endregion

				int elementId = 0, sectionId = 0;
				string elementCode = String.Empty, sectionCode = String.Empty;

				elementId = request.Get<int>("ElementId", elementId);
				sectionId = request.Get<int>("SectionId", sectionId);
				elementCode = request.Get<string>("ElementCode", elementCode);
				sectionCode = request.Get<string>("SectionCode", sectionCode);

				string parentLevelUri = null;
				List<BXPublicMenuItem> menuList = null;

				//Указан идентификатор или символьный код
				if (elementId > 0 || !String.IsNullOrEmpty(elementCode))
				{
					BXFilter elementFilter = new BXFilter(
						new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
						new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y")
					);

					if (elementId > 0)
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId));
					if (!String.IsNullOrEmpty(elementCode))
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Code, BXSqlFilterOperators.Equal, elementCode));
					if (sectionId > 0)
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, sectionId));
					if (!String.IsNullOrEmpty(sectionCode))
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.Code, BXSqlFilterOperators.Equal, sectionCode));

					BXIBlockElementCollection element = BXIBlockElement.GetList(elementFilter, null);
					if (element != null && element.Count > 0)
					{
						BXIBlockElement.BXInfoBlockElementSectionCollection sections = element[0].Sections;
						if (sections != null && sections.Count > 0)
						{
							BXParamsBag<object> replaceItems = new BXParamsBag<object>();
							replaceItems["SECTION_ID"] = sections[0].SectionId;
							replaceItems["SectionId"] = replaceItems["SECTION_ID"];
							replaceItems["SECTION_CODE"] = GetSectionCode(sections[0].SectionId);
							replaceItems["SectionCode"] = replaceItems["SECTION_CODE"];

							parentLevelUri = MakeMenuUri(executionVirtualPath, replaceItems); //Меню строится для раздела, к которому привязан элемент
						}
					}
				}

				//Если ничего не указано выводим все дерево
				Dictionary<string, BXPublicMenu.BXLoadMenuCommandParameter> menuTree = new Dictionary<string, BXPublicMenu.BXLoadMenuCommandParameter>();
				if (parentLevelUri == null && menuList == null)
				{
					BXIBlockSectionCollection sectionsList = BXIBlockSection.GetList(
						new BXFilter(
							new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
							new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
							new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId)
						),
						new BXOrderBy(
							new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)
						),
						null,
						null,
						BXTextEncoder.EmptyTextEncoder
					);

					int previousDepthLevel = 1;
					int previousSectionId = 0;
					Dictionary<int, MenuSectionTree> sectionTree = new Dictionary<int, MenuSectionTree>();
					foreach (BXIBlockSection section in sectionsList)
					{
						BXParamsBag<object> replaceItems = new BXParamsBag<object>();
						replaceItems["SECTION_ID"] = section.Id;
						replaceItems["SECTION_CODE"] = section.Code ?? String.Empty;
						replaceItems["SectionId"] = replaceItems["SECTION_ID"];
						replaceItems["SectionCode"] = replaceItems["SECTION_CODE"];

						sectionTree[section.Id] = new MenuSectionTree();
						string url = MakeMenuUri(executionVirtualPath, replaceItems);
						sectionTree[section.Id].url = url;
						sectionTree[section.Id].sectionId = section.SectionId;

						//Если предыдущий раздел не имеет дочерних разделов, то устанавливаем у него parentLevelUrl
						if (previousSectionId > 0 && section.DepthLevel <= previousDepthLevel)
							sectionTree[previousSectionId].parentLevelUrl = sectionTree[sectionTree[previousSectionId].sectionId].url;
						previousDepthLevel = section.DepthLevel;
						previousSectionId = section.Id;

						BXPublicMenuItem menuItem = new BXPublicMenuItem();
						menuItem.Title = section.Name;
						menuItem.Links.Add(url);
						menuItem.Sort = section.LeftMargin;

						menuItem.ConditionType = ConditionType.Group;
						menuItem.Condition = menuItemPermisson;

						if (!sectionTree.ContainsKey(section.SectionId))
						{
							sectionTree[section.SectionId] = new MenuSectionTree();
							sectionTree[section.SectionId].menuItems = new List<BXPublicMenuItem>();

							if (section.SectionId < 1)
								sectionTree[section.SectionId].url = EnableSef ? BXPath.ToVirtualRelativePath(BXPath.TrimStart(SefFolder)) : BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath);
						}

						if (sectionTree[section.SectionId].menuItems == null)
							sectionTree[section.SectionId].menuItems = new List<BXPublicMenuItem>();

						sectionTree[section.SectionId].menuItems.Add(menuItem);
					}

					//Последний элемент не может иметь дочерних элементов, поэтому указываем parentLevelUrl
					if (sectionTree.ContainsKey(previousSectionId) && sectionTree.ContainsKey(sectionTree[previousSectionId].sectionId))
						sectionTree[previousSectionId].parentLevelUrl = sectionTree[sectionTree[previousSectionId].sectionId].url;

					foreach (KeyValuePair<int, MenuSectionTree> sectionItem in sectionTree)
					{
						if (!menuTree.ContainsKey(sectionItem.Value.url))
						{
							menuTree.Add(
								sectionItem.Value.url,
								new BXPublicMenu.BXLoadMenuCommandParameter(sectionItem.Value.menuItems, true, sectionItem.Value.parentLevelUrl)
							);
						}
					}
				}

				if (menuTree.Count > 0)
					cmd.AddCommandResult("bitrix:catalogue@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, menuTree));
				else if (menuList != null || parentLevelUri != null)
				{
					BXPublicMenu.BXLoadMenuCommandParameter menuResult = new BXPublicMenu.BXLoadMenuCommandParameter(menuList, true, parentLevelUri);
					cmd.AddCommandResult("bitrix:catalogue@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, menuResult));
				}
			}
		}

		private void ParseVariableAliases(BXParamsBag<object> result)
		{
			string[,] variableAlias = {
					{"SectionId", "SECTION_ID"},
					{"SectionCode", "SECTION_CODE"},
					{"ElementId", "ELEMENT_ID"},
					{"ElementCode", "ELEMENT_CODE"},
					{"IblockId", "IBLOCK_ID"}
				};

			for (int i = 0, length = variableAlias.GetLength(0); i < length; i++)
				if (result.ContainsKey(variableAlias[i, 1]))
					result[variableAlias[i, 0]] = result.Get<string>(variableAlias[i, 1], String.Empty);
		}

		private class MenuSectionTree
		{
			public string parentLevelUrl = null;
			public List<BXPublicMenuItem> menuItems;
			public string url;
			public int sectionId;
		}

		private List<BXPublicMenuItem> ExtractMenuItemsFromCollection(BXIBlockSectionCollection sectionsList, string executionVirtualPath)
		{
			if (sectionsList == null || sectionsList.Count < 1)
				return null;

			List<BXPublicMenuItem> menuList = new List<BXPublicMenuItem>();
			foreach (BXIBlockSection section in sectionsList)
			{
				BXPublicMenuItem menuItem = new BXPublicMenuItem();
				menuItem.Title = section.Name;

				BXParamsBag<object> replaceItems = new BXParamsBag<object>();
				replaceItems["SECTION_ID"] = section.Id;
				replaceItems["SECTION_CODE"] = section.Code ?? String.Empty;
				replaceItems["SectionId"] = replaceItems["SECTION_ID"];
				replaceItems["SectionCode"] = replaceItems["SECTION_CODE"];

				menuItem.Links.Add(MakeMenuUri(executionVirtualPath, replaceItems));
				menuItem.Sort = section.LeftMargin;
				menuItem.ConditionType = ConditionType.Group;
				menuItem.Condition = menuItemPermisson;
				menuList.Add(menuItem);
			}

			return menuList;
		}

		private string MakeMenuUri(string executionVirtualPath, BXParamsBag<object> replaceItems)
		{
			string url;

			replaceItems["IBLOCK_ID"] = IBlockId;
			replaceItems["IblockId"] = IBlockId;

			if (EnableSef)
			{
				if (SefFolder == "/" || SefFolder == String.Empty)
					url = Bitrix.IO.BXPath.TrimStart(ElementListTemplate);					
				else
					url = Bitrix.IO.BXPath.Trim(SefFolder) + "/" + Bitrix.IO.BXPath.TrimStart(ElementListTemplate);

				url = BXPath.Combine(BXPath.Trim(SefFolder), BXPath.Trim(ElementListTemplate));	

				url = BXSefUrlUtility.MakeLink(url, replaceItems);
				//url = MakeLink(url, replaceItems);
			}
			else
			{
				url = MakeLink(String.Format("{0}?{1}=#SectionId#", BXSiteRemapUtility.UnmapVirtualPath(BXPath.CutDefaultPage(executionVirtualPath)), SectionIdVariable), replaceItems);
			}

			return url;
		}

		private string GetSectionCode(int sectionId)
		{
			string sectionCode = String.Empty;

			if (sectionId > 0 && EnableSef && (ElementListTemplate.Contains("#SECTION_CODE#") || ElementListTemplate.Contains("#SectionCode#")))
			{
				BXIBlockSectionCollection section = BXIBlockSection.GetList(
					new BXFilter(
						new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.Equal, sectionId),
						new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId)
					),
					null
				);

				if (section != null && section.Count > 0)
					sectionCode = section[0].Code;
			}

			return sectionCode;
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Group = new BXComponentGroup("catalogue", GetMessage("Group"), 10, BXComponentGroup.Content);
			Icon = "images/catalog.gif";

			BXCategory mainCategory = BXCategory.Main,
                listSettings = BXCategory.ListSettings,
                detailSettings = BXCategory.DetailSettings,
                additionalSettings = BXCategory.AdditionalSettings,
                treeSettings = new BXCategory(GetMessageRaw("SectionTreeCategory"), "treeSettings", 850),
                topSettings = new BXCategory(GetMessageRaw("TopSectionSortBy.Category"), "topSettings", 900),
                sef = BXCategory.Sef,
                commentCategory = new BXCategory(GetMessageRaw("CommentSettings"), "CommentSettings", 260),
                votingCategory = new BXCategory(GetMessage("Category.Voting"), "Voting", 220),
                comparisonCategory = new BXCategory(GetMessageRaw("Category.Comparison"), "Comparison", 230),
                stockCatalogCategory = new BXCategory(GetMessageRaw("Category.StockCatalog"), "StockCatalog", 240);

			ParamsDefinition.Add(
				"IBlockTypeId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockType"),
					String.Empty,
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"IBlockId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockCode"),
					String.Empty,
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"ListSortBy",
				new BXParamSingleSelection(
					GetMessageRaw("SortBy"),
					"Id",
					listSettings
				)
			);

			ParamsDefinition.Add(
				"ListSortOrder",
				new BXParamSort(
					GetMessageRaw("SortOrder"),
					true,
					listSettings
				)
			);

			ParamsDefinition.Add(
				"ShowSubElements",
				new BXParamYesNo(
					GetMessageRaw("DisplaySubsectionElements"),
					true,
					listSettings
				));

			ParamsDefinition.Add(
				"ShowAllElementsOnIndex",
				new BXParamYesNo(
					GetMessageRaw("ShowAllElementsOnIndex"),
					true,
					listSettings

			    ));

            ParamsDefinition.Add("ActiveFilter", new BXParamSingleSelection(GetMessageRaw("ActiveFilter"), "Active", listSettings));
            ParamsDefinition.Add("ActiveDateFilter", new BXParamSingleSelection(GetMessageRaw("ActiveDateFilter"), "All", listSettings));

            ParamsDefinition.Add("ListFilterByCustomProperty", //FilterByElementCustomProperty
                new BXParamYesNo(
                    GetMessageRaw("ListFilterByCustomProperty"),
                    false,
                    listSettings,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "ListFilterByCustomProperty", "ListFilterByCustomProperty", string.Empty)       
                ));

            ParamsDefinition.Add("ListCustomPropertyFilterSettings", //ElementCustomPropertyFilterSettings
				new BXParamCustomFieldFilter(
                    GetMessageRaw("ListCustomPropertyFilterSettings"),
					string.Empty,
                    listSettings,
					string.Empty,//BXIBlockModuleConfiguration,
                    new ParamClientSideActionGroupViewMember(ClientID, "ListCustomPropertyFilterSettings", new string[] { "ListFilterByCustomProperty" })
				));

			ParamsDefinition.Add(
				"ListProperties",
				new BXParamMultiSelection(
                    GetMessageRaw("ListProperties"),
					"-",
					listSettings
		        ));

			ParamsDefinition.Add(
				"DetailProperties",
				new BXParamMultiSelection(
                    GetMessageRaw("DetailProperties"),
					"-",
					detailSettings
				)
			);

			ParamsDefinition.Add(
				"PropertyKeywords",
				new BXParamSingleSelection(
					GetMessageRaw("SetPageKeywordsFromProperty"),
					"-",
					detailSettings
				)
			);

			ParamsDefinition.Add(
				"PropertyDescription",
				new BXParamSingleSelection(
					GetMessageRaw("SetPageDescriptionFromProperty"),
					"-",
					detailSettings
				)
			);

			ParamsDefinition.Add(
				"DepthLevel",
				new BXParamText(
					GetMessageRaw("SubsectionsDisplayDepth"),
					"2",
					treeSettings
				)
			);

			ParamsDefinition.Add(
				"ShowCounters",
				new BXParamYesNo(
					GetMessageRaw("DisplayQuantityOfElementsInSection"),
					false,
					treeSettings
				)
			);

			ParamsDefinition.Add(
				"IncludeParentSections",
				new BXParamYesNo(
					GetMessageRaw("IncludeParentSections"),
					true,
					treeSettings
				)
			);

			ParamsDefinition.Add(
				"AddAdminPanelButtons",
				new BXParamYesNo(
					GetMessageRaw("AddButtonsForThisComponentToAdminPanel"),
					false,
					additionalSettings
				)
			);

			ParamsDefinition.Add(
				"SetPageTitle",
				new BXParamYesNo(
					GetMessageRaw("SetPageTitle"),
					true,
					additionalSettings
				)
			);

			ParamsDefinition.Add(
				"RootSectionTitle",
				new BXParamText(
					GetMessageRaw("RootSectionTitle"),
					"",
					additionalSettings
				)
			);

			string clientSideActionGroupViewId = ClientID;

			ParamsDefinition.Add(
				 "ShowTopElements",
				 new BXParamYesNo(
					 GetMessageRaw("DisplayTopOfElements"),
					 false,
					 topSettings,
					 new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId, "ShowTopElements", "Top", "NonTop")
				)
			);

			ParamsDefinition.Add(
				"TopElementCount",
				new BXParamText(
					GetMessageRaw("ElementsPerPage"),
					"6",
					topSettings,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "TopElementCount", new string[] { "Top" })
				)
			);


			ParamsDefinition.Add(
				"TopSortBy",
				new BXParamSingleSelection(
					GetMessageRaw("SortBy"),
					"Id",
					topSettings,
					null,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "TopSortBy", new string[] { "Top" })
				)
			);

			ParamsDefinition.Add(
				"TopSortOrder",
				new BXParamSort(
					GetMessageRaw("SortOrder"),
					true,
					topSettings,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "TopSortOrder", new string[] { "Top" })
				)
			);

			ParamsDefinition.Add(
				"TopProperties",
				new BXParamMultiSelection(
					GetMessageRaw("Properties"),
					"-",
					topSettings,
					null,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "TopProperties", new string[] { "Top" })
				)
			);

			BXParamsBag<BXParam> SefParams = BXParametersDefinition.Sef;
			SefParams["EnableSEF"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId, "EnableSEF", "Sef", "NonSef");
			SefParams["SEFFolder"].ClientSideAction = new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEFFolder", new string[] { "Sef" });

			ParamsDefinition.Add(SefParams);

			ParamsDefinition.Add(
				"SectionIdVariable",
				new BXParamText(
					GetMessageRaw("SectionID"),
					"section_id",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SectionIdVariable", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ElementIdVariable",
				new BXParamText(
					GetMessageRaw("ElementID"),
					"element_id",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ElementIdVariable", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"PageIdVariable",
				new BXParamText(
					GetMessageRaw("PageIdVariable"),
					"page",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "PageIdVariable", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ShowAllVariable",
				new BXParamText(
					GetMessageRaw("ShowAllVariable"),
					"showall",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ShowAllVariable", new string[] { "NonSef" })
				)
			);

            ParamsDefinition.Add(
                "CommentVariable",
                new BXParamText(
                    GetMessageRaw("CommentVariable"),
                    "comment",
                    sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentVariable", new string[] { "NonSef" })
                )
            );

            ParamsDefinition.Add(
                "CommentOperationVariable",
                new BXParamText(
                    GetMessageRaw("CommentOperationVariable"),
                    "act",
                    sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentOperationVariable", new string[] { "NonSef" })
                )
            );

            ParamsDefinition.Add(
                "CommentPageVariable",
                new BXParamText(
                    GetMessageRaw("CommentPageVariable"),
                    "page",
                    sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentPageVariable", new string[] { "NonSef" })
                )
            );

			ParamsDefinition.Add(
				"ElementDetailTemplate",
				new BXParamText(
					GetMessageRaw("ElementDetailTemplate"),
					"/#SectionId#/item-#ElementId#/",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ElementDetailTemplate", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"ElementListTemplate",
				new BXParamText(
					GetMessageRaw("ElementListTemplate"),
					"/#SectionId#/",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ElementListTemplate", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"CommonListPageTemplate",
				new BXParamText(
					GetMessageRaw("CommonListPageTemplate"),
					"/page-#pageId#",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommonListPageTemplate", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"CommonListShowAllTemplate",
				new BXParamText(
					GetMessageRaw("CommonListShowAllTemplate"),
					"/all",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommonListShowAllTemplate", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"SectionListPageTemplate",
				new BXParamText(
					GetMessageRaw("SectionListPageTemplate"),
					"/#SectionId#/page-#pageId#",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SectionListPageTemplate", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"SectionListShowAllTemplate",
				new BXParamText(
					GetMessageRaw("SectionListShowAllTemplate"),
					"/#SectionId#/all",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SectionListShowAllTemplate", new string[] { "Sef" })
				)
			);

            ParamsDefinition.Add(
                "ElementDetailCommentReadTemplate",
                new BXParamText(
                    GetMessageRaw("ElementDetailCommentReadTemplate"),
                    "/#SectionId#/item-#ElementId#/comment-#CommentId#/",
                    sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ElementDetailCommentReadTemplate", new string[] { "Sef" })
                )
            );

            ParamsDefinition.Add(
                "ElementDetailCommentOperationTemplate",
                new BXParamText(
                    GetMessageRaw("ElementDetailCommentOperationTemplate"),
                    "/#SectionId#/item-#ElementId#/comment-#CommentId#/act-#Operation#/",
                    sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ElementDetailCommentOperationTemplate", new string[] { "Sef" })
                )
            );

            ParamsDefinition.Add(
                "ElementDetailCommentPageTemplate",
                new BXParamText(
                    GetMessageRaw("ElementDetailCommentPageTemplate"),
                    "/#SectionId#/item-#ElementId#/page-#PageID#/",
                    sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ElementDetailCommentPageTemplate", new string[] { "Sef" })
                )
            );

			ParamsDefinition.Add(
				"ComparisonResultTemplate",
				new BXParamText(
					GetMessageRaw("Param.ComparisonResultTemplate"),
					"/compare",
					sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ComparisonResultTemplate", new string[] { "ComparisonOn", "Sef" })
				)
			);

            ParamsDefinition.Add(
                "AllowComments",
                new BXParamYesNo(
                    GetMessageRaw("AllowComments"),
                    false,
                    commentCategory,
                    new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId, "AllowComments", "CommentsOn", "CommentsOff")
                    )
                );

            ParamsDefinition.Add(
                "CommentsPerPage",
                new BXParamText(
                    GetMessageRaw("CommentsPerPage"),
                    "5",
                    commentCategory,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentsPerPage", new string[] { "CommentsOn" })
                    )
                );

            ParamsDefinition.Add(
                "CommentsForumId",
                new BXParamSingleSelection(
                    GetMessageRaw("CommentsForumId"),
					"0",
                    commentCategory,
					null,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentsForumId", new string[] { "CommentsOn" })
                    )
                );

            ParamsDefinition.Add(
                "CommentAuthorProfileUrlTemplate",
                new BXParamText(
                    GetMessageRaw("CommentAuthorProfileUrlTemplate"),
                    string.Empty,
                    commentCategory,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentAuthorProfileUrlTemplate", new string[] { "CommentsOn" })
                    )
                );


            ParamsDefinition.Add(
                "DisplayEmailForGuestComment",
                new BXParamYesNo(
                    GetMessageRaw("DisplayEmailForGuestComment"),
                    true,
                    commentCategory,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "DisplayEmailForGuestComment", new string[] { "CommentsOn" })
                    )
                );

            ParamsDefinition.Add(
                "RequireEmailForGuestComment",
                new BXParamYesNo(
                    GetMessageRaw("RequireEmailForGuestComment"),
                    true,
                    commentCategory,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "RequireEmailForGuestComment", new string[] { "CommentsOn" })
                    )
                );

            ParamsDefinition.Add(
                "DisplayCaptchaForGuestComment",
                new BXParamYesNo(
                    GetMessageRaw("DisplayCaptchaForGuestComment"),
                    true,
                    commentCategory,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "DisplayCaptchaForGuestComment", new string[] { "CommentsOn" })
                    )
                );

            ParamsDefinition.Add(
                "CommentMaxWordLength",
                new BXParamText(
                    GetMessageRaw("CommentMaxWordLength"),
                    "15",
                    commentCategory,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentMaxWordLength", new string[] { "CommentsOn" })
                    )
                );

            ParamsDefinition.Add(
                "EnableVotingForElement", 
                new BXParamYesNo(
                    GetMessageRaw("EnableVotingForElement"), 
                    false, 
                    votingCategory)
                    );

            ParamsDefinition.Add(
                "RolesAuthorizedToVote", 
                new BXParamMultiSelection(
                    GetMessageRaw("RolesAuthorizedToVote"), 
                    "User", 
                    votingCategory
                    )
                );

            #region StockCatalog

            ParamsDefinition.Add(
                "DisplayStockCatalogData",
                new BXParamYesNo(
                    GetMessageRaw("Param.DisplayStockCatalogData"),
                    false,
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "DisplayStockCatalogData", "StockCatalog", string.Empty)

                ));

            ParamsDefinition.Add(
                "DisplayAllPriceTiers",
                new BXParamYesNo(
                    GetMessageRaw("Param.DisplayAllPriceTiers"),
                    false,
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "DisplayAllPriceTiers", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "PriceTypes",
                new BXParamMultiSelection(
                    GetMessageRaw("Param.PriceTypes"),
                    string.Empty,
                    stockCatalogCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "PriceTypes", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "InitQuantity",
                new BXParamText(
                    GetMessageRaw("Param.InitQuantity"),
                    "1",
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "InitQuantity", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "AcceptQuantity",
                new BXParamYesNo(
                    GetMessageRaw("Param.AcceptQuantity"),
                    false,
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "AcceptQuantity", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "IncludeVATInPrice",
                new BXParamYesNo(
                    GetMessageRaw("Param.IncludeVATInPrice"),
                    true,
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "IncludeVATInPrice", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "DisplayVAT",
                new BXParamYesNo(
                    GetMessageRaw("Param.DisplayVAT"),
                    false,
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "DisplayVAT", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "ShowCatalogItemProperties",
                new BXParamMultiSelection(
                    GetMessageRaw("Param.ShowCatalogItemProperties"),
                    string.Empty,
                    stockCatalogCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "ShowCatalogItemProperties", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "UserCartUrlTemplate",
                new BXParamText(
                    GetMessageRaw("Param.UserCartUrlTemplate"),
                    "personal/cart.aspx",
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "UserCartUrlTemplate", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "ActionParamName",
                new BXParamText(
                    GetMessageRaw("Param.ActionParamName"),
                    "ctlg_act",
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "ActionParamName", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "CatalogItemIdParamName",
                new BXParamText(
                    GetMessageRaw("Param.CatalogItemIdParamName"),
                    "ctlg_itm_id",
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "CatalogItemIdParamName", new string[] { "StockCatalog" })
                ));

            ParamsDefinition.Add(
                "CatalogItemQuantityParamName",
                new BXParamText(
                    GetMessageRaw("Param.CatalogItemQuantityParamName"),
                    "ctlg_itm_qty",
                    stockCatalogCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "CatalogItemQuantityParamName", new string[] { "StockCatalog" })
                ));
            #endregion

            #region Comarison
            ParamsDefinition.Add(
                "AllowComparison",
                new BXParamYesNo(
                    GetMessageRaw("Param.AllowComparison"),
                    false,
                    comparisonCategory,
                    new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId, "AllowComparison", "ComparisonOn", string.Empty)));

            ParamsDefinition.Add(
                "ComparisonSelectedFields",
                new BXParamMultiSelection(
                    GetMessageRaw("Param.ComparisonSelectedFields"), 
                    string.Empty, 
                    comparisonCategory, 
                    null, 
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ComparisonSelectedFields", new string[] { "ComparisonOn" })));
            #endregion

            ParamsDefinition.Add(BXParametersDefinition.Paging);
			ParamsDefinition.Add(BXParametersDefinition.Cache);
			ParamsDefinition.Add(BXParametersDefinition.Search);
			ParamsDefinition.Add(BXParametersDefinition.Ajax);

			if (!String.IsNullOrEmpty(DesignerSite))
				ParamsDefinition.Add(BXParametersDefinition.Menu(DesignerSite));
		}

		protected override void LoadComponentDefinition()
		{
			//Iblock type
			List<BXParamValue> typeParamValue = new List<BXParamValue>();
			typeParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockType"), ""));

			BXIBlockTypeCollection iblockTypes = BXIBlockType.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
			foreach (BXIBlockType iblockType in iblockTypes)
				typeParamValue.Add(new BXParamValue(iblockType.Translations[BXLoc.CurrentLocale].Name, iblockType.Id.ToString()));

			ParamsDefinition["IBlockTypeId"].Values = typeParamValue;
			ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;

			//Iblock
			int selectedIBlockType = 0;
			if (Parameters.ContainsKey("IBlockTypeId"))
				int.TryParse(Parameters["IBlockTypeId"], out selectedIBlockType);

			BXFilter filter = new BXFilter();
			if (selectedIBlockType > 0)
				filter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, selectedIBlockType));
			if (!String.IsNullOrEmpty(DesignerSite))
				filter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

			List<BXParamValue> iblockParamValue = new List<BXParamValue>();
			iblockParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockID"), "-"));
			BXIBlockCollection iblocks = BXIBlock.GetList(filter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
			foreach (BXIBlock iblock in iblocks)
				iblockParamValue.Add(new BXParamValue(iblock.Name, iblock.Id.ToString()));

			ParamsDefinition["IBlockId"].Values = iblockParamValue;
			ParamsDefinition["IBlockId"].RefreshOnDirty = true;

			//Properties
			int selectedIblockId = 0;
			if (Parameters.ContainsKey("IBlockId"))
				int.TryParse(Parameters["IBlockId"], out selectedIblockId);

			List<BXParamValue> sortProperties = new List<BXParamValue>();
			List<BXParamValue> properties = new List<BXParamValue>();
			properties.Add(new BXParamValue(GetMessageRaw("NotSelected"), "-"));

            BXCustomFieldCollection customFields = null;
			if (selectedIblockId > 0)
			{
                ((BXParamCustomFieldFilter)ParamsDefinition["ListCustomPropertyFilterSettings"]).EntityId = BXIBlock.GetCustomFieldsKey(selectedIblockId);

                foreach (BXCustomField customField in (customFields = BXIBlock.GetCustomFields(selectedIblockId)))
				{
					string title = BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel);
					string code = customField.Name.ToUpper();
					properties.Add(new BXParamValue(title, code));
					sortProperties.Add(new BXParamValue(title, "-" + code));
				}
			}

            ParamsDefinition["ListCustomPropertyFilterSettings"].Values = properties;
            ParamsDefinition["ListProperties"].Values = properties;
			ParamsDefinition["DetailProperties"].Values = properties;
			ParamsDefinition["PropertyKeywords"].Values = properties;
			ParamsDefinition["PropertyDescription"].Values = properties;
			ParamsDefinition["TopProperties"].Values = properties;

			//Sorting
			List<BXParamValue> sortingFields = new List<BXParamValue>();
			sortingFields.Add(new BXParamValue(GetMessageRaw("ElementID"), "ID"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("ElementName"), "Name"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("ActiveFromDate"), "ActiveFromDate"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("ActiveUntilDate"), "ActiveToDate"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("SortIndex"), "Sort"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("DateOfModification"), "UpdateDate"));
			sortingFields.AddRange(sortProperties);

            VotingFacility.PrepareSortingParamValues(sortingFields, this);

			ParamsDefinition["ListSortBy"].Values = sortingFields;
			ParamsDefinition["TopSortBy"].Values = sortingFields;

            ParamsDefinition["ActiveFilter"].Values = new List<BXParamValue>();
            ParamsDefinition["ActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.Active"), ActiveFilter.Active.ToString()));
            ParamsDefinition["ActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.NotActive"), ActiveFilter.NotActive.ToString()));
            ParamsDefinition["ActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.All"), ActiveFilter.All.ToString()));

            ParamsDefinition["ActiveDateFilter"].Values = new List<BXParamValue>();
            ParamsDefinition["ActiveDateFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveDateFilter.Active"), ActiveFilter.Active.ToString()));
            ParamsDefinition["ActiveDateFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveDateFilter.NotActive"), ActiveFilter.NotActive.ToString()));
            ParamsDefinition["ActiveDateFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveDateFilter.All"), ActiveFilter.All.ToString()));

            IList<BXParamValue> rolesValues = ParamsDefinition["RolesAuthorizedToVote"].Values;
            rolesValues.Clear();
            foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
            {
                if (string.Equals(r.RoleName, "Guest", StringComparison.Ordinal))
                    continue;
                rolesValues.Add(new BXParamValue(r.Title, r.RoleName));
            }

            if (Catalog != null)
            {
                List<BXParamValue> priceTypeValues = new List<BXParamValue>();
                foreach (CatalogPriceTypeInfo priceType in Catalog.GetPriceTypes())
                    priceTypeValues.Add(new BXParamValue(priceType.Name, priceType.Id.ToString()));
                ParamsDefinition["PriceTypes"].Values = priceTypeValues;
            }

            if (selectedIblockId > 0)
            {
                List<BXParamValue> fieldValues = new List<BXParamValue>();
                for (int i = 0; i < CatalogItemCustomFields.Length; i++)
                    fieldValues.Add(new BXParamValue(CatalogItemCustomFields[i].EditFormLabel, CatalogItemCustomFields[i].Name.ToUpperInvariant()));

				if(CatalogSKUIBlockId != 0 && CatalogSKUIBlockId != IBlockId)
				{
					foreach (BXCustomField cf in CatalogSKUItemCustomFields)
						fieldValues.Add(
							new BXParamValue(cf.TextEncoder.Decode(cf.EditFormLabel), 
							CatalogSKUIBlockId.ToString() + "-" + cf.Name.ToUpper()));
				}
                ParamsDefinition["ShowCatalogItemProperties"].Values = fieldValues;
            }

            #region ComparisonSelectedFields
            List<BXParamValue> cmpFields = new List<BXParamValue>();
            cmpFields.Add(new BXParamValue(GetMessageRaw("Field.ElementID"), "ID"));
            cmpFields.Add(new BXParamValue(GetMessageRaw("Field.PreviewText"), "PreviewText"));
            cmpFields.Add(new BXParamValue(GetMessageRaw("Field.PreviewImage"), "PreviewImage"));
            cmpFields.Add(new BXParamValue(GetMessageRaw("Field.DetailText"), "DetailText"));
            cmpFields.Add(new BXParamValue(GetMessageRaw("Field.DetailImage"), "DetailImage"));
            cmpFields.Add(new BXParamValue(GetMessageRaw("Field.ActiveFromDate"), "ActiveFromDate"));
            cmpFields.Add(new BXParamValue(GetMessageRaw("Field.ActiveToDate"), "ActiveToDate"));
            if(customFields != null)
                for (int i = 0; i < customFields.Count; i++)
                    cmpFields.Add(new BXParamValue(customFields[i].EditFormLabel, "-" + customFields[i].Name.ToUpperInvariant()));

			if(CatalogSKUIBlockId != 0 && CatalogSKUIBlockId != IBlockId)
				foreach (BXCustomField cf in CatalogSKUItemCustomFields)
					cmpFields.Add(
						new BXParamValue(cf.TextEncoder.Decode(cf.EditFormLabel), 
						"-" +  CatalogSKUIBlockId.ToString() + "-" + cf.Name.ToUpper()));

            ParamsDefinition["ComparisonSelectedFields"].Values = cmpFields;
            #endregion

            List<BXParamValue> forumValues = new List<BXParamValue>();
			forumValues.Add(new BXParamValue(GetMessageRaw("NotSelectedMasculine"), "0"));
			if(IsForumModuleInstalled && Forum != null)
				foreach(ForumInfo forum in Forum.GetForums())
					forumValues.Add(new BXParamValue(forum.Name, forum.Id.ToString()));		
			ParamsDefinition["CommentsForumId"].Values = forumValues;
		}

        private BXCustomField[] catalogItemCustomFields = null;
        protected BXCustomField[] CatalogItemCustomFields
        {
            get
            {
                if (this.catalogItemCustomFields != null)
                    return this.catalogItemCustomFields;

                if (IBlockId <= 0)
                    return this.catalogItemCustomFields = new BXCustomField[0];

                return this.catalogItemCustomFields = BXCustomEntityManager.GetFields(
                    BXIBlockElement.GetCustomFieldsKey(IBlockId),
                    new BXFilter(new BXFilterOr(
                        new BXFilterItem(BXCustomField.Fields.Multiple, BXSqlFilterOperators.Equal, true),
						new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.System.Text"),
                        new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.System.Enumeration"),
                        new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.System.Boolean"),
                        new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.System.List"),
                        new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.IBlock.ElementId")
                        )),
                    new BXOrderBy(new BXOrderByPair(BXCustomField.Fields.Name, BXOrderByDirection.Asc))
                    ).ToArray();
            }
        }

		protected int CatalogSKUIBlockId
		{
			get { return IBlockId > 0 && Catalog != null ? Catalog.GetSkuIBlockId(IBlockId) : 0; }
		}


		private BXCustomField[] catalogSKUItemCustomFields = null;
        protected BXCustomField[] CatalogSKUItemCustomFields
        {
            get
            {
				if(CatalogSKUIBlockId <= 0)
                    return new BXCustomField[0];

				if(catalogSKUItemCustomFields != null)
					return catalogSKUItemCustomFields;

                return (catalogSKUItemCustomFields = BXCustomEntityManager.GetFields(
                    BXIBlockElement.GetCustomFieldsKey(CatalogSKUIBlockId),
                    new BXFilter(new BXFilterOr(
                        new BXFilterItem(BXCustomField.Fields.Multiple, BXSqlFilterOperators.Equal, true),
						new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.System.Text"),
                        new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.System.Enumeration"),
                        new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.System.Boolean"),
                        new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.System.List"),
                        new BXFilterItem(BXCustomField.Fields.CustomTypeId, BXSqlFilterOperators.Equal, "Bitrix.IBlock.ElementId")
                        )),
                    new BXOrderBy(new BXOrderByPair(BXCustomField.Fields.Name, BXOrderByDirection.Asc))).ToArray());
            }
        }

        private static bool? isCatalogModuleInstalled = null;
        private static bool IsCatalogModuleInstalled
        {
            get { return (isCatalogModuleInstalled ?? (isCatalogModuleInstalled = Bitrix.Modules.BXModuleManager.IsModuleInstalled("catalog"))).Value; }
        }

        /// <summary>
        /// Типы цен
        /// </summary>
        public class CatalogPriceTypeInfo
        {
            public CatalogPriceTypeInfo(int id, string name)
            {
                this.id = id;
                this.name = HttpUtility.HtmlEncode(name);
            }

            private int id = 0;
            public int Id
            {
                get { return this.id; }
            }

            private string name = string.Empty;
            public string Name
            {
                get { return this.name; }
            }
        }

        /// <summary>
        /// Торговый каталог
        /// </summary>
        public interface ICatalog
        {
            CatalogPriceTypeInfo[] GetPriceTypes();
			int GetSkuIBlockId(int catalogId);
        }

        private ICatalog Catalog
        {
            get { return GetCatalog(this); }
        }

        private static object catalogSync = new object();
        private static bool isCatalogLoaded = false;
        private static volatile ICatalog catalog = null;
        private static ICatalog GetCatalog(TemplateControl caller)
        {
            if (isCatalogLoaded)
                return catalog;

            lock (catalogSync)
            {
                if (isCatalogLoaded)
                    return catalog;

                if (IsCatalogModuleInstalled)
                    catalog = caller.LoadControl("catalog.ascx") as ICatalog;

                isCatalogLoaded = true;
                return catalog;
            }
        }

        private static bool? isForumModuleInstalled = null;
        private static bool IsForumModuleInstalled
        {
            get { return (isForumModuleInstalled ?? (isForumModuleInstalled = Bitrix.Modules.BXModuleManager.IsModuleInstalled("forum"))).Value; }
        }

        /// <summary>
        /// Описание Форума
        /// </summary>
        public class ForumInfo
        {
            public ForumInfo(int id, string name)
            {
                this.id = id;
                this.name = name;
            }

            private int id = 0;
            public int Id
            {
                get { return this.id; }
            }

            private string name = string.Empty;
            public string Name
            {
                get { return this.name; }
            }
        }

        /// <summary>
        /// Форум
        /// </summary>
        public interface IForum
        {
            ForumInfo[] GetForums();
        }

        private IForum Forum
        {
            get { return GetForum(this); }
        }

        private static object forumSync = new object();
        private static bool isForumLoaded = false;
        private static volatile IForum forum = null;
        private static IForum GetForum(TemplateControl caller)
        {
            if (isForumLoaded)
                return forum;

            lock (forumSync)
            {
                if (isForumLoaded)
                    return forum;

                if (IsForumModuleInstalled)
                    forum = caller.LoadControl("forum.ascx") as IForum;

                isForumLoaded = true;
                return forum;
            }
        }

        private static class VotingFacility
        {
            private static string totalValueCustomFieldName = "-_BX_RATING_VOTING_TOTAL_VALUE";
            private static string totalVotesCustomFieldName = "-_BX_RATING_VOTING_TOTAL_VOTES";

            //Добавляет пользовательские поля связанные с голосованием даже если их пока нет в контексте эл-тов данного инфоблока
            public static void PrepareSortingParamValues(List<BXParamValue> paramValues, CatalogueComponent parent)
            {
                if (paramValues.FindIndex(delegate(BXParamValue obj) { return string.Equals(obj.Value, totalValueCustomFieldName); }) < 0)
                    paramValues.Add(new BXParamValue(parent.GetMessageRaw("TotalValueCustomFieldEditFormLabel"), totalValueCustomFieldName));

                if (paramValues.FindIndex(delegate(BXParamValue obj) { return string.Equals(obj.Value, totalVotesCustomFieldName); }) < 0)
                    paramValues.Add(new BXParamValue(parent.GetMessageRaw("TotalVotesCustomFieldEditFormLabel"), totalVotesCustomFieldName));
            }
        }
	}
}
