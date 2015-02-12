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
using Bitrix.Security;
using Bitrix.UI;

public partial class bitrix_admin_UpdateSystemLog : BXAdminPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate("UpdateSystem"))
			BXAuthentication.AuthenticationRequired();

	}
}
