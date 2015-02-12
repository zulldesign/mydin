using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Services.Rating;
using Bitrix.Services;
using System.Linq;

public partial class bitrix_admin_RatingList : BXAdminPage
{

    protected override void OnInit(EventArgs e)
    {

        if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.RatingManagement))
            BXAuthentication.AuthenticationRequired();

		PopupPanelView.Commands.First(x => x.UserCommandId == "recalculate").OnClickScript = string.Concat(
			"window.location.href = 'RatingList.aspx?action=recalculate&id=' + UserData['ID'] +'&" 
			+ BXCsrfToken.BuildQueryStringPair() 
			+ "'; return false;"
		);

        if (string.Equals(Request["action"], "recalculate") && BXCsrfToken.CheckTokenFromRequest(Request.QueryString))
        {
            int id = 0;
            if (int.TryParse(Request["id"], out id) && id > 0)
            {
                BXRating.Recalculate(id);
                Response.Redirect(VirtualPathUtility.ToAbsolute(Request.Path));
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
        //if (!BXPrincipal.Current.IsCanOperate(""))
        //    return;

        List<RatingWrapper> list = new List<RatingWrapper>();

        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXRating.Fields);

        BXSelect s = new BXSelect(
            BXSelectFieldPreparationMode.Normal,
            BXRating.Fields.Id,
            BXRating.Fields.Name,
            BXRating.Fields.Created,
            BXRating.Fields.LastModified,
            BXRating.Fields.LastCalculated,
            BXRating.Fields.IsUnderCalculation,
            BXRating.Fields.Active,
            BXRating.Fields.BoundEntityTypeId,
            BXRating.Fields.CalculationMethod,
            BXRating.Fields.RefreshMethod,
            BXRating.Fields.XmlId
            );

        BXRatingCollection col = BXRating.GetList(
            f,
            new BXOrderBy(BXRating.Fields, string.IsNullOrEmpty(e.SortExpression) ? "Id" : e.SortExpression),
            s,
            new BXQueryParams(e.PagingOptions),
            BXTextEncoder.EmptyTextEncoder
            );

        foreach (BXRating item in col)
            list.Add(new RatingWrapper(item, this));

        e.Data = list;
    }


    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        e.Count = BXRating.Count(new BXFilter(ItemFilter.CurrentFilter, BXRating.Fields));
    }

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        RatingWrapper wrapper = (RatingWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);
    }


    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
    {
        //if (!BXPrincipal.Current.IsCanOperate())
        //    return;
        BXRatingCollection col;
        try
        {
            BXFilter filter = (e.Keys == null)
                ? new BXFilter(ItemFilter.CurrentFilter, BXRating.Fields)
                : new BXFilter(new BXFilterItem(BXRating.Fields.Id, BXSqlFilterOperators.Equal, e.Keys["ID"]));
            col = BXRating.GetList(filter, null);
        }
        catch (Exception ex)
        {
            ErrorMessage.AddErrorMessage(ex.Message);
            return;
        }
        bool errorTextAdded = false;
        foreach (BXRating item in col)
        {
            try
            {
                item.Delete();
                e.DeletedCount++;
            }
            catch (Exception ex2)
            {
                if (!errorTextAdded)
                {
                    ErrorMessage.AddErrorMessage(GetMessage("Rating.DeleteFailure"));
                    errorTextAdded = true;
                }
            }
        }

    }
}

public class RatingWrapper
{
    private BXRating charge = null;
    private BXAdminPage parentPage = null;
    public RatingWrapper(BXRating charge, BXAdminPage parentPage)
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

    public string Name
    {
        get { return this.charge.Name; }
    }

    public string Created
    {
        get { return this.charge.Created.ToString("g"); }
    }

    public string LastModified
    {
        get { return this.charge.LastModified.ToString("g"); }
    }

    public string Active
    {
        get { return this.charge.Active ? this.parentPage.GetMessageRaw("Kernel.Yes") : this.parentPage.GetMessageRaw("Kernel.No"); }
    }

    public string BoundEntityTypeId
    {
        get { return this.charge.BoundEntityTypeId; }
    }

    public string CalculationMethod
    {
        get 
        {
            return this.parentPage.GetMessageRaw(
                Enum.IsDefined(typeof(BXRatingCalculationMethod), this.charge.CalculationMethod) ? 
                string.Concat("RatingCalculationMethod.", this.charge.CalculationMethod.ToString("G")) :
                "Unknown.Masculine"
                );
        }
    }

    public string RefreshMethod
    {
        get
        {
            return this.parentPage.GetMessageRaw(
                Enum.IsDefined(typeof(BXRatingRefreshMethod), this.charge.RefreshMethod) ?
                string.Concat("RatingRefreshMethod.", this.charge.RefreshMethod.ToString("G")) :
                "Unknown.Masculine"
                );
        }
    }

    public string LastCalculated
    {
        get { return this.charge.LastCalculated.ToString("g"); }
    }

    public string Status
    {
        get 
        {
            if (this.charge.IsUnderCalculation)
                return this.parentPage.GetMessage("RatingStatus.UnderUpdating");

            if (this.charge.Created == this.charge.LastCalculated)
                return this.parentPage.GetMessage("RatingStatus.WaitingForUpdating");

            int countersToCalculate = BXRatingCounter.Count(
                new BXFilter(
                    new BXFilterItem(BXRatingCounter.Fields.RatingId, BXSqlFilterOperators.Equal, this.charge.Id),
                    new BXFilterItem(BXRatingCounter.Fields.IsCalculated, BXSqlFilterOperators.Equal, 0)
                    )
                );

            return countersToCalculate > 0 ? this.parentPage.GetMessage("RatingStatus.WaitingForUpdating") : this.parentPage.GetMessage("RatingStatus.Calculated");
        }
    }

    public string XmlId
    {
        get { return this.charge.XmlId; }
    }

}
