<%@ Reference Control="~/bitrix/components/bitrix/forum.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumListTemplate" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>

<div class="forum-content">
	<div class="forum-info-box forum-quick-jump">
		<div class="forum-info-box-inner">
		
			<select name="<%= UniqueID %>$forumId" onchange="document.getElementById('<%= RedirectButton.ClientID %>').click();">
			<%
			int itemIdex = 0;
			bool firstCategory = true;
			int? categoryId = null;

			int currentForumId = Component.Parameters.GetInt("CurrentForumId", 0);	
					
			ForumListComponent.ForumListItem item = null;		
			for (int i = 0; i < Component.ForumList.Count; i++)
			{
				item = Component.ForumList[i];
				if (categoryId.HasValue && categoryId != item.Forum.CategoryId)
				{
					itemIdex = 0;
					firstCategory = false;
				}

				itemIdex++;
				categoryId = item.Forum.CategoryId;%>

				<% if (itemIdex == 1) { %>
				<% if (!firstCategory) {%>
				</optgroup>
				<%} %>
				<optgroup label="<%= (item.Category == null || String.IsNullOrEmpty(item.Category.Name) ? GetMessage("DefaultCategoryTitle") : item.Category.Name)%>">
				<%} %>
					<option <% if (currentForumId == item.Forum.Id) {%>class="forum-option-selected" selected="selected"<%} %> value="<%= item.ForumId %>"><%= item.Forum.Name %></option>
			<%
			}
			if (item != null && item.Category != null) {%>
				</optgroup>
			<%} %>								
			</select>
			<asp:Button runat="server" ID="RedirectButton" Text="OK" OnClick="Redirect"/>
		</div>
	</div>
</div>
<script runat="server">
	void Redirect(Object sender, EventArgs e)
	{
		string[] idStrings = Request.Form.GetValues(UniqueID + "$forumId");
		if (idStrings == null || idStrings.Length == 0)
			return;

		int forumId;
		if (!int.TryParse(idStrings[0], out forumId) || forumId <= 0)
			return;

		int index = Component.ForumList.FindIndex(delegate(ForumListComponent.ForumListItem item) { return item.ForumId == forumId; });
		if (index >= 0 && !String.IsNullOrEmpty(Component.ForumList[index].TopicListHref))
		{
			string url = Component.ForumList[index].TopicListHref;
			if (url.StartsWith("?"))
			{
				UriBuilder uri = new UriBuilder(Bitrix.Services.BXSefUrlManager.CurrentUrl);
				uri.Query = url.Substring(1);
				url = uri.Uri.ToString();
			}
			Response.Redirect(url);
		}
	}
</script>
