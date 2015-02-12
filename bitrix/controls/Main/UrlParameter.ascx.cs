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
using Bitrix.Services;
using Bitrix.Services.Text;


public partial class bitrix_ui_UrlParameter : System.Web.UI.UserControl, Bitrix.UI.IUrlParameter
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    public string ParameterName
    {
        get { return Parameter.Text.Trim(); }
        set { Parameter.Text = value; }
    }

    public string ParameterValue
    {
        get { return Value.Text.Trim(); }
        set { Value.Text = value; }
    }

    public string Str
    {
        set
        {
            if (string.IsNullOrEmpty(value))
                return;

            ParameterName = BXStringUtility.StringToParam(value)[0];
            ParameterValue = BXStringUtility.StringToParam(value)[1];
        }
    }
}
