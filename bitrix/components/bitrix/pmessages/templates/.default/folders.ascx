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
	ID="folders" 
	runat="server" 
	ComponentName="bitrix:pmessages.folder.list" 
	Template=".default" 
	ThemeCssFilePath="" 
	ColorCssFilePath=""
	MessageReadUrlTemplate="<%$ Results:MessageReadUrlTemplate %>" 
	FolderUrlTemplate="<%$ Results:FolderUrlTemplate %>" 
/>
