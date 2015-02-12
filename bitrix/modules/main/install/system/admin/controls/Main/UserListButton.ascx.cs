using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;

using Bitrix.Services;
using Bitrix.UI;
using System.Collections.Generic;
using Bitrix.IBlock;
using Bitrix.DataTypes;
using Bitrix.DataLayer;
using Bitrix.Services.Js;
using Bitrix.Security;

public partial class UserListButton : BXControl
{
    private int _userId;
    public int UserId
    {
        get
		{ 
			int.TryParse(tbValue.Text, out _userId);
			return _userId;
		}
        set { _userId = value; }
    }

    protected override void OnPreRender(EventArgs e)
    {
        bSearch.Attributes["onclick"] = String.Format(
            "jsUtils.OpenWindow('{0}?editor_id={1}&label_id={2}', 800, 500);",
            BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/UserSearch.aspx")),
            tbValue.ClientID,
            lbName.ClientID
        );

        //Настраиваем внешний вид
        int id;
        if (int.TryParse(_userId.ToString(), out id) && id > 0)
        {
            BXFilter f = new BXFilter();
            if (id > 0)
            {
                f.Add(new BXFilterItem(BXUser.Fields.UserId, BXSqlFilterOperators.Equal, id));
                _userId = id;
            }
            BXUserCollection user = BXUser.GetList(f, null);

            if (user.Count == 1)
            {
                tbValue.Text = user[0].UserId.ToString();
                lbName.Text = string.Format("[{0}] ({1}) {2}", user[0].UserId.ToString(), user[0].UserName, user[0].FirstName + " " + user[0].LastName);
            }
            else
            {
                tbValue.Text = string.Empty;
                lbName.Text = string.Empty;
            }
        }


        base.OnPreRender(e);
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
            DataBindChildren();
        }
    }

}
