using System;

using Bitrix.UI;
using System.ComponentModel;
using Bitrix.Services;
using System.Web.UI.WebControls;
using Bitrix.Services.Js;
using Bitrix.IO;
using Bitrix.Security;

public partial class bitrix_admin_controls_Main_AdminUserLink : BXControl
{
    public string UserID;

    protected void Page_PreRender(object sender, EventArgs e)
    {
        BXUser u = BXUser.GetById(UserID);
        if (u != null)
        {
            string result = u.GetDisplayName();
            result = String.Format("<a href=\"AuthUsersEdit.aspx?id={0}\">{1}</a>", UserID, result);
            this.UserLink.Text = result;
        }
        else
            this.UserLink.Text = "N/A";
    }
}