<%@ WebHandler Language="C#" Class="StartMenuHandler" %>

using System;
using System.Web;
using Bitrix.Modules;
using System.Collections.Generic;
using Bitrix.Main;
using Bitrix.Services.Js;
using Bitrix.Security;

public class StartMenuHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState {
    
    public void ProcessRequest (HttpContext context) {
        if (context == null)
            throw new ArgumentNullException("context");
        //---
        HttpRequest request = context.Request;
        if (request == null)
            throw new InvalidOperationException("Could not find context request!");
        
        HttpResponse response = context.Response;
        if (response == null)
            throw new InvalidOperationException("Could not find context response!");
        //---
        response.ContentType = "text/html";
        //Ожидается, что определена в web.config <globalization responseEncoding="">
        //response.ContentEncoding = Encoding.UTF8; 

		if(!(HttpContext.Current.User.Identity.IsAuthenticated 
			&& BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.AccessAdminSystem)
			&& BXCsrfToken.CheckTokenFromRequest(request.Params)))
        {
			response.Write("menuItems={\"styles\":[], \"items\":[]}<script>window.location.href = window.location.href;</script>");
            response.StatusCode = 200;
            response.StatusDescription = "OK";
			
			response.Expires = 0;
			response.Cache.SetNoStore();
			response.AppendHeader("Pragma", "no-cache"); 
			return; 	
        }
			
        string moduleId = HttpUtility.UrlDecode(request.QueryString["admin_mnu_module_id"]);
        string itemId = HttpUtility.UrlDecode(request.QueryString["admin_mnu_menu_id"]);

        if (string.IsNullOrEmpty(moduleId) || string.IsNullOrEmpty(itemId))
        {
            //запрос всех эл-тов
            response.StatusCode = 200;
            response.StatusDescription = "OK";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("menuItems={");
            try
            {
                BXAdminMenuItemCollection menuItemCol = BXAdminMenuManager.GetModulesMenu(new string[0]);
                if (menuItemCol != null && menuItemCol.Count > 0)
                {
                    List<BXAdminMenuItemProxy> menuItemProxyLst = new List<BXAdminMenuItemProxy>();
                    PrepareMenuItemFormCollection(menuItemCol, BxAdminMenuItemType.Content, menuItemProxyLst);
                    PrepareMenuItemFormCollection(menuItemCol, BxAdminMenuItemType.Services, menuItemProxyLst);
					PrepareMenuItemFormCollection(menuItemCol, BxAdminMenuItemType.Store, menuItemProxyLst);
                    PrepareMenuItemFormCollection(menuItemCol, BxAdminMenuItemType.Settings, menuItemProxyLst);
                    if (menuItemProxyLst.Count > 0)
                    {
                        sb.Append("\"items\":");
                        System.Web.Script.Serialization.JavaScriptSerializer json = CreateSerialier();
                        json.Serialize(menuItemProxyLst.ToArray(), sb);
                    }
                    else
                        sb.Append(GetNoItemsStub());
                }
                else
                    sb.Append(GetNoItemsStub());
            }
            catch
            {
                sb.Append(GetNoItemsStub());
            }
            sb.Append("}");
            response.Write(sb.ToString());
        }
        else if (!string.IsNullOrEmpty(moduleId) && !string.IsNullOrEmpty(itemId))
        {
            //запрос подменю
            response.StatusCode = 200;
            response.StatusDescription = "OK";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("menuItems={");
            try
            {
                BXAdminMenuItemCollection menuItemCol = BXAdminMenuManager.GetModuleSubMenu(moduleId, itemId);
                if (menuItemCol != null && menuItemCol.Count > 0)
                {
                    sb.Append("\"items\":");
                    MenuItemCollection2Response(menuItemCol, sb);
                }
                else
                    sb.Append(GetNoItemsStub());
            }
            catch 
            {
                sb.Append(GetNoItemsStub());
            }
            sb.Append("}");
            response.Write(sb.ToString());
        }
        else
        {
            response.StatusCode = 404;
            response.StatusDescription = "Not Found";
        }

        response.Expires = 0;
        response.Cache.SetNoStore();
        response.AppendHeader("Pragma", "no-cache");        
    }

	private string GetNoItemsStub()
	{
        return string.Format("\"items\":[{{TEXT:\"{0}\"}}]", BXJSUtility.Encode(BXMain.GetModuleMessage("Kernel.StartMenu.Stub.NoData", false))); 
	}
	
    private void PrepareMenuItemFormCollection(BXAdminMenuItemCollection scrCol, BxAdminMenuItemType type, IList<BXAdminMenuItemProxy> dstList)
    {
        if (scrCol == null)
            throw new ArgumentNullException("scrCol");
        //---
        int menuItemProxyCount = -1;
        BXAdminMenuItemProxy[] menuItemProxyArr = null;
        BXAdminMenuItem menuItem = null;

        //if ((menuItem = BXAdminMenuHelper.GetItemByType(type, scrCol)) == null)
        //    throw new InvalidOperationException(string.Format("Could not find '{0}'!", Enum.GetName(typeof(BxAdminMenuItemType), type)));
        if ((menuItem = BXAdminMenuHelper.GetItemByType(type, scrCol)) == null)
            return;
        //---  
        if ((menuItemProxyCount = (menuItemProxyArr = BXAdminMenuItemProxyHelper.PrepareItems(menuItem)) != null ? menuItemProxyArr.Length : 0) > 0)
            for (int i = 0; i < menuItemProxyCount; i++)
                dstList.Add(menuItemProxyArr[i]);
    }


    ///TODO: StartMenuHandler.ashx::srJsonMaxJsonLength, StartMenuHandler.ashx::srJsonRecursionLimit в web.config?
    private static readonly int srJsonMaxJsonLength = 65536;
    private static readonly int srJsonRecursionLimit = 1024;
    
    private void MenuItem2Response(BXAdminMenuItem menuItem, System.Text.StringBuilder sb)
    {
        if (menuItem == null)
            throw new ArgumentNullException("menuItem");
        //---
        if(sb == null)
            throw new ArgumentNullException("sb");
        //---

        BXAdminMenuItemProxy[] resultArr = BXAdminMenuItemProxyHelper.PrepareItems(menuItem);
        if (resultArr != null)
        {
            System.Web.Script.Serialization.JavaScriptSerializer json = CreateSerialier();
            json.Serialize(resultArr, sb);
        }           
    }

    private void MenuItemCollection2Response(BXAdminMenuItemCollection menuItemCol, System.Text.StringBuilder sb)
    {
        if (menuItemCol == null)
            throw new ArgumentNullException("menuItemCol");
        //---
        if (sb == null)
            throw new ArgumentNullException("sb");
        //---

        BXAdminMenuItemProxy[] resultArr = BXAdminMenuItemProxyHelper.PrepareItemArray(menuItemCol);
        if (resultArr != null)
        {
            System.Web.Script.Serialization.JavaScriptSerializer json = CreateSerialier();
            json.Serialize(resultArr, sb);
        }
    }

    private System.Web.Script.Serialization.JavaScriptSerializer CreateSerialier()
    {
        System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
        json.MaxJsonLength = srJsonMaxJsonLength;
        json.RecursionLimit = srJsonRecursionLimit;
        json.RegisterConverters(new System.Web.Script.Serialization.JavaScriptConverter[] { new BXAdminMenuItemProxyConverter() });
        return json;         
    }
    
    public bool IsReusable {
        get {
            return false;
        }
    }
}