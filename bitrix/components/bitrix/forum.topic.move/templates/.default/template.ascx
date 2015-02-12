<%@ Reference Control="~/bitrix/components/bitrix/forum.topic.move/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumTopicMoveTemplate" EnableViewState="false" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Forum" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>

<% 
	if (Component.FatalError != ForumTopicMoveComponent.ErrorCode.None)
	{ 
		%>
		<div class="forum-content">
		<div class="forum-note-box forum-note-error">
			<div class="forum-note-box-text"><%= Component.GetErrorHtml(Component.FatalError) %></div>
		</div>	
		</div>
		<% 
		return;
	}
%>

<div class="forum-content">

<div class="forum-header-box">
<div class="forum-header-title"><span><%= Component.Topics.Count == 1 ? GetMessage("Title.Singular") : GetMessage("Title.Plural")%></span></div>
</div>

<div class="forum-info-box forum-move-topics">
	<div class="forum-info-box-inner">
	<% for(int i = 0; i < Component.Topics.Count; i++) { %>
		<div class="forum-topic-move"><input name="<%= UniqueID %>$topics" type="checkbox" checked="checked" value="<%= Component.Topics[i].Topic.Id %>" /><a href="<%= Component.Topics[i].TopicHref %>"><%= Component.Topics[i].Topic.Name %></a></div>
	<% } %>
	
		<div class="forum-topic-move-buttons">
			<asp:Button ID="Move" runat="server" Text="<%$ LocRaw:ButtonText.MoveSelectedTopics %>" OnClick="MoveClick" />
			<span><%= GetMessage("To") %></span> 
			<select name="<%= UniqueID %>$forum">
			<% foreach(ForumTopicMoveComponent.ForumInfo f in Component.Forums) { %>
				<option value="<%= f.Forum.Id %>"><%= f.Forum.Name %></option>
			<% } %>
			</select>
		</div>

		<div class="forum-topic-move"><asp:CheckBox ID="Link" runat="server" Text="<%$ Loc:CheckBoxText.CreateLink %>" /></div>
	</div>
</div>

</div>

<script runat="server">
	private void MoveClick(object sender, EventArgs e)
	{
		if (Component.FatalError != ForumTopicMoveComponent.ErrorCode.None)
			return;
				
		string[] topics = Request.Form.GetValues(UniqueID + "$topics");
		if (topics == null || topics.Length == 0)
			return;
		
		List<int> ids = new List<int>();
		foreach (string s in topics)
		{
			int id;
			int k;
			
			if (!int.TryParse(s, out id) || id <= 0 || (k = ids.BinarySearch(id)) >= 0)
				continue;
			ids.Insert(~k, id);
		}
		
		int forum;
		if (!int.TryParse(Request.Form[UniqueID + "$forum"], out forum) || forum <= 0)
			return;
		
		Component.MoveTopics(ids, forum, Link.Checked);
	}
</script>