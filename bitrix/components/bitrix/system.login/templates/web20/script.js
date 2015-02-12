function ShowLoginForm(errorMessage)
{
	var div = document.getElementById("login-form-window");
	if (!div)
		return;
	div.style.display = "block";//.visibility = "visible";
	document.forms[0].appendChild(div);

	if (errorMessage) alert(errorMessage);
	//document.body.appendChild(div);
	return false;
}

function CloseLoginForm()
{
	var div = document.getElementById("login-form-window");
	if (!div)
		return;

	div.style.display = "none";//.visibility = "hidden";
	return false;
}