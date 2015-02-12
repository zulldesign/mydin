<%@ Control Language="C#" ClassName="form" Inherits="Bitrix.UI.BXComponentTemplate" %>

<bx:IncludeComponent runat="server" ID="webform"
	ComponentName="bitrix:iblock.element.webform"
	Template=".default"
	ElementID = "<%$ Results:ElementID %>"
/>

<% 
if (!String.IsNullOrEmpty(Parameters.GetString("RedirectPageUrl")))
{
	%><br /><br /><a href="<%=Parameters.GetString("RedirectPageUrl") %>"><%=GetMessage("BackToTheList")%></a><%	
}
%>
