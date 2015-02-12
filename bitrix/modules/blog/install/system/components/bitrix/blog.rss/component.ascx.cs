using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.DataLayer;
using Bitrix.Services.Syndication;
using Bitrix.DataTypes;
using System.IO;
using System.Xml;
using System.Text;
using Bitrix.Configuration;
using Bitrix.Services;
using Bitrix.Security;
using Bitrix.Services.Text;

namespace Bitrix.Blog.Components
{

	/// <summary>
	/// Параметр компонента "RSS блога"
	/// </summary>
	public enum BlogRssParameter
	{
		/// <summary>
		/// Тип содержания (что выгружаем?)
		/// </summary>
		StuffType = 1,
		/// <summary>
		/// Тип фильтрации
		/// </summary>
		FiltrationType,
		/// <summary>
		/// Ид категории
		/// </summary>
		CategoryId,
		/// <summary>
		/// Псевдоним блога
		/// </summary>
		BlogSlug,
		/// <summary>
		/// Ид сообщения
		/// </summary>
		PostId,
		/// <summary>
		/// Отображать содержимое врезки сообщения блога
		/// </summary>
		EnablePostCut,
		/// <summary>
		/// Кол-во эл-тов
		/// </summary>
		ItemQuantity,
		/// <summary>
		/// Шаблон ссылки на элемент канала
		/// </summary>
		FeedItemUrlTemplate,
		/// <summary>
		/// Шаблон ссылки на канал
		/// </summary>
		FeedUrlTemplate,
		/// <summary>
		/// Заголовок канала
		/// </summary>
		FeedTitle,
		/// <summary>
		/// Описание канала
		/// </summary>
		FeedDescription,
		/// <summary>
		/// Фильтрация по полям сообщений
		/// </summary>
		FilterByPostCustomProperty,
		/// <summary>
		/// Настройки фильтрации по полям сообщений
		/// </summary>
		PostCustomPropertyFilterSettings,
		/// <summary>
		/// Фильтрация по тегам сообщений
		/// </summary>
		FilterByPostTag,
		/// <summary>
		/// Настройки фильтрации по тегам сообщений
		/// </summary>
		PostTagFilterSettings,
		/// <summary>
		/// Не использовать CDATA
		/// </summary>
		NoCData
	}

	/// <summary>
	/// Тип содержания
	/// </summary>
	public enum BlogRssStuffType
	{
		/// <summary>
		/// Сообщения
		/// </summary>
		Post = 1,
		/// <summary>
		/// Комментарии
		/// </summary>
		Comment
	}

	/// <summary>
	/// Тип фильтрации
	/// </summary>
	public enum BlogRssFiltrationType
	{
		/// <summary>
		/// без фильтрации
		/// </summary>
		None = 0,
		/// <summary>
		/// по Ид категории
		/// </summary>
		CategoryId,
		/// <summary>
		/// по псевдониму блога
		/// </summary>
		BlogSlug,
		/// <summary>
		/// по Ид сообщения
		/// </summary>
		PostId
	}

	/// <summary>
	/// Ошибка
	/// </summary>
	public enum BlogRssError
	{
		None = 0,
		CategoryNotFound = -1,
		BlogNotFound = -2,
		PostNotFound = -3,
		NoData = -4
	}

	/// <summary>
	/// Компонент RSS блога
	/// </summary>
	public partial class BlogRssComponent : BXComponent
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			IncludeComponentTemplate();

			BXBlogUserPermissionResult result = null;
			if (StuffType == BlogRssStuffType.Post)
				result = BXBlogUserPermissions.GetBlogsForPosts(BXIdentity.Current.Id, BXBlogPostPermissionLevel.Read, true, Page.Items, null);					

			if (IsCached(result))
				return;

			PrepareFeed(result);

			if (ComponentError != BlogRssError.None)
				AbortCache();
		}

		private BlogRssError _error = BlogRssError.None;
		/// <summary>
		/// Ошибка
		/// </summary>
		public BlogRssError ComponentError
		{
			get { return _error; }
		}

		/// <summary>
		/// Текст ошибки
		/// </summary>
		public string ComponentErrorText
		{
			get
			{
				if (_error == BlogRssError.None)
					return string.Empty;
				else if (_error == BlogRssError.CategoryNotFound)
					return GetMessageRaw("Error.CategoryNotFound");
				else if (_error == BlogRssError.BlogNotFound)
					return GetMessageRaw("Error.BlogNotFound");
				else if (_error == BlogRssError.PostNotFound)
					return GetMessageRaw("Error.PostNotFound");
				else if (_error == BlogRssError.NoData)
					return GetMessageRaw("Error.NoData");
				else
					return GetMessageRaw("Error.General");
			}
		}

		/// <summary>
		/// Тип содержания
		/// </summary>
		public BlogRssStuffType StuffType
		{
			get
			{
				string parVal = Parameters.Get(GetParameterKey(BlogRssParameter.StuffType));
				if (string.IsNullOrEmpty(parVal))
					return BlogRssStuffType.Post;
				BlogRssStuffType result = BlogRssStuffType.Post;
				try
				{
					result = (BlogRssStuffType)Enum.Parse(typeof(BlogRssStuffType), parVal);
				}
				catch (Exception /*exc*/)
				{
				}
				return result;
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.StuffType)] = value.ToString("G");
			}
		}

		/// <summary>
		/// Тип фильтрации
		/// </summary>
		public BlogRssFiltrationType FiltrationType
		{
			get
			{
				string parVal = Parameters.Get(GetParameterKey(BlogRssParameter.FiltrationType));
				if (string.IsNullOrEmpty(parVal))
					return BlogRssFiltrationType.None;
				BlogRssFiltrationType result = BlogRssFiltrationType.None;
				try
				{
					result = (BlogRssFiltrationType)Enum.Parse(typeof(BlogRssFiltrationType), parVal);
				}
				catch (Exception /*exc*/)
				{
				}
				return result;
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.FiltrationType)] = value.ToString("G");
			}
		}

		/// <summary>
		/// Ид категории
		/// </summary>
		public int CategoryId
		{
			get
			{
				string parVal = Parameters.Get(GetParameterKey(BlogRssParameter.CategoryId));
				if (string.IsNullOrEmpty(parVal))
					return 0;
				int result = 0;
				try
				{
					result = Convert.ToInt32(parVal);
				}
				catch (Exception /*exc*/)
				{
				}
				return result;
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.CategoryId)] = value.ToString();
			}
		}

		/// <summary>
		/// Псевдоним блога
		/// </summary>         
		public string BlogSlug
		{
			get
			{
				return Parameters.Get(GetParameterKey(BlogRssParameter.BlogSlug), string.Empty);
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.BlogSlug)] = value;
			}
		}

		/// <summary>
		/// Ид сообщения
		/// </summary>         
		public int PostId
		{
			get
			{
				string parVal = Parameters.Get(GetParameterKey(BlogRssParameter.PostId));
				if (string.IsNullOrEmpty(parVal))
					return 0;
				int result = 0;
				try
				{
					result = Convert.ToInt32(parVal);
				}
				catch (Exception /*exc*/)
				{
				}
				return result;
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.PostId)] = value.ToString();
			}
		}

		/// <summary>
		/// Отображать содержимое врезки
		/// </summary>
		public bool EnablePostCut
		{
			get
			{
				return Parameters.GetBool(GetParameterKey(BlogRssParameter.EnablePostCut), false);
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.EnablePostCut)] = value.ToString();
			}
		}

		/// <summary>
		/// Количество элементов
		/// </summary>         
		public int ItemQuantity
		{
			get
			{
				object o = null;
				if (ComponentCache.TryGetValue(GetParameterKey(BlogRssParameter.ItemQuantity), out o))
					return (int)o;

				int n = Parameters.GetInt(GetParameterKey(BlogRssParameter.ItemQuantity), 10);
				if (n <= 0 || n > 100)
					n = 10;
				ComponentCache[GetParameterKey(BlogRssParameter.ItemQuantity)] = n;
				return n;
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.ItemQuantity)] = value.ToString();
				ComponentCache[GetParameterKey(BlogRssParameter.ItemQuantity)] = value;
			}
		}

		/// <summary>
		/// Шаблон ссылки на канал
		/// </summary>
		public string FeedUrlTemplate
		{
			get
			{
				return Parameters.Get(GetParameterKey(BlogRssParameter.FeedUrlTemplate), string.Empty);
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.FeedUrlTemplate)] = value;
			}
		}

		/// <summary>
		/// Шаблон ссылки на элемент канала
		/// </summary>
		public string FeedItemUrlTemplate
		{
			get
			{
				return Parameters.Get(GetParameterKey(BlogRssParameter.FeedItemUrlTemplate), string.Empty);
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.FeedItemUrlTemplate)] = value;
			}
		}

		/// <summary>
		/// Заголовок канала
		/// </summary>
		public string FeedTitle
		{
			get
			{
				return Parameters.Get(GetParameterKey(BlogRssParameter.FeedTitle));
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.FeedTitle)] = value;
			}
		}

		/// <summary>
		/// Описание канала
		/// </summary>
		public string FeedDescription
		{
			get
			{
				return Parameters.Get(GetParameterKey(BlogRssParameter.FeedDescription));
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.FeedDescription)] = value;
			}
		}

		/// <summary>
		/// Фильтрация по пользовательским свойствам сообщений
		/// </summary>
		public bool FilterByPostCustomProperty
		{
			get
			{
				return Parameters.GetBool("FilterByPostCustomProperty", false);
			}
			set
			{
				Parameters["FilterByPostCustomProperty"] = value.ToString();
			}
		}

		/// <summary>
		/// Настройки фильтрации по полям сообщений
		/// </summary>
		public BXParamsBag<object> PostCustomPropertyFilterSettings
		{
			get
			{
				return BXParamsBag<object>.FromString(Parameters.GetString("PostCustomPropertyFilterSettings", string.Empty));
			}
			set
			{
				Parameters["PostCustomPropertyFilterSettings"] = value.ToString();
			}
		}

		/// <summary>
		/// Фильтровать по тегам сообщений
		/// </summary>
		public bool FilterByPostTag
		{
			get
			{
				return Parameters.GetBool(GetParameterKey(BlogRssParameter.FilterByPostTag), false);
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.FilterByPostTag)] = value.ToString();
			}
		}

		/// <summary>
		/// Настройки фильтрации по тегам сообщений
		/// </summary>
		public string PostTagFilterSettings
		{
			get
			{
				object obj = null;
				if (ComponentCache.TryGetValue(GetParameterKey(BlogRssParameter.PostTagFilterSettings), out obj))
					return (string)obj;

				string s = Parameters.Get(GetParameterKey(BlogRssParameter.PostTagFilterSettings), string.Empty);
				ComponentCache[GetParameterKey(BlogRssParameter.PostTagFilterSettings)] = s;
				return s;

			}
			set
			{
				ComponentCache[GetParameterKey(BlogRssParameter.PostTagFilterSettings)] = value;
				Parameters[GetParameterKey(BlogRssParameter.PostTagFilterSettings)] = value;
			}
		}

		/// <summary>
		/// Использовать CDATA
		/// </summary>
		public bool UseCData
		{
			get
			{
				return !Parameters.GetBool(GetParameterKey(BlogRssParameter.NoCData), false);
			}
			set
			{
				Parameters[GetParameterKey(BlogRssParameter.NoCData)] = (!value).ToString();
			}
		}

		private static bool? _isSearchModuleInstalled = null;
		private static bool IsSearchModuleInstalled
		{
			get
			{
				return (_isSearchModuleInstalled ?? (_isSearchModuleInstalled = Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search"))).Value;
			}
		}

		private BXBlogPostChain _postBbcodeProcessor = null;
		private BXBlogPostChain PostBBCodeProcessor
		{
			get
			{
				if (_postBbcodeProcessor != null)
					return _postBbcodeProcessor;

				_postBbcodeProcessor = new BXBlogPostChain();
				_postBbcodeProcessor.MaxWordLength = 0;
				_postBbcodeProcessor.RenderBeginCut += new EventHandler<BXBlogCutTagEventArgs>(RenderBeginCut);
				return _postBbcodeProcessor;
			}
		}

		private BXBlogPostHtmlProcessor _postHtmlProcessor = null;
		private BXBlogPostHtmlProcessor PostHtmlProcessor
		{
			get
			{
				if (_postHtmlProcessor != null)
					return _postHtmlProcessor;

				_postHtmlProcessor = new BXBlogPostHtmlProcessor();
				_postHtmlProcessor.RenderBeginCut += new EventHandler<BXBlogCutTagEventArgs>(RenderBeginCut);
				return _postHtmlProcessor;
			}
		}

		private BXBlogPostFullHtmlProcessor _postFullHtmlProcessor = null;
		private BXBlogPostFullHtmlProcessor PostFullHtmlProcessor
		{
			get
			{
				if (_postFullHtmlProcessor != null)
					return _postFullHtmlProcessor;

				_postFullHtmlProcessor = new BXBlogPostFullHtmlProcessor();
				_postFullHtmlProcessor.RenderBeginCut += new EventHandler<BXBlogCutTagEventArgs>(RenderBeginCut);
				return _postFullHtmlProcessor;
			}
		}

		private int cutCount = 0;
		private void RenderBeginCut(object sender, BXBlogCutTagEventArgs e)
		{
			cutCount++;
		}

		protected IBXBlogPostProcessor GetPostProcessor(BXBlogPost post, Uri postUrl)
		{
			IBXBlogPostProcessor r;			
			if (post.ContentType == BXBlogPostContentType.FullHtml)
				r = PostFullHtmlProcessor;
			else if (post.ContentType == BXBlogPostContentType.FilteredHtml)
				r = PostHtmlProcessor;
			else
				r = PostBBCodeProcessor;

			r.ParentUrl = postUrl;
			return r;
		}

		private BXBlogCommentChain _commentProcessor = null;
		protected BXBlogCommentChain CommentProcessor
		{
			get
			{
				if (_commentProcessor != null)
					return _commentProcessor;
				_commentProcessor = new BXBlogCommentChain();
				_commentProcessor.MaxWordLength = 0;
				return _commentProcessor;
			}
		}

		private Uri _uri;
		private string Convert2AbsoluteUrl(string url)
		{
			return !string.IsNullOrEmpty(url) && Uri.TryCreate(Request.Url, Page.ResolveUrl(url), out _uri) ? _uri.ToString() : VirtualPathUtility.ToAbsolute("~/");
		}

		protected string GetParameterKey(BlogRssParameter parameter)
		{
			return parameter.ToString("G");
		}

		protected string GetParameterTitle(BlogRssParameter parameter)
		{
			return GetMessageRaw(string.Concat("Param.", parameter.ToString("G")));
		}

		protected string GetParameterValueTitle(BlogRssParameter parameter, string valueKey)
		{
			return GetMessageRaw(string.Concat("Param.", parameter.ToString("G"), ".", valueKey));
		}

		private string currentSiteUrl = null;
		protected string GetCurrentSiteUrl()
		{
			//return VirtualPathUtility.ToAbsolute("~/");
			if (currentSiteUrl != null)
				return currentSiteUrl;

			string url = Request.Url.AbsoluteUri;
			int pathInd = url.IndexOf(Request.Url.PathAndQuery);
			return currentSiteUrl = pathInd >= 0 ? string.Format("{0}{1}", url.Substring(0, pathInd), VirtualPathUtility.ToAbsolute("~/")) : url;
		}

		private BXSite _currentSite = null;
		protected BXSite CurrentSite
		{
			get { return _currentSite ?? (_currentSite = BXSite.Current); }
		}

		//private event EventHandler<BXBlogCutTagEventArgs> RenderHideCut;

		/// <summary>
		/// Подготовить RSS канал
		/// </summary>
		private void PrepareFeed(BXBlogUserPermissionResult postsPermissions)
		{
			_feedSavingOptions = new BXSyndicationElementXmlSavingOptions();
			_feedSavingOptions.UseCData = UseCData;

			_feedWrapper = BXRss20Feed.Create();
			//atom
			_feedWrapper.XmlNamespaces.Add(BXAtom10Element.XmlNameSpace);
			//content
			_feedWrapper.XmlNamespaces.Add(BXContentElement.XmlNameSpace);
			//dc
			_feedWrapper.XmlNamespaces.Add(BXDublinCoreElement.XmlNameSpace);

			_feed = _feedWrapper.CreateChannel();
			_feed.Link = GetCurrentSiteUrl();
			_feed.Title = FeedTitle;
			_feed.Description = FeedDescription;
			_feed.LastBuildDate = DateTime.Now.ToUniversalTime();
			_feed.Language = CurrentSite.GetCultureInfo().Name;
			_feed.Generator = "bitrix::blog.rss";

			BXAtom10Link feedSelfLink = BXAtom10Link.Create();
			feedSelfLink.Rel = BXAtom10LinkRelationType.Self;
			feedSelfLink.Type = "application/rss+xml";
			_feed.ElementExtensions.Add(feedSelfLink);

			bool isPermitted = false;
			BXRoleCollection roles = BXRoleManager.GetAllRolesForOperation(BXBlog.Operations.BlogPublicView, BXBlogModuleConfiguration.Id, string.Empty);
			foreach (BXRole role in roles)
			{
				if (!string.Equals(role.RoleName, "Guest", StringComparison.InvariantCulture))
					continue;
				isPermitted = true;
				break;
			}

			if (!isPermitted)
			{
				_error = BlogRssError.NoData;
				return;
			}

			BlogRssStuffType stuffType = StuffType;
			using (BXTagCachingScope scope = BeginTagCaching())
			{
				switch (stuffType)
				{
					case BlogRssStuffType.Post:
						PreparePosts(scope, feedSelfLink, postsPermissions);
						break;
					case BlogRssStuffType.Comment:
						PrepareComments(scope, feedSelfLink);
						break;
					default:
						throw new NotSupportedException(string.Format("Stuff type '{0}' is unknown in current context!", stuffType.ToString("G")));
				}
			}
		}

		void PreparePosts(BXTagCachingScope scope, BXAtom10Link feedSelfLink, BXBlogUserPermissionResult bp)
		{
			BXParamsBag<object> replaceParams = new BXParamsBag<object>();
			string feedUrlTemplate = FeedUrlTemplate;
			string feedLink = string.Empty;
			BXFilter f = null;
			BXOrderBy o = null;
			Comparison<BXBlogPost> sort = null;
						
			bool hasBlogs = bp.Exclude || bp.Ids.Count > 0;
			
			if (IsSearchModuleInstalled && FilterByPostTag && PostTagFilterSettings.Length > 0)
			{
				scope.AddTag(BXBlogCategory.CreateSpecialTag(BXCacheSpecialTagType.All));

				IBlogRssComponentTagFilter tagFilter = (IBlogRssComponentTagFilter)LoadControl("tagfilter.ascx");
				tagFilter.InitFilter(DesignerSite, PostTagFilterSettings, bp.Ids, bp.Exclude, FiltrationType == BlogRssFiltrationType.CategoryId ? CategoryId : 0);
				IList<int> tagIdLst = tagFilter.GetPostIdList(new BXPagingOptions(0, ItemQuantity));
				f = new BXFilter(
					new BXFilterItem(
						BXBlogPost.Fields.Id,
						BXSqlFilterOperators.In,
						tagIdLst
					)
				);

				sort =
					delegate(BXBlogPost a, BXBlogPost b)
					{
						return tagIdLst.IndexOf(a.Id) - tagIdLst.IndexOf(b.Id);
					};
			}
			else
			{
				BlogRssFiltrationType filtrationType = FiltrationType;
				switch (filtrationType)
				{
					case BlogRssFiltrationType.None:
					case BlogRssFiltrationType.PostId: //игнорируется, т.к. получается "Новые сообщения из сообщения"
						{
							scope.AddTag(BXBlogCategory.CreateSpecialTag(BXCacheSpecialTagType.All));

							if (string.IsNullOrEmpty(_feed.Title))
								_feed.Title = GetMessageRaw("Param.FeedTitle.Default");
							if (string.IsNullOrEmpty(_feed.Description))
								_feed.Description = string.Format(GetMessageRaw("Param.FeedDescription.Default"), CurrentSite.Name);

							f = new BXFilter(new BXFilterItem(BXBlogPost.Fields.Blog.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, CurrentSite.Id));
						}
						break;
					case BlogRssFiltrationType.CategoryId:
						{
							BXBlogCategory category = null;
							int categoryId = CategoryId;
							if (categoryId > 0 && (category = BXBlogCategory.GetById(categoryId, BXTextEncoder.EmptyTextEncoder)) != null)
							{
								category.PrepareForTagCaching(scope);

								f = new BXFilter(new BXFilterItem(BXBlogPost.Fields.Blog.Categories.CategoryId, BXSqlFilterOperators.Equal, categoryId));
								if (!string.IsNullOrEmpty(feedUrlTemplate))
								{
									replaceParams["CategoryId"] = categoryId;
									replaceParams["BlogId"] = string.Empty;
									replaceParams["BlogSlug"] = string.Empty;
									replaceParams["PostId"] = string.Empty;
									feedLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
								}
							}
							else
							{
								_error = BlogRssError.CategoryNotFound;
								return;
							}

							if (string.IsNullOrEmpty(_feed.Title))
								_feed.Title = string.Format(GetMessageRaw("FeedTitle.NewBlogPostsInCategory"), category.Name);
							if (string.IsNullOrEmpty(_feed.Description))
								_feed.Description = string.Format(GetMessageRaw("FeedDescription.NewBlogPostsInCategory"), category.Name, CurrentSite.Name);
						}
						break;
					case BlogRssFiltrationType.BlogSlug:
						{
							BXBlog blog = null;
							string blogSlug = BlogSlug;
							if (!string.IsNullOrEmpty(blogSlug) && (blog = BXBlog.GetBySlug(blogSlug, null, null, BXTextEncoder.EmptyTextEncoder)) != null)
							{
								blog.PrepareForTagCaching(scope);

								f = new BXFilter(new BXFilterItem(BXBlogPost.Fields.Blog.Slug, BXSqlFilterOperators.Equal, blogSlug));
								if (!string.IsNullOrEmpty(feedUrlTemplate))
								{
									replaceParams["CategoryId"] = string.Empty;
									replaceParams["BlogId"] = blog.Id;
									replaceParams["BlogSlug"] = blogSlug;
									replaceParams["PostId"] = string.Empty;
									feedLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
								}
							}
							else
							{
								_error = BlogRssError.BlogNotFound;
								return;
							}

							if (string.IsNullOrEmpty(_feed.Title))
								_feed.Title = string.Format(GetMessageRaw("FeedTitle.NewPostsInBlog"), blog.Name);
							if (string.IsNullOrEmpty(_feed.Description))
								_feed.Description = !string.IsNullOrEmpty(blog.Description) ? blog.Description : string.Format(GetMessageRaw("FeedDescription.NewPostsInBlog"), blog.Name, CurrentSite.Name);
						}
						break;
					default:
						throw new NotSupportedException(string.Format("Filtration type '{0}' is unknown in current context!", filtrationType.ToString("G")));
				}
				if (f == null)
					f = new BXFilter();


				if (hasBlogs && bp.Ids.Count > 0)
				{
					IBXFilterItem blogFilter = new BXFilterItem(BXBlogPost.Fields.Blog.Id, BXSqlFilterOperators.In, bp.Ids);
					if (bp.Exclude)
						blogFilter = new BXFilterNot(blogFilter);
					f.Add(blogFilter);
				}

				//только из активных блогов
				f.Add(new BXFilterItem(BXBlogPost.Fields.Blog.Active, BXSqlFilterOperators.Equal, true));
				//только опубликованные сообщения
				f.Add(new BXFilterItem(BXBlogPost.Fields.IsPublished, BXSqlFilterOperators.Equal, true));
				//только опубликованные до настоящего времени
				f.Add(new BXFilterItem(BXBlogPost.Fields.DatePublished, BXSqlFilterOperators.LessOrEqual, DateTime.Now));

				//пользовательские поля
				BXParamsBag<object> customPropertyFilterSettings = FilterByPostCustomProperty ? PostCustomPropertyFilterSettings : null;
				if (customPropertyFilterSettings != null && customPropertyFilterSettings.Count > 0)
				{
					BXFormFilter customPropertyFilter = ((BXParamCustomFieldFilter)ParamsDefinition["PostCustomPropertyFilterSettings"]).BuildFormFilter(customPropertyFilterSettings);
					if (customPropertyFilter != null && customPropertyFilter.Count > 0)
					{
						int[] postIds = BXCustomEntityManager.GetObjectIds(BXBlogModuleConfiguration.PostCustomFieldEntityId, customPropertyFilter);
						f.Add(new BXFilterItem(BXBlogPost.Fields.Id, BXSqlFilterOperators.In, postIds));
					}
				}
				o = new BXOrderBy(new BXOrderByPair(BXBlogPost.Fields.DatePublished, BXOrderByDirection.Desc));
			}

			if (string.IsNullOrEmpty(feedLink) && !string.IsNullOrEmpty(feedUrlTemplate))
			{
				replaceParams["CategoryId"] = string.Empty;
				replaceParams["BlogId"] = string.Empty;
				replaceParams["BlogSlug"] = string.Empty;
				replaceParams["PostId"] = string.Empty;
				feedLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
			}

			if (string.IsNullOrEmpty(feedLink))
				feedLink = GetCurrentSiteUrl();

			feedSelfLink.Href = feedLink;

			replaceParams.Remove("CategoryId");
			replaceParams.Remove("BlogId");
			replaceParams.Remove("BlogSlug");
			replaceParams.Remove("PostId");

			BXBlogPostCollection posts =
				hasBlogs
				? BXBlogPost.GetList(
					f,
					o,
					new BXSelect(
						BXSelectFieldPreparationMode.Normal,
						BXBlogPost.Fields.Id,
						BXBlogPost.Fields.Title,
						BXBlogPost.Fields.Content,
						BXBlogPost.Fields.AuthorName,
						BXBlogPost.Fields.DatePublished,
						BXBlogPost.Fields.ContentType,
						BXBlogPost.Fields.Tags,
						BXBlogPost.Fields.Blog.Id,
						BXBlogPost.Fields.Blog.Slug,
						BXBlogPost.Fields.Blog.Name,
						BXBlogPost.Fields.Syndication.Post.Id,
						BXBlogPost.Fields.Syndication.Guid,
						BXBlogPost.Fields.Syndication.IsPermaLinkGuid
					),
					ItemQuantity > 0 ? new BXQueryParams(new BXPagingOptions(0, ItemQuantity)) : null,
					BXTextEncoder.EmptyTextEncoder
				)
				: new BXBlogPostCollection();

			if (sort != null)
				posts.Sort(sort);

			int cutNum = 0;
			string postLink = string.Empty;
			//RenderHideCut += delegate (object sender, BXBlogCutTagEventArgs e)
			//{
			//    string title = !string.IsNullOrEmpty(e.Option) ? e.Option.Trim() : string.Empty;
			//    if (string.IsNullOrEmpty(title))
			//        title = GetMessage("PostCutDefaultTitle");
			//    e.Writer.Write(string.Concat("<a href=\"", postLink, "#cut", ++cutNum, "\">", Encode(title), "</a>"));
			//};

			string feedItemUrlTemplate = FeedItemUrlTemplate;
			for (int i = 0; i < posts.Count; i++)
			{
				cutNum = 0;
				BXBlogPost post = posts[i];
				BXBlog blog = post.Blog;
				if (blog == null)
					continue; //некорректный экземпляр
				BXRss20ChannelItem item = _feed.Items.Create();
				item.Title = post.Title;
				string[] postTags = post.Tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < postTags.Length; j++)
					item.Categories.Add(new BXRss20Category(postTags[j].Trim()));

				if (!string.IsNullOrEmpty(feedItemUrlTemplate))
				{
					replaceParams["BlogId"] = blog.Id;
					replaceParams["BlogSlug"] = blog.Slug;
					replaceParams["PostId"] = post.Id;
					DateTime date = post.DatePublished;
					replaceParams["PostYear"] = date.Year.ToString("0000");
					replaceParams["PostMonth"] = date.Month.ToString("00");
					replaceParams["PostDay"] = date.Day.ToString("00");
					item.Link = postLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedItemUrlTemplate, replaceParams));
					replaceParams.Remove("BlogId");
					replaceParams.Remove("BlogSlug");
					replaceParams.Remove("PostId");
				}
				else
					postLink = Request.Url.ToString();

				Uri postUrl;
				Uri.TryCreate(postLink, UriKind.Absolute, out postUrl);
				//TODO: в description аннотацию, в content:encoded полное содержание
				//description
				IBXBlogPostProcessor postProc = GetPostProcessor(post, postUrl);
				postProc.HideCut = false;
				cutCount = 0;
				item.Description = postProc.Process(post.Content);
				if (cutCount > 0 && !EnablePostCut)
				{
					BXContentEncoded contentEncoded = BXContentEncoded.Create();
					contentEncoded.Value = item.Description;
					item.ElementExtensions.Add(contentEncoded);
					postProc.HideCut = true;
					item.Description = postProc.Process(post.Content);
				}

				//item.Author = post.AuthorName;
				//dc:creator
				BXDublinCoreCreator creator = BXDublinCoreCreator.Create();
				item.ElementExtensions.Add(creator);
				creator.Value = post.AuthorName;

				if (post.Syndication != null)
					item.Guid = new BXRss20ChannelItemGuid(post.Syndication.Guid, post.Syndication.IsPermaLinkGuid);
				else
					item.Guid = new BXRss20ChannelItemGuid(string.Concat("urn:bitrix:blog:post:", post.Id.ToString()), false);
				item.PubDate = post.DatePublished;
				//item.Source = ""//?;
			}
		}
		void PrepareComments(BXTagCachingScope scope, BXAtom10Link feedSelfLink)
		{
			BXParamsBag<object> replaceParams = new BXParamsBag<object>();
			string feedUrlTemplate = FeedUrlTemplate;
			string feedLink = string.Empty;

			BXBlogPost post = null;
			int postId = PostId;
			if (postId <= 0 || (post = BXBlogPost.GetById(postId, BXTextEncoder.EmptyTextEncoder)) == null)
			{
				_error = BlogRssError.PostNotFound;
				return;
			}

			post.PrepareForTagCaching(scope);

			BXBlog blog = null;
			if ((blog = BXBlog.GetById(post.BlogId, BXTextEncoder.EmptyTextEncoder)) == null)
			{
				_error = BlogRssError.BlogNotFound;
				return;
			}

			
			if (!new BXBlogAuthorization(blog, post).CanReadThisPostComments)
			{
				_error = BlogRssError.NoData;
				return;
			}

			BXFilter f = new BXFilter(new BXFilterItem(BXBlogComment.Fields.Post.Id, BXSqlFilterOperators.Equal, postId));

			if (!string.IsNullOrEmpty(feedUrlTemplate))
			{
				replaceParams["CategoryId"] = string.Empty;
				replaceParams["BlogId"] = blog.Id;
				replaceParams["BlogSlug"] = blog.Slug;
				replaceParams["PostId"] = postId;
				feedLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
			}

			if (string.IsNullOrEmpty(feedLink))
				feedLink = GetCurrentSiteUrl();

			feedSelfLink.Href = feedLink;

			replaceParams.Remove("CategoryId");
			replaceParams.Remove("BlogId");
			replaceParams.Remove("BlogSlug");
			replaceParams.Remove("PostId");

			//только прошедшие модерацию
			f.Add(new BXFilterItem(BXBlogComment.Fields.IsApproved, BXSqlFilterOperators.Equal, true));
			//только не удалённые
			f.Add(new BXFilterItem(BXBlogComment.Fields.MarkedForDelete, BXSqlFilterOperators.Equal, false));
			//только из опубликованных сообщений
			f.Add(new BXFilterItem(BXBlogComment.Fields.Post.IsPublished, BXSqlFilterOperators.Equal, true));
			//только из активных блогов
			f.Add(new BXFilterItem(BXBlogComment.Fields.Blog.Active, BXSqlFilterOperators.Equal, true));

			BXBlogCommentCollection comments = BXBlogComment.GetList(
				f,
				new BXOrderBy(new BXOrderByPair(BXBlogComment.Fields.Id, BXOrderByDirection.Desc)),
				new BXSelect(
					BXSelectFieldPreparationMode.Normal,
					BXBlogComment.Fields.Id,
					BXBlogComment.Fields.Content,
					BXBlogComment.Fields.AuthorName,
					BXBlogComment.Fields.DateCreated
				),
				ItemQuantity > 0 ? new BXQueryParams(new BXPagingOptions(0, ItemQuantity)) : null,
				BXTextEncoder.EmptyTextEncoder
			);

			string feedItemUrlTemplate = FeedItemUrlTemplate;
			replaceParams["BlogId"] = blog.Id;
			replaceParams["BlogSlug"] = blog.Slug;
			replaceParams["PostId"] = post.Id;
			for (int i = 0; i < comments.Count; i++)
			{
				BXBlogComment comment = comments[i];

				BXRss20ChannelItem item = _feed.Items.Create();
				item.Title = string.Concat(GetMessageRaw("FromUser"), " ", !string.IsNullOrEmpty(comment.AuthorName) ? comment.AuthorName : GetMessageRaw("UnknownUser"));
				//item.Categories.Add(new BXRss20Category(blog.Name)); //комментарии выгружаются без категории
				item.Description = CommentProcessor.Process(comment.Content);
				if (!string.IsNullOrEmpty(feedItemUrlTemplate))
				{
					replaceParams["CommentId"] = comment.Id;
					item.Link = Convert2AbsoluteUrl(ResolveTemplateUrl(feedItemUrlTemplate, replaceParams));
					replaceParams.Remove("CommentId");
				}
				item.Author = comment.AuthorName;
				item.Guid = new BXRss20ChannelItemGuid(string.Concat("urn:bitrix:blog:comment:", comment.Id.ToString()), false);
				item.PubDate = comment.DateCreated;
				//item.Source = ""//?;
			}

			if (string.IsNullOrEmpty(_feed.Title))
				_feed.Title = string.Format(GetMessageRaw("FeedTitle.NewCommentsInPost"), post.Title);
			if (string.IsNullOrEmpty(_feed.Description))
				_feed.Description = string.Format(GetMessageRaw("FeedDescription.NewCommentsInPost"), post.Title, blog.Name, CurrentSite.Name);

		}

		private BXRss20Feed _feedWrapper = null;
		public BXRss20Feed FeedWrapper
		{
			get { return _feedWrapper; }
		}

		private BXSyndicationElementXmlSavingOptions _feedSavingOptions;
		/// <summary>
		/// Параметры вывода канала
		/// </summary>
		public BXSyndicationElementXmlSavingOptions FeedSavingOptions
		{
			get
			{
				return _feedSavingOptions;
			}
		}

		private BXRss20Channel _feed = null;
		/// <summary>
		/// Rss канал
		/// </summary>
		public BXRss20Channel Feed
		{
			get { return _feed; }
		}


		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = BXBlogModuleConfiguration.GetComponentGroup();
			SortIndex = 1000;

			ParamsDefinition.Add(BXParametersDefinition.Cache);
			BXCategory mainCategory = BXCategory.Main;
			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

			ParamsDefinition.Add(
				GetParameterKey(BlogRssParameter.FeedTitle),
				new BXParamText(
					GetParameterTitle(BlogRssParameter.FeedTitle),
					GetMessageRaw("Param.FeedTitle.Default"),
					mainCategory
				)
			);

			ParamsDefinition.Add(
				GetParameterKey(BlogRssParameter.FeedDescription),
				new BXParamText(
					GetParameterTitle(BlogRssParameter.FeedDescription),
					string.Format(GetMessageRaw("Param.FeedDescription.Default"), CurrentSite.Name),
					mainCategory
				)
			);

			string stuffTypeKey = GetParameterKey(BlogRssParameter.StuffType);
			ParamsDefinition.Add(
				stuffTypeKey,
				new BXParamSingleSelection(
					GetParameterTitle(BlogRssParameter.StuffType),
					BlogRssStuffType.Post.ToString("G"),
					mainCategory,
					null,
					new ParamClientSideActionGroupViewSelector(ClientID, stuffTypeKey)
				)
			);

			string filtrationTypeKey = GetParameterKey(BlogRssParameter.FiltrationType);
			/*
			ParamsDefinition.Add(
				filtrationTypeKey,
				new BXParamSingleSelection(
					GetParameterTitle(BlogRssParameter.FiltrationType),
					BlogRssFiltrationType.None.ToString("G"),
					mainCategory,
					null,
					new ParamClientSideActionPack(
						new ParamClientSideActionGroupViewSelector(ClientID, filtrationTypeKey),
						new ParamClientSideActionGroupViewMember(ClientID, filtrationTypeKey, new string[] { "StandardFiltration" })
						)
				)
			);
			*/
			ParamsDefinition.Add(
				filtrationTypeKey,
				new BXParamSingleSelection(
					GetParameterTitle(BlogRssParameter.FiltrationType),
					BlogRssFiltrationType.None.ToString("G"),
					mainCategory,
					null,
					new ParamClientSideActionGroupViewSelector(ClientID, filtrationTypeKey)
				)
			);
			string categoryKey = GetParameterKey(BlogRssParameter.CategoryId);
			ParamsDefinition.Add(
				categoryKey,
				new BXParamSingleSelection(
					GetParameterTitle(BlogRssParameter.CategoryId),
					string.Empty,
					mainCategory,
					null,
					new ParamClientSideActionGroupViewMember(ClientID, categoryKey, new string[] { BlogRssFiltrationType.CategoryId.ToString("G") })
				)
			);

			string blogKey = GetParameterKey(BlogRssParameter.BlogSlug);
			ParamsDefinition.Add(
				blogKey,
				new BXParamSingleSelection(
					GetParameterTitle(BlogRssParameter.BlogSlug),
					string.Empty,
					mainCategory,
					null,
					new ParamClientSideActionGroupViewMember(ClientID, blogKey, new string[] { BlogRssFiltrationType.BlogSlug.ToString("G") })
				)
			);

			string postKey = GetParameterKey(BlogRssParameter.PostId);
			ParamsDefinition.Add(
				postKey,
				new BXParamText(
					GetParameterTitle(BlogRssParameter.PostId),
					string.Empty,
					mainCategory,
					new ParamClientSideActionGroupViewMember(ClientID, postKey, new string[] { BlogRssFiltrationType.PostId.ToString("G") })
				)
			);


			string enableCutKey = GetParameterKey(BlogRssParameter.EnablePostCut);
			ParamsDefinition.Add(
				enableCutKey,
				new BXParamYesNo(
					GetParameterTitle(BlogRssParameter.EnablePostCut),
					false,
					mainCategory,
					new ParamClientSideActionGroupViewMember(ClientID, enableCutKey, new string[] { "StandardFiltration", BlogRssStuffType.Post.ToString("G") })
				)
			);

			ParamsDefinition.Add(
				GetParameterKey(BlogRssParameter.ItemQuantity),
				new BXParamText(
					GetParameterTitle(BlogRssParameter.ItemQuantity),
					"10",
					mainCategory
				)
			);

			ParamsDefinition.Add(
				GetParameterKey(BlogRssParameter.FeedUrlTemplate),
				new BXParamText(
					GetParameterTitle(BlogRssParameter.FeedUrlTemplate),
					string.Empty,
					mainCategory
				)
			);

			ParamsDefinition.Add(
				GetParameterKey(BlogRssParameter.FeedItemUrlTemplate),
				new BXParamText(
					GetParameterTitle(BlogRssParameter.FeedItemUrlTemplate),
					string.Empty,
					mainCategory
				)
			);

			BXCategory customFieldCategory = BXCategory.CustomField;

			string filterByPostCustomKey = GetParameterKey(BlogRssParameter.FilterByPostCustomProperty);
			ParamsDefinition.Add(filterByPostCustomKey,
				new BXParamYesNo(
					GetParameterTitle(BlogRssParameter.FilterByPostCustomProperty),
					false,
					customFieldCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, filterByPostCustomKey, "FilterByPostCustomProperty", string.Empty)
					)
				);

			string postCustomPropertyFilterSettingsKey = GetParameterKey(BlogRssParameter.PostCustomPropertyFilterSettings);
			ParamsDefinition.Add(postCustomPropertyFilterSettingsKey,
				new BXParamCustomFieldFilter(
					GetParameterTitle(BlogRssParameter.PostCustomPropertyFilterSettings),
					string.Empty,
					customFieldCategory,
					BXBlogModuleConfiguration.PostCustomFieldEntityId,
					new ParamClientSideActionGroupViewMember(ClientID, postCustomPropertyFilterSettingsKey, new string[] { "FilterByPostCustomProperty" })
					)
				);

			BXCategory tagsCategory = new BXCategory(GetMessage("Category.Tags"), "Tags", 130);

			string filterByTags = GetParameterKey(BlogRssParameter.FilterByPostTag);
			ParamsDefinition.Add(
				filterByTags,
				new BXParamYesNo(
					GetParameterTitle(BlogRssParameter.FilterByPostTag),
					false,
					tagsCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, filterByTags, "TagFiltration", string.Empty)
				)
			);

			string tagFilterKey = GetParameterKey(BlogRssParameter.PostTagFilterSettings);
			ParamsDefinition.Add(
				tagFilterKey,
				new BXParamText(
					GetParameterTitle(BlogRssParameter.PostTagFilterSettings),
					string.Empty,
					tagsCategory,
					new ParamClientSideActionGroupViewMember(ClientID, tagFilterKey, new string[] { "TagFiltration" })
				)
			);

			ParamsDefinition.Add(
				GetParameterKey(BlogRssParameter.NoCData),
				new BXParamYesNo(
					GetParameterTitle(BlogRssParameter.NoCData),
					false,
					additionalSettingsCategory
				)
			);
		}

		protected override void LoadComponentDefinition()
		{
			IList<BXParamValue> stuffTypeValues = ParamsDefinition[GetParameterKey(BlogRssParameter.StuffType)].Values;
			stuffTypeValues.Clear();
			stuffTypeValues.Add(new BXParamValue(GetParameterValueTitle(BlogRssParameter.StuffType, BlogRssStuffType.Post.ToString("G")), BlogRssStuffType.Post.ToString("G")));
			stuffTypeValues.Add(new BXParamValue(GetParameterValueTitle(BlogRssParameter.StuffType, BlogRssStuffType.Comment.ToString("G")), BlogRssStuffType.Comment.ToString("G")));

			IList<BXParamValue> filtrationTypeValues = ParamsDefinition[GetParameterKey(BlogRssParameter.FiltrationType)].Values;
			filtrationTypeValues.Clear();
			filtrationTypeValues.Add(new BXParamValue(GetParameterValueTitle(BlogRssParameter.FiltrationType, BlogRssFiltrationType.None.ToString("G")), BlogRssFiltrationType.None.ToString("G")));
			filtrationTypeValues.Add(new BXParamValue(GetParameterValueTitle(BlogRssParameter.FiltrationType, BlogRssFiltrationType.CategoryId.ToString("G")), BlogRssFiltrationType.CategoryId.ToString("G")));
			filtrationTypeValues.Add(new BXParamValue(GetParameterValueTitle(BlogRssParameter.FiltrationType, BlogRssFiltrationType.BlogSlug.ToString("G")), BlogRssFiltrationType.BlogSlug.ToString("G")));
			filtrationTypeValues.Add(new BXParamValue(GetParameterValueTitle(BlogRssParameter.FiltrationType, BlogRssFiltrationType.PostId.ToString("G")), BlogRssFiltrationType.PostId.ToString("G")));

			IList<BXParamValue> categoryValues = ParamsDefinition[GetParameterKey(BlogRssParameter.CategoryId)].Values;
			categoryValues.Clear();
			categoryValues.Add(new BXParamValue(GetMessageRaw("NotSelectedFeminine"), "0"));
			BXBlogCategoryCollection categorires = BXBlogCategory.GetList(
				null,
				new BXOrderBy(
					new BXOrderByPair(BXBlogCategory.Fields.Name, BXOrderByDirection.Asc)),
				new BXSelect(
					BXSelectFieldPreparationMode.Normal,
					BXBlogCategory.Fields.Id,
					BXBlogCategory.Fields.Name
					),
				null
				);
			foreach (BXBlogCategory category in categorires)
				categoryValues.Add(new BXParamValue(category.TextEncoder.Decode(category.Name), category.Id.ToString()));

			IList<BXParamValue> blogValues = ParamsDefinition[GetParameterKey(BlogRssParameter.BlogSlug)].Values;
			blogValues.Clear();
			blogValues.Add(new BXParamValue(GetMessageRaw("NotSelectedMasculine"), "0"));
			BXBlogCollection blogs = BXBlog.GetList(
				new BXFilter(new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true)),
				new BXOrderBy(new BXOrderByPair(BXBlog.Fields.Name, BXOrderByDirection.Asc)),
				new BXSelect(
					BXSelectFieldPreparationMode.Normal,
					BXBlog.Fields.Id,
					BXBlog.Fields.Slug,
					BXBlog.Fields.Name
					),
				null
				);

			foreach (BXBlog blog in blogs)
				blogValues.Add(new BXParamValue(blog.TextEncoder.Decode(blog.Name), blog.Slug));
		}

		protected override string GetCacheOutput()
		{
			return ((BlogRssTemplate)ComponentTemplate).OutputXml;
		}

		protected override void SetCacheOutput(string output)
		{
			((BlogRssTemplate)ComponentTemplate).OutputXml = output;
		}
	}

	/// <summary>
	/// Базовый класс для шаблонов компонента "BlogRssComponent"
	/// </summary>
	public abstract class BlogRssTemplate : BXComponentTemplate<BlogRssComponent>
	{
		private string _outputXml = null;
		public string OutputXml
		{
			get { return _outputXml; }
			set { _outputXml = value; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (Component.ComponentError != BlogRssError.None)
			{
				BXError404Manager.Set404Status(Response);
				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null)
					bitrixPage.Title = Component.ComponentErrorText;
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			bool aboutError = Component.ComponentError != BlogRssError.None;

			if (aboutError)
			{
				if (!Component.IsComponentDesignMode && !BXConfigurationUtility.IsDesignMode)
					Response.End();
				return;
			}

			if (string.IsNullOrEmpty(_outputXml))
			{
				BXRss20Feed feedWrapper = Component.FeedWrapper;
				if (feedWrapper == null)
					throw new InvalidOperationException("Could not find feed!");

				using (MemoryStream ms = new MemoryStream())
				{
					XmlWriterSettings s = new XmlWriterSettings();
					s.CheckCharacters = false;
					s.CloseOutput = false;
					s.ConformanceLevel = ConformanceLevel.Document;
					s.Indent = true;
					s.Encoding = new UTF8Encoding(false);
					//System.Diagnostics.Stopwatch t = new System.Diagnostics.Stopwatch();
					//using (XmlWriter xw = XmlWriter.Create(ms, s))
					using (XmlWriter xw = new BXSyndicationXmlCheckingWriter(XmlWriter.Create(ms, s)))
					{
						feedWrapper.SaveToXml(xw, Component.FeedSavingOptions);
						xw.Flush();
					}
					_outputXml = Encoding.UTF8.GetString(ms.ToArray());
				}
			}
			if (IsComponentDesignMode || (BXConfigurationUtility.IsDesignMode && Bitrix.UI.TemplateRequisite.GetCurrentPublicPanelVisiblity(Component.Page)))
			{
				if (BXConfigurationUtility.IsDesignMode)
					writer.Write("<pre style='width:500px;overflow:scroll;'>");
				else
					writer.Write("<pre>");
				writer.Write(HttpUtility.HtmlEncode(_outputXml));
				writer.Write("</pre>");
			}
			else
			{
				HttpResponse r = Response;
				r.Buffer = true;
				writer.Flush();
				r.Clear();
				r.ContentType = "text/xml";
				r.Write(_outputXml);
				r.End();
			}
		}
	}

	public interface IBlogRssComponentTagFilter
	{
		void InitFilter(string siteId, string tags, List<int> ids, bool exclude, int categoryId);
		int Count();
		IList<int> GetPostIdList(BXPagingOptions paging);
	}
}