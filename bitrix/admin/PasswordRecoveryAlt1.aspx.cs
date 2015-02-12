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
using System.Configuration.Provider;
using Bitrix.UI;

public partial class PasswordRecoveryAlt1 : BXAdminPage
{
	CaptchaValidator _captchaValidator = null;
	protected void Page_Load(object sender, EventArgs e)
	{
		RequiredFieldValidator1.ErrorMessage = Encode(string.Format(GetMessageRaw("Message.FillField"), GetMessageRaw("Legend.Login")));
		RequiredFieldValidator2.ErrorMessage = Encode(string.Format(GetMessageRaw("Message.FillField"), GetMessageRaw("Legend.ControlString")));
		RequiredFieldValidator3.ErrorMessage = Encode(string.Format(GetMessageRaw("Message.FillField"), GetMessageRaw("Legend.NewPassword")));
		RequiredFieldValidator4.ErrorMessage = Encode(string.Format(GetMessageRaw("Message.FillField"), GetMessageRaw("Legend.PasswordConfirmation")));
		MasterTitle = Page.Title;
		if (!Page.IsPostBack)
			if (Page.Request.QueryString["checkword"] != null)
				tbCheckWord.Text = Page.Request.QueryString["checkword"];

		_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, errorMessage);
		_captchaValidator.ID = "cvCaptcha";
		captchaPlaceHolder.Controls.Add(_captchaValidator);
		_captchaValidator.ValidationGroup = "vgLoginForm";
	}

	protected void Button1_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			string login = tbLogin.Text;
			string checkWord = tbCheckWord.Text;
			string password = tbPassword.Text;
			string passwordConf = tbPasswordConf.Text;

			if (password.Equals(passwordConf, StringComparison.InvariantCultureIgnoreCase))
			{
				BXUser user = BXUserManager.GetByName(login, null, false);
				if (user != null && user.IsApproved)
				{
					try
					{
						string val1 = user.ResetPassword(null, checkWord, password);

						successMessage.Visible = true;
						successMessage.Content = GetMessage("PasswordHasBeenChanged");
					}
					catch (NotSupportedException exception)
					{
						errorMessage.AddErrorMessage(GetMessage("Error.AnErrorHasOccurredWhileChangePassword") + Encode(exception.Message));
					}
					catch (MembershipPasswordException exception)
					{
						if (exception.Message.Equals("WrongAnswer", StringComparison.InvariantCultureIgnoreCase))
							errorMessage.AddErrorMessage(GetMessage("Error.ControlStringIsIncorrect"));
						else
							errorMessage.AddErrorMessage(GetMessage("Error.PasswordIsIncorrect") + Encode(exception.Message));
					}
					catch (ProviderException exception)
					{
						errorMessage.AddErrorMessage(GetMessage("Error.ProviderError") + Encode(exception.Message));
					}
					catch (Exception exception)
					{
						errorMessage.AddErrorMessage(GetMessage("Error.GeneralError") + Encode(exception.Message));
					}
				}
				else
				{
					errorMessage.AddErrorMessage(GetMessage("Error.UserIsNotFound"));
				}
			}
			else
			{
				errorMessage.AddErrorMessage(GetMessage("Error.PasswordAndItsConfirmationDontMatch"));
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
