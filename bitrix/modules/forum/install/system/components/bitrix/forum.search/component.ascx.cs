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
using Bitrix.Search;

namespace Bitrix.Forum.Components
{
    /// <summary>
    /// Параметры компонента "forum.search"
    /// </summary>
    public enum ForumSearchComponentParameter
    {
        /// <summary>
        /// Форумы (элемент фильтра)
        /// </summary>
        Forums = 1,
        /// <summary>
        /// Поле для сортировки
        /// </summary>
        SortBy,
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
        SetPageTitle,
        /// <summary>
        /// Строка поиска
        /// </summary>
        SearchString,
        /// <summary>
        /// Интервал дат для поиска
        /// </summary>
        DateInterval
    }

    /// <summary>
    /// Ошибки компонента "forum.search"
    /// </summary>
    public enum ForumSearchComponentError
    {
        None = 0,
        General = 1,
        UnauthorizedReadAll = 2,
        PageDoesNotExist = 4,
        DataReadingFailed = 8,
        UserNotFound = 16,
        SearchQueryIsEmpty = 32,
        SearchExpressionIsEmpty = 64
    }

    /// <summary>
    /// Временные интервалы в днях для поиска
    /// </summary>

    public enum ForumSearchComponentDateInterval
    {
        Any = -1,
        Day = 0,
        ThreeDays = 3,
        Week = 7,
        Month = 30
    }

    /// <summary>
    /// Временные интервалы в днях для поиска
    /// </summary>
    public enum ForumSearchComponentSorting
    {
        Relevance,
        Date,
        Topic
    }


	public partial class ForumSearchComponent : BXComponent
	{
		private IList<ForumSearchPostWrapper> posts = (IList<ForumSearchPostWrapper>)new ForumSearchPostWrapper[0];
        private Dictionary<int, BXForumPostChain> processors;
		private BXForumSignatureChain signatureProcessor = new BXForumSignatureChain();

		private bool templateIncluded;
		private Exception fatalException;
		private BXParamsBag<object> replace;
		private int maxWordLength = -1;
		private List<int> availableForums;
        private BXSearchQuery query;
        private BXSearchExpression exp;
       
        ForumSearchComponentError _componentError = ForumSearchComponentError.None;
        public ForumSearchComponentError ComponentError
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
        public static string GetParameterKey(ForumSearchComponentParameter parameter)
        {
            return parameter.ToString();
        }

        /// <summary>
        /// Получить ключ заголовка параметра
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetParameterTitleKey(ForumSearchComponentParameter parameter)
        {
            return string.Concat("Param.", parameter.ToString("G"));
        }

        public string GetComponentErrorMessage(ForumSearchComponentError error)
        {
            return error != ForumSearchComponentError.None ? GetMessageRaw(string.Concat("Error.", error.ToString("G"))) : string.Empty;
        }
        public string[] GetComponentErrorMessages()
        {
            if (_componentError == ForumSearchComponentError.None)
                return new string[0];

            List<string> result = new List<string>();
            if ((_componentError & ForumSearchComponentError.General) != 0)
                result.Add(GetComponentErrorMessage(ForumSearchComponentError.General));
            if ((_componentError & ForumSearchComponentError.UnauthorizedReadAll) != 0)
                result.Add(GetComponentErrorMessage(ForumSearchComponentError.UnauthorizedReadAll));
            if ((_componentError & ForumSearchComponentError.PageDoesNotExist) != 0)
                result.Add(GetComponentErrorMessage(ForumSearchComponentError.PageDoesNotExist));
            if ((_componentError & ForumSearchComponentError.DataReadingFailed) != 0)
                result.Add(GetComponentErrorMessage(ForumSearchComponentError.DataReadingFailed));
            if ((_componentError & ForumSearchComponentError.UserNotFound) != 0)
                result.Add(GetComponentErrorMessage(ForumSearchComponentError.UserNotFound));
            if ((_componentError & ForumSearchComponentError.SearchQueryIsEmpty) != 0)
                result.Add(GetComponentErrorMessage(ForumSearchComponentError.SearchQueryIsEmpty));
            if ((_componentError & ForumSearchComponentError.SearchExpressionIsEmpty) != 0)
                result.Add(GetComponentErrorMessage(ForumSearchComponentError.SearchExpressionIsEmpty));
            return result.ToArray();
        }

        public ForumSearchComponentSorting DefaultSortBy
        {
            get
            {
                return ForumSearchComponentSorting.Relevance;
            }
        }

        public ForumSearchComponentSorting SortBy
        {
            get
            {
                try
                {
                    ForumSearchComponentSorting s =
                        (ForumSearchComponentSorting)Enum.Parse(typeof(ForumSearchComponentSorting), 
                                                                Parameters.GetString(GetParameterKey(ForumSearchComponentParameter.SortBy))
                                                                );
                    return Enum.IsDefined(typeof(ForumSearchComponentSorting), s) ? s : DefaultSortBy;
                }
                catch (ArgumentException)
                {
                    return DefaultSortBy;
                }
            }
            set
            {
                Parameters[GetParameterKey(ForumSearchComponentParameter.SortBy)] = value.ToString();
            }
        }

        public ForumSearchComponentDateInterval DefaultDateInterval
        {
            get
            {
                return ForumSearchComponentDateInterval.Any;
            }
        }

        public ForumSearchComponentDateInterval DateInterval
        {
            get
            {
                try
                {
                    ForumSearchComponentDateInterval i =  (ForumSearchComponentDateInterval)
                        Enum.Parse( typeof(ForumSearchComponentDateInterval), 
                                    Parameters.GetString(GetParameterKey(ForumSearchComponentParameter.DateInterval)));
                    return Enum.IsDefined(typeof(ForumSearchComponentDateInterval), i) ? i : DefaultDateInterval;
                }
                catch (ArgumentException)
                {
                    return DefaultDateInterval;
                }
            }
            set
            {
                Parameters[GetParameterKey(ForumSearchComponentParameter.DateInterval)] = value.ToString();
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
                return Parameters.GetString(GetParameterKey(ForumSearchComponentParameter.AuthorProfileUrlTemplate), DefaultAuthorProfileUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumSearchComponentParameter.AuthorProfileUrlTemplate)] = value;
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
                return Parameters.GetString(GetParameterKey(ForumSearchComponentParameter.ForumReadUrlTemplate), DefaultForumReadUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumSearchComponentParameter.ForumReadUrlTemplate)] = value;
            }
        }


		public List<int> Forums
		{
			get
			{
              return Parameters.GetListInt(GetParameterKey(ForumSearchComponentParameter.Forums));
			}
            set
            {
                if (value == null || value.Count == 0)
                    Parameters[GetParameterKey(ForumSearchComponentParameter.Forums)] = string.Empty;
                else
                {
                    string[] arr = new string[value.Count];
                    for (int i = 0; i < value.Count; i++)
                        arr[i] = value[i].ToString();
                    Parameters[GetParameterKey(ForumSearchComponentParameter.Forums)] = BXStringUtility.ListToString(arr);
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
                return Parameters.GetString(GetParameterKey(ForumSearchComponentParameter.TopicReadUrlTemplate), DefaultTopicReadUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumSearchComponentParameter.TopicReadUrlTemplate)] = value;
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


		public string PostReadUrlTemplate
		{
            get
            {
                return Parameters.GetString(GetParameterKey(ForumSearchComponentParameter.PostReadUrlTemplate), DefaultPostReadUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumSearchComponentParameter.PostReadUrlTemplate)] = value;
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
                return Parameters.GetInt(GetParameterKey(ForumSearchComponentParameter.MaxWordLength), DefaultMaxWordLength);
            }
            set
            {
                Parameters[GetParameterKey(ForumSearchComponentParameter.MaxWordLength)] = value.ToString();
            }
        }

		private IList<ForumSearchPostWrapper> _postListRO = null;
		public IList<ForumSearchPostWrapper> PostList
        {
			get { return _postListRO ?? (_postListRO = new ReadOnlyCollection<ForumSearchPostWrapper>(posts)); }
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

        public String EmptyMessage
        {
            get
            {
                return GetMessage("Message.NoPostsFound");
            }
        }


        /// <summary>
        /// Запрос наличия сообщений
        /// </summary>
        public bool HasPosts
        {
            get { return PostCount > 0; }
        }

        public String SearchString
        {
            get
            {
                return Parameters.GetString(GetParameterKey(ForumSearchComponentParameter.SearchString));
            }
            set
            {
                Parameters[GetParameterTitleKey(ForumSearchComponentParameter.SearchString)] = value;
            }
        }

        public BXSearchExpression SearchExpression
        {
            get
            {
                return exp;
            }
        }

        protected void AddError(ForumSearchComponentError error)
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

        IList<long> GetPostIdList(BXPagingOptions paging)
        {
            List<long> resultList = new List<long>();
            query.PagingOptions = paging;

            foreach (BXSearchResult result in query.Execute())
            {
                string id = result.Content.ItemId;
                long i;
                if ( long.TryParse(id,out i))
                    resultList.Add(i);
            }
            return resultList;
        }

        protected void FillPostWrappers(IList<long> postIdList)
        {
            BXSelectAdd select = new BXSelectAdd();
            select.Add(BXForumPost.Fields.Author);
            select.Add(BXForumPost.Fields.Topic);
            select.Add(BXForumPost.Fields.Forum);
            select.Add(BXForumPost.Fields.Author.User);
            select.Add(BXForumPost.Fields.Author.User.Image);


            BXForumPostCollection postCol = BXForumPost.GetList(
                                                           new BXFilter(new BXFilterItem(
                                                               BXForumPost.Fields.Id,
                                                               BXSqlFilterOperators.In,
                                                               postIdList)),
                                                           null,
                                                           select,
                                                           null
                                                           );

            postCol.Sort(delegate(BXForumPost a, BXForumPost b)
                    {
                        return postIdList.IndexOf(a.Id) - postIdList.IndexOf(b.Id);
                    });


			this.posts = postCol.ConvertAll<ForumSearchPostWrapper>(delegate(BXForumPost input)
            {
				ForumSearchPostWrapper info = new ForumSearchPostWrapper(input, GetProcessor(input.Forum), this);
                info.Author = input.Author;
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

        protected void InitSearchQuery(BXSearchExpression ex)
        {
            query = new BXSearchQuery();
            query.SiteId = DesignerSite;
            BXSearchContentGroupFilter f = new BXSearchContentGroupFilter(BXFilterExpressionCombiningLogic.And);
            //discrete timestep for count caching
            DateTime now = DateTime.Now;
            DateTime nowFloor = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0);
            f.Add(BXSearchContentFilter.IsActive(nowFloor));
            f.Add(new BXSearchContentFilterItem(BXSearchField.ModuleId, BXSqlFilterOperators.Equal, "forum"));
            if (Forums.Count > 0)
                f.Add(new BXSearchContentFilterItem(BXSearchField.ItemGroup, BXSqlFilterOperators.In, ForumsForSelect));

            if (DateInterval != ForumSearchComponentDateInterval.Any)
            {
                DateTime period = DateTime.Now.AddDays(-(int)DateInterval);
                f.Add(new BXSearchContentFilterItem(BXSearchField.ContentDate, BXSqlFilterOperators.Greater, period));
            }

            query.Filter = f;

            query.CalculateRelevance = false;

            query.QueryExpression = ex;

            BXSearchOrderBy orderBy = new BXSearchOrderBy();

            switch (SortBy)
            {
                case ForumSearchComponentSorting.Date:
                    orderBy.AddItem(new BXSearchOrderByPair(BXSearchField.ContentDate, BXOrderByDirection.Desc));
                    break;
                case ForumSearchComponentSorting.Topic:
                    orderBy.AddItem(new BXSearchOrderByPair(BXSearchField.ItemGroup,BXOrderByDirection.Desc));
                    orderBy.AddItem(new BXSearchOrderByPair(BXSearchField.ItemId, BXOrderByDirection.Desc));
                    break;
                case ForumSearchComponentSorting.Relevance:
                    query.CalculateRelevance = true;
                    break;
            }
            query.OrderBy = orderBy;
        }

        protected void ExecuteSearch(BXSearchExpression ex)
        {
            InitSearchQuery(ex);
            int cnt = -1;
            BXQueryParams queryParams = GetQueryParams(cnt);
            if (queryParams != null)
                FillPostWrappers(GetPostIdList(new BXPagingOptions(queryParams.PagingStartIndex, queryParams.PagingRecordCount)));
            
        }

        protected int Count()
        {
            return query.ExecuteCount(true);
        }

        protected BXQueryParams GetQueryParams(int count)
        {
            //Determine page
            BXPagingParams pagingParams = PreparePagingParams();

            bool legal;
            BXQueryParams queryParams = PreparePaging(
               pagingParams,
               delegate
               {
                   return count = Count();
               },
                 replace,
                 out legal
            );
            if (!legal)
            {
                AddError(ForumSearchComponentError.PageDoesNotExist);
                return null;
            }
            return queryParams;
        }


        string[] forumsForSelect=null;

        protected string[] ForumsForSelect
        {
            get
            {
                if (forumsForSelect==null)
                {
                    BXFilter forumsFilter = new BXFilter();
                    forumsFilter.Add(new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true));
                    forumsFilter.Add(new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));
                    forumsFilter.Add(new BXFilterItem(BXForum.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "ForumPublicRead"));

                    if (Forums.Count > 0)
                        forumsFilter.Add(new BXFilterItem(BXForum.Fields.Id, BXSqlFilterOperators.In, Forums.ToArray()));

                    BXForumCollection forums = BXForum.GetList(forumsFilter, null, new BXSelect(BXForum.Fields.Id), null);
                    return forumsForSelect = forums.ConvertAll<string>(delegate(BXForum input)
                            {
                                return input.Id.ToString();
                            }).ToArray();
                }
                return forumsForSelect;
            }
        }

        string pageTitle;

        protected String PageTitle
        {
            get
            {
                return pageTitle;
            }
            set
            {
                pageTitle = value;
            }
        }

		protected void Page_Load(object sender, EventArgs e)
		{
				CacheMode = BXCacheMode.None;
				replace = new BXParamsBag<object>();
                processors = new Dictionary<int, BXForumPostChain>();

                BXPublicPage bitrixPage = Page as BXPublicPage;
                PageTitle = GetMessage("Page.PageTitle");
                if (bitrixPage != null && !IsComponentDesignMode)
                {
                     if (Parameters.GetBool("SetPageTitle", true))
                     {

                         if (!string.IsNullOrEmpty(PageTitle))
                         {
                             bitrixPage.MasterTitleHtml = BXWordBreakingProcessor.Break(PageTitle, MaxWordLength, true);
                             bitrixPage.Title = PageTitle;
                         }
                     }
                }
                if (SearchString == String.Empty) AddError(ForumSearchComponentError.SearchQueryIsEmpty);
                else
                {
                    exp = new BXSearchExpression(BXSite.Current.LanguageId, SearchString);
                    if (!exp.IsEmpty)
                        ExecuteSearch(exp);
                    else
                    {
                        AddError(ForumSearchComponentError.SearchExpressionIsEmpty);
                    }
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
			Group = new BXComponentGroup("forum", GetMessageRaw("Group"), 115, BXComponentGroup.Communication);

			
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
             GetParameterKey(ForumSearchComponentParameter.SearchString),
             new BXParamText(
                 GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.SearchString)),
                 "",
                 mainCategory
                )
            );

            List<BXParamValue> sortParams = new List<BXParamValue>();

            foreach (string name in Enum.GetNames(typeof(ForumSearchComponentDateInterval)))
                sortParams.Add(new BXParamValue(GetMessage("Param.DateInterval." + name), ((int)Enum.Parse(typeof(ForumSearchComponentDateInterval), name)).ToString()));

            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.SortBy),
                new BXParamSingleSelection(
                GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.SortBy)),
                "ByRelevance",
                mainCategory,
                sortParams
                )
            );

            List<BXParamValue> dateParams = new List<BXParamValue>();

            foreach (string name in Enum.GetNames(typeof(ForumSearchComponentDateInterval)))
                dateParams.Add(new BXParamValue(GetMessage("Param.DateInterval." + name), ((int)Enum.Parse(typeof(ForumSearchComponentDateInterval), name)).ToString()));

            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.DateInterval),
                new BXParamSingleSelection(
                    GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.DateInterval)),
                    "forums",
                    mainCategory,
                    dateParams
                    )
                );

            string clientSideActionGroupViewId = ClientID;

            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.Forums),
                new BXParamMultiSelection(
                GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.Forums)),
                string.Empty,
                mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.SetPageTitle),
                new BXParamYesNo(
                GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.SetPageTitle)),
                true,
                mainCategory
                 )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.ThemeCssFilePath),
                new BXParamText(GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.ThemeCssFilePath)),
                                "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css", 
                                mainCategory
                                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.ColorCssFilePath),
                new BXParamText(GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.ColorCssFilePath)),
                    "~/bitrix/components/bitrix/forum/templates/.default/style.css",
                    mainCategory
                    )
            );



            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.ForumReadUrlTemplate),
                new BXParamText(
                GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.ForumReadUrlTemplate)),
                DefaultForumReadUrlTemplate,
                mainCategory
                )
            );

            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.TopicReadUrlTemplate),
                new BXParamText(
                    GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.TopicReadUrlTemplate)),
                    DefaultTopicReadUrlTemplate,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.PostReadUrlTemplate),
                new BXParamText(
                    GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.PostReadUrlTemplate)),
                    DefaultPostReadUrlTemplate,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumSearchComponentParameter.MaxWordLength),
                new BXParamText(
                    GetMessageRaw(GetParameterTitleKey(ForumSearchComponentParameter.MaxWordLength)),
                    DefaultMaxWordLength.ToString(),
                    mainCategory
                    )
                );

 

          
        }

        private IList<BXParamValue> BuildUpParameterValueList(ForumSearchComponentParameter parameter, Type parameterValueEnumType, IList<BXParamValue> parameterValueList)
        {
            string key = GetParameterKey(parameter);
            if (parameterValueList == null)
            {
                parameterValueList = ParamsDefinition[key].Values;
                if (parameterValueList.Count > 0)
                    parameterValueList.Clear();
            }
            foreach (string s in Enum.GetNames(parameterValueEnumType))
                parameterValueList.Add(new BXParamValue(GetMessageRaw(string.Concat(GetParameterTitleKey(parameter), ".", s)), s));
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
            ParamsDefinition[GetParameterKey(ForumSearchComponentParameter.Forums)].Values = forums.ConvertAll<BXParamValue>(delegate(BXForum input)
			{
				return new BXParamValue(input.Name, input.Id.ToString());
			});

            List<BXParamValue> sortByValues = ParamsDefinition[GetParameterKey(ForumSearchComponentParameter.SortBy)].Values;
            if (sortByValues.Count > 0)
                sortByValues.Clear();
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.Relevance"), "ID"));
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.Date"), "Date"));
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.Topic"), "Topic"));

            ParamsDefinition[GetParameterKey(ForumSearchComponentParameter.SortBy)].Values = sortByValues;
		}

		private string MakeHref(string template, BXParamsBag<object> parameters)
		{
			return Encode(ResolveTemplateUrl(template, parameters));
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

	public class ForumSearchTemplate : BXComponentTemplate<ForumSearchComponent>
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
    public class ForumSearchPostWrapper
    {
        internal BXForum forum;
        internal BXForumTopic topic;
        internal BXForumSignatureChain signatureChain;
        private ForumSearchComponent component;
        private BXForumPost post;
        private BXForumUser author;
        private string authorNameHtml;
        private string contentHtml;
        private string signatureHtml;
        private string topicTitleHtml;
        private string userProfileHref;
        private string thisPostHref;
        private string thisTopicHref;
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
        private StringBuilder replace = null;

        public ForumSearchPostWrapper() { }

		public ForumSearchPostWrapper(BXForumPost post, BXForumPostChain processor, ForumSearchComponent component)
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
                return _forumReadUrl ?? (_forumReadUrl = HttpUtility.HtmlAttributeEncode(ResolveUrl(Replace(component.ForumReadUrlTemplate, "#FORUMID#", post.Forum != null ? post.Forum.Id.ToString() : string.Empty))));
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
                if (_topicReadUrl != null) return _postReadUrl;
                BXParamsBag<object> replace = new BXParamsBag<object>();
                replace.Add("ForumId", forum.Id);
                replace.Add("TopicId", topic.Id);
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
                if (_postReadUrl != null) return _postReadUrl;
                BXParamsBag<object> replace = new BXParamsBag<object>();
                replace.Add("ForumId", forum.Id);
                replace.Add("TopicId", topic.Id);
                replace.Add("PostId", post.Id);
                return _postReadUrl = component.ResolveTemplateUrl(component.PostReadUrlTemplate, replace);
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
                if (_authorProfileUrl != null) return _authorProfileUrl;
                BXParamsBag<object> replace = new BXParamsBag<object>();
                if (author == null) return String.Empty;
                replace.Add("UserId", author.Id);
                return _authorProfileUrl = component.ResolveTemplateUrl(component.AuthorProfileUrlTemplate, replace);
            }
        }


        private string Replace(string template, string oldVal, string newVal)
        {
            if (string.IsNullOrEmpty(template) || string.IsNullOrEmpty(oldVal))
                return string.Empty;

            if (newVal == null)
                newVal = string.Empty;

            if (replace == null)
                replace = new StringBuilder();
            if (replace.Length > 0)
                replace.Length = 0;

            int curIndex = 0,
                paramInd = -1;
            while (curIndex < template.Length - 1 && (paramInd = template.IndexOf(oldVal, curIndex, StringComparison.InvariantCultureIgnoreCase)) >= 0)
            {
                replace.Append(template.Substring(curIndex, paramInd - curIndex));
                replace.Append(newVal);
                curIndex = paramInd + oldVal.Length;
            }
            replace.Append(template.Substring(curIndex));
            return replace.ToString();
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
        public string GetContentPreview(int length, bool returnFullLastWord)
        {
            string content = post.Post;
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            string result = BXStringUtility.StripOffSimpleTags(processor.StripBBCode(content));
            if (string.IsNullOrEmpty(result))
                return string.Empty;

            int newLength = length;
            if (returnFullLastWord && result.Length > length)
            {
                while (newLength < result.Length && Char.IsLetter(result, newLength))
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
