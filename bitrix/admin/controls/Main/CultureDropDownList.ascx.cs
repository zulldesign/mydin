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
using System.Globalization;
using System.Collections.Generic;

public partial class CultureDropDownList : System.Web.UI.UserControl
{
	protected void Page_Init(object sender, EventArgs e)
	{
		List<CultureInfo> cultures = new List<CultureInfo>();
		foreach (CultureInfo c in CultureInfo.GetCultures(CultureTypes.AllCultures))
		{
			if (c.IsNeutralCulture)
				continue;
			cultures.Add(c);
		}

		cultures.Sort(delegate(CultureInfo a, CultureInfo b)
		{
			return a.DisplayName.CompareTo(b.DisplayName);
		});

		foreach (CultureInfo c in cultures)
			L.Items.Add(new ListItem(c.DisplayName, c.Name));

		L.SelectedValue = CultureInfo.CurrentCulture.Name;
	}

	public string  SelectedCulture
	{
		get
		{
			return L.SelectedValue ?? CultureInfo.CurrentCulture.Name;
		}
		set
		{
			try
			{
				L.SelectedValue = value;
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
	}

}
