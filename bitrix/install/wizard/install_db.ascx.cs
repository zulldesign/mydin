using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.Hosting;
using Bitrix.DataTypes;
using Bitrix.UI.Wizards;
using Bitrix.Services.Text;

namespace Bitrix.Wizards.Install
{
	public partial class InstallDBWizardStep : BXWizardStepStandardHtmlControl
	{
		protected override void OnWizardInit()
		{
			UI.SetProgressBarMaxValue("Installer.ProgressBar", "DB", 5);
		}


		List<string> errors;
		private List<string> Errors
		{
			get { return errors ?? (errors = new List<string>()); }
		}
		
		protected string htmlMessage;
		private BXWizardResult ShowStatus(string htmlMessage, string nextStep, string nextAction)
		{
			this.htmlMessage = htmlMessage;
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
			if ((errors == null || errors.Count == 0) && (!string.IsNullOrEmpty(nextStep) || !string.IsNullOrEmpty(nextAction)))
			{
				view.AutoRedirect = true;
				view.RedirectStep = nextStep;
				view.RedirectAction = nextAction;
			}
			WizardContext.Navigation.Selected = "install";
			return view;
		}


		protected override BXWizardResult OnWizardAction(string action, BXCommonBag parameters)
		{
			switch (action)
			{
				case "":
					UI.ClearProgressBar("Installer.ProgressBar", false);
					return ShowStatus(GetMessage("SubTitle"), null, "createdb");
				case "createdb":
					return CreateDB();
				case "createuser":
					return CreateUser();
				case "attachuser":
					return AttachUser();
				case "preparedb":
					return PrepareDB();
				case "setcollation":
					return SetCollation();
				case "finalize":
					return Finalize();
				default:
					return base.OnWizardAction(action, parameters);
			}
		}

		
		private static string BuildAdminConnectionString(BXParamsBag<object> data)
		{
			SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
			sb.DataSource = data.GetString("ServerName");
			sb.ConnectTimeout = 1000;
			if (data.GetBool("AdminWindows"))
				sb.IntegratedSecurity = true;
			else
			{
				sb.UserID = data.GetString("AdminLogin", "");
				sb.Password = data.GetString("AdminPassword", "");
			}
			return sb.ConnectionString;
		}
		private static string BuildUserConnectionString(BXParamsBag<object> data, bool setTimeout)
		{
			SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
			sb.DataSource = data.GetString("ServerName");
			sb.InitialCatalog = data.GetString("DBName");
			if (setTimeout)
				sb.ConnectTimeout = 1000;
			if (data.GetString("CreateUser") == "Windows")
				sb.IntegratedSecurity = true;
			else
			{
				sb.UserID = data.GetString("UserLogin", "");
				sb.Password = data.GetString("UserPassword", "");
			}
			return sb.ConnectionString;
		}

		private BXWizardResult CreateDB()
		{
			BXParamsBag<object> data = (BXParamsBag<object>)WizardContext.State["DB"];
			WizardContext.State["Install.DB.DatabaseCreated"] = false;
			if (!WizardContext.State.ContainsKey("Options.ConnectionString") && data.GetString("CreateDB") == "True")
			{
				using (SqlConnection c = new SqlConnection(BuildAdminConnectionString(data)))
				using (SqlCommand cmd = new SqlCommand(
					string.Format("CREATE DATABASE [{0}] COLLATE SQL_Latin1_General_CP1_CI_AS", data.GetString("DBName").Replace("]", "]]")),
					c
				))
				{
					c.Open();
					cmd.ExecuteNonQuery();
				}
				WizardContext.State["Install.DB.DatabaseCreated"] = true;
			}
			UI.SetProgressBarValue("Installer.ProgressBar", "DB", 1);
			return ShowStatus(GetMessage("SubTitle"), null, "createuser");
		}
		private BXWizardResult CreateUser()
		{
			BXParamsBag<object> data = (BXParamsBag<object>)WizardContext.State["DB"];
			WizardContext.State["Install.DB.UserCreated"] = false;
			if (!WizardContext.State.ContainsKey("Options.ConnectionString") && data.GetString("CreateUser") == "True")
			{
				using (SqlConnection c = new SqlConnection(BuildAdminConnectionString(data)))
				using (SqlCommand cmd = new SqlCommand(
					string.Format(
						"CREATE LOGIN [{0}] WITH PASSWORD = '{1}'",
						data.GetString("UserLogin").Replace("]", "]]"),
						data.GetString("UserPassword").Replace("'", "''")),
					c
				))
				{
					c.Open();
					cmd.ExecuteNonQuery();
				}
				WizardContext.State["Install.DB.UserCreated"] = true;
			}
			UI.SetProgressBarValue("Installer.ProgressBar", "DB", 2);
			return ShowStatus(GetMessage("SubTitle"), null, "attachuser");
		}
		private BXWizardResult AttachUser()
		{
			if ((bool)WizardContext.State["Install.DB.DatabaseCreated"] || (bool)WizardContext.State["Install.DB.UserCreated"])
			{
				BXParamsBag<object> data = (BXParamsBag<object>)WizardContext.State["DB"];
				if (
					data.GetString("CreateUser") != "Windows"
					&& !string.Equals(data.GetString("UserLogin"), "sa", StringComparison.OrdinalIgnoreCase)
					&& (
						data.GetBool("AdminWindows")
						|| !string.Equals(data.GetString("UserLogin"), data.GetString("AdminLogin"), StringComparison.OrdinalIgnoreCase)
					)
				)
				{
					using (SqlConnection c = new SqlConnection(BuildAdminConnectionString(data)))
					using (SqlCommand cmd = new SqlCommand(
						string.Format(
							"USE [{0}]; CREATE USER [{1}] FOR LOGIN [{1}]; EXEC sp_addrolemember N'db_owner', N'{2}';EXEC sp_addrolemember N'db_backupoperator', N'{2}';",
							data.GetString("DBName").Replace("]", "]]"),
							data.GetString("UserLogin").Replace("]", "]]"),
							data.GetString("UserLogin").Replace("'", "''")
						),
						c
					))
					{
						c.Open();
						cmd.ExecuteNonQuery();
					}
				}
			}
			UI.SetProgressBarValue("Installer.ProgressBar", "DB", 3);
			return ShowStatus(GetMessage("SubTitle"), null, "preparedb");
		}
		private BXWizardResult PrepareDB()
		{
			BXParamsBag<object> data = (BXParamsBag<object>)WizardContext.State["DB"];
			string cs = WizardContext.State.GetString("Options.ConnectionString");
			if (cs != null 
				|| !(bool)WizardContext.State["Install.DB.DatabaseCreated"] && data.GetBool("EmptyDB"))
			{
				using (SqlConnection c = new SqlConnection(cs != null ? cs : BuildUserConnectionString(data, true)))
				using (SqlCommand cmd = new SqlCommand(
					File.ReadAllText(HostingEnvironment.MapPath(AppRelativeTemplateSourceDirectory + "cleandb.sql"), Encoding.UTF8),
					c
				))
				{
					c.Open();
					cmd.ExecuteNonQuery();
				}
			}

			using (SqlConnection c = new SqlConnection(cs != null ? cs : BuildUserConnectionString(data, true)))
			using (SqlCommand cmd = new SqlCommand(
				File.ReadAllText(HostingEnvironment.MapPath(AppRelativeTemplateSourceDirectory + "setdboschema.sql"), Encoding.UTF8),
				c
			))
			{
				c.Open();
				cmd.ExecuteNonQuery();
			}
			UI.SetProgressBarValue("Installer.ProgressBar", "DB", 4);
			return ShowStatus(GetMessage("SubTitle"), null, "setcollation");
		}
		private BXWizardResult SetCollation()
		{
			if (!(bool)WizardContext.State["Install.DB.DatabaseCreated"])
			{
				if (!WizardContext.State.ContainsKey("Options.ConnectionString"))
				{
					BXParamsBag<object> data = (BXParamsBag<object>)WizardContext.State["DB"];
					DoSetCollation(
						BuildUserConnectionString(data, true), 
						data.GetString("DBName")
					);
				}
				else if (
					!BXStringUtility.IsNullOrTrimEmpty(WizardContext.State.GetString("Options.ForceCollationConnectionString")) 
					&& !BXStringUtility.IsNullOrTrimEmpty(WizardContext.State.GetString("Options.ForceCollationDatabase"))					
				)
				{
					DoSetCollation(
						WizardContext.State.GetString("Options.ForceCollationConnectionString"), 
						WizardContext.State.GetString("Options.ForceCollationDatabase")
					);
				}
			}
			UI.SetProgressBarValue("Installer.ProgressBar", "DB", 5);
			return ShowStatus(GetMessage("SubTitle"), null, "finalize");
		}

		private static void DoSetCollation(string cs, string db)
		{
			SqlConnection conn = null;
			try
			{
				try
				{
					using (SqlCommand cmd = new SqlCommand(
						string.Format(
							@"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
							db.Replace("]", "]]")
						),
						conn = new SqlConnection(cs)
					))
					{
						conn.Open();
						cmd.ExecuteNonQuery();
					}
				}
				finally
				{
					if (conn != null)
					{
						conn.Close();
						SqlConnection.ClearPool(conn);
						conn = null;
					}
				}

				try
				{
					using (SqlCommand cmd = new SqlCommand(
						string.Format(
							@"ALTER DATABASE [{0}] COLLATE SQL_Latin1_General_CP1_CI_AS",
							db.Replace("]", "]]")
						),
						conn = new SqlConnection(cs)
					))
					{
						conn.Open();
						cmd.ExecuteNonQuery();
					}
				}
				finally
				{
					if (conn != null)
					{
						conn.Close();
						SqlConnection.ClearPool(conn);
						conn = null;
					}
				}
			}
			catch
			{
			}
			finally
			{
				try
				{
					using (SqlCommand cmd = new SqlCommand(
						string.Format(
							@"ALTER DATABASE [{0}] SET MULTI_USER",
							db.Replace("]", "]]")
						),
						conn = new SqlConnection(cs)
					))
					{
						conn.Open();
						cmd.ExecuteNonQuery();
					}
				}
				catch { }
				finally
				{
					if (conn != null)
					{
						conn.Close();
						SqlConnection.ClearPool(conn);
						conn = null;
					}
				}
			}
		}
		private BXWizardResult Finalize()
		{
			string cs;
			if (WizardContext.State.TryGetString("Options.ConnectionString", out cs))
				WizardContext.State["Install.ConnectionString"] = cs;
			else 
				WizardContext.State["Install.ConnectionString"] = BuildUserConnectionString((BXParamsBag<object>)WizardContext.State["DB"], false);
			return Result.Next();
		}
	}
}