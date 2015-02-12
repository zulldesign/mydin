<%@ Reference Control="~/bitrix/components/bitrix/catalogue.element.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.IBlock.Components.CatalogueElementListTemplate" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<script runat="server">
	//Template name
	public override string Title
	{
		get { return GetMessageRaw("TemplateTitle");}
	}
</script>

<%
if (Component.isErrorOccured)
{
	%><span class="errortext"><%= Component.errorMessage%></span><%
	return;
}
else if (Component.Items == null || Component.Items.Count==0)
   return;
%>


<%
Random rnd = new Random();
CatalogueElementListComponent.ElementListItem listItem = Component.Items[rnd.Next(Component.Items.Count)];
	%>
           <%= listItem.Element.CustomPublicValues.Get<string>("QUOTE","")%>
				<%
				if (listItem.Element.PreviewImage != null) 
				{
					%><p align="center">
						<img border="0" src="<%= listItem.Element.PreviewImage.FilePath %>" width="<%= Math.Min(listItem.Element.PreviewImage.Width, 100) %>" alt="<%= listItem.Element.PreviewImage.Description %>" enableajax="true" />
					</p><%
				}
                %>
				<% string itemContainerId = GetItemContainerClientID(listItem.ElementId); %>	
				<p id="<%= itemContainerId %>" align="right" style="color:#999; font-size: 0.85em;">
				<%=listItem.Element.CustomPublicValues.Get<string>("TITLE", "")%>
				<%
				if (!String.IsNullOrEmpty(listItem.ElementDetailUrl))
				{
					%><br/><a href="<%= listItem.ElementDetailUrl %>" enableajax="true"><%=listItem.Element.Name %></a><%
				}
				else
				{
					%><%=listItem.Element.Name %><%
				}
				%>
				<% RenderElementToolbar(listItem.Element, itemContainerId); %>
				</p>