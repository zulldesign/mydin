using Bitrix.UI;
using System;

namespace Bitrix.Installer
{
	public partial class DefaultInstaller : BXControl
	{
		public string Url
		{
			get
			{
				return WizardHost.Url;
			}
			set
			{
				WizardHost.Url = value;
			}
		}

		public string Locale
		{
			get
			{
				return WizardHost.Locale;
			}
			set
			{
				WizardHost.Locale = value;
				Bitrix.Services.BXLoc.CurrentLocale = value;
			}
		}

		public string WizardPath
		{
			get
			{
				return WizardHost.WizardPath;
			}
			set
			{
				WizardHost.WizardPath = value;
			}
		}
	
		public event EventHandler<WizardHostFinishEventArgs> Finish
		{
			add { WizardHost.Finish += value; }
			remove { WizardHost.Finish -= value; }
		}

		public event EventHandler<WizardHostInitStateEventArgs> InitState
		{
			add { WizardHost.InitState += value; }
			remove { WizardHost.InitState -= value; }
		}
	}
}