using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Bitrix.UI;
using Bitrix.Security;
using Bitrix.Services.User;
using Bitrix.Services.Text;
using Bitrix.DataLayer;
using System.Collections.Generic;
using Bitrix.Services;

namespace Bitrix.Blog.UI
{
	public partial class ProfileEditor : BXUserProfileAdminEditor
	{
		protected List<BindingInfo> bindings = new List<BindingInfo>();
		protected BXBlogUser blogUser;
		private List<BlogInfo> blogs;

		protected List<BlogInfo> Blogs { get { return blogs ?? LoadBlogs(); } }

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);			
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			BXPage.Scripts.RequireCore(Page, "ajax");
		}

		public override void Load(BXUser user)
		{			
			if (user == null || user.IsNew)
				return;

		    blogUser = BXBlogUser.GetList(
		        new BXFilter(new BXFilterItem(BXBlogUser.Fields.Id, BXSqlFilterOperators.Equal, user.UserId)),
		        null,
				new BXSelectAdd(
					BXBlogUser.Fields.UserGroups,
					BXBlogUser.Fields.UserGroups.UserGroup,
					BXBlogUser.Fields.UserGroups.UserGroup.Blog.Name,					
					BXBlogUser.Fields.UserGroups.UserGroup.Blog.IsTeam
				),
		        null,
		        BXTextEncoder.EmptyTextEncoder
		    )
		    .FirstOrDefault();

			if (blogUser == null)
			    return;

			foreach (var gl in blogUser.UserGroups)
			{
				var g = gl.UserGroup;
				if (g == null)
					continue;
				bindings.Add(new BindingInfo { BlogGroup = g, Editable = g.Blog != null && g.Blog.IsTeam && g.Type == BXBlogUserGroupType.UserDefined, IsAuto = gl.IsAuto });
			}
			bindings.Sort((a, b) =>
			{
				return a.Editable.CompareTo(b.Editable);
			});


		}
		public override void Save(BXUser user, BXSqlTransaction tran)
		{
			if (blogUser == null)
			{
				blogUser = BXBlogUser.GetList(
					new BXFilter(new BXFilterItem(BXBlogUser.Fields.Id, BXSqlFilterOperators.Equal, user.UserId)),
					null,
					null,
					new BXQueryParams { Transaction = tran },
					BXTextEncoder.EmptyTextEncoder
				)
				.FirstOrDefault();
			}
			if (blogUser == null)
			{
				blogUser = new BXBlogUser(BXTextEncoder.EmptyTextEncoder);
				blogUser.Id = user.UserId;
			}

			var groups = Request.Form.GetValues(UniqueID + "$group");
			var blogs = Request.Form.GetValues(UniqueID + "$blog");
			var autos = Request.Form.GetValues(UniqueID + "$auto");
			var newBindings = new List<BindingInfo>();
			if (groups != null && groups.Length > 0)
			{
				for (int i = 0; i < groups.Length; i++)
				{
					int id;
					if (!int.TryParse(groups[i], out id) || id <= 0)
						continue;

					var group = BXBlogUserGroup.GetList(
						new BXFilter(new BXFilterItem(BXBlogUserGroup.Fields.Id, BXSqlFilterOperators.Equal, id)),
						null,
						new BXSelectAdd(BXBlogUserGroup.Fields.Blog.Name),
						new BXQueryParams { AllowPaging = true, PagingStartIndex = 0, PagingRecordCount = 1, Transaction = tran },
						BXTextEncoder.EmptyTextEncoder
					)
					.FirstOrDefault();
					
					if (group == null)
						continue;
					
					if (group.BlogId <= 0 || !Blogs.Exists(x => x.Id == group.BlogId) || group.Type != BXBlogUserGroupType.UserDefined)
					{
						var b = bindings.Find(x => x.BlogGroup.Id == id);
						if (b != null && !newBindings.Exists(x => x.BlogGroup.Id == group.Id))
							newBindings.Add(new BindingInfo { BlogGroup = group, Editable = false, IsAuto = b.IsAuto });
						continue;
					}

					if (!newBindings.Exists(x => x.BlogGroup.Id == group.Id))
					{
						var b = bindings.Find(x => x.BlogGroup.Id == id);
						newBindings.Add(new BindingInfo { BlogGroup = group, Editable = true, IsAuto = b != null && b.IsAuto && autos[i] == "true" });
					}
				}				
			}
			bindings = newBindings;

			blogUser.UserGroups.Clear();
			blogUser.UserGroups.AddRange(bindings.ConvertAll(x => new BXBlogUser2Group(x.BlogGroup.Id)));
			blogUser.Save(tran != null ? tran.Connection : null, tran);
		}
		protected string MakeAjaxUrl()
		{			
			return new Uri(BXSefUrlManager.CurrentUrl, VirtualPathUtility.ToAbsolute("~/bitrix/handlers/Blog/GetBlogGroups.ashx") + "?" + BXCsrfToken.BuildQueryStringPair()).AbsoluteUri;
		}
		private List<BlogInfo> LoadBlogs()
		{
			blogs = BXBlog.GetList(
				new BXFilter(
					new BXFilterItem(BXBlog.Fields.IsTeam, BXSqlFilterOperators.Equal, true),
					new BXFilterItem(BXBlog.Fields.GetFieldByKey("HasUserDefinedGroups"), BXSqlFilterOperators.Equal, true)
				),
				new BXOrderBy(new BXOrderByPair(BXBlog.Fields.Name, BXOrderByDirection.Asc)),
				new BXSelect(BXBlog.Fields.Id, BXBlog.Fields.Name),
				null,
				BXTextEncoder.EmptyTextEncoder
			)
			.ConvertAll(x => new BlogInfo
			{
				Id = x.Id,
				Title = x.Name
			});
			
			return blogs;
		}

		protected class BindingInfo
		{
			public BXBlogUserGroup BlogGroup;
			public bool IsAuto;
			public bool Editable;
			public bool Delete;
		}

		protected class BlogInfo
		{
			public string Title;
			public int Id;
		}
	}
}