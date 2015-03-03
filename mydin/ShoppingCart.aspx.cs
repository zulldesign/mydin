using Navigation;
using System.Collections.Generic;
using System.Web.UI;
using mydin.Logic;
using mydin.Models;

namespace mydin
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