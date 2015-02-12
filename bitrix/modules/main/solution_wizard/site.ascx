<%@ Control Language="C#" AutoEventWireup="true" CodeFile="site.ascx.cs" Inherits="Bitrix.Wizards.Solutions.SiteWizardStep" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<script type="text/javascript">
	function OnRadioClick(mode)
	{
		var d = document.getElementById('existing_site_selector');
		if (d) d.style.visibility = mode == 'existing' ? '' : 'hidden';
		
		d = document.getElementById('new_site_options');
		if (d) d.style.display = mode == 'existing' ? 'none' : '';
	}
</script>
<% using (var list = UI.RadioButtonList("SiteMode")) { %>
	<div>
		<% list.RadioButton(GetMessage("Option.ExistingSite"), "existing", new[] { new KeyValuePair<string, string>("onclick", "OnRadioClick('existing');") }); %>
		<span id="existing_site_selector" style="margin-left: 5px; <%= UI.Data.GetString("SiteMode") != "existing" ? "visibility:hidden;" : ""  %>">
		<% 
			UI.Select(
				"ExistingSiteId", 
				Bitrix.BXSite.GetAllSitesReadOnly()
					.Where(x => x.Active)
					//.Where(x => { var site = BXSite.GetCurrentSite(x.DirectoryVirtualPath, Bitrix.Services.BXSefUrlManager.CurrentUrl.Host); return site != null && string.Equals(site.TextEncoder.Decode(site.Id), x.TextEncoder.Decode(x.Id), StringComparison.InvariantCultureIgnoreCase); })
					.Select(x => new ListItem(x.TextEncoder.Decode(x.Name), x.TextEncoder.Decode(x.Id)))
					.ToArray(),
				null
			); 
		%>
		</span>
	</div>
	<div style="margin-top:5px">
		<% list.RadioButton(GetMessage("Option.NewSite"), "new", new[] { new KeyValuePair<string, string>("onclick", "OnRadioClick('new');") }); %>
		<div id="new_site_options" class="wizard-input-form" style="margin-top: 5px; padding-left: 20px; <%= UI.Data.GetString("SiteMode") != "new" ? "display:none;" : ""  %>">
			
			<div class="wizard-input-form-block">
				<h4><span style="color: red">*</span><%= GetMessage("SiteLanguage") %></h4>
				<div class="wizard-input-form-block-content">
				<div class="wizard-input-form-field">
					<% 
						UI.Select(
							"NewSiteLang", 
							BXLanguage.GetList(
								new BXFilter(new BXFilterItem(BXLanguage.Fields.Active, BXSqlFilterOperators.Equal, true)), 
								new BXOrderBy(new BXOrderByPair(BXLanguage.Fields.Sort, BXOrderByDirection.Asc)), 
								new Bitrix.DataLayer.BXSelect(BXLanguage.Fields.ID, BXLanguage.Fields.Name), 
								null, 
								Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder
							)  
								.Select(x => new ListItem(x.Name, x.Id))
								.ToArray(),
							null
						); 
					%>
				</div>
				</div>
			</div>

			<div class="wizard-input-form-block">
				<h4><span style="color: red">*</span><%= GetMessage("SiteId") %></h4>
				<div class="wizard-input-form-block-content">
				<div class="wizard-input-form-field wizard-input-form-field-text">
					<% UI.InputText("NewSiteId", null); %>					
				</div>
				<div class="wizard-input-form-field-desc"><%= GetMessage("SiteId.Comment") %></div>
				</div>
			</div>

			<div class="wizard-input-form-block">
				<h4><span style="color: red">*</span><%= GetMessage("SiteName") %></h4>
				<div class="wizard-input-form-block-content">
				<div class="wizard-input-form-field wizard-input-form-field-text">
					<% UI.InputText("NewSiteName", null); %>
				</div>
				<div class="wizard-input-form-field-desc"><%= GetMessage("SiteName.Comment") %></div>
				</div>
			</div>
			
			<div class="wizard-input-form-block">
				<h4><span style="color: red">*</span><%= GetMessage("SiteFolder") %></h4>
				<div class="wizard-input-form-block-content">
				<div class="wizard-input-form-field wizard-input-form-field-text">
					<% UI.InputText("NewSiteFolder", null); %>					
				</div>
				<div class="wizard-input-form-field-desc"><%= GetMessage("SiteFolder.Comment") %></div>
				</div>
			</div>
		</div>
			
	</div>
<% } %>
