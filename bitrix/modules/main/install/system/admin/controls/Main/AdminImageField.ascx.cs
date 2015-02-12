using System;

using Bitrix.UI;
using System.ComponentModel;
using Bitrix.Services;
using System.Web.UI.WebControls;
using Bitrix.Services.Js;
using Bitrix.IO;
using Bitrix.Services.Text;


public partial class bitrix_ui_AdminImageField : BXAdminImageField
{
	//FIELDS
	BXFile imageFile;
	int imageId;

    string newImageUploadingHint = string.Empty;
    #region Attributes
    [DefaultValue(0)]
    #endregion
    public string NewImageUploadingHint
    {
        get
        {
            return newImageUploadingHint;
        }
        set
        {
            newImageUploadingHint = value;
        }
    }
	//PROPERTIES
	#region Attributes
	[DefaultValue(0)]
	#endregion
	public int MaxImageWidth
	{
		get
		{
			object o = ViewState["MaxImageWidth"];
			return (o == null) ? 0 : (int)o;
		}
		set
		{
			ViewState["MaxImageWidth"] = value;
		}
	}
	#region Attributes
	[DefaultValue(0)]
	#endregion
	public int MaxImageHeight
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
	#region Attributes
	[DefaultValue(false)]
	#endregion
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
	#region Attributes
	[DefaultValue("")]
	#endregion
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
	#region Attributes
	[DefaultValue(true)]
	#endregion
	public bool Editable
	{
		get { return editable; }
		set { editable = value; }
	}

	#region Attributes
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	#endregion
	public override BXFile ImageFile
	{
		get
		{
			return imageFile;
		}
		set
		{
			imageFile = value;
			StoreImage();
		}
	}

	public int ImageId
	{
		get
		{
			return imageId;
		}

		set
		{
			if (value > 0)
			{
				ImageFile = BXFile.GetById(value, BXTextEncoder.EmptyTextEncoder);
				if (imageFile != null)
					imageId = imageFile.Id;
			}
		}

	}

	public override FileUpload GetUpload()
	{
		return Up;
	}

	#region Attributes
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	#endregion
	public FileUpload Upload
	{
		get
		{
			return Up;
		}
	}
	#region Attributes
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	#endregion
	public override bool DeleteFile
	{
		get
		{
			return Del.Checked;
		}
	}
	
	#region Attributes
	[DefaultValue(0)]
	#endregion
	int ImageWidth
	{
		get
		{
			object o = ViewState["ImageWidth"];
			return (o == null) ? 0 : (int)o;
		}
		set
		{
			ViewState["ImageWidth"] = value;
		}
	}
	#region Attributes
	[DefaultValue(0)]
	#endregion
	int ImageHeight
	{
		get
		{
			object o = ViewState["ImageHeight"];
			return (o == null) ? 0 : (int)o;
		}
		set
		{
			ViewState["ImageHeight"] = value;
		}
	}
	#region Attributes
	[DefaultValue(0)]
	#endregion
	int ImageSize
	{
		get
		{
			object o = ViewState["ImageSize"];
			return (o == null) ? 0 : (int)o;
		}
		set
		{
			ViewState["ImageSize"] = value;
		}
	}
	#region Attributes
	[DefaultValue(0)]
	#endregion
	string ImagePath
	{
		get
		{
			object o = ViewState["ImagePath"];
			return (o == null) ? string.Empty : (string)o;
		}
		set
		{
			ViewState["ImagePath"] = value;
		}
	}
	#region Attributes
	[DefaultValue(0)]
	#endregion
	bool IsImage
	{
		get
		{
			object o = ViewState["IsImage"];
			return (o == null) ? false : (bool)o;
		}
		set
		{
			ViewState["IsImage"] = value;
		}
	}

	public override void SetMaxImageDisplaySize(int maxHeight, int maxWidth)
	{
		if (maxHeight > 0)
			MaxImageHeight = maxHeight;
		if (maxWidth > 0)
			MaxImageWidth = maxWidth;
	}


	protected void Page_PreRender(object sender, EventArgs e)
	{
		UpdateDisplay();
	}
	void StoreImage()
	{
		if (imageFile == null)
		{
			ImagePath = null;
			Description = null;
			return;
		}

		ImagePath = imageFile.TextEncoder.Decode(imageFile.FilePath);
		ImageSize = imageFile.FileSize;
		Description = imageFile.TextEncoder.Decode(imageFile.Description);
		IsImage = imageFile.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase) && imageFile.Width > 0 && imageFile.Height > 0;
		if (IsImage)
		{
			ImageWidth = imageFile.Width;
			ImageHeight = imageFile.Height;
		}
	}
	void UpdateDisplay()
	{
		S.Visible = ShowDescription;
		
		if (!string.IsNullOrEmpty(ImagePath))
		{
			D.Visible = true;
			Lbl.Visible = true;
			if (IsImage)
			{
				int w = ImageWidth;
				int h = ImageHeight;
				ImageDisplay.ImageUrl = ImagePath;
				ImageParameters.Text = string.Format(
					"{0}: {1}<br/>{2}: {3}<br/>{4}: {5}<br/>{6}: {7}<br/>",
					GetMessage("LiteralTextPartFile.ImageParameters"), Encode(ImagePath),
					GetMessage("LiteralTextPartWidth.ImageParameters"), w,
					GetMessage("LiteralTextPartHeight.ImageParameters"), h,
					GetMessage("LiteralTextPartSize.ImageParameters"), Encode(BXStringUtility.BytesToString(ImageSize))
					);

				if (MaxImageWidth > 0 && w > MaxImageWidth)
				{
					h = h * MaxImageWidth / w;
					w = MaxImageWidth;
				}

				if (MaxImageHeight > 0 && h > MaxImageHeight)
				{
					w = w * MaxImageHeight / h;
					h = MaxImageHeight;
				}

				ImageDisplay.Width = w;
				ImageDisplay.Height = h;

				Img.HRef = ImagePath;
				Img.Attributes["onclick"] = string.Format(
					"ImgShw('{0}','{1}','{2}', ''); return false;",
					BXJSUtility.Encode(ImagePath),
					ImageWidth,
					ImageHeight
					);
			}
			else
			{
				ImageParameters.Text = string.Format(
					"{0}: {1}<br/>{2}: {3}<br/><br/><b>{4}</b>",
					GetMessageRaw("LiteralTextPartFile.ImageParameters"), Encode(ImagePath),
					GetMessageRaw("LiteralTextPartSize.ImageParameters"), Encode(BXStringUtility.BytesToString(ImageSize)),
					GetMessageRaw("LiteralTextPartIsNotImage.ImageParameters")
					);
				Img.Visible = false;
			}
		}
	}
}
