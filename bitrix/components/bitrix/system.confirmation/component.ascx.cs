using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Collections.Generic;
using System.Configuration.Provider;
using Bitrix.Modules;
using Bitrix.UI;

using Bitrix.Services;
using Bitrix;
using Bitrix.DataTypes;
using Bitrix.Configuration;
using System.Threading;
using Bitrix.Components;
using Bitrix.Security;
using System.Web.Hosting;
using System.IO;

namespace Bitrix.Main.Components
{

	public partial class ConfirmationComponent : BXComponent
	{
		ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		Exception fatalException;
		bool templateIncluded;
		bool userIsAuthorized;

		public bool UserIsAuthorized
		{
			get
			{
				return userIsAuthorized;
			}
			set
			{
				userIsAuthorized = value;
			}
		}

		public int UserId
		{
			get
			{
				return Parameters.GetInt("UserId", 0);
			}
		}

		public string ActivationToken
		{
			get
			{
				return Parameters.GetString("ActivationToken", String.Empty);
			}
		}

		public bool RegistrationDoAuthentication
		{
			get
			{
				return Parameters.GetBool("RegistrationDoAuthentication", false);
			}
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = new BXComponentGroup("Auth", GetMessage("Group"), 100, BXComponentGroup.Utility);

			BXCategory mainCategory = BXCategory.Main;

			ParamsDefinition.Add(
				"UserId",
				new BXParamText(
				GetMessageRaw("Param.UserId"),
				"<%$ Request:UserId %>",
				mainCategory
				)
			);

			ParamsDefinition.Add(
				"ActivationToken",
				new BXParamText(
					GetMessageRaw("Param.ActivationToken"),
					"<%$ Request:ActivationToken %>",
					mainCategory
					)
				);

			ParamsDefinition.Add(
				"RegistrationDoAuthentication",
				new BXParamYesNo(GetMessageRaw("Param.RegistrationDoAuthentication"), 
				false, 
				mainCategory)
			);

		}

		public enum ErrorCode
		{
			None,
			Fatal,
			FatalException,
			FatalUserNotFound,
			FatalTokenIsInvalid,
			FatalComponentNotExecuted,
			FatalUserAuthFailed
		}

		public ErrorCode FatalError
		{
			get
			{
				return fatalError;
			}
		}

		void Fatal(ErrorCode code)
		{
			if (code == ErrorCode.FatalException)
				throw new InvalidOperationException("Use method with Exception argument");
			fatalError = code;
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}

		void Fatal(Exception ex)
		{
			if (ex == null)
				throw new ArgumentNullException("ex");

			fatalError = ErrorCode.FatalException;
			fatalException = ex;
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}

		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalTokenIsInvalid:
					return GetMessage("Error.FatalTokenIsInvalid");
				case ErrorCode.FatalUserNotFound:
					return GetMessage("Error.UserNotFound");
				case ErrorCode.FatalUserAuthFailed:
					return GetMessage("Error.FatalUserAuthFailed");
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ?
						("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				default:
					return GetMessage("Error.Unknown");
			}
		}


		protected void Page_Load(object sender, EventArgs e)
		{
			if (BXPrincipal.Current.Identity.IsAuthenticated)
			{
				UserIsAuthorized = true;
				fatalError = ErrorCode.None;
				IncludeComponentTemplate();
				return;
			}
			if (UserId == 0 || ActivationToken == String.Empty || ActivationToken == "<%$ Request:ActivationToken %>")
			{
				return;
			}

			BXUser curUser = BXUser.GetById(UserId);
			if (curUser == null)
			{
				Fatal(ErrorCode.FatalUserNotFound);
				return;
			}

			if (curUser.ActivationToken == string.Empty)
			{
				UserIsAuthorized = true;
				fatalError = ErrorCode.None;
				return;
			}

			if (curUser.ActivationToken != ActivationToken)
			{
				Fatal(ErrorCode.FatalTokenIsInvalid);
				return;
			}

			curUser.ActivationToken = String.Empty;
			curUser.IsApproved = true;
			try
			{
				curUser.Save();
			}
			catch (Exception ex)
			{
				Fatal(ex);
			}

			if (RegistrationDoAuthentication)
			{
				try
				{
					BXAuthentication.SetAuthCookie(curUser.UserName, curUser.ProviderName, false);
					Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
				}
				catch
				{
					Fatal(ErrorCode.FatalUserAuthFailed);
					return;
				}
			}

			if (fatalError == ErrorCode.FatalComponentNotExecuted)
				fatalError = ErrorCode.None;

			string redirectUrl = GetRedirectUrl();

			if ( !String.IsNullOrEmpty(redirectUrl))
				try
				{
					Response.Redirect(redirectUrl);
				}
				catch (ThreadAbortException)
				{
				}

			IncludeComponentTemplate();
		}

		public string GetRedirectUrl()
		{
			string redirectUrl = Request.QueryString["ReturnUrl"];
			if (string.IsNullOrEmpty(redirectUrl))
				redirectUrl = Request.QueryString[BXConfigurationUtility.Constants.BackUrl];
			return redirectUrl;
		}

	}

	public partial class ConfirmationComponentTemplate : BXComponentTemplate<ConfirmationComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{

			string stylePath = AppRelativeTemplateSourceDirectory + "style.css";
			if (File.Exists(HostingEnvironment.MapPath(stylePath)))
				BXPage.RegisterStyle(stylePath);
		}
	}
}




