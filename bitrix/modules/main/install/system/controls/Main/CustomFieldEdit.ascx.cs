using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using Bitrix;

using Bitrix.Services;
using Bitrix.UI;
using Bitrix.DataTypes;

public partial class CustomFieldEdit : BXControl
{
	//FIELDS
	readonly Dictionary<string, Dictionary<string, TextBox>> locControls = new Dictionary<string, Dictionary<string, TextBox>>(StringComparer.InvariantCultureIgnoreCase);
	bool dynamicsLoaded;
	string validationGroup = "CustomFieldEdit";
	string fieldName;
	string userTypeId;
	bool? multiple;

	IBXCustomTypeSetting settings;
	IBXCustomTypeAdvancedSetting extraSettings;

	//PROPERTIES
	public UpdatePanel UpdatePanel
	{
		get
		{
			return U;
		}
	}
	#region Attributes
	[DefaultValue("CustomFieldEdit")]
	#endregion
	public string ValidationGroup
	{
		get
		{
			return validationGroup;
		}
		set
		{
			validationGroup = value;
		}
	}
	
	//EVENTS
	public event EventHandler<EventArguments> Store;

	//METHODS
	void InitStatics()
	{
		foreach (BXCustomType t in BXCustomTypeManager.CustomTypes.Values)
			UserTypeIdTextBox.Items.Add(new ListItem(t.Title, t.TypeName));
		UserTypeIdTextBox.SelectedValue = "Bitrix.System.Text";
		
		foreach (string name in Enum.GetNames(typeof(BXCustomFieldFilterVisibility)))
			ShowFilter.Items.Add(new ListItem(GetMessageRaw("ShowFilterList." + name), name));

		Localization.DataSource = BXLanguage.GetList(null, null);
		
		D.DataBind();
		//Localization.DataBind();

		dynamicsLoaded = true;
	}
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!D.StartHidden)
		{
			GetStoredValues();
			InitStatics();
		}
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!D.StartHidden)
		{
			UserTypeIdTextBox.SelectedValue = Request.Form[UserTypeIdTextBox.UniqueID];
			InstantiateSettings();
			InstantiateExtraSettings(null);
		}
	}
	protected void Page_PreRender(object sender, EventArgs e)
	{
		DisplayStoredValues();
		SetStoredValues();
		T.OnCancelClientClick = string.Format("{0}.ClosePopupDialog(); return false;", D.GetJSObjectName());
		//MainTabControl.OnSaveClientClick = "debugger;";
		D.NoRendering = D.StartHidden;
		VS.Visible = !D.NoRendering;

		if (UserTypeIdTextBox.Visible)
		{
			List<string> obsoletes = new List<string>();
			foreach (KeyValuePair<string, BXCustomType> p in BXCustomTypeManager.CustomTypes)
			{
				for (Type t = p.Value.GetType(); t != typeof(BXCustomType) && t != null; t = t.BaseType)
				{
					object[] attributes = t.GetCustomAttributes(typeof(ObsoleteAttribute), false);
					if (attributes != null && attributes.Length != 0)
					{
						obsoletes.Add(p.Key);
						break;
					}
				}
			}
			foreach (ListItem i in UserTypeIdTextBox.Items)
			{
				if (i.Selected)
					continue;
				if (obsoletes.Contains(i.Value))
					i.Enabled = false;
			}
		}
	}
	private void InstantiateSettings()
	{
		Control s = null;
		string typename = userTypeId ?? UserTypeIdTextBox.SelectedValue;
		try
		{
			s = BXCustomTypeManager.GetCustomType(typename).Settings;
		}
		catch (Exception e)
		{
			//ValidationSummary.AddErrorMessage(BXServices.BuildExceptionValidationMessage(e));
			BXLogService.LogAll(new BXLogMessage(e, 0, BXLogMessageType.Warning, "CustomFieldEdit"));
		}
		settings = s as IBXCustomTypeSetting;
		SettingsHolder.Controls.Clear();
		if (settings != null && s != null)
		{
			settings.ValidationGroup = ValidationGroup;
			s.ID = typename.Replace(".", "_") + "_Settings";
			SettingsHolder.Controls.Add(s);
		}
		SettingsHolder.Visible = (settings != null);
	}
	private void InstantiateExtraSettings(object state)
	{
		E.Visible = true;
		Control s = null;
		string typename = userTypeId ?? UserTypeIdTextBox.SelectedValue;
		try
		{
			s = BXCustomTypeManager.GetCustomType(typename).AdvancedSettings;
		}
		catch
		{
			E.Visible = false;
		}
		extraSettings = s as IBXCustomTypeAdvancedSetting;
		ExtraSettingsPlaceholder.Controls.Clear();

		if (s != null && extraSettings != null)
		{
			extraSettings.ValidationGroup = ValidationGroup;
			s.ID = typename.Replace(".", "_") + "_ExtraSettings";
			if (state != null)
				extraSettings.SetSettings(state);
			ExtraSettingsPlaceholder.Controls.Add(s);
		}
	}

	void GetStoredValues()
	{
		userTypeId = Page.Request.Form[StoredUserTypeId.UniqueID];
		if (string.IsNullOrEmpty(userTypeId))
			userTypeId = null;
		
		fieldName = Page.Request.Form[StoredFieldName.UniqueID];
		if (string.IsNullOrEmpty(fieldName))
			fieldName = null;
		
		string s = Page.Request.Form[StoredMultiple.UniqueID];
		bool b;
		if (!String.IsNullOrEmpty(s) && bool.TryParse(s, out b))
			multiple = b;
		else
			multiple = null;
	}
	void SetStoredValues()
	{
		StoredUserTypeId.Value = userTypeId ?? string.Empty;
		StoredFieldName.Value = fieldName ?? string.Empty;
		StoredMultiple.Value = multiple.HasValue ? multiple.ToString() : string.Empty;
	}
	void DisplayStoredValues()
	{
		if (userTypeId != null)
		{
			UserTypeIdLiteral.Text = Encode(BXCustomTypeManager.GetCustomType(userTypeId).Title);
			MultipleLiteral.Text = multiple.Value ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No");
			FieldNameLiteral.Text = Encode(fieldName);
		}
		UserTypeIdLiteral.Visible = FieldNameLiteral.Visible = MultipleLiteral.Visible = (userTypeId != null);
		UserTypeIdTextBox.Visible = FieldNameTextBox.Visible = MultipleCheckBox.Visible = (userTypeId == null); 
	}

	protected void mainTabControl_Command(object sender, BXTabControlCommandEventArgs e)
	{
		UserTypeIdRequired.Enabled = 
		UserTypeIdValidator.Enabled = 
		FieldNameRequired.Enabled = 
		FieldNameValidator.Enabled = 
		cvFieldName.Enabled =
			(userTypeId == null);
		
		Page.Validate(ValidationGroup);
		if (e.CommandName == "save" && (/*userTypeId != null ||*/ Page.IsValid))
		{
			D.StartHidden = true;
			if (Store != null)
			{
				EventArguments ea = new EventArguments();
				ea.State = Gather();
				Store(this, ea);
			}
		}
	}
	protected void UserTypeIdValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = false;
		if (String.IsNullOrEmpty(UserTypeIdTextBox.SelectedValue))
			return;
		if (!BXCustomTypeManager.CustomTypes.ContainsKey(UserTypeIdTextBox.SelectedValue))
			return;
		args.IsValid = true;
	}
	protected void Localization_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
			return;
		BXLanguage lang = (BXLanguage)e.Item.DataItem;
		if (!locControls.ContainsKey(lang.Id))
			locControls.Add(lang.Id, new Dictionary<string, TextBox>());
		locControls[lang.Id].Add("EditFormLabel", (TextBox)e.Item.FindControl("EditFormLabel"));
		locControls[lang.Id].Add("ListColumnLabel", (TextBox)e.Item.FindControl("ListColumnLabel"));
		locControls[lang.Id].Add("ListFilterLabel", (TextBox)e.Item.FindControl("ListFilterLabel"));
		locControls[lang.Id].Add("ErrorMessage", (TextBox)e.Item.FindControl("ErrorMessage"));
		locControls[lang.Id].Add("HelpMessage", (TextBox)e.Item.FindControl("HelpMessage"));
	}

	public void Activate(BXParamsBag<object> state)
	{
		D.StartHidden = false;
		if (!dynamicsLoaded)
			InitStatics();

		UserTypeIdTextBox.SelectedValue = state.Get("CustomTypeId", "Bitrix.System.Text");
		DontEditInList.Checked = !state.Get("EditInList", true);
		FieldNameTextBox.Text = state.Get("FieldName", string.Empty);
		IsSearchable.Checked = state.Get("IsSearchable", false);
		Mandatory.Checked = state.Get("Mandatory", false);
		MultipleCheckBox.Checked = state.Get("Multiple", false);
		ShowFilter.SelectedValue = ((BXCustomFieldFilterVisibility)state.Get("ShowInFilter", (int)BXCustomFieldFilterVisibility.CompleteMatch)).ToString();
		DontShowInList.Checked = !state.Get("ShowInList", true);
		Sort.Text = state.Get("Sort", string.Empty);
		XmlId.Text = state.Get("XmlId", string.Empty);

		foreach (KeyValuePair<string, Dictionary<string, TextBox>> l in locControls)
		{
			string key = "Loc." + l.Key;
			if (state.ContainsKey(key))
			{
				string[] phrases = (string[])state[key];

				l.Value["EditFormLabel"].Text = phrases[0];
				l.Value["ErrorMessage"].Text = phrases[1];
				l.Value["HelpMessage"].Text = phrases[2];
				l.Value["ListColumnLabel"].Text = phrases[3];
				l.Value["ListFilterLabel"].Text = phrases[4];
			}
		}

		userTypeId = state.Get<string>("@CustomTypeId");
		if (string.IsNullOrEmpty(userTypeId))
			userTypeId = null;

		fieldName = state.Get<string>("@FieldName");
		if (string.IsNullOrEmpty(fieldName))
			fieldName = null;

		if (state.ContainsKey("@Multiple"))
			multiple = state.Get("@Multiple", false);
		else
			multiple = null;

		InstantiateSettings();
		BXParamsBag<object> settingsState = state.Get<BXParamsBag<object>>("Settings");
		if (settings != null && settingsState != null)
			settings.SetSettings(settingsState);
		
		object extrasState = state.Get("Extras", null);
		InstantiateExtraSettings(extrasState);

		StoredSender.Value = state.Get("@Sender", string.Empty);
	}
	public void Hide()
	{
		StoredSender.Value = string.Empty;
		StoredFieldName.Value = string.Empty;
		StoredMultiple.Value = string.Empty;
		StoredUserTypeId.Value = string.Empty;

		D.StartHidden = true;
		U.Update();
	}

	BXParamsBag<object> Gather()
	{
		BXParamsBag<object> storage = new BXParamsBag<object>();

		storage.Add("CustomTypeId", UserTypeIdTextBox.SelectedValue);
		storage.Add("EditInList", !DontEditInList.Checked);
		storage.Add("FieldName", FieldNameTextBox.Text);
		storage.Add("IsSearchable", IsSearchable.Checked);
		storage.Add("Mandatory", Mandatory.Checked);
		storage.Add("Multiple", MultipleCheckBox.Checked);
		storage.Add("ShowInFilter", (int)Enum.Parse(typeof(BXCustomFieldFilterVisibility), ShowFilter.SelectedValue));
		storage.Add("ShowInList", !DontShowInList.Checked);
		storage.Add("Sort", Sort.Text);
		storage.Add("XmlId", XmlId.Text);

		foreach (KeyValuePair<string, Dictionary<string, TextBox>> l in locControls)
		{
			string key = "Loc." + l.Key;
			storage[key] = new string[] 
			{
				l.Value["EditFormLabel"].Text,
				l.Value["ErrorMessage"].Text,
				l.Value["HelpMessage"].Text,
				l.Value["ListColumnLabel"].Text,
				l.Value["ListFilterLabel"].Text
			};
		}

		/*
		if (userTypeId != null)
			storage.Add("@CustomTypeId", userTypeId);
		if (fieldName != null)
			storage.Add("@FieldName", fieldName);
		if (multiple != null)
			storage.Add("@Multiple", multiple.Value);
		*/

		if (settings != null)
			storage.Add("Settings", settings.GetSettings());
		if (extraSettings != null)
			storage.Add("Extras", extraSettings.GetSettings());

		storage.Add("@Sender", StoredSender.Value);

		return storage;
	}

	//NESTED CLASSES
	public class EventArguments : EventArgs
	{
		public BXParamsBag<object> State;
	}
}
