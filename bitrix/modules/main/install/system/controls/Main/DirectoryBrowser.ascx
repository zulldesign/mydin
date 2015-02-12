<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DirectoryBrowser.ascx.cs"
	Inherits="DirectoryBrowser" %>
<bx:BXPopupDialog ID="browser" runat="server" ShowResetButton="false" Width="335px">
	<ContentPanel CssClass="content">
		<div class="DirectoryBrowser" >
		<table id="layout" runat="server" cellspacing="0" cellpadding="0" style="width: 100%; border: 0px none;">
			<tr runat="server">
				<td runat="server">
					<table cellspacing="0" cellpadding="0" style="width: 100%; border: 1px solid #C4C4C4">
						<tr>
							<td style="width: 100%; vertical-align: top">
								<div class="DirectoryBrowser-Selector" id="selector" runat="server" style="width: 300px; height:360px; overflow: auto" />
							</td>
							<td runat="server" id="extras" style="vertical-align: top; padding: 8px; padding-bottom: 0px; width:100%; visibility:hidden; display:none;  border-left: 1px solid #C4C4C4">
								<table cellspacing="0" cellpadding="0" style="height: auto; width: 100%; border: 0px none">
									<tr>
										<td style="border-bottom: 1px solid #C4C4C4; padding-bottom:8px;">
											<span style="font-size:8pt"><%= GetMessage("Preview") + ":" %></span>
											<table cellspacing="0" cellpadding="0" style="height: 160px; width: 260px; border: 1px solid #C4C4C4;
												vertical-align: middle; text-align: center">
												<tr>
													<td id="preview" runat="server" style="overflow: hidden">&nbsp;</td>
												</tr>
											</table>
										</td>
									</tr>
									<tr>
										<td style="padding-top:4px" >
											<iframe id="up" runat="server" frameborder="0" scrolling="no" style="border:0px none; overflow:hidden; width: 260px; height: 160px" ></iframe>
										</td>
									</tr>
								</table>
							</td>
							<td class="divider-bg">
								<div id="switcher" runat="server" class="divider right" />
							</td>
						</tr>
					</table>
				</td>
			</tr>
			<tr runat="server">
				<td style="padding-top: 3px" runat="server">
					<input id="selectedName" runat="server" type="text" style="width: 100%; box-sizing:border-box; -webkit-box-sizing: border-box;-moz-box-sizing:border-box;" /></td>
			</tr>
		</table>
	</div>
	</ContentPanel>
</bx:BXPopupDialog>
