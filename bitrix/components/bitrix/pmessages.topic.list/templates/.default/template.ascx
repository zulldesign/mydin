<%@ Reference Control="~/bitrix/components/bitrix/pmessages.topic.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.CommunicationUtility.Components.PrivateMessageTopicListTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.CommunicationUtility.Components" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Services" %>

<%
	if (Component.FatalError != PrivateMessageTopicListComponent.ErrorCode.None)
	{ 
		%>
		<div class="forum-content">
		<div class="forum-note-box forum-note-error">
			<div class="forum-note-box-text"><%= Component.GetErrorHtml(Component.FatalError) %></div>
		</div>	
		</div>
		<% 
		return;
	}
%>


<div class="pmessages-content">

<div class="forum-navigation-box forum-navigation-top">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" 
		Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="forum-" />
	</div>
	<div class="forum-clear-float"></div>
</div>

<div class="forum-header-box">
	<div class="forum-header-options">
	</div>
	<div class="forum-header-title"><span><%=GetMessage("Header")%><%= (Component.CurrentFolder!=null ? String.Format(GetMessage("InFolder"),Component.CurrentFolder.Title):"" )  %></span></div>
</div>

<bx:InlineScript runat="server" ID="Script">
<script type="text/javascript">
function <%= ClientID %>_SelectRow(row)
{
	if (row == null)
		return;

	if(row.className.match(/forum-row-selected/))
		row.className = row.className.replace(/\s*forum-row-selected/i, '');
	else
		row.className += ' forum-row-selected';
}
function <%= ClientID %>_SelectAll(table)
{
	if (table == null)
		return;
	var inputs = table.getElementsByTagName("INPUT");
	if (!inputs)
		return;
	var status;
	var hasStatus = false;
	for(var i = 0; i < inputs.length; i++)
	{
		var input = inputs[i];
		if (input.type != "checkbox")
			continue;
		
		if (!hasStatus)
		{
			status = !input.checked;
			hasStatus = true;
		}
		
		if (input.checked != status)
			input.click();
	}
}
function <%= ClientID %>_OKClick()
{
	var ddl = document.getElementById('<%= ClientID %>');
	if (ddl == null)
		return true;
	if (ddl.value == '')
		return false;
	if (ddl.value == '<%= PrivateMessageTopicListComponent.TopicOperation.Delete %>')
		return window.confirm('<%= GetMessageJS("ConfirmDelete") %>');
	return true;
}

</script>
</bx:InlineScript>

<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">
			<table cellspacing="0" class="forum-table forum-topic-list">
<% if (Component.Topics.Count > 0) { %>
			<thead>
				<tr>
					<th class="forum-column-title" colspan="2"><div class="forum-head-title"><span><%= GetMessage("Topics") %></span></div></th>
					<th class="forum-column-replies"><span><%= GetMessage("Replies") %></span></th>
					<th class="forum-column-lastpost"><span><%= GetMessage("LastPost") %></span></th>
				</tr>
			</thead>
			<tbody>
<%
	StringBuilder cssClass = new StringBuilder();
	StringBuilder statusHtml = new StringBuilder();
	string statusFormat = @"<span class=""{0}"">{1}</span>";
	for (int i = 0; i < Component.Topics.Count; i++)
	{
		PrivateMessageTopicListComponent.TopicInfo info = Component.Topics[i];
		cssClass.Length = 0;
		if (i == 0)
			cssClass.Append("forum-row-first ");
		if (i == Component.Topics.Count - 1)
			cssClass.Append("forum-row-last ");
		cssClass.Append(i % 2 == 0 ? "forum-row-odd" : "forum-row-even"); //because of zero-based index

        if (info.NotifyByEmail) cssClass.Append("topic-email-notification-enabled");    	
		
		statusHtml.Length = 0;

		string iconCss=info.UnreadMessageCount>0 ? "forum-icon-newposts":"forum-icon-default";
        string iconTitle = GetMessage(info.UnreadMessageCount > 0 ? "ToolTip.HaveUnreadMessages" : "ToolTip.NoUnreadMessages");

		
		if (statusHtml.Length > 0)
			statusHtml.Append(":&nbsp;");
%>
 				<tr class="<%= cssClass %>">
					<td class="forum-column-icon">
						<div class="forum-icon-container">
							<div class="forum-icon <%= iconCss %>" title="<%= iconTitle %>"><!-- ie --></div>
						</div>
					</td>
					<td class="forum-column-title">
						<div class="forum-item-info">
							<div class="forum-item-name"><%= statusHtml %><span class="forum-item-title">
							<a href="<%= info.TopicHref %>"><%= info.TitleHtml %></a></span></div>
							<span class="forum-item-author"><span><%= GetMessage("Author") %>:</span>&nbsp;
							<%= info.Topic.StarterId == 0 ? GetMessageRaw("Guest") : info.StarterNameHtml %><%= info.AuthorNameHtml %></span>
						</div>
					</td>

					<td class="forum-column-replies"><span><%= info.MessageCount %></span> <%if (info.UnreadMessageCount > 0)
                                                                              { %>
                                                                              <span class="prmessages-new-answers">(<%=info.UnreadMessageCount %>)</span>
                                                                              <% } %></td>
					<td class="forum-column-lastpost">

				    <div class="forum-select-box">
				        <input type="checkbox" name="<%= UniqueID %>$operate" value="<%= info.MappingId %>" onclick="<%= ClientID %>_SelectRow(this.parentNode.parentNode.parentNode)" />
				    </div>

				    <div class="forum-lastpost-box">
						<span class="forum-lastpost-date"><a href="<%= info.LastMessageHref %>"><%= info.Topic.LastMessageDate.ToString("g") %></a></span>
						<span class="forum-lastpost-title"><span class="forum-lastpost-author"><%= info.Topic.LastPosterId == 0 ? GetMessageRaw("Guest") : 
						                                                                           string.Empty %><%= info.LastPosterNameHtml %></span></span>
					</div>

					</td>
				</tr>
				<% } %>
			</tbody>
			<tfoot>
				<tr>
					<td colspan="5" class="forum-column-footer">
						<div class="forum-footer-inner">
							<div class="forum-topics-moderate">
								<select id="<%= ClientID %>" name="<%= UniqueID %>$operation">
									<option value=""><%= GetMessage("ManageTopics")%></option>
									<option value="<%= PrivateMessageTopicListComponent.TopicOperation.MarkAsRead %>"><%= GetMessage("Option.MarkAsRead")%></option>
									<option value="<%= PrivateMessageTopicListComponent.TopicOperation.RemoveReadMark %>"><%= GetMessage("Option.RemoveReadMark")%></option>

								
              <% if (Component.Auth.CanDeleteOwnTopics)
                 { %>
                            <option value="<%= PrivateMessageTopicListComponent.TopicOperation.Delete %>"><%= GetMessage("Option.DeleteTopics")%></option>
              	
              	<% } 
                            if (Component.Auth.CanManageFolders && Component.Folders.Count > 0)
                            { 
                                     %>
									    <optgroup label="<%= GetMessage("Option.MoveTopics")%>">
									    <option value="<%=PrivateMessageTopicListComponent.TopicOperation.Move.ToString() %>_0"><%= GetMessage("MyTopics")%></option>
									    <% foreach (Bitrix.CommunicationUtility.Components.PrivateMessageTopicListComponent.FolderInfo folder in Component.Folders)
                { %>
									    <option value="<%=PrivateMessageTopicListComponent.TopicOperation.Move.ToString() %>_<%=folder.Id  %>"><%= folder.TitleHtml%></option>
									    
									    <% } %>
									    </optgroup>
									<% } %>
									
								</select>&nbsp;<asp:Button runat="server" ID="OK" Text="<%$ LocRaw:Kernel.OK %>" OnClick="OKClick" />
							</div>
							
							<span class="forum-footer-option forum-footer-selectall forum-footer-option-first">
							    <a href="javascript:void(0)" 
							        onclick="<%= ClientID %>_SelectAll(this.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode)">
							        <%= GetMessage("SelectAll")%>
							    </a>
							</span>
						</div>
					</td>
				</tr>

			</tfoot>

<%
   
   } else { %>
			<tbody>
 				<tr class="forum-row-first forum-row-last forum-row-odd">
					<td class="forum-column-alone">
						<div class="forum-empty-message"><%= GetMessageRaw("NoTopicsHere") %> 
                         <br /><%= string.Format(GetMessageRaw("YouCanCreateNewTopic"), @"<a href=""" + Component.NewTopicHref + @""">", @"</a>")%>
                         </div>
					</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class="forum-column-footer">
						<div class="forum-footer-inner">&nbsp;</div>
					</td>
				</tr>
			</tfoot>
<% } %>
			</table>
		</div>
	</div>
</div>
<div class="forum-navigation-box forum-navigation-bottom">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager"  
		Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="forum-" />
	</div>

	<div class="forum-clear-float"></div>
</div>

</div>

<script runat="server">
    const string dispModeVar = "showmode";
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		OK.OnClientClick = String.Format("if (!{0}_OKClick()) return false;", ClientID);

	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		if (Component.FatalError != PrivateMessageTopicListComponent.ErrorCode.None)
			return;

		if (Component.Topics.Count == 0)
			Results["PagingTotalRecordCount"] = "1";

		if (Request == null)
			return;
	}
	
	private void OKClick(object sender, EventArgs e)
	{
		
		string operationString = Request.Form[UniqueID + "$operation"];
        string moveString = PrivateMessageTopicListComponent.TopicOperation.Move.ToString()+"_";
        int index =  operationString.IndexOf(moveString);
        PrivateMessageTopicListComponent.TopicOperation operation;
		if (string.IsNullOrEmpty(operationString) || (!Enum.IsDefined(typeof(PrivateMessageTopicListComponent.TopicOperation), operationString)
            && index != 0))
			return;
        int folderId=0;
        
        if (index == 0)
        {
            string strFolderId = operationString.Substring(moveString.Length);
            Int32.TryParse(strFolderId, out folderId);
            operation = PrivateMessageTopicListComponent.TopicOperation.Move;              
        }
        else 
		 operation = 
            (PrivateMessageTopicListComponent.TopicOperation)Enum.Parse(typeof(PrivateMessageTopicListComponent.TopicOperation), operationString);
				
		string[] idStrings = Request.Form.GetValues(UniqueID + "$operate");
		if (idStrings == null || idStrings.Length == 0)
			return;
		List<int> ids = new List<int>();
		
		foreach (string s in idStrings)
		{
			int id;
			if (!int.TryParse(s, out id) || id <= 0)
				continue;
			int i = ids.BinarySearch(id);
			if (i >= 0)
				continue;
			ids.Insert(~i, id);
		}
		
		if (ids.Count == 0)
			return;
		
		Component.DoOperation(operation, ids, folderId);
		Response.Redirect(GetRedirectUrl(null));
	}

	private string GetRedirectUrl(NameValueCollection queryParams)
	{
		NameValueCollection query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
        query.Remove(dispModeVar);
		query.Remove(BXCsrfToken.TokenKey);
        if ( queryParams!=null )
            query.Add(queryParams);
		
		UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
		uri.Query = query.ToString();
		
		return uri.Uri.ToString();
	}
	
</script>
