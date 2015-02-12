<% if (UploadMode) {%><!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"><%}%>
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="edit.aspx.cs" Inherits="BXCustomTypeFileEditExtra" %>
<% if (JSCodeMode) {%>
//<script type="text/javascript">
function CustomTypeFile_ClearFile(sender, id)
{
	var sender = sender;
    var savedId = id + '_SavedName';
    var cachedId = id + '_CachedId';
    var clearId = id + '_ValueClear';    
    var uploadId = id + '_ValueUpload';
    var storedId = id + '_StoredId';
        
    
    var c = null;
    if (c = document.getElementById(savedId))
		c.innerHTML = '';
	if (c = document.getElementById(cachedId))
	{
		var req = new JCHttpRequest();
		req.Send('<% =JSEncode(VirtualPathUtility.ToAbsolute(AppRelativeVirtualPath)) %>?delete=' + encodeURIComponent(c.value));
		c.value = '';
	}	
	if (c = document.getElementById(storedId))
		c.value = '';	
	if (c = document.getElementById(clearId))
	{
		c.style.display = 'none';
		c.style.visibility = 'hidden';
	}
	if (c = document.getElementById(uploadId))
	{
		c.style.display = 'inline';
		c.style.visibility = 'visible';
	}
}

function CustomTypeFile_UploadFile(sender, id)
{
	var sender = sender;
	var frameId = id + '_Frame';
    var formId = id + '_Form';
    var savedId = id + '_SavedName';
    var cachedId = id + '_CachedId';
    var buttonTdId = id + '_ButtonPlaceholder';
    var clearId = id + '_ValueClear';    
    var ctypeId = id + '_CustomType';
    var uri = false;
    var placeholder = null;
    var buttonPlaceholder = null;
    var cachedName = null;
    
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
	var LoadComplete = function(evt)
	{	
		if (typeof(evt) != 'boolean')
			evt = evt ? evt : event;
		if (evt && evt.target && evt.target != io)
			return;
		//Safari hack
		if (!allowFire)
			return;
		if (loaded)
			return;
		loaded = true;
		
		if(window.detachEvent) // IE
			io.detachEvent('onload', LoadComplete);
		else 
		{
			io.removeEventListener('load', LoadComplete, false);
			//Mozilla hack for error catching
			window.removeEventListener("DOMFrameContentLoaded", LoadComplete, false);
		}
		//IE Hack to clear file input
		f.reset();
		
		setTimeout(function(){
			if (status)
			{
				status.style.display = "none";
				status.style.visibility = "hidden";
			}
			
			if (buttonPlaceholder && cancelButton)
				buttonPlaceholder.removeChild(cancelButton);
			
			f.removeChild(sender);
			
			document.body.removeChild(f);
			delete f;
			
			var success = false;
			try
			{
				if (io.contentWindow.success)
					success = true;
			}
			catch(e){}
			
			var errorMessage = '<% =JSEncode(HtmlEncode(GetMessageRaw("Error.Loading"))) %><br/>';
			try
			{
				if (typeof(io.contentWindow.errorMessage) == 'string')
					errorMessage = io.contentWindow.errorMessage;
			}
			catch(e){}
			
			document.body.removeChild(io);
			delete io;
			
			if (!success)
			{
				if (savedName)
				{
					savedName.style.color = 'red';
					savedName.innerHTML = errorMessage;
				}
				if (cachedName)
					cachedName.value = '';
			}
			
			sender.value = '';
			sender.value = null;
			if (placeholder)
			{
				placeholder.appendChild(sender);
				if (!success)
				{
					sender.style.display = "inline";
					sender.style.visibility = "visible";
				}
			}
			
			if (success)
			{
				var clearButton = document.getElementById(clearId);
				if (clearButton)
				{
					clearButton.style.display = 'inline';
					clearButton.style.visibility = 'visible';
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
    cachedName = document.getElementById(cachedId);
    f.action = '<% =JSEncode(VirtualPathUtility.ToAbsolute(AppRelativeVirtualPath)) %>?id=' + encodeURIComponent(id);
    if (cachedName && cachedName.value != '')
    f.action += '&old=' + encodeURIComponent(cachedName.value);
    f.target = frameId;
	document.body.appendChild(f);
	
	placeholder = sender.parentNode;
	placeholder.removeChild(sender);
	f.appendChild(sender);
	
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

	setTimeout(function() {
		//Attach Event Handler
		if (window.attachEvent) // IE
			io.attachEvent('onload', LoadComplete);
		else {
			io.addEventListener('load', LoadComplete, false);
			//Mozilla hack for error catching
			window.addEventListener("DOMFrameContentLoaded", LoadComplete, false);
		}
		setTimeout(function() {
			try {
				allowFire = true;
				f.submit();
			} catch (e) {
				LoadComplete(false);
			}
		}, 100);
	}, 100);
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

var cachedId = d.getElementById('<% =OwnerId %>_CachedId');
if (cachedId)
	cachedId.value = '<% =JSEncode(CachedId) %>';
	
var savedName = d.getElementById('<% =OwnerId %>_SavedName');
if (savedName)
{
	savedName.style.color = '';
	savedName.innerHTML = 
		'<% =JSEncode(HtmlEncode(GetMessageRaw("File")))%>: <% =JSEncode(HtmlEncode(Uploaded.FileNameOriginal)) %><br/>'
		+ '<% =JSEncode(HtmlEncode(GetMessageRaw("Size")))%>: <% =JSEncode(HtmlEncode(Bitrix.Services.Text.BXStringUtility.BytesToString(Uploaded.FileSize))) %><br/>';
}
	
window.success = true;	
</script>
<% } else {%>
<script type="text/javascript">
var r = parent;
var d = r.document;

window.errorMessage = '<% =ErrorMessage %>';
</script>
<% } %>
</body>
</html>
<% } %>