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

using System.Collections.Generic;
using Bitrix.DataTypes;

public partial class bitrix_components_bitrix_photogallery_photo_templates__default_template : BXComponentTemplate<PhotogalleryPhotoEdit>
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Component.Photo == null)
            return;



        AlbumTitle.Text = Component.Photo.NameRaw;
        AlbumDescription.Text = Component.Photo.DetailTextRaw;


        lbSections.Items.Clear();
        foreach (ListItem item in Component.Albums)
        {
            lbSections.Items.Add(item);
            item.Selected = Component.Photo.Sections.Contains(int.Parse(item.Value));
        }
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            BXParamsBag<object> par = new BXParamsBag<object>();
            par.Add("Id", Parameters.Get("AlbumID", 0));

            List<int> albumList = new List<int>();
            foreach (ListItem album in lbSections.Items)
            {
                int albumId = 0;

                if (album.Selected && int.TryParse(album.Value, out albumId))
                    albumList.Add(albumId);
            }

            par.Add("Albums", albumList);
            par.Add("Title", AlbumTitle.Text);
            par.Add("Description", AlbumDescription.Text);

            List<string> errors = new List<string>();

            if (!Component.ProcessCommand("edit", par, errors)) 
                foreach (string error in errors)
                    validationSummary.AddErrorMessage(HttpUtility.HtmlEncode(error));
                
            //if (!Component.ProcessCommand("edit", par, err))
            //{
            //    //foreach (string kvp in err)
            //    //    errorMessage.AddErrorMessage(kvp, "vgLoginForm", "LoginField");
            //}
        }
    }
}
