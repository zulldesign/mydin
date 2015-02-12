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
using Bitrix;
using System.Text;
using Bitrix.IO;
using Bitrix.UI;
using System.IO;
using System.Text.RegularExpressions;
using Bitrix.Components;
using Bitrix.Security;
using System.Collections.Generic;
using Bitrix.DataTypes;
using Bitrix.Services.Js;
using Bitrix.Services.Undo;

public partial class bitrix_dialogs_NewPageWizard : Bitrix.UI.BXDialogWizardPage
{

	private string _clientVirtualPath = string.Empty;
	private string _clientVirtualDirPath = string.Empty;
	private string _clientFileName = string.Empty;
	private string _clientTitle = string.Empty;
	private BXParamsBag<string> _keywordsTypes = null;
	private Dictionary<string, Dictionary<string, string>> _keywords = null;
	private Dictionary<string, string> _menuTypeDic = null;
	private int _maxMenuItemIndex = -1;
	protected bool _menuItemsVisible = true;
	protected bool _createMenuVisible = true;
	protected bool _createMenuChecked;

	protected override void OnInit(EventArgs e)
	{
		try
		{
			_clientTitle = GetMessage("DefaultTitle");

			_clientVirtualPath = !string.IsNullOrEmpty(Request.QueryString["path"]) ? Request.QueryString["path"] : "~";
			if (!VirtualPathUtility.IsAppRelative(_clientVirtualPath))
			{
				Close(string.Format("{0}: '{1}'", GetMessage("InvalidPath"), _clientVirtualPath), BXDialogGoodbyeWindow.LayoutType.Error, -1);
				return;
			}
			_clientVirtualPath = VirtualPathUtility.RemoveTrailingSlash(_clientVirtualPath);

			string clientPhysicalPath = Request.MapPath(_clientVirtualPath);
			DirectoryInfo di = new DirectoryInfo(clientPhysicalPath);
			if (di.Exists)
			{
				_clientVirtualDirPath = _clientVirtualPath;
				if (!IsPostBack)
					_clientFileName = CreateDefaultFileName(clientPhysicalPath, ref _clientTitle);
			}
			else
			{
				FileInfo fi = new FileInfo(clientPhysicalPath);

				if (fi.Exists)
				{
					if (!IsPostBack)
						_clientFileName = CreateDefaultFileName(fi.DirectoryName, ref _clientTitle);
				}
				else
				{
					if (!Directory.Exists(fi.DirectoryName))
					{
						Close(string.Format("{0}: '{1}'", GetMessage("DirectoryIsNotFound"), fi.DirectoryName), BXDialogGoodbyeWindow.LayoutType.Error, -1);
						return;
					}

					if (!IsPostBack)
					{
						if (!string.IsNullOrEmpty(fi.Extension) && string.Compare(".aspx", fi.Extension, StringComparison.OrdinalIgnoreCase) == 0)
							_clientFileName = fi.Name;
						else
							_clientFileName = CreateDefaultFileName(fi.DirectoryName, ref _clientTitle);
					}
				}
				_clientVirtualDirPath = fi.DirectoryName.Replace(HttpRuntime.AppDomainAppPath, "/").Replace(Path.DirectorySeparatorChar, '/');
			}

			if (!IsPostBack)
			{
				if (string.IsNullOrEmpty(_clientFileName))
					throw new InvalidOperationException("File is not defined!");

				if (_clientFileName.Length < 5)
					throw new InvalidOperationException(string.Format("Invalid file name has been detected: '{0}'!", _clientFileName));

				tbxPageName.Text = _clientFileName.Remove(_clientFileName.Length - 5);
				tbxTitle.Text = _clientTitle;
			}

			//MenuTypes && MenuItems
			StringBuilder jsMenuItems = new StringBuilder("var bxMenuType = {");
			_menuTypeDic = BXPublicMenu.GetMenuTypes(CurrentBxSite.Id);

			ListItemCollection menuTypeItemColl = ddlMenuTypes.Items;

			bool isJsMenuItemsArrayAdded = false;
			foreach (string menuTypeKey in _menuTypeDic.Keys)
			{
				var info = GetMenuTypeInfo(CurrentBxSite.TextEncoder.Decode(CurrentBxSite.Id), menuTypeKey, _clientVirtualPath, _clientVirtualDirPath);

				if (info == null)
					continue;

				ListItem li = new ListItem(_menuTypeDic[menuTypeKey], menuTypeKey);
				menuTypeItemColl.Add(li);

				int menuItemCount = info.SourcePath != null && info.Items != null ? info.Items.Count : 0;

				if (_maxMenuItemIndex < menuItemCount - 1)
					_maxMenuItemIndex = menuItemCount - 1;

				if (isJsMenuItemsArrayAdded)
					jsMenuItems.Append(", ");
				else
					isJsMenuItemsArrayAdded = true;

				jsMenuItems.AppendFormat(@"""{0}"":{{", BXJSUtility.Encode(menuTypeKey));


				if (menuItemCount > 0)
				{
					jsMenuItems.Append("ITEMS:[");
					for (int j = 0; j < menuItemCount; j++)
					{
						BXPublicMenuItem menuItem = info.Items[j];
						if (j > 0)
							jsMenuItems.Append(", ");
						jsMenuItems.AppendFormat(@"""{0}""", BXJSUtility.Encode(menuItem.Title));
					}
					jsMenuItems.Append("],");
				}

				jsMenuItems.AppendFormat("SECTION:{0}}}", info.SectionPath != null ? "true" : "false");
			}

			if (menuTypeItemColl.Count == 0)
			{
				chbxAddToMenu.Checked = false;
				chbxAddToMenu.Enabled = false;
				chbxAddToMenu.ToolTip = string.Format(GetMessageRaw("CheckBoxToolTip.FormatThereAreNoMenuTypesForThisSite"), CurrentBxSite.Name);
			}


			_createMenuChecked = Request.Form[string.Concat(UniqueID, IdSeparator, "cbCreateInSection")] == "Y";

			jsMenuItems.Append("};");
			ClientScript.RegisterClientScriptBlock(GetType(), "bxMenuType", jsMenuItems.ToString(), true);
            ddlMenuTypes.Attributes.Add("onchange", "Bitrix.NewPageWizard.onChangeMenuType(this.options[this.selectedIndex].value, true)");
			//-Try to detect current user menu
			List<string> menuTypeIds = new List<string>();
			foreach (string menuTypeKey in _menuTypeDic.Keys)
				menuTypeIds.Add(menuTypeKey);

			List<string> userMenuTypes = new List<string>();
			string[] userMenuFiles = null;
			string userMenuDir = VirtualPathUtility.AppendTrailingSlash(_clientVirtualPath);
			while (!string.Equals(userMenuDir, "/", StringComparison.Ordinal) && (userMenuFiles = Directory.GetFiles(Request.MapPath(userMenuDir), "*.menu")).Length == 0)
				userMenuDir = VirtualPathUtility.GetDirectory(VirtualPathUtility.RemoveTrailingSlash(userMenuDir));
			foreach(string menuFilePath in userMenuFiles)
			{
				string menuId = Path.GetFileName(menuFilePath);
				menuId = menuId.Substring(0, menuId.Length - 5);
				int ind = menuTypeIds.FindIndex(a => string.Equals(a, menuId, StringComparison.OrdinalIgnoreCase));
				if(ind < 0)
					continue;

				userMenuTypes.Add(menuTypeIds[ind]);
			}

			for(int i = 0; i < menuTypeIds.Count; i++)
				if(userMenuTypes.FindIndex(x => string.Equals(x, menuTypeIds[i], StringComparison.Ordinal)) >= 0)
				{
					ddlMenuTypes.SelectedIndex = i;
					break;
				}
			PrepareMenuItemsList();
			ddlMenuTypes.SelectedIndexChanged += new EventHandler(ddlMenuTypes_SelectedIndexChanged);
			//-
            chbxAddToMenu.Attributes.Add("onclick", "Bitrix.NewPageWizard.onChangeAddToMenu(this.checked)");

			_keywords = new Dictionary<string, Dictionary<string, string>>();
			_keywordsTypes = BXPageManager.GetKeywords(CurrentBxSite.Id);

			BXSectionInfo defInfo = BXSectionInfo.GetCumulativeSection(_clientVirtualDirPath);
			IDictionary<string, string> defInfoKeywords = defInfo.Keywords;

			foreach (string key in _keywordsTypes.Keys)
			{
				Dictionary<string, string> paramDic = new Dictionary<string, string>();
				paramDic.Add("caption", _keywordsTypes[key]);

				string keyValue = string.Empty;
				if (defInfoKeywords.ContainsKey(key))
					keyValue = defInfoKeywords[key];

				paramDic.Add("defaultValue", keyValue);
				paramDic.Add("value", keyValue);
				_keywords.Add(key, paramDic);
			}

			LoadKeywords();

			vlrPageName.ValidationExpression = BXPath.NameValidationRegexString;
			vlrPageName.Text = GetMessage("IllegalCharactersIsDecetedInFileName");
			vlrPageNameRequired.Text = GetMessage("EmptyFileName");
		}
		catch (System.Threading.ThreadAbortException /*ex*/)
		{
			//...игнорируем, вызвано Close();
		}
		catch (Exception ex)
		{
			Close(ex.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
		}

		base.OnInit(e);
	}

	private class MenuTypeInfo
	{
		public BXPublicMenuItemCollection Items;
		public string SourcePath;
		public string SectionPath;
	}
	private static MenuTypeInfo GetMenuTypeInfo(string siteId, string menuType, string virtualPath, string virtualPathDir)
	{
		BXPublicMenuItemCollection menuItemCol = BXPublicMenu.Menu.GetMenuByUri(menuType, virtualPath);
		//если коллекция есть, но нет пути, значит она не загружалась из файла и мы не можем её модифицировать
		if (menuItemCol != null && string.IsNullOrEmpty(menuItemCol.MenuFilePath))
			return null;

		string sourcePath = menuItemCol != null ? menuItemCol.MenuFilePath : null;
		bool addToSource = IsMenuWritable(siteId, sourcePath);

		string sectionPath = null;
		bool addToSection = false;
		var vDirWSlash = VirtualPathUtility.AppendTrailingSlash(virtualPathDir);
		if (menuItemCol == null || !string.Equals(VirtualPathUtility.GetDirectory(menuItemCol.MenuFilePath), vDirWSlash, StringComparison.InvariantCultureIgnoreCase))
		{
			sectionPath = VirtualPathUtility.Combine(vDirWSlash, string.Concat(menuType, ".menu"));
			addToSection = !BXSecureIO.FileExists(sectionPath) && IsMenuWritable(siteId, sectionPath);
		}

		if (!addToSource && !addToSection)
			return null;

		return new MenuTypeInfo
		{
			Items = menuItemCol,
			SourcePath = addToSource ? sourcePath : null,
			SectionPath = addToSection ? sectionPath : null
		};
	}
	private static bool IsMenuWritable(string menuPath, string siteId)
	{
		if (string.IsNullOrEmpty(menuPath))
			throw new ArgumentException("Is not defined!", "menuTypeId");
		var user = BXPrincipal.Current;
		if (!user.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit)
			&& !user.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", siteId)
			&& !BXSecureIO.CheckWrite(menuPath))
			return false;

		string filePhysicalPath = BXPath.ToPhysicalPath(menuPath);
		if (!Directory.Exists(Path.GetDirectoryName(filePhysicalPath)))
			throw new ArgumentException(string.Format("Could not find folder : {0}!", BXPath.GetDirectory(menuPath)));

		if (File.Exists(filePhysicalPath))
		{
			FileStream fstrm = null;
			try
			{
#if DEBUG
				FileAttributes attrs = BXSecureIO.FileClearAttributes(filePhysicalPath);
#endif
				fstrm = File.Open(filePhysicalPath, FileMode.Open, FileAccess.Write);
#if DEBUG
				BXSecureIO.FileSetAttributes(filePhysicalPath, attrs);
#endif
			}
			catch (UnauthorizedAccessException /*e*/)
			{
				return false;
			}
			finally
			{
				if (fstrm != null)
					fstrm.Close();
			}
		}
		return true;
	}


	void ddlMenuTypes_SelectedIndexChanged(object sender, EventArgs e)
	{
		PrepareMenuItemsList();
	}


	private void PrepareMenuItemsList()
	{
		ListItemCollection menuItems = ddlMenuItems.Items;
		menuItems.Clear();

		int menuTypeIndex = ddlMenuTypes.SelectedIndex;

		if (menuTypeIndex < 0)
			return;

		int menuTypeCount = _menuTypeDic != null ? _menuTypeDic.Count : 0;
		if (menuTypeCount == 0)
			return;

		if (menuTypeIndex >= menuTypeCount)
			menuTypeIndex = menuTypeCount;

		string menuItemTypeId = ddlMenuTypes.Items[menuTypeIndex].Value;

		var info = GetMenuTypeInfo(CurrentBxSite.TextEncoder.Decode(CurrentBxSite.Id), menuItemTypeId, _clientVirtualPath, _clientVirtualDirPath);
		if (info == null)
			return;

		if (info.SourcePath != null && info.Items != null)
		{
			for (int i = 0; i < info.Items.Count; i++)
				menuItems.Add(new ListItem(info.Items[i].Title, i.ToString()));
		}

		menuItems.Add(new ListItem(GetMessage("LastMenuItem"), menuItems.Count.ToString()));
		if (!string.IsNullOrEmpty(Request.Params[ddlMenuItems.ClientID]))
			ddlMenuItems.SelectedIndex = Convert.ToInt32(Request.Params[ddlMenuItems.ClientID]);
		else
			ddlMenuItems.SelectedIndex = menuItems.Count - 1;

		_menuItemsVisible = info.SourcePath != null;
		_createMenuVisible = info.SectionPath != null;
	}


	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		try
		{
			BXPageAsDialogWizardBehaviour wizard = DialogWizard;
			wizard.ButtonClick += new EventHandler<BXPageAsDialogButtonClickEventArgs>(wizard_ButtonClick);
			wizard.Settings.MinWidth = 655;
			wizard.Settings.MinHeight = 420;
			wizard.Settings.Width = 655;
			wizard.Settings.Height = 420;
			wizard.Settings.Resizeable = true;

			DescriptionIconClass = "bx-create-new-page";
			if (string.IsNullOrEmpty(_clientVirtualDirPath))
				throw new InvalidOperationException("Directory path is not defined!");

			DescriptionInfoParagraphs.Add(string.Format("{0} <b>{1}</b>", GetMessage("NewPageCreationInDir"), _clientVirtualDirPath));
			DescriptionInfoParagraphs.Add(string.Format("<a href=\"{0}?path={1}&new=\">{2}</a>", VirtualPathUtility.ToAbsolute("~/bitrix/admin/FileManEdit.aspx"), HttpUtility.UrlEncode(_clientVirtualDirPath), GetMessage("CreatePageInControlPanel")));

			if (IsPostBack)
			{
				_clientFileName = tbxPageName.Text;
				_clientTitle = tbxTitle.Text;
			}

			tblNewPageProp.DataSource = _keywordsTypes;
			tblNewPageProp.DataBind();

			Validate();
		}
		catch (System.Threading.ThreadAbortException /*ex*/)
		{
			//...игнорируем, вызвано Close();
		}
		catch (Exception ex)
		{
			Close(ex.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
		}
	}

	private BXSite _site = null;
	protected BXSite CurrentBxSite
	{
		get
		{
			if (_site == null)
			{
				if (string.IsNullOrEmpty(_clientVirtualPath))
					throw new InvalidOperationException("Could not find client virtual path!");
				_site = BXSite.GetCurrentSite(_clientVirtualPath, Request.Url.Host);
				if (_site == null)
					throw new InvalidOperationException(string.Format("Could not find site for path '{0}'!", _clientVirtualPath));
			}
			return _site;
		}
	}

	protected string CreateDefaultFileName(string directoryPhysicalPath, ref string title)
	{
		if (string.IsNullOrEmpty(directoryPhysicalPath))
			throw new ArgumentException("Is not defined!", "directoryPhysicalPath");

		DirectoryInfo di = new DirectoryInfo(directoryPhysicalPath);
		if (!di.Exists)
			throw new InvalidOperationException(string.Format("Directory '{0}' is not exists!", directoryPhysicalPath));

		string workDirPath = di.FullName;

		string defaultFileName = "Default";
		string defaultFileExt = "aspx";
		string[] fileNameArr = Directory.GetFileSystemEntries(workDirPath, string.Format("{0}*.{1}", defaultFileName, defaultFileExt));

		int fileNameArrLength = fileNameArr != null ? fileNameArr.Length : 0;
		string result = null;

		if (fileNameArrLength > 0)
		{
			Regex rxFileNameFormat = new Regex(string.Format(@"{0}(?<number>[1-9]{{1}}[0-9]{{0,5}})\.{1}", defaultFileName, defaultFileExt), RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

			int maxNum = 1;
			for (int i = 0; i < fileNameArrLength; i++)
			{
				string curFileName = fileNameArr[i];
				Match m = rxFileNameFormat.Match(curFileName);
				if (!m.Success)
					continue;
				//-
				int curNum = Convert.ToInt32(m.Groups["number"].Value);
				if (maxNum < curNum)
					maxNum = curNum;
			}

			result = string.Format("{0}{1}.{2}", defaultFileName, maxNum + 1, defaultFileExt);
			title = string.Format("{0} ({1})", GetMessage("DefaultTitle"), maxNum + 1);
		}

		if (result == null)
		{
			result = string.Format("{0}1.{1}", defaultFileName, defaultFileExt);
			title = string.Format("{0} ({1})", GetMessage("DefaultTitle"), 1);
		}
		return result;
	}
	void wizard_ButtonClick(object sender, BXPageAsDialogButtonClickEventArgs e)
	{
		try
		{
			if (!BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
			{
				Close(GetMessage("ACCESS_DENIED"), BXDialogGoodbyeWindow.LayoutType.Error, -1);
				return;
			}

			if (!IsValid)
				return;

			if (e.ButtonEntry != BXPageAsDialogButtonEntry.Done)
				return;

			if (string.IsNullOrEmpty(_clientFileName))
			{
				ShowError(GetMessage("EmptyFileName"));
				return;
			}

			string initClientFileName = _clientFileName;

			if (_clientFileName.EndsWith("."))
				_clientFileName = _clientFileName.Remove(_clientFileName.Length - 1);

			string fnExt = VirtualPathUtility.GetExtension(_clientFileName);
			if (string.IsNullOrEmpty(fnExt))
			{
				_clientFileName = string.Format("{0}.aspx", _clientFileName);
			}
			else if (string.Compare(fnExt, ".aspx", StringComparison.Ordinal) != 0)
			{
				_clientFileName = string.Format("{0}.aspx", _clientFileName);
			}

			//имя 
			string pageNameExtensionless = _clientFileName.Remove(_clientFileName.Length - 5);

			tbxPageName.Text = pageNameExtensionless;
			_clientVirtualPath = VirtualPathUtility.Combine(VirtualPathUtility.AppendTrailingSlash(_clientVirtualDirPath), _clientFileName);

			if (BXSecureIO.FileExists(_clientVirtualPath))
			{
				ShowError(String.Format("{0}: {1}", GetMessage("FileAlreadyExists"), _clientVirtualPath));
				return;
			}

			string menuTypeId = null;
			MenuTypeInfo info = null;
			if (chbxAddToMenu.Checked && ((info = CheckMenuTypeInfo(out menuTypeId)) == null || menuTypeId == null))
			{
				ShowError(GetMessageRaw("Error.YourAccountDontHasTheRightToModifyMenu"));
				return;
			}

			string content = Bitrix.Services.Text.BXDefaultFiles.BuildAspx(!string.IsNullOrEmpty(tbxTitle.Text) ? Encode(tbxTitle.Text) : GetMessage("NoTitle"), _clientVirtualPath, null);

			Bitrix.DataTypes.BXParamsBag<string> keywordBag = new Bitrix.DataTypes.BXParamsBag<string>();

			foreach (string keywordKey in _keywords.Keys)
			{
				if (!IsKeywordValueChanged(keywordKey))
					continue;

				string keywordValue = string.Empty;
				_keywords[keywordKey].TryGetValue("value", out keywordValue);

				if (string.IsNullOrEmpty(keywordValue))
					continue;

				keywordBag.Add(keywordKey, keywordValue);
			}

			content = BXParser.PersistPageKeywords(content, keywordBag);

			BXSecureIO.FileWriteAllText(_clientVirtualPath, content, Bitrix.Configuration.BXConfigurationUtility.DefaultEncoding);

			if (chbxAddToMenu.Checked)
				AddToMenu(menuTypeId, info, pageNameExtensionless);

			BXUndoPageCreationOperation undoOperation = new BXUndoPageCreationOperation();
			undoOperation.FileVirtualPath = _clientVirtualPath;
			undoOperation.MenuTypeId = menuTypeId;

			BXUndoInfo undo = new BXUndoInfo();
			undo.Operation = undoOperation;
			undo.Save();

			BXDialogGoodbyeWindow goodbye = new BXDialogGoodbyeWindow(string.Format(
				GetMessageRaw("NewPageIsSuccessfullyCreated"), 
				string.Concat(undo.GetClientScript(), " return false;"), 
				"#"), -1, BXDialogGoodbyeWindow.LayoutType.Success);
			BXDialogGoodbyeWindow.SetCurrent(goodbye);

			if (chbxEditAfterSave.Checked)
			{
				BXDialogSettings dlgSetting = new BXDialogSettings();
				dlgSetting.Height = 604;
				dlgSetting.Width = 968;
				dlgSetting.MinHeight = 400;
				dlgSetting.MinWidth = 780;

				SwitchToDialog(string.Format("{0}?path={1}&clientType=WindowManager&forcedRedirection=&noundo=", VirtualPathUtility.ToAbsolute("~/bitrix/dialogs/VisualPageEditor.aspx"), HttpUtility.UrlEncode(_clientVirtualPath)), dlgSetting, GetMessage("NewPageIsSuccessfullyCreatedShowEditDialog"), BXDialogGoodbyeWindow.LayoutType.Success, 2000);				
				return;
			}
			Redirect(BXSite.GetUrlForPath(_clientVirtualPath, null), string.Empty, BXDialogGoodbyeWindow.LayoutType.Success);
			return;
		}
		catch (System.Threading.ThreadAbortException)
		{
			//...игнорируем, вызвано Close();
		}
		catch (Exception ex)
		{
			Close(ex.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
		}
	}

	private MenuTypeInfo CheckMenuTypeInfo(out string menuTypeId)
	{
		menuTypeId = null;
		int menuTypeCount = _menuTypeDic != null ? _menuTypeDic.Count : 0;
		if (menuTypeCount > 0)
		{
			int menuTypeIndex = ddlMenuTypes.SelectedIndex;
			if (menuTypeIndex >= 0)
			{
				if (menuTypeIndex >= menuTypeCount)
					menuTypeIndex = menuTypeCount - 1;

				menuTypeId = ddlMenuTypes.Items[menuTypeIndex].Value;
			}
			if (menuTypeId != null)
				return GetMenuTypeInfo(CurrentBxSite.TextEncoder.Decode(CurrentBxSite.Id), menuTypeId, _clientVirtualPath, _clientVirtualDirPath);
		}
		return null;
	}
	private void AddToMenu(string menuTypeId, MenuTypeInfo info, string pageNameExtensionless)
	{
		BXPublicMenuItemCollection menuItemCol = null;
		if ((info.SourcePath != null && Request.Form[string.Concat(UniqueID, IdSeparator, "cbCreateInSection")] != "Y"))
			menuItemCol = info.Items;
		else if (info.SectionPath == null)
			return;

		bool isNewMenu = false;
		if (menuItemCol == null)
		{
			menuItemCol = new BXPublicMenuItemCollection();
			isNewMenu = true;
		}

		int menuItemIndex = ddlMenuItems.SelectedIndex;
		int menuItemCount = menuItemCol.Count;
		if (menuItemIndex < 0 || menuItemIndex > menuItemCount)
			menuItemIndex = menuItemCount;
		BXPublicMenuItem menuItem = new BXPublicMenuItem();
		menuItem.Link = Bitrix.Services.BXSiteRemapUtility.UnmapVirtualPath(_clientVirtualPath);

		string title = !string.IsNullOrEmpty(tbxMenuName.Text) ? tbxMenuName.Text : pageNameExtensionless;
		menuItem.Title = HttpUtility.HtmlEncode(title);

		menuItemCol.Insert(menuItemIndex, menuItem);
		menuItemCount++;
		for (int i = 0; i < menuItemCount; i++)
			menuItemCol[i].Sort = (i + 1) * 10;

		string menuDirPath = BXPath.GetDirectory(!isNewMenu ? info.SourcePath : info.SectionPath);
		BXPublicMenu.Menu.Save(menuDirPath, menuTypeId, menuItemCol);
	}

	protected bool IsKeywordValueChanged(string key)
	{
		if (string.IsNullOrEmpty(key))
			throw new ArgumentException("Is not defined!", "key");

		if (_keywords == null)
			throw new InvalidOperationException("Keywords is not assigned!");

		Dictionary<string, string> keywordDic = null;
		if (!_keywords.TryGetValue(key, out keywordDic))
			return false;
		string defaultValue = string.Empty, currentValue = string.Empty;
		keywordDic.TryGetValue("defaultValue", out defaultValue);

		if (string.IsNullOrEmpty(defaultValue)) //отсутствие значения по умолчанию - значение изменено
			return true;

		keywordDic.TryGetValue("value", out currentValue);
		return string.Compare(defaultValue, currentValue, StringComparison.CurrentCulture) != 0;
	}

	protected string GetKeywordValue(string key)
	{
		if (string.IsNullOrEmpty(key))
			throw new ArgumentException("Is not defined!", "key");

		if (_keywords == null)
			throw new InvalidOperationException("Keywords is not assigned!");

		Dictionary<string, string> keywordDic = null;
		if (!_keywords.TryGetValue(key, out keywordDic))
			return string.Empty;
		string result = null;
		return keywordDic.TryGetValue("value", out result) ? result : string.Empty;
	}

	/// <summary>
	/// Определено какое-либо меню
	/// </summary>
	protected bool IsAnyMenuDefined
	{
		get
		{
			return _menuTypeDic != null && _menuTypeDic.Count > 0;
		}
	}


	protected override void EnsureDialogWizard()
	{
		DialogWizard = IsAnyMenuDefined ? newPageWizardLong : newPageWizardShort;
	}

	/// <summary>
	/// CheckUserAuthorization
	/// Проверить авторизацию пользователя для работы с диалогом
	/// </summary>
	/// <returns></returns>
	protected override bool CheckUserAuthorization()
	{
		return BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage);
	}

	protected override string GetParametersBagName()
	{
		return "BitrixDialogNewPageWizardParamsBag";
	}

	protected override void ExternalizeParameters(BXParamsBag<string> paramsBag)
	{
		if (paramsBag == null)
			throw new ArgumentNullException("paramsBag");

		paramsBag.Add(tbxPageName.ID, tbxPageName.Text);
		paramsBag.Add(tbxTitle.ID, tbxTitle.Text);
		paramsBag.Add(chbxEditAfterSave.ID, chbxEditAfterSave.Checked.ToString());
		paramsBag.Add(chbxAddToMenu.ID, chbxAddToMenu.Checked.ToString());
		paramsBag.Add(tbxMenuName.ID, tbxMenuName.Text);

		paramsBag.Add(ddlMenuTypes.ID, Request.Form[ddlMenuTypes.UniqueID] ?? string.Empty);
		paramsBag.Add(ddlMenuItems.ID, Request.Form[ddlMenuItems.UniqueID] ?? string.Empty);

		//при попытке получения неавторизированным пользователем ёще неясно появится ли право на модификацию меню после авторизации 
		if (!BXUser.Identity.IsAuthenticated && !IsPostBack)
			paramsBag.Add("checkForGrantMenuModification", true.ToString());

		LoadKeywords();
		foreach (KeyValuePair<string, Dictionary<string, string>> keywordPair in _keywords)
		{
			string keywordValue;
			if (!keywordPair.Value.TryGetValue("value", out keywordValue))
				continue;
			paramsBag.Add(string.Format("keyword[{0}]", keywordPair.Key), keywordValue);
		}
	}

	protected override void InternalizeParameters(BXParamsBag<string> paramsBag)
	{
		if (paramsBag == null)
			throw new ArgumentNullException("paramsBag");

		if (paramsBag.ContainsKey(tbxPageName.ID))
			tbxPageName.Text = paramsBag[tbxPageName.ID];

		if (paramsBag.ContainsKey(tbxTitle.ID))
			tbxTitle.Text = paramsBag[tbxTitle.ID];

		if (paramsBag.ContainsKey(chbxEditAfterSave.ID))
			chbxEditAfterSave.Checked = Convert.ToBoolean(paramsBag[chbxEditAfterSave.ID]);

		if (paramsBag.ContainsKey(chbxAddToMenu.ID))
			chbxAddToMenu.Checked = Convert.ToBoolean(paramsBag[chbxAddToMenu.ID]);

		if (!chbxAddToMenu.Checked &&
			paramsBag.ContainsKey("checkForGrantMenuModification") &&
			!string.IsNullOrEmpty(paramsBag["checkForGrantMenuModification"]) &&
			BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit))
			chbxAddToMenu.Checked = true;

		if (paramsBag.ContainsKey(tbxMenuName.ID))
			tbxMenuName.Text = paramsBag[tbxMenuName.ID];

		if (paramsBag.ContainsKey(ddlMenuTypes.ID))
		{
			string text = paramsBag[ddlMenuTypes.ID];
			if (!string.IsNullOrEmpty(text))
			{
				ListItem li = ddlMenuTypes.Items.FindByValue(text);
				if (li != null)
				{
					int index = ddlMenuTypes.Items.IndexOf(li);
					if (ddlMenuTypes.SelectedIndex != index)
					{
						ddlMenuTypes.SelectedIndex = index;
						PrepareMenuItemsList();
					}
				}
			}
		}

		if (paramsBag.ContainsKey(ddlMenuItems.ID))
		{
			string text = paramsBag[ddlMenuItems.ID];
			if (!string.IsNullOrEmpty(text))
			{
				ListItem li = ddlMenuItems.Items.FindByValue(text);
				if (li != null)
				{
					int index = ddlMenuItems.Items.IndexOf(li);
					if (ddlMenuItems.SelectedIndex != index)
						ddlMenuItems.SelectedIndex = index;
				}
			}
		}

		foreach (KeyValuePair<string, Dictionary<string, string>> keywordPair in _keywords)
		{
			string paramKey = string.Format("keyword[{0}]", keywordPair.Key);
			if (!paramsBag.ContainsKey(paramKey))
				continue;

			string keywordValue = paramsBag[paramKey];
			if (keywordPair.Value.ContainsKey("value"))
				keywordPair.Value["value"] = keywordValue;
			else
				keywordPair.Value.Add("value", keywordValue);
		}
	}

	private static readonly Regex srxKeyWordCode = new Regex(@"^PROPERTY\[(?<index>\d+)\]\[CODE\]$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

	private void LoadKeywords()
	{
		if (!IsPostBack)
			return;

		if (_keywords == null)
			throw new InvalidOperationException("Keywords are not initialized!");

		Match m = null;
		foreach (string key in Request.Form.Keys)
		{
			if (!(m = srxKeyWordCode.Match(key)).Success)
				continue;

			int index = Convert.ToInt32(m.Groups["index"].Value);
			string value = Request.Form[string.Format("PROPERTY[{0}][VALUE]", index)];
			if (string.IsNullOrEmpty(value))
				continue;

			string code = Request.Form[key];
			Dictionary<string, string> keywordDic = null;
			if (!_keywords.TryGetValue(code, out keywordDic))
				continue;

			if (keywordDic.ContainsKey("value"))
				keywordDic["value"] = value;
			else
				keywordDic.Add("value", value);
		}
	}

    protected override void OnPreRender(EventArgs e)
    {
        ClientScriptManager cs = ClientScript;
        if (cs == null)
            throw new InvalidOperationException("Could not find ClientScriptManager!");

        cs.RegisterStartupScript(GetType(), "Bitrix.NewPageWizard.onPageLoad", "Bitrix.NewPageWizard.onPageLoad();", true);

        base.OnPreRender(e);
    }

	protected override void Render(HtmlTextWriter writer)
	{
		if (_maxMenuItemIndex < 0)
			_maxMenuItemIndex = 0;
		else
			_maxMenuItemIndex++;

		ClientScriptManager cs = ClientScript;
		if (cs == null)
			throw new InvalidOperationException("Could not find ClientScriptManager!");

		for (int i = 0; i <= _maxMenuItemIndex; i++)
			cs.RegisterForEventValidation(ddlMenuItems.UniqueID, i.ToString());

		base.Render(writer);
	}



}
