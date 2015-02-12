using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Bitrix.IBlock;
using Bitrix.UI;
using Bitrix.DataTypes;
using Bitrix.Services;
using Bitrix.Configuration;

public partial class PhotogalleryPhotoEdit : BXComponent
{

    //PROPERTIES
    public int AlbumId
    {
        get { return Results.Get("AlbumId", 0); }
        set { Results["albumid"] = value; }
    }

    public BXInfoBlockElementOld Photo
    {
        get
        {
            return Results.Get<BXInfoBlockElementOld>("Photo", null);
        }
        set
        {
            Results["Photo"] = value;
        }
    }

    public int PhotoId
    {
        get { return Results.Get("PhotoId", 0); }
        set { Results["PhotoId"] = value; }
    }

    public bool CanModify
    {
        get { return Results.Get("ModifyElements", false); }
        set { Results["ModifyElements"] = value; }
    }

    public string BackUrl
    {
        get { return Results.Get("BackUrl", ""); }
        set { Results["BackUrl"] = value; }
    }

    public ListItemCollection Albums
    {
        get
        {
            return Results.Get<ListItemCollection>("Albums", new ListItemCollection());
        }
        set
        {
            Results["Albums"] = value;
        }
    }



    protected void Page_Load(object sender, EventArgs e)
    {
        if (EnableSef)
        {
            MapVariable(Parameters.Get<string>("SEFFolder"), Parameters.Get<string>("SEF_PhotoEdit"), Results);
        }
        else
        {
            BXParamsBag<string> variableAlias = new BXParamsBag<string>();

            variableAlias["PhotoId"] = Parameters.Get<string>("ParamPhoto");

            MapVariable(variableAlias, Results);
        }

        if (PhotoId > 0)
        {
            Photo = BXInfoBlockElementManagerOld.GetById(PhotoId);

            if (BXInfoBlockManagerOld.GetById(Photo.IBlockId).IsUserCanOperate("IBlockModifyElements"))
                CanModify = true;
            else
            {
                IncludeComponentTemplate();
                return;
            }

            Page.Title = Photo.NameRaw;
            int parentSection = Photo.Sections.Count > 0 ? Photo.Sections[0] : 0;
            Albums = new ListItemCollection();
            foreach (BXInfoBlockSectionOld album in BXInfoBlockSectionManagerOld.GetTree(Photo.IBlockId, 0))
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 1; i < album.DepthLevel; i++)
                    sb.Append(" . ");
                sb.Append(album.NameRaw);
                ListItem item = new ListItem(sb.ToString(), album.SectionId.ToString());
                item.Selected = album.SectionId == parentSection;
                Albums.Add(item);
            }

            BackUrl = MakeLink(Parameters.Get<string>("UrlTemplatePhoto", BXConfigurationUtility.Constants.ErrorHref), Results);
        }


        IncludeComponentTemplate();
    }

    public override bool ProcessCommand(string commandName, BXParamsBag<object> commandParameters, List<string> commandErrors)
    {
        int photoId = Results.Get<int>("PhotoId");
        Results["PhotoId"] = photoId;

        List<int> albumList = (List<int>)commandParameters["Albums"];

        BXInfoBlockElementOld photo;
        if (photoId > 0)
        {
            photo = BXInfoBlockElementManagerOld.GetById(photoId);
            photo.Name = commandParameters["Title"].ToString();
            photo.DetailText = commandParameters["Description"].ToString();
            photo.Sections.Clear();
            foreach (int albumId in albumList)
            {
                    photo.Sections.Add(albumId);
            }

            photo.Update();
        }

        string url = MakeLink(Parameters.Get<string>("UrlTemplatePhoto", BXConfigurationUtility.Constants.ErrorHref), Results);

        if (Parameters.Get("EnableAjax", false) && !IsComponentDesignMode)
        {
            string script = string.Format("setTimeout(\"PageTransfer('{0}');\",0);",
                url);
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "AJAX_BUTTON_CLICK", script, true);
        }
        else
            Response.Redirect(url);

        return true;
    }
}