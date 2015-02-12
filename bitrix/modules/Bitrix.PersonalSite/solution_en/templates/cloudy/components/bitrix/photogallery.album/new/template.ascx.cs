using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;


using Bitrix.Components;
using Bitrix.UI;
using Bitrix.DataTypes;
using System.Text;
using Bitrix.Services;
using System.Collections.Specialized;
using Bitrix.Security;
using Bitrix.Configuration;
using Bitrix.IBlock;
using Bitrix.IO;

public partial class bitrix_components_bitrix_photogallery_section_templates__default_template : BXComponentTemplate<PhotogalleryAlbum>
{
    protected const int MainAlbumHeightPadding = 65;
    protected const int ImageContainerHeightPadding = 50;
    protected const int MainAlbumWidthPadding = 25;
    protected const int ImageContainerWidthPadding = 25;
    protected const int RowContainerWidthPadding = 25;
    protected const int RowContainerHeightPadding = 18;
    protected const int MainPhotoWidthPadding = 15;
    protected const int MainPhotoHeightPadding = 25;

	protected string GetOriginalText(BXIBlockElement foto)
	{
		BXFile image;
		image = foto.DetailImage ?? foto.PreviewImage;
		if ( image == null ) return string.Empty;

		return GetMessage("Original") + " (" + image.Width.ToString() + " x " + image.Height.ToString() + ") px";
	}

	protected string PicturesList()
	{
		StringBuilder s = new StringBuilder();
		int pageNum=0;
		if ( !int.TryParse(Component.Parameters["PageId"],out pageNum))
			pageNum = 1;
		s.Append("[");
		var b = new BXParamsBag<object>();
		for (int i = 0; i < Component.PhotoItems.Count; i++)
		{
			b.Add("PHOTOID",Component.PhotoItems[i].Id);
			s.AppendFormat("{{url:'{0}',title:'{1}',id:'{2}',page:'{3}',editurl:'{4}',height:{5},width:{6}, origtext:'{7}'}}", 
				Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].DetailUrl, 
				Encode(Component.PhotoItems[i].Name),
				Component.PhotoItems[i].Id,
				pageNum,
				Component.MakeLink(Component.Parameters.Get("UrlTemplatePhotoEdit", BXConfigurationUtility.Constants.ErrorHref),b),
				Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].DetailHeight,
				Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].DetailWidth,
				GetOriginalText(Component.PhotoItems[i])
				);
			b.Remove("PHOTOID");
			if (i != Component.PhotoItems.Count - 1)
				s.Append(",");
		}
		s.Append("]");
		return s.ToString();
	}

    protected void Page_Load(object sender, EventArgs e)
    {

		if (Request.QueryString["ajax"] =="true")
		{
			Response.Clear();
			Response.StatusCode = 200;
			Response.ContentType = "application/json";
			Response.Write(PicturesList());
			Response.End();
		}



        if (Component.Album != null)
        {
            lButtonDelete.Visible = true;
            lButtonDelete.OnClientClick = string.Format("return confirm(\"{0}\");", JSEncode(GetMessageRaw("ConfirmText.DoYouReallyWantRemoveThisAlbum")));
        }
		int i;
		if (Request.QueryString["del"] != null && Request.QueryString["ValidationToken"] != null 
			&& Bitrix.Security.BXCsrfToken.CheckToken( Request.QueryString["ValidationToken"]) && int.TryParse(Request.QueryString["del"],out i))
		{
			//delete album
			BXParamsBag<object> par = new BXParamsBag<object>();
			par["AlbumId"] = i;
			List<string> err = new List<string>();
			 if (!Component.ProcessCommand("delete", par, err))
			 {
			 }
		}
    }
    protected void lbDelete_Click(object sender, EventArgs e)
    {
        BXParamsBag<object> par = new BXParamsBag<object>();
		//par.Add("AlbumId", Request["__EVENTARGUMENT"]);
        List<string> err = new List<string>();

        if (!Component.ProcessCommand("delete", par, err))
        {
            //foreach (string kvp in err)
            //    errorMessage.AddErrorMessage(kvp, "vgLoginForm", "LoginField");
        }
    }

	protected String PathForAjaxGetQuery
	{
		get {

			if (Component.EnableSef)
			{
				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace.Add("ALBUMID", Component.AlbumId);
				replace.Add("PAGEID", "bxpagenumber");
				string s = Component.ResolveTemplateUrl(Component.Parameters["UrlTemplateAlbumPage"], replace) + "?ajax=true&ValidationToken=" + BXCsrfToken.GenerateToken();
				return s;
			}
			else
			{

				String url = BXSefUrlManager.CurrentUrl.AbsoluteUri;
				NameValueCollection queryParams = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
				queryParams.Remove("ValidationToken");
				queryParams.Add("ValidationToken", BXCsrfToken.GenerateToken());
				queryParams.Remove("page");
				queryParams.Add("page", "bxpagenumber");
				queryParams.Add("ajax", "true");
				string query = BXSefUrlManager.CurrentUrl.Query;
				if (query.Length > 0)
					url = url.Substring(0, url.Length - query.Length);
				return "?" + queryParams.ToString();
			}
		}
	}

	protected String CurrentPage
	{
		get { return Component.Parameters.GetInt("PageId")==0 ?  "1" : Component.Parameters["PageId"] ; }
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		BXPage.Scripts.RequireUtils();
	}

	protected int FotoSize()
	{
		int hei = (int ) (Component.Parameters.GetInt("PreviewHeight", 75) * 0.6);
		int w = (int)(Component.Parameters.GetInt("PreviewHeight", 75) * 0.6);
		if (w > hei) return w;
		else return hei;
	}
}
