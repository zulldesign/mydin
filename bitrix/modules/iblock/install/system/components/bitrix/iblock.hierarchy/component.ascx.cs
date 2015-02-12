using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

using Bitrix.UI;
using Bitrix.Components;
using Bitrix.IBlock;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.DataTypes;
using Bitrix.Components.Editor;
using Bitrix.UI.Components;
using Bitrix.IBlock.UI;
using Bitrix.Configuration;
using Bitrix.UI.Hermitage;

namespace Bitrix.IBlock.Components
{
	public partial class IBlockHierarchyComponent : BXComponent
	{
		public enum ActiveFilter
        {
            Active = 1,
            NotActive,
            All
        }
		
		//FIELDS
		List<Section> resultSections;

		//PROPERTIES

		//PROPERTIES
		public int IBlockTypeId
		{
			get
			{
				return Math.Max(Parameters.GetInt("IBlockType"), 0);
			}
		}

		public int IBlockId
		{
			get
			{
				return Math.Max(Parameters.GetInt("IBlock"), 0);
			}
		}


        public string IBlockName
        {
            get { return ComponentCache.Get<string>("IBlockName", String.Empty); }
            set { ComponentCache["IBlockName"] = value; }
        }

		public int IBlockSectionId
		{
			get
			{
				return Math.Max(Parameters.GetInt("IBlockSection"), 0);
			}
		}
		public int Depth
		{
			get
			{
				return Math.Max(Parameters.GetInt("Depth"), 0);
			}
		}
		public string SectionUrlTemplate
		{
			get
			{
				return Parameters.Get("SectionUrlTemplate");
			}
		}
		public string ElementUrlTemplate
		{
			get
			{
				return Parameters.Get("ElementUrlTemplate");
			}
		}
		public bool GetElements
		{
			get
			{
				return Parameters.GetBool("GetElements", true);
			}
		}

        public int SectionsCount
        {
            get { return Parameters.Get<int>("PagingRecordsPerPage"); }
        }
		
		public List<Section> Sections
		{
			get
			{
				return resultSections;
			}
		}

        public bool PagingShow
        {
            get
            {
                return Parameters.Get<bool>("PagingShow",true);
            }
        }
        public string PagingPosition
        {
            get
            {
                return Parameters.Get("PagingPosition");
            }
        }

        public string SortBy1
        {
            get { return Parameters["SortBy1"]; }
        }

        public string SortBy2
        {
            get { return Parameters["SortBy2"]; }
        }

        public string SortOrder1
        {
            get { return Parameters["SortOrder1"]; }
        }

        public string SortOrder2
        {
            get { return Parameters["SortOrder2"]; }
        }

		public ActiveFilter SectionActiveFilter
		{
			get
			{
				object obj = null;
				if (ComponentCache.TryGetValue("SectionActiveFilter", out obj))
					return (ActiveFilter)obj;

				ActiveFilter filter = Parameters.GetEnum<ActiveFilter>("SectionActiveFilter", ActiveFilter.Active);
				ComponentCache["SectionActiveFilter"] = (int)filter;
				return filter;
			}
		}


		public ActiveFilter ElementActiveFilter
		{
			get
			{
				object obj = null;
				if (ComponentCache.TryGetValue("ActiveFilter", out obj))
					return (ActiveFilter)obj;

				ActiveFilter filter = Parameters.GetEnum<ActiveFilter>("ActiveFilter", ActiveFilter.Active);
				ComponentCache["ActiveFilter"] = (int)filter;
				return filter;
			}
		}

		public ActiveFilter ElementActiveDateFilter
		{
			get
			{
				object obj = null;
				if (ComponentCache.TryGetValue("ActiveDateFilter", out obj))
					return (ActiveFilter)obj;

				ActiveFilter filter = Parameters.GetEnum<ActiveFilter>("ActiveDateFilter", ActiveFilter.All);
				ComponentCache["ActiveDateFilter"] = (int)filter;
				return filter;
			}
		}

		//METHODS

		//Метод возвращает предопределенных значений для типов инфоблоков 
		private List<BXParamValue> GetIBlockTypeParamValues()
		{
			List<BXParamValue> values = new List<BXParamValue>();
			
			//Пустое значение
			values.Add(new BXParamValue("-", ""));
			
			//Собственно выборка всех типов инфоблока
			BXIBlockTypeCollection typeCollection = BXIBlockType.GetList(
				null,
				new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Sort, BXOrderByDirection.Asc)),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			foreach (BXIBlockType t in typeCollection)
				values.Add(new BXParamValue(t.Translations[BXLoc.CurrentLocale].Name, t.Id.ToString()));
			
			return values;
		}

		//Метод возвращает предопределенных значений для инфоблоков определенного типа
		private List<BXParamValue> GetIBlockParamValues(int iblockTypeId)
		{
			List<BXParamValue> values = new List<BXParamValue>();
			
			
			BXFilter iblockFilter = new BXFilter();
			iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Active, BXSqlFilterOperators.Equal, "Y"));
			
			//Если предоставлен корректный ID типа инфоблока, то выбираем только инфоблоки этого типа
			if (iblockTypeId > 0)
				iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, iblockTypeId));
			
			//Если можно определить сайт, на котором расположен компонент, то выбираем только инфоблоки, принадлежащие этому сайту
			string designerSite = DesignerSite;
			if (!string.IsNullOrEmpty(designerSite))
				iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, designerSite));

			//Собственно выборка
			BXIBlockCollection iblockCollection = BXIBlock.GetList(
				iblockFilter,
				new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Sort, BXOrderByDirection.Asc)),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			foreach (BXIBlock b in iblockCollection)
				values.Add(new BXParamValue(b.Name, b.Id.ToString()));
			
			
			return values;
		}

		//Метод возвращает предопределенных значений для секций определенного инфоблока
		private List<BXParamValue> GetIBlockSectionParamValues(int iblockId)
		{
			
			List<BXParamValue> values = new List<BXParamValue>();

			//Пустое значение - соответствует корневому разделу инфоблока
			values.Add(new BXParamValue("-", ""));
			
			//Если задан некорректный ID инфоблока, производить выборку бессмысленно
			if (iblockId <= 0)
				return values;

			//Собственно выборка секций, упорядоченных по вложенности
			BXIBlockSectionCollection sections = BXIBlockSection.GetList(
				new BXFilter(
					new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId),
					new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y")
				),
				new BXOrderBy(
					new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)
				),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			foreach (BXIBlockSection section in sections)
				values.Add(new BXParamValue(new string('-', section.DepthLevel - 1) + " " + section.Name, section.Id.ToString()));

			return values;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			BXPagingParams pagingParams = PreparePagingParams(
					Parameters.Get("PageID", HttpContext.Current.Request.QueryString["page"]),
					Parameters.Get("PageShowAll", HttpContext.Current.Request.QueryString["showall"] != null));

			if (IsCached(pagingParams))
			{
				SetTemplateCachedData();
				return;
			}

			if (IBlockId == 0)
				return;

			using (BXTagCachingScope cacheScope = BeginTagCaching())
			{
				BXIBlockSection rootSection = IBlockSectionId != 0 ? BXIBlockSection.GetById(IBlockSectionId) : null;
				if (rootSection != null && rootSection.IBlockId != IBlockId)
					rootSection = null;

				BXFilter sectionsFilter = new BXFilter();
				sectionsFilter.Add(new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId));
				if (Depth > 0)
				{
					int rootDepth = rootSection != null ? rootSection.DepthLevel : 1;
					sectionsFilter.Add(new BXFilterItem(BXIBlockSection.Fields.DepthLevel, BXSqlFilterOperators.LessOrEqual, rootDepth + Depth));
				}
				if (SectionActiveFilter == ActiveFilter.Active)
					sectionsFilter.Add(new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"));
				
				if (rootSection != null)
				{
					sectionsFilter.Add(new BXFilterItem(BXIBlockSection.Fields.LeftMargin, BXSqlFilterOperators.GreaterOrEqual, rootSection.LeftMargin));
					sectionsFilter.Add(new BXFilterItem(BXIBlockSection.Fields.RightMargin, BXSqlFilterOperators.LessOrEqual, rootSection.RightMargin));
				}

				bool isLegalPage;
				BXQueryParams queryParams = PreparePaging(
					pagingParams,
					delegate { return BXIBlockSection.Count(sectionsFilter); },
					new BXParamsBag<object>(),
					out isLegalPage
				);

				if (!Parameters.Get<bool>("PagingAllow") && SectionsCount > 0)
					queryParams = new BXQueryParams(new BXPagingOptions(0, SectionsCount));
				else if (!isLegalPage)
					AbortCache();
	            
				BXOrderBy sectionsOrderBy = new BXOrderBy();
				sectionsOrderBy.Add(BXIBlockSection.Fields, String.Format("{0} {1}", this.SortBy1, this.SortOrder1));
				if (!this.SortBy1.Equals(this.SortBy2, StringComparison.InvariantCultureIgnoreCase))
					sectionsOrderBy.Add(BXIBlockSection.Fields, String.Format("{0} {1}", this.SortBy2, this.SortOrder2));

				//Производим выборку секций и преобразуем ее во внутренний формат компонента
				List<Section> allSections = BXIBlockSection.GetList(
					sectionsFilter,
					sectionsOrderBy,
					new BXSelectAdd(BXIBlockSection.Fields.IBlock),
					queryParams
				).ConvertAll<Section>(delegate(BXIBlockSection input)
				{
					return new Section(input, this);
				});
				resultSections = new List<Section>(allSections);
				
				foreach (Section section in allSections)
				{
					//Создаем иерархию секций
					foreach (Section sub in allSections)
						if (sub.Data.SectionId == section.Data.Id)
						{
							section.Sections.Add(sub);
							sub.parent = section;
						}

					//Удаляем дочерние секции из результата, т.к. в результате должны остаться только секции верхнего уровня
					resultSections.RemoveAll(
						delegate(Section sub)
						{
							return sub.Data.SectionId == section.Data.Id;
						});

					//Наполняем секции элементами
					BXFilter elementsFilter = new BXFilter();
					elementsFilter.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId));
					elementsFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, section.Data.Id));
					if (ElementActiveFilter == ActiveFilter.Active)
						elementsFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"));
					else if (ElementActiveFilter == ActiveFilter.NotActive)
						elementsFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "N"));

					if (ElementActiveDateFilter == ActiveFilter.Active)
						elementsFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, "Y"));
					else if (ElementActiveDateFilter == ActiveFilter.NotActive)
						elementsFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, "N"));

					BXOrderBy elementsOrder = new BXOrderBy();
					elementsOrder.Add(BXIBlockElement.Fields.Sort, BXOrderByDirection.Asc);
					elementsOrder.Add(BXIBlockElement.Fields.Name, BXOrderByDirection.Asc);

					section.elements = 
						GetElements
						? BXIBlockElement.GetList(elementsFilter, elementsOrder).ConvertAll<Element>(delegate(BXIBlockElement item)
						{
							return new Element(item, section, this);
						})
						: new List<Element>();
				}

				SetTemplateCachedData();
				IncludeComponentTemplate();
			}
		}

		private void SetTemplateCachedData()
		{
			BXPublicPage bitrixPage = Page as BXPublicPage;
			if (bitrixPage != null && !IsComponentDesignMode && !string.IsNullOrEmpty(IBlockName))
			{
				bool setTitle = false;
				if ((Parameters.TryGetBool("SetTitle", out setTitle) || Parameters.TryGetBool("SetPageTitle", out setTitle)) && setTitle)
					bitrixPage.BXTitle = BXTextEncoder.DefaultEncoder.Decode(IBlockName);
			}
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Page.Title");
			Description = GetMessageRaw("Page.Description");
            Group = new BXComponentGroup("iblocks", GetMessage("Group"), 100, BXComponentGroup.Content);
			Icon = "/images/icon.gif";

			BXCategory dataSourceCategory = BXCategory.DataSource;

			ParamsDefinition.Add(BXParametersDefinition.Cache);

            ParamsDefinition.Add("IBlockType", new BXParamSingleSelection(GetMessageRaw("ParamText.IBlockType"), "", BXCategory.Main));
            ParamsDefinition.Add("IBlock", new BXParamSingleSelection(GetMessageRaw("ParamText.IBlock"), "", BXCategory.Main));
            ParamsDefinition.Add("IBlockSection", new BXParamSingleSelection(GetMessageRaw("ParamText.IBlockSection"), "", BXCategory.Main));
            ParamsDefinition.Add("Depth", new BXParamText(GetMessageRaw("ParamText.Depth"), "1", BXCategory.Main));
            ParamsDefinition.Add("GetElements", new BXParamYesNo(GetMessageRaw("ParamText.GetElements"), true, BXCategory.Main, new ParamClientSideActionGroupViewSwitch(ClientID, "ShowMenu", "GetElementsOn", String.Empty)));

            ParamsDefinition.Add("SectionUrlTemplate", new BXParamText(GetMessageRaw("ParamText.SectionUrlTemplate"), "?section=#SectionId#", BXCategory.UrlSettings));
            ParamsDefinition.Add("ElementUrlTemplate", new BXParamText(GetMessageRaw("ParamText.ElementUrlTemplate"), "?element=#ElementId#", BXCategory.UrlSettings));

            ParamsDefinition.Add(
            "PagingRecordsPerPage",
            new BXParamText(
                GetMessageRaw("ParamTitle.SectionsPerPage"),
                "10",
                BXCategory.Main
                )
            );

            //SetTitle
            ParamsDefinition.Add(
                "SetTitle",
                new BXParamYesNo(
                    GetMessageRaw("ParamTitle.SetPageTitle"),
                    false,
                    BXCategory.Main
                    )
                );

            //SortBy1
            ParamsDefinition.Add(
                "SortBy1",
                new BXParamSingleSelection(
                    GetMessageRaw("ParamTitle.FirstSortBy"),
                    "ActiveFromDate",
                    dataSourceCategory
                    )
                );

            //SortBy2
            ParamsDefinition.Add(
                "SortBy2",
                new BXParamSingleSelection(
                    GetMessageRaw("ParamTitle.SecondSortBy"),
                    "Sort",
                    dataSourceCategory
                    )
                );

            //SortOrder1
            ParamsDefinition.Add(
                "SortOrder1",
                new BXParamSort(
                    GetMessageRaw("ParamTitle.FirstSortOrder"),
                    dataSourceCategory
                    )
                );

            //SortOrder2
            ParamsDefinition.Add(
                "SortOrder2",
                new BXParamSort(
                    GetMessageRaw("ParamTitle.SecondSortOrder"),
                    dataSourceCategory
                    )
                );

			ParamsDefinition.Add("SectionActiveFilter", new BXParamSingleSelection(GetMessageRaw("ParamTitle.SectionActiveFilter"), "Active", dataSourceCategory));
			ParamsDefinition.Add("ActiveFilter", new BXParamSingleSelection(GetMessageRaw("ParamTitle.ActiveFilter"), "Active", dataSourceCategory, null, new ParamClientSideActionGroupViewMember(ClientID, "ActiveFilter", new string[] {"GetElementsOn"})));
			ParamsDefinition.Add("ActiveDateFilter", new BXParamSingleSelection(GetMessageRaw("ParamTitle.ActiveDateFilter"), "All", dataSourceCategory, null, new ParamClientSideActionGroupViewMember(ClientID, "ActiveDateFilter", new string[] {"GetElementsOn"})));


            BXParamsBag<BXParam> paging = BXParametersDefinition.Paging;
            paging.Remove("PagingRecordsPerPage");
            ParamsDefinition.Add(paging);
        }
		protected override void LoadComponentDefinition()
		{
			// Редактор свойств компонента должен обновить список свойств компонента при изменении типа инфоблока
			ParamsDefinition["IBlockType"].RefreshOnDirty = true;
			// Задаем список предопределенных типов инфоблоков
			ParamsDefinition["IBlockType"].Values = GetIBlockTypeParamValues();

			// Редактор свойств компонента должен обновить список свойств компонента при изменении инфоблока
			ParamsDefinition["IBlock"].RefreshOnDirty = true;
			// Задаем список предопределенных инфоблоков
			ParamsDefinition["IBlock"].Values = GetIBlockParamValues(Parameters.Get("IBlockType", 0));

			// Задаем список предопределенных секций инфоблока
			ParamsDefinition["IBlockSection"].Values = GetIBlockSectionParamValues(Parameters.Get("IBlock", 0));

            //SortBy1 SortBy2
            List<BXParamValue> sortFields = new List<BXParamValue>();
            sortFields.Add(new BXParamValue("ID", "ID"));
            sortFields.Add(new BXParamValue(GetMessageRaw("ParamText.SectionName"), "Name"));
            sortFields.Add(new BXParamValue(GetMessageRaw("ParamText.Sort"), "Sort"));
            sortFields.Add(new BXParamValue(GetMessageRaw("ParamText.DateOfLastModification"), "UpdateDate"));

            ParamsDefinition["SortBy1"].Values = sortFields;
            ParamsDefinition["SortBy2"].Values = sortFields;

			ParamsDefinition["SectionActiveFilter"].Values = new List<BXParamValue>();
			ParamsDefinition["SectionActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.Active"), ActiveFilter.Active.ToString()));
			ParamsDefinition["SectionActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.All"), ActiveFilter.All.ToString()));

			ParamsDefinition["ActiveFilter"].Values = new List<BXParamValue>();
			ParamsDefinition["ActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.Active"), ActiveFilter.Active.ToString()));
			ParamsDefinition["ActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.NotActive"), ActiveFilter.NotActive.ToString()));
			ParamsDefinition["ActiveFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.All"), ActiveFilter.All.ToString()));

			ParamsDefinition["ActiveDateFilter"].Values = new List<BXParamValue>();
			ParamsDefinition["ActiveDateFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.Active"), ActiveFilter.Active.ToString()));
			ParamsDefinition["ActiveDateFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.NotActive"), ActiveFilter.NotActive.ToString()));
			ParamsDefinition["ActiveDateFilter"].Values.Add(new BXParamValue(GetMessageRaw("ActiveFilter.All"), ActiveFilter.All.ToString()));			
		}

		//Создаем контекстное меню
		public override BXComponentPopupMenuInfo PopupMenuInfo
		{
			get
			{
				if(this.popupMenuInfo != null)
					return this.popupMenuInfo;

				BXComponentPopupMenuInfo info = base.PopupMenuInfo;

				if (BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) &&
					BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements))
				{
					info.CreateComponentContentMenuItems = delegate(BXShowMode showMode)
					{
						if (showMode == BXShowMode.View)
							return new BXHermitagePopupMenuBaseItem[0];

						BXHermitageToolbarSettings settings = new BXHermitageToolbarSettings();
						settings.Data["IBlockId"] = IBlockId;
						settings.Data["IBlockTypeId"] = IBlockTypeId;

						if ((iblock ?? (iblock = BXIBlock.GetById(IBlockId))) != null)
							settings.Data["IBlock"] = iblock;

						int sectionId = IBlockSectionId;
						if(sectionId > 0)
							settings.Data["SectionId"] = sectionId;
						//else if(resultSections != null && resultSections.Count > 0)
						//	settings.Data["SectionId"] = resultSections[0].Data.Id;

						string elementName = iblock != null && !string.IsNullOrEmpty(iblock.CaptionsInfo.ElementName) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(iblock.CaptionsInfo.ElementName.ToLower()) : GetMessageRaw("IBlockElementName"),
							sectionName = iblock != null && !string.IsNullOrEmpty(iblock.CaptionsInfo.SectionName) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(iblock.CaptionsInfo.SectionName.ToLower()) : GetMessageRaw("IBlockSectionName");

						settings.Data["AddElement"] = string.Format(GetMessageRaw("AddNewElement"), elementName);
						settings.Data["AddNewSection"] = string.Format(GetMessageRaw("AddNewSection"), sectionName);

						return BXHermitageToolbarHelper.CreateToolbar(BXHermitageMenuCommand.CreateElement | BXHermitageMenuCommand.CreateSection, settings).ItemsToArray();
					};
				}
				return info;
			}
		}

		private BXIBlock iblock  = null;
		public BXHermitageToolbar CreateElementContextToolbar(BXIBlockElement element, string containerClientID)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			if (!(BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) &&
				BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifyElements)))
				return new BXHermitageToolbar();

			BXHermitageToolbarSettings settings = new BXHermitageToolbarSettings();
			settings.RequireIncludeAreasFlag = true;
			settings.ParentClientID = containerClientID;
			settings.Data.Add("IBlockId", IBlockId);

			settings.Data.Add("IBlockTypeId", IBlockTypeId);

			if (element != null)
				settings.Data.Add("Element", element);

			if ((iblock ?? (iblock = BXIBlock.GetById(IBlockId))) != null)
				settings.Data.Add("IBlock", iblock);

			string elementName = iblock != null && !string.IsNullOrEmpty(iblock.CaptionsInfo.ElementName) ? BXHtmlTextEncoder.HtmlTextEncoder.Decode(iblock.CaptionsInfo.ElementName.ToLower()) : GetMessageRaw("EditElementMenuItem");

			settings.Data.Add("ChangeElement", string.Format(GetMessageRaw("EditElementMenuItem"), elementName));
			settings.Data.Add("DeleteElement", string.Format(GetMessageRaw("DeleteElementMenuItem"), elementName));
			settings.Data.Add("ElementDeletionConfirmation",
				string.Format(GetMessageRaw("ElementDeletionConfirmation"),
				elementName));
			settings.Data.Add("ElementDetailUrl", Parameters.Get<string>("ElementUrlTemplate"));

			return BXHermitageToolbarHelper.CreateToolbar(BXHermitageMenuCommand.ModifyElement | BXHermitageMenuCommand.DeleteElement, settings);
		}

		//NESTED CLASSES
		public class Section
		{
			//FIELDS
			BXIBlockSection data;
			List<Section> sections;
			IBlockHierarchyComponent component;
			string link;

			internal List<Element> elements;
			internal Section parent;

			//PROPERTIES
			
			//Родительская секция или null, если это секция верхнего уровня
			public Section Parent
			{
				get
				{
					return parent;
				}
			}
			
			//Сущность секции инфоблока
			public BXIBlockSection Data
			{
				get
				{
					return data;
				}
			}
			
			//Список элементов секции
			public List<Element> Elements
			{
				get
				{
					if (elements == null)
						elements = new List<Element>();
					return elements;
				}
			}
			
			//Список дочерних секций
			public List<Section> Sections
			{
				get
				{
					if (sections == null)
						sections = new List<Section>();
					return sections;
				}
			}

			//Ссылка на страницу секции (формируется на основе параметров компонента)
			public string Link
			{
				get
				{
					if (link == null)
					{
						BXParamsBag<object> parameters = new BXParamsBag<object>();
						parameters.Add("SectionId", Data.Id);
						parameters.Add("SectionCode", Data.Code);
						link = BXComponentManager.MakeLink(component.SectionUrlTemplate, parameters);
					}
					return link;
				}
			}

			//Ссылка на страницу секции, закодированная для вывода в HTML (формируется на основе параметров компонента)
			public string Href
			{
				get
				{
					return HttpUtility.HtmlEncode(Link);
				}
			}


			//CONSTRUCTOR
			internal Section(BXIBlockSection iblockSection, IBlockHierarchyComponent component)
			{
				this.data = iblockSection;
				this.component = component;
			}
		}
		public class Element
		{
			//FIELDS
			BXIBlockElement data;
			string link;
			Section parent;
			IBlockHierarchyComponent component;

			//PROPERTIES

			//Родительская секция
			public Section Parent
			{
				get
				{
					return parent;
				}
			}

			//Сущность элемента инфоблока
			public BXIBlockElement Data
			{
				get
				{
					return data;
				}
			}

			//Ссылка на страницу элемента (формируется на основе параметров компонента)
			public string Link
			{
				get
				{
					if (link == null)
					{
						BXParamsBag<object> parameters = new BXParamsBag<object>();
						if (parent != null)
						{
							parameters.Add("SectionId", parent.Data.Id);
							parameters.Add("SectionCode", parent.Data.Code);
						}
						parameters.Add("ElementId", Data.Id);
						parameters.Add("ElementCode", Data.Code);
						link = BXComponentManager.MakeLink(component.ElementUrlTemplate, parameters);
					}
					return link;
				}
			}

			//Ссылка на страницу элемента, закодированная для вывода в HTML (формируется на основе параметров компонента)
			public string Href
			{
				get
				{
					return HttpUtility.HtmlEncode(Link);
				}
			}

			//CONSTRUCTOR
			internal Element(BXIBlockElement iblockElement, Section parent, IBlockHierarchyComponent component)
			{
				this.data = iblockElement;
				this.parent = parent;
				this.component = component;
			}
		}
	}

	public class IBlockHierarchyTemplate : BXComponentTemplate<IBlockHierarchyComponent>
	{
		//METHODS
		IEnumerable<Action> GetActionsForSections(List<IBlockHierarchyComponent.Section> sections, bool collapseEmpty, int depth)
		{
			foreach (IBlockHierarchyComponent.Section section in sections)
			{
				//Если секция пустая и collapseEmpty == true, то сворачиваем "Начало секции" и "Конец секции" в одно действие
				if (collapseEmpty && section.Sections.Count == 0 && section.Elements.Count == 0)
				{
					yield return new Action(ActionType.SectionEmpty, section, null, depth);
					continue;
				}


				//Добавляем действие "Начало секции"
				yield return new Action(ActionType.SectionStart, section, null, depth);


				//Если список подсекций пуст и collapseEmpty == true, то сворачиваем "Начало списка подсекций" и "Конец списка подсекций" в одно действие
				if (collapseEmpty && section.Sections.Count == 0)
					yield return new Action(ActionType.SectionsEmpty, section, null, depth);
				else
				{
					//Добавляем действие "Начало списка подсекций"
					yield return new Action(ActionType.SectionsStart, section, null, depth);

					//Добавляем действия для подсекций
					if (section.Sections.Count != 0)
						foreach (Action a in GetActionsForSections(section.Sections, collapseEmpty, depth + 1))
							yield return a;

					//Добавляем действие "Конец списка подсекций"
					yield return new Action(ActionType.SectionsEnd, section, null, depth);
				}


				//Если список элементов пуст и collapseEmpty == true, то сворачиваем "Начало списка элементов" и "Конец списка элементов" в одно действие
				if (collapseEmpty && section.Elements.Count == 0)
					yield return new Action(ActionType.ElementsEmpty, section, null, depth);
				else
				{
					//Добавляем действие "Начало списка элементов"
					yield return new Action(ActionType.ElementsStart, section, null, depth);

					//Добавляем действие "Вывод элемента"
					if (section.Elements.Count != 0)
						foreach (IBlockHierarchyComponent.Element element in section.Elements)
							yield return new Action(ActionType.Element, section, element, depth);

					//Добавляем действие "Конец списка элементов"
					yield return new Action(ActionType.ElementsEnd, section, null, depth);
				}


				//Добавляем действие "Конец секции"
				yield return new Action(ActionType.SectionEnd, section, null, depth);
			}
		}
		public IEnumerable<Action> GetActions(bool collapseEmpty)
		{
			return GetActionsForSections(Component.Sections, collapseEmpty, 0);
		}

		protected string GetItemContainerClientID(int itemId)
		{
			return string.Concat(ClientID, ClientIDSeparator, "HierarchyItem", itemId.ToString());
		}

		public void RenderElementToolbar(BXIBlockElement element, string containerClientID)
		{
			Component.CreateElementContextToolbar(element, containerClientID).Render(CurrentWriter);
		}

		//NESTED TYPES
		public struct Action
		{
			//Тип действия
			public ActionType Type;

			//Секция, соответствующая действию
			public IBlockHierarchyComponent.Section Section;

			//Элемент, соотвтествующий действию
			public IBlockHierarchyComponent.Element Element;

			//Текущая глубина вложенности (начинается с 0)
			public int Depth;

			internal Action(ActionType type, IBlockHierarchyComponent.Section section, IBlockHierarchyComponent.Element element, int depth)
			{
				Type = type;
				Section = section;
				Element = element;
				Depth = depth;
			}
		}
		[Flags]
		public enum ActionType
		{
			//Нет действия
			None = 0,
			
			//Начало секции
			SectionStart = 1,
			
			//Конец секции
			SectionEnd = 2,

			//Пустая секция
			SectionEmpty = SectionStart | SectionEnd,
			
			//Начало списка подсекций
			SectionsStart = 4,

			//Конец списка подсекций
			SectionsEnd = 8,

			//Пустой список подсекций
			SectionsEmpty = SectionsStart | SectionsEnd,

			//Начало списка элементов
			ElementsStart = 16,

			//Конец списка элементов
			ElementsEnd = 32,

			//Пустой список элементов
			ElementsEmpty = ElementsStart | ElementsEnd,

			//Вывод элемента
			Element = 64
		}
	}
}