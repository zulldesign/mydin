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
using Bitrix.Services;
using Bitrix;


public partial class bitrix_components_bitrix_system_login_templates__default_template : BXComponentTemplate<LoginComponent>
{
	CaptchaValidator _captchaValidator = null;
	protected override void  OnLoad(EventArgs e)
{
 	 base.OnLoad(e);

		Page.Title = GetMessageRaw("PageTitle");

		//BXIdentity identity = BXPrincipal.Current.GetIdentity();
		//if (identity.IsAuthenticated)
			//Response.Redirect(BXSite.Current.DirectoryAbsolutePath);

		if (Component.UseCaptcha)
		{
			_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, ErrorMessage);
			captchaPlaceholder.Controls.Add(_captchaValidator);
			_captchaValidator.ValidationGroup = ClientID;
			rfvCaptcha.ValidationGroup = ClientID;
		}
		else rfvCaptcha.ValidationGroup = "";

	}

	protected void LiveIdLoginButton_Click(object sender, EventArgs e)
	{
		List<string> err = new List<string>();
		BXParamsBag<object> par = new BXParamsBag<object>();
		if (!Component.ProcessCommand("LiveIdlogin", par, err))
		{
			foreach (string kvp in err)
				ErrorMessage.AddErrorMessage(kvp);
		}
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
				ErrorMessage.AddErrorMessage(kvp);
		}
	}

	protected void LoginButton_Click(object sender, EventArgs e)
	{
		//return;
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
					ErrorMessage.AddErrorMessage(kvp);
			}
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
