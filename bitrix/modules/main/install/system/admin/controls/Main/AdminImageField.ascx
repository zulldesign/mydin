<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AdminImageField.ascx.cs" Inherits="bitrix_ui_AdminImageField" %>

<% 
if (Editable)
{
	%><div runat="server" ID="Lbl" visible="false"><%= GetMessage("QuestionText.AboutNewImageUploading") %></div>
	<% if(!string.IsNullOrEmpty(NewImageUploadingHint)) {%>
	    <div><%= HttpUtility.HtmlEncode(NewImageUploadingHint)%></div>
	<%} %>
	<asp:FileUpload ID="Up" runat="server" EnableViewState="False" />
	<span id="S" runat="server" enableviewstate="false">
		<br /><%= GetMessage("LegendText.Description") %>
		<br /><asp:TextBox ID="Desc" runat="server" />
	</span><%
}
%>
<div id="D" runat="server" visible="false" enableviewstate="false">

<% 
if (Editable)
{
	%><asp:Literal ID="ImageParameters" runat="server" /><%
} 
%>

	<a id="Img" runat="server" title="<%$ Loc:AnchorTitle.Img %>" target="_blank"><asp:Image ID="ImageDisplay" runat="server" AlternateText="<%$ Loc:ImageAlternateText.ImageDisplay %>" /><br /></a>
	
<% 
if (Editable)
{
	%><asp:CheckBox ID="Del" runat="server" Text="<%$ Loc:CheckBoxText.Del %>" /><%
}
%>
</div>