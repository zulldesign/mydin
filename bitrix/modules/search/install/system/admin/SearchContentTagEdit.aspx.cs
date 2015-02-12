using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.Modules;
using Bitrix.Search;
using Bitrix.Security;
using Bitrix.Services.Text;
using Bitrix.UI;

public partial class bitrix_admin_SearchContentTagEdit : BXAdminPage
{
	bool canModify;
	
	public enum TagEditorMode
	{
		Creation = 1,
		Modification = 2
	}
	
	private int? tagId = null;
	protected int TagId
	{
		get
		{
			if (tagId.HasValue)
				return tagId.Value;

			string tagIdStr = Request.QueryString["id"];
			if (string.IsNullOrEmpty(tagIdStr))
				tagId = 0;
			else
			{
				int i;
				tagId = int.TryParse(Request.QueryString["id"], out i) ? i : 0;
			}
			return tagId.Value;
		}
	}

	private TagEditorMode editorMode = TagEditorMode.Creation;
	protected TagEditorMode EditorMode
	{
		get
		{
			return editorMode;
		}
	}

	private BXContentTag tag;
	protected BXContentTag Tag
	{
		get
		{
			return tag;
		}
	}

	private string _errorMessage = null;
	protected string ErrorMessage
	{
		get
		{
			return _errorMessage;
		}
	}

	protected override string BackUrl
	{
		get
		{
			return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "SearchContentTagList.aspx";
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();
		canModify = BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);

		TryLoadTag();
		if (Tag == null)
			TabControl.Visible = false;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		BXContentTag tag = Tag;
		string title = (tag != null && EditorMode == TagEditorMode.Modification) ? string.Format(GetMessageRaw("PageTitle.Edit"), tag.Name) : GetMessageRaw("PageTitle.Create");

		MasterTitle = title;
		Page.Title = title;

	}

	private void TryLoadTag()
	{
		int id = TagId;
		if (id <= 0)
		{
			editorMode = TagEditorMode.Creation;
			tag = new BXContentTag();
		}
		else
		{
			editorMode = TagEditorMode.Modification;
			BXContentTagQuery q = new BXContentTagQuery();
			q.Filter = new BXContentTagFilterItem(BXContentTagField.Id, BXSqlFilterOperators.Equal, id);
			q.SelectLastUpdate = true;
			q.SelectTagCount = true;
			BXContentTagCollection tags = q.Execute();
			tag = tags.Count != 0 ? tags[0] : null;

			if (tag == null)
			{
				_errorMessage = GetMessageRaw("Error.NotFound");
				return;
			}
		}

		if (editorMode == TagEditorMode.Modification)
			Status.SelectedValue = tag.Status.ToString();
		else
		{
			Name.Text = "";
			Status.SelectedValue = "Approved";
		}
	}

	private void TrySaveTag()
	{
		if (tag == null)
			return;
		try
		{
			if (editorMode == TagEditorMode.Creation)
				tag.Name = Name.Text;
			try
			{
				tag.Status = (BXContentTagStatus)Enum.Parse(typeof(BXContentTagStatus), Status.SelectedValue);
			}
			catch
			{
				tag.Status = BXContentTagStatus.Unknown;
			}
			
			tag.Save();
			tagId = tag.Id;
		}
		catch (Exception exc)
		{
			_errorMessage = exc.Message;
		}
	}

	protected override void OnPreRender(EventArgs e)
	{
		if (ErrorMessage != null)
			errorMessage.AddErrorMessage(ErrorMessage);

		AddButton.Visible = canModify && EditorMode == TagEditorMode.Modification;
		DeleteButton.Visible = canModify && EditorMode == TagEditorMode.Modification && tag != null;
		TabControl.ShowApplyButton = TabControl.ShowSaveButton = canModify;

		base.OnPreRender(e);
	}


	protected void OnTagEdit(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "save":
				{
					if (canModify && IsValid && ErrorMessage == null)
					{
						TrySaveTag();
						if (ErrorMessage == null)
							GoBack();
					}
				}
				break;
			case "apply":
				{
					if (canModify && IsValid && ErrorMessage == null)
					{
						TrySaveTag();
						if (ErrorMessage == null)
							Response.Redirect(string.Format("SearchContentTagEdit.aspx?id={0}{1}", TagId, TabControl.SelectedIndex > 0 ? ("&tabindex=" + TabControl.SelectedIndex) : ""));
					}
				}
				break;
			case "cancel":
				GoBack();
				break;
		}
	}

	protected void OnToolBarButtonClick(object sender, CommandEventArgs e)
	{
		if (e.CommandName == "delete")
		{
			try
			{
				if (canModify)
				{
					BXContentTag tag = Tag;
					if (tag != null)
						tag.Delete();
				}
				GoBack();
			}
			catch (Exception ex)
			{
				_errorMessage = ex.Message;
			}
		}
	}
}
