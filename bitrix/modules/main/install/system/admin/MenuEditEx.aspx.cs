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
using System.IO;

using Bitrix;
using Bitrix.Services;
using Bitrix.IO;
using Bitrix.Services.Js;
using Bitrix.Services.Text;
using System.Collections.Generic;
using Bitrix.Security;

public partial class bitrix_admin_MenuEditEx : BXAdminPage, IBXFileManPage
{
    // FIELDS
    StringBuilder _ErrorText = new StringBuilder();
    int _ShowMessage;
    protected int index = 1;
    string script;
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
    private void Bind()
    {
        index = 1;
        script = string.Empty;
        //Menu.Sort();
        repItems.DataSource = Menu;
        repItems.DataBind();
        for (int i = 0; i < repItems.Items.Count; i++)
        {
            Control rep = repItems.Items[i];
            TextBox tbExtra = rep.FindControl("tbExtra") as TextBox;
            BXPublicMenuItem item = Menu[i];
            DropDownList ctrl = repItems.Items[i].FindControl("ConditionType") as DropDownList;
			CheckBox cbDelete = rep.FindControl("cbDelete") as CheckBox;
            
			ctrl.SelectedIndex = (int)item.ConditionType;
			cbDelete.Checked = IsDeleted(item);

            switch (item.ConditionType)
            {
                case ConditionType.FileOrFolder:
                    TextBox tbFileOrFolder = rep.FindControl("tbFileOrFolder") as TextBox;
                    tbFileOrFolder.Text = item.Condition;
                    break;
                case ConditionType.Group:
                    IRoleList rlGroup = rep.FindControl("rlGroup") as IRoleList;
                    rlGroup.Str = item.Condition;
                    break;
                case ConditionType.Time:
                    ITimeInterval tiPeriod = rep.FindControl("tiPeriod") as ITimeInterval;
                    tiPeriod.Str = item.Condition;
                    break;
                case ConditionType.Url:
                    IUrlParameter upParams = rep.FindControl("upParams") as IUrlParameter;
                    upParams.Str = item.Condition;
                    break;
            }

            for (int j = 1; j < item.Links.Count; j++)
                tbExtra.Text += item.Links[j] + "\r\n";

            ctrl.Attributes.Add("onchange", string.Format("ShowSelected(this.value,{0});", i + 1));

            script += string.Format("ShowSelected({0},{1});", (int)Menu[i].ConditionType, i + 1);
        }
    }
    private void SaveMenu(bool removeDeleted)
    {
        int sort = 0;
        Menu.Clear();
        foreach (Control rep in repItems.Controls)
        {
            TextBox tbTitle = rep.FindControl("tbTitle") as TextBox;
            TextBox tbLink = rep.FindControl("tbLink") as TextBox;
            TextBox tbParamName = rep.FindControl("tbParamName") as TextBox;
            TextBox tbParamValue = rep.FindControl("tbParamValue") as TextBox;
            TextBox tbSort = rep.FindControl("tbSort") as TextBox;
            CheckBox cbDelete = rep.FindControl("cbDelete") as CheckBox;
            TextBox tbExtra = rep.FindControl("tbExtra") as TextBox;
            DropDownList ddlConditiontype = rep.FindControl("ConditionType") as DropDownList;
            BXPublicMenuItem item = new BXPublicMenuItem();
            item.ConditionType = (ConditionType)Enum.Parse(typeof(ConditionType), ddlConditiontype.SelectedValue);

            switch (item.ConditionType)
            {
                case ConditionType.None:
                    item.Condition = string.Empty;
                    break;
                case ConditionType.FileOrFolder:
                    TextBox tbFileOrFolder = rep.FindControl("tbFileOrFolder") as TextBox;
                    if (string.IsNullOrEmpty(tbFileOrFolder.Text))
                        item.ConditionType = ConditionType.None;
                    else
                        item.Condition = tbFileOrFolder.Text;
                    break;
                case ConditionType.Group:
                    IRoleList rlGroup = rep.FindControl("rlGroup") as IRoleList;
                    if (rlGroup.SelectedRoles.Length == 0)
                        item.ConditionType = ConditionType.None;
                    else
                        item.Condition = BXStringUtility.ListToString(rlGroup.SelectedRoles);
                    break;
                case ConditionType.Time:
                    ITimeInterval tiPeriod = rep.FindControl("tiPeriod") as ITimeInterval;
                    if (tiPeriod.StartDate.Equals(DateTime.MinValue) && tiPeriod.EndDate.Equals(DateTime.MaxValue))
                        item.ConditionType = ConditionType.None;
                    else
						item.Condition = BXStringUtility.TimeIntervalToString(tiPeriod.StartDate, tiPeriod.EndDate);
                    break;
                case ConditionType.Url:
                    IUrlParameter upParams = rep.FindControl("upParams") as IUrlParameter;
                    if (string.IsNullOrEmpty(upParams.ParameterName))
                        item.ConditionType = ConditionType.None;
                    else
						item.Condition = BXStringUtility.ParamToString(upParams.ParameterName, upParams.ParameterValue);
                    break;
            }
            //Sort
            if (!int.TryParse(tbSort.Text, out sort))
                sort += 10;
            item.Sort = sort;
            if (cbDelete.Checked)
				MarkDeleted(item);
            item.Title = tbTitle.Text; //Title
            item.Link = tbLink.Text; //Link

            //ExtraLinks
            foreach (string link in tbExtra.Text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                item.Links.Add(link);
            }

            //Param
            if (!string.IsNullOrEmpty(tbParamName.Text) &&
                !string.IsNullOrEmpty(tbParamValue.Text))
            {
                item.ParamName = tbParamName.Text.Trim();
                item.ParamValue = tbParamValue.Text.Trim();
            }

            if (!removeDeleted
				|| !IsDeleted(item) && !string.IsNullOrEmpty(item.Title))
                Menu.Add(item);
        }
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
	BXPublicMenuItemCollection menu;
	List<BXPublicMenuItem> deleted;
    public BXPublicMenuItemCollection Menu
    {
        get
        {
			if (menu == null)
			{
				int count;
				if (IsPostBack && int.TryParse(MenuItemsCount.Value, out count))
				{
					menu = new BXPublicMenuItemCollection();
					for (int i = 0; i < count; i++)
						menu.Add(new BXPublicMenuItem());
				}
				else
				{
					menu = BXPublicMenu.Menu.Load(CurDir, MenuType);
				}
			}
			return menu;
        }
    }
	public bool IsDeleted(BXPublicMenuItem item)
	{
		if (deleted == null)
			return false;
		return deleted.Contains(item);
	}
	public void MarkDeleted(BXPublicMenuItem item)
	{
		if (deleted == null)
			deleted = new List<BXPublicMenuItem>();
		deleted.Add(item);
	}
	public string SiteId
	{
		get
		{
			if (curSiteId == null)
			{
				BXSite site = BXSite.GetCurrentSite(Path, Request.Url.Host);
				if (site != null)
					curSiteId = site.Id;
			}
			return curSiteId;
		}
	}
	protected override string BackUrl
	{
		get
		{
			return base.BackUrl ?? ("MenuEdit.aspx?path=" + HttpUtility.UrlEncode(Path ?? string.Empty));
		}
	}

    // EVENTS HANDLERS
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
			BXAuthentication.AuthenticationRequired();
		
		bool canOperate = 
			(string.IsNullOrEmpty(SiteId) ? !BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", SiteId) : !BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit))
			| BXSecureIO.CheckWrite(BXPath.Combine(CurDir, MenuType + ".menu")); 

		if (!canOperate)
			BXAuthentication.AuthenticationRequired();

		if (string.IsNullOrEmpty(Path))
			GoBack();

		if (!string.IsNullOrEmpty(SiteId))
		{
			foreach (KeyValuePair<string, string> p in BXPublicMenu.GetMenuTypes(curSiteId))
				ddlMenu.Items.Add(new ListItem(p.Value, p.Key));
			if (ddlMenu.Items.FindByValue(MenuType) == null)
				GoBack();
			ddlMenu.SelectedValue = MenuType;
		}
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		Bind();
	}
	protected void Page_PreRender(object sender, EventArgs e)
	{
		MasterTitle = GetMessage("MasterTitle");
		PrepareResultMessage();
		MenuItemsCount.Value = Menu.Count.ToString();

		ClientScript.RegisterStartupScript(this.GetType(), "InitShow", script, true);
		ClientScript.RegisterStartupScript(this.GetType(), "Localization", string.Format("var AreYouSure = '{0}';", GetMessageJS("AreYouSure")), true);
	}

    protected void mainTabControl_Command(object sender, BXTabControlCommandEventArgs e)
    {
        if (string.IsNullOrEmpty(Path))
            return;

        switch (e.CommandName.ToLower())
        {
            case "apply":
            case "save":
                SaveMenu(true);
                BXPublicMenu.Menu.Save(CurDir, MenuType,  Menu);
				break;
        }
        if (e.CommandName.ToLower() == "apply")
			Response.Redirect(Request.RawUrl);
        else
           GoBack();
    }
    protected void btmInsertBottom_Click(object sender, EventArgs e)
    {
        SaveMenu(false);

        Button btn = sender as Button;
        Menu.Insert(int.Parse(btn.CommandArgument), new BXPublicMenuItem());
        //RESORT
        for (int i = 0; i < Menu.Count; i++)
            Menu[i].Sort = 10 * (i + 1);
        Bind();
    }

	#region IBXFileManPage Members

	public string ProvidePath()
	{
		return BXPath.GetDirectory(BXPath.ToVirtualRelativePath(Request["path"] ?? "~")).ToLowerInvariant();
	}

	#endregion
}