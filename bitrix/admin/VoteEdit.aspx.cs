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
using Bitrix.Services.Text;
using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Modules;
using System.Collections.Generic;
using Bitrix.Security;
using Bitrix.Configuration;
using Bitrix.CommunicationUtility.Rating;

public partial class bitrix_admin_VoteEdit : BXAdminPage
{
    protected bool ErrorsOnPage = false;

    private int? _id = null;
    protected int Id
    {
        get
        {
            if (_id.HasValue)
                return _id.Value;

            string IdStr = Request.QueryString["id"];
            if (string.IsNullOrEmpty(IdStr))
                _id = 0;
            else
            {
                try
                {
                    _id = Convert.ToInt32(Request.QueryString["id"]);
                }
                catch (Exception /*exc*/)
                {
                    _id = 0;
                }
            }
            return _id.Value;
        }
    }

    private BXRatingVote _vote;


    private string _errorMessage = string.Empty;
    protected string ErrorMessage
    {
        get { return _errorMessage; }
    }

    protected override string BackUrl
    {
        get { return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "VotingList.aspx"; }
    }

    protected override void OnInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRatingVoting.Operations.RatingVotingAdminManagement))
            BXAuthentication.AuthenticationRequired();

        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        string title = this.GetMessage("PageTitle");
        MasterTitle = title;
        Page.Title = title;
        if (!TryLoadVoting()) return;

        base.OnLoad(e);
    }

    private bool TryLoadVoting()
    {
        int id = Id;
        if (id <= 0)
        {
            errorMessage.AddErrorMessage(GetMessage("Error.VoteSelect"));
            ErrorsOnPage = true;
            return false;
        }
        if ((_vote = BXRatingVote.GetById(id, BXTextEncoder.EmptyTextEncoder)) == null)
        {
            errorMessage.AddErrorMessage(GetMessage("Error.VoteSelect"));
            ErrorsOnPage = true;
            return false;
        }

        BXRatingVoting _voting = BXRatingVoting.GetById(_vote.RatingVotingId);

        VotingSubject.ItemId = _voting.RatedItem.Identity;
        VotingSubject.TypeName = _voting.RatedItem.TypeName;
        VotingSubject.DisplayTypeName = true;
        VotingSubject.DataBind();

        //VoteTypeName.Text = this.GetMessage("VotingEntityType."+_voting.RatedItem.TypeName);

        VoteRatingVotingId.Text = @"<a href='VotingView.aspx?id="+_vote.RatingVotingId.ToString()+"'>"
            +_vote.RatingVotingId.ToString()+"</a>";

        VoteCreatedUtc.Text = _vote.CreatedUtc.ToLocalTime().ToString();

        VoteUser.UserID = _vote.UserId.ToString();

        if (!IsPostBack)
        {
            VoteValue.Text = _vote.Value.ToString();
        }
        return true;
    }

    protected void OnVoteEdit(object sender, BXTabControlCommandEventArgs e)
    {

        if (e.CommandName == "save")
        {
            double vv =0;
            double.TryParse(VoteValue.Text, out vv);
            _vote.Value = vv;
            _vote.Update();
            GoBack();
        }
        else if (e.CommandName == "apply")
        {
            int val = 0;
            int.TryParse(VoteValue.Text, out val);
            _vote.Value = val;
            _vote.Update();
            _vote.Save();
        }
        else
            GoBack();
    }

    protected void OnToolBarButtonClick(object sender, CommandEventArgs e)
    {
        if (e.CommandName == "delete")
        {
            try
            {
                BXRatingVoting v = BXRatingVoting.GetById(_vote.RatingVotingId);
                BXRatingVote.Delete(Id);
                v.Calculate(true);
                GoBack("~/bitrix/admin/VoteList.aspx");
            }
            catch (Exception ex)
            {
                errorMessage.AddErrorMessage(GetMessage("Error.DeleteFailture"));
            }
        }
    }

    private BXRatingVoting _voting;
}
