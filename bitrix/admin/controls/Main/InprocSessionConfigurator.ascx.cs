using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Configuration;
using Bitrix.Services;

namespace Bitrix.Main.UI
{
	public partial class InprocSessionConfigurator : BXControl
	{
		BXInprocSessionConfigurator configurator;
		protected bool isSet;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			configurator = (BXInprocSessionConfigurator)Page.Items[this];			
			isSet = configurator.IsSet;			
		}
		
		protected void Install_Click(object sender, EventArgs e)
		{
			try
			{			
				BXInprocSessionConfigurator.InstallConfig();
			}
			catch(Exception ex)
			{
				configurator.ReportError(GetMessageRaw("Message.Error"));
				BXLogService.LogAll(ex, BXLogMessageType.Error, "InprocSessionConfigurator.Install");
				return;
			}
			isSet = true;
			configurator.ReportSuccess();
		}		
	}
}