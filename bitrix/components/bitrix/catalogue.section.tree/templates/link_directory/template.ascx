<%@ Reference Control="~/bitrix/components/bitrix/catalogue.section.tree/component.ascx"%>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.IBlock.Components.CatalogueSectionTreeTemplate"%>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<% 
if (Component.TreeItems == null)
	return;
%>

<table class="catalogue-sections" width="100%" cellspacing="2">
<%

int rootItemCount = 0;
const int columnNumber = 2;

for (int rootIndex = 0; rootIndex < Component.TreeItems.Count; rootIndex++)
{
	CatalogueSectionTreeComponent.SectionTreeItem rootItem = Component.TreeItems[rootIndex];
	if (rootItem.Section.DepthLevel > 1)
		continue;

	if (rootItemCount % columnNumber == 0)
	{ 
		%><tr><% 
	}
	%>
	<td valign="top" width="<%= String.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{0:.##}", 100.0/columnNumber) %>">
		<h3><a href="<%= rootItem.SectionDetailUrl %>"><%= rootItem.Section.Name%></a></h3>
			
	<%
	for (int subIndex = rootIndex + 1; subIndex < Component.TreeItems.Count; subIndex++)
	{
		CatalogueSectionTreeComponent.SectionTreeItem subItem = Component.TreeItems[subIndex];

		if (subItem.Section.DepthLevel != rootItem.Section.DepthLevel + 1)
			break;

		if (subIndex != rootIndex + 1)
		{
			%>, <%
		}
		%><a href="<%= subItem.SectionDetailUrl %>"><%= subItem.Section.Name%></a><%
	}
	%>
	</td>
	<%
	if (rootItemCount % columnNumber == columnNumber - 1)
	{ 
		%></tr><% 
	}

	rootItemCount++;
}

while (rootItemCount % columnNumber != 0)
{
	%><td> </td><%
	if (rootItemCount % columnNumber == columnNumber - 1)
	{ 
		%></tr><% 
	}
	rootItemCount++;
}		     
	                                                         	
%>
</table>