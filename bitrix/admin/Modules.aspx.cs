using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.Modules;
using Bitrix.Main;
using System.Collections.Specialized;
using System.Text;
using Bitrix.UI;

using Bitrix;
using Bitrix.Services;
using Bitrix.Security;

public partial class bitrix_admin_Modules : Bitrix.UI.BXAdminPage
{
	bool canManageSettings;
	
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();
		canManageSettings = BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		Title = GetMessage("TableTitleModules");
		MasterTitle = GetMessage("TableTitleModules");
		List<ModuleDescription> modules = new List<ModuleDescription>();

		foreach (BXModuleConfig module in BXModuleManager.AllModules)
		{
			ModuleDescription desc = new ModuleDescription();
			desc.Id = module.ModuleId;
			desc.IsValid = Bitrix.Activation.LicenseManager.IsViolating(desc.Id);
			desc.IsInstalled = module.Installed;
			desc.NameHtml = Encode(BXLoc.GetModuleMessage(module.ModuleId, "Module.Name", true) ?? string.Format(GetMessageRaw("Module"), module.ModuleId));
			if (desc.IsValid && desc.IsInstalled)
				desc.NameHtml = string.Concat(@"<span style=""color:red"">", desc.NameHtml, @"</span>");
			desc.Description = BXLoc.GetModuleMessage(module.ModuleId, "Module.Description", true) ?? "---";
			desc.Version = module.Version;
			if (desc.IsInstalled)
				desc.Description = BXModuleManager.GetModule(module.ModuleId).Description;
			
			modules.Add(desc);
		}

		modules.Sort(delegate(ModuleDescription a, ModuleDescription b)
		{
			int aRank = string.Equals(a.Id, "main", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
			int bRank = string.Equals(b.Id, "main", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
			int result = aRank.CompareTo(bRank);
			if (result == 0)
				result = string.Compare(a.Id, b.Id, true);
			return result;
		});

		GridView.DataSource = modules;
		GridView.DataBind();
	}

	//NESTED TYPES
	public class ModuleDescription
	{
		public string Id { get; set; }
		public string NameHtml { get; set; }
		public string Description { get; set; }
		public string Version { get; set; }
		public bool IsInstalled { get; set; }	
		public bool IsValid { get; set; }
	}
	protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		BXGridViewRow row = (BXGridViewRow)e.Row;
		ModuleDescription desc = (ModuleDescription)row.DataItem;
		if (row.RowType != DataControlRowType.DataRow)
			return;

		List<string> allowed = new List<string>();

		if (string.Equals(desc.Id, "main", StringComparison.OrdinalIgnoreCase) || !canManageSettings)
			allowed.Add("nop");
		else if (desc.IsInstalled)
			allowed.Add("uninstall");
		else
			allowed.Add("install");

		row.AllowedCommandsList = allowed.ToArray();
	}
	protected void GridView_PopupMenuClick(object sender, BXPopupMenuClickEventArgs e)
	{
		if (!canManageSettings)
			return;

		string moduleId = (string)GridView.DataKeys[e.EventRowIndex].Value;

		if (e.CommandName.ToLower() == "install")
			Response.Redirect("~/bitrix/admin/modulesinstall.aspx?action=install&module=" + moduleId + "&" + BXCsrfToken.BuildQueryStringPair());
		else
			Response.Redirect("~/bitrix/admin/modulesinstall.aspx?action=uninstall&module=" + moduleId + "&" + BXCsrfToken.BuildQueryStringPair());
	}
}
