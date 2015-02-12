using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.CommunicationUtility;
using Bitrix.CommunicationUtility.Rating;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Services.Rating;
using Bitrix.Services;

public partial class bitrix_admin_VoteList : BXAdminPage
{

    protected override void OnInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRatingVoting.Operations.RatingVotingAdminManagement))
            BXAuthentication.AuthenticationRequired();

        PageMode = VotingId > 0 ? BXVoteListPageMode.StaticFilter : BXVoteListPageMode.Standard;

        base.OnInit(e);
    }

    protected override void OnLoadComplete(EventArgs e)
    {
        if (VotingId > 0)
        {
            ItemFilter.CurrentFilter.Clear();
            ItemFilter.CurrentFilter.Add(new BXFormFilterItem("RatingVotingId", VotingId, BXSqlFilterOperators.Equal));
            ((TextBox)RatingVotingId.Controls[0]).Text = VotingId.ToString();

            //foreach (BXFormFilterItem fi in ItemFilter.CurrentFilter)
            //{ 
            //    if (fi.filterName=="RatingVotingId" && (int)fi.filterValue==VotingId)
            //    {
            //        ItemFilter.CurrentFilter.Add(new BXFormFilterItem("RatingVotingId", VotingId, BXSqlFilterOperators.Equal));
            //        TextBox tb = (TextBox)RatingVotingId.Controls[0];
            //        tb.Text = VotingId.ToString();
            //    }
            //}
            if (PositiveVotes)
                ItemFilter.CurrentFilter.Add(new BXFormFilterItem("Value", 0, BXSqlFilterOperators.Greater));
            else if (NegativeVotes)
                ItemFilter.CurrentFilter.Add(new BXFormFilterItem("Value", 0, BXSqlFilterOperators.Less));
        }

        Go2ListAll.Href = "VoteList.aspx";
        Go2List.Href = string.Concat("VoteList.aspx?VotingId=", VotingId.ToString());
        Go2YListButton.Href = string.Concat(Go2List.Href, "&PositiveVotes=Y");
        Go2NListButton.Href = string.Concat(Go2List.Href, "&NegativeVotes=Y");

        if (PageMode == BXVoteListPageMode.Standard)
        {
            MasterTitle = Page.Title = GetMessage("Standard.PageTitle");
            Go2YListButton.Visible = Go2NListButton.Visible = Go2List.Visible = Go2ListAll.Visible = false;
        }
        else
        {
            if (PositiveVotes)
            {
                MasterTitle = Page.Title = string.Format(GetMessage("StaticFilter.Y.PageTitle"), VotingId.ToString());
                Go2YListButton.Visible = false;
            }
            else if (NegativeVotes)
            {
                MasterTitle = Page.Title = string.Format(GetMessage("StaticFilter.N.PageTitle"), VotingId.ToString());
                Go2NListButton.Visible = false;
            }
            else
            {
                MasterTitle = Page.Title = string.Format(GetMessage("StaticFilter.All.PageTitle"), VotingId.ToString());
                Go2List.Visible = false;
            }
            ItemFilter.Visible = false;
        }

        base.OnLoadComplete(e);
    }

    protected override void OnPreRender(EventArgs e)
    {
        BXPage.Scripts.RequireUtils();
        User.AttachScript();
        ClientScript.RegisterStartupScript(GetType(), "PrepareGridLegend", "Bitrix.VoteListGridLegend.prepare();", true);

        base.OnPreRender(e);
    }

    protected void ItemGrid_Select(object sender, BXSelectEventArgs e)
    {
        //if (!BXPrincipal.Current.IsCanOperate(""))
        //    return;

        List<RatingVoteWrapper> list = new List<RatingVoteWrapper>();

        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXRatingVote.Fields);

        BXSelect s = new BXSelect(
            BXSelectFieldPreparationMode.Normal,
            BXRatingVote.Fields.Id,
            BXRatingVote.Fields.Active,
            BXRatingVote.Fields.CreatedUtc,
            BXRatingVote.Fields.RatingVotingId,
            BXRatingVote.Fields.UserId,
            BXRatingVote.Fields.Value,
            BXRatingVote.Fields.CustomFields);

        BXRatingVoteCollection col = BXRatingVote.GetList(
            f,
            new BXOrderBy(BXRatingVote.Fields, string.IsNullOrEmpty(e.SortExpression) ? "RatingVotingId DESC" : e.SortExpression),
            s,
            new BXQueryParams(e.PagingOptions),
            BXTextEncoder.EmptyTextEncoder
        );

        foreach (BXRatingVote item in col)
            list.Add(new RatingVoteWrapper(item, this));
        e.Data = list;
    }

    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        e.Count = BXRatingVote.Count(new BXFilter(ItemFilter.CurrentFilter, BXRatingVote.Fields));
    }

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        RatingVoteWrapper wrapper = (RatingVoteWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);
    }

    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
    {
        //if (!BXPrincipal.Current.IsCanOperate())
        //    return;
        BXRatingVoteCollection col;
        try
        {
            BXFilter filter = (e.Keys == null)
                ? new BXFilter(ItemFilter.CurrentFilter, BXRatingVote.Fields)
                : new BXFilter(new BXFilterItem(BXRatingVote.Fields.Id, BXSqlFilterOperators.Equal, e.Keys["ID"]));
            col = BXRatingVote.GetList(filter, null);
            
        }
        catch
        {
            ErrorMessage.AddErrorMessage(GetMessage("Error.DeleteFailture"));
            return;
        }
        bool errorTextAdded = false;
        foreach (BXRatingVote item in col)
        {
            try
            {
                BXRatingVoting v = BXRatingVoting.GetById(item.RatingVotingId);
                item.Delete();
                v.Calculate(true);
                e.DeletedCount++;
            }
            catch
            {
                if (!errorTextAdded)
                {
                    ErrorMessage.AddErrorMessage(GetMessage("Error.DeleteFailture"));
                    errorTextAdded = true;
                }
            }
        }

    }

    private int? votingId = null;
    public int VotingId
    {
        get 
        {
            if(votingId.HasValue)
                return votingId.Value;

            int i;
            return (votingId = !string.IsNullOrEmpty(Request["VotingId"]) && int.TryParse(Request["VotingId"].ToString(), out i) ? i : 0).Value;
        }
    }

    private bool? positiveVotes = null;
    public bool PositiveVotes
    {
        get { return (positiveVotes ?? (positiveVotes = string.Equals(Request["PositiveVotes"], "Y", StringComparison.OrdinalIgnoreCase))).Value; }
    }

    private bool? negativeVotes = null;
    public bool NegativeVotes
    {
        get { return (negativeVotes ?? (negativeVotes = string.Equals(Request["NegativeVotes"], "Y", StringComparison.OrdinalIgnoreCase))).Value; }
    }    

    private BXVoteListPageMode pageMode = BXVoteListPageMode.Standard;
    public BXVoteListPageMode PageMode
    {
        get { return pageMode; }
        private set { pageMode = value; }
    }
}

public enum BXVoteListPageMode
{
    Standard = 1,
    StaticFilter = 2
}

public class RatingVoteWrapper
{
    private BXRatingVote charge = null;
    private BXAdminPage parentPage = null;
    private BXRatingVoting voting = null;
    public RatingVoteWrapper(BXRatingVote charge, BXAdminPage parentPage)
    {
        if (charge == null)
            throw new ArgumentNullException("charge");
        this.charge = charge;

        if (parentPage == null)
            throw new ArgumentNullException("parentPage");
        this.parentPage = parentPage;
    }

    public string ID
    {
        get { return this.charge.Id.ToString(); }
    }


    public string Created
    {
        get { return this.charge.CreatedUtc.ToLocalTime().ToString("g"); }
    }

    public string VotingId
    {
        get
        {
            return String.Format("<a href=\"VotingView.aspx?id={0} \">{1}</a>", this.charge.RatingVotingId, this.charge.RatingVotingId);
        }
    }

    public string VoteValue
    {
        get
        {
            return this.charge.Value.ToString();
        }
    }

    public BXRatingVoting Voting
    {
        get
        {
            return BXRatingVoting.GetById(this.charge.RatingVotingId);
        }
    }

    public string UserId
    {
        get
        {
            return this.charge.UserId.ToString();
        }
    }

    public string XmlId
    {
        get { return this.charge.XmlId; }
    }

}
