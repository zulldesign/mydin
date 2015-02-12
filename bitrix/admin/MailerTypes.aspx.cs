using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.ObjectModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.Text;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Security;


public partial class bitrix_admin_MailerTypes : Bitrix.UI.BXAdminPage
{

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        MasterTitle = GetMessage("EventTypes.MasterTitle");

		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

    }
}
