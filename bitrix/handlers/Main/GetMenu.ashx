<%@ WebHandler Language="C#" Class="BitrixAdminGetMenuHandler"  %>

using System;
using System.Web;
using System.Web.UI;
using System.Text;
using Bitrix.Modules;
using System.Collections.Generic;
using Bitrix.Services;
using Bitrix.Configuration;

public class BitrixAdminGetMenuHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{

    private HttpContext mContext = null;
    private HttpContext Context
    {
        get
        {
            if (mContext == null)
                throw new InvalidOperationException("Could not find context!");
            //---
            return mContext;
        }
        set
        {
            mContext = value;
        }
    }
    private HttpRequest Request
    {
        get
        {
            HttpRequest request = Context.Request;
            if (request == null)
                throw new InvalidOperationException("Could not find request!");
            //--- 
            return request;
        }
    }
    private HttpResponse Response
    {
        get
        {
            HttpResponse response = Context.Response;
            if (response == null)
                throw new InvalidOperationException("Could not find response!");
            //--- 
            return response;
        }
    }

    private void SetResponseStatus(int aCode, string aDescription)
    {
        HttpResponse response = Response;
        response.StatusCode = aCode;
        response.StatusDescription = aDescription;
    }

    private void SetResponseNoChache()
    {
        HttpResponse response = Response;

        response.Expires = 0;
        response.Cache.SetNoStore();
        response.AppendHeader("Pragma", "no-cache");
    }    
    
    private List<string> _activeSections;
    private List<string> _openSections;
    private string _activeMenuKey;
    
    private bool ShowSubMenu(BXAdminMenuItem item, string itemId, int level)
    {
        HttpResponse response = Response;
        if (item.Children.Count > 0)
        {
            if (item.ItemId == itemId)
            {
                StringBuilder menuString = new StringBuilder();
                foreach (BXAdminMenuItem subItem in item.Children)
                    AddSubMenuItems(subItem, menuString, level);
                response.Write(menuString.ToString());

                return true;
            }

            foreach (BXAdminMenuItem subItem in item.Children)
            {
                if (ShowSubMenu(subItem, itemId, level + 1))
                    return true;
            }
        }
        return false;
    }

    private void AddSubMenuItems(BXAdminMenuItem menuItem, StringBuilder menuString, int level)
    {
        bool haveSubMenu = (menuItem.Children.Count > 0);
        bool isSectionActive = this._activeSections.Contains(menuItem.ItemId) || this._openSections.Contains(menuItem.ItemId);
        //false;//((this.ActiveSections.Count > level + 1) && (this.ActiveSections[level + 1] != null) && (this.ActiveSections[level + 1] == menuItem.ItemId));

        menuString.Append("<div class=\"menuline\"><table cellspacing=\"0\"><tr>");
        for (int i = 0; i < level; i++)
            menuString.Append("<td><div class=\"menuindent\"></div></td>");

        if (menuItem.Dynamic && !isSectionActive)
        {
            menuString.Append(
                string.Format(
                    "<td><div class=\"sign signplus\" onclick=\"JsAdminMenu.ToggleDynSection(this, '{0}', '{1}', '{2}')\"></div></td>",
                    menuItem.ModuleId,
                    menuItem.ItemId,
                    level + 1
                )
            );
        }
        else
        {
            menuString.Append(
                string.Format(
                    "<td><div class=\"sign {0}\" {1}></div></td>",
                    (haveSubMenu ? (isSectionActive ? "signminus" : "signplus") : "signdot"),
                    (haveSubMenu ? string.Format("onclick=\"JsAdminMenu.ToggleSection(this, '{0}', {1})\"", menuItem.ItemId, (level + 1)) : "")
                )
            );
        }

        //menuString.Append(
        //    string.Format(
        //        "<td class=\"menuicon\">{0}</td>",
        //        ((!String.IsNullOrEmpty(menuItem.Icon) && menuItem.Url != null) ? String.Format("<a href=\"{0}\" title=\"{1}\" id=\"{2}\"></a>", this.Request.ApplicationPath + menuItem.Url, menuItem.Hint, menuItem.ItemId) : "")
        //    )
        //);

        menuString.Append(
            string.Format(
                "<td class=\"menuicon\">{0}</td>",
                (!String.IsNullOrEmpty(menuItem.Icon)) ? String.Format("<a id=\"{0}\"></a>", menuItem.Icon) : ""
            )
        );

        if (menuItem.Url != null)
            menuString.Append(
                string.Format(
                    "<td class=\"menutext\"><a href=\"{0}\" title=\"{1}\"{2}>{3}</a></td></tr></table>",
                    ((this.Request.ApplicationPath.Length > 0 && !this.Request.ApplicationPath.Equals("/", StringComparison.InvariantCultureIgnoreCase)) ? this.Request.ApplicationPath : "") + menuItem.Url,
                    menuItem.Hint,
                    (menuItem.ItemId == this._activeMenuKey ? " class=\"active\"" : ""), //"",//(menuItem.ItemId == this.ActiveMenuKey ? " class=\"active\"" : ""),
                    menuItem.Title
                )
            );
        else
            menuString.Append(
                string.Format(
                    "<td class=\"menutext menutext-no-url\">{0}</td></tr></table>",
                    menuItem.Title
                )
            );

        if (haveSubMenu || menuItem.Dynamic)
        {
            menuString.Append(
                string.Format(
                    "<div id=\"{0}\" style=\"display:{1};\">",
                    menuItem.ItemId,
                    (isSectionActive ? "block" : "none")
                )
            );

            if (!menuItem.Dynamic || haveSubMenu)
                foreach (BXAdminMenuItem subItem in menuItem.Children)
                    AddSubMenuItems(subItem, menuString, level + 1);

            menuString.Append("</div>");
        }

        menuString.Append("</div>");
    }
              
    public void ProcessRequest (HttpContext context) {

        Context = context;
        try
        {
            HttpRequest request = Request;

            HttpResponse response = Response;
            response.ContentType = "text/html";
            //Ожидается, что определена в web.config <globalization responseEncoding="">
            //response.ContentEncoding = Encoding.UTF8; 

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                response.Write("<script>window.location.href = window.location.href;</script>");
                response.StatusCode = 200;
                response.StatusDescription = "OK";
            }
            else
            {
                string moduleId = request["admin_mnu_module_id"];
                if (moduleId == null)
                    throw new HttpException("Module identifier not found in request during delay load.");

                string itemId = request["admin_mnu_menu_id"];
                if (itemId == null)
                    throw new HttpException("Item identifier not found in request during delay load.");

                BXModule module = BXModuleManager.GetModule(moduleId);
                if (module == null)
                    throw new InvalidOperationException(string.Format("Could not find module with id = '{0}'!", moduleId));
                //---
                string[] arModules = new string[] { module.GetType().FullName };

                List<string> arOpenSectionsTmp = new List<string>();
                arOpenSectionsTmp.Add(itemId);

                BXProfileValue pv = BXProfileManager.GetOption("admin_menu", "pos", new BXProfileValue());
                if (pv.ContainsKey("sections"))
                {
                    string[] ss = pv["sections"].Split(new char[] { ',' });
                    foreach (string s in ss)
                        arOpenSectionsTmp.Add(s);
                }

                string[] arOpenSections = arOpenSectionsTmp.ToArray();

                BXAdminMenuItemCollection menu = BXAdminMenuManager.GetModulesMenu(arModules, arOpenSections);

                BXAdminMenuManager.SearchActiveSections(menu, ref this._activeSections, ref this._activeMenuKey);
                BXAdminMenuManager.SearchOpenSections(ref this._openSections);
                foreach (BXAdminMenuItem menuItem in menu)
                {
                    if (ShowSubMenu(menuItem, itemId, 0))
                        break;
                }

                SetResponseStatus(200, "OK");
            }
        }
        catch (Exception /*exp*/)
        {
            //Запись в log  
            SetResponseStatus(500, "Internal Server Error");
        }

        SetResponseNoChache();
    }
 
    public bool IsReusable {
        get { return false; }
    }

}