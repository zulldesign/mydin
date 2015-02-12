using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Services.Storage;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Services;
using Bitrix.Security;
using Bitrix.Configuration;
using System.Xml;
using System.IO;
using System.Text;

public partial class bitrix_admin_StorageList : BXAdminPage, ICallbackEventHandler
{
    private BXStorageSynchronizerStatus syncStatus;
    private BXStorageSynchronizerStatus InternalSynchronizerStatus
    {
        get
        {
            if (this.syncStatus != null)
            {
                return this.syncStatus;
            }

            string s = BXOptionManager.GetOptionString("BXStorageSynchronizer", "Status", string.Empty);
            this.syncStatus = !string.IsNullOrEmpty(s) ? BXStorageSynchronizerStatus.CreateFromXml(s) : new BXStorageSynchronizerStatus();                

            return this.syncStatus;
        }
    }

    private void ClearSynchronizerStatus()
    {
        BXOptionManager.DeleteOption("BXStorageSynchronizer", "Status", null);
    }

    protected override void OnInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
            BXAuthentication.AuthenticationRequired();

        PopupPanelView.Commands.First(x => x.UserCommandId == "load").OnClickScript = string.Concat(
            "window.location.href = 'StorageConfigList.aspx?action=load&id=' + UserData['ID'] +'&"
            + BXCsrfToken.BuildQueryStringPair()
            + "'; return false;");

        PopupPanelView.Commands.First(x => x.UserCommandId == "unload").OnClickScript = string.Concat(
            "window.location.href = 'StorageConfigList.aspx?action=unload&id=' + UserData['ID'] +'&"
            + BXCsrfToken.BuildQueryStringPair()
            + "'; return false;");

        if (InternalSynchronizerStatus != null && InternalSynchronizerStatus.Mode != BXStorageSynchronizerMode.Undefined)
        {
            ClientScript.RegisterStartupScript(
                GetType(),
                "FileTransferStatusStart",
                "Bitrix.FileTransferStatus.getInstance().start();",
                true);
        }

        if (string.Equals(Request["action"], "load") && BXCsrfToken.CheckTokenFromRequest(Request.QueryString))
        {
            int id = 0;
            if (int.TryParse(Request["id"], out id) && id > 0)
            {
                BXStorageConfiguration config = BXStorageConfiguration.GetById(id);
                if (config != null)
                {
                    BXStorageSynchronizerSettings settings = new BXStorageSynchronizerSettings
                    {
                        Mode = BXStorageSynchronizerMode.Loading,
                        StorageConfigId = config.Id,
                        StorageLastModifiedUtc = config.LastModifiedUtc,
                        Interval = 3
                    };
                    BXStorageSynchronizer.Start(settings);
                    Response.Redirect(VirtualPathUtility.ToAbsolute(Request.Path));
                }
            }
        }
        else if (string.Equals(Request["action"], "unload") && BXCsrfToken.CheckTokenFromRequest(Request.QueryString))
        {
            int id = 0;
            if (int.TryParse(Request["id"], out id) && id > 0)
            {
                BXStorageConfiguration config = BXStorageConfiguration.GetById(id);
                if (config != null)
                {
                    BXStorageSynchronizerSettings settings = new BXStorageSynchronizerSettings
                    {
                        Mode = BXStorageSynchronizerMode.Unloading,
                        StorageConfigId = config.Id,
                        StorageLastModifiedUtc = config.LastModifiedUtc,
                        Interval = 3
                    };
                    BXStorageSynchronizer.Start(settings);
                    Response.Redirect(VirtualPathUtility.ToAbsolute(Request.Path));
                }
            }
        }

        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        MasterTitle = Page.Title = GetMessage("PageTitle");
        base.OnLoad(e);
    }

    protected void ItemGrid_Select(object sender, BXSelectEventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
            return;

        List<StorageConfigWrapper> list = new List<StorageConfigWrapper>();

        BXSelect s = new BXSelect(BXSelectFieldPreparationMode.Normal,
            BXStorageConfiguration.Fields.Id,
            BXStorageConfiguration.Fields.IsActive,
            BXStorageConfiguration.Fields.Sort,
            BXStorageConfiguration.Fields.CreatedUtc,
            BXStorageConfiguration.Fields.StorageTypeName);

        BXStorageConfigurationCollection col = BXStorageConfiguration.GetList(
            null,
            new BXOrderBy(BXStorageConfiguration.Fields, string.IsNullOrEmpty(e.SortExpression) ? "Id" : e.SortExpression),
            s,
            new BXQueryParams(e.PagingOptions),
            BXTextEncoder.EmptyTextEncoder);

        foreach (BXStorageConfiguration item in col)
            list.Add(new StorageConfigWrapper(item, this));

        e.Data = list;
    }

    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        e.Count = BXStorageConfiguration.Count(null);
    }

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        StorageConfigWrapper wrapper = (StorageConfigWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);
    }

    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
            return;

        BXStorageConfigurationCollection entities;
        try
        {
            BXFilter filter = e.Keys != null
                ? new BXFilter(new BXFilterItem(BXStorageConfiguration.Fields.Id, BXSqlFilterOperators.Equal, e.Keys["ID"])) 
                : null;
            entities = BXStorageConfiguration.GetList(filter, null);
        }
        catch (Exception ex)
        {
            ErrorMessage.AddErrorMessage(ex.Message);
            return;
        }
        bool errorTextAdded = false;
        foreach (BXStorageConfiguration entity in entities)
        {
            try
            {
                entity.Delete();
                e.DeletedCount++;
            }
            catch
            {
                if (!errorTextAdded)
                {
                    ErrorMessage.AddErrorMessage(GetMessage("StorageConfiguration.DeleteFailure"));
                    errorTextAdded = true;
                }
            }
        }

    }

    internal class StorageConfigWrapper
    {
        public StorageConfigWrapper(BXStorageConfiguration entity, BXAdminPage page)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            this.entity = entity;

            if(page == null)
                throw new ArgumentNullException("page");
            this.page = page;
        }

        private BXStorageConfiguration entity;
        public BXStorageConfiguration Entity
        {
            get { return this.entity; }
        }

        private BXAdminPage page;
        private BXStorageConfiguration item;
        private bitrix_admin_StorageList bitrix_admin_StorageList;
        public BXAdminPage Page
        {
            get { return this.page; }
        }

        public string ID
        {
            get { return this.entity.Id.ToString(); }
        }

        public string ProviderName
        {
            get 
            {
                BXTypeInfo info =  BXStorageManager.GetStorageInfoByTypeName(this.entity.StorageTypeName);
                return info != null ? info.Description : "<?>";
            }
        }

        public string IsActive
        {
            get { return this.entity.IsActive ? this.page.GetMessageRaw("Kernel.Yes") : this.page.GetMessageRaw("Kernel.No"); }
        }

        public string Sort
        {
            get { return this.entity.Sort.ToString(); }
        }

        public string CreatedUtc
        {
            get { return this.entity.CreatedUtc.ToLocalTime().ToString("g"); }
        }
    }

    #region ICallbackEventHandler
    public string GetCallbackResult()
    {
        BXStorageSynchronizerStatus s = InternalSynchronizerStatus;
        if (s == null || s.Mode == BXStorageSynchronizerMode.Undefined)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("mode:'").Append(s.Mode.ToString("G")).Append("'");
        sb.Append(",").Append("filesProcessed:").Append(s.FilesProcessed.ToString());
        sb.Append(",").Append("filesTransfered:").Append(s.FilesTransfered.ToString());
        sb.Append(",").Append("isCompleted:").Append(s.IsCompleted.ToString().ToLowerInvariant());

        sb.Insert(0, "{").Append("}");

        if (s.IsCompleted)
        {
            ClearSynchronizerStatus();
        }
        return sb.ToString();
    }

    public void RaiseCallbackEvent(string eventArgument)
    {
    }
    #endregion
}