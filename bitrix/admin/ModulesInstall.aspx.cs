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
using Bitrix.Modules;
using System.Collections.Generic;
using System.IO;

using Bitrix.Services;

using Bitrix.UI;
using Bitrix.IO;
using Bitrix.Security;
using System.Collections.ObjectModel;

public partial class bitrix_admin_ModulesInstall : Bitrix.UI.BXAdminPage
{
    string action;
    string module;

	protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
			BXAuthentication.AuthenticationRequired();

        action = Request.QueryString["action"];
        module = Request.QueryString["module"];

        if(String.IsNullOrEmpty(action) 
			|| String.IsNullOrEmpty(module)
			|| String.Equals(module, "main", StringComparison.OrdinalIgnoreCase)
			|| !BXCsrfToken.CheckTokenFromRequest(Request.QueryString))
            Response.Redirect("~/bitrix/admin/modules.aspx");

		string pageTitle;
		BXModule currentModule;
        switch (action.ToLower())
        {
            case "install" :

				errorMessage.Title = GetMessageRaw("InstallErrorTitle");
				pageTitle = GetMessageRaw("InstallPageTitle");
				MasterTitle = pageTitle;
				Title = pageTitle;

				try
				{
					currentModule = BXModuleManager.LoadModule(module);

					pageTitle = String.Concat(pageTitle, " ", currentModule.Name);
					MasterTitle = pageTitle;
					Title = pageTitle; 

					if (File.Exists(BXPath.MapPath(string.Format("~/bitrix/modules/{0}/install/install.ascx", module))))
					{
						BXModuleWizard ctrl = (BXModuleWizard)LoadControl(string.Format("~/bitrix/modules/{0}/install/install.ascx", module));
						if (ctrl != null)
						{
							ctrl.Finish += new EventHandler(Finish);
							ctrl.Cancel += new EventHandler(Cancel);
							ctrl.Error += new EventHandler<BXModuleWizardErrorEventArgs>(Error);
							ctrl.ID = "WIZARD";
							ctrl.module = currentModule;
							phWizard.Controls.Add(ctrl);
							break;
						}

					}

					BXModuleInstaller installer = currentModule.GetInstaller();
					BXModuleManager.Install(installer, BXModuleInstallOptions.All);
					Bitrix.Activation.LicenseManager.Refresh();
					Response.Redirect("~/bitrix/admin/modules.aspx");
				}
				catch (Exception ex)
				{
					errorMessage.Visible = true;
					errorMessage.Content = ex.Message;
				}
                
                break;

            case "uninstall":

				pageTitle = GetMessageRaw("UnstallPageTitle");
				errorMessage.Title = GetMessageRaw("UnstallErrorTitle");

				MasterTitle = pageTitle;
				Title = pageTitle;

				currentModule = BXModuleFactory.GetFactory().GetModule(module);
				Collection<BXModuleDependency> dependencies = BXModuleManager.CheckBeforeUninstall(module);

				if (currentModule == null || (dependencies != null && dependencies.Count > 0))
				{
					errorMessage.Visible = true;
					

					foreach (BXModuleDependency dependency in dependencies)
						errorMessage.Content += dependency.ValidationError + "<br />";

					btnCancel.Visible = true;
					break;
				}

				pageTitle = String.Concat(pageTitle, " ", currentModule.Name);
				MasterTitle = pageTitle;
				Title = pageTitle;

				try
				{
					if (File.Exists(BXPath.MapPath(string.Format("~/bitrix/modules/{0}/uninstall/uninstall.ascx", module))))
					{
						BXModuleWizard ctrl = (BXModuleWizard)LoadControl(String.Format("~/bitrix/modules/{0}/uninstall/uninstall.ascx", module));
						if (ctrl != null)
						{
							ctrl.Finish += new EventHandler(UnFinish);
							ctrl.Cancel += new EventHandler(UnCancel);
							ctrl.Error += new EventHandler<BXModuleWizardErrorEventArgs>(Error);
							ctrl.ID = "WIZARD";
							ctrl.module = currentModule;
							phWizard.Controls.Add(ctrl);
							break;
						}

					}
					else
					{
						BXModuleWizard ctrl = (BXModuleWizard)LoadControl("~/bitrix/modules/uninstall.ascx");
						if (ctrl != null)
						{
							ctrl.Finish += new EventHandler(UnFinish);
							ctrl.Cancel += new EventHandler(UnCancel);
							ctrl.Error += new EventHandler<BXModuleWizardErrorEventArgs>(Error);
							ctrl.ID = "WIZARD";
							ctrl.module = currentModule;
							phWizard.Controls.Add(ctrl);
						}
					}

				}
				catch (Exception ex)
				{
					errorMessage.Visible = true;
					errorMessage.Content = ex.Message;
				}

                break;

            default:
                break;
        }
    }

	protected void UnCancel(object sender, EventArgs e)
    {
        Response.Redirect(AppRelativeVirtualPath);
    }

	protected void UnFinish(object sender, EventArgs e)
    {
		Bitrix.Activation.LicenseManager.Refresh();
        Response.Redirect(AppRelativeVirtualPath);
    }

    protected void Cancel(object sender, EventArgs e)
    {
        Response.Redirect(AppRelativeVirtualPath);
    }

    protected void Finish(object sender, EventArgs e)
    {
		Bitrix.Activation.LicenseManager.Refresh();
        Response.Redirect(AppRelativeVirtualPath);
    }

	protected void Error(object sender, BXModuleWizardErrorEventArgs args)
	{
		errorMessage.Visible = true;
		if (args != null && args.Error != null)
			errorMessage.Content = args.Error.Message;
	}

    protected void Page_Load(object sender, EventArgs e)
    {




    }
}
