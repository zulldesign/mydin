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
using System.Collections.Generic;
using Bitrix.UI;
using Bitrix.Forum;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix;
using System.Text;
using Bitrix.Security;

public partial class BXForumAdminPageForumList : BXAdminPage
{

	protected struct Category
	{
		public Category(int id, string name)
		{
			this.id = id;
			this.name = name;
		}

		private int id;
		public int ID
		{
			get { return id;}
			set { id = value; }
		}

		private string name;
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
	}

	private List<Category> categories;
	protected List<Category> Categories
	{
		get { return categories; }
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		bool allowManage = false;
		if (BXPrincipal.Current.IsCanOperate(BXForum.Operations.ForumAdminManage))
			allowManage = true;
		else
		{
			int forumsToManage = BXForum.Count(
				new BXFilter(
					new BXFilterItem(BXForum.Fields.CheckPermissions, BXSqlFilterOperators.Equal, new string[] { BXForum.Operations.ForumAdminManage })
				)
			);

			allowManage = forumsToManage > 0;

			AddButton.Visible = false;
		}


		if (!allowManage)
			BXAuthentication.AuthenticationRequired();

		categories = new List<Category>();
		categories.Add(new Category(0, GetMessage("Option.NotSet")));
		BXForumCategoryCollection categoryCollection = BXForumCategory.GetList(null, new BXOrderBy(new BXOrderByPair(BXForumCategory.Fields.Sort, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
		foreach (BXForumCategory category in categoryCollection)
		{
			categories.Add(new Category(category.Id, category.Name));
			categoryIdFilter.Values.Add(new ListItem(category.Name, category.Id.ToString()));
		}

		BXSiteCollection siteColl = BXSite.GetList(null, null, null, null, BXTextEncoder.EmptyTextEncoder);
		foreach (BXSite site in siteColl)
			siteIdFilter.Values.Add(new ListItem(site.Name, site.Id));

		MasterTitle = GetMessage("PageTitle");
	}

	protected void BXForumGrid_Select(object sender, BXSelectEventArgs e)
	{
		BXFilter filter = new BXFilter(new BXFilterItem(BXForum.Fields.CheckPermissions, BXSqlFilterOperators.Equal, new string[] { BXForum.Operations.ForumAdminManage }));
		filter.Add(new BXFilter(BXForumFilter.CurrentFilter, BXForum.Fields));

		BXOrderBy orderBy;

		if (String.IsNullOrEmpty(e.SortExpression))
			orderBy = new BXOrderBy(
				new BXOrderByPair(BXForum.Fields.Category.Sort, BXOrderByDirection.Asc),
				new BXOrderByPair(BXForum.Fields.Category.Id, BXOrderByDirection.Asc),
				new BXOrderByPair(BXForum.Fields.Sort, BXOrderByDirection.Asc),
				new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)
			);
		else
			orderBy = new BXOrderBy(BXForum.Fields, e.SortExpression);

		e.Data = BXForum.GetList(
			filter,
			orderBy,
			null, 
			new BXQueryParams(e.PagingOptions),
			BXTextEncoder.EmptyTextEncoder
		);
	}
	protected void BXForumGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BXForum.Count(new BXFilter(BXForumFilter.CurrentFilter, BXForum.Fields));
	}

	protected void BXForumGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
	{
		GridView grid = (GridView)sender;
		DropDownList dropDown = grid.Rows[e.RowIndex].FindControl("categoryIdEdit") as DropDownList;

		int categoryId;
		if (dropDown != null && int.TryParse(dropDown.SelectedValue, out categoryId))
			e.NewValues["CategoryId"] = categoryId;
	}

	protected void BXForumGrid_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;

		BXGridViewRow row = (BXGridViewRow)e.Row;
		BXForum forum = (BXForum)row.DataItem;

		row.UserData.Add("ID", forum.Id);

		if ((row.RowState & DataControlRowState.Edit) > 0)
		{
			DropDownList dropDown = row.FindControl("categoryIdEdit") as DropDownList;
			if (dropDown != null)
				dropDown.SelectedValue = forum.CategoryId.ToString();
		}
	}

	protected void BXForumGrid_Update(object sender, BXUpdateEventArgs e)
	{
		int forumID = (int)e.Keys["ID"];
		if (forumID < 1)
			return;

		try
		{
			BXForum forum = BXForum.GetById(forumID);
			if (forum == null || !BXPrincipal.Current.IsCanOperate(BXForum.Operations.ForumAdminManage, "forum", forum.Id))
				return;

			if (e.NewValues.Contains("Active"))
				forum.Active = (bool)e.NewValues["Active"];

			if (e.NewValues.Contains("Name") && !BXStringUtility.IsNullOrTrimEmpty((string)e.NewValues["Name"]))
				forum.Name = (string)e.NewValues["Name"];

			if (e.NewValues.Contains("Description"))
				forum.Description = (string)e.NewValues["Description"];

			if (e.NewValues.Contains("CategoryId"))
				forum.CategoryId = (int)e.NewValues["CategoryId"];

			int sortIndex;
			if (e.NewValues.Contains("Sort") && int.TryParse((string)e.NewValues["Sort"], out sortIndex))
				forum.Sort = sortIndex;

			if (e.NewValues.Contains("AllowBBCode"))
				forum.AllowBBCode = (bool)e.NewValues["AllowBBCode"];

			if (e.NewValues.Contains("AllowSmiles"))
				forum.AllowSmiles = (bool)e.NewValues["AllowSmiles"];

			if (e.NewValues.Contains("IndexContent"))
				forum.IndexContent = (bool)e.NewValues["IndexContent"];

			if (e.NewValues.Contains("Code"))
				forum.Code = (string)e.NewValues["Code"];

			if (e.NewValues.Contains("XmlId"))
				forum.XmlId = (string)e.NewValues["XmlId"];

			forum.Update();
		}
		catch (Exception ex)
		{
			ErrorMessage.AddErrorMessage(ex.Message);
		}
	}

	protected void BXForumGrid_Delete(object sender, BXDeleteEventArgs e)
	{
		BXForumCollection forums;
		if (e.Keys != null)
		{
			forums = new BXForumCollection();
			forums.Add(BXForum.GetById(e.Keys["ID"]));
		}
		else
			forums = BXForum.GetList(new BXFilter(BXForumFilter.CurrentFilter, BXForum.Fields), null);

		foreach (BXForum forum in forums)
		{
			if (forum == null || !BXPrincipal.Current.IsCanOperate(BXForum.Operations.ForumAdminManage, "forum", forum.Id))
				continue;
			try
			{
				forum.Delete();
			}
			catch (Exception ex)
			{
				ErrorMessage.AddErrorMessage(ex.Message);
			}
			e.DeletedCount++;
		}
	}

	protected string GetCategoryName(object dataItem)
	{
		string result = "&nbsp;";
		BXForum forum = dataItem as BXForum;
		if (forum != null && forum.CategoryId > 0)
		{
			int index = Categories.FindIndex(delegate(Category category) { return category.ID == forum.CategoryId; });
			if (index != -1)
				result = HttpUtility.HtmlEncode(Categories[index].Name);
		}
		return result;
	}

	protected string GetSites(object dataItem)
	{
		StringBuilder result = new StringBuilder(String.Empty);

		BXForum forum = dataItem as BXForum;
		if (forum != null)
		{
			BXForum.BXForumSiteCollection sites = forum.Sites;
			if (sites != null)
			{
				string[] ids = new string[sites.Count];
				for (int i = 0; i < sites.Count; i++)
					ids[i] = sites[i].SiteId;

				BXSiteCollection siteCollection = BXSite.GetList(new BXFilter(new BXFilterItem(BXSite.Fields.ID, BXSqlFilterOperators.In, ids)), null);
				foreach (BXSite site in siteCollection)
				{
					result.Append(site.Name);
					result.Append("<br />");
				}
			}
		}

		return result.ToString();
	}
}
