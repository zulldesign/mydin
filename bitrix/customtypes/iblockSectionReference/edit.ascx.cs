using System;
using System.Collections;
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

using Bitrix.Services;
using Bitrix.UI;
using System.Collections.Generic;
using Bitrix.IBlock;
using Bitrix.DataTypes;

public partial class BXCustomTypeIBlockSectionReferenceEdit : BXControl, IBXCustomTypeEdit
{
	private BXCustomField field;
	private BXCustomProperty value;

	protected void Page_Load(object sender, EventArgs e)
	{
		DataBindChildren();
	}

	#region IBXCustomTypeEdit Members
	public void Initialize(BXCustomField currentField, BXCustomProperty currentValue)
	{
		field = currentField;
        value = currentValue;

		if (field == null)
			return;

		string fieldName = currentField.EditFormLabel;

		BXParamsBag<object> settings = new BXParamsBag<object>(currentField.Settings);
		ValueRequired.Enabled = currentField.Mandatory;
		if (ValueRequired.Enabled)
			ValueRequired.ErrorMessage = GetMessageFormat("Error.Required", fieldName);

		ValueList.Rows = settings.ContainsKey("TextBoxSize") ? (int)settings["TextBoxSize"] : 5;
		ValueList.SelectionMode = (currentField.Multiple ? ListSelectionMode.Multiple : ListSelectionMode.Single);

		List<string> selectedValues = new List<string>();
		if (value != null)
		{
			foreach (object v in value.Values)
				selectedValues.Add(v.ToString());
		}

		ValueList.Items.Clear();

		int iblockId = 0;
		if (!settings.ContainsKey("IBlockId"))
		{
			BXInfoBlockCollectionOld c = BXInfoBlockManagerOld.GetList(null, null);
			if (c.Count > 0)
				iblockId = c[0].IBlockId;
		}
		else
			iblockId = (int)settings["IBlockId"];


		BXInfoBlockSectionCollectionOld sectionCollection = BXInfoBlockSectionManagerOld.GetTree(iblockId, 0);
		foreach (BXInfoBlockSectionOld section in sectionCollection)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < section.DepthLevel; i++)
				sb.Append(" . ");
			sb.Append(section.Name);
			ListItem l = new ListItem(sb.ToString(), section.SectionId.ToString());
			if (value != null && selectedValues.Contains(section.SectionId.ToString()))
				l.Selected = true;
			ValueList.Items.Add(l);
		}
    }

	public void Save(BXCustomPropertyCollection storage)
    {
		if (field == null)
			return;

		List<int> selectedValues = new List<int>();
		foreach (ListItem item in ValueList.Items)
		{
			int i;
			if (item.Selected && int.TryParse(item.Value, out i))
				selectedValues.Add(i);
		}
		
		BXCustomType t = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
		storage[field.Name] = new BXCustomProperty(field.Name, field.Id, t.DbType, t.IsClonable ? false : field.Multiple, selectedValues.ToArray());
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
    #endregion

}
