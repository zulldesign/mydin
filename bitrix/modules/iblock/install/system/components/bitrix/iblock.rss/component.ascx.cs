using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Services;
using Bitrix.Configuration;
using Bitrix.Services.Syndication;
using System.IO;
using System.Xml;
using System.Text;
using Bitrix.Services.Text;
using Bitrix.Components.Editor;
using Bitrix.DataLayer;
using Bitrix.Security;
using Bitrix.DataTypes;
using Bitrix.IO;

namespace Bitrix.IBlock.Components
{
    /// <summary>
    /// Параметр компонента "RSS инфоблока"
    /// </summary>
    public enum IBlockRssParameter
    {
        /// <summary>
        /// Ид типа инфоблока
        /// </summary>
        IBlockTypeId = 1,
        /// <summary>
        /// Ид инфоблока
        /// </summary>
        IBlockId,
        /// <summary>
        /// Ид раздела
        /// </summary>
        IBlockSectionId,
        /// <summary>
        /// Включать подразделы
        /// </summary>
		IncludeSubsections,
        /// <summary>
        /// Кол-во эл-тов
        /// </summary>
		ItemQuantity,
        /// <summary>
        /// Кол-во дней
        /// </summary>
        DayQuantity,
        /// <summary>
        /// Заголовок канала
        /// </summary>
        FeedTitle,
        /// <summary>
        /// Описание канала
        /// </summary>
        FeedDescription,
        /// <summary>
        /// Шаблон ссылки на канал
        /// </summary>
        FeedUrlTemplate,
        /// <summary>
        /// Шаблон ссылки на элемент канала
        /// </summary>
        FeedItemUrlTemplate,
        /// <summary>
        /// Время жизни 
        /// </summary>
        FeedTtl,
        /// <summary>
        /// Поле инфоблока для заполнения заголовка сообщения канала
        /// </summary>
        FeedItemTitleSourceField,
        /// <summary>
        /// Режим построения содержания сообщения канала
        /// </summary>
        FeedItemDescriptionBuildMode,
        /// <summary>
        /// Поле инфоблока для заполнения содержания сообщения канала
        /// </summary>
        FeedItemDescriptionSourceField,
        /// <summary>
        /// Шаблон для заполнения содержания сообщения канала
        /// </summary>
        FeedItemDescriptionTemplate,
        /// <summary>
        /// Режим построения категорий
        /// </summary>
        FeedItemCategoryBuildMode,
        /// <summary>
        /// Режим сортировки
        /// </summary>
        FeedItemSortMode,
        /// <summary>
        /// Фильтрация по активности
        /// </summary>
        FiltrationByActivity,
		/// <summary>
		/// Не использовать CDATA
		/// </summary>
		NoCData
    }

    /// <summary>
    /// Режим построения содержания сообщения канала
    /// </summary>
    public enum IBlockRssFeedItemDescriptionBuildMode
    {
        /// <summary>
        /// Описание берётся из одного поля инфоблока
        /// </summary>
        SingleField = 1,
        /// <summary>
        /// Описание составляется из полей инфоблока по заданному шаблону
        /// </summary>
        MultipleField
    }

    /// <summary>
    /// Режим построения категорий сообщения
    /// </summary>
    public enum IBlockRssFeedItemCategoryBuildMode
    { 
        /// <summary>
        /// Нет
        /// </summary>
        None = 0,
        /// <summary>
        /// Из резделов
        /// </summary>
        IBlockSections,
        /// <summary>
        /// Из тегов
        /// </summary>
        Tags
    }

    /// <summary>
    /// Режим сортировки сообщений
    /// </summary>
    public enum IBlockRssFeedItemSortMode
    {
        /// <summary>
        /// По Ид
        /// </summary>
        ID = 1,
        /// <summary>
        /// Дата создания
        /// </summary>
        DateOfCreation,
        /// <summary>
        /// Дата модификации
        /// </summary>
        DateOfModification,
        /// <summary>
        /// Дата начала активности
        /// </summary>
        DateOfActivationStart
    }

    public enum IBlockRssFiltrationByActivity
    { 
        /// <summary>
        /// Нет
        /// </summary>
        None = 0,
        /// <summary>
        /// Активные
        /// </summary>
        Active,
        /// <summary>
        /// Неактивные
        /// </summary>
        NotActive
    }

    /// <summary>
    /// Ошибка
    /// </summary>
    public enum IBlockRssError
    {
        None = 0,
        /// <summary>
        /// Нет даных (нет данных доступных для просмотра неавторизованными пользователями)
        /// </summary>
        NoData = -4,
        IBlockNotFound  = -5,
        FeedCreationGeneral = -100
    }

    /// <summary>
    /// Компонент RSS информационного блока
    /// </summary>
    public partial class IBlockRssComponent : BXComponent
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
                _error = IBlockRssError.FeedCreationGeneral;
            }
            if (ComponentError != IBlockRssError.None)
                AbortCache();
        }


        private string GetElementPropertyValueForDescription(string name, BXIBlockElement element) 
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            string nameUp = name.ToUpperInvariant();

            if (string.Equals(nameUp, "NAME", StringComparison.InvariantCulture))
                return element.Name;
            else if (string.Equals(nameUp, "ACTIVEFROMDATE", StringComparison.InvariantCulture))
                return element.ActiveFromDate.ToString("d");
            else if (string.Equals(nameUp, "ACTIVETODATE", StringComparison.InvariantCulture))
                return element.ActiveToDate.ToString("d");
            else if (string.Equals(nameUp, "CREATEDATE", StringComparison.InvariantCulture))
                return element.CreateDate.ToString("d");
            else if (string.Equals(nameUp, "UPDATEDATE", StringComparison.InvariantCulture))
                return element.UpdateDate.ToString("d");
            else if (string.Equals(nameUp, "CREATEDBY", StringComparison.InvariantCulture) 
                || string.Equals(nameUp, "CREATEDBYUSER", StringComparison.InvariantCulture))
            {
                BXUser user = element.CreatedByUser;
                return user != null ? user.UserName : string.Empty;
            }
            else if (string.Equals(nameUp, "MODIFIEDBY", StringComparison.InvariantCulture) 
                || string.Equals(nameUp, "MODIFIEDBYUSER", StringComparison.InvariantCulture))
            {
                BXUser user = element.ModifiedByUser;
                return user != null ? user.UserName : string.Empty;
            }
            else if (string.Equals(nameUp, "DETAILIMAGEID", StringComparison.InvariantCulture) 
                || string.Equals(nameUp, "DETAILIMAGE", StringComparison.InvariantCulture))
            {
                BXFile file = null;
                if (element.DetailImageId <= 0 || (file = element.DetailImage) == null)
                    return string.Empty;
                return string.Concat("<img src=\"", Convert2AbsoluteUrl(file.FilePath), "\" width=\"", file.Width.ToString(), "px\" height=\"", file.Height.ToString(), "\"/>");
            }
            else if (string.Equals(nameUp, "PREVIEWIMAGEID", StringComparison.InvariantCulture) 
                || string.Equals(nameUp, "PREVIEWIMAGE", StringComparison.InvariantCulture))
            {
                BXFile file = null;
                if (element.PreviewImageId <= 0 || (file = element.PreviewImage) == null)
                    return string.Empty;
                return string.Concat("<img src=\"", Convert2AbsoluteUrl(file.FilePath), "\" border=\"0\" width=\"", file.Width.ToString(), "px\" height=\"", file.Height.ToString(), "\"/>");
            }
            else if (string.Equals(nameUp, "IBLOCK", StringComparison.InvariantCulture))
            {
                BXIBlock block = element.IBlock;
                return block != null ? block.Name : string.Empty;
            }

            else if (string.Equals(nameUp, "PREVIEWTEXT", StringComparison.InvariantCulture))
                return element.PreviewTextType == BXTextType.Html ? element.PreviewText: HttpUtility.HtmlEncode(element.PreviewText);
            else if (string.Equals(nameUp, "DETAILTEXT", StringComparison.InvariantCulture))
                return element.DetailTextType == BXTextType.Html ? element.DetailText : HttpUtility.HtmlEncode(element.DetailText);
            else if (string.Equals(nameUp, "PREVIEWTEXTORDETAILTEXT", StringComparison.InvariantCulture))
            {
                string r = !BXStringUtility.IsNullOrTrimEmpty(element.PreviewText) ? element.PreviewText : element.DetailText;
                return element.DetailTextType == BXTextType.Html ? r : HttpUtility.HtmlEncode(r);
            }
            else if (string.Equals(nameUp, "DETAILTEXTORPREVIEWTEXT", StringComparison.InvariantCulture))
            {
                string r = !BXStringUtility.IsNullOrTrimEmpty(element.DetailText) ? element.DetailText : element.PreviewText;
                return element.DetailTextType == BXTextType.Html ? r : HttpUtility.HtmlEncode(r);
            }

            BXCustomFieldCollection fields = element.CustomFields;
            BXCustomField field;
            if (!fields.TryGetValue(name, out field))
                return string.Empty;
            BXCustomPropertyCollection values = element.CustomValues;
            BXCustomProperty property;
            if (!values.TryGetValue(name, out property))
                return string.Empty;

            BXCustomTypePublicView view = BXCustomTypeManager.GetCustomType(field.CustomTypeId).CreatePublicView();
            view.Init(property, field);
            return view.GetHtml();
        }

        protected string GetCurrentSiteUrl()
        {
            return VirtualPathUtility.ToAbsolute("~/");
        }

        private Uri _uri;
        private string Convert2AbsoluteUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && Uri.TryCreate(BXSefUrlManager.CurrentUrl, Page.ResolveUrl(url), out _uri) ? _uri.ToString() : VirtualPathUtility.ToAbsolute("~/");
        }

        /*
        private BXIBlockCollection _permittedBlocks = null;
        /// <summary>
        /// Запрос разрешённых блоков
        ///  - активные относящиеся к текущему сайту;
        ///  - разрешающие чтение для "Everyone". 
        /// </summary>
        /// <returns></returns>
        private BXIBlockCollection GetPermittedBlocks()
        {
            if (_permittedBlocks != null)
                return _permittedBlocks;

            BXFilter filter = new BXFilter(
                    new BXFilterItem(BXIBlock.Fields.Active, BXSqlFilterOperators.Equal, 'Y'),
                    new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
                    );

            BXIBlockCollection blocks = BXIBlock.GetList(
                filter,
                new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Name, BXOrderByDirection.Asc)),
                new BXSelect(BXSelectFieldPreparationMode.Normal, BXIBlock.Fields.ID, BXIBlock.Fields.Name),
                null
                );

            _permittedBlocks = new BXIBlockCollection();
            foreach (BXIBlock block in blocks)
            {
                // TODO Переделать код для запроса списка ролей операции единожды
                BXRoleCollection roles = BXRoleManager.GetAllRolesForOperation(BXIBlock.Operations.IBlockPublicRead, "iblock", block.Id.ToString());
                foreach (BXRole role in roles)
                {
                    if (!string.Equals(role.RoleName, "Guest", StringComparison.InvariantCulture))
                        continue;
                    _permittedBlocks.Add(block);
                    break;
                }
            }
            return _permittedBlocks;
        }
        */
        /// <summary>
        /// Подготовить RSS канал
        /// </summary>
        private void PrepareFeed()
        {
			_feedSavingOptions = new BXSyndicationElementXmlSavingOptions();
			_feedSavingOptions.UseCData = UseCData;

            _feed = new BXRss20Channel();
            //_feed.Title = FeedTitle;
            //_feed.Description = FeedDescription;
            _feed.LastBuildDate = DateTime.Now.ToUniversalTime();
            _feed.Language = CurrentSite.GetCultureInfo().Name;
            _feed.Generator = "bitrix::iblock.rss";
            int feedTtl = FeedTtl;
            if(feedTtl > 0)
                _feed.Ttl = feedTtl;

            /*
            BXIBlockCollection permittedBlocks = GetPermittedBlocks();
            List<int> permittedBlocksIdList = new List<int>();
            foreach (BXIBlock permittedBlock in permittedBlocks)
                permittedBlocksIdList.Add(permittedBlock.Id);

            if (permittedBlocksIdList.Count == 0)
            {
                _error = IBlockRssError.NoData;
                return;
            }
            */
            BXQueryParams qp = null;
            int itemQuantity = ItemQuantity;
            if (itemQuantity > 0)
                qp = new BXQueryParams(new BXPagingOptions(0, itemQuantity));

            BXFilter f = new BXFilter();

            BXIBlock block = null;
            int blockId = IBlockId;
            if (blockId <= 0 || (block = BXIBlock.GetById(blockId, BXTextEncoder.EmptyTextEncoder)) == null)
            {
                _error = IBlockRssError.IBlockNotFound;
                return;
            }

            BXFile image = block.Image;
            if (image != null)
            {
                BXRss20ChannelImage feedImage = new BXRss20ChannelImage();
                feedImage.Title = block.Name;
                feedImage.Url = Convert2AbsoluteUrl(image.FilePath);
                feedImage.Link = Convert2AbsoluteUrl(CurrentSite.Directory);
                feedImage.Width = image.Width <= BXRss20ChannelImage.MaxWidth ? image.Width : BXRss20ChannelImage.MaxWidth;
                feedImage.Height = image.Height <= BXRss20ChannelImage.MaxHeight ? image.Height : BXRss20ChannelImage.MaxHeight;
                _feed.Image = feedImage;
            }

            bool isBlockPublicReadPermitted = false;
            BXRoleCollection blockPublicReadRoles = BXRoleManager.GetAllRolesForOperation(BXIBlock.Operations.IBlockPublicRead, "iblock", blockId.ToString());
            foreach (BXRole blockPublicReadRole in blockPublicReadRoles)
            {
                if (!string.Equals(blockPublicReadRole.RoleName, "Guest", StringComparison.InvariantCulture))
                    continue;
                isBlockPublicReadPermitted = true;
                break;
            }
            if (!isBlockPublicReadPermitted)
            {
                _error = IBlockRssError.NoData;
                return;
            }

            //фильтруем по блоку
            f.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, blockId));

            BXIBlockSection section = null;
            int sectionId = IBlockSectionId;
            if (sectionId > 0 && (section = BXIBlockSection.GetById(sectionId, BXTextEncoder.EmptyTextEncoder)) != null)  //фильтруем по секции
            {
                if (IncludeSubsections)
                {
                    f.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.LeftMargin, BXSqlFilterOperators.GreaterOrEqual, section.LeftMargin));
                    f.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.RightMargin, BXSqlFilterOperators.LessOrEqual, section.RightMargin));
                }
                else
                    f.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, sectionId));
            }

            if (!string.IsNullOrEmpty(FeedTitle))
                _feed.Title = FeedTitle;
            else if (block != null)
                _feed.Title = section != null ? string.Concat(block.Name, "/", section.Name) : block.Name;

            if (!string.IsNullOrEmpty(FeedDescription))
                _feed.Description = FeedDescription;
            else if (block != null)
                _feed.Description = section != null ? string.Concat(block.Name, "/", section.Name) : block.Name;

            //построение ссылки на канал
            BXParamsBag<object> replaceParams = new BXParamsBag<object>();
            string feedUrlTemplate = FeedUrlTemplate;
            string feedLink = string.Empty;
            if (!string.IsNullOrEmpty(feedUrlTemplate))
            {
                replaceParams["IBlockId"] = block != null ? block.Id : 0;
                replaceParams["SectionId"] = section != null ? section.Id : 0;
                replaceParams["SectionCode"] = section != null ? section.Code : string.Empty;
                feedLink = Convert2AbsoluteUrl(BXSefUrlUtility.MakeLink(feedUrlTemplate, replaceParams));
            }

            if (!string.IsNullOrEmpty(feedLink))
                _feed.Link = feedLink;
            else if (!string.IsNullOrEmpty(feedUrlTemplate))
            {
                replaceParams["IBlockId"] = 0;
                replaceParams["SectionId"] = 0;
                replaceParams["SectionCode"] = string.Empty;
                //_feed.Link = Convert2AbsoluteUrl(BXSefUrlUtility.MakeLink(feedUrlTemplate, replaceParams));
                _feed.Link = ResolveUrl(BXSefUrlUtility.MakeLink(feedUrlTemplate, replaceParams));
            }
            else
                _feed.Link = GetCurrentSiteUrl();

            replaceParams.Remove("IBlockId");
            replaceParams.Remove("SectionId");
            replaceParams.Remove("SectionCode");
            //---

            //только активные
            //f.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, 'Y'));
            switch (FiltrationByActivity)
            {
                case IBlockRssFiltrationByActivity.Active:
                    f.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, 'Y'));
                    break;
                case IBlockRssFiltrationByActivity.NotActive:
                    f.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, 'N'));
                    break;
                case IBlockRssFiltrationByActivity.None:
                    break;
                default:
                    throw new NotSupportedException(string.Format("IBlockRssFiltrationByActivity.{0} is unknown in current context!", FiltrationByActivity.ToString("G")));
            }
            //только в периоде активности
            f.Add(new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, 'Y'));
            //только из разрешённых блоков
            //f.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.In, permittedBlocksIdList));

            BXOrderBy sort = null;
            IBlockRssFeedItemSortMode itemSortMode = FeedItemSortMode;
            switch (itemSortMode)
            {
                case IBlockRssFeedItemSortMode.ID:
                    sort = new BXOrderBy(new BXOrderByPair(BXIBlockElement.Fields.ID, BXOrderByDirection.Desc));
                    break;
                case IBlockRssFeedItemSortMode.DateOfCreation:
                    sort = new BXOrderBy(new BXOrderByPair(BXIBlockElement.Fields.CreateDate, BXOrderByDirection.Desc));
                    break;
                case IBlockRssFeedItemSortMode.DateOfModification:
                    sort = new BXOrderBy(new BXOrderByPair(BXIBlockElement.Fields.UpdateDate, BXOrderByDirection.Desc));
                    break;
                case IBlockRssFeedItemSortMode.DateOfActivationStart:
                    sort = new BXOrderBy(new BXOrderByPair(BXIBlockElement.Fields.ActiveFromDate, BXOrderByDirection.Desc));
                    break;
                default:
                    throw new InvalidOperationException(string.Format("IBlockRssFeedItemSortMode.{0} is unknown in current context!", itemSortMode.ToString("G")));
            }

            BXIBlockElementCollection elements = BXIBlockElement.GetList(
                f,
                sort,
                new BXSelect(
                    BXSelectFieldPreparationMode.Normal,
                    BXIBlockElement.Fields.ID,
                    BXIBlockElement.Fields.Name,
                    BXIBlockElement.Fields.Active,
                    BXIBlockElement.Fields.ActiveFromDate,
                    BXIBlockElement.Fields.ActiveToDate,
                    BXIBlockElement.Fields.Code,
                    BXIBlockElement.Fields.CreateDate,
                    BXIBlockElement.Fields.CreatedBy,
                    BXIBlockElement.Fields.UpdateDate,
                    BXIBlockElement.Fields.ModifiedBy,
                    BXIBlockElement.Fields.PreviewTextType,
                    BXIBlockElement.Fields.PreviewText,
                    BXIBlockElement.Fields.PreviewImageId,
                    BXIBlockElement.Fields.DetailTextType,
                    BXIBlockElement.Fields.DetailText,
                    BXIBlockElement.Fields.DetailImageId,
                    BXIBlockElement.Fields.IBlock.ID,
                    BXIBlockElement.Fields.IBlock.Name,
                    BXIBlockElement.Fields.CreatedBy,
                    BXIBlockElement.Fields.CreateDate,
                    BXIBlockElement.Fields.Sections.Section,
                    BXIBlockElement.Fields.Tags,
                    BXIBlockElement.Fields.CustomFields[blockId]
                    ),
                qp,
                BXTextEncoder.EmptyTextEncoder
            );

            string feedItemUrlTemplate = FeedItemUrlTemplate;
            IBlockRssFeedItemDescriptionBuildMode itemDescrBuildMode = FeedItemDescriptionBuildMode;
            IBlockRssFeedItemCategoryBuildMode itemCategoryBuildMode = FeedItemCategoryBuildMode;
            string itemTitleSourceField = FeedItemTitleSourceField,
                itemDescrSourceField = FeedItemDescriptionSourceField,
                itemDescrTemplate = FeedItemDescriptionTemplate;
            for (int i = 0; i < elements.Count; i++)
            {
                BXIBlockElement element = elements[i];
                BXIBlock elementBlock = element.IBlock;
                if (elementBlock == null)
                    throw new InvalidOperationException("Could not find IBlock!");

                BXRss20ChannelItem item = _feed.Items.Create();
                item.Title = !string.IsNullOrEmpty(itemTitleSourceField) ? GetElementPropertyValueForDescription(itemTitleSourceField, element) : element.Name;
                //item.Categories.Add(new BXRss20Category(elementBlock.Name));
                switch (itemCategoryBuildMode)
                {
                    case IBlockRssFeedItemCategoryBuildMode.None:
                        break;
                    case IBlockRssFeedItemCategoryBuildMode.IBlockSections:
                        {
                            BXIBlockSectionCollection sections = element.GetSections();
                            if (sections != null && sections.Count > 0)
                                for(int j = 0; j < sections.Count; j++)
                                    item.Categories.Add(new BXRss20Category(sections[j].Name));
                        }
                        break;
                    case IBlockRssFeedItemCategoryBuildMode.Tags:
                        {
                            string tagsStr = element.Tags;
                            if (!string.IsNullOrEmpty(tagsStr))
                            {
                                string[] tags = tagsStr.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                                for(int j = 0; j < tags.Length; j++)
                                    item.Categories.Add(new BXRss20Category(tags[j].Trim()));
                            }
                        }
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("IBlockRssFeedItemCategoryBuildMode.{0} is unknown in current context!", itemCategoryBuildMode.ToString("G")));
                }
                string itemDescr = string.Empty;
                if (itemDescrBuildMode == IBlockRssFeedItemDescriptionBuildMode.SingleField)
                {
                    if (!string.IsNullOrEmpty(itemDescrSourceField))
                        itemDescr = GetElementPropertyValueForDescription(itemDescrSourceField, element);
                }
                else if (!string.IsNullOrEmpty(itemDescrTemplate))
                {
                    StringBuilder sb = new StringBuilder();
                    int startInd = 0;
                    while (startInd <= itemDescrTemplate.Length - 1)
                    {
                        int fInd = -1, sInd = -1;
                        if ((fInd = itemDescrTemplate.IndexOf('#', startInd)) < 0 || (sInd = itemDescrTemplate.IndexOf('#', fInd + 1)) < 0)
                        {
                            sb.Append(itemDescrTemplate.Substring(startInd));
                            break;
                        }

                        sb.Append(itemDescrTemplate.Substring(startInd, fInd - startInd));
                        string subsParamName = itemDescrTemplate.Substring(fInd + 1, sInd - fInd -1);
                        if (!string.IsNullOrEmpty(subsParamName))
                            sb.Append(GetElementPropertyValueForDescription(subsParamName, element));
                        startInd = sInd + 1;
                    }
                    itemDescr = sb.ToString();
                }
                item.Description = !string.IsNullOrEmpty(itemDescr) ? itemDescr : element.PreviewTextType == BXTextType.Text ? HttpUtility.HtmlEncode(element.PreviewText) : element.PreviewText;

                if (!string.IsNullOrEmpty(feedItemUrlTemplate))
                {
                    replaceParams["IBlockId"] = elementBlock.Id;
                    BXIBlockSectionCollection elementSections = element.GetSections();
                    BXIBlockSection elementSection = elementSections != null && elementSections.Count > 0 ? elementSections[0] : null;
                    replaceParams["SectionId"] = elementSection != null ? elementSection.Id : 0;
                    replaceParams["SectionCode"] = elementSection != null ? elementSection.Code : string.Empty;
                    replaceParams["ElementId"] = element.Id;
                    replaceParams["ElementCode"] = element.Code;
                    item.Link = Convert2AbsoluteUrl(BXSefUrlUtility.MakeLink(feedItemUrlTemplate, replaceParams));
                    replaceParams.Remove("IBlockId");
                    replaceParams.Remove("SectionId");
                    replaceParams.Remove("SectionCode");
                    replaceParams.Remove("ElementId");
                    replaceParams.Remove("ElementCode");
                }
                else
                    item.Link = Request.Url.ToString();

                item.Guid = new BXRss20ChannelItemGuid(string.Concat("urn:bitrix:iblockelement:", element.Id.ToString()), false);
                //item.Source = ""//?;

                switch (itemSortMode)
                {
                    case IBlockRssFeedItemSortMode.ID:
                    case IBlockRssFeedItemSortMode.DateOfCreation:
                        {
                            BXUser author = element.CreatedByUser;
                            if (author != null)
                                item.Author = author.GetDisplayName();
                            item.PubDate = element.CreateDate;
                        }
                        break;
                    case IBlockRssFeedItemSortMode.DateOfModification:
                        {
                            BXUser author = element.ModifiedByUser;
                            if (author != null)
                                item.Author = author.GetDisplayName();
                            item.PubDate = element.UpdateDate;
                        }
                        break;
                    case IBlockRssFeedItemSortMode.DateOfActivationStart:
                        {
                            BXUser author = element.CreatedByUser;
                            if (author != null)
                                item.Author = author.GetDisplayName();
                            item.PubDate = element.ActiveFromDate;
                        }
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("IBlockRssFeedItemSortMode.{0} is unknown in current context!", FeedItemSortMode.ToString("G")));
                }
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

        private BXSite _currentSite = null;
        protected BXSite CurrentSite
        {
            get { return _currentSite ?? (_currentSite = BXSite.GetById(DesignerSite)); }
        }

        private IBlockRssError _error = IBlockRssError.None;
        /// <summary>
        /// Ошибка
        /// </summary>
        public IBlockRssError ComponentError
        {
            get { return _error; }
        }

        private BXIBlock _block = null;
        private bool _isBlockLoaded = false;
        /// <summary>
        /// Инфоблок
        /// </summary>
        public BXIBlock IBlock
        {
            get
            {
                if (_isBlockLoaded)
                    return _block;

                int blockId = IBlockId;
                _block = blockId > 0 ? BXIBlock.GetById(blockId) : null;
                _isBlockLoaded = true;
                return _block;
            }
        }

        /// <summary>
        /// Ид инфоблока
        /// </summary>
        public int IBlockId
        {
            get { return Parameters.Get<int>(GetParameterKey(IBlockRssParameter.IBlockId), 0); }
            set
            {
                if (IBlockId == value)
                    return;
                _isBlockLoaded = false;
                _block = null;
                Parameters[GetParameterKey(IBlockRssParameter.IBlockId)] = value.ToString();
            }
        }

        /// <summary>
        /// Ид секции
        /// </summary>
        public int IBlockSectionId
        {
            get { return Parameters.Get<int>(GetParameterKey(IBlockRssParameter.IBlockSectionId), 0); }
            set { Parameters[GetParameterKey(IBlockRssParameter.IBlockSectionId)] = value.ToString(); }
        }

		/// <summary>
        /// Включать подразделы
        /// </summary>
        public bool IncludeSubsections
        {
            get { return Parameters.GetBool(GetParameterKey(IBlockRssParameter.IncludeSubsections)); }
            set { Parameters[GetParameterKey(IBlockRssParameter.IncludeSubsections)] = value.ToString(); }
        }

        /// <summary>
        /// Количество элементов
        /// </summary>
        public int ItemQuantity
        {
            get { return Parameters.Get<int>(GetParameterKey(IBlockRssParameter.ItemQuantity), 0); }
            set { Parameters[GetParameterKey(IBlockRssParameter.ItemQuantity)] = value.ToString(); }
        }

        /*
        /// <summary>
        /// Количество дней
        /// </summary>
        public int DayQuantity
        {
            get { return Parameters.Get<int>(GetParameterKey(IBlockRssParameter.DayQuantity), 0); }
            set { Parameters[GetParameterKey(IBlockRssParameter.DayQuantity)] = value.ToString(); }
        }
        */
        /// <summary>
        /// Заголовок канала
        /// </summary>
        public string FeedTitle
        {
            get { return Parameters.Get(GetParameterKey(IBlockRssParameter.FeedTitle)); }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedTitle)] = value; }
        }

        /// <summary>
        /// Описание канала
        /// </summary>
        public string FeedDescription
        {
            get { return Parameters.Get(GetParameterKey(IBlockRssParameter.FeedDescription)); }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedDescription)] = value; }
        }

        /// <summary>
        /// Поле заголовка статьи
        /// </summary>
        public string FeedItemTitleSourceField
        {
            get { return Parameters.GetString(GetParameterKey(IBlockRssParameter.FeedItemTitleSourceField), "Name"); }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedItemTitleSourceField)] = value; }
        }

        /// <summary>
        /// Режим построения описания
        /// </summary>
        public IBlockRssFeedItemDescriptionBuildMode FeedItemDescriptionBuildMode
        {
            get
            {
                string resultStr = Parameters.Get(GetParameterKey(IBlockRssParameter.FeedItemDescriptionBuildMode));
                return !string.IsNullOrEmpty(resultStr) ? (IBlockRssFeedItemDescriptionBuildMode)Enum.Parse(typeof(IBlockRssFeedItemDescriptionBuildMode), resultStr) : IBlockRssFeedItemDescriptionBuildMode.SingleField;
            }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedItemDescriptionBuildMode)] = value.ToString("G"); }
        }

        /// <summary>
        /// Режим построения категорий
        /// </summary>
        public IBlockRssFeedItemCategoryBuildMode FeedItemCategoryBuildMode
        {
            get
            {
                string resultStr = Parameters.Get(GetParameterKey(IBlockRssParameter.FeedItemCategoryBuildMode));
                return !string.IsNullOrEmpty(resultStr) ? (IBlockRssFeedItemCategoryBuildMode)Enum.Parse(typeof(IBlockRssFeedItemCategoryBuildMode), resultStr) : IBlockRssFeedItemCategoryBuildMode.IBlockSections;
            }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedItemCategoryBuildMode)] = value.ToString("G"); }
        }

        /// <summary>
        /// Режим сортировки
        /// </summary>
        public IBlockRssFeedItemSortMode FeedItemSortMode
        {
            get
            {
                string resultStr = Parameters.Get(GetParameterKey(IBlockRssParameter.FeedItemSortMode));
                return !string.IsNullOrEmpty(resultStr) ? (IBlockRssFeedItemSortMode)Enum.Parse(typeof(IBlockRssFeedItemSortMode), resultStr) : IBlockRssFeedItemSortMode.ID;
            }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedItemSortMode)] = value.ToString("G"); }
        }

        /// <summary>
        /// Поле описания статьи
        /// </summary>
        public string FeedItemDescriptionSourceField
        {
            get { return Parameters.GetString(GetParameterKey(IBlockRssParameter.FeedItemDescriptionSourceField), "PreviewText"); }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedItemDescriptionSourceField)] = value; }
        }

        /// <summary>
        /// Шаблон описания статьи
        /// </summary>
        public string FeedItemDescriptionTemplate
        {
            get { return Parameters.Get(GetParameterKey(IBlockRssParameter.FeedItemDescriptionTemplate)); }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedItemDescriptionTemplate)] = value; }
        }

        /// <summary>
        /// Шаблон ссылки на канал
        /// </summary>
        public string FeedUrlTemplate
        {
            get { return Parameters.Get(GetParameterKey(IBlockRssParameter.FeedUrlTemplate), string.Empty); }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedUrlTemplate)] = value; }
        }

        /// <summary>
        /// Шаблон ссылки на элемент канала
        /// </summary>
        public string FeedItemUrlTemplate
        {
            get { return Parameters.Get(GetParameterKey(IBlockRssParameter.FeedItemUrlTemplate), string.Empty); }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedItemUrlTemplate)] = value; }
        }

        /// <summary>
        /// Время жизни, в мин.
        /// </summary>
        public int FeedTtl
        {
            get { return Parameters.Get<int>(GetParameterKey(IBlockRssParameter.FeedTtl), 60); }
            set { Parameters[GetParameterKey(IBlockRssParameter.FeedTtl)] = value.ToString(); }
        }

        /// <summary>
        /// Фильтрация по активности
        /// </summary>
        public IBlockRssFiltrationByActivity FiltrationByActivity
        {
            get { return Parameters.GetEnum<IBlockRssFiltrationByActivity>(GetParameterKey(IBlockRssParameter.FiltrationByActivity), IBlockRssFiltrationByActivity.Active); }
            set { Parameters[GetParameterKey(IBlockRssParameter.FiltrationByActivity)] = value.ToString("G"); }
        }

		/// <summary>
        /// Использовать CDATA
        /// </summary>
        public bool UseCData
        {
            get
            {
                return !Parameters.GetBool(GetParameterKey(IBlockRssParameter.NoCData), false);
            }
            set
            {
                Parameters[GetParameterKey(IBlockRssParameter.NoCData)] = (!value).ToString();
            }
        }

        /// <summary>
        /// Текст ошибки
        /// </summary>
        public string ComponentErrorText
        {
            get
            {
                if (_error == IBlockRssError.None)
                    return string.Empty;
                else if (_error == IBlockRssError.NoData)
                    return GetMessageRaw("Error.NoData");
                else if(_error == IBlockRssError.IBlockNotFound)
                    return GetMessageRaw("Error.IBlockNotFound");
                else
                    return GetMessageRaw("Error.General");
            }
        }
        protected string GetParameterKey(IBlockRssParameter parameter)
        {
            return parameter.ToString("G");
        }

        protected string GetParameterTitle(IBlockRssParameter parameter)
        {
            return GetMessageRaw(string.Concat("Param.", parameter.ToString("G")));
        }
        protected string GetParameterValueTitle(IBlockRssParameter parameter, string valueKey)
        {
            return GetMessageRaw(string.Concat("Param.", parameter.ToString("G"), ".", valueKey));
        }
        #region BXComponent
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";
            new BXComponentGroup("rss", "RSS", 150, BXComponentGroup.Content);
            ParamsDefinition.Add(BXParametersDefinition.Cache);

            BXCategory mainCategory = BXCategory.Main;
            BXCategory rssCategory = new BXCategory(GetMessageRaw("Category.RssSettings"), "RssSettings", 110);
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

            string iblockTypeKey = GetParameterKey(IBlockRssParameter.IBlockTypeId);
            ParamsDefinition.Add(
                iblockTypeKey,
                new BXParamSingleSelection(
                    GetParameterTitle(IBlockRssParameter.IBlockTypeId),
                    string.Empty,
                    mainCategory
                    )
                );

            string iblockKey = GetParameterKey(IBlockRssParameter.IBlockId);
            ParamsDefinition.Add(
                iblockKey,
                new BXParamSingleSelectionWithText(
                    GetParameterTitle(IBlockRssParameter.IBlockId),
                    String.Empty,
                    mainCategory
                    )
                );

            string iblockSectionKey = GetParameterKey(IBlockRssParameter.IBlockSectionId);
            ParamsDefinition.Add(
                iblockSectionKey,
                new BXParamText(
                    GetParameterTitle(IBlockRssParameter.IBlockSectionId),
                    string.Empty,
                    mainCategory
                    )
                );

			string iblockIncludeSubsectionsKey = GetParameterKey(IBlockRssParameter.IncludeSubsections);
            ParamsDefinition.Add(
                iblockIncludeSubsectionsKey,
                new BXParamYesNo(
                    GetParameterTitle(IBlockRssParameter.IncludeSubsections),
                    true,
                    mainCategory
                    )
                );

            string filtrationByActivityKey = GetParameterKey(IBlockRssParameter.FiltrationByActivity);
            ParamsDefinition.Add(
                filtrationByActivityKey,
                new BXParamSingleSelection(
                    GetParameterTitle(IBlockRssParameter.FiltrationByActivity),
                    IBlockRssFiltrationByActivity.Active.ToString("G"),
                    mainCategory
                    )
                );

            string itemQuantityKey = GetParameterKey(IBlockRssParameter.ItemQuantity);
            ParamsDefinition.Add(
                itemQuantityKey,
                new BXParamText(
                    GetParameterTitle(IBlockRssParameter.ItemQuantity),
                    "20",
                    mainCategory
                    )
                );

            /*
            string dayQuantityKey = GetParameterKey(IBlockRssParameter.DayQuantity);
            ParamsDefinition.Add(
                 dayQuantityKey,
                 new BXParamText(
                     GetParameterTitle(IBlockRssParameter.DayQuantity),
                     string.Empty,
                     mainCategory
                     )
                 );
            */
            ParamsDefinition.Add(
                GetParameterKey(IBlockRssParameter.FeedTitle),
                new BXParamText(
                    GetParameterTitle(IBlockRssParameter.FeedTitle),
                    string.Empty,
                    rssCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(IBlockRssParameter.FeedDescription),
                new BXParamText(
                    GetParameterTitle(IBlockRssParameter.FeedDescription),
                    string.Empty,
                    rssCategory
                    )
                );


            ParamsDefinition.Add(
                GetParameterKey(IBlockRssParameter.FeedItemTitleSourceField),
                new BXParamSingleSelection(
                    GetParameterTitle(IBlockRssParameter.FeedItemTitleSourceField),
                    "Name",
                    rssCategory
                    )
                );

            string itemCategoryBuildMode = GetParameterKey(IBlockRssParameter.FeedItemCategoryBuildMode);
            ParamsDefinition.Add(
                itemCategoryBuildMode,
                new BXParamSingleSelection(
                    GetParameterTitle(IBlockRssParameter.FeedItemCategoryBuildMode),
                    IBlockRssFeedItemCategoryBuildMode.IBlockSections.ToString("G"),
                    rssCategory
                    )
                );

            string itemDescrBuildModeKey = GetParameterKey(IBlockRssParameter.FeedItemDescriptionBuildMode);
            ParamsDefinition.Add(
                itemDescrBuildModeKey,
                new BXParamSingleSelection(
                    GetParameterTitle(IBlockRssParameter.FeedItemDescriptionBuildMode),
                    IBlockRssFeedItemDescriptionBuildMode.SingleField.ToString("G"),
                    rssCategory,
                    null,
                    new ParamClientSideActionGroupViewSelector(ClientID, itemDescrBuildModeKey)
                    )
                );

            string itemDescrSourceFieldKey = GetParameterKey(IBlockRssParameter.FeedItemDescriptionSourceField);
            ParamsDefinition.Add(
                itemDescrSourceFieldKey,
                new BXParamSingleSelection(
                    GetParameterTitle(IBlockRssParameter.FeedItemDescriptionSourceField),
                    "PreviewText",
                    rssCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, itemDescrSourceFieldKey, new string[] { IBlockRssFeedItemDescriptionBuildMode.SingleField.ToString("G") })
                    )
                );

            string itemDescrTemplateKey = GetParameterKey(IBlockRssParameter.FeedItemDescriptionTemplate);
            ParamsDefinition.Add(
                itemDescrTemplateKey,
                new BXParamMultilineText(
                    GetParameterTitle(IBlockRssParameter.FeedItemDescriptionTemplate),
                    "<div>#Name#</div><div>#PreviewText#</div>",
                    rssCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, itemDescrTemplateKey, new string[] { IBlockRssFeedItemDescriptionBuildMode.MultipleField.ToString("G") })
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(IBlockRssParameter.FeedUrlTemplate),
                new BXParamText(
                    GetParameterTitle(IBlockRssParameter.FeedUrlTemplate),
                    string.Empty,
                    rssCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(IBlockRssParameter.FeedItemUrlTemplate),
                new BXParamText(
                    GetParameterTitle(IBlockRssParameter.FeedItemUrlTemplate),
                    "~/iblockelement.aspx?item=#ElementId#",
                    rssCategory
                    )
                );

            string itemSortModeKey = GetParameterKey(IBlockRssParameter.FeedItemSortMode);
            ParamsDefinition.Add(
                itemSortModeKey,
                new BXParamSingleSelection(
                    GetParameterTitle(IBlockRssParameter.FeedItemSortMode),
                    IBlockRssFeedItemSortMode.ID.ToString("G"),
                    rssCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(IBlockRssParameter.FeedTtl),
                new BXParamText(
                GetParameterTitle(IBlockRssParameter.FeedTtl),
                    "60",
                    rssCategory
                    )
                );

			ParamsDefinition.Add(
                GetParameterKey(IBlockRssParameter.NoCData),
                new BXParamYesNo(
                    GetParameterTitle(IBlockRssParameter.NoCData),
                    false,
                    additionalSettingsCategory
                )
            );
        }

        protected override void LoadComponentDefinition()
        {
            //Iblock type
            List<BXParamValue> typeParamValue = new List<BXParamValue>();
            typeParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockType"), string.Empty));

            BXIBlockTypeCollection iblockTypes = BXIBlockType.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
            foreach (BXIBlockType iblockType in iblockTypes)
                typeParamValue.Add(new BXParamValue(iblockType.Translations[BXLoc.CurrentLocale].Name, iblockType.Id.ToString()));

            ParamsDefinition["IBlockTypeId"].Values = typeParamValue;
            ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;

            //Iblock
            int selectedIBlockType = 0;
            if (Parameters.ContainsKey("IBlockTypeId"))
                int.TryParse(Parameters["IBlockTypeId"], out selectedIBlockType);

            BXFilter filter = new BXFilter();
            if (selectedIBlockType > 0)
                filter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, selectedIBlockType));
            if (!String.IsNullOrEmpty(DesignerSite))
                filter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

            List<BXParamValue> iblockParamValue = new List<BXParamValue>();
            iblockParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockId"), string.Empty));
            BXIBlockCollection iblocks = BXIBlock.GetList(filter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
            foreach (BXIBlock iblock in iblocks)
            {
                // TODO Переделать код для запроса списка ролей операции единожды
                BXRoleCollection roles = BXRoleManager.GetAllRolesForOperation(BXIBlock.Operations.IBlockPublicRead, "iblock", iblock.Id.ToString());
                foreach (BXRole role in roles)
                {
                    if (!string.Equals(role.RoleName, "Guest", StringComparison.InvariantCulture))
                        continue;
                    iblockParamValue.Add(new BXParamValue(iblock.Name, iblock.Id.ToString()));
                    break;
                }
            }
            ParamsDefinition["IBlockId"].Values = iblockParamValue;
            ParamsDefinition["IBlockId"].RefreshOnDirty = true;

            //ItemDescrBuildMode
            IList<BXParamValue> itemDescrBuildModeValues = ParamsDefinition[GetParameterKey(IBlockRssParameter.FeedItemDescriptionBuildMode)].Values;
            itemDescrBuildModeValues.Clear();
            itemDescrBuildModeValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FeedItemDescriptionBuildMode, IBlockRssFeedItemDescriptionBuildMode.SingleField.ToString("G")), IBlockRssFeedItemDescriptionBuildMode.SingleField.ToString("G")));
            itemDescrBuildModeValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FeedItemDescriptionBuildMode, IBlockRssFeedItemDescriptionBuildMode.MultipleField.ToString("G")), IBlockRssFeedItemDescriptionBuildMode.MultipleField.ToString("G")));

            //ItemCategoryBuildMode
            IList<BXParamValue> itemCategoryBuildModeValues = ParamsDefinition[GetParameterKey(IBlockRssParameter.FeedItemCategoryBuildMode)].Values;
            itemCategoryBuildModeValues.Clear();
            itemCategoryBuildModeValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FeedItemCategoryBuildMode, IBlockRssFeedItemCategoryBuildMode.None.ToString("G")), IBlockRssFeedItemCategoryBuildMode.None.ToString("G")));
            itemCategoryBuildModeValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FeedItemCategoryBuildMode, IBlockRssFeedItemCategoryBuildMode.IBlockSections.ToString("G")), IBlockRssFeedItemCategoryBuildMode.IBlockSections.ToString("G")));
            itemCategoryBuildModeValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FeedItemCategoryBuildMode, IBlockRssFeedItemCategoryBuildMode.Tags.ToString("G")), IBlockRssFeedItemCategoryBuildMode.Tags.ToString("G")));

            //ItemSortMode
            IList<BXParamValue> itemSortModeValues = ParamsDefinition[GetParameterKey(IBlockRssParameter.FeedItemSortMode)].Values;
            itemSortModeValues.Clear();
            itemSortModeValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FeedItemSortMode, IBlockRssFeedItemSortMode.ID.ToString("G")), IBlockRssFeedItemSortMode.ID.ToString("G")));
            itemSortModeValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FeedItemSortMode, IBlockRssFeedItemSortMode.DateOfCreation.ToString("G")), IBlockRssFeedItemSortMode.DateOfCreation.ToString("G")));
            itemSortModeValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FeedItemSortMode, IBlockRssFeedItemSortMode.DateOfModification.ToString("G")), IBlockRssFeedItemSortMode.DateOfModification.ToString("G")));
            itemSortModeValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FeedItemSortMode, IBlockRssFeedItemSortMode.DateOfActivationStart.ToString("G")), IBlockRssFeedItemSortMode.DateOfActivationStart.ToString("G")));

            //itemTitleSource, ItemDescriptionSource
            IList<BXParamValue> itemTitleSourceValues = ParamsDefinition[GetParameterKey(IBlockRssParameter.FeedItemTitleSourceField)].Values;
            itemTitleSourceValues.Clear();
            IList<BXParamValue> itemDescriptionSourceValues = ParamsDefinition[GetParameterKey(IBlockRssParameter.FeedItemDescriptionSourceField)].Values;
            itemDescriptionSourceValues.Clear();

            //BXParamValue blockFieldParamValueNs = new BXParamValue(GetMessageRaw("NotSelectedNeuter"), string.Empty);
            //itemTitleSourceValues.Add(blockFieldParamValueNs);
            //itemDescriptionSourceValues.Add(blockFieldParamValueNs);
            BXParamValue blockFieldParamValueName = new BXParamValue(GetMessageRaw("IBlockElementName"), "Name");
            itemTitleSourceValues.Add(blockFieldParamValueName);
            itemDescriptionSourceValues.Add(blockFieldParamValueName);
            BXParamValue blockFieldParamValuePreviewText = new BXParamValue(GetMessageRaw("IBlockElementPreviewText"), "PreviewText");
            itemTitleSourceValues.Add(blockFieldParamValuePreviewText);
            itemDescriptionSourceValues.Add(blockFieldParamValuePreviewText);
            BXParamValue blockFieldParamValueDetail = new BXParamValue(GetMessageRaw("IBlockElementDetailText"), "DetailText");
            itemTitleSourceValues.Add(blockFieldParamValueDetail);
            itemDescriptionSourceValues.Add(blockFieldParamValueDetail);
            BXParamValue paramValuePreviewOrDetail = new BXParamValue(GetMessageRaw("IBlockElementPreviewTextOrIBlockElementDetailText"), "PreviewTextOrDetailText");
            itemTitleSourceValues.Add(paramValuePreviewOrDetail);
            itemDescriptionSourceValues.Add(paramValuePreviewOrDetail);
            BXParamValue paramValueDetailOrPreview = new BXParamValue(GetMessageRaw("IBlockElementDetailTextOrIBlockElementPreviewText"), "DetailTextOrPreviewText");
            itemTitleSourceValues.Add(paramValueDetailOrPreview);
            itemDescriptionSourceValues.Add(paramValueDetailOrPreview);


            int blockId = 0;
            if (Parameters.ContainsKey("IBlockId"))
                int.TryParse(Parameters["IBlockId"], out blockId);

            if (blockId > 0)
            {
                BXCustomFieldCollection customFields = BXIBlock.GetCustomFields(blockId);
                foreach (BXCustomField customField in customFields)
                {
                    BXParamValue blockCustomFieldParamValue = new BXParamValue(
                        BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel),
                        customField.Name
                        );

                    itemTitleSourceValues.Add(blockCustomFieldParamValue);
                    itemDescriptionSourceValues.Add(blockCustomFieldParamValue);
                }
            }

            IList<BXParamValue> filtrationByActivityValues = ParamsDefinition[GetParameterKey(IBlockRssParameter.FiltrationByActivity)].Values;
            if (filtrationByActivityValues.Count > 0)
                filtrationByActivityValues.Clear();
            filtrationByActivityValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FiltrationByActivity, IBlockRssFiltrationByActivity.None.ToString("G")), IBlockRssFiltrationByActivity.None.ToString("G")));
            filtrationByActivityValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FiltrationByActivity, IBlockRssFiltrationByActivity.Active.ToString("G")), IBlockRssFiltrationByActivity.Active.ToString("G")));
            filtrationByActivityValues.Add(new BXParamValue(GetParameterValueTitle(IBlockRssParameter.FiltrationByActivity, IBlockRssFiltrationByActivity.NotActive.ToString("G")), IBlockRssFiltrationByActivity.NotActive.ToString("G")));
        }

        protected override string GetCacheOutput()
        {
            return ((IBlockRssTemplate)ComponentTemplate).OutputXml;
        }

        protected override void SetCacheOutput(string output)
        {
            ((IBlockRssTemplate)ComponentTemplate).OutputXml = output;
        }
        #endregion
    }

    /// <summary>
    /// Базовый класс для шаблонов компонента "IBlockRssComponent"
    /// </summary>
    public abstract class IBlockRssTemplate : BXComponentTemplate<IBlockRssComponent>
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
            if (Component.ComponentError != IBlockRssError.None)
            {
                BXError404Manager.Set404Status(Response);
                BXPublicPage bitrixPage = Page as BXPublicPage;
                if (bitrixPage != null)
                    bitrixPage.Title = Component.ComponentErrorText;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            bool aboutError = Component.ComponentError != IBlockRssError.None;

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
