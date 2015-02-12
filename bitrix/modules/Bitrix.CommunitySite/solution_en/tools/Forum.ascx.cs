using System;
using System.Collections.Generic;
using System.Web.UI;
using Bitrix.Forum;
using Bitrix.DataTypes;
using Bitrix.Services.Text;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using Bitrix.DataLayer;
using Bitrix.Security;


namespace Bitrix.Install.Internal
{
	public partial class ForumXmlImporterDummy : UserControl
	{
	
	}

	public class ForumXmlImporter
	{
		Dictionary<int, Dictionary<string, int>> customFieldEnums = new Dictionary<int, Dictionary<string, int>>();

		DateProcessor dateProcessor;
		string siteId;
		BXSite site;

		private BXParamsBag<string> replace;
		public BXParamsBag<string> Replace
		{
			get
			{
				return replace ?? (replace = new BXParamsBag<string>());
			}
		}

		public ForumXmlImporter(string siteId)
		{
			dateProcessor = new DateProcessor();
			this.siteId = siteId;
			this.site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
		}

		public void LoadForums(string filePath)
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
					ImportForumCategory(reader);
					break;
				case "forum":
					ImportForum(reader);
					break;
				case "topic":
					ImportForumTopic(reader);
					break;
				case "post":
					ImportForumPost(reader);
					break;
				case "fields":
					ImportCustomFields(reader, root);
					break;
			}
		}

		private void ImportForumCategory(XmlReader reader)
		{
			var xmlId = ProcessValue(reader.GetAttribute("xmlid"));
			var category = GetCategory(xmlId, true);

			Set(reader.GetAttribute("name"), x => category.Name = x);
			Set(reader.GetAttribute("sort"), x => category.Sort = x);

			category.Save();
		}
		private BXForumCategory GetCategory(string xmlId, bool create)
		{
			BXForumCategory cat = null;
			if (!string.IsNullOrEmpty(xmlId))
			{
				var cats = BXForumCategory.GetList(
					new BXFilter(new BXFilterItem(BXForumCategory.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)),
					null,
					null,
					null,
					BXTextEncoder.EmptyTextEncoder
				);

				if (cats.Count > 0)
					cat = cats[0];
			}

			if (cat == null && create)
			{
				cat = new BXForumCategory("", BXTextEncoder.EmptyTextEncoder);
				if (!string.IsNullOrEmpty(xmlId))
					cat.XmlId = xmlId;
			}

			return cat;
		}

		private void ImportForum(XmlReader reader)
		{
			var xmlId = ProcessValue(reader.GetAttribute("xmlid"));

			var forum = GetForum(xmlId, true);

			Set(reader.GetAttribute("active"), x => forum.Active = x);
			Set(reader.GetAttribute("allowbbcode"), x => forum.AllowBBCode = x);
			Set(reader.GetAttribute("allowsmiles"), x => forum.AllowSmiles = x);
			Set(reader.GetAttribute("code"), x => forum.Code = x);
			Set(reader.GetAttribute("indexcontent"), x => forum.IndexContent = x);
			Set(reader.GetAttribute("name"), x => forum.Name = x);
			Set(reader.GetAttribute("sort"), x => forum.Sort = x);

			var categoryXmlId = ProcessValue(reader.GetAttribute("category"));
			BXForumCategory category;
			if (!String.IsNullOrEmpty(categoryXmlId) && (category = GetCategory(categoryXmlId, false)) != null && forum.CategoryId != category.Id)
				forum.CategoryId = category.Id;

			if (!forum.Sites.Exists(x => String.Equals(x.SiteId, site.Id, StringComparison.OrdinalIgnoreCase)))
				forum.Sites.Add(new BXForum.BXForumSite(site.Id));

			if (!reader.IsEmptyElement && reader.Read())
				while (true)
				{
					var name = reader.Name;
					if (reader.NodeType == XmlNodeType.Element)
						switch (name)
						{
							case "description":
								if (reader.IsEmptyElement)
									break;
								forum.Description = ProcessValue(reader.ReadElementContentAsString());
								continue;

							case "properties":
								using (var propertiesReader = reader.ReadSubtree())
									ReadProperties(propertiesReader, forum.CustomFields, forum.CustomPublicValues);
								break;
						}
					if (!reader.Read())
						break;
				}

			forum.Save();
		}

		private BXForum GetForum(string xmlId, bool create)
		{
			BXForum forum = null;
			if (!string.IsNullOrEmpty(xmlId))
			{
				var forums = BXForum.GetList(
					new BXFilter(new BXFilterItem(BXForum.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)),
					null,
					new BXSelectAdd(BXForum.Fields.Category, BXForum.Fields.Sites, BXForum.Fields.CustomFields.DefaultFields),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (forums.Count > 0)
					forum = forums[0];
			}

			if (forum == null && create)
			{
				forum = new BXForum(BXTextEncoder.EmptyTextEncoder);
				if (!string.IsNullOrEmpty(xmlId))
					forum.XmlId = xmlId;
			}

			return forum;
		}

		private void ImportForumTopic(XmlReader reader)
		{
			var forumXmlId = ProcessValue(reader.GetAttribute("forumid"));
			BXForum forum = null;
			if (String.IsNullOrEmpty(forumXmlId) || (forum = GetForum(forumXmlId, false)) == null)
				return;

			var topicXmlId = ProcessValue(reader.GetAttribute("xmlid"));
			var topic = GetTopic(topicXmlId, (int?)forum.Id, true);

			Set(reader.GetAttribute("approved"), x => topic.Approved = x);
			Set(reader.GetAttribute("closed"), x => topic.Closed = x);
			Set(reader.GetAttribute("datecreate"), x => topic.DateCreate = x);
			Set(reader.GetAttribute("name"), x => topic.Name = x);
			Set(reader.GetAttribute("views"), x => topic.Views = x);
			Set(reader.GetAttribute("stickyindex"), x => topic.StickyIndex = x);
			Set(reader.GetAttribute("description"), x => topic.Description = x);
			Set(reader.GetAttribute("movedto"), x => topic.MovedTo = x);

			int authorId; string authorName; string authorEmail;
			GetUserAttr(reader, out authorId, out authorName, out authorEmail);

			topic.AuthorId = authorId;
			topic.AuthorName = authorName;

			if (!reader.IsEmptyElement && reader.Read())
				while (true)
				{
					var name = reader.Name;
					if (reader.NodeType == XmlNodeType.Element)
						switch (name)
						{
							case "properties":
								using (var propertiesReader = reader.ReadSubtree())
									ReadProperties(propertiesReader, topic.CustomFields, topic.CustomPublicValues);
								break;
						}
					if (!reader.Read())
						break;
				}


			topic.Save();
		}

		private void GetUserAttr(XmlReader reader, out int authorId, out string authorName, out string authorEmail)
		{
			//Defaults
			authorId = 0;
			authorName = ProcessValue(reader.GetAttribute("authorName"));
			authorEmail = ProcessValue(reader.GetAttribute("authorEmail"));
			if (String.IsNullOrEmpty(authorName))
				authorName = "Гость";

			var userId = ProcessValue(reader.GetAttribute("authorId"));
			var userLogin = ProcessValue(reader.GetAttribute("authorLogin"));

			BXUser user = null;
			if (!String.IsNullOrEmpty(userId) && int.TryParse(userId, out authorId) && (user = BXUser.GetById(authorId, BXTextEncoder.EmptyTextEncoder)) != null)
			{
				authorId = user.UserId;
				authorName = user.GetDisplayName();
				authorEmail = user.Email;
			}
			else if (!String.IsNullOrEmpty(userLogin))
			{
				BXFilter filter = new BXFilter(new BXFilterItem(BXUser.Fields.UserName, BXSqlFilterOperators.Equal, userLogin));
				var providerName = ProcessValue(reader.GetAttribute("providerName"));
				if (!String.IsNullOrEmpty(providerName))
					filter.Add(new BXFilterItem(BXUser.Fields.ProviderName, BXSqlFilterOperators.Equal, providerName));

				BXUserCollection users = BXUser.GetList(filter, null, null, null, BXTextEncoder.EmptyTextEncoder);

				if (users.Count > 0)
				{
					user = users[0];
					authorId = user.UserId;
					authorName = user.GetDisplayName();
					authorEmail = user.Email;
				}
			}
		}

		private BXForumTopic GetTopic(string xmlId, int? forumId, bool create)
		{
			BXForumTopic topic = null;
			if (!string.IsNullOrEmpty(xmlId))
			{
				var filter = new BXFilter(new BXFilterItem(BXForumTopic.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId));
				if (forumId != null)
					filter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.Equal, forumId));

				var topics = BXForumTopic.GetList(
					filter,
					null,
					new BXSelectAdd(BXForum.Fields.CustomFields.DefaultFields),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (topics.Count > 0)
					topic = topics[0];
			}

			if (topic == null && create)
			{
				topic = new BXForumTopic();
				if (!string.IsNullOrEmpty(xmlId))
					topic.XmlId = xmlId;

				if (forumId != null)
					topic.ForumId = (int)forumId;
			}

			return topic;
		}

		private void ImportForumPost(XmlReader reader)
		{
			BXForum forum = null;
			var forumXmlId = ProcessValue(reader.GetAttribute("forumid"));
			if (!String.IsNullOrEmpty(forumXmlId))
				forum = GetForum(forumXmlId, false);

			BXForumTopic topic = null;
			var topicXmlId = ProcessValue(reader.GetAttribute("topicid"));
			if (String.IsNullOrEmpty(topicXmlId) || (topic = GetTopic(topicXmlId, forum != null ? (int?)forum.Id : null, false)) == null)
				return;
			
			var postXmlId = ProcessValue(reader.GetAttribute("xmlid"));
			var post = GetPost(postXmlId, (int?)topic.Id, forum != null ? (int?)forum.Id : null, true);

			Set(reader.GetAttribute("allowsmiles"), x => post.AllowSmiles = x);
			Set(reader.GetAttribute("approved"), x => post.Approved = x);
			Set(reader.GetAttribute("authorip"), x => post.AuthorIP = x);
			Set(reader.GetAttribute("datecreate"), x => post.DateCreate = x);
			Set(reader.GetAttribute("dateupdate"), x => post.DateUpdate = x);

			post.TopicId = topic.Id;
			post.ForumId = topic.ForumId;

			int authorId; string authorName; string authorEmail;
			GetUserAttr(reader, out authorId, out authorName, out authorEmail);

			post.AuthorId = authorId;
			post.AuthorName = authorName;
			post.AuthorEmail = authorEmail;

			if (!reader.IsEmptyElement && reader.Read())
				while (true)
				{
					var name = reader.Name;
					if (reader.NodeType == XmlNodeType.Element)
						switch (name)
						{
							case "content":
								if (reader.IsEmptyElement)
									break;
								post.Post = ProcessValue(reader.ReadElementContentAsString());
								continue;

							case "properties":
								using (var propertiesReader = reader.ReadSubtree())
									ReadProperties(propertiesReader, post.CustomFields, post.CustomPublicValues);
								break;
						}
					if (!reader.Read())
						break;
				}

			post.Save();
		}

		private BXForumPost GetPost(string xmlId, int? topicId, int? forumId, bool create)
		{
			BXForumPost post = null;
			if (!string.IsNullOrEmpty(xmlId))
			{
				var filter = new BXFilter(new BXFilterItem(BXForumPost.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId));
				if (topicId != null)
					filter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, topicId));

				if (forumId != null)
					filter.Add(new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, forumId));

				var posts = BXForumPost.GetList(
					filter,
					null,
					new BXSelectAdd(BXForumPost.Fields.CustomFields.DefaultFields),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (posts.Count > 0)
					post = posts[0];
			}

			if (post == null && create)
			{
				post = new BXForumPost();
				if (!string.IsNullOrEmpty(xmlId))
					post.XmlId = xmlId;
			}

			return post;
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

		private void ImportCustomFields(XmlReader r, string root)
		{
			string entity = ProcessValue(r.GetAttribute("for"));
			if (BXStringUtility.IsNullOrTrimEmpty(entity))
				return;
			if (string.Equals(entity, "forum", StringComparison.OrdinalIgnoreCase))
				entity = "Forum";
			else if (string.Equals(entity, "category", StringComparison.OrdinalIgnoreCase))
				entity = "ForumCategory";
			else if (string.Equals(entity, "topic", StringComparison.OrdinalIgnoreCase))
				entity = "ForumTopic";
			else if (string.Equals(entity, "post", StringComparison.OrdinalIgnoreCase))
				entity = "ForumPost";
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
			//if (string.Equals(key, "UserId", StringComparison.OrdinalIgnoreCase))
			//    return userId.ToString();
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

		private void Set(string value, Action<DateTime> action)
		{
			DateTime date;
			if (value != null && dateProcessor.Process(value, out date))
				action(date);
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