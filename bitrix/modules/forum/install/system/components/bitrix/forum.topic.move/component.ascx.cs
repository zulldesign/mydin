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

namespace Bitrix.Forum.Components
{
	public partial class ForumTopicMoveComponent : BXComponent
	{
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private bool templateIncluded;
		private Exception fatalException;
		private BXParamsBag<object> replace;
		private List<int> availableForums;
		private List<int> topicsToMove;
		private List<TopicInfo> topics;
		private List<ForumInfo> forums;
		private int userId;

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
		public List<int> AvailableForums
		{
			get
			{
				return availableForums ?? (availableForums = Parameters.GetListInt("AvailableForums"));
			}
		}
		public List<int> TopicsToMove
		{
			get
			{
				return topicsToMove ?? (topicsToMove = Parameters.GetListInt("TopicsToMove"));
			}
		}
		public List<TopicInfo> Topics
		{
			get
			{
				return topics;
			}
		}
		public List<ForumInfo> Forums
		{
			get
			{
				return forums;
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

		public void Return()
		{
			string redirectUrl = Parameters.GetString("RedirectUrlTemplate");
			if (BXStringUtility.IsNullOrTrimEmpty(redirectUrl))
				Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
			RedirectTemplateUrl(redirectUrl.Trim(), replace);
		}
		public void MoveTopics(IEnumerable<int> topicIds, int targetForum, bool createLinks)
		{
			if (fatalError != ErrorCode.None)
				return;
			try
			{
				BXForum target = null;
				foreach (ForumInfo f in forums)
					if (f.Forum.Id == targetForum)
					{
						target = f.Forum;
						break;
					}

				if (target == null)
				{
					Fatal(ErrorCode.FatalTargetForumNotFound);
					return;
				}

				BXForumTopicCollection targetTopics = new BXForumTopicCollection();
				foreach (int id in topicIds)
				{
					foreach (TopicInfo t in topics)
						if (t.Topic.Id == id && t.Topic.ForumId != targetForum)
						{
							targetTopics.Add(t.Topic);
							break;
						}
				}

				foreach (BXForumTopic t in targetTopics)
				{
					int originalId = t.ForumId;
					t.ForumId = target.Id;
					t.Save();

					if (createLinks)
					{
						BXForumTopic link = t.CreateLink(originalId);
						link.Save();
					}
				}

				replace["ForumId"] = target.Id;
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}

			Return();
		}
		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.UnauthorizedRead:
					return GetMessage("Error.NoRightsToView");
				case ErrorCode.UnauthorizedMove:
					return GetMessage("Error.NoRightsToMove");
				case ErrorCode.FatalTargetForumNotFound:
					return GetMessage("Error.TargetForumNotFound");
				case ErrorCode.FatalTopicsNotFound:
					return GetMessage("Error.TopicsNotFound");
				case ErrorCode.FatalTargetForumsNotFound:
					return GetMessage("Error.TargetForumsNotFound");
				default:
					return GetMessageRaw("Error.Unknown");
			}
		}
		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;
			try
			{
				CacheMode = BXCacheMode.None;

				userId = ((BXIdentity)BXPrincipal.Current.Identity).Id;

				replace = new BXParamsBag<object>();

				BXFilter f1 = new BXFilter();
				f1.Add(new BXFilterItem(BXForumTopic.Fields.Id, BXSqlFilterOperators.In, TopicsToMove));
				f1.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Active, BXSqlFilterOperators.Equal, true));
				f1.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));
				if (AvailableForums.Count > 0)
					f1.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.In, AvailableForums));
				List<BXForumTopic> topicsList = BXForumTopic.GetList(
					f1,
					null,
					new BXSelectAdd(BXForumTopic.Fields.Forum),
					null
				).FindAll(delegate(BXForumTopic input)
				{
					return
						BXForum.IsUserCanOperate(input.ForumId, BXForum.Operations.ForumPublicRead)
						&& (input.Approved || BXForum.IsUserCanOperate(input.ForumId, BXForum.Operations.ForumTopicApprove));
				});

				if (topicsList.Count == 0)
				{
					Fatal(ErrorCode.FatalTopicsNotFound);
					return;
				}
				topics = topicsList.FindAll(delegate(BXForumTopic input)
				{
					return
						BXForum.IsUserCanOperate(input.ForumId, BXForum.Operations.ForumTopicMove)
						|| userId != 0 && input.AuthorId == userId && BXForum.IsUserCanOperate(input.ForumId, BXForum.Operations.ForumOwnTopicMove);
				}).ConvertAll<TopicInfo>(delegate(BXForumTopic input)
				{
					TopicInfo info = new TopicInfo();
					info.Topic = input;
					replace["ForumId"] = input.ForumId;
					replace["TopicId"] = input.Id;
					info.TopicHref = Encode(ResolveTemplateUrl(TopicUrlTemplate, replace));
					return info;
				});
				replace.Remove("ForumId");
				replace.Remove("TopicId");

				if (topics.Count == 0)
				{
					Fatal(ErrorCode.UnauthorizedMove);
					return;
				}

				BXFilter f2 = new BXFilter();
				f2.Add(new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true));
				f2.Add(new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));
				f2.Add(new BXFilterItem(BXForum.Fields.CheckPermissions, BXSqlFilterOperators.Equal, BXForum.Operations.ForumTopicCreate));
				f2.Add(new BXFilterItem(BXForum.Fields.CheckPermissions, BXSqlFilterOperators.Equal, BXForum.Operations.ForumPublicRead));
				if (AvailableForums.Count > 0)
					f2.Add(new BXFilterItem(BXForum.Fields.Id, BXSqlFilterOperators.In, AvailableForums));
				forums = BXForum.GetList(f2, null).ConvertAll<ForumInfo>(delegate(BXForum input)
				{
					ForumInfo info = new ForumInfo();
					info.Forum = input;
					return info;
				});

				if (forums.Count == 0)
				{
					Fatal(ErrorCode.FatalTargetForumsNotFound);
					return;
				}

				if (!templateIncluded)
				{
					templateIncluded = true;
					IncludeComponentTemplate();
				}

				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null && !IsComponentDesignMode && Parameters.GetBool("SetPageTitle", true))
					bitrixPage.Title = topics.Count == 1 ? GetMessage("PageTitle.Singular") : GetMessage("PageTitle.Plural");

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

			ParamsDefinition["ThemeCssFilePath"] = new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/style.css", mainCategory);
			ParamsDefinition["ColorCssFilePath"] = new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css", mainCategory);

			ParamsDefinition["TopicsToMove"] = new BXParamText(GetMessageRaw("Param.TopicsToMove"), "", additionalSettingsCategory);
			ParamsDefinition["AvailableForums"] = new BXParamMultiSelection(GetMessageRaw("Param.AvailableForums"), "", additionalSettingsCategory);
			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);

			ParamsDefinition["TopicUrlTemplate"] = new BXParamText(GetMessageRaw("Param.TopicUrlTemplate"), "viewtopic.aspx?topic=#TopicId#", urlCategory);
			ParamsDefinition["RedirectUrlTemplate"] = new BXParamText(GetMessageRaw("Param.RedirectUrlTemplate"), "viewforum.aspx?forum=#ForumId#", urlCategory);
		}
		protected override void LoadComponentDefinition()
		{
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
		
		[Flags]
		public enum ErrorCode
		{
			None = 0,
			Error = 1,
			Fatal = Error,
			Unauthorized = Error | 2,
			FatalException = Fatal | 4,
			FatalTargetForumNotFound = Fatal | 8,
			FatalTopicsNotFound = Fatal | 8 | 4,
			FatalComponentNotExecuted = Fatal | 16,
			FatalTargetForumsNotFound = Fatal | 16 | 4,
			UnauthorizedRead = Unauthorized,
			UnauthorizedMove = Unauthorized | 4
		}
		public class TopicInfo
		{
			private BXForumTopic topic;
			private string topicHref;

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
		}
		public class ForumInfo
		{
			private BXForum forum;

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
		}
	}

	public class ForumTopicMoveTemplate : BXComponentTemplate<ForumTopicMoveComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & ForumTopicMoveComponent.ErrorCode.Fatal) != 0)
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
