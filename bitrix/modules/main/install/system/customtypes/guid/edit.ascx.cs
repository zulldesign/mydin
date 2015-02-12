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
using System.Data.SqlTypes;

public partial class BXCustomTypeGuidEdit : BXControl, IBXCustomTypeEdit
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
			ValueRequired.ErrorMessage = GetMessageFormat("FieldMustBeFilled", fieldName);
		ValueMask.ErrorMessage = GetMessageFormat("ValueMustBeGuid", fieldName);

		string v = null;

        if (value != null && value.Values.Count > 0)
		{
			object val = value.Value;
			if (val != null)
			{
				if (val is SqlGuid)
					v = ((SqlGuid)val).Value.ToString();
				else if (val is Guid)
					v = ((Guid)val).ToString();
			}
		}
		else if (settings.GetBool("GenerateDefault"))
			v = Guid.NewGuid().ToString();
		ValueTextBox.Text = v ?? "";
    }

	public void Save(BXCustomPropertyCollection storage)
    {
		if (field == null)
			return;
		
		object value;
		try
		{
			value = new SqlGuid(ValueTextBox.Text.Trim().TrimStart('{').TrimEnd('}').Replace("-", ""));
		}
		catch
		{
			value = null;
		}

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
