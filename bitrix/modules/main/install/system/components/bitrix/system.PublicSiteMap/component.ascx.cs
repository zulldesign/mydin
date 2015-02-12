using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

using Bitrix;
using Bitrix.Components;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.IO;
using Bitrix.Configuration;
using Bitrix.Services.Js;
using Bitrix.UI.Popup;
using Bitrix.Security;

namespace Bitrix.Main.Components
{
	public class SystemPublicSiteMapComponent : BXComponent
	{
		public BXPublicMenuItemCollection RootNode
		{
			get
			{
				return Results.Get<BXPublicMenuItemCollection>("root");
			}
		}

		public int NumberOfColumns
		{
			get
			{
				return Parameters.Get("NumberOfColumns", 1);
			}
		}

		public int Depth
		{
			get
			{
				return Parameters.Get("depth", 0);
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			int depth = Parameters.Get("depth", 0);
			int numberOfColumns = Parameters.Get("NumberOfColumns", 1);

			string siteId;
			if (!IsComponentDesignMode)
				siteId = BXSite.Current.Id;
			else
				siteId = DesignerSite;

			if (String.IsNullOrEmpty(siteId))
				siteId = BXSite.DefaultSite.Id;

			BXPublicMenuItemCollection siteMap = BXPublicMenu.Menu.GetSiteMap(siteId);
			Results["root"] = siteMap != null ? siteMap[0].Children : null;

			IncludeComponentTemplate();
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessage("Title");
			Description = GetMessage("Description");
			Icon = "component.gif";

			Group = new BXComponentGroup("system_menu", GetMessage("Group"), 100, BXComponentGroup.Utility);
			BXCategory mainCategory = BXCategory.Main;

			ParamsDefinition["Depth"] = new BXParamText(GetMessageRaw("Component.Parameter.Title.MenuDepth"), "0", mainCategory);
			ParamsDefinition["NumberOfColumns"] = new BXParamText(GetMessageRaw("Component.Parameter.Title.QuantityOfColumns"), "1", mainCategory);

			//ParamsDefinition["MenuName"] = new BXParamSingleSelection("Тип меню", "top", BXCategory.Main);
			//ParamsDefinition.Add(BXParametersDefinition.Cache);
		}

		protected override void LoadComponentDefinition()
		{
		}

		
	}
	public class SystemPublicSiteMapTemplate : BXComponentTemplate<SystemPublicSiteMapComponent>
	{
	}
}

#region Compatibility Issue
public partial class PublicSiteMapComponent : Bitrix.Main.Components.SystemPublicSiteMapComponent
{
	//NESTED CLASSES
	public class Template : BXComponentTemplate<PublicSiteMapComponent>
	{

	}
}
#endregion