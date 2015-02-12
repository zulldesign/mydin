<%@ Page Language="C#" %>
<%@ Reference VirtualPath="controls/Main/AdminWizardHost.ascx" %>
<%@ Register Src="controls/Main/AdminWizard.ascx" TagName="Wizard" TagPrefix="bx" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<bx:Wizard ID="Wizard" runat="server" WizardPath="~/bitrix/modules/Main/solution_wizard" OnFinish="Installer_Finish" OnInitState="Installer_InitState" />
<script runat="server">
	protected override void OnPreInit(EventArgs e)
	{
		base.OnPreInit(e);
		if (!Bitrix.Security.BXPrincipal.Current.IsCanOperate(Bitrix.Security.BXRoleOperation.Operations.ProductSettingsManage)
			|| !Bitrix.Security.BXPrincipal.Current.IsCanOperate(Bitrix.Security.BXRoleOperation.Operations.SystemMaintenance))
		{
			Bitrix.Security.BXAuthentication.AuthenticationRequired();
			return;
		}
			
		Wizard.Locale = Bitrix.Services.BXLoc.CurrentLocale;
		Wizard.Url = Request.Url.PathAndQuery;
	}
	
	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		
		Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
		Response.Cache.SetValidUntilExpires(false); 
		Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
		Response.Cache.SetCacheability(HttpCacheability.NoCache);
		Response.Cache.SetNoStore();
	}
	
	void Installer_InitState(object sender, Bitrix.UI.AdminWizardHostInitStateEventArgs e)
	{
 		string siteId = Request.QueryString["site"];
		if (!string.IsNullOrEmpty(siteId))
		{
			e.State["Installer.SiteId"] = siteId;
			if (Request.QueryString["act"] == "setup")
				e.State["Installer.Solution"] = Bitrix.Configuration.BXOptionManager.GetOptionString("main", "InstalledSolution", null, siteId);
				
		}
		e.State["Installer.UserId"] = Bitrix.Security.BXIdentity.Current.Id;
	}
	
	void Installer_Finish(object sender, Bitrix.UI.AdminWizardHostFinishEventArgs e)
	{
		string returnUrl = e.State.GetString(BXConfigurationUtility.Constants.BackUrl);
		
		if(string.IsNullOrEmpty(returnUrl))
 			returnUrl = Request.QueryString[BXConfigurationUtility.Constants.BackUrl];
		
		if (string.IsNullOrEmpty(returnUrl))
		{
			string siteId = e.State.GetString("Installer.SiteId");
			Bitrix.BXSite site;
			if (!string.IsNullOrEmpty(siteId) && (site = Bitrix.BXSite.GetById(siteId)) != null)
				returnUrl = site.GetAbsoluteUrl(site.UrlVirtualPath, null);
		}
		
		if (string.IsNullOrEmpty(returnUrl))
			returnUrl = VirtualPathUtility.ToAbsolute("~/bitrix/admin/");
		
		e.CancelDefaultFinish = true;
		Response.Redirect(returnUrl);
	}
</script>