<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DeletePage.aspx.cs" Inherits="bitrix_dialogs_DeletePage" Title="<%$ Loc:TITLE %>" %>

<html>
<head id="Head1" runat="server">
</head>
<body>
    <form id="form1" runat="server">
        <bx:BXPageAsDialogBehaviour runat="server" ID="Behaviour" OnYes="Behaviour_Yes" />
        <div>
            <div runat="server" id="deleteMenuItem">
                <asp:CheckBox runat="server" ID="deleteMenuItemChkBx" Text="<%$ Loc:CheckBoxText.DeletePageFromMenus %>" Checked="true" />
            </div>
            <div runat="server" id="defaultPageDeletionAlert" style="color: #ff0000 !important;">
                <%= GetMessage("DELETION_OF_DAFAULT_PAGE_ALERT") %>
            </div>
            <div runat="server" id="scriptContainer">
            </div>            
        </div>
    </form>
</body>
</html>
