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
using System.Collections.ObjectModel;
using Bitrix.DataLayer;

namespace Bitrix.Main.Components
{
	public class SystemSiteSelectorComponent : BXComponent
	{
		List<ASP.SiteSelector.SiteInfo> siteInfos = new List<ASP.SiteSelector.SiteInfo>();

		public ReadOnlyCollection<ASP.SiteSelector.SiteInfo> Sites
		{
			get
			{
				return siteInfos.AsReadOnly();
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			BXSiteCollection sites = BXSite.GetList(
				new BXFilter(new BXFilterItem(BXSite.Fields.Active, BXSqlFilterOperators.Equal, true)),
				new BXOrderBy(new BXOrderByPair(BXSite.Fields.Sort, BXOrderByDirection.Asc))
			);
			ComponentCache["sites"] = sites;
			foreach (BXSite site in sites)
			{
				ASP.SiteSelector.SiteInfo siteInfo = new ASP.SiteSelector.SiteInfo();
				siteInfo.directory = site.Directory;
				siteInfo.id = site.Id;
				siteInfo.name = site.SiteName;
				siteInfo.isSelected = BXSite.Current != null && string.Equals(BXSite.Current.Id, site.Id, StringComparison.InvariantCultureIgnoreCase);

				BXSiteDomainCollection domains = BXSiteDomain.GetList(new BXFilter(new BXFilterItem(BXSiteDomain.Fields.ID, BXSqlFilterOperators.Equal, site.Id)), null);
				siteInfo.domains = domains.ConvertAll<string>(delegate(BXSiteDomain input)
				{
					return input.Domain;
				});
				ComponentCache["_" + site.Id] = domains;

				siteInfos.Add(siteInfo);
			}

			IncludeComponentTemplate();
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessage("Component.Title");
			Description = GetMessage("Component.Description");
			Icon = "images/selector.gif";
			Group = new BXComponentGroup("system_menu", GetMessage("Group"), 100, BXComponentGroup.Utility);
			//ParamsDefinition.Add(BXParametersDefinition.Cache);
		}
	}
	public class SystemSiteSelectorTemplate : BXComponentTemplate<SystemSiteSelectorComponent>
	{
		public ReadOnlyCollection<ASP.SiteSelector.SiteInfo> Sites
		{
			get
			{
				return Component.Sites;
			}
		}
	}
}

#region Compatibility Issue
namespace ASP
{
	public partial class SiteSelector : Bitrix.Main.Components.SystemSiteSelectorComponent
	{
		//NESTED CLASSES
		public class SiteInfo
		{
			internal string id;
			internal string name;
			internal string directory;
			internal bool isSelected;
			internal List<string> domains;

			public string Id
			{
				get
				{
					return id;
				}
			}
			public string Name
			{
				get
				{
					return name;
				}
			}
			public string Directory
			{
				get
				{
					return directory;
				}
			}
			public bool IsSelected
			{
				get
				{
					return isSelected;
				}
			}
			public ReadOnlyCollection<string> Domains
			{
				get
				{
					return domains.AsReadOnly();
				}
			}

			internal SiteInfo()
			{

			}
		}
		public new class Template : BXComponentTemplate<SiteSelector>
		{
			public ReadOnlyCollection<ASP.SiteSelector.SiteInfo> Sites
			{
				get
				{
					return Component.Sites;
				}
			}
		}
	}
}
#endregion