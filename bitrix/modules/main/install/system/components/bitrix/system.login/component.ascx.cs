using System;
using System.Collections.Generic;
using System.Web;
using Bitrix.Components;
using Bitrix.Security;
using Bitrix.UI;
using Bitrix.DataTypes;
using Bitrix;
using Bitrix.Services;
using Bitrix.OpenId;
using Bitrix.Services.Text;
using Bitrix.OpenId.PlugIns.Extensions;
using Bitrix.Components.Editor;
using System.Collections.Specialized;
using WindowsLive;
using Bitrix.Configuration;

namespace Bitrix.Main.Components
{
	public partial class SystemLoginComponent : BXComponent
	{
		string login;

		List<string> errors;

		public string ProfilePath
		{
			get { return BXUri.ToRelativeUri(Parameters.Get<string>("ProfilePath", string.Empty)); }
		}

		string profilePathResolved;
		public string ProfilePathResolved
		{
			get
			{
				if (profilePathResolved != null)
					return profilePathResolved;

				var replace = new BXParamsBag<object>();
				replace["UserId"] = (BXPrincipal.Current.Identity as BXIdentity).Id;

				return profilePathResolved = ResolveTemplateUrl(Parameters.Get<string>("ProfilePath", string.Empty), replace);
			}
		}

		private string ReplaceParameters(string src, IDictionary<string, string> paramDic)
		{
			if (string.IsNullOrEmpty(src))
				return string.Empty;

			int paramCount = paramDic != null ? paramDic.Keys.Count : 0;
			if (paramDic == null || paramDic.Keys.Count == 0)
				return src;

			string result = src;

			foreach (string key in paramDic.Keys)
			{
				if (string.IsNullOrEmpty(key))
					continue;

				string paramVal = paramDic[key];
				if (paramVal == null)
					paramVal = string.Empty;
				int index = -1;
				while ((index = result.IndexOf(key, 0, StringComparison.InvariantCultureIgnoreCase)) >= 0)
					result = string.Concat(result.Substring(0, index), paramVal, result.Substring(index + key.Length));
			}
			return result;
		}

		public string LoginRedirectPath
		{
			get
			{
				string result = Parameters.Get<string>("LoginRedirectPath", string.Empty);
				return !string.IsNullOrEmpty(result) ? BXUri.ToRelativeUri(result) : string.Empty; 
			}
		}

		public string PasswordRecoveryPath
		{
			get { return BXUri.ToRelativeUri(Parameters.Get<string>("PasswordRecoveryPath", string.Empty)); }
		}

		public string PasswordRecoveryCodePath
		{
			get { return BXUri.ToRelativeUri(Parameters.Get<string>("PasswordRecoveryCodePath", string.Empty)); }
		}

		public bool ShowRegistrationLink
		{
			get
			{
				return AllowRegistration || Component == null;  
			}
		}

		public string RegisterPath
		{
			get 
			{
				string r = Parameters.Get<string>("RegisterPath", string.Empty);
				if (!string.IsNullOrEmpty(r))
				{
					Dictionary<string, string> paramDic = new Dictionary<string, string>();
					paramDic.Add("#ReturnUrl#", HttpUtility.UrlEncode(GetReturnUrl()));
					r = ReplaceParameters(r, paramDic);
					r = BXUri.ToRelativeUri(r);
				}
				return r;
			}
		}

		public string UrlToConfirmationPage
		{
			get
			{
				return Parameters.GetString("UrlToConfirmationPage", String.Empty);
			}
		}

		public bool RequiresQuestionAndAnswer
		{
			get { return BXUserManager.Provider.RequiresQuestionAndAnswer; }
		}

		public bool RequiresCheckWord
		{
			get { return BXUserManager.Provider.RequiresCheckWord; }
		}

		public bool EnablePasswordReset
		{
			get { return BXUserManager.Provider.EnablePasswordReset; }
		}

		OpenIdClient openId;

		public OpenIdClient OpenId
		{
			get
			{
				return openId ?? ( openId = new OpenIdClient());
			}
		}
		List<string> registerCustomFields;
		List<string> RegisterCustomFields
		{
			get
			{
				return registerCustomFields ?? (registerCustomFields = Parameters.GetListString("EditFields"));
			}
		}

		public IList<string> Errors
		{
			get { return errors ?? (errors = new List<string>()); }
		}

		//определим, можно ли регистрировать пользователя - параметры пришли от комплексного компонента, нет обязательных полей на форме регистрации, 
		//которые мы не можем заполнить. 
		bool HaveRequiredRegistrationInfo(string email,string nickName)
		{
			

			if (BXStringUtility.IsNullOrTrimEmpty(email))
				return false;

			//если параметры о необходимых полях отсутствуют, то не имеем права регистрировать пользователя, вернем false

			string firstNameFieldMode = Parameters.GetString("FirstNameFieldMode", "");
			string lastNameFieldMode = Parameters.GetString("LastNameFieldMode", "");
			string displayNameFieldMode = Parameters.GetString("DisplayNameFieldMode", "");

			if (String.IsNullOrEmpty(firstNameFieldMode) || String.IsNullOrEmpty(lastNameFieldMode) ||
				String.IsNullOrEmpty(displayNameFieldMode))
				return false;

			BXCustomField field;
			var fields = BXCustomEntityManager.GetFields(BXUser.GetCustomFieldsKey());
			// если есть обязательные поля которые мы не можем заполнить через SimpleRegistration, вернем false
			if (Parameters.GetString("FirstNameFieldMode","").Equals("require", StringComparison.OrdinalIgnoreCase) ||
				 Parameters.GetString("LastNameFieldMode","").Equals("require", StringComparison.OrdinalIgnoreCase) ||
				(Parameters.GetString("DisplayNameFieldMode","").Equals("require", StringComparison.OrdinalIgnoreCase)
				&& BXStringUtility.IsNullOrTrimEmpty(nickName)))

				return false;

			foreach (string fieldCode in RegisterCustomFields)
			{
				if (fieldCode == null || !fieldCode.StartsWith("PROPERTY")) continue;
				field = fields.Find(x => x.CorrectedName.Equals(fieldCode.Substring(9), StringComparison.OrdinalIgnoreCase));
				if (field != null && field.Mandatory) return false;
			}

			return true;
		}

		string email;

		public string UserEmail
		{
			get { return email; }
			set { email = value; }
		}

		protected void OpenIdValidationSucceeded(object sender, EventArgs e)
		{
			
			OpenIdClient client = sender as OpenIdClient;

			SimpleRegistration sr = new SimpleRegistration(client);
			sr.AddOptionalFields(SimpleRegistrationFields.Nickname, SimpleRegistrationFields.Email);

			OpenIdUser user = client != null ? client.RetrieveUser() : null;
			if (user == null || BXStringUtility.IsNullOrTrimEmpty(user.Identity))
			{
				//LoginErrorMessage.AddErrorMessage("К сожалению, не удалось выполнить вход на сайт.<br />Попробуйте повторить попытку заново.");
				return;
			}

			string email = user.GetValue(SimpleRegistrationFields.Email);
			string nickName = user.GetValue(SimpleRegistrationFields.Nickname);

			Dictionary<string, string> param = new Dictionary<string, string>();
			param["login"] = user.Identity;
			param["email"] = email;
			param["nickname"] = nickName;
			param["authtype"] = "OpenId";

			Session["ExternalAuth"] = param;
			BXUser bUser = null;
			try
			{
				bUser = BXUser.GetByOpenID(user.Identity);
			}
			catch (Exception ex)
			{
				Errors.Add(GetMessage("Error.OpenIdFieldNotExists"));
				return;
			}
			if (bUser != null)
			{
				try
				{
					Session.Remove("ExternalAuth");
					BXAuthentication.SetAuthCookie(bUser.UserName, bUser.ProviderName, true);
					Response.Redirect(GetReturnUrl(), true);
					return;
				}
				catch (Exception ex)
				{

					return;
				}
			} // если пользователь не найден, его нужно зарегистрировать, либо направить на страницу регистрации
			else if (TryRegisterNewExternalUser && HaveRequiredRegistrationInfo(email,nickName))
			{
				if (String.IsNullOrEmpty(UrlToConfirmationPage) && SendConfirmationRequest)
				{
					Errors.Add(GetMessage("Error.UrlToConfirmationPageIsEmpty"));
					return;
				}
				string error = string.Empty;
				bUser = BXUser.RegisterOpenIdUser(user.Identity, user.Identity, email, null, null, nickName, 
					!SendConfirmationRequest, SendConfirmationRequest ? ActivationToken : String.Empty, out error);

				try
				{
					bUser.Save();
				}
				catch (Exception ex)
				{
					Errors.Add(ex.Message);
				}

				if (String.IsNullOrEmpty(error) && !SendConfirmationRequest) // пользователь удачно зарегистрирован, авторизуем его
				{
					Session.Remove("ExternalAuth");
					BXAuthentication.SetAuthCookie(bUser.UserName, bUser.ProviderName, true);
					string returnUrl = GetReturnUrl();
					if (!BXStringUtility.IsNullOrTrimEmpty(returnUrl))
						Response.Redirect(returnUrl);
					string path = LoginRedirectPath;
					if (!BXStringUtility.IsNullOrTrimEmpty(path))
						Response.Redirect(path, true);
					else
					{
						BXPage page = Page as BXPage;
						if (page != null)
							page.Redirect(BXSefUrlManager.CurrentUrl.ToString());
						else
						{
							try { Response.Redirect(BXSefUrlManager.CurrentUrl.ToString(), true); }
							catch (System.Threading.ThreadAbortException /*exp*/ ) { }
						}
					}
				}
				else if (String.IsNullOrEmpty(error))
				{

					Bitrix.Main.Components.SystemRegisterComponent.SendEmailConfirmationRequest(bUser, UrlToConfirmationPage);
					UserEmail = email;
				}
				else
				{
					Errors.Add(error);
				}
				return;
			}
			//зарегистрировать на удалось, перенаправим на страницу регистрации
			if (BXStringUtility.IsNullOrTrimEmpty(RegisterPath))
			{
				Errors.Add(GetMessage("Error.RegisterPathIsEmpty"));
				return;
			}

			Response.Redirect(RegisterPath);
				

		}

		protected void OpenIdValidationFailed(object sender, EventArgs e)
		{
			Errors.Add(GetMessage("Error.OpenIdAuthFailed"));
		}

		protected void OpenIdReceivedCancelled(object sender, EventArgs e)
		{
			Errors.Add(GetMessage("Error.OpenIdAuthFailed"));
		}

		public static bool IsLiveIdPost(NameValueCollection form)
		{
			return !String.IsNullOrEmpty(form["stoken"]);
		}

		public static string GetTokenFromContext(string context)
		{
			if (String.IsNullOrEmpty(context))
				return null;
			var index = context.IndexOf(BXCsrfToken.TokenKey+":");
			if (index < 0) return null;
			var str = context.Substring(index + 18);
			if (str.IndexOf(";") >= 0)
				str = str.Substring(0, str.IndexOf(";"));
			return str;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			var page = Page as BXPublicPage;
			if (page!=null )
			if ( IsLiveIdPost(Request.Form)){
				var token = GetTokenFromContext(Request.Form["appctx"]);
				page.CsrfTokenToValidate = token;
			}	
		}

		public bool AllowRegistration
		{
			get 
			{
				return Parameters.GetBool("RegistrationAllow", false);
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Results["CacheMode"] = "None";
			Parameters["CacheMode"] = "None";
			string componentId = Request.QueryString["BXOpenIdAuth_ComponentID"];
			string context =  Request.Form["appctx"];
			string componentIdFromLiveID = null;
			string redirectUrl = null;
			if ( !String.IsNullOrEmpty(context))
			{
				List<String> contextValues = BXStringUtility.CsvToList(context);
				if ( contextValues.Count > 1 )
					redirectUrl = contextValues[1];
				if ( contextValues.Count > 0 )
					componentIdFromLiveID = contextValues[0];
			}

			if (UseOpenIdAuth && componentId == ClientID && !BXPrincipal.Current.Identity.IsAuthenticated)
			{
				Session.Remove("LiveIdAuthToken");
				openId = new OpenIdClient();
				openId.ValidationSucceeded += new EventHandler(OpenIdValidationSucceeded);
				openId.ValidationFailed += new EventHandler(OpenIdValidationFailed);
				openId.ReceivedCancel += new EventHandler(OpenIdReceivedCancelled);
				openId.DetectAndHandleResponse();
			}

			if (UseLiveIdAuth && (componentIdFromLiveID == ClientID || !String.IsNullOrEmpty(redirectUrl)))
			{
				WindowsLiveLogin liveLogin = null;
				try
				{
					liveLogin =
						new WindowsLiveLogin(
							BXConfigurationUtility.Options.LiveIDApplicationID,
							BXConfigurationUtility.Options.LiveIDSecretKey);
				}
				catch (Exception ex)
				{
					Errors.Add(GetMessage("Error.InvalidLiveIDParameters"));
					IncludeComponentTemplate();
					return;
				}

				WindowsLiveLogin.User u = liveLogin.ProcessToken(Request.Form["stoken"]);
				
				if (u != null && !String.IsNullOrEmpty(u.Id) )
				{
					var index = redirectUrl.IndexOf("?");
					var redirectUrlPath = index >= 0 ? redirectUrl.Substring(0,index) : redirectUrl;
					if (!String.IsNullOrEmpty(redirectUrl) && !redirectUrlPath.Equals(BXSefUrlManager.CurrentUrl.AbsoluteUri.ToString(), StringComparison.OrdinalIgnoreCase)) // we come from somewhere, will redirect back
						{
							try
							{
								Session["linkToLiveId"] = Request.Form["stoken"];
								Response.Redirect(redirectUrl);

							}
							catch (Exception ex)
							{
							}
						}
					BXUser user = null;
					Session.Remove("ExternalAuth");
					try
					{
						user = BXUser.GetByLiveID(u.Id);
					}
					catch (Exception ex)
					{
						Errors.Add(GetMessage("Error.LiveIdFieldNotExists"));
						IncludeComponentTemplate();
						return;
					}
					Session.Remove("ExternalAuth");
					if (user == null)
					{
						Session["LiveIdAuthToken"] = u.Id;
						Response.Redirect(RegisterPath);
					}
					else
					{
						BXAuthentication.SetAuthCookie(user.UserName, user.ProviderName, true);
						Response.Redirect(BXSefUrlManager.CurrentUrl.AbsoluteUri, true);
					}
				}
				else
				{
					Errors.Add(GetMessage("Error.LiveIdAuthFailed"));
				}
			}

			IncludeComponentTemplate();
		}

		public bool UseCaptcha
		{
			get
			{
				return Parameters.GetBool("UseCaptcha", false);
			}
		}

		bool useOpenIdAuth = BXConfigurationUtility.Options.EnableOpenId;

		public bool UseOpenIdAuth
		{
			get
			{
				return useOpenIdAuth;
			}
			set
			{
				useOpenIdAuth = value;
			}
		}

		bool useLiveIdAuth = BXConfigurationUtility.Options.EnableLiveId;

		public bool UseLiveIdAuth
		{
			get
			{
				return useLiveIdAuth;
			}
			set
			{
				useLiveIdAuth = value;
			}
		}

		public bool TryRegisterNewExternalUser
		{
			get
			{
				return Parameters.GetBool("TryRegisterNewExternalUser", false);
			}
			set
			{
				Parameters["TryRegisterNewExternalUser"] = value.ToString();
			}
		}

		protected bool ProcessOpenIdRequest(string identifier, List<string> commandErrors)
		{
			var cl = new OpenIdClient();

			cl.Identity = identifier.Replace(" ","");
			Uri uri =/* new Uri( MakeClearUrl( */BXSefUrlManager.CurrentUrl/*,new string[]{} ))*/;
			NameValueCollection queryParams = HttpUtility.ParseQueryString(StripOpenIdParams(uri.Query));
			queryParams.Remove("BXOpenIdAuth_ComponentID");
			queryParams.Add("BXOpenIdAuth_ComponentID", ClientID);
			queryParams.Remove(BXCsrfToken.TokenKey);
			queryParams.Add(BXCsrfToken.TokenKey, BXCsrfToken.GenerateToken());
			UriBuilder ub = new UriBuilder(BXSefUrlManager.CurrentUrl);
			ub.Query = queryParams.ToString();

			cl.ReturnUrl = ub.Uri;

			
			if ( commandErrors == null ) 
				commandErrors = new List<string>();
			bool success = true;
			try
			{
				SimpleRegistration sr = new SimpleRegistration(cl.StateContainer);
				sr.AddOptionalFields(SimpleRegistrationFields.Nickname,SimpleRegistrationFields.Email);
				cl.CreateRequest(false, true);

				if ((cl.ErrorState & ErrorCondition.NoServersFound) == ErrorCondition.NoServersFound){
					
					commandErrors.Add(GetMessage("Error.OpenIdProviderNotFound"));
					success = false;
				}
				else if ((cl.ErrorState & ErrorCondition.NoErrors) != ErrorCondition.NoErrors){
					commandErrors.Add(GetMessage("Error.OpenIdAuthFailed"));
					success = false;
				}
			}
			catch (Exception ex)
			{
				commandErrors.Add(GetMessage("Error.OpenIdGeneral"));
				success = false;
			}
			return success;
		}

		public int ShowCaptchaAfterFailedAttempts
		{
			get
			{
				return Parameters.GetInt("ShowCaptchaAfterFailedAttempts", 3);
			}
		}

		string StripOpenIdParams(string query)
		{
			NameValueCollection col = HttpUtility.ParseQueryString(query);
			foreach (var param in OpenIdClient.OpenIdQueryParams)
				col.Remove(param);
			return col.ToString();
		}

		string MakeClearUrl(Uri uri,string [] removeQueryParams)
		{
			NameValueCollection col = HttpUtility.ParseQueryString(uri.Query);
			foreach (var param in removeQueryParams)
				col.Remove(param);
			foreach (var param in OpenIdClient.OpenIdQueryParams)
				col.Remove(param);
			return uri.AbsolutePath + (col.Count > 0 ? "?" + col.ToString() : "");
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessage("Title");
			Description = GetMessage("Description");
			Icon = "images/user_authform.gif";

			Group = new BXComponentGroup("Auth", GetMessage("Group"), 100, BXComponentGroup.Utility);

			ParamsDefinition.Add(
				"PasswordRecoveryPath",
				new BXParamText(
					GetMessageRaw("PathToPasswordRecoveryScript"),
					String.Empty,
					BXCategory.Main
					)
				);

			ParamsDefinition.Add(
				"PasswordRecoveryCodePath",
				new BXParamText(
					GetMessageRaw("PathToControlWordScript"),
					String.Empty,
					BXCategory.Main
					)
				);

			ParamsDefinition.Add(
				"LoginRedirectPath",
				new BXParamText(
					GetMessageRaw("PathToSuccessfulLoginRedirectionScript"),
					String.Empty,
					BXCategory.Main
					)
				);

			ParamsDefinition.Add(
				"ProfilePath",
				new BXParamText(
					GetMessageRaw("PathToUserProfile"),
					String.Empty,
					BXCategory.Main
					)
				);

			ParamsDefinition.Add(
				"RegisterPath",
				new BXParamText(
					GetMessageRaw("PathToUserRegistrationScript"),
					String.Empty,
					BXCategory.Main
					)
				);

			ParamsDefinition.Add(
				"UrlToConfirmationPage",
				new BXParamText(
					GetMessageRaw("UrlToConfirmationPage"),
					String.Empty,
					BXCategory.Main
					)
				);

			ParamsDefinition.Add(
				"UseCaptcha",
				new BXParamYesNo(
					GetMessageRaw("UseCaptcha"),
					false,
					BXCategory.Main
				)
			);
		}

		public bool SendConfirmationRequest
		{
			get
			{
				return BXConfigurationUtility.Options.SendConfirmationRequest;
			}
		}

		string activationToken;

		public string ActivationToken
		{
			get
			{
				return activationToken ?? (activationToken = BXStringUtility.GenerateUniqueString(15));
			}
		}

		private string GetReturnUrl()
		{
			HttpContext current = HttpContext.Current;
			string str = current.Request.QueryString["ReturnUrl"];
			if (str == null)
			{
				str = current.Request.Form["ReturnUrl"];
				if ((!string.IsNullOrEmpty(str) && !str.Contains("/")) && str.Contains("%"))
				{
					str = HttpUtility.UrlDecode(str);
				}
			}
			if (str == null)
			{
				if (!String.IsNullOrEmpty(this.LoginRedirectPath))
					return this.LoginRedirectPath;
				else
					return current.Request.RawUrl;
			}
			return str;
		}

		public override bool ProcessCommand(string commandName, BXParamsBag<object> commandParameters,
											List<string> commandErrors)
		{
			if (commandName.Equals("login", StringComparison.InvariantCultureIgnoreCase))
			{
				string providerName = null;

				login = (string)commandParameters["Login"];
				string val1 = (string)commandParameters["Password"];
				bool remember = (bool)commandParameters["Remember"];

				try
				{
					if (BXAuthentication.Authenticate(login, val1, out providerName))
					{
						BXAuthentication.SetAuthCookie(login, providerName, remember);
						Response.Redirect(GetReturnUrl(), true);
					}
					else
					{
						commandErrors.Add(GetMessage("Error.InvalidLoginOrPassword"));
					}
				}
				catch (Exception e)
				{
					commandErrors.Add(e.Message);
				}
			}
			if (commandName.Equals("logout", StringComparison.InvariantCultureIgnoreCase))
			{
				BXAuthentication.SignOut();
				Session.Remove("ExternalAuth");
				#region old
				//Response.Clear();
				//Response.StatusCode = 200;
				//if ((Page.Form == null) || !string.Equals(Page.Form.Method, "get", StringComparison.OrdinalIgnoreCase))
				//    Response.Redirect(Request.RawUrl, true );
				//else
				//    Response.Redirect(Request.Path, true);
				#endregion
				//перегрузка страницы, инициирует window.location.href = "http://website/default.aspx"; Абсолютный URL гарантирует новый запрос страницы.
				BXPage page = Page as BXPage;
				if (page != null)
					page.Redirect(MakeClearUrl(BXSefUrlManager.CurrentUrl, new string[] { "BXOpenIdAuth_ComponentID",BXCsrfToken.TokenKey }));
				else
				{
					try { Response.Redirect(MakeClearUrl(BXSefUrlManager.CurrentUrl, new string[] { "BXOpenIdAuth_ComponentID" }), true); }
					catch (System.Threading.ThreadAbortException /*exp*/ ) { }
				}
			}
			if (commandName.Equals("openidLogin", StringComparison.InvariantCultureIgnoreCase))
			{
				string openIdLogin = (string)commandParameters["OpenIdLogin"];
				return ProcessOpenIdRequest(openIdLogin, commandErrors);
			}
			if (commandName.Equals("LiveIdLogin", StringComparison.OrdinalIgnoreCase))
			{
				var appId = BXConfigurationUtility.Options.LiveIDApplicationID;
				var key = BXConfigurationUtility.Options.LiveIDSecretKey;
				if (BXStringUtility.IsNullOrTrimEmpty(key) || BXStringUtility.IsNullOrTrimEmpty(appId))
				{
					commandErrors.Add(GetMessage("Error.EmptyLiveIdAppIdOrKey"));
					return false;
				}
				WindowsLiveLogin liveLogin = null;
				try
				{
					liveLogin = new WindowsLiveLogin(
									BXConfigurationUtility.Options.LiveIDApplicationID,
									BXConfigurationUtility.Options.LiveIDSecretKey);
				}
				catch (Exception ex)
				{
					commandErrors.Add(GetMessage("Error.InvalidLiveIDParameters"));
					return false;
				}

				var context = ClientID + ";" + BXSefUrlManager.CurrentUrl.ToString() + ";" + BXCsrfToken.TokenKey+":"+BXCsrfToken.GenerateToken();
				Response.Redirect(liveLogin.GetLoginUrl(context));
			}
			return false;
		}

	
		[Flags]
		public enum ErrorCode
		{
			None = 0,
			OpenIdError = 1,
			InvalidLoginOrPassword,
			OpenIdProviderNotFound,
			OpenIdGeneral,
			OpenIdAuthFailed
		}

	}

	public class SystemLoginTemplate : BXComponentTemplate<SystemLoginComponent> {};
}

#region Compatibility
[Obsolete("Use Bitrix.Main.Components.SystemLoginComponent class")]
public partial class LoginComponent : Bitrix.Main.Components.SystemLoginComponent { }
#endregion