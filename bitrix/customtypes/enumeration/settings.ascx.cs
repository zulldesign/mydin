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


public partial class BXCustomTypeEnumerationSettings : BXControl, IBXCustomTypeSetting
{
    #region IBXCustomTypeSetting Members

	public BXParamsBag<object> GetSettings()
	{
        BXParamsBag<object> settings = new BXParamsBag<object>();

        if (rbList.Checked)
            settings.Add("ViewMode", "list");

        int listSize;

        if (int.TryParse(ListSize.Text, out listSize))
            settings.Add("ListSize", listSize);
        else
            settings.Add("ListSize", 5);

        return settings;
	}

	public void SetSettings(BXParamsBag<object> settings)
	{
        ListSize.Text = settings.ContainsKey("ListSize") ? settings["ListSize"].ToString() : "5";
        rbList.Checked = settings.ContainsKey("ViewMode") ? true : false;
        rbFlags.Checked = !settings.ContainsKey("ViewMode") ? true : false;
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
