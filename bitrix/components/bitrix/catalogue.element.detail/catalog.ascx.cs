using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.Catalog;
using Bitrix.DataLayer;
using Bitrix.Main;
using Bitrix.Services;
using System.Web.Caching;

namespace Bitrix.IBlock.Components
{
    public partial class CatalogueElementDetailStockCatalog : UserControl, CatalogueElementDetailComponent.ICatalog
    {
        #region ICatalogAdapter Members
        private CatalogueElementDetailComponent.CatalogPriceTypeInfo[] priceTypes = null;
        public CatalogueElementDetailComponent.CatalogPriceTypeInfo[] GetPriceTypes()
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

            this.priceTypes = new CatalogueElementDetailComponent.CatalogPriceTypeInfo[col.Count];
            for (int i = 0; i < col.Count; i++)
                this.priceTypes[i] = new CatalogueElementDetailComponent.CatalogPriceTypeInfo(col[i].Id, col[i].GetLocalizedName());

            return this.priceTypes;
        }

        public CatalogueElementDetailComponent.CatalogClientPriceInfoSet GetClientPriceSet(int itemId, int initQuantity, bool displayAllTiers, int userId, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes)
        {
            BXCatalogClientPriceSet set = new BXCatalogClientPriceSet(itemId, !displayAllTiers ? initQuantity : 0, userId, currencyId, includeVATInPrice, priceTypes, BXCatalogClientPriceSetMode.View);
            CatalogueElementDetailComponent.CatalogClientPriceInfo[] items = 
                new CatalogueElementDetailComponent.CatalogClientPriceInfo[set.Count];
            for (int i = 0; i < set.Count; i++)
                items[i] = CreateInfo(set[i]);

            return new CatalogueElementDetailComponent.CatalogClientPriceInfoSet(GetPriceTypes(), items, CreateInfo(set.GetSellingPrice(initQuantity > 0 ? initQuantity : 1)));
        }

		public bool CheckStockAvailability(int catalogId)
		{
			BXCatalog catalog = BXCatalog.GetById(catalogId);
			return catalog != null ? catalog.CheckStockAvailability : false;
		}

		public int QuantityInStock(int elementId)
		{
			var item = BXCatalogItem.GetById(elementId);
			return item != null ? item.Quantity : 0;
		}

		private readonly string skuItemColKey = "_BX_CATALOG_SKU_ITEM_COL_";

		public CatalogueElementDetailComponent.CatalogSKUItem[] GetSKUItems(int elementId, int iblockId)
		{
			if(elementId <= 0 || iblockId <= 0)
				return new CatalogueElementDetailComponent.CatalogSKUItem[0];

			BXCatalog catalog = BXCatalog.GetByParentId(iblockId);
			int skuCatalogId = catalog != null && catalog.Enabled ? catalog.Id : 0;
			if (skuCatalogId <= 0)
				return new CatalogueElementDetailComponent.CatalogSKUItem[0];

			string key = string.Concat(skuItemColKey, skuCatalogId, "_", elementId, "_");
			BXCatalogItemCollection col = BXCacheManager.MemoryCache.Application.Get(key) as BXCatalogItemCollection;
			if (col == null)
				using (BXTagCachingScope scope = BXTagCachingScope.Open(key))
				{
					col = BXCatalogItem.GetList(
						new BXFilter(new BXFilterItem(BXCatalogItem.Fields.ProductId, BXSqlFilterOperators.Equal, elementId),
							new BXFilterItem(BXCatalogItem.Fields.CatalogId, BXSqlFilterOperators.Equal, skuCatalogId),
							new BXFilterItem(BXCatalogItem.Fields.Enabled, BXSqlFilterOperators.Equal, true)),
						new BXOrderBy(new BXOrderByPair(BXCatalogItem.Fields.Id, BXOrderByDirection.Asc)),
						new BXSelectAdd(BXCatalogItem.Fields.IBlockElement, BXCatalogItem.Fields.IBlockElement.CustomFields[skuCatalogId]),
						null);

					BXCacheManager.MemoryCache.Application.Insert(key, col, scope.CreateDependency(), DateTime.Now.AddMinutes(5d), Cache.NoSlidingExpiration, CacheItemPriority.Low, null);
				}

			CatalogueElementDetailComponent.CatalogSKUItem[] ary = new CatalogueElementDetailComponent.CatalogSKUItem[col.Count];
			for(int i = 0; i < ary.Length; i++)
				ary[i] = new CatalogueElementDetailComponent.CatalogSKUItem(this, col[i].IBlockElement);

			return ary;
		}

		public CatalogueElementDetailComponent.CatalogSKUItem GetItem(int elementId, int iblockId)
		{
			if(elementId <= 0 || iblockId <= 0)
				return null;

			BXCatalog catalog = BXCatalog.GetByParentId(iblockId);
			int skuCatalogId = catalog != null && catalog.Enabled ? catalog.Id : 0; 
			if(skuCatalogId <= 0)
				return null;

			BXCatalogItemCollection itemCol = BXCatalogItem.GetList(
				new BXFilter(new BXFilterItem(BXCatalogItem.Fields.Id, BXSqlFilterOperators.Equal, elementId), 
					new BXFilterItem(BXCatalogItem.Fields.CatalogId, BXSqlFilterOperators.Equal, skuCatalogId)),
				null,
				new BXSelectAdd(BXCatalogItem.Fields.IBlockElement, BXCatalogItem.Fields.IBlockElement.CustomFields[skuCatalogId]),
				null);

			return itemCol.Count > 0 ? new CatalogueElementDetailComponent.CatalogSKUItem(this, itemCol[0].IBlockElement) : null;
		}
        #endregion

		public CatalogDiscountInfo GetDiscount(
			int userId, IEnumerable<int> roleIds, int itemId, int priceTypeId,
			decimal price, int quantity, IList<int> sectionIds, string siteId,
			int currencyId
			)
		{
			var result = new Bitrix.IBlock.Components.CatalogDiscountInfo();
			result.HasInfo = false;
			var catalogItem = BXCatalogItem.GetById(itemId);
			if (catalogItem == null)
				return result;

			var findResult = BXCatalogDiscountManager.FindSuitableDiscount(new BXCatalogFindDiscountInfo(userId, roleIds, catalogItem,
				priceTypeId, price, quantity, sectionIds, siteId, currencyId));

			if (findResult != null && findResult.Discount != null)
			{
				result.HasInfo = true;
				result.Name = findResult.Discount.Name;
				result.Value = findResult.Value;
				result.DisplayHtml = findResult.DiscountedPriceDisplayHtml;
			}
			return result;
		}

        private CatalogueElementDetailComponent.CatalogClientPriceInfo CreateInfo(BXCatalogClientPrice price)
        {
            if (price == null)
                return null;

            return new CatalogueElementDetailComponent.CatalogClientPriceInfo(price.PriceTypeId,
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
    }
}
