using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Bitrix;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.DataTypes;

public partial class NewsLine : BXComponent
{
	//FIELDS
	private List<NewsLineItem> items;

	//PROPERTIES
	public int NewsCount
	{
		get
		{
			return Parameters.Get<int>("NewsCount");
		}
	}

	public int IBlockId
	{
		get { return Parameters.Get<int>("IBlockId"); }
	}

	public int[] IBlockIds
	{
		get
		{
			List<int> result = new List<int>();
			result = Parameters.GetListInt("IBlockIds");

			if (IBlockId > 0)
				result.Add(IBlockId);

			return result.ToArray();

		}
	}

	public string SortBy1
	{
		get
		{
			return Parameters["SortBy1"];
		}
	}

	public string SortBy2
	{
		get
		{
			return Parameters["SortBy2"];
		}
	}

	public string SortOrder1
	{
		get
		{
			return Parameters["SortOrder1"];
		}
	}

	public string SortOrder2
	{
		get
		{
			return Parameters["SortOrder2"];
		}
	}

	private string[] roles = null;
	public string[] Roles
	{
		get
		{
			return roles ?? (roles = ((BXPrincipal)Page.User).GetAllRoles(true));
		}
	}

	//PROPERTIES:RESULTS
	public List<NewsLineItem> Items
	{
		get
		{
			return items;
		}
	}


	//METHODS
	protected void Page_Load(object sender, EventArgs e)
	{
		if (IsCached(Roles))
			return;

		using (BXTagCachingScope cacheScope = BeginTagCaching())
		{
			if (IBlockIds.Length > 0)
			{
				BXFilter elementFilter = new BXFilter(
					new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.In, IBlockIds),
					new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
					new BXFilterItem(BXIBlockElement.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y")
				);

				BXOrderBy elementOrderBy = new BXOrderBy();
				elementOrderBy.Add(BXIBlockElement.Fields, String.Format("{0} {1}", SortBy1, SortOrder1));
				elementOrderBy.Add(BXIBlockElement.Fields, String.Format("{0} {1}", SortBy2, SortOrder2));
				if (!"ID".Equals(SortBy1, StringComparison.InvariantCultureIgnoreCase)
					&& !"ID".Equals(SortBy2, StringComparison.InvariantCultureIgnoreCase))
				{
					elementOrderBy.Add(BXIBlockElement.Fields.ID, BXOrderByDirection.Desc);
				}

				items = null;
				List<NewsLineItem> results = new List<NewsLineItem>();
				BXIBlockElementCollection elementCollection = BXIBlockElement.GetList(
					elementFilter,
					elementOrderBy
				);
				int cnt = 0;
				foreach (BXIBlockElement element in elementCollection)
				{
					NewsLineItem newItem = new NewsLineItem();
					newItem.ElementId = element.Id;
					newItem.Code = element.Code;
					newItem.IBlockId = element.IBlockId;
					newItem.Name = element.Name;

					string displayDate;
					if (element.ActiveFromDate != DateTime.MinValue)
						displayDate = element.ActiveFromDate.ToString(Parameters["ActiveDateFormat"]);
					else
						displayDate = "";
					newItem.DisplayDate = displayDate;

					string detailUrl = null;
					if (!String.IsNullOrEmpty(Parameters["DetailUrl"]))
					{
						string str = Parameters["DetailUrl"].Replace("#SITE_DIR#", BXSite.Current.Directory);
						BXParamsBag<object> replaceItems = new BXParamsBag<object>();
						replaceItems.Add("IBLOCK_ID", element.IBlockId);
						replaceItems.Add("ELEMENT_ID", element.Id);
						replaceItems.Add("ElementId", element.Id);
						replaceItems.Add("Code", element.Code);
						replaceItems.Add("SectionId", (element.Sections.Count > 0) ? element.Sections[0].SectionId.ToString() : String.Empty);
						replaceItems.Add("SECTION_ID", replaceItems["SectionId"]);
						str = MakeLink(str, replaceItems);
						detailUrl = str;
					}
					newItem.DetailUrl = detailUrl;

					results.Add(newItem);

					cnt++;
					if (cnt > NewsCount)
						break;
				}
				if (results.Count > 0)
					items = results;

			}

			IncludeComponentTemplate();
		}
	}

	protected override void PreLoadComponentDefinition()
	{
		Title = GetMessage("Title");
		Description = GetMessage("Description");
		Icon = "images/news_line.gif";

		Group = new BXComponentGroup("news", GetMessage("Group"), 20, BXComponentGroup.Content);

		ParamsDefinition.Add(BXParametersDefinition.Cache);

		//IBlockTypeId
		ParamsDefinition.Add(
			"IBlockTypeId",
			new BXParamSingleSelection(
				GetMessageRaw("InfoBlockType"),
				String.Empty,
				BXCategory.Main
			)
		);
		ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;

		//IBlockId
		ParamsDefinition.Add(
			"IBlockIds",
			new BXParamMultiSelection(
				GetMessageRaw("InfoBlockCode"),
				String.Empty,
				BXCategory.Main
			)
		);
		ParamsDefinition["IBlockIds"].RefreshOnDirty = true;

		//NewsCount
		ParamsDefinition.Add(
			"NewsCount",
			new BXParamText(
				GetMessageRaw("NewsPerPage"),
				"20",
				BXCategory.Main
			)
		);

		//SortBy1
		ParamsDefinition.Add(
			"SortBy1",
			new BXParamSingleSelection(
				GetMessageRaw("FirstSortBy"),
				"ActiveFromDate",
				BXCategory.DataSource
			)
		);

		//SortBy2
		ParamsDefinition.Add(
			"SortBy2",
			new BXParamSingleSelection(
				GetMessageRaw("SecondSortBy"),
				"Sort",
				BXCategory.DataSource
			)
		);

		//SortOrder1
		ParamsDefinition.Add(
			"SortOrder1",
			new BXParamSort(
				GetMessageRaw("FirstSortOrder"),
				BXCategory.DataSource
			)
		);

		//SortOrder2
		ParamsDefinition.Add(
			"SortOrder2",
			new BXParamSort(
				GetMessageRaw("SecondsortOrder"),
				BXCategory.DataSource
			)
		);

		//DetailUrl
		ParamsDefinition.Add(
			"DetailUrl",
			new BXParamText(
				GetMessageRaw("DetailPageUrl"),
				"NewsDetail.aspx?id=#ELEMENT_ID#",
				BXCategory.Sef
			)
		);

		//ActiveDateFormat
		ParamsDefinition.Add(
			"ActiveDateFormat",
			new BXParamSingleSelection(
				GetMessageRaw("DateDisplayFormat"),
				"dd.MM.yyyy",
				BXCategory.ListSettings
			)
		);
	}

	protected override void LoadComponentDefinition()
	{

		//IBlockTypeId
		List<BXParamValue> types = new List<BXParamValue>();
		types.Add(new BXParamValue("-", ""));
		BXIBlockTypeCollection typeCollection = BXIBlockType.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Sort, BXOrderByDirection.Asc)));
		foreach (BXIBlockType t in typeCollection)
			types.Add(new BXParamValue(t.Translations[BXLoc.CurrentLocale].Name, t.Id.ToString()));

		ParamsDefinition["IBlockTypeId"].Values = types;

		//IBlockId
		BXFilter iblockFilter = new BXFilter();
		if (Parameters.ContainsKey("IBlockTypeId"))
		{
			int typeId;
			int.TryParse(Parameters["IBlockTypeId"], out typeId);
			if (typeId > 0)
				iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, typeId));
		}
		if (!string.IsNullOrEmpty(DesignerSite))
			iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

		List<BXParamValue> iblocks = new List<BXParamValue>();
		BXIBlockCollection iblockCollection = BXIBlock.GetList(iblockFilter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Sort, BXOrderByDirection.Asc)));
		foreach (BXIBlock b in iblockCollection)
			iblocks.Add(new BXParamValue(b.Name, b.Id.ToString()));

		ParamsDefinition["IBlockIds"].Values = iblocks;


		//SortBy1 SortBy2
		List<BXParamValue> sortFields = new List<BXParamValue>();
		sortFields.Add(new BXParamValue("ID", "ID"));
		sortFields.Add(new BXParamValue(GetMessageRaw("Title"), "Name"));
		sortFields.Add(new BXParamValue(GetMessageRaw("ActiveFromDate"), "ActiveFromDate"));
		sortFields.Add(new BXParamValue(GetMessageRaw("Sort"), "Sort"));
		sortFields.Add(new BXParamValue(GetMessageRaw("DateOfLastModification"), "UpdateDate"));

		ParamsDefinition["SortBy1"].Values = sortFields;
		ParamsDefinition["SortBy2"].Values = sortFields;


		List<BXParamValue> dateFormats = new List<BXParamValue>();
		DateTime now = DateTime.Now;
		dateFormats.Add(new BXParamValue(now.ToString("dd-MM-yyyy"), "dd-MM-yyyy"));
		dateFormats.Add(new BXParamValue(now.ToString("MM-dd-yyyy"), "MM-dd-yyyy"));
		dateFormats.Add(new BXParamValue(now.ToString("yyyy-MM-dd"), "yyyy-MM-dd"));
		dateFormats.Add(new BXParamValue(now.ToString("dd.MM.yyyy"), "dd.MM.yyyy"));
		dateFormats.Add(new BXParamValue(now.ToString("MM.dd.yyyy"), "MM.dd.yyyy"));
		dateFormats.Add(new BXParamValue(now.ToString("dd/MM/yyyy"), "dd/MM/yyyy"));
		dateFormats.Add(new BXParamValue(now.ToString("dd.MM.yyyy HH:mm"), "dd.MM.yyyy HH:mm"));
		dateFormats.Add(new BXParamValue(now.ToString("D"), "D"));
		dateFormats.Add(new BXParamValue(now.ToString("f"), "f"));

		ParamsDefinition["ActiveDateFormat"].Values = dateFormats;

		base.LoadComponentDefinition();
	}

	//NESTED TYPES

}

public class NewsLineItem
{
	//FIELDS
	private int elementId;
	private string code;
	private int iBlockId;
	private string name;
	private string displayDate;
	private string detailUrl;

	//PROPERTIES
	public int ElementId
	{
		get { return elementId; }
		set { elementId = value; }
	}

	public string Code
	{
		get { return code; }
		set { code = value; }
	}

	public int IBlockId
	{
		get { return iBlockId; }
		set { iBlockId = value; }
	}

	public string Name
	{
		get { return name; }
		set { name = value; }
	}

	public string DisplayDate
	{
		get { return displayDate; }
		set { displayDate = value; }
	}

	public string DetailUrl
	{
		get { return detailUrl; }
		set { detailUrl = value; }
	}
}

public partial class NewsLineTemplate : BXComponentTemplate<NewsLine>
{
	protected override void Render(HtmlTextWriter writer)
	{
		StartWidth = "100%";
		if (IsComponentDesignMode && Component.Items == null)
			writer.Write(HttpUtility.HtmlEncode(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent")));
		else
			base.Render(writer);
	}
}