<%@ Reference VirtualPath="~/bitrix/components/bitrix/pmessages.message.form/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.CommunicationUtility.Components.PrivateMessageFormTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.CommunicationUtility.Components" %>
<%@ Register TagPrefix="bx" Assembly="CommunicationUtility"  Namespace="Bitrix.CommunicationUtility" %>
<script runat="server">
	
	public override string PostTextareaClientID
	{
		get
		{
			return Content.ClientID;
		}
	}
	
	protected override void LoadData(PrivateMessageFormComponent.Data data)
	{
        if (Component.ComponentTarget == PrivateMessageFormComponent.Target.Topic)
		{
			TopicTitle.Text = data.TopicTitle;
		}



        cbNotifyByEmail.Checked = data.NotifyByEmail;
		Content.Text = data.PostContent;

	}
    
	protected override void SaveData(PrivateMessageFormComponent.Data data)
	{
        if (Component.FatalError != PrivateMessageFormComponent.ErrorCode.None)
            return;
        if (Component.ComponentTarget == PrivateMessageFormComponent.Target.Topic)
		{
			data.TopicTitle = TopicTitle.Text;
		}
		data.PostContent = Content.Text;

        if (    Receivers.Visible 
                && Component.ComponentTarget == PrivateMessageFormComponent.Target.Topic 
                && (Component.ComponentMode == PrivateMessageFormComponent.Mode.Add || Component.ComponentMode == PrivateMessageFormComponent.Mode.Invite))
        {

            Receivers.LoadPostData(Receivers.ValuesFormName, Request.Form);

            if (Receivers.Values.Count > 0)
            {
                int resCount = Receivers.Values.Count > Component.MaxReceiversCount && Component.MaxReceiversCount > 0 ? Component.MaxReceiversCount : Receivers.Values.Count;
                string strValues = string.Empty;
				int k=0;
				for (int i = 0; i < resCount; i++)
					if (int.TryParse(Receivers.Values[i], out k))
						Component.Receivers.Add(k);

                //strValues = strValues.Remove(strValues.Length - 1, 1);
                //Component.Parameters["Receivers"] = strValues;
            }
        }
        data.NotifyByEmail = cbNotifyByEmail.Checked;
        data.NotifySender = false;
	}
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
        Save.Text = DefaultButtonTitle;

        if (Component.FatalError != PrivateMessageFormComponent.ErrorCode.None)
			return;
		
        TopicTitleValidator.Enabled = (Component.ComponentTarget == PrivateMessageFormComponent.Target.Topic && Component.ComponentMode!= PrivateMessageFormComponent.Mode.Invite);
		string validationGroup = ClientID;
		Errors.ValidationGroup = validationGroup;
		Save.ValidationGroup = validationGroup;
		TopicTitleValidator.ValidationGroup = validationGroup;
		ContentValidator.ValidationGroup = validationGroup;


		if (Component.ComponentMode == PrivateMessageFormComponent.Mode.Invite)
		{
			ContentTitleContainer.Visible = false;
			ContentContainer.Visible = false;
			ContentHeaderContainer.Visible = false;
			Preview.Visible = false;
		}

        if ((Component.ComponentMode == PrivateMessageFormComponent.Mode.Edit || Component.ComponentMode == PrivateMessageFormComponent.Mode.Invite) && Component.ComponentTarget == PrivateMessageFormComponent.Target.Topic)
            ContentValidator.Enabled = false;
		
        if (Component.ComponentTarget == PrivateMessageFormComponent.Target.Topic && (( Component.ComponentMode == PrivateMessageFormComponent.Mode.Add
			&& Component.ReceiverUsers.Count == 0) || (Component.ComponentMode == PrivateMessageFormComponent.Mode.Invite)))
        {
            //cbNotifyByEmail.Checked = true;
            Receivers.ListItemTemplate =
                String.Format(@"<div class=""forum-reply-field"">#data.name#&nbsp<a href=""{4}"">{2}</a>&nbsp
                        <a href = ""#"" onclick=""{5}RemoveItem('{0}','#parentId#','{1}','#hiddenId#'); return false;"">{3}</a></div>",
                                    Receivers.ListContainerId,
                                    Receivers.ListContainerId,
                                    GetMessage("Label.ProfileLink"),
                                    GetMessage("Label.DeleteReceiver"),
                                    Component.Parameters["UserProfileUrlTemplate"],
                                    Receivers.ClientID);
            Receivers.MaxItemsCount = Component.MaxReceiversCount;
            string s = HttpRuntime.AppDomainAppVirtualPath;
            string currentUrl = HttpUtility.UrlDecode(Bitrix.Services.BXSefUrlManager.CurrentUrl.AbsolutePath);

            Receivers.Url = VirtualPathUtility.ToAppRelative(currentUrl) + Bitrix.Services.BXSefUrlManager.CurrentUrl.Query;
            Receivers.LabelText = GetMessage("Label.Receivers")+(Component.MaxReceiversCount>0 ? 
                Component.MaxReceiversCount==1 ? " "+GetMessage("Title.AddReceiver"): " "+String.Format(GetMessage("Title.AddReceivers"),Component.MaxReceiversCount):string.Empty);
            Receivers.DefaultText = GetMessage("Message.EnterUser");
            if (Request.Form[Receivers.ValuesFormName] != null)
            {
                Receivers.LoadPostData(Receivers.ValuesFormName, Request.Form);
                Receivers.InitialShowData = Component.GetReceiversInitialData(Receivers.Values);
            }
            Receivers.AttachScript();

        }
        else Receivers.Visible = false;
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		
		if (Component.FatalError != PrivateMessageFormComponent.ErrorCode.None)
		{
			Errors.Enabled = false;
			TopicTitleValidator.Enabled = false;
			ContentValidator.Enabled = false;
			return;
		}

        
        if (Component.ComponentMode == Bitrix.CommunicationUtility.Components.PrivateMessageFormComponent.Mode.Edit
            && Component.ComponentTarget == Bitrix.CommunicationUtility.Components.PrivateMessageFormComponent.Target.Topic)
        {
            Preview.Visible = false;
            ContentValidator.Enabled = false;
        }

		string validateScript = @"
		if (typeof(ValidatorOnSubmit) == ""function"")
		{{	
			var isValidated = ValidatorOnSubmit();
			if (!isValidated)
			{{
				window.location=""#postform"";													
				return false;
			}}
			return true;
		}}";

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
	if (Component.FatalError != PrivateMessageFormComponent.ErrorCode.None)
	{ 
		%>
		<div class="forum-content">
		<div class="forum-note-box forum-note-error">
			<div class="forum-note-box-text"><%= Component.GetErrorHtml(Component.FatalError) %></div>
		</div>	
		</div>
		<%

	}
%>
<% 
        if ((Component.FatalError & PrivateMessageFormComponent.ErrorCode.Fatal) == PrivateMessageFormComponent.ErrorCode.Fatal)
		return;
		%>

<div class="pmessages-content">

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
	</div>
	<div class="forum-header-title"><span><%= DefaultHeaderTitle %></span></div>
</div>

<div class="forum-reply-form" id="postform">
	<%
		Errors.HeaderText = string.Format(@"<div class=""forum-note-box-text"">{0}</div>", GetMessage("Error.Title"));
		foreach (string s in Component.ErrorSummary)
			Errors.AddErrorMessage(s);
	%>
		
	<bx:BXValidationSummary runat="server" ID="Errors" ValidationGroup="bxPMessage" CssClass="forum-note-box forum-note-error" ForeColor="" /> 
	
	<% if (Component.ComponentTarget == PrivateMessageFormComponent.Target.Topic) { %>
	<div id="pmessages_reply_recievers" class="forum-reply-fields">

		<div class="forum-reply-field forum-reply-field-desc">

		<bx:BXAutoCompleteTextBox
    	        runat="server" 
    	        ID="Receivers"
    	        TextBoxClass="receivers-textbox"
    	        MainContainerClass="autocomplete-main-container"
    	        LabelText=""
    	        Url=""
                CreateMainContainer="true"
                CreateListContainer="true"
                SetFieldFontSizeToPopup="true"
                DataIdReplaceString="#UserId#"
                DefaultText=""
                EmptyCssClass="pmessages-newtopic-combo-empty"
                FullCssClass="pmessages-newtopic-combo-full"
    	        />
    	       
   
	<% if (Component.ReceiverUsers.Count>0)
     { %>
        <span><%=( GetMessage(Component.ReceiverUsers.Count == 1 ? "Label.Receiver":"Label.Receivers") ) + ": " + Component.ReceiverNamesList%></span>
	 <%} %>
		</div>
		<div class="forum-reply-field forum-reply-field-title" runat="server" id = "ContentTitleContainer">

			<label for="<%= Encode(TopicTitle.ClientID) %>"><%= GetMessage("Label.TopicTitle")%><span class="forum-required-field">*</span></label>
			<asp:TextBox runat="server" ID="TopicTitle" MaxLength="255" TabIndex="2" Columns="70" />
			<asp:RequiredFieldValidator runat="server" ID="TopicTitleValidator" ErrorMessage="<%$ Loc:Error.TopicTitleRequired %>" ControlToValidate="TopicTitle" Display="None" SetFocusOnError="true" ValidationGroup="bxPMessage" />
		</div>

    </div>
	<% } %>    
    
    <% if (Component.ComponentTarget == Bitrix.CommunicationUtility.Components.PrivateMessageFormComponent.Target.Topic &&
		   Component.ComponentMode != PrivateMessageFormComponent.Mode.Invite && Component.AllowNotifyByEmail)
       { %>
    
    <div class="forum-reply-fields">
    <asp:CheckBox runat="server" ID="cbNotifyByEmail" TabIndex="4"/>
    <label for="<%=cbNotifyByEmail.ClientID%>"><%=GetMessage("Label.NotifyByEmail")%></label>
    
    </div>
    
    <%}
      
     %>
    
<%--   <% if (!(Component.ComponentMode == Bitrix.CommunicationUtility.Components.PrivateMessageFormComponent.Mode.Edit
            && Component.ComponentTarget == Bitrix.CommunicationUtility.Components.PrivateMessageFormComponent.Target.Topic))
      {%>--%>

	<div class="forum-reply-header" runat="server" id= "ContentHeaderContainer">
		    <%= GetMessage("Label.PostContent")%><span class="forum-required-field">*</span></div>
	<%--	    <%} %>--%>
	<div class="forum-reply-fields" >
	
	
<%--   <% if (!(Component.ComponentMode == Bitrix.CommunicationUtility.Components.PrivateMessageFormComponent.Mode.Edit
            && Component.ComponentTarget == Bitrix.CommunicationUtility.Components.PrivateMessageFormComponent.Target.Topic))
      {%>--%>
	<div runat="server" id = "ContentContainer">
		<div class="forum-reply-field forum-reply-field-bbcode">
			<bx:BBCodeLine runat="server" ID="BBCode" TextControl="Content" TagList="b,u,i,s,url,quote,code,list,color,img,video,audio" CssClass="forum-bbcode-line"/>
			<div class="forum-clear-float"></div>
		</div>

		<div class="forum-reply-field forum-reply-field-text"> 
			<asp:TextBox runat="server" ID="Content" TextMode="MultiLine" Columns="55" Rows="14" TabIndex="6" />
			<asp:RequiredFieldValidator runat="server" ID="ContentValidator" ErrorMessage="<%$ Loc:Error.PostContentRequired %>" ControlToValidate="Content" Display="None" SetFocusOnError="true" ValidationGroup="bxPMessage" />
		</div>
	</div>
<%--		<%} %>--%>
		<div class="forum-reply-buttons">
			<asp:Button runat="server" ID="Save" TabIndex="11" OnClick="SaveClick" ValidationGroup="bxPMessage" /> 
			<asp:Button runat="server" ID="Preview" TabIndex="12" Text="<%$ LocRaw:ButtonText.Preview %>" OnClick="PreviewClick" CausesValidation="false" />
		</div>
	</div>
</div>

		<% if ( Component.MaxMessageCount > 0 ){ %>
		<div class="forum-info-box">
		<span><%=Component.MailBoxStatusMessage%></span>
		</div>
		<%} %>

</div>
<script type="text/javascript">

    function <%=Receivers.ClientID%>RemoveItem(listContainerId, listItemId, mainContainerId, hiddenId) {

        var lc = document.getElementById(listContainerId);
        var mc = document.getElementById(mainContainerId);
        var li = document.getElementById(listItemId);
        var hi = document.getElementById(hiddenId);
        if ( mc && hi )
            mc.removeChild(hi);
        if ( lc && li )
            lc.removeChild(li);
    }

</script>
