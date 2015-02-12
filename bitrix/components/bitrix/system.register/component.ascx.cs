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
using Bitrix.Components;
using Bitrix.Security;
using System.Collections.Generic;
using System.Configuration.Provider;
using Bitrix.Modules;
using Bitrix.UI;

using Bitrix.Services;
using Bitrix;
using Bitrix.DataTypes;
using Bitrix.Configuration;
using System.Threading;
using Bitrix.Components.Editor;
using System.Text;
using Bitrix.IO;
using System.Collections.Specialized;
using Bitrix.Services.Text;
using System.Web.Hosting;
using Bitrix.DataLayer;

namespace Bitrix.Main.Components
{
	public partial class SystemRegisterComponent : BXComponent
	{
		string activationToken;
		BXUser user;

		public string UserActivationToken
		{
			get
			{
				return user == null ? null : user.ActivationToken;
			}
		}

		public string UserEmail
		{
			get
			{
				return user == null ? null : user.Email;
			}
		}

		public bool RequiresQuestionAndAnswer
		{
			get { return BXUserManager.Provider.RequiresQuestionAndAnswer; }
		}

		public List<string> EditFields
		{
			get { return Parameters.GetListString("EditFields"); }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Results["CacheMode"] = "None";


			IncludeComponentTemplate();
			bool fileFieldExistsInForm = false;

			foreach (string fieldId in EditFields)
			{
					
					if (!UserCustomFields.ContainsKey(fieldId))
						continue;

					if (!fileFieldExistsInForm && UserCustomFields[fieldId].CustomType != null && UserCustomFields[fieldId].CustomType.IsFile)
						fileFieldExistsInForm = true;
					//BXCustomFieldWrapper field = UserCustomFields[propertyId];
				
			}

			HtmlForm form;
			if (Page != null && (form = Page.Form) != null && form.Enctype.Length == 0 && fileFieldExistsInForm)
				form.Enctype = "multipart/form-data";
		}

		Dictionary<string, string> externalAuthFields;

		public Dictionary<string, string> OpenIdAuthFields
		{
			get
			{ 
				if (externalAuthFields != null)
					return externalAuthFields;
				if (Session["ExternalAuth"] != null)
					return externalAuthFields = Session["ExternalAuth"] as Dictionary<string, string>;
				return externalAuthFields = new Dictionary<string, string>();
			}
		}

		public string LiveIdAuthToken
		{
			get { return (string)Session["LiveIdAuthToken"]; }
		}


		protected List<BXCustomField> EditFieldsList = new List<BXCustomField>();

		public string GetRedirectUrl()
		{
			string redirectUrl = Request.QueryString["ReturnUrl"];
			if (string.IsNullOrEmpty(redirectUrl))
				redirectUrl = Request.QueryString[BXConfigurationUtility.Constants.BackUrl];
			if (user != null && string.IsNullOrEmpty(redirectUrl))
			{
				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace["UserId"] = user.UserId;
				replace["UserName"] = user.UserName;
				replace["Email"] = user.Email;
				redirectUrl = ResolveTemplateUrl(Parameters.Get<string>("RedirectUrl"),replace);
			}
			else
			if (string.IsNullOrEmpty(redirectUrl))
				redirectUrl = Parameters.Get<string>("RedirectUrl");
			if (string.IsNullOrEmpty(redirectUrl))
				redirectUrl = "~/";
			return redirectUrl;
		}

		public FieldMode FirstNameFieldMode
		{
			get
			{
				return GetFieldMode(Parameters.GetString("FirstNameFieldMode"));
			}
		}
		public FieldMode LastNameFieldMode
		{
			get
			{
				return GetFieldMode(Parameters.GetString("LastNameFieldMode"));
			}
		}
		public FieldMode DisplayNameFieldMode
		{
			get
			{
				return GetFieldMode(Parameters.GetString("DisplayNameFieldMode"));
			}
		}

		public string UrlToConfirmationPage
		{
			get
			{
				return Parameters.GetString("UrlToConfirmationPage", String.Empty);
			}
		}

		public bool SendConfirmationRequest
		{
			get
			{
				return BXConfigurationUtility.Options.SendConfirmationRequest;
			}
		}

		public string ActivationToken
		{
			get
			{
				return activationToken ?? (activationToken = BXStringUtility.GenerateUniqueString(15));
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


		protected override void PreLoadComponentDefinition()
		{
			base.Title = GetMessage("Component.Title");
			base.Description = GetMessage("Component.Description");
			base.Icon = "images/user_authform.gif";

			Group = new BXComponentGroup("Auth", GetMessage("User"), 100, BXComponentGroup.Utility);

			BXCategory fields = new BXCategory(GetMessageRaw("FieldSettings"), "fields", 200);

			ParamsDefinition.Add(
				"AddToRoles",
				new BXParamMultiSelection(
					GetMessageRaw("AddToRoles"),
					"",
					BXCategory.Main
					)
				);

			ParamsDefinition.Add(
				"DoAuthentication",
				new BXParamYesNo(
					GetMessageRaw("DoAuthentication"),
					false,
					BXCategory.Main
					)
				);

			ParamsDefinition.Add(
				"RedirectUrl",
				new BXParamText(
					GetMessageRaw("RedirectUrl"),
					string.Empty,
					BXCategory.Main
					)
				);

			ParamsDefinition.Add(
				"UrlToConfirmationPage",
				new BXParamText(
					GetMessageRaw("UrlToConfirmationPage"),
					"~/confirmation.aspx",
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

			ParamsDefinition.Add(
				"FirstNameFieldMode",
				new BXParamSingleSelection(
					GetMessageRaw("FirstNameFieldMode"),
					"require",
					fields
					)
				);

			ParamsDefinition.Add(
				"LastNameFieldMode",
				new BXParamSingleSelection(
					GetMessageRaw("LastNameFieldMode"),
					"require",
					fields
					)
				);

			ParamsDefinition.Add(
				"DisplayNameFieldMode",
				new BXParamSingleSelection(
					GetMessageRaw("DisplayNameFieldMode"),
					"hide",
					fields
					)
				);

			ParamsDefinition.Add(
			"EditFields",
			new BXParamDoubleList(
				GetMessageRaw("EditFields"),
				"Name",
				BXCategory.Main
			)
			);



		}

		List<string> addToRoles;

		IList<string> AddToRoles
		{
			get {
				if (addToRoles != null)
					return addToRoles;
				return addToRoles = Parameters.GetListString("AddToRoles"); 
			}
		}

		Dictionary<string, SystemRegisterCustomFieldWrapper> userCustomFields;

		public Dictionary<string, SystemRegisterCustomFieldWrapper> UserCustomFields
		{
			get
			{
				if (userCustomFields != null)
					return userCustomFields;

				BXCustomFieldCollection col = BXCustomEntityManager.GetFields(BXUser.GetCustomFieldsKey());
				userCustomFields = new Dictionary<string, SystemRegisterCustomFieldWrapper>();
				foreach (BXCustomField field in col)
					userCustomFields["PROPERTY_" + field.CorrectedName] = new SystemRegisterCustomFieldWrapper(field, ClientID + "$_fld_" + field.CorrectedName, UniqueID + "$_fld_" + field.CorrectedName);
				return userCustomFields;
			}
		}

		protected override void LoadComponentDefinition()
		{
			ParamsDefinition["FirstNameFieldMode"].Values = new List<BXParamValue>(new BXParamValue[] {
				new BXParamValue(GetMessageRaw("Field.Hide"), "hide"),
				new BXParamValue(GetMessageRaw("Field.Show"), "show"),
				new BXParamValue(GetMessageRaw("Field.Require"), "require")
		});
			ParamsDefinition["LastNameFieldMode"].Values = new List<BXParamValue>(new BXParamValue[] {
				new BXParamValue(GetMessageRaw("Field.Hide"), "hide"),
				new BXParamValue(GetMessageRaw("Field.Show"), "show"),
				new BXParamValue(GetMessageRaw("Field.Require"), "require")
		});
			ParamsDefinition["DisplayNameFieldMode"].Values = new List<BXParamValue>(new BXParamValue[] {
				new BXParamValue(GetMessageRaw("Field.Hide"), "hide"),
				new BXParamValue(GetMessageRaw("Field.Show"), "show"),
				new BXParamValue(GetMessageRaw("Field.Require"), "require")
		});


			//ParamsDefinition["SendConfirmationRequest"].ClientSideAction =
				//new ParamClientSideActionGroupViewSwitch(ClientID, "SendConfirmationRequest", "SendConfirmationRequest", "DoNotSendConfirmationRequest");

			//ParamsDefinition["UrlToConfirmationPage"].ClientSideAction =
			//    new ParamClientSideActionGroupViewMember(ClientID, "UrlToConfirmationPage", new string[] { "SendConfirmationRequest" });

			List<BXParamValue> fields = new List<BXParamValue>();

			foreach (var customField in UserCustomFields.Values)
			{
					string title = BXTextEncoder.HtmlTextEncoder.Decode(customField.Field.EditFormLabel);
					string code = customField.Field.Name.ToUpper();
					fields.Add(new BXParamValue(title, "PROPERTY_" + code));
			}
			ParamsDefinition["EditFields"].Values = fields;

			List<BXParamValue> addToRoles = ParamsDefinition["AddToRoles"].Values;
			if (addToRoles.Count > 0)
				addToRoles.Clear();

			foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), 
				new BXOrderBy_old("RoleName", "Asc")))
			{
				BXParamValue v = new BXParamValue(r.Title, r.RoleName);
				addToRoles.Add(v);
			}
			ParamsDefinition["AddToRoles"].Values = addToRoles;

		}

		public bool DoAuthentication
		{
			get { return Parameters.GetBool("DoAuthentication", false); }
		}

		private string GetErrorMessage(MembershipCreateStatus status)
		{
			switch (status)
			{
				case MembershipCreateStatus.DuplicateUserName:
					return GetMessage("Error.UserWithLoginYouSpecifiedAlreadyExists");

				case MembershipCreateStatus.DuplicateEmail:
					return GetMessage("Error.UserWithEmailYouSpecifiedAlreadyExists");

				case MembershipCreateStatus.InvalidPassword:
					return GetMessage("Error.PasswordContraveneSiteRules") +
						(BXUserManager.Provider.MinRequiredPasswordLength > 0 ? String.Format(GetMessage("Error.FormatPasswordMustNotBeShorterThan"), BXUserManager.Provider.MinRequiredPasswordLength) : "") +
						(BXUserManager.Provider.MinRequiredNonAlphanumericCharacters > 0 ? String.Format(GetMessage("Error.FormatPasswordMustContain"), BXUserManager.Provider.MinRequiredNonAlphanumericCharacters) : "");

				case MembershipCreateStatus.InvalidEmail:
					return GetMessage("Error.EmailYouSpecifiedIsInvalid");

				case MembershipCreateStatus.InvalidAnswer:
					return GetMessage("Error.AnswerForSecretQuestionContraveneSiteRules");

				case MembershipCreateStatus.InvalidQuestion:
					return GetMessage("Error.SecretQuestionContraveneSiteRules");

				case MembershipCreateStatus.InvalidUserName:
					return GetMessage("Error.LoginContraveneSiteRules");

				case MembershipCreateStatus.ProviderError:
					return GetMessage("Error.AuthenticationProviderError");

				case MembershipCreateStatus.UserRejected:
					return GetMessage("Error.RegistrationAttemptHasBeenRejected");

				default:
					return GetMessage("Error.AnUnknownErrorHasOccurred");
			}
		}
		private FieldMode GetFieldMode(string val)
		{
			val = val != null ? val.ToLowerInvariant() : val;
			switch (val)
			{
				case "hide":
					return FieldMode.Hide;
				case "require":
					return FieldMode.Require;
				default:
					return FieldMode.Show;
			}
		}



		public override bool ProcessCommand(string commandName, BXParamsBag<object> commandParameters, List<string> commandErrors)
		{
			if (commandName.Equals("register", StringComparison.InvariantCultureIgnoreCase))
			{
				string firstName = (string)commandParameters["FirstName"];
				string lastName = (string)commandParameters["LastName"];
				string displayName = (string)commandParameters["DisplayName"];
				string login = (string)commandParameters["Login"];
				string val1 = (string)commandParameters["Password"];
				string email = (string)commandParameters["Email"];
				string question = (string)commandParameters["Question"];
				string answer = (string)commandParameters["Answer"];
				string openIdLogin = OpenIdAuthFields.ContainsKey("login") ? OpenIdAuthFields["login"] : null;
				string liveIdToken = (string)Session["LiveIdAuthToken"];
				//MembershipCreateStatus status;

				try
				{
					try
					{
						user = new BXUser();

						user.UserName = login;
						user.ProviderName = BXUserManager.Provider.Name;
						user.Password = val1;
						user.Email = email;
						user.PasswordQuestion = (BXUserManager.Provider.RequiresQuestionAndAnswer ? question : null);
						user.PasswordAnswer = (BXUserManager.Provider.RequiresQuestionAndAnswer ? answer : null);
						if (SendConfirmationRequest)
						{
							user.IsApproved = false;
							user.ActivationToken = ActivationToken;
						}
						else
							user.IsApproved = true;
						if (firstName != null)
							user.FirstName = firstName;
						if (lastName != null)
							user.LastName = lastName;
						if (displayName != null)
							user.DisplayName = displayName;
						user.SiteId = DesignerSite;
						List<string> validateErrors = null;
						bool hasErrors = false;
						foreach (var s in EditFields)
							if (UserCustomFields.ContainsKey(s))
							{
								var field = UserCustomFields[s];
								validateErrors = validateErrors ?? new List<string>();
								validateErrors.Clear();
								if (!field.Editor.DoValidate(field.FormFieldName, validateErrors))
								{
									hasErrors = true;
									field.ValidateErrors = validateErrors;
									commandErrors.AddRange(validateErrors);
									validateErrors = null;
								}
							}



						foreach (var s in EditFields)
							if (UserCustomFields.ContainsKey(s))
							{
								var field = UserCustomFields[s];
								field.Editor.DoSave(field.FormFieldName, user.CustomValues, new List<string>());
							}

						//привяжем пользователя к его OpenId

						if (!BXStringUtility.IsNullOrTrimEmpty(openIdLogin))
							try
							{
								user.SetOpenID(openIdLogin);
							}
							catch(ArgumentException ex)
							{
								commandErrors.Add(GetMessage(ex.Message == "propertyCode" ? "Error.OpenIdFieldNotFound" : "Error.AnUnknownErrorHasOccurred"));
								hasErrors = true;
							}
							catch (Exception ex)
							{
								commandErrors.Add(GetMessage("Error.AnUnknownErrorHasOccurred"));
								return false;
							}



						if (UseLiveIdAuth && liveIdToken != null)
						{
							try
							{
								user.SetLiveID(liveIdToken);
							}
							catch (ArgumentException ex)
							{
								commandErrors.Add(GetMessage(ex.Message == "propertyCode" ? "Error.LiveIdFieldNotFound" : "Error.AnUnknownErrorHasOccurred"));
								hasErrors = true;
							}
							catch (Exception ex)
							{
								commandErrors.Add(GetMessage("Error.AnUnknownErrorHasOccurred"));
								return false;
							}
						}

						if (hasErrors)
							return false;

						try
						{
							user.Save();
						}
						catch (MembershipCreateUserException ex)
						{
							throw new ProviderException(GetErrorMessage(ex.StatusCode));
						}

						if (SendConfirmationRequest)
							SendEmailConfirmationRequest(user,UrlToConfirmationPage);
					}
					catch (MembershipCreateUserException ex)
					{
						throw new ProviderException(GetErrorMessage(ex.StatusCode));
					}

					//AddToRoles.Remove("Guest");
					//AddToRoles.Remove("");

					foreach (string roleName in AddToRoles)
						BXRoleManager.AddUserToRole(login, "self", roleName, "", "");

					if (DoAuthentication && !SendConfirmationRequest)
					{
						string providerName = null;
						if (BXAuthentication.Authenticate(login, val1, out providerName))
							BXAuthentication.SetAuthCookie(login, providerName, false);
						else
							throw new InvalidOperationException(GetMessage("Error.InvalidLoginOrPassword"));
					}
					string redirectUrl = GetRedirectUrl();

					if ( redirectUrl!="~/" && !SendConfirmationRequest )

						try
						{
							Response.Redirect(redirectUrl);
						}
						catch (ThreadAbortException /*exc*/) { }

					return true;
				}
				catch (BXEventException e)
				{
					foreach (string s in e.Messages)
						commandErrors.Add(s);
				}
				catch (Exception e)
				{
					commandErrors.Add(e.Message);
				}
			}
			else if (commandName.Equals("linkToOpenId", StringComparison.InvariantCultureIgnoreCase))
			{

				string providerName;

				string login = commandParameters.GetString("login", "");
				string password = commandParameters.GetString("password", "");
				string openIdLogin = OpenIdAuthFields.ContainsKey("login") ? OpenIdAuthFields["login"] : null;
				if (BXStringUtility.IsNullOrTrimEmpty(login) || BXStringUtility.IsNullOrTrimEmpty(password)
					|| BXStringUtility.IsNullOrTrimEmpty(openIdLogin))
				{
					
					commandErrors.Add(GetMessage("Error.LinkToOpenIdGeneral"));
					return false;
				}

				if (BXAuthentication.Authenticate(login,password, out providerName))
				{
					Bitrix.Security.BXUser user = BXUserManager.GetByName(login, providerName, false);

					if (user != null)
						try
						{
							user.SetOpenID(openIdLogin);
							user.Save();
						}
						catch (ArgumentException e)
						{
							commandErrors.Add(GetMessage(e.Message == "propertyCode" ? "Error.OpenIdFieldNotFound" : "Error.AnUnknownErrorHasOccurred"));
							return false;
						}
						catch (Exception ex)
						{
							commandErrors.Add(GetMessage("Error.AnUnknownErrorHasOccurred"));
							return false;
						}

					Session.Remove("ExternalAuth");
					BXAuthentication.SetAuthCookie(login, providerName, true);
					Response.Redirect(GetRedirectUrl());
					return true;
				}
				else
				{
					commandErrors.Add(GetMessage("Error.LoginOrPasswordIsInvalid"));
					//show error here - login or pass is invalid
				}
			}
			else if (commandName.Equals("linkToLiveId", StringComparison.InvariantCultureIgnoreCase))
			{

				string providerName;

				string login = commandParameters.GetString("login", "");
				string password = commandParameters.GetString("password", "");
				string liveIdLogin = LiveIdAuthToken;
				if (BXStringUtility.IsNullOrTrimEmpty(login) || BXStringUtility.IsNullOrTrimEmpty(password)
					|| BXStringUtility.IsNullOrTrimEmpty(liveIdLogin))
				{

					commandErrors.Add(GetMessage("Error.LinkToLiveIdGeneral"));
					return false;
				}

				if (BXAuthentication.Authenticate(login, password, out providerName))
				{
					Bitrix.Security.BXUser user = BXUserManager.GetByName(login, providerName, false);
					Session.Remove("LiveIdAuthToken");
					if (user != null)
						try
						{
							user.SetLiveID(liveIdLogin);
							user.Save();
						}
						catch (ArgumentException ex1)
						{
							commandErrors.Add(GetMessage(ex1.Message == "propertyCode" ? "Error.LiveIdFieldNotFound" : "Error.AnUnknownErrorHasOccurred"));
							return false;
						}
						catch (Exception ex)
						{
							commandErrors.Add(GetMessage("Error.AnUnknownErrorHasOccurred"));
							return false;
						}

					BXAuthentication.SetAuthCookie(login, providerName, true);
					Response.Redirect(GetRedirectUrl());
					return true;
				}
				else
				{
					commandErrors.Add(GetMessage("Error.LoginOrPasswordIsInvalid"));
					//show error here - login or pass is invalid
				}
			}
			return false;
		}

		public static void SendEmailConfirmationRequest(BXUser user, string urlToConfPage)
		{
			BXCommand cmd = new BXCommand("Bitrix.Main.ConfirmationRequest");
			FillParameters(cmd.Parameters, user, urlToConfPage);
			cmd.Send();
		}

		static void FillParameters(BXParamsBag<object> parameters,BXUser user, string urlToConfPage)
		{
			parameters["DISPLAY_NAME"] = user.GetDisplayName();
			parameters["LOGIN"] = user.UserName;
			parameters["LAST_NAME"] = user.LastName;
			parameters["FIRST_NAME"] = user.FirstName;
			parameters["EMAIL"] = user.Email;
			
			parameters["CONFIRMATION_LINK"] = GenerateConfirmationLink(user, urlToConfPage);
			parameters["USER_ID"] = user.UserId;
		}

		static string GenerateConfirmationLink(BXUser user ,string urlToConfPage)
		{
			if (String.IsNullOrEmpty(urlToConfPage)) 
				return string.Empty;

			try
			{
				string resolvedUrl = urlToConfPage;

				int index = resolvedUrl.IndexOf('?');
				string path = (index!=-1) ? resolvedUrl.Substring(0, index) : resolvedUrl;
				string query = (index != -1) ? resolvedUrl.Substring( index + 1,resolvedUrl.Length - index - 1) : "";
				Uri uri = new Uri(BXPath.ToUri(path, true));
				NameValueCollection queryParams = HttpUtility.ParseQueryString(query);
				queryParams["UserId"] = user.UserId.ToString();
				queryParams["ActivationToken"] = user.ActivationToken;

				return String.Concat(uri.AbsoluteUri, queryParams.Count > 0 ? "?" + queryParams.ToString() : "");
			}
			catch
			{
				return urlToConfPage;
			}
		}

		public enum FieldMode
		{
			Hide,
			Show,
			Require
		}
	}

	public class SystemRegisterCustomFieldWrapper
	{
		BXCustomField field;
		BXCustomTypePublicEdit editor;
		string formFieldName;
		string uniqueId;
		BXCustomType type;
		List<string> validateErrors;

		public SystemRegisterCustomFieldWrapper(BXCustomField field, string formFieldName, string uniqueId)
		{
			if (field == null) throw new ArgumentNullException("field");
			BXCustomType type = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
			if (type == null) throw new ArgumentNullException("type");

			this.formFieldName = formFieldName;
			this.uniqueId = uniqueId;
			this.field = field;
			editor = type.CreatePublicEditor();
			editor.Init(field);
			this.type = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
		}

		public BXCustomType CustomType
		{
			get { return type; }
		}

		public string FormFieldName
		{
			get { return formFieldName; }
			set { formFieldName = value;}
		}

		public List<string> ValidateErrors
		{
			get { return validateErrors; }
			set { validateErrors = value; }
		}

		public string UniqueID
		{
			get { return uniqueId; }
			set { uniqueId = value; }
		}

		public string Render()
		{
			return editor.Render(formFieldName,uniqueId);
		}

		public BXCustomTypePublicEdit Editor
		{
			get { return editor; }
		}

		public BXCustomField Field
		{
			get { return field; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				field = value;
			}
		}
	}

	public class SystemRegisterTemplate : BXComponentTemplate<SystemRegisterComponent> { }
}

#region Compatibility
[Obsolete("Use Bitrix.Main.Components.SystemRegisterComponent class")]
public partial class RegisterComponent : Bitrix.Main.Components.SystemRegisterComponent
{
}
#endregion
