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
using System.Linq;

public partial class DaysOfWeek : BXControl
{
    private int[] _days;
    public int[] Days
    {
        get { return _days ?? LoadPost(); }
        set { _days = value;}
    }

    bool postLoaded;
    private int[] LoadPost()
    {
        if (postLoaded || !IsPostBack)
            return null;

        postLoaded = true;
        
        var values = Request.Form.GetValues(UniqueID + "$Days");
        if (values == null)
            return _days = new int[0];

       return _days = values.Select(x => int.Parse(x)).ToArray();
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
    }
}

enum DaysOfWeekEnum
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7
}
