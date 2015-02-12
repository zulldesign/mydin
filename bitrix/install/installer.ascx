<%@ Control Language="C#" AutoEventWireup="true" CodeFile="installer.ascx.cs" Inherits="Bitrix.Installer.DefaultInstaller" %>
<%@ Import Namespace="Bitrix.Installer" %>
<%@ Register TagPrefix="bx" TagName="Wizard" Src="wizardhost.ascx" %>
<html>
	<head>
		<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
		<title><%= GetMessageRaw("Title") %></title>
		<noscript>
			<style type="text/css">
				div {display: none;}
				#noscript {padding: 3em; font-size: 130%; background:white;}
			</style>
		</noscript>
		
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
				-webkit-box-sizing: border-box;
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
				-webkit-box-sizing: border-box;
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

			table.data-table
			{
				width:100%;
				border-collapse:collapse;
				border:1px solid #d0d0d0;
			}

			table.data-table td
			{
				padding:5px !important;
				border:1px solid #d0d0d0;
			}

			table.data-table td.header
			{
				text-align:center;
				background: #e3f0f9;
				font-weight: bold;
			}

			#menu td.menu-number, #menu td.menu-name
			{
				background:#eaeaea url(<%= images %>/menu_fill.gif) repeat-x;
				height:40px;
				color:#c0c0c0;
			}

			#menu tr.menu-separator
			{
				height:2px;
				background: none;
			}

			#menu tr.selected td.menu-number, #menu tr.selected td.menu-name
			{
				background:#b41d07 url(<%= images %>/menu_fill_selected.gif) repeat-x;
				color:white;
			}

			#menu tr.done td.menu-number, #menu tr.done td.menu-name
			{
				color:black;
			}

			#menu td.menu-end
			{
				background: url(<%= images %>/menu_end.gif) repeat-x;
				width:11px;
			}

			#menu tr.selected td.menu-end
			{
				background: url(<%= images %>/menu_end_selected.gif) repeat-x;
				width:11px;
			}

			#menu td.menu-number
			{
				width:30px;
				font-size: 170%;
				text-align:center;
			}

			#menu td.menu-name
			{
				font-size:110%;
				padding-bottom:1px;
			}

			#copyright {font-size:95%; color:#606060; margin:4px 7px 0 7px; zoom:1;}

			form {margin:0; padding:0;}
			#step-error {color:red; padding:4px 4px 4px 25px;margin-bottom:4px; background:url(<%= images %>/error.gif) no-repeat;}
			small{font-size:85%;}
			
			a.wizard-button
			{
				background: transparent url(<%= images %>/button.png) no-repeat scroll top right;
				display: block;
				float: right;
				font-size:14px;
				height: 31px;
				padding-right: 18px;
				margin-left:15px;
				text-decoration: none;
				font-weight:bold;
			}
	
			a.wizard-button span
			{
				background: transparent url(<%= images %>/button.png) no-repeat;
				display: block;
				line-height: 17px;
				color:black;
				padding: 5px 0 9px 18px;
			}
			
			
			a.wizard-next-button
			{
				background: transparent url(<%= images %>/button_next.png) no-repeat scroll top right;
				display: block;
				float: right;
				font-size:14px;
				height: 31px;
				padding-right: 35px;
				margin-left:15px;
				text-decoration: none;
				font-weight:bold;
			}
	
			a.wizard-next-button span
			{
				background: transparent url(<%= images %>/button_next.png) no-repeat;
				display: block;
				line-height: 17px;
				color:black;
				padding: 5px 0 9px 18px;
			}

			a.wizard-prev-button
			{
				background: transparent url(<%= images %>/button_prev.png) no-repeat scroll top right;
				display: block;
				float: right;
				font-size:14px;
				height: 31px;
				padding-right: 18px;
				text-decoration: none;
				font-weight:bold;
			}
	
			a.wizard-prev-button span
			{
				background: transparent url(<%= images %>/button_prev.png) no-repeat;
				display: block;
				line-height: 17px;
				color:black;
				padding: 5px 0 9px 35px;
			}

			#solutions-container
			{
				margin-bottom: 15px;
			}
			
			a.solution-item
			{
				display:block; 
				border: 0; 
				margin-bottom: 10px; 
				color: Black;
				text-decoration: none;
				outline: none;
			}
						
			a.solution-item h4
			{
				margin: 10px;
				margin-top: 9px; /*compensating 1px padding*/
				font-family:Helvetica;
				font-size:1.5em;
			}
			a.solution-item p
			{
				margin: 10px;
			}
			
			div.solution-item-wrapper
			{
				width: 97px;
				float: left;
			}
			
			a.solution-picture-item
			{
				margin: 3px;
				text-align: center;
			}
			
			div.solution-description
			{
				margin-top: 3px;
				margin-left: 4px;
				color: #999;
				text-align:left;
			}
			
			a.solution-picture-item img.solution-image
			{
				width: 70px; 
				float: none;
				margin: 7px 0px 7px;
			}
			
			img.solution-image
			{
				width: 100px; 
				float: left; 
				margin: 10px;
				margin-left: 0px;
				border: 1px solid #CFCFCF;
			}
			div.solution-inner-item
			{
				padding: 1px;
				overflow: hidden;
				zoom: 1;
			}
			
			a.solution-item div.solution-inner-item, 
			a.solution-item b 
			{
				background-color:#F7F7F7;
				cursor: pointer;
				cursor: hand;
			}
			
			a.solution-item div.solution-inner-item div.solution-inner-item-content
			{
				padding-left: 20px;
				padding-bottom: 10px;
			}
			
			a.solution-item div.solution-inner-item div.solution-inner-item-image-content
			{
				padding-left: 125px;
			}
			
			a.solution-item:hover div.solution-inner-item, 
			a.solution-item:hover b 
			{
				background-color: #FFF0B2;
			}
			
			a.solution-item-selected div.solution-inner-item, 
			a.solution-item-selected b,
			a.solution-item-selected:hover div.solution-inner-item, 
			a.solution-item-selected:hover b
			{
				background-color: #CADBEC;
			}
			
			#solution-preview
			{
				margin-top: 10px;
			}
			
			#solution-preview div.solution-inner-item, 
			#solution-preview b 
			{
				background-color:#F7F7F7;
			}
			
			#solution-preview div.solution-inner-item
			{
				padding: 10px;
				text-align: center;
			}
			
			#solution-preview-image
			{
				border: 1px solid #CFCFCF;
				width: 450px;
			}
			
			/* Round Corners */
			.r0, .r1, .r2, .r3, .r4 { overflow: hidden; font-size:1px; display: block; height: 1px;}
			.r4 { margin: 0 4px; }
			.r3 { margin: 0 3px; }
			.r2 { margin: 0 2px; }
			.r1 { margin: 0 1px; }
			
			
			div.wizard-input-form
			{
			}
			
			div.wizard-input-form-block
			{
				margin-bottom:30px;
			}
			
			div.wizard-input-form-block h4
			{
				font-size:14px;
				margin-bottom:12px;
				color: #5E7CAD;
			}
			
			div.wizard-input-form-block-content
			{
				margin-left: 30px;
				margin-bottom: 25px;
				zoom: 1;
			}
			
			div.wizard-input-form-block-content img
			{
				border: solid 1px #D6D6D6;
				margin-bottom: 5px;
			}
			
			div.wizard-input-form-block-content img.no-border
			{
				border: none;
			}
			
			div.wizard-input-form-field-text input,
			div.wizard-input-form-field-textarea textarea,
			div.wizard-input-form-field-file input
			{
				width: 90%;
				border: solid 1px #CECECE;
				background-color: #F5F5F5;
				padding: 3px;

				font: 100%/100% Arial, sans-serif;
				float: left;
			}
			
			div.wizard-input-form-field-desc
			{
				color: rgb(119, 119, 119);
				zoom:1;
			}

			div.wizard-input-form-field
			{
				overflow: hidden;
				margin-bottom: 5px;
			}
		</style>
		
		<script type="text/javascript">
		<!--
			function InsertTitle(html)
			{ 
				window.setTimeout(function() { document.getElementById("step-title").innerHTML = html; }, 0);
			}

			function InsertButtons(html)
			{
				window.setTimeout(function() { document.getElementById("buttons_container").innerHTML = html; }, 0);
			}
			
			function InsertNavigation(html)
			{ 
				window.setTimeout(function() { document.getElementById("navigation_container").innerHTML = html; }, 0);
			}
			
			function InsertHTML(html)
			{
				window.setTimeout(function() { document.getElementById("html_container").innerHTML = html; }, 0);
			}
			
			function InsertErrors(htmls)
			{
				window.setTimeout(function() 
				{ 
					var container = document.getElementById("step-error"); 
					if (htmls && htmls.length > 0)
					{
						var s = '';
						for (var i = 0; i < htmls.length; i++)
						{
							if (i > 0)
								s += '<br/>';
							s += htmls[i];	
						}
						container.innerHTML = s;
						container.style.display = '';
					}
					else 
						container.style.display = 'none';
				}, 0);
			}

		//-->
		</script>


	</head>
<body id="bitrix_install_template">
<noscript>
<p id="noscript"><%= string.Format(GetMessageRaw("Placeholder.NoScript"), root) %></p>
</noscript>
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
										<%= string.Format(GetMessageRaw("Placeholder.Box"), root) %>
									</td>
								</tr>
								<tr>
									<td height="100%" valign="top" id="navigation_container">
										<asp:PlaceHolder runat="server" ID="NavigationView" />
										
									</td>
								</tr>
								<tr>
									<td align="center" height="100">
										<%= string.Format(GetMessageRaw("Placeholder.Logo"), root) %>
									</td>
								</tr>
							</table>
						</td>
						<td width="535" valign="top">
							<!-- Right column -->
							<table width="100%" height="77" cellpadding="0" cellspacing="0">
								<tr>
									<td width="9" style="background: #e3f0f9;"><img src="<%= images %>/top_gradient_begin.gif" width="9" height="77" alt="" /></td>
									<td class="wizard-title" width="14">&nbsp;</td>
									<td class="wizard-title"><%= WizardHost.WizardTitleHtml %></td>
								</tr>
							</table>
							<div id="step-title"><%= WizardHost.WizardStepTitleHtml %></div>
							
							<%= WizardHost.RenderBeginForm() %>
							<div id="step-content">
								<div id="step-error" <% if (WizardHost.WizardStepErrorHtml == null || WizardHost.WizardStepErrorHtml.Count == 0) { %>style="display: none"<% } %> >
								<% if (WizardHost.WizardStepErrorHtml != null && WizardHost.WizardStepErrorHtml.Count != 0) { %>
									<% foreach(string error in WizardHost.WizardStepErrorHtml) { %>
									<%= error %><br />
									<% } %>
								<% } %>
								</div>
								<div id="html_container">
								<bx:Wizard ID="WizardHost" runat="server" WizardButtonsPlaceHolderId="ButtonsView" WizardNavigationPlaceHolderId="NavigationView" />
								</div>
								<br />
								<br />
								<br />
								<div class="buttons" id="buttons_container">
									<asp:PlaceHolder runat="server" ID="ButtonsView" />
								</div>
								<br />
								<br />
								<br />
							</div>
							<%= WizardHost.RenderEndForm() %>
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
							<td><%= string.Format(GetMessageRaw("Placeholder.Copyright"), root) %></td>
							<td align="right"><%= string.Format(GetMessageRaw("Placeholder.Links"), root) %></td>
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