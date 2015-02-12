<%@ Reference Control="~/bitrix/components/bitrix/forum.comment.block/component.ascx" %>
<%@ Register TagPrefix="bx" Assembly="CommunicationUtility"  Namespace="Bitrix.CommunicationUtility" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Forum.Components.ForumCommentBlockTemplate" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Import Namespace="Bitrix.Forum" %>
<%@ Import Namespace="System.Collections.Generic" %>
<script runat="server">
    protected override void OnLoad(EventArgs e)
    {
        GuestNameValidator.Enabled = !Component.IsAuthentificated;
        GuestEmailValidator.Enabled = !Component.IsAuthentificated && Component.RequireGuestEmail;
        GuestCaptchaValidator.Enabled = !Component.IsAuthentificated && Component.ShowGuestCaptcha;

        if (Component.Operation == ForumCommentBlockComponent.PostOperation.Edit && Component.PostId > 0)
        {
            ForumCommentBlockComponent.PostData data = Component.ComponentPostData;
            PostFormPlaceholderData.Value = string.Concat("{\"mode\":Bitrix.ForumCommentFormMode.edit, \"containerId\":\"", GetPostFormContainerClientID(Component.PostId), "\"}");
            Content.Text = data.PostContent;
            if (!Component.IsAuthentificated)
            {
                GuestName.Text = data.GuestName;
                GuestEmail.Text = data.GuestEmail;
            }

            if (Component.CanApprove)
                Hide.Checked = !data.IsApproved; 
                
            Cancel.OnClientClick = string.Concat("window.location.href='", Component.PostReadUrl, "'; return false;");
        }        
        base.OnLoad(e);
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (Component.ComponentError !=  ForumCommentBlockError.None)
        {
            Errors.Enabled = false;
            GuestNameValidator.Enabled = false;
            GuestEmailValidator.Enabled = false;
            GuestCaptchaValidator.Enabled = false;
            ContentValidator.Enabled = false;
            return;
        }

        Page.ClientScript.RegisterOnSubmitStatement(
            GetType(), 
            "NewValidate",
            string.Concat("if (typeof(ValidatorOnSubmit) == \"function\") { if(ValidatorOnSubmit()) return true; window.location=\"#", PostFormContainerClientID, "\"; return false; }")
        );

        
        Page.ClientScript.RegisterStartupScript(
            GetType(),
            "Initialize",
            string.Format(
                "Bitrix.ForumCommentBlockTemplate.create('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', Bitrix.ForumCommentFormMode.{6}).layout();",
                ClientID,
                PostFormContainerClientID,
                Content.ClientID,
                ParentComment.ClientID,
                PostFormPlaceholderData.ClientID,
                PostPreviewContainerClientID,
                Component.Operation == ForumCommentBlockComponent.PostOperation.Edit && Component.PostId > 0 ? "edit" : "add"
            ),
            true
            );

		if (Component.ShowPostForm && Component.Operation == ForumCommentBlockComponent.PostOperation.Add
			&& Component.CanReplyToTopic && (!Component.IsTopicClosed || Component.CanOpenCloseTopic))
		{
		Page.ClientScript.RegisterStartupScript(
		  GetType(),
		  "ShowPostForm",
		  string.Format(
			  "Bitrix.ForumCommentBlockTemplate.reply2Author('{0}', '{1}', '{2}', {3}, '0', true)",
			  ClientID,
			  ClientID + ClientIDSeparator + "BottomItemFormContainer",
			  JSEncode(Component.TopicAuthorName),
			  Component.IsBbCodeAllowed ? "true" : "false"
		  ),
		  true
		  );
		}
        
        if (PreviewPost)
            Content.Focus();
    }

    protected string postFormContainerClientID = null;
    protected string PostFormContainerClientID
    {
        get { return this.postFormContainerClientID ?? (this.postFormContainerClientID = string.Concat(ClientID, ClientIDSeparator, "PostFormContainer")); } 
    }

    protected string postPreviewContainerClientID = null;
    protected string PostPreviewContainerClientID
    {
        get { return this.postPreviewContainerClientID ?? (this.postPreviewContainerClientID = string.Concat(ClientID, ClientIDSeparator, "PostPreviewContainer")); }
    }

    protected string GetPostFormContainerClientID(long postId)
    {
        return string.Concat(ClientID, ClientIDSeparator, postId > 0L ? postId.ToString() : "0", ClientIDSeparator,  "ItemFormContainer"); 
    }   
     
    protected override void GetPostData(ForumCommentBlockComponent.PostData data)
    {
        if (data == null)
            return;
        
        if (!Component.IsAuthentificated)
        {
            data.GuestName = GuestName.Text;
            if (Component.ShowGuestEmail)
                data.GuestEmail = GuestEmail.Text;
            if (Component.ShowGuestCaptcha)
            {
                data.GuestCapthca = GuestCaptcha.Text;
                data.GuestCaptchaGuid = GuestCaptchaGuid.Value;
            }
        }
        data.PostContent = Content.Text;
        long parentId = 0;
        if(!string.IsNullOrEmpty(ParentComment.Value))
            long.TryParse(ParentComment.Value, out parentId);
        data.ParentPostId = parentId;
        
        if (Component.CanApprove)
            data.IsApproved = !Hide.Checked;
    }

    protected override void SetPostData(ForumCommentBlockComponent.PostData data)
    {
        if (data == null)
            return;

        if (!Component.IsAuthentificated)
        {
            GuestName.Text = data.GuestName;
            if (Component.ShowGuestEmail)
                GuestEmail.Text = data.GuestEmail;
            if (Component.ShowGuestCaptcha)
                GuestCaptchaGuid.Value = data.GuestCaptchaGuid;
        }
        Content.Text = data.PostContent;
        ParentComment.Value = data.ParentPostId.ToString();
        
        if (Component.CanApprove)
            Hide.Checked = !data.IsApproved;                
    }     
     
    protected void SavePostClick(object sender, EventArgs e)
    {
        if (Page.IsValid)
            SavePost();
    }
    
    protected void PreviewPostClick(object sender, EventArgs e)
    {
        PreviewPost = true;
    }            
</script>

<% 
    if ((Component.ComponentError != ForumCommentBlockError.None))
	{ 
		%>
		<div class="blog-content">
		<div class="blog-note-box blog-note-error">
	    <% foreach (string errorMsg in GetErrorMessages()){%>
	        <div class="blog-note-box-text"><%= errorMsg%></div>
	    <%} %>
		</div>	
		</div>
		<% 
		return;
	}
%>
<div class="blog-content">
<div class="blog-comments" id="forumCommentBlockItems">
	<%if (Component.Paging.IsTopPosition) {%>
	<div class="blog-navigation-box blog-navigation-top">
		<div class="blog-page-navigation">
			<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="blog-" />
		</div>
	</div>
	<%} %>
	<% int postCount = Component.PostCount; float padding = 2.5f; %>
	<% if (postCount > 1 && Component.Operation == ForumCommentBlockComponent.PostOperation.Add && Component.CanReplyToTopic && (!Component.IsTopicClosed || Component.CanOpenCloseTopic)) { %>
	<div class="blog-add-comment blog-add-comment-top">
		<a href="" onclick="Bitrix.ForumCommentBlockTemplate.reply2Author('<%= ClientID %>', '<%= ClientID + ClientIDSeparator %>TopItemFormContainer', '<%= JSEncode(Component.TopicAuthorName) %>', <%=(Component.IsBbCodeAllowed ? "true" : "false") %>, '0', true); return false;"><span><%= GetMessage("UrlTitle.Reply") %></span></a>
	</div>
	<div id="<%= ClientID + ClientIDSeparator %>TopItemFormContainer" class="blog-form-container blog-form-container-top"></div>
	<% } %>	
	<% if (postCount > 0) {%>
	<% for (int i = 0; i < postCount; i++) {%>
	    <% ForumCommentBlockComponent.PostItem post = Component.Posts[i]; %>
	    <% bool postEditing = Component.Operation == ForumCommentBlockComponent.PostOperation.Edit && Component.PostId == post.PostId; %>
	    <div class="<%= i > 0 ? "blog-comment" : "blog-comment blog-comment-first" %>" id="<%= post.PostBookmark %>" <% if ( Component.UseTreeComments ) { %> style="padding-left: <%= (post.Level * padding).ToString(".##", System.Globalization.NumberFormatInfo.InvariantInfo) %>em;" <%} %>>
	        <div class="blog-comment-cont">
	            <div class="blog-comment-cont-white">
	            <% if (!Component.UseTreeComments || post.IsApproved)
				{ %>

	                <div class="blog-comment-info">
						<% if (post.HasAvatar)
		 { %>
						<div class="blog-comment-avatar" width="<%= post.Avatar.Width.ToString() %>" height="<%= post.Avatar.Height.ToString() %>" style="background:url('<%= HttpUtility.HtmlAttributeEncode(post.Avatar.FilePath) %>') no-repeat center center;"></div>
						<% }
		 else if (post.HasAuthor)
		 {%>
						<div class="blog-comment-register-avatar"></div>
						<% }
		 else
		 { %>
						<div class="blog-comment-guest-avatar"></div>
						<%} %>	
						
						<div class="blog-author">
						<% if (!string.IsNullOrEmpty(post.UserProfileHref))
		 { %>
							<a class="blog-author-icon" href="<%= HttpUtility.HtmlAttributeEncode(post.UserProfileHref) %>"></a><a href="<%= HttpUtility.HtmlAttributeEncode(post.UserProfileHref) %>"><%= post.AuthorNameHtml%></a>
						<% }
		 else
		 { %>
							<span class="blog-author-icon" ></span><%= post.AuthorNameHtml%>
							<% } %>
						</div>
						<div class="blog-comment-date" ><%= post.DateOfCreationString%></div>
						<% if (Component.CanViewIP)
		 { %>
						<div class="blog-comment-author-ip">
							<a href="http://whois.domaintools.com/<%= UrlEncode(post.AuthorIP) %>"><%= Encode(post.AuthorIP)%></a>
						</div>
						<% } %>
						<div class="blog-comment-number">
						    <a rel="nofollow" title="<%= GetMessage("ToolTip.PermanentLink") %>" href="<%= HttpUtility.HtmlAttributeEncode(post.PostPermanentHref) %>" onclick="prompt('<%= GetMessage("Dialog.CopyThisLinkToClipboard") %>', this.href); return false;">#<%= post.Number.ToString()%></a>
						</div>												                    
	                </div>
	                <div class="blog-comment-content">
						<div class="blog-comment-avatar">
							<% if (post.HasAvatar)
		  { %>
							<img alt="" src="<%= HttpUtility.HtmlAttributeEncode(post.Avatar.FilePath) %>"  border="0" width="<%= post.Avatar.Width.ToString() %>"  height="<%= post.Avatar.Height.ToString() %>" />
							<% } %>
						</div>
						<div id="forumCommentBlockItemContent<%= post.PostId %>" class="<%= postEditing ? "blog-comment-content-editing-text" : "blog-comment-content-text"  %>"><%= post.ContentHtml%></div>
						<% if (!string.IsNullOrEmpty(post.AuthorSignatureHtml))
		 { %>
						<div class="blog-comment-user-signature">
							<div class="blog-comment-user-signature-line"></div>
							<span class="blog-comment-user-signature-content"><%= post.AuthorSignatureHtml%></span>
						</div>
						<% } %>							                    
	                </div>
	                <% int actionCount = 0; %>
	                <% if (!postEditing)
					{%>
	                <div class="blog-comment-meta">
					    <% if (Component.Authorization.CanApprove && !Component.UseTreeComments)
							{ %>
						    <% if (post.IsApproved)
								{ %>
						    <span class="blog-comment-action-hide"><a href="<%= HttpUtility.HtmlAttributeEncode(post.DisapproveHyperlink) %>" rel="nofollow"><%= GetMessage(  "UrlTitle.HidePost")%></a></span>
						    <% }
							 else
								{ %>
						    <span class="blog-comment-action-approve"><a href="<%= HttpUtility.HtmlAttributeEncode(post.ApproveHyperlink) %>" rel="nofollow"><%= GetMessage("UrlTitle.ApprovePost")%></a></span>
						    <% } %>
						    <% actionCount++; %>						    
					    <% } %>
						<% if (post.IsDeleteable)
						{ %>
						<% if (actionCount > 0)
						{%>
						<span class="blog-vert-separator">|</span> 
						<%} %>
						<span class="blog-comment-action-delete"><a href="<%= HttpUtility.HtmlAttributeEncode(post.DeleteHyperlink) %>" onclick="return Bitrix.ForumCommentBlockTemplate.confirmPostDeletion();" rel="nofollow"><%= GetMessage("UrlTitle.DeletePost")%></a></span>
						<% actionCount++; %>
						<% } %>
					    <% if (post.IsEditable)
						{ %>
						<% if (actionCount > 0)
						{%>
						<span class="blog-vert-separator">|</span> 
						<%} %>					    
					    <span class="blog-comment-action-edit"><a href="<%= HttpUtility.HtmlAttributeEncode(post.EditHyperlink) %>" rel="nofollow"><%= GetMessage("UrlTitle.EditPost")%></a></span>
					    <% actionCount++; %>
					    <% } %>	
						<% if (Component.CanReplyToTopic)
						{ %>
						<% if (actionCount > 0)
						{%>
						<span class="blog-vert-separator">|</span> 
						<%} %>							
						<span class="blog-comment-action-quote"><a href="" rel="nofollow"  onclick="Bitrix.ForumCommentBlockTemplate.quote('<%= ClientID %>', '<%= ClientID + ClientIDSeparator + post.PostId.ToString() + ClientIDSeparator %>ItemFormContainer', 'forumCommentBlockItemContent<%= post.PostId %>', '<%= JSEncode(post.AuthorName) %>', <%=(Component.IsBbCodeAllowed ? "true" : "false") %>, <%= post.PostId %>); return false;"><%= GetMessage("UrlTitle.QuotePost")%></a></span>
						<span class="blog-vert-separator">|</span>
						<span class="blog-comment-action-reply"><a href="#postform" rel="nofollow" onclick="Bitrix.ForumCommentBlockTemplate.reply2Author('<%= ClientID %>', '<%= ClientID + ClientIDSeparator + post.PostId.ToString() + ClientIDSeparator %>ItemFormContainer', '<%= JSEncode(post.AuthorName) %>', <%=(Component.IsBbCodeAllowed ? "true" : "false") %>, <%= post.PostId %>, false); return false;" title="<%= GetMessage("UrlTitle.Reply2Author.Title")%>"><%= GetMessage("UrlTitle.Reply2Author")%></a></span>
						<% actionCount += 2; %>
						<% } %>		
		                    
	                </div>	
	                <% }%>  
	                				    <%}else{%>
	       <div class="blog-comment-content"><%=GetMessage("CommentDeleted") %></div>
	    <%}%>      	                	            	            		            
	            </div>	        
	        </div>

	        <div class="blog-comment-clear-float"></div>
	    </div>
	    <div id="<%= GetPostFormContainerClientID(post.PostId) %>" class="blog-form-container"></div>

<%} %>
	<% } %>
	<%if (Component.Paging.IsBottomPosition) { %>
	<div class="blog-navigation-box blog-navigation-bottom">
		<div class="blog-page-navigation">
			<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="blog-" />
		</div>
	</div>
	<%} %>
	
	
	<% if (Component.Operation == ForumCommentBlockComponent.PostOperation.Add && Component.CanReplyToTopic && (!Component.IsTopicClosed || Component.CanOpenCloseTopic)) { %>
	<div class="blog-add-comment blog-add-comment-bottom">
		<a href="" onclick="Bitrix.ForumCommentBlockTemplate.reply2Author('<%= ClientID %>', '<%= ClientID + ClientIDSeparator %>BottomItemFormContainer', '<%= JSEncode(Component.TopicAuthorName) %>', <%=(Component.IsBbCodeAllowed ? "true" : "false") %>, '0', true); return false;"><span><%= GetMessage("UrlTitle.Reply") %></span></a>
	</div>
	<div id="<%= ClientID + ClientIDSeparator %>BottomItemFormContainer" class="blog-form-container blog-form-container-bottom"></div>
	<% } %>	
</div>
</div>
<div class="blog-form" id="<%= PostFormContainerClientID %>" style="display:none;" >
<asp:HiddenField runat="server" ID="PostFormPlaceholderData" Value="" />
<asp:HiddenField runat="server" ID="ParentComment" Value="0" />
<% if (PreviewPost && !string.IsNullOrEmpty(Content.Text)) {%>
<div class="blog-comment-preview" id="<%= PostPreviewContainerClientID %>" >
    <div class="blog-form-header-box">
	    <div class="blog-comment-cont"><span><%= GetMessage("Label.Preview")%></span></div>
    </div>
    <div class="blog-form-header-info-box blog-post-preview">
	    <div class="blog-form-header-info-box-inner"><% PreparePreview(Content.Text, CurrentWriter); %></div>
    </div>
</div>
<% } %>

<% 
    Errors.HeaderText = string.Format(@"<div class=""blog-note-box-text"">{0}</div>", GetMessage("Error.Title"));
    foreach (string s in Component.ComponentPostDataValidationErrors)
			Errors.AddErrorMessage(Encode(s)); 
%>
	<bx:BXValidationSummary runat="server" ID="Errors" ValidationGroup="bxForumCommentBlock" CssClass="blog-note-box blog-note-error" ForeColor="" />

	<% if (!Component.IsAuthentificated) { %>
	<div class="blog-comment-fields">	
		<div class="blog-field-user">
			<div class="blog-field blog-field-author">
				<label for="<%= GuestName.ClientID %>"><%= GetMessage("Label.GuestName") %><span class="blog-required-field">*</span></label>
				<span><asp:TextBox runat="server" ID="GuestName" MaxLength="255" TabIndex="3" Columns="30" /></span>
				<asp:RequiredFieldValidator runat="server" ID="GuestNameValidator" ErrorMessage="<%$ Loc:Error.GuestNameRequired %>" ControlToValidate="GuestName" Display="None" SetFocusOnError="true" ValidationGroup="bxForumCommentBlock" />
			</div>
			
			<div class="blog-field-user-sep">&nbsp;</div>
			
			<% if (Component.ShowGuestEmail) { %>
			<div class="blog-field blog-field-email">
				<label for="<%= GuestEmail.ClientID %>">e-mail<% if (Component.RequireGuestEmail) { %><span class="forum-required-field">*</span><% } %></label>
				<span><asp:TextBox runat="server" ID="GuestEmail" MaxLength="255" TabIndex="4" Columns="30" /></span>
				<asp:RequiredFieldValidator runat="server" ID="GuestEmailValidator" ErrorMessage="<%$ Loc:Error.GuestEmailRequired %>" ControlToValidate="GuestEmail" Display="None" SetFocusOnError="true" ValidationGroup="bxForumCommentBlock" />
			</div>
			<% } %>
			
			<div class="blog-clear-float">
			</div>
		</div>
	</div>
	<% } %>
	
	<div class="blog-add-item-header"><%= GetMessage("Label.PostContent") %><span class="blog-required-field">*</span></div>
	<div class="blog-comment-fields">
		<% if (Component.IsBbCodeAllowed) { %>
		<div class="blog-comment-field blog-comment-field-bbcode">
			<bx:BBCodeLine runat="server" ID="BBCode" TextControl="Content" TagList="b,u,i,s,url,quote,code,list,color,img,video,audio" CssClass="bbcode-line"/>
			<div class="blog-clear-float"></div>
		</div>
		<% } %>

		<div class="blog-comment-field blog-comment-field-content">
			<asp:TextBox runat="server" ID="Content" TextMode="MultiLine" Columns="50" Rows="10" TabIndex="6" />
			<asp:RequiredFieldValidator runat="server" ID="ContentValidator" ErrorMessage="<%$ Loc:Error.PostContentRequired %>" ControlToValidate="Content" Display="None" SetFocusOnError="true" ValidationGroup="bxForumCommentBlock" />
		</div>
		
		<% if (!Component.IsAuthentificated && Component.ShowGuestCaptcha) { %>
		<div class="blog-field blog-field-captcha">
			<div class="blog-field-captcha-label">
				<% GuestCaptchaGuid.Value = Component.CaptchaGuid; %>
				<% GuestCaptcha.Text = string.Empty; %>
				<label for="<%= GuestCaptcha.ClientID %>"><%= GetMessage("Label.GuestCaptcha") %><span class="blog-required-field">*</span></label>
				<asp:TextBox runat="server" ID="GuestCaptcha" MaxLength="10" TabIndex="7" Columns="30" />
				<asp:RequiredFieldValidator runat="server" ID="GuestCaptchaValidator" ErrorMessage="<%$ Loc:Error.GuestCaptchaRequired %>" ControlToValidate="GuestCaptcha" Display="None" SetFocusOnError="true" ValidationGroup="bxForumCommentBlock" />
				<asp:HiddenField ID="GuestCaptchaGuid" runat="server" />
			</div>
			
			<div class="blog-field-captcha-image">
				<img src="<%= Component.CaptchaHref %>" width="180" height="50" alt="<%= GetMessage("Label.GuestCaptcha") %>" />
			</div>
		</div>
		<% } %>
		
		<% if (Component.CanApprove) { %>
		<div class="blog-field blog-field-settings">
			<div class="blog-field-setting"><asp:CheckBox runat="server" ID="Hide" TabIndex="8" Text="<%$ LocRaw:CheckBoxTitle.HidePostAfterCreation %>" />
			
			</div>
		</div>
		<% } %>
		<div class="blog-comment-buttons">
			<asp:Button runat="server" ID="Save" TabIndex="11" Text="<%$ LocRaw:ButtonText.Save %>" OnClick="SavePostClick" ValidationGroup="bxForumCommentBlock" /> 
			<asp:Button runat="server" ID="Preview" TabIndex="12" Text="<%$ LocRaw:ButtonText.Preview %>" OnClick="PreviewPostClick"  CausesValidation="false" />
			<% if (Component.Operation == ForumCommentBlockComponent.PostOperation.Edit && Component.PostId > 0) { %>
		    <asp:Button runat="server" ID="Cancel" TabIndex="13" Text="<%$ LocRaw:ButtonText.Cancel %>"  CausesValidation="false" />
		    <% } %>
		</div>
	</div>
</div>    
<script type="text/javascript">
if(typeof(Bitrix) == "undefined") {
	var Bitrix = new Object();
}
if(typeof(Bitrix.ForumCommentFormData) == "undefined") {
    Bitrix.ForumCommentFormMode = {
        add: 1,
        edit: 2
    }
    
    Bitrix.ForumCommentFormData = function(formId, containerId) {
        this.formId = Bitrix.TypeUtility.isNotEmptyString(formId) ? formId : "";
        this.containerId = Bitrix.TypeUtility.isNotEmptyString(containerId) ? containerId : "";
        this.mode = Bitrix.ForumCommentFormMode.add;
        this.parentCommentId = 0;
        
    }
    Bitrix.ForumCommentFormData.prototype = {
        toJSON: function() { return "{\"mode\":" + this.mode + ", \"formId\":\"" + this.formId + "\", \"containerId\":\"" + this.containerId + "\", \"parentCommentId\":" + this.parentCommentId + "}"; }
    }
    Bitrix.ForumCommentFormData.fromJSON = function(s) {
        var self = new Bitrix.ForumCommentFormData();
        if(Bitrix.TypeUtility.isNotEmptyString(s)) {
            try {
                var obj = eval("(" + s + ")");
                if(typeof(obj) == "object") {
                    if("formId" in obj) self.formId = obj.formId;
                    if("containerId" in obj) self.containerId = obj.containerId;
                    if("mode" in obj) self.mode = obj.mode;
                    if("parentCommentId" in obj) self.parentCommentId = obj.parentCommentId;
                }
            }
            catch(e) {}
        }
        return self;
    }
}
if(typeof(Bitrix.ForumCommentBlockTemplate) == "undefined") {
    Bitrix.ForumCommentBlockTemplate = function() {
        this._initialized = false;
        this._id = "";
        this._formId = "";
        this._formInputId = "";
		this._formParentCommentId = "";
        this._formDataId = "";
        this._previewId = "";
        this._containerId = "";
        this._mode = Bitrix.ForumCommentFormMode.add;
    }
    
    Bitrix.ForumCommentBlockTemplate.prototype = {
        initialize: function(id, formId, formInputId, formParentCommentId, formDataId, previewId, mode) {
            if(this._initialized) return;
            if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ForumCommentBlockTemplate: id is not defined.";
            this._id = id; 
            this.setFormId(formId);
            this.setFormInputId(formInputId);
			this.setParentCommentId(formParentCommentId);
			this.setParentComment("0");
            this.setFormDataId(formDataId);
            this.setPreviewId(previewId);
            this.setMode(mode);
            this._initialized = true;
        },
        isInitialized: function() { return this._initialized; },
        getId: function() { return this._id; },    
        getFormId: function() { return this._formId; },
        setFormId: function(id) { this._formId = Bitrix.TypeUtility.isNotEmptyString(id) ? id : ""; },
        getFormEl: function() { return Bitrix.TypeUtility.isNotEmptyString(this._formId) ? document.getElementById(this._formId) : null; },        
        getFormInputId: function() { return this._formInputId; },
        setFormInputId: function(id) { this._formInputId = Bitrix.TypeUtility.isNotEmptyString(id) ? id : ""; },
        getFormInputEl: function() { return Bitrix.TypeUtility.isNotEmptyString(this._formInputId) ? document.getElementById(this._formInputId) : null; },        
        getFormDataId: function() { return this._formDataId; },
        setFormDataId: function(id) { this._formDataId = Bitrix.TypeUtility.isNotEmptyString(id) ? id : ""; },                   
        getFormDataEl: function() { return Bitrix.TypeUtility.isNotEmptyString(this._formDataId) ? document.getElementById(this._formDataId) : null; },        
        getPreviewId: function() { return this._previewId; },
        setPreviewId: function(id) { this._previewId = Bitrix.TypeUtility.isNotEmptyString(id) ? id : ""; },        
        getPreviewEl: function() { return Bitrix.TypeUtility.isNotEmptyString(this._previewId) ? document.getElementById(this._previewId) : null; },        
        getContainerId: function() { return this._containerId; },
        setContainerId: function(id) { this._containerId = Bitrix.TypeUtility.isNotEmptyString(id) ? id : ""; },
        getMode: function() { this._mode; },
        setMode: function(mode) { this._mode = mode; },
		getParentCommentId: function() { return this._formParentCommentId; },
		setParentCommentId: function(id) { this._formParentCommentId = Bitrix.TypeUtility.isNotEmptyString(id) ? id : ""; },
        showPreview: function(show) {
            var preview = this.getPreviewEl();
            if(preview) preview.style.display = show ? "" : "none";
        },
	    layout: function() {
	        var formEl = Bitrix.TypeUtility.isNotEmptyString(this.getFormId()) ? document.getElementById(this.getFormId()) : null;
	        if(!formEl) return;
	        
	        var formDataEl = this.getFormDataEl();
	        if(!(formDataEl && Bitrix.TypeUtility.isNotEmptyString(formDataEl.value))) return;
	         	        
	        var data = Bitrix.ForumCommentFormData.fromJSON(formDataEl.value);
	        this.setForumPostFormContainer(data.containerId);
	        this.setParentComment(data.parentCommentId);
	        if(formEl.style.display == "none") formEl.style.display = "";
	        if(typeof(formEl.scrollIntoView) != "undefined") formEl.scrollIntoView();
	    },
        setForumPostFormContainer: function(containerId) {
            if(!Bitrix.TypeUtility.isNotEmptyString(containerId)) return;
            var containerEl = null, formEl = null, formDataEl = null;
            if(!((containerEl = document.getElementById(containerId)) && (formEl = this.getFormEl()))) return;
            containerEl.appendChild(formEl);
            this._containerId = containerId;
	        if(typeof(formEl.scrollIntoView) != "undefined") formEl.scrollIntoView();
	        if(formEl.style.display == "none") formEl.style.display = "";          
            if(formDataEl = this.getFormDataEl()) formDataEl.value = (new Bitrix.ForumCommentFormData(this._formId, this._containerId)).toJSON();            
        },
        setForumPostFormContent: function(text, overwrite, setFocus) {
            var formInputEl = this.getFormInputEl();
            if(!formInputEl) return; 
            if(overwrite) formInputEl.value = text;
            else {
                if(formInputEl.value.length > 0)
                    formInputEl.value += "\n";        
                formInputEl.value += text;
            }
            if(setFocus) formInputEl.focus(); 
        },
		setParentComment: function(id) {
			var parentCommentEl = document.getElementById(this._formParentCommentId);
			if(!parentCommentEl) return;
			if(Bitrix.TypeUtility.isNotEmptyString(id)) {
				val = parseInt(id);
				parentCommentEl.value = !val.isNaN ? id : "0";
			}
			else if(Bitrix.TypeUtility.isNumber(id)) {
				parentCommentEl.value = !id.isNaN ? id.toString() : "0";
			}
			else
				parentCommentEl.value = "0";
		},
		hasParentComment: function() {
			var parentCommentEl = document.getElementById(this._formParentCommentId);
			return parentCommentEl != null && Bitrix.TypeUtility.isNotEmptyString(parentCommentEl.value) && parentCommentEl.value != "0";
		},
        reply2Author: function(containerId, authorName, enableBBCode, parentPostId, overwrite) {
            if(this._mode == Bitrix.ForumCommentFormMode.add) this.setForumPostFormContainer(containerId);
	        this.setForumPostFormContent(Bitrix.TypeUtility.isNotEmptyString(authorName) ? enableBBCode ? ("[b]" + authorName + "[/b], ") : (authorName + ", ") : "", overwrite, true);
	        
	        this.showPreview(false);
			if(this._mode == Bitrix.ForumCommentFormMode.add && !this.hasParentComment()) this.setParentComment(parentPostId);
	    },
        _prepareCodeBlockForQuote: function(str, codeBlock) {
            if(typeof(Bitrix.ForumQuotationProcessors) == "undefined") return "[code]\r\n" + codeBlock + "[/code]";
            var r = null;
            for(var p in Bitrix.ForumQuotationProcessors)
                if((r = Bitrix.ForumQuotationProcessors[p](codeBlock))!= null) 
                    break;        
            return "[code]\r\n" + (r ? r : codeBlock) + "[/code]";
        },	
        quote: function(containerId, contentContainerId, authorName, enableBBCode, parentPostId) {
            var text = enableBBCode ? "[quote]" : "";
            if(authorName)
                text += authorName + ":\n";
                
            if(!enableBBCode)
		        text += "===========================\n";
            
            var selectedText = Bitrix.DocumentSelection.create().getSelectedText();
            if(selectedText.length == 0){
                var messageId = parseInt(contentContainerId.replace(/forumCommentBlockItemContent/gi, ""));
                if(isNaN(messageId)) return;
                
                var messageContainer = document.getElementById(contentContainerId);
                if(!messageContainer) return;

                var messageContainerHtml = messageContainer.innerHTML; 
		        messageContainerHtml = messageContainerHtml.replace(/\<br(\s)*(\/)*\>[\r\n]*/gi, "\r\n");
    			
		        messageContainerHtml = messageContainerHtml.replace(/\<(\/?)(b|i|s|u)\>/gi, "[$1$2]");
    			
		        messageContainerHtml = messageContainerHtml.replace(/\<script[^\>]*>[\r\n]*/gi, "\001").replace(/\<\/script[^\>]*>[\r\n]*/gi, "\002").replace(/\001([^\002]*)\002/gi, "");
		        messageContainerHtml = messageContainerHtml.replace(/\<noscript[^\>]*>[\r\n]*/gi, "\001").replace(/\<\/noscript[^\>]*>[\r\n]*/gi, "\002").replace(/\001([^\002]*)\002/gi, "");
		        // Quote & Code
		        messageContainerHtml = messageContainerHtml.replace(/\<table\s*class\s*\=\s*(\"|\')?forum-quote(\"|\')?([^>]*)\>\s*\<tbody\>\s*(\<tr\>\s*\<th\>\s*([^<]*)\s*\<\/th\>\s*\<\/tr\>\s*)?\s*\<tr\>\s*\<td\>/gi, "\001");
		        messageContainerHtml = messageContainerHtml.replace(/\<\/td\>\<\/tr\>\<\/tbody\>\<\/table\>/gi, "\003");                  
		        var i = 0;
		        while(i < 50 && (messageContainerHtml.search(/\002([^\002\003]*)\003/gi) >= 0 || messageContainerHtml.search(/\001([^\001\003]*)\003/gi) >= 0))
		        {
			        i++;
			        messageContainerHtml = messageContainerHtml.replace(/\001([^\001\003]*)\003/gi, "[quote]$1[/quote]");				
		        }
		        messageContainerHtml = messageContainerHtml.replace(/[\001\002\003]/gi, "");
		        messageContainerHtml = messageContainerHtml.replace(/\<div[^\>]*class\s*=\s*(?:\"|\')?forum-code-box(?:\"|\')?[^\>]*\>([\w\W]*?)\<\/div\><!--ForumCodeBoxEnd-->/ig, this._prepareCodeBlockForQuote);				
    			
		        // Hrefs 
		        messageContainerHtml = messageContainerHtml.replace(/\<a[^>]+href=[\"]([^\"]+)\"[^>]+\>([^<]+)\<\/a\>/gi, "[url=$1]$2[/url]");
		        messageContainerHtml = messageContainerHtml.replace(/\<a[^>]+href=[\']([^\']+)\'[^>]+\>([^<]+)\<\/a\>/gi, "[url=$1]$2[/url]");
    			
                messageContainerHtml = messageContainerHtml.replace(/\<[^\>]+\>/gi, "");        
                selectedText = Bitrix.HttpUtility.htmlDecode(messageContainerHtml);
                this.showPreview(false);
                if(this._mode == Bitrix.ForumCommentFormMode.add && !this.hasParentComment()) this.setParentComment(parentPostId);
            }
        
            if(selectedText.length == 0)
                return true;
                
            text += selectedText;
            text += enableBBCode ? "[/quote]" : "\n===========================\n";
            if(this._mode == Bitrix.ForumCommentFormMode.add) this.setForumPostFormContainer(containerId);
	        this.setForumPostFormContent(text, false, true);        
        }	                      	                    
    }
    
    Bitrix.ForumCommentBlockTemplate._items = null;
    Bitrix.ForumCommentBlockTemplate.create = function(id, formId, formInputId, formParentCommentId, formDataId, previewId, mode) {
        if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ForumCommentBlockTemplate: id is not defined.";
        if(this.getInstance(id)) throw "Bitrix.ForumCommentBlockTemplate: item '" + id + "' already exists.";
        var self = new Bitrix.ForumCommentBlockTemplate();
        self.initialize(id, formId, formInputId, formParentCommentId, formDataId, previewId, mode);
        (this._items ? this._items : (this._items = {}))[id] = self;
        return self;
    }
    Bitrix.ForumCommentBlockTemplate.getInstance = function(id) {
        if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ForumCommentBlockTemplate: id is not defined.";
        return this._items && id in this._items && this._items[id];
    }
    Bitrix.ForumCommentBlockTemplate.reply2Author = function(id, containerId, authorName, enableBBCode, parentPostId, overwrite) {
        var self = this.getInstance(id);
        if(self) self.reply2Author(containerId, authorName, enableBBCode, parentPostId, overwrite);
    }
    Bitrix.ForumCommentBlockTemplate.quote = function(id, containerId, contentContainerId, authorName, enableBBCode, parentPostId) {
        var self = this.getInstance(id);
        if(self) self.quote(containerId, contentContainerId, authorName, enableBBCode, parentPostId);    
    }
    Bitrix.ForumCommentBlockTemplate.confirmPostDeletion = function() { return window.confirm('<%= GetMessageJS("ConfirmOfPostDeletion") %>'); }     	
}
</script>


