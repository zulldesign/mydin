<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AdminWizardHost.ascx.cs" Inherits="Bitrix.UI.AdminWizardHost" %>
<%@ Import Namespace="System.IO" %>
<% if (Request.QueryString["inplace"] == null) { %>
	<% 
		string fullUrl = Url;
		Uri url;
		if (Uri.TryCreate(Request.Url, fullUrl, out url))
			fullUrl = url.AbsoluteUri;
	%>
<script type="text/javascript">
	function WizardSubmit(action, raw)
	{
		for(var key in WizardSubmitSubscribers){
			var s = WizardSubmitSubscribers[key];
			if(s && typeof(s) == "function") s();
		}

		var iframe = WizardCreateIFrame("wizard_inplace_iframe");
		document.body.appendChild(iframe);
		
		var form = document.getElementById("wizard_form");
		form.action = '<%= JSEncode(Url) %><%= (Url.IndexOf('?') == -1) ? "?inplace=" : "&inplace=" %>'
		form.target = "wizard_inplace_iframe";
		
		var input = document.createElement("INPUT");
		input.type = 'hidden';
		input.name = ((action instanceof String) || (typeof action == 'string')) ? (raw ? action : ('action$' +  action)) : action.name;
		form.appendChild(input);
		
		form.submit();
		
		form.removeChild(input);
		delete input;
		return false;
	}
	WizardSubmitSubscribers = {}
	function WizardOnSubmit(iframe, data)
	{
		var form = document.getElementById("wizard_form");
		form.target = '';
		form.action = '<%= JSEncode(Url) %>';
		
		
		document.body.removeChild(iframe);
		delete iframe;
		
		if (data.type == 'view')
		{
			if (data.title != null)
				InsertTitle(data.title);
			if (data.html != null)
				InsertHTML(data.html);
			if (data.buttons != null)
				InsertButtons(data.buttons);
			if (data.navigation != null)
				InsertNavigation(data.navigation);
			InsertErrors(data.errors);
			
			if (data.script != null && data.script != '')
			{
				window.setTimeout(function() 
				{
					 if (window.execScript) 
					 {
						window.execScript(data.script);
						return;
					}
					
					(function() 
					{
						window.eval.call(window, data.script);
					})
					();
    			}, 0);
			}		
		}
		else if (data.type == 'finish')
		{
			var endForm = document.createElement('FORM');
			endForm.style.display = 'none';
			endForm.method = 'POST';
			endForm.action = '<%= JSEncode(fullUrl) %>';
			document.body.appendChild(endForm);
			
			if (data.state != null)
			{
				var state = document.createElement('INPUT');
				state.type = 'hidden';
				state.name = 'wizard$state';
				state.value = data.state;
				endForm.appendChild(state);
			}
			
			if (data.token != null)
			{
				var token = document.createElement('INPUT');
				token.type = 'hidden';
				token.name = '<%= JSEncode(Bitrix.Security.BXCsrfToken.TokenKey) %>';
				token.value = data.token;
				endForm.appendChild(token);
			}
			
			endForm.submit();
			document.body.removeChild(endForm);
		}
	}
	function WizardCreateIFrame(frameId)
	{
		var io;
		if(window.ActiveXObject) 
		{
			var io = document.createElement('<iframe id="' + frameId + '" name="' + frameId + '" />');
			io.src = 'javascript:void(0)';
		}
		else 
		{
			io = document.createElement('iframe');
			io.id = frameId;
			io.name = frameId;
		}
		
		//Don't set display: none because of Safari
		io.style.position = 'absolute';
		io.style.top = '-1000px';
		io.style.left = '-1000px';
		io.style.width = "1px";
		io.style.height = "1px";
		return io;
	}
</script>
<%= Bitrix.Security.BXCsrfToken.RenderAsHiddenField() %>

<% if (storeState != null) { %>
<input type="hidden" name="wizard$state" value="<%= storeState %>" />
<% } %>

<asp:PlaceHolder ID="HtmlView" runat="server"/>
	<% if (view.AutoRedirect) { %>
<script type="text/javascript">
	window.setTimeout(function() 
	{ 
		WizardSubmit('<%= view.RedirectAction %><% if (!string.IsNullOrEmpty(view.RedirectStep)) { %>$<%= view.RedirectStep %><% } %>'); 
	}, 0);
</script>
	<% } %>
<% } else { %>
<% if (doFinish) { %>
<script type="text/javascript">
var data = 
{
	type: 'finish',
	state: <%= storeState != null ? "'" + JSEncode(storeState) + "'" : "null" %>,
	token: '<%= JSEncode(Bitrix.Security.BXCsrfToken.GenerateToken()) %>'
}
</script>
<% } else { %>
<%
	string resultHtml;
	string resultScript;
	using (StringWriter w = new StringWriter())
	using (HtmlTextWriter h = new HtmlTextWriter(w)) 
	{ 
		Bitrix.Security.BXCsrfToken.RenderAsHiddenField(h);
		if (storeState != null)
			h.Write(@"<input type=""hidden"" name=""wizard$state"" value=""{0}"" />", storeState);
		
		HtmlView.RenderControl(h); 
		string s = w.ToString();
		StringBuilder html = null;
		StringBuilder script = null;	
		int start = 0;	
		for(Match m = Regex.Match(s, @"<script[^>]*>(.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline); m.Success; m = m.NextMatch())
		{
			if (start == 0)
			{
				html = new StringBuilder();
				script = new StringBuilder();	
			}		
			if (m.Index > start)
				html.Append(s, start, m.Index - start);
			start = m.Index + m.Length;
			script.Append(m.Groups[1].Value);		
			script.Append(';');
		}
		if (start == 0)
		{
			resultHtml = s;
			resultScript = "";
		}
		else 
		{	
			if (s.Length > start)
				html.Append(s, start, s.Length - start);
			resultHtml = html.ToString();
			resultScript = script.ToString();
		}
	}
%>
<script type="text/javascript">
var data = 
{
	type: 'view',
	
	title: '<%= JSEncode(WizardStepTitleHtml) %>',
	
	html: '<%= JSEncode(resultHtml) %>',
	
	script: '<%= JSEncode(resultScript) %><% if (view.AutoRedirect) { %>; window.setTimeout(function() { WizardSubmit(\'<%= view.RedirectAction %><% if (!string.IsNullOrEmpty(view.RedirectStep)) { %>$<%= view.RedirectStep %><% } %>\'); }, 0);<% } %>',
	
	<% if (WizardStepErrorHtml != null ) { %>
	errors: [
	<% 
	bool first = true;
	foreach(string e in WizardStepErrorHtml) { %>
		<% if (!first) { %>,<% } %>
		'<%= JSEncode(e) %>'
	<% 
		first = false; 
	} 
	%>
	],
	<% } %>
	
	<% 
	using (StringWriter w = new StringWriter())
	using (HtmlTextWriter h = new HtmlTextWriter(w)) { 
		renderButtons(h, this); 
		string s = w.ToString();
	%>
	buttons: '<%= JSEncode(s) %>',
	<% } %>
	
	<% 
	using (StringWriter w = new StringWriter())
	using (HtmlTextWriter h = new HtmlTextWriter(w)) { 
		RenderNavigation(h, this); 
		string s = w.ToString();
	%>
	navigation: '<%= JSEncode(s) %>'
	<% } %>
};
</script>
<% } %>
<script type="text/javascript">
if (parent && parent.WizardOnSubmit)
	parent.WizardOnSubmit(window.frameElement, data);
</script>
<% } %>