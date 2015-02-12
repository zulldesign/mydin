using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.Services.Syndication;
using Bitrix.DataTypes;
using Bitrix.Security;
using Bitrix.Configuration;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Text;
using Bitrix.Components.Editor;
using Bitrix.Services;
using System.Web.Caching;
using Bitrix.Services.Text;

namespace Bitrix.Forum.Components
{
    /// <summary>
    /// Параметр компонента "RSS форума"
    /// </summary>
    public enum ForumRssParameter
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
        /// Ид форума
        /// </summary>
        ForumId,
        /// <summary>
        /// Ид темы
        /// </summary>
        TopicId,
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
        /// Доступные форумы
        /// </summary>
        AvailableForumIdList,
        /// <summary>
        /// Заголовок канала
        /// </summary>
        FeedTitle,
        /// <summary>
        /// Описание канала
        /// </summary>
        FeedDescription,
        /// <summary>
        /// Время жизни
        /// </summary>
        FeedTtl,
		/// <summary>
		/// Не использовать CDATA
		/// </summary>
		NoCData
    }

    /// <summary>
    /// Тип содержания
    /// </summary>
    public enum ForumRssStuffType
    { 
        /// <summary>
        /// Темы
        /// </summary>
        Topic   = 1,
        /// <summary>
        /// Сообщения
        /// </summary>
        Post
    }

    /// <summary>
    /// Тип фильтрации
    /// </summary>
    public enum ForumRssFiltrationType
    { 
        /// <summary>
        /// без фильтрации
        /// </summary>
        None        = 0,
        /// <summary>
        /// по Ид категории
        /// </summary>
        CategoryId,
        /// <summary>
        /// по Ид форума
        /// </summary>
        ForumId,
        /// <summary>
        /// по Ид темы
        /// </summary>
        TopicId
    }


    /// <summary>
    /// Ошибка
    /// </summary>
    public enum ForumRssError
    {
        None = 0,
        CategoryNotFound = -1,
        ForumNotFound = -2,
        TopicNotFound = -3,
        /// <summary>
        /// Нет даных (нет фрумов доступных для просмотра неавторизованными пользователями)
        /// </summary>
        NoData = -4,
        /// <summary>
        /// Общая при созданнии канала
        /// </summary>
        FeedCreationGeneral = -100
    }

    /// <summary>
    /// Компонент RSS форума
    /// </summary>
    public partial class ForumRssComponent : BXComponent
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            IncludeComponentTemplate();
            IsCached();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            try
            {
                PrepareFeed();
            }
            catch (Exception /*exc*/)
            {
                _error = ForumRssError.FeedCreationGeneral;
            }
            if (ComponentError != ForumRssError.None)
                AbortCache();
        }

        private ForumRssError _error = ForumRssError.None;
        /// <summary>
        /// Ошибка
        /// </summary>
        public ForumRssError ComponentError
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
                if (_error == ForumRssError.None)
                    return string.Empty;
                else if (_error == ForumRssError.CategoryNotFound)
                    return GetMessageRaw("Error.CategoryNotFound");
                else if (_error == ForumRssError.ForumNotFound)
                    return GetMessageRaw("Error.ForumNotFound");
                else if (_error == ForumRssError.TopicNotFound)
                    return GetMessageRaw("Error.TopicNotFound");
                else if (_error == ForumRssError.NoData)
                    return GetMessageRaw("Error.NoData");
                else if(_error == ForumRssError.FeedCreationGeneral)
                    return GetMessageRaw("Error.FeedCreationGeneral");
                else
                    return GetMessageRaw("Error.General");
            }
        }

        /// <summary>
        /// Тип содержания
        /// </summary>
        public ForumRssStuffType StuffType
        {
            get
            {
                string parVal = Parameters.Get(GetParameterKey(ForumRssParameter.StuffType));
                if (string.IsNullOrEmpty(parVal))
                    return ForumRssStuffType.Post;
                ForumRssStuffType result = ForumRssStuffType.Post;
                try
                {
                    result = (ForumRssStuffType)Enum.Parse(typeof(ForumRssStuffType), parVal);
                }
                catch (Exception /*exc*/)
                {
                }
                return result;
            }
            set
            {
                Parameters[GetParameterKey(ForumRssParameter.StuffType)] = value.ToString("G");
            }
        }

        /// <summary>
        /// Тип фильтрации
        /// </summary>
        public ForumRssFiltrationType FiltrationType
        {
            get 
            {
                string parVal = Parameters.Get(GetParameterKey(ForumRssParameter.FiltrationType));
                if (string.IsNullOrEmpty(parVal))
                    return ForumRssFiltrationType.None;
                ForumRssFiltrationType result = ForumRssFiltrationType.None;
                try
                {
                    result = (ForumRssFiltrationType)Enum.Parse(typeof(ForumRssFiltrationType), parVal);
                }
                catch (Exception /*exc*/)
                {
                }
                return result;
            }
            set 
            {
                Parameters[GetParameterKey(ForumRssParameter.FiltrationType)] = value.ToString("G");
            }
        }

        /// <summary>
        /// Ид категории
        /// </summary>
        public int CategoryId
        {
            get 
            {
                string parVal = Parameters.Get(GetParameterKey(ForumRssParameter.CategoryId));
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
                Parameters[GetParameterKey(ForumRssParameter.CategoryId)] = value.ToString();
            }
        }

        /// <summary>
        /// Ид форума
        /// </summary>         
        public int ForumId
        {
            get
            {
                string parVal = Parameters.Get(GetParameterKey(ForumRssParameter.ForumId));
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
                Parameters[GetParameterKey(ForumRssParameter.ForumId)] = value.ToString();
            }
        }

        /// <summary>
        /// Ид темы
        /// </summary>         
        public int TopicId
        {
            get
            {
                string parVal = Parameters.Get(GetParameterKey(ForumRssParameter.TopicId));
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
                Parameters[GetParameterKey(ForumRssParameter.TopicId)] = value.ToString();
            }
        }

        /// <summary>
        /// Количество элементов
        /// </summary>         
        public int ItemQuantity
        {
            get
            {
                string parVal = Parameters.Get(GetParameterKey(ForumRssParameter.ItemQuantity));
                if (string.IsNullOrEmpty(parVal))
                    return 10;
                int result = 10;
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
                Parameters[GetParameterKey(ForumRssParameter.ItemQuantity)] = value.ToString();
            }
        }

        /// <summary>
        /// Шаблон ссылки на канал
        /// </summary>
        public string FeedUrlTemplate
        {
            get
            {
                return Parameters.Get(GetParameterKey(ForumRssParameter.FeedUrlTemplate), "~/forumlist.aspx");
            }
            set
            {
                Parameters[GetParameterKey(ForumRssParameter.FeedUrlTemplate)] = value;
            }
        }

        /// <summary>
        /// Шаблон ссылки на элемент канала
        /// </summary>
        public string FeedItemUrlTemplate
        {
            get 
            {
                return Parameters.Get(GetParameterKey(ForumRssParameter.FeedItemUrlTemplate), "~/forumtopic.aspx?post=#PostId#");
            }
            set 
            {
                Parameters[GetParameterKey(ForumRssParameter.FeedItemUrlTemplate)] = value;
            }
        }

        /// <summary>
        /// Время жизни, в мин.
        /// </summary>
        public int FeedTtl
        {
            get { return Parameters.Get<int>(GetParameterKey(ForumRssParameter.FeedTtl), 60); }
            set { Parameters[GetParameterKey(ForumRssParameter.FeedTtl)] = value.ToString(); }
        }

        private List<int> _availableForumIdList = null;
        /// <summary>
        /// Ид доступных форумов
        /// </summary>
        public IList<int> AvailableForumIdList
        {
            get
            {
                if (_availableForumIdList != null)
                    return _availableForumIdList;
                _availableForumIdList = Parameters.GetListInt(GetParameterKey(ForumRssParameter.AvailableForumIdList));
                if(_availableForumIdList == null)
                    _availableForumIdList = new List<int>();
                return _availableForumIdList;
            }
        }

        /// <summary>
        /// Заголовок канала
        /// </summary>
        public string FeedTitle
        {
            get
            {
                return Parameters.Get(GetParameterKey(ForumRssParameter.FeedTitle));
            }
            set
            {
                Parameters[GetParameterKey(ForumRssParameter.FeedTitle)] = value;
            }
        }

        /// <summary>
        /// Описание канала
        /// </summary>
        public string FeedDescription
        {
            get
            {
                return Parameters.Get(GetParameterKey(ForumRssParameter.FeedDescription));
            }
            set
            {
                Parameters[GetParameterKey(ForumRssParameter.FeedDescription)] = value;
            }
        }

		/// <summary>
        /// Использовать CDATA
        /// </summary>
        public bool UseCData
        {
            get
            {
                return !Parameters.GetBool(GetParameterKey(ForumRssParameter.NoCData), false);
            }
            set
            {
                Parameters[GetParameterKey(ForumRssParameter.NoCData)] = (!value).ToString();
            }
        }

        private BXForumPostChain _processor = null;
        protected BXForumPostChain Processor
        {
            get 
            {
                if (_processor != null)
                    return _processor;
                _processor = new BXForumPostChain();
                _processor.MaxWordLength = 0;
                _processor.EnableImages = true;
                return _processor;
            }
        }

        private Uri _uri;
        private string Convert2AbsoluteUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && Uri.TryCreate(BXSefUrlManager.CurrentUrl, Page.ResolveUrl(url), out _uri) ? _uri.ToString() : VirtualPathUtility.ToAbsolute("~/");
        }

        private BXForumCollection _permittedForums = null;
        /// <summary>
        /// Запрос разрешённых форумов
        ///  - активные относящиеся к текущему сайту;
        ///  - входящие в список доступных для этого компонента;
        ///  - разрешающие чтение для "Everyone". 
        /// </summary>
        /// <returns></returns>
        private BXForumCollection GetPermittedForums()
        {
            if (_permittedForums != null)
                return _permittedForums;

            BXFilter filter = new BXFilter(
                    new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true),
                    new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
                    );

            BXForumCollection forums = BXForum.GetList(
                filter,
                new BXOrderBy(new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)),
                new BXSelect(BXSelectFieldPreparationMode.Normal, BXForum.Fields.Id, BXForum.Fields.Name),
                null
                );

            _permittedForums = new BXForumCollection();
            foreach (BXForum forum in forums)
            {
                // TODO Переделать код для запроса списка ролей операции единожды
                BXRoleCollection roles = BXRoleManager.GetAllRolesForOperation(BXForum.Operations.ForumPublicRead, BXForumModuleConfiguration.Id, forum.Id.ToString());
                foreach (BXRole role in roles)
                {
                    if (!string.Equals(role.RoleName, "Guest", StringComparison.InvariantCulture))
                        continue;
                    _permittedForums.Add(forum);
                    break;
                }
            }

            return _permittedForums;
        }

        /// <summary>
        /// Подготовить RSS канал
        /// </summary>
        private void PrepareFeed()
        {
			_feedSavingOptions = new BXSyndicationElementXmlSavingOptions();
			_feedSavingOptions.UseCData = UseCData;

            _feed = new BXRss20Channel();
            _feed.Title = FeedTitle;

            //BXParamsBag<object> replaceParams = new BXParamsBag<object>();
            //string feedUrlTemplate = FeedUrlTemplate;
            //if (!string.IsNullOrEmpty(feedUrlTemplate))
            //{
            //    replaceParams["ForumId"] = ForumId;
            //    _feed.Link = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
            //    replaceParams.Remove("ForumId");
            //}
            //else
            //    _feed.Link = Convert2AbsoluteUrl(VirtualPathUtility.ToAbsolute("~/"));

            _feed.Description = FeedDescription;
            _feed.LastBuildDate = DateTime.Now.ToUniversalTime();
            _feed.Language = BXSite.Current.GetCultureInfo().Name;
            _feed.Generator = "bitrix::forum.rss";
            int feedTtl = FeedTtl;
            if (feedTtl > 0)
                _feed.Ttl = feedTtl;

            BXForumCollection permittedForums = GetPermittedForums();
            List<int> permittedForumIdList = new List<int>();
            IList<int> availableForumIdList = AvailableForumIdList;
            if (availableForumIdList.Count == 0)
                foreach (BXForum permittedForum in permittedForums)
                    permittedForumIdList.Add(permittedForum.Id);
            else
                foreach (BXForum permittedForum in permittedForums)
                {
                    if (availableForumIdList.IndexOf(permittedForum.Id) < 0)
                        continue;
                    permittedForumIdList.Add(permittedForum.Id);
                }
            if (permittedForumIdList.Count == 0)
            {
                _error = ForumRssError.NoData;
                return;
            }

            BXQueryParams qp = null;
            int itemQuantity = ItemQuantity;
            if (itemQuantity > 0)
                qp = new BXQueryParams(new BXPagingOptions(0, itemQuantity));

            ForumRssStuffType stuffType = StuffType;
            switch (stuffType)
            {
                case ForumRssStuffType.Post:
                    {
                        BXParamsBag<object> replaceParams = new BXParamsBag<object>();
                        string feedUrlTemplate = FeedUrlTemplate;
                        string feedLink = string.Empty;
                        BXFilter f = null;
                        ForumRssFiltrationType filtrationType = FiltrationType;
                        switch (filtrationType)
                        {
                            case ForumRssFiltrationType.None:
                                {
                                    if (string.IsNullOrEmpty(_feed.Title))
                                        _feed.Title = string.Format(GetMessageRaw("FeedTitle.NewForumPostsOnSite"), CurrentSite.Name);
                                    if (string.IsNullOrEmpty(_feed.Description))
                                        _feed.Description = string.Concat(GetMessageRaw("FeedDescription.NewForumPostsOnSite"), CurrentSite.Name);
                                }
                                break;
                            case ForumRssFiltrationType.CategoryId:
                                {
                                    BXForumCategory category = null;
                                    int categoryId = CategoryId;
                                    if (categoryId > 0 && (category = BXForumCategory.GetById(categoryId, BXTextEncoder.EmptyTextEncoder)) != null)
                                    {
                                        f = new BXFilter(new BXFilterItem(BXForumPost.Fields.Forum.Category.Id, BXSqlFilterOperators.Equal, categoryId));
                                        if (!string.IsNullOrEmpty(feedUrlTemplate))
                                        {
                                            replaceParams["CategoryId"] = categoryId;
                                            replaceParams["ForumId"] = string.Empty;
                                            replaceParams["TopicId"] = string.Empty;
                                            feedLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
                                        }
                                    }
                                    else
                                    {
                                        _error = ForumRssError.CategoryNotFound;
                                        return;
                                    }

                                    if (string.IsNullOrEmpty(_feed.Title))
                                        _feed.Title = string.Format(GetMessageRaw("FeedTitle.NewForumPostsInCategory"), category.Name);
                                    if (string.IsNullOrEmpty(_feed.Description))
                                        _feed.Description = string.Format(GetMessageRaw("FeedDescription.NewForumPostsInCategory"), category.Name, CurrentSite.Name);

                                }
                                break;
                            case ForumRssFiltrationType.ForumId:
                                {
                                    BXForum forum = null;
                                    int forumId = ForumId;
                                    if (forumId > 0 && (forum = BXForum.GetById(forumId, BXTextEncoder.EmptyTextEncoder)) != null)
                                    {
                                        f = new BXFilter(new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, forumId));
                                        if (!string.IsNullOrEmpty(feedUrlTemplate))
                                        {
                                            replaceParams["CategoryId"] = string.Empty;
                                            replaceParams["ForumId"] = forumId;
                                            replaceParams["TopicId"] = string.Empty;
                                            feedLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
                                        }
                                    }
                                    else
                                    {
                                        _error = ForumRssError.ForumNotFound;
                                        return;
                                    }

                                    if (string.IsNullOrEmpty(_feed.Title))
                                        _feed.Title = string.Format(GetMessageRaw("FeedTitle.NewPostsInForum"), forum.Name);
                                    if (string.IsNullOrEmpty(_feed.Description))
                                    {
                                        string descr = BXStringUtility.StripOffSimpleTags(BXStringUtility.StripOffScriptTag(forum.Description));
                                        _feed.Description = string.Format(!string.IsNullOrEmpty(descr) ? descr : GetMessageRaw("FeedDescription.NewPostsInForum"), forum.Name, CurrentSite.Name);
                                    }
                                }
                                break;
                            case ForumRssFiltrationType.TopicId:
                                {
                                    BXForumTopic topic = null;
                                    int topicId = TopicId;
                                    if (topicId <= 0 || (topic = BXForumTopic.GetById(topicId, BXTextEncoder.EmptyTextEncoder)) == null)
                                    {
                                        _error = ForumRssError.TopicNotFound;
                                        return;
                                    }

                                    BXForum forum = null;
                                    if ((forum = BXForum.GetById(topic.ForumId, BXTextEncoder.EmptyTextEncoder)) == null)
                                    {
                                        _error = ForumRssError.ForumNotFound;
                                        return;
                                    }

                                    f = new BXFilter(new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, topicId));
                                    if (!string.IsNullOrEmpty(feedUrlTemplate))
                                    {
                                        replaceParams["CategoryId"] = string.Empty;
                                        replaceParams["ForumId"] = forum.Id;
                                        replaceParams["TopicId"] = topicId;
                                        feedLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
                                    }

                                    if (string.IsNullOrEmpty(_feed.Title))
                                        _feed.Title = string.Format(GetMessageRaw("FeedTitle.NewPostsInTopic"), topic.Name);
                                    if (string.IsNullOrEmpty(_feed.Description))
                                        _feed.Description = string.Format(GetMessageRaw("FeedDescription.NewPostsInTopic"), topic.Name, forum.Name, CurrentSite.Name);
                                }
                                break;
                            default:
                                throw new NotSupportedException(string.Format("Filtration type '{0}' is unknown in current context!", filtrationType.ToString("G")));
                        }

                        if (!string.IsNullOrEmpty(feedLink))
                            _feed.Link = feedLink;
                        else if (!string.IsNullOrEmpty(feedUrlTemplate))
                        {
                            replaceParams["CategoryId"] = string.Empty;
                            replaceParams["ForumId"] = string.Empty;
                            replaceParams["TopicId"] = string.Empty;
                            _feed.Link = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
                        }
                        else
                            _feed.Link = GetCurrentSiteUrl();

                        replaceParams.Remove("CategoryId");
                        replaceParams.Remove("ForumId");
                        replaceParams.Remove("TopicId");

                        if (f == null)
                            f = new BXFilter();

                        //только потверждённые темы
                        f.Add(new BXFilterItem(BXForumPost.Fields.Topic.Approved, BXSqlFilterOperators.Equal, true));
                        //только потверждённые сообщения
                        f.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));
                        //только из разрешённых форумов
                        f.Add(new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.In, permittedForumIdList));

                        BXForumPostCollection posts = BXForumPost.GetList(
                            f,
                            new BXOrderBy(new BXOrderByPair(BXForumPost.Fields.Id, BXOrderByDirection.Desc)),
                            new BXSelect(
                                BXSelectFieldPreparationMode.Normal,
                                BXForumPost.Fields.Id,
                                BXForumPost.Fields.Forum.Id,
                                BXForumPost.Fields.Topic.Id,
                                BXForumPost.Fields.Topic.Name,
                                BXForumPost.Fields.Topic.FirstPost.Id,
                                BXForumPost.Fields.Post,
                                BXForumPost.Fields.AuthorName,
                                BXForumPost.Fields.DateCreate
                                ),
                            qp
                        );

                        string feedItemUrlTemplate = FeedItemUrlTemplate;
                        for (int i = 0; i < posts.Count; i++)
                        {
                            BXForumPost post = posts[i];
                            BXRss20ChannelItem item = _feed.Items.Create();
                            BXForumTopic topic = post.Topic;
                            if (topic != null)
                            {
                                string topicName = topic.TextEncoder.Decode(topic.Name);
                                item.Title = post.Id != topic.FirstPostId ? string.Concat("Re: ", topicName) : topicName;
                                item.Categories.Add(new BXRss20Category(topicName));
                            }
                            item.Description = Processor.Process(post.Post);
                            if (!string.IsNullOrEmpty(feedItemUrlTemplate))
                            {
                                replaceParams["ForumId"] = post.ForumId;
                                replaceParams["TopicId"] = post.TopicId;
                                replaceParams["PostId"] = post.Id;
                                item.Link = Convert2AbsoluteUrl(ResolveTemplateUrl(feedItemUrlTemplate, replaceParams));
                                replaceParams.Remove("ForumId");
                                replaceParams.Remove("TopicId");
                                replaceParams.Remove("PostId");
                            }
                            item.Author = post.TextEncoder.Decode(post.AuthorName);
                            item.Guid = new BXRss20ChannelItemGuid(string.Concat("urn:bitrix:forum:post:", post.Id.ToString()), false);
                            item.PubDate = post.DateCreate;
                            //item.Source = ""//?;
                        }
                    }
                    break;
                case ForumRssStuffType.Topic:
                    {
                        BXParamsBag<object> replaceParams = new BXParamsBag<object>();
                        string feedUrlTemplate = FeedUrlTemplate;
                        string feedLink = string.Empty;
                        BXFilter f = null;
                        ForumRssFiltrationType filtrationType = FiltrationType;
                        switch (filtrationType)
                        {
                            case ForumRssFiltrationType.None:
                            case ForumRssFiltrationType.TopicId: //игнорируется, т.к. получается "Новые темы из темы"
                                {
                                    if (string.IsNullOrEmpty(_feed.Title))
                                        _feed.Title = string.Format(GetMessageRaw("FeedTitle.NewForumTopicsInSite"), CurrentSite.Name);
                                    if (string.IsNullOrEmpty(_feed.Description))
                                        _feed.Description = string.Format(GetMessageRaw("FeedDescription.NewForumTopicsInSite"), CurrentSite.Name);
                                }
                                break;
                            case ForumRssFiltrationType.CategoryId:
                                {
                                    BXForumCategory category = null;
                                    int categoryId = CategoryId;
                                    if (categoryId > 0 && (category = BXForumCategory.GetById(categoryId, BXTextEncoder.EmptyTextEncoder)) != null)
                                    {
                                        f = new BXFilter(new BXFilterItem(BXForumTopic.Fields.Forum.Category.Id, BXSqlFilterOperators.Equal, categoryId));
                                        if (!string.IsNullOrEmpty(feedUrlTemplate))
                                        {
                                            replaceParams["CategoryId"] = categoryId;
                                            replaceParams["ForumId"] = string.Empty;
                                            replaceParams["TopicId"] = string.Empty;
                                            feedLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
                                        }
                                    }
                                    else
                                    {
                                        _error = ForumRssError.CategoryNotFound;
                                        return;
                                    }

                                    if (string.IsNullOrEmpty(_feed.Title))
                                        _feed.Title = string.Format(GetMessageRaw("FeedTitle.NewForumTopicsInCategory"), category.Name);
                                    if (string.IsNullOrEmpty(_feed.Description))
                                        _feed.Description = string.Format(GetMessageRaw("FeedDescription.NewForumTopicsInCategory"), category.Name, CurrentSite.Name);
                                }
                                break;
                            case ForumRssFiltrationType.ForumId:
                                {

                                    BXForum forum = null;
                                    int forumId = ForumId;
                                    if (forumId > 0 && (forum = BXForum.GetById(forumId, BXTextEncoder.EmptyTextEncoder)) != null)
                                    {
                                        f = new BXFilter(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.Equal, forumId));
                                        if (!string.IsNullOrEmpty(feedUrlTemplate))
                                        {
                                            replaceParams["CategoryId"] = string.Empty;
                                            replaceParams["ForumId"] = forumId;
                                            replaceParams["TopicId"] = string.Empty;
                                            feedLink = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
                                        }
                                    }
                                    else
                                    {
                                        _error = ForumRssError.ForumNotFound;
                                        return;
                                    }

                                    if (string.IsNullOrEmpty(_feed.Title))
                                        _feed.Title = string.Format(GetMessageRaw("FeedTitle.NewTopicsInForum"), forum.Name);
                                    if (string.IsNullOrEmpty(_feed.Description))
                                    {
                                        string descr = BXStringUtility.StripOffSimpleTags(BXStringUtility.StripOffScriptTag(forum.Description));
                                        _feed.Description = string.Format(!string.IsNullOrEmpty(descr) ? descr : GetMessageRaw("FeedDescription.NewTopicsInForum"), forum.Name, CurrentSite.Name);
                                    }
                                }
                                break;
                            default:
                                throw new NotSupportedException(string.Format("Filtration type '{0}' is unknown in current context!", filtrationType.ToString("G")));
                        }

                        if (!string.IsNullOrEmpty(feedLink))
                            _feed.Link = feedLink;
                        else if (!string.IsNullOrEmpty(feedUrlTemplate))
                        {
                            replaceParams["CategoryId"] = string.Empty;
                            replaceParams["ForumId"] = string.Empty;
                            replaceParams["TopicId"] = string.Empty;
                            _feed.Link = Convert2AbsoluteUrl(ResolveTemplateUrl(feedUrlTemplate, replaceParams));
                        }
                        else
                            _feed.Link = GetCurrentSiteUrl();

                        replaceParams.Remove("CategoryId");
                        replaceParams.Remove("ForumId");
                        replaceParams.Remove("TopicId");

                        if (f == null)
                            f = new BXFilter();

                        //только потверждённые темы
                        f.Add(new BXFilterItem(BXForumTopic.Fields.Approved, BXSqlFilterOperators.Equal, true));
                        //только из разрешённых форумов
                        f.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.In, permittedForumIdList));
                        BXForumTopicCollection topics = BXForumTopic.GetList(
                            f,
                            new BXOrderBy(new BXOrderByPair(BXForumTopic.Fields.Id, BXOrderByDirection.Desc)),
                            new BXSelect(
                                BXSelectFieldPreparationMode.Normal,
                                BXForumTopic.Fields.Id,
                                BXForumTopic.Fields.Name,
								BXForumTopic.Fields.Forum.Name,
								BXForumTopic.Fields.FirstPost.Forum.Id,
								BXForumTopic.Fields.FirstPost.Topic.Id,
                                BXForumTopic.Fields.FirstPost.Id,
                                BXForumTopic.Fields.FirstPost.Post,
                                BXForumTopic.Fields.FirstPost.AuthorName,
                                BXForumTopic.Fields.FirstPost.DateCreate
                                ),
                            qp
                            );

                        string feedItemUrlTemplate = FeedItemUrlTemplate;
                        for (int i = 0; i < topics.Count; i++)
                        {
                            BXForumTopic topic = topics[i];
                            BXForum forum = topic.Forum;
                            if (forum == null)
                                continue;
                            BXForumPost post = topic.FirstPost;
                            if (post == null)
                                continue;

                            BXRss20ChannelItem item = _feed.Items.Create();
                            string topicName = topic.TextEncoder.Decode(topic.Name);
                            item.Title = topicName;
                            item.Categories.Add(new BXRss20Category(forum.TextEncoder.Decode(forum.Name)));
                            item.Description = Processor.Process(post.Post);
                            if (!string.IsNullOrEmpty(feedItemUrlTemplate))
                            {
                                replaceParams["ForumId"] = post.ForumId;
                                replaceParams["TopicId"] = post.TopicId;
                                replaceParams["PostId"] = post.Id;
                                item.Link = Convert2AbsoluteUrl(ResolveTemplateUrl(feedItemUrlTemplate, replaceParams));
                                replaceParams.Remove("ForumId");
                                replaceParams.Remove("TopicId");
                                replaceParams.Remove("PostId");
                            }
                            else
                                item.Link = Request.Url.ToString();

                            item.Author = post.TextEncoder.Decode(post.AuthorName);
                            item.Guid = new BXRss20ChannelItemGuid(string.Concat("urn:bitrix:forum:post:", post.Id.ToString()), false);
                            item.PubDate = post.DateCreate;
                            //item.Source = ""//?;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException(string.Format("Stuff type '{0}' is unknown in current context!", stuffType.ToString("G")));
            }
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

        protected string GetParameterKey(ForumRssParameter parameter)
        {
            return parameter.ToString("G");
        }

        protected string GetParameterTitle(ForumRssParameter parameter)
        {
            return GetMessageRaw(string.Concat("Param.", parameter.ToString("G")));
        }

        protected string GetParameterValueTitle(ForumRssParameter parameter, string valueKey)
        {
            return GetMessageRaw(string.Concat("Param.", parameter.ToString("G"), ".", valueKey));
        }

        protected string GetCurrentSiteUrl()
        {
            return Convert2AbsoluteUrl(VirtualPathUtility.ToAbsolute("~/"));
        }

        private BXSite _currentSite = null;
        protected BXSite CurrentSite
        {
            get { return _currentSite ?? (_currentSite = BXSite.Current); }
        }

        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";
            Group = new BXComponentGroup("forum", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);
            SortIndex = 1000;

            ParamsDefinition.Add(BXParametersDefinition.Cache);
            BXCategory mainCategory = BXCategory.Main;
            BXCategory urlCategory = BXCategory.UrlSettings;
            BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

            ParamsDefinition.Add(
                GetParameterKey(ForumRssParameter.FeedTitle),
                new BXParamText(
                    GetParameterTitle(ForumRssParameter.FeedTitle),
                    GetMessageRaw("Param.FeedTitle.Default"),
                    mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumRssParameter.FeedDescription),
                new BXParamText(
                    GetParameterTitle(ForumRssParameter.FeedDescription),
                    string.Format(GetMessageRaw("Param.FeedDescription.Default"), BXSite.Current.Name),
                    mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumRssParameter.StuffType), 
                new  BXParamSingleSelection(
                    GetParameterTitle(ForumRssParameter.StuffType),
                    ForumRssStuffType.Post.ToString("G"),
                    mainCategory
                )
            );

            string filtrationTypeKey = GetParameterKey(ForumRssParameter.FiltrationType);
            ParamsDefinition.Add(
                filtrationTypeKey,
                new BXParamSingleSelection(
                    GetParameterTitle(ForumRssParameter.FiltrationType),
                    ForumRssFiltrationType.None.ToString("G"),
                    mainCategory, 
                    null,
                    new ParamClientSideActionGroupViewSelector(ClientID, filtrationTypeKey)
                )
            );

            string categoryKey = GetParameterKey(ForumRssParameter.CategoryId);
            ParamsDefinition.Add(
                categoryKey,
                new BXParamSingleSelection(
                    GetParameterTitle(ForumRssParameter.CategoryId),
                    string.Empty,
                    mainCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, categoryKey, new string[] { ForumRssFiltrationType.CategoryId.ToString("G") })
                )
            );

            string forumKey = GetParameterKey(ForumRssParameter.ForumId);
            ParamsDefinition.Add(
                forumKey,
                new BXParamSingleSelection(
                    GetParameterTitle(ForumRssParameter.ForumId),
                    string.Empty,
                    mainCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, forumKey, new string[] { ForumRssFiltrationType.ForumId.ToString("G") })
                )
            );

            string topicKey = GetParameterKey(ForumRssParameter.TopicId);
            ParamsDefinition.Add(
                topicKey,
                new BXParamText(
                    GetParameterTitle(ForumRssParameter.TopicId),
                    string.Empty,
                    mainCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, topicKey, new string[] { ForumRssFiltrationType.TopicId.ToString("G") })
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumRssParameter.ItemQuantity),
                new BXParamText(
                    GetParameterTitle(ForumRssParameter.ItemQuantity),
                    "10",
                    mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumRssParameter.FeedUrlTemplate),
                new BXParamText(
                    GetParameterTitle(ForumRssParameter.FeedUrlTemplate),
                    string.Empty,
                    mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumRssParameter.FeedItemUrlTemplate),
                new BXParamText(
                    GetParameterTitle(ForumRssParameter.FeedItemUrlTemplate),
                    string.Empty,
                    mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumRssParameter.FeedTtl),
                new BXParamText(
                GetParameterTitle(ForumRssParameter.FeedTtl),
                    "60",
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumRssParameter.AvailableForumIdList),
                new BXParamMultiSelection(
                    GetParameterTitle(ForumRssParameter.AvailableForumIdList), 
                    string.Empty, 
                    additionalSettingsCategory
                )
            );

			ParamsDefinition.Add(
                GetParameterKey(ForumRssParameter.NoCData),
                new BXParamYesNo(
                    GetParameterTitle(ForumRssParameter.NoCData),
                    false,
                    additionalSettingsCategory
                )
            );
        }

        protected override void LoadComponentDefinition()
        {
            IList<BXParamValue> stuffTypeValues = ParamsDefinition[GetParameterKey(ForumRssParameter.StuffType)].Values;
            stuffTypeValues.Clear();
            stuffTypeValues.Add(new BXParamValue(GetParameterValueTitle(ForumRssParameter.StuffType, ForumRssStuffType.Topic.ToString("G")), ForumRssStuffType.Topic.ToString("G")));
            stuffTypeValues.Add(new BXParamValue(GetParameterValueTitle(ForumRssParameter.StuffType, ForumRssStuffType.Post.ToString("G")), ForumRssStuffType.Post.ToString("G")));

            IList<BXParamValue> filtrationTypeValues = ParamsDefinition[GetParameterKey(ForumRssParameter.FiltrationType)].Values;
            filtrationTypeValues.Clear();
            filtrationTypeValues.Add(new BXParamValue(GetParameterValueTitle(ForumRssParameter.FiltrationType, ForumRssFiltrationType.None.ToString("G")), ForumRssFiltrationType.None.ToString("G")));
            filtrationTypeValues.Add(new BXParamValue(GetParameterValueTitle(ForumRssParameter.FiltrationType, ForumRssFiltrationType.CategoryId.ToString("G")), ForumRssFiltrationType.CategoryId.ToString("G")));
            filtrationTypeValues.Add(new BXParamValue(GetParameterValueTitle(ForumRssParameter.FiltrationType, ForumRssFiltrationType.ForumId.ToString("G")), ForumRssFiltrationType.ForumId.ToString("G")));
            filtrationTypeValues.Add(new BXParamValue(GetParameterValueTitle(ForumRssParameter.FiltrationType, ForumRssFiltrationType.TopicId.ToString("G")), ForumRssFiltrationType.TopicId.ToString("G")));

            IList<BXParamValue> categoryValues = ParamsDefinition[GetParameterKey(ForumRssParameter.CategoryId)].Values;
            categoryValues.Clear();
            categoryValues.Add(new BXParamValue(GetMessageRaw("NotSelectedFeminine"), "0"));
            BXForumCategoryCollection categorires = BXForumCategory.GetList(
                null, 
                new BXOrderBy(
                    new BXOrderByPair(
                        BXForumCategory.Fields.Name, 
                        BXOrderByDirection.Asc
                    )
                )
            );
            foreach(BXForumCategory category in categorires)
                categoryValues.Add(new BXParamValue(category.TextEncoder.Decode(category.Name), category.Id.ToString()));

            IList<BXParamValue> forumValues = ParamsDefinition[GetParameterKey(ForumRssParameter.ForumId)].Values;
            forumValues.Clear();
            forumValues.Add(new BXParamValue(GetMessageRaw("NotSelectedMasculine"), "0"));
            IList<BXParamValue> availableForumValues = ParamsDefinition[GetParameterKey(ForumRssParameter.AvailableForumIdList)].Values;
            availableForumValues.Clear();
            /*
            BXForumCollection forums = BXForum.GetList(
                null,
                new BXOrderBy(
                    new BXOrderByPair(
                        BXForum.Fields.Name,
                        BXOrderByDirection.Asc
                    )
                )
            );
            */
            BXForumCollection forums = GetPermittedForums();
            foreach (BXForum forum in forums)
            {
                BXParamValue val = new BXParamValue(forum.TextEncoder.Decode(forum.Name), forum.Id.ToString());
                forumValues.Add(val);
                availableForumValues.Add(val);
            }
        }

        protected override string GetCacheOutput()
        {
            return ((ForumRssTemplate)ComponentTemplate).OutputXml;
        }

        protected override void SetCacheOutput(string output)
        {
            ((ForumRssTemplate)ComponentTemplate).OutputXml = output;
        }
    }

    /// <summary>
    /// Базовый класс для шаблонов компонента "ForumRssComponent"
    /// </summary>
    public abstract class ForumRssTemplate : BXComponentTemplate<ForumRssComponent> 
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
            if (Component.ComponentError != ForumRssError.None)
            {
                BXError404Manager.Set404Status(Response);
                BXPublicPage bitrixPage = Page as BXPublicPage;
                if (bitrixPage != null)
                    bitrixPage.Title = Component.ComponentErrorText;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            bool aboutError = Component.ComponentError != ForumRssError.None;

            if (aboutError)
            {
                writer.WriteBeginTag("span");
                writer.WriteAttribute("class", "errortext");
                writer.Write(HtmlTextWriter.TagRightChar);
                writer.Write(HttpUtility.HtmlEncode(Component.ComponentErrorText));
                writer.WriteEndTag("span");
                if (!IsComponentDesignMode && !BXConfigurationUtility.IsDesignMode)
                    Response.End();
                return;
            }

            if (string.IsNullOrEmpty(_outputXml))
            {
                BXRss20Channel feed = Component.Feed;

                using (MemoryStream ms = new MemoryStream())
                {
                    XmlWriterSettings s = new XmlWriterSettings();
                    s.CheckCharacters = false;
                    s.CloseOutput = false;
                    s.ConformanceLevel = ConformanceLevel.Document;
                    s.Indent = true;
                    s.Encoding = new UTF8Encoding(false);
                    System.Diagnostics.Stopwatch t = new System.Diagnostics.Stopwatch();
                    //using (XmlWriter xw = XmlWriter.Create(ms, s))
                    using (XmlWriter xw = new BXSyndicationXmlCheckingWriter(XmlWriter.Create(ms, s)))
                    {
                        feed.SaveToXml(xw, Component.FeedSavingOptions);
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
}
