<% if (UploadMode) {%><!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"><%}%>
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SilverlightFileUploadHandler.aspx.cs" Inherits="BXAjaxFileUploadHandler" %>
<% if (JSCodeMode) {%>
//<script type="text/javascript">
function SilverlightFileUploadHandler_ClearFile(sender, id,delCheckBoxId)
{
	var sender = sender;
    var clearId = id + '_ValueClear';
    var displayId = id + '_DisplayName';
    var sizeId = id + '_DisplaySize';
    var uploadId = id + '_ValueUpload';
    var uploadPlaceHolderId = id + '_ValuePlaceholder';
    var storedId = id + '_StoredId';
    var slContainerId = id + '_SwfContainer_SilverlightControl';
    var slParentId = id + "_SwfContainer";
    var hfFilePathId = id + "_hfFilePath";
    var heightCaptionId = id + "_heightCaption";
    var widthCaptionId = id + "_widthCaption";
    var fileDescId = id + "_FileDescription";
    var lblId = id + "_Lbl";
    
    
    
    var c = null;
	if (c = document.getElementById(displayId)) {
	    c.innerText = '';
	    c.textContent = '';
	}
	if (c = document.getElementById(storedId))
		c.value = '';
    if (c = document.getElementById(sizeId)) 
    {
        c.innerText = '';
        c.textContent = '';
    }
    if (c = document.getElementById(widthCaptionId)) {
        c.innerText = '';
        c.textContent = '';
    }
    if (c = document.getElementById(fileDescId)) {
        c.style.display = 'none';
        c.style.visibility = 'hidden';
    }
    if (c = document.getElementById(heightCaptionId)) {
        c.innerText = '';
        c.textContent = '';
    }
	if (c = document.getElementById(clearId))
	    {
		    c.style.display = 'none';
		    c.style.visibility = 'hidden';
    }
    if (c = document.getElementById(lblId)) {
        c.style.display = 'inline';
        c.style.visibility = 'visible';
    }
    var p;
    if ((c = document.getElementById(slContainerId)) && (p = document.getElementById(slParentId))) {
        p.removeChild(c);
        p.style.width = "1px";
        p.style.height = "1px";
    }
    if (c = document.getElementById(uploadPlaceHolderId)) {
        c.style.display = 'inline';
        c.style.visibility = 'visible';
    }
	if (c = document.getElementById(uploadId))
	{
		c.style.display = 'inline';
		c.style.visibility = 'visible';
    }

    if (c = document.getElementById(hfFilePathId))
        c.value = "";

    if (c = document.getElementById(delCheckBoxId))
        c.checked = true;

    if (!Silverlight.isInstalled()) {
        p = document.getElementById(slParentId);
        if (p) {
            while (p.hasChildNodes())
                p.removeChild(p.lastChild);
        }
    }

    var msg = document.getElementById(id +'_UploadErrorMessage');
    if (msg) {
        msg.innerText = "";
        msg.textContent = "";
        msg.style.visibilty = "hidden";
        msg.style.display = "none";
    }

}

function SilverlightFileUploadHandler_UploadFile(sender, id, delCheckBoxId)
{
	var sender = sender;
	var frameId = id + '_Frame';
    var formId = id + '_Form';
    var savedId = id + '_SavedName';
    var buttonTdId = id + '_ButtonPlaceholder';
    var clearId = id + '_ValueClear';
    var displayId = id + '_DisplayName';
    var csrfTokenId = id + '_csrfToken';
    var errorMsgId = id + '_UploadErrorMessage';
    var slContainerId = id + '_SwfContainer';
    var uri = false;
    var placeholder = null;
    var buttonPlaceholder = null;

    //check if its XAP file before upload

    var val = sender.value;

    var ext = val.substring(val.lastIndexOf("."));

    if (ext.toLowerCase() != ".xap") {
        var err = document.getElementById(errorMsgId);
        if (err) {
            err.style.display = 'block';
            err.style.visibility = 'visible';
            err.innerText = '<%=JSEncode(HtmlEncode(GetMessageRaw("Error.IncorrectFileExtension"))) %>';
            err.textContent = '<%=JSEncode(HtmlEncode(GetMessageRaw("Error.IncorrectFileExtension"))) %>';
        }
        sender.value = ""; 
        return;   
    }
    
	sender.style.display = "none";
	sender.style.visibility = "hidden";
	
	var status = document.getElementById(id + "_Loading");
	if (status)
	{
		status.style.display = "inline";
		status.style.visibility = "visible";
	}
	
	var savedName = document.getElementById(savedId);
	if (savedName)
		savedName.innerHTML = ''; 
		
	var displayName = document.getElementById(displayId);
	if (displayName)
		displayName.value = '';
	
	//create frame
    if(window.ActiveXObject) {
        var io = document.createElement('<iframe id="' + frameId + '" name="' + frameId + '" />');
        if(typeof uri== 'boolean')
            io.src = 'javascript:void(0)';
        else if(typeof uri== 'string')
            io.src = uri;
    }
    else {
        var io = document.createElement('iframe');
        io.id = frameId;
        io.name = frameId;
    }
    //Don't set display: none because of Safari
    io.style.position = 'absolute';
    io.style.top = '-1000px';
    io.style.left = '-1000px';
    io.style.width = "1px";
    io.style.height = "1px";
    document.body.appendChild(io);
	//Load Complete Event;
	var loaded = false;
	var allowFire = false;
	var LoadComplete = function(evt) {
	    if (typeof (evt) != 'boolean')
	        evt = evt ? evt : event;
	    if (evt && evt.target && evt.target != io)
	        return;
	    //Safari hack
	    if (!allowFire)
	        return;
	    if (loaded)
	        return;
	    loaded = true;

	    if (window.detachEvent) // IE
	        io.detachEvent('onload', LoadComplete);
	    else {
	        io.removeEventListener('load', LoadComplete, false);
	        //Mozilla hack for error catching
	        window.removeEventListener("DOMFrameContentLoaded", LoadComplete, false);
	    }
	    //IE Hack to clear file input
	    f.reset();

	    setTimeout(function() {
	        if (status) {
	            status.style.display = "none";
	            status.style.visibility = "hidden";
	        }

	        if (buttonPlaceholder && cancelButton)
	            buttonPlaceholder.removeChild(cancelButton);

	        f.removeChild(sender);

	        document.body.removeChild(f);
	        delete f;

	        var success = false;
	        try {
	            if (io.contentWindow.success)
	                success = true;
	        }
	        catch (e) { }

	        var errorMessage = '<% =JSEncode(HtmlEncode(GetMessageRaw("Error.Loading"))) %><br/>';
	        try {
	            if (typeof (io.contentWindow.errorMessage) == 'string')
	                errorMessage = io.contentWindow.errorMessage;
	        }
	        catch (e) { }

	        document.body.removeChild(io);
	        delete io;

	        sender.value = '';
	        sender.value = null;
	        if (placeholder) {
	            placeholder.appendChild(sender);
	            if (!success) {
	                sender.style.display = "inline";
	                sender.style.visibility = "visible";
	            }
	        }

	        if (success) {
	            var clearButton = document.getElementById(clearId);
	            if (clearButton) {
	                clearButton.style.display = 'inline';
	                clearButton.style.visibility = 'visible';
	            }

	            if (c = document.getElementById(delCheckBoxId))
	                c.checked = false;
	        }
	        else {
	            if (evt) {
	                var msg = document.getElementById(errorMsgId);
	                if (msg) {

	                    msg.innerText = '<% =JSEncode(HtmlEncode(GetMessageRaw("Error.Loading"))) %>';
	                    msg.textContent = '<% =JSEncode(HtmlEncode(GetMessageRaw("Error.Loading"))) %>';
	                    msg.style.display = "block";
	                    msg.style.visibilty = "visible";
	                }
	            }
	        }
	    }, 1);
	}

	
	//Create Form
    var f = document.createElement('form');
    f.style.position = 'absolute';
    f.style.top = '-1000px';
    f.style.left = '-1000px';
    f.style.width = "1px";
    f.style.height = "1px";
    f.style.display = "none";
    f.style.visibility = "hidden";
    f.method = "POST";
    if(f.encoding)
        f.encoding = 'multipart/form-data';				
    else
        f.enctype = 'multipart/form-data';
    f.action = '<% =JSEncode(VirtualPathUtility.ToAbsolute(AppRelativeVirtualPath)) %>?id=' + encodeURIComponent(id);
    f.target = frameId;
	document.body.appendChild(f);
	
	placeholder = sender.parentNode;
	placeholder.removeChild(sender);
	f.appendChild(sender);

	//Create hidden field with Csrf token


	var hf_sender = document.getElementById(csrfTokenId);
	if (hf_sender) {
	    var hf = document.createElement("input");
	    hf.type = "hidden";
	    hf.name = "csrfToken";
	    hf.value = hf_sender.value;
	    f.appendChild(hf);
	}
	
	//Create Cancel button
	var cancelButton = document.createElement('input');
	cancelButton.type = 'button';
	cancelButton.value = 'X';
	cancelButton.onclick = function () {
		LoadComplete(false);
		return false;
	};
	buttonPlaceholder = document.getElementById(buttonTdId);
	if (buttonPlaceholder)
		buttonPlaceholder.appendChild(cancelButton);
	
	setTimeout(function(){
		//Attach Event Handler
		if(window.attachEvent) // IE
			io.attachEvent('onload', LoadComplete);
		else
		{
			io.addEventListener('load', LoadComplete, false);
			//Mozilla hack for error catching
			window.addEventListener("DOMFrameContentLoaded", LoadComplete, false);
		}
		setTimeout(function(){
			try 
			{
				allowFire = true;
				f.submit();	
			} catch(e)
			{
				LoadComplete(false);
			}
		}, 1);
	}, 1);
}
//</script>
<% } else if (UploadMode) { %>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head><title></title></head>
<body>
<% if (!HasError) { %>
<script type="text/javascript">
var r = parent;
var d = r.document;
	
var displayName = d.getElementById('<% =OwnerId %>_DisplayName');
if (displayName) {
    displayName.innerHtml = '<% =JSEncode(SavedName) %>';
    displayName.textContent = '<% =JSEncode(SavedName) %>';
}

var displaySize = d.getElementById('<% =OwnerId %>_DisplaySize');
if (displaySize) {
    displaySize.innerText = '<% =JSEncode(DisplaySize) %>';
    displaySize.textContent = '<% =JSEncode(DisplaySize) %>';
}

var filePath = '<%=JSEncode(FilePath) %>';
var hfFilePath = d.getElementById('<%=OwnerId %>_hfFilePath');
var hfFileName = d.getElementById('<%=OwnerId %>_hfFileName');
var hfFileContentType = d.getElementById('<%=OwnerId %>_hfFileContentType');

var fileDesc = d.getElementById('<%=OwnerId%>_FileDescription');

if (fileDesc) {
    fileDesc.style.display = 'block';
    fileDesc.style.visibility = 'visible';
}
var c = d.getElementById('<%=OwnerId%>_Lbl');

if (c) {
    c.style.display = 'none';
    c.style.visibility = 'hidden';
}

if (hfFileContentType)
    hfFileContentType.value = '<% =JSEncode(HtmlEncode(FileContentType)) %>';

if (hfFileName)
    hfFileName.value = '<% =JSEncode(HtmlEncode(SavedName)) %>';

r.Bitrix.SilverlightUtility.getInstance().createElement('<%=OwnerId %>_SwfContainer', filePath, 1, 1, '<%=OwnerId %>_slwidth', '<%=OwnerId %>_slheight', 
            '<%=OwnerId %>_widthCaption', '<%=OwnerId %>_heightCaption',null,null,'<%=OwnerId%>_UploadErrorMessage');
hfFilePath.value = filePath;

var msg = d.getElementById('<% =OwnerId %>_UploadErrorMessage');
if (msg) {
    msg.innerText = "";
    msg.textContent = "";
    msg.style.visibilty = "hidden";
    msg.style.display = "none";
}
	
window.success = true;	
</script>
<% } else {%>
<script type="text/javascript">
var r = parent;
var d = r.document;

var msg = d.getElementById('<% =OwnerId %>_UploadErrorMessage');
if (msg) {
    msg.innerText = '<% =ErrorMessage %>';
    msg.textContent = '<% =ErrorMessage %>';
    msg.style.visibilty = "visible";
    msg.style.display = "block";
}

</script>
<% } %>
</body>
</html>
<% } %>

