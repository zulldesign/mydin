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
using Bitrix.UI;
using Bitrix.DataTypes;
using System.Threading;
using Bitrix.Services.Text;
using System.Text;
using Bitrix.Security;


public partial class bitrix_components_bitrix_system_register_templates__default_template : BXComponentTemplate<RegisterComponent>
{
    CaptchaValidator _captchaValidator = null;
    private bool _isValid = false;
    protected void Page_Load(object sender, EventArgs e)
    {
		Page.Title = GetMessageRaw("PageTitle");

		//RegisterButton.Enabled = false;
		if (Component.UseCaptcha)
		{
			_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, errorMessage);
			captchaPlaceholder.Controls.Add(_captchaValidator);
			_captchaValidator.ValidationGroup = "CreateUserWizard1";
		}
		else
			rfvCaptcha.ValidationGroup = "";
        //CreateUserStep1.Visible = true;
        //CreateUserStep2.Visible = false;

		rfvDisplayName.Enabled = (Component.DisplayNameFieldMode == RegisterComponent.FieldMode.Require);
		rfvFirstName.Enabled = (Component.FirstNameFieldMode == RegisterComponent.FieldMode.Require);
		rfvLastName.Enabled = (Component.LastNameFieldMode == RegisterComponent.FieldMode.Require);

		// если пришла информация о внешнем пользователе

		//openID
		if (HaveExternalUserInfo)
		{
			if (Component.OpenIdAuthFields.ContainsKey("login"))
				tbLogin.Text = Component.OpenIdAuthFields["login"];
			if (Component.OpenIdAuthFields.ContainsKey("email"))
				tbEmail.Text = Component.OpenIdAuthFields["email"];
			if (Component.OpenIdAuthFields.ContainsKey("nickname"))
				tbDisplayName.Text = Component.OpenIdAuthFields["nickname"];
			extFields.Visible = true;
			dPassword.Visible = false;
			dPasswordConf.Visible = false;
			try
			{
				var pwd = BXUser.GetCorrectPassword(BXUserManager.GetProvider("self") as BXSqlMembershipProvider);
				if (pwd != null)
				{
					dPassword.Visible = false;
					dPasswordConf.Visible = false;
				}
				else
				{
					dPassword.Visible = true;
					dPasswordConf.Visible = true;
				}
			}
			catch (Exception ex)
			{

				dPassword.Visible = true;
				dPasswordConf.Visible = true;
			}
		}
	}

	protected void ExternalIdLinkClick(object sender, EventArgs e)
	{
		if (!Page.IsValid) return;
		List<string> err = new List<string>();
		BXParamsBag<object> par = new BXParamsBag<object>();

		par.Add("login", tbOpenIdLogin.Text);
		par.Add("password", tbOpenIdPassword.Text);

		string commandName =
			(Component.UseOpenIdAuth && Component.OpenIdAuthFields.ContainsKey("login")) ? "linkToOpenId" : "linkToLiveId";

		if (!Component.ProcessCommand(commandName, par, err))
		{
			foreach (string kvp in err)
				errorMessage.AddErrorMessage(kvp);
			_isValid = false;
		}
	}

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (Component.UseCaptcha && (!Page.IsPostBack || !_isValid))
        {
            BXCaptchaEngine captchaEng = _captchaValidator.CaptchaEngine;
            tbxCaptcha.MaxLength = captchaEng.Captcha.Length;
            tbxCaptcha.Attributes.Add("length", (2 * captchaEng.Captcha.Length).ToString());
            imgCaptcha.ImageUrl = _captchaValidator.GetCaptchaImageUrl();
        }
    }

	protected bool PageIsValid
	{
		get
		{
			return _isValid;
		}
	}

	protected bool HaveExternalUserInfo
	{
		get
		{
			return (Component.UseOpenIdAuth && Component.OpenIdAuthFields.ContainsKey("login")) ||
				(Component.UseLiveIdAuth && !BXStringUtility.IsNullOrTrimEmpty(Component.LiveIdAuthToken));
		}
	}

	protected void OnUserRegister(object sender, EventArgs e)
	{
		//return;
        _isValid = Page.IsValid;
        if (_isValid)
        {
			//_registredFirstName = tbFirstName.Text;
			//_registredLastName = tbLastName.Text;
			//_registredLogin = tbLogin.Text;
			//_registredEmail = tbEmail.Text;

			BXParamsBag<object> par = new BXParamsBag<object>();

			string pwd = string.Empty;
			if (!tbPassword.Visible)
			{
				try
				{
					pwd = BXUser.GetCorrectPassword(BXUserManager.GetProvider("self") as BXSqlMembershipProvider);
				}
				catch (Exception ex)
				{
					errorMessage.AddErrorMessage(GetMessage(""));
					return;
				}
			}

			par.Add("Password", dPassword.Visible ? tbPassword.Text : pwd);

            
			par.Add("DisplayName", Component.DisplayNameFieldMode != RegisterComponent.FieldMode.Hide ? tbDisplayName.Text : null);
			par.Add("FirstName", Component.FirstNameFieldMode != RegisterComponent.FieldMode.Hide ? tbFirstName.Text : null);
			par.Add("LastName", Component.LastNameFieldMode != RegisterComponent.FieldMode.Hide ? tbLastName.Text : null);
			par.Add("Login", tbLogin.Text);
			par.Add("Email", tbEmail.Text);
			par.Add("Question", String.Empty);
			par.Add("Answer", String.Empty);

            List<string> err = new List<string>();

            if (!Component.ProcessCommand("register", par, err))
            {
                foreach (string kvp in err)
                    errorMessage.AddErrorMessage(kvp);
                _isValid = false;
            }
            else if (!(Component.SendConfirmationRequest && !String.IsNullOrEmpty(Component.UserActivationToken)))
			{
					//CreateUserStep1.Visible = false;
					//CreateUserStep2.Visible = true;
					string redirectUrl = Component.GetRedirectUrl();
					if (string.IsNullOrEmpty(redirectUrl))
						redirectUrl = "~/";
					try
					{
						Response.Redirect(redirectUrl);
					}
					catch (ThreadAbortException /*exc*/) { }
			}
            
        }
	}

    protected override void PrepareDesignMode()
    {
        MinimalWidth = "530";
        MinimalHeight = "410";
    }
}
