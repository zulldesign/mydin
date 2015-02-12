<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Options.ascx.cs" Inherits="BitrixCommunityOptions" EnableViewState="false" %>

<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgInnerForm" HeaderText="<%$ Loc:Kernel.Error %>" />
<% if (Request["status"] == "ok") { %> 
<bx:BXMessage ID="successMessage" runat="server" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Content="Parameters have been saved successfully." />
<% } %>


<bx:BXTabControl ID="TabControl" ValidationGroup="vgInnerForm" runat="server" OnCommand="TabControl_Command">
	<bx:BXTabControlTab runat="server" ID="Tab" Selected="True" Title="Component parameters" Text="Components">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
						
			<tr valign="top">
				<td class="field-name" width="40%">Site Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="SiteFolder" /></td>
			</tr>			
			
		    <tr class="heading">
				<td colspan="2">Blogs</td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="BlogSefFolder" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Blog Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="BlogUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Blog Post Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="PostViewUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Blog Tags:</td>
				<td width="60%"><asp:TextBox runat="server" ID="SearchTagsUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Blog Post Comments RSS:</td>
				<td width="60%"><asp:TextBox runat="server" ID="PostRssUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">New Blog:</td>
				<td width="60%"><asp:TextBox runat="server" ID="NewBlogUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Post Edit:</td>
				<td width="60%"><asp:TextBox runat="server" ID="PostEditUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">New Post:</td>
				<td width="60%"><asp:TextBox runat="server" ID="NewPostUrlTemplate" width="300px" /></td>
			</tr>	
			
			<tr valign="top">
				<td class="field-name" width="40%">Blog Color Theme Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="BlogColorScheme" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Category ID For New Blogs:</td>
				<td width="60%"><asp:TextBox runat="server" ID="BlogCategoryId" width="300px" /></td>
			</tr>											
			
		    <tr class="heading">
				<td colspan="2">Forums</td>
			</tr>			
			
			<tr valign="top">
				<td class="field-name" width="40%">SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="ForumSefFolder" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Topic View Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="TopicReadUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Forum Topics Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="ForumReadUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Forum Color Theme Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="ForumColorScheme" width="300px" /></td>
			</tr>				
			
		    <tr class="heading">
				<td colspan="2">Users</td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="PeopleSefFolder" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">User Profile:</td>
				<td width="60%"><asp:TextBox runat="server" ID="UserProfileUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">User Profile Edit Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="UserProfileEditUrlTemplate" width="300px" /></td>
			</tr>			
			
			<tr valign="top">
				<td class="field-name" width="40%">User PM Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="UserMailUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">PM Creation Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="UserMailNewUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">PM View Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="UserMailReadUrlTemplate" width="300px" /></td>
			</tr>

			<tr valign="top">
				<td class="field-name" width="40%">Recipient Aware PM Creation Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="UserMailNewForUsersUrlTemplate" width="300px" /></td>
			</tr>
			
		    <tr class="heading">
				<td colspan="2">Authorization</td>
			</tr>

			<tr valign="top">
				<td class="field-name" width="40%">SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="LoginSefFolder" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Login Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="LoginUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Logout Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="LogoutUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">User Registration Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="RegistrationUrlTemplate" width="300px" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Password Recovery Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="RecoveryUrlTemplate" width="300px" /></td>
			</tr>												
			
		    <tr class="heading">
				<td colspan="2">Include Areas</td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Website Logo Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="TemplateLogoIncludeArea" width="300px" /></td>
			</tr>	
			
			<tr valign="top">
				<td class="field-name" width="40%">Website Header Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="TemplateLogoText" width="300px" /></td>
			</tr>	
			
			<tr valign="top">
				<td class="field-name" width="40%">Website Footer Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="CopyrightText" width="300px" /></td>
			</tr>													
			
		    <tr class="heading">
				<td colspan="2">Search</td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Search Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="SearchPageUrlTemplate" width="300px" /></td>
			</tr>								

		</table>
	</bx:BXTabControlTab>
</bx:BXTabControl>
