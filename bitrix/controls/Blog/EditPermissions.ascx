<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditPermissions.ascx.cs" Inherits="Bitrix.Blog.UI.EditPermissions" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<script type="text/javascript">
	function <%= ClientID %>_AddPostGroup(before)
	{
		var table = BX('<%= ClientID %>');
		var row = table.insertRow(BX.util.array_search(before, table.rows));
		row.style.verticalAlign = 'top';
		
		var cell = row.insertCell(-1);
		cell.className = '';
		cell.setAttribute('width', '40%');
		cell.innerHTML = '<%= BXJSUtility.RenderControl(Cell1) %>';
		
		cell = row.insertCell(-1);
		cell.className = 'field-posts';		
		cell.innerHTML = '<%= BXJSUtility.RenderControl(Cell2) %>';
		
		cell = row.insertCell(-1);
		cell.className = 'field-comments';		
		cell.innerHTML = '<%= BXJSUtility.RenderControl(Cell3) %>';
		
		cell = row.insertCell(-1);		
		cell.setAttribute('width', '60%');
		cell.innerHTML = '<%= BXJSUtility.RenderControl(Cell4) %>';
		
		return false;
	}
		
	function <%= ClientID %>_DeletePostGroup(row)
	{
		row.parentNode.removeChild(row);
		return false;
	}
</script>
<table id="<%= ClientID %>" class="edit-table" cellspacing="0" cellpadding="0" border="0">
	<tr class="heading">
		<td>&nbsp;</td>
		<td><%= GetMessageRaw("Column.Posts") %></td>
		<td><%= GetMessageRaw("Column.Comments") %></td>
		<td>&nbsp;</td>
	</tr>
	<% foreach (var i in Permissions) { %>
	<% if (i.IsDeleted) continue; %>
	<tr style="vertical-align:top">
		<td class="" width="40%">
			<% if (i.Group == null || i.Group.Type == BXBlogUserGroupType.UserDefined) { %>
				<% if (i.HasError) { %><span style="color:Red">*</span><% } %><input type="text" name="<%= UniqueID %>$name" value="<%= Encode(i.Title)  %>" />				
			<% } else { %>
				<%= Encode(i.Title) %>:
				<input type="hidden" name="<%= UniqueID %>$name" value="" />				
			<% } %>			
			<input type="hidden" name="<%= UniqueID %>$id" value="<%= i.Group != null ? i.Group.Id : 0 %>" />
		</td>
		<td class="field-posts">
			<select name="<%= UniqueID %>$postlevel">
			<% foreach (BXBlogPostPermissionLevel j in Enum.GetValues(typeof(BXBlogPostPermissionLevel))) { %>
				<option value="<%= (int)j %>" <% if ((int)j == i.PostLevel) { %>selected="selected"<% } %>><%= BXBlogModule.GetMessageRaw("UserGroups", "PostPermission." + j.ToString()) %></option>
			<% } %>
            </select>
		</td>
		<td class="field-comments">
			<select name="<%= UniqueID %>$commentlevel">
			<% foreach (BXBlogCommentPermissionLevel j in Enum.GetValues(typeof(BXBlogCommentPermissionLevel))) { %>
				<option value="<%= (int)j %>" <% if ((int)j == i.CommentLevel) { %>selected="selected"<% } %>><%= BXBlogModule.GetMessageRaw("UserGroups", "CommentPermission." + j.ToString()) %></option>
			<% } %>
            </select>
		</td>
		<td width="60%">
			<% if (i.Group == null || (i.Group.Type == BXBlogUserGroupType.UserDefined && i.Group.BlogId != 0)) { %>
            <a href="" onclick="return <%= ClientID %>_DeletePostGroup(this.parentNode.parentNode);" ><%= GetMessageRaw("Kernel.Delete") %></a>
            <% } else { %>
            &nbsp;
            <% } %>
		</td>
	</tr>
	<% } %>
	<tr valign="top">
		<td>&nbsp;</td>
		<td colspan="3">
			<a href="" onclick="return <%= ClientID %>_AddPostGroup(this.parentNode.parentNode);" ><%= GetMessageRaw("Kernel.Add") %></a>
		</td>
	</tr>	
</table>


<% if (false) { %>	
	<asp:PlaceHolder runat="server" ID="Cell1">		
	<input type="text" name="<%= UniqueID %>$name" value="<%= GetMessage("DefaultUserGroupName") %>" />
	<input type="hidden" name="<%= UniqueID %>$id" value="0" />
	</asp:PlaceHolder>
	
	<asp:PlaceHolder runat="server" ID="Cell2">
	<select name="<%= UniqueID %>$postlevel">
	<% foreach (BXBlogPostPermissionLevel j in Enum.GetValues(typeof(BXBlogPostPermissionLevel))) { %>
		<option value="<%= (int)j %>" <% if (j == BXBlogUserPermission.DefaultPostPermission) { %>selected="selected"<% } %>><%= BXBlogModule.GetMessageRaw("UserGroups", "PostPermission." + j.ToString()) %></option>
	<% } %>
    </select>
	</asp:PlaceHolder>
	
	<asp:PlaceHolder runat="server" ID="Cell3">
	<select name="<%= UniqueID %>$commentlevel">
	<% foreach (BXBlogCommentPermissionLevel j in Enum.GetValues(typeof(BXBlogCommentPermissionLevel))) { %>
		<option value="<%= (int)j %>" <% if (j == BXBlogUserPermission.DefaultCommentPermission) { %>selected="selected"<% } %>><%= BXBlogModule.GetMessageRaw("UserGroups", "CommentPermission." + j.ToString()) %></option>
	<% } %>
    </select>
	</asp:PlaceHolder>
	
	<asp:PlaceHolder runat="server" ID="Cell4">
	<a href="" onclick="return <%= ClientID %>_DeletePostGroup(this.parentNode.parentNode);" ><%= GetMessageRaw("Kernel.Delete") %></a>            
	</asp:PlaceHolder>
<% } %>
