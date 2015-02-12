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

public partial class bitrix_admin_VotingView : BXAdminPage
{
    protected bool errorOnPage = false;
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
                    _id = Convert.ToInt32(IdStr);
                }
                catch
                {
                    _id = 0;
                }
            }
            return _id.Value;
        }
    }

    private BXRatingVoting _voting;
    protected BXRatingVoting Voting
    {
        get
        {
            return _voting;
        }
    }


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

        TabControl.ButtonsMode = BXTabControl.ButtonsModeType.Hidden;

        TryLoadVoting();
        
        string title = string.Format(this.GetMessage("PageTitle"), Id.ToString());
        MasterTitle = title;
        Page.Title = title;

        base.OnLoad(e);
    }

    private void TryLoadVoting()
    {
        int id = Id;
        if (id <= 0)
        {
            errorMessage.AddErrorMessage(GetMessage("Error.UnableToFindVoting"));
            errorOnPage = true;
            return;
        }
        if ((_voting = BXRatingVoting.GetById(id, BXTextEncoder.EmptyTextEncoder)) == null)
        {
            errorMessage.AddErrorMessage(GetMessage("Error.UnableToFindVoting"));
            errorOnPage = true;
            return;
        }

        VotingTypeName.Text = _voting.RatedItem.TypeName;
        VotingTypeName.Enabled = false;
        //VotingSubject.Text = votingSubject;

        VotingTypeName.Text = this.GetMessage("VotingEntityType." + _voting.RatedItem.TypeName);
        VotingSubject.TypeName = _voting.RatedItem.TypeName;
        VotingSubject.ItemId = _voting.RatedItem.Identity;
        VotingSubject.DataBind();

        VotingCreatedUtc.Text = _voting.CreatedUtc.ToLocalTime().ToString();
        VotingCreatedUtc.Enabled = false;

        VotingLastCalculatedUtc.Text = _voting.LastCalculatedUtc.ToLocalTime().ToString();
        VotingLastCalculatedUtc.Enabled = false;

        VotingTotalNegativeVotes.Text = _voting.TotalNegativeVotes.ToString();
        VotingTotalNegativeVotes.Enabled = false;

        VotingTotalPositiveVotes.Text = _voting.TotalPositiveVotes.ToString();
        VotingTotalPositiveVotes.Enabled = false;

        VotingTotalVotes.Text = _voting.TotalVotes.ToString();
        VotingTotalVotes.Enabled = false;

        VotingTotalValue.Text = _voting.TotalValue.ToString();
        VotingTotalValue.Enabled = false;
    }


    protected void OnToolBarButtonClick(object sender, CommandEventArgs e)
    {
        if (e.CommandName == "delete")
        {
            try
            {
                BXRatingVoting.Delete(Id);
                GoBack("~/bitrix/admin/VotingList.aspx");
            }
            catch
            {
                errorMessage.AddErrorMessage(GetMessage("Error.DeleteFailture"));
            }
        }
        else if (e.CommandName == "recalculate")
        {
            try
            {
                _voting.Calculate(true);
            }
            catch
            {
                errorMessage.AddErrorMessage(GetMessage("Error.CalculateFailture"));
            }
        }
    }
}