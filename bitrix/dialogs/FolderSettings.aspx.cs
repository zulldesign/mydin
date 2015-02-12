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
using System.Text.RegularExpressions;
using Bitrix;
using Bitrix.UI;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Components;
using System.Collections.Generic;
using Bitrix.Configuration;
using System.Text;
using Bitrix.DataTypes;

public partial class bitrix_dialogs_FolderSettings : Bitrix.UI.BXDialogPage
{
    private Bitrix.DataTypes.BXParamsBag<string> _keywordsTypes = null;
    private Dictionary<string, Dictionary<string, string>> _keywords = null;
    private static readonly Regex srxKeyWordCode = new Regex(@"^PROPERTY\[(?<index>\d+)\]\[CODE\]$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private bool IsAuthorized()
    {
        if (string.IsNullOrEmpty(CurrentDirPath))
            throw new InvalidOperationException("CurrentDirPath is not assigned!");

        return BXSecureIO.CheckWriteDirectory(CurrentDirPath);
        BXSecureIO.CheckView("");
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        try
        {
            string userPath = Request.QueryString["path"];

            if (string.IsNullOrEmpty(userPath))
            {
                CurrentPath = CurrentDirPath = "~/";
                //ParentDirPath = string.Empty;
            }
            else
            {
                string appRelUserPath = null;
                try
                {
                    appRelUserPath = VirtualPathUtility.ToAppRelative(userPath);
                }
                catch (Exception /*e*/) { }


                if (!string.IsNullOrEmpty(appRelUserPath))
                {
                    CurrentPath = appRelUserPath;
                    CurrentDirPath = VirtualPathUtility.GetDirectory(appRelUserPath);
                    string tmpDirPath = VirtualPathUtility.RemoveTrailingSlash(CurrentDirPath);
                    if(!string.Equals(tmpDirPath, "~", StringComparison.Ordinal))
                        ParentDirPath = VirtualPathUtility.GetDirectory(tmpDirPath);
                }

                if (string.IsNullOrEmpty(CurrentDirPath))
                {
                    Close(string.Format("{0}: {1}!", GetMessage("COULD_NOT_PARSE_PATH"), userPath), BXDialogGoodbyeWindow.LayoutType.Error, -1);
                    return;
                }
                if (!System.IO.Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath(CurrentDirPath)))
                {
                    Close(string.Format("{0}: {1}!", GetMessage("COULD_NOT_FIND_FOLDER"), CurrentDirPath), BXDialogGoodbyeWindow.LayoutType.Error, -1);
                    return;
                }
            }

            string userQuery = Request.QueryString["query"];
            if (!string.IsNullOrEmpty(userQuery))
                CurrentQuery = userQuery;

            //...проверка авторизации на модификацию раздела (просмотр без права на изменение не разрешён).
            if (!IsAuthorized())
            { 
                //...диалог авторизации;
                Close(string.Format("{0}: {1}!", GetMessage("INSUFFICIENT_RIGHTS"), CurrentDirPath), BXDialogGoodbyeWindow.LayoutType.Error, -1);
                return;
            }

            BXSite site = BXSite.GetCurrentSite(CurrentDirPath, Request.Url.Host);
            if (site == null)
            {
                Close(string.Format("{0} '{1}'!", GetMessage("SITE_IS_NOT_FOUND"), CurrentDirPath), BXDialogGoodbyeWindow.LayoutType.Error, -1);
                return;
            }

            _keywords = new Dictionary<string, Dictionary<string, string>>();
            _keywordsTypes = BXPageManager.GetKeywords(site.Id);
            foreach (string key in _keywordsTypes.Keys)
            {
                Dictionary<string, string> paramDic = new Dictionary<string, string>();
                paramDic.Add("caption", _keywordsTypes[key]);
                _keywords.Add(key, paramDic);
            }

            bool isRoot = string.IsNullOrEmpty(ParentDirPath);
            if (!isRoot)
            {
                BXSectionInfo parSectionInfo = BXSectionInfo.GetCumulativeSection(ParentDirPath);
                foreach (KeyValuePair<string, string> parSectionKeyword in parSectionInfo.Keywords)
                {
                    if (!_keywordsTypes.ContainsKey(parSectionKeyword.Key))
                        continue;

                    Dictionary<string, string> paramDic = null;
                    if (!_keywords.TryGetValue(parSectionKeyword.Key, out paramDic))
                        continue;

                    paramDic.Add("defaultValue", parSectionKeyword.Value);
                }
            }

            BXSectionInfo curSectionInfo = BXSectionInfo.GetSection(CurrentDirPath);
            //ClientFolderTitle = curSectionInfo.Name;

            foreach (KeyValuePair<string, string> curSectionKeyword in curSectionInfo.Keywords)
            {
                if (!_keywordsTypes.ContainsKey(curSectionKeyword.Key))
                    continue;

                Dictionary<string, string> paramDic = null;
                if (!_keywords.TryGetValue(curSectionKeyword.Key, out paramDic))
                    continue;

                paramDic.Add("value", curSectionKeyword.Value);
            }



            LoadKeywords();

            Behaviour.Settings.MinWidth = 400;
            Behaviour.Settings.MinHeight = 250;
            Behaviour.Settings.Width = 600;
            Behaviour.Settings.Height = 320;
            Behaviour.Settings.Resizeable = true;

            string query = CurrentQuery;
            if (!string.IsNullOrEmpty(query))
                query = BXQueryString.RemoveParameter(BXConfigurationUtility.Constants.BackUrl, query);

            string backUrl = !string.IsNullOrEmpty(query) ? string.Concat(CurrentPath, "?", query) : CurrentPath;

            DescriptionIconClass = "bx-property-folder";
            DescriptionParagraphs.Add(string.Format("{0} <b>{1}</b>", GetMessage("DESCRIPTION_HEADER"), CurrentDirPath));
            DescriptionParagraphs.Add(
                string.Format(
                    "<a href=\"{0}?path={1}&{2}={3}\" >{4}</a>", 
                    VirtualPathUtility.ToAbsolute("~/bitrix/admin/FileManFolderSettings.aspx"), 
                    HttpUtility.UrlEncode(CurrentDirPath),
                    BXConfigurationUtility.Constants.BackUrl,
                    !string.IsNullOrEmpty(backUrl) ? HttpUtility.UrlEncode(backUrl) : string.Empty,
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

        tblFolderProp.DataSource = _keywordsTypes;
        tblFolderProp.DataBind();
    }

    private string mCurrentPath = string.Empty;
    protected string CurrentPath
    {
        get { return mCurrentPath; }
        set { mCurrentPath = value; }
    }

    private string mCurrentQuery = string.Empty;
    protected string CurrentQuery
    {
        get { return mCurrentQuery; }
        set { mCurrentQuery = value; }
    }

    private string mCurrentDirPath = string.Empty;
    protected string CurrentDirPath
    {
        get { return mCurrentDirPath; }
        set { mCurrentDirPath = value; }
    }


    private string mParentDirPath = string.Empty;
    protected string ParentDirPath
    {
        get { return mParentDirPath; }
        set { mParentDirPath = value; }
    }



    protected void Behaviour_Save(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(CurrentDirPath))
            throw new InvalidOperationException("CurrentDirPath is not assigned!");

        try
        {
            if (!IsAuthorized())
            {
                Close(GetMessage("INSUFFICIENT_RIGHTS"), BXDialogGoodbyeWindow.LayoutType.Error, -1);
                return;
            }

            BXSectionInfo dirInfo = BXSectionInfo.GetSection(CurrentDirPath);
            if (_keywords == null)
                throw new InvalidOperationException("Keywords is not assinned!");

            foreach (string keywordKey in _keywords.Keys)
            {
                string keywordValue = string.Empty;
                if (!_keywords[keywordKey].TryGetValue("value", out keywordValue) || string.IsNullOrEmpty(keywordValue))
                {
                    if (dirInfo.Keywords.ContainsKey(keywordKey))
                        dirInfo.Keywords.Remove(keywordKey);
                }
                else
                {
                    string defaultKeywordValue = string.Empty;
                    if (!_keywords[keywordKey].TryGetValue("defaultValue", out defaultKeywordValue))
                    {
                        if (dirInfo.Keywords.ContainsKey(keywordKey))
                            dirInfo.Keywords[keywordKey] = keywordValue;
                        else
                            dirInfo.Keywords.Add(keywordKey, keywordValue);
                    }
                    else
                    {
                        if (string.Equals(keywordValue, defaultKeywordValue, StringComparison.CurrentCulture))
                        {
                            if (dirInfo.Keywords.ContainsKey(keywordKey))
                                dirInfo.Keywords.Remove(keywordKey);
                        }
                        else
                        {
                            if (dirInfo.Keywords.ContainsKey(keywordKey))
                                dirInfo.Keywords[keywordKey] = keywordValue;
                            else
                                dirInfo.Keywords.Add(keywordKey, keywordValue);
                        }
                    }
                }
            }

            dirInfo.Save();

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

        //return string.Compare(defaultValue, currentValue, StringComparison.CurrentCulture) != 0;
        return true;
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
