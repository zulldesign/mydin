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
using Bitrix.Services;

namespace Bitrix.Blog.Components
{
    public partial class BlogCategoryListComponent : BXComponent
    {
        private static readonly string ConstantThemeCssFilePath = "~/bitrix/components/bitrix/blog/templates/.default/style.css";
        private static readonly string ConstantColorCssFilePath = "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css";

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

        public string SortByFirst
        {
            get { return Parameters.Get("SortByFirst", "Name"); }
        }

        public string SortOrderFirst
        {
            get { return Parameters.Get("SortOrderFirst", "Asc"); }
        }

        public string SortBySecond
        {
            get { return Parameters.Get("SortBySecond", "Sort"); }
        }

        public string SortOrderSecond
        {
            get { return Parameters.Get("SortOrderSecond", "Asc"); }
        }

        public string BlogCategoryPageUrlTemplate
        {
            get { return Parameters.Get("BlogCategoryPageUrlTemplate"); }
        }

        public int DisplayLimit
        {
            get 
            { 
                int result = Parameters.Get<int>("DisplayLimit", 0);
                if (result < 0)
                    result = 0;
                return result;
            }
        }

        public int ColumnCount
        {
            get 
            { 
                int result = Parameters.Get<int>("ColumnCount", 2);
                if (result <= 0)
                    result = 2;
                return result;
            }
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
                additionalSettingsCategory = BXCategory.AdditionalSettings,
                display = new BXCategory(GetMessage("Category.DisplaySettings"), "DisplaySettings", mainCategory.Sort + 1); ;

            ParamsDefinition.Add(
                "ThemeCssFilePath",
                new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), ConstantThemeCssFilePath, mainCategory)
                );

            ParamsDefinition.Add(
                "ColorCssFilePath",
                new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), ConstantColorCssFilePath, mainCategory)
                );

            ParamsDefinition.Add(
                "SortByFirst",
                new BXParamSingleSelection(
                    GetMessageRaw("Param.SortByFirst"), "Name", dataSource)
                );

            ParamsDefinition.Add(
                "SortOrderFirst",
                new BXParamSort(GetMessageRaw("Param.SortOrderFirst"), false, dataSource)
                );

            ParamsDefinition.Add(
                "SortBySecond",
                new BXParamSingleSelection(GetMessageRaw("Param.SortBySecond"), "Sort", dataSource)
                );

            ParamsDefinition.Add(
                "SortOrderSecond",
                new BXParamSort(GetMessageRaw("Param.SortOrderSecond"), false, dataSource)
                );

            ParamsDefinition.Add(
                "BlogCategoryPageUrlTemplate",
                new BXParamText(GetMessageRaw("Param.BlogCategoryPageUrlTemplate"), string.Empty, sef)
                );

            ParamsDefinition.Add(
                "DisplayLimit",
                new BXParamText(GetMessageRaw("Param.DisplayLimit"), "0", display)
                );

            ParamsDefinition.Add(
                "ColumnCount",
                new BXParamText(GetMessageRaw("Param.ColumnCount"), "2", display)
                );

            ParamsDefinition.Add(
                "MaxWordLength",
                new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory)
                );
            //Cache
            ParamsDefinition.Add(BXParametersDefinition.Cache);
        }

        protected override void LoadComponentDefinition()
        {
            //SortByFirst, SortBySecond
            List<BXParamValue> sortFields = new List<BXParamValue>();
            sortFields.Add(new BXParamValue("ID", "ID"));
            sortFields.Add(new BXParamValue(GetMessageRaw("FiledCaption.Sort"), "Sort"));
            sortFields.Add(new BXParamValue(GetMessageRaw("FiledCaption.Name"), "Name"));

            ParamsDefinition["SortByFirst"].Values = sortFields;
            ParamsDefinition["SortBySecond"].Values = sortFields;
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
			base.OnLoad(e);

			if(IsCached())
				return;

			using (BXTagCachingScope scope = BeginTagCaching())
			{
				scope.AddTag(BXBlogCategory.CreateSpecialTag(BXCacheSpecialTagType.All));

				BXOrderBy order = new BXOrderBy();

				string sortBy1 = SortByFirst;
				if (!string.IsNullOrEmpty(sortBy1))
					order.Add(BXBlogCategory.Fields, sortBy1, SortOrderFirst);

				string sortBy2 = SortBySecond;
				if (!string.IsNullOrEmpty(sortBy2) && !string.Equals(sortBy1, sortBy2))
					order.Add(BXBlogCategory.Fields, sortBy2, SortOrderSecond);

				BXBlogCategoryCollection col = BXBlogCategory.GetList(
					new BXFilter(new BXFilterItem(BXBlogCategory.Fields.Sites.Site.ID, BXSqlFilterOperators.Equal, DesignerSite)), 
					order, 
					null, 
					null, 
					BXTextEncoder.HtmlTextEncoder);

				int count = col != null ? col.Count : 0;
				int displayLimit = DisplayLimit;
				for (int i = 0; i < count; i++)
				{
					if (displayLimit > 0 && displayLimit <= i)
						break;

					ListItem item = new ListItem(col[i], this);
					AddItem(item);
				}

				IncludeComponentTemplate();
			}
        }

        protected override void OnPreRender(EventArgs e)
        {
            BXPage.RegisterStyle(ThemeCssFilePath);
            BXPage.RegisterStyle(ColorCssFilePath);

            base.OnPreRender(e);
        }

        public class ListItem
        {
            private BXBlogCategory _blogCategory;
            private BlogCategoryListComponent _component;

            public ListItem(BXBlogCategory blogCategory, BlogCategoryListComponent component)
            {
                if (blogCategory == null)
                    throw new ArgumentNullException("blogCatogory");

                if (component == null)
                    throw new ArgumentNullException("component");

                _blogCategory = blogCategory;
                _component = component;
            }

            private string _blogCategoryName = null;
            public string CategoryName
            {
                get
                {
                    return _blogCategoryName != null ? _blogCategoryName : (_blogCategoryName = BXWordBreakingProcessor.Break(_blogCategory.TextEncoder.Decode(_blogCategory.Name), _component.MaxWordLength, true));
                }
            }

            private string _blogCategoryUrl = string.Empty;
            private bool _IsBlogUrlCategoryBuilt = false;
            public string BlogCategoryUrl
            {
                get
                {
                    if (_IsBlogUrlCategoryBuilt)
                        return _blogCategoryUrl;


                    int blogCategoryId = _blogCategory.Id;
                    if (blogCategoryId <= 0)
                        _blogCategoryUrl = string.Empty;
                    else
                    {
                        string template = _component.BlogCategoryPageUrlTemplate;
                        if (string.IsNullOrEmpty(template))
                        {
                            string url = BXSefUrlManager.CurrentUrl.PathAndQuery;
                            int whatInd = url.IndexOf('?');
                            string path = whatInd >= 0 ? url.Substring(0, whatInd) : url;
                            BXQueryString qs = new BXQueryString(whatInd >= 0 ? url.Substring(whatInd) : string.Empty);
                            qs.Add("CategoryId", blogCategoryId.ToString());
                            _blogCategoryUrl = string.Concat(path, qs.ToString());
                        }
                        else
                        {
                            BXParamsBag<object> replaceItems = new BXParamsBag<object>();
                            replaceItems.Add("CategoryId", blogCategoryId);
                            replaceItems.Add("CATEGORY_ID", blogCategoryId);
                            replaceItems.Add("BlogCategory", blogCategoryId);
                            replaceItems.Add("BLOG_CATEGORY", blogCategoryId);
                            replaceItems.Add("BlogCategoryId", blogCategoryId);
                            replaceItems.Add("BLOG_CATEGORY_ID", blogCategoryId);
                            _blogCategoryUrl = BXSefUrlUtility.MakeLink(template, replaceItems);

                            int whatInd = _blogCategoryUrl.IndexOf('?');
                            string path = whatInd >= 0 ? _blogCategoryUrl.Substring(0, whatInd) : _blogCategoryUrl;
                            if (!string.IsNullOrEmpty(path) && VirtualPathUtility.IsAppRelative(path))
                            {
                                path = VirtualPathUtility.ToAbsolute(path);
                                if (whatInd >= 0)
                                    _blogCategoryUrl = string.Concat(path, _blogCategoryUrl.Substring(whatInd));
                                else
                                    _blogCategoryUrl = path;
                            }
                        }
                    }

                    _IsBlogUrlCategoryBuilt = true;
                    return _blogCategoryUrl;
                }
            }
        }
    }


    public class BlogCategoryListTemplate : BXComponentTemplate<BlogCategoryListComponent>
    {
        protected override void OnLoad(EventArgs e)
        {
            PrepareColums();
            base.OnLoad(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            StartWidth = "100%";
            //if (IsComponentDesignMode)
            //{
            //}
            base.Render(writer);
        }

        protected void PrepareColums()
        {
            IList<ColumnData> columns = Columns;
            if(columns.Count > 0)
                columns.Clear();

            IList<BlogCategoryListComponent.ListItem> items = Component.Items;

            int itemCount = items.Count;
            if (itemCount == 0)
                return;

            int columnCount = Component.ColumnCount,
                itemsPerColumn = itemCount / columnCount,
                extraItemCount = itemCount % columnCount;

            if (itemsPerColumn == 0)
            {
                columnCount = itemCount;
                itemsPerColumn = 1;
                extraItemCount = 0;
            }

            ColumnData curColumn = null;
            int curColumnIndex = -1;
            for (int i = 0; i < itemCount; i++)
            {
                if (curColumn != null && curColumn.Items.Count >= (curColumnIndex > 0 ? itemsPerColumn : itemsPerColumn + extraItemCount))
                    curColumn = null;

                if (curColumn == null)
                {
                    curColumn = new ColumnData();
                    columns.Add(curColumn);
                    curColumnIndex++;
                }

                curColumn.Items.Add(items[i]);
            }
        }

        private List<ColumnData> _columns;
        public IList<ColumnData> Columns
        {
            get { return _columns != null ? _columns : (_columns = new List<ColumnData>()); }
        }

        public int GetMaxColumnItemsCount()
        {
            int result = 0;
            IList<ColumnData> columns = Columns;
            foreach (ColumnData column in columns)
            {
                int curCount = column.Items.Count;
                if(result < curCount)
                    result = curCount;
            }
            return result;
        }

        public class ColumnData
        {
            private List<BlogCategoryListComponent.ListItem> _items;
            public IList<BlogCategoryListComponent.ListItem> Items
            {
                get { return _items != null ? _items : (_items = new List<BlogCategoryListComponent.ListItem>()); }
            }
        }
    }
}