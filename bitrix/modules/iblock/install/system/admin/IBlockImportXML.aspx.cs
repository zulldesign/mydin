using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix;
using Bitrix.IBlock;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.UI;

public partial class bitrix_admin_IBlockImportXML : BXAdminPage, ICallbackEventHandler
{
    private string eventName;
    private static string progressFormat;
    private EventWaitHandle cancelEvent;
    private BXIBlockImportExportHelper helper = new BXIBlockImportExportHelper();
    private BXIBlockImportXmlSettings settings;

    private delegate void ExecuteDelegate();

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        this.cancelEvent = this.OpenEvent();
        this.settings = BXIBlockImportXmlSettings.Load();
        if (this.settings == null)
        {
            this.settings = new BXIBlockImportXmlSettings();
        }

        var siteTypes = TypeCB.Items;
        siteTypes.Clear();
        siteTypes.Add(new ListItem(GetMessageRaw("Combo.SelectIBlockType"), string.Empty));
        foreach (var type in this.helper.AllTypes)
        {
            siteTypes.Add(new ListItem(this.helper.GetTypeName(type), type.Id.ToString()));
        }

        var siteSites = SitesList.Items;
        siteSites.Clear();
        foreach (var site in this.helper.AllSites)
        {
            siteSites.Add(new ListItem(site.Name, site.Id));
        }

        if (siteSites.Count > 0)
        {
            siteSites[0].Selected = true;
        }

        this.CreatePreviewImage.Checked = this.settings.CreatePreviewImage;
        this.PreviewImageWidth.Text = this.settings.PreviewImageWidth.ToString();
        this.PreviewImageHeight.Text = this.settings.PreviewImageHeight.ToString();
        this.ResizeDetailImage.Checked = this.settings.ResizeDetailImage;
        this.DetailImageWidth.Text = this.settings.DetailImageWidth.ToString();
        this.DetailImageHeight.Text = this.settings.DetailImageHeight.ToString();
        this.ElementActions.SelectedIndex = (int)this.settings.ElementOmission;
        this.SectionActions.SelectedIndex = (int)this.settings.SectionOmission;
        this.SourceFile.Text = this.settings.SourceFile;

        this.UpdateControls();
    }

    private WebControl[] ToggleControls
    {
        get 
        {
            return new WebControl[]
            {
                this.SourceFile,
                this.SelectDirButton,
                this.SitesList,
                this.TypeCB,
                this.CreatePreviewImage,
                this.PreviewImageWidth,
                this.PreviewImageHeight,
                this.ResizeDetailImage,
                this.DetailImageWidth,
                this.DetailImageHeight,
                this.ElementActions,
                this.SectionActions,
                this.StartButton
            };
        }
    }

    private void CreateCallbackScripts()
    {
        var cm = Page.ClientScript;
        if (cm != null)
        {
            var showResultScript = new StringBuilder();
            showResultScript.Append(
            @"<script type='text/javascript'>
                    function ShowResult(arg, context)
                    {
                        var disable = true;
                        if(arg != '.')
                        {
                            window.setTimeout('CallServer()', 1000);
                            document.getElementById('" + this.ProgressLabel.ClientID + @"').innerHTML = arg;                            
                        }
                        else
                        {
                            document.getElementById('" + this.UpdateProgress.ClientID + @"').style.display = 'none';
                            document.getElementById('" + this.StopButton.ClientID + @"').disabled = true;
                            disable = false;
                        }");

            showResultScript.AppendLine();
            foreach (var control in this.ToggleControls)
            {
                showResultScript.Append(@"document.getElementById('" + control.ClientID + @"').disabled = disable;");
                showResultScript.AppendLine();
            }

            showResultScript.Append(
            @"}

                    window.setTimeout('CallServer()', 1000);
                </script>");

            cm.RegisterClientScriptBlock(this.GetType(), "ShowResult", showResultScript.ToString());

            var cbReference = cm.GetCallbackEventReference(this, "arg", "ShowResult", string.Empty);
            var callServerScript =
            @"<script type='text/javascript'>
                function CallServer(arg, context)
                {" + cbReference + @"; }
            </script>";

            cm.RegisterClientScriptBlock(this.GetType(), "CallServer", callServerScript);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        this.MasterTitle = this.Page.Title = GetMessage("PageTitle");

        this.CreateCallbackScripts();
    }

    protected EventWaitHandle OpenEvent()
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
                this.eventName = string.Format("bitrix:xml:import:{0}:site:{1}:user:{2}", 
                    this.helper.ComputeHash(BXSite.Current.DirectoryAbsolutePath), BXSite.Current.Id, BXIdentity.Current.Id);
            }

            return this.eventName;
        }
    }

    private void DoImport()
    {
        try
        {
            var importer = new BXIBlockImportXml(this.settings, this.ReportProgress, this.CancellationPending);
            importer.Execute();
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

    protected void OnStopImport(object sender, EventArgs e)
    {
        this.CancelImport();
        this.UpdateControls();
    }

    protected void OnStartImport(object sender, EventArgs e)
    {
        this.cancelEvent = this.OpenEvent();
        if (this.cancelEvent != null)
        {
            this.cancelEvent.Reset();
        }

        this.cancelEvent = new EventWaitHandle(false, EventResetMode.ManualReset, this.EventName);
        BXIBlockImportXmlSettings.Delete(BXIdentity.Current.Id, BXSite.Current.Id);

        this.settings = new BXIBlockImportXmlSettings(BXIdentity.Current.Id, BXSite.Current.Id);
        this.settings.Locale = BXLoc.CurrentLocale;
        this.settings.ImportExportMode = BXIBlockImportExportMode.InfoBlock;
        this.settings.TypeId = this.GetSelectedComboValue(this.TypeCB);
        this.settings.Sites.AddRange(from ListItem item in SitesList.Items where item.Selected select item.Value);
        this.settings.SourceFile = this.SourceFile.Text;

        this.settings.CreatePreviewImage = this.CreatePreviewImage.Checked;
        this.ParseInt(this.PreviewImageWidth.Text, ref this.settings.PreviewImageWidth);
        this.ParseInt(this.PreviewImageHeight.Text, ref this.settings.PreviewImageHeight);

        this.settings.ResizeDetailImage = this.ResizeDetailImage.Checked;
        this.ParseInt(this.DetailImageWidth.Text, ref this.settings.DetailImageWidth);
        this.ParseInt(this.DetailImageHeight.Text, ref this.settings.DetailImageHeight);

        this.settings.ElementOmission = (BXXmlImportOmissionProcessing)this.ElementActions.SelectedIndex;
        this.settings.SectionOmission = (BXXmlImportOmissionProcessing)this.SectionActions.SelectedIndex;
        this.settings.Save();

        this.StartImport();
        this.UpdateControls();
    }

    private IAsyncResult StartImport()
    {
        return new ExecuteDelegate(this.DoImport).BeginInvoke(new AsyncCallback(this.CallBackMethod), null);
    }

    private void CallBackMethod(IAsyncResult ar)
    {
        var result = (AsyncResult)ar;
        var runner = (ExecuteDelegate)result.AsyncDelegate;
        runner.EndInvoke(result);
    }

    private void UpdateControls()
    {
        var canceled = this.CancellationPending();
        this.UpdateProgress.Visible = !canceled;
        if (!canceled)
        {
            this.ProgressLabel.Text = this.GetCallbackResult();          
        }

        this.StopButton.Enabled = !canceled;
        this.StartButton.Enabled = canceled;

        var props = BXIBlockImportXmlSettings.Load();
        if (props != null && !string.IsNullOrEmpty(props.LastError))
        {
            this.ProgressLabel.Text = props.LastError;
        }
    }

    private void CancelImport()
    {
        this.cancelEvent = this.OpenEvent();
        if (this.cancelEvent != null)
        {
            this.cancelEvent.Set();
        }
    }

    private void ParseInt(string str, ref int value)
    {
        int result = 0;
        if (int.TryParse(str, out result) && result > 0)
        {
            value = result;
        }
    }

    private int GetSelectedComboValue(DropDownList cb)
    {
        if (cb.SelectedIndex != -1 && !string.IsNullOrEmpty(cb.SelectedItem.Value))
        {
            int result;
            if (int.TryParse(cb.SelectedItem.Value, out result))
            {
                return result;
            }
        }

        return -1;
    }

    private bool CancellationPending()
    {
        return this.cancelEvent != null ? this.cancelEvent.WaitOne(0) : true;
    }

    private void ReportProgress(int sections, int elements, double percent)
    {
        BXIBlockImportExportHelper.ReportProgress(this.CancellationPending(), this.settings, sections, elements, percent);
    }

    public string GetCallbackResult()
    {
        if (progressFormat == null)
        {
            progressFormat = GetMessageRaw("Progress.Section") + ": {0}<br>" +
                           GetMessageRaw("Progress.Elements") + ": {1}<br>" +
                           GetMessageRaw("Progress.Total") + ": {2:p}";
        }

        return BXIBlockImportExportHelper.GetCallbackResult<BXIBlockImportXmlSettings>(progressFormat, this.CancellationPending());
    }

    public void RaiseCallbackEvent(string eventArgument)
    {
    }
}