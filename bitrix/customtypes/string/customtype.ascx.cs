using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Bitrix;
using Bitrix.DataTypes;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;

public partial class BXCustomTypeString : BXCustomType
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
			return SqlDbType.NVarChar;
		}
	}

	public override string TypeName
	{
		get
		{
			return "Bitrix.System.Text";
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
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public override BXCustomTypePublicView CreatePublicView()
	{
		return new PublicView();
	}

	public override Control GetFilter(BXCustomField field)
	{
		BXTextBoxStringFilter filter = new BXTextBoxStringFilter();
		filter.Key = "@" + field.OwnerEntityId + ":" + field.Name;
		return filter;
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
		private string[] propertyValues;

		//Settings
		private string defaultValue;
		private bool required;

		private TextBoxMode textMode;
		private int rowsCount;
		private int textBoxSize;
		private int minLength;
		private int maxLength;
		private Regex regex;

		public PublicEditor(BXCustomType type)
		{
			this.type = type;
		}

		public override void Init(BXCustomField currentField)
		{
			field = currentField;
			if (field == null)
				return;

			BXParamsBag<object> settings = new BXParamsBag<object>(currentField.Settings);
			defaultValue = settings.ContainsKey("DefaultValue") ? (string)settings["DefaultValue"] : String.Empty;
			required = field.Mandatory;

			rowsCount = settings.ContainsKey("RowsCount") ? settings.GetInt("RowsCount") : 1;
			textBoxSize = settings.ContainsKey("TextBoxSize") ? settings.GetInt("TextBoxSize") : 20;
			textMode = (rowsCount > 1) ? TextBoxMode.MultiLine : TextBoxMode.SingleLine;

			minLength = settings.ContainsKey("MinLength") ? settings.GetInt("MinLength") : 0;
			maxLength = settings.ContainsKey("MaxLength") ? settings.GetInt("MaxLength") : 0;

			if (settings.ContainsKey("ValidationRegex") && !String.IsNullOrEmpty((string)settings["ValidationRegex"]))
			{
				try
				{
					regex = new Regex((string)settings["ValidationRegex"]);
				}
				catch (Exception)
				{
				}
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

			string result;
			if (field.Multiple)
			{
				if (propertyValues == null || propertyValues.Length < 1)
					propertyValues = new string[] { defaultValue };

				List<string> propertyRender = new List<string>(propertyValues.Length);
				foreach (string value in propertyValues)
				{
					if (BXStringUtility.IsNullOrTrimEmpty(value))
						continue;

					propertyRender.Add(
						String.Format(
							"{0}{1}",
							GetRender(formFieldName, value),
							"&nbsp;{0}"
						)
					);
				}

				result = BXCustomTypeHelper.GetMultipleView(propertyRender, GetRender(formFieldName, String.Empty) + "&nbsp;{0}", uniqueID);
			}
			else
			{
				string value = propertyValues == null || propertyValues.Length < 1 ? defaultValue : propertyValues[0];
				result = GetRender(formFieldName, value);
			}

			return result;
		}

		private string GetRender(string formFieldName, string propertyValue)
		{
			string textBoxPattern = "<textarea cols=\"{0}\" rows=\"{1}\" name=\"{2}\" class=\"bx-custom-field custom-field-string\">{3}</textarea>";
			string textAreaPattern = "<input size=\"{0}\" name=\"{1}\" class=\"bx-custom-field custom-field-string\" value=\"{2}\" />";
			string result;

			if (textMode == TextBoxMode.MultiLine)
				result = String.Format(textBoxPattern, textBoxSize, rowsCount, HttpUtility.HtmlEncode(formFieldName), HttpUtility.HtmlEncode(propertyValue));
			else
				result = String.Format(textAreaPattern, textBoxSize, HttpUtility.HtmlEncode(formFieldName), HttpUtility.HtmlEncode(propertyValue));

			return result;
		}

		protected override bool Validate(string formFieldName, ICollection<string> errors)
		{
			if (field == null)
				return false;

			propertyValues = HttpContext.Current.Request.Form.GetValues(formFieldName);
			if (propertyValues == null || propertyValues.Length < 1)
			{
				if (required)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "FieldMustBeFilled")), field.EditFormLabel));
					return false;
				}
				return true;
			}

			if (field.Multiple)
			{
				bool isAnyValueSet = !required;
				foreach (string value in propertyValues)
				{
					if (BXStringUtility.IsNullOrTrimEmpty(value))
						continue;

					if (!ValidateValue(value, errors))
						return false;
					else
						isAnyValueSet = true;
				}

				if (!isAnyValueSet)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "FieldMustBeFilled")), field.EditFormLabel));
					return false;
				}
				return true;
			}
			else
			{
				propertyValues = new string[] { propertyValues[0] };
				return ValidateValue(propertyValues[0], errors);
			}
		}

		private bool ValidateValue(string value, ICollection<string> errors)
		{
			if (BXStringUtility.IsNullOrTrimEmpty(value))
			{
				if (required)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "FieldMustBeFilled")), field.EditFormLabel));
					return false;
				}

				return true;
			}
			else if (minLength > 0 && value.Length < minLength)
			{
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "LengthMustBeNotLess")), field.EditFormLabel, minLength));
				return false;
			}
			else if (maxLength > 0 && value.Length > maxLength)
			{
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "LengthMustBeNotGreater")), field.EditFormLabel, maxLength));
				return false;
			}
			else if (regex != null && !regex.IsMatch(value))
			{
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "Error.RegexInvalid")), field.EditFormLabel));
				return false;
			}

			return true;
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
			if (field == null || !string.IsNullOrEmpty(defaultValue))
				return;

			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, defaultValue);
			storage[field.Name] = property;
		}
	}
    private enum ViewMode
    {
        Text = 1,
        Html = 2,
        Pattern = 3
    }
	class PublicView : BXCustomTypePublicView
	{
        private ViewMode? mode = null;
        protected ViewMode Mode
        {
            get 
            {
                if (mode.HasValue)
                    return mode.Value;

                string textType = ((field != null ? field.Settings.GetString("TextType") : null) ?? "").ToUpperInvariant();
                if (string.Equals(textType, "PATTERN", StringComparison.InvariantCulture))
                    mode = ViewMode.Pattern;
                else if (string.Equals(textType, "HTML", StringComparison.InvariantCulture))
                    mode = ViewMode.Html;
                else
                    mode = ViewMode.Text;

                return mode.Value;
            }
        }
        private string pattern = null;
        protected string Pattern
        {
            get { return pattern ?? (pattern = field.Settings.GetString("pattern", string.Empty)); }
        }
        #region BXCustomTypePublicView
        public override string GetHtml(string uniqueId, string separatorHtml)
		{
			if (property == null || property.Values.Count == 0)
				return string.Empty;

			StringBuilder s = new StringBuilder();
            string curStr = null;
			foreach (object value in property.Values)
			{
                if(string.IsNullOrEmpty((curStr = value != null ? value.ToString() : null)))
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);

                if (Mode == ViewMode.Text)
                    s.Append(BXStringUtility.HtmlEncodeEx(curStr));
                else if (Mode == ViewMode.Html)
                    s.Append(curStr);
                else if (Mode == ViewMode.Pattern)
                {
                    if (Pattern.Length > 0)
                    {
                        string r = Pattern.Replace("#value#", curStr)
                            .Replace("#Value#", curStr)
                            .Replace("#HtmlValue#", HttpUtility.HtmlEncode(curStr))
                            .Replace("#UrlValue#", HttpUtility.UrlEncode(curStr))
                            .Replace("#UrlHtmlValue#", HttpUtility.HtmlEncode(HttpUtility.UrlEncode(curStr)));
                        s.Append(r);
                    }
                }
			}
			return s.ToString();
		}

		public override bool IsEmpty
		{
			get
			{
				if (property == null || property.Values.Count == 0)
					return true;
				foreach (object val in property.Values)
					if (val != null && !BXStringUtility.IsNullOrTrimEmpty(val.ToString()))
						return false;
				return true;
			}
        }
        #endregion
    }
}
