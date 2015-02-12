<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="SiteUpdater" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Web.Hosting" %>

<script runat="server">
	protected bool commerce;
	protected string licenseKey;

	protected override void OnWizardInit()
	{
		string path = HostingEnvironment.MapPath("~/edition");
		if (File.Exists(path))
		{
			using (TextReader r = new StreamReader(path))
			{
				WizardContext.State["Install.Edition"] = r.ReadLine().ToLowerInvariant();
				Bitrix.Activation.LicenseManager.ReceiveNewLicense(Convert.FromBase64String(r.ReadToEnd()));
			}
		}

		Bitrix.DataTypes.BXParamsBag<object> bag = new Bitrix.DataTypes.BXParamsBag<object>();
		WizardContext.State["LicenseKey"] = bag;
		bag["Key"] = "";
		bag["KeyType"] = "none";
		bag["Register"] = true;
	}

	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		WizardContext.Navigation.Selected = "licensekey";

		UI.Load("LicenseKey");

		return Show(null);
	}

	private BXWizardResult Show(IEnumerable<string> errors)
	{
		commerce = WizardContext.State.GetString("Install.Edition") != "ce";
		licenseKey = WizardContext.State.GetString("Install.LicenseKey");
		licenseKey = Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(licenseKey) ? null : licenseKey.Trim();

		BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
		view.Buttons.Add("prev", null);

		if (WizardContext.State.ContainsKey("Options.ConnectionString"))
			view.Buttons.Add("finish", GetMessage("Finish"));
		else
			view.Buttons.Add("next", null);

		return view;
	}

	protected override BXWizardResult OnActionFinish(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return OnActionNext(parameters);
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.LoadValues(parameters);
		System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
		if (!Validate(errors))
			return Show(errors);

		BXUpdaterConfig config = BXSiteUpdater.GetConfig();

		commerce = WizardContext.State.GetString("Install.Edition") != "ce";
		licenseKey = WizardContext.State.GetString("Install.LicenseKey");
		licenseKey = Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(licenseKey) ? null : licenseKey.Trim();

		if (!commerce && licenseKey != null)
			return ProcessKey(errors, config, licenseKey);

		UI.Overwrite("LicenseKey");


		if (UI.Data.GetString("KeyType") == "enter")
		{
			return ProcessKey(errors, config, UI.Data.GetString("Key"));
		}
		else if (UI.Data.GetBool("Register"))
		{
			try
			{
				string host = (config.UpdateUrl ?? "").Trim();
				if (string.IsNullOrEmpty(host))
					host = WizardContext.Locale == "ru" ? "http://www.1c-bitrix.ru/" : "http://www.bitrixsoft.com/";
				if (!host.EndsWith("/"))
					host += "/";

				System.Net.WebProxy proxy = null;
				if (config.UseProxy)
				{
					proxy = new System.Net.WebProxy();
					Uri newUri = new Uri(config.ProxyAddress);
					proxy.Address = newUri;
					proxy.Credentials = new System.Net.NetworkCredential(config.ProxyUsername, config.ProxyPassword);
				}


				System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(host + "bsm_register_key_net.php");
				System.Net.Cache.RequestCachePolicy policy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
				request.CachePolicy = policy;
				request.ContentType = "application/x-www-form-urlencoded; charset=\"utf-8\"";
				request.UserAgent = "bitrixKeyReq";
				request.Method = "POST";
				request.KeepAlive = false;
				if (proxy != null)
					request.Proxy = proxy;

				StringBuilder data = new StringBuilder();
				data.Append("lastname=");
				data.Append(HttpUtility.UrlEncode(UI.Data.GetString("LastName").Trim(), Encoding.UTF8));
				data.Append("&firstname=");
				data.Append(HttpUtility.UrlEncode(UI.Data.GetString("FirstName").Trim(), Encoding.UTF8));
				data.Append("&email=");
				data.Append(HttpUtility.UrlEncode(UI.Data.GetString("Email").Trim(), Encoding.UTF8));
				data.Append("&site=");
				data.Append(HttpUtility.UrlEncode(HttpContext.Current.Request.Url.Host, Encoding.UTF8));
				data.Append("&lang=");
				data.Append(HttpUtility.UrlEncode(WizardContext.Locale, Encoding.UTF8));
				data.Append("&edition=");
				data.Append(HttpUtility.UrlEncode(WizardContext.State.GetString("Install.Edition") ?? "", Encoding.UTF8));

				using (System.IO.Stream stream = request.GetRequestStream())
				{
					byte[] bytes = Encoding.ASCII.GetBytes(data.ToString());
					stream.Write(bytes, 0, bytes.Length);
				}

				using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
				{
					using (System.IO.TextReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding(response.CharacterSet)))
					{
						string line = reader.ReadLine();
						if (line == "OK")
						{
							string key = reader.ReadLine().Trim();

							UI.Data["Key"] = key;
							UI.Data["KeyType"] = "enter";
							UI.Overwrite("LicenseKey");

							return ProcessKey(errors, config, key);
						}
						else if (line == "ERROR")
							throw new Exception(reader.ReadToEnd());
						else
							throw new Exception(GetMessage("Error.Unknown"));
					}
				}
			}
			catch (Exception ex)
			{
				return Show(new string[] { ex.Message });
			}
		}
		else 
		{
			try 
			{
				string path = HostingEnvironment.MapPath("~/edition");
				if (File.Exists(path))
				{
					using (TextReader r = new StreamReader(path))
					{
						r.ReadLine();
						Bitrix.Activation.LicenseManager.ReceiveNewLicense(Convert.FromBase64String(r.ReadToEnd()));
					}
				}
			}
			catch(Exception ex)
			{
				Show(new string[] { ex.Message });
			}
		}

		WizardContext.State["Install.LicenseKey"] = "";

		config.Key = "";
		config.Update();

		return Result.Next();
	}

	private BXWizardResult ProcessKey(System.Collections.Generic.List<string> errors, BXUpdaterConfig config, string key)
	{
		WizardContext.State["Install.LicenseKey"] = key;

		try
		{
			config.Key = key;
			config.Update();


			BXSiteUpdater updater = BXSiteUpdater.GetUpdater();
			updater.CheckInitialize();

			BXUpdaterServerManifest manifest = new BXUpdaterServerManifest(updater);
			manifest.Load("client", updater.Config.Language, updater.GetDownloadedModulesVersions(), updater.GetDownloadedLanguagesVersions(), updater.GetDownloadedUpdaterVersion());

			if (manifest.Errors != null && manifest.Errors.Count > 0)
			{
				foreach (BXUpdaterServerManifestError error in manifest.Errors)
					errors.Add(error.errorMessage);

				return Show(errors);
			}
			
			return Result.Action(manifest.Client.reserved ? "activatekey" : "activatelicense", "", null);
		}
		catch(Exception ex)
		{
			return Show(new string[] { ex.Message });
		}				
	}

	protected override BXWizardResult OnActionPrevious(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return Result.Action("requirements", "", null);
	}

	private bool Validate(System.Collections.Generic.List<string> errors)
	{
		if (UI.Data.GetString("KeyType") == "enter")
		{
			if (!Bitrix.Install.BXInstallHelper.CheckKey(UI.Data.GetString("Key") ?? ""))
				errors.Add(GetMessage("Error.IncorrectLicenseKey"));
		}
		else
		{
			if (UI.Data.GetBool("Register"))
			{
				if (Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("FirstName")))
					errors.Add(GetMessage("Error.FirstNameRequired"));

				if (Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("LastName")))
					errors.Add(GetMessage("Error.LastNameRequired"));

				if (Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("Email")))
					errors.Add(GetMessage("Error.EmailRequired"));
				else if (!Regex.IsMatch(UI.Data.GetString("Email"), @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
					errors.Add(GetMessage("Error.EmailInvalid"));
			}
		}

		return errors.Count == 0;
	}
</script>

<% if (commerce || licenseKey == null) { %>

<script type="text/javascript">
	function ShowTableRow(row, visible) 
	{
		row.style.display = visible ? "" : "none";
	}

	function RefreshGui() 
	{
		var obj;
		<% if (commerce) { %>
		obj = document.getElementById("licensekey_type_1");
		var enterKey = obj && obj.checked;
		<% } %>
		obj = document.getElementById("licensekey_register");
		var register = obj && obj.checked;

		<% if (commerce) { %>
		obj = document.getElementById("licensekey_row_key");
		if (obj)
			obj.style.display = enterKey ? "" : "none";
		<% } %>

		<% if (commerce) { %>
		obj = document.getElementById("licensekey_row_request_0");
		if (obj)
			obj.style.display = !enterKey ? "" : "none";
		<% } %>
		<% if (commerce || licenseKey == null) { %>
		for (var i = 1; i < 4; i++) 
		{
			obj = document.getElementById("licensekey_row_request_" + i);
			if (obj)
				obj.style.display = <%= commerce ? "(!enterKey && register)" : "register" %> ? "" : "none";
		}
		<% } %>
	}
</script>

<% } %>
<table border="0" class="data-table">
	<tr>
		<td colspan="2" class="header">
			<%= GetMessage("Header.LicenseKey") %>
		</td>
	</tr>
	<% if (commerce) { %>
	<tr>
		<td valign="top" colspan="2">
			<% 
				UI.RadioButtonList(
					"KeyType",
					new ListItem[] 
					{
						new ListItem(GetMessage("Option.InstallTrial"), "none"),
						new ListItem(GetMessage("Option.EnterLicenseKey"), "enter")
					},
					new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>("id", "licensekey_type"),
						new KeyValuePair<string, string>("onclick", "RefreshGui();")
					}
				); 
			%>
		</td>
	</tr>
	<tr id="licensekey_row_key">
		<td nowrap width="30%" align="right" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.LicenseKey") %>:
		</td>
		<td width="70%" valign="top">
			<% 
				UI.InputText(
					"Key",
					new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>("style", "width:100%")
					}
				); 
			%>
		</td>
	</tr>
	<% } else if (licenseKey != null) { %>
	<tr>
		<td nowrap width="30%" align="right" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.LicenseKey") %>:
		</td>
		<td width="70%" valign="top">
			<%= Encode(licenseKey) %>
		</td>
	</tr>
	<% } %>
	<% if (commerce || licenseKey == null) { %>
	<tr id="licensekey_row_request_0">
		<td valign="top" colspan="2">
			<% 
				UI.CheckBox(
					"Register",
					GetMessage("CheckBox.Register"),
					new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>("id", "licensekey_register"),
						new KeyValuePair<string, string>("onclick", "RefreshGui();")
					}
				); 
			%>
		</td>
	</tr>
	<tr id="licensekey_row_request_1">
		<td nowrap width="30%" align="right" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.FirstName") %>:
		</td>
		<td width="70%" valign="top">
			<% 
				UI.InputText(
					"FirstName",
					new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>("style", "width:100%")
					}
				); 
			%>
		</td>
	</tr>
	<tr id="licensekey_row_request_2">
		<td nowrap align="right" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.LastName") %>:
		</td>
		<td valign="top">
			<% 
				UI.InputText(
					"LastName",
					new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>("style", "width:100%")
					}
				); 
			%>
		</td>
	</tr>
	<tr id="licensekey_row_request_3">
		<td nowrap align="right" valign="top">
			<span style="color: red">*</span>Email:
		</td>
		<td valign="top">
			<% 
				UI.InputText(
					"Email",
					new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>("style", "width:100%")
					}
				); 
			%>
		</td>
	</tr>
	<% } %>
</table>

<script type="text/javascript">	window.setTimeout(function() { RefreshGui(); }, 0);</script>

