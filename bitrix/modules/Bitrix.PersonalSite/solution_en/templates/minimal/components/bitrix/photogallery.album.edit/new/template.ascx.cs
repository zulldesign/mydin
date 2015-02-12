using System;
using System.Text;
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
using Bitrix.IBlock;

public partial class bitrix_components_bitrix_photogallery_album_edit_templates__default_template : BXComponentTemplate<PhotogalleryAlbumEdit>
{
    protected void Page_Init(object sender, EventArgs e)
    {
        //AlbumTitle.Text = Results.ContainsKey("AlbumTitle") ? Results["AlbumTitle"].ToString() : string.Empty;
        //AlbumDescription.Text = Results.ContainsKey("AlbumDescription") ? Results["AlbumDescription"].ToString() : string.Empty;
        int albId;
        int? albumId=null;
        if (Parameters.ContainsKey("AlbumId"))
        {
            if ( int.TryParse(Parameters["AlbumId"],out albId))
                albumId = albId;

        }
        //string albumTitle = albumId.HasValue && albumId > 0 && Parameters.ContainsKey("AlbumTitle") ? Parameters["AlbumId"] : null;

        ddlParentSection.Items.Clear();
        ddlParentSection.Items.Add(new ListItem(GetMessageRaw("TopLevel"), "0"));
        if (Results.ContainsKey("SECTIONSITEMS"))
        {
            foreach (BXIBlockSection section in (List<BXIBlockSection>)Results["SECTIONSITEMS"])
            {
                if (albumId.HasValue && albumId.Value == section.SectionId)
                        continue;
                StringBuilder sb = new StringBuilder();
                for (int i = 1; i < section.DepthLevel; i++)
                    sb.Append(" . ");
                sb.Append(section.TextEncoder.Decode(section.Name));
                ddlParentSection.Items.Add(new ListItem(sb.ToString(), section.Id.ToString()));
            }
        }

        //ddlParentSection.SelectedValue = Parameters.Get("AlbumParentID", 0).ToString();
        if (Component.ParentAlbumId.HasValue)
            ddlParentSection.SelectedValue = Component.ParentAlbumId.Value.ToString();
    }

    protected override void OnLoad(EventArgs e)
    {
        AlbumTitle.Text = Results.ContainsKey("AlbumTitle") ? Results["AlbumTitle"].ToString() : string.Empty;
        AlbumDescription.Text = Results.ContainsKey("AlbumDescription") ? Results["AlbumDescription"].ToString() : string.Empty;

        base.OnLoad(e);
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
        {
            Component.SaveAlbumInfo(int.Parse(ddlParentSection.SelectedValue),AlbumTitle.Text,AlbumDescription.Text);
        }
    }
}
