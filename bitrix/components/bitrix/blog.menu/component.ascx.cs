using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Bitrix.Components;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services;
using System.Web.Caching;

namespace Bitrix.Blog.Components
{
	public partial class BlogMenuComponent : BXComponent
	{
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private List<MenuItem> menuItems;
		private BXBlog blog;
		private bool? checkUserPermissions;
		private BXBlogAuthorization auth;

		public ErrorCode FatalError
		{
			get
			{
				return fatalError;
			}
		}
		public string BlogSlug
		{
			get { return Parameters.GetString("BlogSlug"); }
			set { Parameters["BlogSlug"] = value; }
		}

		public int CurrencyId
		{
			get { return Parameters.GetInt("CurrencyId"); }
			set { Parameters["CurrencyId"] = value.ToString(); }
		}

		public string BlogIndexUrl
		{
			get
			{
				return Parameters.GetString("BlogIndexUrl");
			}
			set
			{
				Parameters["BlogIndexUrl"] = value;
			}
		}
		public string UserProfileUrlTemplate
		{
			get
			{
				return Parameters.GetString("UserProfileUrlTemplate");
			}
			set
			{
				Parameters["UserProfileUrlTemplate"] = value;
			}
		}
		public string NewPostUrlTemplate
		{
			get
			{
				return Parameters.GetString("NewPostUrlTemplate");
			}
			set
			{
				Parameters["NewPostUrlTemplate"] = value;
			}
		}
		public string UserBlogUrlTemplate
		{
			get
			{
				return Parameters.GetString("UserBlogUrlTemplate");
			}
			set
			{
				Parameters["UserBlogUrlTemplate"] = value;
			}
		}
		public string UserBlogSettingsUrlTemplate
		{
			get
			{
				return Parameters.GetString("UserBlogSettingsUrlTemplate");
			}
			set
			{
				Parameters["UserBlogSettingsUrlTemplate"] = value;
			}
		}
		public string DraftPostListUrlTemplate
		{
			get
			{
				return Parameters.GetString("DraftPostListUrlTemplate");
			}
			set
			{
				Parameters["DraftPostListUrlTemplate"] = value;
			}
		}
		public string NewBlogUrlTemplate
		{
			get
			{
				return Parameters.GetString("NewBlogUrlTemplate");
			}
			set
			{
				Parameters["NewBlogUrlTemplate"] = value;
			}
		}
		public bool CheckUserPermissions
		{
			get
			{
				return (checkUserPermissions ?? (checkUserPermissions = Parameters.GetBool("CheckUserPermissions"))).Value;
			}
			set
			{
				Parameters["CheckUserPermissions"] = value.ToString();
				checkUserPermissions = value;
			}
		}

		public int CategoryId
		{
			get { return Parameters.GetInt("CategoryId", 0); }
			set { Parameters["CategoryId"] = value.ToString(); }
		}


		public List<MenuItem> MenuItems
		{
			get
			{
				return menuItems;
			}
		}
		public BXBlog Blog
		{
			get { return blog; }
			set { blog = value; }
		}
		private BXBlogAuthorization Auth
		{
			get{ return auth ?? (auth = new BXBlogAuthorization(blog));}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;
			AbortCache();

			if (!Visible)
				return;

			string key = string.Empty;
			string blogSlug = BlogSlug;
			if(!string.IsNullOrEmpty(blogSlug))
				key = string.Concat("_BX_BLOG_MENU_BLOG_", blogSlug);
			else
				key = string.Concat("_BX_BLOG_MENU_BLOG_", BXIdentity.Current.Id, "_", DesignerSite);

			if(blog == null && BXTagCachingManager.IsTagCachingEnabled())
				blog = BXCacheManager.MemoryCache.Application.Get(key) as BXBlog;

			if(blog == null)
			{
				BXFilter filter;
				if (!string.IsNullOrEmpty(blogSlug))
				{
					filter = new BXFilter(
						new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true),
						new BXFilterItem(BXBlog.Fields.Slug, BXSqlFilterOperators.Equal, blogSlug),
						new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.Site.ID, BXSqlFilterOperators.Equal, DesignerSite)
					);
				}
				else
				{
					int userId = BXIdentity.Current.Id;

					if (userId < 1)
					{
						fatalError = ErrorCode.Unauthorized;
						return;
					}

					filter = new BXFilter(
						new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true),
						new BXFilterItem(BXBlog.Fields.Owner.Id, BXSqlFilterOperators.Equal, userId),
						new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.Site.ID, BXSqlFilterOperators.Equal, DesignerSite),
						new BXFilterItem(BXBlog.Fields.IsTeam, BXSqlFilterOperators.Equal, false)
					);
				}

				if (CategoryId > 0)
					filter.Add(new BXFilterItem(BXBlog.Fields.Categories.CategoryId, BXSqlFilterOperators.Equal, CategoryId));

				BXBlogCollection blogCollection = BXBlog.GetList(
					filter,
					null
				);

				if (blogCollection.Count < 1)
				{
					fatalError = ErrorCode.BlogNotFound;
					return;
				}

				blog = blogCollection[0];
				if(BXTagCachingManager.IsTagCachingEnabled())
					BXCacheManager.MemoryCache.Application.Insert(key, blog,  new BXTagCachingDependency(BXBlog.CreateTagById(blog.Id)), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Low, null);				
			}

			menuItems = new List<MenuItem>();

			if (!String.IsNullOrEmpty(NewPostUrlTemplate) && (!CheckUserPermissions || Auth.CanCreatePost))
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.NewPost"), ResolveTemplateUrlDelegate(NewPostUrlTemplate, ResolveValue), "newpost", ""));

			if (!String.IsNullOrEmpty(DraftPostListUrlTemplate) && (!CheckUserPermissions || Auth.CanReadThisBlogDrafts))
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Draft"), ResolveTemplateUrlDelegate(DraftPostListUrlTemplate, ResolveValue), "draft", ""));

			if (!String.IsNullOrEmpty(UserBlogUrlTemplate) && (!CheckUserPermissions || Auth.CanReadThisBlog))
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.UserBlog"), ResolveTemplateUrlDelegate(UserBlogUrlTemplate, ResolveValue), "user-blog", ""));

			if (!String.IsNullOrEmpty(UserProfileUrlTemplate))
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Profile"), ResolveTemplateUrlDelegate(UserProfileUrlTemplate, ResolveValue), "profile", ""));

			if (!String.IsNullOrEmpty(UserBlogSettingsUrlTemplate) && (!CheckUserPermissions || Auth.CanEditThisBlogSettings))
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.UserBlogSettings"), ResolveTemplateUrlDelegate(UserBlogSettingsUrlTemplate, ResolveValue), "blog-settings", ""));

			if (!String.IsNullOrEmpty(NewBlogUrlTemplate) && Auth.CanCreateTeamBlog)
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.CreateBlog"), ResolveTemplateUrlDelegate(NewBlogUrlTemplate, ResolveValue), "create-team-blog", ""));

			IncludeComponentTemplate();
		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (!Visible)
				return;
			foreach (string param in new string[] { Parameters.GetString("ThemeCssFilePath"), Parameters.GetString("ColorCssFilePath") })
			{
				if (BXStringUtility.IsNullOrTrimEmpty(param))
					continue;

				string path = param;
				try
				{
					path = BXPath.ToVirtualRelativePath(path);
					if (BXSecureIO.FileExists(path))
						BXPage.RegisterStyle(path);
				}
				catch { }
			}
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = BXBlogModuleConfiguration.GetComponentGroup();

			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory mainCategory = BXCategory.Main;

			ParamsDefinition.Add("ThemeCssFilePath", new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/style.css", mainCategory));
			ParamsDefinition.Add("ColorCssFilePath", new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", mainCategory));
			ParamsDefinition.Add("BlogSlug", new BXParamText(GetMessageRaw("Param.BlogSlug"), "", mainCategory));
			ParamsDefinition["CategoryId"] = new BXParamSingleSelection(GetMessageRaw("Param.CategoryId"), "", mainCategory);
			ParamsDefinition.Add("NewPostUrlTemplate", new BXParamText(GetMessageRaw("Param.NewPostUrlTemplate"), "", urlCategory));
			ParamsDefinition.Add("DraftPostListUrlTemplate", new BXParamText(GetMessageRaw("Param.DraftPostListUrlTemplate"), "", urlCategory));
			ParamsDefinition.Add("UserBlogUrlTemplate", new BXParamText(GetMessageRaw("Param.UserBlogUrlTemplate"), "", urlCategory));
			ParamsDefinition.Add("UserBlogSettingsUrlTemplate", new BXParamText(GetMessageRaw("Param.UserBlogSettingsUrlTemplate"), "", urlCategory));
			ParamsDefinition.Add("BlogIndexUrl", new BXParamText(GetMessageRaw("Param.BlogIndexUrl"), "", urlCategory));
			ParamsDefinition.Add("UserProfileUrlTemplate", new BXParamText(GetMessageRaw("Param.UserProfileUrlTemplate"), "", urlCategory));
			ParamsDefinition.Add("NewBlogUrlTemplate", new BXParamText(GetMessageRaw("Param.NewBlogUrlTemplate"), "", urlCategory));			
		}

		protected override void LoadComponentDefinition()
		{
			IList<BXParamValue> categoryIds = ((BXParamSingleSelection)ParamsDefinition["CategoryId"]).Values;
			if (categoryIds.Count > 0)
				categoryIds.Clear();

			BXBlogCategoryCollection categories = BXBlogCategory.GetList(
			   new BXFilter(new BXFilterItem(BXBlogCategory.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)),
			   new BXOrderBy(new BXOrderByPair(BXBlogCategory.Fields.Name, BXOrderByDirection.Asc)),
			   new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlogCategory.Fields.Id, BXBlogCategory.Fields.Name),
			   null,
			   BXTextEncoder.EmptyTextEncoder
			   );
			categoryIds.Add(new BXParamValue(GetMessage("Kernel.All"), ""));
			foreach (BXBlogCategory category in categories)
			{
				BXParamValue value = new BXParamValue(category.Name, category.Id.ToString());
				categoryIds.Add(value);
			}
		}

		private string ResolveValue(string key)
		{
			if (fatalError != ErrorCode.None)
				return null;

			if (string.Equals(key, "UserId", StringComparison.OrdinalIgnoreCase))
				return blog.OwnerId.ToString();
			else if (string.Equals(key, "BlogSlug", StringComparison.OrdinalIgnoreCase))
				return blog.Slug ?? "";
			else if (string.Equals(key, "BlogId", StringComparison.OrdinalIgnoreCase))
				return blog.Id.ToString();

			return null;
		}

		public class MenuItem
		{
			private string href;
			private string titleHtml;
			private string className;
			private string tooltipHtml;

			public string TooltipHtml
			{
				get
				{
					return tooltipHtml;
				}
				internal set
				{
					tooltipHtml = value;
				}
			}
			public string ClassName
			{
				get
				{
					return className;
				}
				internal set
				{
					className = value;
				}
			}
			public string TitleHtml
			{
				get
				{
					return titleHtml;
				}
				internal set
				{
					titleHtml = value;
				}
			}
			public string Href
			{
				get
				{
					return href;
				}
				internal set
				{
					href = value;
				}
			}

			internal MenuItem(string title, string url, string cssClass, string tooltip)
			{
				titleHtml = title != null ? HttpUtility.HtmlEncode(title) : null;
				href = url != null ? HttpUtility.HtmlEncode(url) : null;
				className = cssClass != null ? HttpUtility.HtmlEncode(cssClass) : null;
				tooltipHtml = tooltip != null ? HttpUtility.HtmlEncode(tooltip) : null;
			}
		}

		public enum ErrorCode
		{
			None = 0,
			Unauthorized = 1,
			BlogNotFound = 2,
			FatalComponentNotExecuted = 4
		}
	}

	public class BlogMenuTemplate : BXComponentTemplate<BlogMenuComponent>
	{
		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			base.Render(writer);
		}
	}
}
