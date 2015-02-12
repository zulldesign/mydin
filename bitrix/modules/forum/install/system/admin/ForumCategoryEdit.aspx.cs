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
using Bitrix.Forum;
using Bitrix.Services.Text;
using Bitrix.Security;

public partial class BXForumAdminPageCategoryEdit : BXAdminPage
{
	private BXForumCategory category;
	private int id = 0;
	protected int Id
	{
		get { return id; }
		set { id = value; }
	}

	protected override string BackUrl
	{
		get
		{
			return base.BackUrl ?? "ForumCategoryList.aspx";
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXForum.Operations.ForumAdminManage))
			BXAuthentication.AuthenticationRequired();

		LoadCategoryData();
	}

	private void LoadCategoryData()
	{
		int requestId;
		if (int.TryParse(Request.QueryString["id"], out requestId) && requestId > 0)
			id = requestId;

		if (id > 0)
		{
			category = BXForumCategory.GetById(Id, BXTextEncoder.EmptyTextEncoder);
			if (category == null)
			{
				errorMessage.AddErrorMessage(GetMessage("Error.CategoryNotFound"));
				mainTabControl.Visible = false;
				return;
			}

			CategoryName.Text = category.Name;
			CategorySort.Text = category.Sort.ToString();
			CategoryXmlId.Text = category.XmlId;
		}
		else
		{
			CategorySort.Text = "10";
		}
	}

	private bool SaveCategoryData()
	{
		if (!Page.IsValid)
			return false;

		bool result = false;
		try
		{

			if (category == null)
				category = new BXForumCategory();

			category.Name = CategoryName.Text;
			category.XmlId = CategoryXmlId.Text;

			int sort;
			if (int.TryParse(CategorySort.Text, out sort))
				category.Sort = sort;

			category.Save();

			id = category.Id;
			result = true;
		}
		catch (Exception ex)
		{
			errorMessage.AddErrorMessage(ex.Message);
		}

		return result;
	}


    protected void Page_Load(object sender, EventArgs e)
    {
		if (category == null)
		{
			AddButton.Visible = false;
			DeleteButton.Visible = false;
		}

		string title = category != null ? String.Format(GetMessage("EditPageTitle"), HttpUtility.HtmlEncode(category.Name)) : GetMessage("CreatePageTitle");

		MasterTitle = title;
		Page.Title = title;
    }

	protected void OnForumCategoryEdit(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "save":
				if (SaveCategoryData())
					GoBack();
				break;
			case "apply":
				if (SaveCategoryData())
					Response.Redirect("ForumCategoryEdit.aspx?id="+ id.ToString());
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
				if (category != null)
					category.Delete();

				GoBack();
			}
			catch (Exception ex)
			{
				errorMessage.AddErrorMessage(ex.Message);
			}
		}
	}
}
