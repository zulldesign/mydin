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
using Bitrix.IO;
using System.Collections.Generic;
using Bitrix.DataTypes;
using Bitrix;
using System.Text;
using Bitrix.Services;
using Bitrix.Configuration;
using Bitrix.Security;
using System.Threading;
using System.IO;
using Bitrix.Services.Undo;

public partial class bitrix_dialogs_MenuEdit : BXDialogPage
{
	//protected BXPublicMenuItemCollection menu;
	string curDir;
	string curMenuType;
	string curPath;
	private string curSiteId;
	public string SiteId
	{
		get
		{
			if (curSiteId == null)
			{
				BXSite site = BXSite.GetCurrentSite(curPath, Request.Url.Host);
				curSiteId = site != null ? site.Id : string.Empty;
			}
			return curSiteId;
		}
	}

	protected string Serialize(BXPublicMenuItem m)
	{
		BXParamsBag<object> item = new BXParamsBag<object>();
		item["Title"] = m.Title;
		item["Sort"] = m.Sort;
		item["ConditionType"] = (int)m.ConditionType;
		item["Condition"] = m.Condition;
		item["ParamName"] = m.ParamName;
		item["ParamValue"] = m.ParamValue;
		item["Links"] = m.Links;
	
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(BXSerializer.Serialize(item)));
	}

	protected BXPublicMenuItem Deserialize(string value)
	{
		BXPublicMenuItem m = new BXPublicMenuItem();
		if (string.IsNullOrEmpty(value))
			return m;
		BXParamsBag<object> item = BXSerializer.Deserialize(Encoding.UTF8.GetString(Convert.FromBase64String(value))) as BXParamsBag<object>;
		if (item == null)
			return m;

        m.Title = item.Get<string>("Title");
        m.Sort = item.Get<int>("Sort");
        //m.Link = item.Get<string>("Link");
		m.ConditionType = (ConditionType)item.Get<int>("ConditionType");
		m.Condition = item.Get<string>("Condition");
		m.ParamName = item.Get<string>("ParamName");
		m.ParamValue = item.Get<string>("ParamValue");
		m.Links.AddRange(item.Get<IEnumerable<string>>("Links"));
		return m;
	}
	protected void Page_Init(object sender, EventArgs e)
	{
        ScriptManager.RegisterClientScriptInclude(this, GetType(), "jsDD", BXPath.ToVirtualAbsolutePath("~/bitrix/js/main/dd.js"));


		try
		{
			if (Request.QueryString["path"] == null)
				throw new Exception(GetMessageRaw("Exception.PathParameterIsntSpecified"));
			curPath = BXPath.ToVirtualRelativePath(Request.QueryString["path"] ?? "~");
			if (!string.Equals(BXPath.GetExtension(curPath), BXConfigurationUtility.Constants.MenuFileExt, StringComparison.OrdinalIgnoreCase))
				Close(string.Format(GetMessage("Format.GoodbyeMessage.ModificationFilesWithExtIsAllowed"), BXConfigurationUtility.Constants.MenuFileExt), BXDialogGoodbyeWindow.LayoutType.Error, -1);
			BXPath.BreakPath(curPath, ref curDir, ref curMenuType);
			curMenuType = Path.GetFileNameWithoutExtension(curMenuType).Trim();
			if (string.IsNullOrEmpty(curMenuType))
				Close(string.Format(GetMessage("FormatGoodbyeMessage.InvalidMenuType"), curMenuType), BXDialogGoodbyeWindow.LayoutType.Error, -1);
		}
        catch (System.Threading.ThreadAbortException /*ex*/)
        {
            //...игнорируем, вызвано Close();
        }
		catch (Exception ex)
		{
			Close(GetMessage("GoodbyeMessage.AnErrorHasOccurred"), BXDialogGoodbyeWindow.LayoutType.Error, -1);
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
	}

    private BXPublicMenuItemCollection _menuItemCollection = null;

    protected BXPublicMenuItemCollection MenuItemCollection
    {
        get
        {
            if (_menuItemCollection == null)
            {
                _menuItemCollection = new BXPublicMenuItemCollection();
                _menuItemCollection.ID = "menuItems";
            }

            return _menuItemCollection;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        BXPublicMenuItemCollection menuItemCollection = MenuItemCollection;
        if (menuItemCollection.Count > 0)
            menuItemCollection.Clear();
        if (!IsPostBack)
            menuItemCollection.AddRange(BXPublicMenu.Menu.Load(curDir, curMenuType));
        else
        {
            string idsData = (Request.Form["ids[]"] ?? string.Empty).Trim();
            int[] ids =
                string.IsNullOrEmpty(idsData)
                ? new int[0]
                : Array.ConvertAll<string, int>(
                    idsData.Split(','),
                    delegate(string input)
                    {
                        return int.Parse(input);
                    }
                );

            for (int i = 0; i < ids.Length; i++)
            {
                int id = ids[i];
                BXPublicMenuItem item = Deserialize(Request.Form["additional_params_" + id]);
                item.Title = Request.Form["text_" + id];
                item.Link = Request.Form["link_" + id];
                item.Sort = i * 10 + 10;
                menuItemCollection.Add(item);
            }
        }
    }

	protected void Page_LoadComplete(object sender, EventArgs e)
	{

		DescriptionIconClass = "bx-edit-menu";

		DescriptionParagraphs.Add(string.Format(
			"<b>{0}</b>",
			GetMessage("TITLE_EDIT")
		));

		DescriptionParagraphs.Add(GetMessageFormat(
			"DESCRIPTION_EDIT",
			BXPublicMenu.GetMenuTitle(BXSite.Current.Id, curMenuType),
			curDir + '/'
		));

		DescriptionParagraphs.Add(string.Format(
			"<a href=\"{0}\">{1}</a>",
			Encode(string.Format(
				"{0}?path={1}{2}",
				BXPath.ToVirtualAbsolutePath("~/bitrix/admin/MenuEdit.aspx"),
				UrlEncode(curPath),
				Request.QueryString[BXConfigurationUtility.Constants.BackUrl] == null
					? string.Empty
					: string.Format("&{0}={1}", BXConfigurationUtility.Constants.BackUrl, UrlEncode(Request.QueryString[BXConfigurationUtility.Constants.BackUrl]))
			)),
			GetMessage("OLD_STYLE")
		));

        Behaviour.Settings.MinWidth = 400;
        Behaviour.Settings.MinHeight = 250;
        Behaviour.Settings.Width = 720;
        Behaviour.Settings.Height = 430;
        Behaviour.Settings.Resizeable = true;
	}
	protected void Behaviour_Save(object sender, EventArgs e)
	{
		try
		{
			bool canOperate = 
				string.IsNullOrEmpty(SiteId) 
				? BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit)
				: BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", SiteId);

			if (!canOperate)
				BXSecureIO.DemandWrite(BXPath.Combine(curDir, curMenuType + ".menu"));

			BXUndoMenuModificationOperation undoOperation = new BXUndoMenuModificationOperation();
			undoOperation.SiteId = SiteId;
			undoOperation.DirectoryVirtualPath = VirtualPathUtility.AppendTrailingSlash(curDir);
			undoOperation.MenuTypeId = curMenuType;
			undoOperation.SetMenuItems(BXPublicMenu.Menu.Load(curDir, curMenuType));

			BXUndoInfo undo = new BXUndoInfo();
			undo.Operation = undoOperation;
			undo.Save();

            BXPublicMenu.Menu.Save(curDir, curMenuType, MenuItemCollection);

			BXDialogGoodbyeWindow goodbye = new BXDialogGoodbyeWindow(string.Format(
				GetMessageRaw("MenuIsSuccessfullyModified"), 
				string.Concat(undo.GetClientScript(), " return false;"), 
				"#"), -1, BXDialogGoodbyeWindow.LayoutType.Success);
			BXDialogGoodbyeWindow.SetCurrent(goodbye);

			Refresh(string.Empty, BXDialogGoodbyeWindow.LayoutType.Success, -1);
		}
        catch (System.Threading.ThreadAbortException /*ex*/)
        {
            //...игнорируем, вызвано Reload();
        }
		catch (Exception ex)
		{
			ShowError(ex.Message);
		}
	}

    /// <summary>
    /// CheckUserAuthorization
    /// Проверить авторизацию пользователя для работы с диалогом
    /// </summary>
    /// <returns></returns>
    protected override bool CheckUserAuthorization()
    {
        return 
			(string.IsNullOrEmpty(SiteId) 
			? BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit) 
			: BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", SiteId))
			| BXSecureIO.CheckWrite(curPath); 
    }

    protected override string GetParametersBagName()
    {
        return "BitrixDialogMenuEditParamsBag";
    }

    protected override void ExternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        BXPublicMenuItemCollection menuItemCollection = MenuItemCollection;
        int menuItemsCount = menuItemCollection.Count;
        if (menuItemsCount == 0)
            return;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < menuItemsCount; i++)
        {
            if (sb.Length > 0)
                sb.Append(';');
            sb.Append(Serialize(menuItemCollection[i]));
        }
        paramsBag.Add(menuItemCollection.ID, sb.ToString());
    }

    protected override void InternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        BXPublicMenuItemCollection menuItemCollection = MenuItemCollection;
        if (!paramsBag.ContainsKey(menuItemCollection.ID))
            return;

        menuItemCollection.Clear();

        string menuItemCollectionAsStr = paramsBag[menuItemCollection.ID];
        if (string.IsNullOrEmpty(menuItemCollectionAsStr))
            return;

        string[] menuItemArr = menuItemCollectionAsStr.Split(';');
        int menuItemsCount = menuItemArr.Length;
        for (int i = 0; i < menuItemsCount; i++)
            menuItemCollection.Add(Deserialize(menuItemArr[i]));
    }
}
