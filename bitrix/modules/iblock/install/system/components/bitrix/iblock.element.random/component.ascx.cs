using System;
using System.Data;
using System.Configuration;
using System.Collections;
using Bitrix.Components;
using Bitrix.Security;
using System.Collections.Generic;
using Bitrix.Modules;
using Bitrix.UI;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using System.Web;
using Bitrix.Services.Text;
using Bitrix.Services;
using System.Data.SqlClient;
using Bitrix.IO;
using System.Web.UI;

namespace Bitrix.IBlock.Components
{
	[Flags]
	public enum IBlockElementRandomComponentError
	{
		None = 0,
		ErrorSelectingElement
	}

	public partial class IBlockElementRandomComponent : BXComponent
	{
		IBlockElementRandomComponentError error;

		public IBlockElementRandomComponentError Error
		{
			get { return error; }
			set { error = value; }
		}

		public string GetErrorMessage()
		{
			return GetMessage("Error." + error.ToString());
		}

		bool TryFillElements(int iblockId)
		{
			
			var field = new BXCalculatedField("random", SqlDbType.UniqueIdentifier, null,
				delegate(BXOrderByDirection direction)
				{
					return new KeyValuePair<string, SqlParameter[]>("newid()", new SqlParameter[0]);
				},
					null);

			try
			{

				var elementsCol = BXIBlockElement.GetList(
						new BXFilter(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId),
									new BXFilterItem(BXIBlockElement.Fields.GetCustomField(iblockId, FilterCode), BXSqlFilterOperators.Equal, true)),
						new BXOrderBy(new BXOrderByPair(field, BXOrderByDirection.Asc)),
						new BXSelectAdd(BXIBlockElement.Fields.CustomFields[iblockId].GetFieldByKey(PhotoCode),
										BXIBlockElement.Fields.IBlock.Code,
										BXIBlockElement.Fields.Sections),
						new BXQueryParams(new BXPagingOptions(1, ElementsCount))
					);

				BXParamsBag<object> replace = new BXParamsBag<object>();

				if(elementsCol.Count == 0) 
					return false;

				foreach (var el in elementsCol)
				{

					replace.Add("IBlockId", el.IBlockId);
					replace.Add("IBlockCode", el.IBlock.Code);
					replace.Add("ElementId", el.Id);
					replace.Add("SectionId", el.Sections.Count > 0 ? el.Sections[0].SectionId : 0);

					var priceSet = GetClientPriceSet(el);
					var priceHtml = priceSet != null ? priceSet.CurrentSellingPrice != null ? priceSet.CurrentSellingPrice.SellingPriceHtml : String.Empty : String.Empty;

					Elements.Add(new IBlockElementWrapper(el,
						ResolveTemplateUrl(IBlockElementUrlTemplate, replace),
						GetElementImage(el),
						priceHtml)
						);

					replace.Clear();
				}
				return true;

			}
			catch (Exception ex)
			{
				error = IBlockElementRandomComponentError.ErrorSelectingElement;
				return false;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			error = IBlockElementRandomComponentError.None;
			var iblockList = new List<int>();
			iblockList.AddRange(IBlockIds);
			var random = new Random();
			if (iblockList.Count > 0)
			{
				int randomId = 0;
				if (iblockList.Count == 1)
					randomId = iblockList[0];
				else 
				{
					
					randomId = iblockList[random.Next(iblockList.Count - 1)];
				}

				while (!TryFillElements(randomId))
				{
					iblockList.Remove(randomId);
					if (iblockList.Count == 0)
						break;
					randomId = iblockList[random.Next(iblockList.Count - 1)];
				}
			}
			
			IncludeComponentTemplate();
		}

		public string IBlockElementUrlTemplate
		{
			get { return Parameters.Get("IBlockElementUrlTemplate", "/#IBlockCode#/?section_id=#SectionId#&element_id=#ElementId#"); }
			set { Parameters["IBlockElementUrlTemplate"] = value; }
		}

		public List<int> IBlockIds
		{
			get { return Parameters.GetListInt("IBlockIds"); }
		}

		public string FilterCode
		{
			get { return Parameters.GetString("FilterCode", "SPECIALOFFER"); }
			set { Parameters["FilterCode"] = value; }
		}

		public string PhotoCode
		{
			get { return Parameters.GetString("PhotoCode", "MORE_PHOTO"); }
			set { Parameters["PhotoCode"] = value; }
		}

		public int ElementsCount
		{
			get { return Parameters.GetInt("ElementsCount", 1); }
			set { Parameters["ElementsCount"] = value.ToString(); }
		}

		List<IBlockElementWrapper> elements;

		public List<IBlockElementWrapper> Elements
		{
			get { return elements ?? (elements = new List<IBlockElementWrapper>()); }
		}


		BXFile GetElementImage( BXIBlockElement element )
		{
			int fileId = 0;
			try
			{
				fileId = element.DetailImageId;
				if(fileId <= 0)
				{
					var str = element.CustomPublicValues.GetString(PhotoCode);
					if (!String.IsNullOrEmpty(str))
					{
						var index = str.IndexOf(",");
						if (index > 0)
							str = str.Substring(0, index);

						int.TryParse(str, out fileId);
					}
				}
			}
			catch { }

			if (fileId != 0)
			{
				return BXFile.GetById(fileId);
			}

			return null;
			
		}

		public int PriceType
		{
			get { return Parameters.GetInt("PriceType", 0); }
			set { Parameters["PriceType"] = value.ToString(); }
		}

		protected override void PreLoadComponentDefinition()
		{
			base.Title = GetMessage("Component.Title");
			base.Description = GetMessage("Component.Description");
			base.Icon = "images/iblock_element_random.gif";

			Group = new BXComponentGroup("iblocks", GetMessage("Group"), 100, BXComponentGroup.Store);

			BXCategory urlCategory = BXCategory.UrlSettings,
				mainCategory = BXCategory.Main;

			ParamsDefinition.Add(
				"IBlockIds",
				new BXParamMultiSelection(
					GetMessageRaw("Param.IBlockIds"),
					"",
					mainCategory
					)
				);

			if (BXModuleManager.IsModuleInstalled("catalog"))
			{
			

			ParamsDefinition.Add(
				"PriceType",
				new BXParamSingleSelection(
					GetMessageRaw("Param.PriceType"),
					"",
					mainCategory
					)
				);
			}

			ParamsDefinition.Add(
				"ElementsCount",
				new BXParamText(
					GetMessageRaw("Param.ElementsCount"),
					"1",
					mainCategory
					)
				);

			ParamsDefinition.Add(
				"FilterCode",
				new BXParamText(
					GetMessageRaw("Param.FilterCode"),
					"SPECIALOFFER",
					mainCategory
					)
				);

			ParamsDefinition.Add(
				"PhotoCode",
				new BXParamText(
					GetMessageRaw("Param.PhotoCode"),
					"MORE_PHOTO",
					mainCategory
					)
				);

			ParamsDefinition.Add(
				"IBlockElementUrlTemplate",
				new BXParamText(
					GetMessageRaw("Param.IBlockElementUrlTemplate"),
					"list.aspx?iblock_id=#IBlockId#&id=#ElementId#",
					urlCategory
					)
				);

		}

		protected override void LoadComponentDefinition()
		{
			var iblocks = BXIBlock.GetList(null, null);
			var values = new List<BXParamValue>();
			foreach (var iblock in iblocks)
				values.Add(new BXParamValue(iblock.Name, iblock.Id.ToString()));

			ParamsDefinition["IBlockIds"].Values = values;

			if (BXModuleManager.IsModuleInstalled("catalog"))
			{
				if (Catalog != null)
				{
					List<BXParamValue> priceTypeValues = new List<BXParamValue>();
					foreach (CatalogPriceTypeInfo priceType in Catalog.GetPriceTypes())
						priceTypeValues.Add(new BXParamValue(priceType.Name, priceType.Id.ToString()));
					ParamsDefinition["PriceType"].Values = priceTypeValues;
				}
			}
		}

		private int UserId
		{
			get
			{
				BXIdentity identity = (BXIdentity)BXPrincipal.Current.Identity;
				return identity != null ? identity.Id : 0;
			}
		}


		#region catalog
		

		public CatalogClientPriceInfoSet GetClientPriceSet(BXIBlockElement element)
		{
			if (Catalog == null || element == null)
				return null;

			return  Catalog.GetClientPriceSet(element.Id, 1, false, UserId, 0, true, new int[]{ PriceType});
		
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


		#endregion
	}

	public class IBlockElementWrapper
	{
		public string DetailUrl { get; set; }
		public string PriceHtml { get; set; }
		public BXFile Image { get; set; }

		BXIBlockElement element;

		public BXIBlockElement Element 
		{
			get { return element; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("element");
				element = value;
			}
		}

		public IBlockElementWrapper(BXIBlockElement element, string url, BXFile image, string priceHtml)
		{
			PriceHtml = priceHtml;
			this.Element = element;
			DetailUrl = url;
			this.Image = image;
		}


	}

	public class IBlockElementRandomTemplate : BXComponentTemplate<IBlockElementRandomComponent>
	{
	}
}
