<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.UI.BXComponentTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.Services" %>

<script runat="server">
	protected void Search_Click(object sender, EventArgs args)
	{
		BXParamsBag<object> p = new BXParamsBag<object>();
		p.Add("query", query.Text);

		Component.ProcessCommand("search", p, new List<string>());
	}
</script>

<asp:Panel ID="Container" runat="server" DefaultButton="search" >
	<table style="border: none 0px; padding: 0px; border-spacing: 0px; border-collapse: collapse;
		width: 100%">
		<tr>
			<td style="width: 100%">
				<asp:TextBox ID="query" runat="server" Width="100%" /></td>
			<td>
				<asp:Button ID="search" runat="server" Text="<%$ LocRaw:Search %>" OnClick="Search_Click" />
			</td>
		</tr>
	</table>
</asp:Panel>