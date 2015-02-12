using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.DataLayer;
using Bitrix.Search;

namespace Bitrix.Blog.Components
{
	public partial class BlogRssComponentTagFilter : System.Web.UI.UserControl, IBlogRssComponentTagFilter
	{
		BXSearchQuery q;
		bool empty;

		void IBlogRssComponentTagFilter.InitFilter(string siteId, string tags, List<int> ids, bool exclude, int categoryId)
		{
			if (ids.Count == 0 && !exclude)
			{
				empty = true;
				return;
			}

			q = new BXSearchQuery();
			q.SiteId = siteId;
			q.LanguageId = string.Empty;
			q.FieldsToSelect.Add(BXSearchField.ItemId);

			BXSearchContentGroupFilter f = new BXSearchContentGroupFilter(BXFilterExpressionCombiningLogic.And);
			
			//discrete timestep for count caching
			DateTime now = DateTime.Now;
			DateTime nowFloor = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0);
			f.Add(BXSearchContentFilter.IsActive(nowFloor));

			f.Add(new BXSearchContentFilterItem(BXSearchField.ModuleId, BXSqlFilterOperators.Equal, "blog"));


			List<int> ids2 = null;
			if (categoryId > 0)
			{
				ids2 = BXBlog.GetList(
					new BXFilter(new BXFilterItem(BXBlog.Fields.Categories.CategoryId, BXSqlFilterOperators.Equal, categoryId)),
					null,
					new BXSelect(BXBlog.Fields.Id),
					null
				)
				.ConvertAll(x => x.Id);
			}

			if (ids2 != null)
			{
				if (ids2.Count == 0)
					ids = ids2;									
				if (exclude)
				{
					if (ids.Count == 0)
						ids = ids2;
					else 
						ids = Bitrix.DataTypes.BXSet.Difference(ids2, ids);
				}
				else
					ids = Bitrix.DataTypes.BXSet.Intersection(ids2, ids);

				exclude = false;

				if (ids.Count == 0)
				{
					empty = true;
					return;
				}
			}


			if (!exclude || ids.Count > 0)
			{
				BXSearchContentFilter scf = new BXSearchContentFilterItem(BXSearchField.ItemGroup, Bitrix.DataLayer.BXSqlFilterOperators.In, ids.ConvertAll(x => x.ToString()).ToArray());
				if (exclude)
					scf = new BXSearchContentNotFilter(scf);
				f.Add(scf);
			}

			q.Filter = f;

			q.CalculateRelevance = false;
			q.AddTags(tags, ',');
            q.OrderBy = new BXSearchOrderBy(
                new BXSearchOrderByPair(BXSearchField.DateFrom, BXOrderByDirection.Desc),
                new BXSearchOrderByPair(BXSearchField.ContentDate, BXOrderByDirection.Desc),
                new BXSearchOrderByPair(BXSearchField.Id, BXOrderByDirection.Asc)
            ); 
		}

		int IBlogRssComponentTagFilter.Count()
		{
			return !empty ? q.ExecuteCount(true) : 0;
		}

		IList<int> IBlogRssComponentTagFilter.GetPostIdList(BXPagingOptions paging)
		{
			if (empty)
				return new List<int>();

			q.PagingOptions = paging;

			List<int> resultLst = new List<int>();
			foreach (BXSearchResult result in q.Execute())
			{
				string id = result.Content.ItemId;
				if (!id.StartsWith("p"))
					continue;

				id = id.Substring(1);
				int i;
				if (!int.TryParse(id, out i))
					continue;
                resultLst.Add(i);
			}
            return resultLst;
		}
	}
}