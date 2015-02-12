<%@ Reference Control="~/bitrix/components/bitrix/forum.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumListTemplate" %>
<%@ Import Namespace="Bitrix.Components" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>

<%
int itemIdex = 0;
bool firstCategory = true;
int? categoryId = null;
           
    
System.Collections.Generic.List<int> currentForumIds = Component.Parameters.GetListInt("CurrentForumId");

%>

<select id="<%= Component.ID %>$forumId" name="<%= Component.ID %>$forumId" <% if ( Parameters.GetBool("Template_IsMultiple") ){ %>multiple="multiple" size="6" <%} %> >
<option value="0" <% if (currentForumIds.Contains(0)) {%>class="forum-option-selected" selected="selected"<%} %>><%=GetMessage("PostList.PostFilter.AllForums")%></option>
			<%
	    	
					
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
		<option <% if (currentForumIds.Contains(item.Forum.Id)) {%>class="forum-option-selected" selected="selected"<%} %> value="<%= item.ForumId %>"><%= item.Forum.Name %></option>
<%
}
if (item != null && item.Category != null) {%>
	</optgroup>
<%} %>								
</select>

<script runat="server">
    protected override void PreLoadTemplateDefinition(Bitrix.Components.BXParametersDefinition def)
    {
        BXCategory urlCategory = BXCategory.UrlSettings;
        def.Add(
              "Template_IsMultiple",
              new BXParamYesNo(
                  GetMessage(""),
                  true,
                  urlCategory
              )
          );
    }
</script>