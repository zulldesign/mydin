<%@ Reference Control="~/bitrix/components/bitrix/pmessages/component.ascx" %>
<%@ Reference Control="~/bitrix/components/bitrix/pmessages.topic.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.CommunicationUtility.Components.PrivateMessagesTemplate" %>
<%@ Import Namespace="Bitrix.CommunicationUtility.Components" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Services" %>

<%
    
    PrivateMessageTopicListComponent topicsComponent = (PrivateMessageTopicListComponent)topics.Component;
    %>

   
    


<bx:IncludeComponent
 id="menu"
 runat="server"
 Visible="<%$ Results:ShowMenu %>"
 componentname="bitrix:pmessages.menu"
 template=".default"
 ThemeCssFilePath=""
 ColorCssFilePath=""
 NewTopicUrl="<%$ Results:NewTopicUrlTemplate %>"
 IndexUrl="<%$ Results:IndexUrl %>"
 FoldersUrl="<%$ Results:FoldersUrl %>" 
 FolderUrlTemplate="<%$ Results:FolderUrlTemplate %>"
 />


<bx:IncludeComponent 
	ID="topics" 
	runat="server" 
	ComponentName="bitrix:pmessages.topic.list" 
	Template=".default" 
	FolderId="<%$ Results:FolderId %>"
	ThemeCssFilePath="" 
	ColorCssFilePath=""
	PagingPageID="<%$ Request:page %>" 
	PagingIndexTemplate="<%$ Results:TopicListUrlTemplate %>" 
	PagingPageTemplate="<%$ Results:TopicListPageUrlTemplate %>" 
	TopicReadUrlTemplate="<%$ Results:TopicReadUrlTemplate %>" 
	MessageReadUrlTemplate="<%$ Results:MessageReadUrlTemplate %>" 
	NewTopicUrlTemplate="<%$ Results:NewTopicUrlTemplate %>"
	TopicEditUrlTemplate="<%$ Results:TopicReadUrlTemplate %>" 
/>

<% 
	if (
		 topicsComponent.FatalError == PrivateMessageTopicListComponent.ErrorCode.None) 
	{ 
%>

<div class="forum-info-box">
					    <label for="<%=ClientID %>_MsgFilter"><%=GetMessage("MsgFilter.LabelText") %></label>
					    <select id="<%=ClientID %>_MsgFilter" name="<%= UniqueID %>$msgFilter" onchange="selectClick();">
					        <option value="All" 
					        <%= (((Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent)topics.Component).ComponentDisplayMode == Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode.All ? "selected=\"selected\"":"") %>>
					        <%= GetMessage("MsgFilter.Option.All") %></option>
							<option value="In"
							<%= (((Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent)topics.Component).ComponentDisplayMode == Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode.In ? "selected=\"selected\"":"") %>
							><%= GetMessage("MsgFilter.Option.In")%></option>
							<option value="Out"
							<%= (((Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent)topics.Component).ComponentDisplayMode == Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode.Out ? "selected=\"selected\"":"") %>
							><%= GetMessage("MsgFilter.Option.Out")%></option>
					    </select>
					    <asp:Button ID="btnShow" OnClick="MsgFilterClick" runat="server" Text="<%$ Loc:MsgFilter.Refresh %>" class="" />
</div>

<%} %>

<script type="text/javascript">
	function selectClick(){
		document.getElementById('<%= btnShow.ClientID%>').click();
	}
</script>

 <script runat="server">
    
        public static String ConstructQueryString(NameValueCollection parameters)
        {
            System.Collections.Generic.List<String> items = new System.Collections.Generic.List<String>();

            foreach (String name in parameters)
                items.Add(String.Concat(name, "=", parameters[name]));

            return String.Join("&", items.ToArray());
        }
        
    const string dispModeVar = "showmode";
    const string folderVar = "folder";
    const string pageVar = "page";

    bool DisplayModeIsOk(string modeString, out Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode mode)
    {
        try
        {
            mode = (Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode)
                Enum.Parse(typeof(Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode),
                            modeString);
            if (!Enum.IsDefined(typeof(Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode), mode))
                return false;
        }
        catch { mode = Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode.All; return false; }
        return true;
    }
    
    protected void MsgFilterClick(object sender, EventArgs e)
    {
        Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode mode;
        NameValueCollection queryParams = new NameValueCollection();
        if ( Request.Form[UniqueID+"$msgFilter"]!=null && DisplayModeIsOk(Request.Form[UniqueID+"$msgFilter"], out mode))
        {
            queryParams.Add(dispModeVar, HttpUtility.UrlEncode(Request.Form[UniqueID + "$msgFilter"]));
        }
        Response.Redirect(GetRedirectUrl(queryParams));
         
    }

    protected override void OnInit(EventArgs e)
    {
        Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.DisplayMode mode;
        if (Request.QueryString[dispModeVar] != null && DisplayModeIsOk(Request.QueryString[dispModeVar], out mode))
        {
            topics.Component.Parameters["DisplayMode"] = mode.ToString();
        }

        NameValueCollection queryParams;

        queryParams = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
        
        string pTemplate = topics.Component.Parameters["PagingPageTemplate"];

        string key = string.Empty;
        
        int index = pTemplate.IndexOf("?");
        NameValueCollection oldParams = new NameValueCollection();

        if (index != -1)
        {
            oldParams =  HttpUtility.ParseQueryString(pTemplate.Substring(index + 1, pTemplate.Length - index-1));
            if (oldParams.Count > 0)
            {
                foreach (String name in oldParams.AllKeys)
                        queryParams.Set(name, oldParams[name]);
            }
        }
        
        topics.Component.Parameters["PagingPageTemplate"] = String.Concat(index == -1 ? pTemplate : pTemplate.Substring(0, index), queryParams.Count == 0 ? String.Empty : "?" + ConstructQueryString(queryParams));

        pTemplate = topics.Component.Parameters["PagingIndexTemplate"];
        index = pTemplate.IndexOf("?");
        oldParams = new NameValueCollection();

        if (index != -1)
        {
            oldParams = HttpUtility.ParseQueryString(pTemplate.Substring(index + 1, pTemplate.Length - index - 1));
            if (oldParams.Count > 0)
            {
                foreach (String name in oldParams.AllKeys)
                    queryParams.Set(name, oldParams[name]);
            }
        }
        queryParams.Remove(pageVar);
        topics.Component.Parameters["PagingIndexTemplate"] = String.Concat(index == -1 ? pTemplate : pTemplate.Substring(0, index), queryParams.Count == 0 ? String.Empty : "?" + ConstructQueryString(queryParams));
    }

    private string GetRedirectUrl(NameValueCollection queryParams)
    {
        NameValueCollection query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
        //if ( query[dispModeVar]!=null && query[dispModeVar] != topics.Component.Parameters["DisplayMode"])
        query.Remove(pageVar);    
        query.Remove(dispModeVar);
        query.Remove(BXCsrfToken.TokenKey);
        if (queryParams != null)
            query.Add(queryParams);

        UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
        uri.Query = query.ToString();

        return uri.Uri.ToString();
    }
</script>

