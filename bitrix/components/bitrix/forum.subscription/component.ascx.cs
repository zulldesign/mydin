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

namespace Bitrix.Forum.Components
{
	public partial class ForumSubscriptionComponent : BXComponent
	{
		private int userId;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private List<SubscriptionInfo> subscriptions;
		private bool templateIncluded;
		private Exception fatalException;

		private int maxWordLength = -1;
		private List<int> availableForums;

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

		public List<SubscriptionInfo> Subscriptions
		{
			get
			{
				return subscriptions ?? (subscriptions = new List<SubscriptionInfo>());
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

		public string ForumUrlTemplate
		{
			get
			{
				return Parameters.GetString("ForumUrlTemplate");
			}
			set
			{
				Parameters["ForumUrlTemplate"] = value;
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

		public void DoOperation(SubscriptionOperation operation, IList<int> subscriptionIds)
		{
			if (fatalError != ErrorCode.None || subscriptionIds == null || subscriptionIds.Count < 1)
				return;

			try
			{
				foreach (SubscriptionInfo subscriptionInfo in subscriptions)
				{
					if (!subscriptionIds.Contains(subscriptionInfo.Subscription.Id))
						continue;

					switch (operation)
					{
						case SubscriptionOperation.Unsubscribe:
							subscriptionInfo.Subscription.Delete();
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}

			Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
		}
		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalInvalidPage:
					return GetMessage("Error.InvalidPage");
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.UnauthorizedRead:
					return GetMessage("Error.NoRightsToRead");
				case ErrorCode.UnauthorizedDelete:
					return GetMessage("Error.NoRightsToDelete");
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

				userId = Parameters.GetInt("UserId", 0);

				if (BXIdentity.Current == null || !BXIdentity.Current.IsAuthenticated)
				{
					Fatal(ErrorCode.Unauthorized);
					return;
				}
				else if (userId > 0 && userId != BXIdentity.Current.Id)
				{
					Fatal(ErrorCode.UnauthorizedRead);
					return;
				}
				else
					userId = BXIdentity.Current.Id;


				//Право ForumSubscriptionManage
				//MovedTo

				BXFilter filter = new BXFilter(
					new BXFilterItem(BXForumSubscription.Fields.Subscriber.Id, BXSqlFilterOperators.Equal, userId),
					new BXFilterItem(BXForumSubscription.Fields.Forum.Active, BXSqlFilterOperators.Equal, true),
					new BXFilterItem(BXForumSubscription.Fields.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite),
					new BXFilterItem(
						BXForumSubscription.Fields.Forum.CheckPermissions,
						BXSqlFilterOperators.In,
						new string[] { BXForum.Operations.ForumPublicRead, BXForum.Operations.ForumSubscriptionManage }
					),
					new BXFilterOr(
						new BXFilterItem(BXForumSubscription.Fields.Topic.Approved, BXSqlFilterOperators.Equal, null),
						new BXFilterItem(BXForumSubscription.Fields.Topic.Approved, BXSqlFilterOperators.Equal, true)
					)
				);

				if (AvailableForums.Count > 0)
					filter.Add(new BXFilterItem(BXForumSubscription.Fields.Forum.Id, BXSqlFilterOperators.In, AvailableForums));

				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace["UserId"] = userId;

				bool legal;
				BXPagingParams pagingParams = PreparePagingParams();
				BXQueryParams queryParams = PreparePaging(
					pagingParams,
					delegate()
					{
						return BXForumSubscription.Count(filter);
					},
					replace,
					out legal
				);
				if (!legal)
				{
					Fatal(ErrorCode.FatalInvalidPage);
					return;
				}

				BXForumSubscriptionCollection subscriptions = BXForumSubscription.GetList(
					filter,
					new BXOrderBy(new BXOrderByPair(BXForumSubscription.Fields.Id, BXOrderByDirection.Desc)),
					new BXSelectAdd(BXForumSubscription.Fields.Forum, BXForumSubscription.Fields.Topic),
					queryParams
				);

				this.subscriptions = subscriptions.ConvertAll<SubscriptionInfo>(delegate(BXForumSubscription input)
				{
					SubscriptionInfo info = new SubscriptionInfo();
					info.Subscription = input;
					info.Topic = input.Topic;
					info.Forum = input.Forum;

					if (info.Topic != null)
					{
						info.TitleHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(info.Topic.Name), MaxWordLength, true);
						info.DescriptionHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(info.Topic.Description), MaxWordLength, true);

						info.Replies = input.Topic.Replies;
						info.LastPosterNameHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Topic.LastPosterName), MaxWordLength, true);
						info.LastPostDate = input.Topic.LastPostDate;
						info.LastPostId = input.Topic.LastPostId;

						replace.Clear();
						replace["TopicId"] = input.Topic.Id;
						replace["ForumId"] = input.Topic.ForumId;
						replace["PostId"] = input.Topic.FirstPostId;
						info.DetailHref = Encode(ResolveTemplateUrl(TopicUrlTemplate, replace));

						replace["PostId"] = input.Topic.LastPostId;
						info.LastPostHref = Encode(ResolveTemplateUrl(PostReadUrlTemplate, replace));
						info.LastPosterId = input.Topic.LastPosterId;
					}
					else
					{
						info.TitleHtml = info.Forum.Name;
						info.DescriptionHtml = info.Forum.Description;

						info.Replies = info.Forum.Replies;
						info.LastPosterNameHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(info.Forum.LastPosterName), MaxWordLength, true);
						info.LastPostDate = input.Forum.LastPostDate;
						info.LastPosterId = input.Forum.LastPosterId;
						info.LastPostId = input.Forum.LastPostId;

						replace.Clear();
						replace.Add("ForumId", input.Forum.Id);
						replace.Add("ForumCode", input.Forum.Code);
						info.DetailHref = Encode(ResolveTemplateUrl(ForumUrlTemplate, replace));

						if (input.Forum.LastPostId > 0 && input.Forum.LastPost != null)
						{
							replace.Add("TopicId", input.Forum.LastPost.TopicId);
							replace.Add("PostId", input.Forum.LastPostId);
							info.LastPostHref = Encode(ResolveTemplateUrl(PostReadUrlTemplate, replace));
						}
					}

					return info;
				});

				if (!templateIncluded)
				{
					templateIncluded = true;
					IncludeComponentTemplate();
				}

				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null && !IsComponentDesignMode && Parameters.GetBool("SetPageTitle", true))
					bitrixPage.Title = GetMessage("Page.Title");
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

			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory mainCategory = BXCategory.Main;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

			ParamsDefinition.Add("ThemeCssFilePath", new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/style.css", mainCategory));
			ParamsDefinition.Add("ColorCssFilePath", new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css", mainCategory));
			ParamsDefinition.Add("UserId", new BXParamText(GetMessageRaw("Param.UserId"), "0", mainCategory));

			ParamsDefinition.Add("ForumUrlTemplate", new BXParamText(GetMessageRaw("Param.ForumUrlTemplate"), "viewforum.aspx?forum=#ForumId#", urlCategory));
			ParamsDefinition.Add("TopicUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicUrlTemplate"), "viewtopic.aspx?topic=#TopicId#", urlCategory));
			ParamsDefinition.Add("PostReadUrlTemplate", new BXParamText(GetMessageRaw("Param.PostReadUrlTemplate"), "viewtopic.aspx?post=#PostId###post#PostId#", urlCategory));

			ParamsDefinition.Add("AvailableForums", new BXParamMultiSelection(GetMessageRaw("Param.AvailableForums"), "", additionalSettingsCategory));
			ParamsDefinition.Add("SetPageTitle", new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory));
			ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));
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

		public class SubscriptionInfo
		{
			private BXForumTopic topic;
			private BXForum forum;
			private BXForumSubscription subscription;

			private int replies;
			private string detailHref;
			private string lastPostHref;
			private BXForumAuthorization auth;
			private string titleHtml;
			private string descriptionHtml;

			private int lastPosterId;
			private DateTime lastPostDate;
			private string lastPosterNameHtml;
			private Int64 lastPostId;


			public BXForum Forum
			{
				get
				{
					return forum;
				}
				internal set
				{
					forum = value;
				}
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
			public BXForumSubscription Subscription
			{
				get
				{
					return subscription;
				}
				internal set
				{
					subscription = value;
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

			public string DetailHref
			{
				get
				{
					return detailHref;
				}
				internal set
				{
					detailHref = value;
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
			public string LastPosterNameHtml
			{
				get { return lastPosterNameHtml; }
				internal set { lastPosterNameHtml = value; }
			}

			public DateTime LastPostDate
			{
				get { return lastPostDate; }
				internal set { lastPostDate = value; }
			}

			public int LastPosterId
			{
				get { return lastPosterId; }
				internal set { lastPosterId = value; }
			}

			public Int64 LastPostId
			{
				get { return lastPostId; }
				internal set { lastPostId = value; }
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

		}

		[Flags]
		public enum ErrorCode
		{
			None = 0,
			Error = 1,
			Fatal = Error,
			Unauthorized = Error | 2,
			FatalInvalidPage = Fatal | 4,
			FatalException = Fatal | 8,
			FatalComponentNotExecuted = Fatal | 16,
			UnauthorizedRead = Unauthorized,
			UnauthorizedDelete = Unauthorized | 8
		}

		public enum SubscriptionOperation
		{
			Unsubscribe
		}
	}

	public class ForumSubscriptionTemplate : BXComponentTemplate<ForumSubscriptionComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & ForumSubscriptionComponent.ErrorCode.Fatal) != 0)
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
