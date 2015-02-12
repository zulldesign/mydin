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
using System.Collections.Generic;
using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.Configuration;
using Bitrix.Security;
using Bitrix.Modules;


public partial class bitrix_CustomField : BXAdminPage
{
	bool currentUserCanModifySettings;
	protected string GetTypeName(string typeId)
	{
		BXCustomType type = BXCustomTypeManager.GetCustomType(typeId);
		if (type != null)
			return type.Title;

		return GetMessageRaw("FilterText.Undefined");
	}

	protected string GetFilterVisibility(BXCustomFieldFilterVisibility visibility)
	{
		switch (visibility)
		{
			case BXCustomFieldFilterVisibility.Hidden:
				return GetMessageRaw("FilterText.DontDisplay");
			case BXCustomFieldFilterVisibility.CompleteMatch:
				return GetMessageRaw("FilterText.ExactFit");
			case BXCustomFieldFilterVisibility.MaskMatch:
				return GetMessageRaw("FilterText.MaskSearch");
			case BXCustomFieldFilterVisibility.SubstringMatch:
				return GetMessageRaw("FilterText.SubstringSearch");
		}
		return GetMessageRaw("FilterText.Undefined");
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();
		currentUserCanModifySettings = BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		AddButton.Visible = currentUserCanModifySettings;
		if (!currentUserCanModifySettings)
			BXGridView1.PopupCommandMenuId = PopupPanelView.ID;
		foreach (KeyValuePair<string, BXCustomType> t in BXCustomTypeManager.CustomTypes)
			CustomTypeFilter.Values.Add(new ListItem(t.Value.Title, t.Key));

		foreach (BXCustomFieldFilterVisibility v in Enum.GetValues(typeof(BXCustomFieldFilterVisibility)))
			ShowInFilter.Values.Add(new ListItem(GetFilterVisibility(v), ((int)v).ToString()));
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		MasterTitle = Title;
		
	}
	protected void BXGridView1_Select(object sender, BXSelectEventArgs e)
	{
		e.Data = BXCustomField.GetList(
			new BXFilter(BXAdminFilter1.CurrentFilter, BXCustomField.Fields),
			new BXOrderBy(BXCustomField.Fields, e.SortExpression),
			null,
			new BXQueryParams(e.PagingOptions)
		);
	}
	protected void BXGridView1_Delete(object sender, BXDeleteEventArgs e)
	{
		try
		{
			if (!currentUserCanModifySettings)
				throw new PublicException(GetMessageRaw("InsufficientRightsToDeleteCustomFields"));
			BXCustomFieldCollection fields;
			if (e.Keys != null)
			{
				fields = new BXCustomFieldCollection();
				fields.Add(BXCustomField.GetById(e.Keys["Id"]));
			}
			else
				fields = BXCustomField.GetList(new BXFilter(BXAdminFilter1.CurrentFilter, BXCustomField.Fields), null);

			foreach(BXCustomField f in fields)
				f.Delete();
		}
		catch(Exception ex)
		{
			ProcessException(ex);
		}
	}
	protected void BXGridView1_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BXCustomField.Count(new BXFilter(BXAdminFilter1.CurrentFilter, BXCustomField.Fields));
	}

	protected void BXGridView1_PopupMenuClick1(object sender, BXPopupMenuClickEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "edit":
				Response.Redirect(string.Format("~/bitrix/admin/CustomFieldEdit.aspx?id={0}&{1}={2}", BXGridView1.DataKeys[e.EventRowIndex].Value, BXConfigurationUtility.Constants.BackUrl, Request.RawUrl));
				break;
		}
	}
	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "add":
				Response.Redirect(string.Format("~/bitrix/admin/CustomFieldEdit.aspx?{0}={1}", BXConfigurationUtility.Constants.BackUrl, Request.RawUrl));
				break;
		}
	}

	protected void BXDropDownKeyTextBoxFilter1_CustomBuildFilter(object sender, BXDropDownKeyTextBoxFilter.BuildFilterEventArgs e)
	{
		e.FilterItems.Clear();
		switch (e.KeyValue)
		{
			case "Id":
				int id;
				if (int.TryParse(e.TextBoxValue, out id))
					e.FilterItems.Add(new BXFormFilterItem("Id", id, BXSqlFilterOperators.Equal));
				break;
			case "OwnerEntityId":
				if (!string.IsNullOrEmpty(e.TextBoxValue))
					e.FilterItems.Add(new BXFormFilterItem("OwnerEntityId", e.TextBoxValue, BXSqlFilterOperators.Like));
				break;
		}
	}
	private void ProcessException(Exception ex)
	{
		if (ex is PublicException)
		{
			errorMessage.AddErrorMessage(Encode(ex.Message));
			if (ex.InnerException != null)
				BXLogService.LogAll(ex.InnerException, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
			return;
		}

		BXEventException eventException = ex as BXEventException;
		if (eventException != null)
		{
			foreach (string msg in eventException.Messages)
				errorMessage.AddErrorMessage(Encode(msg));
			return;
		}

		errorMessage.AddErrorMessage(GetMessage("Kernel.UnknownError"));
		BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
	}
}
