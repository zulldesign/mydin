<%@ Reference Control="~/bitrix/components/bitrix/iblock.hierarchy/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.IBlock.Components.IBlockHierarchyTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.IO" %>
<% 
if ( Component.PagingShow && Component.PagingPosition!="bottom" ) {%>
<div class="news-list-pager">
		<bx:IncludeComponent 
		    runat="server" 
		    ID="HeaderPager" 
		    ComponentName="bitrix:system.pager" 
		    CurrentPosition="top" 
		    Template="<%$ 
		    Parameters:PagingTemplate 
		    %>"/>
	</div><br />
	<% }%>
<%
	foreach (Action a in GetActions(false))
		switch (a.Type)
		{
			case ActionType.SectionStart:
				%>
<h3><a href="<%= a.Section.Href %>" ><%= a.Section.Data.Name %></a></h3>
				<%
				break;
			case ActionType.ElementsStart:
				%>
<table>
				<%
				break;
			case ActionType.Element:
				%>
		<% var elementContainerId = GetItemContainerClientID(a.Element.Data.Id); %>
	<tr id="<%= elementContainerId %>">
		<td class="Image">
			<%
				BXFile image = null;
				if (a.Element.Data.PreviewImageId >= 0)
					image = a.Element.Data.PreviewImage;
				if (image == null && a.Element.Data.DetailImageId >= 0)
					image = a.Element.Data.DetailImage;
				if(image != null)
				{
					%><img style="border: none 0px; width: 150px;" src="<%= Encode(image.FilePath) %>" alt="" /><%
				}
			%>
		</td>
		<td class="Text">
			<b><%= a.Element.Data.Name %></b><br />
			<%= a.Element.Data.PreviewText %><br />
			<a href="<%= a.Element.Href %>" ><%= GetMessage("Title.LinkMessage") %></a>
			<% RenderElementToolbar(a.Element.Data, elementContainerId); %>
		</td>
	</tr>
				<%			
				break;
			case ActionType.ElementsEnd:
				%>
</table>
				<%
				break;
		}
%>
<% 
if ( Component.PagingShow && Component.PagingPosition!="top" ) {%>
<div class="news-list-pager">
		<bx:IncludeComponent 
		    runat="server" 
		    ID="FooterPager" 
		    ComponentName="bitrix:system.pager" 
		    CurrentPosition="bottom" 
		    Template="<%$ 
		    Parameters:PagingTemplate 
		    %>"/>
	</div><br />
	<% }%>
