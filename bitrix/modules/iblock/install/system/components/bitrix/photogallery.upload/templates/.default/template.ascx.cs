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
using System.IO;
using System.Collections.Generic;

using Bitrix.IBlock;
using Bitrix.Modules;
using Bitrix.Security;

public partial class bitrix_components_bitrix_photogallery_upload_templates__default_template : BXComponentTemplate<PhotogalleryUpload>
{
    protected override void OnInit(EventArgs e)
    {
		base.OnInit(e);

		ListItemCollection albums = null;
        if (Results.ContainsKey("Albums"))
        {
            lbSections.Items.Clear();
            albums = (ListItemCollection)Results["Albums"];
            foreach (ListItem item in albums)
            {
                lbSections.Items.Add(item);
                SelectAlbums.Items.Add(item);
            }
        }

        lbSections.SelectedValue = Component.AlbumId.ToString();
        SelectAlbums.SelectedValue = Component.AlbumId.ToString();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Component.TrySaveImages(Component.AlbumId);
    }

    protected override void OnPreRender(EventArgs e)
    {
		base.OnPreRender(e);
		BXPage.Scripts.RequireUtils();
		BXPage.RegisterScriptInclude("~/bitrix/image_uploader/iuembed.js");

		ListItemCollection albums = (ListItemCollection)Results["Albums"];
		if (albums == null || albums.Count == 0)
		{
			errorMessage.AddErrorMessage(GetMessage("Error.ThereAreNoAlbums"));
			errorMessage.Visible = true;
			GoToAlbum.Enabled = false;
		}
		else
			errorMessage.Visible = false;
    }

    protected void GoToAlbum_Click(object sender, EventArgs e)
    {
        Results["AlbumId"] = int.Parse(SelectAlbums.SelectedValue);
        Response.Redirect(Component.MakeLink(Parameters.Get("UrlTemplateUpload","")));
    }
}
