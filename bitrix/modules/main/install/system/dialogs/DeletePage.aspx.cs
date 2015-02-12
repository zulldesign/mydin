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
using Bitrix.IO;
using Bitrix.Configuration;
using System.IO;
using System.Text;
using Bitrix;
using System.Collections.Generic;
using System.Web.SessionState;
using Bitrix.DataTypes;
using System.Web.Hosting;
using Bitrix.Services.Undo;

public partial class bitrix_dialogs_DeletePage : BXDialogPage
{
    /// <summary>
    /// Путь к удаляемой странице
    /// </summary>
    private string mPath = null;
    /// <summary>
    /// Путь к директории (часть Path)
    /// </summary>
    private string mDirectoryPath = null;

    /// <summary>
    /// Имя файла (часть Path)
    /// </summary>
    private string mFileName = null;
    private List<string> mPathList = null;

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        try
        {
            //if (!BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
            //    Close(GetMessage("INSUFFICIENT_RIGHTS"), BXDialogGoodbyeWindow.LayoutType.Error, -1);

            mPath = Request["path"];
            if (string.IsNullOrEmpty(mPath))
                Close(GetMessage("PATH_IS_NOT_SPECIFIED"), BXDialogGoodbyeWindow.LayoutType.Error, -1);


            bool isPathValid = false;
            try
            {
                isPathValid = VirtualPathUtility.IsAppRelative(mPath);
            }
            catch (Exception /*exc*/){ }

            if(!isPathValid)
                Close(GetMessage("PATH_IS_NOT_VALID"), BXDialogGoodbyeWindow.LayoutType.Error, -1);


            string phPath = HostingEnvironment.MapPath(mPath);
            if (Directory.Exists(phPath))
            {
                mDirectoryPath = VirtualPathUtility.AppendTrailingSlash(mPath);
                mPath = VirtualPathUtility.Combine(mDirectoryPath, BXConfigurationUtility.Constants.DefaultPage);            }
            else if (File.Exists(phPath))
            {
                mDirectoryPath = VirtualPathUtility.GetDirectory(mPath);
                //mDirectoryPath = VirtualPathUtility.AppendTrailingSlash(mDirectoryPath); //страховка
            }

            if (string.IsNullOrEmpty(mDirectoryPath))
                Close(string.Format("{0}: {1}!", GetMessage("DIRECTORY_IS_NOT_SPECIFIED_IN_PATH"), mPath));


            mFileName = VirtualPathUtility.GetFileName(mPath);
            if (string.IsNullOrEmpty(mFileName))
                Close(string.Format("{0}: {1}!", GetMessage("FILE_NAME_IS_NOT_SPECIFIED_IN_PATH"), mPath));

            DescriptionIconClass = "bx-delete-page";
            DescriptionParagraphs.Add(string.Format("{0} <b>{1}</b>?", GetMessage("DO_YOU_REALLY_WANT_DELETE_PAGE"), mPath));
            //предупреждение об удалении страницы по умолчанию
            defaultPageDeletionAlert.Visible = IsDefaultFileName;
            //удаление соотв. пунктов меню
            deleteMenuItem.Visible = true;/*IsPathPresentInMenu;*/

            //if (!defaultPageDeletionAlert.Visible && !deleteMenuItem.Visible)
            //    scriptContainer.InnerHtml = "<script type='text/javascript'>jsPopup.HideDialogContent();</script>";

            Behaviour.ButonSetLayout = BXPageAsDialogButtonSetLayout.YesNo;
            Behaviour.SetButtonText(BXPageAsDialogButtonEntry.Yes, GetMessageRaw("ButtonText.YesDelete"));
            Behaviour.SetButtonText(BXPageAsDialogButtonEntry.No, GetMessageRaw("ButtonText.DontDelete"));

            Behaviour.Settings.MinWidth = 600;
            Behaviour.Settings.Width = 600;
            Behaviour.Settings.MinHeight = 150;
            Behaviour.Settings.Height = 150;
            Behaviour.Settings.Resizeable = false;
        }
        catch (System.Threading.ThreadAbortException /*exception*/)
        {
            //...игнорируем, вызвано Close();
        }
        catch (Exception exception)
        {
            Close(exception.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
        }
    }


    protected override string GetParametersBagName()
    {
        return "BitrixDialogDeletePageParamsBag";
    }

    protected override void ExternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        paramsBag.Add(deleteMenuItemChkBx.ID, deleteMenuItemChkBx.Checked.ToString());
    }

    protected override void InternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        if (paramsBag.ContainsKey(deleteMenuItemChkBx.ID))
            deleteMenuItemChkBx.Checked = Convert.ToBoolean(paramsBag[deleteMenuItemChkBx.ID]);
    }

    protected bool IsDefaultFileName
    {
        get
        {
            if (string.IsNullOrEmpty(mFileName))
                throw new InvalidOperationException("Could not find file name!");
            return string.Compare(mFileName, BXConfigurationUtility.Constants.DefaultPage, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }

    //protected bool IsPathPresentInMenu
    //{
    //    get 
    //    {
    //        if (string.IsNullOrEmpty(mPath))
    //            throw new InvalidOperationException("Could not find path!");

    //        List<BXMenu> resultLst = new List<BXMenu>();
    //        BXMenuManager.MenuPair[] menuIdArr = BXMenuManager.GetMenuTypes(BXSite.Current.Id);
    //        int menuCount = menuIdArr != null ? menuIdArr.Length : 0;
    //        for (int i = 0; i < menuCount; i++)
    //        {
    //            BXMenuManager.MenuPair menuId = menuIdArr[i];
    //            BXMenu menu = BXMenuManager.Load(mPath, menuId.Id, 1);
    //            int menuItemCount = menu != null ? menu.Count : 0;
    //            for (int j = 0; j < menuItemCount; j++)
    //            {
    //                BXMenuItem menuItem = menu[j];

    //                string menuItemPath = menuItem.Link;

    //                menuItemPath = BXPath.ToVirtualRelativePath(menuItem.Link);
    //                if (string.Compare(menuItemPath, mPath, StringComparison.OrdinalIgnoreCase) == 0 ||
    //                    (IsDefaultFileName && 
    //                    string.Compare(menuItemPath, mDirectoryPath, StringComparison.OrdinalIgnoreCase) == 0)
    //                    ) 
    //                    return true;
    //            }
    //        }
    //        return false;
    //    }
    //}

    //private List<string> handledMenuPathList = null;

    protected void RemovePathFromMenus(string virtualDirectoryPath, string virtualUrlPath, BXSite site, BXUndoPageDeletionOperation undoOperation)
	{
		if (string.IsNullOrEmpty(this.mPath))
			throw new ArgumentException("Is not specified!", "mPath");
		//---
		if (string.IsNullOrEmpty(this.mDirectoryPath))
			throw new ArgumentException("Is not specified!", "mDirectoryPath");
		//---

		if (string.IsNullOrEmpty(virtualDirectoryPath))
			throw new ArgumentException("Is not specified!", "virtualDirectoryPath");
		//---
		

//		BXSite site = Bitrix.Services.BXSiteRemapUtility.UnmapVirtualPath(virtualDirectoryPath, out virtualDirectoryPath);// BXSite.GetCurrentSite(virtualDirectoryPath, Request.Url.Host);
		if (site == null)
			throw new InvalidOperationException(string.Format("Could not find site for path '{0}'!", virtualDirectoryPath));
		//---

		var path = Bitrix.Services.BXSiteRemapUtility.UnmapVirtualPath(this.mPath, site);
		var directoryPath = Bitrix.Services.BXSiteRemapUtility.UnmapVirtualPath(this.mDirectoryPath, site);


		Dictionary<string, string> menuTypeDic = BXPublicMenu.GetMenuTypes(site.Id);
		foreach (string key in menuTypeDic.Keys)
		{
			//BXMenuManager.MenuPair menuId = menuIdArr[i];
			string menuId = key;
			BXPublicMenuItemCollection menuItemCol = null;
			try
			{
				menuItemCol = BXPublicMenu.Menu.GetMenuByUri(menuId, virtualUrlPath);
			}
			catch (Exception /*exception*/)
			{
				menuItemCol = null;
			}

			if (menuItemCol == null || menuItemCol.Count == 0)
				continue;

			if (!string.IsNullOrEmpty(menuItemCol.MenuFilePath) &&
				mPathList != null &&
			   mPathList.FindIndex(
				   delegate(string curPath)
				   { return string.Compare(menuItemCol.MenuFilePath, curPath, StringComparison.OrdinalIgnoreCase) == 0; }
			   ) >= 0)
				continue;

			bool menuChanged = false;
			int j = 0;

			while (j < menuItemCol.Count)
			{
				BXPublicMenuItem menuItem = menuItemCol[j];
				string menuItemPath = null;

				if (!string.IsNullOrEmpty(menuItem.Link))
					try
					{
						int whatInd = menuItem.Link.IndexOf('?');
						if (whatInd < 0)
							menuItemPath = BXPath.ToVirtualRelativePath(menuItem.Link);
						else
							menuItemPath = BXPath.ToVirtualRelativePath(menuItem.Link.Substring(0, whatInd));
					}
					catch (Exception /*exception*/)
					{
						menuItemPath = null;
					}

				bool aboutDelete = false;
				if (!string.IsNullOrEmpty(menuItemPath))
				{
					if (string.Equals(mFileName, BXConfigurationUtility.Constants.DefaultPage, StringComparison.OrdinalIgnoreCase))
						aboutDelete = string.Equals(VirtualPathUtility.AppendTrailingSlash(menuItemPath), directoryPath, StringComparison.OrdinalIgnoreCase);

					if (!aboutDelete)
						aboutDelete = string.Equals(VirtualPathUtility.RemoveTrailingSlash(menuItemPath), path, StringComparison.OrdinalIgnoreCase);
				}

				if (!aboutDelete)
				{
					j++;
					continue;
				}

				if(undoOperation != null)
				{
					BXUndoPageDeletionOperation.MenuItemInfo menuItemInfo = null;
					foreach(BXUndoPageDeletionOperation.MenuItemInfo curMenuItemInfo in undoOperation.MenuItemInfoList)
					{
						if(!string.Equals(curMenuItemInfo.MenuTypeId, menuId, StringComparison.Ordinal))
							continue;

						menuItemInfo = curMenuItemInfo;
						break;
					}

					if(menuItemInfo == null)
					{
						menuItemInfo = new BXUndoPageDeletionOperation.MenuItemInfo();
						undoOperation.MenuItemInfoList.Add(menuItemInfo);
					}

					menuItemInfo.MenuTypeId = menuId;
					menuItemInfo.MenuItemName = menuItem.Title;
					menuItemInfo.MenuItemIndex = j;
					menuItemInfo.Link = menuItem.Link;
				}

				menuItemCol.RemoveAt(j);

				if (!menuChanged)
					menuChanged = true;
			}

			if (menuChanged)
			{
				BXPublicMenu.Menu.Save(virtualDirectoryPath, menuId, menuItemCol);				
			}
			if (mPathList == null)
				mPathList = new List<string>();

			if (!string.IsNullOrEmpty(menuItemCol.MenuFilePath) &&
				mPathList.FindIndex(
					delegate(string curPath)
					{ return string.Compare(menuItemCol.MenuFilePath, curPath, StringComparison.OrdinalIgnoreCase) == 0; }
				) < 0)
				mPathList.Add(menuItemCol.MenuFilePath);
		}

		foreach (var d in new DirectoryInfo(BXPath.ToPhysicalPath(virtualDirectoryPath)).GetDirectories())
		{
			///HACK: Ignoring service derectories
			string childDirectoryVirtualPath = string.Concat(virtualDirectoryPath, d.Name, "/");
			if (BXPath.StartsWithPath(childDirectoryVirtualPath, "~/app_browsers/")
				|| BXPath.StartsWithPath(childDirectoryVirtualPath, "~/app_data/")
				|| BXPath.StartsWithPath(childDirectoryVirtualPath, "~/app_themes/")
				|| BXPath.StartsWithPath(childDirectoryVirtualPath, "~/bin/")
				|| BXPath.StartsWithPath(childDirectoryVirtualPath, "~/bitrix/")
				|| BXPath.StartsWithPath(childDirectoryVirtualPath, "~/upload/")
			)
				continue;

			RemovePathFromMenus(childDirectoryVirtualPath, string.Concat(virtualUrlPath, d.Name, "/"), site, undoOperation);
		}
	}
    protected void Behaviour_Yes(object sender, EventArgs e)
    {
        try
        {
            BXUser.DemandOperations(BXRoleOperation.Operations.FileManage);

            if (string.IsNullOrEmpty(mPath))
                throw new InvalidOperationException("Could not find path!");

            string phPath = BXPath.ToPhysicalPath(mPath);
            FileInfo fi = new FileInfo(phPath);
            if (!fi.Exists)
                Close(GetMessage("PATH_IS_NOT_EXISTS"), BXDialogGoodbyeWindow.LayoutType.Error, -1);

            string ext = fi.Extension;

            if (string.Compare(ext, ".aspx", StringComparison.OrdinalIgnoreCase) != 0)
                Close(GetMessage("FILE_HAS_INVALID_EXTENSION"), BXDialogGoodbyeWindow.LayoutType.Error, -1);

            if (string.IsNullOrEmpty(mDirectoryPath))
                throw new InvalidOperationException("Could not find directory path!");

            //если удаляем страницу по умолчанию, то текущим каталогом сразу становиться родительский каталог
            //if(string.Equals(mFileName, BXConfigurationUtility.Constants.DefaultPage, StringComparison.InvariantCultureIgnoreCase))
            //    mDirectoryPath = VirtualPathUtility.GetDirectory(mDirectoryPath);
            //mDirectoryPath = VirtualPathUtility.AppendTrailingSlash(mDirectoryPath);

            //пытаемся перенаправить пользователя на страницу по умолчанию текущего каталога, если её не существует поднимаемся выше по дереву каталогов
            //если удаляем страницу по умолчанию, то текущим каталогом сразу становиться родительский каталог

            string redirectionDirPath = 
				string.Equals(mFileName, BXConfigurationUtility.Constants.DefaultPage, StringComparison.InvariantCultureIgnoreCase) 
				? VirtualPathUtility.GetDirectory(mDirectoryPath) : 
				mDirectoryPath;

            redirectionDirPath = VirtualPathUtility.AppendTrailingSlash(redirectionDirPath);
            string redirectionPath = VirtualPathUtility.Combine(redirectionDirPath, BXConfigurationUtility.Constants.DefaultPage);
            while (!BXSecureIO.FileExists(redirectionPath) && !string.Equals(redirectionDirPath, "~/", StringComparison.Ordinal)) 
            {
                redirectionDirPath = VirtualPathUtility.GetDirectory(redirectionDirPath);
                redirectionPath = VirtualPathUtility.Combine(redirectionDirPath, BXConfigurationUtility.Constants.DefaultPage);
            }

            StringBuilder sb = new StringBuilder(BXSite.GetUrlForPath(redirectionDirPath, null));
            string showModeParamValue = BXConfigurationUtility.GetShowModeParamValue(BXConfigurationUtility.GetShowMode(sb.ToString()));
            sb.Append('?');
            sb.Append(HttpUtility.UrlEncode(BXConfigurationUtility.Constants.ShowModeParamName));
            sb.Append('=');
            sb.Append(HttpUtility.UrlEncode(showModeParamValue));

			BXUndoPageDeletionOperation undoOperation = new BXUndoPageDeletionOperation();
			undoOperation.FileVirtualPath = mPath;
			undoOperation.FileEncodingName = BXConfigurationUtility.DefaultEncoding.WebName;
			undoOperation.FileContent = BXSecureIO.FileReadAllText(mPath, BXConfigurationUtility.DefaultEncoding);
			BXSite site = BXSite.GetCurrentSite(mPath, Bitrix.Services.BXSefUrlManager.CurrentUrl.Host);
			undoOperation.SiteId = site != null ? site.Id : BXSite.DefaultSite.Id;

            BXSecureIO.FileDelete(phPath);

            if (deleteMenuItemChkBx.Checked)
            {
                if (mPathList != null && mPathList.Count > 0)
                    mPathList.Clear();

				string unmapped = Bitrix.Services.BXSiteRemapUtility.UnmapVirtualPath(mPath, site);
			    RemovePathFromMenus(site.DirectoryVirtualPath, site.UrlVirtualPath, site, undoOperation);
            }

			BXUndoInfo undo = new BXUndoInfo();
			undo.Operation = undoOperation;
			undo.Save();

			BXDialogGoodbyeWindow goodbye = new BXDialogGoodbyeWindow(string.Format(
				GetMessageRaw("OPERATION_IS_COMPLETED_SUCCESSFULLY_UNDO"), 
				string.Concat(undo.GetClientScript(), " return false;"), 
				"#"), -1, BXDialogGoodbyeWindow.LayoutType.Success);
			BXDialogGoodbyeWindow.SetCurrent(goodbye);

            Redirect(sb.ToString(), string.Empty);
        }
        catch (System.Threading.ThreadAbortException /*exception*/)
        {
            //...игнорируем, вызвано Close();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }
}
