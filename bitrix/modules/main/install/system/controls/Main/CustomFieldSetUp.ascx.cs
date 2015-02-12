using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Bitrix;

using Bitrix.Services;
using Bitrix.UI;
using Bitrix.DataTypes;
using System.ComponentModel;

public partial class bitrix_ui_CustomFieldSetUp : BXControl, IBXCustomFieldSetUp
{
	//FIELDS
	private string entityId;
	private string validationGroup;
	private readonly List<RowControl> rows = new List<RowControl>();
	private readonly Dictionary<object, RowControl> rowsForButton = new Dictionary<object, RowControl>();
	private readonly Dictionary<object, RowControl> rowsForCode = new Dictionary<object, RowControl>();
	private readonly Dictionary<object, RowControl> rowsForName = new Dictionary<object, RowControl>();
	private bool allowDelete = true;
	protected CustomFieldEdit DetailEditDialog;

	public Control DialogPlaceholder { get; set; }

	//PROPERTIES
	public string EntityId
	{
		get
		{
			return entityId;
		}
		set
		{
			entityId = value;
		}
	}
	
	[DefaultValue(true)]
	public bool AllowDelete
	{
		get
		{
			return allowDelete;
		}
		set
		{
			allowDelete = value;
		}
	}

	//METHODS
	private void AddField(BXCustomField field, int index)
	{
		RowControl r = new RowControl();
		r.Row = new HtmlTableRow();


		r.CustomField = field;

		HtmlTableCell idCell = new HtmlTableCell();
		Label idLb = r.IdLabel = new Label();
		idCell.Controls.Add(idLb);
		HiddenField fieldState = r.State = new HiddenField();
		idCell.Controls.Add(fieldState);
		r.Row.Cells.Add(idCell);

		//CODE
		HtmlTableCell codeCell = new HtmlTableCell();
		TextBox codeTxt = r.Code = new TextBox();
		codeTxt.MaxLength = 20;
		codeTxt.Columns = 15;
		codeCell.Controls.Add(codeTxt);
		Label codeLb = r.CodeLabel = new Label();
		codeCell.Controls.Add(codeLb);
		r.CodeValidator = new CustomValidator();
		r.CodeValidator.ValidateEmptyText = true;
		r.CodeValidator.ValidationGroup = ValidationGroup;
		r.CodeValidator.ServerValidate += new ServerValidateEventHandler(CodeValidator_ServerValidate);
		r.CodeValidator.ClientValidationFunction = "customFieldSetUp_validateCode";
		r.CodeValidator.Text = "*";
		r.CodeValidator.ErrorMessage = GetMessage("Message.CodeRequired");
		r.CodeValidator.Display = ValidatorDisplay.Static;
		codeCell.Controls.Add(r.CodeValidator);
		r.Row.Cells.Add(codeCell);

		//NAME
		HtmlTableCell nameCell = new HtmlTableCell();
		nameCell.Width = "100%";
		TextBox nameTxt = r.Name = new TextBox();
		nameTxt.MaxLength = 50;
		nameTxt.Width = Unit.Percentage(100);
		nameCell.Controls.Add(nameTxt);
		r.NameValidator = new CustomValidator();
		r.NameValidator.ValidateEmptyText = true;
		r.NameValidator.ValidationGroup = ValidationGroup;
		r.NameValidator.ServerValidate += new ServerValidateEventHandler(NameValidator_ServerValidate);
		r.NameValidator.ClientValidationFunction = "customFieldSetUp_validateName";
		r.NameValidator.Text = "*";
		r.NameValidator.ErrorMessage = GetMessage("Message.NameRequired");
		r.NameValidator.Display = ValidatorDisplay.Static;
		nameCell.Controls.Add(r.NameValidator);
		r.Row.Cells.Add(nameCell);

		//ACTIVE
		HtmlTableCell actCell = new HtmlTableCell();
		CheckBox actCb = r.Active = new CheckBox();
		actCb.Checked = true;
		actCell.Controls.Add(actCb);
		r.Row.Cells.Add(actCell);

		//TYPE
		HtmlTableCell typeCell = new HtmlTableCell();
		DropDownList typeDdl = r.Type = new DropDownList();
		typeDdl.DataSource = BXCustomTypeManager.CustomTypes.Values;
		typeDdl.DataTextField = "Title";
		typeDdl.DataValueField = "TypeName";
		typeDdl.DataBind();
		typeCell.Controls.Add(typeDdl);
		Label typeLb = r.TypeLabel = new Label();
		typeCell.Controls.Add(typeLb);
		r.Row.Cells.Add(typeCell);

		//MULTIPLE
		HtmlTableCell multCell = new HtmlTableCell();
		CheckBox multCb = r.Multiple = new CheckBox();
		multCell.Controls.Add(multCb);
		Label multLb = r.MultipleLabel = new Label();
		multCell.Controls.Add(multLb);
		r.Row.Cells.Add(multCell);

		//SORT
		HtmlTableCell sortCell = new HtmlTableCell();
		TextBox sortTxt = r.Sort = new TextBox();
		sortTxt.Text = string.Format("{0}", 10 * index);
		sortTxt.MaxLength = 10;
		sortTxt.Columns = 3;
		sortCell.Controls.Add(sortTxt);
		r.Row.Cells.Add(sortCell);

		//EDIT
		HtmlTableCell editCell = new HtmlTableCell();
		editCell.Align = "center";
		Button editBtn = r.Edit = new Button();
		editBtn.UseSubmitBehavior = false;
		editBtn.Text = "...";
		editBtn.Click += editBtn_Click;
		editBtn.CausesValidation = false;
		editCell.Controls.Add(editBtn);
		r.Row.Cells.Add(editCell);

		//DELETE
		HtmlTableCell deleteCell = new HtmlTableCell();
		deleteCell.Visible = AllowDelete;
		deleteCell.Align = "center";
		BXIconButton deleteLbn = r.Delete = new BXIconButton();
		deleteLbn.OnClientClick = "return confirm('" + GetMessage("ConfirmMessage") + "');";
		deleteLbn.CssClass = "delete";
		deleteLbn.CausesValidation = false;
		deleteLbn.Visible = false;
		deleteLbn.Click += deleteLbn_Click;
		deleteCell.Controls.Add(deleteLbn);
		r.Row.Cells.Add(deleteCell);


		if (field != null)
		{
			nameTxt.Text = field.TextEncoder.Decode(field.EditFormLabel);
			actCb.Checked = field.ShowInList;
			multCb.Checked = field.Multiple;
			multCb.Enabled = false;
			idLb.Text = field.Id.ToString();
			typeDdl.SelectedValue = field.CustomTypeId;
			deleteLbn.Visible = true;
			deleteLbn.CommandArgument = field.Id.ToString();
			editBtn.CommandArgument = field.Id.ToString();
			codeLb.Text = HttpUtility.HtmlEncode(field.CorrectedName);
			codeTxt.Visible = false;
			r.CodeValidator.Enabled = false;
			multLb.Text = field.Multiple ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No");
			multCb.Visible = false;

			typeDdl.Visible = false;

			r.CustomType = BXCustomTypeManager.GetCustomType(field.CustomTypeId);

			if (r.Type != null)
				typeLb.Text = HttpUtility.HtmlEncode(r.CustomType.Title);
			else
				typeLb.Text = HttpUtility.HtmlEncode(field.CustomTypeId);
		}
		else
			typeDdl.SelectedValue = "Bitrix.System.Text";

		CustomFields.Rows.Add(r.Row);
		rows.Add(r);
		rowsForButton.Add(r.Edit, r);
		rowsForCode.Add(r.CodeValidator, r);
		rowsForName.Add(r.NameValidator, r);

		r.Store();
	}

	void CodeValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		RowControl r = rowsForCode[source];
		args.IsValid = string.IsNullOrEmpty(r.Name.Text) || !string.IsNullOrEmpty(args.Value);
	}

	void NameValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		RowControl r = rowsForName[source];
		args.IsValid = (r.Code.Visible && string.IsNullOrEmpty(r.Code.Text)) || !string.IsNullOrEmpty(args.Value);
	}

	private void AssignTriggers()
	{
		ScriptManager m = ScriptManager.GetCurrent(Page);
		if (m != null)
			foreach (RowControl r in rows)
			{
				m.RegisterAsyncPostBackControl(r.Edit);
				m.RegisterAsyncPostBackControl(r.Delete);				
			}
	}
	private int GetIndexById(int id)
	{
		for (int i = 0; i < rows.Count; i++)
			if (rows[i].IdLabel.Text.Equals(id.ToString(), StringComparison.InvariantCultureIgnoreCase))
				return i;

		return 0;
	}
	private void GenerateIds()
	{
		for (int i = 0; i < rows.Count; i++)
		{
			RowControl r = rows[i];
			r.State.ID = string.Format("C_STATE_{0}", i);
			r.IdLabel.ID = string.Format("C_ID_{0}", i);
			r.Name.ID = string.Format("C_NAME_{0}", i);
			r.Active.ID = string.Format("C_ACTIVE_{0}", i);
			r.Multiple.ID = string.Format("C_MULT_{0}", i);
			r.Sort.ID = string.Format("C_SORT_{0}", i);
			r.Code.ID = string.Format("C_CODES_{0}", i);
			r.Type.ID = string.Format("C_TYPES_{0}", i);
			r.Edit.ID = string.Format("C_EDITBTN_{0}", i);
			r.Delete.ID = string.Format("C_DELBTN_{0}", i);
			r.Sort.Text = string.Format("{0}", 10 * (i + 1));
			r.CodeValidator.ControlToValidate = r.Code.ID;
			r.NameValidator.ControlToValidate = r.Name.ID;
		}
	}
	public void FillField(BXCustomField field, BXParamsBag<object> state)
	{
		field.ShowInList = state.Get("ShowInList", true);
		field.Sort = state.Get("Sort", 10);
		field.Settings.Assign(state.Get<BXParamsBag<object>>("Settings"));
		field.EditInList = state.Get("EditInList", true);
		field.IsSearchable = state.Get("IsSearchable", false);
		field.Mandatory = state.Get("Mandatory", false);
		field.ShowInFilter = (BXCustomFieldFilterVisibility)state.Get("ShowInFilter", (int)BXCustomFieldFilterVisibility.CompleteMatch);
		field.XmlId = state.Get("XmlId", string.Empty);
	}
	public void FillLocalization(BXCustomField field, BXParamsBag<object> state)
	{
		foreach (string lang in BXLoc.Locales)
		{
			string key = "Loc." + lang;


			BXCustomFieldLocalization l = field.Localization[lang];
			if (l == null) 
			{
				l = new BXCustomFieldLocalization();
				field.Localization[lang] = l;
			}

			if (!state.ContainsKey(key))
				continue;

			string[] phrases = state.Get<string[]>(key);
			l.EditFormLabel = phrases[0];
			l.ErrorMessage = phrases[1];
			l.HelpMessage = phrases[2];
			l.ListColumnLabel = phrases[3];
			l.ListFilterLabel = phrases[4];
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		DetailEditDialog = (CustomFieldEdit)LoadControl("~/bitrix/controls/Main/CustomFieldEdit.ascx");
		(DialogPlaceholder ?? DP).Controls.Add(DetailEditDialog);

		int quantity = 0;
		foreach (BXCustomField field in BXCustomEntityManager.GetFields(EntityId))
		{
			quantity++;
			AddField(field, quantity);
		}

		for (int i = 0; i < 5; i++)
			AddField(null, quantity + 1 + i);

		DeleteHeader.Visible = AllowDelete;

		GenerateIds();
		AssignTriggers();
		DetailEditDialog.Store += DetailEditDialog_Store;
	}
	protected void Page_PreRender(object sender, EventArgs e)
	{
		List<string> obsoletes = new List<string>();
		foreach (KeyValuePair<string, BXCustomType> p in BXCustomTypeManager.CustomTypes)
		{
			for(Type t = p.Value.GetType(); t != typeof(BXCustomType) && t != null; t = t.BaseType)
			{
				object[] attributes = t.GetCustomAttributes(typeof(ObsoleteAttribute), false);
				if (attributes != null && attributes.Length != 0)
				{
					obsoletes.Add(p.Key);
					break;
				}
			}
		}
		foreach (RowControl r in rows)
		{
			if (!r.Type.Visible)
				continue;
			foreach (ListItem i in r.Type.Items)
			{
				if (i.Selected)
					continue;
				if (obsoletes.Contains(i.Value))
					i.Enabled = false;
			}
		}
	}
	private void DetailEditDialog_Store(object sender, CustomFieldEdit.EventArguments e)
	{
		foreach (RowControl r in rows)
		{
			if (r.Edit.ClientID != e.State.Get("@Sender", string.Empty))
				continue;

			BXParamsBag<object> state = e.State;

			state.Remove("@Sender");
			string locKey = "Loc." + BXLoc.CurrentLocale;
			r.Name.Text = (state.Get<string[]>(locKey) ?? new string[1] { string.Empty })[0];
			r.Active.Checked = state.Get("ShowInList", true);
			r.Multiple.Checked = state.Get("Multiple", false);
			r.Sort.Text = state.Get("Sort", "");
			if (r.Code.Visible)
				r.Code.Text = state.Get("FieldName", "");
			if (r.Type.Visible)
				r.Type.SelectedValue = state.Get("CustomTypeId", "Bitrix.System.Text");

			r.Store(state);
			Fields.Update();
			break;
		}
	}
	private void DeleteField(int itemIndex)
	{
		rowsForButton.Remove(rows[itemIndex].Edit);
		rowsForCode.Remove(rows[itemIndex].Code);
		rowsForName.Remove(rows[itemIndex].Name);
		rows.RemoveAt(itemIndex);
		CustomFields.Rows.RemoveAt(itemIndex + 1);
	}

	private void deleteLbn_Click(object sender, EventArgs e)
	{
		if (!AllowDelete)
			return;

		LinkButton btn = sender as LinkButton;

		if (btn != null && !string.IsNullOrEmpty(btn.CommandArgument))
		{
			BXCustomField.Delete(int.Parse(btn.CommandArgument));

			DeleteField(GetIndexById(int.Parse(btn.CommandArgument)));
			GenerateIds();
			DetailEditDialog.Hide();
			Fields.Update();
		}
	}
	private void editBtn_Click(object sender, EventArgs e)
	{
		RowControl r = rowsForButton[sender];
		BXParamsBag<object> state = r.Restore();
		string locKey = "Loc." + BXLoc.CurrentLocale;
		if (!state.ContainsKey(locKey))
			state.Add(locKey, new string[5] { r.Name.Text, null, null, null, null });
		else
			((string[])state[locKey])[0] = r.Name.Text;
		state["ShowInList"] = r.Active.Checked;
		state["Multiple"] = r.Multiple.Checked;
		state["Sort"] = r.Sort.Text;
		if (r.Code.Visible)
			state["FieldName"] = r.Code.Text;
		if (r.Type.Visible)
			state["CustomTypeId"] = r.Type.SelectedValue;

		state.Add("@Sender", r.Edit.ClientID);

		if (r.CustomField != null)
		{
			state.Add("@FieldName", r.CustomField.Name);
			state.Add("@Multiple", r.CustomField.Multiple);
			state.Add("@CustomTypeId", r.CustomField.CustomTypeId);
		}

		DetailEditDialog.Activate(state);
		DetailEditDialog.UpdatePanel.Update();
	}

	#region IBXCustomFieldSetUp Members
	public string ValidationGroup
	{
		get
		{
			return validationGroup;
		}
		set
		{
			validationGroup = value;
			foreach (RowControl r in rows)
				r.CodeValidator.ValidationGroup = r.NameValidator.ValidationGroup = value;
		}
	}

	private void SaveExtras(object state, BXCustomField field, BXCustomType t)
	{
		if (state != null && t != null)
		{
			IBXCustomTypeAdvancedSetting extras = null;
			try
			{
				extras = t.AdvancedSettings as IBXCustomTypeAdvancedSetting;
			}
			catch
			{
			}
			if (extras != null)
			{
				extras.Initialize(field);
				extras.SetSettings(state);
				extras.Save();
			}
		}
	}
	public void Save()
	{
		int fieldsAdded = 0;

		int[] sort = new int[rows.Count];
		for (int i = 0; i < rows.Count; i++)
		{
			RowControl r = rows[i];
			int sortValue;
			sort[i] = int.TryParse(r.Sort.Text, out sortValue) ? sortValue : (i + 1) * 10;
		}

		int[] index = new int[rows.Count];
		for (int i = 0; i < rows.Count; i++)
			index[i] = i;

		Array.Sort(index, delegate(int a, int b)
		{
			return sort[a] - sort[b];		
		});

		for (int i = 0; i < rows.Count; i++)
		{
			RowControl r = rows[index[i]];
			BXParamsBag<object> state = r.Restore();

			//UPDATE FIELD
			if (r.CustomField != null)
			{
				BXCustomField field = r.CustomField;
				FillField(field, state);

				field.Sort = (i + 1) * 10;
				field.ShowInList = r.Active.Checked;
				
				FillLocalization(field, state);
				BXCustomFieldLocalization l  = field.Localization[BXLoc.CurrentLocale];
				if (l != null)
					l.EditFormLabel = r.Name.Text;
				field.Save();

				SaveExtras(state.Get("Extras"), field, r.CustomType);

				r.Store();
			}
			//CREATE FIELD
			else if (string.IsNullOrEmpty(r.Code.Text))
			{
				//ERROR
			}
			else
			{
				string canonical = BXCustomField.CorrectName(r.Code.Text);
				r.Code.Text = canonical;

				BXCustomField field = new BXCustomField();
				FillField(field, state);
				field.CustomTypeId = r.Type.SelectedValue;
				field.ShowInList = r.Active.Checked;
				field.Multiple = r.Multiple.Checked;
				field.Name = canonical;
				field.OwnerEntityId = EntityId;
				field.Sort = (i + 1) * 10;
				

				FillLocalization(field, state);
				BXCustomFieldLocalization l = field.Localization[BXLoc.CurrentLocale];
				if (l != null)
					l.EditFormLabel = r.Name.Text;

				field.Save();
				fieldsAdded++;

				r.CustomField = field;
				r.CustomType = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
				SaveExtras(state.Get("Extras"), field, r.CustomType);
				r.Store(field, r.CustomType);

				//UPDATE CONTROLS
				r.IdLabel.Text = field.Id.ToString();
				r.Edit.CommandArgument = field.Id.ToString();
				r.Delete.CommandArgument = field.Id.ToString();
				r.Delete.Visible = true;
				r.CodeLabel.Text = Encode(field.CorrectedName);
				r.CodeLabel.Visible = true;
				r.Code.Visible = false;
				r.MultipleLabel.Text = field.Multiple ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No");
				r.MultipleLabel.Visible = true;
				r.Multiple.Visible = false;

				r.Type.Visible = false;
				r.TypeLabel.Visible = true;
				r.TypeLabel.Text = HttpUtility.HtmlEncode((r.CustomType != null) ? r.CustomType.Title : field.CustomTypeId);
			}
			r.Sort.Text = ((i + 1) * 10).ToString();
		}

		HtmlTableRowCollection tableRows = CustomFields.Rows;
		for (int i = 0; i < rows.Count; i++)
			tableRows.Remove(rows[i].Row);

		rows.Sort(delegate(RowControl a, RowControl b)
		{
			int i = ((a.CustomField == null) ? 1 : 0) - ((b.CustomField == null) ? 1 : 0);
			if (i != 0)
				return i;
			if (a.CustomField == null) // then b.CustomField == null
				return 0;
			return a.CustomField.Sort - b.CustomField.Sort;
		});

		int insertAt = 1;
		foreach (RowControl r in rows)
			tableRows.Insert(insertAt++, r.Row);
		
		for (int i = 0; i < fieldsAdded; i++)
			AddField(null, rows.Count + 1 + i);

		GenerateIds();
	}
	#endregion

	//NESTED CLASSES
	public class RowControl
	{
		public HtmlTableRow Row;
		public TextBox Name;
		public CheckBox Active;
		public DropDownList Type;
		public CheckBox Multiple;
		public TextBox Sort;
		public TextBox Code;
		public Button Edit;
		public BXIconButton Delete;
		public HiddenField State;
		public Label IdLabel;
		public Label CodeLabel;
		public Label TypeLabel;
		public Label MultipleLabel;
		public BXCustomField CustomField;
		public BXCustomType CustomType;
		public CustomValidator CodeValidator;
		public CustomValidator NameValidator;

		public void Store()
		{
			Store(CustomField, CustomType);
		}
		public void Store(BXCustomField field, BXCustomType type)
		{
			if (field == null)
			{
				State.Value = string.Empty;
				return;
			}
			BXParamsBag<object> state = new BXParamsBag<object>();
			state.Add("Settings", field.Settings);
			state.Add("CustomTypeId", field.CustomTypeId);
			state.Add("EditInList", field.EditInList);
			state.Add("FieldName", field.Name);
			state.Add("IsSearchable", field.IsSearchable);
			state.Add("Mandatory", field.Mandatory);
			state.Add("Multiple", field.Multiple);
			state.Add("ShowInFilter", (int)field.ShowInFilter);
			state.Add("ShowInList", field.ShowInList);
			state.Add("Sort", field.Sort);
			state.Add("XmlId", field.XmlId);

			foreach (BXCustomFieldLocalization l in field.Localization)
			{
				string[] phrases = new string[5];
				phrases[0] = l.TextEncoder.Decode(l.EditFormLabel);
				phrases[1] = l.TextEncoder.Decode(l.ErrorMessage);
				phrases[2] = l.TextEncoder.Decode(l.HelpMessage);
				phrases[3] = l.TextEncoder.Decode(l.ListColumnLabel);
				phrases[4] = l.TextEncoder.Decode(l.ListFilterLabel);

				state.Add("Loc." + l.LanguageId, phrases);
			}

			if (type != null)
			{

				IBXCustomTypeAdvancedSetting adv = null;
				try
				{
					adv = type.AdvancedSettings as IBXCustomTypeAdvancedSetting;
				}
				catch
				{
				}
				if (adv != null)
				{
					adv.Initialize(field);
					state.Add("Extras", adv.GetSettings());
				}
			}

			Store(state);
		}
		public void Store(BXParamsBag<object> state)
		{
			State.Value = Convert.ToBase64String(Encoding.Unicode.GetBytes(BXSerializer.Serialize(state)));
		}
		public BXParamsBag<object> Restore()
		{
			return
				BXSerializer.Deserialize(Encoding.Unicode.GetString(Convert.FromBase64String(State.Value))) as BXParamsBag<object> ??
				new BXParamsBag<object>();
		}
	}
}