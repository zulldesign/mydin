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
using Bitrix.IBlock;
using System.Collections.Generic;
using Bitrix.DataLayer;
using Bitrix;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.Configuration;

public partial class bitrix_admin_IBlockAdmin : Bitrix.UI.BXAdminPage
{
	protected int typeId = -1;
	bool hasSections;
	string urlSectionAdminPage;
	string urlElementAdminPage;

	protected void Page_Init(object sender, EventArgs e)
	{
		//if (!this.BXUser.IsCanOperate("IBlockView"))
		//    BXAuthentication.AuthenticationRequired();

		if (BXModuleManager.IsModuleInstalled("Search"))
		{
			BXDropDownFilter indexedFilter = new BXDropDownFilter();
			indexedFilter.Key = "IndexContent";
			indexedFilter.Text = GetMessageRaw("FilterText.IsIndexed");
			indexedFilter.Values.Add(new ListItem(GetMessageRaw("Kernel.Any"), string.Empty));
			indexedFilter.Values.Add(new ListItem(GetMessageRaw("Kernel.Yes"), "Y"));
			indexedFilter.Values.Add(new ListItem(GetMessageRaw("Kernel.No"), "N"));
			BXAdminFilter1.Items.Add(indexedFilter);
		}
		InitPage();
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		successMessage.Visible = false;

		if (typeId > 0)
			hfTypeId.Value = typeId.ToString();

		BXInfoBlockTypeOld type = BXInfoBlockTypeManagerOld.GetById(typeId);
		if (type != null)
		{
			hasSections = type.HaveSections;
			PopupPanel1.Commands[0].ItemText =  GetMessage("PopupText.Chapters");
			PopupPanel1.Commands[1].ItemText =  GetMessage("Elements");

			Page.Title = String.Format(GetMessage("FormattedPageTitle.InformationBlocks"), type.TypeLang.ContainsKey(BXLoc.CurrentLocale) ? type.TypeLang[BXLoc.CurrentLocale].Name : GetMessage("Type"));
			((BXAdminMasterPage)Page.Master).Title = Page.Title;
		}
		else
		{
			Response.Redirect("IBlockTypeList.aspx");
		}
	}

	private void InitPage()
	{
		filterSite.Values.Add(new ListItem(GetMessageRaw("All"), ""));
		BXSiteCollection siteColl = BXSite.GetList(null, null);
		foreach (BXSite site in siteColl)
			filterSite.Values.Add(new ListItem("[" + HttpUtility.HtmlEncode(site.Id) + "] " + HttpUtility.HtmlEncode(site.Name), HttpUtility.HtmlEncode(site.Id)));

		typeId = this.GetRequestInt("type_id");
		if (typeId <= 0)
			Int32.TryParse(Request.Form[hfTypeId.UniqueID], out typeId);

		urlSectionAdminPage = ("Y".Equals(BXOptionManager.GetOptionString("iblock", "combined_list_mode", "Y"), StringComparison.InvariantCultureIgnoreCase) ? "IBlockListAdmin.aspx" : "IBlockSectionList.aspx");
		urlElementAdminPage = ("Y".Equals(BXOptionManager.GetOptionString("iblock", "combined_list_mode", "Y"), StringComparison.InvariantCultureIgnoreCase) ? "IBlockListAdmin.aspx" : "IBlockElementList.aspx");
		AddButton.Visible = BXUser.IsCanOperate(BXIBlock.Operations.IBlockManageAdmin);
	}
	BXFormFilter MakeCurrentFilter()
	{
		BXFormFilter filter = new BXFormFilter(BXAdminFilter1.CurrentFilter);
		filter.Add(new BXFormFilterItem("TypeId", typeId, BXSqlFilterOperators.Equal));
		filter.Add(new BXFormFilterItem("CheckPermissions", new string[] { "IBlockView" }, BXSqlFilterOperators.Equal));
		return filter;
	}

	protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		//У всех выбранных элементов уже есть право IBlockView
		
		BXGridView grid = (BXGridView)sender;
		BXGridViewRow row = (BXGridViewRow)e.Row;
		DataRowView drv = (DataRowView)row.DataItem;
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;
		List<string> allowed = new List<string>();

		row.UserData["TypeId"] = typeId;
		row.UserData["IBlockId"] = drv["IBlockId"];
		row.UserData["SectionPage"] = urlSectionAdminPage;
		row.UserData["ElementPage"] = urlElementAdminPage;

		if (hasSections)
			allowed.Add("sections");
		
		allowed.Add("elements");
		allowed.Add("");

		if (BXIBlock.IsUserCanOperate((int)drv["IBlockId"], BXIBlock.Operations.IBlockManageAdmin))
		{
			allowed.Add("edit");
			allowed.Add("");
			allowed.Add("delete");
		}
		else
			allowed.Add("view");

		row.AllowedCommandsList = allowed.ToArray();
	}
	protected void GridView1_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
	{
		int startRowIndex = 0;
		int maximumRows = 0;
		if (e.PagingOptions != null)
		{
			startRowIndex = e.PagingOptions.startRowIndex;
			maximumRows = e.PagingOptions.maximumRows;
		}

		BXOrderBy_old sortOrder = BXDatabaseHelper.ConvertOrderBy(e.SortExpression);
		BXInfoBlockCollectionOld iblockCollection = BXInfoBlockManagerOld.GetList(MakeCurrentFilter(), sortOrder, (maximumRows > 0 ? startRowIndex + maximumRows : 0));
		e.Data = new DataView(FillTable(iblockCollection, startRowIndex, maximumRows));
	}
	private DataTable FillTable(BXInfoBlockCollectionOld iblockCollection, int startRowIndex, int maximumRows)
	{
		if (iblockCollection == null)
			iblockCollection = new BXInfoBlockCollectionOld();

		DataTable result = new DataTable();

		result.Columns.Add("num", typeof(int));
		result.Columns.Add("IBlockId", typeof(int));
		result.Columns.Add("Name", typeof(string));
		result.Columns.Add("Sort", typeof(int));
		result.Columns.Add("Active", typeof(string));
		result.Columns.Add("Code", typeof(string));
		result.Columns.Add("ElementsCount", typeof(int));
		result.Columns.Add("SectionsCount", typeof(int));
		result.Columns.Add("Sites", typeof(string));
		result.Columns.Add("IndexContent", typeof(string));
		result.Columns.Add("UpdateDate", typeof(DateTime));

		int ind = -1;
		foreach (BXInfoBlockOld t in iblockCollection)
		{
			ind++;
			if (startRowIndex > ind)
				continue;

			DataRow r = result.NewRow();
			r["num"] = ind;
			r["IBlockId"] = t.IBlockId;
			r["Name"] = t.NameRaw;
			r["Sort"] = t.Sort;
			r["Active"] = (t.Active ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No"));
			r["Code"] = t.CodeRaw;
			r["ElementsCount"] = t.ElementsCount;
			r["SectionsCount"] = t.SectionsCount;
			r["Sites"] = "";
			r["IndexContent"] = (t.IndexContent ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No"));
			if (t.Sites.Count > 0)
			{
				r["Sites"] = t.Sites[0];
				for (int i = 1; i < t.Sites.Count; i++)
					r["Sites"] += ", " + t.Sites[i];
			}
			r["UpdateDate"] = t.UpdateDate;
			result.Rows.Add(r);

			if (maximumRows > 0 && ind == startRowIndex + maximumRows - 1)
				break;
		}

		return result;
	}
	protected void GridView1_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BXInfoBlockManagerOld.Count(MakeCurrentFilter());
	}
	protected void GridView1_Delete(object sender, BXDeleteEventArgs e)
	{
		BXGridView grid = (BXGridView)sender;
		try
		{
			BXInfoBlockCollectionOld elements;
			if (e.Keys != null) //Delete one element
			{
				elements = new BXInfoBlockCollectionOld();
				elements.Add(BXInfoBlockManagerOld.GetById((int)e.Keys["IBlockId"]));
			}
			else //All elements
			{
				elements = BXInfoBlockManagerOld.GetList(MakeCurrentFilter(), null);
			}

			foreach (BXInfoBlockOld element in elements)
			{
				if (element == null)
					throw new PublicException(GetMessageRaw("ExceptionText.BlockIsNotFound"));
				if (!BXIBlock.IsUserCanOperate(element.IBlockId, BXIBlock.Operations.IBlockManageAdmin))
					throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteThisRecord"));

				BXIBlock.Delete(element.IBlockId);

				e.DeletedCount++;
			}
			successMessage.Visible = true;
		}
		catch (Exception ex)
		{
			ProcessException(ex, errorMessage.AddErrorText);
		}
		grid.MarkAsChanged();
	}
}
