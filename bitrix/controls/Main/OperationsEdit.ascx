<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OperationsEdit.ascx.cs" Inherits="OperationsEdit" %>
<!--[if IE 7]>
<style type="text/css">
table.BXSecurityUI input.input 
{
	margin-top: -1px !important;
	margin-bottom: -1px !important;
}
</style>
<![endif]-->
<table class="BXSecurityUIWrapper" cellspacing="0" cellpadding="0">
<tr><td>
<table class="BXSecurityUI" cellspacing="0" cellpadding="0">
	<tr>
		<td style="vertical-align: top; height: 100%">
			<div class="roles-div">
				<div class="roles-container">
					<table id="<%= ClientID %>_roles" class="roles">
					</table>
				</div>
			</div>
		</td>
		<td style="width: 16px" rowspan="3">
		</td>
		<td rowspan="3" style="vertical-align: top">
			<div class="operations-div">
				<div class="operations-container">
					<table id="<%= ClientID %>_operations" class="operations">
					</table>
				</div>
			</div>
		</td>
	</tr>
	<tr class="horizontal-separator">
		<td>
			<div class="empty">
			</div>
		</td>
	</tr>
	<tr>
		<td>
			<div >
				<table id="<%= ClientID %>_dropdown" cellspacing="0" cellpadding="0" style="border-collapse: collapse;
					width: 100%; height: 25px;">
					<tr>
						<td>
							<div style="border: solid 1px #ABB6D7; border-right: none;">
								<div style="height: 23">
									<input id="<%= ClientID %>_add" type="text" onkeyup="<%= JSName %>.ProcessIntel(event, this)" onfocus="this.value = '';"
										onblur="this.value = 'type here...'" class="input" />
								</div>
							</div>
						</td>
						<td style="width: 25px;">
							<div class="drop-down" onclick="<%= JSName %>.ShowAllIntel();" onmouseover="this.className='drop-down drop-down-highlight';"
								onmouseout="this.className='drop-down';">
								<div class="empty">
								</div>
							</div>
						</td>
					</tr>
				</table>
				
			</div>
		</td>
	</tr>
</table>
</td></tr>
<tr><td>
<%
	if (ShowNotes)
	{
%>
<div class="BXSecurity-notes" style="display:none" id="<%= ClientID %>_notes" >
<table cellspacing="0" cellpadding="0" border="0" width="100%" class="BXSecurity-notes">
	<tr class="top">
		<td class="left"><div class="empty"></div></td>
		<td><div class="empty"></div></td>
		<td class="right"><div class="empty"></div></td>
	</tr>
	<tr>
		<td class="left"><div class="empty"></div></td>
		<td class="content" id="<%= ClientID %>_notesinner"></td>
		<td class="right"><div class="empty"></div></td>
	</tr>
	<tr class="bottom">
		<td class="left"><div class="empty"></div></td>
		<td><div class="empty"></div></td>
		<td class="right"><div class="empty"></div></td>
	</tr>
</table>
</div>
<% 
	}
	
	if (ShowLegend && AllowedStates != BXOperationsEditAllowedOperationState.None) 
	{
%>
<div class="BXSecurity-notes">
<table cellspacing="0" cellpadding="0" border="0" width="100%" class="BXSecurity-notes">
	<tr class="top">
		<td class="left"><div class="empty"></div></td>
		<td><div class="empty"></div></td>
		<td class="right"><div class="empty"></div></td>
	</tr>
	<tr>
		<td class="left"><div class="empty"></div></td>
		<td class="content">
			<table cellspacing="0" cellpadding="0" border="0" width="100%" class="BXSecurity-legend">
				<% 
					if ((AllowedStates & BXOperationsEditAllowedOperationState.Allowed) != BXOperationsEditAllowedOperationState.None)
					{
						string html = Encode(LegendText.Allow ?? GetMessageRaw("Legend.Allow"));
						if (!string.IsNullOrEmpty(html))
						{
				%>
				<tr>
					<td class="BXSecurity-legend-icon"><img src="<%= Encode(BXThemeHelper.AddAbsoluteThemePath("images/security/tick.gif")) %>" alt="V" /></td>
					<td>-&nbsp;</td>
					<td class="BXSecurity-legend-description"><%= html %></td>
				</tr>
				<%
						}
					}
					if ((AllowedStates & BXOperationsEditAllowedOperationState.Denied) != BXOperationsEditAllowedOperationState.None)
					{	
						string html = Encode(LegendText.Deny ?? GetMessageRaw("Legend.Deny"));
						if (!string.IsNullOrEmpty(html))
						{	
				%>
				<tr>
					<td class="BXSecurity-legend-icon"><img src="<%= Encode(BXThemeHelper.AddAbsoluteThemePath("images/security/cross.gif")) %>" alt="X" /></td>
					<td>-&nbsp;</td>
					<td class="BXSecurity-legend-description"><%= html %></td>
				</tr>
				<%
					
						}
					}
					if ((AllowedStates & BXOperationsEditAllowedOperationState.Inherited) != BXOperationsEditAllowedOperationState.None)
					{	
						string html = Encode(LegendText.InheritAllow ?? GetMessageRaw("Legend.InheritAllow"));
						if (!string.IsNullOrEmpty(html))
						{	
				%>
				<tr>
					<td class="BXSecurity-legend-icon"><img src="<%= Encode(BXThemeHelper.AddAbsoluteThemePath("images/security/tick-gray.gif")) %> " alt="v" /></td>
					<td>-&nbsp;</td>
					<td class="BXSecurity-legend-description"><%= html %></td>
				</tr>
				<%
						}
						html = Encode(LegendText.InheritDeny ?? GetMessageRaw("Legend.InheritDeny"));
						if (!string.IsNullOrEmpty(html))
						{
				%>
				<tr>
					<td class="BXSecurity-legend-icon"><img src="<%= Encode(BXThemeHelper.AddAbsoluteThemePath("images/security/cross-gray.gif")) %>" alt="x" /></td>
					<td>-&nbsp;</td>
					<td class="BXSecurity-legend-description"><%= html %></td>
				</tr>
				<%
						}
					}
					if (ShowLegendDontModify)
					{
						string html = Encode(LegendText.DontModify ?? GetMessageRaw("Legend.DontModify"));
						if (!string.IsNullOrEmpty(html))
						{
				%>
				<tr>
					<td class="BXSecurity-legend-icon"><img src="<%= Encode(BXThemeHelper.AddAbsoluteThemePath("images/security/question.gif")) %>" alt="?" /></td>
					<td>-&nbsp;</td>
					<td class="BXSecurity-legend-description"><%= html %></td>
				</tr>
				<%
						}
					}
				%>
			</table>
		</td>
		<td class="right"><div class="empty"></div></td>
	</tr>
	<tr class="bottom">
		<td class="left"><div class="empty"></div></td>
		<td><div class="empty" ></div></td>
		<td class="right"><div class="empty"></div></td>
	</tr>
</table>
</div>
<%
	}
%>
</td></tr>
</table>
<input type="hidden" id="<%= ClientID %>_state" name="<%= UniqueID %>" />
<bx:InlineScript runat="server" ID="Script" AsyncMode="Startup" >
	<script type="text/javascript">
		window.setTimeout(function()
		{
		<%= JSName %> = new BXSecurityUI(
			<%= Bitrix.Services.Js.BXJSUtility.BuildJSON(BuildRolesData()) %>, 
			<%= Bitrix.Services.Js.BXJSUtility.BuildJSArray(BuildOperationsData()) %>,
			<%= state == null ? "null" : state.ToJSON() %>,
			{
				rolesTable: document.getElementById('<%= ClientID %>_roles'), 
				operationsTable: document.getElementById('<%= ClientID %>_operations'),
				intelDropDown: document.getElementById('<%= ClientID %>_dropdown')<%
		if (ShowNotes)
		{ 
				%>,
				notesText: document.getElementById('<%= ClientID %>_notesinner'),
				notesContainer: document.getElementById('<%= ClientID %>_notes')
				<%
		}
				%>
			},
			{
				defaultState: <%= (int)DefaultOperationState %>,
				defaultInheritedState: <%= (int)DefaultInheritedOperationState %>,
				imagesPath: '<%= JSEncode(BXThemeHelper.AddAbsoluteThemePath("images/security")) %>',
				showNotes: <%= ShowNotes.ToString().ToLower() %>,
				allowedStates: <%= (int)AllowedStates %>
			},
			{
				emptyTextboxValue: '<%= JSEncode(GetMessageRaw("RolesDropDownInvitation")) %>',
				operationAllow: '<%= JSEncode(GetMessageRaw("CheckTooltip.Allow")) %>',
				operationDeny: '<%= JSEncode(GetMessageRaw("CheckTooltip.Deny")) %>',
				operationInheritAllow: '<%= JSEncode(GetMessageRaw("CheckTooltip.InheritAllow")) %>',
				operationInheritDeny: '<%= JSEncode(GetMessageRaw("CheckTooltip.InheritDeny")) %>',
				operationDontModify: '<%= JSEncode(GetMessageRaw("CheckTooltip.DontModify")) %>',
				deleteRoleButtonToolTip: '<%= JSEncode(GetMessageRaw("ButtonToolTip.DeleteRole")) %>',
				deleteConfirmation: '<%= JSEncode(GetMessageRaw("Confirmation.DeleteRole")) %>'
			}
		);
		<%= JSName %>.PrepareTextBox(document.getElementById('<%= ClientID %>_add'));
		}, 0);
	</script>
</bx:InlineScript>
