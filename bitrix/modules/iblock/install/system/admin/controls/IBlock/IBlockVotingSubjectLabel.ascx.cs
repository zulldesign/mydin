using System;

using Bitrix.UI;
using System.ComponentModel;
using Bitrix.Services;
using System.Web.UI.WebControls;
using Bitrix.Services.Js;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.Modules;
using Bitrix.Main;
using Bitrix.Security;
using Bitrix.CommunicationUtility;
using Bitrix.CommunicationUtility.Rating;
using Bitrix.Search;
using Bitrix;
using Bitrix.DataLayer;
using System.Web;
using Bitrix.IBlock;

public partial class bitrix_admin_controls_IBlockVotingSubjectLabel : BXControl
{
    protected override void OnPreRender(EventArgs e)
    {
        this.VotingSubject.Text = GetSubject();
        base.OnPreRender(e);
    }

    public string GetSubject()
    {
        string typeName = Attributes["TypeName"],
            itemId = Attributes["ItemID"];

        bool displayTypeName;
        if (!bool.TryParse(Attributes["DisplayTypeName"], out displayTypeName))
            displayTypeName = false;

        BXIBlockElement el = BXIBlockElement.GetById(itemId, BXTextEncoder.EmptyTextEncoder);
        return el != null ? String.Format("{0}<a href=\"IBlockElementEdit.aspx?id={1}&iblock_id={2}\">{3}</a>", displayTypeName ? GetMessage("TypeName.IBlockElement", true) + " / " : string.Empty, el.Id, el.IBlockId, HttpUtility.HtmlEncode(el.Name)) : "N/A";     
    }
}