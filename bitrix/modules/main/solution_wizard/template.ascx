<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="Bitrix.Wizards.Solutions.TemplateWizardStep" %>
<script type="text/javascript">
	function SelectTemplate(element, solutionId)
	{
		var container = document.getElementById('solutions-container');
		var anchors = container.getElementsByTagName('A');
		for (var i = 0; i < anchors.length; i++)
		{
			if (anchors[i].parentNode != container)
				continue;
			anchors[i].className = 'solution-item';
		}
		element.className = 'solution-item solution-item-selected';
		var check = document.getElementById('template_radio_' + solutionId);
		if (check)
			check.checked = true;
		var hidden = document.getElementById("selected-solution");
		if (!hidden) 
		{
			hidden = document.createElement("INPUT");
			hidden.type = "hidden"
			hidden.id = "selected-solution";
			hidden.name = "selected-solution";
			container.appendChild(hidden);
		}
		hidden.value = solutionId;
	}
</script>
<div id="solutions-container">
<% string writeHidden = null; %>
<% foreach (var template in templates) { %>
	<a class="solution-item<% if (selected != null && selected == template.Id) { writeHidden = template.Id; selected = null; %> solution-item-selected<% } %>" href="javascript:void(0);" onclick="SelectTemplate(this, '<%= JSEncode(template.Id) %>');" ondblclick="WizardSubmit('next'); return false;">
		<b class="r3"></b><b class="r1"></b><b class="r1"></b>
		<div class="solution-inner-item">
			<input type="radio" id="template_radio_<%= Encode(template.Id) %>" name="template" style="float:left;margin-left:4px;margin-top:10px;margin-bottom:20px;" <% if (writeHidden == template.Id) { %>checked="checked"<% } %> />
			<% if (!string.IsNullOrEmpty(template.ImageUrl)) { %>
			<img alt="" src="<%= Encode(template.ImageUrl) %>" class="solution-image" />
			<% } %>
			<h4><%= template.TitleHtml %></h4>
			<% if (!string.IsNullOrEmpty(template.DescriptionHtml)) { %>
			<div class="solution-inner-item-content<% if (!string.IsNullOrEmpty(template.ImageUrl)) { %> solution-inner-item-image-content<% } %>"><%= template.DescriptionHtml %></div>
			<% } %>
		</div>
		<b class="r1"></b><b class="r1"></b><b class="r3"></b>
	</a>
<% } %>
<% if (writeHidden != null) { %>
	<input type="hidden" id="selected-solution" name="selected-solution" value="<%= Encode(writeHidden) %>" />
<% } %>
</div>