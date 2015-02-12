using System;
using Bitrix.Components;
using Bitrix.Security;
using Bitrix.UI;
using System.Collections.Generic;
using Bitrix.Components.Editor;
using Bitrix.Services;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using Bitrix.DataTypes;
using Bitrix;
using Bitrix.Services.Text;
using Bitrix.Configuration;
using Bitrix.DataLayer;

public partial class AuthComponent : BXComponent
{
	string sefFolder;
	public string SefFolder
	{
		get { return sefFolder ?? (sefFolder = Parameters.GetString("SEFFolder", "")); }
	}

	private string ActionVariable
	{
		get
		{
			return Parameters.GetString("ActionVariable", "");
		}
		set
		{
			Parameters["ActionVariable"] = value;
		}
	}

	public bool SendConfirmationRequest
	{
		get
		{
			return BXConfigurationUtility.Options.SendConfirmationRequest;
		}
	}

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		var page = Page as BXPublicPage;
		if (page != null)
			if (Bitrix.Main.Components.SystemLoginComponent.IsLiveIdPost(Request.Form))
			{
				var token = Bitrix.Main.Components.SystemLoginComponent.GetTokenFromContext(Request.Form["appctx"]);
				page.CsrfTokenToValidate = token;
			}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		Results["EnablePasswordRecovery"] = BXUserManager.Provider.EnablePasswordReset;
		Results["RequiresQuestionAndAnswer"] = BXUserManager.Provider.RequiresQuestionAndAnswer;
		Results["RequiresCheckWord"] = BXUserManager.Provider.RequiresCheckWord;

		IncludeComponentTemplate(Parameters.GetBool("EnableSEF") ? PrepareSefMode() : PrepareNormalMode());
	}


	private string PrepareSefMode()
	{
		if (!Parameters.ContainsKey("ProfilePath"))
			Parameters["ProfilePath"] = string.Empty;

		Results["PasswordRecoveryPath"] = CombineLink(SefFolder,  Parameters.GetString("PasswordRecoveryTemplate"));
		Results["PasswordRecoveryCodePath"] = CombineLink(SefFolder,  Parameters.GetString("PasswordResetTemplate"));
		Results["RegisterPath"] = CombineLink(SefFolder,  Parameters.GetString("RegisterTemplate"));
		Results["LoginRedirectPath"] = Request["back_url"] ?? CombineLink(SefFolder, "");
		Results["LoginPath"] =  CombineLink(SefFolder, "");
		Results["LoginLink"] = Results["LoginPath"];

		BXParamsBag<string> sefMap = new BXParamsBag<string>();
		if (Parameters.GetBool("RegistrationAllow"))
			sefMap.Add("register", Parameters.GetString("RegisterTemplate", ""));
		sefMap.Add("PasswordRecovery", Parameters.GetString("PasswordRecoveryTemplate", ""));
		sefMap.Add("PasswordRecoveryCode", Parameters.GetString("PasswordResetTemplate", ""));
		if (SendConfirmationRequest)
		{
			sefMap.Add("confirmation", Parameters.GetString("ConfirmationTemplate", ""));
			Results["UrlToConfirmationPage"] = CombineLink(SefFolder, Parameters.GetString("ConfirmationTemplate"));
		}
		
		return BXSefUrlUtility.MapVariable(SefFolder, sefMap, ComponentCache, "login", null, null);
	}
	private string PrepareNormalMode()
	{
		if (!Parameters.ContainsKey("ProfilePath"))
			Parameters["ProfilePath"] = string.Empty;

		string url = BXSefUrlManager.CurrentUrl.AbsolutePath;

		StringBuilder q = new StringBuilder();
		NameValueCollection query = Request.QueryString;
		for (int i = 0; i < query.Count; i++)
		{
			string key = query.GetKey(i);
			if (string.Equals(key, ActionVariable, StringComparison.OrdinalIgnoreCase) || string.Equals(key, "checkword", StringComparison.OrdinalIgnoreCase))
				continue;
			string[] values = query.GetValues(i);
			if (values == null)
				continue;

			foreach (string v in values)
			{
				q.Append(q.Length != 0 ? '&' : '?');
				if (key != null)
				{
					q.Append(HttpUtility.UrlEncode(key));
					q.Append('=');
				}
				q.Append(HttpUtility.UrlEncode(v ?? ""));
			}
		}

		string combined = url + q.ToString();
		Results["LoginRedirectPath"] = combined;
		if (Request["back_url"] != null)
			Results["LoginRedirectPath"] = Request["back_url"];
		Results["LoginPath"] = combined;
		Results["LoginLink"] = Results["LoginPath"];

		q.Append(q.Length != 0 ? "&" : "?");
		q.Append(HttpUtility.UrlEncode(ActionVariable));
		q.Append('=');
		combined = url + q.ToString();

		Results["PasswordRecoveryPath"] = string.Concat(combined, "recovery");
		Results["PasswordRecoveryCodePath"] = string.Concat(combined, "recoverycode");
		Results["RegisterPath"] = string.Concat(combined, "register");
		Results["UrlToConfirmationPage"] = BXSefUrlManager.CurrentUrl.AbsolutePath + "?" + ActionVariable + "=" + "confirmation";// string.Concat(combined, "confirmation");

		string action = Request[ActionVariable];
		if (!string.IsNullOrEmpty(action))
			action = action.ToLowerInvariant();

		switch (action)
		{
			case "recovery":
				return "PasswordRecovery";
			case "recoverycode":
				return "PasswordRecoveryCode";
			case "confirmation":
				if (SendConfirmationRequest)
					return "confirmation";
				else
					goto default;
			case "register":
				if (Parameters.GetBool("RegistrationAllow"))
					return "register";
				else 
					goto default;
			default:
				return "login";
		}
	}

	protected override void PreLoadComponentDefinition()
	{
		Title = GetMessage("SystemAuth.Title");
		Description = GetMessage("SystemAuth.Description");
		Icon = "images/system_auth.gif";

		Group = new BXComponentGroup("Auth", GetMessage("Group"), 100, BXComponentGroup.Utility);

		BXCategory main = BXCategory.Main;
		ParamsDefinition["ProfilePath"] = new BXParamText(GetMessageRaw("UserProfilePath"), "", main);
		ParamsDefinition["RegistrationAllow"] = new BXParamYesNo(GetMessageRaw("RegistrationAllow"), true, main);
		ParamsDefinition.Add(
			"AddToRoles",
			new BXParamMultiSelection(
				GetMessageRaw("AddToRoles"),
				"",
				BXCategory.Main
				)
			);
		ParamsDefinition["RegistrationDoAuthentication"] = new BXParamYesNo(GetMessageRaw("RegistrationDoAuthentication"), false, main);
		ParamsDefinition["RegistrationRedirectUrl"] = new BXParamText(GetMessageRaw("RegistrationRedirectUrl"), "", main);

		ParamsDefinition.Add(
			"TryRegisterNewExternalUser",
			new BXParamYesNo(
				GetMessageRaw("TryRegisterNewExternalUser"),
				true,
				BXCategory.Main
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

		ParamsDefinition.Add(
			"UseCaptcha",
			new BXParamYesNo(
				GetMessageRaw("UseCaptcha"),
				false,
				BXCategory.Main
			)
		);


   
		BXCategory fields = new BXCategory(GetMessageRaw("FieldSettings"), "fields", 200);
		ParamsDefinition["FirstNameFieldMode"] = new BXParamSingleSelection(GetMessageRaw("FirstNameFieldMode"), "require", fields);
		ParamsDefinition["LastNameFieldMode"] = new BXParamSingleSelection(GetMessageRaw("LastNameFieldMode"), "require", fields);
		ParamsDefinition["DisplayNameFieldMode"] = new BXParamSingleSelection(GetMessageRaw("DisplayNameFieldMode"), "hide", fields);

		// Query string variable names
		BXCategory sef = BXCategory.Sef;
		ParamsDefinition.Add(BXParametersDefinition.Sef);
		ParamsDefinition["ActionVariable"] = new BXParamText(GetMessageRaw("ActionVariable"), "auth_page", sef);
		ParamsDefinition["RegisterTemplate"] = new BXParamText(GetMessageRaw("RegisterTemplate"), "/register/", sef);
		ParamsDefinition["PasswordRecoveryTemplate"] = new BXParamText(GetMessageRaw("PasswordRecoveryTemplate"), "/recovery/", sef);
		ParamsDefinition["PasswordResetTemplate"] = new BXParamText(GetMessageRaw("PasswordResetTemplate"), "/reset/", sef);
		ParamsDefinition["ConfirmationTemplate"] = new BXParamText(GetMessageRaw("ConfirmationTemplate"), "/confirmation/", sef);
	}
	protected override void LoadComponentDefinition()
	{
		ParamsDefinition["RegistrationAllow"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(ClientID, "RegistrationAllow", "RegistrationEnabled", "RegistrationDisabled");
		ParamsDefinition["RegistrationDoAuthentication"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "RegistrationDoAuthentication", new string[] { "RegistrationEnabled" });
		ParamsDefinition["RegistrationRedirectUrl"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "RegistrationRedirectUrl", new string[] { "RegistrationEnabled" });

		ParamsDefinition["FirstNameFieldMode"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "FirstNameFieldMode", new string[] { "RegistrationEnabled" });
		ParamsDefinition["LastNameFieldMode"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "LastNameFieldMode", new string[] { "RegistrationEnabled" });
		ParamsDefinition["DisplayNameFieldMode"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "DisplayNameFieldMode", new string[] { "RegistrationEnabled" });

		ParamsDefinition["EnableSEF"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(ClientID, "EnableSEF", "Sef", "NonSef");
		ParamsDefinition["SEFFolder"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "SEFFolder", new string[] { "Sef" });
		ParamsDefinition["ActionVariable"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "ActionVariable", new string[] { "NonSef" });
		ParamsDefinition["RegisterTemplate"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "RegisterTemplate", new string[] { "Sef", "RegistrationEnabled" }, ParamClientSideActionGroupViewMemberDisplayCondition.And);
		ParamsDefinition["PasswordRecoveryTemplate"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "PasswordRecoveryTemplate", new string[] { "Sef" });
		ParamsDefinition["PasswordResetTemplate"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "PasswordResetTemplate", new string[] { "Sef" });
		ParamsDefinition["ConfirmationTemplate"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "ConfirmationTemplate", new string[] { "Sef" });

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

		BXCustomFieldCollection col = BXCustomEntityManager.GetFields(BXUser.GetCustomFieldsKey());
		List<BXParamValue> fields = new List<BXParamValue>();

		foreach (BXCustomField customField in col)
		{
			string title = BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel);
			string code = customField.Name.ToUpper();
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
}
