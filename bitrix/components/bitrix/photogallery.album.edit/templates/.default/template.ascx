<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_album_edit_templates__default_template" EnableViewState="false" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery.album.edit/component.ascx" %>
<% if(Component.CanModify){ %>
<div class="content-form">
<div class="fields"

            <bx:BXValidationSummary ID="validationSummary" runat="server" 
                CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"
                BorderColor="Red" BorderStyle="Solid" BorderWidth="1px" />

    <div class="field">
	<label for="<%=AlbumTitle.ClientID %>" class="field-title"><%= GetMessage("Legend.Title") %><span style="color: Red; visibility: hidden;" id="">*</span></label>
	<div class="form-input">
            <asp:TextBox ID="AlbumTitle" runat="server" CssClass="input-field" ></asp:TextBox>
            <asp:RequiredFieldValidator runat="server" ID="valTitle" ControlToValidate="AlbumTitle" ErrorMessage="<%$ Loc:Error.TitleIsRequired %>">*</asp:RequiredFieldValidator>
            </div>
	</div>
	<div class="field">
		<label for="<%=ddlParentSection.ClientID %>" class="field-title"><%= GetMessage("Legend.ParentSection")%></label>
		<div class="form-input">
            <asp:DropDownList ID="ddlParentSection" runat="server">
            </asp:DropDownList>
		</div>
    </div>
	<div class="field">
			<label for="<%=AlbumDescription.ClientID %>" class="field-title"><%= GetMessage("Legend.Description")%></label>
			<div class="form-input">
            <asp:TextBox ID="AlbumDescription" runat="server" TextMode="MultiLine" Rows="10" Width="100%" CssClass="input-field"></asp:TextBox>
            </div>
	</div>
	<div class="field field-button">
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="<%$ Loc:Kernel.Save %>" />
            <asp:Button  runat="server" ID="Cancel" Text="<%$ Loc:Cancel %>" OnClientClick="GoBack(); return false;" UseSubmitBehavior="false" /> 
	</div>
</div>
</div>
<%} else { %>
    <%= GetMessage("YouDontHaveRightsToPerformThisAction") %> <a href="<%= Component.BackUrl %>" EnableAjax="true"><%= GetMessage("Return") %></a>
<%} %>

<script type="text/javascript" language="javascript">
    window.GoBack = function()
    {
        window.location.href = "<%= Component.BackUrl %>";
    }
</script>

