using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Bitrix.Components;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.Services;
using System.Collections.Specialized;
using Bitrix.DataLayer;
using System.IO;

namespace Bitrix.CommunicationUtility.Components
{
    public partial class PrivateMessagesMenuComponent : BXComponent
    {
        ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
        bool templateIncluded = false;
        Exception fatalException;
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

		public string IndexUrl
		{
			get
			{
				return Parameters.GetString("IndexUrl");
			}
			set
			{
				Parameters["IndexUrl"] = value;
			}
		}

        public string FoldersUrl
        {
            get
            {
                return Parameters.GetString("FoldersUrl");
            }
            set
            {
                Parameters["FoldersUrl"] = value;
            }
        }

        public string NewTopicUrl
        {
            get
            {
                return Parameters.GetString("NewTopicUrl");
            }
            set
            {
                Parameters["NewTopicUrl"] = value;
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

        public string FolderUrlTemplate
        {
            get
            {
                return Parameters.GetString("FolderUrlTemplate");
            }
            set
            {
                Parameters["FolderUrlTemplate"] = value;
            }
        }

        public string MyTopicsTitle
        {
            get
            {
                return GetMessage("MyTopics.Title");

            }
        }

        public bool ShowFolders
        {
            get
            {
                return Parameters.GetBool("ShowFolders");
            }
            set
            {
                Parameters["ShowFolders"] = value.ToString();
            }
        }

        List<FolderInfo> folders;

        public IList<FolderInfo> Folders
        {
            get
            {
                return folders ?? (folders = new List<FolderInfo>());
            }
        }

		private string currentUrl;
		public string CurrentUrl
		{
			get
			{
				if (currentUrl == null)
				{
					UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
					NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
					query.Remove("ReturnUrl");
					uri.Query = query.ToString();
					currentUrl = HttpUtility.UrlEncode(uri.Uri.PathAndQuery);
				}

				return currentUrl;
			}
		}

		private string userProfileUrl;
		public string UserProfileUrl
		{
			get
			{
				return userProfileUrl;
			}
		}

        private List<MenuItem> menuItems;
		public List<MenuItem> MenuItems
		{
			get
			{
				return menuItems;
			}
		}

		private BXUser currentUser;
		public BXUser CurrentUser
		{
			get
			{
				if (currentUser == null && BXPrincipal.Current != null)
				{
					BXIdentity identity = BXPrincipal.Current.GetIdentity();
					currentUser = identity != null ? identity.User : null;
				}

				return currentUser;
			}
		}

		private BXParamsBag<object> GetMenuTextReplace()
		{
			BXParamsBag<object> replace = new BXParamsBag<object>();
			replace["AppRoot"] = BXUri.ApplicationPath;
			replace["ReturnUrl"] = CurrentUrl;

			if (CurrentUser != null)
			{
				replace["UserName"] = HttpUtility.HtmlEncode(CurrentUser.UserName);
				replace["UserDisplayName"] = HttpUtility.HtmlEncode(CurrentUser.GetDisplayName());
				replace["UserId"] = CurrentUser.UserId;
				replace["UserFirstName"] = HttpUtility.HtmlEncode(CurrentUser.FirstName);
				replace["UserLastName"] = HttpUtility.HtmlEncode(CurrentUser.LastName);
				replace["UserEmail"] = HttpUtility.HtmlEncode(CurrentUser.Email);
			}
			return replace;
		}

        public ErrorCode FatalError
        {
            get
            {
                return fatalError;
            }
        }

        public string GetErrorHtml(ErrorCode code)
        {
            switch (code)
            {
                case ErrorCode.FatalComponentNotExecuted:
                    return GetMessage("Error.ComponentNotExecuted");
                case ErrorCode.UnauthorizedRead:
                    return GetMessage("Error.UnauthorizedRead");
                case ErrorCode.FatalErrorUnknown:
                    return GetMessage("Error.ErrorUnknown");
                case ErrorCode.Unauthorized:
                    return GetMessage("Error.Unauthorized");
                case ErrorCode.FatalException:
                    return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ?
                        ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
                default:
                    return GetMessage("Error.Unknown");
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

		protected void Page_Load(object sender, EventArgs e)
		{
			menuItems = new List<MenuItem>();
            fatalError = ErrorCode.None;



            if (CurrentUser == null )
            {
                Fatal(ErrorCode.Unauthorized);
                return;
            }

            auth = new BXPrivateMessageAuthorization(null, null, CurrentUser.UserId);

            if (!auth.CanRead)
            {
                Fatal(ErrorCode.UnauthorizedRead);
                return;
            }

            try
            {

                if (!string.IsNullOrEmpty(IndexUrl))
                    menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Topics"), ResolveTemplateUrl(IndexUrl, null), "pmessages", ""));

                if (!string.IsNullOrEmpty(NewTopicUrl) && auth.CanCreateTopic)
                    menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.NewTopic"), ResolveTemplateUrl(NewTopicUrl, null), "pmessages", ""));

                if (!string.IsNullOrEmpty(FoldersUrl) && auth.CanManageFolders)
                    menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Folders"), ResolveTemplateUrl(FoldersUrl, null), "pmessages", ""));

                if (ShowFolders && auth.CanManageFolders)
                {
                    BXPrivateMessageFolderCollection col = BXPrivateMessageFolder.GetList(new BXFilter(new BXFilterItem(
                                                                                            BXPrivateMessageFolder.Fields.User.Id,
                                                                                            BXSqlFilterOperators.Equal,
                                                                                            CurrentUser.UserId
                                                                                            )
                                                                          ),
                                                                          new BXOrderBy(new BXOrderByPair(BXPrivateMessageFolder.Fields.Sort,BXOrderByDirection.Asc)));
                    BXParamsBag<object> replace = new BXParamsBag<object>();



                    folders = col.ConvertAll<FolderInfo>(delegate(BXPrivateMessageFolder input)
                        {
                            FolderInfo info = new FolderInfo();
                            info.Folder = input;
                            replace.Add("FolderId", input.Id);
                            info.Href = ResolveTemplateUrl(FolderUrlTemplate, replace);
                            replace.Remove("FolderId");
                            return info;

                        }
                    );
                }
            }
            catch (Exception ex)
            {
                Fatal(ex);
                return;
            }
			IncludeComponentTemplate();
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
				catch {}
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

            ParamsDefinition.Add("ShowFolders", new BXParamYesNo(GetMessageRaw("Param.ShowFolders"), true, mainCategory));
            ParamsDefinition.Add("FolderUrlTemplate", new BXParamText(GetMessageRaw("Param.FolderUrlTemplate"), "viewfolders.aspx?folder=#FolderId#", urlCategory));

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

            ParamsDefinition.Add("IndexUrl", new BXParamText(GetMessageRaw("Param.IndexUrl"), "viewtopics.aspx", urlCategory));
            ParamsDefinition.Add("FoldersUrl", new BXParamText(GetMessageRaw("Param.FoldersUrl"), "viewfolders.aspx", urlCategory));
            ParamsDefinition.Add("NewTopicUrl", new BXParamText(GetMessageRaw("Param.NewTopicUrl"), "viewtopic.aspx?act=new", urlCategory));

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

        #region NESTED CLASSES

        public enum ErrorCode
        {
            None,
            FatalException,
            FatalErrorUnknown,
            FatalComponentNotExecuted,
            Unauthorized,
            UnauthorizedRead
        }

        public class MenuItem
        {
            private string href;
            private string titleHtml;
            private string className;
            private string tooltipHtml;

            public string TooltipHtml
            {
                get
                {
                    return tooltipHtml;
                }
                internal set
                {
                    tooltipHtml = value;
                }
            }
            public string ClassName
            {
                get
                {
                    return className;
                }
                internal set
                {
                    className = value;
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
            public string Href
            {
                get
                {
                    return href;
                }
                internal set
                {
                    href = value;
                }
            }


            internal MenuItem(string title, string url, string cssClass, string tooltip)
            {
                titleHtml = title != null ? HttpUtility.HtmlEncode(title) : null;
                href = url != null ? HttpUtility.HtmlEncode(url) : null;
                className = cssClass != null ? HttpUtility.HtmlEncode(cssClass) : null;
                tooltipHtml = tooltip != null ? HttpUtility.HtmlEncode(tooltip) : null;
            }
        }

        public class FolderInfo
        {
            BXPrivateMessageFolder folder;
            string href;

            public BXPrivateMessageFolder Folder
            {
                get
                {
                    return folder;
                }
                internal set
                {
                    folder = value;
                }
            }

            public string Href
            {
                get
                {
                    return href;
                }
                set
                {
                    href = value;
                }
            }
            
        }

        #endregion
    }

	public class PrivateMessagesMenuTemplate : BXComponentTemplate<PrivateMessagesMenuComponent>
    {

        protected override void Render(HtmlTextWriter writer)
        {
            StartWidth = "100%";
            base.Render(writer);
        }
    }
}
