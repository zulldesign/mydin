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
using Bitrix.UI;

public partial class bitrix_components_bitrix_system_auth_templates__default_PasswordRecovery : BXComponentTemplate
{
    protected void Page_Load(object sender, EventArgs e)
    {
		if ((bool)Results["EnablePasswordRecovery"])
		{
			if ((bool)Results["RequiresQuestionAndAnswer"])
			{
				IncludeComponent1.Visible = true;
				IncludeComponent2.Visible = false;
			}
			else
			{
				if ((bool)Results["RequiresCheckWord"])
				{
					IncludeComponent1.Visible = false;
					IncludeComponent2.Visible = true;
				}
				else
				{
					IncludeComponent1.Visible = false;
					IncludeComponent2.Visible = false;
				}
			}
		}
		else
		{
			IncludeComponent1.Visible = false;
			IncludeComponent2.Visible = false;
		}
	}
}
