function copyObj(obj)
{
	var res = {};
	for (var i in obj) 
	{
		if (typeof obj[i] == 'object') 
			res[i] = copyObj(obj[i])
		else 
			res[i] = obj[i];
	}
	return res;
}

function htmlEncode(html)
{
	if (!html)
		return '';
	// Сначала необходимо заменить & 
	html = html.replace(/&/g, "&amp;");
	// А затем всё остальное в любой последовательности 
	html = html.replace(/</g, "&lt;");
	html = html.replace(/>/g, "&gt;");
	html = html.replace(/"/g, "&quot;");
	// Возвращаем полученное значение 
	return html;
}

function BXSecurityUI(rolesData, operationsData, initialState, controls, options, localization)
{
	var _this = this;
	var selectedRole = null;
	
	for(var r in rolesData)
		rolesData[r].loweredTitle = rolesData[r].title.toLowerCase();
	
	var rolesTable = controls.rolesTable;
	var operationsTable = controls.operationsTable;
	var rolesDiv = jsUtils.FindParentObject(rolesTable, 'div', 'roles-container');
	//var intelBox = document.getElementById(intelBoxId);
	
	var intelDropDown = controls.intelDropDown;
	
	var intelDiv = document.createElement('div');
	intelDiv.className = 'BXSecurityUI-intel-popup';
	intelDiv.style.display = 'none';
	
	var intelContainer = document.createElement('div');
	intelContainer.className = 'BXSecurityUI-intel-popup-container';
	intelDiv.appendChild(intelContainer);
	
	var intelDivContainer = jsUtils.FindContainingBlock(intelDropDown);
	
	setTimeout(function()
	{
		intelDivContainer.appendChild(intelDiv);
	}, 1);
	
	
//	var intelDiv = document.getElementById(intelPopupId);
//	var intelContainer = document.getElementById(intelContainerId);
	
	var state = {};
	var rolesRows = {};
	var operationsChecks = {};
	var stateImages = ['tick-gray', 'cross-gray', 'cross', 'tick', 'question'];
	
	var defaultState = options.defaultState;
	if (isNaN(defaultState))
		defaultState = 1;
	var defaultInheritedState = options.defaultInheritedState;
	if (isNaN(defaultInheritedState))
		defaultInheritedState = 0;
	
	var isAllowed = {};
	isAllowed[-1] = (options.allowedStates & 1) > 0;
	isAllowed[0] = (options.allowedStates & 2) > 0;
	isAllowed[1] = (options.allowedStates & 4) > 0;
	
	
	this.AddRole = function(roleId)
	{
		if (!rolesData[roleId]) 
			return;
		if (state[roleId]) 
			return;
		state[roleId] = {};
		for (var op in rolesData[roleId].operations) 
			state[roleId][op] = _GetRoleState(roleId, op);
		
		var roleRow = rolesTable.insertRow(-1);
		roleRow.style.cursor = 'default';
		roleRow.onclick = function()
		{
			_this.SelectRole(roleId);
		}
		roleRow.roleId = roleId;
		roleRow.title = rolesData[roleId].title;
		rolesRows[roleId] = roleRow;
		
		/* delete cell */
		var deleteCell = roleRow.insertCell(-1);
		deleteCell.className = 'delete-cell';
		
		var extenderDeleteDiv = document.createElement('div');
		extenderDeleteDiv.className = 'delete-div';
		deleteCell.appendChild(extenderDeleteDiv);
		
		var deleteImg = document.createElement('div');
		deleteImg.className = 'delete-button';
		deleteImg.title = localization.deleteRoleButtonToolTip;
		deleteImg.onmouseover = function()
		{
			this.className='delete-button delete-button-selected';
		};
		deleteImg.onmouseout = function()
		{
			this.className='delete-button';
		};
		deleteImg.onclick = function()
		{
			var roleId = jsUtils.FindParentObject(this, 'tr').roleId;
			if (!confirm(localization.deleteConfirmation.replace(/#ROLE#/, rolesData[roleId].title)))
				return;
			_this.DeleteRole(roleId);
		}
		extenderDeleteDiv.appendChild(deleteImg);
		/* end of delete cell */
		
		
		/* image cell*/
		var imgCell = roleRow.insertCell(-1);
		imgCell.className = 'image-cell';
		imgCell.style.textAlign = 'center';
				
		var extenderImgDiv = document.createElement('div');
		extenderImgDiv.className = 'image-div ' +  (rolesData[roleId].icon ? rolesData[roleId].icon : 'default-image');
		//extenderImgDiv.className = 'image-div';
		//extenderImgDiv.style.width = '42px';
		//extenderImgDiv.style.height = '32px';
		imgCell.appendChild(extenderImgDiv);
				
		//var img = document.createElement('div');
		//img.className = 'image-div ' +  (rolesData[roleId].icon ? rolesData[roleId].icon : 'default-image');
		//extenderImgDiv.appendChild(img);
		/* end of image cell*/
		
		var textCell = roleRow.insertCell(-1);
				
		var titleText = document.createElement('p');
		titleText.className = 'text-title';
		titleText.innerHTML = htmlEncode(rolesData[roleId].title);
		textCell.appendChild(titleText);
		
		var descText = document.createElement('p');
		descText.className = 'text-desc';
		descText.innerHTML = htmlEncode(rolesData[roleId].description);
		textCell.appendChild(descText);
		
		
	}
	this.DeleteRole = function(roleId)
	{
		if (!rolesData[roleId]) 
			return;
		if (!state[roleId]) 
			return;
		
		if (selectedRole == roleId)
			this.SelectRole(null);
		
		rolesTable.deleteRow(rolesRows[roleId].rowIndex);
		delete rolesRows[roleId];
		delete state[roleId];
	}
	this.SelectRole = function(roleId)
	{
		if (roleId == selectedRole)
			return;
		
		if (options.showNotes)
			controls.notesContainer.style.display = 'none';
			
		if (roleId == null && selectedRole != null) 
		{
			rolesRows[selectedRole].className = '';
			selectedRole = null;
			for (var op in operationsChecks) 
				this.SetOperation(op, null);
			return;
		}
		
		if (!state[roleId] || !rolesRows[roleId]) 
			return;
		
		if (selectedRole) 
			rolesRows[selectedRole].className = '';
		
		rolesRows[roleId].className = "role-selected";
		selectedRole = roleId;
		for (var op in operationsChecks) 
		{
			this.SetOperation(op, state[roleId][op]);
			var inheritedState = _GetRoleInheritedState(roleId, op);
			var initState = _GetRoleState(roleId, op);
			if ((initState == -1) || (initState == 2))
				operationsChecks[op].afterInherit = 1 - inheritedState;
			else if (initState == inheritedState)
				operationsChecks[op].afterInherit = inheritedState;
			else 
				operationsChecks[op].afterInherit = 1 - initState;
		}
		if (options.showNotes && rolesData[roleId].operations)
		{
			var notes = '';
			var ops = rolesData[roleId].operations;
			for(var op in ops)
				if (ops[op].note)
				{
					notes += '<b>' + htmlEncode(operationTitles[op]) + ':</b>';
					notes += '<div style="margin-left: 24px" >' + ops[op].note + '</div>'
				}
			if (notes.length > 0)
			{
				controls.notesText.innerHTML = notes;
				controls.notesContainer.style.display = 'block';
			}
		}
	}
	this.ClickOperation = function(operationId)
	{
		if (selectedRole == null) 
			return;
		var img = operationsChecks[operationId];
		var curState = img.state;
		if (isNaN(curState) || curState < -1 || curState > 2) 
			curState = -1;
		//var inherited = rolesData[selectedRole].operations[operationId].inheritedState;
		var dontModify = false;
		if (rolesData[selectedRole].operations && rolesData[selectedRole].operations[operationId])
 			dontModify = rolesData[selectedRole].operations[operationId].allowDontModify;
		
		var after = operationsChecks[operationId].afterInherit;

		var statesOrder = [];
		if (isAllowed[-1]) statesOrder.push(-1);
		if (isAllowed[after]) statesOrder.push(after);
		if (isAllowed[1 - after]) statesOrder.push(1 - after);
		if (dontModify) statesOrder.push(2);

		var newState = statesOrder[0];
		for(var i = 0; i < statesOrder.length; i++)
			if (statesOrder[i] == curState)
			{
				newState = statesOrder[(i + 1) % statesOrder.length];
				break;				
			}
		this.SetOperation(operationId, newState);
	}
	this.SetOperation = function(operationId, value)
	{
		var img = operationsChecks[operationId];
		if (isNaN(value) || value < -1 || value > 2) 
			value = -1;
		var displayIndex;
		if (selectedRole != null)
		{
		 	displayIndex = value + 2;
			if (value == -1)
				displayIndex -= _GetRoleInheritedState(selectedRole, operationId);
		}
		else
			displayIndex = 1;
		
		img.state = value;
		img.alt = ['v', 'x', 'X','V', '?'] [displayIndex];
		img.src = options.imagesPath + '/' + stateImages[displayIndex] + '.gif';
		switch (displayIndex)
		{
			case 0:
				img.title = localization.operationInheritAllow;
				break;
			case 1:
				img.title = localization.operationInheritDeny;
				break;
			case 2:
				img.title = localization.operationDeny;
				break;
			case 3:
				img.title = localization.operationAllow;
				break;
			case 4:
				img.title = localization.operationDontModify;
				break;
			default:
				img.title = '';
		}
		if (selectedRole != null)
			state[selectedRole][operationId] = value;
	}
	this.ShowIntel = function()
	{
		var div = intelDiv;
		if (div.style.display != 'none') 
			return;
		
		setTimeout(function()
		{
			jsUtils.addEvent(document, "keypress", _IntelKeypress);
			jsUtils.addEvent(document, "click", _IntelClick);
		}, 1);
		
		var cpos = jsUtils.GetRealPos(intelDivContainer);
		var cleft = cpos ? cpos.left : 0;		
		var ctop = cpos ? cpos.top : 0;
				
		var pos = jsUtils.GetRealPos(intelDropDown);
		div.style.top = pos.bottom + 1 - ctop + 'px'
		div.style.left = pos.left + 1 - cleft + 'px'
		div.style.display = 'block';
	}
	this.HideIntel = function()
	{
		var div = intelDiv;
		if (div.style.display == 'none') 
			return;
		
		div.style.display = 'none';
		
		jsUtils.removeEvent(document, "keypress", _IntelKeypress);
		jsUtils.removeEvent(document, "click", _IntelClick);
	}
	this.ProcessIntel = function(e, textbox)
	{
		if (!e) 
			e = window.event
		if (!e) 
			return;
		if (e.keyCode == 27) 
			return;
		
		var input = textbox.value.toLowerCase();
		
		if (input.length < 1) 
		{
			this.HideIntel();
			return;
		}
		
		_ClearIntel();
		for (var roleId in rolesData) 
			if (rolesData[roleId].loweredTitle.indexOf(input) != -1 && !state[roleId]) 
				_AddRoleToIntel(roleId);
		this.ShowIntel();
	}
	this.ShowAllIntel = function(all)
	{
		_ClearIntel();
		for (var roleId in rolesData) 
		{
			//if (!(all || !state[roleId]))
			//	alert(rolesData[roleId].title);
			if (all || !state[roleId]) 
				_AddRoleToIntel(roleId);
		}
		this.ShowIntel();
	}
	this.PrepareTextBox = function(textbox)
	{
		textbox.onfocus = function()
		{
			textbox.value = '';
			textbox.style.textAlign = '';
			textbox.style.color = '';
		}
		textbox.onblur = function()
		{
			textbox.value = localization.emptyTextboxValue;
			textbox.style.textAlign = 'center';
			textbox.style.color = 'Gray';
		}

		textbox.value = localization.emptyTextboxValue;
		textbox.style.textAlign = 'center';
		textbox.style.color = 'Gray';
	}
	this.PersistState = function()
	{
		var persisted = '';
		var count = 0;
		for(var role in state)
		{
			count++;
			if (typeof(role) == 'number')
				persisted += 'i:' + role + ';';
			else 
				persisted += 's:"' + role.replace(/[\\"]/g, '\\$&') + '";';
			
			var innercount = 0;
			var innerpersisted = '';
			for (var op in state[role])
			{
				innercount++;
				if (typeof(op) == 'number')
					innerpersisted += 'i:' + op + ';';
				else 
					innerpersisted += 's:"' + op.replace(/[\\"]/g, '\\$&') + '";';
				innerpersisted += 'i:' + state[role][op] + ';';
			}
			persisted += 'd:' + innercount + ':{' + innerpersisted + '};';
		}
		persisted = 'd:' + count + ':{' + persisted + '};';
		return persisted;
		//return _oldSubmit ? _oldSubmit() : true;
	}
	
	var _GetRoleState = function(roleId, operationId)
	{
		if (rolesData[roleId].operations[operationId] != null)
			return  rolesData[roleId].operations[operationId].state;
		return defaultState;		
	}
	var _GetRoleInheritedState = function(roleId, operationId)
	{
		if (rolesData[roleId].operations[operationId] != null)
			return  rolesData[roleId].operations[operationId].inheritedState;
		return defaultInheritedState;		
	}
	var _ClearIntel = function()
	{
		intelContainer.innerHTML = '';
	}
	var _AddRoleToIntel = function(roleId)
	{
		var div = document.createElement('div');
		div.className ="intel-popup-rowdiv"
		
		div.onmouseover = function()
		{
			for (var i = 0; i < intelContainer.childNodes.length; i++) 
			{
				var r = intelContainer.childNodes[i];
				r.className = (r == this) ? "intel-popup-rowdiv selected" : "intel-popup-rowdiv";
			}
		};
		div.onclick = function()
		{
			_this.AddRole(roleId);
			_this.SelectRole(roleId);
			setTimeout(function()
			{
				rolesDiv.scrollTop = rolesRows[roleId].offsetTop;//scrollIntoView(false);
			}, 0);
		};
		div.roleId = roleId;
		div.innerHTML = htmlEncode(rolesData[roleId].title);
		intelContainer.appendChild(div);
	}
	var _IntelKeypress = function(e)
	{
		if (!e) 
			e = window.event
		if (!e) 
			return;
		if (e.keyCode == 27) 
			_this.HideIntel();
	}
	var _IntelClick = function(e)
	{
		_this.HideIntel();
	}
	var _AddSeparator = function(separator)
	{
		if (operationsTable.rows.length != 0) 
		{
			var emptyRow = operationsTable.insertRow(-1)
			
			var emptyCell = emptyRow.insertCell(-1);
			emptyCell.colSpan = 2;
			emptyCell.className = 'empty';
		}
		var separatorRow = operationsTable.insertRow(-1)
						
		var titleCell = separatorRow.insertCell(-1);
		titleCell.colSpan = 2;
		titleCell.title = separator.title;
		titleCell.className = 'separator';
		titleCell.innerHTML = '<p style="margin:0pt !important; padding:0pt !important;">' + htmlEncode(separator.title) + '</p>';
		
		var emptyRow = operationsTable.insertRow(-1)
						
		var emptyCell = emptyRow.insertCell(-1);
		emptyCell.colSpan = 2;
		emptyCell.className = 'empty';
	}
	
	var _AddOperation = function(operation)
	{
		if (operationsTable.rows.length == 0) 
		{
			var emptyRow = operationsTable.insertRow(-1)
			
			var emptyCell = emptyRow.insertCell(-1);
			emptyCell.colSpan = 2;
			emptyCell.className = 'empty';
		}
		
		operationTitles[operation.id] = operation.title;
		
		var operationRow = operationsTable.insertRow(-1)
		//operationRow.style.height = '30px';
		
		
		
		var checkCell = operationRow.insertCell(-1);
		
		checkCell.className = 'check';
		
		var img = document.createElement('img');
		img.operationId = operation.id;
		img.onclick = function()
		{
			_this.ClickOperation(operation.id);
		}
		checkCell.appendChild(img);
		operationsChecks[operation.id] = img;
				
		var titleCell = operationRow.insertCell(-1);
		titleCell.className = 'operation';
		titleCell.title = operation.title;
		
		titleCell.innerHTML = '<p style="margin:0pt !important; padding:0pt !important;">' + htmlEncode(operation.title) + '</p>';
		titleCell.onclick = function()
		{
			_this.ClickOperation(operation.id);
		}
	}
	
	var operationTitles = {};
	for (var i = 0; i < operationsData.length; i++)
	{
		var op = operationsData[i]
		if (op.separator) 
			_AddSeparator(op);
		else 
		{
			_AddOperation(op);
			this.SetOperation(op.id, null);
		}
	}
	if (initialState != null)
	{
		var firstId = null;
		for(var roleId in initialState)
		{
			if (firstId == null)
				firstId = roleId;
			this.AddRole(roleId);
			for(var operationId in initialState[roleId])
				state[roleId][operationId] = initialState[roleId][operationId];
		}
		if (firstId != null)
			this.SelectRole(firstId);
	}
	//var	_oldSubmit = theForm.onsubmit;
	//theForm.onsubmit = _PersistState;
}



if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 