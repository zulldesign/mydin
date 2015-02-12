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
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix;
using System.Text;
using Bitrix.Security;
using Bitrix.Advertising;

public partial class bitrix_admin_AdvertisingSpaceList : BXAdminPage
{
    protected override void OnPreInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.SpaceManagement))
            BXAuthentication.AuthenticationRequired();

        base.OnPreInit(e);
    }


    protected override void OnLoad(EventArgs e)
    {
        MasterTitle = GetMessage("PageTitle");
        Page.Title = GetMessage("PageTitle");

        base.OnLoad(e);
    }

    protected void ItemGrid_Select(object sender, BXSelectEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.SpaceManagement))
            return;

        List<AdvertisingSpaceWrapper> list = new List<AdvertisingSpaceWrapper>();

        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXAdvertisingSpace.Fields);

        BXSelect s = new BXSelect(
            BXSelectFieldPreparationMode.Normal,
            BXAdvertisingSpace.Fields.Id,
            BXAdvertisingSpace.Fields.Name,
            BXAdvertisingSpace.Fields.Description,
            BXAdvertisingSpace.Fields.Active,
            BXAdvertisingSpace.Fields.DateCreated,
            BXAdvertisingSpace.Fields.DateLastModified
            );

        BXAdvertisingSpaceCollection advSpaceList = BXAdvertisingSpace.GetList(
            f,
            new BXOrderBy(BXAdvertisingSpace.Fields, string.IsNullOrEmpty(e.SortExpression) ? "Id" : e.SortExpression),
            s,
            null,
            BXTextEncoder.EmptyTextEncoder
            );

        foreach (BXAdvertisingSpace advSpace in advSpaceList)
            list.Add(new AdvertisingSpaceWrapper(advSpace, this));

        e.Data = list;
	}


    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.SpaceManagement))
            return;
        e.Count = BXAdvertisingSpace.Count(new BXFilter(ItemFilter.CurrentFilter, BXAdvertisingSpace.Fields));
	}

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        AdvertisingSpaceWrapper wrapper = (AdvertisingSpaceWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);
    }


    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.SpaceManagement))
            return;
        BXAdvertisingSpaceCollection spaces;
		try
		{
            BXFilter filter = (e.Keys == null)
                ? new BXFilter(ItemFilter.CurrentFilter, BXAdvertisingSpace.Fields)
                : new BXFilter(new BXFilterItem(BXAdvertisingSpace.Fields.Id, BXSqlFilterOperators.Equal, e.Keys["ID"]));
            spaces = BXAdvertisingSpace.GetList(filter, null);

            
			
		}
		catch (Exception ex)
		{
			ErrorMessage.AddErrorMessage(ex.Message);
            return;
		}
        bool errorTextAdded = false;
        foreach(BXAdvertisingSpace space in spaces)
        {
            try
            {
                space.Delete();
                e.DeletedCount++;
            }
            catch (Exception ex2)
            {
                if (!errorTextAdded)
                {
                    ErrorMessage.AddErrorMessage(GetMessage("AdvertisingSpace.DeleteFailure"));
                    errorTextAdded = true;
                }
            }
            
        }
	}
}
/// <summary>
/// Обёртка для рекламной области
/// </summary>
public class AdvertisingSpaceWrapper
{
    BXAdvertisingSpace _charge;
    BXAdminPage _parentPage;
    public AdvertisingSpaceWrapper(BXAdvertisingSpace charge, BXAdminPage parentPage)
    {
        if (charge == null)
            throw new ArgumentNullException("charge");

        if (parentPage == null)
            throw new ArgumentNullException("parentPage");

        _charge = charge;
        _parentPage = parentPage;
    }

    public string ID
    {
        get { return _charge.Id.ToString(); }
    }

    public string Name
    {
        get { return _charge.Name; }
    }

    public string Description
    {
        get { return _charge.Description; }
    }

    public string Sort
    {
        get { return _charge.Sort.ToString(); }
    }

    public string DateCreated
    {
        get { return _charge.DateCreated.ToString("g"); }
    }

    public string DateLastModified
    {
        get { return _charge.DateLastModified.ToString("g"); }
    }

    public string Active
    {
        get { return _charge.Active ? _parentPage.GetMessageRaw("Kernel.Yes") : _parentPage.GetMessageRaw("Kernel.No"); }
    }

    public string BannerCount
    {
        get 
        {
            int result = BXAdvertisingBanner.Count(
                new BXFilter(
                    new BXFilterItem(
                        BXAdvertisingBanner.Fields.Space.Id, BXSqlFilterOperators.Equal, _charge.Id
                    )
                )
            );

            return result > 0 ? result.ToString() : "-"; 
        }
    }

}
