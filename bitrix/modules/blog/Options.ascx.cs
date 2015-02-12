using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.Configuration;
using Bitrix.Blog;
using Bitrix.Services;

public enum BlogModuleOptionsEditorError
{
    None = 0,
    DataSavingFailed = -1
}

public enum BlogModuleOptionsEditorCommand
{
    Save = 1,
    Apply
}

public partial class bitrix_modules_Blog_Options : BXControl
{
    private bool? _isAuthorized = null;
    public bool IsAuthorized
    {
        get  { return (_isAuthorized ?? (_isAuthorized = BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))).Value; }
    }

    private string _backUrl = null;
    public string BackUrl
    {
        get { return _backUrl ?? (_backUrl = Request.QueryString[BXConfigurationUtility.Constants.BackUrl] ?? BXSefUrlManager.CurrentUrl.ToString()); }
    }

    private BlogModuleOptionsEditorError _editorError = BlogModuleOptionsEditorError.None;
    public BlogModuleOptionsEditorError EditorError
    {
        get { return _editorError; }
        private set { _editorError = value; }
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        tabContainer.ShowApplyButton = tabContainer.ShowSaveButton = IsAuthorized;
        tabContainer.ShowCancelButton = BackUrl.Length > 0;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!IsPostBack)
            LoadData();
        else
            Page.Validate();
    }

    protected void OnTabCommand(object sender, BXTabControlCommandEventArgs e)
    {
        BlogModuleOptionsEditorCommand? cmd = null;

        if (string.Equals(e.CommandName, "save", StringComparison.Ordinal))
            cmd = BlogModuleOptionsEditorCommand.Save;
        else if (string.Equals(e.CommandName, "apply", StringComparison.Ordinal))
            cmd = BlogModuleOptionsEditorCommand.Apply;
		
        if (!cmd.HasValue)
            Response.Redirect(BackUrl);

        if (Page.IsValid && (cmd.Value == BlogModuleOptionsEditorCommand.Apply || cmd.Value == BlogModuleOptionsEditorCommand.Save))
            SaveData();

        if (EditorError == BlogModuleOptionsEditorError.None)
        {
            if (cmd.Value == BlogModuleOptionsEditorCommand.Save && BackUrl.Length > 0)
                Page.Response.Redirect(BackUrl);
            else
            {
                LoadData();
                successMessage.Visible = true;
            }
        }
    }

    private void SaveData()
    {
        string refreshIntervalStr = tbxBlogSyndicationRefreshIntervalInMin.Text.Trim();
        int refreshInterval = 0;
        if (refreshIntervalStr.Length > 0)
            try
            {
                refreshInterval = Convert.ToInt32(refreshIntervalStr);
            }
            catch
            {
            }

        try
        {
            BXBlogModuleConfiguration.BlogSyndicationRefreshIntermalInMin = refreshInterval;
        }
        catch (Exception exc)
        {
            EditorError = BlogModuleOptionsEditorError.DataSavingFailed;
            errorMessage.AddErrorMessage(Encode(exc.Message));
        }
    }

    private void LoadData()
    {
        tbxBlogSyndicationRefreshIntervalInMin.Text = BXBlogModuleConfiguration.BlogSyndicationRefreshIntermalInMin.ToString();
    }
}
