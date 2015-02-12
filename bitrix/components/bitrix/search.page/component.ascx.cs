using System;
using Bitrix.Components;
using Bitrix.Security;
using Bitrix.UI;
using System.Collections.Generic;
using Bitrix.DataTypes;
using System.Web.UI.WebControls;
using System.Web;
using Bitrix.DataLayer;
using System.Text;
using Bitrix.Services;
using Bitrix.Components.Editor;
using Bitrix.Services.Text;

namespace Bitrix.Search.Components
{
	public class SearchPageComponent : BXComponent
	{
		BXSearchQuery searchQuery;
		private List<string> tags;
		static readonly char[] sep = new char[] { ',' };
		private Dictionary<string, BXSearchContentFilterInfo> searchFilterDic;
		private List<string> searchFilter;
		private List<string> showFilterItems;
		private BXSearchExpression expression;

		public BXSearchResultCollection SearchResults
		{
			get
			{
				return ComponentCache.Get<BXSearchResultCollection>("SearchResults");
			}
		}
		public int TotalSearchResultsCount
		{
			get
			{
				return ComponentCache.Get("TotalSearchResultsCount", 0);
			}
		}
		public bool ShowSearchFilter
		{
			get
			{
				return Parameters.GetBool("ShowFilter");
			}
			set
			{
				Parameters["ShowFilter"] = value.ToString();
			}
		}
		public string SearchFilter
		{
			get
			{
				return Where;
			}
		}
		public string SearchQuery
		{
			get
			{
				return Query;
			}
		}
		public IEnumerable<ListItem> SearchFilterItems
		{
			get
			{
				yield return new ListItem(GetMessageRaw("Option.Everywhere"), "");

				Dictionary<string, BXSearchContentFilterInfo> dic = SearchFilterDic;
				foreach (string s in ShowFilterItems)
				{
					BXSearchContentFilterInfo info;
					if (string.IsNullOrEmpty(s))
						continue;
					if (SearchFilters.Count != 0 && !SearchFilters.Contains(s))
						continue;
					if (!dic.TryGetValue(s, out info))
						continue;
					yield return new ListItem(info.Name, s);
				}
			}
		}
		public string PagingPosition
		{
			get
			{
				return Parameters.Get("PagingPosition", "top").ToLowerInvariant();
			}
		}
		public bool PagingShow
		{
			get
			{
				return ComponentCache.GetBool("PagingShow");
			}
		}
		public string ParamSearch
		{
			get
			{
				return Parameters.GetString("ParamSearch", "q");
			}
		}
		public string ParamWhere
		{
			get
			{
				return Parameters.GetString("ParamWhere", "where");
			}
		}
		public string ParamTags
		{
			get
			{
				return Parameters.GetString("ParamTags", "tags");
			}
		}
		public string ParamPage
		{
			get
			{
				return Parameters.GetString("ParamPage", "page");
			}
		}
		public string Query
		{
			get
			{
				string val;
				if (Parameters.TryGetValue("Query", out val))
					return val;
				string key = Parameters.GetString("ParamSearch");
				return key != null ? Request.QueryString[key] : null;
			}
			set
			{
				Parameters["Query"] = value;
			}
		}
		public string Where
		{
			get
			{
				string val;
				if (Parameters.TryGetValue("Filter", out val))
					return val;
				string key = Parameters.GetString("ParamWhere");
				return key != null ? Request.QueryString[key] : null;
			}
			set
			{
				Parameters["Filter"] = value;
			}
		}
		public List<string> Tags
		{
			get
			{
				if (tags == null)
				{
					string tagsList;
					if (!Parameters.TryGetValue("Tags", out tagsList))
					{
						string key = Parameters.GetString("ParamTags");
						if (key != null)
							tagsList = Request.QueryString[key];
					}
					tags = new List<string>();
					if (!string.IsNullOrEmpty(tagsList))
					{
						string[] tagsArray = tagsList.Split(sep, StringSplitOptions.RemoveEmptyEntries);
						foreach (string t in tagsArray)
						{
							if (BXStringUtility.IsNullOrTrimEmpty(t))
								continue;
							if (!tags.Exists(delegate(string input) { return string.Equals(input, t, StringComparison.InvariantCultureIgnoreCase); }))
								tags.Add(t);
						}
					}
				}
				return tags;
			}
		}
		public BXSearchQuerySelectTagsMode ShowTags
		{
			get
			{
				return Parameters.GetEnum("ShowTags", BXSearchQuerySelectTagsMode.None);
			}
			set
			{
				Parameters["ShowTags"] = value.ToString();
			}
		}
		public List<string> SearchFilters
		{
			get
			{
				if (searchFilter == null)
					searchFilter = Parameters.GetListString("SearchFilter");
				return searchFilter;
			}
		}
		public List<string> ShowFilterItems
		{
			get
			{
				if (!Parameters.GetBool("ShowFilter"))
					return null;
				if (showFilterItems == null)
					showFilterItems = Parameters.GetListString("ShowFilterItems");
				return showFilterItems;
			}
		}
		private Dictionary<string, BXSearchContentFilterInfo> SearchFilterDic
		{
			get
			{
				return searchFilterDic ?? (searchFilterDic = BXSearchContentManager.GetSearchFiltersInfo());
			}
		}


		public event EventHandler<SearchPageComponentBuildQueryEventArgs> BuildQuery;

		public BXSearchQuery GetSearchQuery()
		{
			return searchQuery ?? (searchQuery = BuildSearchQuery(BXSite.Current, expression));
		}


        protected override void OnInit(EventArgs e)
        {
            CacheMode = BXCacheMode.None;
            base.OnInit(e);
        }

		protected void Page_Load(object sender, EventArgs e)
		{
			BXSite site = BXSite.Current;

			if (!BXStringUtility.IsNullOrTrimEmpty(Query))
				expression = new BXSearchExpression(site.TextEncoder.Decode(site.LanguageId), Query);

			if ((expression != null && !expression.IsEmpty || Tags.Count != 0) && !IsPostBack)
			{
				ComponentCache["Query"] = Query;
				GetResults();
			}

			IncludeComponentTemplate();
		}
		private BXSearchQuery BuildSearchQuery(BXSite site, BXSearchExpression expr)
		{
			BXSearchQuery search = new BXSearchQuery();
			if (expr != null && !expr.IsEmpty)
				search.QueryExpression = expr;
			search.SiteId = site.TextEncoder.Decode(site.Id);
			search.LanguageId = site.TextEncoder.Decode(site.LanguageId);
			search.SelectTags = ShowTags;
			search.SelectNullContent = false;
			search.AddTags(Tags);

			string where = Where;
			BXSearchContentFilter custFilter = null;

			if (ShowFilterItems != null && ShowFilterItems.Contains(where) && (SearchFilters.Count == 0 || SearchFilters.Contains(where)))
				custFilter = BXSearchContentFilter.TryGetById(where);
			else if (SearchFilters.Count != 0)
			{
				BXSearchContentGroupFilter gf = new BXSearchContentGroupFilter(BXFilterExpressionCombiningLogic.Or);
				foreach (string s in SearchFilters)
				{
					BXSearchContentFilter f = BXSearchContentFilter.TryGetById(s);
					if (f != null)
						gf.Add(f);
				}
				custFilter = gf;
			}

			//discrete timestep for count caching
			DateTime now = DateTime.Now;
			DateTime nowFloor = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0);

			search.Filter = new BXSearchContentGroupFilter(
				BXFilterExpressionCombiningLogic.And,
				custFilter,
				new BXSearchContentGroupFilter(
					BXFilterExpressionCombiningLogic.Or,
					new BXSearchContentFilterItem(BXSearchField.DateFrom, BXSqlFilterOperators.Equal, null),
					new BXSearchContentFilterItem(BXSearchField.DateFrom, BXSqlFilterOperators.LessOrEqual, nowFloor)
				),
				new BXSearchContentGroupFilter(
					BXFilterExpressionCombiningLogic.Or,
					new BXSearchContentFilterItem(BXSearchField.DateTo, BXSqlFilterOperators.Equal, null),
					new BXSearchContentFilterItem(BXSearchField.DateTo, BXSqlFilterOperators.GreaterOrEqual, nowFloor)
				)
			);

			if (BuildQuery != null)
				BuildQuery(this, new SearchPageComponentBuildQueryEventArgs(search));

			return search;
		}
		private void GetResults()
		{
			int count = -1;
			ComponentCache["ShowPager"] = false;

			string pageKey = ParamPage;

			string defaultUrl = Parameters.GetString("PagingIndexTemplate");
			string pageUrl = Parameters.GetString("PagingPageTemplate"); //string.Format("?{0}=#PageId#", pageKey);
			if ((defaultUrl == null || pageUrl == null) && !IsComponentDesignMode)
			{
				string absolutePath = BXSefUrlManager.CurrentUrl.AbsolutePath;
				BXQueryString query = new BXQueryString(BXSefUrlManager.CurrentUrl.Query);
				if (query.ContainsKey(pageKey))
					query.Remove(pageKey);
				if (defaultUrl == null)
					defaultUrl = absolutePath + query;
				if (pageUrl == null)
				{
					query[pageKey] = "#PageId#";
					query.SetEncode(pageKey, false);
					pageUrl = absolutePath + query;
				}
			}

			BXSearchQuery search = GetSearchQuery();

			BXPagingParams pagingParams = PreparePagingParams();
			pagingParams.ShowAll = false;
			int p;
			if (ParamPage != null && int.TryParse(Request.QueryString[ParamPage], out p))
				pagingParams.Page = p;

			BXParamsBag<object> replace = new BXParamsBag<object>();
			replace["SearchTags"] = string.Join(",", Tags.ToArray());
			replace["SearchQuery"] = Query;
			bool isLegal;
			BXQueryParams queryParams = PreparePaging(
				pagingParams,
				delegate
				{
					return count = search.ExecuteCount(true);
				},
				replace,
				"PageId",
				defaultUrl,
				pageUrl,
				string.Empty,
				out isLegal
			);

			if (!isLegal)
				return;

			BXSearchResultCollection results;
			if (count == 0)
				results = new BXSearchResultCollection();
			else
			{
				if (queryParams.AllowPaging)
					search.PagingOptions = new BXPagingOptions(queryParams.PagingStartIndex, queryParams.PagingRecordCount);
				search.PreviewLength = Parameters.GetInt("MaxChars", 500);
				search.PreviewHighlightRadius = Parameters.GetInt("HighlightDiameter", 200) / 2;
				results = search.Execute();
			}
			
			//if not paging
			if (count == -1)
				count = results.Count;

			ComponentCache["SearchResults"] = results;
			ComponentCache["TotalSearchResultsCount"] = count;
		}

		public override bool ProcessCommand(string commandName, BXParamsBag<object> commandParameters, List<string> commandErrors)
		{
			if (commandName.Equals("search", StringComparison.OrdinalIgnoreCase))
			{
				DoSearch(commandParameters.GetString("query"), commandParameters.GetString("where"), null);
				return true;
			}
			return false;
		}

		public void DoSearch(string query, string where, string tags)
		{
			string url = MakeSearchLink(query, where, tags, null, null);

			UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
			uri.Query = url != null ? url.Substring(1) : url;
			url = uri.Uri.ToString();
			Response.Redirect(uri.Uri.ToString());
		}
		public string MakeSearchLink(string query, string where, string tags, int? page, string additionalQuery)
		{
			StringBuilder link = new StringBuilder();

			if (!string.IsNullOrEmpty(query) && !string.IsNullOrEmpty(ParamSearch))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(ParamSearch);
				link.Append("=");
				link.Append(UrlEncode(query));
			}

			if (!string.IsNullOrEmpty(where) && !string.IsNullOrEmpty(ParamWhere))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(ParamWhere);
				link.Append("=");
				link.Append(UrlEncode(where));
			}


			if (!string.IsNullOrEmpty(tags) && !string.IsNullOrEmpty(ParamTags))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(ParamTags);
				link.Append("=");
				link.Append(UrlEncode(tags));
			}

			if (page != null && page.Value >= 0 && !string.IsNullOrEmpty(ParamPage))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(ParamPage);
				link.Append("=");
				link.Append(page.Value);
			}

			if (!string.IsNullOrEmpty(additionalQuery))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(additionalQuery);
			}

			return link.Length != 0 ? link.ToString() : null;
		}
		public string MakeSearchLinkTemplate(string queryTemplate, string whereTemplate, string tagsTemplate, string pageTemplate, string additionalQuery)
		{
			StringBuilder link = new StringBuilder();

			if (!string.IsNullOrEmpty(queryTemplate) && !string.IsNullOrEmpty(ParamSearch))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(ParamSearch);
				link.Append("=");
				link.Append(queryTemplate);
			}

			if (!string.IsNullOrEmpty(whereTemplate) && !string.IsNullOrEmpty(ParamWhere))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(ParamWhere);
				link.Append("=");
				link.Append(whereTemplate);
			}


			if (!string.IsNullOrEmpty(tagsTemplate) && !string.IsNullOrEmpty(ParamTags))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(ParamTags);
				link.Append("=");
				link.Append(tagsTemplate);
			}

			if (!string.IsNullOrEmpty(pageTemplate) && !string.IsNullOrEmpty(ParamPage))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(ParamPage);
				link.Append("=");
				link.Append(pageTemplate);
			}

			if (!string.IsNullOrEmpty(additionalQuery))
			{
				link.Append(link.Length == 0 ? "?" : "&");
				link.Append(additionalQuery);
			}

			return link.Length != 0 ? link.ToString() : null;
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessage("Title");
			Description = GetMessage("Description");
			Icon = "images/search_page.gif";
			Group = BXSearchModule.GetComponentGroup();

			BXCategory searchSettingsCategory = BXCategory.SearchSettings;
			BXCategory urlSettingsCategory = BXCategory.UrlSettings;
			BXCategory dataSourceCategory = BXCategory.DataSource;

			ParamsDefinition.Add("SearchFilter", new BXParamMultiSelection(GetMessageRaw("SearchFilter"), "", dataSourceCategory));

			ParamsDefinition.Add("MaxChars", new BXParamText(GetMessageRaw("MaxChars"), "500", searchSettingsCategory));
			ParamsDefinition.Add("HighlightDiameter", new BXParamText(GetMessageRaw("HighlightDiameter"), "200", searchSettingsCategory));
			ParamsDefinition.Add("ShowFilter", new BXParamYesNo(GetMessageRaw("ShowFilter"), false, searchSettingsCategory));
			ParamsDefinition.Add("ShowFilterItems", new BXParamDoubleList(GetMessageRaw("ShowFilterItems"), "", searchSettingsCategory));
			ParamsDefinition.Add("ShowTags", new BXParamSingleSelection(GetMessageRaw("ShowTags"), "NotRejected", searchSettingsCategory));

			ParamsDefinition.Add("ParamSearch", new BXParamText(GetMessageRaw("ParamSearch"), "q", urlSettingsCategory));
			ParamsDefinition.Add("ParamPage", new BXParamText(GetMessageRaw("ParamPage"), "page", urlSettingsCategory));
			ParamsDefinition.Add("ParamWhere", new BXParamText(GetMessageRaw("ParamWhere"), "where", urlSettingsCategory));
			ParamsDefinition.Add("ParamTags", new BXParamText(GetMessageRaw("ParamTags"), "tags", urlSettingsCategory));

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
		}
		protected override void LoadComponentDefinition()
		{
			Dictionary<string, BXSearchContentFilterInfo> dic = SearchFilterDic;

			ParamsDefinition["SearchFilter"].RefreshOnDirty = true;
			List<BXParamValue> searchFilter = new List<BXParamValue>();
			foreach (KeyValuePair<string, BXSearchContentFilterInfo> kv in dic)
				searchFilter.Add(new BXParamValue(kv.Value.SystemName, kv.Key));
			ParamsDefinition["SearchFilter"].Values = searchFilter;

			ParamsDefinition["ShowFilter"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(ClientID, "ShowFilterSwitch", "ShowFilterOn", "ShowFilterOff");

			List<string> sf = Parameters.GetListString("SearchFilter");
			sf.RemoveAll(delegate(string input) { return !dic.ContainsKey(input); });
			List<BXParamValue> showFilterItems = new List<BXParamValue>();
			foreach (KeyValuePair<string, BXSearchContentFilterInfo> kv in dic)
			{
				if (sf.Count == 0 || sf.Contains(kv.Key))
					showFilterItems.Add(new BXParamValue(kv.Value.SystemName, kv.Key));
			}
			ParamsDefinition["ShowFilterItems"].Values = showFilterItems;
			ParamsDefinition["ShowFilterItems"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "ShowFilterItems", new string[] { "ShowFilterOn" });

			List<BXParamValue> showTags = new List<BXParamValue>();
			showTags.Add(new BXParamValue(GetMessageRaw("Option.DontShow"), BXSearchQuerySelectTagsMode.None.ToString()));
			showTags.Add(new BXParamValue(GetMessageRaw("Option.Approved"), BXSearchQuerySelectTagsMode.Approved.ToString()));
			showTags.Add(new BXParamValue(GetMessageRaw("Option.NotRejected"), BXSearchQuerySelectTagsMode.NotRejected.ToString()));
			showTags.Add(new BXParamValue(GetMessageRaw("Option.All"), BXSearchQuerySelectTagsMode.All.ToString()));
			ParamsDefinition["ShowTags"].Values = showTags;

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
		}
	}
	public class SearchPageComponentBuildQueryEventArgs : EventArgs
	{
		private BXSearchQuery query;

		public SearchPageComponentBuildQueryEventArgs(BXSearchQuery query)
		{
			this.query = query;
		}

		public BXSearchQuery Query
		{
			get
			{
				return query;
			}
		}
	}
	public class SearchPageTemplate : BXComponentTemplate<SearchPageComponent> { }

	#region Compatibility Issue
	public partial class SearchPage : SearchPageComponent
	{

	}
	#endregion
}
