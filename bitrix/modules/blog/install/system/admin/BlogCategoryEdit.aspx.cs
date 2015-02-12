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
using Bitrix.Blog;

public enum BlogCategoryEditorMode
{
    Creation = 1,
    Modification = 2
}

public enum BlogCategoryEditorError
{
    None                        = 0,
    BlogCategoryIsNotFound      = -1,
    BlogCategoryCreation        = -2,
    BlogCategoryModification    = -3,
    BlogCategoryDeleting        = -4
}

public partial class bitrix_admin_BlogCategoryEdit : BXAdminPage
{
	private int? _blogCategoryId = null;
	protected int BlogCategoryId
	{
        get 
        {
            if (_blogCategoryId.HasValue)
                return _blogCategoryId.Value;

            string blogCategoryIdStr = Request.QueryString["id"];
            if (string.IsNullOrEmpty(blogCategoryIdStr))
                _blogCategoryId = 0;
            else
            {
                try
                {
                    _blogCategoryId = Convert.ToInt32(Request.QueryString["id"]);
                }
                catch (Exception /*exc*/)
                {
                    _blogCategoryId = 0;
                }
            }
            return _blogCategoryId.Value; 
        }
	}

    private BlogCategoryEditorMode _editorMode = BlogCategoryEditorMode.Creation;
    protected BlogCategoryEditorMode EditorMode
    {
        get { return _editorMode; }
    }

    private BXBlogCategory _blogCategory;
    protected BXBlogCategory BlogCategory
    {
        get { return _blogCategory; }
    }

    private string _errorMessage = string.Empty;
    protected string ErrorMessage
    {
        get { return _errorMessage; }
    }

    private BlogCategoryEditorError _editorError = BlogCategoryEditorError.None;
    protected BlogCategoryEditorError EditorError
    {
        get { return _editorError; }
    }

	protected override string BackUrl
	{
		get { return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "BlogCategoryList.aspx"; }
	}

    protected override void OnInit(EventArgs e)
    {

        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
            BXAuthentication.AuthenticationRequired();

        BXSiteCollection siteColl = BXSite.GetList(null, 
            new BXOrderBy(new BXOrderByPair(BXSite.Fields.Sort, BXOrderByDirection.Asc)), 
            new BXSelect(BXSite.Fields.ID, BXSite.Fields.Name), 
            null,
			BXTextEncoder.EmptyTextEncoder
            );

        ListItemCollection siteItems = BlogCategorySites.Items;
        siteItems.Clear();

        foreach (BXSite site in siteColl)
            siteItems.Add(new ListItem(string.Format("[{0}] {1}", site.Id, site.Name), site.Id));

        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        TryLoadBlogCategory();
        BXBlogCategory blogCategory = BlogCategory;
        string title = EditorMode == BlogCategoryEditorMode.Modification ? string.Format(GetMessage("PageTitle.EditCategory"), blogCategory != null ? HttpUtility.HtmlEncode(blogCategory.Name) : "?") : GetMessage("PageTitle.CreateCategory");

		if (EditorMode == BlogCategoryEditorMode.Creation)
		{
			AddButton.Visible = false;
			DeleteButton.Visible = false;
		}

        MasterTitle = title;
        Page.Title = title;

        base.OnLoad(e);
    }

    private void TryLoadBlogCategory()
	{
        int id = BlogCategoryId;
        if (id <= 0)
        {
            _editorMode = BlogCategoryEditorMode.Creation;
            _blogCategory = new BXBlogCategory();
        }
        else
        {
            _editorMode = BlogCategoryEditorMode.Modification;
            if ((_blogCategory = BXBlogCategory.GetById(id, BXTextEncoder.EmptyTextEncoder)) == null)
            {
                _errorMessage = string.Format(GetMessageRaw("Error.UnableToFindCategory"), id);
                _editorError = BlogCategoryEditorError.BlogCategoryIsNotFound;
                return;
            }
        }

        if (!IsPostBack)
        {
            BlogCategoryName.Text = _blogCategory.Name;
            BlogCategorySort.Text = _blogCategory.Sort.ToString();
            BlogCategoryXmlId.Text = _blogCategory.XmlId;
            ListItemCollection siteItems = BlogCategorySites.Items;
            string[] siteIds = _blogCategory.GetSiteIds();
            foreach (string siteId in siteIds)
            {
                ListItem li = siteItems.FindByValue(siteId);
                if (li != null)
                    li.Selected = true;
            }
        }
        else
        {
            _blogCategory.Name = BlogCategoryName.Text;
            if (string.IsNullOrEmpty(BlogCategorySort.Text))
                _blogCategory.Sort = 0;
            else
            {
                try
                {
                    _blogCategory.Sort = Convert.ToInt32(BlogCategorySort.Text);
                }
                catch (Exception /*exc*/) { }
            }
            _blogCategory.XmlId = BlogCategoryXmlId.Text;

            List<string> siteIdList = new List<string>();
            ListItemCollection siteItems  = BlogCategorySites.Items;
            foreach (ListItem siteItem in siteItems)
            {
                if (!siteItem.Selected)
                    continue;
                siteIdList.Add(siteItem.Value);
            }
            _blogCategory.SetSiteIds(siteIdList.ToArray());
        }
	}

    private void TrySaveBlogCategory()
    {
        if (_blogCategory == null)
            return;

        try
        {
            _blogCategory.Save();
			_blogCategoryId = _blogCategory.Id;
        }
        catch (Exception exc)
        {
            _errorMessage = exc.Message;
            _editorError = EditorMode == BlogCategoryEditorMode.Creation ? BlogCategoryEditorError.BlogCategoryCreation : BlogCategoryEditorError.BlogCategoryModification; 
        }
    }

    protected override void OnPreRender(EventArgs e)
    {
        if (EditorError != BlogCategoryEditorError.None)
        {
            errorMessage.AddErrorMessage(ErrorMessage);
            if (EditorMode == BlogCategoryEditorMode.Modification)
            {
                if (EditorError == BlogCategoryEditorError.BlogCategoryIsNotFound)
                    TabControl.Visible = false;
            }
        }

        base.OnPreRender(e);
    }


    protected void OnBlogCategoryEdit(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
            case "save":
                {
                    if (IsValid)
                    {
                        TrySaveBlogCategory();
                        if (EditorError == BlogCategoryEditorError.None)
                            GoBack(); 
                    }
                }
                break;
            case "apply":
                {
                    if (IsValid)
                    {
                        TrySaveBlogCategory();
                        if (EditorError == BlogCategoryEditorError.None)
                            Response.Redirect(string.Format("BlogCategoryEdit.aspx?id={0}&tabindex={1}", BlogCategoryId.ToString(), TabControl.SelectedIndex));
                    }
                }
                break;
			case "cancel":
				GoBack();
				break;
		}
	}

	protected void OnToolBarButtonClick(object sender, CommandEventArgs e)
	{
		if (e.CommandName == "delete")
		{
			try
			{
                BXBlogCategory blogCategory = BlogCategory;
                if (blogCategory != null)
                    blogCategory.Delete();

				GoBack();
			}
			catch (Exception ex)
			{
                _errorMessage = ex.Message;
                _editorError = BlogCategoryEditorError.BlogCategoryDeleting;
			}
		}
	}
}
