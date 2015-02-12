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
using Bitrix.DataTypes;


public partial class BXCustomTypeBooleanSettings : BXControl, IBXCustomTypeSetting
{
 	#region IBXCustomTypeSetting Members

	public BXParamsBag<object> GetSettings()
	{
		BXParamsBag<object> settings = new BXParamsBag<object>();

        if (rbCheckbox.Checked)
            settings.Add("view", 0);

        if (rbRadiobuttons.Checked)
            settings.Add("view", 1);

        if (rbDropDown.Checked)
            settings.Add("view", 2);

        settings.Add("default",DefaultValue.SelectedIndex);

		return settings;
	}

	public void SetSettings(BXParamsBag<object> settings)
	{
        if (settings.ContainsKey("view"))
        {
            switch ((int)settings["view"])
            {
                case 0:
                    rbCheckbox.Checked = true;
                    break;
                case 1:
                    rbRadiobuttons.Checked = true;
                    break;
                case 2:
                    rbDropDown.Checked = true;
                    break;
            }
        }

        if (settings.ContainsKey("default"))
        {
            DefaultValue.SelectedIndex = (int)settings["default"];
        }
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
