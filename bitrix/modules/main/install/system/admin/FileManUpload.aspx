<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="FileManUpload.aspx.cs" Inherits="bitrix_admin_FileManUpload" Title="<%$ Loc:PageTitle %>"
	EnableViewState="false"  EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<script type="text/javascript">
		<% string uploadFileIdPrefix = "_inputFile"; %>
		function fileMan_NewFileName(ob)
		{
			var str_filename;			
			var str_file = ob.value.replace(/\//g, '\\');
			var filename = str_file.substr(str_file.lastIndexOf('\\') + 1);

			var row = ob.parentNode.parentNode;
			var nameInput = row.cells[0].getElementsByTagName("INPUT")[0];
			nameInput.value = filename;
			
			var table = document.getElementById('fileManUpload_uploadsTable');
			var counter = document.getElementById('<%= fileCount.ClientID %>');
						
			if (counter.value == row.rowIndex)
			{								
				var newrow = table.insertRow(-1);				
				newrow.insertCell(0).innerHTML = '<input name="fileManUpload_uploadsTable_itemText_' + (newrow.rowIndex - 1) + '" type="text" maxlength="255" size="30" value="">';		
				newrow.insertCell(1).innerHTML = '<input name="fileManUpload_uploadsTable_itemFile_' + (newrow.rowIndex - 1) + '" type="file" size="50" value="" onChange="fileMan_NewFileName(this);">';
				counter.value = newrow.rowIndex;
			}
		}
		function fileMan_GoBack()
		{
			window.location.href = '<%= Bitrix.Services.Js.BXJSUtility.Encode(BackUrl) %>';
			return false;
		}
		function fileMan_ConfirmOverwrite(sender, args)
		{
			args.IsValid = true;
			
			var counter = document.getElementById('<%= fileCount.ClientID %>');
			var table = document.getElementById('fileManUpload_uploadsTable');
			var files = [];
			var names = [];
			for(var i = 1; i < table.rows.length; i++)
			{				
				var file = table.rows[i].cells[1].getElementsByTagName('INPUT')[0].value;
				if (file == '')
					continue;
				
				files.push(file);
				names.push(table.rows[i].cells[0].getElementsByTagName('INPUT')[0].value);
			}
			
			if (files.length == 0)
				return;
				
			var request = null;
			if(window.XMLHttpRequest)
			{
				try {request = new XMLHttpRequest();} catch(e){}
			}
			else if(window.ActiveXObject)
			{
				try {request = new ActiveXObject("Microsoft.XMLHTTP");} catch(e){}
				if(!request)
					try {request = new ActiveXObject("Msxml2.XMLHTTP");} catch (e){}
			}
			
			var postData = '';
			for(var i = 0; i < files.length; i++)
			{
				if (i != 0)
					postData += '&';
				postData += 'file=' + encodeURIComponent(files[i]) + '&name=' + encodeURIComponent(names[i]);
			}
			
			
			var isSuccessful = true;
			try
			{ 
				request.open(
					"POST", 
					'<%= JSEncode(VirtualPathUtility.ToAbsolute("~/bitrix/admin/FileManUpload.aspx")) %>?path=<%= JSEncode(UrlEncode(curPath)) %>&check=',
					false
				);
				
				request.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
				request.setRequestHeader("Content-length", postData.length);
				request.setRequestHeader("Connection", "close");
				
				request.send(postData); 
			} 
			catch(e)
			{
				isSuccessful = false;       
			}
			isSuccessful &= (request.status == 200);
			
			if(!isSuccessful)
				return;
			
			var filesExist = eval('(' + request.responseText + ')');
			
			if (filesExist == null || !(filesExist instanceof Array) || filesExist.length == 0)
				return;
				
			var filesList = '';
			for(var i = 0; i < filesExist.length; i++)
			{
				if (i != 0)
					filesList += ', ';
				filesList += filesExist[i];
			}
			
			var overwrite = args.IsValid = confirm('<%= JSEncode(string.Format(GetMessageRaw("Message.OverwriteFiles"), curPath)) %>'.replace(/#FILES#/g, filesList));
			if (overwrite)
				document.getElementById('<%= Overwrite.ClientID %>').value = "Y";
		}
	</script>
	
	<% ButtonBack.Href = BackUrl;%>
	<bx:BXContextMenuToolbar id="BXContextMenuToolbar1" runat="server">
		<Items>
			<bx:BXCmImageButton runat="server" ID="ButtonBack" CssClass="context-button icon btn_folder_up" CommandName="back"
				Text="<%$ Loc:ActionText.GoBack %>" Title="<%$ Loc:ActionTitle.GoBack %>"
				/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
	<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.Success %>"
		CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False"
		Width="438px" />
	<bx:BXTabControl ID="mainTabControl" runat="server" OnCommand="mainTabControl_Command" ValidationGroup="Upload" >
		<bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText.Upload %>" Title="<%$ Loc:TabTitle.Upload %>">			
			<asp:HiddenField runat="server" ID="Overwrite" />
			<asp:HiddenField runat="server" ID="fileCount" />
			<asp:CustomValidator runat="Server" ID="ConfirmOverwrite" Display="None" ClientValidationFunction="fileMan_ConfirmOverwrite" ValidationGroup="Upload" />
			<table width="100%" cellspacing="0" cellpadding="0" border="0" class="edit-table">
				<tr>
					<td>
						<table id="fileManUpload_uploadsTable" cellspacing="0" cellpadding="0" border="0" class="internal">
							<tr class="heading">
								<td valign="middle" align="center"><%= GetMessage("Column.Filename") %></td>
								<td valign="top" align="center"><%= GetMessage("Column.File") %></td>
							</tr>
							<% for (int i = 0; i < FilesCount; i++) { %>
								<tr>
									<td>
										<input type="text" name="fileManUpload_uploadsTable_itemText_<%= i %>" maxlength="255" size="30" />										
									</td>
									<td>
										<input type="file" name="fileManUpload_uploadsTable_itemFile_<%= i %>" size="50" onchange="fileMan_NewFileName(this);" />										
									</td>
								</tr>
							<% } %>
						</table>
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>
</asp:Content>
