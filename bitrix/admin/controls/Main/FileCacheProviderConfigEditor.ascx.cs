using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix.DataTypes;
using Bitrix.Services;
using Bitrix.UI;

namespace Bitrix.Main.UI
{
	public partial class FileCacheProviderConfigEditor : BXControl, IBXConfigEditor
	{		
		public void LoadConfig(BXParamsBag<object> config)
		{			
			IsLocal.Checked = config == null || config.GetBool("isLocal", true);
		}
		
		public BXParamsBag<object> SaveConfig()
		{			
			return new BXParamsBag<object>
			{
				{ "isLocal", IsLocal.Checked }
			};
		}
	}
}