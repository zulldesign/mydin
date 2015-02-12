using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Marketplace;
using Bitrix.UI;

public partial class bitrix_admin_Marketplace : Bitrix.UI.BXAdminPage
{
	BXMarketplaceQueryResult query;
	BXMarketplaceModuleDownloader downloader;
	string moduleId;

    protected override void OnInit(EventArgs e)
    {
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.UpdateSystem))
			BXAuthentication.AuthenticationRequired();

        base.OnInit(e);
        MasterTitle = "Marketplace";
		try
		{
			query = new BXMarketplaceQuery
			{
				ModuleId = "-"
			}
			.Execute();
		}
		catch(Exception ex)
		{
			ErrorMessage.AddErrorText(ex.Message);
			query = new BXMarketplaceQueryResult();
		}

		CategoryFilter.Values.Clear();
		CategoryFilter.Values.Add(new ListItem(GetMessageRaw("Kernel.All"), ""));
		CategoryFilter.Values.AddRange(query.Categories.ConvertAll(x => new ListItem(x.Title, x.Id.ToString())).ToArray());

		TypeFilter.Values.Clear();
		TypeFilter.Values.Add(new ListItem(GetMessageRaw("Kernel.All"), ""));
		TypeFilter.Values.AddRange(query.Types.ConvertAll(x => new ListItem(x.Title, x.Id.ToString())).ToArray());

		
    }

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		var f = AdminFilter.CurrentFilter;
	}

	private BXMarketplaceQuery BuildQuery()
	{
		var query = new BXMarketplaceQuery();
		foreach (var f in AdminFilter.CurrentFilter)
		{
			if (f.filterOperator != Bitrix.DataLayer.BXSqlFilterOperators.Equal)
				continue;
			switch (f.filterName)
			{
				case "Title":
					query.Title = ((string)f.filterValue).Trim();
					break;
				case "Category":
					query.Category = (int)f.filterValue;
					break;
				case "Type":
					query.Type = (int)f.filterValue;
					break;
			}
		}

		return query;
	}

	IAsyncResult BeginAsyncOperation(object sender, EventArgs e, AsyncCallback callback, object state)
    {
		downloader = new BXMarketplaceModuleDownloader();
		moduleId = (string)state;
		return downloader.BeginDownloadPackage(moduleId, callback, null);
    }

    void EndAsyncOperation(IAsyncResult result)
    {
		try
		{
			downloader.EndDownloadPackage(result);
		}
		catch(BXMarketplaceModuleDownloaderException ex)
		{
			ErrorMessage.AddErrorMessage(ex.HtmlMessage);
		}
		catch(Exception ex)
		{
			ErrorMessage.AddErrorText(GetMessage("Error.Unknown"));
			BXLogService.LogAll(ex, BXLogMessageType.Error, "MarketplaceDetail.aspx");
		}

		Response.Redirect("ModulesInstall.aspx?action=install&module=" + moduleId + "&" + BXCsrfToken.BuildQueryStringPair());
    }

	protected void GridView_Select(object sender, BXSelectEventArgs e)
	{
		var query = BuildQuery();

		Match m;
		if ((m = Regex.Match(e.SortExpression ?? "", @"^(\w+)(?:\s+(ASC|DESC))?$", RegexOptions.IgnoreCase)).Success)
		{
			try
			{
				query.OrderBy = (BXMarketplaceQueryOrderBy)Enum.Parse(typeof(BXMarketplaceQueryOrderBy), m.Groups[1].Value, true);
				query.OrderByAsc = !string.Equals(m.Groups[2].Value, "DESC", StringComparison.OrdinalIgnoreCase);
			}
			catch
			{

			}
		}
		
		BXMarketplaceQueryResult r;
		try
		{
			r = query.Execute();
		}
		catch(Exception ex)
		{
			ErrorMessage.AddErrorText(ex.Message);
			r = new BXMarketplaceQueryResult();
		}

		var q = r.Modules.Skip(e.PagingOptions.startRowIndex);
		if (e.PagingOptions.maximumRows > 0)
			q = q.Take(e.PagingOptions.maximumRows);
		e.Data = q.ToList();
	}
	protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;
		var row = (BXGridViewRow)e.Row;
		var module =(BXMarketplaceModule)row.DataItem;
		row.UserData["Id"] = module.Id;

		row.AllowedCommandsList = module.Downloaded ? new[] { "view" } : new[] { "view", "install" };
	}
	protected void GridView_PopupMenuClick(object sender, BXPopupMenuClickEventArgs e)
	{
		if (e.CommandName != "install")
			return;

		AddOnPreRenderCompleteAsync(BeginAsyncOperation, EndAsyncOperation, GridView.DataKeys[e.EventRowIndex].Value);
	}
}
