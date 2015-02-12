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
using Bitrix.DataTypes;


public partial class BXCustomTypeBooleanEdit : System.Web.UI.UserControl, IBXCustomTypeEdit
{
    //FIELDS
    private BXCustomField field;
    private BXCustomProperty value;

    protected override void OnInit(EventArgs e)
    {
        
        base.OnInit(e);
    }

    public void Initialize(BXCustomField currentField, BXCustomProperty currentValue)
    {
        field = currentField;
        value = currentValue;

		if (field == null)
			return;

		BXParamsBag<object> settings = new BXParamsBag<object>(field.Settings);

		MultiView1.ActiveViewIndex = settings.ContainsKey("view") ? (int)settings["view"] : 0;

		//BIND VALUE
		if (value != null)
		{
			if (value.Value != null && value.Value is bool)
			{
				bool flag = (bool)value.Value;
				chValue.Checked = flag;
				ddValue.SelectedIndex = flag ? 0 : 1;
				No.Checked = !flag;
				Yes.Checked = flag;

				return; //Skip default setup
			}
		}

		//BIND DEFAULT
		int defVal;
		if(settings.TryGetInt("default", out defVal))
			switch (defVal)
			{
				case 0: //True
					chValue.Checked = true;
					ddValue.SelectedIndex = 0;
					Yes.Checked = true;
					break;
				case 1: //False
					chValue.Checked = false;
					ddValue.SelectedIndex = 1;
					No.Checked = true;
					break;
			}
    }

    public void Save(BXCustomPropertyCollection storage)
    {
        if (field == null)
            return;

        object value = null;
        switch (MultiView1.ActiveViewIndex)
        {
            case 0:
                value = chValue.Checked;
                break;
            case 1:
                if (Yes.Checked)
                    value = true;
                else if (No.Checked)
                    value = false;
                break;
            case 2:
                if (ddValue.SelectedIndex == 0)
                    value = true;
                else if (ddValue.SelectedIndex == 1)
                    value = false;
                break;
        }
        BXCustomType t = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
        storage[field.Name] = new BXCustomProperty(field.Name, field.Id, t.DbType, t.IsClonable ? false : field.Multiple, value);
    }

    public void SetData(BXCustomProperty prop)
    {

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
}
