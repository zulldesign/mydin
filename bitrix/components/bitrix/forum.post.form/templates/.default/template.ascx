<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.post.form/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Forum.Components.ForumPostFormTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Register TagPrefix="bx" Assembly="CommunicationUtility"  Namespace="Bitrix.CommunicationUtility" %>
<script runat="server">
	
	public override string PostTextareaClientID
	{
		get
		{
			return Content.ClientID;
		}
	}

	private bool showApproveCheckbox;
	private bool showSubscribeCheckbox;
	
	protected override void LoadData(ForumPostFormComponent.Data data)
	{
		if (Component.ComponentTarget == ForumPostFormComponent.Target.Topic)
		{
			TopicTitle.Text = data.TopicTitle;
			TopicDescription.Text = data.TopicDescription;
		}
		if (Component.IsGuest)
		{
			GuestName.Text = data.GuestName;
			if (Component.ShowGuestEmail)
				GuestEmail.Text = data.GuestEmail;
		}
		Content.Text = data.PostContent;
		Hide.Checked = !data.IsApproved;
		SubcribeToNewTopic.Checked = data.SubscribeToNewTopic;
	}
	protected override void SaveData(ForumPostFormComponent.Data data)
	{
		if (Component.ComponentTarget == ForumPostFormComponent.Target.Topic)
		{
			data.TopicTitle = TopicTitle.Text;
			data.TopicDescription = TopicDescription.Text;
		}
		if (Component.IsGuest)
		{
			data.GuestName = GuestName.Text;
			if (Component.ShowGuestEmail)
				data.GuestEmail = GuestEmail.Text;
			if (Component.RequireGuestCaptcha)
			{
				data.GuestCapthca = GuestCaptcha.Text;
				data.GuestCaptchaGuid = GuestCaptchaGuid.Value;
			}
		}
		data.PostContent = Content.Text;

		if (showApproveCheckbox)
			data.IsApproved = !Hide.Checked;
		if (showSubscribeCheckbox)
			data.SubscribeToNewTopic = SubcribeToNewTopic.Checked;
	}
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		if (Component.FatalError != ForumPostFormComponent.ErrorCode.None)
			return;
		
		GuestNameValidator.Enabled = Component.IsGuest;
		GuestEmailValidator.Enabled = Component.IsGuest && Component.RequireGuestEmail;
		GuestCaptchaValidator.Enabled = Component.IsGuest && Component.RequireGuestCaptcha;
		TopicTitleValidator.Enabled = (Component.ComponentTarget == ForumPostFormComponent.Target.Topic);

		string validationGroup = ClientID;
		Errors.ValidationGroup = validationGroup;
		Save.ValidationGroup = validationGroup;
		GuestNameValidator.ValidationGroup = validationGroup;
		GuestEmailValidator.ValidationGroup = validationGroup;
		GuestCaptchaValidator.ValidationGroup = validationGroup;
		TopicTitleValidator.ValidationGroup = validationGroup;
		ContentValidator.ValidationGroup = validationGroup;

		showApproveCheckbox = Component.CanApprove 
			&& Component.ComponentTarget == ForumPostFormComponent.Target.Post 
			&& Component.ComponentMode == ForumPostFormComponent.Mode.Add;

		showSubscribeCheckbox = Component.Auth.CanSubscribe
			&& Component.ComponentTarget == ForumPostFormComponent.Target.Topic
			&& Component.ComponentMode == ForumPostFormComponent.Mode.Add;
		
		Save.Text = DefaultButtonTitle;
		if (showApproveCheckbox)
			Hide.Text = GetMessage("CheckBoxTitle.HidePostAfterCreation");

		if (showSubscribeCheckbox)
			SubcribeToNewTopic.Text = GetMessage("CheckBoxTitle.SubcribeToNewTopic");
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		
		if (Component.FatalError != ForumPostFormComponent.ErrorCode.None)
		{
			Errors.Enabled = false;
			GuestNameValidator.Enabled = false;
			GuestEmailValidator.Enabled = false;
			GuestCaptchaValidator.Enabled = false;
			TopicTitleValidator.Enabled = false;
			ContentValidator.Enabled = false;
			return;
		}

		string validateScript = String.Format(@"
		if (typeof(ValidatorOnSubmit) == ""function"")
		{{
			var isValidated = ValidatorOnSubmit();
			if (!isValidated)
			{{
				window.location=""#postform"";													
				return false;
			}}
			return true;
		}}", Content.ClientID);

		Page.ClientScript.RegisterOnSubmitStatement(Page.GetType(), "NewValidate", validateScript);
		
		if (PreviewPost)
			Content.Focus();
	}

	protected new void SaveClick(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;
		
		base.SaveClick(sender, e);
	}
</script>

<% 
	if (Component.FatalError != ForumPostFormComponent.ErrorCode.None)
	{ 
		%>
		<div class="forum-content">
		<div class="forum-note-box forum-note-error">
			<div class="forum-note-box-text"><%= Component.GetErrorHtml(Component.FatalError) %></div>
		</div>	
		</div>
		<%
		return;
	}
%>

<div class="forum-content">

<% if (PreviewPost && !String.IsNullOrEmpty(Content.Text)) {%>
<div class="forum-header-box">
	<div class="forum-header-title"><span><%= GetMessage("Label.Preview")%></span></div>
</div>
<div class="forum-info-box forum-post-preview">
	<div class="forum-info-box-inner"><% Component.Preview(Content.Text, CurrentWriter); %></div>
</div>
<% } %>

<div class="forum-header-box">
	<div class="forum-header-options">
	<% 
	for (int i = 0; i < Component.HeaderLinks.Count; i++) 
	{ 
		ForumPostFormComponent.LinkInfo link = Component.HeaderLinks[i];
		%><% if (i != 0) { %>&nbsp;&nbsp;<% } %><span class="<%= link.CssClass ?? ("forum-option-" + i) %>"><a href="<%= link.Href %>"><%= link.Title %></a></span><%
	}	
	%>
	</div>
	<div class="forum-header-title"><span><%= DefaultHeaderTitle %></span></div>
</div>

<div class="forum-reply-form" id="postform">
	<%
		Errors.HeaderText = string.Format(@"<div class=""forum-note-box-text"">{0}</div>", GetMessage("Error.Title"));
		foreach (string s in Component.ErrorSummary)
			Errors.AddErrorMessage(s);
	%>
		
	<bx:BXValidationSummary runat="server" ID="Errors" ValidationGroup="bxForumPost" CssClass="forum-note-box forum-note-error" ForeColor="" /> 

	<% if (Component.ComponentTarget == ForumPostFormComponent.Target.Topic || Component.IsGuest) { %>
	<div class="forum-reply-fields">
		<% if (Component.ComponentTarget == ForumPostFormComponent.Target.Topic) { %>
		<div class="forum-reply-field forum-reply-field-title">
			<label for="<%= Encode(TopicTitle.ClientID) %>"><%= GetMessage("Label.TopicTitle") %><span class="forum-required-field">*</span></label>
			<asp:TextBox runat="server" ID="TopicTitle" MaxLength="255" TabIndex="1" Columns="70" />
			<asp:RequiredFieldValidator runat="server" ID="TopicTitleValidator" ErrorMessage="<%$ Loc:Error.TopicTitleRequired %>" ControlToValidate="TopicTitle" Display="None" SetFocusOnError="true" ValidationGroup="bxForumPost" />
		</div>
		
		<div class="forum-reply-field forum-reply-field-desc">
			<label for="<%= Encode(TopicDescription.ClientID) %>"><%= GetMessage("Label.TopicDescription") %></label>
			<asp:TextBox runat="server" ID="TopicDescription" MaxLength="255" TabIndex="2" Columns="70" />
		</div>
		<% } %>
		
		<% if (Component.IsGuest) { %>
		<div class="forum-reply-field-user">
			<div class="forum-reply-field forum-reply-field-author">
				<label for="<%= Encode(GuestName.ClientID) %>"><%= GetMessage("Label.GuestName") %><span class="forum-required-field">*</span></label>
				<span><asp:TextBox runat="server" ID="GuestName" MaxLength="255" TabIndex="3" Columns="30" /></span>
				<asp:RequiredFieldValidator runat="server" ID="GuestNameValidator" ErrorMessage="<%$ Loc:Error.GuestNameRequired %>" ControlToValidate="GuestName" Display="None" SetFocusOnError="true" ValidationGroup="bxForumPost" />
			</div>
			
			<div class="forum-reply-field-user-sep">&nbsp;</div>
			
			<% if (Component.ShowGuestEmail) { %>
			<div class="forum-reply-field forum-reply-field-email">
				<label for="<%= Encode(GuestEmail.ClientID) %>">E-mail<% if (Component.RequireGuestEmail) { %><span class="forum-required-field">*</span><% } %></label>
				<span><asp:TextBox runat="server" ID="GuestEmail" MaxLength="255" TabIndex="4" Columns="30" /></span>
				<asp:RequiredFieldValidator runat="server" ID="GuestEmailValidator" ErrorMessage="<%$ Loc:Error.GuestEmailRequired %>" ControlToValidate="GuestEmail" Display="None" SetFocusOnError="true" ValidationGroup="bxForumPost" />
			</div>
			<% } %>
			
			<div class="forum-clear-float">
			</div>
		</div>
		<% } %>
	</div>
	<% } %>
	
	<div class="forum-reply-header"><%= GetMessage("Label.PostContent") %><span class="forum-required-field">*</span></div>
	<div class="forum-reply-fields">
	
		<% if (Component.Forum.AllowBBCode) { %>
		<div class="forum-reply-field forum-reply-field-bbcode">
			<bx:BBCodeLine runat="server" ID="BBCode" TextControl="Content" TagList="b,u,i,s,url,quote,code,list,color,img,video,audio" CssClass="forum-bbcode-line"/>
			<div class="forum-clear-float"></div>
		</div>
		<% } %>

		<div class="forum-reply-field forum-reply-field-text">
			<asp:TextBox runat="server" ID="Content" TextMode="MultiLine" Columns="55" Rows="14" TabIndex="6" />
			<asp:RequiredFieldValidator runat="server" ID="ContentValidator" ErrorMessage="<%$ Loc:Error.PostContentRequired %>" ControlToValidate="Content" Display="None" SetFocusOnError="true" ValidationGroup="bxForumPost" />
		</div>
		
		<% if (Component.IsGuest && Component.RequireGuestCaptcha) { %>
		<div class="forum-reply-field forum-reply-field-captcha">
			<div class="forum-reply-field-captcha-label">
				<% GuestCaptchaGuid.Value = Component.ComponentData.GuestCaptchaGuid; %>
				<% GuestCaptcha.Text = string.Empty; %>
				<label for="<%= Encode(GuestCaptcha.ClientID) %>"><%= GetMessage("Label.GuestCaptcha") %><span class="forum-required-field">*</span></label>
				<asp:TextBox runat="server" ID="GuestCaptcha" MaxLength="10" TabIndex="7" Columns="30" />
				<asp:RequiredFieldValidator runat="server" ID="GuestCaptchaValidator" ErrorMessage="<%$ Loc:Error.GuestCaptchaRequired %>" ControlToValidate="GuestCaptcha" Display="None" SetFocusOnError="true" ValidationGroup="bxForumPost" />
				<asp:HiddenField ID="GuestCaptchaGuid" runat="server" />
			</div>
			
			<div class="forum-reply-field-captcha-image">
				<img src="<%= Component.ComponentData.GuestCaptchaHref %>" width="180" height="50" alt="<%= GetMessage("Label.GuestCaptcha") %>" />
			</div>
		</div>
		<% } %>
		
		<% if (showApproveCheckbox || showSubscribeCheckbox) { %>
		<div class="forum-reply-field forum-reply-field-settings">
			<% if (showApproveCheckbox) { %><div class="forum-reply-field-setting"><asp:CheckBox runat="server" ID="Hide" TabIndex="8" /></div><%} %>
			<% if (showSubscribeCheckbox) { %><div class="forum-reply-field-setting"><asp:CheckBox runat="server" ID="SubcribeToNewTopic" TabIndex="9" /></div><%} %>
		</div>
		<% } %>
		<div class="forum-reply-buttons">
			<asp:Button runat="server" ID="Save" TabIndex="11" OnClick="SaveClick" ValidationGroup="bxForumPost" /> 
			<asp:Button runat="server" ID="Preview" TabIndex="12" Text="<%$ LocRaw:ButtonText.Preview %>" OnClick="PreviewClick" CausesValidation="false" />
		</div>
	</div>
</div>

</div>
