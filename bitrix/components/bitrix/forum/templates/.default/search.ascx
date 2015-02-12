<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Forum.Components.ForumTemplate" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.search/component.ascx" %>
<%@ Import Namespace="Bitrix.Components" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>


<script runat="server">
    
    public static String ConstructQueryString(NameValueCollection parameters,bool HtmlEncode)
	{
	    System.Collections.Generic.List<String> items = new System.Collections.Generic.List<String>();
	 
	    foreach (String name in parameters)
            items.Add(String.Concat(
                                HtmlEncode ? HttpUtility.HtmlEncode(name):name, 
                                "=",HtmlEncode ? HttpUtility.HtmlEncode(parameters[name]) : parameters[name] 
                                )
                     );
	 
	    return String.Join("&", items.ToArray());
	}
    
    //разберем пришедший QueryString и установим параметры компонента по фильтру
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        btnFind.Text = GetMessage("SearchFilter.ButtonText.Find");

        string sortVar = Parameters.GetString("SortingVariable");
        string forumsVar = Parameters.GetString("ForumsVariable");
        string dateVar = Parameters.GetString("DateIntervalVariable");
        string queryVar = Parameters.GetString("SearchStringVariable");
 
        string strForums = String.Empty;
        string strForumsQueryStringValue = String.Empty;

        NameValueCollection queryParams = new NameValueCollection();

        if (Request.QueryString[forumsVar] != null)
        {
            string[] queryForums = Request.QueryString[forumsVar].Split(',');
            System.Collections.Generic.List<string> newForums = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<int> cForums = ForumSearch.Component.Parameters.GetListInt("AvailableForums");

            int i;
           
            if (queryForums.Length > 0 && ((IList)queryForums).Contains("0"))
            {
                strForums = String.Join(";", ForumSearch.Component.Parameters.GetListString("AvailableForums").ToArray());
                strForumsQueryStringValue = "0";
                forums.Component.Parameters["CurrentForumId"] = "0";
            }
            else
            {
                for (int j = 0; j < queryForums.Length; j++)
                {
                    if (Int32.TryParse(queryForums[j], out i))
                    {
                        if (cForums.Contains(i) || cForums.Count == 0)
                        {
                            newForums.Add(queryForums[j]);
                        }
                    }
                }
                if (newForums.Count == 0)
                {
                    newForums.Add("-1");
                }
                strForums = String.Join(";", newForums.ToArray());
                strForumsQueryStringValue = String.Join(",", newForums.ToArray());
                forums.Component.Parameters["CurrentForumId"] = strForums;
            }
        }
        else
        {
            strForums = String.Join(",", ForumSearch.Component.Parameters.GetListString("AvailableForums").ToArray());
            strForumsQueryStringValue = "0";
            forums.Component.Parameters["CurrentForumId"] = "0";
        }

        ForumSearch.Component.Parameters["Forums"] = strForums;
        queryParams.Add(forumsVar, strForumsQueryStringValue);

        ForumSearchComponentDateInterval interval = ForumSearchComponentDateInterval.Any;
        
        if (Request.QueryString[dateVar] != null)
        {
            try
            {
                interval = (ForumSearchComponentDateInterval)
                                Enum.Parse(typeof(ForumSearchComponentDateInterval), Request.QueryString[dateVar]);
                queryParams.Add(dateVar, Request.QueryString[dateVar]);
            }
            catch { }
        }
        
        ForumSearch.Component.Parameters["DateInterval"] = interval.ToString();

        ForumSearchComponentSorting sort = ForumSearchComponentSorting.Relevance;

        if (Request.QueryString[sortVar] != null)
        {
            try
            {
                sort = (ForumSearchComponentSorting)
                            Enum.Parse(typeof(ForumSearchComponentSorting), Request.QueryString[sortVar]);
                queryParams.Add(sortVar, Request.QueryString[sortVar]);
            }
            catch { }

        }

        ForumSearch.Component.Parameters["SortBy"] = sort.ToString();  

        if (!String.IsNullOrEmpty(Request.QueryString[queryVar]) && Request.Form[btnFind.UniqueID] == null)
        {
            ForumSearch.Component.Parameters["SearchString"] = Request.QueryString[queryVar];
            queryParams.Add(queryVar, HttpUtility.UrlEncode(Request.QueryString[queryVar]));
        }
        else 
			ForumSearch.Component.Parameters["SearchString"] = String.Empty;
        
        
        String key = String.Empty;
        
        NameValueCollection tmp = new NameValueCollection();
        NameValueCollection oldParams = new NameValueCollection();
        
        string strParam = ForumSearch.Component.Parameters["PagingPageTemplate"];
        
        int index = strParam.IndexOf('?');
        if (index != -1)
        {
            oldParams = HttpUtility.ParseQueryString(strParam.Substring(index+1, strParam.Length - index - 1));
            if (oldParams.Count > 0)
            {
                foreach (String name in oldParams.AllKeys)
                    if (oldParams[name] != "#PageId#")
                        tmp.Set(name, oldParams[name]);
                    else key = name;
            }
        }
        tmp.Add(queryParams);
        
        if (key!=String.Empty) 
			tmp.Set(key, oldParams[key]);

        ForumSearch.Component.Parameters["PagingPageTemplate"] = 
            String.Concat(index == -1 ? strParam : strParam.Substring(0, index), "?", 
                ConstructQueryString(tmp,false));

        strParam = ForumSearch.Component.Parameters["PagingIndexTemplate"];
        
        index = strParam.IndexOf('?');
        tmp = new NameValueCollection();

        if (index != -1)
        {
            oldParams = HttpUtility.ParseQueryString(strParam.Substring(index + 1, strParam.Length - index-1));
            if (oldParams.Count > 0) tmp.Add(oldParams);
        }
        tmp.Add(queryParams);
        ForumSearch.Component.Parameters["PagingIndexTemplate"] = 
            String.Concat(index == -1 ? strParam : strParam.Substring(0, index), 
                tmp.Count == 0 ? String.Empty : "?" +  ConstructQueryString(tmp,false));
       
    }

   // делаем редирект на себя же, но Get'ом
    protected void SearchFilterClick(object sender, EventArgs e)
    { 
        string componentName = forums.ID + "$forumId";
        bool resetPaging = false;
        Uri url = Bitrix.Services.BXSefUrlManager.CurrentUrl;
        NameValueCollection queryParams = new NameValueCollection(HttpUtility.ParseQueryString(url.Query));
        string sortVar = Parameters.GetString("SortingVariable");
        string forumsVar = Parameters.GetString("ForumsVariable");
        string dateVar = Parameters.GetString("DateIntervalVariable");
        string queryVar = Parameters.GetString("SearchStringVariable");
        queryParams.Remove(sortVar);
        queryParams.Remove(dateVar);
        queryParams.Remove(forumsVar);
        queryParams.Remove(queryVar);

        if (!String.IsNullOrEmpty(Request.Form[componentName]))
        {
            if (Request.Form[componentName] != "-1")
            {
                string strParamForums;
                string[] strForums = Request.Form[componentName].Split(',');
                if (((IList)strForums).Contains("0")) strParamForums = "0";
                else strParamForums = Request.Form[componentName];
                queryParams.Set(forumsVar, strParamForums);
            }
            if (!resetPaging) 
				resetPaging = (Request.Form[componentName] != ForumSearch.Component.Parameters["Forums"]);
        }



        if (!String.IsNullOrEmpty(Request.Form["forum-filter-search-period"]))
        {
            if (Request.Form["forum-filter-search-period"] != "-1")
                queryParams.Set(dateVar, HttpUtility.UrlEncode(Request.Form["forum-filter-search-period"]));
            resetPaging = (Request.Form["forum-filter-search-period"] != ForumSearch.Component.Parameters["DateInterval"]);
        }

        if (!String.IsNullOrEmpty(Request.Form["forum-filter-search-sortby"]))
        {
            if (Request.Form["forum-filter-search-sortby"] != "0")
                queryParams.Set(sortVar, HttpUtility.UrlEncode(Request.Form["forum-filter-search-sortby"]));
            if (!resetPaging) 
				resetPaging = (Request.Form["forum-filter-search-sortby"] != ForumSearch.Component.Parameters["DateInterval"]);
        }

        if (Request.Form["forum-filter-search-query"] != null)
        {
            queryParams.Set(queryVar, HttpUtility.UrlEncode(Request.Form["forum-filter-search-query"]));
            if (!resetPaging) 
				resetPaging = (Request.Form["forum-filter-search-query"] != ForumSearch.Component.Parameters["SearchString"]);
        }

        if (resetPaging) 
			queryParams.Remove("page");
        
        
        Response.Redirect(String.Concat(url.AbsolutePath,"?",ConstructQueryString(queryParams,false)));
    }
</script>

<script type="text/javascript">
<%=btnFind.ClientID %>Component_FireDefaultButton = function(event, target) 
{
    if (event.keyCode == 13) 
    {
        var src = event.srcElement || event.target;
        if (!src || (src.tagName.toLowerCase() != "textarea")) 
        {
            var defaultButton = document.getElementById(target);
            if (defaultButton && typeof(defaultButton.click) != "undefined") 
            {
                defaultButton.click();
                event.cancelBubble = true;
                
                if (event.stopPropagation) 
                    event.stopPropagation();
                    
                return false;
            }
        }
    }
    
    return true;
}

</script>

<bx:IncludeComponent 
	ID="menu" 
	runat="server" 
	Visible="<%$ Results:ShowMenu %>"
	
	ComponentName="bitrix:forum.menu" 
	Template=".default"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	
	SearchUrl = "<%$ Results:SearchUrlTemplate %>"
	ForumIndexUrl="<%$ Results:IndexUrlTemplate %>"
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
	ForumRulesUrl="<%$ Results:ForumRulesUrl %>" 
	ForumHelpUrl="<%$ Results:ForumHelpUrl %>"
	ActiveTopicsUrl="<%$ Results:ActiveTopicsUrlTemplate %>"  
	UnAnsweredTopicsUrl="<%$ Results:UnAnsweredTopicsUrlTemplate %>"
	UserSubscriptionsUrlTemplate="<%$ Results:UserSubscriptionsUrlTemplate %>" 
/>

<%@ Register TagName="ForumBreadcrumb" TagPrefix="bx" Src="breadcrumb.ascx" %>
<%
	if (Component.ShowNavigation)
	{
		topbreadcrumb.Component = Component;
		topbreadcrumb.CustomCrumbTitle = GetMessage("Breadcrumb.Search.Name");
	}
%>
<bx:ForumBreadcrumb 
	runat="server" 
	ID="topbreadcrumb" 
	Visible="<%$ Results:ShowNavigation %>"
	
	CssPostfix="top" 
	MaxWordLength="<%$ Results:MaxWordLength %>"
	RootTitle="<%$ Parameters:ForumListTitle %>" 
	RootUrlTemplate="<%$ Results:IndexUrlTemplate %>" 
	ForumUrlTemplate="<%$ Results:TopicListUrlTemplate %>" 
	TopicUrlTemplate="<%$ Results:TopicUrlTemplate %>" 
/>

<div class="forum-content">
	<div class="forum-header-box">
		<div class="forum-header-title"><span><%= GetMessage("Breadcrumb.Search.Name")%></span></div>
	</div>
	<div class="forum-info-box">
		<div class="forum-filter forum-filter-search" onkeypress="return <%= btnFind.ClientID %>Component_FireDefaultButton(event,'<%=btnFind.ClientID %>');">
			<div class="forum-filter-fields">
				<div class="forum-filter-field forum-filter-field-query">
					<label for="forum-filter-search-query"><%=GetMessage("SearchFilter.Text.SearchString")%>:</label>
					<span>
						<input type="text" id="forum-filter-search-query" name="forum-filter-search-query" 
							value="<%= ForumSearch.Component.Parameters["SearchString"]!=null ? HttpUtility.HtmlEncode(ForumSearch.Component.Parameters["SearchString"]): ""%>"
							class="forum-search-searchstring" />
					</span>
					<div class="forum-clear-float"></div>
				</div>
				
				<div class="forum-filter-field forum-filter-field-forums">
					<label for="forums$forumId"><%= GetMessage("SearchFilter.Text.Forums") %>:</label>
					<span>
						<bx:IncludeComponent 
							ID="forums" 
							runat="server" 
							ComponentName="bitrix:forum.list" 
							Template="dropdown_postlist" 
							CurrentForumId="<%$ Results:ForumId %>"
							ThemeCssFilePath="" 
							ColorCssFilePath="" 
						/>
					</span>
					<div class="forum-clear-float"></div>
				</div>

				<div class="forum-filter-field forum-filter-field-period">
					<label for="forum-filter-search-period"><%=GetMessage("SearchFilter.Text.Period")%>:</label>
					<span>
						<select id="forum-filter-search-period" name="forum-filter-search-period">
						<% foreach (string name in Enum.GetNames(typeof(ForumSearchComponentDateInterval)))
						   { %>
						   <option 
								value="<%=(int) Enum.Parse( typeof(ForumSearchComponentDateInterval),name,false) %>"
								<% if (ForumSearch.Component.Parameters["DateInterval"] == name){ %> selected="selected"<%} %>
								>
							<%=GetMessage("Search.DateInterval.SelectText."+name)%></option> 
						<%} %>
						</select>
					</span>
					<div class="forum-clear-float"></div>
				</div>

				<div class="forum-filter-field forum-filter-field-sorting">
					<label for="forum-filter-search-sortby"><%=GetMessage("SearchFilter.Text.Sorting")%>:</label>
					<span>
						<select id="forum-filter-search-sortby" name="forum-filter-search-sortby">
						<% foreach (string name in Enum.GetNames(typeof(ForumSearchComponentSorting)))
						   { %>
						   <option value="<%=name%>"
						   <% if (ForumSearch.Component.Parameters["SortBy"] == name){ %> selected="selected"<%} %>>
							<%=GetMessage("Search.Sorting.SelectText."+name)%></option> 
						<%} %>
						</select>
					</span>
					<div class="forum-clear-float"></div>
				</div>
				<div class="forum-filter-buttons">
					<asp:Button ID="btnFind" OnClick="SearchFilterClick" runat="server" class="" />
				</div>		
			</div>
		</div>
	</div>
</div>

<% if (String.IsNullOrEmpty(Parameters["SearchStringVariable"]) || Request.QueryString[Parameters.GetString("SearchStringVariable")] != null) { %>
<bx:IncludeComponent
 id="ForumSearch"
 runat="server"
 componentname="bitrix:forum.search"
 template=".default"
 Forums=""
 ThemeCssFilePath=""
 ColorCssFilePath=""
 SortBy="Relevance"
 SearchString=""
 TopicReadUrlTemplate="<%$Results:TopicUrlTemplate%>"
 PostReadUrlTemplate="<%$Results:PostReadUrlTemplate%>"
 AuthorProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate%>"
 
 PagingIndexTemplate="<%$ Results:SearchUrlTemplate %>" 
 PagingPageTemplate="<%$ Results:SearchPageUrlTemplate %>" 
 MaxWordLength="<%$Parameters:MaxWordLength %>"
 PagingAllow="<%$Parameters:PagingAllow %>"
 PagingMode="<%$Parameters:PagingMode %>"
 PagingTemplate="<%$Parameters:PagingTemplate %>"
 PagingShowOne="<%$Parameters:PagingShowOne %>"
 PagingRecordsPerPage="<%$Parameters:PagingRecordsPerPage %>"
 PagingTitle="<%$Parameters:PagingTitle %>"
 PagingPosition="<%$Parameters:PagingPosition %>"
 PagingMaxPages="<%$Parameters:PagingMaxPages %>"
 PagingMinRecordsInverse="<%$Parameters:PagingMinRecordsInverse %>"
 PagingPageID="<%$ Request:page%>"
 /> 
<%} %>
