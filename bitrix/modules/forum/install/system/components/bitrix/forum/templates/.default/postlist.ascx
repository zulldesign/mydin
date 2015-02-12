<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Forum.Components.ForumTemplate" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.post.list/component.ascx" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>

<script runat="server">
    protected string DateCreateFromCurrentCulture
    {
        get
        {
            try
            {
                return DateTime.Parse(UserPosts.Component.Parameters["DateCreateFrom"],
                        System.Globalization.CultureInfo.InvariantCulture).ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            }
            catch
            {
                return String.Empty;
            }
        }
    }

    protected string DateCreateToCurrentCulture
    {
        get
        {
            try
            {
                return DateTime.Parse(UserPosts.Component.Parameters["DateCreateTo"],
                        System.Globalization.CultureInfo.InvariantCulture).ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            }
            catch
            {
                return String.Empty;
            }
        }
    }
    
    public static String ConstructQueryString(NameValueCollection parameters)
	{
	    System.Collections.Generic.List<String> items = new System.Collections.Generic.List<String>();
	 
	    foreach (String name in parameters)
	        items.Add(String.Concat(name, "=", parameters[name]));
	 
	    return String.Join("&", items.ToArray());
	}
    
    //разберем пришедший QueryString и установим параметры компонента по фильтру
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        BXCalendarHelper.RegisterScriptFiles();
        string strForums = String.Empty;
        string strForumsQueryStringValue = String.Empty;

        NameValueCollection queryParams = new NameValueCollection();

        if (Request.QueryString["forums"] != null)
        {
            string[] queryForums = Request.QueryString["forums"].Split(';');
            System.Collections.Generic.List<string> newForums = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<int> cForums = UserPosts.Component.Parameters.GetListInt("AvailableForums");

            int i;

            if (queryForums.Length > 0 && queryForums[0] == "0")
            {
                strForums = String.Join(";", UserPosts.Component.Parameters.GetListString("AvailableForums").ToArray());
                strForumsQueryStringValue = queryForums[0];
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
                            if (queryForums.Length == 1)
                            {
                                strForums = queryForums[j];
                                forums.Component.Parameters["CurrentForumId"] = strForums;
                                strForumsQueryStringValue = strForums;
                            }
                        }
                    }
                }
                if (newForums.Count == 0)
                {
                    newForums.Add("-1");
                }

                strForums = String.Join(";", newForums.ToArray());

            }

        }
        else
        {
            strForums = String.Join(";", UserPosts.Component.Parameters.GetListString("AvailableForums").ToArray());
            strForumsQueryStringValue = "0";
        }
        UserPosts.Component.Parameters["Forums"] = strForums;
        queryParams.Add("forums", strForumsQueryStringValue);


        ForumPostListDisplayMode mode = ForumPostListDisplayMode.AllPosts;

        if (Request.QueryString["mode"] != null)
        {
            
            try
            {
                    mode =  (ForumPostListDisplayMode)Enum.Parse(typeof(ForumPostListDisplayMode), Request.QueryString["mode"]);
                    queryParams.Add("mode", mode.ToString());
            }
            catch
            {}     
        }

        UserPosts.Component.Parameters["DisplayMode"] = mode.ToString();

        if (Request.QueryString["grouping"] != null && mode == ForumPostListDisplayMode.AllPosts)
        {
            ForumPostListGroupingOption grouping = ForumPostListGroupingOption.None;
            try
            {
                grouping = (ForumPostListGroupingOption)Enum.Parse(typeof(ForumPostListGroupingOption), Request.QueryString["grouping"]);
                queryParams.Add("grouping", grouping.ToString());
            }
            catch { }
            
            UserPosts.Component.Parameters["GroupingOption"] = grouping.ToString();
        }
        else UserPosts.Component.Parameters["GroupingOption"] = ForumPostListGroupingOption.None.ToString();

        DateTime dt;
        if (Request.QueryString["from"] != null)
        {
            if (DateTime.TryParse(Request.QueryString["from"], out dt))
			{
                UserPosts.Component.Parameters["DateCreateFrom"] = dt.ToString(System.Globalization.CultureInfo.InvariantCulture);
				queryParams.Add("from", dt.ToShortDateString());
			}
            else 
				UserPosts.Component.Parameters["DateCreateFrom"] = String.Empty;
        }
		
        if (Request.QueryString["to"] != null)
        {
            if (DateTime.TryParse(Request.QueryString["to"], out dt))
			{
                UserPosts.Component.Parameters["DateCreateTo"] = dt.ToString(System.Globalization.CultureInfo.InvariantCulture);
				queryParams.Add("to", dt.ToShortDateString());
			}
            else 
				UserPosts.Component.Parameters["DateCreateTo"] = String.Empty;
        }

        if (Request.Form[btnShow.UniqueID] != null) 
			UserPosts.Component.Parameters["Forums"] = "-1";
        
        String key = String.Empty;
        
        NameValueCollection tmp = new NameValueCollection();
        NameValueCollection oldParams = new NameValueCollection();
        
        string strParam = UserPosts.Component.Parameters["PagingPageTemplate"];
        
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
        
        if (key!=String.Empty) tmp.Set(key, oldParams[key]);
        
        UserPosts.Component.Parameters["PagingPageTemplate"] = String.Concat(index == -1 ? strParam : strParam.Substring(0, index), "?", ConstructQueryString(tmp));

        strParam = UserPosts.Component.Parameters["PagingIndexTemplate"];
        
        index = strParam.IndexOf('?');
        tmp = new NameValueCollection();

        if (index != -1)
        {
            oldParams = HttpUtility.ParseQueryString(strParam.Substring(index + 1, strParam.Length - index-1));
            if (oldParams.Count > 0) tmp.Add(oldParams);
        }
        tmp.Add(queryParams);
        UserPosts.Component.Parameters["PagingIndexTemplate"] = String.Concat(index == -1 ? strParam : strParam.Substring(0, index),tmp.Count==0 ? String.Empty: "?"+ ConstructQueryString(tmp));
        
        btnShow.Text = GetMessage("PostList.PostFilter.Show");
       
    }
   // делаем редирект на себя же, но Get'ом
    protected void PostFilterClick(object sender, EventArgs e)
    { 
        string componentName = forums.ID + "$forumId";
        bool resetPaging = false;
        DateTime dt;
        Uri url = Bitrix.Services.BXSefUrlManager.CurrentUrl;
        NameValueCollection queryParams = HttpUtility.ParseQueryString(url.Query);
        queryParams.Remove("from");
        queryParams.Remove("to");
        queryParams.Remove("forums");
        queryParams.Remove("grouping");
        queryParams.Remove("mode");

        if (!String.IsNullOrEmpty(Request.Form[componentName]))
        {
            if (Request.Form["forums"] != "-1")
                queryParams.Set("forums", Request.Form[componentName]);
            
            if (!resetPaging) 
				resetPaging = (Request.Form[componentName] != UserPosts.Component.Parameters["Forums"]);
        }

        if (!String.IsNullOrEmpty(Request.Form["forum-filter-userposts-showmode"]))
        {
            ForumPostListDisplayMode mode = ForumPostListDisplayMode.AllPosts;
            try
            {
                mode = (ForumPostListDisplayMode)Enum.Parse(typeof(ForumPostListDisplayMode), Request.Form["forum-filter-userposts-showmode"]);
                queryParams.Set("mode", mode.ToString());
            }
            catch
            {
            }

            if (mode == ForumPostListDisplayMode.AllPosts)
                if (Request.Form["forum-filter-userposts-grouping"] == "on")
                    queryParams.Set("grouping", ForumPostListGroupingOption.GroupByTopic.ToString());

            if (!resetPaging) 
				resetPaging = (Request.Form["forum-filter-userposts-showmode"] != UserPosts.Component.Parameters["DisplayMode"]);
        }
        
        if (!String.IsNullOrEmpty(Request.Form["forum-filter-userposts-datefrom"]))
        {
            if (DateTime.TryParse(Request.Form["forum-filter-userposts-datefrom"], out dt))
                queryParams.Set("from", Request.Form["forum-filter-userposts-datefrom"]);
            resetPaging = (Request.Form["forum-filter-userposts-datefrom"] != UserPosts.Component.Parameters["DateCreateFrom"]);
        }

        if (!String.IsNullOrEmpty(Request.Form["forum-filter-userposts-dateto"]))
        {
            if (DateTime.TryParse(Request.Form["forum-filter-userposts-dateto"], out dt))
                queryParams.Set("to", Request.Form["forum-filter-userposts-dateto"]);
            if ( !resetPaging) 
				resetPaging = (Request.Form["forum-filter-userposts-dateto"] != UserPosts.Component.Parameters["DateCreateTo"]);
        }

        if (resetPaging) 
			queryParams.Remove("page");
        
        Response.Redirect(String.Concat(url.AbsolutePath,queryParams.Count>0 ? "?" + queryParams.ToString():""));
    }
    
</script>

<script type="text/javascript">
<%=btnShow.ClientID %>Component_FireDefaultButton = function(event, target) 
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
<%=Component.ID %>_OnShowModeChange = function(target,elementId)
{
    var el = document.getElementById(elementId);
    if ( !el ) return;
    if ( target.selectedIndex!=0 )
        el.style.display = 'none';
    else el.style.display='block'; 
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
	SearchUrl="<%$ Results:SearchUrlTemplate %>" 	
	ForumIndexUrl="<%$ Results:IndexUrlTemplate %>"
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
	ForumRulesUrl="<%$ Results:ForumRulesUrl %>" 
	ForumHelpUrl="<%$ Results:ForumHelpUrl %>"
	ActiveTopicsUrl="<%$ Results:ActiveTopicsUrlTemplate %>"  
	UnAnsweredTopicsUrl="<%$ Results:UnAnsweredTopicsUrlTemplate %>"
	UserSubscriptionsUrlTemplate="<%$ Results:UserSubscriptionsUrlTemplate %>" 
/>

<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.post.list/component.ascx" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%
	string userName = String.Empty;
	ForumPostListComponent component = UserPosts.Component as ForumPostListComponent;
	if (component != null && component.Author != null && component.Author.User != null)
		userName = Bitrix.CommunicationUtility.BXWordBreakingProcessor.Break(component.Author.User.GetDisplayName(), Parameters.GetInt("MaxWordLength", 15), true);
%>

<div class="forum-content">
	<div class="forum-header-box">
		<div class="forum-header-title"><span><%= String.Format(GetMessage("PostList.PostFilter.Title"), userName)%></span></div>
	</div>
	<div class="forum-info-box">
		<div class="forum-filter forum-filter-userposts">
			<div class="forum-filter-fields" onkeypress="return <%= btnShow.ClientID %>Component_FireDefaultButton(event,'<%=btnShow.ClientID %>');">

				<div class="forum-filter-field forum-filter-field-forums">
					<label for="forums$forumId"><%= GetMessage("SearchFilter.Text.Forums") %>:</label>
					<span>
						<bx:IncludeComponent 
							ID="forums" 
							runat="server" 
							ComponentName="bitrix:forum.list" 
							Template="dropdown_postlist" 
							CurrentForumId=""
							ThemeCssFilePath="" 
							ColorCssFilePath=""
							Template_IsMultiple="false"
						/>
					</span>
					<div class="forum-clear-float"></div>
				</div>

				<div class="forum-filter-field forum-filter-field-date">
					<label for="forum-filter-userposts-datefrom"><%=GetMessage("PostList.PostFilter.DateFrom") %>:</label>
					<span>
						<input type="text" name="forum-filter-userposts-datefrom" 
                            id="forum-filter-userposts-datefrom" 
                            value="<%=DateCreateFromCurrentCulture %>" size="11"/>
                            <%= BXCalendarHelper.GetMarkupByInputElement("'forum-filter-userposts-datefrom'", false)%>
							<%=GetMessage("PostList.PostFilter.DateTo") %>
							<input type="text" name="forum-filter-userposts-dateto" id="forum-filter-userposts-dateto" 
                            value="<%=DateCreateToCurrentCulture %>" size="11" />
                            <%=BXCalendarHelper.GetMarkupByInputElement("'forum-filter-userposts-dateto'", false)%>
					</span>
					<div class="forum-clear-float"></div>
				</div>				

				<div class="forum-filter-field forum-filter-field-showmode">
					<label for="forum-filter-userposts-showmode"><%= GetMessage("PostList.PostFilter.ShowMode")%>:</label>
					<span>
					   <select name="forum-filter-userposts-showmode" id="forum-filter-userposts-showmode" onchange= "<%=Component.ID %>_OnShowModeChange(this,'forum-filter-userposts-groupingrow')">

							<option value="<%=ForumPostListDisplayMode.AllPosts.ToString()%>"
							<% if (((ForumPostListComponent)UserPosts.Component).ComponentDisplayMode.ToString() == "AllPosts" ) {%> selected="selected"<%}
							%>><%=GetMessage("PostList.PostFilter.Enum.ShowMode.AllPosts")%></option>
               
							<option value="<%=ForumPostListDisplayMode.AllTopicsCreated.ToString()%>"
							<% if (((ForumPostListComponent)UserPosts.Component).ComponentDisplayMode.ToString() == "AllTopicsCreated" ) {%> selected="selected"<%}
							%>><%=GetMessage("PostList.PostFilter.Enum.ShowMode.AllTopicsCreated")%></option>
               
							 <option value="<%=ForumPostListDisplayMode.AllTopicsParticipated.ToString()%>"
							<% if (((ForumPostListComponent)UserPosts.Component).ComponentDisplayMode.ToString() == "AllTopicsParticipated" ) {%> selected="selected"<%}
							%>><%=GetMessage("PostList.PostFilter.Enum.ShowMode.AllTopicsParticipated") %></option>
						</select>        
					</span>
					<div class="forum-clear-float"></div>
				</div>
			
				<div class="forum-filter-field forum-filter-field-grouping" id ="forum-filter-userposts-groupingrow" style="display:<%=((ForumPostListComponent)UserPosts.Component).ComponentDisplayMode == ForumPostListDisplayMode.AllPosts ? "":"none"%>">
					<label for="forum-filter-userposts-grouping"><%= GetMessage("PostList.PostFilter.GroupingOption") %>:</label>
					<span>
						<input  type="checkbox" 
						"<%=UserPosts.Component.Parameters["GroupingOption"]=="GroupByTopic" ? "checked=\"checked\"":String.Empty %>" 
						name="forum-filter-userposts-grouping" id="forum-filter-userposts-grouping" />
					</span>
					<div class="forum-clear-float"></div>
				</div>
				<div class="forum-filter-buttons"><asp:Button ID="btnShow" OnClick="PostFilterClick" runat="server" class="" /></div>
			</div>
		</div>
	</div>
</div>

<bx:IncludeComponent
 id="UserPosts"
 runat="server"
 componentname="bitrix:forum.post.list"
 template=".default"
 AuthorId="<%$Results:UserId %>"
 DisplayMode="AllPosts"
 GroupingOption="None"
 DateCreateFrom=""
 DateCreateTo=""
 Forums="<%$Results:ForumId%>"
 ThemeCssFilePath=""
 ColorCssFilePath=""
 SortBy="ID"
 SortDirection="Desc"
 TopicReadUrlTemplate="<%$Results:TopicUrlTemplate%>"
 PostReadUrlTemplate="<%$Results:PostReadUrlTemplate%>"
 AuthorProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate%>"
 UserPostsTemplate="<%$ Results:UserPostsTemplate%>"
 
 PagingIndexTemplate="<%$ Results:UserPostsTemplate %>" 
 PagingPageTemplate="<%$ Results:UserPostsPageTemplate %>" 
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
