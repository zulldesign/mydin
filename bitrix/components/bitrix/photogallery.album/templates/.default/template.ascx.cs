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

public partial class bitrix_components_bitrix_photogallery_album_templates__default_template : BXComponentTemplate<PhotogalleryAlbum>
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



	protected string GetVotingInfo(BXIBlockElement photo)
	{
		
		if (Component.EnableVoting && Component.IsVotingAllowed)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("voting_allowed:{0},voting_value:'",
				Component.PhotoDescriptionDictionary[photo].IsVotingAllowed ? "true" : "false").Append(Component.PhotoDescriptionDictionary[photo].VotingTotals.Value.ToString()).Append("',");
			sb.AppendFormat("voting_result:'{0}'", Component.PhotoDescriptionDictionary[photo].VotingTotals.Value).Append(",");
			sb.AppendFormat("voting_html : '<span id=\"bx-voting-container\" ><a href=\"\" title=\"{0}\" onclick=\"" + HttpUtility.HtmlEncode("return SlideSlider.voteForPhoto(\"{1}\",true);")
				+ "\" class=\"rating-vote-plus\"></a><a href=\"\" title=\"{2}\" onclick=\"" + HttpUtility.HtmlEncode("return SlideSlider.voteForPhoto(\"{1}\",false);") +
				"\" class=\"rating-vote-minus\"></a><span id=\"bx-voting-result-container\">',",
				GetMessage("Voting.Like"),
				photo.Id,
				GetMessage("Voting.DontLike"));
				sb.AppendFormat("voting_url:'{0}'",
				BXSefUrlManager.CurrentUrl.AbsolutePath + "?action=vote&" + Component.Parameters.GetString("ParamPhoto", "photo") + "=" + photo.Id.ToString() + "&"
				+ Component.Parameters.GetString("ParamAlbum", "album") + "=" + Component.AlbumId);
			return sb.ToString();
		}
		else
		{
			return "voting_allowed:false";
		}

	}

	protected string GetCommentUrl(int photoId)
	{
		BXParamsBag<object> replace = new BXParamsBag<object>();
		replace.Add("PHOTOID",photoId);
		replace.Add("ALBUMID", Component.AlbumId);
		return Component.ResolveTemplateUrl(Parameters.GetString("CommentAddUrlTemplate", ""), replace);
	}

	string picList = null;
	protected string PicturesList(bool getCount)
	{
		if (picList != null)
			return picList;
		StringBuilder s = new StringBuilder();
		int pageNum=0;
		if ( !int.TryParse(Component.Parameters["PageId"],out pageNum))
			pageNum = 1;
		s.AppendFormat("{{start_number:{0},elements_count:{1},recsPerPage:{2},elements:",
			(pageNum - 1) * Component.Parameters.GetInt("PagingRecordsPerPage") + 1, getCount ? Component.TotalFotoCount : 0, Component.Parameters.GetInt("PagingRecordsPerPage", Component.PhotoItems.Count));
		s.Append("[");

			var b = new BXParamsBag<object>();
				b.Add("ALBUMID", Component.AlbumId);
			for (int i = 0; i < Component.PhotoItems.Count; i++)
			{
				b.Add("PHOTOID", Component.PhotoItems[i].Id);
				s.AppendFormat("{{url:'{0}',title:'{1}',page:'{3}',id:{2},editurl:'{4}',height:{5},width:{6}, origtext:'{7}',description:'{8}',photourl:'{9}',{10},commenturl:'{11}'}}",
					Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].DetailUrl,
					Encode(Component.PhotoItems[i].Name),
					Component.PhotoItems[i].Id,
					pageNum,
					Component.MakeLink(Component.Parameters.Get("UrlTemplatePhotoEdit", BXConfigurationUtility.Constants.ErrorHref), b),
					Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].DetailHeight,
					Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].DetailWidth,
					GetOriginalText(Component.PhotoItems[i]),
					JSEncode(Component.PhotoItems[i].DetailText),
					Component.PhotoDescriptionDictionary[Component.PhotoItems[i]].PhotoUrl,
					GetVotingInfo(Component.PhotoItems[i]),
					GetCommentUrl(Component.PhotoItems[i].Id)
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

		if (Request.QueryString["action"] == "vote" && Component.IsVotingAllowed && Component.EnableVoting)
		{
			int photoId = 0;
			if (int.TryParse(Request.QueryString[Component.Parameters.GetString("ParamPhoto","photo")], out photoId) && !String.IsNullOrEmpty(Request.QueryString["sign"]))
			{
				BXRatingVotingTotals totals =  Component.Vote(photoId, Request.QueryString["sign"]);
				Response.Clear();
				Response.StatusCode = 200;
				Response.ContentType = "application/json";
				Response.Write( totals!=null ? String.Concat("{total_value:",totals.Value.ToString(),"}"): "(false)");
				Response.End();
			}
		}

        if (Component.Album != null)
        {
            lButtonDelete.Visible = true;
            lButtonDelete.OnClientClick = string.Format("return confirm(\"{0}\");", JSEncode(GetMessageRaw("ConfirmText.DoYouReallyWantRemoveThisAlbum")));
        }
		
		string target = Request.QueryString["target"] ?? Request.Form["target"];
		string ids = Request.QueryString["del"] ?? Request.Form["del"];
		string validationToken = Request.QueryString[BXCsrfToken.TokenKey] ?? Request.Form[BXCsrfToken.TokenKey];
		if ((ids != null && validationToken != null ||  target !=null
			&& Bitrix.Security.BXCsrfToken.CheckToken(validationToken ) ))
		{

			if (target == "album")
			{
				DeleteAlbum(ids);
			}
			else if (target == "photos")
			{
				DeletePhotos(ids);
			}
		}
    }

	protected void DeleteAlbum(string albumId)
	{
		int i = 0;
		if (!int.TryParse(albumId, out i))
			return;
		//delete album
		BXParamsBag<object> par = new BXParamsBag<object>();
		par["AlbumId"] = i;
		List<string> err = new List<string>();
		if (!Component.ProcessCommand("delete", par, err))
		{
		}
	}

	protected void DeletePhotos(string ids)
	{
		if (ids == null) return;
		List<string> tmpList = BXStringUtility.CsvToList(ids);
		int i=0;
		List<int> photoIds = new List<int>();
		foreach(string s in tmpList)
			if ( int.TryParse(s,out i))
				photoIds.Add(i);
		if (photoIds.Count == 0) return;
		List<string> err = new List<string>();
		BXParamsBag<object> par = new BXParamsBag<object>();
		par["photoIds"] = photoIds;
		if (!Component.ProcessCommand("deletephotos", par, err))
		{
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

				return pathForQuery = Component.ResolveTemplateUrl(Component.Parameters["UrlTemplateAlbumPage"], replace) + "?ajax=true&ValidationToken=" + BXCsrfToken.GenerateToken();;
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
