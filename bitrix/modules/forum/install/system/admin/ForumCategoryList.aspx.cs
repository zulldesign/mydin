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
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Security;

public partial class BXForumAdminPageCategoryList : BXAdminPage
{

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXForum.Operations.ForumAdminManage))
			BXAuthentication.AuthenticationRequired();
	}
	
    protected void Page_Load(object sender, EventArgs e)
    {
		MasterTitle = GetMessage("PageTitle");
    }

	protected void BXForumCategoryGrid_Select(object sender, BXSelectEventArgs e)
	{
		e.Data = BXForumCategory.GetList(
			new BXFilter(BXForumCategoryFilter.CurrentFilter, BXForumCategory.Fields),
			new BXOrderBy(BXForumCategory.Fields, String.IsNullOrEmpty(e.SortExpression) ? "Sort" : e.SortExpression),
			null,
			new BXQueryParams(e.PagingOptions),
			BXTextEncoder.EmptyTextEncoder
		);
	}
	protected void BXForumCategoryGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BXForumCategory.Count(new BXFilter(BXForumCategoryFilter.CurrentFilter, BXForumCategory.Fields));
	}

	protected void BXForumCategoryGrid_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;

		BXGridViewRow row = (BXGridViewRow)e.Row;
		BXForumCategory category = (BXForumCategory)row.DataItem;

		row.UserData.Add("ID", category.Id);
	}

	protected void BXForumCategoryGrid_Update(object sender, BXUpdateEventArgs e)
	{
		int categoryID = (int)e.Keys["ID"];
		if (categoryID < 1)
			return;

		try
		{
			BXForumCategory category = BXForumCategory.GetById(categoryID);
			if (category == null)
				return;

			if (e.NewValues.Contains("Name") && !BXStringUtility.IsNullOrTrimEmpty((string)e.NewValues["Name"]))
				category.Name = (string)e.NewValues["Name"];

			int sortIndex;
			if (e.NewValues.Contains("Sort") && int.TryParse((string)e.NewValues["Sort"], out sortIndex))
				category.Sort = sortIndex;

			if (e.NewValues.Contains("XmlId"))
				category.XmlId = (string)e.NewValues["XmlId"];

			category.Update();
		}
		catch (Exception ex)
		{
			ErrorMessage.AddErrorMessage(ex.Message);
		}
	}

	protected void BXForumCategoryGrid_Delete(object sender, BXDeleteEventArgs e)
	{
		try
		{
			if (e.Keys != null)
				BXForumCategory.Delete(e.Keys["ID"]);
			else
				BXForumCategory.Delete(null);

			e.DeletedCount++;
		}
		catch (Exception ex)
		{
			ErrorMessage.AddErrorMessage(ex.Message);
		}
	}
}
