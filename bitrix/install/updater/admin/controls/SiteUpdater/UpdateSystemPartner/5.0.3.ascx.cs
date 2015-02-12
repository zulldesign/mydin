using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix.Security;
using Bitrix.UI;
using SiteUpdater;
using System.Text;
using System.Text.RegularExpressions;
using Bitrix.Services.Js;

public partial class bitrix_admin_controls_SiteUpdater_UpdateSystemPartner_5_0_3 : BXControl
{
	protected BXSiteUpdater siteUpdater;
	private string locale;
	protected string displayMessage;

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate("UpdateSystem"))
			BXAuthentication.AuthenticationRequired();

		Server.ScriptTimeout = 10000;
		ScriptManager.GetCurrent(Page).AsyncPostBackTimeout = 10000;

		siteUpdater = BXSiteUpdater.GetPartnerUpdater();
		locale = Bitrix.Services.BXLoc.CurrentLocale; // save locale, because site updater has no context to store locale
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		Page.Title = GetMessage("UpdateSystem");
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		InitMultiView();
	}

	private string UpdateWord(int n)
	{
		string result = GetMessage("Updates_1");
		if ((n == 1) || (n >= 21) && (n % 10 == 1))
			result = GetMessage("Update_2");
		else if ((n >= 2) && (n <= 4) || (n >= 22) && (n % 10 >= 2) && (n % 10 <= 4))
			result = GetMessage("Updates_3");
		return result;
	}

	private string GetErrorHtml(Exception ex)
	{
		if (ex == null)
			return "";

		BXUpdaterException uex = ex as BXUpdaterException;
		string str;
		if (uex != null)
		{
			StringBuilder s = new StringBuilder();
			foreach (BXUpdaterError error in uex.Errors)
			{
				if (s.Length != 0)
					s.Append("<br/>");
				s.AppendFormat(Bitrix.Services.BXLoc.GetMessage(locale, AppRelativeVirtualPath, "UpdateError." + error.Type), error.Parameters);
			}
			str = s.ToString();
		}
		else
			str = ex.Message;

		return str + ((ex.InnerException != null) ? ("<br/><br/>" + ex.InnerException.Message) : "");
	}

	private void InitMultiView()
	{
		this.InitMultiView(null, null);
	}

	private void InitMultiView(string type, string message)
	{
		int numberOfModulesUpdates = 0;
		int numberOfLanguagesUpdates = 0;

		SelectView(ref type, ref message, ref numberOfModulesUpdates, ref numberOfLanguagesUpdates);

		Dictionary<string, BXUpdaterVersion> nonInstalledLanguagesUpdates;

		var currentView = MultiView1.Views[MultiView1.ActiveViewIndex];
		if (currentView == ViewMain)
		{

			StringBuilder sb = new StringBuilder();
			if (numberOfModulesUpdates > 0)
				sb.AppendFormat(GetMessage("FormatModules"), numberOfModulesUpdates, UpdateWord(numberOfModulesUpdates));
			if (numberOfLanguagesUpdates > 0)
			{
				if (sb.Length > 0)
					sb.Append(", ");
				sb.AppendFormat(GetMessage("FormatLanguageFiles"), numberOfLanguagesUpdates, UpdateWord(numberOfLanguagesUpdates));
			}
			if (sb.Length <= 0)
				sb.Append(GetMessage("None"));
			lbImportantUpdates.Text = sb.ToString();

			nonInstalledLanguagesUpdates = siteUpdater.GetDownloadedNonInstalledLanguagesUpdates();
			if (nonInstalledLanguagesUpdates != null && nonInstalledLanguagesUpdates.Count > 0)
				lbOptionalUpdates.Text = String.Format(GetMessage("FormatAlsoAvailableLanguageFiles"), nonInstalledLanguagesUpdates.Count, UpdateWord(nonInstalledLanguagesUpdates.Count));
			else
				lbOptionalUpdates.Text = "";
		}
		else if (currentView == ViewNoUpdates)
		{
			nonInstalledLanguagesUpdates = siteUpdater.GetDownloadedNonInstalledLanguagesUpdates();
			if (nonInstalledLanguagesUpdates != null && nonInstalledLanguagesUpdates.Count > 0)
				lbOptionalNoUpdates.Text = String.Format(GetMessage("FormatAvailableLanguageFiles"), nonInstalledLanguagesUpdates.Count, UpdateWord(nonInstalledLanguagesUpdates.Count));
			else
				lbOptionalNoUpdates.Text = GetMessage("NoAvailableUpdates");
		}
		else if (currentView == ViewDownload)
		{
			numberOfModulesUpdates = (siteUpdater.ServerManifest.Modules != null ? siteUpdater.ServerManifest.Modules.Count : 0);
			numberOfLanguagesUpdates = (siteUpdater.ServerManifest.InstalledLanguages != null ? siteUpdater.ServerManifest.InstalledLanguages.Count : 0);
			bool usUpdated = (siteUpdater.ServerManifest.UpdateSystemVersion != null);

			lbImportantUpdatesDld.Text = "";
			if (numberOfModulesUpdates > 0)
				lbImportantUpdatesDld.Text += String.Format(GetMessage("FormatModules"), numberOfModulesUpdates, UpdateWord(numberOfModulesUpdates));
			if (numberOfLanguagesUpdates > 0)
			{
				if (lbImportantUpdatesDld.Text.Length > 0)
					lbImportantUpdatesDld.Text += ", ";
				lbImportantUpdatesDld.Text += String.Format(GetMessage("FormatLanguageFiles"), numberOfLanguagesUpdates, UpdateWord(numberOfLanguagesUpdates));
			}
			if (usUpdated)
			{
				if (lbImportantUpdatesDld.Text.Length > 0)
					lbImportantUpdatesDld.Text += ", ";
				lbImportantUpdatesDld.Text += GetMessage("UpdateSystemHasBeenUpdated");
			}
			if (lbImportantUpdatesDld.Text.Length <= 0)
				lbImportantUpdatesDld.Text += GetMessage("None");

			int numberOfLanguagesUpdates1 = (siteUpdater.ServerManifest.Languages != null ? siteUpdater.ServerManifest.Languages.Count - siteUpdater.ServerManifest.InstalledLanguages.Count : 0);
			if (numberOfLanguagesUpdates1 > 0)
				lbOtherUpdatesDld.Text = String.Format(GetMessage("FormatAlsoAvailableLanguageFiles"), numberOfLanguagesUpdates1, UpdateWord(numberOfLanguagesUpdates1));
			else
				lbOtherUpdatesDld.Text = "";
		}
		else if (currentView == ViewList)
		{
			List<string> modulesArray = new List<string>();
			List<string> langsArray = new List<string>();
			bool isListPostBack = false;
			if (!String.IsNullOrEmpty(hfViewListModules.Value) || !String.IsNullOrEmpty(hfViewListLangs.Value))
			{
				isListPostBack = true;

				if (!String.IsNullOrEmpty(hfViewListModules.Value))
				{
					string[] ma = hfViewListModules.Value.Split(',');
					foreach (string s in ma)
						modulesArray.Add(s);
				}

				if (!String.IsNullOrEmpty(hfViewListLangs.Value))
				{
					string[] la = hfViewListLangs.Value.Split(',');
					foreach (string s in la)
						langsArray.Add(s);
				}
			}

			if (ViewListTable.Rows.Count > 1)
				for (int i = ViewListTable.Rows.Count - 1; i > 0; i--)
					ViewListTable.Rows.RemoveAt(i);

			Dictionary<string, BXUpdaterModule> modulesUpdates = siteUpdater.GetDownloadedModulesUpdates();
			Dictionary<string, BXUpdaterVersion> modulesInstalled = BXUpdaterModuleManager.GetCurrentVersions();
			foreach (KeyValuePair<string, BXUpdaterModule> kvp in modulesUpdates)
			{
				HtmlTableRow tr = new HtmlTableRow();

				HtmlTableCell td = new HtmlTableCell();
				td.Style.Add("width", "30px");
				td.Style.Add("text-align", "center");
				td.Attributes.Add("class", "ListBody");
				HtmlInputCheckBox cb = new HtmlInputCheckBox();
				cb.ID = String.Format("id_viewtable_cb_{0}", kvp.Key);
				cb.Name = String.Format("viewtable_cb_{0}", kvp.Key);
				if (isListPostBack)
				{
					cb.Checked = modulesArray.Contains(kvp.Key);
				}
				else
				{
					cb.Checked = true;
					if (!String.IsNullOrEmpty(hfViewListModules.Value))
						hfViewListModules.Value += ",";
					hfViewListModules.Value += kvp.Key;
				}
				cb.Value = "Y";
				cb.Attributes.Add("onclick", "ViewListClick('M', this, '" + kvp.Key + "')");
				td.Controls.Add(cb);
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Attributes.Add("class", "ListBody");
				td.InnerHtml = kvp.Key;
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Attributes.Add("class", "ListBody");
				td.InnerHtml = (modulesInstalled.ContainsKey(kvp.Key) ? GetMessage("ModuleUpdate") : GetMessage("NewModule"));
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Attributes.Add("class", "ListBody");
				td.InnerHtml = kvp.Value.versions.Values[kvp.Value.versions.Count - 1].version.ToString();
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Attributes.Add("class", "ListBodyRight");
				td.InnerHtml = string.Format("<a href='javascript:ShowDescription(\"{0}\");'>{1}</a>", kvp.Key, GetMessage("Description"));
				tr.Cells.Add(td);

				tr.Attributes.Add("ondblclick", "ShowDescription('" + kvp.Key + "')");

				ViewListTable.Rows.Add(tr);
			}

			Dictionary<string, BXUpdaterVersion> installedLanguagesUpdates = siteUpdater.GetDownloadedInstalledLanguagesUpdates();
			foreach (KeyValuePair<string, BXUpdaterVersion> kvp in installedLanguagesUpdates)
			{
				HtmlTableRow tr = new HtmlTableRow();

				HtmlTableCell td = new HtmlTableCell();
				td.Style.Add("width", "30px");
				td.Style.Add("text-align", "center");
				td.Attributes.Add("class", "ListBody");
				HtmlInputCheckBox cb = new HtmlInputCheckBox();
				cb.ID = String.Format("id_viewtable_cbl_{0}", kvp.Key);
				cb.Name = String.Format("viewtable_cbl_{0}", kvp.Key);
				if (isListPostBack)
				{
					cb.Checked = langsArray.Contains(kvp.Key);
				}
				else
				{
					cb.Checked = true;
					if (!String.IsNullOrEmpty(hfViewListLangs.Value))
						hfViewListLangs.Value += ",";
					hfViewListLangs.Value += kvp.Key;
				}
				cb.Value = "Y";
				cb.Attributes.Add("onclick", "ViewListClick('L', this, '" + kvp.Key + "')");
				td.Controls.Add(cb);
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Attributes.Add("class", "ListBody");
				td.InnerHtml = kvp.Key;
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Attributes.Add("class", "ListBody");
				td.InnerHtml = GetMessage("LanguageFiles");
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Attributes.Add("class", "ListBody");
				td.InnerHtml = kvp.Value.ToString();
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Attributes.Add("class", "ListBody");
				td.InnerHtml = "&nbsp;";
				tr.Cells.Add(td);

				ViewListTable.Rows.Add(tr);
			}
		}
		else if (currentView == ViewFinish)
		{
			lbInstalledUpdates.Text = message;
		}
		
		displayMessage = message;
	}

	private void SelectView(ref string type, ref string message, ref int numberOfModulesUpdates, ref int numberOfLanguagesUpdates)
	{
		if (!string.IsNullOrEmpty(type))
		{
			MultiView1.ActiveViewIndex =
				type.Equals("error", StringComparison.InvariantCultureIgnoreCase)
				? MultiView1.Views.IndexOf(ViewError)
				: MultiView1.Views.IndexOf(ViewFinish);
			return;
		}

		try
		{
			bool hasUpdates = siteUpdater.CheckForUpdates();

			if (siteUpdater.ServerManifest.UpdateSystemVersion != null)
			{
				MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(ViewDownload);
				return;
			}

			if (hasUpdates)
			{
				MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(ViewDownload);
				return;
			}

			Dictionary<string, BXUpdaterModule> modulesUpdates = siteUpdater.GetDownloadedModulesUpdates();
			Dictionary<string, BXUpdaterVersion> installedLanguagesUpdates = siteUpdater.GetDownloadedInstalledLanguagesUpdates();
			numberOfModulesUpdates = (modulesUpdates != null ? modulesUpdates.Count : 0);
			numberOfLanguagesUpdates = (installedLanguagesUpdates != null ? installedLanguagesUpdates.Count : 0);

			if (numberOfModulesUpdates > 0 || numberOfLanguagesUpdates > 0)
			{
				if (MultiView1.Views[MultiView1.ActiveViewIndex] != ViewList)
					MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(ViewMain);
			}
			else
			{
				MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(ViewNoUpdates);
			}
		}
		catch (Exception e)
		{
			type = "error";
			MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(ViewError);
			message = GetErrorHtml(e);
		}
	}

	protected string BuildUpdatesDescription()
	{
		var descr = new Dictionary<string, string>();
		var sbDescr1 = new StringBuilder();
		foreach (KeyValuePair<string, BXUpdaterModule> kvp in siteUpdater.GetDownloadedModulesUpdates())
		{
			sbDescr1.Append("<div class='title'><table cellspacing='0' width='100%'><tr>");
			sbDescr1.AppendFormat("<td width='100%' class='title-text' onmousedown='jsFloatDiv.StartDrag(arguments[0], document.getElementById(\"updates_float_div\"));'>{0}</td>", GetMessage("UpdateDescription"));
			sbDescr1.AppendFormat("<td width='0%'><a class='close' href='javascript:CloseDescription();' title='{0}'></a></td>", GetMessage("Close"));
			sbDescr1.Append("</tr></table></div>");
			sbDescr1.Append("<div class='content' style='overflow:auto;overflow-y:auto;height:400px;'>");
			sbDescr1.AppendFormat("<h2>{0} ({1})</h2>", kvp.Value.name, kvp.Value.moduleId);
			sbDescr1.AppendFormat("<table cellspacing='0'><tr><td>{0}</td></tr></table><br>", kvp.Value.description);

			if (kvp.Value.versions != null && kvp.Value.versions.Count > 0)
			{
				sbDescr1.Append("<table cellspacing='0'>");
				List<KeyValuePair<BXUpdaterVersion, BXUpdaterModule.BXVersion>> versions = new List<KeyValuePair<BXUpdaterVersion, BXUpdaterModule.BXVersion>>(kvp.Value.versions);
				versions.Reverse();
				foreach (KeyValuePair<BXUpdaterVersion, BXUpdaterModule.BXVersion> p in versions)
				{
					sbDescr1.Append("<tr><td><b>");
					sbDescr1.AppendFormat(GetMessage("FormatVersion"), p.Value.version);
					sbDescr1.Append("</b></td></tr>");
					sbDescr1.Append("<tr><td>");
					sbDescr1.AppendFormat("{0}", p.Value.description);
					sbDescr1.Append("</td></tr>");

					bool required = false;
					if (p.Value.versionControl != null)
					{
						if (!required)
						{
							sbDescr1.Append("<tr><td>");
							sbDescr1.Append(GetMessage("ThisProductVersionIsRequired"));
							required = true;
						}
						foreach (BXUpdaterModule.BXVersionControl vc in p.Value.versionControl)
						{
							sbDescr1.Append("<br/>- ");
							sbDescr1.AppendFormat(GetMessage("FormatModuleWithVersionOrAbove"), vc.moduleId, vc.version);
						}
					}
					if (!string.IsNullOrEmpty(p.Value.frameworkControl))
					{
						if (!required)
						{
							sbDescr1.Append("<tr><td>");
							sbDescr1.Append(GetMessage("ThisProductVersionIsRequired"));
							required = true;
						}
						sbDescr1.Append("<br/>- ");
						sbDescr1.AppendFormat(GetMessage("FormatFrameworkVersionOrAbove"), p.Value.frameworkControl);
					}
					if (required)
						sbDescr1.Append("</td></tr>");
				}
				sbDescr1.Append("</table>");
			}
			sbDescr1.Append("</div>");

			descr.Add(kvp.Key, Regex.Replace(sbDescr1.ToString(), @"\r?\n", "<br>").Replace("\"", "&quot;"));
			sbDescr1.Length = 0;
		}

		return BXJSUtility.BuildJSON(descr);
	}

	private void OnDownloadComplete(object sender, BXDownloadCompleteEventArgs e)
	{
		if (e.UpdateSucceeded)
			InitMultiView();
		else
			InitMultiView("error", String.Format("{0}<br/>{1}", e.ErrorMessage, GetErrorHtml(e.FailureException)));
	}

	protected void btnDownload_Click(object sender, EventArgs e)
	{
		if (MultiView1.ActiveViewIndex == 2)
		{
			try
			{
				siteUpdater.OnDownloadComplete += OnDownloadComplete;
				siteUpdater.DownloadUpdate();
			}
			finally
			{
				siteUpdater.OnDownloadComplete -= OnDownloadComplete;
			}
		}
	}

	protected void btnInstallUpdates_Click(object sender, EventArgs e)
	{
		if (MultiView1.ActiveViewIndex == 0)
		{
			try
			{
				siteUpdater.OnUpdateSiteComplete += OnUpdateSiteComplete;
				siteUpdater.UpdateSite();
			}
			finally
			{
				siteUpdater.OnUpdateSiteComplete -= OnUpdateSiteComplete;
			}
		}
	}

	private void OnUpdateSiteComplete(object sender, BXUpdateSiteCompleteEventArgs e)
	{
		if (e.UpdateSucceeded)
		{
			StringBuilder sb = new StringBuilder();
			if (e.NewModuleVersions != null)
			{
				foreach (KeyValuePair<string, BXUpdaterVersion> kvp in e.NewModuleVersions)
				{
					sb.Append("&nbsp;&nbsp;&nbsp;");
					sb.AppendFormat(GetMessage("FormatModuleFromToVersion"), kvp.Key, kvp.Value);
					sb.Append("<br/>");
				}
			}
			if (e.NewLanguageVersions != null)
			{
				foreach (KeyValuePair<string, BXUpdaterVersion> kvp in e.NewLanguageVersions)
				{
					sb.Append("&nbsp;&nbsp;&nbsp;");
					sb.AppendFormat(GetMessage("FormatLanguageUntilVersion"), kvp.Key, kvp.Value);
					sb.Append("<br/>");
				}
			}
			InitMultiView("success", sb.ToString());
		}
		else
		{
			InitMultiView("error", String.Format("{0}<br/>{1}", e.ErrorMessage, GetErrorHtml(e.FailureException)));
		}
	}

	private void OnUpdateUpdateComplete(object sender, BXUpdateUpdateCompleteEventArgs e)
	{
		if (e.UpdateSucceeded)
			Response.Redirect("UpdateSystemPartner.aspx");
		else
			InitMultiView("error", String.Format("{0}<br/>{1}", e.ErrorMessage, GetErrorHtml(e.FailureException)));
	}

	protected void ViewListInstallButton_Click(object sender, EventArgs e)
	{
		List<string> m = new List<string>();
		List<string> l = new List<string>();

		string modulesString = hfViewListModules.Value;
		string[] modulesArray = modulesString.Split(',');

		Dictionary<string, BXUpdaterModule> modulesUpdates = siteUpdater.GetDownloadedModulesUpdates();
		foreach (string module in modulesArray)
		{
			if (modulesUpdates.ContainsKey(module))
				m.Add(module);
		}

		string langsString = hfViewListLangs.Value;
		string[] langsArray = langsString.Split(',');

		Dictionary<string, BXUpdaterVersion> installedLanguagesUpdates = siteUpdater.GetDownloadedInstalledLanguagesUpdates();
		foreach (string lang in langsArray)
		{
			if (installedLanguagesUpdates.ContainsKey(lang))
				l.Add(lang);
		}

		try
		{
			siteUpdater.OnUpdateSiteComplete += OnUpdateSiteComplete;
			siteUpdater.UpdateSite(m, l);
		}
		finally
		{
			siteUpdater.OnUpdateSiteComplete -= OnUpdateSiteComplete;
		}
	}

	protected void ViewListCancelButton_Click(object sender, EventArgs e)
	{
		Page.Response.Redirect("UpdateSystemPartner.aspx", true);
	}

	protected void btnViewUpdates_Click(object sender, EventArgs e)
	{
		MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(ViewList);
		InitMultiView();
	}
}
