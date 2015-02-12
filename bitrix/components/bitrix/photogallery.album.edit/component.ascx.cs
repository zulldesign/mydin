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
using Bitrix.Components;
using Bitrix.IBlock;
using System.Collections.Generic;
using Bitrix.DataTypes;
using Bitrix.Services.Text;
using Bitrix.Configuration;


public partial class PhotogalleryAlbumEdit : BXComponent
{
    //PROPERTIES
    public int AlbumId
    {
        get
        {
            if (!IsComponentDesignMode)
            {
				return ComponentCache.Get("AlbumId", 0);
            }

            return 0;
        }
    }

    public string BackUrl
    {
        get
        {
			return ComponentCache.Get("BackUrl", "");
        }
        set
        {
			ComponentCache["BackUrl"] = value;
        }
    }

    public string ParamAlbum
    {
        get
        {
            return Parameters.Get<string>("ParamAlbum", "Album");
        }
    }

    public bool CanModify
    {
        get
        {
            return ComponentCache.Get("ModifyStructure", false);
        }

        private set
        {
			ComponentCache["ModifyStructure"] = value;
        }
    }

    public void SaveAlbumInfo(int parentAlbumId, string title, string description)
    {
        BXIBlockSection sec;
        if (AlbumId > 0 && this.Action=="edit")
        {
            sec = BXIBlockSection.GetById(AlbumId);
            sec.SectionId = parentAlbumId;
            sec.Name = title;
            sec.DescriptionType = BXTextType.Text;
            sec.Description = description;
			sec.Update();
        }
        else if (this.Action=="add"){
            sec = new BXIBlockSection(Parameters.Get("IBlockId", 0),
                parentAlbumId,
                title
             );

			sec.Active = true;
			sec.Description = description;
			sec.DescriptionType = BXTextType.Text;
			sec.Sort=500;
            sec.Code=string.Empty;
            sec.XmlId = string.Empty;
			sec.Save();
		}

        string url;

		if (!String.IsNullOrEmpty(BackUrl))
			url = BackUrl;
		else
        if (EnableSef)
        {
            Results["AlbumId"] = (parentAlbumId > 0) ? parentAlbumId.ToString() : string.Empty;
            if (AlbumId > 0)
            {
                url = MakeLink(Parameters.Get<string>("SefFolder"), Parameters.Get<string>("Sef_Album"), ComponentCache);
            }
            else
            {
                url = MakeLink(Parameters.Get<string>("SefFolder"), string.Empty, ComponentCache);
            }
        }
        else
        {
            if (parentAlbumId > 0)
            {
                url = string.Format("{0}?{1}={2}",
                    Request.Path,
                    Parameters.Get<string>("ParamAlbum", "Album"),
                    parentAlbumId);
            }
            else
            {
                url = Request.Path;
            }
        }

        if (Parameters.Get<bool>("EnableAjax", false) && !IsComponentDesignMode)
        {
            string script = string.Format("setTimeout(\"PageTransfer('{0}');\",0);",
                url);
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "AJAX_BUTTON_CLICK", script, true);
        }
        else
            Response.Redirect(url);
    }

    public override bool ProcessCommand(string commandName, BXParamsBag<object> commandParameters, System.Collections.Generic.List<string> commandErrors)
    {
        int albumId = (int)commandParameters["Id"];
        int parentAlbumId = (int)commandParameters["ParentId"];

        BXIBlockSection sec;
        if (albumId > 0)
        {
			sec = BXIBlockSection.GetById(albumId);
            sec.SectionId = parentAlbumId;
            sec.Name = commandParameters["Title"].ToString();
            sec.Description = commandParameters["Description"].ToString();

			sec.Update();
        }
        else
		{
			sec = new BXIBlockSection(Parameters.Get("IBlockId", 0),
				   parentAlbumId,
				   commandParameters["Title"].ToString()
				);

			sec.Active = true;
			sec.Description = commandParameters["Description"].ToString();
			sec.DescriptionType = BXTextType.Text;
			sec.Sort = 500;
			sec.Code = string.Empty;
			sec.XmlId = string.Empty;
			sec.Save();
		}


        string url;
        if (EnableSef)
        {
            Results["AlbumId"] = (albumId > 0) ? albumId.ToString() : string.Empty;
            if (albumId > 0)
            {
                url = MakeLink(Parameters.Get<string>("SefFolder"), Parameters.Get<string>("Sef_Album"), ComponentCache);
            }
            else
            {
                url = MakeLink(Parameters.Get<string>("SefFolder"), string.Empty, ComponentCache);
            }
        }
        else
        {
            if (albumId > 0)
            {
                url = string.Format("{0}?{1}={2}",
                    Request.Path,
                    Parameters.Get<string>("ParamAlbum", "Album"),
                    albumId);
            }
            else
            {
                url = Request.Path;
            }
        }

        if (Parameters.Get<bool>("EnableAjax", false) && !IsComponentDesignMode)
        {
            string script = string.Format("setTimeout(\"PageTransfer('{0}');\",0);",
                url);
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "AJAX_BUTTON_CLICK", script, true);
        }
        else
            Response.Redirect(url);

        return true;
        //return base.ProcessCommand(commandName, commandParameters, commandErrors);
    }

	string Action
	{
		get { return Parameters.GetString("Action", "add"); }
	}

    protected void Page_Load(object sender, EventArgs e)
    {
        int parentId=0;

        if (EnableSef)
        {
			MapVariable(Parameters.Get<string>("SEFFolder", "/photogallery"), Parameters.Get<string>("SEF_AlbumAdd"), ComponentCache);
			MapVariable(Parameters.Get<string>("SEFFolder", "/photogallery"), Parameters.Get<string>("SEF_AlbumEdit"), ComponentCache);

			if (ComponentCache.ContainsKey("ParentID") && int.TryParse(Results["ParentID"].ToString(), out parentId))
                ParentAlbumId = parentId;

			if (!String.IsNullOrEmpty(Request.QueryString[BXConfigurationUtility.Constants.BackUrl]))
				BackUrl = Request.QueryString[BXConfigurationUtility.Constants.BackUrl];
			else
			if(AlbumId > 0)
				BackUrl = MakeLink(Parameters.Get<string>("SefFolder"), Parameters.Get<string>("Sef_Album"), ComponentCache);
            else
				BackUrl = MakeLink(Parameters.Get<string>("SefFolder"), string.Empty, ComponentCache);
        }
        else
        {

            BXParamsBag<string> variableAlias = new BXParamsBag<string>();

            variableAlias["AlbumID"] = Parameters.Get<string>("ParamAlbum", "Album");

			MapVariable(variableAlias, ComponentCache);
			if (!String.IsNullOrEmpty(Request.QueryString[BXConfigurationUtility.Constants.BackUrl]))
				BackUrl = Request.QueryString[BXConfigurationUtility.Constants.BackUrl];
			else
				BackUrl = string.Format("{0}?{1}={2}",
					Request.Path,
					ParamAlbum,
					AlbumId);
            
        }

        if (BXIBlock.GetById(Parameters.Get("IBlockId", 0)).IsUserCanOperate("IBlockModifyStructure"))
            CanModify = true;

        if (AlbumId > 0 && Action=="edit")
        {
            BXIBlockSection sec = BXIBlockSection.GetById(AlbumId);

			if (BXIBlock.GetById(sec.IBlockId).IsUserCanOperate("IBlockModifyStructure"))
                CanModify = true;

            Results["AlbumTitle"] = sec.TextEncoder.Decode(sec.Name);
			Results["AlbumDescription"] = sec.TextEncoder.Decode(sec.Description);
            Results["ParentAlbum"] = sec.SectionId;
            ParentAlbumId = sec.SectionId;
        }
        else
        {
            
            if (Action=="add" && Parameters.ContainsKey("AlbumParentID") && int.TryParse(Parameters["AlbumParentID"],out parentId))
                    ParentAlbumId = parentId;
            Results["ParentAlbum"] = 0;
        }

        List<BXIBlockSection> sections = new List<BXIBlockSection>();

		List<BXInfoBlockSectionOld> oldsections = new List<BXInfoBlockSectionOld>();

        foreach (BXIBlockSection s in BXIBlockSection.GetTree(Parameters.Get("IBlockId", 0),0 , true, BXTextEncoder.EmptyTextEncoder))
        {
            sections.Add(s);
			oldsections.Add(BXInfoBlockSectionManagerOld.ConvertToOldElement(s));
        }


        Results["SECTIONSITEMS"] = sections;
		Results["SECTIONS"] = oldsections;

        IncludeComponentTemplate();
    }

    private int? _parentAlbumId = null;
    public int? ParentAlbumId 
    {
        get { return _parentAlbumId; }
        protected set { _parentAlbumId = value; }
    }
}
