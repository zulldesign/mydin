<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_album_templates__default_template" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery.album/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/rating.vote/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<%@ Import Namespace="Bitrix.Services" %>

<% 
	if (Component.IsComponentDesignMode && Component.IBlockId <= 0) 
	{ 
		%><%= string.Format(GetMessageRaw("FormatYouHaveToAdjustTheComponent"), Encode(Component.Title))%><%
	} 
%>
<%
if (!String.IsNullOrEmpty(Component.errorMessage))
{ 
	%><span class="errortext"><%=Component.errorMessage%></span><br /><br /><%
}
	                                                                        	                                                                       
 %>   
<p>
<% if (Component.IsNested){ %>
<a href="<%= Component.BackUrl %>" enableajax="true">
    <%= GetMessage("BackLink") %>
</a>
<%} %>

<% if (Component.CanModify)
   { %>
<% if (Component.IsNested)
      { %>
|
<%} %>
<a enableajax="true" href="<%= Component.AddUrl %>">
    <%= GetMessage("AddAlbum") %>
</a>| <a href="<%= Component.UploadUrl %>"><%= GetMessage("UploadPhoto") %></a>
<% if (Component.AlbumItem != null)
   { %>
| <a enableajax="true" href="<%= Component.EditUrl %>"><%= GetMessage("Kernel.Edit") %></a> |
<asp:LinkButton runat="server" ID="lButtonDelete" Text="<%$ Loc:Delete %>" OnClick="lbDelete_Click" Visible="false"><%= GetMessage("Delete") %></asp:LinkButton>
<%}%>
<%} %>
</p>
<% if (Component.IsParent)
   { %>
<h4>
    <%= GetMessage("Legend.Albums") %>
</h4>
<%} %>
<div class="photo-page-main">
<% if (Component.Sections.Count > 0)
   {
	   
	   
	   %>

<ul class="photo-items-list photo-album-list">
    <%
	   
		string urlToNoImagePic = Component.Parameters.GetString("ColorCss", "")+"/images/album/cover_empty.gif";
		
		foreach (BXIBlockSection section in Component.SectionItems)
       {
		  

		  %>
    
	<li class="photo-album-item photo-album-active" id="photo_album_info_<%=section.Id %>">
		<div class="photo-item-cover-block-outside" >
			<div class="photo-item-cover-block-container" 	style="height: <%=Component.CoverHeight+16%>px; width: <%=Component.CoverWidth+40%>px;" >
				<div class="photo-item-cover-block-outer" 	style="height: <%=Component.CoverHeight+16%>px; width: <%=Component.CoverWidth+40%>px;" >
					<div class="photo-item-cover-block-inner" 	style="height: <%=Component.CoverHeight+16%>px; width: <%=Component.CoverWidth+40%>px;" >
						<div class="photo-item-cover-block-inside">
							<div class="photo-item-cover photo-album-avatar <%= Component.AlbumDescriptionDictionary[section].IsCoverEmpty ? "photo-album-avatar-empty" :"" %>" id="photo_album_cover_<%=section.Id %>" title="<%=Encode(section.Name)%>"
							onclick="window.location= '<%=Component.AlbumDescriptionDictionary[section].Url%>';" 
							style="<%= (!Component.AlbumDescriptionDictionary[section].IsCoverEmpty ? "background-image: url('"+  Component.AlbumDescriptionDictionary[section].CoverImageUrl+"')":"") %>; 
								height: <%=Component.CoverHeight%>px; width: <%=Component.CoverWidth%>px;" 
							<% if (Component.CanModify){ %>onmouseover="this.firstChild.style.display='block';"<%} %>><%  if (Component.CanModify)
																{ %><div class="photo-album-menu" onmouseout="this.style.display='none';" 
							style="display: none;" >
								<div class="photo-album-menu-substrate"></div>
									<div class="photo-album-menu-controls">
										<a rel="nofollow" href="<%=Component.AlbumDescriptionDictionary[section].EditUrl%>" class="photo-control-edit photo-control-album-edit" 
											 title="<%=GetMessage("EditAlbum") %>">
											<span><%=GetMessage("Edit") %></span>
										</a>
										<a rel="nofollow" href="<%=Bitrix.Services.BXSefUrlManager.CurrentUrl.AbsolutePath %>?del=<%=section.Id %>&target=album&<%=Bitrix.Security.BXCsrfToken.TokenKey %>=<%= Bitrix.Security.BXCsrfToken.GenerateToken() %>" 
											class="photo-control-drop photo-control-album-drop" onclick="if (!confirm('<%=GetMessage("DeleteAlbum") %>')){ jsUtils.PreventDefault(event); return false;}" title="<%=GetMessage("DeleteAlbum") %>">
											<span><%=GetMessage("Delete") %></span>
										</a>
									</div>
								</div>		
								<%} %>					
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
		<div class="photo-item-info-block-outside" style="width: <%=Component.CoverWidth+40%>px;">
			<div class="photo-item-info-block-container">
				<div class="photo-item-info-block-outer">
					<div class="photo-item-info-block-inner">
						<div class="photo-album-photos-top"><%= section.ElementsCount %>&nbsp<%=GetMessage("Photo") %></div>
						<div class="photo-album-name">
							<a href="<%= Component.AlbumDescriptionDictionary[section].Url %>" id="photo_album_name_<%=section.Id %>" 
							title="<%=Encode(section.Name)%>"><%=Encode(section.Name.Length > 15 ? section.Name.Substring(0,13)+"...":section.Name)%></a>
						</div>
						<div class="photo-album-description" id="photo_album_description_<%=section.Id %>"></div>
						<div class="photo-album-date"><span id="photo_album_date_<%=section.Id %>"><%=section.CreateDate.ToString("d") %></span></div>
						<div class="photo-album-photos"><%= section.ElementsCount %>&nbsp<%=GetMessage("Photo") %></div>
					</div>
				</div>
			</div>
		</div>
	</li>

    <% } %>
</ul>
<br clear="all" />

<%} %>
<% if (Component.Photos.Count>0)
   { %>

<% 
    if (Results.Get<bool>("PagingShow") && !"bottom".Equals(Parameters.Get<string>("PagingPosition"), StringComparison.InvariantCultureIgnoreCase))
    {  %>
<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" CurrentPosition="top"
    Template="<%$ Parameters:PagingTemplate %>" />
<br />
<% } %>


<div class="photo-controls photo-controls-photo-top">
	<div class="empty-clear"></div>
</div> 

<% for (int i =0; i< Component.PhotoItems.Count;i++)
      { 
var photo = Component.PhotoItems[i];


	    %>
<% if (!Component.PhotoDescriptionDictionary.ContainsKey(photo)) continue; %>

<% 
	   var fotoSize = FotoSize();
	    %>

<div class="photo-photo-item photo-photo-item-ascetic">
	<div class="photo-photo-item-ascetic-inner" style="width:<%=fotoSize%>px;
		height:<%=fotoSize%>px;overflow: hidden; position: relative;"><% if (Component.CanModify)
	 { %><input type="checkbox" id = "cb_photo_<%=photo.Id %>" 
	 onclick="var res = this.parentNode.parentNode; if (this.checked) {res.className += ' photo-photo-item-checked'} else {res.className = res.className.replace(/photo\-photo\-item\-checked/g, ' ').replace(/\s\s/g, ' ');}" 
	 style="position: absolute; top: 0pt; left: 0pt; z-index: 100;"/><% } %>
		<a class="photo-photo-item-ascetic-inner" onclick="window.SlideSlider.ShowItem(<%= i+1 %>); return false;" 
		style="width:<%=fotoSize%>px;
		height:<%=fotoSize%>px; display: block;" href="<%=Component.PhotoDescriptionDictionary[photo].DetailUrl %>"  id="photo_<%=photo.Id %>">
		<div style="margin-top:
			<%=Component.PhotoDescriptionDictionary[photo].ActualCoverHeight > fotoSize ? "-5.33%" : (50 - Component.PhotoDescriptionDictionary[photo].ActualCoverHeight*50/fotoSize).ToString()+"%" %>; 
			 margin-left:<% = Component.PhotoDescriptionDictionary[photo].ActualCoverHeight > fotoSize ? "-5.33%" : (50 - Component.PhotoDescriptionDictionary[photo].ActualCoverWidth*50/fotoSize).ToString()+"%" %>; 
			 text-align: left; position: static;"><img src="<%= Component.PhotoDescriptionDictionary[photo].Preview %>" 
		alt="<%= Encode(photo.Name) %>" title="<%= Encode(photo.Name) %>" vspace="0" width="<%=Component.PhotoDescriptionDictionary[photo].ActualCoverWidth%>px;" border="0" height="<%=Component.PhotoDescriptionDictionary[photo].ActualCoverHeight%>px;" hspace="0"></div>
		</a>
	</div>
</div>		

<% } %>
<br clear="all" />
<% if (Component.CanModify)
   { %>
   
   <noindex>
	<div class="photo-controls photo-controls-photo-bottom">
		<ul class="photo-controls">
			<li class="photo-control photo-control-first photo-control-photo-selectall">
				<span>
					<input type="checkbox" value="N" name="select_all" onclick="selectAllPhotos(this);" id="select_all"/>
					<label title="<%= GetMessage("SelectAllPhotos") %>" for="select_all"><%=GetMessage("SelectAll") %></label>
				</span>
			</li>
			<li class="photo-control photo-control-photo-drop">
				<span><a onclick="deletePhotos(event); return false;" href="#"><input type="hidden"><%=GetMessage("DeletePhotos")%></a></span>
			</li>
		</ul>
		<div class="empty-clear"></div>
	</div> 
	</noindex>
<br clear="all" />
<% } %>
<% 
    if (Results.Get<bool>("PagingShow") && !"top".Equals(Parameters.Get<string>("PagingPosition"), StringComparison.InvariantCultureIgnoreCase))
    {  %>
    <br />
<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" CurrentPosition="bottom"
    Template="<%$ Parameters:PagingTemplate %>" />
<%	}
} %>
</div>
<asp:PlaceHolder runat="server" ID="Voting"></asp:PlaceHolder>
<script type="text/javascript">

function deletePhotos(evt)
{
	var confirmText = '<%=GetMessageJS("ConfirmText.DoYouReallyWantRemovePhotos") %>';
	
	var inputs = document.getElementsByTagName('INPUT');
	var photoIds = [];
	for ( var i =0; i< inputs.length;i++ )
		if ( inputs[i].id.match(/cb_photo\_(\d+)/gi) && inputs[i].checked)
			photoIds.push(inputs[i].id.replace('cb_photo_',''));
	
	if ( photoIds.length==0 )
	{
		jsUtils.PreventDefault(evt);
		return false;
	}
	
	if (!confirm(confirmText)){
		jsUtils.PreventDefault(evt);
		return false;
	}
	
	var href = window.location.href;
	
	var hashIndex = href.indexOf("#");

	if ( hashIndex!=-1 )
		href = href.substring(0,hashIndex);
	
	var index = href.indexOf('?');
	if( index==-1 ) href+='?';
	else href+='&';	
	href+='<%=Bitrix.Security.BXCsrfToken.TokenKey %>=<%= Bitrix.Security.BXCsrfToken.GenerateToken() %>&target=photos&del='+photoIds.join(';');
	window.location.assign(href);
}

function selectAllPhotos(cb)

{
	var inputs = document.getElementsByTagName('INPUT');
	var photoIds = [];
	for ( var i =0; i< inputs.length;i++ )
		if ( inputs[i].id.match(/cb_photo\_(\d+)/gi)){
			inputs[i].checked = cb.checked;
			var res = inputs[i].parentNode.parentNode;
			if (cb.checked)
			{res.className += ' photo-photo-item-checked'} else 
			{res.className = res.className.replace(/photo\-photo\-item\-checked/g, ' ').replace(/\s\s/g, ' ');}
			
			}
}
window['__photo_result'] = <%= PicturesList(true) %>;
var oPhotoObjects = {min_slider_width:400,min_slider_height:400};

function __photo_to_init_slider()
{
	var res = document.getElementsByTagName('a');
	for (var ii = 0; ii < res.length; ii++)
	{
		if (res[ii].id.match(/photo\_(\d+)/gi))
		{
			res[ii].onclick = function(){ setTimeout(new Function("photo_init_big_slider(" + this.id.replace('photo_', '') + ");"), 10); return false; }
			res[ii].ondbclick = function(){ jsUtils.Redirect([], this.href); }
			var div = document.getElementById(res[ii]["id"] + '__id');
			if ( !div )
			div = document.createElement('div');
			div.style.position = "absolute"; 
			div.style.display = "none"; 
			div.className = "photo-photo-item-popup"; 
			div.id = res[ii]["id"] + '__id'; 
			div.title = '<%= GetMessageJS("GoToDetailInfo") %>';
			
			div.onshow = new Function(
				"this.style.visibility = 'hidden'; " + 
				"this.style.display = 'block'; " + 
				"var width = parseInt(this.offsetWidth); " + 
				"var height = parseInt(this.offsetHeight); " + 
				" if (width > 0 && height > 0) " + 
				" { " + 
					" this.style.top = (this.parentNode.offsetHeight - height) + 'px'; " + 
					" this.style.left = (this.parentNode.offsetWidth - width) + 'px'; " + 
				" } " + 
				" this.style.visibility = 'visible'; " + 
				" this.onshow = function() {this.style.display = 'block';} ");
				
				
			div.onmouseout = function()
			{
				this.bxMouseOver = 'N';
				var __this = this; 
				setTimeout(
					function()
					{
						if (__this.nextSibling && __this.nextSibling.bxMouseOver != "Y") 
						{ 
							__this.style.display = 'none'; 
						}
					}, 
					100);
			}
			div.onmouseover = function()
			{
				this.bxMouseOver = 'Y';
			}
			var id = res[ii].id.replace('photo_', '');
			var href = "#";
			if (window['__photo_result'])
				if ( window['__photo_result']['elements'])
					for ( var i =0;i<window['__photo_result']['elements'].length;i++ )
						if ( window['__photo_result']['elements'][i].id == id ){
							href = window['__photo_result']['elements'][i].photourl;
							break;
							}
			
			eval("div.onclick = function(e){jsUtils.PreventDefault(e); jsUtils.Redirect([], '" + href+ "');};");
			res[ii].parentNode.insertBefore(div, res[ii]);
			res[ii].onmouseover = function()
			{
				this.previousSibling.onshow();
				this.bxMouseOver = 'Y';
			}; 
			res[ii].onmouseout = function() 
			{
				this.bxMouseOver = 'N';
				var __this = this; 
				setTimeout(
					function()
					{
						if (__this.previousSibling && __this.previousSibling.bxMouseOver != "Y") 
						{ 
							__this.previousSibling.style.display = 'none'; 
						}
					}, 
					100);
			}
			
		}
	}
}
__photo_to_init_slider(); 

window.__photo_params = {

 'speed' : 4,
 'effects' : false,
 'template' : ('<div class="photo-title">#title#</div>' + '<table cellpadding="0" border="0" cellspacing="0"><tr>' + '<td class="td-slider-last"><div class="photo-original"><a href="#url#" target="blank">#origtext#</a></div></td></tr></table>'),
 'template_additional' : '<% if ( Component.EnableVoting && Component.IsVotingAllowed ){ %><div class="photo-rating">#voting_html#&nbsp;#voting_result#</span></span></div><%} %><% if ( Component.EnableComments ){ %><div class="photo-comments"><a href="#commenturl#"><%=GetMessage("Comment") %></a></div><%} %><div class="photo-description">#description#</div>'}; 

function photo_init_big_slider(id)
{
	var div = document.getElementById('bx_slider');
	if (!div)
	{
		var res = document.body.appendChild(document.createElement("DIV"));
		res.innerHTML = '<div id=\"bx_slider\" style=\"position:absolute;width:100%;display:none;\"><div id=\"bx_slider_container_outer\" style=\"height:400px;width:440px;\"><div id=\"bx_slider_container_header\" style=\"height:20px;width:100%;background-color:white;visibility:hidden;overflow:hidden;\"><div style=\"padding: 0 10px 0 10px;\"><a href=\"#\" id=\"bx_slider_nav_stop\" style=\"float:right;\" onclick=\"if(player){player.stop();PhotoMenu.PopupHide();} return false;\" title=\"<%=GetMessage("CloseSlider") %>\"><span></span></a><div class=\"bxp-data-pagen\"><div><%= GetMessage("Foto") %><span id=\"element_number\"></span><%=GetMessage("Of") %><span id=\"element_count\"></span></div></div></div></div><div id=\"bx_slider_container\"><div id=\"bx_slider_content_item\"></div><div id=\"bx_slider_nav\" style=\"margin-top:20px;\"><a href=\"#\" id=\"bx_slider_nav_prev\" hidefocus=\"true\" onclick=\"if(player){player.step(\'prev\');}return false;\" style=\"display:none;\"></a><a href=\"#\" id=\"bx_slider_nav_next\" hidefocus=\"true\" onclick=\"if(player){player.step(\'next\');}return false;\" style=\"display:none;\"></a></div><div id=\"bx_slider_content_loading\"><a href=\"#\" id=\"bx_slider_content_loading_link\"><span></span></a></div></div></div><div id=\"bx_slider_datacontainer_outer\" class=\"bxp-data\" style=\"width:440px;\"><div class=\"bxp-data-inner\"><div id=\"bx_slider_datacontainer\" style=\"display:none;\" class=\"bxp-data-container\"><div class=\"bxp-table\"><table cellpadding=\"0\" cellspasing=\"0\" border=\"0\" class=\"bxp-table\"><tr valign=\"top\"><td class=\"bxp-td-player\"><div class=\"bxp-mixer-container\"><div class=\"bxp-mixer-container-inner\"><table cellpadding=\"0\" border=\"0\" cellspacing=\"0\" class=\"bxp-mixer-container-table\"><tr><td class=\"bxp-mixer-container-player\"><a href=\"#\" id=\"bx_slider_nav_play\" onclick=\"if(player){if (player.params[\'status\'] == \'paused\') {player.params[\'status\']=\'play\';player.play(); this.title=\'<%= GetMessageJS("Pause") %>\';} else {player.stop(); this.title=\'<%= GetMessageJS("BeginSlideShow") %>\';}} return false;\" title=\"<%= GetMessageJS("BeginSlideShow") %>\"><span></span></a></td><td class=\"bxp-mixer-container-speed\"><div id=\"bx_slider_speed_panel\" title=\"<%= GetMessageJS("SlideShowSpeed") %>\"><div id=\"bx_slider_mixers\"><table cellpadding=\"0\" cellspasing=\"0\" border=\"0\" align=\"center\"><tr><td><a id=\"bx_slider_mixers_minus\"><span></span></a></td><td><div id=\"bx_slider_mixers_border\"><a id=\"bx_slider_mixers_cursor\" href=\"#\" style=\"left:80%;\"><span></span></a></div></td><td><a id=\"bx_slider_mixers_plus\"><span></span></a></td></tr></table></div><div id=\"bx_slider_speed_title\"><%= GetMessageJS("Original") %>:&nbsp;<span id=\"bx_slider_speed\">4</span>&nbsp;<%= GetMessageJS("Seconds") %></div><div class=\"empty-clear\" style=\"clear:both;\"></div></div></td></tr></table></div></div></td></tr></table></div><div id=\"bx_caption\"></div><div id=\"bx_caption_additional\"></div></div><div class=\"empty-clear\" style=\"clear:both;\"></div></div></div></div>';
		div = document.getElementById('bx_slider');
	}
	
	var res = GetImageWindowSize();

	PhotoMenu.PopupShow(
		div, 
		{
			'left' : '0', 
			'top' : (res['top'] + parseInt((res['height'] - 400)/2))
		}, 
		false, 
		false, 
		{
			'AfterHide' : function() {
				window.location.hash = 'gallery'; 
				if (window.player) 
				{
					window.player.stop();
					// remove events
					jsUtils.removeEvent(document, "keypress", __checkKeyPress);
				}
			}
		}
	);

	var res = false;
	if (window['__photo_result'] && window['__photo_result']['elements'] && id > 0)
	{
		for (var ii = 0; ii < window['__photo_result']['elements'].length; ii++)
		{
			if (window['__photo_result']['elements'][ii]['id'] == id)
			{
				res = true;
				break;
			}
		}
	}
	if (window.__photo_result && res)
	{
		__show_slider(id, '<%= PathForAjaxGetQuery %>', window.__photo_result);
	}
	else 
	{
		var url = '<%= PathForAjaxGetQuery %>';
		if (url.length <= 0) { url = window.location.href; } 
		var request = Bitrix.HttpUtility.createXMLHttpRequest();
	url = url.replace('bxpagenumber','1');
	request.open("GET", url, true);
	var self = this;

	request.onreadystatechange = function(){
		if(request.readyState != 4) return;
		var r = "", 
			txt = request.responseText;
		try{eval('window.__photo_result=' + txt + ';');__show_slider(" + id + ", '<%= PathForAjaxGetQuery %>', window.__photo_result);}catch(e) {PhotoMenu.PopupHide();};
	};
	request.send(); 
	}
	return false;
}
if (window.location.hash.substr(0, 6) == '#photo')
{
	var __photo_tmp_interval = setInterval(function()
	{
		try {
			if (bPhotoMainLoad === true && bPhotoSliderLoad == true && bPhotoPlayerLoad == true && bPhotoEffectsLoad === true && bPhotoCursorLoad === true && jsAjax && jsUtils)
			{
				photo_init_big_slider(window.location.hash.substr(6));
				clearInterval(__photo_tmp_interval);
			}
		} catch (e) { }
	}, 500);
}

</script>

