<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="Settings.aspx.cs" Inherits="bitrix_admin_Settings" Title="<%$ Loc:PageTitle.GlobalSettings %>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:DropDownList ID="ddlEntitySelector" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlEntitySelector_SelectedIndexChanged">
	</asp:DropDownList>
	<br/>
	<%--<asp:UpdatePanel ID="upSettingsForm" runat="server">
		<Triggers>
			<asp:AsyncPostBackTrigger ControlID="ddlEntitySelector" EventName="SelectedIndexChanged" />
		</Triggers>
		<ContentTemplate>--%>
			<asp:PlaceHolder runat="server" ID="Settings" />
		<%--</ContentTemplate>
	</asp:UpdatePanel>--%>
</asp:Content>
