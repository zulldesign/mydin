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
using Bitrix.Components;
using Bitrix.DataLayer;
using System.Collections.Generic;
using Bitrix.Security;
using System.Text;
using System.Text.RegularExpressions;
using Bitrix.IO;
using Bitrix.DataTypes;
using Bitrix.Services.Js;
using System.Collections.Specialized;
using Bitrix.Services.Text;
using Bitrix.Configuration;
using Bitrix.UI.Hermitage;
using Bitrix.Services;

namespace Bitrix.Advertising.Components
{
    /// <summary>
    /// Параметр компонента "Включаемая область"
    /// </summary>
    public enum BannerComponentParameter
    {
        /// <summary>
        /// Код Рекламной области
        /// </summary>
        Space = 2
    }

    public partial class AdvertisingBannerComponent : BXComponent
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(!BXAdvertisingManager.IsRobotRequest)
                IncludeComponentTemplate();
        }


        private static readonly object _waitingForDisplayBatchScriptKey = new object();
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            BXAdvertisingBanner banner = Banner;
            if (banner == null || BXAdvertisingManager.IsRobotRequest)
                return;

            //Регистрация отображения (не регистрируем показ в режиме редактирования страницы)
            if (banner.EnableFixedRotation && IsBannerDisplayed && !IsComponentDesignMode)
            {
                string batchCode = BXAdvertisingManager.AddToWaitingForDisplayBatch(banner.Id);
                if (!string.IsNullOrEmpty(batchCode) && !Context.Items.Contains(_waitingForDisplayBatchScriptKey))
                {
                    string srcAttrEnc = HttpUtility.HtmlAttributeEncode(string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/handlers/Advertising/RegBannerDisplay.ashx"), "?code=", batchCode));
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<script type=\"text/javascript\">document.write('<div style=\"display:none;\"><iframe src=\"").Append(srcAttrEnc).Append("\" width=\"0\" height=\"0\" style=\"margin: 0; border: 0\"></iframe></div>');</script>");
                    ScriptManager sm;
                    if ((sm = ScriptManager.GetCurrent(Page)) == null || !sm.IsInAsyncPostBack)
                        sb.Append("<noscript><div style=\"display:none;\"><iframe src=\"").Append(srcAttrEnc).Append("\" width=\"0\" height=\"0\" style=\"margin: 0; border: 0\"></iframe></div></noscript>");
                    ScriptManager.RegisterStartupScript(
                        this,
                        GetType(),
                        ClientID + "_WaitingForDisplayBatch",
                        sb.ToString(),
                        false
                        );
                    Context.Items[_waitingForDisplayBatchScriptKey] = new object();
                }
            }

            if(banner.ContentType == BXAdvertisingBannerContentType.Flash)
                BXPage.RegisterScriptInclude("~/bitrix/js/Main/utils_net.js");
        }

        private bool _isBannerDisplayed = false;
        public bool IsBannerDisplayed
        {
            get { return _isBannerDisplayed; }
            set { _isBannerDisplayed = value; }
        }

        public string PrepareClientRedirectionUrl(string redirectionUrl)
        {
            BXAdvertisingBanner banner = Banner;
            if (banner == null)
                return string.Empty;

            string url = !string.IsNullOrEmpty(redirectionUrl) ? redirectionUrl : banner.LinkUrl;
            string code = string.Empty;
            if (!banner.EnableRedirectionCount || string.IsNullOrEmpty(code = BXAdvertisingManager.AddToWaitingForRedirection(banner.Id, url)))
                return url;
            return string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/Handlers/Advertising/RegBannerRedirection.ashx"), "?", RedirectionUrlQueryName, "=", HttpUtility.UrlEncode(url), "&", RedirectionCodeQueryName, "=", HttpUtility.UrlEncode(code));
        }

        private static readonly Regex _rxHtmlLinkUrl = new Regex("<a[^>]+?href\\s*=\\s*(?:\"|\')?(?<url>.+?)(?:\"|\')", RegexOptions.Compiled|RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);

        public string PrepareTextContent()
        {
            BXAdvertisingBanner banner = Banner;
            if (banner == null)
                return string.Empty;

            string contentText = banner.TextContentType == BXAdvertisingBannerTextContentType.Plain ? HttpUtility.HtmlEncode(banner.TextContent) : banner.TextContent;
            if (string.IsNullOrEmpty(contentText))
                return string.Empty;

            if (banner.TextContentType == BXAdvertisingBannerTextContentType.Html && banner.EnableRedirectionCount)
                contentText =  _rxHtmlLinkUrl.Replace(
                    contentText,
                    delegate(Match match)
                    {
                        string text = match.Value;
                        if (string.IsNullOrEmpty(text))
                            return string.Empty;

                        Group g = match.Groups["url"];
                        if (!g.Success || string.IsNullOrEmpty(g.Value)) return match.Value;
                        return text.Replace(g.Value, PrepareClientRedirectionUrl(g.Value));
                    }
                    );
            return contentText;
        }

        private static string GetParameterKey(BannerComponentParameter param)
        {
            return param.ToString("G");
        }

        /// <summary>
        /// Код рекламной области
        /// </summary>
        public string SpaceCode
        {
            get
            {
                object obj = Parameters.Get(GetParameterKey(BannerComponentParameter.Space));
                string result = string.Empty;
                if (obj != null)
                {
                    try
                    {
                        result = Convert.ToString(obj);
                    }
                    catch (Exception /*exc*/)
                    {
                    }
                }
                return result;
            }
            set
            {
                Parameters[GetParameterKey(BannerComponentParameter.Space)] = value.ToString();
            }
        }

        /// <summary>
        /// Имя параметра для кода перенаправления
        /// </summary>
        private string RedirectionCodeQueryName
        {
            get { return "code"; }
        }

        /// <summary>
        /// Имя параметра для Url перехода
        /// </summary>
        private string RedirectionUrlQueryName
        {
            get { return "url"; }
        }

        private int? _bannerId = null;
        public int BannerId
        {
            get 
            {
                if (_bannerId.HasValue)
                    return _bannerId.Value;

                if (BXAdvertisingManager.IsRobotRequest)
                    return 0;

                string spaceCode = SpaceCode;
                if (!string.IsNullOrEmpty(spaceCode))
                {
                    BXIdentity identity = (BXIdentity)(BXPrincipal.Current.Identity);
                    int userId = identity != null && identity.User != null ? identity.User.UserId : 0;
                    _bannerId = BXAdvertisingManager.GetRandomBannerId(spaceCode,
                        userId,
                        DesignerSite,
                        Request.RawUrl,
                        BXAdvertisingManager.GetPersistentDisplayInfoCollection().ToArray()
                        );
                }
                else
                    _bannerId = 0;

                return _bannerId.Value;

            }
        }

        private BXAdvertisingBanner _banner = null;
        private bool _isBannerLoaded = false;
        /// <summary>
        /// Рекламный баннер
        /// </summary>
        public BXAdvertisingBanner Banner
        {
            get 
            {
                if (_isBannerLoaded)
                    return _banner;

                _banner = BannerId > 0 ? BXAdvertisingModule.GetBannerById(BannerId) : null;
                _isBannerLoaded = true;
                return _banner;
            }
        }

        protected override void PreLoadComponentDefinition()
        {
            if (BXAdvertisingManager.IsRobotRequest)
                return;

            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";

            Group = new BXComponentGroup("Advertising", GetMessageRaw("Group"), 100, BXComponentGroup.Service);

            string paramLegendPrefix = "Param.",
                clientSideActionGroupViewId = ClientID;

            string spaceKey = GetParameterKey(BannerComponentParameter.Space);
            ParamsDefinition.Add(
                spaceKey,
                new BXParamSingleSelection(
                    GetMessageRaw(string.Concat(paramLegendPrefix, spaceKey)),
                    string.Empty,
                    BXCategory.Main,
                    null,
                    null
                )
            );
        }

        protected override void LoadComponentDefinition()
        {
            if (BXAdvertisingManager.IsRobotRequest)
                return;

            BXAdvertisingSpaceCollection spaces = BXAdvertisingSpace.GetList(
                null, 
                new BXOrderBy(new BXOrderByPair(BXAdvertisingSpace.Fields.Name, BXOrderByDirection.Asc)),
                new BXSelect(BXSelectFieldPreparationMode.Normal, BXAdvertisingSpace.Fields.Code, BXAdvertisingSpace.Fields.Name), 
                null
                );

            if (spaces.Count > 0)
            {
                BXParamValue[] paramValues = new BXParamValue[spaces.Count];
                for (int i = 0; i < spaces.Count; i++)
                {
                    BXAdvertisingSpace space = spaces[i];
                    paramValues[i] = new BXParamValue(space.TextEncoder.Decode(space.Name), space.Code);
                }
                List<BXParamValue> spaceValues = ParamsDefinition[GetParameterKey(BannerComponentParameter.Space)].Values;
                if (spaceValues == null)
                    ParamsDefinition[GetParameterKey(BannerComponentParameter.Space)].Values = spaceValues = new List<BXParamValue>();
                else if (spaceValues.Count > 0)
                    spaceValues.Clear();

                spaceValues.Add(new BXParamValue(GetMessageRaw("ListItemText.SelectSpace"), string.Empty));
                spaceValues.AddRange(paramValues);
            }
        }

		//Создаем контекстное меню
		public override BXComponentPopupMenuInfo PopupMenuInfo
		{
			get
			{
				if(this.popupMenuInfo != null)
					return this.popupMenuInfo;

				BXPrincipal principal = BXPrincipal.Current;
				BXComponentPopupMenuInfo info = base.PopupMenuInfo;
				if (principal != null && principal.IsCanOperate(BXRoleOperation.Operations.AccessAdminSystem) && principal.IsCanOperate(BXAdvertisingModule.Operations.BannerManagement))
					info.CreateComponentContentMenuItems = 
						delegate(BXShowMode showMode)
						{
							if(showMode == BXShowMode.View)
								return new BXHermitagePopupMenuBaseItem[0];

							BXHermitagePopupMenuBaseItem[] menuItems = new BXHermitagePopupMenuBaseItem[ Banner != null ? 2 : 1];
							if(Banner != null)
							{
								BXHermitagePopupMenuItem editItem = new BXHermitagePopupMenuItem();
								menuItems[0] = editItem;
								editItem.Id = string.Concat("ADVERTISING_BANNER_EDIT_", BannerId);
								editItem.IconCssClass = "bx-context-toolbar-edit-icon";
								editItem.Sort = 10;
								editItem.Text = GetMessageRaw("EditBannerMenuItem");
								editItem.ClientClickScript = string.Format(
										"(new BX.CDialogNet({{ 'content_url':'{0}?id={1}&lang={2}&clientType=WindowManager&mode=dlg', 'adminStyle':true, 'width':'800', 'height':'600', 'resizable':true }})).Show();",
                                        BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/AdvertisingBannerEdit.aspx")), 
                                        BannerId,
                                        BXJSUtility.Encode(HttpUtility.UrlEncode(BXLoc.CurrentLocale)));
							}
							BXHermitagePopupMenuItem createItem = new BXHermitagePopupMenuItem();
							menuItems[menuItems.Length - 1] = createItem;
							createItem.Id = "ADVERTISING_BANNER_CREATE";
							createItem.IconCssClass = "bx-context-toolbar-create-icon";
							createItem.Sort = 20;
							createItem.Text = GetMessageRaw("CreateBannerMenuItem");
							createItem.ClientClickScript = string.Format(
								"(new BX.CDialogNet({{ 'content_url':'{0}?spaceCode={1}&lang={2}&clientType=WindowManager&mode=dlg', 'adminStyle':true, 'width':'800', 'height':'600', 'resizable':true }})).Show();",
                                BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/AdvertisingBannerEdit.aspx")), 
                                SpaceCode,
                                BXJSUtility.Encode(HttpUtility.UrlEncode(BXLoc.CurrentLocale)));
								
							return menuItems;
						};
				return info;
			}
		}
    }

    /// <summary>
    /// Базовый класс для шаблонов компонента "AdvertisingBannerComponent"
    /// </summary>
	public abstract class AdvertisingBannerTemplate : BXComponentTemplate<AdvertisingBannerComponent>
    {
	    private Control _contentControl = null;
	    /// <summary>
	    /// Получить элемент управления с содержанием
	    /// </summary>
	    /// <returns></returns>
        protected Control GetContentControl()
        {
            return _contentControl ?? (_contentControl = new LiteralControl(GetContentHtml()));
        }

        private string _contentHtml = null;
        /// <summary>
        /// Подготовить содержание
        /// </summary>
        public void PrepareContentHtml()
        {
            if (_contentHtml != null)
                return;

            _contentHtml = string.Empty;

            BXAdvertisingBanner banner = Component.Banner;
            if (banner != null)
                switch (banner.ContentType)
                {
                    case BXAdvertisingBannerContentType.Flash:
                        {
                            StringBuilder sb = new StringBuilder();
                            BXFile contentFile = banner.ContentFile;
                            if (contentFile != null)
                            {
                                /*if (banner.FlashDynamicCreation)
                                {*/
                                //динамическое конструирование (javascript)
                                string containerId = string.Concat(ClientID, ClientIDSeparator, "FlashBannerContainer");
                                sb.Append("<div id=\"").Append(containerId).Append("\" style=\"margin:0px; padding:0px; width:").Append(contentFile.Width.ToString()).Append("px; height:").Append(contentFile.Height.ToString()).Append("px;").AppendLine("\">");
                                string redirectionUrl = null, toolTip = banner.ToolTip, redirectionTarget = banner.LinkTarget;
                                if (/*banner.FlashUseCustomUrl &&*/ !string.IsNullOrEmpty(redirectionUrl = Component.PrepareClientRedirectionUrl(null)))
                                {
                                    sb.AppendLine("<div style=\"position:absolute; z-index:100;\">");
                                    sb.Append("<a href=\"").Append(HttpUtility.HtmlAttributeEncode(redirectionUrl)).AppendLine("\"");
                                    if (!string.IsNullOrEmpty(toolTip))
                                        sb.Append(" title=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip)).Append("\"");
                                    if (string.IsNullOrEmpty(redirectionTarget) || string.Equals("_SELF", redirectionTarget.ToUpperInvariant(), StringComparison.InvariantCulture))
                                        redirectionTarget = "_top";
                                    sb.Append(" target=\"").Append(HttpUtility.HtmlAttributeEncode(redirectionTarget)).Append("\"");
                                    sb.Append(">");

                                    sb.Append("<img src=\"").Append(HttpUtility.HtmlAttributeEncode(VirtualPathUtility.ToAbsolute("~/bitrix/images/1.gif"))).Append("\" width=\"").Append(contentFile.Width.ToString()).Append("\" height=\"").Append(contentFile.Height.ToString()).Append("\" style=\"border: 0\"");
									sb.Append(" alt=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip ?? "")).Append("\"");
                                    sb.AppendLine("/>");
                                    sb.AppendLine("</a>");
                                    sb.AppendLine("</div>");
                                }
                                sb.AppendLine("</div>");

                                StringBuilder scriptSb = new StringBuilder("Bitrix.SwfUtility.getInstance().createElement(");
                                scriptSb.Append("\"").Append(containerId).Append("\"");
                                scriptSb.Append(", \"").Append(BXJSUtility.Encode(contentFile.FilePath)).Append("\"");
                                scriptSb.Append(", ").Append(contentFile.Width.ToString());
                                scriptSb.Append(", ").Append(contentFile.Height.ToString());
                                scriptSb.Append(", \"").Append(BXJSUtility.Encode(banner.FlashWMode)).Append("\"");
                                if (!string.IsNullOrEmpty(banner.FlashVersion))
                                    scriptSb.Append(", \"").Append(BXJSUtility.Encode(banner.FlashVersion)).Append("\"");
                                else
                                    scriptSb.Append(", null");
                                if (banner.FlashAltImageFile != null)
                                    scriptSb.Append(", \"").Append(BXJSUtility.Encode(banner.FlashAltImageFile.FilePath)).Append("\"");
                                else
                                    scriptSb.Append(", null");
                                scriptSb.Append(");");

                                scriptSb.Insert(0, "<script \"text/javascript\">");
                                scriptSb.Append("</script>");
                                scriptSb.AppendLine("<noscript>");
                                scriptSb.Append("<div style=\"margin:0px; padding:0px; width:").Append(contentFile.Width.ToString()).Append("px; height:").Append(contentFile.Height.ToString()).Append("px;").AppendLine("\">");
                                if (!string.IsNullOrEmpty(redirectionUrl))
                                {
                                    scriptSb.AppendLine("<div style=\"position:absolute; z-index:100;\">");
                                    scriptSb.Append("<a href=\"").Append(HttpUtility.HtmlAttributeEncode(redirectionUrl)).Append("\"");
                                    if (!string.IsNullOrEmpty(toolTip))
                                        scriptSb.Append(" title=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip)).Append("\"");

                                    scriptSb.Append(" target=\"").Append(HttpUtility.HtmlAttributeEncode(redirectionTarget)).Append("\"");
                                    scriptSb.Append(">");

                                    scriptSb.Append("<img src=\"").Append(HttpUtility.HtmlAttributeEncode(VirtualPathUtility.ToAbsolute("~/bitrix/images/1.gif"))).Append("\" width=\"").Append(contentFile.Width.ToString()).Append("\" height=\"").Append(contentFile.Height.ToString()).Append("\" style=\"border: 0\"");                                    
                                    scriptSb.Append(" alt=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip ?? "")).Append("\"");
                                    scriptSb.AppendLine("/>");
                                    scriptSb.AppendLine("</a>");
                                    scriptSb.AppendLine("</div>");
                                }
                                scriptSb.AppendLine();
                                string encodedFilePath = HttpUtility.HtmlAttributeEncode(contentFile.FilePath);
                                scriptSb.Append("<OBJECT classid=\"clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\" codebase=\"http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=6,0,40,0\" WIDTH=\"").Append(contentFile.Width.ToString()).Append("\" HEIGHT=\"").Append(contentFile.Height.ToString()).Append("\" id=\"").Append(string.Concat(Component.ClientID, ClientIDSeparator, "FlashObject")).AppendLine("\">");
                                scriptSb.Append("<PARAM NAME=\"movie\" VALUE=\"").Append(encodedFilePath).AppendLine("\" />");
                                scriptSb.Append("<PARAM NAME=\"wmode\" VALUE=\"").Append(banner.FlashWMode).AppendLine("\" />");
                                scriptSb.AppendLine("<PARAM NAME=\"play\" VALUE=\"true\" />");
                                scriptSb.AppendLine("<PARAM NAME=\"loop\" VALUE=\"true\" />");
                                scriptSb.AppendLine("<PARAM NAME=\"menu\" VALUE=\"false\" />");
                                scriptSb.AppendLine("<PARAM NAME=\"allowScriptAccess\" VALUE=\"always\" />");
                                scriptSb.AppendLine("<PARAM NAME=\"quality\" VALUE=\"high\" />");
                                scriptSb.AppendLine("<PARAM NAME=\"bgcolor\" VALUE=\"#FFFFFF\" />");
                                scriptSb.Append("<EMBED src=\"").Append(encodedFilePath).Append("\" quality=\"high\" play=\"true\" loop=\"true\" menu=\"false\" allowScriptAccess=\"always\" bgcolor=\"#FFFFFF\" WIDTH=\"").Append(contentFile.Width.ToString()).Append("\" HEIGHT=\"").Append(contentFile.Height.ToString()).Append("\" NAME=\"Banner_").Append(banner.Id.ToString()).Append("\" WMODE=\"").Append(HttpUtility.HtmlAttributeEncode(banner.FlashWMode)).AppendLine("\" TYPE=\"application/x-shockwave-flash\" PLUGINSPAGE=\"http://www.macromedia.com/go/getflashplayer\"></EMBED>");
                                scriptSb.AppendLine("</OBJECT>");
                                scriptSb.AppendLine("</div>");
                                scriptSb.AppendLine("</noscript>");
                                /*
                                ScriptManager.RegisterStartupScript(Component.Page,
                                    GetType(),
                                    string.Concat("FlashBannerConstructor#", ClientID),
                                    scriptSb.ToString(),
                                    false
                                    );*/
                                sb.Append(scriptSb.ToString());
                            }
                            #region HTML
                            //string text = Component.PrepareTextContent();
                            //if (!string.IsNullOrEmpty(text))
                            //{
                            //    if (sb.Length > 0)
                            //        sb.AppendLine();
                            //    sb.AppendLine(text);
                            //}
                            #endregion
                            if (sb.Length > 0)
                            {
                                _contentHtml = sb.ToString();
                                Component.IsBannerDisplayed = true;
                            }
                        }
                        break;
                    case BXAdvertisingBannerContentType.Silverlight:
                        {
                            BXFile contentFile = banner.ContentFile;
                            if (contentFile != null)
                            {

                                string encodedFilePath = HttpUtility.HtmlAttributeEncode(contentFile.FilePath);
                                StringBuilder sb = new StringBuilder();
                                string redirectionUrl = null, toolTip = banner.ToolTip, redirectionTarget = banner.LinkTarget;
                                //<script type=\"text/javascript\" src=\"/bitrix/js/main/silverlight.debug.js\">
                                sb.Append("<div style=\"margin:0px; padding:0px; width:").Append(contentFile.Width.ToString()).Append("px; height:").Append(contentFile.Height.ToString()).Append("px;").AppendLine("\">");
                                if (!string.IsNullOrEmpty(redirectionUrl = Component.PrepareClientRedirectionUrl(null)))

                                {
                                    sb.Append("<div style=\"position:absolute;z-index:100; width:").Append(contentFile.Width.ToString()).Append("px; height:").Append(contentFile.Height.ToString()).Append("px;").AppendLine("\">");
                                    sb.Append("<a href=\"").Append(HttpUtility.HtmlAttributeEncode(redirectionUrl)).Append("\"");
                                    if (!string.IsNullOrEmpty(toolTip))
                                        sb.Append(" title=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip)).Append("\"");

                                    sb.Append(" target=\"").Append(HttpUtility.HtmlAttributeEncode(redirectionTarget)).Append("\"");
                                    sb.Append(">");

                                    sb.Append("<img src=\"").Append(HttpUtility.HtmlAttributeEncode(VirtualPathUtility.ToAbsolute("~/bitrix/images/1.gif"))).Append("\" width=\"").Append(contentFile.Width.ToString()).Append("\" height=\"").Append(contentFile.Height.ToString()).Append("\" style=\"border: 0\"");                                    
                                    sb.Append(" alt=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip ?? "")).Append("\"");
                                    sb.AppendLine("/>");
                                    sb.AppendLine("</a>");
                                    sb.AppendLine("</div>");
                                }

                                sb.Append("<object  data=\"data:application/x-silverlight,\" type=\"application/x-silverlight\" width=\"").Append(contentFile.Width.ToString()).Append("\" height=\"").Append(contentFile.Height.ToString()).Append("\" id=\"").Append(string.Concat(Component.ClientID, ClientIDSeparator, "SLObject")).AppendLine("\">");
                                sb.Append("<param name=\"source\" value=\"").Append(encodedFilePath).AppendLine("\" />");
                                sb.Append("<param name=\"autoUpgrade\" value=\"true\" />");
                                sb.Append("<param name=\"windowless\" value=\"true\" />");
                                if ( !String.IsNullOrEmpty(banner.FlashVersion))
                                    sb.AppendFormat("<param name=\"minruntimeversion\" value=\"{0}\" />",BXTextEncoder.HtmlTextEncoder.Encode( banner.FlashVersion));

                                sb.Append("<param name=\"background\" value=\"transparent\" />");
                                sb.AppendFormat("<a href=\"{0}\" style=\"text-decoration: none;\">",
                                    (banner.FlashAltImageFile != null ? banner.LinkUrl : "http://go.microsoft.com/fwlink/?LinkID=124807"));
                                sb.AppendFormat("<img src=\"{0}\" alt=\"{1}\" style=\"border-style: none\"/>",
                                       banner.FlashAltImageFile != null ? BXTextEncoder.HtmlTextEncoder.Encode(banner.FlashAltImageFile.FilePath) : "http://go.microsoft.com/fwlink/?LinkId=108181",
                                       banner.FlashAltImageFile != null ? BXTextEncoder.HtmlTextEncoder.Encode(banner.ToolTip) : "Get Microsoft Silverlight");
                                sb.Append("</a>");

                                sb.AppendLine("</object>");
                                sb.AppendLine("</div>");
                                //if (!string.IsNullOrEmpty(redirectionUrl))
                                  //  
                                if (sb.Length > 0)
                                {
                                    _contentHtml = sb.ToString();
                                    Component.IsBannerDisplayed = true;
                                }
                            }
                        }
                        break;
                    case BXAdvertisingBannerContentType.Image:
                        {
                            StringBuilder sb = new StringBuilder();
                            BXFile contentFile = banner.ContentFile;
                            if (contentFile != null)
                            {
                                string redirectionUrl = Component.PrepareClientRedirectionUrl(null),
                                    toolTip = banner.ToolTip;
                                if (!string.IsNullOrEmpty(redirectionUrl))
                                {
                                    sb.Append("<a href=\"").Append(HttpUtility.HtmlAttributeEncode(redirectionUrl)).Append("\"");
                                    if (!string.IsNullOrEmpty(toolTip))
                                        sb.Append(" title=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip)).Append("\"");
                                    string redirectionTarget = banner.LinkTarget;
                                    if (string.IsNullOrEmpty(redirectionTarget) || string.Equals("_SELF", redirectionTarget.ToUpperInvariant(), StringComparison.InvariantCulture))
                                        redirectionTarget = "_top";
                                    sb.Append(" target=\"").Append(HttpUtility.HtmlAttributeEncode(redirectionTarget)).Append("\"");
                                    sb.Append(">");
                                }
                                sb.Append("<img style=\"border: 0\" src=\"").Append(HttpUtility.HtmlAttributeEncode(contentFile.FilePath)).Append("\"");
                                sb.Append(" width=\"").Append(contentFile.Width.ToString()).Append("\"");
                                sb.Append(" height=\"").Append(contentFile.Height.ToString()).Append("\"");                                
                                sb.Append(" alt=\"").Append(HttpUtility.HtmlAttributeEncode(toolTip ?? "")).Append("\"");
                                sb.Append("/>");
                                if (!string.IsNullOrEmpty(redirectionUrl))
                                    sb.Append("</a>");
                            }
                            #region HTML
                            //string text = Component.PrepareTextContent();
                            //if (!string.IsNullOrEmpty(text))
                            //{
                            //    if (sb.Length > 0)
                            //        sb.AppendLine();
                            //    sb.AppendLine(text);
                            //}
                            #endregion
                            if (sb.Length > 0)
                            {
                                _contentHtml = sb.ToString();
                                Component.IsBannerDisplayed = true;
                            }
                        }
                        break;
                    case BXAdvertisingBannerContentType.TextOnly:
                        {
                            string text = Component.PrepareTextContent();
                            if (!string.IsNullOrEmpty(text))
                            {
                                _contentHtml = text;
                                Component.IsBannerDisplayed = true;
                            }
                        }
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Banner content type '{0}' is unknown in current context!", banner.ContentType.ToString("G")));
                }
        }

        /// <summary>
        /// Запрос содержания
        /// </summary>
        /// <returns></returns>
        protected string GetContentHtml()
        {
            PrepareContentHtml();
            return _contentHtml;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            PrepareContentHtml();
        }
    }
}
