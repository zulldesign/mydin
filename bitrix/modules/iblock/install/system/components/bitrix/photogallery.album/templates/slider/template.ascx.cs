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
using Bitrix.Services.Text;
using Bitrix.CommunicationUtility.Rating;

public partial class bitrix_components_bitrix_photogallery_album_templates_slider_template : BXComponentTemplate<PhotogalleryAlbum>
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

		return GetMessage("Original") + "(" + image.Width.ToString() + " x " + image.Height.ToString() + ") px";
	}



	string picList = null;

	protected string PicturesList(bool getCount)
	{
		if (picList != null)
			return picList;
		StringBuilder s = new StringBuilder();
		int pageNum=0;
		if ( !int.TryParse(Component.Parameters["PageId"],out pageNum) && !int.TryParse(Component.ComponentCache["PageId"].ToString(),out pageNum) )
			pageNum = 1;
		s.AppendFormat("{{start_number:{0},elements_count:{1},recsPerPage:{2},elements:",
			(pageNum - 1) * Component.Parameters.GetInt("PagingRecordsPerPage") + 1, getCount ? Component.TotalFotoCount : 0, Component.Parameters.GetInt("PagingRecordsPerPage", Component.PhotoItems.Count));
		s.Append("[");

		var b = new BXParamsBag<object>();
		b.Add("ALBUMID", Component.AlbumId);
		for (int i = 0; i < Component.PhotoItems.Count; i++)
		{
			b.Add("PHOTOID", Component.PhotoItems[i].Id);
			s.AppendFormat("{{url:'{0}',title:'{1}',page:'{3}',id:{2},editurl:'{4}',height:{5},width:{6}, origtext:'{7}',description:'{8}',photourl:'{9}'}}",
				Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].Preview,
				Encode(Component.PhotoItems[i].Name),
				Component.PhotoItems[i].Id,
				pageNum,
				Component.MakeLink(Component.Parameters.Get("UrlTemplatePhotoEdit", BXConfigurationUtility.Constants.ErrorHref), b),
				Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].ActualCoverHeight,
				Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].ActualCoverWidth,
				GetOriginalText(Component.PhotoItems[i]),
				JSEncode(Component.PhotoItems[i].DetailText),
				Component.MakeLink(Component.Parameters.Get("UrlTemplatePhoto", BXConfigurationUtility.Constants.ErrorHref), b)
				);
			b.Remove("PHOTOID");
			if (i != Component.PhotoItems.Count - 1)
				s.Append(",");
		}
		s.Append("]}");
		return picList = s.ToString();
	}

    protected void Page_Load(object sender, EventArgs e)
    {

		if (Request.QueryString["ajax"] =="true")
		{
			Response.Clear();
			Response.StatusCode = 200;
			Response.ContentType = "application/json";
			Response.Write(PicturesList(false));
			Response.End();
		}
		
    }



	string pathForQuery = null;
	protected String PathForAjaxGetQuery
	{
		get {
			if (pathForQuery != null)
				return pathForQuery;
			if (Component.EnableSef)
			{
				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace.Add("ALBUMID", Component.AlbumId);
				replace.Add("PAGEID", "bxpagenumber");
				replace.Add("PHOTOID", Component.Parameters["PhotoId"]);

				return pathForQuery = Component.ResolveTemplateUrl(Component.Parameters["UrlTemplatePhoto"], replace) + "?page=bxpagenumber&ajax=true&ValidationToken=" + BXCsrfToken.GenerateToken();
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
				return pathForQuery = "?" + queryParams.ToString();
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
		BXPage.Scripts.RequireAdminTools();
	}

	protected int FotoSize()
	{
		int hei = (int ) (Component.Parameters.GetInt("PreviewHeight", 75) * 0.6);
		int w = (int)(Component.Parameters.GetInt("PreviewHeight", 75) * 0.6);
		if (w > hei) return w;
		else return hei;
	}
}
