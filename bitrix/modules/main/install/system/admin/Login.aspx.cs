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
using System.Threading;

public partial class Login1 : BXAdminPage
{
	CaptchaValidator _captchaValidator = null;

	protected void Page_Load(object sender, EventArgs e)
	{
		RequiredFieldValidator3.ErrorMessage = Encode(string.Format(GetMessageRaw("Message.FillField"), GetMessageRaw("LegendLogin")));
		RequiredFieldValidator4.ErrorMessage = Encode(string.Format(GetMessageRaw("Message.FillField"), GetMessageRaw("LegendPassword")));
		MasterTitle = Page.Title;

		divLoginFooterHint.Visible = BXUserManager.Provider.EnablePasswordReset;

		_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, errorMessage);
		_captchaValidator.ID = "cvCaptcha";
		captchaPlaceHolder.Controls.Add(_captchaValidator);
		_captchaValidator.ValidationGroup = "vgLoginForm";
        #region old
        //if (BXUserManager.Provider.RequiresQuestionAndAnswer)
        //    divLoginFooterHint.InnerHtml = string.Format(
        //        "<p><b>{0}</b></p><p>{1}</p>",
        //        GetMessage("DoYouForgetPassword"),
        //        string.Format(GetMessage("FormatGo2PasswordRecoveryForm"), "<a href=\"PasswordRecovery.aspx\">", "</a>")
        //        );
        //if (BXUserManager.Provider.RequiresCheckWord)
        //    divLoginFooterHint.InnerHtml = string.Format(
        //        "<p><b>{0}</b></p><p>{1}</p><p>{2}</p>",
        //        GetMessage("DoYouForgetPassword"),
        //        string.Format(GetMessage("FormatGo2PasswordRecoveryForm"), "<a href=\"PasswordRecoveryAlt.aspx\">", "</a>"),
        //        string.Format(GetMessage("FormatGo2PasswordChangeForm"), "<a href=\"PasswordRecoveryAlt1.aspx\">", "</a>")
        //        );
        #endregion
    }

    protected string GetFotterHintHtml()
    {
        if (BXUserManager.Provider.RequiresQuestionAndAnswer)
            return string.Format(
                "<p><b>{0}</b></p><p>{1}</p>",
                GetMessage("DoYouForgetPassword"),
                string.Format(GetMessage("FormatGo2PasswordRecoveryForm"), "<a href=\"PasswordRecovery.aspx\">", "</a>")
                );
        if (BXUserManager.Provider.RequiresCheckWord)
            return string.Format(
                "<p><b>{0}</b></p><p>{1}</p><p>{2}</p>",
                GetMessage("DoYouForgetPassword"),
                string.Format(GetMessage("FormatGo2PasswordRecoveryForm"), "<a href=\"PasswordRecoveryAlt.aspx\">", "</a>"),
                string.Format(GetMessage("FormatGo2PasswordChangeForm"), "<a href=\"PasswordRecoveryAlt1.aspx\">", "</a>")
                );

        return string.Empty;
    }

	protected void LoginButton_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			string providerName = null;

			string login = LoginField.Text;
			string val1 = PasswordField.Text;
			bool remember = CheckBoxRemember.Checked;

			try
			{
				if (BXAuthentication.Authenticate(login, val1, out providerName))
				{
					BXAuthentication.SetAuthCookie(login, providerName, remember);
					//FormsAuthentication.RedirectFromLoginPage(login, false);
					Response.Redirect(Request.QueryString["ReturnUrl"] == null ? VirtualPathUtility.ToAbsolute("~/bitrix/admin/") : FormsAuthentication.GetRedirectUrl(login, false));
				}
				else
				{
					errorMessage.AddErrorMessage(GetMessage("Error.LoginOrPasswordIsIncorrect"));
				}
			}
			catch (ThreadAbortException)
			{ }
			catch (Exception ex)
			{
				errorMessage.AddErrorMessage(ex.Message);
			}
		}
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		BXCaptchaEngine captchaEng = _captchaValidator.CaptchaEngine;
		tbxCaptcha.MaxLength = captchaEng.Captcha.Length;
		tbxCaptcha.Attributes.Add("length", (2 * captchaEng.Captcha.Length).ToString());
		imgCaptcha.ImageUrl = _captchaValidator.GetCaptchaImageUrl();
	}
}
