using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Configuration;
using Bitrix.Security;
using Bitrix.DataTypes;
using Bitrix.UI.Hermitage;

public partial class bitrix_admin_UserSettings : BXAdminPage
{
	protected override void OnInit(EventArgs e)
	{
		if(!BXIdentity.Current.IsAuthenticated)
			BXAuthentication.AuthenticationRequired();		

		base.OnInit(e);
	}

	protected override void OnLoad(EventArgs e)
	{
		if(!IsPostBack)
		{
			BXHermitageUserSettings userSettings = new BXHermitageUserSettings(BXIdentity.Current.IsAuthenticated ? BXIdentity.Current.Id : 0);
			userSettings.Load();

			this.displayPageEditorToolbarChbx.Checked = userSettings.EnablePageEditControl;
		}
		this.useByDefault.Visible = BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
        MasterTitle = Page.Title = GetMessage("PageTitle");

		base.OnLoad(e);
	}

	protected override void OnPreRender(EventArgs e)
	{
		if(this.errorMessage.Length > 0)
			this.errorSummary.AddErrorMessage(this.errorMessage);
		base.OnPreRender(e);
	}

	private string errorMessage = string.Empty;
    protected string ErrorMessage
    {
        get { return this.errorMessage; }
    }

	private void TrySave()
	{
		try
		{
			if(!BXIdentity.Current.IsAuthenticated)
				throw new InvalidOperationException("User is not authenticated!");

			BXHermitageUserSettings userSettings = new BXHermitageUserSettings(this.useByDefault.Checked 
				&& BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage) ? 0 : BXIdentity.Current.Id);
			userSettings.Load();

			userSettings.EnablePageEditControl = this.displayPageEditorToolbarChbx.Checked;
			userSettings.Save();
		}
		catch(Exception ex)
		{
			this.errorMessage = ex.Message;
		}
	}

    protected void OnCommand(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
            case "save":
                {
                    if (IsValid)
                    {
                        TrySave();
						if(this.errorMessage.Length == 0)
							GoBack();
                    }
                }
                break;
            case "apply":
                {
                    if (IsValid)
                    {
						TrySave();
						if(this.errorMessage.Length == 0)
                            Response.Redirect("UserSettings.aspx");						
                    }
                }
                break;
			case "cancel":
				GoBack();
				break;
		}
	}
}
