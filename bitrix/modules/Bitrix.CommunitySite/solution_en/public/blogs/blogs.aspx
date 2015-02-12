<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="Blogs" %>

<script runat="server">

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		loadTrap.Load += new EventHandler(loadTrap_Load);
		Bitrix.UI.BXPage.RegisterLink(
			"wlwmanifest",
			"application/wlwmanifest+xml",
			Bitrix.BXSite.Current.DirectoryAbsolutePath + "wlwmanifest.xml"
		);
	}
	
	void loadTrap_Load(object sender, EventArgs e)
	{
		BXComponent component = BlogComplex.Component;
		if (component == null)
			return;
		
		var parameters = new[] { 
			"BlogSlug",
			"NewPostUrlTemplate", "PostListUrlTemplate", "DraftPostListUrlTemplate", 
			"UserProfileUrlTemplate", "NewBlogUrlTemplate", "BlogEditUrlTemplate",
			"RssBlogPostsUrlTemplate", "BlogMetaWeblogApiUrlTemplate", "SearchTagsUrlTemplate"
		};

		object value;
		foreach (var param in parameters)
			if (component.Results.TryGetValue(param, out value) && value != null)
				Context.Items["CommunitySite.Sidebar." + param] = value.ToString();
	}
	
</script>

<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server">

<bx:IncludeComponent 
	id="BlogComplex" 
	runat="server" 
	componentname="bitrix:blog" 
	template=".default" 
	ColorCss="<%$ Options:Bitrix.CommunitySite:BlogColorScheme %>" 
	MaxBlogGroups="1" 
	SlugIgnoreList="admin;blog;blogs;users;user;search;posts;rss;tags;new;request;articles;all" 
	MinSlugLength="3" 
	MaxSlugLength="25" 
	SetPageTitle="True" 
	DontSetPostsMasterTitle="True" 
	MaxWordLength="40" 
	PeriodDays="0" 
	PostsPerPage="10" 
	BlogsPerPage="20" 
	CommentsPerPage="30" 
	MainPagePostCount="6" 
	MainPageBlogCount="6" 
	GuestEmail="required" 
	GuestCaptcha="True" 
	EnableSEF="True" 
	SEFFolder="<%$ Options:Bitrix.CommunitySite:BlogSefFolder %>" 
	BlogVariable="blog" 
	CategoryVariable="category" 
	CommentVariable="comment" 
	PostVariable="post" 
	ActionVariable="act" 
	PageVariable="page" 
	UserVariable="user" 
	NewPostTemplate="/#BlogSlug#/new/" 
	PostListTemplate="/#BlogSlug#/" 
	PostListPageTemplate="/#BlogSlug#/page-#PageId#/" 
	DraftPostListTemplate="/#BlogSlug#/draft/" 
	DraftPostListPageTemplate="/#BlogSlug#/draft/page-#PageId#/" 
	PostTemplate="/#BlogSlug#/#PostId#/" 
	PostPageTemplate="/#BlogSlug#/#PostId#/page-#PageId#/##comments" 
	PostEditTemplate="/#BlogSlug#/#PostId#/edit/" 
	CommentReadTemplate="/#BlogSlug#/#PostId#/#CommentId#/##comment#CommentId#" 
	CommentEditTemplate="/#BlogSlug#/#PostId#/#CommentId#/edit/" 
	UserProfileTemplate="/users/#UserId#/" 
	UserProfileEditTemplate="/users/#UserId#/edit/" 
	NewBlogTemplate="/new/" 
	BlogEditTemplate="/#BlogSlug#/edit/" 
	BlogListTemplate="<%$ Options:Bitrix.CommunitySite:BlogSefFolder %>" 
	BlogListPageTemplate="/page-#PageId#/" 
	CategoryBlogListTemplate="" 
	CategoryBlogListPageTemplate="" 
	NewPostListTemplate="/posts/" 
	NewPostListPageTemplate="/posts/#PageId#/" 
	PopularPostListTemplate="/posts/popular/" 
	PopularPostListPageTemplate="/posts/popular/#PageId#/" 
	DiscussPostListTemplate="/posts/discuss/" 
	DiscussPostListPageTemplate="/posts/discuss/#PageId#/" 
	PagingAllow="True" 
	PagingMode="direct" 
	PagingTemplate=".default" 
	PagingShowOne="False" 
	PagingTitle="Pages:" 
	PagingPosition="bottom" 
	PagingMaxPages="10" 
	PagingMinRecordsInverse="1" 
	UserProfileUrl="<%$ Options:Bitrix.CommunitySite:UserProfileUrlTemplate %>" 
	UserProfileEditUrl="<%$ Options:Bitrix.CommunitySite:UserProfileEditUrlTemplate %>" 
	DisplaySearchResults="True" 
	AllowToAjustBlogGroups="False" 
	ObligatoryBlogGroups="<%$ Options:Bitrix.CommunitySite:BlogCategoryId %>" 
	AvailableBlogGroups="" 
	AvailableBlogCustomFieldsForAuthor="" 
	AvailableBlogCustomFieldsForModerator="" 
	AvailablePostCustomFieldsForAuthor="" 
	AvailablePostCustomFieldsForModerator="" 
	TagsVariable="tags" 
	SearchTagsTemplate="/tags/?#SearchTags#" 
	SearchTagsPageTemplate="/tags/?#SearchTags#&amp;page=#PageId#" 
	RssBlogPostsTemplate="/#BlogSlug#/rss/" 
	DisplayMenu="False" 
	MaximalFileSizeInKBytes="1024" 
	MaximalAllFilesSizeInKBytes="8192" 
	MaximalImageWidthInPixels="580" 
	MaximalImageHeightInPixels="0" 
	CacheMode="None" 
	CacheDuration="30" 
	DisplaySidebar="False" 
	BlogMetaWeblogApiTemplate="/#BlogSlug#/metaweblog/" 
	AllowMetaWeblogApi="True" 
	EnableExtendedEntries="True"
	SearchBlogTagsTemplate="/#BlogSlug#/tags/?#SearchTags#" 
	SearchBlogTagsPageTemplate="/#BlogSlug#/tags/?#SearchTags#&amp;page=#PageId#" 
	RssBlogPostCommentsTemplate="/#BlogSlug#/#PostId#/rss/" 
	RssAllPostsTemplate="/rss/" 
	EnableVotingForPost="True" 
	EnableVotingForComment="True" 
	RolesAuthorizedToVote="User" 
	SortBlogPostsByVotingTotals="False" 
/>

<asp:Placeholder ID="loadTrap" runat="server" /></asp:Content>