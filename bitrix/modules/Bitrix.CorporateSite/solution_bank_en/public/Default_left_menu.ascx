<%@ Control Language="C#" ClassName="Default_left_menu" %>
<bx:IncludeComponent 
	ID="DefaultPageLeftMenu" 
	runat="server" 
	componentname="bitrix:system.PublicMenu"
	template="vertical" 
	Depth="2" 
	MenuName="left" 
	SubMenuName="left" 
	Url="<%$ Options: Bitrix.BankSite:DefaultPageMenuPath %>"
	Template_ShowAllLevels="True"
/>