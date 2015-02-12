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
using Bitrix.UI;
using Bitrix.Modules;
using System.Collections.ObjectModel;

public partial class bitrix_admin_AuthOperationsEdit : BXAdminPage
{
	int operationId = -1;

	BXRoleOperation operation;

	bool currentUserCanModifySettings = false;

	private void LoadData()
	{
		ddlModuleId.Items.Clear();
		foreach (BXModule moduleTmp in BXModuleManager.InstalledModules)
		{
			ddlModuleId.Items.Add(new ListItem(moduleTmp.Name ?? moduleTmp.ModuleId, moduleTmp.ModuleId));
		}

		if (operation == null)
		{
			operationId = -1;
			hfOperationId.Value = operationId.ToString();
		}
		else
		{
			Page.Title = HttpUtility.HtmlEncode(string.Format(GetMessageRaw("FormattedPageTitle"), operationId));
			((BXAdminMasterPage)Page.Master).Title = Page.Title;

			tbOperationName.Text = operation.OperationName;
			tbComment.Text = operation.Comment;
			tbOperationType.Text = operation.OperationType;

			foreach (ListItem item in ddlModuleId.Items)
				if (item.Value.Equals(operation.ModuleId, StringComparison.InvariantCultureIgnoreCase))
					item.Selected = true;
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		operationId = base.GetRequestInt("id");
		hfOperationId.Value = Request.Form[hfOperationId.UniqueID];
		if (operationId > 0)
			hfOperationId.Value = operationId.ToString();
		Int32.TryParse(hfOperationId.Value, out operationId);

		if (operationId > 0)
		{
			operation = BXRoleOperationManager.GetById(operationId);
			if (operation == null)
			{
				operationId = 0;
				hfOperationId.Value = operationId.ToString();
			}
		}

		if (operationId <= 0)
		{
			string operationName = base.GetRequestString("name");
			if (!String.IsNullOrEmpty(operationName))
			{
				operation = BXRoleOperationManager.GetByName(operationName);
				if (operation != null)
				{
					operationId = operation.OperationId;
					hfOperationId.Value = operationId.ToString();
				}
			}
		}

		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifySettings = this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		if (!Page.IsPostBack)
			LoadData();
	}

	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		DeleteOperationSeparator.Visible = DeleteOperationButton.Visible = AddOperationSeparator.Visible = AddOperationButton.Visible = operationId > 0;
		BXTabControl1.ShowApplyButton = BXTabControl1.ShowSaveButton = currentUserCanModifySettings;
	}

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{
		bool noRedirect = false;
		bool successAction = true;
		if (e.CommandName == "save")
		{
			if (!SaveOperation())
			{
				successAction = false;
				noRedirect = true;
			}
		}
		else if (e.CommandName == "apply")
		{
			if (!SaveOperation())
				successAction = false;
			noRedirect = true;
		}

		if (!noRedirect)
		{
			Page.Response.Redirect("AuthOperationsList.aspx");
		}
		else
		{
			if (successAction)
			{
				successMessage.Visible = true;
				LoadData();
			}
		}
	}

	private bool SaveOperation()
	{
		if (Page.IsValid)
		{
			if (operationId > 0)
				return UpdateOperation();
			else
				return CreateOperation();
		}
		return false;
	}

	private bool CreateOperation()
	{
		try
		{
			if (!currentUserCanModifySettings)
				throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToCreateOperation"));

			operation = BXRoleOperationManager.Create(tbOperationName.Text, tbOperationType.Text, ddlModuleId.SelectedValue, tbComment.Text);
			if (operation == null)
				throw new Exception(GetMessageRaw("ExceptionText.CreationHasFailed"));

			operationId = operation.OperationId;
			hfOperationId.Value = operationId.ToString();

			return true;
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(e.Message);
		}
		return false;
	}

	private bool UpdateOperation()
	{
		try
		{
			if (!currentUserCanModifySettings)
				throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToModifyOperation"));

			operation.OperationName = tbOperationName.Text;
			operation.OperationType = tbOperationType.Text;
			operation.ModuleId = ddlModuleId.SelectedValue;
			operation.Comment = tbComment.Text;

			operation.Update();

			return true;
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(e.Message);
		}

		return false;
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "deleteoperation":
				try
				{
					if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage))
						throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteOperation"));

					if (operation != null)
					{
						if (!BXRoleOperationManager.Delete(operationId))
							throw new Exception(GetMessageRaw("ExceptionText.DeletionHasFailed"));
					}
					else
					{
						throw new Exception(GetMessageRaw("ExceptionText.OperationIsNotFound"));
					}
					Response.Redirect("AuthOperationsList.aspx");
				}
				catch (BXEventException ex)
				{
					foreach (string s in ex.Messages)
						errorMessage.AddErrorMessage(s);
				}
				catch (Exception ex)
				{
					errorMessage.AddErrorMessage(ex.Message);
				}

				break;

			default:
				break;
		}
	}
}
