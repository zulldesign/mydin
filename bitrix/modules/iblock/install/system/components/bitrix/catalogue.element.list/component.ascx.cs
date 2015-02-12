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
using Bitrix.Components.Editor;
using System.Net;
using System.Collections.Specialized;
using Bitrix.Services.Js;
using System.Linq;
using Bitrix.UI.Hermitage;
using Bitrix.UI.Components;
using Bitrix.IBlock.UI;

namespace Bitrix.IBlock.Components
{
	public partial class CatalogueElementListComponent : BXComponent
	{
		public enum ActiveFilter
		{
			Active = 1,
			NotActive,
			All
		}

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

		public string IBlockCode
		{
			get { return iblock != null ? iblock.Code : string.Empty; }
		}


		BXIBlock ParamIBlock
		{
			get
			{
				int i = 0;
				if (!Parameters.ContainsKey("IBlockId")) return null;
				if (!int.TryParse(Parameters["IBlockId"], out i)) return null;
				return BXIBlock.GetById(i);
			}
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

		protected List<ElementListItem> listItems;
		public List<ElementListItem> Items
		{
			get { return listItems; }
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

		public ActiveFilter ElementActiveFilter
		{
			get
			{
				object obj = null;
				if (ComponentCache.TryGetValue("ActiveFilter", out obj))
					return (ActiveFilter)obj;

				ActiveFilter filter = Parameters.GetEnum<ActiveFilter>("ActiveFilter", ActiveFilter.Active);
				ComponentCache["ActiveFilter"] = (int)filter;
				return filter;
			}
		}

		public ActiveFilter ElementActiveDateFilter
		{
			get
			{
				object obj = null;
				if (ComponentCache.TryGetValue("ActiveDateFilter", out obj))
					return (ActiveFilter)obj;

				ActiveFilter filter = Parameters.GetEnum<ActiveFilter>("ActiveDateFilter", ActiveFilter.All);
				ComponentCache["ActiveDateFilter"] = (int)filter;
				return filter;
			}
		}

        public bool FilterByElementCustomProperty
        {
            get 
            { 
                return Parameters.GetBool("FilterByElementCustomProperty", false); 
            }
            set 
            {
                Parameters["FilterByElementCustomProperty"] = value.ToString();
            }
        }

        private BXParamsBag<object> elementCustomPropertyFilterSettings = null;
		public BXParamsBag<object> ElementCustomPropertyFilterSettings
		{
			get
			{
                return elementCustomPropertyFilterSettings ?? (elementCustomPropertyFilterSettings = BXParamsBag<object>.FromString(Parameters.GetString("ElementCustomPropertyFilterSettings", string.Empty)) ?? new BXParamsBag<object>());
			}
			set
			{
                if ((elementCustomPropertyFilterSettings = value) != null)
                    Parameters["ElementCustomPropertyFilterSettings"] = elementCustomPropertyFilterSettings.ToString();
                else
                    Parameters.Remove("ElementCustomPropertyFilterSettings");
			}
		}
		public bool isErrorOccured = false;
		public string errorMessage = String.Empty;

		public bool DisplayStockCatalogData
		{
			get { return Parameters.GetBool("DisplayStockCatalogData", false); }
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

		public int InitQuantity
		{
			get
			{
				int r = Parameters.GetInt("InitQuantity", 1);
				return r > 0 ? r : 1;
			}
		}

		public bool AcceptQuantity
		{
			get { return Parameters.GetBool("AcceptQuantity", false); }
		}

		public bool IncludeVATInPrice
		{
			get { return Parameters.GetBool("IncludeVATInPrice", true); }
		}

		public bool DisplayVAT
		{
			get { return Parameters.GetBool("DisplayVAT", false); }
		}

		private int UserId
		{
			get
			{
				BXIdentity identity = (BXIdentity)BXPrincipal.Current.Identity;
				return identity != null ? identity.Id : 0;
			}
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

		public string GetAddToCartUrl(int itemId, string detailUrl, bool jsonResponse)
		{
			NameValueCollection qsParams = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
			qsParams[BXCsrfToken.TokenKey] = BXCsrfToken.GenerateToken();
			qsParams[ActionParamName] = "ADD2CART";
			qsParams[CatalogItemIdParamName] = itemId.ToString();
			qsParams["detailUrl"] = detailUrl;

			if (jsonResponse)
				qsParams["responseType"] = "JSON";

			string path = BXSefUrlManager.CurrentUrl.PathAndQuery;
			int whatInd = path.IndexOf('?');
			if (whatInd >= 0)
				path = path.Substring(0, whatInd);

			return string.Concat(path, "?", qsParams.ToString());
		}

		StringBuilder sb = new StringBuilder();
		public string GetAddToCartUrlTemplate(string itemIdPlaceHolder, string quantityPlaceHolder, string detailUrl, bool jsonResponse)
		{
			return GetAddToCartUrlTemplate(itemIdPlaceHolder, quantityPlaceHolder, BXCsrfToken.GenerateToken(), detailUrl, jsonResponse);
		}

		public string GetAddToCartUrlTemplate(string itemIdPlaceHolder, string quantityPlaceHolder, string csrfTokenPairPlaceHolder, string detailUrl, bool jsonResponse)
		{
			NameValueCollection qsParams = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
			qsParams[ActionParamName] = "ADD2CART";
			qsParams["detailUrl"] = detailUrl;

			//хранители не должны кодироваться, поэтому присоединяются к готовой строке
			qsParams.Remove(BXCsrfToken.TokenKey);
			qsParams.Remove(CatalogItemQuantityParamName);
			qsParams.Remove(CatalogItemQuantityParamName);

			if (jsonResponse)
				qsParams["responseType"] = "JSON";

			sb.Length = 0;
			sb.Append(qsParams);
			if (sb.Length > 0)
				sb.Append("&");

			//хранители
			sb.Append(csrfTokenPairPlaceHolder);
			sb.Append("&").Append(CatalogItemIdParamName).Append("=").Append(itemIdPlaceHolder);
			sb.Append("&").Append(CatalogItemQuantityParamName).Append("=").Append(quantityPlaceHolder);

			string path = BXSefUrlManager.CurrentUrl.PathAndQuery;
			int whatInd = path.IndexOf('?');
			if (whatInd >= 0)
				path = path.Substring(0, whatInd);

			return string.Concat(path, "?", sb.ToString());
		}

		private bool? allowComparison = null;
		public bool AllowComparison
		{
			get { return (this.allowComparison ?? (this.allowComparison = Parameters.GetBool("AllowComparison", false))).Value; }
		}

		private string comparerUrlTemplate = null;
		public string ComparerUrlTemplate
		{
			get { return this.comparerUrlTemplate ?? (this.comparerUrlTemplate = Parameters.GetString("ComparerUrlTemplate", "compare.aspx?action=addCmp&id=#ElementId#")); }
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			int id;
			if (CanModifyElements
				&& !string.IsNullOrEmpty(Request["elementAction"])
				&& string.Equals(Request["elementAction"], "delete", StringComparison.OrdinalIgnoreCase)
				&& int.TryParse(Request["elementId"], out id)
				&& id > 0
				&& BXCsrfToken.CheckToken(Request[BXCsrfToken.TokenKey]))
			{
				try
				{
					BXIBlockElement.Delete(id);
				}
				catch (Exception)
				{
				}

				Uri url = BXSefUrlManager.CurrentUrl;
				NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
				qsParams.Remove("elementAction");
				qsParams.Remove("elementId");
				qsParams.Remove(BXCsrfToken.TokenKey);
				Response.Redirect(qsParams.Count > 0 ? string.Concat(url.AbsolutePath, "?", qsParams.ToString()) : url.AbsolutePath, true);
			}

			if (CanModifySection
				&& !string.IsNullOrEmpty(Request["sectionAction"])
				&& string.Equals(Request["sectionAction"], "delete", StringComparison.OrdinalIgnoreCase)
				&& !string.IsNullOrEmpty(Request["sectionId"])
				&& int.TryParse(Request["sectionId"], out id)
				&& id > 0
				&& BXCsrfToken.CheckToken(Request[BXCsrfToken.TokenKey]))
			{
				try
				{
					BXIBlockSection.Delete(id);
				}
				catch (Exception)
				{
				}

				string sectionElementsListTemplate = Parameters.GetString("SectionElementListUrl");
				if (!string.IsNullOrEmpty(sectionElementsListTemplate))
				{
					BXParamsBag<object> replace = new BXParamsBag<object>();
					replace.Add("IBLOCK_ID", IBlockId);
					replace.Add("IBlockId", IBlockId);
					replace.Add("SectionId", string.Empty);
					replace.Add("SECTION_ID", string.Empty);
					replace.Add("SectionCode", string.Empty);
					replace.Add("SECTION_CODE", string.Empty);
					Response.Redirect(BXSefUrlUtility.MakeLink(sectionElementsListTemplate, replace));
				}
				else
				{
					Uri url = BXSefUrlManager.CurrentUrl;
					NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
					qsParams.Remove("sectionAction");
					qsParams.Remove("sectionId");
					qsParams.Remove(BXCsrfToken.TokenKey);
					Response.Redirect(qsParams.Count > 0 ? string.Concat(url.AbsolutePath, "?", qsParams.ToString()) : url.AbsolutePath, true);
				}
			}

			(new SaleCartActionHandler(this)).Process();
			if (CanReadElements)
				CatalogActionHandler.Process(this);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BXPagingParams pagingParams = PreparePagingParams();

			if (IsCached(BXPrincipal.Current.GetAllRoles(true), pagingParams))
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
					GenerateNotFoundError(GetMessage("WrongIBlockCode"));
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
					#region old
					//String[] sectionCodeChain = SectionCode.Trim('/').Split('/');
					//int? parentSectionID = null;
					//for (int i = 0; i < sectionCodeChain.Length; i++)
					//{
					//    BXIBlockSectionCollection sectionCollection = BXIBlockSection.GetList(
					//        new BXFilter(
					//            new BXFilterItem(BXIBlockSection.Fields.Code, BXSqlFilterOperators.Equal, sectionCodeChain[i]),
					//            new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
					//            new BXFilterItem(BXIBlockSection.Fields.SectionId, BXSqlFilterOperators.Equal, parentSectionID)
					//        ),
					//        null
					//    );

					//    if (sectionCollection == null || sectionCollection.Count < 1)
					//    {
					//        Section = null;
					//        GenerateNotFoundError(GetMessage("SectionNotFound"));
					//        return;
					//    }

					//    Section = sectionCollection[0];
					//    SectionId = sectionCollection[0].Id;
					//    parentSectionID = sectionCollection[0].Id;
					//}
					#endregion
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
						GenerateNotFoundError(GetMessage("SectionNotFound"));
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
						GenerateNotFoundError(GetMessage("SectionNotFound"));
						return;
					}

					Section = sectionCollection[0];
				}

				BXParamsBag<object> pagingReplace = new BXParamsBag<object>();
				pagingReplace["IblockId"] = IBlock.Id;
				pagingReplace["IblockCode"] = IBlock.Code;
				if (Section != null && SectionId > 0)
				{
					SectionName = Section.Name;

					pagingReplace["SectionCode"] = Section.Code;
					pagingReplace["SECTION_CODE"] = Section.Code;
					pagingReplace["SectionId"] = Section.Id;
					pagingReplace["SECTION_ID"] = Section.Id;
				}


				BXFilter elementFilter = new BXFilter(
					new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
					new BXFilterItem(BXIBlockElement.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y")
				);

				if (FilterByElementCustomProperty && ElementCustomPropertyFilterSettings != null && ElementCustomPropertyFilterSettings.Count > 0)
				{
					BXCustomFieldCollection sectCustomFields = BXCustomEntityManager.GetFields(BXIBlock.GetCustomFieldsKey(IBlock.Id));
					foreach (KeyValuePair<string, object> kv in ElementCustomPropertyFilterSettings)
					{
						BXCustomField sectCustomField;
						if (!sectCustomFields.TryGetValue(kv.Key, out sectCustomField))
							continue;
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.GetCustomField(IBlock.Id, kv.Key), sectCustomField.Multiple ? BXSqlFilterOperators.In : BXSqlFilterOperators.Equal, kv.Value));
					}
				}


				if (ElementActiveFilter != ActiveFilter.All)
					elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, ElementActiveFilter == ActiveFilter.Active ? "Y" : "N"));

				if (ElementActiveDateFilter != ActiveFilter.All)
					elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, ElementActiveDateFilter == ActiveFilter.Active ? "Y" : "N"));

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
					BXSchemeFieldBase f = BXIBlockElement.Fields.CustomFields[IBlockId].GetFieldByKey(sortBy);
					if (f != null)
						elementOrderBy.Add(f, sortOrder);
				}
				else
					elementOrderBy.Add(BXIBlockElement.Fields, sortBy, sortOrder);

				//Paging
				bool isLegalPage;
				BXQueryParams queryParams = PreparePaging(
					pagingParams,
					delegate { return BXIBlockElement.Count(elementFilter); },
					pagingReplace,
					out isLegalPage
				);

				int rpp = Parameters.Get<int>("PagingRecordsPerPage", 10);
				if (!Parameters.Get<bool>("PagingAllow") && rpp > 0)
					queryParams = new BXQueryParams(new BXPagingOptions(0, rpp));
				else if (!isLegalPage)
					AbortCache();

				BXSelect elementSelect = new BXSelectAdd(
					BXIBlockElement.Fields.Sections.Section,
					BXIBlockElement.Fields.CustomFields[IBlockId],
					BXIBlockElement.Fields.PreviewImage,
					BXIBlockElement.Fields.DetailImage);

				if (DisplayStockCatalogData && Catalog != null)
					Catalog.PrepareElementSelect(elementSelect);

				BXIBlockElementCollection iblockElements = BXIBlockElement.GetList(
					elementFilter,
					elementOrderBy,
					elementSelect,
					queryParams,
					BXTextEncoder.HtmlTextEncoder);

				listItems = new List<ElementListItem>();

				foreach (BXIBlockElement iblockElement in iblockElements)
				{
					ElementListItem listItem = new ElementListItem(this);

					listItem.Element = iblockElement;

					BXIBlockSectionCollection sections = iblockElement.GetSections();
					string detailUrl = Parameters.Get<string>("ElementDetailUrl");
					if (!string.IsNullOrEmpty(detailUrl) || AllowComparison || IsCatalogModuleInstalled)
					{
						int replaceSectionId = 0;
						string replaceSectionCode = String.Empty;

						// Hellish logic to get the section for the element

						if (sections != null && sections.Count != 0)
						{
							bool hasSectionCode = !string.IsNullOrEmpty(SectionCode);
							if (SectionId != 0 || hasSectionCode)
							{
								foreach (BXIBlockSection s in sections)
								{
									if (!s.ActiveGlobal)
										continue;
									if (SectionId == s.Id || hasSectionCode && SectionCode == s.TextEncoder.Decode(s.Code))
									{
										replaceSectionId = s.Id;
										replaceSectionCode = s.Code;
										break;
									}
								}
							}
							if (replaceSectionId == 0)
							{
								foreach (BXIBlockSection s in sections)
								{
									if (!s.ActiveGlobal)
										continue;
									replaceSectionId = s.Id;
									replaceSectionCode = s.Code;
									break;
								}
							}
						}

						BXParamsBag<object> replaceItems = new BXParamsBag<object>();
						replaceItems.Add("IblockId", iblockElement.IBlockId);
						replaceItems.Add("IBLOCK_ID", iblockElement.IBlockId);
						replaceItems.Add("IblockCode", IBlock.Code);
						replaceItems.Add("IBLOCK_CODE", IBlock.Code);
						replaceItems.Add("ELEMENT_ID", iblockElement.Id);
						replaceItems.Add("ElementId", iblockElement.Id);
						replaceItems.Add("ElementCode", iblockElement.Code);
						replaceItems.Add("ELEMENT_CODE", iblockElement.Code);
						replaceItems.Add("SectionId", replaceSectionId);
						replaceItems.Add("SECTION_ID", replaceSectionId);
						replaceItems.Add("SectionCode", replaceSectionCode);
						replaceItems.Add("SECTION_CODE", replaceSectionCode);

						if (!string.IsNullOrEmpty(detailUrl))
							listItem.ElementDetailUrl = BXSefUrlUtility.MakeLink(detailUrl, replaceItems);

						if (AllowComparison)
						{
							string add2CompareUrl = !string.IsNullOrEmpty(ComparerUrlTemplate) ? BXSefUrlUtility.MakeLink(ComparerUrlTemplate, replaceItems) : string.Empty;
							listItem.ElementAdd2ComparisonListUrl = BXIBlockComparisonHelper.GetAddElementUrl(iblockElement.Id, add2CompareUrl, false);
							listItem.ElementAdd2ComparisonListUrlJson = BXIBlockComparisonHelper.GetAddElementUrl(iblockElement.Id, add2CompareUrl, true);
							listItem.ElementAdd2ComparisonListUrlTemplate = BXIBlockComparisonHelper.GetAddElementUrlTemplate(iblockElement.Id, add2CompareUrl, "#ResponseType#", "#CsrfTokenPair#");
						}
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
						property.DisplayValue = GetCustomProperty(property.Property, property.Field, "&nbsp;/&nbsp;");
						elementProperties.Add(property);
					}

					listItem.Properties = elementProperties;

					//Торговый каталог (отображается когда выбран тип цен)
					if (DisplayStockCatalogData && PriceTypes.Length > 0 && Catalog != null)
					{
						listItem.CanPutInCart = Catalog.CanPutInCart(iblockElement);
						listItem.ClientPriceSet = Catalog.GetClientPriceSet(iblockElement, InitQuantity, false, BXPrincipal.Current.GetAllRoles(true), 0, IncludeVATInPrice, PriceTypes);
						//listItem.QuantityInCart = SaleCart.GetQuantity(iblockElement.Id, Request, Response);

						if (listItem.ClientPriceSet == null || listItem.ClientPriceSet.CurrentSellingPrice == null)
							listItem.DiscountInfo = new CatalogDiscountInfo { HasInfo = false };
						else
							listItem.DiscountInfo = Catalog.GetDiscount(0, BXPrincipal.Current.GetAllRoleIds(true), iblockElement.Id, listItem.ClientPriceSet.CurrentSellingPrice.PriceTypeId, listItem.ClientPriceSet.CurrentSellingPrice.PriceValue,
								InitQuantity, sections.Where(x => x.ActiveGlobal).Select<BXIBlockSection, int>(x => x.Id).ToList(),
								BXSite.Current.Id, listItem.ClientPriceSet.CurrentSellingPrice.CurrencyId);

						listItem.HasSkuItems = Catalog.HasSkuItems(iblockElement);
						foreach (BXCustomField catalogItemField in CatalogItemCustomFields)
						{
							BXCustomProperty cp;
							if (!iblockElement.CustomPublicValues.TryGetCustomProperty(catalogItemField.Name, out cp))
								continue;

							string code = cp.Name.ToUpperInvariant();

							int index = ShowProperties.FindIndex(delegate(string s) { return string.Equals(s, code, StringComparison.Ordinal); });
							if (index < 0)
								continue;

							ShowProperties.RemoveAt(index);

							if (!ShowCatalogItemProperties.Contains(code))
								continue;

							ElementListItemProperty p = new ElementListItemProperty();
							p.Property = cp;
							p.Code = code;
							p.Name = iblockElement.CustomPublicValues.GetDisplayName(cp.Name);
							p.Field = catalogItemField;
							p.Values = p.Values;
							p.DisplayValue = iblockElement.CustomPublicValues.GetHtml(cp.Name, "-", "&nbsp;/&nbsp;");

							listItem.CatalogProperties.Add(p);
						}
					}
					listItems.Add(listItem);
				}

				SetTemplateCachedData();
				IncludeComponentTemplate();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			CatalogueElementListTemplate.PrepareEnvironment(DisplayStockCatalogData);
		}

		public SaleCartItem[] GetSaleCartItems()
		{
			return SaleCart != null ? SaleCart.GetSaleCartItems(Request, Response) : new SaleCartItem[0];
		}

		public BXIBlockComparisonBlockSettings GetComparisonSettings()
		{
			return  BXIBlockComparisonHelper.GetCurrentIBlockSettings(IBlockId, Session);
		}

		private bool AddToCart(int itemId, int qty, string detailUrl, out int cartItemCount)
		{
			cartItemCount = 0;

			if(SaleCart == null)
				return false;

			CatalogClientPriceInfoSet priceSet = Catalog.GetClientPriceSet(itemId, InitQuantity, false, BXPrincipal.Current.GetAllRoles(true), 0, IncludeVATInPrice, PriceTypes);
			if (priceSet == null || priceSet.CurrentSellingPrice == null)
				return false;

			if (!SaleCart.IsExists(itemId, Request, Response))
				SaleCart.Add(itemId, AcceptQuantity ? qty : 1, detailUrl ?? string.Empty, Request, Response);

			cartItemCount = SaleCart.Count();
			return true;
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

		private void SetTemplateCachedData()
		{
			BXPublicPage bitrixPage = Page as BXPublicPage;
			if (bitrixPage != null && !IsComponentDesignMode)
			{
				if (Parameters.Get<bool>("SetPageTitle", true) && SectionName != null)
					bitrixPage.BXTitle = BXTextEncoder.HtmlTextEncoder.Decode(SectionName);
			}
		}

		private void GenerateNotFoundError(string error)
		{
			isErrorOccured = true;
			errorMessage = error;

			AbortCache();

			ScriptManager ajax = ScriptManager.GetCurrent(Page);
			if (!IsComponentDesignMode && BXConfigurationUtility.ShowMode == BXShowMode.View && (ajax == null || !ajax.IsInAsyncPostBack))
			{
				BXError404Manager.Set404Status(Response);

				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null && Parameters.GetBool("SetPageTitle", true) && !String.IsNullOrEmpty(error))
					bitrixPage.Title = error;
			}

			IncludeComponentTemplate();
		}

		private string GetCustomProperty(BXCustomProperty property, BXCustomField field, string separator)
		{
			BXCustomTypePublicView view = BXCustomTypeManager.GetCustomType(field.CustomTypeId).CreatePublicView();
			view.Init(property, field);
			return view.GetHtml(separator);
		}

		#region Admin Panel & Context Menu

		public string AdminPanelSectionId
		{
			get { return "CatalogueListMenuSection"; }
		}

		public string AdminPanelButtonMenuId
		{
			get { return String.Format("CatalogueListButtonMenu_{0}", IBlockId); }
		}

		//Создаем секцию и кнопку для выпадающего меню в панели администратора
		public override void CreatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
		{
			if (!Parameters.Get<bool>("AddAdminPanelButtons") || isErrorOccured)
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
			if (!Parameters.Get<bool>("AddAdminPanelButtons") || isErrorOccured)
				return;

			if (sectionList == null)
				throw new ArgumentNullException("sectionList");

			if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
				!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
				return;

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
			BXPopupMenuItem[] menuItems = new BXPopupMenuItem[2];

			menuItems[0] = new BXPopupMenuItem();
			menuItems[0].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/new_element.gif");
			menuItems[0].ID = String.Format("catalogue_element_new_{0}", IBlockId);
			menuItems[0].Text = (iblock != null) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(iblock.CaptionsInfo.AddElement) :
				String.Format(GetMessageRaw("AddNewElement"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower()));
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

			menuItems[1] = new BXPopupMenuItem();
			menuItems[1].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/new_section.gif");
			menuItems[1].ID = String.Format("catalogue_element_section_new_{0}", IBlockId);
			menuItems[1].Text = (iblock != null) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(iblock.CaptionsInfo.AddSection) :
				String.Format(GetMessageRaw("AddNewSection"), String.IsNullOrEmpty(IBlockSectionName) ? GetMessageRaw("IBlockSectionName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockSectionName.ToLower()));
			menuItems[1].ClientClickScript =
				String.Format(
					"jsUtils.Redirect(arguments, '{0}?type_id={2}&iblock_id={1}&section_id={3}&{4}={5}')",
					VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"),
					IBlockId,
					IBlockTypeId,
					SectionId,
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
				if (BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) 
					&& BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements) 
					&& !isErrorOccured)
					info.CreateComponentContentMenuItems =
						delegate(BXShowMode showMode)
						{
							if (showMode == BXShowMode.View)
								return new BXHermitagePopupMenuBaseItem[0];

							BXHermitageToolbarSettings settings = new BXHermitageToolbarSettings();
							settings.Data.Add("IBlockId", IBlockId);
							settings.Data.Add("IBlockTypeId", IBlockTypeId);
							if(SectionId > 0)
								settings.Data.Add("SectionId", SectionId);

							if (IBlock != null)
								settings.Data.Add("IBlock", IBlock);

							settings.Data.Add("AddElement",
								string.Format(GetMessageRaw("AddNewElement"), String.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower())));

							return BXHermitageToolbarHelper.CreateToolbar(BXHermitageMenuCommand.CreateElement, settings).ItemsToArray();
						};
				return info;
			}
		}

		#endregion

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

			settings.Data.Add("Element", element);

			if (IBlock != null)
				settings.Data.Add("IBlock", IBlock);

			string elementName = (IBlock != null)
				? BXHtmlTextEncoder.HtmlTextEncoder.Decode(IBlock.CaptionsInfo.ElementName.ToLower())
				: string.IsNullOrEmpty(IBlockElementName) ? GetMessageRaw("IBlockElementName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockElementName.ToLower());

			settings.Data.Add("ChangeElement",
				string.Format(GetMessageRaw("EditElementMenuItem"), elementName));

			settings.Data.Add("DeleteElement",
				string.Format(GetMessageRaw("DeleteElementMenuItem"), elementName));

			settings.Data.Add("ElementDetailUrl", Parameters.Get<string>("ElementDetailUrl"));

			settings.Data.Add("ElementDeletionConfirmation",
				string.Format(GetMessageRaw("ElementDeletionConfirmation"), elementName));

			return BXHermitageToolbarHelper.CreateToolbar(BXHermitageMenuCommand.ModifyElement | BXHermitageMenuCommand.DeleteElement, settings);
		}

		private bool? canReadElements = null;
		public bool CanReadElements
		{
			get
			{
				return (canReadElements ?? (canReadElements = BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockPublicRead))).Value;
			}
		}

		private bool? canModifyElements = null;
		public bool CanModifyElements
		{
			get
			{
				return (canModifyElements ?? (canModifyElements = BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead)
					&& BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))).Value;
			}
		}

		private bool? canModifySection = null;
		public bool CanModifySection
		{
			get
			{
				return (canModifySection ?? (canModifySection = BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead)
					&& BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifySections))).Value;
			}
		}

		public string GetSectionAddUrl()
		{
			if (!CanModifySection)
				return string.Empty;

			return string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"), "?type_id=", IBlockTypeId.ToString(), "&iblock_id=", IBlockId.ToString(), "&section_id=", SectionId.ToString(), "&", BXConfigurationUtility.Constants.BackUrl, "=", UrlEncode(Request.RawUrl));
		}

		public string GetElementAddUrl()
		{
			if (!CanModifyElements)
				return string.Empty;

			return string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"), "?type_id=", IBlockTypeId.ToString(), "&iblock_id=", IBlockId.ToString(), "&section_id=", SectionId.ToString(), "&", BXConfigurationUtility.Constants.BackUrl, "=", UrlEncode(Request.RawUrl));
		}

		public string GetElementEditUrl(int elementId)
		{
			if (!CanModifyElements)
				return string.Empty;

			return string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementEdit.aspx"), "?id=", elementId.ToString(), "&", BXConfigurationUtility.Constants.BackUrl, "=", UrlEncode(Request.RawUrl));
		}

		public string GetSectionEditUrl()
		{
			if (!CanModifySection || SectionId <= 0)
				return string.Empty;

			return string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"), "?type_id=", IBlockTypeId.ToString(), "&iblock_id=", IBlockId.ToString(), "&section_id=", SectionId.ToString(), "&id=", SectionId.ToString(), "&", BXConfigurationUtility.Constants.BackUrl, "=", UrlEncode(Request.RawUrl));
		}

		public string GetElementDeleteUrl(int elementId)
		{
			if (!CanModifyElements)
				return string.Empty;

			Uri url = BXSefUrlManager.CurrentUrl;
			NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
			qsParams["elementAction"] = "delete";
			qsParams["elementId"] = elementId.ToString();
			qsParams[BXCsrfToken.TokenKey] = BXCsrfToken.GenerateToken();
			return string.Concat(url.AbsolutePath, "?", qsParams.ToString());
		}

		public string GetSectionDeleteUrl()
		{
			if (!CanModifySection || SectionId <= 0)
				return string.Empty;

			Uri url = BXSefUrlManager.CurrentUrl;
			NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
			qsParams["sectionAction"] = "delete";
			qsParams["sectionId"] = SectionId.ToString();
			qsParams[BXCsrfToken.TokenKey] = BXCsrfToken.GenerateToken();
			return string.Concat(url.AbsolutePath, "?", qsParams.ToString());
		}

		public string GetSkuItemsUrl(string elementId)
		{
			if (!CanReadElements)
				return string.Empty;

			Uri url = BXSefUrlManager.CurrentUrl;
			NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
			qsParams[ActionParamName] = "SKUITEMS";
			qsParams["elementId"] = elementId;
			qsParams[BXCsrfToken.TokenKey] = BXCsrfToken.GenerateToken();
			return string.Concat(url.AbsolutePath, "?", qsParams.ToString());
		}

		public string GetSkuItemsUrlTemplate(string elementPlaceHolder, string csrfPairPlaceHolder)
		{
			if (!CanReadElements)
				return string.Empty;

			Uri url = BXSefUrlManager.CurrentUrl;
			NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
			qsParams[ActionParamName] = "SKUITEMS";

			StringBuilder qs = new StringBuilder(qsParams.ToString());
			qs.Append("&").Append("elementId").Append("=").Append(elementPlaceHolder);
			qs.Append("&").Append(csrfPairPlaceHolder);

			return string.Concat(url.AbsolutePath, "?", qs.ToString());
		}

		public string comparerUrl = null;
		public string GetAddSku2ComparisonListUrl(string skuId)
		{
			if (comparerUrl == null)
			{
				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace.Add("IblockId", IBlockId);
				replace.Add("IBLOCK_ID", IBlockId);
				replace.Add("IblockCode", IBlockCode);
				replace.Add("IBLOCK_CODE", IBlockCode);
				comparerUrl = BXSefUrlUtility.MakeLink(ComparerUrlTemplate, replace) ?? string.Empty;
			}
			return BXIBlockComparisonHelper.GetAddElementUrl(skuId, comparerUrl, false);
		}

		private Uri comparerUri = null;
		public string GetAddSku2ComparisonListUrlTemplate(string skuIdPlaceHolder, string responseTypePlaceHolder, string csrfPairPlaceHolder)
		{
			if (comparerUri == null)
			{
				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace.Add("IblockId", IBlockId);
				replace.Add("IBLOCK_ID", IBlockId);
				replace.Add("IblockCode", IBlockCode);
				replace.Add("IBLOCK_CODE", IBlockCode);
				string url = BXSefUrlUtility.MakeLink(ComparerUrlTemplate, replace) ?? string.Empty;
				comparerUri = !string.IsNullOrEmpty(url) ? new Uri(url, UriKind.RelativeOrAbsolute) : BXSefUrlManager.CurrentUrl;
			}
			return BXIBlockComparisonHelper.GetAddElementUrlTemplate(comparerUri, skuIdPlaceHolder, responseTypePlaceHolder, csrfPairPlaceHolder);
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/cat_list.gif";
			Group = new BXComponentGroup("catalogue", GetMessageRaw("Group"), 10, BXComponentGroup.Content);

			BXCategory dataSourceCategory = BXCategory.DataSource,
				mainCategory = BXCategory.Main,
				addSettingsCategory = BXCategory.AdditionalSettings,
				sefCategory = BXCategory.Sef;

			ParamsDefinition.Add(BXParametersDefinition.Cache);
			ParamsDefinition.Add(BXParametersDefinition.Paging);
			ParamsDefinition.Add(BXParametersDefinition.PagingUrl);

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
				"SectionId",
				new BXParamText(
					GetMessageRaw("SectionID"),
					"0",
					mainCategory
				));

			ParamsDefinition.Add(
				"SectionCode",
				new BXParamText(
				GetMessageRaw("SectionMnemonicCode"),
				"",
				mainCategory
			));

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

			ParamsDefinition.Add("ActiveFilter", new BXParamSingleSelection(GetMessageRaw("ActiveFilter"), "Active", dataSourceCategory));
			ParamsDefinition.Add("ActiveDateFilter", new BXParamSingleSelection(GetMessageRaw("ActiveDateFilter"), "All", dataSourceCategory));

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
					"list.aspx?section_id=#SECTION_ID#",
					sefCategory
				));

			ParamsDefinition.Add(
				"ElementDetailUrl",
				new BXParamText(
					GetMessageRaw("DetailedInfo"),
					"detail.aspx?section_id=#SECTION_ID#&element_id=#ELEMENT_ID#",
					sefCategory
				));

			BXCategory customFieldCategory = BXCategory.CustomField;

			ParamsDefinition.Add("FilterByElementCustomProperty",
				new BXParamYesNo(
					GetMessageRaw("FilterByElementCustomProperty"),
					false,
					customFieldCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, "FilterByElementCustomProperty", "FilterByElementCustomProperty", string.Empty)
					)
				);

			ParamsDefinition.Add("ElementCustomPropertyFilterSettings",
				new BXParamCustomFieldFilter(
					GetMessageRaw("ElementCustomPropertyFilterSettings"),
					string.Empty,
					customFieldCategory,
					string.Empty,//BXIBlockModuleConfiguration,
					new ParamClientSideActionGroupViewMember(ClientID, "ElementCustomPropertyFilterSettings", new string[] { "FilterByElementCustomProperty" })
					)
				);

			#region StockCatalog
			BXCategory stockCatalogCategory = new BXCategory(GetMessageRaw("Category.StockCatalog"), "StockCatalog", 240);

			ParamsDefinition.Add(
				"DisplayStockCatalogData",
				new BXParamYesNo(
					GetMessageRaw("Param.DisplayStockCatalogData"),
					false,
					stockCatalogCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, "DisplayStockCatalogData", "StockCatalog", string.Empty)
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

			#region Comarison
			BXCategory comparisonCategory = new BXCategory(GetMessageRaw("Category.Comparison"), "Comparison", 230);

			ParamsDefinition.Add(
				"AllowComparison",
				new BXParamYesNo(
					GetMessageRaw("Param.AllowComparison"),
					false,
					comparisonCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, "AllowComparison", "ComparisonOn", string.Empty)));

			ParamsDefinition.Add(
				"ComparerUrlTemplate",
				new BXParamText(
					GetMessageRaw("Param.ComparerUrlTemplate"),
					"compare.aspx?action=addCmp&id=#ElementId#",
					comparisonCategory,
					new ParamClientSideActionGroupViewMember(ClientID, "ComparerUrlTemplate", new string[] { "ComparisonOn" })));
			#endregion
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

			ParamsDefinition["SortBy"].Values = sortingFields;


			ParamsDefinition["ActiveFilter"].Values = new List<BXParamValue>();
			ParamsDefinition["ActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.Active"), ActiveFilter.Active.ToString()));
			ParamsDefinition["ActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.NotActive"), ActiveFilter.NotActive.ToString()));
			ParamsDefinition["ActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.All"), ActiveFilter.All.ToString()));

			ParamsDefinition["ActiveDateFilter"].Values = new List<BXParamValue>();
			ParamsDefinition["ActiveDateFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveDateFilter.Active"), ActiveFilter.Active.ToString()));
			ParamsDefinition["ActiveDateFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveDateFilter.NotActive"), ActiveFilter.NotActive.ToString()));
			ParamsDefinition["ActiveDateFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveDateFilter.All"), ActiveFilter.All.ToString()));


			if (ParamIBlock != null)
			{
				BXParamCustomFieldFilter cfParamFilter = ParamsDefinition["ElementCustomPropertyFilterSettings"] as BXParamCustomFieldFilter;
				cfParamFilter.EntityId = BXIBlock.GetCustomFieldsKey(ParamIBlock.Id);
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

		public class ElementListItem
		{
			private CatalogueElementListComponent component = null;
			public ElementListItem(CatalogueElementListComponent component)
			{
				if (component == null)
					throw new ArgumentNullException("component");

				this.component = component;
			}

			public bool CanPutInCart { get; set; }

			private BXIBlockElement element;
			public BXIBlockElement Element
			{
				get { return element; }
				set { element = value; }
			}

			public int ElementId
			{
				get { return element != null ? element.Id : 0; }
			}

			private string elementDetailUrl;
			public string ElementDetailUrl
			{
				get { return elementDetailUrl; }
				set { elementDetailUrl = value; }
			}

			private string elementAdd2ComparisonListUrl;
			public string ElementAdd2ComparisonListUrl
			{
				get { return elementAdd2ComparisonListUrl; }
				set { elementAdd2ComparisonListUrl = value; }
			}

			private string elementAdd2ComparisonListUrlJson;
			public string ElementAdd2ComparisonListUrlJson
			{
				get { return elementAdd2ComparisonListUrlJson; }
				set { elementAdd2ComparisonListUrlJson = value; }
			}

			private string elementAdd2ComparisonListUrlTemplate;
			public string ElementAdd2ComparisonListUrlTemplate
			{
				get { return elementAdd2ComparisonListUrlTemplate; }
				set { elementAdd2ComparisonListUrlTemplate = value; }
			}

			private List<ElementListItemProperty> properties;
			public List<ElementListItemProperty> Properties
			{
				get { return properties; }
				set { properties = value; }
			}

			private CatalogClientPriceInfoSet clientPriceSet = null;
			public CatalogClientPriceInfoSet ClientPriceSet
			{
				get { return clientPriceSet; }
				set { clientPriceSet = value; }
			}

			public CatalogClientPriceInfo CurrentSellingPrice
			{
				get { return ClientPriceSet != null ? ClientPriceSet.CurrentSellingPrice : null; }
			}

			public bool IsSellingAllowed
			{
				get { return CurrentSellingPrice != null; }
			}

			public bool InCart
			{
				get { return quantityInCart > 0; }
			}

			public bool InComparisonList
			{
				get { return element != null && BXIBlockComparisonHelper.IsAdded(this.element.Id, this.element.IBlockId, component.Session); }
			}

			int quantityInCart;

			public int QuantityInCart
			{
				get { return quantityInCart; }
				set { quantityInCart = value; }
			}

			private List<ElementListItemProperty> catalogProperties = null;
			public List<ElementListItemProperty> CatalogProperties
			{
				get { return catalogProperties ?? (this.catalogProperties = new List<ElementListItemProperty>()); }
				set { catalogProperties = value; }
			}

			public CatalogDiscountInfo DiscountInfo { get; set; }
			private bool hasSkuItems = false;
			public bool HasSkuItems
			{
				get { return hasSkuItems; }
				set { hasSkuItems = value; }
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

		private static class VotingFacility
		{
			private static string totalValueCustomFieldName = "-_BX_RATING_VOTING_TOTAL_VALUE";
			private static string totalVotesCustomFieldName = "-_BX_RATING_VOTING_TOTAL_VOTES";

			//Добавляет пользовательские поля связанные с голосованием даже если их пока нет в контексте эл-тов данного инфоблока
			public static void PrepareSortingParamValues(List<BXParamValue> paramValues, CatalogueElementListComponent parent)
			{
				if (paramValues.FindIndex(delegate(BXParamValue obj) { return string.Equals(obj.Value, totalValueCustomFieldName); }) < 0)
					paramValues.Add(new BXParamValue(parent.GetMessageRaw("TotalValueCustomFieldEditFormLabel"), totalValueCustomFieldName));

				if (paramValues.FindIndex(delegate(BXParamValue obj) { return string.Equals(obj.Value, totalVotesCustomFieldName); }) < 0)
					paramValues.Add(new BXParamValue(parent.GetMessageRaw("TotalVotesCustomFieldEditFormLabel"), totalVotesCustomFieldName));
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

		public class CatalogDiscountInfo
		{
			public bool HasInfo { get; set; }
			public string Name { get; set; }
			public decimal Value { get; set; }
			public string DisplayHtml { get; set; }
		}

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

			private int priceTypeId = 0;
			public int PriceTypeId
			{
				get { return this.priceTypeId; }
			}

			private int currencyId = 0;
			public int CurrencyId
			{
				get { return this.priceTypeId; }
			}

			private decimal priceValue = 0;
			public decimal PriceValue
			{
				get { return this.priceValue; }
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
			/// <summary>
			/// НДС в цене
			/// </summary>
			public bool IsVATIncluded
			{
				get { return this.isVATIncluded; }
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
			/// <summary>
			/// Отпускная Цена
			/// </summary>
			public string SellingPriceHtml
			{
				get { return this.sellingPriceHtml; }
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
			/// <summary>
			/// НДС
			/// </summary>
			public string VATHtml
			{
				get { return this.vatHtml; }
			}
		}

		public class CatalogClientPriceTierInfo
		{
			public CatalogClientPriceTierInfo(int quantityFrom)
			{
				this.quantityFrom = quantityFrom;
			}

			private int quantityFrom = 0;
			public int QuantityFrom
			{
				get { return this.quantityFrom; }
			}

			private List<CatalogClientPriceInfo> items = null;
			public IList<CatalogClientPriceInfo> Items
			{
				get { return this.items ?? (this.items = new List<CatalogClientPriceInfo>()); }
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

		public class CatalogSkuItem
		{
			public CatalogSkuItem()
			{
			}

			public CatalogSkuItem(int id, int parId, string name, bool isInStock, CatalogClientPriceInfo currentPrice, int quantityInCart)
			{
				Id = id;
				ParentId = parId;
				Name = name;
				IsInStock = isInStock;
				QuantityInCart = quantityInCart;
				CurrentPrice = currentPrice;
			}

			private int id = 0;
			public int Id
			{
				get { return this.id; }
				set { this.id = value > 0 ? value : 0; }
			}

			private int parentId = 0;
			public int ParentId
			{
				get { return this.parentId; }
				set { this.parentId = value > 0 ? value : 0; }
			}

			private string name = string.Empty;
			public string Name
			{
				get { return this.name; }
				set { this.name = value ?? string.Empty; }
			}

			private bool isInStock;
			public bool IsInStock
			{
				get { return this.isInStock; }
				set { this.isInStock = value; }
			}

			private int quantityInCart;
			public int QuantityInCart
			{
				get { return this.quantityInCart; }
				set { this.quantityInCart = value > 0 ? value : 0; }
			}

			private CatalogClientPriceInfo currentPrice = null;
			public CatalogClientPriceInfo CurrentPrice
			{
				get { return this.currentPrice; }
				set { this.currentPrice = value; }
			}

			public string ToJson()
			{
				return string.Format("{{\"id\":{0},\"parentId\":{1},\"name\":\"{2}\",\"isInStock\":{3},sellingPriceHtml:\"{4}\",\"quantityInCart\":{5}}}", this.id.ToString(), this.parentId.ToString(), BXJSUtility.Encode(this.name), this.isInStock.ToString().ToLowerInvariant(), this.currentPrice != null ? BXJSUtility.Encode(this.currentPrice.SellingPriceHtml) : string.Empty, this.QuantityInCart.ToString());
			}
		}

		/// <summary>
		/// Торговый каталог
		/// </summary>
		public interface ICatalog
		{
			CatalogPriceTypeInfo[] GetPriceTypes();
			CatalogClientPriceInfoSet GetClientPriceSet(int itemId, int initQuantity, bool displayAllTiers, string[] userRoleNames, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes);
			CatalogClientPriceInfoSet GetClientPriceSet(BXIBlockElement item, int initQuantity, bool displayAllTiers, string[] userRoleNames, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes);
			bool CanPutInCart(int itemId);
			bool CanPutInCart(BXIBlockElement item);
			bool HasSkuItems(int itemId);
			bool HasSkuItems(BXIBlockElement item);
			CatalogDiscountInfo GetDiscount(int userId, IEnumerable<int> roleIds, int itemId, int priceTypeId,
				decimal price, int quantity, IList<int> sectionIds, string siteId, int currencyId);
			CatalogSkuItem[] GetSkuItems(int itemId, int initQuantity, bool displayAllTiers, int userId, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes);
			void PrepareElementSelect(BXSelect select);
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

		internal enum SaleCartActionResponseType
		{
			Html = 1,
			Json
		}

		internal class SaleCartActionHandler
		{
			public SaleCartActionHandler(CatalogueElementListComponent component)
			{
				if (component == null)
					throw new ArgumentNullException("component");

				this.component = component;
			}

			private CatalogueElementListComponent component = null;
			public CatalogueElementListComponent Component
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
					int id;
					if (string.IsNullOrEmpty(s = request.QueryString[Component.CatalogItemIdParamName]) || !int.TryParse(s, out id))
						return;
					int qty;
					if (string.IsNullOrEmpty(s = request.QueryString[Component.CatalogItemQuantityParamName]) || !int.TryParse(s, out qty))
						qty = 1;

					int count;
					bool isInCart = Component.AddToCart(id, qty, request.QueryString["detailUrl"], out count);

					HttpResponse response = Component.Response;
					if (responseType == SaleCartActionResponseType.Json)
					{
						response.ContentType = "text/x-json";
						response.StatusCode = (int)HttpStatusCode.OK;
						response.Write(string.Format("{{id:'{0}', action:'ADD2CART', result:{{id:{1}, isInCart:{2}, addedQty:{3}, totalCount:{4} }}}}", string.Concat(Component.ClientID, "_", id.ToString()), id.ToString(), isInCart.ToString().ToLowerInvariant(), qty.ToString(), count.ToString()));
					}
					else
						response.Redirect(BXSefUrlManager.CurrentUrl.AbsolutePath, false);

					if (HttpContext.Current != null)
						HttpContext.Current.ApplicationInstance.CompleteRequest();
					response.End();
				}
			}
		}

		internal static class CatalogActionHandler
		{
			public static void Process(CatalogueElementListComponent component)
			{
				if (component == null)
					return;

				if (!BXCsrfToken.CheckTokenFromRequest(component.Request.QueryString))
					return;

				HttpRequest request = component.Request;
				string act = request.QueryString[component.ActionParamName];
				if (string.IsNullOrEmpty(act))
					return;

				if (!string.Equals(act.ToUpperInvariant(), "SKUITEMS"))
					return;

				int elementId;
				string elementIdStr = request.QueryString["elementId"];
				if (!(!string.IsNullOrEmpty(elementIdStr) && int.TryParse(elementIdStr, out elementId) && elementId > 0))
					return;

				StringBuilder sb = new StringBuilder();
				if (component.Catalog != null)
					foreach (CatalogSkuItem skuItem in component.Catalog.GetSkuItems(elementId, component.InitQuantity, false, component.UserId, 0, component.IncludeVATInPrice, component.PriceTypes))
					{
						if (component.SaleCart != null)
							skuItem.QuantityInCart = component.SaleCart.GetQuantity(skuItem.Id, component.Request, component.Response);

						if (sb.Length > 0)
							sb.Append(",");
						sb.Append(skuItem.ToJson());
					}

				sb.Insert(0, "[").Append("]");

				HttpResponse response = component.Response;
				response.Clear();
				response.ContentType = "text/x-json";
				response.StatusCode = (int)HttpStatusCode.OK;
				response.Write(sb.ToString());

				if (HttpContext.Current != null)
					HttpContext.Current.ApplicationInstance.CompleteRequest();
				response.End();
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


	public class CatalogueElementListTemplate : BXComponentTemplate<CatalogueElementListComponent>
	{
		public static void PrepareEnvironment(bool displayCatalogData)
		{
			BXPage.Scripts.RequireUtils();
			BXPage.Scripts.RequireCore(); 

			if(displayCatalogData)
			{
				BXPage.RegisterScriptInclude("~/bitrix/controls/Main/dialog/js/messages.js.aspx?lang=" + HttpUtility.UrlEncode(BXLoc.CurrentLocale));
				BXPage.RegisterScriptInclude("~/bitrix/controls/Catalog/SkuSelector/messages.js.aspx?lang=" + HttpUtility.UrlEncode(BXLoc.CurrentLocale));
				
				BXPage.RegisterScriptInclude("~/bitrix/controls/Main/dialog/js/dialog_base.js");
				BXPage.RegisterScriptInclude("~/bitrix/controls/Catalog/SkuSelector/dialog.js");

				BXPage.RegisterStyle("~/bitrix/controls/Main/dialog/css/dialog_base.css");
				BXPage.RegisterStyle("~/bitrix/controls/Catalog/SkuSelector/dialog_styles.css");        
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			if (IsComponentDesignMode && Component.Items == null)
				writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
			else
				base.Render(writer);
		}

		protected string GetItemContainerClientID(int itemId)
		{
			return string.Concat(ClientID, ClientIDSeparator, "Item", itemId.ToString());
		}

		public void RenderElementToolbar(BXIBlockElement element, string containerClientID)
		{
			Component.CreateElementContextToolbar(element, containerClientID).Render(CurrentWriter);
		}
	}

}
