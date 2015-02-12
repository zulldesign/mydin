<%@ Reference Control="~/bitrix/components/bitrix/catalogue.compare.list/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.IBlock.Components.CatalogueCompareListTemplate" %>
<script runat="server">
</script>
<% if(Component.ElementDataList.Count == 0) return;%>
<div class="bx-catalogue-compare-list-container">
    <table class="bx-catalogue-compare-list-grid" cellspacing="0" cellpadding="0">
        <%foreach(CatalogueCompareListComponent.ElementData data in Component.ElementDataList) {%>
            <tr>
                <td><a href="<%= data.DetailUrl%>"><%= data.Name%></a></td>
                <td><a href="<%= data.GetDeleteUrl(false)%>" title="<%= GetMessage("LinkHint.DeleteItemFromList")%>"><%= GetMessage("LinkText.DeleteItemFromList")%></a></td>
            </tr>
        <%} %>
    </table>
    <div class="bx-catalogue-compare-list-actions">
        <input type="button" value="<%= GetMessage("ButtonText.Compare") %>" onclick="window.location.href='<%= HttpUtility.HtmlAttributeEncode(Component.CompareResultUrl()) %>';" />
    </div>
</div>
