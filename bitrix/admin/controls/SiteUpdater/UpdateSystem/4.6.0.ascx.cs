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

public partial class bitrix_admin_controls_SiteUpdater_UpdateSystem_4_6_0 : BXControl
{
    private BXSiteUpdater siteUpdater;
	private string locale;

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate("UpdateSystem"))
			BXAuthentication.AuthenticationRequired();

		Server.ScriptTimeout = 10000;
		ScriptManager.GetCurrent(Page).AsyncPostBackTimeout = 10000;

		//this.LinkToStylesheetCollectionRender.Add(VirtualPathUtility.ToAbsolute("~/bitrix/themes/.default/UpdateSystem.css"));

		siteUpdater = BXSiteUpdater.GetUpdater();
		locale = Bitrix.Services.BXLoc.CurrentLocale; // save locale, because site updater has no context to store locale
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

	private string GetError(Exception ex)
	{
		if (ex == null)
			return "";

		BXUpdaterException uex = ex as BXUpdaterException;
		string str;
		if (uex != null)
		{
			StringBuilder s = new StringBuilder();
			foreach(BXUpdaterError error in uex.Errors)
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

		if (String.IsNullOrEmpty(type))
		{
			try
			{
				if (siteUpdater.IsUpdateSystemUpdated())
				{
					MultiView1.ActiveViewIndex = 6;
				}
				else
				{
					bool flag = siteUpdater.CheckForUpdates();

					if (siteUpdater.ServerManifest.UpdateSystemVersion != null)
					{
						MultiView1.ActiveViewIndex = 2;
					}
					else
					{
						if (siteUpdater.ServerManifest.Client.reserved)
						{
							MultiView1.ActiveViewIndex = 8;
						}
						else
						{
							if (siteUpdater.ServerManifest.IsServerLicenseNewer())
							{
								MultiView1.ActiveViewIndex = 7;
							}
							else
							{
								if (flag)
								{
									MultiView1.ActiveViewIndex = 2;
								}
								else
								{
									Dictionary<string, BXUpdaterModule> modulesUpdates = siteUpdater.GetDownloadedModulesUpdates();
									Dictionary<string, BXUpdaterVersion> installedLanguagesUpdates = siteUpdater.GetDownloadedInstalledLanguagesUpdates();
									numberOfModulesUpdates = (modulesUpdates != null ? modulesUpdates.Count : 0);
									numberOfLanguagesUpdates = (installedLanguagesUpdates != null ? installedLanguagesUpdates.Count : 0);

									if (numberOfModulesUpdates > 0 || numberOfLanguagesUpdates > 0)
									{
										if (MultiView1.ActiveViewIndex != 3)
											MultiView1.ActiveViewIndex = 0;
									}
									else
									{
										MultiView1.ActiveViewIndex = 1;
									}
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				type = "error";
				MultiView1.ActiveViewIndex = 5;
				message = GetError(e);
			}
		}
		else if (type.Equals("error", StringComparison.InvariantCultureIgnoreCase))
		{
			MultiView1.ActiveViewIndex = 5;
		}
		else
		{
			MultiView1.ActiveViewIndex = 4;
		}

		Dictionary<string, BXUpdaterVersion> nonInstalledLanguagesUpdates;

		switch (MultiView1.ActiveViewIndex)
		{
			case 0:
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
				break;

			case 1:
				nonInstalledLanguagesUpdates = siteUpdater.GetDownloadedNonInstalledLanguagesUpdates();
				if (nonInstalledLanguagesUpdates != null && nonInstalledLanguagesUpdates.Count > 0)
					lbOptionalNoUpdates.Text = String.Format(GetMessage("FormatAvailableLanguageFiles"), nonInstalledLanguagesUpdates.Count, UpdateWord(nonInstalledLanguagesUpdates.Count));
				else
					lbOptionalNoUpdates.Text = GetMessage("NoAvailableUpdates");
				break;

			case 2:
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
				break;

			case 3:
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
				break;

			case 4:
				lbInstalledUpdates.Text = message;
				break;

			case 5:
				lbUpdateError.Text = message;
                if (message.Contains("EMPTY_LICENSE_KEY") || message.Contains("LICENSE_NOT_FOUND"))
                {
                    lbUpdateError.Text += "<br/><br/>";
                    lbUpdateError.Text += GetMessageRaw("FormatHowYouCanGetKeyCommentFull");
                }
				break;

			case 6:
				break;

			case 7:
				iframeLicenseAgreement.Attributes["src"] = siteUpdater.ServerManifest.NewLicensePath;
				hfAgreeLicenceVersion.Value = siteUpdater.ServerManifest.NewLicense.ToString();
				break;

			case 8:
				if (Page.IsPostBack)
				{
					rfvActUserLastName.Enabled = cbActGenerateUser.Checked;
					rfvActUserLogin.Enabled = cbActGenerateUser.Checked;
					rfvActUserName.Enabled = cbActGenerateUser.Checked;
					rfvActUserPassword.Enabled = cbActGenerateUser.Checked;
					rfvActUserPasswordConf.Enabled = cbActGenerateUser.Checked;
					cvActUserPasswordConf.Enabled = cbActGenerateUser.Checked;
				}
				cbActGenerateUser.Attributes["onclick"] = "ActivateEnableDisableUser(this)";
				break;

			default:
				break;
		}
	}

	private void InitHints()
	{
		lbLastCheckDate.Text = HttpUtility.HtmlEncode(siteUpdater.Config.LastUpdateCheck.ToString());
		lbLastUpdateDate.Text = HttpUtility.HtmlEncode(siteUpdater.Config.LastUpdateInstall.ToString());
		if (siteUpdater.ServerManifest != null && siteUpdater.ServerManifest.Client != null)
		{
			lbClientName.Text = HttpUtility.HtmlEncode(siteUpdater.ServerManifest.Client.name);
			lbEditionName.Text = HttpUtility.HtmlEncode(siteUpdater.ServerManifest.Client.license);
			lbSitesCount.Text = HttpUtility.HtmlEncode((siteUpdater.ServerManifest.Client.maxSites > 0) ? siteUpdater.ServerManifest.Client.maxSites.ToString() : GetMessage("WithoutLimits"));
			if (siteUpdater.ServerManifest.Client.dateFrom != DateTime.MinValue && siteUpdater.ServerManifest.Client.dateTo != DateTime.MinValue)
				lbUpdatesActivity.Text = HttpUtility.HtmlEncode(String.Format(GetMessage("FormatFromTo"), siteUpdater.ServerManifest.Client.dateFrom, siteUpdater.ServerManifest.Client.dateTo));
			else if (siteUpdater.ServerManifest.Client.dateFrom != DateTime.MinValue && siteUpdater.ServerManifest.Client.dateTo == DateTime.MinValue)
				lbUpdatesActivity.Text = HttpUtility.HtmlEncode(String.Format(GetMessage("FormatFrom"), siteUpdater.ServerManifest.Client.dateFrom));
			else if (siteUpdater.ServerManifest.Client.dateFrom == DateTime.MinValue && siteUpdater.ServerManifest.Client.dateTo != DateTime.MinValue)
				lbUpdatesActivity.Text = HttpUtility.HtmlEncode(String.Format(GetMessage("FormatTo"), siteUpdater.ServerManifest.Client.dateTo));
			else
				lbUpdatesActivity.Text = HttpUtility.HtmlEncode(GetMessage("WithoutLimits"));
			lbUpdatesServer.Text = HttpUtility.HtmlEncode(siteUpdater.ServerManifest.Client.httpHost);
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		Page.Title = GetMessage("UpdateSystem");
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		InitMultiView();

		RegisterPanel.Visible = siteUpdater.IsCanBeRegistered() && !siteUpdater.IsUpdateSystemUpdated() && (siteUpdater.ServerManifest.UpdateSystemVersion == null);

		InitHints();

		if (!Page.ClientScript.IsClientScriptBlockRegistered(this.GetType(), "ViewListBlock"))
		{
			StringBuilder jssb = new StringBuilder();

			jssb.Append("function ViewListSelectAllRows(checkbox)\r\n");
			jssb.Append("{\r\n");
			jssb.Append("	var tbl = checkbox.parentNode.parentNode.parentNode.parentNode;\r\n");
			jssb.Append("	var bChecked = checkbox.checked;\r\n");
			jssb.Append("	var i;\r\n");
			jssb.Append("	var n = tbl.rows.length;\r\n");
			jssb.Append("	for (i = 1; i < n; i++)\r\n");
			jssb.Append("	{\r\n");
			jssb.Append("		var box = tbl.rows[i].cells[0].childNodes[0];\r\n");
			jssb.Append("		if (box && box.tagName && box.tagName.toUpperCase() == 'INPUT' && box.type.toUpperCase() == 'CHECKBOX')\r\n");
			jssb.Append("		{\r\n");
			jssb.Append("			if (box.checked != bChecked && !box.disabled)\r\n");
			jssb.Append("			{\r\n");
			jssb.Append("				arTmp = box.id.split('_');\r\n");
			jssb.Append("				if (arTmp[arTmp.length - 2] == 'cb')\r\n");
			jssb.Append("					__ViewListClick('M', bChecked, arTmp[arTmp.length - 1]);\r\n");
			jssb.Append("				else if (arTmp[arTmp.length - 2] == 'cbl')\r\n");
			jssb.Append("					__ViewListClick('L', bChecked, arTmp[arTmp.length - 1]);\r\n");
			jssb.Append("				box.checked = bChecked;\r\n");
			jssb.Append("			}\r\n");
			jssb.Append("		}\r\n");
			jssb.Append("	}\r\n");
			jssb.AppendFormat("	var installUpdatesSelButton = document.getElementById(\"{0}\");\r\n", ViewListInstallButton.ClientID);
			jssb.Append("	installUpdatesSelButton.disabled = !bChecked;\r\n");
			jssb.Append("}\r\n");

			jssb.Append("function ViewListClick(t, checkbox, module)\r\n");
			jssb.Append("{\r\n");
			jssb.Append("	__ViewListClick(t, checkbox.checked, module);\r\n");
			jssb.Append("	var tbl = checkbox.parentNode.parentNode.parentNode.parentNode;\r\n");
			jssb.Append("	var bChecked = false;\r\n");
			jssb.Append("	var i;\r\n");
			jssb.Append("	var n = tbl.rows.length;\r\n");
			jssb.Append("	for (i = 1; i < n; i++)\r\n");
			jssb.Append("	{\r\n");
			jssb.Append("		var box = tbl.rows[i].cells[0].childNodes[0];\r\n");
			jssb.Append("		if (box && box.tagName && box.tagName.toUpperCase() == 'INPUT' && box.type.toUpperCase() == 'CHECKBOX')\r\n");
			jssb.Append("		{\r\n");
			jssb.Append("			if (box.checked && !box.disabled)\r\n");
			jssb.Append("			{\r\n");
			jssb.Append("				bChecked = true;\r\n");
			jssb.Append("				break;\r\n");
			jssb.Append("			}\r\n");
			jssb.Append("		}\r\n");
			jssb.Append("	}\r\n");
			jssb.AppendFormat("	var installUpdatesSelButton = document.getElementById(\"{0}\");\r\n", ViewListInstallButton.ClientID);
			jssb.Append("	installUpdatesSelButton.disabled = !bChecked;\r\n");
			jssb.AppendFormat("	document.{0}.{1}.checked = bChecked;\r\n", Page.Form.ClientID, ViewListAllCheckbox.ClientID);
			jssb.Append("}\r\n");

			jssb.Append("function __ViewListClick(t, flag, module)\r\n");
			jssb.Append("{\r\n");
			jssb.Append("	var hf;\r\n");
			jssb.Append("	if (t == 'M')\r\n");
			jssb.AppendFormat("		hf = document.{0}.{1};\r\n", Page.Form.ClientID, hfViewListModules.ClientID);
			jssb.Append("	else if (t == 'L')\r\n");
			jssb.AppendFormat("		hf = document.{0}.{1};\r\n", Page.Form.ClientID, hfViewListLangs.ClientID);
			jssb.Append("	var str = hf.value;\r\n");
			jssb.Append("	var result = '';\r\n");
			jssb.Append("	if (str.length > 0)\r\n");
			jssb.Append("	{\r\n");
			jssb.Append("		var arStr = str.split(',');\r\n");
			jssb.Append("		for (var i = 0; i < arStr.length; i++)\r\n");
			jssb.Append("		{\r\n");
			jssb.Append("			if (arStr[i].length > 0 && arStr[i] != module)\r\n");
			jssb.Append("			{\r\n");
			jssb.Append("				if (result.length > 0)\r\n");
			jssb.Append("					result += ',';\r\n");
			jssb.Append("				result += arStr[i];\r\n");
			jssb.Append("			}\r\n");
			jssb.Append("		}\r\n");
			jssb.Append("	}\r\n");
			jssb.Append("	if (flag)\r\n");
			jssb.Append("	{\r\n");
			jssb.Append("		if (result.length > 0)\r\n");
			jssb.Append("			result += ',';\r\n");
			jssb.Append("		result += module;\r\n");
			jssb.Append("	}\r\n");
			jssb.Append("	hf.value = result;\r\n");
			jssb.Append("}\r\n");

			jssb.Append("function ActivateEnableDisableUser(checkbox)\r\n");
			jssb.Append("{\r\n");
			jssb.AppendFormat("	document.getElementById(\"{0}\").disabled = !checkbox.checked;\r\n", tbActUserName.ClientID);
			jssb.AppendFormat("	document.getElementById(\"{0}\").disabled = !checkbox.checked;\r\n", tbActUserLastName.ClientID);
			jssb.AppendFormat("	document.getElementById(\"{0}\").disabled = !checkbox.checked;\r\n", tbActUserLogin.ClientID);
			jssb.AppendFormat("	document.getElementById(\"{0}\").disabled = !checkbox.checked;\r\n", tbActUserPassword.ClientID);
			jssb.AppendFormat("	document.getElementById(\"{0}\").disabled = !checkbox.checked;\r\n", tbActUserPasswordConf.ClientID);
			jssb.AppendFormat("	ValidatorEnable(document.all(\"{0}\"), checkbox.checked);\r\n", rfvActUserName.ClientID);
			jssb.AppendFormat("	ValidatorEnable(document.all(\"{0}\"), checkbox.checked);\r\n", rfvActUserLastName.ClientID);
			jssb.AppendFormat("	ValidatorEnable(document.all(\"{0}\"), checkbox.checked);\r\n", rfvActUserLogin.ClientID);
			jssb.AppendFormat("	ValidatorEnable(document.all(\"{0}\"), checkbox.checked);\r\n", rfvActUserPassword.ClientID);
			jssb.AppendFormat("	ValidatorEnable(document.all(\"{0}\"), checkbox.checked);\r\n", rfvActUserPasswordConf.ClientID);
			jssb.AppendFormat("	ValidatorEnable(document.all(\"{0}\"), checkbox.checked);\r\n", cvActUserPasswordConf.ClientID);
			jssb.Append("}\r\n");

			jssb.Append("var arModuleUpdatesDescr = {};");

			jssb.Append("function ShowDescription(module)\r\n");
			jssb.Append("{\r\n");
			jssb.Append("	if (document.getElementById('updates_float_div'))\r\n");
			jssb.Append("		CloseDescription();\r\n");

			////dwefewdfewfvew
			//jssb.Append("	alert(module);\r\n");
			//jssb.Append("	alert(arModuleUpdatesDescr);\r\n");
			//jssb.Append("	alert(arModuleUpdatesDescr[module]);\r\n");
			//jssb.Append("	for(key in arModuleUpdatesDescr)\r\n");
			//jssb.Append("	{\r\n");
			//jssb.Append("		alert(key + '=' + arModuleUpdatesDescr[key]);\r\n");
			//jssb.Append("	}\r\n");

			jssb.Append("	var div = document.body.appendChild(document.createElement('DIV'));\r\n");
			jssb.Append("	div.id = 'updates_float_div';\r\n");
			jssb.Append("	div.className = 'settings-float-form';\r\n");
			jssb.Append("	div.style.position = 'absolute';\r\n");
			jssb.Append("	div.innerHTML = arModuleUpdatesDescr[module];\r\n");
			jssb.Append("	var left = parseInt(document.body.scrollLeft + document.body.clientWidth/2 - div.offsetWidth/2);\r\n");
			jssb.Append("	var top = parseInt(document.body.scrollTop + document.body.clientHeight/2 - div.offsetHeight/2);\r\n");
			jssb.Append("	jsFloatDiv.Show(div, left, top);\r\n");

			//jssb.Append("	alert(1);\r\n");

			jssb.Append("	jsUtils.addEvent(document, 'keypress', DescriptionOnKeyPress);\r\n");
			jssb.Append("}\r\n");

			jssb.Append("function DescriptionOnKeyPress(e)\r\n");
			jssb.Append("{\r\n");
			jssb.Append("	if (!e)\r\n");
			jssb.Append("		e = window.event;\r\n");
			jssb.Append("	if (!e)\r\n");
			jssb.Append("		return;\r\n");
			jssb.Append("	if (e.keyCode == 27)\r\n");
			jssb.Append("		CloseDescription();\r\n");
			jssb.Append("}\r\n");

			jssb.Append("function CloseDescription()\r\n");
			jssb.Append("{\r\n");
			jssb.Append("	jsUtils.removeEvent(document, 'keypress', DescriptionOnKeyPress);\r\n");
			jssb.Append("	var div = document.getElementById('updates_float_div');\r\n");
			jssb.Append("	jsFloatDiv.Close(div);\r\n");
			jssb.Append("	div.parentNode.removeChild(div);\r\n");
			jssb.Append("}\r\n");

			ScriptManager.RegisterClientScriptBlock(Page, this.GetType(), "ViewListBlock", jssb.ToString(), true);
		}

		StringBuilder jssb1 = new StringBuilder();
		StringBuilder sbDescr = new StringBuilder();
		Dictionary<string, BXUpdaterModule> modulesUpdates = siteUpdater.GetDownloadedModulesUpdates();
		foreach (KeyValuePair<string, BXUpdaterModule> kvp in modulesUpdates)
		{
			StringBuilder sbDescr1 = new StringBuilder();

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
				List<KeyValuePair<BXUpdaterVersion, BXUpdaterModule.BXVersion>> versions = new List<KeyValuePair<BXUpdaterVersion,BXUpdaterModule.BXVersion>>(kvp.Value.versions);
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

			string sbDescrString = Regex.Replace(sbDescr1.ToString(), @"\r?\n", "<br>").Replace("\"", "&quot;");
			if (sbDescr.Length > 0)
				sbDescr.Append(",\r\n");
			sbDescr.Append("\"");
			sbDescr.Append(kvp.Key);
			sbDescr.Append("\" : \"");
			sbDescr.Append(JSEncode(sbDescrString));
			sbDescr.Append("\"");
		}
		jssb1.Append("arModuleUpdatesDescr = {");
		jssb1.Append(sbDescr.ToString());
		jssb1.Append("};\r\n\r\n");

		ScriptManager.RegisterClientScriptBlock(Page, this.GetType(), "ViewListBlock1", jssb1.ToString(), true);
	}

	protected void Page_PreRender(object sender, EventArgs e)
	{
		CouponPanel.Visible = 
			siteUpdater.ServerManifest != null
			&& siteUpdater.ServerManifest.Client != null 
			&& siteUpdater.ServerManifest.Client.encodeType == BXUpdaterSiteEncodeType.Source
			&& !siteUpdater.ServerManifest.Client.reserved;						
	}

	private void OnDownloadComplete(object sender, BXDownloadCompleteEventArgs e)
	{
		if (e.UpdateSucceeded)
			InitMultiView();
		else
			InitMultiView("error", String.Format("{0}<br/>{1}", e.ErrorMessage, GetError(e.FailureException)));
	}

	protected void btnDownload_Click(object sender, EventArgs e)
	{
		if (MultiView1.ActiveViewIndex == 2)
		{
			siteUpdater.OnDownloadComplete += new BXSiteUpdater.DownloadCompleteEventHandler(this.OnDownloadComplete);
			siteUpdater.DownloadUpdate();
		}
	}

	protected void btnInstallUpdates_Click(object sender, EventArgs e)
	{
		if (MultiView1.ActiveViewIndex == 0)
		{
			siteUpdater.OnUpdateSiteComplete += new BXSiteUpdater.UpdateSiteCompleteEventHandler(this.OnUpdateSiteComplete);
			siteUpdater.UpdateSite();
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
			InitMultiView("error", String.Format("{0}<br/>{1}", e.ErrorMessage, GetError(e.FailureException)));
		}
	}

	private void OnUpdateUpdateComplete(object sender, BXUpdateUpdateCompleteEventArgs e)
	{
		if (e.UpdateSucceeded)
			Response.Redirect("UpdateSystem.aspx");
		else
			InitMultiView("error", String.Format("{0}<br/>{1}", e.ErrorMessage, GetError(e.FailureException)));
	}

	protected void btnUpdateUpdate_Click(object sender, EventArgs e)
	{
		if (MultiView1.ActiveViewIndex == 6)
		{
			siteUpdater.OnUpdateUpdateComplete += new BXSiteUpdater.UpdateUpdateCompleteEventHandler(this.OnUpdateUpdateComplete);
			siteUpdater.UpdateUpdateSystem();
		}
	}

	private void OnRegisterComplete(object sender, BXRegisterCompleteEventArgs e)
	{
		if (e.UpdateSucceeded)
		{
			RegisterPanel.Visible = false;
			Page.Response.Redirect("UpdateSystem.aspx", true);
		}
		else
		{
			lbRegisterError.Visible = true;
			lbRegisterError.Text = String.Format("{0}<br/>{1}", e.ErrorMessage, ((e.FailureException != null) ? e.FailureException.Message : ""));
		}
	}

	protected void btnRegister_Click(object sender, EventArgs e)
	{
		siteUpdater.OnRegisterComplete += new BXSiteUpdater.RegisterCompleteEventHandler(this.OnRegisterComplete);
		siteUpdater.RegisterSystem();
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

		siteUpdater.OnUpdateSiteComplete += new BXSiteUpdater.UpdateSiteCompleteEventHandler(this.OnUpdateSiteComplete);
		siteUpdater.UpdateSite(m, l);
	}

	protected void ViewListCancelButton_Click(object sender, EventArgs e)
	{
		Page.Response.Redirect("UpdateSystem.aspx", true);
	}

	protected void btnViewUpdates_Click(object sender, EventArgs e)
	{
		MultiView1.ActiveViewIndex = 3;
		InitMultiView();
	}

	protected void btnAgreeLicence_Click(object sender, EventArgs e)
	{
		siteUpdater.Config.ApplyLicences.Add(new BXUpdaterVersion(hfAgreeLicenceVersion.Value), DateTime.Now);
		InitMultiView();
	}

	protected void btnActivate_Click(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;
		siteUpdater.ActivateSystem(tbActOwnerName.Text, tbActOwnerPhone.Text, tbActOwnerEMail.Text, tbActSiteUrl.Text,
			tbActContactPerson.Text, tbActContactPhone.Text, tbActContactEMail.Text, tbActContactInfo.Text,
			cbActGenerateUser.Checked, tbActUserName.Text, tbActUserLastName.Text, tbActUserLogin.Text, tbActUserPassword.Text);
		InitMultiView();
	}

	protected void btnCoupon_Click(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;
		try
		{
			siteUpdater.ActivateCoupon(CouponKey.Text);
			CouponSuccess.Visible = true;
		}
		catch(Exception ex)
		{
			CouponError.Visible = true;
			CouponError.Text = "<br/><br/>" + Encode(ex.Message);
		}
	}
}
