using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.Blog;
using Bitrix.Configuration;
using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.Modules;

public partial class bitrix_admin_BlogCustomFieldList : BXAdminPage
{
    private bool? _isUserAuthorized = null;
    protected bool IsUserAuthorized
    {
        get { return _isUserAuthorized.HasValue ? _isUserAuthorized.Value : (_isUserAuthorized = BXPrincipal.Current.IsCanOperate(BXBlog.Operations.AdminManagement)).Value; }
    }

    protected override void OnPreInit(EventArgs e)
    {
        if (!IsUserAuthorized)
            BXAuthentication.AuthenticationRequired();
        base.OnPreInit(e);
    }
    protected override void OnInit(EventArgs e)
    {
        AddForPostButton.Href = string.Format(
            "CustomFieldEdit.aspx?entityid={0}&{1}={2}",
            HttpUtility.UrlEncode(BXBlogModuleConfiguration.PostCustomFieldEntityId),
            BXConfigurationUtility.Constants.BackUrl, 
            HttpUtility.UrlEncode(Request.RawUrl));

        AddForBlogButton.Href = string.Format(
            "CustomFieldEdit.aspx?entityid={0}&{1}={2}",
            HttpUtility.UrlEncode(BXBlogModuleConfiguration.BlogCustomFieldEntityId),
            BXConfigurationUtility.Constants.BackUrl,
            HttpUtility.UrlEncode(Request.RawUrl));
        base.OnInit(e);
    }
    protected override void OnLoad(EventArgs e)
    {
        Page.Title = MasterTitle = GetMessage("PageTitle");
        base.OnLoad(e);
    }
    protected string GetTypeName(string typeId)
    {
        BXCustomType type = BXCustomTypeManager.GetCustomType(typeId);
        return type != null ? type.Title : GetMessageRaw("FilterText.Undefined");
    }
    protected string GetContextName(string ownerEntityId)
    {
        if (string.Equals(ownerEntityId, BXBlogModuleConfiguration.PostCustomFieldEntityId, StringComparison.OrdinalIgnoreCase))
            return GetMessageRaw("Context.Post");
        if (string.Equals(ownerEntityId, BXBlogModuleConfiguration.BlogCustomFieldEntityId, StringComparison.OrdinalIgnoreCase))
            return GetMessageRaw("Context.Blog");
        return GetMessageRaw("Context.Unknown");
    }
    protected string GetFilterVisibilityCaption(BXCustomFieldFilterVisibility visibility)
    {
        switch (visibility)
        {
            case BXCustomFieldFilterVisibility.Hidden:
                return GetMessageRaw("FilterText.DontDisplay");
            case BXCustomFieldFilterVisibility.CompleteMatch:
                return GetMessageRaw("FilterText.ExactFit");
            case BXCustomFieldFilterVisibility.MaskMatch:
                return GetMessageRaw("FilterText.MaskSearch");
            case BXCustomFieldFilterVisibility.SubstringMatch:
                return GetMessageRaw("FilterText.SubstringSearch");
        }
        return GetMessageRaw("FilterText.Undefined");
    }
    protected void BXDropDownKeyTextBoxFilter1_CustomBuildFilter(object sender, BXDropDownKeyTextBoxFilter.BuildFilterEventArgs e)
    {
        e.FilterItems.Clear();
        switch (e.KeyValue)
        {
            case "Id":
                int id;
                if (int.TryParse(e.TextBoxValue, out id))
                    e.FilterItems.Add(new BXFormFilterItem("Id", id, BXSqlFilterOperators.Equal));
                break;
            case "OwnerEntityId":
                if (!string.IsNullOrEmpty(e.TextBoxValue))
                    e.FilterItems.Add(new BXFormFilterItem("OwnerEntityId", e.TextBoxValue, BXSqlFilterOperators.Like));
                break;
        }
    }
    protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
    {      
    }
    private BXFilter CreateSelectFilter()
    {
        BXFilter r = new BXFilter(BXAdminFilter1.CurrentFilter, BXCustomField.Fields);
        r.Add(
            new BXFilterOr(
                new BXFilterItem(BXCustomField.Fields.OwnerEntityId, BXSqlFilterOperators.Equal, BXBlogModuleConfiguration.PostCustomFieldEntityId),
                new BXFilterItem(BXCustomField.Fields.OwnerEntityId, BXSqlFilterOperators.Equal, BXBlogModuleConfiguration.BlogCustomFieldEntityId)
            )
        );

        return r;
    }
    protected void BXGridView1_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        e.Count = BXCustomField.Count(CreateSelectFilter());
    }
    protected void BXGridView1_Select(object sender, BXSelectEventArgs e)
    {
        if (!IsUserAuthorized)
            return;

        e.Data = BXCustomField.GetList(
            CreateSelectFilter(),
            new BXOrderBy(BXCustomField.Fields, e.SortExpression),
            null,
            new BXQueryParams(e.PagingOptions)
        );
    }
    protected void BXGridView1_Delete(object sender, BXDeleteEventArgs e)
    {
        try
        {
            if (!IsUserAuthorized)
                throw new PublicException(GetMessageRaw("InsufficientRightsToDeleteCustomFields"));

            BXCustomFieldCollection fields;
            if (e.Keys != null)
            {
                fields = new BXCustomFieldCollection();
                fields.Add(BXCustomField.GetById(e.Keys["Id"]));
            }
            else
                fields = BXCustomField.GetList(new BXFilter(BXAdminFilter1.CurrentFilter, BXCustomField.Fields), null);

            foreach (BXCustomField f in fields)
                f.Delete();
        }
        catch (Exception ex)
        {
            ProcessException(ex);
        }
    }
    protected void BXGridView1_PopupMenuClick1(object sender, BXPopupMenuClickEventArgs e)
    {
        if(string.Equals(e.CommandName.ToUpperInvariant(), "EDIT", StringComparison.InvariantCulture))
            Response.Redirect(
                string.Format(
                    "~/bitrix/admin/CustomFieldEdit.aspx?id={0}&{1}={2}", 
                    BXGridView1.DataKeys[e.EventRowIndex].Value, 
                    BXConfigurationUtility.Constants.BackUrl, 
                    Request.RawUrl
                    )
            );
    }
    private void ProcessException(Exception ex)
    {
        if (ex is PublicException)
        {
            errorMessage.AddErrorMessage(Encode(ex.Message));
            if (ex.InnerException != null)
                BXLogService.LogAll(ex.InnerException, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
            return;
        }

        BXEventException eventException = ex as BXEventException;
        if (eventException != null)
        {
            foreach (string msg in eventException.Messages)
                errorMessage.AddErrorMessage(Encode(msg));
            return;
        }

        errorMessage.AddErrorMessage(GetMessage("Kernel.UnknownError"));
        BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
    }
}
