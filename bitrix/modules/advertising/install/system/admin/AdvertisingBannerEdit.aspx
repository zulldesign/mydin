<%@ Page Language="C#" 
    MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="false" 
	EnableViewState="false"
	CodeFile="AdvertisingBannerEdit.aspx.cs" 
	Inherits="bitrix_admin_AdvertisingBannerEdit" %>
		
<%@ Register Src="~/bitrix/admin/controls/Main/AdminImageField.ascx" TagName="AdminImageField" TagPrefix="bxAdmin" %>
<%@ Register Src="~/bitrix/admin/controls/Main/AdminFileField.ascx" TagName="AdminFileField" TagPrefix="bxAdmin" %>
<%@ Register Src="~/bitrix/controls/Main/WeekScheduleEditor/BXWeekScheduleEditor.ascx" TagName="WeekScheduleEditor"  TagPrefix="bx" %>	
<%@ Register Src="~/bitrix/controls/Main/WeekScheduleHourSpan/BXWeekScheduleHourSpan.ascx" TagName="WeekScheduleHourSpan" TagPrefix="bx" %>	
<%@ Register Src="~/bitrix/controls/Main/TimeInterval.ascx" TagName="TimeInterval" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/controls/Main/Calendar.ascx" TagName="Calendar" TagPrefix="bx" %>
<%@ Import Namespace="Bitrix.Configuration" %>	
	
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<bx:BXContextMenuToolbar ID="ContextMenuToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="AdvertisingBannerList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton"
				CommandName="add" Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
				CssClass="context-button icon btn_new" Href="AdvertisingBannerEdit.aspx" />
				
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:ActionTitle.Delete %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="AdvertisingBannerEdit" />
	<bx:BXTabControl ID="TabControl" runat="server" OnCommand="OnAdvertisingBannerEdit" ValidationGroup="AdvertisingBannerEdit">
		<bx:BXTabControlTab ID="MainSettingsTab" runat="server" Selected="True" Text="<%$ LocRaw:TabText.AdvertisingBanner %>" Title="<%$ LocRaw:TabTitle.AdvertisingBanner %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<% if (ChargeId > 0){%>
				    <tr valign="top">
				        <td class="field-name">
				            <div class="<%= IsChargeInRotation ? "lamp-green" : "lamp-red"%>" style="float:right;"></div>       
				        </td>
				        <td>
				            <span><%= HttpUtility.HtmlEncode(ChargeRotationLegend)%></span>
				        </td>
				    </tr> 
                    <tr valign="top"><td class="field-name">ID:</td><td><%= ChargeId%></td></tr>				    						
			    <%}%>           
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingBannerName") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.AdvertisingBannerName")%>:</td>
					<td>
						<asp:TextBox ID="abName" runat="server" Width="350px" />
						<asp:RequiredFieldValidator ID="NameRequiredValidator" runat="server" ValidationGroup="AdvertisingBannerEdit" ControlToValidate="abName" ErrorMessage="<%$ Loc:Message.AdvertisingBannerNameIsNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>
						<asp:CustomValidator ID="abNameValidator" runat="server" 
						    ValidationGroup="AdvertisingBannerEdit"
							ControlToValidate="abName"
							ErrorMessage="<%$ Loc:Message.NameFieldLengthLimit %>"
							ValidateEmptyText="False"
							ClientValidationFunction="Bitrix.AdvertisingBannerEditPageManager.checkName">*</asp:CustomValidator>							
					</td>
				</tr>										
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingBannerActive") %>">
					<td class="field-name" width="40%"><%= GetMessage("FieldLabel.AdvertisingBannerActive")%>:</td>
					<td width="60%"><asp:CheckBox ID="abActive" runat="server" /></td>
				</tr>		
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingBannerSpace") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.AdvertisingBannerSpace")%>:</td>
					<td width="60%">
					    <asp:DropDownList ID="abSpace" runat="server"></asp:DropDownList>
					    <asp:RegularExpressionValidator ID="abSpaceValidatorSelected" runat="server" ValidationGroup="AdvertisingBannerEdit"  ControlToValidate="abSpace" ValidationExpression="^[1-9][0-9]*$" ErrorMessage="<%$ Loc:Message.AdvertisingBannerSpaceIsNotSelected %>" Display="Dynamic">*</asp:RegularExpressionValidator>					    
					    <a href="<%= string.Concat("./AdvertisingSpaceEdit.aspx?", BXConfigurationUtility.Constants.BackUrl, "=", Request.RawUrl)%>">[<%= GetMessage("LinkText.CreateSpace") %>]</a>
					</td>
				</tr>
				<tr class="heading">
					<td colspan="2">
						<%= GetMessage("TabTitle.LimitationSettings")%>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name">
					    <asp:Label runat="server" ID="abEnableFixedRotationLabel" AssociatedControlID="abEnableFixedRotation" Text="<%$ Loc:FieldLabel.AdvertisingBannerEnableFixedRotation%>" />: <span id="abEnableFixedRotationLegendContainer"></span>
				    </td>
					<td width="60%">
					    <asp:CheckBox runat="server" ID="abEnableFixedRotation" />
					</td>
				</tr>					
				<tr id="advertisingBannerRotationModeContainer" valign="top">
					<td class="field-name">
					    <%--<asp:Label runat="server" ID="abEnableUniformRotationVelocityLabel" AssociatedControlID="abEnableUniformRotationVelocity" Text="<%$ Loc:FieldLabel.AdvertisingBannerEnableUniformRotationVelocity %>" />: <span id="abUniformRotationVelocityLegendContainer"></span>--%>
					    <asp:Label runat="server" ID="abRotationModeLabel" AssociatedControlID="abRotationModeStandard" Text="<%$ Loc:FieldLabel.AdvertisingBannerRotationMode %>" />: <span id="abRotationModeLegendContainer"></span>
					</td>
					<td width="60%">
					    <%--<asp:CheckBox runat="server" ID="abEnableUniformRotationVelocity" />--%>
					    <asp:RadioButton runat="server" ID="abRotationModeStandard" GroupName="abRotationMode" Text="<%$ LocRaw:ListItemText.AdvertisingBannerRotationModeByWeight %>" /><br/>
					    <asp:RadioButton runat="server" ID="abRotationModeUniform" GroupName="abRotationMode" Text="<%$ LocRaw:ListItemText.AdvertisingBannerRotationModeWithUniformVelocity %>" /><br/>				    
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name">
					<span id="advertisingBannerRotationPeriodAsterisk" class="required">*</span>
					<asp:Label runat="server" ID="abRotationPeriodLabel" AssociatedControlID="abRotationPeriodStartTbx" Text="<%$ Loc:FieldLabel.AdvertisingBannerRotationPeriod%>" />: <span id="abRotationPeriodLegendContainer"></span></td>
					<td width="60%">
					    <asp:TextBox ID="abRotationPeriodStartTbx" runat="server"/>
					    <bx:Calendar ID="abRotationPeriodStartCalendar" runat="server" TextBoxId="abRotationPeriodStartTbx" />
						<asp:RequiredFieldValidator ID="abRotationPeriodStartValidator" runat="server" ValidationGroup="AdvertisingBannerEdit" ControlToValidate="abRotationPeriodStartTbx" ErrorMessage="<%$ Loc:Message.AdvertisingBannerRotationPeriodStartIsNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>                        					    					    					    
					    <span>&nbsp;-&nbsp;</span>
					    <asp:TextBox ID="abRotationPeriodFinishTbx" runat="server"/>
					    <bx:Calendar ID="abRotationPeriodFinishCalendar" runat="server" TextBoxId="abRotationPeriodFinishTbx" />
						<asp:RequiredFieldValidator ID="abRotationPeriodFinishValidator" runat="server" ValidationGroup="AdvertisingBannerEdit" ControlToValidate="abRotationPeriodFinishTbx" ErrorMessage="<%$ Loc:Message.AdvertisingBannerRotationPeriodFinishIsNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>                        					    					    					    					    
						<asp:CompareValidator runat="server" ID="abRotationPeriodValidator" ValidationGroup="AdvertisingBannerEdit" ControlToValidate="abRotationPeriodStartTbx" ControlToCompare="abRotationPeriodFinishTbx" Type="Date" ErrorMessage="<%$ Loc:Message.RotationPeriodStartGreaterThanFinish %>" Operator="LessThan" EnableClientScript="true">*</asp:CompareValidator>
					</td>
				</tr>				
				<tr id="advertisingBannerWeightContainer" valign="top">
					<td class="field-name">
					    <asp:Label runat="server" ID="abWeightLabel"  AssociatedControlID="abWeight" Text="<%$ Loc:FieldLabel.AdvertisingBannerWeight %>" />: <span id="abWeightLegendContainer"></span>
					</td>
					<td>
						<asp:TextBox ID="abWeight" runat="server" Width="50px" />
						<%--<asp:RequiredFieldValidator ID="abWeightValidator" runat="server" ValidationGroup="AdvertisingBannerEdit" ControlToValidate="abWeight" ErrorMessage="<%$ Loc:Message.AdvertisingBannerWeightIsNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>--%>
						<asp:RegularExpressionValidator ID="abWeightValidatorType" runat="server" ValidationGroup="AdvertisingBannerEdit"  ControlToValidate="abWeight" ValidationExpression="^\d+$" ErrorMessage="<%$ Loc:Message.AdvertisingBannerWeightIsNotNumber %>" Display="Dynamic">*</asp:RegularExpressionValidator>
					</td>
				</tr>												
				<tr id="advertisingBannerMaxDisplayCountContainer" valign="top">
					<td class="field-name"><span id="advertisingBannerMaxDisplayCountAsterisk" class="required">*</span>
					    <asp:Label runat="server" ID="abMaxDisplayCountLabel"  AssociatedControlID="abMaxDisplayCount" Text="<%$ Loc:FieldLabel.AdvertisingBannerMaxDisplayCount %>" />: <span id="abMaxDisplayCountLegendContainer"></span>
					</td>
					<td width="60%">
					    <asp:TextBox runat="server" ID="abMaxDisplayCount" Width="50px" />
						<asp:RequiredFieldValidator ID="abMaxDisplayCountValidator" runat="server" ValidationGroup="AdvertisingBannerEdit" ControlToValidate="abMaxDisplayCount" ErrorMessage="<%$ Loc:Message.AdvertisingBannerMaxDisplayCountIsNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>                        					    
                        <asp:RegularExpressionValidator ID="abMaxDisplayCountValidatorType" runat="server" ValidationGroup="AdvertisingBannerEdit"  ControlToValidate="abMaxDisplayCount" ValidationExpression="^[1-9][0-9]*$" ErrorMessage="<%$ Loc:Message.AdvertisingBannerMaxDisplayCountIsNotNumber %>" Display="Dynamic">*</asp:RegularExpressionValidator>					    
					</td>
				</tr>		    
				<tr id="advertisingBannerMaxVisitorCountContainer" valign="top">
					<td class="field-name">
					    <asp:Label runat="server" ID="abMaxVisitorCountLabel"  AssociatedControlID="abMaxVisitorCount" Text="<%$ Loc:FieldLabel.AdvertisingBannerMaxVisitorCount %>" />: <span id="abMaxVisitorCountLegendContainer"></span>
					</td>
					<td width="60%">
					    <asp:TextBox runat="server" ID="abMaxVisitorCount" Width="50px" />					    
                        <asp:RegularExpressionValidator ID="abMaxVisitorCountValidatorType" runat="server" ValidationGroup="AdvertisingBannerEdit"  ControlToValidate="abMaxVisitorCount" ValidationExpression="^[1-9][0-9]*$" ErrorMessage="<%$ Loc:Message.AdvertisingBannerMaxVisitorCountIsNotNumber %>" Display="Dynamic">*</asp:RegularExpressionValidator>					    
					</td>
				</tr>
				<tr id="advertisingBannerMaxDisplayCountPerVisitorContainer" valign="top">
					<td class="field-name">
                        <asp:Label runat="server" ID="abMaxDisplayCountPerVisitorLabel"  AssociatedControlID="abMaxDisplayCountPerVisitor" Text="<%$ Loc:FieldLabel.AdvertisingBannerMaxDisplayCountPerVisitor %>" />: <span id="abMaxDisplayCountPerVisitorLegendContainer"></span>
                    </td>
					<td width="60%">
					    <asp:TextBox runat="server" ID="abMaxDisplayCountPerVisitor" Width="50px" />				    
                        <asp:RegularExpressionValidator ID="abMaxDisplayCountPerVisitorValidatorType" runat="server" ValidationGroup="AdvertisingBannerEdit"  ControlToValidate="abMaxDisplayCountPerVisitor" ValidationExpression="^[1-9][0-9]*$" ErrorMessage="<%$ Loc:Message.AdvertisingBannerMaxDisplayCountPerVisitorIsNotNumber %>" Display="Dynamic">*</asp:RegularExpressionValidator>					    
					</td>
				</tr>										
				<tr id="advertisingBannerEnableRedirectionCountContainer" valign="top">
					<td class="field-name">
					    <asp:Label runat="server" ID="abEnableRedirectionCountLabel"  AssociatedControlID="abEnableRedirectionCount" Text="<%$ Loc:FieldLabel.AdvertisingBannerEnableRedirectionCount %>" />: <span id="abEnableRedirectionCountLegendContainer"></span>
				    </td>
					<td width="60%">
					    <asp:CheckBox runat="server" ID="abEnableRedirectionCount" />
					</td>
				</tr>
				<tr id="advertisingBannerMaxRedirectionCountContainer" valign="top">
					<td class="field-name">
					    <asp:Label runat="server" ID="abMaxRedirectionCountLabel"  AssociatedControlID="abMaxRedirectionCount" Text="<%$ Loc:FieldLabel.AdvertisingBannerMaxRedirectionCount %>" />: <span id="abMaxRedirectionCountLegendContainer"></span>
					</td>
					<td width="60%">
					    <asp:TextBox runat="server" ID="abMaxRedirectionCount" Width="50px" />				    
                        <asp:RegularExpressionValidator ID="abMaxRedirectionCountValidatorType" runat="server" ValidationGroup="AdvertisingBannerEdit"  ControlToValidate="abMaxRedirectionCount" ValidationExpression="^[1-9][0-9]*$" ErrorMessage="<%$ Loc:Message.AdvertisingBannerMaxRedirectionCountIsNotNumber %>" Display="Dynamic">*</asp:RegularExpressionValidator>					    
					</td>
				</tr>												
			</table>
		</bx:BXTabControlTab>	
		<bx:BXTabControlTab ID="ContentSettingsTab" runat="server" Text="<%$ LocRaw:TabText.Content %>" Title="<%$ LocRaw:TabTitle.Content %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingBannerContentType") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerContentType")%>:</td>
					<td width="60%"><asp:DropDownList ID="abContentType" runat="server"></asp:DropDownList></td>
				</tr>
				<tr id="advertisingBannerContentFileContainer" valign="top" style="display:none;" runat="server" title="<%$ Loc:FieldTooltip.AdvertisingBannerContentFile%>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerContentFile")%>:</td>
					<td width="60%">
                        <bxAdmin:AdminFileField ID="abImageContentFile" runat="server" ShowDescription="false" Editable="true" ModuleId="advertising" SubFolder="banners" />
					</td>					
				</tr>
				<tr id="advertisingBannerFlashContentFileContainer" valign="top" runat="server" style="display:none;" title="<%$ Loc: FieldTooltip.AdvertisingBannerContentFile%>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerContentFile")%>:</td>
					<td width="60%">
                        <bxAdmin:AdminFileField ID="abFlashContentFile" runat="server" ShowDescription="false" Editable="true" ModuleId="advertising" SubFolder="banners" />
					</td>					
				</tr>
				<tr id="advertisingBannerSLContentFileContainer" runat="server" style="display:none;" valign="top" title="<%$ Loc:FieldTooltip.AdvertisingBannerContentFile %>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerContentFile")%>:</td>
					<td width="60%">
                        <bxAdmin:AdminFileField ID="abSLContentFile" Behaviour="Silverlight" LabelTextMessage="QuestionText.AboutXapUploading" runat="server" ShowDescription="false" Editable="true" ModuleId="advertising" SubFolder="banners" />
					</td>					
				</tr>	
				<tr id="advertisingBannerLinkUrlContainer" runat="server" style="display:none;" valign="top" title="<%$Loc:FieldTooltip.AdvertisingBannerLinkUrl%>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerLinkUrl")%>: <span id="abLinkUrlLegendContainer"></span></td>
					<td width="60%">
					    <asp:TextBox ID="abLinkUrl" runat="server" Width="350px" />
					</td>
				</tr>
				<tr id="advertisingBannerLinkTargetContainer" style="display:none;" runat="server" valign="top">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerLinkTarget")%>: <span id="abLinkTargetLegendContainer"></span></td>
					<td width="60%">
                        <asp:RadioButton runat="server" ID="abLinkTargetButtonSef" GroupName="abLinkTarget" Text="<%$ Loc:RadioButtonLabel.LinkTargetSelf %>" /><br />
                        <asp:RadioButton runat="server" ID="abLinkTargetButtonBlank" GroupName="abLinkTarget" Text="<%$ Loc:RadioButtonLabel.LinkTargetBlank %>" /><br />                        
                        <%--<asp:RadioButton runat="server" ID="abLinkTargetButtonParent" GroupName="abLinkTarget" Text="<%$ Loc:RadioButtonLabel.LinkTargetParent %>" /><br />                                                
                        <asp:RadioButton runat="server" ID="abLinkTargetButtonTop" GroupName="abLinkTarget" Text="<%$ Loc:RadioButtonLabel.LinkTargetTop %>" /><br />                                                
                        <asp:RadioButton runat="server" ID="abLinkTargetButtonCustom" GroupName="abLinkTarget" Text="target=" /><asp:TextBox runat="server" ID="abLinkTargetCustomText" Width="100px" />--%>
					</td>
				</tr>
				<tr id="advertisingBannerToolTipContainer" runat="server" style="display:none;" valign="top">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerToolTip")%>: <span id="abToolTipLegendContainer"></span></td>
					<td width="60%">
					    <asp:TextBox ID="abToolTip" runat="server" Width="350px" />
						<asp:CustomValidator ID="abToolTipValidator" runat="server" 
						    ValidationGroup="AdvertisingBannerEdit"
							ControlToValidate="abToolTip"
							ErrorMessage="<%$ Loc:Message.ToolTipFieldLengthLimit %>"
							ValidateEmptyText="False"
							ClientValidationFunction="Bitrix.AdvertisingBannerEditPageManager.checkTooiTip">*</asp:CustomValidator>					    
					</td>
				</tr>
				<tr id="advertisingBannerFlashWModeContainer" style="display:none;" valign="top" runat="server" title="<%$ Loc:FieldTooltip.AdvertisingBannerFlashWMode %>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerFlashWMode")%>:</td>
					<td width="60%"><asp:DropDownList ID="abFlashWMode" runat="server"></asp:DropDownList></td>
				</tr>
				<%--<tr id="advertisingBannerFlashDynamicCreationContainer" valign="top">
					<td class="field-name" width="40%"><%= GetMessage("FieldLabel.AdvertisingBannerFlashDynamicCreation")%>: <span id="abFlashDynamicCreationLegendContainer"></span></td>
					<td width="60%"><asp:CheckBox ID="abFlashDynamicCreation" runat="server" /></td>
				</tr>--%>			
				<tr id="advertisingBannerFlashVersionContainer" style="display:none;" runat="server" valign="top" title="<%$ Loc:FieldTooltip.AdvertisingBannerFlashVersion %>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerFlashVersion")%>:</td>
					<td width="60%">
					    <asp:TextBox ID="abFlashVersion" runat="server" Width="50px" />
						<asp:RegularExpressionValidator ID="abFlashVersionValidatorType" runat="server" ValidationGroup="AdvertisingBannerEdit"  ControlToValidate="abFlashVersion" ValidationExpression="^[1-9][0-9]*$" ErrorMessage="<%$ Loc:Message.AdvertisingBannerFlashPlayerVersionMustBeIntegerGreaterThanZero %>" Display="Dynamic">*</asp:RegularExpressionValidator>
					</td>
				</tr>
				<tr id="advertisingBannerSLVersionContainer" style="display:none;" valign="top" runat="server" title="<%$ Loc:FieldTooltip.AdvertisingBannerSilverlightVersion%>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerSilverlightVersion")%>:</td>
					<td width="60%">
					    <asp:TextBox ID="abSilverlightVersion" runat="server" Width="100px" />
					    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ValidationGroup="AdvertisingBannerEdit"  ControlToValidate="abSilverlightVersion" ValidationExpression="[1-9]\.[0-9]\.[0-9]{5}\.[0-9]" ErrorMessage="<%$ Loc:Message.AdvertisingBannerSLVersionMustBeCorrect %>" Display="Dynamic">*</asp:RegularExpressionValidator>
					</td>
				</tr>
				<tr id="advertisingBannerFlashAltImageFileContainer" style="display:none;" valign="top" runat="server" title="<%$ Loc:FieldTooltip.AdvertisingBannerFlashAltImageFile %>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerFlashAltImageFile")%>:</td>
					<td width="60%">
                        <bxAdmin:AdminFileField ID="abFlashAltImageFile" runat="server" ShowDescription="false" Editable="true" ModuleId="advertising" SubFolder="banners" />
					</td>					
				</tr>
				<tr id="advertisingBannerSLAltImageFileContainer" style="display:none;" valign="top" runat="server" title="<%$ Loc:FieldTooltip.AdvertisingBannerSLAltImageFile %>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerSLAltImageFile")%>:</td>
					<td width="60%">
                        <bxAdmin:AdminFileField ID="abSLAltImageFile" runat="server" ShowDescription="false" Editable="true" ModuleId="advertising" SubFolder="banners" />
					</td>					
				</tr>
				<%--<tr id="advertisingBannerFlashUseCustomUrlContainer" valign="top">
					<td class="field-name" width="40%"><%= GetMessage("FieldLabel.AdvertisingBannerFlashUseCustomUrl")%>: <span id="abFlashUseCustomUrlLegendContainer"></span></td>
					<td width="60%"><asp:CheckBox ID="abFlashUseCustomUrl" runat="server" /></td>
				</tr>--%>				
				<tr id="advertisingBannerFlashLinkUrlContainer" style="display:none;" valign="top" runat="server" title="<%$Loc:FieldTooltip.AdvertisingBannerFlashLinkUrl%>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerFlashLinkUrl")%>: <span id="abFlashLinkUrlLegendContainer"></span></td>
					<td width="60%">
					    <asp:TextBox ID="abFlashLinkUrl" runat="server" Width="350px" />
					</td>
				</tr>
				<tr id="advertisingBannerFlashLinkTargetContainer" style="display:none;" runat="server" valign="top">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerFlashLinkTarget")%>: <span id="abFlashLinkTargetLegendContainer"></span></td>
					<td width="60%">
                        <asp:RadioButton runat="server" ID="abFlashLinkTargetButtonSelf" GroupName="abFlashTarget" Text="<%$ Loc:RadioButtonLabel.LinkTargetSelf %>" /><br />
                        <asp:RadioButton runat="server" ID="abFlashLinkTargetButtonBlank" GroupName="abFlashTarget" Text="<%$ Loc:RadioButtonLabel.LinkTargetBlank %>" /><br />                        
                        <%--<asp:RadioButton runat="server" ID="abFlashLinkTargetButtonParent" GroupName="abFlashTarget" Text="<%$ Loc:RadioButtonLabel.LinkTargetParent %>" /><br />                                                
                        <asp:RadioButton runat="server" ID="abFlashLinkTargetButtonTop" GroupName="abFlashTarget" Text="<%$ Loc:RadioButtonLabel.LinkTargetTop %>" /><br />                                                
                        <asp:RadioButton runat="server" ID="abFlashLinkTargetButtonCustom" GroupName="abFlashTarget" Text="target=" /><asp:TextBox runat="server" ID="abFlashLinkTargetCustomText" Width="100px" />--%> 
					</td>
				</tr>
				<tr id="advertisingBannerFlashToolTipContainer" style="display:none;" runat="server" valign="top">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerFlashToolTip")%>: <span id="abFlashToolTipLegendContainer"></span></td>
					<td width="60%">
					    <asp:TextBox ID="abFlashToolTip" runat="server" Width="350px" />
						<asp:CustomValidator ID="abFlashToolTipValidator" runat="server" 
						    ValidationGroup="AdvertisingBannerEdit"
							ControlToValidate="abFlashToolTip"
							ErrorMessage="<%$ Loc:Message.ToolTipFieldLengthLimit %>"
							ValidateEmptyText="False"
							ClientValidationFunction="Bitrix.AdvertisingBannerEditPageManager.checkTooiTip">*</asp:CustomValidator>						    
					</td>
				</tr>
				<%--<tr>
				    <td align="left" colspan="1">
				        <a id="advertisingBannerHtmlCodeDisplaySwitch"  href="javascript:void(0);" onclick="Bitrix.AdvertisingBannerEditPageManager.instance().switchDisplayHtmlCode(); return false;"><%= GetMessage(IsTextContentEmpty ? "Button.Title.ShowAdditionalHtmlCode" : "Button.Title.HideAdditionalHtmlCode") %></a>
				    </td>
				</tr>--%>
			    <tr id="advertisingBannerHtmlCodeContainer" style="display:none;" runat="server" valign="top" title="<%$ Loc:FieldTooltip.advertisingBannerHtmlCode %>">
				    <td colspan="2" align="center">
		                <bx:BXWebEditor ID="abHtmlCode" runat="server" 
			                Width="100%" Height="400px"
			                StartModeSelector="True"
			                StartMode="HTMLVisual"
			                UseHTMLEditor="False" AutoLoadContent="True" ContentType="Text" FullScreen="False" LimitCodeAccess="False" TemplateId="" UseOnlyDefinedStyles="False" WithoutCode="False">
		                    <Taskbars>
			                    <bx:BXWebEditorBar Name="BXPropertiesTaskbar" />
		                    </Taskbars>
		                    <Toolbars>
			                    <bx:BXWebEditorBar Name="manage" />
			                    <bx:BXWebEditorBar Name="standart" />
			                    <bx:BXWebEditorBar Name="style" />
			                    <bx:BXWebEditorBar Name="formating" />
			                    <bx:BXWebEditorBar Name="source" />
			                    <bx:BXWebEditorBar Name="table" />
		                    </Toolbars>
		                </bx:BXWebEditor>				        
				    </td>
			    </tr>																																	    
			</table>
		</bx:BXTabControlTab>		
	    <bx:BXTabControlTab ID="TargetingTab" runat="server" Text="<%$ LocRaw:TabText.Targeting %>" Title="<%$ LocRaw:TabTitle.Targeting %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">			
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingBannerSites") %>">
                    <td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerSites")%>:
                    <br />
                    <img width="44px" height="21px" border="0px" alt="" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/images/mouse.gif") %>" />
                    </td>
                    <td>
                        <asp:ListBox runat="server" ID="abSites" SelectionMode="Multiple" Width="350px" Height="100px">
                        </asp:ListBox>
                    </td>
				</tr>	
				<tr valign="top" >
                    <td class="field-name" ><%= GetMessage("FieldLabel.AdvertisingPermittedForRotationUrlTemplates")%>: <span id="advertisingPermittedForRotationUrlTemplatesLegendContainer"></span></td>
                    <td>
                        <asp:TextBox runat="server" ID="AdvertisingPermittedForRotationUrlTemplates" TextMode="MultiLine" Height="70px" Width="350px"></asp:TextBox>
                        <br />
                        <%= GetMessage("FieldLegend.AdvertisingRotationUrlTemplates") %>
                    </td>
				</tr>
				<tr valign="top">
                    <td class="field-name"><%= GetMessage("FieldLabel.AdvertisingNotPermittedForRotationUrlTemplates")%>: <span id="advertisingNotPermittedForRotationUrlTemplatesLegendContainer"></span></td>
                    <td>
                        <asp:TextBox runat="server" ID="AdvertisingNotPermittedForRotationUrlTemplates" TextMode="MultiLine" Height="70px" Width="350px"></asp:TextBox>
                        <br />
                        <%= GetMessage("FieldLegend.AdvertisingRotationUrlTemplates") %>                          
                    </td>                  
				</tr>
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingBannerUserRoles") %>">
                    <td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerUserRoles")%>:
                    <br />
                    <img width="44px" height="21px" border="0px" alt="" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/images/mouse.gif") %>" />
                    </td>
                    <td>
                        <asp:RadioButtonList runat="server" ID="EnableRotationForVisitorRoles" RepeatLayout="Flow" RepeatDirection="Vertical"></asp:RadioButtonList>
                        <br />
                        <asp:ListBox runat="server" ID="abUserRoles" SelectionMode="Multiple" Width="350px" Height="100px"></asp:ListBox>
                    </td>
				</tr>
				<tr class="heading">
					<td colspan="2">
						<%= GetMessage("FieldLabel.AdvertisingBannerRotationWeekSchedule")%>
					</td>
				</tr>				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingBannerRotationWeekSchedule") %>">
					<td colspan="2">
					    <table align="center">
					        <tr>
					            <td>
					                <bx:WeekScheduleEditor runat="server" ID="abRotationWeekSchedule" EnableSummary="true"></bx:WeekScheduleEditor>
					            </td>
					        </tr>
					    </table>
					</td>
				</tr>																				
			</table>	        
	    </bx:BXTabControlTab>
	    <bx:BXTabControlTab ID="AdditionalSettingsTab" runat="server" Text="<%$ LocRaw:TabText.AdditionalSettings %>" Title="<%$ LocRaw:TabTitle.AdditionalSettings %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">			
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingBannerDescription") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerDescription")%>:</td>
					<td>
					    <asp:TextBox ID="abDescription" runat="server" Width="250px" TextMode="MultiLine" Rows="5"  />
						<asp:CustomValidator ID="abDescriptionValidator" runat="server" 
						    ValidationGroup="AdvertisingBannerEdit"
							ControlToValidate="abDescription"
							ErrorMessage="<%$ Loc:Message.NameFieldLengthLimit %>"
							ValidateEmptyText="False"
							ClientValidationFunction="Bitrix.AdvertisingBannerEditPageManager.checkDescription">*</asp:CustomValidator>						    
					</td>
				</tr>				
                <tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingBannerCode") %>" >
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerCode")%>:</td>
					<td>
						<asp:TextBox ID="abCode"  runat="server" Width="250px" />	
					</td>
				</tr>				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.XmlId") %>" >
					<td class="field-name"><%= GetMessage("FieldLabel.XmlId") %>:</td>
					<td>
						<asp:TextBox ID="abXmlId"  runat="server" Width="250px" />						
					</td>
				</tr>
		    </table>	        
	     </bx:BXTabControlTab>
	</bx:BXTabControl>
	
	<% if (!Charge.IsNew){%>
	<div>
        <table class="edit-table" cellspacing="0" cellpadding="0" border="0">			
	        <tr class="heading">
		        <td colspan="3">
			        <%= GetMessage("Title.BannerStatistics")%>
		        </td>
	        </tr>
			<tr valign="top">
				<td width="40%" class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerDisplayCount")%>:</td>
				<td width="4%" align="right">
			        <span class="legend" id="abDisplayCountContainer" runat="server">
			            <asp:Literal runat="server" ID="abDisplayCount"/>	
			        </span>										
				</td>
				<td>
				    <asp:LinkButton ID="abResetDisplayCountBtn" runat="server" OnClick="OnResetDisplayCountButtonClick" Text="Reset" CssClass="reset" />
				</td>
			</tr>
			<tr valign="top">
				<td width="40%" class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerVisitorCount")%>:</td>			
			    <td width="4%" align="right">
			        <span class="legend" id="abVisitorCountContainer" runat="server">
			            <asp:Literal ID="abVisitorCount" runat="server"/>
			        </span>			
                </td>
                <td>
			        <asp:LinkButton ID="abResetVisitorCountBtn" runat="server" OnClick="OnResetVisitorCountButtonClick" Text="Reset" CssClass="reset" />                
                </td>
			</tr>
			<tr valign="top">
			    <td width="40%" class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerRedirectionCount")%>:</td>
			    <td width="4%" align="right">
			        <span class="legend" id="abRedirectionCountContainer" runat="server">
			            <asp:Literal ID="abRedirectionCount" runat="server"/>
			        </span>					    
			    </td>
			    <td>
                    <asp:LinkButton ID="abResetRedirectionCountBtn" runat="server" OnClick="OnResetRedirectionCountButtonClick" Text="Reset" CssClass="reset" />
			    </td>			
			</tr>
			<tr valign="top">
				<td width="40%" class="field-name"><%= GetMessage("FieldLabel.AdvertisingBannerCTR")%>: <span id="abCTRLegendContainer"></span></td>
				<td width="4%" align="right">
				    <span class="legend">
				        <b><asp:Literal runat="server" ID="abCTR"></asp:Literal></b>
				    </span>
				</td>
				<td></td>
			</tr>        
       </table> 
        <asp:HiddenField ID="hfSLWidth" runat="server" value="" />
        <asp:HiddenField ID="hfSLHeight" runat="server" value="" />
	</div>
	<%} %>
	<script type="text/javascript" language="javascript">
	    
        if(typeof(Bitrix) == "undefined"){
	        var Bitrix = new Object();
        }	    
	    
	    Bitrix.AdvertisingBannerEditContentType = {
	        image:  1,
	        flash:  2,
	        textOnly:   3,
	        silverLight:4
	    }
	    
	    if ( typeof(Bitrix.AdminSilverlightError)=="undefined" ){
	        Bitrix.AdminSilverlightError = new Object();
	    }
	    Bitrix.AdminSilverlightError.ErrorTitle='<%=GetMessageJS("SilverlightError.Title") %>';
	    Bitrix.AdminSilverlightError.ErrorCode ='<%=GetMessageJS("SilverlightError.ErrorCode") %>';
	    Bitrix.AdminSilverlightError.ErrorMessage='<%=GetMessageJS("SilverlightError.ErrorMessage") %>';
	    Bitrix.AdminSilverlightError.ErrorType='<%=GetMessageJS("SilverlightError.ErrorType") %>';
	    Bitrix.AdminSilverlightError.ChooseAnotherFile = '<%=GetMessageJS("SilverlightError.ChooseAnotherFile") %>';
	    
	    
	    Bitrix.AdvertisingBannerEditPageManager = function Bitrix$AdvertisingBannerEditPageManager(){
            if(typeof(Bitrix.AdvertisingBannerEditPageManager.initializeBase) == "function")
                Bitrix.AdvertisingBannerEditPageManager.initializeBase(this);
	        this._initialized = false;
	        
	        this._contentTypeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleContentTypeChange);
	        this._flashUseCustomUrlHandler = Bitrix.TypeUtility.createDelegate(this, this._handleFlashUseCustomUrl);
	        this._fixedRotationFlagHandler = Bitrix.TypeUtility.createDelegate(this, this._handleFixedRotationFlagChange);
	        this._uniformVelocityFlagHandler = Bitrix.TypeUtility.createDelegate(this, this._handleUniformVelocityFlagChange);
	        this._redirectionCountFlagHandler = Bitrix.TypeUtility.createDelegate(this, this._handleRedirectionCountFlagChange);
	        
	        this._contentTypeId = "<%= abContentType.ClientID %>";	
            this._enableFixedRotationId = "<%= abEnableFixedRotation.ClientID %>";
            this._rotationModeStandardId = "<%= abRotationModeStandard.ClientID %>";
            this._rotationModeUniformId = "<%= abRotationModeUniform.ClientID %>";
            this._maxDisplayCountId = "<%= abMaxDisplayCount.ClientID %>";
            this._maxVisitorCountId = "<%= abMaxVisitorCount.ClientID %>";
            this._maxDisplayCountPerVisitorId = "<%= abMaxDisplayCountPerVisitor.ClientID %>";
            this._enableRedirectionCountId = "<%= abEnableRedirectionCount.ClientID %>";            
            this._maxRedirectionCountId = "<%= abMaxRedirectionCount.ClientID %>"; 
            this._weightId = "<%= abWeight.ClientID %>";   
            this._rotationPeriodStartId = "<%= abRotationPeriodStartTbx.ClientID %>";  
            this._rotationPeriodFinishId = "<%= abRotationPeriodFinishTbx.ClientID %>";        
                  
            this._weightValidatorTypeId = "<%= abWeightValidatorType.ClientID %>";
            this._maxDisplayCountValidatorRequiredId = "<%= abMaxDisplayCountValidator.ClientID %>";
            this._maxDisplayCountValidatorTypeId = "<%= abMaxDisplayCountValidatorType.ClientID %>";
            this._rotationPeriodStartValidatorRequiredId = "<%= abRotationPeriodStartValidator.ClientID %>";
            this._rotationPeriodFinishValidatorRequiredId = "<%= abRotationPeriodFinishValidator.ClientID %>";
            this._flashVersionValidatorTypeId = "<%= abFlashVersionValidatorType.ClientID %>";
            this._maxVisitorCountValidatorTypeId = "<%= abMaxVisitorCountValidatorType.ClientID %>";
            this._maxDisplayCountPerVisitorValidatorTypeId = "<%= abMaxDisplayCountPerVisitorValidatorType.ClientID %>";
            this._maxRedirectionCountValidatorTypeId = "<%= abMaxRedirectionCountValidatorType.ClientID %>"; 
            this._tooltipValidator = "<%= abToolTipValidator.ClientID %>";
            this._flashTooltipValidator = "<%= abFlashToolTipValidator.ClientID %>";

	        this._contentFileContainerId = "<%= advertisingBannerContentFileContainer.ClientID %>";
	        this._linkUrlContainerId = "<%= advertisingBannerLinkUrlContainer.ClientID%>";
	        this._linkTargetContainerId = "<%= advertisingBannerLinkTargetContainer.ClientID%>";
	        this._toolTipContainerId = "<%=advertisingBannerToolTipContainer.ClientID%>";
	        this._flashContentFileContainerId = "<%=advertisingBannerFlashContentFileContainer.ClientID%>";
	        this._flashWModeContainerId = "<%=advertisingBannerFlashWModeContainer.ClientID%>"; 
	        this._flashVersionContainerId = "<%=advertisingBannerFlashVersionContainer.ClientID%>"; 
	        this._flashLinkUrlContainerId = "<%=advertisingBannerFlashLinkUrlContainer.ClientID%>";  
	        this._flashLinkTargetContainerId = "<%=advertisingBannerFlashLinkTargetContainer.ClientID%>";
	        this._flashToolTipContainerId = "<%=advertisingBannerFlashToolTipContainer.ClientID%>";
	        this._htmlCodeDisplaySwitchId = "advertisingBannerHtmlCodeDisplaySwitch";
	        this._htmlCodeContainerId = "<%=advertisingBannerHtmlCodeContainer.ClientID%>";
	        this._maxDisplayCountContainerId = "advertisingBannerMaxDisplayCountContainer";
	        this._maxVisitorCountContainerId = "advertisingBannerMaxVisitorCountContainer";
	        this._maxDisplayCountPerVisitorContainerId = "advertisingBannerMaxDisplayCountPerVisitorContainer";
	        this._enableRedirectionCountContainerId = "advertisingBannerEnableRedirectionCountContainer";
	        this._maxRedirectionCountContainerId = "advertisingBannerMaxRedirectionCountContainer";
	        this._weightContainerId = "advertisingBannerWeightContainer";
	        this._maxDisplayCountAsteriskId = "advertisingBannerMaxDisplayCountAsterisk";
	        this._rotationPeriodAsteriskId = "advertisingBannerRotationPeriodAsterisk";
	        this._permittedForRotationUrlTemplatesLegendContainerId = "advertisingPermittedForRotationUrlTemplatesLegendContainer";
	        this._notPermittedForRotationUrlTemplatesLegendContainerId = "advertisingNotPermittedForRotationUrlTemplatesLegendContainer";	
	        this._linkUrlLegendContainerId = "abLinkUrlLegendContainer";    
	        this._flashLinkUrlLegendContainerId = "abFlashLinkUrlLegendContainer";    
	        this._linkTargetLegendContainerId = "abLinkTargetLegendContainer";
	        this._flashLinkTargetLegendContainerId = "abFlashLinkTargetLegendContainer";
	        this._toolTipLegendContainerId = "abToolTipLegendContainer";
	        this._flashToolTipLegendContainerId = "abFlashToolTipLegendContainer";
	        this._weightLegendContainerId = "abWeightLegendContainer";
	        this._rotationModeLegendContainerId = "abRotationModeLegendContainer";
	        this._enableFixedRotationLegendContainerId = "abEnableFixedRotationLegendContainer";
	        this._abCTRLegendContainerId = "abCTRLegendContainer";
	        this._rotationPeriodLegendContainerId = "abRotationPeriodLegendContainer";
	        this._maxDisplayCountLegendContainerId = "abMaxDisplayCountLegendContainer";
	        this._maxDisplayCountPerVisitorLegendContainer = "abMaxDisplayCountPerVisitorLegendContainer";
            this._maxVisitorCountLegendContainerId = "abMaxVisitorCountLegendContainer";
            this._enableRedirectionCountLegendContainerId = "abEnableRedirectionCountLegendContainer";
            this._maxRedirectionCountLegendContainerId = "abMaxRedirectionCountLegendContainer";
            this._flashAltImageFileContainerId = "<%=advertisingBannerFlashAltImageFileContainer.ClientID%>";
            this._SLContentFileContainerId = "<%=advertisingBannerSLContentFileContainer.ClientID%>";
            this._SLContentFileVersionContainerId = "<%=advertisingBannerSLVersionContainer.ClientID%>";
            this._SLAltImageFileContainerId = '<%=advertisingBannerSLAltImageFileContainer.ClientID %>';
            
            this._rotationModeLabelId = "<%= abRotationModeLabel.ClientID %>";
            this._rotationPeriodLabelId = "<%= abRotationPeriodLabel.ClientID %>";
            this._weightLabellId = "<%= abWeightLabel.ClientID %>";
            this._maxDisplayCountLabelId = "<%= abMaxDisplayCountLabel.ClientID %>";
            this._maxVisitorCountLabelId = "<%= abMaxVisitorCountLabel.ClientID %>";
            this._maxDisplayCountPerVisitorLabelId = "<%= abMaxDisplayCountPerVisitorLabel.ClientID %>";
            this._maxRedirectionCountLabelId = "<%= abMaxRedirectionCountLabel.ClientID %>";
            this._abSLContentFileId = "<%=abSLContentFile.ClientID %>";
            this._slContainerId= "<%=abSLContentFile.ClientID %>_SwfContainer";
            this._slUploadId= "<%=abSLContentFile.ClientID %>_ValueUpload";
            this._slfileLabelId = "<%=abSLContentFile.ClientID%>_Lbl";
            
	    }
	    
	    Bitrix.AdvertisingBannerEditPageManager.prototype = {
	        initialize: function(){
                Bitrix.EventUtility.addEventListener(document.getElementById(this._contentTypeId), "change", this._contentTypeHandler);	        
                Bitrix.EventUtility.addEventListener(document.getElementById(this._enableFixedRotationId), "click", this._fixedRotationFlagHandler);	                                                   
                Bitrix.EventUtility.addEventListener(document.getElementById(this._rotationModeStandardId), "click", this._uniformVelocityFlagHandler); 
                Bitrix.EventUtility.addEventListener(document.getElementById(this._rotationModeUniformId), "click", this._uniformVelocityFlagHandler);                 
                Bitrix.EventUtility.addEventListener(document.getElementById(this._enableRedirectionCountId), "click", this._redirectionCountFlagHandler);
	            this._initialized = true;
	        },
	        prepare: function()
	        {
	            this.layout();
	            this.createHints();
	            this.prepareSilverlight();
	        },
            getContentType: function(){
                if(!this._initialized) throw "Is not initialized!";
                elem = document.getElementById(this._contentTypeId);
                if(!elem) throw "Could not find ContentType element!";
                switch(elem.value){
                    case "1":
                        return Bitrix.AdvertisingBannerEditContentType.image;
                    case "2":
                        return Bitrix.AdvertisingBannerEditContentType.flash;
                    case "3":
                        return Bitrix.AdvertisingBannerEditContentType.textOnly;
                    case "4":
                        return Bitrix.AdvertisingBannerEditContentType.silverLight;
                    default:
                        throw "Could not parse string '" + elem.value + "' to Bitrix.AdvertisingBannerEditContentType!";                        
                }
            },
            	        
	        layout: function(){
	            if(!this._initialized) throw "Is not initialized!";
	            this._layoutContentSection();
	            this._layoutLimitationSettingsSection();
	        },
	        createHints: function()
	        {
	            this.createHint(this._permittedForRotationUrlTemplatesLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingPermittedForRotationUrlTemplate") %>");
	            this.createHint(this._notPermittedForRotationUrlTemplatesLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingNotPermittedForRotationUrlTemplate") %>");
	            this.createHint(this._linkUrlLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerLinkUrl") %>");
	            this.createHint(this._flashLinkUrlLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerLinkUrl") %>");
	            this.createHint(this._linkTargetLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerLinkTarget") %>");
	            this.createHint(this._flashLinkTargetLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerLinkTarget") %>");
	            this.createHint(this._toolTipLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerToolTip") %>");
	            this.createHint(this._flashToolTipLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerToolTip") %>");
	            this.createHint(this._weightLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerWeight") %>");
	            this.createHint(this._rotationModeLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerRotationMode") %>"); 
	            this.createHint(this._enableFixedRotationLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerEnableFixedRotation") %>");           
	            this.createHint(this._abCTRLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerCTR") %>");
                this.createHint(this._rotationPeriodLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerRotationPeriod") %>");
                this.createHint(this._maxDisplayCountLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerMaxDisplayCount") %>");
                this.createHint(this._maxDisplayCountPerVisitorLegendContainer, "<%= GetMessageJS("FieldLegend.AdvertisingBannerMaxDisplayCountPerVisitor") %>");
                this.createHint(this._maxVisitorCountLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerMaxVisitorCount") %>");
                this.createHint(this._enableRedirectionCountLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerEnableRedirectionCount") %>");
                this.createHint(this._maxRedirectionCountLegendContainerId, "<%= GetMessageJS("FieldLegend.AdvertisingBannerMaxRedirectionCount") %>");               
	        },
            _handleContentTypeChange: function(){
                this._layoutContentSection();
            },
                       
            _handleFlashUseCustomUrl: function(){
                this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.flash, true);            
            },
            _handleFixedRotationFlagChange: function(){
                this._layoutLimitationSettingsSection();
            },
            _handleUniformVelocityFlagChange: function(){
                this._layoutLimitationSettingsSection();
            }, 
            _handleRedirectionCountFlagChange: function(){
                this._layoutLimitationSettingsSection();
            },           
            _layoutContentSection: function(){
                var contentType = this.getContentType();
                switch(contentType){
                    case Bitrix.AdvertisingBannerEditContentType.image: {
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.flash, false);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.image, true);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.textOnly, false);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.silverLight, false);                                           
                    }
                    break;
                    case Bitrix.AdvertisingBannerEditContentType.flash: {
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.image, false);                       
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.textOnly, false);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.silverLight, false);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.flash, true);                           
                    }
                    break;
                    case Bitrix.AdvertisingBannerEditContentType.textOnly: {
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.image, false);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.flash, false);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.textOnly, true);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.silverLight, false);
                    }
                    break;
                    case Bitrix.AdvertisingBannerEditContentType.silverLight: {
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.image, false);                       
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.flash, false);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.textOnly, false);
                        this._displayContentSubsection(Bitrix.AdvertisingBannerEditContentType.silverLight, true);                    
                    }
                    break;         
                }
                
                if(typeof(ValidatorEnable) == "function"){
                    ValidatorEnable(document.getElementById(this._flashVersionValidatorTypeId), contentType == Bitrix.AdvertisingBannerEditContentType.flash);
                    ValidatorEnable(document.getElementById(this._tooltipValidator), contentType == Bitrix.AdvertisingBannerEditContentType.image);
                    ValidatorEnable(document.getElementById(this._flashTooltipValidator), contentType == Bitrix.AdvertisingBannerEditContentType.flash);                                                                      
                }
            },
            
            _displayContentSubsection: function(contentType, display)
            {
                switch(contentType){
                    case Bitrix.AdvertisingBannerEditContentType.image: {
                        this._displayElement(this._contentFileContainerId, display);
                        this._displayElement(this._linkUrlContainerId, display);
                        this._displayElement(this._linkTargetContainerId, display);
                        this._displayElement(this._toolTipContainerId, display);                                                
                    }
                    break;
                    case Bitrix.AdvertisingBannerEditContentType.flash: {
                        this._displayElement(this._flashContentFileContainerId, display);
                        this._displayElement(this._flashWModeContainerId, display);
                        this._displayElement(this._flashVersionContainerId, display);
                        
                        this._displayElement(this._flashLinkUrlContainerId, display);
                        this._displayElement(this._flashLinkTargetContainerId, display);
                        this._displayElement(this._flashToolTipContainerId, display); 
                        this._displayElement(this._flashAltImageFileContainerId, display);                                                                  
                    }
                    break;
                    case Bitrix.AdvertisingBannerEditContentType.silverLight: {
                        this._displayElement(this._SLContentFileContainerId,display);
                        this._displayElement(this._flashLinkUrlContainerId, display);
                        this._displayElement(this._flashLinkTargetContainerId, display);
                        this._displayElement(this._flashToolTipContainerId, display); 
                        this._displayElement(this._SLAltImageFileContainerId, display);
                        this._displayElement(this._SLContentFileVersionContainerId,display);
                                                                    
                    }
                    break;
                    case Bitrix.AdvertisingBannerEditContentType.textOnly: {
                        this._displayElement(this._htmlCodeContainerId, display);                          
                    }  
                    break;  
                    default: {
                        throw "Unknown content type " + contentType.toString() + "!";                
                    }
                }               
            },
            _enableElement: function(elementId, enable){
                var element = document.getElementById(elementId);
                if(!element) throw "Could not find element '" + elementId.toString() + "'!";
                if(typeof(element.disabled) != "undefined")
                    element.disabled = !enable;
                else
                    element.setAttribute("disabled", !enable);                
            },
            _displayElement: function(elementId, display){
                var element = document.getElementById(elementId);
                if(!element) throw "Could not find element '" + elementId.toString() + "'!";
                element.style.display = display ? "" : "none";
            },
            _layoutLimitationSettingsSection: function(){
                var enableFixedRotation = document.getElementById(this._enableFixedRotationId).checked;
                var fixedRotationDisplay = enableFixedRotation ? "" : "none";
                var enableRedirectionCount = document.getElementById(this._enableRedirectionCountId).checked;
                var aboutUniformVelocity = enableFixedRotation && document.getElementById(this._rotationModeUniformId).checked;
               
                this._enableElement(this._weightLabellId, !aboutUniformVelocity);
                this._enableElement(this._weightId, !aboutUniformVelocity);              
               
                this._enableElement(this._rotationModeLabelId, enableFixedRotation); 
                this._enableElement(this._rotationModeStandardId, enableFixedRotation);
                this._enableElement(this._rotationModeUniformId, enableFixedRotation);

                this._enableElement(this._maxDisplayCountLabelId, enableFixedRotation);
                this._enableElement(this._maxDisplayCountId, enableFixedRotation);
                this._enableElement(this._maxVisitorCountLabelId, enableFixedRotation);
                this._enableElement(this._maxVisitorCountId, enableFixedRotation);

                this._enableElement(this._maxDisplayCountPerVisitorLabelId, enableFixedRotation);                
                this._enableElement(this._maxDisplayCountPerVisitorId, enableFixedRotation);
                this._enableElement(this._maxRedirectionCountLabelId, enableRedirectionCount);
                this._enableElement(this._maxRedirectionCountId, enableRedirectionCount);
                
                this._displayElement(this._maxDisplayCountAsteriskId, aboutUniformVelocity);
                this._displayElement(this._rotationPeriodAsteriskId, aboutUniformVelocity);;

                if(typeof(ValidatorEnable) == "function"){
                    ValidatorEnable(document.getElementById(this._weightValidatorTypeId), !aboutUniformVelocity);
                    ValidatorEnable(document.getElementById(this._maxDisplayCountValidatorRequiredId), aboutUniformVelocity);
                    ValidatorEnable(document.getElementById(this._maxDisplayCountValidatorTypeId), enableFixedRotation);  
                    ValidatorEnable(document.getElementById(this._rotationPeriodStartValidatorRequiredId), aboutUniformVelocity);   
                    ValidatorEnable(document.getElementById(this._rotationPeriodFinishValidatorRequiredId), aboutUniformVelocity);   
                    ValidatorEnable(document.getElementById(this._maxVisitorCountValidatorTypeId), enableFixedRotation); 
                    ValidatorEnable(document.getElementById(this._maxDisplayCountPerVisitorValidatorTypeId), enableFixedRotation);
                    ValidatorEnable(document.getElementById(this._maxRedirectionCountValidatorTypeId), enableRedirectionCount);                                      
                }
            },
            switchDisplayHtmlCode: function(){
                var edElem = document.getElementById(this._htmlCodeContainerId); 
                if(!edElem) return;
                this.displayHtmlCode(!(edElem.style.display != "none")); 
            },
            displayHtmlCode: function(display){
                var edElem = document.getElementById(this._htmlCodeContainerId); 
                if(!edElem) return;
                var curDisplay = edElem.style.display != "none";
                if(curDisplay != display)
                    edElem.style.display = display ? "" : "none";
                var switchElem = document.getElementById(this._htmlCodeDisplaySwitchId);
                if(switchElem)
                    switchElem.innerHTML = display ? "<%= GetMessage("Button.Title.HideAdditionalHtmlCode") %>" : "<%= GetMessage("Button.Title.ShowAdditionalHtmlCode") %>";               
            },
            createHint: function(containerId, html){
                if(typeof(BXHint) == 'undefined') return;
                var container = document.getElementById(containerId);
                if(!container) return;
		        container.appendChild((new BXHint(html)).oIcon);                
            },
            prepareSilverlight:function(){
                if(Silverlight){
                    if (!Silverlight.isInstalled()){//silverlight is not installed, don't allow user to make silverlight banners
                        
                        var fileUpload = document.getElementById(this._slUploadId);
                        if(fileUpload) fileUpload.disabled =true;
                        
                        
                        
                        var lbl = document.getElementById(this._slfileLabelId);
                        if (lbl){
                            lbl.innerText="";
                            lbl.textContent="";
                            var font = document.createElement("font");
                            font.color="red";
                            
                            font.innerText='<%=GetMessage("Silverlight.IsNotInstalled") %>';
                            font.textContent = '<%= GetMessage("Silverlight.IsNotInstalled")%>';
                            lbl.appendChild(font);
                        }
                            var div = document.createElement("DIV");
                            
                            var slLink = document.createElement("A");
                            slLink.href="#";
                            slLink.textContent='<%=GetMessage("Silverlight.GetSilverlight") %>';
                            slLink.innerText='<%=GetMessage("Silverlight.GetSilverlight") %>';
                            slLink.title='<%=GetMessage("Silverlight.GetSilverlight") %>';
                            slLink.style.float="left";
                                           	                                         
                            if(lbl){ 
                                div.style.marginTop="5px";
                                div.style.marginBottom="5px";
                                div.appendChild(slLink);
                                lbl.appendChild(div);
                                Bitrix.EventUtility.addEventListener(slLink, "click", function() { return Silverlight.getSilverlight('');});
                            }
                        
                    }
                }
            }
	    }
	    
	    Bitrix.AdvertisingBannerEditPageManager._instance = null;
	    Bitrix.AdvertisingBannerEditPageManager.instance = function(){
	        if(this._instance == null){
	            this._instance = new Bitrix.AdvertisingBannerEditPageManager();
	            this._instance.initialize();   
	        }
	        return this._instance;
	    }
	    
	    Bitrix.AdvertisingBannerEditPageManager.checkTooiTip = function(sender, args)
	    {
	        args.IsValid = args.Value.length <= 512;
	    }

	    Bitrix.AdvertisingBannerEditPageManager.checkName = function(sender, args)
	    {
	        args.IsValid = args.Value.length <= 256;
	    }
	    
	    Bitrix.AdvertisingBannerEditPageManager.checkDescription = function(sender, args)
	    {
	        args.IsValid = args.Value.length <= 2048;
	    }	    
	    
       window.setTimeout(function(){Bitrix.AdvertisingBannerEditPageManager.instance().prepare();}, 150);
    </script>
</asp:Content>

