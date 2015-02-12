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
using SiteUpdater;
using Bitrix.Security;
using Bitrix.Modules.Intercom;

public partial class bitrix_admin_controls_UpdateSystemSettings_7_0_0 : BXControl
{
	private BXSiteUpdaterIntercomProxy siteUpdater;
	private BXUpdaterConfig config;

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!((BXAdminPage)Page).BXUser.IsCanOperate("UpdateSystem"))
			BXAuthentication.AuthenticationRequired();
		
		siteUpdater = BXSiteUpdaterIntercomProxy.ResolveProxy();
		config = siteUpdater.RequestConfigSettings();
	}

	private void LoadData()
	{
		tbUpdateUrl.Text = config.UpdateUrl;
		tbKey.Text = config.Key;
		cbStableVersionsOnly.Checked = config.StableVersionsOnly;
		ddlLanguage.SelectedIndex = (("ru".Equals(config.Language, StringComparison.InvariantCultureIgnoreCase)) ? 1 : 0);
		tbConnectionString.Text = config.ConnectionString;
		tbDatabaseOwner.Text = config.DatabaseOwner;
		cbAutoStart.Checked = config.AutoStart;
		tbInitialPollInterval.Text = config.InitialPollInterval.ToString();
		tbPollInterval.Text = config.PollInterval.ToString();
		cbUseProxy.Checked = config.UseProxy;
		tbProxyAddress.Text = config.ProxyAddress;
		tbProxyUsername.Text = config.ProxyUsername;
		tbNotificationEmail.Text = config.NotificationEmail;
		cbSafeUpdating.Checked = config.SafeUpdating;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		Page.Title = GetMessage("PageTitle.UpdateSystem");
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		if (!Page.IsPostBack)
			LoadData();
	}

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{
		bool noRedirect = false;
		bool successAction = true;
		if (e.CommandName == "save")
		{
			if (!SaveConfig())
			{
				successAction = false;
				noRedirect = true;
			}
		}
		else if (e.CommandName == "apply")
		{
			if (!SaveConfig())
				successAction = false;
			noRedirect = true;
		}

		if (!noRedirect)
		{
		    Page.Response.Redirect(Request.QueryString["back_url"] ?? "UpdateSystem.aspx");
		}
		else
		{
			if (successAction)
			{
				successMessage.Visible = true;
				LoadData();
			}
		}
	}

	private bool SaveConfig()
	{
		if (Page.IsValid)
		{
			try
			{
				bool refresh = (config.Key != tbKey.Text);

				config.UpdateUrl = tbUpdateUrl.Text;
				config.Key = tbKey.Text;
				config.StableVersionsOnly = cbStableVersionsOnly.Checked;
				config.Language = ((ddlLanguage.SelectedIndex == 1) ? "ru" : "en");
				config.ConnectionString = tbConnectionString.Text;
				config.DatabaseOwner = tbDatabaseOwner.Text;
				config.AutoStart = cbAutoStart.Checked;
				config.NotificationEmail = tbNotificationEmail.Text;
				config.SafeUpdating = cbSafeUpdating.Checked;

				int initialPollInterval;
				int.TryParse(tbInitialPollInterval.Text, out initialPollInterval);
				config.InitialPollInterval = initialPollInterval;

				int pollInterval;
				int.TryParse(tbPollInterval.Text, out pollInterval);
				config.PollInterval = pollInterval;

				config.UseProxy = cbUseProxy.Checked;
				config.ProxyAddress = tbProxyAddress.Text;
				config.ProxyUsername = tbProxyUsername.Text;

				if (!String.IsNullOrEmpty(tbProxyPassword.Text) && tbProxyPassword.Text.Equals(tbProxyPasswordConfirm.Text, StringComparison.InvariantCulture))
					config.ProxyPassword = tbProxyPassword.Text;

				siteUpdater.ApplyConfigSettings(config, RequestActivation.Checked);										
				return true;
			}
			catch (Exception e)
			{
				errorMassage.AddErrorMessage(e.Message);
			}
		}
		return false;
	}
}
