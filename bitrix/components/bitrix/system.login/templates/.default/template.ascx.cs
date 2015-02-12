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
using Bitrix.Components;
using System.Collections.Generic;
using Bitrix.Security;
using Bitrix.UI;
using Bitrix.DataTypes;


public partial class bitrix_components_bitrix_system_login_templates__default_template : BXComponentTemplate<LoginComponent>
{
	CaptchaValidator _captchaValidator = null;

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		if (Component.UseCaptcha)
		{
			BXCaptchaEngine captchaEng = _captchaValidator.CaptchaEngine;
			tbxCaptcha.MaxLength = captchaEng.Captcha.Length;
			tbxCaptcha.Attributes.Add("length", (2 * captchaEng.Captcha.Length).ToString());
			imgCaptcha.ImageUrl = _captchaValidator.GetCaptchaImageUrl();
		}

	}

    protected void Page_Load(object sender, EventArgs e)
    {
		if (Component.Errors.Count > 0)
			foreach (var er in Component.Errors)
				errorMessage.AddErrorMessage(er);

		if (Component.UseCaptcha)
		{
			_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, errorMessage);
			CaptchaValidatorPlaceHolder.Controls.Add(_captchaValidator);
			_captchaValidator.ValidationGroup = "vgLoginForm";
		}
		else rfvCaptcha.ValidationGroup = "";

		if ((HttpContext.Current.User.Identity as BXIdentity).IsAuthenticated)
		{
			AuthComponentSelector.Visible = false;
			AuthComponentSelectorSuccess.Visible = true;

			lCurUserName.Text = HttpContext.Current.User.Identity.Name;
		}
		else
		{

			AuthComponentSelector.Visible = true;
			AuthComponentSelectorSuccess.Visible = false;

			divLoginFooterHint.Visible = Component.EnablePasswordReset || Component.ShowRegistrationLink;

			divLoginFooterHint.InnerHtml = "";

			if (Component.ShowRegistrationLink)
				divLoginFooterHint.InnerHtml = String.Format("<p>{0}</p><p>{1}</p>", string.Format("<a href=\"{0}\">{1}</a>", Encode(Component.RegisterPath), GetMessageRaw("Register")), 
					string.Format(GetMessageRaw("Format.Go2RegisterForm"), string.Format("<a href=\"{0}\">{1}</a>", Encode(Component.RegisterPath), GetMessageRaw("RegisterForm"))));

			if (Component.RequiresQuestionAndAnswer)
                divLoginFooterHint.InnerHtml += String.Format("<p><b>{0}</b></p><p>{1}</p>", GetMessageRaw("DoYouForgetYourRassword"), string.Format(GetMessageRaw("Format.Go2PasswordChangeForm"), string.Format("<a href=\"{0}\">", Encode(Parameters["PasswordRecoveryPath"])), "</a>"));
			if (Component.RequiresCheckWord)
                divLoginFooterHint.InnerHtml += String.Format("<p><b>{0}</b></p><p>{1}</p><p>{2}</p>", GetMessageRaw("DoYouForgetYourRassword"), string.Format(GetMessageRaw("Format.Go2PasswordChangeForm"), string.Format("<a href=\"{0}\">", Encode(Parameters["PasswordRecoveryPath"])), "</a>"), string.Format(GetMessageRaw("Format.AfterYouGetControlStringGoToPasswordChangeForm"), string.Format("<a href=\"{0}\">", Parameters["PasswordRecoveryCodePath"]), "</a>"));
		}
	}


	protected void OpenIdLoginButton_Click(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;

		List<string> err = new List<string>();
		BXParamsBag<object> par = new BXParamsBag<object>();
		par.Add("OpenIdLogin", OpenIdLoginField.Text);
		if (!Component.ProcessCommand("openIdlogin", par, err))
		{
			foreach (string kvp in err)
				errorMessage.AddErrorMessage(kvp);

		}
	}

	protected void LiveIdLoginButton_Click(object sender, EventArgs e)
	{
		List<string> err = new List<string>();
		BXParamsBag<object> par = new BXParamsBag<object>();
		if (!Component.ProcessCommand("LiveIdlogin", par, err))
		{
		    foreach (string kvp in err)
		        errorMessage.AddErrorMessage(kvp);
		}
	}

	protected void LoginButton_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			BXParamsBag<object> par = new BXParamsBag<object>();
			par.Add("Login", LoginField.Text);
			par.Add("Password", PasswordField.Text);
			par.Add("Remember", CheckBoxRemember.Checked);

			List<string> err = new List<string>();

			if (!Component.ProcessCommand("login", par, err))
			{
				foreach (string kvp in err)
					errorMessage.AddErrorMessage(kvp);
			}
		}

	}

	protected void LinkButton1_Click(object sender, EventArgs e)
	{
		List<string> err = new List<string>();
		Component.ProcessCommand("logout", new BXParamsBag<object>(), err);
	}
}
