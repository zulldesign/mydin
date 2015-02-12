using System;
using Bitrix.UI;
using System.ComponentModel;
using Bitrix.Services;
using System.Web.UI.WebControls;
using Bitrix.Services.Js;
using Bitrix.IO;
using Bitrix.Services.Text;
using System.IO;
using System.Web.Hosting;


public partial class bitrix_ui_AdminFileField : BXControl
{
    //FIELDS
    private BXFile _file;
    int _fileId;

    //PROPERTIES
    [DefaultValue(0)]
    public int MaxFileWidth
    {
        get
        {
            object o = ViewState["MaxFileWidth"];
            return (o == null) ? 0 : (int)o;
        }
        set
        {
            ViewState["MaxImageWidth"] = value;
        }
    }
    [DefaultValue(0)]
    public int MaxFileHeight
    {
        get
        {
            object o = ViewState["MaxImageHeight"];
            return (o == null) ? 0 : (int)o;
        }
        set
        {
            ViewState["MaxImageHeight"] = value;
        }
    }

    [DefaultValue(false)]
    public bool ShowDescription
    {
        get
        {
            object o = ViewState["ShowDescription"];
            return (o == null) ? false : (bool)o;
        }
        set
        {
            ViewState["ShowDescription"] = value;
        }
    }
    [DefaultValue("")]
    public string Description
    {
        get
        {
            if (ShowDescription)
                return Desc.Text;
            return string.Empty;
        }
        set
        {
            if (ShowDescription)
                Desc.Text = value;
        }
    }

    private bool editable = true;
    [DefaultValue(true)]
    public bool Editable
    {
        get { return editable; }
        set { editable = value; }
    }

	private string moduleId;
	[DefaultValue("")]
	public string ModuleId
	{
		get
		{
			return moduleId ?? "";
		}
		set
		{
			moduleId = value;
		}
	}

	private string subFolder;
	[DefaultValue("")]
	public string SubFolder
	{
		get
		{
			return subFolder ?? "";
		}
		set
		{
			subFolder = value;
		}
	}
    private string labelTextMessage;
    
    [DefaultValue("QuestionText.AboutNewImageUploading")]
    public string LabelTextMessage
    {
        get
        {
            return labelTextMessage ?? "QuestionText.AboutNewImageUploading";
        }
        set
        {
            labelTextMessage = value;
        }
    }

    private bool _isFileGotById = false;
    private bool _ignorePostedFile = false;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BXFile File
    {
        get
        {
            if (_file != null || _isFileGotById)
                return _file;

            if (IsPostBack && !_ignorePostedFile && Up.HasFile)
            {
                _file = new BXFile(Up.PostedFile, SubFolder, !string.IsNullOrEmpty(ModuleId) ? ModuleId.ToLowerInvariant() : "main", null);
                _fileId = 0;
                _isFileGotById = false;
            }
            else if (IsPostBack && behaviour == BehaviourType.Silverlight && Request.Form[ClientID + "_hfFilePath"] != null && Request.Form[ClientID + "_hfFileName"] != null &&
                Request.Form[ClientID + "_hfFilePath"] != String.Empty)
            {
                _file = new BXFile(new FileStream(System.Web.Hosting.HostingEnvironment.MapPath(Request.Form[ClientID + "_hfFilePath"]),
                    FileMode.Open, FileAccess.Read, FileShare.Read), Request.Form[ClientID + "_hfFileName"],
                                        SubFolder, !string.IsNullOrEmpty(ModuleId) ? ModuleId.ToLowerInvariant() : "main", null);
                _file.ContentType = Request.Form[ClientID + "_hfFileContentType"];
            }
            else
            {
                if (_fileId > 0)
                    _file = BXFile.GetById(_fileId, BXTextEncoder.EmptyTextEncoder);
                _isFileGotById = true;
            }
            return _file;
        }
    }

    [DefaultValue(0)]
    string FilePath
    {
        get
        {
            BXFile f = File;
            return f != null ? f.FilePath : string.Empty;
        }
    }

    public int FileId
    {
        get
        {
            return _fileId;
        }

        set
        {
            if (_fileId == value)
                return;

            _fileId = value;
            _isFileGotById = false;
            _ignorePostedFile = true;
            _file = null;
        }

    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FileUpload Upload
    {
        get
        {
            return Up;
        }
    }
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AboutFileDeletion
    {
        get { return Del.Checked; }
    }

    [DefaultValue(0)]
    int FileWidth
    {
        get
        {
            BXFile f = File;
            return f != null ? f.Width : 0;
        }
    }

    [DefaultValue(0)]
    int FileHeight
    {
        get
        {
            BXFile f = File;
            return f != null ? f.Height : 0;
        }
    }
    [DefaultValue(0)]
    int FileSize
    {
        get
        {
            BXFile f = File;
            return f != null ? f.FileSize : 0;
        }
    }

    [DefaultValue(0)]
    private bool IsImage
    {
        get
        {
            BXFile f = File;
            return f != null ? f.ContentType.ToUpper().StartsWith("IMAGE", StringComparison.Ordinal) : false;
        }
    }

    private bool IsSwf
    {
        get
        {
            BXFile f = File;
            return f != null ? f.ContentType.ToUpper().Equals("APPLICATION/X-SHOCKWAVE-FLASH", StringComparison.Ordinal) : false;
        }
    }

    private bool IsSilverlight
    {
        get
        {
            BXFile f = File;
            return f != null ? System.IO.Path.GetExtension(f.FilePath).Equals(".XAP",StringComparison.OrdinalIgnoreCase) : false;
        }
    }

    private BehaviourType behaviour;

    [DefaultValue(BehaviourType.Standard)]
    public BehaviourType Behaviour
    {
        get { return behaviour; }
        set { behaviour = value; }
    }

    protected override void OnPreRender(EventArgs e)
    {
        S.Visible = ShowDescription;
        if (behaviour == BehaviourType.Silverlight)
        {
            Del.Style.Add("display", "none");
            
        }
        if (File!=null) Lbl.Style.Add("display", "none");

        if (!string.IsNullOrEmpty(FilePath) || behaviour == BehaviourType.Silverlight)
        {
            D.Visible = true;
            
            if (IsImage)
            {
                int w = FileWidth;
                int h = FileHeight;
                ImageDisplay.ImageUrl = FilePath;
                FileParameters.Text = string.Format(
                    "{0}: {1}<br/>{2}: {3}<br/>{4}: {5}<br/>{6}: {7}<br/>",
                    GetMessage("LiteralTextPartFile.ImageParameters"), Encode(FilePath),
                    GetMessage("LiteralTextPartWidth.ImageParameters"), w,
                    GetMessage("LiteralTextPartHeight.ImageParameters"), h,
                    GetMessage("LiteralTextPartSize.ImageParameters"), Encode(BXStringUtility.BytesToString(FileSize))
                    );

                if (MaxFileWidth > 0 && w > MaxFileWidth)
                {
                    h = h * MaxFileWidth / w;
                    w = MaxFileWidth;
                }

                if (MaxFileHeight > 0 && h > MaxFileHeight)
                {
                    w = w * MaxFileHeight / h;
                    h = MaxFileHeight;
                }

                ImageDisplay.Width = w;
                ImageDisplay.Height = h;

                Img.HRef = FilePath;
                Img.Attributes["onclick"] = string.Format(
                    "ImgShw('{0}','{1}','{2}', ''); return false;",
                    BXJSUtility.Encode(FilePath),
                    FileWidth,
                    FileHeight
                    );
            }
            else if (IsSwf)
            {

                int w = FileWidth;
                int h = FileHeight;
                ImageDisplay.ImageUrl = FilePath;
                FileParameters.Text = string.Format(
                    "{0}: {1}<br/>{2}: {3}<br/>{4}: {5}<br/>{6}: {7}<br/>",
                    GetMessage("LiteralTextPartFile.ImageParameters"), Encode(FilePath),
                    GetMessage("LiteralTextPartWidth.ImageParameters"), w,
                    GetMessage("LiteralTextPartHeight.ImageParameters"), h,
                    GetMessage("LiteralTextPartSize.ImageParameters"), Encode(BXStringUtility.BytesToString(FileSize))
                    );
                Img.Visible = false;

                Page.ClientScript.RegisterStartupScript(
                    GetType(),
                    string.Concat("SwfCreateClientObject_", ID),
                    string.Format(
                        "Bitrix.SwfUtility.getInstance().createElement('{0}', '{1}', {2}, {3}, null, null, null);",
                        SwfContainer.ClientID,
                        BXJSUtility.Encode(FilePath),
                        w,
                        h),
                    true
                    );
            }
            else if (IsSilverlight)
            {
                int w = FileWidth;
                int h = FileHeight;
                ImageDisplay.ImageUrl = FilePath;
                FileParameters.Text = string.Format(
                    "<div id=\"{12}\">{0}: <span id=\"{10}\">{1}</span><br/>{2}:<span id=\"{8}\">{3}</span><br/>{4}: <span id=\"{9}\">{5}</span><br/>{6}: <span id=\"{11}\">{7}</span><br/></div>",
                    GetMessage("LiteralTextPartFile.ImageParameters"), Encode(FilePath),
                    GetMessage("LiteralTextPartWidth.ImageParameters"), (w==0 ? String.Empty : w.ToString()),
                    GetMessage("LiteralTextPartHeight.ImageParameters"), (h==0 ? String.Empty : h.ToString()),
                    GetMessage("LiteralTextPartSize.ImageParameters"), Encode(BXStringUtility.BytesToString(FileSize)),
                    ClientID+"_widthCaption",ClientID+"_heightCaption",ClientID+"_DisplayName",ClientID+"_DisplaySize",ClientID+"_FileDescription"
                    );
                Img.Visible = false;

                Page.ClientScript.RegisterStartupScript(
                    GetType(),
                    string.Concat("SilverlightCreateClientObject_", ID),
                    string.Format(
                        "Bitrix.SilverlightUtility.getInstance().createElement('{0}', '{1}', {2}, {3}, '{4}', '{5}', '{6}','{7}',null,null,'{8}');",
                        SwfContainer.ClientID,
                        BXJSUtility.Encode(FilePath),
                        w,
                        h,
                        ClientID+"_slwidth",
                        ClientID + "_slheight",
                        ID + "_widthCaption", 
                        ID + "_heightCaption",
                        ClientID + "_UploadErrorMessage"
                        ),
                    true
                    );
            }
            else
            {
                if (behaviour == BehaviourType.Standard)

                    FileParameters.Text = string.Format(
                        "{0}: {1}<br/>{2}: {3}<br/>",
                        GetMessageRaw("LiteralTextPartFile.ImageParameters"),
                        Encode(FilePath),
                        GetMessageRaw("LiteralTextPartSize.ImageParameters"),
                        Encode(BXStringUtility.BytesToString(FileSize))
                        );
                else
                
                    FileParameters.Text = string.Format(
                   "<div style=\"display:none;\" id=\"{12}\">{0}: <span id=\"{10}\">{1}</span><br/>{2}:<span id=\"{8}\">{3}</span><br/>{4}: <span id=\"{9}\">{5}</span><br/>{6}: <span id=\"{11}\">{7}</span><br/></div>",
                   GetMessage("LiteralTextPartFile.ImageParameters"), String.Empty,
                   GetMessage("LiteralTextPartWidth.ImageParameters"), String.Empty,
                   GetMessage("LiteralTextPartHeight.ImageParameters"), String.Empty,
                   GetMessage("LiteralTextPartSize.ImageParameters"), String.Empty,
                   ClientID + "_widthCaption", ClientID + "_heightCaption", ClientID + "_DisplayName", ClientID + "_DisplaySize",ClientID+"_FileDescription"
                   );
                    
                
                Img.Visible = false;
            }
        }

        if (behaviour == BehaviourType.Silverlight)
			BXPage.RegisterScriptInclude("~/bitrix/admin/SilverlightFileUploadHandler.aspx?lang=" + UrlEncode(BXLoc.CurrentLocale));

        base.OnPreRender(e);
    }

	public enum BehaviourType
	{
		Standard = 0,
		Silverlight
	}
}
