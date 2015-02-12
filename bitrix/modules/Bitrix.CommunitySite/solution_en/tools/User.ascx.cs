using System;
using System.Collections.Generic;
using System.Web.UI;
using Bitrix.Services.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Bitrix.DataLayer;
using System.IO;
using Bitrix.DataTypes;
using Bitrix.Security;
using Bitrix.IO;
using System.Globalization;


namespace Bitrix.Install.Internal
{
	public partial class UserXmlImporterDummy : UserControl
	{
	
	}

	public class UserXmlImporter
	{

		Dictionary<int, Dictionary<string, int>> customFieldEnums = new Dictionary<int, Dictionary<string, int>>();

		DateProcessor dateProcessor;
		//string siteId;
		//BXSite site;

		private BXParamsBag<string> replace;
		public BXParamsBag<string> Replace
		{
			get
			{
				return replace ?? (replace = new BXParamsBag<string>());
			}
		}

		public UserXmlImporter()
		{
			dateProcessor = new DateProcessor();
			//this.siteId = siteId;
			//this.site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
		}

		public void LoadUsers(string filePath)
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
				case "user":
					ImportUser(reader, root);
					break;
				case "role":
					ImportRole(reader);
					break;
				case "task":
					ImportTask(reader);
					break;
				case "fields":
					ImportCustomFields(reader, root);
					break;
			}
		}

		private void ImportTask(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		private void ImportRole(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		private void ImportUser(XmlReader reader, string rootDir)
		{
			var login = ProcessValue(reader.GetAttribute("username"));
			var user = GetUser(login, true);

			//Required Fields
			Set(reader.GetAttribute("email"), x => user.Email = x);
			if (user.IsNew)
			{
				var providerName = ProcessValue(reader.GetAttribute("providerName"));
				user.ProviderName = !String.IsNullOrEmpty(providerName) ? providerName : "self";

				var password = ProcessValue(reader.GetAttribute("password"));
				user.Password = !String.IsNullOrEmpty(password) ? password : BXStringUtility.GenerateUniqueString(10) + '+';
			}

			Set(reader.GetAttribute("firstname"), x => user.FirstName = x);
			Set(reader.GetAttribute("lastname"), x => user.LastName = x);
			Set(reader.GetAttribute("secondname"), x => user.SecondName = x);
			Set(reader.GetAttribute("displayname"), x => user.DisplayName = x);
			Set(reader.GetAttribute("birthdaydate"), x => user.BirthdayDate = x);
			SetEnum<BXUserGender>(reader.GetAttribute("gender"), x => user.Gender = x);
			Set(reader.GetAttribute("isapproved"), x => user.IsApproved = x);
			Set(reader.GetAttribute("siteid"), x => user.SiteId = x);

			var image = reader.GetAttribute("image");
			if (!String.IsNullOrEmpty(image))
			{
				var imagePath = Path.Combine(rootDir, image);

				BXFile file = null;
				string xmlId = "user/" + image;

				//Если обновляем пользователя, то может быть файл уже есть?
				if (!user.IsNew)
				{
					BXFileCollection files = BXFile.GetList(
						new BXFilter(
							new BXFilterItem(BXFile.Fields.Description, BXSqlFilterOperators.Equal, xmlId)),
							null
					);

					if (files.Count > 0)
						file = files[0];
				}

				if (file == null && File.Exists(imagePath))
				{
					string fileName = Path.GetFileName(imagePath);
					string contentType;

					if (!BXFileInfo.TryGetImageMimeType(fileName, out contentType))
						contentType = "image";

					using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
					{
						file = new BXFile(fs, fileName, "user", "main", xmlId, contentType);
						file.Save();
					}
				}

				if (file != null)
					user.ImageId = file.Id;

			}


			List<BXRole> userRoles = new List<BXRole>();
			if (!reader.IsEmptyElement && reader.Read())
				while (true)
				{
					var name = reader.Name;
					if (reader.NodeType == XmlNodeType.Element)
						switch (name)
						{
							case "role":
								if (reader.IsEmptyElement)
									break;
								var roleName = ProcessValue(reader.ReadElementContentAsString());
								BXRole role;
								if (!String.IsNullOrEmpty(roleName) && (role = BXRoleManager.GetByName(roleName)) != null)
									userRoles.Add(role);
								continue;

							case "comment":
								if (reader.IsEmptyElement)
									break;
								user.Comment = ProcessValue(reader.ReadElementContentAsString());
								continue;

							case "properties":
								using (var propertiesReader = reader.ReadSubtree())
									ReadProperties(propertiesReader, user.CustomFields, user.CustomPublicValues);
								break;
						}
					if (!reader.Read())
						break;
				}

			user.Save();

			//Добавляем пользователя в роль
			foreach (var userRole in userRoles)
			{
				if (!user.GetRoles().Exists(delegate(BXRole role) { return role.RoleName == userRole.RoleName; }))
					user.AddToRole(userRole.RoleName, null, null);
			}

		}

		private BXUser GetUser(string login, bool create)
		{
			BXUser user = null;
			if (!string.IsNullOrEmpty(login))
			{
				var users = BXUser.GetList(
					new BXFilter(new BXFilterItem(BXUser.Fields.UserName, BXSqlFilterOperators.Equal, login)),
					null,
					new BXSelectAdd(BXUser.Fields.CustomFields.DefaultFields),
					null,
					BXTextEncoder.EmptyTextEncoder
				);
				if (users.Count > 0)
					user = users[0];
			}

			if (user == null && create)
			{
				user = new BXUser(BXTextEncoder.EmptyTextEncoder);
				if (!string.IsNullOrEmpty(login))
					user.UserName = login;
			}

			return user;
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
			if (string.Equals(entity, "user", StringComparison.OrdinalIgnoreCase))
				entity = "USER";
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
								string typeName = ProcessValue(reader.GetAttribute("type"));
								string value = ProcessValue(reader.ReadElementContentAsString());
								if (!String.IsNullOrEmpty(name))
								{
									if (!String.IsNullOrEmpty(typeName))
									{
										Type type = null;
										try
										{
											type = Type.GetType(typeName);
											fld.Settings[name] = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
											break;
										}
										catch 
										{
										};
									}
									else
										fld.Settings[name] = value;
								}
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
			//if (string.Equals(key, "SiteId", StringComparison.OrdinalIgnoreCase))
			//    return siteId;
			//if (string.Equals(key, "SiteName", StringComparison.OrdinalIgnoreCase))
			//    return site.SiteName;
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