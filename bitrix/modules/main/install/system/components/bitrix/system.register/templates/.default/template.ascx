<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_system_register_templates__default_template" %>
<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="cc1" %>

<%@ Reference Control="~/bitrix/components/bitrix/system.register/component.ascx" %>

<asp:panel ID="CreateUserStep1" defaultbutton="Button1" runat="server">
	<bx:BXValidationSummary ID="errorMessage" runat="server" BorderColor="Red"
		BorderStyle="Solid" BorderWidth="1px" CssClass="errorSummary"
		HeaderText="<%$ Loc:RegistrationError %>" ValidationGroup="CreateUserWizard1" />

	<div class="auth-form">
		<div class="header"><%= GetMessage("RegistrationOnSite") %></div>
		<div>
		<table cellpadding="0" cellspacing="0" border="0">

		
		<%
		          if (Component.SendConfirmationRequest && !String.IsNullOrEmpty(Component.UserActivationToken) && PageIsValid)
      {  %>
      <tr><td>
      <%=String.Format(GetMessage("ConfirmationRequestWasSent"),((Bitrix.Main.Components.SystemRegisterComponent)Component).UserEmail) %>
      </td></tr>
			<% }
      else
      {  %>
      
      <tr id="trLinkUserMessage" visible="false" runat="server"><td colspan="2"><%= GetMessage("LinkExistingUser") %></td></tr>
		<tr id="trOpenIdLogin" runat="server" visible="false">
			<td align="right" valign="top"><asp:Label ID="lbOpenIdLogin" runat="server" AssociatedControlID="tbOpenIdLogin"><%= GetMessage("Legend.Login") %></asp:Label></td>
			<td>
				<asp:TextBox ID="tbOpenIdLogin" runat="server"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfOpenIdLogin" runat="server" 
						ControlToValidate="tbOpenIdLogin" 
						ErrorMessage="<%$ Loc:Message.DisplayNameIsRequired %>" 
						ToolTip="<%$ Loc:Message.DisplayNameIsRequired %>" 
						ValidationGroup="grOpenId">*</asp:RequiredFieldValidator>
			</td>
			
		</tr>
		
		<tr id="trOpenIdPassword" runat="server" visible="false" >
			<td align="right" valign="top"><asp:Label ID="Label1" runat="server" AssociatedControlID="tbOpenIdPassword"><%= GetMessage("Legend.Password") %></asp:Label></td>
			<td>
				<asp:TextBox ID="tbOpenIdPassword" runat="server" TextMode="Password"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfOpenIdPassword" runat="server" 
						ControlToValidate="tbOpenIdPassword" 
						ErrorMessage="<%$ Loc:Message.DisplayNameIsRequired %>" 
						ToolTip="<%$ Loc:Message.DisplayNameIsRequired %>" 
						ValidationGroup="grOpenId">*</asp:RequiredFieldValidator>
			</td>
		</tr>
		
		<tr id="trOpenIdLink" runat="server" visible="false">
			<td></td>
			<td>
				<asp:Button ID="bExternalIdLink" runat="server" CausesValidation="true" ValidationGroup="grOpenId" Text = "<%$ Loc:ButtonText.Link %>" OnClick = "ExternalIdLinkClick"></asp:Button>
			</td>
		</tr>
		<tr id="trNewUserMessage" visible="false" runat="server"><td colspan="2"><%= GetMessage("RegisterNewUser") %></td></tr>
		
			<% if (Component.DisplayNameFieldMode != RegisterComponent.FieldMode.Hide)
      { %>
      
      		
			<tr>

				<td align="right" valign="top">
				
					<% if (Component.DisplayNameFieldMode == RegisterComponent.FieldMode.Require)
        { %><span style="color: red;">*</span><% } %><asp:Label ID="lbDisplayName" runat="server" AssociatedControlID="tbDisplayName"><%= GetMessage("Legend.DisplayName") %></asp:Label></td>
				<td valign="top">
					<asp:TextBox ID="tbDisplayName" runat="server"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvDisplayName" runat="server" 
						ControlToValidate="tbDisplayName" 
						ErrorMessage="<%$ Loc:Message.DisplayNameIsRequired %>" 
						ToolTip="<%$ Loc:Message.DisplayNameIsRequired %>" 
						ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
				</td>
			</tr>
			<% } %>
			<% if (Component.FirstNameFieldMode != RegisterComponent.FieldMode.Hide)
      { %>
			<tr>
				<td align="right" valign="top">
					<% if (Component.FirstNameFieldMode == RegisterComponent.FieldMode.Require)
        { %><span style="color: red;">*</span><% } %><asp:Label ID="lbFirstName" runat="server" AssociatedControlID="tbFirstName"><%= GetMessage("Legend.Name") %></asp:Label></td>
				<td valign="top">
					<asp:TextBox ID="tbFirstName" runat="server"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvFirstName" runat="server" 
						ControlToValidate="tbFirstName" 
						ErrorMessage="<%$ Loc:Message.NameIsRequired %>" 
						ToolTip="<%$ Loc:Message.NameIsRequired %>" 
						ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
				</td>
			</tr>
			<% } %>
			<% if (Component.LastNameFieldMode != RegisterComponent.FieldMode.Hide)
      { %>
			<tr>
				<td align="right" valign="top">
					<% if (Component.LastNameFieldMode == RegisterComponent.FieldMode.Require)
        { %><span style="color: red;">*</span><% } %><asp:Label ID="lbLastName" runat="server" AssociatedControlID="tbLastName"><%= GetMessage("Legend.SecondName") %></asp:Label></td>
				<td valign="top">
					<asp:TextBox ID="tbLastName" runat="server"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvLastName" runat="server" 
						ControlToValidate="tbLastName" 
						ErrorMessage="<%$ Loc:Message.SecondNameIsRequired %>" 
						ToolTip="<%$ Loc:Message.SecondNameIsRequired %>" 
						ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
				</td>
			</tr>
			<% } %>
			<tr>
				<td align="right" valign="top">
					<span style="color: red;">*</span><asp:Label ID="lbLogin" runat="server" AssociatedControlID="tbLogin"><%= GetMessage("Legend.Login") %></asp:Label></td>
				<td valign="top">
					<asp:TextBox ID="tbLogin" runat="server" ValidationGroup="CreateUserWizard1"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvLoginField" runat="server" 
						ControlToValidate="tbLogin" 
						ErrorMessage="<%$ Loc:Message.LoginIsRequired %>" 
						ToolTip="<%$ Loc:Message.LoginIsRequired %>" 
						ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr id = "trPassword" runat="server">
				<td align="right" valign="top">
					<span style="color: red;">*</span><asp:Label ID="lbPassword" runat="server" AssociatedControlID="tbPassword"><%= GetMessage("Legend.Password") %></asp:Label></td>
				<td valign="top">
					<asp:TextBox ID="tbPassword" runat="server" TextMode="Password" ValidationGroup="CreateUserWizard1"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="tbPassword"
						ErrorMessage="<%$ Loc:Message.PasswordIsRequired %>" ToolTip="<%$ Loc:Message.PasswordIsRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr id = "trPasswordConf" runat="server">
				<td align="right" valign="top">
					<span style="color: red;">*</span><asp:Label ID="lbPasswordConf" runat="server" AssociatedControlID="tbPasswordConf"><%= GetMessage("Legend.PasswordConfirmation") %></asp:Label></td>
				<td valign="top">
					<asp:TextBox ID="tbPasswordConf" runat="server" TextMode="Password" ValidationGroup="CreateUserWizard1"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvPasswordConf" Display="Dynamic" runat="server" ControlToValidate="tbPasswordConf"
						ErrorMessage="<%$ Loc:Message.PasswordConfirmationIsRequired %>" ToolTip="<%$ Loc:Message.PasswordConfirmationIsRequired %>"
						ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
					<asp:CompareValidator ID="cvPassword" runat="server" ControlToCompare="tbPassword"
						ControlToValidate="tbPasswordConf" Display="Dynamic" ErrorMessage="<%$ Loc:Message.PasswordAndItsConfirmationDontMatch %>"
						ValidationGroup="CreateUserWizard1">*</asp:CompareValidator>
				</td>
			</tr>
			<tr>
				<td align="right" valign="top">
					<span style="color: red;">*</span><asp:Label ID="lbEmail" runat="server" AssociatedControlID="tbEmail">E-mail:</asp:Label></td>
				<td valign="top">
					<asp:TextBox ID="tbEmail" runat="server" ValidationGroup="CreateUserWizard1"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvEmail" Display="Dynamic" runat="server" ControlToValidate="tbEmail"
						ErrorMessage="<%$ Loc:MessageText.EmailIsRequired %>" ToolTip="<%$ Loc:MessageToolTip.EmailIsRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
					<asp:RegularExpressionValidator ID="revEmail" Display="Dynamic" runat="server" ErrorMessage="<%$ Loc:MessageText.EmailIsInvalid %>" ControlToValidate="tbEmail"
						ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ValidationGroup="CreateUserWizard1">*</asp:RegularExpressionValidator>
				</td>
			</tr>
			<tr id="trQuestion" runat="server">
				<td align="right" valign="top" runat="server">
					<span style="color: red;">*</span><asp:Label ID="lbQuestion" runat="server" AssociatedControlID="tbQuestion"><%= GetMessage("Legend.SecretQuestion") %></asp:Label></td>
				<td runat="server" valign="top">
					<asp:TextBox ID="tbQuestion" runat="server" ValidationGroup="CreateUserWizard1"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvQuestion" runat="server" ControlToValidate="tbQuestion"
						ErrorMessage="<%$ Loc:MessageText.SecretQuestionMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.SecretQuestionMustBeSpecified %>"
						ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr id="trAnswer" runat="server">
				<td align="right" valign="top" runat="server">
					<span style="color: red;">*</span><asp:Label ID="lbAnswer" runat="server" AssociatedControlID="tbAnswer"><%= GetMessage("Legend.AnswerForSecretQuestion") %></asp:Label></td>
				<td runat="server" valign="top">
					<asp:TextBox ID="tbAnswer" runat="server" ValidationGroup="CreateUserWizard1"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvAnswer" runat="server" ControlToValidate="tbAnswer"
						ErrorMessage="<%$ Loc:MessageText.AnswerForSecretQuestionMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.AnswerForSecretQuestionMustBeSpecified %>"
						ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
				</td>
			</tr>
			<% foreach (var p in Component.EditFields)
		 {
			 if (Component.UserCustomFields.ContainsKey(p))
			 {
				 var field = Component.UserCustomFields[p];
				 %>
				<tr><td align="right" valign="top"><% if (field.Field.Mandatory){ %><span style="color: red;">*</span><% } %><label><%= field.Field.EditFormLabel %></label></td><td><%= field.Render() %></td></tr>
			<%}
		 } %>
		
			<% if (Component.UseCaptcha && !HaveExternalUserInfo)
	  { %>
			<tr>
				<td align="right" valign="top">
					<span style="color: red;">*</span><asp:Label ID="lblCaptcha" runat="server" AssociatedControlID="tbxCaptcha" ><%= GetMessage("CaptchaPrompt.EnterTheCodeDisplayedOnPicture")%></asp:Label>				    
			    </td>
			    <td valign="top">
			        <asp:HiddenField ID="hfCaptchaGuid" runat="server" />
			        <asp:TextBox ID="tbxCaptcha" runat="server" ></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvCaptcha" runat="server" ControlToValidate="tbxCaptcha"
						ErrorMessage="<%$ Loc:MessageText.CaptchaCodeMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.CaptchaCodeMustBeSpecified %>"
						ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>				        
			        <br />
			        <asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" />			        
			    </td>
			</tr>
			<% } %>
			<tr>
				<td></td>
				<td>
					<asp:Button ID="Button1" CausesValidation="true" ValidationGroup="CreateUserWizard1" runat="server" CommandName="MoveComplete" Text="<%$ Loc:ButtonText.Register %>" OnClick="Button1_Click"/>				
				</td>
			</tr>
			<%} %>
		</table>
		</div>
	</div>
</asp:panel>
<asp:PlaceHolder ID="CaptchaValidatorPlaceHolder" runat="server"></asp:PlaceHolder>
<%--<asp:panel ID="CreateUserStep2" runat="server">
	<div class="auth-form">
		<div class="header"><%= GetMessage("YouHaveBeenSuccessfullyRegistredOnTheSite") %></div>
		<div class="registrationIsCompleted">
		    <div class="content">
		        <table>
		            <tr>
		                <td><%= GetMessage("Legend.Name") %></td>
		                <td><%= HttpUtility.HtmlEncode(_registredFirstName)%></td>
		            </tr>
		            <tr>
		                <td><%= GetMessage("Legend.SecondName") %></td>
		                <td><%= HttpUtility.HtmlEncode(_registredLastName)%></td>
		            </tr>
		            <tr>
		                <td><%= GetMessage("Legend.Login") %></td>
		                <td><%= HttpUtility.HtmlEncode(_registredLogin)%></td>
		            </tr>
		            <tr>
		                <td>E-mail:</td>
		                <td><%= HttpUtility.HtmlEncode(_registredEmail)%></td>
		            </tr>
		        </table>
		    </div>
		</div>
	</div>
</asp:panel>--%>

<script type="text/javascript">
	function <%=ClientID %>FireDefaultButton(event,target)
	{
	if (event.keyCode == 13) 
		{
        var src = event.srcElement || event.target;
        if (!src || (src.tagName.toLowerCase() != "textarea")) 
        {
            var defaultButton = document.getElementById(target);
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
