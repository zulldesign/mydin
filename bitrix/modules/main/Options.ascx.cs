using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Bitrix;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Js;
using Bitrix.UI;
using Bitrix.Services.Text;
using Bitrix.DataTypes;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

public partial class bitrix_modules_main_Options : BXControl
{
	bool canManageSettings;
	class FileManSettings
	{
		public BXControl Container;
		public string ValidationGroup;
		public string SiteId;
		public HiddenField MenuTypesCount;
		public System.Web.UI.WebControls.Table MenuTypes;
		public List<TypeTitleControl> MenuTypeControls = new List<TypeTitleControl>();
		public HiddenField PropertiesCount;
		public System.Web.UI.WebControls.Table Properties;
		public List<TypeTitleControl> PropertiesControls = new List<TypeTitleControl>();
		public TextBox LoginUrl;
		public TextBox SiteMapMenuTypes;

		public void AddMenuTypeRow(string typeValue, string titleValue)
		{
			TableRow newRow = new TableRow();
			TableCell typeCell = new TableCell();
			newRow.Cells.Add(typeCell);
			TableCell titleCell = new TableCell();
			newRow.Cells.Add(titleCell);

			typeCell.Width = Unit.Percentage(50);
			typeCell.Style[HtmlTextWriterStyle.WhiteSpace] = "nowrap";
			titleCell.Width = Unit.Percentage(50);

			TypeTitleControl mtc = new TypeTitleControl();

			mtc.Type = new TextBox();
			mtc.Type.Text = typeValue;
			mtc.Type.Width = Unit.Percentage(100);
			mtc.Type.ID = "MenuSettingsType_" + MenuTypeControls.Count;
			typeCell.Controls.Add(mtc.Type);

			RegularExpressionValidator validator = new RegularExpressionValidator();
			validator.ValidationExpression = "[a-zA-Z0-9_]+";
			validator.Text = "*";
			validator.ErrorMessage = Container.GetMessage("Error.InvalidMenuType");
			validator.ControlToValidate = mtc.Type.ID;
			validator.Display = ValidatorDisplay.Static;
			validator.ValidationGroup = ValidationGroup;
			typeCell.Controls.Add(validator);

			mtc.Title = new TextBox();
			mtc.Title.Text = titleValue;
			mtc.Title.Width = Unit.Percentage(100);
			mtc.Title.ID = "MenuSettingsTitle_" + MenuTypeControls.Count;
			titleCell.Controls.Add(mtc.Title);

			MenuTypes.Rows.Add(newRow);
			MenuTypeControls.Add(mtc);
			MenuTypesCount.Value = MenuTypeControls.Count.ToString();
		}
		public void DeleteMenuTypeRow()
		{
			if (MenuTypeControls.Count > 0)
			{
				MenuTypes.Rows.RemoveAt(MenuTypes.Rows.Count - 1);
				MenuTypeControls.RemoveAt(MenuTypeControls.Count - 1);
			}
		}
		public void DeleteAllMenuTypeRows()
		{
			while (MenuTypeControls.Count > 0)
				DeleteMenuTypeRow();
		}


		public void AddPropertiesRow(string typeValue, string titleValue)
		{
			TableRow newRow = new TableRow();
			TableCell typeCell = new TableCell();
			newRow.Cells.Add(typeCell);
			TableCell titleCell = new TableCell();
			newRow.Cells.Add(titleCell);

			typeCell.Width = Unit.Percentage(50);
			typeCell.Style[HtmlTextWriterStyle.WhiteSpace] = "nowrap";
			titleCell.Width = Unit.Percentage(50);

			TypeTitleControl pc = new TypeTitleControl();

			pc.Type = new TextBox();
			pc.Type.Text = typeValue;
			pc.Type.Width = Unit.Percentage(100);
			pc.Type.ID = "PropertiesSettingsType_" + PropertiesControls.Count;
			typeCell.Controls.Add(pc.Type);

			RegularExpressionValidator validator = new RegularExpressionValidator();
			validator.ValidationExpression = "[a-zA-Z0-9_]+";
			validator.Text = "*";
			validator.ErrorMessage = Container.GetMessage("Error.InvalidProperty");
			validator.ControlToValidate = pc.Type.ID;
			validator.Display = ValidatorDisplay.Static;
			validator.ValidationGroup = ValidationGroup;
			typeCell.Controls.Add(validator);

			pc.Title = new TextBox();
			pc.Title.Text = titleValue;
			pc.Title.Width = Unit.Percentage(100);
			pc.Title.ID = "PropertiesSettingsTitle_" + PropertiesControls.Count;
			titleCell.Controls.Add(pc.Title);

			Properties.Rows.Add(newRow);
			PropertiesControls.Add(pc);
			PropertiesCount.Value = PropertiesControls.Count.ToString();
		}
		public void DeletePropertiesRow()
		{
			if (PropertiesControls.Count > 0)
			{
				Properties.Rows.RemoveAt(Properties.Rows.Count - 1);
				PropertiesControls.RemoveAt(PropertiesControls.Count - 1);
			}
		}
		public void DeleteAllPropertiesRows()
		{
			while (PropertiesControls.Count > 0)
				DeletePropertiesRow();
		}

		public void Load()
		{
			string site = string.IsNullOrEmpty(SiteId) ? null : SiteId;


			DeleteAllMenuTypeRows();
			string data = BXOptionManager.GetOptionString("main", "MenuTypes", string.Empty, site);
			IDictionary types = BXSerializer.Deserialize(data) as IDictionary;
			if (types != null)
				foreach (DictionaryEntry t in types)
					AddMenuTypeRow(
						t.Key.ToString(),
						t.Value.ToString()
					);
			AddMenuTypeRow(string.Empty, string.Empty);
			AddMenuTypeRow(string.Empty, string.Empty);


			SiteMapMenuTypes.Text = BXConfigurationUtility.Options.Site[site].SiteMapMenuTypes;


			DeleteAllPropertiesRows();
			data = BXOptionManager.GetOptionString("main", "PageProperties", string.Empty, site);
			IDictionary properties = BXSerializer.Deserialize(data) as IDictionary;
			if (properties != null)
				foreach (DictionaryEntry t in properties)
					AddPropertiesRow(
						t.Key.ToString(),
						t.Value.ToString()
					);
			AddPropertiesRow(string.Empty, string.Empty);
			AddPropertiesRow(string.Empty, string.Empty);


			LoginUrl.Text = BXOptionManager.GetOptionString("main", "SiteLoginUrl", string.Empty, site);
		}

		public void Save()
		{
			string site = string.IsNullOrEmpty(SiteId) ? null : SiteId;


			Dictionary<string, string> menuTypes = new Dictionary<string, string>();
			foreach (TypeTitleControl mtc in MenuTypeControls)
			{
				string type = mtc.Type.Text.Trim();
				if (!string.IsNullOrEmpty(type))
					menuTypes[type] = mtc.Title.Text;
			}
			BXOptionManager.SetOptionString("main", "MenuTypes", BXSerializer.Serialize(menuTypes), site);
			BXConfigurationUtility.Options.Site[site].SiteMapMenuTypes = SiteMapMenuTypes.Text;


			Dictionary<string, string> properties = new Dictionary<string, string>();
			foreach (TypeTitleControl pc in PropertiesControls)
			{
				string type = pc.Type.Text.Trim();
				if (!string.IsNullOrEmpty(type))
					properties[type] = pc.Title.Text;
			}
			BXOptionManager.SetOptionString("main", "PageProperties", BXSerializer.Serialize(properties), site);


			BXOptionManager.SetOptionString("main", "SiteLoginUrl", LoginUrl.Text, site);
		}
	}

	class TypeTitleControl
	{
		public TextBox Type;
		public TextBox Title;
	}

	protected class CacheProvider
	{
		public BXExternalCacheProviderInfo Provider;
		public Control Editor;
	}

	protected class SessionConfigurator
	{
		public BXSessionConfigurator Configurator;
		public Control Editor;
	}

	Dictionary<string, FileManSettings> siteSettings = new Dictionary<string, FileManSettings>();
	StringBuilder sitesScript = new StringBuilder();
	protected List<CacheProvider> Providers;
	protected CacheProvider CurrentProvider;
	protected List<SessionConfigurator> SessionConfigurators;
	protected SessionConfigurator CurrentSessionConfigurator;

	protected void MenuSettings_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
			return;

		FileManSettings s = new FileManSettings();
		s.Container = this;
		s.ValidationGroup = BXTabControl1.ValidationGroup;
		s.MenuTypesCount = (HiddenField)e.Item.FindControl("MenuTypesCount");
		s.MenuTypes = (System.Web.UI.WebControls.Table)e.Item.FindControl("MenuTypes");
		s.MenuTypes.Style[HtmlTextWriterStyle.Visibility] = "hidden";
		s.MenuTypes.Style[HtmlTextWriterStyle.Display] = "none";
		s.SiteId = ((ListItem)e.Item.DataItem).Value;

		siteSettings[s.SiteId] = s;
		sitesScript.AppendFormat("r = document.getElementById('{0}'); r.style.visibility = ((this.value == '{1}') ? 'visible' : 'hidden'); r.style.display = ((this.value == '{1}') ? 'inline' : 'none');", s.MenuTypes.ClientID, s.SiteId);
	}
	protected void PropertiesSettings_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
			return;

		FileManSettings s = siteSettings[((ListItem)e.Item.DataItem).Value];

		s.PropertiesCount = (HiddenField)e.Item.FindControl("PropertiesCount");
		s.Properties = (System.Web.UI.WebControls.Table)e.Item.FindControl("Properties");
		s.Properties.Style[HtmlTextWriterStyle.Visibility] = "hidden";
		s.Properties.Style[HtmlTextWriterStyle.Display] = "none";

		sitesScript.AppendFormat("r = document.getElementById('{0}'); r.style.visibility = ((this.value == '{1}') ? 'visible' : 'hidden'); r.style.display = ((this.value == '{1}') ? 'inline' : 'none');", s.Properties.ClientID, s.SiteId);
	}
	protected void LoginUrlSettings_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
			return;

		FileManSettings s = siteSettings[((ListItem)e.Item.DataItem).Value];
		s.LoginUrl = (TextBox)e.Item.FindControl("LoginUrl");
		s.LoginUrl.Style[HtmlTextWriterStyle.Display] = "none";

		sitesScript.AppendFormat("r = document.getElementById('{0}'); r.style.display = ((this.value == '{1}') ? '' : 'none');", s.LoginUrl.ClientID, s.SiteId);
	}
	protected void SiteMapSettings_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
			return;

		FileManSettings s = siteSettings[((ListItem)e.Item.DataItem).Value];
		s.SiteMapMenuTypes = (TextBox)e.Item.FindControl("SiteMapMenuTypes");
		s.SiteMapMenuTypes.Style[HtmlTextWriterStyle.Display] = "none";

		sitesScript.AppendFormat("r = document.getElementById('{0}'); r.style.display = ((this.value == '{1}') ? '' : 'none');", s.SiteMapMenuTypes.ClientID, s.SiteId);
	}
	protected void MailerDefaultEmailFromValidator_Validate(object sender, ServerValidateEventArgs e)
	{
		e.IsValid = string.IsNullOrEmpty(MailerSmtpHost.Text) || !string.IsNullOrEmpty(e.Value);
	}

	protected void PageBaseClassValidator_Validate(object sender, ServerValidateEventArgs e)
	{
		CustomValidator v = (CustomValidator)sender;
		e.IsValid = true;

		string ddValue = Request.Form[UniqueID + "$PageBaseClass"];
		string value = !string.IsNullOrEmpty(ddValue) ? ddValue : PageBaseClassCustom.Text;
		if (BXStringUtility.IsNullOrTrimEmpty(value))
			return;

		Type t;
		try
		{
			t = Type.GetType(value);
			if (t == null)
				throw new Exception();
		}
		catch
		{
			v.ErrorMessage = GetMessage("Error.InvalidClassName");
			e.IsValid = false;
			return;
		}

		if (!typeof(BXPublicPage).IsAssignableFrom(t))
		{
			v.ErrorMessage = GetMessage("Error.InvalidPageClass");
			e.IsValid = false;
		}
	}


	protected void Page_Init(object sender, EventArgs e)
	{
		canManageSettings = BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		BXTabControl1.ShowApplyButton = BXTabControl1.ShowSaveButton = canManageSettings;
		BXTabControl1.ShowCancelButton = Request.QueryString[BXConfigurationUtility.Constants.BackUrl] != null;

		InitCacheProviders();
		InitSessionConfigurators();
	}

	private void InitCacheProviders()
	{
		Providers =
			BXCacheManager.GetExternalCacheProviders()			
			.OrderBy(x => x.Id)
			.Select(x =>
			{
				var control = x.GetConfigEditor(Page, x.Id == BXConfigurationUtility.Options.ExternalCacheProviderId ? BXConfigurationUtility.Options.GetExternalCacheProviderConfig() : null);
				if (control != null)
				{
					control.ID = ProcessId("cacheProvider_", x.Id);
					CacheProviderEditors.Controls.Add(control);
				}
				return new CacheProvider
				{
					Provider = x,
					Editor = control
				};
			})
			.ToList();

		var providerId = IsPostBack ? Request.Form[UniqueID + "$cacheProvider"] : BXConfigurationUtility.Options.ExternalCacheProviderId;
		CurrentProvider = Providers.Where(x => x.Provider.Id == providerId).FirstOrDefault();
	}

	private void InitSessionConfigurators()
	{
		var webConfig = XDocument.Load(MapPath("~/web.config"));
		var element = webConfig.Root.XPathSelectElement("system.web/sessionState");

		var items = BXSessionConfigurator.GetAll();
		items.ForEach(x => 
		{
			x.Error += ReportSessionError;
			x.Success += ReportSessionSuccess;
			x.Init(element);
		});

		SessionConfigurators =
			items			
			.OrderBy(x => x.Id)
			.Select(x =>
			{
				var control = x.GetConfigurationControl(Page);
				if (control != null)
				{
					control.ID = ProcessId("sessionConfigurator_", x.Id);
					SessionConfiguratorsHolder.Controls.Add(control);
				}
				return new SessionConfigurator
				{
					Configurator = x,
					Editor = control
				};
			})
			.ToList();

		var inproc = new BXInprocSessionConfigurator();
		inproc.Init(element);

		if (SessionConfigurators.Count > 0 || !inproc.IsSet)
		{
			inproc.Error += ReportSessionError;
			inproc.Success += ReportSessionSuccess;

			var control = inproc.GetConfigurationControl(Page);
			if (control != null)
			{
				control.ID = ProcessId("sessionConfigurator_", inproc.Id);
				SessionConfiguratorsHolder.Controls.Add(control);
			}

			SessionConfigurators.Insert(
				0,
				new SessionConfigurator
				{
					Configurator = inproc,
					Editor = control
				}
			);
		}

		if (IsPostBack)
		{
			var id = Request.Form[UniqueID + "$sessionConfigurator"];
			CurrentSessionConfigurator = SessionConfigurators.Where(x => x.Configurator.Id == id).FirstOrDefault();
		}
		else
			CurrentSessionConfigurator = SessionConfigurators.Where(x => x.Configurator.IsSet).FirstOrDefault();		
	}

	private void ReportSessionError(object sender, BXSessionConfiguratorErrorEventArgs e)
	{
		errorMessage.AddErrorMessage(Encode(e.Text));
	}

	private void ReportSessionSuccess(object sender, EventArgs e)
	{
		successMessage.Visible = true;
	}

	private string ProcessId(string prefix, string id)
	{
		string s = id.ToLowerInvariant();
		var sb = new StringBuilder(prefix);		
		for (int j = 0; j < s.Length; j++)
		{
			if ((s[j] >= 'a' && s[j] <= 'z') ||
				(s[j] >= '0' && s[j] <= '9'))
				sb.Append(s[j]);
			else if (s[j] == '-' || s[j] == '_' || s[j] == '.')
				sb.Append('_');
		}
		return sb.ToString();
	}
	
	protected void Page_Load(object sender, EventArgs e)
	{
		List<ListItem> sites = new List<ListItem>();
		sites.Add(new ListItem(GetMessageRaw("AllSites"), string.Empty));
		BXSiteCollection siteColl = BXSite.GetList(null, null, null, null, BXTextEncoder.EmptyTextEncoder);
		foreach (BXSite site in siteColl)
			sites.Add(new ListItem(site.Name, site.Id));

		MenuSettings.DataSource = sites;
		MenuSettings.DataBind();
		PropertiesSettings.DataSource = sites;
		PropertiesSettings.DataBind();
		LoginUrlSettings.DataSource = sites;
		LoginUrlSettings.DataBind();
		SiteMapSettings.DataSource = sites;
		SiteMapSettings.DataBind();

		SettingsForSite.Items.Clear();
		SettingsForSite.Items.AddRange(sites.ToArray());
		SettingsForSite.Attributes["onchange"] = "var isIE = jsUtils.IsIE; var r = null;" + sitesScript.ToString();
		string val = Request.Form[SettingsForSite.UniqueID] ?? string.Empty;
		siteSettings[val].MenuTypes.Style[HtmlTextWriterStyle.Visibility] = "visible";
		siteSettings[val].MenuTypes.Style[HtmlTextWriterStyle.Display] = "inline";
		siteSettings[val].Properties.Style[HtmlTextWriterStyle.Visibility] = "visible";
		siteSettings[val].Properties.Style[HtmlTextWriterStyle.Display] = "inline";
		siteSettings[val].LoginUrl.Style[HtmlTextWriterStyle.Display] = "";
		siteSettings[val].SiteMapMenuTypes.Style[HtmlTextWriterStyle.Display] = "";


		DefaultEncoding.Items.Clear();
		FillEncodings(DefaultEncoding);
		if (Request.Form[DefaultEncoding.UniqueID] != null)
			DefaultEncoding.SelectedValue = Request.Form[DefaultEncoding.UniqueID];

		UserExecutableFileExtList.Style.Add("width", "320px");
		UserExcludedFromExecutableFileExtList.Style.Add("width", "320px");

		foreach (FileManSettings s in siteSettings.Values)
		{
			if (Request.Form[s.MenuTypesCount.UniqueID] != null)
				s.MenuTypesCount.Value = Request.Form[s.MenuTypesCount.UniqueID];
			int count = 0;
			int temp;
			if (int.TryParse(s.MenuTypesCount.Value, out temp))
				count = Math.Max(temp, 0);
			s.MenuTypesCount.Value = count.ToString();
			for (int i = 0; i < count; i++)
				s.AddMenuTypeRow(string.Empty, string.Empty);

			if (Request.Form[s.PropertiesCount.UniqueID] != null)
				s.PropertiesCount.Value = Request.Form[s.PropertiesCount.UniqueID];
			count = 0;
			if (int.TryParse(s.PropertiesCount.Value, out temp))
				count = Math.Max(temp, 0);
			s.PropertiesCount.Value = count.ToString();
			for (int i = 0; i < count; i++)
				s.AddPropertiesRow(string.Empty, string.Empty);
		}

		if (!Page.IsPostBack)
			LoadData();
		else
		{
			Control c = (this.Page as BXAdminPage).FindControlRecursive("ddlEntitySelector");
			if (c != null)
			{
				DropDownList ddl = (c as DropDownList);
				if (ddl != null)
				{
					string t = Server.UrlDecode(Page.Request.Params["__EVENTTARGET"]);
					if (t.Equals(ddl.UniqueID, StringComparison.InvariantCultureIgnoreCase))
						LoadData();
				}
			}
		}
	}
	protected void BXTabControl1_Command(object sender, Bitrix.UI.BXTabControlCommandEventArgs e)
	{
		bool noRedirect = false;
		bool successAction = true;
		if (e.CommandName == "cancel")
			Response.Redirect(Page.Request.Params[BXConfigurationUtility.Constants.BackUrl] ?? BXSefUrlManager.CurrentUrl.ToString());
		else if (Page.IsValid)
		{
			if (e.CommandName == "save")
			{
				if (!SaveSettings())
				{
					successAction = false;
					noRedirect = true;
				}
			}
			else if (e.CommandName == "apply")
			{
				if (!SaveSettings())
					successAction = false;
				noRedirect = true;
			}
		}
		if (!noRedirect)
		{
			if (Page.Request.Params[BXConfigurationUtility.Constants.BackUrl] != null)
			{
				Page.Response.Redirect(Page.Request.QueryString[BXConfigurationUtility.Constants.BackUrl]);
			}
			else
			{
				if (successAction && Page.IsValid)
				{
					successMessage.Visible = (e.CommandName != "cancel");
					LoadData();
				}
			}
		}
		else
		{
			if (successAction && Page.IsValid)
			{
				successMessage.Visible = (e.CommandName != "cancel");
				LoadData();
			}
		}

	}
	void FillEncodings(DropDownList list)
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

	protected IEnumerable<ListItem> GetPageClasses()
	{
		Type baseClass = typeof(BXPublicPage);
		yield return new ListItem(GetMessageRaw("Kernel.Default"), ProcessAssemblyQualifiedName(baseClass.AssemblyQualifiedName));
		foreach (Type t in Bitrix.Modules.BXReflector.FindChildTypes(typeof(BXPublicPage), true))
		{
			object[] attrs = t.GetCustomAttributes(typeof(BXPublicPageAttribute), false);
			if (attrs == null || attrs.Length == 0)
				continue;

			//check for app code
			string an = t.Assembly.FullName;
			int i = an.IndexOf('.');
			if (i != -1)
				an = an.Remove(i);
			//end

			BXPublicPageAttribute attr = (BXPublicPageAttribute)attrs[0];
			yield return new ListItem(attr.Title ?? t.Name, an == "App_Code" ? (t.FullName + ", __Code") : ProcessAssemblyQualifiedName(t.AssemblyQualifiedName));
		}
	}

	static string ProcessAssemblyQualifiedName(string name)
	{
		int i = name.IndexOf(',');
		if (i < 0 || i + 1 == name.Length)
			return name;
		i = name.IndexOf(',', i + 1);
		if (i < 0)
			return name;
		return name.Remove(i);
	}

	private void LoadData()
	{
		tbSiteName.Text = BXOptionManager.GetOptionString("main", "site_name", "");
		cbSaveOriginalFileNames.Checked = "Y".Equals(BXOptionManager.GetOptionString("main", "SaveOriginalFileName", "N"), StringComparison.InvariantCultureIgnoreCase);
		cbCorrectFileNames.Checked = "Y".Equals(BXOptionManager.GetOptionString("main", "ConvertOriginalFileName", "Y"), StringComparison.InvariantCultureIgnoreCase);
		UserExecutableFileExtList.Text = String.Join(",", BXConfigurationUtility.Options.UserExecutableFileExtensions);
		UserExcludedFromExecutableFileExtList.Text = String.Join(",", BXConfigurationUtility.Options.UserExcludedFromExecutableFileExtensions);

		MailerSmtpHost.Text = BXOptionManager.GetOptionString("main", "MailerSmtpHost", String.Empty);
		MailerSmtpPort.Text = BXOptionManager.GetOptionInt("main", "MailerSmtpPort", 25).ToString();
		MailerSmtpUsername.Text = BXOptionManager.GetOptionString("main", "MailerSmtpUsername", String.Empty);
		MailerSmtpPassword.Value = BXOptionManager.GetOptionString("main", "MailerSmtpPassword", String.Empty);
		MailerDefaultEmailFrom.Text = BXOptionManager.GetOptionString("main", "MailerDefaultEmailFrom", String.Empty);
		MailerUseSsl.Checked = BXOptionManager.GetOption("main", "MailerUseSsl", false);

		AvatarMaxSizeKB.Text = BXConfigurationUtility.Options.User.AvatarMaxSizeKB != 0 ? BXConfigurationUtility.Options.User.AvatarMaxSizeKB.ToString() : string.Empty;
		AvatarMaxWidth.Text = BXConfigurationUtility.Options.User.AvatarMaxWidth != 0 ? BXConfigurationUtility.Options.User.AvatarMaxWidth.ToString() : string.Empty;
		AvatarMaxHeight.Text = BXConfigurationUtility.Options.User.AvatarMaxHeight != 0 ? BXConfigurationUtility.Options.User.AvatarMaxHeight.ToString() : string.Empty;

		try
		{
			DefaultEncoding.SelectedValue = BXOptionManager.GetOptionString("main", "EditorDefaultEncoding", BXConfigurationUtility.DefaultEncoding.WebName);
		}
		catch
		{
			DefaultEncoding.SelectedValue = BXConfigurationUtility.DefaultEncoding.WebName;
		}

		foreach (FileManSettings s in siteSettings.Values)
			s.Load();

		string val = BXOptionManager.GetOptionString("main", "EditorDefaultPageEditor", "text");
		if (DefaultPageEditor.Items.FindByValue(val) != null)
			DefaultPageEditor.SelectedValue = val;
		else
			DefaultPageEditor.SelectedValue = "text";

		MainContentPlaceHolder.Text = BXConfigurationUtility.Options.MainContentPlaceholderID;

		AutoCache.Checked = BXOptionManager.GetOption("main", "ComponentsAutoCache", true);
		EnableAnonymousDebug.Checked = BXConfigurationUtility.Options.EnableAnonymousDebug;
		HandleError404.Checked = BXConfigurationUtility.Options.Handle404;
		HandleStandardSefExceptions.Checked = BXConfigurationUtility.Options.HandleStandardSefExceptions;
		MultisitingExceptions.Text = string.Join("\r\n", BXConfigurationUtility.Options.MultisitingExceptions);

		RenderComponents.Checked = BXConfigurationUtility.Options.RenderComponents;

		SendConfirmationRequest.Checked = BXConfigurationUtility.Options.SendConfirmationRequest;

		IntervalToStoreUnconfirmedUsers.Text = BXConfigurationUtility.Options.IntervalToStoreUnconfirmedUsers.ToString();

		OpenIdCustomPropertyCode.Text = BXConfigurationUtility.Options.User.OpenIDCustomFieldCode;
		LiveIdCustomPropertyCode.Text = BXConfigurationUtility.Options.User.LiveIDCustomFieldCode;

		EnableOpenId.Checked = BXConfigurationUtility.Options.EnableOpenId;
		EnableLiveId.Checked = BXConfigurationUtility.Options.EnableLiveId;

		LiveIDApplicationID.Text = BXConfigurationUtility.Options.LiveIDApplicationID;
		LiveIDSecretKey.Text = BXConfigurationUtility.Options.LiveIDSecretKey;

		CrossDomainAuthentication.Checked = BXConfigurationUtility.Options.User.CrossDomainAuthentication;
	}

	private bool SaveSettings()
	{
		try
		{
			if (!canManageSettings)
				throw new UnauthorizedAccessException(GetMessageRaw("Auth.UnauthorizedAccessException"));

			//BXOptionManager.SetOptionString("main", "default_admin_language", ddlAdminLanguage.SelectedValue);
			BXOptionManager.SetOptionString("main", "site_name", tbSiteName.Text);
			BXOptionManager.SetOptionString("main", "SaveOriginalFileName", (cbSaveOriginalFileNames.Checked ? "Y" : "N"));
			BXOptionManager.SetOptionString("main", "ConvertOriginalFileName", (cbCorrectFileNames.Checked ? "Y" : "N"));
			BXConfigurationUtility.Options.UserExecutableFileExtensions = UserExecutableFileExtList.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			BXConfigurationUtility.Options.UserExcludedFromExecutableFileExtensions = UserExcludedFromExecutableFileExtList.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			int i;
			BXOptionManager.SetOptionString("main", "MailerSmtpHost", MailerSmtpHost.Text.Trim());
			BXOptionManager.SetOptionInt("main", "MailerSmtpPort", int.TryParse(MailerSmtpPort.Text, out i) ? i : 25);
			BXOptionManager.SetOptionString("main", "MailerSmtpUsername", MailerSmtpUsername.Text);
			//if (MailerSmtpPassword.Value != "")
			BXOptionManager.SetOptionString("main", "MailerSmtpPassword", MailerSmtpPassword.Value);
			BXOptionManager.SetOptionString("main", "MailerDefaultEmailFrom", MailerDefaultEmailFrom.Text);
			BXOptionManager.SetOption("main", "MailerUseSsl", MailerUseSsl.Checked);
			BXMailer.ReloadSettings();

			BXConfigurationUtility.Options.User.AvatarMaxSizeKB = int.TryParse(AvatarMaxSizeKB.Text, out i) ? i : 0;
			BXConfigurationUtility.Options.User.AvatarMaxWidth = int.TryParse(AvatarMaxWidth.Text, out i) ? i : 0;
			BXConfigurationUtility.Options.User.AvatarMaxHeight = int.TryParse(AvatarMaxHeight.Text, out i) ? i : 0;

			BXOptionManager.SetOptionString("main", "EditorDefaultEncoding", string.IsNullOrEmpty(DefaultEncoding.SelectedValue) ? BXConfigurationUtility.DefaultEncoding.WebName : DefaultEncoding.SelectedValue);
			BXOptionManager.SetOptionString("main", "EditorDefaultPageEditor", string.IsNullOrEmpty(DefaultPageEditor.SelectedValue) ? "text" : DefaultPageEditor.SelectedValue);

			foreach (FileManSettings s in siteSettings.Values)
				s.Save();

			BXWebEditor.Options.MainContentPlaceholder = MainContentPlaceHolder.Text;

			BXOptionManager.SetOption<bool>("main", "ComponentsAutoCache", AutoCache.Checked);
			BXConfigurationUtility.Options.EnableAnonymousDebug = EnableAnonymousDebug.Checked;
			BXConfigurationUtility.Options.Handle404 = HandleError404.Checked;
			BXConfigurationUtility.Options.HandleStandardSefExceptions = HandleStandardSefExceptions.Checked;
			BXConfigurationUtility.Options.SetMultisitingExceptions(MultisitingExceptions.Text);


			string ddValue = Request.Form[UniqueID + "$PageBaseClass"];
			string value = !string.IsNullOrEmpty(ddValue) ? ddValue : PageBaseClassCustom.Text;

			BXConfigurationUtility.Options.PageBaseClass = value;
			BXConfigurationUtility.Options.RenderComponents = RenderComponents.Checked;

			BXConfigurationUtility.Options.SendConfirmationRequest = SendConfirmationRequest.Checked;
			// нужно создать агента для удаления пользователей, не подтвердивших регистрацию
			if (SendConfirmationRequest.Checked)
			{
				BXSchedulerAgent a = BXSchedulerAgent.GetByName("delete_unconfirmed_users_agent");
				if (a == null)
				{
					a = new BXSchedulerAgent("delete_unconfirmed_users_agent");
					a.SetClassNameAndAssembly(typeof(Bitrix.Security.BXUser.Scheme.DeleteUnconfirmedUsersExecutor));
					a.Period = new TimeSpan(0, 3, 0);
					a.StartTime = DateTime.Now;
					a.Save();
				}
			}
			BXConfigurationUtility.Options.IntervalToStoreUnconfirmedUsers = int.TryParse(IntervalToStoreUnconfirmedUsers.Text, out i) ? i : 7;

			var useLiveId = EnableLiveId.Checked;
			var useOpenId = EnableOpenId.Checked;
			if (BXConfigurationUtility.Options.EnableLiveId != useLiveId)
				BXConfigurationUtility.Options.EnableLiveId = useLiveId;
			if (BXConfigurationUtility.Options.EnableOpenId != useOpenId)
				BXConfigurationUtility.Options.EnableOpenId = useOpenId;

			var newCode = "";
			if (useOpenId)
			{
				newCode = OpenIdCustomPropertyCode.Text;
				if (!string.IsNullOrEmpty(newCode))
				{
					CreateUserCustomFieldIfNeed(newCode, "OpenID");
					BXConfigurationUtility.Options.User.OpenIDCustomFieldCode = newCode;
				}
			}
			if (useLiveId)
			{
				newCode = LiveIdCustomPropertyCode.Text;
				if (!string.IsNullOrEmpty(newCode))
				{
					CreateUserCustomFieldIfNeed(newCode, "LiveID");
					BXConfigurationUtility.Options.User.LiveIDCustomFieldCode = newCode;
				}
			}

			var liveIdSecretKey = LiveIDSecretKey.Text.Replace(" ", "");

			if (BXConfigurationUtility.Options.LiveIDSecretKey != liveIdSecretKey)
				BXConfigurationUtility.Options.LiveIDSecretKey = liveIdSecretKey;

			var liveIdAppId = LiveIDApplicationID.Text.Replace(" ", "");

			if (BXConfigurationUtility.Options.LiveIDApplicationID != liveIdAppId)
				BXConfigurationUtility.Options.LiveIDApplicationID = liveIdAppId;

			BXConfigurationUtility.Options.User.CrossDomainAuthentication = CrossDomainAuthentication.Checked;

			if (CurrentProvider != null)
			{
				BXConfigurationUtility.Options.ExternalCacheProviderId = CurrentProvider.Provider.Id;
				if (CurrentProvider.Editor != null)
					BXConfigurationUtility.Options.SetExternalCacheProviderConfig(CurrentProvider.Provider.SaveConfig(CurrentProvider.Editor));
				else
					BXConfigurationUtility.Options.SetExternalCacheProviderConfig(null);
			}
			else
			{
				BXConfigurationUtility.Options.ExternalCacheProviderId = null;
				BXConfigurationUtility.Options.SetExternalCacheProviderConfig(null);
			}

			BXCacheManager.ReloadExternalCache();
			BXPublicMenu.Menu.RefreshSettings();

			return true;
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(Encode(e.Message));
		}
		return false;
	}

	BXLanguageCollection langs;

	BXLanguageCollection Langs
	{
		get
		{
			return langs ?? (langs = BXLanguage.GetList(null, null));
		}
	}

	void CreateUserCustomFieldIfNeed(string newCode, string msgPrefix)
	{
		BXCustomFieldCollection fields = BXCustomEntityManager.GetFields("USER");
		BXCustomField field;
		if (!fields.TryGetValue(newCode, out field))
		{
			BXCustomField f = new BXCustomField("USER", newCode, "Bitrix.System.Text");
			f.Mandatory = false;
			f.Multiple = false;
			foreach (var lang in Langs)
			{
				var loc = new BXCustomFieldLocalization();
				loc.ListColumnLabel = BXLoc.GetMessage(lang.Id, "~/bitrix/modules/Main/lang", msgPrefix + "Label");
				loc.ListFilterLabel = BXLoc.GetMessage(lang.Id, "~/bitrix/modules/Main/lang", msgPrefix + "Label");
				loc.EditFormLabel = BXLoc.GetMessage(lang.Id, "~/bitrix/modules/Main/lang", msgPrefix + "Label");
				f.Localization[lang.Id] = loc;
			}
			f.Save();
		}
	}
}
