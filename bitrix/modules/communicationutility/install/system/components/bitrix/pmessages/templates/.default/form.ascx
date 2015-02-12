<%@ Reference Control="~/bitrix/components/bitrix/pmessages/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.CommunicationUtility.Components.PrivateMessagesTemplate" %>

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
	ID="form" 
	runat="server" 
	ComponentName="bitrix:pmessages.message.form" 
	Template=".default" 
	ThemeCssFilePath="" 
	ColorCssFilePath=""
	TopicId="<%$ Results:TopicId %>" 
	ParentMessageId="<%$ Results:ParentMessageId %>"
	MessageReadUrlTemplate = "<%$ Results:MessageReadUrlTemplate %>"
	TopicReadUrlTemplate = "<%$ Results:TopicReadUrlTemplate %>"
	MaxReceiversCount = "<%$ Results:MaxReceiversCount %>"
	MaxMessageCount= "<%$ Results:MaxMessageCount %>" 
	UserProfileUrlTemplate="<%$Results:UserProfileUrlTemplate%>"
	MessageSendingInterval = "<%$Results:MessageSendingInterval %>"
	Receivers="<%$ Results:Receivers %>"
	Mode="<%$ Results:FormMode %>"
    MessageId="<%$ Results:MessageId %>"
/>
