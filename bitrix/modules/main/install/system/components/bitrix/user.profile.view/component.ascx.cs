using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Bitrix.Components;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.Services.User;
using Bitrix.UI;
using Bitrix.Services.Rating;
using System.Text;

namespace Bitrix.Main.Components
{
	public partial class UserProfileViewComponent : BXComponent
	{
		private BXCustomFieldCollection customFields;
		private List<FieldInfo> availableFieldsList;
		private Dictionary<string, FieldInfo> availableFields;
		private Dictionary<BXUserProfileExtensionProvider, BXUserProfilePublicFacade> loadedExtensions;
		private List<FieldGroup> groups;
		private Dictionary<string, Field> fields;
		private BXUser user;
		private int? userId;
		private BXParamsBag<object> replaceDictionary;
		private string profileEditUrl;
		private bool isProfileEditUrlBuilt;
		private string fatalError;
		private bool canModify;

		private BXCustomFieldCollection CustomFields
		{
			get
			{
				return customFields ?? (customFields = BXCustomEntityManager.GetFields(BXUser.GetCustomFieldsKey()));
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
				return Parameters.GetListString("Fields");
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
				return false;
			}
		}
		public bool CanModify
		{
			get
			{
				return canModify;
			}
		}
		public string EditProfileTitle
		{
			get
			{
				return Encode(Parameters.GetString("EditProfileTitle"));
			}
		}
		public string EditProfileUrl
		{
			get
			{
				if (!isProfileEditUrlBuilt)
				{
					isProfileEditUrlBuilt = true;
					profileEditUrl = Parameters.GetString("EditProfileUrlTemplate");
					if (profileEditUrl != null)
						profileEditUrl = ResolveTemplateUrl(profileEditUrl, ReplaceDictionary);
				}
				return profileEditUrl;
			}
		}

        private IList<string> rolesAuthorizedToVote = null;
        public IList<string> RolesAuthorizedToVote
        {
            get
            {
                return this.rolesAuthorizedToVote ?? (this.rolesAuthorizedToVote = Parameters.GetListString("RolesAuthorizedToVote"));
            }
            set
            {
                Parameters["RolesAuthorizedToVote"] = BXStringUtility.ListToCsv(this.rolesAuthorizedToVote = value ?? new List<string>());
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

		public event EventHandler<ProvideCustomFieldViewEventArgs> ProvideCustomFieldView;
		public event EventHandler<ProvideUserFieldViewEventArgs> ProvideUserFieldView;
		public event EventHandler<ProvideProfileFieldViewEventArgs> ProvideProfileFieldView;

		//METHODS
		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				loadedExtensions = new Dictionary<BXUserProfileExtensionProvider, BXUserProfilePublicFacade>();
				groups = new List<FieldGroup>();
				fields = new Dictionary<string, Field>();
				user = LoadUser();
				canModify = (user != null && BXUser.IsCanModify(user.UserId));

				IncludeComponentTemplate();

				if (user == null)
				{
					FatalError = GetMessage("Message.UserNotFound");
					return;
				}

				if (IsPermissionDenied)
					return;

				LoadFields();
			}
			catch (Exception ex)
			{
				BXLogService.LogAll(ex, 0, BXLogMessageType.Error, Name + "@" + (Page != null ? Page.AppRelativeVirtualPath : string.Empty));
			}
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = new BXComponentGroup("Auth", GetMessageRaw("Category"), 100, BXComponentGroup.Utility);

            BXCategory mainCategory = BXCategory.Main,
                addSettingsCategory = BXCategory.AdditionalSettings,
                votingCategory = new BXCategory(GetMessage("Category.Voting"), "Voting", 220);
			ParamsDefinition["UserId"] = new BXParamText(GetMessageRaw("Param.UserId"), "", mainCategory);
			ParamsDefinition["Fields"] = new BXParamDoubleListWithCustomAdd(GetMessageRaw("Param.Fields"), "", mainCategory);

			ParamsDefinition["EditProfileTitle"] = new BXParamText(GetMessageRaw("Param.EditProfileTitle"), "", addSettingsCategory);
			ParamsDefinition["EditProfileUrlTemplate"] = new BXParamText(GetMessageRaw("Param.EditProfileUrlTemplate"), "", addSettingsCategory);

            ParamsDefinition.Add("RolesAuthorizedToVote", new BXParamMultiSelection(GetMessageRaw("Param.RolesAuthorizedToVote"), "User", votingCategory));
		}
		protected override void LoadComponentDefinition()
		{
			BXParamDoubleListWithCustomAdd editFields = (BXParamDoubleListWithCustomAdd)ParamsDefinition["Fields"];
			editFields.AddButtonTitle = GetMessageRaw("Param.Fields.ButtonTitle");
			editFields.CustomValuePrefix = "@";
			editFields.CustomValueTextFormat = "- {0} -";
			editFields.Prompt = GetMessageRaw("Param.Fields.Prompt");
			editFields.DefaultCustomValue = GetMessageRaw("Param.Fields.DefaultName");
			editFields.Values = AvailableFieldsList.ConvertAll<BXParamValue>(delegate(FieldInfo input)
			{
				return new BXParamValue(input.Title, input.ClientId);
			});

            IList<BXParamValue> rolesValues = ParamsDefinition["RolesAuthorizedToVote"].Values;
            rolesValues.Clear();
            foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
            {
                if (string.Equals(r.RoleName, "Guest", StringComparison.Ordinal))
                    continue;
                rolesValues.Add(new BXParamValue(r.Title, r.RoleName));
            }
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
					null,
					null,
					BXTextEncoder.EmptyTextEncoder
				); 
				if (users.Count > 0)
					user = users[0];
			}
			
			return user;
		}
		private void LoadFields()
		{
			List<Field> currentFields = null;
			foreach (string f in SelectedFields)
			{
				if (f.StartsWith("@"))
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
				}
				else
				{
					FieldInfo info;
					if (!AvailableFields.TryGetValue(f, out info))
						continue;

					if (currentFields == null)
						currentFields = new List<Field>();
					Field newField = CreateField(info);
					currentFields.Add(newField);
					fields.Add(info.ClientId, newField);
				}
			}
			if (currentFields != null && currentFields.Count != 0)
			{
				if (groups.Count == 0)
					groups.Add(new FieldGroup());
				FieldGroup curg = groups[groups.Count - 1];
				curg.fields = currentFields;
			}
		}
		private Field CreateField(FieldInfo info)
		{
			Field field;
			if (info.Provider != null)
				field = new ProfileField(info);
			else if(info.IsCustom)
				field = new CustomField(info);
			else
				field = new UserField(info);
			field.UniqueId = ClientID + ClientIDSeparator + info.ClientId;
			field.Id = info.Id;

			if (info.Provider != null)
			{
				ProfileField f = (ProfileField)field;
				f.Settings = ResolveNonCustomFieldSettings(info);
				
				BXUserProfilePublicFacade facade;
				if (!loadedExtensions.TryGetValue(info.Provider, out facade))
				{
					loadedExtensions.Add(info.Provider, facade = info.Provider.CreatePublicFacade());
					facade.User = User;
					facade.Init();
				}

				f.Provider = info.Provider;
				f.Profile = facade;

				if (ProvideProfileFieldView != null)
				{
					ProvideProfileFieldViewEventArgs args = new ProvideProfileFieldViewEventArgs(info.Id, f.RawTitle, f.Settings, info.Provider, facade);
					ProvideProfileFieldView(this, args);
					f.RawTitle = args.Title;
					f.View = args.View;
				}

				f.View = f.View ?? info.Provider.ProvideDefaultPublicView(info.Id);
				if (f.View != null)
					f.View.Init(f.Profile, f.Settings);
			}
			else if (info.IsCustom)
			{
				CustomField f = (CustomField)field;

				f.Field = CustomFields[info.Id];
				f.Type = BXCustomTypeManager.GetCustomType(f.Field.CustomTypeId);
				f.Property = User.CustomValues[info.Id];

				field.IsCustom = true;
				field.CustomProperty = f.Property;
				field.CustomField = f.Field;
				field.CustomType = f.Type;

				if (ProvideCustomFieldView != null)
				{
					ProvideCustomFieldViewEventArgs args = new ProvideCustomFieldViewEventArgs(info.Id, f.RawTitle, f.Field, f.Type);
					ProvideCustomFieldView(this, args);
					f.RawTitle = args.Title;
					f.View = args.View;
				}

				f.View = f.View ?? f.Type.CreatePublicView();
				if (f.View != null)
					f.View.Init(f.Property, f.Field);
			}
			else
			{
				UserField f = (UserField)field;
				f.Settings = ResolveNonCustomFieldSettings(info);
				f.User = User;

				if (ProvideUserFieldView != null)
				{
					ProvideUserFieldViewEventArgs args = new ProvideUserFieldViewEventArgs(info.Id, f.RawTitle, f.Settings);
					ProvideUserFieldView(this, args);
					f.RawTitle = args.Title;
					f.View = args.View;
				}

				f.View = f.View ?? ResolveDefaultFieldView(info.Id);
				if (f.View != null)
					f.View.Init(f.User, f.Settings);
			}
			return field;
		}
		private BXParamsBag<object> ResolveNonCustomFieldSettings(FieldInfo info)
		{
			BXParamsBag<object> settings = new BXParamsBag<object>();
			settings["fieldTitle"] = info.Title;
            settings["host"] = this;
            settings["rolesAuthorizedToVote"] = Parameters.GetString("RolesAuthorizedToVote", string.Empty);

			if (info.Provider != null)
			{
				info.Provider.FillPublicViewSettings(info.Id, settings);
				return settings;
			}
			switch (info.Id)
			{
				case "BirthdayDate":
					settings["showTime"] = false;
					break;
				case "Image":
					settings["maxPreviewWidth"] = BXConfigurationUtility.Options.User.AvatarMaxWidth;
					settings["maxPreviewHeight"] = BXConfigurationUtility.Options.User.AvatarMaxHeight;
					break;
				case "Gender":
					settings["items"] =	new ListItem[] { 
						new ListItem("", ""),
						new ListItem(GetMessageRaw("GenderMale"), "M"),
						new ListItem(GetMessageRaw("GenderFemale"), "F")
					};
					break;
			}
			return settings;
		}
		private BXUserFieldPublicView ResolveDefaultFieldView(string fieldName)
		{
			switch (fieldName)
			{
				case "LastName":
				case "FirstName":
				case "SecondName":
				case "DisplayName":
					return new BXUserFieldView<string>(fieldName, new BXTextFieldView());
				case "BirthdayDate":
					return new BXUserFieldView<DateTime>(fieldName, new BXTextFieldView());
				case "Image":
					return new BXUserFieldView<int>("ImageId", new BXImageFieldView());
				case "Gender":
					return new BXUserFieldView<string>("Gender", new BXListFieldView());
			}
			throw new InvalidOperationException("Unknown field");
		}
		private List<FieldInfo> GetAvailableFields()
		{
			List<FieldInfo> result = new List<FieldInfo>();

			result.Add(new FieldInfo("LastName", GetMessageRaw("Field.LastName")));
			result.Add(new FieldInfo("FirstName", GetMessageRaw("Field.FirstName")));
			result.Add(new FieldInfo("SecondName", GetMessageRaw("Field.SecondName")));
			result.Add(new FieldInfo("Gender", GetMessageRaw("Field.Gender")));

			result.Add(new FieldInfo("BirthdayDate", GetMessageRaw("Field.BirthdayDate")));
			result.Add(new FieldInfo("DisplayName", GetMessageRaw("Field.DisplayName")));
			result.Add(new FieldInfo("Image", GetMessageRaw("Field.Image")));


			foreach (BXUserProfileExtensionProvider p in BXUser.GetProfileExtensionProviders())
				foreach (BXUserProfileFieldInfo f in p.GetAvailableFields())
				{
					FieldInfo fi = new FieldInfo();
					fi.Id = f.Name;
					fi.ClientId = p.UniqueKey + "_" + f.Name;
					fi.Title = f.Title;
					fi.Provider = p;
					result.Add(fi);
				}

			foreach (BXCustomField f in CustomFields)
				result.Add(new CustomFieldInfo(f));

			return result;
		}
		private Dictionary<string, FieldInfo> FillAvailableFields()
		{
			Dictionary<string, FieldInfo> index = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
			foreach (FieldInfo fi in AvailableFieldsList)
				index.Add(fi.ClientId, fi);
			return index;
		}

		//NESTED CLASSES
		private sealed class CustomFieldInfo : FieldInfo
		{
			private BXCustomField field;

			public override string Title
			{
				get
				{
					return base.Title ?? (base.Title = field.TextEncoder.Decode(field.EditFormLabel));
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
		public sealed class UserField : Field
		{
			private BXUser user;
			private BXUserFieldPublicView view;
			internal BXParamsBag<object> Settings;

			public BXUser User
			{
				get
				{
					return user;
				}
				internal set
				{
					user = value;
				}
			}
			public BXUserFieldPublicView View
			{
				set
				{
					view = value;
				}
				get
				{
					return view;
				}
			}

			internal UserField(FieldInfo info)
				: base(info)
			{
			}

			public override string Render()
			{
				return view != null ? view.Render(UniqueId) : string.Empty;
			}

			public override void Render(HtmlTextWriter writer)
			{
				if (view != null)
					view.Render(UniqueId, writer);
			}
			public override bool IsEmpty
			{
				get
				{
					return view == null || view.IsEmpty;
				}
			}
		}
		public sealed class ProfileField : Field
		{
			private BXUserProfileExtensionProvider provider;
			private BXUserProfilePublicFacade profile;
			private BXUserProfileFieldPublicView view;
			internal BXParamsBag<object> Settings;

			public BXUserProfileExtensionProvider Provider
			{
				get
				{
					return provider;
				}
				internal set
				{
					provider = value;
				}
			}
			public BXUserProfilePublicFacade Profile
			{
				get
				{
					return profile;
				}
				internal set
				{
					profile = value;
				}
			}
			public BXUserProfileFieldPublicView View
			{
				set
				{
					view = value;
				}
				get
				{
					return view;
				}
			}

			internal ProfileField(FieldInfo info)
				: base(info)
			{
			}

			public override string Render()
			{
				return view != null ? view.Render(UniqueId) : string.Empty;
			}

			public override void Render(HtmlTextWriter writer)
			{
				if (view != null)
					view.Render(UniqueId, writer);
			}
			public override bool IsEmpty
			{
				get
				{
					return view == null || view.IsEmpty;
				}
			}
		}
		public sealed class CustomField : Field
		{
			private BXCustomField customField;
			private BXCustomType customType;
			private BXCustomProperty customProperty;
			private BXCustomTypePublicView view;

			public BXCustomField Field
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
			public BXCustomType Type
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
			public BXCustomProperty Property
			{
				get
				{
					return customProperty;
				}
				internal set
				{
					customProperty = value;
				}
			}
			public BXCustomTypePublicView View
			{
				set
				{
					view = value;
				}
				get
				{
					return view;
				}
			}

			internal CustomField(FieldInfo info)
				: base(info)
			{
			}

			public override string Render()
			{
				if (view != null)
					return view.GetHtml(UniqueId, ", ");
				return string.Empty;
			}
			public override void Render(HtmlTextWriter writer)
			{
				if (view != null)
					view.Render(UniqueId, ", ", writer);
			}
			public override bool IsEmpty
			{
				get
				{
					return view == null || view.IsEmpty;
				}
			}
		}
		public abstract class Field
		{
			private string id;
			private string uniqueId;
			private bool isCustom;
			internal string RawTitle;
			private BXCustomProperty customProperty;
			private BXCustomType customType;
			private BXCustomField customField;

			public string Title
			{
				get
				{
					return RawTitle != null ? HttpUtility.HtmlEncode(RawTitle) : null;
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

			public string Id
			{
				set
				{
					id = value;
				}
				get
				{
					return id;
				}
			}

			public bool IsCustom
			{
				set
				{
					isCustom = value;
				}
				get
				{
					return isCustom;
				}
			}

			public BXCustomProperty CustomProperty
			{
				get
				{
					return customProperty;
				}

				set { customProperty = value; }
			}

			public BXCustomType CustomType
			{
				get
				{
					return customType;
				}

				set { customType = value; }
			}

			public BXCustomField CustomField
			{
				get
				{
					return customField;
				}

				set { customField = value; }
			}

			internal Field(FieldInfo info)
			{
				RawTitle = info.Title;
			}

			public abstract string Render();
			public abstract void Render(HtmlTextWriter writer);
			public abstract bool IsEmpty
			{
				get;
			}
		}
		public sealed class FieldGroup
		{
			private string title;
			internal List<Field> fields;

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
			public bool CheckNotEmptyFields() 
			{
				if (fields == null || fields.Count == 0)
					return false;
				foreach(Field field in fields)
					if (!field.IsEmpty)
						return true;
				return false;
			}
		}
		public sealed class ProvideCustomFieldViewEventArgs : EventArgs
		{
			private string id;
			private string title;
			private BXCustomType type;
			private BXCustomField field;
			private BXCustomTypePublicView view;

			internal ProvideCustomFieldViewEventArgs(string id, string title, BXCustomField field, BXCustomType type)
			{
				this.id = id;
				this.title = title;
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
			public BXCustomTypePublicView View
			{
				get
				{
					return view;
				}
				set
				{
					view = value;
				}
			}
		}
		public sealed class ProvideUserFieldViewEventArgs : EventArgs
		{
			private BXUserFieldPublicView view;
			private string id;
			private string title;
			private BXParamsBag<object> settings;

			internal ProvideUserFieldViewEventArgs(string id, string title, BXParamsBag<object> settings)
			{
				this.id = id;
				this.title = title;
				this.settings = settings;
			}

			public string Id
			{
				get
				{
					return id;
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
			public BXUserFieldPublicView View
			{
				get
				{
					return view;
				}
				set
				{
					view = value;
				}
			}
		}
		public sealed class ProvideProfileFieldViewEventArgs : EventArgs
		{
			private string id;
			private string title;
			private BXParamsBag<object> settings;
			private BXUserProfileExtensionProvider provider;
			private BXUserProfilePublicFacade profile;
			private BXUserProfileFieldPublicView view;

			internal ProvideProfileFieldViewEventArgs(string id, string title, BXParamsBag<object> settings, BXUserProfileExtensionProvider provider, BXUserProfilePublicFacade profile)
			{
				this.id = id;
				this.title = title;
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
			public BXUserProfileFieldPublicView View
			{
				get
				{
					return view;
				}
				set
				{
					view = value;
				}
			}
		}

        public sealed class RatingTotal
        {
            private string name = string.Empty;
            public string Name
            {
                get { return this.name; }
                set { this.name = value; }
            }

            private double currentValue = 0D;
            public double CurrentValue
            {
                get { return this.currentValue; }
                set { this.currentValue = value; }
            }

            private double previousValue = 0D;
            public double PreviousValue
            {
                get { return this.previousValue; }
                set { this.previousValue = value; }
            }

            private bool isCalculated = false;
            public bool IsCalculated
            {
                get { return this.isCalculated; }
                set { this.isCalculated = value; }
            }

            private DateTime lastCalculated;
            public DateTime LastCalculated
            {
                get { return this.lastCalculated; }
                set { this.lastCalculated = value; }
            }

            public string GetValuesString()
            {
                StringBuilder s = new StringBuilder();
                s.Append(CurrentValue.ToString());

                double diff = CurrentValue - PreviousValue;
                if (diff != 0)
                    s.Append(" (").Append(diff > 0 ? "+" : string.Empty).Append(diff.ToString()).Append(")");
                return s.ToString();
            }
        }
	}

	public class UserProfileViewTemplate : BXComponentTemplate<UserProfileViewComponent>
	{
	}
}