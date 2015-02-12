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
using Bitrix;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using Bitrix.DataTypes;
using Bitrix.IBlock;
using Bitrix.Services.Text;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.UI;

public partial class BXCustomTypeIBlockSectionReference : BXCustomType
{
	public override string Title
	{
		get
		{
			return GetMessage("Title");
		}
	}

	public override string Description
	{
		get
		{
			return GetMessage("Description");
		}
	}

	public override SqlDbType DbType
	{
		get
		{
			return SqlDbType.Int;
		}
	}


	public override string TypeName
	{
		get
		{
			return "Bitrix.IBlock.SectionId";
		}
	}

	public override Control Settings
	{
		get
		{
			return LoadControl("settings.ascx");
		}
	}

	public override Control Edit
	{
		get
		{
			return LoadControl("edit.ascx");
		}
	}

	public override BXCustomTypePublicView CreatePublicView()
	{
		return new PublicView();
	}

	public override Control GetFilter(BXCustomField field)
	{
		var iblockId = field.Settings.GetInt("IBlockId");

		if (iblockId == 0)
		{
			var iblocks = BXIBlock.GetList(
				null,
				null,
				new BXSelect(BXIBlock.Fields.ID),
				new BXQueryParams(new BXPagingOptions(0, 1))
			);
			if (iblocks.Count > 0)
				iblockId = iblocks[0].Id;
		}
		
		var sections = BXIBlockSection.GetList(
			new BXFilter(
				new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
				new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId)
			),
			new BXOrderBy(new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)),
			new BXSelect(
				BXIBlockSection.Fields.ID, 
				BXIBlockSection.Fields.Name, 
				BXIBlockSection.Fields.DepthLevel
			),
			null,
			BXTextEncoder.EmptyTextEncoder
		);


		var filter = new BXDropDownFilter();
		filter.Key = "@" + field.OwnerEntityId + ":" + field.Name;
		filter.ValueType = BXAdminFilterValueType.Integer;
		filter.Values.Add(new ListItem(GetMessageRaw("Kernel.Any"), ""));
		filter.Values.AddRange(sections.ConvertAll(x => new ListItem(BXStringUtility.Clone(". ", x.DepthLevel) + x.Name, x.Id.ToString())).ToArray());
		return filter;
	}

	public override Control AdvancedSettings
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public override bool IsClonable
	{
		get
		{
			return false;
		}
	}

	public override BXCustomTypePublicEdit CreatePublicEditor()
	{
		return new PublicEditor(this);
	}


	class PublicEditor : BXCustomTypePublicEdit
	{
		private BXCustomType type;
		private BXCustomField field;
		private BXCustomProperty property;

		private List<int> propertyValues = new List<int>();
		private int iblockId;
		private bool multiple;
		private int textBoxSize;
		private bool required;
		private string[] postValues;
		private OrderedDictionary sectionTree = new OrderedDictionary();

		public PublicEditor(BXCustomType type)
		{
			this.type = type;
		}

		private class SectionTreeItem
		{
			public int Id;
			public string Name;
			public int DepthLevel;
			public bool HasChildren = false;
		}

		public override void Init(BXCustomField currentField)
		{
			field = currentField;
			if (field == null)
				return;

			BXParamsBag<object> settings = new BXParamsBag<object>(currentField.Settings);
			required = field.Mandatory;
			multiple = field.Multiple;
			iblockId = settings.ContainsKey("IBlockId") ? (int)settings["IBlockId"] : 0;
			textBoxSize = settings.ContainsKey("TextBoxSize") ? (int)settings["TextBoxSize"] : 5;

			BXIBlockSectionCollection sections = BXIBlockSection.GetList(
				new BXFilter(
					new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
					new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId)
				),
				new BXOrderBy(
					new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)
				),
				new BXSelect(
					BXIBlockSection.Fields.ID, BXIBlockSection.Fields.Name, BXIBlockSection.Fields.DepthLevel
				),
				null,
				BXTextEncoder.HtmlTextEncoder
			);

			int currentIndex = 0;
			int previousDepthLevel = 1;
			foreach (BXIBlockSection section in sections)
			{
				if (currentIndex > 0)
					((SectionTreeItem)sectionTree[currentIndex - 1]).HasChildren = section.DepthLevel > previousDepthLevel;
				previousDepthLevel = section.DepthLevel;

				SectionTreeItem treeItem = new SectionTreeItem();
				treeItem.Id = section.Id;
				treeItem.DepthLevel = section.DepthLevel;
				treeItem.Name = section.Name;
				sectionTree.Insert(currentIndex++, section.Id, treeItem);
			}
		}

		public override void Load(BXCustomProperty currentProperty)
		{
			if (field == null)
				return;

			property = currentProperty;

			if (property == null || property.Values == null || property.Values.Count < 1)
				return;

			if (field.Multiple)
			{
				propertyValues = new List<int>();
				foreach (object value in property.Values)
				{
					if (value is int)
						propertyValues.Add((int)value);
				}
			}
			else if (property.Value is int)
			{
				propertyValues = new List<int>(1);
				propertyValues.Add((int)property.Value);
			}
		}

		public override string Render(string formFieldName, string uniqueID)
		{
			if (field == null)
				return String.Empty;

			string dropDownStart = "<select name=\"{0}\" id=\"{1}\" class=\"bx-custom-field custom-field-ibsection\" size=\"{2}\"{3}>";
			string dropDownOption = "<option value=\"{0}\"{1}>{2}</option>";
			string dropDownEnd = "</select>";

			StringBuilder result = new StringBuilder(String.Empty);
			result.AppendFormat(
				dropDownStart,
				HttpUtility.HtmlEncode(formFieldName),
				HttpUtility.HtmlEncode(uniqueID),
				textBoxSize > sectionTree.Count ? sectionTree.Count : textBoxSize,
				multiple ? " multiple=\"multiple\"" : String.Empty
			);

			if (!multiple)
				result.AppendFormat(dropDownOption, 0, String.Empty, HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueNotSet")));

			for (int i = 0; i < sectionTree.Count; i++)
			{
				SectionTreeItem section = (SectionTreeItem)sectionTree[i];
				StringBuilder sectionName = new StringBuilder();
				for (int index = 0; index < section.DepthLevel; index++)
					sectionName.Append(". ");
				sectionName.Append(section.Name);

				result.AppendFormat(
					dropDownOption,
					section.Id,
					propertyValues.Contains(section.Id) ? " selected=\"selected\"" : String.Empty,
					sectionName
				);
			}

			result.Append(dropDownEnd);
			return result.ToString();
		}

		protected override void Save(string formFieldName, BXCustomPropertyCollection storage)
		{
			if (field == null)
				return;

			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, propertyValues);
			storage[field.Name] = property;
		}

		protected override bool Validate(string formFieldName, ICollection<string> errors)
		{
			if (field == null)
				return false;

			bool success = true;
			postValues = HttpContext.Current.Request.Form.GetValues(formFieldName);
			if (postValues == null || postValues.Length < 1)
			{
				propertyValues.Clear();
				if (required)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeSet")), field.EditFormLabel));
					success = false;
				}
			}
			else
			{
				if (!field.Multiple)
					postValues = new string[] { postValues[0] };

				propertyValues = new List<int>(postValues.Length);
				foreach (string value in postValues)
				{
					int sectionId;
					if (!int.TryParse(value, out sectionId) || sectionId < 1 || !sectionTree.Contains(sectionId))
						continue;

					propertyValues.Add(sectionId);
				}

				if (required && propertyValues.Count < 1)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeSet")), field.EditFormLabel));
					success = false;
				}
			}

			return success;
		}
	}
	class PublicView : BXCustomTypePublicView
	{

		private Dictionary<int, BXIBlockSection> sections;
		private Dictionary<int, BXIBlockSection> BuildSections()
		{
			Dictionary<int, BXIBlockSection> sections = new Dictionary<int, BXIBlockSection>();
			BXFilter filter = new BXFilter();

			if (property.IsMultiple)
				filter.Add(new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.In, property.Values));
			else
				filter.Add(new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.Equal, property.Value));

			if (field != null)
			{
				int iblockId = field.Settings.GetInt("IBlockId");
				if (iblockId > 0)
					filter.Add(new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId));
			}

			BXIBlockSectionCollection sectionCollection = BXIBlockSection.GetList(
				filter,
				new BXOrderBy(new BXOrderByPair(BXIBlockSection.Fields.ID, BXOrderByDirection.Asc)),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder
			);

			foreach (BXIBlockSection s in sectionCollection)
				sections[s.Id] = s;

			return sections;
		}

		public override string GetHtml(string uniqueId, string separatorHtml)
		{
			if (property == null || property.Values.Count == 0)
				return string.Empty;

			sections = sections ?? BuildSections();

			StringBuilder s = new StringBuilder();
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				BXIBlockSection sc;
				int sectionId;
				if (!int.TryParse(value.ToString(), out sectionId) || !sections.TryGetValue(sectionId, out sc))
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);

				s.Append(HttpUtility.HtmlEncode(sc.Name ?? ""));

			}
			return s.ToString();
		}
		public override void Render(string uniqueId, string separatorHtml, HtmlTextWriter writer)
		{
			if (property == null || property.Values.Count == 0)
				return;

			sections = sections ?? BuildSections();

			bool separate = false;
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				BXIBlockSection sc;
				if (!sections.TryGetValue((int)value, out sc))
					continue;


				if (separate)
					writer.Write(separatorHtml);
				else
					separate = true;

				writer.WriteEncodedText(sc.Name);

			}
		}
		public override bool IsEmpty
		{
			get
			{
				if (property == null || property.Values.Count == 0)
					return true;

				sections = sections ?? BuildSections();

				foreach (object value in property.Values)
				{
					if (value == null)
						continue;

					if (sections.ContainsKey((int)value))
						return false;
				}

				return true;
			}
		}
	}
}