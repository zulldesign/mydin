<%@ Page Language="C#" AutoEventWireup="false" CodeFile="CopyComponentTemplate.aspx.cs" Inherits="bitrix_dialogs_CopyComponentTemplate" %>
<html>
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <bx:BXPageAsDialogBehaviour runat="server" ID="Behaviour" UseStandardStyles="true" OnSave="Behaviour_Save" />  
        <table cellspacing="0" class="bx-width100"><tbody>
            <tr>
		        <td class="bx-popup-label bx-width50"><%= GetMessage("Legend.CurrentComponentTemplate") %></td>
		        <td><b><%= TemplateName %></b>
		        <% if (TemplateType == ComponentTemplateType.System){%> 
		            &nbsp;/&nbsp;<%= GetMessageRaw("TemplatePostfix.SystemTemplate") %> 
		        <% }%>
		        </td>
	        </tr>
	        <% if (TemplateType == ComponentTemplateType.Site){%> 
	        <tr>
		        <td class="bx-popup-label bx-width50"><%= GetMessage("Legend.UsedInSiteTemplate") %></td>
		        <td><b><%= GetOwnerSiteTemplateName()%></b></td>
	        </tr>	        
	        <% }%>	        
	        <tr>
		        <td class="bx-popup-label bx-width50"><%= GetMessage("Legend.TitleForNewComponentTemplate") %><span style="color:Red;">*</span>:</td>
		        <td>
			        <%--<input runat="server" id="tbxNewTemplateName" type="text" />--%>
			        <asp:TextBox runat="server" ID="tbxNewTemplateName"></asp:TextBox><br/>
		            <asp:RegularExpressionValidator runat="server" ID="vldrNewTemplateName" 
		                Display="Dynamic" Text="" EnableClientScript="true"
                        ControlToValidate="tbxNewTemplateName" 
                        SetFocusOnError="true">
                    </asp:RegularExpressionValidator>			        
			        <asp:RequiredFieldValidator ID="vldrRequiredNewTemplateName" runat="server" 
			            ControlToValidate="tbxNewTemplateName" 
			            Display="Dynamic" Text="" EnableClientScript="true" 
			            SetFocusOnError="true">
                    </asp:RequiredFieldValidator>
		        </td>
	        </tr>
	        <tr>
		        <td valign="top" class="bx-popup-label bx-width50"><%= GetMessage("Legend.CopyToSiteTemplate") %></td>
		        <td>
                    <input runat="server" id="rbtnDefaultSiteTemplate" type="radio" onclick="CheckSiteTemplate(this)" /><label for="rbtnDefaultSiteTemplate"><%= GetMessage("RadioButtonText.ByDefault") %>&nbsp;/&nbsp;<%= DefaultSiteTemplateName %></label><br/>
                    <% if (AboutCopy2ActiveSiteTemplateOptionDisplay){%>
                    <input runat="server" id="rbtnActiveSiteTemplate" type="radio" onclick="CheckSiteTemplate(this)" /><label for="rbtnActiveSiteTemplate"><%= GetMessage("RadioButtonText.Current") %>&nbsp;/&nbsp;<%= GetActiveSiteTemplateName()%></label><br/>
                    <%} %>
                    <% if (SiteTemplates.Count > 0){%>
                    <input runat="server" id="rbtnAnotherSiteTemplate" type="radio" onclick="CheckSiteTemplate(this)" /><label for="rbtnAnotherSiteTemplate"><%= GetMessage("RadioButtonText.Another") %></label>
			        <select runat="server" id="ddlSiteTemplateName" name="ddlSiteTemplateName" ></select>                        
                    <%} %>
		        </td>
	        </tr>
	        <tr>
		        <td class="bx-popup-label bx-width50"><%= GetMessage("Legend.ApplyTheNewComponentTemplate") %></td>
		        <td>
			        <input runat="server" id="chbxApply" type="checkbox" />
		        </td>
	        </tr>
	        <tr>
		        <td class="bx-popup-label bx-width50"><%= GetMessage("Legend.Go2TemplateModification") %></td>
		        <td>
			        <input runat="server" id="chbxGo2Modification" type="checkbox" />
		        </td>
	        </tr>
        </tbody></table>              
    </div>
	<script type="text/javascript" language="javascript">
		window.CheckSiteTemplate = function(rbtn)
		{
			var ddl = document.getElementById('<%= ddlSiteTemplateName.ClientID %>');
			var cb = document.getElementById('<%= chbxApply.ClientID %>');
			
			var enable = (rbtn.id == '<%= rbtnAnotherSiteTemplate.ClientID %>');
			if (ddl)
				ddl.disabled = !enable;
			if (cb)
			{
				cb.disabled = enable;
				cb.checked = !enable;
			}
		}
	</script>
    </form>
</body>
</html>
