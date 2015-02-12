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
using Bitrix.Modules;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Bitrix.Services;
using Bitrix.Security;
using System.Diagnostics;
using Bitrix.UI;
using Bitrix.Services.Js;

public partial class bitrix_kernel_AdminMenu : Bitrix.UI.BXControl
{
	private List<string> _activeSections;
	private List<string> _openSections;
	private BXAdminMenuItemCollection _menu;
	private string _activeMenuKey;

	public string ActiveMenuKey
	{
		get
		{
			return this._activeMenuKey;
		}
		set
		{
			this._activeMenuKey = value;
		}
	}

	public List<string> ActiveSections
	{
		get
		{
			if (this._activeSections == null)
				BXAdminMenuManager.SearchActiveSections(this.Menu, ref this._activeSections, ref this._activeMenuKey);
			return this._activeSections;
		}
	}

	public List<string> OpenSections
	{
		get
		{
			if (this._openSections == null)
				BXAdminMenuManager.SearchOpenSections(ref this._openSections);
			return this._openSections;
		}
	}

	public string OpenSectionsString
	{
		get
		{
			List<string> o = this.OpenSections;
			string res = "";
			foreach (string s in o)
			{
				if (res.Length > 0)
					res += ",";
				res += s;
			}
			return res;
		}
	}

	public BXAdminMenuItemCollection Menu
	{
		get
		{
			if (this._menu == null)
				this._menu = BXAdminMenuManager.GetModulesMenu(this.OpenSections.ToArray());
			return this._menu;
		}
	}


	protected override void Render(HtmlTextWriter writer)
	{
		if (Menu.Count > 0 && Context.User.Identity.IsAuthenticated && (HttpContext.Current.User as BXPrincipal).IsCanOperate(BXRoleOperation.Operations.AccessAdminSystem))
			base.Render(writer);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		 Visible = Context.User != null && Context.User.Identity.IsAuthenticated;

		if (this.ActiveSections != null && this.ActiveSections.Count > 0)
			LabelMenuTitle.Text = this.Menu[this.ActiveSections[0]].Title;
		else
			LabelMenuTitle.Text = GetMessage("LabelText.LabelMenuTitle");

		StringBuilder menuString = new StringBuilder();

		HtmlTableRow smTr = new HtmlTableRow();
		smButtonsContainer.Rows.Add(smTr);
		foreach (BXAdminMenuItem menuItem in this.Menu)
		{
			// Full list
			HtmlTableRow tr = new HtmlTableRow();
			HtmlTableCell td = new HtmlTableCell();

			td.ID = string.Format("btn_{0}", menuItem.ItemId);
			if (this.ActiveSections[0] == menuItem.ItemId)
				td.Attributes.Add("class", "button buttonsel");
			else
				td.Attributes.Add("class", "button");
			td.Attributes.Add("onmouseover", "this.className+=' buttonover';");
			td.Attributes.Add("onmouseout", "this.className=this.className.replace(/\\s*buttonover/ig, '');");
			td.Attributes.Add("onclick", string.Format("JsAdminMenu.ToggleMenu('{0}', '{1}');", menuItem.ItemId, menuItem.Title));
			td.Attributes.Add("title", menuItem.Hint);

			td.InnerHtml = String.Format("<table cellspacing=\"0\">" +
				"<tr>" +
				"<td class=\"left\"></td>" +
				"<td class=\"center\"><div id=\"{0}\">{1}</div></td>" +
				"<td class=\"right\"></td>" +
				"</tr>" +
				"</table>", menuItem.Icon, menuItem.Title);

			tr.Cells.Add(td);
			buttonsContainer.Rows.Add(tr);


			// Short list
			HtmlTableCell smTd = new HtmlTableCell();
			smTd.ID = string.Format("smbtn_{0}", menuItem.ItemId);
			if (this.ActiveSections[0] == menuItem.ItemId)
				smTd.Attributes.Add("class", "smbutton smbuttonsel");
			else
				smTd.Attributes.Add("class", "smbutton");
			smTd.Attributes.Add("onmouseover", "this.className+=' smbuttonover';");
			smTd.Attributes.Add("onmouseout", "this.className=this.className.replace(/\\s*smbuttonover/ig, '');");
			smTd.Attributes.Add("onclick", string.Format("JsAdminMenu.ToggleMenu('{0}', '{1}');", menuItem.ItemId, menuItem.Title));
			smTd.Attributes.Add("title", menuItem.Hint);

			smTd.InnerHtml = string.Format("<div id=\"{0}\">", menuItem.Icon);
			smTr.Cells.Add(smTd);


			// SubLists
			menuString.Append(string.Format("<div id=\"{0}\" style=\"display:{1}\">", menuItem.ItemId, ((this.ActiveSections[0] == menuItem.ItemId) ? "block" : "none")));

			foreach (BXAdminMenuItem subItem in menuItem.Children)
				AddSubMenuItems(subItem, menuString, 0);
			menuString.Append("</div>");
		}
        
        ClientScriptManager csm = Page.ClientScript;
        if (csm == null)
            throw new InvalidOperationException("Could not find client script manager!");
        
		BXPage.RegisterScriptInclude("~/bitrix/js/Main/admin_tools.js");
		BXPage.RegisterScriptInclude("~/bitrix/js/Main/utils.js");

        if (!csm.IsStartupScriptRegistered(GetType(), "initialize"))
            csm.RegisterStartupScript(
                GetType(),
                "initialize",
                string.Format(
                    @"var JsAdminMenu=new JCAdminMenu('{0}', '{2}');
                    JsAdminMenu.sMenuSelected='{1}';",
                    this.OpenSectionsString,
                    ((this.ActiveSections != null && this.ActiveSections.Count > 0) ? this.ActiveSections[0] : ""),
					ClientID
				),
                true);
        //---

		LabelMenuContainer.Text = menuString.ToString();
	}

	protected void Page_PreRender(object sender, EventArgs e)
	{
		if (Menu.Count == 0)
			Visible = false;
	}


    //ZG
    public string WidthAsString 
    {
        get 
        {
            int? storedWidth = BXAdminMenuManager.GetWidth();
            return storedWidth.HasValue ? string.Format("{0}px", storedWidth.ToString()) : "auto";
        }
    }

	private void AddSubMenuItems(BXAdminMenuItem menuItem, StringBuilder menuString, int level)
	{
        //BXLogService.Providers[""].LogMessage(new BXLogMessage());
        //Debugger.Launch();
		bool haveSubMenu = (menuItem.Children.Count > 0);
		bool isSectionActive = this.ActiveSections.Contains(menuItem.ItemId) || this.OpenSections.Contains(menuItem.ItemId);

		menuString.Append("<div class=\"menuline\"><table cellspacing=\"0\"><tr>");
		for (int i = 0; i < level; i++)
			menuString.Append("<td><div class=\"menuindent\"></div></td>");

        //...ZG, BUG#3
        //if (menuItem.Dynamic /*&& !isSectionActive*/ && !this.OpenSections.Contains(menuItem.ItemId))
        if (menuItem.Dynamic)
		{
            //Bitrix.DataLayer.BXFormFilter fltr = new Bitrix.DataLayer.BXFormFilter();
            //Bitrix.DataLayer.BXFormFilterItem item = new Bitrix.DataLayer.BXFormFilterItem("SectionId", Convert.ToInt32(menuItem.ItemId), Bitrix.DataLayer.BXSqlFilterOperators.Equal);
            //fltr.Add(item);
            //int count = Bitrix.IBlock.BXIBlockSectionManager.Count(fltr);
            if (menuItem.RegardAsHasChildren)
            {
                menuString.Append(
                    string.Format(
                        "<td><div class=\"sign {0}\" onclick=\"JsAdminMenu.ToggleDynSection(this, '{1}', '{2}', '{3}')\"></div></td>",
                        haveSubMenu && isSectionActive ? "signminus" : "signplus",
                        menuItem.ModuleId,
                        menuItem.ItemId,
                        level + 1
                    )
                );
            }
            else
                menuString.Append("<td><div class=\"sign signdot\"></div></td>");
		}
		else
		{
			menuString.Append(
				string.Format(
					"<td><div class=\"sign {0}\" {1}></div></td>",
                    (haveSubMenu ? (isSectionActive ? "signminus" : "signplus") : "signdot"),
					(haveSubMenu ? string.Format("onclick=\"JsAdminMenu.ToggleSection(this, '{0}', {1})\"", menuItem.ItemId, (level + 1)) : "")
				)
			);
		}

		//menuString.Append(
		//    string.Format(
		//        "<td class=\"menuicon\">{0}</td>",
		//        ((!String.IsNullOrEmpty(menuItem.Icon) && menuItem.Url != null) ? String.Format("<a href=\"{0}\" title=\"{1}\" id=\"{2}\"></a>", this.Request.ApplicationPath + menuItem.Url, menuItem.Hint, menuItem.ItemId) : "")
		//    )
		//);

		menuString.Append(
			string.Format(
				"<td class=\"menuicon\">{0}</td>",
				(!String.IsNullOrEmpty(menuItem.Icon)) ? String.Format("<a id=\"{0}\"></a>", menuItem.Icon) : ""
			)
		);

        //...ZG, BUG#3
		if (menuItem.Url != null)
			menuString.Append(
				string.Format(
					//"<td class=\"menutext\"><a href=\"{0}\" title=\"{1}\"{2}>{3} ({4}, {5}{6}{7})</a></td></tr></table>",
                    "<td class=\"menutext\"><a href=\"{0}\" title=\"{1}\"{2}>{3}</a></td></tr></table>",
					((this.Request.ApplicationPath.Length > 0 && !this.Request.ApplicationPath.Equals("/", StringComparison.InvariantCultureIgnoreCase)) ? this.Request.ApplicationPath : "") + menuItem.Url,
					menuItem.Hint,
					(menuItem.ItemId == this.ActiveMenuKey ? " class=\"active\"" : ""),
					menuItem.Title
                    //menuItem.Children.Count, //...указываем кол-во потомков;
                    //menuItem.Dynamic ? "D" : "",
                    //this.ActiveSections.Contains(menuItem.ItemId) ? "A" : "",
                    //this.OpenSections.Contains(menuItem.ItemId) ? "O" : ""
				)
			);
		else
			menuString.Append(
				string.Format(
					"<td class=\"menutext menutext-no-url\">{0}</td></tr></table>",
					menuItem.Title
				)
			);

        //ZG, BUG#3
        if (!haveSubMenu && this._openSections != null && this._openSections.Contains(menuItem.ItemId)) 
            this._openSections.Remove(menuItem.ItemId);

		if (haveSubMenu || menuItem.Dynamic)
		{
			menuString.Append(
				string.Format(
					"<div id=\"{0}\" style=\"display:{1};\">",
					menuItem.ItemId,
					(isSectionActive ? "block" : "none")
				)
			);

			if (!menuItem.Dynamic || haveSubMenu)
				foreach (BXAdminMenuItem subItem in menuItem.Children)
					AddSubMenuItems(subItem, menuString, level + 1);

			menuString.Append("</div>");
		}

		menuString.Append("</div>");
	}

    protected string GenerateVerticalMenuHtmlMarkup(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        StringBuilder result = new StringBuilder();
        for(int i = 0; i < text.Length; i++)
            result.AppendFormat("{0}<br/>", text[i]);

        return result.ToString();
    }
}
