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
else if (Component.Items == null)
   return;
                                                           
	                                                                 
%>
<% if (!String.IsNullOrEmpty(Component.SectionName)){ %>
<h2><%=Component.SectionName %></h2>
<%} %>
    <table cellspacing="0" class="data-table">
        	<tr>
				<th></th>
				<th><%= GetMessageRaw("Position") %></th>
				<th><%= GetMessageRaw("Education") %></th>
			</tr>
<%
int i;
for(i =0; i < Component.Items.Count;i++)	
{
    CatalogueElementListComponent.ElementListItem listItem = Component.Items[i];
	%>
	<% string itemContainerId = GetItemContainerClientID(listItem.ElementId); %>
    	<tr id="<%= itemContainerId %>" <%=( i % 2 == 0 ? "class=\"alt-row\"":"") %> >
			<td>
	            <%=listItem.Element.Name %>
				<% RenderElementToolbar(listItem.Element, itemContainerId); %>
	        </td>
	        <td>
	            <%=listItem.Element.CustomPublicValues.Get<string>("TITLE", "") %>
	        </td>
	        <td>
	            <%=listItem.Element.CustomPublicValues.Get<string>("EDUCATION", "") %>

	        </td>
    </tr> 
<%} %>

</table>
