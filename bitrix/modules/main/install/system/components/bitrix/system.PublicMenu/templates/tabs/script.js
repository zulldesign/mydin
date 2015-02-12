var jshover = function() {
	var sfElsTmp = document.getElementById("blue-tabs-menu");
	if (sfElsTmp)
	{
		var sfEls = document.getElementById("blue-tabs-menu").getElementsByTagName("li");
		for (var i=0; i<sfEls.length; i++) 
		{
			sfEls[i].onmouseover=function()
			{
				this.className+=" jshover";
			}
			sfEls[i].onmouseout=function() 
			{
				this.className=this.className.replace(new RegExp(" jshover\\b"), "");
			}
		}
	}
}

if (window.attachEvent) 
  window.attachEvent("onload", jshover);