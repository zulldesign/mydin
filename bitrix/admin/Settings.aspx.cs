using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.Modules;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.IO;
using Bitrix.Security;

public partial class bitrix_admin_Settings : BXAdminPage
{
	private void BindEntitySelector(Dictionary<string, string> selectorList)
	{
		string moduleId = GetRequestString("module_id");
		moduleId = HttpUtility.UrlDecode(moduleId);

		if (String.IsNullOrEmpty(moduleId) || !selectorList.ContainsKey(moduleId))
			moduleId = "Bitrix.Main.BXMain";

		ddlEntitySelector.Items.Clear();
		foreach (KeyValuePair<string, string> kvp1 in selectorList)
			ddlEntitySelector.Items.Add(new ListItem(kvp1.Value, kvp1.Key));

		ddlEntitySelector.SelectedValue = moduleId;
	}
	
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

        ((BXAdminMasterPage)Master).PageIconId = "sys_page_icon";
	}

	protected void ddlEntitySelector_SelectedIndexChanged(object sender, EventArgs e)
	{
		Response.Redirect("Settings.aspx?module_id=" + UrlEncode(ddlEntitySelector.SelectedValue));
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		Dictionary<string, string> selectorList = new Dictionary<string, string>();
		BXCommand command = new BXCommand("Bitrix.Main.OnGlobalOptionsSubscribe");
		command.Send();
		
		List<KeyValuePair<string, BXCommandResult>> values = new List<KeyValuePair<string,BXCommandResult>>();
		foreach (KeyValuePair<string, BXCommandResult> kvp in command.CommandResultDictionary)
			if (kvp.Value.CommandResult == BXCommandResultType.Ok)
				values.Add(kvp);

		values.Sort(delegate(KeyValuePair<string, BXCommandResult> a, KeyValuePair<string, BXCommandResult> b)
		{
			return string.Compare(a.Value.Result.ToString(), b.Value.Result.ToString(), true);
		});

		foreach(KeyValuePair<string, BXCommandResult> kvp in values)
			selectorList.Add(kvp.Key, kvp.Value.Result.ToString());

		if (!Page.IsPostBack)
			BindEntitySelector(selectorList);

		LoadSelectedControl(ddlEntitySelector.SelectedValue, selectorList);

		MasterTitle = GetMessageFormat("MasterTitle.GlobalSettings", selectorList[ddlEntitySelector.SelectedValue]);
	}

	private void LoadSelectedControl(string entityId, Dictionary<string, string> selectorList)
	{
		Dictionary<string, PlaceHolder> placeholderList = new Dictionary<string, PlaceHolder>();
		foreach (KeyValuePair<string, string> kvp1 in selectorList)
		{
			PlaceHolder p = new PlaceHolder();
			Settings.Controls.Add(p);
			placeholderList.Add(kvp1.Key, p);
		}
				
		//List<string> paths = new List<string>();

		BXCommand command = new BXCommand("Bitrix.Main.OnGlobalOptionsEdit");
		//command.FilterListeners = new string[] { entityId };
		command.Parameters.Add("EntityId", entityId);
		command.Send();

		BXCommandResult r;
		if (command.CommandResultDictionary.TryGetValue(entityId, out r) && r.CommandResult == BXCommandResultType.Ok)
		{
			string path = r.Result.ToString();
			if (File.Exists(BXPath.MapPath(path)))
			{
				Control ctrl = LoadControl(path);
				placeholderList[entityId].Controls.Add(ctrl);
			}
		}
	}
}
