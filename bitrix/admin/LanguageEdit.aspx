<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="LanguageEdit.aspx.cs" Inherits="bitrix_admin_LanguageEdit" Title="#"
    Trace="false" %>

<%@ Register Src="~/bitrix/admin/controls/Main/CultureDropDownList.ascx" TagName="CultureDropDownList" TagPrefix="bx" %>

<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <asp:UpdatePanel ID="LangUpdatePanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
			<% CopyAction.Href = String.Concat("LanguageEdit.aspx?copy=", LanguageId); %>
            <bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
                <Items>
                    <bx:BXCmSeparator SectionSeparator="true" />
                    <bx:BXCmImageButton ID="BXCmImageButton1" runat="server" CssClass="context-button icon btn_list" CommandName="getlist"
                        Text="<%$ Loc:List %>" Href="Language.aspx" Title="<%$ Loc:ListDesc %>"
                        />
                    <bx:BXCmSeparator ID="AddSeparator" />
                    <bx:BXCmImageButton ID="AddAction" CssClass="context-button icon btn_new" CommandName="new"
                        Text="<%$ Loc:Add %>" Href="LanguageEdit.aspx" Title="<%$ Loc:AddDesc %>"
                     />
                    <bx:BXCmSeparator ID="CopySeparator" />
                    <bx:BXCmImageButton ID="CopyAction" runat="server" CssClass="context-button icon btn_copy" CommandName="copy"
                        Text="<%$ Loc:Copy %>" Title="<%$ Loc:CopyDesc %>"
                        />
                    <bx:BXCmSeparator ID="DeleteSeparator" />
                    <bx:BXCmImageButton ID="DeleteAction" runat="server" CssClass="context-button icon btn_delete" CommandName="delete"
                        Text="<%$ Loc:Delete %>" Title="<%$ Loc:DeleteDesc %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"
                     />
                </Items>
            </bx:BXContextMenuToolbar>
            <bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
            <bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
                <bx:BXTabControlTab runat="server" Selected="True" ID="edittab0" Title="<%$ Loc:EditTitle %>"
                    Text="<%$ Loc:Parameters %>">
                    <table id="Table1" class="edit-table" cellspacing="0" cellpadding="0" border="0">
                        <tbody>
                            <tr valign="top">
                                <td style="width: 40%" class="field-name">
                                    <span class="required">*</span><%= GetMessage("EdID") + ":" %></td>
                                <td style="width: 60%">
                                    <asp:Label runat="server" Text="ru" ID="lbID" Visible="False"></asp:Label>
                                    <asp:TextBox runat="server" ID="txtID"></asp:TextBox>
                                    <span runat="server" id="starId" class="required" visible="false">*</span>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessage("EdActive") + ":" %>
                                </td>
                                <td>
                                    <asp:CheckBox runat="server" ID="Active" Checked="True"></asp:CheckBox>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <span class="required">*</span><%= GetMessage("EdName") + ":" %>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="Name"></asp:TextBox>
                                    <span runat="server" id="starName" class="required" visible="false">*</span>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessage("EdDefault") + ":" %></td>
                                <td>
                                    <asp:CheckBox runat="server" ID="Default"></asp:CheckBox>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <span class="required">*</span><%= GetMessage("EdSort") + ":" %>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="Sort"></asp:TextBox>
                                    <span runat="server" id="starSort" class="required" visible="false">*</span>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <%= GetMessage("Legend.Culture") %>
                                </td>
                                <td>
									<bx:CultureDropDownList ID="Culture" runat="server"/>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </bx:BXTabControlTab>
            </bx:BXTabControl>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

