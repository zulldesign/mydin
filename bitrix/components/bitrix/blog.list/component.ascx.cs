using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix.Components;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bitrix.DataLayer;
using Bitrix.Security;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.CommunicationUtility;
using System.Text;
using Bitrix.Services;

namespace Bitrix.Blog.Components
{
    public partial class BlogListComponent : BXComponent
    {
        [Flags]
        public enum Error
        {
            /// <summary>
            /// Без ошибок
            /// </summary>
            None = 0,
            /// <summary>
            /// Общая ошибка
            /// </summary>
            General = 1,
            /// <summary>
            /// Некорректная страница
            /// </summary>
            PageDoesNotExist = 2,
            /// <summary>
            /// Ошибка при получении данных
            /// </summary>
            DataGetting = 4,
            /// <summary>
            /// Ошибка при установке заголовка страницы
            /// </summary>
            PageTitleSetting = 8,
			/// <summary>
			/// Отсутствие прав доступа
			/// </summary>
			PermissonDenied = 16
        }

        public string ThemeCssFilePath
        {
            get 
            { 
                string result = Parameters.Get("ThemeCssFilePath", ConstantThemeCssFilePath);
                return !string.IsNullOrEmpty(result) ? result.TrimStart(' ', '\t') : string.Empty; 
            }
        }

        public string ColorCssFilePath
        {
            get
            {
                string result = Parameters.Get("ColorCssFilePath", ConstantColorCssFilePath);
                return !string.IsNullOrEmpty(result) ? result.TrimStart(' ', '\t') : string.Empty;
            }
        }

        public int CategoryId
        {
            get { return Parameters.Get<int>("CategoryId", 0); }
        }

        BXBlogCategory _category = null;
        private bool _isCategoryProcessed = false;
        public BXBlogCategory Category
        {
            get 
            {
                if (_isCategoryProcessed)
                    return _category;

                int categoryId = CategoryId;
                _category = categoryId > 0 ? BXBlogCategory.GetById(categoryId) : null;
                _isCategoryProcessed = true;
                return _category;
            }
        }

        private string _sortByFirst = null;
        public string SortByFirst
        {
            get { return _sortByFirst ?? (_sortByFirst = Parameters.Get("SortByFirst", "DateCreated")); }
        }

        private string _sortOrderFirst = null;
        public string SortOrderFirst
        {
            get { return _sortOrderFirst ?? (_sortOrderFirst = Parameters.Get("SortOrderFirst", "Desc")); }
        }

        private string _sortBySecond = null;
        public string SortBySecond
        {
            get { return _sortBySecond ?? (_sortBySecond = Parameters.Get("SortBySecond", "Id")); }
        }

        string _sortOrderSecond = null;
        public string SortOrderSecond
        {
            get { return _sortOrderSecond ?? (_sortOrderSecond = Parameters.Get("SortOrderSecond", "Desc")); }
        }

        public string BlogPageUrlTemplate
        {
            get { return Parameters.Get("BlogPageUrlTemplate"); }
        }

        public string BlogOwnerProfilePageUrlTemplate
        {
            get { return Parameters.Get("BlogOwnerProfilePageUrlTemplate"); }
        }

        public int MaxWordLength
        {
            get
            {
                int result = Parameters.Get<int>("MaxWordLength", 15);
                if (result < 0)
                    result = 100;
                return result;
            }
        }

        public bool AboutPageTitleSetup
        {
            get { return Parameters.GetBool("SetPageTitle", true); }
        }

        private BXParamsBag<object> _replaceParams = null;
        public BXParamsBag<object> ReplaceParams
        {
            get { return _replaceParams != null ? _replaceParams : (_replaceParams = new BXParamsBag<object>()); }
        }

        private static readonly string ConstantThemeCssFilePath = "~/bitrix/components/bitrix/blog/templates/.default/style.css";
        private static readonly string ConstantColorCssFilePath = "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css";

        #region BXComponent
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";
            Group = BXBlogModuleConfiguration.GetComponentGroup();

            BXCategory mainCategory = BXCategory.Main,
                dataSource = BXCategory.DataSource,
                sef = BXCategory.Sef,
                additionalSettingsCategory = BXCategory.AdditionalSettings;

            ParamsDefinition.Add(
                "ThemeCssFilePath",
                new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), ConstantThemeCssFilePath, mainCategory)
                );

            ParamsDefinition.Add(
                "ColorCssFilePath",
                new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), ConstantColorCssFilePath, mainCategory)
                );

            ParamsDefinition.Add(
                "CategoryId",
                new BXParamSingleSelection(GetMessageRaw("Param.Category"), string.Empty, dataSource)
                );

            ParamsDefinition.Add(
                "SortByFirst",
                new BXParamSingleSelection(
                    GetMessageRaw("Param.SortByFirst"), "ActiveFromDate", dataSource)
                );

            ParamsDefinition.Add(
                "SortOrderFirst",
                new BXParamSort(GetMessageRaw("Param.SortOrderFirst"), dataSource)
                );

            ParamsDefinition.Add(
                "SortBySecond",
                new BXParamSingleSelection(GetMessageRaw("Param.SortBySecond"), "Sort", dataSource)
                );


            ParamsDefinition.Add(
                "SortOrderSecond",
                new BXParamSort(GetMessageRaw("Param.SortOrderSecond"), dataSource)
                );

            ParamsDefinition.Add(
                "BlogPageUrlTemplate",
                new BXParamText(GetMessageRaw("Param.BlogPageUrlTemplate"), string.Empty, sef)
                );

            ParamsDefinition.Add(
                "BlogOwnerProfilePageUrlTemplate",
                new BXParamText(GetMessageRaw("Param.BlogOwnerProfilePageUrlTemplate"), string.Empty, sef)
                );

            ParamsDefinition.Add(
                "MaxWordLength",
                new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory)
                );

            ParamsDefinition.Add(
                "SetPageTitle",
                new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory)
                );

            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

            //Cache
            ParamsDefinition.Add(BXParametersDefinition.Cache);
        }

        protected override void LoadComponentDefinition()
        {
            //CategoryId
            List<BXParamValue> categoryParamValues = new List<BXParamValue>();
            categoryParamValues.Add(new BXParamValue(GetMessageRaw("NotSelectedFeminine"), string.Empty));
            BXBlogCategoryCollection categories = BXBlogCategory.GetList(
                null, 
                new BXOrderBy(new BXOrderByPair(BXBlogCategory.Fields.Name, BXOrderByDirection.Asc)),
                null,
                null,
                BXTextEncoder.EmptyTextEncoder
                );
            foreach (BXBlogCategory category in categories)
                categoryParamValues.Add(new BXParamValue(category.Name, category.Id.ToString()));
            ParamsDefinition["CategoryId"].Values = categoryParamValues;

            //SortByFirst, SortBySecond
            List<BXParamValue> sortFields = new List<BXParamValue>();
            sortFields.Add(new BXParamValue("ID", "ID"));
            sortFields.Add(new BXParamValue(GetMessageRaw("FiledCaption.Sort"), "Sort"));
            sortFields.Add(new BXParamValue(GetMessageRaw("FiledCaption.Name"), "Name"));
            sortFields.Add(new BXParamValue(GetMessageRaw("FiledCaption.DateCreated"), "DateCreated"));
            sortFields.Add(new BXParamValue(GetMessageRaw("FiledCaption.DateLastPosted"), "DateLastPosted"));
            sortFields.Add(new BXParamValue(GetMessageRaw("FiledCaption.PostCount"), "PostCount"));

            foreach (BXCustomField customField in InnerCustomFields)
                sortFields.Add(
                    new BXParamValue(
                        BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel), 
                        string.Concat("#", customField.Name.ToUpper())
                        )
                );

            ParamsDefinition["SortByFirst"].Values = sortFields;
            ParamsDefinition["SortBySecond"].Values = sortFields;

            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.BlogPublicView))
			{
                _errors |= Error.PermissonDenied;
				return;
			}

			var bp = BXBlogUserPermissions.GetBlogsForPosts(BXIdentity.Current.Id, BXBlogPostPermissionLevel.Read, true, Page.Items, null);
			if(IsCached(InternalPagingParams, bp.Hash))
			{
				SetTemplateCachedData();
				return;
			}

			try
			{
				using (BXTagCachingScope scope = BeginTagCaching())
				{
					scope.AddTag(BXBlog.CreateSpecialTag(BXCacheSpecialTagType.All));
					
					bool hasBlogs = bp.Exclude || bp.Ids.Count > 0;
								
					BXFilter filter = new BXFilter(
						new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true),
						new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.Site.ID, BXSqlFilterOperators.Equal, DesignerSite)
					);

					if (hasBlogs && (!bp.Exclude || bp.Ids.Count > 0))
					{
						IBXFilterItem f = new BXFilterItem(BXBlog.Fields.Id, BXSqlFilterOperators.In, bp.Ids);
						if (bp.Exclude)
							f = new BXFilterNot(f);
						filter.Add(f);
					}

					int categoryId = CategoryId;
					string categoryIdParamValue = categoryId > 0 ? categoryId.ToString() : string.Empty;
					if (categoryId > 0)
						filter.Add(new BXFilterItem(BXBlog.Fields.Categories.Category.Id, BXSqlFilterOperators.Equal, categoryId));

					if (String.Equals("PostCount", SortByFirst, StringComparison.OrdinalIgnoreCase))
						filter.Add(new BXFilterItem(BXBlog.Fields.PostCount, BXSqlFilterOperators.Greater, 0));

					BXParamsBag<object> replaceItems = ReplaceParams;
					replaceItems.Add("CategoryId", categoryIdParamValue);
					replaceItems.Add("BlogCategory", categoryIdParamValue);
					replaceItems.Add("BlogCategoryId", categoryIdParamValue);

					bool isPageLegal = true;
					BXQueryParams queryParams = PreparePaging(
						InternalPagingParams,
						delegate()
						{
							return hasBlogs ? BXBlog.Count(filter) : 0;
						},
						replaceItems,
						out isPageLegal
					);


					if (!isPageLegal)
					{
						_errors |= Error.PageDoesNotExist;
						AbortCache();
						return;
					}

					BXOrderBy order = new BXOrderBy();

					if (!string.IsNullOrEmpty(SortByFirst))
						AddFieldToOrderBy(order, SortByFirst, SortOrderFirst);

					if (!string.IsNullOrEmpty(SortBySecond) && !string.Equals(SortByFirst, SortBySecond, StringComparison.Ordinal))
						AddFieldToOrderBy(order, SortBySecond, SortOrderSecond);

					BXBlogCollection blogCol =
						hasBlogs
						? BXBlog.GetList(
							filter,
							order,
							new BXSelectAdd(BXBlog.Fields.Owner, BXBlog.Fields.Owner.User, BXBlog.Fields.Owner.User.Image),
							queryParams,
							BXTextEncoder.HtmlTextEncoder
						)
						: new BXBlogCollection();

					foreach (BXBlog blog in blogCol)
					{
						ListItem item = new ListItem(blog, this);
						AddItem(item);
					}
				}
			}
			catch (Exception exc)
			{
				_exception = exc;
				_errors |= Error.DataGetting;
				AbortCache();
			}
			
            IncludeComponentTemplate();
            SetTemplateCachedData();
        }

        protected override void OnPreRender(EventArgs e)
        {
            BXPage.RegisterStyle(ThemeCssFilePath);
            BXPage.RegisterStyle(ColorCssFilePath);

            base.OnPreRender(e);
        }

        private BXCustomFieldCollection _customFields = null;
        private BXCustomFieldCollection InnerCustomFields 
        {
            get { return _customFields ?? (_customFields = BXCustomEntityManager.GetFields(BXBlog.GetCustomFieldsKey())); }
        }

        private BXPublicPage _publicPage = null;
        private bool _isPublicPageProcessed = false;
        protected BXPublicPage PublicPage
        {
            get 
            {
                if (_isPublicPageProcessed)
                    return _publicPage;

                _isPublicPageProcessed = true;
                return (_publicPage = Page as BXPublicPage);
            }
        }


        private BXPagingParams? _pagingParams = null;
        protected BXPagingParams InternalPagingParams
        {
            get { return (_pagingParams ?? (_pagingParams = PreparePagingParams())).Value; }
        }

        private void AddFieldToOrderBy(BXOrderBy order, string name, string direction)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (string.IsNullOrEmpty(name))
                return;

            if (string.IsNullOrEmpty(direction))
                direction = "ASC";

            BXSchemeFieldBase f;
            if (name[0] != '#')
                order.Add(BXBlog.Fields, name, direction);
            else if ((f = BXBlog.Fields.CustomFields.GetField(name.Substring(1))) != null)
                order.Add(f, direction);  
        }

        private void SetTemplateCachedData()
        {
            //Set page title
            if (!IsComponentDesignMode && PublicPage != null && AboutPageTitleSetup)
            {
                string title = Category != null ? (!string.IsNullOrEmpty(Category.Name) ? Category.TextEncoder.Decode(Category.Name) : string.Format(GetMessageRaw("CategoryWithID"), Category.Id.ToString())) : GetMessageRaw("AllItems");

                PublicPage.MasterTitleHtml = BXWordBreakingProcessor.Break(title, MaxWordLength, true);
                PublicPage.Title = Encode(title);
            } 
        }

        private Error _errors = Error.None;
        public Error ComponentErrors
        {
            get { return _errors; }
        }
        private Exception _exception;

        public string GetErrorMessageHtml()
        {
            if (_errors == Error.None)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

			if ((_errors & Error.PermissonDenied) == Error.PermissonDenied)
				sb.Append(GetMessage("Error.PermissonDenied"));
            else if ((_errors & Error.PageDoesNotExist) == Error.PageDoesNotExist)
                sb.Append(GetMessage("Error.PageDoesNotExist"));
            else if ((_errors & Error.DataGetting) == Error.DataGetting)
                sb.Append(GetMessage("Error.DataGetting"));
            else if ((_errors & Error.PageTitleSetting) == Error.PageTitleSetting)
                sb.Append(GetMessage("Error.PageTitleSetting"));
            else //if ((_errors & Error.General) == Error.General)
                sb.Append(GetMessage("Error.General"));

            if (_exception != null && BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance))
                sb.Append("<br/>").Append(GetMessage("CaptionForErrorDetails")).Append("<br/><pre>").Append(Encode(_exception.ToString())).Append("</pre>");


            return sb.ToString();
        }

        protected List<ListItem> _items;
        protected List<ListItem> InternalItems
        {
            get { return _items != null ? _items : (_items = new List<ListItem>()); }
        }

        protected void AddItem(ListItem item) 
        {
            if (item == null)
                return;
            InternalItems.Add(item);
            ComponentCache["ItemCount"] = InternalItems.Count;
        }
        /// <summary>
        /// Элементы
        /// </summary>
        public IList<ListItem> Items
        {
            get { return new ReadOnlyCollection<ListItem>(InternalItems); }
        }

        /// <summary>
        /// Запрос количества элеметов
        /// </summary>
        public int ItemCount
        {
            get 
            {
                object val;
                return ComponentCache.TryGetValue("ItemCount", out val) ? (int)val : InternalItems.Count; 
            }
        }

        /// <summary>
        /// Запрос наличия элементов
        /// </summary>
        public bool HasItems
        {
            get { return ItemCount > 0; }
        }

        public class ListItem
        {

            private BXBlog _blog;
            private BlogListComponent _component;

            public ListItem(BXBlog blog, BlogListComponent component) 
            {
                if (blog == null)
                    throw new ArgumentNullException("blog");

                if (component == null)
                    throw new ArgumentNullException("component");

                _blog = blog;
                _component = component;
            }

            private string _ownerDisplayName = null;
            public string OwnerDisplayName
            {
                get 
                {
                    if (_ownerDisplayName != null)
                        return _ownerDisplayName;
                    BXBlogUser owner = _blog.Owner;
                    BXUser ownerUser = owner != null ? owner.User : null;
                    if (ownerUser == null)
                        _ownerDisplayName = _component.GetMessage("UnknownUser");
                    else
                    {
                        
                        _ownerDisplayName = ownerUser.DisplayName != null ? ownerUser.DisplayName.Trim() : string.Empty;
                        if (_ownerDisplayName.Length == 0)
                            _ownerDisplayName = string.Concat(ownerUser.FirstName, " ", ownerUser.LastName).Trim();
                        if (_ownerDisplayName.Length == 0)
                            _ownerDisplayName = ownerUser.UserName; //гарантировано непустое значение

                        _ownerDisplayName = BXWordBreakingProcessor.Break(ownerUser.TextEncoder.Decode(_ownerDisplayName), _component.MaxWordLength, true);
                    }
                    return _ownerDisplayName;
                }
            }

            private string _blogName = null;
            public string BlogName
            {
                get 
                {
                    return _blogName != null ? _blogName : (_blogName = BXWordBreakingProcessor.Break(_blog.TextEncoder.Decode(_blog.Name), _component.MaxWordLength, true)); 
                }
            }

            private string _blogDescription = null;
            public string BlogDescription
            {
                get 
                {
                    return _blogDescription != null ? _blogDescription : (_blogDescription = GetBlogDescription(0)); 
                }
            }

            public string GetBlogDescription(int maxLengthInChar)
            {
                if (maxLengthInChar < 0)
                    maxLengthInChar = 0;

                _blogDescription = _blog.Description;
                if (string.IsNullOrEmpty(_blogDescription))
                    return string.Empty;

                if (maxLengthInChar > 0 && _blogDescription.Length > maxLengthInChar)
                    _blogDescription = string.Concat(_blogDescription.Substring(0, maxLengthInChar), "...");

                _blogDescription = BXWordBreakingProcessor.Break(_blog.TextEncoder.Decode(_blogDescription), _component.MaxWordLength, true);
                if (_blogDescription == null)
                    _blogDescription = string.Empty;

                return _blogDescription;
            }

            private string _blogOwnerProfileUrl = string.Empty;
            private bool _IsBlogOwnerProfileUrlBuilt = false;
            public string BlogOwnerProfileUrl
            {
                get 
                {
                    if (_IsBlogOwnerProfileUrlBuilt)
                        return _blogOwnerProfileUrl;

                    int ownerId = _blog.OwnerId;
                    if (ownerId <= 0)
                        _blogOwnerProfileUrl = string.Empty;
                    else
                    {
                        string template = _component.BlogOwnerProfilePageUrlTemplate;
                        if (string.IsNullOrEmpty(template))
                        {
                            string url = BXSefUrlManager.CurrentUrl.PathAndQuery;
                            int whatInd = url.IndexOf('?');
                            string path = whatInd >= 0 ? url.Substring(0, whatInd) : url;
                            BXQueryString qs = new BXQueryString(whatInd >= 0 ? url.Substring(whatInd) : string.Empty);
                            qs.Add("UserId", ownerId.ToString());
                            _blogOwnerProfileUrl = string.Concat(path, qs.ToString());
                        }
                        else
                        {
                            BXParamsBag<object> replaceItems = _component.ReplaceParams;
                            replaceItems.Add("UserId", ownerId);
                            _blogOwnerProfileUrl = _component.MakeLink(template, replaceItems);

                            int whatInd = _blogOwnerProfileUrl.IndexOf('?');
                            string path = whatInd >= 0 ? _blogOwnerProfileUrl.Substring(0, whatInd) : _blogOwnerProfileUrl;
                            if (!string.IsNullOrEmpty(path) && VirtualPathUtility.IsAppRelative(path))
                            {
                                path = VirtualPathUtility.ToAbsolute(path);
                                if (whatInd >= 0)
                                    _blogOwnerProfileUrl = string.Concat(path, _blogOwnerProfileUrl.Substring(whatInd));
                                else
                                    _blogOwnerProfileUrl = path;
                            }
                        }
                    }
                    _IsBlogOwnerProfileUrlBuilt = true;
                    return _blogOwnerProfileUrl; 
                }
            }

            private string _blogUrl = string.Empty;
            private bool _IsBlogUrlBuilt = false;
            public string BlogUrl
            {
                get 
                {
                    if (_IsBlogUrlBuilt)
                        return _blogUrl;

                    string blogSlug = _blog.Slug;
                    string categoryId = _component.CategoryId > 0 ? _component.CategoryId.ToString() : string.Empty;
                    if (string.IsNullOrEmpty(blogSlug))
                        _blogUrl = string.Empty;
                    else
                    {
                        string template = _component.BlogPageUrlTemplate;
                        if (string.IsNullOrEmpty(template))
                        {
							string url = BXSefUrlManager.CurrentUrl.PathAndQuery;
                            int whatInd = url.IndexOf('?');
                            string path = whatInd >= 0 ? url.Substring(0, whatInd) : url;
                            BXQueryString qs = new BXQueryString(whatInd >= 0 ? url.Substring(whatInd) : string.Empty);
                            qs.Add("BlogSlug", blogSlug);
                            if (!string.IsNullOrEmpty(categoryId))
                                qs.Add("CategoryId", categoryId);
                            _blogUrl = string.Concat(path, qs.ToString());
                        }
                        else
                        {
                            BXParamsBag<object> replaceItems = _component.ReplaceParams;
                            replaceItems.Add("BlogSlug", blogSlug);
                            replaceItems.Add("Blog", blogSlug);

                            _blogUrl = _component.MakeLink(template, replaceItems);

                            int whatInd = _blogUrl.IndexOf('?');
                            string path = whatInd >= 0 ? _blogUrl.Substring(0, whatInd) : _blogUrl;
                            if (!string.IsNullOrEmpty(path) && VirtualPathUtility.IsAppRelative(path))
                            {
                                path = VirtualPathUtility.ToAbsolute(path);
                                if (whatInd >= 0)
                                    _blogUrl = string.Concat(path, _blogUrl.Substring(whatInd));
                                else
                                    _blogUrl = path;
                            }
                        }
                    }
                    _IsBlogUrlBuilt = true;
                    return _blogUrl;
                }
            }

			public BXBlog Blog
			{
				get { return _blog; }
			}
        }
    }



    public class BlogListTemplate : BXComponentTemplate<BlogListComponent>
    {
        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);
        //}

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((Component.ComponentErrors & BlogListComponent.Error.PageDoesNotExist) == BlogListComponent.Error.PageDoesNotExist)
                BXError404Manager.Set404Status(Response);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            StartWidth = "100%";
            base.Render(writer);
        }
    }
}
