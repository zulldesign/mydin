<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AdminMenu.ascx.cs" Inherits="bitrix_kernel_AdminMenu" %>


<div id="hiddenmenucontainer" style="display:none;" onclick="JsAdminMenu.verSplitterToggle();" title="<%= GetMessage("DivTitle.HiddenMenuContainer") %>">
    <%--М<br />е<br />н<br />ю<br />--%>
    <%= GenerateVerticalMenuHtmlMarkup(GetMessage("DivInnerHtml.HiddenMenuContainer"))%>
</div>

<div id="menudiv" style="display:block;">
    <table cellpadding="0" cellspacing="0" border="0" width="100%">
	    <tr>
		    <td>
			    <div id="menutitle"><asp:Label ID="LabelMenuTitle" runat="server" Text=""></asp:Label></div>

			    <div id="buttonscontainer" >
				    <table id="buttonsContainer" runat="server" cellpadding="0" cellspacing="0" border="0" class="buttons">
				    </table>
			    </div>

			    <div id="smbuttonscontainer" style="display:none;">
				    <table id="smButtonsContainer" runat="server" cellpadding="0" cellspacing="0" border="0">
				    </table>
			    </div>

		    </td>
	    </tr>
	    <tr>
		    <td>
			    <table cellpadding="0" cellspacing="0" border="0" class="hdivider">
				    <tr>
					    <td><div class="empty"></div></td>
					    <td id="hdividercell" class="hdividerknob hdividerknobup" onmouseover="JsAdminMenu.horSplitter.Highlight(true);" onmouseout="JsAdminMenu.horSplitter.Highlight(false);" onclick="JsAdminMenu.horSplitterToggle();" title=""><div class="empty"></div></td>
					    <td><div class="empty"></div></td>
				    </tr>
			    </table>
		    </td>
	    </tr>
	    <tr>
		    <td>
			    <div id="menucontainer" style='width:<%= WidthAsString %>;'>
				    <asp:Label ID="LabelMenuContainer" runat="server" Text=""></asp:Label>
			    </div>
			    <div id="menu_min_width" class="empty"></div>
		    </td>
	    </tr>
    </table>
</div>

<%--
<table cellpadding="0" cellspacing="0" border="0" width="100%" style="height:100%">
	<tr>
		<td class="toppanel-shadow"><div class="empty"></div></td>
		<td class="vdivider-top-bg" onmousedown="JsAdminMenu.StartDrag();"><div class="empty"></div></td>
		<td class="toppanel-shadow"><div class="empty"></div></td>
	</tr>
	<tr>
		<td valign="top" width="0%">

			<div id="hiddenmenucontainer" style="display:none;" onclick="JsAdminMenu.verSplitterToggle();" title="Показать меню">
			М<br />е<br />н<br />ю<br />
			</div>

			<div id="menudiv" style="display:block;">
				<table cellpadding="0" cellspacing="0" border="0" width="100%">
					<tr>
						<td>
							<div id="menutitle"><asp:Label ID="LabelMenuTitle" runat="server" Text=""></asp:Label></div>

							<div id="buttonscontainer">
								<table id="buttonsContainer" runat="server" cellpadding="0" cellspacing="0" border="0" class="buttons">
								</table>
							</div>

							<div id="smbuttonscontainer" style="display:none;">
								<table id="smButtonsContainer" runat="server" cellpadding="0" cellspacing="0" border="0">
								</table>
							</div>

						</td>
					</tr>
					<tr>
						<td>
							<table cellpadding="0" cellspacing="0" border="0" class="hdivider">
								<tr>
									<td><div class="empty"></div></td>
									<td id="hdividercell" class="hdividerknob hdividerknobup" onmouseover="JsAdminMenu.horSplitter.Highlight(true);" onmouseout="JsAdminMenu.horSplitter.Highlight(false);" onclick="JsAdminMenu.horSplitterToggle();" title=""><div class="empty"></div></td>
									<td><div class="empty"></div></td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td>
							<div id="menucontainer" style='width:<%= WidthAsString %>;'>
								<asp:Label ID="LabelMenuContainer" runat="server" Text=""></asp:Label>
							</div>
						</td>
					</tr>
				</table>
			</div>
		</td>
	</tr>
</table>
--%>