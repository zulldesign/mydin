<%@ Reference Control="~/bitrix/components/bitrix/iblock.list/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.IBlock.Components.IBlockListTemplate" %>

<% if (Component.IBlocks.Count == 0){ %>

<div class="notetext"><%=GetMessage("IBlocksNotFound") %></div>

<%
	return;
   } %>
<div class="catalog-section-list">
<% 
	var childCount = 0;
	var childsInRow = 3;

	
	foreach(var wrapper in Component.IBlocks)

   {
	   var image = wrapper.IBlock.Image != null ? Bitrix.Services.Image.BXImageUtility.GetResizedImage(wrapper.IBlock.Image, 75, 75) : null;

	   var imagePath = image != null ? image.GetUri() : String.Empty;
		 %>
	<div class="catalog-section<%=(image!= null ? "":" no-picture-mode")%>">
		<% if (image != null)
	 { %>
			<div class="catalog-section-image"><a href="<%=wrapper.IBlockDetailUrl %>"><img src="<%= imagePath %>" /></a></div>
		<%} %>
			<div class="catalog-section-info">
				<div class="catalog-section-title"><a href="<%=wrapper.IBlockDetailUrl %>" ><%= wrapper.IBlock.Name%></a></div>
				<div class="catalog-section-desc"><%= wrapper.IBlock.Description %></div>
				<% var childs = wrapper.Sections.FindAll(x=>x.Section.DepthLevel == 1); %>
				<% if(childs.Count > 0 ){ %>
				<div class="catalog-section-childs">
					<table cellspacing="0" class="catalog-section-childs">
					<% 
		
						var count = childs.Count % childsInRow == 0 ?
							childs.Count : childs.Count + childsInRow - childs.Count % childsInRow;

						for (var i = 0; i < count; i++ )
						{
							childCount++;
					%>
						<% if (childCount % childsInRow == 1)
		 {  %>
							 <tr>
							 <%} %>
							<td><% if ( i < childs.Count ){ %><a href="<%=wrapper.GetSectionUrl(childs[i].Section.Id) %>"><%=childs[i].Section.Name%></a></td>
							<%} %>
							<% if (childCount % childsInRow == childsInRow){  %>
							 </tr>
							 <%}%>
					<%} %>
				</table>
				</div>
				<%} %>
			</div>
	</div>
	<div class="catalog-section-separator"></div>
<%} %>
</div>