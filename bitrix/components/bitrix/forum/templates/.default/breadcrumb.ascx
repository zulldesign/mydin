<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXControl" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.Forum" %>
<% if (Component == null) return; %>

<div class="forum-content">
	<div class="forum-breadcrumb<% if (!string.IsNullOrEmpty(CssPostfix)) { %> forum-breadcrumb-<%= CssPostfix %><% } %>">
		<% while (true) { %>
		<span class="forum-crumb-item forum-crumb-first<% if (Forum == null && String.IsNullOrEmpty(CustomCrumbTitle)) { %> forum-crumb-last<% } %>"><%= MakeUrl(RootUrlTemplate, RootTitle) %></span>
		<% if (Forum == null) break; %>
		<span class="forum-crumb-item<% if (Topic == null && String.IsNullOrEmpty(CustomCrumbTitle)) { %> forum-crumb-last<% } %>"><span>	»&#160;</span><%= MakeUrl(ForumUrlTemplate, Forum.TextEncoder.Decode(Forum.Name)) %></span>
		<% if (Topic == null) break; %>
		<span class="forum-crumb-item<% if (String.IsNullOrEmpty(CustomCrumbTitle)) { %> forum-crumb-last<% } %>"><span> »&#160;</span><%= MakeUrl(TopicUrlTemplate, Topic.TextEncoder.Decode(Topic.Name)) %></span>
		<%break; %>
		<% } %>
		
		<% if (!String.IsNullOrEmpty(CustomCrumbTitle)) { %>
		<span class="forum-crumb-item forum-crumb-last"><span> »&#160;</span><%= MakeUrl(CustomCrumbUrlTemplate, CustomCrumbTitle)%></span>
		<% } %>
	</div>
</div>

<script runat="server">
	private string rootTitle;
	public string RootTitle
	{
		get
		{
			return rootTitle;
		}
		set
		{
			rootTitle = value;
		}
	}
	private BXForum forum;
	public BXForum Forum
	{
		get
		{
			return forum;
		}
		set
		{
			forum = value;
		}
	}
	private BXForumTopic topic;
	public BXForumTopic Topic
	{
		get
		{
			return topic;
		}
		set
		{
			topic = value;
		}
	}
	private string cssPostfix;
	public string CssPostfix
	{
		get
		{
			return cssPostfix;
		}
		set
		{
			cssPostfix = value;
		}
	}
	private BXComponent component;
	public BXComponent Component
	{
		get
		{
			return component;
		}
		set
		{
			component = value;
		}
	}
	private string rootUrlTemplate;
	public string RootUrlTemplate
	{
		get
		{
			return rootUrlTemplate;
		}
		set
		{
			rootUrlTemplate = value;
		}
	}
	private string forumUrlTemplate;
	public string ForumUrlTemplate
	{
		get
		{
			return forumUrlTemplate;
		}
		set
		{
			forumUrlTemplate = value;
		}
	}
	private string topicUrlTemplate;
	public string TopicUrlTemplate
	{
		get
		{
			return topicUrlTemplate;
		}
		set
		{
			topicUrlTemplate = value;
		}
	}
	private int maxWordLength = 15;
	public int MaxWordLength
	{
		get
		{
			return maxWordLength;
		}
		set
		{
			maxWordLength = value;
		}
	}
	private string customCrumbUrlTemplate;
	public string CustomCrumbUrlTemplate
	{
		get
		{
			return customCrumbUrlTemplate;
		}
		set
		{
			customCrumbUrlTemplate = value;
		}
	}
	private string customCrumbTitle;
	public string CustomCrumbTitle
	{
		get
		{
			return customCrumbTitle;
		}
		set
		{
			customCrumbTitle = value;
		}
	}
	
	BXParamsBag<object> replace;
	public string MakeUrl(string template, string innerText)
	{
		if (replace == null)
		{

			replace = new BXParamsBag<object>();
			if (Forum != null)
				replace.Add("ForumId", Forum.Id);
			if (Topic != null)
				replace.Add("TopicId", Topic.Id);
		}
		string innerHtml = Bitrix.CommunicationUtility.BXWordBreakingProcessor.Break(innerText, MaxWordLength, true);
		if (string.IsNullOrEmpty(template))
			return innerHtml;
		return "<a href=\"" + Encode(Component.ResolveTemplateUrl(template, replace)) + "\">" + innerHtml + "</a>";
	}
</script>

