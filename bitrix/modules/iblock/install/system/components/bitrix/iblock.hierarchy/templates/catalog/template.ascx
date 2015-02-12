<%@ Reference Control="~/bitrix/components/bitrix/iblock.hierarchy/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.IBlock.Components.IBlockHierarchyTemplate" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.IO" %>
<%
	const int cols = 3;
%>
<table class="iblock-hierarchy-catalog">
	<colgroup>
		<% for (int i = 0; i < cols; i++) { %>
		<col width="<%=  string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{0:.##}", 100.0/cols) %>%" />
		<% } %>
	</colgroup>
	<tbody>
	<%
for (int i = 0; i < Component.Sections.Count; i++)
{	
	IBlockHierarchyComponent.Section section = Component.Sections[i];
	if (i % cols == 0)
	{ 
		%><tr><% 
	}
	%>
	<td>
		<h3>
			<img height="16" width="16" alt="<%= section.Data.Name %>" class="icon" src="<%= section.Data.Image != null ? section.Data.Image.FilePath : "" %>" />
			<a href="<%= section.Href %>"><%= section.Data.Name %></a>
		</h3>
		<p>	
	<%
	for (int j = 0; j < section.Sections.Count; j++)
	{
		IBlockHierarchyComponent.Section sub = section.Sections[j];
		if (j != 0)
		{
			%>, <%
		}
		%><a href="<%= sub.Href %>"><%= sub.Data.Name %></a><%
	}
	%>
		</p>
	</td>
	<%
	if (i % cols == cols - 1)
	{ 
		%></tr><% 
	}
}
		       	
int count = Component.Sections.Count;  	
while (count % cols != 0)
{
	%><td> </td><%
	if (count % cols == cols - 1)
	{ 
		%></tr><% 
	}
	count++;
}
	%>
	</tbody>
</table>
