using System;
using System.Linq;
using Bitrix.UI;
using Bitrix.Security;
using System.Web.UI.WebControls;
using Bitrix.Configuration;
using Bitrix.Modules;
using System.IO;
using Bitrix.IO;
using System.Web.UI;

namespace Bitrix.Main.AdminPages
{
	public partial class SolutionEdit : BXAdminPage
	{
		protected void Page_Init(object sender, EventArgs e)
		{
			if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
				BXAuthentication.AuthenticationRequired();
			MasterTitle = GetMessageRaw("PageTitle");
			
			BXSite site;
			var siteId = Request.QueryString["site"];
			if (string.IsNullOrEmpty(siteId) || (site = BXSite.GetById(siteId)) == null || !site.Active)
			{
				Content.Controls.Add(new Literal { Text = GetMessageRaw("SiteNotFound") });
				return;
			}
			
			var solutionId = BXOptionManager.GetOptionString("main", "InstalledSolution", null, site.TextEncoder.Decode(site.Id));
			var solution = BXSolutionHelper.AvailableSolutions.Where(x => string.Equals(x.Id, solutionId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
			if (solution == null)
			{
				Content.Controls.Add(new Literal { Text = GetMessageRaw("SolutionNotFound") });
				return;
			}
						
			Control control;
			try
			{
				var path = Path.Combine(solution.Path, "Options.ascx");
				if (!File.Exists(path))
					throw new Exception();				
				control = LoadControl(BXPath.ToVirtualRelativePath(path));
			}
			catch
			{
				Content.Controls.Add(new Literal { Text = GetMessageRaw("SettingsPageNotFound") });
				return;
			}

			Content.Controls.Add(control);			
		}
	}
}