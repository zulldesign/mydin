<%@ Reference Control="~/bitrix/components/bitrix/iblock.element.webform/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.IBlock.Components.IBlockElementWebFormTemplate" AutoEventWireup="True" %>

<div class="content-form <%= Parameters.GetString("Template_UserCssClass") %>">
	<div class="fields">
	<%
	if (Component.IsSavingElementSuccess)
	{
		if (Component.Element == null)
		{
			%>
			<span class="notetext"><%= Component.SuccessMessageAfterCreateElement %></span>
	</div>
</div>
			<%
			return;
		}
		%>
		<span class="notetext"><%= Component.SuccessMessageAfterUpdateElement %></span>
		<%	
	}
	else if (!String.IsNullOrEmpty(Component.errorMessage))
	{ 
		%>
		<span class="errortext"><%= Component.errorMessage %></span>
	</div>
</div>
		<%
		return;
	}

	if (Component.SummaryErrors.Count > 0) 
	{
		%>
		<div class="errortext">
			<ul>
			<% foreach (string error in Component.SummaryErrors) { %>
				<li><%= error %></li>
			<% } %>
			</ul>
		</div>
		<%
	}
	
	bool showAsterisk = Parameters.GetBool("Template_ShowAsterisk");
				
	foreach (string fieldID in Component.EditFields)
	{
		if (!Component.ElementFields.ContainsKey(fieldID))
			continue;

		var field = Component.ElementFields[fieldID];

		%>
		<div class="field field-<%= fieldID.ToLower() %><%= (field.ValidateErrors != null && field.ValidateErrors.Count > 0) ? " field-error" : "" %>">
			<label class="field-title"><%= field.Title %><% if (showAsterisk && field.Required){ %><span class="field-required">*</span><% } %></label>
			<div class="form-input"><%= field.Render() %></div>	
		</div>
		<%
	}  	     		
	%>
	
		<div class="field field-button">
		<%
		if (Component.Element == null) 
		{
			%><asp:Button runat="server" ID="Create" Text="<%$ Parameters:CreateButtonTitle %>" OnClick="SaveWebForm" CssClass="input-submit" /><%
		}
		else
		{
			%><asp:Button runat="server" ID="Update" Text="<%$ Parameters:UpdateButtonTitle %>" OnClick="SaveWebForm" CssClass="input-submit" /><%
		}
		%>
		</div>
	</div>
</div>

<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		if (Component.IsPermissionDenied)
		    Bitrix.Security.BXAuthentication.AuthenticationRequired();
	}
	protected override void PreLoadTemplateDefinition(Bitrix.Components.BXParametersDefinition def)
	{
		def["Template_UserCssClass"] = new Bitrix.Components.BXParamText(GetMessageRaw("Param.UserCssClass"), "", Bitrix.Components.BXCategory.AdditionalSettings);
		def["Template_ShowAsterisk"] = new Bitrix.Components.BXParamYesNo(GetMessageRaw("Param.ShowAsterisk"), true, Bitrix.Components.BXCategory.AdditionalSettings);
	}
</script>