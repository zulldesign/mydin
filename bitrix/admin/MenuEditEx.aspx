<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
CodeFile="MenuEditEx.aspx.cs" Inherits="bitrix_admin_MenuEditEx" Title="<%$ Loc:PageTitle %>" EnableViewState="false" %>

<%@ Register Src="~/bitrix/controls/Main/TimeInterval.ascx" TagName="TimeInterval" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/admin/controls/Main/RoleList.ascx" TagName="RoleList" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/controls/Main/UrlParameter.ascx" TagName="UrlParameter" TagPrefix="bx" %>

<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
       <script>
			            function ChType(ob)
			            {
				            window.location.href = "MenuEditEx.aspx?path=" + encodeURIComponent('<%= Bitrix.Services.Js.BXJSUtility.Encode(CurDir) %>\\' + ob[ob.selectedIndex].value + '.menu');
			            }
			            
			            function ShowSelected(value,i)
                        {
                            document.getElementById('type_0_'+i).style.display="none"
                            document.getElementById('type_1_'+i).style.display="none"
                            document.getElementById('type_2_'+i).style.display="none"
                            document.getElementById('type_3_'+i).style.display="none"
                            document.getElementById('type_4_'+i).style.display="none"
                            document.getElementById('type_'+value+"_"+i).style.display="block";
                        }
    </script>
    
    <%
		BackButton.Href = "FileMan.aspx?path=" + UrlEncode(CurDir);
    	SimpleModeButton.Href = "MenuEdit.aspx?path=" + UrlEncode(Path);
    	SendToFilemanButton.Href = "FileManEdit.aspx?path=" + UrlEncode(Path) + "&" + Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl + "=" + UrlEncode(Request.RawUrl) + (!Bitrix.IO.BXSecureIO.FileExists(Path) ? "&new=" : ""); 
    %>
    <bx:BXContextMenuToolbar ID="mainActionBar" runat="server" >
        <Items>
            <bx:BXCmSeparator runat="server" SectionSeparator="true" />
            <bx:BXCmImageButton runat="server" ID="BackButton" CssClass="context-button"
                Text="<%$ Loc:Back %>" Title="<%$ Loc:Back %>" />
            <bx:BXCmSeparator runat="server" SectionSeparator="true" />
            <bx:BXCmImageButton runat="server" ID="SimpleModeButton" CssClass="context-button" 
                Text="<%$ Loc:SimpleMode %>" Title="<%$ Loc:SimpleMode %>" />
            <bx:BXCmSeparator runat="server" SectionSeparator="true" />
            <bx:BXCmImageButton runat="server" ID="SendToFilemanButton" CssClass="context-button" 
                Text="<%$ Loc:EditAsFile %>" Title="<%$ Loc:EditAsFile %>" />
            <bx:BXCmSeparator runat="server" SectionSeparator="true" Visible="false" />
            <bx:BXCmImageButton runat="server" CssClass="context-button" CommandName="delete"  Visible="false"
                Text="<%$ Loc:DeleteMenu %>" Title="<%$ Loc:DeleteMenu %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" />
        </Items>
    </bx:BXContextMenuToolbar>
	<bx:BXMessage ID="resultMessage" runat="server" CssClass="Ok" IconClass="Ok" Visible="False" />
    <asp:HiddenField ID="MenuItemsCount" runat="server" />
    <bx:BXTabControl ID="mainTabControl" runat="server" OnCommand="mainTabControl_Command">
        <bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText %>" Title="<%$ Loc:TabTitle %>">
            <table cellspacing="0" class="edit-table">
                <tr>
                    <td valign="top" class="field-name">
                        <asp:Literal runat="server" ID="Literal4" Text="<%$ Loc:MenuType %>"></asp:Literal>:</td>
                    <td valign="top">
                        <asp:DropDownList runat="server" ID="ddlMenu" onchange="ChType(this);">
                        </asp:DropDownList>
                    </td>
                </tr>
            </table>
            <table cellspacing="0" class="edit-table">
                <tbody>
                    <tr>
                        <td runat="server">
                            <table cellspacing="0" class="internal" width="100%">
                                <tr class="heading" runat="server">
                                    <td align="center" runat="server" colspan="2">
                                        <asp:Literal runat="server" ID="Literal11" Text="<%$ Loc:ColItems %>"></asp:Literal></td>
                                </tr>
                                <tr>
                                    <td align="right" colspan="2">
                                        <asp:Button runat="server" ID="btmInsertTop" Text="<%$ Loc:Insert %>" CommandArgument="0"
                                            OnClick="btmInsertBottom_Click" />
                                    </td>
                                </tr>
                                <asp:Repeater runat="server" ID="repItems">
                                    <ItemTemplate>
                                        <tr>
                                            <td width="50%">
                                                <table width="100%" cellspacing="0" cellpadding="1" border="0">
                                                    <tbody>
                                                        <tr>
                                                            <td width="0%" valign="top" align="right">
                                                                <asp:Literal runat="server" ID="Literal11" Text="<%$ Loc:Name %>"></asp:Literal>:</td>
                                                            <td width="100%" valign="top">
                                                                <asp:TextBox runat="server" ID="tbTitle" Text='<%# Eval("Title") %>' Columns="20"></asp:TextBox>
                                                        </tr>
                                                        <tr>
                                                            <td valign="top" align="right">
                                                                <asp:Literal runat="server" ID="Literal1" Text="<%$ Loc:Link %>"></asp:Literal>:</td>
                                                            <td valign="top">
                                                                <asp:TextBox runat="server" ID="tbLink" Text='<%# Eval("Link") %>' Columns="20"></asp:TextBox>
                                                        </tr>
                                                        <tr>
                                                            <td valign="top" align="right">
                                                                <asp:Literal runat="server" ID="Literal2" Text="<%$ Loc:Sort %>"></asp:Literal>:</td>
                                                            <td valign="top">
                                                                <asp:TextBox runat="server" ID="tbSort" Text='<%# index*10 %>' Columns="5" MaxLength="5"></asp:TextBox>
                                                        </tr>
                                                        <tr>
                                                            <td valign="top" align="right">
                                                                <asp:Literal runat="server" ID="Literal3" Text="<%$ Loc:Delete %>"></asp:Literal></td>
                                                            <td valign="top" align="left">
                                                                <asp:CheckBox runat="server" ID="cbDelete" ></asp:CheckBox> <%--Checked='<%# Eval("IsDeleted") %>'--%>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </td>
                                            <td width="50%">
                                                <table>
                                                    <tbody>
                                                        <tr>
                                                            <td valign="top" align="right">
                                                                <asp:Literal runat="server" ID="Literal5" Text="<%$ Loc:ExtraLinks %>"></asp:Literal>:</td>
                                                            <td valign="top">
                                                                <asp:TextBox Columns="30" Rows="3" TextMode="MultiLine" runat="server" ID="tbExtra"></asp:TextBox></td>
                                                        </tr>
                                                        <tr>
                                                            <td valign="top" align="right">
                                                                <asp:Literal runat="server" ID="Literal6" Text="<%$ Loc:ConditionTYpe %>"></asp:Literal>:</td>
                                                            <td valign="top">
                                                                <asp:DropDownList runat="server" ID="ConditionType">
                                                                    <asp:ListItem Text="<%$ Loc:ConditionType.None %>" Value="0"></asp:ListItem>
                                                                    <asp:ListItem Text="<%$ Loc:ConditionType.FileOrFolder %>" Value="1"></asp:ListItem>
                                                                    <asp:ListItem Text="<%$ Loc:ConditionType.Group %>" Value="2"></asp:ListItem>
                                                                    <asp:ListItem Text="<%$ Loc:ConditionType.Time %>" Value="3"></asp:ListItem>
                                                                    <asp:ListItem Text="<%$ Loc:ConditionType.URL %>" Value="4"></asp:ListItem>
                                                                </asp:DropDownList>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td valign="top" align="right">
                                                                <asp:Literal runat="server" ID="Literal7" Text="<%$ Loc:Condition %>"></asp:Literal>:</td>
                                                            <td valign="top">
                                                                <div id="type_0_<%# index %>">
                                                                    <%= GetMessage("NoCondition")%>
                                                                </div>
                                                                <div id="type_1_<%# index %>" style="display: none;">
                                                                    <asp:TextBox runat="server" ID="tbFileOrFolder" Columns="30" Text='<%#  ((Bitrix.ConditionType)Eval("ConditionType") == Bitrix.ConditionType.FileOrFolder) ? Eval("Condition") : "" %>'></asp:TextBox>
                                                                </div>
                                                                <div id="type_2_<%# index %>" style="display: none;">
                                                                    <bx:RoleList runat="server" ID="rlGroup" Str='<%#  ((Bitrix.ConditionType)Eval("ConditionType") == Bitrix.ConditionType.Group) ? Eval("Condition") : "" %>' />
                                                                </div>
                                                                <div id="type_3_<%# index %>" style="display: none;">
                                                                    <bx:TimeInterval runat="server" ID="tiPeriod" Str='<%#  ((Bitrix.ConditionType)Eval("ConditionType") == Bitrix.ConditionType.Time) ? Eval("Condition") : "" %>' />
                                                                </div>
                                                                <div id="type_4_<%# index %>" style="display: none;">
                                                                    <bx:UrlParameter runat="server" ID="upParams" Str='<%#  ((Bitrix.ConditionType)Eval("ConditionType") == Bitrix.ConditionType.Url) ? Eval("Condition") : "" %>' />
                                                                </div>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td valign="top" align="right">
                                                                <asp:Literal runat="server" ID="Literal8" Text="<%$ Loc:Params %>"></asp:Literal>:</td>
                                                            <td valign="top" nowrap="">
                                                                <table cellspacing="1" cellpadding="0" border="0">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td align="center">
                                                                                <asp:Literal runat="server" ID="Literal9" Text="<%$ Loc:Name %>"></asp:Literal></td>
                                                                            <td align="center">
                                                                                <asp:Literal runat="server" ID="Literal10" Text="<%$ Loc:Value %>"></asp:Literal></td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td nowrap="">
                                                                                <asp:TextBox runat="server" ID="tbParamName" Columns="15" Text='<%# Eval("ParamName") %>'></asp:TextBox>=</td>
                                                                            <td>
                                                                                <asp:TextBox runat="server" ID="tbParamValue" Columns="25" Text='<%# Eval("ParamValue") %>'></asp:TextBox></td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" colspan="2">
                                                <asp:Button runat="server" ID="btmInsertBottom" Text="<%$ Loc:Insert %>" CommandArgument="<%# index++ %>"
                                                    OnClick="btmInsertBottom_Click" />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </table>
                        </td>
                    </tr>
                </tbody>
            </table>
        </bx:BXTabControlTab>
    </bx:BXTabControl>
</asp:Content>
