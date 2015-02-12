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
using Bitrix.IBlock;
using Bitrix.DataLayer;
using System.Collections.Generic;
using System.Text;
using Bitrix.Security;
using Bitrix.Services;
using System.Text.RegularExpressions;
using Bitrix.Services.Text;

public partial class bitrix_admin_IBlockElementSearch : BXAdminPage
{
	int typeId = -1;
	int iblockId = -1;

	BXInfoBlockTypeOld type;
	BXInfoBlockOld iblock;

	protected void Page_Init(object sender, EventArgs e)
	{
		InitPage();

		string fieldId = base.GetRequestString("n");
		if (String.IsNullOrEmpty(fieldId))
			fieldId = hfFieldId.Value;
		fieldId = Regex.Replace(fieldId, @"[^\w\.@$_-]", ""); 
		hfFieldId.Value = fieldId;

		string fieldName = base.GetRequestString("k");
		if (String.IsNullOrEmpty(fieldName))
			fieldName = hfFieldName.Value;
		fieldName = Regex.Replace(fieldName, @"[^\w\.@$_-]", "");
		hfFieldName.Value = fieldName;

		string callback = base.GetRequestString("callback");
		if (String.IsNullOrEmpty(callback))
			callback = hfCallback.Value;
		callback = Regex.Replace(callback, @"[^\w\.@$_-]", "");
		hfCallback.Value = callback;


		if (!Page.ClientScript.IsClientScriptBlockRegistered(this.GetType(), "SomeBlock"))
		{
			Page.ClientScript.RegisterClientScriptBlock(
				this.GetType(),
				"SomeBlock",
				String.Format(@"
					function SelEl(userDate)
					{{
						if (!window.opener)
							return;

						var functionName = ""{0}"";
						if (typeof(window.opener.window[functionName]) == ""function"")
							window.opener.window[functionName](userDate);

						var el = window.opener.document.getElementById(""{1}"");
						if (el)
							el.value = userDate.ID;
						el = window.opener.document.getElementById(""{2}"");
						if (el)
							el.innerHTML = userDate.Name;
	
						window.close();
					}}
				", callback, fieldId, fieldName),
				true
			);
		}
	}

	private void InitPage()
	{
		iblockId = base.GetRequestInt("iblock_id");
		if (iblockId > 0)
			hfIBlockId.Value = iblockId.ToString();
		Int32.TryParse(hfIBlockId.Value, out iblockId);
		if (iblockId > 0)
		{
			iblock = BXInfoBlockManagerOld.GetById(iblockId);
			if (iblock == null)
			{
				iblockId = 0;
				hfIBlockId.Value = iblockId.ToString();
			}
		}

		if (iblockId > 0)
		{
			typeId = iblock.TypeId;
			hfTypeId.Value = typeId.ToString();
			type = BXInfoBlockTypeManagerOld.GetById(typeId);
		}
		else
		{
			typeId = base.GetRequestInt("type_id");
			if (typeId > 0)
				hfTypeId.Value = typeId.ToString();
			Int32.TryParse(hfTypeId.Value, out typeId);
		}

		if (!this.BXUser.IsCanOperate("IBlockView", "iblock", iblockId))
			BXAuthentication.AuthenticationRequired();

		filterSectionId.Values.Add(new ListItem(GetMessage("Any"), ""));
		filterSectionId.Values.Add(new ListItem(GetMessage("TopLevel"), "0"));

		BXIBlockSectionCollection sections = BXIBlockSection.GetList(
			((iblockId > 0)? new BXFilter(new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId)) : null),
			new BXOrderBy(new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)),
			null,
			null,
			BXTextEncoder.EmptyTextEncoder
		);

		foreach (BXIBlockSection section in sections)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < section.DepthLevel; i++)
				sb.Append(" . ");
			sb.Append(section.Name);
			filterSectionId.Values.Add(new ListItem(sb.ToString(), section.Id.ToString()));
		}

		filterModifiedBy.Values.Add(new ListItem(GetMessage("Any"), ""));
		filterCreatedBy.Values.Add(new ListItem(GetMessage("Any"), ""));
		foreach (BXUser user in BXUserManager.GetList(null, new BXOrderBy_old("LastName", "Asc", "FirstName", "Asc", "UserName", "Asc")))
		{
			filterModifiedBy.Values.Add(new ListItem("(" + user.UserName + ") " + user.FirstName + " " + user.LastName, user.UserId.ToString()));
			filterCreatedBy.Values.Add(new ListItem("(" + user.UserName + ") " + user.FirstName + " " + user.LastName, user.UserId.ToString()));
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
        string pageTitle;
        if (type == null) pageTitle = GetMessage("PageTitle.IBlockElements");
        else
            if (iblock == null) pageTitle = String.Format("{0}: {1}", type.TypeLang[BXLoc.CurrentLocale].Name, ((!String.IsNullOrEmpty(type.TypeLang[BXLoc.CurrentLocale].SectionName)) ? type.TypeLang[BXLoc.CurrentLocale].SectionName : GetMessage("Groups")));
            else
		pageTitle = String.Format("{0}: {1}", type.TypeLang[BXLoc.CurrentLocale].Name, ((!String.IsNullOrEmpty(iblock.SectionsName)) ? iblock.SectionsName : ((!String.IsNullOrEmpty(type.TypeLang[BXLoc.CurrentLocale].SectionName)) ? type.TypeLang[BXLoc.CurrentLocale].SectionName : GetMessage("Groups"))));
        Page.Title = pageTitle;
        ((BXAdminMasterPage)Page.Master).Title = pageTitle;
	}

	protected string GetEditCommandIdList(string t)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append("[0");
		for (int i = 0; i < GridView1.Rows.Count; i++)
		{
			DataKey key = GridView1.DataKeys[i];
			if (key == null)
				break;
			sb.Append(",");
			if (t.Equals("ID", StringComparison.InvariantCultureIgnoreCase))
				sb.Append(key[t].ToString());
			else
				sb.Append("'" + GridView1.Rows[i].Cells[2].Text.Replace("'", "") + "'");
		}
		sb.Append("]");
		return sb.ToString();
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

		List<string> visibleColumnsList = new List<string>(GridView1.GetVisibleColumnsKeys());
		
		BXFormFilter filter = new BXFormFilter(BXAdminFilter1.CurrentFilter);
		
        if ( iblockId > 0 )
        filter.Add(new BXFormFilterItem("IBlockId", iblockId, BXSqlFilterOperators.Equal));

		BXInfoBlockElementCollectionOld collection = BXInfoBlockElementManagerOld.GetList(filter, sortOrder);
		e.Data = new DataView(FillTable(collection, startRowIndex, visibleColumnsList));
	}
	protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;
		BXGridViewRow row = (BXGridViewRow)e.Row;
		DataRowView drv = (DataRowView)row.DataItem;
		row.UserData["ID"] = drv["ID"];
		row.UserData["Name"] = drv["Name"];
	}

	private DataTable FillTable(BXInfoBlockElementCollectionOld collection, int startRowIndex, List<string> visibleColumnsList)
	{
		if (collection == null)
			collection = new BXInfoBlockElementCollectionOld();

		DataTable result = new DataTable();

		result.Columns.Add("num", typeof(int));
		result.Columns.Add("Name", typeof(string));
		result.Columns.Add("Sort", typeof(int));
		result.Columns.Add("Code", typeof(string));
		result.Columns.Add("XmlId", typeof(string));
		result.Columns.Add("Active", typeof(string));
		result.Columns.Add("UpdateDate", typeof(DateTime));
		result.Columns.Add("ActiveFromDate", typeof(string));
		result.Columns.Add("ActiveToDate", typeof(string));
		result.Columns.Add("ModifiedBy", typeof(string));
		result.Columns.Add("CreateDate", typeof(DateTime));
		result.Columns.Add("CreatedBy", typeof(string));
		result.Columns.Add("PreviewText", typeof(string));
		result.Columns.Add("ID", typeof(int));

		foreach (BXInfoBlockElementOld t in collection)
		{
			DataRow r = result.NewRow();
			r["num"] = startRowIndex++;
			BXInfoBlockElementOld s = (BXInfoBlockElementOld)t;
			r["Name"] = s.Name;
			r["Sort"] = s.Sort;
			r["Code"] = s.Code;
			r["XmlId"] = s.XmlId;
			r["Active"] = (s.Active ? "Y" : "N");
			r["UpdateDate"] = s.UpdateDate;
			r["ActiveFromDate"] = s.ActiveFromDate.ToString();
			r["ActiveToDate"] = s.ActiveToDate.ToString();
			if (visibleColumnsList.Contains("ModifiedBy"))
				r["ModifiedBy"] = ((s.ModifiedByUser != null) ? String.Format("({0}) {1} {2}", s.ModifiedByUser.UserName, s.ModifiedByUser.FirstName, s.ModifiedByUser.LastName) : "");
			else
				r["ModifiedBy"] = "";
			r["CreateDate"] = s.CreateDate;
			if (visibleColumnsList.Contains("CreatedBy"))
				r["CreatedBy"] = ((s.CreatedByUser != null) ? String.Format("({0}) {1} {2}", s.CreatedByUser.UserName, s.CreatedByUser.FirstName, s.CreatedByUser.LastName) : "");
			else
				r["CreatedBy"] = "";
			r["PreviewText"] = s.PreviewText;
			r["ID"] = s.ElementId;

			result.Rows.Add(r);
		}

		return result;
	}

	protected void GridView1_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		BXFormFilter filter = new BXFormFilter(BXAdminFilter1.CurrentFilter);
        if ( iblockId > 0)
		filter.Add(new BXFormFilterItem("IBlockId", iblockId, BXSqlFilterOperators.Equal));

		e.Count = BXInfoBlockElementManagerOld.Count(filter);
	}

	protected void filterUser_CustomBuildFilter(object sender, BXTextBoxAndDropDownFilter.BuildFilterEventArgs e)
	{
		BXTextBoxAndDropDownFilter filter = (BXTextBoxAndDropDownFilter)sender;
		e.FilterItems.Clear();
		int id;
		if (int.TryParse(e.DropDownValue, out id))
			e.FilterItems.Add(new BXFormFilterItem(filter.Key, id, BXSqlFilterOperators.Equal));
		if (int.TryParse(e.TextBoxValue, out id))
			e.FilterItems.Add(new BXFormFilterItem(filter.Key, id, BXSqlFilterOperators.Equal));
	}
}
