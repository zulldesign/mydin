<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" 
    CodeFile="IBlockImportXML.aspx.cs" Inherits="bitrix_admin_IBlockImportXML" Title="<%$ LocRaw:PageTitle %>" Trace="false"%>

<%@ Register Src="~/bitrix/controls/Main/DirectoryBrowser.ascx" TagName="DirectoryBrowser" TagPrefix="bx" %>

<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<%@ Import Namespace="Bitrix.Main" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <ContentTemplate>
        <asp:Panel ID="UpdateProgress" runat="server" Visible="false">
            <ContentTemplate>
                <div>
                    <img src="../../bitrix/images/update_progressbar.gif" alt="<%= GetMessage("PleaseWaitForCompletionOfOperation") %>" />
                </div>
            </ContentTemplate>
        </asp:Panel>
        <asp:Label id="ProgressLabel" runat="Server"></asp:Label>

        <bx:InlineScript runat="server" ID="Script" AsyncMode="ScriptBlock" >
            <script type="text/javascript">
                function OnFileSelect()
                {
                    <%= selectDirDialog.JavaScriptObject %>.ExpandPath('<%= BXJSUtility.Encode(SourceFile.Text) %>');
                    <%= selectDirDialog.ContainerDialog.GetJSObjectName() %>.ShowPopupDialog(false, true);
                    return false;
                }
                function OnFileOk()
                {
                    document.getElementById('<%= SourceFile.ClientID %>').value = <%= selectDirDialog.JavaScriptObject %>.GetTargetValue();
                    <%= selectDirDialog.ContainerDialog.GetJSObjectName() %>.ClosePopupDialog();
                    return false;
                }
                function OnFileCancel()
                {
                    <%= selectDirDialog.ContainerDialog.GetJSObjectName() %>.ClosePopupDialog();
                    return false;
                }
            </script>
        </bx:InlineScript>

        <bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
        <bx:BXTabControl ID="BXTabControl1" runat="server" ButtonsMode="Hidden">
            <bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True"
            Title="<%$ Loc:EditTitle %>" Text="<%$ Loc:Parameters %>"> 
                <table cellpadding="0" cellspacing="0" border="0" class="edit-table" id="fedit1_edit_table" >
                <tbody>
                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Param.SourceFile")%>:</td>
                        <td width="60%">
                            <asp:TextBox width="300" Id="SourceFile" runat="Server"></asp:TextBox>
                            <asp:Button ID="SelectDirButton" runat="server" Text="..." UseSubmitBehavior="False" OnClientClick="return OnFileSelect();" />
                        </td>
                    </tr>

                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Param.InfoBlockType")%>:</td>
                        <td width="60%">
                            <asp:DropDownList width="300" ID="TypeCB" runat="server"></asp:DropDownList> 
                        </td>
                    </tr>

                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Param.Sites")%>:</td>
                        <td width="60%">
                            <asp:ListBox Width="300" Id="SitesList" SelectionMode="Multiple" Runat="server"></asp:ListBox> 
                        </td>
                    </tr>

                    <tr>
                        <td valign="top" width="40%" class="field-name"><%= GetMessage("Param.ElementAction")%>:</td>
                        <td width="60%">
                            <asp:DropDownList Width="140" Id="ElementActions" runat="server">
                                <asp:ListItem Text="<%$ LocRaw:Option.NoAction %>" Value="None"/>
                                <asp:ListItem Text="<%$ LocRaw:Option.Deactivate %>" Value="Deactivate"/>
                                <asp:ListItem Text="<%$ LocRaw:Option.Delete %>" Value="Delete"/>
                            </asp:DropDownList>
                        </td>
                    </tr>

                    <tr>
                        <td valign="top" width="40%" class="field-name"><%= GetMessage("Param.SectionAction")%>:</td>
                        <td width="60%">
                            <asp:DropDownList Width="140" Id="SectionActions" runat="server">
                                <asp:ListItem Text="<%$ LocRaw:Option.NoAction %>" Value="None"/>
                                <asp:ListItem Text="<%$ LocRaw:Option.Deactivate %>" Value="Deactivate"/>
                                <asp:ListItem Text="<%$ LocRaw:Option.Delete %>" Value="Delete"/>
                            </asp:DropDownList>
                        </td>
                    </tr>

                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Param.EnableCreationOfPreviewImage")%>:</td>
                        <td width="60%">
                            <asp:CheckBox id="CreatePreviewImage" runat="server"></asp:CheckBox>
                        </td>
                    </tr>

                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Param.PreviewImageMaxWidth")%>:</td>
                        <td width="60%">
                            <asp:TextBox id="PreviewImageWidth" Width="140" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    
                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Param.PreviewImageMaxHeight")%>:</td>
                        <td width="60%">
                            <asp:TextBox id="PreviewImageHeight" Width="140" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    
                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Param.EnableDetailImageResize")%>:</td>
                        <td width="60%">
                            <asp:CheckBox id="ResizeDetailImage" runat="server"></asp:CheckBox>
                        </td>
                    </tr>

                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Param.DetailImageMaxWidth")%>:</td>
                        <td width="60%">
                            <asp:TextBox id="DetailImageWidth" Width="140" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    
                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Param.DetailImageMaxHeight")%>:</td>
                        <td width="60%">
                            <asp:TextBox id="DetailImageHeight" Width="140" runat="server"></asp:TextBox>
                        </td>
                    </tr>                    
                    
                </tbody>
                </table>

                <div class="buttons">
                    <asp:Button runat="server" id="StartButton" Text="<%$ LocRaw:Button.StartImport%>" OnClick="OnStartImport"/>
                    <asp:Button runat="server" id="StopButton" Text="<%$ LocRaw:Button.StopImport%>" OnClick="OnStopImport"/>
                </div>

        </bx:BXTabControlTab>
        </bx:BXTabControl>

        <bx:DirectoryBrowser ID="selectDirDialog" runat="server" ShowFiles="true" ItemsToSelect="Files" EnableExtras="false" ShowDescription="true"
            WindowTitle="<%$ Loc:Param.SourceFile %>" 
            OKClientScript="return OnFileOk();"
            CancelClientScript="return OnFileCancel();"
        />

    </ContentTemplate>
</asp:Content>

