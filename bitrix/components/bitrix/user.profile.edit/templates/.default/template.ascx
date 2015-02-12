<%@ Reference VirtualPath="~/bitrix/components/bitrix/user.profile.edit/component.ascx" %>
<%@ Control Language="C#"  AutoEventWireup="false" Inherits="Bitrix.Main.Components.UserProfileEditTemplate" %>
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


<% 
	if (Component.IsSaved) { %>
	<span class="notetext"><%= Component.SuccessMessage %></span><br />
<% } %>


<% if (Component.HasErrors) { %>
	<% foreach (string error in Component.ErrorSummary)  { %>
		<span class="errortext"><%= error %></span><br />
	<% } %>
	<br />		                                                  
<% } %>


<table width="100%" cellpadding="2" cellspacing="4" border="0" class="user-profile-edit">	
<%	
foreach (UserProfileEditComponent.FieldGroup group in Component.FieldGroups)
{
	if (group.IsEmpty)
		continue;
%>
<tr>
	<tr>
		<td align="right" style="width:30%" valign="top">
		</td>
		<td valign="top"><b><%= group.Title %></b></td>
	</tr>
</tr>
<%	
	
	foreach (UserProfileEditComponent.Field field in group.Fields)
	{
%>
	<tr>
		<td align="right" style="width:30%" valign="top">
			<% if (field.IsRequired) { %><span style="color:red;">*</span><% } %>
			<%= field.Title %>:
		</td>
		<td valign="top">
		<% 
		if (field.ValidateErrors != null) 
		{ 
		%>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<td valign="top">
						<div class="iuser-profile-edit-error"><% field.Render(this.CurrentWriter); %></div>
					</td>
					<td valign="top">&nbsp;<span style="color:red; vertical-align:middle;">*</span></td>
				</tr>
			</table>
		<% 
		} 
		else 
		{ 
			field.Render(this.CurrentWriter);
		} 
		%>
		</td>
	</tr>
<%		
	}
}  	     		
%>
	
</table>
<asp:Button runat="server" ID="SaveButton"  OnClick="SaveUserDefaultHandler" Text="<%$ LocRaw:Kernel.Save %>" />
<script type="text/javascript">
	function <%= Component.ClientID %>OpenIdFireDefaultButton(event)
	{
	if (event.keyCode == 13) 
		{
        var src = event.srcElement || event.target;
        if (!src || (src.tagName.toLowerCase() != "textarea")) 
        {
            var defaultButton = src.nextSibling.nextSibling;
            if (defaultButton && typeof(defaultButton.click) != "undefined") 
            {
                defaultButton.click();
                event.cancelBubble = true;
                
                if (event.stopPropagation) 
                    event.stopPropagation();
                    
                return false;
            }
        }
    }
    return true;
	}
	
</script>
