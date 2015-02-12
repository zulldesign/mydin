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
		%><span class="notetext"><%=Component.SuccessMessageAfterCreateElement%></span><%
		return;
	}
	else
	{
		%><span class="notetext"><%=Component.SuccessMessageAfterUpdateElement%></span><%	
	}
}
else if (!String.IsNullOrEmpty(Component.errorMessage))
{ 
	%><span class="errortext"><%=Component.errorMessage%></span><%
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

<table width="100%" cellpadding="2" cellspacing="4" border="0" class="iblock-element-webform">	
<%	
foreach (string fieldID in Component.EditFields)
{
	if (!Component.ElementFields.ContainsKey(fieldID))
		continue;

	IBlockElementWebFormComponent.ElementField field = Component.ElementFields[fieldID];
%>
	<tr>
		<td align="right" width="30%" valign="top">
		<% if (field.Required)
		{
			%><span style="color:red;">*</span><%
		}
		%>
		<%= field.Title%>:
		</td>
					
		<td valign="top">
		<%
		if (field.ValidateErrors != null)
		{
			%><table cellpadding="0" cellspacing="0"><tr><td valign="top"><div class="iblock-element-webform-error"><%= field.Render()%></div></td><td valign="top">&nbsp;<span style="color:red; vertical-align:middle;">*</span></td></tr></table><%
		}
		else
		{
			%><%= field.Render()%><%
		}	
		%>
		</td>
	</tr>
<%		
}  	     		
%>
	
</table>
<br />

<%
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
</script>