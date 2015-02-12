<%@ Reference VirtualPath="~/bitrix/components/bitrix/user.profile.view/component.ascx" %>
<%@ Control Language="C#"  AutoEventWireup="false" Inherits="Bitrix.Main.Components.UserProfileViewTemplate" %>
<%@ Import Namespace="Bitrix.Main.Components" %>

<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		if (Component.IsPermissionDenied)
			Bitrix.Security.BXAuthentication.AuthenticationRequired();
	}
</script>

<% if (Component.IsPermissionDenied) return; %>


<% 
	if (!string.IsNullOrEmpty(Component.FatalError)) 
	{ 
%>
	<span class="errortext"><%= Component.FatalError %></span><br />
<% 
	return;
	} 
%>

<% if (Component.CanModify && !string.IsNullOrEmpty(Component.EditProfileUrl)) { %>
<div style="text-align: right">
	<a href="<%= Encode(Component.EditProfileUrl) %>" ><%= !string.IsNullOrEmpty(Component.EditProfileTitle) ? Component.EditProfileTitle : GetMessage("Kernel.Edit") %></a>
</div>
<% } %>

<table width="100%" cellpadding="2" cellspacing="4" border="0" class="user-profile-view">	
<%	
	foreach (UserProfileViewComponent.FieldGroup group in Component.FieldGroups)
	{
		if (!group.CheckNotEmptyFields())
			continue;
%>
<tr>
	<td align="right" style="width:30%" valign="top"></td>
	<td valign="top"><b><%= group.Title %></b></td>
</tr>
<%	
	
	foreach (UserProfileViewComponent.Field field in group.Fields)
	{
		if (field.IsEmpty)
			continue;
%>
	<tr>
		<td align="right" style="width:30%" valign="top">
			<%= field.Title %>:
		</td>
		<td valign="top">
			<% field.Render(this.CurrentWriter); %>
		</td>
	</tr>
<%		
	}
}  	     		
%>
</table>
