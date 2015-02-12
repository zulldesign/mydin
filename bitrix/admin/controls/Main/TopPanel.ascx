<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TopPanel.ascx.cs" Inherits="bitrix_kernel_TopPanel" %>
<%@ Import Namespace="Bitrix.Services" %>

<div style="display:none; overflow:hidden;" id="bx_top_panel_back"></div>
<div class="top-panel" id="bx_top_panel_container">
    <div class="empty top"></div>
    <table cellspacing="0" cellpadding="0" class="panel-container">
	    <tr valign="top" >
		    <td width="0%" class="start-button">
		        <div runat="server" id="dvStart" class="start-button"></div>
		    </td>
		    <td width="100%">
                <a runat="server" id="topPanelLinkPublic" href="" title="" >
                    <div class="panel-tab">
	                    <div class="tab-left tab-left-first"><div class="tab-icon tab-icon-view"></div>
	                </div>
	                <div class="tab">
	                    <div class="caption"><% =GetMessage("Kernel.TopPanel.PublicArea") %></div>
	                </div>
	                <div class="tab-right"></div>
	                <br />
	                <div class="tab-bottom tab-bottom-first"></div>
                    </div>
                </a>
                <a runat="server" id="topPanelLinkContent" href="" title="">
                    <div class="panel-tab">
	                    <div class="tab-left">
	                        <div class="tab-icon tab-icon-edit"></div>
	                    </div>
	                    <div class="tab">
	                        <div class="caption"><% =GetMessage("Kernel.TopPanel.Content") %></div>
	                    </div>
	                <div class="tab-right"></div>
	                <br />
	                <div class="tab-bottom"></div>
                    </div>
                </a>
                <a runat="server" id="topPanelLinkDesign" href="" title="">
                    <div class="panel-tab">
	                    <div class="tab-left">
	                        <div class="tab-icon tab-icon-configure"></div>
	                    </div>
	                    <div class="tab">
	                        <div class="caption"><% =GetMessage("Kernel.TopPanel.Design") %></div>
	                    </div>
	                    <div class="tab-right tab-right-next-active"></div>
	                    <br />
	                    <div class="tab-bottom"></div>
                    </div>
                </a>
                <a runat="server" id="topPanelLinkAdmin" href= "" title="">
                    <div class="panel-tab panel-tab-admin">
	                    <div class="tab-left">
	                        <div class="tab-icon tab-icon-admin"></div>
	                    </div>
	                    <div class="tab">
	                        <div class="caption"><% =GetMessage("Kernel.TopPanel.ControlPanel") %></div>
	                    </div>
	                    <div class="tab-right"></div>
	                    <br />
	                    <div class="tab-bottom tab-bottom-active"></div>
                    </div>
                </a>
	            <a id="admin_panel_fix_link" class="fix-link fix-on" href="javascript:jsPanel.FixPanel();"></a>
	            <br clear="all" />
                <div class="toppanel">
	                <table id="topPanelButtons" runat="server" border="0" cellspacing="0" cellpadding="0">                   
	                </table>
	            </div>
            </td>
        </tr>
    </table>
</div>
