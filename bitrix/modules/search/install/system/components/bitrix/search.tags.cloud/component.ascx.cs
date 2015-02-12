using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.Search;
using Bitrix.UI;
using Bitrix.DataTypes;
using Bitrix.Components.Editor;

namespace Bitrix.Search.Components
{
    public partial class SearchTagsCloudComponent : BXComponent
    {
        private List<string> filter;
        private List<TagInfo> tags;
        private BXSearchQuery contentQuery;

        public List<string> Filter
        {
            get
            {
                return filter ?? (filter = Parameters.GetListString("Filter"));
            }
        }
        public BXContentTagField SelectionSort
        {
            get
            {
                return Parameters.GetEnum("SelectionSort", BXContentTagField.TagCount);
            }
            set
            {
                Parameters["SelectionSort"] = value.ToString();
            }
        }
        public BXOrderByDirection SelectionOrder
        {
            get
            {
                return Parameters.GetEnum("SelectionOrder", BXOrderByDirection.Asc);
            }
            set
            {
                Parameters["SelectionOrder"] = value.ToString();
            }
        }
        public ModerationMode Moderation
        {
            get
            {
                return Parameters.GetEnum("Moderation", ModerationMode.All);
            }
            set
            {
                Parameters["Moderation"] = value.ToString();
            }
        }
        public BXContentTagField DisplaySort
        {
            get
            {
                return Parameters.GetEnum("DisplaySort", BXContentTagField.TagCount);
            }
            set
            {
                Parameters["DisplaySort"] = value.ToString();
            }
        }
        public BXOrderByDirection DisplayOrder
        {
            get
            {
                return Parameters.GetEnum("DisplayOrder", BXOrderByDirection.Asc);
            }
            set
            {
                Parameters["DisplayOrder"] = value.ToString();
            }
        }

        public SizeInterpolationMode SizeDistribution
        {
            get
            {
                return Parameters.GetEnum("SizeDistribution", SizeInterpolationMode.None);
            }
            set
            {
                Parameters["SizeDistribution"] = value.ToString();
            }
        }
        public int SizeMin
        {
            get
            {
                return Math.Max(Parameters.GetInt("SizeMin"), 0);
            }
            set
            {
                Parameters["SizeMin"] = value.ToString();
            }
        }
        public int SizeMax
        {
            get
            {
                return Math.Max(Parameters.GetInt("SizeMax"), 0);
            }
            set
            {
                Parameters["SizeMax"] = value.ToString();
            }
        }
        public TimeSpan SizePeriod
        {
            get
            {
                return TimeSpan.FromDays(Math.Max(Parameters.GetDouble("SizePeriod"), 0.0));
            }
            set
            {
                Parameters["SizePeriod"] = value.TotalDays.ToString();
            }
        }

        public ColorInterpolationMode ColorDistribution
        {
            get
            {
                return Parameters.GetEnum("ColorDistribution", ColorInterpolationMode.None);
            }
            set
            {
                Parameters["ColorDistribution"] = value.ToString();
            }
        }
        public Color ColorMin
        {
            get
            {
                try
                {
                    return ColorTranslator.FromHtml(Parameters.GetString("ColorMin"));
                }
                catch
                {
                    return Color.Empty;
                }
            }
            set
            {
                try
                {
                    Parameters["ColorMin"] = ColorTranslator.ToHtml(value);
                }
                catch
                {
                    Parameters["ColorMin"] = "";
                }
            }
        }
        public Color ColorMax
        {
            get
            {
                try
                {
                    return ColorTranslator.FromHtml(Parameters.GetString("ColorMax"));
                }
                catch
                {
                    return Color.Empty;
                }
            }
            set
            {
                try
                {
                    Parameters["ColorMin"] = ColorTranslator.ToHtml(value);
                }
                catch
                {
                    Parameters["ColorMin"] = "";
                }
            }
        }

        public string TagLinkTemplate
        {
            get { return Parameters.GetString("TagLinkTemplate"); }
            set { Parameters["TagLinkTemplate"] = value; }
        }


        public BXSearchQuery ContentQuery
        {
            get
            {
                return contentQuery ?? (contentQuery = BuildContentQuery());
            }
            set
            {
                contentQuery = value;
            }
        }

        private BXSearchQuery BuildContentQuery()
        {
            BXSearchQuery searchQuery = new BXSearchQuery();
            searchQuery.CalculateRelevance = false;
            searchQuery.CheckPermissions = true;

            BXSearchContentGroupFilter contentFilter = new BXSearchContentGroupFilter(BXFilterExpressionCombiningLogic.And);
            contentFilter.Add(BXSearchContentFilter.IsActive(DateTime.Now));
			BXSearchContentGroupFilter customFilters = new BXSearchContentGroupFilter(BXFilterExpressionCombiningLogic.Or);
			contentFilter.Add(customFilters);

            foreach (string filterId in Filter)
            {
                BXSearchContentFilter f = BXSearchContentFilter.TryGetById(filterId);
                if (f == null)
                    continue;

                customFilters.Add(f);
            }
            searchQuery.Filter = contentFilter;
            return searchQuery;
        }

        public List<TagInfo> Tags
        {
            get
            {
                return tags;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsCached(null))
                return;

            BXParamsBag<object> replace = new BXParamsBag<object>();
            #region Build Query
            BXContentTagQuery query = new BXContentTagQuery();
            query.SelectTagCount = true;
            query.SelectLastUpdate = true;

            query.OrderBy = new BXContentTagOrderBy(SelectionSort, SelectionOrder);
            BXContentTagGroupFilter tagFilter = new BXContentTagGroupFilter(BXFilterExpressionCombiningLogic.And);
            tagFilter.Add(new BXContentTagFilterItem(BXContentTagField.TagCount, BXSqlFilterOperators.NotEqual, 0));
            query.Filter = tagFilter;

            switch (Moderation)
            {
                case ModerationMode.NotRejected:
                    tagFilter.Add(new BXContentTagFilterItem(BXContentTagField.Status, BXSqlFilterOperators.NotEqual, BXContentTagStatus.Rejected));
                    break;
                case ModerationMode.Approved:
                    tagFilter.Add(new BXContentTagFilterItem(BXContentTagField.Status, BXSqlFilterOperators.Equal, BXContentTagStatus.Approved));
                    break;
            }

            query.SearchQuery = ContentQuery;

            BXPagingParams pagingParams = PreparePagingParams();

            bool legal;
            BXQueryParams queryParams = PreparePaging(
                pagingParams,
                delegate()
                {
                    return query.ExecuteCount();
                },
                replace,
                out legal
            );

            if (!legal)
            {
                AbortCache();
                tags = new List<TagInfo>();
                IncludeComponentTemplate();
                return;
            }

            if (queryParams != null && queryParams.AllowPaging)
                query.PagingOptions = new BXPagingOptions(queryParams.PagingStartIndex, queryParams.PagingRecordCount);

            #endregion

            BXContentTagCollection results = query.Execute();

            #region Display Sort
            BXContentTagField displaySort = DisplaySort;
            BXOrderByDirection displayOrder = DisplayOrder;
            if (displaySort == SelectionSort)
            {
                if (displayOrder != SelectionOrder)
                    results.Reverse();
            }
            else
            {
                results.Sort(delegate(BXContentTag a, BXContentTag b)
                {
                    int result;
                    switch (displaySort)
                    {
                        case BXContentTagField.LastUpdate:
                            result = DateTime.Compare(a.LastUpdate, b.LastUpdate);
                            break;
                        case BXContentTagField.Name:
                            result = string.Compare(a.TextEncoder.Decode(a.Name), b.TextEncoder.Decode(b.Name));
                            break;
                        case BXContentTagField.Status:
                            result = (int)a.Status - (int)b.Status;
                            break;
                        case BXContentTagField.TagCount:
                            result = a.TagCount - b.TagCount;
                            break;
                        default:
                            result = a.Id - b.Id;
                            break;
                    }

                    return displayOrder == BXOrderByDirection.Asc ? result : -result;
                });
            }
            #endregion

            TimeSpan period = SizePeriod;
            DateTime now = DateTime.Now;

            int fontMin = SizeMin;
            int fontMax = Math.Max(fontMin, SizeMax);
            SizeInterpolationMode sizeMode = (fontMax > fontMin) ? SizeDistribution : SizeInterpolationMode.None;

            Color colorOld = ColorMin;
            Color colorNew = ColorMax;
            ColorInterpolationMode colorMode = (colorOld != Color.Empty && colorNew != Color.Empty && colorOld != colorNew) ? ColorDistribution : ColorInterpolationMode.None;

            tags = results.ConvertAll<TagInfo>(delegate(BXContentTag tag)
            {
                TagInfo result = new TagInfo();
                result.Tag = tag;
                result.modifiedCount = tag.TagCount;
                result.FontSize = fontMin;
                result.Color = colorOld;

                replace["TagId"] = tag.Id;
                replace["SearchTags"] = replace["TagName"] = tag.TextEncoder.Decode(tag.Name);
                replace["TagCount"] = tag.TagCount;

                string template = Parameters.GetString("TagLinkTemplate");
                result.Url = !string.IsNullOrEmpty(template) ? ResolveTemplateUrl(template, replace) : "";

                return result;
            });

            #region Calculate Font Size
            if (sizeMode != SizeInterpolationMode.None)
            {
                int countAll = 0;
                int countMin = 0;
                int countMax = 0;

                for (int i = 0; i < tags.Count; i++)
                {
                    BXContentTag tag = tags[i].Tag;
                    countAll += tag.TagCount;
                    if (i == 0)
                    {
                        countMin = tag.TagCount;
                        countMax = tag.TagCount;
                    }
                    else
                    {
                        countMin = Math.Min(countMin, tag.TagCount);
                        countMax = Math.Max(countMax, tag.TagCount);
                    }
                }

                if (countAll != 0 && fontMin > 0 && fontMax > fontMin)
                {
                    double fontRange = fontMax - fontMin;

                    if (period > TimeSpan.Zero && countMin != countMax)
                    {
                        foreach (TagInfo tag in tags)
                        {
                            if (now - tag.Tag.LastUpdate <= period)
                                tag.modifiedCount += (countMax - tag.Tag.TagCount) * (period - (now - tag.Tag.LastUpdate)).TotalSeconds / period.TotalSeconds;
                        }
                    }

                    if (sizeMode == SizeInterpolationMode.Linear)
                    {
                        if (countMin != countMax)
                        {
                            foreach (TagInfo tag in tags)
                            {
                                tag.FontSize = Math.Min(
                                    fontMax,
                                    fontMin + (int)(Math.Pow((tag.modifiedCount - countMin) / Math.Max(1, countMax - countMin), 0.8) * fontRange)
                                );
                            }
                        }
                        else
                        {
                            foreach (TagInfo tag in tags)
                                tag.FontSize = fontMin + (int)(fontRange * 0.5f);
                        }

                    }
                    else if (sizeMode == SizeInterpolationMode.Exponential)
                    {
                        double modCountMin = 0.0;
                        double modCountMax = 0.0;

                        for (int i = 0; i < tags.Count; i++)
                        {
                            TagInfo tag = tags[i];
                            if (i == 0)
                            {
                                modCountMin = tag.modifiedCount;
                                modCountMax = tag.modifiedCount;
                            }
                            else
                            {
                                modCountMin = Math.Min(tag.modifiedCount, modCountMin);
                                modCountMax = Math.Max(tag.modifiedCount, modCountMax);
                            }
                        }


                        double countRange = modCountMax - modCountMin;
                        if (fontRange > 0.0 && countRange > double.Epsilon)
                        {
                            double powBase = Math.Pow(fontRange + 1.0, 1.0 / countRange);
                            foreach (TagInfo tag in tags)
                                tag.FontSize += (int)Math.Round(Math.Pow(powBase, tag.modifiedCount - modCountMin) - 1.0);
                        }
                    }
                }
            }
            #endregion

            #region Calculate Color
            if (colorMode != ColorInterpolationMode.None)
            {

                DateTime dateMax = DateTime.MinValue;
                DateTime dateMin = DateTime.MinValue;

                for (int i = 0; i < tags.Count; i++)
                {
                    BXContentTag tag = tags[i].Tag;
                    DateTime date = tag.LastUpdate;
                    if (date != DateTime.MinValue)
                    {
                        if (dateMax == DateTime.MinValue)
                            dateMax = date;
                        else if (tag.LastUpdate > dateMax)
                            dateMax = tag.LastUpdate;

                        if (dateMin == DateTime.MinValue)
                            dateMin = date;
                        else if (tag.LastUpdate < dateMin)
                            dateMin = tag.LastUpdate;
                    }
                }

                Dictionary<TagInfo, int> logColors = null;
                double logFactor = 1.0;

                if (colorMode == ColorInterpolationMode.Logarithmic)
                {
                    TagInfo[] tagArray = tags.ToArray();
                    Array.Sort(tagArray, delegate(TagInfo a, TagInfo b)
                    {
                        return a.Tag.LastUpdate.CompareTo(b.Tag.LastUpdate);
                    });
                    logColors = new Dictionary<TagInfo, int>();
                    for (int i = 0; i < tagArray.Length; i++)
                        logColors[tagArray[i]] = i;
                    logFactor = 1.0 / tagArray.Length;
                }

                if (colorNew != Color.Empty && colorOld != Color.Empty)
                {
                    foreach (TagInfo tag in tags)
                    {
                        double scale = 0.0;
                        switch (colorMode)
                        {
                            case ColorInterpolationMode.Linear:
                                double interval = (dateMax - dateMin).TotalSeconds;
                                if ((int)interval > 0)
                                    scale = (tag.Tag.LastUpdate - dateMin).TotalSeconds / interval;
                                break;
                            case ColorInterpolationMode.Logarithmic:
                                scale = logColors[tag] * logFactor;
                                break;
                        }
                        Color color =
                            (scale != 0.0)
                            ? Color.FromArgb
                            (
                                Math.Max(0, Math.Min(255, (int)(colorOld.R + scale * ((int)colorNew.R - colorOld.R)))),
                                Math.Max(0, Math.Min(255, (int)(colorOld.G + scale * ((int)colorNew.G - colorOld.G)))),
                                Math.Max(0, Math.Min(255, (int)(colorOld.B + scale * ((int)colorNew.B - colorOld.B))))
                            )
                            : colorOld;

                        tag.Color = color;
                    }
                }
            }
            #endregion

            IncludeComponentTemplate();
        }
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Group = BXSearchModule.GetComponentGroup();
            Description = GetMessageRaw("Description");

            BXCategory mainCategory = BXCategory.Main;
            BXCategory urlCategory = BXCategory.UrlSettings;
            BXCategory sizeCategory = new BXCategory(GetMessageRaw("Category.Size"), "size", 100);
            BXCategory colorCategory = new BXCategory(GetMessageRaw("Category.Color"), "color", 100);

            ParamsDefinition["Filter"] = new BXParamMultiSelection(GetMessageRaw("Param.Filter"), "", mainCategory);
            ParamsDefinition["SelectionSort"] = new BXParamSingleSelection(GetMessageRaw("Param.SelectionSort"), BXContentTagField.TagCount.ToString(), mainCategory);
            ParamsDefinition["SelectionOrder"] = new BXParamSort(GetMessageRaw("Param.SelectionOrder"), true, mainCategory);
            ParamsDefinition["Moderation"] = new BXParamSingleSelection(GetMessageRaw("Param.Moderation"), ModerationMode.NotRejected.ToString(), mainCategory);

            ParamsDefinition["DisplaySort"] = new BXParamSingleSelection(GetMessageRaw("Param.DisplaySort"), BXContentTagField.Name.ToString(), mainCategory);
            ParamsDefinition["DisplayOrder"] = new BXParamSort(GetMessageRaw("Param.DisplayOrder"), false, mainCategory);

            ParamsDefinition["TagLinkTemplate"] = new BXParamText(GetMessageRaw("Param.TagLinkTemplate"), "?tag=#SearchTags#", urlCategory);

            ParamsDefinition["SizeDistribution"] = new BXParamSingleSelection(GetMessageRaw("Param.SizeDistribution"), SizeInterpolationMode.Exponential.ToString(), sizeCategory);
            ParamsDefinition["SizeMin"] = new BXParamText(GetMessageRaw("Param.SizeMin"), "10", sizeCategory);
            ParamsDefinition["SizeMax"] = new BXParamText(GetMessageRaw("Param.SizeMax"), "50", sizeCategory);
            ParamsDefinition["SizePeriod"] = new BXParamText(GetMessageRaw("Param.SizePeriod"), "", sizeCategory);

            ParamsDefinition["ColorDistribution"] = new BXParamSingleSelection(GetMessageRaw("Param.ColorDistribution"), ColorInterpolationMode.None.ToString(), colorCategory);
            ParamsDefinition["ColorMin"] = new BXParamColor(GetMessageRaw("Param.ColorMin"), Color.Empty, colorCategory);
            ParamsDefinition["ColorMax"] = new BXParamColor(GetMessageRaw("Param.ColorMax"), Color.Empty, colorCategory);

            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad);
            ParamsDefinition["PagingAllow"].DefaultValue = "False";
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad);

            #region DEFERRED
            ////Cache
            //ParamsDefinition.Add(BXParametersDefinition.Cache);

            ////Критерий кеширования дополнительный параметр, влияющий на ключ кеширования.
            ////Клиент указывая этот параметр для обеспечения корректного кеширования при динамически устанавливаемом запросе
            //ParamsDefinition["CacheCriterion"] =
            //    new BXParamText(
            //        "CacheCriterion",
            //        string.Empty,
            //        null,
            //        new ParamClientSideActionGroupViewMember(ClientID, "CacheCriterion", new string[] { "Hidden" })
            //        );
            #endregion
        }
        protected override void LoadComponentDefinition()
        {
            List<BXParamValue> filter = new List<BXParamValue>();
            foreach (KeyValuePair<string, BXSearchContentFilterInfo> p in BXSearchContentManager.GetSearchFiltersInfo())
                filter.Add(new BXParamValue(p.Value.SystemName, p.Key));
            ParamsDefinition["Filter"].Values = filter;

            foreach (string s in new string[] { "SelectionSort", "DisplaySort" })
            {
                List<BXParamValue> fields = new List<BXParamValue>();
                fields.Add(new BXParamValue(GetMessageRaw("Option.Name"), BXContentTagField.Name.ToString()));
                fields.Add(new BXParamValue(GetMessageRaw("Option.TagCount"), BXContentTagField.TagCount.ToString()));
                fields.Add(new BXParamValue(GetMessageRaw("Option.LastUpdate"), BXContentTagField.LastUpdate.ToString()));
                ParamsDefinition[s].Values = fields;
            }

            List<BXParamValue> moderation = new List<BXParamValue>();
            moderation.Add(new BXParamValue(GetMessageRaw("Option.All"), ModerationMode.All.ToString()));
            moderation.Add(new BXParamValue(GetMessageRaw("Option.NotRejected"), ModerationMode.NotRejected.ToString()));
            moderation.Add(new BXParamValue(GetMessageRaw("Option.Approved"), ModerationMode.Approved.ToString()));
            ParamsDefinition["Moderation"].Values = moderation;

            List<BXParamValue> sizeDistr = new List<BXParamValue>();
            sizeDistr.Add(new BXParamValue(GetMessageRaw("Option.DontUse"), SizeInterpolationMode.None.ToString()));
            sizeDistr.Add(new BXParamValue(GetMessageRaw("Option.Linear"), SizeInterpolationMode.Linear.ToString()));
            sizeDistr.Add(new BXParamValue(GetMessageRaw("Option.Exponential"), SizeInterpolationMode.Exponential.ToString()));
            ParamsDefinition["SizeDistribution"].Values = sizeDistr;

            List<BXParamValue> colorDistr = new List<BXParamValue>();
            colorDistr.Add(new BXParamValue(GetMessageRaw("Option.DontUse"), ColorInterpolationMode.None.ToString()));
            colorDistr.Add(new BXParamValue(GetMessageRaw("Option.Linear"), ColorInterpolationMode.Linear.ToString()));
            colorDistr.Add(new BXParamValue(GetMessageRaw("Option.Logarithmic"), ColorInterpolationMode.Logarithmic.ToString()));
            ParamsDefinition["ColorDistribution"].Values = colorDistr;

            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad);
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad);
        }

        public class TagInfo
        {
            internal double modifiedCount;
            private BXContentTag tag;
            private int fontSize;
            private Color color;
            private string url;

            internal TagInfo()
            {

            }

            public BXContentTag Tag
            {
                get
                {
                    return tag;
                }
                set
                {
                    tag = value;
                }
            }
            public int FontSize
            {
                get
                {
                    return fontSize;
                }
                set
                {
                    fontSize = value;
                }
            }
            public Color Color
            {
                get
                {
                    return color;
                }
                set
                {
                    color = value;
                }
            }
            public string Url
            {
                get
                {
                    return url;
                }
                set
                {
                    url = value;
                }
            }
            public string Href
            {
                get
                {
                    return HttpUtility.HtmlEncode(Url ?? "");
                }
            }
        }
        public enum ModerationMode
        {
            All,
            NotRejected,
            Approved
        }
        public enum SizeInterpolationMode
        {
            None,
            Linear,
            Exponential
        }
        public enum ColorInterpolationMode
        {
            None,
            Linear,
            Logarithmic
        }
    }
    public class SearchTagsCloudTemplate : BXComponentTemplate<SearchTagsCloudComponent>
    {
    }
}