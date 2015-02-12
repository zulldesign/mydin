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
using Bitrix.UI;
using Bitrix.Services.Text;
using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Modules;
using System.Collections.Generic;
using Bitrix.Security;
using Bitrix.Configuration;
using Bitrix.Advertising;
using Bitrix.IO;
using System.Text;


public enum AdvertisingBannerEditorError
{
    None            = 0,
    IsNotFound      = -1,
    Creation        = -2,
    Modification    = -3,
    Deleting        = -4
}

public partial class bitrix_admin_AdvertisingBannerEdit : BXAdminDialogPage
{
	private int? _chargeId = null;
	private string _chargeSpaceCode = null;
    private string _slWidthName;
    private string _slHeightName;

    protected string SLHeightName
    {
        get { return _slHeightName; }
        set { _slHeightName = value; }
    }

    protected string SLWidthName
    {
        get { return _slWidthName; }
        set { _slWidthName = value; }
    }

	protected int ChargeId
	{
        get 
        {
            if (_chargeId.HasValue)
                return _chargeId.Value;

            _chargeId = 0;
            string idAsStr = Request.QueryString["id"];
            if (!string.IsNullOrEmpty(idAsStr))
            {
                try
                {
                    _chargeId = Convert.ToInt32(idAsStr);
                }
                catch
                {
                }
            }
            return _chargeId.Value; 
        }
        set
        {
            _chargeId = value;
        }
	}

	protected string ChargeSpaceCode
	{
        get 
        {
            if (_chargeSpaceCode != null)
                return _chargeSpaceCode;

            return _chargeSpaceCode = Request.QueryString["spaceCode"] ?? string.Empty;
		}
	}

    private BXAdminPageEditorMode _editorMode = BXAdminPageEditorMode.Creation;
    protected BXAdminPageEditorMode EditorMode
    {
        get { return _editorMode; }
    }

    private BXAdvertisingBanner _charge;
    protected BXAdvertisingBanner Charge
    {
        get { return _charge; }
    }

    protected bool IsTextContentEmpty
    {
        get { return _charge == null || string.IsNullOrEmpty(_charge.TextContent); }
    }

    private string _errorMessage = string.Empty;
    protected string ErrorMessage
    {
        get { return _errorMessage; }
    }

    private AdvertisingBannerEditorError _editorError = AdvertisingBannerEditorError.None;
    protected AdvertisingBannerEditorError EditorError
    {
        get { return _editorError; }
    }

	protected override string BackUrl
	{
		get { return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "AdvertisingBannerList.aspx"; }
	}

    protected override void OnInit(EventArgs e)
    {

        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.BannerManagement)) 
            BXAuthentication.AuthenticationRequired();

        BXAdvertisingSpaceCollection spaceCol = BXAdvertisingSpace.GetList(null, new BXOrderBy(new BXOrderByPair(BXAdvertisingSpace.Fields.Name, BXOrderByDirection.Asc)));
        abSpace.Items.Add(new ListItem(GetMessageRaw("ListItemText.SelectSpace"), "0"));
        foreach (BXAdvertisingSpace space in spaceCol)
            abSpace.Items.Add(new ListItem(space.TextEncoder.Decode(space.Name), space.Id.ToString()));

        abContentType.Items.Add(new ListItem(BXAdvertisingEnumHelper.GetBannerContentTypeName(BXAdvertisingBannerContentType.Image), BXAdvertisingBannerContentType.Image.ToString("d")));
        abContentType.Items.Add(new ListItem(BXAdvertisingEnumHelper.GetBannerContentTypeName(BXAdvertisingBannerContentType.Flash), BXAdvertisingBannerContentType.Flash.ToString("d")));
        abContentType.Items.Add(new ListItem(BXAdvertisingEnumHelper.GetBannerContentTypeName(BXAdvertisingBannerContentType.TextOnly), BXAdvertisingBannerContentType.TextOnly.ToString("d")));
        abContentType.Items.Add(new ListItem(BXAdvertisingEnumHelper.GetBannerContentTypeName(BXAdvertisingBannerContentType.Silverlight),BXAdvertisingBannerContentType.Silverlight.ToString("d")));
        
        abFlashWMode.Items.Add(new ListItem(GetMessageRaw("FlashPlayer.WMode.Transparent"), "transparent"));
        abFlashWMode.Items.Add(new ListItem(GetMessageRaw("FlashPlayer.WMode.Opaque"), "opaque"));
        abFlashWMode.Items.Add(new ListItem(GetMessageRaw("FlashPlayer.WMode.Window"), "window"));

        BXSiteCollection siteColl = BXSite.GetList(null, 
            new BXOrderBy(new BXOrderByPair(BXSite.Fields.Sort, BXOrderByDirection.Asc)), 
            new BXSelect(BXSite.Fields.ID, BXSite.Fields.Name), 
            null,
			BXTextEncoder.EmptyTextEncoder
            );

        ListItemCollection siteItems = abSites.Items;
        siteItems.Clear();

        foreach (BXSite site in siteColl)
            siteItems.Add(new ListItem(string.Format("[{0}] {1}", site.Id, site.TextEncoder.Decode(site.Name)), site.Id));


        BXRoleCollection userRoleCol = BXRoleManager.GetList(null, new BXOrderBy_old("RoleName", "Asc"));
        ListItemCollection userRolesItems = abUserRoles.Items;
        userRolesItems.Clear();
        foreach (BXRole userRole in userRoleCol)
            userRolesItems.Add(new ListItem(userRole.Title, userRole.RoleId.ToString()));

        ListItemCollection enableRotationForVisitorRolesItems = EnableRotationForVisitorRoles.Items;
        enableRotationForVisitorRolesItems.Clear();
        enableRotationForVisitorRolesItems.Add(new ListItem(GetMessageRaw("EnableRotationForVisitorRoles"), "Y"));
        enableRotationForVisitorRolesItems.Add(new ListItem(GetMessageRaw("DisableRotationForVisitorRoles"), "N"));

        

        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        //динамическая подстройка валидаторов
        bool enableFixedRotation = abEnableFixedRotation.Checked;
        bool enableUniformRotationVelocity = abEnableFixedRotation.Checked && abRotationModeUniform.Checked;
        //abWeightValidator.Enabled = !enableUniformRotationVelocity;
        abMaxDisplayCountValidator.Enabled = enableUniformRotationVelocity;
        abMaxDisplayCountValidatorType.Enabled = enableFixedRotation;
        abRotationPeriodStartValidator.Enabled = enableUniformRotationVelocity;
        abRotationPeriodFinishValidator.Enabled = enableUniformRotationVelocity;
        abMaxVisitorCountValidatorType.Enabled = enableFixedRotation;
        abMaxRedirectionCountValidatorType.Enabled = abEnableRedirectionCount.Checked;
        abMaxDisplayCountPerVisitorValidatorType.Enabled = enableFixedRotation;
        abFlashVersionValidatorType.Enabled = false;
        //abSLVersionValidatorType.Enabled = false;

        if(IsPostBack)
            Validate();

        TryLoadAdvertisingBanner();
        BXAdvertisingBanner banner = Charge;
        string title = EditorMode == BXAdminPageEditorMode.Modification ? string.Format(GetMessage("PageTitle.EditAdvertisingBanner"), banner != null ? HttpUtility.HtmlEncode(banner.Name) : "?") : GetMessage("PageTitle.CreateAdvertisingBanner");

        MasterTitle = title;
        Page.Title = title;

        base.OnLoad(e);
    }

        


    private void TryLoadAdvertisingBanner()
	{
        int id = ChargeId;
        if (id <= 0)
        {
            _editorMode = BXAdminPageEditorMode.Creation;
            _charge = new BXAdvertisingBanner();
			if(ChargeSpaceCode.Length > 0)
			{
				BXAdvertisingSpaceCollection c = BXAdvertisingSpace.GetList(
					new BXFilter(new BXFilterItem(BXAdvertisingSpace.Fields.Code, BXSqlFilterOperators.Equal, ChargeSpaceCode)),
					null,
					new BXSelect(BXSelectFieldPreparationMode.Normal, BXAdvertisingSpace.Fields.Id),
					new BXQueryParams(BXPagingOptions.Top(1)));

				if(c.Count > 0)
					_charge.SpaceId = c[0].Id;
			}
        }
        else
        {
            _editorMode = BXAdminPageEditorMode.Modification;
            if ((_charge = BXAdvertisingBanner.GetById(id, BXTextEncoder.EmptyTextEncoder)) == null)
            {
                _errorMessage = string.Format(GetMessageRaw("Error.UnableToFindAdvertisingBanner"), id);
                _editorError = AdvertisingBannerEditorError.IsNotFound;
                return;
            }
        }

        if (!IsPostBack)
        {
            abCode.Text = _charge.Code;
            abActive.Checked = _charge.Active;

            DateTime? rotationStart = _charge.DateOfRotationStart;
            //if (rotationStart.HasValue)
            //    AdvertisingBannerRotationPeriod.StartDate = rotationStart.Value;
            //else
            //    AdvertisingBannerRotationPeriod.SetEmptyStartDate();

            if (rotationStart.HasValue)
                abRotationPeriodStartTbx.Text = rotationStart.Value.ToString("d");


            DateTime? rotationFinish = _charge.DateOfRotationFinish;
            //if (rotationFinish.HasValue)
            //    AdvertisingBannerRotationPeriod.EndDate = rotationFinish.Value;
            //else
            //    AdvertisingBannerRotationPeriod.SetEmptyStartDate();
            if (rotationFinish.HasValue)
                abRotationPeriodFinishTbx.Text = rotationFinish.Value.ToString("d");

            if (_charge.SpaceId <= 0)
            {
                if (abSpace.Items.Count > 0)
                    abSpace.SelectedIndex = 0;
            }
            else
            {
                ListItem item = abSpace.Items.FindByValue(_charge.SpaceId.ToString());
                if (item != null)
                    item.Selected = true;
            }

            abWeight.Text = _charge.Weight.ToString();

            ListItem contentTypeItem = abContentType.Items.FindByValue(_charge.ContentType.ToString("d"));
            
            if (contentTypeItem != null)
                contentTypeItem.Selected = true;

            abName.Text = _charge.Name;
            abDescription.Text = _charge.Description;
            abXmlId.Text = _charge.XmlId;
            if (_charge.ContentType == BXAdvertisingBannerContentType.Image)
                abImageContentFile.FileId = _charge.ContentFileId;
            else if (_charge.ContentType == BXAdvertisingBannerContentType.Flash)
            {
                abFlashVersion.Text = _charge.FlashVersion;
                abFlashContentFile.FileId = _charge.ContentFileId;
                abFlashAltImageFile.FileId = _charge.FlashAltImageFileId;
            }
            else if (_charge.ContentType == BXAdvertisingBannerContentType.Silverlight)
            {
                abSilverlightVersion.Text = _charge.FlashVersion;
                abSLContentFile.FileId = _charge.ContentFileId;
                abSLAltImageFile.FileId = _charge.FlashAltImageFileId;
            }
            
            //abFlashDynamicCreation.Checked = _charge.FlashDynamicCreation;
            

            
            //abFlashUseCustomUrl.Checked = _charge.FlashUseCustomUrl;
            abHtmlCode.StartMode = _charge.TextContentType == BXAdvertisingBannerTextContentType.Plain ? BXWebEditor.StartModeType.PlainText : BXWebEditor.StartModeType.HTMLVisual;
            abHtmlCode.Content = _charge.TextContent;

            abLinkUrl.Text = _charge.LinkUrl;
            LinkTargetFromCharge(BXAdvertisingBannerContentType.Image);
            abToolTip.Text = _charge.ToolTip;

            abFlashLinkUrl.Text = _charge.LinkUrl;
            LinkTargetFromCharge(BXAdvertisingBannerContentType.Flash);
            abFlashToolTip.Text = _charge.ToolTip;

            abRotationWeekSchedule.HourSpans.Clear();
            BXAdvertisingBannerWeekScheduleHourSpanCollection bannerHourSpans = _charge.RotationHourSpans;
            if(bannerHourSpans.Count > 0)
            {
                BXWeekScheduleHourSpanControl[] hourSpanControls = new BXWeekScheduleHourSpanControl[bannerHourSpans.Count];
                for (int i = 0; i < bannerHourSpans.Count; i++)
                    abRotationWeekSchedule.HourSpans.Add(new BXWeekScheduleHourSpanControl(bannerHourSpans[i].Item));
            }
            else if (_charge.IsNew)
                abRotationWeekSchedule.HourSpans.Add(new BXWeekScheduleHourSpanControl(new BXWeekScheduleHourSpan(0, 168)));

            ListItemCollection siteItems = abSites.Items;
            string[] siteIds = _charge.GetSiteIds();
            foreach (string siteId in siteIds)
            {
                ListItem li = siteItems.FindByValue(siteId);
                if (li != null)
                    li.Selected = true;
            }
            StringBuilder bannerUrlTemplatePermitted = new StringBuilder(),
                bannerUrlTemplateNotPermitted = new StringBuilder();
            BXAdvertisingBannerUrlTemplateCollection bannerUrlTemplates = _charge.RotationUrlTemplates;
            foreach (BXAdvertisingBannerUrlTemplate bannerUrlTemplate in bannerUrlTemplates)
            {
                if(bannerUrlTemplate.IsPermitted)
                    bannerUrlTemplatePermitted.AppendLine(bannerUrlTemplate.UrlTemplate);
                else
                    bannerUrlTemplateNotPermitted.AppendLine(bannerUrlTemplate.UrlTemplate);
            }
            AdvertisingPermittedForRotationUrlTemplates.Text = bannerUrlTemplatePermitted.ToString();
            AdvertisingNotPermittedForRotationUrlTemplates.Text = bannerUrlTemplateNotPermitted.ToString();

            ListItemCollection userRolesItems = abUserRoles.Items;
            int[] userRoleIds = _charge.GetVisitorRoleIds();
            foreach (int userRoleId in userRoleIds)
            {
                ListItem li = userRolesItems.FindByValue(userRoleId.ToString());
                if (li != null)
                    li.Selected = true;
            }

            EnableRotationForVisitorRoles.Items.FindByValue(_charge.EnableRotationForVisitorRoles ? "Y" : "N").Selected = true;

            abEnableFixedRotation.Checked = _charge.IsNew ? true : _charge.EnableFixedRotation;
            //abEnableUniformRotationVelocity.Checked = _charge.EnableUniformRotationVelocity;
            RotationModeFromChange();
            if (_charge.MaxVisitorCount != 0)
                abMaxVisitorCount.Text = _charge.MaxVisitorCount.ToString();
            if (_charge.MaxDisplayCountPerVisitor != 0)
                abMaxDisplayCountPerVisitor.Text = _charge.MaxDisplayCountPerVisitor.ToString();
            if (_charge.MaxDisplayCount != 0)
                abMaxDisplayCount.Text = _charge.MaxDisplayCount.ToString();

            abEnableRedirectionCount.Checked = _charge.EnableRedirectionCount;
            if (_charge.MaxRedirectionCount != 0)
                abMaxRedirectionCount.Text = _charge.MaxRedirectionCount.ToString();
            if (_charge.ContentFileId > 0)
            {
                hfSLWidth.Value = _charge.ContentFile.Width.ToString();
                hfSLHeight.Value = _charge.ContentFile.Height.ToString();
            }
        }
        else
        {
            _charge.Code = abCode.Text;
            _charge.Active = abActive.Checked;

            //if (!AdvertisingBannerRotationPeriod.IsStartDateEmpty())
            //    _charge.DateOfRotationStart = AdvertisingBannerRotationPeriod.StartDate;
            //else
            //    _charge.DateOfRotationStart = null;

            //DateTime dt;
            //if (!string.IsNullOrEmpty(abRotationPeriodStartTbx.Text.Trim()) && DateTime.TryParse(abRotationPeriodStartTbx.Text.Trim(), out dt))
            //    _charge.DateOfRotationStart = dt;
           // else
           //     _charge.DateOfRotationStart = null;

            if (!string.IsNullOrEmpty(abRotationPeriodStartTbx.Text.Trim()))
            {
                DateTime startDate;
                if (DateTime.TryParse(abRotationPeriodStartTbx.Text.Trim(), out startDate))
                    _charge.DateOfRotationStart = startDate;
                else
                {
                    _errorMessage = string.Format(GetMessage("Message.CouldnotParseDate"), abRotationPeriodStartTbx.Text.Trim());
                    _editorError = EditorMode == BXAdminPageEditorMode.Creation ? AdvertisingBannerEditorError.Creation : AdvertisingBannerEditorError.Modification;
                    return;
                }
            }
            else
                _charge.DateOfRotationStart = null;

            if (!string.IsNullOrEmpty(abRotationPeriodFinishTbx.Text.Trim()))
            {
                DateTime finishDate;
                if (DateTime.TryParse(abRotationPeriodFinishTbx.Text.Trim(), out finishDate))
                    _charge.DateOfRotationFinish = finishDate;
                else
                {
                    _errorMessage = string.Format(GetMessage("Message.CouldnotParseDate"), abRotationPeriodFinishTbx.Text.Trim());
                    _editorError = EditorMode == BXAdminPageEditorMode.Creation ? AdvertisingBannerEditorError.Creation : AdvertisingBannerEditorError.Modification;
                    return;
                }
            }
            //DateTime dt;
            //if (!AdvertisingBannerRotationPeriod.IsEndDateEmpty())
            //    _charge.DateOfRotationFinish = AdvertisingBannerRotationPeriod.EndDate;
            //else
            //    _charge.DateOfRotationFinish = null;
            //if (!string.IsNullOrEmpty(abRotationPeriodFinishTbx.Text.Trim()) && DateTime.TryParse(abRotationPeriodFinishTbx.Text.Trim(), out dt))
            //    _charge.DateOfRotationFinish = dt;
            //else
            //    _charge.DateOfRotationFinish = null;

            if (abSpace.SelectedItem == null)
                _charge.SpaceId = 0;
            else
            {
                try
                {
                    _charge.SpaceId = Convert.ToInt32(abSpace.SelectedItem.Value);
                }
                catch (Exception /*exception*/)
                {
                    _charge.SpaceId = 0;
                }
            }

            try
            {
                _charge.Weight = Convert.ToInt32(abWeight.Text);
            }
            catch (Exception /*exc*/) 
            {
                _charge.Weight = BXAdvertisingBanner.DefaultWeightValue;
            }

            _charge.Name = abName.Text;
            _charge.Description = abDescription.Text;
            _charge.XmlId = abXmlId.Text;

            if (!_charge.IsNew)
            {
                bool aboutContentFileDeletion = false;
                if (_charge.ContentType == BXAdvertisingBannerContentType.Image)
                    aboutContentFileDeletion = abImageContentFile.AboutFileDeletion;
                else if (_charge.ContentType == BXAdvertisingBannerContentType.Flash)
                    aboutContentFileDeletion = abFlashContentFile.AboutFileDeletion;
                else if (_charge.ContentType == BXAdvertisingBannerContentType.Silverlight)
                    aboutContentFileDeletion = abSLContentFile.AboutFileDeletion;

                if (aboutContentFileDeletion)
                {
                    BXFile contentFile = _charge.ContentFile;
                    if (contentFile != null)
                        _charge.ContentFile = null;
                }
            }

            //BXFile contentFile = abImageContentFile.File;
            //if (contentFile != null)
            //{
            //    _charge.ContentFile = contentFile;
            //    if (contentFile.ContentType.ToUpperInvariant().StartsWith("IMAGE", StringComparison.InvariantCulture))
            //         contentType = BXAdvertisingBannerContentType.Image;
            //     else if (string.Equals(contentFile.ContentType.ToUpperInvariant(), "APPLICATION/X-SHOCKWAVE-FLASH", StringComparison.InvariantCulture))
            //         contentType = BXAdvertisingBannerContentType.Flash;  
            //}
            int flashVer = 0;
            string sVer = String.Empty;
            if (abContentType.SelectedItem != null)
            {
                BXAdvertisingBannerContentType? userContentType = null;
                try
                {
                    userContentType = (BXAdvertisingBannerContentType)Enum.Parse(typeof(BXAdvertisingBannerContentType), abContentType.SelectedItem.Value);
                }
                catch (Exception /*exception*/)
                {
                }

                if (userContentType.HasValue)
                {
                    BXFile contentFile = null;
                    if (userContentType.Value == BXAdvertisingBannerContentType.Image)
                    {
                        contentFile = abImageContentFile.File;
                        _charge.FlashWMode = abFlashWMode.Text;
                        if (contentFile != null)
                        {
                            if (!contentFile.ContentType.ToUpperInvariant().StartsWith("IMAGE", StringComparison.InvariantCulture))
                            {
                                contentFile = null;
                                _errorMessage = GetMessage("Message.ContentTypeContradictFile");
                                _editorError = EditorMode == BXAdminPageEditorMode.Creation ? AdvertisingBannerEditorError.Creation : AdvertisingBannerEditorError.Modification;
                                abImageContentFile.FileId = _charge.ContentFileId;
                            }
                            if (contentFile != null)
                            {
                                contentFile.Save();
                                abImageContentFile.FileId  = contentFile.Id;
                            }
                        }
                    }
                    else if (userContentType.Value == BXAdvertisingBannerContentType.Flash)
                    {
                        _charge.FlashWMode = abFlashWMode.Text;
                        contentFile = abFlashContentFile.File;
                        if (contentFile != null)
                        {
                            if (!contentFile.ContentType.ToUpperInvariant().StartsWith("APPLICATION/X-SHOCKWAVE-FLASH", StringComparison.InvariantCulture))
                            {
                                contentFile = null;
                                _errorMessage = GetMessage("Message.ContentTypeContradictFile");
                                _editorError = EditorMode == BXAdminPageEditorMode.Creation ? AdvertisingBannerEditorError.Creation : AdvertisingBannerEditorError.Modification;
                                abFlashContentFile.FileId = _charge.ContentFileId;
                            }
                            if (contentFile != null)
                            {
                                contentFile.Save();
                                abFlashContentFile.FileId = contentFile.Id;
                            }
                        }
                        try
                        {
                            flashVer = Convert.ToInt32(abFlashVersion.Text);
                            _charge.FlashVersion = flashVer.ToString();
                        }
                        catch (Exception /*exc*/)
                        {
                            _charge.FlashVersion = string.Empty;
                        }

                    }
                    else if (userContentType.Value == BXAdvertisingBannerContentType.Silverlight)
                    {
                        contentFile = abSLContentFile.File;
                        if (contentFile != null)
                        {
                            if (!contentFile.ContentType.ToUpperInvariant().StartsWith("APPLICATION/OCTET-STREAM", StringComparison.InvariantCulture))
                            {
                                contentFile = null;
                                _errorMessage = GetMessage("Message.ContentTypeContradictFile");
                                _editorError = EditorMode == BXAdminPageEditorMode.Creation ? AdvertisingBannerEditorError.Creation : AdvertisingBannerEditorError.Modification;
                                abSLContentFile.FileId = _charge.ContentFileId;
                            }
                            if (contentFile != null)
                            {
                                contentFile.Save();
                                abSLContentFile.FileId = contentFile.Id;
                            }


                        }

                        _charge.FlashVersion = abSilverlightVersion.Text;
                    }

                    if (contentFile != null)
                        _charge.ContentFile = contentFile;
                    else
                    {
                        if (_charge.ContentType != userContentType.Value)
                            _charge.ContentFile = null;
                        _charge.ContentType = userContentType.Value;
                    }
                }
            }

            _charge.TextContentType = abHtmlCode.StartMode == BXWebEditor.StartModeType.HTMLVisual ? BXAdvertisingBannerTextContentType.Html : BXAdvertisingBannerTextContentType.Plain;

            
            //_charge.FlashDynamicCreation = abFlashDynamicCreation.Checked;





            if (abFlashAltImageFile.AboutFileDeletion || abSLAltImageFile.AboutFileDeletion)
                _charge.FlashAltImageFile = null;
            else if (abFlashAltImageFile.File != null)
                _charge.FlashAltImageFile = abFlashAltImageFile.File;
            else if (abSLAltImageFile.File != null)
                _charge.FlashAltImageFile = abSLAltImageFile.File;
            //_charge.FlashUseCustomUrl = abFlashUseCustomUrl.Checked;
           
            _charge.TextContent = abHtmlCode.Content;

            switch (_charge.ContentType)
            {
                case BXAdvertisingBannerContentType.Image:
                    _charge.LinkUrl = abLinkUrl.Text;
                    _charge.ToolTip = abToolTip.Text;
                    break;
                case BXAdvertisingBannerContentType.Flash:
                    _charge.LinkUrl = abFlashLinkUrl.Text;
                    _charge.ToolTip = abFlashToolTip.Text;
                    break;
                case BXAdvertisingBannerContentType.Silverlight:
                    _charge.LinkUrl = abFlashLinkUrl.Text;
                    _charge.ToolTip = abFlashToolTip.Text;
                    break;
            }

            LinkTargetToCharge();

            _charge.RotationHourSpans.Clear();
            foreach (BXWeekScheduleHourSpanControl hourSpan in abRotationWeekSchedule.HourSpans)
                _charge.RotationHourSpans.Add(new BXAdvertisingBannerWeekScheduleHourSpan(hourSpan.Item));

            List<string> siteIdList = new List<string>();
            ListItemCollection siteItems = abSites.Items;
            foreach (ListItem siteItem in siteItems)
            {
                if (!siteItem.Selected)
                    continue;
                siteIdList.Add(siteItem.Value);
            }
            _charge.SetSiteIds(siteIdList.ToArray());

            _charge.RotationUrlTemplates.Clear();
            _charge.RotationUrlTemplates.AddRange(LoadUrlTemplateArrayFromString(AdvertisingPermittedForRotationUrlTemplates.Text, true));
            _charge.RotationUrlTemplates.AddRange(LoadUrlTemplateArrayFromString(AdvertisingNotPermittedForRotationUrlTemplates.Text, false));

            List<int> roleIdList = new List<int>();
            ListItemCollection userRoleItems = abUserRoles.Items;
            foreach (ListItem userRoleItem in userRoleItems)
            {
                if (!userRoleItem.Selected)
                    continue;
                roleIdList.Add(Convert.ToInt32(userRoleItem.Value));
            }
            _charge.SetVisitorRoleIds(roleIdList.ToArray());
            _charge.EnableRotationForVisitorRoles = EnableRotationForVisitorRoles.SelectedItem != null ? string.Equals(EnableRotationForVisitorRoles.SelectedItem.Value, "Y", StringComparison.Ordinal) : false;

            _charge.EnableFixedRotation = abEnableFixedRotation.Checked;
            //_charge.EnableUniformRotationVelocity = abEnableUniformRotationVelocity.Checked;
            RotationModeToCharge();

            if (_charge.EnableFixedRotation)
            {
                try
                {
                    int val = Convert.ToInt32(abMaxDisplayCount.Text);
                    _charge.MaxDisplayCount = val >= 0 ? val : 0;
                }
                catch (Exception/*exc*/)
                {
                    _charge.MaxDisplayCount = 0;
                }

                try
                {
                    int val = Convert.ToInt32(abMaxVisitorCount.Text);
                    _charge.MaxVisitorCount = val >= 0 ? val : 0;
                }
                catch (Exception/*exc*/)
                {
                    _charge.MaxVisitorCount = 0;
                }

                try
                {
                    int val = Convert.ToInt32(abMaxDisplayCountPerVisitor.Text);
                    _charge.MaxDisplayCountPerVisitor = val >= 0 ? val : 0;
                }
                catch (Exception/*exc*/)
                {
                    _charge.MaxDisplayCountPerVisitor = 0;
                }
            }

            _charge.EnableRedirectionCount = abEnableRedirectionCount.Checked;
            if (_charge.EnableRedirectionCount)
            {
                try
                {
                    int val = Convert.ToInt32(abMaxRedirectionCount.Text);
                    _charge.MaxRedirectionCount = val >= 0 ? val : 0;
                }
                catch (Exception/*exc*/)
                {
                    _charge.MaxRedirectionCount = 0;
                }
            }
        }
	}

    private IList<BXAdvertisingBannerUrlTemplate> LoadUrlTemplateArrayFromString(string source, bool isPermitted) 
    {
        List<BXAdvertisingBannerUrlTemplate> resultList = new List<BXAdvertisingBannerUrlTemplate>();
        if (!string.IsNullOrEmpty(source))
        {
            int banerId = _charge != null ? _charge.Id : 0;
            string[] itemStrArray = source.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string itemStr in itemStrArray)
            {
                BXAdvertisingBannerUrlTemplate item = new BXAdvertisingBannerUrlTemplate(banerId, isPermitted);
                try
                {
                    item.UrlTemplate = itemStr;
                }
                catch (Exception /*exc*/)
                {
                }

                if (string.IsNullOrEmpty(item.UrlTemplate))
                    continue;
                resultList.Add(item);
            }
        }
        return resultList.ToArray();
    }
    private void LinkTargetToCharge()
    {
        if (_charge == null)
            throw new InvalidOperationException("Could not find charge!");

        RadioButton sef = null, blank = null, parent = null, top = null, custom = null;
        TextBox text = null;

        if (_charge.ContentType == BXAdvertisingBannerContentType.TextOnly)
            return;
        else if (_charge.ContentType == BXAdvertisingBannerContentType.Image)
        {
            sef = abLinkTargetButtonSef;
            blank = abLinkTargetButtonBlank;
            /*parent = abLinkTargetButtonParent;
            top = abLinkTargetButtonTop;
            custom = abLinkTargetButtonCustom;
            text = abLinkTargetCustomText;*/
        }
        else if (_charge.ContentType == BXAdvertisingBannerContentType.Flash)
        {
            sef = abFlashLinkTargetButtonSelf;
            blank = abFlashLinkTargetButtonBlank;
            /*parent = abFlashLinkTargetButtonParent;
            top = abFlashLinkTargetButtonTop;
            custom = abFlashLinkTargetButtonCustom;
            text = abFlashLinkTargetCustomText;*/
        }
        else if (_charge.ContentType == BXAdvertisingBannerContentType.Silverlight)
        {
            sef = abFlashLinkTargetButtonSelf;
            blank = abFlashLinkTargetButtonBlank;
        }


        if (blank.Checked)
            _charge.LinkTarget = "_blank";
        /*else if (parent.Checked)
            _charge.LinkTarget = "_parent";
        else if (top.Checked)
            _charge.LinkTarget = "_top";
        else if (custom.Checked)
            _charge.LinkTarget = text.Text;*/ 
        else
            _charge.LinkTarget = "_self";
    }

    private void LinkTargetFromCharge(BXAdvertisingBannerContentType type)
    {
        if (_charge == null)
            throw new InvalidOperationException("Could not find charge!");

        RadioButton sef = null, blank = null, parent = null, top = null, custom = null;
        TextBox text = null;

        if (type == BXAdvertisingBannerContentType.TextOnly)
            return;
        else if (type == BXAdvertisingBannerContentType.Image)
        {
            sef = abLinkTargetButtonSef;
            blank = abLinkTargetButtonBlank;
            /*parent = abLinkTargetButtonParent;
            top = abLinkTargetButtonTop;
            custom = abLinkTargetButtonCustom;
            text = abLinkTargetCustomText;*/
        }
        else if (type == BXAdvertisingBannerContentType.Flash)
        {
            sef = abFlashLinkTargetButtonSelf;
            blank = abFlashLinkTargetButtonBlank;
            /*parent = abFlashLinkTargetButtonParent;
            top = abFlashLinkTargetButtonTop;
            custom = abFlashLinkTargetButtonCustom;
            text = abFlashLinkTargetCustomText;*/
        }

        if (string.Equals(_charge.LinkTarget, "_blank", StringComparison.InvariantCulture))
            blank.Checked = true;
        else
            sef.Checked = true;
        /*else if (string.Equals(_charge.LinkTarget, "_parent", StringComparison.InvariantCulture))
            parent.Checked = true;
        else if (string.Equals(_charge.LinkTarget, "_top", StringComparison.InvariantCulture))
            top.Checked = true;
        else
        {
            custom.Checked = true;
            text.Text = _charge.LinkTarget;
        }*/
    }

    private void RotationModeToCharge()
    {
        if (_charge == null)
            throw new InvalidOperationException("Could not find charge!");

        _charge.EnableUniformRotationVelocity = abRotationModeUniform.Checked;
    }

    private void RotationModeFromChange() 
    {
        abRotationModeStandard.Checked = !_charge.EnableUniformRotationVelocity;
        abRotationModeUniform.Checked = _charge.EnableUniformRotationVelocity;
    }

    private void TrySaveAdvertisingBanner()
    {
        if (_charge == null)
            return;

        if (_editorError != AdvertisingBannerEditorError.None)
            return;



        try
        {
            BXAdvertisingBannerContentType? fileContentType = _charge.GetContentFileType();

            if (fileContentType.HasValue && fileContentType.Value != _charge.ContentType)
            {
                _errorMessage = GetMessage("Message.ContentTypeContradictFile");
                _editorError = EditorMode == BXAdminPageEditorMode.Creation ? AdvertisingBannerEditorError.Creation : AdvertisingBannerEditorError.Modification;
                return;
            }
            if (fileContentType.HasValue && fileContentType.Value == BXAdvertisingBannerContentType.Silverlight)
            {
                    BXFile file = BXFile.GetById(_charge.ContentFile.Id);
                    
                    int w;
                    if ( int.TryParse(Request.Form[abSLContentFile.ClientID + "_slheight"],out w ))
                        file.Height = w;
                    
                    if ( int.TryParse(Request.Form[abSLContentFile.ClientID + "_slwidth"],out w ))
                        file.Width = w;
                    if (file.Width == 0 || file.Height == 0)
                    {
                        _charge.ContentFile = null;
                        throw new Exception(GetMessage("SilverlightFileError.ZeroSize"));
                    }
                    file.Save();
                
            }
            BXUser user = BXIdentity.Current != null ? BXIdentity.Current.User : null;
            if (user != null)
            {
                if (EditorMode == BXAdminPageEditorMode.Creation)
                    _charge.AuthorId = user.UserId;
                _charge.LastModificationAuthorId = user.UserId;
            }
            
            _charge.Save();
        }
        catch (Exception exc)
        {
            _errorMessage = exc.Message;
            _editorError = EditorMode == BXAdminPageEditorMode.Creation ? AdvertisingBannerEditorError.Creation : AdvertisingBannerEditorError.Modification; 
        }
    }

    protected void OnResetDisplayCountButtonClick(Object sender, EventArgs e)
    {
        if (!IsValid)
            return;

        if (_editorError != AdvertisingBannerEditorError.None)
            return;

        if (_charge == null)
            return;
        _charge.DisplayCount = 0;
        _charge.DateOfFirstDisplay = null;
        _charge.DateOfLastDisplay = null;
        TrySaveAdvertisingBanner();
    }

    protected void OnResetVisitorCountButtonClick(Object sender, EventArgs e)
    {
        if (!IsValid)
            return;

        if (_editorError != AdvertisingBannerEditorError.None)
            return;

        if (_charge == null)
            return;

        _charge.VisitorCount = 0;
        TrySaveAdvertisingBanner();
    }

    protected void OnResetRedirectionCountButtonClick(Object sender, EventArgs e)
    {
        if (!IsValid)
            return;

        if (_editorError != AdvertisingBannerEditorError.None)
            return;

        if (_charge == null)
            return;

        _charge.RedirectionCount = 0;
        TrySaveAdvertisingBanner();
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

    protected bool IsChargeInRotation
    {
        get { return _charge.IsInRotation; }
    }

    protected string ChargeRotationLegend
    {
        get 
        {
            BXAdvertisingBannerRotationStopReason reason = _charge.GetRotationStopReason();
            if (reason == BXAdvertisingBannerRotationStopReason.None)
                return string.Concat(GetMessage("DisplayState.IsDisplayed"), ".");

            StringBuilder sb = new StringBuilder();
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.Active, reason, GetMessage("RotationStopReason.Active"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.PeriodStart, reason, GetMessage("RotationStopReason.RotationPeriodStart"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.PeriodFinish, reason, GetMessage("RotationStopReason.RotationPeriodFinish"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.DisplayLimit, reason, GetMessage("RotationStopReason.DisplayLimit"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.VistorLimit, reason, GetMessage("RotationStopReason.VistorLimit"), sb);
            AddRotationStopReason(BXAdvertisingBannerRotationStopReason.RedirectionLimit, reason, GetMessage("RotationStopReason.RedirectionLimit"), sb);
            return sb.Insert(0, " (").Insert(0, GetMessage("DisplayState.IsNotDisplayed")).Append(").").ToString();
        }
    }

    protected override void OnPreRender(EventArgs e)
    {
        if (EditorMode != BXAdminPageEditorMode.Modification)
        {
            AddButton.Visible = false;
            DeleteButton.Visible = false;
        }

        BXAdvertisingBannerContentType contentType = (Charge == null ? BXAdvertisingBannerContentType.Image : Charge
            .ContentType);

        // fields visibilty management

        switch (contentType)
        {
            case BXAdvertisingBannerContentType.Image:
                advertisingBannerContentFileContainer.Style.Add("display", "");
                advertisingBannerLinkUrlContainer.Style.Add("display", "");
                advertisingBannerLinkTargetContainer.Style.Add("display", "");
                advertisingBannerToolTipContainer.Style.Add("display", "");
                break;
            case BXAdvertisingBannerContentType.Flash:
                advertisingBannerFlashWModeContainer.Style.Add("display", "");
                advertisingBannerFlashVersionContainer.Style.Add("display", "");
                advertisingBannerFlashAltImageFileContainer.Style.Add("display", "");
                advertisingBannerFlashLinkUrlContainer.Style.Add("display", "");
                advertisingBannerFlashLinkTargetContainer.Style.Add("display", "");
                advertisingBannerFlashToolTipContainer.Style.Add("display", "");
                advertisingBannerFlashContentFileContainer.Style.Add("display", "");
                break;
            case BXAdvertisingBannerContentType.Silverlight:
                advertisingBannerSLVersionContainer.Style.Add("display", "");
                advertisingBannerSLAltImageFileContainer.Style.Add("display", "");
                advertisingBannerFlashToolTipContainer.Style.Add("display", "");
                advertisingBannerFlashLinkTargetContainer.Style.Add("display", "");
                advertisingBannerSLContentFileContainer.Style.Add("display", "");
                advertisingBannerFlashLinkUrlContainer.Style.Add("display", "");
                break;
            case BXAdvertisingBannerContentType.TextOnly:
                advertisingBannerHtmlCodeContainer.Style.Add("display", "");

                break;
        }

        if (EditorError != AdvertisingBannerEditorError.None)
        {
            errorMessage.AddErrorMessage(ErrorMessage);
            if (EditorMode == BXAdminPageEditorMode.Modification)
            {
                if (EditorError == AdvertisingBannerEditorError.IsNotFound)
                    TabControl.Visible = false;
            }
        }


        abResetDisplayCountBtn.Text = GetMessage("Legend.Limitation.ResetCurrentValue");
        abResetVisitorCountBtn.Text = GetMessage("Legend.Limitation.ResetCurrentValue");
        abResetRedirectionCountBtn.Text = GetMessage("Legend.Limitation.ResetCurrentValue");


        if (_charge != null)
        {
            //BXAdvertisingBannerRotationStopReason reason = BXAdvertisingManager.GetBannerRotationState(_charge);

            //abDisplayState.Text = reason == BXAdvertisingBannerRotationStopReason.None ? GetMessage("DisplayState.IsDisplayed") : GetMessage("DisplayState.IsNotDisplayed");
            //if (reason != BXAdvertisingBannerRotationStopReason.None)
            //{
            //    StringBuilder sb = new StringBuilder();
            //    AddRotationStopReason(BXAdvertisingBannerRotationStopReason.Active, reason, GetMessage("RotationStopReason.Active"), sb);
            //    AddRotationStopReason(BXAdvertisingBannerRotationStopReason.PeriodStart, reason, GetMessage("RotationStopReason.RotationPeriodStart"), sb);
            //    AddRotationStopReason(BXAdvertisingBannerRotationStopReason.PeriodFinish, reason, GetMessage("RotationStopReason.RotationPeriodFinish"), sb);
            //    AddRotationStopReason(BXAdvertisingBannerRotationStopReason.DisplayLimit, reason, GetMessage("RotationStopReason.DisplayLimit"), sb);
            //    AddRotationStopReason(BXAdvertisingBannerRotationStopReason.VistorLimit, reason, GetMessage("RotationStopReason.VistorLimit"), sb);
            //    AddRotationStopReason(BXAdvertisingBannerRotationStopReason.RedirectionLimit, reason, GetMessage("RotationStopReason.RedirectionLimit"), sb);
            //    abRotationStopReason.Text = sb.ToString();
            //}

            abDisplayCount.Text = _charge.DisplayCount > 0 ? _charge.DisplayCount.ToString() : "-";
            abVisitorCount.Text = _charge.VisitorCount > 0 ? _charge.VisitorCount.ToString() : "-";
            abRedirectionCount.Text = _charge.RedirectionCount > 0 ? _charge.RedirectionCount.ToString() : "-";

            abCTR.Text = _charge.ClickThroughRation.ToString("0.##");
            if (_charge.DisplayCount == 0)
               abResetDisplayCountBtn.Visible = false;

            if (_charge.VisitorCount == 0)
                abResetVisitorCountBtn.Visible = false;

            if (_charge.RedirectionCount == 0)
                abResetRedirectionCountBtn.Visible = false;
        }
        RegisterScriptInclude(this, "~/bitrix/js/Main/utils_net.js");
#if DEBUG 
        RegisterScriptInclude(this, "~/bitrix/js/main/silverlight.debug.js");
#else
        RegisterScriptInclude(this, "~/bitrix/js/main/silverlight.js");
#endif
        base.OnPreRender(e);
    }


    protected void OnAdvertisingBannerEdit(object sender, BXTabControlCommandEventArgs e)
	{

		switch (e.CommandName)
		{
            case "save":
                {
                    if (IsValid)
                    {
                        TrySaveAdvertisingBanner();
                        if (EditorError == AdvertisingBannerEditorError.None)
                            GoBack(); 
                    }
                }
                break;
            case "apply":
                {
                    if (IsValid)
                    {
                        TrySaveAdvertisingBanner();
                        if (EditorError == AdvertisingBannerEditorError.None)
                        {
                            ChargeId = _charge.Id;
                            Response.Redirect(string.Format("AdvertisingBannerEdit.aspx?id={0}&tabindex={1}", ChargeId.ToString(), TabControl.SelectedIndex));
                        }
                    }
                }
                break;
			case "cancel":
				GoBack();
				break;
		}
	}

	protected void OnToolBarButtonClick(object sender, CommandEventArgs e)
	{
        if (!IsValid)
            return;

		if (e.CommandName == "delete")
		{
			try
			{
                BXAdvertisingBanner charge = Charge;
                if (charge != null)
                    charge.Delete();

				GoBack();
			}
			catch (Exception ex)
			{
                _errorMessage = ex.Message;
                _editorError = AdvertisingBannerEditorError.Deleting;
			}
		}
	}

	protected override void PrepareDialogContent(BXDialogData data)
	{
		if(data == null)
			throw new ArgumentNullException("data");

		BXDialogSectionData content = data.CreateSection(BXDialogSectionType.Content);
		TabControl.PublicMode = true;
		TabControl.ButtonsMode = BXTabControl.ButtonsModeType.Hidden;
		ContextMenuToolbar.Visible = false;
		content.CreateItemFromControl(this);
	}

	protected override bool ValidateDialogData()
	{
		Validate(TabControl.ValidationGroup);
		return IsValid;
	}

	protected override void SaveDialogData()
	{
		TrySaveAdvertisingBanner();
		if(EditorError == AdvertisingBannerEditorError.None)
			Refresh(string.Empty, BXDialogGoodbyeWindow.LayoutType.Success, 0);
	}
}
