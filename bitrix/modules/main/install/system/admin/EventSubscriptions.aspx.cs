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
using Bitrix.Services;
using Bitrix.Security;

public partial class bitrix_admin_EventSubscriptions : Bitrix.UI.BXAdminPage
{
    protected override void OnLoad(EventArgs e)
    {
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

        base.OnLoad(e);
        MasterTitle = GetMessage("MasterTitle");
    }
}
