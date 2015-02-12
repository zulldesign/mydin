<%@ Control Language="C#" AutoEventWireup="true" CodeFile="IBlockPublicEditorGroupRenderer.ascx.cs" Inherits="Bitrix.IBlock.UI.IBlockPublicEditorGroupRenderer" %>
<table class="edit-table" cellpadding="0" cellspacing="0">
<% 
	foreach (var f in fields)
	{
		if (f.FormFieldName != null && f.Id!="Catalog" &&
			(f.Id != "DetailText") && 
			(f.Id != "PreviewText") && 
			(f.Id!="DetailImage" || DetailImagePlaceHolder.Controls.Count==0)
			&& (f.Id != "PreviewImage" || PreviewImagePlaceHolder.Controls.Count == 0)
			)
		{	 %>
<tr valign="top"><td class="field-name" style="width:40%"><%= f.Title%>:</td><td><%= f.Render()%></td></tr>

<%}
		else if (f.Id.Equals("Catalog") && CataloguePlaceHolder.Controls.Count > 0)
		{ %>
   <tr class="heading">
    <td colspan="2"><%= f.Title %></td>
</tr>
<tr><td colspan="2"><asp:PlaceHolder ID="CataloguePlaceHolder" runat="server"></asp:PlaceHolder></td></tr>
<%} 
  else  if(f.Id.Equals("DetailText")) 
  { %>
  <tr class="heading"><td colspan="2" ><%= f.Title %></td></tr>
<tr><td colspan="2">
<asp:PlaceHolder ID="DetailTextPlaceHolder" runat="server"></asp:PlaceHolder>
<% if (!DetailTextWebEditorIncluded)
	 { %><%=f.Render()%><%} %>							
</td></tr>
<%}else if (f.Id.Equals("PreviewText"))
		{ %>
		<tr class="heading"><td colspan="2"><%= f.Title %></td></tr>
		<tr valign="top"><td colspan="2">
		<asp:PlaceHolder ID="PreviewTextPlaceHolder" runat="server"></asp:PlaceHolder>
		<% if (!PreviewTextWebEditorIncluded)
	 { %><%=f.Render()%><%} %>
		</td></tr>
	<%}
		else if (PreviewImagePlaceHolder.Controls.Count > 0 && f.Id.Equals("PreviewImage"))
		{ %>
			<tr valign="top">
		<td class="field-name" style="width:40%">
		<%=f.Title %>:
		</td>
		<td style="width:60%">
		<asp:PlaceHolder ID="PreviewImagePlaceHolder" runat="server"></asp:PlaceHolder>
		</td></tr>
	<%}
		else if (DetailImagePlaceHolder.Controls.Count > 0 && f.Id.Equals("DetailImage"))
		{ %>
	<tr valign="top">
		<td class="field-name" style="width:40%">
		<%=f.Title %>:
		</td>
		<td style="width:60%">
		<asp:PlaceHolder ID="DetailImagePlaceHolder" runat="server"></asp:PlaceHolder>
		</td></tr>
<%} else { %>
<%= f.Render() %>
<%}
	} %>
</table>