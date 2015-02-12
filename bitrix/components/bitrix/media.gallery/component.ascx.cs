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
using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.DataTypes;
using System.Collections.Generic;
using Bitrix.IBlock;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.Modules;
using System.Text;
using Bitrix.Security;

namespace Bitrix.IBlock.Components
{
    public partial class MediaGalleryComponent : Bitrix.UI.BXComponent
    {
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
            get { return Parameters.Get<string>("CommonListPageTemplate", "/page-#PageId#"); }
        }
        public string CommonListShowAllTemplate
        {
            get { return Parameters.Get<string>("CommonListShowAllTemplate", "/all"); }
        } 

        public string SectionListPageTemplate
        {
            get { return Parameters.Get<string>("SectionListPageTemplate", "/#SectionId#/page-#PageId#"); }
        }

        public string SectionListShowAllTemplate
        {
            get { return Parameters.Get<string>("SectionListShowAllTemplate", "/#SectionId#/all"); }
        }

        private string menuItemPermisson = String.Empty;
        public string MenuItemPermisson
        {
            set { menuItemPermisson = value; }
            get { return menuItemPermisson; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

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

                string code = MapVariable(SefFolder, urlToPage, ComponentCache, "index");

                int position = code.IndexOf('.');
                if (position > 0)
                    page = code.Substring(0, position);
                else
                    page = code;

                if (page == "list")
                {
                    BXParamsBag<object> replaceItems = ComponentCache;
                    replaceItems["IblockId"] = replaceItems["IBLOCK_ID"] = IBlockId.ToString();

                    ComponentCache["PagingIndexTemplate"] = CombineLink(SefFolder, MakeLink(ElementListTemplate, replaceItems));
                    ComponentCache["PagingPageTemplate"] = CombineLink(SefFolder, MakeLink(SectionListPageTemplate, replaceItems));
                    ComponentCache["PagingShowAllTemplate"] = CombineLink(SefFolder, MakeLink(SectionListShowAllTemplate, replaceItems));
                }
                else
                {
                    ComponentCache["PagingIndexTemplate"] = CombineLink(SefFolder, String.Empty);
                    ComponentCache["PagingPageTemplate"] = CombineLink(SefFolder, CommonListPageTemplate);
                    ComponentCache["PagingShowAllTemplate"] = CombineLink(SefFolder, CommonListShowAllTemplate);
                }

                ComponentCache["ShowAll"] = (code == "index.all" || code == "list.all" ? "y" : "n");
                ComponentCache["SectionElementListUrl"] = CombineLink(SefFolder, ElementListTemplate);
                ComponentCache["ElementDetailUrl"] = CombineLink(SefFolder, ElementDetailTemplate);
            }
            else
            {
                int sectionId = 0, elementId = 0;
                if (Request[ElementIdVariable] != null && int.TryParse(Request[ElementIdVariable], out elementId) && elementId > 0)
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
                }

                ComponentCache["SectionElementListUrl"] = String.Format("{0}?{1}=#SectionId#", filePath, SectionIdVariable);
                ComponentCache["ElementDetailUrl"] = String.Format("{0}?{1}=#SectionId#&{2}=#ElementId#", filePath, SectionIdVariable, ElementIdVariable);
            }

            if (page == "index" && Parameters.Get<bool>("ShowTopElements", false))
                page = "index_top";

            IncludeComponentTemplate(page);
        
        }

        #region Component
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Group = new BXComponentGroup("media", GetMessage("Group"), 100, BXComponentGroup.Content);
            Icon = "images/mediagallery.gif";

            BXCategory mainCategory = BXCategory.Main;
            BXCategory dataSourceCategory = BXCategory.DataSource;

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
                "IBlockElementPropertyForFilePath",
                new BXParamSingleSelection(
                    GetMessageRaw("IBlockElementPropertyForFilePath"),
                    "",
                    dataSourceCategory
                )
            );

            ParamsDefinition.Add(
                "IBlockElementPropertyForPlaylistPreviewImageFilePath",
                new BXParamSingleSelection(
                    GetMessageRaw("IBlockElementPropertyForPlaylistPreviewImageFilePath"),
                    "",
                    dataSourceCategory
                )
            );

            ParamsDefinition.Add(
                "IBlockElementPropertyForPlayerPreviewImageFilePath",
                new BXParamSingleSelection(
                    GetMessageRaw("IBlockElementPropertyForPlayerPreviewImageFilePath"),
                    "",
                    dataSourceCategory
                )
            );

            ParamsDefinition.Add(
                "IBlockElementPropertyForDownloadingFilePath",
                new BXParamSingleSelection(
                    GetMessageRaw("IBlockElementPropertyForDownloadingFilePath"),
                    "",
                    BXCategory.DataSource
                )
            );

            BXCategory listSettings = BXCategory.ListSettings;
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

            ParamsDefinition.Add(
                "ListProperties",
                new BXParamMultiSelection(
                    GetMessageRaw("Properties"),
                    "-",
                    listSettings
                )
            );

            //ParamsDefinition.Add(
            //    "ListProperties",
            //    new BXParamDoubleList(
            //        GetMessageRaw("Properties"),
            //        "",
            //        listSettings
            //    )
            //);

            BXCategory detailSettings = BXCategory.DetailSettings;
            ParamsDefinition.Add(
                "DetailProperties",
                new BXParamMultiSelection(
                    GetMessageRaw("Properties"),
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
                "PlayerEnableFullScreenModeSwitch",
                new BXParamYesNo(
                    GetMessageRaw("PlayerEnableFullScreenModeSwitch"),
                    true,
                    detailSettings
                    )
            );

            ParamsDefinition.Add(
                "PlayerWidth",
                new BXParamText(
                    GetMessageRaw("PlayerWidth"),
                    "425px",
                    detailSettings
                    )
            );

            ParamsDefinition.Add(
                "PlayerHeight",
                new BXParamText(
                    GetMessageRaw("PlayerHeight"),
                    "344px",
                    detailSettings
                    )
            );

            ParamsDefinition.Add(
                "PlayerStretching",
                new BXParamSingleSelection(
                    GetMessageRaw("PlayerStretching"),
                    Enum.GetName(typeof(MediaPlayerStretchingMode), MediaPlayerStretchingMode.Proportionally),
                    detailSettings
                    )
            );

            string groupControlPanel = "controlPanel";
            ParamsDefinition.Add(
                "PlayerShowControlPanel",
                new BXParamYesNo(
                    GetMessageRaw("PlayerShowControlPanel"),
                    true,
                    detailSettings,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "PlayerShowControlPanel", groupControlPanel, string.Empty)
                    )
            );

            ParamsDefinition.Add(
                "PlayerControlPanelBackgroundColor",
                new BXParamText(
                    GetMessageRaw("PlayerControlPanelBackgroundColor"),
                    "FFFFFF",
                    detailSettings,
                    new ParamClientSideActionGroupViewMember(ClientID, "PlayerControlPanelBackgroundColor", new string[] { groupControlPanel })
                    )
            );

            ParamsDefinition.Add(
                "PlayerControlsColor",
                new BXParamText(
                    GetMessageRaw("PlayerControlsColor"),
                    "000000",
                    detailSettings,
                    new ParamClientSideActionGroupViewMember(ClientID, "PlayerControlsColor", new string[] { groupControlPanel })
                    )
            );

            ParamsDefinition.Add(
                "PlayerControlsOverColor",
                new BXParamText(
                    GetMessageRaw("PlayerControlsOverColor"),
                    "000000",
                    detailSettings,
                    new ParamClientSideActionGroupViewMember(ClientID, "PlayerControlsOverColor", new string[] { groupControlPanel })
                    )
            );

            ParamsDefinition.Add(
                "PlayerScreenColor",
                new BXParamText(
                    GetMessageRaw("PlayerScreenColor"),
                    "000000",
                    detailSettings
                    )
            );

            ParamsDefinition.Add(
                "PlayerVolumeLevelInPercents",
                new BXParamText(
                    GetMessageRaw("PlayerVolumeLevelInPercents"),
                    "90",
                    detailSettings
                    )
            );

            ParamsDefinition.Add(
                "PlayerBufferLengthInSeconds",
                new BXParamText(
                    GetMessageRaw("PlayerBufferLengthInSeconds"),
                    "10",
                    detailSettings
                    )
            );

            ParamsDefinition.Add(
                "PlayerEnableAutoStart",
                new BXParamYesNo(
                    GetMessageRaw("PlayerEnableAutoStart"),
                    false,
                    detailSettings
                    )
            );

            ParamsDefinition.Add(
                "PlayerEnableRepeatMode",
                new BXParamYesNo(
                    GetMessageRaw("PlayerEnableRepeatMode"),
                    false,
                    detailSettings
                    )
            );

            string groupDownloading = "downloading";
            ParamsDefinition.Add(
                "PlayerEnableDownloading",
                new BXParamYesNo(
                    GetMessageRaw("PlayerEnableDownloading"),
                    true,
                    detailSettings,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "PlayerEnableDownloading", groupDownloading, string.Empty)
                    )
            );

            ParamsDefinition.Add(
                "PlayerDownloadingLinkTargetWindow",
                new BXParamSingleSelection(
                    GetMessageRaw("PlayerDownloadingLinkTargetWindow"),
                    "_self",
                    detailSettings,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "PlayerDownloadingLinkTargetWindow", new string[] { groupDownloading })
                    )
            );
            /*BXCategory treeSettings = new BXCategory(GetMessageRaw("SectionTreeCategory"), "treeSettings", 850);
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
            );*/

            BXCategory additionalSettings = BXCategory.AdditionalSettings;
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

            string clientSideActionGroupViewId = ClientID;

            BXCategory topSettings = new BXCategory(GetMessageRaw("TopSectionSortBy.Category"), "topSettings", 900);
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
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SectionIdVariable", new string[] { "NonSef" })
                )
            );

            ParamsDefinition.Add(
                "ElementIdVariable",
                new BXParamText(
                    GetMessageRaw("ElementID"),
                    "element_id",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ElementIdVariable", new string[] { "NonSef" })
                )
            );

            ParamsDefinition.Add(
                "PageIdVariable",
                new BXParamText(
                    GetMessageRaw("PageIdVariable"),
                    "page",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "PageIdVariable", new string[] { "NonSef" })
                )
            );

            ParamsDefinition.Add(
                "ShowAllVariable",
                new BXParamText(
                    GetMessageRaw("ShowAllVariable"),
                    "showall",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ShowAllVariable", new string[] { "NonSef" })
                )
            );

            ParamsDefinition.Add(
                "ElementDetailTemplate",
                new BXParamText(
                    GetMessageRaw("ElementDetailTemplate"),
                    "/#SectionId#/item-#ElementId#/",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ElementDetailTemplate", new string[] { "Sef" })
                )
            );

            ParamsDefinition.Add(
                "ElementListTemplate",
                new BXParamText(
                    GetMessageRaw("ElementListTemplate"),
                    "/#SectionId#/",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ElementListTemplate", new string[] { "Sef" })
                )
            );

            ParamsDefinition.Add(
                "CommonListPageTemplate",
                new BXParamText(
                    GetMessageRaw("CommonListPageTemplate"),
                    "/page-#PageId#",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommonListPageTemplate", new string[] { "Sef" })
                )
            );

            ParamsDefinition.Add(
                "CommonListShowAllTemplate",
                new BXParamText(
                    GetMessageRaw("CommonListShowAllTemplate"),
                    "/all",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommonListShowAllTemplate", new string[] { "Sef" })
                )
            );

            ParamsDefinition.Add(
                "SectionListPageTemplate",
                new BXParamText(
                    GetMessageRaw("SectionListPageTemplate"),
                    "/#SectionId#/page-#PageId#",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SectionListPageTemplate", new string[] { "Sef" })
                )
            );

            ParamsDefinition.Add(
                "SectionListShowAllTemplate",
                new BXParamText(
                    GetMessageRaw("SectionListShowAllTemplate"),
                    "/#SectionId#/all",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SectionListShowAllTemplate", new string[] { "Sef" })
                )
            );

            ParamsDefinition.Add(BXParametersDefinition.GetPaging(clientSideActionGroupViewId));
            ParamsDefinition.Add(BXParametersDefinition.Cache);
            //ParamsDefinition.Add(BXParametersDefinition.Search);
            //ParamsDefinition.Add(BXParametersDefinition.Ajax);

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

            if (selectedIblockId > 0)
            {
                BXCustomFieldCollection customFields = BXIBlock.GetCustomFields(selectedIblockId);
                foreach (BXCustomField customField in customFields)
                {
                    string title = BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel);
                    string code = customField.Name.ToUpper();
                    properties.Add(new BXParamValue(title, code));
                    sortProperties.Add(new BXParamValue(title, "-" + code));
                }
            }

            //List<BXParamValue> properties1 = new List<BXParamValue>(properties);
            //properties1.RemoveAt(0);
            //ParamsDefinition["ListProperties"].Values = properties1;
            ParamsDefinition["ListProperties"].Values = properties;

            ParamsDefinition["DetailProperties"].Values = properties;
            ParamsDefinition["PropertyKeywords"].Values = properties;
            ParamsDefinition["PropertyDescription"].Values = properties;
            ParamsDefinition["TopProperties"].Values = properties;
            ParamsDefinition["IBlockElementPropertyForFilePath"].Values = properties;
            ParamsDefinition["IBlockElementPropertyForPlaylistPreviewImageFilePath"].Values = properties;
            ParamsDefinition["IBlockElementPropertyForPlayerPreviewImageFilePath"].Values = properties;
            ParamsDefinition["IBlockElementPropertyForDownloadingFilePath"].Values = properties;

            //Sorting
            List<BXParamValue> sortingFields = new List<BXParamValue>();
            sortingFields.Add(new BXParamValue(GetMessageRaw("ElementID"), "ID"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("ElementName"), "Name"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("ActiveFromDate"), "ActiveFromDate"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("ActiveUntilDate"), "ActiveToDate"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("SortIndex"), "Sort"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("DateOfModification"), "UpdateDate"));
            sortingFields.AddRange(sortProperties);

            ParamsDefinition["ListSortBy"].Values = sortingFields;
            ParamsDefinition["TopSortBy"].Values = sortingFields;

            #region PlayerStretching
            string[] playerStretchingModeNameArr = Enum.GetNames(typeof(MediaPlayerStretchingMode));
            int playerStretchingModeNameArrCount = playerStretchingModeNameArr != null ? playerStretchingModeNameArr.Length : 0;
            if (playerStretchingModeNameArrCount > 0)
            {
                BXParamValue[] paramValueArr = new BXParamValue[playerStretchingModeNameArrCount];
                for (int i = 0; i < playerStretchingModeNameArrCount; i++)
                {
                    string playerStretchingModeName = playerStretchingModeNameArr[i];
                    paramValueArr[i] = new BXParamValue(GetMessageRaw(string.Concat("MediaPlayerStretchingMode", playerStretchingModeName)), playerStretchingModeName);
                }
                BXParam palyerStretchingModeParam = ParamsDefinition["PlayerStretching"];
                IList<BXParamValue> palyerStretchingModeParamVals = palyerStretchingModeParam.Values;
                if (palyerStretchingModeParamVals != null)
                {
                    if (palyerStretchingModeParamVals.Count > 0)
                        palyerStretchingModeParamVals.Clear();
                    for (int j = 0; j < playerStretchingModeNameArrCount; j++)
                        palyerStretchingModeParamVals.Add(paramValueArr[j]);
                }
                else
                    palyerStretchingModeParam.Values = new List<BXParamValue>(paramValueArr);
            }
            else
            {
                IList<BXParamValue> palyerStretchingModeParamVals = ParamsDefinition["PlayerStretching"].Values;
                if (palyerStretchingModeParamVals != null && palyerStretchingModeParamVals.Count > 0)
                    palyerStretchingModeParamVals.Clear();
            }
            #endregion

            #region PlayerDownloadingLinkTargetWindow
            IList<BXParamValue> playerDownloadingLinkTargetWindowVals = ParamsDefinition["PlayerDownloadingLinkTargetWindow"].Values;
            playerDownloadingLinkTargetWindowVals.Add(new BXParamValue(GetMessageRaw("PlayerDownloadingLinkTargetWindowSelf"), "_self"));
            playerDownloadingLinkTargetWindowVals.Add(new BXParamValue(GetMessageRaw("PlayerDownloadingLinkTargetWindowBlank"), "_blank"));
            #endregion
        }
        #endregion

        public override void ProcessMessage(BXCommand cmd, bool executionContext, string executionVirtualPath)
        {
            if (cmd.Action == "Bitrix.Main.GeneratePublicMenu")
            {
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

                    MapVariable(SefFolder, urlToPage, request, "index", BXUri.ToRelativeUri(cmd.Parameters.Get<string>("uri")));
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
                string permissionCacheKey = "mediaGallery-" + IBlockId.ToString() + "-menu-permission";
                if ((menuItemPermisson = (string)BXCacheManager.MemoryCache.Get(permissionCacheKey)) == null)
                {
                    menuItemPermisson = string.Empty;
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
                        elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.SectionId, BXSqlFilterOperators.Equal, sectionId));
                    if (!String.IsNullOrEmpty(sectionCode))
                        elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.Code, BXSqlFilterOperators.Equal, sectionCode));

                    BXIBlockElementCollection element = BXIBlockElement.GetList(elementFilter, null);
                    if (element != null && element.Count > 0)
                    {
                        BXIBlockElement.BXInfoBlockElementSectionCollection sections = element[0].Sections;
                        if (sections != null && sections.Count > 0)
                        {
                            BXParamsBag<object> replaceItems = new BXParamsBag<object>();
                            replaceItems["SectionId"] = sections[0].SectionId;
                            replaceItems["SectionCode"] = GetSectionCode(sections[0].SectionId);

                            parentLevelUri = MakeMenuUriForElementList(executionVirtualPath, replaceItems); //Меню строится для раздела, к которому привязан элемент
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
                        )
                    );

                    int previousDepthLevel = 1;
                    int previousSectionId = 0;
                    Dictionary<int, MenuSectionTree> sectionTree = new Dictionary<int, MenuSectionTree>();
                    foreach (BXIBlockSection section in sectionsList)
                    {
                        BXParamsBag<object> replaceItems = new BXParamsBag<object>();
                        replaceItems["SectionId"] = section.Id;
                        replaceItems["SectionCode"] = section.Code ?? String.Empty;

                        sectionTree[section.Id] = new MenuSectionTree();
                        string url = MakeMenuUriForElementList(executionVirtualPath, replaceItems);
                        sectionTree[section.Id].url = url;
                        sectionTree[section.Id].sectionId = section.SectionId;

                        //Если предыдущий раздел не имеет вложенных (дочерних) разделов, то для него дополнительно указывается URL на родительскую секцию (и эл-ты меню для данной секции будут запрошены из этой родительской)
                        if (previousSectionId > 0 && section.DepthLevel <= previousDepthLevel)
                            sectionTree[previousSectionId].parentLevelUrl = sectionTree[sectionTree[previousSectionId].sectionId].url;
                        previousDepthLevel = section.DepthLevel;
                        previousSectionId = section.Id;

                        BXPublicMenuItem menuItem = new BXPublicMenuItem();
                        menuItem.Title = section.Name;
                        menuItem.Links.Add(url);
                        menuItem.Sort = section.LeftMargin;

                        if (!String.IsNullOrEmpty(menuItemPermisson))
                        {
                            menuItem.ConditionType = ConditionType.Group;
                            menuItem.Condition = menuItemPermisson;
                        }

                        if (!sectionTree.ContainsKey(section.SectionId))
                        {
                            sectionTree[section.SectionId] = new MenuSectionTree();
                            sectionTree[section.SectionId].menuItems = new List<BXPublicMenuItem>();

                            if (section.SectionId < 1)
                                sectionTree[section.SectionId].url = EnableSef ? SefFolder : BXUri.ToAppRelativeUri(executionVirtualPath);
                        }

                        if (sectionTree[section.SectionId].menuItems == null)
                            sectionTree[section.SectionId].menuItems = new List<BXPublicMenuItem>();

                        sectionTree[section.SectionId].menuItems.Add(menuItem);
                    }

                    //Последний раздел точно не имеет вложенных (дочерних) разделов, для него обязательно указывается URL на родительскую секцию (и эл-ты меню для данной секции будут запрошены из этой родительской)
                    if (sectionTree.ContainsKey(previousSectionId) && sectionTree.ContainsKey(sectionTree[previousSectionId].sectionId))
                        sectionTree[previousSectionId].parentLevelUrl = sectionTree[sectionTree[previousSectionId].sectionId].url;

                    foreach (KeyValuePair<int, MenuSectionTree> sectionItem in sectionTree)
                        menuTree.Add(
                            sectionItem.Value.url,
                            new BXPublicMenu.BXLoadMenuCommandParameter(sectionItem.Value.menuItems, true, sectionItem.Value.parentLevelUrl)
                        );
                }

                if (menuTree.Count > 0)
                    cmd.AddCommandResult("bitrix:mediaGallery@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, menuTree));
                else if (menuList != null || parentLevelUri != null)
                {
                    BXPublicMenu.BXLoadMenuCommandParameter menuResult = new BXPublicMenu.BXLoadMenuCommandParameter(menuList, true, parentLevelUri);
                    cmd.AddCommandResult("bitrix:mediaGallery@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, menuResult));
                }
            }
        }

        private string GetSectionCode(int sectionId)
        {
            string sectionCode = string.Empty;

            if (sectionId > 0 && EnableSef && ElementListTemplate.Contains("#SectionCode#"))
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
        private string MakeMenuUriForElementList(string executionVirtualPath, BXParamsBag<object> replaceItems)
        {
            string url = string.Empty;
            replaceItems["IblockId"] = IBlockId;
            if (EnableSef)
            {
                url = "~/";
                //обрезаемые символы в начале пути
                char[] trimmedChars = new char[] { '~', '/' };
                string sefFolder = SefFolder;
                //присоединяем папку
                if (!string.IsNullOrEmpty(sefFolder)) 
                {
                    sefFolder = sefFolder.TrimStart(trimmedChars);
                    url = VirtualPathUtility.Combine(url, sefFolder);
                    url = VirtualPathUtility.AppendTrailingSlash(url);
                }
                //присоединяем шаблон списка
                string elementListTemplate = ElementListTemplate;
                if (!string.IsNullOrEmpty(elementListTemplate))
                {
                    elementListTemplate = elementListTemplate.TrimStart(trimmedChars);
                    url = VirtualPathUtility.Combine(url, elementListTemplate);
                }
                url = MakeLink(url, replaceItems);
            }
            else
				url = MakeLink(String.Format("{0}?{1}=#SectionId#", BXSiteRemapUtility.UnmapVirtualPath(executionVirtualPath), SectionIdVariable), replaceItems);

            return url;
        }
        private class MenuSectionTree
        {
            public string parentLevelUrl = null;
            public List<BXPublicMenuItem> menuItems;
            public string url;
            public int sectionId;
        }

		/// <summary>
		/// Режим растягивания медиа плеера
		/// </summary>
		public enum MediaPlayerStretchingMode
		{
			None = 0,
			Proportionally = 1,
			Disproportionally = 2,
			Fill = 3
		}
    }
}

