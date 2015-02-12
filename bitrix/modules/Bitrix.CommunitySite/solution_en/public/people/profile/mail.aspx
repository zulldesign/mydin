<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="Personal Messages" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Configuration" %>


<script runat="server">

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		string query = Request.QueryString["user"];
		int userId = 0;
		if (String.IsNullOrEmpty(query) || !int.TryParse(query, out userId) || userId == 0 || Bitrix.Security.BXUser.GetById(userId) == null)
		{
			string path = BXOptionManager.GetOptionString("Bitrix.CommunitySite", "PeopleSefFolder", null, BXSite.Current.Id);
			if (path == null)		
			{
				Response.Redirect(BXSite.Current.DirectoryAbsolutePath);
				return;
			}
			
			if (!path.StartsWith("~/"))
			{
				if (path.StartsWith("/"))
					path = "~" + path;
				else 
					path = "~/" + path;
			}
			
			Response.Redirect(VirtualPathUtility.ToAbsolute(path));
			return;
		}
		
		int currentUserId = Bitrix.Security.BXIdentity.Current.Id;
		if (userId != currentUserId)
		{
			string path = VirtualPathUtility.ToAppRelative(HttpUtility.UrlDecode(Bitrix.Services.BXSefUrlManager.CurrentUrl.AbsolutePath));
			string template = BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserProfileUrlTemplate", null, BXSite.Current.Id);
			if (template == null)
			{
				Response.Redirect(BXSite.Current.DirectoryAbsolutePath);
				return;
			}
			string userTemplate = Bitrix.Services.Text.BXStringUtility.ReplaceIgnoreCase(template, "#UserId#", userId.ToString());
			if (currentUserId != 0 && path.StartsWith(userTemplate, StringComparison.InvariantCultureIgnoreCase))
			{
				string result = Bitrix.Services.Text.BXStringUtility.ReplaceIgnoreCase(template, "#UserId#", currentUserId.ToString());
				if (path.Length > userTemplate.Length)
					result += path.Substring(userTemplate.Length);
				Response.Redirect(VirtualPathUtility.ToAbsolute(result) + Bitrix.Services.BXSefUrlManager.CurrentUrl.Query);
				return;
			}			
			Response.Redirect(VirtualPathUtility.ToAbsolute(userTemplate));
		}

		Bitrix.DataTypes.BXParamsBag<object> replace = new Bitrix.DataTypes.BXParamsBag<object>();
		replace["userId"] = userId;
		
		PrivateMessages.Component.Parameters["EnableSEF"] = "True";

		var sefFolder = BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserMailUrlTemplate", BXSite.Current.DirectoryAbsolutePath, BXSite.Current.Id);
		PrivateMessages.Component.Parameters["SEFFolder"] = ResolveTemplateUrl(sefFolder.Substring(1), replace);
	}

	private string ResolveTemplateUrl(string template, Bitrix.DataTypes.BXParamsBag<object> parameters)
	{
		string url = (parameters != null) ? Bitrix.Services.BXSefUrlUtility.MakeLink(template, parameters, null) : template;
		if (url.StartsWith("~/"))
			url = ResolveUrl(url);
		return url;
	}
	
</script>

<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server"> 

<bx:IncludeComponent 
	id="PrivateMessages" 
	runat="server" 
	componentname="bitrix:pmessages" 
	template=".default" 
	ShowMenu="True" 
	MessageSendingInterval="30" 
	MaxReceiversCount="0" 
	MaxMessageCount="100" 
	AllowNotifyByEmail="True" 
	ThemeCssFilePath="~/bitrix/components/bitrix/forum/templates/.default/style.css" 
	ColorCssFilePath="<%$ Options:Bitrix.CommunitySite:ForumColorScheme %>" 
	UserProfileTemplate="<%$ Options:Bitrix.CommunitySite:UserProfileUrlTemplate %>" 
	EnableSEF="False" 
	ActionVariable="action" 
	TopicVariable="topic" 
	MessageVariable="msg" 
	FolderVariable="folder" 
	PageVariable="page" 
	ReceiversListVariable="receivers" 
	FoldersUrl="/folders/" 
	NewTopicTemplate="/new/" 
	TopicEditTemplate="/#i:TopicId#/edit/" 
	MessageEditTemplate="/#i:TopicId#/#i:MessageId#/edit/" 
	TopicListTemplate="/folders/#i:FolderId#/" 
	TopicListPageTemplate="/folders/#i:FolderId#/?page=#PageId#" 
	TopicReadTemplate="/#i:TopicId#/" 
	TopicReadPageTemplate="/#i:TopicId#/?page=#PageId#" 
	NewMessageTemplate="/#i:TopicId#/new/" 
	MessageReadTemplate="/#i:TopicId#/#i:MessageId#/##msg#MessageId#" 
	MessageQuoteTemplate="/#i:TopicId#/#i:MessageId#/quote/" 
	NewTopicWithReceiversTemplate="/new/#Receivers#/" 
	PagingAllow="True" 
	PagingMode="direct" 
	PagingTemplate="customizable" 
	PagingShowOne="True" 
	PagingRecordsPerPage="10" 
	PagingTitle="Pages" 
	PagingPosition="both" 
	PagingMaxPages="3" 
	PagingMinRecordsInverse="1" 
	SEFFolder="" 
/>
</asp:Content>