using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Bitrix.UI;
using Bitrix.DataLayer;
using System.Data;
using Bitrix.Services;
using System.Data.SqlClient;
using System.Collections;
using Bitrix.DataTypes;
using System.Collections.Specialized;
using Bitrix.Services.Text;
using Bitrix;
using Bitrix.Configuration;
using System.Web.Hosting;
using Bitrix.Security;
using Bitrix.Main;

public partial class bitrix_admin_MailerLog : BXAdminPage
{
    bool hasFatalErrors = false;
    protected bool HasFatalErrors
    {
        get { return hasFatalErrors; }
    }

    protected override string BackUrl
    {
        get
        {
            return "MailerLog.aspx";
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        MasterTitle = GetMessageRaw("Message.SendStatus");
        Page.Title = GetMessageRaw("Message.SendStatus");

        base.OnLoad(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ItemGrid.MultiOperationActionRun += new EventHandler<UserMadeChoiceEventArgs>(ItemGrid_MultiOperationActionRun);
    }

    protected void ItemGrid_MultiOperationActionRun(object sender, UserMadeChoiceEventArgs e)
    {
        if (e.ApplyForAll)
        {
			if (e.CommandOfChoice.CommandName == "SendSelectItems")
			{
				var items = BXMailerEventEntity.GetList(new BXFilter(ItemFilter.CurrentFilter, BXMailerEventEntity.Fields), null);
				BXMailerEventEntity.SetStatus(BXMailerEventStatus.NotProcessed);
			}
			else if (e.CommandOfChoice.CommandName == "DeleteSelectItems")
			{
				var items = BXMailerEventEntity.GetList(new BXFilter(ItemFilter.CurrentFilter, BXMailerEventEntity.Fields), null);
				foreach (var item in items)
					item.Delete();
			}
        }
        else
        {
			if (e.CommandOfChoice.CommandName == "SendSelectItems")
			{
				var indexRows = ItemGrid.GetSelectedRowsIndices();
				string ids = "";
            
				foreach (int index in indexRows)
					ids += ItemGrid.DataKeys[index].Value.ToString() + ",";

				var chArray = new char[] { ',' };
				BXMailerEventEntity.SetStatus(BXMailerEventStatus.NotProcessed, ids.TrimEnd(chArray).Split(chArray));
			}
			else if (e.CommandOfChoice.CommandName == "DeleteSelectItems")
			{
				var indexRows = ItemGrid.GetSelectedRowsIndices();
				List<string> ids = new List<string>();

				foreach (int index in indexRows)
					ids.Add(ItemGrid.DataKeys[index].Value.ToString());

				var items = BXMailerEventEntity.GetList(new BXFilter(new BXFilterItem(BXMailerEventEntity.Fields.Id, BXSqlFilterOperators.In, ids.ToArray())), null);

				foreach (var item in items)
					item.Delete();
			}
        }

        Redirect(BackUrl);
    }


    protected void ItemGrid_Select(object sender, BXSelectEventArgs e)
    {
        List<LogWrapper> list = new List<LogWrapper>();

        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXMailerEventEntity.Fields);

        BXMailerEventEntityCollection subscribePostingList = BXMailerEventEntity.GetList(
            f,
			new BXOrderBy(BXMailerEventEntity.Fields, string.IsNullOrEmpty(e.SortExpression) ? "LastUpdated DESC" : e.SortExpression),
            null,
            new BXQueryParams(e.PagingOptions),
            BXTextEncoder.HtmlTextEncoder
        );

        foreach (BXMailerEventEntity item in subscribePostingList)
            list.Add(new LogWrapper(item, this));

        e.Data = list;
    }

    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        e.Count = BXMailerEventEntity.Count(new BXFilter(ItemFilter.CurrentFilter, BXMailerEventEntity.Fields));
    }

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
    }

    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
    {
    }
}

public class LogWrapper
{
    BXMailerEventEntity _element;
    BXAdminPage _parentPage;
    public LogWrapper(BXMailerEventEntity element, BXAdminPage parentPage)
    {
        if (element == null)
            throw new ArgumentNullException("charge");

        if (parentPage == null)
            throw new ArgumentNullException("parentPage");

        _element = element;
        _parentPage = parentPage;
    }

    public string Id
    {
        get { return _element.Id.ToString(); }
    }

    public string SiteId 
    { 
        get { return _parentPage.Encode(_element.Site ?? ""); }
    }

    public string Subject
    {
        get 
        {
            if (!string.IsNullOrEmpty(_element.Template))
            {
                if (currentMessage == null && currentTemplate == null)
                {
                    var templates = BXMailerTemplate.GetList(new BXFilter(new BXFilterItem(BXMailerTemplate.Fields.Name, BXSqlFilterOperators.Equal, _element.Template)), null);
                    if (templates.Count > 0)
                    {
                        currentTemplate = templates[0];
                        currentMessage = currentTemplate.GenerateMessage(CreateDefaultParameters(_element.Site), _element.Parameters, null);
                    }
                }
                return _parentPage.Encode(currentMessage.Subject);
            }
            return _parentPage.Encode(_element.TemplateData.GetString("subject"));
        }
    }

    public string DateUpdate
    {
        get { return _element.LastUpdated.ToShortDateString() + " " + _element.LastUpdated.ToShortTimeString(); }
    }

    public string TemplateName
    {
        get { return string.IsNullOrEmpty(_element.Template) == false ? string.Format(@"<a target=""_blank"" href=""Mailer.aspx?filter_name={0}"">{0}</a>", _parentPage.Encode(_element.Template)) : "&nbsp;"; }
    }

    public string Status
    {
		get { return _parentPage.GetMessageRaw(BXMailerEvent.StatusToChar(_element.Status)); }
    }

	public string StatusId
	{
		get { return BXMailerEvent.StatusToChar(_element.Status); }
	}


    public string Duplicate
    {
        get { return _element.SendDuplicate ? _parentPage.GetMessageRaw("Kernel.Yes") : _parentPage.GetMessageRaw("Kernel.No"); }
    }

    public string EmailTo
    {
        get
        {
            if (!string.IsNullOrEmpty(_element.Template))
            {
                if (currentMessage == null && currentTemplate == null)
                {
                    var templates = BXMailerTemplate.GetList(new BXFilter(new BXFilterItem(BXMailerTemplate.Fields.Name, BXSqlFilterOperators.Equal, _element.Template)), null);
                    if (templates.Count > 0)
                    {
                        currentTemplate = templates[0];
                        currentMessage = currentTemplate.GenerateMessage(CreateDefaultParameters(_element.Site), _element.Parameters, null);
                    }
                }
                return _parentPage.Encode(string.Join(", ", currentMessage.To.Select(s => s.Address).ToArray()));
            }
            return _parentPage.Encode(_element.TemplateData.GetString("EmailTo"));
        }
    }

    private System.Net.Mail.MailMessage currentMessage { get; set; }
    private BXMailerTemplate currentTemplate { get; set; }

    private NameValueCollection CreateDefaultParameters(string siteId)
    {
        NameValueCollection defParams = new NameValueCollection();
        BXSite site = siteId != null ? BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder) : BXSite.DefaultSite;
        defParams.Add("DEFAULT_EMAIL_FROM", string.IsNullOrEmpty(BXOptionManager.GetOptionString("main", "MailerDefaultEmailFrom", String.Empty)) ? "admin@example.com" : BXOptionManager.GetOptionString("main", "MailerDefaultEmailFrom", String.Empty));
        defParams.Add("SITE_NAME", site.SiteName);
        defParams.Add("SERVER_NAME", site.ServerName);
        string path = VirtualPathUtility.RemoveTrailingSlash(HostingEnvironment.ApplicationVirtualPath);
        if (path == "/")
            path = "";
        defParams.Add("APPLICATION_PATH", path);
        return defParams;
    }
}