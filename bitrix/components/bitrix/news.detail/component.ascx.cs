using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using Bitrix;

using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Modules;
using Bitrix.Services.Text;
using Bitrix.IO;
using Bitrix.Configuration;
using Bitrix.UI.Popup;
using Bitrix.DataTypes;
using Bitrix.UI.Hermitage;
using Bitrix.Services.Js;

public partial class NewsDetail : BXComponent
{
    BXIBlock currentIBlock;
	//PROPERTIES
	public bool UsePermissions
	{
		get { return Parameters.Get<bool>("UsePermissions"); }
	}

	public int ElementId
	{
		get { return Parameters.Get<int>("ElementId"); }
	}

	public List<string> GroupPermissions
	{
		get
		{
			List<string> result = new List<string>();
			foreach (string permission in Parameters.GetList("GroupPermissions", new List<object>()))
			{
				result.Add(permission);
			}

			return result;
		}
	}

	public string ActiveDateFormat
	{
		get { return Parameters["ActiveDateFormat"]; }
	}

	public int IBlockId
	{
		get { return Parameters.Get<int>("IBlockId"); }
	}

	public string IBlockUrl
	{
		get { return Parameters["IBlockUrl"]; }
	}

	public string IBlockUrlTitle
	{
		get { return Parameters.Get<string>("IBlockUrlTitle"); }
	}

	public bool ShowTitle
	{
		get { return Parameters.Get<bool>("ShowTitle"); }
	}

	public bool ShowDate
	{
		get { return Parameters.Get<bool>("ShowDate", true); }
	}

	public bool ShowPreviewText
	{
		get { return Parameters.Get<bool>("ShowPreviewText"); }
	}

	public bool ShowDetailPicture
	{
		get { return Parameters.Get<bool>("ShowDetailPicture"); }
	}

	public bool ShowPreviewPicture
	{
		get { return Parameters.Get<bool>("ShowPreviewPicture"); }
	}

	public string ElementName
	{
		set { ComponentCache["ElementName"] = value; }
		get
		{
			if (!ComponentCache.ContainsKey("ElementName"))
				return null;
			return ComponentCache.Get<string>("ElementName");
		}
	}

	public string IBlockName
	{
		set { ComponentCache["IBlockName"] = value; }
		get
		{
			if (!ComponentCache.ContainsKey("IBlockName"))
				return null;
			return ComponentCache.Get<string>("IBlockName");
		}
	}

	public int IBlockTypeId
	{
		set { ComponentCache["IBlockTypeID"] = value; }
		get { return ComponentCache.Get<int>("IBlockTypeID");}
	}

	public string IBlockElementName
	{
		set { ComponentCache["IBlockElementName"] = value; }
		get { return ComponentCache.Get<string>("IBlockElementName"); }
	}
	

	public List<string> PropertyCode
	{
		get
		{
			List<string> result = new List<string>();
			foreach (string permission in Parameters.GetList("PropertyCode", new List<object>()))
			{
				result.Add(permission);
			}

			return result;
		}
	}

	public BXFile PreviewImage
	{
		get { return ComponentCache.Get("PreviewImage") as BXFile; }
		set { ComponentCache["PreviewImage"] = value; }
	}

	public BXFile DetailImage
	{
		get { return ComponentCache.Get("DetailImage") as BXFile; }
		set { ComponentCache["DetailImage"] = value; }
	}

	private BXIBlockElement element;
	public BXIBlockElement Element
	{
		get { return this.element; }
		set { this.element = value; }
	}

	public string DisplayDate
	{
		get { return ComponentCache.Get<string>("DisplayDate"); }
		set { ComponentCache["DisplayDate"] = value; }
	}

	public string DetailUrl
	{
		get { return ComponentCache.Get<string>("DetailUrl"); }
		set { ComponentCache["DetailUrl"] = value; }
	}

	public Dictionary<string, Dictionary<string, string>> Properties
	{
		get { return ComponentCache.Get("Properties") as Dictionary<string, Dictionary<string, string>>; }
		set { ComponentCache["Properties"] = value; }
	}

	public string errorMessage = String.Empty;

	public string iblockElementSectionId
	{
		get 
		{ 
			//return String.Format("IBlockElementMenuSection_{0}", IBlockId); 
			return "IBlockElementMenuSection";
		}
	}

	public string iblockElementButtonMenuId
	{
		get { return String.Format("IBlockElementButtonMenu_{0}", IBlockId); }
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		bool userCanAccess = true;
		if (UsePermissions)
		{
			userCanAccess = false;

			string[] userRoles = ((BXPrincipal)Page.User).GetAllRoles(true);

			if (GroupPermissions != null)
			{
				foreach (string role in userRoles)
				{
					if (GroupPermissions.Contains(role) || role == "Admin")
					{
						userCanAccess = true;
						break;
					}
				}
			}
		}

		if (!userCanAccess)
		{
			errorMessage = GetMessageRaw("YouDontHaveRightsToBrowsingFullContent");
			IncludeComponentTemplate();
			return;
		}

		if (IsCached(((BXPrincipal)Page.User).GetAllRoles(true)))
		{
			SetTemplateCachedData();
			return;
		}

		using (BXTagCachingScope cacheScope = BeginTagCaching())
		{
			BXIBlock iblock = null;
			bool badParams = false;

			if (IBlockId <= 0)
			{
				badParams = true;
				errorMessage = GetMessage("EmptyIBlockCode");
			}
			else if (ElementId <= 0)
			{
				badParams = true;
				errorMessage = GetMessage("ElementNotFound");
			}
			else
			{
				iblock = BXIBlock.GetById(IBlockId);
				if (iblock == null)
				{
					badParams = true;
					errorMessage = GetMessage("WrongIBlockCode");
				}
			}

			BXIBlockElementCollection elementCollection = null;
			if (!badParams)
			{
				IBlockName = iblock.Name;
				IBlockTypeId = iblock.TypeId;
				IBlockElementName = iblock.ElementName;
				BXFilter elementFilter = new BXFilter(
					new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, ElementId),
					new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblock.Id),
					new BXFilterItem(BXIBlockElement.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y")
				);

				if (Parameters.Get<bool>("ShowActiveElements", true))
				{
					elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"));
					elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, "Y"));
				}

				elementCollection = BXIBlockElement.GetList(
					elementFilter,
					null,
                    new BXSelectAdd(BXIBlockElement.Fields.CustomFields[iblock.Id]),
                    null
				);

				if (elementCollection == null || elementCollection.Count == 0)
				{
					badParams = true;
					errorMessage = GetMessage("ElementNotFound");
				}
                currentIBlock = iblock;
			}

			if (badParams)
			{
				AbortCache();

				ScriptManager ajax = ScriptManager.GetCurrent(Page);
				if (!IsComponentDesignMode && BXConfigurationUtility.ShowMode == BXShowMode.View && (ajax == null || !ajax.IsInAsyncPostBack))
					BXError404Manager.Set404Status(Response);

				IncludeComponentTemplate();
				return;
			}
			
			element = elementCollection[0];

			ElementName = element.Name;

			string displayDate;
			if (element.ActiveFromDate != DateTime.MinValue)
				displayDate = element.ActiveFromDate.ToString(ActiveDateFormat);
			else
				displayDate = "";
			DisplayDate = displayDate;

			string listUrl = null;
			if (!String.IsNullOrEmpty(IBlockUrl))
			{
				string str = IBlockUrl.Replace("#SITE_DIR#", BXSite.Current.Directory);
				str = str.Replace("#IBLOCK_ID#", element.IBlockId.ToString());
				listUrl = str;
			}
			DetailUrl = listUrl;

			if (element.PreviewImageId > 0)
				PreviewImage = element.PreviewImage;
			else
				PreviewImage = null;

			if (element.DetailImageId > 0)
				DetailImage = element.DetailImage;
			else
				DetailImage = null;

			Dictionary<string, Dictionary<string, string>> dict = new BXParamsBag<Dictionary<string, string>>();
			foreach (BXCustomField field in iblock.CustomFields)
			{
				if (PropertyCode.Contains(field.Name))
				{
					Dictionary<string, string> prop = new Dictionary<string, string>();
					dict.Add(field.Name, prop);

					prop.Add("Name", field.EditFormLabel);
					BXCustomProperty property;
					if (element.CustomValues.TryGetValue(field.Name, out property))
					{
						BXCustomTypePublicView view = BXCustomTypeManager.GetCustomType(field.CustomTypeId).CreatePublicView();
						view.Init(property, field);
						
						prop.Add("Multiple", property.IsMultiple ? "Y" : "N");
						prop.Add("Value", view.GetHtml("&nbsp;/&nbsp;"));
					}
				}
			}

			if (dict.Count > 0)
				Properties = dict;
			else
				Properties = null;

			SetTemplateCachedData();
			IncludeComponentTemplate();
		} //if (!IsCached(((BXPrincipal)Page.User).GetAllRoles(true)))

	}

	private void SetTemplateCachedData()
	{
		BXPublicPage bitrixPage = Page as BXPublicPage;
		if (bitrixPage != null && !IsComponentDesignMode && !string.IsNullOrEmpty(ElementName))
		{
			bool setTitle = false;
			if ((Parameters.TryGetBool("SetTitle", out setTitle) || Parameters.TryGetBool("SetPageTitle", out setTitle)) && setTitle)
				bitrixPage.BXTitle = BXTextEncoder.HtmlTextEncoder.Decode(ElementName);
		}
	}

	//Создаем секцию и кнопку для выпадающего меню в панели администратора
	public override void CreatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
	{
		if (!Parameters.Get<bool>("DisplayPanel") || Element == null)
			return;

		if (sectionList == null)
			throw new ArgumentNullException("sectionList");

		if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
			!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
			return;

		//Создаем секцию
		BXPublicPanelMenuSection iblockElementSection = sectionList.FindByID(iblockElementSectionId);
		if (iblockElementSection == null)
		{
			BXPublicPanelMenuSectionInfo sectionInfo = new BXPublicPanelMenuSectionInfo(iblockElementSectionId, 100);
			iblockElementSection = new BXPublicPanelMenuSection(sectionInfo);
			sectionList.Add(iblockElementSection);
		}

		//Создаем кнопку
		BXPublicPanelMenu elementEditMenu = iblockElementSection.FindByID(iblockElementButtonMenuId);
		if (elementEditMenu == null)
		{
			elementEditMenu = new BXPublicPanelMenu();
			elementEditMenu.ID = iblockElementButtonMenuId;
			elementEditMenu.BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/iblock.gif");
			elementEditMenu.Caption = IBlockName;
			elementEditMenu.Hint = IBlockName;
			elementEditMenu.ShowMode = BXShowMode.NonView;
			iblockElementSection.Add(elementEditMenu);
		}
	}

	//Заполняем выпадающее меню пунктами "редактировать элемент" и "добавить элемент"
	public override void PopulatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
	{
		if (!Parameters.Get<bool>("DisplayPanel") || Element == null)
			return;

		if (sectionList == null)
			throw new ArgumentNullException("sectionList");

		if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
			!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
			return;

		BXPublicPanelMenuSection iblockElementSection = sectionList.FindByID(iblockElementSectionId);
		if (iblockElementSection == null)
			return;

		BXPublicPanelMenu elementEditMenu = iblockElementSection.FindByID(iblockElementButtonMenuId);
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
		menuItems[0].ID = String.Format("element_edit_{0}", ElementId);
		menuItems[0].Text = (currentIBlock!=null)? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.ChangeElement) 
            :String.Format(GetMessageRaw("EditElementMenuItem"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : IBlockElementName.ToLower());
		menuItems[0].ClientClickScript = 
			String.Format(
				"jsUtils.Redirect(arguments, '{0}?id={1}&iblock_id={2}&type_id={3}&{4}={5}')",
				VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
				ElementId,
				IBlockId,
				IBlockTypeId,
				BXConfigurationUtility.Constants.BackUrl,
				UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery)
			);

		menuItems[1] = new BXPopupMenuItem();
		menuItems[1].Text =(currentIBlock!=null)? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.AddElement) 
            :String.Format(GetMessageRaw("CreateElementMenuItem"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : IBlockElementName.ToLower());
		menuItems[1].ID = String.Format("element_new_{0}", IBlockId);
		menuItems[1].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/new_element.gif");
		menuItems[1].ClientClickScript =
			String.Format(
				"jsUtils.Redirect(arguments, '{0}?iblock_id={1}&type_id={2}&{3}={4}')",
				VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
				IBlockId,
				IBlockTypeId,
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

			if (BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) &&
				BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
			{
				info.CreateComponentContentMenuItems = delegate(BXShowMode showMode)
				{
					if (showMode == BXShowMode.View)
						return new BXHermitagePopupMenuBaseItem[0];

					BXHermitagePopupMenuBaseItem[] menuItems = new BXHermitagePopupMenuBaseItem[2];

					BXHermitagePopupMenuItem editItem = new BXHermitagePopupMenuItem();
					menuItems[0] = editItem;

					editItem.Id = string.Concat("NEWS_ELEMENT_EDIT_", ElementId);
					editItem.Text = (currentIBlock!=null) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.ChangeElement) 
						: string.Format(GetMessageRaw("EditElementMenuItem"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : IBlockElementName.ToLower());
					editItem.IconCssClass = "bx-context-toolbar-edit-icon";
					editItem.ClientClickScript = string.Format(
						"(new BX.CDialogNet({{ 'content_url':'{0}?id={1}&iblock_id={2}&type_id={3}&section_id={4}&lang={5}&clientType=WindowManager&mode=dlg', 'adminStyle':true, 'width':'800', 'height':'600', 'resizable':true }})).Show();",
                        BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx")),
						ElementId, 
                        IBlockId, 
                        IBlockTypeId,
						Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0,
                        BXJSUtility.Encode(HttpUtility.UrlEncode(BXLoc.CurrentLocale)));
					editItem.Sort = 10;


					BXHermitagePopupMenuItem createItem = new BXHermitagePopupMenuItem();
					menuItems[1] = createItem;

					createItem.Id = string.Concat("NEWS_ELEMENT_CREATE_", IBlockId);
					createItem.Text = (currentIBlock != null) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.AddElement)
						: string.Format(GetMessageRaw("CreateElementMenuItem"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : IBlockElementName.ToLower());
					createItem.IconCssClass = "bx-context-toolbar-create-icon";
					createItem.ClientClickScript = string.Format(
						"(new BX.CDialogNet({{ 'content_url':'{0}?iblock_id={1}&type_id={2}&section_id={3}&lang={4}&clientType=WindowManager&mode=dlg', 'adminStyle':true, 'width':'800', 'height':'600', 'resizable':true }})).Show();",
                        BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx")),
						IBlockId, 
                        IBlockTypeId,
						Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0,
                        BXJSUtility.Encode(HttpUtility.UrlEncode(BXLoc.CurrentLocale)));
					createItem.Sort = 20;

					return menuItems;
				};
			}
			return info;
		}
	}

	//Определим параметры компонента
	protected override void PreLoadComponentDefinition()
	{
		Title = GetMessage("Title");
		Description = GetMessage("Description");
		Icon = "images/news_detail.gif";

		Group = new BXComponentGroup("news", GetMessage("Group"), 20, BXComponentGroup.Content);

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

		//ElementId
		ParamsDefinition.Add(
			"ElementId",
			new BXParamText(
				GetMessageRaw("NewsID"),
				String.Empty,
				BXCategory.Main
			)
		);

		//SetTitle
		ParamsDefinition.Add(
			"SetTitle",
			new BXParamYesNo(
				GetMessageRaw("SetPageTitle"),
				false,
				BXCategory.AdditionalSettings
			)
		);

		//ShowTitle
		ParamsDefinition.Add(
			"ShowTitle",
			new BXParamYesNo(
				GetMessageRaw("DisplayPageTitle"),
				true,
				BXCategory.AdditionalSettings
			)
		);

		//ShowDetailPicture
		ParamsDefinition.Add(
			"ShowDetailPicture",
			new BXParamYesNo(
				GetMessageRaw("ShowDetailPicture"),
				true,
				BXCategory.AdditionalSettings
			)
		);

		//ShowPreviewText
		ParamsDefinition.Add(
			"ShowPreviewText",
			new BXParamYesNo(
				GetMessageRaw("ShowPreviewText"),
				false,
				BXCategory.AdditionalSettings
			)
		);

		//ShowPreviewPicture
		ParamsDefinition.Add(
			"ShowPreviewPicture",
			new BXParamYesNo(
				GetMessageRaw("ShowPreviewPicture"),
				false,
				BXCategory.AdditionalSettings
			)
		);

		//ShowActiveElements
		ParamsDefinition.Add(
			"ShowActiveElements",
			new BXParamYesNo(
				GetMessageRaw("ShowActiveElements"),
				true,
				BXCategory.AdditionalSettings
			)
		);

		//ShowDate
		ParamsDefinition.Add(
			"ShowDate",
			new BXParamYesNo(
				GetMessageRaw("ShowElementDate"),
				true,
				BXCategory.AdditionalSettings
			)
		);

		//ActiveDateFormat
		ParamsDefinition.Add(
			"ActiveDateFormat",
			new BXParamSingleSelection(
				GetMessageRaw("DateDisplayFormat"),
				"dd.MM.yyyy",
				BXCategory.AdditionalSettings
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

		//PropertyCode
		ParamsDefinition.Add(
			"PropertyCode",
			new BXParamMultiSelection(
				GetMessageRaw("Properties"),
				String.Empty,
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



		//IBlockUrl
		ParamsDefinition.Add(
			"IBlockUrl",
			new BXParamText(
				GetMessageRaw("ElementListBrowsingUrl"),
				"News.aspx?id=#IBLOCK_ID#",
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

		ParamsDefinition.Add(BXParametersDefinition.Cache);
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


		//IBlockId
		BXFilter iblockFilter = new BXFilter();
		if (Parameters.ContainsKey("IBlockTypeId"))
		{
			int typeId;
			int.TryParse(Parameters["IBlockTypeId"], out typeId);
			if (typeId > 0)
				iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, typeId));
		}
		if (!String.IsNullOrEmpty(DesignerSite))
			iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

		List<BXParamValue> iblocks = new List<BXParamValue>();
		BXIBlockCollection iblockCollection = BXIBlock.GetList(iblockFilter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Sort, BXOrderByDirection.Asc)));
		foreach (BXIBlock b in iblockCollection)
			iblocks.Add(new BXParamValue(b.Name, b.Id.ToString()));

		ParamsDefinition["IBlockId"].Values = iblocks;


		//PropertyCode
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

		//ActiveDateFormat
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

		//GroupPermissions
		List<BXParamValue> rolesList = new List<BXParamValue>();
		BXRoleCollection roles = BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc"));
		foreach (BXRole role in roles)
			rolesList.Add(new BXParamValue(role.Title, role.RoleName));

		ParamsDefinition["GroupPermissions"].Values = rolesList;

		base.LoadComponentDefinition();
	}
}

public partial class NewsDetailTemplate : BXComponentTemplate<NewsDetail>
{
	protected override void Render(HtmlTextWriter writer)
	{
		if (IsComponentDesignMode && Component.Element == null)
			writer.Write(HttpUtility.HtmlEncode(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent")));
		else
			base.Render(writer);
	}
}