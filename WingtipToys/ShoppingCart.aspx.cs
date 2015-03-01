using Navigation;
using System.Collections.Generic;
using System.Web.UI;
using WingtipToys.Logic;
using WingtipToys.Models;

namespace WingtipToys
{
	public partial class ShoppingCart : Page
	{
		public List<CartItem> GetShoppingCartItems()
		{
			ShoppingCartActions actions = new ShoppingCartActions();
			StateContext.Bag.total = actions.GetTotal();
			return actions.GetCartItems();
		}

	}
}