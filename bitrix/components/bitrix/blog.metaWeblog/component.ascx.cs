using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.CommunicationUtility;
using Bitrix.Components;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Search;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Image;
using Bitrix.Services.Text;
using Bitrix.UI;

namespace Bitrix.Blog.Components
{
	public partial class BlogMetaWeblogComponent : BXComponent
	{
		string blogSlug;
		BXBlog blog;

		public string BlogSlug
		{
			get
			{
				return blogSlug ?? (blogSlug = Parameters.GetString("BlogSlug"));
			}
			set
			{
				blogSlug = value;
			}
		}
		public string BlogUrlTemplate
		{
			get
			{
				return Parameters.GetString("BlogUrlTemplate");
			}
			set
			{
				Parameters["BlogUrlTemplate"] = value;
			}
		}
		public string PostViewUrlTemplate
		{
			get
			{
				return Parameters.GetString("PostViewUrlTemplate");
			}
			set
			{
				Parameters["PostViewUrlTemplate"] = value;
			}
		}
		public int MaximalFileSizeInKBytes
		{
			get { return Parameters.GetInt("MaximalFileSizeInKBytes", 1024); }
			set { Parameters["MaximalFileSizeInKBytes"] = value.ToString(); }
		}
		public int MaximalAllFilesSizeInKBytes
		{
			get { return Parameters.GetInt("MaximalAllFilesSizeInKBytes", 8192); }
			set { Parameters["MaximalAllFilesSizeInKBytes"] = value.ToString(); }
		}
		public int MaximalImageWidthInPixels
		{
			get { return Parameters.GetInt("MaximalImageWidthInPixels", 0); }
			set { Parameters["MaximalImageWidthInPixels"] = value.ToString(); }
		}
		public int MaximalImageHeightInPixels
		{
			get { return Parameters.GetInt("MaximalImageHeightInPixels", 0); }
			set { Parameters["MaximalImageHeightInPixels"] = value.ToString(); }
		}
		public bool EnableExtendedEntries
		{
			get { return Parameters.GetBool("EnableExtendedEntries"); }
			set { Parameters["EnableExtendedEntries"] = value.ToString(); }
		}

		public BXBlog Blog
		{
			get
			{
				return blog ?? (blog = GetBlog());
			}
			set
			{
				blog = value;
			}
		}

		string blogUrl = null;
		public string BlogUrl
		{
			get
			{
				if (blogUrl == null)
				{
					BXParamsBag<object> replace = new BXParamsBag<object>();
					if (Blog != null)
					{
						replace["BlogId"] = Blog.Id;
						replace["BlogSlug"] = Blog.Slug;
					}

					blogUrl = new Uri(BXSefUrlManager.CurrentUrl, ResolveTemplateUrl(BlogUrlTemplate, replace)).AbsoluteUri;
				}

				return blogUrl;
			}
		}

		private BXBlog GetBlog()
		{
			if (BXStringUtility.IsNullOrTrimEmpty(BlogSlug) || !BXBlog.SlugRegex.IsMatch(BlogSlug))
				return null;

			BXFilter blogFilter = new BXFilter(
				new BXFilterItem(BXBlog.Fields.Slug, BXSqlFilterOperators.Equal, BlogSlug),
				new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
			);

			BXBlogCollection blogCollection = BXBlog.GetList(blogFilter, null, new BXSelectAdd(BXBlog.Fields.Owner.User, BXBlog.Fields.Owner.User.Image), null, BXTextEncoder.EmptyTextEncoder);
			if (blogCollection.Count == 0)
				return null;

			return blogCollection[0];
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (BXConfigurationUtility.ShowMode == BXShowMode.View && !IsComponentDesignMode)
			{
				bool valid = true;
				try
				{
					BXXmlRpcServer.ValidateRequest(Request);
				}
				catch
				{
					valid = false;
				}

				if (valid)
				{
					new Server(this, new Processor(this)).ProcessRequest(HttpContext.Current);
					return;
				}
			}

			BXPublicPage bitrixPage = Page as BXPublicPage;
			if (bitrixPage != null && Parameters.GetBool("SetPageTitle", true))
				bitrixPage.Title = GetMessage("PageTitle");

			IncludeComponentTemplate();
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Group = BXBlogModuleConfiguration.GetComponentGroup();

			BXCategory mainCategory = BXCategory.Main;
			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

			ParamsDefinition["BlogSlug"] = new BXParamText(GetMessageRaw("Param.BlogSlug"), "", mainCategory);


			ParamsDefinition["BlogUrlTemplate"] = new BXParamText(GetMessageRaw("Param.BlogUrlTemplate"), "Blog.aspx?blog=#blogSlug#", urlCategory);
			ParamsDefinition["PostViewUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostViewUrlTemplate"), "Post.aspx?post=#postId#", urlCategory);


			ParamsDefinition.Add("SetPageTitle", new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory));

			//File limitations
			ParamsDefinition.Add("MaximalFileSizeInKBytes", new BXParamText(GetMessageRaw("Param.MaximalFileSizeInKBytes"), "1024", additionalSettingsCategory));
			ParamsDefinition.Add("MaximalAllFilesSizeInKBytes", new BXParamText(GetMessageRaw("Param.MaximalAllFilesSizeInKBytes"), "8192", additionalSettingsCategory));
			//Image limitations
			ParamsDefinition.Add("MaximalImageWidthInPixels", new BXParamText(GetMessageRaw("Param.MaximalImageWidthInPixels"), "0", additionalSettingsCategory));
			ParamsDefinition.Add("MaximalImageHeightInPixels", new BXParamText(GetMessageRaw("Param.MaximalImageHeightInPixels"), "0", additionalSettingsCategory));

			ParamsDefinition["EnableExtendedEntries"] = new BXParamYesNo(GetMessageRaw("Param.EnableExtendedEntries"), false, additionalSettingsCategory);
		}


		class Processor : BXMetaWeblogProcessor
		{
			internal BXPrincipal user;
			internal BXIdentity identity;
			BlogMetaWeblogComponent c;

			public Processor(BlogMetaWeblogComponent component)
			{
				c = component;
			}

			private void Authenticate(string username, string password)
			{
				string provider;
				if (!BXAuthentication.Authenticate(username, password, out provider))
					throw new UserException("Username or password is invalid");

				identity = new BXIdentity(username, true, provider);
				user = new BXPrincipal(identity);
				try { HttpContext.Current.User = user; }
				catch { }
			}
			private void GetFileNameAndExtension(string fileName, out string name, out string ext)
			{
				int index = !string.IsNullOrEmpty(fileName) ? fileName.LastIndexOf('.') : -1;
				if (index >= 0)
				{
					name = fileName.Substring(0, index);
					ext = fileName.Substring(index + 1);
				}
				else
					name = ext = string.Empty;
			}
			private List<int> GetAttachedFiles(string content)
			{
				if (string.IsNullOrEmpty(content))
					return null;

				List<int> boundFileList = null;
				Regex rx = new Regex(@"<img\s+[^>]*?src\s*=\s*""(?<url>[^"">]*?)""[^>]*>|<a\s+[^>]*?href\s*=\s*""(?<url>[^"">]*?)""[^>]*>", RegexOptions.IgnoreCase);
				string uploadPath = VirtualPathUtility.ToAbsolute(VirtualPathUtility.AppendTrailingSlash(BXFile.UploadFolder)).ToLowerInvariant();

				for (Match m = rx.Match(content); m.Success; m = m.NextMatch())
				{
					Group g = m.Groups["url"];
					string url = string.Empty;
					if (!g.Success || string.IsNullOrEmpty(url = g.Value))
						continue;

					url = HttpUtility.HtmlDecode(url);
					try
					{
						try
						{
							url = new Uri(BXSefUrlManager.CurrentUrl, url).AbsolutePath;
						}
						catch
						{
						}
						url = url.ToLowerInvariant();

						if (!url.StartsWith(uploadPath))
							continue;
						string folder = string.Empty,
							name = string.Empty;
						url = url.Substring(uploadPath.Length);
						int whatInd = url.IndexOf('?');
						if (whatInd >= 0)
							url = url.Substring(0, whatInd);

						int lastSlashInd = url.LastIndexOf("/");
						if (lastSlashInd >= 0)
						{
							folder = url.Substring(0, lastSlashInd);
							name = url.Substring(lastSlashInd + 1);
						}
						else
							name = url;

						BXFilter f = new BXFilter();
						f.Add(new BXFilterItem(BXFile.Fields.OwnerModuleId, BXSqlFilterOperators.Equal, BXBlogModuleConfiguration.Id));
						if (string.IsNullOrEmpty(folder))
							f.Add(new BXFilterItem(BXFile.Fields.Folder, BXSqlFilterOperators.Equal, folder));
						f.Add(new BXFilterItem(BXFile.Fields.FileName, BXSqlFilterOperators.Equal, name));
						BXFileCollection c = BXFile.GetList(f, null, new BXSelect(BXSelectFieldPreparationMode.Normal, BXFile.Fields.Id), new BXQueryParams(new BXPagingOptions(0, 1)));
						if (c.Count == 0)
							continue;
						if (boundFileList == null)
							boundFileList = new List<int>();
						if (boundFileList.FindIndex(delegate(int obj) { return obj == c[0].Id; }) < 0)
							boundFileList.Add(c[0].Id);
					}
					catch
					{
					}
				}
				return boundFileList;
			}

			public override IEnumerable<BXMetaWeblogBlogInfo> GetUsersBlogs(string appKey, string username, string password)
			{
				Authenticate(username, password);
				BXBlog blog = c.GetBlog();
				if (blog == null)
					yield break;

				BXBlogAuthorization auth = new BXBlogAuthorization(blog);
				auth.SetAuthUser(user);

				if (!auth.CanReadThisBlog || !auth.CanCreatePost || !auth.CanWriteFilteredHtml)
					yield break;

				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace["BlogId"] = blog.Id;
				replace["BlogSlug"] = blog.Slug;

				BXMetaWeblogBlogInfo info = new BXMetaWeblogBlogInfo();
				info.Id = blog.Slug;
				info.Name = blog.Name;
				info.Url = new Uri(BXSefUrlManager.CurrentUrl, c.ResolveTemplateUrl(c.BlogUrlTemplate, replace)).ToString();

				yield return info;
			}
			public override IEnumerable<BXMetaWeblogPostInfo> GetRecentPosts(string blogId, string username, string password, int numberOfPosts)
			{
				if (string.IsNullOrEmpty(blogId))
					yield break;

				Authenticate(username, password);
				BXBlog blog = c.GetBlog();
				if (blog == null)
					yield break;

				if (blog.Slug != blogId)
					yield break;

				BXBlogAuthorization auth = new BXBlogAuthorization(blog);
				auth.SetAuthUser(user);

				if (!auth.CanReadThisBlog || !auth.CanCreatePost || !auth.CanWriteFilteredHtml)
					yield break;

				BXBlogPostCollection posts = BXBlogPost.GetList(
					new BXFilter(
						new BXFilterItem(BXBlogPost.Fields.Blog.Id, BXSqlFilterOperators.Equal, blog.Id)
					),
					new BXOrderBy(new BXOrderByPair(BXBlogPost.Fields.DateCreated, BXOrderByDirection.Desc)),
					null,
					new BXQueryParams(new BXPagingOptions(0, numberOfPosts)),
					BXTextEncoder.EmptyTextEncoder
				);

				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace["BlogId"] = blog.Id;
				replace["BlogSlug"] = blog.Slug;

				foreach (BXBlogPost p in posts)
				{
					replace["PostId"] = p.Id;

					BXMetaWeblogPostInfo info = new BXMetaWeblogPostInfo();
					info.Id = p.Id.ToString();
					info.Title = p.Title;
					info.Published = p.IsPublished;
					info.DateCreated = p.DatePublished;
					info.Description = p.Content;
					info.Categories = (p.Tags ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					info.Url = new Uri(BXSefUrlManager.CurrentUrl, c.ResolveTemplateUrl(c.PostViewUrlTemplate, replace)).ToString();
					yield return info;
				}
			}
			public override BXMetaWeblogPostInfo GetPost(string postId, string username, string password)
			{
				int id;
				if (!int.TryParse(postId, out id))
					throw new UserException(c.GetMessageRaw("Error.IncorrectPostId"));

				Authenticate(username, password);
				BXBlog blog = c.GetBlog();
				if (blog == null)
					throw new UserException(c.GetMessageRaw("Error.BlogNotFound"));

				BXBlogAuthorization auth = new BXBlogAuthorization(blog);
				auth.SetAuthUser(user);

				if (!auth.CanReadThisBlog || !auth.CanCreatePost || !auth.CanWriteFilteredHtml)
					throw new UserException(c.GetMessageRaw("Error.InsufficientPermissions"));


				BXBlogPostCollection posts = BXBlogPost.GetList(
					new BXFilter(
						new BXFilterItem(BXBlogPost.Fields.Id, BXSqlFilterOperators.Equal, id),
						new BXFilterItem(BXBlogPost.Fields.Blog.Id, BXSqlFilterOperators.Equal, blog.Id)
					),
					null,
					null,
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (posts.Count == 0)
					throw new Exception(c.GetMessageRaw("Error.PostNotFound"));

				BXBlogPost p = posts[0];

				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace["BlogId"] = blog.Id;
				replace["BlogSlug"] = blog.Slug;
				replace["PostId"] = p.Id;

				BXMetaWeblogPostInfo info = new BXMetaWeblogPostInfo();
				info.Id = p.Id.ToString();
				info.Title = p.Title;
				info.Published = p.IsPublished;
				info.DateCreated = p.DatePublished;

				string first, second;
				if (c.EnableExtendedEntries && SplitCut(p.Content, out first, out second))
				{
					info.Description = first;
					info.TextMore = second;
				}
				else
					info.Description = p.Content;
				info.Categories = (p.Tags ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				info.Url = new Uri(BXSefUrlManager.CurrentUrl, c.ResolveTemplateUrl(c.PostViewUrlTemplate, replace)).ToString();
				return info;
			}

			private bool SplitCut(string content, out string first, out string second)
			{
				first = second = null;
				var p = new BXBlogPostHtmlProcessor();
				int cutCount = 0;
				string lastCut = null;
				BXHtmlSanitizerTagInfo cutInfo = null;
				foreach(var e in p.Config.Tags)
				{
					if (string.Equals(e.Key, "cut", StringComparison.OrdinalIgnoreCase))
					{
						if (e.Value == null)
							return false;
						cutInfo = e.Value;
						
					}
					else if (e.Value != null)
						e.Value.CustomProcessor = null;
				}
				if (cutInfo == null)
					return false;
				cutInfo.CustomProcessor = delegate (BXHtmlSanitizerTagProcessArgs args)
				{
					args.CancelDefaultProcessing = true;
					args.Output.Append("<!-- cut-");
					args.Output.Append(cutCount++);
					args.Output.Append(" -->");
					lastCut = args.Content;
				};
								
				var html = p.Process(content);
				if (cutCount == 0)
					return false;
				
				int lastCutNum = cutCount - 1;
				var mark = string.Concat("<!-- cut-", lastCutNum.ToString(), " -->");
				int i = html.IndexOf(mark);
				if (i < 0)
					return false;

				for (int pos = i + mark.Length; pos < html.Length; pos++)
				{
					if (!char.IsWhiteSpace(html, pos))
						return false;
				}

				cutCount = 0;
				cutInfo.CustomProcessor = delegate (BXHtmlSanitizerTagProcessArgs args)
				{
					if (cutCount++ == lastCutNum)
						args.CancelDefaultProcessing = true;
				};
				first = p.Process(content);
				second = lastCut;
				return true;
			}
			public override string NewPost(string blogId, string username, string password, BXMetaWeblogPostInfo post)
			{
				if (string.IsNullOrEmpty(blogId))
					throw new UserException(c.GetMessageRaw("Error.IncorrectBlogId"));

				Authenticate(username, password);
				BXBlog blog = c.GetBlog();
				if (blog == null || blog.Slug != blogId)
					throw new UserException(c.GetMessageRaw("Error.BlogNotFound"));

				BXBlogAuthorization auth = new BXBlogAuthorization(blog);
				auth.SetAuthUser(user);

				bool fullHtml = false;
				if (!auth.CanReadThisBlog || !auth.CanCreatePost || (!auth.CanWriteFilteredHtml && !(fullHtml = auth.CanWriteFullHtml)))
					throw new UserException(c.GetMessageRaw("Error.InsufficientPermissions"));

				List<int> ids = GetAttachedFiles(post.Description);

				BXBlogPost p = new BXBlogPost(BXTextEncoder.EmptyTextEncoder);
				p.BlogId = blog.Id;
				p.IsPublished = post.Published;
				p.Title = post.Title;
				p.DatePublished = post.DateCreated != DateTime.MinValue ? post.DateCreated : DateTime.Now;
				
				var processor = fullHtml ? (IBXBlogPostProcessor2)new BXBlogPostFullHtmlProcessor() : new BXBlogPostHtmlProcessor();
				if (c.EnableExtendedEntries && !BXStringUtility.IsNullOrTrimEmpty(post.TextMore))
					p.Content = string.Concat(processor.Correct(post.Description), "<cut>", processor.Correct(post.TextMore), "</cut>");
				else 
					p.Content = processor.Correct(post.Description);
			
				p.ContentType = fullHtml ? BXBlogPostContentType.FullHtml : BXBlogPostContentType.FilteredHtml;
				p.AuthorId = ((BXIdentity)user.Identity).Id;
				p.Tags = string.Join(",", new List<string>(post.Categories).ToArray());
				p.SetFileIds(ids != null && ids.Count != 0 ? ids.ToArray() : null);
				p.Save();

				return p.Id.ToString();
			}
			public override bool DeletePost(string appKey, string postId, string username, string password, bool publish)
			{
				int id;
				if (!int.TryParse(postId, out id))
					return false;
				Authenticate(username, password);
				BXBlog blog = c.GetBlog();
				if (blog == null)
					return false;

				BXBlogAuthorization auth = new BXBlogAuthorization(blog);
				auth.SetAuthUser(user);

				if (!auth.CanReadThisBlog || !auth.CanCreatePost || !auth.CanDeleteThisPost)
					return false;

				BXBlogPost p = BXBlogPost.GetById(postId);
				if (p == null || p.BlogId != blog.Id)
					return false;
				p.Delete();
				return true;
			}
			public override IEnumerable<BXMetaWeblogCategoryInfo> GetCategories(string blogId, string username, string password)
			{
				if (string.IsNullOrEmpty(blogId))
					yield break;

				Authenticate(username, password);
				BXBlog blog = c.GetBlog();
				if (blog == null)
					yield break;

				if (blog.Slug != blogId)
					yield break;

				BXBlogAuthorization auth = new BXBlogAuthorization(blog);
				auth.SetAuthUser(user);

				if (!auth.CanReadThisBlog || !auth.CanCreatePost || !auth.CanWriteFilteredHtml)
					yield break;

				BXContentTagQuery q = new BXContentTagQuery();
				q.SearchQuery = new BXSearchQuery();
				q.SearchQuery.Filter = new BXSearchContentGroupFilter(
					BXFilterExpressionCombiningLogic.And,
					new BXSearchContentFilterItem(BXSearchField.ModuleId, BXSqlFilterOperators.Equal, "blog"),
					new BXSearchContentFilterItem(BXSearchField.ItemGroup, BXSqlFilterOperators.Equal, blog.Id.ToString())
				);
				q.Filter = new BXContentTagFilterItem(BXContentTagField.Status, BXSqlFilterOperators.NotEqual, BXContentTagStatus.Rejected);
				q.TextEncoder = BXTextEncoder.EmptyTextEncoder;
				BXContentTagCollection tags = q.Execute();

				foreach (BXContentTag input in tags)
				{
					BXMetaWeblogCategoryInfo info = new BXMetaWeblogCategoryInfo();
					info.Description = input.Name;
					yield return info;
				};
			}
			public override void EditPost(string username, string password, BXMetaWeblogPostInfo post)
			{
				int id;
				if (!int.TryParse(post.Id, out id))
					throw new UserException(c.GetMessageRaw("Error.IncorrectPostId"));

				Authenticate(username, password);
				BXBlog blog = c.GetBlog();
				if (blog == null)
					throw new UserException(c.GetMessageRaw("Error.BlogNotFound"));

				BXBlogAuthorization auth = new BXBlogAuthorization(blog);
				auth.SetAuthUser(user);

				bool fullHtml = false;
				if (!auth.CanReadThisBlog || !auth.CanCreatePost || (!auth.CanWriteFilteredHtml && !(fullHtml = auth.CanWriteFullHtml)))
					throw new UserException(c.GetMessageRaw("Error.InsufficientPermissions"));


				BXBlogPostCollection posts = BXBlogPost.GetList(
					new BXFilter(
						new BXFilterItem(BXBlogPost.Fields.Id, BXSqlFilterOperators.Equal, id),
						new BXFilterItem(BXBlogPost.Fields.Blog.Id, BXSqlFilterOperators.Equal, blog.Id)
					),
					null,
					null,
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (posts.Count == 0)
					throw new UserException(c.GetMessageRaw("Error.PostNotFound"));

				List<int> ids = GetAttachedFiles(post.Description);

				BXBlogPost p = posts[0];
				p.IsPublished = post.Published;
				p.Title = post.Title;
				if (post.DateCreated != DateTime.MinValue)
					p.DatePublished = post.DateCreated;

				var processor = fullHtml ? (IBXBlogPostProcessor2)new BXBlogPostFullHtmlProcessor() : new BXBlogPostHtmlProcessor();
				if (c.EnableExtendedEntries && !BXStringUtility.IsNullOrTrimEmpty(post.TextMore))
					p.Content = string.Concat(processor.Correct(post.Description), "<cut>", processor.Correct(post.TextMore), "</cut>");
				else 
					p.Content = processor.Correct(post.Description);

				p.ContentType = fullHtml ? BXBlogPostContentType.FullHtml : BXBlogPostContentType.FilteredHtml;
				p.AuthorId = ((BXIdentity)user.Identity).Id;
				p.Tags = string.Join(",", new List<string>(post.Categories).ToArray());
				p.SetFileIds(ids != null && ids.Count != 0 ? ids.ToArray() : null);
				p.Save();
			}
			public override BXMetaWeblogMediaObjectInfo NewMediaObject(string blogId, string username, string password, string name, string type, byte[] data)
			{
				if (string.IsNullOrEmpty(blogId))
					throw new UserException(c.GetMessageRaw("Error.IncorrectBlogId"));

				Authenticate(username, password);
				BXBlog blog = c.GetBlog();
				if (blog == null || blog.Slug != blogId)
					throw new UserException(c.GetMessageRaw("Error.BlogNotFound"));

				BXBlogAuthorization auth = new BXBlogAuthorization(blog);
				auth.SetAuthUser(user);

				if (!auth.CanReadThisBlog || !auth.CanCreatePost || !auth.CanWriteFilteredHtml)
					throw new UserException(c.GetMessageRaw("Error.InsufficientPermissions"));

				string orignalName = name;
				int lastBackSlashInd = orignalName.LastIndexOf('\\'); //FOR IE
				if (lastBackSlashInd >= 0)
					orignalName = orignalName.Substring(lastBackSlashInd + 1);

				string ext;
				GetFileNameAndExtension(orignalName, out name, out ext);
				if (string.IsNullOrEmpty(ext))
					throw new UserException(c.GetMessageRaw("Error.FileNoExtension"));

				string extUpper = ext.ToUpperInvariant();
				if (!(string.Equals(extUpper, "JPG", StringComparison.Ordinal)
					|| string.Equals(extUpper, "JPEG", StringComparison.Ordinal)
					|| string.Equals(extUpper, "GIF", StringComparison.Ordinal)
					|| string.Equals(extUpper, "PNG", StringComparison.Ordinal)))
					throw new UserException(c.GetMessageRaw("Error.FileInvalidExtension"));


				type = type.ToUpperInvariant();
				if (!string.Equals(type, "IMAGE/JPEG", StringComparison.InvariantCulture)
					&& !string.Equals(type, "IMAGE/PJPEG", StringComparison.InvariantCulture) //FOR IE
					&& !string.Equals(type, "IMAGE/JPG", StringComparison.InvariantCulture)
					&& !string.Equals(type, "IMAGE/GIF", StringComparison.InvariantCulture)
					&& !string.Equals(type, "IMAGE/PNG", StringComparison.InvariantCulture))
					throw new UserException(c.GetMessageRaw("Error.FileInvalidMimeType"));


				if (c.MaximalFileSizeInKBytes > 0)
				{
					//проверка размера файла
					if (data.LongLength > (c.MaximalFileSizeInKBytes * 1024))
						throw new UserException(string.Format(c.GetMessageRaw("Error.FileSizeExceeded"), data.LongLength / 1024, c.MaximalFileSizeInKBytes));
				}
				if (c.MaximalAllFilesSizeInKBytes > 0)
				{
					//проверка общего размера
					BXBlogPost2FileCollection presentFiles = null;

					BXFilter f = new BXFilter(new BXFilterItem(BXBlogPost2File.Fields.BlogId, BXSqlFilterOperators.Equal, blog.Id));
					f.Add(new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, null));

					presentFiles = BXBlogPost2File.GetList(
						f,
						new BXOrderBy(new BXOrderByPair(BXBlogPost2File.Fields.FileSize, BXOrderByDirection.Desc)),
						new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlogPost2File.Fields.FileSize),
						null,
						BXTextEncoder.EmptyTextEncoder
					);


					if (presentFiles.Count > 0)
					{
						int limit = c.MaximalAllFilesSizeInKBytes * 1024;
						long current = 0;
						foreach (BXBlogPost2File presentFile in presentFiles)
						{
							current += presentFile.FileSize;
							if (current >= limit)
								throw new UserException(string.Format(c.GetMessageRaw("Error.FileTotalSizeExceeded"), current / 1024, c.MaximalAllFilesSizeInKBytes));
						}
					}
				}

				BXFile file = null;
				using (MemoryStream fileStream = new MemoryStream(data))
				{
					string fileFolderRelativePath = string.Concat(BXBlogModuleConfiguration.ImageDirectoryName, "/", !string.IsNullOrEmpty(blog.Slug) ? blog.Slug : blog.Id.ToString());
					file = new BXFile(BXTextEncoder.EmptyTextEncoder, fileStream, orignalName, fileFolderRelativePath, BXBlogModuleConfiguration.Id, string.Empty, type);

					//проверка ширины и высоты
					if ((c.MaximalImageWidthInPixels <= 0 || file.Width <= c.MaximalImageWidthInPixels) && (c.MaximalImageHeightInPixels <= 0 || file.Height <= c.MaximalImageHeightInPixels))
						file.Create();
					else
					{
						using (MemoryStream ms = new MemoryStream())
						{
							BXImageUtility.Resize(fileStream, ms, c.MaximalImageWidthInPixels, c.MaximalImageHeightInPixels);
							file = new BXFile(BXTextEncoder.EmptyTextEncoder, ms, orignalName, fileFolderRelativePath, BXBlogModuleConfiguration.Id, string.Empty);
							file.Create();
						}
					}
				}

				BXBlogPost2File blogFile = new BXBlogPost2File();
				blogFile.Id = file.Id;
				blogFile.UserId = identity.Id;
				blogFile.BlogId = blog.Id;
				try
				{
					blogFile.Create();
				}
				catch
				{
					file.Delete();
					throw;
				}

				BXMetaWeblogMediaObjectInfo result = new BXMetaWeblogMediaObjectInfo();
				result.Url = BXUri.ToAbsoluteUri(file.FilePath);
				return result;
			}
			public override BXMetaWeblogUserInfo GetUserInfo(string appKey, string username, string password)
			{
				Authenticate(username, password);
				
				BXMetaWeblogUserInfo info = new BXMetaWeblogUserInfo();
				IBXTextEncoder t = identity.User.TextEncoder;

				info.Email = identity.User.Email;
				info.FirstName = t.Decode(identity.User.FirstName);
				info.LastName = t.Decode(identity.User.LastName);
				info.Nickname = t.Decode(identity.User.DisplayName);
				info.Url = "";
				info.UserId = identity.Id.ToString();
				return info;
			}
		}
		new class Server : BXMetaWeblogServer
		{
			BlogMetaWeblogComponent c;
			Processor p;

			public Server(BlogMetaWeblogComponent component, Processor processor)
				: base(processor)
			{
				c = component;
				p = processor;
			}
			
			protected override MethodFault ProcessError(Exception ex)
			{
				MethodFault mf;
				mf.Code = 2401;
				if (ex is UserException)
					mf.String = ex.Message;
				else if (p.user != null && p.user.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance))
					mf.String = ex.ToString();
				else
					mf.String = c.GetMessageRaw("Error.Unknown");
				return mf;
			}
		}
		class UserException : Exception
		{
			public UserException(string message) : base(message) { }
		}
	}

	public class BlogMetaWeblogTemplate : BXComponentTemplate<BlogMetaWeblogComponent>
	{

	}
}