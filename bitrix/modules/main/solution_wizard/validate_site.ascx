<%@ Control Language="C#" AutoEventWireup="true" CodeFile="validate_site.ascx.cs" Inherits="Bitrix.Wizards.Solutions.ValidateSiteWizardStep" %>
<%= 
	string.Format(
		GetMessage("Message"),
		Encode(site.Name),
		Encode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/FileMan.aspx") + "?path=" + HttpUtility.UrlEncode(site.DirectoryVirtualPath))
	)
%>