using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;

using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.CommunicationUtility;
using Bitrix.Modules;

namespace Bitrix.Forum.Components
{
	public partial class ForumPostFormComponent : BXComponent
	{
		private int? forumId;
		private int? topicId;
		private long? postId;
		private long? parentPostId;
		private BXForum forum;
		private BXForumTopic topic;
		private BXForumPost post;
		private BXForumPost parentPost;
		private bool? showGuestEmail;
		private bool? requireGuestEmail;
		private bool? requireGuestCaptcha;
		private Target componentTarget;
		private Mode componentMode;
		private bool captchaPrepared;
		private string captchaHref;
		private string captchaGuid;
		private BXCaptchaEngine captcha;
		private BXForumPostChain processor;
		private Data data;
		private List<string> errorSummary;
		private List<LinkInfo> headerLinks;
		private List<int> availableForums;
		private BXForumAuthorization auth;
		private bool canApprove;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private Exception fatalException;
		private bool templateIncluded;
		private int maxWordLength = -1;

		public int ForumId
		{
			get
			{
				return (forumId ?? (forumId = Parameters.GetInt("ForumId"))).Value;
			}
		}
		public int TopicId
		{
			get
			{
				return (topicId ?? (topicId = Parameters.GetInt("TopicId"))).Value;
			}
		}
		public long PostId
		{
			get
			{
				return (postId ?? (postId = Parameters.GetLong("PostId"))).Value;
			}
		}
		public long ParentPostId
		{
			get
			{
				return (parentPostId ?? (parentPostId = Parameters.GetLong("ParentPostId"))).Value;
			}
		}

		public BXForum Forum
		{
			get
			{
				return forum;
			}
		}
		public BXForumTopic Topic
		{
			get
			{
				return topic;
			}
		}
		public BXForumPost Post
		{
			get
			{
				return post;
			}
		}
		public BXForumPost ParentPost
		{
			get
			{
				return parentPost;
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
		public ErrorCode FatalError
		{
			get
			{
				return fatalError;
			}

			set 
			{
				fatalError = value;
			}
		}
		public Exception FatalException
		{
			get
			{
				return fatalException;
			}
		}
		public Target ComponentTarget
		{
			get
			{
				return componentTarget;
			}
		}
		public Mode ComponentMode
		{
			get
			{
				return componentMode;
			}
		}
		public Data ComponentData
		{
			get
			{
				return data;
			}
		}
		public bool IsGuest
		{
			get
			{
				return HttpContext.Current == null
					|| HttpContext.Current.User == null
					|| HttpContext.Current.User.Identity == null
					|| !HttpContext.Current.User.Identity.IsAuthenticated;
			}
		}
		public List<string> ErrorSummary
		{
			get
			{
				return errorSummary ?? (errorSummary = new List<string>());
			}
		}
		public List<LinkInfo> HeaderLinks
		{
			get
			{
				if (headerLinks != null)
					return headerLinks;

				headerLinks = new List<LinkInfo>();
				string param = Parameters.GetString("HeaderLinks");
				if (string.IsNullOrEmpty(param))
					return headerLinks;

				List<BXParamsBag<object>> links = BXSerializer.Deserialize(param) as List<BXParamsBag<object>>;
				if (links == null)
					return headerLinks;

				foreach (BXParamsBag<object> linkData in links)
				{
					string url = linkData.GetString("url");
					string title = linkData.GetString("title");
					string cssClass = linkData.GetString("class");

					if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(title))
						continue;

					headerLinks.Add(new LinkInfo(url, title, cssClass));
				}
				return headerLinks;
			}
		}
		public List<int> AvailableForums
		{
			get
			{
				return availableForums ?? (availableForums = Parameters.GetListInt("AvailableForums"));
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
		public bool CanApprove
		{
			get
			{
				return canApprove;
			}
		}
		public BXForumAuthorization Auth
		{
			get
			{
				return auth;
			}
		}

		private string CaptchaGuid
		{
			get
			{
				PrepareCaptcha();
				return captchaGuid;
			}
		}
		private string CaptchaHref
		{
			get
			{
				PrepareCaptcha();
				return captchaHref;
			}
		}
		private BXForumPostChain Processor
		{
			get
			{
				if (processor == null)
				{
					processor = new BXForumPostChain(forum);
					processor.MaxWordLength = MaxWordLength;
				}

				return processor;
			}
		}

		public bool Validate()
		{
			ErrorSummary.Clear();

			data.TopicTitle = data.TopicTitle != null ? data.TopicTitle.Trim() : null;
			data.TopicDescription = data.TopicDescription != null ? data.TopicDescription.Trim() : null;

			if (ComponentTarget == Target.Topic && BXStringUtility.IsNullOrTrimEmpty(data.TopicTitle))
				ErrorSummary.Add(GetMessage("Error.TopicTitleRequired"));

			if (IsGuest)
			{
				data.GuestName = data.GuestName != null ? data.GuestName.Trim() : null;
				data.GuestEmail = data.GuestEmail != null ? data.GuestEmail.Trim() : null;
				if (BXStringUtility.IsNullOrTrimEmpty(data.GuestName))
					ErrorSummary.Add(GetMessage("Error.NameRequired"));
				if (RequireGuestEmail)
				{
					if (BXStringUtility.IsNullOrTrimEmpty(data.GuestEmail))
						ErrorSummary.Add(GetMessage("Error.EmailRequired"));
					else if (!Regex.IsMatch(data.GuestEmail, @"^\s*[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]+\s*$"))
						ErrorSummary.Add(GetMessage("Error.EmailInvalid"));
				}

				if (RequireGuestCaptcha)
				{
					captcha = BXCaptchaEngine.Get(data.GuestCaptchaGuid);
					string error = captcha.Validate(data.GuestCapthca);
					if (error != null)
						ErrorSummary.Add(Encode(error));
					data.GuestCaptchaGuid = null;
				}
			}

			if (BXStringUtility.IsNullOrTrimEmpty(data.PostContent))
				ErrorSummary.Add(GetMessage("Error.PostContentRequired"));
			else
				data.PostContent = Processor.CorrectBBCode(data.PostContent.Trim());

			return ErrorSummary.Count == 0;
		}
		public void Save()
		{
			if (FatalError != ErrorCode.None)
				return;
			try
			{
				if (topic == null) //definitely we are creating a topic
				{
					topic = new BXForumTopic();
					topic.ForumId = forum.Id;

					if (IsGuest)
					{
						topic.AuthorId = 0;
						topic.AuthorName = data.GuestName;
					}
					else
					{
						topic.AuthorId = ((BXIdentity)BXPrincipal.Current.Identity).Id;
						topic.AuthorName = GetUserName();
					}
				}

				if (componentTarget == Target.Topic)
				{
					topic.Name = data.TopicTitle;
					topic.Description = data.TopicDescription;
					if (CanApprove)
						topic.Approved = data.IsApproved;
					topic.Save();
				}
				topicId = topic.Id;

				if (post == null)
				{
					post = new BXForumPost();
					post.ForumId = forum.Id;
					post.TopicId = topic.Id;
					if (parentPost != null)
						post.ParentPostId = parentPost.Id;

					if (IsGuest)
					{
						post.AuthorId = 0;
						post.AuthorName = data.GuestName;
						if (ShowGuestEmail && data.GuestEmail != null)
							post.AuthorEmail = data.GuestEmail;
					}
					else
					{
						post.AuthorId = ((BXIdentity)BXPrincipal.Current.Identity).Id;
						post.AuthorName = GetUserName();
					}
					post.AuthorIP = Request.UserHostAddress;
				}

				if (CanApprove)
					post.Approved = data.IsApproved;

				post.Post = data.PostContent;
				post.Save();
				postId = post.Id;

				if (componentMode == Mode.Add)
				{
					if (componentTarget == Target.Topic && data.SubscribeToNewTopic)
						SubscribeToNewTopic(topic);

					SendMail(post);
				}

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
		public void Preview(string input, HtmlTextWriter writer)
		{
			Processor.Process(Processor.CorrectBBCode(input), writer);
		}
		public string Preview(string input)
		{
			using (StringWriter s = new StringWriter())
			{
				Processor.Process(Processor.CorrectBBCode(input), s);
				return s.ToString();
			}
		}
		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.FatalForumNotFound:
					return GetMessage("Error.ForumNotFound");
				case ErrorCode.FatalTopicNotFound:
					return GetMessage("Error.TopicNotFound");
				case ErrorCode.FatalPostNotFound:
					return GetMessage("Error.PostNotFound");
				case ErrorCode.FatalTopicIsMoved:
					return GetMessage("Error.UnableToEditLink");
				case ErrorCode.UnauthorizedCreateTopic:
					return GetMessage("Error.NoRightsToCreateTopic");
				case ErrorCode.UnauthorizedEditTopic:
					return GetMessage("Error.NoRightsToEditTopic");
				case ErrorCode.UnauthorizedCreatePost:
					return GetMessage("Error.NoRightsToCreatePost");
				case ErrorCode.UnauthorizedEditPost:
					return GetMessage("Error.NoRightsToEditPost");
				default:
					return GetMessage("Error.Unknown");
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;

			try
			{
				componentMode = string.Equals(Parameters.GetString("Mode"), "edit", StringComparison.OrdinalIgnoreCase) ? Mode.Edit : Mode.Add;
				if (componentMode == Mode.Edit)
					componentTarget = PostId > 0 ? Target.Post : Target.Topic;
				else
					componentTarget = TopicId > 0 ? Target.Post : Target.Topic;

				if (!LoadEntities())
					return;

				if (!CheckPermissions())
					return;

				data = new Data(this);
				data.IsApproved = true;
				data.SubscribeToNewTopic = false;

				if (post != null)
				{
					data.PostContent = post.Post;
					if (componentTarget == Target.Post)
						data.IsApproved = post.Approved;
				}
				else if (parentPost != null)
				{
					data.PostContent = string.Format(
						forum.AllowBBCode ? "[quote]{0}:\n{1}[/quote]" : "{0}:\n===========================\n{1}\n===========================\n",
						parentPost.AuthorName,  
						parentPost.Post
					);
				}

				if (topic != null)
				{
					data.TopicTitle = topic.Name;
					data.TopicDescription = topic.Description;
					if (componentTarget == Target.Topic)
						data.IsApproved = topic.Approved;
				}

				if (!templateIncluded)
				{
					templateIncluded = true;
					IncludeComponentTemplate();
				}

				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null && !IsComponentDesignMode && Parameters.GetBool("SetPageTitle", true))
				{
					string title;
					if (componentTarget == Target.Topic)
						title = componentMode == Mode.Add ? GetMessage("PageTitle.NewTopic") : GetMessage("PageTitle.EditTopic");
					else
						title = componentMode == Mode.Add ? GetMessage("PageTitle.NewReply") : GetMessage("PageTitle.EditReply");

					if (!String.IsNullOrEmpty(title))
						bitrixPage.Title = title;
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
			Group = new BXComponentGroup("forum", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);

			BXCategory mainCategory = BXCategory.Main;
			BXCategory guestsCategory = new BXCategory(GetMessageRaw("Category.GuestParameters"), "guest_parameters", mainCategory.Sort + 100);
			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

			ParamsDefinition["ThemeCssFilePath"] = new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/style.css", mainCategory);
			ParamsDefinition["ColorCssFilePath"] = new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css", mainCategory);

			ParamsDefinition["ForumId"] = new BXParamSingleSelection(GetMessageRaw("Param.ForumId"), "", mainCategory);
			ParamsDefinition["TopicId"] = new BXParamText(GetMessageRaw("Param.TopicId"), "", mainCategory);
			ParamsDefinition["PostId"] = new BXParamText(GetMessageRaw("Param.PostId"), "", mainCategory);
			ParamsDefinition["ParentPostId"] = new BXParamText(GetMessageRaw("Param.ParentPostId"), "", mainCategory);
			ParamsDefinition["Mode"] = new BXParamSingleSelection(GetMessageRaw("Param.Mode"), "add", mainCategory);

			ParamsDefinition["GuestEmail"] = new BXParamSingleSelection(GetMessageRaw("Param.GuestEmail"), "required", guestsCategory);
			ParamsDefinition["GuestCaptcha"] = new BXParamYesNo(GetMessageRaw("Param.GuestCaptcha"), true, guestsCategory);

			ParamsDefinition["RedirectUrlTemplate"] = new BXParamText(GetMessageRaw("Param.RedirectUrlTemplate"), "viewforum.aspx?forum=#ForumId#", urlCategory);
			ParamsDefinition["PostReadUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostReadTemplate"), "viewtopic.aspx?post=#PostId###post#PostId#", urlCategory);

			ParamsDefinition["AvailableForums"] = new BXParamMultiSelection(GetMessageRaw("Param.AvailableForums"), "", additionalSettingsCategory);
			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);
			ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));
		}
		protected override void LoadComponentDefinition()
		{
			BXForumCollection forums = BXForum.GetList(
				new BXFilter(new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)),
				new BXOrderBy(new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)),
				new BXSelect(BXForum.Fields.Id, BXForum.Fields.Name),
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			
			List<BXParamValue> forumIds = forums.ConvertAll<BXParamValue>(delegate(BXForum input)
			{
				return new BXParamValue(input.Name, input.Id.ToString());
			});
			forumIds.Insert(0, new BXParamValue(GetMessageRaw("Option.SelectForum"), string.Empty));
			ParamsDefinition["ForumId"].Values = forumIds;

			ParamsDefinition["AvailableForums"].Values = forums.ConvertAll<BXParamValue>(delegate(BXForum input)
			{
				return new BXParamValue(input.Name, input.Id.ToString());
			});

			ParamsDefinition["Mode"].Values = new List<BXParamValue>(new BXParamValue[]	{
				new BXParamValue(GetMessageRaw("Mode.Create"), "add"),
				new BXParamValue(GetMessageRaw("Mode.Edit"), "edit")
			});

			ParamsDefinition["GuestEmail"].Values = new List<BXParamValue>(new BXParamValue[]	{
				new BXParamValue(GetMessageRaw("GuestEmail.Hide"), ""),
				new BXParamValue(GetMessageRaw("GuestEmail.Show"), "visible"),
				new BXParamValue(GetMessageRaw("GuestEmail.Require"), "required")
			});

		}

		private void SendMail(BXForumPost post)
		{
			if (post == null || !post.Approved)
				return;

			BXForumNewPostEmail email = new BXForumNewPostEmail(post);
			email.PostUrl = MakeAbsolute(ResolveTemplateUrl(Parameters.GetString("PostReadUrlTemplate"), GetReplaceParameters()));
			//email.MaxEmailsPerStep = 50;
			email.Send();
		}
		private void SubscribeToNewTopic(BXForumTopic topic)
		{
			if (topic == null || !auth.CanSubscribe)
				return;

			try
			{
				BXForumSubscription subscribtion = new BXForumSubscription();
				subscribtion.ForumId = topic.ForumId;
				subscribtion.TopicId = topic.Id;
				subscribtion.SiteId = DesignerSite;
				subscribtion.SubscriberId = auth.UserId;
				subscribtion.Save();
			}
			catch { }
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
		private bool LoadEntities()
		{
			if (componentMode == Mode.Add && PostId == 0 && ParentPostId != 0)
			{
				componentTarget = Target.Post;

				if (ForumId != 0 && AvailableForums.Count > 0 && !AvailableForums.Contains(ForumId))
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return false;
				}

				BXFilter parentPostsFilter = new BXFilter();
				parentPostsFilter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Equal, ParentPostId));
				if (TopicId != 0)
					parentPostsFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId));
				if (ForumId != 0)
					parentPostsFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Id, BXSqlFilterOperators.Equal, ForumId));
				else if (AvailableForums.Count > 0)
					parentPostsFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Id, BXSqlFilterOperators.In, AvailableForums));

				BXForumPostCollection parentPosts = BXForumPost.GetList(
					parentPostsFilter,
					null,
					new BXSelectAdd(BXForumPost.Fields.Topic, BXForumPost.Fields.Topic.Forum),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (parentPosts.Count > 0)
					parentPost = parentPosts[0];

				if (parentPost == null)
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return false;
				}

				topic = parentPost.Topic;
				topicId = topic.Id;

				forum = topic.Forum;
				forumId = forum.Id;
			}

			if (componentTarget == Target.Topic && componentMode == Mode.Add)
			{
				if (ForumId != 0 && (AvailableForums.Count == 0 || AvailableForums.Contains(ForumId)))
				{
					BXForumCollection forums = BXForum.GetList(
						new BXFilter(
							new BXFilterItem(BXForum.Fields.Id, BXSqlFilterOperators.Equal, ForumId),
							new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true),
							new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
						),
						null,
						null,
						null,
						BXTextEncoder.EmptyTextEncoder
					);
					if (forums.Count > 0)
						forum = forums[0];
				}

				if (forum == null)
				{
					Fatal(ErrorCode.FatalForumNotFound);
					return false;
				}

				return true;
			}

			if (componentTarget == Target.Topic && componentMode == Mode.Edit
				|| componentTarget == Target.Post && componentMode == Mode.Add)
			{
				if (TopicId == 0 || ForumId != 0 && AvailableForums.Count > 0 && !AvailableForums.Contains(ForumId))
				{
					Fatal(ErrorCode.FatalTopicNotFound);
					return false;
				}

				BXSelectAdd select = new BXSelectAdd();
				select.Add(BXForumTopic.Fields.Forum);
				if (componentTarget == Target.Topic)
					select = new BXSelectAdd(BXForumTopic.Fields.FirstPost);

				BXFilter topicsFilter = new BXFilter();
				topicsFilter.Add(new BXFilterItem(BXForumTopic.Fields.Id, BXSqlFilterOperators.Equal, TopicId));
				if (ForumId != 0)
					topicsFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.Equal, ForumId));
				else if (AvailableForums.Count > 0)
					topicsFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.In, AvailableForums));
				topicsFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Active, BXSqlFilterOperators.Equal, true));
				topicsFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

				BXForumTopicCollection topics = BXForumTopic.GetList(
					topicsFilter,
					null,
					select,
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (topics.Count > 0)
					topic = topics[0];


				if (topic == null)
				{
					Fatal(ErrorCode.FatalTopicNotFound);
					return false;
				}

				if (topic.MovedTo != 0)
				{
					Fatal(ErrorCode.FatalTopicIsMoved);
					return false;
				}

				forum = topic.Forum;
				forumId = topic.Forum.Id;

				if (componentTarget == Target.Topic && topic.FirstPostId > 0)
				{
					post = topic.FirstPost;
					postId = post.Id;
				}


				if (componentTarget == Target.Post && ParentPostId > 0)
				{
					BXForumPostCollection parents = BXForumPost.GetList(
						new BXFilter(
							new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Equal, ParentPostId),
							new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId)
						),
						null,
						null,
						null,
						BXTextEncoder.EmptyTextEncoder
					);
					if (parents.Count > 0)
						parentPost = parents[0];
					else
						parentPostId = 0;
				}

				return true;
			}

			if (componentTarget == Target.Post && componentMode == Mode.Edit)
			{
				if (PostId == 0 || ForumId != 0 && AvailableForums.Count > 0 && !AvailableForums.Contains(ForumId))
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return false;
				}

				BXFilter postsFilter = new BXFilter();
				postsFilter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Equal, PostId));
				if (TopicId != 0)
					postsFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId));
				if (ForumId != 0)
					postsFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Id, BXSqlFilterOperators.Equal, ForumId));
				else if (AvailableForums.Count > 0)
					postsFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Id, BXSqlFilterOperators.In, AvailableForums));

				BXForumPostCollection posts = BXForumPost.GetList(
					postsFilter,
					null,
					new BXSelectAdd(BXForumPost.Fields.Topic, BXForumPost.Fields.Topic.Forum),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (posts.Count > 0)
					post = posts[0];


				if (post == null)
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return false;
				}


				topic = post.Topic;
				topicId = topic.Id;

				forum = topic.Forum;
				forumId = forum.Id;

				if (topic.FirstPostId == post.Id)
					componentTarget = Target.Topic;
				return true;
			}
			return true;
		}
		private bool CheckPermissions()
		{
			int userId = ((BXIdentity)BXPrincipal.Current.Identity).Id;

			{
				auth = new BXForumAuthorization(ForumId, userId);
				canApprove = auth.CanApprove;

				if (parentPost != null && !parentPost.Approved && !canApprove)
				{
					parentPost = null;
					parentPostId = 0;
				}

				if (!auth.CanRead)
				{
					if (componentTarget == Target.Topic && componentMode == Mode.Add)
						Fatal(ErrorCode.FatalForumNotFound);
					else if (componentTarget == Target.Post && componentMode == Mode.Edit)
						Fatal(ErrorCode.FatalPostNotFound);
					else
						Fatal(ErrorCode.FatalTopicNotFound);
					return false;
				}

				if (componentTarget == Target.Topic && componentMode == Mode.Add && !auth.CanTopicCreate)
				{
					Fatal(ErrorCode.UnauthorizedCreateTopic);
					return false;
				}
			}

			if (topic != null)
			{
				BXForumAuthorization auth = new BXForumAuthorization(ForumId, userId, topic);
				if (!topic.Approved && !auth.CanApprove)
				{
					Fatal((componentTarget == Target.Post && componentMode == Mode.Edit) ? ErrorCode.FatalPostNotFound : ErrorCode.FatalTopicNotFound);
					return false;
				}

				if (componentTarget == Target.Topic)
				{
					if (!auth.CanEditThisTopic || topic.Closed && !auth.CanOpenCloseThisTopic)
					{
						Fatal(ErrorCode.UnauthorizedEditTopic);
						return false;
					}
				}
				else if (componentMode == Mode.Add)
				{
					if (!auth.CanTopicReply || topic.Closed && !auth.CanOpenCloseThisTopic)
					{
						Fatal(ErrorCode.UnauthorizedCreatePost);
						return false;
					}
				}
				else if (topic.Closed && !auth.CanOpenCloseThisTopic)
				{
					Fatal(ErrorCode.UnauthorizedEditPost);
					return false;
				}
			}

			if (post != null)
			{
				BXForumAuthorization auth = new BXForumAuthorization(ForumId, userId, post);
				if (!post.Approved && !auth.CanApprove)
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return false;
				}
				if (componentTarget == Target.Post && !auth.CanEditThisPost)
				{
					Fatal(ErrorCode.UnauthorizedEditPost);
					return false;
				}
			}

			return true;
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
		private void PrepareCaptcha()
		{
			if (captchaPrepared)
				return;

			captcha = captcha ?? BXCaptchaEngine.Create();
			captcha.MaxTimeout = 1800;

			captchaHref = Encode(captcha.Store());
			captchaGuid = captcha.Id;

			captchaPrepared = true;
		}
		private BXParamsBag<object> GetReplaceParameters()
		{
			BXParamsBag<object> parameters = new BXParamsBag<object>();
			parameters["ForumId"] = ForumId;
			parameters["TopicId"] = TopicId;
			parameters["PostId"] = PostId;
			return parameters;
		}
		private string GetUserName()
		{
			BXUser user = ((BXIdentity)BXPrincipal.Current.Identity).User;
			string name = user.TextEncoder.Decode(user.DisplayName);
			if (BXStringUtility.IsNullOrTrimEmpty(name))
				name = user.TextEncoder.Decode(user.FirstName) + " " + user.TextEncoder.Decode(user.LastName);
			if (BXStringUtility.IsNullOrTrimEmpty(name))
				name = user.TextEncoder.Decode(user.UserName);

			return name.Trim();
		}

		//NESTED CLASSES
		public enum Target
		{
			Topic,
			Post
		}
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
			FatalForumNotFound = Fatal | 0,
			FatalTopicNotFound = Fatal | 4,
			FatalTopicIsMoved = Fatal | 8,
			FatalPostNotFound = Fatal | 8 | 4,
			FatalComponentNotExecuted = Fatal | 16,
			FatalException = Fatal | 16 | 4,
			UnauthorizedCreateTopic = Unauthorized | 0,
			UnauthorizedEditTopic = Unauthorized | 4,
			UnauthorizedCreatePost = Unauthorized | 8,
			UnauthorizedEditPost = Unauthorized | 4 | 8
		}
		public class Data
		{
			private ForumPostFormComponent component;
			private string postContent;
			private string guestName;
			private string guestEmail;
			private string guestCaptcha;
			private string guestCaptchaGuid;
			private string topicTitle;
			private string topicDescription;
			private bool isApproved;
			private bool subscribeToNewTopic;

			public string PostContent
			{
				get
				{
					return postContent;
				}
				set
				{
					postContent = value;
				}
			}
			public string GuestName
			{
				get
				{
					return guestName;
				}
				set
				{
					guestName = value;
				}
			}
			public string GuestEmail
			{
				get
				{
					return guestEmail;
				}
				set
				{
					guestEmail = value;
				}
			}
			public string GuestCapthca
			{
				internal get
				{
					return guestCaptcha;
				}
				set
				{
					guestCaptcha = value;
				}
			}
			public string GuestCaptchaGuid
			{
				get
				{
					if (guestCaptchaGuid == null)
						return component.CaptchaGuid;
					return guestCaptchaGuid;
				}
				set
				{
					guestCaptchaGuid = value;
				}
			}
			public string GuestCaptchaHref
			{
				get
				{
					return component.CaptchaHref;
				}
			}
			public string TopicTitle
			{
				get
				{
					return topicTitle;
				}
				set
				{
					topicTitle = value;
				}
			}
			public string TopicDescription
			{
				get
				{
					return topicDescription;
				}
				set
				{
					topicDescription = value;
				}
			}
			public bool IsApproved
			{
				get
				{
					return isApproved;
				}
				set
				{
					isApproved = value;
				}
			}
			public bool SubscribeToNewTopic
			{
				get
				{
					return subscribeToNewTopic;
				}
				set
				{
					subscribeToNewTopic = value;
				}
			}

			internal Data(ForumPostFormComponent component)
			{
				this.component = component;
			}
		}
		public class LinkInfo
		{
			private string title;
			private string url;
			private string cssClass;

			public string Title
			{
				get
				{
					return HttpUtility.HtmlEncode(title ?? string.Empty);
				}
			}
			public string Url
			{
				get
				{
					return url;
				}
			}
			public string Href
			{
				get
				{
					return HttpUtility.HtmlAttributeEncode(url ?? string.Empty);
				}
			}
			public string CssClass
			{
				get
				{
					return cssClass != null ? HttpUtility.HtmlAttributeEncode(cssClass) : null;
				}
			}

			internal LinkInfo(string url, string title, string cssClass)
			{
				this.url = url;
				this.title = title;
				this.cssClass = cssClass;
			}
		}
	}

	public class ForumPostFormTemplate : BXComponentTemplate<ForumPostFormComponent>
	{
		private bool previewPost;
		private string defaultButtonTitle;
		private string defaultHeaderTitle;

		public bool PreviewPost
		{
			get
			{
				return previewPost;
			}
		}
		public string DefaultButtonTitle
		{
			get
			{
				if (defaultButtonTitle == null)
				{
					defaultButtonTitle =
						Component.ComponentTarget == ForumPostFormComponent.Target.Post
						? (Component.ComponentMode == ForumPostFormComponent.Mode.Add
							? Component.GetMessageRaw("ButtonTitle.CreatePost")
							: Component.GetMessageRaw("ButtonTitle.EditPost")
						)
						: (Component.ComponentMode == ForumPostFormComponent.Mode.Add
							? Component.GetMessageRaw("ButtonTitle.CreateTopic")
							: Component.GetMessageRaw("ButtonTitle.EditTopic")
						);
				}
				return defaultButtonTitle;
			}
		}
		public virtual string PostTextareaClientID
		{
			get { return null; }
		}
		public string DefaultHeaderTitle
		{
			get
			{
				if (defaultHeaderTitle == null)
				{
					defaultHeaderTitle =
						Component.ComponentTarget == ForumPostFormComponent.Target.Post
						? (Component.ComponentMode == ForumPostFormComponent.Mode.Add
							? String.Format(
									Component.GetMessage("HeaderTitle.CreatePost"),
									BXWordBreakingProcessor.Break(Component.Topic.Name, Component.MaxWordLength, true)
								)
							: Component.GetMessage("HeaderTitle.EditPost")
						)
						: (Component.ComponentMode == ForumPostFormComponent.Mode.Add
							? String.Format(
									Component.GetMessage("HeaderTitle.CreateTopic"),
									Encode(Component.Forum.Name)
								)
							: Component.GetMessage("HeaderTitle.EditTopic")
						);
				}
				return defaultHeaderTitle;
			}
		}


		protected virtual void LoadData(ForumPostFormComponent.Data data)
		{

		}
		protected virtual void SaveData(ForumPostFormComponent.Data data)
		{

		}
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (!IsPostBack && Component.FatalError == ForumPostFormComponent.ErrorCode.None)
				LoadData(Component.ComponentData);
		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & ForumPostFormComponent.ErrorCode.Fatal) != 0)
			{
				BXError404Manager.Set404Status(Response);
				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null && Parameters.GetBool("SetPageTitle", true))
					bitrixPage.Title = Component.GetErrorHtml(Component.FatalError);
			}
		}

		protected void PreviewClick(object sender, EventArgs e)
		{
			previewPost = true;
		}
		protected void SaveClick(object sender, EventArgs e)
		{
			SaveData(Component.ComponentData);

			if (Component.Validate())
				Component.Save();
		}
	}
}
