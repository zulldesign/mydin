using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Web.Security;
using Bitrix.Components;
using Bitrix.DataTypes;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.UI;

namespace Bitrix.Main.Components
{
	public partial class SystemPasswordRecoveryComponent : BXComponent
	{
		public string LoginLink
		{
			get
			{
				if (!Parameters.ContainsKey("LoginLink"))
					Parameters["LoginLink"] = string.Empty;
				return Parameters["LoginLink"];
			}
		}

		public bool FeatureEnabled
		{
			get { return (BXUserManager.Provider.EnablePasswordReset && BXUserManager.Provider.RequiresQuestionAndAnswer); }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			IncludeComponentTemplate();
		}

		public bool UseCaptcha
		{
			get
			{
				return Parameters.GetBool("UseCaptcha", false);
			}
		}

		protected override void PreLoadComponentDefinition()
		{
			base.Title = GetMessage("SystemPasswordRecovery.Title");
			base.Description = GetMessage("SystemPasswordRecovery.Description");
			base.Icon = "images/user_authform.gif";

			Group = new BXComponentGroup("Auth", GetMessageRaw("User"), 100, BXComponentGroup.Utility);

			ParamsDefinition.Add(
				"LoginLink",
				new BXParamText(
					GetMessageRaw("PathToAuthorizationScript"),
					String.Empty,
					BXCategory.Main
				)
			);

			ParamsDefinition.Add(
				"UseCaptcha",
				new BXParamYesNo(
					GetMessageRaw("UseCaptcha"),
					false,
					BXCategory.Main
				)
			);
		}

		public override bool ProcessCommand(string commandName, BXParamsBag<object> commandParameters, List<string> commandErrors)
		{
			if (commandName.Equals("validate", StringComparison.InvariantCultureIgnoreCase))
			{
				string login = (string)commandParameters["Login"];

				BXUser user = BXUserManager.GetByName(login, null, false);
				if (user != null && user.IsApproved)
				{
					if (user.IsLockedOut)
					{
						commandErrors.Add(GetMessageRaw("Error.UserIsLocked"));
						return false;
					}
					else
					{
						commandParameters["PasswordQuestion"] = user.PasswordQuestion;
						return true;
					}
				}
				else
				{
					commandErrors.Add(GetMessageRaw("Error.UserIsNotFound"));
					return false;
				}
				return false;
			}
			if (commandName.Equals("recovery", StringComparison.InvariantCultureIgnoreCase))
			{
				string login = (string)commandParameters["Login"];
				string passwordAnswer = (string)commandParameters["PasswordAnswer"];

				BXUser user = BXUserManager.GetByName(login, null, false);
				if (user != null)
				{
					try
					{
						string val = user.ResetPassword(passwordAnswer, null);

						BXCommand c = new BXCommand("Bitrix.Main.PasswordRecovery");
						c.Parameters.Add("@site", BXSite.Current.Id);
						c.Parameters.Add("USER_ID", user.UserId);
						c.Parameters.Add("MESSAGE", GetMessageRaw("Command.Parameter.YouHaveRequestedYourRegistrationData"));
						c.Parameters.Add("LOGIN", user.UserName);
						c.Parameters.Add("CHECKWORD", val);
						c.Parameters.Add("EMAIL", user.Email);
						c.Send();

						return true;
					}
					catch (NotSupportedException exception)
					{
						commandErrors.Add(GetMessageRaw("Error.AnErrorHasOccurredWhileChangingPassword") + exception.Message);
					}
					catch (MembershipPasswordException exception)
					{
						if (exception.Message.Equals("WrongAnswer", StringComparison.InvariantCultureIgnoreCase))
							commandErrors.Add(GetMessageRaw("Error.AnswerIsInvalid"));
						else
							commandErrors.Add(GetMessageRaw("Error.PasswordIsInvalid") + exception.Message);
					}
					catch (ProviderException exception)
					{
						commandErrors.Add(GetMessageRaw("Error.ProviderError") + exception.Message);
					}
					catch (Exception exception)
					{
						commandErrors.Add(GetMessageRaw("Error.GeneralError") + exception.Message);
					}
				}
				else
				{
					commandErrors.Add(GetMessageRaw("Error.UserIsNotFound"));
				}

				return false;
			}
			return false;
		}
	}

	public class SystemPasswordRecoveryTemplate : BXComponentTemplate<SystemPasswordRecoveryComponent> {}
}

#region Compatibility
[Obsolete("Use Bitrix.Main.Components.SystemPasswordRecoveryComponent class")]
public partial class RasswordRecoveryComponent : Bitrix.Main.Components.SystemPasswordRecoveryComponent {}
#endregion