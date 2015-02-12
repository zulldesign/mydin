<%@ Control Language="C#" AutoEventWireup="true" CodeFile="launcher.ascx.cs" Inherits="Bitrix.Installer.InstallLauncher" %>
<html>
<head>
	<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
	<title><%= Phrases["Title"] %></title>
	<% 
	string root = VirtualPathUtility.ToAbsolute("~/");
	root = (root == "/") ? "" : VirtualPathUtility.RemoveTrailingSlash(root);
	string images = VirtualPathUtility.ToAbsolute(AppRelativeTemplateSourceDirectory + "images"); 
	%>
	<style type="text/css">

		html {height:100%;}

		body 
		{
			background:#4a507b url(<%= images %>/bg_fill.gif) repeat;
			margin:0;
			padding:0;
			padding-bottom:6px;
			font-family: Arial, Verdana, Helvetica, sans-serif;
			font-size:82%;
			height:100%;
			color:black;
			box-sizing:border-box;
			-moz-box-sizing:border-box;
			-webkit-box-sizing:border-box;
		}

		table {font-size:100.01%;}

		a {color:#2676b9}

		h3 {font-size:120%;}

		#container
		{
			padding-top:6px;
			height:100%;
			background: transparent url(<%= images %>/bg_top.gif) repeat-x;
			box-sizing:border-box;
			-moz-box-sizing:border-box;
			-webkit-box-sizing:border-box;
		}

		#main-table
		{
			width:760px;
			height:100%;
			border-collapse:collapse;
		}

		#main-table td {padding:0;}

		td.wizard-title
		{
			background:#e3f0f9 url(<%= images %>/top_gradient_fill.gif) repeat-x; 
			height:77px; 
			color:#19448a; 
			font-size:140%; 
		}
		#step-title
		{
			color:#cd4d3e; 
			margin: 20px; 
			padding-bottom:20px; 
			border-bottom:1px solid #d9d9d9; 
			font-weight:bold;
			font-size:120%;
		}
		#step-content {margin:20px 25px; zoom:1;}

		#copyright {font-size:95%; color:#606060; margin:4px 7px 0 7px; zoom:1;}

		#step-error {color:red; padding:4px 4px 4px 25px;margin-bottom:4px; background:url(<%= images %>/error.gif) no-repeat;}
		small{font-size:85%;}

	</style>
</head>
<body id="bitrix_install_template" <% if (Request.Form["launch"] == null) { %>onload="document.getElementById('form').submit();"<% } %>>
<% if (Request.Form["launch"] == null) { %>
<form id="form" method="post" action="<%= HttpUtility.HtmlAttributeEncode(Url) %>"><input type="hidden" name="launch" value="" /></form>
<% } %>
<div id="container">
	<table id="main-table" align="center">
		<tr>
			<td width="10" height="10"><img src="<%= images %>/corner_top_left.gif" width="10" height="10" alt="" /></td>
			<td width="100%">
				<table width="100%" height="100%" cellpadding="0" cellspacing="0">
					<tr>
						<td width="215" height="10" style="background:white;"></td>
						<td width="525" height="10" style="background:#e3f0f9;"></td>
					</tr>
				</table>
			</td>
			<td width="10" height="10"><img src="<%= images %>/corner_top_right.gif" width="10" height="10" alt="" /></td>
		</tr>
		<tr>
			<td colspan="3" height="100%" style="background:white">
				<table width="100%" height="100%" cellpadding="0" cellspacing="0">
					<tr>
						<td width="225" valign="top">
							<!-- Left column -->
							<table width="100%" height="100%" cellpadding="0" cellspacing="0">
								<tr>
									<td align="center" height="185">
										<%= string.Format(Phrases["Placeholder.Box"], root) %>
									</td>
								</tr>
								<tr>
									<td height="100%" valign="top" >&nbsp;</td>
								</tr>
								<tr>
									<td align="center" height="100">
										<%= string.Format(Phrases["Placeholder.Logo"], root) %>
									</td>
								</tr>
							</table>
						</td>
						<td width="535" valign="top">
							<!-- Right column -->
							<table width="100%" height="77" cellpadding="0" cellspacing="0">
								<tr>
									<td width="9" style="background: #e3f0f9;">
										<img src="<%= images %>/top_gradient_begin.gif" width="9" height="77" alt="" />
									</td>
									<td class="wizard-title" width="14">&nbsp;</td>
									<td class="wizard-title"><%= Phrases["Title"] %></td>
								</tr>
							</table>
													
							<div id="step-content">
								<% if (Request.Form["launch"] != null) { %>
								
									<p><%= Phrases["Launcher.ProblemsHeader"] %></p>
									<ul>
										<% if (errors != null) %>
										<% foreach (string error in errors) { %>
										<li><%= error %></li>
										<% } %>
									</ul>
									<p><%= Phrases["Launcher.ProblemsFooter"] %></p>
								
								<% } else { %>
									<script type="text/javascript">
									<!--
										document.write('<p><%= Phrases["Launcher.Initializing"] %></p>');
									//-->
									</script>
									<noscript>
									<p><%= Phrases["Launcher.ProblemsHeader"] %></p>
									<ul>
										<li><%= Phrases["Launcher.Problem.JavaScript"] %></li>
									</ul>
									<p><%= Phrases["Launcher.ProblemsFooter"] %></p>
									</noscript>
																	
								<% } %>
							</div>
						</td>
					</tr>
				</table>
			</td>
		</tr>

		<tr height="20" style="background:#e8e8e8;">
			<td colspan="3">
				<div id="copyright">
					<table width="100%" height="100%" cellpadding="0" cellspacing="5">
						<tr>
							<td><%= string.Format(Phrases["Placeholder.Copyright"], root) %></td>
							<td align="right"><%= string.Format(Phrases["Placeholder.Links"], root) %></td>
						</tr>
					</table>
				</div>
		</tr>
		<tr>
			<td width="10" height="10" valign="bottom"><img src="<%= images %>/corner_bottom_left.gif" width="10" height="10" alt="" /></td>
			<td width="100%" style="background:#e8e8e8;"></td>
			<td width="10" height="10" valign="bottom"><img src="<%= images %>/corner_bottom_right.gif" width="10" height="10" alt="" /></td>
		</tr>
	</table>
</div>
</body>
</html>