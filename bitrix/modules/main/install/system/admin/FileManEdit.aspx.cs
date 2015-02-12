using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Compilation;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.Components;
using Bitrix.Services;
using Bitrix.UI;
using DB = Bitrix.DataLayer;
using Bitrix.Security;
using Bitrix.IO;
using Bitrix.Services.Js;
using Bitrix.Configuration;
using Bitrix.DataTypes;
using Bitrix.Services.Text;
using System.Web.Hosting;
using System.Threading;

public partial class bitrix_admin_FileManEdit : Bitrix.UI.BXAdminPage, IBXFileManPage
{
	private string curPath, curDir, curFileName, curSiteId;
	private string savePath, saveDir, saveFileName, saveSiteId;
	protected Encoding readEncoding;
	private bool isNew;
	private bool isAspx;
	private bool isAspxRaw;
	private Dictionary<string, string> menuTypes;
	private Dictionary<string, MenuControls> menuControls;
	private BXParamsBag<TextBox> keywordsControls = new BXParamsBag<TextBox>();
	private BXParamsBag<Literal> keywordsLiterals = new BXParamsBag<Literal>();
	private BXParamsBag<string> keywords;
	private StringBuilder MenuTypesScript;
	private IContentEditor contentEditor;
	protected bool fatalError;

	private string goBackUrl;
	protected override string BackUrl
	{
		get
		{
			goBackUrl = base.BackUrl;
			if (goBackUrl == null)
			{
				try
				{
					string dir = String.Empty;
					string entity = String.Empty;

					if (BXSecureIO.DirectoryExists(curPath))
						return (goBackUrl = "FileMan.aspx?path=" + HttpUtility.UrlEncode(curPath));
					if (!BXPath.BreakExistingPath(curPath, ref dir, ref entity))
						return (goBackUrl = "FileMan.aspx");
					goBackUrl = "FileMan.aspx?path=" + HttpUtility.UrlEncode(dir);
				}
				catch
				{
					return "FileMan.aspx";
				}
			}
			return goBackUrl;
		}
	}

	public override void ProcessRequest(HttpContext context)
	{
		if (context.Request["check"] != null)
		{
			bool result = false;
			try
			{
				if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
					BXAuthentication.AuthenticationRequired();

				string path = BXPath.ToVirtualRelativePath(context.Request["path"]);
				result = BXSecureIO.FileExists(path);
			}
			catch
			{
			}
			context.Response.Write(result.ToString().ToLower());
			context.Response.StatusCode = 200;
			context.Response.ContentType = "text/plain";
			context.Response.Expires = 0;
			context.Response.Cache.SetNoStore();
			context.Response.AppendHeader("Pragma", "no-cache");
			context.Response.End();
		}
		else
			base.ProcessRequest(context);
	}

	private StringBuilder errorText = new StringBuilder();
	private int showMessage;
	private void PrepareResultMessage()
	{
		if (this.showMessage == 0)
		{
			resultMessage.Visible = false;
			return;
		}
		resultMessage.Visible = true;
		resultMessage.CssClass = (this.showMessage > 0) ? "Ok" : "Error";
		resultMessage.IconClass = (this.showMessage > 0) ? "Ok" : "Error";
		resultMessage.Title = (this.showMessage > 0) ? GetMessage("Message.OperationSuccessful") : GetMessage("Message.OperationErrors");
		resultMessage.Content = (this.showMessage > 0) ? String.Empty : this.errorText.ToString();
	}
	private void ShowError(string encodedMessage)
	{
		this.showMessage = -1;
		this.errorText.AppendLine("<br />");
		this.errorText.AppendLine(encodedMessage);
	}
	private void ShowOk()
	{
		if (this.showMessage == 0)
			this.showMessage = 1;
	}
	private void FatalError(string messageText)
	{
		fatalError = true;
		BXContextMenuToolbar1.Visible = false;
		BXTabControl1.Visible = false;
		ShowError(Encode(messageText));
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		try
		{
			ValidateInit();

			SelectContentEditor();
			contentEditor.FilePath = curPath;

			InitEditor();
		}
		catch (ThreadAbortException)
		{
		}
		catch (PublicException ex)
		{
			FatalError(ex.Message);
			if (ex.InnerException != null)
				BXLogService.LogAll(ex.InnerException, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		catch (BXSecurityException ex)
		{
			FatalError(ex.Message);
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		catch (Exception ex)
		{
			FatalError(GetMessageRaw("Error.Unknown"));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		if (fatalError)
			return;

		Validate();
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		try
		{
			if (!IsPostBack && !isNew && Request.QueryString["showok"] != null)
				ShowOk();



			if (fatalError)
				return;
			PrepareEditor();
			contentEditor.FilePath = curPath;
		}
		catch (PublicException ex)
		{
			FatalError(ex.Message);
			if (ex.InnerException != null)
				BXLogService.LogAll(ex.InnerException, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		catch (Exception ex)
		{
			FatalError(GetMessageRaw("Error.Unknown"));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		finally
		{
			PrepareResultMessage();
			MasterTitle = string.Format(
				"{0}: <font style=\"font-weight: normal;\">{1}</font>",
				GetMessage(isAspx ? "MasterTitle.PageEdit" : "MasterTitle.TextEdit"),
				Encode(curPath)
			);
		}
	}

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        BXPage.RegisterScriptInclude("~/bitrix/js/main/utils_net.js");

        ScriptManager sm = ScriptManager.GetCurrent(Page);
        if (sm != null)
        {
            string dispatcherScriptVirtualPath = "~/bitrix/js/main/BXAspnetFormDispatcher.js";
            string dispatcherScriptUrl = string.Concat(VirtualPathUtility.ToAbsolute(dispatcherScriptVirtualPath), "?t=", HttpUtility.UrlEncode(System.IO.File.GetLastWriteTimeUtc(System.Web.Hosting.HostingEnvironment.MapPath(dispatcherScriptVirtualPath)).Ticks.ToString()));

            bool isFound = false;
            foreach (ScriptReference scripRef in sm.Scripts)
            {
                if (!string.Equals(scripRef.Path, dispatcherScriptUrl, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                isFound = true;
                break;
            }
            if (!isFound)
                sm.Scripts.Add(new ScriptReference(dispatcherScriptUrl));

        }
        else
            BXPage.RegisterScriptInclude("~/bitrix/js/main/BXAspnetFormDispatcher.js");

    }

	protected void SaveClick(object sender, EventArgs e)
	{
		if (!IsValid || fatalError)
			return;

		DoSave();

		if (showMessage == -1)
			return;

		if (Request.QueryString["showsaved"] != null)
			Response.Redirect(BXSite.GetUrlForPath(savePath, null));
		else
			GoBack();
	}
	protected void ApplyClick(object sender, EventArgs e)
	{
		if (!IsValid || fatalError)
			return;

		DoSave();

		if (showMessage == -1)
			return;

		Response.Redirect(MakeEditFileUrl(savePath));
	}
	protected void CancelClick(object sender, EventArgs e)
	{
		GoBack();
	}

	protected void MenuTypeOptions_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
			return;
		KeyValuePair<string, string> pair = (KeyValuePair<string, string>)e.Item.DataItem;
		MenuControls c = new MenuControls();
		menuControls[pair.Key] = c;
		c.Where = (HtmlTable)e.Item.FindControl("Where");
		c.New = (RadioButtonList)e.Item.FindControl("New");
		c.NewOptions = (HtmlTable)e.Item.FindControl("NewOptions");
		c.Name = (TextBox)e.Item.FindControl("Name");
		c.Before = (DropDownList)e.Item.FindControl("Before");
		c.ExistsOptions = (HtmlTable)e.Item.FindControl("ExistsOptions");
		c.AddTo = (DropDownList)e.Item.FindControl("AddTo");

		c.New.Items[0].Attributes["onclick"] = string.Format("jsUtils.ShowTable('{1}', false);jsUtils.ShowTable('{0}', true, 'inline');", c.NewOptions.ClientID, c.ExistsOptions.ClientID);
		c.New.Items[1].Attributes["onclick"] = string.Format("jsUtils.ShowTable('{1}', false);jsUtils.ShowTable('{0}', true, 'inline');", c.ExistsOptions.ClientID, c.NewOptions.ClientID);

		MenuTypesScript.AppendFormat("jsUtils.ShowTable('{0}', this.value=='{1}', 'inline');", c.Where.ClientID, BXJSUtility.Encode(pair.Key));

		if (!String.IsNullOrEmpty(pair.Key) && (pair.Key.IndexOfAny(Path.GetInvalidFileNameChars()) == -1))
			try
			{
				string menuPath = BXPath.Combine(this.curDir, pair.Key + ".menu");
				BXSecureIO.DemandView(menuPath);
				BXPublicMenuItemCollection menu = BXPublicMenu.Menu.Load(this.curDir, pair.Key);// new BXMenu(BXPath.ToPhysicalPath(menuPath));
				/*menu.Sort(delegate(BXMenuItem a, BXMenuItem b)
				{
					return a.Sort.CompareTo(b.Sort);
				});*/
				for (int i = 0; i < menu.Count; i++)
				{
					BXPublicMenuItem item = menu[i];
					c.Before.Items.Add(new ListItem(item.Title, i.ToString()));
					c.AddTo.Items.Add(new ListItem(item.Title, i.ToString()));
				}
				c.Before.Items.Add(new ListItem(GetMessageRaw("LastItem"), menu.Count.ToString()));
				c.Before.SelectedIndex = c.Before.Items.Count - 1;
			}
			catch
			{
			}
	}
	protected void KeywordsTable_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
			return;
		e.Item.ID = "k" + keywordsControls.Count;
		keywordsControls.Add(((KeyValuePair<string, string>)e.Item.DataItem).Key, (TextBox)e.Item.FindControl("Value"));
		keywordsLiterals.Add(((KeyValuePair<string, string>)e.Item.DataItem).Key, (Literal)e.Item.FindControl("Inherited"));
	}
	protected void VisualEditor_IncludeWebEditorScript(object sender, BXWebEditor.IncludeWebEditorScriptArgs e)
	{
		e.Writer.WriteLine("<script>");
		e.Writer.WriteLine("FE_MESS = {}");
		e.Writer.WriteLine("FE_MESS.FILEMAN_HTMLED_WARNING = \"" + GetMessageJS("CloseWarning") + "\";");
		e.Writer.WriteLine("FE_MESS.FILEMAN_HTMLED_MANAGE_TB = \"" + GetMessageJS("ManageToolbar") + "\";");
		e.Writer.WriteLine("var _bEdit = {0}", isNew ? "false" : "true");
		e.Writer.WriteLine("window._curDir = \"{0}\";", this.curDir);
		e.Writer.WriteLine("window.BX_DOTNET_ID = {");
		e.Writer.WriteLine("SaveAs: \"{0}\",", SaveAs.ClientID);
		e.Writer.WriteLine("SaveAsPath: \"{0}\",", SaveAsPath.ClientID);
		e.Writer.WriteLine("settingsTab: \"{0}\"", settingsTab.ClientID);
		e.Writer.WriteLine("};");

		e.Writer.WriteLine("window.save_but_id = \"{0}\";", Save.ClientID);
		e.Writer.WriteLine("window.apply_but_id = \"{0}\";", Apply.ClientID);
		e.Writer.WriteLine("window.cancel_but_id = \"{0}\";", Cancel.ClientID);


		// New file command
		if (BXUser.IsCanOperateFile(this.curDir, BXFileOperation.WriteExecutable))
		{
			int i = 0;
			string fileTemplate = BXPath.Combine(this.curDir, "Default");
			string filePath = fileTemplate + ".aspx";
			while (BXSecureIO.FileExists(filePath))
			{
				i++;
				filePath = fileTemplate + i + ".aspx";
			}
			e.Writer.WriteLine("window.bx_new_file_command_path = \"FileManEdit.aspx?path={0}&encoding={1}&new=\"", HttpUtility.UrlEncode(filePath), HttpUtility.UrlEncode(BXConfigurationUtility.DefaultEncoding.WebName));
		}
		e.Writer.WriteEndTag("script");

		string v = Bitrix.IO.BXFile.GetFileTimestamp(BXPath.MapPath("~/bitrix/controls/Main/editor/js/toolbar_aspx.js")).ToString();
		e.Writer.WriteLine("<script type=\"text/javascript\" src=\"" + VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/editor/js/toolbar_aspx.js") + "?v=" + v + "\"></script>");

		v = Bitrix.IO.BXFile.GetFileTimestamp(BXPath.MapPath("~/bitrix/controls/Main/editor/js/FileManEdit_editor.js")).ToString();
		e.Writer.WriteLine("<script type=\"text/javascript\" src=\"" + VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/editor/js/FileManEdit_editor.js") + "?v=" + v + "\"></script>");
	}

	private void FillEncodings(DropDownList list)
	{
		List<ListItem> enList = new List<ListItem>();
		foreach (EncodingInfo en in Encoding.GetEncodings())
			enList.Add(new ListItem(en.DisplayName, en.Name));
		enList.Sort(
			delegate(ListItem a, ListItem b)
			{
				return String.Compare(a.Text, b.Text, StringComparison.InvariantCultureIgnoreCase);
			}
		);
		list.Items.AddRange(enList.ToArray());
	}
	private BXParamsBag<string> GetPageSettings()
	{
		BXParamsBag<string> settings = new BXParamsBag<string>();
		foreach (KeyValuePair<string, TextBox> p in keywordsControls)
			settings.Add(p.Key, p.Value.Text);
		return settings;
	}
	private void SetPageSettings(BXParamsBag<string> settings)
	{
		foreach (KeyValuePair<string, string> p in settings)
			if (keywordsControls.ContainsKey(p.Key))
				keywordsControls[p.Key].Text = p.Value;
	}
	private string MakeEditFileUrl(string filePath)
	{
		UriBuilder uri = new UriBuilder(Request.Url);
		BXQueryString query = BXUri.GetQueryString(Request.Url.ToString());
		query["path"] = filePath;
		query.Remove("new");
		query.Remove("encoding");
		query.Remove("showok");
		query.Add("showok", string.Empty);
		uri.Query = query.ToString().TrimStart('?');
		return uri.Uri.PathAndQuery.ToString();
	}
	protected string MakeChangeEncodingFileUrl()
	{
		UriBuilder uri = new UriBuilder(Request.Url);
		BXQueryString query = BXUri.GetQueryString(Request.Url.ToString());
		query.Remove("encoding");
		query.Remove("showok");
		uri.Query = query.ToString().TrimStart('?');
		return uri.Uri.PathAndQuery.ToString();
	}

	protected string BuildScriptParameters()
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>();
		parameters.Add("gobackUrl", BackUrl);
		parameters.Add("saveAsId", SaveAs.ClientID);
		parameters.Add("saveAsPathId", SaveAsPath.ClientID);
		parameters.Add("saveAsPath", SaveAsPath.Value);
		parameters.Add("curDir", this.curDir);
		parameters.Add("curPath", this.curPath);
		parameters.Add("curFileName", this.curFileName);
		parameters.Add("isNew", this.isNew);
		parameters.Add("confirmMessage", GetMessageRaw("OverwriteConfirm"));
		parameters.Add("errorRequestFailedMessage", GetMessageRaw("ErrorRequestFailedMessage"));
		parameters.Add("tabControlName", BXTabControl1.UniqueID);
		return BXJSUtility.BuildJSON(parameters);
	}
	private void SelectContentEditor()
	{
		string editor = Request.QueryString["editor"] ?? BXOptionManager.GetOptionString("main", "EditorDefaultPageEditor", "text");
		if (editor.Equals("visual", StringComparison.InvariantCultureIgnoreCase) && isAspx)
		{
			//prepare visual editor
			Content.ActiveViewIndex = 1;
			contentEditor = new VisualContentEditor(VisualEditor, this);
		}
		else
		{
			//prepare text editor
			Content.ActiveViewIndex = 0;
			contentEditor = new TextBoxContentEditor(TextEditor, this);
		}
	}

	private void ValidateInit()
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
			BXAuthentication.AuthenticationRequired();

		this.isNew = (Request["new"] != null);
		this.readEncoding = BXConfigurationUtility.DefaultEncoding;
		if (!string.IsNullOrEmpty(Request.QueryString["encoding"]))
			try
			{
				readEncoding = Encoding.GetEncoding(Request.QueryString["encoding"]);
			}
			catch
			{
			}

		//Получаем путь из Request'а
		this.curPath = BXPath.ToVirtualRelativePath(Request["path"] ?? "~");

		//Пробуем сгенерировать новое имя файла
		if (isNew)
		{
			try
			{
				string filePath = BXPath.ToPhysicalPath(curPath);
				FileInfo info = new FileInfo(filePath);
				bool isFileNameDefined = !string.IsNullOrEmpty(info.Name);
				bool isDirectoryExist = Directory.Exists(filePath);
				if (!isFileNameDefined || isDirectoryExist)
				{
					string workDir = isDirectoryExist ? info.FullName : info.DirectoryName;
					string workFile = null;
					int i = 0;
					do
					{
						workFile = Path.Combine(workDir, "Default" + (i == 0 ? string.Empty : i.ToString()) + ".aspx");
						i++;
					}
					while (File.Exists(workFile));

					curPath = BXPath.ToVirtualRelativePath(workFile);
				}
			}
			catch
			{
			}
		}

		BXSecureIO.DemandWrite(this.curPath);

		if (BXSecureIO.FileExists(this.curPath) && isNew
			|| BXSecureIO.DirectoryExists(this.curPath))
		{
			throw new PublicException(string.Format(GetMessageRaw("Message.FileAlreadyExists"), this.curPath));
		}

		if (!BXPath.BreakPath(this.curPath, ref this.curDir, ref this.curFileName))
			throw new PublicException(GetMessageRaw("Message.InvalidDirectory"));

		BXSite site = BXSite.GetCurrentSite(curDir, Request.Url.Host);
		curSiteId = site == null ? null : site.Id;

		BXFileType fileType = BXFileInfo.GetFileType(curPath);
		if ((fileType & BXFileType.TextFormat) == BXFileType.Unknown)
			throw new PublicException(GetMessageRaw("Message.UnknownFormat"));

		string ext = (BXPath.GetExtension(curPath) ?? string.Empty).ToLowerInvariant();
		isAspx = ext.Equals("aspx", StringComparison.InvariantCultureIgnoreCase);
		if (isAspx && Request.QueryString["raw"] != null)
		{
			isAspx = false;
			isAspxRaw = true;
		}
	}
	private void InitEditor()
	{
		FillEncodings(ChangeEncoding);
		ChangeEncoding.SelectedValue = BXConfigurationUtility.DefaultEncoding.WebName;


		SaveAs.Text = this.curFileName;

		vlrSaveAs.ValidationExpression = BXPath.NameValidationRegexString;
		vlrSaveAs.ErrorMessage = GetMessage("IllegalCharactersIsDecetedInFileName");
		vlrSaveAsRequired.ErrorMessage = GetMessage("EmptyFileName");

		if (isAspx)
			InitPageEditor();
	}
	private void InitPageEditor()
	{
		menuTypes = BXPublicMenu.GetMenuTypes(curSiteId);
		foreach (KeyValuePair<string, string> p in menuTypes)
			MenuType.Items.Add(new ListItem(p.Value, p.Key));
		AddMenu.Enabled = (menuTypes.Count > 0);
		if (AddMenu.Enabled)
			AddMenu.Attributes["onclick"] = String.Format("jsUtils.ShowTableRow('{0}', this.checked, 'block');", MenuOptions.ClientID);
		MenuOptions.Visible = (menuTypes.Count > 0);

		menuControls = new Dictionary<string, MenuControls>(StringComparer.InvariantCultureIgnoreCase);
		MenuTypesScript = new StringBuilder();
		MenuTypeOptions.DataSource = menuTypes;
		MenuTypeOptions.DataBind();
		MenuType.Attributes["onchange"] = MenuTypesScript.ToString();

		keywordsControls.Clear();
		KeywordsTable.DataSource = keywords = BXPageManager.GetKeywords(curSiteId);
		KeywordsTable.DataBind();
	}
	private void LoadData()
	{
		try
		{
			contentEditor.Content = String.Empty;
			if (isNew)
			{
				if (isAspx || isAspxRaw)
					contentEditor.Content = BXDefaultFiles.BuildAspx("Untitled Page", curPath, null);
				return;
			}

			try
			{
				contentEditor.Content = BXSecureIO.FileReadAllText(this.curPath, readEncoding);
			}
			catch (Exception ex)
			{
				throw new PublicException(string.Format("{0}: '{1}'", GetMessageRaw("Message.UnableToRead"), curPath), ex);
			}
		}
		catch (PublicException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new PublicException(GetMessageRaw("Error.Unknown"), ex);
		}
	}
	private void PrepareEditor()
	{
		if (isAspx)
			contentEditor.PrepareNew();
		if (!IsPostBack)
			LoadData();

		encodingOptions.Visible = !isNew;
		newPageOptions.Visible = isNew && isAspx;
		settingsTab.Visible = isAspx;
		EditPageTitle.Visible = isAspx;

		if (isAspx)
			PreparePageEditor();

		mainTab.Title = GetMessage(isAspx ? "TabTitle.PageEdit" : "TabTitle.TextEdit");
	}
	private void PreparePageEditor()
	{
		if (!AddMenu.Checked)
			MenuOptions.Style[HtmlTextWriterStyle.Display] = "none";
		foreach (KeyValuePair<string, MenuControls> p in menuControls)
		{
			if (!p.Key.Equals(MenuType.SelectedValue, StringComparison.InvariantCultureIgnoreCase))
				p.Value.Where.Style[HtmlTextWriterStyle.Display] = "none";
			if (p.Value.New.SelectedValue != "new")
				p.Value.NewOptions.Style[HtmlTextWriterStyle.Display] = "none";
			if (p.Value.New.SelectedValue != "exists")
				p.Value.ExistsOptions.Style[HtmlTextWriterStyle.Display] = "none";
		}

		BXSectionInfo sectionInfo = BXSectionInfo.GetCumulativeSection(curDir);
		foreach (string keyword in keywords.Keys)
		{
			string inherited;
			if (string.IsNullOrEmpty(keywordsControls[keyword].Text)
				&& sectionInfo.Keywords.TryGetValue(keyword, out inherited)
				&& !string.IsNullOrEmpty(inherited))
			{
				keywordsLiterals[keyword].Text = string.Format("<b>{0}:</b> {1}", GetMessage("Label.CurrentValue"), Encode(inherited));
			}
		}
	}

	private void ValidateSave()
	{
		this.saveDir = curDir;
		this.saveFileName = SaveAs.Text;
		this.savePath = BXPath.Combine(saveDir, saveFileName);
		this.saveSiteId = curSiteId;

		//Пробуем получить путь из скрытого поля
		if (string.IsNullOrEmpty(Request.Form[SaveAsPath.UniqueID]))
			return;

		string saveAsPath = BXPath.TrimEnd(BXPath.ToVirtualRelativePath(Request.Form[SaveAsPath.UniqueID] ?? "~"));
		this.saveDir = BXPath.GetDirectory(saveAsPath);
		this.savePath = BXPath.Combine(saveDir, saveFileName);

		BXSite saveSite = BXSite.GetCurrentSite(saveDir, Request.Url.Host);
		saveSiteId = saveSite == null ? null : saveSite.Id;

		SaveAsPath.Value = this.savePath;
	}
	private void ValidateMenu()
	{
		if (!AddMenu.Checked)
			return;

		if (saveSiteId != curSiteId)
			throw new PublicException(GetMessageRaw("Message.SiteMismatch"));

		string menuType = MenuType.SelectedValue;

		if (string.IsNullOrEmpty(menuType)
			|| !menuControls.ContainsKey(menuType)
			|| menuType.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new PublicException(GetMessageRaw("Message.IllegalMenuType"));
		}

		if (menuControls[menuType].New.SelectedValue != "exists"
			&& string.IsNullOrEmpty(menuControls[menuType].Name.Text))
		{
			throw new PublicException(GetMessageRaw("Message.MenuNameRequired"));
		}
	}
	private void SaveMenu()
	{
		if (!AddMenu.Checked)
			return;

		try
		{
			string menuType = MenuType.SelectedValue;
			string menuPath = BXPath.Combine(this.saveDir, menuType + ".menu");
			BXSecureIO.DemandView(menuPath);
			BXSecureIO.DemandWrite(menuPath);

			BXPublicMenuItemCollection menu = BXPublicMenu.Menu.Load(this.saveDir, menuType);

			if (menuControls[menuType].New.SelectedValue != "exists") //new
			{
				int j = menuControls[menuType].Before.SelectedIndex;
				if (j < 0 || j > menu.Count)
					j = menu.Count;
				BXPublicMenuItem item = new BXPublicMenuItem();
				item.Title = menuControls[menuType].Name.Text;
				item.Link = BXSiteRemapUtility.UnmapVirtualPath(BXPath.ToVirtualRelativePath(this.savePath));
				menu.Insert(j, item);
				for (int k = 0; k < menu.Count; k++)
					menu[k].Sort = (k + 1) * 10;
				BXPublicMenu.Menu.Save(this.saveDir, menuType, menu);
			}
			else
			{
				int j = menuControls[menuType].AddTo.SelectedIndex;
				if (j < 0 || j >= menu.Count)
					j = menu.Count - 1;
				if (j >= 0)
				{
					BXPublicMenuItem item = menu[j];
					item.Links.Add( BXSiteRemapUtility.UnmapVirtualPath(BXPath.ToVirtualRelativePath(savePath)));
					for (int k = 0; k < menu.Count; k++)
						menu[k].Sort = (k + 1) * 10;
					BXPublicMenu.Menu.Save(this.saveDir, menuType, menu);
				}
			}
		}
		catch (Exception ex)
		{
			throw new PublicException(GetMessageRaw("Message.UnableToCreateMenu"), ex);
		}
	}
	private void SaveContent()
	{
		string content = contentEditor.Content;
		if (!BXSecureIO.CheckWrite(this.savePath, content))
			throw new PublicException(string.Format("{0}: '{1}'", GetMessageRaw("Message.InsufficientRightsToWrite"), this.savePath));

		try
		{
			/*
			 * BXParamsBag<string> k = new BXParamsBag<string>();
			 * k["a"] = "b";
			 * k["\""] = "&";
			 * contentEditor.Content = Bitrix.Components.BXParser.PersistPageKeywords(contentEditor.Content, k);
			 */

			if (isAspx)
				BXSecureIO.SaveAspx(this.savePath, content, null, BXConfigurationUtility.DefaultEncoding);
			else
				BXSecureIO.FileWriteAllText(this.savePath, content, BXConfigurationUtility.DefaultEncoding);
		}
		catch (Exception ex)
		{
			throw new PublicException(String.Format("{0}: '{1}'", GetMessageRaw("Message.UnableToWrite"), savePath), ex);
		}
	}
	private void DoSave()
	{
		try
		{
			ValidateSave();

			if (isNew && isAspx)
			{
				ValidateMenu();
				SaveMenu();
			}

			SaveContent();
		}
		catch (PublicException ex)
		{
			ShowError(Encode(ex.Message));
			if (ex.InnerException != null)
				BXLogService.LogAll(ex.InnerException, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		catch (BXSecurityException ex)
		{
			ShowError(Encode(ex.Message));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		catch (Exception ex)
		{
			ShowError(GetMessageRaw("Error.Unknown"));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
	}

	//NESTED CLASSES
	private class MenuControls
	{
		public HtmlTable Where;
		public RadioButtonList New;
		public HtmlTable NewOptions;
		public TextBox Name;
		public DropDownList Before;
		public HtmlTable ExistsOptions;
		public DropDownList AddTo;
	}
	private interface IContentEditor
	{
		string Content
		{
			get;
			set;
		}
		string FilePath
		{
			set;
		}

		void PrepareNew();
	}
	private class TextBoxContentEditor : IContentEditor
	{
		TextBox content;
		bitrix_admin_FileManEdit page;

		public TextBoxContentEditor(TextBox content, bitrix_admin_FileManEdit page)
		{
			this.content = content;
			this.page = page;
		}

		public string Content
		{
			get
			{
				string c = content.Text;

				if (!string.IsNullOrEmpty(c) && page.isAspx)
				{
					c = BXParser.PersistPageTitle(c, page.PageTitle.Text);
					c = BXParser.PersistPageKeywords(c, page.GetPageSettings());
				}
				return c;
			}
			set
			{
				string c = value;

				if (!string.IsNullOrEmpty(c) && page.isAspx)
				{
					page.PageTitle.Text = BXParser.ParsePageTitle(c);
					page.SetPageSettings(BXParser.ParsePageKeywords(c));
					c = BXParser.PersistPageTitle(c, null);
					c = BXParser.PersistPageKeywords(c, null);
				}
				content.Text = c;
			}
		}
		public string FilePath
		{
			set
			{
			}
		}

		public void PrepareNew()
		{

		}
	}
	private class VisualContentEditor : IContentEditor
	{
		BXWebEditor content;
		bitrix_admin_FileManEdit page;

		public VisualContentEditor(BXWebEditor content, bitrix_admin_FileManEdit page)
		{
			this.content = content;
			this.page = page;
		}

		public string Content
		{
			get
			{
				string c = content.Content;
				//zg, Bitrix, 2008.06.07
				//if (page.isAspx)
				if (!string.IsNullOrEmpty(c) && page.isAspx)
				{
					c = BXParser.PersistPageTitle(c, page.PageTitle.Text);
					c = BXParser.PersistPageKeywords(c, page.GetPageSettings());
				}
				return c;
			}
			set
			{
				string c = value;
				//zg, Bitrix, 2008.06.07
				//if (page.isAspx)
				if (!string.IsNullOrEmpty(c) && page.isAspx)
				{
					page.PageTitle.Text = BXParser.ParsePageTitle(c);
					page.SetPageSettings(BXParser.ParsePageKeywords(c));
					c = BXParser.PersistPageTitle(c, null);
					c = BXParser.PersistPageKeywords(c, null);
				}
				content.Content = c;
			}
		}
		public string FilePath
		{
			set
			{
				content.Path = value.Replace(Path.DirectorySeparatorChar, '/');
			}
		}

		public void PrepareNew()
		{
			content.AutoDetectContent = false;
			content.ContentType = BXWebEditor.VEMode.MasterContent;
		}
	}

	#region IBXFileManPage Members

	public string ProvidePath()
	{
		string path = BXPath.ToVirtualRelativePath(Request["path"] ?? "~");
		string dir = string.Empty;
		string file = string.Empty;
		if (!BXPath.BreakPath(path, ref dir, ref file))
			return "~";
		return BXPath.ToVirtualRelativePath(dir).ToLowerInvariant();
	}

	#endregion
}
