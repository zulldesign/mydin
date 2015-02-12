using System;
using System.Linq;
using Bitrix.UI;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Security;
using Bitrix.Configuration;
using Bitrix.Modules;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.IO;
using System.Web;


namespace Bitrix.Main.AdminPages
{
	public partial class SolutionList : BXAdminPage
	{
		bool canModify;
		string wizardUrl;

		public string WizardUrl 
		{ 
			get
			{
				if (wizardUrl == null)
					wizardUrl = new Uri(Request.Url, VirtualPathUtility.ToAbsolute("~/bitrix/admin/InstallSolutionWizard.aspx")).AbsoluteUri;
				return wizardUrl;
			}
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
				BXAuthentication.AuthenticationRequired();

			canModify = BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
			AddButton.Visible = canModify;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Title = GetMessageRaw("PageTitle");
			MasterTitle = GetMessageRaw("PageTitle");
		}

		protected void GridView_Select(object sender, BXSelectEventArgs e)
		{
			e.Data = BXSite.GetList(
				new BXFilter(new BXFilterItem(BXSite.Fields.Active, BXSqlFilterOperators.Equal, true)),
				new BXOrderBy(
					new BXOrderByPair(BXSite.Fields.Sort, BXOrderByDirection.Asc),
					new BXOrderByPair(BXSite.Fields.Name, BXOrderByDirection.Asc)
				),
				null,
				new BXQueryParams(e.PagingOptions),
				BXTextEncoder.EmptyTextEncoder
			)
			.ConvertAll(x => new SiteInfo(x, this));
		}
		protected void GridView_SelectCount(object sender, BXSelectCountEventArgs e)
		{
			e.Count = BXSite.Count(
				new BXFilter(new BXFilterItem(BXSite.Fields.Active, BXSqlFilterOperators.Equal, true))
			);
		}
		protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType != DataControlRowType.DataRow)
				return;

			var row = (BXGridViewRow)e.Row;
			var item = (SiteInfo)row.DataItem;
			row.UserData.Add("id", item.Site.Id);

			var commands = new List<string>(2);
			if (canModify)
			{
				if (item.SolutionInfo == null)
					row.AllowedCommandsList = new[] { "install" };
				else
				{
					if (File.Exists(Path.Combine(item.SolutionInfo.Path, "Options.ascx")))
						row.AllowedCommandsList = new[] { "settings", "system" };				
					else 
						row.AllowedCommandsList = new[] { "settings" };				
				}
			}
			else 
				row.AllowedCommandsList = new[] { "nop" };			
		}

		class SiteInfo
		{
			public BXSite Site { get; private set; }
			public BXSolutionInfo SolutionInfo { get; private set; }
			public string TitleHtml { get; private set; }

			public SiteInfo(BXSite site, BXPage page)
			{
				Site = site;
				var solutionId = BXOptionManager.GetOptionString("main", "InstalledSolution", null, site.Id);
				SolutionInfo = BXSolutionHelper.AvailableSolutions.Where(x => string.Equals(x.Id, solutionId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
				if (SolutionInfo == null)
					TitleHtml = string.Concat(@"<span style=""color:#777"">", page.GetMessageRaw("NotInstalled"), "</span>");
				else
					TitleHtml = SolutionInfo.TitleHtml;
			}
		}
	}
}