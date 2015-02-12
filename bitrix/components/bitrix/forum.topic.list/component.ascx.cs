using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.UI;

using Bitrix.CommunicationUtility;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.Components.Editor;
using Bitrix.CommunicationUtility.Rating;

namespace Bitrix.Forum.Components
{
	public partial class ForumTopicListComponent : BXComponent
	{
		private int userId;
		private BXForumAuthorization auth;
		private int? forumId;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private List<TopicInfo> topics;
		private string newTopicHref;
		private BXForum forum;
		private bool canModerate;
		private bool templateIncluded;
		private Exception fatalException;
		private BXParamsBag<object> replace;
		private int maxWordLength = -1;
		private List<LinkInfo> headerLinks;
		private List<int> availableForums;
		private BXForumSubscription userSubscription;

		public int ForumId
		{
			get
			{
				return (forumId ?? (forumId = Parameters.GetInt("ForumId"))).Value;
			}
			set
			{
				forumId = value;
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
		public List<TopicInfo> Topics
		{
			get
			{
				return topics;
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
		public List<int> AvailableForums
		{
			get
			{
				return availableForums ?? (availableForums = Parameters.GetListInt("AvailableForums"));
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
		public string NewTopicUrlTemplate
		{
			get
			{
				return Parameters.GetString("NewTopicUrlTemplate");
			}
			set
			{
				Parameters["NewTopicUrlTemplate"] = value;
			}
		}
		public string NewTopicHref
		{
			get
			{
				return newTopicHref;
			}
		}
		public bool CanModerate
		{
			get
			{
				return canModerate;
			}
		}
        //public bool SortByVotingTotals
        //{
        //    get
        //    {
        //        return Parameters.GetBool("SortByVotingTotals", false);
        //    }
        //    set
        //    {
        //        Parameters["SortByVotingTotals"] = value.ToString();
        //    }
        //}

        //public bool EnableVotingForTopic
        //{
        //    get
        //    {
        //        return Parameters.GetBool("EnableVotingForTopic", false);
        //    }
        //    set
        //    {
        //        Parameters["EnableVotingForTopic"] = value.ToString();
        //    }
        //}

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

		public void DoOperation(TopicOperation operation, IEnumerable<int> topicIds)
		{
			if (fatalError != ErrorCode.None)
				return;

			switch (operation)
			{
				case TopicOperation.Delete:
					if (!auth.CanTopicDelete)
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
					if (!auth.CanTopicOpenClose)
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
					if (!auth.CanTopicMove)
					{
						Fatal(ErrorCode.UnauthorizedMove);
						return;
					}
					break;
			}

			try
			{
				if (operation == TopicOperation.Move)
				{
					List<string> ids = new List<string>();
					foreach (int id in topicIds)
						ids.Add(id.ToString());
					replace["MoveTopicsIds"] = BXStringUtility.ListToCsv(ids);
					string template = Parameters.GetString("MoveTopicsUrlTemplate");
					if (BXStringUtility.IsNullOrTrimEmpty(template))
						Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());

					RedirectTemplateUrl(template, replace);
					return;
				}

				BXFilter filter = new BXFilter();
				filter.Add(new BXFilterItem(BXForumTopic.Fields.Id, BXSqlFilterOperators.In, topicIds));
				filter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.Equal, ForumId));
				if (!auth.CanApprove)
					filter.Add(new BXFilterItem(BXForumTopic.Fields.Approved, BXSqlFilterOperators.Equal, true));

				BXForumTopicCollection topics = BXForumTopic.GetList(
					filter,
					null
				);
				foreach (BXForumTopic topic in topics)
				{
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
		}
		public void DoForumOperation(ForumOperation operation)
		{
			if (fatalError != ErrorCode.None)
				return;

			if (!auth.CanSubscribe)
			{
				Fatal(ErrorCode.UnauthorizedSubscribe);
				return;
			}

			try
			{
				if ((operation == ForumOperation.PostsSubscribe || operation == ForumOperation.TopicSubscribe) && userSubscription == null)
				{
					userSubscription = new BXForumSubscription();
					userSubscription.ForumId = forum.Id;
					userSubscription.TopicId = 0;
					userSubscription.SubscriberId = auth.UserId;
					userSubscription.OnlyTopic = operation == ForumOperation.TopicSubscribe;
					userSubscription.SiteId = DesignerSite;
					userSubscription.Save();
				}
				else if (operation == ForumOperation.Unsubscribe && userSubscription != null)
					userSubscription.Delete();
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}
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

				if (ForumId <= 0)
				{
					Fatal(ErrorCode.FatalForumNotFound);
					return;
				}

				userId = ((BXIdentity)BXPrincipal.Current.Identity).Id;
				auth = new BXForumAuthorization(ForumId, userId);
				if (!auth.CanRead)
				{
					Fatal(ErrorCode.UnauthorizedRead);
					return;
				}

				if (AvailableForums.Count > 0 && !AvailableForums.Contains(ForumId))
				{
					Fatal(ErrorCode.FatalForumNotFound);
					return;
				}

				BXForumCollection sourceForums = BXForum.GetList(
					new BXFilter(
						new BXFilterItem(BXForum.Fields.Id, BXSqlFilterOperators.Equal, ForumId),
						new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true),
						new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
					),
					null
				);
				if (sourceForums.Count > 0)
					forum = sourceForums[0];
				if (forum == null)
				{
					Fatal(ErrorCode.FatalForumNotFound);
					return;
				}

				if (auth.CanSubscribe)
				{
					BXForumSubscriptionCollection subscriptions = BXForumSubscription.GetList(
						new BXFilter(
							new BXFilterItem(BXForumSubscription.Fields.Subscriber.Id, BXSqlFilterOperators.Equal, auth.UserId),
							new BXFilterItem(BXForumSubscription.Fields.Topic.Id, BXSqlFilterOperators.Equal, 0),
							new BXFilterItem(BXForumSubscription.Fields.Forum.Id, BXSqlFilterOperators.Equal, forum.Id)
						),
						null,
						null,
						new BXQueryParams(new BXPagingOptions(0, 1))
					);

					if (subscriptions.Count > 0)
						userSubscription = subscriptions[0];
				}


				canModerate =
					auth.CanApprove
					|| auth.CanTopicDelete
					|| auth.CanTopicMove
					|| auth.CanTopicOpenClose
					|| auth.CanTopicStick;

				BXFilter topicFilter = new BXFilter();
				topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.Equal, ForumId));
				if (!auth.CanApprove)
					topicFilter.Add(new BXFilterItem(BXForumTopic.Fields.Approved, BXSqlFilterOperators.Equal, true));

				replace = new BXParamsBag<object>();
				replace["ForumId"] = ForumId;
				newTopicHref = Encode(ResolveTemplateUrl(NewTopicUrlTemplate, replace));

				bool legal;
				BXPagingParams pagingParams = PreparePagingParams();
				BXQueryParams queryParams = PreparePaging(
					pagingParams,
					delegate()
					{
						return BXForumTopic.Count(topicFilter);
					},
					replace,
					out legal
				);
				if (!legal)
				{
					Fatal(ErrorCode.FatalInvalidPage);
					return;
				}


                //В любом случае первая сортировка по StickyIndex
                BXOrderBy topicOrderBy = new BXOrderBy(
                    new BXOrderByPair(BXForumTopic.Fields.StickyIndex, BXOrderByDirection.Desc)
                );

                //if (EnableVotingForTopic && SortByVotingTotals)
                //    BXRatingDataLayerHelper.AddSortingByVotingTotalValue(BXForumTopic.Fields.CustomFields.DefaultFields, topicOrderBy, BXOrderByDirection.Desc);

                topicOrderBy.Add(BXForumTopic.Fields.LastPostDate, BXOrderByDirection.Desc);
                topicOrderBy.Add(BXForumTopic.Fields.Id, BXOrderByDirection.Desc);

                BXSelect topicSelect = null;
                //if (SortByVotingTotals)
                //{
                //    topicSelect = new BXSelectAdd();
                //    BXRatingDataLayerHelper.AddTotalValueToSelection(topicSelect, BXForumTopic.Fields.CustomFields.DefaultFields);
                //    BXRatingDataLayerHelper.AddTotalVotesToSelection(topicSelect, BXForumTopic.Fields.CustomFields.DefaultFields);
                //}

				BXForumTopicCollection topics = BXForumTopic.GetList(
					topicFilter,
					topicOrderBy,
					topicSelect,
					queryParams
				);
				this.topics = topics.ConvertAll<TopicInfo>(delegate(BXForumTopic input)
				{
					if (input.MovedTo > 0 && input.MovedToTopic != null)
					{
						replace["TopicId"] = input.MovedToTopic.Id;
						replace["ForumId"] = input.MovedToTopic.ForumId;
					}
					else	
					{
						replace["TopicId"] = input.Id;
						replace["ForumId"] = input.ForumId;
					}

					TopicInfo info = new TopicInfo();
					info.Topic = input;
					info.TitleHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Name), MaxWordLength, true);
					info.DescriptionHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Description), MaxWordLength, true);
					info.AuthorNameHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.AuthorName), MaxWordLength, true);
					info.Replies = input.Replies;
					info.HiddenReplies = auth.CanApprove ? input.QueuedReplies : 0;
					replace["PostId"] = input.FirstPostId;
					info.TopicHref = Encode(ResolveTemplateUrl(TopicUrlTemplate, replace));
					replace["PostId"] = input.LastPostId;
					info.LastPostHref = Encode(ResolveTemplateUrl(PostReadUrlTemplate, replace));
					info.LastPosterNameHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.LastPosterName), MaxWordLength, true);
					info.Auth = new BXForumAuthorization(ForumId, userId, input);
					info.HasNewPosts = false;
					return info;
				});

				replace["ForumId"] = ForumId;

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
						string title = forum.TextEncoder.Decode(forum.Name);
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
			ParamsDefinition.Add("ForumId", new BXParamSingleSelection(GetMessageRaw("Param.ForumId"), "", mainCategory));

			ParamsDefinition.Add("TopicUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicUrlTemplate"), "viewtopic.aspx?topic=#TopicId#", urlCategory));
			ParamsDefinition.Add("PostReadUrlTemplate", new BXParamText(GetMessageRaw("Param.PostReadUrlTemplate"), "viewtopic.aspx?post=#PostId###post#PostId#", urlCategory));
			ParamsDefinition.Add("NewTopicUrlTemplate", new BXParamText(GetMessageRaw("Param.NewTopicUrlTemplate"), "newtopic.aspx?forum=#ForumId#", urlCategory));
			ParamsDefinition.Add("MoveTopicsUrlTemplate", new BXParamText(GetMessageRaw("Param.MoveTopicsUrlTemplate"), "movetopics.aspx?topics=#MoveTopicsIds#", urlCategory));

			ParamsDefinition.Add("AvailableForums", new BXParamMultiSelection(GetMessageRaw("Param.AvailableForums"), "", additionalSettingsCategory));
			ParamsDefinition.Add("SetPageTitle", new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory));
			ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));

            //ParamsDefinition.Add(
            //    "EnableVotingForTopic", 
            //    new BXParamYesNo(
            //        GetMessageRaw("Param.EnableVotingForTopic"), 
            //        false, 
            //        votingCategory,
            //        new ParamClientSideActionGroupViewSwitch(ClientID, "EnableVotingForTopic", "EnableVoting", string.Empty)
            //        )
            //    );

            //ParamsDefinition.Add(
            //    "SortByVotingTotals", 
            //    new BXParamYesNo(
            //        GetMessageRaw("Param.SortByVotingTotals"), 
            //        false, 
            //        votingCategory,
            //        new ParamClientSideActionGroupViewMember(ClientID, "SortByVotingTotals", new string[] { "EnableVoting" })
            //        )
            //    );
		}
		protected override void LoadComponentDefinition()
		{
			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			BXForumCollection forums = BXForum.GetList(
				new BXFilter(new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)),
				new BXOrderBy(new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)),
				new BXSelect(BXForum.Fields.Id, BXForum.Fields.Name),
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			ParamsDefinition["ForumId"].Values = forums.ConvertAll<BXParamValue>(delegate(BXForum input)
			{
				return new BXParamValue(input.Name, input.Id.ToString());
			});
			ParamsDefinition["AvailableForums"].Values = forums.ConvertAll<BXParamValue>(delegate(BXForum input)
			{
				return new BXParamValue(input.Name, input.Id.ToString());
			});

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

		public class TopicInfo
		{
			private BXForumTopic topic;
			private int replies;
			private int hiddenReplies;
			private bool hasNewMessages;
			private string topicHref;
			private string lastPostHref;
			private BXForumAuthorization auth;
			private string titleHtml;
			private string descriptionHtml;
			private string authorNameHtml;
			private string lastPosterNameHtml;
            private double? votingTotalValue = null;
            private int? votingTotalVotes = null;

			public string LastPosterNameHtml
			{
				get { return lastPosterNameHtml; }
				internal set { lastPosterNameHtml = value; }
			}


			public BXForumTopic Topic
			{
				get
				{
					return topic;
				}
				internal set
				{
					topic = value;
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
			public string DescriptionHtml
			{
				get
				{
					return descriptionHtml;
				}
				internal set
				{
					descriptionHtml = value;
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
			public int Replies
			{
				get
				{
					return replies;
				}
				internal set
				{
					replies = value;
				}
			}
			public int HiddenReplies
			{
				get
				{
					return hiddenReplies;
				}
				internal set
				{
					hiddenReplies = value;
				}
			}
			public bool HasNewPosts
			{
				get
				{
					return hasNewMessages;
				}
				internal set
				{
					hasNewMessages = value;
				}
			}
			public string TopicHref
			{
				get
				{
					return topicHref;
				}
				internal set
				{
					topicHref = value;
				}
			}
			public string LastPostHref
			{
				get
				{
					return lastPostHref;
				}
				internal set
				{
					lastPostHref = value;
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

            public double VotingTotalValue
            {
                get 
                {
                    return (votingTotalValue ?? (votingTotalValue = BXRatingDataLayerHelper.GetVotingTotalValue(topic))).Value;
                }
            }

            public double VotingTotalVotes
            {
                get
                {
                    return (votingTotalVotes ?? (votingTotalVotes = BXRatingDataLayerHelper.GetVotingTotalVotes(topic))).Value;
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
				: this (url, title, cssClass)
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
			FatalComponentNotExecuted = Fatal | 16,
			UnauthorizedRead = Unauthorized,
			UnauthorizedDelete = Unauthorized | 4,
			UnauthorizedOpenClose = Unauthorized | 8,
			UnauthorizedApprove = Unauthorized | 8 | 4,
			UnauthorizedStick = Unauthorized | 16,
			UnauthorizedMove = Unauthorized | 16 | 4,
			UnauthorizedSubscribe = Unauthorized | 16 | 8 | 4
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
			Hide
		}
		public enum ForumOperation
		{
			TopicSubscribe,
			PostsSubscribe,
			Unsubscribe
		}
	}

	public class ForumTopicListTemplate : BXComponentTemplate<ForumTopicListComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & ForumTopicListComponent.ErrorCode.Fatal) != 0)
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
