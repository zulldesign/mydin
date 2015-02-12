<%@ Page Language="C#" AutoEventWireup="true"
    CodeFile="IBlockElementEditSettings.aspx.cs" Inherits="bitrix_admin_IBlockElementEditSettings" ValidateRequest="false" %>   
<html>
<head id="Head1" runat="server">

</head>
<body>   
<form id="form1" runat="server">
<bx:BXPageAsDialogBehaviour runat="server"  ID="Behaviour" OnSave="behaviour_Save" ButonSetLayout="SaveCancel" />
<style type="text/css">
.iblock-struct-settings-list
{
	width:170px;
}
.iblock-struct-setting-btn
{
	width:97px;
	margin-bottom:3px;
}
</style>

<table cellspacing="0">
	<tbody><tr valign="center">
		<td><%= GetMessage("Label.AvailableTabs") %>:</td>
		<td></td>
		<td><%= GetMessage("Label.Tabs") %>:</td>
		<td></td>
	</tr>
	<tr valign="center">
		<td>
		<select class="iblock-struct-settings-list" size="8" id="available_tabs" onchange="selector.refresh('defaultTab');" name="available_tabs" class="select">
		</select>
		</td>
		<td>
			<input type="button" onclick="selector.copyTab();" title="<%= GetMessage("Title.SelectTab") %>" value="&nbsp; &gt; &nbsp;" id="tabs_copy" name="tabs_copy">
		</td>
		<td>
			<select class="iblock-struct-settings-list" onchange="selector.refresh('selectedTab');" size="8" id="selected_tabs" name="selected_tabs" class="select">
		</select>
		</td>
		<td>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.moveUp('selected_tabs');" title="<%= GetMessage("Title.MoveTabUp") %>" value="<%= GetMessage("Label.Up") %>" class="button" id="tabs_up" name="tabs_up"><br>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.moveDown('selected_tabs');" title="<%= GetMessage("Title.MoveTabDown") %>" value="<%= GetMessage("Label.Down") %>" class="button" id="tabs_down" name="tabs_down"><br>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.edit('selected_tabs');" title="<%= GetMessage("Title.EditTabTitle") %>" value="<%= GetMessage("Label.Edit") %>" class="button" id="tabs_rename" name="tabs_rename"><br>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.createTab();" title="<%= GetMessage("Title.AddTab") %>" value="<%= GetMessage("Label.Add") %>" class="button" id="tabs_add" name="tabs_add"><br>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.del('selected_tabs');" title="<%= GetMessage("Title.DeleteTab") %>" value="<%= GetMessage("Label.Delete") %>" class="button" id="tabs_delete" name="tabs_delete"><br>
		</td>
	</tr>
	<tr valign="center">
		<td><%= GetMessage("Label.AvailableFields") %>:</td>
		<td></td>
		<td><%= GetMessage("Label.SelectedFields") %>:</td>
		<td></td>
	</tr>
	<tr valign="center">
		<td>
			<select class="iblock-struct-settings-list" onchange="" multiple="" size="12" id="available_fields" name="available_fields" class="select">
					</select>
		</td>
		<td>
			<input type="button" onclick="selector.copyField();" title="<%= GetMessage("Title.SelectFields") %>" value="&nbsp; &gt; &nbsp;" id="fields_copy" name="fields_copy"><br><br>
		</td>
		<td id="selected_fields_td">
		<select class="iblock-struct-settings-list" onchange="" multiple="" size="12" id="selected_fields" name="selected_fields" class="select">
		</select>		</td>
		<td>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.moveUp('selected_fields');" title="<%= GetMessage("Title.MoveFieldUp")%>" value="<%= GetMessage("Label.Up")%>" class="button" id="fields_up" name="fields_up"><br>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.moveDown('selected_fields');" title="<%= GetMessage("Title.MoveFieldUp")%>" value="<%= GetMessage("Label.Down")%>" class="button" id="fields_down" name="fields_down"><br>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.edit('selected_fields');" title="<%= GetMessage("Title.EditField")%>" value="<%= GetMessage("Label.Edit")%>" class="button" id="fields_rename" name="fields_rename"><br>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.create('selected_fields');" title="<%= GetMessage("Title.AddSection")%>" value="<%= GetMessage("Label.Add")%>" class="button" id="fields_add" name="fields_add"><br>
			<input class="iblock-struct-setting-btn" type="button" onclick="selector.del('selected_fields');" title="<%= GetMessage("Title.DeleteField")%>" value="<%= GetMessage("Label.Delete")%>" class="button" id="fields_delete" name="fields_delete">
		</td>
	</tr>
	<tr>
	<td colspan="4">
	<asp:CheckBox runat="server" ID="SetDefaultToAll" />
	<label for="<%=SetDefaultToAll.ClientID %>" title="<%=GetMessage("Title.SetDefaultSettingsToAll") %>"><%= GetMessage("Label.SetDefaultSettingsToAll") %></label>
	</td>
	</tr>
		<tr>
	<td colspan="4">
	<asp:CheckBox runat="server" ID="ResetSettings" onmousedown="return OnResetSettings();" />
	<label for="<%=ResetSettings.ClientID %>" onmousedown="return OnResetSettings();" title="<%=GetMessage("Title.ResetSettings") %>"><%= GetMessage("Label.ResetSettings") %></label>
	</td>
	</tr>
</tbody></table>
<asp:CustomValidator ID="ReqFieldsValidator" runat="server" ClientValidationFunction="ReqFieldsValidate" ValidationGroup="IBlockSettingsValidate" />
<asp:HiddenField ID="hfFields" runat="server" />
<asp:HiddenField ID="hfTabs" runat="server" />


<script type="text/javascript">

OnResetSettings = function()
{

	var checkbox = document.getElementById("<%= ResetSettings.ClientID %>");

	if(!checkbox.checked){
		if(!window.confirm('<%= GetMessage("Message.DoYouWantToResetSettings") %>')){
			return false;
		}
		checkbox.checked = true;
	}
	return true;
}

Page_ClientValidate = function(name)
{
	if(name != "IBlockSettingsValidate")
		return true;
	return selector.tryRecData('<%= hfFields.ClientID%>','<%=hfTabs.ClientID%>');
}


	if (typeof (Bitrix) == "undefined")
		Bitrix = {};

if (typeof (Bitrix.IBlockSettingsSelector) == "undefined")

	Bitrix.IBlockSettingsSelector = function(params) {
		this.fields = params.fields;
		this.defaultTabs = params.defaultTabs;
	
		this.defaultFieldsContainer = params.defaultFieldsContainer;
		this.defaultTabsContainer = params.defaultTabsContainer;
		this.selectedTabsContainer = params.selectedTabsContainer;
		this.selectedFieldsContainer = params.selectedFieldsContainer;
		this.dataRecorded = false;
		this.sorts = [];
		for(var field in this.fields)
		{
			if(!this.fields.hasOwnProperty(field))
				continue;
			this.sorts[this.sorts.length] = this.fields[field].id;
		}
			
	}

Bitrix.IBlockSettingsSelector.prototype.Init = function() {

	var haveSelected = false;
	for (var i = 0; i < this.defaultTabs.length; i++) {
		var o = document.createElement("OPTION");
		o.text = this.defaultTabs[i].name;
		o.value = this.defaultTabs[i].id;
		if(this.defaultTabs[i].selected){
			this.selectedTabsContainer.options.add(o);
			haveSelected = true;
			}
		else
			this.defaultTabsContainer.options.add(o);
	}
	if(!haveSelected) // nothing is selected - we have default config
	{
		for (var i = 0; i < this.defaultTabsContainer.options.length; i++) 
		{
			this.copyTab(this.defaultTabsContainer.options[i]);
		}
	}	
}
Bitrix.IBlockSettingsSelector.prototype.trim = function(str)
{
	if(!str)
		return null;
	return str.replace(/^\s\s*/, '').replace(/\s\s*$/, '');
}

Bitrix.IBlockSettingsSelector.prototype.copyTab = function(opt)
{
	if(!opt && this.defaultTabsContainer.selectedIndex==-1)
		return;
	if(!opt && this.defaultTabsContainer.selectedIndex>=0)
		opt = this.defaultTabsContainer.options[this.defaultTabsContainer.selectedIndex];
		

	for(var i=0;i<this.selectedTabsContainer.options.length;i++)
	{
		if(	this.selectedTabsContainer.options[i].value == "_"+opt.value)
			return;
	}
	
	if(opt!=null)
	{
		var newopt = document.createElement("OPTION");
		newopt.value = "_"+opt.value;
		newopt.text = opt.text;
		this.selectedTabsContainer.options.add(newopt);
			
		for(var field in this.fields)
		{
			if(!this.fields.hasOwnProperty(field))
				continue;
					
			var f = this.fields[field];
			if(f.tabId == opt.value){
				f.selected = true;
				f.tabId = newopt.value;
				}
		}
		
	}

	
	this.refresh("selectedTab");
	this.refresh("defaultTab");
}

Bitrix.IBlockSettingsSelector.prototype.moveField = function(option, selTabIndex)
{
	var optId = option.value;
	var field = this.fields[optId];
	this.fields[optId].selected = true;
	this.fields[optId].tabId = this.selectedTabsContainer.options[selTabIndex].value;
	
	this.selectedFieldsContainer.appendChild(option);

	var curTabFound = false;
	var indexToInsert = -1;
	var curIndex = -1;

	for(var i=0;i<this.sorts.length;i++)
	{
		if(!curTabFound && this.fields[this.sorts[i]].tabId == field.tabId)
			curTabFound = true;
		if(curTabFound && indexToInsert ==-1 &&  this.fields[this.sorts[i]].tabId != field.tabId){
			indexToInsert = i;
		}
		if(this.sorts[i] == field.id)
			curIndex = i;
	}
	if(indexToInsert == -1)
		indexToInsert = this.sorts.length;
	
	this.sorts.splice(curIndex,1);
	if(curIndex < indexToInsert)
		indexToInsert--;	
	this.sorts.splice(indexToInsert,0, field.id);
}

Bitrix.IBlockSettingsSelector.prototype.copyField = function()
{

	var index = this.defaultFieldsContainer.selectedIndex;
	if(index < 0)
		return;
		
	var selIndex = this.selectedTabsContainer.selectedIndex;
	if(selIndex < 0)
		return;
		
	var defSelIndex = this.defaultTabsContainer.selectedIndex;
	if(defSelIndex < 0)
		return;
	var optId,opt;
	var optsToMove = [];
	var count = 0;
	for(var i = 0; i < this.defaultFieldsContainer.options.length;i++)
	{
		opt = this.defaultFieldsContainer.options[i];
		if(opt.selected){
			optsToMove[count] = opt;
			count++;
			}
	}

	for(var i = 0; i < optsToMove.length; i++)
		this.moveField(optsToMove[i], selIndex);
}

Bitrix.IBlockSettingsSelector.prototype.create = function(id)
{
	var select = document.getElementById(id);
	var curTabIndex = this.selectedTabsContainer.selectedIndex;

	if(curTabIndex < 0)
		return;
	if(!select)
		return;
	var result = window.prompt('<%= GetMessage("Message.EnterNewSectionName") %>', "");
	if(!result)
		return;
	var resText = this.trim(result);
	if(resText=="")
		return;
		var opt = document.createElement("OPTION");
		opt.value=this.getUniqueId();
		if(select.id==this.selectedFieldsContainer.id)
			opt.text ="--"+ resText;
		else 
			opt.text = result;
		select.options.add(opt);
	
	var newField = {};
	newField.id = opt.value;
	newField.name = opt.text;
	newField.selected = true;
	newField.tabId = this.selectedTabsContainer.options[curTabIndex].value;
	newField.isSect = true;
	this.fields[opt.value] = newField;
	var curTabFound = false;
	var indexToInsert = -1;

	for(var i=0;i<this.sorts.length;i++)
	{
		if(!curTabFound && this.fields[this.sorts[i]].tabId == newField.tabId)
			curTabFound = true;
		if(curTabFound &&  this.fields[this.sorts[i]].tabId != newField.tabId){
			indexToInsert = i;
			break;
		}
	}
	if(indexToInsert == -1)
		indexToInsert = this.sorts.length;
		
	this.sorts.splice(indexToInsert,0, newField.id);
}
Bitrix.IBlockSettingsSelector.prototype.createTab = function()
{
	var result = window.prompt('<%= GetMessage("Message.EnterNewTabName")%>', "");
	if(result)
	{
		var resText = this.trim(result);
		if(resText=="")
			return;
		var opt = document.createElement("OPTION");
		opt.value=this.getUniqueId();
		opt.text = resText;
		this.selectedTabsContainer.options.add(opt);
	}
}

Bitrix.IBlockSettingsSelector.prototype.validate = function()
{
	var err = this.checkRequiredAreSelected();

	if(err == "")
		return true;
	alert(err);
	return false;
}

Bitrix.IBlockSettingsSelector.prototype.checkRequiredAreSelected = function()
{
	var result = "";

	for(var field in this.fields)
	{
		if(!this.fields.hasOwnProperty(field))
			continue;
			
		if(this.fields[field].required && !this.fields[field].selected)
			result+=this.fields[field].name+", ";
	}
			
	if(result.length>1)
		result = '<%= GetMessage("Error.RequiredFieldMustBeSelected") %>'+" "+ result.substring(0, result.length-2);
	return result;
}

Bitrix.IBlockSettingsSelector.prototype.removeField = function(opt)
{
	this.fields[opt.value].selected = false;
	this.fields[opt.value].tabId = this.fields[opt.value].defTabId;
	this.selectedFieldsContainer.removeChild(opt);
	
	
}

Bitrix.IBlockSettingsSelector.prototype.removeTab = function(opt)
{
	this.selectedTabsContainer.removeChild(opt);	
	for(var field in this.fields)
	{
		if(!this.fields.hasOwnProperty(field))
			continue;
		var f = this.fields[field];
		if(f.tabId == opt.value){
			f.tabId = f.defTabId;
			f.selected = false;
					
		}
	}
}

Bitrix.IBlockSettingsSelector.prototype.del = function(id)
{
	var select = document.getElementById(id);

	var tabIndex = this.selectedTabsContainer.selectedIndex;
	if(tabIndex < 0)
		return;
	var val = null;

	var tabVal = this.selectedTabsContainer.options[tabIndex].value;	
	var count = 0;
	var optToDel = [];
	if(select.selectedIndex>=0)
	{
		for(var i = 0; i < select.options.length;i++)
		if(select.options[i].selected){
			optToDel[count] = select.options[i];
			count++;
		}
		
		if(select.id == this.selectedTabsContainer.id)
		{
			for(var i=0;i<optToDel.length;i++)
				this.removeTab(optToDel[i]);
		}
		else if(select.id == this.selectedFieldsContainer.id)
		{
			for(var i=0;i<optToDel.length;i++)
				this.removeField(optToDel[i]);
		}
	}
	
	this.refresh("defaultTab");
	this.refresh("selectedTab");
}

Bitrix.IBlockSettingsSelector.prototype.edit = function(id)
{
	var select = document.getElementById(id);
	if(!select)
		return;
		
	var index = select.selectedIndex;
	if(index < 0)
		return;
	var txtValue = select.options[index].text;
	var isSect = false;
	if(select == this.selectedFieldsContainer){

		isSect = this.fields[select.options[index].value].isSect;

		if(isSect)
			txtValue = txtValue.substring(2);
	}
	var result = window.prompt('<%= GetMessage("Message.EditName") %>', txtValue);
	if(result)
	{
		if(isSect)
			result="--"+result;
		select.options[index].text = result;
		if(select == this.selectedFieldsContainer){
			this.fields[select.options[index].value].editedName = result;
			}
	}
}


Bitrix.IBlockSettingsSelector.prototype.getUniqueId = function()
{
	var newDate = new Date;
    return newDate.getTime();
}

Bitrix.IBlockSettingsSelector.prototype.moveUp = function(selectId) {
	var select = document.getElementById(selectId);
	
	if(!selectId)
		return;
		
	if(!select.options)
		return;
		
	var index = select.selectedIndex;
	if(index <= 0)
		return;
		
	var opt = select.options[index];
	var upper = select.options[index-1];
	select.insertBefore(opt, upper);

	for(var i=0;i<this.sorts.length;i++)
		if(this.sorts[i] == opt.value){
			
			this.sorts.splice(i,1);
			this.sorts.splice(i-1, 0, opt.value);
			break;
		}
}

Bitrix.IBlockSettingsSelector.prototype.moveDown = function(selectId) {
	var select = document.getElementById(selectId);
	
	if(!selectId)
		return;
		
	if(!select.options)
		return;
		
	var index = select.selectedIndex;
	if(index < 0 || index == select.options.length-1)
		return;
		
	var opt = select.options[index];
	var upper = select.options[index+1];
	select.insertBefore(opt, upper.nextSibling);
	
	
	for(var i=0;i<this.sorts.length;i++)
		if(this.sorts[i] == opt.value){
			
			this.sorts.splice(i,1);
			this.sorts.splice(i+1,0, opt.value);
			break;
		}
}

Bitrix.IBlockSettingsSelector.prototype.refresh = function(target)
{
	var selected;
	var index;
	if(target == "selectedTab")
		index = this.selectedTabsContainer.selectedIndex;
	else
		index = this.defaultTabsContainer.selectedIndex;
	
	if(index < 0)
		return;
	
	if(target == "selectedTab"){
		selected = this.selectedTabsContainer.options[index];
		this.selectedFieldsContainer.options.length=0;
		}
	else{
		selected = this.defaultTabsContainer.options[index];
		this.defaultFieldsContainer.options.length=0;
		}
	
	for(var i=0;i<this.sorts.length;i++){
		
		var f = this.fields[this.sorts[i]];
		if(f.tabId == selected.value && ((f.selected && target == "selectedTab") ||(!f.selected && target == "defaultTab")))
		{
			var opt = document.createElement("OPTION");
			opt.text = f.name;
			opt.value= f.id;
			if( target == "selectedTab")
				this.selectedFieldsContainer.options.add(opt);
			else
				this.defaultFieldsContainer.options.add(opt);
		}
	}
	
}

Bitrix.IBlockSettingsSelector.prototype.tryRecData = function(id, tabid)
{

	if(!this.validate())
		return false;
	
	this.recData(id, tabid);
	return true;
}

Bitrix.IBlockSettingsSelector.prototype.recData = function(id, tabid)
{

	var field = document.getElementById(id);
	if(!field)
		return;
	
	var tabField = document.getElementById(tabid);
	if(!tabField)
		return;
	var s="";

	for(var i=0;i<this.sorts.length;i++)
	{
		var f = this.fields[this.sorts[i]];
		if(!f.selected)
			continue;

		var name = f.name;
		if(f.hasOwnProperty("editedName"))
			name = f.editedName;
		if(!f.isSect && name.indexOf("_") == 0)
			name = name.substring(1);
		else if(f.isSect)
			name = name.substring(2);
			
		s+=f.id+","+Bitrix.HttpUtility.htmlEncode(name)+","+f.tabId+"," + f.isSect+",";	
	}
	s = s.substring(0, s.length-1);

	field.value = s;
	var options = this.selectedTabsContainer.options;
	s="";
	for(var i=0;i<options.length; i++)
	{
		s+=options[i].value+","+options[i].text+",";
	}
	s = s.substring(0, s.length-1);
	tabField.value = s;
}


var selector;
var fields;


BX.ready(function() {

	var selectorParams = {};
	selectorParams.defaultTabsContainer = document.getElementById("available_tabs");
	selectorParams.selectedTabsContainer = document.getElementById("selected_tabs");
	selectorParams.selectedFieldsContainer = document.getElementById("selected_fields");
	selectorParams.defaultFieldsContainer = document.getElementById("available_fields");
	selectorParams.defaultTabs = <%=  GetTabsJSON() %>;
	selectorParams.defaultFields = <%= GetFieldsJSON() %>;
	selectorParams.fields=<%=GetFieldsJSON() %>;
	selector = new Bitrix.IBlockSettingsSelector(selectorParams);
	selector.Init();
	
	
});
</script>
</form>
 </body>
 </html>
