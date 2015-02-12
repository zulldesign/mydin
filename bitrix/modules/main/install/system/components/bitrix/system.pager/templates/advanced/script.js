document.onkeydown = function PageNavigation(event)
{
   if (!document.getElementById)
      return;

   if (window.event)
      event = window.event;

   if (event.ctrlKey)
   {
      var key = (event.keyCode ? event.keyCode : (event.which ? event.which : null) );
      if (!key)
         return;

      var link = null;
      if (key == 37)
         link = document.getElementById('bx_next_page');
      else if (key == 39)
         link = document.getElementById('bx_previous_page');

      if (link && link.href)
         document.location = link.href;
   }
}

if (typeof(Sys) !== 'undefined') 
	Sys.Application.notifyScriptLoaded();