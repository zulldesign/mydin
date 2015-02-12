<%@ WebHandler Language="C#" Class="BitrixAdminUserOptionsHandler" %>

using System;
using System.Web;
using Bitrix.Services;
using System.Collections.Generic;
using System.Collections.Specialized;
using Bitrix.Configuration;

public class BitrixAdminUserOptionsHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{

    private HttpContext mContext = null;
    private HttpContext Context
    {
        get { return mContext ?? (mContext = HttpContext.Current); }
        set { mContext = value; }
    }
	private HttpRequest mRequest = null;
    private HttpRequest Request
    {
        get { return mRequest ?? (mRequest = Context != null ? Context.Request : null); }
    }
	private HttpResponse mResponse = null;
    private HttpResponse Response
    {        
		get { return mResponse ?? (mResponse = Context != null ? Context.Response : null); }			
    }   
    
    private void SetResponseStatus(int aCode, string aDescription)
    {
		if(Response == null)
			throw new InvalidOperationException("Could not find Response");
		
        Response.StatusCode = aCode;
        Response.StatusDescription = aDescription;                   
    }

    private void SetResponseNoChache() 
    {
		if(Response == null)
			throw new InvalidOperationException("Could not find Response");
        
        Response.Expires = 0;
        Response.Cache.SetNoStore();
        Response.AppendHeader("Pragma", "no-cache");            
    }
         
    public void ProcessRequest (HttpContext context) 
    {        
		if(context == null)
			throw new ArgumentNullException("context");
		
        Context = context;
		    
        try
        {
            HttpRequest request = Request;           
            
            if (!Context.User.Identity.IsAuthenticated)
                SetResponseStatus(401, "Unauthorized");
            else
            {
                Bitrix.Security.BXCsrfToken.ValidateTokenFromRequest();
				if (request["action"] == "delete" && !String.IsNullOrEmpty(request["c"]) && !String.IsNullOrEmpty(request["n"]))
                    BXProfileManager.DeleteOption(request["c"], request["n"], (request["common"] == "Y"));
                else 
                {

                    List<UserOption> loadedUserOptionList = new List<UserOption>();
                    UserOption curUserOption = null;
                    int i = 0;
                    while (UserOption.TryLoad(request.QueryString, i, out curUserOption))
                    {
                        loadedUserOptionList.Add(curUserOption);
                        i++;
                    }

                    foreach (UserOption loadedUserOption in loadedUserOptionList)
                    {
                        BXProfileValue pv = BXProfileManager.GetOption(loadedUserOption.Context, loadedUserOption.Name, new BXProfileValue());
                        
                        foreach (KeyValuePair<string, string> loadedUserOptionParam in loadedUserOption.Parameters)
                            pv[loadedUserOptionParam.Key] = loadedUserOptionParam.Value;

                        BXProfileManager.SetOption(loadedUserOption.Context, loadedUserOption.Name, false, pv);  
                    }                    
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
 
    
    public bool IsReusable 
	{
        get { return false; }
    }
}

internal class UserOption
{
    public UserOption(string context, string name)
    {
        if (string.IsNullOrEmpty(context))
            throw new ArgumentException("Is not specified!", "context");
        //---
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Is not specified!", "name");
        //--- 

        mContext = context;
        mName = name;        
    }
    
    private string mContext = string.Empty;

    public string Context
    {
        get { return mContext; }
        set { mContext = value; }
    }

    private string mName = string.Empty;
    
    public string Name
    {
        get { return mName; }
        set { mName = value; }
    }

    private Dictionary<string, string> mParameters = null;

    public void AddParameter(string name, string value)
    {
        if (mParameters == null)
            mParameters = new Dictionary<string, string>();

        mParameters.Add(name, value);
    }


    public string GetParameter(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Is not specified!", "name");
        //---
        if (mParameters == null)
            return null;
        //---
        return mParameters[name];
    }

    public IDictionary<string, string> Parameters
    {
        get { return mParameters; } 
    }
    
    
    public static bool TryLoad(NameValueCollection srcCollection, int srcIndex, out UserOption dstItem)
    {
        if (srcCollection == null)
            throw new ArgumentNullException("srcCollection");
        //---

        string c = null, n = null;

        if (string.IsNullOrEmpty(c = srcCollection[string.Format("p[{0}][c]", srcIndex)]) || string.IsNullOrEmpty(n = srcCollection[string.Format("p[{0}][n]", srcIndex)]))
        {
            dstItem = null;
            return false;
        }

        dstItem = new UserOption(c, n);

        string[] keyArr = srcCollection.AllKeys;
        int keyCount = keyArr != null ? keyArr.Length : 0;
        for (int k = 0; k < keyCount; k++)
        { 
            string key = keyArr[k];
            if (string.IsNullOrEmpty(key))
                continue;
            //-
            string startOfKey = string.Format("p[{0}][v][", srcIndex);
            if (key.StartsWith(startOfKey, StringComparison.OrdinalIgnoreCase) && key[key.Length - 1] == ']' && key.Length > startOfKey.Length + 1)
            {
                string paramName = key.Substring(startOfKey.Length, key.Length - startOfKey.Length - 1); 
                string paramValue = srcCollection[key];
                dstItem.AddParameter(paramName, paramValue);
            }
        }    
        return true;       
    }  
}