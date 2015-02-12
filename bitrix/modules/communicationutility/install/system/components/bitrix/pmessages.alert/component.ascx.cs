
using System.Collections.Generic;
using System;
using Bitrix.Components;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using System.Web.Hosting;
using System.Text;

namespace Bitrix.CommunicationUtility.Components
{
    public partial class PrivateMessageAlertComponent : BXComponent
    {
        BXUser user;
        int unreadMessagesCount;
		int unnotifiedMessagesCount;
        int maxWordLength = -1;
        const int maxPreiviewLength = 200;
        BXPrivateMessageAuthorization auth;

        List<MessageInfo> unnotifiedMessages;

        public IList<MessageInfo> UnnotifiedMessages
        {
            get
            {
				return unnotifiedMessages ?? (unnotifiedMessages = new List<MessageInfo>());
            }
        }

        public int	UnreadMessagesCount
        {
            get
            {
                return unreadMessagesCount;
            }
        }

		public int UnnotifiedMessagesCount
		{
			get
			{
				return unnotifiedMessagesCount;
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

		string messagesReadUrlResolved;

		public string MessagesReadUrlResolved
		{
			get { return messagesReadUrlResolved; }
		}

        public string MessagesReadUrl
        {
            get
            {
                return Parameters.GetString("MessagesReadUrl", "");
            }
            set
            {
                Parameters["MessagesReadUrl"] = value;
            }
        }

        protected BXUser User
        {
            get
            {
                if (!BXPrincipal.Current.Identity.IsAuthenticated) return null;
                return user ?? (user = BXUser.GetById(((BXIdentity)BXPrincipal.Current.Identity).Id));
            }
        }


        protected bool HaveUnreadMessages
        {
            get
            {
                if (User == null) return false;
                return BXPrivateMessageCacheHelper.HaveUnreadMessages(User.UserId);
            }
        }

		protected bool HaveUnnotifiedMessages
		{
			get
			{
				if (User == null) return false;
				return BXPrivateMessageCacheHelper.HaveUnnotifiedMessages(User.UserId);
			}
		}

        protected void Page_Load(object sender, EventArgs e)
        {

            if (User == null) return;
            auth = new BXPrivateMessageAuthorization(null, null, User.UserId);

			BXParamsBag<object> repl = new BXParamsBag<object>();
			repl.Add("UserId", User != null ? User.UserId : 0);

			messagesReadUrlResolved = ResolveTemplateUrl(MessagesReadUrl, repl);

            BXPrivateMessageChain processor = new BXPrivateMessageChain();

            if (HaveUnreadMessages && auth.CanRead)
            {
                
                BXFilter filter = new BXFilter();
                filter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.UnreadMessageCount, BXSqlFilterOperators.Greater, 0));
                filter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.User.Id, BXSqlFilterOperators.Equal, User.UserId));
                filter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.Deleted, BXSqlFilterOperators.Equal, false));

                BXPrivateMessageMappingCollection mappings =
                    BXPrivateMessageMapping.GetList(filter, null,new BXSelect(
																				BXPrivateMessageMapping.Fields.User,
																				BXPrivateMessageMapping.Fields.Topic,
																				BXPrivateMessageMapping.Fields.Folder.Id,
																				BXPrivateMessageMapping.Fields.UnreadMessageCount

																				),null);
                int messageCount=0;
                unnotifiedMessages = new List<MessageInfo>();
				BXPrivateMessageUser curUser=null;
                foreach (BXPrivateMessageMapping m in mappings)
                {
                    messageCount += m.UnreadMessageCount;
					if (!HaveUnnotifiedMessages)
						continue;
					if (curUser == null)
						curUser = m.User;
                    BXPrivateMessageCollection unNotifiedMessagesCol = BXPrivateMessage.GetList(
                            new BXFilter(new BXFilterItem(BXPrivateMessage.Fields.Topic.Id, BXSqlFilterOperators.Equal, m.TopicId),
                                        new BXFilterItem(BXPrivateMessage.Fields.Id, BXSqlFilterOperators.Greater, curUser.LastNotifiedMessageId),
										new BXFilterItem(BXPrivateMessage.Fields.FromUser.Id,BXSqlFilterOperators.NotEqual,curUser.Id)), null,
                                        new BXSelectAdd(BXPrivateMessage.Fields.Topic,BXPrivateMessage.Fields.FromUser.User.Image),null);

                    BXParamsBag<object> replace = new BXParamsBag<object>();
					List<MessageInfo> currentMessages = unNotifiedMessagesCol.ConvertAll<MessageInfo>(delegate(BXPrivateMessage input)
                        {
                            MessageInfo info = new MessageInfo();
                            if ( input.Topic!=null)
                                info.IsFirst = input.Topic.FirstMessageId == input.Id;
                            replace.Add("MessageId", input.Id);
                            replace.Add("FolderId", m.FolderId);
                            replace.Add("TopicId", input.TopicId);
							replace.Add("FromUserId", input.FromId);
							replace.Add("UserId",User!=null ? User.UserId : 0);
                            info.MessageHref = ResolveTemplateUrl(Parameters.GetString("MessageReadUrlTemplate"), replace);
                            info.Message = input;
                            if (input.FromUser != null && input.FromUser.User != null && input.FromUser.User.Image != null)
                            {
                                info.AvatarHref = input.FromUser.User.Image.FilePath;
                                info.AvatarWidth = input.FromUser.User.Image.Width;
                                info.AvatarHeight = input.FromUser.User.Image.Height;
                                info.AvatarAlt = input.FromUser.User.Image.Description;
                            }
                            else
                            {
                                info.AvatarHref = String.Empty;
                                info.AvatarAlt = String.Empty;
                            }

                            info.Processor = processor;
                            info.MaxPreivewLength = maxPreiviewLength;
                            string strPreview = processor.StripBBCode(input.Body);
                            strPreview = strPreview.Length > maxPreiviewLength ? strPreview.Substring(0, maxPreiviewLength) : strPreview;

                            info.MessagePreviewHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(strPreview), MaxWordLength, true);
                            info.TopicTitleHtml = BXWordBreakingProcessor.Break(m.Topic.TextEncoder.Decode(m.Topic.Title),MaxWordLength,true);
                            replace.Remove("MessageId");
                            replace.Remove("FolderId");
                            replace.Remove("TopicId");
							replace.Remove("UserId");
							replace.Remove("FromUserId");
                            return info;
                        });
                    unnotifiedMessages.AddRange(currentMessages);
                }
				

				if (unnotifiedMessages.Count > 0)
				{
					unnotifiedMessages.Sort(delegate(MessageInfo a, MessageInfo b)
					{
						return a.Message.SentDate.CompareTo(b.Message.SentDate);
					});
					unnotifiedMessagesCount = unnotifiedMessages.Count;
					BXPrivateMessageCacheHelper.FlushNotificationCacheForUser(curUser.Id);
					curUser.LastNotifiedMessageId = unnotifiedMessages[unnotifiedMessages.Count - 1].Message.Id;
					curUser.Save();
				}
                unreadMessagesCount = messageCount;
                
            }
            IncludeComponentTemplate();

   
        }
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";
            Group = new BXComponentGroup("pmessages", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);

            BXCategory urlCategory = BXCategory.UrlSettings;

            ParamsDefinition.Add("MessagesReadUrl",
                    new BXParamText(GetMessageRaw("Param.MessagesReadUrl"),
                                    "viewtopics.aspx",
                                    urlCategory
                                    ));

            ParamsDefinition.Add("MessageReadUrlTemplate",
                new BXParamText(GetMessageRaw("Param.MessageReadUrlTemplate"),
                        "viewtopics.aspx?topic=#TopicId#&message=#MessageId###msg#MessageId#",
                        urlCategory
                        ));
        }

        #region NESTED CLASSES

        public class MessageInfo
        {
            BXPrivateMessage message;
            BXPrivateMessageChain processor;
            string topicTitleHtml;
            string messageHref;
            string avatarHref;
            int avatarWidth;
            int avatarHeight;
            int maxPreviewLength;
            string avatarAlt;
            string messagePreviewHtml;
            bool isFirst;

            public int MaxPreivewLength
            {
                get
                {
                    return maxPreiviewLength;
                }
                set
                {
                    maxPreviewLength = value;
                }
            }

            public string MessagePreviewHtml
            {
                get
                {

                    return messagePreviewHtml;
                }
                set
                {
                    messagePreviewHtml = value;
                }
            }

            public BXPrivateMessageChain Processor
            {
                get
                {
                    return processor;
                }
                set
                {
                    if (value == null) throw new ArgumentNullException("BXPrivateMessageChain");
                    processor = value;
                }
            }

            public string AvatarHref
            {
                get
                {
                    return avatarHref;
                }
                set
                {
                    avatarHref = value;
                }
            }

            public string AvatarAlt
            {
                get
                {
                    return avatarAlt;
                }
                set
                {
                    avatarAlt = value;
                }
            }

            public int AvatarWidth
            {
                get
                {
                    return avatarWidth;
                }
                set
                {
                    avatarWidth = value;
                }
            }

            public int AvatarHeight
            {
                get
                {
                    return avatarHeight;
                }
                set
                {
                    avatarHeight = value;
                }
            }

            public bool IsFirst
            {
                get
                {
                    return isFirst;
                }
                set
                {
                    isFirst = value;
                }
            }

            public BXPrivateMessage Message
            {
                get
                {
                    return message;
                }
                internal set
                {
                    if (value == null) throw new ArgumentNullException("Private Message");
                    message = value;

                }
            }

            public string MessageHref
            {
                get
                {
                    return messageHref;
                }
                set
                {
                    messageHref = value;
                }
            }
            

            public string TopicTitleHtml
            {
                get
                {
                    return topicTitleHtml;
                }
                set
                {
                    topicTitleHtml = value;
                }
            }

        }

        public enum Error
        {
            Unauthorized
        }

        #endregion
    }

    public class PrivateMessageAlertTemplate : BXComponentTemplate<PrivateMessageAlertComponent>
    {
		protected string messageListString;

		protected string BuildMessageList()
		{
			StringBuilder s = new StringBuilder();
			s.Append("[");
			for (int i = 0; i < Component.UnnotifiedMessages.Count; i++)
			{
				Bitrix.CommunicationUtility.Components.PrivateMessageAlertComponent.MessageInfo info = Component.UnnotifiedMessages[i];
				s.AppendFormat("{{'topicTitle':'{0}','userName':'{1}','sentDate':'{2}','url':'{3}','isFirst':'{4}','avatarHref':'{5}','avatarWidth':'{6}','avatarHeight':'{7}','avatarAlt':'{8}','previewHtml':'{9}'}}",
										JSEncode(info.TopicTitleHtml),
										JSEncode(info.Message.FromUserName),
										info.Message.SentDate.ToString("g"),
										JSEncode(info.MessageHref),
										info.IsFirst,
										JSEncode(info.AvatarHref),
										info.AvatarWidth,
										info.AvatarHeight,
										JSEncode(info.AvatarAlt),
										JSEncode(info.MessagePreviewHtml)
										);
				if (i != Component.UnnotifiedMessages.Count - 1) s.Append(",");
			}
			s.Append("]");
			return s.ToString();
		}

		void RegisterPopupScript()
		{
			string path = Bitrix.IO.BXPath.Combine(Component.ComponentRoot, "/js/popup.js");
            if (Bitrix.IO.BXSecureIO.FileExists(path))
            {
                BXPage.Scripts.RequireUtils();
                BXPage.RegisterScriptInclude("~/bitrix/controls/Main/dialog/js/dialog_base.js");
                BXPage.RegisterScriptInclude(path);
                BXPage.RegisterStyle("~/bitrix/controls/Main/dialog/css/dialog_base.css");
            }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (Component.UnnotifiedMessagesCount > 0)
			{
				RegisterPopupScript();
				messageListString = BuildMessageList();
			}
			else return;
		}
    }
}
