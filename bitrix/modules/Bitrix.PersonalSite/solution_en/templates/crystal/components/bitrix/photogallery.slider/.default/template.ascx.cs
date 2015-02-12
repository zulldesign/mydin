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


public partial class bitrix_components_bitrix_photogallery_slider_templates__default_template : BXComponentTemplate<PhotogallerySlider>
{

    protected string[] photoLinks;
    protected string[] pageLinks;
    protected string[] sizes;

    protected void Page_Load(object sender, EventArgs e)
    {

        if (Results.ContainsKey("Photos"))
        {
            BXIBlockElementCollection photos = Component.PhotoItems;
            photoLinks = new string[Component.Size];
            pageLinks = new string[Component.Size+1];
            sizes = new string[Component.Size];
            string Key,Key2;
            for (int i = GetStartIndex; i < GetStartIndex + Component.Size; i++)
            {
                Key = string.Concat("Preview", photos[i].Id.ToString());
                if (!Results.ContainsKey(Key))
                    continue;
                photoLinks[i - GetStartIndex] = Results[Key].ToString();
                Key = string.Concat("PHOTO_LINK_", photos[i].Id.ToString());
                if (!Results.ContainsKey(Key))
                    continue;
                pageLinks[i - GetStartIndex+1] = Results[Key].ToString();

                Key =  string.Concat("PreviewWidth", photos[i].Id.ToString());
                Key2 =  string.Concat("PreviewHeight", photos[i].Id.ToString());
                sizes[i - GetStartIndex] = String.Format("width : {0}px;height :{1}px;", Results[Key].ToString(), Results[Key2].ToString());

            }

            if (Component.SelectedIndex == 0 ) pageLinks[0] = "#";
            else
            {
                Key = string.Concat("PHOTO_LINK_", photos[Component.SelectedIndex - 1].Id.ToString());
                if(Results.ContainsKey(Key)){
                    pageLinks[0] = Results[Key].ToString();
                }
                else pageLinks[0] = "#";
            }

        }

    }
    //determine an index from wich start to display photos in slider
    protected int GetStartIndex
    {
        get
        {
            if (Component.SelectedIndex > Component.PhotoItems.Count - Component.Size) return Component.PhotoItems.Count - Component.Size;

            return Component.SelectedIndex;
        }
    }

}
