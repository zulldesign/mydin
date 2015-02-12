<%--<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="TemplateEdit.aspx.cs" Inherits="bitrix_admin_TemplateEdit" Title="<%$ Loc:PageTitle %>"
    Trace="false" StylesheetTheme="AdminTheme" ValidateRequest="false" %> zg--%>

<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="TemplateEdit.aspx.cs" Inherits="bitrix_admin_TemplateEdit" Title="<%$ Loc:PageTitle %>"
    Trace="false" ValidateRequest="false" %>


<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <%--<asp:UpdatePanel ID="UpdatePanel1" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>--%>
            <bx:BXContextMenuToolbar ID="ContextMenuToolbar" runat="server" OnCommandClick="ContextMenuToolbar_CommandClick">
                <Items>
                    <bx:BXCmSeparator runat="server" SectionSeparator="true" />
                    <bx:BXCmImageButton runat="server" CssClass="context-button icon btn_list" Href="Template.aspx"
                        Text="<%$ Loc:Toolbar.TemplateList %>" Title="<%$ Loc:Toolbar.TemplateListDesc %>"
                      />
                    <bx:BXCmSeparator ID="AddTemplateSeparator" />
                    <bx:BXCmImageButton ID="AddTemplateButton" CssClass="context-button icon btn_new" Href="TemplateEdit.aspx"
                        Text="<%$ Loc:Toolbar.Add %>" Title="<%$ Loc:Toolbar.AddDesc %>" />
                    <bx:BXCmSeparator ID="CopyTemplateSeparator" />
                    <bx:BXCmImageButton ID="CopyTemplateButton" CssClass="context-button icon btn_copy" CommandName="copy"
                        Text="<%$ Loc:Toolbar.Copy %>" Title="<%$ Loc:Toolbar.CopyDesc %>"  />
                    <bx:BXCmSeparator ID="DeleteTemplateSeparator" />
                    <bx:BXCmImageButton ID="DeleteTemplateButton" CssClass="context-button icon btn_delete" CommandName="delete"
                        Text="<%$ Loc:Toolbar.Delete %>" Title="<%$ Loc:Toolbar.DeleteDesc %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" />
                </Items>
            </bx:BXContextMenuToolbar>
            <bx:BXMessage ID="ErrorMessage" Title="<%$ Loc:Kernel.Error %>" runat="server" CssClass="Error"
                IconClass="Error" Visible="false" />
            <bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
                <bx:BXTabControlTab runat="server" Selected="True" ID="Template" Title="<%$ Loc:Tabs.Template.Title %>"
                    Text="<%$ Loc:Tabs.Template %>">
                    <table id="Table1" class="edit-table" cellspacing="0" cellpadding="0" border="0">
                        <tbody>
                            <tr valign="top">
                                <td style="width: 40%" class="field-name">
									<span class="required">*</span>
                                    <asp:Literal runat="server" ID="LabelLU" EnableViewState="False" Text="<%$ Loc:ID %>"></asp:Literal>:
                                </td>
                                <td width="60%">
                                    <asp:Label runat="server" ID="lbID" Text="book"></asp:Label>
                                    <asp:Label runat="server" ID="Left" Text="("></asp:Label><asp:HyperLink runat="server"
                                        ID="hlPath"></asp:HyperLink><asp:Label runat="server" ID="Right" Text=")"></asp:Label>
                                    <asp:TextBox runat="server" ID="txtID" Columns="40"></asp:TextBox></td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
									<asp:Literal runat="server" ID="Label2" EnableViewState="False" Text="<%$ Loc:Name %>"></asp:Literal>:
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="Name" Columns="40"></asp:TextBox>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td class="field-name">
                                    <asp:Literal runat="server" ID="Literal3" EnableViewState="False" Text="<%$ Loc:Description %>"></asp:Literal>:
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="Description" Rows="3" TextMode="MultiLine" Columns="30"></asp:TextBox>
                                </td>
                            </tr>
                            <tr class="heading">
                                <td colspan="2">
                                    <span class="required">*</span><asp:Literal runat="server" ID="Literal1" EnableViewState="False"
                                        Text="<%$ Loc:Message %>"></asp:Literal>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td align="center" colspan="2">
									<bx:BXWebEditor ID="TemplateEditor" runat="server" 
										Width="100%" 
										Height="500px"
										StartModeSelector="True"
										OnlyOnOfSelector="True"
										StartMode="HTMLVisual"
										UseHTMLEditor="False" 
										AutoLoadContent="True"
										ContentType="Text"
										FullScreen="False"
										LimitCodeAccess="False"
										TemplateId=""
										UseOnlyDefinedStyles="False"
										OnIncludeWebEditorScript="TemplateEditor_IncludeWebEditorScript"
										WithoutCode="False">
										<Taskbars>
											<bx:BXWebEditorBar Name="BXPropertiesTaskbar" />
											<bx:BXWebEditorBar Name="ASPXComponentsTaskbar" />
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
									<% /*
                                    	
                                    <asp:TextBox runat="server" Width="100%" Height="550px" ID="txtTemplate" TextMode="MultiLine"></asp:TextBox>			
									 */ %>
                                </td>
                            </tr>
                            <tr valign="top">
                                <td align="left" colspan="2">
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </bx:BXTabControlTab>
                <bx:BXTabControlTab runat="server" ID="SiteStyle"
                    Text="<%$ Loc:Tabs.SiteStyle %>">
                    <asp:TextBox runat="server" Width="100%" Height="550px" ID="txtSiteStyle" TextMode="MultiLine"></asp:TextBox>
                </bx:BXTabControlTab>
                <bx:BXTabControlTab runat="server" ID="TemplateStyle"
                    Text="<%$ Loc:Tabs.TemplateStyle %>">
                    <asp:TextBox runat="server" Width="100%" Height="550px" ID="txtTemplateStyle" TextMode="MultiLine"></asp:TextBox>
                </bx:BXTabControlTab>
                <bx:BXTabControlTab runat="server" ID="Files" Title="<%$ Loc:Tabs.Files.Title %>"
                    Text="<%$ Loc:Tabs.Files %>">
                    <table cellspacing="0" class="edit-table">
                        <tbody>
                            <tr>
                                <td>
                                    <table cellspacing="0" class="internal">
                                        <tbody>
                                            <asp:Repeater runat="server" ID="repFiles">
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <%# Eval("Name") %>
                                                        </td>
                                                        <td />
                                                        <td>
                                                            <a href='FileManView.aspx?path=<%# Eval("FullName").ToString().Replace(MapPath("~/"),"").Replace("\\","/") %>'
                                                                title='<%= GetMessage("EditFile") %> <%# Eval("Name") %>'><%= GetMessage("Edit") %></a></td>
                                                    </tr>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </bx:BXTabControlTab>
            </bx:BXTabControl>
<%--        </ContentTemplate>
    </asp:UpdatePanel>--%>
</asp:Content>
