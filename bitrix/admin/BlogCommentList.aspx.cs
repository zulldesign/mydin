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
using Bitrix.Modules;
using System.IO;

public partial class bitrix_admin_BlogCommentList : BXAdminPage
{
    protected override void OnPreInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
            BXAuthentication.AuthenticationRequired();

        base.OnPreInit(e);
    }

	protected override void OnInit(EventArgs e)
	{
		BXBlogCollection blogs = BXBlog.GetList(
			new BXFilter(new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true)),
			new BXOrderBy(
				new BXOrderByPair(BXBlog.Fields.Name, BXOrderByDirection.Asc),
				new BXOrderByPair(BXBlog.Fields.Id, BXOrderByDirection.Asc)),
			new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlog.Fields.Id, BXBlog.Fields.Name), 
			null,
			BXTextEncoder.EmptyTextEncoder);

		foreach(BXBlog blog in blogs)
			BlogIdFilterItem.Values.Add(new ListItem(blog.Name, blog.Id.ToString()));

		BXBlogCategoryCollection categories = BXBlogCategory.GetList(
			null,
			new BXOrderBy(
				new BXOrderByPair(BXBlogCategory.Fields.Sort, BXOrderByDirection.Asc),
				new BXOrderByPair(BXBlogCategory.Fields.Name, BXOrderByDirection.Asc),
				new BXOrderByPair(BXBlogCategory.Fields.Id, BXOrderByDirection.Asc)),
			new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlogCategory.Fields.Id, BXBlogCategory.Fields.Name), 
			null,
			BXTextEncoder.EmptyTextEncoder);

		foreach(BXBlogCategory category in categories)
			CategoryIdFilterItem.Values.Add(new ListItem(category.Name, category.Id.ToString()));

		BXSiteCollection sites = BXSite.GetList(
			null,
			new BXOrderBy(
				new BXOrderByPair(BXSite.Fields.Sort, BXOrderByDirection.Asc),
				new BXOrderByPair(BXSite.Fields.Name, BXOrderByDirection.Asc),
				new BXOrderByPair(BXSite.Fields.ID, BXOrderByDirection.Asc)),
			new BXSelect(BXSelectFieldPreparationMode.Normal, BXSite.Fields.ID, BXSite.Fields.Name), 
			null,
			BXTextEncoder.EmptyTextEncoder);

		foreach(BXSite site in sites)
			SiteFilterItem.Values.Add(new ListItem(site.Name, site.Id));

		base.OnInit(e);
	}

    protected override void OnLoad(EventArgs e)
    {
        MasterTitle = Page.Title = GetMessage("PageTitle");
        base.OnLoad(e);
    }

    protected override void OnPreRender(EventArgs e)
    {
        BXPage.Scripts.RequireUtils();
        AuthorFilterItem.AttachScript();
        base.OnPreRender(e);
    }

	private BXBlogCommentCollection comments = null;
	private BXBlogCommentChain processor = null;

    protected void ItemGrid_Select(object sender, BXSelectEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
            return;

        List<BlogCommentWrapper> list = new List<BlogCommentWrapper>();

        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXBlogComment.Fields);

        BXSelect s = new BXSelect(BXSelectFieldPreparationMode.Add,
			BXBlogComment.Fields.Author.User.FirstName,
			BXBlogComment.Fields.Author.User.LastName,
			BXBlogComment.Fields.Author.User.UserName,
			BXBlogComment.Fields.Blog.Id,
			BXBlogComment.Fields.Blog.Name,
			BXBlogComment.Fields.Post.Id,
			BXBlogComment.Fields.Post.Title);

        this.comments = BXBlogComment.GetList(
            f,
            new BXOrderBy(BXBlogComment.Fields, string.IsNullOrEmpty(e.SortExpression) ? "Id" : e.SortExpression),
            s,
            new BXQueryParams(e.PagingOptions),
            BXTextEncoder.EmptyTextEncoder
            );

		this.processor = new BXBlogCommentChain();

        foreach (BXBlogComment comment in this.comments)
            list.Add(new BlogCommentWrapper(comment, this, this.processor));

        e.Data = list;
	}

    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
        e.Count = BXBlogComment.Count(new BXFilter(ItemFilter.CurrentFilter, BXBlogComment.Fields));
	}

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        BlogCommentWrapper wrapper = (BlogCommentWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);
    }


    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
            return;
		try
		{
	        BXBlogComment.Delete(e.Keys["ID"]);
			e.DeletedCount++;
		}
		catch (Exception ex)
		{
			ErrorMessage.AddErrorMessage(ex.Message);
		}
	}

	private int ApproveComments(int[] indexes, bool approve)
	{
		if(this.comments == null || this.comments.Count == 0 || indexes == null || indexes.Length == 0)
			return 0;

		int count = 0;
		foreach(int i in indexes)
		{
			if(i < 0 || i >= this.comments.Count)
				continue;

			BXBlogComment comment = this.comments[i];
			if(comment.IsApproved == approve)
				continue;

			comment.IsApproved = approve;
			comment.Save();
			count++;
		}

		return count;
	}

	private int ApproveAllComments(bool approve)
	{
		if(this.comments == null || this.comments.Count == 0)
			return 0;

		int count = 0;
		foreach(BXBlogComment comment in this.comments)
		{
			if(comment.IsApproved == approve)
				continue;

			comment.IsApproved = approve;
			comment.Save();
			count++;
		}

		return count;
	}

	protected void PopupPanelView_CommandClick(object sender, CommandEventArgs e)
	{
		if(this.comments == null || this.comments.Count == 0)
			return;

		string[] argAry = ((CommandItemEventArgs)e).GetEventArgsArray();
		if(argAry == null || argAry.Length == 0)
			return;

		int i;
		if(!int.TryParse(argAry[0], out i))
			return;

		try
		{
			switch (e.CommandName)
			{
				case "approve":
					if(ApproveComments(new int[]{ i }, true) > 0)
						Response.Redirect(Request.RawUrl);
					break;
				case "disapprove":
					if(ApproveComments(new int[]{ i }, false) > 0)
						Response.Redirect(Request.RawUrl);			
					break;
			}
		}
		catch (Exception ex)
		{
			ErrorMessage.AddErrorMessage(ex.Message);
		}
	}

	protected void ItemGrid_MultiOperationActionRun(object sender, UserMadeChoiceEventArgs e)
	{
		try
		{
			switch (e.CommandOfChoice.CommandName)
			{
				case "approve":
					if((e.ApplyForAll ? ApproveAllComments(true) : ApproveComments(ItemGrid.GetSelectedRowsIndices(), true)) > 0)
						Response.Redirect(Request.RawUrl);
					break;
				case "disapprove":
					if((e.ApplyForAll ? ApproveAllComments(false) : ApproveComments(ItemGrid.GetSelectedRowsIndices(), false)) > 0)
						Response.Redirect(Request.RawUrl);
					break;
			}
		}
		catch (Exception ex)
		{
			ErrorMessage.AddErrorMessage(ex.Message);
		}
	}

}
/// <summary>
/// Обёртка для комментария
/// </summary>
public class BlogCommentWrapper
{
    BXBlogComment entity;
	BXBlogCommentChain processor;
    BXAdminPage parentPage;
    public BlogCommentWrapper(BXBlogComment entity, BXAdminPage parentPage, BXBlogCommentChain processor)
    {
        if (entity == null)
            throw new ArgumentNullException("entity");

        if (parentPage == null)
            throw new ArgumentNullException("parentPage");

		this.processor = processor ?? new BXBlogCommentChain();

        this.entity = entity;
        this.parentPage = parentPage;
    }

    public string ID
    {
        get { return this.entity.Id.ToString(); }
    }

	public string IsApproved
	{
		get { return this.parentPage.GetMessageRaw(this.entity.IsApproved ? "Kernel.Yes" : "Kernel.No"); }
	}

	public string DateCreated
	{
		get { return this.entity.DateCreated.ToString("g"); }
	}

	public int AuthorId
	{
		get {  return this.entity.AuthorId; }
	}

	private string authorHtml = null;
	public string AuthorHtml
	{
		get 
		{
			if(this.authorHtml != null)
				return this.authorHtml;

			if(this.entity.Author != null && this.entity.Author.User != null)			
				this.authorHtml = @"<a target=""_blank"" href=""" + HttpUtility.HtmlAttributeEncode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/AuthUsersEdit.aspx")) +"?id=" + this.entity.Author.User.UserId.ToString() + @""">" + Nbsp(HttpUtility.HtmlEncode(this.entity.Author.User.TextEncoder.Decode(this.entity.Author.User.UserName))) + @"</a>";
			else
			{
				this.authorHtml = !string.IsNullOrEmpty(this.entity.AuthorEmail)  
					? string.Format(@"{0} (<a href=""mailto:{1}"">{2}</a>)",
						HttpUtility.HtmlEncode(this.entity.TextEncoder.Decode(this.entity.AuthorName)),
						HttpUtility.HtmlAttributeEncode(this.entity.TextEncoder.Decode(this.entity.AuthorEmail)),
						HttpUtility.HtmlEncode(this.entity.TextEncoder.Decode(this.entity.AuthorEmail))
						)
					: HttpUtility.HtmlEncode(this.entity.TextEncoder.Decode(this.entity.AuthorName));
			}
			return this.authorHtml;
		}
	}


	private string text = null;
	public string Text
	{
		get { return this.text ?? (this.text = Nbsp(this.processor.Process(this.entity.TextEncoder.Decode(this.entity.Content)))); }
	}

	private string postTitleHtml = null;
	public string PostTitleHtml
	{
		get 
		{
			return this.postTitleHtml ?? (this.postTitleHtml = Nbsp(this.entity.Post != null ? GetPostPublicUrl(this.entity.Post, this.parentPage) : string.Empty)); 
		}
	}

	private string blogHtml = null;
	public string BlogHtml
	{
		get 
		{ 
			return this.blogHtml ?? (this.blogHtml = @"<a target=""_blank"" href=""" + HttpUtility.HtmlAttributeEncode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/BlogEdit.aspx") + "?id=" + this.entity.BlogId.ToString()) + @""">" + Nbsp(this.entity.Blog != null ? BXTextEncoder.HtmlTextEncoder.Encode(this.entity.Blog.TextEncoder.Decode(this.entity.Blog.Name)) : this.entity.BlogId.ToString()) + @"</a>"); 
		}
	}

	private string authorIpHtml = null;
	public string AuthorIpHtm
	{
		get 
		{
			if(this.authorIpHtml != null)
				return this.authorIpHtml;

			return this.authorIpHtml = @"<a title=""" + HttpUtility.HtmlAttributeEncode(this.parentPage.GetMessageRaw("FindIpInWhois")) + @""" target=""_blank"" href=""http://whois.domaintools.com/" + HttpUtility.HtmlAttributeEncode(this.entity.TextEncoder.Decode(this.entity.AuthorIP)) +  @""">" + Nbsp(HttpUtility.HtmlEncode(this.entity.TextEncoder.Decode(this.entity.AuthorIP))) +  @"</a>";
		}
	}

	private static Dictionary<int, string> postUrls = new Dictionary<int,string>();
	private static string GetPostPublicUrl(BXBlogPost post, Page page)
	{
		if (!BXModuleManager.IsModuleInstalled("search"))
			return string.Empty;

		string url;
		if(postUrls.TryGetValue(post.Id, out url))
			return url;

		BXControl label = (BXControl)page.LoadControl("~/bitrix/admin/controls/Blog/BlogPostLabel.ascx");
		label.Attributes["PostID"] = post.Id.ToString();
		label.Attributes["PostTitle"] = post.TextEncoder.Decode(post.Title);
        
		StringBuilder sb = new StringBuilder();
		using(StringWriter sw = new StringWriter(sb))
			using(HtmlTextWriter w = new HtmlTextWriter(sw))
				label.RenderControl(w);
		url = sb.ToString();
		postUrls[post.Id] = url;

		return url;
	}

	private string Nbsp(string value)
	{
		return string.IsNullOrEmpty(value) ? "&nbsp;" : value;
	}
}
