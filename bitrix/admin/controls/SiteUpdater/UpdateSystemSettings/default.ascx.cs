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

public partial class bitrix_admin_controls_UpdateSystemSettings_default : BXControl
{
	private BXSiteUpdater siteUpdater;

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!((BXAdminPage)Page).BXUser.IsCanOperate("UpdateSystem"))
			BXAuthentication.AuthenticationRequired();

		siteUpdater = BXSiteUpdater.GetUpdater();
	}

	private void LoadData()
	{
		tbUpdateUrl.Text = siteUpdater.Config.UpdateUrl;
		tbKey.Text = siteUpdater.Config.Key;
		cbStableVersionsOnly.Checked = siteUpdater.Config.StableVersionsOnly;
		ddlLanguage.SelectedIndex = (("ru".Equals(siteUpdater.Config.Language, StringComparison.InvariantCultureIgnoreCase)) ? 1 : 0);
		tbConnectionString.Text = siteUpdater.Config.ConnectionString;
		tbDatabaseOwner.Text = siteUpdater.Config.DatabaseOwner;
		cbAutoStart.Checked = siteUpdater.Config.AutoStart;
		tbInitialPollInterval.Text = siteUpdater.Config.InitialPollInterval.ToString();
		tbPollInterval.Text = siteUpdater.Config.PollInterval.ToString();
		cbUseProxy.Checked = siteUpdater.Config.UseProxy;
		tbProxyAddress.Text = siteUpdater.Config.ProxyAddress;
		tbProxyUsername.Text = siteUpdater.Config.ProxyUsername;
		tbNotificationEmail.Text = siteUpdater.Config.NotificationEmail;
		cbSafeUpdating.Checked = siteUpdater.Config.SafeUpdating;
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
				bool refresh = (siteUpdater.Config.Key != tbKey.Text);

				siteUpdater.Config.UpdateUrl = tbUpdateUrl.Text;
				siteUpdater.Config.Key = tbKey.Text;
				siteUpdater.Config.StableVersionsOnly = cbStableVersionsOnly.Checked;
				siteUpdater.Config.Language = ((ddlLanguage.SelectedIndex == 1) ? "ru" : "en");
				siteUpdater.Config.ConnectionString = tbConnectionString.Text;
				siteUpdater.Config.DatabaseOwner = tbDatabaseOwner.Text;
				siteUpdater.Config.AutoStart = cbAutoStart.Checked;
				siteUpdater.Config.NotificationEmail = tbNotificationEmail.Text;
				siteUpdater.Config.SafeUpdating = cbSafeUpdating.Checked;

				int initialPollInterval;
				int.TryParse(tbInitialPollInterval.Text, out initialPollInterval);
				siteUpdater.Config.InitialPollInterval = initialPollInterval;

				int pollInterval;
				int.TryParse(tbPollInterval.Text, out pollInterval);
				siteUpdater.Config.PollInterval = pollInterval;

				siteUpdater.Config.UseProxy = cbUseProxy.Checked;
				siteUpdater.Config.ProxyAddress = tbProxyAddress.Text;
				siteUpdater.Config.ProxyUsername = tbProxyUsername.Text;

				if (!String.IsNullOrEmpty(tbProxyPassword.Text) && tbProxyPassword.Text.Equals(tbProxyPasswordConfirm.Text, StringComparison.InvariantCulture))
					siteUpdater.Config.ProxyPassword = tbProxyPassword.Text;

				siteUpdater.Config.Update();				

				siteUpdater.Config.ReportToServer();


				if (RequestActivation.Checked)
				{
					try
					{
						siteUpdater.GetLicenseData();
					}
					catch
					{
					}
				}
				if (refresh)
					Bitrix.Activation.LicenseManager.Refresh();
						

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
