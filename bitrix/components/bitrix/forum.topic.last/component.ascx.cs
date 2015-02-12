using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.DataLayer;
using System.Collections.ObjectModel;
using Bitrix.Services.Text;
using Bitrix.DataTypes;
using System.Text;
using Bitrix.Security;
using Bitrix.CommunicationUtility;

namespace Bitrix.Forum.Components
{
    /// <summary>
    /// Параметры компонента "forum.topic.last"
    /// </summary>
    public enum ForumTopicLastComponentParameter 
    {
        /// <summary>
        /// Обрабатываемые форумы
        /// </summary>
        Forums = 1,
        /// <summary>
        /// Сортировать по полю темы
        /// </summary>
        SortBy,
        /// <summary>
        /// Направление сортировки
        /// </summary>
        SortDirection,
        /// <summary>
        /// Шаблон URL для чтения темы
        /// </summary>
        TopicReadUrlTemplate,
        /// <summary>
        /// Шаблон URL для чтения форума
        /// </summary>
        ForumReadUrlTemplate,
        /// <summary>
        /// Шаблон URL для профиля автора
        /// </summary>
        AuthorProfileUrlTemplate,
        /// <summary>
        /// Максимальная длина слова
        /// </summary>
        MaxWordLength
    }

    public enum ForumTopicLastComponentError
    {
        None = 0,
        General = 1,
        UnauthorizedReadAll = 2,
        PageDoesNotExist = 4,
        DataReadingFailed = 8
    }

    public partial class ForumTopicLastComponent : BXComponent
    {
        /// <summary>
        /// Получить ключ параметра
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetParameterKey(ForumTopicLastComponentParameter parameter)
        {
            return parameter.ToString();
        }

        /// <summary>
        /// Получить ключ заголовка параметра
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetParameterTilteKey(ForumTopicLastComponentParameter parameter)
        {
            return string.Concat("Param.", parameter.ToString("G"));
        }

        public IList<int> Forums
        {
            get 
            {
                List<int> result = Parameters.GetListInt(GetParameterKey(ForumTopicLastComponentParameter.Forums));
                return result != null ? new ReadOnlyCollection<int>(result) : new ReadOnlyCollection<int>(new int[0]);
            }
            set 
            {
                if (value == null || value.Count == 0)
                    Parameters[GetParameterKey(ForumTopicLastComponentParameter.Forums)] = string.Empty;
                else
                {
                    string[] arr = new string[value.Count];
                    for (int i = 0; i < value.Count; i++)
                        arr[i] = value[i].ToString();
                    Parameters[GetParameterKey(ForumTopicLastComponentParameter.Forums)] = BXStringUtility.ListToString(arr);
                }
            }
        }

        public string DefaultSortBy
        {
            get { return "ID"; }
        }

        public string SortBy
        {
            get
            {
                return Parameters.GetString(GetParameterKey(ForumTopicLastComponentParameter.SortBy), DefaultSortBy);
            }
            set
            {
                Parameters[GetParameterKey(ForumTopicLastComponentParameter.SortBy)] = value;
            }
        }

        public BXOrderByDirection SortDirection
        {
            get
            {
                string r = Parameters.GetString(GetParameterKey(ForumTopicLastComponentParameter.SortDirection));
                if (string.IsNullOrEmpty(r))
                    return BXOrderByDirection.Desc;

                BXOrderByDirection result = BXOrderByDirection.Desc;
                try
                {
                    result = (BXOrderByDirection)Enum.Parse(typeof(BXOrderByDirection), r);
                }
                catch (Exception /*exc*/)
                {
                }
                return result;
            }
            set
            {
                Parameters[GetParameterKey(ForumTopicLastComponentParameter.SortDirection)] = value.ToString("G");
            }
        }

        public string DefaultForumReadUrlTemplate
        {
            get { return "forum.aspx?forum=#ForumId#"; }
        }

        public string ForumReadUrlTemplate
        {
            get
            {
                return Parameters.GetString(GetParameterKey(ForumTopicLastComponentParameter.ForumReadUrlTemplate), DefaultForumReadUrlTemplate);
            }
            set 
            {
                Parameters[GetParameterKey(ForumTopicLastComponentParameter.ForumReadUrlTemplate)] = value;
            }
        }

        public string DefaultTopicReadUrlTemplate
        {
            get { return "forum.aspx?forum=#ForumId#&topic=#TopicId#"; }
        }

        public string TopicReadUrlTemplate
        {
            get
            {
                return Parameters.GetString(GetParameterKey(ForumTopicLastComponentParameter.TopicReadUrlTemplate), DefaultTopicReadUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumTopicLastComponentParameter.TopicReadUrlTemplate)] = value;
            }
        }

        public string DefaultAuthorProfileUrlTemplate
        {
            get { return "forum.aspx?user=#AuthorId#"; }
        }

        /// <summary>
        /// Шаблон страницы профиля автора сообщения
        /// </summary>
        public string AuthorProfileUrlTemplate
        {
            get
            {
                return Parameters.GetString(GetParameterKey(ForumTopicLastComponentParameter.AuthorProfileUrlTemplate), DefaultAuthorProfileUrlTemplate);
            }
            set
            {
                Parameters[GetParameterKey(ForumTopicLastComponentParameter.AuthorProfileUrlTemplate)] = value;
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
                return Parameters.GetInt(GetParameterKey(ForumTopicLastComponentParameter.MaxWordLength), DefaultMaxWordLength);
            }
            set
            {
                Parameters[GetParameterKey(ForumTopicLastComponentParameter.MaxWordLength)] = value.ToString();
            }
        }

        private int? _userId = null;
        public int UserId
        {
            get { return (_userId ?? (_userId = ((BXIdentity)BXPrincipal.Current.Identity).Id)).Value; }
        }

        private ForumTopicLastComponentError _componentError = ForumTopicLastComponentError.None;
        public ForumTopicLastComponentError ComponentError
        {
            get { return _componentError; }
            protected set { _componentError = value; }
        }

        public string GetComponentErrorMessage(ForumTopicLastComponentError error)
        {
            return error != ForumTopicLastComponentError.None ? GetMessageRaw(string.Concat("Error.", error.ToString("G"))) : string.Empty;
        }
        public string[] GetComponentErrorMessages()
        {
            if (_componentError == ForumTopicLastComponentError.None)
                return new string[0];

            List<string> result = new List<string>();
            if ((_componentError & ForumTopicLastComponentError.General) != 0)
                result.Add(GetComponentErrorMessage(ForumTopicLastComponentError.General));
            if ((_componentError & ForumTopicLastComponentError.UnauthorizedReadAll) != 0)
                result.Add(GetComponentErrorMessage(ForumTopicLastComponentError.UnauthorizedReadAll));
            if ((_componentError & ForumTopicLastComponentError.PageDoesNotExist) != 0)
                result.Add(GetComponentErrorMessage(ForumTopicLastComponentError.PageDoesNotExist));
            if ((_componentError & ForumTopicLastComponentError.DataReadingFailed) != 0)
                result.Add(GetComponentErrorMessage(ForumTopicLastComponentError.DataReadingFailed));

            return result.ToArray();
        }

        private BXForumPostChain _processor = null;
        protected BXForumPostChain InternalProcessor
        {
            get
            {
                if (_processor != null)
                    return _processor;

                _processor = new BXForumPostChain();
                _processor.MaxWordLength = MaxWordLength;
                return _processor;
            }
        }

        private BXParamsBag<object> _replaceParams = null;
        public BXParamsBag<object> ReplaceParams
        {
            get { return _replaceParams != null ? _replaceParams : (_replaceParams = new BXParamsBag<object>()); }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            BXPagingParams pagingParams = PreparePagingParams();
			if (!IsCached(pagingParams, ((BXPrincipal)Page.User).GetAllRoles(true)))
            {
                BXFilter f = new BXFilter(
                    new BXFilterItem(BXForumTopic.Fields.Forum.Active, BXSqlFilterOperators.Equal, true),
                    new BXFilterItem(BXForumTopic.Fields.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite),
                    new BXFilterItem(BXForumTopic.Fields.Approved, BXSqlFilterOperators.Equal, true),
                    new BXFilterItem(BXForumTopic.Fields.Forum.CheckPermissions, BXSqlFilterOperators.Equal, true)
                    );
                IList<int> allowedForumIds = Forums;
                if (allowedForumIds.Count > 0)
                    f.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.In, allowedForumIds));

                BXOrderBy o = new BXOrderBy();
                string sortBy = SortBy;
                o.Add(BXForumTopic.Fields.GetFieldByKey(string.IsNullOrEmpty(sortBy) ? sortBy : DefaultSortBy), SortDirection);

                BXParamsBag<object> replaceItems = ReplaceParams;
                bool isPageLegal = true;
                BXQueryParams q = PreparePaging(
                    pagingParams,
                    delegate() { return BXForumTopic.Count(f); },
                    replaceItems,
                    out isPageLegal
                );


                if (!isPageLegal)
                {
                    _componentError |= ForumTopicLastComponentError.PageDoesNotExist;
                    IncludeComponentTemplate();
                    return;
                }

                BXForumTopicCollection c = null;
                try
                {
                    c = BXForumTopic.GetList(
                        f,
                        o,
                        new BXSelectAdd(
                            BXForumTopic.Fields.Forum,
                            BXForumTopic.Fields.Author
                            ),
                        q,
                        BXTextEncoder.HtmlTextEncoder
                        );
                }
                catch (Exception /*exc*/)
                {
                    _componentError |= ForumTopicLastComponentError.DataReadingFailed;
                }

                if (c != null)
                    for (int i = 0; i < c.Count; i++)
                        InternalTopicList.Add(new ForumTopicWrapper(c[i], InternalProcessor, this));

                IncludeComponentTemplate();
            }
        }

        private IList<BXParamValue> BuildUpParameterValueList(ForumTopicLastComponentParameter parameter, Type parameterValueEnumType, IList<BXParamValue> parameterValueList)
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

        private IList<ForumTopicWrapper> _topicListRO = null;
        /// <summary>
        /// Список тем
        /// </summary>
        public IList<ForumTopicWrapper> TopicList
        {
            get { return _topicListRO ?? (_topicListRO = new ReadOnlyCollection<ForumTopicWrapper>(InternalTopicList)); }
        }

        private List<ForumTopicWrapper> _topicList = null;
        protected IList<ForumTopicWrapper> InternalTopicList
        {
            get { return _topicList ?? (_topicList = new List<ForumTopicWrapper>()); }
        }

        #region BXComponent
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";
            Group = new BXComponentGroup("forum", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);
            BXCategory mainCategory = BXCategory.Main;

            ParamsDefinition.Add(
                GetParameterKey(ForumTopicLastComponentParameter.Forums), 
                new BXParamMultiSelection(
                    GetMessageRaw(GetParameterTilteKey(ForumTopicLastComponentParameter.Forums)),
                    string.Empty,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumTopicLastComponentParameter.SortBy),
                new BXParamSingleSelection(
                    GetMessageRaw(GetParameterTilteKey(ForumTopicLastComponentParameter.SortBy)),
                    DefaultSortBy,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumTopicLastComponentParameter.SortDirection),
                new BXParamSingleSelection(
                    GetMessageRaw(GetParameterTilteKey(ForumTopicLastComponentParameter.SortDirection)),
                    BXOrderByDirection.Desc.ToString("G"),
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumTopicLastComponentParameter.TopicReadUrlTemplate),
                new BXParamText(
                    GetMessageRaw(GetParameterTilteKey(ForumTopicLastComponentParameter.TopicReadUrlTemplate)),
                    DefaultTopicReadUrlTemplate,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumTopicLastComponentParameter.ForumReadUrlTemplate),
                new BXParamText(
                    GetMessageRaw(GetParameterTilteKey(ForumTopicLastComponentParameter.ForumReadUrlTemplate)),
                    DefaultForumReadUrlTemplate,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumTopicLastComponentParameter.AuthorProfileUrlTemplate),
                new BXParamText(
                    GetMessageRaw(GetParameterTilteKey(ForumTopicLastComponentParameter.AuthorProfileUrlTemplate)),
                    DefaultAuthorProfileUrlTemplate,
                    mainCategory
                    )
                );
            ParamsDefinition.Add(
                GetParameterKey(ForumTopicLastComponentParameter.MaxWordLength),
                new BXParamText(
                    GetMessageRaw(GetParameterTilteKey(ForumTopicLastComponentParameter.MaxWordLength)),
                    DefaultMaxWordLength.ToString(),
                    mainCategory
                    )
                );

            ParamsDefinition.Add(BXParametersDefinition.Cache);
            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
        }

        protected override void LoadComponentDefinition()
        {

            IList<BXParamValue> forumValues = ParamsDefinition[GetParameterKey(ForumTopicLastComponentParameter.Forums)].Values;
            if (forumValues.Count > 0)
                forumValues.Clear();
            BXForumCollection forums = BXForum.GetList(
                new BXFilter(
                    new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true),
                    new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
                    ),
                new BXOrderBy(new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)),
                new BXSelect(
                    BXSelectFieldPreparationMode.Normal,
                    BXForum.Fields.Id,
                    BXForum.Fields.Name
                    ),
                null,
                BXTextEncoder.EmptyTextEncoder
                );

            foreach (BXForum f in forums)
                forumValues.Add(new BXParamValue(f.Name, f.Id.ToString()));

            IList<BXParamValue> sortByValues = ParamsDefinition[GetParameterKey(ForumTopicLastComponentParameter.SortBy)].Values;
            if (sortByValues.Count > 0)
                sortByValues.Clear();
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.ID"), "ID"));
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.LastPostId"), "LastPostId"));
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.Name"), "Name"));

            BuildUpParameterValueList(ForumTopicLastComponentParameter.SortDirection, typeof(BXOrderByDirection), null);

            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
        }
        #endregion
    }

    public class ForumTopicWrapper
    {
        private BXForumTopic _topic = null;
        private BXForumPostChain _processor = null;
        private ForumTopicLastComponent _component = null;
        public ForumTopicWrapper(BXForumTopic topic, BXForumPostChain processor, ForumTopicLastComponent component)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");
            _topic = topic;

            if(processor == null)
                throw new ArgumentNullException("processor");
            _processor = processor;

            if (component == null)
                throw new ArgumentNullException("component");
            _component = component;
        }

        public BXForumTopic Topic
        {
            get { return _topic; }
        }


        private string BreakWord(string source, int maxLength, bool encode)
        {
            return !string.IsNullOrEmpty(source) ? BXWordBreakingProcessor.Break(source, maxLength, encode) : string.Empty;
        }

        private string _forumName = null;
        /// <summary>
        /// Имя форума (Закодировано в Html)
        /// </summary>
        public string ForumName
        {
            get
            {
                return _forumName ?? (_forumName = _topic.Forum != null ? BreakWord(_topic.Forum.Name, _component.MaxWordLength, false) : string.Empty);
            }
        }

        private string _name = null;
        /// <summary>
        /// Имя темы (Закодировано в Html)
        /// </summary>
        public string Name
        {
            get
            {
                return _name ?? (_name = BreakWord(_topic.Name, _component.MaxWordLength, false));
            }
        }

        private string _authorName = null;
        /// <summary>
        /// Имя автора (Закодировано в Html)
        /// </summary>
        public string AuthorName
        {
            get
            {
                return _authorName ?? (_authorName = BreakWord(_topic.AuthorName, _component.MaxWordLength, false));
            }
        }

        private string _lastPostAuthorName = null;
        /// <summary>
        /// Имя автора последнего сообщения (Закодировано в Html)
        /// </summary>
        public string LastPostAuthorName
        {
            get
            {
                return _lastPostAuthorName ?? (_lastPostAuthorName = BreakWord(_topic.LastPosterName, _component.MaxWordLength, false));
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

        private StringBuilder _replace = null;
        private string Replace(string template, string oldVal, string newVal)
        {
            if (string.IsNullOrEmpty(template) || string.IsNullOrEmpty(oldVal))
                return string.Empty;

            if (newVal == null)
                newVal = string.Empty;

            if (_replace == null)
                _replace = new StringBuilder();
            if (_replace.Length > 0)
                _replace.Length = 0;

            int curIndex = 0,
                paramInd = -1;
            while (curIndex < template.Length - 1 && (paramInd = template.IndexOf(oldVal, curIndex, StringComparison.InvariantCultureIgnoreCase)) >= 0)
            {
                _replace.Append(template.Substring(curIndex, paramInd - curIndex));
                _replace.Append(newVal);
                curIndex = paramInd + oldVal.Length;
            }
            _replace.Append(template.Substring(curIndex));
            return _replace.ToString();
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
                return _forumReadUrl ?? (_forumReadUrl = HttpUtility.HtmlAttributeEncode(ResolveUrl(Replace(_component.ForumReadUrlTemplate, "#FORUMID#", _topic.Forum != null ? _topic.Forum.Id.ToString() : string.Empty))));
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
                return _topicReadUrl ?? (_topicReadUrl = HttpUtility.HtmlAttributeEncode(ResolveUrl(Replace(Replace(_component.TopicReadUrlTemplate, "#FORUMID#", _topic.Forum != null ? _topic.Forum.Id.ToString() : string.Empty), "#TOPICID#", _topic.Id.ToString()))));
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
				if (_authorProfileUrl != null)
					return _authorProfileUrl;

				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace["AuthorId"] = _topic.AuthorId;
				replace["UserId"] = _topic.AuthorId;

				_authorProfileUrl = _component.ResolveTemplateUrl(_component.AuthorProfileUrlTemplate, replace);
                return _authorProfileUrl;
            }
        }
    }

    public class ForumTopicLastTemplate : BXComponentTemplate<ForumTopicLastComponent>
    {
        protected string[] GetErrorMessages()
        {
            string[] result = Component.GetComponentErrorMessages();
            if (result.Length > 0)
                for (int i = 0; i < result.Length; i++)
                    result[i] = HttpUtility.HtmlEncode(result[i]);

            return result;
        }
    }
}
