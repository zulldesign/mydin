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
using Bitrix.Main;
using System.Text;
using System.Data.SqlClient;
using Bitrix.Modules;
using Bitrix.UI;
using Bitrix.Services;
using System.IO;
using Bitrix.IO;
using Bitrix.Configuration;



public partial class bitrix_admin_Default : BXAdminPage
{
	private BXAdminMenuItemCollection _menu;
	private string _mode;

	public BXAdminMenuItemCollection Menu
	{
		get
		{
			if (this._menu == null)
				this._menu = BXAdminMenuManager.GetModulesMenu(new string[] { });
			return this._menu;
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		spanSiteName.InnerText = BXOptionManager.GetOptionString("main", "site_name", "");
		spanVersion.InnerText = BXModuleManager.GetModule("main").Version;

		this._mode = this.GetRequestString("show_mode");
		if (String.IsNullOrEmpty(this._mode))
			this._mode = "icon";

		if (!Page.IsPostBack)
			DoShow();
	}

	private void DoShow()
	{
		int i = 0;
		foreach (BXAdminMenuItem menuItem in this.Menu)
		{
			HtmlTableRow tr = new HtmlTableRow();
			HtmlTableCell td = new HtmlTableCell();
			if (i > 0)
			{
				tr = new HtmlTableRow();

				td = new HtmlTableCell();
				td.InnerHtml = "<div class=\"section-line\">&nbsp;</div>";
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				tr.Cells.Add(td);

				mainPageTable.Rows.Add(tr);
			}

			tr = new HtmlTableRow();
			tr.VAlign = "top";

			td = new HtmlTableCell();
			td.Align = "center";
			td.Attributes["class"] = "section-container";
			td.InnerHtml = String.Format(
				"<a href=\"{0}\" title=\"{1}\"><div class=\"section-icon\" id=\"{2}\"></div><div class=\"section-text\">{3}</div></a>",
				Encode(BXPath.VirtualRootPath + menuItem.Url),
				menuItem.Hint,
				menuItem.IndexIcon,
				menuItem.Title
			);
			tr.Cells.Add(td);

			td = new HtmlTableCell();
			td.Attributes["class"] = "items-container";
			StringBuilder sb = new StringBuilder();
			foreach (BXAdminMenuItem subMenuItem in menuItem.Children)
			{
				if (this._mode.Equals("list", StringComparison.InvariantCultureIgnoreCase))
				{
					sb.Append("<div class=\"item-container\">");
					if (!String.IsNullOrEmpty(subMenuItem.Url))
					{
						sb.Append(
							String.Format(
								"<a href=\"{0}\" title=\"{1}\"><div class=\"item-icon\" id=\"{2}\"></div></a>",
								Encode(BXPath.VirtualRootPath + subMenuItem.Url),
								subMenuItem.Hint,
								subMenuItem.Icon
							)
						);
						sb.Append(
							String.Format(
								"<div class=\"item-block\"><a href=\"{0}\" title=\"{1}\">{2}</a></div>",
								Encode(BXPath.VirtualRootPath + subMenuItem.Url),
								subMenuItem.Hint,
								subMenuItem.Title
							)
						);
					}
					else
					{
						sb.Append(
							String.Format(
								"<div class=\"item-icon\" id=\"{0}\"></div><div class=\"item-block\">{1}</div>",
								subMenuItem.Icon,
								subMenuItem.Title
							)
						);
					}
					sb.Append("</div>");
				}
				else
				{
					sb.Append("<div class=\"icon-container\" align=\"center\">");
					if (!String.IsNullOrEmpty(subMenuItem.Url))
					{
						sb.Append(
							String.Format(
								"<a href=\"{0}\" title=\"{1}\"><div class=\"icon-icon\" id=\"{2}\"></div><div class=\"icon-text\">{3}</div></a>",
								Encode(BXPath.VirtualRootPath + subMenuItem.Url),
								subMenuItem.Hint,
								subMenuItem.PageIcon,
								subMenuItem.Title
							)
						);
					}
					else
					{
						sb.Append(
							String.Format(
								"<div class=\"icon-icon\" id=\"{0}\"></div><div class=\"icon-text\">{1}</div>",
								subMenuItem.PageIcon,
								subMenuItem.Title
							)
						);
					}
					sb.Append("</div>");
				}
			}
			td.InnerHtml = sb.ToString();
			tr.Cells.Add(td);
			mainPageTable.Rows.Add(tr);
			i++;
		}
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "icon":
				this._mode = "icon";
				DoShow();
				break;
			case "list":
				this._mode = "list";
				DoShow();
				break;
		}
	}
}
