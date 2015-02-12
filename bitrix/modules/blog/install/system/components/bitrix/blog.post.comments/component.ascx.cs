using System;
using System.IO;
using System.Web.UI;
using Bitrix.CommunicationUtility;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services.Text;
using Bitrix.UI;
using System.Collections.Generic;
using System.Web;
using Bitrix.Modules;
using Bitrix.Services;
using System.Text;
using System.Collections.ObjectModel;

namespace Bitrix.Blog.Components
{
	public partial class BlogPostCommentsComponent : BXComponent
	{
		private string blogSlug;
		private int? postId;
		private int? commentId;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private Exception fatalException;
		private bool templateIncluded;
		private BXBlog blog;
		private BXBlogPost post;
		private BXBlogComment targetComment;
		private PostInfo postInfo;
		private BXBlogCommentChain processor;
		private int maxWordLength = -1;
		private BXBlogAuthorization auth;
		private BXParamsBag<object> replace;
		private List<CommentInfo> comments;
		private int count = -1;
		private bool? showGuestEmail;
		private bool? requireGuestEmail;
		private bool? requireGuestCaptcha;

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
		public string BlogSlug
		{
			get
			{
				return blogSlug ?? (blogSlug = Parameters.GetString("BlogSlug"));
			}
		}
		public int PostId
		{
			get
			{
				return (postId ?? (postId = Parameters.GetInt("PostId"))).Value;
			}
		}
		public int CommentId
		{
			get
			{
				return (commentId ?? (commentId = Parameters.GetInt("CommentId"))).Value;
			}
		}
		public BXBlog Blog
		{
			get
			{
				return blog;
			}
			set
			{
				blog = value;
			}
		}
		public BXBlogPost BlogPost
		{
			get
			{
				return post;
			}
			set
			{
				post = value;
			}
		}
		public PostInfo Post
		{
			get
			{
				return postInfo;
			}
		}
		public List<CommentInfo> Comments
		{
			get
			{
				return comments;
			}
		}


		public int MaxWordLength
		{
			get
			{
				return (maxWordLength != -1) ? maxWordLength : (maxWordLength = Math.Max(0, Parameters.GetInt("MaxWordLength", 15)));
			}
			set
			{
				maxWordLength = Math.Max(0, value);
				Parameters["MaxWordLength"] = maxWordLength.ToString();
			}
		}
		public string PostEditUrlTemplate
		{
			get
			{
				return Parameters.GetString("PostEditUrlTemplate");
			}
			set
			{
				Parameters["PostEditUrlTemplate"] = value;
			}
		}
		public string PostViewUrlTemplate
		{
			get
			{
				return Parameters.GetString("PostViewUrlTemplate");
			}
			set
			{
				Parameters["PostViewUrlTemplate"] = value;
			}
		}
		public string UserProfileUrlTemplate
		{
			get
			{
				return Parameters.GetString("UserProfileUrlTemplate");
			}
			set
			{
				Parameters["UserProfileUrlTemplate"] = value;
			}
		}
		public string BlogUrlTemplate
		{
			get
			{
				return Parameters.GetString("BlogUrlTemplate");
			}
			set
			{
				Parameters["BlogUrlTemplate"] = value;
			}
		}
		public string CommentUrlTemplate
		{
			get
			{
				return Parameters.GetString("CommentUrlTemplate");
			}
			set
			{
				Parameters["CommentUrlTemplate"] = value;
			}
		}
        public bool EnableVotingForComment
        {
            get
            {
                return Parameters.GetBool("EnableVotingForComment", false);
            }
            set
            {
                Parameters["EnableVotingForComment"] = value.ToString();
            }
        }
        private IList<string> rolesAuthorizedToVote = null;
        public IList<string> RolesAuthorizedToVote
        {
            get
            {
                return this.rolesAuthorizedToVote ?? (this.rolesAuthorizedToVote = Parameters.GetListString("RolesAuthorizedToVote"));
            }
            set
            {
                Parameters["RolesAuthorizedToVote"] = BXStringUtility.ListToCsv(this.rolesAuthorizedToVote = value ?? new List<string>());
            }
        }
		public BXBlogAuthorization Auth
		{
			get
			{
				return auth;
			}
		}

        public bool EnableComments
        {
            get 
            {
                return post != null && post.EnableComments;
            }
        }


        private bool isPostSyndicationLoaded = false;
        private BXBlogPostSyndication postSyndication = null;
        public BXBlogPostSyndication PostSyndication
        {
            get 
            {
                if (isPostSyndicationLoaded)
                    return postSyndication;
                isPostSyndicationLoaded = true;
                return (postSyndication = post != null ? post.Syndication : null);
            }
        }

		public int CategoryId
		{
			get { return Parameters.GetInt("CategoryId", 0); }
		}
        

        public bool IsSyndicatedPost
        {
            get { return PostSyndication != null; }
        }

        public string SyndicatedPostCommentsUrl
        {
            get 
            {
                return PostSyndication != null ? (!string.IsNullOrEmpty(PostSyndication.CommentsUrl) ? PostSyndication.CommentsUrl : PostSyndication.PostUrl) : string.Empty;
            }
        }

		public bool ShowGuestEmail
		{
			get
			{
				if (showGuestEmail == null)
				{
					string s = Parameters.GetString("GuestEmail", "required");
					showGuestEmail = string.Equals(s, "visible", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(s, "required", StringComparison.OrdinalIgnoreCase);
				}
				return showGuestEmail.Value;
			}
		}
		public bool RequireGuestEmail
		{
			get
			{
				return requireGuestEmail ?? (requireGuestEmail = string.Equals(Parameters.GetString("GuestEmail", "required"), "required", StringComparison.OrdinalIgnoreCase)).Value;
			}
		}
		public bool RequireGuestCaptcha
		{
			get
			{
				return requireGuestCaptcha ?? (requireGuestCaptcha = Parameters.GetBool("GuestCaptcha", true)).Value;
			}
		}
		public bool IsGuest
		{
			get
			{
				return auth.UserId == 0;
			}
		}

		internal BXBlogCommentChain Processor
		{
			get
			{
				if (processor == null)
				{
					processor = new BXBlogCommentChain(blog);
					processor.MaxWordLength = MaxWordLength;
					processor.EnableImages = false;
				}
				return processor;
			}
		}

		/// <summary>
		/// Ид созданного пользователем, но ещё не прошедшего модерацию комментария
		/// </summary>
		public int HiddenCommentId
		{
			get
			{
				object obj = Session != null ? Session[string.Concat("_BX_BLOG_POST_COMMENTS_SHOULD_BE_APPROVED_", ClientID.GetHashCode().ToString())] : null;
				return obj != null ? Convert.ToInt32(obj) : 0;
			}
			set
			{
				if(Session == null)
					return;

				string key = string.Concat("_BX_BLOG_POST_COMMENTS_SHOULD_BE_APPROVED_", ClientID.GetHashCode().ToString());

				if(value <= 0)
					Session.Remove(key);
				else
					Session[key] = value;
			}
		}

		public event EventHandler<CommentEventArgs> CommentAdding;
		public event EventHandler<CommentEventArgs> CommentAdded;

		public event EventHandler<CommentEventArgs> CommentHiding;
		public event EventHandler<CommentEventArgs> CommentHidden;

		public void AddComment(int parentCommentId, string content, CommentData additionalData)
		{
			if (fatalError != ErrorCode.None)
				return;

			if (parentCommentId < 0)
				return;

			if (!auth.CanCommentThisPost)
			{
				Fatal(ErrorCode.UnauthorizedComment);
				return;
			}
			BXBlogComment nc = null;
			try
			{
				BXBlogComment pc = null;
				if (parentCommentId != 0)
				{
					pc = BXBlogComment.GetById(parentCommentId);
					if (pc == null || pc.PostId != post.Id || pc.BlogId != blog.Id)
					{
						Fatal(ErrorCode.FatalCommentNotFound);
						return;
					}
					parentCommentId = pc.Id;
				}

				nc = new BXBlogComment();
				nc.BlogId = blog.Id;
				nc.PostId = post.Id;
				nc.ParentId = parentCommentId;
				nc.Content = content;

				nc.AuthorId = auth.UserId;
				nc.AuthorIP = Request.UserHostAddress;
				if (additionalData != null)
				{
					if (additionalData.UserName != null)
						nc.AuthorName = additionalData.UserName;
					if (additionalData.UserEmail != null)
						nc.AuthorEmail = additionalData.UserEmail;
				}

				if (auth.UserId != 0)
				{
					BXBlogCollection collection = BXBlog.GetList(
						new BXFilter(
							new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true),
							new BXFilterItem(BXBlog.Fields.Owner.Id, BXSqlFilterOperators.Equal, auth.UserId),
							new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
						),
						new BXOrderBy(
							new BXOrderByPair(BXBlog.Fields.Sort, BXOrderByDirection.Asc)
						),
						new BXSelect(BXBlog.Fields.Id),
						null
					);
					if (collection.Count > 0)
						nc.AuthorBlogId = collection[0].Id;
				}

				nc.IsApproved = !auth.ShouldBeModerated(nc, Processor);

				CommentEventArgs args = null;
				if (CommentAdding != null)
					CommentAdding(this, args ?? (args = new CommentEventArgs(nc, pc)));

				nc.Save();

				if (CommentAdded != null)
					CommentAdded(this, args ?? (args = new CommentEventArgs(nc, pc)));

				if(!nc.IsApproved)
					HiddenCommentId = nc.Id;

				//send mail
				if (parentCommentId == 0 && post.Author != null && post.Author.Id != nc.AuthorId && blog.NotifyOfComments)
				{
					SendCommentMail(post.Author.User.Email, null, nc);
				}
				else if (parentCommentId != 0 && pc.Author != null && pc.AuthorId != nc.AuthorId)
				{
					SendCommentMail(pc.Author.User.Email, pc, nc);
				}
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}
			replace["CommentId"] = nc.Id;
			RedirectTemplateUrl(CommentUrlTemplate, replace);
			replace.Remove("CommentId");
		}

		private void SendCommentMail(string ownerEmail, BXBlogComment parent, BXBlogComment current)
		{
			BXCommand cmd = new BXCommand(parent != null ? "Bitrix.Blog.BlogNewComment2Comment" :  "Bitrix.Blog.BlogNewComment");
			cmd.Parameters["@site"] = DesignerSite;

			
			cmd.Parameters["BlogId"] = blog.Id;
			cmd.Parameters["BlogName"] = blog.TextEncoder.Decode(blog.Name);
			cmd.Parameters["BlogSlug"] = blog.Slug;
			cmd.Parameters["PostId"] = post.Id;
			cmd.Parameters["PostTitle"] = post.TextEncoder.Decode(post.Title);
			cmd.Parameters["CommentId"] = current.Id;
			cmd.Parameters["Comment"] = Processor.StripBBCode(current.Content);
			cmd.Parameters["CommentDate"] = current.DateCreated;
			cmd.Parameters["CommentAuthor"] = current.TextEncoder.Decode( current.AuthorName);
			cmd.Parameters["CommentAuthorId"] = current.AuthorId;
			cmd.Parameters["EmailTo"] = ownerEmail;

			cmd.Parameters["PostUrl"] = MakeAbsolute(ResolveTemplateUrl(PostViewUrlTemplate, replace));
			
			replace["CommentId"] = current.Id;
			cmd.Parameters["CommentUrl"] = MakeAbsolute(ResolveTemplateUrl(CommentUrlTemplate, replace));
			cmd.Parameters["ReplyCommentUrl"] = MakeAbsolute(ResolveTemplateUrl(CommentUrlTemplate, replace));
			replace.Remove("CommentId");

			cmd.Send();
		}
		public void Hide(int commentId)
		{
            Hide(commentId, 0);
		}

        public void Hide(int commentId, int focusedId)
        {
            if (fatalError != ErrorCode.None)
                return;

            if (commentId <= 0)
                return;

            BXBlogComment c = BXBlogComment.GetById(commentId);
            if (c == null || c.PostId != post.Id || c.BlogId != blog.Id)
                return;

            BXBlogAuthorization a = new BXBlogAuthorization(blog, post, c);

            if (!a.CanDeleteThisComment)
            {
                Fatal(ErrorCode.UnauthorizedDelete);
                return;
            }

            c.MarkedForDelete = true;
            CommentEventArgs args = null;
            if (CommentHiding != null)
                CommentHiding(this, args ?? (args = new CommentEventArgs(c, null)));
            c.Save();
            if (CommentHidden != null)
                CommentHidden(this, args ?? (args = new CommentEventArgs(c, null)));

            StringBuilder postViewUrlTemplateSb = new StringBuilder(PostViewUrlTemplate);
            if (postViewUrlTemplateSb.Length == 0)
                postViewUrlTemplateSb.Append('?');
            //if (focusedId > 0 && postViewUrlTemplateSb.ToString().IndexOf('#') < 0)
            if (focusedId > 0)
                postViewUrlTemplateSb.Append("#comment").Append(focusedId.ToString());

            RedirectTemplateUrl(postViewUrlTemplateSb.ToString(), replace);
        }

        public void Approve(int commentId, bool approve)
        {
            if (fatalError != ErrorCode.None)
                return;

            if (commentId <= 0)
                return;

            BXBlogComment c = BXBlogComment.GetById(commentId);
            if (c == null || c.PostId != post.Id || c.BlogId != blog.Id)
                return;

            BXBlogAuthorization a = new BXBlogAuthorization(blog, post, c);

            if (!a.CanApproveThisComment)
            {
                Fatal(ErrorCode.UnauthorizedDelete);
                return;
            }

			if(c.IsApproved == approve)
				return;

            c.IsApproved = approve;

			CommentEventArgs args = new CommentEventArgs(c, null);
			if(!approve && CommentHiding != null)
				CommentHiding(this, args);

            c.Save();

            if (!approve && CommentHidden != null)
                CommentHidden(this, args);

            StringBuilder postViewUrlTemplateSb = new StringBuilder(PostViewUrlTemplate);

            if (postViewUrlTemplateSb.Length == 0)
                postViewUrlTemplateSb.Append('?');

            postViewUrlTemplateSb.Append("#comment").Append(commentId.ToString());

            RedirectTemplateUrl(postViewUrlTemplateSb.ToString(), replace);
        }

		public void Preview(string input, HtmlTextWriter writer)
		{
			Processor.Process(input, writer);
		}
		public string Preview(string input)
		{
			using (StringWriter s = new StringWriter())
			{
				Processor.Process(input, s);
				return s.ToString();
			}
		}
		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.FatalPostNotFound:
					return GetMessage("Error.PostNotFound");
				case ErrorCode.FatalPageNotFound:
					return GetMessage("Error.PageNotFound");
				case ErrorCode.FatalCommentNotFound:
					return GetMessage("Error.CommentNotFound");
				case ErrorCode.UnauthorizedView:
					return GetMessage("Error.UnauthorizedView");
				case ErrorCode.UnauthorizedDelete:
					return GetMessage("Error.UnauthorizedDelete");
				case ErrorCode.UnauthorizedComment:
					return GetMessage("Error.UnauthorizedComment");
				default:
					return GetMessage("Error.Unknown");
			}
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
		private bool FillBlogInfo(BXBlogComment input)
		{
			if (input.AuthorBlog != null && input.AuthorBlog.Categories != null)
			{
				foreach (BXBlogCategory c in input.AuthorBlog.Categories)
					foreach (string s in c.GetSiteIds())
						if (s == DesignerSite)
						{
							replace["BlogId"] = input.AuthorBlog.Id;
							replace["BlogSlug"] = input.AuthorBlog.Slug;
							return true;
						}
			}
			replace["BlogId"] = 0;
			replace["BlogSlug"] = "";
			return false;
		}
		private int CountPages()
		{
			if (count >= 0)
				return count;
			BXBlogCommentCollection collection = BXBlogComment.GetList(
				new BXFilter(
						new BXFilterItem(BXBlogComment.Fields.Post.Id, BXSqlFilterOperators.Equal, post.Id),
						new BXFilterItem(BXBlogComment.Fields.LiveRootNodeIndex, BXSqlFilterOperators.Greater, 0)
				),
				new BXOrderBy(
					new BXOrderByPair(BXBlogComment.Fields.LiveRootNodeIndex, BXOrderByDirection.Desc)
				),
				new BXSelect(BXBlogComment.Fields.LiveRootNodeIndex),
				new BXQueryParams(new BXPagingOptions(0, 1))
			);
			if (collection.Count == 0)
				count = 0;
			else 
				count = collection[0].LiveRootNodeIndex;

			return count;


			//return count = BXBlogComment.Count(
			//        new BXFilter(
			//            new BXFilterItem(BXBlogComment.Fields.Post.Id, BXSqlFilterOperators.Equal, post.Id),
			//            new BXFilterItem(BXBlogComment.Fields.Parent.Id, BXSqlFilterOperators.Equal, 0),
			//            new BXFilterItem(BXBlogComment.Fields.LiveRootNodeIndex, BXSqlFilterOperators.NotEqual, 0)
			//        )
			//    );
		}
		private bool LoadPost()
		{
			if (CommentId > 0)
			{
				var filter = new BXFilter(
						new BXFilterItem(BXBlogComment.Fields.Id, BXSqlFilterOperators.Equal, CommentId),
						new BXFilterItem(BXBlogComment.Fields.Post.Blog.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
					);

				if (CategoryId > 0)
					filter.Add(new BXFilterItem(BXBlogComment.Fields.Post.Blog.Categories.CategoryId, BXSqlFilterOperators.Equal, CategoryId));

				BXBlogCommentCollection commentsCollection = BXBlogComment.GetList(
					filter,
					null,
					new BXSelectAdd(BXBlogComment.Fields.Post, BXBlogComment.Fields.Blog, BXBlogComment.Fields.Post.Author, BXBlogComment.Fields.Post.Author.User, BXBlogComment.Fields.Post.Author.User.Image),
					null
				);
				if (commentsCollection.Count == 0)
				{
					Fatal(ErrorCode.FatalCommentNotFound);
					return false;
				}

				targetComment = commentsCollection[0];
			
				if (post == null)
					post = targetComment.Post;

				if (post == null || PostId > 0 && post.Id != PostId)
				{
					Fatal(ErrorCode.FatalCommentNotFound);
					return false;
				}

				postId = post.Id;
				return true;
			}

			if (post == null)
			{
				if (PostId < 0)
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return false;
				}

				var postFilter = new BXFilter(
						new BXFilterItem(BXBlogPost.Fields.Id, BXSqlFilterOperators.Equal, PostId),
						new BXFilterItem(BXBlogPost.Fields.Blog.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
					);

				if (CategoryId > 0)
					postFilter.Add(new BXFilterItem(BXBlogPost.Fields.Blog.Categories.CategoryId, BXSqlFilterOperators.Equal, CategoryId));

				
				BXBlogPostCollection posts = BXBlogPost.GetList(
					postFilter,
					null,
					new BXSelectAdd(BXBlogPost.Fields.Blog, BXBlogPost.Fields.Author, BXBlogPost.Fields.Author.User, BXBlogPost.Fields.Author.User.Image),
					null
				);

				if (posts.Count > 0)
					post = posts[0];
			}

			if (post == null)
			{
				Fatal(ErrorCode.FatalPostNotFound);
				return false;
			}

			return true;
		}
		private string MakeAbsolute(string url)
		{
			if (url.StartsWith("?"))
			{
				UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
				uri.Query = url.Substring(1);
				return uri.Uri.ToString();
			}
			
			return new Uri(BXSefUrlManager.CurrentUrl, url).ToString();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;

			try
			{
				if (!LoadPost())
					return;
				if (blog == null)
					blog = post.Blog;
				if (blog == null || !string.IsNullOrEmpty(BlogSlug) && blog.Slug != BlogSlug)
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return;
				}

				auth = new BXBlogAuthorization(blog, post);

				if (!auth.CanReadThisPost || !auth.CanReadThisPostComments)
				{
					Fatal(ErrorCode.UnauthorizedView);
					return;
				}

				string title = post.TextEncoder.Decode(post.Title);

				replace = new BXParamsBag<object>();
				replace["PostId"] = post.Id;
				DateTime date = post.DatePublished;
				replace["PostYear"] = date.Year;
				replace["PostMonth"] = date.Month;
				replace["PostDay"] = date.Day;
				replace["BlogId"] = blog.Id;
				replace["BlogSlug"] = blog.Slug;


				//Determine page
				BXPagingParams pagingParams = PreparePagingParams();
				if (pagingParams.AllowPaging && targetComment != null && targetComment.LiveRootNodeIndex != 0)
				{
					BXPagingHelper helper = ResolvePagingHelper(CountPages(), pagingParams);
					pagingParams.Page = helper.GetOuterIndex(helper.GetPageIndexForItem(targetComment.LiveRootNodeIndex - 1));
				}

				bool legalPage;
				BXQueryParams queryParams = PreparePaging(
					pagingParams,
					CountPages,
					replace,
					out legalPage
				);

				if (!legalPage)
				{
					Fatal(ErrorCode.FatalPageNotFound);
					return;
				}
				BXFilter commentFilter = new BXFilter();
				commentFilter.Add(new BXFilterItem(BXBlogComment.Fields.Post.Id, BXSqlFilterOperators.Equal, post.Id));
				if (queryParams != null && queryParams.AllowPaging)
				{
					commentFilter.Add(new BXFilterItem(BXBlogComment.Fields.LiveRootNodeIndex, BXSqlFilterOperators.Greater, queryParams.PagingStartIndex));
					commentFilter.Add(new BXFilterItem(BXBlogComment.Fields.LiveRootNodeIndex, BXSqlFilterOperators.LessOrEqual, queryParams.PagingStartIndex + queryParams.PagingRecordCount));
				}
				else
					commentFilter.Add(new BXFilterItem(BXBlogComment.Fields.LiveRootNodeIndex, BXSqlFilterOperators.NotEqual, 0));

				//commentFilter.Add(new BXFilterItem(BXBlogComment.Fields.MarkedForDelete, BXSqlFilterOperators.Equal, false)); //?

				if(!auth.CanApproveComments)
					commentFilter.Add(new BXFilterItem(BXBlogComment.Fields.IsApproved, BXSqlFilterOperators.Equal, true));

				BXBlogCommentCollection commentCollection = BXBlogComment.GetList(
					commentFilter,
					new BXOrderBy(new BXOrderByPair(BXBlogComment.Fields.LeftMargin, BXOrderByDirection.Asc)),
					new BXSelectAdd(
						BXBlogComment.Fields.AuthorBlog.Id,
						BXBlogComment.Fields.AuthorBlog.Slug,
						//BXBlogComment.Fields.AuthorBlog.Categories,
						//BXBlogComment.Fields.AuthorBlog.Categories.Category,
						BXBlogComment.Fields.AuthorBlog.Categories.Category.Sites.SiteId,
						BXBlogComment.Fields.Author,
						BXBlogComment.Fields.Author.User,
						BXBlogComment.Fields.Author.User.Image
					),
					null
				);

				Dictionary<int, CommentInfo> cache = new Dictionary<int, CommentInfo>();
				comments = commentCollection.ConvertAll<CommentInfo>(delegate(BXBlogComment input)
				{
					replace["UserId"] = input.AuthorId;
					replace["CommentId"] = input.Id;

					CommentInfo info = new CommentInfo();

					info.BlogHref = FillBlogInfo(input) ? Encode(ResolveTemplateUrl(BlogUrlTemplate, replace)) : null;
					info.UserProfileHref = input.AuthorId > 0 ? Encode(ResolveTemplateUrl(UserProfileUrlTemplate, replace)) : null;

					info.processor = Processor;
					info.Comment = input;
					info.AuthorNameHtml = BXWordBreakingProcessor.Break(post.TextEncoder.Decode(input.AuthorName), MaxWordLength, true);

					replace["BlogId"] = blog.Id;
					replace["BlogSlug"] = blog.Slug;

					info.CommentHref = Encode(ResolveTemplateUrl(CommentUrlTemplate, replace));
					info.Auth = new BXBlogAuthorization(blog, post, input);
					info.BranchIsDeleted = input.MarkedForDelete;

					CommentInfo parent;
					if (input.ParentId == 0 || !cache.TryGetValue(input.ParentId, out parent))
						parent = null;
					info.Parent = parent;

					cache[input.Id] = info;

					return info;
				});

				replace.Remove("CommentId");
				replace["UserId"] = post.AuthorId;
				replace["BlogId"] = blog.Id;
				replace["BlogSlug"] = blog.Slug;

				postInfo = new PostInfo();
				postInfo.Post = post;
				postInfo.AuthorNameHtml = BXWordBreakingProcessor.Break(post.TextEncoder.Decode(post.AuthorName), MaxWordLength, true);
				postInfo.TitleHtml = BXWordBreakingProcessor.Break(title, MaxWordLength, true);
				postInfo.PostViewHref = Encode(ResolveTemplateUrl(PostViewUrlTemplate, replace));
				postInfo.PostEditHref = Encode(ResolveTemplateUrl(PostEditUrlTemplate, replace));
				postInfo.UserProfileHref = Encode(ResolveTemplateUrl(UserProfileUrlTemplate, replace));
				postInfo.BlogHref = Encode(ResolveTemplateUrl(BlogUrlTemplate, replace));

				for (int i = comments.Count - 1; i >= 0; i--)
				{
					CommentInfo comment = comments[i];

					if (!comment.BranchIsDeleted && comment.Parent != null)
						comment.Parent.BranchIsDeleted = false;
				}
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}

			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
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
			Icon = "images/icon.gif";
			Group = BXBlogModuleConfiguration.GetComponentGroup();

			BXCategory mainCategory = BXCategory.Main,
                urlCategory = BXCategory.UrlSettings,
                additionalSettingsCategory = BXCategory.AdditionalSettings,
                guestsCategory = new BXCategory(GetMessageRaw("Category.GuestParameters"), "guest_parameters", mainCategory.Sort + 100),
                votingCategory = new BXCategory(GetMessage("Category.Voting"), "Voting", 220);

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			ParamsDefinition.Add("ThemeCssFilePath", new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/style.css", mainCategory));
			ParamsDefinition.Add("ColorCssFilePath", new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", mainCategory));

			ParamsDefinition.Add("BlogSlug", new BXParamText(GetMessageRaw("Param.BlogSlug"), "", mainCategory));
			ParamsDefinition.Add("PostId", new BXParamText(GetMessageRaw("Param.PostId"), "", mainCategory));
			ParamsDefinition.Add("CommentId", new BXParamText(GetMessageRaw("Param.CommentId"), "", mainCategory));
			ParamsDefinition["CategoryId"] = new BXParamSingleSelection(GetMessageRaw("Param.CategoryId"), "", mainCategory);
			ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));

			ParamsDefinition.Add("BlogUrlTemplate", new BXParamText(GetMessageRaw("Param.BlogUrlTemplate"), "Blog.aspx?blog=#BlogSlug#", urlCategory));
			ParamsDefinition.Add("UserProfileUrlTemplate", new BXParamText(GetMessageRaw("Param.UserProfileUrlTemplate"), "User.aspx?user=#UserId#", urlCategory));
			ParamsDefinition.Add("PostViewUrlTemplate", new BXParamText(GetMessageRaw("Param.PostViewUrlTemplate"), "Post.aspx?post=#PostId#", urlCategory));
			ParamsDefinition.Add("PostEditUrlTemplate", new BXParamText(GetMessageRaw("Param.PostEditUrlTemplate"), "PostEdit.aspx?post=#PostId#", urlCategory));
			ParamsDefinition.Add("CommentUrlTemplate", new BXParamText(GetMessageRaw("Param.CommentUrlTemplate"), "Post.aspx?post=#PostId#&comment=#CommentId###comment#CommentId#", urlCategory));

			ParamsDefinition["GuestEmail"] = new BXParamSingleSelection(GetMessageRaw("Param.GuestEmail"), "required", guestsCategory);
			ParamsDefinition["GuestCaptcha"] = new BXParamYesNo(GetMessageRaw("Param.GuestCaptcha"), true, guestsCategory);

            ParamsDefinition.Add("EnableVotingForComment", new BXParamYesNo(GetMessageRaw("Param.EnableVotingForComment"), false, votingCategory));
            ParamsDefinition.Add("RolesAuthorizedToVote", new BXParamMultiSelection(GetMessageRaw("Param.RolesAuthorizedToVote"), string.Empty, votingCategory));

		}
		protected override void LoadComponentDefinition()
		{
			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			ParamsDefinition["GuestEmail"].Values = new List<BXParamValue>(new BXParamValue[] {
				new BXParamValue(GetMessageRaw("GuestEmail.Hide"), ""),
				new BXParamValue(GetMessageRaw("GuestEmail.Show"), "visible"),
				new BXParamValue(GetMessageRaw("GuestEmail.Require"), "required")
			});

            IList<BXParamValue> rolesValues = ParamsDefinition["RolesAuthorizedToVote"].Values;
            rolesValues.Clear();
            foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
            {
                if (string.Equals(r.RoleName, "Guest", StringComparison.Ordinal))
                    continue;
                rolesValues.Add(new BXParamValue(r.Title, r.RoleName));
            }

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
				categoryIds.Add(value);
			}
		}

		public enum ErrorCode
		{
			None = 0,
			Fatal = 1,
			Unauthorized = 2,
			FatalComponentNotExecuted = Fatal | (0 << 2),
			FatalException = Fatal | (1 << 2),
			FatalPostNotFound = Fatal | (2 << 2),
			FatalCommentNotFound = Fatal | (3 << 2),
			FatalPageNotFound = Fatal | (4 << 2),
			UnauthorizedView = Unauthorized | (0 << 2),
			UnauthorizedDelete = Unauthorized | (1 << 2),
			UnauthorizedComment = Unauthorized | (2 << 2)
		}
		public class PostInfo
		{
			private string authorNameHtml;
			private string titleHtml;
			private string postEditHref;
			private string userProfileHref;
			private string blogHref;
			private string postViewHref;
			private BXBlogPost post;

			public BXBlogPost Post
			{
				get
				{
					return post;
				}
				internal set
				{
					post = value;
				}
			}
			public string AuthorNameHtml
			{
				get
				{
					return authorNameHtml;
				}
				internal set
				{
					authorNameHtml = value;
				}
			}
			public string TitleHtml
			{
				get
				{
					return titleHtml;
				}
				internal set
				{
					titleHtml = value;
				}
			}
			public string PostEditHref
			{
				get
				{
					return postEditHref;
				}
				internal set
				{
					postEditHref = value;
				}
			}
			public string UserProfileHref
			{
				get
				{
					return userProfileHref;
				}
				internal set
				{
					userProfileHref = value;
				}
			}
			public string BlogHref
			{
				get
				{
					return blogHref;
				}
				internal set
				{
					blogHref = value;
				}
			}
			public string PostViewHref
			{
				get
				{
					return postViewHref;
				}
				internal set
				{
					postViewHref = value;
				}
			}

			internal PostInfo()
			{

			}
		}
		public class CommentInfo
		{
			internal BXBlogCommentChain processor;
			private BXBlogComment comment;
			private string authorNameHtml;
			private string userProfileHref;
			private string blogHref;
			private string postViewHref;
			private BXBlogAuthorization auth;
			private bool branchIsDeleted;
			private CommentInfo parent;
			private string commentHref;
            private List<CommentInfo> children = null;
            private ReadOnlyCollection<CommentInfo> childrenRO = null;
            private int? notDeletedChildCount = null;

			public BXBlogComment Comment
			{
				get
				{
					return comment;
				}
				set
				{
					comment = value;
				}
			}
            public string CommentId
            {
                get
                {
                    return comment.Id.ToString();
                }
            }
			public string AuthorNameHtml
			{
				get
				{
					return authorNameHtml;
				}
				internal set
				{
					authorNameHtml = value;
				}
			}
			public string UserProfileHref
			{
				get
				{
					return userProfileHref;
				}
				internal set
				{
					userProfileHref = value;
				}
			}
			public string BlogHref
			{
				get
				{
					return blogHref;
				}
				internal set
				{
					blogHref = value;
				}
			}
			public string PostViewHref
			{
				get
				{
					return postViewHref;
				}
				internal set
				{
					postViewHref = value;
				}
			}
			public BXBlogAuthorization Auth
			{
				get
				{
					return auth;
				}
				internal set
				{
					auth = value;
				}
			}
			public bool BranchIsDeleted
			{
				get
				{
					return branchIsDeleted;
				}
				set
				{
					branchIsDeleted = value;
				}
			}
			public CommentInfo Parent
			{
				get
				{
					return parent;
				}
				internal set
				{
                    if (parent != null)
                        parent.InternalChildren.Remove(this);

					parent = value;

                    if(parent != null)
                        parent.InternalChildren.Add(this);

				}
			}

            public bool HasChildren
            {
                get 
                {
                    return ChildCount > 0; 
                }
            }

            public int ChildCount
            {
                get 
                {
                    return InternalChildren.Count;
                }
            }

            public int NotDeletedChildCount
            {
                get 
                {
                    if (notDeletedChildCount.HasValue)
                        return notDeletedChildCount.Value;

                    if (InternalChildren.Count == 0)
                        return (notDeletedChildCount = 0).Value;
                    int r = 0;
                    foreach (CommentInfo child in InternalChildren)
                    {
                        if (!child.Comment.MarkedForDelete)
                            r++;
                        r += child.NotDeletedChildCount;
                    }
                    return (notDeletedChildCount = r).Value; 
                }
            }

            internal IList<CommentInfo> InternalChildren
            {
                get 
                {
                    return children ?? (children = new List<CommentInfo>());
                }
            }


            public IList<CommentInfo> Children
            {
                get 
                {
                    return childrenRO ?? (childrenRO = new ReadOnlyCollection<CommentInfo>(InternalChildren));
                }
            }

			public string CommentHref
			{
				get
				{
					return commentHref;
				}
				internal set
				{
					commentHref = value;
				}
			}

			internal CommentInfo()
			{

			}

			public void RenderPostContent(HtmlTextWriter writer)
			{
				processor.Process(comment.Content, writer);
			}
			public string GetPostContent()
			{
				return processor.Process(comment.Content);
			}
		}
		public class CommentData
		{
			private string userName;

			public string UserName
			{
				get
				{
					return userName;
				}
				set
				{
					userName = value;
				}
			}
			private string userEmail;

			public string UserEmail
			{
				get
				{
					return userEmail;
				}
				set
				{
					userEmail = value;
				}
			}
		}
		public class CommentEventArgs : EventArgs
		{
			private BXBlogComment comment;
			public BXBlogComment Comment
			{
				get
				{
					return comment;
				}
			}
			private BXBlogComment parentComment;
			public BXBlogComment ParentComment
			{
				get
				{
					return parentComment;
				}
			}

			internal CommentEventArgs(BXBlogComment comment, BXBlogComment parentComment)
			{
				this.comment = comment;
				this.parentComment = parentComment;
			}
		}
	}

	public class BlogPostCommentsTemplate : BXComponentTemplate<BlogPostCommentsComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & BlogPostCommentsComponent.ErrorCode.Fatal) != 0)
				BXError404Manager.Set404Status(Response);
		}
		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			base.Render(writer);
		}
	}
}

