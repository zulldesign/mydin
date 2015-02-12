<%@ Reference Control="~/bitrix/components/bitrix/pmessages/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.CommunicationUtility.Components.PrivateMessagesTemplate" %>
<%@ Import Namespace="Bitrix.CommunicationUtility.Components" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/pmessages.message.form/component.ascx" %>
<%
    PrivateMessageFormComponent formComponent = (PrivateMessageFormComponent)form.Component;
    PrivateMessageFormTemplate formTemplate = (PrivateMessageFormTemplate)formComponent.ComponentTemplate;
    %>

<script type="text/javascript">
	function GetForumPostTextArea()
	{
		return document.getElementById('<%=formTemplate.PostTextareaClientID %>');
	}
</script>

<% 
    if ( formComponent.FatalError == PrivateMessageFormComponent.ErrorCode.RestrictedAllowedMessageCountExceeded
         || formComponent.FatalError == PrivateMessageFormComponent.ErrorCode.RestrictedMoreThenOneMessageInInterval)
	{ 
		%>

		<div class="forum-note-box forum-note-error">
			<div class="forum-note-box-text"><%= formComponent.GetErrorHtml(formComponent.FatalError)%></div>
		</div>	

		<%
	}
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
	ID="topicread" 
	runat="server" 
	ComponentName="bitrix:pmessages.topic.read" 
	Template=".default" 
	ThemeCssFilePath="" 
	ColorCssFilePath=""
	PagingPageID="<%$ Results:PageId %>"
	TopicId="<%$ Results:TopicId %>" 
	MessageId="<%$ Results:MessageId %>"
	PagingIndexTemplate="<%$ Results:TopicReadUrlTemplate %>" 
	PagingPageTemplate="<%$ Results:TopicReadPageUrlTemplate %>" 
	TopicReadUrlTemplate="<%$ Results:TopicReadUrlTemplate %>" 
	TopicReplyUrlTemplate="<%$ Results:NewMessageUrlTemplate %>"
	TopicEditUrlTemplate="<%$ Results:TopicEditUrlTemplate %>"
	MessageReadUrlTemplate="<%$ Results:MessageReadUrlTemplate %>"
	MessageEditUrlTemplate="<%$ Results:MessageEditUrlTemplate %>"
	MessageQuoteUrlTemplate="<%$ Results:MessageQuoteUrlTemplate %>"  
	NewTopicUrlTemplate="<%$ Results:NewTopicUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>"
	FolderUrlTemplate="<%$ Results:TopicListUrlTemplate %>"  
	InviteUserUrlTemplate="<%$Results:InviteUserUrlTemplate%>"
	
/>
<% 
	if (
		 formComponent.FatalError == PrivateMessageFormComponent.ErrorCode.None 
         || formComponent.FatalError == PrivateMessageFormComponent.ErrorCode.RestrictedAllowedMessageCountExceeded
         || formComponent.FatalError == PrivateMessageFormComponent.ErrorCode.RestrictedMoreThenOneMessageInInterval) 
	{ 
%>
<bx:IncludeComponent 
	ID="form" 
	runat="server" 
	ComponentName="bitrix:pmessages.message.form" 
	Template=".default" 
	ThemeCssFilePath="" 
	ColorCssFilePath=""
	TopicId="<%$ Results:TopicId %>" 
	ParentMessageId="<%$ Results:ParentMessageId %>"
	TopicReadUrlTemplate = "<%$ Results:TopicReadUrlTemplate %>"
	MessageReadUrlTemplate = "<%$ Results:MessageReadUrlTemplate %>"
    MessageSendingInterval = "<%$Results:MessageSendingInterval %>" 
    MaxMessageCount= "<%$ Results:MaxMessageCount %>"  
	SetPageTitle="false"
/>
<%} %>
