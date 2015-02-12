<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_album_templates_slider_template" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery.album/component.ascx" %>

<%@ Import Namespace="Bitrix.IBlock" %>
<%@ Import Namespace="Bitrix.Services" %>

<% 
	if (Component.IsComponentDesignMode && Component.IBlockId <= 0) 
	{ 
		%><%= string.Format(GetMessageRaw("FormatYouHaveToAdjustTheComponent"), Encode(Component.Title))%><%
	} 
%>

<style type="text/css">
	.photo-slider-container .photo-slider-item .photo-slider-thumb {
	height:<%=Component.Parameters.Get<int>("PreviewHeight",200)+20%>px;}
	.photo-slider-container
	{
	    height:<%=Component.Parameters.Get<int>("PreviewHeight",200)+30%>px;
	}

</style>

<script type="text/javascript">
window['__photo_result'] = <%= PicturesList(true) %>;
var oPhotoObjects = {min_slider_width:400,min_slider_height:400};

</script>


<script type="text/javascript">
function __photo_init_slider()
{
	if (window['BPCStretchSlider'] && window['BX'])
	{
		var data = <%=PicturesList(true) %>;
		var __slider = new BPCStretchSlider(
												data.start_number, 
												data.elements_count, 
												<%= Component.Parameters.Get<int>("PhotoId",0) %>,
												'<%= PathForAjaxGetQuery %>',
												data.recsPerPage
											);
		__slider.pack_id = '<%=ClientID %>'; 
		__slider.CreateSlider(data); 
		return true; 
	}
	window.setTimeout("__photo_init_slider('" + pack_id + "');", 70); 

}
window.setTimeout("__photo_init_slider();", 70); 

</script>

<div class="photo-photos photo-photos-slider">
<div class="photo-slider">
	<div class="photo-slider-inner">
		<div class="photo-slider-container">
			
			<span class="photo-prev-enabled" id="prev_<%=ClientID %>"></span>
			<div id="slider_window_<%=ClientID %>"  class="photo-slider-data"><div style="overflow:hidden;left:0px;white-space:nowrap;width:1000%;" class="photo-slider-data-list"></div></div>
			<span class="photo-next-enabled" id="next_<%=ClientID %>"></span>
		</div>
	</div>
</div>
</div>


