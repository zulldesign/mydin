using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.ObjectModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix.Services;
using Bitrix.Main;
using System.Collections.Generic;
using Bitrix;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.IBlock;
using System.Threading;
using Bitrix.IBlock.PublicEditors;
using Bitrix.IBlock.UI;
using Bitrix.Configuration;
using Bitrix.DataTypes;
using Bitrix.Modules;

public partial class bitrix_admin_IBlockElementEditSettings : BXDialogPage
{

	public Dictionary<string, Dictionary<string, Bitrix.IBlock.PublicEditors.BXIBlockPublicEditorElementField>> editors;

	BXIBlock iblock;
	BXIBlockElement element;

	protected int iblockId = -1;
	protected int typeId = -1;
	protected int elementId = -1;

	UserIBlockEditorConfigInfo userConfigInfo = null;

	public BXIBlock IBlock
	{
		get
		{
			if(iblock != null || iblockId >= 0)
				return iblock;

			if (iblockId == -1 && Request.QueryString["iblock_id"] != null)
				int.TryParse(Request.QueryString["iblock_id"], out iblockId);

			if (iblockId == -1)
				iblockId = 0;

			if(iblockId <= 0)
				return null;

			return iblock = BXIBlock.GetById(iblockId); 
				
		}
	}

	protected void Page_LoadComplete(object sender, EventArgs e)
	{
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		if (IBlock == null)
			return;
		userConfigInfo = BXIBlockPublicEditorStructureBuilder.GetConfig(BXIdentity.Current.Id, IBlock.Id, true);

		var page = Page as BXDialogPage;
		Behaviour.ValidationGroup = "IBlockSettingsValidate";
		Page.Title = GetMessage("Title.IBlockFormSettings");

	}

	protected string GetTabsJSON()
	{
		var sb = new StringBuilder();
		sb.Append("[");
		foreach (var g in userConfigInfo.Groups)
		{
			sb.AppendFormat("{{id:\"{0}\",name:\"{1}\", selected:{2}}},", g.GroupId, 
				JSEncode(g.Title), g.IsSelected ? "true":"false");
		}
		sb.Remove(sb.Length - 1, 1);
		sb.Append("]");
		return sb.ToString();
	}

	protected string GetFieldsJSON()
	{
		var sb = new StringBuilder();
		sb.Append("{");

		foreach (var field in userConfigInfo.Fields)
			sb.AppendFormat("{0}:{{id:\"{0}\",name:\"{1}\", selected:{3}, tabId:'{2}', defTabId:'{5}', sort:0, isSect:{6}, required:{4}}},",
				field.Id, JSEncode(field.IsSection ? "--"+ field.Title : field.Title), field.GroupId, field.IsSelected ? "true" : "false", field.IsRequired ? "true" : "false", 
				field.DefaultGroupId, field.IsSection ? "true" : "false");
		
		sb.Remove(sb.Length - 1, 1);
		sb.Append("}");

		return sb.ToString();
	}

	protected void behaviour_Save(object sender, EventArgs e)
	{
		try
		{
			if (iblock == null || !BXIdentity.Current.IsAuthenticated)
			{
				Close(GetMessage("Error.IBlockOrUserIsNotFound"), BXDialogGoodbyeWindow.LayoutType.Error, 0);
				return;
			}

			if (ResetSettings.Checked)
			{
				//BXIBlockPublicEditorStructureBuilder.RestoreUserConfig(BXIdentity.Current.Id, iblock.Id);
				BXIBlockPublicEditorStructureBuilder.DeleteUserConfig(BXIdentity.Current.Id, iblock.Id);
				RedirectOrClose(GetMessage("Message.DataIsSaved"), BXDialogGoodbyeWindow.LayoutType.Success, 0);
				return;
			}


			var fieldInfos = hfFields.Value.Split(',');
			var tabInfos = hfTabs.Value.Split(',');
			BXParamsBag<object> field = null;
			BXParamsBag<object> tab = null;
			BXParamsBag<object> container = new BXParamsBag<object>();
			List<BXParamsBag<object>> fields = new List<BXParamsBag<object>>();
			List<BXParamsBag<object>> tabs = new List<BXParamsBag<object>>();
			int sort = 100;
			for (var i = 0; i < fieldInfos.Length; i++)
			{
				switch (i % 4)
				{
					case 0:
						field = new BXParamsBag<object>();
						field["id"] = fieldInfos[i];
						break;
					case 1:
						field["name"] = HttpUtility.HtmlDecode(fieldInfos[i]);
						break;
					case 2:
						field["groupid"] = fieldInfos[i];

						break;
					case 3:
						field["isSection"] = fieldInfos[i];
						field["sort"] = sort;
						sort += 100;
						fields.Add(field);
						break;
				}
			}

			for (var i = 0; i < tabInfos.Length; i++)
				switch (i % 2)
				{
					case 0:
						tab = new BXParamsBag<object>();
						tab["GroupId"] = tabInfos[i];
						break;
					case 1:
						tab["title"] = tabInfos[i];
						tabs.Add(tab);
						break;

				}

			BXIBlockPublicEditorStructureBuilder.SetUserConfigFromBags(fields, tabs, BXIdentity.Current.Id, iblock.Id);

			if (SetDefaultToAll.Checked)
			{
				BXIBlockPublicEditorStructureBuilder.SetDefaultIBlockConfigFromBags(fields, tabs, iblock.Id);
			}
			RedirectOrClose(GetMessage("Message.DataIsSaved"), BXDialogGoodbyeWindow.LayoutType.Success, 0);

		}
		catch (ThreadAbortException te)
		{
		}
		catch (Exception ex)
		{
			Response.StatusCode = 200;
			Response.ContentType = "text/html";
			BXDialogGoodbyeWindow goodbye = new BXDialogGoodbyeWindow(HttpUtility.HtmlEncode(ex.Message), -1, BXDialogGoodbyeWindow.LayoutType.Error);
			goodbye.ClientType = Behaviour.ClientType;
			string docStub = IsPostBack ?
				BXPageAsDialogHelper.GetClientCloseDialogDocumentStub(goodbye, Behaviour.ClientType) :
				BXPageAsDialogHelper.GetClientSuppressShowDocumentStub(goodbye, Behaviour.ClientType);
			Response.Write(docStub);
			Response.End();
		}

		
	}

	void RedirectOrClose(string message, BXDialogGoodbyeWindow.LayoutType type, int timeout)
	{
		var returnUrl = Request.QueryString["ReturnUrl"];
		if (!String.IsNullOrEmpty(returnUrl))
		{
			Redirect(returnUrl, message, type, timeout);
		}
		else
			Close(message, type, timeout);
	}


	protected void behaviour_OnPopulateData(BXPageAsDialogBaseAdapter sender, BXPageAsDialogBaseAdapter.PopulateDataEventArgs args)
	{
		//BXDialogSectionData title = args.Data.CreateSection(BXDialogSectionType.Title);
		//title.CreateItemFromString(GetMessageRaw("DialogTitle.Authorization"));
	}

	//protected override void Render(System.Web.UI.HtmlTextWriter writer)
	//{
	//    if (!Behaviour.IsRenderingStarted)
	//        Behaviour.RenderControl(writer);
	//    else
	//        base.Render(writer);
	//}
}