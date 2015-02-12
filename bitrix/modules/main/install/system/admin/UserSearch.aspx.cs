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
using Bitrix.DataLayer;
using System.Collections.Generic;
using System.Text;
using Bitrix.Security;
using Bitrix.Services;
using System.Text.RegularExpressions;
using Bitrix.Services.Text;

public partial class bitrix_admin_UserSearch : BXAdminPage
{
    int _userId;
    int[] userViewRoles;

	protected void Page_Init(object sender, EventArgs e)
	{
        userViewRoles = this.BXUser.GetUserOperationRoles(BXRoleOperation.Operations.UserView);
        if (userViewRoles.Length == 0)
            BXAuthentication.AuthenticationRequired();

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
				", callback, base.GetRequestString("editor_id"), base.GetRequestString("label_id")),
				true
            );
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
        ((BXAdminMasterPage)Page.Master).Title = GetMessageRaw("lsUserSearch");
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

		List<string> visibleColumnsList = new List<string>(GridView1.GetVisibleColumnsKeys());
		
		BXFormFilter filter = new BXFormFilter(BXAdminFilter1.CurrentFilter);
		
        if ( _userId > 0 )
            filter.Add(new BXFormFilterItem("UserId", _userId, BXSqlFilterOperators.Equal));

		BXUserCollection collection = BXUserManager.GetList(filter, null);
		e.Data = new DataView(FillTable(collection, startRowIndex, visibleColumnsList));
	}
	protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;
		BXGridViewRow row = (BXGridViewRow)e.Row;
		DataRowView drv = (DataRowView)row.DataItem;
		row.UserData["ID"] = drv["ID"];
        row.UserData["Name"] = string.Format("[{0}] ({1}) {2}", drv["ID"], drv["Login"], drv["FirstName"] + " " + drv["LastName"]); ;
	}

	private DataTable FillTable(BXUserCollection collection, int startRowIndex, List<string> visibleColumnsList)
	{
		if (collection == null)
            collection = new BXUserCollection();

		DataTable result = new DataTable();

        result.Columns.Add("num", typeof(int));
        result.Columns.Add("ID", typeof(int));
        result.Columns.Add("Active", typeof(string));
        result.Columns.Add("Login", typeof(string));
        result.Columns.Add("FirstName", typeof(string));
        result.Columns.Add("LastName", typeof(string));
        result.Columns.Add("Email", typeof(string));
        result.Columns.Add("DateActive", typeof(DateTime));

		foreach (BXUser t in collection)
		{
			DataRow r = result.NewRow();
			r["num"] = startRowIndex++;
            BXUser s = (BXUser)t;
            r["ID"] = s.UserId;
            r["Active"] = s.IsApproved ? GetMessageRaw("Kernel.Yes") : GetMessageRaw("Kernel.No");
            r["Login"] = s.UserName;
            r["FirstName"] = s.FirstName;
            r["LastName"] = s.LastName;
            r["Email"] = s.Email;
            r["DateActive"] = s.LastActivityDate;
			
			result.Rows.Add(r);
		}

		return result;
	}

	protected void GridView1_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		BXFilter filter = new BXFilter();
        if ( _userId > 0)
            filter.Add(new BXFilterItem(Bitrix.Security.BXUser.Fields.UserId, BXSqlFilterOperators.Equal, _userId));
        e.Count = Bitrix.Security.BXUser.Count(filter);
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

    protected void MoveSelectedUp(ListItemCollection items)
    {
        var selItems = new List<ListItem>();
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].Selected)
            {
                selItems.Add(items[i]);
                items.RemoveAt(i);
            }
        }
        foreach (var i in selItems)
            items.Insert(0, i);
    }
}
