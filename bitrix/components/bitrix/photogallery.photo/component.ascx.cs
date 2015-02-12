using System;
using Bitrix;

using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.UI;
using System.Web.UI;
using Bitrix.DataTypes;
using Bitrix.Services;
using Bitrix.Configuration;
using System.Collections.Specialized;
using System.Web;
using Bitrix.Modules;
using Bitrix.IO;
using System.IO;
using Bitrix.Services.Image;

public partial class PhotogalleryPhoto : BXComponent
{
    Bitrix.Services.Image.BXImageInfo imageInfo;
    string errorMessage = string.Empty;
    //PROPERTIES
    public int PhotoId
    {
        get { return Results.Get("PhotoId", 0); }
    }

    public int AlbumId
    {
        get { return Results.Get("AlbumId", 0); }
        set { Results["AlbumId"] = value; }
    }

    public bool ModifyElements
    {
        get { return Results.Get("ModifyElements", false); }
        set { Results["ModifyElements"] = value; }
    }

    public string Description
    {
        get
        {
            return Results.Get("Description", "");
        }
        set
        {
            Results["Description"] = value;
        }
    }

    public string PhotoTitle
    {
        get
        {
            return Results.Get("title", "");
        }

        set
        {
            Results["title"] = value;
        }
    }

    public string BackUrl
    {
        get
        {
            return Results.Get("BACK_URL", "");
        }

        set
        {
            Results["BACK_URL"] = value;
        }
    }

    public string EditUrl
    {
        get
        {
            return Results.Get("EDIT_URL", "");
        }

        set
        {
            Results["EDIT_URL"] = value;
        }
    }


    public string PhotoOriginalUrl
    {
        get
        {
            return Results.Get("OriginalUrl", "");
        }

        set
        {
            Results["OriginalUrl"] = value;
        }
    }

    public string PhotoOriginalSizeText
    {
        get
        {
            return Results.Get("OriginalSize", "");
        }

        set
        {
            Results["OriginalSize"] = value;
        }
    }

    public string PhotoAuthor
    {
        get
        {
            return Results.Get("AUTHOR", "");
        }

        set
        {
            Results["AUTHOR"] = value;
        }
    }

    public string PhotoUrl
    {
        get
        {
            if (Photo == null || Photo.DetailImageId <= 0)
                return string.Empty;

            return BuildPhotoUrl();
        }
    }

    protected string BuildPhotoUrl()
    {
        return (imageInfo == null) ? String.Empty : imageInfo.GetUri();
    }

    public BXIBlockElement Photo
    {
        get
        {
            return Results.Get<BXIBlockElement>("CurrentPhoto", null);
        }
        set
        {
            Results["CurrentPhoto"] = value;
        }
    }

    public int PhotoWidth
    {
        get 
        {
            return Parameters.Get<int>("PhotoWidth", 150);
        }
        set 
        {
            if (Parameters.ContainsKey("PhotoWidth"))
                Parameters["PhotoWidth"] = value.ToString();
            else
                Parameters.Add("PhotoWidth", value.ToString());
        }
    }

    public int PhotoHeight
    {
        get
        {
            return Parameters.Get<int>("PhotoHeight", 150);
        }
        set
        {
            if (Parameters.ContainsKey("PhotoHeight"))
                Parameters["PhotoHeight"] = value.ToString();
            else
                Parameters.Add("PhotoHeight", value.ToString());
        }
    }

    public int ActualPhotoWidth
    {
        get { return (imageInfo == null) ? PhotoWidth : imageInfo.Width; }
    }

    public int ActualPhotoHeight
    {
        get { return (imageInfo == null) ? PhotoHeight : imageInfo.Height; }
    }

	public string CommentStoragePropertyName
	{
		get
		{
			return Parameters.Get("CommentStoragePropertyName", "");
		}
	}

	public bool EnableComments
	{
		get
		{
			return Parameters.Get<bool>("EnableComments", false);
		}
	}

	public bool EnableVoting
	{
		get
		{
			return Parameters.Get<bool>("EnableVoting", false);
		}
	}

	public bool IsForumInstalled
	{
		get 
		{
			return BXModuleManager.IsModuleInstalled("forum");
		}
	}

	public bool IsComUtilInstalled
	{
		get
		{
			return BXModuleManager.IsModuleInstalled("communicationutility");
		}
	}

    protected void Page_Load(object sender, EventArgs e)
    {
        if (EnableSef)
        {
            MapVariable(Parameters.Get<string>("SEFFolder", "/photogallery"), Parameters.Get<string>("SEF_Photo"), Results);
        }
        else
        {
            BXParamsBag<string> variableAlias = new BXParamsBag<string>();

            variableAlias["PhotoId"] = Parameters.Get<string>("ParamPhoto", "photo");

            MapVariable(variableAlias, Results);
        }

        if (PhotoId > 0)
        {
            Photo = BXIBlockElement.GetById(PhotoId);
			if (EnableComments && IsForumInstalled && !String.IsNullOrEmpty(CommentStoragePropertyName)) // комментарии включены, нужно получить ID темы комментариев чтобы отдать компоненту комментариев
			{

					ComponentCache["IdentityPropertyName"] = CommentStoragePropertyName;
					ComponentCache["IdentityPropertyValue"] = PhotoId;
					ComponentCache["IdentityPropertyTypeName"] = "Bitrix.System.Int";
					int commentId = Parameters.Get<int>("CommentId",0);
					ComponentCache["PostId"] = commentId;

					BXParamsBag<object> replace = new BXParamsBag<object>();
					replace.Add("PHOTOID", PhotoId);

					ComponentCache["UrlPhoto"] = ResolveTemplateUrl(Parameters.Get("UrlTemplatePhoto", "/photo/#PHOTOID#/"), replace);
					ComponentCache["PostReadPageUrlTemplate"] = ResolveTemplateUrl(Parameters.Get<string>("CommentReadPageUrlTemplate",String.Empty), replace);
					ComponentCache["PostReadUrlTemplate"] = ResolveTemplateUrl(Parameters.Get<string>("CommentReadUrlTemplate",string.Empty), replace);
					ComponentCache["PostOperationUrlTemplate"] = ResolveTemplateUrl(Parameters.Get<string>("CommentOperationUrlTemplate",string.Empty), replace);
					
				//}
			}
            if (Photo != null)
            {
                AlbumId = Photo.Sections.Count > 0 ? Photo.Sections[0].SectionId : 0;
                PhotoAuthor = (Photo.ModifiedByUser == null )? String.Empty : Photo.ModifiedByUser.UserName;

                if (Photo != null || Photo.DetailImageId > 0)
                    imageInfo = Bitrix.Services.Image.
                        BXImageUtility.GetResizedImage(Photo.DetailImage, PhotoWidth, PhotoHeight);

                if (BXIBlock.GetById(Photo.IBlockId).IsUserCanOperate("IBlockModifyElements"))
                    ModifyElements = true;

                Page.Title = Server.HtmlDecode(Photo.Name);
                PhotoTitle = Photo.Name;
                Description = Photo.DetailText ?? String.Empty;
                PhotoOriginalUrl = BXUri.ToRelativeUri(Photo.DetailImage.TextEncoder.Decode(Photo.DetailImage.FilePath));
                PhotoOriginalSizeText = string.Format(GetMessage("FormatOriginalSize"), Photo.DetailImage.Width, Photo.DetailImage.Height);

                //if (Photo.DetailImageId > 0)
                //{
                //    PhotoUrl = string.Format(
                //            "{0}ImageResizer.ashx?path={1}&width={2}&height={3}&fit=true",
                //            BXUri.ToRelativeUri("~/bitrix/handlers/Main/"),
                //			  UrlEncode(Photo.DetailImage.FileVirtualPath),
                //            Parameters.Get("PhotoWidth", 150),
                //            Parameters.Get("PhotoHeight", 150));
                //}


                EditUrl = MakeLink(Parameters.Get("UrlTemplatePhotoEdit", BXConfigurationUtility.Constants.ErrorHref), Results);

                if (Photo.Sections.Count > 0)
                {
                    BXIBlockSectionCollection navChain = BXIBlockSection.GetNavChain(Photo.Sections[0].SectionId);
                    for (int i = 0; i < navChain.Count; i++)
                    {
                        //AlbumId = navChain[i].SectionId;
                        string albumUrl = MakeLink(Parameters.Get<string>("UrlTemplateAlbum", BXConfigurationUtility.Constants.ErrorHref), Results);

                        string parentAlbumUrl;

                        if (i != 0)
                        {
                            // AlbumId = navChain[i].SectionId;
                            parentAlbumUrl = MakeLink(Parameters.Get<string>("UrlTemplateAlbum", BXConfigurationUtility.Constants.ErrorHref), Results);
                        }
                        else
                        {
                            parentAlbumUrl = Parameters.Get<string>("GalleryRoot");
                        }

                        BXSiteMapManager.AddNode(
                            parentAlbumUrl,
                            albumUrl,
                            navChain[i].Name);
                    }
                }

                BXSiteMapManager.AddNode(
                        BackUrl,
                        MakeLink(Parameters.Get<string>("UrlTemplatePhoto", BXConfigurationUtility.Constants.ErrorHref), Results),
                        Photo.Name);


            }
            else
            {

                Results.Add("AlbumId", 0);
                Page.Title = GetMessage("Photo");
            }

       } 
        else
        {
            Results.Add("AlbumId", 0);
            Page.Title = GetMessage("Photo");
        }

        BackUrl = MakeLink(Parameters.Get("UrlTemplateAlbum", BXConfigurationUtility.Constants.ErrorHref), Results);
        IncludeComponentTemplate();
    }

	public int CoverHeight
	{
		get
		{
			return Parameters.Get("CoverHeight", 75);
		}
	}

	public int CoverWidth
	{
		get
		{
			return Parameters.Get("CoverWidth", 75);
		}
	}

	public bool SetCover()
	{
		if (Photo != null && Photo.DetailImage != null && AlbumId > 0)
		{
			try
			{
				var album = BXIBlockSection.GetById(AlbumId);

				if (album == null)
					return false;

				if (album.Image != null)
					album.Image.Delete();

                BXFile newImage = new BXFile();
                newImage.OwnerModuleId = "iblock";
                newImage.FileNameOriginal = Photo.DetailImage.FileNameOriginal;
                newImage.SetFile(
                    BXImageUtility.GetResizedImageStream(Photo.DetailImage, CoverWidth, CoverHeight),
                    "iblock");
                newImage.Save();

				album.ImageId = newImage.Id;
				album.Save();
			}
			catch
			{
				return false;
			}

			return true;
		}
		return false;
	}

    public bool DeletePhoto()
    {
        bool results = false;

        string url = MakeLink(Parameters.Get<string>("UrlTemplateAlbum", BXConfigurationUtility.Constants.ErrorHref), AlbumId);

        if (PhotoId > 0)
        {
				if ( IsForumInstalled && Parameters.GetBool("EnableComments",false))
				{ // have to delete comments
						var c = new IncludeComponent("bitrix:forum.comment.block", "blog_comments");
						c.Attributes["ForumId"] = Component.Parameters.GetString("CommentForumId", "0");
						c.Attributes["IdentityPropertyName"] = Component.Parameters.GetString("CommentStoragePropertyName");
						c.Attributes["IdentityPropertyValue"] = PhotoId.ToString();
						c.Attributes["PostOperation"] = "DeleteAll";
						Page.Controls.Add(c);
					
				}
				BXIBlockElement.Delete(PhotoId);
                results = true;

        }

        if (Parameters.Get<bool>("EnableAjax", false) && !IsComponentDesignMode)
        {
            string script = string.Format("setTimeout(\"PageTransfer('{0}');\",0);",
                url);
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "AJAX_BUTTON_CLICK", script, true);
        }
        else if(errorMessage == String.Empty)
            Response.Redirect(url);

        return results;
    }

    protected override void OnPreRender(EventArgs e)
    {
        if (Photo != null)
        {
            Photo.IncViewsCount(1, null);
        }
        base.OnPreRender(e);
    }

}