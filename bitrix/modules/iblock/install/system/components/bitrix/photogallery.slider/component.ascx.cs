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
using System.IO;
using Bitrix.UI;
using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.Components;

using System.Text;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Services;
using Bitrix.Configuration;
using Bitrix;

public partial class PhotogallerySlider : BXComponent
{
    //PROPERTIES
    public int PhotoId
    {
        get { return Results.Get("PhotoId",0); }
        set { Results["PhotoId"] = value; }
    }

    public int Size
    {
        get { return Results.Get("Size", 2); }
        set { Results["Size"] = value; }
    }

    public int PreviewWidth
    {
        get { return Parameters.Get<int>("PreviewWidth"); }
        set { Results["PreviewWidth"] = value; }
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

    public string PhotoFolder
    {
        get { return Parameters.Get<string>("PhotoFolder"); }

    }

    public int SelectedIndex
    {
        get { return Results.Get("SelectedIndex", 0); }
        set { Results["SelectedIndex"] = value; }
    }

    public string PreviewFolder
    {
        get { return Parameters.Get<string>("PreviewFolder"); }
    }

    public BXInfoBlockElementCollectionOld Photos
    {
        get { return BXInfoBlockElementManagerOld.
                ConvertToOldElements(Results.Get("Photos", new BXIBlockElementCollection())); }
        //set { Results["Photos"] = value; }
    }

    public BXIBlockElementCollection PhotoItems
    {
        get { return Results.Get("Photos", new BXIBlockElementCollection()); }
        set { Results["Photos"] = value;}
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Size = Parameters.Get("Size", 2);

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
            BXIBlockElement photoItem = BXIBlockElement.GetById(PhotoId);

            if (photoItem != null)
            {
                if (photoItem.Sections.Count > 0)
                {
                    int albumId = photoItem.Sections[0].SectionId;

                    BXFilter photoFilter = new BXFilter();
                    photoFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, albumId));
                    PhotoItems = BXIBlockElement.GetList(photoFilter,null,new BXSelectAdd(BXIBlockElement.Fields.DetailImage,BXIBlockElement.Fields.PreviewImage),null);
                }
                else
                {
                    PhotoItems = new BXIBlockElementCollection();
                    PhotoItems.Add(photoItem);
                }
                    foreach (BXIBlockElement photo in PhotoItems)
                    {

                        if (PhotoId == photo.Id)
                            SelectedIndex = PhotoItems.IndexOf(photo);

                        Results["Preview" + photo.Id.ToString()] = string.Empty;

                        BXFile image = photo.DetailImageId > 0 ? photo.DetailImage : photo.PreviewImage;

                        if (image == null)
                            continue;
                        BXParamsBag<object> vars = new BXParamsBag<object>();
                        vars["PhotoId"] = photo.Id;
                        Results["PHOTO_LINK_" + photo.Id.ToString()] = MakeLink(Parameters.Get<string>("UrlTemplatePhoto", BXConfigurationUtility.Constants.ErrorHref), vars);
                        // if we've found preview file with needed dimensions stored in cash folder, we return it

                        Bitrix.Services.Image.BXImageInfo imageInfo = Bitrix.Services.Image.
                            BXImageUtility.GetResizedImage(image, PreviewWidth, PreviewHeight);

                        Results["Preview" + photo.Id.ToString()] = imageInfo.GetUri();
                        Results["PreviewWidth" + photo.Id.ToString()] = imageInfo.Width;
                        Results["PreviewHeight" + photo.Id.ToString()] = imageInfo.Height;


                    }
                


            }
            if (Size > PhotoItems.Count)
                Size = PhotoItems.Count;
            IncludeComponentTemplate();
        }
    }

    //NESTED CLASSES
    public class Photo
    {

    }
}
