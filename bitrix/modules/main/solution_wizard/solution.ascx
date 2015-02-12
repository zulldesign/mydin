<%@ Control Language="C#" AutoEventWireup="true" CodeFile="solution.ascx.cs" Inherits="Bitrix.Wizards.Solutions.SolutionWizardStep" %>
<script type="text/javascript">
	function SelectSolution(element, solutionId)
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
		var check = document.getElementById('solution_radio_' + solutionId);
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
<% foreach (var solution in solutions) { %>
	<a class="solution-item<% if (selected != null && selected == solution.Id) { writeHidden = solution.Id; selected = null; %> solution-item-selected<% } %>" href="javascript:void(0);" onclick="SelectSolution(this, '<%= JSEncode(solution.Id) %>');" ondblclick="WizardSubmit('next'); return false;">
		<b class="r3"></b><b class="r1"></b><b class="r1"></b>
		<div class="solution-inner-item">
			<input type="radio" id="solution_radio_<%= Encode(solution.Id) %>" name="solution" style="float:left;margin-left:4px;margin-top:10px;margin-bottom:20px;" <% if (writeHidden == solution.Id) { %>checked="checked"<% } %> />
			<% if (!string.IsNullOrEmpty(solution.ImageUrl)) { %>
			<img alt="" src="<%= Encode(solution.ImageUrl) %>" class="solution-image" />
			<% } %>
			<h4><%= solution.TitleHtml %></h4>
			<div class="solution-inner-item-content<% if (!string.IsNullOrEmpty(solution.ImageUrl)) { %> solution-inner-item-image-content<% } %>"><%= solution.DescriptionHtml %></div>
		</div>
		<b class="r1"></b><b class="r1"></b><b class="r3"></b>
	</a>
<% } %>
<% if (writeHidden != null) { %>
	<input type="hidden" id="selected-solution" name="selected-solution" value="<%= Encode(writeHidden) %>" />
<% } %>
</div>