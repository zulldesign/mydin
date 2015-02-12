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
using System.Data.SqlTypes;
using Bitrix.UI;

public partial class BXCustomTypeGuid : BXCustomType
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
			return SqlDbType.UniqueIdentifier;
		}
	}


	public override string TypeName
	{
		get
		{
			return "Bitrix.System.Guid";
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
		return null;
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
		private const int GuidCharacters = 38;
		private const string TextBoxPattern = "<input name=\"{0}\" value=\"{1}\" size=\"{2}\" class=\"bx-custom-field custom-field-guid\" />";

		private BXCustomType type;
		private BXCustomField field;
		private BXCustomProperty property;
		private List<Guid> propertyValues;

	
		// Settings
		private bool required;
		private bool generateDefault;
		private Guid? defaultValue;

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

			generateDefault = currentField.Settings.GetBool("GenerateDefault");
			required = field.Mandatory;
		}

		public override void Load(BXCustomProperty currentProperty)
		{
			if (field == null)
				return;
			
			property = currentProperty;

			if (property == null || property.Values == null || property.Values.Count < 1)
				return;

			propertyValues = new List<Guid>(1);
			foreach (object value in property.Values)
			{
				if (value is SqlGuid)
					propertyValues.Add(((SqlGuid)value).Value);
				else if (value is Guid)
					propertyValues.Add((Guid)value);

				if (!field.Multiple)
					break;
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
					return GetRender<Guid>(formFieldName, uniqueID, propertyValues);		
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
			return String.Format(
				TextBoxPattern,
				HttpUtility.HtmlEncode(formFieldName),
				HttpUtility.HtmlEncode(propertyValue),
				GuidCharacters
			);
		}

		private string RenderDefaultControl(string formFieldName, string uniqueID)
		{
			string propertyValue = property == null && generateDefault ? (defaultValue ?? (defaultValue = Guid.NewGuid())).Value.ToString() : String.Empty;
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
			bool isAnyValueSet = !required;
			postValues = HttpContext.Current.Request.Form.GetValues(formFieldName);

			if (postValues != null && postValues.Length > 0)
			{
				
				propertyValues = new List<Guid>();
				foreach (string value in postValues)
				{
					if (BXStringUtility.IsNullOrTrimEmpty(value))
						continue;

					Guid? guidValue;
					if (ValidateValue(value, errors, out guidValue))
					{
						isAnyValueSet = true;
						if (guidValue.HasValue)
							propertyValues.Add(guidValue.Value);
					}
					else
					{
						isValidated = false;
						break;
					}

					if (!field.Multiple)
						break;
				}
			}

			if (isValidated && !isAnyValueSet)
			{
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "FieldMustBeFilled")), field.EditFormLabel));
				isValidated = false;
			}

			return isValidated;
		}

		private bool ValidateValue(string value, ICollection<string> errors, out Guid? guid)
		{
			try
			{
				guid = new Guid(value);
			}
			catch
			{
				guid = null;
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueMustBeGuid")), field.EditFormLabel));
				return false;
			}
			
			return true;
		}

		protected override void Save(string formFieldName, BXCustomPropertyCollection storage)
		{
			if (field == null)
				return;

			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, propertyValues.ConvertAll<SqlGuid>(delegate (Guid input) { return new SqlGuid(input); }));
			storage[field.Name] = property;
		}

		public override void SaveDefault(BXCustomPropertyCollection storage)
		{
			if (field == null || !generateDefault)
				return;

			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, new SqlGuid((defaultValue ?? (defaultValue = Guid.NewGuid())).Value));
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
				
				string v;
				if (value is SqlGuid)
					v =	((SqlGuid)value).Value.ToString();
				else if (value is Guid)
					v = ((Guid)value).ToString();
				else
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);
				s.Append(HttpUtility.HtmlEncode(v));
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

				string v;
				if (value is SqlGuid)
					v =	((SqlGuid)value).Value.ToString();
				else if (value is Guid)
					v = ((Guid)value).ToString();
				else
					continue;

				if (separate)
					writer.Write(separatorHtml);
				else
					separate = true;

				writer.WriteEncodedText(v);
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
