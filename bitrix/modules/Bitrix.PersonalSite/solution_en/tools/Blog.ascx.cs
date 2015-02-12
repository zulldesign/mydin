using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Xml;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services.Text;
using Bitrix.Blog;
using System.Web;
using Bitrix.DataTypes;

namespace Bitrix.Install.Internal
{
	public partial class BlogXmlImporterDummy : UserControl
	{
	
	}

	public class BlogXmlImporter
	{
		Dictionary<int, Dictionary<string, int>> customFieldEnums = new Dictionary<int, Dictionary<string, int>>();

		DateProcessor dateProcessor;
		string siteId;
		int userId;
		string solutionId;
		BXSite site;

		private BXParamsBag<string> replace;
		public BXParamsBag<string> Replace
		{
			get
			{
				return replace ?? (replace = new BXParamsBag<string>());
			}
		}

		public BlogXmlImporter(string siteId, int userId, string solutionId)
		{
			dateProcessor = new DateProcessor();
			this.siteId = siteId;
			this.userId = userId;
			this.solutionId = solutionId;
			this.site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
		}

		public void LoadBlogs(string filePath)
		{
			string dir = Path.GetDirectoryName(filePath);
			using (XmlTextReader reader = new XmlTextReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
			{
				while (reader.NodeType != XmlNodeType.Element && reader.Read())
					;
				if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
					return;

				// we are now at the root element;

				while (reader.Read())
				{
					if (reader.NodeType != XmlNodeType.Element)
						continue;
					using (var subReader = reader.ReadSubtree())
						ReadElement(subReader, dir);
				}
			}

			return;
		}

		private void ReadElement(XmlReader reader, string root)
		{
			reader.Read();
			switch (reader.Name)
			{
				case "category":
					ImportBlogCategory(reader);
					break;
				case "blog":
					ImportBlog(reader);
					break;
				case "post":
					ImportBlogPost(reader, root);
					break;
				case "fields":
					ImportCustomFields(reader, root);
					break;
			}
		}

		private void ImportCustomFields(XmlReader r, string root)
		{
			string entity = ProcessValue(r.GetAttribute("for"));
			if (BXStringUtility.IsNullOrTrimEmpty(entity))
				return;
			if (string.Equals(entity, "blog", StringComparison.OrdinalIgnoreCase))
				entity = "Blog";
			else if (string.Equals(entity, "post", StringComparison.OrdinalIgnoreCase))
				entity = "BlogPost";
			else 
				return;

			if (r.IsEmptyElement)
				return;

			while (r.Read())
			{
				if (r.NodeType != XmlNodeType.Element || r.Name != "def")
					continue;
				ImportCustomPropertyDefinition(r, entity);
			}
		}

		private void ImportBlogPost(XmlReader r, string root)
		{
			var xmlId = ProcessValue(r.GetAttribute("xmlid"));
			
			BXBlog blog = null;
			var blogXmlId = ProcessValue(r.GetAttribute("blogid"));
			if (!string.IsNullOrEmpty(blogXmlId))
				blog = GetBlog(blogXmlId, false);
			
			var post = GetPost(xmlId, blog != null ? (int?)blog.Id : null, true);
			Set(r.GetAttribute("authorid"), x => post.AuthorId = x);
			Set(r.GetAttribute("authorname"), x => post.AuthorName = x);
			Set(r.GetAttribute("authoremail"), x => post.AuthorEmail = x);

			if (blog == null)
				blog = post.Blog;
			else
				post.BlogId = blog.Id;

			if (blog == null)
				return;

			Set(r.GetAttribute("published"), x => post.IsPublished = x);
			Set(r.GetAttribute("views"), x => post.ViewCount += x);
			Set(r.GetAttribute("enablecomments"), x => post.EnableComments = x);
			Set(r.GetAttribute("notifyofcomments"), x => post.NotifyOfComments = x);
			Set(r.GetAttribute("tags"), x => post.Tags = x);
			Set(r.GetAttribute("title"), x => post.Title = x);


			List<BXBlogComment> comments = null;
			if (!r.IsEmptyElement && r.Read())
			while (true)
			{
				if (r.NodeType == XmlNodeType.Element)
				switch (r.Name)
				{
					case "content":
						if (r.IsEmptyElement)
							break;

						SetEnum<BXBlogPostContentType>(r.GetAttribute("type"), x => post.ContentType = x);
						SetBlogPostContent(post, blog, ProcessValue(r.ReadElementContentAsString()), root);
						continue;
					case "comment":
						if (comments == null)
							comments = new List<BXBlogComment>();
						using (var commentsReader = r.ReadSubtree())
							ImportBlogComment(comments, -1, commentsReader);
						break;
					case "properties":
						using (var propertiesReader = r.ReadSubtree())
							ReadProperties(propertiesReader, BXBlogPost.CustomFields, post.CustomPublicValues);
						break;
				}
				if (!r.Read())
					break;
			}
		
			post.Save();
			if (comments != null && comments.Count > 0)
			{
				foreach (var c in comments)
				{
					c.BlogId = blog.Id;
					c.PostId = post.Id;
					if (c.ParentId == -1)
						c.ParentId = 0;
					else
						c.ParentId = comments[c.ParentId].Id;
					c.Save();
				}
			}
		}

		private void ImportBlogComment(List<BXBlogComment> comments, int parent, XmlReader r)
		{
			r.Read();
			if (r.IsEmptyElement)
				return;

			var comment = new BXBlogComment();
			comment.IsApproved = true;
			comment.ParentId = parent;
			comments.Add(comment);
			int index = comments.Count - 1;

			comment.AuthorId = 0;
			Set(r.GetAttribute("authorid"), x => comment.AuthorId = x);
			Set(r.GetAttribute("authorname"), x => comment.AuthorName = x);
			Set(r.GetAttribute("authoremail"), x => comment.AuthorEmail = x);

			if (r.Read())
			while (true)
			{
				if (r.NodeType == XmlNodeType.Element)
				switch (r.Name)
				{
					case "content":
						if (r.IsEmptyElement)
							break;

						comment.Content = ProcessValue(r.ReadElementContentAsString());
						continue;
					case "comment":
						using (var commentsReader = r.ReadSubtree())
							ImportBlogComment(comments, index, commentsReader);
						break;
				}
				if (!r.Read())
					break;
			}
		}

		private void SetBlogPostContent(BXBlogPost post, BXBlog blog, string content, string root)
		{
			var subfolder = BXBlogModuleConfiguration.ImageDirectoryName + "/" + (!string.IsNullOrEmpty(blog.Slug) ? blog.Slug : blog.Id.ToString());

			post.Content = Regex.Replace(
				content,
				@"#\s*img\s*:\s*([^#]*)#",
				m =>
				{
					string path = m.Groups[1].Value;
					if (BXStringUtility.IsNullOrTrimEmpty(path))
						return m.Value;

					path = Path.Combine(root, path);

					string fileName = Path.GetFileName(path);
					string contentType;
					if (!BXFileInfo.TryGetImageMimeType(fileName, out contentType))
						contentType = "image";

					BXFile file;
					using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
					{
						file = new BXFile(fs, fileName, subfolder, "blog", "", contentType);
						file.Save();
					}
					post.FileLinks.Add(new BXBlogPost2File(file.Id, blog.Id, post.Id));

					return VirtualPathUtility.ToAbsolute(file.FileVirtualPath);
				},
				RegexOptions.IgnoreCase
			);
		}

		private BXBlogPost GetPost(string xmlId, int? blogId, bool create)
		{
			BXBlogPost post = null;
			if (!string.IsNullOrEmpty(xmlId))
			{
				var f = new BXFilter(new BXFilterItem(BXBlogPost.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId));
				if (blogId != null)
					f.Add(new BXFilterItem(BXBlogPost.Fields.Blog.Id, BXSqlFilterOperators.Equal, blogId));

				var posts = BXBlogPost.GetList(
					f,
					null,
					new BXSelectAdd(BXBlogPost.Fields.Files, BXBlogPost.Fields.CustomFields.DefaultFields),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (posts.Count > 0)
					post = posts[0];
			}

			if (post == null && create)
			{
				post = new BXBlogPost(BXTextEncoder.EmptyTextEncoder);
				if (!string.IsNullOrEmpty(xmlId))
					post.XmlId = xmlId;
			}

			return post;
		}

		private void ImportBlog(XmlReader r)
		{
			var xmlId = ProcessValue(r.GetAttribute("xmlid"));
			var blog = GetBlog(xmlId, true);
			Set(r.GetAttribute("active"), x => blog.Active = x);
			Set(r.GetAttribute("slug"), x => blog.Slug = x);
			SetEnum<BXBlogIndexContentMode>(r.GetAttribute("indexcontent"), x => blog.IndexContent = x);
			Set(r.GetAttribute("name"), x => blog.Name = x);
			Set(r.GetAttribute("notifyofcomments"), x => blog.NotifyOfComments = x);
			Set(r.GetAttribute("ownerid"), x => blog.OwnerId = x);
			Set(r.GetAttribute("sort"), x => blog.Sort = x);
			string store = r.GetAttribute("store-id");

			if (!r.IsEmptyElement && r.Read())
			while (true)
			{
				var name = r.Name;
				if (r.NodeType == XmlNodeType.Element)
				switch (name)
				{
					case "description":
						if (r.IsEmptyElement)
							break;
						blog.Description = ProcessValue(r.ReadElementContentAsString());
						continue;
					case "category":
						if (r.IsEmptyElement)
							break;
						var catXmlId = ProcessValue(r.ReadElementContentAsString());
						BXBlogCategory cat;
						if (!string.IsNullOrEmpty(catXmlId) && (cat = GetCategory(catXmlId, false)) != null && !blog.CategoryLinks.Exists(x => x.CategoryId == cat.Id))
							blog.CategoryLinks.Add(new BXBlog2Category(cat.Id));
						continue;
					case "properties":
						using (var propertiesReader = r.ReadSubtree())
							ReadProperties(propertiesReader, blog.CustomFields, blog.CustomPublicValues);
						break;
				}
				if (!r.Read())
					break;
			}

			blog.Save();
			if (!string.IsNullOrEmpty(store))
				 BXOptionManager.SetOptionInt(solutionId, store, blog.Id, siteId);
		}

		private void ReadProperties(XmlReader r, BXCustomFieldCollection fields, BXCustomPropertyValueModificator values)
		{
			r.Read();
			if (r.IsEmptyElement)
				return;

			while (r.Read())
			{
				if (r.NodeType != XmlNodeType.Element)
					continue;

				var name = r.Name;
				if (r.IsEmptyElement)
					continue;
				var value = ProcessValue(r.ReadElementContentAsString());

				BXCustomField f;
				if (!fields.TryGetValue(name, out f))
					return;

				if (f.CustomTypeId == "Bitrix.System.Enumeration")
				{
					int id = GetCustomFieldEnumId(f.Id, value);
					if (id > 0)
						values.Set(f.Name, id);
				}
				else
					values.Set(f.Name, value);
			}
		}

		private BXBlog GetBlog(string xmlId, bool create)
		{
			BXBlog blog = null;
			if (!string.IsNullOrEmpty(xmlId))
			{
				var blogs = BXBlog.GetList(
					new BXFilter(new BXFilterItem(BXBlog.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)),
					null,
					new BXSelectAdd(BXBlog.Fields.Categories, BXBlog.Fields.CustomFields.DefaultFields),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (blogs.Count > 0)
					blog = blogs[0];
			}

			if (blog == null && create)
			{
				blog = new BXBlog(BXTextEncoder.EmptyTextEncoder);
				if (!string.IsNullOrEmpty(xmlId))
					blog.XmlId = xmlId;
			}

			return blog;
		}

		private void ImportBlogCategory(XmlReader r)
		{
			var xmlId = ProcessValue(r.GetAttribute("xmlid"));
			var category = GetCategory(xmlId, true);
			Set(r.GetAttribute("name"), x => category.Name = x);
			Set(r.GetAttribute("sort"), x => category.Sort = x);

			if (!category.SiteLinks.Exists(x => string.Equals(x.SiteId, site.Id, StringComparison.OrdinalIgnoreCase)))
				category.SiteLinks.Add(new BXBlogCategory2Site(site.Id));
			category.Save();
		}

		private BXBlogCategory GetCategory(string xmlId, bool create)
		{
			BXBlogCategory cat = null;
			if (!string.IsNullOrEmpty(xmlId))
			{
				var cats = BXBlogCategory.GetList(
					new BXFilter(new BXFilterItem(BXBlogCategory.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)),
					null,
					new BXSelectAdd(BXBlogCategory.Fields.Sites),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (cats.Count > 0)
					cat = cats[0];
			}

			if (cat == null && create)
			{
				cat = new BXBlogCategory(BXTextEncoder.EmptyTextEncoder);
				if (!string.IsNullOrEmpty(xmlId))
					cat.XmlId = xmlId;
			}

			return cat;
		}

		#region Import Custom property definition
		void ImportCustomPropertyDefinition(XmlReader reader, string entity)
		{
			string idAttr, typeAttr;

			idAttr = reader.GetAttribute("id");
			typeAttr = reader.GetAttribute("type");
			string listColumnLabel = reader.GetAttribute("listcolumnlabel");

			BXCustomField fld = null;
			try
			{
				fld = GetCustomField(entity, idAttr, typeAttr);
				fld.IsSearchable = (reader.GetAttribute("issearchable") ?? "").Equals("true", StringComparison.OrdinalIgnoreCase);
				fld.Mandatory = (reader.GetAttribute("mandatory") ?? "").Equals("true", StringComparison.OrdinalIgnoreCase);
				fld.ShowInFilter = BXCustomFieldFilterVisibility.CompleteMatch;
				fld.ShowInList = true;
				fld.EditInList = true;
				fld.Save();
			}
			catch
			{
				reader.Skip();
				return;
			}

			using (XmlReader nestedReader = reader.ReadSubtree())
			{
				while (nestedReader.Read())
				{
					if (nestedReader.NodeType.Equals(XmlNodeType.Element))
					{
						switch (nestedReader.Name)
						{
							case "loc":
								string lang = nestedReader.GetAttribute("lang");
								BXCustomFieldLocalization loc = new BXCustomFieldLocalization(lang);
								loc.EditFormLabel = ProcessValue(nestedReader.ReadElementContentAsString());

								fld.Localization.Add(loc);
								break;
							case "enum":
								ImportCustomFieldEnum(reader, fld);
								break;
							case "settings":
								ImportCustomFieldSettings(reader, fld);
								break;
						}
					}
				}
				fld.Save();
			}
		}
		void ImportCustomFieldEnum(XmlReader reader, BXCustomField fld)
		{
			int i;
			using (XmlReader nestedReader = reader.ReadSubtree())
			{
				while (nestedReader.Read())
				{
					if (nestedReader.NodeType.Equals(XmlNodeType.Element))
					{
						switch (nestedReader.Name)
						{
							case "value":

								int sort = 100;
								Set(nestedReader.GetAttribute("sort"), x => sort = x);
								
								bool isDefault = false;
								Set(nestedReader.GetAttribute("default"), x => isDefault = x);
								
								string xmlId = ProcessValue(nestedReader.GetAttribute("xmlid"));
								string name = ProcessValue(nestedReader.ReadElementContentAsString());
								if (!String.IsNullOrEmpty(name))
								{
									BXCustomFieldEnum enumeration = GetCustomFieldEnum(xmlId, fld.Id);
									enumeration.FieldId = fld.Id;
									enumeration.Value = name;
									enumeration.XmlId = xmlId;
									enumeration.Default = isDefault;
									enumeration.Sort = sort;
									enumeration.Save();
								}

								break;
						}

					}

				}

			}
		}
		void ImportCustomFieldSettings(XmlReader reader, BXCustomField fld)
		{
			using (XmlReader nestedReader = reader.ReadSubtree())
			{
				while (nestedReader.Read())
				{
					if (nestedReader.NodeType.Equals(XmlNodeType.Element))
					{
						switch (nestedReader.Name)
						{
							case "setting":
								string name = ProcessValue(reader.GetAttribute("name"));
								string value = ProcessValue(reader.ReadElementContentAsString());
								if (!String.IsNullOrEmpty(name))
									fld.Settings[name] = value;
								break;
						}

					}

				}
				fld.Save();
			}
		}
		BXCustomFieldEnum GetCustomFieldEnum(string xmlId, int fieldId)
		{
			BXCustomFieldEnumCollection customFieldColl = BXCustomFieldEnum.GetList(
				new BXFilter(
					new BXFilterItem(BXCustomFieldEnum.Fields.FieldId, BXSqlFilterOperators.Equal, fieldId),
					new BXFilterItem(BXCustomFieldEnum.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)
				),
				null
			);

			return customFieldColl.Count > 0 ? customFieldColl[0] : new BXCustomFieldEnum();
		}
		BXCustomField GetCustomField(string entityId, string fieldName, string customTypeId)
		{
			BXCustomFieldCollection customFieldColl = BXCustomField.GetList(
				new BXFilter(
					new BXFilterItem(BXCustomField.Fields.Name, BXSqlFilterOperators.Equal, fieldName),
					new BXFilterItem(BXCustomField.Fields.OwnerEntityId, BXSqlFilterOperators.Equal, entityId)
				),
				null
			);

			return customFieldColl.Count > 0 ? customFieldColl[0] : new BXCustomField(entityId, fieldName, customTypeId);
		}
		int GetCustomFieldEnumId(int fieldId, string name)
		{
			if (customFieldEnums.ContainsKey(fieldId) && customFieldEnums[fieldId].ContainsKey(name))
				return customFieldEnums[fieldId][name];

			BXCustomFieldEnumCollection col = BXCustomFieldEnum.GetList(
				new BXFilter(new BXFilterItem(BXCustomFieldEnum.Fields.FieldId, BXSqlFilterOperators.Equal, fieldId),
								new BXFilterItem(BXCustomFieldEnum.Fields.Value, BXSqlFilterOperators.Equal, name)), null);

			if (col.Count > 0)
			{
				if (!customFieldEnums.ContainsKey(fieldId))
					customFieldEnums.Add(fieldId, new Dictionary<string, int>());
				if (customFieldEnums[fieldId] == null)
					customFieldEnums[fieldId] = new Dictionary<string, int>();

				customFieldEnums[fieldId].Add(col[0].Value, col[0].Id);
				return col[0].Id;
			}
			else
				return -1;
		}
		#endregion


		string ProcessValue(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			return Regex.Replace(input, "#([^#]*)#", GetReplacement);
		}

		private string GetReplacement(Match m)
		{
			string key = m.Groups[1].Value;
			if (key == "")
				return "#";

			string val;
			if (replace != null && replace.TryGetValue(key, out val))
				return val;
			if (string.Equals(key, "SiteId", StringComparison.OrdinalIgnoreCase))
				return siteId;
			if (string.Equals(key, "SiteName", StringComparison.OrdinalIgnoreCase))
				return site.SiteName;
			if (string.Equals(key, "UserId", StringComparison.OrdinalIgnoreCase))
				return userId.ToString();
			return m.Value;
		}

		private void Set(string value, Action<string> action)
		{
			if (value != null)
				action(ProcessValue(value));
		}
		private void Set(string value, Action<int> action)
		{
			int i;
			if (value != null && int.TryParse(ProcessValue(value), out i))
				action(i);
		}
		private void Set(string value, Action<bool> action)
		{
			bool f;
			if (value != null && bool.TryParse(ProcessValue(value), out f))
				action(f);
		}
		private void SetEnum<TEnum>(string value, Action<TEnum> action) where TEnum : struct
		{

			if (value != null)
			{
				try
				{
					action((TEnum)Enum.Parse(typeof(TEnum), value, true));
				}
				catch
				{
				}
			}
		}

		public class DateProcessor
		{
			public char[] actionSeparators = { '.', '(' };
			public string[] Operations
			{
				get
				{
					return new string[] { "AddDays", "AddHours", "AddSeconds" };
				}
			}
			public string[] Values
			{
				get
				{
					return new string[] { "Now" };
				}
			}
			public bool Process(string input, out DateTime outputDate)
			{

				if (DateTime.TryParse(input, out outputDate))
					return true;
				outputDate = DateTime.Now;
				if (String.IsNullOrEmpty(input))
					return false;
				if (input.Equals("Now", StringComparison.OrdinalIgnoreCase))
					return true;
				string[] workArray = input.Split(actionSeparators);
				if (workArray.Length != 3)
					return false;

				string value = workArray[0];
				if (Array.IndexOf(Values, value) < 0)
					return false;

				string operation = workArray[1];
				if (Array.IndexOf(Operations, operation) < 0)
					return false;

				int opValue;
				if (!Int32.TryParse(workArray[2].Replace(")", ""), out opValue))
					return false;

				DateTime valueToStartWith = new DateTime();

				switch (value)
				{
					case "Now":
						valueToStartWith = DateTime.Now;
						break;
				}
				switch (operation)
				{
					case "AddDays":
						outputDate = valueToStartWith.AddDays(opValue);
						return true;
					case "AddHours":
						outputDate = valueToStartWith.AddHours(opValue);
						return true;
					case "AddSeconds":
						outputDate = valueToStartWith.AddSeconds(opValue);
						return true;
				}
				return false;
			}
		}
	}
}