<%@ Reference Control="~/bitrix/components/bitrix/advertising.banner/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Advertising.Components.AdvertisingBannerTemplate" %>
<script runat="server" type="text/C#">
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Control content = GetContentControl();
        if (content != null)
            Controls.Add(content);
    }    
</script>
