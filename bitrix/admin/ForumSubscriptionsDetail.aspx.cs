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
using System.Collections.Generic;
using Bitrix.UI;
using Bitrix.Forum;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix;
using System.Text;
using Bitrix.Security;

public partial class BXForumAdminPageForumSubscriptionsDetail : BXAdminPage
{
    BXFilter currentFilter;
    int userId;
    Dictionary<string, string> sites;

    protected int UserId
    {
        get
        { 
            if ( userId!=0 ) return userId;
            if ( Request.QueryString["id"]==null) return userId = -1;
            int tmp;
            if ( Int32.TryParse(Request.QueryString["id"],out tmp))
                return userId = tmp;
            return userId=-1;
        }
    }

    protected class BXSubscriptionsSelect
    {
        int id;
        String forumName;
        String topicName;
        bool onlyTopic;
        DateTime dateStart;
        String siteId;
        String subscriptionTypeMessage;

        public BXSubscriptionsSelect(int id,String forumName,String topicName,bool onlyTopic,DateTime dateStart,String siteId,String subscriptionTypeMessage)
        {
            this.id = id;
            this.forumName = forumName;
            this.topicName = topicName;
            this.onlyTopic = onlyTopic;
            this.dateStart = dateStart;
            this.siteId = siteId;
            this.subscriptionTypeMessage = subscriptionTypeMessage;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public String ForumName
        {
            get { return forumName; }
            set { forumName = value; }
        }

        public String TopicName
        {
            get { return topicName; }
            set { topicName = value; }
        }

        public bool OnlyTopic
        {
            get { return onlyTopic; }
            set { onlyTopic=value; }
        }

        public DateTime DateStart
        {
            get { return dateStart; }
            set { dateStart = value; }
        }

        public String SiteId
        {
            get { return siteId; }
            set { siteId = value; }
        }

        public String SubscriptionTypeMessage
        {
            get
            {
                return subscriptionTypeMessage;
            }
            set
            {
                subscriptionTypeMessage = value;
            }
        }

    }

    protected void Page_Init(object sender, EventArgs e)
    {
        MasterTitle = GetMessage("PageTitle");
        BXSiteCollection siteColl = BXSite.GetList(null, null, null, null, BXTextEncoder.EmptyTextEncoder);
        sites = new Dictionary<string,string>();
        foreach (BXSite site in siteColl)
        {
            siteIdFilter.Values.Add(new ListItem(site.Name, site.Id));
            sites.Add(site.Id, site.Name);
        }
        if ( UserId<= 0 )
            ErrorMessage.AddErrorMessage(GetMessageRaw("Error.UserNotSpecified"));
    }

    protected void BXSubscriptionsGrid_Select(object sender, BXSelectEventArgs e)
    {
        if (UserId <= 0)
        {
            e.Data = new List<BXSubscriptionsSelect>();
            return;
        }
        currentFilter = new BXFilter(new BXFilterItem(BXForumSubscription.Fields.Subscriber.Id,BXSqlFilterOperators.Equal,UserId));
        currentFilter.Add(new BXFilter(BXSubscriptionsFilter.CurrentFilter, BXForumSubscription.Fields));
        //BXSubscriptionsFilter.CurrentFilter.con
        BXOrderBy orderBy;

        if (String.IsNullOrEmpty(e.SortExpression))
            orderBy = new BXOrderBy(new BXOrderByPair(BXForumSubscription.Fields.Id, BXOrderByDirection.Asc));
        else
            orderBy = new BXOrderBy(BXForumSubscription.Fields, e.SortExpression);

        BXForumSubscriptionCollection subs = BXForumSubscription.GetList(
                                                            currentFilter,
                                                            orderBy,
                                                            new BXSelectAdd(BXForumSubscription.Fields.Topic,BXForumSubscription.Fields.Forum),
                                                            new BXQueryParams(e.PagingOptions),
                                                            BXTextEncoder.EmptyTextEncoder
                                                        );
        List<BXSubscriptionsSelect> list = subs.ConvertAll<BXSubscriptionsSelect>(delegate(BXForumSubscription input)
            {
                string siteName = String.Empty;
                if (sites.ContainsKey(input.SiteId)) siteName = sites[input.SiteId];
                else throw new KeyNotFoundException("site is not found");
            return new BXSubscriptionsSelect(   
                                                input.Id, input.Forum!=null ? input.Forum.Name:"", 
                                                input.Topic!=null? input.Topic.Name:"", input.OnlyTopic, 
                                                input.DateStart, siteName,
                                                GetMessageRaw(input.TopicId>0? "ColumnText.Topic" : input.OnlyTopic ? "ColumnText.OnlyTopic" : "ColumnText.FullForum")
                                            );

            }
        );

        e.Data = list;

    }
    protected void BXSubscriptionsGrid_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        if (UserId <= 0)
        {
            e.Count = 0;
            return;
        }
        currentFilter = new BXFilter(new BXFilterItem(BXForumSubscription.Fields.Subscriber.Id,BXSqlFilterOperators.Equal,UserId));
        currentFilter.Add(new BXFilter(BXSubscriptionsFilter.CurrentFilter, BXForumSubscription.Fields));
        e.Count = BXForumSubscription.Count(currentFilter);
    }

    protected void BXSubscriptionsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        BXSubscriptionsSelect select = (BXSubscriptionsSelect)row.DataItem;
        row.UserData.Add("ID", select.Id);
    }

    protected void BXSubscriptionsGrid_Delete(object sender, BXDeleteEventArgs e)
    {
        if (e.Keys != null)
        {
            int[] ids = new int[e.Keys.Count];
            e.Keys.Values.CopyTo(ids, 0);
            BXForumSubscriptionCollection subs = BXForumSubscription.GetList(
                                                    new BXFilter(
                                                        new BXFilterItem(
                                                            BXForumSubscription.Fields.Id, BXSqlFilterOperators.In, ids
                                                            )
                                                        )
                                                   , null);

            foreach (BXForumSubscription sub in subs)
            {
                if (sub == null) continue;
                try
                {
                    sub.Delete();
                }
                catch (Exception ex)
                {
                    ErrorMessage.AddErrorMessage(ex.Message);
                }
                e.DeletedCount++;
            }
        }
    }


}