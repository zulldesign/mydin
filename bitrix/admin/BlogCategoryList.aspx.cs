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
using Bitrix.Blog;

public partial class bitrix_admin_BlogCategoryList : BXAdminPage
{
    protected override void OnPreInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
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
        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
            return;

        List<BlogCategoryWrapper> list = new List<BlogCategoryWrapper>();

        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXBlogCategory.Fields);

        BXSelect s = new BXSelect(BXSelectFieldPreparationMode.Normal,
            BXBlogCategory.Fields.Id,
            BXBlogCategory.Fields.Name,
            BXBlogCategory.Fields.Sites.Site.ID,
            BXBlogCategory.Fields.Sites.Site.Name,
            BXBlogCategory.Fields.Sort
            );

        BXBlogCategoryCollection blogCategoryList = BXBlogCategory.GetList(
            f,
            new BXOrderBy(BXBlogCategory.Fields, string.IsNullOrEmpty(e.SortExpression) ? "Id" : e.SortExpression),
            s,
            null,
            BXTextEncoder.EmptyTextEncoder
            );

        foreach (BXBlogCategory blogCategory in blogCategoryList)
            list.Add(new BlogCategoryWrapper(blogCategory, this));

        e.Data = list;
	}


    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
        e.Count = BXBlogCategory.Count(new BXFilter(ItemFilter.CurrentFilter, BXBlogCategory.Fields));
	}

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        BlogCategoryWrapper wrapper = (BlogCategoryWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);
    }


    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
            return;
		try
		{
	        BXBlogCategory.Delete(e.Keys["ID"]);
			e.DeletedCount++;
		}
		catch (Exception ex)
		{
			ErrorMessage.AddErrorMessage(ex.Message);
		}
	}
}
/// <summary>
/// Обёртка для категории блога
/// </summary>
public class BlogCategoryWrapper
{
    BXBlogCategory _charge;
    BXAdminPage _parentPage;
    public BlogCategoryWrapper(BXBlogCategory charge, BXAdminPage parentPage)
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

    public string Sort
    {
        get { return _charge.Sort.ToString(); }
    }

    public string Sites
    {
        get 
        {
            BXSiteCollection sites = _charge.Sites;
            if (sites == null)
                return "N/A";

            StringBuilder sb = new StringBuilder();
            foreach (BXSite site in sites)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(site.Name);
            }
            return sb.ToString();
        }
    }

}
