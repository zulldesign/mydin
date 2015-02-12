using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Bitrix;

using Bitrix.UI;
using Bitrix.Services;
using Bitrix.DataTypes;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.Services.Text;

public partial class BXCustomTypeListAdvancedSettings : BXControl, IBXCustomTypeAdvancedSetting
{
	//FIELDS
	private BXCustomField field;
	private string validationGroup;
	private int quantity = 0;
	private int listSize = 0;
	bool initialized;
	Dictionary<object, TextBox> valueForXmlId = new Dictionary<object, TextBox>();
	Dictionary<object, TextBox> xmlIdForValue = new Dictionary<object, TextBox>();

	//PROPERTIES
	public int Quantity
	{
		get
		{
			if (quantity == 0)
			{
				if (Page == null || Request.Form[Q.UniqueID] == null || !int.TryParse(Request.Form[Q.UniqueID], out quantity))
					quantity = 1;
				quantity = Math.Max(1, quantity);
			}
			return (quantity > BXConfigurationUtility.Constants.MaxFieldQuantity) ? BXConfigurationUtility.Constants.MaxFieldQuantity : quantity;
		}

		set
		{
			quantity = value;
		}
	}
	public BXCustomField Field
	{
		get
		{
			return field;
		}
		set
		{
			field = value;
		}
	}
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

	//METHODS
	private void BindField()
	{
		//ListValue.Rows.Clear();
		int offset = 0;
		bool defaultSelected = false;

		if (field != null)
		{
			BXCustomFieldEnumCollection values = BXCustomFieldEnum.GetList(field.Id, field.FieldType, BXTextEncoder.EmptyTextEncoder);
			listSize = values.Count;
			foreach (BXCustomFieldEnum item in values)
			{
				defaultSelected = item.Default ? true : defaultSelected;
				AddRow(offset, item);
				offset++;
			}
		}

		if (Quantity < offset + 1)
			Quantity = offset + 1;

		for (int i = offset; i < Quantity; i++)
			AddRow(i, null);

		//rdNone.Checked = !defaultSelected;
		initialized = true;
	}
	private void AddRow(int index, BXCustomFieldEnum item)
	{
		HtmlTableRow newRow = new HtmlTableRow();

		HtmlTableCell newCell = new HtmlTableCell();
		Label newID = new Label();
		newCell.Controls.Add(newID);
		newRow.Cells.Add(newCell);

		newCell = new HtmlTableCell();
		TextBox newXmlId = new TextBox();
		newXmlId.ID = string.Format("XMLID_{0}", index);
		newXmlId.Columns = 15;
		newCell.Controls.Add(newXmlId);
		CustomValidator newXmlIdValidator = new CustomValidator();
		newXmlIdValidator.ControlToValidate = newXmlId.ID;
		newXmlIdValidator.ValidateEmptyText = true;
		newXmlIdValidator.ValidationGroup = ValidationGroup;
		newXmlIdValidator.ServerValidate += new ServerValidateEventHandler(newXmlIdValidator_ServerValidate);
		newXmlIdValidator.ClientValidationFunction = "customTypeListAdvancedSettings_validateXmlId";
		newXmlIdValidator.Text = "*";
		newXmlIdValidator.ErrorMessage = GetMessage("Message.XmlIdRequired");
		newXmlIdValidator.Display = ValidatorDisplay.Static;
		newCell.Controls.Add(newXmlIdValidator);
		newRow.Cells.Add(newCell);

		newCell = new HtmlTableCell();
		TextBox newValue = new TextBox();
		newValue.ID = string.Format("VALUE_{0}", index);
		newValue.Columns = 35;
		newCell.Controls.Add(newValue);
		CustomValidator newValueValidator = new CustomValidator();
		newValueValidator.ControlToValidate = newValue.ID;
		newValueValidator.ValidateEmptyText = true;
		newValueValidator.ValidationGroup = ValidationGroup;
		newValueValidator.ServerValidate += new ServerValidateEventHandler(newValueValidator_ServerValidate);
		newValueValidator.ClientValidationFunction = "customTypeListAdvancedSettings_validateValue";
		newValueValidator.Text = "*";
		newValueValidator.ErrorMessage = GetMessage("Message.ValueRequired");
		newValueValidator.Display = ValidatorDisplay.Static;
		newCell.Controls.Add(newValueValidator);
		newRow.Cells.Add(newCell);

		valueForXmlId.Add(newXmlIdValidator, newValue);
		xmlIdForValue.Add(newValueValidator, newXmlId);

		newCell = new HtmlTableCell();
		TextBox newSort = new TextBox();
		newSort.ID = string.Format("SORT_{0}", index);
		newSort.Columns = 5;
		newSort.Text = string.Format("{0}", index * 10 + 10);
		newCell.Controls.Add(newSort);
		newRow.Cells.Add(newCell);

		newCell = new HtmlTableCell();
		CheckBox newDefault = new CheckBox();
		newDefault.ID = string.Format("DEFAULT_{0}", index);
		newCell.Controls.Add(newDefault);
		newRow.Cells.Add(newCell);

		newCell = new HtmlTableCell();
		CheckBox newFlag = new CheckBox();
		newFlag.ID = string.Format("FLAG_{0}", index);
		newCell.Controls.Add(newFlag);
		newRow.Cells.Add(newCell);

		if (item != null)
		{
			newID.Text = (item.Id != 0) ? item.Id.ToString() : string.Empty;
			newXmlId.Text = item.XmlId;
			newValue.Text = item.Value;
			newDefault.Checked = item.Default;
		}
		else
		{
			newID.Text = string.Empty;
			newXmlId.Text = string.Empty;
			newValue.Text = string.Empty;
		}
		newFlag.Checked = false;

		ListValue.Rows.Add(newRow);
	}

	void newXmlIdValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = !string.IsNullOrEmpty(args.Value) || string.IsNullOrEmpty(valueForXmlId[source].Text);
	}

	void newValueValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = !string.IsNullOrEmpty(args.Value) || string.IsNullOrEmpty(xmlIdForValue[source].Text);
	}


	protected override void OnInit(EventArgs e)
	{
		if (!initialized)
			BindField();
		base.OnInit(e);
	}
	protected override void OnPreRender(EventArgs e)
	{
		Q.Value = Quantity.ToString();
		base.OnPreRender(e);
	}
	protected void Button1_Click(object sender, EventArgs e)
	{
		AddRow(listSize + Quantity, null);
		Quantity++;
	}

	#region IBXAdvancedSetting Implementation
	public void Initialize(BXCustomField currentField)
	{
		field = currentField;
	}
	public object GetSettings()
	{
		if (!initialized)
			BindField();
		List<BXParamsBag<object>> storage = new List<BXParamsBag<object>>();

		for (int i = 2; i < ListValue.Rows.Count; i++)
		{
			if (((CheckBox)ListValue.Rows[i].Cells[5].Controls[0]).Checked)
				continue;


			string xmlId = ((TextBox)ListValue.Rows[i].Cells[1].Controls[0]).Text;

			string value = ((TextBox)ListValue.Rows[i].Cells[2].Controls[0]).Text;

			string sortStr = ((TextBox)ListValue.Rows[i].Cells[3].Controls[0]).Text;

			int sort;
			if (!int.TryParse(sortStr, out sort))
				sort = 100;

			bool isDefault = ((CheckBox)ListValue.Rows[i].Cells[4].Controls[0]).Checked;


			if (string.IsNullOrEmpty(xmlId)
			|| string.IsNullOrEmpty(value))
				continue;

			BXParamsBag<object> data = new BXParamsBag<object>();
			//data.Add("Id", )
			data.Add("XmlId", xmlId);
			data.Add("Value", value);
			data.Add("Default", isDefault);
			data.Add("Sort", sort);

			storage.Add(data);
		}
		return storage;
	}
	public void SetSettings(object settings)
	{
		List<BXParamsBag<object>> bindingState = settings as List<BXParamsBag<object>> ?? new List<BXParamsBag<object>>();
		while (ListValue.Rows.Count > 2)
			ListValue.Rows.RemoveAt(2);

		int offset = 0;
		bool defaultSelected = false;

		listSize = bindingState.Count;

		foreach (BXParamsBag<object> data in bindingState)
		{
			BXCustomFieldEnum item = new BXCustomFieldEnum(BXTextEncoder.EmptyTextEncoder);
			item.XmlId = data.Get("XmlId", string.Empty);
			item.Value = data.Get("Value", string.Empty);
			item.Default = data.Get("Default", false);
			item.Sort = data.Get("Sort", 100);

			defaultSelected = item.Default ? true : defaultSelected;
			AddRow(offset, item);
			offset++;
		}
		if (Quantity < offset + 1)
			Quantity = offset + 1;

		for (int i = offset; i < Quantity; i++)
			AddRow(i, null);

		//rdNone.Checked = !defaultSelected;
		initialized = true;
	}

	public void Save()
	{
		BXCustomFieldEnum.Delete(
			new BXFilter(
				new BXFilterItem(BXCustomFieldEnum.Fields.FieldType, BXSqlFilterOperators.Equal, field.FieldType),
				new BXFilterItem(BXCustomFieldEnum.Fields.FieldId, BXSqlFilterOperators.Equal, field.Id)
			)
		);

		bool firstRadio = false;
		BXCustomFieldEnumCollection values = new BXCustomFieldEnumCollection();
		for (int i = 2; i < ListValue.Rows.Count; i++)
		{
			if (((CheckBox)ListValue.Rows[i].Cells[5].Controls[0]).Checked)
				continue;

			string xmlId = ((TextBox)ListValue.Rows[i].Cells[1].Controls[0]).Text;
			string value = ((TextBox)ListValue.Rows[i].Cells[2].Controls[0]).Text;
			string sortStr = ((TextBox)ListValue.Rows[i].Cells[3].Controls[0]).Text;

			int sort;
			if (!int.TryParse(sortStr, out sort))
				sort = 100;

			bool isDefault = ((CheckBox)ListValue.Rows[i].Cells[4].Controls[0]).Checked;
			if (!field.Multiple)
			{
				if (!firstRadio && isDefault)
					firstRadio = true;
				else
					isDefault = false;
			}

			if (string.IsNullOrEmpty(xmlId)
			|| string.IsNullOrEmpty(value))
				continue;

			BXCustomFieldEnum item = new BXCustomFieldEnum();

			item.Default = isDefault;
			item.Sort = sort;
			item.FieldType = field.FieldType;
			item.FieldId = field.Id;
			item.XmlId = xmlId;
			item.Value = value;

			values.Add(item);
		}
		values.Save();

		while (ListValue.Rows.Count != 2)
			ListValue.Rows.RemoveAt(2);

		Quantity = 1;

		BindField();
	}
	#endregion
}
