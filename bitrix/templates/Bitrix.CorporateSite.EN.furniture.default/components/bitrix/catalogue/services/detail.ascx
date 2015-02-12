<%@ Reference VirtualPath="~/bitrix/components/bitrix/catalogue/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>
<script runat="server">
    protected override void OnInit(EventArgs e)
    {
        CatalogueComponent catalogueComponent = (CatalogueComponent)Component;
        if (catalogueComponent.AllowComments && Bitrix.Modules.BXModuleManager.IsModuleInstalled("Forum"))
            Comments.Controls.Add(LoadControl("detail-comments.ascx"));
        
        base.OnInit(e);
    }
</script>
<bx:IncludeComponent runat="server"
	id="CatalogueElementDetail" 
	ComponentName="bitrix:catalogue.element.detail"
	Template=".default"	
	ElementId="<%$ Results:ElementId %>"
	ElementCode="<%$ Results:ElementCode %>"
	SectionId="<%$ Results:SectionId %>" 
	SectionCode="<%$ Results:SectionCode %>" 
	Properties="<%$ Parameters:DetailProperties %>"
	Template_BackUrlTemplate="<%$ Results:SectionElementListUrl %>"
/> 
<asp:PlaceHolder runat="server" ID="Comments" />