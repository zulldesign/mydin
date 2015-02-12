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
using Bitrix.Modules;

public partial class bitrix_modules_DefaultUninstallWizard : BXModuleWizard
{
    protected void Page_Load(object sender, EventArgs e)
    {
		Page.Title = HttpUtility.HtmlEncode(string.Format(GetMessageRaw("PageTitle"), Module.Name ?? Module.ModuleId));
		BXAdminPage adminPage = Page as BXAdminPage;
		if (adminPage != null)
			adminPage.MasterTitle = Page.Title;
    }
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        OnCancel();
    }
    protected void btnUninstall_Click(object sender, EventArgs e)
    {
		try
		{
			if (cbDb.Checked)
				BXModuleManager.Uninstall(Module.GetInstaller(), BXModuleInstallOptions.All);
			else
				BXModuleManager.Uninstall(Module.GetInstaller(), BXModuleInstallOptions.All ^ BXModuleInstallOptions.Database);
		}
		catch (Exception ex)
		{
			OnError(new BXModuleWizardErrorEventArgs(ex));
			return;
		}

        OnFinish();
    }
}
