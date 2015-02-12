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
using Bitrix.Modules;
using System.Configuration.Provider;
using Bitrix.UI;

public partial class bitrix_admin_PasswordRecovery : BXAdminPage
{
	CaptchaValidator _captchaValidator = null;
	protected void Page_Load(object sender, EventArgs e)
	{
        MasterTitle = Page.Title;

		divPasswordRecoveryStep1.Visible = true;
		divPasswordRecoveryStep2.Visible = false;

		_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, errorMessage);
		_captchaValidator.ID = "cvCaptcha";
		_captchaValidator.ValidationGroup = "vgLoginForm";
		captchaPlaceHolder.Controls.Add(_captchaValidator);



		if (!BXUserManager.Provider.EnablePasswordReset || !BXUserManager.Provider.RequiresQuestionAndAnswer)
            errorMessage.AddErrorMessage(GetMessage("Error.PasswordRecoveryBySecretQuestionIsInaccessible"));
	}

	protected void Button1_Click(object sender, EventArgs e)
	{
			string login = tbLogin.Text;
			BXUser user = BXUserManager.GetByName(login, null, false);
			if (user != null)
			{
				if (user.IsLockedOut)
				{
					errorMessage.AddErrorMessage(GetMessage("Error.UserIsLocked"));
				}
				else
				{
					divPasswordRecoveryStep1.Visible = false;
					divPasswordRecoveryStep2.Visible = true;

					lbLogin.Text = login;
					lbQuestion.Text = user.PasswordQuestion;
				}
			}
			else
			{
				errorMessage.AddErrorMessage(GetMessage("Error.UserIsNotFound"));
			}
	}

	protected void Button3_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			string login = lbLogin.Text;
			BXUser user = BXUserManager.GetByName(login, null, false);
			if (user != null && user.IsApproved)
			{
				try
				{
					string val = user.ResetPassword(tbAnswer.Text, null);

					BXCommand c = new BXCommand("Bitrix.Main.PasswordRecovery");
					c.Parameters.Add("USER_ID", user.UserId);
					c.Parameters.Add("MESSAGE", GetMessage("YouHaveRequestedYourRegistrationData"));
					c.Parameters.Add("LOGIN", user.UserName);
					c.Parameters.Add("CHECKWORD", val);
					c.Parameters.Add("EMAIL", user.Email);
					c.Send();

					successMessage.Visible = true;
				}
				catch (NotSupportedException exception)
				{
					errorMessage.AddErrorMessage(GetMessage("Error.AnErrorHasOccurredWhilePasswordChange") + Encode(exception.Message));
				}
				catch (MembershipPasswordException exception)
				{
					if (exception.Message.Equals("WrongAnswer", StringComparison.InvariantCultureIgnoreCase))
						errorMessage.AddErrorMessage(GetMessage("Error.AnswerIsNotCorrect"));
					else
						errorMessage.AddErrorMessage(GetMessage("Error.PasswordIsNotCorrect") + Encode(exception.Message));
				}
				catch (ProviderException exception)
				{
					errorMessage.AddErrorMessage(GetMessage("Error.ProviderError") + Encode(exception.Message));
				}
				catch (Exception exception)
				{
					errorMessage.AddErrorMessage(GetMessage("Error.Error") + Encode(exception.Message));
				}
			}
			else
			{
				errorMessage.AddErrorMessage(GetMessage("Error.UserIsNotFound"));
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
