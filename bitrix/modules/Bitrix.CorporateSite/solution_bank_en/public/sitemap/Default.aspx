<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="Site Map" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server" > 

  <bx:IncludeComponent 
	  id="systempublicsitemap1" 
	  runat="server" 
	  componentname="bitrix:system.PublicSiteMap" 
	  template="bank_sitemap" 
	  Depth="0" 
	  NumberOfColumns="2" 
  /> 

 </asp:Content>

