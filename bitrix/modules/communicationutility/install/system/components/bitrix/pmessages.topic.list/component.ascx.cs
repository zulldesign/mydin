

using Bitrix.UI;
using System.Collections.Generic;
using System;
using Bitrix.DataLayer;
using System.Data.SqlTypes;
using System.Threading;
using Bitrix.Security;
using Bitrix.Components;
using Bitrix.DataTypes;
using Bitrix.Services.Text;
using Bitrix.IO;
using System.IO;
using System.Web;
using Bitrix.Services;
using System.Web.UI;
namespace Bitrix.CommunicationUtility.Components
{
	public partial class PrivateMessageTopicListComponent : BXComponent
	{
		ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		List<TopicInfo> topics;
        List<FolderInfo> folders;
		string newTopicHref;
		bool templateIncluded;
		Exception fatalException;
		BXParamsBag<object> replace;
		int maxWordLength = -1;
        BXPrivateMessageAuthorization auth;
        bool cssFromForum;
        bool colorCssFromForum;

        public BXPrivateMessageAuthorization Auth
        {
            get { return auth; }
        }

        public IList<FolderInfo> Folders
        {
            get
            {
                return folders ?? (folders = new List<FolderInfo>());
            }
        }

        BXUser user;

        public BXUser User
        {
            get
            {
				return user ?? (BXPrincipal.Current != null && BXPrincipal.Current.Identity.IsAuthenticated ? user = BXUser.GetById(((BXIdentity)BXPrincipal.Current.Identity).Id) : null);
            }
        }

        public int FolderId
        {
            get
            {
                return Parameters.GetInt("FolderId", -1);
            }
            set
            {
                Parameters["FolderId"]=value.ToString();
            }
        }

        public DisplayMode ComponentDisplayMode
        {
            get
            {
                DisplayMode mode = DisplayMode.All;

                try
                {
                    mode = (DisplayMode)Enum.Parse(typeof(DisplayMode),
                            Parameters.Get<String>("Displaymode","All"));
                    return Enum.IsDefined(typeof(DisplayMode), mode) ? mode : DisplayMode.All;
                }
                catch { }
                return mode;
            }
            set
            {
                    Parameters["DisplayMode"] = value.ToString();
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

        public string TopicReadUrlTemplate
        {
            get
            {
                return Parameters.GetString("TopicReadUrlTemplate","");
            }
            set
            {
                Parameters["TopicReadUrlTemplate"] = value;
            }
        }

        public string NewMessageUrlTemplate
        {
            get
            {
                return Parameters.GetString("NewMessageUrlTemplate", "");
            }
            set
            {
                Parameters["NewMessageUrlTemplate"] = value;
            }
        }

        BXPrivateMessageFolder curFolder;

        public BXPrivateMessageFolder CurrentFolder
        {
            get
            {
                return curFolder;
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

		public string NewTopicHref
		{
			get
			{
				return newTopicHref;
			}
		}

        protected string DefaultColorCssFilePath
        {
            get { return "~/bitrix/components/bitrix/pmessages/templates/.default/themes/default/style.css"; }
        }

        protected string DefaultThemeCssFilePath
        {
            get { return "~/bitrix/components/bitrix/pmessages/templates/.default/style.css"; }
        }

		public void DoOperation(TopicOperation operation, IEnumerable<int> topicIds, int FolderId)
		{
			if (fatalError != ErrorCode.None)
				return;

			try
			{
                if (operation == TopicOperation.Move )
                {
                    if (!auth.CanManageFolders)
                    {
                        Fatal(ErrorCode.UnauthorizedFolderMove);
                        return;
                    }
                    List<int> ids = new List<int>();
                    foreach ( int t in topicIds ) 
                        ids.Add(t);
                    BXPrivateMessageMapping.Move(User.UserId,FolderId, ids);
                }

				BXFilter filter = new BXFilter();
                filter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.Id, BXSqlFilterOperators.In, topicIds));

				BXPrivateMessageMappingCollection mappings = BXPrivateMessageMapping.GetList(
					filter,
					null
				);

				foreach (BXPrivateMessageMapping mapping in mappings)
				{
                    BXPrivateMessageAuthorization mAuth = new BXPrivateMessageAuthorization(null, mapping, User.UserId);


                        switch (operation)
                        {
                            case TopicOperation.Delete:
                                if (mAuth.CanDeleteThisTopic)
                                {
                                    mapping.Deleted = true;
                                    mapping.Save();
                                }
                                break;
                            case TopicOperation.MarkAsRead:
                                if (mAuth.CanRead)
                                {
                                    mapping.ReadDate = DateTime.Now;
                                    mapping.Save();  
                                }
                                break;
                            case TopicOperation.RemoveReadMark:
                                if (mAuth.CanRead)
                                {
                                    mapping.ReadDate = (DateTime)SqlDateTime.MinValue;
                                    mapping.Save();
                                }
                                break;
                            case TopicOperation.EnableEmailNotification:
                                if (mAuth.CanRead)
                                {
                                    mapping.NotifyByEmail = true;
                                    mapping.Save();
                                }
                                break;
                            case TopicOperation.DisableEmailNotification:
                                if (mAuth.CanRead)
                                {
                                    mapping.NotifyByEmail = false;
                                    mapping.Save();
                                }
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
	

		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalInvalidPage:
					return GetMessage("Error.InvalidPage");
                case ErrorCode.FatalUserNotFound:
                    return GetMessage("Error.UserNotFound");
                case ErrorCode.Unauthorized:
                    return GetMessage("Error.Unauthorized");
                case ErrorCode.UnauthorizedFolderMove:
                    return GetMessage("Error.UnauthorizedFolderMove");
                case ErrorCode.UnauthorizedTopicEdit:
                    return GetMessage("Error.UnauthorizedTopicEdit");
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? 
                        ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
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

				if (User==null || User.UserId <= 0)
				{
                    Fatal(ErrorCode.Unauthorized);
					return;
				}

                auth = new BXPrivateMessageAuthorization(null, null, User.UserId);

                if (!auth.CanRead)
                {
                    Fatal(ErrorCode.Unauthorized);
                    return;
                }

                if (auth.CanManageFolders)
                {
                    BXPrivateMessageFolderCollection foldersCol = BXPrivateMessageFolder.GetList(new BXFilter(
                                new BXFilterItem(BXPrivateMessageFolder.Fields.User.Id, BXSqlFilterOperators.Equal, User.UserId)),
                                                    new BXOrderBy(new BXOrderByPair(BXPrivateMessageFolder.Fields.Title, BXOrderByDirection.Asc))
                                                    );
                    folders = foldersCol.ConvertAll<FolderInfo>(delegate(BXPrivateMessageFolder input)
                        {
                            FolderInfo info = new FolderInfo();
                            info.Folder = input;
                            info.TitleHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Title), MaxWordLength, true);
                            return info;
                        }
                    );
                }

				BXFilter topicFilter = new BXFilter();
                topicFilter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.User.Id, BXSqlFilterOperators.Equal, User.UserId));
                topicFilter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.Deleted, BXSqlFilterOperators.Equal, false));

                switch ( ComponentDisplayMode )
                {
                    case DisplayMode.Out:
                        topicFilter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.Topic.Starter.Id,BXSqlFilterOperators.Equal,User.UserId));
                        break;
                    case DisplayMode.In:
                        topicFilter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.Topic.Starter.Id, BXSqlFilterOperators.NotEqual, User.UserId));
                        break;
                }

                topicFilter.Add(new BXFilterItem(BXPrivateMessageMapping.Fields.Folder.Id,BXSqlFilterOperators.Equal,FolderId==-1 ? 0 : FolderId));

                if (FolderId > 0)
                    curFolder = BXPrivateMessageFolder.GetById(FolderId);

                BXOrderBy topicOrderBy = new BXOrderBy(new BXOrderByPair(BXPrivateMessageMapping.Fields.Topic.LastMessageDate,BXOrderByDirection.Desc));

				replace = new BXParamsBag<object>();

				//newTopicHref = Encode(ResolveTemplateUrl(NewTopicUrlTemplate, replace));

				bool legal;
				BXPagingParams pagingParams = PreparePagingParams();
				BXQueryParams queryParams = PreparePaging(
					pagingParams,
					delegate()
					{
						return BXPrivateMessageMapping.Count(topicFilter);
					},
					replace,
					out legal
				);
				if (!legal)
				{
					Fatal(ErrorCode.FatalInvalidPage);
					return;
				}

                BXPrivateMessageMappingCollection mappings = 
                    BXPrivateMessageMapping.GetList(topicFilter,
                                                    topicOrderBy,
                                                    new BXSelectAdd(BXPrivateMessageMapping.Fields.Topic),
                                                    queryParams);

                this.topics = mappings.ConvertAll<TopicInfo>(delegate(BXPrivateMessageMapping input)
				{
                    TopicInfo info = new TopicInfo();
                    info.TitleHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Topic.Title), MaxWordLength, true);
                    info.LastPosterNameHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Topic.LastPosterName), MaxWordLength, true);
                    info.StarterNameHtml = BXWordBreakingProcessor.Break(input.TextEncoder.Decode(input.Topic.StarterName), MaxWordLength, true);
                    replace["MessageId"] = input.Topic.LastMessageId;
                    replace["TopicId"] = input.TopicId;
                    replace["FolderId"] = FolderId;
                    info.LastMessageHref = ResolveTemplateUrl(MessageReadUrlTemplate, replace);
                    
                    info.TopicHref = ResolveTemplateUrl(TopicReadUrlTemplate, replace);
                    info.Topic = input.Topic;
                    info.Mapping = input;

                    return info;
				});


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
                        string title = String.Empty;
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
            Group = new BXComponentGroup("pmessages", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);

            BXCategory urlCategory = BXCategory.UrlSettings;
            BXCategory mainCategory = BXCategory.Main;
            BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

			BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

            ParamsDefinition.Add("FolderId", new BXParamText(GetMessageRaw("Param.FolderId"), "", mainCategory));

            List<BXParamValue> displayParams = new List<BXParamValue>();
            foreach (string name in Enum.GetNames(typeof(DisplayMode)))
            {
                displayParams.Add(new BXParamValue(
                    GetMessageRaw("Param.DisplayMode." +name),name));
            }

            ParamsDefinition.Add(
                "DisplayMode", 
                new BXParamSingleSelection(
                    GetMessageRaw("Param.DisplayMode"),
                    DisplayMode.All.ToString(), 
                    mainCategory,
                    displayParams
                )
            );

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

            ParamsDefinition.Add("NewMessageUrlTemplate", 
                                new BXParamText(GetMessageRaw("Param.NewMessageUrlTemplate"),
                                                "viewtopic.aspx?topic=#TopicId#", 
                                                urlCategory
                                                ));

            ParamsDefinition.Add("TopicReadUrlTemplate", 
                                new BXParamText(GetMessageRaw("Param.TopicReadUrlTemplate"),
                                                "viewtopic.aspx?topic=#TopicId#", 
                                                urlCategory
                                                ));

            ParamsDefinition.Add("MessageReadUrlTemplate", 
                                new BXParamText(GetMessageRaw("Param.MessageReadUrlTemplate"),
                                                "viewtopic.aspx?msg=#MessageId###msg#MessageId#", 
                                                urlCategory
                                                ));

            ParamsDefinition.Add("SetPageTitle", new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), false, additionalSettingsCategory));
            ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));

			
		}
		protected override void LoadComponentDefinition()
		{
			BXParametersDefinition.SetPaging(
                                                ParamsDefinition, 
                                                ClientID, 
                                                BXParametersDefinition.PagingParamsOptions.DisablePreLoad 
                                                    | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
			BXParametersDefinition.SetPagingUrl(
                                                ParamsDefinition, 
                                                ClientID, 
                                                BXParametersDefinition.PagingParamsOptions.DisablePreLoad | 
                                                BXParametersDefinition.PagingParamsOptions.DisableShowAll
                                                );

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

		public class TopicInfo
		{
			BXPrivateMessageTopic topic;
            BXPrivateMessageMapping mapping;
			bool hasNewMessages;
            int unreadMessageCount;
			string titleHtml;
			string authorNameHtml;
            string lastMessageHref;
            string topicHref;
			string lastPosterNameHtml;
            string starterNameHtml;
            int mappingId;
            bool notifyByEmail;

            public BXPrivateMessageMapping Mapping
            {
                get
                {
                    return mapping;
                }
                set
                {
                    if (value == null) throw new ArgumentNullException("mapping");

                    mapping = value;
                }
            }

            public int MappingId
            {
                get 
                { 
                    return mapping == null ? 0 : mapping.Id; 
                }
            }

            public bool NotifyByEmail
            {
                get
                {
                    return mapping == null ? true : mapping.NotifyByEmail;
                }
            }

            public string StarterNameHtml
            {
                get { return starterNameHtml; }
                set { starterNameHtml = value; }
            }

			public string LastPosterNameHtml
			{
				get { return lastPosterNameHtml; }
				internal set { lastPosterNameHtml = value; }
			}

            public string TitleHtml
            {
                get { return titleHtml; }
                set { titleHtml = value; }
            }

            public string LastMessageHref
            {
                get { return lastMessageHref; }
                set { lastMessageHref = value; }
            }

            public string TopicHref
            {
                get { return topicHref; }
                set { topicHref = value; }
            }

            public int UnreadMessageCount
            {
                get
                { 
                    return mapping==null ? 0 : mapping.UnreadMessageCount; 
                }
            }

            public int MessageCount
            {
                get
                {
                    return topic.MessageCount;
                }
            }

			public BXPrivateMessageTopic Topic
			{
				get
				{
					return topic;
				}
				internal set
				{
                    if (value == null)
                        throw new ArgumentNullException("topic");
					topic = value;
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

		}

        public enum DisplayMode
        {
            All,
            In,
            Out
        }
		[Flags]
		public enum ErrorCode
		{
			None = 0,
			Error = 1,
			Fatal = Error,
			Unauthorized,
            UnauthorizedFolderMove,
            UnauthorizedTopicEdit,
			FatalInvalidPage,
			FatalException, 
			FatalComponentNotExecuted, 
            FatalUserNotFound,
		}
		public enum TopicOperation
		{
			Delete,
			Move,
            MarkAsRead,
            RemoveReadMark,
            EnableEmailNotification,
            DisableEmailNotification
		}

        public class FolderInfo
        {
            BXPrivateMessageFolder folder;
            string titleHtml;

            public int Id
            {
                get { return folder.Id; }
            }

            public string TitleHtml
            {
                get { return titleHtml; }
                set { titleHtml = value; }
            }

            public BXPrivateMessageFolder Folder
            {
                get { return folder; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException("folder");
                    folder = value;
                }
            }
        }
	}


	public class PrivateMessageTopicListTemplate : BXComponentTemplate<PrivateMessageTopicListComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
            if ((Component.FatalError & PrivateMessageTopicListComponent.ErrorCode.Fatal) != 0)
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
