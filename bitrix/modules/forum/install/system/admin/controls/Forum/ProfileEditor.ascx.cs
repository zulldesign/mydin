using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix.UI;
using Bitrix.Forum;
using Bitrix.Security;
using Bitrix.Services.User;
using Bitrix.Services.Text;
using Bitrix.DataLayer;

public partial class bitrix_admin_controls_Forum_ProfileEditor : BXUserProfileAdminEditor
{
	BXForumUser forumUser = null;
	
	public override void Load(BXUser user)
	{
		if (user != null && !user.IsNew)
			forumUser = BXForumUser.GetById(user.UserId, BXTextEncoder.EmptyTextEncoder);

		if (forumUser == null)
			return;

		Posts.Text = forumUser.Posts.ToString("#,0");
		Signature.Text = forumUser.Signature;
		OwnPostNotification.Checked = forumUser.OwnPostNotification;
	}

	public override void Save(BXUser user, BXSqlTransaction tran)
	{
		forumUser = forumUser ?? BXForumUser.GetById(user.UserId, BXTextEncoder.EmptyTextEncoder);
		if (forumUser == null)
		{
			forumUser = new BXForumUser(BXTextEncoder.EmptyTextEncoder);
			forumUser.Id = user.UserId;
		}

		forumUser.Signature = Signature.Text;
		forumUser.OwnPostNotification = OwnPostNotification.Checked;
		forumUser.Save(tran != null ? tran.Connection : null, tran);
	}
}
