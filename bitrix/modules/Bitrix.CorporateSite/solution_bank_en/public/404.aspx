<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main"  Title="404 Not Found" %>
<asp:Content ID="Content" ContentPlaceHolderID="BXContent" runat="server">
<bx:IncludeComponent 
	runat="server" 
	ID="SiteMap" 
	ComponentName="bitrix:system.PublicSiteMap" 
	Template="bank_sitemap" 
	Depth="3" 
	NumberOfColumns="2" 
/>
</asp:Content>
<script runat="server">
	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		Bitrix.Services.BXError404Manager.Set404Status(Response);
	}
</script>