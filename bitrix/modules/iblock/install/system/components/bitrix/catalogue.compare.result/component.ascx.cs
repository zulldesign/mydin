using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.Services.Text;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.Security;
using System.Web.SessionState;
using System.Net;
using Bitrix.DataTypes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;

namespace Bitrix.IBlock.Components
{
	public partial class CatalogueCompareResultComponent : BXComponent
	{
		private static readonly string guidKey = "__BX_IBLOCK_COMPARE_RESULT_GUID__";

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			bool cached = IsCached(BXPrincipal.Current.GetAllRoles(true), ((BXIdentity)BXPrincipal.Current.Identity).Id);
			if(cached && Session != null)
			{
				string guid = Session[guidKey] as string, 
					cacheGuid;

				Results.TryGetString(guidKey, out cacheGuid);

				if(string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(cacheGuid) || !string.Equals(guid, cacheGuid, StringComparison.Ordinal))
				{
					ClearCache();
					cached = false;
				}
			}

			ActionHandler.Process(IBlockId, this);

			if (cached)
				return;

			using (BXTagCachingScope cacheScope = BeginTagCaching())
			{
				string diffOnly = Request[DefaultDifferenceOnlyParamName];
				if (!string.IsNullOrEmpty(diffOnly))
					DisplayDifferenceOnly = string.Equals(diffOnly, "Y");

				BXIBlock iblock = null;
				if (IBlockId > 0)
				{
					BXIBlockCollection iblockCol = BXIBlock.GetList(
						new BXFilter(
							new BXFilterItem(BXIBlock.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
							new BXFilterItem(BXIBlock.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y"),
							new BXFilterItem(BXIBlock.Fields.ID, BXSqlFilterOperators.Equal, IBlockId)),
							null, null, null, BXTextEncoder.HtmlTextEncoder);
					iblock = iblockCol.Count > 0 ? iblockCol[0] : null;
				}

				if (iblock == null)
					this.error = CatalogueCompareResultComponentError.IBlockIsNotFound;
				else
				{
					IBlockTypeId = iblock.TypeId;
					cacheScope.AddTag(ActionHandler.GetCacheTag(IBlockId));

					BXIBlockComparisonBlockSettings iblockData = ActionHandler.GetCurrentIBlockSettings(IBlockId, Session);
					if (iblockData != null)
					{
						BXOrderBy o = null;
						if (!string.IsNullOrEmpty(SortBy))
						{
							o = new BXOrderBy();
							if (SortBy.StartsWith("-", StringComparison.Ordinal))
							{
								string sortBy = SortBy.Substring(1).ToUpper();
								BXSchemeFieldBase f = BXIBlockElement.Fields.CustomFields[IBlockId].GetFieldByKey(sortBy);
								if (f != null)
									o.Add(f, SortOrder);
							}
							else
								o.Add(BXIBlockElement.Fields, SortBy, SortOrder);
						}
						BXIBlockElementCollection iblockElCol = BXIBlockElement.GetList(
							new BXFilter(
								new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
								new BXFilterItem(BXIBlockElement.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y"),
								new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.In, iblockData.IdList)),
							o);

						foreach (BXIBlockElement el in iblockElCol)
						{
							ElementData data = new ElementData(this, el);
							//Торговый каталог (отображается когда выбран тип цен)
							if (DisplayStockCatalogData && PriceTypes.Count > 0 && Catalog != null)
								data.ClientPriceSet = Catalog.GetClientPriceSet(el.Id, InitQuantity, false, UserId, 0, IncludeVATInPrice, PriceTypes);

							InternalElementDataList.Add(data);
						}

						if (string.IsNullOrEmpty(SortBy))
							InternalElementDataList.Sort(
								delegate(ElementData x, ElementData y)
								{
									return iblockData.IdList.IndexOf(x.Element.Id) - iblockData.IdList.IndexOf(y.Element.Id);
								});
					}
				}

				//Сохраняем GUID для валидации кеша 
				if(Session != null)
					Session[guidKey] = Results[guidKey] = Guid.NewGuid().ToString();

				IncludeComponentTemplate();
			}
		}

		private CatalogueCompareResultComponentError error = CatalogueCompareResultComponentError.None;
		public CatalogueCompareResultComponentError ComponentError
		{
			get { return this.error; }
		}

		public string[] BuildErrorMessages(bool encode)
		{
			if (this.error == CatalogueCompareResultComponentError.None)
				return new string[0];

			List<string> r = new List<string>();
			if ((this.error & CatalogueCompareResultComponentError.IBlockIsNotFound) != 0)
				r.Add(GetMessage("Error.IBlockIsNotFound", encode));
			return r.ToArray();
		}

		private int? iblockId = null;
		public int IBlockId
		{
			get { return (this.iblockId ?? (this.iblockId = Parameters.GetInt("IBlockId", 0))).Value; }
			set { Parameters["IBlockId"] = (this.iblockId = value).Value.ToString(); }
		}

		public int IBlockTypeId
		{
			get { return ComponentCache.GetInt("IBlockTypeID", 0); }
			set { ComponentCache["IBlockTypeID"] = value; }
		}

		private string sortBy = null;
		public string SortBy
		{
			get { return this.sortBy ?? (this.sortBy = Parameters.GetString("SortBy", string.Empty)); }
			set { Parameters["SortBy"] = this.sortBy = value ?? string.Empty; }
		}

		private string sortOrder = null;
		public string SortOrder
		{
			get { return this.sortOrder ?? (this.sortOrder = Parameters.GetString("SortOrder", "Desc")); }
			set { Parameters["SortOrder"] = this.sortOrder = value ?? string.Empty; }
		}

		private IList<string> selectedFields = null;
		/// <summary>
		/// Поля выбранные пользователем для отображения (поля сущности IBlockElement и пользовательские свойства)
		/// </summary>
		public IList<string> SelectedFields
		{
			get
			{
				if (this.selectedFields != null)
					return this.selectedFields;

				if ((this.selectedFields = Parameters.GetListString("SelectedFields", null)) == null)
					this.selectedFields = new List<string>();

				//if (this.showFields.Count == 0)
				//    this.showFields.Add("Name");

				return this.selectedFields;
			}
			set { Parameters["SelectedFields"] = BXStringUtility.ListToCsv((this.selectedFields = value ?? new List<string>())); }
		}

		List<string> diffOnlyFields = null;
		/// <summary>
		/// Отфильтрованный список SelectedFields (исключены поля с одинаковой информацией)
		/// </summary>
		public IList<string> DifferenceOnlyFields
		{
			get
			{
				if (this.diffOnlyFields != null)
					return this.diffOnlyFields;

				this.diffOnlyFields = new List<string>();

				if (InternalElementDataList.Count < 2)
				{
					this.diffOnlyFields.AddRange(SelectedFields);
					return this.diffOnlyFields;
				}

				foreach (string name in SelectedFields)
				{
					if (name.StartsWith("-", StringComparison.Ordinal))
						continue;

					string nameUc = name.ToUpperInvariant();
					if (string.Equals(nameUc, "PREVIEWTEXT", StringComparison.Ordinal))
					{
						bool eq = true;
						for (int i = 1; i < InternalElementDataList.Count && eq; i++)
							eq = string.Equals(InternalElementDataList[i].Element.PreviewText, InternalElementDataList[i - 1].Element.PreviewText);
						if (!eq)
							this.diffOnlyFields.Add(name);
					}
					else if (string.Equals(nameUc, "DETAILTEXT", StringComparison.Ordinal))
					{
						bool eq = true;
						for (int i = 1; i < InternalElementDataList.Count && eq; i++)
							eq = string.Equals(InternalElementDataList[i].Element.DetailText, InternalElementDataList[i - 1].Element.DetailText);
						if (!eq)
							this.diffOnlyFields.Add(name);
					}
					else if (string.Equals(nameUc, "ACTIVEFROMDATE", StringComparison.Ordinal))
					{
						bool eq = true;
						for (int i = 1; i < InternalElementDataList.Count && eq; i++)
							eq = InternalElementDataList[i].Element.ActiveFromDate == InternalElementDataList[i - 1].Element.ActiveFromDate;
						if (!eq)
							this.diffOnlyFields.Add(name);
					}
					else if (string.Equals(nameUc, "ACTIVETODATE", StringComparison.Ordinal))
					{
						bool eq = true;
						for (int i = 1; i < InternalElementDataList.Count && eq; i++)
							eq = InternalElementDataList[i].Element.ActiveToDate == InternalElementDataList[i - 1].Element.ActiveToDate;
						if (!eq)
							this.diffOnlyFields.Add(name);
					}
					else
						this.diffOnlyFields.Add(name);
				}

				foreach (string name in SelectedFields)
				{
					if (!name.StartsWith("-", StringComparison.Ordinal))
						continue;

					string nameUc = name.Substring(1).ToUpperInvariant();

					bool eq = true;
					for (int i = 1; i < InternalElementDataList.Count && eq; i++)
						eq = ElementCustomPropertyData.Equals(InternalElementDataList[i].GetPropertyData(nameUc), InternalElementDataList[i - 1].GetPropertyData(nameUc));
					if (!eq)
						this.diffOnlyFields.Add(name);
				}
				return this.diffOnlyFields;
			}
		}

		private bool displayDifferenceOnly = false;
		public bool DisplayDifferenceOnly
		{
			get { return this.displayDifferenceOnly; }
			private set { this.displayDifferenceOnly = value; }
		}

		public IList<string> DisplayFields
		{
			get { return displayDifferenceOnly ? DifferenceOnlyFields : SelectedFields; }
		}

		private List<CatalogPriceTypeInfo> priceTypeInfos = null;
		public IList<CatalogPriceTypeInfo> PriceTypeInfos
		{
			get
			{
				if (this.priceTypeInfos != null)
					return this.priceTypeInfos;

				this.priceTypeInfos = new List<CatalogPriceTypeInfo>();
				if (Catalog != null && PriceTypes.Count > 0)
					foreach (CatalogPriceTypeInfo info in Catalog.GetPriceTypes())
						if (PriceTypes.Contains(info.Id))
							this.priceTypeInfos.Add(info);

				return this.priceTypeInfos;
			}
		}

		public string ElementListUrlTemplate
		{
			get { return Parameters.GetString("ElementListUrlTemplate", "./list.aspx?id=#IBlockId#"); }
			set { Parameters["ElementListUrlTemplate"] = value ?? string.Empty; }
		}

		public string ElementUrlTemplate
		{
			get { return Parameters.GetString("ElementUrlTemplate", "./view.aspx?id=#ElementId#"); }
			set { Parameters["ElementUrlTemplate"] = value ?? string.Empty; }
		}

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
				path = VirtualPathUtility.ToAbsolute(Request.AppRelativeCurrentExecutionFilePath);
			else if (!VirtualPathUtility.IsAbsolute(path))
				path = VirtualPathUtility.ToAbsolute(VirtualPathUtility.Combine(Request.AppRelativeCurrentExecutionFilePath, path));

			return string.Concat(path, query);
		}

		private BXParamsBag<object> replaceItems;
		public BXParamsBag<object> ReplaceItems
		{
			get { return this.replaceItems ?? (this.replaceItems = new BXParamsBag<object>()); }
		}

		public string GetElementUrl(BXIBlockElement element)
		{
			if (element == null)
				return string.Empty;

			BXParamsBag<object> repl = ReplaceItems;
			repl.Clear();

			repl.Add("IblockId", element.IBlockId.ToString());
			repl.Add("BlockId", element.IBlockId.ToString());

			repl.Add("ElementId", element.Id.ToString());
			repl.Add("Id", element.Id.ToString());

			repl.Add("ElementCode", element.Code.ToString());
			repl.Add("Code", element.Code.ToString());

			int sectionId = 0;
			string sectionCode = string.Empty;

			BXIBlockSectionCollection sections = element.GetSections();
			if (sections != null && sections.Count != 0)
				foreach (BXIBlockSection s in sections)
				{
					if (!s.ActiveGlobal)
						continue;

					sectionId = s.Id;
					sectionCode = s.Code;
					break;
				}

			repl.Add("SectionId", sectionId);
			repl.Add("SectionCode", sectionCode);

			return PrepareUrl(MakeLink(ElementUrlTemplate, repl));
		}

		public string GetElementListUrl()
		{
			BXParamsBag<object> repl = ReplaceItems;
			repl.Clear();

			repl.Add("IblockId", IBlockId.ToString());
			repl.Add("BlockId", IBlockId.ToString());

			return PrepareUrl(MakeLink(ElementListUrlTemplate, repl));
		}

		public string GetDeleteAllElementsUrl(bool jsonResponse)
		{
			return ActionHandler.GetDeleteAllElemenstUrl(jsonResponse ? CatalogueCompareResultResponseType.Json : CatalogueCompareResultResponseType.Html);
		}

		public string GetDisplayDifferenceOnlyUrl(bool enable)
		{
			Uri uri = BXSefUrlManager.CurrentUrl;
			NameValueCollection qsParams = HttpUtility.ParseQueryString(uri.Query);
			qsParams[DefaultDifferenceOnlyParamName] = enable ? "Y" : "N";

			return string.Concat(uri.AbsolutePath, "?", qsParams.ToString());
		}

		public string DefaultDifferenceOnlyParamName
		{
			get { return "diff"; }
		}

		private BXCustomFieldCollection iblockCustomFields = null;
		public BXCustomFieldCollection IBlockCustomFields
		{
			get { return this.iblockCustomFields ?? (this.iblockCustomFields = IBlockId > 0 ? BXIBlock.GetCustomFields(IBlockId) : new BXCustomFieldCollection()); }
		}

		public string GetCustomPropertyDisplayName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return string.Empty;

			if (name.StartsWith("-", StringComparison.Ordinal))
				name = name.Substring(1);

			BXCustomFieldCollection col = IBlockCustomFields;
			int dashInd = name.IndexOf("-", StringComparison.Ordinal);
			if (dashInd >= 0)
			{
				int blockId;
				if (!int.TryParse(name.Substring(0, dashInd), out blockId) || blockId != IBlockId)
					col = BXCustomEntityManager.GetFields(BXIBlockElement.GetCustomFieldsKey(blockId));
				name = name.Substring(dashInd + 1);
			}

			name = name.ToUpperInvariant();
			foreach (BXCustomField f in col)
				if (string.Equals(f.Name.ToUpperInvariant(), name, StringComparison.Ordinal))
					return f.EditFormLabel;

			return string.Empty;
		}

		public bool DisplayStockCatalogData
		{
			get { return Parameters.GetBool("DisplayStockCatalogData", false); }
			set { Parameters["DisplayStockCatalogData"] = value.ToString(); }
		}

		private IList<int> priceTypes = null;
		public IList<int> PriceTypes
		{
			get { return this.priceTypes ?? (this.priceTypes = Parameters.GetListInt("PriceTypes", null) ?? new List<int>()); }
			set
			{

				this.priceTypes = value ?? new List<int>();
				List<string> lst = new List<string>();
				foreach (int i in this.priceTypes)
					lst.Add(i.ToString());

				Parameters["PriceTypes"] = BXStringUtility.ListToCsv(lst);
			}
		}

		public int InitQuantity
		{
			get
			{
				int r = Parameters.GetInt("InitQuantity", 1);
				return r > 0 ? r : 1;
			}
			set { Parameters["InitQuantity"] = (value > 0 ? value : 1).ToString(); }
		}

		public bool IncludeVATInPrice
		{
			get { return Parameters.GetBool("IncludeVATInPrice", true); }
			set { Parameters["IncludeVATInPrice"] = value.ToString(); }
		}

		public string UserCartUrlTemplate
		{
			get { return Parameters.GetString("UserCartUrlTemplate", "personal/cart.aspx"); }
			set { Parameters["UserCartUrlTemplate"] = value ?? string.Empty; }
		}

		private List<ElementData> elementDataList = null;
		internal List<ElementData> InternalElementDataList
		{
			get { return this.elementDataList ?? (this.elementDataList = new List<ElementData>()); }
		}

		private ReadOnlyCollection<ElementData> elementDataListRO = null;
		public IList<ElementData> ElementDataList
		{
			get { return this.elementDataListRO ?? (this.elementDataListRO = new ReadOnlyCollection<ElementData>(InternalElementDataList)); }
		}

		private int UserId
		{
			get
			{
				BXIdentity identity = (BXIdentity)BXPrincipal.Current.Identity;
				return identity != null ? identity.Id : 0;
			}
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/iblock_compare_tbl.gif";

			Group = new BXComponentGroup("catalogue", GetMessageRaw("Group"), 10, BXComponentGroup.Content);

			BXCategory mainCategory = BXCategory.Main,
				addSettingsCategory = BXCategory.AdditionalSettings,
				dataSourceCategory = BXCategory.DataSource,
				stockCatalogCategory = new BXCategory(GetMessageRaw("Category.StockCatalog"), "StockCatalog", 1000);

			#region Main
			ParamsDefinition.Add(
				"IBlockTypeId",
				new BXParamSingleSelection(
					GetMessageRaw("Param.InfoBlockType"), string.Empty, mainCategory));

			ParamsDefinition.Add(
				"IBlockId",
				new BXParamSingleSelection(
					GetMessageRaw("Param.InfoBlockCode"), string.Empty, mainCategory));
			#endregion

			#region DataSourceCategory
			ParamsDefinition.Add(
				"SortBy",
				new BXParamSingleSelection(
					GetMessageRaw("Param.SortBy"), string.Empty, dataSourceCategory));

			ParamsDefinition.Add(
				"SortOrder",
				new BXParamSort(
					GetMessageRaw("Param.SortOrder"), true, dataSourceCategory));
			#endregion

			#region AdditionalSettings
			ParamsDefinition.Add(
				"ElementUrlTemplate",
				new BXParamText(
					GetMessageRaw("Param.ElementUrlTemplate"),
					"./view.aspx?id=#ElementId#",
					addSettingsCategory
				));

			ParamsDefinition.Add(
				"ElementListUrlTemplate",
				new BXParamText(
					GetMessageRaw("Param.ElementListUrlTemplate"),
					"./list.aspx?id=#IBlockId#",
					addSettingsCategory
				));

			ParamsDefinition.Add(
				"SelectedFields",
				new BXParamMultiSelection(
					GetMessageRaw("Param.SelectedFields"), string.Empty, addSettingsCategory));

			#endregion

			#region StockCatalog
			ParamsDefinition.Add(
				"DisplayStockCatalogData",
				new BXParamYesNo(
					GetMessageRaw("Param.DisplayStockCatalogData"),
					true,
					stockCatalogCategory,
					new ParamClientSideActionGroupViewSwitch(
							ClientID, "DisplayStockCatalogData", "StockCatalog", string.Empty)));

			ParamsDefinition.Add(
				"PriceTypes",
				new BXParamMultiSelection(
					GetMessageRaw("Param.PriceTypes"),
					string.Empty,
					stockCatalogCategory,
					null,
					new ParamClientSideActionGroupViewMember(
						ClientID, "PriceTypes", new string[] { "StockCatalog" })));

			ParamsDefinition.Add(
				"InitQuantity",
				new BXParamText(
					GetMessageRaw("Param.InitQuantity"),
					"1",
					stockCatalogCategory,
					new ParamClientSideActionGroupViewMember(
						ClientID, "InitQuantity", new string[] { "StockCatalog" })));

			ParamsDefinition.Add(
				"IncludeVATInPrice",
				new BXParamYesNo(
					GetMessageRaw("Param.IncludeVATInPrice"),
					true,
					stockCatalogCategory,
					new ParamClientSideActionGroupViewMember(
						ClientID, "IncludeVATInPrice", new string[] { "StockCatalog" })));

			ParamsDefinition.Add(
				"UserCartUrlTemplate",
				new BXParamText(
					GetMessageRaw("Param.UserCartUrlTemplate"),
					"personal/cart.aspx",
					stockCatalogCategory,
					new ParamClientSideActionGroupViewMember(
						ClientID, "UserCartUrlTemplate", new string[] { "StockCatalog" })));
			#endregion
		}

		protected override void LoadComponentDefinition()
		{
			#region IBlockType
			List<BXParamValue> typeValues = new List<BXParamValue>();
			typeValues.Add(new BXParamValue(GetMessageRaw("SelectIBlockType"), string.Empty));

			BXIBlockTypeCollection iblockTypes = BXIBlockType.GetList(
				null,
				new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Name, BXOrderByDirection.Asc)),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder);

			foreach (BXIBlockType iblockType in iblockTypes)
				typeValues.Add(new BXParamValue(iblockType.Translations[BXLoc.CurrentLocale].Name, iblockType.Id.ToString()));

			ParamsDefinition["IBlockTypeId"].Values = typeValues;
			ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;
			#endregion

			#region IBlock
			int selectedIBlockType = 0;
			if (Parameters.ContainsKey("IBlockTypeId"))
				int.TryParse(Parameters["IBlockTypeId"], out selectedIBlockType);

			BXFilter filter = new BXFilter();
			if (selectedIBlockType > 0)
				filter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, selectedIBlockType));
			if (!string.IsNullOrEmpty(DesignerSite))
				filter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

			List<BXParamValue> iblockValues = new List<BXParamValue>();
			iblockValues.Add(new BXParamValue(GetMessageRaw("SelectIBlockID"), string.Empty));
			BXIBlockCollection iblocks = BXIBlock.GetList(
				filter,
				new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Name, BXOrderByDirection.Asc)),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder);

			foreach (BXIBlock iblock in iblocks)
				iblockValues.Add(new BXParamValue(iblock.Name, iblock.Id.ToString()));

			ParamsDefinition["IBlockId"].Values = iblockValues;
			ParamsDefinition["IBlockId"].RefreshOnDirty = true;
			#endregion

			#region SelectedFields
			List<BXParamValue> showFieldValues = new List<BXParamValue>();
			showFieldValues.Add(new BXParamValue(GetMessageRaw("Field.ElementID"), "ID"));
			//showFieldValues.Add(new BXParamValue(GetMessageRaw("Field.ElementName"), "Name"));  /*отображается всегда*/
			showFieldValues.Add(new BXParamValue(GetMessageRaw("Field.PreviewText"), "PreviewText"));
			showFieldValues.Add(new BXParamValue(GetMessageRaw("Field.PreviewImage"), "PreviewImage"));
			showFieldValues.Add(new BXParamValue(GetMessageRaw("Field.DetailText"), "DetailText"));
			showFieldValues.Add(new BXParamValue(GetMessageRaw("Field.DetailImage"), "DetailImage"));
			showFieldValues.Add(new BXParamValue(GetMessageRaw("Field.ActiveFromDate"), "ActiveFromDate"));
			showFieldValues.Add(new BXParamValue(GetMessageRaw("Field.ActiveToDate"), "ActiveToDate"));

			ParamsDefinition["SelectedFields"].Values = showFieldValues;
			#endregion

			#region Sorting
			List<BXParamValue> sortingFields = new List<BXParamValue>();
			sortingFields.Add(new BXParamValue(GetMessageRaw("NotSelected"), string.Empty));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.Sort"), "Sort"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.UpdateDate"), "UpdateDate"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.ElementName"), "Name"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.ElementID"), "ID"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.ActiveFromDate"), "ActiveFromDate"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.ActiveToDate"), "ActiveToDate"));

			ParamsDefinition["SortBy"].Values = sortingFields;
			#endregion

			#region Add Custom Properties
			int selectedIBlockId;
			if (Parameters.TryGetInt("IBlockId", out selectedIBlockId) && selectedIBlockId > 0)
			{
				BXCustomFieldCollection customFields = BXIBlock.GetCustomFields(selectedIBlockId);
				foreach (BXCustomField customField in customFields)
				{
					string title = BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel),
						code = string.Concat("-", customField.Name.ToUpper());

					ParamsDefinition["SelectedFields"].Values.Add(new BXParamValue(title, code));
					ParamsDefinition["SortBy"].Values.Add(new BXParamValue(title, code));
				}
			}
			#endregion

			#region PriceTypes
			if (Catalog != null)
			{
				List<BXParamValue> priceTypeValues = new List<BXParamValue>();
				foreach (CatalogPriceTypeInfo priceType in Catalog.GetPriceTypes())
					priceTypeValues.Add(new BXParamValue(priceType.Name, priceType.Id.ToString()));
				ParamsDefinition["PriceTypes"].Values = priceTypeValues;
			}
			#endregion
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
				string vatText
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
			}

			private int priceTypeId = 0;
			public int PriceTypeId
			{
				get { return this.priceTypeId; }
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
		/// <summary>
		/// Торговый каталог
		/// </summary>
		public interface ICatalog
		{
			CatalogPriceTypeInfo[] GetPriceTypes();
			CatalogClientPriceInfoSet GetClientPriceSet(int itemId, int initQuantity, bool displayAllTiers, int userId, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes);
		}

		private static bool? isCatalogModuleInstalled = null;
		private static bool IsCatalogModuleInstalled
		{
			get { return (isCatalogModuleInstalled ?? (isCatalogModuleInstalled = Bitrix.Modules.BXModuleManager.IsModuleInstalled("catalog"))).Value; }
		}

		internal ICatalog Catalog
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

		//internal class ComparisonIBlockSettings
		//{
		//    public ComparisonIBlockSettings()
		//    {
		//    }

		//    public ComparisonIBlockSettings(int iblockId)
		//    {
		//        this.iblockId = iblockId;
		//    }

		//    private int iblockId = 0;
		//    public int IBlockId
		//    {
		//        get { return this.iblockId; }
		//        set { this.iblockId = value; }
		//    }

		//    private List<int> idList = null;
		//    public IList<int> IdList
		//    {
		//        get { return this.idList ?? (this.idList = new List<int>()); }
		//    }

		//    public void CopyTo(ComparisonIBlockSettings inst)
		//    {
		//        if (inst == null)
		//            throw new ArgumentNullException("inst");

		//        inst.IBlockId = IBlockId;

		//        if (inst.IdList.Count > 0)
		//            inst.IdList.Clear();

		//        foreach (int id in IdList)
		//            inst.IdList.Add(id);
		//    }

		//    public ComparisonIBlockSettings Clone()
		//    {
		//        ComparisonIBlockSettings r = new ComparisonIBlockSettings();
		//        CopyTo(r);
		//        return r;
		//    }
		//}
		//internal class ComparisonSettings
		//{
		//    private List<ComparisonIBlockSettings> iblockDataList = null;
		//    public IList<ComparisonIBlockSettings> IBlockDataList
		//    {
		//        get { return this.iblockDataList ?? (this.iblockDataList = new List<ComparisonIBlockSettings>()); }
		//    }

		//    public ComparisonIBlockSettings FindIBlockData(int iblockId)
		//    {
		//        if (iblockId <= 0 || this.iblockDataList == null)
		//            return null;

		//        for (int i = 0; i < this.iblockDataList.Count; i++)
		//            if (this.iblockDataList[i].IBlockId == iblockId)
		//                return this.iblockDataList[i];

		//        return null;
		//    }

		//    public void CopyTo(ComparisonSettings inst)
		//    {
		//        if (inst == null)
		//            throw new ArgumentNullException("inst");

		//        if (inst.IBlockDataList.Count > 0)
		//            inst.IBlockDataList.Clear();

		//        foreach (ComparisonIBlockSettings iblockData in IBlockDataList)
		//            inst.IBlockDataList.Add(iblockData.Clone());
		//    }



		//    public ComparisonSettings Clone()
		//    {
		//        ComparisonSettings r = new ComparisonSettings();
		//        CopyTo(r);
		//        return r;
		//    }
		//}

		internal enum CatalogueCompareResultResponseType
		{
			Html = 1,
			Json
		}
		internal static class ActionHandler
		{
			private static object sync = new object();
			public static BXIBlockComparisonSettings GetCurrentSettings(HttpSessionState session)
			{
				if (session == null)
					throw new ArgumentNullException("session");

				lock (sync)
				{
					BXIBlockComparisonSettings settings = session[BXIBlockComparisonHelper.SettingsKey] as BXIBlockComparisonSettings;
					if (settings != null)
						return settings.Clone();
				}
				return null;
			}

			public static string GetDeleteElementUrl(int id, CatalogueCompareResultResponseType responseType)
			{
				return BXIBlockComparisonHelper.GetDeleteElementUrl(id, null, responseType == CatalogueCompareResultResponseType.Json);
			}
			public static string GetDeleteAllElemenstUrl(CatalogueCompareResultResponseType responseType)
			{
				return BXIBlockComparisonHelper.GetDeleteAllElemenstUrl(null, responseType == CatalogueCompareResultResponseType.Json);
			}

			public static BXIBlockComparisonBlockSettings GetCurrentIBlockSettings(int iblockId, HttpSessionState session)
			{
				if (session == null)
					throw new ArgumentNullException("session");

				lock (sync)
				{
					BXIBlockComparisonSettings settings = session[BXIBlockComparisonHelper.SettingsKey] as BXIBlockComparisonSettings;
					if (settings != null)
						return settings.FindIBlockData(iblockId);
				}
				return null;
			}

			public static void Process(int iblockId, CatalogueCompareResultComponent component)
			{
				if (iblockId <= 0 || component == null)
					return;

				HttpRequest request = component.Request;
				HttpResponse response = component.Response;
				HttpSessionState session = component.Session;

				if (request == null || response == null || session == null)
					return;

				if (!BXCsrfToken.CheckTokenFromRequest(request.QueryString))
					return;

				CatalogueCompareResultResponseType responseType =
					string.Equals(request.QueryString[BXIBlockComparisonHelper.DefaultResponseTypeParamName], "JSON", StringComparison.OrdinalIgnoreCase) ?
						CatalogueCompareResultResponseType.Json : CatalogueCompareResultResponseType.Html;

				string act = request.QueryString[BXIBlockComparisonHelper.DefaultActionParamName];
				if (string.IsNullOrEmpty(act))
					return;

				act = act.ToUpperInvariant();

				string responseStr = string.Empty;
				if (string.Equals(BXIBlockComparisonHelper.AddActionName, act, StringComparison.Ordinal))
				{
					int count = 0;
					int id = GetElementId(request);
					if (id > 0)
						lock (sync)
						{
							BXIBlockComparisonSettings settings = session[BXIBlockComparisonHelper.SettingsKey] as BXIBlockComparisonSettings;
							if (settings == null)
								session[BXIBlockComparisonHelper.SettingsKey] = settings = new BXIBlockComparisonSettings();

							BXIBlockComparisonBlockSettings iblockData = settings.FindIBlockData(iblockId);
							if (iblockData == null)
								settings.IBlockDataList.Add(iblockData = new BXIBlockComparisonBlockSettings(iblockId));

							if (iblockData.IdList.IndexOf(id) < 0)
								iblockData.IdList.Add(id);

							count = iblockData.IdList.Count;
						}
					if (responseType == CatalogueCompareResultResponseType.Json)
						responseStr = string.Format("{{blockId:{0}, elementId:{1}, action:'{2}', result:{{totalCount:{3}}}}}", iblockId.ToString(), id.ToString(), BXIBlockComparisonHelper.AddActionName, count.ToString());
				}
				else if (string.Equals(BXIBlockComparisonHelper.RemoveActionName, act, StringComparison.Ordinal))
				{
					int count = 0;
					int id = GetElementId(request);
					if (id > 0)
						lock (sync)
						{
							BXIBlockComparisonSettings settings = session[BXIBlockComparisonHelper.SettingsKey] as BXIBlockComparisonSettings;
							if (settings != null)
							{
								BXIBlockComparisonBlockSettings iblockData = settings.FindIBlockData(iblockId);
								if (iblockData != null)
								{
									int index;
									if ((index = iblockData.IdList.IndexOf(id)) >= 0)
										iblockData.IdList.RemoveAt(index);

									count = iblockData.IdList.Count;
								}
							}
						}
					if (responseType == CatalogueCompareResultResponseType.Json)
						responseStr = string.Format("{{blockId:{0}, elementId:{1}, action:'{2}', result:{{totalCount:{3}}}}}", iblockId.ToString(), id.ToString(), BXIBlockComparisonHelper.RemoveActionName, count.ToString());
				}
				else if (string.Equals(BXIBlockComparisonHelper.RemoveAllActionName, act, StringComparison.Ordinal))
				{
					int count = 0;
					lock (sync)
					{
						BXIBlockComparisonSettings settings = session[BXIBlockComparisonHelper.SettingsKey] as BXIBlockComparisonSettings;
						if (settings != null)
						{
							BXIBlockComparisonBlockSettings iblockData = settings.FindIBlockData(iblockId);
							if (iblockData != null)
								iblockData.IdList.Clear();
						}
					}
					if (responseType == CatalogueCompareResultResponseType.Json)
						responseStr = string.Format("{{blockId:{0}, action:'{1}', result:{{totalCount:{2}}}}}", iblockId.ToString(), BXIBlockComparisonHelper.RemoveAllActionName, count.ToString());
				}
				else
					return;

				BXComponent.ClearCacheByTags(new string[] { GetCacheTag(iblockId) });
				//component.ClearCache();

				if (responseType == CatalogueCompareResultResponseType.Json)
				{
					response.ContentType = "text/x-json";
					response.StatusCode = (int)HttpStatusCode.OK;
					response.Write(responseStr);
				}
				else
				{
					Uri uri = BXSefUrlManager.CurrentUrl;
					NameValueCollection qsParams = HttpUtility.ParseQueryString(uri.Query);
					qsParams.Remove(BXCsrfToken.TokenKey);
					qsParams.Remove(BXIBlockComparisonHelper.DefaultActionParamName);
					qsParams.Remove(BXIBlockComparisonHelper.DefaultResponseTypeParamName);
					qsParams.Remove(BXIBlockComparisonHelper.DefaultElementIdParamName);

					response.Redirect(qsParams.Count > 0 ? string.Concat(uri.AbsolutePath, "?", qsParams.ToString()) : uri.AbsolutePath, false);
				}

				if (HttpContext.Current != null)
					HttpContext.Current.ApplicationInstance.CompleteRequest();
				response.End();
			}

			private static int GetElementId(HttpRequest request)
			{
				int r;
				string s = request.QueryString[BXIBlockComparisonHelper.DefaultElementIdParamName];
				return !string.IsNullOrEmpty(s) && int.TryParse(s, out r) && r > 0 ? r : 0;
			}

			public static string GetCacheTag(int iblockId)
			{
				return string.Concat("iblockComparison:iblockId=", iblockId.ToString(), ",userId=", ((BXIdentity)BXPrincipal.Current.Identity).Id.ToString());
			}
		}

		public class ElementCustomPropertyData
		{
			private BXCustomProperty property = null;
			public ElementCustomPropertyData(BXCustomProperty property)
			{
				if (property == null)
					throw new ArgumentNullException("property");

				this.property = property;
			}

			/// <summary>
			/// Имя 
			/// (HTML-кодирование)
			/// </summary>
			public string Name
			{
				get { return HttpUtility.HtmlEncode(this.property.UserLikeName); }
			}

			public string GetHtml()
			{
				return this.property.ToHtml();
			}

			public static bool Equals(ElementCustomPropertyData a, ElementCustomPropertyData b)
			{
				if (a == null && b == null)
					return true;

				if ((a != null && b == null) || (a == null && b != null))
					return false;

				BXCustomProperty aPr = a.property,
					bPr = b.property;

				if (!aPr.HasValues && !bPr.HasValues) //нет значений
					return true;

				if (aPr.DbType != bPr.DbType)
					return false;

				if (aPr.IsMultiple != bPr.IsMultiple) //одно множественное, другое нет
					return false;

				if (!aPr.IsMultiple)
					return CompareValues(aPr.Value, bPr.Value, aPr.DbType) == 0;

				if (aPr.Values.Count != aPr.Values.Count) //разное кол-во значений
					return false;

				bool equals = true;
				for (int i = 0; i < aPr.Values.Count && equals; i++)
				{
					bool found = false;
					for (int j = 0; j < bPr.Values.Count && !found; j++)
						found = CompareValues(aPr.Values[i], bPr.Values[j], aPr.DbType) == 0;

					equals = found;
				}
				return equals;
			}

			private static int CompareValues(object a, object b, SqlDbType dbType)
			{
				if (a == null && b == null)
					return 0;

				if (a != null && b == null)
					return 1;

				if (a == null && b != null)
					return -1;

				switch (dbType)
				{
					case SqlDbType.BigInt:
						return ConvertToInt64(a).CompareTo(ConvertToInt64(b));
					case SqlDbType.Int:
						return ConvertToInt32(a).CompareTo(ConvertToInt32(b));
					case SqlDbType.Bit:
						return ConvertToBoolean(a).CompareTo(ConvertToBoolean(b));
					case SqlDbType.Date:
					case SqlDbType.DateTime:
					case SqlDbType.DateTime2:
						return ConvertToDateTime(a).CompareTo(ConvertToDateTime(b));
					case SqlDbType.Float:
						return ConvertToDouble(a).CompareTo(ConvertToDouble(b));
					case SqlDbType.Decimal:
						return ConvertToDecimal(a).CompareTo(ConvertToDecimal(b));
					case SqlDbType.Real:
						return ConvertToSingle(a).CompareTo(ConvertToSingle(b));
					case SqlDbType.UniqueIdentifier:
						return ConvertToGuid(a).CompareTo(ConvertToGuid(b));
					case SqlDbType.Char:
					case SqlDbType.NText:
					case SqlDbType.NVarChar:
					case SqlDbType.Text:
						return ConvertToString(a).CompareTo(ConvertToString(b));
					default:
						throw new NotSupportedException();
				}
			}

			private static long ConvertToInt64(object obj)
			{
				try
				{
					return Convert.ToInt64(obj);
				}
				catch
				{
					return 0L;
				}
			}

			private static int ConvertToInt32(object obj)
			{
				try
				{
					return Convert.ToInt32(obj);
				}
				catch
				{
					return 0;
				}
			}

			private static bool ConvertToBoolean(object obj)
			{
				try
				{
					return Convert.ToBoolean(obj);
				}
				catch
				{
					return false;
				}
			}

			private static DateTime ConvertToDateTime(object obj)
			{
				try
				{
					return Convert.ToDateTime(obj);
				}
				catch
				{
					return DateTime.MinValue;
				}
			}

			private static double ConvertToDouble(object obj)
			{
				try
				{
					return Convert.ToDouble(obj);
				}
				catch
				{
					return 0D;
				}
			}

			private static decimal ConvertToDecimal(object obj)
			{
				try
				{
					return Convert.ToDecimal(obj);
				}
				catch
				{
					return 0M;
				}
			}

			private static float ConvertToSingle(object obj)
			{
				try
				{
					return Convert.ToSingle(obj);
				}
				catch
				{
					return 0F;
				}
			}

			private static Guid ConvertToGuid(object obj)
			{
				try
				{
					return new Guid(obj.ToString());
				}
				catch
				{
					return Guid.Empty;
				}
			}

			private static string ConvertToString(object obj)
			{
				return Convert.ToString(obj);
			}
		}

		public class ElementData
		{
			private CatalogueCompareResultComponent component = null;
			private BXIBlockElement element = null;
			internal BXIBlockElement Element
			{
				get { return this.element; }
			}

			public ElementData(CatalogueCompareResultComponent component, BXIBlockElement element)
			{
				if (component == null)
					throw new ArgumentNullException("component");

				if (element == null)
					throw new ArgumentNullException("element");

				this.component = component;
				this.element = element;
			}

			public string ID
			{
				get { return this.element.Id.ToString(); }
			}

			/// <summary>
			/// Имя (HTML-кодирование)
			/// </summary>
			public string Name
			{
				get { return this.element.Name; }
			}

			private string url = null;
			/// <summary>
			/// URL эл-та
			/// </summary>
			public string DetailUrl
			{
				get { return this.url ?? (this.url = this.component.GetElementUrl(this.element)); }
			}

			public string GetDeleteUrl(bool jsonResponse)
			{
				return ActionHandler.GetDeleteElementUrl(this.element.Id, jsonResponse ? CatalogueCompareResultResponseType.Json : CatalogueCompareResultResponseType.Html);
			}

			public string PreviewText
			{
				get { return this.element.PreviewText; }
			}

			public string PreviewImageUrl
			{
				get { return this.element.PreviewImage != null ? this.element.TextEncoder.Decode(this.element.PreviewImage.FilePath) : string.Empty; }
			}

			public string DetailText
			{
				get { return this.element.DetailText; }
			}

			public string DetailImageUrl
			{
				get { return this.element.DetailImage != null ? this.element.TextEncoder.Decode(this.element.DetailImage.FilePath) : string.Empty; }
			}

			public string ActiveFromDate
			{
				get { return this.element.ActiveFromDate != DateTime.MinValue ? this.element.ActiveFromDate.ToString("d") : string.Empty; }
			}

			public string ActiveToDate
			{
				get { return this.element.ActiveToDate != DateTime.MinValue ? this.element.ActiveToDate.ToString("d") : string.Empty; }
			}

			public ElementCustomPropertyData GetPropertyData(string name)
			{
				if (string.IsNullOrEmpty(name))
					return null;

				if (name.StartsWith("-", StringComparison.Ordinal))
					name = name.Substring(1);

				int dashInd = name.IndexOf("-", StringComparison.Ordinal);
				if (dashInd >= 0)
				{
					int blockId;
					if (!int.TryParse(name.Substring(0, dashInd), out blockId) || blockId != this.element.IBlockId)
						return null;

					name = name.Substring(dashInd + 1);
				}

				BXCustomProperty p;
				return this.element.CustomPublicValues.TryGetCustomProperty(name, out p) && p != null ? new ElementCustomPropertyData(p) : null;
			}

			private CatalogClientPriceInfoSet clientPriceSet = null;
			public CatalogClientPriceInfoSet ClientPriceSet
			{
				get { return this.clientPriceSet; }
				set { this.clientPriceSet = value; }
			}

			public CatalogClientPriceInfo GetPriceInfoByPriceTypeId(int id)
			{
				return this.clientPriceSet != null ? this.clientPriceSet.GetPriceInfoByPriceTypeId(id) : null;
			}
		}
	}

	public partial class CatalogueCompareResultTemplate : BXComponentTemplate<CatalogueCompareResultComponent>
	{
		protected override void Render(HtmlTextWriter writer)
		{
			if (IsComponentDesignMode && (Component.IBlockTypeId <= 0 || Component.IBlockId <= 0))
				writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
			else
				base.Render(writer);
		}
	}

	[FlagsAttribute]
	public enum CatalogueCompareResultComponentError
	{
		None = 0x0,
		IBlockIsNotFound = 0x1
	}
}