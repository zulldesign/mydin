<%@ Control Language="C#" AutoEventWireup="true" Inherits="SettingsStep" codefile="settings.ascx.cs"%>
<%@ Import Namespace="System.Collections.Generic" %>
<% 
	var textarea = new[] 
	{ 
		new KeyValuePair<string, string>("rows", "5")
	}; 
	
	var file = new[] { 
		new KeyValuePair<string, string>("onchange", "return WizardSubmit('upload');")
	}; 
%>
<div class="wizard-input-form">
	<div class="wizard-input-form-block">
		<h4>Site Name</h4>
		<div class="wizard-input-form-block-content">
		<div class="wizard-input-form-field wizard-input-form-field-text"><% UI.InputText("SiteName", null); %></div>
		</div>
	</div>

<%
	var solutionPath = WizardContext.State.GetString("Installer.SolutionPath");
	var template = WizardContext.State.GetString("Installer.Template");	
%>

<% if (template == "taby") { %>
	<div class="wizard-input-form-block">
		<h4>Logo (recommended size: 100x100)</h4>
		<div class="wizard-input-form-block-content">
			<% if (!string.IsNullOrEmpty(logoUrl)) { %>
			<img src="<%=logoUrl %>" alt="" />
			<% } %>
			<div class="wizard-input-form-field wizard-input-form-field-file"><% UI.InputFile("logoImage", file); %></div>
		</div>
	</div>
<% } %>

	<div class="wizard-input-form-block">
		<h4>Site Title</h4>
		<div class="wizard-input-form-block-content">
			<div class="wizard-input-form-field wizard-input-form-field-textarea"><% UI.Textarea("Header", textarea); %></div>
			<%
				var path = VirtualPathUtility.ToAbsolute(Bitrix.IO.BXPath.Combine(solutionPath, "images/" + template + ".jpg"));
			%>
			<img src="<%= path %>" width="373" height="150" alt="" class="no-border"/>
		</div>
	</div>

	<div class="wizard-input-form-block">
		<h4>Copyrights</h4>
		<div class="wizard-input-form-block-content">
			<div class="wizard-input-form-field wizard-input-form-field-text"><% UI.InputText("Copyright", null); %></div>
		</div>
	</div>

	<div class="wizard-input-form-block">
		<div class="wizard-input-form-block-content">
			<div class="wizard-input-form-field wizard-input-form-field-checkbox"><% UI.CheckBox("InstallDemoData", "Install Demo Data", null); %></div>
			<div class="wizard-input-form-field wizard-input-form-field-checkbox"><% UI.CheckBox("Overwrite", "Overwrite Existing Files", null); %></div>
		</div>
	</div>
</div>