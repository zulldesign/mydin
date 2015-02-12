using System;
using System.Collections.Generic;
using System.Web;
using Bitrix;

using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.DataTypes;
using Bitrix.Components.Editor;
using System.Text;
using Bitrix.Configuration;
using System.Web.UI;
using Bitrix.UI.Hermitage;

public partial class News : BXComponent
{
    //PROPERTIES
    public int IBlockId
    {
        get { return Parameters.Get<int>("IBlockId"); }
    }

    private string permCacheKey;
    public string PermissionCacheKey
    {
        get
        {
            if (permCacheKey != null)
                return permCacheKey;

            StringBuilder cacheKey = new StringBuilder();

            cacheKey.Append("news-");
            cacheKey.Append(IBlockId);
            cacheKey.Append("-");
            cacheKey.Append(Parameters.Get<bool>("UsePermissions", false));
            cacheKey.Append("-");
            cacheKey.Append(HttpContext.Current.Request.FilePath);
            cacheKey.Append("-");
            foreach (string permission in Parameters.GetList("GroupPermissions", new List<object>()))
            {
                cacheKey.Append(permission);
                cacheKey.Append("-");
            }
            permCacheKey = cacheKey.ToString().GetHashCode().ToString();
            return permCacheKey;
        }
    }

    public string SefNews
    {
        get { return Parameters.Get<string>("SEF_News"); }
    }

    public string SefSection
    {
        get { return Parameters.Get<string>("SEF_Section"); }
    }

    public string SefPage
    {
        get
        {
            return Parameters.Get<string>("SEF_Page");
        }
    }

    public string SefShowAll
    {
        get
        {
            return Parameters.Get<string>("SEF_ShowAll", String.Empty);
        }
    }

    public string SefSectionPage
    {
        get
        {
            return Parameters.Get<string>("Sef_Section_Page", String.Empty);
        }
    }

    public string SefSectionShowAll
    {
        get
        {
            return Parameters.Get<string>("Sef_Section_ShowAll", String.Empty);
        }
    }

    public string SefFolder
    {
        get { return Bitrix.IO.BXPath.TrimStart(Parameters.Get<string>("SefFolder", String.Empty)); }
        //get { return Parameters.Get<string>("SefFolder", String.Empty); }
    }

    private string sefDetailCommentRead = null;
    public string SefDetailCommentRead
    {
        get { return this.sefDetailCommentRead ?? (this.sefDetailCommentRead = Parameters.Get("Sef_Detail_Comment_Read", string.Empty)); }
    }

    private string sefDetailCommentOperation = null;
    public string SefDetailCommentOperation
    {
        get { return this.sefDetailCommentOperation ?? (this.sefDetailCommentOperation = Parameters.Get("Sef_Detail_Comment_Operation", string.Empty)); }
    }

    private string sefDetailCommentPage = null;
    public string SefDetailCommentPage
    {
        get { return this.sefDetailCommentPage ?? (this.sefDetailCommentPage = Parameters.Get("Sef_Detail_Comment_Page", string.Empty)); }
    }

    public string ParamNews
    {
        get { return Parameters.Get<string>("ParamNews", String.Empty); }
    }

    public string ParamSection
    {
        get { return Parameters.Get<string>("ParamSection", String.Empty); }
    }

    public string ParamPage
    {
        get
        {
            return Parameters.Get<string>("ParamPage", String.Empty);
        }
    }

    public string ParamShowAll
    {
        get
        {
            return Parameters.Get<string>("ParamShowAll", String.Empty);
        }
    }

    public string ParamComment
    {
        get { return Parameters.Get("ParamComment", String.Empty); }
    }

    public string ParamCommentOperation
    {
        get { return Parameters.Get("ParamCommentOperation", String.Empty); }
    }

    public string ParamCommentPage
    {
        get { return Parameters.Get("ParamCommentPage", String.Empty); }
    }

    
    public string menuItemPermisson
    {
        set { ComponentCache["menuItemPermisson"] = value; }
        get
        {
            if (!ComponentCache.ContainsKey("menuItemPermisson"))
                return String.Empty;
            return ComponentCache.Get<string>("menuItemPermisson");
        }
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

    private string CommentCustomPropertyName
    {
        get { return "_BXCommentForumTopicID"; }
    }

    //METHODS
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AllowComments)
            ComponentCache["_BXCommentCustomProperty"] = CommentCustomPropertyName;
       
        string page;
        BXParamsBag<string> urlToPage = new BXParamsBag<string>();

        if (EnableSef)
        {
            BXParamsBag<object> replaceItems = new BXParamsBag<object>();
            replaceItems["SectionId"] = String.Empty;

            urlToPage["detail"] = SefNews;
            urlToPage["detail.withoutsection"] = MakeLink(SefNews, replaceItems).Replace("//", "/");
            if (AllowComments)
            {
                urlToPage["detail.comment.read"] = SefDetailCommentRead;
                urlToPage["detail.sectionless.comment.read"] = MakeLink(SefDetailCommentRead, replaceItems).Replace("//", "/");
                urlToPage["detail.comment.operation"] = SefDetailCommentOperation;
                urlToPage["detail.sectionless.comment.operation"] = MakeLink(SefDetailCommentOperation, replaceItems).Replace("//", "/");
                urlToPage["detail.comment.page"] = SefDetailCommentPage;
                urlToPage["detail.sectionless.comment.page"] = MakeLink(SefDetailCommentPage, replaceItems).Replace("//", "/");
            }

            urlToPage["section"] = SefSection;
            urlToPage["section.page"] = SefSectionPage;
            urlToPage["section.showall"] = SefSectionShowAll;
            urlToPage["news.page"] = SefPage;
            urlToPage["news.showall"] = SefShowAll;
            urlToPage["rss"] = Parameters.Get("Sef_Rss", String.Empty);
            urlToPage["sectionrss"] = Parameters.Get("Sef_Rss_Section", String.Empty);

            string[] identityParts = MapVariable(SefFolder, urlToPage, Results, "news").Split(new char[] { '.' });
            page = identityParts[0];
            //string suffix = identityParts.Length > 1 ? identityParts[1] : string.Empty;

            if (page == "news.showall" || page == "section.showall")
                Results["PageShowAll"] = true;

            if (page.StartsWith("section"))
            {
                string sectionId = Results.Get<string>("SectionId", "");
                replaceItems["SectionId"] = sectionId;
                Results["UrlTemplatesNews"] = CombineLink(SefFolder, MakeLink(SefSection, replaceItems));
                Results["UrlTemplatesNewsPage"] = CombineLink(SefFolder, MakeLink(SefSectionPage, replaceItems));
                Results["UrlTemplatesNewsShowAll"] = CombineLink(SefFolder, MakeLink(SefSectionShowAll, replaceItems));
                Results["UrlTemplatesSectionRss"] = CombineLink(SefFolder, MakeLink(Parameters.Get("Sef_Rss_Section", String.Empty), replaceItems));
            }
            else
            {
                Results["UrlTemplatesNews"] = CombineLink(SefFolder, string.Empty);
                Results["UrlTemplatesNewsPage"] = CombineLink(SefFolder, SefPage);
                Results["UrlTemplatesNewsShowAll"] = CombineLink(SefFolder, SefShowAll);

                if (string.Equals(page, "detail", StringComparison.Ordinal) && AllowComments)
                {
                    string action = ComponentCache.Get("Operation", string.Empty);
                    if (!string.IsNullOrEmpty(action))
                        ComponentCache["CommentOperation"] = action;

                    string pageId = ComponentCache.Get("PageID", string.Empty);
                    if (!string.IsNullOrEmpty(pageId))
                        ComponentCache["CommentPage"] = pageId;

                    int elementId = ComponentCache.GetInt("ElementId", 0);
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

                    replaceItems["SectionId"] = ComponentCache.Get("SectionId", string.Empty);
                    replaceItems["ElementId"] = elementId.ToString();

                    ComponentCache["CommentReadUrlTemplate"] = CombineLink(SefFolder, MakeLink(SefDetailCommentRead, replaceItems)).Replace("//", "/");
                    ComponentCache["CommentOperationUrlTemplate"] = CombineLink(SefFolder, MakeLink(SefDetailCommentOperation, replaceItems)).Replace("//", "/");
                    ComponentCache["CommentReturnUrl"] = ComponentCache["CommentPageIndexTemplate"] = CombineLink(SefFolder, MakeLink(SefNews, replaceItems)).Replace("//", "/");
                    ComponentCache["CommentPageTemplate"] = CombineLink(SefFolder, MakeLink(SefDetailCommentPage, replaceItems)).Replace("//", "/");
                }
            }

            Results["UrlTemplatesDetail"] = CombineLink(SefFolder, SefNews);
            Results["UrlTemplatesSection"] = CombineLink(SefFolder, SefSection);
            Results["UrlTemplatesRss"] = CombineLink(SefFolder, Parameters.Get("Sef_Rss", String.Empty));
        }
        else
        {
            urlToPage["detail"] = ParamNews;
            urlToPage["section"] = ParamSection;
            urlToPage["news.page"] = ParamPage;
            urlToPage["news.showall"] = ParamShowAll;
            urlToPage["rss"] = "rss";
            urlToPage["sectionrss"] = "sectionrss";

            string[] identityParts = Map(urlToPage, "news").Split(new char[] { '.' });
            page = identityParts[0];
            //string suffix = identityParts.Length > 1 ? identityParts[1] : string.Empty;

            BXParamsBag<string> variableAlias = new BXParamsBag<string>();
            switch (page)
            {
                case "detail":
                    {
                        if (AllowComments)
                        {
                            string action = Request.QueryString[ParamCommentOperation];
                            if (!string.IsNullOrEmpty(action))
                                ComponentCache["CommentOperation"] = action;

                            string comment = Request.QueryString[ParamComment];
                            if (!string.IsNullOrEmpty(comment))
                                ComponentCache["CommentId"] = comment;

                            string commentPage = Request.QueryString[ParamCommentPage];
                            if (!string.IsNullOrEmpty(commentPage))
                                ComponentCache["CommentPage"] = commentPage;
                        }
                        variableAlias["ElementId"] = ParamNews;
                        variableAlias["SectionId"] = ParamSection;

                        break;
                    }
                case "section":
                    variableAlias["SectionId"] = ParamSection;
                    variableAlias["PageId"] = ParamPage;
                    variableAlias["PageShowAll"] = ParamShowAll;
                    break;
                case "rss":
                    break;
                case "sectionrss":
                    variableAlias["SectionId"] = "sectionrss";
                    break;
                default:
                    variableAlias["PageId"] = ParamPage;
                    variableAlias["PageShowAll"] = ParamShowAll;
                    break;
            }

            MapVariable(variableAlias, Results);
            string filePath = BXSefUrlManager.CurrentUrl.AbsolutePath;
            if (page == "section")
            {
                int sectionId = Results.Get<int>("SectionId", 0);
                Results["UrlTemplatesNews"] = String.Format("{0}?{1}={2}", filePath, ParamSection, sectionId);
                Results["UrlTemplatesNewsPage"] = String.Format("{0}?{1}={2}&{3}=#PageId#", filePath, ParamSection, sectionId, ParamPage);
                Results["UrlTemplatesNewsShowAll"] = String.Format("{0}?{1}={2}&{3}=", filePath, ParamSection, sectionId, ParamShowAll);
            }
            else
            {
                Results["UrlTemplatesNews"] = filePath;
                Results["UrlTemplatesNewsPage"] = String.Format("{0}?{1}=#PageId#", filePath, ParamPage);
                Results["UrlTemplatesNewsShowAll"] = String.Format("{0}?{1}=", filePath, ParamShowAll);

                if (string.Equals(page, "detail", StringComparison.Ordinal) && AllowComments)
                {
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

                    ComponentCache["CommentReadUrlTemplate"] = string.Format("{0}?{1}={2}&{3}={4}&{5}=#CommentId#", filePath, ParamSection, sectionId.ToString(), ParamNews, elementId.ToString(), ParamComment);
                    ComponentCache["CommentOperationUrlTemplate"] = string.Format("{0}?{1}={2}&{3}={4}&{5}=#CommentId#&{6}=#Operation#", filePath, ParamSection, sectionId.ToString(), ParamNews, elementId.ToString(), ParamComment, ParamCommentOperation);
                    ComponentCache["CommentReturnUrl"] = string.Format("{0}?{1}={2}&{3}={4}", filePath, ParamSection, sectionId.ToString(), ParamNews, elementId.ToString());
                    ComponentCache["CommentPageIndexTemplate"] = ComponentCache["CommentReturnUrl"];
                    ComponentCache["CommentPageTemplate"] = string.Format("{0}?{1}={2}&{3}={4}&{5}=#PageID#", filePath, ParamSection, sectionId.ToString(), ParamNews, elementId.ToString(), ParamCommentPage);
                }
            }

            Results["PageShowAll"] = Results.ContainsKey("PageShowAll");

            Results["UrlTemplatesSection"] = String.Format("{0}?{1}=#SectionId#", filePath, ParamSection);
            Results["UrlTemplatesDetail"] = String.Format("{0}?{1}=#SectionId#&{2}=#ElementId#", filePath, ParamSection, ParamNews);
            Results["UrlTemplatesRss"] = String.Format("{0}?{1}=Y", filePath, "rss");
            Results["UrlTemplatesSectionRss"] = String.Format("{0}?{1}=#SectionId#", filePath, "sectionrss");
        }

        IncludeComponentTemplate(page);
    }

    protected override void PreLoadComponentDefinition()
    {
        Title = GetMessage("Title");
        Description = GetMessage("Description");
        Icon = "images/news_all.gif";

        Group = new BXComponentGroup("news", GetMessage("Group"), 20, BXComponentGroup.Content);

        string clientSideActionGroupViewId = ClientID;

        //test
        //ParamsDefinition.Add(
        //    "TestParam",
        //    new BXParamSingleSelectionWithText(
        //        "Test param",
        //        String.Empty,
        //        BXCategory.Main, new BXParamValue[] { new BXParamValue("1", "1"), new BXParamValue("2", "2"), new BXParamValue("3", "3") }
        //    )
        //);

        //IBlockTypeId
        ParamsDefinition.Add(
            "IBlockTypeId",
            new BXParamSingleSelection(
                GetMessageRaw("InfoBlockType"),
                String.Empty,
                BXCategory.Main
            )
        );
        ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;

        BXCategory detailGroup = BXCategory.DetailSettings;
        BXCategory listGroup = BXCategory.ListSettings;

        listGroup.Sort = 210;
        detailGroup.Sort = 220;


        //IBlockId
        ParamsDefinition.Add(
            "IBlockId",
            new BXParamSingleSelection(
                GetMessageRaw("InfoBlockCode"),
                String.Empty,
                BXCategory.Main
            )
        );
        ParamsDefinition["IBlockId"].RefreshOnDirty = true;

        //NewsCount
        ParamsDefinition.Add(
            "PagingRecordsPerPage",
            new BXParamText(
                GetMessageRaw("NewsPerPage"),
                "20",
                BXCategory.Main
            )
        );


        //ParamsDefinition.Add(BXParametersDefinition.Sef);
        BXParamsBag<BXParam> sefParBag = BXParametersDefinition.Sef;
        //sefParBag["EnableSEF"].ClientSideAction = new ParamClientSideActionShowSefRelated(true);
        sefParBag["EnableSEF"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId, "EnableSEF", "Sef", "NonSef");
        //sefParBag["SEFFolder"].ClientSideAction = new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef);
        sefParBag["SEFFolder"].ClientSideAction = new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEFFolder", new string[] { "Sef" });
        ParamsDefinition.Add(sefParBag);

        //ParamSection
        ParamsDefinition.Add(
            "ParamSection",
            new BXParamText(
                GetMessageRaw("SectionID"),
                "section",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamSection", new string[] { "NonSef" }) 
            )
        );

        //ParamNews
        ParamsDefinition.Add(
            "ParamNews",
            new BXParamText(
                GetMessageRaw("NewsID"),
                "news",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamNews", new string[] { "NonSef" }) 
            )
        );

        //ParamPage
        ParamsDefinition.Add(
            "ParamPage",
            new BXParamText(
                GetMessageRaw("PageParamForList"),
                "page",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamPage", new string[] { "NonSef" }) 
            )
        );

        //ParamShowAll
        ParamsDefinition.Add(
            "ParamShowAll",
            new BXParamText(
                GetMessageRaw("ShowAllParam"),
                "showall",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamShowAll", new string[] { "NonSef" }) 
            )
        );

        ParamsDefinition.Add(
            "ParamComment",
            new BXParamText(
                GetMessageRaw("ParamComment"),
                "comment",
                BXCategory.Sef,
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamComment", new string[] { "NonSef" })
            )
        );

        ParamsDefinition.Add(
            "ParamCommentOperation",
            new BXParamText(
                GetMessageRaw("ParamCommentOperation"),
                "act",
                BXCategory.Sef,
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamCommentOperation", new string[] { "NonSef" })
            )
        );

        ParamsDefinition.Add(
            "ParamCommentPage",
            new BXParamText(
                GetMessageRaw("ParamCommentPage"),
                "page",
                BXCategory.Sef,
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamCommentPage", new string[] { "NonSef" })
            )
        );

        //Sef_News
        ParamsDefinition.Add(
            "Sef_News",
            new BXParamText(
                GetMessageRaw("PageOfDetailedView"),
                "/#SectionId#/news-#ElementId#/",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_News", new string[] { "Sef" }) 
            )
        );

        //Sef_RSS
        ParamsDefinition.Add(
            "Sef_Rss",
            new BXParamText(
                GetMessageRaw("RssPage"),
                "/rss/",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_Rss", new string[] { "Sef" }) 
            )
        );

        //Sef_Section
        ParamsDefinition.Add(
            "Sef_Section",
            new BXParamText(
                GetMessageRaw("SectionPage"),
                "/#SectionId#/",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_Section", new string[] { "Sef" }) 
            )
        );

        //Sef_RSS
        ParamsDefinition.Add(
            "Sef_Rss_Section",
            new BXParamText(
                GetMessageRaw("SectionRssPage"),
                "/rss/#SectionId#/",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_Rss_Section", new string[] { "Sef" }) 
            )
        );

        //Sef_Page
        ParamsDefinition.Add(
            "Sef_Page",
            new BXParamText(
                GetMessageRaw("OnePageInNewsList"),
                "/page-#pageId#",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_Page", new string[] { "Sef" }) 
            )
        );

        //Sef_ShowAll
        ParamsDefinition.Add(
            "Sef_ShowAll",
            new BXParamText(
                GetMessageRaw("AllPagesInNewsList"),
                "/all",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_ShowAll", new string[] { "Sef" }) 
            )
        );

        //Sef_Page
        ParamsDefinition.Add(
            "Sef_Section_Page",
            new BXParamText(
                GetMessageRaw("OnePageInSectionNewsList"),
                "/#SectionId#/page-#pageId#",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_Section_Page", new string[] { "Sef" }) 
            )
        );

        //Sef_ShowAll
        ParamsDefinition.Add(
            "Sef_Section_ShowAll",
            new BXParamText(
                GetMessageRaw("AllPagesInSectionNewsList"),
                "/#SectionId#/all",
                BXCategory.Sef,
                //new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_Section_ShowAll", new string[] { "Sef" }) 
            )
        );

        //Sef_Detail_Comment_Read
        ParamsDefinition.Add(
            "Sef_Detail_Comment_Read",
            new BXParamText(
                GetMessageRaw("SefDetailCommentRead"),
                "/#SectionId#/news-#ElementId#/comment-#CommentId#/",
                BXCategory.Sef,
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_Detail_Comment_Read", new string[] { "Sef" })
            )
        );

        //Sef_Detail_Comment_Operation
        ParamsDefinition.Add(
            "Sef_Detail_Comment_Operation",
            new BXParamText(
                GetMessageRaw("SefDetailCommentOperation"),
                "/#SectionId#/news-#ElementId#/comment-#CommentId#/act-#Operation#/",
                BXCategory.Sef,
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_Detail_Comment_Operation", new string[] { "Sef" })
            )
        );

        //Sef_Detail_Comment_Page
        ParamsDefinition.Add(
            "Sef_Detail_Comment_Page",
            new BXParamText(
                GetMessageRaw("SefDetailCommentPage"),
                "/#SectionId#/news-#ElementId#/page-#PageID#/",
                BXCategory.Sef,
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "Sef_Detail_Comment_Page", new string[] { "Sef" })
            )
        );

        ParamsDefinition.Add(BXParametersDefinition.Ajax);

        BXCategory rssCategory = new BXCategory(GetMessageRaw("RssSettings"), "RssSettings", 250);

        ParamsDefinition.Add(
            "UseRss",
            new BXParamYesNo(
                GetMessageRaw("AllowRss"),
                true,
                rssCategory
            )
        );

        ParamsDefinition.Add(
            "RssElementsCount",
            new BXParamText(
                GetMessageRaw("RssElementCount"),
                "20",
                rssCategory
            )
        );

        ParamsDefinition.Add(
            "RssDaysCount",
            new BXParamText(
                GetMessageRaw("RssDayCount"),
                "",
                rssCategory
            )
        );

        //SortBy1
        ParamsDefinition.Add(
            "SortBy1",
            new BXParamSingleSelection(
                GetMessageRaw("SortByFirst"),
                "ActiveFromDate",
                BXCategory.DataSource
            )
        );

        //SortBy2
        ParamsDefinition.Add(
            "SortBy2",
            new BXParamSingleSelection(
                GetMessageRaw("SortBySecond"),
                "ID",
                BXCategory.DataSource
            )
        );

        //SortOrder1
        ParamsDefinition.Add(
            "SortOrder1",
            new BXParamSort(
                GetMessageRaw("FirstSortOrder"),
                BXCategory.DataSource
            )
        );

        //SortOrder2
        ParamsDefinition.Add(
            "SortOrder2",
            new BXParamSort(
                GetMessageRaw("SecondSortOrder"),
                BXCategory.DataSource
            )
        );

        ParamsDefinition.Add(
            "ListShowPreviewText",
            new BXParamYesNo(
                GetMessageRaw("ShowPreviewText"),
                true,
                listGroup
            )
        );

        ParamsDefinition.Add(
            "ListShowDetailText",
            new BXParamYesNo(
                GetMessageRaw("ListShowDetailText"),
                false,
                listGroup
            )
        );


        ParamsDefinition.Add(
            "ListShowPreviewPicture",
            new BXParamYesNo(
                GetMessageRaw("ShowPreviewPicture"),
                true,
                listGroup
            )
        );
        //PreviewTruncateLen
        ParamsDefinition.Add(
            "PreviewTruncateLen",
            new BXParamText(
                GetMessageRaw("MaxAnnouncementOutputLength"),
                "0",
                listGroup
            )
        );

        ParamsDefinition.Add(
            "ListShowTitle",
            new BXParamYesNo(
                GetMessageRaw("ShowTitle"),
                true,
                listGroup
            )
        );

        //SetTitle
        ParamsDefinition.Add(
            "ListSetTitle",
            new BXParamYesNo(
                GetMessageRaw("SetPageTitle"),
                false,
                listGroup
            )
        );

        ParamsDefinition.Add(
            "ListShowDate",
            new BXParamYesNo(
                GetMessageRaw("ShowDate"),
                true,
                listGroup
            )
        );

        //ActiveDateFormat
        ParamsDefinition.Add(
            "ActiveDateFormat",
            new BXParamSingleSelection(
                GetMessageRaw("DateDisplayFormat"),
                "dd.MM.yyyy",
                listGroup
            )
        );

        //HideLinkWhenNoDetail
        ParamsDefinition.Add(
            "HideLinkWhenNoDetail",
            new BXParamYesNo(
                GetMessageRaw("HideLinkIfDetailedDescriptionIsNotExist"),
                false,
                listGroup
            )
        );

        //PropertyCode
        ParamsDefinition.Add(
            "PropertyCode",
            new BXParamMultiSelection(
                GetMessageRaw("Properties"),
                String.Empty,
                listGroup
            )
        );

        ParamsDefinition.Add(
            "DetailShowTitle",
            new BXParamYesNo(
                GetMessageRaw("ShowTitle"),
                true,
                detailGroup
            )
        );

        ParamsDefinition.Add(
            "DetailSetTitle",
            new BXParamYesNo(
                GetMessageRaw("SetPageTitle"),
                false,
                detailGroup
            )
        );

        //ShowDetailPicture
        ParamsDefinition.Add(
            "DetailShowDetailPicture",
            new BXParamYesNo(
                GetMessageRaw("ShowDetailPicture"),
                true,
                detailGroup
            )
        );

        ParamsDefinition.Add(
            "DetailShowPreviewPicture",
            new BXParamYesNo(
                GetMessageRaw("ShowPreviewPicture"),
                false,
                detailGroup
            )
        );

        ParamsDefinition.Add(
            "DetailShowPreviewText",
            new BXParamYesNo(
                GetMessageRaw("ShowPreviewText"),
                false,
                detailGroup
            )
        );

        ParamsDefinition.Add(
            "DetailShowDate",
            new BXParamYesNo(
                GetMessageRaw("ShowDate"),
                true,
                detailGroup
            )
        );

        //DetailActiveDateFormat
        ParamsDefinition.Add(
            "DetailActiveDateFormat",
            new BXParamSingleSelection(
                GetMessageRaw("DateDisplayFormat"),
                "dd.MM.yyyy",
                detailGroup
            )
        );

        ParamsDefinition.Add(
            "IBlockUrlTitle",
            new BXParamText(
                GetMessageRaw("LinkNameToElementsList"),
                GetMessageRaw("BackToNewsList"),
                detailGroup
            )
        );

        //DetailPropertyCode
        ParamsDefinition.Add(
            "DetailPropertyCode",
            new BXParamMultiSelection(
                GetMessageRaw("PropertiesForDetailedView"),
                String.Empty,
                detailGroup
            )
        );

        //DisplayPanel
        ParamsDefinition.Add(
            "DisplayPanel",
            new BXParamYesNo(
                GetMessageRaw("AddButtonsForThisComponentToAdminPanel"),
                false,
                BXCategory.AdditionalSettings
            )
        );

        ParamsDefinition.Add(
            "ShowActiveElements",
            new BXParamYesNo(
                GetMessageRaw("ShowActiveElements"),
                true,
                BXCategory.AdditionalSettings
            )
        );

        ParamsDefinition.Add(
            "IncludeSubsections",
            new BXParamYesNo(
                GetMessageRaw("IncludeSubsections"),
                true,
                BXCategory.AdditionalSettings
                )
        );

        //UsePermissions
        ParamsDefinition.Add(
            "UsePermissions",
            new BXParamYesNo(
                GetMessageRaw("UsePermissions"),
                false,
                BXCategory.AdditionalSettings
            )
        );

        //GroupPermissions
        ParamsDefinition.Add(
            "GroupPermissions",
            new BXParamMultiSelection(
                GetMessageRaw("GroupPermissions"),
                "1",
                BXCategory.AdditionalSettings
            )
        );

        BXCategory commentCategory = new BXCategory(GetMessageRaw("CommentSettings"), "CommentSettings", 260);
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
            new BXParamText(
                GetMessageRaw("CommentsForumId"),
                "0",
                commentCategory,
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

        if (!string.IsNullOrEmpty(DesignerSite))
            ParamsDefinition.Add(BXParametersDefinition.Menu(DesignerSite));

        ParamsDefinition.Add(BXParametersDefinition.Search);

        ParamsDefinition.Add(BXParametersDefinition.Cache);

        BXParamsBag<BXParam> paging = BXParametersDefinition.Paging;
        paging.Remove("PagingRecordsPerPage");
        ParamsDefinition.Add(paging);
    }

    protected override void LoadComponentDefinition()
    {
        //IBlockTypeId
        List<BXParamValue> types = new List<BXParamValue>();
        types.Add(new BXParamValue("-", ""));
        BXIBlockTypeCollection typeCollection = BXIBlockType.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Sort, BXOrderByDirection.Asc)));
        foreach (BXIBlockType t in typeCollection)
            types.Add(new BXParamValue(t.Translations[BXLoc.CurrentLocale].Name, t.Id.ToString()));

        ParamsDefinition["IBlockTypeId"].Values = types;


        BXFilter iblockFilter = new BXFilter();
        if (Parameters.ContainsKey("IBlockTypeId"))
        {
            int typeId;
            int.TryParse(Parameters["IBlockTypeId"], out typeId);
            if (typeId > 0)
                iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, typeId));
        }
        if (!string.IsNullOrEmpty(DesignerSite))
            iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

        //IBlockId
        List<BXParamValue> iblocks = new List<BXParamValue>();
        BXIBlockCollection iblockCollection = BXIBlock.GetList(iblockFilter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Sort, BXOrderByDirection.Asc)));
        foreach (BXIBlock b in iblockCollection)
            iblocks.Add(new BXParamValue(b.Name, b.Id.ToString()));

        ParamsDefinition["IBlockId"].Values = iblocks;


        //SortBy1 & SortBy2
        List<BXParamValue> sortFields = new List<BXParamValue>();
        sortFields.Add(new BXParamValue("ID", "ID"));
        sortFields.Add(new BXParamValue(GetMessageRaw("SortByName"), "Name"));
        sortFields.Add(new BXParamValue(GetMessageRaw("ActiveFromDate"), "ActiveFromDate"));
        sortFields.Add(new BXParamValue(GetMessageRaw("Sort"), "Sort"));
        sortFields.Add(new BXParamValue(GetMessageRaw("DateOfLastModification"), "UpdateDate"));

        ParamsDefinition["SortBy1"].Values = sortFields;
        ParamsDefinition["SortBy2"].Values = sortFields;

        //ActiveDateFormat & DetailActiveDateFormat
        List<BXParamValue> dateFormats = new List<BXParamValue>();
        DateTime now = DateTime.Now;
        dateFormats.Add(new BXParamValue(now.ToString("dd-MM-yyyy"), "dd-MM-yyyy"));
        dateFormats.Add(new BXParamValue(now.ToString("MM-dd-yyyy"), "MM-dd-yyyy"));
        dateFormats.Add(new BXParamValue(now.ToString("yyyy-MM-dd"), "yyyy-MM-dd"));
        dateFormats.Add(new BXParamValue(now.ToString("dd.MM.yyyy"), "dd.MM.yyyy"));
        dateFormats.Add(new BXParamValue(now.ToString("MM.dd.yyyy"), "MM.dd.yyyy"));
        dateFormats.Add(new BXParamValue(now.ToString("dd/MM/yyyy"), "dd/MM/yyyy"));
        dateFormats.Add(new BXParamValue(now.ToString("dd.MM.yyyy HH:mm"), "dd.MM.yyyy HH:mm"));
        dateFormats.Add(new BXParamValue(now.ToString("D"), "D"));
        dateFormats.Add(new BXParamValue(now.ToString("f"), "f"));

        ParamsDefinition["ActiveDateFormat"].Values = dateFormats;
        ParamsDefinition["DetailActiveDateFormat"].Values = dateFormats;

        //PropertyCode & DetailPropertyCode
        List<BXParamValue> properties = new List<BXParamValue>();
        if (Parameters.ContainsKey("IBlockId"))
        {
            int iblockId;
            int.TryParse(Parameters["IBlockId"], out iblockId);
            if (iblockId > 0)
            {
                BXCustomFieldCollection cfCollection = BXCustomEntityManager.GetFields(String.Format("IBlock_{0}", iblockId));
                foreach (BXCustomField f in cfCollection)
                    properties.Add(new BXParamValue(f.EditFormLabel, f.Name));
            }
        }

        ParamsDefinition["PropertyCode"].Values = properties;
        ParamsDefinition["DetailPropertyCode"].Values = properties;


        //GroupPermissions
        List<BXParamValue> rolesList = new List<BXParamValue>();
        BXRoleCollection roles = BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc"));
        foreach (BXRole role in roles)
            rolesList.Add(new BXParamValue(role.Title, role.RoleName));

        ParamsDefinition["GroupPermissions"].Values = rolesList;

        foreach (string parDefKey in ParamsDefinition.Keys)
            ParamsDefinition[parDefKey].AdjustPresentation(this);
    }

    public override void ProcessMessage(BXCommand cmd, bool executionContext, string executionVirtualPath)
    {
        #region Search
        if (cmd.Action == "Bitrix.Search.ProvideUrl")
        {
            string moduleId = cmd.Parameters.Get("moduleId", string.Empty);
            if (string.Equals(moduleId, "forum", StringComparison.OrdinalIgnoreCase) 
                && AllowComments 
                && BXModuleManager.IsModuleInstalled("Forum"))
            {
                //Поиск по комментариям к новостям
                int blockId = IBlockId;
                int forumId = cmd.Parameters.GetInt("itemGroup", 0);
                int postId = cmd.Parameters.GetInt("itemId", 0);
                int topicId = cmd.Parameters.GetInt("param1");


                if (blockId == 0 || !string.Equals(moduleId, "forum", StringComparison.OrdinalIgnoreCase) || forumId <= 0 || postId <= 0)
                    return;

                BXFilter f = new BXFilter(
                        new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, blockId),
                        new BXFilterItem(BXIBlockElement.Fields.CustomFields[blockId].GetFieldByKey(CommentCustomPropertyName), BXSqlFilterOperators.Equal, topicId)
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
                    commentUrl = string.Format("{0}?{1}={2}&{3}={4}#comment{4}", BXSite.GetUrlForPath(executionVirtualPath, null, cmd.SiteId), ParamNews, el.Id.ToString(), ParamComment, postId.ToString());
                else
                {
                    BXParamsBag<object> commentSefParams = new BXParamsBag<object>();
                    commentSefParams["SectionId"] = string.Empty;
                    commentSefParams["ElementId"] = el.Id.ToString();
                    commentSefParams["CommentId"] = postId.ToString();
                    string t = SefDetailCommentRead;
                    if (t.IndexOf("##") < 0)
                        t = string.Concat(t, "##comment#CommentId#");
                    commentUrl = BXSefUrlUtility.MakeLink(CombineLink(SefFolder, t), commentSefParams).Replace("//", "/");
                }
                cmd.AddCommandResult(string.Concat("bitrix:forum@", executionVirtualPath), new BXCommandResult(BXCommandResultType.Ok, commentUrl));                    
                return;
            }

            if (!string.Equals(moduleId, "iblock", StringComparison.OrdinalIgnoreCase))
                return;

            if (IBlockId < 0 || cmd.Parameters.Get<int>("itemGroup") != IBlockId)
                return;

            int elementId = cmd.Parameters.Get("itemId", -1);
            if (elementId < 0)
                return;

            BXIBlockElement elm = BXIBlockElement.GetById(elementId);
            if (elm == null) return;
            BXParamsBag<object> p = new BXParamsBag<object>();
            p.Add("ElementId", elementId);
            p.Add("SectionId", elm.Sections.Count > 0 ? elm.Sections[0].SectionId.ToString() : String.Empty);
            string url;
            if (EnableSef)
            {
                url = MakeLink(SefFolder, SefNews, p);
                if (elm.Sections.Count == 0)
                    url = url.Replace("//", "/");
            }
            else
                url = MakeLink(string.Format("{0}?{1}=#ElementId#", BXSite.GetUrlForPath(executionVirtualPath, null, cmd.SiteId), ParamNews), p);
            cmd.AddCommandResult("bitrix:news@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, url));
        }
        #endregion

        if (cmd.Action == "Bitrix.Main.GeneratePublicMenu")
        {
            //Совпадает ли тип меню в параметрах компонента с типом, который запрашивает система.
            if (!Parameters.Get("GenerateMenuType", "left").Equals(cmd.Parameters.Get<string>("menuType"), StringComparison.InvariantCultureIgnoreCase))
                return;

            //Генерируем меню только для тех адресов, которые выводит сам компонент.
            if (!Parameters.Get<bool>("EnableSEF") && !BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath).Equals(cmd.Parameters.Get<string>("uriPath"), StringComparison.InvariantCultureIgnoreCase))
                return;
            else if (Parameters.Get<bool>("EnableSEF") && !cmd.Parameters.Get<string>("uriPath").StartsWith(Bitrix.IO.BXPath.ToVirtualRelativePath(SefFolder.TrimStart('\\', '/')) + "/", StringComparison.InvariantCultureIgnoreCase))
                return;

            if (IBlockId < 0)
                return;

            BXParamsBag<object> request = new BXParamsBag<object>();
            BXParamsBag<string> urlToPage = new BXParamsBag<string>();

            if (EnableSef)
            {
                urlToPage["detail"] = SefNews;
                urlToPage["section"] = SefSection;
                urlToPage["news.page"] = SefPage;
                urlToPage["news.showall"] = SefShowAll;
                urlToPage["rss"] = Parameters.Get("Sef_Rss", String.Empty);
                urlToPage["sectionrss"] = Parameters.Get("Sef_Rss_Section", String.Empty);

                MapVariable(SefFolder, urlToPage, request, String.Empty, BXUri.ToRelativeUri(cmd.Parameters.Get<string>("uri")));
            }
            else
            {
                BXParamsBag<string> variableAlias = new BXParamsBag<string>();
                variableAlias["ElementId"] = ParamNews;
                variableAlias["SectionId"] = ParamSection;

                MapVariable(variableAlias, request, BXUri.ToRelativeUri(cmd.Parameters.Get<string>("uri")));
            }

            //Определим уровень доступа пунктов
            if ((menuItemPermisson = (string)BXCacheManager.MemoryCache.Get(PermissionCacheKey)) == null)
            {
                menuItemPermisson = String.Empty;
                StringBuilder menuItemRoles = new StringBuilder();
                if (Parameters.Get<bool>("UsePermissions"))
                {
                    foreach (string permission in Parameters.GetList("GroupPermissions", new List<object>()))
                    {
                        menuItemRoles.Append(permission);
                        menuItemRoles.Append(";");
                    }

                    if (menuItemRoles.Length > 0)
                        menuItemRoles.Append("Admin");
                }
                else
                {
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
                }

                if (menuItemRoles != null && menuItemRoles.Length > 0)
                    menuItemPermisson = menuItemRoles.ToString();

                BXCacheManager.MemoryCache.Insert(PermissionCacheKey, menuItemPermisson);
            }

            int elementId = 0, sectionId = 0;
            elementId = request.Get<int>("ElementId", elementId);
            sectionId = request.Get<int>("SectionId", sectionId);

            string parentLevelUri = null;
            List<BXPublicMenuItem> menuList = null;

            if (elementId > 0 && sectionId > 0)
            {
                parentLevelUri = MakeMenuUri(executionVirtualPath, sectionId);
            }
            //Если указан только элемент
            else if (elementId > 0)
            {
                BXFilter elementFilter = new BXFilter(
                    new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId),
                    new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId)
                );

                if (Parameters.Get<bool>("ShowActiveElements", true))
                {
                    elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"));
                    elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, "Y"));
                }

                BXIBlockElementCollection element = BXIBlockElement.GetList(elementFilter, null);
                if (element != null && element.Count > 0)
                {
                    BXIBlockElement.BXInfoBlockElementSectionCollection sections = element[0].Sections;
                    if (sections != null && sections.Count > 0)
                    {
                        sectionId = sections[0].SectionId;
                        parentLevelUri = MakeMenuUri(executionVirtualPath, sectionId); //Меню строится для раздела, к которому привязан элемент
                    }
                }
            }

            //Если указан раздел выводим его дочерние разделы
            if (parentLevelUri == null && sectionId > 0)
            {
                BXIBlockSectionCollection sectionList = BXIBlockSection.GetList(
                    new BXFilter(
                        new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
                        new BXFilterItem(BXIBlockSection.Fields.SectionId, BXSqlFilterOperators.Equal, sectionId)
                    ),
                    new BXOrderBy(
                        new BXOrderByPair(BXIBlockSection.Fields.Sort, BXOrderByDirection.Asc),
                        new BXOrderByPair(BXIBlockSection.Fields.Name, BXOrderByDirection.Asc)
                    )
                );

                menuList = ExtractMenuItemsFromCollection(sectionList, executionVirtualPath);

                //Если нет дочерних разделов, то меню строится для родителя указанного раздела 
                if (menuList == null)
                {
                    sectionList = BXIBlockSection.GetList(
                        new BXFilter(
                            new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                            new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
                            new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
                            new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.Equal, sectionId)
                        ),
                        new BXOrderBy(
                            new BXOrderByPair(BXIBlockSection.Fields.Sort, BXOrderByDirection.Asc),
                            new BXOrderByPair(BXIBlockSection.Fields.Name, BXOrderByDirection.Asc)
                        )
                    );

                    if (sectionList != null && sectionList.Count > 0 && sectionList[0].SectionId > 0)
                    {
                        parentLevelUri = MakeMenuUri(executionVirtualPath, sectionList[0].SectionId);
                    }
                    else
                    {
                        //Если такой раздел не существует или это раздел корневой (нет родителя)
                        if (Parameters.Get<bool>("EnableSEF"))
                            parentLevelUri = SefFolder;
                        else
                            parentLevelUri = BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath);
                    }
                }
            }

            //Если ничего не указано выводим корневые разделы
            Dictionary<string, BXPublicMenu.BXLoadMenuCommandParameter> menuTree = new Dictionary<string, BXPublicMenu.BXLoadMenuCommandParameter>();
            if (parentLevelUri == null && menuList == null)
            {

                /*BXIBlockSectionCollection sectionList = BXIBlockSection.GetList(
                    new BXFilter(
                        new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
                        new BXFilterItem(BXIBlockSection.Fields.SectionId, BXSqlFilterOperators.Equal, null)
                    ),

                    new BXOrderBy(
                        new BXOrderByPair(BXIBlockSection.Fields.Sort, BXOrderByDirection.Asc),
                        new BXOrderByPair(BXIBlockSection.Fields.Name, BXOrderByDirection.Asc)
                    )
                );

                            menuList = ExtractMenuItemsFromCollection(sectionList, executionVirtualPath);
            */
                BXIBlockSectionCollection sectionsList = BXIBlockSection.GetList(
                    new BXFilter(
                        new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId)
                    ),
                    new BXOrderBy(
                        new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc),
                        new BXOrderByPair(BXIBlockSection.Fields.Sort, BXOrderByDirection.Asc),
                        new BXOrderByPair(BXIBlockSection.Fields.Name, BXOrderByDirection.Asc)
                    )
                );

                Dictionary<int, List<BXPublicMenuItem>> sectionTree = new Dictionary<int, List<BXPublicMenuItem>>();
                foreach (BXIBlockSection section in sectionsList)
                {
                    if (!sectionTree.ContainsKey(section.SectionId))
                        sectionTree.Add(section.SectionId, new List<BXPublicMenuItem>());

                    BXPublicMenuItem menuItem = new BXPublicMenuItem();
                    menuItem.Title = section.Name;
                    menuItem.Links.Add(MakeMenuUri(executionVirtualPath, section.Id));
                    menuItem.Sort = section.Sort;
                    if (!String.IsNullOrEmpty(menuItemPermisson))
                    {
                        menuItem.ConditionType = ConditionType.Group;
                        menuItem.Condition = menuItemPermisson;
                    }
                    sectionTree[section.SectionId].Add(menuItem);
                }

                foreach (KeyValuePair<int, List<BXPublicMenuItem>> submenuList in sectionTree)
                {
                    string url = null;
                    if (submenuList.Key > 0)
                    {
                        url = MakeMenuUri(executionVirtualPath, submenuList.Key);
                    }
                    else
                    {
                        if (Parameters.Get<bool>("EnableSEF"))
                            url = SefFolder;
                        else
                            url = BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath);
                    }

                    menuTree.Add(url, new BXPublicMenu.BXLoadMenuCommandParameter(submenuList.Value, true, null));
                }
            }

            if (menuTree.Count > 0)
                cmd.AddCommandResult("bitrix:news@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, menuTree));
            else
            {
                BXPublicMenu.BXLoadMenuCommandParameter menuResult = new BXPublicMenu.BXLoadMenuCommandParameter(menuList, true, parentLevelUri);
                cmd.AddCommandResult("bitrix:news@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, menuResult));
            }


        }
    }

    public BXPopupMenuItem[] GetMenuItems()
    {
        BXPopupMenuItem[] menuItems = new BXPopupMenuItem[1];
        menuItems[0] = new BXPopupMenuItem();
        menuItems[0].Text = GetMessage("UpdateCachedMenuData");
        menuItems[0].ClientClickEventHandler = delegate(object sender, EventArgs e)
        {
            string url = null;
            if (Parameters.Get<bool>("EnableSEF"))
                url = Bitrix.IO.BXPath.ToVirtualRelativePath(SefFolder.TrimStart('\\', '/')) + "/";
            else
                url = VirtualPathUtility.ToAppRelative(HttpUtility.UrlDecode(BXSefUrlManager.CurrentUrl.AbsolutePath));

            BXPublicMenu.ClearGlobalMenu(BXSite.Current.Id, Parameters.Get("GenerateMenuType", "left"), url, true);

            if (IBlockId > 0)
                BXCacheManager.MemoryCache.Remove(PermissionCacheKey);
        };

        return menuItems;
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
            menuItem.Links.Add(MakeMenuUri(executionVirtualPath, section.Id));
            menuItem.Sort = section.Sort;
            if (!String.IsNullOrEmpty(menuItemPermisson))
            {
                menuItem.ConditionType = ConditionType.Group;
                menuItem.Condition = menuItemPermisson;
            }
            menuList.Add(menuItem);
        }

        return menuList;
    }

    private string MakeMenuUri(string executionVirtualPath, int sectionId)
    {
        string url;
        BXParamsBag<object> replace = new BXParamsBag<object>();
        replace["SectionId"] = sectionId;

        if (Parameters.Get<bool>("EnableSEF"))
            url = MakeLink(SefFolder, SefSection, replace);
        else
            url = MakeLink(String.Format("{0}?{1}=#SectionId#", BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath), ParamSection), replace);

        return url;
    }
}
