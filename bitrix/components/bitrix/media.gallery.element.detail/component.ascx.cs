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
using Bitrix;
using Bitrix.UI;
using Bitrix.IBlock;
using Bitrix.IO;
using System.Collections.Generic;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Configuration;
using System.Text;
using Bitrix.Components;
using Bitrix.Services;
using Bitrix.UI.Popup;
using Bitrix.Components.Editor;
using Bitrix.UI.Hermitage;

namespace Bitrix.IBlock.Components
{
	public partial class MediaGalleryElementDetailComponent : Bitrix.UI.BXComponent
	{
        #region Enums
        [FlagsAttribute]
        public enum ComponentError
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
            /// Не найден элемент
            /// </summary>
            ErrIBlockElementIsNotFound = 2
        }
        #endregion
        private ComponentError _errors = ComponentError.ErrNone;
        public ComponentError ComponentErrors
        {
            get { return _errors; }
        }

        private void GenerateNotFoundError(ComponentError error)
        {
            _errors |= error;

            AbortCache();

            ScriptManager ajax = ScriptManager.GetCurrent(Page);
            if (!IsComponentDesignMode && BXConfigurationUtility.ShowMode == BXShowMode.View && (ajax == null || !ajax.IsInAsyncPostBack))
                BXError404Manager.Set404Status(Response);
        }

		private BXIBlockElement element;
		public BXIBlockElement Element
		{
            get { return this.element; }
			internal set{ this.element = value; }
		}

		public int ElementId
		{
			get{ return GetParameter<int>("ElementId", 0); }
            internal set{ ComponentCache["ElementId"] = value; }
		}

		public string ElementCode
		{
			get{ return GetParameter<string>("ElementCode", string.Empty); }
            internal set { ComponentCache["ElementCode"] = value; }
		}

		public string ElementName
		{
			set{ ComponentCache["ElementName"] = value; }
			get
			{
				if (!ComponentCache.ContainsKey("ElementName"))
					return string.Empty;
				return ComponentCache.Get<string>("ElementName");
			}
		}

		public int IBlockId
		{
			get{ return GetParameter<int>("IBlockId", 0); }
		}

		public string IBlockUrl
		{
			get { return Parameters["IBlockUrl"]; }
		}

		public string IBlockUrlTitle
		{
			get { return Parameters.Get<string>("IBlockUrlTitle"); }
		}

		public string PlayerWidth
		{
			get{ return GetParameter<string>("PlayerWidth", string.Empty); }
		}

		public string PlayerHeight
		{
			get{ return GetParameter<string>("PlayerHeight", string.Empty); }
		}

		public MediaPlayerStretchingMode PlayerStretching
		{
			get
			{
				string str = Parameters.Get("PlayerStretching", string.Empty);
				MediaPlayerStretchingMode r = !string.IsNullOrEmpty(str) ? (MediaPlayerStretchingMode)Enum.Parse(typeof(MediaPlayerStretchingMode), str, true) : MediaPlayerStretchingMode.Proportionally;
                ComponentCache["PlayerStretching"] = r;
                return r;
			}
		}

		public string PlayerDownloadingLinkTargetWindow
		{
			get{ return GetParameter("PlayerDownloadingLinkTargetWindow", "_self"); }
		}

        private T GetParameter<T>(string key, T defaultValue) 
        {
            object rObj = null;
            if (ComponentCache.TryGetValue(key, out rObj))
                return (T)rObj;

            T r = Parameters.Get<T>(key, defaultValue);
            ComponentCache[key] = r;
            return r;
        }

		/// <summary>
		/// Разрешить скачивание
		/// </summary>
		public bool PlayerEnableDownloading
		{
			get{ return GetParameter<bool>("PlayerEnableDownloading", false); }
		}

        /// <summary>
        /// Разрешить переключение в полноэкранный режим
        /// </summary>
        public bool PlayerEnableFullScreenModeSwitch
        {
            get{ return GetParameter<bool>("PlayerEnableFullScreenModeSwitch", true); }
        }

		/// <summary>
		/// Отображать панель управления
		/// </summary>
		public bool PlayerShowControlPanel
		{
			get{ return GetParameter<bool>("PlayerShowControlPanel", true); }
		}

		public string PlayerControlPanelBackgroundColor
		{
            get{ return GetParameter<string>("PlayerControlPanelBackgroundColor", string.Empty); }
		}

		public string PlayerControlsColor
		{
            get{ return GetParameter<string>("PlayerControlsColor", string.Empty); }
		}

		public string PlayerControlsOverColor
		{
            get{ return GetParameter<string>("PlayerControlsOverColor", string.Empty); }
		}

		public string PlayerScreenColor
		{
            get{ return GetParameter<string>("PlayerScreenColor", string.Empty); }
		}

        public int PlayerVolumeLevelInPercents
        {
            get { return GetParameter<int>("PlayerVolumeLevelInPercents", 90); }
        }

        public bool PlayerEnableAutoStart
        {
            get { return GetParameter<bool>("PlayerEnableAutoStart", false); }
        }


        public bool PlayerEnableRepeatMode
        {
            get { return GetParameter<bool>("PlayerEnableRepeatMode", false); }
        }

		/// <summary>
		/// Размер буфера в секундах
		/// </summary>
		public int PlayerBufferLengthInSeconds
		{
            get{ return GetParameter<int>("PlayerBufferLengthInSeconds", 10); }
			set
			{
				if (value < 0)
					value = 10;
				Parameters["PlayerBufferLengthInSeconds"] = value.ToString();
                ComponentCache["PlayerBufferLengthInSeconds"] = value;
			}
		}

		public List<string> ShowProperties
		{
			get{ return Parameters.GetListString("Properties"); }
		}

		public string PropertyKeywordsCode
		{
            get{ return GetParameter<string>("PropertyKeywords", "-"); }
		}

		public string PropertyKeywordsValue
		{
			set{ ComponentCache["PropertyKeywords"] = value; }
			get
			{
				if (!ComponentCache.ContainsKey("PropertyKeywords"))
					return null;
				return ComponentCache.Get<string>("PropertyKeywords");
			}
		}

		public string PropertyDescriptionCode
		{
			get{ return GetParameter<string>("PropertyDescription", "-"); }
		}

		public string PropertyDescriptionValue
		{
			set
			{
				ComponentCache["PropertyDescription"] = value;
			}
			get
			{
				if (!ComponentCache.ContainsKey("PropertyDescription"))
					return null;
				return ComponentCache.Get<string>("PropertyDescription");
			}
		}

		private List<ElementDetailProperty> properties;
		public List<ElementDetailProperty> Properties
		{
			get
			{
				return properties;
			}
			set
			{
				properties = value;
			}
		}

		public string IBlockName
		{
			set
			{
				ComponentCache["IBlockName"] = value;
			}
			get
			{
				if (!ComponentCache.ContainsKey("IBlockName"))
					return null;
				return ComponentCache.Get<string>("IBlockName");
			}
		}

		public int IBlockTypeId
		{
			set
			{
				ComponentCache["IBlockTypeID"] = value;
			}
			get
			{
				return ComponentCache.Get<int>("IBlockTypeID");
			}
		}

		public string IBlockElementName
		{
			set
			{
				ComponentCache["IBlockElementName"] = value;
			}
			get
			{
				return ComponentCache.Get<string>("IBlockElementName");
			}
		}

		Dictionary<int, BXFile> cachedFiles = new Dictionary<int, BXFile>();

		public string ElementFileUrl
		{
			get
			{
				if (this.element == null)
					return string.Empty;
				string propName = Parameters["IBlockElementPropertyForFilePath"];
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

					return f != null ? string.Format("{0}://{1}:{2}{3}", Request.Url.Scheme, Request.Url.Host, Request.Url.Port, f.FilePath) : string.Empty;
				}
				return prop.Value.ToString();
			}
		}


		public string ElementPlayerPreviewImageFileUrl
		{
			get
			{
				if (this.element == null)
					return string.Empty;

				string propName = Parameters["IBlockElementPropertyForPlayerPreviewImageFilePath"];
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

					return f != null ? string.Format("{0}://{1}:{2}{3}", Request.Url.Scheme, Request.Url.Host, Request.Url.Port, f.FilePath) : string.Empty;
				}
				return prop.Value.ToString();
			}
		}

		public string ElementPropertyForDownloadingFilePath
		{
			get
			{
				if (this.element == null)
					return string.Empty;

				string propName = Parameters["IBlockElementPropertyForDownloadingFilePath"];
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

					return f != null ? string.Format("{0}://{1}:{2}{3}", Request.Url.Scheme, Request.Url.Host, Request.Url.Port, f.FilePath) : string.Empty;
				}
				return prop.Value.ToString();
			}
		}


		#region Public Menu
		public string AdminPanelSectionId
		{
			get
			{
				return "MediaGalleryDetailMenuSection";
			}
		}

		public string AdminPanelButtonMenuId
		{
			get
			{
				return String.Format("MediaGalleryDetailButtonMenu_{0}", IBlockId);
			}
		}
		#endregion

		public string errorMessage = String.Empty;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (IsCached(((BXPrincipal)Page.User).GetAllRoles(true)))
			{
				SetTemplateCachedData();
				return;
			}

			using (BXTagCachingScope cacheScope = BeginTagCaching())
			{
				BXIBlock iblock = null;

				if (IBlockId <= 0)
					GenerateNotFoundError(ComponentError.ErrIBlockIsNotFound);
				else if (ElementId <= 0 && String.IsNullOrEmpty(ElementCode))
					GenerateNotFoundError(ComponentError.ErrIBlockElementIsNotFound);
				else
				{
					iblock = BXIBlock.GetById(IBlockId, BXTextEncoder.HtmlTextEncoder);
					if (iblock == null)
						GenerateNotFoundError(ComponentError.ErrIBlockIsNotFound);
				}

				BXIBlockElementCollection elementCollection = null;
				if (_errors == ComponentError.ErrNone)
				{
					IBlockName = iblock.Name;
					IBlockTypeId = iblock.TypeId;
					IBlockElementName = iblock.ElementName;

					BXFilter elementFilter = new BXFilter(
						new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlockElement.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId)
						//new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ActiveGlobal, BXSqlFilterOperators.Equal, "Y")
					);

					if (!String.IsNullOrEmpty(ElementCode))
					{
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Code, BXSqlFilterOperators.Equal, ElementCode));

						if (ElementId > 0)
							elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, ElementId));
					}
					else
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, ElementId));

					elementCollection = BXIBlockElement.GetList(
						elementFilter, 
						null,
						new BXSelectAdd(BXIBlockElement.Fields.CustomFields[IBlockId]), 
						null, 
						BXTextEncoder.HtmlTextEncoder
						);
					if (elementCollection == null || elementCollection.Count == 0)
						GenerateNotFoundError(ComponentError.ErrIBlockElementIsNotFound);
				}

				if (_errors != ComponentError.ErrNone)
				{
					IncludeComponentTemplate();
					return;
				}

				element = elementCollection[0];
				ElementId = element.Id;

				ElementName = element.Name;
				//PreviewImage = element.PreviewImageId > 0 ? element.PreviewImage : null;
				//DetailImage = element.DetailImageId > 0 ? element.DetailImage : null;

				Dictionary<string, BXCustomField> iblockCustomFields = new Dictionary<string, BXCustomField>(iblock.CustomFields.Count);
				foreach (BXCustomField field in iblock.CustomFields)
					iblockCustomFields[field.CorrectedName] = field;

				//Properties
				List<ElementDetailProperty> elementProperties = new List<ElementDetailProperty>();
				foreach (KeyValuePair<string, BXCustomProperty> elementProperty in element.Properties)
				{
					ElementDetailProperty property = new ElementDetailProperty();
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

				Properties = elementProperties;

				if (element.Properties.ContainsKey(PropertyKeywordsCode))
					PropertyKeywordsValue = GetCustomProperty(element.Properties[PropertyKeywordsCode], ", ");

				if (element.Properties.ContainsKey(PropertyDescriptionCode))
					PropertyDescriptionValue = GetCustomProperty(element.Properties[PropertyDescriptionCode], ", ");

				SetTemplateCachedData();
				IncludeComponentTemplate();
			}
		}

		private void SetTemplateCachedData()
		{
			BXPublicPage bitrixPage = Page as BXPublicPage;
			if (bitrixPage != null && !IsComponentDesignMode)
			{
				if (Parameters.Get<bool>("SetPageTitle", true) && ElementName != null)
					bitrixPage.BXTitle = BXTextEncoder.HtmlTextEncoder.Decode(ElementName);

				if (PropertyDescriptionValue != null)
					bitrixPage.Keywords["description"] = PropertyDescriptionValue;

				if (PropertyKeywordsValue != null)
					bitrixPage.Keywords["keywords"] = PropertyKeywordsValue;
			}
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

		//Создаем секцию и кнопку для выпадающего меню в панели администратора
		public override void CreatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
		{
			if (!Parameters.Get<bool>("AddAdminPanelButtons") || Element == null)
				return;

			if (sectionList == null)
				throw new ArgumentNullException("sectionList");

			if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
				!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
				return;

			//Создаем секцию
			BXPublicPanelMenuSection iblockElementSection = sectionList.FindByID(AdminPanelSectionId);
			if (iblockElementSection == null)
			{
				BXPublicPanelMenuSectionInfo sectionInfo = new BXPublicPanelMenuSectionInfo(AdminPanelSectionId, 100);
				iblockElementSection = new BXPublicPanelMenuSection(sectionInfo);
				sectionList.Add(iblockElementSection);
			}

			//Создаем кнопку
			BXPublicPanelMenu elementEditMenu = iblockElementSection.FindByID(AdminPanelButtonMenuId);
			if (elementEditMenu == null)
			{
				elementEditMenu = new BXPublicPanelMenu();
				elementEditMenu.ID = AdminPanelButtonMenuId;
				elementEditMenu.BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/iblock.gif");
				elementEditMenu.Caption = BXTextEncoder.HtmlTextEncoder.Decode(IBlockName);
				elementEditMenu.Hint = BXTextEncoder.HtmlTextEncoder.Decode(IBlockName);
				elementEditMenu.ShowMode = BXShowMode.NonView;
				iblockElementSection.Add(elementEditMenu);
			}
		}

		//Заполняем выпадающее меню пунктами "редактировать элемент" и "добавить элемент"
		public override void PopulatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
		{
			if (!Parameters.Get<bool>("AddAdminPanelButtons") || Element == null)
				return;

			if (sectionList == null)
				throw new ArgumentNullException("sectionList");

			if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
				!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
				return;

			BXPublicPanelMenuSection iblockElementSection = sectionList.FindByID(AdminPanelSectionId);
			if (iblockElementSection == null)
				return;

			BXPublicPanelMenu elementEditMenu = iblockElementSection.FindByID(AdminPanelButtonMenuId);
			if (elementEditMenu == null)
				return;

			if (elementEditMenu.PopupMenu == null)
				elementEditMenu.PopupMenu = new BXPopupMenuV2();

			BXPopupMenuItem[] menuItems = GetMenuItems();
			for (int menuIndex = 0; menuIndex < menuItems.Length; menuIndex++)
			{
				bool itemExists = false;
				foreach (BXPopupMenuBaseItem item in elementEditMenu.PopupMenu.Items)
				{
					if (item.ID == menuItems[menuIndex].ID)
					{
						itemExists = true;
						break;
					}
				}

				if (!itemExists)
					elementEditMenu.PopupMenu.AddItem(menuItems[menuIndex]);
			}
		}

		public BXPopupMenuItem[] GetMenuItems()
		{
			BXPopupMenuItem[] menuItems = new BXPopupMenuItem[2];

			menuItems[0] = new BXPopupMenuItem();
			menuItems[0].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/edit_element.gif");
			menuItems[0].ID = String.Format("mediagallery_element_edit_{0}", ElementId);
			menuItems[0].Text = String.Format(GetMessageRaw("EditElementMenuItem"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
			menuItems[0].ClientClickScript =
				String.Format(
					"jsUtils.Redirect(arguments, '{0}?type_id={3}&iblock_id={2}&section_id={4}&id={1}&{5}={6}')",
					VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
                    ElementId,
					IBlockId,
					IBlockTypeId,
					Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0,
					BXConfigurationUtility.Constants.BackUrl,
					UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery)
				);

			menuItems[1] = new BXPopupMenuItem();
			menuItems[1].Text = String.Format(GetMessageRaw("CreateElementMenuItem"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
			menuItems[1].ID = String.Format("mediagallery_element_new_{0}", IBlockId);
			menuItems[1].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/new_element.gif");
			menuItems[1].ClientClickScript =
				String.Format(
					"jsUtils.Redirect(arguments, '{0}?type_id={2}&iblock_id={1}&section_id={3}&{4}={5}')",
					VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
					IBlockId,
					IBlockTypeId,
					Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0,
					BXConfigurationUtility.Constants.BackUrl,
					UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery)
				);

			return menuItems;
		}

		//Создаем контекстное меню
		public override BXComponentPopupMenuInfo PopupMenuInfo
		{
			get
			{
				if(this.popupMenuInfo != null)
					return this.popupMenuInfo;

				BXComponentPopupMenuInfo info = base.PopupMenuInfo;

				if (Element != null && BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) &&
					BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
				{
					info.CreateComponentContentMenuItems = delegate(BXShowMode showMode)
					{
						if (showMode == BXShowMode.View)
							return new BXHermitagePopupMenuBaseItem[0];

						BXHermitagePopupMenuBaseItem[] menuItems = new BXHermitagePopupMenuBaseItem[2];

						BXHermitagePopupMenuItem editItem = new BXHermitagePopupMenuItem();
						menuItems[0] = editItem;

						editItem.Id = string.Concat("MEDIAGALLERY_ELEMENT_EDIT_", ElementId);
						editItem.Text = string.Format(GetMessageRaw("EditElementMenuItem"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
						editItem.IconCssClass = "bx-context-toolbar-edit-icon";
						editItem.ClientClickScript = string.Format(
								"jsUtils.Redirect([], '{0}?type_id={3}&iblock_id={2}&section_id={4}&id={1}&{5}={6}')",
								VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
								ElementId,
								IBlockId,
								IBlockTypeId,
								Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0,
								BXConfigurationUtility.Constants.BackUrl,
								UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery));

						editItem.Sort = 10;

						BXHermitagePopupMenuItem createItem = new BXHermitagePopupMenuItem();
						menuItems[1] = createItem;

						createItem.Id = string.Concat("MEDIAGALLERY_ELEMENT_CREATE_", IBlockId);
						createItem.Text = string.Format(GetMessageRaw("CreateElementMenuItem"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
						createItem.IconCssClass = "bx-context-toolbar-create-icon";
						createItem.ClientClickScript = string.Format(
								"jsUtils.Redirect([], '{0}?type_id={2}&iblock_id={1}&section_id={3}&{4}={5}')",
								VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
								IBlockId,
								IBlockTypeId,
								Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0,
								BXConfigurationUtility.Constants.BackUrl,
								UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery));
						createItem.Sort = 20;
						return menuItems;
					};
				}
				return info;
			}
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/playlistitem.gif";

			Group = new BXComponentGroup("media", GetMessageRaw("Group"), 100, BXComponentGroup.Content);

			ParamsDefinition.Add(BXParametersDefinition.Cache);

			ParamsDefinition.Add(
				"IBlockTypeId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockType"),
					String.Empty,
					BXCategory.Main
				)
			);

			ParamsDefinition.Add(
				"IBlockId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockCode"),
					String.Empty,
					BXCategory.Main
				)
			);

			ParamsDefinition.Add(
				"ElementId",
				new BXParamText(
					GetMessageRaw("ElementID_1"),
					"0",
					BXCategory.Main
				));

			ParamsDefinition.Add(
				"ElementCode",
				new BXParamText(
				GetMessageRaw("ElementCode"),
				String.Empty,
				BXCategory.Main
			));

			BXCategory appearance = new BXCategory(GetMessageRaw("Category.AppearanceCommon.Title"), "PLAYER_APPEARANCE", 110);

			ParamsDefinition.Add(
				"PlayerEnableFullScreenModeSwitch",
				new BXParamYesNo(
                    GetMessageRaw("PlayerEnableFullScreenModeSwitch"),
					true,
					appearance
					)
			);

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

			string groupControlPanel = "controlPanel";
			ParamsDefinition.Add(
				"PlayerShowControlPanel",
				new BXParamYesNo(
					GetMessageRaw("PlayerShowControlPanel"),
					true,
					appearance,
					new ParamClientSideActionGroupViewSwitch(ClientID, "PlayerShowControlPanel", groupControlPanel, string.Empty)
					)
			);

			ParamsDefinition.Add(
				"PlayerControlPanelBackgroundColor",
				new BXParamText(
					GetMessageRaw("PlayerControlPanelBackgroundColor"),
					"FFFFFF",
					appearance,
					new ParamClientSideActionGroupViewMember(ClientID, "PlayerControlPanelBackgroundColor", new string[] { groupControlPanel })
					)
			);

			ParamsDefinition.Add(
				"PlayerControlsColor",
				new BXParamText(
					GetMessageRaw("PlayerControlsColor"),
					"000000",
					appearance,
					new ParamClientSideActionGroupViewMember(ClientID, "PlayerControlsColor", new string[] { groupControlPanel })
					)
			);

			ParamsDefinition.Add(
				"PlayerControlsOverColor",
				new BXParamText(
					GetMessageRaw("PlayerControlsOverColor"),
					"000000",
					appearance,
					new ParamClientSideActionGroupViewMember(ClientID, "PlayerControlsOverColor", new string[] { groupControlPanel })
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

            ParamsDefinition.Add(
                "PlayerVolumeLevelInPercents",
                new BXParamText(
                    GetMessageRaw("PlayerVolumeLevelInPercents"),
                    "90",
                    appearance
                    )
            );

			ParamsDefinition.Add(
				"PlayerBufferLengthInSeconds",
				new BXParamText(
					GetMessageRaw("PlayerBufferLengthInSeconds"),
					"10",
					appearance
                    )
			);

            ParamsDefinition.Add(
                "PlayerEnableAutoStart",
                new BXParamYesNo(
                    GetMessageRaw("PlayerEnableAutoStart"),
                    false,
                    appearance
                    )
            );

            ParamsDefinition.Add(
                "PlayerEnableRepeatMode",
                new BXParamYesNo(
                    GetMessageRaw("PlayerEnableRepeatMode"),
                    false,
                    appearance
                    )
            );

			string groupDownloading = "downloading";
			ParamsDefinition.Add(
				"PlayerEnableDownloading",
				new BXParamYesNo(
					GetMessageRaw("PlayerEnableDownloading"),
					false,
					appearance,
					new ParamClientSideActionGroupViewSwitch(ClientID, "PlayerEnableDownloading", groupDownloading, string.Empty)
					)
			);

			ParamsDefinition.Add(
				"PlayerDownloadingLinkTargetWindow",
				new BXParamSingleSelection(
					GetMessageRaw("PlayerDownloadingLinkTargetWindow"),
					"_self",
					appearance,
					null,
					new ParamClientSideActionGroupViewMember(ClientID, "PlayerDownloadingLinkTargetWindow", new string[] { groupDownloading })
					)
			);

			ParamsDefinition.Add(
				"IBlockElementPropertyForFilePath",
				new BXParamSingleSelection(
					GetMessageRaw("IBlockElementPropertyForFilePath"),
					"",
					BXCategory.DataSource
				)
			);

			ParamsDefinition.Add(
				"IBlockElementPropertyForPlayerPreviewImageFilePath",
				new BXParamSingleSelection(
					GetMessageRaw("IBlockElementPropertyForPlayerPreviewImageFilePath"),
					"",
					BXCategory.DataSource
				)
			);

			ParamsDefinition.Add(
				"IBlockElementPropertyForDownloadingFilePath",
				new BXParamSingleSelection(
					GetMessageRaw("IBlockElementPropertyForDownloadingFilePath"),
					"",
					BXCategory.DataSource,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "IBlockElementPropertyForDownloadingFilePath", new string[] { groupDownloading })
				)
			);

			ParamsDefinition.Add(
				"Properties",
				new BXParamMultiSelection(
					GetMessageRaw("Properties"),
					"-",
					BXCategory.AdditionalSettings
				));

			ParamsDefinition.Add(
				"PropertyKeywords",
				new BXParamSingleSelection(
					GetMessageRaw("SetPageKeywordsFromProperty"),
					"-",
					BXCategory.AdditionalSettings
				));
			ParamsDefinition.Add(
				"PropertyDescription",
				new BXParamSingleSelection(
					GetMessageRaw("SetPageDescriptionFromProperty"),
					"-",
					BXCategory.AdditionalSettings
				));

			ParamsDefinition.Add(
				"AddAdminPanelButtons",
				new BXParamYesNo(
					GetMessageRaw("AddButtonsForThisComponentToAdminPanel"),
					false,
					BXCategory.AdditionalSettings
				));
			ParamsDefinition.Add(
				"SetPageTitle",
				new BXParamYesNo(
					GetMessageRaw("SetPageTitle"),
					true,
					BXCategory.AdditionalSettings
				));

			ParamsDefinition.Add(
				"IBlockUrl",
				new BXParamText(
					GetMessageRaw("ElementListBrowsingUrl"),
					"List.aspx",
					BXCategory.Sef
				)
			);

			ParamsDefinition.Add(
				"IBlockUrlTitle",
				new BXParamText(
					GetMessageRaw("LinkNameToElementsList"),
					GetMessageRaw("BackToNewsList"),
					BXCategory.Sef
				)
			);
		}

		protected override void LoadComponentDefinition()
		{
			List<BXParamValue> types = new List<BXParamValue>();
			types.Add(new BXParamValue(GetMessageRaw("SelectIBlockType"), "-"));
			BXIBlockTypeCollection typeCollection = BXIBlockType.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Sort, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
			foreach (BXIBlockType t in typeCollection)
				types.Add(new BXParamValue(t.Translations[BXLoc.CurrentLocale].Name, t.Id.ToString()));

			ParamsDefinition["IBlockTypeId"].Values = types;
			ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;

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

			List<BXParamValue> iblocks = new List<BXParamValue>();
			iblocks.Add(new BXParamValue(GetMessageRaw("SelectIBlockID"), "-"));

			BXIBlockCollection iblockCollection = BXIBlock.GetList(iblockFilter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Sort, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
			foreach (BXIBlock b in iblockCollection)
				iblocks.Add(new BXParamValue(b.Name, b.Id.ToString()));

			ParamsDefinition["IBlockId"].Values = iblocks;
			ParamsDefinition["IBlockId"].RefreshOnDirty = true;

			int selectedIblockId = 0;
			if (Parameters.ContainsKey("IBlockId"))
				int.TryParse(Parameters["IBlockId"], out selectedIblockId);

			List<BXParamValue> properties = new List<BXParamValue>();
			properties.Add(new BXParamValue(GetMessageRaw("NotSelected"), "-"));
			if (selectedIblockId > 0)
			{
				BXCustomFieldCollection customFields = BXIBlock.GetCustomFields(selectedIblockId);
				foreach (BXCustomField customField in customFields)
					properties.Add(new BXParamValue(BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel), customField.Name.ToUpper()));
			}

			ParamsDefinition["Properties"].Values = properties;
			ParamsDefinition["IBlockElementPropertyForFilePath"].Values = properties;
			ParamsDefinition["IBlockElementPropertyForPlayerPreviewImageFilePath"].Values = properties;
			ParamsDefinition["IBlockElementPropertyForDownloadingFilePath"].Values = properties;

			ParamsDefinition["PropertyKeywords"].Values = properties;
			ParamsDefinition["PropertyDescription"].Values = properties;

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


		public class ElementDetailProperty
		{
			BXCustomField field;
			public BXCustomField Field
			{
				set
				{
					field = value;
				}
				get
				{
					return field;
				}
			}

			BXCustomProperty property;
			public BXCustomProperty Property
			{
				set
				{
					property = value;
				}
				get
				{
					return property;
				}
			}

			private string name;
			public string Name
			{
				set
				{
					name = value;
				}
				get
				{
					return name;
				}
			}

			private string code;
			public string Code
			{
				get
				{
					return code;
				}
				set
				{
					code = value;
				}
			}

			private List<object> values;
			public List<object> Values
			{
				get
				{
					return values;
				}
				set
				{
					values = value;
				}
			}

			private string displayValue;
			public string DisplayValue
			{
				get
				{
					return displayValue;
				}
				set
				{
					displayValue = value;
				}
			}
		}
	}
	public partial class MediaGalleryElementDetailTemplate : BXComponentTemplate<MediaGalleryElementDetailComponent>
	{
		protected override void Render(HtmlTextWriter writer)
		{
			if (IsComponentDesignMode && Component.ComponentErrors != MediaGalleryElementDetailComponent.ComponentError.ErrNone)
				writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
			else
				base.Render(writer);
		}
	}
}
