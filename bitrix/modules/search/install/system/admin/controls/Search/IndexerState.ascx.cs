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


using Bitrix.Modules;
using Bitrix.Search;
using Bitrix.UI;
using Bitrix.DataTypes;
using Bitrix.Security;

public partial class bitrix_admin_controls_Search_IndexerState : BXControl
{
	const int RefreshInterval = 10000;

	protected void Page_PreRender(object sender, EventArgs e)
	{
		Dictionary<string, BXPair<string, bool>> state = new Dictionary<string,BXPair<string,bool>>();
		bool working = BXSearchContentIndexer.ReadState(state);
		StateText.Text = working ? GetMessage("IndexingInProcess") : (state.Count == 0 ?GetMessage("IndexingFinished") : GetMessage("IndexingStopped"));
		foreach (KeyValuePair<string, BXPair<string, bool>> f in state)
			if (f.Value.B)
				StateText.Text += string.Format("<br/>{0}: {1}", HttpUtility.HtmlEncode(f.Key), GetMessage("Finished"));
		foreach (KeyValuePair<string, BXPair<string, bool>> f in state)
			if (!f.Value.B)
				StateText.Text += string.Format("<br/>{0}: {1} ({2})", HttpUtility.HtmlEncode(f.Key), working ? GetMessage("Indexing") : GetMessage("IndexingStoppedOn"), HttpUtility.HtmlEncode(f.Value.A));

		Restart.Enabled = !working;
		Stop.Enabled = working;
		Resume.Enabled = !working && state.Count != 0;
		string script =
@"if (!window.RefreshIndexer) 
{{
	window.RefreshIndexer = function ()
	{{
		var d = document.getElementById('{0}');
		if (d) 
			d.click(); 
		window.RefreshIndexer = null;
	}}; 
	setTimeout(window.RefreshIndexer, {1});
}}";
		if (working)
			ScriptManager.RegisterStartupScript(this.StatePanel, GetType(), "SetIndexerStateRefresh", string.Format(script, Refresh.ClientID, RefreshInterval), true);
	}
	protected void Restart_Click(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
			BXAuthentication.AuthenticationRequired();
		BXSearchContentIndexer.Start(true, DropIndex.Checked);
	}
	protected void Stop_Click(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
			BXAuthentication.AuthenticationRequired();
		BXSearchContentIndexer.Stop();
	}
	protected void Resume_Click(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
			BXAuthentication.AuthenticationRequired();
		BXSearchContentIndexer.Start(false);
	}
}
