<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_slider_templates__default_template" EnableViewState="false" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery.slider/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<input type="hidden" runat = "server" id = "currentPhotoId" />
<table border="0" >
      <tr>
        <td>
            <a id="sliderLeft" enableajax="true" noajaxstyle="true" href="<%=pageLinks[0] %>"  class="<%=(Component.SelectedIndex==0)? "slider-left-disabled" : "slider-left"%>" <%=(Component.SelectedIndex==0)? "onclick=\"return false;\"":"" %> ></a>
        </td>
        <%
            for (int i = 0; i < Component.Size; i++)
           {%>
            <td>
              
                <div class="photo" style="overflow:hidden">
                    <% if(Parameters.Get<bool>("FlickrMode",false)) { %>
                        <table  cellpadding="0" cellspacing="0" style="<%=sizes[i]%>">
                        <tr valign="middle" align="center">
                            <td>
                                <a enableajax="true" noajaxstyle="true" id="prefix_<%= ID %>_link_<%= i %>" href="<%=pageLinks[i+1] %>">
                                    <img border="0" style="border:hidden;" alt="<%= GetMessage("ZoomIn") %>" id="prefix_<%= ID %>_<%= i %>" src="<%=photoLinks[i] %>" />
                                </a>
                            </td>
                        </tr>
                    </table>
                    <% } else { %>
                    <table  cellpadding="0" cellspacing="0" style="height:<%= Component.PreviewHeight%>px;width:<%= Component.PreviewWidth%>px;">
                        <tr valign="middle" align="center">
                            <td>
                                <table><tr><td class = "<%=((i+GetStartIndex == Component.SelectedIndex)? "preview-frame-selected" : "preview-frame") %>"><a enableajax="true" noajaxstyle="true" id="prefix_<%= ID %>_link_<%= i %>" href="<%=pageLinks[i+1] %>"><img border="0" style="border:hidden;<%=sizes[i]%>" alt="<%= GetMessage("ZoomIn") %>" id="prefix_<%= ID %>_<%= i %>" src="<%= photoLinks[i] %>" /></a></td></tr></table>
                            </td>
                        </tr>
                    </table>        
                    <% } %>
                </div>
            </td>
        <% }%>
        <td>
            <a id="sliderRight" enableajax="true" noajaxstyle="true" href="<%=(Component.SelectedIndex - GetStartIndex +2 == pageLinks.Length)?"#":pageLinks[Component.SelectedIndex - GetStartIndex +2] %>" class="<%=(Component.SelectedIndex==Component.PhotoItems.Count-1)? "slider-right-disabled" : "slider-right"%>" <%=(Component.SelectedIndex==Component.PhotoItems.Count-1)? "onclick=\"return false;\"":"" %>></a>
        </td>
      </tr>
</table>