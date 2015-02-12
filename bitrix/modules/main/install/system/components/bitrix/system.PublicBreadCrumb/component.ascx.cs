using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

using Bitrix;
using Bitrix.Components;
using Bitrix.Configuration;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Js;
using Bitrix.UI;
using Bitrix.UI.Popup;

namespace Bitrix.Main.Components
{
	public class SystemPublicBreadCrumbComponent : BXComponent
	{
		BXPublicMenuItem[] crumbs;

		public BXPublicMenuItem[] BreadCrumb
		{
			get
			{
				return crumbs ?? LoadCrumbs();
			}
		}

		private BXPublicMenuItem[] LoadCrumbs()
		{
			if (!Parameters.GetBool("ShowOnIndexPage", false) && IsRootPage(new Uri(BXUri.ToAbsoluteUri(Page.Request.RawUrl))))
				return null;

			crumbs = BXPublicMenu.Menu.GetBreadCrumbByUri(BXSefUrlManager.CurrentUrl.PathAndQuery);
			if (crumbs != null && crumbs.Length > 0)
			{
				string rootItemTitle = Parameters.GetString("RootItemTitle");
				if (!String.IsNullOrEmpty(rootItemTitle))
					crumbs[0].Title = rootItemTitle;
			}

			return crumbs;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			
			IncludeComponentTemplate();
		}

		private bool IsRootPage(Uri requestURI)
		{
			string siteUri = BXSite.Current.DirectoryAbsolutePath;
			return
				(
					String.Equals(HttpUtility.UrlDecode( requestURI.AbsolutePath), siteUri, StringComparison.OrdinalIgnoreCase) ||
					String.Equals(requestURI.AbsolutePath, siteUri + "default.aspx", StringComparison.OrdinalIgnoreCase)
				);
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessage("Title");
			Description = GetMessage("Description");
			Icon = "component.gif";

			Group = new BXComponentGroup("system_menu", GetMessage("Group"), 100, BXComponentGroup.Utility);

			BXCategory mainCategory = BXCategory.Main;
			ParamsDefinition["RootItemTitle"] = new BXParamText(GetMessageRaw("Component.Parameter.Title.RootItemTitle"), BXLoc.GetModuleMessage("main", "PublicMenu.RootItem.Title"), mainCategory);
			ParamsDefinition["ShowOnIndexPage"] = new BXParamYesNo(GetMessageRaw("Component.Parameter.Title.ShowOnIndexPage"), false, mainCategory);

		}
	}
	public class SystemPublicBreadCrumbTemplate : BXComponentTemplate<SystemPublicBreadCrumbComponent>
	{
	}
}

#region Compatibility Issue
public partial class PublicBreadCrumbComponent : Bitrix.Main.Components.SystemPublicBreadCrumbComponent
{
	//NESTED CLASSES
	public class Template : BXComponentTemplate<PublicBreadCrumbComponent>
	{
	}
}
#endregion