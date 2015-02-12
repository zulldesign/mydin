using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Bitrix.Components;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.Modules;
using Bitrix.OpenId;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.Services.User;
using Bitrix.UI;
using WindowsLive;

namespace Bitrix.Main.Components
{
	public partial class UserProfileEditComponent : BXComponent
	{
		private BXCustomFieldCollection customFields;
		private List<FieldInfo> availableFieldsList;
		private Dictionary<string, FieldInfo> availableFields;
		private List<string> requiredFields;
		private List<string> ownerFields;
		private List<string> selectedFields;
		private Dictionary<BXUserProfileExtensionProvider, BXUserProfilePublicFacade> loadedExtensions;
		private List<FieldGroup> groups;
		private Dictionary<string, Field> fields;
		private BXUser user;
		private List<string> globalErrors;
		private bool hasErrors;
		private bool useCustomProperties;
		private int? userId;
		private bool isPermissionDenied;
		private BXParamsBag<object> replaceDictionary;
		private string profileUrl;
		private bool isProfileUrlBuilt;
		private bool isSaved;
		private string fatalError;
		private PasswordEditorController passwordController;

		private BXCustomFieldCollection CustomFields
		{
			get
			{
				return
					customFields
					?? (customFields = BXCustomField.GetList(
						new BXFilter(new BXFilterItem(BXCustomField.Fields.OwnerEntityId, BXSqlFilterOperators.Equal, BXUser.GetCustomFieldsKey())),
						new BXOrderBy(new BXOrderByPair(BXCustomField.Fields.Sort, BXOrderByDirection.Asc)),
						null,
						null,
						BXTextEncoder.EmptyTextEncoder
					));
			}
		}
		private List<FieldInfo> AvailableFieldsList
		{
			get
			{
				return availableFieldsList ?? (availableFieldsList = GetAvailableFields());
			}
		}
		private Dictionary<string, FieldInfo> AvailableFields
		{
			get
			{
				return availableFields ?? (availableFields = FillAvailableFields());
			}
		}

		private List<string> SelectedFields
		{
			get
			{
				return selectedFields ?? (selectedFields = GetSelectedFields());
			}
		}

		private List<string> GetSelectedFields()
		{
			var f = Parameters.GetListString("EditFields");
			if (CurrentUserId <= 0 || CurrentUserId != UserId)
				f.RemoveAll(x => OwnerFields.BinarySearch(x) >= 0);
			return f;
		}
		private List<string> RequiredFields
		{
			get
			{
				return requiredFields ?? (requiredFields = GetRequiredFields());
			}
		}
		private List<string> OwnerFields
		{
			get
			{
				return ownerFields ?? (ownerFields = GetOwnerFields());
			}
		}
		private BXParamsBag<object> ReplaceDictionary
		{
			get
			{
				if (replaceDictionary == null)
				{
					replaceDictionary = new BXParamsBag<object>();
					replaceDictionary.Add("Id", User.UserId);
					replaceDictionary.Add("Name", User.UserName);
					replaceDictionary.Add("UserId", User.UserId);
					replaceDictionary.Add("UserName", User.UserName);
				}
				return replaceDictionary;
			}
		}

		public int UserId
		{
			get
			{
				return (userId ?? ((userId = Parameters.GetInt("UserId")) > 0 ? userId : userId = ((BXIdentity)BXPrincipal.Current.Identity).Id)).Value;
			}
		}

		int curUserId = -1;

		public int CurrentUserId
		{
			get { return curUserId != -1 ? curUserId : (curUserId = BXPrincipal.Current.Identity.IsAuthenticated ? ((BXIdentity)BXPrincipal.Current.Identity).Id : 0); }
		}

		public bool CanModifyExternalAuth
		{
			get { return UserId == CurrentUserId && CurrentUserId > 0; }
		}

		public List<FieldGroup> FieldGroups
		{
			get
			{
				return groups;
			}
		}
		public Dictionary<string, Field> Fields
		{
			get
			{
				return fields;
			}
		}
		public BXUser User
		{
			get
			{
				return user;
			}
		}
		public bool IsPermissionDenied
		{
			get
			{
				return isPermissionDenied;
			}
		}
		public List<string> GlobalErrors
		{
			get
			{
				return globalErrors ?? (globalErrors = new List<string>());
			}
		}
		public IEnumerable<string> ErrorSummary
		{
			get
			{
				foreach (Field field in Fields.Values)
					if (field.ValidateErrors != null && field.ValidateErrors.Count != 0)
						foreach (string error in field.ValidateErrors)
							yield return error;

				if (globalErrors != null && globalErrors.Count != 0)
					foreach (string error in globalErrors)
						yield return error;
			}
		}
		public bool HasErrors
		{
			get
			{
				return hasErrors;
			}
		}
		public string RedirectPageUrl
		{
			get
			{
				string s = Parameters.GetString("RedirectPageUrl");
				string url = !BXStringUtility.IsNullOrTrimEmpty(s) ? s : ProfileUrl;
				if (url.StartsWith("?"))
				{
					UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
					uri.Query = url.Substring(1);
					url = uri.ToString();
				}
				return url;
			}
		}
		public string ProfileUrl
		{
			get
			{
				if (!isProfileUrlBuilt)
				{
					isProfileUrlBuilt = true;
					profileUrl = Parameters.GetString("ProfileUrlTemplate");
					if (profileUrl != null)
						profileUrl = ResolveTemplateUrl(profileUrl, ReplaceDictionary);
				}
				return profileUrl;
			}
		}
		private string DefaultSuccessMessage
		{
			get
			{
				string msg = Parameters.GetString("SuccessMessage");
				return Encode(!string.IsNullOrEmpty(msg) ? msg : GetMessageRaw("Message.UpdateSuccess"));
			}
		}
		private string successMessage;
		public string SuccessMessage
		{
			get
			{
				return successMessage ?? DefaultSuccessMessage;
			}
		}
		public bool IsSaved
		{
			get
			{
				return isSaved;
			}
			set
			{
				isSaved = value;
			}
		}
		public string FatalError
		{
			get
			{
				return fatalError;
			}
			set
			{
				fatalError = value;
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


		Field openIdAuthField;
		Field liveIdAuthField;

		public ICollection<BXUserProfilePublicFacade> LoadedExtensions
		{
			get { return loadedExtensions.Values; }
		}

		public event EventHandler<CreateCustomFieldEditorEventArgs> CreateCustomFieldEditor;
		public event EventHandler<CreateUserFieldEditorEventArgs> CreateUserFieldEditor;
		public event EventHandler<CreateProfileFieldEditorEventArgs> CreateProfileFieldEditor;
		public event EventHandler<BeforeSaveEventArgs> BeforeSave;
		public event EventHandler<AfterSaveEventArgs> AfterSave;

		//METHODS

		//пользователь успешно авторизован с текущим openid, сохраним его openid в профиле
		protected void OpenIdValidationSucceeded(object sender, EventArgs e)
		{
			if (User == null)
				return;

			var client = sender as OpenIdClient;
			OpenIdUser user = client != null ? client.RetrieveUser() : null;
			if (user == null)
			{
				return;
			}
			if (User != null)
			{
				try
				{
					User.SetOpenID(user.Identity);
					User.Save();
				}
				catch (Exception ex)
				{
					hasErrors = true;
					GlobalErrors.Add(ex.Message);
					return;
				}
				IsSaved = true;
			}

			Response.Redirect(BXSefUrlManager.CurrentUrl.AbsolutePath + "?openid=y");
			Context.ApplicationInstance.CompleteRequest();
		}

		protected void OpenIdValidationFailed(object sender, EventArgs e)
		{
			Response.Redirect(BXSefUrlManager.CurrentUrl.AbsolutePath);
			Context.ApplicationInstance.CompleteRequest();
		}

		protected void OpenIdReceivedCancelled(object sender, EventArgs e)
		{
			Response.Redirect(RedirectPageUrl);
			Context.ApplicationInstance.CompleteRequest();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				if (!IsPostBack && Request.QueryString["emailConfirmation"] != null)
					ConfirmEmail(Request.QueryString["emailConfirmation"]);


				loadedExtensions = new Dictionary<BXUserProfileExtensionProvider, BXUserProfilePublicFacade>();
				groups = new List<FieldGroup>();
				fields = new Dictionary<string, Field>();
				user = LoadUser();

				IncludeComponentTemplate();

				if (user == null)
				{
					FatalError = GetMessage("Message.UserNotFound");
					return;
				}

				if (IsPermissionDenied)
					return;

				string argument = Request.Form.Get(Page.postEventArgumentID);

				if (IsPostBack && argument == "DetachOpenId")
				{
					if (CanModifyExternalAuth && CanEdit(UserId, ""))
					{
						try
						{
							User.RemoveOpenID();
						}
						catch
						{
							FatalError = GetMessage("Error.LiveIdFieldNotFound");
						}

						User.Save();
						IsSaved = true;
						Session.Remove("ExternalAuth");
					}
					else
					{
						GlobalErrors.Add(GetMessage("CanNotModifyUserOpenId"));
						hasErrors = true;
					}

				}

				if (IsPostBack && argument == "DetachLiveId")
				{
					if (CanModifyExternalAuth)
					{
						try
						{
							User.RemoveLiveID();
						}
						catch
						{
							FatalError = GetMessage("Error.LiveIdFieldNotFound");
						}
						User.Save();
						IsSaved = true;
						Session.Remove("LiveIdAuthToken");
						//Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
					}
					else
					{
						GlobalErrors.Add(GetMessage("CanNotModifyUserLiveId"));
						hasErrors = true;
					}

				}

				LoadFields();

				if (IsPostBack && argument == "AttachOpenId")
					AttachOpenId();

				if (IsPostBack && argument == "AttachLiveId")
					AttachLiveId();

				foreach (FieldGroup group in groups)
				{
					if (group.IsEmpty)
						continue;
					foreach (Field f in group.Fields)
						f.Editor.Load(User, f.IsCustom ? User.CustomValues : null, f.Settings);
				}

				if (UseOpenIdAuth)
				{
					var openId = new OpenIdClient();
					openId.ValidationSucceeded += new EventHandler(OpenIdValidationSucceeded);
					openId.ValidationFailed += new EventHandler(OpenIdValidationFailed);
					openId.ReceivedCancel += new EventHandler(OpenIdReceivedCancelled);
					openId.DetectAndHandleResponse();
				}

				string liveIdToken = (string)Session["linkToLiveId"];


				if (UseLiveIdAuth && !String.IsNullOrEmpty(liveIdToken))
				{
					if (CanModifyExternalAuth)
					{
						WindowsLiveLogin liveLogin =
							new WindowsLiveLogin(
								BXConfigurationUtility.Options.LiveIDApplicationID,
								BXConfigurationUtility.Options.LiveIDSecretKey);
						WindowsLiveLogin.User u = null;
						try
						{
							u = liveLogin.ProcessToken(liveIdToken);
							Session.Remove("linkToLiveId");
						}
						catch (Exception ex)
						{
							GlobalErrors.Add(GetMessage("Error.LiveIdAuthFailed"));
							hasErrors = true;
							return;
						}

						if (u != null && !String.IsNullOrEmpty(u.Id))
						{
							BXUser bUser = null;
							try
							{
								bUser = BXUser.GetByLiveID(u.Id);
							}
							catch (Exception ex2)
							{
								hasErrors = true;
								GlobalErrors.Add(GetMessage("Error.LiveIdFieldNotFound"));
								return;
							}
							if (bUser == null)
							{

								User.SetLiveID(u.Id);

								User.Save();
								IsSaved = true;
								((LiveIdEditor)liveIdAuthField.Editor).IsAttach = false;
								((LiveIdEditor)liveIdAuthField.Editor).ButtonText = GetMessage("OpenID.Detach");
								Session.Remove("linkToLiveId");
							}
							else
							{
								if (bUser.UserId != CurrentUserId)
								{
									GlobalErrors.Add(GetMessage("LiveIdError.UserWithLiveIdAlreadyExists"));
									hasErrors = true;
								}
							}
						}

					}
					else
					{
						GlobalErrors.Add(GetMessage("Error.LiveIdAuthFailed"));
						hasErrors = true;
					}
				}

				if (!IsSaved && Request.QueryString["openid"] == "y")
					IsSaved = true;
			}
			catch (Exception ex)
			{
				BXLogService.LogAll(ex, 0, BXLogMessageType.Error, Name + "@" + (Page != null ? Page.AppRelativeVirtualPath : string.Empty));
			}
		}

		private void ConfirmEmail(string token)
		{
			string[] strs;
			try
			{
				strs = Encoding.Unicode.GetString(Convert.FromBase64String(token)).Split(',');
			}
			catch
			{
				return;
			}

			if (strs.Length != 2)
				return;

			int userId;
			if (!int.TryParse(strs[0], out userId))
				return;

			if (string.IsNullOrEmpty(strs[1]))
				return;

			var userToken = BXUserToken.GetList(
				new BXFilter(
					new BXFilterItem(BXUserToken.Fields.UserId, BXSqlFilterOperators.Equal, userId),
					new BXFilterItem(BXUserToken.Fields.Type, BXSqlFilterOperators.Equal, BXUserTokenType.EmailChange),
					new BXFilterItem(BXUserToken.Fields.DateExpiresUtc, BXSqlFilterOperators.GreaterOrEqual, DateTime.UtcNow),
					new BXFilterItem(BXUserToken.Fields.Token, BXSqlFilterOperators.Equal, strs[1])
				),
				null,
				new BXSelectAdd(BXUserToken.Fields.User),
				new BXQueryParams(BXPagingOptions.Top(1)),
				BXTextEncoder.EmptyTextEncoder
			)
			.FirstOrDefault();

			if (userToken == null || !BXUser.IsCanModify(userToken.UserId) || !CanEdit(userToken.UserId, "Email"))
				return;

			try
			{
				var email = userToken.Info;
				if (!string.IsNullOrEmpty(email) && userToken.User != null && BXUserManager.GetUserNameByEmail(email).Count == 0)
				{
					userToken.User.Email = email;
					userToken.User.Save();

					IsSaved = true;
					successMessage = GetMessageRaw("Message.EmailSuccessfullyChanged");
				}
			}
			catch
			{
			}

			// double save - first we try to change email, then to drop the token
			try
			{
				userToken.Delete();
			}
			catch
			{
			}
		}
		private bool CanEdit(int userId, string field)
		{
			if (CurrentUserId > 0 && CurrentUserId == UserId)
				return true;
			if (OwnerFields.Count == 0)
				return true;

			return !OwnerFields.Contains(field);
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = new BXComponentGroup("Auth", GetMessageRaw("Category"), 100, BXComponentGroup.Utility);

			BXCategory mainCategory = BXCategory.Main;
			BXCategory addSettingsCategory = BXCategory.AdditionalSettings;

			ParamsDefinition["UserId"] = new BXParamText(GetMessageRaw("Param.UserId"), "", mainCategory);
			ParamsDefinition["EditFields"] = new BXParamDoubleListWithCustomAdd(GetMessageRaw("Param.EditFields"), "", mainCategory);
			ParamsDefinition["RequiredFields"] = new BXParamMultiSelection(GetMessageRaw("Param.RequiredFields"), "", mainCategory);
			ParamsDefinition["OwnerFields"] = new BXParamMultiSelection(GetMessageRaw("Param.OwnerFields"), "", mainCategory);

			ParamsDefinition["RedirectPageUrl"] = new BXParamText(GetMessageRaw("Param.RedirectPageUrl"), "", addSettingsCategory);
			ParamsDefinition["ProfileUrlTemplate"] = new BXParamText(GetMessageRaw("Param.ProfileUrlTemplate"), "", addSettingsCategory);
			ParamsDefinition["SuccessMessage"] = new BXParamText(GetMessageRaw("Param.SuccessMessage"), "", addSettingsCategory);
		}
		protected override void LoadComponentDefinition()
		{
			BXParamDoubleListWithCustomAdd editFields = (BXParamDoubleListWithCustomAdd)ParamsDefinition["EditFields"];
			editFields.AddButtonTitle = GetMessageRaw("Param.EditFields.ButtonTitle");
			editFields.CustomValuePrefix = "@";
			editFields.CustomValueTextFormat = "- {0} -";
			editFields.Prompt = GetMessageRaw("Param.EditFields.Prompt");
			editFields.DefaultCustomValue = GetMessageRaw("Param.EditFields.DefaultName");
			editFields.Values = AvailableFieldsList.ConvertAll<BXParamValue>(delegate(FieldInfo input)
			{
				return new BXParamValue(input.Title, input.ClientId);
			});

			//List<BXParamValue> requiredFields = new List<BXParamValue>();
			//foreach (FieldInfo f in AvailableFieldsList)
			//    if (!f.IsCustom)
			//        requiredFields.Add(new BXParamValue(f.Title, f.ClientId));
			//ParamsDefinition["RequiredFields"].Values = requiredFields;

			SetupMultiList("RequiredFields", AvailableFieldsList.Where(x => !x.IsCustom), x => x.ClientId, x => x.Title);
			SetupMultiList("OwnerFields", AvailableFieldsList, x => x.ClientId, x => x.Title);
		}

		private void SetupMultiList<T>(string param, IEnumerable<T> options, Func<T, string> key, Func<T, string> title)
		{
			var available = options.Select(key).ToArray();
			var values = Parameters.GetListString(param, null);
			if (values != null && values.Count != 0)
			{
				values = values.FindAll(x => Array.IndexOf(available, x) >= 0);
				Parameters[param] = BXStringUtility.ListToCsv(values);
			}

			IQueryable<T> q = options.AsQueryable();

			if (values != null && values.Count > 0)
				q = q.OrderBy(x => x, new OrderComparer<T>(values, key));

			ParamsDefinition[param].Values = q.Select(x => new BXParamValue(title(x), key(x))).ToList();
		}

		class OrderComparer<T> : IComparer<T>
		{
			List<string> values;
			Func<T, string> key;

			public OrderComparer(List<string> values, Func<T, string> key)
			{
				this.values = values;
				this.key = key;
			}

			#region IComparer<T> Members

			public int Compare(T a, T b)
			{
				int ia = values.Contains(key(a)) ? 0 : 1;
				int ib = values.Contains(key(b)) ? 0 : 1;
				//if (ia != ib)
				return ia - ib;
			}

			#endregion
		}


		private BXUser LoadUser()
		{
			BXUser user = null;
			if (UserId > 0)
			{
				BXUserCollection users = BXUser.GetList(
					new BXFilter(
						new BXFilterItem(BXUser.Fields.UserId, BXSqlFilterOperators.Equal, UserId),
						new BXFilterItem(BXUser.Fields.IsApproved, BXSqlFilterOperators.Equal, true),
						new BXFilterItem(BXUser.Fields.IsLockedOut, BXSqlFilterOperators.Equal, false)
					),
					null,
					new BXSelectAdd(BXUser.Fields.CustomFields.DefaultFields),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (users.Count > 0)
					user = users[0];
			}
			if (user != null && !BXUser.IsCanModify(user))
				isPermissionDenied = true;
			return user;
		}
		private void LoadFields()
		{
			List<Field> currentFields = null;
			foreach (string f in SelectedFields)
			{
				//С символа '@' начанается имя группы
				if (f.StartsWith("@", StringComparison.Ordinal))
				{
					if (currentFields != null && currentFields.Count != 0)
					{
						if (groups.Count == 0)
							groups.Add(new FieldGroup());
						FieldGroup curg = groups[groups.Count - 1];
						curg.fields = currentFields;
						currentFields = null;
					}

					FieldGroup g = new FieldGroup();
					g.Title = f.Substring(1);
					groups.Add(g);
					continue;
				}
				//Обработка поля
				FieldInfo info;
				if (!AvailableFields.TryGetValue(f, out info))
					continue;

				foreach (Field nf in CreateField(info))
				{
					(currentFields ?? (currentFields = new List<Field>())).Add(nf);
					fields.Add(nf.Id, nf);
				}
			}
			if (currentFields != null && currentFields.Count != 0)
			{
				if (groups.Count == 0)
					groups.Add(new FieldGroup());
				FieldGroup curg = groups[groups.Count - 1];
				curg.fields = currentFields;
			}

			if (UseOpenIdAuth || UseLiveIdAuth)
			{
				FieldGroup externalAuthGroup = new FieldGroup();
				externalAuthGroup.Title = GetMessage("ExternalAuth");
				BXParamsBag<object> settings = new BXParamsBag<object>();

				if (UseOpenIdAuth)
				{

					openIdAuthField = new Field(new FieldInfo(BXConfigurationUtility.Options.User.OpenIDCustomFieldCode, GetMessage("OpenID")));
					openIdAuthField.FormFieldName = ClientID + "$openid";
					var userOpenId = User.CustomPublicValues.GetString(BXConfigurationUtility.Options.User.OpenIDCustomFieldCode) ?? String.Empty;

					settings.Add("required", false);
					openIdAuthField.Settings = settings;
					var editor = new OpenIdEditor("OpenID", userOpenId,
						String.IsNullOrEmpty(userOpenId) ? GetMessage("OpenID.Attach") : GetMessage("OpenID.Detach"), this, String.IsNullOrEmpty(userOpenId), CanModifyExternalAuth);

					editor.ConfirmMessage = GetMessageJS("OpenIDDetachConfirm");
					editor.ShowConfirmMessage = true;

					openIdAuthField.Editor = editor;
					externalAuthGroup.Fields.Add(openIdAuthField);

				}

				if (UseLiveIdAuth)
				{
					liveIdAuthField = new Field(new FieldInfo(BXConfigurationUtility.Options.User.LiveIDCustomFieldCode, GetMessage("LiveID")));
					var userLiveId = User.CustomPublicValues.GetString(BXConfigurationUtility.Options.User.LiveIDCustomFieldCode) ?? String.Empty;
					settings.Clear();
					settings.Add("required", false);
					liveIdAuthField.Settings = settings;
					var lEditor = new LiveIdEditor(GetMessage("LiveID"),
						String.IsNullOrEmpty(userLiveId) ? GetMessage("LiveID.Attach") : GetMessage("LiveID.Detach"),
						this, String.IsNullOrEmpty(userLiveId), CanModifyExternalAuth);

					lEditor.ShowConfirmMessage = true;
					lEditor.ConfirmMessage = GetMessageJS("LiveIDDetachConfirm");

					liveIdAuthField.Editor = lEditor;
					externalAuthGroup.Fields.Add(liveIdAuthField);
				}

				groups.Add(externalAuthGroup);
			}
		}
		private IEnumerable<Field> CreateField(FieldInfo info)
		{
			Field field = new Field(info);
			useCustomProperties |= info.IsCustom;
			field.FormFieldName = ID + info.ClientId;
			field.UniqueId = ClientID + ClientIDSeparator + info.ClientId;
			field.Settings = ResolveFieldSettings(info);

			if (info.Provider != null)
			{
				field.IsRequired = RequiredFields.BinarySearch(info.ClientId) >= 0;

				BXUserProfileFieldPublicEditor editor = null;
				BXUserProfilePublicFacade facade;
				if (!loadedExtensions.TryGetValue(info.Provider, out facade))
				{
					loadedExtensions.Add(info.Provider, facade = info.Provider.CreatePublicFacade());
					facade.User = User;
					facade.Init();
				}

				if (CreateProfileFieldEditor != null)
				{
					CreateProfileFieldEditorEventArgs args = new CreateProfileFieldEditorEventArgs(info.Id, field.RawTitle, field.FormFieldName, field.Settings, info.Provider, facade);
					CreateProfileFieldEditor(this, args);
					field.RawTitle = args.Title;
					editor = args.Editor;
				}

				field.Editor = new ProfileFieldEditor(editor ?? info.Provider.CreateDefaultPublicEditor(info.Id), facade);
			}
			else if (info.IsCustom)
			{
				BXCustomTypePublicEdit editor = null;
				field.CustomField = CustomFields[info.Id];
				field.CustomType = BXCustomTypeManager.GetCustomType(field.CustomField.CustomTypeId);
				field.IsRequired = field.CustomField.Mandatory;
				if (CreateCustomFieldEditor != null)
				{
					CreateCustomFieldEditorEventArgs args = new CreateCustomFieldEditorEventArgs(info.Id, field.RawTitle, field.FormFieldName, field.CustomField, field.CustomType);
					CreateCustomFieldEditor(this, args);
					field.RawTitle = args.Title;
					editor = args.Editor;
				}

				field.Editor = new CustomFieldEditor(field.CustomField, editor);
			}
			else
			{
				field.IsRequired = RequiredFields.BinarySearch(info.ClientId) >= 0;

				if (CreateUserFieldEditor != null)
				{
					CreateUserFieldEditorEventArgs args = new CreateUserFieldEditorEventArgs(info.Id, field.RawTitle, field.FormFieldName, field.Settings, field.IsRequired);
					CreateUserFieldEditor(this, args);
					field.RawTitle = args.Title;
					field.Editor = args.Editor;
				}
				if (info.Id == "Password" && field.Editor == null)
				{
					foreach (Field f in CreateSpecialPasswordFields(info, field))
						yield return f;
					yield break;
				}
				field.Editor = field.Editor ?? ResolveDefaultFieldEditor(info.Id);
			}
			yield return field;
		}

		private IEnumerable<Field> CreateSpecialPasswordFields(FieldInfo info, Field field)
		{
			passwordController = new PasswordEditorController(this);

			Field oldPasswordField = new Field(info);
			oldPasswordField.Id = info.ClientId + "_Old";
			oldPasswordField.FormFieldName = field.FormFieldName + "_Old";
			oldPasswordField.UniqueId = field.UniqueId + ClientIDSeparator + "Old";
			oldPasswordField.Settings = field.Settings;
			oldPasswordField.RawTitle = GetMessageRaw("PasswordEditor.OldPassword");
			oldPasswordField.Editor = passwordController.ProvideOldEditor(oldPasswordField);
			yield return oldPasswordField;

			Field newPasswordField = new Field(info);
			newPasswordField.Id = info.ClientId + "_New";
			newPasswordField.FormFieldName = field.FormFieldName + "_New";
			newPasswordField.UniqueId = field.UniqueId + ClientIDSeparator + "_New";
			newPasswordField.Settings = field.Settings;
			newPasswordField.RawTitle = GetMessageRaw("PasswordEditor.NewPassword");
			newPasswordField.Editor = passwordController.ProvideNewEditor(newPasswordField);
			newPasswordField.IsRequired = field.IsRequired;
			yield return newPasswordField;

			Field confirmPasswordField = new Field(info);
			confirmPasswordField.Id = info.ClientId + "_Confirm";
			confirmPasswordField.FormFieldName = field.FormFieldName + "_Confirm";
			confirmPasswordField.UniqueId = field.UniqueId + ClientIDSeparator + "_Confirm";
			confirmPasswordField.Settings = field.Settings;
			confirmPasswordField.RawTitle = GetMessageRaw("PasswordEditor.Confirmation");
			confirmPasswordField.Editor = passwordController.ProvideConfirmEditor(confirmPasswordField);
			yield return confirmPasswordField;
		}


		private BXParamsBag<object> ResolveFieldSettings(FieldInfo info)
		{
			if (info.IsCustom)
				return null;

			BXParamsBag<object> settings = new BXParamsBag<object>();
			bool required = RequiredFields.BinarySearch(info.ClientId) >= 0;
			settings["required"] = required;
			settings["fieldTitle"] = info.Title;

			if (info.Provider != null)
			{
				info.Provider.FillPublicEditorSettings(info.Id, settings);
				return settings;
			}
			switch (info.Id)
			{
				case "Image":
					settings["maxFileSizeUpload"] = BXConfigurationUtility.Options.User.AvatarMaxSizeKB;
					settings["maxWidth"] = BXConfigurationUtility.Options.User.AvatarMaxWidth;
					settings["maxHeight"] = BXConfigurationUtility.Options.User.AvatarMaxHeight;
					settings["hintText"] = string.Format(GetMessageRaw("Hint.ImageMaxSize"), BXConfigurationUtility.Options.User.AvatarMaxWidth.ToString(), BXConfigurationUtility.Options.User.AvatarMaxHeight.ToString());
					settings["enableShrinking"] = true;
					break;
				case "Gender":
					settings["items"] =
						required
						? new ListItem[] { 
							new ListItem(GetMessageRaw("GenderMale"), "M"),
							new ListItem(GetMessageRaw("GenderFemale"), "F")
						}
						: new ListItem[] { 
							new ListItem(GetMessageRaw("Kernel.Unknown"), ""),
							new ListItem(GetMessageRaw("GenderMale"), "M"),
							new ListItem(GetMessageRaw("GenderFemale"), "F")
						};
					break;
				case "BirthdayDate":
					settings["validateForSql"] = true;
					break;
			}


			return settings;
		}
		private BXUserFieldPublicEditor ResolveDefaultFieldEditor(string fieldName)
		{
			switch (fieldName)
			{
				case "LastName":
				case "FirstName":
				case "SecondName":
				case "DisplayName":
					return new BXUserFieldEditor<string>(fieldName, new BXTextFieldEditor());
				case "BirthdayDate":
					return new BXUserFieldEditor<DateTime>(fieldName, new BXDateTimeFieldEditor());
				case "Image":
					return new BXUserFieldEditor<int>("ImageId", new BXImageFieldEditor("main", "user"));
				case "Gender":
					return new BXUserFieldEditor<string>("Gender", new BXListFieldEditor(true, true));
				case "Password":
					return new PasswordEditor(this);
				case "Email":
					return new EmailEditor(this);
			}
			throw new InvalidOperationException("Unknown field");
		}

		private List<FieldInfo> GetAvailableFields()
		{
			List<FieldInfo> result = new List<FieldInfo>();

			result.Add(new FieldInfo("Email", GetMessageRaw("Field.Email")));

			result.Add(new FieldInfo("LastName", GetMessageRaw("Field.LastName")));
			result.Add(new FieldInfo("FirstName", GetMessageRaw("Field.FirstName")));
			result.Add(new FieldInfo("SecondName", GetMessageRaw("Field.SecondName")));
			result.Add(new FieldInfo("Gender", GetMessageRaw("Field.Gender")));

			result.Add(new FieldInfo("BirthdayDate", GetMessageRaw("Field.BirthdayDate")));
			result.Add(new FieldInfo("DisplayName", GetMessageRaw("Field.DisplayName")));
			result.Add(new FieldInfo("Image", GetMessageRaw("Field.Image")));
			result.Add(new FieldInfo("Password", GetMessageRaw("Field.Password")));


			foreach (BXUserProfileExtensionProvider p in BXUser.GetProfileExtensionProviders())
				foreach (BXUserProfileFieldInfo f in p.GetAvailableFields())
				{
					if (f.IsReadOnly)
						continue;
					FieldInfo fi = new FieldInfo();
					fi.Id = f.Name;
					fi.ClientId = p.UniqueKey + "_" + f.Name;
					fi.Title = f.Title;
					fi.Provider = p;
					result.Add(fi);
				}

			foreach (BXCustomField f in CustomFields)
			{
				if (f.CorrectedName.Equals(BXConfigurationUtility.Options.User.LiveIDCustomFieldCode, StringComparison.OrdinalIgnoreCase) ||
										f.CorrectedName.Equals(BXConfigurationUtility.Options.User.OpenIDCustomFieldCode, StringComparison.OrdinalIgnoreCase))
					continue;
				result.Add(new CustomFieldInfo(f));
			}

			return result;
		}

		private List<string> GetRequiredFields()
		{
			List<string> result = Parameters.GetListString("RequiredFields");
			result.Sort();
			return result;
		}
		private List<string> GetOwnerFields()
		{
			var result = Parameters.GetListString("OwnerFields");
			result.Sort();
			return result;
		}
		private Dictionary<string, FieldInfo> FillAvailableFields()
		{
			Dictionary<string, FieldInfo> index = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
			foreach (FieldInfo fi in AvailableFieldsList)
				index.Add(fi.ClientId, fi);
			return index;
		}

		public void AttachLiveId()
		{
			if (liveIdAuthField == null)
				return;
			if (!CanModifyExternalAuth)
			{
				GlobalErrors.Add(GetMessage("CanNotModifyUserLiveId"));
				hasErrors = true;
				return;
			}

			var appId = BXConfigurationUtility.Options.LiveIDApplicationID;
			var key = BXConfigurationUtility.Options.LiveIDSecretKey;
			if (BXStringUtility.IsNullOrTrimEmpty(key) || BXStringUtility.IsNullOrTrimEmpty(appId))
			{
				GlobalErrors.Add(GetMessage("LiveIdError.EmptyLiveIdAppIdOrKey"));
				hasErrors = true;
				return;
			}

			WindowsLiveLogin liveLogin =
					new WindowsLiveLogin(
						appId,
						key);

			//liveLogin.ReturnUrl = BXSefUrlManager.CurrentUrl.ToString();
			var context = ClientID + ";" + BXSefUrlManager.CurrentUrl.ToString() + ";" + BXCsrfToken.TokenKey + ":" + BXCsrfToken.GenerateToken();

			Response.Redirect(liveLogin.GetLoginUrl(context));
		}

		public void AttachOpenId()
		{
			//Invoke Validate
			//Session.Remove("AllowLogin");
			if (openIdAuthField == null)
				return; // handle error
			if (!CanModifyExternalAuth)
			{
				GlobalErrors.Add(GetMessage("CanNotModifyUserOpenId"));
				hasErrors = true;
				return;
			}

			// если данный openid уже привязан, не нужно ничего проверять, можно сразу выкинуть ошибку
			var identity = Request.Form[openIdAuthField.FormFieldName];
			if (identity != null)
			{
				BXUser u = null;
				try
				{
					u = BXUser.GetByOpenID(identity);
				}
				catch (Exception ex)
				{
					hasErrors = true;
					GlobalErrors.Add(GetMessage("Error.OpenIdFieldNotFound"));
					return;
				}
				if (u != null)
				{
					hasErrors = true;
					GlobalErrors.Add(GetMessage("OpenIdError.UserWithOpenIdAlreadyExists"));
					return;
				}
			}
			List<string> validateErrors = new List<string>();
			if (!openIdAuthField.Editor.Validate(openIdAuthField.FormFieldName, validateErrors))
			{
				hasErrors = true;
				GlobalErrors.AddRange(validateErrors);
				//handle errors
			}
		}

		public void Save()
		{
			if (!string.IsNullOrEmpty(FatalError))
				return;

			//Password Field Crutch
			if (passwordController != null)
				passwordController.ReadData();

			//Invoke Validate
			List<string> validateErrors = null;
			foreach (Field field in Fields.Values)
			{
				validateErrors = validateErrors ?? new List<string>();
				validateErrors.Clear();
				if (!field.Editor.Validate(field.FormFieldName, validateErrors))
				{
					hasErrors = true;
					field.ValidateErrors = validateErrors;
					validateErrors = null;
				}
			}

			if (hasErrors)
				return;

			try
			{
				//Invoke Save
				foreach (Field field in Fields.Values)
					field.Editor.Save(field.FormFieldName, User, field.IsCustom ? User.CustomValues : null);

				//Update Element
				foreach (BXUserProfilePublicFacade facade in loadedExtensions.Values)
					facade.User = User;

				if (BeforeSave != null)
				{
					var args = new BeforeSaveEventArgs(User, LoadedExtensions);
					BeforeSave(this, args);
					if (args.Cancel)
					{
						if (args.errors != null && args.errors.Count > 0)
							GlobalErrors.AddRange(args.errors);
						return;
					}
				}

				User.Update();
				foreach (BXUserProfilePublicFacade facade in loadedExtensions.Values)
					facade.Save(null);
			}
			catch (Exception)
			{
				GlobalErrors.Add(GetMessage("Message.ProfileUpdateError"));
				hasErrors = true;
			}

			if (hasErrors)
				return;

			IsSaved = true;

			if (AfterSave != null)
			{
				var e = new AfterSaveEventArgs(User, LoadedExtensions, DefaultSuccessMessage);
				AfterSave(this, e);
				successMessage = e.SuccessMessageHtml.ToString();
			}

			//Redirect
			string redirectPage = RedirectPageUrl;
			if (!BXStringUtility.IsNullOrTrimEmpty(redirectPage))
			{
				if (redirectPage.StartsWith("~/"))
					redirectPage = ResolveUrl(redirectPage);

				Response.Redirect(redirectPage);
			}
		}


		//NESTED CLASSES
		private sealed class CustomFieldInfo : FieldInfo
		{
			private BXCustomField field;

			public override string Title
			{
				get
				{
					return base.Title ?? (base.Title = field.EditFormLabel);
				}
				set
				{
					base.Title = value;
				}
			}

			public CustomFieldInfo(BXCustomField field)
			{
				Id = field.Name;
				ClientId = "Property_" + field.Name;
				IsCustom = true;

				this.field = field;
			}
		}

		internal class FieldInfo
		{
			private string title;

			public string Id;
			public string ClientId;
			public BXUserProfileExtensionProvider Provider;
			public bool IsCustom;

			public virtual string Title
			{
				get
				{
					return title;
				}
				set
				{
					title = value;
				}
			}

			public FieldInfo()
			{

			}
			public FieldInfo(string normalId, string title)
			{
				Id = ClientId = normalId;
				this.title = title;
			}
		}
		public sealed class Field
		{
			private string id;
			private string formFieldName;
			private string uniqueId;
			private bool required;
			private List<string> validateErrors;
			private BXUserFieldPublicEditor editor;
			private BXCustomField customField;
			private BXCustomType customType;
			internal string RawTitle;
			internal bool IsCustom;
			internal BXParamsBag<object> Settings;

			public string Id
			{
				get
				{
					return id;
				}
				internal set
				{
					id = value;
				}
			}

			public string Title
			{
				get
				{
					return RawTitle != null ? HttpUtility.HtmlEncode(RawTitle) : null;
				}
			}
			public string FormFieldName
			{
				get
				{
					return formFieldName;
				}
				set
				{
					formFieldName = value;
				}
			}
			public string UniqueId
			{
				set
				{
					uniqueId = value;
				}
				get
				{
					return uniqueId;
				}
			}
			public bool IsRequired
			{
				set
				{
					required = value;
				}
				get
				{
					return required;
				}
			}
			public List<string> ValidateErrors
			{
				get
				{
					return validateErrors;
				}
				set
				{
					validateErrors = value;
				}
			}
			public BXUserFieldPublicEditor Editor
			{
				set
				{
					editor = value;
				}
				get
				{
					return editor;
				}
			}
			public BXCustomField CustomField
			{
				get
				{
					return customField;
				}
				internal set
				{
					customField = value;
				}
			}
			public BXCustomType CustomType
			{
				get
				{
					return customType;
				}
				internal set
				{
					customType = value;
				}
			}

			internal Field(FieldInfo info)
			{
				Id = info.ClientId;
				RawTitle = info.Title;
				IsCustom = info.IsCustom;
			}

			public string Render()
			{
				if (editor != null)
					return editor.Render(FormFieldName, UniqueId);
				else
					return string.Empty;
			}
			public void Render(HtmlTextWriter writer)
			{
				if (editor != null)
					editor.Render(FormFieldName, UniqueId, writer);
			}
		}
		public sealed class FieldGroup
		{
			private string title;
			internal List<Field> fields;
			bool? hasErrors;

			public string Title
			{
				get
				{
					return title;
				}
				set
				{
					title = value;
				}
			}
			public List<Field> Fields
			{
				get
				{
					return fields ?? (fields = new List<Field>());
				}
			}
			public bool IsEmpty
			{
				get
				{
					return fields == null || fields.Count == 0;
				}
			}
			public IEnumerable<string> ErrorSummary
			{
				get
				{
					if (!IsEmpty)
						foreach (Field field in fields)
							if (field.ValidateErrors != null && field.ValidateErrors.Count != 0)
								foreach (string error in field.ValidateErrors)
									yield return error;
				}
			}
			public bool HasErrors
			{
				get
				{
					if (hasErrors != null)
						return hasErrors.Value;

					if (!IsEmpty)
						foreach (Field field in fields)
							if (field.ValidateErrors != null && field.ValidateErrors.Count != 0)
								return (hasErrors = true).Value;

					return (hasErrors = false).Value;
				}
			}
		}

		public sealed class CreateCustomFieldEditorEventArgs : EventArgs
		{
			private string id;
			private string formFieldName;
			private string title;
			private BXCustomType type;
			private BXCustomField field;
			private BXCustomTypePublicEdit editor;

			internal CreateCustomFieldEditorEventArgs(string id, string title, string formFieldName, BXCustomField field, BXCustomType type)
			{
				this.id = id;
				this.title = title;
				this.formFieldName = formFieldName;
				this.field = field;
				this.type = type;
			}

			public string Id
			{
				get
				{
					return id;
				}
			}
			public string FormFieldName
			{
				get
				{
					return formFieldName;
				}
			}
			public string Title
			{
				get
				{
					return title;
				}
				set
				{
					title = value;
				}
			}
			public BXCustomType CustomType
			{
				get
				{
					return type;
				}
			}
			public BXCustomField CustomField
			{
				get
				{
					return field;
				}
			}
			public BXCustomTypePublicEdit Editor
			{
				get
				{
					return editor;
				}
				set
				{
					editor = value;
				}
			}
		}
		public sealed class CreateUserFieldEditorEventArgs : EventArgs
		{
			private BXUserFieldPublicEditor editor;
			private string id;
			private string formFieldName;
			private string title;
			private BXParamsBag<object> settings;
			private bool required;

			internal CreateUserFieldEditorEventArgs(string id, string title, string formFieldName, BXParamsBag<object> settings, bool required)
			{
				this.id = id;
				this.title = title;
				this.formFieldName = formFieldName;
				this.settings = settings;
				this.required = required;
			}

			public string Id
			{
				get
				{
					return id;
				}
			}
			public string FormFieldName
			{
				get
				{
					return formFieldName;
				}
			}
			public string Title
			{
				get
				{
					return title;
				}
				set
				{
					title = value;
				}
			}
			public BXParamsBag<object> Settings
			{
				get
				{
					return settings;
				}
			}
			public bool Required
			{
				get
				{
					return required;
				}
			}
			public BXUserFieldPublicEditor Editor
			{
				get
				{
					return editor;
				}
				set
				{
					editor = value;
				}
			}
		}
		public sealed class CreateProfileFieldEditorEventArgs : EventArgs
		{
			private string id;
			private string formFieldName;
			private string title;
			private BXParamsBag<object> settings;
			private BXUserProfileExtensionProvider provider;
			private BXUserProfilePublicFacade profile;
			private BXUserProfileFieldPublicEditor editor;

			internal CreateProfileFieldEditorEventArgs(string id, string title, string formFieldName, BXParamsBag<object> settings, BXUserProfileExtensionProvider provider, BXUserProfilePublicFacade profile)
			{
				this.id = id;
				this.title = title;
				this.formFieldName = formFieldName;
				this.settings = settings;
				this.provider = provider;
				this.profile = profile;
			}

			public string Id
			{
				get
				{
					return id;
				}
			}
			public string FormFieldName
			{
				get
				{
					return formFieldName;
				}
			}
			public string Title
			{
				get
				{
					return title;
				}
				set
				{
					title = value;
				}
			}
			public BXParamsBag<object> Settings
			{
				get
				{
					return settings;
				}
			}
			public BXUserProfileExtensionProvider Provider
			{
				get
				{
					return provider;
				}
			}
			public BXUserProfilePublicFacade Profile
			{
				get
				{
					return profile;
				}
			}
			public BXUserProfileFieldPublicEditor Editor
			{
				get
				{
					return editor;
				}
				set
				{
					editor = value;
				}
			}
		}
		public sealed class BeforeSaveEventArgs : EventArgs
		{
			internal BeforeSaveEventArgs(BXUser user, ICollection<BXUserProfilePublicFacade> extensions)
			{
				this.user = user;
				this.extensions = extensions;
			}

			private BXUser user;
			public BXUser User
			{
				get
				{
					return user;
				}
			}

			private ICollection<BXUserProfilePublicFacade> extensions;
			public ICollection<BXUserProfilePublicFacade> Extensions
			{
				get
				{
					return extensions;
				}
			}

			public bool Cancel { get; set; }

			internal List<string> errors;
			public ICollection<string> Errors
			{
				get
				{
					return errors ?? (errors = new List<string>());
				}
			}
		}
		public sealed class AfterSaveEventArgs : EventArgs
		{
			internal AfterSaveEventArgs(BXUser user, ICollection<BXUserProfilePublicFacade> facades, string successMessageHtml)
			{
				this.user = user;
				this.extensions = extensions;
				this.successMessageHtml = new StringBuilder(successMessageHtml);
			}

			private BXUser user;
			public BXUser User
			{
				get
				{
					return user;
				}
			}

			private ICollection<BXUserProfilePublicFacade> extensions;
			public ICollection<BXUserProfilePublicFacade> Extensions
			{
				get
				{
					return extensions;
				}
			}

			private StringBuilder successMessageHtml;
			public StringBuilder SuccessMessageHtml
			{
				get { return successMessageHtml; }
			}
		}


		private sealed class ProfileFieldEditor : BXUserFieldPublicEditor
		{
			BXUserProfileFieldPublicEditor editor;
			BXUserProfilePublicFacade facade;

			public ProfileFieldEditor(BXUserProfileFieldPublicEditor editor, BXUserProfilePublicFacade facade)
			{
				this.editor = editor;
				this.facade = facade;
			}

			public override void Load(BXUser user, BXCustomPropertyCollection properties, BXParamsBag<object> settings)
			{
				editor.Load(facade, settings);
			}

			public override string Render(string formFieldName, string uniqueID)
			{
				return editor.Render(formFieldName, uniqueID);
			}
			public override void Render(string formFieldName, string uniqueID, HtmlTextWriter writer)
			{
				editor.Render(formFieldName, uniqueID, writer);
			}
			public override void Save(string formFieldName, BXUser user, BXCustomPropertyCollection properties)
			{
				editor.Save(formFieldName, facade);
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				return editor.Validate(formFieldName, errors);
			}
		}
		private sealed class CustomFieldEditor : BXUserFieldPublicEditor
		{
			BXCustomTypePublicEdit editor;
			BXCustomField field;

			public CustomFieldEditor(BXCustomField field, BXCustomTypePublicEdit publicEditor)
			{
				if (field == null)
					throw new ArgumentNullException("field");
				this.field = field;

				if (publicEditor == null)
				{
					BXCustomType type = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
					publicEditor = type.CreatePublicEditor();
				}
				publicEditor.Init(field);
				this.editor = publicEditor;
			}

			public override void Load(BXUser user, BXCustomPropertyCollection properties, BXParamsBag<object> settings)
			{
				if (user == null)
					return;

				BXCustomProperty value;
				if (properties != null && properties.TryGetValue(field.CorrectedName, out value))
					editor.Load(value);
			}
			public override string Render(string formFieldName, string uniqueID)
			{
				return editor.Render(formFieldName, uniqueID);
			}
			public override void Render(string formFieldName, string uniqueID, HtmlTextWriter writer)
			{
				editor.Render(formFieldName, uniqueID, writer);
			}
			public override void Save(string formFieldName, BXUser user, BXCustomPropertyCollection properties)
			{
				editor.DoSave(formFieldName, properties, null);
			}
			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				return editor.DoValidate(formFieldName, errors);
			}
		}
		private sealed class PasswordEditorController
		{
			BXComponent parent;
			string oldPassword;
			string newPassword;
			string confirmation;
			bool empty;

			PasswordOldEditor oldEditor;
			PasswordNewEditor newEditor;
			PasswordConfirmEditor confirmEditor;

			Field oldField;
			Field newField;
			Field confirmField;

			public PasswordEditorController(BXComponent parent)
			{
				this.parent = parent;
			}

			public void ReadData()
			{
				if (oldField != null && oldEditor != null)
					oldEditor.ReadData(oldField.FormFieldName);
				if (newField != null && newEditor != null)
					newEditor.ReadData(newField.FormFieldName);
				if (confirmField != null && confirmEditor != null)
					confirmEditor.ReadData(confirmField.FormFieldName);

				empty = string.IsNullOrEmpty(oldPassword) && string.IsNullOrEmpty(newPassword) && string.IsNullOrEmpty(confirmation);
			}

			public BXUserFieldPublicEditor ProvideOldEditor(Field field)
			{
				oldField = field;
				return oldEditor ?? (oldEditor = new PasswordOldEditor(this));
			}

			public BXUserFieldPublicEditor ProvideNewEditor(Field field)
			{
				newField = field;
				return newEditor ?? (newEditor = new PasswordNewEditor(this));
			}

			public BXUserFieldPublicEditor ProvideConfirmEditor(Field field)
			{
				confirmField = field;
				return confirmEditor ?? (confirmEditor = new PasswordConfirmEditor(this));
			}

			private sealed class PasswordOldEditor : BXUserFieldPublicEditor
			{
				PasswordEditorController parent;
				BXUser user;

				public PasswordOldEditor(PasswordEditorController parent)
				{
					this.parent = parent;
				}

				public override void Load(BXUser user, BXCustomPropertyCollection properties, BXParamsBag<object> settings)
				{
					this.user = user;
				}

				public override void Render(string formFieldName, string uniqueID, HtmlTextWriter writer)
				{
					writer.Write(@"<input name=""{0}"" type=""password"" value=""{1}"" autocomplete=""off"" />", formFieldName, parent.oldPassword);
				}

				public override void Save(string formFieldName, BXUser user, BXCustomPropertyCollection properties)
				{
				}

				public void ReadData(string formFieldName)
				{
					parent.oldPassword = HttpContext.Current.Request.Form[formFieldName];
				}

				public override bool Validate(string formFieldName, ICollection<string> errors)
				{
					if (parent.empty)
						return true;

					if (!BXUserManager.ValidateUser(user.UserName, user.ProviderName, parent.oldPassword))
						errors.Add(parent.parent.GetMessageRaw("PasswordEditor.InvalidPassword"));

					return errors.Count == 0;
				}
			}
			private sealed class PasswordNewEditor : BXUserFieldPublicEditor
			{
				PasswordEditorController parent;
				string title;
				bool required;
				BXUser user;

				public PasswordNewEditor(PasswordEditorController parent)
				{
					this.parent = parent;
				}

				public override void Load(BXUser user, BXCustomPropertyCollection properties, BXParamsBag<object> settings)
				{
					this.user = user;
					this.required = settings.GetBool("required");
					this.title = settings.GetString("fieldTitle");
				}

				public override void Render(string formFieldName, string uniqueID, HtmlTextWriter writer)
				{
					writer.Write(@"<input name=""{0}"" type=""password"" value="""" autocomplete=""off"" />", formFieldName);
				}

				public override void Save(string formFieldName, BXUser user, BXCustomPropertyCollection properties)
				{
					if (!parent.empty)
						BXUserManager.ChangePassword(user.UserId, parent.oldPassword, parent.newPassword);
					parent.oldPassword = null;
				}

				public void ReadData(string formFieldName)
				{
					parent.newPassword = HttpContext.Current.Request.Form[formFieldName];
				}

				public override bool Validate(string formFieldName, ICollection<string> errors)
				{
					if (parent.empty)
					{
						if (required)
						{
							errors.Add(string.Format(parent.parent.GetMessageRaw("PasswordEditor.FieldIsEmpty"), title));
							return false;
						}
						return true;
					}

					if (string.IsNullOrEmpty(parent.newPassword))
						errors.Add(parent.parent.GetMessageRaw("PasswordEditor.NewPasswordIsMissing"));

					MembershipProvider p;
					if (!string.IsNullOrEmpty(parent.newPassword) && (p = BXUserManager.GetProvider(user.ProviderName)) != null)
					{
						BXUserPasswordPolicyError er = BXUserManager.CheckPasswordPolicy(user.ProviderName, parent.newPassword);
						if ((er & BXUserPasswordPolicyError.Length) != BXUserPasswordPolicyError.None)
							errors.Add(string.Format(parent.parent.GetMessageRaw("PasswordEditor.PasswordToSmall"), p.MinRequiredPasswordLength));
						if ((er & BXUserPasswordPolicyError.NonAlphanumericsCount) != BXUserPasswordPolicyError.None)
							errors.Add(string.Format(parent.parent.GetMessageRaw("PasswordEditor.PasswordToLittleAlphanumerics"), p.MinRequiredNonAlphanumericCharacters));
						if ((er & BXUserPasswordPolicyError.StrengthRegex) != BXUserPasswordPolicyError.None)
							errors.Add(parent.parent.GetMessageRaw("PasswordEditor.PasswordIsWeak"));
					}
					return errors.Count == 0;
				}
			}
			private sealed class PasswordConfirmEditor : BXUserFieldPublicEditor
			{
				PasswordEditorController parent;

				BXUser user;

				public PasswordConfirmEditor(PasswordEditorController parent)
				{
					this.parent = parent;
				}

				public override void Load(BXUser user, BXCustomPropertyCollection properties, BXParamsBag<object> settings)
				{
					this.user = user;
				}

				public override void Render(string formFieldName, string uniqueID, HtmlTextWriter writer)
				{
					writer.Write(@"<input name=""{0}"" type=""password"" value="""" autocomplete=""off"" />", formFieldName);
				}

				public override void Save(string formFieldName, BXUser user, BXCustomPropertyCollection properties)
				{
				}

				public void ReadData(string formFieldName)
				{
					parent.confirmation = HttpContext.Current.Request.Form[formFieldName];
				}

				public override bool Validate(string formFieldName, ICollection<string> errors)
				{
					if (parent.empty)
						return true;

					if (parent.newPassword != parent.confirmation)
						errors.Add(parent.parent.GetMessageRaw("PasswordEditor.PasswordsDontMatch"));

					return errors.Count == 0;
				}
			}
		}


		public sealed class LiveIdEditor : BXUserFieldPublicEditor
		{
			string value;
			bool required;
			string buttonText;
			string title;
			string confirmMessage;
			BXComponent parent;
			bool attach;
			bool canModify;
			bool showConfirmMessage;
			string buttonClassName;

			public string Value
			{
				get { return value; }
				set { this.value = value; }
			}

			public string ButtonText
			{
				get { return buttonText; }
				set { buttonText = value; }
			}

			public bool ShowConfirmMessage
			{
				get { return showConfirmMessage; }
				set { showConfirmMessage = value; }
			}

			public bool IsAttach
			{
				get { return attach; }
				set { attach = value; }
			}

			public string ButtonClassName
			{
				get { return buttonClassName; }
				set { buttonClassName = value; }
			}

			public string ConfirmMessage
			{
				get { return confirmMessage; }
				set { confirmMessage = value; }
			}

			public LiveIdEditor(string title, string buttonText, BXComponent parent, bool attach, bool canModify)
			{
				this.buttonText = buttonText;
				this.parent = parent;
				this.attach = attach;
				this.canModify = canModify;
				this.title = title;
			}

			public override void Load(BXUser user, BXCustomPropertyCollection properties, BXParamsBag<object> settings)
			{
				this.required = settings.GetBool("required");
			}

			public override void Render(string formFieldName, string uniqueID, HtmlTextWriter writer)
			{
				string s =
					this.parent.Page.ClientScript.GetPostBackEventReference(this.parent, attach ? "AttachLiveId" : "DetachLiveId") + ";";

				writer.Write(@"<button {4} type=""submit"" onclick=""{3}"" {2}/>{0}</button>",
					buttonText, attach ? "AttachLiveId" : "DetachLiveId", canModify ? "" : @"disabled=""true""",
					!attach && showConfirmMessage ? "if (window.confirm('" + ConfirmMessage + "')) " + s + " else return false;" : s,
					!BXStringUtility.IsNullOrTrimEmpty(buttonClassName) ? "class=\"" + buttonClassName + "\"" : "");
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				return true;
			}

			public override void Save(string formFieldName, BXUser user, BXCustomPropertyCollection properties)
			{
			}
		}
		public sealed class OpenIdEditor : BXUserFieldPublicEditor
		{
			string title;
			string value;
			string confirmMessage;
			bool required;
			string buttonText;
			BXComponent parent;
			bool attach;
			bool canModify;
			bool showConfirmMessage;
			string fieldClassName;
			string buttonClassName;

			public string Value
			{
				get { return value; }
				set { this.value = value; }
			}

			public string ButtonText
			{
				get { return buttonText; }
				set { buttonText = value; }
			}

			public bool IsAttach
			{
				get { return attach; }
				set { attach = value; }
			}

			public string FieldClassName
			{
				get { return fieldClassName; }
				set { fieldClassName = value; }
			}

			public string ButtonClassName
			{
				get { return buttonClassName; }
				set { buttonClassName = value; }
			}

			public bool ShowConfirmMessage
			{
				get { return showConfirmMessage; }
				set { showConfirmMessage = value; }
			}

			public string ConfirmMessage
			{
				get { return confirmMessage; }
				set { confirmMessage = value; }
			}

			public OpenIdEditor(string title, string val, string buttonText, BXComponent parent, bool attach, bool canModify)
			{
				this.title = title;
				this.value = val;
				this.buttonText = buttonText;
				this.parent = parent;
				this.attach = attach;
				this.canModify = canModify;
			}

			public override void Load(BXUser user, BXCustomPropertyCollection properties, BXParamsBag<object> settings)
			{
				this.required = settings.GetBool("required");
			}

			public override void Render(string formFieldName, string uniqueID, HtmlTextWriter writer)
			{
				string s =
					this.parent.Page.ClientScript.GetPostBackEventReference(this.parent, attach ? "AttachOpenId" : "DetachOpenId") + ";";

				writer.Write(@"<input name=""{0}"" {4} type=""text"" onkeypress=""return {3}OpenIdFireDefaultButton(event)""
         maxlength=""255"" size=""30"" value=""{1}"" {2} />",
															formFieldName,
															value,
															attach ? string.Empty : @"disabled=""disabled""",
															parent.ClientID,
															!BXStringUtility.IsNullOrTrimEmpty(fieldClassName) ? "class=\"" + fieldClassName + "\"" : ""
															);
				writer.Write(@"<button {3} onclick=""{2}"" {1}/>{0}</button>",
					buttonText, canModify ? "" : @"disabled=""true""",
					!attach && showConfirmMessage ? "if (window.confirm('" + ConfirmMessage + "')) " + s + " else return false;" : s,
					!BXStringUtility.IsNullOrTrimEmpty(buttonClassName) ? "class=\"" + buttonClassName + "\"" : "");
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				string identity = parent.Request.Form[formFieldName];
				if (BXStringUtility.IsNullOrTrimEmpty(identity) && attach)
				{
					errors.Add(parent.GetMessage("OpenIdError.IdentityIsEmpty"));
					return false;
				}
				else if (!attach)
				{
					return true;
				}

				var openId = new OpenIdClient();
				openId.Identity = identity.Replace(" ", "");
				NameValueCollection queryParams = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
				queryParams.Remove("BXOpenIdAuth_ComponentID");
				queryParams.Add("BXOpenIdAuth_ComponentID", parent.ClientID);
				queryParams.Remove(BXCsrfToken.TokenKey);
				queryParams.Add(BXCsrfToken.TokenKey, BXCsrfToken.GenerateToken());
				UriBuilder ub = new UriBuilder(BXSefUrlManager.CurrentUrl);
				ub.Query = queryParams.ToString();
				openId.ReturnUrl = ub.Uri;
				openId.CreateRequest();

				if ((openId.ErrorState & ErrorCondition.NoServersFound) == ErrorCondition.NoServersFound)
				{

					errors.Add(parent.GetMessage("OpenIdError.OpenIdProviderNotFound"));
					return false;
				}
				else if ((openId.ErrorState & ErrorCondition.NoErrors) != ErrorCondition.NoErrors)
				{
					errors.Add(parent.GetMessage("OpenIdError.OpenIdAuthFailed"));
					return false;
				}

				return true;
			}

			public override void Save(string formFieldName, BXUser user, BXCustomPropertyCollection properties)
			{
			}
		}
		private sealed class PasswordEditor : BXUserFieldPublicEditor
		{
			BXComponent parent;
			string oldPassword;
			string newPassword;
			string confirmation;
			bool empty;
			bool required;
			string title;
			BXUser user;

			public PasswordEditor(BXComponent parent)
			{
				this.parent = parent;
			}

			public override void Load(BXUser user, BXCustomPropertyCollection properties, BXParamsBag<object> settings)
			{
				this.user = user;
				this.required = settings.GetBool("required");
				this.title = settings.GetString("fieldTitle");
			}

			public override void Render(string formFieldName, string uniqueID, HtmlTextWriter writer)
			{
				writer.WriteEncodedText(parent.GetMessageRaw("PasswordEditor.OldPassword"));
				writer.Write(@"<br/><input name=""{0}$o"" type=""password"" value=""{1}"" autocomplete=""off"" /><br/>", formFieldName, oldPassword);
				writer.WriteEncodedText(parent.GetMessageRaw("PasswordEditor.NewPassword"));
				writer.Write(@"<br/><input name=""{0}$n"" type=""password"" value="""" autocomplete=""off"" /><br/>", formFieldName);
				writer.WriteEncodedText(parent.GetMessageRaw("PasswordEditor.Confirmation"));
				writer.Write(@"<br/><input name=""{0}$c"" type=""password"" value="""" autocomplete=""off"" /><br/>", formFieldName);
			}

			public override void Save(string formFieldName, BXUser user, BXCustomPropertyCollection properties)
			{
				if (!empty)
					BXUserManager.ChangePassword(user.UserId, oldPassword, newPassword);
				oldPassword = null;
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				oldPassword = HttpContext.Current.Request.Form[formFieldName + "$o"];
				newPassword = HttpContext.Current.Request.Form[formFieldName + "$n"];
				confirmation = HttpContext.Current.Request.Form[formFieldName + "$c"];
				empty = string.IsNullOrEmpty(oldPassword) && string.IsNullOrEmpty(newPassword) && string.IsNullOrEmpty(confirmation);
				if (empty)
				{
					if (required)
					{
						errors.Add(string.Format(parent.GetMessageRaw("PasswordEditor.FieldIsEmpty"), title));
						return false;
					}
					return true;
				}

				if (!BXUserManager.ValidateUser(user.UserName, user.ProviderName, oldPassword))
					errors.Add(parent.GetMessageRaw("PasswordEditor.InvalidPassword"));
				if (string.IsNullOrEmpty(newPassword))
					errors.Add(parent.GetMessageRaw("PasswordEditor.NewPasswordIsMissing"));
				else if (newPassword != confirmation)
					errors.Add(parent.GetMessageRaw("PasswordEditor.PasswordsDontMatch"));

				MembershipProvider p;
				if (!string.IsNullOrEmpty(newPassword) && (p = BXUserManager.GetProvider(user.ProviderName)) != null)
				{
					BXUserPasswordPolicyError er = BXUserManager.CheckPasswordPolicy(user.ProviderName, newPassword);
					if ((er & BXUserPasswordPolicyError.Length) != BXUserPasswordPolicyError.None)
						errors.Add(string.Format(parent.GetMessageRaw("PasswordEditor.PasswordToSmall"), p.MinRequiredPasswordLength));
					if ((er & BXUserPasswordPolicyError.NonAlphanumericsCount) != BXUserPasswordPolicyError.None)
						errors.Add(string.Format(parent.GetMessageRaw("PasswordEditor.PasswordToLittleAlphanumerics"), p.MinRequiredNonAlphanumericCharacters));
					if ((er & BXUserPasswordPolicyError.StrengthRegex) != BXUserPasswordPolicyError.None)
						errors.Add(parent.GetMessageRaw("PasswordEditor.PasswordIsWeak"));
				}
				return errors.Count == 0;
			}
		}
		private sealed class EmailEditor : BXUserFieldPublicEditor
		{
			UserProfileEditComponent parent;
			bool required;
			string original;
			string value;
			bool waitForConfirmation;

			public EmailEditor(UserProfileEditComponent parent)
			{
				this.parent = parent;
			}

			public override void Load(BXUser user, BXCustomPropertyCollection properties, BXParamsBag<object> settings)
			{
				this.required = settings.GetBool("required");
				//this.title = settings.GetString("fieldTitle");
				this.value = this.original = (user.Email ?? "").Trim();
				this.waitForConfirmation =
					BXConfigurationUtility.Options.SendConfirmationRequest
					&& BXUserToken.GetList(
						new BXFilter(
							new BXFilterItem(BXUserToken.Fields.UserId, BXSqlFilterOperators.Equal, user.UserId),
							new BXFilterItem(BXUserToken.Fields.Type, BXSqlFilterOperators.Equal, BXUserTokenType.EmailChange),
							new BXFilterItem(BXUserToken.Fields.DateExpiresUtc, BXSqlFilterOperators.GreaterOrEqual, DateTime.UtcNow)
						),
						null,
						new BXSelect(BXUserToken.Fields.Id),
						new BXQueryParams(BXPagingOptions.Top(1))
					)
					.FirstOrDefault() != null;
			}

			public override void Render(string formFieldName, string uniqueID, HtmlTextWriter writer)
			{
				writer.Write(@"<input name=""{0}"" id=""{1}"" type=""text"" value=""{2}"" autocomplete=""off"" />", formFieldName, uniqueID, value);
				if (waitForConfirmation)
					writer.Write(@"<br/><small>{0}</small>", parent.GetMessageRaw("Note.WaitingForEmailConfirmation"));
			}

			public override void Save(string formFieldName, BXUser user, BXCustomPropertyCollection properties)
			{
				if (string.IsNullOrEmpty(value) || string.Equals(original, value, StringComparison.InvariantCultureIgnoreCase))
					return;

				if (!BXConfigurationUtility.Options.SendConfirmationRequest)
					user.Email = value;
				else
					parent.AfterSave += OnAfterSave;					
			}

			void OnAfterSave(object sender, AfterSaveEventArgs e)
			{
				try
				{
					var token = BXUserToken.RegisterToken(e.User.UserId, BXUserTokenType.EmailChange, TimeSpan.FromDays(BXConfigurationUtility.Options.IntervalToStoreUnconfirmedUsers), value, null);

					var cmd = new BXCommand("Bitrix.Main.EmailChangeRequest");
					cmd.Parameters["DISPLAY_NAME"] = e.User.GetDisplayName();
					cmd.Parameters["LOGIN"] = e.User.UserName;
					cmd.Parameters["LAST_NAME"] = e.User.LastName;
					cmd.Parameters["FIRST_NAME"] = e.User.FirstName;
					cmd.Parameters["EMAIL"] = value;
					cmd.Parameters["OLD_EMAIL"] = e.User.Email;
					cmd.Parameters["CONFIRMATION_LINK"] = GenerateConfirmationLink(e.User.UserId, token);
					cmd.Parameters["USER_ID"] = e.User.UserId;
					cmd.Send();
				}
				catch
				{
					if (e.SuccessMessageHtml.Length > 0)
						e.SuccessMessageHtml.Append("<br/>");
					e.SuccessMessageHtml.Append(parent.GetMessageRaw("Message.UnableToSendConfirmationEmail"));
					return;
				}

				if (e.SuccessMessageHtml.Length > 0)
					e.SuccessMessageHtml.Append("<br/>");
				e.SuccessMessageHtml.Append(parent.GetMessageRaw("Message.ConfirmationEmailSent"));

				waitForConfirmation = true;
			}

			static string GenerateConfirmationLink(int userId, string token)
			{
				var url = new UriBuilder(BXSefUrlManager.CurrentUrl);
				var query = HttpUtility.ParseQueryString(url.Query);
				query["emailConfirmation"] = Convert.ToBase64String(Encoding.Unicode.GetBytes(string.Concat(
					userId.ToString(),
					",",
					token
				)));
				url.Query = query.ToString();

				return url.Uri.AbsoluteUri;
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				value = (parent.Request.Form[formFieldName] ?? "").Trim();
				if (required && string.IsNullOrEmpty(value))
				{
					errors.Add(parent.GetMessageRaw("EmailRequired"));
					return false;
				}

				if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, @"^[\w]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)*$"))
				{
					errors.Add(parent.GetMessageRaw("EmailInvalid"));
					return false;
				}

				if (string.IsNullOrEmpty(value) || string.Equals(original, value, StringComparison.InvariantCultureIgnoreCase))
					return true;

				if (BXUserManager.GetUserNameByEmail(value).Count > 0)
				{
					errors.Add(parent.GetMessageRaw("EmailAlreadyExists"));
					return false;
				}

				return true;
			}
		}
	}

	public class UserProfileEditTemplate : BXComponentTemplate<UserProfileEditComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			HtmlForm form;
			if (Page != null && (form = Page.Form) != null && form.Enctype.Length == 0)
				form.Enctype = "multipart/form-data";
		}

		public void SaveUserDefaultHandler(object sender, EventArgs e)
		{
			Component.Save();
		}
	}
}