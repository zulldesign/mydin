using Navigation;
using System;
using System.Web.UI;
using WingtipToys.Logic;

namespace WingtipToys
{
	public partial class AddToCart : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ShoppingCartActions usersShoppingCart = new ShoppingCartActions();
			usersShoppingCart.AddToCart(StateContext.Bag.productID);
			StateController.Navigate("Cart");
		}
	}
}