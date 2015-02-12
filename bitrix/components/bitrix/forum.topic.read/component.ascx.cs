using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;

using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using System.Threading;
using Bitrix.CommunicationUtility;

namespace Bitrix.Forum.Components
{
	public partial class ForumTopicReadComponent : BXComponent
	{
		private int userId;
		private int forumId;
		private int? topicId;
		private Int64? postId;

		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private List<PostInfo> posts;
		private BXForum forum;
		private BXForumTopic topic;
		private BXForumPostChain processor;
		private BXForumSignatureChain signatureProcessor;
		private BXForumAuthorization auth;
		private BXForumSubscription userSubscription;

		private bool canModerateTopic;
		private bool canModeratePosts;
		private bool templateIncluded;
		private Exception fatalException;
		private BXParamsBag<object> replace;
		private int maxWordLength = -1;
		private List<LinkInfo> headerLinks;
		private List<int> availableForums;
		private string topicReplyHref;
		private string topicEditHref;
		private string topicTitleHtml;
		private string topicDescriptionHtml;

		public int ForumId
		{
			get
			{
				return forumId;// ?? (forumId = Parameters.GetInt("ForumId"))).Value;
			}
		}
		public int TopicId
		{
			get
			{
				return (topicId ?? (topicId = Parameters.GetInt("TopicId"))).Value;
			}
			set
			{
				topicId = value;
			}
		}
		public Int64 PostId
		{
			get
			{
				return (postId ?? (postId = Parameters.GetLong("PostId"))).Value;
			}
			set
			{
				postId = value;
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
		public List<PostInfo> Posts
		{
			get
			{
				return posts;
			}
		}
		public BXForumAuthorization Auth
		{
			get
			{
				return auth;
			}
		}
		public BXForumSubscription UserSubscription
		{
			get
			{
				return userSubscription;
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
		public List<int> AvailableForums
		{
			get
			{
				return availableForums ?? (availableForums = Parameters.GetListInt("AvailableForums"));
			}
		}

		public string ForumUrlTemplate
		{
			get
			{
				return Parameters.GetString("TopicUrlTemplate");
			}
			set
			{
				Parameters["TopicUrlTemplate"] = value;
			}
		}

		public string TopicUrlTemplate
		{
			get
			{
				return Parameters.GetString("TopicUrlTemplate");
			}
			set
			{
				Parameters["TopicUrlTemplate"] = value;
			}
		}
		public string TopicListUrlTemplate
		{
			get
			{
				return Parameters.GetString("TopicListUrlTemplate");
			}
			set
			{
				Parameters["TopicListUrlTemplate"] = value;
			}
		}
		public string TopicReplyUrlTemplate
		{
			get
			{
				return Parameters.GetString("TopicReplyUrlTemplate");
			}
			set
			{
				Parameters["TopicReplyUrlTemplate"] = value;
			}
		}
		public string TopicMoveUrlTemplate
		{
			get
			{
				return Parameters.GetString("TopicMoveUrlTemplate");
			}
			set
			{
				Parameters["TopicMoveUrlTemplate"] = value;
			}
		}
		public string TopicEditUrlTemplate
		{
			get
			{
				return Parameters.GetString("TopicEditUrlTemplate");
			}
			set
			{
				Parameters["TopicEditUrlTemplate"] = value;
			}
		}

		public string PostReadUrlTemplate
		{
			get
			{
				return Parameters.GetString("PostReadUrlTemplate");
			}
			set
			{
				Parameters["PostReadUrlTemplate"] = value;
			}
		}
		public string PostQuoteUrlTemplate
		{
			get
			{
				return Parameters.GetString("PostQuoteUrlTemplate");
			}
			set
			{
				Parameters["PostQuoteUrlTemplate"] = value;
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

		public string TopicReplyHref
		{
			get
			{
				return topicReplyHref;
			}
		}
		public string TopicEditHref
		{
			get
			{
				return topicEditHref;
			}
		}
		public string TopicTitleHtml
		{
			get
			{
				return topicTitleHtml;
			}
		}
		public string TopicDescriptionHtml
		{
			get
			{
				return topicDescriptionHtml;
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

        public string UserPostsReadUrlTemplate
        {
            get
            {
                return Parameters.Get<String>("UserPostsReadUrlTemplate", String.Empty);
            }
        }

		public bool CanModerateTopic
		{
			get
			{
				return canModerateTopic;
			}
		}
		public bool CanModeratePosts
		{
			get
			{
				return canModeratePosts;
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

        public bool EnableVotingForTopic
        {
            get { return Forum != null ? Forum.AllowVotingForTopic : false; }
            set { throw new NotSupportedException(); }
        }
        public bool EnableVotingForPost
        {
            get { return Forum != null ? Forum.AllowVotingForPost : false; }
            set { throw new NotSupportedException(); }
        }
        //Заглушка (для совместимости)
        public IList<string> RolesAuthorizedToVote
        {
            get { return new string[] { "User" }; }
            set{ throw new NotSupportedException(); }
        }

		public void DoTopicOperation(TopicOperation operation)
		{
			if (fatalError != ErrorCode.None)
				return;

			switch (operation)
			{
				case TopicOperation.Delete:
					if (!auth.CanDeleteThisTopic)
					{
						Fatal(ErrorCode.UnauthorizedDelete);
						return;
					}
					break;
				case TopicOperation.Approve:
				case TopicOperation.Hide:
					if (!auth.CanApprove)
					{
						Fatal(ErrorCode.UnauthorizedApprove);
						return;
					}
					break;
				case TopicOperation.Open:
				case TopicOperation.Close:
					if (!auth.CanOpenCloseThisTopic)
					{
						Fatal(ErrorCode.UnauthorizedOpenClose);
						return;
					}
					break;
				case TopicOperation.Stick:
				case TopicOperation.Unstick:
					if (!auth.CanTopicStick)
					{
						Fatal(ErrorCode.UnauthorizedStick);
						return;
					}
					break;
				case TopicOperation.Move:
					if (!auth.CanMoveThisTopic)
					{
						Fatal(ErrorCode.UnauthorizedMove);
						return;
					}
					break;
				case TopicOperation.Subscribe:
				case TopicOperation.Unsubscribe:
					if (!auth.CanSubscribe || topic.MovedTo > 0)
					{
						Fatal(ErrorCode.UnauthorizedSubscribe);
						return;
					}
					break;
			}

			try
			{
				if (operation == TopicOperation.Move)
				{
					string template = TopicMoveUrlTemplate;
					if (BXStringUtility.IsNullOrTrimEmpty(template))
						RedirectTemplateUrl(TopicUrlTemplate, replace);
					RedirectTemplateUrl(template, replace);
					return;
				}

				switch (operation)
				{
					case TopicOperation.Delete:
						topic.Delete();
						break;
					case TopicOperation.Approve:
					case TopicOperation.Hide:
						topic.Approved = (operation == TopicOperation.Approve);
						topic.Save();
						break;
					case TopicOperation.Open:
					case TopicOperation.Close:
						if (topic.MovedTo == 0)
						{
							topic.Closed = (operation == TopicOperation.Close);
							topic.Save();
						}
						break;
					case TopicOperation.Stick:
					case TopicOperation.Unstick:
						topic.StickyIndex = (operation == TopicOperation.Stick) ? Math.Max(1, topic.StickyIndex + 1) : 0;
						topic.Save();
						break;
					case TopicOperation.Subscribe:
						if (userSubscription == null)
						{
							userSubscription = new BXForumSubscription();
							userSubscription.ForumId = topic.ForumId;
							userSubscription.TopicId = topic.Id;
							userSubscription.SubscriberId = auth.UserId;
							userSubscription.OnlyTopic = false;
							userSubscription.SiteId = DesignerSite;
							userSubscription.Save();
						}
						break;
					case TopicOperation.Unsubscribe:
						if (userSubscription != null)
							userSubscription.Delete();
						break;
				}
			}
			catch (ThreadAbortException)
			{
				return;
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}
			RedirectTemplateUrl(operation != TopicOperation.Delete ? TopicUrlTemplate : TopicListUrlTemplate, replace);
		}
		public void DoPostOperation(PostOperation operation, IEnumerable<int> ids)
		{
			if (fatalError != ErrorCode.None)
				return;

			try
			{
				switch (operation)
				{
					case PostOperation.Approve:
					case PostOperation.Hide:
						if (!auth.CanApprove)
						{
							Fatal(ErrorCode.UnauthorizedApprovePosts);
							return;
						}
						break;
				}


				BXFilter filter = new BXFilter();
				filter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.In, ids));
				filter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.NotEqual, topic.FirstPostId));
				filter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId));
				if (!auth.CanApprove)
					filter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));
				BXForumPostCollection posts = BXForumPost.GetList(
					filter,
					null
				);

				//Validate operations
				switch (operation)
				{
					case PostOperation.Delete:
						foreach (BXForumPost post in posts)
							if (!new BXForumAuthorization(ForumId, userId, post).CanDeleteThisPost)
							{
								Fatal(ErrorCode.UnauthorizedDeletePosts);
								return;
							}
						break;
				}

				//Do operations
				foreach (BXForumPost post in posts)
				{
					switch (operation)
					{
						case PostOperation.Delete:
							post.Delete();
							break;
						case PostOperation.Approve:
						case PostOperation.Hide:
							post.Approved = (operation == PostOperation.Approve);
							post.Save();
							break;
					}
				}
			}
			catch (ThreadAbortException)
			{
				return;
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}
			RedirectTemplateUrl(TopicUrlTemplate, replace);
		}

		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalForumNotFound:
					return GetMessage("Error.ForumNotFound");
				case ErrorCode.FatalInvalidPage:
					return GetMessage("Error.InvalidPage");
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.FatalTopicNotFound:
					return GetMessage("Error.TopicNotFound");
				case ErrorCode.FatalPostNotFound:
					return GetMessage("Error.PostNotFound");
				case ErrorCode.UnauthorizedRead:
					return GetMessage("Error.NoRightsToRead");
				case ErrorCode.UnauthorizedDelete:
					return GetMessage("Error.NoRightsToDelete");
				case ErrorCode.UnauthorizedApprove:
					return GetMessage("Error.NoRightsToApprove");
				case ErrorCode.UnauthorizedOpenClose:
					return GetMessage("Error.NoRightsToOpenClose");
				case ErrorCode.UnauthorizedStick:
					return GetMessage("Error.NoRightsToStick");
				case ErrorCode.UnauthorizedMove:
					return GetMessage("Error.NoRightsToMove");
				case ErrorCode.UnauthorizedDeletePosts:
					return GetMessage("Error.NoRightsToDeletePosts");
				case ErrorCode.UnauthorizedApprovePosts:
					return GetMessage("Error.NoRightsToApprovePosts");
				case ErrorCode.UnauthorizedSubscribe:
					return GetMessage("Error.NoRightsToSubscribe");
				default:
					return GetMessage("Error.Unknown");
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;
			try
			{
				CacheMode = BXCacheMode.None;
				BXForumPost targetPost = null;
				replace = new BXParamsBag<object>();

				//if post is provided
				if (PostId != 0)
				{
					BXFilter targetPostFilter = new BXFilter();
					targetPostFilter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Equal, PostId));
					targetPostFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Active, BXSqlFilterOperators.Equal, true));
					targetPostFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));
					if (AvailableForums.Count > 0)
						targetPostFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Forum.Id, BXSqlFilterOperators.In, AvailableForums));
					
					BXForumPostCollection targetPosts = BXForumPost.GetList(
						targetPostFilter,
						null,
						new BXSelectAdd(BXForumPost.Fields.Topic, BXForumPost.Fields.Topic.Forum),
						null
					);
					if (targetPosts.Count != 0)
						targetPost = targetPosts[0];
					if (targetPost == null
						|| targetPost.TopicId <= 0
						|| targetPost.Topic == null
						|| (TopicId > 0 && targetPost.TopicId != topicId)
						|| (!BXForum.IsUserCanOperate(targetPost.Topic.ForumId, BXForum.Operations.ForumTopicApprove) && (!targetPost.Approved || !targetPost.Topic.Approved)))
					{
						Fatal(ErrorCode.FatalPostNotFound);
						return;
					}

					topicId = targetPost.TopicId;
					topic = targetPost.Topic;
				}

				// If post is not provided
				if (topic == null)
				{
					if (TopicId <= 0)
					{
						Fatal(ErrorCode.FatalTopicNotFound);
						return;
					}
					BXFilter topicFilter = new BXFilter();
					topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Id, BXSqlFilterOperators.Equal, TopicId));
					topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Active, BXSqlFilterOperators.Equal, true));
					topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));
					if (AvailableForums.Count > 0)
						topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.In, AvailableForums));

					BXForumTopicCollection topics = BXForumTopic.GetList(
						topicFilter,
						null,
						new BXSelectAdd(BXForumTopic.Fields.Forum),
						null
					);
					if (topics.Count != 0)
						topic = topics[0];

					if (topic == null
						|| (!topic.Approved && !BXForum.IsUserCanOperate(topic.ForumId, BXForum.Operations.ForumTopicApprove)))
					{
						Fatal(ErrorCode.FatalTopicNotFound);
						return;
					}

					if (topic.MovedTo > 0 && topic.MovedToTopic != null)
					{
						replace["ForumId"] = topic.MovedToTopic.ForumId;
						replace["TopicId"] = topic.MovedToTopic.Id;
						RedirectTemplateUrl(TopicUrlTemplate, replace);
						return;
					}
				}

                ComponentCache["UsersBannedToVoteForTopic"] = topic.AuthorId.ToString();

				forum = topic.Forum;
				forumId = forum.Id;
				userId = ((BXIdentity)BXPrincipal.Current.Identity).Id;
				auth = new BXForumAuthorization(ForumId, userId, topic);

				if (!auth.CanRead)
				{
					Fatal(ErrorCode.UnauthorizedRead);
					return;
				}
				else if (auth.CanSubscribe)
				{
					BXForumSubscriptionCollection subscriptions = BXForumSubscription.GetList(
						new BXFilter(
							new BXFilterItem(BXForumSubscription.Fields.Subscriber.Id, BXSqlFilterOperators.Equal, auth.UserId),
							new BXFilterOr(
								new BXFilter(
									//Подписка на данную тему
									new BXFilterItem(BXForumSubscription.Fields.Topic.Id, BXSqlFilterOperators.Equal, topic.Id)
								),
								new BXFilter(
									//Подписка на все сообщения форума
									new BXFilterItem(BXForumSubscription.Fields.Topic.Id, BXSqlFilterOperators.Equal, 0),
									new BXFilterItem(BXForumSubscription.Fields.Forum.Id, BXSqlFilterOperators.Equal, forum.Id),
									new BXFilterItem(BXForumSubscription.Fields.OnlyTopic, BXSqlFilterOperators.Equal, false)
								)
							)
						),
						new BXOrderBy(new BXOrderByPair(BXForumSubscription.Fields.Topic.Id, BXOrderByDirection.Desc)),
						null,
						new BXQueryParams(new BXPagingOptions(0,1))
					);

					if (subscriptions.Count > 0)
						userSubscription = subscriptions[0];
				}

				canModeratePosts = auth.CanPostDelete || auth.CanApprove;
				canModerateTopic = auth.CanOpenCloseThisTopic || auth.CanApprove || auth.CanTopicStick || auth.CanMoveThisTopic || auth.CanDeleteThisTopic;

				processor = new BXForumPostChain(forum);
				processor.MaxWordLength = MaxWordLength;
				processor.EnableImages = true;

				signatureProcessor = new BXForumSignatureChain();
				signatureProcessor.MaxWordLength = MaxWordLength;

				replace = new BXParamsBag<object>();
				replace["ForumId"] = ForumId;
				replace["TopicId"] = TopicId;
				topicReplyHref = MakeHref(TopicReplyUrlTemplate, replace);
				topicEditHref = MakeHref(TopicEditUrlTemplate, replace);
				topicTitleHtml = BXWordBreakingProcessor.Break(topic.TextEncoder.Decode(topic.Name ?? ""), MaxWordLength, true).Trim();
				topicDescriptionHtml = BXWordBreakingProcessor.Break(topic.TextEncoder.Decode(topic.Description ?? ""), MaxWordLength, true).Trim();


				BXFilter postFilter = new BXFilter();
				postFilter.Add(new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, ForumId));
				postFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId));
				if (!auth.CanApprove)
					postFilter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));

				int count = BXForumPost.Count(postFilter);

				//Determine page
				BXPagingParams pagingParams = PreparePagingParams();
				if (targetPost != null)
				{
					BXFilter countFilter = new BXFilter();
					countFilter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Less, targetPost.Id));
					countFilter.Add(new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, ForumId));
					countFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId));
					if (!auth.CanApprove)
						countFilter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));

					int index = BXForumPost.Count(countFilter);

					BXPagingHelper helper = ResolvePagingHelper(count, pagingParams);
					pagingParams.Page = helper.GetOuterIndex(helper.GetPageIndexForItem(index));
				}

				//Get paging options
				bool legal;
				BXQueryParams queryParams = PreparePaging(
					pagingParams,
					delegate
					{
						return count;
					},
					replace,
					out legal
				);
				if (!legal)
				{
					Fatal(ErrorCode.FatalInvalidPage);
					return;
				}

				// Query items
				BXForumPostCollection postCollection = BXForumPost.GetList(
					postFilter,
					new BXOrderBy(new BXOrderByPair(BXForumPost.Fields.Id, BXOrderByDirection.Asc)),
					new BXSelectAdd(BXForumPost.Fields.Author, BXForumPost.Fields.Author.User, BXForumPost.Fields.Author.User.Image),
					queryParams
				);
				int startNum = (queryParams != null && queryParams.AllowPaging) ? queryParams.PagingStartIndex : 0;
				this.posts = postCollection.ConvertAll<PostInfo>(delegate(BXForumPost input)
				{
					PostInfo info = new PostInfo();
					info.Post = input;
					info.postChain = processor;
					info.content = input.Post;
					info.signatureChain = signatureProcessor;
                    info.signature = input.Author != null ? input.Author.TextEncoder.Decode(input.Author.Signature) : String.Empty;
					info.AuthorNameHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.AuthorName), MaxWordLength, true);
					info.Auth = new BXForumAuthorization(ForumId, userId, input);
					info.Author = input.Author;
					info.Num = ++startNum;

					replace["PostId"] = input.Id;

					if (input.Author != null && input.Author.User != null)
					{
						replace["UserId"] = input.Author.User.UserId;
						info.UserProfileHref = MakeHref(UserProfileUrlTemplate, replace);
                        info.UserPostsReadHref = UserPostsReadUrlTemplate!=String.Empty ? MakeHref(UserPostsReadUrlTemplate, replace) : String.Empty;
						replace.Remove("UserId");
					}
					info.ThisPostHref = MakeHref(PostReadUrlTemplate, replace);
					info.PostEditHref = MakeHref(PostEditUrlTemplate, replace);
					info.PostQuoteHref = MakeHref(PostQuoteUrlTemplate, replace);

					return info;
				});
				replace.Remove("PostId");

				if (!templateIncluded)
				{
					templateIncluded = true;
					IncludeComponentTemplate();
				}

				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null && !IsComponentDesignMode)
				{
					if (Parameters.GetBool("SetPageTitle", true))
					{
						string title = topic.TextEncoder.Decode(topic.Name);
						if (!string.IsNullOrEmpty(title))
						{
							bitrixPage.MasterTitleHtml = BXWordBreakingProcessor.Break(title, MaxWordLength, true);
							bitrixPage.Title = Encode(title);
						}
					}
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

            if (fatalError == ErrorCode.None && topic != null)
                BXForumTopic.IncrementViews(topic.Id);
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = new BXComponentGroup("forum", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);

            BXCategory urlCategory = BXCategory.UrlSettings,
                mainCategory = BXCategory.Main,
                additionalSettingsCategory = BXCategory.AdditionalSettings;
                //votingCategory = new BXCategory(GetMessage("Category.Voting"), "Voting", 220);

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			ParamsDefinition.Add("ThemeCssFilePath", new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/style.css", mainCategory));
			ParamsDefinition.Add("ColorCssFilePath", new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css", mainCategory));
			ParamsDefinition.Add("TopicId", new BXParamText(GetMessageRaw("Param.TopicId"), "", mainCategory));
			ParamsDefinition.Add("PostId", new BXParamText(GetMessageRaw("Param.PostId"), "", mainCategory));

			ParamsDefinition.Add("TopicUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicUrlTemplate"), "viewtopic.aspx?topic=#TopicId#", urlCategory));
			ParamsDefinition.Add("TopicListUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicListUrlTemplate"), "viewforum.aspx?forum=#ForumId#", urlCategory));
			ParamsDefinition.Add("TopicReplyUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicReplyUrlTemplate"), "newpost.aspx?topic=#TopicId#", urlCategory));
			ParamsDefinition.Add("TopicMoveUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicMoveUrlTemplate"), "movetopics.aspx?topics=#TopicId#", urlCategory));
			ParamsDefinition.Add("TopicEditUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicEditUrlTemplate"), "edittopic.aspx?topic=#TopicId#", urlCategory));

			ParamsDefinition.Add("PostReadUrlTemplate", new BXParamText(GetMessageRaw("Param.PostReadUrlTemplate"), "viewtopic.aspx?post=#PostId###post#PostId#", urlCategory));
			ParamsDefinition.Add("PostQuoteUrlTemplate", new BXParamText(GetMessageRaw("Param.PostQuoteUrlTemplate"), "newpost.aspx?topic=#TopicId#&quote=#PostId#", urlCategory));
			ParamsDefinition.Add("PostEditUrlTemplate", new BXParamText(GetMessageRaw("Param.PostEditUrlTemplate"), "editpost.aspx?post=#PostId#", urlCategory));

			ParamsDefinition.Add("UserProfileUrlTemplate", new BXParamText(GetMessageRaw("Param.UserProfileUrlTemplate"), "profile.aspx?user=#UserId#", urlCategory));
            ParamsDefinition.Add("UserPostsReadUrlTemplate", new BXParamText(GetMessageRaw("Param.UserPostsUrlTemplate"), "viewposts.aspx?user=#UserId#", urlCategory));

			ParamsDefinition.Add("AvailableForums", new BXParamMultiSelection(GetMessageRaw("Param.AvailableForums"), "", additionalSettingsCategory));
			ParamsDefinition.Add("SetPageTitle", new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory));
			ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));

            //ParamsDefinition.Add("EnableVotingForTopic", new BXParamYesNo(GetMessageRaw("Param.EnableVotingForTopic"), false, votingCategory));
            //ParamsDefinition.Add("EnableVotingForPost", new BXParamYesNo(GetMessageRaw("Param.EnableVotingForPost"), false, votingCategory));
            //ParamsDefinition.Add("RolesAuthorizedToVote", new BXParamMultiSelection(GetMessageRaw("Param.RolesAuthorizedToVote"), string.Empty, votingCategory));
		}
		protected override void LoadComponentDefinition()
		{
			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			BXForumCollection forums = BXForum.GetList(
				new BXFilter(
					new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
				),
				new BXOrderBy(
					new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)
				),
				new BXSelect(BXForum.Fields.Id, BXForum.Fields.Name),
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			ParamsDefinition["AvailableForums"].Values = forums.ConvertAll<BXParamValue>(delegate(BXForum input)
			{
				return new BXParamValue(input.Name, input.Id.ToString());
			});

            /*
            //--- RolesAuthorizedToVote ---
            IList<BXParamValue> rolesValues = ParamsDefinition["RolesAuthorizedToVote"].Values;
            rolesValues.Clear();
            foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
            {
                if (string.Equals(r.RoleName, "Guest", StringComparison.Ordinal))
                    continue;
                rolesValues.Add(new BXParamValue(r.Title, r.RoleName));
            }
            */
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
		private string MakeHref(string template, BXParamsBag<object> parameters)
		{
			return Encode(ResolveTemplateUrl(template, parameters));
		}
		
		public class PostInfo
		{
			internal BXForumPostChain postChain;
			internal BXForumSignatureChain signatureChain;
			private BXForumPost post;
			private BXForumAuthorization auth;
			private string authorNameHtml;
			private string contentHtml;
			private string signatureHtml;
			private BXForumUser author;
			private int num;
			private string userProfileHref;
			private string thisPostHref;
			private string editPostHref;
			private string quotePostHref;
            private string userPostsReadHref;
			internal string content;
			internal string signature;

			public BXForumPost Post
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
			public string ContentHtml
			{
				get
				{
					return contentHtml ?? (contentHtml = postChain.Process(content));
				}
			}
			public string AuthorSignatureHtml
			{
				get
				{
					return signatureHtml ?? (signatureHtml = signatureChain.Process(signature));
				}
			}
			public BXForumAuthorization Auth
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
			public BXForumUser Author
			{
				get
				{
					return author;
				}
				internal set
				{
					author = value;
				}
			}
			public int Num
			{
				get
				{
					return num;
				}
				internal set
				{
					num = value;
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
			public string ThisPostHref
			{
				get
				{
					return thisPostHref;
				}
				internal set
				{
					thisPostHref = value;
				}
			}
			public string PostEditHref
			{
				get
				{
					return editPostHref;
				}
				internal set
				{
					editPostHref = value;
				}
			}
			public string PostQuoteHref
			{
				get
				{
					return quotePostHref;
				}
				internal set
				{
					quotePostHref = value;
				}
			}

            public string UserPostsReadHref
            {
                get
                {
                    return userPostsReadHref;
                }
                set
                {
                    userPostsReadHref = value;
                }
            }

		}
		public class LinkInfo
		{
			private string title;
			private string url;
			private string cssClass;
			private string customAttrs;

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
			public string CustomAttrs
			{
				get { return customAttrs ?? String.Empty; }
			}

			public LinkInfo(string url, string title, string cssClass)
			{
				this.url = url;
				this.title = title;
				this.cssClass = cssClass;
			}

			public LinkInfo(string url, string title, string cssClass, string customAttrs)
				: this(url, title, cssClass)
			{
				this.customAttrs = customAttrs;
			}
		}
		[Flags]
		public enum ErrorCode
		{
			None = 0,
			Error = 1,
			Fatal = Error,
			Unauthorized = Error | 2,
			FatalForumNotFound = Fatal,
			FatalInvalidPage = Fatal | 4,
			FatalException = Fatal | 8,
			FatalTopicNotFound = Fatal | 8 | 4,
			FatalPostNotFound = Fatal | 16,
			FatalComponentNotExecuted = Fatal | 16 | 4,
			UnauthorizedRead = Unauthorized,
			UnauthorizedDelete = Unauthorized | 4,
			UnauthorizedOpenClose = Unauthorized | 8,
			UnauthorizedApprove = Unauthorized | 4 | 8,
			UnauthorizedStick = Unauthorized | 16,
			UnauthorizedMove = Unauthorized | 16 | 4,
			UnauthorizedDeletePosts = Unauthorized | 16 | 8,
			UnauthorizedApprovePosts = Unauthorized | 16 | 8 | 4,
			UnauthorizedSubscribe = Unauthorized | 32 | 16 | 8 | 4,
		}
		public enum TopicOperation
		{
			Delete,
			Move,
			Stick,
			Unstick,
			Open,
			Close,
			Approve,
			Hide,
			Subscribe,
			Unsubscribe

		}
		public enum PostOperation
		{
			Delete,
			Approve,
			Hide
		}
	}

	public class ForumTopicReadTemplate : BXComponentTemplate<ForumTopicReadComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & ForumTopicReadComponent.ErrorCode.Fatal) != 0)
			{
				BXError404Manager.Set404Status(Response);
				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null && Parameters.GetBool("SetPageTitle", true))
					bitrixPage.Title = Component.GetErrorHtml(Component.FatalError);
			}
		}
		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			base.Render(writer);
		}
	}
}
