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
using Bitrix.Security;


public partial class bitrix_dialogs_Login : Bitrix.UI.BXPage
{
    private string _errorText = null;
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        //behaviour.ButtonClick += new EventHandler<BXPageAsDialogButtonClickEventArgs>(behaviour_ButtonClick);
        behaviour.SetButtonText(BXPageAsDialogButtonEntry.Continue, GetMessageRaw("ButtonText.RequestOfAuthorization"));

        behaviour.Settings.MinWidth = 340;
        behaviour.Settings.MinHeight = 120;
        behaviour.Settings.Width = 340;
        behaviour.Settings.Height = 120;
        behaviour.Settings.Resizeable = false;  
    }

    protected void behaviour_Continue(object sender, EventArgs e)
    {
        try
        {
            string providerName;
            if (BXAuthentication.Authenticate(login.Login, login.Password, out providerName))
            {
                BXAuthentication.SetAuthCookie(login.Login, providerName, false);
                string returnUrl = FormsAuthentication.GetRedirectUrl(login.Login, false);
                Response.StatusCode = 200;
                Response.ContentType = "text/html";

				Response.Write(BXPageAsDialogHelper.GetClientSwitchDialogDocumentStub(returnUrl, new BXDialogGoodbyeWindow(GetMessageRaw("Message.UserHasBeenSuccessfullyAuthorized"), 800, BXDialogGoodbyeWindow.LayoutType.Success), null, behaviour.ClientType));
                Response.End(); return;
            }
            else 
            {
                _errorText = GetMessageRaw("Message.InvalidUserNameOrPassword");
            }
        }
        catch (System.Threading.ThreadAbortException /*exception*/)
        {
            //...игнорируем
        }
        catch (Exception ex)
        {
            Response.StatusCode = 200;
            Response.ContentType = "text/html";
            BXDialogGoodbyeWindow goodbye = new BXDialogGoodbyeWindow(HttpUtility.HtmlEncode(ex.Message), -1, BXDialogGoodbyeWindow.LayoutType.Error);
			goodbye.ClientType = behaviour.ClientType;
            string docStub = IsPostBack ?
				BXPageAsDialogHelper.GetClientCloseDialogDocumentStub(goodbye, behaviour.ClientType) :
				BXPageAsDialogHelper.GetClientSuppressShowDocumentStub(goodbye, behaviour.ClientType);
            Response.Write(docStub);
            Response.End();
        }
    }

    protected void behaviour_OnPopulateData(BXPageAsDialogBaseAdapter sender, BXPageAsDialogBaseAdapter.PopulateDataEventArgs args)
    {
        BXDialogSectionData title = args.Data.CreateSection(BXDialogSectionType.Title);
        title.CreateItemFromString(GetMessageRaw("DialogTitle.Authorization"));

        if (!string.IsNullOrEmpty(_errorText))
        {
            BXDialogSectionData description = args.Data.CreateSectionIfNeed(BXDialogSectionType.Description);

            description.CreateItemFromControl(new BXDialogStandardDescription(
                string.Empty, new string[]{string.Format("<font class=\"errortext\">{0}</font>", Encode(_errorText))}
            ));
        }

        BXDialogSectionData content = args.Data.CreateSectionIfNeed(BXDialogSectionType.Content);
        content.CreateItemFromControl(this);
    }

	protected override void Render(System.Web.UI.HtmlTextWriter writer)
	{
		if (!behaviour.IsRenderingStarted)
			behaviour.RenderControl(writer);
		else
			base.Render(writer);
	}
}
