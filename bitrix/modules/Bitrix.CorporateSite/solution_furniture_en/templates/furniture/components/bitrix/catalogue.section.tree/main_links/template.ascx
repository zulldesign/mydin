<%@ Reference Control="~/bitrix/components/bitrix/catalogue.section.tree/component.ascx"%>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.IBlock.Components.CatalogueSectionTreeTemplate"%>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<% 
if (Component.TreeItems == null)
	return;
%>

<div class="product-list">
<h3><%=GetMessage("Goods") %></h3>

<%

int currentDepthLevel = 0;
foreach (CatalogueSectionTreeComponent.SectionTreeItem treeItem in Component.TreeItems)
{
    if (treeItem.Section.DepthLevel==1)
    {
            
            %>
            <div class="product">
                <div class="product-overlay"></div>
                <div class="product-image" style="<%=(treeItem.Section.Image != null) ? "background-image: url(" + treeItem.Section.Image.FilePath + ")" :"" %>"></div>
                <a class="product-desc" href="<%= treeItem.SectionDetailUrl %>">
                    <b><%=treeItem.Section.Name %></b>
                    <p><%=treeItem.Section.Description %></p>
                </a>
            </div>
            <%
        
    }
}
	%>
</div>
<script runat="server">
	protected override void PreLoadTemplateDefinition(Bitrix.Components.BXParametersDefinition def)
	{
		def["Template_RootTitle"] = new Bitrix.Components.BXParamText(GetMessageRaw("Param.RootTitle"), "", Bitrix.Components.BXCategory.AdditionalSettings);
		def["Template_RootUrl"] = new Bitrix.Components.BXParamText(GetMessageRaw("Param.RootUrl"), "", Bitrix.Components.BXCategory.AdditionalSettings);
	}
</script>