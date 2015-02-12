using Bitrix.Security;
using Bitrix.UI;
using System;
using Bitrix.Components;
using Bitrix.DataTypes;
using Bitrix.DataLayer;
using System.Collections.Generic;
using Bitrix.Services.Text;
using Bitrix.IO;
using System.Web;
using Bitrix.Services;
using System.Web.UI;
using System.IO;

namespace Bitrix.CommunicationUtility.Components
{
	public partial class PrivateMessageTopicReadComponent : BXComponent
	{
		//private int curUserId;
        private BXUser user;
		private int? mappingId;

		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
        private List<MessageInfo> messages;
		private BXPrivateMessageMapping mapping;
        private BXPrivateMessageTopic topic;
		private BXPrivateMessageChain processor;

		private bool templateIncluded;
		private Exception fatalException;
		private BXParamsBag<object> replace;
		private int maxWordLength = -1;
		private string topicReplyHref;
		private string topicEditHref;
		private string inviteUserHref;
		private string topicTitleHtml;
		private string topicDescriptionHtml;
        List<UserInfo> participatingUsers;
        BXPrivateMessageAuthorization auth;
        bool cssFromForum;
        bool colorCssFromForum;

        public BXPrivateMessageAuthorization Auth
        {
            get
            {
                return auth;
            }
        }

		public BXUser User
		{
			get
			{
				return user ?? (BXPrincipal.Current != null && BXPrincipal.Current.Identity.IsAuthenticated ? user = BXUser.GetById(((BXIdentity)BXPrincipal.Current.Identity).Id) : null);
			}
		}

        int? topicId;

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


		public int MappingId
		{
			get
			{
				return (mappingId ?? (mappingId = Parameters.GetInt("MappingId"))).Value;
			}
			set
			{
				mappingId = value;
			}
		}

        public int MessageId
        {
            get
            {
                return Parameters.Get<int>("MessageId", 0);
            }
            set
            {
                Parameters["MessageId"] = value.ToString();
            }
        }

        public bool ShowParticipants
        {
            get
            {
                return Parameters.GetBool("ShowParticipants", true);
            }
            set
            {
                Parameters["ShowParticipants"] = value.ToString();
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
		public List<MessageInfo> Messages
		{
			get
			{
				return messages;
			}
		}

		public BXPrivateMessageMapping Mapping
		{
			get
			{
				return mapping;
			}
		}

        public BXPrivateMessageTopic Topic
        {
            get
            {
                return topic;
            }
        }

		public string TopicUrlTemplate
		{
			get
			{
				return Parameters.GetString("TopicUrlTemplate","");
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
				return Parameters.GetString("TopicListUrlTemplate","");
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
				return Parameters.GetString("TopicReplyUrlTemplate","");
			}
			set
			{
				Parameters["TopicReplyUrlTemplate"] = value;
			}
		}

        public string TopicEditUrlTemplate
        {
            get
            {
                return Parameters.GetString("TopicEditUrlTemplate", "");
            }
            set
            {
                Parameters["TopicEditUrlTemplate"] = value;
            }
        }

		public string InviteUserUrlTemplate
		{
			get
			{
				return Parameters.GetString("InviteUserUrlTemplate", "");
			}
			set
			{
				Parameters["InviteUserUrlTemplate"] = value;
			}
		}

		public string MessageReadUrlTemplate
		{
			get
			{
				return Parameters.GetString("MessageReadUrlTemplate","");
			}
			set
			{
				Parameters["MessageReadUrlTemplate"] = value;
			}
		}

		public string MessageQuoteUrlTemplate
		{
			get
			{
				return Parameters.GetString("MessageQuoteUrlTemplate","");
			}
			set
			{
				Parameters["MessageQuoteUrlTemplate"] = value;
			}
		}
		public string MessageEditUrlTemplate
		{
			get
			{
				return Parameters.GetString("MessageEditUrlTemplate","");
			}
			set
			{
				Parameters["MessageEditUrlTemplate"] = value;
			}
		}

		public string UserProfileUrlTemplate
		{
			get
			{
				return Parameters.GetString("UserProfileUrlTemplate","");
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

		public string InviteUserHref
		{
			get
			{
				return inviteUserHref;
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

        public IEnumerable<UserInfo> ParticipatingUsers
        {
            get
            {
                return participatingUsers ?? (participatingUsers = new List<UserInfo>());
            }
        }

		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalInvalidPage:
					return GetMessage("Error.InvalidPage");
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.FatalTopicNotFound:
					return GetMessage("Error.TopicNotFound");
				case ErrorCode.FatalPostNotFound:
					return GetMessage("Error.PostNotFound");
                case ErrorCode.Unauthorized:
                    return GetMessage("Error.Unauthorized");
                case ErrorCode.FatalComponentNotExecuted:
                    return GetMessage("Error.FatalComponentNotExecuted");
				default:
					return GetMessage("Error.Unknown");
			}
		}

        public string EmptyMessage
        {
            get
            {
                return GetMessage("TopicIsEmpty");
            }
        }

		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;
			try
			{
				CacheMode = BXCacheMode.None;

                if ( User == null )
                {
                    Fatal(ErrorCode.Unauthorized);
					return;
                }

                if (TopicId <= 0)
                {
                    Fatal(ErrorCode.FatalTopicNotFound);
                    return;
                }

				// If mapping is not provided
				if (mapping == null)
				{
					BXFilter mappingFilter = new BXFilter();
					mappingFilter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId));
					mappingFilter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.Deleted, BXSqlFilterOperators.Equal, false));
                    mappingFilter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.User.Id, BXSqlFilterOperators.Equal, User.UserId));

					BXPrivateMessageMappingCollection mappingsCol = BXPrivateMessageMapping.GetList(
						mappingFilter,
						null,
						new BXSelectAdd(BXPrivateMessageMapping.Fields.Topic,
                                        BXPrivateMessageMapping.Fields.User),
						null
					);
					if (mappingsCol.Count != 0)
						mapping = mappingsCol[0];

				}

                if (mapping == null || mapping.Topic==null)
			    {
					Fatal(ErrorCode.FatalTopicNotFound);
					return;
				}
                topic = mapping.Topic;
                BXPrivateMessage targetMessage = null;
                if (MessageId != 0)
                {
                    if (MessageId < 0)
                    {
                        Fatal(ErrorCode.FatalPostNotFound);
                        return;
                    }
                    targetMessage = BXPrivateMessage.GetById(MessageId);
                    if (    targetMessage == null 
                            || targetMessage.Topic == null 
                            || targetMessage.Topic.Id != Topic.Id)
                    {
                        Fatal(ErrorCode.FatalPostNotFound);
                        return;
                    }

                }

                auth = new BXPrivateMessageAuthorization(targetMessage, mapping, User.UserId);

                if (!auth.CanReadThisTopic)
                {
                    Fatal(ErrorCode.UnauthorizedReadTopic);
                    return;
                }

				processor = new BXPrivateMessageChain();
				processor.MaxWordLength = MaxWordLength;
				processor.EnableImages = true;

				replace = new BXParamsBag<object>();
				replace["TopicId"] = topic.Id;
				topicReplyHref = MakeHref(TopicReplyUrlTemplate, replace);
                topicEditHref = MakeHref(TopicEditUrlTemplate, replace);
				inviteUserHref = MakeHref(InviteUserUrlTemplate, replace);
				topicTitleHtml = BXWordBreakingProcessor.Break(Topic.TextEncoder.Decode(Topic.Title ?? ""), MaxWordLength, true).Trim();


				BXFilter msgFilter = new BXFilter();
				msgFilter.Add(new BXFilterItem(BXPrivateMessage.Fields.Topic.Id, BXSqlFilterOperators.Equal, Topic.Id));
				msgFilter.Add(new BXFilterItem(BXPrivateMessage.Fields.IsSystem, BXSqlFilterOperators.Equal, false));


				int count = Topic.MessageCount;

				//Determine page
				BXPagingParams pagingParams = PreparePagingParams();
                if (targetMessage != null)
                {
                    BXFilter countFilter = new BXFilter();
                    countFilter.Add(new BXFilterItem(BXPrivateMessage.Fields.Id, BXSqlFilterOperators.Less, targetMessage.Id));
                    countFilter.Add(new BXFilterItem(BXPrivateMessage.Fields.Topic.Id, BXSqlFilterOperators.Equal, Topic.Id));

                    int index = BXPrivateMessage.Count(countFilter);

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

                BXPrivateMessageCollection col = BXPrivateMessage.GetList(
                    msgFilter,
                    new BXOrderBy(new BXOrderByPair(BXPrivateMessage.Fields.SentDate, BXOrderByDirection.Asc)),
                    new BXSelectAdd(BXPrivateMessage.Fields.FromUser.User,
                                    BXPrivateMessage.Fields.FromUser.User.Image),                 
                    queryParams
                    );
                
                participatingUsers = new List<UserInfo>();

                BXPrivateMessageMappingCollection mappings =
                    BXPrivateMessageMapping.GetList(new BXFilter(new BXFilterItem(BXPrivateMessageMapping.Fields.Topic.Id, BXSqlFilterOperators.Equal, Topic.Id)),
                                                    null,
                                                    new BXSelectAdd(BXPrivateMessageMapping.Fields.User.User),
                                                    null);

                foreach (BXPrivateMessageMapping m in mappings)
                {
                    UserInfo info = new UserInfo();
                    info.User = m.User.User;
                    info.MappingDeleted = m.Deleted;
                    participatingUsers.Add(info);
                }
                    

				int startNum = (queryParams != null && queryParams.AllowPaging) ? queryParams.PagingStartIndex : 0;
				this.messages = col.ConvertAll<MessageInfo>(delegate(BXPrivateMessage input)
				{
                    
					MessageInfo info = new MessageInfo();
					info.Message = input;
					info.msgChain = processor;
                    info.content = input.TextEncoder.Decode(input.Body);
					info.AuthorNameHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.FromUserName), MaxWordLength, true);
					info.Author = input.FromUser;
					info.Num = ++startNum;
					replace["MessageId"] = input.Id;
                    replace["FolderId"] = mapping.FolderId;

					if (input.FromUser != null && input.FromUser.User != null)
					{
						replace["UserId"] = input.FromUser.User.UserId;
						info.UserProfileHref = MakeHref(UserProfileUrlTemplate, replace);
						replace.Remove("UserId");
					}
                    info.Auth = new BXPrivateMessageAuthorization(input,mapping,User.UserId);
					info.ThisMsgHref = MakeHref(MessageReadUrlTemplate, replace);
					info.MsgEditHref = MakeHref(MessageEditUrlTemplate, replace);
					info.MsgQuoteHref = MakeHref(MessageQuoteUrlTemplate, replace);


					return info;
				});
				replace.Remove("MessageId");




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
						string title = mapping.Topic.TextEncoder.Decode(mapping.Topic.Title);
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
                return;
			}
            try
            {
                if (mapping != null)
                {
                    int mCount = mapping.UnreadMessageCount;
                    mapping.ReadDate = DateTime.Now;
                    mapping.Save();
                    if (mCount > 0)
                    {
                        BXPrivateMessageMappingCollection unreadMappings =
                            BXPrivateMessageMapping.GetList(
                                new BXFilter(
                                    new BXFilterItem(BXPrivateMessageMapping.Fields.User.Id, BXSqlFilterOperators.Equal, User.UserId),
                                    new BXFilterItem(BXPrivateMessageMapping.Fields.Deleted, BXSqlFilterOperators.Equal, false),
                                    new BXFilterItem(BXPrivateMessageMapping.Fields.UnreadMessageCount,BXSqlFilterOperators.Greater,0)
                                    ),
                                    null,
                                    new BXSelect(BXPrivateMessageMapping.Fields.UnreadMessageCount),
                                    null);

                        if (unreadMappings.Count == 0)
                            BXPrivateMessageCacheHelper.FlushCacheForUser(User.UserId);
                    }
                }
            }
            catch (Exception ex2 )
            {
                Fatal(ex2);
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
            Group = new BXComponentGroup("pmessages", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);

			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory mainCategory = BXCategory.Main;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

            ParamsDefinition.Add("MappingId", new BXParamText(GetMessageRaw("Param.TopicId"), "", mainCategory));
            ParamsDefinition.Add("MessageId", new BXParamText(GetMessageRaw("Param.MessageId"), "", mainCategory));
            ParamsDefinition.Add("ShowParticipants", new BXParamYesNo(GetMessageRaw("Param.ShowParticipants"), true, mainCategory));

            string path = "~/bitrix/components/bitrix/forum/templates/.default/style.css";
            if (!Bitrix.IO.BXSecureIO.FileExists(path))
				path = "~/bitrix/components/bitrix/pmessages/templates/.default/themes/style.css";
            else
                cssFromForum = true;


            ParamsDefinition["ThemeCssFilePath"] = new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"),
                path, mainCategory);
            path = "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css";

            if (!Bitrix.IO.BXSecureIO.FileExists(path))
                path = "~/bitrix/components/bitrix/pmessages/templates/.default/themes/default/style.css";
            else
                colorCssFromForum = true;

            ParamsDefinition["ColorCssFilePath"] = new BXParamSingleSelectionWithText(GetMessageRaw("Param.ColorCssFilePath"),
                path, mainCategory);
            
            ParamsDefinition.Add("TopicUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicUrlTemplate"), "viewtopic.aspx?topic=#TopicId#", urlCategory));
			ParamsDefinition.Add("TopicListUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicListUrlTemplate"), "viewtopics.aspx", urlCategory));
            ParamsDefinition.Add("TopicEditUrlTemplate", new BXParamText(GetMessageRaw("Param.TopicEditUrlTemplate"), "viewtopic.aspx?topic=#TopicId#&act=edit", urlCategory));
			ParamsDefinition.Add("InviteUserUrlTemplate", new BXParamText(GetMessageRaw("Param.InviteUserUrlTemplate"), "viewtopic.aspx?topic=#TopicId#&act=invite", urlCategory));
            ParamsDefinition.Add("MessageReadUrlTemplate", new BXParamText(GetMessageRaw("Param.MessageReadUrlTemplate"), "viewtopic.aspx?msg=#MessageId###msg#MessageId#", urlCategory));
            ParamsDefinition.Add("MessageQuoteUrlTemplate", new BXParamText(GetMessageRaw("Param.MessageQuoteUrlTemplate"), "viewtopic.aspx?topic=#TopicId#&msg=#MessageId#&act=quote", urlCategory));

            ParamsDefinition.Add("MessageEditUrlTemplate", new BXParamText(GetMessageRaw("Param.MessageEditUrlTemplate"), "viewtopic.aspx?msg=#MessageId#&act=edit", urlCategory));

            
			ParamsDefinition.Add("UserProfileUrlTemplate", new BXParamText(GetMessageRaw("Param.UserProfileUrlTemplate"), "profile.aspx?user=#UserId#", urlCategory));

			ParamsDefinition.Add("SetPageTitle", new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory));
			ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));
		}
		protected override void LoadComponentDefinition()
		{
			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            
            string curCssDir = String.Format("~/bitrix/components/bitrix/{0}/templates/.default/themes/", cssFromForum ? "forum" : "pmessages");

            DirectoryInfo info = new DirectoryInfo(BXPath.MapPath(curCssDir));
            if (info.Exists)
            {
                List<BXParamValue> values = new List<BXParamValue>();
                foreach (DirectoryInfo sub in info.GetDirectories())
                {
                    if (!File.Exists(Path.Combine(sub.FullName, "style.css")))
                        continue;

                    string themeTitle = BXLoc.GetMessage(String.Format("~/bitrix/components/bitrix/{0}/templates/.default/themes/", cssFromForum ? "forum" : "pmessages") + sub.Name + "/description", "Title");
                    if (String.IsNullOrEmpty(themeTitle))
                        themeTitle = sub.Name;

                    values.Add(new BXParamValue(themeTitle, VirtualPathUtility.Combine(curCssDir, sub.Name + "/style.css")));
                }
                ParamsDefinition["ColorCssFilePath"].Values = values;
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
		private string MakeHref(string template, BXParamsBag<object> parameters)
		{
			return Encode(ResolveTemplateUrl(template, parameters));
		}

        public class UserInfo
        {
            BXUser user;
            bool mappingDeleted;

            public BXUser User
            {
                get
                {
                    return user;
                }
                set
                {
                    if (value == null) throw new ArgumentNullException("user");
                    user = value;
                }
            }

            public bool MappingDeleted
            {
                get
                {
                    return mappingDeleted;
                }
                set
                {
                    mappingDeleted = value;
                }
            }

        }
		
		public class MessageInfo
		{
			internal BXPrivateMessageChain msgChain;
            BXPrivateMessageAuthorization auth;
            BXPrivateMessage msg;
			string authorNameHtml;
			string contentHtml;
			BXPrivateMessageUser author;
			int num;
			string userProfileHref;
			string thisMsgHref;
			string editMsgHref;
			string quoteMsgHref;
            int posts;
			internal string content;

			public BXPrivateMessage Message
			{
				get
				{
					return msg;
				}
				internal set
				{
					msg = value;
				}
			}

            public BXPrivateMessageAuthorization Auth
            {
                get
                {
                    return auth;
                }
                set
                {
                    if ( value == null ) throw new ArgumentNullException("Auth");
                    auth = value;
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
					return contentHtml ?? (contentHtml = msgChain.Process(content));
				}
			}


			public BXPrivateMessageUser Author
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
			public string ThisMsgHref
			{
				get
				{
					return thisMsgHref;
				}
				internal set
				{
					thisMsgHref = value;
				}
			}
			public string MsgEditHref
			{
				get
				{
					return editMsgHref;
				}
				internal set
				{
					editMsgHref = value;
				}
			}
			public string MsgQuoteHref
			{
				get
				{
					return quoteMsgHref;
				}
				internal set
				{
					quoteMsgHref = value;
				}
			}
		}

		[Flags]
		public enum ErrorCode
		{
			None,
			Error,
			Fatal,
			Unauthorized,
			FatalInvalidPage,
			FatalException ,
			FatalTopicNotFound,
			FatalPostNotFound,
			FatalComponentNotExecuted,
			UnauthorizedRead,
            UnauthorizedReadMessage,
            UnauthorizedReadTopic
		}

	}

	public class PrivateMessageTopicReadTemplate : BXComponentTemplate<PrivateMessageTopicReadComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & PrivateMessageTopicReadComponent.ErrorCode.Fatal) != 0)
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
