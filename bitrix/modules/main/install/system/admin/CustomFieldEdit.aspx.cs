using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;

using Bitrix.Modules;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Services.Js;
using Bitrix.Configuration;
using Bitrix.Security;

public partial class bitrix_admin_CustomFieldEdit : BXAdminPage
{
	int? id = null;
	string entityid;
	string usertypeid;
	string fieldname;
	bool? multiple = null;
	IBXCustomTypeSetting settings;
	IBXCustomTypeAdvancedSetting extraSettings;
	bool currentUserCanModifySettings;

	private BXCustomField item;
	private BXCustomField changed;
	public bool EditMode
	{
		get
		{
			return id.HasValue;
		}
	}
	public BXCustomField Item
	{
		get
		{
			if (item == null)
			{
				if (id != null)
					item = BXCustomField.GetById(id);
				if (item == null)
					item = new BXCustomField();
			}
			return item;
		}
		set
		{
			item = value;
		}
	}
	private Dictionary<string, Dictionary<string, TextBox>> locControls = new Dictionary<string, Dictionary<string, TextBox>>(StringComparer.InvariantCultureIgnoreCase);

	private void InitNewDynamics()
	{
		foreach (BXCustomType t in BXCustomTypeManager.CustomTypes.Values)
			UserTypeIdList.Items.Add(new ListItem(t.Title, t.TypeName));

		UserTypeIdList.SelectedValue = "Bitrix.System.Text";
	}
	private void InitCommonDynamics()
	{
		foreach (string name in Enum.GetNames(typeof(BXCustomFieldFilterVisibility)))
			ShowFilter.Items.Add(new ListItem(GetMessageRaw("ShowFilterList." + name), name));
	}
	private void InitEdit()
	{
		if (!Item.IsNew)
		{
			XmlId.Text = Item.XmlId;
			Sort.Text = Item.Sort.ToString();
			Mandatory.Checked = Item.Mandatory;
			ShowFilter.SelectedValue = Item.ShowInFilter.ToString();
			DontShowInList.Checked = !Item.ShowInList;
			DontEditInList.Checked = !Item.EditInList;
			IsSearchable.Checked = Item.IsSearchable;

			id = Item.Id;
			entityid = Item.OwnerEntityId;
			usertypeid = Item.CustomTypeId;
			multiple = Item.Multiple;
			fieldname = Item.Name;
		}
		else
			Response.Redirect("CustomFieldEdit.aspx");
	}
	private void PrepareValidators()
	{
		UserTypeIdValidator.Enabled = UserTypeIdRequired.Enabled = !EditMode;
		EntityIdValidator.Enabled = EntityIdRequired.Enabled = !(Request["entityid"] != null || EditMode);
		FieldNameValidator.Enabled = FieldNameRequired.Enabled = !EditMode;
		cvFieldName.Enabled = cvFieldName.Enabled = !EditMode;
	}
	private void PrepareForDisplay()
	{
		if (EditMode)
		{
			IdLiteral.Text = id.Value.ToString();
			UserTypeIdLiteral.Text = HttpUtility.HtmlEncode(BXCustomTypeManager.GetCustomType(usertypeid).Title);
			MultipleLiteral.Text = multiple.Value ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No");
			FieldNameLiteral.Text = HttpUtility.HtmlEncode(fieldname);
		}
		if (Request["entityid"] != null || EditMode)
			EntityIdLiteral.Text = HttpUtility.HtmlEncode(entityid);

		IdRow.Visible = EditMode;
		UserTypeIdList.Visible = !(UserTypeIdLiteral.Visible = EditMode);
		EntityIdTextBox.Visible = !(EntityIdLiteral.Visible = (EditMode || Request["entityid"] != null));
		FieldNameTextBox.Visible = !(FieldNameLiteral.Visible = EditMode);
		MultipleCheckBox.Visible = !(MultipleLiteral.Visible = EditMode);
		mainTabControl.ShowSaveButton = mainTabControl.ShowApplyButton = BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
	}
	private void GetStoredValues()
	{
		string s = Request["id"] ?? null;
		s = Request.Form["@id"] ?? s;
		int i;
		if (!String.IsNullOrEmpty(s) && int.TryParse(s, out i))
			id = i;
		else
			id = null;

		s = Request["entityid"] ?? null;
		entityid = Request.Form["@entityid"] ?? s;

		usertypeid = Request.Form["@usertypeid"] ?? null;

		fieldname = Request.Form["@fieldname"] ?? null;

		s = Request.Form["@multiple"];
		bool b;
		if (!String.IsNullOrEmpty(s) && bool.TryParse(s, out b))
			multiple = b;
		else
			multiple = null;
	}
	private void SetStoredValues()
	{
		if (id.HasValue)
			ClientScript.RegisterHiddenField("@id", id.Value.ToString());
		if (entityid != null)
			ClientScript.RegisterHiddenField("@entityid", entityid);
		if (usertypeid != null)
			ClientScript.RegisterHiddenField("@usertypeid", usertypeid);
		if (fieldname != null)
			ClientScript.RegisterHiddenField("@fieldname", fieldname);
		if (multiple.HasValue)
			ClientScript.RegisterHiddenField("@multiple", multiple.Value.ToString());
	}

	public static string BuildExceptionValidationMessage(Exception e)
	{
		StringBuilder s = new StringBuilder();
		s.AppendFormat(@"{1} ({0})", HttpUtility.HtmlEncode(e.Source), HttpUtility.HtmlEncode(e.GetType().Name));
		s.AppendFormat(@"&nbsp;<a href=""javascript:void(0)"" onclick=""{0}"" style=""color: blue"">-&gt;</a>", "if (this.nextSibling) this.nextSibling.style.display = (this.nextSibling.style.display == 'none') ? 'inline' : 'none';");
		s.AppendFormat(@"<span style=""color: black; display: none""><br/><br/>{0}</span>", HttpUtility.HtmlEncode(e.ToString()));
		return s.ToString();
	}

	private void InstantiateSettings()
	{
		Control s = null;
		string typename = EditMode ? usertypeid : UserTypeIdList.SelectedValue;
		try
		{
			s = BXCustomTypeManager.GetCustomType(typename).Settings;
		}
		catch (Exception e)
		{
			ValidationSummary.AddErrorMessage(BuildExceptionValidationMessage(e));
			BXLogService.LogAll(new BXLogMessage(e, 0, BXLogMessageType.Warning, "CustomFieldEdit"));
		}
		settings = s as IBXCustomTypeSetting;
		SettingsHolder.Controls.Clear();
		if (settings != null)
		{
			settings.ValidationGroup = "edit";
			s.ID = typename.Replace(".", "_") + "_Settings";
			SettingsHolder.Controls.Add(s);
		}
		SettingsHeader.Visible = SettingsHolder.Visible = (settings != null);
	}
	private void InstantiateExtraSettings()
	{
		ExtraTab.Visible = true;
		Control s = null;
		string typename = EditMode ? usertypeid : UserTypeIdList.SelectedValue;
		try
		{
			s = BXCustomTypeManager.GetCustomType(typename).AdvancedSettings;
		}
		catch
		{
			ExtraTab.Visible = false;
		}
		extraSettings = s as IBXCustomTypeAdvancedSetting;
		ExtraSettingsPlaceholder.Controls.Clear();

		if (extraSettings != null)
		{
			extraSettings.ValidationGroup = "edit";
			s.ID = typename.Replace(".", "_") + "_ExtraSettings";
			extraSettings.Initialize(Item);
			ExtraSettingsPlaceholder.Controls.Add(s);
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();
		mainTabControl.ShowSaveButton = mainTabControl.ShowApplyButton = BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);

		GetStoredValues();

		if (!EditMode)
			InitNewDynamics();
		InitCommonDynamics();
		if (EditMode && !IsPostBack)
			InitEdit();

		Localization.DataSource = BXLanguage.GetList(null, null);
		Localization.DataBind();
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		//Because it requires post data from DD Lists
		InstantiateSettings();
		InstantiateExtraSettings();

		if (EditMode && !IsPostBack && settings != null)
			settings.SetSettings(Item.Settings);
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = GetMessage("MasterTitle");
		PrepareForDisplay();
		SetStoredValues();
		//BackAction.OnClickScript = "window.location.href='" + BXJSUtility.Encode(Request[BXConfigurationUtility.Constants.BackUrl] ?? "CustomField.aspx") + "'; return false;";
	}
	protected void Page_PreRender(object sender, EventArgs e)
	{

		if (UserTypeIdList.Visible)
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
			foreach (ListItem i in UserTypeIdList.Items)
			{
				if (i.Selected)
					continue;
				if (obsoletes.Contains(i.Value))
					i.Enabled = false;
			}
		}
	}

	private void ReturnBack()
	{
		Response.Redirect(Request[BXConfigurationUtility.Constants.BackUrl] ?? "CustomField.aspx");
	}

	private bool TrySave()
	{
		PrepareValidators();
		if (!IsValid)
			return false;
		try
		{
			BXUser.DemandOperations(BXRoleOperation.Operations.ProductSettingsManage);
			if (!Save())
				return false;
		}
		catch (BXEventException e)
		{
			foreach (string s in e.Messages)
				ValidationSummary.AddErrorMessage(HttpUtility.HtmlEncode(s));
			return false;
		}
		catch (Exception e)
		{
			ValidationSummary.AddErrorMessage(BuildExceptionValidationMessage(e));
			return false;
		}
		PrepareValidators();
		return SuccessMessage.Visible = true;
	}
	private bool Save()
	{

		if (!EditMode)
		{
			Item.CustomTypeId = UserTypeIdList.SelectedValue;
			string e = (Request["entityid"] == null) ? EntityIdTextBox.Text : entityid;
			if (String.IsNullOrEmpty(e))
				throw new BXEventException(GetMessage("Error.EntityIdRequired"));
			Item.OwnerEntityId = e;
			Item.Multiple = MultipleCheckBox.Checked;
			Item.Name = FieldNameTextBox.Text;
		}
		ValidateFieldName();


		Item.XmlId = XmlId.Text;
		int i;
		if (!int.TryParse(Sort.Text, out i))
			i = 10;
		Item.Sort = i;

		Item.Mandatory = Mandatory.Checked;
		Item.ShowInFilter = (BXCustomFieldFilterVisibility)Enum.Parse(typeof(BXCustomFieldFilterVisibility), ShowFilter.SelectedValue, true);
		Item.ShowInList = !DontShowInList.Checked;
		Item.EditInList = !DontEditInList.Checked;
		Item.IsSearchable = IsSearchable.Checked;
		Item.Settings.Clear();
		if (settings != null)
			Item.Settings.Assign(settings.GetSettings());

		BXCustomFieldLocalizationCollection loc = Item.Localization;
		loc.Clear();
		foreach (KeyValuePair<string, Dictionary<string, TextBox>> langLine in locControls)
		{
			BXCustomFieldLocalization l = new BXCustomFieldLocalization();

			l.EditFormLabel = langLine.Value["EditFormLabel"].Text;
			l.ListColumnLabel = langLine.Value["ListColumnLabel"].Text;
			l.ListFilterLabel = langLine.Value["ListFilterLabel"].Text;
			l.ErrorMessage = langLine.Value["ErrorMessage"].Text;
			l.HelpMessage = langLine.Value["HelpMessage"].Text;

			loc[langLine.Key] = l;
		}

		Item.Save();

		id = Item.Id;
		entityid = Item.OwnerEntityId;
		usertypeid = Item.CustomTypeId;
		fieldname = Item.Name;
		multiple = Item.Multiple;

		//EXTRA SETTINGS
		if (extraSettings != null)
			extraSettings.Save();

		return true;
	}

	private void ValidateFieldName()
	{
		if (!EditMode)
		{
			string entityId = (Request["entityid"] == null) ? EntityIdTextBox.Text : entityid;
			if (!BXCustomField.CheckUnique(entityId, FieldNameTextBox.Text))
				throw new BXEventException(string.Format(GetMessage("Error.FieldNameAlreadyExist"), FieldNameTextBox.Text));
		}

		bool isValid = false;
		string trimed = FieldNameTextBox.Text.Trim().ToUpper();
		isValid = (trimed != "VALUE")
			&& (trimed != "VALUEID")
			&& (trimed != "VALUEINT")
			&& (trimed != "VALUEDOUBLE")
			&& (trimed != "VALUEDATE");

		if (!IsValid)
			throw new BXEventException(GetMessage("Error.FieldNameIllegal"));
	}

	protected void mainTabControl_Command(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "save":
				if (TrySave())
				{
					changed = Item;
					ReturnBack();
				}
				break;
			case "apply":
				TrySave();
				break;
			case "cancel":
				ReturnBack();
				break;
		}
	}
	protected void UserTypeIdValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = false;
		if (String.IsNullOrEmpty(UserTypeIdList.SelectedValue))
			return;
		if (!BXCustomTypeManager.CustomTypes.ContainsKey(UserTypeIdList.SelectedValue))
			return;
		args.IsValid = true;
	}
	/*protected void UserTypeId_SelectedIndexChanged(object sender, EventArgs e)
	{
		InstantiateSettings();
		InstantiateExtraSettings();
	}*/
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

		if (EditMode && !IsPostBack)
		{
			BXCustomFieldLocalization loc = Item.Localization[lang.Id];
			if (loc != null)
			{
				locControls[lang.Id]["EditFormLabel"].Text = loc.EditFormLabel;
				locControls[lang.Id]["ListColumnLabel"].Text = loc.ListColumnLabel;
				locControls[lang.Id]["ListFilterLabel"].Text = loc.ListFilterLabel;
				locControls[lang.Id]["ErrorMessage"].Text = loc.ErrorMessage;
				locControls[lang.Id]["HelpMessage"].Text = loc.HelpMessage;
			}
		}
	}
}
