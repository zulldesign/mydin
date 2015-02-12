using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;

namespace Bitrix.IBlock.Components
{
	public partial class PhotoGalleryComponent : BXComponent
	{

		const int DefaultPreviewWidth = 75;
		const int DefaultPreviewHeight = 75;
		const int DefaultPhotoWidth = 150;
		const int DefaultPhotoHeight = 150;

		string AlbumVariable
		{
			get { return Parameters.GetString("ParamAlbum", "Album"); }
		}

		string ActionVariable
		{
			get { return Parameters.GetString("ParamAction", "act"); }
		}

		string PhotoVariable
		{
			get { return Parameters.GetString("ParamPhoto", "photo"); }
		}

		string CommentVariable
		{
			get { return Parameters.GetString("ParamComment", "comment"); }
		}

		private string PageVariable
		{
			get
			{
				return Parameters.GetString("ParamAlbumPage", "");
			}
		}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string page = EnableSef ? PrepareSefMode() : PrepareNormalMode();
            ComponentCache["PageShowAll"] = ComponentCache.ContainsKey("PageShowAll");
			//var pms = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
			//pms.Remove(BXConfigurationUtility.Constants.BackUrl);
			//pms.Add(
            IncludeComponentTemplate(page);
        }

		private int? ReadQueryInt(string param)
		{
			string[] values = Request.QueryString.GetValues(param);
			if (values == null || values.Length == 0)
				return null;
			int i;
			if (!int.TryParse(values[0], out i))
				return null;
			return i;
		}

		private string AddBackUrl(string uriStr)
		{
			var result=string.Empty;
			var qIndex = uriStr.IndexOf('?');
			return uriStr +((qIndex != -1 && qIndex != uriStr.Length -1) ? "&": "?") + BXConfigurationUtility.Constants.BackUrl + "=" + UrlEncode(BXSefUrlManager.CurrentUrl.ToString());
			
		}
		private string PrepareSefMode()
		{
			string sefFolder = Parameters.GetString("SEFFolder", "/photogallery");
			ComponentCache["UrlTemplateAlbum"] = CombineLink(sefFolder, Parameters.GetString("SEF_Album"));
			ComponentCache["UrlTemplateAlbumPage"] = CombineLink(sefFolder,Parameters.Get<string>("SEF_AlbumPage") );
			ComponentCache["UrlTemplateAlbumShowAll"] = CombineLink(sefFolder, Parameters.Get<string>("SEF_AlbumShowAll"));
			ComponentCache["UrlTemplateAlbumEdit"] = AddBackUrl(CombineLink(sefFolder, Parameters.Get<string>("SEF_AlbumEdit")));
			ComponentCache["UrlTemplatePhoto"] = CombineLink(sefFolder, Parameters.Get<string>("SEF_Photo", "/photo/#PHOTOID#"));
			ComponentCache["UrlTemplatePhotoEdit"] = CombineLink(sefFolder, Parameters.Get<string>("SEF_PhotoEdit"));
			ComponentCache["UrlTemplateUpload"] = CombineLink(sefFolder, Parameters.Get<string>("SEF_Upload"));
			ComponentCache["UrlTemplateAlbumAdd"] = CombineLink(sefFolder, Parameters.Get<string>("SEF_AlbumAdd"));
			ComponentCache["CommentReadUrlTemplate"] = CombineLink(sefFolder, Parameters.Get<string>("CommentReadUrlTemplate"));
			ComponentCache["CommentReadPageUrlTemplate"] = CombineLink(sefFolder, Parameters.Get<string>("CommentReadPageUrlTemplate"));
			ComponentCache["CommentOperationUrlTemplate"] = CombineLink(sefFolder, Parameters.Get<string>("CommentPostOperationUrlTemplate"));

			ComponentCache["CommentAddUrlTemplate"] = CombineLink(sefFolder, Parameters.GetString("CommentPostAddUrlTemplate",""));
			return PrepareSefPage();
		}

		private string PrepareSefPage()
		{
			BXParamsBag<string> urlToPage = new BXParamsBag<string>();
			urlToPage["album.edit"] = Parameters.Get<string>("SEF_AlbumEdit");
			urlToPage["album.add"] = Parameters.Get<string>("SEF_AlbumAdd");
			urlToPage["upload"] = Parameters.Get<string>("SEF_Upload");
			urlToPage["album"] = Parameters.Get<string>("SEF_Album");
			urlToPage["album.page"] = Parameters.Get<string>("SEF_AlbumPage");
			urlToPage["album.showall"] = Parameters.Get<string>("SEF_AlbumShowAll");
			urlToPage["photo.edit"] = Parameters.Get<string>("SEF_PhotoEdit");
			urlToPage["photo"] = Parameters.Get<string>("SEF_Photo", "/photo/#PHOTOID#");
			urlToPage["search"] = "/search?q=#QUERY#";
			urlToPage["photo.comment"] = Parameters.Get<string>("CommentReadUrlTemplate", "/photo/#PHOTOID#/comments/#COMMENTID#/");
			urlToPage["photo.comment.page"] = Parameters.Get<string>("CommentReadPageUrlTemplate", "/photo/#PHOTOID#/comments/page-#PAGEID#/");
			urlToPage["photo.comment.operation"] = Parameters.Get<string>("CommentPostOperationUrlTemplate", "/photo/#PHOTOID#/comments/#COMMENTID#/#OPERATION#/");
			urlToPage["photo.comment.operation.add"] = Parameters.Get<string>("CommentAddUrlTemplate", "/photo/#PHOTOID#/comments/add/");

			string page = BXSefUrlUtility.MapVariable(Parameters.GetString("SEFFolder", ""), urlToPage, ComponentCache, "album", null, null);

			if (page == "album.page")
				page = "album";
			else if (page == "album.showall")
			{
				page = "album";
				Results["PageShowAll"] = true;
			}
			else if (page == "album.edit")
			{
				ComponentCache["Action"] = "edit";
			}
			else if (page == "album.add")
			{
				ComponentCache["Action"] = "add";
			}
			else if (page == "photo.comment" || page == "photo.comment.page" || page == "photo.comment.operation")
			{
				page = "photo";
			}
			else if (page == "photo.comment.operation.add")
			{
				page = "photo";
				ComponentCache["Operation"] = "add";
			}


			Results["GalleryRoot"] = MakeLink(Parameters.Get<string>("SEFFolder", "/photogallery"), "");

			if (page == "photo.comment" || page == "photo.comment.page" || page == "photo.comment.operation")
			{
				Results["SelectedPage"] = page;
				page = "photo";
			}
			return page;
		}

		private string PrepareNormalMode()
		{
			string path = BXSefUrlManager.CurrentUrl.AbsolutePath;
			string albumVar = UrlEncode(AlbumVariable);
			string commentVar = UrlEncode(CommentVariable);
			string actionVar = UrlEncode(ActionVariable);
			string photoVar = UrlEncode(PhotoVariable);
			string pageVar = UrlEncode(PageVariable);

			ComponentCache["UrlTemplateAlbum"] = string.Format("{0}?{1}=#AlbumId#", path, AlbumVariable);
			ComponentCache["UrlTemplateAlbumPage"] = string.Format("{0}?{1}=#AlbumId#&{2}=#PageId#", path, albumVar, pageVar);
			ComponentCache["UrlTemplateAlbumShowAll"] = string.Format("{0}?{1}=#AlbumId#&{2}=", path, albumVar, Parameters.Get<string>("ParamAlbumShowAll", "Page"));
			ComponentCache["UrlTemplateAlbumEdit"] = AddBackUrl(string.Format("{0}?{1}=#AlbumId#&{2}=edit", path, albumVar,actionVar));
			ComponentCache["CommentReadPageUrlTemplate"] = string.Format("{0}?{1}=#PHOTOID#&{2}=#PAGEID#", path, photoVar, pageVar);
			ComponentCache["CommentReadUrlTemplate"] = string.Format("{0}?{1}=#PHOTOID#&{2}=#CommentId#", path, photoVar, commentVar);
			ComponentCache["CommentOperationUrlTemplate"] = string.Format("{0}?{1}=#PHOTOID#&{2}=#CommentId#&{3}=#OPERATION#", path, photoVar, commentVar,actionVar);
			ComponentCache["CommentAddUrlTemplate"] = string.Format("{0}?{1}=#PHOTOID#&{2}=addcomment", path, photoVar, actionVar);
			ComponentCache["UrlTemplatePhoto"] = string.Format("{0}?{1}=#PhotoId#", path, photoVar);
			ComponentCache["UrlTemplatePhotoEdit"] = string.Format("{0}?{1}=#PhotoId#&{2}=edit", path, photoVar,actionVar);
			ComponentCache["UrlTemplateUpload"] = string.Format("{0}?{1}=#AlbumId#&{2}=upload", path, albumVar,actionVar);
			ComponentCache["UrlTemplateAlbumAdd"] = string.Format("{0}?{1}=#AlbumId#&{2}=add", path, albumVar,actionVar);
			return PrepareNormalPage();
		}

		string PrepareNormalPage()
		{
			string page;
			int? albumId = ReadQueryInt(AlbumVariable);
			int? photoId = ReadQueryInt(PhotoVariable);
			int? commentId = ReadQueryInt(CommentVariable);
			string action =	Request.QueryString[ActionVariable];
			int? pageId = ReadQueryInt(PageVariable);
			if (pageId != null)
				ComponentCache["PageId"] = pageId;
			if (photoId != null)
			{
				ComponentCache["PhotoId"] = photoId.Value;
				switch (action)
				{
					case "addcomment":
						ComponentCache["Operation"] = "add";
						page = "photo";
						break;
					case "add":
						ComponentCache["Operation"] = "add";
						page = "photo";
						break;
					case "edit":
						if (commentId != null)
						{
							page = "photo";
							ComponentCache["Operation"] = "edit";
							ComponentCache["CommentId"] = commentId.Value;
						}
						else
						{
							page = "photo.edit";
							
						}
						break;
					case "delete":

						ComponentCache["Operation"] = "delete";
						if (commentId != null)
						{
							ComponentCache["CommentId"] = commentId.Value;
						}
						page = "photo";
						break;
					default:
						page = "photo";
						break;
				}

			}
			else
				if (albumId != null)
				{
					

					if (albumId.Value > 0)
					{
						BXIBlockSection parent = BXIBlockSection.GetById(albumId.Value);

						ComponentCache["AlbumParentID"] = (parent != null) ? parent.SectionId : 0;

					}
					switch (action)
					{
						case "upload":
							page = "upload";
							ComponentCache["AlbumId"] = albumId.Value;
							break;
						case "edit":
							page = "album.edit";
							ComponentCache["AlbumId"] = albumId.Value;
                            ComponentCache["Action"] = "edit";
							break;
						case "add":
							ComponentCache["AlbumParentID"] = albumId.Value;
							page = "album.add";
                            ComponentCache["Action"] = "add";
							break;
						default:
							page = "album";
							ComponentCache["AlbumId"] = albumId.Value;
							break;
					}
				}
				else
					switch (action)
					{
						case "search":
							page = "search";
							break;
						default:
							page = "album";
							break;
					}

			ComponentCache["GalleryRoot"] = BXSefUrlManager.CurrentUrl.AbsolutePath;
            return page;
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessage("Title");
			Description = GetMessage("Description");
			Icon = "images/photo.gif";

			ParamsDefinition.Add(BXParametersDefinition.Paging);
			ParamsDefinition.Add(BXParametersDefinition.Search);
			ParamsDefinition.Add(BXParametersDefinition.Cache);

			//ParamsDefinition.Add(BXParametersDefinition.Sef);
			BXParamsBag<BXParam> sefParBag = BXParametersDefinition.Sef;
			string clientSideActionGroupViewId = ClientID;
			//sefParBag["EnableSEF"].ClientSideAction = new ParamClientSideActionShowSefRelated(true);
			sefParBag["EnableSEF"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId, "EnableSEF", "Sef", "NonSef");
			//sefParBag["SEFFolder"].ClientSideAction = new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef);
			sefParBag["SEFFolder"].ClientSideAction = new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEFFolder", new string[] { "Sef" });
			ParamsDefinition.Add(sefParBag);

			Group = new BXComponentGroup("photo", GetMessage("Group"), 100, BXComponentGroup.Content);

			ParamsDefinition.Add(
				"IBlockTypeId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockType"),
					String.Empty,
					BXCategory.Main
				)
			);
			ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;


			ParamsDefinition.Add(
				"IBlockId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockCode"),
					String.Empty,
					BXCategory.Main
				)
			);
			ParamsDefinition["IBlockId"].RefreshOnDirty = true;



			ParamsDefinition["ColorCss"] =
				new BXParamSingleSelectionWithText(GetMessageRaw("Param.ColorCss"), "~/bitrix/components/bitrix/photogallery/templates/.default/themes/gray/style.css", BXCategory.Main);

			ParamsDefinition.Add(
				"EnableComments",
				new BXParamYesNo(
				GetMessageRaw("EnableComments"),
				false,
				BXCategory.CommentSettings
				)
			);
			ParamsDefinition["EnableComments"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId+"comments", "EnableComments", "Enable", "Disable");

			ParamsDefinition["CommentColorCss"] = 
				new BXParamSingleSelectionWithText(GetMessageRaw("Param.CommentColorCss"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", BXCategory.CommentSettings);

			ParamsDefinition["CommentThemeCss"] =
				new BXParamText(GetMessageRaw("Param.CommentThemeCss"), "~/bitrix/components/bitrix/blog/templates/.default/style.css", BXCategory.CommentSettings);

			ParamsDefinition.Add(
				"CommentForumId",
				new BXParamText(
				GetMessageRaw("Param.CommentForumId"),
				"0",
				BXCategory.CommentSettings,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId + "comments", "CommentForumId", new string[] { "Enable" })
				)
			);

			ParamsDefinition.Add(
				"CommentStoragePropertyName",
				new BXParamText(
				GetMessageRaw("Param.CommentStoragePropertyName"),
				"COMMENTS_STORAGE",
				BXCategory.CommentSettings,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId + "comments", "CommentStoragePropertyName", new string[] { "Enable" })
				)
			);

			ParamsDefinition.Add(
				"CommentShowGuestEmail",
				new BXParamYesNo(
				GetMessageRaw("Param.CommentShowGuestEmail"),
				false,
				BXCategory.CommentSettings,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId + "comments", "CommentShowGuestEmail", new string[] { "Enable" })
				)
			);

			ParamsDefinition.Add(
				"CommentRequireGuestEmail",
				new BXParamYesNo(
				GetMessageRaw("Param.CommentRequireGuestEmail"),
				false,
				BXCategory.CommentSettings,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId + "comments", "CommentRequireGuestEmail", new string[] { "Enable" })
				)
			);

			ParamsDefinition.Add(
				"CommentShowGuestCaptcha",
				new BXParamYesNo(
				GetMessageRaw("Param.CommentShowGuestCaptcha"),
				true,
				BXCategory.CommentSettings,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId + "comments", "CommentShowGuestCaptcha", new string[] { "Enable" })
				)
			);

			ParamsDefinition.Add(
				"UseTreeComments",
				new BXParamYesNo(
				GetMessageRaw("Param.UseTreeComments"),
				false,
				BXCategory.CommentSettings
				)
			);

			ParamsDefinition.Add(
				"CommentUserProfileUrlTemplate",
				new BXParamText(
				GetMessageRaw("Param.CommentUserProfileUrlTemplate"),
				"profile.aspx?user=#UserId#",
				BXCategory.CommentSettings,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId + "comments", "CommentUserProfileUrlTemplate", new string[] { "Enable" })
				)
			);
			ParamsDefinition.Add(BXParametersDefinition.GetCommentPaging(clientSideActionGroupViewId + "comments"));

			ParamsDefinition.Add(
				"EnableVoting",
				new BXParamYesNo(
				GetMessageRaw("Param.EnableVoting"),
				false,
				BXCategory.VotingSettings,
				new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId + "voting","EnableVoting","Enable","Disable")
				)
			);

			ParamsDefinition.Add(
				"VotingRoles",
				new BXParamMultiSelection(
				GetMessageRaw("Param.VotingRoles"),
				String.Empty,
				BXCategory.VotingSettings,
				null,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId + "voting", "VotingRoles", new string[]{"Enable"}))
			);

			ParamsDefinition.Add(
			"PositiveVoteValue",
			new BXParamText(
				GetMessageRaw("Param.PositiveVoteValue"),
				"1",
				BXCategory.VotingSettings,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId + "voting", "PositiveVoteValue", new string[]{"Enable"})
				)
			);

			ParamsDefinition.Add(
				"NegativeVoteValue",
				new BXParamText(
					GetMessageRaw("Param.NegativeVoteValue"),
					"-1",
					BXCategory.VotingSettings,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId + "voting", "NegativeVoteValue", new string[]{"Enable"})
					)
				);


			//Управление адресами страниц
			ParamsDefinition.Add(
				"SEF_Album",
				new BXParamText(
					GetMessageRaw("BrowseAlbum.FriendlyUrl"),
					"/album/#ALBUMID#/",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEF_Album", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"SEF_AlbumEdit",
				new BXParamText(
					GetMessageRaw("ModifyAlbum.FriendlyUrl"),
					"/album/#ALBUMID#/edit",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEF_AlbumEdit", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"SEF_AlbumAdd",
				new BXParamText(
					GetMessageRaw("AddAlbum.FriendlyUrl"),
					"/album/#PARENTID#/add",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEF_AlbumAdd", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"SEF_Upload",
				new BXParamText(
					GetMessageRaw("UploadPhotos.FriendlyUrl"),
					"/album/#ALBUMID#/upload",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEF_Upload", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
			   "SEF_AlbumPage",
			   new BXParamText(
				   GetMessageRaw("AlbumPage.FriendlyUrl"),
				   "/album/#ALBUMID#/page-#PAGEID#",
				   BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
				   new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEF_AlbumPage", new string[] { "Sef" })
			   )
		   );

			ParamsDefinition.Add(
			   "SEF_AlbumShowAll",
			   new BXParamText(
				   GetMessageRaw("AlbumShowAll.FriendlyUrl"),
				   "/album/#ALBUMID#/all",
				   BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
				   new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEF_AlbumShowAll", new string[] { "Sef" })
			   )
		   );

			ParamsDefinition.Add(
			   "SEF_PhotoEdit",
			   new BXParamText(
				   GetMessageRaw("ModifyPhoto.FriendlyUrl"),
				   "/album/#ALBUMID#/#PHOTOID#/edit/",
				   BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
				   new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEF_PhotoEdit", new string[] { "Sef" })
			   )
		   );

			ParamsDefinition.Add(
			   "SEF_Photo",
			   new BXParamText(
				   GetMessageRaw("BrowsePhoto.FriendlyUrl"),
				   "/album/#ALBUMID#/#PHOTOID#",
				   BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.Sef)
				   new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "SEF_Photo", new string[] { "Sef" })
			   )
		   );

			ParamsDefinition.Add(
				"CommentReadUrlTemplate",
					new BXParamText(
					GetMessageRaw("Param.CommentReadUrlTemplate"),
					"/album/#ALBUMID#/#PHOTOID#/comments/#COMMENTID#/",
					BXCategory.Sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentReadUrlTemplate", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"CommentPostOperationUrlTemplate",
				new BXParamText(
				GetMessageRaw("Param.CommentPostOperationUrlTemplate"),
				"/album/#ALBUMID#/#PHOTOID#/comments/#COMMENTID#/#OPERATION#/",
				BXCategory.Sef,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentPostOperationUrlTemplate", new string[] { "Sef" })
				)
			);

			ParamsDefinition.Add(
				"CommentPostAddUrlTemplate",
				new BXParamText(
				GetMessageRaw("Param.CommentPostAddUrlTemplate"),
				"/album/#ALBUMID#/#PHOTOID#/comments/add",
				BXCategory.Sef,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "CommentPostAddUrlTemplate", new string[] { "Sef" })
				)
			);


			ParamsDefinition.Add(
				"ParamAlbum",
				new BXParamText(
					GetMessageRaw("QueryParam.Album"),
					"album",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamAlbum", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ParamAlbumPage",
				new BXParamText(
					GetMessageRaw("QueryParam.AlbumPage"),
					"page",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamAlbumPage", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ParamAlbumShowAll",
				new BXParamText(
					GetMessageRaw("QueryParam.ShowAll"),
					"showall",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamAlbumShowAll", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ParamAlbumEdit",
				new BXParamText(
					GetMessageRaw("QueryParam.ModifyAlbum"),
					"AlbumEdit",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamAlbumEdit", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
					"ParamAlbumAdd",
					new BXParamText(
					GetMessageRaw("QueryParam.AddAlbum"),
					"AlbumAdd",
					BXCategory.Sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamAlbumAdd", new string[] { "NonSef" })
					)
			);

			ParamsDefinition.Add(
				"ParamPhoto",
				new BXParamText(
					GetMessageRaw("QueryParam.Photo"),
					"photo",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamPhoto", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ParamPhotoEdit",
				new BXParamText(
					GetMessageRaw("QueryParam.ModifyPhoto"),
					"PhotoEdit",
					BXCategory.Sef,
				//new ParamClientSideActionShowSefRelated(ParamClientSideActionShowSefRelatedParticipantType.NonSef)
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamPhotoEdit", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ParamSearch",
				new BXParamText(
					GetMessageRaw("QueryParam.Search"),
					"SEARCH",
					BXCategory.Sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamSearch", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ParamAlbumUpload",
				new BXParamText(
					GetMessageRaw("QueryParam.UploadPhotos"),
					"Upload",
					BXCategory.Sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamAlbumUpload", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ParamComment",
				new BXParamText(
					GetMessageRaw("QueryParam.Comment"),
					"comment",
					BXCategory.Sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamComment", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ParamCommentOperation",
				new BXParamText(
					GetMessageRaw("QueryParam.CommentOperation"),
					"act",
					BXCategory.Sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamCommentOperation", new string[] { "NonSef" })
				)
			);

			ParamsDefinition.Add(
				"ParamCommentPage",
				new BXParamText(
					GetMessageRaw("QueryParam.CommentPage"),
					"page",
					BXCategory.Sef,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, "ParamCommentPage", new string[] { "NonSef" })
				)
			);

			//Настройки фотогалереи
			ParamsDefinition.Add(
				"CoverWidth",
				new BXParamText(
					GetMessageRaw("AlbumThumbnail.WidthInPixels"),
					"120",
					BXCategory.PhotogallerySettings
				)
			);

			ParamsDefinition.Add(
				"CoverHeight",
				new BXParamText(
					GetMessageRaw("AlbumThumbnail.HeightInPixels"),
					"120",
					BXCategory.PhotogallerySettings
				)
			);

			ParamsDefinition.Add(
				"PreviewWidth",
				new BXParamText(
					GetMessageRaw("PhotoThumbnail.WidthInPixels"),
					"210",
					BXCategory.PhotogallerySettings
				)
			);

			ParamsDefinition.Add(
				"PreviewHeight",
				new BXParamText(
					GetMessageRaw("PhotoThumbnail.HeightInPixels"),
					"210",
					BXCategory.PhotogallerySettings
				)
			);

			ParamsDefinition.Add(
				"PhotoWidth",
				new BXParamText(
					GetMessageRaw("Photo.WidthInPixels"),
					"300",
					BXCategory.PhotogallerySettings
				)
			);

			ParamsDefinition.Add(
				"PhotoHeight",
				new BXParamText(
					GetMessageRaw("Photo.HeightInPixels"),
					"300",
					BXCategory.PhotogallerySettings
				)
			);

			ParamsDefinition.Add(
				"FlickrMode",
				new BXParamYesNo(
					GetMessageRaw("FlickrModeOrEnablePhotoCutting"),
					false,
					BXCategory.PhotogallerySettings
				)
			);
			ParamsDefinition.Add(
			"AlbumSortBy",
				new BXParamSingleSelection(GetMessageRaw("Param.AlbumSortBy"), "", BXCategory.ListSettings));

			ParamsDefinition.Add(
				"AlbumSortOrder",
				new BXParamSort(GetMessageRaw("Param.AlbumSortOrder"), BXCategory.ListSettings));

			ParamsDefinition.Add(
				"PhotoSortBy",
				new BXParamSingleSelection(GetMessageRaw("Param.PhotoSortBy"), "", BXCategory.ListSettings));

			ParamsDefinition.Add(
				"PhotoSortOrder",
				new BXParamSort(GetMessageRaw("Param.PhotoSortOrder"), BXCategory.ListSettings));
		}

		protected override void LoadComponentDefinition()
		{
			//ТИП ИНФОБЛОКА
			List<BXParamValue> types = new List<BXParamValue>();
			types.Add(new BXParamValue("-", ""));
			BXIBlockTypeCollection typeCollection = BXIBlockType.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Sort, BXOrderByDirection.Asc)));
			foreach (BXIBlockType t in typeCollection)
				types.Add(new BXParamValue(t.Translations[BXLoc.CurrentLocale].Name, t.Id.ToString()));
			//types.Add(new BXParamValue(t.TypeLang[BXLoc.CurrentLocale].Name, t.Id.ToString()));

			ParamsDefinition["IBlockTypeId"].Values = types;

			BXFilter iblockFilter = new BXFilter();
			if (Parameters.ContainsKey("IBlockTypeId"))
			{
				int typeId = Parameters.Get("IBlockTypeId", 0);
				if (typeId > 0)
					iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, typeId));
			}
			if (!string.IsNullOrEmpty(DesignerSite))
				iblockFilter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

			List<BXParamValue> iblocks = new List<BXParamValue>();
			BXIBlockCollection iblockCollection = BXIBlock.GetList(iblockFilter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Sort, BXOrderByDirection.Asc)));
			foreach (BXIBlock b in iblockCollection)
				iblocks.Add(new BXParamValue(string.Format("[{0}] {1}", b.Id, b.Name), b.Id.ToString()));

			ParamsDefinition["IBlockId"].Values = iblocks;

			foreach (string parDefKey in ParamsDefinition.Keys)
				ParamsDefinition[parDefKey].AdjustPresentation(this);

			DirectoryInfo info = new DirectoryInfo(BXPath.MapPath("~/bitrix/components/bitrix/photogallery/templates/.default/themes/"));
			if (info.Exists)
			{
				List<BXParamValue> values = new List<BXParamValue>();
				foreach (DirectoryInfo sub in info.GetDirectories())
				{
					if (!File.Exists(Path.Combine(sub.FullName, "style.css")))
						continue;

					string themeTitle = BXLoc.GetMessage("~/bitrix/components/bitrix/photogallery/templates/.default/themes/" + sub.Name + "/description", "Title");
					if (String.IsNullOrEmpty(themeTitle))
						themeTitle = sub.Name;

					values.Add(new BXParamValue(themeTitle, VirtualPathUtility.Combine("~/bitrix/components/bitrix/photogallery/templates/.default/themes/", sub.Name + "/style.css")));
				}
				ParamsDefinition["ColorCss"].Values = values;
			}

			info = new DirectoryInfo(BXPath.MapPath("~/bitrix/components/bitrix/blog/templates/.default/themes/"));
			if (info.Exists)
			{
				List<BXParamValue> values = new List<BXParamValue>();
				foreach (DirectoryInfo sub in info.GetDirectories())
				{
					if (!File.Exists(Path.Combine(sub.FullName, "style.css")))
						continue;

					string themeTitle = BXLoc.GetMessage("~/bitrix/components/bitrix/blog/templates/.default/themes/" + sub.Name + "/description", "Title");
					if (String.IsNullOrEmpty(themeTitle))
						themeTitle = sub.Name;

					values.Add(new BXParamValue(themeTitle, VirtualPathUtility.Combine("~/bitrix/components/bitrix/blog/templates/.default/themes/", sub.Name + "/style.css")));
				}
				ParamsDefinition["CommentColorCss"].Values = values;
			}


			IList<BXParamValue> rolesValues = ParamsDefinition["VotingRoles"].Values;
			rolesValues.Clear();

			foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
			{
				if (string.Equals(r.RoleName, "Guest", StringComparison.Ordinal))
					continue;
				rolesValues.Add(new BXParamValue(r.Title, r.RoleName));
			}

			var sortFields = new List<BXParamValue>();
			sortFields.Add(new BXParamValue("ID", "ID"));
			sortFields.Add(new BXParamValue(GetMessageRaw("ParamValue.Sort"), "Sort"));
			sortFields.Add(new BXParamValue(GetMessageRaw("ParamValue.DateCreate"), "Sort"));

			ParamsDefinition["AlbumSortBy"].Values = sortFields;
			ParamsDefinition["PhotoSortBy"].Values = sortFields;
		}

		

		public override void ProcessMessage(BXCommand cmd, bool executionContext, string executionVirtualPath)
		{
			if (cmd.Action == "Bitrix.Search.ProvideUrl")
			{
				if (cmd.Parameters.Get<string>("moduleId") != "iblock")
					return;
				int iblockId = Parameters.Get<int>("IBlockId", -1);
				if (iblockId < 0 || cmd.Parameters.Get<int>("itemGroup") != iblockId)
					return;
				int elementId = cmd.Parameters.Get<int>("itemId", -1);
				if (elementId < 0)
					return;

				BXParamsBag<object> p = new BXParamsBag<object>();
				p.Add("PhotoId", elementId);

				string url;
				if (Parameters.Get<bool>("EnableSEF"))
					url = MakeLink(Parameters.Get<string>("SEFFolder"), Parameters.Get<string>("SEF_Photo", "/photo/#PHOTOID#"), p);
				else
					url = MakeLink(string.Format("{0}?{1}=#PhotoId#", BXSite.GetUrlForPath(executionVirtualPath, null, cmd.SiteId), Parameters.Get<string>("ParamPhoto", "photo")), p);

				cmd.AddCommandResult("bitrix:photogallery@" + executionVirtualPath, new BXCommandResult(BXCommandResultType.Ok, url));
			}
		}
	}

	public class PhotoGalleryTemplate : BXComponentTemplate<PhotoGalleryComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
            BXPage.Scripts.RequireCore(Page);
            BXPage.RegisterScriptInclude(AppRelativeTemplateSourceDirectory + "js/classes.js");
            BXPage.RegisterScriptInclude(AppRelativeTemplateSourceDirectory + "js/helpers.js");
			
			var cssParams = new string[] { Parameters.GetString("ColorCss", "") };
			foreach (string css in cssParams)
			{
				if (!BXStringUtility.IsNullOrTrimEmpty(css))
				{
					try
					{
						string vCss = BXPath.ToVirtualRelativePath(css);
						if (BXSecureIO.FileExists(vCss))
							BXPage.RegisterStyle(vCss);
					}
					catch
					{
					}
				}
			}
			base.OnPreRender(e);
		}
	}
}

#region compatibility

public partial class PhotoGallery : Bitrix.IBlock.Components.PhotoGalleryComponent
{
}

#endregion