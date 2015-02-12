<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UserListButton.ascx.cs" Inherits="UserListButton" %>

<asp:TextBox ID="tbValue" Width="60" runat="server"></asp:TextBox>
<input id="bSearch" runat="server" type="button" value="..." onclick="" />
<asp:Label ID="lbName" runat="server" Text=""></asp:Label>
<asp:RequiredFieldValidator ID="ValueRequired" runat="server" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="tbValue" >*</asp:RequiredFieldValidator>

<script type="text/javascript" language="javascript">
    function SetEnabled(cmd) {
        var editor = document.getElementById("<%= tbValue.ClientID%>");
        var button = document.getElementById("<%= bSearch.ClientID%>");
        var label = document.getElementById("<%= lbName.ClientID%>");
        if (cmd) {
            editor.setAttribute("disabled", cmd);
            button.setAttribute("disabled", cmd);
            editor.value = "";
            label.innerHTML = "";
        } else {
            editor.disabled = cmd;
            button.disabled = cmd;
        }
    }
</script>
