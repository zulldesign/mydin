using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Bitrix.UI;
using Bitrix;
using System.IO;
using Bitrix.Services;

using Bitrix.IO;
using System.Collections.Generic;
using Bitrix.Security;

public partial class bitrix_admin_MenuEdit : BXAdminPage, IBXFileManPage
{
    // FIELDS
    StringBuilder _ErrorText = new StringBuilder();
    int _ShowMessage;
	string curPath;
	string curDir;
	string curMenuType;
	string curSiteId;

    // METHODS
    void PrepareResultMessage()
    {
        if (_ShowMessage == 0)
        {
            resultMessage.Visible = false;
            return;
        }
        resultMessage.Visible = true;
        resultMessage.CssClass = (_ShowMessage > 0) ? "Ok" : "Error";
        resultMessage.IconClass = (_ShowMessage > 0) ? "Ok" : "Error";
        resultMessage.Title = (_ShowMessage > 0) ? GetMessage("Message.OperationSuccessful") : GetMessage("Message.OperationErrors");
        resultMessage.Content = (_ShowMessage > 0) ? String.Empty : _ErrorText.ToString();
    }
    void ShowError(string encodedMessage)
    {
        _ShowMessage = -1;
        _ErrorText.AppendLine("<br />");
        _ErrorText.AppendLine(encodedMessage);
    }
    void ShowOk()
    {
        if (_ShowMessage == 0)
            _ShowMessage = 1;
    }

    // PROPERTIES
	public string Path
	{
		get
		{
			if (curPath == null && !string.IsNullOrEmpty(Request["path"]))
				curPath = BXPath.ToVirtualRelativePath(Request["path"]);

			return curPath;
		}
	}
	public string CurDir
	{
		get
		{
			if (curDir == null && Path != null)
			{
				string temp = null;
				BXPath.BreakPath(Path, ref curDir, ref temp);
			}
			return curDir;
		}
	}
	public string MenuType
	{
		get
		{
			if (curMenuType == null && Path != null && Path.EndsWith(".menu"))
			{
				try
				{
					curMenuType = System.IO.Path.GetFileNameWithoutExtension(Path).ToLowerInvariant().Trim();
				}
				catch
				{
					curMenuType = null;
				}
			}
			return curMenuType;
		}
	}
	protected override string BackUrl
	{
		get
		{
			return base.BackUrl ?? ("FileMan.aspx?path=" + HttpUtility.UrlEncode(CurDir ?? "~"));
		}
	}

	// EVENTS HANDLERS
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
			BXAuthentication.AuthenticationRequired();

		if (string.IsNullOrEmpty(Path))
			Response.Redirect("FileMan.aspx");
		if (string.IsNullOrEmpty(MenuType))
			Response.Redirect("FileMan.aspx");

		BXSite site = BXSite.GetCurrentSite(Path, Request.Url.Host);
		if (site != null)
		{
			curSiteId = site.Id;
			foreach (KeyValuePair<string, string> p in BXPublicMenu.GetMenuTypes(curSiteId))
				ddlMenu.Items.Add(new ListItem(p.Value, p.Key));
			if (ddlMenu.Items.FindByValue(MenuType) == null)
				return;
			ddlMenu.SelectedValue = MenuType;
		}

		bool canOperate = 
			(string.IsNullOrEmpty(curSiteId) ? !BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", curSiteId) : !BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit))
			| BXSecureIO.CheckWrite(BXPath.Combine(CurDir, MenuType + ".menu"));  

		if (!canOperate)
			BXAuthentication.AuthenticationRequired();

		BXPublicMenuItemCollection menu = BXPublicMenu.Menu.Load(CurDir, MenuType);
		int sort = 1;
		foreach (BXPublicMenuItem item in menu)
			tblItems.Rows.Add(CreateRow(item.Title, item.Link, 10 * sort++, true));

		for (int i = 0; i < 5; i++)
			tblItems.Rows.Add(CreateRow(string.Empty, string.Empty, 10 * sort++, false));
	}
    protected void Page_LoadComplete(object sender, EventArgs e)
    {
		MasterTitle = GetMessage("MasterTitle");
        PrepareResultMessage();
    }
	
    private HtmlTableRow CreateRow(string title, string link, int sort, bool isDeletable)
    {
        HtmlTableRow row = new HtmlTableRow();

        //TITLE
        HtmlTableCell cell = new HtmlTableCell();
        cell.Align = "center";
        TextBox tbTitle = new TextBox();
        tbTitle.Columns = 31;
        tbTitle.Text = title;
        cell.Controls.Add(tbTitle);
        row.Cells.Add(cell);

        //LINK
        cell = new HtmlTableCell();
        cell.Align = "center";
        TextBox tbLink = new TextBox();
        tbLink.Columns = 31;
        tbLink.Text = link;
        cell.Controls.Add(tbLink);
		HtmlInputButton btLink = new HtmlInputButton();
		btLink.Value = "...";
		tbLink.PreRender += delegate(object sender, EventArgs e)
		{
			btLink.Attributes["onclick"] = string.Format("return SelectDirClick('{0}');", tbLink.ClientID);
		};
		cell.Controls.Add(btLink);
        row.Cells.Add(cell);

        //SORT
        cell = new HtmlTableCell();
        cell.Align = "center";
        TextBox tbSort = new TextBox();
        tbSort.Text = sort.ToString();
        tbSort.Columns = 3;
        tbSort.MaxLength = 3;
        cell.Controls.Add(tbSort);
        row.Cells.Add(cell);

        //DELETE
        cell = new HtmlTableCell();
        cell.Align = "center";
        CheckBox delete = new CheckBox();
        cell.Controls.Add(delete);
        delete.Visible = isDeletable;
        row.Cells.Add(cell);

        return row;
    }
    protected void mainTabControl_Command(object sender, BXTabControlCommandEventArgs e)
    {
        if (string.IsNullOrEmpty(Path))
            return;

        BXPublicMenuItemCollection menu = new BXPublicMenuItemCollection();

        switch (e.CommandName.ToLower())
        {
            case "apply":
            case "save":
                for (int i = 1; i < tblItems.Rows.Count; i++)
                {
                    TextBox title = tblItems.Rows[i].Cells[0].Controls[0] as TextBox;
                    TextBox link = tblItems.Rows[i].Cells[1].Controls[0] as TextBox;
                    TextBox sort = tblItems.Rows[i].Cells[2].Controls[0] as TextBox;
                    CheckBox delete = tblItems.Rows[i].Cells[3].Controls[0] as CheckBox;

                    if (!delete.Checked &&
                        !string.IsNullOrEmpty(title.Text.Trim()))
                    {
						BXPublicMenuItem item = new BXPublicMenuItem();
                        item.Link = link.Text;
                        item.Title = title.Text;
                        int weight;
                        if (int.TryParse(sort.Text, out weight))
                            item.Sort = weight;
                        else
                            item.Sort = 10;

                        menu.Add(item);
                    }

                }
				BXPublicMenu.Menu.Save(CurDir, MenuType, menu);
				if (e.CommandName.ToLower() == "apply")
					Response.Redirect(Request.RawUrl);
                break;
        }
		GoBack();
    }
    protected void ddlMenu_SelectedIndexChanged(object sender, EventArgs e)
    {
        Response.Redirect("MenuEdit.aspx?path=" + UrlEncode(BXPath.Combine(CurDir, ddlMenu.SelectedValue + ".menu")));
    }
    protected void mainActionBar_CommandClick(object sender, CommandEventArgs e)
    {
        switch (e.CommandName.ToLower())
        {
            case "back":
                GoBack();
                break;
        }
    }


	#region IBXFileManPage Members

	public string ProvidePath()
	{
		return BXPath.GetDirectory(BXPath.ToVirtualRelativePath(Request["path"] ?? "~")).ToLowerInvariant();
	}

	#endregion
}