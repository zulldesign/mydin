<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="Captcha.aspx.cs" Inherits="bitrix_admin_Captcha" Title="<%$ LocRaw:PageTitle %>" Trace="false" %>
    
<%@ Import Namespace="Bitrix.Services"%>
<%@ Import Namespace="System.Collections.Generic"%>
<%@ Import Namespace="System.Drawing"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <asp:UpdatePanel ID="LangUpdatePanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
			<bx:BXAdminNote runat="server" ID="EngineNote">
				<p><%= GetMessage(BXCaptchaImage.CurrentLibraryType == BXCaptchaImageLibraryType.GD ? "LibInfo.GD" : "LibInfo.GDIPlus") %></p>
			</bx:BXAdminNote>
            <bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
            <bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command" >
                <bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True"
                Title="<%$ Loc:EditTitle %>" Text="<%$ Loc:Parameters %>"> 
                    <table cellpadding="0" cellspacing="0" border="0" class="edit-table" id="fedit1_edit_table" >
                    <tbody>
                       <%--<tr>
                            <td width="70%" class="field-name"><%= GetMessage("Library") + ":"%></td>
                            <td>
                                <asp:DropDownList runat="server" ID="Library" name="Library" DataType="int" CausesValidation="false" AutoPostBack="true">
                                </asp:DropDownList>                            
                            </td>
                       </tr>--%>
	                   <tr>
	                        <td width="70%" class="field-name"><%= GetMessage("Preset") + ":"%></td>
	                        <td>
                                <asp:DropDownList runat="server" ID="Preset" name="Preset" DataType="int">
                                </asp:DropDownList>
	                        </td>
	                        <td width="10%" rowspan="23" align=left>
	                            <% for (int i = 0; i < 10; i++)
                                {%>
	                                <div>
	                                    <img height=50 width=180 id="CaptchaImage_<%=i %>" src="Captcha.aspx?show_captcha=Y&<%=i%>>" alt="<%= GetMessage("Loading") %>" />
	                                </div>
	                            <%} %>
	                        </td> 	                        
                        </tr>
                        <tr>
	                        <td class="field-name"><%= GetMessage("TextLength") %>:</td>
	                        <td>
	                            <asp:TextBox runat="server" ID="TextLength" name="TextLength" Columns="2" DataType="int"></asp:TextBox>
			                </td>
                        </tr>                        
                        <tr>
	                        <td class="field-name">
	                            <%= GetMessage("TextTransparency") + ":"%>
	                        </td>
	                        <td>
	                            <asp:TextBox runat="server" ID="TextTransparency" name="TextTransparency" Columns="5" DataType="int"></asp:TextBox>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%= GetMessage("BackgroundColorFrom") + ":"%>
	                        </td>
	                        <td>
	                            <asp:TextBox runat="server" ID="BackgroundColorFrom" name="BackgroundColorFrom" Columns="7" DataType="color"></asp:TextBox>
			                    <a style="vertical-align:bottom" href="javascript:void(0)" onclick='Bitrix.UI.ColorPicker.Instantiate().Toggle(this, <%=this.BackgroundColorFrom.ClientID %>_OnPickColor);'><img style="border-color:Black;border-width:1px" src="../controls/Main/editor/js/images/htmledit2/bgcolor.gif" /></a>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%= GetMessage("BackgroundColorTo") + ":"%>
	                        </td>
	                        <td>
	                            <asp:TextBox runat="server" ID="BackgroundColorTo" name="BackgroundColorTo" Columns="7" DataType="color"></asp:TextBox>
	                            <a style="vertical-align:bottom" href="javascript:void(0)" onclick='Bitrix.UI.ColorPicker.Instantiate().Toggle(this, <%=this.BackgroundColorTo.ClientID %>_OnPickColor);'><img style="border-color:Black;border-width:1px" src="../controls/Main/editor/js/images/htmledit2/bgcolor.gif" /></a>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%= GetMessage("NumCircles") + ":"%>
	                        </td>
	                        <td>
	                            <asp:TextBox runat="server" ID="NumCircles" name="NumCircles" Columns="5" DataType="int"></asp:TextBox>
			                </td>
                        </tr>                        
                        <tr>
                            <td class="field-name">
                                <%=GetMessage("CircleColorFrom") + ":"%>
                            </td>
                            <td>
                                <asp:TextBox runat="server" ID="CircleColorFrom" name="CircleColorFrom" Columns="7" DataType="color"></asp:TextBox>
                                <a style="vertical-align:bottom" href="javascript:void(0)" onclick='Bitrix.UI.ColorPicker.Instantiate().Toggle(this, <%=this.CircleColorFrom.ClientID %>_OnPickColor);'><img style="border-color:Black;border-width:1px" src="../controls/Main/editor/js/images/htmledit2/bgcolor.gif" /></a>
                            </td>
                        </tr>
                        <tr>
                            <td class="field-name">
                                <%=GetMessage("CircleColorTo") + ":"%>
                            </td>
                            <td>
                                <asp:TextBox runat="server" ID="CircleColorTo" name="CircleColorTo" Columns="7" DataType="color"></asp:TextBox>
                                <a style="vertical-align:bottom" href="javascript:void(0)" onclick='Bitrix.UI.ColorPicker.Instantiate().Toggle(this, <%=this.CircleColorTo.ClientID %>_OnPickColor);'><img style="border-color:Black;border-width:1px" src="../controls/Main/editor/js/images/htmledit2/bgcolor.gif" /></a>
                            </td>
                        </tr>                        
                        <tr>
	                        <td class="field-name">
                               <%=GetMessage("TextUnderLines") + ":"%>
	                        </td>
	                        <td>
					            <asp:CheckBox runat="server" id="TextUnderLines" name="TextUnderLines" DataType="bool"/>
                            </td>
                        </tr>
                        <tr>
                            <td class="field-name">
                                <%=GetMessage("NumLines") + ":"%>
                            </td>
                            <td>
                                <asp:TextBox runat="server" ID="NumLines" name="NumLines" Columns="5" DataType="int"></asp:TextBox>
                            </td>
                        </tr>    
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("LineColorFrom") + ":"%>
	                        </td>
	                        <td>
					            <asp:TextBox runat="server" ID="LineColorFrom" name="LineColorFrom" Columns="7" DataType="color"></asp:TextBox>  
					            <a style="vertical-align:bottom" href="javascript:void(0)" onclick='Bitrix.UI.ColorPicker.Instantiate().Toggle(this, <%=this.LineColorFrom.ClientID %>_OnPickColor);'><img style="border-color:Black;border-width:1px" src="../controls/Main/editor/js/images/htmledit2/bgcolor.gif" /></a>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("LineColorTo") + ":"%>
	                        </td>
	                        <td>
					            <asp:TextBox runat="server" ID="LineColorTo" name="LineColorTo" Columns="7" DataType="color"></asp:TextBox>  
			                    <a style="vertical-align:bottom" href="javascript:void(0)" onclick='Bitrix.UI.ColorPicker.Instantiate().Toggle(this, <%=this.LineColorTo.ClientID %>_OnPickColor);'><img style="border-color:Black;border-width:1px" src="../controls/Main/editor/js/images/htmledit2/bgcolor.gif" /></a>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("LeftOffset") + ":"%>
	                        </td>
	                        <td>
                                <asp:TextBox runat="server" ID="LeftOffset" name="LeftOffset" Columns="5" DataType="int"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("FontSize") + ":"%>
	                        </td>
	                        <td>
	                            <asp:TextBox runat="server" ID="FontSize" name="FontSize" Columns="5" DataType="int"></asp:TextBox>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("FontColorFrom") + ":"%>
	                        </td>
	                        <td>
	                           <asp:TextBox runat="server" ID="FontColorFrom" name="FontColorFrom" Columns="7" DataType="color"></asp:TextBox>
                                <a style="vertical-align:bottom" href="javascript:void(0)" onclick='Bitrix.UI.ColorPicker.Instantiate().Toggle(this, <%=this.FontColorFrom.ClientID %>_OnPickColor);'><img style="border-color:Black;border-width:1px" src="../controls/Main/editor/js/images/htmledit2/bgcolor.gif" /></a>
                            </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("FontColorTo") + ":"%>
	                        </td>
	                        <td>
	                           <asp:TextBox runat="server" ID="FontColorTo" name="FontColorTo" Columns="7" DataType="color"></asp:TextBox>
	                           <a style="vertical-align:bottom" href="javascript:void(0)" onclick='Bitrix.UI.ColorPicker.Instantiate().Toggle(this, <%=this.FontColorTo.ClientID %>_OnPickColor);'><img style="border-color:Black;border-width:1px" src="../controls/Main/editor/js/images/htmledit2/bgcolor.gif" /></a>
                            </td>
                        </tr>                        

                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("VertAngleOfDipFrom") + ":"%>
	                        </td>
	                        <td>
	                           <asp:TextBox runat="server" ID="VertAngleOfDipFrom" name="VertAngleOfDipFrom" Columns="5" DataType="int"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("VertAngleOfDipTo") + ":"%>
	                        </td>
	                        <td>
	                           <asp:TextBox runat="server" ID="VertAngleOfDipTo" name="VertAngleOfDipTo" Columns="5" DataType="int"></asp:TextBox>
                            </td>
                        </tr>                        
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("SymbolDistanceFrom") + ":"%>
	                        </td>
	                        <td>
                                <asp:TextBox runat="server" ID="SymbolDistanceFrom" name="SymbolDistanceFrom" Columns="5" DataType="int"></asp:TextBox>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("SymbolDistanceTo") + ":"%>
	                        </td>
	                        <td>
                                <asp:TextBox runat="server" ID="SymbolDistanceTo" name="SymbolDistanceTo" Columns="5" DataType="int"></asp:TextBox>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
                                <%=GetMessage("NonlinearDistortion") + ":"%>
	                        </td>
	                        <td>
                                <asp:CheckBox runat="server" id="NonlinearDistortion" name="NonlinearDistortion" DataType="bool"/>
                            </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("BorderColor") + ":"%>
	                        </td>
	                        <td>
					            <asp:TextBox runat="server" ID="BorderColor" name="BorderColor" Columns="7" DataType="color"></asp:TextBox>
			                    <a style="vertical-align:bottom" href="javascript:void(0)" onclick='Bitrix.UI.ColorPicker.Instantiate().Toggle(this, <%=this.BorderColor.ClientID %>_OnPickColor);'><img style="border-color:Black;border-width:1px" src="../controls/Main/editor/js/images/htmledit2/bgcolor.gif" /></a>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("FontWhitelist") + ":"%>
	                        </td>
	                        <td>
                                    <asp:ListBox SelectionMode="Multiple" runat="server" ID="FontWhitelist" name="FontWhitelist" DataType="string_array">
                                    </asp:ListBox>
			                </td>
                        </tr>
                        <tr>
	                        <td class="field-name">
		                        <%=GetMessage("RandomTextChars") + ":"%>
	                        </td>
	                        <td>
					            <asp:TextBox name="RandomTextChars" ID="RandomTextChars" Columns="35" runat="server" DataType="string"></asp:TextBox>
			                </td>
                        </tr>

                    </tbody>
                    </table>    
           </bx:BXTabControlTab>
           </bx:BXTabControl>
			           
        </ContentTemplate>
    </asp:UpdatePanel>
<script language=javascript>
if(typeof(Bitrix) == "undefined"){
	var Bitrix = {};
}

Bitrix.CaptchaPresets = <%= GetCaptchaPresetJson() %>;

<% foreach (WebControl c in BXTabControlTab1.Controls) {
    if (c.Attributes["DataType"] != null && c.Attributes["DataType"]=="color")
    {
    %>
function <%=c.ClientID %>_OnPickColor(sender, color)
{
    document.getElementById('<%=c.ClientID %>').value = color; 
}
    <%}
} %>

<% foreach(WebControl wc in BXTabControlTab1.Controls) { %>
	<%
        string cvalue = "";
        switch (wc.Attributes["DataType"])
	    {
		    case "int":
                cvalue = "";
                OptionValue<int, string> a = (OptionValue<int, string>)CaptchaOptions.GetValue<OptionValue<int, string>>(wc.ID);
                cvalue = a.FrontValue;
			    break;
		    case "string":
                cvalue = "";
                OptionValue<string, string> b = (OptionValue<string, string>)CaptchaOptions.GetValue<OptionValue<string, string>>(wc.ID);
			    cvalue = b.FrontValue;
			    break;
		    case "color":
                cvalue = "";
                OptionValue<Color, string> c = (OptionValue<Color, string>)CaptchaOptions.GetValue<OptionValue<Color, string>>(wc.ID);
			    cvalue = c.FrontValue;
			    break;
		    case "bool":
                cvalue = "";
                OptionValue<bool, string> d = (OptionValue<bool, string>)CaptchaOptions.GetValue<OptionValue<bool, string>>(wc.ID);
			    cvalue = d.FrontValue;
			    break;
		    case "string_array":
                OptionValue<string[], string> e = (OptionValue<string[], string>)CaptchaOptions.GetValue<OptionValue<string[], string>>(wc.ID);
			    cvalue = e.FrontValue;
			    break;
	    }
	    %>
        var <%=wc.ID%>  = '<%=cvalue%>';
<% } %>

var j = 0;
var preset_selected = false;

function CheckForChanges()
{
	var changed = false;
	var url = 'Captcha.aspx?show_captcha=Y';
	var ctl, b;
	
<% foreach(WebControl wc in BXTabControlTab1.Controls)  {%>
    <% if (wc.ID=="Preset") continue; %>
	ctl = document.getElementById('<%=wc.ClientID%>');
	<% if (wc.GetType().ToString()=="System.Web.UI.WebControls.CheckBox") { %>
		if(ctl.checked)
				b = 'Y';
			else
				b = 'N';
	<% } else if (wc.GetType().ToString()=="System.Web.UI.WebControls.ListBox") { %>
		b = '';
		for(var i = 0; i < ctl.length; i++)
		{
			if(ctl[i].selected)
			{
				if(b.length)
					b += ';' + ctl[i].value;
				else
					b += ctl[i].value;
			}
		} 
	<% } else if (wc.GetType().ToString()=="System.Web.UI.WebControls.DropDownList") { %>
	    b = '';
        var myindex  = ctl.selectedIndex;
        b = ctl.options[myindex].value
	    
	<% } else { %>
		b = ctl.value;
	<% } %>
	if(b != <%=wc.ID%>)
	{
		changed = true;	
	}
	<%=wc.ID%> = b;
	url += '&<%=wc.ID%>='+encodeURIComponent(<%=wc.ID%>);
<% } %>	
	if(changed)
	{
		j++;
		for(var i = 0;i < 10; i++)
		{
			var img = document.getElementById('CaptchaImage_'+i);
			img.src = url + '&i=' + i + '&j=' + j;
		}
		if(!preset_selected)
			document.getElementById('<%=this.Preset.ClientID%>').value = 'UserDefinedPreset';
		else
			preset_selected = false;
	}
	setTimeout('CheckForChanges()', 2000);
}

setTimeout('CheckForChanges()', 3000);

function set_presets() {
    preset_selected = true;
    
    var presets = Bitrix.CaptchaPresets;
    if(!presets) return;
    var presetName = document.getElementById('<%=this.Preset.ClientID %>').value,
        preset = null;
    for(var i = 0; i < presets.length; i++) {
        if(presets[i].name != presetName) continue;
        preset = presets[i];
        break;
    }
    
    if(!preset) return;
   	
	document.getElementById('<%=this.TextTransparency.ClientID %>').value = preset.textTransparency;
	document.getElementById('<%=this.BackgroundColorFrom.ClientID %>').value = preset.backgroundColorFrom;
	document.getElementById('<%=this.BackgroundColorTo.ClientID %>').value = preset.backgroundColorTo;
	document.getElementById('<%=this.NumCircles.ClientID %>').value = preset.numEllipses;
	document.getElementById('<%=this.CircleColorFrom.ClientID %>').value = preset.ellipsesColorFrom;
	document.getElementById('<%=this.CircleColorTo.ClientID %>').value = preset.ellipsesColorTo;
	document.getElementById('<%=this.TextUnderLines.ClientID %>').checked = preset.textUnderLines;
	document.getElementById('<%=this.NumLines.ClientID %>').value = preset.numLines;
	document.getElementById('<%=this.LineColorFrom.ClientID %>').value = preset.lineColorFrom;
	document.getElementById('<%=this.LineColorTo.ClientID %>').value = preset.lineColorTo;
	document.getElementById('<%=this.LeftOffset.ClientID %>').value = preset.leftOffset;
	document.getElementById('<%=this.FontSize.ClientID %>').value = preset.fontSize;
	document.getElementById('<%=this.FontColorFrom.ClientID %>').value = preset.fontColorFrom;
	document.getElementById('<%=this.FontColorTo.ClientID %>').value = preset.fontColorTo;
	document.getElementById('<%=this.VertAngleOfDipFrom.ClientID %>').value = preset.vertAngleOfDipFrom;
	document.getElementById('<%=this.VertAngleOfDipTo.ClientID %>').value = preset.vertAngleOfDipTo;
	document.getElementById('<%=this.SymbolDistanceFrom.ClientID %>').value = preset.symbolDistanceFrom;
	document.getElementById('<%=this.SymbolDistanceTo.ClientID %>').value = preset.symbolDistanceTo;
	document.getElementById('<%=this.NonlinearDistortion.ClientID %>').checked = preset.nonlinearDistortion;
	document.getElementById('<%=this.BorderColor.ClientID %>').value = preset.borderColor;    
    
    var fontFamilies = Bitrix.ArrayHelper.fromString(preset.fontFamilyName);
    var fontList = document.getElementById('<%=this.FontWhitelist.ClientID %>');
    for(var i = 0; i < fontList.options.length; i++) { 
        var name = fontList.options[i].value;
        var selected = false;
        for(var j = 0; j < fontFamilies.length; j++) {
            if(fontFamilies[j] != name) continue;
            selected = true;
            break;
        }
        fontList.options[i].selected = selected;    
    }	
}
</script>
</asp:Content>