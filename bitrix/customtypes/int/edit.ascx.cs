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

public partial class BXCustomTypeIntEdit : BXControl, IBXCustomTypeEdit
{
	private BXCustomField field;
    private BXCustomProperty value;

	protected void Page_Load(object sender, EventArgs e)
	{
		DataBindChildren();
	}


    #region IBXCustomTypeEdit Members
    public void Initialize(BXCustomField currentField, BXCustomProperty currentValue)
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

		ValueTextBox.Columns = settings.ContainsKey("TextBoxSize") ? (int)settings["TextBoxSize"] : 20;

		ValueRange.Enabled = true;
		ValueRange.MinimumValue = int.MinValue.ToString();
		ValueRange.MaximumValue = int.MaxValue.ToString();
		ValueRange.ErrorMessage = GetMessageFormat("Error.DefaultRangeInvalid", fieldName);
		if (settings.ContainsKey("MinValue"))
		{
			int min = (int)settings["MinValue"];
			ValueRange.MinimumValue = min.ToString();
		}
		if (settings.ContainsKey("MaxValue"))
		{
			int max = (int)settings["MaxValue"];
			ValueRange.MaximumValue = max.ToString();
		}

        if (value != null)
			ValueTextBox.Text = string.Format("{0}", value.Value);
        else
			ValueTextBox.Text = settings.ContainsKey("DefaultValue") ? settings["DefaultValue"].ToString() : String.Empty;
    }

	public void Save(BXCustomPropertyCollection storage)
    {
		if (field == null)
			return;
		
		int i;
		object value = null;
		if (int.TryParse(ValueTextBox.Text, out i))
			value = i;
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
