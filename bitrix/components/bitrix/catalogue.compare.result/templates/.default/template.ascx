<%@ Reference Control="~/bitrix/components/bitrix/catalogue.compare.result/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.IBlock.Components.CatalogueCompareResultTemplate" %>
<script runat="server">
</script>
<div class="bx-catalogue-compare-result-container">
    <%if(Component.ElementDataList.Count == 0) {%>
        <div class="notetext">
            <%= string.Format(GetMessageRaw("Message.ListIsEmpty"), Component.GetElementListUrl()) %>
        </div>
        <% return;%>
    <%} %>
    <div class="bx-catalogue-compare-result-filter-container">
        <%if(Component.DisplayDifferenceOnly) {%>
            <%= string.Format(GetMessageRaw("FilterHtml.DisplayOnlyDefference"), "a", "class=\"bx-catalogue-compare-result-filter-diff-only-not-active\" href=\"" + HttpUtility.HtmlAttributeEncode(Component.GetDisplayDifferenceOnlyUrl(false)) + "\"", "label", "class=\"bx-catalogue-compare-result-filter-diff-only-active\"") %>    
        <%} %>
        <%else {%>
            <%= string.Format(GetMessageRaw("FilterHtml.DisplayOnlyDefference"), "label", "class=\"bx-catalogue-compare-result-filter-diff-only-active\"", "a", "class=\"bx-catalogue-compare-result-filter-diff-only-not-active\" href=\"" + HttpUtility.HtmlAttributeEncode(Component.GetDisplayDifferenceOnlyUrl(true)) + "\"") %>
        <%} %>
    </div>    
    <div class="bx-catalogue-compare-result-grid-container">
        <table class="bx-catalogue-compare-result-grid" cellspacing="0" cellpadding="0" <%= Component.ElementDataList.Count > 3 ? "style=\"width:" + (Component.ElementDataList.Count * 34 + 30).ToString() + "%;table-layout:fixed;\"" : ""%>">
            <tr>
                <td class="bx-catalogue-compare-result-grid-item-header"><%= GetMessage("ColumnName.Name")%></td>
                <% foreach (CatalogueCompareResultComponent.ElementData data in Component.ElementDataList) {%>
                <td class="bx-catalogue-compare-result-grid-header">
                    <a class="bx-catalogue-compare-result-delete-item" href="<%= data.GetDeleteUrl(false) %>" title="<%= GetMessage("ButtonHint.DeteleItem") %>"></a>
                    <a href="<%= data.DetailUrl %>" title="<%= GetMessage("LinkHint.Go2Item")%>" ><%= data.Name %></a>
                </td>    
                <%} %>
            </tr>
            <% foreach (string fieldName in Component.DisplayFields) {%>
                <% if(fieldName.StartsWith("-", StringComparison.Ordinal)) continue; %>
                <% string fieldNameUc = fieldName.ToUpperInvariant(); %>
                <tr>
                    <td>
                     <% if (string.Equals(fieldNameUc, "ID", StringComparison.Ordinal)) {%>
                        <span class="bx-catalogue-compare-result-grid-title"><%= GetMessage("FieldName.ID")%></span>
                     <%} %>
                     <% else if (string.Equals(fieldNameUc, "PREVIEWIMAGE", StringComparison.Ordinal)) {%>
                        <span class="bx-catalogue-compare-result-grid-title"><%= GetMessage("FieldName.PreviewImage")%></span>
                     <%} %>                      
                     <% else if (string.Equals(fieldNameUc, "PREVIEWTEXT", StringComparison.Ordinal)) {%>
                        <span class="bx-catalogue-compare-result-grid-title"><%= GetMessage("FieldName.PreviewText")%></span>
                     <%} %>                                          
                     <% else if (string.Equals(fieldNameUc, "DETAILIMAGE", StringComparison.Ordinal)) {%>
                        <span class="bx-catalogue-compare-result-grid-title"><%= GetMessage("FieldName.DetailImage")%></span>
                     <%} %>                      
                     <% else if (string.Equals(fieldNameUc, "DETAILTEXT", StringComparison.Ordinal)) {%>
                        <span class="bx-catalogue-compare-result-grid-title"><%= GetMessage("FieldName.DetailText")%></span>
                     <%} %>                     
                     <% else if (string.Equals(fieldNameUc, "ACTIVEFROMDATE", StringComparison.Ordinal)) {%>
                        <span class="bx-catalogue-compare-result-grid-title"><%= GetMessage("FieldName.ActiveFromDate")%></span>
                     <%} %>                      
                     <% else if (string.Equals(fieldNameUc, "ACTIVETODATE", StringComparison.Ordinal)) {%>
                        <span class="bx-catalogue-compare-result-grid-title"><%= GetMessage("FieldName.ActiveToDate")%></span>
                     <%} %>                                           
                    </td>
                <% foreach (CatalogueCompareResultComponent.ElementData data in Component.ElementDataList){%>
                     <td <%= Component.ElementDataList.Count > 0 && Component.ElementDataList.Count <= 3 ? "style=\"width:" + (70/Component.ElementDataList.Count).ToString() + "%;\"" : ""%>>
                     <% if (string.Equals(fieldNameUc, "ID", StringComparison.Ordinal)) {%>
                        <a href="<%= data.DetailUrl %>" title="<%= GetMessage("LinkHint.Go2Item")%>"><%= data.ID %></a>
                     <%} %>
                     <% else if (string.Equals(fieldNameUc, "PREVIEWIMAGE", StringComparison.Ordinal)) {%>
                        <a href="<%= data.DetailUrl %>" title="<%= GetMessage("LinkHint.Go2Item")%>"><img style="border:0 none;" src="<%= data.PreviewImageUrl%>" /></a>
                     <%} %>                      
                     <% else if (string.Equals(fieldNameUc, "PREVIEWTEXT", StringComparison.Ordinal)) {%>
                        <%= data.PreviewText%>
                     <%} %>                                          
                     <% else if (string.Equals(fieldNameUc, "DETAILIMAGE", StringComparison.Ordinal)) {%>
                        <a href="<%= data.DetailUrl %>" title="<%= GetMessage("LinkHint.Go2Item")%>"><img style="border:0 none;" src="<%= data.DetailImageUrl%>" /></a>
                     <%} %>                      
                     <% else if (string.Equals(fieldNameUc, "DETAILTEXT", StringComparison.Ordinal)) {%>
                        <%= data.DetailText%>
                     <%} %>                     
                     <% else if (string.Equals(fieldNameUc, "ACTIVEFROMDATE", StringComparison.Ordinal)) {%>
                        <%= data.ActiveFromDate%>
                     <%} %>                      
                     <% else if (string.Equals(fieldNameUc, "ACTIVETODATE", StringComparison.Ordinal)) {%>
                        <%= data.ActiveToDate%>
                     <%} %>
                     </td>                                   
                <%} %>                
                </tr>
            <%} %>
            <%if(Component.DisplayStockCatalogData && Component.PriceTypes.Count > 0) {%>
                <%foreach(CatalogueCompareResultComponent.CatalogPriceTypeInfo priceTypeInfo in Component.PriceTypeInfos) {%>
                <tr>
                    <td>
                        <span class="bx-catalogue-compare-result-grid-title"><%= HttpUtility.HtmlEncode(priceTypeInfo.Name) %></span>
                    </td>
                    <% foreach (CatalogueCompareResultComponent.ElementData data in Component.ElementDataList){%>
                    <td>
                        <% CatalogueCompareResultComponent.CatalogClientPriceInfo priceInfo = data.GetPriceInfoByPriceTypeId(priceTypeInfo.Id); %>
                        <% if(priceInfo != null) {%>
                            <span class="bx-catalogue-compare-result-grid-price"><%= priceInfo.SellingPriceHtml %></span>
                        <%} %>
                        <% else {%>
                            &mdash; 	
                        <%} %>
                    </td>
                    <%} %>                    
                </tr>                    
                <%} %>
            <%} %>
            <% foreach (string fieldName in Component.DisplayFields) {%>
                <% if(!fieldName.StartsWith("-", StringComparison.Ordinal)) continue; %>
                <% string fieldNameUc = fieldName.ToUpperInvariant(); %>
                <tr>
                    <td>
                        <span class="bx-catalogue-compare-result-grid-title"><%= Component.GetCustomPropertyDisplayName(fieldNameUc) %></span>
                    </td>
                    <% foreach (CatalogueCompareResultComponent.ElementData data in Component.ElementDataList){%>
                    <td>
                        <% CatalogueCompareResultComponent.ElementCustomPropertyData propertyData = data.GetPropertyData(fieldNameUc); %>
                        <% if(propertyData != null) {%>
                        <%= propertyData.GetHtml() %>
                        <%} %>                    
                    </td>
                    <%} %>
                </tr>
            <%} %>             
        </table>
    </div>
    <div class="bx-catalogue-compare-result-delete-all-items-container">
        <a href="<%= Component.GetDeleteAllElementsUrl(false) %>"><%= GetMessage("DeleteAllElements") %></a>
    </div>    
</div>
