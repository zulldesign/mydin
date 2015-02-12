<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="Feedback" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server" > 

  <bx:IncludeComponent 
	  id="Feedback" 
	  runat="server" 
	  componentname="bitrix:iblock.element.webform" 
	  template="template1" 
	  IBlockTypeId="" 
	  IBlockId="<%$ Options:Bitrix.BankSite:FeedBackIBlockId,int %>" 
	  ElementId="0" 
	  EditFields="PROPERTY_TOPIC;Name;PROPERTY_EMAIL;PROPERTY_PHONE;DetailText;Captcha" 
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
	  SuccessMessageAfterCreateElement="Your request has been sent." 
	  SuccessMessageAfterUpdateElement="" 
	  RedirectPageUrl="" 
	  SendEmailAfterCreate="True" 
	  EmailSubject="#SITE_NAME#:  A feedback form message" 
	  EmailTo="<%$ Options:Bitrix.BankSite:ContactsEmail %>"  
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
 
 </asp:Content>
