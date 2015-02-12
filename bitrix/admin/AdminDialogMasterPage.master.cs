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

public partial class bitrix_admin_AdminDialogMasterPage : BXAdminMasterPage
{
    protected override void OnPreRender(EventArgs e)
    {
        BXAdminPage page = Page as BXAdminPage;
        if (page == null)
            throw new InvalidOperationException("Page must inherit from BXAdminPage!");
        //---
        //page.LinkToStylesheetCollectionRender.Add(VirtualPathUtility.ToAbsolute(Bitrix.UI.BXThemeHelper.SimpleCombineWithCurrentThemePath("pubstyles.css")));
        //page.LinkToStylesheetCollectionRender.Add(VirtualPathUtility.ToAbsolute(Bitrix.UI.BXThemeHelper.SimpleCombineWithCurrentThemePath("stylesheet.css")));
        //page.LinkToStylesheetCollectionRender.Add(VirtualPathUtility.ToAbsolute(Bitrix.UI.BXThemeHelper.SimpleCombineWithCurrentThemePath("adminstyles.css")));
        //page.LinkToStylesheetCollectionRender.Add(VirtualPathUtility.ToAbsolute(Bitrix.UI.BXThemeHelper.SimpleCombineWithCurrentThemePath("start_menu.css")));

        base.OnPreRender(e);
    }
}
