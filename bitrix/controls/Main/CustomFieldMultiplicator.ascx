<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomFieldMultiplicator.ascx.cs" Inherits="bitrix_ui_CustomFieldMultiplicator" %>
<asp:PlaceHolder runat="server" ID="Editors"></asp:PlaceHolder>
<br />
<asp:Button ID="Button1" runat="server" CausesValidation="False" OnClick="Button1_Click"
    Text="<%$ Loc:Kernel.Add %>" />