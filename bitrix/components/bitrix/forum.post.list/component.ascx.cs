using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;

using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using System.Threading;
using Bitrix.CommunicationUtility;
using System.Text;
using System.Globalization;

namespace Bitrix.Forum.Components
{
    /// <summary>
    /// Параметры компонента "forum.post.list"
    /// </summary>
    public enum ForumPostListComponentParameter
    {
        /// <summary>
        /// Форумы (элемент фильтра)
        /// </summary>
        Forums = 1,
        /// <summary>
        /// Ид автора (элемент фильтра)
        /// </summary>
        AuthorId,
        /// <summary>
        /// Сортировать по полю сообщения
        /// </summary>
        SortBy,
        /// <summary>
        /// Направление сортировки
        /// </summary>
        SortDirection,
        /// <summary>
        /// Шаблон URL для чтения форума
        /// </summary>
        ForumReadUrlTemplate,
        /// <summary>
        /// Шаблон URL для чтения темы
        /// </summary>
        TopicReadUrlTemplate,
        /// <summary>
        /// Шаблон URL для чтения сообщения
        /// </summary>
        PostReadUrlTemplate,
        /// <summary>
        /// Шаблон URL для профиля автора
        /// </summary>
        AuthorProfileUrlTemplate,
        /// <summary>
        /// Максимальная длина слова
        /// </summary>
        MaxWordLength,
        /// <summary>
        /// Путь к css файлу с темой
        /// </summary>
        ThemeCssFilePath,
        ColorCssFilePath,
        /// <summary>
        /// Фильтр - дата сообщения с 
        /// </summary>
        DateCreateFrom,
        /// <summary>
        /// Фильтр - дата сообщения по
        /// </summary>
        DateCreateTo,
        /// <summary>
        /// Устанавливать заголовок страницы
        /// </summary>
        SetPageTitle,
        /// <summary>
        /// Режим отображения
        /// </summary>
        DisplayMode,
        /// <summary>
        /// Режим группировки
        /// </summary>
        GroupingOption

    }

    /// <summary>
    /// Ошибки компонента "forum.post.list"
    /// </summary>
    public enum ForumPostListComponentError
    {
        None = 0,
        General = 1,
        UnauthorizedReadAll = 2,
        PageDoesNotExist = 4,
        DataReadingFailed = 8,
        UserNotFound = 16,
		OperationError = 32
    }




	public partial class ForumPostListComponent : BXComponent
	{
		private IList<ForumPostListPostWrapper> posts = (IList<ForumPostListPostWrapper>)new ForumPostListPostWrapper[0];
        private Dictionary<int, BXForumPostChain> processors;
		private BXForumSignatureChain signatureProcessor = new BXForumSignatureChain();
		private bool templateIncluded;
		private BXParamsBag<object> replace;

		public enum PostOperation
		{
			Delete = 0
		}
       
        ForumPostListComponentError _componentError = ForumPostListComponentError.None;
        public ForumPostListComponentError ComponentError
        {
            get { return _componentError; }
        }

        BXParamsBag<object> _replaceParams = null;
        public BXParamsBag<object> ReplaceParams
        {
            get { return _replaceParams != null ? _replaceParams : (_replaceParams = new BXParamsBag<object>()); }
        }

        /// <summary>
        /// Получить ключ параметра
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetParameterKey(ForumPostListComponentParameter parameter)
        {
            return parameter.ToString();
        }

        /// <summary>
        /// Получить ключ заголовка параметра
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetParameterTilteKey(ForumPostListComponentParameter parameter)
        {
            return string.Concat("Param.", parameter.ToString("G"));
        }

        public string GetComponentErrorMessage(ForumPostListComponentError error)
        {
            return error != ForumPostListComponentError.None ? GetMessageRaw(string.Concat("Error.", error.ToString("G"))) : string.Empty;
        }
        public string[] GetComponentErrorMessages()
        {
            if (_componentError == ForumPostListComponentError.None)
                return new string[0];

            List<string> result = new List<string>();
            if ((_componentError & ForumPostListComponentError.General) != 0)
                result.Add(GetComponentErrorMessage(ForumPostListComponentError.General));
            if ((_componentError & ForumPostListComponentError.UnauthorizedReadAll) != 0)
                result.Add(GetComponentErrorMessage(ForumPostListComponentError.UnauthorizedReadAll));
            if ((_componentError & ForumPostListComponentError.PageDoesNotExist) != 0)
                result.Add(GetComponentErrorMessage(ForumPostListComponentError.PageDoesNotExist));
            if ((_componentError & ForumPostListComponentError.DataReadingFailed) != 0)
                result.Add(GetComponentErrorMessage(ForumPostListComponentError.DataReadingFailed));
            if ((_componentError & ForumPostListComponentError.UserNotFound) != 0)
                result.Add(GetComponentErrorMessage(ForumPostListComponentError.UserNotFound));
			if (commandErrorMessages != null)
				result.AddRange(commandErrorMessages.ToArray());
            return result.ToArray();
        }

        /// <summary>
        /// Ид автора
        /// </summary>
        public int AuthorId
        {
            get
            {
                return Parameters.GetInt(GetParameterKey(ForumPostListComponentParameter.AuthorId), 0);
            }
            set
            {
                Parameters[GetParameterKey(ForumPostListComponentParameter.AuthorId)] = value.ToString();
            }
        }

        /// <summary>
        /// Автор постов ( если указан, иначе NULL )
        /// </summary>
        BXForumUser author;
        public BXForumUser Author
        {
            get
            {
                return author;
            }
            set
            {
                author=value;
            }
        }

        /// <summary>
        /// Поле сортировки по умолчанию
        /// </summary>
        public string DefaultSortBy
        {
            get { return "ID"; }
        }

        /// <summary>
        /// Сортировать по полю сообщения
        /// </summary>
        public string SortBy
        {
            get
            {
                return Parameters.GetString(GetParameterKey(ForumPostListComponentParameter.SortBy), DefaultSortBy);
            }
            set
            {
                Parameters[GetParameterKey(ForumPostListComponentParameter.SortBy)] = value;
            }
        }

        /// <summary>
        /// Направление сортировки
        /// </summary>
        public BXOrderByDirection SortDirection
        {
            get
            {
                string r = Parameters.GetString(GetParameterKey(ForumPostListComponentParameter.SortDirection));
                if (string.IsNullOrEmpty(r))
                    return BXOrderByDirection.Asc;

                BXOrderByDirection result = BXOrderByDirection.Asc;
                try
                {

                    result = (BXOrderByDirection)Enum.Parse(typeof(BXOrderByDirection), r);
                    return Enum.IsDefined(typeof(BXOrderByDirection),result) ? result : BXOrderByDirection.Asc;

                }
                catch (Exception /*exc*/)
                {
                }
                return result;
            }
            set
            {
                Parameters[GetParameterKey(ForumPostListComponentParameter.SortDirection)] = value.ToString("G");
            }
        }

        public string DefaultForumReadUrlTemplate
        {
            get { return "forum.aspx?forum=#ForumId#"; }
        }

        /// <summary>
        /// Шаблон страницы чтения форума
        /// </summary>
        public string ForumReadUrlTemplate
        {
            get
            {
                return Parameters.GetString(GetParameterKey(ForumPostListComponentParameter.ForumReadUrlTemplate), DefaultForumReadUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumPostListComponentParameter.ForumReadUrlTemplate)] = value;
            }
        }

        public ForumPostListDisplayMode ComponentDisplayMode 
        {
            get
            {
                ForumPostListDisplayMode mode = ForumPostListDisplayMode.AllPosts;

                try
                {
                    mode = (ForumPostListDisplayMode)Enum.Parse(typeof(ForumPostListDisplayMode),
                            Parameters.Get<String>(GetParameterKey(ForumPostListComponentParameter.DisplayMode),
                                           "AllPosts"));
                    return Enum.IsDefined(typeof(ForumPostListDisplayMode), mode) ? mode : ForumPostListDisplayMode.AllPosts;
                }
                catch { }
                return mode;
            }
            set
            {
                if (Parameters.ContainsKey(GetParameterKey(ForumPostListComponentParameter.DisplayMode)))
                    Parameters[GetParameterKey(ForumPostListComponentParameter.DisplayMode)] = value.ToString();
                else
                    Parameters.Add(GetParameterKey(ForumPostListComponentParameter.DisplayMode), value.ToString());
            }
        }

        public DateTime DateCreateFrom
        {
            get
            {
                DateTime dt;
                if (DateTime.TryParse(Parameters.Get<string>(GetParameterKey(ForumPostListComponentParameter.DateCreateFrom)),
                                        DateTimeFormatInfo.InvariantInfo,
                                        DateTimeStyles.None, 
                                        out dt)) 
                    return dt;
                
                else return DateTime.MinValue;
            }
            set {
                if (Parameters.ContainsKey(GetParameterKey(ForumPostListComponentParameter.DateCreateFrom)))
                    Parameters[GetParameterKey(ForumPostListComponentParameter.DateCreateFrom)] = value.ToString();
                else
                    Parameters.Add(GetParameterKey(ForumPostListComponentParameter.DateCreateFrom), value.ToString());
            }
     
	    }

        public DateTime DateCreateTo
        {
            get
            {
                DateTime dt;
                if (DateTime.TryParse(Parameters.Get<string>(GetParameterKey(ForumPostListComponentParameter.DateCreateTo)),
                                        DateTimeFormatInfo.InvariantInfo,
                                        DateTimeStyles.None,
                                        out dt))
                    return dt;

                else return DateTime.MaxValue;
            }
            set
            {
                if (Parameters.ContainsKey(GetParameterKey(ForumPostListComponentParameter.DateCreateTo)))
                    Parameters[GetParameterKey(ForumPostListComponentParameter.DateCreateTo)] = value.ToString();
                else
                    Parameters.Add(GetParameterKey(ForumPostListComponentParameter.DateCreateTo), value.ToString());
            }

        }

        public ForumPostListGroupingOption ComponentGroupingOption
        {
            get
            {
                ForumPostListGroupingOption grouping = ForumPostListGroupingOption.None;
                try
                {
                    grouping = (ForumPostListGroupingOption)Enum.Parse(typeof(ForumPostListGroupingOption),
                            Parameters.Get<String>( GetParameterKey(ForumPostListComponentParameter.GroupingOption), 
                                                    "None"));
                                                    
                    return Enum.IsDefined(typeof(ForumPostListGroupingOption), grouping) ? grouping : ForumPostListGroupingOption.None;
                }
                catch { }
                return grouping;
            }
            set
            {
                if (Parameters.ContainsKey(GetParameterKey(ForumPostListComponentParameter.GroupingOption)))
                    Parameters[GetParameterKey(ForumPostListComponentParameter.GroupingOption)] = value.ToString();
                else
                    Parameters.Add(GetParameterKey(ForumPostListComponentParameter.GroupingOption), value.ToString());
            }
        }

		public List<int> Forums
		{
			get
			{
              return Parameters.GetListInt(GetParameterKey(ForumPostListComponentParameter.Forums));
			}
            set
            {
                if (value == null || value.Count == 0)
                    Parameters[GetParameterKey(ForumPostListComponentParameter.Forums)] = string.Empty;
                else
                {
                    string[] arr = new string[value.Count];
                    for (int i = 0; i < value.Count; i++)
                        arr[i] = value[i].ToString();
                    Parameters[GetParameterKey(ForumPostListComponentParameter.Forums)] = BXStringUtility.ListToString(arr);
                }
            }
		}

        /// <summary>
        /// Шаблон страницы чтения темы
        /// </summary>

		public string TopicReadUrlTemplate
		{
            get
            {
                return Parameters.GetString(GetParameterKey(ForumPostListComponentParameter.TopicReadUrlTemplate), DefaultTopicReadUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumPostListComponentParameter.TopicReadUrlTemplate)] = value;
            }
		}

        public string DefaultTopicReadUrlTemplate
        {
            get { return "forum.aspx?forum=#ForumId#&topic=#TopicId#"; }
        }

        public string DefaultPostReadUrlTemplate
        {
            get { return "forum.aspx?forum=#ForumId#&topic=#TopicId#&post=#PostId#"; }
        }
		int curUserId = -1;
		public int CurrentUserId
		{
			get
			{
				return curUserId == -1 ? (curUserId = BXPrincipal.Current.Identity.IsAuthenticated ? ((BXIdentity)BXPrincipal.Current.Identity).Id : 0) : curUserId;
			}
		}

		public string PostReadUrlTemplate
		{
            get
            {
                return Parameters.GetString(GetParameterKey(ForumPostListComponentParameter.PostReadUrlTemplate), DefaultPostReadUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumPostListComponentParameter.PostReadUrlTemplate)] = value;
            }
		}

        public string DefaultAuthorProfileUrlTemplate
        {
            get { return "forum.aspx?user=#AuthorId#"; }
        }

		public string AuthorProfileUrlTemplate
		{
            get
            {
                return Parameters.GetString(GetParameterKey(ForumPostListComponentParameter.AuthorProfileUrlTemplate), DefaultAuthorProfileUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumPostListComponentParameter.AuthorProfileUrlTemplate)] = value;
            }
		}

        public int DefaultMaxWordLength
        {
            get { return 50; }
        }

        /// <summary>
        /// Максимальная длина слова
        /// </summary>
        public int MaxWordLength
        {
            get
            {
                return Parameters.GetInt(GetParameterKey(ForumPostListComponentParameter.MaxWordLength), DefaultMaxWordLength);
            }
            set
            {
                Parameters[GetParameterKey(ForumPostListComponentParameter.MaxWordLength)] = value.ToString();
            }
        }

		private IList<ForumPostListPostWrapper> _postListRO = null;
		public IList<ForumPostListPostWrapper> PostList
        {
			get { return _postListRO ?? (_postListRO = new ReadOnlyCollection<ForumPostListPostWrapper>(posts)); }
        }

        /// <summary>
        /// Количество сообщений
        /// </summary>
        public int PostCount
        {
            get
            {
                object val;
                return ComponentCache.TryGetValue("PostCount", out val) ? (int)val : posts.Count;
            }
            private set
            {
                ComponentCache["PostCount"] = value;
            }
        }

        String profileUrl;

        protected String AuthorProfileUrl
        {
            get
            {
                if (profileUrl != null) return profileUrl;
                if (AuthorId <= 0) return profileUrl = String.Empty;
                BXParamsBag<object> replace = new BXParamsBag<object>();
                replace.Add("UserId", AuthorId);
                return profileUrl = ResolveTemplateUrl(AuthorProfileUrlTemplate, replace);
            }
        }

        String emptyMessage;
        public String EmptyMessage
        {
            get
            {
                if (emptyMessage != null) return emptyMessage;
                if (AuthorId > 0)
                {
                    return emptyMessage = String.Format(GetMessage("Message.NoPostsFound"), " <a href="+ AuthorProfileUrl+ "> "+
                        BXTextEncoder.HtmlTextEncoder.Encode(Author.User.GetDisplayName())+"</a> ");
                }
                return GetMessage("Message.NoPosts");
            }
        }


        /// <summary>
        /// Запрос наличия сообщений
        /// </summary>
        public bool HasPosts
        {
            get { return PostCount > 0; }
        }

        protected void AddError(ForumPostListComponentError error)
        {
            _componentError |= error;
            templateIncluded = true;
            IncludeComponentTemplate();
            
        }

        protected BXForumPostChain GetProcessor(BXForum forum)
        {
            if (!processors.ContainsKey(forum.Id)) 
                processors.Add(forum.Id, new BXForumPostChain(forum));
            return processors[forum.Id];
        }

        #region data getters

        protected void FillPostWrappersFromTopics(BXFilter filter, BXOrderBy orderBy,BXQueryParams queryParams)
        {
            BXSelectAdd selectAdd = new BXSelectAdd(BXForumTopic.Fields.FirstPost);
            selectAdd.Add(BXForumTopic.Fields.Forum);
            if (Author == null)
            {
                selectAdd.Add(BXForumTopic.Fields.Author.User);
                selectAdd.Add(BXForumTopic.Fields.Author);
            }
            BXForumTopicCollection topics = BXForumTopic.GetList(filter, orderBy,selectAdd,queryParams);
			this.posts = topics.ConvertAll<ForumPostListPostWrapper>(delegate(BXForumTopic input)
            {
				ForumPostListPostWrapper info = new ForumPostListPostWrapper(input.FirstPost, GetProcessor(input.Forum), this);
                info.content = input.FirstPost.Post;

                info.Author = Author ?? input.Author;

                info.signature = info.Author != null ? info.Author.TextEncoder.Decode(info.Author.Signature) : String.Empty;
                info.AuthorName = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.AuthorName), MaxWordLength, true);
                info.TopicTitleHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Name), MaxWordLength, true);
                info.TopicDescriptionHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Description), MaxWordLength, true);
                info.TopicLastPosterNameHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.LastPosterName), MaxWordLength, true);
                info.TopicReplies = input.Replies;
                info.Forum = input.Forum;
                info.Topic = input;
                info.signatureChain = signatureProcessor;
                info.TopicHasNewPosts = false;
                return info;
            }
            );
        }
        //all posts of user
        protected void FillAllPosts(BXFilter filter,BXOrderBy orderBy,BXQueryParams queryParams)
        {

            BXForumPostCollection postIdCol= BXForumPost.GetList(
                                                filter, 
                                                orderBy,
                                                new BXSelect(BXSelectFieldPreparationMode.Normal, BXForumPost.Fields.Id),
                                                queryParams
                                                );
            BXSelectAdd selectAdd = new BXSelectAdd(
                                                BXForumPost.Fields.Topic,
                                                BXForumPost.Fields.Forum
                                                );

            if (Author == null)
            {    
                selectAdd.Add(BXForumPost.Fields.Author);
                selectAdd.Add(BXForumPost.Fields.Author.User);
            }

            BXForumPostCollection postCol = BXForumPost.GetList(
                                                new BXFilter(new BXFilterItem(
                                                    BXForumPost.Fields.Id,
                                                    BXSqlFilterOperators.In,
                                                    postIdCol.ConvertAll<long>(
                                                        delegate(BXForumPost post) { return post.Id; }
                                                        ))),
                                                orderBy,
                                                selectAdd,
                                                null
                                                );



			this.posts = postCol.ConvertAll<ForumPostListPostWrapper>(delegate(BXForumPost input)
                {
					ForumPostListPostWrapper info = new ForumPostListPostWrapper(input, GetProcessor(input.Forum), this);
                    info.Author = Author ?? input.Author;
                    info.content = input.Post;
                    info.signatureChain = signatureProcessor;
                    info.signature = info.Author != null ? info.Author.TextEncoder.Decode(info.Author.Signature) : String.Empty;
                    info.AuthorName = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.AuthorName), MaxWordLength, true);
                    info.TopicTitleHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Topic.Name), MaxWordLength, true);
                    
                    info.Forum = input.Forum;
                    info.Topic = input.Topic;
                    return info;
                });
        }

        #endregion

        protected BXQueryParams GetQueryParams(int count)
        {
            //Determine page
            BXPagingParams pagingParams = PreparePagingParams();

            bool legal;
            BXQueryParams queryParams = PreparePaging(
               pagingParams,
               delegate
               {
                   return count;
               },
                 replace,
                 out legal
            );
            if (!legal)
            {
                AddError(ForumPostListComponentError.PageDoesNotExist);
                return null;
            }
            return queryParams;
        }

        protected void MakePostFilter(BXFilter filter)
        {
            if (DateCreateFrom != DateTime.MinValue)
                filter.Add(new BXFilterItem(BXForumPost.Fields.DateCreate, BXSqlFilterOperators.GreaterOrEqual, DateCreateFrom));

            if (DateCreateTo != DateTime.MaxValue)
                filter.Add(new BXFilterItem(BXForumPost.Fields.DateCreate, BXSqlFilterOperators.LessOrEqual, DateCreateTo.AddDays(1).AddMilliseconds(-1)));

            filter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));
            filter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Approved, BXSqlFilterOperators.Equal, true));

            if (AuthorId > 0)
                filter.Add(new BXFilterItem(BXForumPost.Fields.Author.Id, BXSqlFilterOperators.Equal, AuthorId));

            filter.Add(new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.In, ForumsForSelect));

        }

        protected void MakeTopicsOrderBy(BXOrderBy orderBy)
        {
			orderBy.Add(BXForumTopic.Fields.LastPostDate, BXOrderByDirection.Desc);
			orderBy.Add(BXForumTopic.Fields.Id, BXOrderByDirection.Desc);
        }

        List<int> forumsForSelect=null;

        protected List<int> ForumsForSelect
        {
            get
            {
                if (forumsForSelect==null)
                {
                    BXFilter forumsFilter = new BXFilter();
                    forumsFilter.Add(new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true));
                    forumsFilter.Add(new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, BXSite.Current.Id));
                    forumsFilter.Add(new BXFilterItem(BXForum.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "ForumPublicRead"));

                    if (Forums.Count > 0)
                        forumsFilter.Add(new BXFilterItem(BXForum.Fields.Id, BXSqlFilterOperators.In, Forums.ToArray()));

                    BXForumCollection forums = BXForum.GetList(forumsFilter, null, new BXSelect(BXForum.Fields.Id), null);
                    return forumsForSelect = forums.ConvertAll<int>(delegate(BXForum input)
                            {
                                return input.Id;
                            });
                }
                return forumsForSelect;
            }
        }

        internal void SetPageTitle(BXUser author)
        {
            BXPublicPage bitrixPage = Page as BXPublicPage;
            if (bitrixPage != null && !IsComponentDesignMode)
            {
                if (Parameters.GetBool("SetPageTitle", true))
                {
                    string title = author!=null ? String.Concat(GetMessageRaw("Page.PageTitle"), " ", 
                        author.GetDisplayName()) : GetMessageRaw("Page.PageTitle."+ComponentDisplayMode.ToString());
                    if (!string.IsNullOrEmpty(title))
                    {
                        bitrixPage.MasterTitleHtml = BXWordBreakingProcessor.Break(title, MaxWordLength, true);
                        bitrixPage.Title = Encode(title);
                    }
                }
            }
        }

		protected void Page_Load(object sender, EventArgs e)
		{
            if (Parameters["Forums"] == "-1") return;// для комплексного компонента, если шел postBack по кнопке, не нужно ничего делать
			CacheMode = BXCacheMode.None;
		    replace = new BXParamsBag<object>();
            processors = new Dictionary<int, BXForumPostChain>();
            int count;
            if (AuthorId > 0)
                {

                    BXForumUserCollection authors = BXForumUser.GetList(
                        new BXFilter(
                            new BXFilterItem(BXForumUser.Fields.Id, BXSqlFilterOperators.Equal, AuthorId),
                            new BXFilterItem(BXForumUser.Fields.User.IsApproved, BXSqlFilterOperators.Equal, true)
                        ),
                        null,
                        new BXSelectAdd(BXForumUser.Fields.User),
                        null,
                        BXTextEncoder.EmptyTextEncoder
                     );

                    if (authors.Count == 0)
                    {
                        AddError(ForumPostListComponentError.UserNotFound);
                        return;
                    }
                    Author = authors[0];
                    BXUser author = authors[0].User;
                    if (author != null)
                        SetPageTitle(author);
                }
                else
                {
                    if ( ComponentDisplayMode!=ForumPostListDisplayMode.ActiveTopics && ComponentDisplayMode!=ForumPostListDisplayMode.UnAnsweredTopics )
                        ComponentDisplayMode = ForumPostListDisplayMode.AllPosts;
                    ComponentGroupingOption = ForumPostListGroupingOption.None;
                }
                
                if ( replace.ContainsKey("UserId")) replace["UserId"] = (object)AuthorId;
                else replace.Add("UserId",(object)AuthorId);
                if (Parameters.ContainsKey("UserPostsTemplate")) Parameters["UserPostsTemplate"] =
                   ResolveTemplateUrl(Parameters["UserPostsTemplate"], replace); 
                
                 BXFilter filter = new BXFilter();
                 BXOrderBy orderBy = new BXOrderBy();
                 string sortBy = SortBy;
                    

                 switch (ComponentDisplayMode)
                    {
                        case ForumPostListDisplayMode.AllPosts:

                            MakePostFilter(filter);

                            if (ComponentGroupingOption == ForumPostListGroupingOption.GroupByTopic)
                                orderBy.Add(BXForumPost.Fields.Topic.Id, SortDirection);
                            orderBy.Add(BXForumPost.Fields.GetFieldByKey(string.IsNullOrEmpty(sortBy) ? sortBy : DefaultSortBy), SortDirection);
                            count = BXForumPost.Count(filter);
                            FillAllPosts(filter, orderBy, GetQueryParams(count));
                            break;

                        case ForumPostListDisplayMode.AllTopicsCreated:

                            if (DateCreateFrom != DateTime.MinValue)
                                filter.Add(new BXFilterItem(BXForumTopic.Fields.DateCreate, BXSqlFilterOperators.GreaterOrEqual, DateCreateFrom));

                            if (DateCreateTo != DateTime.MaxValue)
                                filter.Add(new BXFilterItem(BXForumTopic.Fields.DateCreate, BXSqlFilterOperators.LessOrEqual, DateCreateTo.AddDays(1).AddMilliseconds(-1)));

                            filter.Add(new BXFilterItem(BXForumTopic.Fields.Approved, BXSqlFilterOperators.Equal, 1));
                            filter.Add(new BXFilterItem(BXForumTopic.Fields.Author.Id, BXSqlFilterOperators.Equal, AuthorId));

                            filter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.In, ForumsForSelect));

                            orderBy.Add(BXForumTopic.Fields.GetFieldByKey(string.IsNullOrEmpty(sortBy) ? sortBy : DefaultSortBy), SortDirection);

                            count = BXForumTopic.Count(filter);
                            FillPostWrappersFromTopics(filter, orderBy, GetQueryParams(count));
                            break;

                        case ForumPostListDisplayMode.AllTopicsParticipated:

                            MakePostFilter(filter);

                            filter.Add(new BXFilterItem(BXForumPost.Fields.LastPostInTopicByAuthor, BXSqlFilterOperators.Equal, AuthorId));
                               
                                
                            count = BXForumTopic.GetCountWhereAuthorParticipated(AuthorId,ForumsForSelect.ToArray(),null);
                            orderBy.Add(BXForumPost.Fields.Topic.Id, SortDirection);
                            FillAllPosts(filter, orderBy, GetQueryParams(count));

                            break;

                        case ForumPostListDisplayMode.UnAnsweredTopics:
                        case ForumPostListDisplayMode.ActiveTopics:
                            MakeTopicsOrderBy(orderBy);

                            if (ComponentDisplayMode == ForumPostListDisplayMode.ActiveTopics)

                                filter.Add(new BXFilterItem(BXForumTopic.Fields.LastPostDate, BXSqlFilterOperators.Greater, DateTime.Now.AddDays(-1)));

                            else

                                filter.Add(new BXFilterItem(BXForumTopic.Fields.Replies, BXSqlFilterOperators.Equal, 0));

                            
                            filter.Add(new BXFilterItem(BXForumTopic.Fields.Approved, BXSqlFilterOperators.Equal, 1));
                            filter.Add(new BXFilterItem(BXForumTopic.Fields.MovedToTopic.Id, BXSqlFilterOperators.Equal, 0));
                            filter.Add(new BXFilterItem(BXForumTopic.Fields.FirstPost.Id, BXSqlFilterOperators.Greater, 0));

                            filter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.In, ForumsForSelect));

                            count = BXForumTopic.Count(filter);
                            FillPostWrappersFromTopics(filter, orderBy, GetQueryParams(count));
                            SetPageTitle(null);  
                            break;
                    }
                    if (!templateIncluded)
                    {
                        templateIncluded = true;
                        IncludeComponentTemplate();
                    }
                

		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

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
				catch
				{
				}
			}

		}
		protected override void PreLoadComponentDefinition()
		{
			
            Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = new BXComponentGroup("forum", GetMessageRaw("Group"), 110, BXComponentGroup.Communication);

			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory mainCategory = BXCategory.Main;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

			BXParametersDefinition.SetPaging(
                ParamsDefinition, 
                ClientID, 
                BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);


			BXParametersDefinition.SetPagingUrl(
                ParamsDefinition, 
                ClientID, 
                BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            
            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.AuthorId),
                new BXParamText(GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.AuthorId)), 
                "0", 
                mainCategory
                )
            );
            
            List<BXParamValue> displayParams = new List<BXParamValue>();
            foreach (string name in Enum.GetNames(typeof(ForumPostListDisplayMode)))
            {
                displayParams.Add(new BXParamValue(
                    GetMessageRaw("Param.DisplayMode." +name),name));
            }

            string clientSideActionGroupViewId = ClientID;
            ParamsDefinition.Add(
                "DisplayMode", 
                new BXParamSingleSelection(
                    GetMessageRaw("Param.DisplayMode"),
                    ForumPostListDisplayMode.AllPosts.ToString(), 
                    mainCategory,
                    displayParams,
                    new ParamClientSideActionGroupViewSelector(clientSideActionGroupViewId,"ShowGrouping")
                )
            );

            displayParams.Clear();

            displayParams.Add(new BXParamValue(GetMessageRaw("Param.GroupByNone"), ForumPostListGroupingOption.None.ToString()));
            displayParams.Add(new BXParamValue(GetMessageRaw("Param.GroupByTopic"), ForumPostListGroupingOption.GroupByTopic.ToString()));

            ParamsDefinition.Add(
                "GroupingOption",
                new BXParamSingleSelection(
                GetMessageRaw("Param.GroupingOptions"),
                ForumPostListGroupingOption.None.ToString(),
                mainCategory,
                displayParams,
                new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "GroupingOption", new string[] { "AllPosts" })
                )
            );
 
            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.DateCreateFrom),
                new BXParamDate(GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.DateCreateFrom)), mainCategory)
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.DateCreateTo),
                new BXParamDate(GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.DateCreateTo)), mainCategory)
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.Forums),
                new BXParamMultiSelection(
                GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.Forums)),
                string.Empty,
                mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.SetPageTitle),
                new BXParamYesNo(
                GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.SetPageTitle)),
                true,
                mainCategory
                 )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.ThemeCssFilePath), 
                new BXParamText( GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.ThemeCssFilePath)),
                                "~/bitrix/components/bitrix/forum/templates/.default/style.css", 
                                mainCategory
                                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.ColorCssFilePath),
                new BXParamText(GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.ColorCssFilePath)),
                    "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css",
                    mainCategory
                    )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.SortBy),
                new BXParamSingleSelection(
                GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.SortBy)),
                DefaultSortBy,
                mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.SortDirection),
                new BXParamSingleSelection(
                    GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.SortDirection)),
                    BXOrderByDirection.Asc.ToString("G"),
                    mainCategory
                    )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.ForumReadUrlTemplate),
                new BXParamText(
                GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.ForumReadUrlTemplate)),
                DefaultForumReadUrlTemplate,
                mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.TopicReadUrlTemplate),
                new BXParamText(
                    GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.TopicReadUrlTemplate)),
                    DefaultTopicReadUrlTemplate,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.PostReadUrlTemplate),
                new BXParamText(
                    GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.PostReadUrlTemplate)),
                    DefaultPostReadUrlTemplate,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.AuthorProfileUrlTemplate),
                new BXParamText(
                    GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.AuthorProfileUrlTemplate)),
                    DefaultAuthorProfileUrlTemplate,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumPostListComponentParameter.MaxWordLength),
                new BXParamText(
                    GetMessageRaw(GetParameterTilteKey(ForumPostListComponentParameter.MaxWordLength)),
                    DefaultMaxWordLength.ToString(),
                    mainCategory
                    )
                );
        
        }

        private IList<BXParamValue> BuildUpParameterValueList(ForumPostListComponentParameter parameter, Type parameterValueEnumType, IList<BXParamValue> parameterValueList)
        {
            string key = GetParameterKey(parameter);
            if (parameterValueList == null)
            {
                parameterValueList = ParamsDefinition[key].Values;
                if (parameterValueList.Count > 0)
                    parameterValueList.Clear();
            }
            foreach (string s in Enum.GetNames(parameterValueEnumType))
                parameterValueList.Add(new BXParamValue(GetMessageRaw(string.Concat(GetParameterTilteKey(parameter), ".", s)), s));
            return parameterValueList;
        }

		protected override void LoadComponentDefinition()
		{
			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			BXForumCollection forums = BXForum.GetList(
				new BXFilter(
					new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
				),
				new BXOrderBy(
					new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)
				),
				new BXSelect(BXForum.Fields.Id, BXForum.Fields.Name),
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			ParamsDefinition[GetParameterKey(ForumPostListComponentParameter.Forums)].Values = forums.ConvertAll<BXParamValue>(delegate(BXForum input)
			{
				return new BXParamValue(input.Name, input.Id.ToString());
			});

            List<BXParamValue> sortByValues = ParamsDefinition[GetParameterKey(ForumPostListComponentParameter.SortBy)].Values;
            if (sortByValues.Count > 0)
                sortByValues.Clear();
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.ID"), "ID"));
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.DateCreate"), "DateCreate"));

            ParamsDefinition[GetParameterKey(ForumPostListComponentParameter.SortBy)].Values = sortByValues;

            BuildUpParameterValueList(ForumPostListComponentParameter.SortDirection, typeof(BXOrderByDirection), null);
		}

		private string MakeHref(string template, BXParamsBag<object> parameters)
		{
			return Encode(ResolveTemplateUrl(template, parameters));
        }

		private List<string> commandErrorMessages;

		public override bool ProcessCommand(string commandName, BXParamsBag<object> commandParameters, List<string> commandErrors)
		{
			if (commandName.Equals("delete", StringComparison.OrdinalIgnoreCase))
			{
				List<long> posts = (List<long>)commandParameters["posts"];
				foreach (int id in posts)
				{
					BXForumPost post = BXForumPost.GetById(id);
					if (post == null) continue;
					BXForumAuthorization auth = new BXForumAuthorization(post.ForumId, CurrentUserId, post.Topic, post);
					if (auth.CanDeleteThisPost)
						try
						{
							if (post.Topic.FirstPostId == post.Id)
							{
								if (auth.CanDeleteThisTopic)
									post.Topic.Delete();
								else
									throw new Exception(GetMessage("Error.UnauthorizedDeletePost"));
							}
							else 
							post.Delete();
						}
						catch (Exception e)
						{
							commandErrors.Add(GetMessage("Error.DeleteFailed")+": "+e.ToString());
						}
					else 
						commandErrors.Add(GetMessage("Error.UnauthorizedDeletePost"));
				}
				if (commandErrors.Count == 0)
				{
					Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
				}
				else
				{
					commandErrorMessages = new List<string>();
					commandErrorMessages.AddRange(commandErrors);
					_componentError = ForumPostListComponentError.OperationError;
				}
			}
			return false;
		}

        public class LinkInfo
		{
			private string title;
			private string url;
			private string cssClass;

			public string Title
			{
				get
				{
					return HttpUtility.HtmlEncode(title ?? string.Empty);
				}
			}
			public string Url
			{
				get
				{
					return url;
				}
			}

			public string Href
			{
				get
				{
					return HttpUtility.HtmlAttributeEncode(url ?? string.Empty);
				}
			}
			public string CssClass
			{
				get
				{
					return cssClass != null ? HttpUtility.HtmlAttributeEncode(cssClass) : null;
				}
			}

			internal LinkInfo(string url, string title, string cssClass)
			{
				this.url = url;
				this.title = title;
				this.cssClass = cssClass;
			}
        }

    }

    #region Enumerations

    public enum ForumPostListDisplayMode
    {
        AllPosts,
        AllTopicsCreated,
        AllTopicsParticipated,
        ActiveTopics,
        UnAnsweredTopics
    }
    public enum ForumPostListGroupingOption
    {
        None,
        GroupByTopic
    }
    #endregion

	public class ForumPostListTemplate : BXComponentTemplate<ForumPostListComponent>
	{
        protected string[] GetErrorMessages()
        {
            string[] result = Component.GetComponentErrorMessages();

            if (result.Length > 0)
                for (int i = 0; i < result.Length; i++)
                    result[i] = HttpUtility.HtmlEncode(result[i]);

            return result;
        }
		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			base.Render(writer);
		}
    }
    #region ForumPostWrapper
    public class ForumPostListPostWrapper
    {

        internal BXForum forum;
        internal BXForumTopic topic;
        internal BXForumSignatureChain signatureChain;
        private ForumPostListComponent component;
        private BXForumPost post;
        private BXForumUser author;
        private string authorNameHtml;
        private string contentHtml;
        private string signatureHtml;
        private string topicTitleHtml;
        private string topicDescriptionHtml;
        private string topicLastPostHref;
        private bool hasNewPosts;
        private int replies;
        private string lastPosterNameHtml;
        internal string content;
        internal string signature;
        private BXForumPostChain processor;
        private string forumName;
        private string topicName = null;
		private BXForumAuthorization auth;

        public ForumPostListPostWrapper() { }

		public ForumPostListPostWrapper(BXForumPost post, BXForumPostChain processor, ForumPostListComponent component)
        {
            if (post == null)
                throw new ArgumentNullException("post");
            this.post = post;

            if (processor == null)
                throw new ArgumentNullException("processor");
            this.processor = processor;


            if (component == null)
                throw new ArgumentNullException("component");
            this.component = component;
        }

		public bool CanDeleteTopic
		{
			get
			{
				if (auth == null)
					auth = new BXForumAuthorization(this.post.ForumId, component.CurrentUserId, this.post.Topic,this.post);
				return auth.CanDeleteThisTopic;
			}
		}

		public bool CanDeletePost
		{
			get
			{
				if (auth == null)
					auth = new BXForumAuthorization(this.post.ForumId, component.CurrentUserId, this.post.Topic,this.post);
				return auth.CanDeleteThisPost;
			}
		}

        public BXForumPostChain Processor
        {
            get { return processor; }
            set { processor = value; }
        }

        public BXForumPost Post
        {
            get
            {
                return post;
            }
            internal set
            {
                post = value;
            }
        }

        private string BreakWord(string source, int maxLength, bool encode)
        {
            return !string.IsNullOrEmpty(source) ? BXWordBreakingProcessor.Break(source, maxLength, encode) : string.Empty;
        }


        /// <summary>
        /// Имя темы (Закодировано в Html)
        /// </summary>
        public string TopicName
        {
            get
            {
                return topicName ?? (topicName = post.Topic != null ? BreakWord(post.Topic.Name, component.MaxWordLength, false) : string.Empty);
            }
        }

        public string AuthorName
        {
            get
            {
                return authorNameHtml;
            }
            internal set
            {
                authorNameHtml = value;
            }
        }

        public string TopicTitleHtml
        {
            get
            {
                return topicTitleHtml;
            }
            internal set
            {
                topicTitleHtml = value;
            }
        }

        public string TopicLastPosterNameHtml
        {
            get
            {
                return lastPosterNameHtml;
            }
            internal set
            {
                lastPosterNameHtml = value;
            }
        }

        public string TopicDescriptionHtml
        {
            get
            {
                return topicDescriptionHtml;
            }
            internal set
            {
                topicDescriptionHtml = value;
            }
        }

        public int TopicReplies
        {
            get 
            { 
                return replies; 
            }
            set 
            { 
                replies = value; 
            }
        }

        public bool TopicHasNewPosts
        {
            get
            {
                return hasNewPosts;
            }
            set
            {
                hasNewPosts = value;
            }
        }



        /// <summary>
        /// Имя форума (Закодировано в Html)
        /// </summary>
        public string ForumName
        {
            get
            {
                return forumName ?? (forumName = post.Forum != null ? BreakWord(post.Forum.Name, component.MaxWordLength, false) : string.Empty);
            }
        }

        public string ContentHtml
        {
            get
            {

                if (contentHtml != null) return contentHtml;
                if (processor == null) return String.Empty;
                return processor.Process(content);

            }
        }
        public string AuthorSignatureHtml
        {
            get
            {
                return signatureHtml ?? (signatureHtml = signatureChain.Process(signature));
            }
        }

        public BXForumUser Author
        {
            get
            {
                return author;
            }
            internal set
            {
                author = value;
            }
        }

        public BXForum Forum
        {
            get { return forum; }
            set { forum = value; }
        }
        public BXForumTopic Topic
        {
            get { return topic; }
            set { topic = value; }
        }




        private string _forumReadUrl = null;
        /// <summary>
        /// URL страницы чтения форума
        /// Закодировано в HtmlAttribute
        /// </summary>
        public string ForumReadUrl
        {
            get
            {
				if (_forumReadUrl != null)
					return _forumReadUrl;

				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace.Add("ForumId", post.ForumId);
                return _forumReadUrl = component.ResolveTemplateUrl(component.ForumReadUrlTemplate, replace);
            }
        }

        private string _topicReadUrl = null;
        /// <summary>
        /// URL страницы чтения темы
        /// Закодировано в HtmlAttribute
        /// </summary>
        public string TopicReadUrl
        {
            get
            {
				if (_topicReadUrl != null) 
					return _topicReadUrl;

                BXParamsBag<object> replace = new BXParamsBag<object>();
                replace.Add("ForumId", post.ForumId);
                replace.Add("TopicId", post.TopicId);
                return _topicReadUrl = component.ResolveTemplateUrl(component.TopicReadUrlTemplate, replace);
            }

            set
            {
                _topicReadUrl = HttpUtility.HtmlAttributeEncode(value);
            }
        }

        private string _postReadUrl = null;
        /// <summary>
        /// URL страницы чтения сообщения
        /// Закодировано в HtmlAttribute
        /// </summary>
        public string PostReadUrl
        {
            get
            {
                if (_postReadUrl != null) 
					return _postReadUrl;

                BXParamsBag<object> replace = new BXParamsBag<object>();
                replace.Add("ForumId", post.ForumId);
                replace.Add("TopicId", post.TopicId);
                replace.Add("PostId", post.Id);
                return _postReadUrl = component.ResolveTemplateUrl(component.PostReadUrlTemplate,replace);
            }

            set
            {
                _postReadUrl = HttpUtility.HtmlAttributeEncode(value);
            }
        }

        private string _authorProfileUrl = null;
        /// <summary>
        /// URL страницы профиля пользователя
        /// Закодировано в HtmlAttribute
        /// </summary>
        public string AuthorProfileUrl
        {
            get
            {
                if (_authorProfileUrl!=null ) return _authorProfileUrl;
                BXParamsBag<object> replace = new BXParamsBag<object>();
                if (author == null) return String.Empty;
                replace.Add("UserId",author.Id);
                return _authorProfileUrl = component.ResolveTemplateUrl(component.AuthorProfileUrlTemplate, replace);
            }
        }

        private string ResolveUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            int whatInd = url.IndexOf('?');
            string path = whatInd >= 0 ? path = url.Substring(0, whatInd) : url;
            if (!VirtualPathUtility.IsAppRelative(path))
                return url;
            return whatInd >= 0 ? string.Concat(VirtualPathUtility.ToAbsolute(path), url.Substring(whatInd)) : VirtualPathUtility.ToAbsolute(path);
        }

        public DateTime DateOfCreation
        {
            get { return post.DateCreate; }
        }

        /// <summary>
        /// Получить текст содержания для просмотра
        /// (Html-теги вырезаны; закодировано в Html)
        /// </summary>
        /// <param name="length"></param>
        /// <param name="returnFullLastWord">Если true,возвращает preview не обрезая последнего слова</param>
        /// <returns></returns>
        public string GetContentPreview(int length,bool returnFullLastWord)
        {
            string content = post.Post;
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            string result = BXStringUtility.StripOffSimpleTags(processor.StripBBCode(content));
            if (string.IsNullOrEmpty(result))
                return string.Empty;

            int newLength = length;
            if (returnFullLastWord && result.Length>length)
            {
                while (newLength < result.Length && Char.IsLetter(result, newLength) )
                    newLength++;    
            }

            return BXWordBreakingProcessor.Break(newLength <= 0 || result.Length <= newLength ? result : result.Substring(0, newLength), component.MaxWordLength, true);
        }
        /// <summary>
        /// Получить текст содержания для просмотра
        /// (Html-теги вырезаны; закодировано в Html)
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GetContentPreview(int length)
        {
            return GetContentPreview(length, false);
        }

        private string _content = null;
        /// <summary>
        /// Полное содержание
        /// (Закодировано в Html)
        /// </summary>
        public string Content
        {
            get
            {
                if (_content != null)
                    return _content;

                if (!string.IsNullOrEmpty((_content = post.Post)))
                    _content = processor.Process(_content);
                return _content;
            }
        }

        public string TopicLastPostHref
        {
            get
            {
                if (topicLastPostHref != null) return topicLastPostHref;
                BXParamsBag<object> replace = new BXParamsBag<object>();
                if (topic == null) return String.Empty;
                replace.Add("ForumId", forum.Id);
                replace.Add("TopicId", topic.Id);
                replace.Add("PostId", topic.LastPostId);

                return topicLastPostHref = component.ResolveTemplateUrl(component.PostReadUrlTemplate, replace);
            }
        }
    }
    #endregion
}
