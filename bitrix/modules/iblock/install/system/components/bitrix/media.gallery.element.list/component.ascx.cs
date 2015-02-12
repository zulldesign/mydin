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
using Bitrix.IBlock;
using Bitrix.DataLayer;
using System.Collections.Generic;
using Bitrix.Services;
using Bitrix;
using Bitrix.Configuration;
using Bitrix.IO;
using Bitrix.DataTypes;
using System.Text;
using Bitrix.Services.Text;
using Bitrix.Security;
using System.IO;
using Bitrix.UI.Popup;
using System.Collections.ObjectModel;
using Bitrix.UI.Hermitage;
using Bitrix.UI.Components;
using Bitrix.IBlock.UI;

namespace Bitrix.IBlock.Components
{
    public partial class MediaGalleryElementListComponent : Bitrix.UI.BXComponent
    {
        #region Enums
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
            ErrIBlockIsNotFound = 1,
            /// <summary>
            /// Не найдена секция
            /// </summary>
            ErrIBlockSectionIsNotFound = 2
        }
        #endregion

        #region Iblock properties

        public int IBlockId
        {
            get { return Parameters.Get<int>("IBlockId"); }
        }

        protected BXIBlock iblock;
        public BXIBlock IBlock
        {
            get { return iblock; }
            protected set { iblock = value; }
        }

        public string IBlockName
        {
            get { return ComponentCache.Get<string>("IBlockName", String.Empty); }
            set { ComponentCache["IBlockName"] = value; }
        }

        public string IBlockElementName
        {
            get { return ComponentCache.Get<string>("IBlockElementName", String.Empty); }
            set { ComponentCache["IBlockElementName"] = value; }
        }

        public string IBlockSectionName
        {
            get { return ComponentCache.Get<string>("IBlockSectionName", String.Empty); }
            set { ComponentCache["IBlockSectionName"] = value; }
        }

        public int IBlockTypeId
        {
            get { return ComponentCache.Get<int>("IBlockTypeId"); }
            set { ComponentCache["IBlockTypeId"] = value; }
        }

        #endregion

        #region Player
        /*
        public string PlayerWidth
        {
            get
            {
                return Parameters.Get<string>("PlayerWidth", string.Empty);
            }
        }

        public string PlayerHeight
        {
            get
            {
                return Parameters.Get<string>("PlayerHeight", string.Empty);
            }
        }

        public MediaPlayerStretchingMode PlayerStretching
        {
            get
            {
                string str = Parameters.Get("PlayerStretching", string.Empty);
                return !string.IsNullOrEmpty(str) ? (MediaPlayerStretchingMode)Enum.Parse(typeof(MediaPlayerStretchingMode), str, true) : MediaPlayerStretchingMode.Proportionally;
            }
        }

        public string PlayerControlPanelBackgroundColor
        {
            get
            {
                return Parameters.Get<string>("PlayerControlPanelBackgroundColor", string.Empty);
            }
        }
        public string PlayerControlsColor
        {
            get
            {
                return Parameters.Get<string>("PlayerControlsColor", string.Empty);
            }
        }
        public string PlayerControlsOverColor
        {
            get
            {
                return Parameters.Get<string>("PlayerControlsOverColor", string.Empty);
            }
        }
        public string PlayerScreenColor
        {
            get
            {
                return Parameters.Get<string>("PlayerScreenColor", string.Empty);
            }
        }
        */
        #endregion

        #region Section properties

        protected BXIBlockSection section;
        public BXIBlockSection Section
        {
            get { return section; }
            set { section = value; }
        }

        private int sectionId = 0;
        public int SectionId
        {
            get
            {
                if (sectionId > 0)
                    return sectionId;
                else
                    return Parameters.Get<int>("SectionId");
            }
            private set { sectionId = value; }
        }

        public string SectionCode
        {
            get { return Parameters.Get<string>("SectionCode", String.Empty); }
        }

        public string SectionName
        {
            get
            {
                if (ComponentCache.ContainsKey("SectionName"))
                    return ComponentCache.Get<string>("SectionName");
                return null;
            }
            set { ComponentCache["SectionName"] = value; }
        }

        #endregion

        protected List<ElementListItem> _listItems = null;
        public IList<ElementListItem> Items
        {
            get { return new ReadOnlyCollection<ElementListItem>(GetListItems()); }
        }

        private List<ElementListItem> GetListItems() 
        {
            if (_listItems == null)
                _listItems = new List<ElementListItem>();
            return _listItems;
        }

        public IList<string> ShowProperties
        {
            get { return Parameters.GetListString("Properties"); }
        }

        public string errorMessage = String.Empty;
        private Error _errors = Error.ErrNone;
        public Error ComponentErrors
        {
            get { return _errors; }
        }

        protected override void OnLoad(EventArgs e)
        {
			base.OnLoad(e);

            BXPagingParams pagingParams = PreparePagingParams();
            if (IsCached(((BXPrincipal)Page.User).GetAllRoles(true), pagingParams))
            {
                SetTemplateCachedData();
                return;
            }

			using (BXTagCachingScope cacheScope = BeginTagCaching())
			{
				BXIBlockCollection iblockCollection = BXIBlock.GetList(
					new BXFilter(
						new BXFilterItem(BXIBlock.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlock.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlock.Fields.ID, BXSqlFilterOperators.Equal, IBlockId)
					),
					null,
					null,
					null,
					BXTextEncoder.HtmlTextEncoder
				);

				if (iblockCollection == null || iblockCollection.Count < 1)
				{
					GenerateNotFoundError(Error.ErrIBlockIsNotFound);
					return;
				}

				IBlockTypeId = iblockCollection[0].TypeId;
				IBlock = iblockCollection[0];
				IBlockName = iblockCollection[0].Name;
				IBlockElementName = iblockCollection[0].ElementName;
				IBlockSectionName = iblockCollection[0].SectionName;

				Dictionary<string, BXCustomField> iblockCustomFields = new Dictionary<string, BXCustomField>(iblock.CustomFields.Count);
				foreach (BXCustomField field in iblock.CustomFields)
					iblockCustomFields[field.CorrectedName] = field;

				//Filter
				if (!String.IsNullOrEmpty(SectionCode))
				{
					BXFilter sectionFilter = new BXFilter(
						new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
						new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlockSection.Fields.Code, BXSqlFilterOperators.Equal, SectionCode)
					);

					if (SectionId > 0)
						sectionFilter.Add(new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.Equal, SectionId));

					BXIBlockSectionCollection section = BXIBlockSection.GetList(sectionFilter, null, null, null, BXTextEncoder.HtmlTextEncoder);
					if (section != null && section.Count > 0)
					{
						Section = section[0];
						SectionId = section[0].Id;
					}
					else
					{
						Section = null;
						GenerateNotFoundError(Error.ErrIBlockSectionIsNotFound);
						return;
					}
				}
				else if (SectionId > 0)
				{
					BXIBlockSectionCollection sectionCollection = BXIBlockSection.GetList(
						new BXFilter(
							new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
							new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.Equal, SectionId),
							new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y")
						),
						null,
						null,
						null,
						BXTextEncoder.HtmlTextEncoder
					);

					if (sectionCollection == null || sectionCollection.Count < 1)
					{
						GenerateNotFoundError(Error.ErrIBlockSectionIsNotFound);
						return;
					}

					Section = sectionCollection[0];
				}

				if (Section != null && SectionId > 0)
					SectionName = Section.Name;

				BXFilter elementFilter = new BXFilter(
					new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
					new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
					new BXFilterItem(BXIBlockElement.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y")
				);

				if (SectionId > 0)
				{
					elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, SectionId));

					if (Parameters.Get<bool>("ShowSubElements", true))
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.IncludeParentSections, BXSqlFilterOperators.Equal, "Y"));
				}
				else if (!Parameters.Get<bool>("ShowAllElementsOnIndex", true))
				{
					elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.InRootSection, BXSqlFilterOperators.Equal, true));
				}

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

				if (!Parameters.GetBool("PagingAllow") && Parameters.GetInt("PagingRecordsPerPage", 0) > 0)
					queryParams = new BXQueryParams(new BXPagingOptions(0, Parameters.GetInt("PagingRecordsPerPage")));
				else if (!isLegalPage)
					AbortCache();

				BXIBlockElementCollection iblockElements = BXIBlockElement.GetList(
					elementFilter, 
					elementOrderBy,
					new BXSelectAdd(BXIBlockElement.Fields.CustomFields[IBlockId]), 
					queryParams, 
					BXTextEncoder.HtmlTextEncoder
					);
				List<ElementListItem> listItems = GetListItems();
				if (listItems.Count > 0)
					listItems.Clear();

				foreach (BXIBlockElement iblockElement in iblockElements)
				{
					ElementListItem listItem = new ElementListItem(this);

					listItem.Element = iblockElement;

					string url = Parameters.Get<string>("ElementDetailUrl");
					if (!String.IsNullOrEmpty(url))
					{
						string replaceSectionId = "0";
						string replaceSectionCode = String.Empty;

						if (iblockElement.Sections.Count > 0)
						{
							replaceSectionId = iblockElement.Sections[0].SectionId.ToString();
							if (url.Contains("#SectionCode#"))
							{
								BXIBlockSection elementSection = BXIBlockSection.GetById(replaceSectionId);
								if (elementSection != null)
									replaceSectionCode = elementSection.Code;
							}
						}

						BXParamsBag<object> replaceItems = new BXParamsBag<object>();
						replaceItems.Add("IblockId", iblockElement.IBlockId);
						replaceItems.Add("IblockCode", IBlock.Code);
						replaceItems.Add("ElementId", iblockElement.Id);
						replaceItems.Add("ElementCode", iblockElement.Code);
						replaceItems.Add("SectionId", replaceSectionId);
						replaceItems.Add("SectionCode", replaceSectionCode);

						url = MakeLink(url, replaceItems);

						listItem.ElementDetailUrl = url;

					}

					List<ElementListItemProperty> elementProperties = new List<ElementListItemProperty>();
					foreach (KeyValuePair<string, BXCustomProperty> elementProperty in iblockElement.Properties)
					{
						ElementListItemProperty property = new ElementListItemProperty();
						property.Name = elementProperty.Value.UserLikeName;
						property.Property = elementProperty.Value;

						string correctedName = elementProperty.Value.Name.ToUpper();
						property.Code = correctedName;
						if (iblockCustomFields.ContainsKey(correctedName))
							property.Field = iblockCustomFields[correctedName];

						property.Values = elementProperty.Value.Values;
						property.DisplayValue = GetCustomProperty(elementProperty.Value, "&nbsp;/&nbsp;");
						elementProperties.Add(property);
					}

					listItem.Properties = elementProperties;
					listItems.Add(listItem);
				}

				SetTemplateCachedData();
				IncludeComponentTemplate();
			}
        }
        private void SetTemplateCachedData()
        {
            if (IsComponentDesignMode || !Parameters.Get<bool>("SetPageTitle", true) || string.IsNullOrEmpty(SectionName))
                return;

            BXPublicPage bitrixPage = Page as BXPublicPage;
            if (bitrixPage != null)
                bitrixPage.BXTitle = BXTextEncoder.HtmlTextEncoder.Decode(SectionName);
        }

        private void GenerateNotFoundError(Error error)
        {
            _errors |= error;

            AbortCache();

            ScriptManager ajax = ScriptManager.GetCurrent(Page);
            if (!IsComponentDesignMode && BXConfigurationUtility.ShowMode == BXShowMode.View && (ajax == null || !ajax.IsInAsyncPostBack))
				BXError404Manager.Set404Status(Response);

            IncludeComponentTemplate();
        }

        private string GetCustomProperty(BXCustomProperty property, string separator)
        {
            if (property.IsMultiple && property.Values.Count > 0)
            {
                StringBuilder returnString = new StringBuilder();
                returnString.Append(BXTextEncoder.HtmlTextEncoder.Encode(property.Values[0].ToString()));

                for (int j = 1; j < property.Values.Count; j++)
                {
                    returnString.Append(separator);
                    returnString.Append(BXTextEncoder.HtmlTextEncoder.Encode(property.Values[j].ToString()));
                }

                if (returnString.Length > 0)
                    return returnString.ToString();
            }
            else if (property.Value != null)
            {
                return BXTextEncoder.HtmlTextEncoder.Encode(property.Value.ToString());
            }

            return null;
        }

        #region Admin Panel & Context Menu

        public string AdminPanelSectionId
        {
            get { return "MediaGalleryListMenuSection"; }
        }

        public string AdminPanelButtonMenuId
        {
            get { return String.Format("MediaGalleryListButtonMenu_{0}", IBlockId); }
        }

        //Создаем секцию и кнопку для выпадающего меню в панели администратора
        public override void CreatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
        {
             
            if (!(Parameters.Get<bool>("AddAdminPanelButtons") && _errors == Error.ErrNone))
                return;

            if (sectionList == null)
                throw new ArgumentNullException("sectionList");

            if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
                !BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
                return;

            //Создаем секцию
            BXPublicPanelMenuSection iblockListSection = sectionList.FindByID(AdminPanelSectionId);
            if (iblockListSection == null)
            {
                BXPublicPanelMenuSectionInfo sectionInfo = new BXPublicPanelMenuSectionInfo(AdminPanelSectionId, 100);
                iblockListSection = new BXPublicPanelMenuSection(sectionInfo);
                sectionList.Add(iblockListSection);
            }

            //Создаем кнопку
            BXPublicPanelMenu listButtonMenu = iblockListSection.FindByID(AdminPanelButtonMenuId);
            if (listButtonMenu == null)
            {
                listButtonMenu = new BXPublicPanelMenu();
                listButtonMenu.ID = AdminPanelButtonMenuId;
                listButtonMenu.BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/iblock.gif");
                listButtonMenu.Caption = BXTextEncoder.HtmlTextEncoder.Decode(IBlockName);
                listButtonMenu.Hint = BXTextEncoder.HtmlTextEncoder.Decode(IBlockName);
                listButtonMenu.ShowMode = BXShowMode.NonView;
                iblockListSection.Add(listButtonMenu);
            }
        }

        //Заполняем выпадающее меню пунктами
        public override void PopulatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
        {
            if (!(Parameters.Get<bool>("AddAdminPanelButtons") && _errors == Error.ErrNone))
                return;

            if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
                !BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
                return;

            if (sectionList == null)
                throw new ArgumentNullException("sectionList");

            BXPublicPanelMenuSection iblockListSection = sectionList.FindByID(AdminPanelSectionId);
            if (iblockListSection == null)
                return;

            BXPublicPanelMenu listButtonMenu = iblockListSection.FindByID(AdminPanelButtonMenuId);
            if (listButtonMenu == null)
                return;

            if (listButtonMenu.PopupMenu == null)
                listButtonMenu.PopupMenu = new BXPopupMenuV2();

            BXPopupMenuItem[] menuItems = GetMenuItems();
            for (int menuIndex = 0; menuIndex < menuItems.Length; menuIndex++)
            {
                bool itemExists = false;
                foreach (BXPopupMenuBaseItem item in listButtonMenu.PopupMenu.Items)
                {
                    if (item.ID == menuItems[menuIndex].ID)
                    {
                        itemExists = true;
                        break;
                    }
                }

                if (!itemExists)
                    listButtonMenu.PopupMenu.AddItem(menuItems[menuIndex]);
            }
        }
        public BXPopupMenuItem[] GetMenuItems()
        {
            BXPopupMenuItem[] menuItems = new BXPopupMenuItem[1];

            menuItems[0] = new BXPopupMenuItem();
            menuItems[0].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/new_element.gif");
            menuItems[0].ID = String.Format("mediagallery_element_new_{0}", IBlockId);
            menuItems[0].Text = String.Format(GetMessageRaw("AddNewElement"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
            menuItems[0].ClientClickScript =
                String.Format(
                    "jsUtils.Redirect(arguments, '{0}?type_id={2}&iblock_id={1}&section_id={3}&{4}={5}')",
                    VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
                    IBlockId,
                    IBlockTypeId,
                    SectionId,
                    BXConfigurationUtility.Constants.BackUrl,
                    UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery)
                );

            //menuItems[1] = new BXPopupMenuItem();
            //menuItems[1].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/new_section.gif");
            //menuItems[1].ID = String.Format("mediagallery_section_new_{0}", IBlockId);
            //menuItems[1].Text = String.Format(GetMessageRaw("AddNewSection"), String.IsNullOrEmpty(IBlockSectionName) ? GetMessageRaw("IBlockSectionName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockSectionName.ToLower()));
            //menuItems[1].ClientClickScript =
            //    String.Format(
            //        "jsUtils.Redirect(arguments, '{0}?type_id={2}&iblock_id={1}&section_id={3}&{4}={5}')",
            //        VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"),
            //        IBlockId,
            //        IBlockTypeId,
            //        SectionId,
            //        BXConfigurationUtility.Constants.BackUrl,
            //        UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery)
            //    );

            return menuItems;
        }

		public BXHermitageToolbar CreateElementContextToolbar(BXIBlockElement element, string containerClientID)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			if (!(BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) &&
				BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements)))
				return new BXHermitageToolbar();

			BXHermitageToolbarSettings settings = new BXHermitageToolbarSettings();
			settings.RequireIncludeAreasFlag = true;
			settings.ParentClientID = containerClientID;
			settings.Data.Add("IBlockId", IBlockId);
			settings.Data.Add("IBlockTypeId", IBlockTypeId);

			if (element != null)
				settings.Data.Add("Element", element);

			if (IBlock != null)
				settings.Data.Add("IBlock", IBlock);

			settings.Data.Add("ChangeElement",
				string.Format(GetMessageRaw("EditElementMenuItem"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower())));

			settings.Data.Add("DeleteElement",
				string.Format(GetMessageRaw("DeleteElementMenuItem"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower())));

			settings.Data.Add("ElementDetailUrl", Parameters.Get<string>("DetailUrl"));

			settings.Data.Add("ElementDeletionConfirmation",
				string.Format(GetMessageRaw("ElementDeletionConfirmation"),
				(IBlock != null) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(IBlock.CaptionsInfo.ElementName.ToLower())
				: string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower())));

			return BXHermitageToolbarHelper.CreateToolbar(BXHermitageMenuCommand.ModifyElement | BXHermitageMenuCommand.DeleteElement, settings);
		}

        //Создаем контекстное меню
        public override BXComponentPopupMenuInfo PopupMenuInfo
        {
            get
            {
				if(this.popupMenuInfo != null)
					return this.popupMenuInfo;

                BXComponentPopupMenuInfo info = base.PopupMenuInfo;

                if (BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) 
					&& BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements) 
					&& _errors == Error.ErrNone)
                {
                    info.CreateComponentContentMenuItems = delegate(BXShowMode showMode)
                    {
						if (showMode == BXShowMode.View)
							return new BXHermitagePopupMenuBaseItem[0];


						BXHermitageToolbarSettings settings = new BXHermitageToolbarSettings();
						settings.Data.Add("IBlockId", IBlockId);
						settings.Data.Add("IBlockTypeId", IBlockTypeId);

						if (IBlock != null)
							settings.Data.Add("IBlock", IBlock);

						settings.Data.Add("AddElement",
							string.Format(GetMessageRaw("AddNewElement"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower())));

						return BXHermitageToolbarHelper.CreateToolbar(BXHermitageMenuCommand.CreateElement, settings).ItemsToArray();
                    };
                }
                return info;
            }
        }
        #endregion

        protected override void PreLoadComponentDefinition()
        {            
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/playlist.gif";
            Group = new BXComponentGroup("media", GetMessageRaw("Group"), 100, BXComponentGroup.Content);

            BXCategory dataSourceCategory = BXCategory.DataSource;
            BXCategory mainCategory = BXCategory.Main;
            BXCategory addSettingsCategory = BXCategory.AdditionalSettings;
            BXCategory sefCategory = BXCategory.Sef;

            string viewID = ClientID;
            ParamsDefinition.Add(BXParametersDefinition.Cache);
            ParamsDefinition.Add(BXParametersDefinition.GetPaging(viewID));
            ParamsDefinition.Add(BXParametersDefinition.GetPagingUrl(viewID));

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

            //ParamsDefinition.Add(
            //    "SectionId",
            //    new BXParamText(
            //        GetMessageRaw("SectionID"),
            //        "0",
            //        mainCategory
            //    ));

            ParamsDefinition.Add(
                "SectionId",
                new BXParamSingleSelection(
                    GetMessageRaw("SectionId"),
                    String.Empty,
                    mainCategory
                )
            );

            ParamsDefinition.Add(
                "SectionCode",
                new BXParamText(
                GetMessageRaw("SectionMnemonicCode"),
                "",
                mainCategory
            ));

            #region Player
            /*
            BXCategory appearance = new BXCategory(GetMessageRaw("Category.AppearanceCommon.Title"), "PLAYER_APPEARANCE", 110); 
            ParamsDefinition.Add(
                "PlayerWidth",
                new BXParamText(
                    GetMessageRaw("PlayerWidth"),
                    "425px",
                    appearance
                    )
            );


            ParamsDefinition.Add(
                "PlayerHeight",
                new BXParamText(
                    GetMessageRaw("PlayerHeight"),
                    "344px",
                    appearance
                    )
            );

            ParamsDefinition.Add(
                "PlayerStretching",
                new BXParamSingleSelection(
                    GetMessageRaw("PlayerStretching"),
                    Enum.GetName(typeof(MediaPlayerStretchingMode), MediaPlayerStretchingMode.Proportionally),
                    appearance
                    )
            );

            ParamsDefinition.Add(
                "PlayerControlPanelBackgroundColor",
                new BXParamText(
                    GetMessageRaw("PlayerControlPanelBackgroundColor"),
                    "FFFFFF",
                    appearance
                    )
            );

            ParamsDefinition.Add(
                "PlayerControlsColor",
                new BXParamText(
                    GetMessageRaw("PlayerControlsColor"),
                    "000000",
                    appearance
                    )
            );

            ParamsDefinition.Add(
                "PlayerControlsOverColor",
                new BXParamText(
                    GetMessageRaw("PlayerControlsOverColor"),
                    "000000",
                    appearance
                    )
            );

            ParamsDefinition.Add(
                "PlayerScreenColor",
                new BXParamText(
                    GetMessageRaw("PlayerScreenColor"),
                    "000000",
                    appearance
                    )
            );
            */
            #endregion
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
                "ShowSubElements",
                new BXParamYesNo(
                    GetMessageRaw("DisplaySubsectionElements"),
                    true,
                    dataSourceCategory
                ));

            ParamsDefinition.Add(
                "ShowAllElementsOnIndex",
                new BXParamYesNo(
                    GetMessageRaw("ShowAllElementsOnIndex"),
                    true,
                    dataSourceCategory

            ));

            ParamsDefinition.Add(
                "Properties",
                new BXParamMultiSelection(
                    GetMessageRaw("Properties"),
                    "-",
                    addSettingsCategory
                ));

            ParamsDefinition.Add(
                "AddAdminPanelButtons",
                new BXParamYesNo(
                    GetMessageRaw("AddButtonsForThisComponentToAdminPanel"),
                    false,
                    addSettingsCategory
                ));

            ParamsDefinition.Add(
                "SetPageTitle",
                new BXParamYesNo(
                    GetMessageRaw("SetPageTitle"),
                    true,
                    addSettingsCategory
               ));

            ParamsDefinition.Add(
                "SectionElementListUrl",
                new BXParamText(
                    GetMessageRaw("Section"),
                    "list.aspx?section_id=#SectionId#",
                    sefCategory
                ));

            ParamsDefinition.Add(
                "ElementDetailUrl",
                new BXParamText(
                    GetMessageRaw("DetailedInfo"),
                    "detail.aspx?section_id=#SectionId#&element_id=#ElementId#",
                    sefCategory
                ));
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
            List<BXParamValue> iblockProperty = new List<BXParamValue>();
            int selectedIblockId = 0;
            if (Parameters.ContainsKey("IBlockId"))
                int.TryParse(Parameters["IBlockId"], out selectedIblockId);

            List<BXParamValue> properties = new List<BXParamValue>();
            List<BXParamValue> sortProperties = new List<BXParamValue>();

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

            ParamsDefinition["Properties"].Values = properties;
            ParamsDefinition["IBlockElementPropertyForFilePath"].Values = properties;
            ParamsDefinition["IBlockElementPropertyForPlaylistPreviewImageFilePath"].Values = properties;
            ParamsDefinition["IBlockElementPropertyForPlayerPreviewImageFilePath"].Values = properties;

            //Sorting
            List<BXParamValue> sortingFields = new List<BXParamValue>();
            sortingFields.Add(new BXParamValue(GetMessageRaw("ElementID"), "ID"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("ElementName"), "Name"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("ActiveFromDate"), "ActiveFromDate"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("ActiveUntilDate"), "ActiveToDate"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("SortIndex"), "Sort"));
            sortingFields.Add(new BXParamValue(GetMessageRaw("DateOfModification"), "UpdateDate"));
            sortingFields.AddRange(sortProperties);

            ParamsDefinition["SortBy"].Values = sortingFields;


            #region PlayerStretching
            /*
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
            */
            #endregion

            #region SectionId
            List<BXParamValue> sectionValueLst = new List<BXParamValue>();
            sectionValueLst.Add(new BXParamValue(GetMessageRaw("NotSelected2"), "0"));

            if (IBlockId > 0)
            {
                BXIBlockSectionCollection sectionList = BXIBlockSection.GetList(
                    new BXFilter(
                        new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
                        new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId)
                    ),
                    new BXOrderBy(
                        new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)
                    )
                );

                int sectionListCount = sectionList.Count;
                if (sectionListCount > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < sectionListCount; i++)
                    {
                        BXIBlockSection section = sectionList[i];
                        if (sb.Length > 0)
                            sb.Length = 0;
                        for (int j = 0; j < section.DepthLevel; j++)
                            sb.Append('.');
                        sb.Append(section.Name);
                        sectionValueLst.Add(new BXParamValue(sb.ToString(), section.Id.ToString()));
                    }
                }
                ParamsDefinition["SectionId"].Values = sectionValueLst;
            }
            #endregion
        }

        public class ElementListItem
        {
            private MediaGalleryElementListComponent _parent = null;
            public ElementListItem(MediaGalleryElementListComponent parent) 
            {
                if (parent == null)
                    throw new ArgumentNullException("parent");
                _parent = parent;
            }
            private BXIBlockElement element;
            public BXIBlockElement Element
            {
                get { return element; }
                set { element = value; }
            }

            private string elementDetailUrl;
            public string ElementDetailUrl
            {
                get { return elementDetailUrl; }
                set { elementDetailUrl = value; }
            }

			Dictionary<int, BXFile> cachedFiles = new Dictionary<int,BXFile>();

            public string ElementFileUrl 
            {
                get 
                {
                    if (element == null)
                        return string.Empty;
                    string propName = _parent.Parameters["IBlockElementPropertyForFilePath"];
                    if (string.IsNullOrEmpty(propName))
                        return string.Empty;

					BXCustomProperty prop;
					if (!element.Properties.TryGetValue(propName, out prop) || prop.Value == null)
						return string.Empty;

					if (prop.IsFile)
					{
						int id = (int)prop.Value;
						BXFile f;
						if (!cachedFiles.TryGetValue(id, out f))
							cachedFiles.Add(id, f = BXFile.GetById(id, BXTextEncoder.EmptyTextEncoder));

						return f != null ? (HttpContext.Current != null ? string.Format("{0}://{1}:{2}{3}",  HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port, f.FilePath) : f.FilePath) : string.Empty;
					}
					return prop.Value.ToString();
                }
            }

            public string ElementPlaylistPreviewImageFileUrl
            {
                get
                {
                    if (element == null)
                        return string.Empty;
                    string propName = _parent.Parameters["IBlockElementPropertyForPlaylistPreviewImageFilePath"];
                    if (string.IsNullOrEmpty(propName))
                        return string.Empty;

					BXCustomProperty prop;
					if (!element.Properties.TryGetValue(propName, out prop) || prop.Value == null)
						return string.Empty;

					if (prop.IsFile)
					{
						int id = (int)prop.Value;
						BXFile f;
						if (!cachedFiles.TryGetValue(id, out f))
							cachedFiles.Add(id, f = BXFile.GetById(id, BXTextEncoder.EmptyTextEncoder));

						return f != null ? (HttpContext.Current != null ? string.Format("{0}://{1}:{2}{3}",  HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port, f.FilePath) : f.FilePath) : string.Empty;
					}
					return prop.Value.ToString();
                }
            }

            public string ElementPlayerPreviewImageFileUrl
            {
                get
                {
                    if (element == null)
                        return string.Empty;
                    string propName = _parent.Parameters["IBlockElementPropertyForPlayerPreviewImageFilePath"];
                    if (string.IsNullOrEmpty(propName))
                        return string.Empty;

					BXCustomProperty prop;
					if (!element.Properties.TryGetValue(propName, out prop) || prop.Value == null)
						return string.Empty;

					if (prop.IsFile)
					{
						int id = (int)prop.Value;
						BXFile f;
						if (!cachedFiles.TryGetValue(id, out f))
							cachedFiles.Add(id, f = BXFile.GetById(id, BXTextEncoder.EmptyTextEncoder));

						return f != null ? (HttpContext.Current != null ? string.Format("{0}://{1}:{2}{3}",  HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port, f.FilePath) : f.FilePath) : string.Empty;
					}
					return prop.Value.ToString();
                }
            }

            public string ElementPreviewText 
            {
                get { return element != null ? element.PreviewText : string.Empty; }
            }

            private List<ElementListItemProperty> properties;
            public List<ElementListItemProperty> Properties
            {
                get { return properties; }
                set { properties = value; }
            }
        }

        public class ElementListItemProperty
        {
            BXCustomField field;
            public BXCustomField Field
            {
                set { field = value; }
                get { return field; }
            }

            BXCustomProperty property;
            public BXCustomProperty Property
            {
                set { property = value; }
                get { return property; }
            }

            private string name;
            public string Name
            {
                set { name = value; }
                get { return name; }
            }

            private string code;
            public string Code
            {
                get { return code; }
                set { code = value; }
            }

            private List<object> values;
            public List<object> Values
            {
                get { return values; }
                set { values = value; }
            }

            private string displayValue;
            public string DisplayValue
            {
                get { return displayValue; }
                set { displayValue = value; }
            }
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

    public class MediaGalleryElementListTemplate : BXComponentTemplate<MediaGalleryElementListComponent>
    {
        protected override void Render(HtmlTextWriter writer)
        {
            StartWidth = "100%";
            if (IsComponentDesignMode && 
                ((Component.ComponentErrors & MediaGalleryElementListComponent.Error.ErrIBlockIsNotFound) == MediaGalleryElementListComponent.Error.ErrIBlockIsNotFound ||
                (Component.ComponentErrors & MediaGalleryElementListComponent.Error.ErrIBlockSectionIsNotFound) == MediaGalleryElementListComponent.Error.ErrIBlockSectionIsNotFound))
                writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
            else
                base.Render(writer);
        }

		protected string GetItemContainerClientID(int itemId)
		{
			return string.Concat(ClientID, ClientIDSeparator, "MediaGalleryItem", itemId.ToString());
		}

		public void RenderElementToolbar(BXIBlockElement element, string containerClientID)
		{
			Component.CreateElementContextToolbar(element, containerClientID).Render(CurrentWriter);
		}
    }
}
