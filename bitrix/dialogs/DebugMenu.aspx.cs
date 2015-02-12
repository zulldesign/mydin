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
using Bitrix.UI;
using Bitrix.Security;

public enum BXDebugMenuMode
{
    Standard = 1,
    SiteMap
}

public partial class bitrix_dialogs_DebugMenu : Bitrix.UI.BXDialogPage
{
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        try
        {
            BXDebugMenuMode mode = string.Equals(Request.QueryString["debugMenuMode"], "SiteMap", StringComparison.InvariantCultureIgnoreCase) ? BXDebugMenuMode.SiteMap : BXDebugMenuMode.Standard;

            BXUser.DemandOperations(BXRoleOperation.Operations.SystemMaintenance);

            //DescriptionIconClass = "bx-delete-page";
            DescriptionParagraphs.Add(mode == BXDebugMenuMode.SiteMap ? GetMessage("Description.SiteMap") : GetMessage("Description.Standard"));

            Behaviour.ButonSetLayout = BXPageAsDialogButtonSetLayout.Continue;
            string continueText = GetMessageRaw("ButtonText.Continue");
            if (string.IsNullOrEmpty(continueText))
                continueText = "[Continue]";
            Behaviour.SetButtonText(BXPageAsDialogButtonEntry.Continue, continueText);
            Behaviour.Settings.MinWidth = 300;
            Behaviour.Settings.MinHeight = 150;
            Behaviour.Settings.Width = 600;
            Behaviour.Settings.Height = 300;
            Behaviour.Settings.Resizeable = true;

            string content = BXPublicMenu.Menu.Dump(mode == BXDebugMenuMode.SiteMap);
            debugMenuContent.InnerText = content;
        }
        catch (System.Threading.ThreadAbortException /*exception*/)
        {
            //...игнорируем, вызвано Close();
        }
        catch (Exception exception)
        {
            Close(exception.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
        }
    }
    //protected void Behaviour_Continue(object sender, EventArgs e) {}
}
