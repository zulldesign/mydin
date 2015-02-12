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
using System.Text;

public partial class BXCustomTypeStringSettings : BXControl, IBXCustomTypeSetting
{
	protected void Page_Load(object sender, EventArgs e)
	{
		DataBindChildren();
		DefaultValueConditions.ClientValidationFunction = ClientID + "_ValidateDefault";
        string textTypeOnClick = string.Concat(HandleTypeChangeFunctionName, "();");
        TextButton.Attributes.Add("onclick", textTypeOnClick);
        HtmlButton.Attributes.Add("onclick", textTypeOnClick);
        PatternButton.Attributes.Add("onclick", textTypeOnClick);
	}

    protected string HandleTypeChangeFunctionName
    {
        get { return ClientID + "_HandleTypeChange"; }
    }
    protected string PutMacroParamInPatternFunctionName
    {
        get { return ClientID + "_PutMacroParamInPattern"; }
    }
	#region IBXCustomTypeSetting Members

	public BXParamsBag<object> GetSettings()
	{
		BXParamsBag<object> result = new BXParamsBag<object>();
		int i;
		result.Add("DefaultValue", DefaultValue.Text);
		if (int.TryParse(TextBoxSize.Text, out i))
			result.Add("TextBoxSize", i);
		if (int.TryParse(RowsCount.Text, out i))
			result.Add("RowsCount", i);
		if (int.TryParse(MinLength.Text, out i))
			result.Add("MinLength", i);
		if (int.TryParse(MaxLength.Text, out i))
			result.Add("MaxLength", i);

		result.Add("TextType", PatternButton.Checked ? "pattern" : HtmlButton.Checked ? "html" : "text");
        result.Add("Pattern", Pattern.Text);
		result.Add("ValidationRegex", ValidationRegex.Text);
		return result;
	}

	public void SetSettings(BXParamsBag<object> settings)
	{
        string textType = (settings.GetString("TextType") ?? "").ToUpperInvariant();
        if (string.Equals(textType, "PATTERN", StringComparison.InvariantCulture))
        {
            PatternButton.Checked = true;
            PatternContainer.Style.Add(HtmlTextWriterStyle.Display, "");
        }
        else if (string.Equals(textType, "HTML", StringComparison.InvariantCulture))
            HtmlButton.Checked = true;
        else
            TextButton.Checked = true;

        Pattern.Text = settings.ContainsKey("Pattern") ? (string)settings["Pattern"] : String.Empty;
		DefaultValue.Text = settings.ContainsKey("DefaultValue") ? (string)settings["DefaultValue"] : String.Empty;
		TextBoxSize.Text = settings.ContainsKey("TextBoxSize") ? settings["TextBoxSize"].ToString() : String.Empty;
		RowsCount.Text = settings.ContainsKey("RowsCount") ? settings["RowsCount"].ToString() : String.Empty;
		MinLength.Text = settings.ContainsKey("MinLength") ? settings["MinLength"].ToString() : String.Empty;
		MaxLength.Text = settings.ContainsKey("MaxLength") ? settings["MaxLength"].ToString() : String.Empty;
		ValidationRegex.Text = settings.ContainsKey("ValidationRegex") ? (string)settings["ValidationRegex"] : String.Empty;
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
		int i = DefaultValue.Text.Length;
		int j;
		if (int.TryParse(MinLength.Text, out j) && j != 0 && i < j)
		{
			args.IsValid = false;
			return;
		}
		if (int.TryParse(MaxLength.Text, out j) && j != 0 && i > j)
		{
			args.IsValid = false;
			return;
		}
	}
}
