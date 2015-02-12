<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_photo_edit_templates__default_template" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery.photo.edit/component.ascx"  %>
<% if(Component.CanModify){ %>
<div class="content-form">
<div class="fields">
            <bx:BXValidationSummary ID="validationSummary" runat="server" 
                CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"
                BorderColor="Red" BorderStyle="Solid" BorderWidth="1px" />        
    <div class="field">
            <label for="<%=AlbumTitle.ClientID %>" class="field-title"><%= GetMessage("Legend.Title") %><span style="color: Red; visibility: hidden;" id="">*</span></label>
			<div class="form-input">
            <asp:TextBox ID="AlbumTitle" runat="server" ></asp:TextBox>
             <asp:RequiredFieldValidator runat="server" ID="valTitle" ControlToValidate="AlbumTitle" ErrorMessage="<%$ Loc:Error.TitleIsRequired %>">*</asp:RequiredFieldValidator>
            </div>
           
    </div>
    <div class="field">
            <label for="<%=lbSections.ClientID %>" class="field-title"><%= GetMessage("Legeng.Albums")%></label>
            <div class="form-input">
            <asp:ListBox ID="lbSections" Rows="10" SelectionMode="Multiple" runat="server"></asp:ListBox>
            </div>
     </div>
    <div class="field">
            <label for="<%=AlbumDescription.ClientID %>" class="field-title"><%= GetMessage("Legend.Description")%></label>
            <div class="form-input">
            <asp:TextBox ID="AlbumDescription" runat="server" TextMode="MultiLine" Rows="10"></asp:TextBox>
            </div>
    </div>
    <div class="field field-button">
            <%--<asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="<%$ Loc:Kernel.Save %>" /> <%= GetMessage("Or") %> <a href="<%= Component.BackUrl %>" EnableAjax="true"><%= GetMessage("Cancel") %></a>--%>
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="<%$ Loc:Kernel.Save %>" Width="100px" UseSubmitBehavior="false" CausesValidation="true" />
            <asp:Button ID="Button2" runat="server"  Text="<%$ Loc:Cancel %>" UseSubmitBehavior="false"  OnClientClick="GoBack(); return false;" Width="100px" CausesValidation="false" />
	</div>
</div>
</div>
<script type="text/javascript" language="javascript">
    window.GoBack = function()
    {
        window.location.href = "<%= Component.BackUrl %>";
    }
</script>
<%} else { %>
    <%= GetMessage("YouDontHaveRightsToPerformThisAction") %> <a href="<%= Component.BackUrl %>" EnableAjax="true"><%= GetMessage("Return") %></a>
<%} %>