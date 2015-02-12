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
using System.Text;
using System.Collections.Generic;
using Bitrix.Modules;
using Bitrix.Configuration;
using Bitrix.IO;
using System.Collections.Specialized;

public partial class bitrix_kernel_PublicPanel : Bitrix.UI.BXControl
{
	string backUrl;


	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		backUrl = BackUrl;
		
		dvStart.Attributes.Add("onclick", "jsStartMenu.ShowStartMenu(this);");
		dvStart.Attributes.Add("onmouseover", "this.className+=' start-over'");
		dvStart.Attributes.Add("onmouseout", @"this.className=this.className.replace(/\s*start-over/i, '')");
		dvStart.Attributes.Add("title", GetMessage("Kernel.Start"));
        EnsureChildControls();
	}

	private string AdminUrl
	{
		get
		{
			string adminUrl = Request.QueryString[BXConfigurationUtility.Constants.BackUrlAdmin];
			if (!string.IsNullOrEmpty(adminUrl))
				Session[BXConfigurationUtility.Constants.BackUrlAdmin] = adminUrl;
			if (string.IsNullOrEmpty(adminUrl))
				adminUrl = (Session[BXConfigurationUtility.Constants.BackUrlAdmin] ?? string.Empty).ToString();
			if (string.IsNullOrEmpty(adminUrl))
				adminUrl = BXPath.TrimEnd(BXPath.ToVirtualAbsolutePath("~/bitrix/admin/")) + BXPath.AltDirectorySeparatorChar;
			return adminUrl;
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
				string key = (i != -1) ? HttpUtility.UrlDecode(part.Remove(i)) : null;
				string value = HttpUtility.UrlDecode(part.Substring(i + 1));
				queryParts.Add(key, value);
			}

			queryParts.Remove(BXConfigurationUtility.Constants.BackUrlAdmin);
			queryParts.Remove(BXConfigurationUtility.Constants.BackUrlPub);
			queryParts.Remove(BXConfigurationUtility.Constants.ShowModeParamName);

			StringBuilder queryString = new StringBuilder();
			foreach (string key in queryParts.Keys)
				foreach (string value in queryParts.GetValues(key))
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

	protected string BuildTabUrl(BXShowMode mode)
	{
		return HttpUtility.HtmlEncode(InjectQueryParameters(
			backUrl, 
			BXConfigurationUtility.Constants.ShowModeParamName, BXConfigurationUtility.GetShowModeParamValue(mode)
		));
	}

	protected string BuildAdminUrl()
	{
		return HttpUtility.HtmlEncode(InjectQueryParameters(
			AdminUrl,
			BXConfigurationUtility.Constants.BackUrlPub, backUrl
		));
	}

	protected override void OnPreRender(EventArgs e)
	{
        if (!Visible)
        {
            base.OnPreRender(e);
            return;
        }

        TemplateRequisite.RequireScriptManager(Page, true);

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
				//ResolveUrl("~/bitrix/tools/startmenuhandler.ashx")),
				VirtualPathUtility.ToAbsolute(VirtualPathUtility.Combine(Bitrix.Configuration.BXConfigurationUtility.Constants.PublicHandlersVirtualPath, "main/startmenuhandler.ashx"))),
				true
				);

		BXPage.Scripts.RequirePublicDialogFramework();
		BXPage.Scripts.RequireAdminTools();
	
		if (!cm.IsStartupScriptRegistered(GetType(), "initialize"))
			cm.RegisterStartupScript(
				GetType(),
				"initialize",
				//TODO: Код не работает в IE
				//IsFixed ?
				//@"jsPanel.FixOn(); window.topPanelPopup = new PopupMenu('topPanelPopup');" : @"jsPanel.FixOn(); jsPanel.FixPanel(); window.topPanelPopup = new PopupMenu('topPanelPopup');",
                IsFixed ? @"jsPanel.FixOn(); window.topPanelPopup = new PopupMenu('topPanelPopup');" : @"window.topPanelPopup = new PopupMenu('topPanelPopup');",
				true);

		BXPage.Styles.RequirePublicStyles();
		//BXPage.RegisterThemeStyle("start_menu.css");
        base.OnPreRender(e);
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


	protected override void CreateChildControls()
	{
		BXCommand cmdCreate = new BXCommand("Bitrix.Modules.BXPublicPanel.CreateMenu");
		BXPublicPanelMenuSectionList sectionList = new BXPublicPanelMenuSectionList();
		cmdCreate.AddCommandResult("SectionList", new BXCommandResult(BXCommandResultType.Ok, sectionList));
		cmdCreate.Send();

		Visible = CalculateVisibility(sectionList);
		if (!Visible)
			return;

		if (BXConfigurationUtility.IsDesignMode)
			Response.Cache.SetCacheability(HttpCacheability.NoCache);

		BXCommand cmdFill = new BXCommand("Bitrix.Modules.BXPublicPanel.PopulateMenu");
		cmdFill.Parameters.Add("ShowMode", Bitrix.Configuration.BXConfigurationUtility.ShowMode);
		cmdFill.AddCommandResult("SectionList", new BXCommandResult(BXCommandResultType.Ok, sectionList));
		cmdFill.Send();

		int count = sectionList.Count;
		if (count == 0)
			return;
		
		sectionList.SortByOrder();
		bool firstSection = true;
		foreach (BXPublicPanelMenuSection section in sectionList)
			if (section.ShouldRender)
			{
				if (!firstSection)
				{
					HtmlGenericControl separator = new HtmlGenericControl("div");
					separator.Attributes.Add("class", "bx-pnseparator");
					panelButtonsPlaceHolder.Controls.Add(separator);
				}
				else
					firstSection = false;
				panelButtonsPlaceHolder.Controls.Add(section);
			}
	}

	private bool CalculateVisibility(BXPublicPanelMenuSectionList sectionList)
	{
		foreach (BXPublicPanelMenuSection section in sectionList)
		    foreach (BXPublicPanelMenu menu in section.Controls)
		        if (menu.InfluencePublicPanelVisibility)
		            return true;
		return false;
	}


}
