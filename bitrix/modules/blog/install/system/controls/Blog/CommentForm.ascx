<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommentForm.ascx.cs" Inherits="Bitrix.Blog.CommentForm" EnableViewState="false" %>
<%@ Register TagPrefix="bx" Assembly="CommunicationUtility"  Namespace="Bitrix.CommunicationUtility" %>
<div id="<%= ClientID %>" <%= Hidden ? @"style=""display:none;""" : "" %><%= !string.IsNullOrEmpty(CssClass) ? " class=\"" + Encode(CssClass) + "\"" : "" %>>
	<%
	Errors.HeaderText = string.Format(@"<div class=""blog-note-box-text"">{0}</div>", GetMessage("ErrorTitle"));
	%>
	<div id="<%= ID %>"></div>
	<bx:BXValidationSummary runat="server" ID="Errors" CssClass="blog-note-box blog-note-error" ForeColor=""  /> 
	<asp:HiddenField runat="server" ID="ParentId" />
	
	<div class="blog-comment-fields">
		<% if (RequireUserName != FieldMode.Hide || RequireUserEmail != FieldMode.Hide) { %>
		<div class="blog-comment-field-user">
			<% if (RequireUserName != FieldMode.Hide) { %>
			<div class="blog-comment-field blog-comment-field-author">
				<div class="blog-comment-field-text">
					<label for="<%= Name.ClientID %>" class="blog-comment-field-caption"><%= GetMessage("Label.Name") %></label><% if (RequireUserName == FieldMode.Require) { %><span class="blog-required-field">*</span><% } %>
				</div>
				<span>
				<asp:TextBox runat="server" ID="Name" Columns="30" TabIndex="3" MaxLength="255" />
				<asp:RequiredFieldValidator runat="server"  Display="None" ID="NameRequired" ControlToValidate="Name" ErrorMessage="<%$ Loc:Message.NameIsNotSpecified %>" SetFocusOnError="true" ></asp:RequiredFieldValidator>
				</span>
			</div>
			<% } %>
			<% if (RequireUserName != FieldMode.Hide && RequireUserEmail != FieldMode.Hide) { %>
			<div class="blog-comment-field-user-sep">&nbsp;</div>
			<% } %>
			<% if (RequireUserEmail != FieldMode.Hide) { %>
			<div class="blog-comment-field blog-comment-field-email">
				<div class="blog-comment-field-text">
					<label for="<%= Email.ClientID %>" class="blog-comment-field-caption">E-mail</label><% if (RequireUserEmail == FieldMode.Require) { %><span class="blog-required-field">*</span><% } %>
				</div>
				<span>
				<asp:TextBox runat="server" ID="Email" Columns="30" TabIndex="4" MaxLength="255" />
				<asp:RequiredFieldValidator runat="server" Display="None" ID="EmailRequired" ControlToValidate="Email" ErrorMessage="<%$ Loc:Message.EmailIsNotSpecified %>" SetFocusOnError="true" ></asp:RequiredFieldValidator>
				<asp:RegularExpressionValidator runat="server" Display="None" ID="EmailValid" ControlToValidate="Email" ErrorMessage="<%$ Loc:Message.InvalidEmail %>" SetFocusOnError="true" ValidationExpression="^\s*[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]+\s*$" ></asp:RegularExpressionValidator>
				</span>
			</div>
			<% } %>
			<div class="blog-clear-float"></div>
		</div>
		<% } %>
		<div class="blog-comment-field blog-comment-field-bbcode">
			<bx:BBCodeLine runat="server" ID="BBCode" TextControl="Content" TagList="b,u,i,s,url,quote,code,list,color"/>
			<div class="blog-clear-float"></div>
		</div>
		<div class="blog-comment-field blog-comment-field-content">
			
			<asp:TextBox runat="server" ID="Content" TextMode="MultiLine" TabIndex="5" Columns="50" Rows="10" />
			<asp:RequiredFieldValidator runat="server" Display="None" ID="ContentRequired" ControlToValidate="Content" ErrorMessage="<%$ Loc:Message.CommentContentIsEmpty %>" SetFocusOnError="true"></asp:RequiredFieldValidator>
		</div>
		<% if (RequireCaptcha) { %>
		<div class="blog-comment-field blog-comment-field-captcha">
			<div class="blog-comment-field-captcha-label">
				<label for="<%= CaptchaTextBox.ClientID %>" class="blog-comment-field-caption"><%= GetMessage("Label.Captcha") %></label><span class="blog-required-field">*</span><br/>
				<% CaptchaTextBox.Text = ""; %>
				<asp:TextBox runat="server" ID="CaptchaTextBox" Columns="30" TabIndex="7" />
				<asp:RequiredFieldValidator runat="server" Display="None" ID="CaptchaRequired" ControlToValidate="CaptchaTextBox" ErrorMessage="<%$ Loc:Message.CaptchaIsNotSpecified %>" SetFocusOnError="true" ></asp:RequiredFieldValidator>
				<% Guid.Value = CaptchaGuid; %>
				<asp:HiddenField runat="server" ID="Guid" />
			</div>
			<div class="blog-comment-field-captcha-image">
				<img alt="Captcha" src="<%= CaptchaHref %>" />
			</div>
		</div>
		<% } %>
		<div class="blog-comment-buttons">
			<asp:Button runat="server" ID="SubmitButton" Text="<%$ LocRaw:ButtonText.Submit %>" TabIndex="10" OnClick="Submit_Click" />
			<asp:Button runat="server" ID="PreviewButton" Text="<%$ LocRaw:ButtonText.View %>" TabIndex="11" OnClick="Preview_Click" CausesValidation="false" />
		</div>
	</div>
</div>
<script type="text/javascript">
	function <%= ClientID %>_Allocate(target, parentId, visible)
	{
		if (target == null) return;
		
		var div = document.getElementById('<%= ClientID %>');
		if (div == null) return;
		
		var parent = div.parentNode;
		if (!visible)
			div.style.display = "none";
			
		parent.removeChild(div);
		target.appendChild(div);
		
		document.getElementById("<%= ParentId.ClientID %>").value = parentId;
			
		if (visible)
			div.style.display = "";
	}
	function <%= ClientID %>_Focus()
	{
		var div = document.getElementById('<%= ClientID %>');
		if (div == null) return;
		if (div.style.display == "none") return;
		
		var target = document.getElementById('<%= Content.ClientID %>');
		if (target != null)
			target.focus();
	}
</script>