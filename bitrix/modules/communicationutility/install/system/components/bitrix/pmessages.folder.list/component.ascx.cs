

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
	public partial class PrivateMessageFolderListComponent : BXComponent
	{
		ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
        BXPrivateMessageAuthorization auth;
        List<FolderInfo> folders;
		bool templateIncluded;
		Exception fatalException;
		BXParamsBag<object> replace;
		int maxWordLength = -1;
        bool cssFromForum;
        bool colorCssFromForum;

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


        protected string DefaultColorCssFilePath
        {
            get { return "~/bitrix/components/bitrix/pmessages/templates/.default/themes/default/style.css"; }
        }

        protected string DefaultThemeCssFilePath
        {
            get { return "~/bitrix/components/bitrix/pmessages/templates/themes/style.css"; }
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
                case ErrorCode.UnauthorizedRead:
                    return GetMessage("Error.UnauthorizedRead");
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
            replace = new BXParamsBag<object>();
			try
			{
				CacheMode = BXCacheMode.None;

				if (User==null || User.UserId <= 0)
				{
                    Fatal(ErrorCode.Unauthorized);
					return;
				}

                auth = new BXPrivateMessageAuthorization(null, null, User.UserId);

                if (!auth.CanManageFolders)
                {
                    Fatal(ErrorCode.UnauthorizedRead);
                    return;
                }

                BXPrivateMessageFolderCollection foldersCol = BXPrivateMessageFolder.GetList(new BXFilter(
                            new BXFilterItem(   BXPrivateMessageFolder.Fields.User.Id, BXSqlFilterOperators.Equal, User.UserId)),
                                                new BXOrderBy(new BXOrderByPair(BXPrivateMessageFolder.Fields.Sort,BXOrderByDirection.Asc))  
                                                );

                BXPrivateMessageMappingCollection mappings = BXPrivateMessageMapping.GetList(new BXFilter(
                            new BXFilterItem(BXPrivateMessageMapping.Fields.User.Id, BXSqlFilterOperators.Equal, User.UserId),
                            new BXFilterItem(BXPrivateMessageMapping.Fields.Folder.Id, BXSqlFilterOperators.NotEqual, 0),
                            new BXFilterItem(BXPrivateMessageMapping.Fields.Deleted,BXSqlFilterOperators.Equal,false)
                            ), 
                            new BXOrderBy(new BXOrderByPair(BXPrivateMessageMapping.Fields.ReadDate,BXOrderByDirection.Desc)),
                            new BXSelectAdd(BXPrivateMessageMapping.Fields.Topic),null);

                Dictionary<int, BXPrivateMessageMapping> foldersMappings = new Dictionary<int, BXPrivateMessageMapping>();

                foreach (BXPrivateMessageMapping mapping in mappings)
                {
                    if (mapping.UnreadMessageCount > 0)
                        if (foldersMappings.ContainsKey(mapping.FolderId) && foldersMappings[mapping.FolderId].UnreadMessageCount == 0)
                            foldersMappings[mapping.FolderId] = mapping;
                    if ( foldersMappings.ContainsKey(mapping.FolderId) ) continue;
                    foldersMappings.Add(mapping.FolderId, mapping);
                }

                folders = foldersCol.ConvertAll<FolderInfo>(delegate(BXPrivateMessageFolder input)
                    {
                        FolderInfo info = new FolderInfo();
                        info.Folder = input;
                        replace.Add("FolderId", input.Id);
                        bool haveKey = false;
                        if (foldersMappings.ContainsKey(input.Id))
                        {
                            BXPrivateMessageMapping curMapping = foldersMappings[input.Id];
                            info.HaveUnreadMessages = curMapping.UnreadMessageCount > 0;
                            replace.Add("MessageId", curMapping.Topic.LastMessageId);
                            replace.Add("TopicId", curMapping.TopicId);
                            info.TopicTitleHtml = BXWordBreakingProcessor.Break(
                                curMapping.Topic.TextEncoder.Decode(curMapping.Topic.Title), MaxWordLength, true);
                            info.LastPosterNameHtml = BXWordBreakingProcessor.Break(
                                curMapping.Topic.TextEncoder.Decode(curMapping.Topic.LastPosterName), MaxWordLength, true);
                            info.LastMessageDateHtml = curMapping.Topic.LastMessageDate.ToString("g");

                            haveKey = true;
                        }
                        else
                        {
                            info.HaveUnreadMessages = false;
                        }

                        info.FolderUrl = ResolveTemplateUrl(Parameters.GetString("FolderUrlTemplate"), replace);
                        info.LastMessageUrl = haveKey ? ResolveTemplateUrl(Parameters.GetString("MessageReadUrlTemplate"),replace) : String.Empty;
                        replace.Remove("FolderId");
                        replace.Remove("MessageId");
                        info.TitleHtml = input.Title;
                        
                        return info;
                    }
                );

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
            Group = new BXComponentGroup("pmessages", GetMessageRaw("PrivateMessages"), 100, BXComponentGroup.Communication);

            BXCategory urlCategory = BXCategory.UrlSettings;
            BXCategory mainCategory = BXCategory.Main;
            BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

            ParamsDefinition.Add("FolderUrlTemplate", 
                                new BXParamText(GetMessageRaw("Param.FolderUrlTemplate"),
                                                "viewfolders.aspx?folder=#FolderId#", 
                                                urlCategory
                                                ));
            ParamsDefinition.Add("MessageReadUrlTemplate",
                    new BXParamText(GetMessageRaw("Param.MessageReadUrlTemplate"),
                                    "viewtopic.aspx?topic=#TopicId#&msg=#MessageId###msg#MessageId#",
                                    urlCategory
                                    ));

            string path = "~/bitrix/components/bitrix/forum/templates/.default/style.css";
            if (!Bitrix.IO.BXSecureIO.FileExists(path))
            {
                path = "~/bitrix/components/bitrix/pmessages/templates/.default/themes/style.css";
                
            }
            else cssFromForum = true;

            ParamsDefinition["ThemeCssFilePath"] = new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"),
                path, mainCategory);
            path = "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css";
            if (!Bitrix.IO.BXSecureIO.FileExists(path))
            {
                path = "~/bitrix/components/bitrix/pmessages/templates/.default/themes/default/style.css";
               
            }
            else colorCssFromForum = true;
            ParamsDefinition["ColorCssFilePath"] = new BXParamSingleSelectionWithText(GetMessageRaw("Param.ColorCssFilePath"),
                path, mainCategory);

            ParamsDefinition.Add("SetPageTitle", new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), false, additionalSettingsCategory));
            ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));
		}
		protected override void LoadComponentDefinition()
		{

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

        public void DoOperation(FolderOperation operation, IDictionary<int,string> ids)
        {
            if (fatalError != ErrorCode.None)
				return;
            if ( User == null ) 
            {
                Fatal(ErrorCode.FatalUserNotFound);
                return;
            }
            if (!auth.CanManageFolders)
            {
                Fatal(ErrorCode.Unauthorized);
                return;
            }
            try
            {
                BXPrivateMessageFolderCollection folders =
                    BXPrivateMessageFolder.GetList(new BXFilter(new BXFilterItem(BXPrivateMessageFolder.Fields.Id, BXSqlFilterOperators.In, ids.Keys),
                                                                new BXFilterItem(BXPrivateMessageFolder.Fields.User.Id, BXSqlFilterOperators.Equal, User.UserId)),
                                                   null);


                if (operation == FolderOperation.MoveDown || operation == FolderOperation.MoveUp)
                {
                    string[] strIds = new string[ids.Count];
                    int i = 0;
                    foreach (int id in ids.Keys)
                        strIds[i++] = id.ToString();
                    BXPrivateMessageFolder.Move(strIds, operation == FolderOperation.MoveUp);
                }
                else
                {
                    foreach (BXPrivateMessageFolder folder in folders)
                    {
                        switch (operation)
                        {
                            case FolderOperation.Delete:
                                folder.Delete();
                                break;
                            case FolderOperation.Edit:
                                if (!BXStringUtility.IsNullOrTrimEmpty(ids[folder.Id]))
                                {
                                    folder.Title = ids[folder.Id];
                                    folder.Save();
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Fatal(e);
            }
        }

        public void CreateFolder(string newFolderName)
        {
            if (User == null)
            {
                Fatal(ErrorCode.FatalUserNotFound);
                return;
            }

            if (!auth.CanManageFolders)
            {
                Fatal(ErrorCode.Unauthorized);
                return;
            }

            if (!BXStringUtility.IsNullOrTrimEmpty(newFolderName))
            {
                try
                {
                    BXPrivateMessageFolder folder = new BXPrivateMessageFolder();
                    folder.UserId = User.UserId;
                    folder.Title = newFolderName;
                    folder.Save();
                }
                catch (Exception e)
                {
                    Fatal(e);
                }
            }

        }

        public enum ErrorCode
        {
            None,
            Fatal,
            FatalException,
            FatalComponentNotExecuted,
            FatalUserNotFound,
            FatalInvalidPage,
            Unauthorized,
            UnauthorizedRead
        }

        public enum FolderOperation
        {
            Create,
            Edit,
            Delete,
            MoveUp,
            MoveDown
        }

        public class FolderInfo
        {
            BXPrivateMessageFolder folder;
            string titleHtml;
            string folderUrl;
            bool haveUnreadMessages;
            string lastMessageUrl;
            string topicTitleHtml;
            string lastMessageDateHtml;
            string lastPosterNameHtml;

            public int Id
            {
                get { return folder.Id; }
            }

            public string TitleHtml
            {
                get { return titleHtml; }
                set { titleHtml = value; }
            }

            public string TopicTitleHtml
            {
                get { return topicTitleHtml; }
                set { topicTitleHtml = value; }
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

            public string FolderUrl
            {
                get
                {
                    return folderUrl;
                }
                set
                {
                    folderUrl = value;
                }
            }

            public bool HaveUnreadMessages
            {
                get
                {
                    return haveUnreadMessages;
                }
                set
                {
                    haveUnreadMessages = value;
                }
            }

            public string LastMessageUrl
            {
                get
                {
                    return lastMessageUrl;
                }
                set
                {
                    lastMessageUrl = value;
                }
            }

            public string LastMessageDateHtml
            {
                get
                {
                    return lastMessageDateHtml;
                }
                set
                {
                    lastMessageDateHtml = value;
                }
            }

            public string LastPosterNameHtml
            {
                get
                {
                    return lastPosterNameHtml;
                }
                set
                {
                    lastPosterNameHtml = value;
                }
            }
           
        }
	}

	public class PrivateMessageFolderListTemplate : BXComponentTemplate<PrivateMessageFolderListComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
            if ((Component.FatalError & PrivateMessageFolderListComponent.ErrorCode.Fatal) != 0)
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
