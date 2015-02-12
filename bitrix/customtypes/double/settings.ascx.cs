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

using Bitrix.UI;
using Bitrix.DataTypes;
using System.Globalization;

public partial class BXCustomTypeDoubleSettings : BXControl, IBXCustomTypeSetting
{
	protected bool TryParseDouble(string s, out double val)
	{
		return double.TryParse(s, /*NumberStyles.Float, CultureInfo.InvariantCulture,*/ out val);
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		DataBindChildren();
		//DefaultValueConditions.ClientValidationFunction = ClientID + "_ValidateDefault";
	}

	#region IBXCustomTypeSetting Members

	public BXParamsBag<object> GetSettings()
	{
		BXParamsBag<object> result = new BXParamsBag<object>();
		int i;
		if (int.TryParse(Precision.Text, out i))
			if (i > 0)
				result.Add("Precision", i);
			else
				result.Add("Precision", 0);
		else
			result.Add("Precision", 0);

		double val;
		if (TryParseDouble(DefaultValue.Text, out val))
			result.Add("DefaultValue", val);
		if (int.TryParse(TextBoxSize.Text, out i))
			result.Add("TextBoxSize", i);
		if (TryParseDouble(MinValue.Text, out val))
			result.Add("MinValue", val);
		if (TryParseDouble(MaxValue.Text, out val))
			result.Add("MaxValue", val);
		return result;
	}

	public void SetSettings(BXParamsBag<object> settings)
	{
		Precision.Text = settings.ContainsKey("Precision") ? settings["Precision"].ToString() : String.Empty;
		DefaultValue.Text = settings.ContainsKey("DefaultValue") ? settings.Get<double>("DefaultValue").ToString(CultureInfo.InvariantCulture) : String.Empty;
		TextBoxSize.Text = settings.ContainsKey("TextBoxSize") ? settings["TextBoxSize"].ToString() : String.Empty;
		MinValue.Text = settings.ContainsKey("MinValue") ? settings["MinValue"].ToString().ToString(CultureInfo.InvariantCulture) : String.Empty;
		MaxValue.Text = settings.ContainsKey("MaxValue") ? settings["MaxValue"].ToString().ToString(CultureInfo.InvariantCulture) : String.Empty;
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
		double cur;
		if (!TryParseDouble(args.Value, out cur))
			return;
		double limit;
		if (TryParseDouble(MinValue.Text, out limit) && cur < limit)
		{
			args.IsValid = false;
			return;
		}
		if (TryParseDouble(MaxValue.Text, out limit) && cur > limit)
		{
			args.IsValid = false;
			return;
		}
	}
}
