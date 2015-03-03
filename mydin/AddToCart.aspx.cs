using Navigation;
using System;
using System.Web.UI;
using mydin.Logic;

namespace mydin
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