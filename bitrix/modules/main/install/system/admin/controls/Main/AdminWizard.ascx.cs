using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bitrix.UI
{
	public partial class AdminWizard : BXControl
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
	
		public event EventHandler<AdminWizardHostFinishEventArgs> Finish
		{
			add { WizardHost.Finish += value; }
			remove { WizardHost.Finish -= value; }
		}
		public event EventHandler<AdminWizardHostInitStateEventArgs> InitState
		{
			add { WizardHost.InitState += value; }
			remove { WizardHost.InitState -= value; }
		}
	}
}