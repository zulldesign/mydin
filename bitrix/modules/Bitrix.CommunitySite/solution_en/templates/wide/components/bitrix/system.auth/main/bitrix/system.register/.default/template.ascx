<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_system_register_templates__default_template" %>
<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="cc1" %>
<%@ Reference Control="~/bitrix/components/bitrix/system.register/component.ascx" %>

<asp:panel ID="CreateUserStep1" runat="server">
<%	if
			(Component.SendConfirmationRequest && !String.IsNullOrEmpty(Component.UserActivationToken) && PageIsValid)
	{  %>
		<div class="auth-box">
	<div class="content-rounded-box" runat="server" id="Div1">
		<b class="r1"></b><b class="r0"></b>
		<div class="inner-box">	
      <%=String.Format(GetMessage("ConfirmationRequestWasSent"), ((Bitrix.Main.Components.SystemRegisterComponent)Component).UserEmail)%>
      </div>
		<b class="r0"></b><b class="r1"></b>
	</div>
	</div>
			<% }
	else
	{  %>
<div class="auth-box">
	<div class="content-rounded-box" visible="false" runat="server" id="extFields">
		<b class="r1"></b><b class="r0"></b>
		<div class="inner-box">	
			<div class="content-form register-form" onkeypress="return FireDefaultButton(event, '<%= bExternalIdLink.ClientID %>')">
				<div id="extLegen" runat="server" class="legend"><%= GetMessageRaw("Title") %></div>

				<div class="fields">
					<div style="color: rgb(102, 102, 102); position: relative; left: -1.5em;" class="field">
					<%= GetMessageRaw("IfAlreadyRegistered") %>
					</div>
					<div class="field">
						<asp:Label ID="Label1" runat="server" AssociatedControlID="tbOpenIdLogin" CssClass="field-title"><%= GetMessageRaw("Login") %><span class="starrequired">*</span></asp:Label>
						<div class="form-input">
						<asp:TextBox ID="tbOpenIdLogin" runat="server"></asp:TextBox>
							<asp:RequiredFieldValidator ID="rfOpenIdLogin" runat="server" 
							ControlToValidate="tbOpenIdLogin" 
							ErrorMessage="<%$ Loc:Message.DisplayNameIsRequired %>" 
							ToolTip="<%$ Loc:Message.DisplayNameIsRequired %>" 
							ValidationGroup="grOpenId">*</asp:RequiredFieldValidator>
						</div>
					</div>
					<div class="field">
						<asp:Label ID="Label2" runat="server" AssociatedControlID="tbOpenIdPassword" CssClass="field-title"><%= GetMessageRaw("Password") %><span class="starrequired">*</span></asp:Label>
						<div class="form-input">
							<asp:TextBox ID="tbOpenIdPassword" runat="server" TextMode="Password"></asp:TextBox>
							<asp:RequiredFieldValidator ID="rfOpenIdPassword" runat="server" 
								ControlToValidate="tbOpenIdPassword" 
								ErrorMessage="<%$ Loc:Message.DisplayNameIsRequired %>" 
								ToolTip="<%$ Loc:Message.DisplayNameIsRequired %>" 
								ValidationGroup="grOpenId">*</asp:RequiredFieldValidator>
						</div>
					</div>
					<div class="button">		        	
						<asp:Button ID="bExternalIdLink" runat="server" CssClass="input-submit" CausesValidation="true" ValidationGroup="grOpenId" Text = "<%$ LocRaw:Attach %>"  OnClick = "ExternalIdLinkClick"></asp:Button>
					</div>
				</div>

			</div>
		</div>
		<b class="r0"></b><b class="r1"></b>
	</div>
	
						<bx:BXValidationSummary runat="server" ID="errorMessage" ValidationGroup="CreateUserWizard1" CssClass="errortext" ForeColor="" />
	
	<div class="content-rounded-box">
		<b class="r1"></b><b class="r0"></b>
		<div class="inner-box">	
			<div class="content-form register-form" onkeypress="return FireDefaultButton(event, '<%= RegisterButton.ClientID %>')">
				<div class="legend"><%= GetMessageRaw("RegistrationFormTitle") %></div>

				<div class="fields">
	

				
					<div class="field">
						<asp:Label ID="lbLogin" runat="server" AssociatedControlID="tbLogin" CssClass="field-title"><%= GetMessageRaw("Login") %><span class="starrequired">*</span></asp:Label>
						<div class="form-input"><asp:TextBox ID="tbLogin" runat="server" ValidationGroup="CreateUserWizard1" CssClass="input-field"></asp:TextBox></div>
						<asp:RequiredFieldValidator ID="rfvLoginField"  Display="None" runat="server" ControlToValidate="tbLogin" ErrorMessage="<%$ LocRaw:Message.LoginRequired %>" ToolTip="<%$ LocRaw:Message.LoginRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
					</div>

					<% if (Component.DisplayNameFieldMode != RegisterComponent.FieldMode.Hide)
		{ %>
					<div class="field">
						<% if (Component.DisplayNameFieldMode == RegisterComponent.FieldMode.Require)
		 { %><span class="starrequired">*</span><% } %>
						<asp:Label ID="lbDisplayName" runat="server" AssociatedControlID="tbDisplayName" CssClass="field-title"><%= GetMessageRaw("DisplayName") %></asp:Label></td>
						<div class="form-input"><asp:TextBox ID="tbDisplayName" AutoCompleteType="DisplayName" runat="server" CssClass="input-field"></asp:TextBox></div>
						<div class="description"><%= GetMessageRaw("DisplayNameTooltip") %></div>
						<asp:PlaceHolder ID="captchaPlaceholder" runat="server"></asp:PlaceHolder>	
						<asp:RequiredFieldValidator ID="rfvDisplayName" Display="None" runat="server" ControlToValidate="tbDisplayName" ErrorMessage="<%$ LocRaw:Message.DisplayNameRequired %>" ToolTip="<%$ LocRaw:Message.DisplayNameRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
					</div>
					<% } %>
					
					<div class="field">		
						<asp:Label ID="lbEmail" runat="server" AssociatedControlID="tbEmail" CssClass="field-title"><%= GetMessageRaw("Email") %><span class="starrequired">*</span></asp:Label>
						<div class="form-input"><asp:TextBox ID="tbEmail" runat="server" AutoCompleteType="Email" ValidationGroup="CreateUserWizard1" CssClass="input-field"></asp:TextBox></div>
						<asp:RequiredFieldValidator ID="rfvEmail" Display="None" runat="server" ControlToValidate="tbEmail" ErrorMessage="<%$ LocRaw:Message.EmailRequired %>" ToolTip="<%$ LocRaw:Message.EmailRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
						<asp:RegularExpressionValidator ID="revEmail" Display="None" runat="server" ErrorMessage="<%$ LocRaw:Message.IncorrectEmail %>" ControlToValidate="tbEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ValidationGroup="CreateUserWizard1">*</asp:RegularExpressionValidator>
					</div>
					
					<div class="field" runat="server" id="dPassword">	
						<asp:Label ID="lbPassword" runat="server" AssociatedControlID="tbPassword" CssClass="field-title"><%= GetMessageRaw("Password") %><span class="starrequired">*</span></asp:Label>
						<div class="form-input"><asp:TextBox ID="tbPassword" runat="server"  AutoCompleteType="Disabled" TextMode="Password" ValidationGroup="CreateUserWizard1" CssClass="input-field"></asp:TextBox></div>
						<asp:RequiredFieldValidator ID="rfvPassword" runat="server" Display="None" ControlToValidate="tbPassword" ErrorMessage="<%$ LocRaw:Message.PasswordRequired %>" ToolTip="<%$ LocRaw:Message.PasswordRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
					</div>
					
					<div class="field" runat="server" id="dPasswordConf">			
						<asp:Label ID="lbPasswordConf" runat="server" AssociatedControlID="tbPasswordConf" CssClass="field-title"><%= GetMessageRaw("PasswordConfirmation") %><span class="starrequired">*</span></asp:Label>
						<div class="form-input"><asp:TextBox ID="tbPasswordConf" AutoCompleteType="Disabled" runat="server" TextMode="Password" ValidationGroup="CreateUserWizard1" CssClass="input-field"></asp:TextBox></div>
						<asp:RequiredFieldValidator ID="rfvPasswordConf" Display="None" runat="server" ControlToValidate="tbPasswordConf" ErrorMessage="<%$ LocRaw:Message.PasswordConfirmationRequired %>" ToolTip="<%$ LocRaw:Message.PasswordConfirmationRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
						<asp:CompareValidator ID="cvPassword" runat="server" Display="None" ControlToCompare="tbPassword" ControlToValidate="tbPasswordConf" ErrorMessage="<%$ LocRaw:Message.PasswordsDontMatch %>" ValidationGroup="CreateUserWizard1">*</asp:CompareValidator>
					</div>			
						
					<% if (Component.FirstNameFieldMode != RegisterComponent.FieldMode.Hide)
		{ %>
					<div class="field">		

						<asp:Label ID="lbFirstName" runat="server" AssociatedControlID="tbFirstName" CssClass="field-title"><%= GetMessageRaw("FirstName") %><% if (Component.FirstNameFieldMode == RegisterComponent.FieldMode.Require) { %><span class="starrequired">*</span><% } %></asp:Label></td>
						<div class="form-input"><asp:TextBox ID="tbFirstName" AutoCompleteType="FirstName" runat="server" CssClass="input-field"></asp:TextBox></div>
						<asp:RequiredFieldValidator ID="rfvFirstName" runat="server" Display="None" ControlToValidate="tbFirstName" ErrorMessage="<%$ LocRaw:Message.FirstNameRequired %>" ToolTip="<%$ LocRaw:Message.FirstNameRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
					</div>
					<% } %>
					
					<% if (Component.LastNameFieldMode != RegisterComponent.FieldMode.Hide)
		{ %>
					<div class="field">

						<asp:Label ID="lbLastName" runat="server" AssociatedControlID="tbLastName" CssClass="field-title"><%= GetMessageRaw("LastName") %><% if (Component.LastNameFieldMode == RegisterComponent.FieldMode.Require) { %><span class="starrequired">*</span><% } %></asp:Label></td>
						<div class="form-input"><asp:TextBox ID="tbLastName" AutoCompleteType="LastName" runat="server" CssClass="input-field"></asp:TextBox></div>
						<asp:RequiredFieldValidator ID="rfvLastName" runat="server" Display="None" ControlToValidate="tbLastName" ErrorMessage="<%$ LocRaw:Message.LastNameRequired %>" ToolTip="<%$ LocRaw:Message.LastNameRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
					</div>
					<% } %>
					<% if (Component.UseCaptcha)
		{ %>
					<div class="field">		
						<asp:Label ID="lblCaptcha" runat="server" AssociatedControlID="tbxCaptcha" CssClass="field-title"><%= GetMessageRaw("Captcha") %><span class="starrequired">*</span></asp:Label>				    
						<asp:HiddenField ID="hfCaptchaGuid" runat="server" />
						<div class="form-input"><asp:TextBox ID="tbxCaptcha" runat="server" AutoCompleteType="Disabled" CssClass="input-field"></asp:TextBox></div>
						<asp:RequiredFieldValidator ID="rfvCaptcha" runat="server"  Display="None" ControlToValidate="tbxCaptcha" ErrorMessage="<%$ LocRaw:Message.CaptchaRequired %>" ToolTip="<%$ LocRaw:Message.CaptchaRequired %>" ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>				        
						<p style="clear:left;"><asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" /></p>
					</div>
					<% } %>
				</div>	
	
				<div class="button">		        	
					<asp:Button ID="RegisterButton" CausesValidation="true" ValidationGroup="CreateUserWizard1" runat="server" CommandName="MoveComplete" Text="<%$ LocRaw:ButtonText.Register %>" OnClick="OnUserRegister" CssClass="input-submit" />			
				</div>
			</div>
		</div>
		<b class="r0"></b><b class="r1"></b>
	</div>
</div>	
<%} %>
</asp:panel>
