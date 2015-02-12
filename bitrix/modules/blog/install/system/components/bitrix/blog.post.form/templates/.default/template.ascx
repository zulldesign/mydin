<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.post.form/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPostFormTemplate" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Register TagPrefix="bx" Assembly="CommunicationUtility"  Namespace="Bitrix.CommunicationUtility" %>
<% 
	if (Component.FatalError != BlogPostFormComponent.ErrorCode.None) 
	{ 
%>
	<div class="blog-content">
		<div class="blog-note-box blog-note-error">
			<div class="blog-note-box-text"><%= Component.GetErrorHtml(Component.FatalError) %></div>
		</div>
	</div>
<%
		return;
	} 

	Errors.HeaderText = string.Format(@"<div class=""blog-note-box-text"">{0}</div>", GetMessage("ErrorTitle"));
	foreach (string message in Component.ValidationErrors)
		Errors.AddErrorMessage(message);
%>
<bx:BXValidationSummary runat="server" ID="Errors" CssClass="blog-note-box blog-note-error" ForeColor="" /> 

<div class="blog-content">

<% 
	if (preview) 
	{
		BXUser user = BXIdentity.Current.User;
		StringBuilder postClasses = new StringBuilder();
		if (Component.Post != null && (Component.Post.Flags & BXBlogPostFlags.FromSyndication) != BXBlogPostFlags.None)
			postClasses.Append(" blog-post-from-syndication");
		DateTime date = Component.Data.DatePublished;
		postClasses.Append(" blog-post-year-");
		postClasses.Append(date.Year);
		postClasses.Append(" blog-post-month-");
		postClasses.Append(date.Month);
		postClasses.Append(" blog-post-day-");
		postClasses.Append(date.Day);
%>
	<div class="blog-post blog-post-detail<%= postClasses %>">
		<h2 class="blog-post-title"><span><%= BXWordBreakingProcessor.Break(PostTitle.Text, Component.MaxWordLength, true)%></span></h2>
		<div class="blog-post-info-back blog-post-info-top">
			<div class="blog-post-info">
				<% if (user != null) { %>
				<div class="blog-author"><a class="blog-author-icon" href="#"></a><a href="#"><%= Encode(user.GetDisplayName())%></a></div>
				<%} %>
				<div class="blog-post-date" title="<%= Encode(Component.Data.DatePublished.ToString("g"))%>">
					<span class="blog-post-day"><%= Encode(Component.Data.DatePublished.ToString("d")) %></span> <span class="blog-post-time"><%= Encode(Component.Data.DatePublished.ToString("t")) %></span>
				</div>
			</div>
		</div>
		<div class="blog-post-content">
			<% if (user != null && user.Image != null) { %>
			<div class="blog-post-avatar"><img alt="" width="<%= user.Image.Width %>px" height="<%= user.Image.Height %>px" src="<%= user.Image.FilePath %>" /></div>
			<% } %>
			<% 
				if (IsCompatible(currentMode, originalMode) || currentMode == BXBlogPostContentType.BBCode && Override.Checked) 
				{
					%><%= Component.Preview(CurrentContent.Text, currentMode) %><%
				}
				else 
				{
					%><%= Component.Preview(Component.Data.Content, Component.Data.ContentType) %><%
				}
			%>
			<div class="blog-clear-float"></div>
		</div>
		<div class="blog-post-meta">
			<div class="blog-post-info-back blog-post-info-bottom">
				<div class="blog-post-info">
					<% if (user != null) { %>
					<div class="blog-author"><a class="blog-author-icon" href="#"></a><a href="#"><%= Encode(user.GetDisplayName())%></a></div>
					<% } %>
					<div class="blog-post-date" title="<%= Encode(Component.Data.DatePublished.ToString("g"))%>">
						<span class="blog-post-day"><%= Encode(Component.Data.DatePublished.ToString("d")) %></span> <span class="blog-post-time"><%= Encode(Component.Data.DatePublished.ToString("t")) %></span>
					</div>
				</div>
			</div>
		</div>
	</div>
<%
	}
%>

	<div class="blog-edit-form blog-edit-post-form">

		<div class="blog-edit-form-title">
		<%= (Component.Data.IsNew ? GetMessage("FormTitle.CreatePost") : GetMessage("FormTitle.EditPost")) %>
		</div>
		<div class="blog-edit-fields">	
			<%
				if (!IsPostBack)
				{					
					PostTitle.Text = Component.Data.Title;
					CurrentContent.Text = IsCompatible(currentMode, originalMode) ? Component.Data.Content : "";
					DatePublished.Text = Component.Data.DatePublished.ToString();
					Tags.Text = Component.Data.Tags;
					EnableComments.Checked = Component.Data.EnableComments;
					HtmlContent.ViewMode = 
						(Component.Data.Flags & BXBlogPostFlags.EditSourceCode) != BXBlogPostFlags.None
						? BXWebEditorLiteViewMode.SourceCode
						: BXWebEditorLiteViewMode.Wysiwyg;
										

					if (string.IsNullOrEmpty(PostTitle.Text))
						PostTitle.Text = GetMessageRaw("Title");
				}
			%>
			<%
				PostTitle.Attributes.Add("onfocus", string.Format("if (this.value=='{0}') {{this.value=''; this.className='blog-editable-field'}}", JSEncode(GetMessageRaw("Title"))));
				PostTitle.Attributes.Add("onblur", string.Format("if (this.value=='') {{this.value='{0}'; this.className=''}};", JSEncode(GetMessageRaw("Title"))));
				if (PostTitle.Text != JSEncode(GetMessageRaw("Title")))
					PostTitle.CssClass = "blog-editable-field";		
			%>
			<div class="blog-edit-field blog-edit-field-title">
				<asp:TextBox ID="PostTitle" runat="server" TabIndex="1"/>
				<asp:RequiredFieldValidator ID="PostTitleValidator" runat="server" ControlToValidate="PostTitle" Display="None" />
			</div>

			<div class="blog-edit-field blog-edit-field-post-date">
				<script type="text/javascript">
					window.document.writeln('<a href="" onclick="document.getElementById(\'<%= JSEncode(Encode(JSEncode(DatePublished.ClientID))) %>$date\').style.display=\'block\'; this.style.display=\'none\'; return false;"><%= JSEncode(Component.Data.DatePublished.ToString()) %></a>');
					window.document.writeln('<div style="display:none;" id="<%= JSEncode(Encode(JSEncode(DatePublished.ClientID))) %>$date">');
				</script>
				<asp:TextBox ID="DatePublished" runat="server" TabIndex="2" />
				<script type="text/javascript">
					window.document.write('<%= JSEncode(BXCalendarHelper.GetMarkupByInputElement("document.getElementById('" + JSEncode(DatePublished.ClientID) + "')", true)) %>');
					window.document.writeln('</div>');
				</script>
			</div>
			
			<asp:MultiView runat="server" ID="Editor">
				<asp:View runat="server" ID="BBCodeView">
					<% if (originalMode != BXBlogPostContentType.BBCode) { %>
					<script type="text/javascript">
						function <%= ClientID %>_ShowBBCodeEditor(cb, show)
						{
							for(var div = cb.parentNode.nextSibling; div != null; div = div.nextSibling)
							{
								if (div.nodeType != 1 || div.tagName != 'DIV')
									continue;
								
								div.style.display = show ? '' : 'none';
								break;
							}
						}
					</script>
					
					<div class="blog-edit-field blog-edit-field-bbcode-override">
						<div class="blog-edit-field-bbcode-override-note blog-note-box">
						<%= GetMessageRaw("Note.HtmlNotAllowed") %>
						</div>
						<% Override.Attributes["onclick"] = ClientID + "_ShowBBCodeEditor(this, this.checked);"; %>
						<asp:CheckBox runat="server" ID="Override" Text="<%$ Loc:CheckBoxText.UseBBCode %>"/>
					</div>
					<% } %>
					
					<div class="blog-edit-editor-area blog-edit-editor-area-bbcode"<% if (originalMode != BXBlogPostContentType.BBCode && !Override.Checked) { %> style="display:none"<% } %>>
						<div class="blog-edit-field blog-edit-field-bbcode">
							<bx:BBCodeLine runat="server" ID="BBCode" TextControl="BBCodeContent" TagList="b,u,i,s,url,img,imgUpload,audio,video,quote,cut,code,list,color"/>
							<div class="blog-clear-float"></div>
						</div>

						<div class="blog-edit-field blog-edit-field-text">
							<asp:TextBox ID="BBCodeContent" runat="server" TextMode="MultiLine" Rows="20" Columns="50" TabIndex="3" />
						</div>
					</div>
				</asp:View>
				<asp:View runat="server" ID="HtmlView">
					<% if (originalMode == BXBlogPostContentType.FullHtml && currentMode == BXBlogPostContentType.FilteredHtml) { %>
					<div class="blog-edit-field blog-edit-field-bbcode-override">
						<div class="blog-edit-field-bbcode-override-note blog-note-box">
						<%= GetMessageRaw("Note.LimitedHtml") %> 
						</div>						
					</div>
					<% } %>
					<div class="blog-edit-editor-area blog-edit-editor-area-html-wysiwyg">
						<bx:BXWebEditorLite 
							runat="server" 
							ID="HtmlContent" 
							TabIndex="3"
							Width="95%"
							Height="500px"
							CssClass="blog-edit-field blog-edit-field-html-wysiwyg"
							LoadStandardDialogs="false" 
							LoadStandardStyles="false" 
							OnRequireAdditionalScripts="HtmlScripts" 
							OnRequireAdditionalStyles="HtmlStyles" 
							OnProvideInitScript="HtmlInit"
							
							ToolbarButtons="Source,Bold,Italic,Underline,Strike,RemoveFormat,CreateLink,DeleteLink,Image,ImageUpload,Video,Audio,InsertCode,InsertQuote,Justify,InsertOrderedList,InsertUnorderedList,Outdent,Indent,ForeColor,BackColor,HeaderList,InsertCut"
							
							Resizable="false"
							AutoResize="false"
						/>
					</div>
				</asp:View>
			</asp:MultiView>
			
			
			<div class="blog-edit-field blog-edit-field-tags">
				<label class="blog-edit-field-caption" for="<%= Encode(Tags.ClientID) %>"><%= GetMessage("Tags") %></label>
				<asp:TextBox ID="Tags" runat="server" Columns="50" TabIndex="4" />
			</div>
			
			<div class="blog-edit-field blog-edit-field-enable-comments">
				<asp:CheckBox runat="server" ID="EnableComments" Text=" <%$ Loc:EnableComments %>" TextAlign="Right"  />
			</div>			
			
			<% if (Component.PostCustomFieldEditors.Length > 0){  %>
		        <% foreach (var ed in Component.PostCustomFieldEditors){%>
					<div class="blog-edit-field blog-edit-custom-property-<%= ed.Field.Name.ToLowerInvariant() %>">
					<% if (string.Equals(ed.Field.CustomTypeId.ToUpperInvariant(), "BITRIX.SYSTEM.BOOLEAN", StringComparison.Ordinal) && ed.Field.Settings.GetInt("view", 0) == 0) { %>
						<span class="blog-edit-custom-property-single-line-editor"><%= ed.Render() %></span><label for="<%= Encode(ed.ClientID) %>" class="blog-edit-field-caption blog-edit-custom-property-single-line-caption"><%= ed.Caption %>
							<% if (ed.IsRequired){ %>
							   <span class="blog-required-field">*</span>
							<%} %>
						</label>		                
					<%} %>
					<%else { %>
						<label class="blog-edit-field-caption">
							<%= ed.Caption %>
							<% if (ed.IsRequired){ %>
								<span class="blog-required-field">*</span>
							<%} %>
						</label>
						<%= ed.Render() %>
					<% } %>
					</div>
				<%} %>		    
			<%} %>
			<%
				Publish.Text = (Component.Data.IsNew || !Component.Data.IsPublished) ? GetMessageRaw("ButtonText.Publish") : GetMessageRaw("ButtonText.Save");
				Store.Text = (Component.Data.IsNew || !Component.Data.IsPublished) ? GetMessageRaw("ButtonText.SaveAsDraft") : GetMessageRaw("ButtonText.ToDrafts");
				if (Component.Data.IsNew || !Component.Data.IsPublished)	
					Publish.OnClientClick = string.Format("if (!confirm('{0}')) return false;", JSEncode(GetMessageRaw("Confirm.Publish")));
			%>
			<div class="blog-edit-buttons">
				<% if (Component.Auth.CanPublishPost) { %>
				<asp:Button runat="server" ID="Publish" OnClick="Publish_Click" TabIndex="5" />
				<% } %>
				<asp:Button runat="server" ID="Store" OnClick="Store_Click" TabIndex="6" />
				<asp:Button runat="server" ID="Preview" Text="<%$ LocRaw:ButtonText.View %>" OnClick="Preview_Click" CausesValidation="false" TabIndex="7" />
                <% if (Editor.ActiveViewIndex == 1) {%>
                <script type="text/javascript">
                    (function () {
                        var publishBtn = document.getElementById('<%= Publish.ClientID %>');
                        if (publishBtn) {
                            publishBtn.setAttribute('disabled', 'disabled');
                        }

                        var storeBtn = document.getElementById('<%= Store.ClientID %>');
                        storeBtn.setAttribute('disabled', 'disabled');

                        var previewBtn = document.getElementById('<%= Preview.ClientID %>');
                        previewBtn.setAttribute('disabled', 'disabled');

                        BX.addCustomEvent(window, 'LHE_OnReady',
                            function () {
                                if (publishBtn) {
                                    publishBtn.removeAttribute('disabled');
                                }

                                storeBtn.removeAttribute('disabled');
                                previewBtn.removeAttribute('disabled');
                            });
                    })();
                </script>
                <%} %>
			</div>
		</div>
	</div>
</div>
<script runat="server">
	private bool preview;
	BXBlogPostContentType originalMode;
	BXBlogPostContentType currentMode;

	protected ITextControl CurrentContent
	{
		get
		{
			return 
				(currentMode == BXBlogPostContentType.FilteredHtml || currentMode == BXBlogPostContentType.FullHtml)  
				? (ITextControl)HtmlContent 
				: (ITextControl)BBCodeContent;
		}
	}
	
	
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		
		Errors.ValidationGroup = ClientID;
		PostTitleValidator.ValidationGroup = ClientID;
		Publish.ValidationGroup = ClientID;
		Store.ValidationGroup = ClientID;
		
		PostTitleValidator.ErrorMessage = Component.GetValidationMessageHtml(BlogPostFormComponent.ValidationMessageCode.TitleIsRequired);
			
		if (Component.FatalError != BlogPostFormComponent.ErrorCode.None) 
			return;
		
		
		bool allowHtml = Component.Auth.CanWriteFilteredHtml;
		bool allowFullHtml = Component.Auth.CanWriteFullHtml;

		if (Component.Data == null || Component.Data.IsNew)
			originalMode = allowFullHtml ? BXBlogPostContentType.FullHtml : (allowHtml ? BXBlogPostContentType.FilteredHtml : BXBlogPostContentType.BBCode);
		else
			originalMode = Component.Data.ContentType;
		
		if (IsCompatible(originalMode, BXBlogPostContentType.FullHtml) && allowFullHtml)
			currentMode = BXBlogPostContentType.FullHtml;
		else if (IsCompatible(originalMode, BXBlogPostContentType.FilteredHtml) && allowHtml)
			currentMode = BXBlogPostContentType.FilteredHtml;
		else
			currentMode = BXBlogPostContentType.BBCode;
		
		Editor.SetActiveView((currentMode == BXBlogPostContentType.FilteredHtml || currentMode == BXBlogPostContentType.FullHtml) ? HtmlView : BBCodeView);
		
		HtmlContent.UserStyles.Add(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Blog/editor.light.blog/user_styles.css"));
	}
	
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		BBCode.ContextParameters.Add("blogId", Component.Blog != null ? Component.Blog.Id.ToString() : "0");
		BBCode.ContextParameters.Add("blogSlug", Component.Blog != null ? Component.Blog.Slug : "");
		BBCode.ContextParameters.Add("postId", Component.PostId.ToString());

		BBCode.ImageHandlerPath = Bitrix.Services.BXSefUrlManager.CurrentUrl.ToString();
	}
	
	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		if (Component.FatalError != BlogPostFormComponent.ErrorCode.None)
		{
			Errors.Enabled = false;
			PostTitleValidator.Enabled = false;
			return;
		}
		BXCalendarHelper.RegisterScriptFiles();
		if (Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search"))
		{
			BXPage.Scripts.RequireUtils();
			BXPage.RegisterScriptInclude("~/bitrix/js/Main/BXCompletionPopup.js");
			BXPage.RegisterScriptInclude("~/bitrix/js/Search/utils.js");

			string popupOptions = String.Format(",popup:{{onItemChangeState:Bitrix.UI.CompletionPopup.BuildChangeState(document.getElementById(\"{0}\"))}}", Tags.ClientID);
			ScriptManager.RegisterStartupScript(
				this, 
				GetType(), 
				"TagsCompletion", 
				string.Format(
					"window.setTimeout(function(){{new Bitrix.Search.CompletionController(document.getElementById('{0}'), {{ siteId:'{1}', moduleId:'blog' {2} }});}}, 0);", 
					Tags.ClientID,
					Bitrix.Services.Js.BXJSUtility.Encode(Bitrix.BXSite.Current.TextEncoder.Decode(Bitrix.BXSite.Current.Id)),
					popupOptions
				),
				true
			);
		}
	}
	
	bool IsCompatible(BXBlogPostContentType type1, BXBlogPostContentType type2)
	{
		int type1i = type1 == BXBlogPostContentType.BBCode ? 0 : 1;
		int type2i = type2 == BXBlogPostContentType.BBCode ? 0 : 1;
		return type1i == type2i;
	}

	void Save(bool draft)
	{
		if (!Page.IsValid)
			return;

		DateTime d;
		Component.Data.Title = PostTitle.Text;
		
		Component.Data.DatePublished = DateTime.TryParse(DatePublished.Text, out d) ? d : DateTime.MinValue;
		
		Component.Data.IsPublished = Component.Auth.CanPublishPost && !draft;
		Component.Data.Tags = Tags.Text;
		Component.Data.EnableComments = EnableComments.Checked;
		
		if (IsCompatible(currentMode, originalMode) || Override.Checked)
		{
			Component.Data.Content = CurrentContent.Text;
			Component.Data.ContentType = currentMode;
			
			
			Component.Data.Flags &= ~BXBlogPostFlags.EditSourceCode; // a pattern to clear bit flag
			if ((Component.Data.ContentType == BXBlogPostContentType.FilteredHtml || Component.Data.ContentType == BXBlogPostContentType.FullHtml) && HtmlContent.ViewMode == BXWebEditorLiteViewMode.SourceCode)
				Component.Data.Flags |= BXBlogPostFlags.EditSourceCode; // a pattern to set bit flag
				
		}
		
		if (!Component.Validate())
			return;

		Component.Save();
	}

	void Publish_Click(object sender, EventArgs e)
	{
		Save(false);
	}
	void Store_Click(object sender, EventArgs e)
	{
		Save(true);
	}
	void Preview_Click(object sender, EventArgs e)
	{
		preview = true;
	}
	void HtmlScripts(object sender, BXWebEditorLiteRequireFilesEventArgs e)
	{
		e.Files.Add("~/bitrix/controls/main/media.player/wmv/silverlight.js");
		e.Files.Add("~/bitrix/controls/main/media.player/wmv/wmvplayer.js");
		e.Files.Add("~/bitrix/controls/main/media.player/js/player.js");	
	
		e.Files.Add("~/bitrix/controls/Main/dialog/js/messages.js.aspx?lang=" + HttpUtility.UrlEncode(Bitrix.Services.BXLoc.CurrentLocale));
		e.Files.Add("~/bitrix/controls/Main/dialog/js/dialog_base.js");	
		e.Files.Add("~/bitrix/controls/CommunicationUtility/editor.dialogs/messages.js.aspx?lang=" + HttpUtility.UrlEncode(Bitrix.Services.BXLoc.CurrentLocale));
		e.Files.Add("~/bitrix/controls/CommunicationUtility/editor.dialogs/dialog_link_paste.js");
		e.Files.Add("~/bitrix/controls/CommunicationUtility/editor.dialogs/dialog_code_paste.js");
		e.Files.Add("~/bitrix/controls/CommunicationUtility/editor.dialogs/dialog_image_paste.js");
		e.Files.Add("~/bitrix/controls/CommunicationUtility/editor.dialogs/dialog_audio_paste.js");	
		e.Files.Add("~/bitrix/controls/CommunicationUtility/editor.dialogs/dialog_movie_paste.js");
				
		e.Files.Add("~/bitrix/controls/Blog/editor.light.blog/messages.js.aspx?lang=" + HttpUtility.UrlEncode(Bitrix.Services.BXLoc.CurrentLocale));
		e.Files.Add("~/bitrix/controls/Blog/editor.light.blog/le_toolbarbuttons.js");
	}
	void HtmlStyles(object sender, BXWebEditorLiteRequireFilesEventArgs e)
	{	        
		e.Files.Add("~/bitrix/controls/Blog/editor.light.blog/light_editor.css");
			
		e.Files.Add("~/bitrix/controls/Main/dialog/css/dialog_base.css");
		e.Files.Add("~/bitrix/controls/CommunicationUtility/editor.dialogs/dialog_styles.css");
	}
	void HtmlInit(object sender, BXWebEditorLiteInitScriptEventArgs e)
	{   
		e.Script = string.Format(
@"
{0}.arConfig.blogImageUploadOptions = 
{{
	contextRequestParams: {{ blogId: '{2}', blogSlug: '{3}', postId: '{4}', {5}: '{6}' }},
	handlerPath: '{1}'	
}};
{0}.arConfig.blogVideoOptions = 
{{
	maxWidth: 640,
	maxHeight: 480
}};
",
			e.EditorVariable,
			Bitrix.Services.Js.BXJSUtility.Encode(Bitrix.Services.BXSefUrlManager.CurrentUrl.ToString()),
			Component.Blog != null ? Component.Blog.Id.ToString() : "0",
			Component.Blog != null ? Bitrix.Services.Js.BXJSUtility.Encode(Component.Blog.Slug) : "",
			Component.PostId.ToString(),
			BXCsrfToken.TokenKey,
			Bitrix.Services.Js.BXJSUtility.Encode(BXCsrfToken.GenerateToken())
		);
	}
</script>
