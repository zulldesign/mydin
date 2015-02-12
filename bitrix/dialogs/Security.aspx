<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Security.aspx.cs" Inherits="bitrix_dialogs_Security"
	Title="<%$ Loc:PageTitle.ModificationOfAccessRights %>" %>

<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit"
	TagPrefix="uc1" %>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
		<bx:BXPageAsDialogBehaviour runat="server" ID="Behaviour" UseStandardStyles="false"
			OnSave="Behaviour_Save" />
		<uc1:OperationsEdit ID="OperationsEdit" runat="server" ShowLegend="true" ShowNotes="false"
			LegendText-Allow="<%$ LocRaw:Legend.Allow %>" LegendText-Deny="<%$ LocRaw:Legend.Deny %>" LegendText-InheritAllow="<%$ LocRaw:Legend.InheritAllow %>"
			LegendText-InheritDeny="<%$ LocRaw:Legend.InheritDeny %>" />
	</form>
</body>
</html>
