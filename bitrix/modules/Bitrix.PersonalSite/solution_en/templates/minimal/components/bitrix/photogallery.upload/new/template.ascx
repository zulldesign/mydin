<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_upload_templates__default_template" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery.upload/component.ascx" %>
<% if (Results.ContainsKey("ShowAlbumSelect")) { %>
<table border="0" cellspacing="0">
    <tr>
        <td align="left" colspan="2">
            <div id="Div6">
                <%= GetMessage("PleaseSpecifyDestinationAlbum") %>
                <asp:DropDownList runat="server" ID="SelectAlbums">
                </asp:DropDownList>
            </div>
        </td>
    </tr>
    <tr>
        <td colspan="2">
            <table width="100%">
                <tr>
                    <td style="height: 26px;">
                        <asp:Button runat="server" ID="GoToAlbum" Text="<%$ Loc:ButtonText.UploadInAlbum %>" OnClick="GoToAlbum_Click" />
                    </td>
                    <td width="100%" style="height: 26px;">
                        <asp:Button  runat="server" ID="Cancel" Text="<%$ Loc:Cancel %>" OnClientClick="GoBack(); return false;" UseSubmitBehavior="false" />
					</td>
                </tr>
            </table>
        </td>
    </tr>
</table>  
<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errortext" HeaderText="<%$ Loc:Kernel.Error %>"/>
<script type="text/javascript" language="javascript">
    window.GoBack = function()
    {
        window.location.href = "<%= Results["BACK_URL"] %>";
    }
</script>
<% } else if (Results.ContainsKey("ModifyElements")) { %>

<script type="text/javascript">
show_tags = 'N';
window.urlRedirectThis = '#SECTION_LINK';
window.urlRedirect = '#SECTION_LINK_EMTY';

if (typeof oText != "object")
	oText = {};
	oText["instructionsCommon1"] = "<%= GetMessageJS("InstructionsCommon") %>";
	oText["instructionsNotWinXPSP21"] = "<%= GetMessageJS("InstructionsNotWinxpsp2") %>";
	oText["instructionsWinXPSP21"] = "<%= GetMessageJS("InstructionsWinxpsp2") %>";
	oText["Title"] = "Title";
	oText["Tags"] = "Tags";
	oText["Description"] = "Description";
	oText["NoPhoto"] = "NoPhoto";
	
iu = null;
t = null;
bInitPhotoUploader = false;
function to_init()
{
	is_loaded = false;
	try
	{
		if (PUtilsIsLoaded == true)
			is_loaded = true;
	}
	catch(e){}
	
	if (is_loaded)
	{
//		if (__browser.isOpera)
//		{
//			document.getElementById('ControlsAppletForm').style.display = 'none';
//		}
		if (!bInitPhotoUploader && InitPhotoUploader)
		{
			InitPhotoUploader();
			bInitPhotoUploader = true;
		}
	}
	if (!bInitPhotoUploader)
		setTimeout(to_init, 100);
	return;
}

function AfterUploadLink2(htmlPage)
{
	var sel = document.getElementById("<%= lbSections.ClientID %>");
    window.location.href = "<%= Parameters.Get("UrlTemplateAlbum","error").ToLower() %>".replace("#albumid#",sel.options[sel.selectedIndex].value);   
}

function InnerCompleteLink(Status, StatusText)
{
    var scriptId = "photogalleryUploadingCompletion";
    var scriptRx = new RegExp("<script[^>]*id=\""+scriptId+"\"[^>]*>([\\s\\S]+?)<\/script>", "i");
    var m = scriptRx.exec(StatusText);
    if(!m || m.length < 2)
        return;
    var script = m[1];
    //console.log("> Completion script: %s", script); 
    window.eval(script);        
}

function BeforeUploadLink2()
{
	if (PhotoClass && PhotoClass.Uploader)
        PhotoClass.Uploader.AddField('<%= JSEncode(Bitrix.Security.BXCsrfToken.TokenKey) %>', '<%= JSEncode(Bitrix.Security.BXCsrfToken.GenerateToken()) %>');
    BeforeUploadLink();
}

function InitPhotoUploader()
{
	iu = new ImageUploaderWriter("ImageUploader1", "100%", 315);
//	
	iu.addEventListener("SelectionChange", "ChangeSelectionLink");
	iu.addEventListener("UploadFileCountChange", "ChangeFileCountLink");
	iu.addEventListener("AfterUpload", "AfterUploadLink2");
	iu.addEventListener("BeforeUpload", "BeforeUploadLink2");
	
	iu.addEventListener("InnerComplete", "InnerCompleteLink");
//	
	iu.fullPageLoadListenerName = "InitLink";
//	
	iu.addParam("ShowDescriptions", "false");
	iu.addParam("AllowRotate", "true");
	iu.addParam("PaneLayout", "OnePane");
	iu.addParam("UseSystemColors", "false");
	iu.addParam("BackgroundColor", "#ededed");
	iu.addParam("UploadPaneBackgroundColor", "#ededed");
	iu.addParam("UploadPaneBorderStyle", "none");
	iu.addParam("PreviewThumbnailBorderColor", "#afafaf");
	iu.addParam("PreviewThumbnailBorderHoverColor", "#91a7d3");
	iu.addParam("PreviewThumbnailActiveSelectionColor", "#ff8307");
	iu.addParam("PreviewThumbnailInactiveSelectionColor", "#ff8307");
	
	iu.addParam("ShowUploadListButtons", "false");
	iu.addParam("ShowButtons", "false");
	iu.addParam("FolderView", "Thumbnails");

	iu.addParam("UploadSourceFile", "false");
	iu.addParam("UploadFileDescription", "true");
	//iu.addParam("MaxTotalFileSize", "<%= Component.MaxRequestLength.ToString() %>");
//	
	iu.addParam("UploadThumbnail1FitMode", "Fit");
	iu.addParam("UploadThumbnail1Width", "<%= Parameters.Get("PreviewWidth",150) %>");
	iu.addParam("UploadThumbnail1Height", "<%= Parameters.Get("PreviewHeight",150) %>");
	iu.addParam("UploadThumbnail1JpegQuality", "<%= Parameters.Get("PreviewQuality",100) %>");
//	
	iu.addParam("UploadThumbnail2FitMode", "Fit");
	iu.addParam("UploadThumbnail2Width", "<%= Parameters.Get("PhotoWidth",250) %>");
	iu.addParam("UploadThumbnail2Height", "<%= Parameters.Get("PhotoHeight",250) %>");
	iu.addParam("UploadThumbnail2JpegQuality", "<%= Parameters.Get("Quality",100) %>");
//	
	iu.addParam("UploadThumbnail3FitMode", "ActualSize");
	iu.addParam("UploadThumbnail3CopyExif", "false");
//	iu.addParam("ExtractExif", "ExifDateTime;ExifOrientation;ExifModel");
	iu.addParam("UploadThumbnail3JpegQuality", "<%= Parameters.Get("Quality",100) %>");
//	
//	//Configure upload settings.
	iu.addParam("FilesPerOnePackageCount", "<%= Parameters.Get("Quantity",10) %>");
//	
//Configure URL files are uploaded to.
    iu.addParam("TimeOut", "6000");
	iu.addParam("Action", "<%= JSEncode(Bitrix.Services.BXSefUrlManager.CurrentUrl.AbsoluteUri) %>");
	//Configure URL where to redirect after upload.
//	iu.addParam("RedirectUrl", "");
	
	//For ActiveX control full path to CAB file (including file name) should be specified.
	iu.activeXControlCodeBase = "<%= Bitrix.BXUri.ToRelativeUri("~/bitrix/image_uploader/ImageUploader.cab") %>";
	iu.activeXClassId = "718B3D1E-FF0C-4EE6-9F3B-0166A5D1C1B9";
	iu.activeXControlVersion = "6,5,7,0";
	
	//For Java applet only path to directory with JAR files should be specified (without file name).
	iu.javaAppletCodeBase = "<%= Bitrix.BXUri.ToRelativeUri("~/bitrix/image_uploader/") %>";
	iu.javaAppletClassName="com.bitrixsoft.imageuploader.ImageUploader.class"; 
	iu.javaAppletJarFileName="ImageUploader.jar"; 
	iu.javaAppletVersion = "6.5.7.0";
	iu.javaAppletCached = true;
		
	iu.showNonemptyResponse = "off";
	
	//iu.activeXControlEnabled = false;
//	
	//Configure appearance.
	//Set and configure advanced details view.
	iu.addParam("ButtonAddToUploadListText", "<%= GetMessageJS("ButtonAddToUploadListText") %>");
	iu.addParam("ButtonAddAllToUploadListText","<%= GetMessageJS("ButtonAddAllToUploadListText") %>");
	iu.addParam("ButtonRemoveFromUploadListText", "<%= GetMessageJS("ButtonRemoveFromUploadListText") %>");
	iu.addParam("ButtonRemoveAllFromUploadListText", "<%= GetMessageJS("ButtonRemoveAllFromUploadListText") %>");
	
	iu.addParam("MenuThumbnailsText","<%= GetMessageJS("MenuThumbnailsText") %>");
	iu.addParam("MenuDetailsText", "<%= GetMessageJS("MenuDetailsText") %>");
	iu.addParam("MenuListText","<%= GetMessageJS("MenuListText") %>");
	iu.addParam("MenuIconsText","<%= GetMessageJS("MenuIconsText") %>");
	iu.addParam("MenuArrangeByText","<%= GetMessageJS("MenuArrangeByText") %>");
	iu.addParam("MenuArrangeByUnsortedText","<%= GetMessageJS("MenuArrangeByUnsortedText") %>");
	iu.addParam("MenuArrangeByNameText","<%= GetMessageJS("MenuArrangeByNameText") %>");
	iu.addParam("MenuArrangeBySizeText", "<%= GetMessageJS("MenuArrangeBySizeText") %>");
	iu.addParam("MenuArrangeByTypeText", "<%= GetMessageJS("MenuArrangeByTypeText") %>");
	iu.addParam("MenuArrangeByModifiedText", "<%= GetMessageJS("MenuArrangeByModifiedText") %>");
	iu.addParam("MenuArrangeByPathText","<%= GetMessageJS("MenuArrangeByPathText") %>");
	iu.addParam("MenuSelectAllText", "<%= GetMessageJS("MenuSelectAllText") %>");
	iu.addParam("MenuDeselectAllText", "<%= GetMessageJS("MenuDeselectAllText") %>");
	iu.addParam("MenuInvertSelectionText","<%= GetMessageJS("MenuInvertSelectionText") %>");
	iu.addParam("MenuRemoveFromUploadListText", "<%= GetMessageJS("MenuRemoveFromUploadListText") %>");
	iu.addParam("MenuRemoveAllFromUploadListText", "<%= GetMessageJS("MenuRemoveAllFromUploadListText") %>");
	iu.addParam("MenuRefreshText", "<%= GetMessageJS("MenuRefreshText") %>");
	
	iu.instructionsCommon = "P_INSTRUCTIONS_COMMON";
	iu.instructionsNotWinXPSP2 = "P_INSTRUCTIONS_NOT_WINXPSP2";
	iu.instructionsWinXPSP2 = "P_INSTRUCTIONS_WINXPSP2";
	
	//ImageUploader properties
	iu.addParam("AuthenticationRequestBasicText", "<%= GetMessageJS("AuthenticationRequestBasicText") %>");
	iu.addParam("AuthenticationRequestButtonCancelText", "<%= GetMessageJS("AuthenticationRequestButtonCancelText") %>");
	iu.addParam("AuthenticationRequestDomainText", "<%= GetMessageJS("AuthenticationRequestDomainText") %>");
	iu.addParam("AuthenticationRequestLoginText","<%= GetMessageJS("AuthenticationRequestLoginText") %>");
	iu.addParam("AuthenticationRequestNtlmText", "<%= GetMessageJS("AuthenticationRequestNtlmText") %>");
	iu.addParam("AuthenticationRequestPasswordText", "<%= GetMessageJS("AuthenticationRequestPasswordText") %>");
	iu.addParam("ButtonAddFilesText", "<%= GetMessageJS("ButtonAddFilesText") %>");
	iu.addParam("ButtonAdvancedDetailsCancelText", "<%= GetMessageJS("ButtonAdvancedDetailsCancelText") %>");
	iu.addParam("ButtonDeselectAllText", "<%= GetMessageJS("ButtonDeselectAllText") %>");
	iu.addParam("RotateIconClockwiseTooltipText", "<%= GetMessageJS("RotateIconClockwiseTooltipText") %>");
	iu.addParam("RotateIconCounterclockwiseTooltipText", "<%= GetMessageJS("RotateIconCounterclockwiseTooltipText") %>");
	
	iu.addParam("ButtonSelectAllText", "<%= GetMessageJS("ButtonSelectAllText") %>");
	iu.addParam("ButtonSendText", "<%= GetMessageJS("ButtonSendText") %>");
	iu.addParam("DescriptionEditorButtonCancelText", "<%= GetMessageJS("DescriptionEditorButtonCancelText") %>");
	iu.addParam("FileIsTooLargeText", "<%= GetMessageJS("FileIsTooLargeText") %>");
	iu.addParam("HoursText", "<%= GetMessageJS("HoursText") %>");
	iu.addParam("KilobytesText", "<%= GetMessageJS("KilobytesText") %>");
	iu.addParam("LoadingFilesText", "<%= GetMessageJS("LoadingFilesText") %>");
	iu.addParam("MegabytesText", "<%= GetMessageJS("MegabytesText") %>");
	iu.addParam("MenuAddAllToUploadListText", "<%= GetMessageJS("MenuAddAllToUploadListText") %>");
	iu.addParam("MenuAddToUploadListText", "<%= GetMessageJS("MenuAddToUploadListText") %>");
	iu.addParam("MessageBoxTitleText", "<%= GetMessageJS("MessageBoxTitleText") %>");
	iu.addParam("MessageCannotConnectToInternetText", "<%= GetMessageJS("MessageCannotConnectToInternetText") %>");
	iu.addParam("MessageMaxFileCountExceededText", "<%= GetMessageJS("MessageMaxFileCountExceededText") %>");
	iu.addParam("MessageMaxTotalFileSizeExceededText", "<%= GetMessageJS("MessageMaxTotalFileSizeExceededText") %>");
	iu.addParam("MessageNoResponseFromServerText", "<%= GetMessageJS("MessageNoResponseFromServerText") %>");
	iu.addParam("MessageServerNotFoundText", "<%= GetMessageJS("MessageServerNotFoundText") %>");
	iu.addParam("MessageUnexpectedErrorText", ""); //подавление сообщений
	iu.addParam("MessageUploadCancelledText", "<%= GetMessageJS("MessageUploadCancelledText") %>");
	iu.addParam("MessageUploadCompleteText", ""); //подавление сообщения

	iu.addParam("MessageUploadFailedText", "<%= GetMessageJS("MessageUploadFailedText") %>");
	iu.addParam("MessageUserSpecifiedTimeoutHasExpiredText", "<%= GetMessageJS("MessageUserSpecifiedTimeoutHasExpiredText") %>");
	iu.addParam("MinutesText", "<%= GetMessageJS("MinutesText") %>");
	iu.addParam("ProgressDialogCancelButtonText", "<%= GetMessageJS("ProgressDialogCancelButtonText") %>");
	iu.addParam("ProgressDialogCloseButtonText", "<%= GetMessageJS("ProgressDialogCloseButtonText") %>");
	iu.addParam("ProgressDialogCloseWhenUploadCompletesText", "<%= GetMessageJS("ProgressDialogCloseWhenUploadCompletesText") %>");
	iu.addParam("ProgressDialogEstimatedTimeText", "<%= GetMessageJS("ProgressDialogEstimatedTimeText") %>");
	iu.addParam("ProgressDialogPreparingDataText","<%= GetMessageJS("ProgressDialogPreparingDataText") %>");
	iu.addParam("ProgressDialogSentText","<%= GetMessageJS("ProgressDialogSentText") %>");
	iu.addParam("ProgressDialogTitleText","<%= GetMessageJS("ProgressDialogTitleText") %>");
	iu.addParam("ProgressDialogWaitingForResponseFromServerText","<%= GetMessageJS("ProgressDialogWaitingForResponseFromServerText") %>");
	iu.addParam("ProgressDialogWaitingForRetryText","<%= GetMessageJS("ProgressDialogWaitingForRetryText") %>");
	iu.addParam("RemoveIconTooltipText","<%= GetMessageJS("RemoveIconTooltipText") %>");
	iu.addParam("SecondsText", "<%= GetMessageJS("SecondsText") %>");
	iu.addParam("DropFilesHereText","<%= GetMessageJS("DropFilesHereText") %>");
	
	iu.addParam("FileMask", "*.jpeg;*.jpg;*.jpe;*.gif;*.png;*.bmp;");
	iu.addParam("LicenseKey", "Bitrix");
	
	if (!IUCommon.browser.isOpera)
	{
		iu.addParam("MessageRetryOpenFolderText","<%= GetMessageJS("MessageRetryOpenFolderText") %>");
		iu.addParam("MessageRedirectText", "<%= GetMessageJS("MessageRedirectText") %>");
		iu.addParam("MessageSwitchAnotherFolderWarningText", "<%= GetMessageJS("MessageSwitchAnotherFolderWarningText") %>");
		iu.addParam("MessageDimensionsAreTooLargeText", "<%= GetMessageJS("MessageDimensionsAreTooLargeText") %>");
		iu.addParam("MessageNoInternetSessionWasEstablishedText","<%= GetMessageJS("MessageNoInternetSessionWasEstablishedText") %>");
		iu.addParam("UnixFileSystemRootText","<%= GetMessageJS("UnixFileSystemRootText") %>");
		iu.addParam("UnixHomeDirectoryText", "<%= GetMessageJS("UnixHomeDirectoryText") %>");
	}
	iu.writeHtml();
}


function AddCookies()
{
	try
	{
		var iu = getImageUploader("ImageUploader1");<% 
	for(int i = 0; i < Request.Cookies.Count; i++) 
	{ 
		HttpCookie c = Request.Cookies[i];
%>
		iu.AddCookie("<%= c.Name %>=<%= c.Value %>");<% 
	
	} 
%>
	}
	catch(e)
	{
	}
}

</script>



<script type="text/javascript">
bInitThumbnailWriter = false;
function to_init_thumb()
{
	is_loaded = false;
	try
	{
		if (PUtilsIsLoaded == true)
			is_loaded = true;
	}
	catch(e){}
	
	if (is_loaded)
	{
		if (!bInitThumbnailWriter && InitThumbnailWriter)
		{
			InitThumbnailWriter();
			bInitThumbnailWriter = true;
		}
	}
	if (!bInitThumbnailWriter)
		setTimeout(to_init_thumb, 100);
	return;
}

function InitThumbnailWriter()
{
	t = new ThumbnailWriter("Thumbnail1", 120, 120);
	t.addParam("BackgroundColor", "#d8d8d8");
	//For ActiveX control full path to CAB file (including file name) should be specified.
	t.activeXControlCodeBase = "<%= Bitrix.BXUri.ToRelativeUri("~/bitrix/image_uploader/ImageUploader.cab") %>";
	t.activeXClassId = "58C8ACD5-D8A6-4AC8-9494-2E6CCF6DD2F8";
	t.activeXControlVersion = "3,5,204,0";
	//For Java applet only path to directory with JAR files should be specified (without file name).
	t.javaAppletCodeBase = "<%= Bitrix.BXUri.ToRelativeUri("~/bitrix/image_uploader/") %>";
	t.javaAppletJarFileName="ImageUploader.jar"; 
	t.javaAppletCached = true;
	t.javaAppletVersion = "1.1.81.0";
	
	t.addParam("ParentControlName", "ImageUploader1");
	t.writeHtml();
}
</script>

<table width="100%" border="0" cellspacing="0">
    <tr valign="top">
        <td align="left" colspan="2">
            <div>
                            <input type="button" class="c" id="AddFolders" onclick="if ((jsUtils.IsIE() && getImageUploader('ImageUploader1')) || (!jsUtils.IsIE() && getImageUploader('ImageUploader1') && getImageUploader('ImageUploader1').AddFolders)) {getImageUploader('ImageUploader1').AddFolders();}"
								value=" <%= GetMessage("AddFolder")%>" />      
            </div>
            <div>

                <input type="button" class="c" id="AddFiles" onclick="if ((jsUtils.IsIE() && getImageUploader('ImageUploader1')) || (!jsUtils.IsIE() && getImageUploader('ImageUploader1') && getImageUploader('ImageUploader1').AddFiles)) {getImageUploader('ImageUploader1').AddFiles();}"
                    value=" <%= GetMessage("AddFiles")%>" />
                         
            </div>
            <div>
                <input type="button" class="c" id="Clear" 
                    onclick="if ((jsUtils.IsIE() && getImageUploader('ImageUploader1')) || (!jsUtils.IsIE() && getImageUploader('ImageUploader1') && getImageUploader('ImageUploader1').RemoveAllFromUploadList))  {getImageUploader('ImageUploader1').RemoveAllFromUploadList();}"
                                       value=" <%= GetMessage("Clear")%>" />
            </div>
        </td>
    </tr>
    <tr>
        <td colspan="2">

            <script type="text/javascript">
                to_init();
            </script>

        </td>
    </tr>
    <tr>
        <td colspan="2">
            <div class="hr">
            </div>
        </td>
    </tr>
    <tr class="settings">
        <td width="100%">
            <%= GetMessage("PhotoSettings") %></td>
    </tr>
    <tr>
        <td id="description" colspan="2">
            <table width="100%">
                <tr valign="top">
                    <td valign="middle">

                        <script>to_init_thumb();</script>

                    </td>
                    <td width="76%">
                        <%= GetMessage("Legend.Title") %><br />
                        <input style="width: 99%" name="Title" id="PhotoTitle" class="Title" type="text" />
                        <br />
                        <%= GetMessage("Legend.Description") %><br />
                        <textarea style="width: 99%" rows="5" name="Description" id="PhotoDescription" class="Description"></textarea>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
    <tr class="settings">
        <td width="100%">
            <%= GetMessage("Legend.UploadingParameters") %></td>
    </tr>
    <tr>
        <td align="left" colspan="2">
            <div id="photo_resize_div">
                <%= GetMessage("Legend.UploadPhotosInSize") %>
                <select name="photo_resize_size" id="photo_resize_size">
                    <option value="0" selected><%= GetMessage("Legend.Original") %></option>
                    <option value="1">1024x768</option>
                    <option value="2">800x600</option>
                    <option value="3">640x480</option>
                </select>
            </div>
            <div id="photo_watermark">
                <%= GetMessage("Legend.Watermark") %>
                <input type="text" id="watermark" name="watermark" />
            </div>
            <div id="photo_albums_to_move">
                <%= GetMessage("Legend.Album") %>
                <asp:DropDownList runat="server" ID="lbSections">
                </asp:DropDownList>
            </div>
        </td>
    </tr>
    <tr>
        <td colspan="2">
            <table width="100%">
                <tr>
                    <td>
                        <div id="Send">
                                     <input type="button" class="c" id="SendColor"
                                       value=" <%= GetMessage("Upload")%>" />
                        </div>
                        <div id="CancelSend">
                                    <input type="button" class="c" id="CancelSendColor" onclick="GoBack();"
                                       value=" <%= GetMessage("Cancel")%>" />
                        </div>                         
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>

<script type="text/javascript" language="javascript">

    var AlbumSelector = document.getElementById("<%= lbSections.ClientID %>");
    window.GoBack = function()
    {
        window.location.href = "<%= Results["BACK_URL"] %>";
    }    
</script>

<div style="visibility: hidden" class="photo-bold" id="photo_count_to_upload_div">
    <div id="photo_count_to_upload">
    </div>
    <%= GetMessage("Photo") %>
</div>
<%}
  else
  { %>
<%= GetMessage("YouDontHaveRightsToPerformThisAction") %> <a href="?<%= Parameters.Get<string>("UrlAlbum","album") %>=<%= Parameters.Get<int>("AlbumId",0) %>"
    enableajax="true"><%= GetMessage("Return") %></a>
<%} %>
