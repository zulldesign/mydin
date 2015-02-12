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

public partial class BXCustomTypeInt : BXCustomType
{
	protected void Page_Load(object sender, EventArgs e)
	{

	}

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
			return "Bitrix.System.Int";
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
		if (field.ShowInFilter == BXCustomFieldFilterVisibility.CompleteMatch)
		{
			var filter = new BXTextBoxFilter();
			filter.Key = "@" + field.OwnerEntityId + ":" + field.Name;
			filter.ValueType = BXAdminFilterValueType.Integer;
			return filter;
		}
		else
		{
			var filter = new BXBetweenFilter();
			filter.Key = "@" + field.OwnerEntityId + ":" + field.Name;
			filter.ValueType = BXAdminFilterValueType.Integer;
			return filter;
		}
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
		private List<int> propertyValues;

		//Settings
		private int? defaultValue = null;
		private int? minValue = null;
		private int? maxValue = null;

		private bool required;
		private int textBoxSize;

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
			textBoxSize = settings.ContainsKey("TextBoxSize") ? (int)settings["TextBoxSize"] : 20;

			if (settings.ContainsKey("DefaultValue"))
				defaultValue =  (int)settings["DefaultValue"];

			if (settings.ContainsKey("MinValue"))
				minValue = (int)settings["MinValue"];

			if (settings.ContainsKey("MaxValue"))
				maxValue = (int)settings["MaxValue"];
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

			if (!isValidated && postValues != null && postValues.Length > 0)
				return GetRender<string>(formFieldName, uniqueID, postValues);	 
			else
			{
				if (propertyValues == null || propertyValues.Count < 1)
					return RenderDefaultControl(formFieldName, uniqueID);
				else
					return GetRender<int>(formFieldName, uniqueID, propertyValues);		
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
						propertyRender.Add(String.Format("{0}{1}", RenderControl(formFieldName, value.ToString()), "&nbsp;{0}"));
				}

				result = BXCustomTypeHelper.GetMultipleView(propertyRender, RenderControl(formFieldName, String.Empty) + "&nbsp;{0}", uniqueID);
			}
			else
				result = RenderControl(formFieldName, values[0].ToString());

			return result;
		}

		private string RenderControl(string formFieldName, string propertyValue)
		{
			string textBoxPattern = "<input name=\"{0}\" value=\"{1}\" size=\"{2}\" class=\"bx-custom-field custom-field-int\" />";

			return String.Format(
				textBoxPattern,
				HttpUtility.HtmlEncode(formFieldName),
				HttpUtility.HtmlEncode(propertyValue),
				textBoxSize
			);
		}

		private string RenderDefaultControl(string formFieldName, string uniqueID)
		{
			string propertyValue = property == null && defaultValue.HasValue ? defaultValue.Value.ToString() : String.Empty;
			if (field.Multiple)
			{
				List<string> propertyRender = new List<string>(1) ;
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
				propertyValues = new List<int>();
				foreach (string value in postValues)
				{
					if (BXStringUtility.IsNullOrTrimEmpty(value))
						continue;

					int? integerValue = null;
					if (ValidateValue(value, errors, ref integerValue))
					{
						isAnyValueSet = true;
						if (integerValue.HasValue)
							propertyValues.Add(integerValue.Value);
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
				int? integerValue = null;
				if (ValidateValue(postValues[0], errors, ref integerValue))
				{
					if (integerValue.HasValue)
					{
						propertyValues = new List<int>(1);
						propertyValues.Add(integerValue.Value);
					}
					else
						propertyValues = null;
				}
				else
					isValidated = false;
			}

			return isValidated;
		}

		private bool ValidateValue(string value, ICollection<string> errors, ref int? integerValue)
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

			int number;
			if (!int.TryParse(value, out number))
			{
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeInteger")), field.EditFormLabel));
				return false;
			}

			integerValue = number;
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

			StringBuilder s = new StringBuilder();
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);
				s.Append(value);
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

				writer.Write(value);
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
