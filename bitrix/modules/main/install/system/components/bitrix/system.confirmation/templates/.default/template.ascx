<%@ Reference Control="~/bitrix/components/bitrix/system.confirmation/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Main.Components.ConfirmationComponentTemplate" %>
<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="cc1" %>

<script runat="server">

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        if (Component.FatalError != Bitrix.Main.Components.ConfirmationComponent.ErrorCode.None)
            errorMessage.AddErrorMessage(Component.GetErrorHtml(Component.FatalError));
    }
    
</script>

	<bx:BXValidationSummary ID="errorMessage" runat="server" BorderColor="Red"
		BorderStyle="Solid" BorderWidth="1px" CssClass="errorSummary"
		HeaderText="<%$ Loc:ConfirmationError %>"  />
<% if (Component.FatalError == Bitrix.Main.Components.ConfirmationComponent.ErrorCode.None)
   { %>
    <div class="confirmation-content">
		<div class="confirmation-note-box">
		    <div class="confirmation-note-box-text">
		    <%=  (((Bitrix.Main.Components.ConfirmationComponent)Component).UserIsAuthorized ? GetMessage("Authorized") : GetMessage("ConfirmationSuccessful"))%>
		    </div>
		</div>
    </div>
    <%} %>



