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
using System.Text;

public enum BlogEditorMode
{
    Creation = 1,
    Modification = 2
}

public enum BlogEditorError
{
    None                = 0,
    BlogIsNotFound      = -1,
    BlogOwnerIsNotFound = -2,
    BlogCreation        = -3,
    BlogModification    = -4,
    BlogDeleting        = -5
}

public partial class bitrix_admin_BlogEdit : BXAdminPage
{
	private int? _blogId = null;
	protected int BlogId
	{
        get 
        {
            if (_blogId.HasValue)
                return _blogId.Value;

            string blogIdStr = Request.QueryString["id"];
            if (string.IsNullOrEmpty(blogIdStr))
                 _blogId = 0;
            else
            {
                try
                {
                    _blogId = Convert.ToInt32(Request.QueryString["id"]);
                }
                catch (Exception /*exc*/)
                {
                    _blogId = 0;
                }
            }
            return _blogId.Value; 
        }
	}

    private BlogEditorMode _editorMode = BlogEditorMode.Creation;
    protected BlogEditorMode EditorMode
    {
        get { return _editorMode; }
    }

    private BXBlog _blog;
    protected BXBlog Blog
    {
        get { return _blog; }
    }

    private BXBlogSyndication _syndication = null;
    protected BXBlogSyndication Syndication
    {
        get
        {
            if (_syndication != null)
                return _syndication;

            if ((_syndication = BXBlogSyndication.Find(BlogId, null)) == null)
                _syndication = new BXBlogSyndication(BlogId);

            return _syndication;
        }
    }

    private string _errorMessage = string.Empty;
    protected string ErrorMessage
    {
        get { return _errorMessage; }
    }

    private BlogEditorError _editorError = BlogEditorError.None;
    protected BlogEditorError EditorError
    {
        get { return _editorError; }
    }

	protected override string BackUrl
	{
		get { return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "BlogList.aspx"; }
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement))
			BXAuthentication.AuthenticationRequired();

		#region BLOG CATEGORY
		BXBlogCategoryCollection categoryList = BXBlogCategory.GetList(
			null,
			null,
			new BXSelect(
				BXSelectFieldPreparationMode.Normal,
				BXBlogCategory.Fields.Id,
				BXBlogCategory.Fields.Name
				),
			null,
			BXTextEncoder.EmptyTextEncoder
			);

		ListItemCollection categoryItems = BlogCategories.Items;
		categoryItems.Clear();

		foreach (BXBlogCategory category in categoryList)
			categoryItems.Add(new ListItem(category.Name, category.Id.ToString()));
		#endregion

		#region COMMENT PREMODERATION SCOPE
		ListItemCollection commentPremodScopeItems = CommentModerationMode.Items;
		commentPremodScopeItems.Clear();

		commentPremodScopeItems.Add(new ListItem(GetMessageRaw("ListItem.CommentModerationMode.All"), "ALL"));
		commentPremodScopeItems.Add(new ListItem(GetMessageRaw("ListItem.CommentModerationMode.Filter"), "FILTER"));

		#endregion

		TryLoadBlog();

		CustomFieldList1.EntityId = BXBlog.GetCustomFieldsKey();

        if(_blog != null)
            CustomFieldList1.Load(_blog.CustomValues);

		Permissions.BlogId = BlogId;
    }

	protected void Page_Load(object sender, EventArgs e)
	{
        SyndicationRssFeedUrlRequired.Enabled = SyndicationRssFeedUrlRegex.Enabled = EnableSyndication.Checked;
        MasterTitle = Page.Title = EditorMode == BlogEditorMode.Modification ? string.Format(GetMessage("PageTitle.EditBlog"), Blog != null ? HttpUtility.HtmlEncode(Blog.Name) : "?") : GetMessage("PageTitle.CreateBlog");
	}

	private void TryLoadBlog()
	{
        int id = BlogId;
        if (id <= 0)
        {
            _editorMode = BlogEditorMode.Creation;
            _blog = new BXBlog();
        }
        else
        {
            _editorMode = BlogEditorMode.Modification;
            #region OLD
            //if ((_blog = BXBlog.GetById(id, BXTextEncoder.EmptyTextEncoder)) == null)
            //{
            //    _errorMessage = string.Format(GetMessageRaw("Error.UnableToFindBlog"), id);
            //    _editorError = BlogEditorError.BlogIsNotFound;
            //    return;
            //}
            #endregion
            BXBlogCollection blogCol = BXBlog.GetList(
                new BXFilter(new BXFilterItem(BXBlog.Fields.Id, BXSqlFilterOperators.Equal, id)),
                null,
                new BXSelectAdd(
					BXBlog.Fields.CustomFields.DefaultFields,
					BXBlog.Fields.Settings),
                null);

            if ((_blog = blogCol.Count > 0 ? blogCol[0] : null) == null)
            {
                _errorMessage = string.Format(GetMessageRaw("Error.UnableToFindBlog"), id);
                _editorError = BlogEditorError.BlogIsNotFound;
                return;
            }
        }

		if (_editorMode == BlogEditorMode.Modification)
        {
            BlogActive.Checked = _blog.Active;
            BlogName.Text = _blog.TextEncoder.Decode(_blog.Name);
            BlogSlug.Text = _blog.Slug;
            BlogDescription.Text = _blog.TextEncoder.Decode(_blog.Description);
            BlogSort.Text = _blog.Sort.ToString();
            BlogNotifyOfComments.Checked = _blog.NotifyOfComments;
			BlogIsTeam.Checked = _blog.IsTeam;
            BlogXmlId.Text = _blog.TextEncoder.Decode(_blog.XmlId);
			BlogIndexContent.SelectedValue = _blog.IndexContent.ToString();
            ListItemCollection categoryItems = BlogCategories.Items;
            int[] categoryIds = _blog.GetCategoryIds();
            foreach (int categoryId in categoryIds)
            {
                ListItem li = categoryItems.FindByValue(categoryId.ToString());
                if (li != null)
                    li.Selected = true;
            }

            #region OLD
            /*
            if (!_blog.IsNew)
            {
                ListItemCollection userItems = BlogOwner.Items;
                ListItem userItem = userItems.FindByValue(_blog.OwnerId.ToString());
                if (userItem != null)
                    userItem.Selected = true;
            }
            */
            #endregion

            if (!_blog.IsNew)
            {
                BXBlogUser owner = _blog.Owner;
                FindUserAutocomplete.HiddenValue = _blog.OwnerId.ToString();
				string firstName = owner.User.TextEncoder.Decode(owner.User.FirstName);
				firstName = (String.IsNullOrEmpty(firstName)) ? String.Empty : firstName.Trim();

				string lastName = owner.User.TextEncoder.Decode(owner.User.LastName);
                lastName = 
					String.IsNullOrEmpty(lastName)
					? String.Empty 
					: (
						string.IsNullOrEmpty(firstName) 
						? lastName.Trim() + " " 
						: " " + lastName + " "
					);
                FindUserAutocomplete.TextBoxValue = firstName + lastName + "(" + owner.TextEncoder.Decode(owner.GetUserName()) + ")";
            }

            EnableSyndication.Checked = Syndication.Enabled;
            SyndicationRssFeedUrl.Text = Syndication.FeedUrl;
            SyndicationUpdateable.Checked = Syndication.Updateable;
            SyndicationRedirectToComments.Checked = Syndication.RedirectToComments;

            SyndicationRssFeedUrlRequired.Enabled = Syndication.Enabled;
            SyndicationRssFeedUrlRegex.Enabled = Syndication.Enabled;

            #region DEFERRED
            /*
            ListItem postVisibilityModeItem = BlogPostVisibilityMode.Items.FindByValue(_blog.PostVisibilityMode.ToString("G"));
            if (postVisibilityModeItem != null)
                postVisibilityModeItem.Selected = true;

            ListItem addCommentPermissionItem = BlogAddCommentPermission.Items.FindByValue(_blog.AddCommentPermission.ToString("G"));
            if (addCommentPermissionItem != null)
                addCommentPermissionItem.Selected = true;

            ListItem commentApprovalItem = BlogCommentApproval.Items.FindByValue(_blog.CommentApproval.ToString("G"));
            if (commentApprovalItem != null)
                commentApprovalItem.Selected = true;
            */
            #endregion

			BXBlogSettings settings = _blog.Settings ?? new BXBlogSettings(_blog.Id);

			EnableCommentModeration.Checked = settings.EnableCommentModeration;
			ListItem moderationMode = CommentModerationMode.Items.FindByValue(settings.CommentModerationMode.ToString("G").ToUpperInvariant());
			if(moderationMode != null)
				moderationMode.Selected = true;
			CommentPremoderationFilterLinkThresholdTbx.Text = settings.CommentModerationFilter.LinkThreshold.ToString();
			StringBuilder sb = new StringBuilder();
			foreach(string stopListItem in settings.CommentModerationFilter.StopList)
				sb.AppendLine(stopListItem);
			CommentPremoderationFilterStopListTbx.Text = sb.ToString();
        }
        else
        {
			BlogActive.Checked = true;
			BlogSort.Text = "10";
			BlogNotifyOfComments.Checked = true;
			BlogIndexContent.SelectedValue = BXBlogIndexContentMode.All.ToString();
			EnableCommentModeration.Checked = false;
			ListItem moderationMode = CommentModerationMode.Items.FindByValue(BXBlogCommentModerationMode.All.ToString("G").ToUpperInvariant());
			if(moderationMode != null)
				moderationMode.Selected = true;
			CommentPremoderationFilterLinkThresholdTbx.Text = "0";
			CommentPremoderationFilterStopListTbx.Text = string.Empty;
        }
	}

    private void TrySaveBlog()
    {
        if (_blog == null)
            return;
        try
        {
            int userId;
            if (!int.TryParse(FindUserAutocomplete.HiddenValue, out userId))
            {
                Bitrix.Security.BXUser owner=null;
                if (!Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(FindUserAutocomplete.TextBoxValue))
                {
                    BXUserCollection owners = 
                        Bitrix.Security.BXUser.GetList(new BXFilter(
                                                            new BXFilterItem(
                                                                    Bitrix.Security.BXUser.Fields.UserName,
                                                                    BXSqlFilterOperators.Equal,
                                                                    FindUserAutocomplete.TextBoxValue
                                                                    )), 
                                                       null);
                    if (owners.Count > 0) owner = owners[0];
                }

                _blog.OwnerId = owner==null ? 0 : owner.UserId;
            }
            else
            {
                BXUser user = Bitrix.Security.BXUser.GetById(userId);
                if (user != null)
                    _blog.OwnerId = userId;
                else
                {
                    _errorMessage = string.Format(GetMessageRaw("Error.UnableToFindUser"), userId);
                    _editorError = BlogEditorError.BlogOwnerIsNotFound;
                }
            }

			_blog.Active = BlogActive.Checked;
			_blog.Name = BlogName.Text;
			_blog.Slug = BlogSlug.Text;
			_blog.Description = BlogDescription.Text;
			if (string.IsNullOrEmpty(BlogSort.Text))
				_blog.Sort = 0;
			else
			{
				try
				{
					_blog.Sort = Convert.ToInt32(BlogSort.Text);
				}
				catch
                {
                }
			}

			_blog.NotifyOfComments = BlogNotifyOfComments.Checked;
			_blog.IsTeam = BlogIsTeam.Checked;
			_blog.XmlId = BlogXmlId.Text;

			List<int> categoryIdList = null;
			ListItemCollection categoryItems = BlogCategories.Items;
			foreach (ListItem categoryItem in categoryItems)
			{
				if (!categoryItem.Selected)
					continue;
				try
				{
                    (categoryIdList ?? (categoryIdList = new List<int>())).Add(Convert.ToInt32(categoryItem.Value));
				}
				catch
				{
				}
			}
            _blog.SetCategoryIds(categoryIdList != null ? categoryIdList.ToArray() : new int[0]);
		
			if (BXModuleManager.IsModuleInstalled("Search"))
			{
				_blog.IndexContent = BXBlogIndexContentMode.All;
				try
				{
					if (Enum.IsDefined(typeof(BXBlogIndexContentMode), BlogIndexContent.SelectedValue))
						_blog.IndexContent = (BXBlogIndexContentMode)Enum.Parse(typeof(BXBlogIndexContentMode), BlogIndexContent.SelectedValue);
				}
				catch
				{
				}
			}

			if (CustomFieldList1.HasItems)
				_blog.CustomValues.Override(CustomFieldList1.Save());

			BXBlogSettings settings = _blog.Settings;
			if(settings == null)
				_blog.Settings = settings = new BXBlogSettings(_blog.Id);

			settings.EnableCommentModeration = EnableCommentModeration.Checked;

			try
			{
				settings.CommentModerationMode = (BXBlogCommentModerationMode)Enum.Parse(typeof(BXBlogCommentModerationMode), CommentModerationMode.SelectedValue, true);
			}
			catch(ArgumentException)
			{
			}

			int linkThreshold;
			if(!int.TryParse(CommentPremoderationFilterLinkThresholdTbx.Text, out linkThreshold))
				linkThreshold = 0;

			settings.CommentModerationFilter.LinkThreshold = linkThreshold;

			if(string.IsNullOrEmpty(CommentPremoderationFilterStopListTbx.Text))
				settings.CommentModerationFilter.SetStopList(null);
			else
				settings.CommentModerationFilter.SetStopList(CommentPremoderationFilterStopListTbx.Text.Replace("\r", string.Empty).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

            _blog.Save();
			_blogId = _blog.Id;

            /*
             * Если синдикация уже есть или пользователь инициировал её создание в этом запросе
             */
            if (!Syndication.IsNew || EnableSyndication.Checked)
            {
                Syndication.Enabled = EnableSyndication.Checked;
                Syndication.FeedUrl = SyndicationRssFeedUrl.Text.TrimStart();
                Syndication.Updateable = SyndicationUpdateable.Checked;
                Syndication.RedirectToComments = SyndicationRedirectToComments.Checked;
                Syndication.Save();
            }


			Permissions.BlogId = _blog.Id;
			Permissions.Save(null);
        }
        catch (Exception exc)
        {
            _errorMessage = exc.Message;
            _editorError = EditorMode == BlogEditorMode.Creation ? BlogEditorError.BlogCreation : BlogEditorError.BlogModification; 
        }
    }

    protected override void OnPreRender(EventArgs e)
    {
        if (EditorError != BlogEditorError.None)
        {
            errorMessage.AddErrorMessage(ErrorMessage);
            if (EditorMode == BlogEditorMode.Modification && EditorError == BlogEditorError.BlogIsNotFound)
                TabControl.Visible = false;
        }

        if (EditorMode != BlogEditorMode.Modification)
        {
            AddButton.Visible = false;
            DeleteButton.Visible = false;
        }

        FindUserAutocomplete.AttachScript();
       
        base.OnPreRender(e);
    }


	protected void OnBlogEdit(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
            case "save":
                {
                    if (IsValid && EditorError == BlogEditorError.None)
                    {
						if (!Permissions.Validate(null))
						{
							foreach (var err in Permissions.Errors)
								errorMessage.AddErrorText(err);
							break;
						}

                        TrySaveBlog();
                        if (EditorError == BlogEditorError.None)
                            GoBack(); 
                    }
                }
                break;
            case "apply":
                {
                    if (IsValid && EditorError == BlogEditorError.None)
                    {
                        if (!Permissions.Validate(null))
						{
							foreach (var err in Permissions.Errors)
								errorMessage.AddErrorText(err);
							break;
						}
						
						TrySaveBlog();
                        if (EditorError == BlogEditorError.None)
                            Response.Redirect(string.Format("BlogEdit.aspx?id={0}&tabindex={1}", BlogId.ToString(), TabControl.SelectedIndex));
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
                BXBlog blog = Blog;
                if (blog != null)
                    blog.Delete();

				GoBack();
			}
			catch (Exception ex)
			{
                _errorMessage = ex.Message;
                _editorError = BlogEditorError.BlogDeleting;
			}
		}
	}
}
