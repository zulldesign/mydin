<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_section_templates__default_template" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery.album/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<%@ Import Namespace="Bitrix.Services" %>
<%
	string themeUrl = Bitrix.IO.BXPath.ToUri(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath+ "/bitrix/templates/" + Bitrix.BXSite.CurrentTemplate + "/components/bitrix/photogallery/new/themes/gray/style.css", true); 
%>
<script runat="server">
	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		Page.ClientScript.RegisterStartupScript(GetType(), "initslider", "window.setTimeout(initSlider,0);",true);
	}
</script>
<link type="text/css" rel="Stylesheet" href="<%=themeUrl %>" />

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
<% if (Component.Sections.Count > 0)
   {
	   
	   
	   %>
<ul class="photo-items-list photo-album-list">
    <%
		string urlToNoImagePic = Bitrix.IO.BXPath.ToUri("bitrix/templates/" + 
			Bitrix.BXSite.CurrentTemplate + "/components/bitrix/photogallery/new/themes/gray/images/album/cover_empty.gif",true);
     foreach (BXIBlockSection section in Component.SectionItems)
       {
		  

		  %>
    
	<li class="photo-album-item photo-album-active" id="photo_album_info_<%=section.Id %>">
		<div class="photo-item-cover-block-outside">
			<div class="photo-item-cover-block-container">
				<div class="photo-item-cover-block-outer">
					<div class="photo-item-cover-block-inner">
						<div class="photo-item-cover-block-inside">
							<div class="photo-item-cover photo-album-avatar " id="photo_album_cover_<%=section.Id %>" title="<%=Encode(section.Name)%>"
							onclick="window.location= '<%=Component.AlbumDescriptionDictionary[section].Url%>';" 
							style="background-image: url('<%= Component.AlbumDescriptionDictionary[section].IsCoverEmpty ? urlToNoImagePic : Component.AlbumDescriptionDictionary[section].CoverImageUrl %>'); 
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
										<a rel="nofollow" href="<%=Bitrix.Services.BXSefUrlManager.CurrentUrl.AbsolutePath %>?del=<%=section.Id %>&ValidationToken=<%= Bitrix.Security.BXCsrfToken.GenerateToken() %>" 
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
						<div class="photo-album-photos-top"><%= string.Format(GetMessageRaw("PhotosCount"), section.ElementsCount) %></div>
						<div class="photo-album-name">
							<a href="<%= Component.AlbumDescriptionDictionary[section].Url %>" id="photo_album_name_<%=section.Id %>" 
							title="<%=Encode(section.Name)%>"><%=Encode(section.Name.Length > 30 ? section.Name.Substring(0,25)+"...":section.Name)%></a>
						</div>
						<div class="photo-album-description" id="photo_album_description_<%=section.Id %>"></div>
						<div class="photo-album-date"><span id="photo_album_date_<%=section.Id %>"><%=section.CreateDate.ToString("d") %></span></div>
						<div class="photo-album-photos"><%= string.Format(GetMessageRaw("PhotosCount"), section.ElementsCount) %></div>
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

<div class="photo-photo-item-ascetic">
	<div class="photo-photo-item-ascetic-inner" style="width:<%=fotoSize%>px;
		height:<%=fotoSize%>px;overflow: hidden; position: relative;" onmouseover="this.firstChild.style.display='block'" onmouseout="this.firstChild.style.display='none'"><div style="position: absolute; display: none; visibility: visible; top: <%=fotoSize-20%>px; left: <%=fotoSize-20%>px;" class="photo-photo-item-popup" 
		 title="<%=GetMessage("ViewFotoDetail") %>" onclick="window.location.href='<%=  Component.PhotoDescriptionDictionary[photo].PhotoUrl %>'"></div>		
		<a class="photo-photo-item-ascetic-inner" onclick="if ( picSlider ){ picSlider.open(<%= i.ToString() %>);} return false;" 
		style="width:<%=fotoSize%>px;
		height:<%=fotoSize%>px; display: block;" href="<%=Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].DetailUrl %>" id="photo_<%=photo.Id %>">
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
<% 
    if (Results.Get<bool>("PagingShow") && !"top".Equals(Parameters.Get<string>("PagingPosition"), StringComparison.InvariantCultureIgnoreCase))
    {  %>
    <br />
<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" CurrentPosition="bottom"
    Template="<%$ Parameters:PagingTemplate %>" />
<%	}
} %>


<div id="bx-slider" style="position: absolute; width: 100%; z-index: 100; left: 0px; top: 10px;display:none;">
<div id="bx_slider_container_outer" style="height: 579px; width: 831px;">
	<div id="bx_slider_container_header" style="height: 20px; width: 100%; background-color: white; visibility: visible; overflow: hidden;">
		<div style="padding: 0pt 10px;">
		<a href="#" id="bx_slider_nav_stop" style="float: right;" onclick="if(picSlider){picSlider.close()} return false;" title="<%= GetMessageRaw("Close") %>">
		<span></span></a>
		<div class="bxp-data-pagen" style="display:none;">
			<div><%=GetMessage("Foto") %>&nbsp<span id="<%= ClientID %>element_number"></span>&nbsp<%=GetMessage("Of") %>&nbsp<span id="<%= ClientID %>element_count"></span>
			</div>
		</div>
	</div>
	</div>
	<div id="bx_slider_container">
		<div style="visibility: visible;" id="bx_slider_content_item">
			<div style="overflow: hidden; width: 811px; height: 542px;" id="<%=ClientID %>imgcontainer" class="bx-slider-image-container">
			<img id = "<%=ClientID %>dispImg" src="" title="" alt="" style="width: 811px; height: 542px; visibility: visible;">
			</div>
			</div>
			<div id="bx_slider_nav" style="margin-top: 20px;">
				<a href="#" id="bx_slider_nav_prev" hidefocus="true"
				onclick="if(picSlider){picSlider.prev();}return false;" style="display: block; height: 579px;"></a>
				<a href="#" id="bx_slider_nav_next" hidefocus="true" onclick="if(picSlider){picSlider.next();}return false;" style="display: block; height: 579px;"></a>
			</div>
			<div style="" id="bx_slider_content_loading">
				<a href="#" id="bx_slider_content_loading_link"><span></span></a>
			</div>
		</div>
	</div>
			<div id="bx_slider_datacontainer_outer" class="bxp-data" style="width: 831px; display: block;">
			<div class="bxp-data-inner">
				<div id="bx_slider_datacontainer" style="display: block;" class="bxp-data-container">
					<div class="bxp-table">
					
		<div class="photo-title" id = "<%=ClientID %>photo_title">
		</div>
		
			<table border="0" cellpadding="0" cellspacing="0">
			<tbody>
				<tr>
					<td class="td-slider-last">
						<div class="photo-shows">
								 <a id="<%=ClientID %>originalUrl" target="_blank" class="bx-photourl-original" href=""></a>
						</div>
					</td>
				</tr>
				<tr>
					<td class="td-slider-last">
						<div class="photo-shows"><% if (Component.CanModify)
								  { %><a id="<%=ClientID %>editUrl" href=""><%= GetMessageRaw("Edit") %></a><%} %></div>
					</td>
				</tr>
			</tbody>
			</table>
		</div>
		<div id="bx_caption_additional">
			<div class="photo-description"></div>
		</div>
		</div>
		<div class="empty-clear" style="clear: both;">
		</div>
		</div>
		</div>
</div>

<script type="text/javascript">

function getScrollXY() {
  var scrOfX = 0, scrOfY = 0;
  if( typeof( window.pageYOffset ) == 'number' ) {
    //Netscape compliant
    scrOfY = window.pageYOffset;
    scrOfX = window.pageXOffset;
  } else if( document.body && ( document.body.scrollLeft || document.body.scrollTop ) ) {
    //DOM compliant
    scrOfY = document.body.scrollTop;
    scrOfX = document.body.scrollLeft;
  } else if( document.documentElement && ( document.documentElement.scrollLeft || document.documentElement.scrollTop ) ) {
    //IE6 standards compliant mode
    scrOfY = document.documentElement.scrollTop;
    scrOfX = document.documentElement.scrollLeft;
  }
  return [ scrOfX, scrOfY ];
}

function GetWindowScrollSize(pDoc)
	{
		var width, height;
		if (!pDoc)
			pDoc = document;

		if ( (pDoc.compatMode && pDoc.compatMode == "CSS1Compat"))
		{
			width = pDoc.documentElement.scrollWidth;
			height = pDoc.documentElement.scrollHeight;
		}
		else
		{
			if (pDoc.body.scrollHeight > pDoc.body.offsetHeight)
				height = pDoc.body.scrollHeight;
			else
				height = pDoc.body.offsetHeight;

			if (pDoc.body.scrollWidth > pDoc.body.offsetWidth || 
				(pDoc.compatMode && pDoc.compatMode == "BackCompat") ||
				(pDoc.documentElement && !pDoc.documentElement.clientWidth)
			)
				width = pDoc.body.scrollWidth;
			else
				width = pDoc.body.offsetWidth;
		}
		return {scrollWidth : width, scrollHeight : height};
	}

function GetWindowInnerSize(pDoc)
 {
	var width, height;
	if (!pDoc)
	pDoc = document;

	if (self.innerHeight) // all except Explorer
	{
		width = self.innerWidth;
		height = self.innerHeight;
	}
	else if (pDoc.documentElement && (pDoc.documentElement.clientHeight || pDoc.documentElement.clientWidth)) // Explorer 6 Strict Mode
	{
	 width = pDoc.documentElement.clientWidth;
	height = pDoc.documentElement.clientHeight;
	}
	else if (pDoc.body) // other Explorers
	{
		width = pDoc.body.clientWidth;
		height = pDoc.body.clientHeight;
	}
	return {innerWidth : width, innerHeight : height};
 }




var slider = function $Slider(){};

slider.prototype.next = function()
{

	if ( this.position == this.pictures.length - 1)
	{
		if (!this.gotLastPage){

			this.sendNewPageRequest("next");
			return;
		}
	}
	if ( this.position >= this.pictures.length-1 ){

		return;
	}
	this.position++;
	this.refreshInfo();

}

slider.prototype.prev = function()
{

	if ( this.position <=0 )
	{
		if ( this.page<=1 ) return;
		this.page--;
		if (!this.gotFirstPage){

			this.sendNewPageRequest("prev");

		}
		return;
	}

	this.position--;
	this.refreshInfo();
}

slider.prototype.setNewPopupSizes = function()
{
	
	var windowSize = GetWindowInnerSize();
	var scrollSize = GetWindowScrollSize();

	this.maxPopupHeight = Math.round(windowSize.innerHeight*0.75);
	this.maxPopupWidth = Math.round(this.maxPopupHeight*1.5);
	var max,koeff;
	if ( this.pictures[this.position].width > this.maxPopupWidth || this.pictures[this.position].height > this.maxPopupHeight ){

		if (  this.pictures[this.position].width > this.pictures[this.position].height ){
			this.img.style.width = this.maxPopupWidth+"px";
			this.popupWidth = this.maxPopupWidth;
			max = this.pictures[this.position].width;
			koeff = this.maxPopupWidth/max;
			this.popupHeight =  Math.round(koeff*this.pictures[this.position].height);

			this.img.style.height  =this.popupHeight + "px";
			
		}
		else 
		{
			this.popupHeight = this.maxPopupHeight;
			this.img.style.height = this.maxPopupHeight+"px";
			max = this.pictures[this.position].height;
			koeff = this.maxPopupHeight/max;
			this.popupWidth = Math.round(koeff*this.pictures[this.position].width) ;
			this.img.style.width = this.popupWidth + "px";
		}
	}
	
	else {

		if ( this.pictures[this.position].width > this.minPopupWidth || this.pictures[this.position].height > this.minPopupHeight )
		{
			if (  this.pictures[this.position].width > this.pictures[this.position].height ){

			this.img.style.width = this.pictures[this.position].width+"px";
			this.popupWidth = this.pictures[this.position].width;
			koeff = this.pictures[this.position].height/this.pictures[this.position].width;
			this.popupHeight =  this.pictures[this.position].height;

			this.img.style.height = this.popupHeight + "px";
			
			}
			
		}
		else {

			this.popupWidth = this.minPopupWidth;
			this.popupHeight = this.minPopupHeight;
			this.img.style.width = this.pictures[this.position].width+"px";
		
			this.img.style.height = this.pictures[this.position].height+"px";
		}
		
	}

	this.container.style.width = this.popupWidth+25+"px";
	this.container.style.height= this.popupHeight+100+"px";
	var scOffset = getScrollXY();
	this.container.style.left = Math.round((windowSize.innerWidth - this.popupWidth)/2)+"px";
	this.container.style.top = Math.round((windowSize.innerHeight - this.popupHeight)/2 + scOffset[1]-40)+"px";

	this.imageContainer.style.width = this.popupWidth+"px";
	this.imageContainer.style.height= this.popupHeight+"px";
	
	var container_outer = document.getElementById('bx_slider_container_outer');
	if ( container_outer ){
		container_outer.style.width = this.popupWidth+20+"px";
		container_outer.style.height =  this.popupHeight+"px";
	}
	
	var data_outer = document.getElementById('bx_slider_datacontainer_outer');
	if ( data_outer ){
		data_outer.style.width = this.popupWidth+20+"px";
	}
	

	if ( this.a_next ){
		this.a_next.style.height = this.popupHeight+"px";
	}
	

	if ( this.a_prev ){
		this.a_prev.style.height = this.popupHeight+"px";
	}

}

slider.prototype.refreshInfo = function()
{
	this.imageIsLoaded = false;

	this.setNewPopupSizes();

	this.img.src = this.pictures[this.position].url;

	this.center();

	this.showSpinner();
	if ( this.editLink && this.editLink.href )
		this.editLink.href = this.pictures[this.position].editurl;
		
	if ( this.origLink && this.origLink.href ){
		this.origLink.href = this.pictures[this.position].url;
		if(this.origLink.innerText )
			this.origLink.innerText = this.pictures[this.position].origtext;
		else
			this.origLink.textContent = this.pictures[this.position].origtext;
	}
	
	if ( this.titleDiv.innerText )
		this.titleDiv.innerText = this.pictures[this.position].title;
	else
		this.titleDiv.textContent = this.pictures[this.position].title;
		
	if ( this.page<=1 && this.position==0 ){
		if ( this.a_prev )
			this.a_prev.style.display="none";
	}
	else
	{
		if ( this.a_prev )
			this.a_prev.style.display="inline";
	}
	if ( this.a_next && !(this.gotLastPage && this.position == this.pictures.length-1))
		this.a_next.style.display="inline";	
	else 
		this.a_next.style.display="none";
}

slider.prototype.center = function()
{
	var parent = this.img.parentNode;

	if ( this.pictures[this.position].width < this.minPopupWidth && this.pictures[this.position].height < this.minPopupHeight){ //have to center

		this.imageCenteringContainer.style.paddingTop = (this.minPopupHeight- this.pictures[this.position].height - 10)/2+"px";
		this.imageCenteringContainer.style.paddingLeft = (this.minPopupWidth- this.pictures[this.position].width - 10)/2+"px";
		
		if ( parent ){
			if ( parent == this.imageCenteringContainer )
				return;
			parent.removeChild(this.img);
			this.imageCenteringContainer.appendChild(this.img);
			parent.appendChild(this.imageCenteringContainer);
			
		}

	}
	else 
	{
		if ( parent ){
			if ( parent ==  this.imageCenteringContainer  ){
				if ( this.imageCenteringContainer.parentNode == this.imageContainer ){
					this.imageContainer.removeChild(this.imageCenteringContainer);
					this.imageContainer.appendChild(this.img);
				}
			}
			
		}
	}
}

slider.prototype.hideSpinner = function()
{
	if ( this.imageIsLoaded )
		this.spinner.style.display="none";
	else 
		window.setTimeout(function(){return this.hideSpinner()},500);
}

slider.prototype.showSpinner = function()
{

	if (!this.imageIsLoaded && this.spinner){
		this.spinner.style.display = "";
		this.img.style.display="none";
		}
}

slider.prototype.sendNewPageRequest = function(direction)
{
	var request = Bitrix.HttpUtility.createXMLHttpRequest();
	var page;
	if ( direction == "next")
		page = parseInt(this.pictures[this.position].page)+1;
	else 
		page = parseInt(this.pictures[this.position].page)-1;
	var reqPath = this.path.replace("bxpagenumber",page);
	request.open("GET", reqPath, true);
	var self = this;
	request.onreadystatechange = function(){
		if(request.readyState != 4) return;
		var r = "", 
			txt = request.responseText;
		if(Bitrix.TypeUtility.isNotEmptyString(txt)){
			txt = Bitrix.HttpUtility.htmlDecode(txt);
			try{ r = eval("(" + txt + ")"); } 
			catch(e){}		
			}
		self.callBack(request.status, r,direction);
	};
	request.send();
}

slider.prototype.callBack = function(status,result,direction)
{
	if ( status!="200" ) return;
	if ( result.length > 0 ){
		if ( this.topLastLoadedId == result[0].id ){
			if ( direction=="next" ){
			 this.gotLastPage = true;
				if ( this.a_next )
					this.a_next.style.display="none";
			 }
			else 
				this.gotFirstPage = true;
			
			return;
		}
		else if ( this.bottomLastLoadedId == result[0].id )
			{
				if ( direction=="prev" ) this.gotFirstPage = true;
				else 
					this.gotLastPage = true;
				return;
		}
		else{
			if ( direction=="next" )
				this.topLastLoadedId = result[0].id;
			else 
				this.bottomLastLoadedId = result[0].id;
		}
		
		
		if ( direction=="next" ){
			for ( var i = 0 ;i < result.length;i++)
				this.pictures.push(result[i]);
			this.next();
		}
		else{
			this.position = result.length;
			for ( var i = 0 ;i < this.pictures.length;i++)
				result.push(this.pictures[i]);
			this.pictures=result;
			this.prev();
		}

		
	}
	else 
	{
		if ( direction=="next" ) 
			this.gotLastPage = true;
		else 
			this.gotFirstPage = true;
	}
}

slider.prototype.close = function()
{
	this.container.style.display="none";
	this.overlay.style.display="none";
}
slider.prototype.open = function(pos)
{

	if ( pos >= this.pictures.length || pos < 0 ) return;
	this.imageIsLoaded = false;
	this.position = pos;
	
	this.refreshInfo();
	this.container.style.display = "block";
	this.createOverlay();

}

slider.prototype.createOverlay = function()
{
	var windowSize = GetWindowScrollSize();
	
	if (!this.overlay){
		this.overlay = document.createElement('div');
		document.body.appendChild(this.overlay);
		this.overlay.style.background=" none repeat scroll 0% 0% rgb(5, 38, 53)";
		this.overlay.style.position="absolute";
		this.overlay.style.display="none";
		this.overlay.style.opacity = "0.5";
		this.overlay.style.top= "0pt"; 
		this.overlay.style.left ="0pt";
		this.overlay.style.position="absolute";
		this.overlay.style.zIndex="99";
		this.overlay.style.filter ="progid:DXImageTransform.Microsoft.Alpha(opacity=50)";
	}
	this.overlay.style.width = windowSize.scrollWidth + "px";
	this.overlay.style.height = windowSize.scrollHeight + "px";
	this.overlay.style.display="block";

}


slider.prototype.init = function (overlay,container,img,countSpan,posSpan,title,path,page,recsPerPage,spinner,editLink,origLink, pictures)
{
	this.pictures = pictures;
	this.position=0;
	this.path = path;
	this.recsPerPage = recsPerPage;
	this.pageCounter = 0;

	this.imageIsCentered = false;
	this.imageCenteringContainerId = "<%=ClientID %>_centering";
	this.imageCenteringContainer = document.createElement("DIV");
	this.imageCenteringContainer.id = this.imageCenteringContainerId;

	this.a_prev = document.getElementById('bx_slider_nav_prev');
	this.a_next = document.getElementById('bx_slider_nav_next');
	this.editLink = document.getElementById(editLink);
	this.origLink = document.getElementById(origLink);
	this.spinner = document.getElementById(spinner);
	this.imageIsLoaded = false;
	this.page = parseInt(page);
	if ( pictures.length > 0 ){
		this.topLastLoadedId = pictures[0].id;
		this.bottomLastLoadedId = pictures[0].id;
	}
	else {
		this.topLastLoadedId = 0;
		this.bottomLastLoadedId = 0;
	}
	this.gotLastPage = false;
	this.gotFirstPage = false;
	this.container = document.getElementById(container);
	this.container.parentNode.removeChild(this.container);
	document.body.appendChild(this.container);
	this.countSpan = document.getElementById(countSpan);
	this.posSpan = document.getElementById(posSpan);
	this.titleDiv = document.getElementById(title);
	this.img = document.getElementById(img);
	this.imageContainer = this.img.parentNode;
	
	this.maxPage = 0;
	
	this.popupWidth = 0;
	this.popupHeight = 0;
	this.minPopupHeight = 300;
	this.minPopupWidth = 300;

	
	this.img.onload = function (sender) {
		return function(){ 

		sender.imageIsLoaded = true;
		sender.hideSpinner();
		sender.img.style.display="";
		}

	}(this);
	
	if ( this.countSpan.innerText )
		this.countSpan.innerText = pictures.length;
	else
		this.countSpan.textContent = pictures.length;
}

var picSlider = new slider();

function initSlider()
{
	picSlider.init('photo_substrate','bx-slider','<%=ClientID %>dispImg',"<%= ClientID %>element_count",
	"<%= ClientID %>element_number","<%= ClientID %>photo_title","<%=PathForAjaxGetQuery%>",
	'<%= CurrentPage %>',<% = Component.Parameters["PagingRecordsPerPage"] %>,"bx_slider_content_loading","<%=ClientID %>editUrl","<%=ClientID %>originalUrl",<%=PicturesList() %>);
}


</script>

