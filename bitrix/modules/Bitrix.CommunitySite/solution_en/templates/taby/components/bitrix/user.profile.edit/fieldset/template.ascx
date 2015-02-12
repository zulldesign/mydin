<%@ Reference VirtualPath="~/bitrix/components/bitrix/user.profile.edit/component.ascx" %>
<%@ Control Language="C#"  Inherits="Bitrix.Main.Components.UserProfileEditTemplate" %>

<% if (!string.IsNullOrEmpty(Component.FatalError)) { %><span class="errortext"><%= Component.FatalError %></span><br /><% return;} %>


<% if (Component.IsSaved) { %>
	<span class="notetext"><%= Component.SuccessMessage %></span><br />
<% } %>


<% if (Component.HasErrors) { %>
	<div class="errortext">
		<ul>
		<% foreach (string error in Component.ErrorSummary)  { %>
			<li><%= error %></li>
		<% } %>
		</ul>
	</div>		                                                  
<% } %>
<% if (Component.IsPermissionDenied) return; %>
<div class="errortext" id="TemplateErrors" runat="server" visible="false"></div>

<%	
	string liveIdFieldCode = Bitrix.Configuration.BXConfigurationUtility.Options.User.LiveIDCustomFieldCode;
	string openIdFieldCode = Bitrix.Configuration.BXConfigurationUtility.Options.User.OpenIDCustomFieldCode;
	
foreach (var group in Component.FieldGroups)
{
	if (group.IsEmpty)
		continue;
%>

<div class="content-rounded-box" style="margin-bottom: 2em;" onkeypress="return FireDefaultButton(event, '<%= SaveButton.ClientID %>')">
	<b class="r1"></b><b class="r0"></b>
	<div class="inner-box">
		<div class="content-form profile-form">
			<div class="legend"><%= group.Title %></div>
				<div class="fields">
	<%
	foreach (var field in group.Fields)
	{
		string className = String.Concat("field", " field-", field.Id.ToLower());

		if (field.Id.Equals(openIdFieldCode, StringComparison.OrdinalIgnoreCase))
		{
			className = "field field-openid";
			var editor = (Bitrix.Main.Components.UserProfileEditComponent.OpenIdEditor)field.Editor;
			editor.ButtonClassName = "input-submit";
			editor.FieldClassName = "input-openid";
		}
		else if (field.Id.Equals(liveIdFieldCode, StringComparison.OrdinalIgnoreCase))
		{
			className = "field field-liveid";
		}
		else
			if (field.CustomType != null)
			{
				int position = field.CustomType.TypeName.LastIndexOf('.');
				className = String.Concat(
					className,
					" field-",
					position != -1 ? field.CustomType.TypeName.Substring(position + 1).ToLower() : field.CustomType.TypeName.ToLower());
			}

		if (field.ValidateErrors != null && field.ValidateErrors.Count > 0)
			className += " field-error";
		
		%>
		<div class="<%= className %>">	
			<label class="field-title"><% if (field.IsRequired) { %><span style="color:red;">*</span><% } %><%= field.Title %></label>
			<div class="form-input"><% field.Render(this.CurrentWriter); %></div>
			
		</div><%		
	}%>
			</div>
		</div>
	</div>
	<b class="r0"></b><b class="r1"></b>
</div><%
}  	     		
%>

<br />
<div class="content-form profile-form">
	<div class="button"><asp:Button runat="server" ID="SaveButton"  OnClick="SaveUserDefaultHandler" Text="<%$ LocRaw:ButtonText.Save %>" CssClass="input-submit"/></div>
</div>


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
