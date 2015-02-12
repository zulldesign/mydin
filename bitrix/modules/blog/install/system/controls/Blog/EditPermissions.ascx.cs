using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.DataLayer;
using Bitrix.Services.Text;

namespace Bitrix.Blog.UI
{
	public partial class EditPermissions : BXControl	
	{
		List<string> errors;
		List<PermissionLevel> permissions;

		public int BlogId { get; set; }

		protected List<PermissionLevel> Permissions 
		{ 
			get { return permissions ?? (IsPostBack ? LoadPost() : LoadDB()); } 
		}

		private List<PermissionLevel> LoadDB()
		{
			permissions = new List<PermissionLevel>();

			var groups = BXBlogUserGroup.GetList(
				new BXFilter(
					new BXFilterItem(BXBlogUserGroup.Fields.BlogId, BXSqlFilterOperators.In, new[] { BlogId, 0 })										
				),
				new BXOrderBy(
					new BXOrderByPair(BXBlogUserGroup.Fields.Id, BXOrderByDirection.Asc)
				),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder
			);

			var userPermissions = BXBlogUserPermission.GetList(
				new BXFilter(
					new BXFilterItem(BXBlogUserPermission.Fields.BlogId, BXSqlFilterOperators.Equal, BlogId),
					new BXFilterItem(BXBlogUserPermission.Fields.BlogUserGroupId, BXSqlFilterOperators.In, groups.ConvertAll(x => x.Id))
				),
				new BXOrderBy(
					new BXOrderByPair(BXBlogUserPermission.Fields.Id, BXOrderByDirection.Asc)
				),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder
			);

			permissions = groups.ConvertAll(g =>
			{
				var permPosts = userPermissions.Find(x => x.BlogUserGroupId == g.Id && x.Type == BXBlogUserPermissionType.Posts);
				var permComments = userPermissions.Find(x => x.BlogUserGroupId == g.Id && x.Type == BXBlogUserPermissionType.Comments);
				return new PermissionLevel
				{
					PostLevel = permPosts != null ? permPosts.PermissionLevel : (int)BXBlogUserPermission.DefaultPostPermission,
					CommentLevel = permComments != null ? permComments.PermissionLevel : (int)BXBlogUserPermission.DefaultCommentPermission,
					Title = g.GetDisplayName(),
					Group = g
				};
			});

			return permissions;
		}

		private List<PermissionLevel> LoadPost()
		{
			var ids = Request.Form.GetValues(UniqueID + "$id");
			var names = Request.Form.GetValues(UniqueID + "$name");
			var postLevels = Request.Form.GetValues(UniqueID + "$postlevel");
			var commentLevels = Request.Form.GetValues(UniqueID + "$commentlevel");


			permissions = BXBlogUserGroup.GetList(
				new BXFilter(
					new BXFilterItem(BXBlogUserGroup.Fields.BlogId, BXSqlFilterOperators.In, new[] { BlogId, 0 })										
				),
				new BXOrderBy(
					new BXOrderByPair(BXBlogUserGroup.Fields.Id, BXOrderByDirection.Asc)
				),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder
			)
			.ConvertAll(g =>
			{				
				return new PermissionLevel
				{
					PostLevel = (int)BXBlogUserPermission.DefaultPostPermission,
					CommentLevel = (int)BXBlogUserPermission.DefaultCommentPermission,
					Group = g
				};
			});


			if (ids != null)
			{
				for (int i = 0; i < ids.Length; i++)
				{
					int id = int.Parse(ids[i]);
					var level = id <= 0 ? new PermissionLevel() : permissions.Find(x => x.Group.Id == id);
					if (level == null)
						continue;

					if (level.Group == null || level.Group.Type == BXBlogUserGroupType.UserDefined)
					{
						level.Title = (names[i] ?? "").Trim();
						if (level.Title.Length == 0)
						{
							level.HasError = true;
							Errors.Add(GetMessageRaw("Error.UserGroupNameRequired"));
						}
					}
					else
					{
						level.Title = level.Group.GetDisplayName();
					}
					level.PostLevel = CheckEnum(postLevels[i], BXBlogUserPermission.DefaultPostPermission);
					level.CommentLevel = CheckEnum(commentLevels[i], BXBlogUserPermission.DefaultCommentPermission);
					level.IsDirty = true;

					if (level.Group == null)
						permissions.Add(level);
				}
			}

			foreach (var p in permissions)
			{
				if (!p.IsDirty && p.Group.BlogId != 0 && p.Group.Type == BXBlogUserGroupType.UserDefined)
					p.IsDeleted = true;
			}
			
			return permissions;
		}

		public bool Validate(BXSqlTransaction tran)
		{
			foreach (var p in Permissions)
			{
				if (p.HasError)
					return false;
			}
			return true;
		}

		public List<string> Errors
		{
			get { return errors ?? (errors = new List<string>()); }
		}

		public void Save(BXSqlTransaction tran)
		{
			var userPermissions = BXBlogUserPermission.GetList(
				new BXFilter(
					new BXFilterItem(BXBlogUserPermission.Fields.BlogId, BXSqlFilterOperators.Equal, BlogId),
					new BXFilterItem(BXBlogUserPermission.Fields.BlogUserGroupId, BXSqlFilterOperators.In, Permissions.FindAll(x => x.Group != null).ConvertAll(x => x.Group.Id))
				),
				new BXOrderBy(
					new BXOrderByPair(BXBlogUserPermission.Fields.Id, BXOrderByDirection.Asc)
				),
				null,
				new BXQueryParams { Transaction = tran },
				BXTextEncoder.EmptyTextEncoder
			);
						
			foreach (var p in permissions)
			{
				if (p.IsDeleted)
				{
					p.Group.Delete(tran != null ? tran.Connection : null, tran);
					continue;
				}

				if (p.IsDirty)
				{
					if (p.Group == null)
					{
						p.Group = new BXBlogUserGroup
						{
							BlogId = BlogId,
							Name = p.Title,
							Type = BXBlogUserGroupType.UserDefined
						};
						p.Group.Save(tran != null ? tran.Connection : null, tran);
					}
					else if (p.Group.Name != p.Title)
					{
						p.Group.Name = p.Title;
						p.Group.Save();
					}

					var up = userPermissions.Find(x => x.BlogUserGroupId == p.Group.Id && x.Type == BXBlogUserPermissionType.Posts) ?? new BXBlogUserPermission { BlogId = BlogId, BlogUserGroupId = p.Group.Id, Type = BXBlogUserPermissionType.Posts };
					if (up.PermissionLevel != p.PostLevel)
					{
						up.PermissionLevel = p.PostLevel;
						up.Save();
					}
					
					up = userPermissions.Find(x => x.BlogUserGroupId == p.Group.Id && x.Type == BXBlogUserPermissionType.Comments) ?? new BXBlogUserPermission { BlogId = BlogId, BlogUserGroupId = p.Group.Id, Type = BXBlogUserPermissionType.Comments };
					if (up.PermissionLevel != p.CommentLevel)
					{
						up.PermissionLevel = p.CommentLevel;
						up.Save();
					}
				}
			}
		}

		private int CheckEnum<T>(string value, T defaultValue) where T: IConvertible
		{
			var ic = System.Globalization.CultureInfo.InvariantCulture;
			int num = int.Parse(value);
			foreach (T val in Enum.GetValues(typeof(T)))
				if (num == val.ToInt32(ic))
					return num;
			return defaultValue.ToInt32(ic);			
		}
		

		protected class PermissionLevel
		{			
			public string Title { get; set; }
			public BXBlogUserGroup Group { get; set; }

			public int PostLevel { get; set; }
			public int CommentLevel { get; set; }

			internal bool IsDirty;
			internal bool IsDeleted;
			internal bool HasError;
		}
	}
}