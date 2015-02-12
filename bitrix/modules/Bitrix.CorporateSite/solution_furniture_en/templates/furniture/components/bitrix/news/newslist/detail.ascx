<%@ Reference VirtualPath="~/bitrix/components/bitrix/news/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>
<script runat="server">
    protected override void OnInit(EventArgs e)
    {
        News newsComponent = (News)Component;
        if (newsComponent.AllowComments && Bitrix.Modules.BXModuleManager.IsModuleInstalled("Forum"))
            Comments.Controls.Add(LoadControl("detail-comments.ascx"));
        
        base.OnInit(e);
    }
</script>
<bx:IncludeComponent 
	runat="server"
	ID="NewsDetail" 
	ComponentName="bitrix:news.detail"
	CacheMode="<%$ Parameters:CacheMode %>"
	PropertyCode="<%$ Parameters:DetailPropertyCode %>"
	ElementId="<%$ Results:ElementId %>"
	IBlockUrl="<%$ Results:UrlTemplatesNews %>"
	SectionUrl="<%$ Results:UrlTemplatesSection %>"  
	ShowPreviewText="<%$ Parameters:DetailShowPreviewText%>"
	ShowTitle="<%$ Parameters:DetailShowTitle%>"
	SetTitle="<%$ Parameters:DetailSetTitle%>"
	ShowDate="<%$ Parameters:DetailShowDate%>"
	ActiveDateFormat="<%$ Parameters:DetailActiveDateFormat%>"
	ShowPreviewPicture="<%$ Parameters:DetailShowPreviewPicture%>"
	ShowDetailPicture="<%$ Parameters:DetailShowDetailPicture%>"
	/>	
<asp:PlaceHolder runat="server" ID="Comments" />
