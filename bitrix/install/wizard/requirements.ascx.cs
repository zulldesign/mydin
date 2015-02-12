using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using Bitrix.UI.Wizards;
using Bitrix.DataTypes;

namespace Bitrix.Wizards.Install
{
	public partial class RequirementsWizardStep : BXWizardStepStandardHtmlControl
	{
		protected int iisMajor;
		protected enum Check
		{
			IisVersion,
			FrameworkVersion,
			TrustLevel,
			WebConfig,
			BitrixFolder,
			UploadFolder,
			ModulesFolder,
			RootFolder
		}
		protected struct Status
		{
			public string Value;
			public string Description;
			public StatusGrade Grade;
			public string Style()
			{
				switch (Grade)
				{
					case StatusGrade.Red:
						return @"style=""color: Red""";
					case StatusGrade.Green:
						return @"style=""color: Green""";
					case StatusGrade.Yellow:
						return @"style=""color: Olive""";
					default:
						return null;
				}
			}
		}
		protected enum StatusGrade
		{
			Red,
			Yellow,
			Green
		}

		private Dictionary<Check, Status> checks;
		protected Dictionary<Check, Status> Checks
		{
			get
			{
				return checks ?? (checks = DoChecks());
			}
		}

		private Dictionary<Check, Status> DoChecks()
		{
			Dictionary<Check, Status> result = new Dictionary<Check, Status>();
			result[Check.IisVersion] = CheckIisVersion();
			result[Check.FrameworkVersion] = CheckFrameworkVersion();
			result[Check.TrustLevel] = CheckTrustLevel();
			result[Check.WebConfig] = CheckFile("~/web.config");
			result[Check.BitrixFolder] = CheckFolder("~/bitrix/");
			result[Check.ModulesFolder] = CheckFolder("~/bitrix/modules/");
			result[Check.UploadFolder] = CheckFolder("~/upload/");
			result[Check.RootFolder] = CheckFolder("~/");
			return result;
		}

		private Status CheckFile(string virtualPath)
		{
			Status status;
			string content;
			try
			{
				content = File.ReadAllText(HostingEnvironment.MapPath(virtualPath), Encoding.UTF8);
			}
			catch
			{
				status.Value = GetMessage("Value.ReadNotAllowed");
				status.Grade = StatusGrade.Red;
				status.Description = string.Format(GetMessage("Error.FileRead"), virtualPath);
				return status;
			}

			try
			{
				File.WriteAllText(HostingEnvironment.MapPath(virtualPath), content, Encoding.UTF8);
			}
			catch
			{
				status.Value = GetMessage("Value.WriteNotAllowed");
				status.Grade = StatusGrade.Red;
				status.Description = string.Format(GetMessage("Error.FileWrite"), virtualPath);
				return status;
			}

			status.Value = GetMessage("Value.ReadWriteAllowed");
			status.Grade = StatusGrade.Green;
			status.Description = null;
			return status;
		}

		private Status CheckFolder(string virtualPath)
		{
			string folder = HostingEnvironment.MapPath(virtualPath);
			string path = Path.Combine(folder, Guid.NewGuid().ToString("N"));
			Status status;
			try
			{
				try
				{
					File.WriteAllText(path, "", Encoding.UTF8);
				}
				catch
				{
					status.Value = GetMessage("Value.WriteNotAllowed");
					status.Grade = StatusGrade.Red;
					status.Description = string.Format(GetMessage("Error.FileInFolderCreate"), virtualPath);
					return status;
				}

				try
				{
					string text = File.ReadAllText(path, Encoding.UTF8);
				}
				catch
				{
					status.Value = GetMessage("Value.ReadNotAllowed");
					status.Grade = StatusGrade.Red;
					status.Description = string.Format(GetMessage("Error.FileInFolderRead"), virtualPath);
					return status;
				}
			}
			finally
			{
				try
				{
					if (File.Exists(path))
						File.Delete(path);
				}
				catch
				{

				}
			}

			try
			{
				Directory.CreateDirectory(path);
			}
			catch (SecurityException ex)
			{
				status.Grade = StatusGrade.Red;
				if (ex.PermissionType == typeof(FileIOPermission))
					status.Value = GetMessage("Value.NotEnoughFilePermissions");
				else
					status.Value = GetMessage("Value.FolderCreateSecurityException");
				status.Description = string.Format(GetMessage("Error.UnableToCreateFolderTrust"), virtualPath);
				return status;
			}
			catch
			{
				status.Value = GetMessage("Value.FolderCreateNotAllowed");
				status.Grade = StatusGrade.Red;
				status.Description = string.Format(GetMessage("Error.UnableToCreateFolder"), virtualPath);
				return status;
			}
			finally
			{
				try
				{
					if (Directory.Exists(path))
						Directory.Delete(path);
				}
				catch
				{

				}
			}

			status.Value = GetMessage("Value.ReadWriteAllowed");
			status.Grade = StatusGrade.Green;
			status.Description = null;
			return status;
		}

		private Status CheckIisVersion()
		{
			Status status;

			string serverVersion = HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"] ?? "";
			Match m = Regex.Match(serverVersion, @"microsoft-iis/(\d+(?:\.\d+(?:\.\d+)?)?)", RegexOptions.IgnoreCase);
			if (m.Success)
			{
				Version v = new Version(m.Groups[1].Value);
				status.Value = v.ToString();
				status.Grade = (v.Major < 5 || v.Major == 5 && v.Minor < 1) ? StatusGrade.Red : StatusGrade.Green;
				iisMajor = v.Major;
			}
			else
			{
				status.Value = GetMessage("Value.Unknown");
				status.Grade = StatusGrade.Yellow;
			}
			if (status.Grade == StatusGrade.Red)
				status.Description = GetMessage("Error.IISVersion");
			else
				status.Description = null;

			return status;
		}

		private Status CheckFrameworkVersion()
		{
			Status status;
			if (Environment.Version.Major >= 4)
			{
				status.Grade = StatusGrade.Green;
				status.Value = GetMessage("Value.Framework40");
				status.Description = "";
				return status;
			}

			
			try
			{
				System.Reflection.Assembly.Load("System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
				System.Reflection.Assembly.Load("System.Data.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
				status.Grade = StatusGrade.Green;
				status.Value = GetMessage("Value.Framework35SP1");
				status.Description = "";
				return status;
			}
			catch
			{
			}
			
			status.Grade = StatusGrade.Red;
			status.Value = Environment.Version.ToString();
			status.Description = GetMessage("Error.Framework");
			return status;
		}

		private Status CheckTrustLevel()
		{
			Status status;
			AspNetHostingPermissionLevel trustLevel = AspNetHostingPermissionLevel.None;
			foreach (AspNetHostingPermissionLevel l in new AspNetHostingPermissionLevel[] {
			AspNetHostingPermissionLevel.Unrestricted,
			AspNetHostingPermissionLevel.High,
			AspNetHostingPermissionLevel.Medium,
			AspNetHostingPermissionLevel.Low,
			AspNetHostingPermissionLevel.Minimal 
		})
			{
				try
				{
					new AspNetHostingPermission(l).Demand();
				}
				catch (System.Security.SecurityException)
				{
					continue;
				}
				trustLevel = l;
				break;
			}
			status.Value = trustLevel.ToString();
			status.Grade = trustLevel >= AspNetHostingPermissionLevel.Medium ? StatusGrade.Green : StatusGrade.Red;
			if (status.Grade == StatusGrade.Red)
				status.Description = GetMessage("Error.TrustLevel");
			else
				status.Description = null;

			return status;
		}

		protected bool TestCreateDirectory(string virtualPath)
		{
			string path = HostingEnvironment.MapPath(virtualPath);
			try
			{
				Directory.CreateDirectory(path);
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				try
				{
					if (Directory.Exists(path))
						Directory.Delete(path);
				}
				catch
				{

				}
			}
		}

		protected bool TestCreateFile(string virtualPath)
		{
			string path = HostingEnvironment.MapPath(virtualPath);
			try
			{
				File.WriteAllText(path, "", Encoding.UTF8);
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				try
				{
					if (File.Exists(path))
						File.Delete(path);
				}
				catch
				{

				}
			}
		}

		protected string TestSpecialCharsFolders()
		{
			string name = Guid.NewGuid().ToString("N");
			string s;

			s = "~/." + name;
			if (!TestCreateFile(s))
				return string.Format(GetMessage("Value.UnableToCreateSpecialFile"), s);
			if (!TestCreateDirectory(s))
				return string.Format(GetMessage("Value.UnableToCreateSpecialFolder"), s);

			s = "~/~" + name;
			if (!TestCreateFile(s))
				return string.Format(GetMessage("Value.UnableToCreateSpecialFile"), s);
			if (!TestCreateDirectory(s))
				return string.Format(GetMessage("Value.UnableToCreateSpecialFolder"), s);

			return null;
		}

		protected override BXWizardResult OnActionShow(BXCommonBag parameters)
		{
			WizardContext.Navigation.Selected = "requirements";
			
			BXWizardResultView view = Result.Render(GetMessage("Title"));
			view.Buttons.Add("prev", null);
			view.Buttons.Add("next", null);

			return view;
		}

		protected override BXWizardResult OnActionNext(BXCommonBag parameters)
		{
			List<string> errors = new List<string>();
			if (!Validate(errors))
			{
				BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
				view.Buttons.Add("prev", null);
				view.Buttons.Add("next", null);
				return view;
			}
			return Result.Action("install_updater", "", null);
		}

		protected override BXWizardResult OnActionPrevious(BXCommonBag parameters)
		{
			return Result.Previous();
		}

		private bool Validate(List<string> errors)
		{
			foreach (Status s in Checks.Values)
			{
				if (s.Grade == StatusGrade.Red)
					errors.Add(s.Description);
			}
			return errors.Count == 0;
		}
	}
}