using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Web.Security;
using Bitrix.Components;
using Bitrix.DataTypes;
using Bitrix.Security;
using Bitrix.UI;

namespace Bitrix.Main.Components
{
	public partial class SystemPasswordRecoveryAltCodeComponent : BXComponent
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
			get { return (BXUserManager.Provider.EnablePasswordReset && BXUserManager.Provider.RequiresCheckWord); }
		}

		public bool UseCaptcha
		{
			get
			{
				return Parameters.GetBool("UseCaptcha", false);
			}
		}

		public string Checkword
		{
			get { return (string)(Results.ContainsKey("Checkword") ? Results["Checkword"] : ""); }
			set { Results["Checkword"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
				if (Page.Request.QueryString["checkword"] != null)
					this.Checkword = Page.Request.QueryString["checkword"];

			IncludeComponentTemplate();
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessage("SystemPasswordRecoveryAltCode.Title");
			Description = GetMessage("SystemPasswordRecoveryAltCode.Description");
			Icon = "images/user_authform.gif";

			Group = new BXComponentGroup("Auth", GetMessage("Security"), 100, BXComponentGroup.Utility);

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
			if (commandName.Equals("recovery", StringComparison.InvariantCultureIgnoreCase))
			{
				string login = (string)commandParameters["Login"];
				string checkWord = (string)commandParameters["Checkword"];
				string password = (string)commandParameters["Password"];
				string passwordConf = (string)commandParameters["PasswordConf"];

				if (password.Equals(passwordConf, StringComparison.InvariantCultureIgnoreCase))
				{
					BXUser user = BXUserManager.GetByName(login, null, false);
					if (user != null && user.IsApproved)
					{
						try
						{
							user.ResetPassword(null, checkWord, password);
							return true;
						}
						catch (NotSupportedException exception)
						{
							commandErrors.Add(GetMessage("Error.AnErrorHasOccurredWhileChangingPassword") + exception.Message);
						}
						catch (MembershipPasswordException exception)
						{
							if (exception.Message.Equals("WrongAnswer", StringComparison.InvariantCultureIgnoreCase))
								commandErrors.Add(GetMessage("Error.ControlStringIsIncorrect"));
							else
								commandErrors.Add(GetMessage("Error.PasswordIsInvalid") + exception.Message);
						}
						catch (ProviderException exception)
						{
							commandErrors.Add(GetMessage("Error.ProviderError") + exception.Message);
						}
						catch (Exception exception)
						{
							commandErrors.Add(GetMessage("Error.GereralError") + exception.Message);
						}
					}
					else
					{
						commandErrors.Add(GetMessage("Error.UserIsNotFound"));
					}
				}
				else
				{
					commandErrors.Add(GetMessage("Error.PasswordAndItsConfirmationDontMatch"));
				}
			}
			return false;
		}
	}

	public class SystemPasswordRecoveryAltCodeTemplate : BXComponentTemplate<SystemPasswordRecoveryAltCodeComponent> {}
}

#region Compatibility
[Obsolete("Use Bitrix.Main.Components.SystemPasswordRecoveryAltCodeComponent class")]
public partial class RasswordRecoveryAltCodeComponent : Bitrix.Main.Components.SystemPasswordRecoveryAltCodeComponent {}
#endregion