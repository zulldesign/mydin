<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PasswordRecovery/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Main.Components.SystemPasswordRecoveryTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<script runat="server">
	bool success;
	
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		
		Page.Title = GetMessageRaw("PageTitle");
				
		ErrorMessage1.ValidationGroup = ClientID + "_1";
		LoginValidator.ValidationGroup = ClientID + "_1";
		NextButton.ValidationGroup = ClientID + "_1";
		
		ErrorMessage2.ValidationGroup = ClientID + "_2";
		AnswerValidator.ValidationGroup = ClientID + "_2";
		AnswerButton.ValidationGroup = ClientID + "_2";
	}	
	protected void NextButton_Click(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;
		
		var parameters = new BXParamsBag<object>();
		parameters.Add("Login", LoginField.Text);
		var errors = new List<string>();
		if (!Component.ProcessCommand("validate", parameters, errors))
		{
			foreach (var error in errors)
				ErrorMessage1.AddErrorMessage(error);
			return;
		}
		
		LoginValue.Value = LoginField.Text;
		QuestionValue.Value = parameters.GetString("PasswordQuestion");
		Step2.Value = "True";
	}

	protected void AnswerButton_Click(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;
	
		var parameters = new BXParamsBag<object>();
		parameters.Add("Login", LoginValue.Value);
		parameters.Add("PasswordAnswer", AnswerField.Text);
		var errors = new List<string>();
		if (!Component.ProcessCommand("recovery", parameters, errors))
		{
			foreach (var error in errors)
				ErrorMessage2.AddErrorMessage(error);
			return;
		}
		
		success = true;
	}
</script>
<% if (Component.FeatureEnabled) { %>
<% if (string.IsNullOrEmpty(Step2.Value)) { %>
<div class="content-form forgot-form" onkeypress="return FireDefaultButton(event, '<%= NextButton.ClientID %>');">
	<div class="fields">
		<bx:BXValidationSummary id="ErrorMessage1" runat="server" CssClass="errortext" ForeColor="" />
		
		<div class="field">
			<label class="field-title" for="<%= Encode(LoginField.ClientID) %>"><%= GetMessageRaw("Login") %><asp:RequiredFieldValidator ID="LoginValidator" runat="server" ControlToValidate="LoginField" ErrorMessage="<%$ LocRaw:LoginRequired %>">*</asp:RequiredFieldValidator></label>
			<div class="form-input"><asp:TextBox ID="LoginField" runat="server" CssClass="input-field" /></div>
		</div>
		<div class="field field-button">
			<asp:Button ID="NextButton" runat="server" Text="<%$ LocRaw:ButtonText.Next %>" OnClick="NextButton_Click" CssClass="input-submit" />
		</div>
		
		<p><%= GetMessageRaw("EnterLogin") %></p>
		<p><%= string.Format(GetMessageRaw("ReturnToAuth"), Encode(Component.LoginLink)) %></p>
	</div>
</div>
<% } else { %>
<div class="content-form forgot-form" onkeypress="return FireDefaultButton(event, '<%= AnswerButton.ClientID %>');">
	<div class="fields">
		<bx:BXValidationSummary id="ErrorMessage2" runat="server" CssClass="errortext" ForeColor="" />
		
		<% if (success) { %>
		<div class="field">
			<span class="notetext"><%= GetMessageRaw("SuccessMessage") %></span>
		</div>
		<% } %>		
		
		<asp:HiddenField ID="Step2" runat="server" />	
		<asp:HiddenField ID="LoginValue" runat="server" />
		<asp:HiddenField ID="QuestionValue" runat="server" />
		
		<div class="field">
			<label class="field-title"><%= GetMessageRaw("Login") %></label>
			<div class="form-text"><%= Encode(LoginValue.Value) %></div>
		</div>
		<div class="field">
			<label class="field-title"><%= GetMessageRaw("Question") %></label>
			<div class="form-text"><%= Encode(QuestionValue.Value) %></div>
		</div>
		<div class="field">
			<label class="field-title" for="<%= Encode(AnswerField.ClientID) %>"><%= GetMessageRaw("Answer") %><asp:RequiredFieldValidator ID="AnswerValidator" runat="server" ControlToValidate="AnswerField" ValidationGroup="vgPasswordRecoveryFrom" ErrorMessage="<%$ LocRaw:Message.AnswerRequired %>">*</asp:RequiredFieldValidator></label>
			<div class="form-input"><asp:TextBox ID="AnswerField" runat="server" CssClass="input-field" /></div>
		</div>
		<div class="field field-button">
			<asp:Button ID="AnswerButton" runat="server" Text="<%$ LocRaw:ButtonText.ChangePassword %>" OnClick="AnswerButton_Click" CssClass="input-submit" />
		</div>
		
		<p><%= GetMessageRaw("EnterAnswer") %></p>
		<p><%= string.Format(GetMessageRaw("ReturnToAuth"), Encode(Component.LoginLink)) %></p>
	</div>
</div>
<% } %>
<% } else { %>
<div class="content-form forgot-form" >
	<div class="fields">
		<span class="notetext"><%= GetMessageRaw("Disabled") %></span>
	</div>
</div>
<% } %>