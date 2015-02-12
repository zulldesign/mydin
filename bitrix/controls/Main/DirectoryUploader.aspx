<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<%@ Page Language="C#" EnableViewState="false" Inherits="Bitrix.UI.BXPage" %>

<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.IO" %>

<script runat="server">
	string message;
	string filepath;

	protected void Page_LoadComplete(object sender, EventArgs e)
	{
        up.OnClientClick += string.Format("if (parent.jsUtils.trim(document.getElementById('path').value) == '') {{ window.alert('{0}'); return false; }}", BXJSUtility.Encode(GetMessageRaw("SelectDirectory")));
        up.OnClientClick += string.Format("if (parent.jsUtils.trim(document.getElementById('file').value) == '') {{ window.alert('{0}'); return false; }}", BXJSUtility.Encode(GetMessageRaw("SelectFile")));
        
		up.OnClientClick += "document.getElementById('wait').style.display = 'inline';";
		if (!IsPostBack && Request.QueryString["defaultDir"] != null)
		{
            path.Value = BXPath.ToVirtualRelativePath(Request.QueryString["defaultDir"]);
            //path.Value = BXServices.PrepareWebPath(Request.QueryString["defaultDir"]);
            //if (!path.Value.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            //    path.Value = System.IO.Path.DirectorySeparatorChar + path.Value;
            
		}
		
		dir.Text = path.Value;
		name.Text = string.Empty;
	}

	string ProcessFile()
	{
		try
		{
			if (!file.HasFile)
				return GetMessageRaw("Error.NoFile");

			string path = this.path.Value;
			if (string.IsNullOrEmpty(path.Trim()))
				return GetMessageRaw("Error.NoDir");
			//path = BXServices.PrepareWebPath(path);
			path = BXPath.ToVirtualRelativePath(path);

			string filename = this.file.FileName;
			if (!string.IsNullOrEmpty(this.name.Text.Trim()))
				filename = this.name.Text.Trim();

			//filepath = BXServices.ConcatPath(path, filename);
            filepath = BXPath.Combine(path, filename);
            
			if (BXSecureIO.FileOrDirectoryExists(filepath))
				return GetMessageRaw("Error.AlreadyExists");

			if (!BXSecureIO.CheckUpload(filepath))
				return GetMessageRaw("Error.InsufficientRights");

			try
			{
				//file.SaveAs(BXServices.AddAppPath(filepath));
                file.SaveAs(BXPath.ToPhysicalPath(filepath));
			}
			catch (Exception e)
			{
				BXLogService.LogDefault(new BXLogMessage(e, 0, BXLogMessageType.Error, this.AppRelativeVirtualPath));
				return GetMessageRaw("Error.Save");
			}

			return null;
		}
		catch (Exception e)
		{
			BXLogService.LogDefault(new BXLogMessage(e, 0, BXLogMessageType.Error, this.AppRelativeVirtualPath));
			return GetMessageRaw("Error.Unknown");
		}
	}

	protected void up_Click(object sender, EventArgs e)
	{
		message = ProcessFile();
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Upload</title>
</head>
<body style="overflow: hidden; border: 0px none; margin: 0px; padding: 0px; font-family: Verdana,Arial,helvetica,sans-serif">
	<form id="form" runat="server">
		<asp:HiddenField ID="path" runat="server" />
		<table cellspacing="0" cellpadding="0" style="border: 0px none; width: 250px">
			<% if (!string.IsNullOrEmpty(message)) { %>
			<tr>
				<td colspan="2"><span style="font-size: 8pt; color: Red"><%= HttpUtility.HtmlEncode(message) %></span></td>
			</tr>
			<% } %>
			<tr>
				<td colspan="2">
					<span style="font-size: 8pt">
						<asp:Literal ID="Literal1" runat="server" Text="<%$ Loc:UploadFile %>" />:</span><br />
					<asp:FileUpload ID="file" runat="server" Width="245px" /></td>
			</tr>
			<tr>
				<td colspan="2">
					<span style="font-size: 8pt">
						<asp:Literal ID="Literal2" runat="server" Text="<%$ Loc:ServerFileName %>" />:</span><br />
					<asp:TextBox ID="name" runat="server" Width="245px" /></td>
			</tr>
			<tr>
				<td colspan="2">
					<span style="font-size: 8pt">
						<asp:Literal ID="Literal3" runat="server" Text="<%$ Loc:ServerDir %>" />:</span><br />
					<asp:TextBox ID="dir" runat="server" Width="245px" Enabled="false" /></td>
			</tr>
			<tr>
				<td style="text-align: left"><asp:Button ID="up" runat="server" Text="<%$ LocRaw:Upload %>" OnClick="up_Click" /></td>
				<%--
				    <td style="text-align: left; width: 100%"><img id="wait" alt="<%= GetMessage("Loading") %>" style="margin: 0px 5px 0px 5px; border: none; display: none" src="<%= BXConfigurationUtility.Constants.AdminThemeRoot + "images/wait.gif" %>" /> </td>
				zg, 25.04.2008
				--%>
                    <td style="text-align: left; width: 100%"><img id="wait" alt="<%= GetMessage("Loading") %>" style="margin: 0px 5px 0px 5px; border: none; display: none" src="<%= VirtualPathUtility.ToAbsolute(Bitrix.UI.BXThemeHelper.SimpleCombineWithCurrentThemePath("images/wait.gif")) %>" /> </td>
			</tr>
		</table>

		<script type="text/javascript">
		document.getElementById('<%= file.ClientID %>').onchange = function()
		{
			var path = this.value.replace(/\\/ig, '/');
			document.getElementById("<%= name.ClientID %>").value = path.substr(path.lastIndexOf("/") + 1);
		};
		<% if (IsPostBack && string.IsNullOrEmpty(message)) { %>
		try
		{
			window.frameElement.DirectoryBrowser.Reset();
			window.frameElement.DirectoryBrowser.ExpandPath('<%= Bitrix.Services.Js.BXJSUtility.Encode(filepath) %>');
		}
		catch(e)
		{}
		<% } %>
		</script>
	</form>
</body>
</html>
