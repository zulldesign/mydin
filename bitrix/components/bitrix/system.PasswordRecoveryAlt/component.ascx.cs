using System;
using System.Collections.Generic;
using Bitrix.Components;
using Bitrix.DataTypes;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.UI;

namespace Bitrix.Main.Components
{
	public partial class SystemPasswordRecoveryAltComponent : BXComponent
	{
		public string PasswordRecoveryCodeLink
		{
			get
			{
				if (!Parameters.ContainsKey("PasswordRecoveryCodeLink"))
					Parameters["PasswordRecoveryCodeLink"] = string.Empty;
				return Parameters["PasswordRecoveryCodeLink"];
			}
		}

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
			base.Title = GetMessage("SystemPasswordRecoveryAlt.Title");
			base.Description = GetMessage("SystemPasswordRecoveryAlt.Description");
			base.Icon = "images/user_authform.gif";

			Group = new BXComponentGroup("Auth", GetMessage("User"), 100, BXComponentGroup.Utility);

			ParamsDefinition.Add(
				"PasswordRecoveryCodeLink",
				new BXParamText(
					GetMessageRaw("PathToGettingControlWordScript"),
					String.Empty,
					BXCategory.Main
				)
			);

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
				string email = (string)commandParameters["Email"];
				BXUser user;

				if (String.IsNullOrEmpty(login) && String.IsNullOrEmpty(email))
				{
					commandErrors.Add(GetMessageRaw("LoginOrEmailIsRequired"));
				}
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
							c.Parameters.Add("@site", BXSite.Current.Id);
							c.Parameters.Add("SERVER_NAME", Bitrix.Services.BXSefUrlManager.CurrentUrl.Host);
							c.Parameters.Add("USER_ID", user.UserId);
							c.Parameters.Add("MESSAGE", GetMessageRaw("Command.Parameter.YouHaveRequestedYourRegistrationData"));
							c.Parameters.Add("LOGIN", user.UserName);
							c.Parameters.Add("CHECKWORD", Server.UrlEncode(newCheckWord));
							c.Parameters.Add("EMAIL", user.Email);
							c.Parameters.Add("LINK", Parameters["PasswordRecoveryCodeLink"] + (Parameters["PasswordRecoveryCodeLink"].Contains("?") ? "&" : "?") + "checkword=" + Server.UrlEncode(newCheckWord));
							c.Parameters.Add(
								"USER_NAME",
								(user.FirstName == null ? "" : user.FirstName) 
								+ ((!String.IsNullOrEmpty(user.FirstName) && !String.IsNullOrEmpty(user.LastName)) ? " " : "") 
								+ (user.LastName == null ? "" : user.LastName)
								+ ((!String.IsNullOrEmpty(user.FirstName) || !String.IsNullOrEmpty(user.LastName)) ? "," : ""));
							c.Send();

							return true;
						}
						else
						{
							commandErrors.Add(GetMessageRaw("Error.UserIsNotFound"));
						}
					}
					else
					{
						commandErrors.Add(GetMessageRaw("Error.EmailIsNotFound"));
					}
				}
			}
			return false;
		}
	}

	public class SystemPasswordRecoveryAltTemplate : BXComponentTemplate<SystemPasswordRecoveryAltComponent> {}
}

#region Compatibility
[Obsolete("Use Bitrix.Main.Components.SystemPasswordRecoveryAltComponent class")]
public partial class RasswordRecoveryAltComponent : Bitrix.Main.Components.SystemPasswordRecoveryAltComponent {}
#endregion