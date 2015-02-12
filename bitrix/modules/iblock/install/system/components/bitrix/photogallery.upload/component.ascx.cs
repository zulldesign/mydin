using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix.IBlock;
using System.Text;
using Bitrix.Components;

using Bitrix.Security;
using Bitrix.Modules;
using Bitrix.IO;
using Bitrix.DataTypes;
using Bitrix.Services.Text;
using System.Web.Configuration;
using System.Collections.Generic;
using Bitrix.Services.Js;

public partial class PhotogalleryUpload : BXComponent
{

    public int AlbumId
    {
        get { return Results.Get("AlbumId", 0); }
    }

    public int PreviewWidth
    {
        get { return Parameters.Get<int>("PreviewWidth"); } 
    }

    public int PreviewHeight
    {
        get { return Parameters.Get<int>("PreviewHeight"); } 

    }

    public int PhotoHeight
    {
        get { return Parameters.Get<int>("PhotoHeight"); }
    }

    public int PhotoWidth
    {
        get { return Parameters.Get<int>("PhotoWidth"); }
    }

    public int CoverHeight
    {
        get { return Parameters.Get<int>("CoverHeight"); }
    }

    public int CoverWidth
    {
        get { return Parameters.Get<int>("CoverWidth"); }
    }


    protected void Page_Load(object sender, EventArgs e)
    {
		if (EnableSef)
        {
            MapVariable(Parameters.Get<string>("SEFFolder"), Parameters.Get<string>("SEF_Upload"), Results);
        }
        else
        {
            BXParamsBag<string> variableAlias = new BXParamsBag<string>();
            variableAlias["AlbumID"] = Parameters.Get<string>("ParamAlbum", "Album");

            MapVariable(variableAlias, Results);
        }

        Parameters["CacheMode"] = "None";

        if (AlbumId == 0)
        {
            Results["ShowAlbumSelect"] = true;
        }
        else if (BXIBlock.GetById(BXIBlockSection.GetById(AlbumId).IBlockId).IsUserCanOperate("IBlockModifyElements"))
            Results["ModifyElements"] = true;
        else
        {
            IncludeComponentTemplate();
            return;
        }

        ListItemCollection albums = new ListItemCollection();

        foreach (BXIBlockSection album in BXIBlockSection.GetTree(Parameters.Get("IBlockId", 6), 0,true,Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder))
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < album.DepthLevel; i++)
                sb.Append(" . ");
            sb.Append(album.Name);
            albums.Add(new ListItem(sb.ToString(), album.Id.ToString()));
        }

        Results["Albums"] = albums;

        if (EnableSef)
        {
            Results["BACK_URL"] = MakeLink(Parameters.Get<string>("SefFolder"), Parameters.Get<string>("Sef_Album"), Results);
        }
        else
        {
            Results["BACK_URL"] = string.Format("{0}?{1}={2}",
                    Request.Path,
                    Parameters.Get<string>("Paramalbum", "album"),
                    Parameters.Get<int>("albumId", 0));
        }

        IncludeComponentTemplate();
    }

    protected override void PreLoadComponentDefinition()
    {
        Title = GetMessage("PhotoUpload.Title");
        Description = GetMessage("PhotoUpload.Description");
    }

    public void TrySaveImages(int albumId)
    {
        if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
			return;

		//try
        //{
            int fileCount;

            if (!int.TryParse(Request.Form["FileCount"], out fileCount))
                return;

            //Iterate through uploaded data and save the original file, thumbnail, and description.
            for (int i = 1; i <= fileCount; i++)
            {
                //Get first thumbnail (the single thumbnail in this code sample) and save it to disk.
                HttpPostedFile thumbnail1File = Request.Files["Thumbnail1_" + i];
                HttpPostedFile thumbnail2File = Request.Files["Thumbnail2_" + i];
                HttpPostedFile thumbnail3File = Request.Files["Thumbnail3_" + i];

                //Save file info.
                string description = Request.Form["Description_" + i];
                string title = Request.Form["Title_" + i];
                if (title == String.Empty) title = thumbnail3File.FileName;

                //CREATE NEW FOTO

                //try
                //{
                    //if (!currentUserCanModifyElement)
                    //    throw new Exception("У вас не достаточно прав для создания нового элемента");

                bool savedSuccessfully = true;
                BXFile preview = null, detail = null;
                try
                {

                    detail = SaveFile(thumbnail3File);
                    preview = SaveFile(thumbnail1File);
                    
                    if (detail != null)
                    {
                        string detailFileName = detail.FileName.Substring(0,detail.FileName.LastIndexOf("."));
                        
                        Bitrix.Services.Image.BXImageInfo imageInfo =
                            Bitrix.Services.Image.BXImageUtility.GetResizedImage(
                                detail, PreviewWidth, PreviewHeight);

                        imageInfo =
                            Bitrix.Services.Image.BXImageUtility.GetResizedImage(
                                detail, PhotoWidth, PhotoHeight);

                        //imageInfo =
                        //    Bitrix.Services.Image.BXImageUtility.GetResizedImage(
                        //        detail, CoverWidth, CoverHeight);

                    //SaveFileToUploadFolder(thumbnail2File, PhotoWidth, PhotoHeight, Parameters.Get<string>("PhotoFolder"), BXPath.ToPhysicalPath(detail.FilePath),detailFileName);
                    }
                }
                catch (FormatException e)
                {
                    savedSuccessfully = false;
                    AddError(string.Concat(thumbnail1File.FileName, ": " ,e.Message));
                }


                if (!savedSuccessfully)
                    continue;

                BXIBlockElement el = new BXIBlockElement(Parameters.Get("IBlockId", 6), title, BXTextEncoder.EmptyTextEncoder);

                el.Active = true;
                el.ActiveFromDate = DateTime.MinValue;
                el.ActiveToDate = DateTime.MaxValue;
                el.CreatedBy = ((BXIdentity)Page.User.Identity).Id;
                el.ModifiedBy = ((BXIdentity)Page.User.Identity).Id;
                el.Sort = 500;
                el.DetailImageId = (detail!=null)? detail.Id : 0;
                el.DetailText = description;
                el.PreviewImageId = ((preview != null) ? preview.Id : 0);
                el.DetailTextType = BXTextType.Text;
                el.XmlId = Guid.NewGuid().ToString();
                el.Code = String.Empty;

                el.Sections.Add(albumId);
                el.Save();

                    if (el == null)
                        throw new Exception(GetMessageRaw("Exception.AnErrorHasOccurredWhileCreationElement"));
                //}
                //catch (BXEventException ee)
                //{
                //    throw ee;
                //}
                //catch (Exception ee)
                //{
                //    throw ee;
                //}
            }
        //}
        //catch{}

        Response.AddHeader("X-Powered-CMS", "Bitrix Site Manager (582fd04dac6869e159ea80524ec43d0d)");
    }

    private List<string> _errorList = null;
    protected void AddError(string error) 
    {
        if (string.IsNullOrEmpty(error))
            throw new ArgumentNullException("error");

        if (_errorList == null)
            _errorList = new List<string>();

        _errorList.Add(error);
    }

    protected void ShowErrors()
    {
        if (_errorList == null || _errorList.Count == 0)
            return;

        StringBuilder sb = new StringBuilder();
        foreach (string error in _errorList)
            sb.AppendLine(error);
        BXDialogGoodbyeWindow goodbyeWin = new BXDialogGoodbyeWindow();
        //goodbyeWin.Layout = BXDialogGoodbyeWindow.LayoutType.Error;
        //goodbyeWin.TimeToLive = -1;
        //goodbyeWin.Content = sb.ToString();

        //ScriptManager.RegisterClientScriptBlock(this, GetType(), "photogalleryUploadErrorList", string.Format("<script type=\"text/javascript\" id=\"photogalleryUploadingCompletion\">{0}</script>", goodbyeWin.GenerateClientScriptForShow(true)), false);

        ScriptManager.RegisterClientScriptBlock(this, GetType(), "photogalleryUploadErrorList", string.Format("<script type=\"text/javascript\" id=\"photogalleryUploadingCompletion\">window.alert(\"{0}\");</script>", BXJSUtility.Encode(sb.ToString())), false);

    }

    protected BXFile SaveFile(HttpPostedFile postedFile)
    {
        BXFile image = new BXFile(postedFile, "iblock", "iblock", null);
		BXFileValidationResult fResult = image.ValidateAsImage(0, 0, 0);
        if (fResult != BXFileValidationResult.Valid)
        {
            string fError = "";
            if ((fResult & BXFileValidationResult.InvalidContentType) == BXFileValidationResult.InvalidContentType)
                fError += GetMessageRaw("Error.InvalidType");
            if ((fResult & BXFileValidationResult.InvalidExtension) == BXFileValidationResult.InvalidExtension)
            {
                if (!String.IsNullOrEmpty(fError))
                    fError += ", ";
                fError += GetMessageRaw("Error.Inavlid.extension");
            }
            if ((fResult & BXFileValidationResult.InvalidImage) == BXFileValidationResult.InvalidImage)
            {
                if (!String.IsNullOrEmpty(fError))
                    fError += ", ";
                fError += GetMessageRaw("Error.InvalidImage");
            }
            throw new FormatException(fError);
        }
        image.DemandFileUpload();
        image.Save();
        return image;
    }

    public int MaxRequestLength
    {
        get 
        {
            //return ((HttpRuntimeSection)WebConfigurationManager.GetSection("system.web/httpRuntime")).MaxRequestLength * 1024;
            return 4194304;
        }
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        ShowErrors();
    }
}
