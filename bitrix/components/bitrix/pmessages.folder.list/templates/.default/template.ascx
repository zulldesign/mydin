<%@ Reference Control="~/bitrix/components/bitrix/pmessages.folder.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.CommunicationUtility.Components.PrivateMessageFolderListTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.CommunicationUtility.Components" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Services" %>

<%
	if (Component.FatalError != PrivateMessageFolderListComponent.ErrorCode.None)
	{ 
		%>
		<div class="forum-content">
		<div class="forum-note-box forum-note-error">
			<div class="forum-note-box-text"><%= Component.GetErrorHtml(Component.FatalError) %></div>
		</div>	
		</div>
		<% 
		return;
	}
%>

<bx:InlineScript runat="server" ID="Script">
<script type="text/javascript">

<%=bCreateFolder.ClientID %>Component_FireDefaultButton = function(event, target) 
{
    if (event.keyCode == 13) 
    {
        var src = event.srcElement || event.target;
        if (!src || (src.tagName.toLowerCase() != "textarea")) 
        {
            var defaultButton = document.getElementById(target);
            if (defaultButton && typeof(defaultButton.click) != "undefined") 
            {
                defaultButton.click();
                event.cancelBubble = true;
                if (event.stopPropagation) 
                    event.stopPropagation(); 
                return false;
            }
        }
    }
    
    return true;
}

function <%= ClientID %>_SelectRow(row)
{
	if (row == null)
		return;

	if(row.className.match(/forum-row-selected/))
		row.className = row.className.replace(/\s*forum-row-selected/i, '');
	else
		row.className += ' forum-row-selected';
}
function <%= ClientID %>_SelectAll(table)
{
	if (table == null)
		return;
	var inputs = table.getElementsByTagName("INPUT");
	if (!inputs)
		return;
	var status;
	var hasStatus = false;
	for(var i = 0; i < inputs.length; i++)
	{
		var input = inputs[i];
		if (input.type != "checkbox")
			continue;
		
		if (!hasStatus)
		{
			status = !input.checked;
			hasStatus = true;
		}
		
		if (input.checked != status)
			input.click();
	}
}
function <%= ClientID %>_OKClick()
{
	var ddl = document.getElementById('<%= ClientID %>');
	if (ddl == null)
		return true;
	if (ddl.value == '')
		return false;
	if (ddl.value == '<%= PrivateMessageFolderListComponent.FolderOperation.Delete %>')
		return window.confirm('<%= GetMessageJS("ConfirmDelete") %>');
	return true;
}

function <%= ClientID %>_OperationClick()
{
    var ddl = document.getElementById('<%= ClientID %>');
    var table = document.getElementById('<%=ClientID %>_table');

	if (ddl == null)
		return true;
        var inputs = document.getElementsByName("<%= UniqueID %>$operate");
	    if (!inputs)
		    return;
	    var status;
	    var hasStatus = false;
	    for(var i = 0; i < inputs.length; i++)
	    {
		    var input = inputs[i];
		    if (input.type != "checkbox")
			    continue;
			
			 <%=ClientID %>_ChangeRow(input,ddl);
			
	    }
    
}

function <%=ClientID %>_ChangeRow(input,ddl)
{
    var link = document.getElementById("<%= ClientID %>_link_"+input.value);
	var textbox = document.getElementById("<%= ClientID %>_tb_"+input.value);
	if (!link || !textbox) return true;
	if (ddl.value == '<%= PrivateMessageFolderListComponent.FolderOperation.Edit %>' && input.checked == true ){
		link.style.display="none";
		textbox.style.display="";
    }
	else
	{
		link.style.display="";
		textbox.style.display="none";
	}
	
}

function <%=ClientID %>_FolderNameValidate()
{
    var tb = document.getElementById("<%=tbNewFolderName.ClientID %>");
    if ( tb )
    {
        if ( tb.value.replace(/\s+$/,"")=="" ){
            var div = document.getElementById("<%=ClientID %>_Errors");
            if ( div ){
                div.innerHTML = '<%= GetMessage("Error.FolderNameRequired") %>';
                div.style.display="";
            }
            return false;
        }
    }
    
    return true;
}

</script>
</bx:InlineScript>

<div class="pmessages-content">
<div class="forum-info-box" onkeypress = "return <%=bCreateFolder.ClientID %>Component_FireDefaultButton(event,'<%=bCreateFolder.ClientID %>');">
    <asp:TextBox ID="tbNewFolderName" runat="server"></asp:TextBox>
    <asp:Button ID="bCreateFolder" runat="server" OnClick="CreateFolder"/>
    <div class = "forum-note-box forum-note-error" id = "<%=ClientID %>_Errors" style="display:none"></div>
</div>

<div class="forum-header-box">
	<div class="forum-header-options">
	</div>
	<div class="forum-header-title"><span><%=GetMessage("Header")%></span></div>
</div>
<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">
			<table id="<%=ClientID %>_table" cellspacing="0" class="forum-table forum-topic-list">
<% if (Component.Folders.Count > 0) { %>
			<thead>
				<tr>
					<th class="forum-column-title" colspan="2"><div class="forum-head-title"><span><%= GetMessage("Folders") %></span></div></th>
					<th class="forum-column-replies"><span><%= GetMessage("Topics") %></span></th>
					<th class="forum-column-lastpost"><span><%= GetMessage("LastMessage") %></span></th>
				</tr>
			</thead>
			<tbody>
<%
	StringBuilder cssClass = new StringBuilder();
	StringBuilder statusHtml = new StringBuilder();
	string statusFormat = @"<span class=""{0}"">{1}</span>";
	for (int i = 0; i < Component.Folders.Count; i++)
	{
		PrivateMessageFolderListComponent.FolderInfo info = Component.Folders[i];
		cssClass.Length = 0;
		if (i == 0)
			cssClass.Append("forum-row-first ");
		if (i == Component.Folders.Count - 1)
			cssClass.Append("forum-row-last ");
		cssClass.Append(i % 2 == 0 ? "forum-row-odd" : "forum-row-even"); //because of zero-based index
			
		
		statusHtml.Length = 0;

		string iconCss=info.HaveUnreadMessages ? "forum-icon-newposts":"forum-icon-default";
		string iconTitle=GetMessage(info.HaveUnreadMessages ? "ToolTip.HaveUnreadMessages":"ToolTip.NoUnreadMessages");

		
		if (statusHtml.Length > 0)
			statusHtml.Append(":&nbsp;");
%>
 				<tr class="<%= cssClass %>">
					<td class="forum-column-icon">
						<div class="forum-icon-container">
							<div class="forum-icon <%= iconCss %>" title="<%= iconTitle %>"><!-- ie --></div>
						</div>
					</td>
					<td class="forum-column-title">
						<div class="forum-item-info">
							<div class="forum-item-name"><span class="forum-item-title">
							<a id="<%=ClientID %>_link_<%=info.Folder.Id.ToString() %>" href="<%= info.FolderUrl %>"><%= info.TitleHtml %></a></span>
                            <input type ="text" 
                                id="<%=ClientID %>_tb_<%=info.Folder.Id.ToString() %>" 
                                name="<% = UniqueID + "$flname_"+info.Folder.Id.ToString()%>" 
                                style="display:none;width:70%;" 
                                value="<%=info.TitleHtml %>" 
                                
                                />
                                </div>
						</div>
					</td>

					<td class="forum-column-replies"><span><%=info.Folder.TopicsCount %></span></td>
					<td class="forum-column-lastpost">
					<div class="forum-select-box">
				        <input type="checkbox" name="<%= UniqueID %>$operate" onclick="<%=ClientID %>_ChangeRow(this,document.getElementById('<%=ClientID %>'));" value="<%= info.Folder.Id %>" 
				                onclick="<%= ClientID %>_SelectRow(this.parentNode.parentNode.parentNode)" 
				                />
				    </div>

				    <div class="forum-lastpost-box">
				        <% if (info.LastMessageUrl != String.Empty)
                        { %>
						<span class="forum-lastpost-date"><a href="<%= info.LastMessageUrl %>"><%= info.LastMessageDateHtml%></a></span>
						<span class="forum-lastpost-title"><span class="forum-lastpost-author"><%= info.LastPosterNameHtml%></span></span>
						<%} %>
					</div>
				    </td>
				</tr>
				<% } %>
			</tbody>
			<tfoot>
				<tr>
					<td colspan="4" class="forum-column-footer">
						<div class="forum-footer-inner">
								<div class="forum-topics-moderate">
								<select id="<%= ClientID %>" name="<%= UniqueID %>$operation" onchange="<%= ClientID %>_OperationClick()">
									<option value=""><%= GetMessage("ManageFolders") %></option>
									<option value="<%= PrivateMessageFolderListComponent.FolderOperation.Edit %>"><%= GetMessage("Option.Edit") %></option>
									<option value="<%= PrivateMessageFolderListComponent.FolderOperation.Delete %>"><%= GetMessage("Option.Delete") %></option>
									<option value="<%= PrivateMessageFolderListComponent.FolderOperation.MoveUp %>"><%= GetMessage("Option.MoveUp") %></option>
									<option value="<%= PrivateMessageFolderListComponent.FolderOperation.MoveDown %>"><%= GetMessage("Option.MoveDown") %></option>
							    </select>
							    &nbsp;<asp:Button runat="server" ID="OK" Text="<%$ LocRaw:Kernel.OK %>" OnClick="OKClick" />
							    </div>
					    </div>
				    </td>
			    </tr>
			</tfoot>

<%
   
   } else { %>
			<tbody>
 				<tr class="forum-row-first forum-row-last forum-row-odd">
					<td class="forum-column-alone">
						<div class="forum-empty-message"><%= GetMessage("NoFoldersHere") %> 
                         </div>
					</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class="forum-column-footer">
						<div class="forum-footer-inner">

						&nbsp;</div>
					</td>
				</tr>
			</tfoot>
<% } %>
			</table>
		</div>
	</div>
</div>
</div>

<script runat="server">

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		if (Component.FatalError != PrivateMessageFolderListComponent.ErrorCode.None)
			return;

        bCreateFolder.OnClientClick = "return " + ClientID + "_FolderNameValidate();";
        bCreateFolder.Text = GetMessage("Title.CreateFolder");
	}

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        OK.OnClientClick = String.Format("if (!{0}_OKClick()) return false;", ClientID);
    }

    private void CreateFolder(object sender,EventArgs e)
    {
        if (!Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(tbNewFolderName.Text))
            Component.CreateFolder(tbNewFolderName.Text);
        Response.Redirect(GetRedirectUrl(null));
    }

    private void OKClick(object sender, EventArgs e)
    {
        string operationString = Request.Form[UniqueID + "$operation"];
        string newFolderName;
        PrivateMessageFolderListComponent.FolderOperation operation;
        if (string.IsNullOrEmpty(operationString) || (!Enum.IsDefined(typeof(PrivateMessageFolderListComponent.FolderOperation), operationString)))
            return;

        operation =
           (PrivateMessageFolderListComponent.FolderOperation)Enum.Parse(typeof(PrivateMessageFolderListComponent.FolderOperation), operationString);

        string[] idStrings = Request.Form.GetValues(UniqueID + "$operate");
       
        if (idStrings == null || idStrings.Length == 0)
            return;
        Dictionary<int,string> ids = new Dictionary<int,string>();
        
        foreach (string s in idStrings)
        {
            int id;
            if (!int.TryParse(s, out id) || id <= 0)
                continue;
            if (ids.ContainsKey(id))
                continue;
            if ( operation == PrivateMessageFolderListComponent.FolderOperation.Edit )
            {
                newFolderName =  Request.Form[UniqueID + "$flname_"+id.ToString()];
                if ( !Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(newFolderName))
                {
                    ids.Add(id,newFolderName);
                }
                
            }
            else
                ids.Add(id,string.Empty);
        }

        if (ids.Count == 0)
            return;

        Component.DoOperation(operation, ids);
        Response.Redirect(GetRedirectUrl(null));
    }
	

	private string GetRedirectUrl(NameValueCollection queryParams)
	{
		NameValueCollection query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
		query.Remove(BXCsrfToken.TokenKey);
        if ( queryParams!=null )
            query.Add(queryParams);
		
		UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
		uri.Query = query.ToString();
		
		return uri.Uri.ToString();
	}

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        string validateScript = @"
		if (typeof(ValidatorOnSubmit) == ""function"")
		{{		
			var isValidated = ValidatorOnSubmit();		
			if (!isValidated)
			{{
				window.location=""#"";													
				return false;
			}}
			return true;
		}}";

        Page.ClientScript.RegisterOnSubmitStatement(Page.GetType(), "NewValidate", validateScript);
    }
</script>
