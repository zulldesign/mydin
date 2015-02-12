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
using Bitrix;

using System.Text;
using Bitrix.Services;
using Bitrix.UI;
using System.Collections.Generic;
using Bitrix.DataTypes;

public partial class BXCustomTypeStringEdit : BXControl, IBXCustomTypeEdit
{
	//FIELDS
	string minValidationScript;
	string maxValidationScript;
	int? minLength;
	int? maxLength;
	BXCustomProperty value;
	BXCustomField field;

	//METHODS
	private void RegisterJavaScript()
	{
		StringBuilder s = new StringBuilder();
		bool register = false;
		if (!String.IsNullOrEmpty(minValidationScript))
		{
			s.AppendFormat("function {0}(source,args){{args.IsValid=false;{1}args.IsValid=true}}\n", ClientID + "_ValidateMinLength", minValidationScript);
			ValueMinLength.ClientValidationFunction = ClientID + "_ValidateMinLength";
			register = true;
		}
		if (!String.IsNullOrEmpty(maxValidationScript))
		{
			s.AppendFormat("function {0}(source,args){{args.IsValid=false;{1}args.IsValid=true}}\n", ClientID + "_ValidateMaxLength", maxValidationScript);
			ValueMaxLength.ClientValidationFunction = ClientID + "_ValidateMaxLength";
			register = true;
		}
		if (register)
			ScriptManager.RegisterClientScriptBlock(Page, GetType(), ClientID, s.ToString(), true);
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		DataBindChildren();
	}
	protected void Page_PreRender(object sender, EventArgs e)
	{
		RegisterJavaScript();
	}

	protected void ValueMinLength_ServerValidate(object source, ServerValidateEventArgs args)
	{

		args.IsValid = false;
		if (minLength.HasValue && minLength.Value > 0 && args.Value.Length < minLength.Value)
			return;
		args.IsValid = true;
	}
	protected void ValueMaxLength_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = false;
		if (maxLength.HasValue && maxLength.Value > 0 && args.Value.Length > maxLength.Value)
			return;
		args.IsValid = true;
	}

	#region IBXCustomTypeEdit Members
	public void Initialize(BXCustomField currentField, BXCustomProperty currentValue)
	{
		field = currentField;
		value = currentValue;
		if (currentField == null)
			return;

		string fieldName = currentField.EditFormLabel;
		string errorMessage = currentField.ErrorMessage;
		if (string.IsNullOrEmpty(errorMessage))
			errorMessage = GetMessageFormat("Error.RegexInvalid", fieldName);

		BXParamsBag<object> settings = new BXParamsBag<object>(currentField.Settings);
		ValueRequired.Enabled = currentField.Mandatory;
		if (ValueRequired.Enabled)
			ValueRequired.ErrorMessage = GetMessageFormat("Error.Required", fieldName);

		int i = settings.ContainsKey("RowsCount") ? settings.GetInt("RowsCount") : 1;
		ValueTextBox.TextMode = (i > 1) ? TextBoxMode.MultiLine : TextBoxMode.SingleLine;
		if (ValueTextBox.TextMode == TextBoxMode.MultiLine)
			ValueTextBox.Rows = i;



		ValueTextBox.Columns = settings.ContainsKey("TextBoxSize") ? settings.GetInt("TextBoxSize") : 20;

		minLength = null;
		if (settings.ContainsKey("MinLength"))
			minLength = settings.GetInt("MinLength");
		maxLength = null;
		if (settings.ContainsKey("MaxLength"))
			maxLength = settings.GetInt("MaxLength");

		ValueMinLength.Enabled = minLength.HasValue && minLength > 0;
		ValueMaxLength.Enabled = maxLength.HasValue && maxLength > 0;
		minValidationScript = null;
		maxValidationScript = null;
		if (ValueMinLength.Enabled)
		{
			minValidationScript = string.Format("if(!args.Value||args.Value.length<{0})return;", minLength.Value);
			ValueMinLength.ErrorMessage = GetMessageFormat("Error.MinLength", fieldName, minLength.Value);
		}
		if (ValueMaxLength.Enabled)
		{
			maxValidationScript = string.Format("if(args.Value&&args.Value.length>{0})return;", maxLength.Value);
			ValueMaxLength.ErrorMessage = GetMessageFormat("Error.MaxLength", fieldName, maxLength.Value);
		}

		ValueRegex.Enabled = settings.ContainsKey("ValidationRegex") && !String.IsNullOrEmpty((string)settings["ValidationRegex"]);
		if (ValueRegex.Enabled)
		{
			ValueRegex.ValidationExpression = (string)settings["ValidationRegex"];
			ValueRegex.ErrorMessage = errorMessage;
		}

		if (value != null && value.Value is string)
			ValueTextBox.Text = value.Value.ToString();
		else
			ValueTextBox.Text = settings.ContainsKey("DefaultValue") ? (string)settings["DefaultValue"] : String.Empty;
	}

	public void Save(BXCustomPropertyCollection storage)
	{
		if (field == null)
			return;

		object value = ValueTextBox.Text;
		if (string.IsNullOrEmpty(ValueTextBox.Text) && field.Multiple)
			value = null;
		
		BXCustomType t = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
		storage[field.Name] = new BXCustomProperty(field.Name, field.Id, t.DbType, t.IsClonable ? false : field.Multiple, value);
	}

	private string validationGroup = String.Empty;
	public string ValidationGroup
	{
		get
		{
			return validationGroup;
		}
		set
		{
			validationGroup = value;
			// IMPLEMENT SET VALIDATORS
		}
	}
	#endregion
}
