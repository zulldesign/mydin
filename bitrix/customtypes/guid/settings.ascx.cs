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
using Bitrix.DataTypes;
using Bitrix.UI;

public partial class BXCustomTypeGuidSettings : BXControl, IBXCustomTypeSetting
{
	#region IBXCustomTypeSetting Members

	public BXParamsBag<object> GetSettings()
	{
		BXParamsBag<object> result = new BXParamsBag<object>();
		result["GenerateDefault"] = GenerateDefault.Checked;
		return result;
	}

	public void SetSettings(BXParamsBag<object> settings)
	{
		GenerateDefault.Checked = settings.GetBool("GenerateDefault");
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
