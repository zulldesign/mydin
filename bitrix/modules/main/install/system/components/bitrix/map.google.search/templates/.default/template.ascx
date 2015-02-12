<%@ Reference VirtualPath="~/bitrix/components/bitrix/map.google.search/component.ascx" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="MapGoogleSearchComponentTemplate" %>
<script runat="server">    
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        
        mapCanvas.Attributes.Add("style", string.Concat("width:", Component.MapWidthInPixels.ToString(), "px; height:", Component.MapHeightInPixels.ToString(), "px;"));

        Bitrix.UI.BXPage.Scripts.RequireUtils();
        Bitrix.UI.BXPage.RegisterScriptInclude("~/bitrix/controls/Main/map.google/messages.js.aspx?lang=" + HttpUtility.UrlEncode(BXLoc.CurrentLocale));
        Page.ClientScript.RegisterClientScriptInclude(Component.GetType(), "GoogleMapsApi", "http://maps.google.com/maps/api/js?sensor=false");
        Bitrix.UI.BXPage.RegisterScriptInclude("~/bitrix/controls/Main/map.google/google_maps.js");
        Bitrix.UI.BXPage.RegisterStyle("~/bitrix/controls/Main/map.google/google_maps.css");
                
        if (!Page.ClientScript.IsClientScriptBlockRegistered(typeof(object), "GoogleMapsSettings"))
        {
            StringBuilder settingsSb = new StringBuilder("if(typeof(Bitrix.GoogleMapsSettings) == 'undefined') { Bitrix.GoogleMapsSettings={");
            settingsSb.Append("centerMarkerImageUrl:'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.google/images/cross.png")).Append("'");
            settingsSb.Append(", standardMarkerImageUrl:'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.google/images/arrow.png")).Append("'");
            settingsSb.Append(", dragPanelIconImageUrl:'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.google/images/btn_hand.png")).Append("'");
            settingsSb.Append(", dragPanelIconSelImageUrl:'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.google/images/btn_hand_pushed.png")).Append("'");
            settingsSb.Append(", centerPanelIconImageUrl:'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.google/images/btn_cross.png")).Append("'");
            settingsSb.Append(", centerPanelIconSelImageUrl:'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.google/images/btn_cross_pushed.png")).Append("'");
            settingsSb.Append(", markerPanelIconImageUrl:'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.google/images/btn_arrow.png")).Append("'");
            settingsSb.Append(", markerPanelIconSelImageUrl:'").Append(VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/map.google/images/btn_arrow_pushed.png")).Append("'");
            settingsSb.Append(", language:'").Append(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName).Append("'");
            settingsSb.Append("};}");

            ScriptManager.RegisterClientScriptBlock(
                Page,
                typeof(object),
                "GoogleMapsSettings",
                settingsSb.ToString(),
                true
                );
        }

        ScriptManager.RegisterStartupScript(
            this,
            GetType(),
            string.Concat(UniqueID, "$Initialize"),
            string.Format(@"Bitrix.EventUtility.addEventListener(window, ""load"", function(){{ Bitrix.GoogleMapsManager.createEntity(Bitrix.GoogleMapsData.fromObject({{ initialState:{0}, id:""{1}"", mapControlTypes:""{2}"", mapOptions:""{3}"", draggableCursor:""{4}"", draggingCursor:""{5}"", searchPanel:true }}), document.getElementById(""{6}"")).construct(); }});",
                Component.InitialState,
                Component.UniqueID,
                Component.MapControls.ToString("F"),
                Component.MapOptions.ToString("F"),
                Component.DraggableCursor,
                Component.DraggingCursor,
                this.mapCanvas.ClientID
                ),
            true
            );
    }
</script>


<div id="mapCanvas" runat="server">
</div>


