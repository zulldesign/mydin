<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="Contacts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server" > 
	
<p>Get answers to your questions, quickly and easily.</p>

<p>You can contact us by phone, email or visit the Bank branch. We will be glad to answer all your questions.</p>

<h2>Contact us by phone</h2>

    <table width="100%">
		<tr>
			<td width="30%">Bank cards</td> <td width="35%">New Customers</td><td width="35%">Existing Customers</td>
		</tr>
		<tr>
			<td width="30%"> </td> <td width="35%">X.XXX.XXX.XXXX</td><td width="35%">X.XXX.XXX.XXXX</td>
		</tr>
		<tr>
			<td width="30%">Deposits</td> <td width="35%">New Customers</td><td width="35%">Existing Customers</td>
		</tr>
		<tr>
			<td width="30%"> </td> <td width="35%">X.XXX.XXX.XXXX</td><td width="35%">X.XXX.XXX.XXXX</td>
		</tr>
		<tr>
			<td width="30%">Mortgage and Auto loans</td> <td width="35%">New Customers</td><td width="35%">Existing Customers</td>
		</tr>
		<tr>
			<td width="30%"> </td> <td width="35%">X.XXX.XXX.XXXX</td><td width="35%">X.XXX.XXX.XXXX</td>
		</tr>
	</table>
	
    <p>Customer service representatives are available 24 hours a day/ 7 days a week.</p>

	<h2>Contact us by email</h2>
	bank@bank.com
<h2>Bank Branch in Bristol</h2> 				 			
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
	InitialState="{ mapType: &quot;roadmap&quot;, center: { lat: 51.45665432908274, lng: -2.592473030090332 }, zoom: 15, markers: [ { title: &quot;Bank Branch in Bristol&quot;, position: { lat: 51.45665432908274, lng: -2.592473030090332 } } ] }" 
/>

 </asp:Content>

