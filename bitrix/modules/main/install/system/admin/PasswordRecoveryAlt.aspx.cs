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
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.UI;

public partial class PasswordRecoveryAlt : BXAdminPage
{
	CaptchaValidator _captchaValidator = null;


	protected void Page_Load(object sender, EventArgs e)
	{
		MasterTitle = Page.Title;

		_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, errorMessage);
		_captchaValidator.ID = "cvCaptcha";
		captchaPlaceHolder.Controls.Add(_captchaValidator);
		_captchaValidator.ValidationGroup = "vgLoginForm";

		if (!BXUserManager.Provider.EnablePasswordReset || !BXUserManager.Provider.RequiresCheckWord)
			errorMessage.AddErrorMessage(GetMessage("Error.PasswordRecoveryByControlStringIsInaccessible"));
	}

	protected void Button1_Click(object sender, EventArgs e)
	{
		if (Page.IsValid )
		{
		string login = tbLogin.Text;
		string email = tbEMail.Text;
		BXUser user;

		if (String.IsNullOrEmpty(login) && String.IsNullOrEmpty(email))
			errorMessage.AddErrorMessage(GetMessage("Error.EmailIsRequierd"));
		else
		{
			if (String.IsNullOrEmpty(login))
			{
				BXUserCollection userCollection = BXUserManager.GetUserNameByEmail(email);
				if (userCollection.Count == 1)
					login = userCollection[0].UserName;
			}

			if (!String.IsNullOrEmpty(login))
			{
				user = BXUserManager.GetByName(login, null, false);
				if (user != null && user.IsApproved)
				{
					string newCheckWord = user.ChangeCheckWord();

					BXCommand c = new BXCommand("Bitrix.Main.PasswordRecovery");
					var site = Bitrix.BXSite.GetById(user.SiteId) ?? Bitrix.BXSite.DefaultSite;
					c.Parameters.Add("@site", site.TextEncoder.Decode(site.Id));
					c.Parameters.Add("SERVER_NAME", Bitrix.Services.BXSefUrlManager.CurrentUrl.Host);
					c.Parameters.Add("NAME", user.FirstName);
					c.Parameters.Add("LAST_NAME", user.LastName);
					c.Parameters.Add("STATUS", string.Empty);
					c.Parameters.Add("USER_ID", user.UserId);
					c.Parameters.Add("MESSAGE", GetMessage("YouHaveRequestedYourRegistrationData"));
					c.Parameters.Add("LOGIN", user.UserName);
					c.Parameters.Add("CHECKWORD", Server.UrlEncode(newCheckWord));
					c.Parameters.Add("EMAIL", user.Email);
					c.Parameters.Add("LINK", VirtualPathUtility.ToAbsolute("~/bitrix/admin/PasswordRecoveryAlt1.aspx") + "?checkword=" + Server.UrlEncode(newCheckWord));
					c.Parameters.Add(
						"USER_NAME",
						(user.FirstName == null ? "" : user.FirstName) 
						+ ((!String.IsNullOrEmpty(user.FirstName) && !String.IsNullOrEmpty(user.LastName)) ? " " : "") 
						+ (user.LastName == null ? "" : user.LastName)
						+ ((!String.IsNullOrEmpty(user.FirstName) || !String.IsNullOrEmpty(user.LastName)) ? "," : "")
					);
					c.Send();

					loginMessage.Visible = true;
				}
				else
				{
					errorMessage.AddErrorMessage(GetMessage("Error.UserIsNotfound"));
				}
			}
			else
			{
				errorMessage.AddErrorMessage(GetMessage("Error.EmailIsNotFound"));
			}
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
