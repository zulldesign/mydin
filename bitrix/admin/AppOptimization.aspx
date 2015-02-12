<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false" 
CodeFile="AppOptimization.aspx.cs" Inherits="bitrix_admin_AppOptimization" Title="<%$ LocRaw:PageTitle %>"  EnableViewState="false" %>

<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <asp:UpdatePanel ID="LangUpdatePanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
            <bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="AppOptimization" />
            <bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="OnTabControlCommand" >
                <bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Title="<%$ Loc:TabTitle.Main %>" Text="<%$ Loc:TabText.Main %>"> 
                    <table cellpadding="0" cellspacing="0" border="0" class="edit-table" id="fedit1_edit_table" >
                        <tbody>
                            <tr class="heading">
				                <td colspan="2"><%= GetMessage("Header.DynamicCompilationSettings")%></td>
				            </tr>
				            <tr>
                                <td width="60%" class="field-name"><%= GetMessage("FieldName.CompilationEnableDebug")%>: <span id="compilationEnableDebugLegendCont"></span></td>
                                <td>
                                    <asp:CheckBox runat="server" ID="CompilationEnableDebug" Text="" />
                                </td>
				            </tr>				            
				            <tr>
                                <td width="60%" class="field-name"><%= GetMessage("FieldName.CompilationEnableBatch")%>: <span id="compilationEnableBatchLegendCont"></span></td>
                                <td>
                                    <asp:CheckBox runat="server" ID="CompilationEnableBatch" Text="" />
                                </td>
				            </tr>				            
				            <% if (HasCompilationAssemblyReferencences) {%>
				            <tr>
				                <td colspan="2">
				                    <bx:BXAdminNote runat="server"><%= GetMessage("Intro.DynamicCompilationSettings")%></bx:BXAdminNote>
				                </td>
				            </tr>
				            <tr>
                                <td width="60%" class="field-name"><%= GetMessage("Assembly.EnterpriseServices")%>: </td>
                                <td>
                                    <asp:CheckBox runat="server" ID="EnableEnterpriseServices" Text="" />
                                </td>
				            </tr>
				            <tr>
				                <td width="60%" class="field-name"><%= GetMessage("Assembly.ServiceModel")%>: </td>
				                <td>
				                    <asp:CheckBox runat="server" ID="EnableServiceModel" Text="" />
				                </td>
				            </tr>
				            <tr>
				                <td width="60%" class="field-name"><%= GetMessage("Assembly.ServiceModelWeb")%>: </td>
				                <td>
				                    <asp:CheckBox runat="server" ID="EnableServiceModelWeb" Text="" />
				                </td>
				            </tr>
				            <tr>
				                <td width="60%" class="field-name"><%= GetMessage("Assembly.IdentityModel")%>: </td>
				                <td>
				                    <asp:CheckBox runat="server" ID="EnableIdentityModel" Text="" />
				                </td>
				            </tr>
				            <tr>
				                <td width="60%" class="field-name"><%= GetMessage("Assembly.WebMobile")%>: </td>
				                <td>
				                    <asp:CheckBox runat="server" ID="EnableWebMobile" Text="" />
				                </td>				            
				            </tr>
			                <tr>
			                    <td width="60%" class="field-name"><%= GetMessage("Assembly.WorkflowServices")%>: </td>
			                    <td>
			                        <asp:CheckBox runat="server" ID="EnableWorkflowServices" Text="" />
			                    </td>
			                </tr>
				        <%} %>
				            <tr class="heading">
				                <td colspan="2"><%= GetMessage("Header.MemorySettings")%></td>
				            </tr>	                        
	                        <tr>
	                            <td width="60%" class="field-name"><%= GetMessage("FieldName.EnableMemoryCollection") %>: <span id="enableMemoryCollectionLegendCont"></span></td>
	                            <td>
					                <asp:CheckBox runat="server" ID="EnableMemoryCollection" />	                                
	                            </td>	                        
                            </tr>
                            <tr id="privateBytesLimitRow" >
                                <td width="60%" class="field-name"><%= GetMessage("FieldName.PrivateBytesLimit")%>: <span id="privateBytesLimitLegendCont"></span></td>
                                <td>
                                    <asp:TextBox runat="server" ID="PrivateBytesLimit" TextMode="SingleLine" />
                                    <asp:RegularExpressionValidator ID="PrivateBytesLimitValidator" runat="server" ValidationGroup="AppOptimization"  ControlToValidate="PrivateBytesLimit" ValidationExpression="^\d+$" ErrorMessage="<%$ Loc:Message.PrivateBytesLimitIsNotNumber %>" Display="Dynamic">*</asp:RegularExpressionValidator>
                                </td>
                            </tr>
                            <tr id="privateBytesPollTimeRow" >
                                <td width="60%" class="field-name"><%= GetMessage("FieldName.PrivateBytesPollTime")%>: <span id="privateBytesPollTimeLegendCont"></span></td>
                                <td>
                                    <asp:TextBox runat="server" ID="PrivateBytesPollTime" TextMode="SingleLine" />
                                    <asp:RegularExpressionValidator ID="PrivateBytesPollTimeValidator" runat="server" ValidationGroup="AppOptimization"  ControlToValidate="PrivateBytesPollTime" ValidationExpression="^\d+$" ErrorMessage="<%$ Loc:Message.PrivateBytesPollTimeIsNotNumber %>" Display="Dynamic">*</asp:RegularExpressionValidator>                                    
                                </td>
                            </tr>				        	
				            <tr>
				                <td colspan="2">
				                    <bx:BXAdminNote runat="server"><%= GetMessageRaw("Intro.BackupRecommendation")%></bx:BXAdminNote>
				                </td>
				            </tr>				        		                                                                                                                       			            	                                                        
                        </tbody>
                    </table>                
                </bx:BXTabControlTab>
                <bx:BXTabControlTab ID="ComponentCachingTab" runat="server" Title="<%$ Loc:TabTitle.ComponentCaching %>" Text="<%$ Loc:TabText.ComponentCaching %>"> 
                    <table cellpadding="0" cellspacing="0" border="0" class="edit-table" id="componentCachingTab">
						<tbody>
							<tr>
								<td align="left" valign="top">
									<span style="color:<%= IsComponentAutoCachingEnabled ? "green":"red" %>"><b>
										<%=  GetMessage(IsComponentAutoCachingEnabled ? "MessageText.ComponentAutoCachingEnabled" : "MessageText.ComponentAutoCachingNotEnabled") %></b>
									</span>
								</td>
							</tr>
							<tr>
								<td align="left" valign="top">
									<asp:Button ID="btnSwitchComponentAutoCaching" runat="server" OnCommand="SwitchComponentAutoCaching"></asp:Button>
								</td>
							</tr>
							<tr>
								<td>
									<div class="notes">
										<table cellspacing="0" cellpadding="0" border="0" class="notes">
											<tbody>
												<tr class="top">									
													<td class="left"><div class="empty"></div></td>
													<td><div class="empty"></div></td>
													<td class="right"><div class="empty"></div></td>												
												</tr>
												<tr>
													<td class="left">
														<div class="empty"></div>
													</td>
													<td class="content">
														<%= GetMessageRaw("Html.ComponentCachingIntro") %>																																								
													</td>
												</tr>
												<tr class="bottom">
													<td class="left"><div class="empty"></div></td>
													<td><div class="empty"></div></td>
													<td class="right"><div class="empty"></div></td>
												</tr>												
											</tbody>
										</table>
									</div>
								</td>
							</tr>														
                        </tbody>
                    </table>                
                </bx:BXTabControlTab>
                <bx:BXTabControlTab ID="ManagedCachingTab" runat="server" Title="<%$ Loc:TabTitle.ManagedCaching %>" Text="<%$ Loc:TabText.ManagedCaching %>"> 
                    <table cellpadding="0" cellspacing="0" border="0" class="edit-table" id="Table1">
						<tbody>
							<tr>
								<td align="left" valign="top">
									<span style="color:<%= IsTagCachingEnabled ? "green":"red" %>"><b>
										<%= GetMessage(IsTagCachingEnabled ? "MessageText.ManagedCachingEnabled" : "MessageText.ManagedCachingNotEnabled") %></b>
									</span>
								</td>
							</tr>
							<tr>
								<td align="left" valign="top">
									<asp:Button ID="btnSwitchManagedCaching" runat="server" OnCommand="SwitchManagedCaching"></asp:Button>
								</td>
							</tr>
							<tr>
								<td>
									<div class="notes">
										<table cellspacing="0" cellpadding="0" border="0" class="notes">
											<tbody>
												<tr class="top">									
													<td class="left"><div class="empty"></div></td>
													<td><div class="empty"></div></td>
													<td class="right"><div class="empty"></div></td>												
												</tr>
												<tr>
													<td class="left">
														<div class="empty"></div>
													</td>
													<td class="content">
														<%= GetMessageRaw("Html.ManagedCachingIntro") %>																																								
													</td>
												</tr>
												<tr class="bottom">
													<td class="left"><div class="empty"></div></td>
													<td><div class="empty"></div></td>
													<td class="right"><div class="empty"></div></td>
												</tr>												
											</tbody>
										</table>									
									</div>
								</td>
							</tr>						
						</tbody>
                    </table>                
                </bx:BXTabControlTab>                
            </bx:BXTabControl>
        </ContentTemplate>
    </asp:UpdatePanel>
    
<script type="text/javascript">
    if (typeof (Bitrix) == "undefined") {
        var Bitrix = new Object();
    }

    Bitrix.AppOptimization = function Bitrix$AppOptimization() {
        this._initialized = false;
        this._enableMemCollectionId = this._privBytesLimitContId = this._privBytesPollTimeContId = null;
    }
    Bitrix.AppOptimization.prototype = {
        initialize: function(privBytesPollTimeContId, privateBytesPollTimeLegendContId) {
            this._enableMemCollectionId = '<%= EnableMemoryCollection.ClientID %>';
            this._privBytesLimitContId = 'privateBytesLimitRow';
            this._privBytesPollTimeContId = 'privateBytesPollTimeRow';

            Bitrix.EventUtility.addEventListener(this._getEnableMemCollection(), "click", Bitrix.TypeUtility.createDelegate(this, this._handleEnableMemCollectionChange));
            this.layoutMemory();
            this.createHint('enableMemoryCollectionLegendCont', Bitrix.AppOptimization.messages.disableMemoryCollectionLegend);
            this.createHint('privateBytesLimitLegendCont', Bitrix.AppOptimization.messages.privBytesLimitLegend);
            this.createHint('privateBytesPollTimeLegendCont', Bitrix.AppOptimization.messages.privBytesPollTimeLegend);
            this.createHint('compilationEnableDebugLegendCont', Bitrix.AppOptimization.messages.compilationEnableDebugLegend);
            this.createHint('compilationEnableBatchLegendCont', Bitrix.AppOptimization.messages.compilationEnableBatchLegend);

            this._initialized = true;
        },
        layoutMemory: function() {
            var c = this._getEnableMemCollection().checked;
            this._getPrivBytesLimitCont().style.display = this._getPrivBytesPollTimeCont().style.display = c ? '' : 'none';
        },
        createHint: function(containerId, html) {
            if (typeof (BXHint) == 'undefined') return;
            var container = document.getElementById(containerId);
            if (!container) return;
            container.appendChild((new BXHint(html)).oIcon);
        },
        _getEnableMemCollection: function() { return document.getElementById(this._enableMemCollectionId); },
        _getPrivBytesLimitCont: function() { return document.getElementById(this._privBytesLimitContId); },
        _getPrivBytesPollTimeCont: function() { return document.getElementById(this._privBytesPollTimeContId); },
        _handleEnableMemCollectionChange: function() { this.layoutMemory(); }
    }
    Bitrix.AppOptimization._instance = null;
    Bitrix.AppOptimization.instantiate = function() {
        if (this._instance == null) {
            this._instance = new Bitrix.AppOptimization();
            this._instance.initialize();
        }
        return this._instance;
    }
    Bitrix.AppOptimization.messages = {
        'disableMemoryCollectionLegend':'<%= GetMessageJS("Legend.EnableMemoryCollection") %>',
        'privBytesLimitLegend':'<%= GetMessageJS("Legend.PrivateBytesLimit") %>',
        'privBytesPollTimeLegend': '<%= GetMessageJS("Legend.PrivateBytesPollTime") %>',
        'compilationEnableDebugLegend': '<%= GetMessageJS("Legend.CompilationEnableDebug") %>',
        'compilationEnableBatchLegend': '<%= GetMessageJS("Legend.CompilationEnableBatch") %>'
    }

    Bitrix.AppOptimization.instantiate();                   
</script>
</asp:Content>

