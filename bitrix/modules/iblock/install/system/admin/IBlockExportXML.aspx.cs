using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix;
using Bitrix.Configuration;
using Bitrix.IBlock;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.UI;

public partial class bitrix_admin_IBlockExportXML : BXAdminPage, ICallbackEventHandler
{
    private string eventName;
    private string progressFormat;
    private EventWaitHandle cancelEvent;
    private BXIBlockImportExportHelper helper = new BXIBlockImportExportHelper();
    private BXIBlockExportXmlSettings settings;
    private delegate void ExecuteDelegate();

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        this.cancelEvent = this.OpenEvent();
        this.settings = BXIBlockExportXmlSettings.Load();
        if (this.settings == null)
        {
            this.settings = new BXIBlockExportXmlSettings();
            this.settings.DestinationFile = BXConfigurationUtility.Options.UploadFolderPath +
                                    "export_file_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml";
        }

        this.DestinationFile.Text = this.settings.DestinationFile;

        var siteTypes = TypeCB.Items;
        siteTypes.Clear();
        siteTypes.Add(new ListItem(GetMessageRaw("SelectItem.IBlockType"), string.Empty));

        foreach (var type in this.helper.AllTypes)
        {
            var name = this.helper.GetTypeName(type);
            siteTypes.Add(new ListItem(name, type.Id.ToString()));
        }

        int typeId = -1;
        if (!string.IsNullOrEmpty(settings.TypeName))
        {
            var item = TypeCB.Items.FindByValue(settings.TypeName);
            if (item != null)
            {
                item.Selected = true;
            }

            Int32.TryParse(settings.TypeName, out typeId);
        }

        this.UpdateBlocks(typeId);
        this.UpdateControls();

        if (!string.IsNullOrEmpty(settings.BlockName))
        {
            var item = BlockCB.Items.FindByValue(settings.BlockName);
            if (item != null)
            {
                item.Selected = true;
            }
        }

        this.Sections.SelectedIndex = (int)this.settings.SectionsMode;
        this.Elements.SelectedIndex = (int)this.settings.ElementsMode;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        this.MasterTitle = this.Page.Title = GetMessage("PageTitle");
    }

    private EventWaitHandle OpenEvent()
    {
        if (this.cancelEvent == null)
        {
            try
            {
                this.cancelEvent = EventWaitHandle.OpenExisting(this.EventName);
            }
            catch (System.Exception)
            {
            }
        }

        return this.cancelEvent;
    }

    private string EventName
    {
        get
        {
            if (string.IsNullOrEmpty(this.eventName))
            {
                this.eventName = string.Format("bitrix:xml:export{0}:site:{1}:user:{2}",
                    helper.ComputeHash(BXSite.Current.DirectoryAbsolutePath), BXSite.Current.Id, BXIdentity.Current.Id);
            }

            return this.eventName;
        }
    }

    private void DoExport()
    {
        try
        {
            var exporter = new BXIBlockExportXml(this.settings, this.ReportProgress, this.CancellationPending);
            exporter.Execute();
        }
        catch (Exception ex)
        {
            this.settings.LastError = ex.Message;
        }
        finally
        {
            if (!this.CancellationPending())
            {
                this.settings.ProcessedPercent = 100;
            }

            this.settings.Save();
            this.cancelEvent.Set();
        }
    }

    private IAsyncResult StartExport()
    {
        return new ExecuteDelegate(DoExport).BeginInvoke(new AsyncCallback(CallBackMethod), null);
    }

    private void CallBackMethod(IAsyncResult ar)
    {
        var result = (AsyncResult)ar;
        var runner = (ExecuteDelegate)result.AsyncDelegate;
        runner.EndInvoke(result);
    }

    protected void OnStartExport(object sender, EventArgs e)
    {
        if (this.TypeCB.SelectedIndex == 0)
        {
            this.ProgressLabel.Text = GetMessageRaw("SelectItem.IBlockType");
            return;
        }

        var blockId = this.GetSelectedComboValue(this.BlockCB);
        if (blockId != -1)
        {
            this.cancelEvent = this.OpenEvent();
            if (this.cancelEvent != null)
            {
                this.cancelEvent.Reset();
            }

            this.cancelEvent = new EventWaitHandle(false, EventResetMode.ManualReset, this.EventName);
            BXIBlockExportXmlSettings.Delete(BXIdentity.Current.Id, BXSite.Current.Id);

            this.settings = new BXIBlockExportXmlSettings(BXIdentity.Current.Id, BXSite.Current.Id);
            this.settings.Locale = BXLoc.CurrentLocale;
            this.settings.BlockId = blockId;
            this.settings.ExportMode = XmlExportMode.Catalog;
            this.settings.BlockName = this.BlockCB.SelectedValue;
            this.settings.TypeName = this.TypeCB.SelectedValue;
            this.settings.DestinationFile = this.DestinationFile.Text;
            this.settings.SectionsMode = (BXIBlockExportMode)this.Sections.SelectedIndex;
            this.settings.ElementsMode = (BXIBlockExportMode)this.Elements.SelectedIndex;
            this.settings.Save();
            
            this.StartExport();
            this.UpdateControls();
        }
        else
        {
            this.ProgressLabel.Text = GetMessageRaw("SelectItem.IBlock");
        }
    }

    protected void OnStopExport(object sender, EventArgs e)
    {
        this.CancelExport();
        this.UpdateControls();
    }

    private void CancelExport()
    {
        this.cancelEvent = OpenEvent();
        if (this.cancelEvent != null)
        {
            cancelEvent.Set();
        }
    }

    protected void OnTypeChanged(object sender, EventArgs e)
    {
        var typeId = this.GetSelectedComboValue(this.TypeCB);

        this.UpdateBlocks(typeId);
        this.UpdateControls();
    }

    protected void OnBlockChanged(object sender, EventArgs e)
    {
        this.UpdateControls();
    }

    private int GetSelectedComboValue(DropDownList cb)
    {
        int result = -1;
        if (cb.SelectedIndex != -1 && !string.IsNullOrEmpty(cb.SelectedItem.Value))
        {
            int.TryParse(cb.SelectedItem.Value, out result);
        }

        return result;
    }

    private void UpdateBlocks(int typeId)
    {
        var siteBlocks = BlockCB.Items;
        siteBlocks.Clear();
        siteBlocks.Add(new ListItem(GetMessageRaw("SelectItem.IBlock"), string.Empty));

        foreach (var item in this.helper.AllBlocks)
        {
            if (item.TypeId == typeId)
            {
                siteBlocks.Add(new ListItem(item.Name, item.Id.ToString()));
            }
        }
    }

    private void UpdateControls()
    {
        var canceled = this.CancellationPending();
        this.UpdateProgress.Visible = !canceled;
        this.StopButton.Enabled = !canceled;
        this.StartButton.Enabled = canceled;
        if (!canceled)
        {
            this.ProgressLabel.Text = this.GetCallbackResult();
        }

        var props = BXIBlockExportXmlSettings.Load();
        if (props != null)
        {
            this.ProgressLabel.Text = props.LastError;
        }
    }

    private void ReportProgress(int sections, int elements, double percent)
    {
        BXIBlockImportExportHelper.ReportProgress(this.CancellationPending(), this.settings, sections, elements, percent);
    }

    private bool CancellationPending()
    {
        return cancelEvent != null ? cancelEvent.WaitOne(0) : true;
    }

    public string GetCallbackResult()
    {
        if (progressFormat == null)
        {
            progressFormat = GetMessageRaw("Progress.Section") + ": {0}<br>" +
                           GetMessageRaw("Progress.Elements") + ": {1}<br>" +
                           GetMessageRaw("Progress.Total") + ": {2:p}";
        }

        return BXIBlockImportExportHelper.GetCallbackResult<BXIBlockExportXmlSettings>(progressFormat, this.CancellationPending());       
    }

    public void RaiseCallbackEvent(string eventArgument)
    {
    }
}