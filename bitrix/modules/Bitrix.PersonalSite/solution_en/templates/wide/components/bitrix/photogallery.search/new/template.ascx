<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_search_templates__default_template" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<% if (Results.ContainsKey("PHOTOS"))
   { %>
   <h4><%= GetMessage("Legend.Photos") %> </h4>
   <% foreach (BXInfoBlockElementOld photo in (BXInfoBlockElementCollectionOld)Results["PHOTOS"])
       { %>
       <% if(Parameters.Get<bool>("FlickrMode",false)){ %>
    <div style="height:<%= Parameters.Get<int>("PreviewHeight",150) %>px;width:<%= Parameters.Get<int>("PreviewWidth",150) %>px;border:1px solid black;float:left;margin:5px;">
        <a EnableAjax="true" href="?<%= Parameters.Get<string>("PhotoUrl","photo") %>=<%= photo.ElementId %>" title="<%= photo.Name %>" style="display:block;height:<%= Parameters.Get<string>("PreviewHeight","150") %>px;width:<%= Parameters.Get<string>("PreviewWidth","150") %>px;background-image:url(<%= Results["Preview" + photo.ElementId.ToString()] %>)"></a>
    </div>
    <% } else { %>
        <div style="height:<%= Parameters.Get<int>("PreviewHeight",150) + 25 %>px;width:<%= Parameters.Get<int>("PreviewWidth",150) + 25 %>px;float:left;margin:5px;">
            <table style="height:<%= Parameters.Get<int>("PreviewWidth",150) + 25 %>px;width:<%= Parameters.Get<int>("PreviewWidth",150) + 25 %>px;">
                <tr valign="middle" align="center">
                    <td>
                        <a EnableAjax="true" href="?<%= Parameters.Get<string>("PhotoUrl","photo") %>=<%= photo.ElementId %>" title="<%= photo.Name %>" >
                            <img border="0" style="border:1px solid black;" src="<%= Results["Preview" + photo.ElementId.ToString()] %>" />
                        </a>
                    </td>
                </tr>
            </table>
            
        </div>
    <% } %>
    <% } %>
<br clear="all"/>
<%} else { %>
    <%= GetMessage("SearchIsCompleteThereAreNoResultsToDisplay") %> <a EnableAjax="true" href="<%= Request.UrlReferrer.AbsolutePath %>"><%= GetMessage("Return") %></a>
<%}%>