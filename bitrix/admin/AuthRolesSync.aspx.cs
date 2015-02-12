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
using Bitrix.Security;
using Bitrix.Modules;
using Bitrix.UI;

public partial class bitrix_admin_AuthRolesSync : BXAdminPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		Page.Title = GetMessage("PageTitle.SynchronizationOfRoleList");
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsView))
			BXAuthentication.AuthenticationRequired();

		BXTabControl1.ShowSaveButton = BXTabControl1.ShowApplyButton = BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage);

		if (!Page.IsPostBack)
		{
			lbSyncRoles.Items.Clear();
			foreach (RoleProvider p in Roles.Providers)
				if(!(p is BXSqlRoleProvider))
					lbSyncRoles.Items.Add(new ListItem(p.Name));

			lblSyncRolesStub.Visible = !(lbSyncRoles.Visible = lbSyncRoles.Items.Count > 0);
		}
	}

	protected void BXTabControl1_Command(object sender, Bitrix.UI.BXTabControlCommandEventArgs e)
	{
		bool noRedirect = false;
		bool successAction = true;
		if (e.CommandName == "save")
		{
			if (!SyncRoles())
			{
				successAction = false;
				noRedirect = true;
			}
		}
		else if (e.CommandName == "apply")
		{
			if (!SyncRoles())
				successAction = false;
			noRedirect = true;
		}

		if (!noRedirect)
		{
			Page.Response.Redirect("AuthRolesList.aspx");
		}
		else
		{
			if (successAction)
			{
				successMessage.Visible = true;
			}
		}
	}

	private bool SyncRoles()
	{
		try
		{
			if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage))
				throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteThisRecord"));

			foreach (ListItem item in lbSyncRoles.Items)
				if (item.Selected)
					BXRoleManager.SynchronizeProviderRoles(item.Value);

			return true;
		}
		catch (BXEventException e)
		{
			foreach (string s in e.Messages)
				errorMessage.AddErrorMessage(s);
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(e.Message);
		}
		return false;
	}
}
