using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Specialized;
using System.Net;

using Bitrix;
using Bitrix.Components;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IBlock;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.UI.Popup;
using Bitrix.Components.Editor;
using Bitrix.UI.Hermitage;
using Bitrix.Services.Js;
using Bitrix.IBlock.UI;
using Bitrix.UI.Components;
using System.Runtime.Serialization;



namespace Bitrix.IBlock.Components
{
	public partial class CatalogueElementDetailComponent : BXComponent
	{
		private BXIBlockElement element;
		private BXIBlock currentIBlock;

		public BXIBlockElement Element
		{
			get { return this.element; }
			set { this.element = value; }
		}

		public int ElementId
		{
			get
			{
				object rObj = null;
				if (ComponentCache.TryGetValue("ElementId", out rObj))
					return (int)rObj;

				int id = Parameters.GetInt("ElementId", 0);
				ComponentCache["ElementId"] = id;
				return id;
			}

			internal set
			{
				ComponentCache["ElementId"] = value;
			}

		}

		public string ElementCode
		{
			get { return Parameters.Get<string>("ElementCode", String.Empty); }
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

		public int IBlockId
		{
			get
			{
				return Parameters.Get("IBlockId", 0);
			}
		}

		private List<string> showProperties = null;
		public List<string> ShowProperties
		{
			get { return this.showProperties ?? (this.showProperties = Parameters.GetListString("Properties", new List<string>())); }
		}

		private List<string> showCatalogItemProperties = null;
		public List<string> ShowCatalogItemProperties
		{
			get { return this.showCatalogItemProperties ?? (this.showCatalogItemProperties = Parameters.GetListString("ShowCatalogItemProperties", new List<string>())); }
		}

		public string PropertyKeywordsCode
		{
			get
			{
				return Parameters.Get<string>("PropertyKeywords", "-");
			}
		}

		public string PropertyKeywordsValue
		{
			set { ComponentCache["PropertyKeywords"] = value; }
			get
			{
				if (!ComponentCache.ContainsKey("PropertyKeywords"))
					return null;
				return ComponentCache.Get<string>("PropertyKeywords");
			}
		}

		public string PropertyDescriptionCode
		{
			get
			{
				return Parameters.Get<string>("PropertyDescription", "-");
			}
		}

		public string PropertyDescriptionValue
		{
			set { ComponentCache["PropertyDescription"] = value; }
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
			get { return properties; }
			set { properties = value; }
		}

		public bool DisplayStockCatalogData
		{
			get { return Parameters.GetBool("DisplayStockCatalogData", false); }
		}

		public bool IncludeVATInPrice
		{
			get { return Parameters.GetBool("IncludeVATInPrice", true); }
		}

		public bool DisplayVAT
		{
			get { return Parameters.GetBool("DisplayVAT", false); }
		}

		public int InitQuantity
		{
			get
			{
				int r = Parameters.GetInt("InitQuantity", 1);
				return r > 0 ? r : 1;
			}
		}

		public bool DisplayAllPriceTiers
		{
			get { return Parameters.GetBool("DisplayAllPriceTiers", false); }
		}

		public bool AcceptQuantity
		{
			get { return Parameters.GetBool("AcceptQuantity", false); }
		}

		public string UserCartUrlTemplate
		{
			get { return Parameters.GetString("UserCartUrlTemplate", "personal/cart.aspx"); }
		}

		private string userCartUrl = null;
		public string UserCartUrl
		{
			get
			{
				if (this.userCartUrl != null)
					return this.userCartUrl;

				BXParamsBag<object> pb = new BXParamsBag<object>();
				pb.Add("UserId", UserId);
				return this.userCartUrl = ResolveTemplateUrl(UserCartUrlTemplate, pb);
			}
		}

		public string ElementDetailUrl
		{
			get { return Parameters.Get<string>("ElementDetailUrl", "detail.aspx?section_id=#SECTION_ID#&element_id=#ELEMENT_ID#"); }
		}

		private int UserId
		{
			get
			{
				BXIdentity identity = (BXIdentity)BXPrincipal.Current.Identity;
				return identity != null ? identity.Id : 0;
			}
		}

		private List<ElementDetailProperty> catalogItemProperties = null;
		public List<ElementDetailProperty> CatalogItemProperties
		{
			get { return this.catalogItemProperties ?? (this.catalogItemProperties = new List<ElementDetailProperty>()); }
			set { this.catalogItemProperties = value; }
		}

		private int[] priceTypes = null;
		public int[] PriceTypes
		{
			get
			{
				if (this.priceTypes != null)
					return priceTypes;

				IList<int> lst = Parameters.GetListInt("PriceTypes", null);
				this.priceTypes = new int[lst != null ? lst.Count : 0];
				for (int i = 0; i < this.priceTypes.Length; i++)
					this.priceTypes[i] = lst[i];

				return this.priceTypes;
			}
		}



		private Dictionary<int, CatalogDiscountInfo> discountInfos;
		private List<int> elementSections;

		public CatalogDiscountInfo GetDiscountByPriceInfo(CatalogClientPriceInfo info)
		{
			if (!IsCatalogModuleInstalled || Element == null || CurrentSellingPrice == null)
				return null;
			if (elementSections == null)
			{
				var col = Element.GetSections();
				if (col == null)
					elementSections = new List<int>();
				else
					elementSections = col.ConvertAll<int>(x => x.Id);
			}

			int priceTypeId = info != null ? info.PriceTypeId : CurrentSellingPrice.PriceTypeId;
			decimal priceValue = info != null ? info.PriceValue : CurrentSellingPrice.PriceValue;
			int currencyId = info != null ? info.CurrencyId : CurrentSellingPrice.CurrencyId;

			CatalogDiscountInfo discount = null;
			if (Catalog != null)
			{
				if (CurrentSku != null)
					discount = Catalog.GetDiscount(0,
							BXPrincipal.Current.GetAllRoleIds(true),
							CurrentSku.Id,
							priceTypeId,
							priceValue,
							InitQuantity,
							elementSections,
							BXSite.Current.Id,
							currencyId
						);
				else
					discount = Catalog.GetDiscount(0,
							BXPrincipal.Current.GetAllRoleIds(true),
							Element.Id,
							priceTypeId,
							priceValue,
							InitQuantity,
							elementSections,
							BXSite.Current.Id,
							currencyId
						);
			}
			return discount;
		}


		private CatalogClientPriceInfoSet clientPriceInfoSet = null;
		public CatalogClientPriceInfoSet ClientPriceInfoSet
		{
			get { return this.clientPriceInfoSet; }
			private set { this.clientPriceInfoSet = value; }
		}

		public CatalogClientPriceInfo CurrentSellingPrice
		{
			get { return ClientPriceInfoSet != null ? ClientPriceInfoSet.CurrentSellingPrice : null; }
		}

		public bool IsSellingAllowed
		{
			get { return CurrentSellingPrice != null; }
		}

		public bool IsInCart
		{
			get { return SaleCart != null && SaleCart.IsExists(CurrentSkuId > 0 ? CurrentSkuId : ElementId, Request, Response); }
		}

		public int QuantityInCart
		{
			get { return SaleCart != null ? SaleCart.GetQuantity(CurrentSkuId > 0 ? CurrentSkuId : ElementId, Request, Response) : 0; }
		}

		private CatalogueElementDetailComponent.CatalogSKUItem[] skuItems = null;
		public CatalogueElementDetailComponent.CatalogSKUItem[] SkuItems
		{
			get { return this.skuItems ?? (this.skuItems = new CatalogueElementDetailComponent.CatalogSKUItem[0]); }
			private set { this.skuItems = value; }
		}

		private int currentSkuId = 0;
		public int CurrentSkuId
		{
			get { return this.currentSkuId; }
			private set { this.currentSkuId = value; }
		}

		private CatalogSKUItem currentSku = null;
		public CatalogSKUItem CurrentSku
		{
			get { return this.currentSku; }
			private set { this.currentSku = value; }
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
			get { return ComponentCache.Get<int>("IBlockTypeID"); }
		}

		public string IBlockElementName
		{
			set { ComponentCache["IBlockElementName"] = value; }
			get { return ComponentCache.Get<string>("IBlockElementName"); }
		}

		public string iblockElementSectionId
		{
			get { return "IBlockElementMenuSection"; }
		}

		public string iblockElementButtonMenuId
		{
			get { return String.Format("IBlockElementButtonMenu_{0}", IBlockId); }
		}

		public bool EnableVotingForElement
		{
			get
			{
				return Parameters.GetBool("EnableVotingForElement", false);
			}
			set
			{
				Parameters["EnableVotingForElement"] = value.ToString();
			}
		}
		private IList<string> rolesAuthorizedToVote = null;
		public IList<string> RolesAuthorizedToVote
		{
			get
			{
				return this.rolesAuthorizedToVote ?? (this.rolesAuthorizedToVote = Parameters.GetListString("RolesAuthorizedToVote"));
			}
			set
			{
				Parameters["RolesAuthorizedToVote"] = BXStringUtility.ListToCsv(this.rolesAuthorizedToVote = value ?? new List<string>());
			}
		}

		public string ActionParamName
		{
			get { return Parameters.GetString("ActionParamName", "act"); }
		}

		public string CatalogItemIdParamName
		{
			get { return Parameters.GetString("CatalogItemIdParamName", "id"); }
		}

		public string CatalogItemQuantityParamName
		{
			get { return Parameters.GetString("CatalogItemQuantityParamName", "qty"); }
		}

		public string GetAddToCartUrl(bool jsonResponse)
		{
			NameValueCollection qsParams = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
			qsParams[BXCsrfToken.TokenKey] = BXCsrfToken.GenerateToken();
			qsParams[ActionParamName] = "ADD2CART";
			qsParams[CatalogItemIdParamName] = (CurrentSkuId > 0 ? CurrentSkuId : ElementId).ToString();
			qsParams["detailUrl"] = BXSefUrlManager.CurrentUrl.PathAndQuery;

			if (jsonResponse)
				qsParams["responseType"] = "JSON";

			string path = BXSefUrlManager.CurrentUrl.PathAndQuery;
			int whatInd = path.IndexOf('?');
			if (whatInd >= 0)
				path = path.Substring(0, whatInd);

			return string.Concat(path, "?", qsParams.ToString());
		}

		public string GetSelectSkuUrl(string skuId)
		{
			NameValueCollection qsParams = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
			qsParams["skuId"] = skuId;

			string path = BXSefUrlManager.CurrentUrl.PathAndQuery;
			int whatInd = path.IndexOf('?');
			if (whatInd >= 0)
				path = path.Substring(0, whatInd);

			return string.Concat(path, "?", qsParams.ToString());
		}

		public string errorMessage = String.Empty;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			int elementId;
			if (CanModifyElement
				&& string.Equals(Request["elementAction"], "delete", StringComparison.OrdinalIgnoreCase)
				&& !string.IsNullOrEmpty(Request["elementId"])
				&& int.TryParse(Request["elementId"], out elementId)
				&& elementId == ElementId
				&& BXCsrfToken.CheckToken(Request[BXCsrfToken.TokenKey]))
			{
				try
				{
					BXIBlockElement.Delete(elementId);
				}
				catch (Exception)
				{
				}

				/*
				Uri url = BXSefUrlManager.CurrentUrl;
				NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
				qsParams.Remove("elementAction");
				qsParams.Remove("elementId");
				qsParams.Remove(BXCsrfToken.TokenKey);
				Response.Redirect(qsParams.Count > 0 ? string.Concat(url.AbsolutePath, "?", qsParams.ToString()) : url.AbsolutePath, true);
				*/

				Response.Redirect(GetElementPostDeleteUrl(), true);
			}

			if (DisplayStockCatalogData)
				int.TryParse(Request["skuId"] ?? string.Empty, out this.currentSkuId);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DisplayStockCatalogData && PriceTypes.Length > 0 && Catalog != null)
			{
				SkuItems = Catalog.GetSKUItems(ElementId, IBlockId);
				if (CurrentSkuId <= 0
					|| (CurrentSku = Array.Find<CatalogSKUItem>(SkuItems, delegate(CatalogSKUItem sku) { return sku.Id == CurrentSkuId; })) == null)
					CurrentSkuId = 0;

				if (CurrentSkuId == 0 && SkuItems.Length > 0)
					CurrentSkuId = (CurrentSku = SkuItems[0]).Id;

				if (CurrentSku != null) //вывод цен SKU если он выбран
					ClientPriceInfoSet = CurrentSku.GetClientPriceInfoSet(InitQuantity, DisplayAllPriceTiers, ((BXIdentity)BXPrincipal.Current.Identity).Id, 0, IncludeVATInPrice, PriceTypes);
				else
					ClientPriceInfoSet = Catalog.GetClientPriceSet(ElementId, InitQuantity, DisplayAllPriceTiers, ((BXIdentity)BXPrincipal.Current.Identity).Id, 0, IncludeVATInPrice, PriceTypes);

				if(ClientPriceInfoSet != null)
					foreach(CatalogClientPriceInfo priceInfo in ClientPriceInfoSet.Items)
					{
						CatalogDiscountInfo discountInfo = GetDiscountByPriceInfo(priceInfo);
						if(discountInfo != null && discountInfo.HasInfo)
							priceInfo.DiscountHtml = discountInfo.DisplayHtml;
					}

			}

			(new SaleCartActionHandler(this)).Process();

            if (IsCached(EnableVotingForElement ? (object)BXPrincipal.Current.Identity.Name : (object)BXPrincipal.Current.GetAllRoles(true)))
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
				else if (ElementId <= 0 && String.IsNullOrEmpty(ElementCode))
				{
					badParams = true;
					errorMessage = GetMessage("ElementNotFound");
				}
				else
				{
					iblock = BXIBlock.GetById(IBlockId, BXTextEncoder.HtmlTextEncoder);
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
					{
						badParams = true;
						errorMessage = GetMessage("ElementNotFound");
					}
				}

				if (badParams)
				{
					AbortCache();

					ScriptManager ajax = ScriptManager.GetCurrent(Page);
					if (!IsComponentDesignMode && BXConfigurationUtility.ShowMode == BXShowMode.View && (ajax == null || !ajax.IsInAsyncPostBack))
					{
						BXError404Manager.Set404Status(Response);
						BXPublicPage bitrixPage = Page as BXPublicPage;
						if (bitrixPage != null && Parameters.GetBool("SetPageTitle", true) && !String.IsNullOrEmpty(errorMessage))
							bitrixPage.Title = errorMessage;
					}

					IncludeComponentTemplate();
					return;
				}

				this.element = elementCollection[0];
				ComponentCache["CustomPropertyEntityId"] = BXIBlockElement.GetCustomFieldsKey(this.element.IBlockId);
				ComponentCache["UsersBannedToVote"] = this.element.CreatedBy.ToString();

				ElementId = element.Id;
				ElementName = element.Name;
				PreviewImage = element.PreviewImageId > 0 ? element.PreviewImage : null;
				DetailImage = element.DetailImageId > 0 ? element.DetailImage : null;
				currentIBlock = iblock;

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
					property.DisplayValue = GetCustomProperty(property.Property, property.Field, "&nbsp;/&nbsp;");
					elementProperties.Add(property);

					if (string.Equals(PropertyKeywordsCode, elementProperty.Key, StringComparison.InvariantCultureIgnoreCase))
						PropertyKeywordsValue = GetCustomProperty(property.Property, property.Field, ", ");

					if (string.Equals(PropertyDescriptionCode, elementProperty.Key, StringComparison.InvariantCultureIgnoreCase))
						PropertyDescriptionValue = GetCustomProperty(property.Property, property.Field, ", ");
				}

				Properties = elementProperties;

				//Торговый каталог (отображается когда выбран тип цен)
				if (DisplayStockCatalogData && PriceTypes.Length > 0 && Catalog != null)
				{
					this.catalogItemProperties = new List<ElementDetailProperty>();
					foreach (BXCustomField catalogItemField in CatalogItemCustomFields)
					{
						//int dashInd = name.IndexOf("-", StringComparison.Ordinal);
						//if(dashInd >= 0)
						//{
						//    int blockId;
						//    if(!int.TryParse(name.Substring(0, dashInd), out blockId) || blockId != this.element.IBlockId)				
						//        return null;

						//    name = name.Substring(dashInd + 1);
						//}

						BXCustomProperty cp;
						if (!element.CustomPublicValues.TryGetCustomProperty(catalogItemField.Name, out cp))
							continue;

						string code = cp.Name.ToUpperInvariant();

						int index = ShowProperties.FindIndex(delegate(string s) { return string.Equals(s, code, StringComparison.Ordinal); });
						if (index < 0)
							continue;

						ShowProperties.RemoveAt(index);

						if (!ShowCatalogItemProperties.Contains(code))
							continue;

						ElementDetailProperty p = new ElementDetailProperty();
						p.Property = cp;
						p.Code = code;
						p.Name = cp.UserLikeName;
						p.Field = catalogItemField;
						p.Values = cp.Values;
						p.DisplayValue = cp.ToHtml(string.Empty, "&nbsp;/&nbsp;");

						this.catalogItemProperties.Add(p);
					}

					CatalogueElementDetailComponent.CatalogSKUItem propertySourceItem = CurrentSku ?? Catalog.GetItem(ElementId, IBlockId);
					if (propertySourceItem != null && propertySourceItem.Element != null)
						foreach (string itemProperty in ShowCatalogItemProperties)
						{
							int dashInd = itemProperty.IndexOf("-", StringComparison.Ordinal);
							if (dashInd < 0)
								continue;

							int blockId;
							if (!int.TryParse(itemProperty.Substring(0, dashInd), out blockId) || blockId != propertySourceItem.BlockId)
								continue;

							BXCustomProperty cp;
							propertySourceItem.Element.CustomPublicValues.TryGetCustomProperty(itemProperty.Substring(dashInd + 1), out cp);
							if (cp == null)
								continue;

							ElementDetailProperty p = new ElementDetailProperty();
							p.Property = cp;
							p.Code = cp.Name.ToUpperInvariant();
							p.Name = cp.UserLikeName;
							p.Field = cp.Field;
							p.Values = cp.Values;
							p.DisplayValue = cp.ToHtml("-", "&nbsp;/&nbsp;");

							this.catalogItemProperties.Add(p);
						}
				}

				SetTemplateCachedData();
				IncludeComponentTemplate();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			CatalogueElementDetailTemplate.PrepareEnvironment();
		}

		//public SaleCartItem[] GetSaleCartItems()
		//{
		//    return SaleCart != null ? SaleCart.GetSaleCartItems(Request, Response) : new SaleCartItem[0];
		//}

		private bool AddToCart(int qty, string detailUrl, out int cartItemCount)
		{
			cartItemCount = 0;
			if (!IsSellingAllowed || SaleCart == null)
				return false;

			int id = CurrentSkuId > 0 ? CurrentSkuId : ElementId;
			if (!SaleCart.IsExists(id, Request, Response))
				SaleCart.Add(id, AcceptQuantity ? qty : 1, detailUrl, Request, Response);

			cartItemCount = SaleCart.Count();
			return true;
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

		private string GetCustomProperty(BXCustomProperty property, BXCustomField field, string separator)
		{
			BXCustomTypePublicView view = BXCustomTypeManager.GetCustomType(field.CustomTypeId).CreatePublicView();
			view.Init(property, field);
			return view.GetHtml(separator);
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
			menuItems[0].Text = (currentIBlock != null)
				? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.ChangeElement)
				: String.Format(GetMessageRaw("EditElementMenuItem"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
			menuItems[0].ClientClickScript = String.Format(
				"jsUtils.Redirect([], '{0}?type_id={2}&iblock_id={1}&section_id={3}&{4}={5}')",
				VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
				IBlockId,
				IBlockTypeId,
				Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0,
				BXConfigurationUtility.Constants.BackUrl,
				UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery));

			menuItems[1] = new BXPopupMenuItem();
			menuItems[1].Text = (currentIBlock != null)
				? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.AddElement)
				: String.Format(GetMessageRaw("CreateElementMenuItem"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
			menuItems[1].ID = String.Format("element_new_{0}", IBlockId);
			menuItems[1].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/new_element.gif");
			menuItems[1].ClientClickScript = String.Format(
				"jsUtils.Redirect([], '{0}?type_id={2}&iblock_id={1}&section_id={3}&{4}={5}')",
				VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
				IBlockId,
				IBlockTypeId,
				Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0,
				BXConfigurationUtility.Constants.BackUrl,
				UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery));

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
				if (BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) 
					&& BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements)
					&& string.IsNullOrEmpty(this.errorMessage))
					info.CreateComponentContentMenuItems =
						delegate(BXShowMode showMode)
						{
							if (showMode == BXShowMode.View)
								return new BXHermitagePopupMenuBaseItem[0];

							BXHermitageToolbarSettings settings = new BXHermitageToolbarSettings();
							settings.Data.Add("ElementId", ElementId);
							settings.Data.Add("IBlockId", IBlockId);
							settings.Data.Add("IBlockTypeId", IBlockTypeId);

							if (element != null)
								settings.Data.Add("Element", element);

							if (currentIBlock != null)
								settings.Data.Add("IBlock", currentIBlock);

							settings.Data.Add("ChangeElement",
								string.Format(GetMessageRaw("EditElementMenuItem"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower())));

							settings.Data.Add("AddElement",
								String.Format(GetMessageRaw("CreateElementMenuItem"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower())));

							settings.Data.Add("DeleteElement",
								string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));

							settings.Data.Add("ElementDetailUrl", ElementDetailUrl);
							settings.Data.Add("ElementPostDeleteUrl", GetElementPostDeleteUrl());

							settings.Data.Add("ElementDeletionConfirmation",
								string.Format(GetMessageRaw("ElementDeletionConfirmation"),
								(currentIBlock != null) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.ElementName.ToLower())
								: string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower())));

							return BXHermitageToolbarHelper.CreateToolbar(BXHermitageMenuCommand.CreateElement | BXHermitageMenuCommand.ModifyElement | BXHermitageMenuCommand.DeleteElement, settings).ItemsToArray();
							#region old
							/*
							BXHermitagePopupMenuBaseItem[] menuItems = new BXHermitagePopupMenuBaseItem[3];
							BXHermitagePopupMenuItem editItem = new BXHermitagePopupMenuItem();
							menuItems[0] = editItem;
							editItem.Id = string.Concat("CATALOGUE_ELEMENT_EDIT_", ElementId);
							editItem.IconCssClass = "bx-context-toolbar-edit-icon";
							editItem.Sort = 10;
							editItem.Text = (currentIBlock != null)
								? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.ChangeElement)
								: string.Format(GetMessageRaw("EditElementMenuItem"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
							editItem.ClientClickScript =
								String.Format(
									"(new BX.CDialogNet({{ 'content_url':'{0}?id={1}&iblock_id={2}&type_id={3}&section_id={4}&clientType=WindowManager&mode=dlg', 'adminStyle':true, 'width':'800', 'height':'600', 'resizable':true }})).Show();",
									VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
									ElementId, IBlockId, IBlockTypeId,
									Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0);
							

							BXHermitagePopupMenuItem createItem = new BXHermitagePopupMenuItem();
							menuItems[1] = createItem;
							createItem.Id = string.Concat("CATALOGUE_ELEMENT_CREATE_", IBlockId);
							createItem.IconCssClass = "bx-context-toolbar-create-icon";
							createItem.Sort = 20;
							createItem.Text = (currentIBlock!=null) 
								? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.AddElement) 
								: String.Format(GetMessageRaw("CreateElementMenuItem"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
							createItem.ClientClickScript = String.Format(
								"(new BX.CDialogNet({{ 'content_url':'{0}?iblock_id={1}&type_id={2}&section_id={3}&{4}={5}&clientType=WindowManager&mode=dlg', 'adminStyle':true, 'width':'800', 'height':'600', 'resizable':true }})).Show();",
								VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"),
								IBlockId, IBlockTypeId,
								Element != null && Element.Sections.Count > 0 ? Element.Sections[0].SectionId : 0,
								BXConfigurationUtility.Constants.BackUrl,
								UrlEncode(ElementDetailUrl));
							 
							BXHermitagePopupMenuItem deleteItem = new BXHermitagePopupMenuItem();
							menuItems[2] = deleteItem;
							deleteItem.Id = string.Concat("CATALOGUE_ELEMENT_DELETE_", ElementId);;
							deleteItem.IconCssClass = "bx-context-toolbar-delete-icon";
							deleteItem.Text = (currentIBlock != null)
								? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.DeleteElement)
								: string.Format(GetMessageRaw("DeleteElementMenuItem"), string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
							deleteItem.Sort = 30;
							deleteItem.ClientClickScript = string.Format(
								"if(window.confirm('{0}')){{ BX.ajax({{ 'method':'POST', 'dataType':'JSON', 'url':'{1}', 'data':{{ 'event':'Bitrix.Bitrix.IBlocklElementDelete:{2}', '{3}':'{4}' }}, 'onsuccess':function(){{ window.location.assign('{5}') }} }}); }};",
								BXJSUtility.Encode(string.Format(GetMessageRaw("ElementDeletionConfirmation"), 
								(currentIBlock!=null) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.ElementName.ToLower()) 
								: string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()))),
								BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/handlers/Main/HermitageEventHandler.ashx")),
								ElementId.ToString(),
								BXJSUtility.Encode(BXCsrfToken.TokenKey), 
								BXJSUtility.Encode(BXCsrfToken.GenerateToken()),
								BXJSUtility.Encode(GetElementPostDeleteUrl()));	
							return menuItems;
							*/
							#endregion
						};
				return info;
			}
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/cat_detail.gif";

			Group = new BXComponentGroup("catalogue", GetMessageRaw("Group"), 10, BXComponentGroup.Content);

			ParamsDefinition.Add(BXParametersDefinition.Cache);

			BXCategory main = BXCategory.Main;

			ParamsDefinition.Add(
				"IBlockTypeId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockType"),
					String.Empty,
					main
				)
			);

			ParamsDefinition.Add(
				"IBlockId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockCode"),
					String.Empty,
					main
				)
			);

			ParamsDefinition.Add(
				"ElementId",
				new BXParamText(
					GetMessageRaw("ElementID_1"),
					"0",
					main
				));

			ParamsDefinition.Add(
				"ElementCode",
				new BXParamText(
				GetMessageRaw("ElementCode"),
				String.Empty,
				main
			));

			BXCategory additionalSettings = BXCategory.AdditionalSettings;

			ParamsDefinition.Add(
				"Properties",
				new BXParamMultiSelection(
					GetMessageRaw("Properties"),
					"-",
					additionalSettings
				));

			ParamsDefinition.Add(
				"PropertyKeywords",
				new BXParamSingleSelection(
					GetMessageRaw("SetPageKeywordsFromProperty"),
					"-",
					additionalSettings
				));

			ParamsDefinition.Add(
				"PropertyDescription",
				new BXParamSingleSelection(
					GetMessageRaw("SetPageDescriptionFromProperty"),
					"-",
					additionalSettings
				));

			ParamsDefinition.Add(
				"AddAdminPanelButtons",
				new BXParamYesNo(
					GetMessageRaw("AddButtonsForThisComponentToAdminPanel"),
					false,
					additionalSettings
				));
			ParamsDefinition.Add(
				"SetPageTitle",
				new BXParamYesNo(
					GetMessageRaw("SetPageTitle"),
					true,
					additionalSettings
				));

			ParamsDefinition.Add(
				"ElementDetailUrl",
				new BXParamText(
					GetMessageRaw("DetailedInfo"),
					"detail.aspx?section_id=#SECTION_ID#&element_id=#ELEMENT_ID#",
					additionalSettings
				));

			BXCategory votingCategory = new BXCategory(GetMessageRaw("Category.Voting"), "Voting", 220);

			ParamsDefinition.Add(
				"EnableVotingForElement",
				new BXParamYesNo(
					GetMessageRaw("Param.EnableVotingForElement"),
					false,
					votingCategory
				));

			ParamsDefinition.Add(
				"RolesAuthorizedToVote",
				new BXParamMultiSelection(
					GetMessageRaw("Param.RolesAuthorizedToVote"),
					string.Empty,
					votingCategory
				));

			#region StockCatalog
			BXCategory stockCatalogCategory = new BXCategory(GetMessageRaw("Category.StockCatalog"), "StockCatalog", 1000);

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
					"act",
					stockCatalogCategory,
					new ParamClientSideActionGroupViewMember(ClientID, "ActionParamName", new string[] { "StockCatalog" })
				));

			ParamsDefinition.Add(
				"CatalogItemIdParamName",
				new BXParamText(
					GetMessageRaw("Param.CatalogItemIdParamName"),
					"id",
					stockCatalogCategory,
					new ParamClientSideActionGroupViewMember(ClientID, "CatalogItemIdParamName", new string[] { "StockCatalog" })
				));

			ParamsDefinition.Add(
				"CatalogItemQuantityParamName",
				new BXParamText(
					GetMessageRaw("Param.CatalogItemQuantityParamName"),
					"qty",
					stockCatalogCategory,
					new ParamClientSideActionGroupViewMember(ClientID, "CatalogItemQuantityParamName", new string[] { "StockCatalog" })
				));
			#endregion
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

			ParamsDefinition["PropertyKeywords"].Values = properties;
			ParamsDefinition["PropertyDescription"].Values = properties;

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
				ParamsDefinition["ShowCatalogItemProperties"].Values = fieldValues;
			}
		}

		private bool? canModifyElement = null;
		public bool CanModifyElement
		{
			get
			{
				return (canModifyElement ?? (canModifyElement = BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead)
					&& BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))).Value;
			}
		}

		public string GetElementEditUrl()
		{
			return CanModifyElement ? string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"), "?id=", ElementId.ToString(), "&", BXConfigurationUtility.Constants.BackUrl, "=", UrlEncode(Request.RawUrl)) : string.Empty;
		}

		public string GetElementDeleteUrl()
		{
			Uri url = BXSefUrlManager.CurrentUrl;
			NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
			qsParams["elementAction"] = "delete";
			qsParams["elementId"] = ElementId.ToString();
			qsParams[BXCsrfToken.TokenKey] = BXCsrfToken.GenerateToken();
			return string.Concat(url.AbsolutePath, "?", qsParams.ToString());
		}

		public string GetElementPostDeleteUrl()
		{
			string urlTemplate = Parameters.Get("Template_BackUrlTemplate");
			if (!string.IsNullOrEmpty(urlTemplate))
			{
				BXParamsBag<object> replace = new Bitrix.DataTypes.BXParamsBag<object>();
				replace["ElementId"] = replace["ELEMENT_ID"] = ElementId;
				replace["ElementCode"] = replace["ELEMENT_CODE"] = ElementCode;

				int sectionId = Parameters.GetInt("SectionId");
				if (sectionId == 0 && Element != null && Element.Sections.Count != 0)
					sectionId = Element.Sections[0].SectionId;
				replace["SectionId"] = replace["SECTION_ID"] = sectionId;

				replace["SectionCode"] = replace["SECTION_CODE"] = Parameters.GetString("SectionCode", string.Empty);
				return ResolveTemplateUrl(urlTemplate, replace);
			}

			Uri url = BXSefUrlManager.CurrentUrl;
			NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
			qsParams.Remove("elementAction");
			qsParams.Remove("elementId");
			qsParams.Remove(BXCsrfToken.TokenKey);
			return qsParams.Count > 0 ? string.Concat(url.AbsolutePath, "?", qsParams.ToString()) : url.AbsolutePath;
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

		private static bool? isCatalogModuleInstalled = null;
		private static bool IsCatalogModuleInstalled
		{
			get { return (isCatalogModuleInstalled ?? (isCatalogModuleInstalled = Bitrix.Modules.BXModuleManager.IsModuleInstalled("catalog"))).Value; }
		}

		/// <summary>
		/// Типы цен
		/// </summary>
		[DataContract]
		public class CatalogPriceTypeInfo
		{
			public CatalogPriceTypeInfo(int id, string name)
			{
				this.id = id;
				this.name = HttpUtility.HtmlEncode(name);
			}

			private int id = 0;
			[DataMember(Name = "id", Order = 1, IsRequired = true)]
			public int Id
			{
				get { return this.id; }
				internal set { ; }
			}

			private string name = string.Empty;
			[DataMember(Name = "name", Order = 2, IsRequired = true)]
			public string Name
			{
				get { return this.name; }
				internal set { ; }
			}
		}

		[DataContract]
		public class CatalogClientPriceInfo
		{
			public CatalogClientPriceInfo(
				int priceTypeId,
				int quantityFrom,
				bool isVATIncluded,
				string markedPriceText,
				string adjustmentText,
				string sellingPriceText,
				string vatRateText,
				string vatText,
				decimal priceValue,
				int currencyId
				)
			{
				this.priceTypeId = priceTypeId;
				this.quantityFrom = quantityFrom;
				this.isVATIncluded = isVATIncluded;
				this.markedPriceHtml = markedPriceText;
				this.adjustmentHtml = HttpUtility.HtmlEncode(adjustmentText);
				this.sellingPriceHtml = HttpUtility.HtmlEncode(sellingPriceText);
				this.vatRateHtml = HttpUtility.HtmlEncode(vatRateText);
				this.vatHtml = HttpUtility.HtmlEncode(vatText);
				this.priceValue = priceValue;
				this.currencyId = currencyId;
			}

			private int currencyId;

			public int CurrencyId
			{
				get { return this.currencyId; }
			}

			private decimal priceValue;
			public decimal PriceValue
			{
				get { return this.priceValue; }
			}

			private int priceTypeId = 0;
			[DataMember(Name = "typeId", Order = 1, IsRequired = true)]
			public int PriceTypeId
			{
				get { return this.priceTypeId; }
				internal set { ; }
			}

			private int quantityFrom = 0;
			/// <summary>
			/// Нижняя граница кол-ва товара в заказе, при котором разрешается применять цену
			/// </summary>
			public int QuantityFrom
			{
				get { return this.quantityFrom; }
			}

			private bool isVATIncluded = true;
			[DataMember(Name = "vatIncluded", Order = 4, IsRequired = false)]
			/// <summary>
			/// НДС в цене
			/// </summary>
			public bool IsVATIncluded
			{
				get { return this.isVATIncluded; }
				internal set { ; }
			}

			private string markedPriceHtml = string.Empty;
			/// <summary>
			/// Начальная Цена (без скидок и наценок)
			/// </summary>
			public string MarkedPriceHtml
			{
				get { return this.markedPriceHtml; }
			}

			private string adjustmentHtml = string.Empty;
			/// <summary>
			/// Поправка к цене
			/// </summary>
			public string AdjustmentHtml
			{
				get { return this.adjustmentHtml; }
			}

			private string sellingPriceHtml = string.Empty;
			[DataMember(Name = "priceHtml", Order = 2, IsRequired = true)]
			/// <summary>
			/// Отпускная Цена
			/// </summary>
			public string SellingPriceHtml
			{
				get { return this.sellingPriceHtml; }
				internal set { ; }
			}

			private string vatRateHtml = string.Empty;
			/// <summary>
			/// Ставка НДС
			/// </summary>
			public string VATRateHtml
			{
				get { return this.vatRateHtml; }
			}

			private string vatHtml = string.Empty;
			[DataMember(Name = "vatHtml", Order = 3, IsRequired = false)]
			/// <summary>
			/// НДС
			/// </summary>
			public string VATHtml
			{
				get { return this.vatHtml; }
				internal set { ; }
			}

			private string discountHtml = string.Empty;
			[DataMember(Name = "discountHtml", Order = 4, IsRequired = false)]
			/// <summary>
			/// Ставка НДС
			/// </summary>
			public string DiscountHtml
			{
				get { return this.discountHtml; }
				internal set { ; }
			}
		}

		[DataContract]
		public class CatalogClientPriceTierInfo
		{
			public CatalogClientPriceTierInfo(int quantityFrom)
			{
				this.quantityFrom = quantityFrom;
			}

			private int quantityFrom = 0;
			[DataMember(Name = "qtyFrom", Order = 1, IsRequired = true)]
			public int QuantityFrom
			{
				get { return this.quantityFrom; }
				internal set { ; }
			}

			private List<CatalogClientPriceInfo> items = null;
			[DataMember(Name = "prices", Order = 2, IsRequired = false)]
			public IList<CatalogClientPriceInfo> Items
			{
				get { return this.items ?? (this.items = new List<CatalogClientPriceInfo>()); }
				internal set { ; }
			}

			public CatalogClientPriceInfo GetPriceInfoByPriceTypeId(int id)
			{
				if (this.items == null || this.items.Count == 0)
					return null;

				int index = this.items.FindIndex(delegate(CatalogClientPriceInfo obj) { return obj.PriceTypeId == id; });
				return index >= 0 ? this.items[index] : null;
			}
		}

		public class CatalogClientPriceInfoSet
		{
			public CatalogClientPriceInfoSet(CatalogPriceTypeInfo[] priceTypes, CatalogClientPriceInfo[] items, CatalogClientPriceInfo currentSellingPrice)
			{
				this.priceTypes = priceTypes;
				this.items = items;
				this.currentSellingPrice = currentSellingPrice;
			}

			private CatalogPriceTypeInfo[] priceTypes = null;
			public CatalogPriceTypeInfo[] PriceTypes
			{
				get { return this.priceTypes ?? (this.priceTypes = new CatalogPriceTypeInfo[0]); }
			}

			private CatalogClientPriceInfo[] items = null;
			public CatalogClientPriceInfo[] Items
			{
				get { return this.items ?? (this.items = new CatalogClientPriceInfo[0]); }
			}

			private CatalogClientPriceInfo currentSellingPrice = null;
			public CatalogClientPriceInfo CurrentSellingPrice
			{
				get { return this.currentSellingPrice; }
			}

			private CatalogClientPriceTierInfo[] tiers = null;
			public CatalogClientPriceTierInfo[] GetTiers()
			{
				if (this.tiers != null)
					return this.tiers;

				if (this.items == null || this.items.Length == 0)
					return this.tiers = new CatalogClientPriceTierInfo[0];

				List<CatalogClientPriceTierInfo> list = new List<CatalogClientPriceTierInfo>();
				for (int i = 0; i < this.items.Length; i++)
				{
					CatalogClientPriceInfo item = this.items[i];
					int index = list.FindIndex(delegate(CatalogClientPriceTierInfo obj) { return obj.QuantityFrom == item.QuantityFrom; });
					CatalogClientPriceTierInfo tier = index >= 0 ? list[index] : null;
					if (tier == null)
					{
						tier = new CatalogClientPriceTierInfo(item.QuantityFrom);
						list.Add(tier);
					}
					tier.Items.Add(item);
				}
				list.Sort(delegate(CatalogClientPriceTierInfo x, CatalogClientPriceTierInfo y) { return x.QuantityFrom - y.QuantityFrom; });
				return this.tiers = list.ToArray();
			}

			public CatalogClientPriceInfo GetPriceInfoByPriceTypeId(int id)
			{
				if (this.items == null || this.items.Length == 0)
					return null;

				int index = Array.FindIndex<CatalogClientPriceInfo>(this.items, delegate(CatalogClientPriceInfo obj) { return obj.PriceTypeId == id; });
				return index >= 0 ? this.items[index] : null;
			}
		}

		/// <summary>
		/// SKU (предложение)
		/// </summary>
		public class CatalogSKUItem
		{
			public CatalogSKUItem(ICatalog catalog, BXIBlockElement element)
			{
				if (catalog == null)
					throw new ArgumentNullException("element");

				this.catalog = catalog;

				if (element == null)
					throw new ArgumentNullException("element");

				this.element = element;
				this.id = element.Id;
				this.blockId = element.IBlockId;
			}

			private ICatalog catalog = null;
			public ICatalog Catalog
			{
				get { return this.catalog; }
			}

			private int blockId = 0;
			public int BlockId
			{
				get { return this.blockId; }
			}

			private int id = 0;
			public int Id
			{
				get { return this.id; }
				set
				{
					this.id = value > 0 ? value : 0;
					if (this.element != null)
						this.element = null;
				}
			}

			private BXIBlockElement element = null;
			public BXIBlockElement Element
			{
				get
				{
					if (this.element != null)
						return this.element;

					if (this.id <= 0)
						return null;

					BXIBlockElementCollection c = BXIBlockElement.GetList(
						new BXFilter(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, this.id)),
						null,
						new BXSelectAdd(BXIBlockElement.Fields.CustomFields[this.blockId]),
						new BXQueryParams(BXPagingOptions.Top(1)),
						BXTextEncoder.EmptyTextEncoder);

					return this.element = c.Count > 0 ? c[0] : null;
				}
			}

			/// <summary>
			/// Получить наименование
			/// </summary>
			public string GetName(bool encode)
			{
				if (Element == null)
					return string.Empty;

				return encode ? BXTextEncoder.HtmlTextEncoder.Encode(Element.Name) : Element.Name;
			}

			public CatalogClientPriceInfoSet GetClientPriceInfoSet(int initQty, bool displayAllTiers, int userId, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes)
			{
				return this.catalog.GetClientPriceSet(this.id, initQty, displayAllTiers, userId, currencyId, includeVATInPrice, priceTypes);
			}
		}

		public bool IsInStock
		{
			get {  return Catalog != null && Catalog.CheckStockAvailability(IBlockId) && Catalog.QuantityInStock(CurrentSkuId > 0 ? CurrentSkuId : ElementId) > 0; }
		}

		/// <summary>
		/// Торговый каталог
		/// </summary>
		public interface ICatalog
		{
			int QuantityInStock(int elementId);
			bool CheckStockAvailability(int catalogId);
			CatalogPriceTypeInfo[] GetPriceTypes();
			CatalogClientPriceInfoSet GetClientPriceSet(int itemId, int initQuantity, bool displayAllTiers, int userId, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes);
			CatalogueElementDetailComponent.CatalogSKUItem GetItem(int elementId, int iblockId);
			CatalogSKUItem[] GetSKUItems(int elementId, int iblockId);
			CatalogDiscountInfo GetDiscount(int userId, IEnumerable<int> roleIds, int itemId, int priceTypeId,
				decimal price, int quantity, IList<int> sectionIds, string siteId, int currencyId);

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

		private static bool? isSaleModuleInstalled = null;
		private static bool IsSaleModuleInstalled
		{
			get { return (isSaleModuleInstalled ?? (isSaleModuleInstalled = Bitrix.Modules.BXModuleManager.IsModuleInstalled("sale"))).Value; }
		}

		/// <summary>
		/// Корзина покупателя
		/// </summary>
		public interface ISaleCart
		{
			bool Add(int itemId, int itemQty, string itemDetailUrl, HttpRequest request, HttpResponse response);
			int GetQuantity(int itemId, HttpRequest request, HttpResponse response);
			bool IsExists(int itemId, HttpRequest request, HttpResponse response);
			int Count();
			SaleCartItem[] GetSaleCartItems(HttpRequest request, HttpResponse response);
		}

		private ISaleCart SaleCart
		{
			get { return GetSaleCart(this); }
		}

		private static object saleCartSync = new object();
		private static bool isSaleCartLoaded = false;
		private static volatile ISaleCart saleCart = null;
		private static ISaleCart GetSaleCart(TemplateControl caller)
		{
			if (isSaleCartLoaded)
				return saleCart;

			lock (saleCartSync)
			{
				if (isSaleCartLoaded)
					return saleCart;

				if (IsSaleModuleInstalled)
					saleCart = caller.LoadControl("saleCart.ascx") as ISaleCart;

				isSaleCartLoaded = true;
				return saleCart;
			}
		}

		public class ElementDetailProperty
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

		internal enum SaleCartActionResponseType
		{
			Html = 1,
			Json
		}

		internal class SaleCartActionHandler
		{
			public SaleCartActionHandler(CatalogueElementDetailComponent component)
			{
				if (component == null)
					throw new ArgumentNullException("component");

				this.component = component;
			}

			private CatalogueElementDetailComponent component = null;
			public CatalogueElementDetailComponent Component
			{
				get { return this.component; }
			}

			public void Process()
			{
				if (!BXCsrfToken.CheckTokenFromRequest(Component.Request.QueryString))
					return;

				HttpRequest request = Component.Request;
				string act = request.QueryString[Component.ActionParamName];
				if (string.IsNullOrEmpty(act))
					return;

				SaleCartActionResponseType responseType =
					string.Equals(request.QueryString["responseType"], "JSON", StringComparison.OrdinalIgnoreCase) ?
						SaleCartActionResponseType.Json : SaleCartActionResponseType.Html;

				act = act.ToUpperInvariant();
				if (string.Equals("ADD2CART", act, StringComparison.Ordinal))
				{
					string s;
					int qty;
					if (string.IsNullOrEmpty(s = request.QueryString[Component.CatalogItemQuantityParamName]) || !int.TryParse(s, out qty))
						qty = 1;


					string detailUrl = request.QueryString["detailUrl"];
					if (string.IsNullOrEmpty(detailUrl))
						detailUrl = BXSefUrlManager.CurrentUrl.AbsoluteUri;

					int count;
					bool isInCart = Component.AddToCart(qty, detailUrl, out count);

					HttpResponse response = Component.Response;
					if (responseType == SaleCartActionResponseType.Json)
					{
						response.ContentType = "text/x-json";
						response.StatusCode = (int)HttpStatusCode.OK;
						response.Write(string.Format("{{id:'{0}', action:'ADD2CART', result:{{isInCart:{1}, addedQty:{2}, totalCount:{3}}}}}", string.Concat(Component.ClientID, "_", Component.ElementId.ToString()), isInCart.ToString().ToLowerInvariant(), qty.ToString(), count.ToString()));
					}
					else
						response.Redirect(BXSefUrlManager.CurrentUrl.AbsolutePath, false);

					if (HttpContext.Current != null)
						HttpContext.Current.ApplicationInstance.CompleteRequest();
					response.End();
				}
			}
		}

		/// <summary>
		/// Элемент корзины пользователя
		/// </summary>
		public class SaleCartItem
		{
			private int elementId = 0;
			public int ElementId
			{
				get { return this.elementId; }
				set { this.elementId = value > 0 ? value : 0; }
			}

			private int quantity = 0;
			public int Quantity
			{
				get { return this.quantity; }
				set { this.quantity = value > 0 ? value : 0; }
			}
		}
	}

	public partial class CatalogueElementDetailTemplate : BXComponentTemplate<CatalogueElementDetailComponent>
	{
		public static void PrepareEnvironment()
		{
			BXPage.Scripts.RequireUtils();
		}

		protected override void Render(HtmlTextWriter writer)
		{
			if (IsComponentDesignMode && Component.Element == null)
				writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
			else
				base.Render(writer);
		}
	}

	public class CatalogDiscountInfo
	{
		public bool HasInfo { get; set; }
		public string Name { get; set; }
		public decimal Value { get; set; }
		public string DisplayHtml { get; set; }
	}
}
