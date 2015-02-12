<%@ Reference VirtualPath="~/bitrix/components/bitrix/map.yandex.search/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="MapYandexSearchComponentTemplate" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.UI" %>
<script runat="server">    
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        BXPage.Scripts.RequireUtils();
        if (!string.IsNullOrEmpty(Component.YandexMapsApiKey))
            Page.ClientScript.RegisterClientScriptInclude(Component.GetType(), "YMapsApi", "http://api-maps.yandex.ru/1.1/?key=" + Component.YandexMapsApiKey);
        BXPage.RegisterScriptInclude("~/bitrix/controls/Main/map.yandex/yandex_maps.js");
        BXPage.RegisterStyle("~/bitrix/controls/Main/map.yandex/yandex_maps.css");
        BXPage.RegisterScriptInclude(string.Concat("~/bitrix/controls/Main/map.yandex/messages.js.aspx?lang=", HttpUtility.UrlEncode(BXLoc.CurrentLocale)));        
        
        if (Component.ComponentErrors != MapYandexSearchComponentError.None)
            return;
        
        mapCanvas.Attributes.Add("style", string.Concat("width:", Component.MapWidthInPixels.ToString(), "px; height:", Component.MapHeightInPixels.ToString(), "px;"));        
        
        if (!Page.ClientScript.IsClientScriptBlockRegistered(typeof(object), "YandexMapSettings"))
        {
            StringBuilder settingsSb = new StringBuilder("if(typeof(Bitrix.YandexMapSettings)=='undefined'){Bitrix.YandexMapSettings={");
            settingsSb.Append("'centerMarkerImage':{'url':'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.yandex/images/cross.png")).Append("','width':24,'height':25}");
            settingsSb.Append(",'standardMarkerImage':{'url':'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.yandex/images/arrow.png")).Append("','width':23,'height':31}");
            settingsSb.Append(",'dragPanelIconImage':{'url':'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.yandex/images/btn_hand.png")).Append("','width':28,'height':28}");
            settingsSb.Append(",'dragPanelIconSelImage':{'url':'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.yandex/images/btn_hand_pushed.png")).Append("','width':28,'height':28}");
            settingsSb.Append(",'centerPanelIconImage':{'url':'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.yandex/images/btn_cross.png")).Append("','width':28,'height':28}");
            settingsSb.Append(",'centerPanelIconSelImage':{'url':'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.yandex/images/btn_cross_pushed.png")).Append("','width':28,'height':28}");
            settingsSb.Append(",'markerPanelIconImage':{'url':'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.yandex/images/btn_arrow.png")).Append("','width':28,'height':28}");
            settingsSb.Append(",'markerPanelIconSelImage':{'url':'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.yandex/images/btn_arrow_pushed.png")).Append("','width':28,'height':28}");

            settingsSb.Append(",'language':'").Append(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName).Append("'");
            settingsSb.Append("};}");

            ScriptManager.RegisterClientScriptBlock(
                Page,
                typeof(object),
                "YandexMapSettings",
                settingsSb.ToString(),
                true
                );
        }

        Page.ClientScript.RegisterStartupScript(
            GetType(),
            string.Concat(UniqueID, "$Initialize"),
            string.Concat("Bitrix.EventUtility.addEventListener(window,'load',function(){var entity=Bitrix.YandexMapManager.createEntity(Bitrix.YandexMapData.fromObject({'initialState':", Component.InitialState, ",'id':'", Component.UniqueID, "','mapControlTypes':'", Component.MapControls.ToString("F"), "','mapOptions':'", Component.MapOptions.ToString("F"), "'}),document.getElementById('", this.mapCanvas.ClientID, "')); if(entity)entity.construct();});"),
            true
            );             
    }
</script>
<div id="mapCanvas" runat="server"></div>


