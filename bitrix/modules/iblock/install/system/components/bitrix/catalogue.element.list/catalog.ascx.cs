using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.Catalog;
using Bitrix.DataLayer;
using Bitrix.Main;
using Bitrix.Services;

namespace Bitrix.IBlock.Components
{
    public partial class CatalogueElementListStockCatalog : UserControl, CatalogueElementListComponent.ICatalog
    {
        #region ICatalog Members
        private CatalogueElementListComponent.CatalogPriceTypeInfo[] priceTypes = null;
        public CatalogueElementListComponent.CatalogPriceTypeInfo[] GetPriceTypes()
        {
            if (this.priceTypes != null)
                return this.priceTypes;

            BXCatalogPriceTypeCollection col = BXCatalogPriceType.GetList(
                null,
                new BXOrderBy(
                    new BXOrderByPair(BXCatalogPriceType.Fields.Sort, BXOrderByDirection.Asc),
                    new BXOrderByPair(BXCatalogPriceType.Fields.Code, BXOrderByDirection.Asc)
                    ),
                new BXSelect(
                    BXSelectFieldPreparationMode.Normal, 
                    BXCatalogPriceType.Fields.Id, 
                    BXCatalogPriceType.Fields.Code,
                    BXCatalogPriceType.Fields.LocalizationInfos
                    ),
                null
                );

            this.priceTypes = new CatalogueElementListComponent.CatalogPriceTypeInfo[col.Count];
            for (int i = 0; i < col.Count; i++)
                this.priceTypes[i] = new CatalogueElementListComponent.CatalogPriceTypeInfo(col[i].Id, col[i].GetLocalizedName());

            return this.priceTypes;
        }

        public CatalogueElementListComponent.CatalogClientPriceInfoSet GetClientPriceSet(int itemId, int initQuantity, bool displayAllTiers, string[] userRoleNames, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes)
        {
            BXCatalogClientPriceSet set = new BXCatalogClientPriceSet(itemId, !displayAllTiers ? initQuantity : 0, userRoleNames, currencyId, includeVATInPrice, priceTypes, BXCatalogClientPriceSetMode.View);
            CatalogueElementListComponent.CatalogClientPriceInfo[] items =
                new CatalogueElementListComponent.CatalogClientPriceInfo[set.Count];
            for (int i = 0; i < set.Count; i++)
                items[i] = CreateInfo(set[i]);

            return new CatalogueElementListComponent.CatalogClientPriceInfoSet(GetPriceTypes(), items, CreateInfo(set.GetSellingPrice(initQuantity > 0 ? initQuantity : 1)));
        }

        public CatalogueElementListComponent.CatalogClientPriceInfoSet GetClientPriceSet(BXIBlockElement item, int initQuantity, bool displayAllTiers, string[] userRoleNames, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes)
        {
			if(item == null)
				throw new ArgumentNullException("item");

			BXCatalogItem catalogItem = GetCatalogItem(item);
			BXCatalogClientPriceSet set = catalogItem != null 
				? new BXCatalogClientPriceSet(catalogItem, !displayAllTiers ? initQuantity : 0, userRoleNames, currencyId, includeVATInPrice, priceTypes, BXCatalogClientPriceSetMode.View)
				: new BXCatalogClientPriceSet(item.Id, !displayAllTiers ? initQuantity : 0, userRoleNames, currencyId, includeVATInPrice, priceTypes, BXCatalogClientPriceSetMode.View);

            CatalogueElementListComponent.CatalogClientPriceInfo[] items = 
				new CatalogueElementListComponent.CatalogClientPriceInfo[set.Count];

            for (int i = 0; i < set.Count; i++)
                items[i] = CreateInfo(set[i]);

            return new CatalogueElementListComponent.CatalogClientPriceInfoSet(GetPriceTypes(), items, CreateInfo(set.GetSellingPrice(initQuantity > 0 ? initQuantity : 1)));
        }

		public bool CanPutInCart(int itemId)
		{
			var item = BXCatalogItem.GetById(itemId);
			
			if (item != null)
			{
				var catalog = BXCatalog.GetById(item.CatalogId);
				return !catalog.CheckStockAvailability || item.Quantity > 0;
			}
			return false;
		}

		public bool CanPutInCart(BXIBlockElement item)
		{
			if(item == null)
				throw new ArgumentNullException("item");

			BXCatalogItem catalogItem = GetCatalogItem(item);
			if(catalogItem == null)
				return CanPutInCart(item.Id);

			return catalogItem.Catalog != null && (!catalogItem.Catalog.CheckStockAvailability || catalogItem.Quantity > 0);
		}

		public bool HasSkuItems(int itemId)
		{
			BXCatalogItemCollection c = BXCatalogItem.GetList(
				new BXFilter(
					new BXFilterItem(BXCatalogItem.Fields.Id, BXSqlFilterOperators.Equal, itemId)),
					null,
					new BXSelect(BXSelectFieldPreparationMode.Normal,
						BXCatalogItem.Fields.Id,
						BXCatalogItem.Fields.SKUQuantity),
					new BXQueryParams(BXPagingOptions.Top(1)));

			return c.Count > 0 ? c[0].SKUQuantity > 0 : false;
		}

		public bool HasSkuItems(BXIBlockElement item)
		{
			if(item == null)
				throw new ArgumentNullException("item");

			BXCatalogItem catalogItem = GetCatalogItem(item);
			if(catalogItem != null)
				return catalogItem.SKUQuantity > 0;

			return HasSkuItems(item.Id);
		}

		public CatalogueElementListComponent.CatalogSkuItem[] GetSkuItems(int itemId, int initQuantity, bool displayAllTiers, int userId, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes)
		{
			if(itemId <= 0)
				return new CatalogueElementListComponent.CatalogSkuItem[0];

			BXCatalogItemCollection c = BXCatalogItem.GetList(
				new BXFilter(
					new BXFilterItem(BXCatalogItem.Fields.ProductId, BXSqlFilterOperators.Equal, itemId),
					new BXFilterItem(BXCatalogItem.Fields.Enabled, BXSqlFilterOperators.Equal, true)),
					null,
					new BXSelect(BXSelectFieldPreparationMode.Normal,
						BXCatalogItem.Fields.Id,
						BXCatalogItem.Fields.Quantity,
						BXCatalogItem.Fields.IBlockElement.ID,
						BXCatalogItem.Fields.IBlockElement.Name),
					null);

			CatalogueElementListComponent.CatalogSkuItem[] ary = new CatalogueElementListComponent.CatalogSkuItem[c.Count];
			for(int i = 0; i < ary.Length; i++)
			{
				BXCatalogItem item = c[i];
				BXCatalogClientPrice price = (new BXCatalogClientPriceSet(item.Id, !displayAllTiers ? initQuantity : 0, userId, currencyId, includeVATInPrice, priceTypes, BXCatalogClientPriceSetMode.View)).GetCurrentSellingPrice();
				ary[i] = new CatalogueElementListComponent.CatalogSkuItem(item.Id, itemId, item.GetName(), item.Quantity > 0, price != null ? CreateInfo(price) : null, 0);
			}
			return ary;
		}

		private CatalogueElementListComponent.CatalogClientPriceInfo CreateInfo(BXCatalogClientPrice price)
		{
			if (price == null)
				return null;

			return new CatalogueElementListComponent.CatalogClientPriceInfo(price.PriceTypeId,
				price.QuantityFrom,
				price.IsVATIncluded,
				price.MarkedPriceAsString,
				price.AdjustmentAsString,
				price.SellingPriceAsString,
				price.VATRateAsString,
				price.VATAsString,
				price.SellingPrice,
				price.CurrencyId
				);
		}

		public Bitrix.IBlock.Components.CatalogueElementListComponent.CatalogDiscountInfo GetDiscount(
			int userId, IEnumerable<int> roleIds, int itemId, int priceTypeId, 
			decimal price, int quantity, IList<int> sectionIds, string siteId,
			int currencyId
			)
		{
			var result = new Bitrix.IBlock.Components.CatalogueElementListComponent.CatalogDiscountInfo();
			result.HasInfo = false;
			var catalogItem = BXCatalogItem.GetById(itemId);
			if(catalogItem == null )	
				return result;

			var findResult = BXCatalogDiscountManager.FindSuitableDiscount(new BXCatalogFindDiscountInfo(userId, roleIds, catalogItem, 
				priceTypeId,price, quantity, sectionIds, siteId, currencyId));

			if (findResult != null && findResult.Discount!=null)
			{
				result.HasInfo = true;
				result.Name = findResult.Discount.Name;
				result.Value = findResult.Value;
				result.DisplayHtml = findResult.DiscountedPriceDisplayHtml;
			}
			return result;
		}

		private BXEntityDynamicJoinAdapter<BXIBlockElement,BXIBlockElementCollection,BXIBlockElement.Scheme,BXCatalogItem,BXCatalogItemCollection,BXCatalogItem.Scheme> catalogItemAdapter = null;
		private BXEntityDynamicJoinAdapter<BXIBlockElement,BXIBlockElementCollection,BXIBlockElement.Scheme,BXCatalogItem,BXCatalogItemCollection,BXCatalogItem.Scheme> InnerCatalogItemAdapter
		{
			get 
			{ 
				if(this.catalogItemAdapter != null)
					return this.catalogItemAdapter;
				return (this.catalogItemAdapter = new BXEntityDynamicJoinAdapter<BXIBlockElement,BXIBlockElementCollection,BXIBlockElement.Scheme,BXCatalogItem,BXCatalogItemCollection,BXCatalogItem.Scheme>(BXIBlockElement.Fields, "ID", BXCatalogItem.Fields, "ID", "CatalogItem", "CI", BXSchemeRelationship.OneToOne)); 
			}
		}

		private BXCatalogItem GetCatalogItem(BXIBlockElement item)
		{
			if(item == null)
				return null;

			BXCatalogItem catalogItem;
			InnerCatalogItemAdapter.TryGetJoinedEntity(item, "_BX_CATALOG_ITEM_", out catalogItem);
			return catalogItem;
		}

		public void PrepareElementSelect(BXSelect select)
		{
			if(select == null)
				return;

			select.Add(InnerCatalogItemAdapter.JoinScheme);
			select.Add(InnerCatalogItemAdapter.JoinScheme.Catalog);
			select.Add(InnerCatalogItemAdapter.JoinScheme.VATRate);
		}
		#endregion
	}
}
