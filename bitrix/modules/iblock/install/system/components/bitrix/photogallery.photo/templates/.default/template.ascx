<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_photo_templates__default_template" EnableViewState="false" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery.photo/component.ascx" %>
           
<div id = "dMessage" class="notetext" runat="server" visible = "false">
</div>
          
<a href="<%=  Component.BackUrl %>" EnableAjax="true"><%= GetMessage("Kernel.Back") %></a> 
<% if(Component.Photo!=null){ %>
<% if (Component.ModifyElements)
   { %>
        <% if (Component.Photo != null)
        { %>
            | <a enableajax="true" href="<%= Component.EditUrl %>"><%= GetMessage("Kernel.Edit") %></a>
            | <asp:LinkButton runat="server" ID="lButtonSetCover" Text="<%$ Loc:SetCover %>" OnClick="lbSetCover_Click" Visible="false"><%= GetMessage("SetCover") %></asp:LinkButton>
            | <asp:LinkButton runat="server" ID="lButtonDelete" Text="<%$ Loc:Delete %>" OnClick="lbDelete_Click" Visible="false"><%= GetMessage("Delete") %></asp:LinkButton>
      <%}%>
<%}%>

<script>
    var fileBottomNavCloseImage = "<%= Bitrix.BXUri.ToRelativeUri(AppRelativeTemplateSourceDirectory) %>images/close.gif";
    var fileLoadingImage = "<%= Bitrix.BXUri.ToRelativeUri(AppRelativeTemplateSourceDirectory) %>images/loading.gif";
</script>

<div class="photo-detail">
	<div class="photo-detail-image">
		<div class="photo-detail-photo">
			<div class="photo-detail-img">
					<img border="0" width:"<%=Component.PhotoWidth%>px" height:"<%=Component.PhotoHeight%>px" hspace="0"  vspace="0"  src="<%= Component.PhotoUrl %>">
			</div>
		</div>
	</div>
	<div class="photo-photo-info" id="photo_text_description">
		<div id="photo_title" class="photo-photo-name"><%= Component.PhotoTitle %></div>
		<div id="photo_date" class="photo-photo-date"><%= Component.Photo.CreateDate.ToString("d") %></div>
		<div class="photo-photo-rating">	
	            <asp:PlaceHolder ID="Voting" runat="server"></asp:PlaceHolder>
		</div>
		<div id="photo_description" class="photo-photo-description"><%=Component.Description%></div>


		<div class="photo-controls photo-controls-photo">
			<noindex>
			</noindex>
		</div> 
	</div>
	<div class="empty-clear"></div>
</div>

<bx:IncludeComponent 
    runat="server" 
    id="Albums" 
    ComponentName="bitrix:photogallery.album" 
    Template="slider" 
    CacheMode="<%$ Parameters:CacheMode %>" 
    CacheDuration="<%$ Parameters:CacheDuration %>" 
    EnableSef="<%$ Parameters:EnableSef %>"
	SefFolder="<%$ Parameters:SefFolder %>" 
	EnableAjax="<%$ Parameters:EnableAjax %>"
	
	AlbumId="<%$ Results:AlbumId %>"
	PageId="<%$ Request:page %>"
	PageShowAll="<%$ Results:PageShowAll %>"
	
	GalleryRoot="<%$ Results:GalleryRoot %>"
	
	UrlTemplateAlbum="<%$ Parameters:UrlTemplateAlbum %>"
	UrlTemplateAlbumPage="<%$ Parameters:UrlTemplateAlbumPage %>"
	UrlTemplateAlbumShowAll="<%$ Results:UrlTemplateAlbumshowAll %>"
	UrlTemplateAlbumEdit="<%$ Results:UrlTemplateAlbumEdit %>"
	UrlTemplateAlbumAdd="<%$ Results:UrlTemplateAlbumAdd %>"
	UrlTemplatePhoto="<%$ Parameters:UrlTemplatePhoto %>"
	UrlTemplatePhotoEdit="<%$ Results:UrlTemplatePhotoEdit %>"
	UrlTemplateUpload="<%$ Results:UrlTemplateUpload %>"
	PhotoId = "<%$ Results:PhotoId %>"
	RecordsPerPage="4"
 />
<asp:PlaceHolder ID="Comments" runat="server"></asp:PlaceHolder>
<% } else {%>
  <h1><%= GetMessage("PhotoNotFound") %></h1>  
<%} %>