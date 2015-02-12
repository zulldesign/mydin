using System;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.Services;
using Bitrix.DataTypes;
using Bitrix.Components.Editor;
using System.Web;
using Bitrix.Security;
using System.IO;
using System.Collections.Generic;
namespace Bitrix.CommunicationUtility.Components
{
	public partial class PrivateMessagesComponent : BXComponent
	{
        static readonly string[] IncludeAreaExts = new string[] { ".ascx", ".html", ".txt", ".htm" };

        BXParamsBag<object> replace;

        bool cssFromForum;
        bool colorCssFromForum;

        protected string MessageVariable
        {
            get
            {
                return Parameters.GetString("MessageVariable", "");
            }
            set
            {
                Parameters["MessageVariable"] = value;
            }
        }

        protected string ReceiversListVariable
        {
            get
            {
                return Parameters.GetString("ReceiversListVariable", "");
            }
            set
            {
                Parameters["ReceiversListVariable"] = value;
            }
        }

        protected string TopicVariable
        {
            get
            {
                return Parameters.GetString("TopicVariable", "");
            }
            set
            {
                Parameters["TopicVariable"] = value;
            }
        }

        protected string ActionVariable
        {
            get
            {
                return Parameters.GetString("ActionVariable", "");
            }
            set
            {
                Parameters["ActionVariable"] = value;
            }
        }

        protected string FolderVariable
        {
            get
            {
                return Parameters.GetString("FolderVariable", "");
            }
            set
            {
                Parameters["FolderVariable"] = value;
            }
        }

        bool ShowMenu
        {
            get
            {
                return Parameters.GetBool("ShowMenu", true);
            }
            set
            {
                Parameters["ShowMenu"] = value.ToString();
            }
        }

        string PageVariable
        {
            get
            {
                return Parameters.GetString("PageVariable", "");
            }
            set
            {
                Parameters["PageVariable"] = value;
            }
        }

        string TopicReadTemplate
        {
            get
            {
                return Parameters.GetString("TopicReadTemplate", "");
            }
            set
            {
                Parameters["TopicReadUrlTemplate"] = value;
            }
        }

        protected override void PreLoadComponentDefinition()
        {

            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Group = new BXComponentGroup("pmessages", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);

            BXCategory mainCategory = BXCategory.Main;
            BXCategory urlCategory = BXCategory.UrlSettings;
            BXCategory sefCategory = BXCategory.Sef;

            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

            ParamsDefinition.Add(BXParametersDefinition.Sef);
            
            ParamsDefinition["PagingTemplate"].DefaultValue = "customizable";
            ParamsDefinition["PagingTitle"].DefaultValue = GetMessageRaw("DefaultPagingTitle");
            ParamsDefinition["PagingRecordsPerPage"].DefaultValue = "25";
            ParamsDefinition["PagingShowOne"].DefaultValue = "true";
            ParamsDefinition["PagingMaxPages"].DefaultValue = "3";

            ParamsDefinition["SEFFolder"] = new BXParamText(GetMessageRaw("Param.SEFFolder"), "/messages/", sefCategory);

            ParamsDefinition["ShowMenu"] = new BXParamYesNo(GetMessageRaw("Param.ShowMenu"), true, mainCategory);
            ParamsDefinition["MessageSendingInterval"] = new BXParamText(GetMessageRaw("Param.MessageSendingInterval"), "0", mainCategory);
            ParamsDefinition["MaxReceiversCount"] = new BXParamText(GetMessageRaw("Param.MaxReceiversCount"), "0", mainCategory);
            ParamsDefinition["MaxMessageCount"] = new BXParamText(GetMessageRaw("Param.MaxMessageCount"), "100", mainCategory);
            ParamsDefinition["AllowNotifyByEmail"] = new BXParamYesNo(GetMessageRaw("Param.AllowNotifyByEmail"), true, mainCategory);

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
            ParamsDefinition.Add(BXParametersDefinition.Sef);

            // Query string variable names
            ParamsDefinition["ActionVariable"] = new BXParamText(GetMessageRaw("Param.ActionVariable"), "action", sefCategory);
            ParamsDefinition["TopicVariable"] = new BXParamText(GetMessageRaw("Param.TopicVariable"), "topic", sefCategory);
            ParamsDefinition["MessageVariable"] = new BXParamText(GetMessageRaw("Param.MessageVariable"), "msg", sefCategory);
            ParamsDefinition["FolderVariable"] = new BXParamText(GetMessageRaw("Param.FolderVariable"), "folder", sefCategory);
            ParamsDefinition["PageVariable"] = new BXParamText(GetMessageRaw("Param.PageVariable"), "page", sefCategory);
            ParamsDefinition["ReceiversListVariable"] = new BXParamText(GetMessageRaw("Param.ReceiversListVariable"), "receivers", sefCategory);

            // Sef templates
            ParamsDefinition["FoldersUrl"] = new BXParamText(GetMessageRaw("Param.FoldersUrl"), "/folders/", sefCategory);
            ParamsDefinition["NewTopicTemplate"] = new BXParamText(GetMessageRaw("Param.NewTopicTemplate"), "/new/", sefCategory);

            ParamsDefinition["TopicEditTemplate"] = new BXParamText(GetMessageRaw("Param.TopicEditTemplate"), "/#i:TopicId#/edit/", sefCategory);
			ParamsDefinition["InviteUserTemplate"] = new BXParamText(GetMessageRaw("Param.InviteUserTemplate"), "/#i:TopicId#/invite/", sefCategory);
            ParamsDefinition["MessageEditTemplate"] = new BXParamText(GetMessageRaw("Param.MessageEditTemplate"), "/#i:TopicId#/#i:MessageId#/edit/", sefCategory);

            ParamsDefinition["TopicListTemplate"] = new BXParamText(GetMessageRaw("Param.TopicListTemplate"), "/folders/#i:FolderId#/", sefCategory);
            ParamsDefinition["TopicListPageTemplate"] = new BXParamText(GetMessageRaw("Param.TopicListPageTemplate"), "/folders/#i:FolderId#/?page=#PageId#", sefCategory);
            ParamsDefinition["TopicReadTemplate"] = new BXParamText(GetMessageRaw("Param.TopicReadTemplate"), "/#i:TopicId#/", sefCategory);
            ParamsDefinition["TopicReadPageTemplate"] = new BXParamText(GetMessageRaw("Param.TopicReadPageTemplate"), "/#i:TopicId#/?page=#PageId#", sefCategory);
            ParamsDefinition["NewMessageTemplate"] = new BXParamText(GetMessageRaw("Param.NewMessageTemplate"), "/#i:TopicId#/new/", sefCategory);
            ParamsDefinition["MessageReadTemplate"] = new BXParamText(GetMessageRaw("Param.MessageReadTemplate"), "/#i:TopicId#/#i:MessageId#/##msg#MessageId#", sefCategory);
            ParamsDefinition["MessageQuoteTemplate"] = new BXParamText(GetMessageRaw("Param.MessageQuoteTemplate"), "/#i:TopicId#/#i:MessageId#/quote/", sefCategory);
            ParamsDefinition["NewTopicWithReceiversTemplate"] = new BXParamText(GetMessageRaw("Param.NewTopicWithReceiversTemplate"), "/new/#Receivers#/", sefCategory);

            ParamsDefinition["UserProfileTemplate"] = new BXParamText(GetMessageRaw("Param.UserProfileTemplate"), "~/profile.aspx?user=#UserId#", urlCategory);
            
        }

        protected override void LoadComponentDefinition()
        {
            BXParametersDefinition.SetPaging(
                    ParamsDefinition, 
                    ClientID, 
                    BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

            ParamsDefinition["EnableSEF"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(ClientID, "EnableSEF", "Sef", "NonSef");
            ParamsDefinition["SEFFolder"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "SEFFolder", new string[] { "Sef" });

            foreach (string id in new string[] {
				"TopicVariable", "MessageVariable", 
				"ActionVariable",  "PageVariable",
                "FolderVariable","ReceiversListVariable"
			})
                ParamsDefinition[id].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, id, new string[] { "NonSef" });

            string[] ids = new string[] {
                "NewTopicTemplate", "TopicListPageTemplate", "TopicListTemplate", "NewMessageTemplate","FoldersUrl", 
				"TopicReadTemplate", "TopicReadPageTemplate", "MessageReadTemplate","MessageQuoteTemplate",
                "NewTopicWithReceiversTemplate","TopicEditTemplate","MessageEditTemplate"};

            foreach (string id in ids)
                ParamsDefinition[id].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, id, new string[] { "Sef" });

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

        private BXParamsBag<string> GetSefMap()
        {
            BXParamsBag<string> sefMap = new BXParamsBag<string>();
            if (Parameters.GetString("TopicReadPageTemplate", "")!="")
                sefMap.Add("topicread.page", Parameters.GetString("TopicReadPageTemplate", ""));
            if (Parameters.GetString("NewTopicTemplate", "") != "")
                sefMap.Add("form.topic", Parameters.GetString("NewTopicTemplate", ""));
            if (Parameters.GetString("NewTopicWithReceiversTemplate", "") != "")
                sefMap.Add("form.topicwithreceivers", Parameters.GetString("NewTopicWithReceiversTemplate", ""));
            if (Parameters.GetString("MessageEditTemplate", "") != "")
                sefMap.Add("form.editmessage", Parameters.GetString("MessageEditTemplate", ""));
            if (Parameters.GetString("TopicEditTemplate", "") != "")
                sefMap.Add("form.edittopic",Parameters.GetString("TopicEditTemplate",""));
			if (Parameters.GetString("InviteUserTemplate", "") != "")
				sefMap.Add("form.invite", Parameters.GetString("InviteUserTemplate", ""));
            if (Parameters.GetString("NewMessageTemplate", "") != "")
                sefMap.Add("form.msg", Parameters.GetString("NewMessageTemplate", ""));
            if (Parameters.GetString("MessageQuoteTemplate", "") != "")
                sefMap.Add("form.quote", Parameters.GetString("MessageQuoteTemplate", ""));
            if (Parameters.GetString("TopicListTemplate", "") != "")
                sefMap.Add("pmessages", Parameters.GetString("TopicListTemplate", ""));
            if (Parameters.GetString("TopicListPageTemplate", "") != "")
                sefMap.Add("pmessages.page", Parameters.GetString("TopicListPageTemplate", ""));
            if (Parameters.GetString("TopicReadTemplate", "") != "")
                sefMap.Add("topicread", Parameters.GetString("TopicReadTemplate", ""));
            if (Parameters.GetString("FoldersUrl", "") != "")
                sefMap.Add("folders", Parameters.GetString("FoldersUrl", ""));
            if (Parameters.GetString("MessageReadTemplate", "") != "")
            sefMap.Add("topicread.msg", Parameters.GetString("MessageReadTemplate", ""));

            return sefMap;
        }

        string PrepareSefPage()
        {
            BXParamsBag<string> sefMap = GetSefMap();
            string code = BXSefUrlUtility.MapVariable(Parameters.GetString("SEFFolder", ""), sefMap, ComponentCache, "pmessages", null, null);
            string page;
            string action;
            int folderId;
            
            int i = code.IndexOf('.');
            if (i == -1)
            {
                page = code;
                action = string.Empty;
            }
            else
            {
                page = code.Remove(i);
                action = code.Substring(i + 1);
            }
            ComponentCache["FolderUrlTemplate"] = ResolveTemplateUrl(ComponentCache["FolderUrlTemplate"].ToString(), replace);
            if (ComponentCache.TryGetValue<int>("FolderId",out folderId))
                replace.Add("FolderId", folderId);
            else replace.Add("FolderId","0");


            ComponentCache["MaxMessageCount"] = Parameters.Get("MaxMessageCount", 0);
            ComponentCache["MessageSendingInterval"] = Parameters.Get("MessageSendingInterval", 0);
            ComponentCache["NewTopicUrlTemplate"] = ResolveTemplateUrl(ComponentCache["NewTopicUrlTemplate"].ToString(), replace);
            ComponentCache["TopicListUrlTemplate"] = ResolveTemplateUrl(ComponentCache["TopicListUrlTemplate"].ToString(), replace);

            switch (page)
            {
                case "topicread":
                    ComponentCache["TopicReadPageUrlTemplate"] = ResolveTemplateUrl(ComponentCache["TopicReadPageUrlTemplate"].ToString(), replace);
                    break;
                case "pmessages":
                    ComponentCache["TopicListPageUrlTemplate"] = ResolveTemplateUrl(ComponentCache["TopicListPageUrlTemplate"].ToString(), replace);
                    ComponentCache["TopicReadUrlTemplate"] = ResolveTemplateUrl(ComponentCache["TopicReadUrlTemplate"].ToString(), replace);
                    break;
                case "form":
                    switch (action)
                    {
                        case "editmessage":
                        case "edittopic":
                            ComponentCache["FormMode"] = "Edit";
                            break;
						case "invite":
							ComponentCache["FormMode"] = "Invite";
							break;
                        case "topicwithreceivers":
                           // ComponentCache["Receivers"] = replace["Receivers"];
                            break;
                        case "quote":
                            ComponentCache["ParentMessageId"] = ComponentCache["MessageId"];
                            break;
                    }
                    break;
            }

            return page;
        }

        string PrepareSefMode()
        {
            string sefFolder = Parameters.GetString("SEFFolder");

            ComponentCache["IndexUrl"] = CombineLink(sefFolder, "");

            if (Parameters.GetString("FoldersUrl", "") != "")
                ComponentCache["FoldersUrl"] = CombineLink(sefFolder, Parameters.GetString("FoldersUrl"));
            else
                ComponentCache["FoldersUrl"] = "";

            ComponentCache["FolderUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicListTemplate"));
            if (Parameters.GetString("NewTopicTemplate", "") != "")
                ComponentCache["NewTopicUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("NewTopicTemplate"));
            else
                ComponentCache["NewTopicUrlTemplate"] = "";
            ComponentCache["NewMessageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("NewMessageTemplate"));
            ComponentCache["MessageQuoteUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("MessageQuoteTemplate"));
            ComponentCache["MessageReadUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("MessageReadTemplate"));
            ComponentCache["TopicListUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicListTemplate"));
            ComponentCache["TopicListPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicListPageTemplate"));
            ComponentCache["TopicReadUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicReadTemplate"));
            ComponentCache["TopicReadPageUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicReadPageTemplate"));
            ComponentCache["TopicEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("TopicEditTemplate"));
			ComponentCache["InviteUserUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("InviteUserTemplate"));
            ComponentCache["MessageEditUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("MessageEditTemplate"));

            string page = PrepareSefPage();

            return page;
        }

        string PrepareNormalMode()
        {
            string filePath = BXSefUrlManager.CurrentUrl.AbsolutePath;
			string messageVar = UrlEncode(MessageVariable);
			string topicVar = UrlEncode(TopicVariable);
            string actionVar = UrlEncode(ActionVariable);
            string folderVar = UrlEncode(FolderVariable);
            string pageVar = UrlEncode(PageVariable);

            ComponentCache["FoldersUrl"] = String.Format("{0}?{1}=folders", filePath, actionVar);
			ComponentCache["NewMessageUrlTemplate"] = String.Format("{0}?{1}=#TopicId#&{2}=new", filePath,topicVar, actionVar);
            ComponentCache["NewTopicUrlTemplate"] = String.Format("{0}?{1}=new", filePath,actionVar);
			ComponentCache["TopicReadUrlTemplate"] = String.Format("{0}?{1}=#TopicId#", filePath,topicVar);
			ComponentCache["MessageReadUrlTemplate"] = String.Format("{0}?{1}=#TopicId#&{2}=#MessageId###msg#MessageId#", filePath,topicVar,messageVar);
            ComponentCache["MessageEditUrlTemplate"] = String.Format("{0}?{1}=#TopicId#&{2}=#MessageId#&{3}=edit", filePath, topicVar, messageVar, actionVar);
            ComponentCache["TopicEditUrlTemplate"] = String.Format("{0}?{1}=#TopicId#&{2}=edit", filePath, topicVar, actionVar);
            ComponentCache["MessageQuoteUrlTemplate"] = String.Format("{0}?{1}=#TopicId#&{2}=#MessageId#&{3}=quote", filePath,topicVar,messageVar, actionVar);
            ComponentCache["TopicListUrlTemplate"] = String.Format("{0}?{1}=#FolderId#", filePath, folderVar);
            ComponentCache["TopicListPageUrlTemplate"] = String.Format("{0}?{1}=#FolderId#&{2}=#PageId#", filePath, folderVar, pageVar);
            ComponentCache["TopicReadPageUrlTemplate"] = String.Format("{0}?{1}=#TopicId#&{2}=#PageId#", filePath,topicVar, pageVar);
            ComponentCache["IndexUrl"] = String.Format("{0}?{1}=0", filePath, folderVar);
			ComponentCache["InviteUserUrlTemplate"] = String.Format("{0}?{1}=#TopicId#&{2}=invite", filePath, topicVar, actionVar);

            string page = PrepareNormalPage();

            return page;
        }

        string PrepareNormalPage()
        {
            int fId;
            int? topicId = ReadQueryInt(TopicVariable);
            int? messageId = ReadQueryInt(MessageVariable);
            int? pageId = ReadQueryInt(PageVariable);
            int? folderId = ReadQueryInt(FolderVariable);
            string receiversList = Request.QueryString[ReceiversListVariable];
            string action = (Request.QueryString[ActionVariable] ?? string.Empty).ToLowerInvariant();

            ComponentCache["PageId"] = pageId.ToString();
            ComponentCache["MessageId"] = messageId.ToString();
            ComponentCache["TopicId"] = topicId.ToString();
            ComponentCache["FolderId"] = folderId.ToString();
            ComponentCache["FolderUrlTemplate"] = ResolveTemplateUrl(ComponentCache["TopicListUrlTemplate"].ToString(), replace);
            if (ComponentCache.TryGetValue<int>("FolderId", out fId))
                replace.Add("FolderId", folderId);
            else replace.Add("FolderId", "0");

            ComponentCache["TopicReadUrlTemplate"] = ResolveTemplateUrl(ComponentCache["TopicReadUrlTemplate"].ToString(), replace);
            ComponentCache["NewTopicUrlTemplate"] = ResolveTemplateUrl(ComponentCache["NewTopicUrlTemplate"].ToString(), replace);
            ComponentCache["TopicReadPageUrlTemplate"] = ResolveTemplateUrl(ComponentCache["TopicReadPageUrlTemplate"].ToString(), replace);
            ComponentCache["TopicListPageUrlTemplate"] = ResolveTemplateUrl(ComponentCache["TopicListPageUrlTemplate"].ToString(), replace);
            ComponentCache["TopicListUrlTemplate"] = ResolveTemplateUrl(ComponentCache["TopicListUrlTemplate"].ToString(), replace);

            ComponentCache["MaxMessageCount"] = Parameters.Get("MaxMessageCount", 0);
            ComponentCache["MessageSendingInterval"] = Parameters.Get("MessageSendingInterval", 0);

            if (messageId != null)
            {
                switch (action)
                {
                    case "edit":
                        ComponentCache["FormMode"] = "Edit";
                        return "form";
                    case "quote":
                        ComponentCache["ParentMessageId"] = messageId;
                        ComponentCache["MessageId"] = String.Empty;
                        return "form";

                    default:
                        return "topicread";
                }
            }



            switch (action)
            {
                case "edit":
                    replace.Add("TopicId", topicId.ToString());
                    ComponentCache["RedirectUrl"] = ResolveTemplateUrl("{0}?{1}=#TopicId#&{2}=view", replace);
                    replace.Remove("TopicId");
                    ComponentCache["FormMode"] = "Edit";
                    return "form";
                case "new":
                    replace.Add("TopicId", topicId.ToString());
                    ComponentCache["RedirectUrl"] = ResolveTemplateUrl("{0}?{1}=#TopicId#&{2}=view", replace);
                    replace.Remove("TopicId");
                        if (!Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(receiversList))
                            ComponentCache["Receivers"] = receiversList;

                        return "form"; 
				case "invite":
						ComponentCache["FormMode"] = "Invite";
						return "form";
                case "folders":
                        return "folders";
            }
            if (topicId != null)
                return "topicread";

            return "pmessages";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            replace = new BXParamsBag<object>();
            ComponentCache["ShowMenu"] = ShowMenu;

            ComponentCache["UserPostsReadUrlTemplate"] = GetExternalUrl("UserPostsTemplate");
            ComponentCache["UserProfileUrlTemplate"] = GetExternalUrl("UserProfileTemplate");
            ComponentCache["MaxReceiversCount"] = Parameters["MaxReceiversCount"];
            

            string page = EnableSef ? PrepareSefMode() : PrepareNormalMode();

            IncludeComponentTemplate(page);
        }

        private int? ReadQueryInt(string param)
        {
            string[] values = Request.QueryString.GetValues(param);
            if (values == null || values.Length == 0)
                return null;
            int i;
            if (!int.TryParse(values[0], out i))
                return null;
            return i;
        }

        private string GetExternalUrlTemplate(string parameter)
        {
            string val = Parameters.GetString(parameter);
            return !BXStringUtility.IsNullOrTrimEmpty(val) ? val : null;
        }
        private string GetExternalUrl(string urlParameter)
        {
            string url;
            return ProcessInfoUrl(Parameters.GetString(urlParameter), out url) ? url : null;
        }
 
        private bool ProcessInfoUrl(string url, out string target)
        {
            if (BXStringUtility.IsNullOrTrimEmpty(url))
            {
                target = null;
                return false;
            }

            url = url.Trim();
            if (url.StartsWith("~/"))
            {
                bool appRel = false;
                try
                {
                    appRel = VirtualPathUtility.IsAppRelative(url);
                }
                catch
                {
                }
                if (appRel)
                {
                    string ext = VirtualPathUtility.GetExtension(url);
                    if (ext != null && Array.IndexOf(IncludeAreaExts, ext) != -1)
                    {
                        target = url;
                        return false;
                    }
                }
                url = ResolveUrl(url);
            }
            target = url;
            return true;
        }
		
	}

	public class PrivateMessagesTemplate : BXComponentTemplate<PrivateMessagesComponent>
	{
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            string css = Parameters.GetString("ThemeCssFilePath");
            if (!BXStringUtility.IsNullOrTrimEmpty(css))
            {
                try
                {
                    css = BXPath.ToVirtualRelativePath(css);
                    if (BXSecureIO.FileExists(css))
                        BXPage.RegisterStyle(css);
                }
                catch
                {
                }
            }

            css = Parameters.GetString("ColorCssFilePath");
            if (!BXStringUtility.IsNullOrTrimEmpty(css))
            {
                try
                {
                    css = BXPath.ToVirtualRelativePath(css);
                    if (BXSecureIO.FileExists(css))
                        BXPage.RegisterStyle(css);
                }
                catch
                {
                }
            }

        }
	}
}
