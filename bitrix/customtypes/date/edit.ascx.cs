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
using Bitrix.UI;

using System.Collections.Generic;
using Bitrix.DataTypes;

public partial class BXCustomTypeDateEdit : System.Web.UI.UserControl, IBXCustomTypeEdit
{
    //FIELDS
    private BXCustomField field;
    private BXCustomProperty value;
	private bool showTime = true;

	public void Initialize(BXCustomField currentField,BXCustomProperty value)
    {
        field = currentField;
        this.value = value;
		if (field == null)
			return;

		IBXCalendar cal = Calendar1 as IBXCalendar;

        valDate.Enabled = currentField.Mandatory;
		DateTime? dateTime = null;

		BXParamsBag<object> settings = new BXParamsBag<object>(currentField.Settings);
		showTime = settings.ContainsKey("showTime") ? (bool)settings["showTime"] : true;

        if (value == null)
        {
            if (cal != null)
            {
                if (settings.ContainsKey("default"))
					dateTime = (DateTime)settings["default"];

                if (settings.ContainsKey("current"))
					dateTime = DateTime.Now;				
            }
        }
        else
        {
            if (value.Value is DateTime)
				dateTime = (DateTime)value.Value;
        }

		if (dateTime.HasValue)
			txtDate.Text = showTime ? dateTime.Value.ToString() : dateTime.Value.ToString("d");
    }

	public void Save(BXCustomPropertyCollection storage)
    {
		if (field == null)
			return;

		IBXCalendar cal = Calendar1 as IBXCalendar;
		object value = null;
		if (cal != null && cal.Date != DateTime.MinValue)
			value = showTime ? cal.Date : cal.Date.Date;

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
            valDate.ValidationGroup = value;
			// IMPLEMENT SET VALIDATORS
		}
	}
	

}
