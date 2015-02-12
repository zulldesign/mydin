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

namespace Bitrix.Blog
{
	public partial class CommentForm : BXControl
	{
		private bool requireCaptcha;
		private BXCaptchaEngine captcha;
		private bool captchaPrepared;
		private string captchaHref;
		private string captchaGuid;
		private FieldMode requireUserName;
		private FieldMode requireUserEmail;
		private bool hidden;
		private string cssClass = "";

		public bool RequireCaptcha
		{
			get
			{
				return requireCaptcha;
			}
			set
			{
				requireCaptcha = value;
			}
		}
		public FieldMode RequireUserName
		{
			get
			{
				return requireUserName;
			}
			set
			{
				requireUserName = value;
			}
		}
		public FieldMode RequireUserEmail
		{
			get
			{
				return requireUserEmail;
			}
			set
			{
				requireUserEmail = value;
			}
		}
		public bool Hidden
		{
			get
			{
				return hidden;
			}
			set
			{
				hidden = value;
			}
		}
		public string UserName
		{
			get
			{
				return Name.Text;
			}
			set
			{
				Name.Text = value;
			}
		}
		public string UserEmail
		{
			get
			{
				return Email.Text;
			}
			set
			{
				Email.Text = value;
			}
		}
		public string Text
		{
			get
			{
				return Content.Text;
			}
			set
			{
				Content.Text = value;
			}
		}
		public int ParentCommentId
		{
			get
			{
				int id;
				return int.TryParse(ParentId.Value, out id) ? id : 0;
			}
			set
			{
				ParentId.Value = value.ToString();
			}
		}
		public bool Enabled
		{
			get
			{
				return Errors.Enabled;
			}
			set
			{
				Errors.Enabled = value;
				NameRequired.Enabled = value && RequireUserName == FieldMode.Require;
				EmailRequired.Enabled = EmailValid.Enabled = value && RequireUserEmail == FieldMode.Require;
				ContentRequired.Enabled = value;
				CaptchaRequired.Enabled = value && RequireCaptcha;
			}
		}


		protected string CaptchaGuid
		{
			get
			{
				PrepareCaptcha();
				return captchaGuid;
			}
		}
		protected string CaptchaHref
		{
			get
			{
				PrepareCaptcha();
				return captchaHref;
			}
		}
		
		public string CssClass { get { return cssClass; } set { cssClass = value; } }

		public event EventHandler<SubmitEventArgs> Submit;
		public event EventHandler Preview;

		public string GetAllocateScript(string targetNodeScript, string parentCommentScript, bool visible)
		{
			return string.Format(@"{0}_Allocate({1}, {2}, {3});", ClientID, targetNodeScript, parentCommentScript, visible.ToString().ToLowerInvariant());
		}
		public string GetFocusScript()
		{
			return string.Format(@"{0}_Focus();", ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Errors.ValidationGroup = ClientID;
			SubmitButton.ValidationGroup = ClientID;
			NameRequired.ValidationGroup = ClientID;
			EmailRequired.ValidationGroup = EmailValid.ValidationGroup = ClientID;
			ContentRequired.ValidationGroup = ClientID;
			CaptchaRequired.ValidationGroup = ClientID;

			//To initialize validator state
			Enabled = Enabled;
		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			string validateScript = String.Format(@"
				if (typeof(ValidatorOnSubmit) == ""function"")
				{{
					var isValidated = ValidatorOnSubmit();
					if (!isValidated)
					{{
						window.location=""#{0}"";													
						return false;
					}}
					return true;
				}}", ID);

			Page.ClientScript.RegisterOnSubmitStatement(Page.GetType(), "NewValidate", validateScript);

		}
		protected void Submit_Click(object sender, EventArgs e)
		{
			bool success = Page.IsValid;

			if (success && RequireCaptcha)
			{
				captcha = BXCaptchaEngine.Get(Guid.Value);
				string error = captcha.Validate(CaptchaTextBox.Text);
				if (error != null)
				{
					Errors.AddErrorText(error);
					success = false;
				}
			}

			if (Submit != null)
				Submit(this, new SubmitEventArgs(success));

		}
		protected void Preview_Click(object sender, EventArgs e)
		{
			if (Preview != null)
				Preview(this, EventArgs.Empty);
		}

		private void PrepareCaptcha()
		{
			if (captchaPrepared)
				return;

			captcha = captcha ?? BXCaptchaEngine.Create();
			captcha.MaxTimeout = 1800;

			captchaHref = Encode(captcha.Store());
			captchaGuid = captcha.Id;

			captchaPrepared = true;
		}

		public enum FieldMode
		{
			Hide,
			Show,
			Require
		}
		public class SubmitEventArgs : EventArgs
		{
			bool success;

			public bool Success
			{
				get
				{
					return success;
				}
			}

			internal SubmitEventArgs(bool success)
			{
				this.success = success;
			}
		}
	}
}