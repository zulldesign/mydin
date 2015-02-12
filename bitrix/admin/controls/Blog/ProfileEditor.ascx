<%@ Control Language="C#" AutoEventWireup="false" CodeFile="ProfileEditor.ascx.cs" Inherits="Bitrix.Blog.UI.ProfileEditor" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.Blog" %>
<script type="text/javascript">
	function AddBinding(before)
	{
		var table = BX('<%= ClientID %>');		
		var row = table.insertRow(BX.util.array_search(before, table.rows));
		row.style.verticalAlign = 'top';
		
		var cell = row.insertCell(-1);
		cell.innerHTML = '&nbsp;';
		cell.className = 'field-name';
		cell.setAttribute('width', '40%');
		 
		cell = row.insertCell(-1);
		cell.style.whiteSpace = 'nowrap';
		cell.innerHTML = '<%= BXJSUtility.RenderControl(Cell1) %>';
		
		return false;
	}
	
	function DeleteBinding(row)
	{
		row.parentNode.removeChild(row);
		return false;
	}
	
	function ChangeBlog(blog)
	{		
		if (blog.bx_xhr && blog.bx_xhr.abort)
		{
			try { blog.bx_xhr.abort(); } catch(e) {}			
		}
		
		if (blog.selectedIndex <= 0)
		{
			blog.bx_xhr = null;
			var group = blog.parentNode.getElementsByTagName("SELECT")[1];
			while (group.options.length > 1)
				group.remove(group.options.length - 1);
			return;
		}
		
		var v = "";
		var s = blog.selectedIndex;
		if (s >= 0)
			v = blog.options[s].value;

		<% var url = MakeAjaxUrl(); %>
		blog.bx_xhr = BX.ajax.loadJSON(
			'<%= JSEncode(url) %>&id=' + encodeURIComponent(v),
			function(data)
			{
				blog.bx_xhr = null;
				
				var group = blog.parentNode.getElementsByTagName("SELECT")[1];
				
				var v = "";
				var s = group.selectedIndex;
				if (s >= 0)
					v = group.options[s].value;
								
				while (group.options.length > 1)
					group.remove(group.options.length - 1);
					
				for(var i = 0; i < data.length; i++)
				{
					var opt = new Option(data[i].title, data[i].value);
					try
					{
						group.add(opt, null);		
					}
					catch(e)
					{
						group.add(opt);		
					}
				}
					
				for(var i = group.options.length - 1; i >= 0; i--)
				{
					if (group.options[i].value == v)
					{
						group.selectedIndex = i;
						break;
					}
				}
			}
		);
	}
</script>
<%	
	var blogIds = new List<int>();
	foreach (var b in bindings)
	{
		if (!b.Editable)
			continue;
		if (blogIds.Contains(b.BlogGroup.BlogId))
			continue;

		blogIds.Add(b.BlogGroup.BlogId);
	}

	var groups =
	   (blogIds.Count > 0)
	   ? BXBlogUserGroup.GetList(
		   new BXFilter(new BXFilterItem(BXBlogUserGroup.Fields.BlogId, BXSqlFilterOperators.In, blogIds)),
		   new BXOrderBy(
			   new BXOrderByPair(BXBlogUserGroup.Fields.BlogId, BXOrderByDirection.Asc),
			   new BXOrderByPair(BXBlogUserGroup.Fields.Name, BXOrderByDirection.Asc)
		   ),
		   new BXSelect(BXBlogUserGroup.Fields.Id, BXBlogUserGroup.Fields.Name, BXBlogUserGroup.Fields.BlogId),
		   null,
		   Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder
	   )
	   : new BXBlogUserGroupCollection();
%>

<table class="edit-table" cellspacing="0" cellpadding="0" border="0" id="<%= ClientID %>">
	<tr class="heading">
		<td align="center" colspan="2"><%= GetMessageRaw("Title.BlogGroups") %></td>
	</tr>
	<% foreach (var b in bindings) { %>
	<tr valign="top">
		<td class="field-name" width="40%">
			&nbsp;
		</td>
		<td style="white-space: nowrap;">
			<% if (b.Editable) { %>
			<select name="<%= UniqueID %>$blog" onchange="ChangeBlog(this);" style="width:200px">
				<option value=""><%= GetMessageRaw("Option.SelectBlog") %></option>
				<% foreach (var bl in Blogs) { %>
				<option value="<%= bl.Id %>" <% if (b.BlogGroup.BlogId == bl.Id) { %>selected="selected"<% } %>><%= Encode(bl.Title) %></option>
				<% } %>
			</select>			
			<%= GetMessageRaw("InGroup") %>	
			<select name="<%= UniqueID %>$group" style="width:200px">
				<option value=""><%= GetMessageRaw("Option.SelectGroup") %></option>
				<% for (int i = groups.FindIndex(x => x.BlogId == b.BlogGroup.BlogId); i >= 0 && i < groups.Count && groups[i].BlogId == b.BlogGroup.BlogId; i++) { %>
				<option value="<%= groups[i].Id %>" <% if (groups[i].Id == b.BlogGroup.Id) { %>selected="selected"<% } %>><%= Encode(groups[i].Name) %></option>
				<% } %>
			</select>					
			<% } else { %>			
			<input type="hidden" name="<%= UniqueID %>$blog" value="" />
			<input type="hidden" name="<%= UniqueID %>$group" value="<%= b.BlogGroup.Id %>" />
			<b><%= Encode(b.BlogGroup.GetDisplayName()) %></b>
			<% if (b.BlogGroup.BlogId != 0 && b.BlogGroup.Blog != null) { %>
			<%= GetMessageRaw("InBlog") %> <a href="<%= VirtualPathUtility.ToAbsolute("~/bitrix/admin/BlogEdit.aspx") %>?id=<%= b.BlogGroup.BlogId %>"><%= Encode(b.BlogGroup.Blog.Name) %></a>
			<% } %>
			<% } %>
			<span>
			<input type="hidden" name="<%= UniqueID %>$auto" value="<%= b.Editable && b.IsAuto ? "true" : "" %>" />
			<% if (b.Editable && b.IsAuto) { %>
			<input type="checkbox" checked="checked" title="<%= GetMessage("Tooltip.CreatedAutomatically") %>" onclick="this.parentNode.getElementsByTagName('INPUT')[0].value = this.checked ? 'true' : '';" />			
			<% } %>
			</span>			
			&nbsp;
			<a href="" onclick="return DeleteBinding(this.parentNode.parentNode);"><%= GetMessageRaw("Kernel.Delete") %></a>
		</td>
	</tr>
	<% } %>
	<tr valign="top">
		<td class="field-name" width="40%">&nbsp;</td>
		<td>
			<a href="" onclick="return AddBinding(this.parentNode.parentNode);"><%= GetMessageRaw("AddGroup") %></a>
		</td>
	</tr>
</table>


<% if (false) { %>
	<asp:PlaceHolder runat="server" ID="Cell1">
	<select name="<%= UniqueID %>$blog" onchange="ChangeBlog(this);" style="width:200px">
		<option value="" selected="selected"><%= GetMessageRaw("Option.SelectBlog") %></option>
		<% foreach (var bl in Blogs) { %>
		<option value="<%= bl.Id %>"><%= Encode(bl.Title) %></option>
		<% } %>
	</select>
	<%= GetMessageRaw("InGroup") %>
	<select name="<%= UniqueID %>$group" style="width:200px">
		<option value="" selected="selected"><%= GetMessageRaw("Option.SelectGroup") %></option>
	</select>		
	&nbsp;
	<a href="" onclick="return DeleteBinding(this.parentNode.parentNode);"><%= GetMessageRaw("Kernel.Delete") %></a>
	</asp:PlaceHolder>
<% } %>	