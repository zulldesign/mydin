<%@ Control Language="C#" %>
<bx:IncludeComponent 
	id="SideBar" 
	runat="server" 
	componentname="bitrix:includeArea"  
	template=".default" 
	Mode="File" 
	FilePath="<%$ Options:Bitrix.CommunitySite:SiteFolder:{0}sidebar.ascx %>" 
	AllowEditing="True" 
/>
