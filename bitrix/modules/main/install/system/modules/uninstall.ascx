<%@ Control Language="C#" AutoEventWireup="true" CodeFile="uninstall.ascx.cs" Inherits="bitrix_modules_DefaultUninstallWizard" %>
<style type="text/css">
div.uninstall
{
	margin-top: 20px;
	margin-bottom: 20px;
}
</style>
<%--<div class="uninstall">
	<h1>
		Параметры деинсталляции модуля</h1>
</div>--%>
<bx:BXMessage ID="BXMessage1" runat="server" CssClass="error" IconClass="error" Title="<%$ Loc:Message.Attention %>" />
<div class="uninstall">
	<p>
		<asp:CheckBox ID="cbDb" runat="server" Text="<%$ Loc:CheckBoxText.DeleteTables %>" />
	</p>
</div>
<div class="uninstall">
	<asp:Button ID="btnUninstall" runat="server" Text="<%$ LocRaw:ButtonText.Uninstall %>" OnClick="btnUninstall_Click" />
	<asp:Button ID="btnCancel" runat="server" Text="<%$ LocRaw:Kernel.Cancel %>" OnClick="btnCancel_Click" />
</div>
