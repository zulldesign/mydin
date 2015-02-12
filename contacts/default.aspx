<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main"  Title="Contact Us" %>

<script runat="server" id="@__bx_pagekeywords">
	public override void SetPageKeywords(System.Collections.Generic.IDictionary<string, string> keywords)
	{
		keywords[@"guga"]=@"";
	}
</script>

<asp:Content ID="Content1" ContentPlaceHolderID="BXContent" runat="server" > 

   <p>Have a question you can't find the answer to?</p>

<p>Our furniture experts are here to help.</p>

<h2>Contact us by phone</h2>
<p>Please don't hesitate to contact us on the telephone number below.</p>
<p>X.XXX.XXX.XXXX</p>

<h2>Contact us by email</h2>

<ul> 
  <li><a href="mailto:info@example.com">info@example.com</a> &mdash; For order status on an unshipped order</li>
  <li><a href="mailto:sales@example.com">sales@example.com</a> &mdash; For product questions or to place an order</li>
</ul>

<h2>Visit our showroom at Castlemilk, Glasgow</h2>
<bx:IncludeComponent 
	id="mapgoogle_view1" 
	runat="server" 
	componentname="bitrix:map.google.view" 
	template=".default" 
	MapWidthInPixels="600" 
	MapHeightInPixels="600" 
	MapControls="" 
	MapOptions="" 
	DraggableCursor="" 
	DraggingCursor="" 
	InitialState="{ mapType: &quot;roadmap&quot;, center: { lat: 55.88119494391713, lng: -4.256536178588872 }, zoom: 13, markers: [ { title: &quot;Showroom at Castlemilk, Glasgow&quot;, position: { lat: 55.87512840720154, lng: -4.276363067626958 } } ] }" 
/>
</asp:Content>
