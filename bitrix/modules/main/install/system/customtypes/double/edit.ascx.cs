using System;
using System.Collections;
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

using Bitrix.Services;
using Bitrix.UI;
using System.Collections.Generic;
using Bitrix.DataTypes;

public partial class BXCustomTypeDoubleEdit : BXControl, IBXCustomTypeEdit
{
	private BXCustomField field;
    private BXCustomProperty value;

    protected void Page_Load(object sender, EventArgs e)
    {
        DataBindChildren(); //TO BIND VALIDATIONGROUP
    }

    #region IBXCustomTypeEdit Members
    public void Initialize(BXCustomField currentField,BXCustomProperty currentValue)
    {
		field = currentField;
        value = currentValue;

		if (field == null)
			return;

		string fieldName = currentField.EditFormLabel;

		BXParamsBag<object> settings = new BXParamsBag<object>(currentField.Settings);
		ValueRequired.Enabled = currentField.Mandatory;
		if (ValueRequired.Enabled)
			ValueRequired.ErrorMessage = GetMessageFormat("Error.Required", fieldName);


		ValueTextBox.Columns = settings.ContainsKey("TextBoxSize") ? settings.GetInt("TextBoxSize") : 20;

		ValueMin.Enabled = ValueMax.Enabled = false;
		ValueType.ErrorMessage = 
		ValueMax.ErrorMessage =
		ValueMin.ErrorMessage =
			GetMessageFormat("Error.DefaultRangeInvalid", fieldName);

		if (settings.ContainsKey("MinValue"))
		{
			double min = Convert.ToDouble(settings["MinValue"]);// is int ? (double)((int)settings["MinValue"]) : (double)settings["MinValue"];
			ValueMin.ValueToCompare = min.ToString();
			ValueMin.Enabled = true;
		}
		if (settings.ContainsKey("MaxValue"))
		{
			double max = Convert.ToDouble(settings["MaxValue"]); //is int ? (double)((int)settings["MaxValue"]) : (double)settings["MaxValue"];
			ValueMax.ValueToCompare = max.ToString();
			ValueMax.Enabled = true;
		}

		int precision = settings.ContainsKey("Precision") ? settings.GetInt("Precision") : 4;

        if (value != null)
			ValueTextBox.Text = string.Format("{0:F" + precision + "}", value.Value);
        else
			ValueTextBox.Text = settings.ContainsKey("DefaultValue") ? settings["DefaultValue"].ToString() : String.Empty;
    }

	public void Save(BXCustomPropertyCollection storage)
    {
		if (field == null)
			return;

		double d;
		object value = null;
		if (double.TryParse(ValueTextBox.Text, out d))
			value = d;
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
