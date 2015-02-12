using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Services.Promo;
using Bitrix.DataLayer;
using Bitrix.Security;
using Bitrix.Services.Text;

public partial class bitrix_admin_PromoRuleList : BXAdminPage
{
    protected override void OnInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.PromotionManagement))
            BXAuthentication.AuthenticationRequired();

        if (string.Equals(Request["action"], "apply"))
        {
            int id = 0;
            if (int.TryParse(Request["id"], out id) && id > 0)
            {
                BXPromoRule.Apply(id);
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
        List<PromoRuleWrapper> list = new List<PromoRuleWrapper>();

        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXPromoRule.Fields);

        BXSelect s = new BXSelect(
            BXSelectFieldPreparationMode.Normal,
            BXPromoRule.Fields.Id,
            BXPromoRule.Fields.Name,
            BXPromoRule.Fields.CreatedUtc,
            BXPromoRule.Fields.LastModifiedUtc,
            BXPromoRule.Fields.LastAppliedUtc,
            BXPromoRule.Fields.Active,
            BXPromoRule.Fields.BoundEntityTypeId,
            BXPromoRule.Fields.XmlId
            );

        BXPromoRuleCollection col = BXPromoRule.GetList(
            f,
            new BXOrderBy(BXPromoRule.Fields, string.IsNullOrEmpty(e.SortExpression) ? "Id" : e.SortExpression),
            s,
            new BXQueryParams(e.PagingOptions),
            BXTextEncoder.EmptyTextEncoder
            );

        foreach (BXPromoRule item in col)
            list.Add(new PromoRuleWrapper(item, this));

        e.Data = list;
    }


    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        e.Count = BXPromoRule.Count(new BXFilter(ItemFilter.CurrentFilter, BXPromoRule.Fields));
    }

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        PromoRuleWrapper wrapper = (PromoRuleWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);
    }


    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
    {
        BXPromoRuleCollection col;
        try
        {
            BXFilter filter = (e.Keys == null)
                ? new BXFilter(ItemFilter.CurrentFilter, BXPromoRule.Fields)
                : new BXFilter(new BXFilterItem(BXPromoRule.Fields.Id, BXSqlFilterOperators.Equal, e.Keys["ID"]));
            col = BXPromoRule.GetList(filter, null);
        }
        catch (Exception ex)
        {
            ErrorMessage.AddErrorMessage(ex.Message);
            return;
        }
        bool errorTextAdded = false;
        foreach (BXPromoRule item in col)
        {
            try
            {
                item.Delete();
                e.DeletedCount++;
            }
            catch
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

public class PromoRuleWrapper
{
    private BXPromoRule charge = null;
    private BXAdminPage parentPage = null;
    public PromoRuleWrapper(BXPromoRule charge, BXAdminPage parentPage)
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
        get { return this.charge.CreatedUtc.ToLocalTime().ToString("g"); }
    }

    public string LastModified
    {
        get { return this.charge.LastModifiedUtc.ToLocalTime().ToString("g"); }
    }

    public string Active
    {
        get { return this.charge.Active ? this.parentPage.GetMessageRaw("Kernel.Yes") : this.parentPage.GetMessageRaw("Kernel.No"); }
    }

    public string BoundEntityTypeId
    {
        get { return this.charge.BoundEntityTypeId; }
    }


    public string LastApplied
    {
        get { return this.charge.LastAppliedUtc.HasValue ? this.charge.LastAppliedUtc.Value.ToLocalTime().ToString("g") : string.Empty; }
    }

    public string Status
    {
        get
        {
            if (BXPromoRule.IsPending(this.charge.Id))
                return this.parentPage.GetMessage("Status.Pending");

            return this.parentPage.GetMessage(this.charge.LastAppliedUtc.HasValue ? "Status.Applied" : "Status.NotApplied");
        }
    }

    public string XmlId
    {
        get { return this.charge.XmlId; }
    }

}


