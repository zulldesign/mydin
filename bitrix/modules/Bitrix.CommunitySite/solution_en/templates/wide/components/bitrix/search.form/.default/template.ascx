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

<div id="search" onkeypress="return FireDefaultButton(event,'<%=Search.ClientID %>')">
	<div class="rounded-box">
		<b class="r1 top"></b>
		<div class="search-inner-box"><asp:TextBox ID="query" runat="server" /></div>
		<b class="r1 bottom"></b>
	</div>
	<div id="search-button">
		<div class="search-button-box"><b class="r1 top"></b><asp:Button ID="Search" runat="server" Text="<%$ LocRaw:Search %>" OnClick="Search_Click"  /><b class="r1 bottom"></b></div>
	</div>
</div>