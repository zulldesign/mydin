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

public partial class BXCustomTypeFileSettings : BXControl, IBXCustomTypeSetting
{
	protected void Page_Load(object sender, EventArgs e)
	{
		DataBindChildren();
	}
	
	#region IBXCustomTypeSetting Members
	public BXParamsBag<object> GetSettings()
	{
		BXParamsBag<object> result = new BXParamsBag<object>();
		
		int i;
		if (int.TryParse(TextBoxSize.Text, out i))
			result.Add("TextBoxSize", i);
		if (int.TryParse(MaxSize.Text, out i))
			result.Add("MaxSize", i);
		
		string e = AllowedExtensions.Text.Trim();
		string[] es = e.Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
		List<string> l = new List<string>();
		foreach(string ext in es) 
		{
			string item = ext.Trim().ToLowerInvariant();
			if (!String.IsNullOrEmpty(item) && !l.Contains(item))
				l.Add(item);
		}
		if (l.Count > 0)
			result.Add("AllowedExtensions", l.ToArray());
		result.Add("AddDescription", AddDescription.Checked);
		return result;
	}

	public void SetSettings(BXParamsBag<object> settings)
	{
		if (settings.ContainsKey("TextBoxSize"))
			TextBoxSize.Text = settings["TextBoxSize"].ToString();
		if (settings.ContainsKey("MaxSize"))
			MaxSize.Text = settings["MaxSize"].ToString();
		if (settings.ContainsKey("AllowedExtensions"))
			AllowedExtensions.Text = string.Join(" ", (string[])settings["AllowedExtensions"]);
		AddDescription.Checked = settings.GetBool("AddDescription");
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
		}
	}
	#endregion
}
