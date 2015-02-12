<%@ Reference Control="~/bitrix/components/bitrix/catalogue.section.tree/component.ascx"%>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.IBlock.Components.CatalogueSectionTreeTemplate"%>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<% 
if (Component.TreeItems == null)
	return;
%>

<div class="catalogue-section-tree">
<% 
	string title = Parameters.Get("Template_RootTitle");
	string url = Component.ResolveTemplateUrl(Parameters.Get("Template_RootUrl", string.Empty), null);
	bool root = !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(url);
%>
<% if (root) { %>
<ul>
<li>
	<% if (Component.SectionId == 0)  { %>
	<b><%= Encode(title) %></b>
	<% } else { %>
	<a href="<%= Encode(url) %>" enableajax="true"><%= Encode(title) %></a>
	<% } %>
<% } %>
<ul>
<%

int currentDepthLevel = 0;
foreach (CatalogueSectionTreeComponent.SectionTreeItem treeItem in Component.TreeItems)
{
	if (currentDepthLevel > 0 && treeItem.Section.DepthLevel < currentDepthLevel)
	{
		for (int i = 0; i < currentDepthLevel - treeItem.Section.DepthLevel; i++)
		{
			%></ul></li><%	
		}
	}

	if (treeItem.HasChildren)
	{
		if (Component.SectionId == treeItem.Section.Id)
		{
			%><li><b><%= treeItem.Section.Name%><%
			if (Component.ShowCounters)
			{  
				%>&nbsp;(<%= treeItem.ElementsCount%>)<%
			}
			%></b>
			<ul><%
		}
		else
		{
			%><li><a href="<%= treeItem.SectionDetailUrl  %>" enableajax="true"><%= treeItem.Section.Name%><%
			if (Component.ShowCounters)
			{  
				%>&nbsp;(<%= treeItem.ElementsCount%>)<%
			}
			%></a>
			<ul><%
		}
	}
	else
	{
		if (Component.SectionId == treeItem.Section.Id)
		{
			%><li><b><%= treeItem.Section.Name%></b><%
			if (Component.ShowCounters)
			{  
				%>&nbsp;(<%= treeItem.ElementsCount%>)<%
			}
			%></li><%
		}
		else
		{
			%><li><a href="<%= treeItem.SectionDetailUrl  %>" enableajax="true"><%= treeItem.Section.Name%><%
			if (Component.ShowCounters)
			{  
				%>&nbsp;(<%= treeItem.ElementsCount%>)<%
			}
			%></a></li><%
		}
	}

	currentDepthLevel = treeItem.Section.DepthLevel;
}
		                                                        	
//close last item tags
int sectionDepthLevel = Component.Section != null ? Component.Section.DepthLevel : 0;     	
if (currentDepthLevel > sectionDepthLevel + 1)
{
	for (int i = 0; i < currentDepthLevel - 1; i++)
	{
		%></ul></li><%		
	}
}
	                                                         	
%>
</ul>
<% if (root) { %>
</li>
</ul>
<% } %>
</div>

<script runat="server">
	protected override void PreLoadTemplateDefinition(Bitrix.Components.BXParametersDefinition def)
	{
		def["Template_RootTitle"] = new Bitrix.Components.BXParamText(GetMessageRaw("Param.RootTitle"), "", Bitrix.Components.BXCategory.AdditionalSettings);
		def["Template_RootUrl"] = new Bitrix.Components.BXParamText(GetMessageRaw("Param.RootUrl"), "", Bitrix.Components.BXCategory.AdditionalSettings);
	}
</script>