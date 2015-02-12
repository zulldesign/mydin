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
using System.Collections.Generic;
using Bitrix.UI;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix;
using System.Text;
using Bitrix.Security;
using Bitrix.Advertising;
using Bitrix.IO;
using Bitrix.Services.Js;

public partial class bitrix_admin_AdvertisingBannerList : BXAdminPage
{
    private List<BXAdvertisingSpace> advSpaces;
    protected List<BXAdvertisingSpace> AdvSpaces
    {
        get { return advSpaces; }
    }

    protected override void OnPreInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.BannerManagement))
            BXAuthentication.AuthenticationRequired();

        base.OnPreInit(e);
    }

    protected override void OnInit(EventArgs e)
    {
        ListBoxFilterSites.Values.Clear();
        BXSiteCollection siteCol = BXSite.GetList(null, null);
        foreach (BXSite site in siteCol)
            ListBoxFilterSites.Values.Add(new ListItem(site.TextEncoder.Decode(site.Name), site.Id));

        DropDownFilterSpaces.Values.Clear();
        DropDownFilterSpaces.Values.Add(new ListItem(GetMessageRaw("Option.All"), string.Empty));
        advSpaces = BXAdvertisingSpace.GetList(null, new BXOrderBy(new BXOrderByPair(BXAdvertisingSpace.Fields.Name, BXOrderByDirection.Asc)));
        foreach(BXAdvertisingSpace space in advSpaces)
            DropDownFilterSpaces.Values.Add(new ListItem(space.TextEncoder.Decode(space.Name), space.Id.ToString()));
        abFilterActive.Values.Add(new ListItem(GetMessageRaw("Kernel.Yes"), "True"));
        abFilterActive.Values.Add(new ListItem(GetMessageRaw("Kernel.No"), "False"));

        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        MasterTitle = GetMessage("PageTitle");
        Page.Title = GetMessage("PageTitle");

        base.OnLoad(e);
    }

    protected override void OnPreRender(EventArgs e)
    {
        RegisterScriptInclude(this, "~/bitrix/js/Main/utils_net.js");

        base.OnPreRender(e);
    }

    protected void ItemGrid_Select(object sender, BXSelectEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.BannerManagement))
            return;

        List<AdvertisingBannerWrapper> list = new List<AdvertisingBannerWrapper>();

        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXAdvertisingBanner.Fields);

        BXSelect s = new BXSelect(
            BXSelectFieldPreparationMode.Normal,
            BXAdvertisingBanner.Fields.Id,
            BXAdvertisingBanner.Fields.Name,
            BXAdvertisingBanner.Fields.ContentType,
            BXAdvertisingBanner.Fields.ContentFile,
            BXAdvertisingBanner.Fields.TextContentType,
            BXAdvertisingBanner.Fields.TextContent,
            BXAdvertisingBanner.Fields.Description,
            BXAdvertisingBanner.Fields.Active,
            BXAdvertisingBanner.Fields.DateCreated,
            BXAdvertisingBanner.Fields.DateLastModified,
            BXAdvertisingBanner.Fields.DateOfFirstDisplay,
            BXAdvertisingBanner.Fields.DateOfLastDisplay,
            BXAdvertisingBanner.Fields.EnableFixedRotation,
            BXAdvertisingBanner.Fields.EnableUniformRotationVelocity,
            BXAdvertisingBanner.Fields.EnableRedirectionCount,
            BXAdvertisingBanner.Fields.DateOfRotationStart,
            BXAdvertisingBanner.Fields.DateOfRotationFinish,
            BXAdvertisingBanner.Fields.DisplayCount,
            BXAdvertisingBanner.Fields.VisitorCount,
            BXAdvertisingBanner.Fields.RedirectionCount,
            BXAdvertisingBanner.Fields.MaxDisplayCount,
            BXAdvertisingBanner.Fields.MaxDisplayCountPerVisitor,
            BXAdvertisingBanner.Fields.MaxRedirectionCount,
            BXAdvertisingBanner.Fields.MaxVisitorCount,
            BXAdvertisingBanner.Fields.Weight,
            BXAdvertisingBanner.Fields.Space.Name,
            BXAdvertisingBanner.Fields.FlashVersion
            );

        BXAdvertisingBannerCollection advBannerList = BXAdvertisingBanner.GetList(
            f,
            new BXOrderBy(BXAdvertisingBanner.Fields, string.IsNullOrEmpty(e.SortExpression) ? "ID DESC" : e.SortExpression),
            s,
            new BXQueryParams(e.PagingOptions),
            BXTextEncoder.EmptyTextEncoder
            );

        foreach (BXAdvertisingBanner advBanner in advBannerList)
            list.Add(new AdvertisingBannerWrapper(advBanner, this));

        e.Data = list;
	}


    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.BannerManagement))
            return;
        e.Count = BXAdvertisingBanner.Count(new BXFilter(ItemFilter.CurrentFilter, BXAdvertisingBanner.Fields));
	}

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        AdvertisingBannerWrapper wrapper = (AdvertisingBannerWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);



        if ((row.RowState & DataControlRowState.Edit) > 0)
        {
            DropDownList dropDown = row.FindControl("categoryIdEdit") as DropDownList;
            if (dropDown != null)
                dropDown.SelectedValue = wrapper.SpaceName;
        }
    }

    protected void ItemGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        GridView grid = (GridView)sender;
        DropDownList dropDown = grid.Rows[e.RowIndex].FindControl("spaceEdit") as DropDownList;

        int spaceId;
        if (dropDown != null && int.TryParse(dropDown.SelectedValue, out spaceId))
            e.NewValues["SpaceId"] = spaceId.ToString();
    }

    protected void ItemGrid_Update(object sender, BXUpdateEventArgs e)
    {
        int bannerID=0;
        if (!int.TryParse(e.Keys["ID"].ToString(), out bannerID)) return;
        
        if (bannerID < 1)
            return;

        try
        {
            BXAdvertisingBanner banner = BXAdvertisingBanner.GetById(bannerID);
            if (banner == null || !BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.BannerManagement))
                return;

            if (e.NewValues.Contains("isActive"))
                banner.Active = (bool)e.NewValues["isActive"];

            if (e.NewValues.Contains("Name") && !BXStringUtility.IsNullOrTrimEmpty((string)e.NewValues["Name"]))
                banner.Name = (string)e.NewValues["Name"];

            int weight;
            if (e.NewValues.Contains("Weight") && int.TryParse((string)e.NewValues["Weight"], out weight))
                banner.Weight = weight;

            int spaceId;
            if (e.NewValues.Contains("SpaceId") && int.TryParse((string)e.NewValues["SpaceId"], out spaceId))
                banner.SpaceId = spaceId;

            banner.Update();
        }
        catch (Exception ex)
        {
            ErrorMessage.AddErrorMessage(ex.Message);
        }
    }


    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
	{
        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.BannerManagement))
            return;
        BXAdvertisingBannerCollection banners;

        try
        {
            BXFilter filter = (e.Keys == null)
                ? new BXFilter(ItemFilter.CurrentFilter, BXAdvertisingBanner.Fields)
                : new BXFilter(new BXFilterItem(BXAdvertisingBanner.Fields.Id, BXSqlFilterOperators.Equal, e.Keys["ID"]));
            banners = BXAdvertisingBanner.GetList(filter, null);

        }
        catch (Exception ex)
        {
            ErrorMessage.AddErrorMessage(ex.Message);
            return;
        }
        bool errorTextAdded = false;
        foreach (BXAdvertisingBanner banner in banners)
        {
            try
            {
                banner.Delete();
                e.DeletedCount++;
            }
            catch (Exception ex2)
            {
                if (!errorTextAdded)
                {
                    ErrorMessage.AddErrorMessage(GetMessage("Banner.DeleteFailure"));
                    errorTextAdded = true;
                }
            }

        }
	}
}
/// <summary>
/// Обёртка для рекламного баннера
/// </summary>
public class AdvertisingBannerWrapper
{
    BXAdvertisingBanner _charge;
    BXAdminPage _parentPage;
    public AdvertisingBannerWrapper(BXAdvertisingBanner charge, BXAdminPage parentPage)
    {
        if (charge == null)
            throw new ArgumentNullException("charge");

        if (parentPage == null)
            throw new ArgumentNullException("parentPage");

        _charge = charge;
        _parentPage = parentPage;
    }

    public string ID
    {
        get { return _charge.Id.ToString(); }
    }

    public string Name
    {
        get { return _charge.Name; }
    }

    public string Description
    {
        get { return _charge.Description; }
    }

    public string DateCreated
    {
        get { return _charge.DateCreated.ToString("g"); }
    }

    public string DateLastModified
    {
        get { return _charge.DateLastModified.ToString("g"); }
    }

    public string Active
    {
        get { return _charge.Active ? _parentPage.GetMessageRaw("Kernel.Yes") : _parentPage.GetMessageRaw("Kernel.No"); }
    }

    public bool isActive
    {
        get { return _charge.Active; }
    }

    public string Weight
    {
        get { return _charge.Weight.ToString(); }
    }

    public string SpaceName
    {
        get 
        {
            BXAdvertisingSpace space = _charge.Space;
            return space != null ? space.Name : "-"; 
        }
    }


    public int SpaceId
    {
        get
        {
            BXAdvertisingSpace space = _charge.Space;
            return space != null ? space.Id : 0;
        }
    }

    public string DateOfFirstDisplay
    {
        get 
        {
            DateTime? r = _charge.DateOfFirstDisplay;
            return r.HasValue ? r.Value.ToString("g") : "-";
        }
    }

    public string DateOfLastDisplay
    {
        get
        {
            DateTime? r = _charge.DateOfLastDisplay;
            return r.HasValue ? r.Value.ToString("g") : "-";
        }
    }

    public string EnableFixedRotation
    {
        get { return _charge.EnableFixedRotation ? _parentPage.GetMessageRaw("Kernel.Yes") : _parentPage.GetMessageRaw("Kernel.No"); }
    }

    public string EnableUniformRotationVelocity
    {
        get { return _charge.EnableUniformRotationVelocity ? _parentPage.GetMessageRaw("Kernel.Yes") : _parentPage.GetMessageRaw("Kernel.No"); }
    }


    private BXAdvertisingBannerRotationStopReason? _rotationStopReason = null;
    public BXAdvertisingBannerRotationStopReason RotationStopReason
    {
        get
        {
            return (_rotationStopReason ?? (_rotationStopReason = _charge.GetRotationStopReason())).Value;
        }
    }

    public bool IsInRotation
    {
        get { return _charge.IsInRotation; }
    }

    public string DisplayCount
    {
        get { return _charge.DisplayCount.ToString(); }
    }

    public string RedirectionCount
    {
        get { return _charge.RedirectionCount.ToString(); }
    }

    public string VisitorCount
    {
        get { return _charge.VisitorCount.ToString(); }
    }

    public string ClickThroughRation
    {
        get { return _charge.ClickThroughRation.ToString("0.##"); }
    }

    private void AddRotationStopReason(BXAdvertisingBannerRotationStopReason reason,
        BXAdvertisingBannerRotationStopReason source,
        string text,
        StringBuilder destination)
    {
        if ((source & reason) == 0)
            return;

        if (destination.Length != 0)
            destination.Append("; ");
        destination.Append(text);
    }

    public string RotationLegend
    {
        get 
        {
            BXAdvertisingBannerRotationStopReason reason = RotationStopReason;
            if (reason == BXAdvertisingBannerRotationStopReason.None)
                return string.Concat(_parentPage.GetMessage("DisplayState.IsDisplayed"), ".");

            StringBuilder sb = new StringBuilder();
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.Active, reason, _parentPage.GetMessage("RotationStopReason.Active"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.PeriodStart, reason, _parentPage.GetMessage("RotationStopReason.RotationPeriodStart"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.PeriodFinish, reason, _parentPage.GetMessage("RotationStopReason.RotationPeriodFinish"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.DisplayLimit, reason, _parentPage.GetMessage("RotationStopReason.DisplayLimit"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.VistorLimit, reason, _parentPage.GetMessage("RotationStopReason.VistorLimit"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.RedirectionLimit, reason, _parentPage.GetMessage("RotationStopReason.RedirectionLimit"), sb);
            return sb.Insert(0, " (").Insert(0, _parentPage.GetMessage("DisplayState.IsNotDisplayed")).Append(").").ToString();
        }
    }

    private void AdjustSize(int maxWidth, int maxHeight, ref int width, ref int height)
    {
        double factorOfWidth = width > 0 ? (double)maxWidth / (double)width : 1D,
            factorOfHeight = height > 0 ? (double)maxHeight / (double)height : 1D,
            factor = factorOfWidth < factorOfHeight ? factorOfWidth : factorOfHeight;
        if (factor >= 1)
        {
            width = width > 0 ? width : maxWidth;
            height = height > 0 ? height : maxHeight;
        }
        else
        {
            width = width > 0 ? Convert.ToInt32(Math.Truncate(width * factor)) : maxWidth;
            height = height > 0 ? Convert.ToInt32(Math.Truncate(height * factor)) : maxHeight;
        }
    }

    public string GetContentHTML(int maxWidth, int maxHeight)
    {
        if (maxWidth <= 0)
            throw new ArgumentOutOfRangeException("maxWidth", "Must be greater than zero!");

        if (maxHeight <= 0)
            throw new ArgumentOutOfRangeException("maxHeight", "Must be greater than zero!");

        StringBuilder resultSb = new StringBuilder();
        switch (_charge.ContentType)
        {
            case BXAdvertisingBannerContentType.Flash:
                {
                    BXFile contentFile = _charge.ContentFile;
                    if (contentFile != null)
                    {
                        int width = contentFile.Width,
                            height = contentFile.Height;
                        AdjustSize(maxWidth, maxHeight, ref width, ref height);
                        string containerId = string.Concat("FlashBannerContainer", "_", _charge.Id.ToString());
                        resultSb.Append("<div id=\"").Append(containerId).Append("\" style=\"margin:0px; padding:0px; width:").Append(width.ToString()).Append("px; height:").Append(height.ToString()).Append("px;").AppendLine("\"></div>");

                        StringBuilder scriptSb = new StringBuilder("Bitrix.SwfUtility.getInstance().createElement(");
                        scriptSb.Append("\"").Append(containerId).Append("\"");
                        scriptSb.Append(", \"").Append(BXJSUtility.Encode(contentFile.FilePath)).Append("\"");
                        scriptSb.Append(", ").Append(width.ToString());
                        scriptSb.Append(", ").Append(height.ToString());
                        scriptSb.Append(", \"").Append(BXJSUtility.Encode(_charge.FlashWMode)).Append("\"");
                        if (!string.IsNullOrEmpty(_charge.FlashVersion))
                            scriptSb.Append(", \"").Append(BXJSUtility.Encode(_charge.FlashVersion)).Append("\"");
                        else
                            scriptSb.Append(", null");
                        if (_charge.FlashAltImageFile != null)
                            scriptSb.Append(", \"").Append(BXJSUtility.Encode(_charge.FlashAltImageFile.FilePath)).Append("\"");
                        else
                            scriptSb.Append(", null");
                        scriptSb.Append(");");

                        ScriptManager.RegisterStartupScript(_parentPage, 
                            GetType(), 
                            string.Concat("FlashBannerConstructor#", _charge.Id.ToString()),
                            scriptSb.ToString(),
                            true);
                    }
                }
                break;
            case BXAdvertisingBannerContentType.Silverlight:
                {
                    BXFile contentFile = _charge.ContentFile;
                    if (contentFile != null)
                    {
                        int width = contentFile.Width,
                            height = contentFile.Height;

                        resultSb.Append("<div style=\"margin:0px; padding:0px; width:").Append(contentFile.Width.ToString()).Append("px; height:").Append(contentFile.Height.ToString()).Append("px;").Append("\">");

                        resultSb.Append("<object  data=\"data:application/x-silverlight,\" type=\"application/x-silverlight\" width=\"").Append(contentFile.Width.ToString()).Append("\" height=\"").Append(contentFile.Height.ToString()).Append("\" id=\"").Append(string.Concat("SLObject", _charge.Id)).AppendLine("\">");
                        resultSb.Append("<param name=\"source\" value=\"").Append(BXJSUtility.Encode(contentFile.FilePath)).AppendLine("\" />");
                        resultSb.AppendLine("<param name=\"autoUpgrade\" value=\"true\" />");
                        resultSb.AppendLine("<param name=\"windowless\" value=\"true\" />");
                        if (_charge.Id == 7)
                        {
                            int i = 0;
                        }
                        if (!String.IsNullOrEmpty(_charge.FlashVersion))
                            resultSb.AppendFormat("<param name=\"minruntimeversion\" value=\"{0}\" />", BXJSUtility.Encode(_charge.FlashVersion));
                        if (!String.IsNullOrEmpty(_charge.FlashWMode))
                        {
                            resultSb.AppendFormat("<param name=\"background\" value=\"{0}\" />", BXJSUtility.Encode(_charge.FlashWMode));
                        }
            
                        resultSb.Append("<param name=\"background\" value=\"transparent\" />");
                        resultSb.AppendFormat("<a href=\"{0}\" style=\"text-decoration: none;\">",
                            (_charge.FlashAltImageFile != null ? _charge.LinkUrl : "http://go.microsoft.com/fwlink/?LinkID=124807"));
                        resultSb.AppendFormat("<img src=\"{0}\" alt=\"{1}\" style=\"border-style: none\"/>",
                               _charge.FlashAltImageFile != null ? _charge.FlashAltImageFile.FilePath : "http://go.microsoft.com/fwlink/?LinkId=108181",
                               _charge.FlashAltImageFile != null ? _charge.ToolTip : "Get Microsoft Silverlight");
                        resultSb.Append("</a>");
                        resultSb.AppendLine("</object>");
                        resultSb.AppendLine("</div>");
                    }
                }
                break;
            case BXAdvertisingBannerContentType.Image:
                {
                    BXFile contentFile = _charge.ContentFile;
                    if (contentFile != null)
                    {
                        int width = contentFile.Width,
                            height = contentFile.Height;
                        AdjustSize(maxWidth, maxHeight, ref width, ref height);
                        resultSb.Append("<img border=\"0px\" src=\"").Append(HttpUtility.HtmlAttributeEncode(contentFile.FilePath)).Append("\"");
                        resultSb.Append(" width=\"").Append(width.ToString()).Append("px\"");
                        resultSb.Append(" height=\"").Append(height.ToString()).Append("px\"");
                        string toolTip = _charge.ToolTip;
                        if (!string.IsNullOrEmpty(toolTip))
                        {
                            resultSb.Append(" alt=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip)).Append("\"");
                            resultSb.Append(" title=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip)).Append("\"");
                        }
                        resultSb.Append("/>");
                    }
                }
                break;
            case BXAdvertisingBannerContentType.TextOnly:
                {
                    if (!string.IsNullOrEmpty(_charge.TextContent))
                    {

                        //resultSb.Append("<div style=\"margin:0px; padding:0px; width:").Append(maxWidth.ToString()).Append("px; height:").Append(maxHeight.ToString()).Append("px;\" >");
                        //resultSb.AppendLine(_charge.TextContentType == BXAdvertisingBannerTextContentType.Html ? _charge.TextContent : HttpUtility.HtmlEncode(_charge.TextContent));
                        resultSb.AppendLine(HttpUtility.HtmlEncode(_charge.TextContent));
                        //resultSb.AppendLine("</div>");
                    }
                }
                break;
            default:
                throw new NotSupportedException(string.Format("Banner content type '{0}' is unknown in current context!", _charge.ContentType.ToString("G")));

        }
        return resultSb.ToString();
    }
}
