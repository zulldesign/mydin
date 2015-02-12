using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Bitrix;

using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.IO;
using Bitrix.DataTypes;
using Bitrix.Services.Text;
using System.Web.UI;
using Bitrix.Configuration;
using Bitrix.UI.Popup;
using Bitrix.UI.Hermitage;
using Bitrix.UI.Components;
using Bitrix.IBlock.UI;

public partial class NewsList : BXComponent
{
	//PROPERTIES:PARAMETERS
	public int IBlockId
	{
		get { return Parameters.Get<int>("IBlockId"); }
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

	public int IBlockTypeId
	{
		get { return ComponentCache.Get<int>("IBlockTypeId"); }
		set { ComponentCache["IBlockTypeId"] = value; }
	}

	public bool IncludeSubsections
	{
		get { return Parameters.Get<bool>("IncludeSubsections"); }
	}

	public int ParentSectionId
	{
		get { return Parameters.Get<int>("ParentSectionId"); }
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

	public string SortBy1
	{
		get { return Parameters["SortBy1"]; }
	}

	public string SortBy2
	{
		get { return Parameters["SortBy2"]; }
	}

	public string SortOrder1
	{
		get { return Parameters["SortOrder1"]; }
	}

	public string SortOrder2
	{
		get { return Parameters["SortOrder2"]; }
	}

	public bool ShowTitle
	{
		get { return Parameters.Get<bool>("ShowTitle", true); }
	}

	public int PreviewTruncateLen
	{
		get { return Parameters.Get<int>("PreviewTruncateLen"); }
	}

	public bool ShowDate
	{
		get { return Parameters.Get<bool>("ShowDate", true); }
	}

	public bool ShowPreviewText
	{
		get { return Parameters.Get<bool>("ShowPreviewText", true); }
	}

	public bool ShowDetailText
	{
		get { return Parameters.Get<bool>("ShowDetailText", false); }
	}

	public bool ShowPreviewPicture
	{
		get { return Parameters.Get<bool>("ShowPreviewPicture", true); }
	}

	public string DetailUrl
	{
		get { return Parameters["DetailUrl"]; }
	}

	public int NewsCount
	{
		get { return Parameters.Get<int>("PagingRecordsPerPage"); }
	}

	public bool HideLinkWhenNoDetail
	{
		get { return Parameters.Get<bool>("HideLinkWhenNoDetail"); }
	}

	public string PagingPosition
	{
		get { return Parameters.Get<string>("PagingPosition").ToLowerInvariant(); }
	}

	//PROPERTIES:RESULTS
	protected BXIBlock iblock;
	protected List<NewsListItem> items;

	public List<NewsListItem> Items
	{
		get
		{
			return this.items;
		}
	}

	public BXIBlock IBlock
	{
		get
		{
			return this.iblock;
		}
		protected set
		{
			this.iblock = value;
		}
	}

	public bool PagingShow
	{
		get { return ComponentCache.Get<bool>("PagingShow"); }
	}

	public string[] Roles
	{
		get { return ((BXPrincipal)Page.User).GetAllRoles(true); }
	}

	public bool UsePermissions
	{
		get { return Parameters.Get<bool>("UsePermissions"); }
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

	public bool isErrorOccured = false;
	public string errorMessage = String.Empty;

	public string iblockListSectionId
	{
		get
		{
			//return String.Format("IBlockListMenuSection_{0}", IBlockId); 
			return "IBlockListMenuSection";
		}
	}

	public string iblockListButtonMenuId
	{
		get { return String.Format("IBlockListButtonMenu_{0}", IBlockId); }
	}


	//METHODS
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
			isErrorOccured = true;
			errorMessage = GetMessageRaw("YouDontHaveRightsToBrowsingFullContent");
			IncludeComponentTemplate();
			return;
		}

		BXPagingParams pagingParams = PreparePagingParams(
			Parameters.Get("PageID", HttpContext.Current.Request.QueryString["page"]),
			Parameters.Get("PageShowAll", HttpContext.Current.Request.QueryString["showall"] != null));

		if (this.IsCached(this.Roles, pagingParams))
		{
			SetTemplateCachedData();
			return;
		}

		using (BXTagCachingScope cacheScope = BeginTagCaching())
		{
			this.IBlock = null;
			Dictionary<string, string> iblockProperties = null;

			if (this.IBlockId > 0)
			{
				iblockProperties = new Dictionary<string, string>();

				BXIBlockCollection iblockCollection = BXIBlock.GetList(
					new BXFilter(
						new BXFilterItem(BXIBlock.Fields.ID, BXSqlFilterOperators.Equal, this.IBlockId),
						new BXFilterItem(BXIBlock.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlock.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y")
					),
					null
				);
				foreach (BXIBlock iblock in iblockCollection)
				{
					this.IBlock = iblock;

					BXCustomFieldCollection fieldCollection = iblock.CustomFields;
					BXParamsBag<BXCustomField> fieldDictionary = new BXParamsBag<BXCustomField>();
					foreach (BXCustomField field in fieldCollection)
						fieldDictionary.Add(field.Name, field);

					foreach (string propertyName in this.PropertyCode)
					{
						if (fieldDictionary.ContainsKey(propertyName))
							iblockProperties.Add(fieldDictionary[propertyName].Name, fieldDictionary[propertyName].EditFormLabel);
					}

					break;
				}
			}

			if (this.IBlock == null)
			{
				isErrorOccured = true;
				errorMessage = GetMessageRaw("WrongIBlockCode");
				this.AbortCache();
				IncludeComponentTemplate();
				return;
			}

			IBlockName = IBlock.Name;
			IBlockTypeId = IBlock.TypeId;
			IBlockElementName = IBlock.ElementName;

			BXFilter elementFilter = new BXFilter(
				new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, this.IBlock.Id),
				new BXFilterItem(BXIBlockElement.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y")
			);

			if (Parameters.Get<bool>("ShowActiveElements", true))
			{
				elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"));
				elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, "Y"));
			}

			if (this.ParentSectionId > 0)
			{
				elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, this.ParentSectionId));
				if (this.IncludeSubsections)
					elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.IncludeParentSections, BXSqlFilterOperators.Equal, "Y"));
			}

			BXOrderBy elementOrderBy = new BXOrderBy();
			elementOrderBy.Add(BXIBlockElement.Fields, String.Format("{0} {1}", this.SortBy1, this.SortOrder1));
			if (!this.SortBy1.Equals(this.SortBy2, StringComparison.InvariantCultureIgnoreCase))
				elementOrderBy.Add(BXIBlockElement.Fields, String.Format("{0} {1}", this.SortBy2, this.SortOrder2));
			if (!"ID".Equals(this.SortBy1, StringComparison.InvariantCultureIgnoreCase)
				&& !"ID".Equals(this.SortBy2, StringComparison.InvariantCultureIgnoreCase))
			{
				elementOrderBy.Add(BXIBlockElement.Fields.ID, BXOrderByDirection.Desc);
			}

			bool isLegalPage;
			BXQueryParams queryParams = PreparePaging(
				pagingParams,
				delegate { return BXIBlockElement.Count(elementFilter); },
				new BXParamsBag<object>(),
				"pageid",
				Parameters.Get<string>("UrlTemplatesNews", BXSefUrlManager.CurrentUrl.AbsolutePath),
				Parameters.Get<string>("UrlTemplatesNewsPage", "?page=#pageid#"),
				Parameters.Get<string>("UrlTemplatesNewsShowAll", "?showall="),
				out isLegalPage
			);

			if (!Parameters.Get<bool>("PagingAllow") && NewsCount > 0)
				queryParams = new BXQueryParams(new BXPagingOptions(0, NewsCount));
			else if (!isLegalPage)
				AbortCache();

			BXIBlockElementCollection elementCollection = BXIBlockElement.GetList(
				elementFilter,
				elementOrderBy,
				new BXSelectAdd(BXIBlockElement.Fields.CustomFields[IBlockId]),
				queryParams
				);

			this.items = new List<NewsListItem>();
			foreach (BXIBlockElement element in elementCollection)
			{
				NewsListItem item = new NewsListItem();

				item.Element = element;
				item.ElementId = element.Id;
				item.Code = element.Code;
				item.CreatedBy = element.CreatedBy;
				item.CreatedByLastName = (element.CreatedByUser != null ? element.CreatedByUser.LastName : "");
				item.CreatedByFirstName = (element.CreatedByUser != null ? element.CreatedByUser.FirstName : "");
				item.IBlockId = element.IBlockId;
				item.ModifiedBy = element.ModifiedBy;
				item.Name = element.Name;
				item.PreviewTextType = element.PreviewTextType.ToString();
				item.UpdateDate = element.UpdateDate;
				item.XmlId = element.XmlId;
				item.DetailText = !string.IsNullOrEmpty(element.DetailText);
				item.DetailTextString = element.DetailText;

				string text = element.PreviewText;
				if (PreviewTruncateLen > 0 && element.PreviewTextType != BXTextType.Html)
				{
					int endPos = PreviewTruncateLen;
					if (endPos < text.Length)
					{
						while (endPos < text.Length && !" ".Equals(text.Substring(endPos, 1), StringComparison.InvariantCultureIgnoreCase))
							endPos++;
						if (endPos < text.Length)
							text = text.Substring(0, endPos);
					}
				}
				item.PreviewText = text;

				string displayDate;
				if (element.ActiveFromDate != DateTime.MinValue)
					displayDate = element.ActiveFromDate.ToString(Parameters["ActiveDateFormat"]);
				else
					displayDate = "";
				item.DisplayDate = displayDate;

				string detailUrl = null;
				if (!String.IsNullOrEmpty(DetailUrl))
				{
					string str = DetailUrl.Replace("#SITE_DIR#", BXSite.Current.Directory);
					BXParamsBag<object> replaceItems = new BXParamsBag<object>();
					replaceItems.Add("IBLOCK_ID", this.IBlock.Id);
					replaceItems.Add("ELEMENT_ID", element.Id);
					replaceItems.Add("ElementId", element.Id);
					replaceItems.Add("Code", element.Code);
					replaceItems.Add("SectionId", (element.Sections.Count > 0) ? element.Sections[0].SectionId.ToString() : String.Empty);
					replaceItems.Add("SECTION_ID", replaceItems["SectionId"]);
					str = MakeLink(str, replaceItems);

					//Если у элемента нет секции вырезаем из ссылки параметр раздела
					str = str.Replace("//", "/");
					string paramSection = Parameters.Get<string>("ParamSection", "");
					if (element.Sections.Count < 1 && !String.IsNullOrEmpty(paramSection))
					{
						str = str.Replace(paramSection + "=&", "");
						str = str.Replace("&" + paramSection + "=", "");
					}

					detailUrl = str;
				}
				item.DetailUrl = detailUrl;

				if (element.PreviewImageId > 0)
					item.PreviewImage = element.PreviewImage;
				else
					item.PreviewImage = null;

				if (element.DetailImageId > 0)
					item.DetailImage = element.DetailImage;
				else
					item.DetailImage = null;

				if (PropertyCode.Count > 0)
				{
					Dictionary<string, string> dict = new Dictionary<string, string>();
					//BXCustomPropertyCollection propertyCollection = BXCustomEntityManager.GetProperties(BXIBlockElement.GetCustomFieldsKey(this.IBlock.Id), element.Id);
					BXCustomPropertyCollection propertyCollection = element.CustomValues;
					foreach (KeyValuePair<string, string> kvp in iblockProperties)
					{
						BXCustomProperty property;
						if (propertyCollection.TryGetValue(kvp.Key, out property))
						{
							BXCustomField field = IBlock.CustomFields[kvp.Key];
							BXCustomTypePublicView view = BXCustomTypeManager.GetCustomType(field.CustomTypeId).CreatePublicView();
							view.Init(property, field);

							dict.Add(kvp.Value, view.GetHtml("&nbsp;/&nbsp;"));
						}
					}
					item.Properties = dict;
				}
				else
				{
					item.Properties = null;
				}

				this.items.Add(item);
			}

			SetTemplateCachedData();
			IncludeComponentTemplate();
		}
	}

	private void SetTemplateCachedData()
	{
		BXPublicPage bitrixPage = Page as BXPublicPage;
		if (bitrixPage != null && !IsComponentDesignMode && !string.IsNullOrEmpty(IBlockName))
		{
			bool setTitle = false;
			if ((Parameters.TryGetBool("SetTitle", out setTitle) || Parameters.TryGetBool("SetPageTitle", out setTitle)) && setTitle)
				bitrixPage.BXTitle = BXTextEncoder.HtmlTextEncoder.Decode(IBlockName);
		}
	}

	//Создаем секцию и кнопку для выпадающего меню в панели администратора
	public override void CreatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
	{
		if (!Parameters.Get<bool>("DisplayPanel") || isErrorOccured)
			return;

		if (sectionList == null)
			throw new ArgumentNullException("sectionList");

		if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
			!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
			return;

		//Создаем секцию
		BXPublicPanelMenuSection iblockListSection = sectionList.FindByID(iblockListSectionId);
		if (iblockListSection == null)
		{
			BXPublicPanelMenuSectionInfo sectionInfo = new BXPublicPanelMenuSectionInfo(iblockListSectionId, 100);
			iblockListSection = new BXPublicPanelMenuSection(sectionInfo);
			sectionList.Add(iblockListSection);
		}

		//Создаем кнопку
		BXPublicPanelMenu listButtonMenu = iblockListSection.FindByID(iblockListButtonMenuId);
		if (listButtonMenu == null)
		{
			listButtonMenu = new BXPublicPanelMenu();
			listButtonMenu.ID = iblockListButtonMenuId;
			listButtonMenu.BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/iblock.gif");
			listButtonMenu.Caption = IBlockName;
			listButtonMenu.Hint = IBlockName;
			listButtonMenu.ShowMode = BXShowMode.NonView;
			iblockListSection.Add(listButtonMenu);
		}
	}

	//Заполняем выпадающее меню пунктами "редактировать элемент" и "добавить элемент"
	public override void PopulatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
	{
		if (!Parameters.Get<bool>("DisplayPanel") || isErrorOccured)
			return;

		if (sectionList == null)
			throw new ArgumentNullException("sectionList");

		if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
			!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
			return;

		BXPublicPanelMenuSection iblockListSection = sectionList.FindByID(iblockListSectionId);
		if (iblockListSection == null)
			return;

		BXPublicPanelMenu listButtonMenu = iblockListSection.FindByID(iblockListButtonMenuId);
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
		menuItems[0].ID = String.Format("element_new_{0}", IBlockId);
		menuItems[0].Text = (iblock != null) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(iblock.CaptionsInfo.AddElement)
			: String.Format(GetMessageRaw("AddNewElement"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : IBlockElementName.ToLower());
		menuItems[0].ClientClickScript =
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
				BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements) && !isErrorOccured)
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
					#region old
					/*
					BXHermitagePopupMenuBaseItem[] menuItems = new BXHermitagePopupMenuBaseItem[1];

					BXHermitagePopupMenuItem createItem = new BXHermitagePopupMenuItem();
					menuItems[0] = createItem;

					createItem.Id = string.Concat("NEWS_ELEMENT_CREATE_", IBlockId);
					createItem.Text = (iblock != null) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(iblock.CaptionsInfo.AddElement)
						: string.Format(GetMessageRaw("AddNewElement"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : IBlockElementName.ToLower());
					createItem.IconCssClass = "bx-context-toolbar-create-icon";

					createItem.ClientClickScript = string.Format(
						"(new BX.CDialogNet({{ 'content_url':'{0}?iblock_id={1}&type_id={2}&clientType=WindowManager&mode=dlg', 'adminStyle':true, 'width':'800', 'height':'600', 'resizable':true }})).Show();",
						VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
						IBlockId, IBlockTypeId);
					createItem.Sort = 20;
					return menuItems;
					*/
					#endregion
				};
			}
			return info;
		}
	}

	public BXHermitageToolbar CreateElementContextToolbar(BXIBlockElement element, string containerClientID)
	{
		if (element == null)
			throw new ArgumentNullException("element");

		if (!(BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) &&
			BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements) && !isErrorOccured))
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


	protected override void PreLoadComponentDefinition()
	{
		Title = GetMessage("Title");
		Description = GetMessage("Description");
		Icon = "images/news_list.gif";

		Group = new BXComponentGroup("news", GetMessage("Group"), 20, BXComponentGroup.Content);

		ParamsDefinition.Add(BXParametersDefinition.Cache);

		BXCategory listSettingsGroup = BXCategory.ListSettings;
		listSettingsGroup.Sort = 110;

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

		//NewsCount
		ParamsDefinition.Add(
			"PagingRecordsPerPage",
			new BXParamText(
				GetMessageRaw("NewsPerPage"),
				"20",
				BXCategory.Main
				)
			);

		//SortBy1
		ParamsDefinition.Add(
			"SortBy1",
			new BXParamSingleSelection(
				GetMessageRaw("FirstSortBy"),
				"ActiveFromDate",
				BXCategory.DataSource
				)
			);

		//SortBy2
		ParamsDefinition.Add(
			"SortBy2",
			new BXParamSingleSelection(
				GetMessageRaw("SecondsortBy"),
				"Sort",
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

		//DetailUrl
		ParamsDefinition.Add(
			"DetailUrl",
			new BXParamText(
				GetMessageRaw("DetailPageUrl"),
				"NewsDetail.aspx?id=#ELEMENT_ID#",
				BXCategory.Sef
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

		//ShowActiveElements
		ParamsDefinition.Add(
			"ShowActiveElements",
			new BXParamYesNo(
				GetMessageRaw("ShowActiveElements"),
				true,
				BXCategory.AdditionalSettings
			)
		);

		//ParentSectionId
		ParamsDefinition.Add(
			"ParentSectionId",
			new BXParamText(
				GetMessageRaw("SectionID"),
				"0",
				BXCategory.AdditionalSettings
				)
			);

		//IncludeSubsections
		ParamsDefinition.Add(
			"IncludeSubsections",
			new BXParamYesNo(
				GetMessageRaw("IncludeSubsections"),
				true,
				BXCategory.AdditionalSettings
				)
			);

		//ShowPreviewText
		ParamsDefinition.Add(
			"ShowPreviewText",
			new BXParamYesNo(
				GetMessageRaw("ShowPreviewText"),
				true,
				listSettingsGroup
			)
		);

		//ShowDetailText
		ParamsDefinition.Add(
			"ShowDetailText",
			new BXParamYesNo(
				GetMessageRaw("ShowDetailText"),
				false,
				listSettingsGroup
			)
		);

		//ShowPreviewPicture
		ParamsDefinition.Add(
			"ShowPreviewPicture",
			new BXParamYesNo(
				GetMessageRaw("ShowPreviewPicture"),
				true,
				listSettingsGroup
			)
		);

		//PreviewTruncateLen
		ParamsDefinition.Add(
			"PreviewTruncateLen",
			new BXParamText(
				GetMessageRaw("MaxOutputLengthForAnnouncement"),
				String.Empty,
				listSettingsGroup
				)
			);

		//ShowTitle
		ParamsDefinition.Add(
			"ShowTitle",
			new BXParamYesNo(
				GetMessageRaw("DisplayPageTitle"),
				true,
				listSettingsGroup
			)
		);

		//SetTitle
		ParamsDefinition.Add(
			"SetTitle",
			new BXParamYesNo(
				GetMessageRaw("SetPageTitle"),
				false,
				listSettingsGroup
				)
			);

		//ShowDate
		ParamsDefinition.Add(
			"ShowDate",
			new BXParamYesNo(
				GetMessageRaw("ShowElementDate"),
				true,
				listSettingsGroup
			)
		);

		//ActiveDateFormat
		ParamsDefinition.Add(
			"ActiveDateFormat",
			new BXParamSingleSelection(
				GetMessageRaw("DateDisplayFormat"),
				"dd.MM.yyyy",
				listSettingsGroup
				)
			);

		//HideLinkWhenNoDetail
		ParamsDefinition.Add(
			"HideLinkWhenNoDetail",
			new BXParamYesNo(
				GetMessageRaw("HideLinkIfDetailedDescriptionIsNotExist"),
				false,
				listSettingsGroup
				)
			);

		//PropertyCode
		ParamsDefinition.Add(
			"PropertyCode",
			new BXParamMultiSelection(
				GetMessageRaw("Properties"),
				String.Empty,
				listSettingsGroup
				)
			);

		BXParamsBag<BXParam> paging = BXParametersDefinition.Paging;
		paging.Remove("PagingRecordsPerPage");
		ParamsDefinition.Add(paging);
	}

	protected override void LoadComponentDefinition()
	{
		//IBlockTypeId
		List<BXParamValue> types = new List<BXParamValue>();
		types.Add(new BXParamValue("-", ""));
		BXIBlockTypeCollection typeCollection = BXIBlockType.GetList(
			null,
			new BXOrderBy(
				new BXOrderByPair(BXIBlockType.Fields.Sort, BXOrderByDirection.Asc)
			)
		);
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
		BXIBlockCollection iblockCollection = BXIBlock.GetList(
			iblockFilter,
			new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Sort, BXOrderByDirection.Asc))
		);
		foreach (BXIBlock b in iblockCollection)
			iblocks.Add(new BXParamValue(b.Name, b.Id.ToString()));

		ParamsDefinition["IBlockId"].Values = iblocks;


		//SortBy1 SortBy2
		List<BXParamValue> sortFields = new List<BXParamValue>();
		sortFields.Add(new BXParamValue("ID", "ID"));
		sortFields.Add(new BXParamValue(GetMessageRaw("NewsName"), "Name"));
		sortFields.Add(new BXParamValue(GetMessageRaw("ActiveFromDate"), "ActiveFromDate"));
		sortFields.Add(new BXParamValue(GetMessageRaw("Sort"), "Sort"));
		sortFields.Add(new BXParamValue(GetMessageRaw("DateOfLastModification"), "UpdateDate"));

		ParamsDefinition["SortBy1"].Values = sortFields;
		ParamsDefinition["SortBy2"].Values = sortFields;

		List<BXParamValue> properties = new List<BXParamValue>();
		if (Parameters.ContainsKey("IBlockId"))
		{
			int iblockId;
			int.TryParse(Parameters["IBlockId"], out iblockId);
			if (iblockId > 0)
			{
				BXCustomFieldCollection cfCollection =
					BXCustomEntityManager.GetFields(String.Format("IBlock_{0}", iblockId));
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

public class NewsListItem
{
	private string code;
	private int createdBy;
	private string createdByFirstName;
	private string createdByLastName;
	private BXFile detailImage;
	private bool detailText;
	private string detailTextString;
	private string detailUrl;
	private string displayDate;
	private int elementId;
	private int iBlockId;
	private int modifiedBy;
	private string name;
	private BXFile previewImage;
	private string previewText;
	private string previewTextType;
	private Dictionary<string, string> properties;
	private DateTime updateDate;
	private string xmlId;
	private BXIBlockElement element;

	public int ElementId
	{
		get { return elementId; }
		set { elementId = value; }
	}

	public string Code
	{
		get { return code; }
		set { code = value; }
	}

	public int CreatedBy
	{
		get { return createdBy; }
		set { createdBy = value; }
	}

	public string CreatedByLastName
	{
		get { return createdByLastName; }
		set { createdByLastName = value; }
	}

	public string CreatedByFirstName
	{
		get { return createdByFirstName; }
		set { createdByFirstName = value; }
	}

	public int IBlockId
	{
		get { return iBlockId; }
		set { iBlockId = value; }
	}

	public int ModifiedBy
	{
		get { return modifiedBy; }
		set { modifiedBy = value; }
	}

	public string Name
	{
		get { return name; }
		set { name = value; }
	}

	public string PreviewTextType
	{
		get { return previewTextType; }
		set { previewTextType = value; }
	}

	public DateTime UpdateDate
	{
		get { return updateDate; }
		set { updateDate = value; }
	}

	public string XmlId
	{
		get { return xmlId; }
		set { xmlId = value; }
	}

	public bool DetailText
	{
		get { return detailText; }
		set { detailText = value; }
	}

	public string DetailTextString
	{
		get { return detailTextString; }
		set { detailTextString = value; }
	}

	public string PreviewText
	{
		get { return previewText; }
		set { previewText = value; }
	}

	public string DisplayDate
	{
		get { return displayDate; }
		set { displayDate = value; }
	}

	public string DetailUrl
	{
		get { return detailUrl; }
		set { detailUrl = value; }
	}

	public BXFile PreviewImage
	{
		get { return previewImage; }
		set { previewImage = value; }
	}

	public BXFile DetailImage
	{
		get { return detailImage; }
		set { detailImage = value; }
	}

	public Dictionary<string, string> Properties
	{
		get { return properties; }
		set { properties = value; }
	}

	public BXIBlockElement Element
	{
		get
		{
			return element;
		}
		set
		{
			element = value;
		}
	}
}

public partial class NewsListTemplate : BXComponentTemplate<NewsList>
{
	protected override void Render(HtmlTextWriter writer)
	{
		StartWidth = "100%";
		if (IsComponentDesignMode && Component.Items == null)
			writer.Write(HttpUtility.HtmlEncode(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent")));
		else
			base.Render(writer);
	}

	protected string GetItemContainerClientID(int itemId)
	{
		return string.Concat(ClientID, ClientIDSeparator, "NewsItem", itemId.ToString());
	}

	public void RenderElementToolbar(BXIBlockElement element, string containerClientID)
	{
		Component.CreateElementContextToolbar(element, containerClientID).Render(CurrentWriter);
	}
}
