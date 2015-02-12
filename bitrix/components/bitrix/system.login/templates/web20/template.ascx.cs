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
using Bitrix.UI;
using System.Collections.Generic;
using Bitrix.DataTypes;
using System.Text;


public partial class bitrix_components_bitrix_system_login_templates_web20_template : BXComponentTemplate<LoginComponent>
{
	CaptchaValidator _captchaValidator = null;

    protected override void PrepareDesignMode()
    {
        MinimalWidth = "60";
        MinimalHeight = "30";
        //StartWidth = "150";
        //StartHeight = "30";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
		if (Component.UseCaptcha)
		{
			_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, errorMessage);
			Controls.Add(_captchaValidator);
			_captchaValidator.ValidationGroup = "vgLoginForm11";
		}
		else rfvCaptcha.ValidationGroup = "";

		string passwordRecoveryPath = Component.PasswordRecoveryPath;
		if (!String.IsNullOrEmpty(passwordRecoveryPath))
			hlForgotPassword.NavigateUrl = passwordRecoveryPath;
		else
			hlForgotPassword.Visible = false;

		string registerPath = Component.RegisterPath;
		if (!String.IsNullOrEmpty(registerPath))
		{
			hlRegister.NavigateUrl = registerPath;
			hlRegister1.NavigateUrl = registerPath;
		}
		else
		{
			hlRegister.Visible = false;
			hlRegister1.Visible = false;
		}

		imgLogin.ImageUrl = VirtualPathUtility.ToAbsolute(String.Format("{0}/images/login.gif", this.Component.TemplateFolder));
		imgRegister.ImageUrl = VirtualPathUtility.ToAbsolute(String.Format("{0}/images/register.gif", this.Component.TemplateFolder));

		ibLogout.ImageUrl = VirtualPathUtility.ToAbsolute(String.Format("{0}/images/login.gif", this.Component.TemplateFolder));

		formTypeNonLogin.Visible = HttpContext.Current.User.Identity.IsAuthenticated;
		formTypeLogin.Visible = !HttpContext.Current.User.Identity.IsAuthenticated;


    }

	protected void LoginButton_Click(object sender, EventArgs e)
	{
		StringBuilder s = new StringBuilder();
		if (Page.IsValid)
		{
			BXParamsBag<object> par = new BXParamsBag<object>();
			par.Add("Login", LoginField.Text);
			par.Add("Password", PasswordField.Text);
			par.Add("Remember", CheckBoxRemember.Checked);

			List<string> err = new List<string>();

			if (!Component.ProcessCommand("login", par, err))
			{
				foreach (string kvp in err){
					errorMessage.AddErrorMessage(kvp);
					s.Append(kvp).Append(JSEncode("\n"));
				}
			}
		}
		else
		{	
			if (!_captchaValidator.IsValid)
				s.Append(GetMessage("CaptchaPrompt.CaptchaCodeIsInvalid"));
		}
		if ( s.Length > 0 )
			Page.RegisterStartupScript(errorMessage.ClientID + "openscript",
				String.Format("<script>window.setTimeout('ShowLoginForm(\"{0}\")',0);</script>",JSEncode(s.ToString())));
	}

	protected void ibLogout_Click(object sender, ImageClickEventArgs e)
	{
		List<string> err = new List<string>();
		Component.ProcessCommand("logout", new BXParamsBag<object>(), err);
	}

	protected void OpenIdLoginButton_Click(object sender, EventArgs e)
	{
		if (!Page.IsValid) return;
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
}
