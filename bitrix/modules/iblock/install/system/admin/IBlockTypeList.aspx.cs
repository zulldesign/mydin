using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;

public partial class bitrix_admin_IBlockTypeList : BXAdminPage
{
    bool currentUserCanModifyType = false;
    protected StringBuilder deletedList = new StringBuilder();

    protected void Page_Init(object sender, EventArgs e)
    {
        if (!this.BXUser.IsCanOperate("IBlockView"))
            BXAuthentication.AuthenticationRequired();
        
        currentUserCanModifyType = this.BXUser.IsCanOperate("IBlockAdmin");
        AddButton.Visible = currentUserCanModifyType;
        if (!currentUserCanModifyType)
            GridView1.PopupCommandMenuId = PopupPanelView.ID;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        successMessage.Visible = false;
        ((BXAdminMasterPage)Page.Master).Title = Page.Title;
        deletedList.Append(GetMessageRaw("Message.RecordsHaveBeenDeletedSuccessfully"));
        deletedList.Append("<ul class=\"deleted-list\" style=\"font-size:120%\">");
    }

    protected void GridView1_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
    {
        List<string> visibleColumnsList = new List<string>();
        for (int i = 0; i < ((BXGridView)sender).Columns.Count; i++)
            if (((BXGridView)sender).Columns[i].Visible)
                visibleColumnsList.Add("");

        BXIBlockTypeCollection collection = BXIBlockType.GetList(
            new BXFilter(MakeCurrentFilter(), BXIBlockType.Fields),
            new BXOrderBy(BXIBlockType.Fields, e.SortExpression),
            null,
            new BXQueryParams(e.PagingOptions), 
            BXTextEncoder.EmptyTextEncoder
        );

        e.Data = new DataView(FillTable(collection, visibleColumnsList));
    }
    private DataTable FillTable(BXIBlockTypeCollection collection, List<string> visibleColumnsList)
    {
        if (collection == null)
            collection = new BXIBlockTypeCollection();

        DataTable result = new DataTable();

        result.Columns.Add("TypeId", typeof(int));
        result.Columns.Add("Name", typeof(string));
        result.Columns.Add("Sort", typeof(int));
        result.Columns.Add("HaveSections", typeof(string));

        foreach (BXIBlockType t in collection)
        {
            DataRow r = result.NewRow();
            r["TypeId"] = t.Id;
            r["Name"] = t.Translations[BXLoc.CurrentLocale].Name;
            r["Sort"] = t.Sort;
            r["HaveSections"] = (t.HaveSections ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No"));
            result.Rows.Add(r);
        }

        return result;
    }
    protected void GridView1_Delete(object sender, BXDeleteEventArgs e)
    {
        BXGridView grid = (BXGridView)sender;
        try
        {
            BXIBlockTypeCollection elements;
            if (e.Keys != null) //Delete one element
            {
                elements = new BXIBlockTypeCollection();

                int typeId;
                Int32.TryParse(e.Keys["TypeId"].ToString(), out typeId);

                if (typeId <= 0)
                    throw new PublicException(GetMessageRaw("Error.CodeOfTypeIsNotFound"));

                BXIBlockType elem = BXIBlockType.GetById(typeId);
                if (elem == null)
                    throw new PublicException(GetMessageRaw("Exception.TypeIsNotFound"));
                elements.Add(elem);

            }
            else //All elements
            {
                elements = BXIBlockType.GetList(new BXFilter(MakeCurrentFilter(), BXIBlockType.Fields), null);
            }
            int iblocksCount;
            int delErrorCount = 0;
            string elemName;
            foreach (BXIBlockType element in elements)
            {
                if (element == null)
                    throw new PublicException(GetMessageRaw("Exception.ElementIsNotFound"));

                if (!currentUserCanModifyType)
                    throw new PublicException(GetMessageRaw("Exception.YouDontHaveRightsToDeleteThisRecord"));

                iblocksCount = BXIBlock.Count(new BXFilter(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, element.Id)));
                elemName = element.Translations[BXLoc.CurrentLocale].Name;
                if (iblocksCount > 0)
                {
                    delErrorCount++;
                    errorMessage.AddErrorText(String.Format(GetMessageRaw("Exception.IBlockTypeContainsIBlocks"),BXTextEncoder.HtmlTextEncoder.Decode(elemName) ));
                }
                else
                {

                    //throw new PublicException(GetMessageRaw("Exception.AnErrorHasOccurredWhileDeletingElement"));
                    //successMessage.AddErrorText(String.Format("\"{0}\"", BXTextEncoder.HtmlTextEncoder.Decode(element.Translations[BXLoc.CurrentLocale].Name)));
                    BXIBlockType.Delete(element.Id);
                    deletedList.Append(String.Format("<li>\"{0}\"</li>",elemName ));
                    e.DeletedCount++;
                }
            }
            if (delErrorCount == 0)
                successMessage.Visible = true;
        }
        catch (Exception ex)
        {
            ProcessException(ex, errorMessage.AddErrorText);

        }
        
        grid.MarkAsChanged();
    }
    protected void GridView1_PopupMenuClick(object sender, Bitrix.UI.BXPopupMenuClickEventArgs e)
    {
        BXGridView grid = sender as BXGridView;

        DataKey drv = grid.DataKeys[e.EventRowIndex];
        if (drv == null)
            return;

        int typeId;
        Int32.TryParse(drv.Value.ToString(), out typeId);
        

        if (typeId > 0)
        {
            switch (e.CommandName)
            {
                case "edit":
                    e.Cancel = true;
                    Response.Redirect(String.Format("IBlockTypeEdit.aspx?id={0}", typeId));
                    break;
                default:
                    break;
            }
        }
        else
            errorMessage.AddErrorMessage(GetMessage("Exception.CodeOfTypeIsNotFound"));
    }
    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        BXGridView grid = (BXGridView)sender;
        BXGridViewRow row = (BXGridViewRow)e.Row;
        DataRowView drv = (DataRowView)row.DataItem;
        if (row.RowType != DataControlRowType.DataRow)
            return;

        row.UserData.Add("Id", drv["TypeId"]);
    }

    BXFormFilter MakeCurrentFilter()
    {
        BXFormFilter filter = new BXFormFilter(BXAdminFilter1.CurrentFilter);
        return filter;
    }

    protected void Page_LoadComplete(object sender, EventArgs e)
    {
        deletedList.Append("</ul>");
        successMessage.Content = deletedList.ToString();
    }
}
