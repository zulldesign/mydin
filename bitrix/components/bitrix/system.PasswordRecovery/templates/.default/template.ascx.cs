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

using System.Collections.Generic;
using Bitrix.DataTypes;
using Bitrix.Main.Components;

public partial class bitrix_components_bitrix_system_PasswordRecovery_templates__default_template : BXComponentTemplate<SystemPasswordRecoveryComponent>
{
	CaptchaValidator _captchaValidator = null;

    protected void Page_Load(object sender, EventArgs e)
    {

		if (Component.UseCaptcha)
		{
			_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, errorMessage);
			Controls.Add(_captchaValidator);
			_captchaValidator.ValidationGroup = "vgPasswordRecoveryFrom";
		}
		else rfvCaptcha.ValidationGroup = "";

		if (Component.FeatureEnabled)
		{
			divPasswordRecoveryStep1.Visible = true;
			divPasswordRecoveryStep2.Visible = false;
			divPasswordRecoveryError.Visible = false;
		}
		else
		{
			divPasswordRecoveryStep1.Visible = false;
			divPasswordRecoveryStep2.Visible = false;
			divPasswordRecoveryError.Visible = true;
		}

    }

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		if (Component.UseCaptcha && divPasswordRecoveryStep2.Visible)
		{
			BXCaptchaEngine captchaEng = _captchaValidator.CaptchaEngine;
			tbxCaptcha.MaxLength = captchaEng.Captcha.Length;
			tbxCaptcha.Attributes.Add("length", (2 * captchaEng.Captcha.Length).ToString());
			imgCaptcha.ImageUrl = _captchaValidator.GetCaptchaImageUrl();
		}

	}

	protected void Button1_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			BXParamsBag<object> par = new BXParamsBag<object>();
			par.Add("Login", tbLogin.Text);

			List<string> err = new List<string>();

			if (!Component.ProcessCommand("validate", par, err))
			{
				foreach (string kvp in err)
					errorMessage.AddErrorMessage(kvp);
			}
			else
			{
				divPasswordRecoveryStep1.Visible = false;
				divPasswordRecoveryStep2.Visible = true;

				lbLogin.Text = tbLogin.Text;
				lbQuestion.Text = (string)par["PasswordQuestion"];
			}
		}
	}

	protected void Button3_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			BXParamsBag<object> par = new BXParamsBag<object>();
			par.Add("Login", lbLogin.Text);
			par.Add("PasswordAnswer", tbAnswer.Text);

			List<string> err = new List<string>();

			if (!Component.ProcessCommand("recovery", par, err))
			{
				foreach (string kvp in err)
					errorMessage.AddErrorMessage(kvp);
			}
			else
			{
				successMessage.Visible = true;
			}
		}
	}
}
