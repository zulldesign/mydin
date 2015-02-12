<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main"  Title="Personal site" %>
<asp:Content ID="Content" ContentPlaceHolderID="BXContent" runat="server">
<bx:IncludeComponent 
	runat="server" 
	ID="Blog" 
	ComponentName="bitrix:blog.personal" 
	Template=".default" 
	PagingAllow="True" 
	PagingMode="direct" 
	PagingTemplate=".default" 
	PagingShowOne="False" 
	EnableSEF="True" 
	PagingTitle="Pages:" 
	PagingPosition="bottom" 
	PagingMaxPages="3" 
	PagingMinRecordsInverse="1" 
	SEFFolder="<%$ Options:Bitrix.PersonalSite:BlogSefFolder %>" 
	DisplaySearchResults="True" 
	ThemeCssFilePath="~/bitrix/components/bitrix/blog/templates/.default/style.css" 
	ColorCssFilePath="<%$ Options:Bitrix.PersonalSite:BlogColorScheme %>" 
	BlogSlug="personal" 
	AvailableBlogCustomFields="" 
	AvailablePostCustomFields="" 
	CommentVariable="comment" 
	PostVariable="post" 
	ActionVariable="act" 
	PageVariable="page" 
	TagsVariable="tags" 
	NewPostTemplate="/new-post" 
	PostsPageTemplate="?page=#i:PageId#" 
	DraftsTemplate="/drafts/" 
	DraftsPageTemplate="/drafts/?page=#i:PageId#" 
	PostTemplate="/#i:PostId#/" 
	PostPageTemplate="/#i:PostId#/?page=#i:PageId###comments" 
	PostEditTemplate="/#i:PostId#/edit" 
	CommentReadTemplate="/#i:PostId#/#i:CommentId#/##comment#i:CommentId#" 
	CommentEditTemplate="/#i:PostId#/#i:CommentId#/edit" 
	BlogEditTemplate="/settings" 
	SearchTagsTemplate="/tags/?#SearchTags#" 
	SearchTagsPageTemplate="/tags/?#SearchTags#&amp;page=#PageId#" 
	MetaWeblogApiTemplate="/metaweblog" 
	RssPostsTemplate="/rss" 
	RssPostCommentsTemplate="/#i:PostId#/rss" 
	DisplayMenu="False" 
	SetPageTitle="True" 
	MaxWordLength="15" 
	PostsPerPage="5" 
	CommentsPerPage="20" 
	AllowMetaWeblogApi="True" 
	GuestEmail="required" 
	GuestCaptcha="True" 
	UserProfileUrl="<%$ Options:Bitrix.PersonalSite:ProfileUrl %>" 
	MaximalFileSizeInKBytes="1024" 
	MaximalAllFilesSizeInKBytes="8192" 
	MaximalImageWidthInPixels="540" 
	MaximalImageHeightInPixels="0" 
	DontSetPostsMasterTitle="True" 
/>
</asp:Content>
<script runat="server">
	//protected override void OnPreRenderComplete(EventArgs e)
	//{
	//    base.OnPreRenderComplete(e);
	//    MasterTitleHtml = null;
	//    //Blog.Component.Te
	//}
</script>