<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditBlogUsers.ascx.cs" Inherits="Bitrix.Blog.UI.EditBlogUsers" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Services.Js" %>

<script type="text/javascript">
	var <%= ClientID %>_Groups = <%= BXJSUtility.BuildJSArray(Groups.ConvertAll(x => new Dictionary<string, string> { { "text",  x.Name }, { "value", x.Id.ToString() } })) %>;

	function <%= ClientID %>_Add(data)
	{
		var item = document.createElement("DIV");
		item.className = "blog-users-item";
		
		if (data.userData && data.userData.image)
		{
			var img = document.createElement("IMG");
			img.className = "blog-users-item-img";
			img.src = data.userData.image;
			img.alt = data.text;
			img.title = data.text;
			item.appendChild(img);
		}
		else
		{
			var img = document.createElement("DIV");
			img.className = "blog-users-item-img";			
			item.appendChild(img);
		}
		var input = document.createElement("INPUT");
		input.type = "hidden";
		input.name = "<%= UniqueID %>$image";
		input.value = (data.userData && data.userData.image) ? data.userData.image : '';
		item.appendChild(input);
		
		
		var a = document.createElement("A");
		a.href = '<%= BXJSUtility.Encode(MakeUserProfileTemplate()) %>'.replace(/#UserId#/gi, data.id);
		a.className = "blog-users-item-name";
		a.appendChild(document.createTextNode(data.text));
		item.appendChild(a);
		
		var input = document.createElement("INPUT");
		input.type = "hidden";
		input.name = "<%= UniqueID %>$name";
		input.value = data.text;
		item.appendChild(input);
		
		input = document.createElement("INPUT");
		input.type = "hidden";
		input.name = "<%= UniqueID %>$id";
		input.value = data.id;
		item.appendChild(input);
				
		var select = document.createElement("SELECT");
		select.name = "<%= UniqueID %>$group";
		var options = <%= ClientID %>_Groups;
		for(var i = 0; i < options.length; i++)
			select.options[select.options.length] = new Option(options[i].text, options[i].value);
		if (data.userData && data.userData.group)
		{
			for(var i = 0; i < select.options.length; i++)
			{
				if (select.options[i].value == data.userData.group)
				{
					select.selectedIndex = i;
					break;
				}
			}
		}
		item.appendChild(select);
		
		input = document.createElement("INPUT");
		input.type = "hidden";
		input.name = "<%= UniqueID %>$auto";
		input.value = "";			
		item.appendChild(input);
		
		if (data.userData && data.userData.hasAuto)
		{
			var check = document.createElement("INPUT");
			check.bxHiddenField = input;
			check.type = "checkbox";
			check.title = "<%= GetMessageJS("CreatedAutomatically") %>";	
			check.setAttribute("onclick", "this.bxHiddenField.value = this.checked ? 'true' : '';");		
			if (data.userData && data.userData.isAuto)
			{
				check.checked = true;
				check.defaultChecked = true;
				input.value = "true";				
			}
						
			item.appendChild(check);
		}				
		
		input = document.createElement("INPUT");
		input.title = "<%= GetMessageJS("CreatedAutomatically") %>";
		input.type = "hidden";
		input.name = "<%= UniqueID %>$hasAuto";
		input.value = (data.userData && data.userData.hasAuto) ? "true" : "";			
		item.appendChild(input);
		
		
		a = document.createElement("A");
		a.href = '';
		a.className = "blog-users-item-delete";
		a.setAttribute("onclick", "return <%= ClientID %>_Delete(this);")
		a.appendChild(document.createTextNode("<%= GetMessageJS("Kernel.Delete") %>"));
		item.appendChild(a);
		
		var div = document.createElement("DIV");
		div.style.clear = "both";
		item.appendChild(div);
		
		document.getElementById("<%= ClientID %>_items").appendChild(item);			
	}

	function <%= ClientID %>_Delete(a)
	{
		var b = a.parentNode;
		if (b && b.parentNode) 
			b.parentNode.removeChild(b);
		return false;
	}
</script>
<div id="<%= ClientID %>">	
	<div id="<%= ClientID %>_items">
		
	</div>
	<div id="<%= ClientID %>_input" class="blog-edit-field blog-edit-field-adduser">
		<bx:BXAutoCompleteTextBox
	        runat="server" 
	        ID="BlogUsers"
	        TextBoxClass="receivers-textbox"	       
	        LabelText=""
	        Url="~/bitrix/handlers/Main/UsersHandler.ashx" 
            CreateMainContainer="false"
            CreateListContainer="false"
            ListContainerClass="fff"
            SetFieldFontSizeToPopup="true"
            DataIdReplaceString="#UserId#"
            DefaultText=""            
		/>
	</div>
</div>
