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
using System.Web.Hosting;
using Bitrix.Services.Js;
using System.Text;

namespace Bitrix.CommunicationUtility.Components
{
	public partial class PrivateMessageFormComponent : BXComponent
	{
        const int maxRowsReturnedFromHandler = 20;

		private int? folderId;
		private int? topicId;
		private long? messageId;
        private int? mappingId;
		private BXPrivateMessageMapping mapping;
        private BXPrivateMessageTopic topic;
		private BXPrivateMessage message;
        private BXPrivateMessage parentMessage;
        private BXPrivateMessageUser pmUser;
		private Target componentTarget;
		private Mode componentMode;
		private BXPrivateMessageChain processor;
		private Data data;
		private List<string> errorSummary;
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private Exception fatalException;
		private bool templateIncluded;
		private int maxWordLength = -1;
        private BXPrivateMessageAuthorization auth;
        private bool cssFromForum;
        private bool colorCssFromForum;

		public long MessageId
		{
			get
			{
				return (messageId ?? (messageId = Parameters.GetLong("MessageId"))).Value;
			}
		}

        public int TopicId
        {
            get
            {
                return (topicId ?? (topicId = Parameters.GetInt("TopicId"))).Value;
            }
        }
		List<int> receivers;
        public List<int> Receivers
        {
            get
            {
                return receivers ?? (receivers = Parameters.GetListInt("Receivers") ?? new List<int>());
            }
        }

        List<BXUser> receiverUsers;

        public List<BXUser> ReceiverUsers
        {
            get
            {
                if (receiverUsers != null) return receiverUsers;
                List<int> receivers = Receivers;
                if (Receivers.Contains(User.UserId)) receivers.Remove(User.UserId);
                BXUserCollection users = BXUser.GetList(new BXFilter(new BXFilterItem(BXUser.Fields.UserId, BXSqlFilterOperators.In, receivers)), null);
                return receiverUsers = users.ConvertAll<BXUser>(delegate(BXUser input) { return input; });
            }
        }

        string receiverNamesList;

        public string ReceiverNamesList
        {
            get
            {
                BXParamsBag<object> replace = new BXParamsBag<object>();
                if (receiverNamesList != null) return receiverNamesList;
                string namesList = String.Join(", ",(
                    ReceiverUsers.ConvertAll<string>(delegate (BXUser input){
                        
                        replace.Add("UserId",input.UserId);
                        return String.Format("<a href=\"{0}\">{1}</a>",
                            ResolveTemplateUrl(Parameters["UserProfileUrlTemplate"], replace),input.GetDisplayName());
                    })).ToArray());
                return receiverNamesList = namesList;
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

		public BXPrivateMessage Message
		{
			get
			{
				return message;
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
            set
            {
              componentMode = value;
            }
		}

		public Data ComponentData
		{
			get
			{
				return data;
			}
		}

        BXUser user;

        public BXUser User
        {
            get
            {
                return user ?? (user = BXUser.GetById( ((BXIdentity)BXPrincipal.Current.Identity).Id));
            }
        }

		public List<string> ErrorSummary
		{
			get
			{
				return errorSummary ?? (errorSummary = new List<string>());
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

        public int MaxMessageCount
        {
            get
            {
                return Parameters.GetInt("MaxMessageCount", 0);
            }
            set
            {
                Parameters["MaxMessageCount"] = value.ToString();
            }
        }

		private BXPrivateMessageChain Processor
		{
			get
			{
				if (processor == null)
				{
					processor = new BXPrivateMessageChain();
					processor.MaxWordLength = MaxWordLength;
				}

				return processor;
			}
		}

		public bool Validate()
		{
			ErrorSummary.Clear();

			data.TopicTitle = data.TopicTitle != null ? data.TopicTitle.Trim() : null;

			if (ComponentTarget == Target.Topic && ComponentMode!=Mode.Invite && BXStringUtility.IsNullOrTrimEmpty(data.TopicTitle))
				ErrorSummary.Add(GetMessage("Error.TopicTitleRequired"));

			if (BXStringUtility.IsNullOrTrimEmpty(data.PostContent) && ComponentTarget==Target.Message)
				ErrorSummary.Add(GetMessage("Error.PostContentRequired"));
			else
				data.PostContent = Processor.CorrectBBCode(data.PostContent.Trim());

			return ErrorSummary.Count == 0;
		}

        Dictionary<int, BXPrivateMessageMapping> receiverMappings;

        Dictionary<int, BXPrivateMessageMapping> ReceiverMappings
        {
            get
            {
                if (receiverMappings != null) return receiverMappings;
                BXPrivateMessageMappingCollection col = BXPrivateMessageMapping.GetList(
                    new BXFilter(new BXFilterItem(BXPrivateMessageMapping.Fields.Topic.Id, BXSqlFilterOperators.Equal,Topic.Id)), 
                    null,
                    new BXSelectAdd(BXPrivateMessageMapping.Fields.User.User),
                    null
                    );
                receiverMappings = new Dictionary<int, BXPrivateMessageMapping>();
                foreach (BXPrivateMessageMapping m in col)
                {
                    receiverMappings.Add(m.User.Id, m);
                }
                return receiverMappings;
            }
        }

		public void Save()
		{
			if (FatalError != ErrorCode.None)
				return;
            try
            {
                if (User == null)
                {
                    Fatal(ErrorCode.FatalUnauthorized);
                    return;
                }

                if (ComponentMode == Mode.Add && pmUser != null)
                {
                    if (MessageSendingInterval > 0 && DateTime.Now.Subtract(pmUser.LastMessageDate).TotalSeconds < MessageSendingInterval)
                    {
                        Fatal(ErrorCode.RestrictedMoreThenOneMessageInInterval);
                        return;
                    }
                    if (MaxMessageCount > 0 && pmUser.MessageCount >= MaxMessageCount)
                    {
                        Fatal(ErrorCode.RestrictedAllowedMessageCountExceeded);
                        return;
                    }
                }

                string userName = User.TextEncoder.Decode(User.GetDisplayName());
                if (mapping == null) // we should create a topic and mappings 
                {
                    receiverUsers = null;
                    if (ReceiverUsers.Count == 0)
                    {
                        Fatal(ErrorCode.RestrictedNoReceiversFound);
                        return;
                    }

                    topic = new BXPrivateMessageTopic();
                    topic.StarterId = User.UserId;
                    topic.StarterName = userName;
                    topic.LastPosterName = userName;
                    topic.Title = data.TopicTitle;
                    topic.Save();
                    receiverMappings = new Dictionary<int, BXPrivateMessageMapping>();
                    BXPrivateMessageMapping mp;

                    int i = 0;
                    foreach (BXUser user in ReceiverUsers)
                    {
                        if (i >= MaxReceiversCount && MaxReceiversCount != 0)
                            continue;
                        mp = new BXPrivateMessageMapping();
                        mp.Deleted = false;
                        mp.FolderId = 0;
                        mp.NotifyByEmail = true;
                        mp.NotifySender = false;
                        mp.TopicId = topic.Id;
                        mp.UserId = user.UserId;
                        if (user.UserId == User.UserId)
                        {
                            mp.ReadDate = DateTime.Now;
                            mapping = mp;
                            mp.NotifyByEmail = data.NotifyByEmail;
                            mp.NotifySender = data.NotifySender;
                        }
                        else i++;

                        mp.Save();
                        receiverMappings.Add(user.UserId, mp);
                    }

                    if (mapping == null)
                    {
                        mp = new BXPrivateMessageMapping();
                        mp.Deleted = false;
                        mp.FolderId = 0;
                        mp.NotifyByEmail = data.NotifyByEmail;
                        mp.NotifySender = data.NotifySender;
                        mp.TopicId = topic.Id;
                        mp.UserId = User.UserId;
                        mp.ReadDate = DateTime.Now;
                        mp.NotifyByEmail = data.NotifyByEmail;
                        mp.NotifySender = data.NotifySender;
                        mapping = mp;
                        mp.Save();
                    }
                }

				if (ComponentTarget == Target.Topic && ComponentMode == Mode.Invite)
				{
					if (topic == null)
					{
						Fatal(ErrorCode.FatalTopicNotFound);
						return;
					}
					if (ReceiverUsers.Count == 0)
					{
						Fatal(ErrorCode.RestrictedNoReceiversFound);
						return;
					}
					var ms = BXPrivateMessageMapping.GetList(new BXFilter(new BXFilterItem(BXPrivateMessageMapping.Fields.Topic.Id, BXSqlFilterOperators.Equal, topic.Id)), null);
					var recUsers = new List<int>();
					recUsers.AddRange(ReceiverUsers.ConvertAll<int>(x=>x.UserId));

					foreach (var m in ms)
						recUsers.Remove(m.UserId);

					foreach (int userId in recUsers)
					{
						var newMapping = new BXPrivateMessageMapping();
						newMapping.UserId = userId;
						newMapping.TopicId = topic.Id;
						newMapping.NotifyByEmail = true;
						newMapping.NotifySender = false;
						newMapping.Deleted = false;
						newMapping.FolderId = 0;
						newMapping.ReadDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
						newMapping.Save();

						if ( userId != User.UserId )
							BXPrivateMessageCacheHelper.SetCacheForUser(userId);
					}
					Response.Redirect(TopicReadUrl);
				}

                if (componentTarget == Target.Topic && ComponentMode == Mode.Edit)
                {
                    if (topic == null)
                    {
                        Fatal(ErrorCode.FatalTopicNotFound);
                        return;
                    }
                    topic.Title = data.TopicTitle;
                    mapping.NotifySender = data.NotifySender;
                    mapping.NotifyByEmail = data.NotifyByEmail;
                    mapping.Save();
                    topic.Save();
                    //Response.Redirect(TopicReadUrl);
                }

                if (message == null)
                {
                    message = new BXPrivateMessage();
                    message.Body = data.PostContent;
                    message.FromId = User.UserId;
                    message.FromUserName = userName;
                    message.TopicId = topic.Id;
                    message.IsSystem = false;
                    message.SentDate = DateTime.Now;
                }

                if (ComponentMode == Mode.Edit)
                {
                    message.Body = data.PostContent;
                }
                message.Save();

                if (ComponentMode == Mode.Add)
                {
                    SendEmailNotification();
                    foreach (BXPrivateMessageMapping m in ReceiverMappings.Values)
                    {
                        if ( m.User.Id!=User.UserId )
                            BXPrivateMessageCacheHelper.SetCacheForUser(m.User.Id);
                    }
                }
                Response.Redirect(MessageReadUrl);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Fatal(ex);
            }

		}

        int ParentMessageId
        {
            get
            {
                return Parameters.GetInt("ParentMessageId");
            }
            set
            {
                Parameters["ParentMessageId"] = value.ToString();
            }
        }

        public string TopicTitleHtml
        {
            get 
            {
                if (topic == null) return string.Empty;
                return BXWordBreakingProcessor.Break(topic.TextEncoder.Decode(topic.Title ?? ""), MaxWordLength, true).Trim();
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

				case ErrorCode.FatalTopicNotFound:
					return GetMessage("Error.TopicNotFound");
				case ErrorCode.FatalMessageNotFound:
					return GetMessage("Error.PostNotFound");
				case ErrorCode.FatalUserNotFound:
					return GetMessage("Error.UserNotFound");
                case ErrorCode.RestrictedNoReceiversFound:
					return GetMessage("Error.NoReceiversFound");

				case ErrorCode.FatalUnauthorizedRead:
                    return GetMessage("Error.UnauthorizedRead");
				case ErrorCode.FatalUnauthorizedCreateTopic:
                    return GetMessage("Error.UnauthorizedCreateTopic");
                case ErrorCode.FatalUnauthorizedEditMessage:
                    return GetMessage("Error.UnauthorizedEditMessage");
                case ErrorCode.FatalUnauthorizedEditTopic:
                    return GetMessage("Error.UnauthorizedEditTopic");
                case ErrorCode.FatalUnauthorizedReply:
                    return GetMessage("Error.UnauthorizedReply");

				case ErrorCode.RestrictedMailBoxFull:
                    return GetMessage("Error.MailBoxFull");
                case ErrorCode.FatalComponentNotExecuted:
                    return GetMessage("Error.ComponentNotExecuted");
				case ErrorCode.RestrictedAllowedMessageCountExceeded:
                    return GetMessage("Error.FatalAllowedMessageCountExceeded");
                case ErrorCode.FatalUnauthorized:
                    return GetMessage("Error.Unauthorized");
                case ErrorCode.RestrictedMoreThenOneMessageInInterval:
                    return String.Format(GetMessage("Error.FatalMoreThenOneMessageInInterval"),MessageSendingInterval.ToString());
				default:
					return GetMessage("Error.Unknown");
			}
		}

        bool CheckPermissions()
        {

            auth = new BXPrivateMessageAuthorization(message, mapping, User.UserId);

            if (!auth.CanRead)
            {
                if (componentTarget == Target.Topic && componentMode == Mode.Add)
                    Fatal(ErrorCode.FatalUnauthorizedCreateTopic);
                else if (componentTarget == Target.Message && componentMode == Mode.Edit)
                    Fatal(ErrorCode.FatalMessageNotFound);
                else
                    Fatal(ErrorCode.FatalTopicNotFound);
                return false;
            }

            if (ComponentMode == Mode.Add && ComponentTarget == Target.Message && !auth.CanReply)
            {
                Fatal(ErrorCode.FatalUnauthorizedReply);
                return false;
            }
            if (ComponentMode == Mode.Add && ComponentTarget == Target.Topic && !auth.CanCreateTopic)
            {
                Fatal(ErrorCode.FatalUnauthorizedCreateTopic);
                return false;
            }
            if (ComponentMode == Mode.Edit && ComponentTarget == Target.Message && !auth.CanEditThisMessage)
            {
                Fatal(ErrorCode.FatalUnauthorizedEditMessage);
                return false;
            }
            if (ComponentMode == Mode.Edit && ComponentTarget == Target.Topic && !auth.CanEditThisTopic)
            {
                Fatal(ErrorCode.FatalUnauthorizedEditTopic);
                return false;
            }
            
            return true;
        }

		protected void Page_Load(object sender, EventArgs e)
		{
			fatalError = ErrorCode.None;
			if (ComponentIsHandler)
				return;

            if (User == null)
            {
                Fatal(ErrorCode.FatalUnauthorized);
                return;
            }

			try
			{
				componentMode = string.Equals(Parameters.GetString("Mode"), "edit", StringComparison.OrdinalIgnoreCase) ? Mode.Edit : 
					string.Equals(Parameters.GetString("Mode"), "invite", StringComparison.OrdinalIgnoreCase) ? Mode.Invite : Mode.Add;

				if (componentMode == Mode.Edit)
					componentTarget = MessageId > 0 ? Target.Message : Target.Topic;
				else if (componentMode == Mode.Invite)
					componentTarget = Target.Topic;
				else
					componentTarget = TopicId > 0 ? Target.Message : Target.Topic;

                if (componentMode == Mode.Add && componentTarget == Target.Topic)
                {

                    if (MaxReceiversCount > 0 && ReceiverUsers.Count > MaxReceiversCount)
                    {
                        ReceiverUsers.RemoveRange(MaxReceiversCount,ReceiverUsers.Count-MaxReceiversCount);
                    }
                        
                }

				if (!LoadEntities())
				{

					return;
				}

                if ( componentMode==Mode.Add && pmUser!=null && MaxMessageCount > 0 && pmUser.MessageCount >= MaxMessageCount)
                {
                    Fatal(ErrorCode.RestrictedAllowedMessageCountExceeded);
                    return;
                }

				if (!CheckPermissions())
					return;

				data = new Data(this);

                if (componentMode == Mode.Add && ComponentTarget == Target.Topic)
                    data.NotifyByEmail = true;

				if (message != null)
				{
					data.PostContent = message.TextEncoder.Decode(message.Body);
				}
                else if (parentMessage != null)

					data.PostContent = string.Format("[quote]{0}:\n{1}[/quote]",
						parentMessage.FromUserName,  
						parentMessage.Body
					);

				if (topic != null)
				{
					data.TopicTitle = topic.Title;
				}
                if ( mapping != null ){
                    data.NotifyByEmail = mapping.NotifyByEmail;
                    data.NotifySender = mapping.NotifySender;
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
						title = componentMode == Mode.Add ? GetMessage("PageTitle.NewTopic") : componentMode == Mode.Edit ? GetMessage("PageTitle.EditTopic") : GetMessage("PageTitle.InviteUser");
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

        public bool AllowNotifyByEmail
        {
            get
            {
                return Parameters.GetBool("AllowNotifyByEmail", false);
            }
            set
            {
                Parameters["AllowNotifyByEmail"] = value.ToString();
            }
        }

        public string MessageReadUrlTemplate
        {
            get
            {
                return Parameters.Get("MessageReadUrlTemplate","");
            }
            set
            {
                Parameters["MessageReadUrlTemplate"] = value;
            }
        }

        public int MessageSendingInterval
        {
            get
            {
                return Parameters.GetInt("MessageSendingInterval", 0);
            }
            set
            {
                Parameters["MessageSendingInterval"] = value.ToString();
            }
        }

        public int UserMessagesCount
        {
            get
            {
                if (pmUser == null) throw new ArgumentNullException("Private Message User");
                return pmUser.MessageCount;
            }
        }

        public string MailBoxStatusMessage
        {
            get
            {
                if (pmUser == null) throw new ArgumentNullException("Private Message User");
                if (MaxMessageCount == 0) return String.Empty;
                return String.Format(GetMessage("MailBoxStatusMessage"), MailBoxFillPercent, UserMessagesCount, MaxMessageCount);

            }
        }

        public int MailBoxFillPercent
        {
            get
            {
                if (pmUser == null) throw new ArgumentNullException("Private Message User");
                if ( MaxMessageCount == 0) return 0;
                return (int)Math.Round(((double)pmUser.MessageCount / MaxMessageCount * 100));
            }
        }

        string messageReadUrl;
        string redirectUrl;

        protected string MessageReadUrl
        {
            get
            {
                if (messageReadUrl != null) return messageReadUrl;
                BXParamsBag<object> replace = new BXParamsBag<object>();
                if ( message!=null )
                    replace.Add("MessageId",message.Id);
                replace.Add("TopicId",Topic.Id);
                return messageReadUrl = ResolveTemplateUrl(MessageReadUrlTemplate, replace);
            }
        }

        bool ComponentIsHandler
        {
            get 
            { 
                return Request.QueryString["query"] != null; 
            }
        }

        public string TopicReadUrlTemplate
        {
            get
            {
                return Parameters.Get("TopicReadUrlTemplate", "");
            }
            set
            {
                Parameters["TopicReadUrlTemplate"] = value;
            }
        }

        public int MaxReceiversCount
        {
            get
            {
                return Parameters.GetInt("MaxReceiversCount",0);
            }
            set
            {
                Parameters["MaxReceiversCount"] = value.ToString();
            }
        }

        protected string TopicReadUrl
        {
            get
            {
                if (redirectUrl != null) return redirectUrl;
                BXParamsBag<object> replace = new BXParamsBag<object>();
                replace.Add("TopicId", Topic.Id);
                return redirectUrl = ResolveTemplateUrl(TopicReadUrlTemplate, replace);
            }
        }

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
            Group = new BXComponentGroup("pmessages", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);

			BXCategory mainCategory = BXCategory.Main;
			BXCategory urlCategory = BXCategory.UrlSettings;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;


			ParamsDefinition["TopicId"] = new BXParamText(GetMessageRaw("Param.TopicId"), "", mainCategory);
            ParamsDefinition["Receivers"] = new BXParamText(GetMessageRaw("Param.Receivers"), "", mainCategory);
			ParamsDefinition["MessageId"] = new BXParamText(GetMessageRaw("Param.MessageId"), "", mainCategory);
            ParamsDefinition["MaxReceiversCount"] = new BXParamText(GetMessageRaw("Param.MaxReceiversCount"), "", mainCategory);
            ParamsDefinition["ParentMessageId"] = new BXParamText(GetMessageRaw("Param.ParentMessageId"), "", mainCategory);
			ParamsDefinition["Mode"] = new BXParamSingleSelection(GetMessageRaw("Param.Mode"), "add", mainCategory);
            ParamsDefinition["MessageSendingInterval"] = new BXParamText(GetMessageRaw("Param.MessageSendingInterval"), "0", mainCategory);
            ParamsDefinition["MaxMessageCount"] = new BXParamText(GetMessageRaw("Param.MaxMessageCount"), "100", mainCategory);
            ParamsDefinition["AllowNotifyByEmail"] = new BXParamYesNo(GetMessageRaw("Param.AllowNotifyByEmail"), true, mainCategory);

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

            ParamsDefinition["TopicReadUrlTemplate"] = new BXParamText(GetMessageRaw("Param.TopicReadUrlTemplate"), "viewtopics.aspx?topic=#TopicId#", urlCategory);
            ParamsDefinition["MessageReadUrlTemplate"] = new BXParamText(GetMessageRaw("Param.PostReadTemplate"), "viewtopic.aspx?msg=#MessageId###msg#MessageId#", urlCategory);
            ParamsDefinition["UserProfileUrlTemplate"] = new BXParamText(GetMessageRaw("Param.UserProfileUrlTemplate"), "profile.aspx?user=#UserId#", urlCategory);

			ParamsDefinition["SetPageTitle"] = new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory);
			ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "50", additionalSettingsCategory));
		}
		protected override void LoadComponentDefinition()
		{
			ParamsDefinition["Mode"].Values = new List<BXParamValue>(new BXParamValue[]	{
				new BXParamValue(GetMessageRaw("Mode.Create"), "add"),
				new BXParamValue(GetMessageRaw("Mode.Edit"), "edit"),
				new BXParamValue(GetMessageRaw("Mode.Invite"), "invite")
			});

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

		public string MakeAbsolute(string url)
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
			try
			{
				BXPrivateMessageUserCollection users =
					BXPrivateMessageUser.GetList(
						new BXFilter(new BXFilterItem(BXPrivateMessageUser.Fields.Id, BXSqlFilterOperators.Equal, User.UserId)),
						null,
						new BXSelectAdd(BXPrivateMessageUser.Fields.User),
						null);
				if (users.Count > 0)
				{
					pmUser = users[0];
					if (pmUser.User != null) user = pmUser.User;
					else
					{
						Fatal(ErrorCode.FatalUserNotFound);
						return false;
					}
				}
				else
				{

					pmUser = new BXPrivateMessageUser();
					pmUser.Id = User.UserId;
					pmUser.Save();

				}


				if (ComponentMode != Mode.Add || ComponentTarget != Target.Topic) // we have to load topic and mapping
				{
					BXPrivateMessageMappingCollection mappings =
						BXPrivateMessageMapping.GetList(new BXFilter(new BXFilterItem(BXPrivateMessageMapping.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId),
							new BXFilterItem(BXPrivateMessageMapping.Fields.User.Id, BXSqlFilterOperators.Equal, User.UserId)),
										 null,
										 new BXSelectAdd(BXPrivateMessageMapping.Fields.Topic,
														 BXPrivateMessageMapping.Fields.User),
										 null, BXTextEncoder.EmptyTextEncoder);
					if (mappings.Count == 0 || mappings[0].Topic == null || mappings[0].Deleted || mappings[0].User == null)
					{
						Fatal(ErrorCode.FatalTopicNotFound);
						return false;
					}
					mapping = mappings[0];
					topic = mappings[0].Topic;
				}

				if (ComponentMode == Mode.Edit) // we have to load message to edit
				{
					if (ComponentTarget == Target.Message)
					{
						if (MessageId <= 0)
						{
							Fatal(ErrorCode.FatalMessageNotFound);
							return false;
						}

						BXPrivateMessageCollection messages =
							BXPrivateMessage.GetList(new BXFilter(new BXFilterItem(BXPrivateMessage.Fields.Id, BXSqlFilterOperators.Equal, MessageId)),
													 null,
													 new BXSelectAdd(BXPrivateMessage.Fields.Topic),
													 null,
													 BXTextEncoder.EmptyTextEncoder);

						if (messages.Count > 0)
						{
							message = messages[0];
							if (message.Topic == null)
							{
								Fatal(ErrorCode.FatalTopicNotFound);
								return false;
							}
							topic = message.Topic;
						}
					}
					else
					{
						if (topic == null)
						{
							Fatal(ErrorCode.FatalTopicNotFound);
							return false;
						}
						message = BXPrivateMessage.GetById(topic.FirstMessageId);
						if (message == null)
						{
							Fatal(ErrorCode.FatalMessageNotFound);
							return false;
						}

					}
				}

				if (ParentMessageId > 0)
				{
					parentMessage = BXPrivateMessage.GetById(ParentMessageId, BXTextEncoder.EmptyTextEncoder);
					if (parentMessage == null)
					{
						Fatal(ErrorCode.FatalMessageNotFound);
						return false;
					}
				}
			}
			catch (Exception e)
			{
				Fatal(e);
				return false;
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

        void SendEmailNotification()
        {
            foreach (BXPrivateMessageMapping mapping in ReceiverMappings.Values)
            {
                if (mapping.UserId == User.UserId) continue;
                if (ReceiverMappings != null && ReceiverMappings.ContainsKey(mapping.UserId) && ReceiverMappings[mapping.UserId].NotifyByEmail)
                {
                    BXCommand cmd = new BXCommand("Bitrix.CommunicationUtility.PrivateMessagesNewMessage");
					if ( mapping.User!=null && mapping.User.User!=null )
						FillParameters(cmd.Parameters, mapping.User.User);
                    cmd.Send();
                }
            }
        }

        void FillParameters(BXParamsBag<object> parameters, BXUser user)
        {
            parameters["EmailTo"] = user.Email;
            parameters["SiteName"] = DesignerSite;
            parameters["ContentText"] = Message.TextEncoder.Decode(BXEmailBBCodeProcessor.BBCodeToText(Message.Body));
            parameters["ContentHtml"] = Message.TextEncoder.Decode(processor.Process(Message.Body));
            parameters["SentDate"] = Message.SentDate.ToString();
            parameters["MessageAuthorName"] = User.TextEncoder.Decode(User.GetDisplayName());
            parameters["TopicTitle"] =Topic.TextEncoder.Decode( Topic.Title);
            parameters["MessageUrl"] = MakeAbsolute(MessageReadUrl);
        }

        string Query
        {
            get
            {
                return Request.QueryString["query"];
            }
        }

        bool CheckRightsForHandler()
        {
            return BXCsrfToken.CheckTokenFromRequest(Request.QueryString) && System.Web.HttpContext.Current.User != null &&
                System.Web.HttpContext.Current.User.Identity.IsAuthenticated && 
                     (System.Web.HttpContext.Current.User as BXPrincipal).IsCanOperate("PMessagesTopicCreate");
        }

		public string GetReceiversInitialData(IList<string> userIds)
		{
			BXUserCollection users = BXUser.GetList(new BXFilter(new BXFilterItem(BXUser.Fields.UserId, BXSqlFilterOperators.In, userIds)), null);
			if (users.Count > 0)
			{
				return BXJSUtility.BuildJSArray(users.ConvertAll<Dictionary<string, object>>(delegate(BXUser input)
				{
					Dictionary<string, object> dic = new Dictionary<string, object>();
					dic.Add("text", "(" + input.UserId.ToString() + ") " + input.GetDisplayName());
					dic.Add("id", input.UserId);
					return dic;
				}));
			}
			return String.Empty;
		}

        protected override void Render(HtmlTextWriter writer)
        {
            if (ComponentIsHandler)
            {
                if (!CheckRightsForHandler())
                {
                    Response.Clear();
                    Response.Write("(false)");
                    Response.End();
                }
                string output = String.Empty;
				BXUserCollection users = BXUser.GetUsersByQuery(20,Query,1,BXTextEncoder.EmptyTextEncoder);
                    if (users.Count > 0)
					{
						output = BXJSUtility.BuildJSArray(users.ConvertAll<Dictionary<string, object>>(delegate(Bitrix.Security.BXUser input)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("name",input.GetDisplayName()+" ("+ input.UserId.ToString()+")");
                            dic.Add("id", input.UserId);
							dic.Add("json", BXJSUtility.BuildJSON(dic));
                            return dic;
                        }));
					}
                Response.Clear();
                Response.Write("(");
                Response.Write(output);
                Response.Write(")");
                Response.End();
            }
            else
            base.Render(writer);
        }

		//NESTED CLASSES
		public enum Target
		{
			Topic,
			Message
		}
		public enum Mode
		{
			Add,
			Edit,
			Invite
		}
		public enum ErrorCode
		{
			None = 0, 
			Fatal = 1,
			Restricted = 2,
			FatalUnauthorizedRead = Fatal | 4,
			FatalUnauthorizedCreateTopic = Fatal | 8,
			FatalUnauthorizedReply = Fatal | 8 | 4,
			FatalUnauthorizedEditTopic = Fatal | 16,
			FatalUnauthorizedEditMessage = Fatal |16 | 4,
			FatalTopicNotFound = Fatal | 16 | 8,
            FatalUserNotFound = Fatal | 16 | 8 | 4,
            FatalMessageNotFound = Fatal | 32,
            FatalComponentNotExecuted = Fatal | 32 | 16,
			FatalUnauthorized = Fatal | 32 | 16 | 8,
            FatalException = Fatal,

            RestrictedMailBoxFull = Restricted | 4,
            RestrictedNoReceiversFound = Restricted | 8,
			RestrictedAllowedMessageCountExceeded = Restricted | 8 | 4,
            RestrictedMoreThenOneMessageInInterval = Restricted | 16
		}
		public class Data
		{
			private PrivateMessageFormComponent component;
			private string postContent;
			private string topicTitle;
            private bool notifySender;
            private bool notifyByEmail;

            public bool NotifyByEmail
            {
                get
                {
                    return notifyByEmail;
                }
                set
                {
                    notifyByEmail = value;
                }
            }

            public bool NotifySender
            {
                get
                {
                    return notifySender;
                }
                set
                {
                    notifySender = value;
                }
            }

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

			internal Data(PrivateMessageFormComponent component)
			{
				this.component = component;
			}
		}


		
	}

	public class PrivateMessageFormTemplate : BXComponentTemplate<PrivateMessageFormComponent>
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
						Component.ComponentTarget == PrivateMessageFormComponent.Target.Message
						? (Component.ComponentMode == PrivateMessageFormComponent.Mode.Add
							? Component.GetMessageRaw("ButtonTitle.CreatePost")
							: Component.GetMessageRaw("ButtonTitle.EditPost")
						)
						: (Component.ComponentMode == PrivateMessageFormComponent.Mode.Add
						? Component.GetMessageRaw("ButtonTitle.CreateTopic") : 
						Component.ComponentMode == PrivateMessageFormComponent.Mode.Invite ?
							Component.GetMessageRaw("ButtonTitle.InviteUsers") : Component.GetMessageRaw("ButtonTitle.EditTopic")
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
                return defaultHeaderTitle ?? (
                    defaultHeaderTitle =
                        Component.ComponentTarget == PrivateMessageFormComponent.Target.Message
                        ? (Component.ComponentMode == PrivateMessageFormComponent.Mode.Add
                            ? String.Format(
                                    Component.GetMessage("HeaderTitle.CreatePost"),
                                    BXWordBreakingProcessor.Break(Component.Topic.Title, Component.MaxWordLength, true)
                                )
                            : Component.GetMessage("HeaderTitle.EditPost")
                        )
                        : (Component.ComponentMode == PrivateMessageFormComponent.Mode.Add
						? Component.GetMessage("HeaderTitle.CreateTopic") : Component.ComponentMode == PrivateMessageFormComponent.Mode.Invite ?
							 Component.GetMessage("HeaderTitle.InviteUsers")
                            : Component.GetMessage("HeaderTitle.EditTopic")
                        )
                   );
			}
		}


        protected virtual void LoadData(PrivateMessageFormComponent.Data data)
		{

		}
        protected virtual void SaveData(PrivateMessageFormComponent.Data data)
		{

		}
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
            if (!IsPostBack && Component.FatalError == PrivateMessageFormComponent.ErrorCode.None)
				LoadData(Component.ComponentData);
		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
            if ((Component.FatalError & PrivateMessageFormComponent.ErrorCode.Fatal) != 0)
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
