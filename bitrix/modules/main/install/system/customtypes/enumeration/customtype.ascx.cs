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
using Bitrix.UI;
using System.Text;
using System.Collections.Generic;
using Bitrix.DataTypes;
using Bitrix.Services.Text;
using Bitrix.Services;
using Bitrix.DataLayer;

public partial class BXCustomTypeEnumeration : BXCustomType
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
			return "Bitrix.System.Enumeration";
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

	public override Control AdvancedSettings
	{
		get
		{
			return LoadControl("advanced_settings.ascx");
		}
	}

    public override bool TryGetDefaultValue(BXCustomField field, out object value)
    {
        if (field != null)
        {
            BXCustomFieldEnumCollection values = BXCustomFieldEnum.GetList(
                new BXFilter(
					new BXFilterItem(BXCustomFieldEnum.Fields.FieldType, BXSqlFilterOperators.Equal, field.FieldType),
                    new BXFilterItem(BXCustomFieldEnum.Fields.FieldId, BXSqlFilterOperators.Equal, field.Id),
                    new BXFilterItem(BXCustomFieldEnum.Fields.Default, BXSqlFilterOperators.Equal, true)
                    ),
                null,
                new BXSelect(
                    BXSelectFieldPreparationMode.Normal,
                    BXCustomFieldEnum.Fields.Id
                    ),
                null,
                BXTextEncoder.EmptyTextEncoder
            );

            if (values.Count > 0)
            {
                value = field.Multiple ? (object)values.ConvertAll(x => x.Id) : (object)values[0].Id;
                return true;
            }
        }
        value = 0;
        return false;
    }

	public override BXCustomTypePublicView CreatePublicView()
	{
		return new PublicView();
	}

	public override Control GetFilter(BXCustomField field)
	{
		var filter = new BXDropDownFilter();
		filter.Key = "@" + field.OwnerEntityId + ":" + field.Name;
		filter.ValueType = BXAdminFilterValueType.Integer;
		foreach (var e in BXCustomFieldEnum.GetList(field.Id, field.FieldType, BXTextEncoder.EmptyTextEncoder))
			filter.Values.Add(new ListItem(e.Value, e.Id.ToString()));
		return filter;
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
		private ViewMode viewMode;
		private bool required;
		private int listSize;
		private BXCustomFieldEnumCollection listValues;
		private int[] propertyValues;
		private bool isValidated = true;
		bool isPosted;

		public PublicEditor(BXCustomType type)
		{
			this.type = type;
		}

		private enum ViewMode
		{
			List,
			Flags
		}

		public override void Init(BXCustomField currentField)
		{
			field = currentField;
			if (field == null)
				return;

			BXParamsBag<object> settings = new BXParamsBag<object>(field.Settings);
			listValues = BXCustomFieldEnum.GetList(field.Id, field.FieldType, BXTextEncoder.EmptyTextEncoder);
			required = field.Mandatory;
			viewMode = settings.ContainsKey("ViewMode") ? ViewMode.List : ViewMode.Flags;
			listSize = settings.GetInt("ListSize", 5);
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
				propertyValues = new int[property.Values.Count];
				int index = 0;
				foreach (int value in property.Values)
					propertyValues[index++] = value;
			}
			else if (property.Value is int)
				propertyValues = new int[] { (int)property.Value };
		}

		public override string Render(string formFieldName, string uniqueID)
		{
			if (field == null)
				return String.Empty;

			string dropDownStart = "<select name=\"{0}\" id=\"{1}\" class=\"bx-custom-field custom-field-list-dropdown\" size=\"{2}\"{3}>";
			string dropDownOption = "<option value=\"{2}\"{1}>{0}</option>";
			string dropDownEnd = "</select>";

			string radioPattern = "<input type=\"radio\" class=\"bx-custom-field custom-field-list-radio\" name=\"{0}\" value=\"{1}\" id=\"{2}{3}\" {4}/><label for=\"{2}{3}\">&nbsp;{5}</label><br />";
			string checkBoxPattern = "<input type=\"checkbox\" class=\"bx-custom-field custom-field-list-checkbox\" name=\"{0}\" value=\"{1}\" id=\"{2}{3}\" {4}/><label for=\"{2}{3}\">&nbsp;{5}</label><br />";

			StringBuilder result = new StringBuilder(String.Empty);
			if (viewMode == ViewMode.List)
			{
				result.AppendFormat(
					dropDownStart,
					HttpUtility.HtmlEncode(formFieldName),
					HttpUtility.HtmlEncode(formFieldName),
					listSize < listValues.Count ? listSize : listValues.Count,
					field.Multiple ? " multiple=\"multiple\"" : String.Empty
				);

				foreach (BXCustomFieldEnum listValue in listValues)
				{
					bool selected = property == null && propertyValues == null && !isPosted ? listValue.Default : propertyValues != null && Array.IndexOf(propertyValues, listValue.Id) != -1;

					result.AppendFormat(
						dropDownOption,
						HttpUtility.HtmlEncode(listValue.Value),
						selected ? " selected=\"selected\"" : String.Empty,
						listValue.Id
					);
				}

				result.Append(dropDownEnd);
			}
			else
			{
				int counter = 1;
				foreach (BXCustomFieldEnum listValue in listValues)
				{
					bool isChecked = property == null && propertyValues == null && !isPosted ? listValue.Default : propertyValues != null && Array.IndexOf(propertyValues, listValue.Id) != -1;
					string htmlValue = HttpUtility.HtmlEncode(listValue.Value);

					result.AppendFormat(
						field.Multiple ? checkBoxPattern : radioPattern,
						HttpUtility.HtmlEncode(formFieldName),
						listValue.Id,
						HttpUtility.HtmlEncode(formFieldName),
						counter++,
						isChecked ? "checked=\"checked\"" : String.Empty,
						htmlValue
					);
				}
			}

			return result.ToString();
		}

		protected override bool Validate(string formFieldName, ICollection<string> errors)
		{
			if (field == null)
				return false;

			isPosted = true;
			isValidated = true;

			propertyValues = GetFromPost(formFieldName);
			if (propertyValues == null || propertyValues.Length < 1)
			{
				if (required)
				{
                    if(errors != null)
					    errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeSet")), field.EditFormLabel));
					isValidated = false;
				}
			}
			else
			{
				if (!field.Multiple)
					Array.Resize(ref propertyValues, 1);

				foreach (int value in propertyValues)
				{
					if (!IsContains(value))
					{
                        if(errors != null)
						    errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "IncorrectValue")), field.EditFormLabel));
						isValidated = false;
						break;
					}
				}
			}

			return isValidated;
		}

		private int[] GetFromPost(string formFieldName)
		{
			string[] postValues = HttpContext.Current.Request.Form.GetValues(formFieldName);
			if (postValues == null)
				return null;

			List<int> values = new List<int>(postValues.Length);
			foreach (string v in postValues)
			{
				int id;
				if (int.TryParse(v, out id))
					values.Add(id);
			}
			return values.ToArray();
		}

		private bool IsContains(int value)
		{
			if (listValues == null)
				return false;

			return listValues.Exists(delegate(BXCustomFieldEnum listValue)
			{
				return listValue.Id == value;
			});
		}

		protected override void Save(string formFieldName, BXCustomPropertyCollection storage)
		{
			if (field == null)
				return;

			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, propertyValues);
			storage[field.Name] = property;
		}

		public override void SaveDefault(BXCustomPropertyCollection storage)
		{
			if (field == null)
				return;

			var defaults = listValues.FindAll(x => x.Default).ConvertAll(x => x.Id).ToArray();
			if (defaults.Length == 0)
				return;
			
			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, defaults);
			storage[field.Name] = property;
		}
	}
	class PublicView : BXCustomTypePublicView
	{
		Dictionary<int, BXCustomFieldEnum> enums;

		private Dictionary<int, BXCustomFieldEnum> BuildEnums()
		{
			Dictionary<int, BXCustomFieldEnum> enums = new Dictionary<int, BXCustomFieldEnum>();
			BXCustomFieldEnumCollection enumCollection = BXCustomFieldEnum.GetList(field.Id, field.FieldType, BXTextEncoder.EmptyTextEncoder);

			foreach (BXCustomFieldEnum e in enumCollection)
				enums[e.Id] = e;

			return enums;
		}

		public override string GetHtml(string uniqueId, string separatorHtml)
		{
			if (property == null || property.Values.Count == 0 || field == null)
				return string.Empty;

			enums = enums ?? BuildEnums();

			StringBuilder s = new StringBuilder();
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				BXCustomFieldEnum e;
				if (!enums.TryGetValue((int)value, out e))
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);
				s.Append(HttpUtility.HtmlEncode(e.Value ?? ""));
			}
			return s.ToString();
		}
		public override void Render(string uniqueId, string separatorHtml, HtmlTextWriter writer)
		{
			if (property == null || property.Values.Count == 0 || field == null)
				return;

			enums = enums ?? BuildEnums();

			bool separate = false;
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				BXCustomFieldEnum e;
				if (!enums.TryGetValue((int)value, out e))
					continue;
			
				if (separate)
					writer.Write(separatorHtml);
				else
					separate = true;

				writer.WriteEncodedText(e.Value);
			}
		}
		public override bool IsEmpty
		{
			get
			{
				if (property == null || property.Values.Count == 0 || field == null)
					return true;

				enums = enums ?? BuildEnums();

				foreach (object value in property.Values)
				{
					if (value == null)
						continue;

					if (enums.ContainsKey((int) value))
						return false;
				}
				return true;
			}
		}
	}
}
