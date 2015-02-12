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
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix;
using System.Text;
using Bitrix.Security;
using Bitrix.Search;
using Bitrix.Modules;

public partial class bitrix_admin_SearchContentTagList : BXAdminPage
{
	bool canModify;
	Dictionary<string, string> siteNames;
	BXContentTagQuery query;

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();
		canModify = BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);

		ItemGrid.AddColumnKeys(4, "LastUpdate");
	}
	protected override void OnLoad(EventArgs e)
	{
		MasterTitle = GetMessage("PageTitle");
		Page.Title = GetMessage("PageTitle");
		
		AddButton.Visible = canModify;
		MultiActionMenuToolbar.Visible = canModify;

		base.OnLoad(e);
	}

	protected void ItemGrid_Select(object sender, BXSelectEventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			return;

		BXContentTagQuery query = BuildTagQuery();
		query.OrderBy = new BXContentTagOrderBy(string.IsNullOrEmpty(e.SortExpression) ? "Name" : e.SortExpression);
		query.PagingOptions = e.PagingOptions;
		query.TextEncoder = BXTextEncoder.EmptyTextEncoder;

		e.Data = query.Execute();
	}

	protected void ItemGrid_Update(object sender, BXUpdateEventArgs e)
	{
		if (!canModify)
			return;
		int elementID = (int)e.Keys["Id"];
		if (elementID <= 0)
			return;

		try
		{
			BXContentTag tag = BXContentTag.GetById(e.Keys["Id"]);
			if (tag == null)
				return;

			if (e.NewValues.Contains("Status"))
			{
				try
				{
					tag.Status = (BXContentTagStatus)Enum.Parse(typeof(BXContentTagStatus), (string)e.NewValues["Status"]);
				}
				catch
				{
				}
			}
			tag.Save();
		}
		catch (BXEventException exception)
		{
			foreach (string s in exception.Messages)
				ErrorMessage.AddErrorMessage(Encode(s));
		}
		catch (Exception exception)
		{
			ErrorMessage.AddErrorMessage(Encode(exception.Message));
		}
	}

	private BXContentTagQuery BuildTagQuery()
	{
		//if (query != null)
		//	return query;

		BXContentTagQuery query = new BXContentTagQuery();
		BXContentTagGroupFilter tf = null;

		foreach (BXFormFilterItem i in ItemFilter.CurrentFilter)
		{
			if (!i.filterName.StartsWith("Tag."))
				continue;

			tf = tf ?? new BXContentTagGroupFilter(BXFilterExpressionCombiningLogic.And);
			tf.Add(new BXContentTagFilterItem(
				(BXContentTagField)Enum.Parse(typeof(BXContentTagField), i.filterName.Substring("Tag.".Length), true), 
				i.filterOperator, 
				i.filterValue
			));
		}
		query.Filter = tf;
		if (Array.IndexOf(ItemGrid.GetVisibleColumnsKeys(), "TagCount") != -1)
			query.SelectTagCount = true;
		if (Array.IndexOf(ItemGrid.GetVisibleColumnsKeys(), "LastUpdate") != -1)
			query.SelectLastUpdate = true;

		//BXSearchQuery sq = new BXSearchQuery();
		//sq.CalculateRelevance = false;
		//sq.CheckPermissions = false;
		//sq.SiteId = "";
		//sq.LanguageId = "";
		//query.SearchQuery = sq;

		return query;
	}
	protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BuildTagQuery().ExecuteCount();
	}
	protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;

		BXGridViewRow row = (BXGridViewRow)e.Row;
		BXContentTag tag = (BXContentTag)row.DataItem;

		row.UserData.Add("Id", tag.Id);
		row.AllowedCommandsList = 
			canModify 
			? new string[] { "edit", "delete" }
			: new string[] { "view" };
	}
	protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
	{
		if (!canModify)
			return;

		BXContentTagCollection tags;
		try
		{
			BXFilter filter = 
				(e.Keys == null)
				? new BXFilter(ItemFilter.CurrentFilter, BXContentTag.Fields) 
				: new BXFilter(new BXFilterItem(BXContentTag.Fields.Id, BXSqlFilterOperators.Equal, e.Keys["Id"]));
			tags = BXContentTag.GetList(filter, null);
		}
		catch (Exception ex)
		{
			ErrorMessage.AddErrorMessage(ex.Message);
			return;
		}

		foreach(BXContentTag tag in tags)
			try
			{
				tag.Delete();
				e.DeletedCount++;
			}
			catch (Exception ex)
			{
				ErrorMessage.AddErrorMessage(ex.Message);
			}
	}
}

