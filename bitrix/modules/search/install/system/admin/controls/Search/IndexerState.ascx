<%@ Control Language="C#" AutoEventWireup="true" CodeFile="IndexerState.ascx.cs"
	Inherits="bitrix_admin_controls_Search_IndexerState" EnableViewState="False" %>
<asp:UpdatePanel ID="StatePanel" runat="server">
	<contenttemplate>
<table style="border: none 0px; border-collapse: collapse; padding: 0px" >
	<tr>
		<td colspan="4">
					<asp:Panel ID="State" runat="server" Height="150px" ScrollBars="Vertical" Width="100%">
						<span style="font-size: 75%"><asp:Literal ID="StateText" runat="server"  /></span></asp:Panel>
		</td>
	</tr>
	<tr>
		<td colspan="4" style="font-size:75%"><asp:CheckBox runat="server" ID="DropIndex" Text="<%$ Loc:DropIndex %>" /></td>
	</tr>
	<tr>
		<% Restart.OnClientClick = string.Format("if (document.getElementById('{0}').checked && !confirm('{1}')) return false;", JSEncode(DropIndex.ClientID), GetMessageJS("DropIndexWarning")); %>
		<td><asp:Button ID="Restart" runat="server" Text="<%$ LocRaw:Restart %>" OnClick="Restart_Click" UseSubmitBehavior="false" /></td>
		<td><asp:Button ID="Stop" runat="server" Text="<%$ LocRaw:Stop %>" OnClick="Stop_Click" UseSubmitBehavior="false" /></td>
		<td><asp:Button ID="Resume" runat="server" Text="<%$ LocRaw:Resume %>" OnClick="Resume_Click" UseSubmitBehavior="false" /></td>
		<td><asp:Button ID="Refresh" runat="server" Text="<%$ LocRaw:Refresh %>" UseSubmitBehavior="false" /></td>
	</tr>
	
</table>
</contenttemplate>
	<triggers>
					<asp:AsyncPostBackTrigger ControlID="Restart" />
					<asp:AsyncPostBackTrigger ControlID="Stop" />
					<asp:AsyncPostBackTrigger ControlID="Resume" />
					<asp:AsyncPostBackTrigger ControlID="Refresh" />
				</triggers>
</asp:UpdatePanel>