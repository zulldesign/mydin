<%@ Control Language="C#" AutoEventWireup="true" CodeFile="theme.ascx.cs" Inherits="Bitrix.Wizards.Solutions.ThemeWizardStep" %>
<script type="text/javascript">
	function SelectTheme(element, solutionId, imageUrl)
	{
		var container = document.getElementById('solutions-container');
		var anchors = container.getElementsByTagName('A');
		for (var i = 0; i < anchors.length; i++)
		{
			if (anchors[i].parentNode.parentNode != container)
				continue;
			anchors[i].className = 'solution-item solution-picture-item';
		}
		element.className = 'solution-item  solution-picture-item solution-item-selected';
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

		var preview = document.getElementById("solution-preview");
		if (!imageUrl)
			preview.style.display = 'none';
		else 
		{
			document.getElementById("solution-preview-image").src = imageUrl;
			preview.style.display = '';
		}
	}
</script>
<div id="solutions-container" style="overflow:hidden">
<% 
	ThemeInfo selectedTheme = null;
	int i = 0;
%>
<% foreach (var theme in themes) { %>
	<div class="solution-item-wrapper">
	<a class="solution-item solution-picture-item<% if (selected != null && selected == theme.Id) { selectedTheme = theme; selected = null; %> solution-item-selected<% } %>" href="javascript:void(0);" onclick="SelectTheme(this, '<%= JSEncode(theme.Id) %>', '<%= JSEncode(theme.BigImageUrl) %>'); return false;" ondblclick="WizardSubmit('next'); return false;">
		<b class="r3"></b><b class="r1"></b><b class="r1"></b>
		<div class="solution-inner-item">
			<% if (!string.IsNullOrEmpty(theme.SmallImageUrl)) { %>
			<img alt="" src="<%= Encode(theme.SmallImageUrl) %>" class="solution-image" />
			<% } %>
			
		</div>
		<b class="r1"></b><b class="r1"></b><b class="r3"></b>
		<div class="solution-description"><%= ++i %>. <%= theme.TitleHtml %></div>
		
	</a>
	</div>
<% } %>
<% if (selectedTheme != null) { %>
	<input type="hidden" id="selected-solution" name="selected-solution" value="<%= Encode(selectedTheme.Id) %>" />
<% } %>
</div>
<div id="solution-preview" <% if (selectedTheme == null || string.IsNullOrEmpty(selectedTheme.BigImageUrl)) { %>style="display: none;"<% } %>>
	<b class="r3"></b><b class="r1"></b><b class="r1"></b>
	<div class="solution-inner-item">
	<img id="solution-preview-image" alt="" src="<% if (selectedTheme != null && !string.IsNullOrEmpty(selectedTheme.BigImageUrl)) { %><%= Encode(selectedTheme.BigImageUrl) %><% } %>" />
	</div>
	<b class="r1"></b><b class="r1"></b><b class="r3"></b>
</div>