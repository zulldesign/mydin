﻿using Navigation;
using System;
using System.Linq;
using System.Web.UI;
using mydin.Models;

namespace mydin
{
	public partial class ProductDetails : Page
	{
		public IQueryable<Product> GetProduct([NavigationData] string productName)
		{
			var _db = new mydin.Models.ProductContext();
			IQueryable<Product> query = _db.Products;
			if (!String.IsNullOrEmpty(productName))
			{
				query = query.Where(p =>
						  String.Compare(p.ProductName, productName) == 0);
			}
			else
			{
				query = null;
			}
			return query;
		}
	}
}