<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="SiteEdit.aspx.cs" Inherits="bitrix_admin_SiteEdit" Title="#" Trace="false" %>    

<%@ Register Src="~/bitrix/admin/controls/Main/CultureDropDownList.ascx" TagName="CultureDropDownList" TagPrefix="bx" %>

<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <asp:UpdatePanel ID="LangUpdatePanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
			<% CopySiteButton.Href = String.Concat("SiteEdit.aspx?copy=", SiteId); %>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
                <Items>
                    <bx:BXCmSeparator runat="server" SectionSeparator="true" />
                    <bx:BXCmImageButton runat="server" CssClass="context-button icon btn_list" CommandName="getlist"
                        Text="<%$ Loc:List %>" Href="SiteAdmin.aspx" Title="<%$ Loc:ListDesc %>"
                     />
                    <bx:BXCmSeparator ID="AddSiteSeparator" />
                    <bx:BXCmImageButton ID="AddSiteButton" CssClass="context-button icon btn_new" CommandName="new"
                        Text="<%$ Loc:Add %>" Href="SiteEdit.aspx" Title="<%$ Loc:AddDesc %>"
                    />
                    <bx:BXCmSeparator ID="CopySiteSeparator" />
                    <bx:BXCmImageButton ID="CopySiteButton" CssClass="context-button icon btn_copy" CommandName="copy"
                        Text="<%$ Loc:Copy %>" Title="<%$ Loc:CopyDesc %>"
                    />
                    <bx:BXCmSeparator ID="DeleteSiteSeparator" />
                    <bx:BXCmImageButton ID="DeleteSiteButton" CssClass="context-button icon btn_delete" CommandName="delete"
                        Text="<%$ Loc:Delete %>" Title="<%$ Loc:DeleteDesc %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"
                         />
                </Items>
            </bx:BXContextMenuToolbar>
            <asp:HiddenField ID="EditId" runat="server" EnableViewState="false" />
            <bx:BXMessage ID="resultMessage" runat="server" CssClass="Ok" IconClass="Ok" Visible="False" />
            <bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
            <bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
                <bx:BXTabControlTab runat="server" Selected="True" ID="edittab0" Title="<%$ Loc:EditTitle %>"
                    Text="<%$ Loc:Parameters %>" >
                    <table id="Table1" class="edit-table" cellspacing="0" cellpadding="0" border="0">
                        <tbody>
                            <tr valign="top">
                                <td style="width: 40%" class="field-name">
                                    <span class="required">*</span><%= GetMessage("ID") + ":" %>
                                </td>
                                <td style="width: 60%">
                                    <asp:Label runat="server" Text="ru" ID="lbID" Visible="False"></asp:Label>
                                    <asp:TextBox runat="server" ID="txtID" MaxLength="50" Columns="30"></asp:TextBox>
                                    <span runat="server" id="starID" class="required" visible="false" >*</span>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessage("Active") + ":" %>
                                </td>
                                <td>
                                    <asp:CheckBox runat="server" ID="Active" Checked="True"></asp:CheckBox>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <span class="required">*</span><%= GetMessage("Name") + ":" %>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="Name" Columns="30"></asp:TextBox>
                                    <span runat="server" id="starName" class="required" visible="false" >*</span>
                                </td>
                            </tr>
                            <tr class="heading">
                                <td colspan="2">
                                    <asp:Literal runat="server" ID="Literal1" EnableViewState="False" Text="<%$ Loc:ParamsPublic %>" />
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <%  if (!string.IsNullOrEmpty(SiteId) && Bitrix.Modules.BXModuleManager.IsModuleInstalled("search")) { %><a href="#remark1" style="vertical-align:super; text-decoration:none"><span class="required">1</span></a><% } %><span class="required">*</span><%= GetMessage("Language") + ":" %>
                                </td>
                                <td>
                                    <asp:DropDownList runat="server" ID="Language">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessage("Default") + ":" %>
                                </td>
                                <td>
                                    <asp:CheckBox runat="server" ID="Default" Columns="30"></asp:CheckBox>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessage("Domains") + ":" %>
                                    <br />
                                    <%= GetMessage("Domains2") %>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="Domains" Rows="3" TextMode="MultiLine" Columns="23"></asp:TextBox>
                                </td>
                            </tr>
                            
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessageRaw("RemapFolder") %>:
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="RemapFolder" Columns="30"></asp:TextBox>
                                </td>
                            </tr>
                            
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessage("SiteFolder") + ":" %>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="SiteFolder" Columns="30"></asp:TextBox>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <span class="required">*</span><%= GetMessage("Sort") + ":" %>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="Sort" Columns="30">100</asp:TextBox>
                                    <span runat="server" id="starSort" class="required" visible="false" >*</span>                                                                   
                                </td>
                            </tr>
                            <tr class="heading">
                                <td colspan="2">
                                    <asp:Literal runat="server" ID="Literal2" EnableViewState="False" Text="<%$ Loc:Params %>" />
                                </td>
                            </tr>
							<tr>
								<td class="field-name">
									<%= GetMessage("Culture") %>
								</td>
								<td>
									<bx:CultureDropDownList ID="Culture" runat="server" />
								</td>
							</tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <span class="required">*</span><%= GetMessage("SiteName") + ":" %>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="SiteName" Columns="30"></asp:TextBox>
                                    <span runat="server" id="starSiteName" class="required" visible="false" >*</span>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessage("ServerUrl") + ":" %>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="ServerUrl" Columns="30"></asp:TextBox>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessage("Email") + ":" %>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="Email" Columns="30"></asp:TextBox>
                                </td>
                            </tr>
                            <tr class="heading">
                                <td colspan="2">
                                    <asp:Literal runat="server" ID="Literal10" EnableViewState="False" Text="<%$ Loc:Templates %>" />:
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <table cellspacing="0" class="edit-table" width="100%">
                                        <tr>
                                            <td>
                                                <table cellspacing="0" class="internal" width="100%" runat="server" id="ConditionTable">
                                                    <tr class="heading" runat="server">
                                                        <td runat="server">
                                                            <span class="required">*</span><asp:Literal runat="server" ID="Literal11" EnableViewState="False"
                                                                Text="<%$ Loc:Templates %>" />
                                                        </td>
                                                        <td runat="server">
                                                            <asp:Literal runat="server" ID="Literal12" EnableViewState="False" Text="<%$ Loc:SortShort %>" />
                                                        </td>
                                                        <td runat="server">
                                                            <asp:Literal runat="server" ID="Literal13" EnableViewState="False" Text="<%$ Loc:ConditionType %>" />
                                                        </td>
                                                        <td width="100%" runat="server">
                                                            <asp:Literal runat="server" ID="Literal14" EnableViewState="False" Text="<%$ Loc:Condition %>" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </bx:BXTabControlTab>
            </bx:BXTabControl> 
            <% if (!string.IsNullOrEmpty(SiteId) && Bitrix.Modules.BXModuleManager.IsModuleInstalled("search")) { %>
            <bx:BXAdminNote ID="BXAdminNote1" runat="server">
                <span class="required" style="vertical-align:super" id="remark1" >1</span><%= string.Format(GetMessageRaw("Remark.ReindexRequired"), "SearchReindex.aspx") %>
            </bx:BXAdminNote>
            <% } %>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
