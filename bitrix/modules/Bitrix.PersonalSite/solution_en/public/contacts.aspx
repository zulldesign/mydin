<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main"  Title="Contact" %>
<asp:Content ID="Content" ContentPlaceHolderID="BXContent" runat="server" >
<bx:IncludeComponent 
	id="Feedback" 
	runat="server" 
	componentname="bitrix:iblock.element.webform" 
	template=".default" 
	IBlockTypeId="" 
	IBlockId="<%$ Options:Bitrix.PersonalSite:ContactsFeedbackIBlockId,int %>" 
	ElementId="0" 
	EditFields="Name;PROPERTY_EMAIL;DetailText;Captcha" 
	RequiredFields="Name;DetailText" 
	EnableActivationPeriodProcessing="False" 
	DaysBeforeActivationPeriodStart="0" 
	ActivationPeriodLengthInDays="0" 
	MannerOfUserIdentification="Current" 
	CustomUserId="0" 
	RolesAuthorizedToManage="Guest" 
	RolesAuthorizedToAdminister="" 
	MaxUserElements="0" 
	MannerOfIssueModificationPermission="Active" 
	RolesAuthorizedToManageOfActivation="" 
	ElementActiveAfterSave="Active" 
	MannerOfUserAssociation="CreatedBy" 
	UserAssociatedByCustomIBlockProperty="" 
	MaxSectionSelect="3" 
	OnlyLeafSelect="False" 
	MaxFileSizeUpload="1024" 
	ActiveFromDateShowTime="False" 
	ActiveToDateShowTime="False" 
	NameFieldMacros="" 
	TextBoxSize="30" 
	CreateButtonTitle="Send" 
	UpdateButtonTitle="" 
	SuccessMessageAfterCreateElement="Thank you! Your message has been submitted." 
	SuccessMessageAfterUpdateElement="" 
	RedirectPageUrl="" 
	SendEmailAfterCreate="True" 
	EmailSubject="#SITE_NAME#: A feedback form message" 
	EmailTo="<%$ Options:Bitrix.PersonalSite:ContactsEmail %>" 
	EmailMessageTemplate="#Name# has left you a message:

#DetailText#

--------------------
Use the following e-mail to reply to this message: #Property_EMAIL#" 
	ActiveCustomTitle="" 
	NameCustomTitle="Name" 
	ActiveFromDateCustomTitle="" 
	ActiveToDateCustomTitle="" 
	SectionsCustomTitle="" 
	PreviewTextCustomTitle="" 
	PreviewImageCustomTitle="" 
	DetailTextCustomTitle="Message text" 
	DetailImageCustomTitle="" 
	CaptchaCustomTitle="CAPTCHA" 
	Template_UserCssClass="feedback-form" 
	Template_ShowAsterisk="False" 
/>




<h1>Contact Information</h1>
 
<div class="hr"></div>
 
<ul> 	 
  <li>E-mail: <a href="mailto:19Victoria84@gmail.com">19Victoria84@gmail.com</a>.</li>
  <li>Skype: <a href="callto:Morrison_Victoria">Morrison_Victoria</a>.</li>
 </ul>
 </asp:Content>