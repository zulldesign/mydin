if (typeof (Bitrix) == "undefined") {
    var Bitrix = new Object();
}

Bitrix.PhotoGalleryPhoto = function() {
    this._image = null;
}

Bitrix.PhotoGalleryPhoto.prototype.Init = function(image) {
    this._image = image;
}

Bitrix.PhotoGalleryPhoto.prototype.SetUrl = function(url) {
    this._image.src = url;
   }

try
{
if (Sys && Sys.Application)
	Sys.Application.notifyScriptLoaded();
}
catch (e) { }

