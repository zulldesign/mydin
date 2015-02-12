<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.personal/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPersonalTemplate" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>

<% if (Component.DisplayMenu) { %><asp:PlaceHolder runat="server" ID="MenuPlaceholder" /><% } %>

<bx:IncludeComponent
	ID="Edit" 
	runat="server" 
	ComponentName="bitrix:blog.post.form"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	
	BlogSlug="<%$ Results:BlogSlug %>" 
	PostId="<%$ Results:PostId %>" 
	PublishUrlTemplate="<%$ Results:PostUrlTemplate %>" 
	DraftUrlTemplate="<%$ Results:DraftPostListUrlTemplate %>" 
	RedirectUrlTemplate="<%$ Results:RedirectUrlTemplate %>"
	AvailablePostCustomFieldsForAuthor="<%$ Parameters:AvailablePostCustomFields %>"
	AvailablePostCustomFieldsForModerator="<%$ Parameters:AvailablePostCustomFields %>"
/>

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
	UserBlogSettingsUrlTemplate="<%$ Results:BlogEditUrlTemplate %>" 
	BlogIndexUrl="" 
	UserProfileUrlTemplate=""
	DraftPostListUrlTemplate="<%$ Results:DraftsUrlTemplate%>"
	Visible="<%$ Results:DisplayMenu %>"
/>
<% } %>

<%@ Reference  VirtualPath="~/bitrix/components/bitrix/blog.post.form/component.ascx"  %>
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
		BlogPostFormComponent postForm = (BlogPostFormComponent)Edit.Component;	
		if (postForm.FatalError != BlogPostFormComponent.ErrorCode.None)
		{
			Menu.Component.Visible = false;
			return;
		}
		MenuPlaceholder.SetRenderMethodDelegate(Menu_Render);
		BlogMenuComponent menu = (BlogMenuComponent)Menu.Component;
		if (postForm.ComponentMode == BlogPostFormComponent.Mode.Add)
			menu.Parameters["NewPostUrlTemplate"] = "";
		menu.Blog = postForm.Blog;
	}
</script>