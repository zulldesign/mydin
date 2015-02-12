using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Services.Storage;
using Bitrix.Security;
using Bitrix.Services.Text;
using Bitrix.Services;
using System.Web.Script.Serialization;

public partial class bitrix_admin_StorageConfigEdit : BXAdminPage
{
    private BXTypeInfo[] storageInfos;
    private BXStorageSettingsEditor[] storageSettingsEditors; 

    private int entityId = 0;
    public int EntityId
    {
        get { return this.entityId; }
    }

    private BXStorageConfiguration entity;
    public BXStorageConfiguration Entity
    {
        get { return this.entity; }
    }

    private BXAdminPageEditorMode editorMode = BXAdminPageEditorMode.Creation;
    protected BXAdminPageEditorMode EditorMode
    {
        get { return this.editorMode; }
    }

    private string errorMessageText = string.Empty;
    protected string ErrorMessageText
    {
        get { return this.errorMessageText; }
    }

    private StorageConfigEditorError editorError = StorageConfigEditorError.None;
    protected StorageConfigEditorError EditorError
    {
        get { return this.editorError; }
    }

    private void TrySaveEntity()
    {
        if (this.editorError != StorageConfigEditorError.None)
            return;

        if (this.entity == null)
        {
            this.editorError = StorageConfigEditorError.IsNotFound;
            this.errorMessageText = GetMessageRaw("Message.EntityIsNotFound");
            return;
        }

        try
        {
            this.entity.IsActive = ActiveChkBx.Checked;
            int i;
            this.entity.Sort = int.TryParse(SortTbx.Text, out i) ? i : 10;
            this.entity.StorageTypeName = ProviderDdl.SelectedValue;

            this.entity.StorageSettings.Assign(PrepareSettingsEditorJsonSerializer().Deserialize<BXStorageSettingsEditor>(SettingsEditorDataFld.Value).Settings);

            BXStorageBinding[] bindings = null;
            if (BindingDataFld.Value.Length > 0)
            {
                List<BXStorageBinding> lst = PrepareBindingJsonSerializer().Deserialize<List<BXStorageBinding>>(BindingDataFld.Value);
                if (lst != null)
                    bindings = lst.ToArray();
            }
            else
                bindings = new BXStorageBinding[0];

            this.entity.SetBindings(bindings);

            this.entity.Save();
            this.entityId = this.entity.Id;
        }
        catch (Exception ex)
        {
            this.errorMessageText = ex.Message;
            this.editorError = this.entity.IsNew ? StorageConfigEditorError.Creation : StorageConfigEditorError.Modification;
        }
    }

    private JavaScriptSerializer PrepareBindingJsonSerializer()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.RegisterConverters(new JavaScriptConverter[] { new BXStorageBindingJsonConv() });
        return serializer;
    }

    private JavaScriptSerializer PrepareSettingsEditorJsonSerializer()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        List<JavaScriptConverter> converters = new List<JavaScriptConverter>();

        if (this.storageSettingsEditors == null)
            throw new InvalidOperationException("Could not find Storage Settings Editors.");

        for (int i = 0; i < this.storageSettingsEditors.Length; i++)
        {
            BXStorageSettingsEditor editor = this.storageSettingsEditors[i];
            converters.AddRange(editor.CreateJsonConverters());
        }
        serializer.RegisterConverters(converters);
        return serializer;
    }

    protected override void OnInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
            BXAuthentication.AuthenticationRequired();

        string id = Request.QueryString["id"];
        if (string.IsNullOrEmpty(id))
            this.entityId = 0;
        else
        {
            try
            {
                this.entityId = Convert.ToInt32(Request.QueryString["id"]);
            }
            catch
            {
                this.entityId = 0;
            }
        }

        if (this.entityId == 0)
            this.entity = new BXStorageConfiguration();
        else if ((this.entity = BXStorageConfiguration.GetById(EntityId, BXTextEncoder.EmptyTextEncoder)) == null)
        {
            this.editorError = StorageConfigEditorError.IsNotFound;
            this.errorMessageText = GetMessageRaw("Message.EntityIsNotFound");
            return;
        }
        this.editorMode = this.entity.IsNew ? BXAdminPageEditorMode.Creation : BXAdminPageEditorMode.Modification;

        List<BXTypeInfo> storageInfoLst = new List<BXTypeInfo>();
        foreach (BXTypeInfo storageInfo in BXStorageManager.GetStorageInfos())
        {
            //do not include BXFileSystemStorage - is not cloud storage
            if (!string.Equals(storageInfo.Name, typeof(BXFileSystemStorage).FullName, StringComparison.OrdinalIgnoreCase))
            {
                storageInfoLst.Add(storageInfo);
            } 
        }
        this.storageInfos = storageInfoLst.ToArray();

        List<BXStorageSettingsEditor> storageSettingsEditorLst = new List<BXStorageSettingsEditor>();
        foreach(BXTypeInfo storageInfo in this.storageInfos)
        {
            BXStorageSettingsEditor editor = BXStorageManager.CreateStorageSettingsEditor(storageInfo.Name);
            if (editor != null)
            {
                storageSettingsEditorLst.Add(editor);
                if (!this.entity.IsNew && string.Equals(this.entity.StorageTypeName, storageInfo.Name, StringComparison.OrdinalIgnoreCase))
                {
                    editor.Settings = this.entity.StorageSettings;
                }
            }
        }
        this.storageSettingsEditors = storageSettingsEditorLst.ToArray();

        MasterTitle = Page.Title = this.editorMode == BXAdminPageEditorMode.Creation ? GetMessage("PageTitle.Creation") : string.Concat(GetMessage("PageTitle.Modification"), " #", EntityId.ToString());
        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        if (this.editorError != StorageConfigEditorError.None)
            return;

        if (!IsPostBack)
        {
            ActiveChkBx.Checked = this.entity.IsActive;
            SortTbx.Text = this.entity.Sort.ToString();

            ListItemCollection providerItems = ProviderDdl.Items;
            providerItems.Clear();
            foreach (BXTypeInfo info in this.storageInfos)
            {
                providerItems.Add(new ListItem(info.Description, info.Name));
            }

            ListItem selectedProviderItem = providerItems.FindByValue(this.entity.StorageTypeName);
            if (selectedProviderItem != null)
                selectedProviderItem.Selected = true;
            else
                ProviderDdl.SelectedIndex = -1;
        }

        base.OnLoad(e);
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (EditorError != StorageConfigEditorError.None)
        {
            errorMessage.AddErrorMessage(ErrorMessageText);
            if (EntityId > 0 && EditorError == StorageConfigEditorError.IsNotFound)
                TabControl.Visible = false;
        }

        BindingDataFld.Value = PrepareBindingJsonSerializer().Serialize(this.entity.GetBindings());

        if (this.storageSettingsEditors.Length > 0)
        {
            SettingsEditorDataFld.Value = PrepareSettingsEditorJsonSerializer().Serialize(this.storageSettingsEditors);
        }
        else
            SettingsEditorDataFld.Value = "[]";

        BXPage.Scripts.RequireUtils();

        for (int i = 0; i < this.storageSettingsEditors.Length; i++)
        {
            string[] paths = this.storageSettingsEditors[i].GetClientSideFilePaths();
            if (paths == null || paths.Length == 0)
                continue;
            for (int j = 0; j < paths.Length; j++)
                BXPage.RegisterScriptInclude(paths[j]);
        }

        Page.ClientScript.RegisterOnSubmitStatement(GetType(), "SaveSettings", "Bitrix.StorageController.getInstance().saveConfig();");

        if (EditorMode != BXAdminPageEditorMode.Modification)
        {
            AddButton.Visible = DeleteButton.Visible = false;
        }
    }

    protected void OnToolBarButtonClick(object sender, CommandEventArgs e)
    {
        if (e.CommandName == "delete")
        {
            try
            {
                if (this.entity != null)
                    this.entity.Delete();

                GoBack();
            }
            catch (Exception ex)
            {
                this.errorMessageText = ex.Message;
                this.editorError = StorageConfigEditorError.Deleting;
            }
        }
    }

    protected override string BackUrl
    {
        get
        {
            return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "StorageConfigList.aspx";
        }
    }

    protected void OnEntityEdit(object sender, BXTabControlCommandEventArgs e)
    {
        switch (e.CommandName)
        {
            case "save":
                {
                    if (IsValid && this.editorError == StorageConfigEditorError.None)
                    {
                        TrySaveEntity();
                        if (this.editorError == StorageConfigEditorError.None)
                            GoBack();
                    }
                }
                break;
            case "apply":
                {
                    if (IsValid && this.editorError == StorageConfigEditorError.None)
                    {
                        TrySaveEntity();
                        if (this.editorError == StorageConfigEditorError.None)
                            Response.Redirect(string.Format("StorageConfigEdit.aspx?id={0}&tabindex={1}", this.entityId.ToString(), TabControl.SelectedIndex));
                    }
                }
                break;
            case "cancel":
                GoBack();
                break;
        }
    }
}

public enum StorageConfigEditorError
{
    None = 0,
    IsNotFound = -1,
    Creation = -2,
    Modification = -3,
    Deleting = -4
}