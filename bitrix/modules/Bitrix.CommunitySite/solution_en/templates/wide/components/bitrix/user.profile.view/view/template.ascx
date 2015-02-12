<%@ Reference VirtualPath="~/bitrix/components/bitrix/user.profile.view/component.ascx" %>
<%@ Control Language="C#"  AutoEventWireup="false" Inherits="Bitrix.Main.Components.UserProfileViewTemplate" %>
<%@ Import Namespace="Bitrix.Main.Components" %>

<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
	
		if (Component.User != null)
			Page.Title = Component.User.GetDisplayName();
		
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

<div class="content-rounded-box">
	<b class="r1"></b><b class="r0"></b>
	<div class="inner-box">
	
		<div class="content-form">
			<div class="legend"><%= GetMessageRaw("Title") %></div><br /><br />

			<table cellspacing="0" class="content-table">

				<% if (Component.User != null) {%>
				<tr>
					<td class="label">&nbsp;</td>
					<td class="value header">
						<h2><%= HttpUtility.HtmlEncode(Component.User.GetDisplayName()) %></h2>
						<% if (Component.CanModify && !string.IsNullOrEmpty(Component.EditProfileUrl)) { %>
							<a href="<%= Encode(Component.EditProfileUrl) %>"><%= GetMessageRaw("EditProfile") %></a>
						<% } %>
					</td>
				</tr>
				<% } %>
			<%	
			foreach (UserProfileViewComponent.FieldGroup group in Component.FieldGroups)
			{
				if (!group.CheckNotEmptyFields())
					continue;
				
				foreach (UserProfileViewComponent.Field field in group.Fields)
				{
					if (field.IsEmpty)
						continue;

					if (field.Id == "URL" && field.CustomProperty != null && field.CustomProperty.Value != null && field.CustomProperty.Value.ToString() == "http://")
						continue;
			%>
				<tr>
					<td class="label"><%= field.Title %>:</td>
					<td class="value"><% field.Render(this.CurrentWriter); %></td>
				</tr>
			<%		
				}
			}  	     		
			%>
			<% if (Component.User != null) { %>
				<tr>
					<td class="label"><%= GetMessageRaw("Registered") %>:</td>
					<td class="value"><%= string.Format(GetMessageRaw("RegisteredDateAndTime"), Component.User.CreationDate.ToString("D"), Component.User.CreationDate.ToString("t")) %></td>
				</tr>
			<% } %>
			</table>

		</div>
	</div>
	<b class="r0"></b><b class="r1"></b>
</div>