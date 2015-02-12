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
using Bitrix.Services;
using System.Text;
using System.Collections.Generic;
using Bitrix.DataTypes;

public partial class BXCustomTypeBoolean : BXCustomType
{
	public override string Title
	{
		get
		{
			return BXLoc.GetMessage(this, "TypeName");
		}
	}

	public override string Description
	{
		get
		{
			return BXLoc.GetMessage(this, "TypeName");
		}
	}

	public override SqlDbType DbType
	{
		get
		{
			return SqlDbType.Bit;
		}
	}

	public override string TypeName
	{
		get
		{
			return "Bitrix.System.Boolean";
		}
	}



	public override BXCustomTypePublicView CreatePublicView()
	{
		return new PublicView();
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

	public override Control GetFilter(BXCustomField field)
	{
		var filter = new BXDropDownFilter();
		filter.Key = "@" + field.OwnerEntityId + ":" + field.Name;
		filter.ValueType = BXAdminFilterValueType.Boolean;
		filter.Values.Add(new ListItem(GetMessageRaw("Kernel.All"), ""));
		filter.Values.Add(new ListItem(GetMessageRaw("Kernel.Yes"), "true"));
		filter.Values.Add(new ListItem(GetMessageRaw("Kernel.No"), "false"));
		return filter;
	}

    public override bool TryGetDefaultValue(BXCustomField field, out object value)
    {
        int v;
        if (field != null && field.Settings.TryGetInt("default", out v))
        {
            value = v != 1;
            return true;
        }
        value = false;
        return false;        
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
		private bool? propertyValue;
		
		//Settings
		private bool defaultValue;
		private BooleanViewType view;

		public PublicEditor(BXCustomType type)
		{
			this.type = type;
		}

		private enum BooleanViewType : int
		{
			Checkbox,
			RadioButton,
			DropDown,
			Default = Checkbox
		}

		public override void Init(BXCustomField currentField)
		{
			field = currentField;
			if (field == null)
				return;

			BXParamsBag<object> settings = new BXParamsBag<object>(currentField.Settings);
			defaultValue = settings.GetInt("default", 1) != 1;
			view = settings.ContainsKey("view") ? (BooleanViewType)settings["view"] : BooleanViewType.Default;
		}

		public override void Load(BXCustomProperty currentProperty)
		{
			if (field == null)
				return;

			property = currentProperty;

			if (property != null && property.Value != null && property.Value is bool)
				propertyValue = (bool)property.Value;			
		}

		public override string Render(string formFieldName, string uniqueID)
		{
			if (field == null)
				return String.Empty;

			string checkBoxPattern = "<input type=\"checkbox\" name=\"{0}\" value=\"{1}\" id=\"{3}\" class=\"bx-custom-field custom-field-bool-checkbox\" {2}/>";
			string radioButtonPattern = "<input type=\"radio\" name=\"{0}\" value=\"{1}\" id=\"{7}Y\" class=\"bx-custom-field custom-field-bool-radio\" {2}/><label for=\"{7}Y\">&nbsp;{3}</label><br /><input type=\"radio\" name=\"{0}\" id=\"{7}N\" value=\"{4}\" class=\"custom-field-radio\" {5}/><label for=\"{7}N\">&nbsp;{6}</label>";
			string dropDownPattern = "<select name=\"{0}\" class=\"bx-custom-field custom-field-bool-dropdown\"><option value=\"{1}\"{2}>{3}</option><option value=\"{4}\"{5}>{6}</option></select>";

			bool value = propertyValue.HasValue ? propertyValue.Value : defaultValue;
			string result;
			
			if (view == BooleanViewType.RadioButton)
			{
				result = String.Format(
					radioButtonPattern,
					HttpUtility.HtmlEncode(formFieldName),
					"Y",
					value ? "checked=\"checked\"" : String.Empty,
					HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "Kernel.Yes")),
					"N",
					!value ? "checked=\"checked\"" : String.Empty,
					HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "Kernel.No")),
					HttpUtility.HtmlEncode(uniqueID)
				);
			}
			else if (view == BooleanViewType.DropDown)
			{
				result = String.Format(
					dropDownPattern,
					HttpUtility.HtmlEncode(formFieldName),
					"Y",
					value ? " selected=\"selected\"" : String.Empty,
					HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "Kernel.Yes")),
					"N",
					!value ? " selected=\"selected\"" : String.Empty,
					HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "Kernel.No"))
				);
			}
			else
			{
				result = String.Format(
					checkBoxPattern,
					HttpUtility.HtmlEncode(formFieldName),
					"Y",
					value ? "checked=\"checked\"" : String.Empty,
					HttpUtility.HtmlEncode(uniqueID)
				);
			}

			return result;
		}

		protected override bool Validate(string formFieldName, ICollection<string> errors)
		{
			string[] propertyValues = HttpContext.Current.Request.Form.GetValues(formFieldName);
			if (propertyValues != null && propertyValues.Length > 0 && propertyValues[0].Equals("Y", StringComparison.OrdinalIgnoreCase))
				propertyValue = true;
			else
				propertyValue = false;

			return true;
		}

		protected override void Save(string formFieldName, BXCustomPropertyCollection storage)
		{
			if (field == null)
				return;

			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, propertyValue.Value);
			storage[field.Name] = property;
		}

		public override void SaveDefault(BXCustomPropertyCollection storage)
		{
			if (field == null)
				return;

			property = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, defaultValue);
			storage[field.Name] = property;
		}
	}
	class PublicView : BXCustomTypePublicView
	{
		public override string GetHtml(string uniqueId, string separatorHtml)
		{
			if (property == null || property.Value == null)
				return string.Empty;
			return HttpUtility.HtmlEncode((bool)property.Value ? BXLoc.GetModuleMessage("main", "Kernel.Yes") : BXLoc.GetModuleMessage("main", "Kernel.No"));
		}
		public override void Render(string uniqueId, string separatorHtml, HtmlTextWriter writer)
		{
			if (property == null || property.Value == null)
				return;
			writer.WriteEncodedText((bool)property.Value ? BXLoc.GetModuleMessage("main", "Kernel.Yes") : BXLoc.GetModuleMessage("main", "Kernel.No"));
		}
		public override bool IsEmpty
		{
			get
			{
				return (property == null || property.Value == null);
			}
		}
	}
}
