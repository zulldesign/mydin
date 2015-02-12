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
using Bitrix.CommunicationUtility;

public partial class bitrix_admin_BlogList : BXAdminPage
{
    protected override void OnPreInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
            BXAuthentication.AuthenticationRequired();

        base.OnPreInit(e);
    }

	protected override void OnInit(EventArgs e)
	{
		ItemGrid.CreateCustomColumns(BXBlog.GetCustomFieldsKey(), ItemGrid_ExtractCustomProperty);
		ItemFilter.CreateCustomFilters(BXBlog.GetCustomFieldsKey());

		base.OnInit(e);
	}

    protected override void OnLoad(EventArgs e)
    {
        MasterTitle = GetMessage("PageTitle");
        Page.Title = GetMessage("PageTitle");

        base.OnLoad(e);
    }

	private BXCustomProperty ItemGrid_ExtractCustomProperty(object dataItem, string key)
	{
		var item = (BlogWrapper)dataItem;
		return item.Charge.CustomValues[key];
	}

    protected void ItemGrid_Select(object sender, BXSelectEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
            return;

        List<BlogWrapper>  list = new List<BlogWrapper>();

        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXBlog.Fields);

        BXSelect s = new BXSelect(BXSelectFieldPreparationMode.Normal,
            BXBlog.Fields.Id,
            BXBlog.Fields.Name,
            BXBlog.Fields.DateCreated,
            BXBlog.Fields.DateLastPosted,
            BXBlog.Fields.Active,
            BXBlog.Fields.Owner,
            BXBlog.Fields.Slug,
			BXBlog.Fields.IndexContent,
			BXBlog.Fields.IsTeam,
            BXBlog.Fields.Categories.Category,
			BXBlog.Fields.CustomFields.DefaultFields
        );

        BXBlogCollection blogList = BXBlog.GetList(
            f,
            new BXOrderBy(BXBlog.Fields, string.IsNullOrEmpty(e.SortExpression) ? "Id" : e.SortExpression),
            s,
            new BXQueryParams(e.PagingOptions),
            BXTextEncoder.EmptyTextEncoder
            );

        foreach (BXBlog blog in blogList)
            list.Add(new BlogWrapper(blog, this));

        e.Data = list;
	}


    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
        e.Count = BXBlog.Count(new BXFilter(ItemFilter.CurrentFilter, BXBlog.Fields));
	}

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        BlogWrapper wrapper = (BlogWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);
    }


    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
            return;
        BXBlogCollection blogs;
        try
        {
            BXFilter filter = (e.Keys == null)
                ? new BXFilter(ItemFilter.CurrentFilter, BXBlog.Fields)
                : new BXFilter(new BXFilterItem(BXBlog.Fields.Id, BXSqlFilterOperators.Equal, e.Keys["ID"]));
            blogs = BXBlog.GetList(filter, null);
        }
        catch (Exception ex)
        {
            ErrorMessage.AddErrorMessage(ex.Message);
            return;
        }
        bool errorTextAdded = false;
        foreach (BXBlog blog in blogs)
        {
            try
            {
                blog.Delete();
                e.DeletedCount++;
            }
            catch (Exception ex2)
            {
                if (!errorTextAdded)
                {
                    ErrorMessage.AddErrorMessage(GetMessage("Blog.DeleteFailure"));
                    errorTextAdded = true;
                }
            }
        }
        
	}
}
/// <summary>
/// Обёртка для блога
/// </summary>
public class BlogWrapper
{
    BXBlog _charge;
    BXAdminPage _parentPage;
    public BlogWrapper(BXBlog charge, BXAdminPage parentPage)
    {
        if (charge == null)
            throw new ArgumentNullException("charge");

        if (parentPage == null)
            throw new ArgumentNullException("parentPage");

        _charge = charge;
        _parentPage = parentPage;
    }

	public BXBlog Charge
	{
		get
		{
			return _charge;
		}
	}

    public string ID
    {
        get { return _charge.Id.ToString(); }
    }

    public string Name
    {
        get { return _charge.Name; }
    }

    public string DateCreated
    {
        get { return _charge.DateCreated.ToString("g"); }
    }

    public string DateLastPosted
    {
        get { return _charge.DateLastPosted.ToString("g"); }
    }

    public string Active
    {
        get { return _charge.Active ? _parentPage.GetMessageRaw("Kernel.Yes") : _parentPage.GetMessageRaw("Kernel.No"); }
    }

    public string Owner
    {
        get 
        { 
            BXBlogUser owner = _charge.Owner;
            if (owner == null) return String.Empty;
            string firstName = (String.IsNullOrEmpty(owner.User.FirstName)) ? String.Empty : owner.User.FirstName.Trim();
            string lastName = (String.IsNullOrEmpty(owner.User.LastName)) ? String.Empty : 
                            ((firstName== String.Empty ) ? owner.User.LastName.Trim()+ " " : " " + owner.User.LastName.Trim() + " ");
            
            return firstName + lastName + "(" + owner.User.UserName +")"; 

        }
    }

    public string Slug
    {
        get { return _charge.Slug; }
    }

    public string Categories
    {
        get 
        {
            BXBlogCategoryCollection categories = _charge.Categories;
            if (categories == null || categories.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            foreach (BXBlogCategory category in categories)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(category.Name);
            }
            return sb.ToString();
        }
    }

	public string IndexContent
	{
		get
		{
			return _parentPage.GetMessage("IndexContent." + _charge.IndexContent);
		}
	}
}
