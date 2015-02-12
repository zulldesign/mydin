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

public partial class BXForumAdminPageForumSubscriptionsList : BXAdminPage
{

    protected const string UserEditLinkTemplate = "(<a title=\"#Title#\" href=\"AuthUsersEdit.aspx?id=#UserId#\">#UserName#</a>) #DisplayName#";

	protected void Page_Init(object sender, EventArgs e)
	{
        MasterTitle = GetMessage("PageTitle");

	}

    protected class SubscriptionsGridSelect
    {
        BXUser user;
        BXForumUser forumUser;
        string editUserToolTipText;

        public SubscriptionsGridSelect(BXUser user,BXForumUser forumUser,string editUserToolTipText)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (forumUser == null) throw new ArgumentNullException("forumUser");
            this.user = user;
            this.forumUser = forumUser;
            this.editUserToolTipText = editUserToolTipText;
        }

        public int UserId
        {
            get
            {
                return user.UserId;
            }
        }

        public String Email
        {
            get
            {
                return user.Email;
            }
        }

        public String FirstName
        {
            get
            {
                return user.FirstName;
            }
        }

        public String LastName
        {
            get
            {
                return user.LastName;
            }
        }

        public String UserName
        {
            get
            {
                return user.UserName;
            }
        }

        public String DisplayNameWithEditLink
        {
            get
            {
                return UserEditLinkTemplate.Replace("#UserId#", user.UserId.ToString())
                                            .Replace("#UserName#", HttpUtility.HtmlEncode(UserName))
                                            .Replace("#DisplayName#", HttpUtility.HtmlEncode(DisplayName))
                                            .Replace("#Title#",editUserToolTipText );
            }
        }

        public int SubscriptionsCount
        {
            get
            {
                return forumUser.SubscriptionsCount;

            }
        }

        public String DisplayName
        {
            get
            {
                return user.GetDisplayName();
            }

        }
    }

	protected void BXUsersGrid_Select(object sender, BXSelectEventArgs e)
	{
        BXFilter filter = new BXFilter();
        filter.Add(new BXFilter(BXUsersFilter.CurrentFilter, BXForumUser.Fields));

        BXOrderBy orderBy;

        if (String.IsNullOrEmpty(e.SortExpression))
            orderBy = new BXOrderBy(new BXOrderByPair(BXForumUser.Fields.Id, BXOrderByDirection.Asc));
        else
            orderBy = new BXOrderBy(BXForumUser.Fields, e.SortExpression);

        BXForumUserCollection subs = BXForumUser.GetList(   filter, 
                                                            orderBy,
                                                            new BXSelectAdd(BXForumUser.Fields.User,BXForumUser.Fields.SubscriptionsCount),
                                                            new BXQueryParams(e.PagingOptions),
                                                            BXTextEncoder.EmptyTextEncoder
                                                        );
        List<SubscriptionsGridSelect> list = new List<SubscriptionsGridSelect>();
        string msg = GetMessage("GridText.ToolTip.EditUser");
        foreach (BXForumUser u in subs)
            list.Add(new SubscriptionsGridSelect(u.User,u,msg));
        e.Data = list;
 
	}
	protected void BXUsersGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
        BXFilter countFilter = new BXFilter();
            countFilter.Add(new BXFilter(BXUsersFilter.CurrentFilter, BXForumUser.Fields));
		    e.Count = BXForumUser.Count(countFilter);
	}

	protected void BXUsersGrid_RowDataBound(object sender, GridViewRowEventArgs e)
	{
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        SubscriptionsGridSelect select = (SubscriptionsGridSelect)row.DataItem;

        row.UserData.Add("UserID", select.UserId);
	}

	protected void BXUsersGrid_Delete(object sender, BXDeleteEventArgs e)
	{
        //BXForumCollection forums;
       if (e.Keys != null)
        {
           int [] ids = new int[e.Keys.Count];
           e.Keys.Values.CopyTo(ids,0);
            BXForumSubscriptionCollection subs = BXForumSubscription.GetList(
                                                    new BXFilter(
                                                        new BXFilterItem(
                                                            BXForumSubscription.Fields.Subscriber.Id, BXSqlFilterOperators.In, ids
                                                            )
                                                        )
                                                   ,null);

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
