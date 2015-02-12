using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.Main;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;

public partial class bitrix_admin_SiteEdit : Bitrix.UI.BXAdminPage
{
	bool currentUserCanModify;
    bool saveSuccess;


	protected override string BackUrl
	{
		get
		{
			return base.BackUrl ?? "SiteAdmin.aspx";
		}
	}

	#region Page_Init()
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();
		currentUserCanModify = BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
	} 
	#endregion

	#region Page_Load()
	protected void Page_Load(object sender, EventArgs e)
	{
		BindConditions();
	}
	#endregion
   
    private void PrepareResultMessage()
    {
        if (!saveSuccess)
        {
            resultMessage.Visible = false;
            return;
        }
        resultMessage.Visible = true;
        resultMessage.CssClass = "Ok";
        resultMessage.Title = GetMessage("Message.OperationSuccessful");
        resultMessage.Content = String.Empty;
    }

	#region Page_LoadComplete()

	protected void Page_LoadComplete(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(SiteId))
            MasterTitle = string.Format(GetMessage("EditPageTitle"), Encode(SiteId));
        else
            MasterTitle = GetMessage("CreatePageTitle");

		AddSiteSeparator.Visible = AddSiteButton.Visible = 
			CopySiteSeparator.Visible = CopySiteButton.Visible = 
			DeleteSiteSeparator.Visible = DeleteSiteButton.Visible = !string.IsNullOrEmpty(SiteId) && currentUserCanModify;

		BXTabControl1.ShowSaveButton = BXTabControl1.ShowApplyButton = currentUserCanModify;

        Title = MasterTitle;

		txtID.Visible = string.IsNullOrEmpty(SiteId);
		lbID.Visible = !string.IsNullOrEmpty(SiteId);
		lbID.Text = Encode(SiteId);

        if (!IsPostBack)
        {
            Language.DataSource = BXLanguage.GetList(
				new BXFilter(new BXFilterItem(BXLanguage.Fields.Active, BXSqlFilterOperators.Equal, true)),
				new BXOrderBy(new BXOrderByPair(BXLanguage.Fields.Default, BXOrderByDirection.Desc)), 
				new BXSelect(BXLanguage.Fields.ID, BXLanguage.Fields,Name), 
				null, 
				BXTextEncoder.EmptyTextEncoder
			);
            Language.DataTextField = "Name";
            Language.DataValueField = "ID";
            Language.DataBind();

           

            if (!string.IsNullOrEmpty(SiteId))
                BindSite(SiteId);
			else if (!string.IsNullOrEmpty(SourceSiteId))
				BindSite(SourceSiteId);
			else
			{
				Language.SelectedValue = BXLoc.CurrentLocale;
			}
        }

        PrepareResultMessage();
    }
    #endregion

    #region BindSite()
    private void BindSite(string siteId)
    {
        BXSite site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder); //Ибо и так все кодируется
		txtID.Text = site.Id;
        Active.Checked = site.Active;
        Active.Enabled = !site.Default;
        Default.Checked = site.Default;
		Default.Enabled = !site.Default;

        Name.Text = site.Name;
        Sort.Text = site.Sort.ToString();
		Culture.SelectedCulture = site.Culture;
        Language.SelectedValue = site.LanguageId;
        Email.Text = site.Email;
        SiteFolder.Text = site.Directory;
		RemapFolder.Text = site.RemapDirectory;
        ServerUrl.Text = site.ServerName;
        SiteName.Text = site.SiteName;

        BXSiteDomainCollection domains = BXSiteDomain.GetList(new BXFilter(new BXFilterItem(BXSiteDomain.Fields.ID, BXSqlFilterOperators.Equal, siteId)), null, null, null, BXTextEncoder.EmptyTextEncoder);
        Domains.Text = string.Empty;
        foreach (BXSiteDomain domain in domains)
        {
            Domains.Text += domain.Domain;
            Domains.Text += "\n";
        }

    }


    #endregion

    #region BindConditions()
    private void BindConditions()
    {
        int currentRowIndex = 0;
        int currentSort = 0;
        bool alreadyBinded = false;

		BXTemplateConditionCollection templCollection = BXTemplateCondition.GetList(
			new BXFilter(new BXFilterItem(BXTemplateCondition.Fields.SiteId, BXSqlFilterOperators.Equal, SiteId)),
			new BXOrderBy(new BXOrderByPair(BXTemplateCondition.Fields.Sort, BXOrderByDirection.Asc)),
			null, 
			null,
			BXTextEncoder.EmptyTextEncoder
		);

		if (ConditionTable.Rows.Count >= templCollection.Count && ConditionTable.Rows.Count != 1)
            alreadyBinded = true;

		foreach (BXTemplateCondition condition in templCollection)
        {
            currentRowIndex++;
            currentSort += 10;
            HtmlTableRow row = new HtmlTableRow();
            if (alreadyBinded)
            {
                row = ConditionTable.Rows[currentRowIndex];
            }

            row.ID = string.Format("row_{0}_", currentRowIndex);

            //ШАБЛОН
            if (!alreadyBinded)
                row.Cells.Add(new HtmlTableCell());
            CreateTemplateCell(condition.Template, row.Cells[0]);

            //СОРТИРОВКА
            if (!alreadyBinded)
                row.Cells.Add(new HtmlTableCell());
            CreateSortCell(currentSort, row.Cells[1]);

            //ТИП УСЛОВИЯ
            if (!alreadyBinded)
                row.Cells.Add(new HtmlTableCell());
            CreateConditionTypeCell(condition.ConditionType, row.Cells[2]);

            //Условие
            if (!alreadyBinded)
                row.Cells.Add(new HtmlTableCell());
            CreateConditionCell(currentRowIndex, condition, row.Cells[3]);

            if (!alreadyBinded)
                ConditionTable.Rows.Add(row);
        }

        while (ConditionTable.Rows.Count > currentRowIndex + 4)
        {
            ConditionTable.Rows.RemoveAt(ConditionTable.Rows.Count - 1);
        }

        for (int i = currentRowIndex + 1; i < ConditionTable.Rows.Count; i++)
        {
            currentSort += 10;
            //CLEAR ROW
            DropDownList templates = ConditionTable.Rows[i].Cells[0].Controls[0] as DropDownList;
            templates.SelectedValue = "";

            TextBox txt = ConditionTable.Rows[i].Cells[1].Controls[0] as TextBox;
            txt.Text = currentSort.ToString();

            DropDownList types = ConditionTable.Rows[i].Cells[2].Controls[0] as DropDownList;
            types.SelectedValue = "0";

            ConditionTable.Rows[i].Cells[3].Controls[0].Visible = false;
            ConditionTable.Rows[i].Cells[3].Controls[1].Visible = false;
            ConditionTable.Rows[i].Cells[3].Controls[2].Visible = false;
            ConditionTable.Rows[i].Cells[3].Controls[3].Visible = false;
        }

        //ADD 3 EXTRA CONDITION
        for (int i = 0; i < currentRowIndex + 4 - ConditionTable.Rows.Count; i++)
        {
            currentRowIndex++;
            currentSort += 10;
            HtmlTableRow row = new HtmlTableRow();
            row.ID = string.Format("row_{0}_", currentRowIndex);

            //ШАБЛОН
            row.Cells.Add(new HtmlTableCell());
            CreateTemplateCell("", row.Cells[0]);

            //СОРТИРОВКА
            row.Cells.Add(new HtmlTableCell());
            CreateSortCell(currentSort, row.Cells[1]);

            //ТИП УСЛОВИЯ
            row.Cells.Add(new HtmlTableCell());
            CreateConditionTypeCell(0, row.Cells[2]);

            //Условие
            row.Cells.Add(new HtmlTableCell());
            CreateConditionCell(currentRowIndex, new BXTemplateCondition(), row.Cells[3]);
            ConditionTable.Rows.Add(row);
        }
    }

    #region Control Creation Helpers
    private void CreateConditionCell(int rowIndex, BXTemplateCondition condition, HtmlTableCell cell)
    {
        string strCondition = string.IsNullOrEmpty(condition.Condition) ? "" : condition.Condition;
        //FileOrFolder
        bool controlExist = false;
        TextBox txt = new TextBox();
		Control group = LoadControl("~/bitrix/admin/controls/Main/rolelist.ascx");
		Control url = LoadControl("~/bitrix/controls/Main/urlparameter.ascx");
		Control time = LoadControl("~/bitrix/controls/Main/TimeInterval.ascx");
        if (cell.Controls.Count == 4)
        {
            controlExist = true;
            txt = cell.Controls[0] as TextBox;
            group = cell.Controls[1];
            url = cell.Controls[2];
            time = cell.Controls[3];

        }
        txt.Width = new Unit("100%");
        txt.ID = string.Format("row_{0}_textbox", rowIndex);
        txt.Visible = (condition.ConditionType == (int)ConditionType.FileOrFolder);
        if (txt.Visible)
            txt.Text = strCondition;


        //Group
        group.ID = string.Format("row_{0}_rolelist", rowIndex);
        group.Visible = (condition.ConditionType == (int)ConditionType.Group);
        if (condition.ConditionType == (int)ConditionType.Group)
        {
            IRoleList rl = group as IRoleList;
            rl.SelectedRoles = BXStringUtility.StringToList(strCondition);
        }


        //Url
        url.ID = string.Format("row_{0}_urlparameter", rowIndex);
        url.Visible = (condition.ConditionType == (int)ConditionType.Url);
        if (url.Visible)
        {
            IUrlParameter up = url as IUrlParameter;
            up.ParameterName = BXStringUtility.StringToParam(strCondition)[0];
            up.ParameterValue = BXStringUtility.StringToParam(strCondition)[1];
        }


        //Time
        time.ID = string.Format("row_{0}_TimeInterval", rowIndex);
        time.Visible = (condition.ConditionType == (int)ConditionType.Time);
        if (time.Visible)
        {
            ITimeInterval ti = time as ITimeInterval;
            ti.StartDate = BXStringUtility.StringToTimeInterval(strCondition)[0];
            ti.EndDate = BXStringUtility.StringToTimeInterval(strCondition)[1];
        }
        if (!controlExist)
        {
            cell.Controls.Add(txt);
            cell.Controls.Add(group);
            cell.Controls.Add(url);
            cell.Controls.Add(time);
        }
    }

    private void CreateConditionTypeCell(int conditionType, HtmlTableCell cell)
    {
        bool controlExist = false;
        DropDownList conditiontypes = new DropDownList();
        if (cell.Controls.Count == 1)
        {
            controlExist = true;
            conditiontypes = cell.Controls[0] as DropDownList;
        }


        if (!controlExist)
        {
            conditiontypes.SelectedIndexChanged += new EventHandler(conditiontypes_SelectedIndexChanged);
            conditiontypes.Items.Add(new ListItem(GetMessage("ConditionType.None"), "0"));
            conditiontypes.Items.Add(new ListItem(GetMessage("ConditionType.FileOrFolder"), "1"));
            conditiontypes.Items.Add(new ListItem(GetMessage("ConditionType.Group"), "2"));
            conditiontypes.Items.Add(new ListItem(GetMessage("ConditionType.Time"), "3"));
            conditiontypes.Items.Add(new ListItem(GetMessage("ConditionType.URL"), "4"));
            conditiontypes.AutoPostBack = true;
            cell.Controls.Add(conditiontypes);
        }

        conditiontypes.SelectedValue = conditionType.ToString();
    }

    private void CreateSortCell(int sort, HtmlTableCell currentCell)
    {
        TextBox txt = new TextBox();
        bool controlExist = false;
        if (currentCell.Controls.Count == 1)
        {
            txt = currentCell.Controls[0] as TextBox;
            controlExist = true;
        }

        txt.MaxLength = 3;
        txt.Columns = 3;
        txt.Text = sort.ToString();
        if (!controlExist)
            currentCell.Controls.Add(txt);
    }

    private DirectoryInfo[] templateDirectories;
    private void CreateTemplateCell(string template, HtmlTableCell cell)
    {
        bool controlExist = false;
        DropDownList templates = new DropDownList();
        if (cell.Controls.Count == 1)
        {
            controlExist = true;
            templates = cell.Controls[0] as DropDownList;
        }

        if (!controlExist)
        {
            templates.Items.Add(new ListItem(GetMessage("NoSelection"), ""));
            if (templateDirectories == null)
			{
				DirectoryInfo dir = new DirectoryInfo(MapPath("~/bitrix/templates"));
				templateDirectories = dir.Exists ? dir.GetDirectories() : new DirectoryInfo[0];
			}

            foreach (DirectoryInfo temp in templateDirectories)
            {
				if (!File.Exists(Path.Combine(temp.FullName, BXConfigurationUtility.Constants.TemplateFileName)))
					continue;
				
				ListItem li = new ListItem();
                li.Value = temp.Name;
                li.Text = temp.Name;
                string resorcePath = string.Format("{0}\\lang\\{1}\\include.lang", temp.FullName, BXLoc.CurrentLocale);
                if (File.Exists(resorcePath))
                {
                    using (BXResourceFile res = new BXResourceFile(resorcePath))
                    {
                        if (res.ContainsKey("Template.Name"))
                            li.Text = res["Template.Name"];
                    }
                }
                templates.Items.Add(li);
            }
            cell.Controls.Add(templates);
        }
        templates.SelectedValue = template;
    }
    #endregion

    #endregion

    #region ValidateFields()
    protected bool ValidateFields()
    {
        //SET DEFAULTS
        starID.Visible = false;
		starName.Visible = false;
		starSort.Visible = false;
        starSiteName.Visible = false;
        bool errors = false;

        //ID
        if (string.IsNullOrEmpty(SiteId))
        {
            txtID.Text = txtID.Text.Trim().ToLower();

            //EMPTY OR TOO LONG
            if (string.IsNullOrEmpty(txtID.Text.Trim()) || txtID.Text.Trim().Length > 50)
            {
				starID.Visible = true;
                errorMessage.AddErrorMessage(GetMessage("ErrorID"));
                errors = true;
            }
            //DUPLICATE
            else
            {
                BXSite site = BXSite.GetById(txtID.Text);
                if (site != null)
                {
                    starID.Visible = true;
                    errorMessage.AddErrorMessage(GetMessage("ErrorDuplicate"));
                    errors = true;
                }
            }
        }

        //NAME
        if (string.IsNullOrEmpty(Name.Text.Trim()))
        {
            starName.Visible = true;
            errorMessage.AddErrorMessage(GetMessage("ErrorName"));
            errors = true;
        }

        //SORT
        int sort;
        if (!int.TryParse(Sort.Text, out sort))
        {
            starSort.Visible = true;
            errorMessage.AddErrorMessage(GetMessage("ErrorSort"));
            errors = true;
        }

        //Template
        if (Conditions.Count == 0)
        {
            errorMessage.AddErrorMessage(GetMessage("ErrorCondition"));
			errors = true;
        }

        //SiteName
        string siteName = SiteName.Text;
        if (!string.IsNullOrEmpty(siteName))
            siteName = siteName.Trim();

        if (string.IsNullOrEmpty(siteName))
        {
            errorMessage.AddErrorMessage(GetMessage("ErrorSiteName"));
            errors = true;
        }
        return !errors;
    }
    #endregion

    #region SaveSite()
    private void SaveSite(bool isApply)
    {
        saveSuccess = true;
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
		{
			errorMessage.AddErrorMessage(GetMessage("Exception.YouDontHaveRightsToPerformThisOperation"));
			return;
		}

        string[] domains = Domains.Text.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        BXSite site = BXSite.GetById(SiteId, BXTextEncoder.EmptyTextEncoder); //Ибо и так все кодируется
		if (site == null)
        {
			site = new BXSite(txtID.Text, BXTextEncoder.EmptyTextEncoder);
        }

		site.Name = Name.Text.Trim();
        site.Active = Active.Checked;
		if (!site.Default)
			site.Default = Default.Checked;
		site.Culture = Culture.SelectedCulture;
        //site.Charset = Encoding.Text.Trim();
        site.Sort = int.Parse(Sort.Text);
        site.LanguageId = Language.SelectedValue;
        site.SiteName = SiteName.Text.Trim();
        site.ServerName = ServerUrl.Text.Trim();
        site.Directory = SiteFolder.Text.Trim();
		site.RemapDirectory = RemapFolder.Text.Trim();
        site.Email = Email.Text.Trim();
        site.DomainLimited = domains.Length > 0;

        try
        {
            site.Save();

            siteId = EditId.Value = site.Id;

            //SAVE DOMAINS
            BXSiteDomain.SaveList(site.Id, domains);

            //SAVE CONDITIONS
            BXTemplateCondition.Delete(new BXFilter(new BXFilterItem(BXTemplateCondition.Fields.SiteId, BXSqlFilterOperators.Equal, site.Id)));
            foreach (BXTemplateCondition cond in Conditions)
            {
                cond.SiteId = site.Id;
                cond.Save();
            }

            BXLoc.ResetCultureCache();
        }
        catch (Exception e)
        {
            errorMessage.AddErrorMessage(e.Message);
            saveSuccess = false;
        }

        if (saveSuccess && !isApply)
        {
            //zg, Bitrix, 2008.06.05
            //Response.Redirect("SiteAdmin.aspx");
            GoBack();
            //---
        }
        else
        {
            BindConditions();
        }
    }
    #endregion

    #region SiteId
    private string siteId;
    public string SiteId
    {
        get
        {
			if (siteId == null)
			{
				siteId = EditId.Value.Trim();
				if (String.IsNullOrEmpty(siteId))
					siteId = GetSiteId(Request.QueryString["id"]);
			}
            return siteId;
        }
    }
    private string GetSiteId(string requestSiteId)
    {
		if (!string.IsNullOrEmpty(requestSiteId))
        {
            BXSite s = BXSite.GetById(requestSiteId);
            if (s.IsNew)
                return string.Empty;
            else
                return requestSiteId;
        }

        return string.Empty;
    }
    #endregion

    #region Conditions
    /// <summary>
    /// Список всех условий.
    /// </summary>
    /// <value>Условия</value>
    private List<BXTemplateCondition> Conditions
    {
        get
        {
            List<BXTemplateCondition> conditions = new List<BXTemplateCondition>();

            foreach (HtmlTableRow row in ConditionTable.Rows)
            {
                if (row.Cells[2].Controls[0] is DropDownList)
                {
                    DropDownList ddlSender = row.Cells[2].Controls[0] as DropDownList;
                    BXTemplateCondition condition = new BXTemplateCondition();
                    int sort;
                    if (int.TryParse((row.Cells[1].Controls[0] as TextBox).Text, out sort))
                        condition.Sort = sort;

                    string template = (row.Cells[0].Controls[0] as DropDownList).SelectedValue;
                    if (string.IsNullOrEmpty(template))
                        continue;
                    else
                        condition.Template = template;

                    condition.Condition = "";
					condition.ConditionType = (int)ConditionType.None;

                    switch ((ConditionType)Enum.Parse(typeof(ConditionType), ddlSender.SelectedValue))
                    {
                        case ConditionType.FileOrFolder:
                            TextBox txt = row.Cells[3].FindControl(row.ID + "textbox") as TextBox;
                            condition.Condition = txt.Text;

                            if (string.IsNullOrEmpty(condition.Condition))
                                condition.ConditionType = (int)ConditionType.None;
                            else
                                condition.ConditionType = (int)ConditionType.FileOrFolder;

                            break;

                        case ConditionType.Group:
                            IRoleList rl = row.Cells[3].FindControl(row.ID + "rolelist") as IRoleList;
                            condition.Condition = BXStringUtility.ListToString(rl.SelectedRoles);

                            if (rl.SelectedRoles.Length == 0)
                                condition.ConditionType = (int)ConditionType.None;
                            else
                                condition.ConditionType = (int)ConditionType.Group;
                            break;

                        //TIME
                        case ConditionType.Time:

                            ITimeInterval ti = row.Cells[3].FindControl(row.ID + "timeinterval") as ITimeInterval;
							condition.Condition = BXStringUtility.TimeIntervalToString(ti.StartDate, ti.EndDate);
                            if (ti.StartDate.Equals(DateTime.MinValue) && ti.EndDate.Equals(DateTime.MaxValue))
                                condition.ConditionType = (int)ConditionType.None;
                            else
                                condition.ConditionType = (int)ConditionType.Time;
                            break;

                        case ConditionType.Url:
                            IUrlParameter up = row.Cells[3].FindControl(row.ID + "urlparameter") as IUrlParameter;
							condition.Condition = BXStringUtility.ParamToString(up.ParameterName, up.ParameterValue);
                            if (string.IsNullOrEmpty(up.ParameterName))
                                condition.ConditionType = (int)ConditionType.None;
                            else
                                condition.ConditionType = (int)ConditionType.Url;
                            break;
                        default:
                            break;
                    }

                    if (condition != null)
                        conditions.Add(condition);
                }
            }

            return conditions;
        }
    }
    #endregion

    #region SourceSiteId
    private string sourceSiteId;
    public string SourceSiteId
    {
        get
        {
            if (sourceSiteId == null)
                sourceSiteId = GetSiteId(Request.QueryString["copy"]);

            return sourceSiteId;
        }
    }
    #endregion

    #region Event Handlers
    protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
    {
        switch (e.CommandName.ToLower())
        {
            case "getlist": Response.Redirect("SiteAdmin.aspx"); break;
            case "new": Response.Redirect("SiteEdit.aspx"); break;
            case "copy":
                Response.Redirect("SiteEdit.aspx?copy=" + SiteId); break;
            case "delete":
				if (this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
				{
					BXSite lang = BXSite.GetById(SiteId);
					if (!lang.IsNew)
						lang.Delete();
					Response.Redirect("SiteAdmin.aspx");
				}
				else
				{
					errorMessage.AddErrorMessage(GetMessage("Exception.YouDontHaveRightsToPerformThisOperation"));
				}
                break;
            default:
                break;
        }
    }

   
    protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
    {
        switch (e.CommandName.ToLower())
        {
            case "save":
                if (ValidateFields())
                {
                    SaveSite(false);
                }
                break;
            case "apply":
                if (ValidateFields())
                {
                    SaveSite(true);
                } 
                break;
            case "cancel":
                //zg, Bitrix, 2008.06.05
                //Response.Redirect("SiteAdmin.aspx"); 
                GoBack();
                break;
            default:
                break;
        }
    }

    void conditiontypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        foreach (HtmlTableRow row in ConditionTable.Rows)
        {
            if (row.Cells[2].Controls[0] is DropDownList)
            {
                Control ctrl = row.Cells[3].FindControl(row.ID + "textbox");
                if (ctrl != null)
                    ctrl.Visible = false;

                ctrl = row.Cells[3].FindControl(row.ID + "rolelist");
                if (ctrl != null)
                    ctrl.Visible = false;

                ctrl = row.Cells[3].FindControl(row.ID + "urlparameter");
                if (ctrl != null)
                    ctrl.Visible = false;

                ctrl = row.Cells[3].FindControl(row.ID + "timeinterval");
                if (ctrl != null)
                    ctrl.Visible = false;

                DropDownList ddlSender = row.Cells[2].Controls[0] as DropDownList;
                if (ddlSender.SelectedIndex != 0)
                {
                    string controlPostfix = "";
                    switch ((ConditionType)Enum.Parse(typeof(ConditionType), ddlSender.SelectedValue))
                    {
                        case ConditionType.None:
                            break;
                        case ConditionType.FileOrFolder:
                            controlPostfix = "textbox";
                            break;
                        case ConditionType.Group:
                            controlPostfix = "rolelist";
                            break;
                        case ConditionType.Time:
                            controlPostfix = "timeinterval";
                            break;
                        case ConditionType.Url:
                            controlPostfix = "urlparameter";
                            break;
                        default:
                            break;
                    }

                    Control c = row.Cells[3].FindControl(row.ID + controlPostfix);
                    if (c != null)
                        c.Visible = true;
                }
            }
        }


    }
    #endregion
}
