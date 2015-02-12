<%@ Page Language="C#" AutoEventWireup="false" CodeFile="EditComponentParameters.aspx.cs" Inherits="bitrix_dialogs_EditComponentParameters" %>
<html>
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <bx:BXPageAsDialogBehaviour runat="server" ID="Behaviour" UseStandardStyles="true" OnSave="Behaviour_Save" />
        <table cellspacing="0" class="bx-width100"><tbody>
        <% foreach (Bitrix.Components.BXCategory componentParameterCategory in ComponentParameterCategories)
           {
               string sectionId = HttpUtility.HtmlAttributeEncode(string.Concat("paramsection_", componentParameterCategory.Code)),
               sectionExpanderId = HttpUtility.HtmlAttributeEncode(string.Concat("paramsection_expander_", componentParameterCategory.Code));
               %>
            <tr id="<%= sectionId %>" class="section">
                <td colspan="4">
                    <table cellspacing="0"><tbody>
                        <tr>
                          <td>
                            <%--<a id="<%= sectionExpanderId %>" class="bx-popup-sign bx-popup-minus" title="<%= GetMessage("ToolTip.ExpandCollapseSection") %>" onclick='var expand = this.className == "bx-popup-sign bx-popup-plus"; if(!Bitrix.ComponentParametersEditor.getInstance().expandSection("<%= sectionId %>", expand)) return; this.className = expand ? "bx-popup-sign bx-popup-minus": "bx-popup-sign bx-popup-plus";' href="javascript:void(0)"></a>--%>
                            <a id="<%= sectionExpanderId %>" class="bx-popup-sign bx-popup-minus" title="<%= GetMessage("ToolTip.ExpandCollapseSection") %>" onclick="var expand = this.className == 'bx-popup-sign bx-popup-plus'; Bitrix.ComponentParametersEditor.getInstance().expandSection('<%= componentParameterCategory.Code %>', expand);" href="javascript:void(0)"></a>                            
                          </td>
                          <td><%= HttpUtility.HtmlEncode(componentParameterCategory.Title) %></td>  
                        </tr>
                    </tbody></table>
                </td>
            </tr>
            <% Bitrix.Components.BXParametersDefinition componentParametersDefinition = GetComponentParametersDefinitionByCategory(componentParameterCategory);
            if (componentParametersDefinition.Keys.Count > 0){%>
            <%   foreach (string componentParamKey in componentParametersDefinition.Keys){
                     Bitrix.Components.BXParam componentParam = componentParametersDefinition[componentParamKey];%>
            <%= RenderComponentRow(componentParam, componentParamKey)%>
            <%   } %>
            <% } %>
            <% else{ %>
               <tr>
                    <td><%= GetMessage("NoData") %></td>
               </tr>               
            <% }%>  
            <tr class="empty">
                <td colspan="2">
                    <div class="empty"></div>
                </td>
            </tr> 
            <script type="text/javascript">
                Bitrix.ComponentParametersEditor.getInstance().setSection(Bitrix.ComponentParameterSection.create("<%= componentParameterCategory.Code %>", "<%= sectionId %>", "<%= sectionExpanderId %>"));
            </script>                     
         <%} %>        
        </tbody></table>
        <% componentParameters.Visible = false; %>
        <asp:PlaceHolder runat="server" ID="componentParameters"></asp:PlaceHolder>
    </form>
</body>
</html>
