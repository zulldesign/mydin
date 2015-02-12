using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using System.Web.UI;

namespace Bitrix.Blog.Components
{
	public partial class BlogEditComponent : BXComponent
	{
		private BXBlogAuthorization auth;
		private BXBlog blog;
        private BXBlogSyndication syndication = null;
		private string blogSlug;
		private Data data;
		private List<string> errorSummary;
		private bool templateIncluded;
		private Mode componentMode;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private Exception fatalException;
		private BXBlogCategoryCollection siteCategories;
		private List<string> slugIgnoreList;
		private Bitrix.Blog.UI.EditPermissions editPermissions;
		private Bitrix.Blog.UI.EditBlogUsers editBlogUsers;
		private bool canCreatePersonalBlog;
		private bool canCreateTeamBlog;

		public BXBlogAuthorization Auth
		{
            get { return InternalAuthorization; }
		}
		public BXBlog Blog
		{
			get { return blog; }
			set { blog = value; }
		}

        public BXBlogSyndication Syndication
        {
            get 
            {
                if (!CanSyndicateContent || Blog == null)
                    return null;

                if (syndication != null)
                    return syndication;

                if ((syndication = BXBlogSyndication.Find(Blog.Id, null)) == null)
                    syndication = new BXBlogSyndication(Blog.Id);

                return syndication;
            }
        }

		public int CategoryId
		{
			get
			{
				return Parameters.GetInt("CategoryId");
			}
		}

		public string BlogSlug
		{
			get
			{
				return (blogSlug ?? (blogSlug = Parameters.GetString("BlogSlug")));
			}
		}

		public int MaxBlogGroups
		{
			get
			{
				return Math.Max(1, Parameters.GetInt("MaxBlogGroups", 1));
			}
		}

        public int[] AvailableCategoryIds
        {
            get  
            { 
                List<int> r = Parameters.GetListInt("AvailableBlogGroups", null); 
                return r != null && r.Count > 0 ? r.ToArray() : new int[0];
            }
        }

        public bool AllowToAjustBlogGroups
        {
            get 
            {
                return Parameters.GetBool("AllowToAjustBlogGroups", true);
            }
        }

        public int[] ObligatoryCategoryIds
        {
            get
            {
                List<int> r = Parameters.GetListInt("ObligatoryBlogGroups", null);
                return r != null && r.Count > 0 ? r.ToArray() : new int[0];
            }
        }

		public int MinSlugLength
		{
			get
			{
				return Math.Max(1, Parameters.GetInt("MinSlugLength", 3));
			}
		}
		public int MaxSlugLength
		{
			get
			{
				return Math.Min(Math.Max(1, Parameters.GetInt("MaxSlugLength", 10)), 128);
			}
		}
		public List<string> SlugIgnoreList
		{
			get 
			{
				if (slugIgnoreList == null)
					slugIgnoreList = Parameters.GetListString("SlugIgnoreList", new List<string>());
				return slugIgnoreList;
			}
		}

		public Mode ComponentMode
		{
			get { return componentMode; }
		}
		public Data ComponentData
		{
			get { return this.data; }
		}

		public BXBlogCategoryCollection SiteCategories
		{
			get
			{
				if (siteCategories == null)
				{
					siteCategories = BXBlogCategory.GetList(
						new BXFilter(new BXFilterItem(BXBlogCategory.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)), 
						new BXOrderBy(new BXOrderByPair(BXBlogCategory.Fields.Sort, BXOrderByDirection.Asc)),
						null,
						null,
						BXTextEncoder.EmptyTextEncoder
					);
				}

				return siteCategories;
			}
		}
		public List<string> ErrorSummary
		{
			get
			{
				return errorSummary ?? (errorSummary = new List<string>());
			}
		}
		public ErrorCode FatalError
		{
			get
			{
				return fatalError;
			}
		}
		public Exception FatalException
		{
			get
			{
				return fatalException;
			}
		}
		public bool CanModerate
		{
			get { return Auth.CanApproveBlog; }
		}
		public bool IsOwner
		{
			get { return blog != null && blog.OwnerId == BXIdentity.Current.Id; }
		}
        public bool CanSyndicateContent
        {
            get { return InternalAuthorization.CanSyndicateContent; }
        }

		public bool CanCreatePersonalBlog
		{
			get { return canCreatePersonalBlog; }
		}
		public bool CanCreateTeamBlog
		{
			get { return canCreateTeamBlog; }
		}

        public bool CanApproveComments
        {
			//без блога нельзя определить право модерации комментариев
            get { return blog == null || InternalAuthorization.CanApproveComments; }
        }


        private BXCustomField[] availableBlogCustomFieldsForAuthor = null;
        public BXCustomField[] AvailableBlogCustomFieldsForAuthor
        {
            get
            {
                if (this.availableBlogCustomFieldsForAuthor != null)
                    return this.availableBlogCustomFieldsForAuthor;

                List<string> names = null;
                BXCustomFieldCollection allFields = null;
                if ((names = Parameters.GetListString("AvailableBlogCustomFieldsForAuthor")).Count == 0
                    || (allFields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.BlogCustomFieldEntityId)).Count == 0)
                    return (this.availableBlogCustomFieldsForAuthor = new BXCustomField[0]);

                List<BXCustomField> l = new List<BXCustomField>();
                foreach (string name in names)
                {
                    int index = allFields.FindIndex(
                        delegate(BXCustomField o) 
                        { 
                            return string.Equals(o.Name, name, StringComparison.InvariantCultureIgnoreCase); 
                        });
                    if (index < 0)
                        continue;
                    l.Add(allFields[index]);
                }
                return (this.availableBlogCustomFieldsForAuthor = l.ToArray());
            }
        }
        private BXCustomField[] availableBlogCustomFieldsForModerator = null;
        public BXCustomField[] AvailableBlogCustomFieldsForModerator
        {
            get
            {
                if (this.availableBlogCustomFieldsForModerator != null)
                    return this.availableBlogCustomFieldsForModerator;

                List<string> names = null;
                BXCustomFieldCollection allFields = null;
                if ((names = Parameters.GetListString("AvailableBlogCustomFieldsForModerator")).Count == 0
                    || (allFields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.BlogCustomFieldEntityId)).Count == 0)
                    return (this.availableBlogCustomFieldsForModerator = new BXCustomField[0]);

                List<BXCustomField> l = new List<BXCustomField>();
                foreach (string name in names)
                {
                    int fIndex = allFields.FindIndex(delegate(BXCustomField obj) { return string.Equals(obj.Name, name, StringComparison.InvariantCultureIgnoreCase); });
                    if (fIndex < 0)
                        continue;
                    l.Add(allFields[fIndex]);
                }
                return (this.availableBlogCustomFieldsForModerator = l.ToArray());
            }
        }

        private void PrepareBlogCustomFieldEditors()
        {
            if (this.blogCustomFieldEditors != null)
                return;

            BXCustomField[] authorFields = AvailableBlogCustomFieldsForAuthor,
                moderatorFields = InternalAuthorization.CanEditPost ? AvailableBlogCustomFieldsForModerator : new BXCustomField[0];
            if (authorFields.Length == 0 && moderatorFields.Length == 0)
            {
                this.blogCustomFieldEditors = new CustomFieldEditor[0];
                return;
            }

            List<CustomFieldEditor> l = new List<CustomFieldEditor>();
            if (authorFields.Length > 0)
                for (int i = 0; i < authorFields.Length; i++)
                {
                    try
                    {
                        CustomFieldEditor ed = new CustomFieldEditor(this, authorFields[i]);
                        ed.Load(this.data.CustomProperties);
                        l.Add(ed);
                    }
                    catch
                    {
                    }
                }

            if (moderatorFields.Length > 0)
                for (int i = 0; i < moderatorFields.Length; i++)
                {
                    BXCustomField f = moderatorFields[i];
                    if (l.FindIndex(
                            delegate(CustomFieldEditor obj)
                            { return string.Equals(obj.Field.Name, f.Name, StringComparison.InvariantCulture); }) >= 0)
                        continue;
                    try
                    {
                        CustomFieldEditor ed = new CustomFieldEditor(this, f);
                        ed.Load(this.data.CustomProperties);
                        l.Add(ed);
                    }
                    catch
                    {
                    }
                }
            this.blogCustomFieldEditors = l.ToArray();
        }

        private CustomFieldEditor[] blogCustomFieldEditors = null;
        public CustomFieldEditor[] BlogCustomFieldEditors
        {
            get
            {
                if (this.blogCustomFieldEditors == null)
                    PrepareBlogCustomFieldEditors();

                return this.blogCustomFieldEditors;
            }
        }

		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;

			try
			{
				componentMode = BXStringUtility.IsNullOrTrimEmpty(BlogSlug) ? Mode.Add : Mode.Edit;

                //Выбор категорий запрещён, но обязательные не заданы - создание невозможно
                if (ComponentMode == Mode.Add && !AllowToAjustBlogGroups && ObligatoryCategoryIds.Length == 0)
                {
                    Fatal(ErrorCode.FatalNoGroupsButAdjustingDisabled);
                    return;
                }

				if (!LoadEntities())
					return;

				if (!CheckPermissions())
					return;

				this.data = new Data(this);
				this.data.BlogSlug = BlogSlug;
				this.data.BlogActive = true;

				if (Blog != null)
				{
					this.data.BlogName = Blog.Name;
                    this.data.BlogDescription = Blog.Description;
                    this.data.NotifyComments = Blog.NotifyOfComments;
                    this.data.BlogActive = Blog.Active;
					this.data.IsTeam = Blog.IsTeam;
				}

                if (Syndication != null)
                {
                    this.data.EnableSyndication = Syndication.Enabled;
                    this.data.SyndicationFeedUrl = Syndication.FeedUrl;
                    this.data.SyndicationUpdateableContent = Syndication.Updateable;
                    this.data.SyndicationRedirectToComments = Syndication.RedirectToComments;
                }

                //обязательно, т.к. редакторы пользовательских св-в могут внести изменения в структуру страницы. например, подключить javascript
                PrepareBlogCustomFieldEditors();

				if (!templateIncluded)
				{
					templateIncluded = true;

					BXPublicPage bitrixPage = Page as BXPublicPage;
					if (bitrixPage != null && !IsComponentDesignMode && Parameters.Get<bool>("SetPageTitle", true))
						bitrixPage.BXTitle = componentMode == Mode.Add ? GetMessageRaw("PageTitle.CreateBlog") : GetMessageRaw("PageTitle.EditBlog");

					IncludeComponentTemplate();
				}
			}
			catch (Exception ex)
			{
				Fatal(ex);
			}

		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			foreach (string param in new string[] { Parameters.GetString("ThemeCssFilePath"), Parameters.GetString("ColorCssFilePath") })
			{
				if (BXStringUtility.IsNullOrTrimEmpty(param))
					continue;
				string path = param;
				try
				{
					path = BXPath.ToVirtualRelativePath(path);
					if (BXSecureIO.FileExists(path))
						BXPage.RegisterStyle(path);
				}
				catch
				{
				}
			}
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Group = BXBlogModuleConfiguration.GetComponentGroup();

			BXCategory mainCategory = BXCategory.Main,
                urlCategory = BXCategory.UrlSettings,
                additionalSettingsCategory = BXCategory.AdditionalSettings,
                customFieldCategory = BXCategory.CustomField;

			ParamsDefinition["ThemeCssFilePath"] = new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/style.css", mainCategory);
			ParamsDefinition["ColorCssFilePath"] = new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", mainCategory);
			ParamsDefinition["BlogSlug"] = new BXParamText(GetMessageRaw("Param.BlogSlug"), "", mainCategory);
			ParamsDefinition["CategoryId"] = new BXParamSingleSelection(GetMessageRaw("Param.CategoryId"), "", mainCategory);
            ParamsDefinition["AllowToAjustBlogGroups"] = 
                new BXParamYesNo(
                    GetMessageRaw("Param.AllowToAjustBlogGroups"), 
                    true, 
                    mainCategory,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "AllowToAjustBlogGroups", "NonObligatoryBlogGroupsBinding", "ObligatoryBlogGroupsBinding")
                    );
            ParamsDefinition["ObligatoryBlogGroups"] = 
                new BXParamMultiSelection(
                    GetMessageRaw("Param.ObligatoryBlogGroups"), 
                    string.Empty, 
                    mainCategory,
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "ObligatoryBlogGroups", new string[] { "ObligatoryBlogGroupsBinding" })
                    );
            ParamsDefinition["MaxBlogGroups"] = 
                new BXParamText(
                    GetMessageRaw("Param.MaxBlogGroups"), 
                    "1", 
                    mainCategory,
                    new ParamClientSideActionGroupViewMember(ClientID, "MaxBlogGroups", new string[] { "NonObligatoryBlogGroupsBinding" })
                    );
            ParamsDefinition["AvailableBlogGroups"] =
                new BXParamMultiSelection(
                    GetMessageRaw("Param.AvailableGroups"), 
                    string.Empty, 
                    mainCategory, 
                    null,
                    new ParamClientSideActionGroupViewMember(ClientID, "AvailableBlogGroups", new string[] { "NonObligatoryBlogGroupsBinding" })
                    );

			ParamsDefinition["SlugIgnoreList"] = new BXParamText(GetMessageRaw("Param.SlugIgnoreList"), "admin;blog;blogs;users;user;search;posts;rss;tags", mainCategory);
			ParamsDefinition["MinSlugLength"] = new BXParamText(GetMessageRaw("Param.MinSlugLength"), "3", mainCategory);
			ParamsDefinition["MaxSlugLength"] = new BXParamText(GetMessageRaw("Param.MaxSlugLength"), "10", mainCategory);
			ParamsDefinition["RedirectUrlTemplate"] = new BXParamText(GetMessageRaw("Param.RedirectUrlTemplate"), "blog.aspx?blog=#blogSlug#", urlCategory);
			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);


            ParamsDefinition["AvailableBlogCustomFieldsForAuthor"] =
                new BXParamMultiSelection(
                    GetMessageRaw("Param.AvailableBlogCustomFieldsForAuthor"),
                    string.Empty,
                    customFieldCategory
                    );

            ParamsDefinition["AvailableBlogCustomFieldsForModerator"] =
                new BXParamMultiSelection(
                    GetMessageRaw("Param.AvailableBlogCustomFieldsForModerator"),
                    string.Empty,
                    customFieldCategory
                    );
		}

        protected override void LoadComponentDefinition()
        {
            //AvailableBlogGroups, ObligatoryBlogGroups
            IList<BXParamValue> availableBlogGroupsValues = ((BXParamMultiSelection)ParamsDefinition["AvailableBlogGroups"]).Values;
            if(availableBlogGroupsValues.Count > 0)
                availableBlogGroupsValues.Clear();
            IList<BXParamValue> obligatoryBlogGroupsValues = ((BXParamMultiSelection)ParamsDefinition["ObligatoryBlogGroups"]).Values;
            if (obligatoryBlogGroupsValues.Count > 0)
                obligatoryBlogGroupsValues.Clear();

			IList<BXParamValue> categoryIds = ((BXParamSingleSelection)ParamsDefinition["CategoryId"]).Values;
			if (categoryIds.Count > 0)
				categoryIds.Clear();

            BXBlogCategoryCollection categories = BXBlogCategory.GetList(
                new BXFilter(new BXFilterItem(BXBlogCategory.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)),
                new BXOrderBy(new BXOrderByPair(BXBlogCategory.Fields.Name, BXOrderByDirection.Asc)),
                new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlogCategory.Fields.Id, BXBlogCategory.Fields.Name),
                null,
                BXTextEncoder.EmptyTextEncoder
                );
			categoryIds.Add(new BXParamValue(GetMessage("Kernel.All"), ""));
            foreach (BXBlogCategory category in categories)
            {
                BXParamValue value = new BXParamValue(category.Name, category.Id.ToString());
                availableBlogGroupsValues.Add(value);
                obligatoryBlogGroupsValues.Add(value);
				categoryIds.Add(value);
            }
            //---
            //AvailableBlogCustomFieldsForAuthor, AvailableBlogCustomFieldsForModerator
            IList<BXParamValue> availableBlogCustomFieldsForAuthor = ((BXParamMultiSelection)ParamsDefinition["AvailableBlogCustomFieldsForAuthor"]).Values;
            if (availableBlogCustomFieldsForAuthor.Count > 0)
                availableBlogCustomFieldsForAuthor.Clear();

            IList<BXParamValue> availableBlogCustomFieldsForModerator = ((BXParamMultiSelection)ParamsDefinition["AvailableBlogCustomFieldsForModerator"]).Values;
            if (availableBlogCustomFieldsForModerator.Count > 0)
                availableBlogCustomFieldsForModerator.Clear();

            BXCustomFieldCollection fields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.BlogCustomFieldEntityId);
            foreach (BXCustomField field in fields)
            {
                BXParamValue param = new BXParamValue(field.TextEncoder.Decode(field.EditFormLabel), field.Name);
                availableBlogCustomFieldsForAuthor.Add(param);
                availableBlogCustomFieldsForModerator.Add(param);
            }
            //---
        }

		private bool LoadEntities()
		{
			if (componentMode == Mode.Edit)
			{
				BXFilter filter = new BXFilter(
					new BXFilterItem(BXBlog.Fields.Slug, BXSqlFilterOperators.Equal, BlogSlug),
					new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
				);

				if (CategoryId > 0)
					filter.Add(new BXFilterItem(BXBlog.Fields.Categories.CategoryId, BXSqlFilterOperators.Equal, CategoryId));

				if (!BXPrincipal.Current.IsCanOperate(BXBlog.Operations.BlogApprove))
					filter.Add(new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true));

				BXBlogCollection blogCollection = BXBlog.GetList(
                    filter,
                    null,
                    new BXSelectAdd(
						BXBlog.Fields.CustomFields.DefaultFields,
						BXBlog.Fields.Settings), 
                    null, 
                    BXTextEncoder.EmptyTextEncoder
                    );

				if (blogCollection.Count == 0)
				{
					Fatal(ErrorCode.FatalBlogNotFound);
					return false;
				}

				Blog = blogCollection[0];
			}
			else
			{
				int userId = BXPrincipal.Current.GetIdentity().Id;

				BXBlogCollection blogCollection = BXBlog.GetList(
					new BXFilter(
						new BXFilterItem(BXBlog.Fields.Owner.Id, BXSqlFilterOperators.Equal, userId),
						new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite),
						new BXFilterItem(BXBlog.Fields.IsTeam, BXSqlFilterOperators.Equal, false)
					),
					null
				);
				canCreateTeamBlog = Auth.CanCreateTeamBlog;
				canCreatePersonalBlog = blogCollection.Count == 0;
				if (!canCreatePersonalBlog && !canCreateTeamBlog)
				{
					Fatal(ErrorCode.FatalBlogAlreadyExists);
					return false;
				}
			}

			return true;
		}

        private BXBlogAuthorization InternalAuthorization
        {
            get { return auth ?? (auth = new BXBlogAuthorization(blog)); }
        }

		private bool CheckPermissions()
		{
			if (BXIdentity.Current.Id < 1)
			{
				Fatal(ErrorCode.UnauthorizedNotLogIn);
				return false;
			}
            if (componentMode == Mode.Add && !InternalAuthorization.CanCreateBlog)
			{
				canCreatePersonalBlog = false;
				Fatal(ErrorCode.UnauthorizedCreateBlog);
				return false;
			}
            else if (componentMode == Mode.Edit && !InternalAuthorization.CanEditThisBlogSettings)
			{
				Fatal(ErrorCode.UnauthorizedEditBlog);
				return false;
			}

			return true;
		}
		private BXParamsBag<object> GetReplaceParameters()
		{
			BXParamsBag<object> parameters = new BXParamsBag<object>();
			parameters["BlogSlug"] = BlogSlug;
			if (Blog != null)
				parameters["BlogId"] = Blog.Id;
			return parameters;
		}
		private void Fatal(ErrorCode code)
		{
			if (code == ErrorCode.FatalException)
				throw new InvalidOperationException("Use method with Exception argument");
			fatalError = code;
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}
		private void Fatal(Exception ex)
		{
			if (ex == null)
				throw new ArgumentNullException("ex");

			fatalError = ErrorCode.FatalException;
			fatalException = ex;
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}

		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.FatalBlogAlreadyExists:
					return GetMessage("Error.FatalBlogAlreadyExists");
				case ErrorCode.FatalBlogNotFound:
					return GetMessage("Error.FatalBlogNotFound");
                case ErrorCode.FatalNoGroupsButAdjustingDisabled:
                    return GetMessage("Error.FatalNoGroupsButAdjustingDisabled");
				case ErrorCode.UnauthorizedNotLogIn:
					return componentMode == Mode.Edit ? GetMessage("Error.UnauthorizedNotLogIn.Edit") : GetMessage("Error.UnauthorizedNotLogIn.Add");
				case ErrorCode.UnauthorizedCreateBlog:
					return GetMessage("Error.UnauthorizedCreateBlog");
				case ErrorCode.UnauthorizedEditBlog:
					return GetMessage("Error.UnauthorizedEditBlog");
				default:
					return GetMessage("Error.Unknown");
			}
		}
		public bool Validate()
		{
			ErrorSummary.Clear();

            this.data.BlogName = this.data.BlogName != null ? this.data.BlogName.Trim() : null;
            this.data.BlogDescription = this.data.BlogDescription != null ? this.data.BlogDescription.Trim() : null;

            if (BXStringUtility.IsNullOrTrimEmpty(this.data.BlogName))
				ErrorSummary.Add(GetMessage("Error.BlogNameRequired"));

			if (componentMode == Mode.Add)
			{
                if (BXStringUtility.IsNullOrTrimEmpty(this.data.BlogSlug))
					ErrorSummary.Add(GetMessage("Error.BlogSlugRequired"));
                else if (!BXBlog.SlugRegex.IsMatch(this.data.BlogSlug))
					ErrorSummary.Add(GetMessage("Error.BlogSlugInvalid"));
                else if (SlugIgnoreList.Contains(this.data.BlogSlug))
                    ErrorSummary.Add(String.Format(GetMessage("Error.BlogSlugInIgnoreList"), this.data.BlogSlug));
                else if (this.data.BlogSlug.Length < MinSlugLength || this.data.BlogSlug.Length > MaxSlugLength)
					ErrorSummary.Add(String.Format(GetMessage("Error.SlugLength"), MinSlugLength, MaxSlugLength));
                else if (BXBlog.GetBySlug(this.data.BlogSlug) != null)
					ErrorSummary.Add(GetMessage("Error.BlogSlugAlreadyExists"));
			}

            if (this.data.Categories == null || this.data.Categories.Length <= 0)
				ErrorSummary.Add(GetMessage("Error.BlogCategoryRequired"));
			else if (data.Categories.Length > MaxBlogGroups)
				ErrorSummary.Add(String.Format(GetMessage("Error.BlogCategoryLimit"), MaxBlogGroups));
			else
			{
                foreach (int categoryId in this.data.Categories)
				{
					if (!SiteCategories.Exists(delegate(BXBlogCategory category) { return categoryId == category.Id; }))
					{
						ErrorSummary.Add(GetMessage("Error.BlogWrongCategory"));
						break;
					}
				}
			}

            List<string> customFieldErrors = new List<string>();
            foreach (CustomFieldEditor ed in BlogCustomFieldEditors)
                ed.Save(this.data.CustomProperties, customFieldErrors);

            if (customFieldErrors.Count > 0)
                foreach (string customFieldError in customFieldErrors)
                    ErrorSummary.Add(customFieldError);

			if (editPermissions != null)
			{
				if (!editPermissions.Validate(null))
					ErrorSummary.AddRange(editPermissions.Errors);
			}

			if (editBlogUsers != null)
			{
				if (!editBlogUsers.Validate(null))
					ErrorSummary.AddRange(editBlogUsers.Errors);
			}

			return ErrorSummary.Count == 0;
		}
		public void Save()
		{
			if (FatalError != ErrorCode.None)
				return;
			try
			{
				if (blog == null)
				{
					blog = new BXBlog();
					blog.OwnerId = ((BXIdentity)BXPrincipal.Current.Identity).Id;
					blog.Active = true;
				}

				BXBlogAuthorization auth = new BXBlogAuthorization(blog);

				if (componentMode == Mode.Add && CanCreateTeamBlog && data.IsTeam)
					blog.IsTeam = true;				
				
				if (auth.CanApproveBlog && this.data.BlogActiveOptional.HasValue)
                    blog.Active = this.data.BlogActiveOptional.Value;

                blog.Name = this.data.BlogName;
                blog.Description = this.data.BlogDescription;

				if (componentMode == Mode.Add)
                    blog.Slug = this.data.BlogSlug;

                blog.NotifyOfComments = this.data.NotifyComments;
                blog.SetCategoryIds(this.data.Categories);
                blog.CustomValues.Assign(this.data.CustomProperties);

				if(auth.CanApproveComments)
				{
					BXBlogSettings settings = blog.Settings;
					if(settings == null)
						blog.Settings = settings = new BXBlogSettings();

					settings.EnableCommentModeration = data.EnableCommentModeration;
					settings.CommentModerationMode = !string.IsNullOrEmpty(data.CommentModerationMode) ? (BXBlogCommentModerationMode)Enum.Parse(typeof(BXBlogCommentModerationMode), data.CommentModerationMode, true) : BXBlogCommentModerationMode.Filter;
					settings.CommentModerationFilter.LinkThreshold = data.CommentModerationLinkThreshold;
					settings.CommentModerationFilter.SetStopList(data.CommentModerationStopList);
				}

                using (BXSqlTransaction tran = new BXSqlTransaction())
                {
                    blog.Save(tran.Connection, tran);
                    if (Syndication != null)
                    {
                        Syndication.BlogId = blog.Id;
                        Syndication.Enabled = this.data.EnableSyndication && blog.Active;
                        Syndication.FeedUrl = this.data.SyndicationFeedUrl;
                        Syndication.Updateable = this.data.SyndicationUpdateableContent;
                        Syndication.RedirectToComments = this.data.SyndicationRedirectToComments;
                        Syndication.Save(tran.Connection, tran);
                    }

					if (editPermissions != null)
						editPermissions.Save(tran);

					if (editBlogUsers != null)
						editBlogUsers.Save(tran);

                    //BXCustomEntityManager.SaveEntity(BXBlogModuleConfiguration.BlogCustomFieldEntityId, blog.Id, data.CustomProperties, tran);
                    tran.Commit();
                }
                this.blogSlug = this.data.BlogSlug;

				string redirectUrl = Parameters.GetString("RedirectUrlTemplate");
				if (BXStringUtility.IsNullOrTrimEmpty(redirectUrl))
					Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
				RedirectTemplateUrl(redirectUrl, GetReplaceParameters());
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex)
			{
				Fatal(ex);
			}
		}
		
		public Control GetGroupsEditor()
		{
			if (editPermissions == null && Blog != null && Blog.IsTeam)
			{
				editPermissions = (Bitrix.Blog.UI.EditPermissions)LoadControl("~/bitrix/controls/Blog/EditPermissions.ascx");
				editPermissions.BlogId = Blog.Id;
			}

			return editPermissions;
		}
		public Control GetUsersEditor()
		{
			if (editBlogUsers == null && Blog != null && Blog.IsTeam)
			{
				editBlogUsers = (Bitrix.Blog.UI.EditBlogUsers)LoadControl("~/bitrix/controls/Blog/EditBlogUsers.ascx");
				editBlogUsers.BlogId = Blog.Id;
				editBlogUsers.UserProfileTemplate = Component.ComponentCache.GetString("UserProfileUrlTemplate");
			}

			return editBlogUsers;
		}

		//NESTED CLASSES
		public enum Mode
		{
			Add,
			Edit
		}

		public enum ErrorCode
		{
			None = 0,
			Fatal = 1,
			Unauthorized = 2,

			FatalBlogNotFound = Fatal | 4,
			FatalException = Fatal | 8,
            FatalBlogAlreadyExists = Fatal | 8 | 4,
			FatalComponentNotExecuted = Fatal | 16,
            FatalNoGroupsButAdjustingDisabled = Fatal | 16 | 8,

			UnauthorizedNotLogIn = Unauthorized | 4 | 8,
			UnauthorizedCreateBlog = Unauthorized | 4,
			UnauthorizedEditBlog = Unauthorized | 8
		}

		public class Data
		{
			private BlogEditComponent component;
			private bool? blogActive;
			private string blogName;
			private string blogDescription;
			private string blogSlug;
			private bool notifyComments;
			private int[] categories;
            private BXCustomPropertyCollection customProperties;
            private bool enableSyndication = false;
            private string syndicationFeedUrl = "http://";
            private bool syndicationUpdateableContent = true;
            private bool syndicationRedirectToComments = true;

			public bool? BlogActiveOptional
			{
				get { return blogActive; }
				set { blogActive = value; }
			}

			public bool BlogActive
			{
				get { return blogActive ?? false; }
				set { blogActive = value; }
			}
			public string BlogName
			{
				get { return blogName; }
				set { blogName = value; }
			}
			public string BlogDescription
			{
				get { return blogDescription; }
				set { blogDescription = value; }
			}
			public string BlogSlug
			{
				get { return blogSlug; }
				set { blogSlug = value; }
			}
			public bool NotifyComments
			{
				get { return notifyComments; }
				set { notifyComments = value; }
			}
			public int[] Categories
			{
				get 
				{ 
					if (categories == null)
						categories = component.Blog != null ? component.Blog.GetCategoryIds() : new int[0];

					return categories; 
				}
				set { categories = value; }
			}

            public bool EnableSyndication
            {
                get { return enableSyndication; }
                set { enableSyndication = value; }
            }

            public string SyndicationFeedUrl
            {
                get { return syndicationFeedUrl; }
                set { syndicationFeedUrl = !string.IsNullOrEmpty(value) ? value : "http://"; }
            }

            public bool SyndicationUpdateableContent
            {
                get { return syndicationUpdateableContent; }
                set { syndicationUpdateableContent = value; }
            }

            public bool SyndicationRedirectToComments
            {
                get { return syndicationRedirectToComments; }
                set { syndicationRedirectToComments = value; }
            }

            public int Id
            {
                get { return this.component.Blog != null ? this.component.Blog.Id : 0; }
            }

			public bool IsTeam { get; set; }

            public BXCustomPropertyCollection CustomProperties
            {
                //get { return this.customProperties ?? (this.customProperties = Id > 0 ? BXCustomEntityManager.GetProperties(BXBlogModuleConfiguration.BlogCustomFieldEntityId, Id) : new BXCustomPropertyCollection()); }
                get 
                {
                    if (customProperties != null)
                        return customProperties;

                    this.customProperties = new BXCustomPropertyCollection();
                    if (component.Blog != null)
                        customProperties.Assign(component.Blog.CustomValues);
                    return customProperties;
                }
            }

			private bool? enableCommentModeration = null;
			public bool EnableCommentModeration
			{
				get { return (enableCommentModeration ?? (enableCommentModeration = BlogSettings != null ? BlogSettings.EnableCommentModeration : false)).Value; }
				set { enableCommentModeration = value; }
			}

			private string commentModerationMode = null;
			public string CommentModerationMode
			{
				get { return commentModerationMode ?? (commentModerationMode = (BlogSettings != null ? BlogSettings.CommentModerationMode : BXBlogCommentModerationMode.Filter).ToString("G").ToUpperInvariant()); }
				set { commentModerationMode = value ?? string.Empty; }
			}

			private int? commentModerationLinkThreshold = null;
			public int CommentModerationLinkThreshold
			{
				get { return (commentModerationLinkThreshold ?? (commentModerationLinkThreshold = BlogSettings != null ? BlogSettings.CommentModerationFilter.LinkThreshold : 1)).Value; }
				set { commentModerationLinkThreshold = value >= 0 ? value : 0; }
			}

			private IList<string> commentModerationStopList = null;
			public IList<string> CommentModerationStopList
			{
				get { return commentModerationStopList ?? (commentModerationStopList = BlogSettings != null ? BlogSettings.CommentModerationFilter.StopList : new string[0]); }
				set { commentModerationStopList = value ?? new string[0]; }
			}

			internal BXBlogSettings BlogSettings
			{
				get { return component.Blog != null ? component.Blog.Settings : null; }
			}

			internal Data(BlogEditComponent component)
			{
				this.component = component;
			}
		}
	}
    public class CustomFieldEditor
    {
        private BlogEditComponent parent = null;
        private BXCustomField field = null;
        private BXCustomType fieldType = null;
        private BXCustomTypePublicEdit editor = null;
        private string formName = string.Empty;
        private string clientId = string.Empty;
        public CustomFieldEditor(BlogEditComponent parent, BXCustomField field)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            this.parent = parent;

            if (field == null)
                throw new ArgumentNullException("field");
            this.field = field;

            this.clientId = string.Concat(parent.ClientID, "_", this.field.Name);
            this.formName = string.Concat(parent.UniqueID, "$", this.field.Name);

            this.fieldType = BXCustomTypeManager.GetCustomType(this.field.CustomTypeId);
            if (this.fieldType == null)
                throw new InvalidOperationException("Could not get custom field type!");

            this.editor = this.fieldType.CreatePublicEditor();
            if (this.editor == null)
                throw new InvalidOperationException("Could not create custom field editor!");
            this.editor.Init(this.field);
        }

        public string ClientID
        {
            get { return this.clientId; }
        }
        public string FormName
        {
            get { return this.formName; }
        }

        public BXCustomField Field
        {
            get { return this.field; }
        }

        public string Caption
        {
            get
            {
                return HttpUtility.HtmlEncode(this.field.TextEncoder.Decode(this.field.EditFormLabel));
            }
        }
        public bool IsRequired
        {
            get { return this.field.Mandatory; }
        }
        public void Load(BXCustomPropertyCollection properties)
        {
            if (properties == null || properties.Count == 0)
                return;

            BXCustomProperty p = null;
            if (properties.TryGetValue(this.field.Name, out p))
                this.editor.Load(p);
        }
        public string Render()
        {
            return this.editor.Render(this.formName, this.clientId);
        }
        public void Save(BXCustomPropertyCollection properties, ICollection<string> errors)
        {
            this.editor.DoSave(this.formName, properties, errors);
        }
        public void Validate(ICollection<string> errors)
        {
            this.editor.DoValidate(this.formName, errors);
        }
    }
	public class BlogEditTemplate : BXComponentTemplate<BlogEditComponent>
	{
		protected virtual void LoadData(BlogEditComponent.Data data)
		{
		}
		protected virtual void SaveData(BlogEditComponent.Data data)
		{
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (!IsPostBack && Component.FatalError == BlogEditComponent.ErrorCode.None)
				LoadData(Component.ComponentData);
		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

            if ((Component.FatalError & BlogEditComponent.ErrorCode.Fatal) != 0)
                BXError404Manager.Set404Status(Response);
            else if (Page.Form != null && string.IsNullOrEmpty(Page.Form.Enctype))
                    Page.Form.Enctype = "multipart/form-data";            
		}

		protected void SaveClick(object sender, EventArgs e)
		{
			SaveData(Component.ComponentData);
			Component.ComponentData.IsTeam = false;

			if (Component.Validate())
				Component.Save();
		}
		protected void SaveTeamClick(object sender, EventArgs e)
		{
			SaveData(Component.ComponentData);
			Component.ComponentData.IsTeam = true;

			if (Component.Validate())
				Component.Save();
		}
	}
}
