<%@ Control Language="C#" AutoEventWireup="false" CodeFile="PublicPanel.ascx.cs" Inherits="bitrix_kernel_PublicPanel" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<script type="text/javascript">
    if (typeof (dotNetVars) == 'undefined') dotNetVars = {};
    dotNetVars.messPanelFixOn = '<%= Bitrix.Services.Js.BXJSUtility.Encode(GetMessageRaw("FixOn")) %>';
    dotNetVars.messPanelFixOff = '<%= Bitrix.Services.Js.BXJSUtility.Encode(GetMessageRaw("FixOff")) %>';
</script>

<%
	BXProfileValue v = BXProfileManager.GetOption("admin_panel", "settings", null);
	string val;
	bool collapsed = 
		v != null 
		&& v.TryGetValue("collapsed", out val) 
		&& string.Equals(val, "on", StringComparison.OrdinalIgnoreCase);
%>
<div id="bx_top_panel_back" style="overflow: hidden; display: none; height: <%= collapsed ? 7 : 66 %>px;"> </div>
<div id="bx_top_panel_container" class="bx-top-panel" style="left: 0px; top: 0px; z-index: 1000;">
    <div style="display: <%= collapsed ? "none" : "block" %>;" id="bx_top_panel_splitter">
        <div class="bx-panel-empty">
        </div>
        <table cellspacing="0" cellpadding="0" class="bx-panel-container">
            <tr>
                <td class="bx-button-cell">
                    <div runat="server" id="dvStart" class="bx-start-button"></div>
                </td>
                <td class="bx-tabs-cell">
                    <%--BXShowMode.View--%>  
                    
	                <%  
                       
	                	if (Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.View)
                        {
                            %><a title="<%= GetMessage("Kernel.TopPanel.PublicAreaTitle") %>" href="<%= BuildTabUrl(Bitrix.Configuration.BXShowMode.View) %>" ><%
                        }
							%>					
							<div class="bx-panel-tab <%= Bitrix.Configuration.BXConfigurationUtility.ShowMode == Bitrix.Configuration.BXShowMode.View  ? " bx-panel-tab-active" : "" %>" >
	                        <div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.View ? "bx-tab-left bx-tab-left-first" : "bx-tab-left bx-tab-left-first bx-tab-left-active" %>">
	                            <div class="bx-tab-icon bx-tab-icon-view"></div>
	                        </div>
	                        <div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.View ? "bx-tab" : "bx-tab bx-tab-active" %>">
	                            <div class="bx-panel-caption"><%= GetMessage("Kernel.TopPanel.PublicArea") %></div>
	                        </div>
	                        <div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.View ? (Bitrix.Configuration.BXConfigurationUtility.ShowMode == Bitrix.Configuration.BXShowMode.Editor ? "bx-tab-right bx-tab-right-next-active" : "bx-tab-right") : "bx-tab-right bx-tab-right-active" %> "></div>
	                        <br/>
	                        <div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.View ? "bx-tab-bottom bx-tab-bottom-first" : "bx-tab-bottom bx-tab-bottom-active bx-tab-bottom-first-active" %>" ></div>
	                        </div>
	                <% 
	                    if (Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.View)
						{
							%></a><%
						}
		            %>
		            
		            <%--BXShowMode.Edit--%>  
	                <%  
						if (Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Editor)
						{
                            %>
                            <a href="<%= BuildTabUrl(Bitrix.Configuration.BXShowMode.Editor) %>" title="<%= GetMessage("Kernel.TopPanel.ContentTitle") %>">
                            <div class="bx-panel-tab">
                            <%
						}
						else
						{
							%>
							<div class="bx-panel-tab bx-panel-tab-active">
							<%
						}
	                %>
								<div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Editor ? Bitrix.Configuration.BXConfigurationUtility.ShowMode == Bitrix.Configuration.BXShowMode.View ? "bx-tab-left bx-tab-left-prev-active" : "bx-tab-left" : "bx-tab-left bx-tab-left-active"%>">
									<div class="bx-tab-icon bx-tab-icon-edit"></div>
								</div>
								<div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Editor ? "bx-tab" : "bx-tab bx-tab-active" %>">
									<div class="bx-panel-caption"><% =GetMessage("Kernel.TopPanel.Content") %></div>
								</div>
								<div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Editor ? Bitrix.Configuration.BXConfigurationUtility.ShowMode == Bitrix.Configuration.BXShowMode.Configurator ? "bx-tab-right bx-tab-right-next-active" : "bx-tab-right" : "bx-tab-right bx-tab-right-active" %>"></div>
								<br/>
								<div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Editor  ? Bitrix.Configuration.BXConfigurationUtility.ShowMode == Bitrix.Configuration.BXShowMode.View ? "bx-tab-bottom bx-tab-bottom-prev-active" : "bx-tab-bottom" : "bx-tab-bottom bx-tab-bottom-active"%>"></div>
							</div>    
	                <%
						if (Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Editor)
						{
							%></a><%
						}
		            %>		            
		            <%--BXShowMode.Configurator--%>  
	                <%  
						if (Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Configurator)
						{
							%>
                            <a href="<%= BuildTabUrl(Bitrix.Configuration.BXShowMode.Configurator) %>" title="<%= GetMessage("Kernel.TopPanel.DesignTitle") %>" >
                            <div class="bx-panel-tab">
                            <%
						}
						else
						{
							%>
							<div class="bx-panel-tab bx-panel-tab-active">
							<%
						}
	                %>
								<div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Configurator ? (Bitrix.Configuration.BXConfigurationUtility.ShowMode == Bitrix.Configuration.BXShowMode.Editor ? "bx-tab-left bx-tab-left-prev-active" : "bx-tab-left") : "bx-tab-left bx-tab-left-active"  %>">
									<div class="bx-tab-icon bx-tab-icon-configure"></div>
								</div>
								<div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Configurator ? "bx-tab" : "bx-tab bx-tab-active" %>">
									<div class="bx-panel-caption"><% =GetMessage("Kernel.TopPanel.Design") %></div>
								</div>
								<div class="<%=  Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Configurator ? "bx-tab-right" : "bx-tab-right bx-tab-right-active"%>"></div>
								<br/>
								<div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Configurator ? (Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Editor ? "bx-tab-bottom bx-tab-bottom-prev-active" : "bx-tab-bottom") : "bx-tab-bottom bx-tab-bottom-active" %>" ></div>
							</div>
					<%		
						if (Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Configurator)
						{
							%></a><%
						} 
		            %>	
		            <%--Control Panel --%>		
                    <a title="<% =GetMessage("Kernel.TopPanel.ControlPanelTitle") %>" href="<%= BuildAdminUrl() %>">
                        <div class="bx-panel-tab bx-panel-tab-admin">
	                        <div class="<%= Bitrix.Configuration.BXConfigurationUtility.ShowMode != Bitrix.Configuration.BXShowMode.Configurator ? "bx-tab-left" : "bx-tab-left bx-tab-left-prev-active" %>">
	                            <div class="bx-tab-icon bx-tab-icon-admin"></div>
	                        </div>
	                        <div class="bx-tab">
	                            <div class="bx-panel-caption"><% =GetMessage("Kernel.TopPanel.ControlPanel") %></div>
	                        </div>
	                        <div class="bx-tab-right"></div>
	                        <br/>
	                        <div class="bx-tab-bottom"></div>
                        </div>
                    </a>
	                <a id="admin_panel_fix_link" class="fix-link fix-on" href="javascript:jsPanel.FixPanel();" ></a>
                    <br clear="all"/>
                    <%--buttons panel --%>   
                    <table cellspacing="0" cellpadding="0" border="0" style="width:100% !important;">
                        <tbody>
                            <tr>
                                <td style="vertical-align:top !important; width:5px ! important;"><div class="bx-panel-left"></div></td>
                                <td>
                                    <div id="panelButtonsContainer" class="bx-panel-buttons">
<%--                                        <asp:PlaceHolder ID="panelButtonsEditPlaceHolder" runat="server">
                                        </asp:PlaceHolder>
                                        <div class="bx-pnseparator"></div>
                                       <asp:PlaceHolder ID="panelButtonsUserPlaceHolder" runat="server">
                                        </asp:PlaceHolder> --%> 
                                        <asp:PlaceHolder ID="panelButtonsPlaceHolder" runat="server">
                                        </asp:PlaceHolder>                                      
                                        <br clear="all"/>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </td>
            </tr>
	    </table>
    </div>
    <table cellspacing="0" class="splitter">
	    <tbody>
	        <tr>
		        <td>
		            <a title="<%= GetMessage("LinkTitle.HidePanel") %>" class="splitterknob<%= collapsed ? " splitterknobdown" : "" %>" onclick="jsPanel.DisplayPanel(this);" href="javascript:void(0);"></a>
		        </td>
	        </tr>
        </tbody>
    </table>
</div>