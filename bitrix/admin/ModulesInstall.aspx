<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="ModulesInstall.aspx.cs" Inherits="bitrix_admin_ModulesInstall" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<bx:BXMessage runat="server" ID="errorMessage" CssClass="error" IconClass="error" Visible="false"/>
	<asp:PlaceHolder runat="server" ID="phWizard"></asp:PlaceHolder>
	<asp:Button ID="btnCancel" Visible="false" runat="server" Text="<%$ LocRaw:Kernel.Cancel %>" OnClick="Cancel" />
</asp:Content>

