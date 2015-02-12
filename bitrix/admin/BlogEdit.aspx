<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="true" CodeFile="BlogEdit.aspx.cs" Inherits="bitrix_admin_BlogEdit" %>
	
<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit"
	TagPrefix="bx" %>
<%@ Register Src="~/bitrix/controls/Main/CustomFieldList.ascx" TagName="CustomFieldList" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/controls/Blog/EditPermissions.ascx" TagName="EditPermissions" TagPrefix="bx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	
	<bx:BXContextMenuToolbar ID="BXBlogEditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="BlogList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton"
				CommandName="add" Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
				CssClass="context-button icon btn_new" Href="BlogEdit.aspx" />
				
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:ActionTitle.Delete %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="BlogEdit" />
	<bx:BXTabControl ID="TabControl" runat="server" OnCommand="OnBlogEdit" ValidationGroup="BlogEdit">
		<bx:BXTabControlTab ID="MainSettingsTab" Runat="server" Selected="True" Text="<%$ LocRaw:TabText.Blog %>" Title="<%$ LocRaw:TabTitle.Blog %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<% 
				if (BlogId > 0)
				{
					%><tr valign="top"><td class="field-name">ID:</td><td><%= BlogId %></td></tr><%
				}
				%>
				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.Active") %>">
					<td class="field-name" width="40%"><%= GetMessage("FieldLabel.Active") %>:</td>
					<td width="60%"><asp:CheckBox ID="BlogActive" runat="server" /></td>
				</tr>

				<tr valign="top" title="<%= GetMessage("FieldTooltip.Owner") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.Owner") %>:</td>
					<td>
					    <%--<asp:DropDownList runat="server" ID="BlogOwner"></asp:DropDownList>--%>
                       <%-- <asp:TextBox runat="server" ID="BlogOwner"></asp:TextBox>--%>
					<bx:BXAutoCompleteFilter 
				            ID="FindUserAutocomplete" 
				            runat="server" 
				            Key="" 
				            Text="<%$ LocRaw:FilterText.CreatedBy%>"
				            Url="~/bitrix/handlers/Main/UsersHandler.ashx" 
				            TextBoxWidth="350px"
				    />
						<%--<asp:RequiredFieldValidator ID="BlogOwnerRequiredFieldValidator" runat="server" ValidationGroup="BlogEdit" ControlToValidate="FindUserAutocomplete" ErrorMessage="<%$ LocRaw:Message.OwnerNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>						--%>
					</td>
				</tr>					

				<tr valign="top" title="<%= GetMessage("FieldTooltip.Name") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.Name") %>:</td>
					<td>
						<asp:TextBox ID="BlogName" runat="server" Width="350px" />
						<asp:RequiredFieldValidator ID="NameRequiredValidator" runat="server" ValidationGroup="BlogEdit" ControlToValidate="BlogName" ErrorMessage="<%$ Loc:Message.NameNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>
					</td>
				</tr>				

				<tr valign="top" title="<%= GetMessage("FieldTooltip.Slug") %>" >
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.Slug") %>:<br /><small><%= GetMessage("FieldComment.Slug") %></small></td>
					<td>
						<asp:TextBox ID="BlogSlug" runat="server" Width="350px" />
						<asp:RequiredFieldValidator ID="SlugValudatorRequired" runat="server" ValidationGroup="BlogEdit" ControlToValidate="BlogSlug" ErrorMessage="<%$ Loc:Message.SlugNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator runat="server" ID="SlugValudatorContent" ValidationExpression="^[a-zA-Z\-\d]+$" ControlToValidate="BlogSlug" ValidationGroup="BlogEdit" ErrorMessage="<%$ Loc:Message.InvalidSlug %>" Display="Dynamic">*</asp:RegularExpressionValidator>
					</td>
				</tr>
				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.Description") %>" >
					<td class="field-name"><%= GetMessage("FieldLabel.Description") %>:</td>
					<td>
						<asp:TextBox Rows="7" ID="BlogDescription"  TextMode="MultiLine" runat="server" Width="350px" />						
					</td>
				</tr>
							
                <tr valign="top" title="<%= GetMessage("FieldTooltip.Categories") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.Categories") %>:</td>
					<td>
                        <asp:ListBox runat="server" ID="BlogCategories" SelectionMode="Multiple" Width="350px" Height="175px">
                        </asp:ListBox>
					</td>
				</tr>				
				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.CommentNotification") %>">
					<td class="field-name" width="40%">&nbsp;</td>
					<td width="60%">
		                <asp:CheckBox runat="server" ID="BlogNotifyOfComments" Text="<%$ Loc:FieldLabel.CommentNotification %>" />
					</td>
				</tr>					
				
				<tr valign="top" title="<%= GetMessage("TeamBlogs.Tooltip") %>">
					<td class="field-name" width="40%">&nbsp;</td>
					<td width="60%">
		                <asp:CheckBox runat="server" ID="BlogIsTeam" Text="<%$ LocRaw:CheckBoxText.IsTeam %>" />
					</td>
				</tr>					
				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.Sort") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.Sort") %>:</td>
					<td>
					    <asp:TextBox ID="BlogSort" runat="server" Width="50px" />
					    <asp:RegularExpressionValidator runat="server" ID="SortValidator" ValidationExpression="^[\d]*$" ControlToValidate="BlogSort" ValidationGroup="BlogEdit" ErrorMessage="<%$ Loc:Message.InvalidSortIndex %>" Display="Dynamic">*</asp:RegularExpressionValidator>
					</td>
				</tr>
				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.XmlId") %>" >
					<td class="field-name"><%= GetMessage("FieldLabel.XmlId") %>:</td>
					<td>
						<asp:TextBox Rows="7" ID="BlogXmlId"  runat="server" Width="350px" />						
					</td>
				</tr>		
				<% 
					if (Bitrix.Modules.BXModuleManager.IsModuleInstalled("search"))
					{	 
				%>
				<tr valign="top">
					<td class="field-name" width="40%">
						<a href="#remark1" style="vertical-align:super; text-decoration:none"><span class="required">1</span></a><%= GetMessage("FieldLabel.IndexContent") %>:
					</td>
					<td width="60%">
						<asp:DropDownList ID="BlogIndexContent" runat="server" >
						    <asp:ListItem Text="<%$ LocRaw:Option.Nothing %>" Value="Nothing" />
						    <asp:ListItem Text="<%$ LocRaw:Option.All %>" Value="All" Selected="True" />
						    <asp:ListItem Text="<%$ LocRaw:Option.TagsOnly %>" Value="TagsOnly" />
						</asp:DropDownList>
					</td>
				</tr>
				<%
					} 
				%>	
				<%
					if (CustomFieldList1.HasItems)
					{
				%>
				<tr class="heading">
					<td colspan="2"><%= GetMessage("FieldHeading.Properties") %>:</td>
				</tr>
				<tr valign="top">
					<td colspan="2" align="center">
						<asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
				        <ContentTemplate>
	                        <bx:CustomFieldList ID="CustomFieldList1" runat="server" EditMode="false" ValidationGroup="BlogEdit" />
						</ContentTemplate>
						</asp:UpdatePanel>
					</td>
				</tr>
				<%
					}
				%>	
			</table>
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="UserGroups" runat="server" Text="<%$ LocRaw:TabText.UserGroups %>" Title="<%$ LocRaw:TabTitle.UserGroups %>">
			<bx:EditPermissions runat="server" ID="Permissions" />  
		</bx:BXTabControlTab>		
		<bx:BXTabControlTab ID="PermissionsTab" Runat="server" Text="<%$ LocRaw:TabText.Moderation %>" Title="<%$ LocRaw:TabTitle.Moderation %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr valign="top" title="<%= GetMessage("FieldTooltip.EnableCommentPremoderation") %>">
					<td class="field-name" width="40%">
						<asp:Label ID="EnableCommentModerationLbl" runat="server" AssociatedControlID="EnableCommentModeration" Text="<%$ LocRaw:FieldLabel.EnableCommentModeration %>" />:
					</td>
					<td width="60%">
						<asp:CheckBox ID="EnableCommentModeration" runat="server" />
					</td>
				</tr>
				<tr valign="top" title="<%= GetMessage("FieldTooltip.CommentModerationMode") %>">
					<td class="field-name" width="40%">
						<asp:Label ID="CommentModerationModeLbl" runat="server" AssociatedControlID="CommentModerationMode" Text="<%$ LocRaw:FieldLabel.CommentModerationMode %>" />:
					</td>
					<td width="60%">
		                <asp:DropDownList runat="server" ID="CommentModerationMode" Width="400px"></asp:DropDownList>
					</td>
				</tr>
				<tr id="commentPremoderationFilterHead" class="heading">
					<td colspan="2"><%= GetMessage("FieldHeading.CommentPremoderationFilter") %></td>
				</tr>				
				<tr id="commentPremoderationFilterLinkThreshold" valign="top" title="<%= GetMessage("FieldTooltip.CommentPremoderationFilter.LinkThreshold") %>">
					<td class="field-name" width="40%">
						<asp:Label ID="CommentPremoderationFilterLinkThresholdLbl" runat="server" AssociatedControlID="CommentPremoderationFilterLinkThresholdTbx" Text="<%$ LocRaw:FieldLabel.CommentPremoderationFilter.LinkThreshold %>" />:						
					</td>
					<td width="60%">
						<asp:TextBox runat="server" ID="CommentPremoderationFilterLinkThresholdTbx" TextMode="SingleLine" Width="50px"/>
					</td>
				</tr>
				<tr id="commentPremoderationFilterStopList" valign="top" title="<%= GetMessage("FieldTooltip.CommentPremoderationFilter.StopList") %>">
					<td class="field-name" width="40%">
						<asp:Label ID="CommentPremoderationFilterStopListLbl" runat="server" AssociatedControlID="CommentPremoderationFilterStopListTbx" Text="<%$ LocRaw:FieldLabel.CommentPremoderationFilter.StopList %>" />:						
					</td>
					<td width="60%">
						<asp:TextBox runat="server" ID="CommentPremoderationFilterStopListTbx" TextMode="MultiLine" Width="400px" Height="200px"/>
					</td>
				</tr>											
			</table>
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="SyndicationTab" Runat="server" Text="<%$ LocRaw:TabText.Syndication %>" Title="<%$ LocRaw:TabTitle.Syndication %>">
		    <table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr valign="top">
					<td class="field-name" width="40%"><asp:Label ID="EnableSyndicationLabel" runat="server" AssociatedControlID="EnableSyndication" Text="<%$ LocRaw:FieldLabel.EnableSyndication %>" />:</td>
					<td width="60%">
		                <asp:CheckBox ID="EnableSyndication" runat="server" />
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name"><asp:Label ID="SyndicationRssFeedUrlLabel" runat="server" AssociatedControlID="SyndicationRssFeedUrl" Text="<%$ LocRaw:FieldLabel.SyndicationRssFeedUrl %>" />:</td>
					<td>
						<asp:TextBox ID="SyndicationRssFeedUrl" runat="server" Width="350px" />
                        <asp:RequiredFieldValidator ID="SyndicationRssFeedUrlRequired" runat="server" Display="Dynamic" ValidationGroup="BlogEdit" ControlToValidate="SyndicationRssFeedUrl" ErrorMessage="<%$ Loc:Message.SyndicationRssFeedUrlRequired %>" Text="*" SetFocusOnError="true" />
				        <asp:RegularExpressionValidator ID="SyndicationRssFeedUrlRegex" runat="server" Display="Dynamic" ValidationGroup="BlogEdit" ControlToValidate="SyndicationRssFeedUrl" ErrorMessage="<%$ Loc:Message.SyndicationRssFeedUrlNotValid %>" ValidationExpression="\s*https?://.+" Text="*" SetFocusOnError="true" />                    					
					</td>
				</tr>
				<tr valign="top" title="<%= GetMessage("FieldTooltip.SyndicationUpdateable") %>">
					<td class="field-name" width="40%"><asp:Label ID="SyndicationUpdateableLabel" runat="server" AssociatedControlID="SyndicationUpdateable" Text="<%$ LocRaw:FieldLabel.SyndicationUpdateable %>" />:</td>
					<td width="60%">
		                <asp:CheckBox ID="SyndicationUpdateable" runat="server" />
					</td>
				</tr>
				<tr valign="top" title="<%= GetMessage("FieldTooltip.SyndicationRedirectToComments") %>">
					<td class="field-name" width="40%"><asp:Label ID="SyndicationRedirectToCommentsLabel" runat="server" AssociatedControlID="SyndicationRedirectToComments" Text="<%$ LocRaw:FieldLabel.SyndicationRedirectToComments %>" />:</td>
					<td width="60%">
		                <asp:CheckBox ID="SyndicationRedirectToComments" runat="server" />
					</td>
				</tr>												
		    </table>
		</bx:BXTabControlTab>
		<%-- 
		<bx:BXTabControlTab ID="PermissionsTab" Runat="server" Text="Разрешения" Title="Установка разрешений">
		    <table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr valign="top" title="Доступ к сообщениям по умолчанию (кто может видеть сообщения)">
					<td class="field-name" width="40%">Доступ к сообщениям:</td>
					<td width="60%">
		                <asp:DropDownList runat="server" ID="BlogPostVisibilityMode" Width="300px" ></asp:DropDownList>
					</td>
				</tr>
				<tr valign="top" title="Разрешить комментировать">
					<td class="field-name">Разрешить комментировать:</td>
					<td>
		                <asp:DropDownList runat="server" ID="BlogAddCommentPermission" Width="300px" ></asp:DropDownList>
					</td>
				</tr>
				<tr valign="top" title="Скрывать комментарии">
					<td class="field-name">Скрывать комментарии:</td>
					<td>
                        <asp:DropDownList runat="server" ID="BlogCommentApproval" Width="300px" ></asp:DropDownList>
					</td>
				</tr>											
			</table>	
		</bx:BXTabControlTab>
		
		<bx:BXTabControlTab ID="AdditionalSettingsTab" Runat="server" Text="Дополнительно" Title="Дополнительные настройки блога">
		    <table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr valign="top" title="Управление уведомлениями о новых комментариях">
					<td class="field-name" width="40%">Уведомлять о новых комментариях:</td>
					<td width="60%">
		                <asp:CheckBox runat="server" ID="BlogNotifyOfComments" />
					</td>
				</tr>
            </table>
        </bx:BXTabControlTab>
        --%>
	</bx:BXTabControl>
	
<% if (Bitrix.Modules.BXModuleManager.IsModuleInstalled("search")) { %>
<bx:BXAdminNote runat="server">
    <span class="required" style="vertical-align:super" id="remark1" >1</span><%= string.Format(GetMessageRaw("Note.Reindex"), "SearchReindex.aspx") %>
</bx:BXAdminNote>
<% } %>

<script type="text/javascript" language="javascript">
    if(typeof(Bitrix) == "undefined") {
        var Bitrix = new Object();
    }
    Bitrix.BlogEditPageManager = function Bitrix$BlogEditPageManager() {
        if(typeof(Bitrix.BlogEditPageManager.initializeBase) == "function")
            Bitrix.BlogEditPageManager.initializeBase(this);
        this._initialized = false;
        this._enableSyndicationElId = "<%= EnableSyndication.ClientID %>";	
        this._syndicationRssFeedUrlElId = "<%= SyndicationRssFeedUrl.ClientID %>";
        this._syndicationUpdateableElId = "<%= SyndicationUpdateable.ClientID %>";
        this._syndicationRedirectToCommentsElId = "<%= SyndicationRedirectToComments.ClientID %>";
        this._syndicationRssFeedUrlLabelElId = "<%= SyndicationRssFeedUrlLabel.ClientID %>";
        this._syndicationUpdateableLabelElId = "<%= SyndicationUpdateableLabel.ClientID %>";
        this._syndicationRedirectToCommentsLabelElId = "<%= SyndicationRedirectToCommentsLabel.ClientID %>";
        this._syndicationRssFeedUrlRequiredElId = "<%= SyndicationRssFeedUrlRequired.ClientID %>";
        this._syndicationRssFeedUrlRegexElId = "<%= SyndicationRssFeedUrlRegex.ClientID %>";
        this._enableCommentModedationElId = "<%= EnableCommentModeration.ClientID %>";
        this._commentModerationModeLabelElId = "<%= CommentModerationModeLbl.ClientID %>";
        this._commentModerationModeElId = "<%= CommentModerationMode.ClientID %>";
        this._commentModerationFilterLinkThresholdLblElId = "<%= CommentPremoderationFilterLinkThresholdLbl.ClientID %>";
        this._commentModerationFilterLinkThresholdTbxElId = "<%= CommentPremoderationFilterLinkThresholdTbx.ClientID %>";
        this._commentModerationFilterStopListLblElId = "<%= CommentPremoderationFilterStopListLbl.ClientID %>";
        this._commentModerationFilterStopListTbxElId = "<%= CommentPremoderationFilterStopListTbx.ClientID %>";
    }

    Bitrix.BlogEditPageManager.prototype = {
    	initialize: function() {
    		Bitrix.EventUtility.addEventListener(document.getElementById(this._enableSyndicationElId), "click", Bitrix.TypeUtility.createDelegate(this, this._handleChange));
    		Bitrix.EventUtility.addEventListener(document.getElementById(this._enableCommentModedationElId), "click", Bitrix.TypeUtility.createDelegate(this, this._handleChange));
    		Bitrix.EventUtility.addEventListener(document.getElementById(this._commentModerationModeElId), "change", Bitrix.TypeUtility.createDelegate(this, this._handleModeChange));
    		this._initialized = true;
    	},
    	prepare: function() { this.layout(); },
    	layout: function() {
    		var enabled = document.getElementById(this._enableSyndicationElId).checked;
    		this._enableElement(this._syndicationRssFeedUrlElId, enabled);
    		this._enableElement(this._syndicationUpdateableElId, enabled);
    		this._enableElement(this._syndicationRedirectToCommentsElId, enabled);
    		this._enableElement(this._syndicationRssFeedUrlLabelElId, enabled);
    		this._enableElement(this._syndicationUpdateableLabelElId, enabled);
    		this._enableElement(this._syndicationRedirectToCommentsLabelElId, enabled);
    		ValidatorEnable(document.getElementById(this._syndicationRssFeedUrlRequiredElId), enabled);
    		ValidatorEnable(document.getElementById(this._syndicationRssFeedUrlRegexElId), enabled);

    		enabled = document.getElementById(this._enableCommentModedationElId).checked;
    		this._enableElement(this._commentModerationModeLabelElId, enabled);
    		this._enableElement(this._commentModerationModeElId, enabled);
    		this._enableElement(this._commentModerationFilterLinkThresholdLblElId, enabled);
    		this._enableElement(this._commentModerationFilterLinkThresholdTbxElId, enabled);
    		this._enableElement(this._commentModerationFilterStopListLblElId, enabled);
    		this._enableElement(this._commentModerationFilterStopListTbxElId, enabled);

    		this._layoutMode();
    	},
    	_layoutMode: function() {
    		var displayFilter = document.getElementById(this._commentModerationModeElId).value == "FILTER";
    		this._displayElement("commentPremoderationFilterHead", displayFilter);
    		this._displayElement("commentPremoderationFilterLinkThreshold", displayFilter);
    		this._displayElement("commentPremoderationFilterStopList", displayFilter);
    	},
    	_handleChange: function() { this.layout(); },
    	_handleModeChange: function() { this._layoutMode(); },
    	_enableElement: function(elementId, enable) {
    		var element = document.getElementById(elementId);
    		if (!element) return;
    		if (typeof (element.disabled) != "undefined")
    			element.disabled = !enable;
    		else
    			element.setAttribute("disabled", !enable);
    	},
    	_displayElement: function(elementId, display) {
    		var element = document.getElementById(elementId);
    		if (element) element.style.display = display ? "" : "none";
    	}
    }
   
    Bitrix.BlogEditPageManager._instance = null;
    Bitrix.BlogEditPageManager.instance = function(){
        if(this._instance == null){
            this._instance = new Bitrix.BlogEditPageManager();
            this._instance.initialize();   
        }
        return this._instance;
    } 
    window.setTimeout(function(){Bitrix.BlogEditPageManager.instance().prepare();}, 150);                
</script>

</asp:Content>

