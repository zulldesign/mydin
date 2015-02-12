function ShowSection(el) {
var bShow = el.className == "sign plus";
el.className = bShow ? "sign minus" : "sign plus";
var tr = jsUtils.FindParentObject(el,"tr");
var id = tr.id;
if ((tr = jsUtils.FindNextSibling(tr, "tr"))) {
if (tr.className && tr.className == "empty") {
//break;
}
if (bShow) {
try {
tr.style.display = "table-row";
} catch (e) {
tr.style.display = "block";
}
} else {
tr.style.display = "none";
}
}
}

//PShowWaitMessage("photo_note", true);
var temp = [];
var bShowApplet = false, bInitApplet = false;
var strCookie = "";//PhotoUtils.GetCookies('UploadShowMode');
if (typeof oText != "object")
	var oText = {};
	

// User functions
var PhotoClass =
{
    Uploader: null,
    FileCount: 0,
    Flags: {},
    oFile: {},
    _this: this,
    active: { "number": null, "guid": null },
    photo_album_id: 0,
    Init: function() {
        this.Uploader = getImageUploader("ImageUploader1");
        //PCloseWaitMessage("photo_note", true);
        //console.log(">PhotoClass is initialized...");
        this.Flags["CancelHideDescription"] = false;
    },

    ChangeFileCount: function() {
        if (this.Uploader) {
            var guid = 0;
            this.FileCount = this.Uploader.getUploadFileCount();
            this.Flags["CancelHideDescription"] = true;
            for (var i = 1; i <= this.FileCount; i++) {
                guid = this.Uploader.getUploadFileGuid(i);
                if (typeof (this.oFile[guid]) != "object" || (!this.oFile[guid]) || (this.oFile[guid] == null)) {
                    var sFileName = this.Uploader.getUploadFileName(i);
                    sFileName = sFileName.replace(/\\/g, '/');

                    var aFileName = sFileName.split('/');
                    if (aFileName.length > 0)
                        sFileName = aFileName[aFileName.length - 1];
                    this.oFile[guid] = { "Title": sFileName, "Tag": "", "Description": "" };
                }
            }

            if (this.FileCount <= 0) {
                document.getElementById("photo_count_to_upload").innerHTML = oText["NoPhoto"];
                if (document.getElementById("Send")) {
                    document.getElementById("Send").onclick = function() { return false; };
                    document.getElementById("SendColor").style.color = "gray";
                }
                this.Flags["SetButtonFunction"] = "N";
            }
            else {
                if (this.Flags["SetButtonFunction"] != "Y") {
                    if (document.getElementById("Send")) {
                        document.getElementById("Send").onclick = function() {
                            if (getImageUploader("ImageUploader1"))
                                getImageUploader("ImageUploader1").Send();
                            return;
                        }
                        document.getElementById("SendColor").style.color = "#4E4EA5";
                    }
                    this.Flags["SetButtonFunction"] = "Y";
                }

                document.getElementById("photo_count_to_upload").innerHTML = this.FileCount;
            }
        }
    },

    ChangeSelection: function() {
        var thumbnail1 = getImageUploader("Thumbnail1");
        if (this.Uploader && thumbnail1) {
            if (!this.Flags["CancelHideDescription"]) this.HideDescription();
            for (var i = 1; i <= this.Uploader.getUploadFileCount(); i++) {
                try {
                    if (this.Uploader.getUploadFileSelected(i)) {
                        this.active = {
                            "number": i,
                            "guid": this.Uploader.getUploadFileGuid(i)
                        };
                        break;
                    }
                }
                catch (e) {
                    alert('From ChangeSelection i=' + i + '\n\n' + e);
                }
            }
            this.Flags["CancelHideDescription"] = false;
            if ((this.active["number"] != null) && (typeof (this.active["number"]) == "number") && (typeof (this.oFile[this.active["guid"]]) != "undefined"))
                this.ShowDescription();
        }
    },

    ShowDescription: function() {
        var thumbnail1 = getImageUploader("Thumbnail1");
        if (this.Uploader && thumbnail1 && (typeof (this.active["number"]) == "number")) {
            document.getElementById("PhotoTitle").disabled = false;
            document.getElementById("PhotoTitle").value = this.oFile[this.active["guid"]]["Title"];
            if (document.getElementById("PhotoTag")) {
                document.getElementById("PhotoTag").disabled = false;
                document.getElementById("PhotoTag").value = this.oFile[this.active["guid"]]["Tag"];
            }
            document.getElementById("PhotoDescription").disabled = false;
            document.getElementById("PhotoDescription").value = this.oFile[this.active["guid"]]["Description"];

            thumbnail1.setGuid(this.active["guid"]);
        }
    },

    HideDescription: function() {
        var thumbnail1 = getImageUploader("Thumbnail1");
        if (this.Uploader && thumbnail1) {
            if (typeof (this.active["number"]) == "number") {
                this.oFile[this.active["guid"]]["Title"] = document.getElementById("PhotoTitle").value;
                if (document.getElementById("PhotoTag"))
                    this.oFile[this.active["guid"]]["Tag"] = document.getElementById("PhotoTag").value;
                this.oFile[this.active["guid"]]["Description"] = document.getElementById("PhotoDescription").value;
            }
            this.active = { "number": null, "guid": null };
            document.getElementById("PhotoTitle").disabled = true;
            document.getElementById("PhotoTitle").value = "";
            if (document.getElementById("PhotoTag")) {
                document.getElementById("PhotoTag").disabled = true;
                document.getElementById("PhotoTag").value = "";
            }
            document.getElementById("PhotoDescription").disabled = true;
            document.getElementById("PhotoDescription").value = "";
            thumbnail1.setGuid("");
        }
    },
	BeforeUpload:function(){
		  this.HideDescription();
		  try{
		  
		    // watermark
                font_color = "000000";
                if (document.getElementById("watermark_color"))
                    font_color = document.getElementById("watermark_color").value;
                font_color = "#" + font_color;
                font_size = "15";
                if (document.getElementById("watermark_fonts"))
                    font_size = document.getElementById("watermark_fonts").value;

                this.Uploader.setUploadThumbnail3Watermark("opacity=100;size=" + font_size + ";FillColor=" + font_color + ";text='" +
						document.getElementById("watermark").value.split("'").join("''") + "'");

                this.Uploader.setUploadThumbnail2Watermark("opacity=100;size=" + font_size + ";FillColor=" + font_color + ";text='" +
						document.getElementById("watermark").value.split("'").join("''") + "'");

                if (document.getElementById("photo_resize_size") && document.getElementById("photo_resize_size").value > 0) {
                    this.Uploader.setUploadThumbnail3FitMode("Fit");
                    if (document.getElementById("photo_resize_size").value == 1) {
                        this.Uploader.setUploadThumbnail3Width(1024);
                        this.Uploader.setUploadThumbnail3Height(1024);
                    }
                    else if (document.getElementById("photo_resize_size").value == 2) {
                        this.Uploader.setUploadThumbnail3Width(800);
                        this.Uploader.setUploadThumbnail3Height(800);
                    }
                    else if (document.getElementById("photo_resize_size").value == 3) {
                        this.Uploader.setUploadThumbnail3Width(640);
                        this.Uploader.setUploadThumbnail3Height(640);
                    }
                }
                else {
                    this.Uploader.setUploadThumbnail3FitMode("ActualSize");
                }

                if (document.getElementById("sessid"))
                    this.Uploader.AddField("sessid", document.getElementById("sessid").value);
                if (document.getElementById("photo_album_id")) {
                    this.Uploader.AddField("photo_album_id", document.getElementById("photo_album_id").value);
                    this.photo_album_id = document.getElementById("photo_album_id").value;
                }
                this.Uploader.AddField("save_upload", "Y");
                this.Uploader.AddField("AJAX_CALL", "Y");
                this.Uploader.AddField("CACHE_RESULT", "Y");
                this.Uploader.AddField("AlbumId", AlbumSelector.value);

				

                AddCookies();
                }
                catch(e){}
	},
    PackageBeforeUpload: function(PackageIndex) {
        if (this.Uploader) {
          
            try {
                var guid = 0;
                this.FileCount = this.Uploader.getUploadFileCount();

                for (var i = 1; i <= this.FileCount; i++) {

                    guid = this.Uploader.getUploadFileGuid(i);
                    this.Uploader.setUploadFileDescription(i, this.oFile[guid]["Description"]);
                    var sFileName = this.oFile[guid]["Title"];
                    sFileName = sFileName.replace(/\\/g, '/');

                    var aFileName = sFileName.split('/');
                    if (aFileName.length > 0)
                        sFileName = aFileName[aFileName.length - 1];
                    this.Uploader.AddField('Title_' + i, sFileName);
                    if (this.oFile[guid]["Tag"])
                        this.Uploader.AddField('Tags_' + i, this.oFile[guid]["Tag"]);
                }
              
            }
            catch (e) { }
        }
    },

    AfterUpload: function(htmlPage) {
        var result = {};
        var error = false;
        if (!document.getElementById("photo_error"))
            return;
        document.getElementById("photo_error").innerHTML = "";
        try {
            eval("result=" + htmlPage);
        }
        catch (e)
		{ }

        if (typeof result != "object")
            result = {};

        if (result["status"] == "success") {
            for (var key in result["files"]) {
                if (result["files"][key] && result["files"][key]["status"] != "success") {
                    if (result["files"][key]["error"]) {
                        document.getElementById("photo_error").innerHTML += result["files"][key]["error"] + " (" + key + ")<br />";
                        error = true;
                    }

                }
            }
        }
        else {
            if (result["error"]) {
                document.getElementById("photo_error").innerHTML = result["error"];
                error = true;
            }
        }

        if (!error) {
            if (this.photo_album_id > 0)
                jsUtils.Redirect([], window.urlRedirect.replace('#SECTION_ID#', this.photo_album_id));
            else
                jsUtils.Redirect([], window.urlRedirectThis);
        }
    },

    SendTags: function(oObj) {
        try {
            if (TcLoadTI) {
                if (typeof window.oObject[oObj.id] != 'object')
                    window.oObject[oObj.id] = new JsTc(oObj);
                return;
            }
            setTimeout(PhotoClass.SendTags(oObj), 10);
        }
        catch (e) {
            setTimeout(PhotoClass.SendTags(oObj), 10);
        }
    },

    ShowForm: function() {
        if (document.getElementById('table_photo_object') && document.getElementById('photo_form_table')) {
            if (bInitApplet) {
                document.getElementById('table_photo_object').style.display = '';
                document.getElementById('photo_form_table').style.display = 'none';
            }
            else {
                PCloseWaitMessage("photo_note", true);
                document.getElementById('table_photo_object').style.display = 'none';
                document.getElementById('photo_form_table').style.display = 'block';
            }
        }
        else {
            setTimeout(PhotoClass.ShowForm, 100);
        }
    },

    ChangeMode: function() {
        if (strCookie != "form")
            PhotoUtils.SetCookies('UploadShowMode', 'form');
        else
            PhotoUtils.DelCookies('UploadShowMode');
        return true;
    }
}

function PackageBeforeUploadLink(PackageIndex)
{
	PhotoClass.PackageBeforeUpload(PackageIndex);
}

function BeforeUploadLink()
{
	PhotoClass.BeforeUpload();
}

function AfterUploadLink(htmlPage)
{
	PhotoClass.AfterUpload(htmlPage);
}
function ChangeSelectionLink()
{
	PhotoClass.ChangeSelection();
}
function ChangeFileCountLink()
{
	PhotoClass.ChangeFileCount();
}
function InitLink()
{
	PhotoClass.Init();
}
PUtilsIsLoaded = true;