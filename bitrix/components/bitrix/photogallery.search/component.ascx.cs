using System;

using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.UI;
using Bitrix;
using Bitrix.Services;
using Bitrix.IO;
using Bitrix.Configuration;

public partial class bitrix_components_bitrix_photogallery_search_component : BXComponent
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string searchData = Request[Parameters.Get<string>("UrlSearch", "Search")];

        if (!string.IsNullOrEmpty(searchData))
        {
			searchData = BXConfigurationUtility.DefaultEncoding.GetString(Convert.FromBase64String(searchData));
            BXFormFilter searchQuery = BXFormFilter.Deserialize(searchData);

            BXInfoBlockElementCollectionOld photos = BXInfoBlockElementManagerOld.GetList(searchQuery, null);

            if (photos.Count > 0)
            {
                Results["PHOTOS"] = photos;
                foreach (BXInfoBlockElementOld photo in photos)
                {
                    Results["Preview" + photo.ElementId.ToString()] = string.Empty;

                    BXFile image = photo.DetailImageId > 0 ? photo.DetailImage : photo.PreviewImage;

                    if (image == null)
                        continue;

                    Results["Preview" + photo.ElementId.ToString()] = string.Format(
                    "{0}ImageResizer.ashx?path={1}&width={2}&height={3}&fit={4}",
					BXUri.ToRelativeUri("~/bitrix/handlers/Main/"),
					UrlEncode(image.FileVirtualPath),
                    Parameters.Get<int>("PreviewWidth", 150),
                    Parameters.Get<int>("PreviewHeight", 150),
                    !Parameters.Get<bool>("FlickrMode", false));
                }
            }
        }


        IncludeComponentTemplate();
    }
}
