<%@ Control Language="C#" AutoEventWireup="true" CodeFile="requirements.ascx.cs" Inherits="Bitrix.Wizards.Install.RequirementsWizardStep" %>
<%@ Import Namespace="Bitrix.Install" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Web.Hosting" %>
<%@ Import Namespace="System.IO" %>
<script type="text/javascript">
	
	function SendAjaxGet(url, callback)
	{
		var req = false;
		if(window.XMLHttpRequest)
			try{req = new XMLHttpRequest();} catch(e) {req = false;}
		else if(window.ActiveXObject)
		{
			try 
			{
				req = new ActiveXObject("Msxml2.XMLHTTP");
			}
			catch(e)
			{
				try {req = new ActiveXObject("Microsoft.XMLHTTP");}	catch(e) { req = false;}
			}
		}
	
		if (!req)
			return false;

		req.onreadystatechange = function() { callback(req) };
		req.open("GET", url, true);
		req.send("");
		return true;
	}
</script>
<h3><%= GetMessage("Header.RequiredParameters") %></h3>
<%= GetMessage("Description.RequiredParameters") %><br/>
<br/>
<table border="0" class="data-table">
	<tr>
		<td class="header"><%= GetMessage("Column.Parameter") %></td>
		<td class="header"><%= GetMessage("Column.Required") %></td>
		<td class="header"><%= GetMessage("Column.CurrentValue") %></td>
	</tr>
	<tr>
		<td valign="top"><%= GetMessage("Title.IISVersion") %></td>
		<td valign="top"><%= GetMessage("Value.RequiredIISVersion") %></td>
		<td valign="top"><b><span <%= Checks[Check.IisVersion].Style() %>><%= Checks[Check.IisVersion].Value %></span></b></td>
	</tr>
	<tr>
		<td valign="top"><%= GetMessage("Title.FrameworkVersion") %></td>
		<td valign="top"><%= GetMessage("Value.RequiredFrameworkVersion") %></td>
		<td valign="top"><b><span <%= Checks[Check.FrameworkVersion].Style() %>><%= Checks[Check.FrameworkVersion].Value %></span></b></td>
	</tr>
	<tr>
		<td valign="top"><%= GetMessage("Title.TrustLevel") %></td>
		<td valign="top">Medium</td>
		<td valign="top"><b><span <%= Checks[Check.TrustLevel].Style() %>><%= Checks[Check.TrustLevel].Value %></span></b></td>
	</tr>
</table>
<h3><%= GetMessage("Header.DiskAccess") %></h3>
<%= GetMessage("Description.DiskAccess") %><br/>
<br>
<table border="0" class="data-table">
	<tr>
		<td class="header"><%= GetMessage("Column.Parameter") %></td>
		<td class="header"><%= GetMessage("Column.Value") %></td>
	</tr>
	<tr>
		<td valign="top"><%= string.Format(GetMessage("Title.DiskAccessPublicArea"), Encode(HostingEnvironment.MapPath("~/"))) %></td>
		<td valign="top"><b><span <%= Checks[Check.RootFolder].Style() %>><%= Checks[Check.RootFolder].Value %></span></b></td>
	</tr>
	<tr>
		<td valign="top"><%= string.Format(GetMessage("Title.DiskAccessKernel"), Encode(HostingEnvironment.MapPath("~/bitrix/"))) %></td>
		<td valign="top"><b><span <%= Checks[Check.BitrixFolder].Style() %>><%= Checks[Check.BitrixFolder].Value %></span></b></td>
	</tr>
	<tr>
		<td valign="top"><%= string.Format(GetMessage("Title.DiskAccessFile"), Encode(HostingEnvironment.MapPath("~/web.config"))) %></td>
		<td valign="top"><b><span <%= Checks[Check.WebConfig].Style() %>><%= Checks[Check.WebConfig].Value %></span></b></td>
	</tr>
	<tr>
		<td valign="top"><%= string.Format(GetMessage("Title.DiskAccessFolder"), Encode(HostingEnvironment.MapPath("~/bitrix/modules/"))) %></td>
		<td valign="top"><b><span <%= Checks[Check.ModulesFolder].Style() %>><%= Checks[Check.ModulesFolder].Value %></span></b></td>
	</tr>
	<tr>
		<td valign="top"><%= string.Format(GetMessage("Title.DiskAccessFolder"), Encode(HostingEnvironment.MapPath("~/upload/"))) %></td>
		<td valign="top"><b><span <%= Checks[Check.UploadFolder].Style() %>><%= Checks[Check.UploadFolder].Value %></span></b></td>
	</tr>
</table>
<h3><%= GetMessage("Header.Recomendations") %></h3>
<%= GetMessage("Description.Recomendations") %>
<br>
<br>
<table border="0" class="data-table">
	<tr>
		<td class="header"><%= GetMessage("Column.Parameter") %></td>
		<td class="header"><%= GetMessage("Column.Recomended") %></td>
		<td class="header"><%= GetMessage("Column.CurrentValue") %></td>
	</tr>
	<tr>
		<td valign="top">Microsoft .NET Framework 4.0</td>
		<td valign="top"><%= GetMessage("Value.Installed") %></td>
		<td valign="top">
			<% 
				bool framework4Installed = Environment.Version.Major >= 4;  		
			%>
			<b><span style="color: <%= framework4Installed ? "Green" : "Olive" %>"><%= framework4Installed ? GetMessage("Value.Installed") : GetMessage("Value.NotInstalled")%></span></b>
		</td>
	</tr>
	<tr>
		<td valign="top"><%= GetMessage("Title.IntegratedMode") %></td>
		<td valign="top"><%= GetMessage("Value.Enabled") %></td>
		<td valign="top">
			<% 
				bool integratedModeSupported = iisMajor >= 7;
				bool integratedMode = integratedModeSupported && Context.Items.Contains(BXInstallerIMCheckerHttpModule.IntegratedModeKey); 
			%>
			<b><span style="color: <%= integratedMode ? "Green" : "Olive" %>"><%= integratedModeSupported ? (integratedMode ? GetMessage("Value.Enabled") : GetMessage("Value.Disabled")) : GetMessage("Value.IntegratedModeNotSupported") %></span></b>
		</td>
	</tr>
	<tr>
		<td valign="top"><%= GetMessage("Title.SEFUrlProcessing") %></td>
		<td valign="top"><%= GetMessage("Value.EnabledF") %></td>
		<td valign="top"><span id="sef_url_test"><b><span style="color: Olive"><%= GetMessage("Value.NA") %></span></b></span>
		<script type="text/javascript">
			if (!SendAjaxGet(
				"<%= JSEncode(VirtualPathUtility.ToAbsolute("~/sef/url/test/")) %>?random=" + Math.random(),
				function(req)
				{
					if (req.readyState == 4)
					{
						if (req.responseText == "SUCCESS")
							res = '<b><span style="color:Green"><%= GetMessage("Value.EnabledF") %></span></b>';
						else
							res = '<b><span style="color:Olive"><%= GetMessage("Value.DisabledF") %></span></b>';
						document.getElementById("sef_url_test").innerHTML = res;
					}
				}
			))
			{
				document.getElementById("sef_url_test").innerHTML = '<b><span style="color:Olive"><%= GetMessage("Value.JavascriptError") %></span></b>';
			}
		</script>
		</td>
	</tr>
	<tr>
		<td colspan="3"><b><%= GetMessage("Header.HostingConfiguration") %>:</b></td>
	</tr>
	<tr>
		<td valign="top"><%= GetMessage("Title.SpecificVDirs") %></td>
		<td valign="top"><%= GetMessage("Value.NotPresent") %></td>
		<%
		string specificVDir = null;		
		{
			
			DirectoryInfo root = new DirectoryInfo(HostingEnvironment.MapPath("~/"));
			foreach (DirectoryInfo dir in root.GetDirectories())
			{
				try
				{
					string newPath = HostingEnvironment.MapPath("~/" + dir.Name);
					if (!string.Equals(newPath, dir.FullName, StringComparison.InvariantCultureIgnoreCase))
					{
						specificVDir = dir.Name;
						break;
					}
				}
				catch
				{
					specificVDir = dir.Name;
					break;
				}
			}
		}
		%>
		<td valign="top"><b><span style="color: <%= specificVDir == null ? "Green" : "Olive" %>"><%= specificVDir == null ? GetMessage("Value.NotPresent") : string.Format(GetMessage("Value.VDirFound"), Encode(specificVDir)) %></span></b></td>
	</tr>
	<tr>
		<td valign="top"><%= GetMessage("Title.FileNameSpecialChars") %></td>
		<td valign="top"><%= GetMessage("Value.ProcessedCorrectly") %></td>
		<%
		string specialChars = TestSpecialCharsFolders();	
		%>
		<td valign="top"><span id="url_special_chars"><b><span style="color: Olive"><%= specialChars == null ? GetMessage("Value.NA") : specialChars %></span></b></span>
		<% if (specialChars == null) { %>
		<script type="text/javascript">
			var urlSpecialsPath = [
				"<%= JSEncode(VirtualPathUtility.ToAbsolute(AppRelativeTemplateSourceDirectory) + "tests/.folder/file.txt") %>",
				"<%= JSEncode(VirtualPathUtility.ToAbsolute(AppRelativeTemplateSourceDirectory) + "tests/~folder/file.txt") %>",
				"<%= JSEncode(VirtualPathUtility.ToAbsolute(AppRelativeTemplateSourceDirectory) + "tests/.file.txt") %>",
				"<%= JSEncode(VirtualPathUtility.ToAbsolute(AppRelativeTemplateSourceDirectory) + "tests/~file.txt") %>"
			];
			function UrlSpecials(i)
			{
				if (i >= urlSpecialsPath.length)
				{
					document.getElementById("url_special_chars").innerHTML = '<b><span style="color:Green"><%= GetMessage("Value.ProcessedCorrectly") %></span></b>';
					return;
				}
				if (!SendAjaxGet(
					urlSpecialsPath[i] + "?random=" + Math.random(),
					function(req)
					{
						if (req.readyState == 4) 
						{
							if (req.responseText == "SUCCESS")
								UrlSpecials(i + 1);
							else
								document.getElementById("url_special_chars").innerHTML = '<b><span style="color:Olive">' + '<%= GetMessage("Value.UnableToReadFile") %>'.replace(/\{0\}/i, urlSpecialsPath[i].replace('$', '$$')) + '</span></b>';
						}
					}
				))
				{
					document.getElementById("url_special_chars").innerHTML = '<b><span style="color:Olive"><%= GetMessage("Value.JavascriptError") %></span></b>';
					return;
				}
			}
			UrlSpecials(0);
		</script>
		<% } %>
		</td>
		
		
	</tr>
	<tr>
		<td valign="top"><%= GetMessage("Title.EscapedUrlSequences") %></td>
		<td valign="top"><%= GetMessage("Value.ProcessedCorrectly") %></td>
		<td valign="top"><span id="query_special_chars"><b><span style="color: Olive"><%= GetMessage("Value.NA") %></span></b></span>
		<% string paramsString = "%7e%2f.%2b-%25%23!%40*%5c%22%257e"; %>
		<script type="text/javascript">
			if (!SendAjaxGet(
				"<%= JSEncode(VirtualPathUtility.ToAbsolute(AppRelativeTemplateSourceDirectory) + "tests/success.ashx") %>?params1=<%= paramsString %>&params2=<%= HttpUtility.UrlEncode(paramsString) %>&random=" + Math.random(),
				function(req)
				{
					if (req.readyState == 4)
					{
						if (req.responseText == "SUCCESS")
							res = '<b><span style="color:Green"><%= GetMessage("Value.ProcessedCorrectly") %></span></b>';
						else
							res = '<b><span style="color:Olive"><%= GetMessage("Value.ProcessedIncorrectly") %></span></b>';
						document.getElementById("query_special_chars").innerHTML = res;
					}
				}
			))
			{
				document.getElementById("query_special_chars").innerHTML = '<b><span style="color:Olive"><%= GetMessage("Value.JavascriptError") %></span></b>';
			}
		</script>
		</td>
	</tr>
</table>
<br />
<br />
<table class="data-table">
	<tr><td width="0%"><%= GetMessage("Legend") %></td></tr>
</table>
