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
	public partial class SqlSessionConfigurator : BXControl
	{
		BXSqlSessionConfigurator configurator;
		protected bool isSet;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			configurator = (BXSqlSessionConfigurator)Page.Items[this];			
			isSet = configurator.IsSet;			
		}
		
		protected void Install_Click(object sender, EventArgs e)
		{
			try
			{
				BXSqlSessionConfigurator.InstallDB();
				BXSqlSessionConfigurator.InstallAgent();
				BXSqlSessionConfigurator.InstallConfig();
			}
			catch(Exception ex)
			{
				configurator.ReportError(GetMessageRaw("Message.Error"));
				BXLogService.LogAll(ex, BXLogMessageType.Error, "SqlSessionConfigurator.Install");
				return;
			}
			isSet = true;
			configurator.ReportSuccess();
		}
		protected void Uninstall_Click(object sender, EventArgs e)
		{
			if (configurator.IsSet)
			{
				configurator.ReportError(GetMessageRaw("Message.IsActive"));
				return;
			}
			
			try
			{
				BXSqlSessionConfigurator.UninstallDB();
				BXSqlSessionConfigurator.UninstallAgent();				
			}
			catch(Exception ex)
			{
				configurator.ReportError(GetMessageRaw("Message.Error"));
				BXLogService.LogAll(ex, BXLogMessageType.Error, "SqlSessionConfigurator.Uninstall");
				return;
			}

			configurator.ReportSuccess();
		}		
	}
}