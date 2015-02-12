<%@ Control Language="C#" AutoEventWireup="false" CodeFile="AdminFileField.ascx.cs" Inherits="bitrix_ui_AdminFileField" %>

<% 
if (Editable )
{
	%><div runat="server" ID="Lbl"><%= GetMessage(LabelTextMessage)%></div>
	<% if (Behaviour == BehaviourType.Standard) { %>
	<asp:FileUpload ID="Up" runat="server" EnableViewState="False" />
	<%} %>
	<span id="S" runat="server" enableviewstate="false">
		<br /><%= GetMessage("LegendText.Description") %>
		<br /><asp:TextBox ID="Desc" runat="server" />
	</span>

	<%
}
%>
    <div id="D" runat="server" visible="false" enableviewstate="false">

<%
if (Editable)
{
	%><asp:Literal ID="FileParameters" runat="server" /><%
} 
%>

    <div id="ImgContainer">
	    <a id="Img" runat="server" title="<%$ Loc:AnchorTitle.Img %>" target="_blank"><asp:Image ID="ImageDisplay" runat="server" AlternateText="<%$ Loc:ImageAlternateText.ImageDisplay %>" /><br /></a>
	</div>
	<div ID="SwfContainer" runat="server"></div>
    <input type="hidden" id="<%=ClientID%>_slheight" name="<%=ClientID%>_slheight" />
	<input type="hidden" id="<%=ClientID%>_slwidth" name="<%=ClientID%>_slwidth" />

<%
if (Behaviour == BehaviourType.Silverlight){
 %>	
    <table id="<% =ClientID + "_Loading" %>" border="0" cellpadding="0" cellspacing="0" style="display: none; visibility: hidden" >
    <tr>
	    <td>
		    <img src="<% =Bitrix.UI.BXThemeHelper.AddAbsoluteThemePath("images/wait.gif")%>" alt="" />
	    </td>
	    <td>&nbsp;</td>
	    <td><% =GetMessage("Loading") %></td>
	    <td>&nbsp;</td>
	    <td id="<% =ClientID + "_ButtonPlaceholder" %>"></td>
    </tr>
    </table>
    <span id="SavedName" runat="server" />
    <input type="hidden" id="<%=ClientID%>_hfFilePath" name="<%=ClientID%>_hfFilePath" />
    <input type="hidden" id="<%=ClientID%>_hfFileContentType" name="<%=ClientID%>_hfFileContentType" />
    <input type="hidden" id="<%=ClientID %>_csrfToken" value="<%=Bitrix.Security.BXCsrfToken.GenerateToken() %>"/>

    <span id="<% =ClientID + "_ValuePlaceholder" %>" style="<% =(FileId > 0 ? "display:none":"") %>">
    <input id="<% =ClientID + "_ValueUpload" %>" name="<% =ClientID + "_ValueUpload" %>" type="file" 
	onchange="SilverlightFileUploadHandler_UploadFile(this, '<% =ClientID  %>','<% =Del.ClientID  %>');" />
    <input type="hidden" id="<%=ClientID%>_hfFileName" name="<%=ClientID%>_hfFileName" />
    </span>
    <font color="red"><pre id="<% =ClientID%>_UploadErrorMessage" style="display:none;font-family:tahoma;" ></pre></font>
    <input id="<% =ClientID + "_ValueClear" %>"  style="<% =(FileId > 0 ? "" : "display:none")%>" name="<% =ClientID + "_ValueClear" %>" type="button"
	    onclick="SilverlightFileUploadHandler_ClearFile(this, '<% =ClientID  %>','<% =Del.ClientID  %>');" value="<% =GetMessage("ButtonCaption.Clear") %>"
    />
<%
}
 %>

	
<% 
    if (Editable)
    {

	%><asp:CheckBox ID="Del" runat="server" Text="<%$ Loc:CheckBoxText.Del %>" /><%
    }
%>
</div>