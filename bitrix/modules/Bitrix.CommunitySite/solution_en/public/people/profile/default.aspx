<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="" %>
<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server" >


<bx:IncludeComponent 
	id="UserProfile" 
	runat="server" 
	componentname="bitrix:user.profile.view" 
	template="view" 
	UserId="<%$ Request:user %>" 
	Fields="Image;Property_REGION;Gender;BirthdayDate;Property_ABOUT;Property_URL;Property_TWITTER;Property_OCCUPATION;Property_INTERESTS;Property_SKYPE;Property_ICQ;BitrixForum_Signature;Property_RATING;BitrixRatingVoting_Voting" 
	EditProfileTitle="Edit Profile" 
	EditProfileUrlTemplate="<%$ Options:Bitrix.CommunitySite:UserProfileEditUrlTemplate %>" 
	EnableVoting="True" 
	RolesAuthorizedToVote="User" 
/>

</asp:Content>
