<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Options.ascx.cs" Inherits="bitrix_modules_main_Options" EnableViewState="false" %>

<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgInnerForm" HeaderText="<%$ Loc:Kernel.Error %>" />
<bx:BXMessage ID="successMessage" runat="server" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>"
	Visible="False" Content="<%$ Loc:Message.SettingsHasBeenSavedSuccessfully %>" />

<bx:InlineScript runat="server" ID="Script">
<script type="text/javascript">
	function MailerDefaultEmailFromValidator_Validate(sender, args)
	{
		args.IsValid = document.getElementById('<%= MailerSmtpHost.ClientID %>').value == '' || args.Value != '';
	}
</script>
</bx:InlineScript>

<bx:BXTabControl ID="BXTabControl1" ValidationGroup="vgInnerForm" runat="server" OnCommand="BXTabControl1_Command">
	<bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText.AdjustmentOfModuleParameters %>"
		Title="<%$ Loc:Kernel.TopPanel.Settings %>">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.SiteTitle") %>
				</td>
				<td width="60%">
					<asp:TextBox ID="tbSiteName" ValidationGroup="vgInnerForm" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="AutoCache" Text="<%$ Loc:CheckBoxText.PerformAutocaching %>" />
				</td>
			</tr>			
			<tr class="heading">
				<td colspan="2">
					<%= GetMessage("Legend.Files") %>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal16" runat="server" Text="<%$ Loc:Legend.UserExecutableFileExtList %>" />:
				</td>
				<td width="60%">
					<asp:TextBox ID="UserExecutableFileExtList" runat="server" ValidationGroup="vgInnerForm"/>
					<asp:RegularExpressionValidator ID="FileExtValidator" runat="server" EnableClientScript="false"
													ValidationGroup="vgInnerForm"  ControlToValidate="UserExecutableFileExtList" 
													ValidationExpression="[0-9a-zA-Z\s,]*" 
													ErrorMessage="<%$ Loc:Message.IncorrectFileExtensions %>" 
													Display="Dynamic">*</asp:RegularExpressionValidator>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal17" runat="server" Text="<%$ Loc:Legend.UserExcludedFromExecutableFileExtList %>" />:
				</td>
				<td width="60%">
					<asp:TextBox ID="UserExcludedFromExecutableFileExtList" runat="server" ValidationGroup="vgInnerForm"/>
					<asp:RegularExpressionValidator ID="FileExtValidator2" runat="server" EnableClientScript="false"
													ValidationGroup="vgInnerForm"  ControlToValidate="UserExcludedFromExecutableFileExtList" 
													ValidationExpression="[0-9a-zA-Z\s,]*" 
													ErrorMessage="<%$ Loc:Message.IncorrectFileExtensions %>" 
													Display="Dynamic">*</asp:RegularExpressionValidator>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"></td>
				<td width="60%">
					<asp:CheckBox ID="cbSaveOriginalFileNames" ValidationGroup="vgInnerForm"  Text="<%$ Loc:Legend.SaveOriginalFileNames %>" runat="server" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"></td>
				<td width="60%">
					<asp:CheckBox ID="cbCorrectFileNames" ValidationGroup="vgInnerForm" Text="<%$ Loc:Legend.ReplaceInvalidCharactersInNamesOfUploadedFiles %>" runat="server" />
				</td>
			</tr>
			<tr class="heading">
				<td colspan="2">
					<asp:Literal ID="Literal1" runat="server" Text="<%$ Loc:Header.Mailer %>" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal2" runat="server" Text="<%$ Loc:Mailer.SmtpHost %>" />:
				</td>
				<td width="60%">
					<asp:TextBox ID="MailerSmtpHost" runat="server" ValidationGroup="vgInnerForm" Width="320px" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal3" runat="server" Text="<%$ Loc:Mailer.SmtpPort %>" />:
				</td>
				<td width="60%">
					<asp:TextBox ID="MailerSmtpPort" runat="server" ValidationGroup="vgInnerForm" Text="25" Width="320px" />
					<asp:RangeValidator ID="MailerSmtpPortValidator" runat="server" ValidationGroup="vgInnerForm" 
						ControlToValidate="MailerSmtpPort" Type="Integer" MinimumValue="1" MaximumValue="65535"
						Display="Dynamic" ErrorMessage="<%$ Loc:Error.InvalidPort %>" >*</asp:RangeValidator>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal4" runat="server" Text="<%$ Loc:Mailer.SmtpUsername %>" />:
				</td>
				<td width="60%">
					<asp:TextBox ID="MailerSmtpUsername" runat="server" autocomplete="off" Width="320px" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal5" runat="server" Text="<%$ Loc:Mailer.SmtpPassword %>" />:
				</td>
				<td width="60%">
					<bx:BXHtmlInputPassword runat="server" ID="MailerSmtpPassword" autocomplete="off" Width="320px" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal13" runat="server" Text="<%$ Loc:Mailer.DefaultEmailFrom %>" />:
				</td>
				<td width="60%">
					<asp:TextBox ID="MailerDefaultEmailFrom" runat="server" Width="320px" /><asp:CustomValidator 
					ID="MailerDefaultEmailFromValidator" runat="server" ValidationGroup="vgInnerForm" ValidateEmptyText="true" 
					ClientValidationFunction="MailerDefaultEmailFromValidator_Validate" ControlToValidate="MailerDefaultEmailFrom"
					OnServerValidate="MailerDefaultEmailFromValidator_Validate" 
					ErrorMessage="<%$ Loc:Error.DefaultEmailFromRequired %>" 
					>*</asp:CustomValidator><asp:RegularExpressionValidator 
					ID="MailerDefaultEmailFromEmailValidator" runat="server" ValidationGroup="vgInnerForm" 
					ControlToValidate="MailerDefaultEmailFrom" 
					ValidationExpression="^(?:[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*|[^<>]*<[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*>)$" 
					Display="Dynamic" ErrorMessage="<%$ Loc:Error.InvalidEmailFormat %>" 
					>*</asp:RegularExpressionValidator>
					<br />
					<small><%= GetMessageRaw("Mailer.DefaultEmailFrom.Comment") %></small>
				</td>
			</tr>	
			<tr valign="top">
				<td class="field-name" width="40%">&nbsp;</td>
				<td width="60%">
					<asp:CheckBox ID="MailerUseSsl" runat="server" Text="<%$ Loc:Mailer.UseSsl %>" />
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
	<bx:BXTabControlTab runat="server" Text="<%$ Loc:TabText.StructureManagement %>" Title="<%$ Loc:Kernel.TopPanel.Settings %>">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr class="heading">
				<td colspan="2">
					<asp:Literal ID="Literal7" runat="server" Text="<%$ Loc:Header.EditorSettings %>" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal10" runat="server" Text="<%$ Loc:EditorSettings.DefaultEncoding %>" />:
				</td>
				<td width="60%">
					<asp:DropDownList ID="DefaultEncoding" runat="server" Width="200px" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal11" runat="server" Text="<%$ Loc:EditorSettings.DefaultPageEditor %>" />:
				</td>
				<td width="60%">
					<asp:DropDownList ID="DefaultPageEditor" runat="server" Width="200px" >
						<asp:ListItem Value="text" Text="<%$ Loc:DefaultPageEditor.Text %>" />
						<asp:ListItem Value="visual" Text="<%$ Loc:DefaultPageEditor.Visual %>" />
					</asp:DropDownList>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal14" runat="server" Text="<%$ Loc:EditorSettings.MainContentPlaceHolder %>" />:
				</td>
				<td width="60%">
					<asp:TextBox ID="MainContentPlaceHolder" runat="server" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal15" runat="server" Text="<%$ Loc:EditorSettings.PageBaseClass %>" />:
				</td>
				<td width="60%">
					<% 
						string value = Request.Form[UniqueID + "$PageBaseClass"] ?? Bitrix.Configuration.BXConfigurationUtility.Options.PageBaseClass;
						bool selected = false;
					%>
					<select name="<%= UniqueID %>$PageBaseClass" onchange="document.getElementById('<%= PageBaseClassCustom.ClientID %>').disabled = (this.value != '');">
						<option value=""><%= GetMessage("PageBaseClass.Custom") %></option>
						<% foreach(ListItem i in GetPageClasses()) { %>
						<option value="<%= Encode(i.Value) %>" <% if (value == i.Value) { selected = true; %>selected="selected"<% } %>><%= Encode(i.Text) %></option>
						<% } %>
					</select>
					<% 
						if (selected)
							PageBaseClassCustom.Attributes["disabled"] = "true";
						else if (!string.IsNullOrEmpty(value))
							PageBaseClassCustom.Text = value;			
					%>
					<asp:TextBox ID="PageBaseClassCustom" runat="server" Width="250px" /><asp:CustomValidator 
					ID="PageBaseClassValidator" runat="server" ValidationGroup="vgInnerForm" ValidateEmptyText="true" 
					ControlToValidate="PageBaseClassCustom"	OnServerValidate="PageBaseClassValidator_Validate">*</asp:CustomValidator><br />
					<small><%= GetMessageRaw("PageBaseClass.Description") %></small>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"></td>
				<td width="60%">
					<asp:CheckBox ID="RenderComponents" ValidationGroup="vgInnerForm" Text="<%$ LocRaw:EditorSettings.RenderComponents %>" runat="server" />
				</td>
			</tr>
			<tr class="heading">
				<td colspan="2">
					<asp:Literal ID="Literal6" runat="server" Text="<%$ Loc:Header.SiteSettings %>" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<asp:Literal ID="Literal9" runat="server" Text="<%$ Loc:SiteSettings.SettingsForSite %>" />:
				</td>
				<td width="60%">
					<asp:DropDownList ID="SettingsForSite" runat="server" Width="200px" />
				</td>
			</tr>
			<tr>
				<td class="field-name" width="40%" valign="top" ><%= GetMessage("SiteSettings.MenuTypes") %>:</td>
				<td width="60%">
					<asp:Repeater ID="MenuSettings" runat="server" OnItemDataBound="MenuSettings_ItemDataBound" >
					<ItemTemplate>
							<asp:HiddenField ID="MenuTypesCount" runat="server" Value="0" />
							<asp:Table ID="MenuTypes" runat="server" BorderWidth="0px" BorderStyle="None" CellPadding="0" CellSpacing="2" Width="100%" CssClass="edit-table">
							<asp:TableHeaderRow CssClass="heading" runat="server">
								<asp:TableCell Text="<%$ Loc:SiteSettings.MenuType %>" runat="server" />
								<asp:TableCell Text="<%$ Loc:SiteSettings.MenuTitle %>" runat="server" />
							</asp:TableHeaderRow>
							</asp:Table>
					</ItemTemplate>
					</asp:Repeater>
				</td>
			</tr>
			<tr>
				<td class="field-name" width="40%" valign="top" ><%= GetMessage("SiteSettings.SiteMap") %>:</td>
				<td width="60%">
					<asp:Repeater ID="SiteMapSettings" runat="server" OnItemDataBound="SiteMapSettings_ItemDataBound" >
					<ItemTemplate>
						<asp:TextBox ID="SiteMapMenuTypes" runat="server" Width="300px" />
					</ItemTemplate>
					</asp:Repeater>
					<br /><small><%= GetMessageRaw("SiteSettings.SiteMapDescription") %></small>
				</td>
			</tr>
			<tr>
				<td class="field-name" width="40%" valign="top" ><%= GetMessage("SiteSettings.Properties") %>:</td>
				<td width="60%">
					<asp:Repeater ID="PropertiesSettings" runat="server" OnItemDataBound="PropertiesSettings_ItemDataBound" >
					<ItemTemplate>
							<asp:HiddenField ID="PropertiesCount" runat="server" Value="0" />
							<asp:Table ID="Properties" runat="server" BorderWidth="0px" BorderStyle="None" CellPadding="0" CellSpacing="2" Width="100%" CssClass="edit-table">
							<asp:TableHeaderRow CssClass="heading" runat="server">
								<asp:TableCell Text="<%$ Loc:SiteSettings.PropertyType %>" runat="server" />
								<asp:TableCell Text="<%$ Loc:SiteSettings.PropertyTitle %>" runat="server" />
							</asp:TableHeaderRow>
							</asp:Table>
					</ItemTemplate>
					</asp:Repeater>
				</td>
			</tr>
			<tr>
				<td class="field-name" width="40%" valign="top" ><%= GetMessage("SiteSettings.LoginUrl") %>:</td>
				<td width="60%">
					<asp:Repeater ID="LoginUrlSettings" runat="server" OnItemDataBound="LoginUrlSettings_ItemDataBound" >
					<ItemTemplate>
						<asp:TextBox ID="LoginUrl" runat="server" Width="300px" />
					</ItemTemplate>
					</asp:Repeater>
					<br /><small><%= GetMessage("SiteSettings.LoginUrlDescription") %></small>
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
	<bx:BXTabControlTab runat="server" Text="<%$ LocRaw:TabText.Users %>" Title="<%$ LocRaw:TabTitle.Users %>" >
	
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr valign="top">
				<td class="field-name" width="40%">
					&nbsp;
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="CrossDomainAuthentication" Text="<%$ LocRaw:CheckBoxText.SpreadAuthentication %>" />
				</td>
			</tr>
		
		
			<tr class="heading">
				<td colspan="2"><%= GetMessage("Heading.UserRegistrationSettings") %></td>
			</tr>
			
			
			
				
			<tr valign="top">
				<td class="field-name" width="40%">
					&nbsp;
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="SendConfirmationRequest" Text="<%$ LocRaw: Label.AskForConfirmation %>" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Label.IntervalToStoreLockedUsers") %>:
				</td>
				<td width="60%">
					<asp:TextBox runat="server" ID="IntervalToStoreUnconfirmedUsers" />
				</td>
			</tr>
			
			
			
			
			<tr class="heading">
				<td colspan="2"><%= GetMessage("Heading.OpenIdLiveId") %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Label.EnableOpenId") %>:
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="EnableOpenId" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Label.OpenIdCustomPropertyCode") %>:
				</td>
				<td width="60%">
					<asp:TextBox runat="server" ID="OpenIdCustomPropertyCode" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Label.EnableLiveId") %>:
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="EnableLiveId" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Label.LiveIdCustomPropertyCode") %>:
				</td>
				<td width="60%">
					<asp:TextBox runat="server" ID="LiveIdCustomPropertyCode" />
				</td>
			</tr>
			 <tr valign="top">
				<td class="field-name">
					<%= GetMessage("LiveIDApplicationID") + ":"%>
				</td>
				<td>
					<asp:TextBox runat="server" ID="LiveIDApplicationID" Columns="30"></asp:TextBox>
					<a href="http://manage.dev.live.com" target="_blank"><%=GetMessage("GetLiveIDAppIdAndKey")%></a>
				</td>
				
			</tr>
			<tr valign="top">
				<td class="field-name">
					<%= GetMessage("LiveIDSecretKey") + ":"%>
				</td>
				<td>
					<asp:TextBox runat="server" ID="LiveIDSecretKey" Columns="30"></asp:TextBox>
				</td>
			</tr>
		</table>
	
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr class="heading">
				<td colspan="2"><%= GetMessage("Heading.UserImage") %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Label.MaxImageSize") %>:
				</td>
				<td width="60%">
					<asp:TextBox runat="server" ID="AvatarMaxSizeKB" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Label.MaxImageWidth") %>:
				</td>
				<td width="60%">
					<asp:TextBox runat="server" ID="AvatarMaxWidth" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Label.MaxImageHeight") %>:
				</td>
				<td width="60%">
					<asp:TextBox runat="server" ID="AvatarMaxHeight" />
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
	<bx:BXTabControlTab runat="server" Text="<%$ LocRaw:TabText.Text %>" Title="<%$ LocRaw:TabTitle.Title %>">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr class="heading">
				<td colspan="2"><%= GetMessageRaw("Kernel.Information") %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessage("Legend.Scheduler") %>:</td>
				<td width="60%"><b><%= Bitrix.Services.BXScheduler.IsActive ? GetMessage("Scheduler.Started") : GetMessage("Scheduler.Stopped") %></b></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessage("Legend.Mailer") %>:</td>
				<td width="60%"><b><%= Bitrix.Services.BXMailer.IsActive ? GetMessage("Mailer.Started") : GetMessage("Mailer.Stopped") %></b></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessageRaw("Legend.MachineName") %>:</td>
				<td width="60%"><%= Encode(Environment.MachineName) %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessageRaw("Legend.SiteName") %>:</td>
				<td width="60%"><%= Encode(System.Web.Hosting.HostingEnvironment.SiteName) %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessageRaw("Legend.ApplicationVirtualPath") %>:</td>
				<td width="60%"><%= Encode(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath) %></td>
			</tr>
			<% 
			try 
			{
				var appId = System.Web.Hosting.HostingEnvironment.ApplicationID;
			%>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessageRaw("Legend.ApplicationID") %>:</td>
				<td width="60%"><%= Encode(appId) %></td>				
			</tr>						
			<% 
			} 
			catch 
			{
			} 
			%>				
			<tr valign="top">
				<td class="field-name" width="40%">
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="EnableAnonymousDebug" Text="<%$ Loc:CheckBoxText.EnableAnonymousDebug %>" />
				</td>
			</tr>
			<tr class="heading">
				<td colspan="2"><%= GetMessageRaw("Header.UrlProcessing") %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="HandleError404" Text="<%$ Loc:CheckBoxText.Enable404Handling %>" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="HandleStandardSefExceptions" Text="<%$ Loc:CheckBoxText.HandleStandardSefExceptions %>" />
				</td>
			</tr>
			
			<tr class="heading">
				<td colspan="2"><%= GetMessageRaw("Header.Multisiting") %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessageRaw("StandardExceptions") %>:
				</td>
				<td width="60%">
					<%= string.Join("<br/>", Array.ConvertAll(Bitrix.Services.BXSiteRemapUtility.GetStandardRemapExceptions(), x => Encode(x))) %>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessageRaw("CustomExceptions") %>:
				</td>
				<td width="60%">
					<asp:TextBox runat="server" ID="MultisitingExceptions" TextMode="MultiLine" Rows="4" /><br />
					<small><%= GetMessageRaw("CustomExceptionsDescription") %></small>
				</td>
			</tr>

			<% if (Providers.Count > 0) { %>
			<tr class="heading">
				<td colspan="2"><%= GetMessageRaw("Header.CacheProvider") %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessageRaw("CacheProvider") %>:
				</td>
				<td width="60%">
					<script type="text/javascript">
					var <%= ClientID %>_CacheProvidersMapping = [];
					function <%= ClientID %>_CacheProvidersChange(value)
					{
						var items = <%= ClientID %>_CacheProvidersMapping;
						for (var i = 0; i < items.length; i++)
						{
							document.getElementById(items[i].editor).style.display = (items[i].id == value) ? '' : 'none';
						} 
					}
					</script>
					<select name="<%= UniqueID %>$cacheProvider" onchange="<%= ClientID %>_CacheProvidersChange(value)">
						<option><%= GetMessageRaw("Option.DontUseCache") %></option>
						<% foreach (var p in Providers) { %>
						<option value="<%= Encode(p.Provider.Id) %>" <% if (p == CurrentProvider) { %>selected="selected"<% } %> ><%= Encode(p.Provider.Title) %></option>
						<% } %>
					</select>
					<% if (false) { %>
					<asp:PlaceHolder ID="CacheProviderEditors" runat="server" />
					<% } %>
				</td>
			</tr>			
			<tr valign="top">
			<td colspan="2">
				<% int i = 0; %>
				<% foreach (var p in Providers) { %>
				<div id="<%= ClientID %>_CacheProviderConfig_<%= i %>" <% if (p != CurrentProvider) { %>style="display:none"<% } %>>
				
				<% if (p.Editor != null) { %>
				<% CacheProviderEditorRenderer.SetRenderMethodDelegate((HtmlTextWriter output, Control container) => { p.Editor.RenderControl(output); });  %>
				<asp:PlaceHolder ID="CacheProviderEditorRenderer" runat="server" />
				
				<% } %>

				</div>
				
				<script type="text/javascript">
				<%= ClientID %>_CacheProvidersMapping.push({ id: "<%= JSEncode(p.Provider.Id) %>", editor: "<%= ClientID %>_CacheProviderConfig_<%= i %>"});
				</script>

				<% i++; %>
				<% } %>
			</td>
			</tr>
			<% } %>

			<% if (SessionConfigurators.Count > 0) { %>
			<tr class="heading">
				<td colspan="2"><%= GetMessageRaw("Header.Session") %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessageRaw("SessionConfigurator") %>:
				</td>
				<td width="60%">
					<script type="text/javascript">
					var <%= ClientID %>_SessionConfiguratorsMapping = [];
					function <%= ClientID %>_SessionConfiguratorsChange(value)
					{
						var items = <%= ClientID %>_SessionConfiguratorsMapping;
						for (var i = 0; i < items.length; i++)
						{
							document.getElementById(items[i].editor).style.display = (items[i].id == value) ? '' : 'none';
						}
					}
					</script>
					<select name="<%= UniqueID %>$sessionConfigurator" onchange="<%= ClientID %>_SessionConfiguratorsChange(value)">
						<option><%= GetMessageRaw("Option.SelectConfigurator") %></option>			
						<% foreach (var p in SessionConfigurators) { %>
						<option value="<%= Encode(p.Configurator.Id) %>" <% if (p == CurrentSessionConfigurator) { %>selected="selected"<% } %> ><%= Encode(p.Configurator.Title) %></option>
						<% } %>			
					</select>
					<bx:BXAdminNote runat="server" ID="SessionWarning">
						<%= GetMessageRaw("SessionConfiguratorNote") %>
					</bx:BXAdminNote>
					<% if (false) { %>
					<asp:PlaceHolder ID="SessionConfiguratorsHolder" runat="server" />
					<% } %>
				</td>
			</tr>	
			<tr valign="top">
			<td colspan="2">
				<% int i = 0; %>
				<% foreach (var p in SessionConfigurators) { %>
				<div id="<%= ClientID %>_SessionConfigurator_<%= i %>" <% if (p != CurrentSessionConfigurator) { %>style="display:none"<% } %>>
				
				<% if (p.Editor != null) { %>
				<% SessionConfiguratorRenderer.SetRenderMethodDelegate((HtmlTextWriter output, Control container) => { p.Editor.RenderControl(output); });  %>
				<asp:PlaceHolder ID="SessionConfiguratorRenderer" runat="server" />				
				<% } %>

				</div>
				
				<script type="text/javascript">
				<%= ClientID %>_SessionConfiguratorsMapping.push({ id: "<%= JSEncode(p.Configurator.Id) %>", editor: "<%= ClientID %>_SessionConfigurator_<%= i %>"});
				</script>

				<% i++; %>
				<% } %>
			</td>
			</tr>
			<% } %>
		</table>	
	</bx:BXTabControlTab>
</bx:BXTabControl>
