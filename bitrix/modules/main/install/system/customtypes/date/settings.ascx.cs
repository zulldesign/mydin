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

using System.Globalization;
using Bitrix.DataTypes;

public partial class BXCustomTypeDateSettings : BXControl, IBXCustomTypeSetting
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

	#region IBXCustomTypeSetting Members

	public BXParamsBag<object> GetSettings()
	{
        BXParamsBag<object> settings = new BXParamsBag<object>();

        if (rbCurrent.Checked)
            settings.Add("current", "true");

		settings.Add("showTime", showTime.Checked);

        if (rbCustom.Checked)
        {
            IBXCalendar cal = Calendar1 as IBXCalendar;
            if (cal != null && !cal.Date.Equals(DateTime.MinValue))
                settings.Add("default", cal.Date);
        }

        return settings;
	}

	public void SetSettings(BXParamsBag<object> settings)
	{
        if (settings.ContainsKey("default"))
        {
            rbCustom.Checked = true;
            IBXCalendar cal = Calendar1 as IBXCalendar;
            if (cal != null)
                cal.Date = (DateTime)settings["default"];
        }

		showTime.Checked = settings.ContainsKey("showTime") ? (bool)settings["showTime"] : true;

        if (settings.ContainsKey("current"))
            rbCurrent.Checked = true;

        rbNone.Checked = true;
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
