using Navigation;
using System;
using System.Linq;
using System.Web.UI;
using WingtipToys.Logic;
using WingtipToys.Models;

namespace WingtipToys
{
	public partial class Site : MasterPage
	{
		protected void Page_PreRender(object sender, EventArgs e)
		{
			ShoppingCartActions usersShoppingCart = new ShoppingCartActions();
			StateContext.Bag.count = usersShoppingCart.GetCount();
		}

		public IQueryable<Category> GetCategories()
		{
			var db = new WingtipToys.Models.ProductContext();
			IQueryable<Category> query = db.Categories;
			return query;
		}
	}
}