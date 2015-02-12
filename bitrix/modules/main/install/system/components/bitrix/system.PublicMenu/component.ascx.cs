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
using Bitrix.Services.Text;
using Bitrix.UI.Hermitage;

namespace Bitrix.Main.Components
{
	public class SystemPublicMenuComponent : BXComponent
	{
		bool menuLoaded;
		public BXPublicMenuItemCollection Menu
		{
			get { return ComponentCache.Get<BXPublicMenuItemCollection>("menu"); }
			private set { ComponentCache["menu"] = value; }
		}

		public bool HasMenu
		{
			get { return Menu != null; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			EnsureMenu();
			IncludeComponentTemplate();
		}

		private void EnsureMenu()
		{
			if (menuLoaded)
				return;
			if (InternalSiteMenuTypes.Count > 0)
			{
				int depth = Parameters.GetInt("depth", 1);

				var realTypes = MenuTypes.FindAll(x => InternalSiteMenuTypes.Exists(y => string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase)));
			
				string url = Parameters.GetString("Url");
				if (!BXStringUtility.IsNullOrTrimEmpty(url))
				{
					if (url.StartsWith("~/"))
						url = VirtualPathUtility.ToAbsolute(url);
					else if (!url.StartsWith("/"))
						url = VirtualPathUtility.ToAbsolute("~/") + url;
				}
				else
					url = DesignerUrl.PathAndQuery;

				try
				{
					Menu = BXPublicMenu.Menu.GetMenuTreeByUri(realTypes.ToArray(), url, depth);
				}
				catch (Exception /*exc*/) { }
			}
			menuLoaded = true;
		}

		private List<string> _siteMenuTypes = null;
		protected List<string> InternalSiteMenuTypes
		{
			get
			{
				if (_siteMenuTypes != null)
					return _siteMenuTypes;
				_siteMenuTypes = new List<string>(2);
				foreach (KeyValuePair<string, string> kv in BXPublicMenu.GetMenuTypes(DesignerSite))
					_siteMenuTypes.Add(kv.Key);
				return _siteMenuTypes;
			}
		}

		private List<string> menuTypes;
		List<string> MenuTypes
		{
			get
			{
				return menuTypes ?? (menuTypes = Parameters.GetListString("MenuTypes").FindAll(x => !string.IsNullOrEmpty(x)));
			}
		}

		private string MenuPath
		{
			get
			{
				if (Menu == null)
				{
					if (MenuTypes.Count == 0)
						return null;
					HttpRequest request = HttpContext.Current != null ? HttpContext.Current.Request : null;
					if (request == null)
						throw new InvalidOperationException("Could not find request!");
					return BXPath.Combine(BXPath.GetDirectory(VirtualPathUtility.ToAppRelative(request.FilePath)), MenuTypes[0] + ".menu");
				}
				return Menu.MenuFilePath;
			}
		}

		private string Redirect2EditPageJavascript(string menuPath)
		{
			if (string.IsNullOrEmpty(menuPath))
				menuPath = MenuPath;
			return string.Format(
				"jsUtils.Redirect(arguments, '{0}')",
				BXJSUtility.Encode(string.Format(
					"{0}?path={1}&{2}={3}",
					BXPath.ToVirtualAbsolutePath("~/bitrix/admin/MenuEdit.aspx"),
					HttpUtility.UrlEncode(menuPath),
					BXConfigurationUtility.Constants.BackUrl,
					HttpUtility.UrlEncode(Request.RawUrl)
				))
			);

		}

		private string OpenEditDialogJavascript(string menuPath)
		{
			if (string.IsNullOrEmpty(menuPath))
				menuPath = MenuPath;

			return string.Format(
				"(new BX.CDialogNet({{ 'content_url':'{0}?path={1}&{2}={3}&lang={4}&clientType=WindowManager', 'min_width':'400', 'min_height':'250', 'width':'720', 'height':'425' }})).Show();",
					BXJSUtility.Encode(BXPath.ToVirtualAbsolutePath("~/bitrix/dialogs/MenuEdit.aspx")),
                    BXJSUtility.Encode(HttpUtility.UrlEncode(menuPath)),
					BXConfigurationUtility.Constants.BackUrl,
                    BXJSUtility.Encode(HttpUtility.UrlEncode(Request.RawUrl)),
                    BXJSUtility.Encode(HttpUtility.UrlEncode(BXLoc.CurrentLocale)));
		}


		/// <summary>
		/// Создание эл-тов контекстного меню
		/// </summary>
		/// <returns></returns>
		public BXPopupMenuBaseItem[] CreateToolbarPopupMenuItems(BXShowMode showMode)
		{
			if (showMode != BXShowMode.View)
			{
				bool hasOperation =
					BXSite.Current != null
					? BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", BXSite.Current.TextEncoder.Decode(BXSite.Current.Id))
					: BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit);

				List<BXPopupMenuBaseItem> result = new List<BXPopupMenuBaseItem>();


				string dir = null;
				string file = null;
				BXPath.BreakPath(Page.AppRelativeVirtualPath, ref dir, ref file);

				if (Menu != null)
				{
					var menus = new List<string>();
					ScanMenu(Menu, menus);
					Dictionary<string, int> levels = new Dictionary<string,int>();
					foreach (var path in menus)
					{
						if (!BXSecureIO.FileExists(path) || (!hasOperation && BXSecureIO.CheckWrite(path)))
							continue;

						string typeName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
						int count;
						levels.TryGetValue(typeName, out count);
						levels[typeName] = ++count;
						
						string menuTypeTitle;
						switch (typeName)
						{
							case "top":
								menuTypeTitle = GetMessage("PopupMenuItem.Text.Part.TopMenu");
								break;
							case "left":
								menuTypeTitle = GetMessage("PopupMenuItem.Text.Part.LeftMenu");
								break;
							case "bottom":
								menuTypeTitle = GetMessage("PopupMenuItem.Text.Part.BottomMenu");
								break;
							default:
								menuTypeTitle = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.MenuOfType"), "\"" + BXPublicMenu.GetMenuTitle(BXSite.Current.Id, typeName) + "\"");
								break;
						}
						
						BXPopupMenuItem item = new BXPopupMenuItem();
						item.IconCssClass = "icon menu-edit";
						item.Text = string.Format(GetMessage(count == 1 ? "PopupMenuItem.Text.FormatPart.EditMenuItems" : "PopupMenuItem.Text.FormatPart.EditMenuItemsWithLevels"), menuTypeTitle, count.ToString());
						item.ClientClickScript = OpenEditDialogJavascript(path);
						result.Add(item);
					}
				}

				var processed = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
				foreach (var menuType in MenuTypes)
				{
					if (processed.Contains(menuType))
						continue;
					processed.Add(menuType);

					string sectionMenu = null;
					if (dir != null)
						sectionMenu = BXPath.Combine(dir, menuType + ".menu");

					//Отображается во всех режимах
					if (!string.IsNullOrEmpty(sectionMenu) && !BXSecureIO.FileExists(sectionMenu) && (hasOperation || BXSecureIO.CheckWrite(sectionMenu)))
					{
						BXPopupMenuItem item = new BXPopupMenuItem();
						item.IconCssClass = "icon menu-edit";

						string createTypeNameLoc;
						switch (menuType.ToLowerInvariant())
						{
							case "top":
								createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateTopMenu");
								break;
							case "left":
								createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateLeftMenu");
								break;
							case "bottom":
								createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateBottomMenu");
								break;
							default:
								createTypeNameLoc = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.MenuOfType"), BXPublicMenu.GetMenuTitle(BXSite.Current.Id, menuType));
								break;
						}
						item.Text = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.CreateMenuInThisSection"), createTypeNameLoc);
						item.ClientClickScript = OpenEditDialogJavascript(sectionMenu);
						result.Add(item);
					}
				}
				return result.ToArray();
			}
			return null;
		}

		private void ScanMenu(BXPublicMenuItemCollection menu, List<string> menus)
		{
			var path = menu.MenuFilePath;

			if (!string.IsNullOrEmpty(path))
			{
				if (!menus.Exists(x => string.Equals(x, path, StringComparison.InvariantCultureIgnoreCase)))	
						menus.Add(path);
			}

			foreach (var m in menu)
			{
				if (!m.IsAccessible || !m.IsSelected)
					continue;
				if (m.Children != null)
					ScanMenu(m.Children, menus);
			}
		}

		public override BXComponentPopupMenuInfo PopupMenuInfo
		{
			get
			{
				BXComponentPopupMenuInfo info = base.PopupMenuInfo;
				info.CreateComponentContentMenuItems = delegate(BXShowMode showMode) 
				{
					if (showMode == BXShowMode.View)
						return new BXHermitagePopupMenuBaseItem[0];

					bool hasOperation =
						BXSite.Current != null
						? BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", BXSite.Current.TextEncoder.Decode(BXSite.Current.Id))
						: BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit);

					List<BXHermitagePopupMenuBaseItem> result = new List<BXHermitagePopupMenuBaseItem>();

					string dir = null,
						file = null;

					BXPath.BreakPath(Page.AppRelativeVirtualPath, ref dir, ref file);

					if (Menu != null)
					{
						var menus = new List<string>();
						ScanMenu(Menu, menus);
						Dictionary<string, int> levels = new Dictionary<string,int>();
						BXHermitagePopupMenuItemContainer container = null;
						foreach (var path in menus)
						{
							if (!BXSecureIO.FileExists(path) || (!hasOperation && BXSecureIO.CheckWrite(path)))
								continue;

							string typeName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
							int count;
							levels.TryGetValue(typeName, out count);
							levels[typeName] = ++count;
							
							string menuTypeTitle;
							switch (typeName)
							{
								case "top":
									menuTypeTitle = GetMessage("PopupMenuItem.Text.Part.TopMenu");
									break;
								case "left":
									menuTypeTitle = GetMessage("PopupMenuItem.Text.Part.LeftMenu");
									break;
								case "bottom":
									menuTypeTitle = GetMessage("PopupMenuItem.Text.Part.BottomMenu");
									break;
								default:
									menuTypeTitle = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.MenuOfType"), "\"" + BXPublicMenu.GetMenuTitle(BXSite.Current.Id, typeName) + "\"");
									break;
							}
							
							if(container == null)
							{
								container = new BXHermitagePopupMenuItemContainer();
								result.Add(container);

								container.Id = string.Concat("MENU_EDIT_", typeName.ToUpperInvariant());
								container.IconCssClass = "bx-context-toolbar-edit-icon";
								container.Text = string.Format(GetMessage(count == 1 ? "PopupMenuItem.Text.FormatPart.EditMenuItems" : "PopupMenuItem.Text.FormatPart.EditMenuItemsWithLevels"), menuTypeTitle, count.ToString());
								container.ClientClickScript = OpenEditDialogJavascript(path);
								container.Sort = 10;
							}
							else
							{
								BXHermitagePopupMenuItem item = container.CreateItem();
								item.Id = string.Concat("MENU_EDIT_", typeName.ToUpperInvariant());
								item.IconCssClass = "bx-context-toolbar-edit-icon";
								item.Text = string.Format(GetMessage(count == 1 ? "PopupMenuItem.Text.FormatPart.EditMenuItems" : "PopupMenuItem.Text.FormatPart.EditMenuItemsWithLevels"), menuTypeTitle, count.ToString());
								item.ClientClickScript = OpenEditDialogJavascript(path);
								item.Sort = count * 10;
							}
						}
					}


					var processed = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
					for (int i = 0; i < MenuTypes.Count; i++)
					{
						var menuType = MenuTypes[i];
						if (processed.Contains(menuType))
							continue;
						processed.Add(menuType);

						string sectionMenu = null;
						if (dir != null)
							sectionMenu = BXPath.Combine(dir, menuType + ".menu");

						//Отображается во всех режимах
						if (!string.IsNullOrEmpty(sectionMenu) && !BXSecureIO.FileExists(sectionMenu) && (hasOperation || BXSecureIO.CheckWrite(sectionMenu)))
						{
							BXHermitagePopupMenuItem item = new BXHermitagePopupMenuItem();

							string createTypeNameLoc;
							switch (menuType.ToLowerInvariant())
							{
								case "top":
									createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateTopMenu");
									break;
								case "left":
									createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateLeftMenu");
									break;
								case "bottom":
									createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateBottomMenu");
									break;
								default:
									createTypeNameLoc = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.MenuOfType"), BXPublicMenu.GetMenuTitle(BXSite.Current.Id, menuType));
									break;
							}
							item.Id = string.Concat("MENU_CREATE_", menuType.ToUpperInvariant());
							item.Text = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.CreateMenuInThisSection"), createTypeNameLoc);
							item.IconCssClass = "bx-context-toolbar-create-icon";
							item.AddLayoutFlag(BXHermitagePopupMenuLayoutFlag.Auxiliary);
							item.ClientClickScript = OpenEditDialogJavascript(sectionMenu);
							item.Sort = (i + 1) * 10;
							result.Add(item);
						}
					}
					return result.ToArray();			
				};

				return info;
			}
		}

		/*
		public override void CreatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
		{
			if (MenuPath == null)
				return;

			bool canOperate =
				(BXSite.Current != null
				? BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", BXSite.Current.TextEncoder.Decode(BXSite.Current.Id))
				: BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit))
				| BXSecureIO.CheckWrite(MenuPath);

			if (!canOperate)
				return;

			if (sectionList == null)
				throw new ArgumentNullException("sectionList");

			BXPublicPanelPopulator.EnsureMenu(BXPublicPanelMenuEntry.MenuGeneralMenu, BXShowMode.All, sectionList);
		}
		*/
		public override void CreateHermitagePublicPanelMenu(BXHermitagePublicPanelInfo info)
		{
			if (MenuPath == null)
				return;

			if(info == null)
				return;

			bool canOperate =
				(BXSite.Current != null
				? BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", BXSite.Current.TextEncoder.Decode(BXSite.Current.Id))
				: BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit))
				|| BXSecureIO.CheckWrite(MenuPath);

			if(!canOperate)
				return;

			BXHermitagePublicPanelSectionInfo structure = info.EnsureStandardSection(BXHermitagePublicPanelInfo.StandardSections.Structure.Id);
			BXHermitagePublicPanelMenuInfo menu = structure.GetMenuById("Bitrix.PublicMenu");
			if(menu == null)
			{
				menu = new BXHermitagePublicPanelMenuInfo("Bitrix.PublicMenu", GetMessageRaw("PublicMenu.Title"));
				menu.Sort = 0;
				menu.HintTitle = GetMessageRaw("PublicMenu.Title");
				menu.Hint = GetMessageRaw("PublicMenu.Hint");
				menu.Size = BXHermitagePublicPanelMenuSize.Small;
				menu.IconCssClass = "bx-panel-menu-icon";

				structure.Menus.Add(menu);
			}
		}

		/*
		public override void PopulatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
		{
			EnsureMenu();

			if (sectionList == null)
				return;
			BXPublicPanelMenu menu = BXPublicPanelPopulator.FindMenu(BXPublicPanelMenuEntry.MenuGeneralMenu, sectionList);
			if (menu == null)
				return;

			string dir = null;
			string file = null;
			if (!BXPath.BreakPath(Page.AppRelativeVirtualPath, ref dir, ref file))
			{
				base.PopulatePublicPanelMenu(sectionList);
				return;
			}

			BXPopupMenuV2 menuPopup = menu.PopupMenu;
			if (menuPopup == null)
				menuPopup = menu.PopupMenu = new BXPopupMenuV2();
			BXPopupMenuBaseItem[] menuItems = menuPopup.Items;

			// we need theese to create separator correctly
			bool hasModify = false;
			bool hasCreate = false;
			foreach (var m in menuItems)
			{
				if (m.ID == null || !m.ID.StartsWith("MenuEdit_", StringComparison.Ordinal))
					continue;

				if (!hasModify)
					hasModify = m.ID.EndsWith("_Modify", StringComparison.Ordinal);

				if (!hasCreate)
					hasCreate = m.ID.EndsWith("_Create", StringComparison.Ordinal);

				if (hasModify && hasCreate)
					break;
			}

			bool canOperate =
				BXSite.Current != null
				? BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", BXSite.Current.TextEncoder.Decode(BXSite.Current.Id))
				: BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit);

			var processed = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var typeName in MenuTypes)
			{
				if (processed.Contains(typeName))
					continue;
				processed.Add(typeName);

				string menuItemPrefix = string.Format("MenuEdit_{0}_", typeName);
				if (Array.FindIndex(menuItems, obj => obj.ID != null && obj.ID.StartsWith(menuItemPrefix, StringComparison.Ordinal)) < 0)
				{
					string sectionMenu = BXPath.Combine(dir, typeName + ".menu");
					if (!canOperate && !BXSecureIO.CheckWrite(sectionMenu))
						continue;
					
					bool create = !BXSecureIO.FileExists(sectionMenu);

					BXPopupMenuSeparator createSeparator = null;
					int createSeparatorIndex = Array.FindIndex<BXPopupMenuBaseItem>(menuItems, obj => string.CompareOrdinal(obj.ID, "MenuEditCreateSeparator") == 0);
					if (create && hasModify || !create && hasCreate)
					{
						if (createSeparatorIndex < 0)
						{
							createSeparator = new BXPopupMenuSeparator();
							createSeparator.ID = "MenuEditCreateSeparator";
							createSeparatorIndex = menuPopup.ItemCount;
							menuPopup.AddItem(createSeparator, createSeparatorIndex);
						}
						else
							createSeparator = (BXPopupMenuSeparator)menuItems[createSeparatorIndex];
					}

					if (!create)
					{
						string modifyTypeNameLoc;
						switch (typeName.ToLowerInvariant())
						{
							case "top":
								modifyTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.TopMenu");
								break;
							case "left":
								modifyTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.LeftMenu");
								break;
							case "bottom":
								modifyTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.BottomMenu");
								break;
							default:
								modifyTypeNameLoc = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.MenuOfType"), "\"" + BXPublicMenu.GetMenuTitle(BXSite.Current.Id, typeName) + "\"");
								break;
						}


						BXPopupMenuItem menuPopupEditPublic = new BXPopupMenuItem();
						menuPopupEditPublic.ID = menuItemPrefix + "_Modify";
						menuPopupEditPublic.Text = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.EditMenuItems"), modifyTypeNameLoc);
						menuPopupEditPublic.Title = menuPopupEditPublic.Text;
						menuPopupEditPublic.IconCssClass = "icon menu-edit";
						menuPopupEditPublic.ClientClickScript = OpenEditDialogJavascript(sectionMenu);

						int menuPopupEditPublicIndex = createSeparatorIndex >= 0 ? createSeparatorIndex : menuPopup.ItemCount;
						menuPopup.AddItem(menuPopupEditPublic, menuPopupEditPublicIndex);
						if (createSeparatorIndex >= 0)
							createSeparatorIndex++;

						hasModify = true;
					}
					else 
					{
						string createTypeNameLoc;
						switch (typeName.ToLowerInvariant())
						{
							case "top":
								createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateTopMenu");
								break;
							case "left":
								createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateLeftMenu");
								break;
							case "bottom":
								createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateBottomMenu");
								break;
							default:
								createTypeNameLoc = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.MenuOfType"), BXPublicMenu.GetMenuTitle(BXSite.Current.Id, typeName));
								break;
						}

						BXPopupMenuItem menuPopupCreatePublic = new BXPopupMenuItem();
						menuPopupCreatePublic.ID = menuItemPrefix + "_Create";
						menuPopupCreatePublic.IconCssClass = "icon menu-edit";
						menuPopupCreatePublic.Text = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.CreateMenuInThisSection"), createTypeNameLoc);
						menuPopupCreatePublic.ClientClickScript = OpenEditDialogJavascript(sectionMenu);
						menuPopup.AddItem(menuPopupCreatePublic, menuPopup.ItemCount);

						hasCreate = true;
					}
				}
				menu.PopupMenuItemIndexForDefaultAction = 0;
			}
			base.PopulatePublicPanelMenu(sectionList);
		}
		*/
		public override void PopulateHermitagePublicPanelMenu(Bitrix.UI.Hermitage.BXHermitagePublicPanelInfo info)
		{
			if(info == null)
				return;

			BXHermitagePublicPanelSectionInfo structure = info.EnsureStandardSection(BXHermitagePublicPanelInfo.StandardSections.Structure.Id);

			if (structure == null)
				return;

			EnsureMenu();

			string dir = null,
				file = null;

			if (!BXPath.BreakPath(Page.AppRelativeVirtualPath, ref dir, ref file))
			{
				base.PopulateHermitagePublicPanelMenu(info);
				return;
			}

			BXHermitagePublicPanelMenuInfo menu = structure.GetMenuById("Bitrix.PublicMenu");
			if(menu == null)
				return;

			bool hasCreate = menu.Popup.FindItem(x => x.Id.StartsWith("MenuEdit_", StringComparison.Ordinal) && x.Id.EndsWith("_Create", StringComparison.Ordinal)) != null,
				hasModify = menu.Popup.FindItem(x => x.Id.StartsWith("MenuEdit_", StringComparison.Ordinal) && x.Id.EndsWith("_Modify", StringComparison.Ordinal)) != null;

			bool canOperate =
				BXSite.Current != null
				? BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", BXSite.Current.TextEncoder.Decode(BXSite.Current.Id))
				: BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit);

			HashSet<string> processed = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			for (int i = 0; i < MenuTypes.Count; i++)
			{
				string typeName = MenuTypes[i];

				if (processed.Contains(typeName))
					continue;

				processed.Add(typeName);

				string prefix = string.Concat("MenuEdit_", typeName, "_");
				if (menu.Popup.FindItem(x => x.Id.LastIndexOf('_') >= 0 &&
					x.Id.Substring(0, x.Id.LastIndexOf('_')).Equals(prefix, StringComparison.OrdinalIgnoreCase)) != null)
					continue;

				string sectionMenu = BXPath.Combine(dir, string.Concat(typeName, ".menu"));
				if (!canOperate && !BXSecureIO.CheckWrite(sectionMenu))
					continue;

				bool create = !BXSecureIO.FileExists(sectionMenu);

				//int createSeparatorIndex = -1;
				//if ((create && hasModify) || (!create && hasCreate))
				//	createSeparatorIndex = menu.Popup.Items.IndexOf(menu.Popup.EnsureSeparator("MenuEditCreateSeparator"));

				if (!create)
				{
					string modifyTypeNameLoc;
					switch (typeName.ToLowerInvariant())
					{
						case "top":
							modifyTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.TopMenu");
							break;
						case "left":
							modifyTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.LeftMenu");
							break;
						case "bottom":
							modifyTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.BottomMenu");
							break;
						default:
							modifyTypeNameLoc = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.MenuOfType"), "\"" + BXPublicMenu.GetMenuTitle(BXSite.Current.Id, typeName) + "\"");
							break;
					}

					BXHermitagePopupMenuItem modifyItem = new BXHermitagePopupMenuItem();
					modifyItem.Id = string.Concat(prefix, "_Modify");
					modifyItem.Text = modifyItem.Title = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.EditMenuItems"), modifyTypeNameLoc);
					modifyItem.IconCssClass = "icon menu-edit";				
					modifyItem.ClientClickScript = OpenEditDialogJavascript(sectionMenu);

					int modifyItemIndex = /*createSeparatorIndex >= 0 ? createSeparatorIndex : */menu.Popup.ItemCount;
					menu.Popup.Items.Insert(/*createSeparatorIndex >= 0 ? createSeparatorIndex : */menu.Popup.ItemCount, modifyItem);
					//if (createSeparatorIndex >= 0)
					//	createSeparatorIndex++;

					modifyItem.IsDefault = modifyItemIndex == 0;

					hasModify = true;
				}
				else 
				{
					string createTypeNameLoc;
					switch (typeName.ToLowerInvariant())
					{
						case "top":
							createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateTopMenu");
							break;
						case "left":
							createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateLeftMenu");
							break;
						case "bottom":
							createTypeNameLoc = GetMessage("PopupMenuItem.Text.Part.CreateBottomMenu");
							break;
						default:
							createTypeNameLoc = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.MenuOfType"), BXPublicMenu.GetMenuTitle(BXSite.Current.Id, typeName));
							break;
					}

					BXHermitagePopupMenuItem createItem = menu.Popup.CreateItem();
					createItem.Id = string.Concat(prefix, "_Create");
					createItem.Text = createItem.Title = string.Format(GetMessage("PopupMenuItem.Text.FormatPart.CreateMenuInThisSection"), createTypeNameLoc);
					createItem.IconCssClass = "icon menu-edit";				
					createItem.ClientClickScript = OpenEditDialogJavascript(sectionMenu);

					hasCreate = true;
				}
			}
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessage("Title");
			Description = GetMessage("Description");
			Icon = "component.gif";

			Group = new BXComponentGroup("system_menu", GetMessage("Group"), 100, BXComponentGroup.Utility);

			ParamsDefinition["Depth"] = new BXParamText(GetMessageRaw("ComponentParam.Title.MenuDepth"), "1", BXCategory.Main);

			// compatibility issue
			var t1 = Parameters.GetString("MenuName", "top");
			var t2 = Parameters.GetString("SubMenuName", "left");
			ParamsDefinition["MenuTypes"] = new BXParamDoubleList(GetMessageRaw("ComponentParam.Title.MenuTypes"), string.Concat(t1, ";", t2), true, BXCategory.Main);

			ParamsDefinition["Url"] = new BXParamText(GetMessageRaw("ComponentParam.Title.Url"), "", BXCategory.AdditionalSettings);
		}

		protected override void LoadComponentDefinition()
		{
			var menuNames = (BXParamDoubleList)ParamsDefinition["MenuTypes"];
			menuNames.Values.Clear();
			if (!string.IsNullOrEmpty(DesignerSite))
			{
				foreach (KeyValuePair<string, string> p in BXPublicMenu.GetMenuTypes(DesignerSite))
					menuNames.Values.Add(new BXParamValue(string.Format("{0} ({1})", p.Value, p.Key.ToLowerInvariant()), p.Key.ToLowerInvariant()));
			}
		}
	}

	public class SystemPublicMenuTemplate : BXComponentTemplate<SystemPublicMenuComponent>
	{
		protected override void PrepareDesignMode()
		{
			MinimalWidth = "85";
			MinimalHeight = "45";
			StartWidth = "100%";
			StartHeight = "100";

			if (Results["menu"] == null || (Results["menu"] as BXPublicMenuItemCollection).Count == 0)
			{
				BXPublicMenuItemCollection menu = new BXPublicMenuItemCollection();

				for (int i = 0; i < 5; i++)
				{
					BXPublicMenuItem item = new BXPublicMenuItem();
					if (i == 0)
						item.Link = "~/bitrix";
					else
						item.Link = string.Format("~/bitrix{0}", i);

					item.Title = string.Format("Bitrix {0}", i + 1);
					menu.Add(item);
				}

				Results["menu"] = menu;
			}
		}
	}
}

#region Compatibility Issue
public partial class PublicMenuComponent : Bitrix.Main.Components.SystemPublicMenuComponent
{
	//NESTED CLASSES
	public class Template : BXComponentTemplate<PublicMenuComponent>
	{
		protected override void PrepareDesignMode()
		{
			MinimalWidth = "85";
			MinimalHeight = "45";
			StartWidth = "100%";
			StartHeight = "100";

			if (Results["menu"] == null || (Results["menu"] as BXPublicMenuItemCollection).Count == 0)
			{
				BXPublicMenuItemCollection menu = new BXPublicMenuItemCollection();

				for (int i = 0; i < 5; i++)
				{
					BXPublicMenuItem item = new BXPublicMenuItem();
					if (i == 0)
						item.Link = "~/bitrix";
					else
						item.Link = string.Format("~/bitrix{0}", i);

					item.Title = string.Format("Bitrix {0}", i + 1);
					menu.Add(item);
				}

				Results["menu"] = menu;
			}
		}
	}
}
#endregion