using System;
using System.Collections.Generic;
using System.Web.UI;
using Bitrix;

using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.UI;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Services;
using Bitrix.Configuration;
using System.Collections.Specialized;
using System.Web;
using Bitrix.Modules;
using Bitrix.CommunicationUtility.Rating;
using Bitrix.Services.Rating;
using Bitrix.Security;
using Bitrix.Services.Text;

public partial class PhotogalleryAlbum : BXComponent
{
	//PROPERTIES
	public BXInfoBlockSectionOld Album
	{
		get
		{
			return BXInfoBlockSectionManagerOld.ConvertToOldElement(Results.Get<BXIBlockSection>("CurrentAlbum", null));
		}
	}

	public BXIBlockSection AlbumItem
	{
		get
		{
			return Results.Get<BXIBlockSection>("CurrentAlbum", null);
		}
		set
		{
			Results["CurrentAlbum"] = value;
		}
	}

	public int AlbumId
	{
		#region old
		//get
		//{
		//    if (!IsComponentDesignMode)
		//    {
		//        return Results.Get("AlbumId", 0);
		//    }
		//    return 0;
		//}
		#endregion
		get
		{
			return Parameters.Get<int>("AlbumID", 0);
		}
	}

	public int IBlockId
	{
		get
		{
			return Parameters.Get("IBlockId", 0);
		}
	}

	public int ParentAlbumId
	{
		get
		{
			return Results.Get("ParentAlbumId", 0);
		}
		set
		{
			Results["ParentAlbumId"] = value;
		}
	}

	public string BackUrl
	{
		get { return Results.Get("BackUrl", ""); }
		set { Results["backUrl"] = value; }
	}

	public string UploadUrl
	{
		get { return Results.Get("UploadUrl", ""); }
		set { Results["UploadUrl"] = value; }
	}

	public string EditUrl
	{
		get { return Results.Get("EditUrl", ""); }
		set { Results["EditUrl"] = value; }
	}

	public string AddUrl
	{
		get { return Results.Get("AddUrl", ""); }
		set { Results["AddUrl"] = value; }
	}

	public bool IsParent
	{
		get { return Results.Get("IsParent", false); }
		set { Results["IsParent"] = value; }
	}

	public bool CanModify
	{
		get { return Results.Get("CanModify", false); }
		set { Results["CanModify"] = value; }
	}

	public bool IsNested
	{
		get { return Results.Get("IsNested", false); }
		set { Results["IsNested"] = value; }
	}

	public string AlbumCoverUrl
	{
		get { return Results.Get("AlbumCoverUrl", ""); }
		set { Results["AlbumCoverUrl"] = value; }
	}

	public int PositiveVoteValue
	{
		get { return Parameters.GetInt("PositiveVoteValue", 1); }
	}

	public int NegativeVoteValue
	{
		get { return Parameters.GetInt("NegativeVoteValue", -1); }
	}

	BXInfoBlockElementCollectionOld photos;

	public BXInfoBlockElementCollectionOld Photos
	{
		get
		{
			return photos ?? (photos = BXInfoBlockElementManagerOld.ConvertToOldElements(Results.Get("Photos", new BXIBlockElementCollection())));
		}
	}

	public BXIBlockElementCollection PhotoItems
	{
		get
		{
			return Results.Get("Photos", new BXIBlockElementCollection());
		}
		set
		{
			Results["Photos"] = value;
		}
	}

	BXInfoBlockSectionCollectionOld _sections = null;
	BXInfoBlockElementCollectionOld _photoDescription = null;

	public BXInfoBlockSectionCollectionOld Sections
	{
		get
		{
			/*
			if (_sections != null)
				return _sections;
			_sections = BXInfoBlockSectionManagerOld.ConvertToOldElements(Results.Get("Sections", new BXIBlockSectionCollection()));
			return _sections;
			*/
			return _sections ?? (_sections = BXInfoBlockSectionManagerOld.ConvertToOldElements(Results.Get("Sections", new BXIBlockSectionCollection())));

		}
	}

	protected BXInfoBlockSectionOld GetSectionOldById(int id)
	{
		foreach (BXInfoBlockSectionOld s in Sections)
			if (s.SectionId == id)
				return s;
		return null;
	}

	protected BXInfoBlockElementOld GetElementOldById(int id)
	{
		foreach (BXInfoBlockElementOld item in Photos)
			if (item.ElementId == id) return item;

		return null;

	}

	protected int CurrentUserId
	{
		get
		{
			return  BXPrincipal.Current.Identity.IsAuthenticated ? ((BXIdentity)BXPrincipal.Current.Identity).Id : 0;
		}
	}


	public BXIBlockSectionCollection SectionItems
	{
		get { return Results.Get("Sections", new BXIBlockSectionCollection()); }
		set { Results["Sections"] = value; }
	}


	public int PreviewHeight
	{
		get
		{
			return Parameters.Get("PreviewHeight", 75);
		}
	}

	public int PreviewWidth
	{
		get
		{
			return Parameters.Get("PreviewWidth", 75);
		}
	}

	public int CoverHeight
	{
		get
		{
			return Parameters.Get("CoverHeight", 75);
		}
	}

	public int CoverWidth
	{
		get
		{
			return Parameters.Get("CoverWidth", 75);
		}
	}

	public string AlbumSortBy
	{
		get { return Parameters.GetString("AlbumSortBy", "ID"); }
	}

	public string AlbumSortOrder
	{
		get { return Parameters.GetString("AlbumSortOrder", "ASC"); }
	}

	public string PhotoSortBy
	{
		get { return Parameters.GetString("PhotoSortBy", "ID"); }
	}

	public string PhotoSortOrder
	{
		get { return Parameters.GetString("PhotoSortOrder", "ASC"); }
	}

	public Dictionary<BXInfoBlockSectionOld, AlbumDescription> AlbumDescription
	{
		get
		{
			return ConvertToOldAlbumDescription();
		}
	}

	public Dictionary<BXIBlockSection, AlbumDescription> AlbumDescriptionDictionary
	{
		get
		{
			return Results.Get("AlbumDescription", new Dictionary<BXIBlockSection, AlbumDescription>());
		}
		set
		{
			Results["AlbumDescription"] = value;
		}
	}
	Dictionary<BXInfoBlockElementOld, PhotoDescription> _photoDescriptionDic;


	public Dictionary<BXInfoBlockElementOld, PhotoDescription> PhotoDescription
	{
		get
		{
			return _photoDescriptionDic ?? (_photoDescriptionDic = GetOldPhotoDescription());
		}
	}

	Dictionary<BXIBlockElement, PhotoDescription> _photoDescriptionDictionary;

	public Dictionary<BXIBlockElement, PhotoDescription> PhotoDescriptionDictionary
	{
		get
		{
			return _photoDescriptionDictionary ?? (_photoDescriptionDictionary = Results.Get("PhotoDescription", new Dictionary<BXIBlockElement, PhotoDescription>()));
		}
		set
		{
			Results["PhotoDescription"] = value;
		}
	}

	public string errorMessage = String.Empty;

	int totalFotoCount = -1;
	public int TotalFotoCount
	{
		get
		{
			if (totalFotoCount != -1) return totalFotoCount;
			return (totalFotoCount = BXIBlockElement.Count(
				new BXFilter(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, Parameters.Get("IBlockId", 0)),
							new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, AlbumId))));
		}
	}


	//METHODS
	protected void Page_Load(object sender, EventArgs e)
	{
		if (IBlockId > 0)
		{
			//if (EnableSef)
			//{
			//	MapVariable(Parameters.Get<string>("SEFFolder", "/photogallery"), Parameters.Get<string>("SEF_Album"), Results);
			//}
			//else
			//{
			//    BXParamsBag<string> variableAlias = new BXParamsBag<string>();
			//    variableAlias["AlbumID"] = Parameters.Get<string>("ParamAlbum", "Album");
			//    variableAlias["PageID"] = Parameters.Get<string>("ParamAlbumPage", "Album");
			//    variableAlias["PageShowAll"] = Parameters.Get<string>("ParamAlbumShowAll", "Album");
			//    MapVariable(variableAlias, Results);

			//    if (AlbumId > 0)
			//    {
			//        Results["AlbumParentID"] = BXInfoBlockSectionManagerOld.GetById(AlbumId).ParentSectionId;
			//    }
			//}

			int albumParent = 0;


			if (BXIBlock.IsUserCanOperate(IBlockId, BXIBlock.Operations.IBlockModifySections))
				CanModify = true;

			if (AlbumId > 0)
			{
				//load album
				AlbumItem = BXIBlockSection.GetById(AlbumId, Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder);

				if (AlbumItem != null)
				{
					albumParent = AlbumItem.SectionId;



					if (Page is BXPublicPage)
						((BXPublicPage)Page).MasterTitle = Server.HtmlDecode(AlbumItem.Name);
					Page.Title = AlbumItem.Name;
					IsNested = true;

					BXFile image = AlbumItem.Image;

					if (image != null)
					{
						Bitrix.Services.Image.BXImageInfo imageInfo = Bitrix.Services.Image.
							   BXImageUtility.GetResizedImage(image, Parameters.Get("PreviewWidth", 75),
							   Parameters.Get("PreviewHeight", 75));

						AlbumCoverUrl = imageInfo.GetUri();
					}

					FillNavChain();
				}
			}

			BackUrl = GetAlbumUrl(albumParent);

			UploadUrl = MakeLink(Parameters.Get<string>("UrlTemplateUpload", BXConfigurationUtility.Constants.ErrorHref), AlbumId);
			EditUrl = MakeLink(Parameters.Get<string>("UrlTemplateAlbumEdit", BXConfigurationUtility.Constants.ErrorHref), AlbumId);
			AddUrl = MakeLink(Parameters.Get<string>("UrlTemplateAlbumAdd", BXConfigurationUtility.Constants.ErrorHref), AlbumId);

			BXFilter filter = new BXFilter();
			BXPagingParams pagingParams = new BXPagingParams();
			if (AlbumItem != null)
			{

				filter.Add(new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, Parameters.Get("IBlockId", 0)));
				filter.Add(new BXFilterItem(BXIBlockSection.Fields.SectionId, BXSqlFilterOperators.Equal, AlbumItem.Id));
				filter.Add(new BXFilterItem(BXIBlockSection.Fields.DepthLevel, BXSqlFilterOperators.Less, Album.DepthLevel + 2));

				BXFilter photoFilter = new BXFilter();
				photoFilter.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, Parameters.Get("IBlockId", 0)));
				photoFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, Album.SectionId));

				BXParamsBag<object> mapParams = new BXParamsBag<object>();
		
				mapParams["AlbumID"] = AlbumId;
				
				
				pagingParams.AllowPaging = true;
				string pageId = Request.QueryString["page"];
				int photoId = Parameters.Get<int>("PhotoId",0);// если шаблон фото детально передал параметр, 
				//значит нужно показать элемент вместе с текущей страницей для вывода в слайдер
				pagingParams = PreparePagingParams();
				if (photoId > 0 && pageId==null)
				{
					//Determine page
					
					BXIBlockElement currentPhoto = BXIBlockElement.GetById(photoId);

					int count = BXIBlockElement.Count(new BXFilter(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, AlbumItem.Id),
																	new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, currentPhoto.IBlockId)));


					if (currentPhoto != null)
					{
						BXFilter countFilter = new BXFilter();
						countFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Less, currentPhoto.Id));
						countFilter.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, currentPhoto.IBlockId));
						countFilter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, AlbumItem.Id));

						int index = BXIBlockElement.Count(countFilter);

						BXPagingHelper helper = ResolvePagingHelper(count, pagingParams);
						pagingParams.Page = helper.GetOuterIndex(helper.GetPageIndexForItem(index));
						Component.Parameters["PagingPageId"] = (pagingParams.Page ?? 1).ToString();
						Component.Parameters["PageId"] = (pagingParams.Page ?? 1).ToString();
					}
				}




				BXQueryParams queryParams = PreparePaging(
					pagingParams,
										delegate
										{
											return BXIBlockElement.Count(photoFilter);
										},
					mapParams,
					"PageID",
					Parameters.Get<string>("UrlTemplateAlbum"),
					Parameters.Get<string>("UrlTemplateAlbumPage"),
					Parameters.Get<string>("UrlTemplateAlbumShowAll")
					);
				var photoOrderBy = new BXOrderBy();
				photoOrderBy.Add(BXIBlockElement.Fields, String.Format("{0} {1}", this.PhotoSortBy, this.PhotoSortOrder));
				PhotoItems = BXIBlockElement.GetList(photoFilter, photoOrderBy, null, queryParams, Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder);
				PhotoDescriptionDictionary = new Dictionary<BXIBlockElement, PhotoDescription>();
				//Component.Parameters["PageId"] = (pagingParams.Page ?? 1).ToString();
				ComponentCache["PageId"] = (pagingParams.Page ?? 1).ToString();
				if (Photos.Count > 0)
				{
					foreach (BXIBlockElement photo in PhotoItems)
					{
						PhotoDescription photoDesc = new PhotoDescription();

						photoDesc.Preview = string.Empty;

						BXFile image = photo.DetailImageId > 0 ? photo.DetailImage : photo.PreviewImage;

						if (image == null)
							continue;

						Bitrix.Services.Image.BXImageInfo imageInfo = Bitrix.Services.Image.
						BXImageUtility.GetResizedImage(image, Parameters.Get("PreviewWidth", 75),
						Parameters.Get("PreviewHeight", 75));
						photoDesc.Preview = imageInfo.GetUri();
						photoDesc.DetailUrl = image.GetUri();
						BXParamsBag<object> replace = new BXParamsBag<object>();
						replace.Add("PHOTOID", photo.Id);
						replace.Add("ALBUMID", AlbumId);
						if (!EnableSef)
						{
							photoDesc.PhotoUrl =
								string.Format("?{0}={1}", Parameters.Get<string>("ParamPhoto", "photo"), photo.Id);
						}
						else
						{
							photoDesc.PhotoUrl =
								MakeLink(Parameters.Get<string>("SEFFolder"), Parameters.Get<string>("SEF_Photo"), replace);
						}

						photoDesc.ActualCoverHeight = imageInfo.Height;
						photoDesc.ActualCoverWidth = imageInfo.Width;
						photoDesc.CurUserId = CurrentUserId;
						photoDesc.DetailHeight = image.Height;
						photoDesc.DetailWidth = image.Width;
						photoDesc.Element = photo;
						PhotoDescriptionDictionary[photo] = photoDesc;
						

					}
				}

				ParentAlbumId = Album.ParentSectionId;
			}
			else
			{
				filter.Add(new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, Parameters.Get("IBlockId", 0)));
				filter.Add(new BXFilterItem(BXIBlockSection.Fields.DepthLevel, BXSqlFilterOperators.Less, 2));
				if (Page is BXPublicPage)
					((BXPublicPage)Page).MasterTitle = GetMessageRaw("Photogallery");
				Page.Title = GetMessageRaw("Photogallery");
				ParentAlbumId = 0;
			}
			var albumOrderBy = new BXOrderBy();
			albumOrderBy.Add(BXIBlockSection.Fields, String.Format("{0} {1}", this.AlbumSortBy, this.AlbumSortOrder));
			SectionItems = BXIBlockSection.GetList(filter, albumOrderBy, null, null, Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder);
			AlbumDescriptionDictionary = new Dictionary<BXIBlockSection, AlbumDescription>();

			if ((Sections.Count != 0) && (AlbumId > 0))
				IsParent = true;

			foreach (BXIBlockSection section in SectionItems)
			{
				AlbumDescription albumDesc = new AlbumDescription();

				filter = new BXFilter();
				filter.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, Parameters.Get("IBlockId", 0)));
				filter.Add(new BXFilterItem(BXIBlockElement.Fields.Sections.Section.ID, BXSqlFilterOperators.Equal, section.Id));

				BXIBlockElementCollection elements = BXIBlockElement.GetList(filter, null);

				albumDesc.CoverImageUrl =
					BXUri.ToRelativeUri(
						"~/bitrix/components/bitrix/photogallery.album/templates/.default/images/no_image.png");

				albumDesc.Count = elements.Count;

				albumDesc.Url = GetAlbumUrl(section.Id);
				albumDesc.EditUrl = MakeLink(Parameters.Get<string>("UrlTemplateAlbumEdit", BXConfigurationUtility.Constants.ErrorHref), section.Id);

				BXFile image = section.Image;

				if (image == null)
					foreach (BXIBlockElement element in elements)
					{
						image = element.PreviewImage;
						if (image != null)
							break;
					}

				albumDesc.IsCoverEmpty = true;
				if (image != null)
				{
				    Bitrix.Services.Image.BXImageInfo imageInfo = Bitrix.Services.Image.
					    BXImageUtility.GetResizedImage(
                        image, Parameters.Get("CoverWidth", 75),
					    Parameters.Get("CoverHeight", 75));

				    albumDesc.CoverImageUrl = imageInfo.GetUri();
				    albumDesc.IsCoverEmpty = false;
				}

				AlbumDescriptionDictionary[section] = albumDesc;

			}

		}

		IncludeComponentTemplate();
	}

	private void FillNavChain()
	{
		BXMenu menu = BXMenuManager.Load(Request.AppRelativeCurrentExecutionFilePath, "left");

		BXIBlockSectionCollection navChain = BXIBlockSection.GetNavChain(AlbumId);

		for (int i = 0; i < navChain.Count; i++)
		{
			string albumUrl =
				MakeLink(Parameters.Get<string>("UrlTemplateAlbum", BXConfigurationUtility.Constants.ErrorHref), navChain[i].SectionId);

			string parentAlbumUrl;

			if (i != 0)
			{
				parentAlbumUrl =
					MakeLink(Parameters.Get<string>("UrlTemplateAlbum", BXConfigurationUtility.Constants.ErrorHref), navChain[i].SectionId);
			}
			else
			{
				parentAlbumUrl = Parameters.Get<string>("GalleryRoot");
			}

			BXSiteMapManager.AddNode(
				parentAlbumUrl,
				albumUrl,
				navChain[i].Name);
		}
	}

	public override bool ProcessCommand(string commandName, BXParamsBag<object> commandParameters, List<string> commandErrors)
	{
		string url;
		if ( commandName.Equals("delete",StringComparison.OrdinalIgnoreCase)){
			int delAlbumId = commandParameters.GetInt("AlbumId", AlbumId);


			int parentAlbumId = 0;
			try
			{
				parentAlbumId = BXIBlockSection.GetById(delAlbumId).SectionId;
				BXIBlockSection.Delete(delAlbumId);
			}
			catch (Exception ex)
			{
				errorMessage = GetMessage("Error.FailedToDeleteAlbum");
			}

			if (EnableSef)
			{
				if (parentAlbumId > 0)
				{
					Results["AlbumId"] = parentAlbumId;
					url = MakeLink(Parameters.Get<string>("SefFolder"), Parameters.Get<string>("Sef_Album"), Results);
				}
				else
				{
					url = MakeLink(Parameters.Get<string>("SefFolder"), string.Empty, Results);
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
			if (errorMessage == String.Empty)
			{
				if (Parameters.Get("EnableAjax", false) && !IsComponentDesignMode)
				{
					string script = string.Format("setTimeout(\"PageTransfer('{0}');\",1);",
												  url);
					ScriptManager.RegisterStartupScript(Page, typeof(Page), "AJAX_BUTTON_CLICK", script, true);
				}
				else
					Response.Redirect(url);
				return true;
			}


		}
		else if (commandName.Equals("deletephotos", StringComparison.OrdinalIgnoreCase))
		{
			var photos = commandParameters.Get<List<int>>("photoIds", new List<int>());

			foreach (var id in photos)
			{
				BXIBlockElement photo = null;
				try
				{
					photo = BXIBlockElement.GetById(id);
					if (photo != null)
						photo.Delete();
				}
				catch (Exception ex2)
				{
					commandErrors.Add(GetMessage("UnableToDeletePhoto") + (photo != null ? " " + photo.Name : ""));
				}

			}
			if (BXModuleManager.IsModuleInstalled("forum") && EnableComments)// have to delete comments for all photos
			{
				var c = new IncludeComponent("bitrix:forum.comment.block", "blog_comments");
				c.Attributes["ForumId"] = Component.Parameters.GetString("CommentForumId", "0");
				c.Attributes["IdentityPropertyName"] = Component.Parameters.GetString("CommentStoragePropertyName");
				c.Attributes["IdentityPropertyValue"] = photos[0].ToString();//required by design
				c.Attributes["PostOperation"] = "DeleteCommentsForEntityList";
				c.Attributes["EntityList"] = BXStringUtility.ListToCsv(photos.ConvertAll(x => x.ToString()));
				CommentsDeletePlaceHolder.Controls.Add(c);
			}
			if (EnableSef)
			{

					if (AlbumId > 0)
					{
						Results["AlbumId"] = AlbumId;
						url = MakeLink(Parameters.Get<string>("SefFolder"), Parameters.Get<string>("Sef_Album"), Results);
					}
					else
					{
						url = MakeLink(Parameters.Get<string>("SefFolder"), string.Empty, Results);
					}
			}
			else
			{
						NameValueCollection pars = HttpUtility.ParseQueryString(Request.Url.Query);
						pars.Remove("target");
						pars.Remove("ValidationToken");
						pars.Remove("del");
						url = string.Format("{0}?{1}",
													Request.Path,
													pars.ToString());
			}
			
			Response.Redirect(url);
		}
		
		return false;

	}

	protected override void PreLoadComponentDefinition()
	{
		Title = GetMessage("PhotogalleryAlbum.Title");
		Description = GetMessage("PhotogalleryAlbum.Description");
		Icon = "images/icon.gif";

		ParamsDefinition.Add(BXParametersDefinition.Paging);
	}

	protected string GetAlbumUrl(int albumId)
	{
		if (albumId == 0)
			return Parameters.Get<string>("GalleryRoot", Request.Path);
		else
			return MakeLink(Parameters.Get<string>("UrlTemplateAlbum", BXConfigurationUtility.Constants.ErrorHref), albumId);
	}

	public Dictionary<BXInfoBlockSectionOld, AlbumDescription> ConvertToOldAlbumDescription()
	{
		Dictionary<BXInfoBlockSectionOld, AlbumDescription> result = new Dictionary<BXInfoBlockSectionOld, AlbumDescription>();
		foreach (BXIBlockSection item in AlbumDescriptionDictionary.Keys)
		{

			result.Add(GetSectionOldById(item.Id), AlbumDescriptionDictionary[item]);
		}
		return result;
	}

	public bool EnableVoting
	{
		get { return Parameters.GetBool("EnableVoting", false); }
	}

	public bool EnableComments
	{
		get { return Parameters.GetBool("EnableComments", false); }
	}

	protected Dictionary<BXInfoBlockElementOld, PhotoDescription> GetOldPhotoDescription()
	{
		Dictionary<BXInfoBlockElementOld, PhotoDescription> result = new Dictionary<BXInfoBlockElementOld, PhotoDescription>();
		foreach (BXIBlockElement item in PhotoDescriptionDictionary.Keys)
		{
			result.Add(GetElementOldById(item.Id), PhotoDescriptionDictionary[item]);
		}
		return result;
	}

	private IList<string> rolesAuthorizedToVote = null;
	public IList<string> RolesAuthorizedToVote
	{
		get { return this.rolesAuthorizedToVote ?? (this.rolesAuthorizedToVote = Parameters.GetListString("VotingRoles")); }
	}

	private bool? isVotingAllowed = null;
	public bool IsVotingAllowed
	{
		get
		{
			if (this.isVotingAllowed.HasValue)
				return this.isVotingAllowed.Value;

			if (RolesAuthorizedToVote.Count > 0 && BXPrincipal.Current != null)
				foreach (string r in BXPrincipal.Current.GetAllRoles())
					if (RolesAuthorizedToVote.Contains(r))
						return (this.isVotingAllowed = true).Value;

			return (this.isVotingAllowed = false).Value;
		}
	}

	public BXRatingVotingTotals Vote(int photoId, string sign)
	{
		if (!EnableVoting || !IsVotingAllowed) return null;
		var photo = BXIBlockElement.GetById(photoId);
		if (photo == null || photo.IBlockId!=IBlockId) return null;

		var desc = new PhotoDescription();
		desc.Element = photo;
		desc.CurUserId = CurrentUserId;

		bool sgn = sign == "true";

		BXRatingVote vote = new BXRatingVote();
		vote.Active = true;

		vote.RatingVotingId = desc.Voting.Id;

		if (desc.VotingTotals.UserHasVoted || !desc.IsVotingAllowed)
		{
			return null;
		}
		//vote.Value = sign ? Voting.Config.PlusValue : Voting.Config.MinusValue;
		vote.Value = sgn ? PositiveVoteValue : NegativeVoteValue;
		vote.UserId = CurrentUserId;
		vote.UserIP = Request.UserHostAddress;

		vote.Create();
		desc.VotingTotals = null;
		return desc.VotingTotals;
	}


}



public class AlbumDescription
{
	string url;
	string editUrl;
	string coverImageUrl;
	int count;
	bool isCoverEmpty;

	public bool IsCoverEmpty
	{
		get { return isCoverEmpty; }
		set { isCoverEmpty = value; }
	}



	public string Url
	{
		get { return url; }
		set { url = value; }
	}

	public string EditUrl
	{
		get { return editUrl; }
		set { editUrl = value; }
	}

	public string CoverImageUrl
	{
		get { return coverImageUrl; }
		set { coverImageUrl = value; }
	}

	public int Count
	{
		get { return count; }
		set { count = value; }
	}

}

public class PhotoDescription
{
	string preview;
	string photoUrl;
	int actualCoverWidth;
	int actualCoverHeight;
	int detailWidth;
	int detailHeight;
	string detailUrl;
	int curUserId;
	BXIBlockElement element;

	public bool IsVotingAllowed
	{
		get
		{
			return !VotingTotals.UserHasVoted && element.CreatedBy != curUserId;
		}
	}

	public BXIBlockElement Element
	{
		get { return element; }
		set { element = value; }
	}

	public int CurUserId
	{
		get { return curUserId; }
		set { curUserId = value; }
	}

	public string Preview
	{
		get { return preview; }
		set { preview = value; }
	}

	public string DetailUrl
	{
		get { return detailUrl; }
		set { detailUrl = value; }
	}

	public int DetailWidth
	{
		get { return detailWidth; }
		set { detailWidth = value; }
	}

	public int DetailHeight
	{
		get { return detailHeight; }
		set { detailHeight = value; }
	}

	public string PhotoUrl
	{
		get { return photoUrl; }
		set { photoUrl = value; }
	}

	public int ActualCoverWidth
	{
		get { return actualCoverWidth; }
		set { actualCoverWidth = value; }
	}


	public int ActualCoverHeight
	{
		get { return actualCoverHeight; }
		set { actualCoverHeight = value; }
	}

	// Voting


	private bool isVotingLoaded = false;
	private BXRatingVoting voting = null;
	public BXRatingVoting Voting
	{
		get
		{
			if (this.isVotingLoaded)
				return this.voting;

			this.isVotingLoaded = true;

			if (VotingTotals != null && VotingTotals.RatingVotingId > 0)
				this.voting = BXRatingVoting.GetById(VotingTotals.RatingVotingId);

			if (this.voting != null)
				return this.voting;

			this.voting = BXRatingVoting.CreateIfNeed(new BXRatedItem("IBlockElement", element.Id.ToString(), "iblock_"+element.IBlockId.ToString()));

			return this.voting;
		}
	}

	private bool isVotingTotalsLoaded = false;
	private BXRatingVotingTotals votingTotals = null;
	public BXRatingVotingTotals VotingTotals
	{
		get
		{
			if (this.isVotingTotalsLoaded)
				return this.votingTotals;

			this.isVotingTotalsLoaded = true;

			this.votingTotals = BXRatingVotingTotals.Create("IBlockElement",element.Id.ToString(),curUserId);

			return this.votingTotals;
		}
		set
		{
			if ( value==null )
				isVotingTotalsLoaded = false;
			votingTotals = value;
		}
	}


}