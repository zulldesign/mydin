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
using Bitrix.Configuration;
using Bitrix.IO;
using System.IO;
using Bitrix.DataTypes;

public enum BitrixStylesheetEditorMode
{
    SiteStyle = 1,
    TemplateStyle
}

public partial class bitrix_dialogs_StylesheetEditor : Bitrix.UI.BXDialogPage
{
    private string _virtualFilePath = null;
    protected string VirtualFilePath
    {
        get 
        {
            if (string.IsNullOrEmpty(_virtualFilePath))
            {
                switch (Mode)
                {
                    case BitrixStylesheetEditorMode.SiteStyle:
                        _virtualFilePath = BXPath.Combine(VirtualFolderPath, BXConfigurationUtility.Constants.SiteStyleFileName);
                        break;
                    case BitrixStylesheetEditorMode.TemplateStyle:
                        _virtualFilePath = BXPath.Combine(VirtualFolderPath, BXConfigurationUtility.Constants.TemplateStyleFileName);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("'{0}' is not supported in this context!", Enum.GetName(typeof(BitrixStylesheetEditorMode), Mode)));
                }

                if (string.IsNullOrEmpty(_virtualFilePath))
                    throw new InvalidOperationException("Could not get templates file path!");
            }
            return _virtualFilePath;
        }
    }

    private string _virtualFolderPath = null;
    protected string VirtualFolderPath
    {
        get
        {
            if (string.IsNullOrEmpty(_virtualFolderPath))
            {
                _virtualFolderPath = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, TemplateId);
                if (string.IsNullOrEmpty(_virtualFolderPath))
                    throw new InvalidOperationException("Could not get templates folder path!");
            }
            return _virtualFolderPath;
        }
    }

    protected string TemplateId
    {
        get
        {
            if (Request["id"] != null && Directory.Exists(MapPath(BXConfigurationUtility.Constants.TemplatesFolderPath + Request["id"])))
                return Request["id"].Replace("/", "").Replace("\\", "").ToLower();

            return null;
        }
    }

    protected BitrixStylesheetEditorMode Mode
    {
        get 
        {
            string mode = Request.QueryString["StylesheetEditorMode"];
            return !string.IsNullOrEmpty(mode) &&
                string.Compare(mode, "TemplateStyle", StringComparison.OrdinalIgnoreCase) == 0 ? 
                BitrixStylesheetEditorMode.TemplateStyle : 
                BitrixStylesheetEditorMode.SiteStyle;
        }
    }

    protected bool MayManage(string path)
    {
        return BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage) && 
            BXSecureIO.CheckWrite(path);
    }

    protected bool MayView(string path)
    {
        return (BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage) || 
            BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView)) && 
            BXSecureIO.CheckView(path);
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        string virtualFilePath = VirtualFilePath;

        Title = string.Format(GetMessageRaw("FORMAT_MODIFICATION_OF_FILE"), virtualFilePath);
        if (MayView(virtualFilePath) && BXSecureIO.FileExists(virtualFilePath))
            textEditor.Text = BXSecureIO.FileReadAllText(virtualFilePath, BXConfigurationUtility.DefaultEncoding);

		textEditor.Attributes.Add("style", "border:solid 1px #CCCCCC; width:100%; padding:0px; margin:0px; height:99%; overflow-y:auto; wrap:soft; display:block;");
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        string virtualFilePath = VirtualFilePath;

        if (!MayView(virtualFilePath)) 
        {
            Close(GetMessageRaw("INSUFFICIENT_RIGHTS_TO_PRODUCT_SETTINGS_VIEW"), BXDialogGoodbyeWindow.LayoutType.Error, -1);
            return;
        }

        if (!MayManage(virtualFilePath))
            behaviour.SetButtonDisabled(BXPageAsDialogButtonEntry.Save, true);

        behaviour.Settings.MinWidth = 450;
        behaviour.Settings.MinHeight = 300;
        behaviour.Settings.Width = 500;
        behaviour.Settings.Height = 550;
        behaviour.Settings.Resizeable = true;

        int templateEditorTabIndex = -1;
        switch (Mode)
        {
            case BitrixStylesheetEditorMode.SiteStyle:
                templateEditorTabIndex = 1;
                break;
            case BitrixStylesheetEditorMode.TemplateStyle:
                templateEditorTabIndex = 2;
                break;
            default:
                throw new NotImplementedException(string.Format("'{0}' is not supported in current context!", Enum.GetName(typeof(BitrixStylesheetEditorMode), Mode)));
        }

        if(BXUser.IsCanOperate(BXRoleOperation.Operations.AccessAdminSystem))
            DescriptionParagraphs.Add(string.Format("<a href=\"{1}?id={2}&tabindex={3}\">{0}</a>", virtualFilePath, VirtualPathUtility.ToAbsolute("~/bitrix/admin/TemplateEdit.aspx"), TemplateId, templateEditorTabIndex.ToString()));
    }

    protected void behaviour_Save(object sender, EventArgs e) 
    {
        try
        {
            if (!BXSecureIO.CheckWrite(VirtualFilePath)) 
            {
                Close(GetMessageRaw("INSUFFICIENT_RIGHTS_PRODUCT_TO_SETTINGS_MANAGE"), BXDialogGoodbyeWindow.LayoutType.Error, -1);
                return;
            }

            BXSecureIO.DirectoryEnsureExists(VirtualFolderPath);
            BXSecureIO.FileWriteAllText(VirtualFilePath, textEditor.Text, BXConfigurationUtility.DefaultEncoding);

            Refresh(GetMessageRaw("OPERATION_HAS_BEEN_COMPLETED_SUCCESSFULLY"), BXDialogGoodbyeWindow.LayoutType.Success, -1);
        }
        catch (System.Threading.ThreadAbortException /*exception*/)
        {
            //...на Reload()->HTTPRESPONSE.End();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        } 
    }

    protected override string GetParametersBagName()
    {
        return "BitrixDialogStylesheetEditorParamsBag";
    }

    protected override void ExternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        paramsBag.Add(textEditor.ID, Request.Form[textEditor.UniqueID]);
    }

    protected override void InternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        if (paramsBag.ContainsKey(textEditor.ID))
        {
            string text = paramsBag[textEditor.ID];
            if (!string.IsNullOrEmpty(text))
                textEditor.Text = text;
        }
    }
}
