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
using Bitrix.DataTypes;
using Bitrix.Services.Text;
using Bitrix.Services;
using Bitrix.UI;

public partial class BXCustomTypeDouble: BXCustomType
{
	public override string Title
	{
		get
		{
			return GetMessageRaw("Title");
		}
	}

	public override string Description
	{
		get
		{
			return GetMessageRaw("Description");
		}
	}

	public override SqlDbType DbType
	{
		get
		{
			return SqlDbType.Float;
		}
	}


	public override string TypeName
	{
		get
		{
			return "Bitrix.System.Double";
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
		var filter = new BXBetweenFilter();
		filter.Key = "@" + field.OwnerEntityId + ":" + field.Name;
		filter.ValueType = BXAdminFilterValueType.Float;
		return filter;
	}

	public override Control AdvancedSettings
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
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
		private List<double> propertyValues;

		//Settings
		private double? defaultValue = null;
		private double? minValue = null;
		private double? maxValue = null;

		private bool required;
		private int textBoxSize;
		private int precision;

		private bool isValidated = true;
		private string[] postValues;

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

			required = field.Mandatory;
			textBoxSize = settings.ContainsKey("TextBoxSize") ? settings.GetInt("TextBoxSize") : 20;

			if (settings.ContainsKey("DefaultValue"))
				defaultValue = settings.GetDouble("DefaultValue");

			if (settings.ContainsKey("MinValue"))
				minValue = settings.GetDouble("MinValue");

			if (settings.ContainsKey("MaxValue"))
				maxValue = settings.GetDouble("MaxValue");

			precision = settings.ContainsKey("Precision") ? settings.GetInt("Precision") : 4;
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
				propertyValues = new List<double>();
				foreach (object value in property.Values)
				{
					if (value is double)
						propertyValues.Add((double)value);
				}
			}
			else if (property.Value is double)
			{
				propertyValues = new List<double>(1);
				propertyValues.Add((double)property.Value);
			}
		}

		public override string Render(string formFieldName, string uniqueID)
		{
			if (field == null)
				return String.Empty;

			if (!isValidated && postValues != null && postValues.Length > 0)
				return GetRender<string>(formFieldName, uniqueID, postValues);
			else
			{
				if (propertyValues == null || propertyValues.Count < 1)
					return RenderDefaultControl(formFieldName, uniqueID);
				else
					return GetRender<double>(formFieldName, uniqueID, propertyValues);
			}
		}

		private string GetRender<T>(string formFieldName, string uniqueID, IList<T> values)
		{
			string result;

			if (field.Multiple)
			{
				List<string> propertyRender = new List<string>(values.Count);
				foreach (T value in values)
				{
					string stringValue = value.ToString();
					if (!BXStringUtility.IsNullOrTrimEmpty(stringValue))
					{
						propertyRender.Add(
							String.Format(
								"{0}{1}", 
								RenderControl(formFieldName, (value is double ? String.Format("{0:F" + precision + "}", value) : value.ToString())), 
								"&nbsp;{0}"
							)
						);
					}
				}

				result = BXCustomTypeHelper.GetMultipleView(propertyRender, RenderControl(formFieldName, String.Empty) + "&nbsp;{0}", uniqueID);
			}
			else
				result = RenderControl(formFieldName, (values[0] is double ? String.Format("{0:F" + precision + "}", values[0]) : values[0].ToString()));

			return result;
		}

		private string RenderControl(string formFieldName, string propertyValue)
		{
			string textBoxPattern = "<input name=\"{0}\" value=\"{1}\" size=\"{2}\" class=\"bx-custom-field custom-field-double\" />";

			return String.Format(
				textBoxPattern,
				HttpUtility.HtmlEncode(formFieldName),
				HttpUtility.HtmlEncode(propertyValue),
				textBoxSize
			);
		}

		private string RenderDefaultControl(string formFieldName, string uniqueID)
		{
			string propertyValue = property == null && defaultValue.HasValue ? String.Format("{0:F" + precision + "}", defaultValue.Value) : String.Empty;
			if (field.Multiple)
			{
				List<string> propertyRender = new List<string>(1);
				propertyRender.Add(String.Format("{0}{1}", RenderControl(formFieldName, propertyValue), "&nbsp;{0}"));
				return BXCustomTypeHelper.GetMultipleView(propertyRender, RenderControl(formFieldName, String.Empty) + "&nbsp;{0}", uniqueID);
			}
			else
				return RenderControl(formFieldName, propertyValue);
		}

		protected override bool Validate(string formFieldName, ICollection<string> errors)
		{
			if (field == null)
				return false;

			isValidated = true;
			postValues = HttpContext.Current.Request.Form.GetValues(formFieldName);
			if (postValues == null || postValues.Length < 1)
			{
				if (required)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "FieldMustBeFilled")), field.EditFormLabel));
					isValidated = false;
				}
			}
			else if (field.Multiple)
			{
				bool isAnyValueSet = !required;
				propertyValues = new List<double>();
				foreach (string value in postValues)
				{
					if (BXStringUtility.IsNullOrTrimEmpty(value))
						continue;

					double? doubleValue = null;
					if (ValidateValue(value, errors, ref doubleValue))
					{
						isAnyValueSet = true;
						if (doubleValue.HasValue)
							propertyValues.Add(doubleValue.Value);
					}
					else
					{
						isValidated = false;
						break;
					}
				}

				if (isValidated && !isAnyValueSet)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "FieldMustBeFilled")), field.EditFormLabel));
					isValidated = false;
				}
			}
			else
			{
				double? doubleValue = null;
				if (ValidateValue(postValues[0], errors, ref doubleValue))
				{
					if (doubleValue.HasValue)
					{
						propertyValues = new List<double>(1);
						propertyValues.Add(doubleValue.Value);
					}
					else
						propertyValues = null;
				}
				else
					isValidated = false;
			}

			return isValidated;
		}

		private bool ValidateValue(string value, ICollection<string> errors, ref double? doubleValue)
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

			double number;
			if (!double.TryParse(value, out number))
			{
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeDouble")), field.EditFormLabel));
				return false;
			}

			doubleValue = number;
			if (minValue.HasValue && number < minValue)
			{
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeGreaterOrEqual")), field.EditFormLabel, minValue));
				return false;
			}
			else if (maxValue.HasValue && number > maxValue)
			{
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeLessOrEqual")), field.EditFormLabel, maxValue));
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
			if (field == null || defaultValue == null)
				return;

			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, defaultValue.Value);
			storage[field.Name] = property;
		}
	}
	class PublicView : BXCustomTypePublicView
	{
		public override string GetHtml(string uniqueId, string separatorHtml)
		{
			if (property == null || property.Values.Count == 0)
				return string.Empty;

			int precision = 4;
			if (field != null)
			{
				precision = field.Settings.GetInt("Precision", precision);
			}

			StringBuilder s = new StringBuilder();
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);
				s.Append(HttpUtility.HtmlEncode(((double)value).ToString("F" + precision)));
			}
			return s.ToString();
		}
		public override void Render(string uniqueId, string separatorHtml, HtmlTextWriter writer)
		{
			if (property == null || property.Values.Count == 0)
				return;

			int precision = 4;
			if (field != null)
			{
				precision = field.Settings.GetInt("Precision", precision);
			}

			bool separate = false;
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				if (separate)
					writer.Write(separatorHtml);
				else
					separate = true;

				writer.WriteEncodedText(((double)value).ToString("F" + precision));
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
