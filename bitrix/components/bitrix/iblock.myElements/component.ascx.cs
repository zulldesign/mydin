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
using Bitrix.Security;
using Bitrix.DataTypes;
using Bitrix.Components.Editor;
using System.Collections.Generic;
using Bitrix.IBlock;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.Services.Text;

namespace Bitrix.IBlock.Components
{
    public partial class IBlockMyElementsComponent : Bitrix.UI.BXComponent
    {
        #region Enums
        public enum MannerOfUserAssociation
        {
            NoBody = 1,
            CreatedBy,
            IBlockProperty
        }

        /// <summary>
        /// Способ определения пользователя
        /// </summary>
        public enum MannerOfUserIdentification
        {
            /// <summary>
            /// Текущий
            /// </summary>
            Current = 1,
            /// <summary>
            /// Указанный в параметре CustomUserId
            /// </summary>
            Custom
        }

        public enum MannerOfIssueModificationPermission
        {
            Active = 1,
            NotActive,
            Always
        }

        public enum ElementStatus
        {
            Active = 1,
            NotActive
        }
        #endregion

        #region Events
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string templateName = "list";
            string action = string.Empty;

            if (EnableSef)
            {
                BXParamsBag<string> sefTemplates = new BXParamsBag<string>();
                sefTemplates.Add("list$page", SefTemplatePageIndex);
                sefTemplates.Add("list$all", SefTemplateShowAll);
                sefTemplates.Add("form$new", SefTemplateCreateElement);
                sefTemplates.Add("form$edit", SefTemplateModifyElement);


                string code = MapVariable(SefFolder, sefTemplates, ComponentCache, "list");

                int position = code.IndexOf('$');
                if (position > 0)
                {
                    templateName = code.Substring(0, position);
                    action = code.Substring(position + 1);
                }
                else
                {
                    templateName = code;
                    action = string.Empty;
                }


                if (string.Equals(templateName, "form", StringComparison.InvariantCultureIgnoreCase))
                {
                    if ((string.Equals(action, "edit", StringComparison.InvariantCultureIgnoreCase) && !InternalBehaviour.IsElementModificationAllowed()) ||
                        (string.Equals(action, "new", StringComparison.InvariantCultureIgnoreCase) && !InternalBehaviour.IsElementCreationAllowed())
                        )
                    {
                        templateName = "list";
                        action = string.Empty;
                    }
                }


                if (string.Equals(templateName, "list", StringComparison.InvariantCultureIgnoreCase))
                {
                    BXParamsBag<object> replaceItems = new BXParamsBag<object>(ComponentCache);
                    replaceItems.Remove("PageID");  

                    replaceItems["IblockId"] = replaceItems["IBLOCK_ID"] = IBlockId.ToString();

                    ComponentCache["PagingIndexTemplate"] = CombineLink(SefFolder, string.Empty);
                    ComponentCache["PagingPageTemplate"] = CombineLink(SefFolder, MakeLink(SefTemplatePageIndex, replaceItems));
                    ComponentCache["PagingShowAllTemplate"] = CombineLink(SefFolder, MakeLink(SefTemplateShowAll, replaceItems));
                    ComponentCache["ShowAll"] = string.Equals(code, "list$all", StringComparison.Ordinal) ? "y" : "n";
                    ComponentCache["ElementCreationUrl"] = CombineLink(SefFolder, MakeLink(SefTemplateCreateElement, replaceItems));
                    ComponentCache["ElementModificationUrl"] = CombineLink(SefFolder, MakeLink(SefTemplateModifyElement, replaceItems));
                }
                else if (string.Equals(templateName, "form", StringComparison.InvariantCultureIgnoreCase))
                    Parameters["RedirectPageUrl"] = CombineLink(SefFolder, string.Empty);
            }
            else
            {
                bool aboutCreateEl = false,
                        aboutModifyEl = false;
                string createElParName = RequestParamNameCreateElement,
                        modifyElParName = RequestParamNameModifyElement;

                if (!string.IsNullOrEmpty(modifyElParName))
                {
                    string elementID = Request.QueryString.Get(modifyElParName);
                    if (!string.IsNullOrEmpty(elementID))
                    {
                        aboutModifyEl = true;
                        ComponentCache["ElementID"] = elementID;
                    }
                }

                if (!aboutModifyEl && !string.IsNullOrEmpty(createElParName))
                {
                    int keyCount = Request.QueryString.Count;
                    for (int i = 0; i < keyCount; i++)
                    {
                        if (!string.Equals(Request.QueryString[i], createElParName, StringComparison.InvariantCultureIgnoreCase))
                            continue;
                        aboutCreateEl = true;
                        ComponentCache["ElementID"] = "0";
                        break;
                    }
                }

                if ((aboutModifyEl && InternalBehaviour.IsElementModificationAllowed()) || (aboutCreateEl && InternalBehaviour.IsElementCreationAllowed()))
                {
                    templateName = "form";
                    Parameters["RedirectPageUrl"] = BXSefUrlManager.CurrentUrl.AbsolutePath;
                }
                else
                {
                    BXParamsBag<string> requestParamAliases = new BXParamsBag<string>();
                    requestParamAliases["PageID"] = RequestParamNamePageIndex;
                    requestParamAliases["ShowAll"] = RequestParamNameShowAll;
                    BXComponentManager.MapVariable(requestParamAliases, ComponentCache, true);

                    if (ComponentCache.ContainsKey("ShowAll") && string.IsNullOrEmpty(ComponentCache.GetString("ShowAll", null)))
                        ComponentCache["ShowAll"] = "y";

                    string filePath = BXSefUrlManager.CurrentUrl.AbsolutePath;

                    ComponentCache["PagingIndexTemplate"] = filePath;
                    ComponentCache["PagingPageTemplate"] = String.Format("{0}?{1}=#PageId#", filePath, RequestParamNamePageIndex);
                    ComponentCache["PagingShowAllTemplate"] = String.Format("{0}?{1}", filePath, RequestParamNameShowAll);

                    ComponentCache["ElementCreationUrl"] = !string.IsNullOrEmpty(createElParName) ? string.Concat("?", createElParName) : string.Empty;
                    ComponentCache["ElementModificationUrl"] = !string.IsNullOrEmpty(modifyElParName) ? string.Concat("?", modifyElParName, "=#ElementID#") : string.Empty;
                }
            }
            IncludeComponentTemplate(templateName);
        }
        #endregion

        #region Component
        /// <summary>
        /// ID инфоблока
        /// </summary>
        public int IBlockId
        {
            get { return Parameters.Get<int>("IBlockId"); }
        }

        private bool _isIBlockDataLoaded = false;
        protected void EnsureIBlockDataLoaded()
        {
            if (_isIBlockDataLoaded)
                return;
            if (IBlockId > 0)
            {
                BXIBlockCollection iblockCol = BXIBlock.GetList(
                    new BXFilter(
                        new BXFilterItem(BXIBlock.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlock.Fields.ID, BXSqlFilterOperators.Equal, IBlockId)
                    ),
                    null,
                    null,
                    null,
                    BXTextEncoder.HtmlTextEncoder
                );
                if(iblockCol.Count > 0)
                    _iblock = iblockCol[0];
            }
            _isIBlockDataLoaded = true;
        }
        private BXIBlock _iblock;
        public BXIBlock IBlock
        {
            get
            {
                EnsureIBlockDataLoaded();    
                return _iblock;
            }
        }

        ///// <summary>
        ///// Флаг включения режима ЧПУ
        ///// </summary>
        //public bool EnableSef
        //{
        //    get { return Parameters.Get<bool>("EnableSef", false); }
        //}

        /// <summary>
        ///Каталог ЧПУ (относительно корня сайта)
        /// </summary>
        public string SefFolder
        {
            get { return Parameters.Get<string>("SEFFolder", string.Empty); }
        }

        /// <summary>
        /// Псевдоним параметра запроса для флага создания нового эл-та.
        /// </summary>
        public string RequestParamNameCreateElement
        {
            get { return Parameters.Get<string>("RequestParamNameCreateElement", "new"); }
        }

        /// <summary>
        /// Псевдоним параметра запроса для ID редактируемого эл-та.
        /// </summary>
        public string RequestParamNameModifyElement
        {
            get { return Parameters.Get<string>("RequestParamNameModifyElement", "id"); }
        }

        /// <summary>
        /// Псевдоним параметра запроса для индекса страницы в списке эл-тов
        /// </summary>
        public string RequestParamNamePageIndex
        {
            get { return Parameters.Get<string>("RequestParamNamePageIndex", "page"); }
        }

        /// <summary>
        /// Псевдоним параметра запроса для флага отображения всех эл-тов
        /// </summary>
        public string RequestParamNameShowAll
        {
            get { return Parameters.Get<string>("RequestParamNameShowAll", "all"); }
        }

        /// <summary>
        /// ЧПУ-шаблон для сценария создания эл-та
        /// </summary>
        public string SefTemplateCreateElement
        {
            get { return Parameters.Get<string>("SefTemplateCreateElement", "/new/"); }
        }

        /// <summary>
        /// ЧПУ-шаблон для сценария редактирования эл-та
        /// </summary>
        public string SefTemplateModifyElement
        {
            get { return Parameters.Get<string>("SefTemplateModifyElement", "/edit-#ElementId#/"); }
        }

        /// <summary>
        /// ЧПУ-шаблон для сценария отображения страницы в списке эл-тов
        /// </summary>
        public string SefTemplatePageIndex
        {
            get { return Parameters.Get<string>("SefTemplatePageIndex", "/page-#PageId#"); }
        }

        /// <summary>
        /// ЧПУ-шаблон для сценария отображения всех эл-тов в списке
        /// </summary>
        public string SefTemplateShowAll
        {
            get { return Parameters.Get<string>("SefTemplateShowAll", "/all"); }
        }

        public MannerOfUserIdentification UserIdentification
        {
            get
            {
                object obj = null;
                if (ComponentCache.TryGetValue("MannerOfUserIdentification", out obj))
                    return (MannerOfUserIdentification)obj;

                MannerOfUserIdentification r = Parameters.GetEnum<MannerOfUserIdentification>("MannerOfUserIdentification", MannerOfUserIdentification.Current);
                ComponentCache["MannerOfUserIdentification"] = r;
                return r;
            } 
        }

        public bool PermitOfCreation
        {
            get
            {
                object obj = null;
                if (ComponentCache.TryGetValue("PermitOfCreation", out obj))
                    return (bool)obj;

                bool r = Parameters.Get<bool>("PermitOfCreation", false);
                ComponentCache["PermitOfCreation"] = r;
                return r;
            }
        }

        public bool PermitOfModification
        {
            get
            {
                object obj = null;
                if (ComponentCache.TryGetValue("PermitOfModification", out obj))
                    return (bool)obj;

                bool r = Parameters.Get<bool>("PermitOfModification", false);
                ComponentCache["PermitOfModification"] = r;
                return r;
            }
        }

        public bool PermitOfDeletion
        {
            get
            {
                object obj = null;
                if (ComponentCache.TryGetValue("PermitOfDeletion", out obj))
                    return (bool)obj;

                bool r = Parameters.Get("PermitOfDeletion", false);
                ComponentCache["PermitOfDeletion"] = r;
                return r;
            }
        }

        public MannerOfIssueModificationPermission IssueModificationPermission
        {
            get
            {
                object obj = null;
                if (ComponentCache.TryGetValue("MannerOfIssueModificationPermission", out obj))
                    return (MannerOfIssueModificationPermission)obj;

                MannerOfIssueModificationPermission r = Parameters.GetEnum<MannerOfIssueModificationPermission>("MannerOfIssueModificationPermission", MannerOfIssueModificationPermission.Always);
                ComponentCache["MannerOfIssueModificationPermission"] = r;
                return r;
            }
        }

        private BXPrincipal _currentPricipal = null;
        public BXPrincipal CurrentPricipal
        {
            get { return _currentPricipal ?? (_currentPricipal = Context.User as BXPrincipal); }
        }

        private int? _currentUserId = null;
        public int CurrentUserId
        {
            get { return _currentUserId ?? (_currentUserId = CurrentPricipal != null && CurrentPricipal.Identity.IsAuthenticated ? ((BXIdentity)CurrentPricipal.Identity).Id : 0).Value; }
        }

        public int CustomUserId
        {
            get
            {
                object obj = null;
                if (ComponentCache.TryGetValue("CustomUserId", out obj))
                    return (int)obj;

                int r = Parameters.GetInt("CustomUserId", 0);
                ComponentCache["CustomUserId"] = r;
                return r;
            }
        }

        private IList<string> _rolesAuthorizedToManage = null;
        /// <summary>
        /// Группы пользователей, имеющие право на добавление/редактирование своих элементов
        /// </summary>
        public IList<string> RolesAuthorizedToManage
        {
            get { return _rolesAuthorizedToManage ?? (_rolesAuthorizedToManage = Parameters.GetListString("RolesAuthorizedToManage")); }
        }

        private IList<string> _rolesAuthorizedToAdminister = null;
        /// <summary>
        /// Группы пользователей, имеющие право на добавление/редактирование чужих элементов
        /// </summary>
        public IList<string> RolesAuthorizedToAdminister
        {
            get { return _rolesAuthorizedToAdminister ?? (_rolesAuthorizedToAdminister = Parameters.GetListString("RolesAuthorizedToAdminister")); }
        }

        private IList<string> _rolesAuthorizedToView = null;
        /// <summary>
        /// Группы пользователей, имеющие право на просмотр чужих элементов
        /// </summary>
        public IList<string> RolesAuthorizedToView
        {
            get { return _rolesAuthorizedToView ?? (_rolesAuthorizedToView = Parameters.GetListString("RolesAuthorizedToView")); }
        }
        private Behaviour _internalBehaviour = null;
        internal Behaviour InternalBehaviour
        {
            get
            {
                if (_internalBehaviour != null)
                    return _internalBehaviour;

                if (UserIdentification == MannerOfUserIdentification.Custom && CustomUserId > 0)
                {
                    if (Behaviour.IsPrincipalInRoles(CurrentPricipal, RolesAuthorizedToAdminister))
                        return (_internalBehaviour = new AdministerBehaviour(this));
                    else if (CurrentUserId == CustomUserId)
                        return (_internalBehaviour = new StandardBehaviour(this));
                    else if (Behaviour.IsPrincipalInRoles(CurrentPricipal, RolesAuthorizedToView))
                        return (_internalBehaviour = new ViewBehaviour(this));
                    else
                        return (_internalBehaviour = new AccessDeniedBehaviour(this));
                }
                return (_internalBehaviour = new StandardBehaviour(this));
            }
        }

        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/eadd.gif";
            Group = new BXComponentGroup("iblock.elements", GetMessageRaw("Group"), 100, BXComponentGroup.Content);

            //BXCategory dataSourceCategory = BXCategory.DataSource;
            BXCategory mainCategory = BXCategory.Main;
            BXCategory sefCategory = BXCategory.Sef;
            BXCategory accessCategory = new BXCategory(GetMessageRaw("Category.Access"), "ACCESS", 110);
            BXCategory listSettings = BXCategory.ListSettings;
            listSettings.Sort = 120;
            BXCategory webFormSettings = new BXCategory(GetMessageRaw("Category.WebFormGeneral"), "WEB_FORM_GENERAL_SETTINGS", 130);
            BXCategory fieldSettingsCategory = new BXCategory(GetMessageRaw("Category.WebFormFields"), "WEB_FORM_FIELD_SETTINGS", 140);
            BXCategory customNameCategory = new BXCategory(GetMessageRaw("Category.WebFormCustomNames"), "WEB_FORM_CUSTOM_NAME_SETTINGS", 150);

            ParamsDefinition.Add(BXParametersDefinition.Paging);

            BXParamsBag<BXParam> sefParBag = BXParametersDefinition.Sef;
            sefParBag["EnableSEF"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(ClientID, "EnableSEF", "Sef", "NonSef");
            sefParBag["SEFFolder"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "SEFFolder", new string[] { "Sef" });
            ParamsDefinition.Add(sefParBag);

            #region Main
            ParamsDefinition.Add(
                "IBlockTypeId",
                new BXParamSingleSelection(
                    GetMessageRaw("InfoBlockType"),
                    String.Empty,
                    mainCategory
                ));

            ParamsDefinition.Add(
                "IBlockId",
                new BXParamSingleSelection(
                    GetMessageRaw("InfoBlockCode"),
                    String.Empty,
                    mainCategory
                ));

            #endregion

            #region Access
            BXRole admin = BXRoleManager.GetById(1);
            ParamsDefinition.Add(
                "MannerOfUserIdentification",
                new BXParamSingleSelection(
                    GetMessageRaw("MannerOfUserIdentification"),
                    MannerOfUserIdentification.Current.ToString("G"),
                    accessCategory,
                    null,
                    new ParamClientSideActionGroupViewSelector(ClientID, "MannerOfUserIdentification")
                ));

            ParamsDefinition.Add(
                "CustomUserId",
                new BXParamText(
                    GetMessageRaw("CustomUserId"),
                    "0",
                    accessCategory,
                new ParamClientSideActionGroupViewMember(ClientID, "CustomUserId", new string[] { MannerOfUserIdentification.Custom.ToString("G") })
                ));

            //просмотр своих записей
            ParamsDefinition.Add(
                "RolesAuthorizedToManage",
                new BXParamMultiSelection(
                    GetMessageRaw("RolesAuthorizedToManage"),
                    admin != null ? admin.RoleName : string.Empty,
                    accessCategory
                ));

            ParamsDefinition.Add(
                "PermitOfCreation",
                new BXParamYesNo(
                    GetMessageRaw("PermitOfCreation"),
                    false,
                    accessCategory,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "PermitOfCreation", "PermitOfElementCreation", string.Empty)
                ));

            ParamsDefinition.Add(
                "MaxUserElements",
                new BXParamText(
                    GetMessageRaw("MaxUserElements"),
                    "15",
                    accessCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "MaxUserElements", new string[] { "PermitOfElementCreation" })
                ));

            ParamsDefinition.Add(
                "PermitOfModification",
                new BXParamYesNo(
                    GetMessageRaw("PermitOfModification"),
                    false,
                    accessCategory,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "PermitOfModification", "PermitOfElementModification", string.Empty)
                ));

            ParamsDefinition.Add(
                "MannerOfIssueModificationPermission",
                new BXParamSingleSelection(
                    GetMessageRaw("MannerOfIssueModificationPermission"),
                    MannerOfIssueModificationPermission.Active.ToString("G"),
                    accessCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "MannerOfIssueModificationPermission", new string[] { "PermitOfElementModification" })
                ));

            ParamsDefinition.Add(
                "PermitOfDeletion",
                new BXParamYesNo(
                    GetMessageRaw("PermitOfDeletion"),
                    false,
                    accessCategory
                ));

            //просмотр чужих записей
            ParamsDefinition.Add(
                "RolesAuthorizedToView",
                new BXParamMultiSelection(
                    GetMessageRaw("RolesAuthorizedToView"),
                    admin != null ? admin.RoleName : string.Empty,
                    accessCategory
                ));

            //управление чужими записями
            ParamsDefinition.Add(
                "RolesAuthorizedToAdminister",
                new BXParamMultiSelection(
                    GetMessageRaw("RolesAuthorizedToAdminister"),
                    admin != null ? admin.RoleName : string.Empty,
                    accessCategory
                ));


            ParamsDefinition.Add(
                "RolesAuthorizedToManageOfActivation",
                new BXParamMultiSelection(
                    GetMessageRaw("RolesAuthorizedToManageOfActivation"),
                    admin != null ? admin.RoleName : string.Empty,
                    accessCategory
                ));

            ParamsDefinition.Add(
                "ElementActiveAfterSave",
                new BXParamSingleSelection(
                    GetMessageRaw("ElementActiveAfterSave"),
                    MannerOfIssueModificationPermission.Active.ToString("G"),
                    accessCategory
                ));

            ParamsDefinition.Add(
                "MannerOfUserAssociation",
                new BXParamSingleSelection(
                    GetMessageRaw("MannerOfUserAssociation"),
                    MannerOfUserAssociation.CreatedBy.ToString("G"),
                    accessCategory,
                    null,
                    new ParamClientSideActionGroupViewSelector(ClientID, "MannerOfUserAssociation")
                ));

            ParamsDefinition.Add(
                "UserAssociatedByCustomIBlockProperty",
                new BXParamSingleSelection(
                    GetMessageRaw("UserAssociatedByCustomIBlockProperty"),
                    string.Empty,
                    accessCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "UserAssociatedByCustomIBlockProperty", new string[] { MannerOfUserAssociation.IBlockProperty.ToString("G") })
                ));
            #endregion

            #region List
            ParamsDefinition.Add(
                "SortBy",
                new BXParamSingleSelection(
                    GetMessageRaw("SortBy"),
                    "Id",
                    listSettings
                ));

            ParamsDefinition.Add(
                "SortOrder",
                new BXParamSort(
                    GetMessageRaw("SortOrder"),
                    true,
                    listSettings
                ));
            #endregion

            #region WebForm
            ParamsDefinition.Add(
                "EditFields",
                new BXParamDoubleList(
                    GetMessageRaw("EditFields"),
                    "Name",
                    webFormSettings
                ));

            ParamsDefinition.Add(
                "RequiredFields",
                new BXParamMultiSelection(
                    GetMessageRaw("RequiredFields"),
                    string.Empty,
                    webFormSettings
                ));

            ParamsDefinition.Add(
                "TextBoxSize",
                new BXParamText(
                    GetMessageRaw("TextBoxSize"),
                    "30",
                    webFormSettings
                ));

            ParamsDefinition.Add(
                "CreateButtonTitle",
                    new BXParamText(
                    GetMessageRaw("CreateButtonTitle"),
                    GetMessageRaw("TitleAddElement"),
                    webFormSettings
                ));

            ParamsDefinition.Add(
                "UpdateButtonTitle",
                    new BXParamText(
                    GetMessageRaw("UpdateButtonTitle"),
                    GetMessageRaw("TitleSaveElement"),
                    webFormSettings
                ));

			//ParamsDefinition.Add(
			//    "SuccessMessageAfterCreateElement",
			//    new BXParamText(
			//        GetMessageRaw("SuccessMessageAfterCreateElement"),
			//        GetMessageRaw("MessageElementHasBeenCreatedSuccessfully"),
			//        webFormSettings
			//    ));

			//ParamsDefinition.Add(
			//    "SuccessMessageAfterUpdateElement",
			//    new BXParamText(
			//        GetMessageRaw("SuccessMessageAfterUpdateElement"),
			//        GetMessageRaw("MessageElementHasBeenUpdatedSuccessfully"),
			//        webFormSettings
			//    ));

            //ParamsDefinition.Add(
            //    "RedirectPageUrl",
            //    new BXParamText(
            //        GetMessageRaw("RedirectPageUrl"),
            //        string.Empty,
            //        webFormSettings
            //    ));
            #endregion

            #region Field
            ParamsDefinition.Add(
                "MaxSectionSelect",
                new BXParamText(
                    GetMessageRaw("MaxSectionSelect"),
                    "3",
                    fieldSettingsCategory
                ));

            ParamsDefinition.Add(
                "OnlyLeafSelect",
                new BXParamYesNo(
                    GetMessageRaw("OnlyLeafSelect"),
                    false,
                    fieldSettingsCategory
                ));

            ParamsDefinition.Add(
                "MaxFileSizeUpload",
                new BXParamText(
                    GetMessageRaw("MaxFileSizeUpload"),
                    "1024",
                    fieldSettingsCategory
                ));

            ParamsDefinition.Add(
                "ActiveFromDateShowTime",
                new BXParamYesNo(
                    GetMessageRaw("ActiveFromDateShowTime"),
                    false,
                    fieldSettingsCategory
                ));

            ParamsDefinition.Add(
                "ActiveToDateShowTime",
                new BXParamYesNo(
                    GetMessageRaw("ActiveToDateShowTime"),
                    false,
                    fieldSettingsCategory
                ));

            ParamsDefinition.Add(
                "NameFieldMacros",
                    new BXParamText(
                    GetMessageRaw("NameFieldMacros"),
                    "#DetailText#-#DateCreate#-#CreatedBy#",
                    fieldSettingsCategory
                ));
            #endregion

            #region CustomName
            ParamsDefinition.Add(
                "ActiveCustomTitle",
                new BXParamText(
                    GetMessageRaw("ActiveCustomTitle"),
                    String.Empty,
                    customNameCategory
                )
            );

            ParamsDefinition.Add(
                "NameCustomTitle",
                new BXParamText(
                    GetMessageRaw("NameCustomTitle"),
                    String.Empty,
                    customNameCategory
                ));

            ParamsDefinition.Add(
                "ActiveFromDateCustomTitle",
                new BXParamText(
                    GetMessageRaw("ActiveFromDateCustomTitle"),
                    String.Empty,
                    customNameCategory
                ));

            ParamsDefinition.Add(
                "ActiveToDateCustomTitle",
                new BXParamText(
                    GetMessageRaw("ActiveToDateCustomTitle"),
                    String.Empty,
                    customNameCategory
                ));

            ParamsDefinition.Add(
                "SectionsCustomTitle",
                new BXParamText(
                    GetMessageRaw("SectionsCustomTitle"),
                    String.Empty,
                    customNameCategory
                ));

            ParamsDefinition.Add(
                "PreviewTextCustomTitle",
                new BXParamText(
                    GetMessageRaw("PreviewTextCustomTitle"),
                    String.Empty,
                    customNameCategory
                ));

            ParamsDefinition.Add(
                "PreviewImageCustomTitle",
                new BXParamText(
                    GetMessageRaw("PreviewPictureCustomTitle"),
                    String.Empty,
                    customNameCategory
                ));

            ParamsDefinition.Add(
                "DetailTextCustomTitle",
                new BXParamText(
                    GetMessageRaw("DetailTextCustomTitle"),
                    String.Empty,
                    customNameCategory
                ));


            ParamsDefinition.Add(
                "DetailImageCustomTitle",
                new BXParamText(
                    GetMessageRaw("DetailPictureCustomTitle"),
                    String.Empty,
                    customNameCategory
                ));


            ParamsDefinition.Add(
                "CaptchaCustomTitle",
                new BXParamText(
                    GetMessageRaw("CaptchaCustomTitle"),
                    String.Empty,
                    customNameCategory
                ));
            #endregion

            #region Sef
            ParamsDefinition.Add(
                "RequestParamNameCreateElement",
                new BXParamText(
                    GetMessageRaw("RequestParamNameCreateElement"),
                    "new",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(ClientID, "RequestParamNameCreateElement", new string[] { "NonSef" })
                ));

            ParamsDefinition.Add(
                "RequestParamNameModifyElement",
                new BXParamText(
                    GetMessageRaw("RequestParamNameModifyElement"),
                    "id",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(ClientID, "RequestParamNameModifyElement", new string[] { "NonSef" })
                ));

            ParamsDefinition.Add(
                "RequestParamNamePageIndex",
                new BXParamText(
                    GetMessageRaw("RequestParamNamePageIndex"),
                    "page",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(ClientID, "RequestParamNamePageIndex", new string[] { "NonSef" })
                ));

            ParamsDefinition.Add(
                "RequestParamNameShowAll",
                new BXParamText(
                    GetMessageRaw("RequestParamNameShowAll"),
                    "all",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(ClientID, "RequestParamNameShowAll", new string[] { "NonSef" })
                ));


            ParamsDefinition.Add(
                "SefTemplateCreateElement",
                new BXParamText(
                    GetMessageRaw("SefPathCreateElement"),
                    "/new/",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(ClientID, "SefTemplateCreateElement", new string[] { "Sef" })
                ));

            ParamsDefinition.Add(
                "SefTemplateModifyElement",
                new BXParamText(
                    GetMessageRaw("SefPathModifyElement"),
                    "/edit-#ElementId#/",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(ClientID, "SefTemplateModifyElement", new string[] { "Sef" })
                ));

            ParamsDefinition.Add(
                "SefTemplatePageIndex",
                new BXParamText(
                    GetMessageRaw("SefPathPageIndex"),
                    "/page-#PageId#",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(ClientID, "SefTemplatePageIndex", new string[] { "Sef" })
                ));

            ParamsDefinition.Add(
                "SefTemplateShowAll",
                new BXParamText(
                    GetMessageRaw("SefPathShowAll"),
                    "/all",
                    BXCategory.Sef,
                    new ParamClientSideActionGroupViewMember(ClientID, "SefTemplateShowAll", new string[] { "Sef" })
                ));
            #endregion
        }

        protected override void LoadComponentDefinition()
        {
            #region Properties
            List<BXParamValue> properties = new List<BXParamValue>(),
                                propertiesForUserReference = new List<BXParamValue>();
            int selectedIblockId = 0;
            if (Parameters.ContainsKey("IBlockId"))
                int.TryParse(Parameters["IBlockId"], out selectedIblockId);

            if (selectedIblockId > 0)
            {
                BXCustomFieldCollection customFields = BXIBlock.GetCustomFields(selectedIblockId);
                foreach (BXCustomField customField in customFields)
                {
                    string title = BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel);
                    string code = customField.Name.ToUpper();
                    properties.Add(new BXParamValue(title, code));
                    if (string.Equals(customField.CustomTypeId, "Bitrix.System.Int", StringComparison.Ordinal))
                        propertiesForUserReference.Add(new BXParamValue(title, code));
                }
            }
            #endregion

            #region Iblock type
            List<BXParamValue> typeParamValue = new List<BXParamValue>();
            typeParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockType"), string.Empty));

            BXIBlockTypeCollection iblockTypes = BXIBlockType.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
            foreach (BXIBlockType iblockType in iblockTypes)
                typeParamValue.Add(new BXParamValue(iblockType.Translations[BXLoc.CurrentLocale].Name, iblockType.Id.ToString()));

            ParamsDefinition["IBlockTypeId"].Values = typeParamValue;
            ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;
            #endregion

            #region Iblock
            int selectedIBlockType = 0;
            if (Parameters.ContainsKey("IBlockTypeId"))
                int.TryParse(Parameters["IBlockTypeId"], out selectedIBlockType);

            BXFilter filter = new BXFilter();
            if (selectedIBlockType > 0)
                filter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, selectedIBlockType));
            if (!string.IsNullOrEmpty(DesignerSite))
                filter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

            List<BXParamValue> iblockParamValue = new List<BXParamValue>();
            iblockParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockID"), string.Empty));
            BXIBlockCollection iblocks = BXIBlock.GetList(filter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
            foreach (BXIBlock iblock in iblocks)
                iblockParamValue.Add(new BXParamValue(iblock.Name, iblock.Id.ToString()));

            ParamsDefinition["IBlockId"].Values = iblockParamValue;
            ParamsDefinition["IBlockId"].RefreshOnDirty = true;
            #endregion

            #region Sorting
            List<BXParamValue> sortingFields = new List<BXParamValue>();
            sortingFields.Add(new BXParamValue(GetMessageRaw("ElementID"), "ID"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("ElementName"), "Name"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("Active"), "Active"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("ActiveFromDate"), "ActiveFromDate"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("ActiveUntilDate"), "ActiveToDate"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("SortIndex"), "Sort"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("DateOfModification"), "UpdateDate"));
            sortingFields.AddRange(properties);

            ParamsDefinition["SortBy"].Values = sortingFields;
            #endregion

            #region Access
            //RolesAuthorizedToManage && RolesAuthorizedToAdminister && RolesAuthorizedToView && RolesAuthorizedToManageOfActivation
            IList<BXParamValue> rolesAuthorizedToManage = ParamsDefinition["RolesAuthorizedToManage"].Values;
            if (rolesAuthorizedToManage.Count > 0)
                rolesAuthorizedToManage.Clear();

            IList<BXParamValue> rolesAuthorizedToAdminister = ParamsDefinition["RolesAuthorizedToAdminister"].Values;
            if (rolesAuthorizedToAdminister.Count > 0)
                rolesAuthorizedToAdminister.Clear();

            IList<BXParamValue> rolesAuthorizedToView = ParamsDefinition["RolesAuthorizedToView"].Values;
            if (rolesAuthorizedToView.Count > 0)
                rolesAuthorizedToView.Clear();

            IList<BXParamValue> authRolesAuthorizedToManageOfActivationValues = ParamsDefinition["RolesAuthorizedToManageOfActivation"].Values;
            if (authRolesAuthorizedToManageOfActivationValues.Count > 0)
                authRolesAuthorizedToManageOfActivationValues.Clear();

            BXRoleCollection roles = BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc"));
            for (int i = 0; i < roles.Count; i++)
            {
                BXRole role = roles[i];
                BXParamValue v = new BXParamValue(role.Title, role.RoleName);
                rolesAuthorizedToManage.Add(v);
                rolesAuthorizedToAdminister.Add(v);
                rolesAuthorizedToView.Add(v);
                authRolesAuthorizedToManageOfActivationValues.Add(v);
            }
            //---
            //MannerOfIssueModificationPermission
            IList<BXParamValue> manOfIssueModificationPermissionValues = ParamsDefinition["MannerOfIssueModificationPermission"].Values;
            if (manOfIssueModificationPermissionValues.Count > 0)
                manOfIssueModificationPermissionValues.Clear();

            foreach (string n in Enum.GetNames(typeof(MannerOfIssueModificationPermission)))
                manOfIssueModificationPermissionValues.Add(new BXParamValue(GetMessageRaw(string.Concat("MannerOfIssueModificationPermission", n)), n));
            //---
            //MannerOfUserAssociation
            IList<BXParamValue> mannerOfUserAssociationValues = ParamsDefinition["MannerOfUserAssociation"].Values;
            if (mannerOfUserAssociationValues.Count > 0)
                mannerOfUserAssociationValues.Clear();

            foreach (string n in Enum.GetNames(typeof(MannerOfUserAssociation)))
                mannerOfUserAssociationValues.Add(new BXParamValue(GetMessageRaw(string.Concat("MannerOfUserAssociation", n)), n));
            //---
            //UserAssociatedByCustomIBlockProperty
            IList<BXParamValue> userAssociatedByCustomIBlockPropertyValues = ParamsDefinition["UserAssociatedByCustomIBlockProperty"].Values;
            if (userAssociatedByCustomIBlockPropertyValues.Count > 0)
                userAssociatedByCustomIBlockPropertyValues.Clear();

            userAssociatedByCustomIBlockPropertyValues.Add(new BXParamValue(GetMessageRaw("NotSelected"), string.Empty));
            for (int n = 0; n < propertiesForUserReference.Count; n++)
                userAssociatedByCustomIBlockPropertyValues.Add(propertiesForUserReference[n]);
            //---
            //MannerOfUserIdentification
            IList<BXParamValue> mannerOfUserIdentificationValues = ParamsDefinition["MannerOfUserIdentification"].Values;
            if (mannerOfUserIdentificationValues.Count > 0)
                mannerOfUserIdentificationValues.Clear();

            foreach (string n in Enum.GetNames(typeof(MannerOfUserIdentification)))
                mannerOfUserIdentificationValues.Add(new BXParamValue(GetMessageRaw(string.Concat("MannerOfUserIdentification", n)), n));
            //---
            IList<BXParamValue> elementStatusValues = ParamsDefinition["ElementActiveAfterSave"].Values;
            if (elementStatusValues.Count > 0)
                elementStatusValues.Clear();
            foreach (string n in Enum.GetNames(typeof(ElementStatus)))
                elementStatusValues.Add(new BXParamValue(GetMessageRaw(string.Concat("ElementStatus", n)), n));
            #endregion

            #region WebForm
            List<BXParamValue> fields = new List<BXParamValue>();
            fields.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionActive"), "Active"));
            fields.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionName"), "Name"));
            fields.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionActiveFromDate"), "ActiveFromDate"));
            fields.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionActiveToDate"), "ActiveToDate"));
            fields.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionSections"), "Sections"));
            fields.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionPreviewText"), "PreviewText"));
            fields.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionPreviewPicture"), "PreviewImage"));
            fields.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionDetailText"), "DetailText"));
            fields.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionDetailPicture"), "DetailImage"));

            IList<BXParamValue> requiredFieldsValues = ParamsDefinition["RequiredFields"].Values;
            if (requiredFieldsValues.Count > 0)
                requiredFieldsValues.Clear();

            IList<BXParamValue> editableFieldsValues = ParamsDefinition["EditFields"].Values;
            if (editableFieldsValues.Count > 0)
                editableFieldsValues.Clear();

            int fieldCount = fields.Count;
            for (int n = 0; n < fieldCount; n++)
            {
                requiredFieldsValues.Add(fields[n]);
                editableFieldsValues.Add(fields[n]);
            }


            editableFieldsValues.Add(new BXParamValue(GetMessageRaw("ElementFieldCaptionCaptcha"), "Captcha"));
            if (selectedIblockId > 0)
            {
                BXCustomFieldCollection customFields = BXIBlock.GetCustomFields(selectedIblockId);
                foreach (BXCustomField customField in customFields)
                {
                    string title = BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel);
                    string code = customField.Name.ToUpper();
                    editableFieldsValues.Add(new BXParamValue(title, string.Concat("PROPERTY_", code)));
                }
            }
            #endregion

        }
        #endregion

        /// <summary>
        /// Управление
        /// </summary>
        internal abstract class Behaviour
        {
            public Behaviour(IBlockMyElementsComponent parent)
            {
                if (parent == null)
                    throw new ArgumentNullException("parent");
                _parent = parent;
            }

            private IBlockMyElementsComponent _parent = null;
            protected IBlockMyElementsComponent Parent
            {
                get { return _parent; }
            }

            public int CurrentUserId
            {
                get { return _parent.CurrentUserId; }
            }

            public BXPrincipal CurrentPricipal
            {
                get { return _parent.CurrentPricipal; }
            }

            private bool? _isCurrentUserAuthenticated = null;
            public bool IsCurrentUserAuthenticated()
            {
                return (_isCurrentUserAuthenticated ?? (_isCurrentUserAuthenticated = CurrentPricipal != null ? CurrentPricipal.Identity.IsAuthenticated : false)).Value;
            }

            public int CustomUserId
            {
                get { return _parent.CustomUserId; }
            }

            public static bool IsPrincipalInRoles(BXPrincipal pricipal, IList<string> roles)
            {
                if (roles != null && roles.Count > 0 && pricipal != null)
                    foreach (string r in pricipal.GetAllRoles())
                        if (roles.Contains(r))
                            return true;
                return false;
            }

            /// <summary>
            /// Разрешение пользоваться данной веб-формой
            /// </summary>
            public abstract bool IsCurrentUserPermitted { get; }
            /// <summary>
            /// Ид представляемого пользователя
            /// </summary>
            public abstract int ImpersonatedUserId { get; }
            /// <summary>
            /// Запрос разрешения на создание
            /// </summary>
            /// <returns></returns>
            public abstract bool IsElementCreationAllowed();
            /// <summary>
            /// Запрос разрешения на редактирование
            /// </summary>
            /// <returns></returns>
            public abstract bool IsElementModificationAllowed();
            /// <summary>
            /// Запрос разрешения на удаление
            /// </summary>
            /// <returns></returns>
            public abstract bool IsElementDeletionAllowed();
        }
        /// <summary>
        /// Управление в режиме "Стандартный" (редактирование своих записей)
        /// </summary>
        internal sealed class StandardBehaviour : Behaviour
        {
            public StandardBehaviour(IBlockMyElementsComponent parent)
                : base(parent)
            {
            }

            private bool? _isCurrentUserPermitted = null;
            public override bool IsCurrentUserPermitted
            {
                get
                {
                    if (_isCurrentUserPermitted.HasValue)
                        return _isCurrentUserPermitted.Value;

                    _isCurrentUserPermitted = false;
                    //неаутентифицированные могут быть допущены к управлению "своими" эл-тами
                    if (CurrentPricipal != null)
                    {
                        IList<string> permittedRoles = Parent.RolesAuthorizedToManage;
                        if (permittedRoles.Count == 0)
                            _isCurrentUserPermitted = Parent.IBlock != null ? Parent.IBlock.IsUserCanOperate(BXIBlock.Operations.IBlockModifyElements) : false;
                        else
                            _isCurrentUserPermitted = IsPrincipalInRoles(CurrentPricipal, permittedRoles);
                    }
                    return _isCurrentUserPermitted.Value;
                }
            }

            public override int ImpersonatedUserId
            {
                get { return CurrentUserId; }
            }

            public override bool IsElementCreationAllowed() 
            {
                return Parent.PermitOfCreation;
            }

            public override bool IsElementModificationAllowed() 
            {
                return Parent.PermitOfModification;
            }

            public override bool IsElementDeletionAllowed() 
            {
                return Parent.PermitOfDeletion;
            }
        }

        /// <summary>
        /// Управление в режиме "Просмотр" (просмотр чужих записей)
        /// </summary>
        internal sealed class ViewBehaviour : Behaviour
        {
            public ViewBehaviour(IBlockMyElementsComponent parent)
                : base(parent)
            {
            }

            private bool? _isCurrentUserPermitted = null;
            public override bool IsCurrentUserPermitted
            {
                get
                {
                    //неаутентифицированные могут проматривать чужие эл-ты
                    return (_isCurrentUserPermitted ?? (_isCurrentUserPermitted = IsPrincipalInRoles(CurrentPricipal, Parent.RolesAuthorizedToView))).Value;
                }
            }

            public override int ImpersonatedUserId { get { return CustomUserId; } }
            public override bool IsElementCreationAllowed() { return false; }
            public override bool IsElementModificationAllowed() { return false; }
            public override bool IsElementDeletionAllowed() { return false; }
        }

        /// <summary>
        /// Управление в режиме "Администрирование" (редактирование чужих записей)
        /// </summary>
        internal sealed class AdministerBehaviour : Behaviour
        {
            public AdministerBehaviour(IBlockMyElementsComponent parent)
                : base(parent)
            {
            }

            private bool? _isCurrentUserPermitted = null;
            public override bool IsCurrentUserPermitted
            {
                get
                {
                    //неаутентифицированные не могут быть допущены к управлению чужими эл-тами
                    return (_isCurrentUserPermitted ?? (_isCurrentUserPermitted = CurrentPricipal != null && IsCurrentUserAuthenticated() && IsPrincipalInRoles(CurrentPricipal, Parent.RolesAuthorizedToAdminister))).Value;
                }
            }
            public override int ImpersonatedUserId { get { return CustomUserId; } }
            public override bool IsElementCreationAllowed() { return true; }
            public override bool IsElementModificationAllowed() { return true; }
            public override bool IsElementDeletionAllowed() { return true; }
        }
        /// <summary>
        /// Управление в режиме "Доступ запрещен"
        /// </summary>
        private sealed class AccessDeniedBehaviour : Behaviour
        {
            public AccessDeniedBehaviour(IBlockMyElementsComponent parent) : base(parent) { }
            public override bool IsCurrentUserPermitted { get { return false; } }
            public override int ImpersonatedUserId { get { return -1; } }
            public override bool IsElementCreationAllowed() { return false; }
            public override bool IsElementModificationAllowed() { return false; }
            public override bool IsElementDeletionAllowed() { return false; }
        }
    }

}
