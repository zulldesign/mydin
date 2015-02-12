<%@ Reference Control="~/bitrix/components/bitrix/forum.subscription/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumSubscriptionTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>

<% 
	if (Component.FatalError != ForumSubscriptionComponent.ErrorCode.None)
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

<div class="forum-content">

<div class="forum-navigation-box forum-navigation-top">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="forum-" />
	</div>
	<div class="forum-clear-float"></div>
</div>

<div class="forum-header-box">
	<div class="forum-header-title"><span><%= GetMessage("ManageSubscriptions") %></span></div>
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
	if (ddl.value == '<%= ForumSubscriptionComponent.SubscriptionOperation.Unsubscribe %>')
		return window.confirm('<%= GetMessageJS("ConfirmUnsubscribe") %>');
	return true;
}
</script>
</bx:InlineScript>

<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">
			<table cellspacing="0" class="forum-table forum-topic-list">
<% if (Component.Subscriptions.Count > 0) { %>
			<thead>
				<tr>
					<th class="forum-column-title" colspan="2"><div class="forum-head-title"><span><%= GetMessage("HeadTitle") %></span></div></th>
					<th class="forum-column-replies"><span><%= GetMessageRaw("Subscribed.For") %></span></th>
					<th class="forum-column-lastpost"><span><%= GetMessage("LastPost") %></span></th>
				</tr>
			</thead>
			<tbody>
<%
	StringBuilder cssClass = new StringBuilder();
	StringBuilder statusHtml = new StringBuilder();
	string statusFormat = @"<span class=""{0}"">{1}</span>";
	for (int i = 0; i < Component.Subscriptions.Count; i++)
	{
		ForumSubscriptionComponent.SubscriptionInfo info = Component.Subscriptions[i];
		cssClass.Length = 0;
		if (i == 0)
			cssClass.Append("forum-row-first ");
		if (i == Component.Subscriptions.Count - 1)
			cssClass.Append("forum-row-last ");
		cssClass.Append(i % 2 == 0 ? "forum-row-odd" : "forum-row-even"); //because of zero-based index
			
		statusHtml.Length = 0;

		if (info.Topic != null)
		{
			if (info.Topic.StickyIndex > 0)
			{
				cssClass.Append(" forum-row-sticky");
				statusHtml.AppendFormat(statusFormat, "forum-status-sticky", GetMessageRaw("Status.Sticky"));
			}
			if (info.Topic.Closed)
			{
				cssClass.Append(" forum-row-closed");
				if (statusHtml.Length > 0)
					statusHtml.Append(", ");
				statusHtml.AppendFormat(statusFormat, "forum-status-closed", GetMessageRaw("Status.Closed"));
			}
			if (info.Topic.MovedTo > 0)
			{

				cssClass.Append(" forum-row-moved");
				if (statusHtml.Length > 0)
					statusHtml.Append(", ");
				statusHtml.AppendFormat(statusFormat, "forum-status-moved", GetMessageRaw("Status.Moved"));
			}
			if (!info.Topic.Approved)
			{
				cssClass.Append(" forum-row-hidden");
			}
		}

		string iconCss = "forum-icon-default";
		string iconTitle = String.Empty;

		if (info.Topic != null)
		{
			if (info.Topic.MovedTo > 0)
			{
				iconCss = "forum-icon-sticky";
				iconTitle = GetMessageRaw("Title.Moved");
			}
			else if (info.Topic.Closed)
			{
				if (info.Topic.StickyIndex > 0)
				{
					iconCss = "forum-icon-sticky-closed";
					iconTitle = GetMessageRaw("Title.StickyClosed");
				}
				else
				{
					iconCss = "forum-icon-closed";
					iconTitle = GetMessageRaw("Title.Closed");
				}
			}
			else if (info.Topic.StickyIndex > 0)
			{
				iconCss = "forum-icon-sticky";
				iconTitle = GetMessageRaw("Title.Sticky");
			}
		}

		
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
							<div class="forum-item-name"><%= statusHtml %><span class="forum-item-title"><a href="<%= info.DetailHref %>"><%= info.TitleHtml %></a></span></div>
							<% if (!string.IsNullOrEmpty(info.DescriptionHtml)) { %>
							<span class="forum-item-desc"><%= info.DescriptionHtml%></span><span class="forum-item-desc-sep">&nbsp;&middot; </span>
							<% } %>
						</div>
					</td>	
					<td class="forum-column-replies">
					<span>
						<% if (info.Topic != null) { %>
							<%= GetMessage("Subscribed.For.Topic") %>
						<%} else if (info.Forum != null) {%>
							<%= (info.Subscription.OnlyTopic ? GetMessage("Subscribed.For.ForumNewTopics") : GetMessage("Subscribed.For.ForumNewPosts"))%>
						<%} %>
					</span>
					</td>
					<td class="forum-column-lastpost">					
						<div class="forum-select-box"><input type="checkbox" name="<%= UniqueID %>$operate" value="<%= info.Subscription.Id %>" onclick="<%= ClientID %>_SelectRow(this.parentNode.parentNode.parentNode)" /></div>
						<% if (info.LastPostHref != null && info.LastPostId > 0) { %>
						<div class="forum-lastpost-box">
							<span class="forum-lastpost-date"><a href="<%= info.LastPostHref %>"><%= info.LastPostDate.ToString("g") %></a></span>
							<span class="forum-lastpost-title"><span class="forum-lastpost-author"><%= info.LastPosterId == 0 ? GetMessageRaw("Guest") : string.Empty %><%= info.LastPosterNameHtml %></span></span>
						</div>
						<%} %>
					</td>
				</tr>
<%
	}
%>
			</tbody>
			<tfoot>
				<tr>
					<td colspan="4" class="forum-column-footer">
						<div class="forum-footer-inner">
							
							<div class="forum-topics-moderate">
								<select id="<%= ClientID %>" name="<%= UniqueID %>$operation">
									<option value=""><%= GetMessage("ManageSubscriptions") %></option>
									<option value="<%= ForumSubscriptionComponent.SubscriptionOperation.Unsubscribe %>"><%= GetMessage("Option.Unsubscribe")%></option>
								</select>&nbsp;<asp:Button runat="server" ID="OK" Text="<%$ LocRaw:Kernel.OK %>" OnClick="OKClick" />
							</div>
							<span class="forum-footer-option forum-footer-selectall forum-footer-option-first"><a href="javascript:void(0)" onclick="<%= ClientID %>_SelectAll(this.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode)"><%= GetMessage("SelectAll") %></a></span>
							
						</div>
					</td>
				</tr>
			</tfoot>
<% } else { %>
			<tbody>
 				<tr class="forum-row-first forum-row-last forum-row-odd">
					<td class="forum-column-alone">
						<div class="forum-empty-message"><%= GetMessage("NoSubscriptionsHere") %><br /><%= GetMessage("HowToSubscribe")%></div>
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
		<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager"  Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="forum-" />
	</div>
	<div class="forum-clear-float"></div>
</div>

</div>

<script runat="server">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		OK.OnClientClick = string.Format("if (!{0}_OKClick()) return false;", ClientID);
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		if (Component.FatalError != ForumSubscriptionComponent.ErrorCode.None)
			return;
		
		if (Component.Subscriptions.Count == 0)
			Results["PagingTotalRecordCount"] = "1";
	}
	
	private void OKClick(object sender, EventArgs e)
	{
		//if (!Component.CanModerate)
			//return;
		
		string operationString = Request.Form[UniqueID + "$operation"];
		if (string.IsNullOrEmpty(operationString) || !Enum.IsDefined(typeof(ForumSubscriptionComponent.SubscriptionOperation), operationString))
			return;

		ForumSubscriptionComponent.SubscriptionOperation operation = (ForumSubscriptionComponent.SubscriptionOperation)Enum.Parse(typeof(ForumSubscriptionComponent.SubscriptionOperation), operationString);
				
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
		
		Component.DoOperation(operation, ids);
	}
</script>
