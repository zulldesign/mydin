using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Services.Js;
using Bitrix.Security;

namespace Bitrix.Blog.UI
{
	public partial class EditBlogUsers : BXControl
	{
		List<string> errors;
		List<UserBinding> bindings;
		List<BXBlogUserGroup> groups;
		string userProfileTemplate;		

		public int BlogId { get; set; }

		public string UserProfileTemplate 
		{ 
			get { return userProfileTemplate; }
			set 
			{
				userProfileTemplate = value;
			}
		}		

		protected List<UserBinding> Bindings
		{
			get { return bindings ?? (IsPostBack ? LoadPost() : LoadDB()); }
		}

		protected List<BXBlogUserGroup> Groups
		{
			get { return groups ?? LoadGroups(); }
		}

		public string MakeUserProfileTemplate()
		{
			if (string.IsNullOrEmpty(UserProfileTemplate))
				return UserProfileTemplate;

			if (UserProfileTemplate.StartsWith("~/"))
				return VirtualPathUtility.ToAbsolute("~/") + (UserProfileTemplate.Length > 2 ? UserProfileTemplate.Substring(2) : "");
			return UserProfileTemplate;
		}

		private List<BXBlogUserGroup> LoadGroups()
		{
			return groups = BXBlogUserGroup.GetList(
				new BXFilter(
					new BXFilterItem(BXBlogUserGroup.Fields.BlogId, BXSqlFilterOperators.Equal, BlogId)
				),
				new BXOrderBy(
					new BXOrderByPair(BXBlogUserGroup.Fields.Id, BXOrderByDirection.Asc)
				),
				new BXSelect(
					BXBlogUserGroup.Fields.Id,
					BXBlogUserGroup.Fields.Name,					
					BXBlogUserGroup.Fields.Type,
					BXBlogUserGroup.Fields.Users,
					BXBlogUserGroup.Fields.Users.User.User.UserId,
					BXBlogUserGroup.Fields.Users.User.User.UserName,
					BXBlogUserGroup.Fields.Users.User.User.FirstName,
					BXBlogUserGroup.Fields.Users.User.User.LastName,
					BXBlogUserGroup.Fields.Users.User.User.DisplayName,
					BXBlogUserGroup.Fields.Users.User.User.Image
				),
				null,
				BXTextEncoder.EmptyTextEncoder
			);
		}

		private List<UserBinding> LoadDB()
		{
			bindings = new List<UserBinding>();
					
			foreach (var g in Groups)
			{
				foreach (var u in g.Users)
				{
					if (u.User == null || u.User.User == null)
						continue;
					var user = u.User.User;
					bindings.Add(new UserBinding
					{
						UserId = user.UserId,
						Title = BXUserListHandler.FormatUserName(user),
						AvatarPath = user.Image != null ? user.Image.FilePath : "",
						GroupId = g.Id,
						IsAuto = u.IsAuto,
						HasAuto = u.IsAuto
					});
				}
			}

			return bindings;
		}

		private List<UserBinding> LoadPost()
		{
			var groups = Request.Form.GetValues(UniqueID + "$group");
			var ids = Request.Form.GetValues(UniqueID + "$id");
			var names = Request.Form.GetValues(UniqueID + "$name");
			var images = Request.Form.GetValues(UniqueID + "$image");
			var autos = Request.Form.GetValues(UniqueID + "$auto");
			var hasAutos = Request.Form.GetValues(UniqueID + "$hasAuto");

			bindings = new List<UserBinding>();

			if (groups == null)
				return bindings;
			
			for (int i = 0; i < groups.Length; i++)
			{
				int groupId;
				if (!int.TryParse(groups[i], out groupId))
					continue;

				int userId;
				if (!int.TryParse(ids[i], out userId))
					continue;

				bindings.Add(new UserBinding
				{
					GroupId = groupId,
					UserId = userId,	
					AvatarPath = images[i],
					Title = names[i],
					HasAuto = hasAutos[i] == "true",
					IsAuto = autos[i] == "true"
				});
			}

			return bindings;
		}

		public bool Validate(BXSqlTransaction tran)
		{
			return true;	
			
			//return errors != null && errors.Count > 0;
		}

		public List<string> Errors
		{
			get { return errors ?? (errors = new List<string>()); }
		}

		public void Save(BXSqlTransaction tran)
		{
			foreach (var g in Groups)
			{
				bool changed = false;
				for (int i = g.Users.Count - 1; i >= 0; i--)
				{
					var u = g.Users[i];

					bool leave = false;
					foreach (var b in Bindings)
					{
						if (b.UserId != u.BlogUserId || b.GroupId != u.BlogUserGroupId)
							continue;

						leave = true;
						b.Processed = true;

						bool newAuto = u.IsAuto && b.HasAuto && b.IsAuto;
						if (u.IsAuto != newAuto)
						{
							u.IsAuto = newAuto;
							changed = true;
						}
					}

					if (!leave)
					{
						g.Users.RemoveAt(i);
						changed = true;
					}
				}

				foreach (var b in Bindings)
				{
					if (b.GroupId != g.Id || b.Processed || b.UserId <= 0)
						continue;

					b.Processed = true;

					var user = BXBlogUser.GetList(
						new BXFilter(new BXFilterItem(BXBlogUser.Fields.Id, BXSqlFilterOperators.Equal, b.UserId)),
						null,
						new BXSelect(
							BXBlogUser.Fields.Id,
							BXBlogUser.Fields.User.UserId
						),
						new BXQueryParams(new BXPagingOptions(0, 1)) { Transaction = tran }
					)
					.FirstOrDefault();

					if (user == null)
					{
						var u = BXUser.GetList(
							new BXFilter(new BXFilterItem(BXUser.Fields.UserId, BXSqlFilterOperators.Equal, b.UserId)),
							null,
							new BXSelect(BXUser.Fields.UserId),
							new BXQueryParams(new BXPagingOptions(0, 1)) { Transaction = tran }
						)
						.FirstOrDefault();
						
						if (u == null)
							continue;

						var bu = new BXBlogUser();
						bu.Id = u.UserId;
						bu.Save(tran != null ? tran.Connection : null, tran);
					}
					else if (user.User == null)
						continue;

					changed = true;
					g.Users.Add(new BXBlogUserGroup2User(b.UserId));
				}

				if (changed)
					g.Save(tran != null ? tran.Connection : null, tran);
			}			
		}

		private int CheckEnum<T>(string value, T defaultValue) where T : IConvertible
		{
			var ic = System.Globalization.CultureInfo.InvariantCulture;
			int num = int.Parse(value);
			foreach (T val in Enum.GetValues(typeof(T)))
				if (num == val.ToInt32(ic))
					return num;
			return defaultValue.ToInt32(ic);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			BlogUsers.DefaultText = GetMessageRaw("AddUserTextBox.Text");
			BlogUsers.CustomShowDataScript = ClientID + "_Add";
			BlogUsers.InitialShowData = BXJSUtility.BuildJSArray(Bindings.ConvertAll(x => new Dictionary<string, object>
			{
				{ "id", x.UserId.ToString() },
				{ "text", x.Title },
				{ "userData", new Dictionary<string, object> {
						{ "image", x.AvatarPath },
						{ "group", x.GroupId.ToString() },
						{ "isAuto", x.IsAuto },
						{ "hasAuto", x.HasAuto }
				}}
			}));	
			BlogUsers.AttachScript();
		}

		public override bool Visible
		{
			get
			{
				return Groups.Count > 0 && base.Visible;
			}
			set
			{
				base.Visible = value;
			}
		}

		protected class UserBinding
		{
			public string Title { get; set; }
			public string AvatarPath { get; set; }
			public int UserId { get; set; }
			public int GroupId { get; set; }
			public bool IsAuto { get; set; }
			public bool HasAuto { get; set; }

			internal bool Processed;			
		}
	}
}