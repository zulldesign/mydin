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
using Bitrix.DataTypes;
using Bitrix;
using Bitrix.Components;
using Bitrix.IO;
using System.Collections.Generic;
using Bitrix.Configuration;
using Bitrix.Security;
using System.Text.RegularExpressions;

public partial class bitrix_dialogs_PageSettings : Bitrix.UI.BXDialogPage
{
    private Bitrix.DataTypes.BXParamsBag<string> _keywordsTypes = null;
    private Dictionary<string, Dictionary<string, string>> _keywords = null;

    private bool mIsPage = false;
    public bool IsPage
    {
        get { return mIsPage; }
        set { mIsPage = value; }
    }

    private string mCurPath = string.Empty;
    public string CurPath
    {
        get { return mCurPath; }
        set { mCurPath = value; }
    }

    private string mCurQuery = string.Empty;
    public string CurQuery
    {
        get { return mCurQuery; }
        set { mCurQuery = value; }
    }

    private string mCurDir = null;
    public string CurDir
    {
        get { return mCurDir; }
    }

    private string mCurFile = string.Empty;
    public string CurFile
    {
        get { return mCurFile; }
    }

    public string CurSiteId
    {
        get 
        {

            BXSite site = BXSite.GetCurrentSite(CurDir, Request.Url.Host);
            return site == null ? null : site.Id;
        }
    }

    private string mCurrentMarkup;

    public string CurrentMarkup
    {
        get { return mCurrentMarkup; }
        set { mCurrentMarkup = value; }
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        try
        {
            string userPath = !string.IsNullOrEmpty(Request["path"]) ? Request["path"] : "~/";
            if (!VirtualPathUtility.IsAppRelative(userPath))
            {
                Close(string.Format(GetMessage("ERROR_FORMATTED_PATH_IS_NOT_APP_RELATIVE"), userPath), BXDialogGoodbyeWindow.LayoutType.Error, -1);
                return;
            }

            CurPath = userPath;
            mCurDir = VirtualPathUtility.GetDirectory(CurPath);
            mCurFile = VirtualPathUtility.GetFileName(CurPath);

            string userQuery = Request.QueryString["query"];
            if (!string.IsNullOrEmpty(userQuery))
                CurQuery = userQuery;

            BXSite site = BXSite.GetCurrentSite(CurPath, Request.Url.Host);
            if (site == null)
            {
                Close(string.Format("{0} '{1}'!", GetMessage("SITE_IS_NOT_FOUND"), CurPath), BXDialogGoodbyeWindow.LayoutType.Error, -1);
                return;
            }

            if (BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
            {
                CurrentMarkup = BXSecureIO.FileReadAllText(CurPath, BXConfigurationUtility.DefaultEncoding);
            }

            mClientPageTitle = !IsPostBack ? !string.IsNullOrEmpty(CurrentMarkup) ? BXParser.ParsePageTitle(CurrentMarkup) : string.Empty : Request.Form["pageTitle"];
            if (string.IsNullOrEmpty(mClientPageTitle))
                mClientPageTitle = GetMessage("UNTITLED_PAGE");

            _keywords = new Dictionary<string, Dictionary<string, string>>();
            _keywordsTypes = BXPageManager.GetKeywords(site.Id);
            foreach (string key in _keywordsTypes.Keys)
            {
                Dictionary<string, string> paramDic = new Dictionary<string, string>();
                paramDic.Add("caption", _keywordsTypes[key]);
                //paramDic.Add("value", GetMessage(string.Format("DefaultKeyword_{0}", key)));
                _keywords.Add(key, paramDic);
            }


            BXSectionInfo curSectionInfo = BXSectionInfo.GetCumulativeSection(mCurDir);
            foreach (KeyValuePair<string, string> curSectionKeyword in curSectionInfo.Keywords)
            {
                if (!_keywordsTypes.ContainsKey(curSectionKeyword.Key))
                    continue;

                Dictionary<string, string> paramDic = null;
                if (!_keywords.TryGetValue(curSectionKeyword.Key, out paramDic))
                    continue;

                paramDic.Add("defaultValue", curSectionKeyword.Value);
            }

            if (BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
            {
                BXParamsBag<string> keywordBag = BXParser.ParsePageKeywords(mCurrentMarkup);
                if (keywordBag != null)
                    foreach (KeyValuePair<string, string> keyword in keywordBag)
                    {
                        if (!_keywordsTypes.ContainsKey(keyword.Key))
                            continue;
                        Dictionary<string, string> paramDic = null;
                        if (!_keywords.TryGetValue(keyword.Key, out paramDic))
                            continue;

                        paramDic.Add("value", keyword.Value);
                        //if (!paramDic.ContainsKey("value"))
                        //    paramDic.Add("value", keyword.Value);
                        //else
                        //    paramDic["value"] = keyword.Value;
                    }
            }
            LoadKeywords();

            Behaviour.Settings.MinWidth = 400;
            Behaviour.Settings.MinHeight = 250;
            Behaviour.Settings.Width = 600;
            Behaviour.Settings.Height = 320;
            Behaviour.Settings.Resizeable = true;

            string query = CurQuery;
            if (!string.IsNullOrEmpty(query))
                query = BXQueryString.RemoveParameter(BXConfigurationUtility.Constants.BackUrl, query);

            string backUrl = !string.IsNullOrEmpty(query) ? string.Concat(CurPath, "?", query) : CurPath;

            DescriptionIconClass = "bx-property-page";
            DescriptionParagraphs.Add(string.Format("{0} <b>{1}</b>", GetMessage("DESCRIPTION_HEADER"), CurPath));
            DescriptionParagraphs.Add(
                string.Format(
                    "<a href=\"{0}?path={1}&selectedTabIndex=1&{2}={3}\" >{4}</a>", 
                    VirtualPathUtility.ToAbsolute("~/bitrix/admin/FileManEdit.aspx"), 
                    HttpUtility.UrlEncode(mCurPath), 
                    BXConfigurationUtility.Constants.BackUrl,
                    HttpUtility.UrlEncode(backUrl),
                    GetMessage("GOTO_CONTROL_PANEL")
                    )
                );
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

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        tblPageProp.DataSource = _keywordsTypes;
        tblPageProp.DataBind();
    }

    protected void Behaviour_Save(object sender, EventArgs e)
    {
        try
        {
            if (!BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
            {
               Close(GetMessage("INSUFFICIENT_RIGHTS"), BXDialogGoodbyeWindow.LayoutType.Error, -1);
                return;
            }

            string pageMarkup = CurrentMarkup;

            Bitrix.DataTypes.BXParamsBag<string> keywordBag = new Bitrix.DataTypes.BXParamsBag<string>();
            foreach (string keywordKey in _keywords.Keys)
            {
                if (!IsKeywordValueChanged(keywordKey))
                    continue;
                string keywordValue = string.Empty;
                _keywords[keywordKey].TryGetValue("value", out keywordValue);
                keywordBag.Add(keywordKey, keywordValue);
            }

            pageMarkup = BXParser.PersistPageKeywords(pageMarkup, keywordBag);
            pageMarkup = BXParser.PersistPageTitle(pageMarkup, mClientPageTitle);

            string savePath = BXPath.Combine(CurDir, CurFile);
            int i = 0;
            while (BXSecureIO.FileExists(savePath + "." + i))
                i++;
            string tempFile = savePath + "." + i;

            BXSecureIO.DemandWrite(tempFile);
            BXSecureIO.DemandWrite(savePath);
                     
            if (BXSecureIO.FileExists(savePath))
                BXSecureIO.FileMove(savePath, tempFile);

            BXSecureIO.SaveAspx(savePath, pageMarkup, null, BXConfigurationUtility.DefaultEncoding);

            if (BXSecureIO.FileExists(tempFile))
                BXSecureIO.FileDelete(tempFile);
               
            Refresh(GetMessage("OPERATION_COMPLETED_SUCCESSFULLY"), BXDialogGoodbyeWindow.LayoutType.Success, -1);
        }
        catch (System.Threading.ThreadAbortException /*exception*/)
        {
            //...на Reload()->HttpResponse.End();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    //protected void KeywordsTable_ItemDataBound(object sender, RepeaterItemEventArgs e)
    //{
    //    if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
    //        return;
    //    e.Item.ID = "k" + keywordsControls.Count;
    //    keywordsControls.Add(((KeyValuePair<string, string>)e.Item.DataItem).Key, (TextBox)e.Item.FindControl("Value"));
    //}

    private string mClientPageTitle = string.Empty;

    protected string GetClientPageTitle(bool encode) 
    {
        if (!encode) return mClientPageTitle;
        return HttpUtility.HtmlEncode(mClientPageTitle);
    }

    protected override string GetParametersBagName()
    {
        return "BitrixDialogPageSettingsParamsBag";
    }

    protected override void ExternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        paramsBag.Add("pageTitle", mClientPageTitle ?? string.Empty);

        LoadKeywords();
        foreach (KeyValuePair<string, Dictionary<string, string>> keywordPair in _keywords)
        {
            string keywordValue;
            if (!keywordPair.Value.TryGetValue("value", out keywordValue))
                continue;
            paramsBag.Add(string.Format("keyword[{0}]", keywordPair.Key), keywordValue);
        }
    }

    protected override void InternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        if (paramsBag.ContainsKey("pageTitle"))
        {
            string text = paramsBag["pageTitle"];
            if(!string.IsNullOrEmpty(text))
                mClientPageTitle = text;
        }

        foreach (KeyValuePair<string, Dictionary<string, string>> keywordPair in _keywords)
        {
            string paramKey = string.Format("keyword[{0}]", keywordPair.Key);
            if (!paramsBag.ContainsKey(paramKey))
                continue;

            string keywordValue = paramsBag[paramKey];
            if (keywordPair.Value.ContainsKey("value"))
                keywordPair.Value["value"] = keywordValue;
            else
                keywordPair.Value.Add("value", keywordValue);
        }
    }

    private static readonly Regex srxKeyWordCode = new Regex(@"^PROPERTY\[(?<index>\d+)\]\[CODE\]$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    protected bool IsKeywordValueChanged(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Is not defined!", "key");

        if (_keywords == null)
            throw new InvalidOperationException("Keywords is not assigned!");

        Dictionary<string, string> keywordDic = null;
        if (!_keywords.TryGetValue(key, out keywordDic))
            return false;
        string defaultValue = string.Empty, currentValue = string.Empty;

        if (!keywordDic.TryGetValue("defaultValue", out defaultValue))
            return true; //значение по умолчанию не было задано - формат представления "переопределённое"

        if (!keywordDic.TryGetValue("value", out currentValue))
            return false; //значение по умолчанию задано и не переопределялось - формат представления "по умолчанию"

        return string.Compare(defaultValue, currentValue, StringComparison.CurrentCulture) != 0;
    }

    private void LoadKeywords()
    {
        if (!IsPostBack)
            return;

        if (_keywords == null)
            throw new InvalidOperationException("Keywords are not initialized!");

        Match m = null;
        foreach (string key in Request.Form.Keys)
        {
            if (!(m = srxKeyWordCode.Match(key)).Success)
                continue;

            int index = Convert.ToInt32(m.Groups["index"].Value);
            string value = Request.Form[string.Format("PROPERTY[{0}][VALUE]", index)];
            //...даём возможность затереть переопределённое значение
            //if (string.IsNullOrEmpty(value)) 
            //    continue; 
            //---
            string code = Request.Form[key];
            Dictionary<string, string> keywordDic = null;
            if (!_keywords.TryGetValue(code, out keywordDic))
                continue;

            if (string.IsNullOrEmpty(value))
            {
                if (keywordDic.ContainsKey("value"))
                    keywordDic.Remove("value"); //...затирание переопределённого значения
            }
            else
            {
                //...сохранение переопределённого значения
                if (keywordDic.ContainsKey("value"))
                    keywordDic["value"] = value;
                else
                    keywordDic.Add("value", value);
            }
        }
    }
    protected string GetKeywordValue(string key, bool encode)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Is not defined!", "key");

        if (_keywords == null)
            throw new InvalidOperationException("Keywords is not assigned!");

        Dictionary<string, string> keywordDic = null;
        if (!_keywords.TryGetValue(key, out keywordDic))
            return string.Empty;

        string result = null;
        if (!keywordDic.TryGetValue("value", out result) && !keywordDic.TryGetValue("defaultValue", out result))
            result = string.Empty;

        return (encode && !string.IsNullOrEmpty(result)) ? HttpUtility.HtmlEncode(result) : result;
    }
}
