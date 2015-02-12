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

using System.Collections.Generic;
using Bitrix.DataTypes;
using Bitrix.Services.Text;

public partial class BXCustomTypeListEdit : System.Web.UI.UserControl, IBXCustomTypeEdit
{
	//FIELDS
	private BXCustomField field;
	private ViewMode viewMode;
	BXCustomProperty value;
	BXParamsBag<object> settings;


	private enum ViewMode
	{
		List,
		Flag
	}

	public void Initialize(BXCustomField currentField, BXCustomProperty currentValue)
	{
		field = currentField;
		value = currentValue;
		if (field == null)
			return;

		settings = new BXParamsBag<object>(field.Settings);
		viewMode = settings.ContainsKey("ViewMode") && (string)settings["ViewMode"] == "list" ? ViewMode.List : ViewMode.Flag;

		BXCustomFieldEnumCollection values = BXCustomFieldEnum.GetList(field.Id, field.FieldType, BXTextEncoder.HtmlTextEncoder);

		valList.Enabled = false;
		valFlag.Enabled = false;
		valCheckbox.Enabled = false;

		if (viewMode == ViewMode.List)
		{
			View.ActiveViewIndex = 0;
			int listSize = settings.ContainsKey("ListSize") ? (int)settings["ListSize"] : 5;
			listSize = (listSize > values.Count) ? values.Count : listSize;
			if (listSize > 0)
			{
				List.Rows = listSize;
				List.DataSource = values.ConvertAll<ListItem>(delegate(BXCustomFieldEnum input) { return new ListItem(input.Value, input.TextEncoder.Decode(input.Value)); });
				List.SelectionMode = field.Multiple ? ListSelectionMode.Multiple : ListSelectionMode.Single;
				List.DataBind();
				valList.ValidationGroup = ValidationGroup;
				valList.Enabled = field.Mandatory;
			}
			else
				List.Visible = false;
		}
		else
		{
			if (field.Multiple)
			{
				View.ActiveViewIndex = 2;
				ChBox.DataSource = values.ConvertAll<ListItem>(delegate(BXCustomFieldEnum input) { return new ListItem(input.Value, input.TextEncoder.Decode(input.Value)); });
				ChBox.DataBind();
				valCheckbox.Enabled = field.Mandatory;
				valCheckbox.ValidationGroup = ValidationGroup;
			}
			else
			{
				View.ActiveViewIndex = 1;
				Flag.DataSource = values.ConvertAll<ListItem>(delegate(BXCustomFieldEnum input) { return new ListItem(input.Value, input.TextEncoder.Decode(input.Value)); });
				Flag.DataBind();
				valFlag.ValidationGroup = ValidationGroup;
				valFlag.Enabled = field.Mandatory;
			}
		}


		if (value != null)
		{
			ListControl listControl = viewMode == ViewMode.List ? (ListControl)List : field.Multiple ? (ListControl)ChBox : (ListControl)Flag;
			ListItemCollection items = listControl.Items;

			bool stop = false;
			foreach (ListItem item in items)
			{
				string itemValue = BXTextEncoder.HtmlTextEncoder.Decode(item.Value);
				foreach (string val in value.Values)
					if (itemValue.Equals(val, StringComparison.InvariantCulture))
					{
						item.Selected = true;
						if (!field.Multiple)
						{
							stop = true;
							break;
						}
					}
				if (stop)
					break;
			}
		}
		else
			//BIND DEFAULT
			foreach (BXCustomFieldEnum item in values)
			{
				if (item.Default)
				{
					if (viewMode == ViewMode.List)
						List.SelectedValue = item.Value;
					else if (field.Multiple)
						ChBox.SelectedValue = item.Value;
					else
						Flag.SelectedValue = item.Value;

					continue;
				}
			}
	}

	protected void ChBox_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = false;
		foreach (ListItem item in ChBox.Items)
		{
			if (item.Selected)
			{
				args.IsValid = true;
				break;
			}
		}
	}

	public void Save(BXCustomPropertyCollection storage)
	{
		if (field == null)
			return;


		List<object> values = new List<object>();

		if (viewMode == ViewMode.List)
		{
			foreach (ListItem item in List.Items)
			{
				if (item.Selected)
					values.Add(item.Value);
			}
		}
		else
		{
			ListItemCollection listCollection = field.Multiple ? ChBox.Items : Flag.Items;
			foreach (ListItem item in listCollection)
			{
				if (item.Selected)
					values.Add(item.Value);
			}
		}

		BXCustomType t = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
		storage[field.Name] = new BXCustomProperty(field.Name, field.Id, t.DbType, t.IsClonable ? false : field.Multiple, values.ToArray());
	}

	private string validationGroup = String.Empty;
	public string ValidationGroup
	{
		get
		{
			return validationGroup;
		}
		set
		{
			validationGroup = value;
			// IMPLEMENT SET VALIDATORS
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		if (!valCheckbox.Enabled)
			return;

		string functionName = ClientID;

		ScriptManager.RegisterClientScriptBlock(this, this.GetType(), ClientID,
		String.Format(@"
			function {0}(sender, args)
			{{
				var obj = document.getElementById(""{1}"");
				var result = false;
				if (obj)
				{{
					var inputs = obj.getElementsByTagName(""INPUT"");
					if (inputs.length > 0)
					{{
						for (var i = 0; i < inputs.length; i++)
						{{
							if (inputs[i].type == ""checkbox"" && inputs[i].checked)
							{{
								result = true;
								break;
							}}
						}}
					}}	
				}}
				args.IsValid = result;		
			}}
		", functionName, ChBox.ClientID), 
		true);

		valCheckbox.ClientValidationFunction = functionName;
	}
}