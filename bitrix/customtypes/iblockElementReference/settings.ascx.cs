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

using Bitrix.UI;
using Bitrix.IBlock;
using Bitrix.DataTypes;
using Bitrix.DataLayer;
using Bitrix.Services.Text;

public partial class BXCustomTypeIBlockElementReferenceSettings : BXControl, IBXCustomTypeSetting
{
	protected void Page_Load(object sender, EventArgs e)
	{
		ddlIBlock.Items.Clear();

		ddlIBlock.Items.Add(new ListItem(GetMessageRaw("SelectIBlockID"), String.Empty));
		BXIBlockCollection iblockCollection = BXIBlock.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
		foreach (BXIBlock iblock in iblockCollection)
			ddlIBlock.Items.Add(new ListItem(iblock.Name, iblock.Id.ToString()));

		DataBindChildren();
	}
	protected void Page_PreRender(object sender, EventArgs e)
	{
		//RegisterJavaScript();
	}

	//private void RegisterJavaScript()
	//{
	//    StringBuilder s = new StringBuilder();
	//    s.Append("if(!isNaN(args.Value)){");
	//    s.Append("var i=parseInt(args.Value);");
		
	//    s.AppendFormat("var tb=document.getElementById('{0}');", MinValue.ClientID);
	//    s.Append("if(tb&&!isNaN(tb.value)){");
	//    s.Append("var j=parseInt(tb.value);");
	//    s.Append("if (i<j)return;");
	//    s.Append("}");

	//    s.AppendFormat("var tb=document.getElementById('{0}');", MaxValue.ClientID);
	//    s.Append("if(tb&&!isNaN(tb.value)){");
	//    s.Append("var j=parseInt(tb.value);");
	//    s.Append("if (i>j)return;");
	//    s.Append("}");

	//    s.Append("}");

	//    DefaultValueConditions.ClientValidationFunction = ClientID + "_ValidateDefault";
	//    string js = string.Format("function {0}(source,args){{args.IsValid=false;{1}args.IsValid=true}}\n", DefaultValueConditions.ClientValidationFunction, s.ToString());
		
	//    ScriptManager.RegisterClientScriptBlock(Page, GetType(), ClientID, js, true);
	//}

	#region IBXCustomTypeSetting Members

	public BXParamsBag<object> GetSettings()
	{
		BXParamsBag<object> result = new BXParamsBag<object>();
		int i;
		if (int.TryParse(ddlIBlock.SelectedValue, out i))
			result.Add("IBlockId", i);
		return result;
	}

	public void SetSettings(BXParamsBag<object> settings)
	{
		if (settings.ContainsKey("IBlockId"))
			ddlIBlock.SelectedValue = settings["IBlockId"].ToString();
		else
			ddlIBlock.SelectedIndex = 0;
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
