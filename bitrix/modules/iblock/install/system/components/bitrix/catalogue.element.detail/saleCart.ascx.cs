using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.Sale.SaleCart;
using Bitrix.Security;

namespace Bitrix.IBlock.Components
{
    public partial class CatalogueElementDetailSaleCart : UserControl, CatalogueElementDetailComponent.ISaleCart
    {

		private int UserId
		{
			get
			{
				BXIdentity identity = (BXIdentity)BXPrincipal.Current.Identity;
				return identity != null ? identity.Id : 0;
			}
		}

        public string GetCartGuid(HttpRequest request)
        {
			if (request == null)
				return string.Empty;

			HttpCookie cookie = request.Cookies.Get(BXSaleCart.ExternalStorageUIDKey);
			if (cookie != null) return cookie.Value;

			if (UserId > 0)
			{
				var cart = BXSaleCart.GetByUserId(UserId);
				if (cart != null)
					return cart.UniqueId;
			}
			return string.Empty;
        }

        private void SetCartGuid(string guid, HttpResponse response)
        {
            if (response == null)
                return;

            HttpCookie cookie = new HttpCookie(BXSaleCart.ExternalStorageUIDKey, guid);
            cookie.Expires = DateTime.Now.AddYears(1);
            response.SetCookie(cookie);
        }

        public bool Add(int itemId, int itemQty, string itemDetailUrl, HttpRequest request, HttpResponse response)
        {
            try
            {
                string guid = GetCartGuid(request);
                string newGuid = BXSaleCartItemManager.AddToCartByElementId(UserId, guid, itemId, BXCatalogSaleCartItemResolver.ResolverId, itemQty, itemDetailUrl);
                if (!string.Equals(guid, newGuid, StringComparison.Ordinal))
                    SetCartGuid(newGuid, response);
            }
            catch 
            {
                return false;
            }
            return true;
        }

		public int GetQuantity(int itemId, HttpRequest request, HttpResponse response) 
        {
			var guid = GetCartGuid(request);
			var newGuid = String.Empty;
			var quantity = BXSaleCartItemManager.GetQuantityInCartByElementId(GetCartGuid(request), itemId, UserId, BXSite.Current.Id, out newGuid);
			if (!string.Equals(guid, newGuid, StringComparison.Ordinal))
				SetCartGuid(newGuid, response);
			return quantity;
        }

		public bool IsExists(int itemId, HttpRequest request, HttpResponse response) 
        {
			return GetQuantity(itemId, request, response) > 0;
        }

		public CatalogueElementDetailComponent.SaleCartItem[] GetSaleCartItems(HttpRequest request, HttpResponse response)
		{
			string guid = GetCartGuid(request),
				newGuid;

			var srcItems = BXSaleCartItemManager.GetSaleCartItems(guid, UserId, BXSite.Current.Id, out newGuid);
			var dstItems = new CatalogueElementDetailComponent.SaleCartItem[srcItems.Count];
			for(int i = 0; i < srcItems.Count; i++)
			{
				BXSaleCartItem srcItem = srcItems[i];
				if(srcItem.IsPutAside)
					continue;

				var dstItem = new CatalogueElementDetailComponent.SaleCartItem();
				dstItem.ElementId = srcItem.ElementId;
				dstItem.Quantity = srcItem.Quantity;
				dstItems[i] = dstItem;
			}

			if (!string.Equals(guid, newGuid, StringComparison.Ordinal))
				SetCartGuid(newGuid, response);

			return dstItems;
		}

        public int Count()
        {
            return 0;
        }
    }
}
