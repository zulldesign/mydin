<%@ Reference Control="~/bitrix/components/bitrix/iblock.element.webform/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.IBlock.Components.IBlockElementWebFormTemplate" AutoEventWireup="True" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix" %>

<%
if (Component.IsPermissionDenied)
{
	return;
}
else if (Component.IsSavingElementSuccess)
{
	if (Component.Element == null)
	{
		%><span class="notetext"><%=Component.SuccessMessageAfterCreateElement%></span><br /><%
		return;
	}
	else
	{
		%><span class="notetext"><%=Component.SuccessMessageAfterUpdateElement%></span><br /><%	
	}
}
else if (!String.IsNullOrEmpty(Component.errorMessage))
{ 
	%><span class="errortext"><%=Component.errorMessage%></span><br /><%
	return;
}	

foreach (string error in Component.SummaryErrors)
{
	%><span class="errortext"><%= error %></span><br /><%
}
if (Component.SummaryErrors.Count > 0)
{
	%><br /><%
}	                                                   	
%>


<%	
foreach (string fieldID in Component.EditFields)
{
	if (!Component.ElementFields.ContainsKey(fieldID))
		continue;

	IBlockElementWebFormComponent.ElementField field = Component.ElementFields[fieldID];
%>

	<% if (field.Required) { %><span style="color:red;">*</span><% } %><b><%= field.Title%></b>
	
	<br /><br />
		
	<%
	if (field.ValidateErrors != null)
	{
		%><table cellpadding="0" cellspacing="0"><tr><td valign="top"><%= field.Render()%></td><td valign="top">&nbsp;<span style="color:red; vertical-align:middle;">*</span></td></tr></table><br /><%
	}
	else
	{
		%><div><%= field.Render()%></div><br /><%
	}	
}  	     		

if (Component.Element == null) 
{
	%><asp:Button runat="server" ID="SaveButton" Text="<%$ Parameters:CreateButtonTitle %>" OnClick="SaveWebForm" /><%
}
else
{
	%><asp:Button runat="server" ID="UpdateButton" Text="<%$ Parameters:UpdateButtonTitle %>" OnClick="SaveWebForm" /><%
}
%>

<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		if (Component.IsPermissionDenied)
		{
			Bitrix.Security.BXAuthentication.AuthenticationRequired();
		}
	}

	public override string Title
	{
		get
		{
			return GetMessageRaw("Title");
		}
	}
</script>
