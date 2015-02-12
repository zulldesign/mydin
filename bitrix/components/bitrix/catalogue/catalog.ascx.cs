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
    public partial class CatalogueStockCatalog : UserControl, CatalogueComponent.ICatalog
    {
        #region ICatalogAdapter Members
        private CatalogueComponent.CatalogPriceTypeInfo[] priceTypes = null;
        public CatalogueComponent.CatalogPriceTypeInfo[] GetPriceTypes()
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

            this.priceTypes = new CatalogueComponent.CatalogPriceTypeInfo[col.Count];
            for (int i = 0; i < col.Count; i++)
                this.priceTypes[i] = new CatalogueComponent.CatalogPriceTypeInfo(col[i].Id, col[i].GetLocalizedName());

            return this.priceTypes;
        }

		public int GetSkuIBlockId(int catalogId)
		{
			if(catalogId <= 0)
				return 0;

			BXCatalogCollection col = BXCatalog.GetList(
				new BXFilter(
					new BXFilterItem(BXCatalog.Fields.ParentId, BXSqlFilterOperators.Equal, catalogId),
					new BXFilterItem(BXCatalog.Fields.Enabled, BXSqlFilterOperators.Equal, true)),
				null,
				new BXSelect(BXSelectFieldPreparationMode.Normal, BXCatalog.Fields.Id),
				new BXQueryParams(BXPagingOptions.Top(1)));

			return col.Count > 0 ? col[0].Id : 0;
		}
        #endregion
    }
}
