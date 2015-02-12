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
using System.Text;
using Bitrix.Services;

public partial class bitrix_components_bitrix_photogallery_photo_templates__default_template : BXComponentTemplate<PhotogalleryPhoto>
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (Component.Photo != null)
		{
			Results["PhotoId"] = Component.Photo.Id;
			lButtonDelete.Visible = true;
			lButtonSetCover.Visible = true;
			lButtonDelete.OnClientClick = string.Format("return confirm(\"{0}\");", JSEncode(GetMessageRaw("DoYouReallyWantRemoveThisPhoto")));
			var coverIsSet = Request.QueryString["setcover"];
			if (!String.IsNullOrEmpty(coverIsSet))
			{
				if (coverIsSet.Equals("y", StringComparison.OrdinalIgnoreCase))
				{
					dMessage.InnerText = GetMessage("CoverSettingSuccessful");
					dMessage.Visible = true;
				}
				else
					if (coverIsSet.Equals("n", StringComparison.OrdinalIgnoreCase))
					{
						dMessage.Attributes["class"] = "errortext";
						dMessage.InnerText = GetMessage("ErrorSettingCover");
						dMessage.Visible = true;
					}
			}
		}
		else
			Results["PhotoId"] = 0;

	}

	protected void lbDelete_Click(object sender, EventArgs e)
	{
		Component.DeletePhoto();
	}

	protected void lbSetCover_Click(object sender, EventArgs e)
	{
		var pms = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
		pms.Remove("setcover");
		pms.Add("setcover", Component.SetCover() ? "y" : "n");

		var ub = new UriBuilder();
		ub.Host = BXSefUrlManager.CurrentUrl.Host;
		ub.Path = BXSefUrlManager.CurrentUrl.AbsolutePath;
		ub.Query = pms.ToString();

		Response.Redirect(ub.Uri.ToString());
	}

	protected override void OnInit(EventArgs e)
	{
		if (Component.IsForumInstalled && Component.EnableComments)
		{
			var c = new IncludeComponent("bitrix:forum.comment.block", ".default");
			c.Attributes["ForumId"] = Component.Parameters.GetString("CommentForumId", "0");
			c.Attributes["ForumTopicId"] = Component.ComponentCache.GetString("ForumTopicId", "0");
			c.Attributes["PostId"] = Component.Parameters.GetString("CommentId", "0");
			c.Attributes["PostOperation"] = Component.Parameters.GetString("CommentOperation", "");

			c.Attributes["IdentityPropertyName"] = Component.ComponentCache.GetString("IdentityPropertyName", "");
			c.Attributes["IdentityPropertyTypeName"] = Component.ComponentCache.GetString("IdentityPropertyTypeName", "");
			c.Attributes["IdentityPropertyValue"] = Component.ComponentCache.GetString("IdentityPropertyValue", "");
			c.Attributes["ShowGuestEmail"] = Component.Parameters.GetString("CommentShowGuestEmail", "");
			c.Attributes["RequireGuestEmail"] = Component.Parameters.GetString("CommentRequireGuestEmail", "");

			c.Attributes["ShowGuestCaptcha"] = Component.Parameters.GetString("CommentShowGuestCaptcha", "");
			c.Attributes["UserProfileUrlTemplate"] = Component.ComponentCache.GetString("UserProfileUrlTemplate", "");
			c.Attributes["PostOperationUrlTemplate"] = Component.ComponentCache.GetString("PostOperationUrlTemplate", "");
			c.Attributes["RedirectUrl"] = Component.ComponentCache.GetString("UrlPhoto", "");
			c.Attributes["PostReadUrlTemplate"] = Component.ComponentCache.GetString("PostReadUrlTemplate", "");


			c.Attributes["PagingAllow"] = Component.Parameters.GetString("CommentPagingAllow", "");
			c.Attributes["PagingMode"] = Component.Parameters.GetString("CommentPagingMode", "");
			c.Attributes["PagingTemplate"] = Component.Parameters.GetString("CommentPagingTemplate", "");
			c.Attributes["PagingShowOne"] = Component.Parameters.GetString("CommentPagingShowOne", "");
			c.Attributes["PagingRecordsPerPage"] = Component.Parameters.GetString("CommentPagingRecordsPerPage", "");
			c.Attributes["PagingTitle"] = Component.Parameters.GetString("CommentPagingTitle", "");
			c.Attributes["PagingPosition"] = Component.Parameters.GetString("CommentPagingPosition", "");
			c.Attributes["PagingMaxPages"] = Component.Parameters.GetString("CommentPagingMaxPages", "");

			c.Attributes["PagingPageID"] = Component.Parameters.GetString("PageId");
			c.Attributes["PagingIndexTemplate"] = Component.ComponentCache.GetString("UrlPhoto", "");
			c.Attributes["PagingPageTemplate"] = Component.ComponentCache.GetString("PostReadPageUrlTemplate", "");
			c.Attributes["ShowPostForm"] = Component.Parameters.GetString("CommentOperation", "") == "add" ? "true" : "false";
			Comments.Controls.Add(c);
		}

		if (Component.IsComUtilInstalled && Component.EnableVoting && Component.Photo != null)
		{
			var voting = new IncludeComponent("bitrix:rating.vote", ".default");
			voting.Attributes["BoundEntityTypeId"] = "IBlockElement";
			voting.Attributes["BoundEntityId"] = Component.ComponentCache.GetString("PhotoId", "0");
			voting.Attributes["CustomPropertyEntityId"] = "iblock_" + Component.Photo.IBlockId;

			voting.Attributes["RolesAuthorizedToVote"] = Component.Parameters.GetString("VotingRoles", "");
			voting.Attributes["NegativeVoteValue"] = Component.Parameters.GetString("NegativeVoteValue", "1");
			voting.Attributes["PositiveVoteValue"] = Component.Parameters.GetString("PositiveVoteValue", "-1");
			Voting.Controls.Add(voting);

		}

		base.OnInit(e);
	}
}





