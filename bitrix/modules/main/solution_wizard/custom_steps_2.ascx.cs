using Bitrix.IO;
using Bitrix.UI.Wizards;
using Bitrix.DataTypes;

namespace Bitrix.Wizards.Solutions
{
	public partial class CustomSteps2WizardStep : BXWizardStepStandardHtmlControl, IBXWizardSubSequenceProvider
	{
		protected override BXWizardResult OnWizardAction(string action, BXCommonBag parameters)
		{
			switch (action)
			{
				case "sequence_finish":
					WizardContext.State["Installer.CustomSteps2.GoBack"] = "";
					return Result.Next();
				case "sequence_cancel":
					WizardContext.State.Remove("Installer.CustomSteps2.GoBack");
					return Result.Previous();
				default:
					return base.OnWizardAction(action, parameters);
			}
		}

		protected override BXWizardResult OnActionShow(BXCommonBag parameters)
		{
			string path = BXPath.Combine((string)WizardContext.State["Installer.SolutionPath"], "custom_steps_2");
			string fullPath = BXPath.MapPath(path);

			if (WizardContext.State.ContainsKey("Installer.CustomSteps2.GoBack"))
			{
				WizardContext.State.Remove("Installer.CustomSteps2.GoBack");
				if (System.IO.Directory.Exists(fullPath))
					return BXWizard.StartSequence("", true, false);
				else 
					return Result.Previous();
			}
			else
			{
				WizardContext.State["Installer.CustomSteps2.GoBack"] = "";
				PrepareProgressBar();
				if (System.IO.Directory.Exists(fullPath))
					return BXWizard.StartSequence("", false, true);
				else 
					return Result.Next();
			}
		}

		private void PrepareProgressBar()
		{
			UI.ClearProgressBar("Installer.ProgressBar", true);
			UI.SetProgressBarMaxValue("Installer.ProgressBar", "InstallTemplate", 1);
		}

		#region IBXWizardSubSequenceProvider Members

		public string GetSubSequencePath(string id, BXWizardContext context)
		{
			return BXPath.Combine((string)context.State["Installer.SolutionPath"], "custom_steps_2");
		}

		#endregion
	}
}