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
using Bitrix.Security;
using System.Collections.Generic;
using Bitrix.DataLayer;
using Bitrix.Modules;
using System.Collections.ObjectModel;
using Bitrix;
using Bitrix.DataTypes;
using Bitrix.Search;

public partial class bitrix_admin_SearchReindex : BXAdminPage
{
	protected bool indexerIsRunning;
	protected List<BXSite> sites;
	protected bool allSitesSelected;
	protected bool allIndexersSelected;
	protected List<string> selectedSites = new List<string>();
	protected List<IndexerInfo> indexers;
	protected List<BXSearchIndexerStatus> indexersStatus = new List<BXSearchIndexerStatus>();

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
			BXAuthentication.AuthenticationRequired();

		if (BXMediator.HasListeners("Bitrix.Search.IndexAll"))
			OldReindexTab.Visible = true;
	
		indexerIsRunning = BXSearchContentIndexer2.ReadState(indexersStatus);

		sites = new List<BXSite>(BXSite.GetAllSitesReadOnly());
		BXCommand cmd = new BXCommand("Bitrix.Search.EnumerateIndexers");
		cmd.Send();

		// Get Indexers
		indexers = new List<IndexerInfo>();
		foreach (BXCommandResult r in cmd.CommandResultDictionary.Values)
		{
			if (r.CommandResult != BXCommandResultType.Ok)
				continue;

			BXParamsBag<object> info = r.Result as BXParamsBag<object>;
			if (info == null)
				continue;
			string id = info.GetString("id");
			if (id == null)
				continue;
			indexers.Add(new IndexerInfo(id, info, Request.Url));
		}
		indexers.Sort(delegate(IndexerInfo a, IndexerInfo b)
		{
			return string.Compare(a.Id, b.Id, true);
		});
		for (int i = 0; i < indexers.Count; i++)
			indexers[i].Num = i;

		if (!IsPostBack)
		{
			if (indexerIsRunning || indexersStatus.Count != 0)
				BXTabControl1.SelectedIndex = 1;
			allSitesSelected = true;
			allIndexersSelected = true;
		}
		else
			ReadPostData();

		IndexerList.DataSource = indexers;
		IndexerList.DataBind();

		ScriptManager.GetCurrent(Page).RegisterAsyncPostBackControl(StatusTimer);
	}

	private void ReadPostData()
	{
		string[] sitesArray = Request.Form.GetValues(UniqueID + "$sites");
		if (sitesArray != null)
		{
			foreach (string s in sitesArray)
			{
				if (string.IsNullOrEmpty(s))
					allSitesSelected = true;
			}
			if (!allSitesSelected)
			{
				foreach (string s in sitesArray)
				{
					BXSite st;
					if ((st = sites.Find(delegate(BXSite input) { return string.Equals(s, input.TextEncoder.Decode(input.Id), StringComparison.InvariantCultureIgnoreCase); })) != null
						&& !selectedSites.Contains(st.TextEncoder.Decode(st.Id)))
						selectedSites.Add(st.TextEncoder.Decode(st.Id));
				}
			}
		}

		string[] indexersArray = Request.Form.GetValues(UniqueID + "$indexers");
		if (indexersArray != null)
			foreach (string s in indexersArray)
			{
				if (string.IsNullOrEmpty(s))
					allIndexersSelected = true;
				IndexerInfo ii;
				if ((ii = indexers.Find(delegate(IndexerInfo input) { return string.Equals(s, input.Id, StringComparison.InvariantCultureIgnoreCase); })) != null)
					ii.Selected = true;
			}
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = GetMessageRaw("MasterTitle");
		StatusTimer.Enabled = indexerIsRunning;

	}

	protected void IndexerList_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
			return;
		IndexerInfo info = (IndexerInfo)e.Item.DataItem;
		if (info.OptionsControl != null)
			e.Item.FindControl("OptionsContainer").Controls.Add(info.OptionsControl);
	}

	protected void Launch_Click(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
			BXAuthentication.AuthenticationRequired();

		BXSearchIndexingOptions indexing = new BXSearchIndexingOptions();

		if (selectedSites.Count == 0 && !allSitesSelected)
		{
			ErrorMessage.AddErrorMessage(GetMessage("ErrorMessage.NoSites"));
			return;
		}
		if (selectedSites.Count > 0)
			indexing.Sites.AddRange(selectedSites);

		if (!allIndexersSelected)
		{
			bool added = false;
			foreach (IndexerInfo ii in indexers)
			{
				if (!ii.Selected)
					continue;

				BXParamsBag<object> options = null;
				if (ii.GetOptions != null)
					options = ii.GetOptions.DynamicInvoke(ii.OptionsControl) as BXParamsBag<object>;

				indexing.Indexers.Add(new BXSearchIndexerInfo(ii.Id, options));
				added = true;
			}
			if (!added)
			{
				ErrorMessage.AddErrorMessage(GetMessage("ErrorMessage.EmptyArea"));
				return;
			}
		}

		indexing.UpdateOnly = Update.Checked;
		indexing.ResumeOnRestart = ResumeOnRestart.Checked;
		BXSearchContentIndexer2.Stop(true);
		BXSearchContentIndexer2.Launch(indexing);
		Response.Redirect("SearchReindex.aspx");
	}
	protected void Pause_Click(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
			BXAuthentication.AuthenticationRequired();
		BXSearchContentIndexer2.Stop(true);
		Response.Redirect("SearchReindex.aspx");
	}
	protected void Resume_Click(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
			BXAuthentication.AuthenticationRequired();
		BXSearchContentIndexer2.Stop(true);
		BXSearchContentIndexer2.Launch(null);
		Response.Redirect("SearchReindex.aspx");
	}

	protected void StatusTimer_Tick(object sender, EventArgs e)
	{
		Status.Update();
		if (!indexerIsRunning)
			Response.Redirect(indexersStatus.Count == 0 ? "SearchReindex.aspx?status=finished" : "SearchReindex.aspx");
	}

	public class IndexerInfo
	{
		public IndexerInfo(string id, BXParamsBag<object> info, Uri currentUrl)
		{
			Id = id;
			Title = info.GetString("title");
			CreateOptionsControl = info.Get<Delegate>("createOptionsControl");
			GetOptions = info.Get<Delegate>("getOptions");
			if (CreateOptionsControl != null)
				OptionsControl = (Control)CreateOptionsControl.DynamicInvoke();
			string icon = info.GetString("icon");
			if (!string.IsNullOrEmpty(icon))
			{
				Uri iconUri;
				if (Uri.TryCreate(currentUrl, icon.StartsWith("~/") ? VirtualPathUtility.ToAbsolute(icon) : icon, out iconUri))
					IconUrl = iconUri.AbsoluteUri;
			}
		}

		public string Id;
		public string Title;
		public Delegate CreateOptionsControl;
		public Delegate GetOptions;
		public string IconUrl;

		public int Num;
		public Control OptionsControl;
		public bool Selected;
	}
}
