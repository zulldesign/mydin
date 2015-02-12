<%@ Reference Control="~/bitrix/components/bitrix/forum.topic.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumTopicListTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Services" %>

<%
	if (Component.FatalError != ForumTopicListComponent.ErrorCode.None)
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
	<div class="forum-new-post">
		<% if (Component.Auth.CanTopicCreate) { %>
		<a href="<%= Component.NewTopicHref %>"><span><%= GetMessage("NewTopic") %></span></a>
		<% } else {%>
		&nbsp;
		<% } %>
	</div>
	<div class="forum-clear-float"></div>
</div>

<div class="forum-header-box">
	<div class="forum-header-options">
	<% 
	for (int i = 0; i < Component.HeaderLinks.Count; i++) 
	{ 
		ForumTopicListComponent.LinkInfo link = Component.HeaderLinks[i];
		%><% if (i != 0) { %>&nbsp;&nbsp;<% } %><span class="<%= link.CssClass ?? ("forum-option-" + i) %>"><a href="<%= link.Href %>"<%= link.CustomAttrs %>><%= link.Title %></a></span><%
	}	
	%>
	</div>
	<div class="forum-header-title"><span><%= Component.Forum.Name %></span></div>
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
	if (ddl.value == '<%= ForumTopicListComponent.TopicOperation.Delete %>')
		return window.confirm('<%= GetMessageJS("ConfirmDelete") %>');
	return true;
}

function <%= ClientID %>_SubscribeForum()
{
	if(typeof(Bitrix) == "undefined" || typeof(Bitrix.Dialog) == "undefined")
		return true;

	var dlg = Bitrix.Dialog.get("Bitrix$ForumSubscriptionDialog");
    if (!dlg)
    {
		var options = new Object();
		options["Subscribe.ActionUrl.NewTopics"] = "<%= Bitrix.Services.Js.BXJSUtility.Encode(GetForumOperationHref(ForumTopicListComponent.ForumOperation.TopicSubscribe)) %>";
		options["Subscribe.ActionUrl.NewPosts"] = "<%= Bitrix.Services.Js.BXJSUtility.Encode(GetForumOperationHref(ForumTopicListComponent.ForumOperation.PostsSubscribe)) %>";
	
		options["Subscribe.Label.NewTopics"] = "<%= GetMessageJS("Subscribe.Label.NewTopics") %>";
		options["Subscribe.Label.NewPosts"] = "<%= GetMessageJS("Subscribe.Label.NewPosts") %>";
		options["Subscribe.Label.Title"] = "<%= GetMessageJS("Subscribe.Label.Title") %>";

		dlg = Bitrix.ForumSubscriptionDialog.create("Bitrix$ForumSubscriptionDialog", "Bitrix$ForumSubscriptionDialog", "<%= GetMessageJS("Subscribe.Window.Title") %>", options);                        
	}
	else if(dlg.isOpened()) 
		return false;

    dlg.open();	
	return false;
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
					<th class="forum-column-views"><span><%= GetMessage("Views") %></span></th>
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
		ForumTopicListComponent.TopicInfo info = Component.Topics[i];
		cssClass.Length = 0;
		if (i == 0)
			cssClass.Append("forum-row-first ");
		if (i == Component.Topics.Count - 1)
			cssClass.Append("forum-row-last ");
		cssClass.Append(i % 2 == 0 ? "forum-row-odd" : "forum-row-even"); //because of zero-based index
			
		
		statusHtml.Length = 0;
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

		string iconCss;
		string iconTitle;
		if (info.Topic.MovedTo > 0)
		{
			iconCss = "forum-icon-sticky";
			iconTitle = GetMessageRaw("Title.Moved");
		}
		else if (info.Topic.Closed)
		{
			if (info.Topic.StickyIndex > 0)
			{
				iconCss = "forum-icon-sticky-closed" + (info.HasNewPosts ? "-newposts" : string.Empty);
				iconTitle = GetMessageRaw("Title.StickyClosed") + (info.HasNewPosts ? (" (" + GetMessageRaw("Title.HasNewMessages") + ")") : string.Empty);
			}
			else
			{
				iconCss = "forum-icon-closed" + (info.HasNewPosts ? "-newposts" : string.Empty);
				iconTitle = GetMessageRaw("Title.Closed") + (info.HasNewPosts ? (" (" + GetMessageRaw("Title.HasNewMessages") + ")") : string.Empty);
			}
		}
		else if (info.Topic.StickyIndex > 0)
		{
			iconCss = "forum-icon-sticky" + (info.HasNewPosts ? "-newposts" : string.Empty);
			iconTitle = GetMessageRaw("Title.Sticky") + (info.HasNewPosts ? (" (" + GetMessageRaw("Title.HasNewMessages") + ")") : string.Empty);
		}
		else
		{
			iconCss = "forum-icon" + (info.HasNewPosts ? "-newposts" : "-default");
			//iconTitle = info.HasNewPosts ? GetMessageRaw("Title.HasNewMessages") : GetMessageRaw("Title.NoNewMessages");
			iconTitle = String.Empty;
		}

		
		if (statusHtml.Length > 0)
			statusHtml.Append(":&nbsp;");
%>
 				<tr class="<%= cssClass.ToString() %>">
					<td class="forum-column-icon">
						<div class="forum-icon-container">
							<div class="forum-icon <%= iconCss %>" title="<%= iconTitle %>"><!-- ie --></div>
						</div>
					</td>
					<td class="forum-column-title">
						<div class="forum-item-info">
							<div class="forum-item-name"><%= statusHtml %><span class="forum-item-title"><a href="<%= info.TopicHref %>"><%= info.TitleHtml %></a></span></div>
							<% if (!string.IsNullOrEmpty(info.DescriptionHtml)) { %>
							<span class="forum-item-desc"><%= info.DescriptionHtml%></span><span class="forum-item-desc-sep">&nbsp;&middot; </span>
							<% } %>
							<span class="forum-item-author"><span><%= GetMessage("Author") %>:</span>&nbsp;<%= info.Topic.AuthorId == 0 ? GetMessageRaw("Guest") : string.Empty %><%= info.AuthorNameHtml %></span>
						</div>
					</td>
				<% if (info.Topic.MovedTo == 0) { %>
					<td class="forum-column-replies<%= info.HiddenReplies > 0 ?  " forum-cell-hidden" : string.Empty %>"><span><%= info.Replies.ToString("#,0") %><% if (info.HiddenReplies > 0) { %> (<a href="<%= info.TopicHref %>" title="<%= GetMessage("HiddenReplies") %>"><%= info.HiddenReplies %></a>)<% } %></span></td>
					<td class="forum-column-views"><span><%= info.Topic.Views.ToString("#,0") %></span></td>
				<% } else { %>
					<td class="forum-column-replies"><span>&nbsp;</span></td>
					<td class="forum-column-views"><span>&nbsp;</span></td>
				<% } %>
					<td class="forum-column-lastpost">
					<% if (Component.CanModerate) { %>
						<div class="forum-select-box"><input type="checkbox" name="<%= UniqueID %>$operate" value="<%= info.Topic.Id %>" onclick="<%= ClientID %>_SelectRow(this.parentNode.parentNode.parentNode)" /></div>
					<% } %>
					<% if (info.Topic.MovedTo == 0) { %>
						<div class="forum-lastpost-box">
							<span class="forum-lastpost-date"><a href="<%= info.LastPostHref %>"><%= info.Topic.LastPostDate.ToString("g") %></a></span>
							<span class="forum-lastpost-title"><span class="forum-lastpost-author"><%= info.Topic.LastPosterId == 0 ? GetMessageRaw("Guest") : string.Empty %><%= info.LastPosterNameHtml %></span></span>
						</div>
					<% } %>
					<% if (!Component.CanModerate && info.Topic.MovedTo != 0) { %>&nbsp;<% } %>
					</td>
				</tr>
<%
	}
%>
			</tbody>
			<tfoot>
				<tr>
					<td colspan="5" class="forum-column-footer">
						<div class="forum-footer-inner">
							<% if (Component.CanModerate) { %>
							<div class="forum-topics-moderate">
								<select id="<%= ClientID %>" name="<%= UniqueID %>$operation">
									<option value=""><%= GetMessage("ManageTopics") %></option>
									<% if (Component.Auth.CanTopicOpenClose) { %>
									<option value="<%= ForumTopicListComponent.TopicOperation.Close %>"><%= GetMessage("Option.CloseTopics") %></option>
									<option value="<%= ForumTopicListComponent.TopicOperation.Open %>"><%= GetMessage("Option.OpenTopics") %></option>
									<% } %>
									<% if (Component.Auth.CanApprove) { %>
									<option value="<%= ForumTopicListComponent.TopicOperation.Hide %>"><%= GetMessage("Option.HideTopics") %></option>
									<option value="<%= ForumTopicListComponent.TopicOperation.Approve %>"><%= GetMessage("Option.ApproveTopics") %></option>
									<% } %>
									<% if (Component.Auth.CanTopicStick) { %>
									<option value="<%= ForumTopicListComponent.TopicOperation.Stick %>"><%= GetMessage("Option.StickTopics") %></option>
									<option value="<%= ForumTopicListComponent.TopicOperation.Unstick %>"><%= GetMessage("Option.UnstickTopics") %></option>
									<% } %>
									<% if (Component.Auth.CanTopicMove) { %>
									<option value="<%= ForumTopicListComponent.TopicOperation.Move %>"><%= GetMessage("Option.MoveTopics") %></option>
									<% } %>
									<% if (Component.Auth.CanTopicDelete) { %>
									<option value="<%= ForumTopicListComponent.TopicOperation.Delete %>"><%= GetMessage("Option.DeleteTopics") %></option>
									<% } %>
								</select>&nbsp;<asp:Button runat="server" ID="OK" Text="<%$ LocRaw:Kernel.OK %>" OnClick="OKClick" />
							</div>
							<span class="forum-footer-option forum-footer-selectall forum-footer-option-first"><a href="javascript:void(0)" onclick="<%= ClientID %>_SelectAll(this.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode)"><%= GetMessage("SelectAll") %></a></span>
							<% } else {%>
							&nbsp;
							<% } %>
						</div>
					</td>
				</tr>
			</tfoot>
<% } else { %>
			<tbody>
 				<tr class="forum-row-first forum-row-last forum-row-odd">
					<td class="forum-column-alone">
						<div class="forum-empty-message"><%= GetMessageRaw("NoTopicsHere") %><% if (Component.Auth.CanTopicCreate) { %><br /><%= string.Format(GetMessageRaw("YouCanCreateNewTopic"), @"<a href=""" + Component.NewTopicHref + @""">", @"</a>")%><% } %></div>
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

	<div class="forum-new-post">
		<% if (Component.Auth.CanTopicCreate) { %>
		<a href="<%= Component.NewTopicHref %>"><span><%= GetMessage("NewTopic") %></span></a>
		<% } else {%>
		&nbsp;
		<% } %>
	</div>

	<div class="forum-clear-float"></div>
</div>

</div>

<script runat="server">
	
	private const string TargetParameter = "_target";
	private const string OperationParameter = "_action";
	private const string PostIdParameter = "_id";
	
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		OK.OnClientClick = String.Format("if (!{0}_OKClick()) return false;", ClientID);
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		if (Component.FatalError != ForumTopicListComponent.ErrorCode.None)
			return;

		if (Component.Topics.Count == 0)
			Results["PagingTotalRecordCount"] = "1";

		AddSubscriptionLink();

		if (Request == null)
			return;
		
		string target = Request.QueryString[TargetParameter];
		if (target != "forum")
			return;
		
		if (Request.QueryString[OperationParameter] == null)
			return;
		
		if (!BXCsrfToken.CheckTokenFromRequest(Request.QueryString))
			return;

		if (target == "forum")
			OperateForum();
	}

	private string GetForumOperationHref(ForumTopicListComponent.ForumOperation operation)
	{
		NameValueCollection query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
		query.Set(TargetParameter, "forum");
		query.Set(OperationParameter, operation.ToString().ToLower());
		BXCsrfToken.SetToken(query);

		UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
		uri.Query = query.ToString();
		return uri.Uri.ToString();
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);

		if (Component.Auth==null || !Component.Auth.CanSubscribe || Component.UserSubscription != null)
			return;
		
		string path = Bitrix.IO.BXPath.Combine(AppRelativeTemplateSourceDirectory, "subscription.js");
		if (Bitrix.IO.BXSecureIO.FileExists(path))
		{
			BXPage.Scripts.RequireUtils();
			BXPage.RegisterScriptInclude("~/bitrix/controls/Main/dialog/js/messages.js.aspx?lang=" + HttpUtility.UrlEncode(Bitrix.Services.BXLoc.CurrentLocale));
			BXPage.RegisterScriptInclude("~/bitrix/controls/Main/dialog/js/dialog_base.js");
			BXPage.RegisterScriptInclude(path);
			BXPage.RegisterStyle("~/bitrix/controls/Main/dialog/css/dialog_base.css");
		}
	}
	
	private void AddSubscriptionLink()
	{
		if (!Component.Auth.CanSubscribe)
			return;

		List<ForumTopicListComponent.LinkInfo> links = Component.HeaderLinks;
		if (links == null)
			links = new List<ForumTopicListComponent.LinkInfo>();

		if (Component.UserSubscription == null)
		{
			links.Add(
				new ForumTopicListComponent.LinkInfo(
					GetForumOperationHref(ForumTopicListComponent.ForumOperation.TopicSubscribe),
					GetMessage("Option.SubscribeForum"),
					"forum-option-subscribe",
					@" title=""" + GetMessage("Subscribe.Link.Title") + @""" onclick=""return "+ ClientID + @"_SubscribeForum();"""
				)
			);
		}
		else
		{
			links.Add(
				new ForumTopicListComponent.LinkInfo(
					GetForumOperationHref(ForumTopicListComponent.ForumOperation.Unsubscribe),
					GetMessage("Option.UnSubscribeForum"),
					"forum-option-unsubscribe",
					@" title=""" + GetMessage("UnSubscribe.Link.Title") + @""""
					
				)
			);
		}
	}
	
	private void OKClick(object sender, EventArgs e)
	{
		if (!Component.CanModerate)
			return;
		
		string operationString = Request.Form[UniqueID + "$operation"];
		if (string.IsNullOrEmpty(operationString) || !Enum.IsDefined(typeof(ForumTopicListComponent.TopicOperation), operationString))
			return;

		ForumTopicListComponent.TopicOperation operation = (ForumTopicListComponent.TopicOperation)Enum.Parse(typeof(ForumTopicListComponent.TopicOperation), operationString);
				
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
		Response.Redirect(GetRedirectUrl());
	}

	private void OperateForum()
	{
		string operation = Request.QueryString[OperationParameter];
		ForumTopicListComponent.ForumOperation op;
		try
		{
			op = (ForumTopicListComponent.ForumOperation)Enum.Parse(typeof(ForumTopicListComponent.ForumOperation), operation, true);
		}
		catch
		{
			return;
		}
		Component.DoForumOperation(op);
		Response.Redirect(GetRedirectUrl());
	}

	private string GetRedirectUrl()
	{
		NameValueCollection query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
		query.Remove(TargetParameter);
		query.Remove(OperationParameter);
		query.Remove(BXCsrfToken.TokenKey);
		
		UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
		uri.Query = query.ToString();
		
		return uri.Uri.ToString();
	}
	
</script>
