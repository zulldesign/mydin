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

public partial class BXCustomTypeDate : BXCustomType
{
	public override string Title
	{
		get { return GetMessage("Title"); }
	}

	public override string Description
	{
		get { return GetMessage("Title"); }
	}

	public override SqlDbType DbType
	{
		get
		{
			return SqlDbType.DateTime;
		}
	}

	public override string TypeName
	{
		get { return "Bitrix.System.DateTime"; }
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
		get { throw new Exception("The method or operation is not implemented."); }
	}

	public override BXCustomTypePublicView CreatePublicView()
	{
		return new PublicView();
	}

	public override Control GetFilter(BXCustomField field)
	{
		var filter = new BXTimeIntervalFilter();
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

		private List<DateTime> propertyValues;

		//Settings
		private DateTime? defaultValue = null;
		private bool required;
		private bool showTime;

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

			BXCalendarHelper.RegisterScriptFiles();

			BXParamsBag<object> settings = new BXParamsBag<object>(currentField.Settings);
			required = field.Mandatory;
			showTime = settings.ContainsKey("showTime") ? (bool)settings["showTime"] : true;

			if (settings.ContainsKey("default"))
				defaultValue = (DateTime)settings["default"];
			else if (settings.ContainsKey("current"))
				defaultValue = showTime ? DateTime.Now : DateTime.Now.Date;
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
				propertyValues = new List<DateTime>();
				foreach (object value in property.Values)
				{
					if (value is DateTime)
						propertyValues.Add((DateTime)value);
				}
			}
			else if (property.Value is DateTime)
			{
				propertyValues = new List<DateTime>(1);
				propertyValues.Add((DateTime)property.Value);
			}
		}

		public override string Render(string formFieldName, string uniqueID)
		{
			if (field == null)
				return String.Empty;

			if (!isValidated && postValues != null && postValues.Length > 0)
				return GetRender<string>(formFieldName, uniqueID, postValues, "{0:G}");
			else
			{
				if (propertyValues == null || propertyValues.Count < 1)
					return RenderDefaultControl(formFieldName, uniqueID);
				else
					return GetRender<DateTime>(formFieldName, uniqueID, propertyValues, showTime ? "{0:G}" : "{0:d}");
			}
		}

		private string GetRender<T>(string formFieldName, string uniqueID, IList<T> values, string format)
		{
			string result;

			if (field.Multiple)
			{
				List<string> propertyRender = new List<string>(values.Count);
				foreach (T value in values)
				{
					string stringValue = value.ToString();
					if (!BXStringUtility.IsNullOrTrimEmpty(stringValue))
						propertyRender.Add(String.Format("{0}{1}", RenderControl(formFieldName, String.Format(format, value)), "&nbsp;{0}"));
				}

				result = BXCustomTypeHelper.GetMultipleView(propertyRender, RenderControl(formFieldName, String.Empty) + "&nbsp;{0}", uniqueID);
			}
			else
				result = RenderControl(formFieldName, String.Format(format, values[0]));

			return result;
		}

		private string RenderControl(string formFieldName, string propertyValue)
		{
			string textBoxPattern = "<input name=\"{0}\" value=\"{1}\" size=\"{2}\" class=\"bx-custom-field custom-field-date\" />{3}";

			return String.Format(
				textBoxPattern,
				HttpUtility.HtmlEncode(formFieldName),
				HttpUtility.HtmlEncode(propertyValue),
				20,
				BXCalendarHelper.GetMarkupByInputElement("this.previousSibling", showTime)
			);
		}

		private string RenderDefaultControl(string formFieldName, string uniqueID)
		{
			string propertyValue = property == null && defaultValue.HasValue ? (showTime ? defaultValue.Value.ToString() : String.Format("{0:d}", defaultValue.Value)) : String.Empty;
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
				propertyValues = new List<DateTime>();
				foreach (string value in postValues)
				{
					if (BXStringUtility.IsNullOrTrimEmpty(value))
						continue;

					DateTime? dateTimeValue = null;
					if (ValidateValue(value, errors, ref dateTimeValue))
					{
						isAnyValueSet = true;
						if (dateTimeValue.HasValue)
							propertyValues.Add(dateTimeValue.Value);
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
				DateTime? dateTimeValue = null;
				if (ValidateValue(postValues[0], errors, ref dateTimeValue))
				{
					if (dateTimeValue.HasValue)
					{
						propertyValues = new List<DateTime>(1);
						propertyValues.Add(dateTimeValue.Value);
					}
					else
						propertyValues = null;
				}
				else
					isValidated = false;
			}

			return isValidated;
		}

		private bool ValidateValue(string value, ICollection<string> errors, ref DateTime? dateTimeValue)
		{
			bool success = true;
			if (BXStringUtility.IsNullOrTrimEmpty(value))
			{
				if (required)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "FieldMustBeFilled")), field.EditFormLabel));
					success = false;
				}
			}
			else
			{
				DateTime number;
				if (!DateTime.TryParse(value, out number))
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeDate")), field.EditFormLabel));
					success = false;
				}
				else
					dateTimeValue = showTime ? number : number.Date;
			}

			return success;
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

			bool showTime = true;
			if (field != null)
			{
				showTime = field.Settings.GetBool("showTime", showTime);
			}

			StringBuilder s = new StringBuilder();
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);
				s.Append(HttpUtility.HtmlEncode(((DateTime)value).ToString(showTime ? "G" : "d")));
			}
			return s.ToString();
		}
		public override void Render(string uniqueId, string separatorHtml, HtmlTextWriter writer)
		{
			if (property == null || property.Values.Count == 0)
				return;

			bool showTime = true;
			if (field != null)
			{
				showTime = field.Settings.GetBool("showTime", showTime);
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

				writer.WriteEncodedText(((DateTime)value).ToString(showTime ? "G" : "d"));
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
