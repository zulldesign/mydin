<%@ Page Language="C#" AutoEventWireup="false" CodeFile="Login.aspx.cs" Inherits="bitrix_dialogs_Login" %>
<html>
<head runat="server">
    <title></title>
</head>
<body>   
    <bx:BXPageAsDialogBehaviour runat="server" ID="behaviour" Enabled ="true" 
        ButonSetLayout="Continue" OnContinue="behaviour_Continue" OnOnPopulateData="behaviour_OnPopulateData" ValidationGroup="LoginDialogValidation" />   
    <form id="form1" runat="server">
        <div>  
            <bx:BXLoginControl runat="server" ID="login" ValidationGroup="LoginDialogValidation" />
        </div>
    </form>
</body>
</html>
