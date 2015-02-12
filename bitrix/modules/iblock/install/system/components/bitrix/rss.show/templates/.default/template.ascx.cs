using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using System.Collections.Generic;
using Bitrix.IBlock;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.Security;
using System.Text;
using System.Xml;
using Bitrix;
using Bitrix.Services;

public partial class ComponentRssShowTemplate : Bitrix.UI.BXComponentTemplate
{
	protected void Page_Load(object sender, EventArgs e)
	{
	}

	protected override void PrepareDesignMode()
	{
		if (!Results.ContainsKey("Items") || Results["Items"] == null)
		{
			Parameters["msg"] = GetMessageRaw("YouHaveToAdjustTheComponent");
		}

		StartWidth = "100%";
	}

	public new RssShow Component
	{
		get { return base.Component as RssShow; }
	}

	
}
