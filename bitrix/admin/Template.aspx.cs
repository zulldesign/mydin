using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.ObjectModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix.Services;
using Bitrix.Main;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using Bitrix;

using Bitrix.Security;
using Bitrix.IO;
using Bitrix.Configuration;
using Bitrix.Services.Text;

public partial class bitrix_admin_Template : BXAdminPage
{
	bool currentUserCanModify = false;

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModify = this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		AddButton.Visible = currentUserCanModify;
		if (!currentUserCanModify)
			GridView.PopupCommandMenuId = PopupPanelView.ID;
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!IsPostBack)
			BindList();
	}

	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = GetMessage("MasterTitle");
		
	}

	#region BindList()
	private void BindList()
	{

	}
	#endregion

	protected void GridView_PopupMenuClick(object sender, BXPopupMenuClickEventArgs e)
	{
		BXGridView grid = sender as BXGridView;
		DataKey drv = grid.DataKeys[e.EventRowIndex];
		if (drv == null)
			return;

		switch (e.CommandName)
		{
			case "copy":
				if (currentUserCanModify)
				{
					string sourceDirectory = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, drv["ID"].ToString());
					string targetDirectory = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, drv["ID"] + "_copy");
					
					while (BXSecureIO.FileOrDirectoryExists(targetDirectory))
						targetDirectory += "_copy";
					BXSecureIO.DirectoryCopy(sourceDirectory, targetDirectory);
					GridView.MarkAsChanged();
				}
				break;
			case "edit":
				Response.Redirect(string.Format("TemplateEdit.aspx?id={0}", drv["ID"]));
				break;
			default:
				break;
		}
	}
	protected void Toolbar_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "add":
				Response.Redirect("TemplateEdit.aspx");
				break;
			default:
				break;
		}
	}

	#region GetPreviewHtml()
	public string GetPreviewHtml(string smallPreview, string bigPreview, string bigPreviewPath)
	{
		StringBuilder sb = new StringBuilder();

		if (!string.IsNullOrEmpty(bigPreview) || !string.IsNullOrEmpty(smallPreview))
			sb.Append("<br/>");
		else
			return string.Empty;

		if (!string.IsNullOrEmpty(bigPreview))
			using (System.Drawing.Image img = System.Drawing.Image.FromFile(bigPreviewPath))
			{
                sb.AppendFormat("<a target=\"_blank\" href=\"{0}\" onclick=\"ImgShw('{0}','{1}','{2}', ''); return false;\" title=\"{3}\">", bigPreview, img.Width, img.Height, GetMessage("Magnify"));
				img.Dispose();
			}

		string imgUrl;
		if (!string.IsNullOrEmpty(smallPreview))
			imgUrl = smallPreview;
		else
			imgUrl = bigPreview;

		if (!string.IsNullOrEmpty(imgUrl))
			sb.AppendFormat("<img width=\"130\" vspace=\"4\" hspace=\"0\" height=\"84\" border=\"0\" align=\"left\" src=\"{0}\"/>", imgUrl);


		if (!string.IsNullOrEmpty(bigPreview))
			sb.Append("</a>");

		return sb.ToString();
		;
	}
	#endregion
	protected void GridView_Select(object sender, BXSelectEventArgs e)
	{
		DataTable dt = new DataTable("Templates");
		dt.Columns.Add("ID", typeof(string));
		dt.Columns.Add("Name", typeof(string));
		dt.Columns.Add("Description", typeof(string));
		dt.Columns.Add("PreviewSmall", typeof(string));
		dt.Columns.Add("PreviewBig", typeof(string));
		dt.Columns.Add("PreviewBigPath", typeof(string));

		int i = -1;
		foreach (DirectoryInfo temp in new DirectoryInfo(BXPath.MapPath(BXConfigurationUtility.Constants.TemplatesFolderPath)).GetDirectories())
		{
			if (!File.Exists(Path.Combine(temp.FullName, BXConfigurationUtility.Constants.TemplateFileName)))
				continue;
			i++;
			if (i < e.PagingOptions.startRowIndex || i >= e.PagingOptions.startRowIndex + e.PagingOptions.maximumRows)
				continue;

			DataRow newRow = dt.NewRow();
			newRow["ID"] = temp.Name;

			string resorcePath = string.Format("{0}\\lang\\{1}\\include.lang", temp.FullName, BXLoc.CurrentLocale);
			if (File.Exists(resorcePath))
			{
				using (BXResourceFile res = new BXResourceFile(resorcePath))
				{
					if (res.ContainsKey("Template.Name"))
						newRow["Name"] = res["Template.Name"];

					if (res.ContainsKey("Template.Description"))
						newRow["Description"] = BXStringUtility.HtmlToText(res["Template.Description"] ?? "");
				}
			}

			if (File.Exists(temp.FullName + "\\preview.gif"))
			{
				newRow["PreviewSmall"] = ResolveClientUrl(BXPath.Combine(
					BXConfigurationUtility.Constants.TemplatesFolderPath,
					temp.Name,
					"preview.gif"
				));
			}

			if (File.Exists(temp.FullName + "\\screen.gif"))
			{
				newRow["PreviewBig"] = ResolveClientUrl(BXPath.Combine(
					BXConfigurationUtility.Constants.TemplatesFolderPath,
					temp.Name,
					"screen.gif"
				));
				newRow["PreviewBigPath"] = temp.FullName + "\\screen.gif";
			}

			dt.Rows.Add(newRow);
		}
		DataView view = new DataView(dt);
		view.Sort = e.SortExpression;

		e.Data = view;
	}
	protected void GridView_Delete(object sender, BXDeleteEventArgs e)
	{
		if (currentUserCanModify)
		{
			List<string> templates = new List<string>();
			if (e.Keys != null)
				templates.Add(e.Keys["ID"].ToString());
			else
				foreach (DirectoryInfo info in new DirectoryInfo(BXPath.MapPath(BXConfigurationUtility.Constants.TemplatesFolderPath)).GetDirectories())
					templates.Add(info.Name);

			foreach (string t in templates)
			{
				string templateDirectory = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, t);
				try
				{
					BXSecureIO.DirectoryDelete(templateDirectory, true);
				}
				catch (IOException)
				{
					BXSecureIO.DirectoryDelete(templateDirectory, true);
				}
			}
		}
	}
	protected void GridView_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = 0;
		foreach (string dir in Directory.GetDirectories(BXPath.MapPath(BXConfigurationUtility.Constants.TemplatesFolderPath)))
			if (File.Exists(Path.Combine(dir, BXConfigurationUtility.Constants.TemplateFileName)))
				e.Count++;
	}
}