using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Services;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using System.Collections.Specialized;
using Bitrix.Security;
using System.Net;
using Bitrix.DataTypes;
using System.Collections.ObjectModel;
using System.Web.SessionState;

namespace Bitrix.IBlock.Components
{
	public partial class CatalogueCompareListComponent : BXComponent
	{
		private CatalogueCompareListComponentError error = CatalogueCompareListComponentError.None;
		public CatalogueCompareListComponentError ComponentError
		{
			get { return this.error; }
		}

		private static readonly string guidKey = "__BX_IBLOCK_COMPARE_LIST_GUID__";

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			bool cached = IsCached(BXPrincipal.Current.GetAllRoles(true), ((BXIdentity)BXPrincipal.Current.Identity).Id);
			if(cached && Session != null)
			{
				string guid = Session[guidKey] as string, 
					cacheGuid;

				Results.TryGetString(guidKey, out cacheGuid);

				if(string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(cacheGuid) || !string.Equals(guid, cacheGuid, StringComparison.Ordinal))
				{
					ClearCache();
					cached = false;
				}
			}

			ActionHandler.Process(IBlockId, this);

			if (cached)
				return;

			using (BXTagCachingScope cacheScope = BeginTagCaching())
			{
				BXIBlock iblock = null;
				if (IBlockId > 0)
				{
					BXIBlockCollection iblockCol = BXIBlock.GetList(
						new BXFilter(
							new BXFilterItem(BXIBlock.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
							new BXFilterItem(BXIBlock.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y"),
							new BXFilterItem(BXIBlock.Fields.ID, BXSqlFilterOperators.Equal, IBlockId)),
							null, null, null, BXTextEncoder.HtmlTextEncoder);
					iblock = iblockCol.Count > 0 ? iblockCol[0] : null;
				}

				if (iblock == null)
					this.error = CatalogueCompareListComponentError.IBlockIsNotFound;
				else
				{
					IBlockTypeId = iblock.TypeId;
					cacheScope.AddTag(ActionHandler.GetCacheTag(IBlockId));

					BXIBlockComparisonBlockSettings iblockData = ActionHandler.GetCurrentIBlockSettings(IBlockId, Session);
					if (iblockData != null)
					{
						BXOrderBy o = null;
						if (!string.IsNullOrEmpty(SortBy))
						{
							o = new BXOrderBy();
							if (SortBy.StartsWith("-", StringComparison.Ordinal))
							{
								string sortBy = SortBy.Substring(1).ToUpper();
								BXSchemeFieldBase f = BXIBlockElement.Fields.CustomFields[IBlockId].GetFieldByKey(sortBy);
								if (f != null)
									o.Add(f, SortOrder);
							}
							else
								o.Add(BXIBlockElement.Fields, SortBy, SortOrder);
						}
						BXIBlockElementCollection iblockElCol = BXIBlockElement.GetList(
							new BXFilter(
								new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
								new BXFilterItem(BXIBlockElement.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y"),
								new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.In, iblockData.IdList)),
							o);

						foreach (BXIBlockElement el in iblockElCol)
							InternalElementDataList.Add(new ElementData(this, el));

						if (string.IsNullOrEmpty(SortBy))
							InternalElementDataList.Sort(
								delegate(ElementData x, ElementData y)
								{
									return iblockData.IdList.IndexOf(x.Element.Id) - iblockData.IdList.IndexOf(y.Element.Id);
								});
					}
				}

				//Сохраняем GUID для валидации кеша 
                if (Session != null)
                {
                    Session[guidKey] = Results[guidKey] = Guid.NewGuid().ToString();
                }

				IncludeComponentTemplate();
			}
		}

		private int? iblockId = null;
		public int IBlockId
		{
			get { return (this.iblockId ?? (this.iblockId = Parameters.GetInt("IBlockId", 0))).Value; }
			set { Parameters["IBlockId"] = (this.iblockId = value).Value.ToString(); }
		}

		public int IBlockTypeId
		{
			get { return ComponentCache.GetInt("IBlockTypeID", 0); }
			set { ComponentCache["IBlockTypeID"] = value; }
		}

		private string sortBy = null;
		public string SortBy
		{
			get { return this.sortBy ?? (this.sortBy = Parameters.GetString("SortBy", string.Empty)); }
			set { Parameters["SortBy"] = this.sortBy = value ?? string.Empty; }
		}

		private string sortOrder = null;
		public string SortOrder
		{
			get { return this.sortOrder ?? (this.sortOrder = Parameters.GetString("SortOrder", "Desc")); }
			set { Parameters["SortOrder"] = this.sortOrder = value ?? string.Empty; }
		}

		public string ElementUrlTemplate
		{
			get { return Parameters.GetString("ElementUrlTemplate", "./view.aspx?id=#ElementId#"); }
			set { Parameters["ElementUrlTemplate"] = value ?? string.Empty; }
		}

		public string CompareResultUrlTemplate
		{
			get { return Parameters.GetString("CompareResultUrlTemplate", "./compare.aspx"); }
			set { Parameters["CompareResultUrlTemplate"] = value ?? string.Empty; }
		}

		public string CompareResultUrl()
		{
			return PrepareUrl(CompareResultUrlTemplate);
		}

		private List<ElementData> elementDataList = null;
		internal List<ElementData> InternalElementDataList
		{
			get { return this.elementDataList ?? (this.elementDataList = new List<ElementData>()); }
		}

		private ReadOnlyCollection<ElementData> elementDataListRO = null;
		public IList<ElementData> ElementDataList
		{
			get { return this.elementDataListRO ?? (this.elementDataListRO = new ReadOnlyCollection<ElementData>(InternalElementDataList)); }
		}

		private BXParamsBag<object> replaceItems;
		public BXParamsBag<object> ReplaceItems
		{
			get { return this.replaceItems ?? (this.replaceItems = new BXParamsBag<object>()); }
		}

		public string GetElementUrl(BXIBlockElement element)
		{
			if (element == null)
				return string.Empty;

			BXParamsBag<object> repl = ReplaceItems;
			repl.Clear();

			repl.Add("IblockId", element.IBlockId.ToString());
			repl.Add("BlockId", element.IBlockId.ToString());

			repl.Add("ElementId", element.Id.ToString());
			repl.Add("Id", element.Id.ToString());

			repl.Add("ElementCode", element.Code.ToString());
			repl.Add("Code", element.Code.ToString());

			int sectionId = 0;
			string sectionCode = string.Empty;

			BXIBlockSectionCollection sections = element.GetSections();
			if (sections != null && sections.Count != 0)
				foreach (BXIBlockSection s in sections)
				{
					if (!s.ActiveGlobal)
						continue;

					sectionId = s.Id;
					sectionCode = s.Code;
					break;
				}

			repl.Add("SectionId", sectionId);
			repl.Add("SectionCode", sectionCode);

			return PrepareUrl(MakeLink(ElementUrlTemplate, repl));
		}

		private string PrepareUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
				return string.Empty;

			if (url.StartsWith("http"))
				return url;

			int whatInd = url.IndexOf('?');
			string query = whatInd >= 0 ? url.Substring(whatInd) : string.Empty;
			string path = whatInd >= 0 ? url.Substring(0, whatInd) : url;
			if (string.IsNullOrEmpty(path))
				path = VirtualPathUtility.ToAbsolute(Request.AppRelativeCurrentExecutionFilePath);
			else if (!VirtualPathUtility.IsAbsolute(path))
				path = VirtualPathUtility.ToAbsolute(VirtualPathUtility.Combine(Request.AppRelativeCurrentExecutionFilePath, path));

			return string.Concat(path, query);
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/iblock_compare_list.gif";

			Group = new BXComponentGroup("catalogue", GetMessageRaw("Group"), 10, BXComponentGroup.Content);

			BXCategory mainCategory = BXCategory.Main,
				addSettingsCategory = BXCategory.AdditionalSettings,
				dataSourceCategory = BXCategory.DataSource;

			#region Main
			ParamsDefinition.Add(
				"IBlockTypeId",
				new BXParamSingleSelection(
					GetMessageRaw("Param.InfoBlockType"), string.Empty, mainCategory));

			ParamsDefinition.Add(
				"IBlockId",
				new BXParamSingleSelection(
					GetMessageRaw("Param.InfoBlockCode"), string.Empty, mainCategory));
			#endregion

			#region DataSourceCategory
			ParamsDefinition.Add(
				"SortBy",
				new BXParamSingleSelection(
					GetMessageRaw("Param.SortBy"), string.Empty, dataSourceCategory));

			ParamsDefinition.Add(
				"SortOrder",
				new BXParamSort(
					GetMessageRaw("Param.SortOrder"), true, dataSourceCategory));
			#endregion


			#region AdditionalSettings
			ParamsDefinition.Add(
				"ElementUrlTemplate",
				new BXParamText(
					GetMessageRaw("Param.ElementUrlTemplate"),
					"./view.aspx?id=#ElementId#",
					addSettingsCategory
				));

			ParamsDefinition.Add(
				"CompareResultUrlTemplate",
				new BXParamText(
					GetMessageRaw("Param.CompareResultUrlTemplate"),
					"./compare.aspx",
					addSettingsCategory
				));
			#endregion
		}

		protected override void LoadComponentDefinition()
		{
			#region IBlockType
			List<BXParamValue> typeValues = new List<BXParamValue>();
			typeValues.Add(new BXParamValue(GetMessageRaw("SelectIBlockType"), string.Empty));

			BXIBlockTypeCollection iblockTypes = BXIBlockType.GetList(
				null,
				new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Name, BXOrderByDirection.Asc)),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder);

			foreach (BXIBlockType iblockType in iblockTypes)
				typeValues.Add(new BXParamValue(iblockType.Translations[BXLoc.CurrentLocale].Name, iblockType.Id.ToString()));

			ParamsDefinition["IBlockTypeId"].Values = typeValues;
			ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;
			#endregion

			#region IBlock
			int selectedIBlockType = 0;
			if (Parameters.ContainsKey("IBlockTypeId"))
				int.TryParse(Parameters["IBlockTypeId"], out selectedIBlockType);

			BXFilter filter = new BXFilter();
			if (selectedIBlockType > 0)
				filter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, selectedIBlockType));
			if (!string.IsNullOrEmpty(DesignerSite))
				filter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

			List<BXParamValue> iblockValues = new List<BXParamValue>();
			iblockValues.Add(new BXParamValue(GetMessageRaw("SelectIBlockID"), string.Empty));
			BXIBlockCollection iblocks = BXIBlock.GetList(
				filter,
				new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Name, BXOrderByDirection.Asc)),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder);

			foreach (BXIBlock iblock in iblocks)
				iblockValues.Add(new BXParamValue(iblock.Name, iblock.Id.ToString()));

			ParamsDefinition["IBlockId"].Values = iblockValues;
			ParamsDefinition["IBlockId"].RefreshOnDirty = true;
			#endregion

			#region Sorting
			List<BXParamValue> sortingFields = new List<BXParamValue>();
			sortingFields.Add(new BXParamValue(GetMessageRaw("NotSelected"), string.Empty));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.Sort"), "Sort"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.UpdateDate"), "UpdateDate"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.ElementName"), "Name"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.ElementID"), "ID"));

			ParamsDefinition["SortBy"].Values = sortingFields;
			#endregion
		}

		//internal class ComparisonIBlockSettings
		//{
		//    public ComparisonIBlockSettings()
		//    {
		//    }

		//    public ComparisonIBlockSettings(int iblockId)
		//    {
		//        this.iblockId = iblockId;
		//    }

		//    private int iblockId = 0;
		//    public int IBlockId
		//    {
		//        get { return this.iblockId; }
		//        set { this.iblockId = value; }
		//    }

		//    private List<int> idList = null;
		//    public IList<int> IdList
		//    {
		//        get { return this.idList ?? (this.idList = new List<int>()); }
		//    }

		//    public void CopyTo(ComparisonIBlockSettings inst)
		//    {
		//        if (inst == null)
		//            throw new ArgumentNullException("inst");

		//        inst.IBlockId = IBlockId;

		//        if (inst.IdList.Count > 0)
		//            inst.IdList.Clear();

		//        foreach (int id in IdList)
		//            inst.IdList.Add(id);
		//    }

		//    public ComparisonIBlockSettings Clone()
		//    {
		//        ComparisonIBlockSettings r = new ComparisonIBlockSettings();
		//        CopyTo(r);
		//        return r;
		//    }
		//}
		//internal class ComparisonSettings
		//{
		//    private List<ComparisonIBlockSettings> iblockDataList = null;
		//    public IList<ComparisonIBlockSettings> IBlockDataList
		//    {
		//        get { return this.iblockDataList ?? (this.iblockDataList = new List<ComparisonIBlockSettings>()); }
		//    }

		//    public ComparisonIBlockSettings FindIBlockData(int iblockId)
		//    {
		//        if (iblockId <= 0 || this.iblockDataList == null)
		//            return null;

		//        for (int i = 0; i < this.iblockDataList.Count; i++)
		//            if (this.iblockDataList[i].IBlockId == iblockId)
		//                return this.iblockDataList[i];

		//        return null;
		//    }

		//    public void CopyTo(ComparisonSettings inst)
		//    {
		//        if (inst == null)
		//            throw new ArgumentNullException("inst");

		//        if (inst.IBlockDataList.Count > 0)
		//            inst.IBlockDataList.Clear();

		//        foreach (ComparisonIBlockSettings iblockData in IBlockDataList)
		//            inst.IBlockDataList.Add(iblockData.Clone());
		//    }



		//    public ComparisonSettings Clone()
		//    {
		//        ComparisonSettings r = new ComparisonSettings();
		//        CopyTo(r);
		//        return r;
		//    }
		//}
		internal enum CatalogueCompareResultResponseType
		{
			Html = 1,
			Json
		}
		internal static class ActionHandler
		{
			private static object sync = new object();
			public static BXIBlockComparisonSettings GetCurrentSettings(HttpSessionState session)
			{
				if (session == null)
					throw new ArgumentNullException("session");

				lock (sync)
				{
					BXIBlockComparisonSettings settings = session[BXIBlockComparisonHelper.SettingsKey] as BXIBlockComparisonSettings;
					if (settings != null)
						return settings.Clone();
				}
				return null;
			}

			public static string GetDeleteElementUrl(int id, CatalogueCompareResultResponseType responseType)
			{
				return BXIBlockComparisonHelper.GetDeleteElementUrl(id, null, responseType == CatalogueCompareResultResponseType.Json);
			}

			public static string GetDeleteAllElemenstUrl(CatalogueCompareResultResponseType responseType)
			{
				return BXIBlockComparisonHelper.GetDeleteAllElemenstUrl(null, responseType == CatalogueCompareResultResponseType.Json);
			}

			public static BXIBlockComparisonBlockSettings GetCurrentIBlockSettings(int iblockId, HttpSessionState session)
			{
				return BXIBlockComparisonHelper.GetCurrentIBlockSettings(iblockId, session);
			}

			public static void Process(int iblockId, CatalogueCompareListComponent component)
			{
				if (iblockId <= 0 || component == null)
					return;

				HttpRequest request = component.Request;
				HttpResponse response = component.Response;
				HttpSessionState session = component.Session;

				if (request == null || response == null || session == null)
					return;

				if (!BXCsrfToken.CheckTokenFromRequest(request.QueryString))
					return;

				CatalogueCompareResultResponseType responseType =
					string.Equals(request.QueryString[BXIBlockComparisonHelper.DefaultResponseTypeParamName], "JSON", StringComparison.OrdinalIgnoreCase) ?
						CatalogueCompareResultResponseType.Json : CatalogueCompareResultResponseType.Html;

				string act = request.QueryString[BXIBlockComparisonHelper.DefaultActionParamName];
				if (string.IsNullOrEmpty(act))
					return;

				act = act.ToUpperInvariant();

				string responseStr = string.Empty;
				if (string.Equals(BXIBlockComparisonHelper.AddActionName, act, StringComparison.Ordinal))
				{
					int count = 0;
					int id = GetElementId(request);
					if (id > 0)
						lock (sync)
						{
							BXIBlockComparisonSettings settings = session[BXIBlockComparisonHelper.SettingsKey] as BXIBlockComparisonSettings;
							if (settings == null)
								session[BXIBlockComparisonHelper.SettingsKey] = settings = new BXIBlockComparisonSettings();

							BXIBlockComparisonBlockSettings iblockData = settings.FindIBlockData(iblockId);
							if (iblockData == null)
								settings.IBlockDataList.Add(iblockData = new BXIBlockComparisonBlockSettings(iblockId));

							if (iblockData.IdList.IndexOf(id) < 0)
								iblockData.IdList.Add(id);

							count = iblockData.IdList.Count;
						}
					if (responseType == CatalogueCompareResultResponseType.Json)
						responseStr = string.Format("{{blockId:{0}, elementId:{1}, action:'{2}', result:{{totalCount:{3}}}}}", iblockId.ToString(), id.ToString(), BXIBlockComparisonHelper.AddActionName, count.ToString());
				}
				else if (string.Equals(BXIBlockComparisonHelper.RemoveActionName, act, StringComparison.Ordinal))
				{
					int count = 0;
					int id = GetElementId(request);
					if (id > 0)
						lock (sync)
						{
							BXIBlockComparisonSettings settings = session[BXIBlockComparisonHelper.SettingsKey] as BXIBlockComparisonSettings;
							if (settings != null)
							{
								BXIBlockComparisonBlockSettings iblockData = settings.FindIBlockData(iblockId);
								if (iblockData != null)
								{
									int index;
									if ((index = iblockData.IdList.IndexOf(id)) >= 0)
										iblockData.IdList.RemoveAt(index);

									count = iblockData.IdList.Count;
								}
							}
						}
					if (responseType == CatalogueCompareResultResponseType.Json)
						responseStr = string.Format("{{blockId:{0}, elementId:{1}, action:'{2}', result:{{totalCount:{3}}}}}", iblockId.ToString(), id.ToString(), BXIBlockComparisonHelper.RemoveActionName, count.ToString());
				}
				else if (string.Equals(BXIBlockComparisonHelper.RemoveAllActionName, act, StringComparison.Ordinal))
				{
					int count = 0;
					lock (sync)
					{
						BXIBlockComparisonSettings settings = session[BXIBlockComparisonHelper.SettingsKey] as BXIBlockComparisonSettings;
						if (settings != null)
						{
							BXIBlockComparisonBlockSettings iblockData = settings.FindIBlockData(iblockId);
							if (iblockData != null)
								iblockData.IdList.Clear();
						}
					}
					if (responseType == CatalogueCompareResultResponseType.Json)
						responseStr = string.Format("{{blockId:{0}, action:'{1}', result:{{totalCount:{2}}}}}", iblockId.ToString(), BXIBlockComparisonHelper.RemoveAllActionName, count.ToString());
				}
				else
					return; //пропустить это действие

				BXComponent.ClearCacheByTags(new string[] { GetCacheTag(iblockId) });
				//component.ClearCache();

				if (responseType == CatalogueCompareResultResponseType.Json)
				{
					//response.ContentType = "text/x-json";
					response.ContentType = "text/plain"; //метод jQuery.get() - не поймёт ответ с типом 'text/x-json'
					response.StatusCode = (int)HttpStatusCode.OK;
					response.Write(responseStr);
				}
				else
				{
					Uri uri = BXSefUrlManager.CurrentUrl;
					NameValueCollection qsParams = HttpUtility.ParseQueryString(uri.Query);
					qsParams.Remove(BXCsrfToken.TokenKey);
					qsParams.Remove(BXIBlockComparisonHelper.DefaultActionParamName);
					qsParams.Remove(BXIBlockComparisonHelper.DefaultResponseTypeParamName);
					qsParams.Remove(BXIBlockComparisonHelper.DefaultElementIdParamName);

					response.Redirect(qsParams.Count > 0 ? string.Concat(uri.AbsolutePath, "?", qsParams.ToString()) : uri.AbsolutePath, false);
				}

				if (HttpContext.Current != null)
					HttpContext.Current.ApplicationInstance.CompleteRequest();
				response.End();
			}

			private static int GetElementId(HttpRequest request)
			{
				int r;
				string s = request.QueryString[BXIBlockComparisonHelper.DefaultElementIdParamName];
				return !string.IsNullOrEmpty(s) && int.TryParse(s, out r) && r > 0 ? r : 0;
			}

			public static string GetCacheTag(int iblockId)
			{
				return string.Concat("iblockComparison:iblockId=", iblockId.ToString(), ",userId=", ((BXIdentity)BXPrincipal.Current.Identity).Id.ToString());
			}
		}

		public class ElementData
		{
			private CatalogueCompareListComponent component = null;
			private BXIBlockElement element = null;
			internal BXIBlockElement Element
			{
				get { return this.element; }
			}

			public ElementData(CatalogueCompareListComponent component, BXIBlockElement element)
			{
				if (component == null)
					throw new ArgumentNullException("component");

				if (element == null)
					throw new ArgumentNullException("element");

				this.component = component;
				this.element = element;
			}

			public string ID
			{
				get { return this.element.Id.ToString(); }
			}

			/// <summary>
			/// Имя (HTML-кодирование)
			/// </summary>
			public string Name
			{
				get { return this.element.Name; }
			}

			private string url = null;
			/// <summary>
			/// URL эл-та
			/// </summary>
			public string DetailUrl
			{
				get { return this.url ?? (this.url = this.component.GetElementUrl(this.element)); }
			}

			public string GetDeleteUrl(bool jsonResponse)
			{
				return ActionHandler.GetDeleteElementUrl(this.element.Id, jsonResponse ? CatalogueCompareResultResponseType.Json : CatalogueCompareResultResponseType.Html);
			}
		}
	}

	public partial class CatalogueCompareListTemplate : BXComponentTemplate<CatalogueCompareListComponent>
	{
		protected override void Render(HtmlTextWriter writer)
		{
			if (IsComponentDesignMode && (Component.IBlockTypeId <= 0 || Component.IBlockId <= 0))
				writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
			else
				base.Render(writer);
		}
	}

	[FlagsAttribute]
	public enum CatalogueCompareListComponentError
	{
		None = 0x0,
		IBlockIsNotFound = 0x1
	}
}
