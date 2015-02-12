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
	public partial class IBLockElementRandomStockCatalog : UserControl, IBlockElementRandomComponent.ICatalog
    {
        #region ICatalogAdapter Members
		private IBlockElementRandomComponent.CatalogPriceTypeInfo[] priceTypes = null;
		public IBlockElementRandomComponent.CatalogPriceTypeInfo[] GetPriceTypes()
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

			this.priceTypes = new IBlockElementRandomComponent.CatalogPriceTypeInfo[col.Count];
            for (int i = 0; i < col.Count; i++)
				this.priceTypes[i] = new IBlockElementRandomComponent.CatalogPriceTypeInfo(col[i].Id, col[i].GetLocalizedName());

            return this.priceTypes;
        }

		public IBlockElementRandomComponent.CatalogClientPriceInfoSet GetClientPriceSet(int itemId, int initQuantity, bool displayAllTiers, int userId, int currencyId, bool includeVATInPrice, IEnumerable<int> priceTypes)
        {
            BXCatalogClientPriceSet set = new BXCatalogClientPriceSet(itemId, !displayAllTiers ? initQuantity : 0, userId, currencyId, includeVATInPrice, priceTypes, BXCatalogClientPriceSetMode.View);
			IBlockElementRandomComponent.CatalogClientPriceInfo[] items =
				new IBlockElementRandomComponent.CatalogClientPriceInfo[set.Count];
            for (int i = 0; i < set.Count; i++)
                items[i] = CreateInfo(set[i]);

			return new IBlockElementRandomComponent.CatalogClientPriceInfoSet(GetPriceTypes(), items, CreateInfo(set.GetSellingPrice(initQuantity > 0 ? initQuantity : 1)));
        }
        #endregion

		private IBlockElementRandomComponent.CatalogClientPriceInfo CreateInfo(BXCatalogClientPrice price)
        {
            if (price == null)
                return null;

			return new IBlockElementRandomComponent.CatalogClientPriceInfo(price.PriceTypeId,
                price.QuantityFrom,
                price.IsVATIncluded,
                price.MarkedPriceAsString,
                price.AdjustmentAsString,
                price.SellingPriceAsString,
                price.VATRateAsString,
                price.VATAsString
                );
        }
    }
}
