<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.edit/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogEditTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.DataTypes" %>

<script runat="server">
	private int pageId;
	private bool showUsers;
	
	protected override void LoadData(BlogEditComponent.Data data)
	{
		showUsers = data.IsTeam;
		
		BlogName.Text = data.BlogName;
		BlogSlug.Text = data.BlogSlug;
		BlogDescription.Text = data.BlogDescription;
		NotifyComments.Checked = data.NotifyComments;
		DisableBlog.Checked = !data.BlogActive;

        if (Component.AllowToAjustBlogGroups && data.Categories != null && data.Categories.Length > 0)
        {
            foreach (ListItem listItem in BlogCategories.Items)
            {
                bool selected = Array.Exists<int>(data.Categories, delegate(int id) { return String.Equals(id.ToString(), listItem.Value); });
                listItem.Selected = selected;
                if (selected && BlogCategories.SelectionMode == ListSelectionMode.Single)
                    break;
            }
        }

		if(Component.CanApproveComments)
		{
			EnableCommentModeration.Checked = data.EnableCommentModeration;
			ListItem modeItem = CommentModerationMode.Items.FindByValue(data.CommentModerationMode);	
			if(modeItem != null)
				modeItem.Selected = true;
			
			CommentModerationLinkThreshold.Text = data.CommentModerationLinkThreshold.ToString();
			StringBuilder stopListSb = new StringBuilder();
			foreach(string s in data.CommentModerationStopList) 
				stopListSb.AppendLine(s);
			
			CommentModerationStopList.Text = stopListSb.ToString();		
		}
		
        if (Component.CanSyndicateContent)
        {
            EnableSyndication.Checked = data.EnableSyndication;
            SyndicationRssFeedUrl.Text = data.SyndicationFeedUrl;
            SyndicationUpdateable.Checked = data.SyndicationUpdateableContent;
            SyndicationRedirectToComments.Checked = data.SyndicationRedirectToComments;
            if (data.EnableSyndication)
            {
                SyndicationRssFeedUrlRequired.Enabled = true;
                SyndicationRssFeedUrlRegex.Enabled = true;
            }
        }        
	}
    
	protected override void SaveData(BlogEditComponent.Data data)
	{
		data.BlogName = BlogName.Text;
		data.BlogDescription = BlogDescription.Text;
		
		if (Component.ComponentMode == BlogEditComponent.Mode.Add)
			data.BlogSlug = BlogSlug.Text;

		if (Component.ComponentMode == BlogEditComponent.Mode.Edit && Component.CanModerate && !Component.IsOwner)
			data.BlogActive = !DisableBlog.Checked;

        if (!Component.AllowToAjustBlogGroups)
        {
            if (Component.ComponentMode == BlogEditComponent.Mode.Add)
                data.Categories = Component.ObligatoryCategoryIds;
        }
        else
        {
            List<int> selectedCategories = new List<int>();
            foreach (ListItem category in BlogCategories.Items)
            {
                int categoryId;
                if (!(category.Selected && int.TryParse(category.Value, out categoryId)))
                    continue;
                selectedCategories.Add(categoryId);
            }
            data.Categories = selectedCategories.ToArray();
        }
		data.NotifyComments = NotifyComments.Checked;

		if(Component.CanApproveComments)
		{
			data.EnableCommentModeration = EnableCommentModeration.Checked;
			data.CommentModerationMode = CommentModerationMode.SelectedValue;
			int linkThreshold;
			if(!int.TryParse(CommentModerationLinkThreshold.Text, out linkThreshold))
				linkThreshold = 0;			
			
			data.CommentModerationLinkThreshold = linkThreshold;
			
			if(string.IsNullOrEmpty(CommentModerationStopList.Text))
				data.CommentModerationStopList = null;
			else
				data.CommentModerationStopList = CommentModerationStopList.Text.Replace("\r", string.Empty).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);						
			
			StringBuilder stopListSb = new StringBuilder();
			foreach(string s in data.CommentModerationStopList) 
				stopListSb.AppendLine(s);
			
			CommentModerationStopList.Text = stopListSb.ToString();		
		}		
		
        if (Component.CanSyndicateContent)
        {
            data.EnableSyndication = EnableSyndication.Checked;
            data.SyndicationFeedUrl = SyndicationRssFeedUrl.Text.TrimStart();
            data.SyndicationUpdateableContent = SyndicationUpdateable.Checked;
            data.SyndicationRedirectToComments = SyndicationRedirectToComments.Checked;
        }        
	}  
    
	protected override void OnInit(EventArgs e)
	{
		var pageIdValue = Request.Form[UniqueID + "$pageId"];
		if (string.IsNullOrEmpty(pageIdValue) || !int.TryParse(pageIdValue, out pageId) || pageId < 0)
			pageId = 0;
		
		
        if (!Component.AllowToAjustBlogGroups)
            BlogCategories.Visible = false;
        else
        {
            if (Component.MaxBlogGroups == 1)
                BlogCategories.Items.Add(new ListItem(GetMessageRaw("Option.SelectCategory"), String.Empty));
            else
            {
                BlogCategories.SelectionMode = ListSelectionMode.Multiple;
                BlogCategories.Rows = Math.Max(2, Component.SiteCategories.Count > 10 || Component.SiteCategories.Count == 0 ? 10 : Component.SiteCategories.Count);
            }

            int[] availableCategoryIds = Component.AvailableCategoryIds;
			BXFilter filter = new BXFilter(new BXFilterItem(BXBlogCategory.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, Component.DesignerSite));
			if (availableCategoryIds.Length > 0)
				filter.Add(new BXFilterItem(BXBlogCategory.Fields.Id, BXSqlFilterOperators.In, availableCategoryIds));
            BXBlogCategoryCollection availableCategories = BXBlogCategory.GetList(
                filter,
                new BXOrderBy(new BXOrderByPair(BXBlogCategory.Fields.Id, BXOrderByDirection.Asc)),
                new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlogCategory.Fields.Id, BXBlogCategory.Fields.Name),
                null,
                BXTextEncoder.EmptyTextEncoder
			);
            foreach (BXBlogCategory category in availableCategories)
                BlogCategories.Items.Add(new ListItem(category.Name, category.Id.ToString()));
        }     
		//foreach (BXBlogCategory category in Component.SiteCategories)
		//	BlogCategories.Items.Add(new ListItem(category.Name, category.Id.ToString
		
		ListItemCollection commentModerationModeItems =  CommentModerationMode.Items;
		commentModerationModeItems.Add(new ListItem(GetMessageRaw("CommentModerationMode.Filter"), "FILTER"));
		commentModerationModeItems.Add(new ListItem(GetMessageRaw("CommentModerationMode.All"), "ALL"));

		var c1 = Component.GetGroupsEditor();
		if (c1 != null)
		{
			showUsers = true;			
			c1.ID = "Groups";
			GroupsPlaceholder.Controls.Add(c1);
		}

		var c2 = Component.GetUsersEditor();
		if (c2 != null)
		{
			showUsers = true;			
			c2.ID = "Users";
			UsersPlaceholder.Controls.Add(c2);
		}

		base.OnInit(e); //important last call
	}
	
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		if (Component.FatalError == BlogEditComponent.ErrorCode.UnauthorizedNotLogIn)
		{
			Bitrix.Security.BXAuthentication.AuthenticationRequired();
			return;
		}
		else if (Component.FatalError != BlogEditComponent.ErrorCode.None)
		{
			BlogSlugEmptyValidator.Enabled = false;
			BlogSlugRegexValidator.Enabled = false;
			BlogSlugLengthValidator.Enabled = false;
			BlogNameValidator.Enabled = false;
			BlogCategoriesValidator.Enabled = false;
            BlogDescriptionValidator.Enabled = false;
            SyndicationRssFeedUrlRequired.Enabled = false;
            SyndicationRssFeedUrlRegex.Enabled = false;
			Errors.Enabled = false;
			return;
		} 
		
		string validationGroup = ClientID;
		Errors.ValidationGroup = validationGroup;
		BlogNameValidator.ValidationGroup = validationGroup;
        BlogDescriptionValidator.ValidationGroup = validationGroup;
        SyndicationRssFeedUrlRequired.ValidationGroup = validationGroup;
        SyndicationRssFeedUrlRegex.ValidationGroup = validationGroup;
        
		if (Component.ComponentMode == BlogEditComponent.Mode.Add)
		{
			BlogSlugEmptyValidator.Enabled = true;
			BlogSlugRegexValidator.Enabled = true;
			BlogSlugEmptyValidator.ValidationGroup = validationGroup;
			BlogSlugRegexValidator.ValidationGroup = validationGroup;
			BlogSlugRegexValidator.ValidationExpression = BXBlog.SlugRegex.ToString();    
			if (Component.MinSlugLength <= Component.MaxSlugLength)
			{
				BlogSlugLengthValidator.ValidationGroup = validationGroup;
				BlogSlugLengthValidator.Enabled = true;
				BlogSlugLengthValidator.ValidationExpression = String.Concat("^.{", Component.MinSlugLength, ",", Component.MaxSlugLength, "}$");
				BlogSlugLengthValidator.ErrorMessage = String.Format(GetMessage("Error.SlugLength"), Component.MinSlugLength, Component.MaxSlugLength);
				BlogSlug.MaxLength = Component.MaxSlugLength;
			}
		}

		BXPage.Scripts.RequireUtils();
		
        if (Component.AllowToAjustBlogGroups)
        {
            BlogCategoriesValidator.Enabled = true;
            BlogCategoriesValidator.ValidationGroup = validationGroup;
        }
        else
            BlogCategoriesValidator.Enabled = false;
        
        if (Component.CanSyndicateContent)
            EnableSyndication.Attributes.Add("onclick", string.Format("var feedUrlEl=document.getElementById(\"syndicationRssFeedUrlContainer\"); var updEl=document.getElementById(\"syndicationUpdateableContainer\"); var redirectEl=document.getElementById(\"syndicationRedirectToCommentsContainer\"); if(this.checked){{feedUrlEl.style.display=\"\";updEl.style.display=\"\";redirectEl.style.display=\"\";document.getElementById(\"{0}\").focus() }}else{{feedUrlEl.style.display=\"none\";updEl.style.display=\"none\";redirectEl.style.display=\"none\";}} ValidatorEnable(document.getElementById(\"{1}\"), this.checked); ValidatorEnable(document.getElementById(\"{2}\"), this.checked);", SyndicationRssFeedUrl.ClientID, SyndicationRssFeedUrlRequired.ClientID, SyndicationRssFeedUrlRegex.ClientID));
        
		Save.ValidationGroup = validationGroup;
	    Save.Text = GetMessageRaw(Component.ComponentMode == BlogEditComponent.Mode.Add ? "ButtonText.CreateBlog" : "ButtonText.EditBlog");
	}

	protected override void OnPreRender(EventArgs e)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendFormat(@"Bitrix.BlogSettingsEditor.create('{0}')", ClientID);
		if(!Component.CanApproveComments)
			sb.Append(@".prepare({ 'display':false });");
		else
		{
			sb.AppendFormat(@".prepare({{ 'display':true, 'EnableCommentModeration':'{0}', 'CommentModerationMode':'{1}', 'CommentModerationStopList':'{2}', 'EnableCommentModerationWrapper':'{3}', 'CommentModerationModeWrapper':'{4}', 'CommentModerationLinkThresholdWrapper':'{5}', 'CommentModerationStopListWrapper':'{6}' }});",
				EnableCommentModeration.ClientID,
				CommentModerationMode.ClientID,
				CommentModerationStopList.ClientID,
				GetClientID("EnableCommentModerationWrapper"),
				GetClientID("CommentModerationModeWrapper"),
				GetClientID("CommentModerationLinkThresholdWrapper"),
				GetClientID("CommentModerationStopListWrapper"));
		}

		BXPage.RegisterScriptInclude("~/bitrix/js/Main/Core/core.js");

		Page.ClientScript.RegisterStartupScript(GetType(),
			ClientID,
			sb.ToString(),
			true);
		
		base.OnPreRender(e);
	}
	
	protected new void SaveClick(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;
		
		base.SaveClick(sender, e);
	}
	
	protected new void SaveTeamClick(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;
		
		base.SaveTeamClick(sender, e);
	}
	
	protected string GetClientID(string id) 
	{
		return ClientID + ClientIDSeparator + id;
	}
</script>

<% 
	if (Component.FatalError != BlogEditComponent.ErrorCode.None)
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
%>

<div class="blog-content">
	<div class="blog-edit-form blog-edit-blog-form">

		<%
			Errors.HeaderText = string.Format(@"<div class=""blog-note-box-text"">{0}</div>", GetMessage("Error.Title"));
			foreach (string s in Component.ErrorSummary)
				Errors.AddErrorMessage(s);
		%>
		
		<bx:BXValidationSummary runat="server" ID="Errors" CssClass="blog-note-box blog-note-error" ForeColor="" /> 
		
		<% pageId = Math.Min(pageId, showUsers ? 1 : 0); %>
	
	
		<% if (showUsers) { %>
		<div class="blog-edit-form-tabs">
			<a href="" onclick="return <%= ClientID %>_SetPage(this);" <% if (pageId == 0) { %>style="border-bottom: none"<% } %> ><%= GetMessageRaw("FormTitle.BlogSettings") %></a>
			<a href="" onclick="return <%= ClientID %>_SetPage(this);" <% if (pageId == 1) { %>style="border-bottom: none"<% } %> ><%= GetMessageRaw("FormTitle.Users") %></a>
		</div>
		<% } %>
		
		<div id="<%= ClientID %>_page0" class="blog-edit-form-page-settings" <% if (pageId != 0) { %>style="display: none"<% } %>>			
			<div class="blog-edit-form-title"><%= (Component.ComponentMode == BlogEditComponent.Mode.Add ? GetMessage("FormTitle.CreateBlog") : GetMessage("FormTitle.BlogSettings")) %></div>
			
			<div class="blog-edit-fields">
			
				<div class="blog-edit-field blog-edit-field-title">
					<label for="<%= Encode(BlogName.ClientID) %>" class="blog-edit-field-caption"><%= GetMessage("Label.Name") %><span class="blog-required-field">*</span></label>
					<asp:TextBox ID="BlogName" runat="server" MaxLength="255" TabIndex="1" Columns="60"></asp:TextBox>
					<asp:RequiredFieldValidator runat="server" ID="BlogNameValidator" ErrorMessage="<%$ Loc:Message.NameNotSpecified %>" ControlToValidate="BlogName" Display="None" SetFocusOnError="true" />
				</div>
				
				<% if (Component.ComponentMode == BlogEditComponent.Mode.Add) { %>
				<div class="blog-edit-field blog-edit-field-slug">
					<label for="<%= Encode(BlogSlug.ClientID) %>" class="blog-edit-field-caption"><%= GetMessage("Label.Slug") %><span class="blog-required-field">*</span></label>
					<asp:TextBox ID="BlogSlug" TabIndex="2" Columns="60" runat="server"></asp:TextBox>
					<% if (Parameters.ContainsKey("BlogAbsoluteUrlTemplate") && !String.IsNullOrEmpty(Parameters["BlogAbsoluteUrlTemplate"])) { 
							BXParamsBag<object> replace = new BXParamsBag<object>();
							replace["BlogSlug"] = GetMessageRaw("BlogSlugDummy");
							string url = BXSefUrlUtility.MakeLink(Parameters["BlogAbsoluteUrlTemplate"], replace);
							url = new Uri(BXSefUrlManager.CurrentUrl, url).ToString();
					%>
					<div class="blog-edit-field-slug-desc"><%= String.Format(GetMessage("BlogSlug.Description"), url) %></div>
					<%} %>
					<asp:RequiredFieldValidator runat="server" Enabled="False" ID="BlogSlugEmptyValidator"	ErrorMessage="<%$ Loc:Message.SlugNotSpecified %>" ControlToValidate="BlogSlug" Display="None" SetFocusOnError="true" />
					<asp:RegularExpressionValidator runat="server" Enabled="False" ID="BlogSlugRegexValidator" ErrorMessage="<%$ Loc:Message.InvalidSlug %>" ControlToValidate="BlogSlug" Display="None" SetFocusOnError="true" />
					<asp:RegularExpressionValidator runat="server" Enabled="False" ID="BlogSlugLengthValidator" ControlToValidate="BlogSlug" Display="None" SetFocusOnError="true" />
				</div>
				<%} %>
				
				<% if (Component.AllowToAjustBlogGroups) { %>
				<div class="blog-edit-field blog-edit-field-cats">
					<label for="<%= Encode(BlogCategories.ClientID) %>" class="blog-edit-field-caption"><%= GetMessage("Label.BlogCategory") %><span class="blog-required-field">*</span></label>
					<asp:ListBox ID="BlogCategories" TabIndex="3" Rows="1" runat="server"></asp:ListBox>
					<asp:RequiredFieldValidator runat="server" ID="BlogCategoriesValidator"	ErrorMessage="<%$ Loc:Message.CategoryNotSpecified %>" ControlToValidate="BlogCategories" Display="None" SetFocusOnError="true" />
				</div>
				<%} %>

				<div class="blog-edit-field blog-edit-field-desc">
					<label for="<%= Encode(BlogDescription.ClientID) %>" class="blog-edit-field-caption"><%= GetMessage("Label.Description") %></label>
					<asp:TextBox ID="BlogDescription"  TabIndex="4" TextMode="MultiLine" Rows="7" runat="server"></asp:TextBox>	 
					<asp:RegularExpressionValidator runat="server" ID="BlogDescriptionValidator" ControlToValidate="BlogDescription" Display="None" SetFocusOnError="true" ErrorMessage="<%$ Loc:Error.DescriptionMaxLengthExceeded %>" ValidationExpression="^[\s\S]{0,2048}$" />				
				</div>
				
				<div class="blog-edit-field blog-edit-field-settings">
					<div class="blog-edit-field-setting"><asp:CheckBox ID="NotifyComments" TabIndex="5" runat="server" Text="<%$ Loc:CheckBoxText.SendEmailNotifications %>" /></div>
				</div>
				
				<% if (Component.ComponentMode == BlogEditComponent.Mode.Edit && Component.CanModerate && !Component.IsOwner) { %>
				<div class="blog-edit-field blog-edit-field-settings">
					<div class="blog-edit-field-setting"><asp:CheckBox ID="DisableBlog" TabIndex="6" runat="server" Text="<%$ Loc:CheckBoxText.DisableBlog %>" /></div>
				</div>
				<%} %>
				
				<% if (Component.CanApproveComments) {%>
					<div id="<%= GetClientID("EnableCommentModerationWrapper") %>" class="blog-edit-field blog-edit-field-settings">
						<div class="blog-edit-field-setting"><asp:CheckBox ID="EnableCommentModeration" TabIndex="7" runat="server" Text="<%$ Loc:CheckBoxText.EnableCommentModeration %>" /></div>
					</div>
					<div id="<%= GetClientID("CommentModerationModeWrapper") %>" class="blog-edit-field blog-edit-field-comment-moderation-mode">
						<label for="<%= Encode(CommentModerationMode.ClientID) %>" class="blog-edit-field-caption"><%= GetMessage("Label.CommentModerationMode") %></label>
						<asp:DropDownList runat="server" ID="CommentModerationMode"></asp:DropDownList>
					</div>
					<div id="<%= GetClientID("CommentModerationLinkThresholdWrapper") %>" class="blog-edit-field blog-edit-field-comment-moderation-link-threshold">
						<label for="<%= Encode(CommentModerationLinkThreshold.ClientID) %>" class="blog-edit-field-caption"><%= GetMessage("Label.CommentModerationLinkThreshold") %></label>
						<asp:TextBox ID="CommentModerationLinkThreshold" runat="server" TabIndex="8"></asp:TextBox>
					</div>
					<div id="<%= GetClientID("CommentModerationStopListWrapper") %>" class="blog-edit-field blog-edit-field-comment-moderation-stop-list">
						<label for="<%= Encode(CommentModerationStopList.ClientID) %>" class="blog-edit-field-caption"><%= GetMessage("Label.CommentModerationStopList") %></label>
						<asp:TextBox ID="CommentModerationStopList" runat="server" TabIndex="9" TextMode="MultiLine"></asp:TextBox>
					</div>				
				<%} %>			
				
				<% if (Component.CanSyndicateContent){ %>
					<div class="blog-edit-field blog-edit-field-enable-syndication">
						<asp:CheckBox ID="EnableSyndication" runat="server" TabIndex="10" Text="<%$ Loc:CheckBoxText.EnableSyndication %>" />
					</div>
					<div id="syndicationRssFeedUrlContainer" class="blog-edit-field blog-edit-field-syndication-rss-feed-url" style="<%=EnableSyndication.Checked?string.Empty:"display:none;"%>">
						<label for="<%= Encode(SyndicationRssFeedUrl.ClientID) %>" class="blog-edit-field-caption"><%= GetMessage("Label.SyndicationRssFeedUrl")%><span class="blog-required-field">*</span></label>
						<asp:TextBox ID="SyndicationRssFeedUrl" runat="server" TabIndex="11" Text="http://"></asp:TextBox>
						<asp:RequiredFieldValidator ID="SyndicationRssFeedUrlRequired" runat="server" Enabled="False" Display="Dynamic" ControlToValidate="SyndicationRssFeedUrl" ErrorMessage="<%$ Loc:Message.SyndicationRssFeedUrlRequired %>" Text="*" SetFocusOnError="true" />
						<asp:RegularExpressionValidator ID="SyndicationRssFeedUrlRegex" runat="server" Enabled="False" Display="Dynamic" ControlToValidate="SyndicationRssFeedUrl" ErrorMessage="<%$ Loc:Message.SyndicationRssFeedUrlNotValid %>" ValidationExpression="\s*https?://.+" Text="*" SetFocusOnError="true" />                    
					</div>
					<div id="syndicationUpdateableContainer" class="blog-edit-field blog-edit-field-syndication-updateable" style="<%=EnableSyndication.Checked?string.Empty:"display:none;"%>">
						<asp:CheckBox ID="SyndicationUpdateable" runat="server" TabIndex="12" Text="<%$ Loc:CheckBoxText.SyndicationUpdateable %>" Checked="true" />
					</div>
					<div id="syndicationRedirectToCommentsContainer" class="blog-edit-field blog-edit-field-syndication-redirect-comments" style="<%=EnableSyndication.Checked?string.Empty:"display:none;"%>">
						<asp:CheckBox ID="SyndicationRedirectToComments" runat="server" TabIndex="13" Text="<%$ Loc:CheckBoxText.SyndicationRedirectToComments %>" Checked="true" />
					</div>				    			
				<%} %>
				
				<% if (Component.BlogCustomFieldEditors.Length > 0){ %>
					<% foreach (CustomFieldEditor ed in Component.BlogCustomFieldEditors){%>
						<div class="blog-edit-field blog-edit-custom-property-<%= ed.Field.Name.ToLowerInvariant() %>">
						<% if(string.Equals(ed.Field.CustomTypeId.ToUpperInvariant(), "BITRIX.SYSTEM.BOOLEAN", StringComparison.Ordinal) && ed.Field.Settings.GetInt("view", 0) == 0){%>
							<span class="blog-edit-custom-property-single-line-editor"><%= ed.Render() %></span><label for="<%= Encode(ed.ClientID) %>" class="blog-edit-field-caption blog-edit-custom-property-single-line-caption"><%= ed.Caption %>
								<% if (ed.IsRequired){ %>
								   <span class="blog-required-field">*</span>
								<%} %>
							</label>
						 <%} %>
						 <% else { %>
							<label class="blog-edit-field-caption">
								<%= ed.Caption %>
								<% if (ed.IsRequired){ %>
								   <span class="blog-required-field">*</span>
								<%} %>
							</label>
							<%= ed.Render() %>
						<%} %>
						</div>
					 <%} %>		    
				<%} %>			
			</div>				
		</div>
		
		<% if (showUsers) { %>
		<div id="<%= ClientID %>_page1" class="blog-edit-form-page-users" <% if (pageId != 1) { %>style="display: none"<% } %>>
			<% if (UsersPlaceholder.Controls.Count > 0 && UsersPlaceholder.Controls[0].Visible) { %>
			<div class="blog-edit-form-title"><%= GetMessageRaw("FormTitle.Users") %></div>
			
			<div class="blog-edit-fields">
			<div class="blog-edit-fields-blog-users">
				<asp:PlaceHolder runat="server" ID="UsersPlaceholder" />
			</div>
			</div>
			<% } %>
			
			<div class="blog-edit-form-title"><%= GetMessageRaw("FormTitle.UserGroups") %></div>
				
			<div class="blog-edit-fields">
			<div class="blog-edit-fields-blog-groups">
				<asp:PlaceHolder runat="server" ID="GroupsPlaceholder" />
			</div>
			</div>
		</div>
		
		
		<input type="hidden" id="<%= ClientID %>_pageId" name="<%= UniqueID %>$pageId" value="<%= pageId %>" />
		<% } %>
		
		<div class="blog-edit-fields">
		<div class="blog-edit-buttons">
			<% if (Component.ComponentMode != BlogEditComponent.Mode.Add || Component.CanCreatePersonalBlog) { %> 
			<asp:Button runat="server" ID="Save" TabIndex="10" OnClick="SaveClick" />
			<% } %>
			<% if (Component.ComponentMode == BlogEditComponent.Mode.Add && Component.CanCreateTeamBlog) { %> 
			<asp:Button runat="server" ID="SaveTeam" TabIndex="11" OnClick="SaveTeamClick" Text="<%$ LocRaw:ButtonText.CreateTeamBlog %>" />
			<% } %>
		</div>
		</div>
	</div>
</div>

<script type="text/javascript">
	function <%= ClientID %>_SetPage(a)
	{
		var links = a.parentNode.getElementsByTagName('A');		
		for (var i = 0; i < links.length; i++)
		{
			var link = links[i];
			link.style.borderBottom = (link == a) ? 'none' : '';
			
			var page = document.getElementById('<%= ClientID %>_page' + i);
			if (page)
				page.style.display = (link == a) ? '' : 'none';
				
			if (link == a)
			{
				var store = document.getElementById('<%= ClientID %>_pageId');
				if (store)
					store.value = i.toString();
			}
		}
		
		return false;
	}

	if (typeof (Bitrix) == 'undefined') { var Bitrix = {}; }

	if(typeof(Bitrix.BlogSettingsEditor) == 'undefined') {
		Bitrix.BlogSettingsEditor = function Bitrix$BlogSettingsEditor() { 
			this._id = "";
			this._config = {};
		}

		Bitrix.BlogSettingsEditor.prototype = {
			initialize: function(id) {
				this._id = id;
			},
			getId: function() { return this._id; },
			prepare: function(config) {
				this._config = config ? config : {};

				var display = this.getOption("display", false);
				if (display) {
					Bitrix.EventUtility.addEventListener(this.getElement("EnableCommentModeration"), "click", Bitrix.TypeUtility.createDelegate(this, this.layout));
					Bitrix.EventUtility.addEventListener(this.getElement("CommentModerationMode"), "change", Bitrix.TypeUtility.createDelegate(this, this.layoutFilter));
					this.layout();
				}
				else {
					this.displayElement(this.getElement("EnableCommentModerationWrapper"), false);
					this.displayElement(this.getElement("CommentModerationModeWrapper"), false);
					this.displayElement(this.getElement("CommentModerationLinkThresholdWrapper"), false);
					this.displayElement(this.getElement("CommentModerationStopListWrapper"), false);
				}
			},
			getOption: function(name, defVal) { return (name in this._config) ? this._config[name] : defVal; },
			getElement: function(optName) {
				var id = this.getOption(optName, null);
				return id ? document.getElementById(id) : null;
			},
			displayElement: function(el, display) { if (el) el.style.display = display ? "" : "none"; },
			layout: function() {
				var enable = this.getElement("EnableCommentModeration");
				var display = enable ? enable.checked : false;

				this.displayElement(this.getElement("EnableCommentModerationWrapper"), true);
				this.displayElement(this.getElement("CommentModerationModeWrapper"), display);
				this.displayElement(this.getElement("CommentModerationLinkThresholdWrapper"), display);
				this.displayElement(this.getElement("CommentModerationStopListWrapper"), display);

				if (display) this.layoutFilter();
			},
			layoutFilter: function() {
				var mode = this.getElement("CommentModerationMode");
				var display = mode ? mode.value == "FILTER" : false;
				this.displayElement(this.getElement("CommentModerationLinkThresholdWrapper"), display);
				this.displayElement(this.getElement("CommentModerationStopListWrapper"), display);
			}
		}
		
		Bitrix.BlogSettingsEditor._items = {};
		Bitrix.BlogSettingsEditor.create = function(id) {
		
			if(id in this._items) return this.items[id];
			var self = new Bitrix.BlogSettingsEditor();
			self.initialize(id);
			this._items[id] = self;
			return self;
		}
	}
</script>