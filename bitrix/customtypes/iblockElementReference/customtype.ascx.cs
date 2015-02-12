using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IBlock;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.Services.Js;

public partial class BXCustomTypeIBlockElementReference : BXCustomType
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
			return "Bitrix.IBlock.ElementId";
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
		var filter = new Filter();
		filter.Key =  "@" + field.OwnerEntityId + ":" + field.Name;
		filter.IBlockId = field.Settings.GetInt("IBlockId");
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


	class Filter : BXAdminFilterBaseItem
	{
		TextBox text;
		HtmlInputButton button;
		Label label;

		public string Key { get; set; }
		public int IBlockId { get; set; }

		public Filter()
		{
			text = new TextBox();
			Controls.Add(text);

			button = new HtmlInputButton();
			button.Value = "...";
			Controls.Add(button);

			Controls.Add(new LiteralControl("<br/>"));
			
			label = new Label();
			Controls.Add(label);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			button.Attributes["onclick"] = string.Format(
				"jsUtils.OpenWindow('{0}?iblock_id={1}&n={2}&k={3}', 600, 500);",
				BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementSearch.aspx")),
				IBlockId,
				text.ClientID,
				label.ClientID
			);

			int id;
			if (string.IsNullOrEmpty(text.Text) || !int.TryParse(text.Text.Trim(), out id) || id <= 0)
			{
				label.Text = "";
				return;
			}
			

			var filter = new BXFilter();
            filter.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, id));
            if (IBlockId > 0)
                filter.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId));
            
			var elements = BXIBlockElement.GetList(
				filter,
				null,
				new BXSelect(BXIBlockElement.Fields.Name),
				new BXQueryParams(new BXPagingOptions(0, 1))
			);
			label.Text = elements.Count == 0 ? "" : elements[0].Name;
		}

		public override BXFormFilterItem[] BuildFilter()
		{
			if (!String.IsNullOrEmpty(Key) && !String.IsNullOrEmpty(text.Text))
				return BuildSingleValueFilter(Key, text.Text, BXSqlFilterOperators.Equal, BXAdminFilterValueType.Integer);
			return new BXFormFilterItem[0];
		}

		public override void Reset()
		{
			text.Text = "";
		}

		public override bool InitValue()
		{
			if (base.DesignMode)
				return false;
			
			var str = HttpContext.Current.Request.QueryString["filter_" + Key];
			if (str == null)
				return false;

			text.Text = str;
			return true;
		}

		public override IDictionary SaveState()
		{
			return new Dictionary<string, object>()
			{
				{ "value", text.Text }
			};
		}

		public override void LoadState(IDictionary state)
		{
			if (state.Contains("value"))
				text.Text = (string)state["value"];
		}
	}
	class PublicEditor : BXCustomTypePublicEdit
	{
		private BXCustomType type;
		private BXCustomField field;
		private BXCustomProperty property;
		private List<int> propertyValues;

		private bool required;
		private int iblockId;

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
			iblockId = settings.ContainsKey("IBlockId") ? (int)settings["IBlockId"] : 0;
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

			string dropDownStart = "<select name=\"{0}\" id=\"{1}\" class=\"bx-custom-field custom-field-ibelement\" size=\"{2}\"{3}>";
			string dropDownOption = "<option value=\"{0}\"{1}>{2}</option>";
			string dropDownEnd = "</select>";

			BXIBlockElementCollection elements = BXIBlockElement.GetList(
				new BXFilter(
					new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId),
					new BXFilterItem(BXIBlockElement.Fields.IBlock.Active, BXSqlFilterOperators.Equal, "Y")
				),
				new BXOrderBy(
					new BXOrderByPair(BXIBlockElement.Fields.Sort, BXOrderByDirection.Asc),
					new BXOrderByPair(BXIBlockElement.Fields.Name, BXOrderByDirection.Asc)
				),
				new BXSelect(BXSelectFieldPreparationMode.Normal, BXIBlockElement.Fields.ID, BXIBlockElement.Fields.Name),
				null,
				BXTextEncoder.HtmlTextEncoder
			);

			StringBuilder result = new StringBuilder();

			result.AppendFormat(
				dropDownStart,
				HttpUtility.HtmlEncode(formFieldName),
				HttpUtility.HtmlEncode(uniqueID),
				field.Multiple ? 5 : 1,
				field.Multiple ? " multiple=\"multiple\"" : String.Empty
			);

			if (!field.Multiple)
				result.AppendFormat(dropDownOption, 0, String.Empty, HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "ValueNotSet")));

			foreach (BXIBlockElement element in elements)
			{
				result.AppendFormat(
					dropDownOption,
					element.Id,
					propertyValues != null && propertyValues.Contains(element.Id) ? " selected=\"selected\"" : String.Empty,
					element.Name
				);
			}

			result.Append(dropDownEnd);

			return result.ToString();
		}

		protected override bool Validate(string formFieldName, ICollection<string> errors)
		{
			if (field == null)
				return false;

			postValues = HttpContext.Current.Request.Form.GetValues(formFieldName);
			if (postValues == null || postValues.Length < 1)
			{
				if (propertyValues != null)
					propertyValues.Clear();

				if (required)
				{
					errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "FieldMustBeFilled")), field.EditFormLabel));
					return false;
				}

				return true;
			}

			if (!field.Multiple)
				postValues = new string[] { postValues[0] };

			bool success = true;
			propertyValues = new List<int>(postValues.Length);
			foreach (string value in postValues)
			{
				int elementId;
				if (!int.TryParse(value, out elementId))
				{
					success = false;
					break;
				}

				if (elementId < 1)
					continue;

				BXIBlockElementCollection element = BXIBlockElement.GetList(
					new BXFilter(
						new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId),
						new BXFilterItem(BXIBlockElement.Fields.Active, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId)
					),
					null,
					new BXSelect(BXSelectFieldPreparationMode.Normal, BXIBlockElement.Fields.ID),
					null
				);

				if (element == null || element.Count != 1)
				{
					success = false;
					break;
				}

				propertyValues.Add(elementId);
			}

			if (!success)
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "IncorrectValue")), field.EditFormLabel));
			else if (required && propertyValues.Count < 1)
			{
				errors.Add(String.Format(HttpUtility.HtmlEncode(BXLoc.GetMessage(type, "FieldMustBeFilled")), field.EditFormLabel));
				success = false;
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
	}
	class PublicView : BXCustomTypePublicView
	{
		private Dictionary<int, BXIBlockElement> elements;
		private void BuildElements()
		{
			if (this.elements != null)
				return;

			this.elements = new Dictionary<int, BXIBlockElement>();

			List<object> vals = property.Values;
			if (vals.Count == 0) //нет элементов
				return;
			if (vals.Count == 1) //элемент один, значение не int или не является идентификатором
			{
				try
				{
					if (((int)vals[0]) < 1)
						return;
				}
				catch (InvalidCastException)
				{
					return;
				}
			}

			BXFilter filter = new BXFilter();
			if (property.IsMultiple)
				filter.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.In, vals));
			else
				filter.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, vals[0]));

			if (field != null)
			{
				int iblockId = field.Settings.GetInt("IBlockId");
				if (iblockId > 0)
					filter.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId));
			}

			foreach (BXIBlockElement e in BXIBlockElement.GetList(filter, new BXOrderBy(new BXOrderByPair(BXIBlockElement.Fields.ID, BXOrderByDirection.Asc)), new BXSelect(BXSelectFieldPreparationMode.Normal, BXIBlockElement.Fields.ID, BXIBlockElement.Fields.Name), null, BXTextEncoder.EmptyTextEncoder))
				this.elements[e.Id] = e;
		}

		public override string GetHtml(string uniqueId, string separatorHtml)
		{
			if (property == null || property.Values.Count == 0)
				return string.Empty;

			if (elements == null)
				BuildElements();

			StringBuilder s = new StringBuilder();
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				int i = 0;
				try
				{
					i = (int)value;
				}
				catch (InvalidCastException)
				{
					continue;
				}

				BXIBlockElement e;
				if (!elements.TryGetValue(i, out e))
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);

				s.Append(HttpUtility.HtmlEncode(e.Name ?? ""));

			}
			return s.ToString();
		}
		public override void Render(string uniqueId, string separatorHtml, HtmlTextWriter writer)
		{
			if (property == null || property.Values.Count == 0)
				return;

			if (elements == null)
				BuildElements();

			bool separate = false;
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				BXIBlockElement e;
				if (!elements.TryGetValue((int)value, out e))
					continue;

				if (separate)
					writer.Write(separatorHtml);
				else
					separate = true;

				writer.WriteEncodedText(e.Name);
			}
		}
		public override bool IsEmpty
		{
			get
			{
				if (property == null || property.Values.Count == 0)
					return true;

				if (elements == null)
					BuildElements();

				foreach (object value in property.Values)
				{
					if (value == null)
						continue;

					if (elements.ContainsKey((int)value))
						return false;
				}
				return true;
			}
		}
	}
}
