using Navigation;
using System;
using System.Linq;
using System.Web.UI;
using mydin.Models;

namespace mydin
{
	public partial class ProductList : Page
	{
		public IQueryable<Product> GetProducts([NavigationData] string categoryName)
		{
			var _db = new mydin.Models.ProductContext();
			IQueryable<Product> query = _db.Products;

			if (!String.IsNullOrEmpty(categoryName))
			{
				query = query.Where(p =>
									String.Compare(p.Category.CategoryName,
									categoryName) == 0);
			}
			return query;
		}
	}
}