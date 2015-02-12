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
using Bitrix.UI;

public partial class BXCustomTypeEnumerationEdit : BXControl, IBXCustomTypeEdit
{
	//FIELDS
	private BXCustomField field;
	private ViewMode viewMode;
	BXCustomProperty value;
	BXParamsBag<object> settings;
	BaseValidator validator;
	BXCustomFieldEnumCollection enums;

	private enum ViewMode
	{
		List,
		Flag
	}


	private BXCustomFieldEnumCollection Enums
	{
		get
		{
			if (field == null)
				return null;
			if (enums == null)
				enums = BXCustomFieldEnum.GetList(field.Id, field.FieldType, BXTextEncoder.HtmlTextEncoder);
			return enums;
		}
	}

	public void Initialize(BXCustomField currentField, BXCustomProperty currentValue)
	{
		field = currentField;
		value = currentValue;
		if (field == null)
			return;

		settings = new BXParamsBag<object>(field.Settings);
		viewMode = settings.ContainsKey("ViewMode") && (string)settings["ViewMode"] == "list" ? ViewMode.List : ViewMode.Flag;

		valDDList.Enabled = false;
		valList.Enabled = false;
		valFlag.Enabled = false;
		valCheckbox.Enabled = false;
		
		ListControl list;
		int listCount = Math.Max(settings.GetInt("ListSize", 5), 1);
		if (viewMode == ViewMode.List)
		{
			if (field.Multiple)
			{
				View.ActiveViewIndex = 0;
				list = List;
				validator = valList;
				List.Rows = Math.Min(listCount, Enums.Count + (!field.Multiple && !field.Mandatory ? 1 : 0));
			}
			else
			{
				View.ActiveViewIndex = 3;
				list = DDList;
				validator = valDDList;
			}
		}
		else
		{
			if (field.Multiple)
			{
				View.ActiveViewIndex = 2;
				list = ChBox;
				validator = valCheckbox;
			}
			else
			{
				View.ActiveViewIndex = 1;
				list = Flag;
				validator = valFlag;
			}
		}

		ListItem none = null;
		if (!field.Multiple && !field.Mandatory)
		{
			none = new ListItem(GetMessage("Option.NotSelected"), "");
			none.Selected = true;
			list.Items.Add(none);
		}

		validator.Enabled = field.Mandatory;
		validator.ValidationGroup = ValidationGroup;

		foreach (BXCustomFieldEnum e in Enums)
			list.Items.Add(new ListItem(e.Value, e.Id.ToString()));


		if (value != null)
		{
			bool stop = false;
			foreach (ListItem item in list.Items)
			{
				foreach (int val in value.Values)
					if (item.Value == val.ToString())
					{
						if (!field.Multiple)
						{
							if (none != null)
								none.Selected = false;
							stop = true;
						}
						
						item.Selected = true;
						break;
					}

				if (stop)
					break;
			}
		}
		else //BIND DEFAULT
		{
			bool stop = false;
			foreach (ListItem item in list.Items)
			{
				foreach (BXCustomFieldEnum val in Enums)
					if (item.Value == val.Id.ToString() && val.Default)
					{
						if (!field.Multiple)
						{
							if (none != null)
								none.Selected = false;
							stop = true;
						}
						
						item.Selected = true;
						break;
					}

				if (stop)
					break;
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
		ListControl list;
		if (viewMode == ViewMode.List)
			list = field.Multiple ? (ListControl)List : (ListControl)DDList;
		else 
			list = field.Multiple ? (ListControl)ChBox : (ListControl)Flag;

		foreach (ListItem item in list.Items)
			if (item.Selected)
			{
				int id;
				if (int.TryParse(item.Value, out id) && Enums.Exists(delegate(BXCustomFieldEnum input)
				{
					return input.Id == id;
				}))
					values.Add(id);
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
			if (validator != null)
				validator.ValidationGroup = value;
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