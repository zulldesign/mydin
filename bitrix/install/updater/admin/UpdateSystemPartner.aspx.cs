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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Bitrix.Security;
using System.IO;
using Bitrix.Modules;

public partial class bitrix_admin_UpdateSystemPartner : BXAdminPage, IComparer<BXUpdaterVersion>
{
	const string ControlsPath = "~/bitrix/admin/controls/SiteUpdater/UpdateSystemPartner/";
	protected BXUpdaterVersion mainModuleVersion;
	
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		DirectoryInfo info = new DirectoryInfo(MapPath(ControlsPath));
		if (!info.Exists)
			return;
		SortedDictionary<BXUpdaterVersion, string> dic = new SortedDictionary<BXUpdaterVersion,string>(this);
		foreach (FileInfo file in info.GetFiles("*.ascx"))
		{
			try
			{
				string ver = file.Name;
				if (!string.IsNullOrEmpty(file.Extension))
					ver = ver.Remove(ver.Length - file.Extension.Length);
				dic[new BXUpdaterVersion(ver)] = ControlsPath + file.Name;
			}
			catch
			{
				continue;
			}
		}
		Control system = null;
		try
		{
			string ver = Request.Form["MainModuleVersion"] ?? Request.QueryString["version"] ?? BXModuleManager.GetModule("main").Version;
			mainModuleVersion = new BXUpdaterVersion(ver);

			foreach (KeyValuePair<BXUpdaterVersion, string> kvp in dic)
			{
				try
				{
					if (mainModuleVersion < kvp.Key)
						continue;
					system = LoadControl(kvp.Value);
					break;
				}
				catch
				{
					continue;
				}
			}
		}
		catch
		{
		}
		if (system == null)
		{
			try
			{
				system = LoadControl(ControlsPath + "default.ascx");
			}
			catch
			{
			
			}
		}
		if (system == null)
			return;
		Content.Controls.Add(system);
	}

	#region IComparer<BXUpdaterVersion> Members

	int IComparer<BXUpdaterVersion>.Compare(BXUpdaterVersion x, BXUpdaterVersion y)
	{
		return -x.CompareTo(y);
	}

	#endregion
}
