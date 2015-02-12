<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>
<bx:IncludeComponent
 runat="server"
 ID="NewsRSS"
 ComponentName="bitrix:iblock.rss"
 Template=".default"
 
 IBlockSectionId="<%$ Results:SectionId %>"
 FiltrationByActivity="Active"
 ItemQuantity="<%$ Parameters:RssElementsCount %>"

 FeedUrlTemplate="<%$ Results:UrlTemplatesRss %>"
 FeedItemUrlTemplate="<%$ Results:UrlTemplatesDetail %>"
 FeedItemSortMode="DateOfActivationStart"
/>