<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_photo_templates__default_template" EnableViewState="false" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery.photo/component.ascx" %>

<a href="<%=  Component.BackUrl %>" EnableAjax="true"><%= GetMessage("Kernel.Back") %></a> 
<% if(Component.Photo!=null){ %>
<% if (Component.ModifyElements)
   { %>
        <% if (Component.Photo != null)
        { %>
            | <a enableajax="true" href="<%= Component.EditUrl %>"><%= GetMessage("Kernel.Edit") %></a>
            | <asp:LinkButton runat="server" ID="lButtonDelete" Text="<%$ Loc:Delete %>" OnClick="lbDelete_Click" Visible="false"><%= GetMessage("Delete") %></asp:LinkButton>
      <%}%>
<%}%>

<script>
    var fileBottomNavCloseImage = "<%= Bitrix.BXUri.ToRelativeUri(AppRelativeTemplateSourceDirectory) %>images/close.gif";
    var fileLoadingImage = "<%= Bitrix.BXUri.ToRelativeUri(AppRelativeTemplateSourceDirectory) %>images/loading.gif";
</script>
<table>
    <tr valign="top">
        <td class="photo-column" align="center" style="width:300px;">
                <p class="photodetail-date-text"><%= Component.Photo.CreateDate.ToString("d") %></p>
                <p class="photodetail-desc-text"><%=Component.Description%></p>
                <div style="width:<%=Component.PhotoWidth%>px;height:<%=Component.PhotoHeight%>px">
                <div style="width:<%=Component.ActualPhotoWidth%>px;height:<%=Component.ActualPhotoHeight%>px;padding-top:30px;">
                <img id="previewImg" src="<%= Component.PhotoUrl %>" style="position:relative;width:<%=Component.ActualPhotoWidth%>px;height:<%=Component.ActualPhotoHeight%>px;" border="0" />
                </div>
                
                <div align="right" style = "padding-top:2px;"><a style="color:#AAA;font-size:0.85em;text-decoration:underline;" href="<%= Component.PhotoOriginalUrl %>"><%=Component.PhotoOriginalSizeText %></a></div>
                </div>
            

        </td>
    </tr>
</table>
<% } else {%>
  <h1><%= GetMessage("PhotoNotFound") %></h1>  
<%} %>