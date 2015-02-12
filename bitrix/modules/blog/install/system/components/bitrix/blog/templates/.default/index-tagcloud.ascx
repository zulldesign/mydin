<%@ Control Language="C#" Inherits="Bitrix.UI.BXControl" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/search.tags.cloud/component.ascx" %>
<%@ Reference VirtualPath="index.ascx" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="Bitrix.Search" %>
<%@ Import Namespace="Bitrix.Search.Components" %>
<%@ Import Namespace="Bitrix.DataTypes"  %>
<%@ Import Namespace="Bitrix.Services" %>
<bx:IncludeComponent runat="server" ID="tagcloud" ComponentName="bitrix:search.tags.cloud"
	Template=".default" />

<script runat="server">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		BlogComponent blogComponent;
		for (Control c = Parent; (blogComponent = c as BlogComponent) == null && c != Page; c = c.Parent)
			;

		SearchTagsCloudComponent cloud = (SearchTagsCloudComponent)tagcloud.Component;

        string blogId = Attributes["BlogId"] ?? string.Empty,
            blogSlug = Attributes["BlogSlug"] ?? string.Empty;

		int categoryId = 0;
		if(blogComponent!=null)
		categoryId = blogComponent.Parameters.GetInt("CategoryId");

        BXParamsBag<object> replace = new BXParamsBag<object>();
        replace.Add("BlogId", blogId);
        replace.Add("BlogSlug", blogSlug);
        cloud.TagLinkTemplate = blogComponent.ResolveTemplateUrl(blogComponent.ComponentCache.GetString(blogSlug.Length > 0 ? "SearchBlogTagsUrlTemplate" : "SearchTagsUrlTemplate") ?? string.Empty, replace);            

		cloud.Moderation = SearchTagsCloudComponent.ModerationMode.NotRejected;
		cloud.SizeDistribution = SearchTagsCloudComponent.SizeInterpolationMode.Exponential;
		cloud.SizeMin = 10;
		cloud.SizeMax = 18;

		BXComponent.BXPaging paging = new BXComponent.BXPaging(cloud);
        paging.Allow = false;
		paging.RecordsPerPage = 20;

		foreach (string s in Attributes.Keys)
		{
			if (s.StartsWith("Cloud-", StringComparison.InvariantCulture))
				cloud.Parameters[s.Substring("Cloud-".Length)] = Attributes[s];
		}

		BXSearchQuery q = new BXSearchQuery();
		BXSearchContentGroupFilter f = new BXSearchContentGroupFilter(Bitrix.DataLayer.BXFilterExpressionCombiningLogic.And);
		f.Add(BXSearchContentFilter.IsActive(DateTime.Now));
		f.Add(new BXSearchContentFilterItem(BXSearchField.ModuleId, Bitrix.DataLayer.BXSqlFilterOperators.Equal, "blog"));

		if (categoryId > 0)
		{
			BXBlogCollection blogCol = null;
			string key = string.Concat("_BX_BLOG_TAG_CLOUD_CATEGORY_BLOG_", categoryId);
			if(BXTagCachingManager.IsTagCachingEnabled())				
				blogCol = BXCacheManager.MemoryCache.Application.Get(key) as BXBlogCollection;
			
			if(blogCol == null)
			{
				blogCol = BXBlog.GetList(new BXFilter(
					new BXFilterItem(BXBlog.Fields.Categories.CategoryId, BXSqlFilterOperators.Equal, categoryId)), null);
				
				if(BXTagCachingManager.IsTagCachingEnabled())
					BXCacheManager.MemoryCache.Application.Insert(key, blogCol,  new BXTagCachingDependency(BXBlogCategory.CreateTagById(categoryId)), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Low, null);
			}		
			f.Add(new BXSearchContentFilterItem(BXSearchField.ItemGroup, Bitrix.DataLayer.BXSqlFilterOperators.In, blogCol.ConvertAll<string>(x=>x.Id.ToString()).ToArray()));
		}
		
        if (blogId.Length > 0)
        {
            f.Add(new BXSearchContentFilterItem(BXSearchField.ItemGroup, Bitrix.DataLayer.BXSqlFilterOperators.Equal, blogId));
            cloud.Parameters["CacheCriterion"] = blogId;
        }
        else if (blogSlug.Length > 0)
        {
            BXBlog blog = null;
			string key = string.Concat("_BX_BLOG_TAG_CLOUD_BLOG_", blogSlug);			
			if(BXTagCachingManager.IsTagCachingEnabled())				
				blog = BXCacheManager.MemoryCache.Application.Get(key) as BXBlog;
			
			if(blog == null)			
			{
				blog = BXBlog.GetBySlug(blogSlug, null, new BXSelect(BXBlog.Fields.Id), BXTextEncoder.DefaultEncoder);
				if(blog != null && BXTagCachingManager.IsTagCachingEnabled())
					BXCacheManager.MemoryCache.Application.Insert(key, blog,  new BXTagCachingDependency(BXBlog.CreateTagById(blog.Id)), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Low, null);				
			}
            if (blog != null)
                f.Add(new BXSearchContentFilterItem(BXSearchField.ItemGroup, Bitrix.DataLayer.BXSqlFilterOperators.Equal, blog.Id));
            else
                f.Add(new BXSearchContentFilterItem(BXSearchField.Param1, Bitrix.DataLayer.BXSqlFilterOperators.Equal, blogSlug));
            cloud.Parameters["CacheCriterion"] = blogSlug;
        }

		q.Filter = f;
		cloud.ContentQuery = q;
	}
</script>

