<%@ Page Language="C#" EnableEventValidation="false" EnableViewState="false" EnableViewStateMac="false" Inherits="Bitrix.UI.BXPage" ValidateRequest="False"%>

<%@ OutputCache Duration="1" Location="None" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="System.Reflection" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Bitrix.Modules" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Components" %>
<%@ Import Namespace="Bitrix.Services" %>

<script runat="server" type="text/C#">
    string operation, langid, siteid;

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
		operation = Request.Params["action"];
		langid = Request.Params["lang"];
		siteid = Request.Params["site"];

        if (!String.IsNullOrEmpty(operation))
            operation = operation.ToLowerInvariant();
    }

    protected override void Render(HtmlTextWriter writer)
    {
        string vardata = String.Empty;
        switch (operation)
        {
            case "sitetemplateparams":
                vardata = ProcessSiteTemplateParams();
				
				writer.WriteLine("<script>");
				writer.WriteLine("window.bx_template_params = " + ProcessSiteTemplateParams());
				writer.WriteEndTag("script");

                break;
            default: break;
        }
    }

    protected string ProcessSiteTemplateParams()
    {
        string templateid = Request.Params["templateID"];
        if (String.IsNullOrEmpty(templateid)) return String.Empty;
        return GetConfigTemplateInfo(templateid);
    }

    protected virtual string GetConfigTemplateInfo(string templateID)
    {
        StringBuilder sb = new StringBuilder();

        string _content = String.Empty;
        string _path = String.Empty;
        string _styles = String.Empty;
        List<string> aspxContent = null;
        List<string> templateStyles = new List<string>();

        try
        {
            _path = Bitrix.BXSite.GetTemplatePath(templateID);
            _content = Bitrix.IO.BXSecureIO.FileReadAllText(_path);

            if (!String.IsNullOrEmpty(_content))
            {
                aspxContent = BXParser.ParseContentPlaceHolders(_content);
            }
            
			
			if (!String.IsNullOrEmpty(_path))
			{
				var csspath = _path;
				int ind = !string.IsNullOrEmpty(csspath) ? csspath.LastIndexOf('/') : -1;
				if (ind >= 0)
					csspath = csspath.Remove(ind);
				csspath = String.Concat(csspath, !csspath.EndsWith("/") ? "/" : null, Bitrix.Configuration.BXConfigurationUtility.Constants.TemplateStyleFileName);

				if (Bitrix.IO.BXSecureIO.FileExists(csspath))
				{
					string csscontent = Bitrix.IO.BXSecureIO.FileReadAllText(csspath);
					if (!String.IsNullOrEmpty(csscontent))
						templateStyles.Add(csscontent);
				}
			}
			
            _styles = String.Concat(templateStyles.ToArray());
        }
        catch { }

        sb.Append("{");
        sb.AppendFormat(" 'ID': '{0}'", templateID);
        sb.AppendFormat(", 'NAME': '{0}'", templateID);
        if (!String.IsNullOrEmpty(_styles))
            sb.AppendFormat(", 'STYLES': '{0}'", _styles.Replace('\n', ' ').Replace('\r', ' '));    
        if (aspxContent != null)
        {
            sb.Append(", 'CPH': [ ");
            int i = 0;
            foreach (string cph in aspxContent)
            {
                if (i > 0) sb.Append(", ");
                sb.AppendFormat("'{0}'", cph);
                i++;
            }
            sb.Append("]");
        }
        // unsupported: sb.AppendFormat(", 'CSS': {0}", GetConfigContentCss());
        sb.Append("}");


        return sb.ToString();
    }
</script>

