<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="SearchReindex.aspx.cs" Inherits="bitrix_admin_SearchReindex" Title="<%$ LocRaw:PageTitle %>" EnableViewState="False" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Search" %>
<%@ Register TagName="IndexerState" TagPrefix="bx" Src="~/bitrix/admin/controls/Search/IndexerState.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<bx:InlineScript runat="server" ID="Script" SyncMode="ScriptBlock" AsyncMode="ScriptBlock" >
	<script type="text/javascript">
		function Search_ToggleSite()
		{
			var allChecked = true;
			for(var i = 0; i < <%= sites.Count %>; i++)
			{
				var cb = document.getElementById('<%= ClientID %>_sites_' + i);
				if (!cb.checked)
				{
					allChecked = false;
					break;
				}
			}
			
			var allCB = document.getElementById('<%= ClientID %>_sites_all');
			if (allCB.checked != allChecked)
				allCB.checked = allChecked;
		}
		function Search_ToggleAllSites(checked)
		{
			for(var i = 0; i < <%= sites.Count %>; i++)
			{
				var cb = document.getElementById('<%= ClientID %>_sites_' + i);
				cb.checked = checked;
			}
		}
		function Search_ToggleIndexer(checkbox, toggleRow)
		{
			document.getElementById('<%= ClientID %>_indexers_all').checked = false;
			
			if (toggleRow)
			{
				var num = checkbox.id.slice(<%= ClientID.Length + "_indexers_".Length %>);
				var id = '<%= JSEncode(ClientID) %>_indexeroptions_' + num;
				
				jsUtils.ShowTableRow(document.getElementById(id), checkbox.checked)
			}
		}
		function Search_ToggleAllIndexers()
		{
			var checkbox = document.getElementById('<%= ClientID %>_indexers_all');
			if (!checkbox.checked)
				return;
			
			for (var i = 0; i < <%= indexers.Count %>; i++)
			{
				var cb = document.getElementById('<%= ClientID %>_indexers_' + i);//.disabled = checkbox.checked;
				cb.checked = false;
				var row = document.getElementById('<%= ClientID %>_indexeroptions_' + i)
				if (row)
					jsUtils.ShowTableRow(row, false);
			}
		}
	</script>
	</bx:InlineScript>
	<% if (Request["status"] == "finished") { %>
	<bx:BXMessage ID="FinishedMessage" runat="server" Content="<%$ Loc:Message.Finished %>" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" />
	<% } %>
	<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
	<% 
		BXTabControlTab2.Visible = indexerIsRunning || indexersStatus.Count != 0; 
		if (BXTabControl1.SelectedIndex == 1 && !BXTabControlTab2.Visible
			|| BXTabControl1.SelectedIndex == 2 && !OldReindexTab.Visible)
			BXTabControl1.SelectedIndex = 0;
	%>
	<bx:BXTabControl ID="BXTabControl1" runat="server" ButtonsMode="Hidden">
		<bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Text="<%$ LocRaw:TabText.Launch %>" Title="<%$ LocRaw:TabTitle.Launch %>">
			<% if (!indexerIsRunning) IndexingInProcess.Visible = false; %>
			<bx:BXAdminNote runat="server" ID="IndexingInProcess" Width="" >
				<%= GetMessageRaw("Note.IndexingInProcess") %>
			</bx:BXAdminNote>
			
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr class="heading">
					<td colspan="2"><%= GetMessage("Label.Sites") %></td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<td width="60%"
						><input 
							type="checkbox" 
							value="" 
							id="<%= ClientID %>_sites_all" 
							name="<%= UniqueID %>$sites" 
							<% if (allSitesSelected) { %>checked="checked"<% } %> 
							onclick="Search_ToggleAllSites(this.checked)" 
						/><label 
							for="<%= ClientID %>_sites_all"
						><i><%= GetMessage("CheckBoxText.AllSites") %></i></label>
					</td>
				</tr>
				<% int siteCount = 0; %>
				<% foreach (Bitrix.BXSite site in sites) { %>
					<tr valign="top">
						<td class="field-name" width="40%"></td>
						<td width="60%"
							><input 
								type="checkbox" 
								value="<%= site.Id %>" 
								id="<%= ClientID %>_sites_<%= siteCount %>" 
								name="<%= UniqueID %>$sites"
								<% if (selectedSites.Contains(site.TextEncoder.Decode(site.Id)) || allSitesSelected) { %>checked="checked"<% } %>
								onclick="Search_ToggleSite()" 
							/><label 
								for="<%= ClientID %>_sites_<%= siteCount %>"
							><%= site.Name %><% if (!site.Active) { %> <%= GetMessage("NotActiveSuffix") %><% } %></label>
						</td>
					</tr>
					<% siteCount++; %>
				<% } %>
				<tr class="heading">
					<td colspan="2"><%= GetMessage("Heading.IndexingArea") %></td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<td width="60%"
						><input 
							type="checkbox" 
							value="" 
							id="<%= ClientID %>_indexers_all" 
							name="<%= UniqueID %>$indexers" 
							onclick="Search_ToggleAllIndexers()"
							<% if (allIndexersSelected) { %>checked="checked"<% } %>
						><label 
							for="<%= ClientID %>_indexers_all"
						><i><%= GetMessage("CheckBoxText.AllContent") %></i></label>
					</td>
				</tr>
				<asp:Repeater runat="server" ID="IndexerList" OnItemDataBound="IndexerList_ItemDataBound">
				<ItemTemplate>
					<tr valign="top">
						<td class="field-name" width="40%"></td>
						<td width="60%"
							><input 
								type="checkbox" 
								value="<%# Encode(((IndexerInfo)Container.DataItem).Id) %>" 
								id="<%= ClientID %>_indexers_<%# ((IndexerInfo)Container.DataItem).Num %>" 
								name="<%= UniqueID %>$indexers"
								<%# ((IndexerInfo)Container.DataItem).Selected ? "checked=\"checked\"" : "" %>
								onclick="Search_ToggleIndexer(this, <%# ((IndexerInfo)Container.DataItem).OptionsControl != null ? "true" : "false" %>);" 
							/><label 
								for="<%= ClientID %>_indexers_<%# ((IndexerInfo)Container.DataItem).Num %>"
							><%# Encode(((IndexerInfo)Container.DataItem).Title) %></label>
						</td>
					</tr>
					<asp:PlaceHolder runat="server" id="OptionsRow" Visible="<%# ((IndexerInfo)Container.DataItem).OptionsControl != null %>" >
					<tr valign="top" id="<%= ClientID %>_indexeroptions_<%# ((IndexerInfo)Container.DataItem).Num %>" <%# !((IndexerInfo)Container.DataItem).Selected ? "style=\"display:none\"" : "" %> >
						<td class="field-name" width="40%"></td>
						<td width="60%"><asp:PlaceHolder runat="server" ID="OptionsContainer" /></td>
					</tr>
					</asp:PlaceHolder>
				</ItemTemplate>
				</asp:Repeater>
				<tr class="heading">
					<td colspan="2"><%= GetMessage("Heading.Options") %></td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<td width="60%"><asp:CheckBox runat="server" ID="Update" Text="<%$ Loc:CheckBoxText.UpdateOnly %>" /></td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<% if (!IsPostBack) ResumeOnRestart.Checked = true; %>
					<td width="60%"><asp:CheckBox runat="server" ID="ResumeOnRestart" Text="<%$ Loc:CheckBoxText.AutoResume %>" /></td>
				</tr>
			</table>
			<bx:InlineScript runat="server" ID="InitIndexerState" SyncMode="Startup" AsyncMode="Startup" >
			<script type="text/javascript">
				Search_ToggleAllIndexers();
			</script>
			</bx:InlineScript>
			
			<div style="margin-top:20px">
				<% 
					Launch.Text = indexerIsRunning ? GetMessageRaw("ButtonText.StopAndLaunch") : GetMessageRaw("ButtonText.Launch"); 
					Launch.OnClientClick = string.Format("return confirm('{0}');", JSEncode(GetMessageRaw("Confirmation.Launch")));
				%>
				<center><asp:Button runat="server" ID="Launch" OnClick="Launch_Click" /></center>
			</div>
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="BXTabControlTab2" runat="server" Text="<%$ LocRaw:TabText.Main %>" Title="<%$ LocRaw:TabTitle.Main %>">
		<asp:UpdatePanel runat="server" ID="Status" UpdateMode="Conditional">
		<ContentTemplate>
		<bx:BXAdminNote runat="server">
			<table cellpadding="0" cellspacing="0" style="width:100%; border: 0; margin-top:5px; margin-bottom:10px;">
			<tr>
			<td>
				&nbsp;
				<% if (indexerIsRunning) { %>
				<b><%= GetMessageRaw("Status.Running") %></b>
				<% } else if (indexersStatus.Count != 0) { %>
				<b><%= GetMessageRaw("Status.Suspended") %></b>
				<% } else { %>
				<%= GetMessageRaw("Status.Stopped") %>
				<% } %>
				&nbsp;
				<% if (indexerIsRunning) { %>
				<asp:Button runat="server" ID="Button1" OnClick="Pause_Click" Text="<%$ LocRaw:ButtonText.Suspend %>" /> 
				<% } else if (indexersStatus.Count != 0 ) { %>
				<asp:Button runat="server" ID="Button2" OnClick="Resume_Click" Text="<%$ LocRaw:ButtonText.Resume %>" /> 
				<% } %>	
			</td>
			</tr>
			</table>
					
		<% bool first = true; %>
		<% foreach(BXSearchIndexerStatus s in indexersStatus) { %>
		<% 
			IndexerInfo i = indexers.Find(delegate(IndexerInfo input) { return input.Id == s.Id; }); 
			int percent = (s.Progress != null) ? Math.Max(Math.Min((int)s.Progress, 100), 0) : -1; 
		%>
			<% if (true || !first) { %>
			<div style="border-top: dashed 1px #CCC; margin: 2px 0px; padding-bottom: 2px" ></div>
			<% 
				} 
				else 
					first = false; 
			%>
			<div style="overflow: hidden;">
				<% if (!string.IsNullOrEmpty(i.IconUrl)) { %>
				<div style="float:left; width:50px; height: 50px;">
					<table cellspacing="0" cellpadding="0" style="border:0; width:100%; height: 100%">
					<tr><td style="text-align:center;"><img alt="Indexer Icon" src="<%= Encode(i.IconUrl) %>" /></td></tr>
					</table>
				</div>
				<% } %>
				<div style="float:right; width:300px; height: 50px">
					<table cellpadding="0" cellspacing="0" style="border: 0; width:100%; height: 100%">
					<tr>
						<td>
							<% if  (percent != -1) { %>
							<div style="border:1px solid #2C8B1B">
								<div style="height:10px; width:<%= percent %>%; background-color:#2C8B1B"></div>
							</div>
							<% } else { %>
							&nbsp;
							<% } %>
						</td>
						<td style="text-align:center; width: 50px"><% if  (percent != -1) { %><%= percent %> %<% } else { %>&nbsp;<% } %></td>
					</tr>
					</table> 
				</div>
				<div>
					<div style="margin:0 100px 0 <%= !string.IsNullOrEmpty(i.IconUrl) ? "55px" : "0" %>; padding: 5px">
						<div style="font-weight: bold"><%= (i != null) ? Encode(i.Title) : Encode(s.Id) %></div>
						<p><%= s.HtmlMessage %></p>
					</div>
				</div>
			</div>
		<% } %>
	
		
		</bx:BXAdminNote>
		
		</ContentTemplate>
		</asp:UpdatePanel>
		<asp:Timer runat="server" ID="StatusTimer" OnTick="StatusTimer_Tick" Interval="2500" />
		</bx:BXTabControlTab>
		<bx:BXTabControlTab runat="server" ID="OldReindexTab" Text="<%$ LocRaw:TabText.OldModules %>" Title="<%$ LocRaw:TabTitle.OldModules %>" Visible="false" >
			<bx:BXAdminNote runat="server" ID="IndexerStateNote" Width="" >
				<%= GetMessageRaw("Note.OldModules") %>
			</bx:BXAdminNote>
			<bx:IndexerState runat="server" ID="IndexerState" />
		</bx:BXTabControlTab>
	</bx:BXTabControl>
	<bx:BXAdminNote runat="server"><%= GetMessageRaw("Note.BackgroundReindexing") %></bx:BXAdminNote>
</asp:Content>
