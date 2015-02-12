using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.DataLayer;
using Bitrix.Search;

namespace Bitrix.Blog.Components
{
	public partial class BlogPostsComponentTagFilter : System.Web.UI.UserControl, IBlogPostsComponentTagFilter
	{
		bool empty;
		BXSearchQuery q;

		void IBlogPostsComponentTagFilter.InitFilter(string siteId, string tags, string blogSlugs, List<int> ids, bool exclude, int categoryId)
		{
			if (ids != null && ids.Count == 0 && exclude)
				ids = null;

			if (ids != null && ids.Count == 0 && !exclude)
			{
				empty = true;
				return;
			}


			q = new BXSearchQuery();
			q.SiteId = siteId;
			q.LanguageId = "";
			q.FieldsToSelect.Add(BXSearchField.ItemId);

			BXSearchContentGroupFilter f = new BXSearchContentGroupFilter(BXFilterExpressionCombiningLogic.And);
			//discrete timestep for count caching
			DateTime now = DateTime.Now;
			//DateTime nowFloor = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0);
			f.Add(BXSearchContentFilter.IsActive(now.AddMinutes(5 - (now.Minute % 5)).AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond)));
			f.Add(new BXSearchContentFilterItem(BXSearchField.ModuleId, BXSqlFilterOperators.Equal, "blog"));

			BXFilter bf = null;

			if (!string.IsNullOrEmpty(blogSlugs))
			{
				string[] blogSlugArr = blogSlugs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (blogSlugArr.Length > 0)
				{
					for (int i = 0; i < blogSlugArr.Length; i++)
						blogSlugArr[i] = blogSlugArr[i].Trim();

					bf = new BXFilter(new BXFilterItem(BXBlog.Fields.Slug, BXSqlFilterOperators.In, blogSlugArr));
				}
			}

			if (categoryId > 0)
			{
				if (bf == null)
					bf = new BXFilter();
				bf.Add(new BXFilterItem(BXBlog.Fields.Categories.CategoryId, BXSqlFilterOperators.Equal, categoryId));
			}

			List<int> blogIdLst = null;
			if (bf != null)
			{
				BXBlogCollection blogs = BXBlog.GetList(
					bf,
					null,
					new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlog.Fields.Id),
					null
				);
				blogIdLst = blogs.ConvertAll(x => x.Id);
			}

			if (blogIdLst != null)
			{
				if (ids != null)
				{
					if (exclude)
					{
						ids = Bitrix.DataTypes.BXSet.Difference(blogIdLst, ids);
						exclude = false;
					}
					else
					{
						ids = Bitrix.DataTypes.BXSet.Intersection(blogIdLst, ids);
					}
				}
				else
				{
					ids = blogIdLst;
					exclude = false;
				}				
			}			
			
			if (ids != null && ids.Count == 0 && !exclude)
			{
				empty = true;
				return;
			}

			if (ids != null)
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

		int IBlogPostsComponentTagFilter.Count()
		{
			return empty ? 0 : q.ExecuteCount(true);
		}

		IList<int> IBlogPostsComponentTagFilter.GetPostIdList(BXPagingOptions paging)
		{
			if (empty)
				return new List<int>();

			q.PagingOptions = paging;

			List<int> postIds = new List<int>();
			foreach (BXSearchResult result in q.Execute())
			{
				string id = result.Content.ItemId;
				if (!id.StartsWith("p"))
					continue;

				id = id.Substring(1);
				int i;
				if (!int.TryParse(id, out i))
					continue;
				postIds.Add(i);
			}
			return postIds;
		}
	}
}