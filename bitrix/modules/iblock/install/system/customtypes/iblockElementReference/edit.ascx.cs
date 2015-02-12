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
using Bitrix.DataLayer;
using Bitrix.Services.Js;

public partial class BXCustomTypeIBlockElementReferenceEdit : BXControl, IBXCustomTypeEdit
{
	private BXCustomField field;
	private BXCustomProperty value;
	private int iblockId;

	#region IBXCustomTypeEdit Members
	public void Initialize(BXCustomField currentField, BXCustomProperty currentValue)
	{
		field = currentField;
		value = currentValue;

		if (field == null)
			return;

		string fieldName = currentField.EditFormLabel;


		ValueRequired.Enabled = currentField.Mandatory;
		if (ValueRequired.Enabled)
			ValueRequired.ErrorMessage = GetMessageFormat("Error.Required", fieldName);

		//Инициализируем только текстбокс
		if (value != null && value.Value != null)
		{
			int elId;
			if (int.TryParse(value.Value.ToString(), out elId) && elId > 0)
				tbValue.Text = elId.ToString();
		}

		BXParamsBag<object> settings = new BXParamsBag<object>(field.Settings);
		iblockId = settings.ContainsKey("IBlockId") ? (int)settings["IBlockId"] : 0;

	}

	protected override void OnPreRender(EventArgs e)
	{
		bSearch.Attributes["onclick"] = String.Format(
			"jsUtils.OpenWindow('{0}?iblock_id={1}&n={2}&k={3}', 600, 500);",
			BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/IBlockElementSearch.aspx")),
			iblockId,
			tbValue.ClientID,
			lbName.ClientID
		);

		//Настраиваем внешний вид
		int elementId;
		if (int.TryParse(tbValue.Text, out elementId) && elementId > 0)
		{
			BXFilter f = new BXFilter();
            f.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId));
            if (iblockId > 0)
                f.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId));
            BXIBlockElementCollection element = BXIBlockElement.GetList(f, null);

			if (element.Count != 0)
			{
				tbValue.Text = element[0].Id.ToString();
				lbName.Text = element[0].Name;
			}
			else
			{
				tbValue.Text = string.Empty;
				lbName.Text = string.Empty;
			}
		}


		base.OnPreRender(e);
	}

	public void Save(BXCustomPropertyCollection storage)
	{
		if (field == null)
			return;

		int elementId;
		int.TryParse(tbValue.Text, out elementId);

        BXFilter f = new BXFilter();
        f.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId));
        if (iblockId > 0)
            f.Add(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId));
        BXIBlockElementCollection element = BXIBlockElement.GetList(f, null);

		if (element == null || element.Count != 1)
			elementId = 0;

		BXCustomType t = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
		storage[field.Name] = new BXCustomProperty(field.Name, field.Id, t.DbType, t.IsClonable ? false : field.Multiple, elementId);
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
			DataBindChildren();
		}
	}
	#endregion

}
