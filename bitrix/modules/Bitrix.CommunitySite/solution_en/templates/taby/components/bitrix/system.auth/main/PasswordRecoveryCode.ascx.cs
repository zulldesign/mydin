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

public partial class bitrix_components_bitrix_system_auth_templates__default_PasswordRecoveryCode : BXComponentTemplate
{
    protected void Page_Load(object sender, EventArgs e)
    {
		IncludeComponent1.Visible = ((bool)Results["EnablePasswordRecovery"] && (bool)Results["RequiresCheckWord"]);
	}
}
