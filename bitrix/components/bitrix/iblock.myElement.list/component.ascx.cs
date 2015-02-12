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
using Bitrix.IBlock;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Security;
using Bitrix.Components.Editor;
using Bitrix.Services;
using Bitrix.DataTypes;
using Bitrix.Configuration;
using Bitrix.IO;
using Bitrix.UI.Hermitage;

namespace Bitrix.IBlock.Components
{
    public partial class IBlockMyElementListComponent : Bitrix.UI.BXComponent
    {
        #region Enum
        public enum MannerOfUserAssociation
        {
            NoBody = 1,
            CreatedBy,
            IBlockProperty
        }

        public enum MannerOfIssueModificationPermission
        {
            Active = 1,
            NotActive,
            Always
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

       [FlagsAttribute]
        public enum Error
        {
            /// <summary>
            /// Без ошибок
            /// </summary>
            ErrNone = 0,  
            /// <summary>
            /// Не найден инфоблок
            /// </summary>
            ErrIBlockIsNotFound = 1
            /// <summary>
            /// Не найдено сво-во инфоблока
            /// </summary>
            //ErrIBlockPropertyIsNotFound
        }

        #endregion

        public class Item
        {
            private IBlockMyElementListComponent _component = null;
            public Item(BXIBlockElement element, IBlockMyElementListComponent component) 
            {
                if (element == null)
                    throw new ArgumentNullException("element");
                _element = element;
                if (component == null)
                    throw new ArgumentNullException("component");
                _component = component;
            }
            private BXIBlockElement _element = null;
            public BXIBlockElement Element
            {
                get { return _element; }
            }
            public bool GetElementActive()
            {
                return _element.Active;
            }
            public int GetElementId() 
            {
                return _element.Id;
            }
            public string GetElementName()
            {
                return _element.Name;
            }
            public string GetElementPreviewText()
            {
                return _element.PreviewText;
            }
            public string GetElementDetailText()
            {
                return _element.DetailText;
            }
            public DateTime GetElementCreationDateTime()
            {
                return _element.CreateDate;
            }

            private bool? _isElementModificationAllowed = null;
            /// <summary>
            /// Запрос разрешения на редактирование эл-та
            /// </summary>
            /// <returns></returns>
            public bool IsElementModificationAllowed()
            {
                return (_isElementModificationAllowed ?? (_isElementModificationAllowed = _component.InternalBehaviour.IsElementModificationAllowed(this))).Value;
            }

            private bool? _isElementDeletionAllowed = null;
            /// <summary>
            /// Запрос разрешения на удаление эл-та
            /// </summary>
            /// <returns></returns>
            public bool IsElementDeletionAllowed()
            {
                return (_isElementDeletionAllowed ?? (_isElementDeletionAllowed = _component.InternalBehaviour.IsElementDeletionAllowed())).Value;
            }
        }

        #region Events

        protected override void OnInit(EventArgs e)
        {
            CacheMode = BXCacheMode.None;
            Page.LoadComplete += new EventHandler(Page_LoadComplete);
            base.OnInit(e);
        }

        void Page_LoadComplete(object sender, EventArgs e)
        {
            if (CurrentPricipal == null)
                return;

            BXPagingParams pagingParams = PreparePagingParams();
            //if (IsCached(((BXPrincipal)Page.User).GetAllRoles(true), pagingParams))
            //{
            //    SetTemplateCachedData();
            //    return;
            //}

            EnsureIBlockDataLoaded();
            if ((ComponentErrors & Error.ErrIBlockIsNotFound) == Error.ErrIBlockIsNotFound || IsPermissionDenied)
                return;

            BXFilter elementFilter = new BXFilter(
                //new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId)
                //new BXFilterItem(BXIBlockElement.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y")
            );

            switch (UserAssociation)
            {
                case MannerOfUserAssociation.CreatedBy:
                    elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.CreatedBy, BXSqlFilterOperators.Equal, InternalBehaviour.ImpersonatedUserId));
                    break;
                case MannerOfUserAssociation.IBlockProperty:
                    {
                        bool isHandled = false;
                        string userAssociatedByCustomIBlockProperty = Parameters.Get<string>("UserAssociatedByCustomIBlockProperty", string.Empty);
                        if (!string.IsNullOrEmpty(userAssociatedByCustomIBlockProperty))
                        {
                            BXCustomFieldCollection customFields = IBlock != null ? IBlock.CustomFields : null;
                            BXCustomField field = null;
                            try
                            {
                                field = customFields[userAssociatedByCustomIBlockProperty];
                            }
                            catch (KeyNotFoundException /*exc*/) { }
                            if (field != null)
                            {
                                elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.GetCustomField(IBlockId, field.Name), BXSqlFilterOperators.Equal, InternalBehaviour.ImpersonatedUserId));
                                isHandled = true;
                            }
                        }

                        if (!isHandled)
                            elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.CreatedBy, BXSqlFilterOperators.Equal, InternalBehaviour.ImpersonatedUserId));
                    }
                    break;
                case MannerOfUserAssociation.NoBody:
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Value '{0}' is not supported in current context!", Enum.GetName(typeof(MannerOfUserAssociation), UserAssociation)));
            }


            Dictionary<string, BXCustomField> iblockCustomFields = new Dictionary<string, BXCustomField>(IBlock.CustomFields.Count);
            foreach (BXCustomField field in IBlock.CustomFields)
                iblockCustomFields[field.CorrectedName] = field;

            //Sorting
            BXOrderBy elementOrderBy = new BXOrderBy();
            string sortBy = Parameters.Get<string>("SortBy", "Id");
            string sortOrder = Parameters.Get<string>("SortOrder", "Desc");

            if (sortBy.StartsWith("-", StringComparison.OrdinalIgnoreCase))
            {
                sortBy = sortBy.Substring(1).ToUpper();
                if (iblockCustomFields.ContainsKey(sortBy))
                    elementOrderBy.Add(BXIBlockElement.Fields.GetCustomField(IBlockId, sortBy), sortOrder);
            }
            else
                elementOrderBy.Add(BXIBlockElement.Fields, sortBy, sortOrder);

            //Paging
            bool isLegalPage;
            BXQueryParams queryParams = PreparePaging(
                pagingParams,
                delegate { return BXIBlockElement.Count(elementFilter); },
                new BXParamsBag<object>(),
                out isLegalPage
            );

            if (!Parameters.Get<bool>("PagingAllow"))
                queryParams = new BXQueryParams(new BXPagingOptions(0, Parameters.Get<int>("PagingRecordsPerPage", 10)));
            //else if (!isLegalPage)
            //    AbortCache();

            BXIBlockElementCollection col = BXIBlockElement.GetList(elementFilter, elementOrderBy, null, queryParams, BXTextEncoder.HtmlTextEncoder);
            int colLength = col != null ? col.Count : 0;
            _items = new List<Item>();
            for (int i = 0; i < colLength; i++)
                _items.Add(new Item(col[i], this));
        }

        protected override void OnLoad(EventArgs e)
        {
            IncludeComponentTemplate();
            base.OnLoad(e);
        }
        #endregion

        #region Service

        private BXPrincipal _currentPricipal = null;
        public BXPrincipal CurrentPricipal
        {
            get { return _currentPricipal ?? (_currentPricipal = Context.User as BXPrincipal); }
        }

        private string[] _currentPricipalRoles = null;
        public string[] CurrentPricipalRoles
        {
            get { return _currentPricipalRoles ?? (_currentPricipalRoles = CurrentPricipal != null ? CurrentPricipal.GetAllRoles() : new string[0]); }
        }

        private int? _currentUserId = null;
        public int CurrentUserId
        {
            get { return _currentUserId ?? (_currentUserId = CurrentPricipal != null && CurrentPricipal.Identity.IsAuthenticated ? ((BXIdentity)CurrentPricipal.Identity).Id : 0).Value; }
        }

        public bool IsUserAutorizedToManageMyElementList() 
        {
            return InternalBehaviour.IsCurrentUserPermitted;
        }

        private bool? isPermissionDenied = null;
        public bool IsPermissionDenied
        {
            get 
            {
                return (isPermissionDenied ?? (isPermissionDenied = !InternalBehaviour.IsCurrentUserPermitted)).Value;
            }
        }

        private bool _isIBlockDataLoaded = false;
        private void EnsureIBlockDataLoaded() 
        {
            if (_isIBlockDataLoaded)
                return;
            BXIBlockCollection iblockCollection = BXIBlock.GetList(
                new BXFilter(
                    new BXFilterItem(BXIBlock.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                //new BXFilterItem(BXIBlock.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y"),
                    new BXFilterItem(BXIBlock.Fields.ID, BXSqlFilterOperators.Equal, IBlockId)
                ),
                null,
                null,
                null,
                BXTextEncoder.HtmlTextEncoder
            );

            if (iblockCollection == null || iblockCollection.Count < 1)
                GenerateNotFoundError(Error.ErrIBlockIsNotFound);
            else
            {
                IBlock = iblockCollection[0];
                IBlockTypeId = IBlock.TypeId;
                IBlockName = IBlock.Name;
            }

            _isIBlockDataLoaded = true;
        }

        //private void SetTemplateCachedData(){}
        private string PrepareUrl(string url) 
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (url.StartsWith("http"))
                return url;

            int whatInd = url.IndexOf('?');
            string query = whatInd >= 0 ? url.Substring(whatInd) : string.Empty;
            string path = whatInd >= 0 ? url.Substring(0, whatInd) : url;
            if (string.IsNullOrEmpty(path))
                path = Request.AppRelativeCurrentExecutionFilePath;
            else if (!VirtualPathUtility.IsAbsolute(path))
                path = VirtualPathUtility.Combine(Request.AppRelativeCurrentExecutionFilePath, path);

            return string.Concat(path, query);
        }

        public string GetUrlForElementCreation() 
        {
            string urlPattern = ElementCreationUrl;
            BXParamsBag<object> replaceItems = new BXParamsBag<object>();
            replaceItems.Add("IblockId", IBlockId);
            replaceItems.Add("IBLOCK_ID", IBlockId);

            return PrepareUrl(MakeLink(urlPattern, replaceItems));
        }

        public string GetUrlForModification(int elementID)
        {
            //string urlPattern = EnableSef ? ElementModificationUrlSef : ElementModificationUrl;
            if (!IsElementModificationAllowed(elementID))
                return string.Empty;
            string urlPattern = ElementModificationUrl;
            if (string.IsNullOrEmpty(urlPattern))
                return string.Empty;
            BXParamsBag<object> replaceItems = new BXParamsBag<object>();
            replaceItems.Add("IblockId", IBlockId);
            replaceItems.Add("IBLOCK_ID", IBlockId);
            replaceItems.Add("ELEMENT_ID", elementID);
            replaceItems.Add("ElementId", elementID);

            return PrepareUrl(MakeLink(urlPattern, replaceItems));
        }

        /// <summary>
        /// Запрос разрешения на редактирование эл-та
        /// </summary>
        /// <param name="elementID"></param>
        /// <returns></returns>
        public bool IsElementModificationAllowed(int elementID) 
        {
            MannerOfIssueModificationPermission mp = IssueModificationPermission;
            if (mp == MannerOfIssueModificationPermission.Always)
                return true;

            Item item = GetItemById(elementID);
            if(item == null)
                throw new InvalidOperationException("Item is not found in list!");
            return item.IsElementModificationAllowed();
        }

        /// <summary>
        /// Запрос разрешения на удаление эл-та
        /// </summary>
        /// <param name="elementID"></param>
        /// <returns></returns>
        public bool IsElementDeletionAllowed(int elementID) 
        {
            return InternalBehaviour.IsElementDeletionAllowed();
        }

        public void DeleteElement(int elementID) 
        {
            if (!InternalBehaviour.IsCurrentUserPermitted)
                throw new InvalidOperationException("User is not authorized!");
            //---
            if (!InternalBehaviour.IsElementDeletionAllowed())
            {
                Response.Redirect(Request.RawUrl);
                return;
            }

            BXIBlockElement element = BXIBlockElement.GetById(elementID);
            //BXIBlockElement element = GetItemById(elementID);
            if (element == null)
            {
                Response.Redirect(Request.RawUrl);
                return;
            }

            bool aboutOwnerShip = false;
            if (InternalBehaviour.ImpersonatedUserId > 0)
                switch (UserAssociation)
                {
                    case MannerOfUserAssociation.CreatedBy:
                        aboutOwnerShip = element.CreatedByUser.UserId == InternalBehaviour.ImpersonatedUserId;
                        break;
                    case MannerOfUserAssociation.IBlockProperty:
                        {
                            BXCustomField field;
                            BXCustomProperty prop;
                            string userAssociatedByCustomIBlockProperty = Parameters.Get<string>("UserAssociatedByCustomIBlockProperty", string.Empty);
                            if (!string.IsNullOrEmpty(userAssociatedByCustomIBlockProperty) &&  
                                element.CustomFields.TryGetValue("userAssociatedByCustomIBlockProperty", out field) &&
                                element.CustomValues.TryGetValue("userAssociatedByCustomIBlockProperty", out prop)
                                )
                            {
                                try
                                {
                                    aboutOwnerShip = string.Equals(field.CustomTypeId, "Bitrix.System.Int", StringComparison.Ordinal) && Convert.ToInt32(prop.Value) == InternalBehaviour.ImpersonatedUserId;
                                }
                                catch(InvalidCastException /*exc*/){}
                            }
                        }
                        break;
                    case MannerOfUserAssociation.NoBody:
                        aboutOwnerShip = true;
                        break;
                }

            if (aboutOwnerShip)
            {
                //BXIBlockElement.Delete(new BXFilter(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementID)));
                BXIBlockElement el = BXIBlockElement.GetById(elementID);
                if (el != null)
                    el.Delete();
            }
            Response.Redirect(Request.RawUrl);
        }

        private Error _err = Error.ErrNone;
        /// <summary>
        /// Ошибка
        /// </summary>
        public Error ComponentErrors
        {
            get { return _err; }
        }

        private void GenerateNotFoundError(Error error)
        {
            _err |= error;

            AbortCache();

            ScriptManager ajax = Page != null ? ScriptManager.GetCurrent(Page) : null;
            if (!IsComponentDesignMode && BXConfigurationUtility.ShowMode == BXShowMode.View && (ajax == null || !ajax.IsInAsyncPostBack))
                BXError404Manager.Set404Status(Response);
        }

        private Item GetItemById(int elementID) 
        {
            if (_items == null || _items.Count == 0)
                return null;
            int itemInd = _items.FindIndex(
                    delegate(Item obj) { return obj.GetElementId() == elementID; }
                    );
			return itemInd >= 0 ? _items[itemInd] : null;
        }

        #endregion

        #region Properties
        #region IBlock

        public int IBlockId
        {
            get { return Parameters.Get<int>("IBlockId"); }
        }

        protected BXIBlock _iblock;
        public BXIBlock IBlock
        {
            get { return _iblock; }
            protected set { _iblock = value; }
        }

        public string IBlockName
        {
            get { return ComponentCache.Get<string>("IBlockName", string.Empty); }
            protected set { ComponentCache["IBlockName"] = value; }
        }

        public string IBlockElementName
        {
            get { return ComponentCache.Get<string>("IBlockElementName", String.Empty); }
            protected set { ComponentCache["IBlockElementName"] = value; }
        }

        public string IBlockSectionName
        {
            get { return ComponentCache.Get<string>("IBlockSectionName", String.Empty); }
            protected set { ComponentCache["IBlockSectionName"] = value; }
        }

        public int IBlockTypeId
        {
            get { return ComponentCache.Get<int>("IBlockTypeId"); }
            protected set { ComponentCache["IBlockTypeId"] = value; }
        }

        public MannerOfUserAssociation UserAssociation
        {
            get 
            {
                object obj = null;
                if (ComponentCache.TryGetValue("MannerOfUserAssociation", out obj))
                    return (MannerOfUserAssociation)obj;

                MannerOfUserAssociation r = Parameters.GetEnum<MannerOfUserAssociation>("MannerOfUserAssociation", MannerOfUserAssociation.CreatedBy);
                ComponentCache["MannerOfUserAssociation"] = r;
                return r;
            }
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


        //public bool EnableSef 
        //{
        //    get { return Parameters.Get<bool>("EnableSEF", false); }
        //}

        //public string ElementModificationUrlSef
        //{
        //    get
        //    {
        //        return Parameters.Get<string>("ElementModificationUrlSef", "/myElementList-#ElementId#/");
        //    }
        //}

        public string ElementCreationUrl
        {
            get
            {
                return Parameters.Get<string>("ElementCreationUrl", "./edit.aspx");
            }
        }

        public string ElementModificationUrl
        {
            get
            {
                return Parameters.Get<string>("ElementModificationUrl", "./edit.aspx?elementId=#ElementId#");
            }
        }

        public string ElementCreationUrlTitle
        {
            get 
            {
                return Parameters.Get<string>("ElementCreationUrlTitle", GetMessageRaw("TitleAddElement"));
            }
        }

        #endregion

        #region Items
        protected List<Item> _items = null;
        public IList<Item> Items
        {
            get 
            {
                if (_items != null)
                    return new ReadOnlyCollection<Item>(_items);
                return new List<Item>(); 
            }
        }
        #endregion
        #endregion

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
                    if(Behaviour.IsPrincipalInRoles(CurrentPricipal, RolesAuthorizedToAdminister))
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
        #region Component
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/iblock_my_el_list.gif";
            Group = new BXComponentGroup("iblock.elements", GetMessageRaw("Group"), 100, BXComponentGroup.Content);

            BXCategory dataSourceCategory = BXCategory.DataSource;
            BXCategory mainCategory = BXCategory.Main;
            BXCategory sefCategory = BXCategory.Sef;
            BXCategory accessCategory = new BXCategory(GetMessageRaw("Category.Access"), "ACCESS", 110);
            BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

            ParamsDefinition.Add(BXParametersDefinition.Paging);
            ParamsDefinition.Add(BXParametersDefinition.PagingUrl);

            //BXParamsBag<BXParam> sefParBag = BXParametersDefinition.Sef;
            //sefParBag["EnableSEF"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(ClientID, "EnableSEF", "Sef", "NonSef");
            //sefParBag["SEFFolder"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "SEFFolder", new string[] { "Sef" });
            //ParamsDefinition.Add(sefParBag);

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
                "SortBy",
                new BXParamSingleSelection(
                    GetMessageRaw("SortBy"),
                    "Id",
                    dataSourceCategory
                )
            );

            ParamsDefinition.Add(
                "SortOrder",
                new BXParamSort(
                    GetMessageRaw("SortOrder"),
                    true,
                    dataSourceCategory
                )
            );

            ParamsDefinition.Add(
                "ElementCreationUrl",
                new BXParamText(
                    GetMessageRaw("ElementCreationUrl"),
                    "./edit.aspx",
                    sefCategory
                //new ParamClientSideActionGroupViewMember(ClientID, "ElementCreationUrl", new string[] { "NonSef" })
                ));

            ParamsDefinition.Add(
                "ElementCreationUrlTitle",
                new BXParamText(
                    GetMessageRaw("ElementCreationUrlTitle"),
                    GetMessageRaw("TitleAddElement") /*"<script type='text/javascript'>alert('Hello all!')<script>"*/,
                    additionalSettingsCategory
                ));

            ParamsDefinition.Add(
                "ElementModificationUrl",
                new BXParamText(
                    GetMessageRaw("ElementModificationUrl"),
                    "./edit.aspx?elementId=#ElementId#",
                    sefCategory
                    //new ParamClientSideActionGroupViewMember(ClientID, "ElementModificationUrl", new string[] { "NonSef" })
                ));

            //ParamsDefinition.Add(
            //    "ElementModificationUrlSef",
            //    new BXParamText(
            //        GetMessageRaw("ElementModificationUrlSef"),
            //        "/BlockId-#IblockId#/ElementId-#ElementId#/",
            //        sefCategory,
            //        new ParamClientSideActionGroupViewMember(ClientID, "ElementModificationUrlSef", new string[] { "Sef" })
            //    ));

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

            //право создавать свои записи
            ParamsDefinition.Add(
                "PermitOfCreation",
                new BXParamYesNo(
                    GetMessageRaw("PermitOfCreation"),
                    false,
                    accessCategory
                ));

            //право редактировать свои записи
            ParamsDefinition.Add(
                "PermitOfModification",
                new BXParamYesNo(
                    GetMessageRaw("PermitOfModification"),
                    false,
                    accessCategory,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "PermitOfModification", "PermitOfElementModification", string.Empty)
                ));

            //разрешение на редактирование записи
            ParamsDefinition.Add(
                "MannerOfIssueModificationPermission",
                new BXParamSingleSelection(
                    GetMessageRaw("MannerOfIssueModificationPermission"),
                    MannerOfIssueModificationPermission.Active.ToString("G"),
                    accessCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "MannerOfIssueModificationPermission", new string[] { "PermitOfElementModification" })
                ));

            //право удалять свои записи
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
                    new ParamClientSideActionGroupViewMember(ClientID, "UserAssociatedByCustomIBlockProperty", new string[] { Enum.GetName(typeof(MannerOfUserAssociation), MannerOfUserAssociation.IBlockProperty) })
                ));
        }

        protected override void LoadComponentDefinition()
        {
            #region Properties
            List<BXParamValue>  properties = new List<BXParamValue>(),
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
            typeParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockType"), ""));

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
            //RolesAuthorizedToManage && RolesAuthorizedToAdminister && RolesAuthorizedToView
            IList<BXParamValue> rolesAuthorizedToManage = ParamsDefinition["RolesAuthorizedToManage"].Values;
            if (rolesAuthorizedToManage.Count > 0)
                rolesAuthorizedToManage.Clear();

            IList<BXParamValue> rolesAuthorizedToAdminister = ParamsDefinition["RolesAuthorizedToAdminister"].Values;
            if (rolesAuthorizedToAdminister.Count > 0)
                rolesAuthorizedToAdminister.Clear();

            IList<BXParamValue> rolesAuthorizedToView = ParamsDefinition["RolesAuthorizedToView"].Values;
            if (rolesAuthorizedToView.Count > 0)
                rolesAuthorizedToView.Clear();

            foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
            {
                BXParamValue v = new BXParamValue(r.Title, r.RoleName);
                rolesAuthorizedToManage.Add(v);
                rolesAuthorizedToAdminister.Add(v);
                rolesAuthorizedToView.Add(v);
            }
            //---
            //MannerOfIssueModificationPermission
            IList<BXParamValue> manOfIssueModificationPermissionValues = ParamsDefinition["MannerOfIssueModificationPermission"].Values;
            if (manOfIssueModificationPermissionValues.Count > 0)
                manOfIssueModificationPermissionValues.Clear();

            string[] manOfIssueModificationPermissionNames  = Enum.GetNames(typeof(MannerOfIssueModificationPermission));
            int manOfIssueModificationPermissionNamesCount = manOfIssueModificationPermissionNames != null ? manOfIssueModificationPermissionNames.Length : 0;
            for (int j = 0; j < manOfIssueModificationPermissionNamesCount; j++) 
            {
                string name = manOfIssueModificationPermissionNames[j];
                manOfIssueModificationPermissionValues.Add(new BXParamValue(GetMessageRaw(string.Concat("MannerOfIssueModificationPermission", name)), name));
            }
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

            foreach(string n in Enum.GetNames(typeof(MannerOfUserIdentification)))
                mannerOfUserIdentificationValues.Add(new BXParamValue(GetMessageRaw(string.Concat("MannerOfUserIdentification", n)), n));
            //---
            #endregion
        }

        public BXPopupMenuItem[] GetMenuItems()
        {
            EnsureIBlockDataLoaded();

            BXPopupMenuItem[] menuItems = new BXPopupMenuItem[1];

            menuItems[0] = new BXPopupMenuItem();
            menuItems[0].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/new_element.gif");
            menuItems[0].ID = String.Format("myElementList_new_{0}", IBlockId);
            menuItems[0].Text = String.Format(GetMessageRaw("AddNewElement"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
            menuItems[0].ClientClickScript =
                String.Format(
                    "jsUtils.Redirect(arguments, '{0}?type_id={2}&iblock_id={1}&{3}={4}')",
                    VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
                    IBlockId,
                    IBlockTypeId,
                    BXConfigurationUtility.Constants.BackUrl,
                    UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery)
                );

            return menuItems;
        }

        /// <summary>
        /// Создание контекстного меню
        /// </summary>
        public override BXComponentPopupMenuInfo PopupMenuInfo
        {
            get
            {
				if(this.popupMenuInfo != null)
					return this.popupMenuInfo;

                BXComponentPopupMenuInfo info = base.PopupMenuInfo;

                if (BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) &&
                    BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements) &&
                    ComponentErrors == Error.ErrNone)
                    info.CreateComponentContentMenuItems = delegate(BXShowMode showMode)
                    {
						if(showMode == BXShowMode.View)
							return new BXHermitagePopupMenuBaseItem[0];

						EnsureIBlockDataLoaded();

						BXHermitagePopupMenuBaseItem[] menuItems = new BXHermitagePopupMenuBaseItem[1];

						BXHermitagePopupMenuItem createItem = new BXHermitagePopupMenuItem();
						menuItems[0] = createItem;
						createItem.Id = string.Concat("MY_ELEMENT_CREATE_", IBlockId);
						createItem.IconCssClass = "bx-context-toolbar-create-icon";
						createItem.Text = string.Format(GetMessageRaw("AddNewElement"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
						createItem.Sort = 10;
						createItem.ClientClickScript =
							string.Format(
								"jsUtils.Redirect([], '{0}?type_id={2}&iblock_id={1}&{3}={4}')",
								VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
								IBlockId,
								IBlockTypeId,
								BXConfigurationUtility.Constants.BackUrl,
								UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery));

						return menuItems;
                    };
                return info;
            }
        }
        #endregion

        /// <summary>
        /// Управление
        /// </summary>
        internal abstract class Behaviour
        {
            public Behaviour(IBlockMyElementListComponent parent)
            {
                if (parent == null)
                    throw new ArgumentNullException("parent");
                _parent = parent;
            }

            private IBlockMyElementListComponent _parent = null;
            protected IBlockMyElementListComponent Parent
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
            /// Разрешение пользоваться данным списком
            /// </summary>
            public abstract bool IsCurrentUserPermitted { get; }
            /// <summary>
            /// Ид представляемого пользователя
            /// </summary>
            public abstract int ImpersonatedUserId { get; }
            /// <summary>
            /// Запрос разрешения на редактирование эл-та
            /// </summary>
            /// <returns></returns>
            public abstract bool IsElementModificationAllowed(Item item);

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
        /// Управление в режиме "Стандартный" (управление своими записями)
        /// </summary>
        internal sealed class StandardBehaviour : Behaviour
        {
            public StandardBehaviour(IBlockMyElementListComponent parent)
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

            /// <summary>
            /// Запрос разрешения на редактирование эл-та
            /// </summary>
            /// <returns></returns>
            public override bool IsElementModificationAllowed(Item item) 
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                if (!IsElementModificationAllowed())
                    return false;

                MannerOfIssueModificationPermission mp = Parent.IssueModificationPermission;
                if (mp == MannerOfIssueModificationPermission.Always)
                    return true;
                else if (mp == MannerOfIssueModificationPermission.Active)
                    return item.GetElementActive();
                else if (mp == MannerOfIssueModificationPermission.NotActive)
                    return !item.GetElementActive();
                else
                    return false;
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
            public ViewBehaviour(IBlockMyElementListComponent parent)
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
            public override bool IsElementModificationAllowed(Item item) { return false; }
            public override bool IsElementCreationAllowed() { return false; }
            public override bool IsElementModificationAllowed() { return false; }
            public override bool IsElementDeletionAllowed() { return false; }
        }

        /// <summary>
        /// Управление в режиме "Администрирование" (редактирование чужих записей)
        /// </summary>
        internal sealed class AdministerBehaviour : Behaviour
        {
            public AdministerBehaviour(IBlockMyElementListComponent parent)
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
            public override bool IsElementModificationAllowed(Item item) { return true; /*в режиме администрирования редактирование разрешено всегда*/ }
            public override bool IsElementCreationAllowed() { return true; }
            public override bool IsElementModificationAllowed() { return true; }
            public override bool IsElementDeletionAllowed() { return true; }
        }
        /// <summary>
        /// Управление в режиме "Доступ запрещен"
        /// </summary>
        internal sealed class AccessDeniedBehaviour : Behaviour
        {
            public AccessDeniedBehaviour(IBlockMyElementListComponent parent) : base(parent) { }
            public override bool IsCurrentUserPermitted { get { return false; } }
            public override int ImpersonatedUserId { get { return -1; } }
            public override bool IsElementModificationAllowed(Item item) { return false; }
            public override bool IsElementCreationAllowed() { return false; }
            public override bool IsElementModificationAllowed() { return false; }
            public override bool IsElementDeletionAllowed() { return false; }
        }
    }

    public class IBlockMyElementListTemplate : BXComponentTemplate<IBlockMyElementListComponent>
    {
        protected bool IsElementCreationAllowed() { return Component.InternalBehaviour.IsElementCreationAllowed(); }
        protected bool IsElementModificationAllowed() {  return Component.InternalBehaviour.IsElementModificationAllowed();  }
        protected bool IsElementDeletionAllowed() { return Component.InternalBehaviour.IsElementDeletionAllowed(); }

        protected override void Render(HtmlTextWriter writer)
        {
            StartWidth = "100%";
            if (IsComponentDesignMode)
            {
                if (IsComponentDesignMode && Component.ComponentErrors != IBlockMyElementListComponent.Error.ErrNone)
                    writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
                else if (Component.Items.Count == 0)
                {
                    writer.Write(BXLoc.GetMessage(Component, "NoData"));
                }
                else
                    base.Render(writer);
                return;
            }
            base.Render(writer);
        }
    }
}
