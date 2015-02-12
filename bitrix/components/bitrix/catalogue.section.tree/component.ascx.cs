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
using Bitrix.Components;
using System.Collections.Generic;
using Bitrix.IBlock;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Configuration;
using Bitrix.UI.Popup;
using Bitrix.Security;
using Bitrix.Components.Editor;
using System.Collections.Specialized;
using Bitrix.UI.Hermitage;
using Bitrix.Services.Js;
using Bitrix.UI.Components;
using Bitrix.IBlock.UI;

namespace Bitrix.IBlock.Components
{
	public partial class CatalogueSectionTreeComponent : BXComponent
	{
		#region Iblock properties

		public int IBlockId
		{
			get { return Parameters.Get<int>("IBlockId"); }
		}

		protected BXIBlock iblock;
		public BXIBlock IBlock
		{
			get { return iblock; }
			protected set { iblock = value; }
		}

		public string IBlockName
		{
			get { return ComponentCache.Get<string>("IBlockName", String.Empty); }
			set { ComponentCache["IBlockName"] = value; }
		}

		public string IBlockElementName
		{
			get { return ComponentCache.Get<string>("IBlockElementName", String.Empty); }
			set { ComponentCache["IBlockElementName"] = value; }
		}

		public string IBlockSectionName
		{
			get { return ComponentCache.Get<string>("IBlockSectionName", String.Empty); }
			set { ComponentCache["IBlockSectionName"] = value; }
		}

		public int IBlockTypeId
		{
			get { return ComponentCache.Get<int>("IBlockTypeId"); }
			set { ComponentCache["IBlockTypeId"] = value; }
		}

		#endregion

		public int DepthLevel
		{
			get { return Parameters.Get<int>("DepthLevel", 2); }
		}

		#region Section properties

		private BXIBlockSection currentSection;
		public BXIBlockSection Section
		{
			get { return currentSection; }
		}

		private int sectionId = 0;
		public int SectionId
		{
			get
			{
				if (sectionId > 0)
					return sectionId;
				else
					return Parameters.Get<int>("SectionId");
			}
			private set { sectionId = value; }
		}

		public string SectionCode
		{
			get { return Parameters.Get<string>("SectionCode", String.Empty); }
		}

		public string SectionName
		{
			get
			{
				if (ComponentCache.ContainsKey("SectionName"))
					return ComponentCache.Get<string>("SectionName");
				return null;
			}
			set { ComponentCache["SectionName"] = value; }
		}



		#endregion

		private List<SectionTreeItem> treeItems;
		public List<SectionTreeItem> TreeItems
		{
			get { return treeItems; }
		}

		public bool ShowCounters
		{
			get { return Parameters.Get<bool>("ShowCounters", false); }
		}

		public bool isErrorOccured = false;
		public string errorMessage = String.Empty;

		protected void Page_Init(object sender, EventArgs e)
		{
			int id;

			if (CanModifySections
				&& !string.IsNullOrEmpty(Request["sectionAction"])
				&& string.Equals(Request["sectionAction"], "delete", StringComparison.OrdinalIgnoreCase)
				&& !string.IsNullOrEmpty(Request["sectionId"])
				&& int.TryParse(Request["sectionId"], out id)
				&& id > 0
				&& BXCsrfToken.CheckToken(Request[BXCsrfToken.TokenKey]))
			{
				try
				{
					BXIBlockSection.Delete(id);
				}
				catch (Exception)
				{
				}

				Response.Redirect(GetSectionPostDeleteUrl(), true);
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsCached(BXPrincipal.Current.GetAllRoles(true)))
				return;

			using (BXTagCachingScope cacheScope = BeginTagCaching())
			{
				BXIBlockCollection iblockCollection = BXIBlock.GetList(
					new BXFilter(
						new BXFilterItem(BXIBlock.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlock.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlock.Fields.ID, BXSqlFilterOperators.Equal, IBlockId)
					),
					null,
					null,
					null,
					BXTextEncoder.HtmlTextEncoder
				);

				if (iblockCollection == null || iblockCollection.Count < 1)
				{
					AbortCache();
					errorMessage = GetMessage("WrongIBlockCode");
					isErrorOccured = true;
					IncludeComponentTemplate();
					return;
				}

				IBlockTypeId = iblockCollection[0].TypeId;
				IBlock = iblockCollection[0];
				IBlockName = iblockCollection[0].Name;
				IBlockElementName = iblockCollection[0].ElementName;
				IBlockSectionName = iblockCollection[0].SectionName;

				BXIBlockSectionCollection sectionCollection = new BXIBlockSectionCollection();
				if (SectionId < 1 && String.IsNullOrEmpty(SectionCode))
				{

					BXFilter filter = new BXFilter(
						new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
						new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y")
					);

					if (SectionCustomPropertyFilterSettings != null && SectionCustomPropertyFilterSettings.Count > 0)
					{
						BXCustomFieldCollection sectCustomFields = BXCustomEntityManager.GetFields(BXIBlockSection.GetCustomFieldsKey(IBlock.Id));
						foreach (KeyValuePair<string, object> kv in SectionCustomPropertyFilterSettings)
						{
							BXCustomField sectCustomField;
							if (!sectCustomFields.TryGetValue(kv.Key, out sectCustomField))
								continue;
							filter.Add(new BXFilterItem(BXIBlockSection.Fields.GetCustomField(IBlock.Id, kv.Key), sectCustomField.Multiple ? BXSqlFilterOperators.In : BXSqlFilterOperators.Equal, kv.Value));
						}
					}

					if (DepthLevel > 0)
						filter.Add(new BXFilterItem(BXIBlockSection.Fields.DepthLevel, BXSqlFilterOperators.LessOrEqual, DepthLevel));

					sectionCollection = BXIBlockSection.GetList(
						filter,
						new BXOrderBy(new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)),
						null,
						null,
						BXTextEncoder.HtmlTextEncoder
					);
				}
				else
				{
					if (!String.IsNullOrEmpty(SectionCode))
					{
						#region old
						//String[] sectionCodeChain = SectionCode.Trim('/').Split('/');
						//int? parentSectionID = null;
						//for (int i = 0; i < sectionCodeChain.Length; i++)
						//{
						//new BXFilterItem(BXIBlockSection.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
						//new BXFilterItem(BXIBlockSection.Fields.Code, BXSqlFilterOperators.Equal, sectionCodeChain[i]),
						//new BXFilterItem(BXIBlockSection.Fields.SectionId, BXSqlFilterOperators.Equal, parentSectionID)
						//parentSectionID = section[0].Id;
						//}
						#endregion
						BXFilter filter = new BXFilter(
							new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
							new BXFilterItem(BXIBlockSection.Fields.Code, BXSqlFilterOperators.Equal, SectionCode),
							new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId)
						);

						if (SectionId > 0)
							filter.Add(new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.Equal, SectionId));

						BXIBlockSectionCollection section = BXIBlockSection.GetList(filter, null, null, null, BXTextEncoder.HtmlTextEncoder);

						if (section != null && section.Count > 0)
						{
							currentSection = section[0];
							SectionId = section[0].Id;
						}
						else
						{
							currentSection = null;
							SectionId = 0;
						}
					}
					else if (SectionId > 0)
					{
						BXIBlockSectionCollection section = BXIBlockSection.GetList(
							new BXFilter(
								new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
								new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.Equal, SectionId),
								new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId)
							),
							null,
							null,
							null,
							BXTextEncoder.HtmlTextEncoder
						);

						currentSection = section != null && section.Count > 0 ? section[0] : null;
					}

					if (currentSection != null)
					{
						BXFilter filter = new BXFilter(
							new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId),
							new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y")
						);

						if (Parameters.Get<bool>("IncludeParentSections", true))
						{
							filter.Add(new BXFilterItem(BXIBlockSection.Fields.RightMargin, BXSqlFilterOperators.Greater, currentSection.LeftMargin));
							filter.Add(new BXFilterItem(BXIBlockSection.Fields.LeftMargin, BXSqlFilterOperators.Less, currentSection.RightMargin));
						}
						else
						{
							filter.Add(new BXFilterItem(BXIBlockSection.Fields.LeftMargin, BXSqlFilterOperators.Greater, currentSection.LeftMargin));
							filter.Add(new BXFilterItem(BXIBlockSection.Fields.RightMargin, BXSqlFilterOperators.Less, currentSection.RightMargin));
						}

						if (DepthLevel > 0)
							filter.Add(new BXFilterItem(BXIBlockSection.Fields.DepthLevel, BXSqlFilterOperators.LessOrEqual, currentSection.DepthLevel + DepthLevel));

						sectionCollection = BXIBlockSection.GetList(
							filter,
							new BXOrderBy(new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)),
							null,
							null,
							BXTextEncoder.HtmlTextEncoder
						);
					}
					else
					{
						AbortCache();
						isErrorOccured = true;
						SectionId = 0;
					}
				}

				int sectionDepthLevel = currentSection != null ? currentSection.DepthLevel : 0;
				int previousDepthLevel = 1;

				string sectionUrlTemplate = Parameters.Get<string>("SectionUrl", String.Empty);
				//bool sectionPathExists = sectionUrlTemplate.Contains("SectionCodePath") || sectionUrlTemplate.Contains("SECTION_CODE_PATH");
				string rootCodePath = String.Empty;
				//Dictionary<int, string> pathTree = new Dictionary<int, string>();
				//pathTree[0] = String.Empty;

				treeItems = new List<SectionTreeItem>();

				foreach (BXIBlockSection section in sectionCollection)
				{
					if (treeItems.Count > 0)
						treeItems[treeItems.Count - 1].HasChildren = section.DepthLevel > previousDepthLevel;
					previousDepthLevel = section.DepthLevel;

					BXParamsBag<object> replaceItems = new BXParamsBag<object>();
					replaceItems.Add("SectionId", section.Id);
					replaceItems.Add("SECTION_ID", section.Id);
					replaceItems.Add("SectionCode", section.Code);
					replaceItems.Add("SECTION_CODE", section.Code);
					replaceItems.Add("IblockId", section.IBlockId);
					replaceItems.Add("IBLOCK_ID", section.IBlockId);
					replaceItems.Add("IblockCode", IBlock.Code);
					replaceItems.Add("IBLOCK_CODE", IBlock.Code);

					//if (sectionPathExists)
					//{
					//    string codePath = section.Code;
					//    if (pathTree[section.SectionId] != null)
					//        codePath = pathTree[section.SectionId].Length > 0 ? pathTree[section.SectionId] + "/" + codePath : codePath;
					//    pathTree[section.Id] = codePath;

					//    replaceItems.Add("SectionCodePath", codePath);
					//    replaceItems.Add("SECTION_CODE_PATH", codePath);
					//}

					SectionTreeItem sectionItem = new SectionTreeItem(this);
					sectionItem.HasChildren = false;

					//sectionItem.SectionDetailUrl = MakeLink(sectionUrlTemplate, replaceItems);
					sectionItem.SectionDetailUrl = BXSefUrlUtility.MakeLink(sectionUrlTemplate, replaceItems);
					sectionItem.Section = section;

					treeItems.Add(sectionItem);
				}
				IncludeComponentTemplate();
			}
		}

		#region Admin panel buttons

		public string AdminPanelSectionId
		{
			get { return "CatalogueTreeMenuSection"; }
		}

		public string AdminPanelButtonMenuId
		{
			get { return String.Format("CatalogueTreeButtonMenu_{0}", IBlockId); }
		}

		//Создаем секцию и кнопку для выпадающего меню в панели администратора
		public override void CreatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
		{
			if (!Parameters.Get<bool>("AddAdminPanelButtons") || isErrorOccured)
				return;

			if (sectionList == null)
				throw new ArgumentNullException("sectionList");

			if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
				!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifySections))
				return;

			//Создаем секцию
			BXPublicPanelMenuSection iblockListSection = sectionList.FindByID(AdminPanelSectionId);
			if (iblockListSection == null)
			{
				BXPublicPanelMenuSectionInfo sectionInfo = new BXPublicPanelMenuSectionInfo(AdminPanelSectionId, 100);
				iblockListSection = new BXPublicPanelMenuSection(sectionInfo);
				sectionList.Add(iblockListSection);
			}

			//Создаем кнопку
			BXPublicPanelMenu listButtonMenu = iblockListSection.FindByID(AdminPanelButtonMenuId);
			if (listButtonMenu == null)
			{
				listButtonMenu = new BXPublicPanelMenu();
				listButtonMenu.ID = AdminPanelButtonMenuId;
				listButtonMenu.BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/iblock.gif");
				listButtonMenu.Caption = BXTextEncoder.HtmlTextEncoder.Decode(this.Title);
				listButtonMenu.Hint = BXTextEncoder.HtmlTextEncoder.Decode(this.Title);
				listButtonMenu.ShowMode = BXShowMode.NonView;
				iblockListSection.Add(listButtonMenu);
			}
		}

		//Заполняем выпадающее меню пунктами
		public override void PopulatePublicPanelMenu(BXPublicPanelMenuSectionList sectionList)
		{
			if (!Parameters.Get<bool>("AddAdminPanelButtons") || isErrorOccured)
				return;

			if (sectionList == null)
				throw new ArgumentNullException("sectionList");

			if (!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) ||
				!BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifySections))
				return;

			BXPublicPanelMenuSection iblockListSection = sectionList.FindByID(AdminPanelSectionId);
			if (iblockListSection == null)
				return;

			BXPublicPanelMenu listButtonMenu = iblockListSection.FindByID(AdminPanelButtonMenuId);
			if (listButtonMenu == null)
				return;

			if (listButtonMenu.PopupMenu == null)
				listButtonMenu.PopupMenu = new BXPopupMenuV2();

			BXPopupMenuItem[] menuItems = GetMenuItems();
			for (int menuIndex = 0; menuIndex < menuItems.Length; menuIndex++)
			{
				bool itemExists = false;
				foreach (BXPopupMenuBaseItem item in listButtonMenu.PopupMenu.Items)
				{
					if (item.ID == menuItems[menuIndex].ID)
					{
						itemExists = true;
						break;
					}
				}

				if (!itemExists)
					listButtonMenu.PopupMenu.AddItem(menuItems[menuIndex]);
			}
		}

		public BXPopupMenuItem[] GetMenuItems()
		{
			BXPopupMenuItem[] menuItems = new BXPopupMenuItem[SectionId > 0 ? 2 : 1];

			menuItems[0] = new BXPopupMenuItem();
			menuItems[0].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/new_section.gif");
			menuItems[0].ID = String.Format("catalogue_tree_section_new_{0}", IBlockId);
			menuItems[0].Text = GetMessageRaw("AddNewSection");
			menuItems[0].ClientClickScript =
				String.Format(
					"jsUtils.Redirect(arguments, '{0}?type_id={2}&iblock_id={1}&section_id={3}&{4}={5}')",
					VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"),
					IBlockId,
					IBlockTypeId,
					SectionId,
					BXConfigurationUtility.Constants.BackUrl,
					UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery)
				);

			if (SectionId > 0)
			{
				menuItems[1] = new BXPopupMenuItem();
				menuItems[1].BackgroundImagePath = BXPath.Combine(BXComponentManager.GetFolder(Name), "/images/edit_section.gif");
				menuItems[1].ID = String.Format("catalogue_section_edit_{0}", IBlockId);
				menuItems[1].Text = GetMessageRaw("EditCurrentSection");
				menuItems[1].ClientClickScript =
					String.Format(
						"jsUtils.Redirect(arguments, '{0}?type_id={2}&iblock_id={1}&section_id={3}&id={4}&{5}={6}')",
						VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"),
						IBlockId,
						IBlockTypeId,
						SectionId,
						SectionId,
						BXConfigurationUtility.Constants.BackUrl,
						UrlEncode(BXSefUrlManager.CurrentUrl.PathAndQuery)
					);
			}

			return menuItems;
		}

		//Создаем контекстное меню
		public override BXComponentPopupMenuInfo PopupMenuInfo
		{
			get
			{
				if(this.popupMenuInfo != null)
					return this.popupMenuInfo;

				BXComponentPopupMenuInfo info = base.PopupMenuInfo;

				if (!isErrorOccured && BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) &&
					BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifySections))
					info.CreateComponentContentMenuItems =
						delegate(BXShowMode showMode)
						{
							if (showMode == BXShowMode.View || isErrorOccured)
								return new BXHermitagePopupMenuBaseItem[0];

							BXHermitageToolbarSettings settings = new BXHermitageToolbarSettings();
							settings.Data.Add("IBlockId", IBlockId);
							settings.Data.Add("IBlockTypeId", IBlockTypeId);
							settings.Data.Add("IBlock", IBlock);
							settings.Data.Add("SectionId", SectionId);
							settings.Data.Add("SectionPostCreateUrl", GetSectionPostCreateUrl());
							settings.Data.Add("SectionPostDeleteUrl", GetSectionPostDeleteUrl());

							string sectionName = (IBlock != null)
								? BXHtmlTextEncoder.HtmlTextEncoder.Decode(IBlock.CaptionsInfo.SectionName.ToLower())
								: string.IsNullOrEmpty(IBlockSectionName) ? GetMessageRaw("IBlockSectionName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockSectionName.ToLower());

							settings.Data.Add("AddNewSection",
								string.Format(GetMessageRaw("AddSectionMenuItem"), sectionName));

							settings.Data.Add("ChangeSection",
								string.Format(GetMessageRaw("EditSectionMenuItem"), sectionName));

							settings.Data.Add("DeleteSection",
								string.Format(GetMessageRaw("DeleteSectionMenuItem"), sectionName));

							settings.Data.Add("SectionDeletionConfirmation",
								string.Format(GetMessageRaw("SectionDeletionConfirmation"), sectionName));

							return BXHermitageToolbarHelper.CreateToolbar(SectionId > 0 ? (BXHermitageMenuCommand.CreateSection | BXHermitageMenuCommand.ModifySection | BXHermitageMenuCommand.DeleteSection) : BXHermitageMenuCommand.CreateSection, settings).ItemsToArray();
							#region old
							/*
							BXHermitagePopupMenuBaseItem[] menuItems = new BXHermitagePopupMenuBaseItem[SectionId > 0 ? 3 : 1];

							BXHermitagePopupMenuItem createSection = new BXHermitagePopupMenuItem();
							menuItems[0] = createSection;
							createSection.Id = string.Concat("CATALOGUE_SECTION_CREATE_", IBlockId);
							createSection.IconCssClass = "bx-context-toolbar-create-icon";
							createSection.Text = GetMessageRaw("AddNewSection");
							createSection.Sort = 40;
							createSection.ClientClickScript = string.Format(
									"(new BX.CDialogNet({{ 'content_url':'{0}?iblock_id={1}&type_id={2}&section_id={3}&{4}={5}&clientType=WindowManager&mode=dlg', 'adminStyle':true, 'width':'800', 'height':'600', 'resizable':true }})).Show();",
									VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"),
									IBlockId,
									IBlockTypeId,
									SectionId,
									BXConfigurationUtility.Constants.BackUrl,
									UrlEncode(GetSectionPostCreateUrl()));

							if (SectionId > 0)
							{
								BXHermitagePopupMenuItem editSection = new BXHermitagePopupMenuItem();
								menuItems[1] = editSection;
								editSection.Id = string.Concat("CATALOGUE_SECTION_EDIT_", IBlockId, "_", SectionId);
								editSection.IconCssClass = "bx-context-toolbar-edit-icon";
								editSection.Text = GetMessageRaw("EditCurrentSection");
								editSection.Sort = 30;
								editSection.ClientClickScript = string.Format(
										"(new BX.CDialogNet({{ 'content_url':'{0}?iblock_id={1}&type_id={2}&section_id={3}&id={3}&clientType=WindowManager&mode=dlg', 'adminStyle':true, 'width':'800', 'height':'600', 'resizable':true }})).Show();",
										VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"),
										IBlockId, IBlockTypeId, SectionId);

								BXHermitagePopupMenuItem deleteSection = new BXHermitagePopupMenuItem();
								menuItems[2] = deleteSection;
								deleteSection.Id = string.Concat("CATALOGUE_SECTION_DELETE_", IBlockId, "_", SectionId);
								deleteSection.IconCssClass = "bx-context-toolbar-delete-icon";
								deleteSection.Text = GetMessageRaw("DeleteCurrentSection");
								deleteSection.Sort = 50;
								deleteSection.ClientClickScript = string.Format(
									"if(window.confirm('{0}')){{ BX.ajax({{ 'method':'POST', 'dataType':'JSON', 'url':'{1}', 'data':{{ 'event':'Bitrix.Bitrix.IBlocklSectionDelete:{2}', '{3}':'{4}' }}, 'onsuccess':function(){{ window.location.assign('{5}') }} }}); }};",
									BXJSUtility.Encode(GetMessageRaw("SectionDeletionConfirmation")),
									BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/handlers/Main/HermitageEventHandler.ashx")),
									SectionId.ToString(),
									BXJSUtility.Encode(BXCsrfToken.TokenKey), 
									BXJSUtility.Encode(BXCsrfToken.GenerateToken()),
									BXJSUtility.Encode(GetSectionPostDeleteUrl()));
							}

							return menuItems;
							*/
							#endregion
						};

				return info;
			}
		}
		#endregion
		BXIBlock paramIblock;
		BXIBlock ParamIBlock
		{
			get
			{
				int i = 0;
				if (!Parameters.ContainsKey("IBlockId")) return null;
				if (!int.TryParse(Parameters["IBlockId"], out i)) return null;
				return paramIblock = BXIBlock.GetById(i);
			}
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/section_tree.gif";
			Group = new BXComponentGroup("catalogue", GetMessageRaw("Group"), 10, BXComponentGroup.Content);

			BXCategory mainCategory = BXCategory.Main;
			ParamsDefinition.Add(
				"IBlockTypeId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockType"),
					String.Empty,
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"IBlockId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockCode"),
					String.Empty,
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"SectionId",
				new BXParamText(
					GetMessageRaw("SectionId"),
					String.Empty,
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"SectionCode",
				new BXParamText(
					GetMessageRaw("SectionMnemonicCodeID"),
					String.Empty,
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"SectionUrl",
				new BXParamText(
					GetMessageRaw("PageAddress"),
					"?section_id=#SECTION_ID#",
					BXCategory.UrlSettings
				)
			);

			BXCategory additionalCategory = BXCategory.AdditionalSettings;
			ParamsDefinition.Add(
				"DepthLevel",
				new BXParamText(
					GetMessageRaw("SubsectionsDisplayDepth"),
					"2",
					additionalCategory
				)
			);

			ParamsDefinition.Add(
				"ShowCounters",
				new BXParamYesNo(
					GetMessageRaw("DisplayQuantityOfElementsInSection"),
					false,
					additionalCategory
				)
			);

			ParamsDefinition.Add(
				"CountSubElements",
				new BXParamYesNo(
					GetMessageRaw("CountSubElements"),
					true,
					additionalCategory
				)
			);

			ParamsDefinition.Add(
				"IncludeParentSections",
				new BXParamYesNo(
					GetMessageRaw("IncludeParentSections"),
					true,
					additionalCategory
				)
			);

			ParamsDefinition.Add(
				"AddAdminPanelButtons",
				new BXParamYesNo(
					GetMessageRaw("AddButtonsForThisComponentToAdminPanel"),
					false,
					additionalCategory
				)
			);

			BXCategory customFieldCategory = BXCategory.CustomField;

			ParamsDefinition.Add("FilterBySectionCustomProperty",
				new BXParamYesNo(
					GetMessageRaw("FilterBySectionCustomProperty"),
					false,
					customFieldCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, "FilterBySectionSectionProperty", "FilterBySectionCustomProperty", string.Empty)
					)
				);

			ParamsDefinition.Add("SectionCustomPropertyFilterSettings",
				new BXParamCustomFieldFilter(
					GetMessageRaw("SectionCustomPropertyFilterSettings"),
					string.Empty,
					customFieldCategory,
					string.Empty,//BXIBlockModuleConfiguration,
					new ParamClientSideActionGroupViewMember(ClientID, "SectionCustomPropertyFilterSettings", new string[] { "FilterBySectionCustomProperty" })
					)
				);

			ParamsDefinition.Add(BXParametersDefinition.Cache);
		}

		public BXParamsBag<object> SectionCustomPropertyFilterSettings
		{
			get
			{
				return BXParamsBag<object>.FromString(Parameters.GetString("SectionCustomPropertyFilterSettings", string.Empty));
			}
			set
			{
				Parameters["SectionCustomPropertyFilterSettings"] = value.ToString();
			}
		}

		protected override void LoadComponentDefinition()
		{
			//Iblock type
			List<BXParamValue> typeParamValue = new List<BXParamValue>();
			typeParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockType"), ""));

			BXIBlockTypeCollection iblockTypes = BXIBlockType.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
			foreach (BXIBlockType iblockType in iblockTypes)
				typeParamValue.Add(new BXParamValue(iblockType.Translations[BXLoc.CurrentLocale].Name, iblockType.Id.ToString()));

			ParamsDefinition["IBlockTypeId"].Values = typeParamValue;
			ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;

			//Iblock
			int selectedIBlockType = 0;
			if (Parameters.ContainsKey("IBlockTypeId"))
				int.TryParse(Parameters["IBlockTypeId"], out selectedIBlockType);

			BXFilter filter = new BXFilter();
			if (selectedIBlockType > 0)
				filter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, selectedIBlockType));
			if (!String.IsNullOrEmpty(DesignerSite))
				filter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

			List<BXParamValue> iblockParamValue = new List<BXParamValue>();
			iblockParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockID"), "-"));
			BXIBlockCollection iblocks = BXIBlock.GetList(filter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
			foreach (BXIBlock iblock in iblocks)
				iblockParamValue.Add(new BXParamValue(iblock.Name, iblock.Id.ToString()));

			ParamsDefinition["IBlockId"].Values = iblockParamValue;
			ParamsDefinition["IBlockId"].RefreshOnDirty = true;


			if (ParamIBlock != null)
			{
				BXParamCustomFieldFilter cfParamFilter = ParamsDefinition["SectionCustomPropertyFilterSettings"] as BXParamCustomFieldFilter;
				cfParamFilter.EntityId = BXIBlockSection.GetCustomFieldsKey(ParamIBlock.Id);
			}
		}

		private bool? canModifySections = null;
		public bool CanModifySections
		{
			get
			{
				return (canModifySections ?? (canModifySections = BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead)
					&& BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifySections))).Value;
			}
		}

		public string GetSectionAddUrl()
		{
			if (!CanModifySections)
				return string.Empty;

			return string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"), "?type_id=", IBlockTypeId.ToString(), "&iblock_id=", IBlockId.ToString(), "&section_id=", SectionId.ToString(), "&", BXConfigurationUtility.Constants.BackUrl, "=", UrlEncode(Request.RawUrl));
		}

		public string GetSectionEditUrl(int sectionId)
		{
			if (!CanModifySections)
				return string.Empty;

			return string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockSectionEdit.aspx"), "?type_id=", IBlockTypeId.ToString(), "&iblock_id=", IBlockId.ToString(), "&section_id=", sectionId.ToString(), "&id=", sectionId.ToString(), "&", BXConfigurationUtility.Constants.BackUrl, "=", UrlEncode(Request.RawUrl));
		}

		public string GetSectionDeleteUrl(int sectionId)
		{
			if (!CanModifySections)
				return string.Empty;

			Uri url = BXSefUrlManager.CurrentUrl;
			NameValueCollection qsParams = HttpUtility.ParseQueryString(url.Query);
			qsParams["sectionAction"] = "delete";
			qsParams["sectionId"] = sectionId.ToString();
			qsParams[BXCsrfToken.TokenKey] = BXCsrfToken.GenerateToken();
			return string.Concat(url.AbsolutePath, "?", qsParams.ToString());
		}

		public string GetSectionPostDeleteUrl()
		{
			string sectionUrl = Parameters.GetString("SectionUrl");
			if (!string.IsNullOrEmpty(sectionUrl) && this.currentSection != null && this.currentSection.SectionId > 0)
			{
				string parentSectionId = this.currentSection.SectionId.ToString();
				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace.Add("IBLOCK_ID", IBlockId);
				replace.Add("IBlockId", IBlockId);
				replace.Add("SectionId", parentSectionId);
				replace.Add("SECTION_ID", parentSectionId);
				replace.Add("SectionCode", parentSectionId);
				replace.Add("SECTION_CODE", parentSectionId);
				return ResolveTemplateUrl(sectionUrl, replace);
			}
			return ResolveTemplateUrl(Parameters.Get("Template_RootUrl", string.Empty), null);
		}

		public string GetSectionPostCreateUrl()
		{
			string sectionUrl = Parameters.GetString("SectionUrl");
			if (!string.IsNullOrEmpty(sectionUrl))
			{
				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace.Add("IBLOCK_ID", IBlockId);
				replace.Add("IBlockId", IBlockId);
				replace.Add("SectionId", null);
				replace.Add("SECTION_ID", null);
				replace.Add("SectionCode", null);
				replace.Add("SECTION_CODE", null);
				return ResolveTemplateUrl(sectionUrl, replace);
			}
			return ResolveTemplateUrl(Parameters.Get("Template_RootUrl", string.Empty), null);
		}

		public BXHermitageToolbar CreateSectionContextToolbar(BXIBlockSection section, string containerClientID)
		{
			if (section == null)
				throw new ArgumentNullException("section");

			if (!(BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockAdminRead) &&
				BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifySections) && !isErrorOccured))
				return new BXHermitageToolbar();

			BXHermitageToolbarSettings settings = new BXHermitageToolbarSettings();
			settings.RequireIncludeAreasFlag = true;
			settings.ParentClientID = containerClientID;
			settings.Data.Add("IBlockId", IBlockId);
			settings.Data.Add("IBlockTypeId", IBlockTypeId);
			settings.Data.Add("IBlock", IBlock);
			settings.Data.Add("SectionId", section.Id);
			settings.Data.Add("SectionPostCreateUrl", GetSectionPostCreateUrl());
			settings.Data.Add("SectionPostDeleteUrl", GetSectionPostDeleteUrl());

			string sectionName = (IBlock != null)
				? BXHtmlTextEncoder.HtmlTextEncoder.Decode(IBlock.CaptionsInfo.SectionName.ToLower())
				: string.IsNullOrEmpty(IBlockSectionName) ? GetMessageRaw("IBlockSectionName") : BXTextEncoder.HtmlTextEncoder.Decode(IBlockSectionName.ToLower());

			settings.Data.Add("ChangeSection",
				string.Format(GetMessageRaw("EditSectionMenuItem"), sectionName));

			settings.Data.Add("DeleteSection",
				string.Format(GetMessageRaw("DeleteSectionMenuItem"), sectionName));

			settings.Data.Add("SectionDeletionConfirmation",
				string.Format(GetMessageRaw("SectionDeletionConfirmation"), sectionName));

			return BXHermitageToolbarHelper.CreateToolbar(BXHermitageMenuCommand.ModifySection | BXHermitageMenuCommand.DeleteSection, settings);
		}

		public class SectionTreeItem
		{
			private CatalogueSectionTreeComponent parent;
			private BXIBlockSection section;
			private string sectionDetailUrl;
			private bool hasChildren;

			public SectionTreeItem(CatalogueSectionTreeComponent parent)
			{
				this.parent = parent;
			}

			public BXIBlockSection Section
			{
				get { return section; }
				set { section = value; }
			}

			public string SectionDetailUrl
			{
				get { return sectionDetailUrl; }
				set { sectionDetailUrl = value; }
			}

			private int elementsCount = -1;
			public int ElementsCount
			{
				get
				{
					if (elementsCount < 0)
					{
						BXFilter elementFilter = new BXFilter();
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, section.Id));
						elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"));

						string s;
						if (!parent.Parameters.TryGetValue("ActiveFilter", out s) || string.Equals(s, "Active", StringComparison.OrdinalIgnoreCase))
							elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"));
						else if (string.Equals(s, "NotActive", StringComparison.OrdinalIgnoreCase))
							elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "N"));


						if (!parent.Parameters.TryGetValue("ActiveDateFilter", out s) || string.Equals(s, "Active", StringComparison.OrdinalIgnoreCase))
							elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, "Y"));
						else if (string.Equals(s, "NotActive", StringComparison.OrdinalIgnoreCase))
							elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ActiveDate, BXSqlFilterOperators.Equal, "N"));

						if (parent.Parameters.Get<bool>("CountSubElements", true))
							elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.IncludeParentSections, BXSqlFilterOperators.Equal, "Y"));

						elementsCount = BXIBlockElement.Count(elementFilter);
					}

					return elementsCount;
				}
			}

			public bool HasChildren
			{
				get { return hasChildren; }
				set { hasChildren = value; }
			}
		}
	}

	public partial class CatalogueSectionTreeTemplate : BXComponentTemplate<CatalogueSectionTreeComponent>
	{
		protected override void Render(HtmlTextWriter writer)
		{
			if (IsComponentDesignMode && Component.TreeItems == null)
				writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
			else
				base.Render(writer);
		}

		protected string GetItemContainerClientID(int itemId)
		{
			return string.Concat(ClientID, ClientIDSeparator, "Item", itemId.ToString());
		}

		public void RenderSectionToolbar(BXIBlockSection section, string containerClientID)
		{
			Component.CreateSectionContextToolbar(section, containerClientID).Render(CurrentWriter);
		}
	}
}
