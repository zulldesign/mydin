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
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix;
using System.Text;
using Bitrix.DataLayer;
using Bitrix.Configuration;
using Bitrix.IO;
using System.Collections.Specialized;

public partial class bitrix_kernel_TopPanel : BXControl
{
	internal struct BXTopPanelButton
	{
		public string text;
		public string title;
		public string link;
		public string link_param;
		public string icon;
		public bool separator;
		public bool selected;

		internal BXTopPanelButton(bool pseparator)
		{
			separator = pseparator;
			text = null;
			title = null;
			link = null;
			link_param = null;
			icon = null;
			selected = false;
		}

		internal BXTopPanelButton(string ptext, string ptitle)
		{
			text = ptext;
			title = ptitle;
			link = null;
			link_param = null;
			icon = null;
			separator = false;
			selected = false;
		}

		internal BXTopPanelButton(string ptitle, string plink, string picon)
		{
			title = ptitle;
			link = plink;
			icon = picon;
			text = null;
			link_param = null;
			separator = false;
			selected = false;
		}

		internal BXTopPanelButton(string ptext, string ptitle, string plink, string picon)
		{
			text = ptext;
			title = ptitle;
			link = plink;
			icon = picon;
			link_param = null;
			separator = false;
			selected = false;
		}

		internal BXTopPanelButton(string ptext, string ptitle, string plink, string picon, string plink_param)
		{
			text = ptext;
			title = ptitle;
			link = plink;
			icon = picon;
			link_param = plink_param;
			separator = false;
			selected = false;
		}
	}

	void SetLocMessages()
	{
		//BXLoc.Load("Kernel.TopPanel");
		topPanelLinkPublic.Title = GetMessage("Kernel.TopPanel.PublicAreaTitle");
		//topPanelLinkEdit.Title = GetMessage("Kernel.TopPanel.EditSiteTitle");
		topPanelLinkContent.Title = GetMessage("Kernel.TopPanel.ContentTitle");
		topPanelLinkDesign.Title = GetMessage("Kernel.TopPanel.DesignTitle");

		topPanelLinkAdmin.Title = GetMessage("Kernel.TopPanel.ControlPanelTitle");
	}

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		SetLocMessages();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);


		if (Request.QueryString["logout"] != null && Request.QueryString["logout"] == "Y")
			LogoutClicked();

		string publicUrl = PublicUrl;
		string backUrl = BackUrl;

		//topPanelLinkPublic.HRef = publicUrl + "&amp;isdesignmode=N";// + "&amp;=";
		//zg
		topPanelLinkPublic.HRef = InjectQueryParameters(
			publicUrl, 
			BXConfigurationUtility.Constants.BackUrlAdmin, backUrl,
			BXConfigurationUtility.Constants.ShowModeParamName, BXConfigurationUtility.GetShowModeParamValue(BXShowMode.View)
		);
		topPanelLinkContent.HRef = InjectQueryParameters(
			publicUrl,
			BXConfigurationUtility.Constants.BackUrlAdmin, backUrl,
			BXConfigurationUtility.Constants.ShowModeParamName, BXConfigurationUtility.GetShowModeParamValue(BXShowMode.Editor)
		);
		topPanelLinkDesign.HRef = InjectQueryParameters(
			publicUrl,
			BXConfigurationUtility.Constants.BackUrlAdmin, backUrl,
			BXConfigurationUtility.Constants.ShowModeParamName, BXConfigurationUtility.GetShowModeParamValue(BXShowMode.Configurator)
		);
		
		topPanelLinkAdmin.HRef = ((Request.ApplicationPath.Length > 0 && !Request.ApplicationPath.Equals("/", StringComparison.InvariantCultureIgnoreCase)) ? Request.ApplicationPath : "") + "/bitrix/admin/Default.aspx";
        BXPrincipal user = Context.User as BXPrincipal;

        List<BXTopPanelButton> buttons = new List<BXTopPanelButton>();
		if (user.Identity.IsAuthenticated)
		{
            if(user.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			{
    			string p = VirtualPathUtility.ToAbsolute("~/bitrix/admin/Settings.aspx") + "?" + BXConfigurationUtility.Constants.BackUrl + "=" + HttpUtility.UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery);
				


				// try to get current module by scanning modules dir - TODO: implement a better method
				string module = null;
				string vp = Page.AppRelativeVirtualPath;
				if (vp != null && vp.StartsWith("~/bitrix/admin/", StringComparison.OrdinalIgnoreCase))
				{
					string strip = vp.Substring("~/bitrix/admin/".Length);
					string pp = MapPath("~/bitrix/modules/");
					foreach (Bitrix.Modules.BXModule m in Bitrix.Modules.BXModuleManager.InstalledModules)
					{
						if (System.IO.File.Exists(pp + m.ModuleId + "\\install\\system\\admin\\" + strip))
						{
							module = m.GetType().FullName;
							break;
						}
					}
				}
				if (module != null)
					p = p + "&module_id=" + HttpUtility.UrlEncode(module);
				// ---
				
				buttons.Add(new BXTopPanelButton(GetMessage("Kernel.TopPanel.Settings"), GetMessage("Kernel.TopPanel.SettingsTitle"), p, "top_panel_settings"));
			}

			if (BXConfigurationUtility._SiteUpdateSystemAvailable && user.IsCanOperate(BXRoleOperation.Operations.UpdateSystem))
            {
                if (buttons.Count > 0)
                    buttons.Add(new BXTopPanelButton(true));
                buttons.Add(new BXTopPanelButton(GetMessage("Kernel.TopPanel.UpdatesTitle"), BXPath.ToVirtualAbsolutePath("~/bitrix/admin/UpdateSystem.aspx"), "top_panel_update"));
            }
		}
		BXLanguageCollection locales = BXLanguage.GetList(new BXFilter(new BXFilterItem(BXLanguage.Fields.Active, BXSqlFilterOperators.Equal, true)), null);
		if (locales.Count > 0)
		{
            if (buttons.Count > 0)
			    buttons.Add(new BXTopPanelButton(true));
			foreach (BXLanguage l in locales)
			{
				UriBuilder u = new UriBuilder(Request.Url);
				StringBuilder q = new StringBuilder();
				foreach (string key in Request.QueryString.AllKeys)
				{
					if (string.IsNullOrEmpty(key))
						continue;
					if (key.Equals("lang", StringComparison.InvariantCultureIgnoreCase))
						continue;
					if (q.Length != 0)
						q.Append("&");
					q.Append(key);
					q.Append("=");
					q.Append(HttpUtility.UrlEncode(Request.QueryString[key]));
				}

				if (q.Length != 0)
					q.Append("&");
				q.Append("lang=");
				q.Append(HttpUtility.UrlEncode(l.Id));
				u.Query = q.ToString();

				BXTopPanelButton b = new BXTopPanelButton(l.Id, l.Name);
				b.link = u.ToString();
				b.selected = l.Id.Equals(BXLoc.CurrentLocale, StringComparison.InvariantCultureIgnoreCase);
				buttons.Add(b);
			}
		}

		if (user.Identity.IsAuthenticated)
		{
            if (buttons.Count > 0)
			    buttons.Add(new BXTopPanelButton(true));
			buttons.Add(new BXTopPanelButton(string.Format(
				"[<a href=\"{0}/bitrix/admin/AuthUsersEdit.aspx?id={1}\">{1}</a>] ({2}) {3}",
				((Request.ApplicationPath.Length > 0 && !Request.ApplicationPath.Equals("/", StringComparison.InvariantCultureIgnoreCase)) ? Request.ApplicationPath : ""),
				((BXIdentity)Context.User.Identity).User.UserId,
				Server.HtmlEncode(Context.User.Identity.Name),
				Server.HtmlEncode(string.Join(" ", new string[] { ((BXIdentity)Context.User.Identity).User.FirstName, ((BXIdentity)Context.User.Identity).User.LastName }))
			), GetMessage("Kernel.TopPanel.CurrentUser")));
			buttons.Add(new BXTopPanelButton(true));


			dvStart.Attributes.Add("onclick", "jsStartMenu.ShowStartMenu(this);");
			dvStart.Attributes.Add("onmouseover", "this.className+=' start-over'");
			dvStart.Attributes.Add("onmouseout", @"this.className=this.className.replace(/\s*start-over/i, '')");
            dvStart.Attributes.Add("title", GetMessage("Kernel.Start"));
		}
		else
		{
			UriBuilder ub = new UriBuilder(HttpContext.Current.Request.Url);
			ub.Path = FormsAuthentication.LoginUrl;
			ub.Query = null;

			dvStart.Attributes.Add("onclick", string.Format("window.location.href='{0}'", ub.Uri.ToString()));
			dvStart.Attributes.Add("onmouseover", "this.className+=' start-over'");
			dvStart.Attributes.Add("onmouseout", @"this.className=this.className.replace(/\s*start-over/i, '')");
            dvStart.Attributes.Add("title", GetMessage("Kernel.Start"));
		}

		HtmlTableRow tr = new HtmlTableRow();
		topPanelButtons.Rows.Add(tr);
		HtmlTableCell tdLeftMost = new HtmlTableCell();
		tdLeftMost.Attributes.Add("class", "left");
		tr.Cells.Add(tdLeftMost);

		foreach (BXTopPanelButton button in buttons)
		{
			HtmlTableCell td = new HtmlTableCell();
			if (button.separator)
			{
				td.InnerHtml = "<div class=\"separator\"></div>";
			}
			else
			{
				if (!String.IsNullOrEmpty(button.link))
				{
					td.InnerHtml = string.Format(
						"<a href=\"{0}\" hidefocus=\"true\" title=\"{1}\" {2} class=\"context-button{3}{4}>{5}</a>",
						HttpUtility.HtmlEncode(button.link),
						HttpUtility.HtmlEncode(button.title),
						button.link_param,
						(button.selected ? " pressed" : ""),
						(!String.IsNullOrEmpty(button.icon) ? string.Format(" icon{0}\" id=\"{1}\"", (String.IsNullOrEmpty(button.text) ? " icon-only" : ""), HttpUtility.HtmlEncode(button.icon)) : "\""),
						button.text
					);
				}
				else
				{
					td.InnerHtml = string.Format(
						"<div title=\"{0}\" class=\"context-text{1}>{2}</div>",
						button.title,
						(!String.IsNullOrEmpty(button.icon) ? string.Format(" icon{0}\" id=\"{1}\"", (String.IsNullOrEmpty(button.text) ? " icon-only" : ""), button.icon) : "\""),
						button.text
					);
				}
			}

			tr.Cells.Add(td);
		}

		if (user.Identity.IsAuthenticated)
		{
			LinkButton linkButton = new LinkButton();
			linkButton.Click += new EventHandler(LogoutButton);
			linkButton.Text = GetMessage("Kernel.TopPanel.Logout");
			linkButton.ToolTip = GetMessage("Kernel.TopPanel.LogoutTitle");
			linkButton.Attributes.Add("hidefocus", "true");
			linkButton.Attributes.Add("class", "context-button icon");
			linkButton.Attributes.Add("id", "top_panel_logout");

			HtmlTableCell td1 = new HtmlTableCell();
			td1.Controls.Add(linkButton);
			tr.Cells.Add(td1);
		}


	}

	private string PublicUrl
	{
		get
		{
			string publicUrl = Request.QueryString[BXConfigurationUtility.Constants.BackUrlPub];
			if (!string.IsNullOrEmpty(publicUrl))
				Session[BXConfigurationUtility.Constants.BackUrlPub] = publicUrl;
			if (string.IsNullOrEmpty(publicUrl))
				publicUrl = (Session[BXConfigurationUtility.Constants.BackUrlPub] ?? string.Empty).ToString();
			if (string.IsNullOrEmpty(publicUrl))
				publicUrl = BXPath.TrimEnd(BXPath.ToVirtualAbsolutePath("~")) + BXPath.AltDirectorySeparatorChar;
			return publicUrl;
		}
	}

	private string InjectQueryParameters(string url, params string[] queryParameters)
	{
		StringBuilder s = new StringBuilder(url);
		bool hasQuery = url.IndexOf('?') != -1;
		int cnt = (queryParameters.Length / 2) * 2;
		for (int i = 0; i < cnt; i += 2)
		{
			s.Append(hasQuery ? '&' : '?');
			hasQuery = true;

			s.AppendFormat("{0}={1}", UrlEncode(queryParameters[i]), UrlEncode(queryParameters[i + 1]));
		}
		return s.ToString();
	}

	private string BackUrl
	{
		get
		{
			UriBuilder url = new UriBuilder(new Uri(Request.Url, Request.RawUrl));
			string query = url.Uri.Query;
			if (string.IsNullOrEmpty(query))
				return Request.RawUrl;
			
			
			NameValueCollection queryParts = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
			string[] parts = query.Substring(1).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string part in parts)
			{
				int i = part.IndexOf('=');
				string key = (i != -1) ?  HttpUtility.UrlDecode(part.Remove(i)) : null;
				string value = HttpUtility.UrlDecode(part.Substring(i + 1));
				queryParts.Add(key, value);
			}

			queryParts.Remove(BXConfigurationUtility.Constants.BackUrlAdmin);
			queryParts.Remove(BXConfigurationUtility.Constants.BackUrlPub);
			queryParts.Remove(BXConfigurationUtility.Constants.ShowModeParamName);

			StringBuilder queryString = new StringBuilder();
			foreach (string key in queryParts.Keys)
				foreach(string value in queryParts.GetValues(key))
				{
					if (queryString.Length != 0)
						queryString.Append('&');
					if (key != null)
					{
						queryString.Append(HttpUtility.UrlEncode(key));
						queryString.Append('=');
					}
					queryString.Append(HttpUtility.UrlEncode(value));
				}
			url.Query = queryString.ToString();
			queryString.Length = 0;

			return url.Uri.PathAndQuery;
		}
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		BXPage page = Page as BXPage;
		if (page == null)
			throw new InvalidOperationException("Page must inherit from BXPage");
	
		ClientScriptManager cm = page.ClientScript;
		if (cm == null)
			throw new InvalidOperationException("Could not find client script manager!");
	
		if (!cm.IsClientScriptBlockRegistered(GetType(), "settings"))
			cm.RegisterClientScriptBlock(
				GetType(),
				"settings",
				string.Format(@"var topPanelSettings = {{
                    'starMenuHandlerUrl':'{0}'
                }};",
				VirtualPathUtility.ToAbsolute(VirtualPathUtility.Combine(Bitrix.Configuration.BXConfigurationUtility.Constants.PublicHandlersVirtualPath, "main/startmenuhandler.ashx"))),
				true
				);

		BXPage.RegisterScriptInclude("~/bitrix/js/Main/utils.js");
		BXPage.RegisterScriptInclude("~/bitrix/js/Main/admin_tools.js");
		BXPage.RegisterScriptInclude("~/bitrix/js/Main/popup_menu_net.js");

		if (!cm.IsStartupScriptRegistered(GetType(), "initialize"))
			cm.RegisterStartupScript(
				GetType(),
				"initialize",
                IsFixed ? @"jsPanel.FixOn(); window.topPanelPopup = new PopupMenu('topPanelPopup');" : @"window.topPanelPopup = new PopupMenu('topPanelPopup');",
				true);

		//BXPage.RegisterThemeStyle("start_menu.css");
	}

	private void LogoutButton(object sender, EventArgs args)
	{
		LogoutClicked();
	}

	private void LogoutClicked()
	{
        Bitrix.Security.BXAuthentication.SignOut();
		Response.Clear();
		Response.StatusCode = 200;

		if ((Page.Form == null) || !string.Equals(Page.Form.Method, "get", StringComparison.OrdinalIgnoreCase))
			Response.Redirect(Request.RawUrl, false);
		else
			Response.Redirect(Request.Path, false);
	}

	private bool? mIsFixed = null;
	protected bool IsFixed
	{
		get
		{
			if (!mIsFixed.HasValue)
				mIsFixed = BXAdminPanelHelper.IsFixed;
			return mIsFixed.Value;
		}
	}
}
