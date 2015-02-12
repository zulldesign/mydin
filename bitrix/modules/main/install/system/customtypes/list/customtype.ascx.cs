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

[Obsolete("use 'enumeration' custom type")]
public partial class BXCustomTypeList : BXCustomType
{
    public override string Title
    {
        get { return GetMessage("Title"); }
    }

    public override string Description
    {
        get { return GetMessage("Description"); }
    }

	public override SqlDbType DbType
	{
		get
		{
			return SqlDbType.NText;
		}
	}


    public override string TypeName
    {
        get { return "Bitrix.System.List"; }
    }

    public override Control Settings
    {
        get { return LoadControl("settings.ascx"); }
    }

    public override Control Edit
    {
        get { return LoadControl("edit.ascx"); }
    }

    public override Control AdvancedSettings
    {
        get { return LoadControl("advanced_settings.ascx"); }
    }

	public override BXCustomTypePublicView CreatePublicView()
	{
		return new PublicView();
	}

    public override Control GetFilter(BXCustomField field)
    {
        BXTextBoxFilter filter = new BXTextBoxFilter();
        filter.Key = field.Name;

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
		private string[] propertyValues;
		private bool isValidated = true;

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
			listSize = settings.ContainsKey("ListSize") ? (int)settings["ListSize"] : 5;
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
				propertyValues = new string[property.Values.Count];
				int index = 0;
				foreach (object value in property.Values)
					propertyValues[index++] = (string)value;
			}
			else if (property.Value is string)
				propertyValues = new string[] { (string)property.Value };
		}

		public override string Render(string formFieldName, string uniqueID)
		{
			if (field == null)
				return String.Empty;

			string dropDownStart = "<select name=\"{0}\" id=\"{1}\" class=\"bx-custom-field custom-field-list-dropdown\" size=\"{2}\"{3}>";
			string dropDownOption = "<option value=\"{0}\"{1}>{0}</option>";
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

				IList sourceValues = (IList)propertyValues;

				foreach (BXCustomFieldEnum listValue in listValues)
				{
					bool selected = property == null && sourceValues == null ? listValue.Default : sourceValues != null && sourceValues.Contains(listValue.Value);

					result.AppendFormat(
						dropDownOption,
						HttpUtility.HtmlEncode(listValue.Value),
						selected ? " selected=\"selected\"" : String.Empty
					);
				}

				result.Append(dropDownEnd);
			}
			else
			{
				int counter = 1;
				IList sourceValues = (IList)propertyValues;
				foreach (BXCustomFieldEnum listValue in listValues)
				{
					bool isChecked = property == null && sourceValues == null ? listValue.Default : sourceValues != null && sourceValues.Contains(listValue.Value);
					string htmlValue = HttpUtility.HtmlEncode(listValue.Value);

					result.AppendFormat(
						field.Multiple ? checkBoxPattern : radioPattern,
						HttpUtility.HtmlEncode(formFieldName),
						htmlValue,
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

			isValidated = true;
			propertyValues = HttpContext.Current.Request.Form.GetValues(formFieldName);
			if (propertyValues == null || propertyValues.Length < 1)
			{
				if (required)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeSet")), field.EditFormLabel));
					isValidated = false;
				}
			}
			else
			{
				if (!field.Multiple)
					propertyValues = new string[] { propertyValues[0] };

				foreach (string value in propertyValues)
				{
					if (!IsContains(value))
					{
						errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "IncorrectValue")), field.EditFormLabel));
						isValidated = false;
						break;
					}
				}
			}

			return isValidated;
		}

		private bool IsContains(string value)
		{
			if (value == null || listValues == null)
				return false;

			foreach (BXCustomFieldEnum listValue in listValues)
			{
				if (value.Equals(listValue.Value, StringComparison.InvariantCulture))
					return true;
			}

			return false;
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

			var defaults = listValues.FindAll(x => x.Default).ConvertAll(x => x.Value).ToArray();
			if (defaults.Length == 0)
				return;
			
			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, defaults);
			storage[field.Name] = property;
		}
	}
	class PublicView : BXCustomTypePublicView
	{
		public override string GetHtml(string uniqueId, string separatorHtml)
		{
			if (property == null || property.Values.Count == 0)
				return string.Empty;

			StringBuilder s = new StringBuilder();
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);
				s.Append(HttpUtility.HtmlEncode(value.ToString()));
			}
			return s.ToString();
		}
		public override void Render(string uniqueId, string separatorHtml, HtmlTextWriter writer)
		{
			if (property == null || property.Values.Count == 0)
				return;

			bool separate = false;
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				if (separate)
					writer.Write(separatorHtml);
				else
					separate = true;

				writer.WriteEncodedText(value.ToString());
			}
		}
		public override bool IsEmpty
		{
			get
			{
				if (property == null || property.Values.Count == 0)
					return true;
				foreach (object val in property.Values)
					if (val != null)
						return false;
				return true;
			}
		}
	}
}
