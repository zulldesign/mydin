<%@ WebHandler Language="C#" Class="Bitrix.Search.GetTagsHandler" %>

using System;
using System.Web;
using System.Collections.Generic;

using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Services.Js;

namespace Bitrix.Search
{
    public class GetTagsHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;

            string query = context.Request.QueryString["query"];
            string site = context.Request.QueryString["site"];
            string filter = context.Request.QueryString["filter"];
            string moduleId = context.Request.QueryString["m"];
            string itemGroup = context.Request.QueryString["g"];
            string itemId = context.Request.QueryString["i"];

            if (query == null)
            {
                context.Response.Write("(false)");
                context.Response.End();
                return;
            }

            BXContentTagQuery tagQuery = new BXContentTagQuery();
            tagQuery.SearchQuery = new BXSearchQuery();
            tagQuery.SearchQuery.SiteId = site ?? "";
            tagQuery.SearchQuery.LanguageId = "";
            BXSearchContentGroupFilter searchFilter = new BXSearchContentGroupFilter(BXFilterExpressionCombiningLogic.And);
            tagQuery.SearchQuery.Filter = searchFilter;

            DateTime now = DateTime.Now;

            searchFilter.Add(
                new BXSearchContentGroupFilter(
                    BXFilterExpressionCombiningLogic.Or,
                    new BXSearchContentFilterItem(BXSearchField.DateFrom, BXSqlFilterOperators.Equal, null),
                    new BXSearchContentFilterItem(BXSearchField.DateFrom, BXSqlFilterOperators.LessOrEqual, now)
                )
            );
            searchFilter.Add(
                new BXSearchContentGroupFilter(
                   BXFilterExpressionCombiningLogic.Or,
                   new BXSearchContentFilterItem(BXSearchField.DateTo, BXSqlFilterOperators.Equal, null),
                   new BXSearchContentFilterItem(BXSearchField.DateTo, BXSqlFilterOperators.GreaterOrEqual, now)
                )
            );

            if (!string.IsNullOrEmpty(moduleId))
            {
                searchFilter.Add(new BXSearchContentFilterItem(BXSearchField.ModuleId, BXSqlFilterOperators.Equal, moduleId));
                if (itemGroup != null)
                    searchFilter.Add(new BXSearchContentFilterItem(BXSearchField.ItemGroup, BXSqlFilterOperators.Equal, itemGroup));
                if (itemId != null)
                    searchFilter.Add(new BXSearchContentFilterItem(BXSearchField.ItemId, BXSqlFilterOperators.Equal, itemId));
            }
            searchFilter.Add(BXSearchContentFilter.TryGetById(filter));

            tagQuery.SelectTagCount = true;
            tagQuery.TextEncoder = BXTextEncoder.EmptyTextEncoder;
            tagQuery.PagingOptions = new BXPagingOptions(0, 10);

            BXContentTagGroupFilter tagQueryFilter = new BXContentTagGroupFilter(BXFilterExpressionCombiningLogic.And);
            tagQuery.Filter = tagQueryFilter;
            tagQueryFilter.Add(new BXContentTagFilterItem(BXContentTagField.Status, BXSqlFilterOperators.NotEqual, BXContentTagStatus.Rejected));

            tagQuery.OrderBy = new BXContentTagOrderBy();

            if (query != "")
            {
                if (query.Length > 2)
                    query = query.Remove(2);
                query = query.ToLowerInvariant();

                tagQueryFilter.Add(new BXContentTagFilterItem(BXContentTagField.Name, BXSqlFilterOperators.StartsLike, query));

                tagQuery.OrderBy.Add(BXContentTagField.Name, BXOrderByDirection.Asc);
            }
            else
            {
                tagQuery.OrderBy.Add(BXContentTagField.TagCount, BXOrderByDirection.Desc);
                tagQuery.OrderBy.Add(BXContentTagField.Name, BXOrderByDirection.Asc);
            }
           BXContentTagCollection tags = tagQuery.Execute();
            string output = BXJSUtility.BuildJSArray(tags.ConvertAll<Dictionary<string, object>>(delegate(BXContentTag input)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("name", input.Name);
                dic.Add("count", input.TagCount);
                return dic;
            }));

            context.Response.Write("(");
            context.Response.Write(output);
            context.Response.Write(")");
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}