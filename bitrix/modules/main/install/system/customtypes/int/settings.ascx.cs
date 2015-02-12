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

public partial class BXCustomTypeIntSettings : BXControl, IBXCustomTypeSetting
{
	protected void Page_Load(object sender, EventArgs e)
	{
		DataBindChildren();
		DefaultValueConditions.ClientValidationFunction = ClientID + "_ValidateDefault";
	}

	#region IBXCustomTypeSetting Members

	public BXParamsBag<object> GetSettings()
	{
		BXParamsBag<object> result = new BXParamsBag<object>();
		int i;
		if (int.TryParse(DefaultValue.Text, out i))
			result.Add("DefaultValue", i);
		if (int.TryParse(TextBoxSize.Text, out i))
			result.Add("TextBoxSize", i);
		if (int.TryParse(MinValue.Text, out i))
			result.Add("MinValue", i);
		if (int.TryParse(MaxValue.Text, out i))
			result.Add("MaxValue", i);
		return result;
	}

	public void SetSettings(BXParamsBag<object> settings)
	{
		DefaultValue.Text = settings.ContainsKey("DefaultValue") ? settings["DefaultValue"].ToString() : String.Empty;
		TextBoxSize.Text = settings.ContainsKey("TextBoxSize") ? settings["TextBoxSize"].ToString() : String.Empty;
		MinValue.Text = settings.ContainsKey("MinValue") ? settings["MinValue"].ToString() : String.Empty;
		MaxValue.Text = settings.ContainsKey("MaxValue") ? settings["MaxValue"].ToString() : String.Empty;
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

	protected void DefaultValueConditions_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = true;
		int i;
		if (!int.TryParse(args.Value, out i))
			return;

		int j;
		if (int.TryParse(MinValue.Text, out j) && i < j)
		{
			args.IsValid = false;
			return;
		}
		if (int.TryParse(MaxValue.Text, out j) && i > j)
		{
			args.IsValid = false;
			return;
		}
	}
}
