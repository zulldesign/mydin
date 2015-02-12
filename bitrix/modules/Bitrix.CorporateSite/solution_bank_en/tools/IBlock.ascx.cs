using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.IBlock;
using Bitrix.IO;
using System.Xml;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using System.IO;
using Bitrix.Services.Text;
using Bitrix.Security;
using System.Text.RegularExpressions;
using Bitrix.DataTypes;
  
namespace Bitrix.Install.Internal
{
	public partial class IBlockXmlImporterDummy : UserControl
	{
	}

	public class IBlockXmlImporter
	{
		private const string iblockTag = "iblock";
		private const string iblockSectionTag = "sect";
		private const string iblockElementNameTag = "el";
		private const string customPropertyTag = "prop";
		private const string iblockTransTag = "trans";
		private const string storageTag = "store";
		private const string propdefTag = "propdef";
		private const string detailTextTag = "detailtext";
		private const string previewTextTag = "previewtext";
		private const string descriptionTag = "desc";
		private const string iblockTypeTag = "iblocktype";
		private const string captionsTag = "captions";
		private const string permissionsTag = "permissions";
		private const string customPropsTag = "customprops";
		private string dirToStoreFilesTemplate;//= "~/files/iblock_#iblockId#/#fileName#/";

		Dictionary<int, Dictionary<string, int>> customFieldEnums = new Dictionary<int, Dictionary<string, int>>();

		string[] iblockOperations;

		DateProcessor dateProcessor;
		string siteId;
		string siteEntityId;
		BXSite site;

		public IBlockXmlImporter(string siteId, string siteEntityId, string dirToStoreFilesTemplate)
		{
			dateProcessor = new DateProcessor();
			this.siteId = siteId;
			this.siteEntityId = siteEntityId;
			this.dirToStoreFilesTemplate = dirToStoreFilesTemplate;
			this.site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
		}


		private BXParamsBag<string> replace;
		public BXParamsBag<string> Replace
		{
			get
			{
				return replace ?? (replace = new BXParamsBag<string>());
			}
		}
	
		/// <summary>
		/// список операций инфоблоков
		/// </summary>

		protected string[] IBlockOperations
		{
			get
			{
				return iblockOperations ?? (iblockOperations = BXIBlock.Operations.GetNames());
			}
		}

		/// <summary>
		/// основной метод импорта
		/// </summary>
		/// <param name="filePath">путь к файлу импорта</param>
		/// <returns></returns>
		public bool LoadInfoBlocks(string filePath)
		{

			BXIBlock iblock = null;
			BXIBlockElement elm = null;
			BXIBlockSection sect = null;
			BXIBlockType iblockType;
			string optName;


			Dictionary<string, List<string>> ElmsInSections = new Dictionary<string, List<string>>();

			Dictionary<string, int> sectionIds = new Dictionary<string, int>();
			string fileName = String.Empty;
			string dir = string.Empty;

			dir = BXPath.ToPhysicalPath(BXPath.GetDirectory(filePath));

			using (XmlTextReader reader = new XmlTextReader(new FileStream(filePath, FileMode.Open)))
			{
				while (reader.Read())
				{
					if (reader.NodeType.Equals(XmlNodeType.Element))
						switch (reader.Name)
						{

							case iblockTypeTag:
								iblockType = ImportIBlockType(reader);
								break;

							case iblockTag:
								iblock = ImportIblock(reader, dir);
								break;

							case captionsTag:
								ImportIblockCaptions(reader, iblock);
								break;

							case iblockSectionTag:
								if (iblock != null)
									ImportSection(reader, iblock, sectionIds, dir, out sect);
								break;

							case permissionsTag:
								if (iblock != null)
									ImportIblockPermissions(reader, iblock);
								break;

							#region Element
							case iblockElementNameTag:
								ImportIblockElement(reader, iblock, sectionIds, dir, out elm);

								break;
							#endregion

							#region Storage
							case storageTag:
								string item = reader.GetAttribute("item");
								optName = reader.GetAttribute("optname");
								switch (item)
								{
									case iblockTag:

										if (!String.IsNullOrEmpty(optName))
										{
											if (iblock != null)
												BXOptionManager.SetOption<int>(siteEntityId, optName, iblock.Id, siteId);

										}
										break;
									case iblockSectionTag:
										if (!String.IsNullOrEmpty(optName))
										{
											if (sect != null)
												BXOptionManager.SetOption<int>(siteEntityId, optName, sect.Id, siteId);

										}
										break;
								}
								break;
							#endregion

							#region Custom Properties

							case propdefTag:
								ImportCustomPropertyDefinition(reader, iblock);
								break;

							#endregion
						}
				}
			}

			return true;
		}

		#region Import IblockType
		/// <summary>
		/// импорт типов инфоблоков
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="iblockType"></param>
		BXIBlockType ImportIBlockType(XmlTextReader reader)
		{
			string langAttr;
			string xmlId = ProcessValue(reader.GetAttribute("xmlid"));
			BXIBlockType iblockType = null;
			if (!String.IsNullOrEmpty(xmlId))
			{
				BXIBlockTypeCollection types = BXIBlockType.GetList(new BXFilter(new BXFilterItem(BXIBlockType.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)), null);
				if (types.Count > 0)
					iblockType = types[0];
			}

			if (iblockType == null)
				iblockType = new BXIBlockType("", false, 100);

			iblockType.XmlId = xmlId;
			Set(reader.GetAttribute("name"), x => iblockType.Name = x);
			Set(reader.GetAttribute("sort"), x => iblockType.Sort = x);
			Set(reader.GetAttribute("havesections"), x => iblockType.HaveSections = x);

			using (XmlReader nestedReader = reader.ReadSubtree())
			{
				while (nestedReader.Read())
				{
					if (nestedReader.NodeType.Equals(XmlNodeType.Element))
					{
						switch (nestedReader.Name)
						{
							case "loc":
								langAttr = nestedReader.GetAttribute("lang");
								string loc = nestedReader.ReadElementContentAsString();
								BXIBlockType.BXInfoBlockTypeLang curLang = iblockType.Translations.Find(x => String.Equals(x.LanguageId, langAttr, StringComparison.OrdinalIgnoreCase));
								if (curLang != null)
									iblockType.Translations.Remove(curLang);
								iblockType.Translations.Add(new BXIBlockType.BXInfoBlockTypeLang(langAttr, loc, "раздел", "элемент"));
								break;
						}
					}
				}
			}

			iblockType.Save();
			return iblockType;
		}
		#endregion

		#region Import Iblock
		/// <summary>
		/// импорт инфоблоков
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="iblock"></param>
		BXIBlock ImportIblock(XmlTextReader reader, string rootDir)
		{
			var xmlId = ProcessValue(reader.GetAttribute("xmlid"));
			BXIBlock iblock = null;
			if (!String.IsNullOrEmpty(xmlId))
			{
				BXIBlockCollection col = BXIBlock.GetList(new BXFilter(new BXFilterItem(BXIBlock.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)), null);
				if (col.Count > 0)
					iblock = col[0];
			}

			if (iblock == null)
			{
				iblock = new BXIBlock();
				iblock.Name = "";
				iblock.Sort = 100;
				iblock.TypeId = 0;
				iblock.XmlId = xmlId;
			}
			Set(reader.GetAttribute("name"), x => iblock.Name = x);
			Set(reader.GetAttribute("sort"), x => iblock.Sort = x);

			string typeId = ProcessValue(reader.GetAttribute("typeid"));
			BXIBlockType type = null;
			if (!String.IsNullOrEmpty(typeId))
			{
				BXIBlockTypeCollection col = BXIBlockType.GetList(new BXFilter(new BXFilterItem(BXIBlockType.Fields.XmlId, BXSqlFilterOperators.Equal, typeId)), null);
				if (col.Count > 0)
					type = col[0];
			}
			if (!String.IsNullOrEmpty(typeId))
				iblock.TypeId = type != null ? type.Id : 0;

			Set(reader.GetAttribute("code"), x => iblock.Code = x);
			Set(reader.GetAttribute("description"), x => { iblock.Description = x; iblock.DescriptionType = BXTextType.Text; });
			Set(reader.GetAttribute("indexcontent"), x => { iblock.IndexContent = x; });
		
			if (!iblock.Sites.Exists(x => string.Equals(x.TextEncoder.Decode(x.SiteId), siteId, StringComparison.OrdinalIgnoreCase)))
				iblock.Sites.Add(new BXIBlock.BXInfoBlockSite(siteId));

			string imgFilePath = reader.GetAttribute("imagepath");

			if (!String.IsNullOrEmpty(imgFilePath))
			{
				try
				{
					imgFilePath = BXPath.Combine(rootDir, imgFilePath);
					using (FileStream fs = new FileStream(imgFilePath, FileMode.Open))
					{
						string dir = String.Empty, fileName = string.Empty;
						BXPath.BreakPath(imgFilePath, ref dir, ref fileName);
						BXFile file = new BXFile(fs, fileName, dirToStoreFilesTemplate.Replace("#iblockId#", iblock.Id.ToString()).Replace("#fileName#", fileName));
						file.OwnerModuleId = "iblock";
						file.ContentType = "image";
						file.Save();
						iblock.ImageId = file.Id;
					}
				}
				catch 
				{
				}
			}
			
			iblock.Save();
			return iblock;
		}
		#endregion

		#region IBlock captions
		/// <summary>
		/// импорт подписей к инфоблокам
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="iblock"></param>
		void ImportIblockCaptions(XmlTextReader reader, BXIBlock iblock)
		{
			if (iblock == null)
			{
				reader.Skip();
				return;
			}
			string type, name;
			using (XmlReader nestedReader = reader.ReadSubtree())
			{
				while (nestedReader.Read())
				{
					if (nestedReader.NodeType.Equals(XmlNodeType.Element))
					{
						switch (nestedReader.Name)
						{
							case "caption":
								type = nestedReader.GetAttribute("type");
								name = ProcessValue(nestedReader.ReadElementContentAsString());
								if (type != null && name != null)
									switch (type)
									{
										case "addelement":
											iblock.CaptionsInfo.AddElement = name;
											break;
										case "addsection":
											iblock.CaptionsInfo.AddSection = name;
											break;
										case "changeelement":
											iblock.CaptionsInfo.ChangeElement = name;
											break;
										case "changesection":
											iblock.CaptionsInfo.ChangeSection = name;
											break;
										case "deleteelement":
											iblock.CaptionsInfo.DeleteElement = name;
											break;
										case "deletesection":
											iblock.CaptionsInfo.DeleteSection = name;
											break;
										case "elementlist":
											iblock.CaptionsInfo.ElementList = name;
											break;
										case "elementname":
											iblock.CaptionsInfo.ElementName = name;
											break;
										case "elementsname":
											iblock.CaptionsInfo.ElementsName = name;
											break;
										case "modifyingelement":
											iblock.CaptionsInfo.ModifyingElement = name;
											break;
										case "modifyingsection":
											iblock.CaptionsInfo.ModifyingSection = name;
											break;
										case "newelement":
											iblock.CaptionsInfo.NewElement = name;
											break;
										case "newsection":
											iblock.CaptionsInfo.NewSection = name;
											break;
										case "sectionlist":
											iblock.CaptionsInfo.SectionList = name;
											break;
										case "sectionname":
											iblock.CaptionsInfo.SectionName = name;
											break;
										case "sectionsname":
											iblock.CaptionsInfo.SectionsName = name;
											break;
									}
								break;
						}
					}
				}
			}
			iblock.Save();
		}

		#endregion

		#region Import Section
		/// <summary>
		/// импорт разделов инфоблоков
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="iblock"></param>
		/// <param name="sectionIds"></param>
		/// <param name="sect"></param>
		void ImportSection(XmlTextReader reader, BXIBlock iblock, Dictionary<string, int> sectionIds, string rootDir, out BXIBlockSection sect)
		{
			string fileName = string.Empty, dir = String.Empty, typeAttr, optName;
			int i;
			sect = null;
			string xmlId = ProcessValue(reader.GetAttribute("xmlid"));
			if (iblock == null)
			{
				reader.Skip();
				return;
			}
			if (xmlId != null)
			{
				BXIBlockSectionCollection col = BXIBlockSection.GetList(new BXFilter(new BXFilterItem(BXIBlockSection.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId),
																					new BXFilterItem(BXIBlockSection.Fields.IBlock.ID,BXSqlFilterOperators.Equal,iblock.Id)), null);
				if (col.Count > 0)
					sect = col[0];
				else
					sect = new BXIBlockSection();
			}
			else
				sect = new BXIBlockSection();
			sect.IBlockId = iblock.Id;
			sect.Name = ProcessValue(reader.GetAttribute("name"));
			sect.Description = ProcessValue(reader.GetAttribute("description"));
			sect.Code = ProcessValue(reader.GetAttribute("code"));
			sect.DescriptionType = BXTextType.Text;
			sect.SectionId = int.TryParse(reader.GetAttribute("sectionid"), out i) ? i : 0;
			sect.Sort = int.TryParse(reader.GetAttribute("sort"), out i) ? i : 100;
			sect.XmlId = ProcessValue(xmlId);

			string flPath = reader.GetAttribute("imagepath");
			try
			{
				if (!String.IsNullOrEmpty(flPath))
				{
					flPath = BXPath.Combine(rootDir, flPath);
					using (FileStream fs = new FileStream(flPath, FileMode.Open, FileAccess.Read))
					{
						BXPath.BreakPath(flPath, ref dir, ref fileName);
						BXFile file = new BXFile(fs, fileName, dirToStoreFilesTemplate.Replace("#iblockId#", iblock.Id.ToString()).Replace("#fileName#", fileName));
						file.OwnerModuleId = "iblock";
						string contentType;
						if (!BXFileInfo.TryGetImageMimeType(fileName, out contentType))
							contentType = "image";
						file.ContentType = contentType;
						file.Save();
						sect.ImageId = file.Id;
					}
				}
			}
			catch { }

			// Custom properties

			foreach (BXCustomField field in sect.CustomFields)
			{
				string attr = ProcessValue(reader.GetAttribute(field.Name.ToLowerInvariant()));
				if (!String.IsNullOrEmpty(attr))
				{
					if (field.CustomTypeId == "Bitrix.System.Enumeration")
					{
						int id = GetCustomFieldEnumId(field.Id, attr);
						if (id > 0)
							sect.CustomPublicValues.Set(field.Name, id);
					}
					else
						sect.CustomPublicValues.Set(field.Name, attr);
				}
			}

			//description
			bool saved = true;

			try
			{
				sect.Save();
				sectionIds.Add(sect.XmlId, sect.Id);
			}
			catch
			{
				saved = false;
			}
			if (saved)
			{
				using (XmlReader nestedReader = reader.ReadSubtree())
				{
					while (nestedReader.Read())
					{
						if (nestedReader.NodeType.Equals(XmlNodeType.Element))
						{
							switch (nestedReader.Name)
							{
								case descriptionTag:
									typeAttr = nestedReader.GetAttribute("type");
									sect.DescriptionType = (typeAttr != null && typeAttr.Equals("Html", StringComparison.OrdinalIgnoreCase)) ? BXTextType.Html : BXTextType.Text;
									sect.Description = ProcessValue(nestedReader.ReadElementContentAsString());
									break;
								case storageTag:
									optName = ProcessValue(nestedReader.GetAttribute("optname"));
									string storedItem = ProcessValue(nestedReader.GetAttribute("item"));
									if (!String.IsNullOrEmpty(optName))
									{
										if (storedItem != null && storedItem.Equals("code", StringComparison.OrdinalIgnoreCase))
											BXOptionManager.SetOptionString(siteEntityId, optName, sect.Code, siteId);
										else
											BXOptionManager.SetOption<int>(siteEntityId, optName, sect.Id, siteId);
									}
									break;
							}
						}
					}
				}
			}

			sect.Save();

		}
		#endregion

		#region Import IBlock Element
		/// <summary>
		/// импорт элементов инфоблоков
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="iblock"></param>
		/// <param name="sectionIds"></param>
		/// <param name="elm"></param>
		void ImportIblockElement(XmlTextReader reader, BXIBlock iblock, Dictionary<string, int> sectionIds, string rootDir, out BXIBlockElement elm)
		{
			string fileName = String.Empty, dir = string.Empty, typeAttr;

			DateTime date;

			string xmlId = ProcessValue(reader.GetAttribute("xmlid"));
			if (!String.IsNullOrEmpty(xmlId))
			{
				BXIBlockElementCollection col = BXIBlockElement.GetList(new BXFilter(
													new BXFilterItem(BXIBlockElement.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)), null);
				if (col.Count > 0)
					elm = col[0];
				else
					elm = new BXIBlockElement();
			}
			else
				elm = new BXIBlockElement();

			elm.Name = ProcessValue(reader.GetAttribute("name"));
			if (dateProcessor.Process(reader.GetAttribute("activefromdate"), out date))
				elm.ActiveFromDate = date;
			if (dateProcessor.Process(reader.GetAttribute("activetodate"), out date))
				elm.ActiveToDate = date;

			elm.Tags = ProcessValue(reader.GetAttribute("tags"));
			elm.XmlId = ProcessValue(reader.GetAttribute("xmlid"));

			elm.IBlockId = iblock.Id;
			string sectList = reader.GetAttribute("sections");
			if (!String.IsNullOrEmpty(sectList))
			{
				var sectionsList = BXStringUtility.CsvToList(ProcessValue(reader.GetAttribute("sections")));
				elm.Sections.Clear();
				foreach (string s in sectionsList)
					elm.Sections.Add(sectionIds[s]);
			}

			string detailImagePath = reader.GetAttribute("detailimagepath");
			try
			{
				if (!String.IsNullOrEmpty(detailImagePath))
				{
					detailImagePath = BXPath.Combine(rootDir, detailImagePath);
					using (FileStream fs = new FileStream(detailImagePath, FileMode.Open))
					{
						BXPath.BreakPath(detailImagePath, ref dir, ref fileName);
						BXFile file = new BXFile(fs, fileName, dirToStoreFilesTemplate.Replace("#iblockId#", iblock.Id.ToString()).Replace("#fileName#", fileName));
						file.OwnerModuleId = "iblock";
						file.ContentType = "image";
						file.Save();
						elm.DetailImageId = file.Id;
					}
				}
			}
			catch { }

			detailImagePath = reader.GetAttribute("previewimagepath");
			try
			{
				if (!String.IsNullOrEmpty(detailImagePath))
				{
					detailImagePath = BXPath.Combine(rootDir, detailImagePath);
					using (FileStream fs = new FileStream(detailImagePath, FileMode.Open))
					{
						BXPath.BreakPath(detailImagePath, ref dir, ref fileName);
						BXFile file = new BXFile(fs, fileName, dirToStoreFilesTemplate.Replace("#iblockId#", iblock.Id.ToString()).Replace("#fileName#", fileName));
						file.OwnerModuleId = "iblock";
						file.ContentType = "image";
						file.Save();
						elm.PreviewImageId = file.Id;
					}
				}
			}
			catch { }

			//texts

			// Custom properties

			foreach (BXCustomField field in elm.CustomFields)
			{
				string attr = ProcessValue(reader.GetAttribute(field.Name.ToLowerInvariant()));
				if (!String.IsNullOrEmpty(attr))
				{
					if (field.CustomTypeId == "Bitrix.System.Enumeration")
					{
						int id = GetCustomFieldEnumId(field.Id, attr);
						if (id > 0)
							elm.CustomPublicValues.Set(field.Name, id);
					}
					else
						elm.CustomPublicValues.Set(field.Name, attr);
				}
			}

			using (XmlReader nestedReader = reader.ReadSubtree())
			{
				while (nestedReader.Read())
				{
					if (nestedReader.NodeType.Equals(XmlNodeType.Element))
					{
						switch (nestedReader.Name)
						{
							case customPropsTag:
								ImportCustomFieldsFromTags("iblockElement", elm, reader);
								break;
							case previewTextTag:
								typeAttr = nestedReader.GetAttribute("type");
								elm.PreviewTextType = (typeAttr != null && typeAttr.Equals("Html", StringComparison.OrdinalIgnoreCase)) ? BXTextType.Html : BXTextType.Text;
								elm.PreviewText = nestedReader.ReadElementContentAsString();
								break;
							case detailTextTag:
								typeAttr = nestedReader.GetAttribute("type");
								elm.DetailTextType = (typeAttr != null && typeAttr.Equals("Html", StringComparison.OrdinalIgnoreCase)) ? BXTextType.Html : BXTextType.Text;
								elm.DetailText = nestedReader.ReadElementContentAsString();
								break;
						}
					}
				}
			}
			
			elm.Save();

		}
		#endregion

		#region Import IBlock Permissions
		/// <summary>
		/// импорт прав доступа к инфоблоку
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="iblock"></param>
		void ImportIblockPermissions(XmlTextReader reader, BXIBlock iblock)
		{
			if (iblock == null)
			{
				reader.Skip();
				return;
			}
			string roleName, permission;
			using (XmlReader nestedReader = reader.ReadSubtree())
			{
				while (nestedReader.Read())
				{
					if (nestedReader.NodeType.Equals(XmlNodeType.Element))
					{
						switch (nestedReader.Name)
						{
							case "permission":
								roleName = ProcessValue(nestedReader.GetAttribute("rolename"));
								permission = ProcessValue(nestedReader.ReadElementContentAsString());
								if (!String.IsNullOrEmpty(roleName) && !String.IsNullOrEmpty(permission) && IBlockOperations.Contains<string>(permission))
								{
									BXRole role = BXRoleManager.GetByName(roleName);
									if (role != null)
									{
										BXRoleCollection roles = BXRoleManager.GetAllRolesForOperation(permission, "iblock", iblock.Id.ToString());
										if (!roles.Exists(delegate(BXRole input) { return input.RoleName == roleName; }))
											BXRoleManager.AddRolesToOperations(new string[] { roleName }, new string[] { permission }, "iblock", iblock.Id.ToString());
									}
								}
								break;
						}
					}
				}
			}
		}

		#endregion

		#region Import Custom property definition
		/// <summary>
		/// импорт набираемых описаний набираемых свойств
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="iblock"></param>
		void ImportCustomPropertyDefinition(XmlTextReader reader, BXIBlock iblock)
		{
			string idAttr, typeAttr;
			string target = reader.GetAttribute("for");
			if (iblock == null)
			{
				reader.Skip();
				return;
			}
			if (!String.IsNullOrEmpty(target))
			{
				idAttr = reader.GetAttribute("id");
				typeAttr = reader.GetAttribute("type");
				string listColumnLabel = reader.GetAttribute("listcolumnlabel");

				BXCustomField fld = null;
				try
				{
					string customFieldKey = target == "section" ? BXIBlockSection.GetCustomFieldsKey(iblock.Id) : BXIBlock.GetCustomFieldsKey(iblock.Id);
					fld = GetCustomField(customFieldKey, idAttr, typeAttr);
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
									loc.EditFormLabel = nestedReader.ReadElementContentAsString();

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

		}
		/// <summary>
		/// импорт значений свойства-списка
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="fld"></param>
		void ImportCustomFieldEnum(XmlTextReader reader, BXCustomField fld)
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

								int sort = (int.TryParse(nestedReader.GetAttribute("sort"), out i) ? i : 100);
								bool isDefault = (nestedReader.GetAttribute("default") ?? "").Equals("true", StringComparison.OrdinalIgnoreCase);
								string xmlId = nestedReader.GetAttribute("xmlid");
								string name = nestedReader.ReadElementContentAsString();
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

		void ImportCustomFieldSettings(XmlTextReader reader, BXCustomField fld)
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
								int i;
								string name = ProcessValue(reader.GetAttribute("name"));
								string type = ProcessValue(reader.GetAttribute("type"));
								if (String.IsNullOrEmpty(type))
									type = "string";
								string value = ProcessValue(reader.ReadElementContentAsString());
								if (!String.IsNullOrEmpty(name))
								{
									switch (type)
									{
										case "int":
											if (Int32.TryParse(value, out i))
											{
												fld.Settings[name] = i;
											}
											break;
										case "string":
											fld.Settings[name] = value;
											break;
									}

								}
								break;
						}

					}

				}
				fld.Save();
			}
		}

		void ImportCustomFieldsFromTags(string entityType, BXEntity entity, XmlTextReader reader)
		{
			if ( entityType != "iblockElement" && entityType != "iblockSection")
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
						switch (reader.Name)
						{
							case "prop":
								string nameAttr = ProcessValue(reader.GetAttribute("name"));
								string value = nestedReader.ReadElementContentAsString();
								if (!String.IsNullOrEmpty(nameAttr))
								{
									BXCustomField field = entity.CustomFields.Find(x => x.Name.Equals(nameAttr,StringComparison.OrdinalIgnoreCase));
									if (field!=null)
									{
										
										if (field.CustomTypeId == "Bitrix.System.Enumeration")
										{
											int id = GetCustomFieldEnumId(field.Id, value);
											if (id > 0)
												entity.CustomPublicValues.Set(field.Name, id);
										}
										else
											entity.CustomPublicValues.Set(field.Name, value);
									}
								}
								break;
						}
						
					}

				}
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
			return "";
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
				if (!Values.Contains<string>(value))
					return false;

				string operation = workArray[1];
				if (!Operations.Contains<string>(operation))
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
