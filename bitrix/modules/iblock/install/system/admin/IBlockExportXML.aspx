<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" 
    CodeFile="IBlockExportXML.aspx.cs" Inherits="bitrix_admin_IBlockExportXML" Title="<%$ LocRaw:PageTitle %>" Trace="false"%>

<%@ Register Src="~/bitrix/controls/Main/DirectoryBrowser.ascx" TagName="DirectoryBrowser" TagPrefix="bx" %>

<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.IO" %>
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

        <script type="text/javascript">

            setTimeout("CallServer()", 1000);

            function CallServer()
            {
                <%= ClientScript.GetCallbackEventReference(this, "", "ShowResult", null) %>;
            }

            function ShowResult(arg, context)
            {
                if(arg != ".")
                {
                    setTimeout("CallServer()", 1000);
                    document.getElementById('<%= ProgressLabel.ClientID %>').innerHTML = arg;
                }
                else
                {
                    document.getElementById('<%= UpdateProgress.ClientID %>').style.display = 'none';
                    document.getElementById('<%= StartButton.ClientID %>').disabled = false;
                    document.getElementById('<%= StopButton.ClientID %>').disabled = true;
                }
            }
        </script>

        <bx:InlineScript runat="server" ID="Script" AsyncMode="ScriptBlock" >
        <script type="text/javascript">
            function OnFileSelect()
            {
                <%= selectDirDialog.JavaScriptObject %>.ExpandPath('<%= BXJSUtility.Encode(DestinationFile.Text) %>');
                <%= selectDirDialog.ContainerDialog.GetJSObjectName() %>.ShowPopupDialog(false, true);
                return false;
            }
            function OnFileOk()
            {
                document.getElementById('<%= DestinationFile.ClientID %>').value = <%= selectDirDialog.JavaScriptObject %>.GetTargetValue();
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
                        <td width="40%" class="field-name"><%= GetMessage("InformationBlock") %>:</td>
                        <td>
                            <asp:DropDownList ID="TypeCB" runat="server" AutoPostBack="True" OnSelectedIndexChanged="OnTypeChanged"></asp:DropDownList> 
                            <asp:DropDownList ID="BlockCB" runat="server" AutoPostBack="True" OnSelectedIndexChanged="OnBlockChanged"></asp:DropDownList> 
                        </td>
                    </tr>

                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Sections") %>:</td>
                        <td width="60%">
                            <asp:DropDownList ID="Sections" name="Sections" runat="server">
                                <asp:ListItem Text="<%$ LocRaw:SelectItem.ExportActive %>" Value="Active"/>
                                <asp:ListItem Text="<%$ LocRaw:SelectItem.ExportAll %>" Value="All"/>
                                <asp:ListItem Text="<%$ LocRaw:SelectItem.ExportNone %>" Value="None"/>
                            </asp:DropDownList>
                        </td>
                    </tr>

                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("Elements") %>:</td>
                        <td width="60%">
                            <asp:DropDownList ID="Elements" name="Elements" runat="server">
                                <asp:ListItem Text="<%$ LocRaw:SelectItem.ExportActive %>" Value="Active"/>
                                <asp:ListItem Text="<%$ LocRaw:SelectItem.ExportAll %>" Value="All"/>
                                <asp:ListItem Text="<%$ LocRaw:SelectItem.ExportNone %>" Value="None"/>
                            </asp:DropDownList>
                        </td>
                    </tr>

                    <tr class="heading">
                        <td colspan="2"><%= GetMessage("SaveFileAs") %></td>
                    </tr> 

                    <tr>
                        <td width="40%" class="field-name"><%= GetMessage("DestinationFile") %>:</td>
                        <td width="60%">
                            <asp:TextBox ID="DestinationFile" name="DestinationFile" runat="server" Columns="40"></asp:TextBox>
                            <asp:Button ID="selectDirButton" runat="server" Text="..." UseSubmitBehavior="False" OnClientClick="return OnFileSelect();" />
                        </td>
                    </tr>

                    <tr>
                    <td></td>
                    <td><small><%= GetMessage("DestinationFileWarning")%></small></td>
                    </tr>
                </tbody>
                </table>

                <div class="buttons">
                    <asp:Button runat="server" id="StartButton" name="StartButton" Text="<%$ LocRaw:StartExport %>" OnClick="OnStartExport"/>
                    <asp:Button runat="server" id="StopButton" name="StopButton" Text="<%$ LocRaw:StopExport %>" OnClick="OnStopExport"/>
                </div>

        </bx:BXTabControlTab>
        </bx:BXTabControl>

        <bx:DirectoryBrowser ID="selectDirDialog" runat="server" ShowFiles="true" ItemsToSelect="Files" EnableExtras="false" ShowDescription="true"
            WindowTitle="<%$ Loc:DestinationFile %>" 
            OKClientScript="return OnFileOk();"
            CancelClientScript="return OnFileCancel();"
        />

    </ContentTemplate>
</asp:Content>