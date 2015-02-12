<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="404 Not Found" %>
<%@ Import namespace="Bitrix.Configuration" %>
<%@ Import namespace="Bitrix" %>

<script runat="server" type="text/javascript">

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		string originatingUrl = null;
		try
		{
			originatingUrl = Bitrix.Services.BXError404Manager.GetOriginatingUrl().PathAndQuery;
		}
		catch {}

		string imageUrl = BXOptionManager.GetOptionString("Bitrix.CommunitySite", "SiteFolder", BXSite.Current.DirectoryAbsolutePath, BXSite.Current.Id);
		image404.Src = ResolveUrl(imageUrl + "assets/errors/404.gif");
		
		Bitrix.Services.BXError404Manager.Set404Status(Response);
	}
	
</script>

<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server">
<div align="center">
<br /><br />
<img  width="293" height="123" src="" id="image404" runat="server" alt="404 Not Found" />
</div>


</asp:Content>