<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="UpdateSystemSettings.aspx.cs" Inherits="bitrix_admin_UpdateSystemSettings" Title="Update System Settings" %>
<%@ OutputCache NoStore="true" Location="None" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<% if (mainModuleVersion != null)  { %>
	<input type="hidden" value="<%= Encode(mainModuleVersion.ToString()) %>" />
	<% } %>
	<asp:PlaceHolder ID="Content" runat="server" />
</asp:Content>