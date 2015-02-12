<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.personal/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPersonalTemplate" %>
<%@ Reference  VirtualPath="~/bitrix/components/bitrix/blog.edit/component.ascx"  %>
<%@ Import Namespace="Bitrix.Blog.Components" %>

<% if (Component.DisplayMenu) { %><asp:PlaceHolder runat="server" ID="MenuPlaceholder" /><% } %>

<bx:IncludeComponent 
	ID="Settings" 
	runat="server" 
	ComponentName="bitrix:blog.edit" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	
	RedirectUrlTemplate="<%$ Results:RedirectUrlTemplate %>" 
	BlogSlug="<%$ Results:BlogSlug %>"
	BlogAbsoluteUrlTemplate="<%$ Results:PostsUrlTemplate %>"
	AvailableBlogCustomFieldsForAuthor="<%$ Parameters:AvailableBlogCustomFields %>"
	AvailableBlogCustomFieldsForModerator="<%$ Parameters:AvailableBlogCustomFields %>"
	AllowToAjustBlogGroups="False"
/>
<% 
	if (((BlogEditComponent)Settings.Component).FatalError == BlogEditComponent.ErrorCode.None && Component.AllowMetaWeblogApi) 
	{ 
		string metaweblogUrl = Component.ResolveTemplateUrl(
			Component.ComponentCache["MetaWeblogApiUrlTemplate"].ToString(), 
			Component.ComponentCache
		);
		if (!string.IsNullOrEmpty(metaweblogUrl)) 
		{
			%>
<div class="blog-content">
	<div class="blog-note-box blog-note-success">
		<div class="blog-note-box-text">
			<%= string.Format(GetMessageRaw("MetaWeblogApiNote"), Encode(metaweblogUrl)) %>
		</div>
	</div>
</div>
			<% 
		} 
	}
%>

<% if (false) { //just to initialize the menu component after the main component %>
<bx:IncludeComponent 
	ID="Menu" 
	runat="server" 
	ComponentName="bitrix:blog.menu" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	
	CheckUserPermissions="True"
	BlogSlug="<%$ Results:BlogSlug %>"		
	NewPostUrlTemplate="<%$ Results:NewPostUrlTemplate %>" 
	UserBlogUrlTemplate="" 
	UserBlogSettingsUrlTemplate="" 
	BlogIndexUrl="" 
	UserProfileUrlTemplate=""
	DraftPostListUrlTemplate="<%$ Results:DraftsUrlTemplate%>"
	Visible="<%$ Results:DisplayMenu %>"
/>
<% } %>


<%@ Reference  VirtualPath="~/bitrix/components/bitrix/blog.menu/component.ascx"  %>
<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		Menu.Load += new EventHandler(Menu_Load);
	}
	void Menu_Render(HtmlTextWriter output, Control container)
	{
		Menu.RenderControl(output);
	}
	void Menu_Load(object sender, EventArgs e)
	{
		BlogEditComponent edit = (BlogEditComponent)Settings.Component;	
		if (edit.FatalError != BlogEditComponent.ErrorCode.None)
		{
			Menu.Component.Visible = false;
			return;
		}
		MenuPlaceholder.SetRenderMethodDelegate(Menu_Render);
		BlogMenuComponent menu = (BlogMenuComponent)Menu.Component;
		menu.Blog = edit.Blog;
	}
</script>