<%@ Control Language="C#" AutoEventWireup="true" %>
<%= Bitrix.Services.BXLoc.GetMessage(AppRelativeVirtualPath, "Content") %>
<script runat="server">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		Page.Title = Bitrix.Services.BXLoc.GetMessage(AppRelativeVirtualPath, "Title");
		Bitrix.UI.BXAdminPage page = Page as Bitrix.UI.BXAdminPage;
		if (page != null)
			page.MasterTitle = Bitrix.Services.BXLoc.GetMessage(AppRelativeVirtualPath, "Title");
	}
</script>