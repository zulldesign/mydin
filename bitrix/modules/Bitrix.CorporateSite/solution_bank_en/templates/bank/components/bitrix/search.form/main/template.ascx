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

<div id="search" onkeypress="return FireDefaultButton(event, '<%= Bitrix.Services.Js.BXJSUtility.Encode(search.ClientID) %>');">	
	<div id="search-button">
		<asp:Button ID="search" runat="server" Text="<%$ LocRaw:Search %>" OnClick="Search_Click" />
	</div>
	<div class="search-box"><asp:TextBox ID="query" runat="server" Width="100%" /></div>
</div>
