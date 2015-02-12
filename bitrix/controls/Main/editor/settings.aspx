<%@ Page Language="C#" EnableEventValidation="false" EnableViewState="false" EnableViewStateMac="false" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="System.Collections" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>
<%@ Import Namespace="Bitrix.Configuration" %>

<script runat="server" type="text/C#">
    string Target;
    string EdName;    
    bool ToolTips = false;
	bool VisEffects = false;
    
    public void displayJSOtherSettings(string edname, HtmlTextWriter writer)
    {
        writer.WriteLine("<script>");
        string tt = BXOptionManager.GetOptionString(edname, "show_tooltips"+edname, "Y");
        writer.WriteLine("window.__show_tooltips = " + ((tt == "N") ? "false" : "true") + ";");
		
        tt = BXOptionManager.GetOptionString(edname, "visual_effects"+edname, "Y");
        writer.WriteLine("window.__visual_effects = " + ((tt == "N") ? "false" : "true") + ";");
		writer.WriteEndTag("script");
    }

    protected void displayJSToolbars(string edname, HtmlTextWriter writer)
    {
        writer.WriteLine("<script>");
        writer.WriteLine("window.arToolbarSettings = [];");        
        
        string x = BXOptionManager.GetOptionString(edname, "toolbar_settings_" + edname, String.Empty);
        if (!String.IsNullOrEmpty(x))
        {
            string[] xx = x.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            if (xx != null)
            {                
                string block;
                for (int j = 0; j < xx.Length; j++)
                {
                    block = xx[j];
                    int i = block.IndexOf(":");
                    if (i == -1) continue;
                    string toolbar = block.Substring(0, i), show, dock, pos;
                    block = block.Substring(i+1);

                    i = block.IndexOf(","); if (i == -1) continue;
                    show = block.Substring(0, i); block = block.Substring(i + 1);

                    i = block.IndexOf(","); if (i == -1) continue;
                    dock = block.Substring(0, i); block = block.Substring(i + 1);

                    pos = block;

                    //displayJSToolbar(toolbar, show, dock, pos, writer);

					writer.WriteLine("var _ar = {};");
					writer.WriteLine("_ar.show = {0};", show);
					writer.WriteLine("_ar.docked = {0};", dock);
					writer.WriteLine("_ar.position = {0};	", pos);
					writer.WriteLine("window.arToolbarSettings['{0}'] = _ar;", toolbar);
                }
            }
        }

		writer.WriteLine("<" + "/script>");
    }
    
    protected void displayJSToolbar(string tb, string show, string dock, string pos, HtmlTextWriter w)
    {
        if (String.IsNullOrEmpty(tb) ||
            String.IsNullOrEmpty(show) ||
            String.IsNullOrEmpty(dock) ||
            String.IsNullOrEmpty(pos))
            return;
        
        w.WriteLine("<script>");
	    w.WriteLine("var _ar = {};");
	    w.WriteLine("_ar.show = {0};", show);
	    w.WriteLine("_ar.docked = {0};", dock);
		w.WriteLine("_ar.position = {0};	", pos);
		w.WriteLine("window.arToolbarSettings['{0}'] = _ar;", tb);
	    w.WriteLine("<"+"/script>");
    }

    protected void displayJSTaskbars(string edname, HtmlTextWriter writer)
    {
        writer.WriteLine("<script>");
        writer.WriteLine("window.arTaskbarSettings = [];");        
        writer.WriteLine("<" + "/script>");

        string x = BXOptionManager.GetOptionString(edname, "taskbar_settings_" + edname, String.Empty);
        if (!String.IsNullOrEmpty(x))
        {
            string[] xx = x.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            if (xx != null)
            {
                string block;
                for (int j = 0; j < xx.Length; j++)
                {
                    block = xx[j];
                    int i = block.IndexOf(":");
                    if (i == -1) continue;
                    string taskbar = block.Substring(0, i), show, dock, pos;
                    block = block.Substring(i + 1);

                    i = block.IndexOf(","); if (i == -1) continue;
                    show = block.Substring(0, i); block = block.Substring(i + 1);

                    i = block.IndexOf(","); if (i == -1) continue;
                    dock = block.Substring(0, i); block = block.Substring(i + 1);

                    pos = block;

                    displayJSTaskbar(taskbar, show, dock, pos, writer);
                }
            }
        }
    }

    protected void displayJSTaskbar(string tb, string show, string dock, string pos, HtmlTextWriter w)
    {
        if (String.IsNullOrEmpty(tb) ||
            String.IsNullOrEmpty(show) ||
            String.IsNullOrEmpty(dock) ||
            String.IsNullOrEmpty(pos))
            return;

		if (tb == "ASPXComponentsTaskbar")
			show = "true";
		
        w.WriteLine("<script>");
        w.WriteLine("var _ar = [];");
        w.WriteLine("_ar.show = {0};", show);
        w.WriteLine("_ar.docked = {0};", dock);
        w.WriteLine("_ar.position = {0};	", pos);
        w.WriteLine("window.arTaskbarSettings['{0}'] = _ar;", tb);
        w.WriteLine("<" + "/script>");
    }

	protected void unsetSettings(string edname, HtmlTextWriter writer)
    {	
		BXOptionManager.SetOptionString(edname, "toolbar_settings_" + edname, String.Empty);
		BXOptionManager.SetOptionString(edname, "taskbar_settings_" + edname, String.Empty);
		BXOptionManager.SetOptionString(edname, "taskbarset_settings_" + edname, String.Empty);		
		BXOptionManager.SetOptionString(edname, "show_tooltips" + edname, String.Empty);
		BXOptionManager.SetOptionString(edname, "visual_effects" + edname, String.Empty);
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        Target = Request.Params["Target"];
        EdName = Request.Params["EdName"];        
        if (Request.Params["tooltips"] == "Y") ToolTips = true;
		if (Request.Params["visual_effects"] == "Y") VisEffects = true;
    }
    
    protected override void Render(HtmlTextWriter writer)
    {        
        HandlePage(Target, EdName, writer);
    }
    
    protected virtual void HandlePage(string target, string edname, HtmlTextWriter writer)
    {
        if (String.IsNullOrEmpty(edname)) return;
        if (target == "get_all")
        {
            displayJSOtherSettings(edname, writer);
            displayJSToolbars(edname, writer);
            displayJSTaskbars(edname, writer);
        }
		else if (target == "toolbars")
		{
			setToolbarSettings(target, edname, writer);
		}
		else if (target == "taskbars")
		{
			setTaskbarSettings(target, edname, writer);
		}
		else if (target == "tooltips")
		{
			setTooltipsSettings(edname, writer);
		}
		else if (target == "visual_effects")
		{
			setVisualEffectsSettings(edname, writer);
		}
		else if (target == "unset")
		{
			unsetSettings(edname, writer);
		}	
    }

    protected Dictionary<string, string> getBaseList(string what)
    {
        int i;
        string x;
        Dictionary<string, string> tlist = new Dictionary<string, string>();
        
        foreach (string key in Request.Params)
        {
            if (key.StartsWith(what+"["))
            {
                x = key.Substring(what.Length + 1);
                if (!String.IsNullOrEmpty(x) && ((i = x.IndexOf("]"))>-1))
                {
                    x = x.Substring(0, i);
                    if (!String.IsNullOrEmpty(x) && !tlist.ContainsKey(x))
                        tlist.Add(x, "Y");
                }
            }
        }

        return tlist;
    }

    protected virtual void setTooltipsSettings(string edname, HtmlTextWriter writer)
    {
        BXOptionManager.SetOptionString(edname, "show_tooltips" + edname, (ToolTips) ? "Y" : "N");
    }

	protected virtual void setVisualEffectsSettings(string edname, HtmlTextWriter writer)
	{
		BXOptionManager.SetOptionString(edname, "visual_effects" + edname, (VisEffects) ? "Y" : "N");
	}
    
    protected virtual void setToolbarSettings(string target, string edname, HtmlTextWriter writer)
    {
        Dictionary<string, string> tlist = getBaseList("tlbrset");
        StringBuilder sb = new StringBuilder();

        bool r;
        string si1, si2, si3;
        int i1, i2, i3;
        foreach (string key in tlist.Keys)
        {
            if (Request.Params["tlbrset[" + key+"][show]"] != null)
            {
                r = (Request.Params["tlbrset["+key+"][show]"]=="true")?true:false;
                sb.Append(key+":"+r.ToString().ToLowerInvariant());
                if (Request.Params["tlbrset["+key+"][docked]"] != null)
                {
                    r = (Request.Params["tlbrset[" + key + "][docked]"] == "true") ? true : false;
                    sb.Append("," + r.ToString().ToLowerInvariant());
                    if (Request.Params["tlbrset[" + key + "][position][0]"] != null &&
                        Request.Params["tlbrset[" + key + "][position][1]"] != null &&
                        Request.Params["tlbrset[" + key + "][position][2]"] != null)
                    {
                        r = true;
                        si1 = Request.Params["tlbrset[" + key + "][position][0]"];
                        si2 = Request.Params["tlbrset[" + key + "][position][1]"];
                        si3 = Request.Params["tlbrset[" + key + "][position][2]"];
                        if (si1.EndsWith("px")) si1 = si1.Substring(0, si1.Length - 1);
                        if (si2.EndsWith("px")) si2 = si2.Substring(0, si2.Length - 1);                        
                        if (si3.EndsWith("px")) si3 = si3.Substring(0, si3.Length - 1);
                        r &= Int32.TryParse(si1, out i1);
                        r &= Int32.TryParse(si2, out i2);
                        r &= Int32.TryParse(si3, out i3);
                        if (r)
                        {
                            sb.AppendFormat(",[{0},{1},{2}]", i1, i2, i3);
                        }
                    }
                }
                sb.Append("||");
            }
        }
		
        if (sb.Length > 2)
				sb.Remove(sb.Length-2, 2);

		BXOptionManager.SetOptionString(edname, "toolbar_settings_" + edname, sb.ToString());        
    }

    protected virtual void setTaskbarSettings(string target, string edname, HtmlTextWriter writer)
    {
        Dictionary<string, string> tlist = getBaseList("tskbrset");
        StringBuilder sb = new StringBuilder();

        bool r;
        string si1, si2, si3;
        int i1, i2, i3, set;

		var res = new Bitrix.DataTypes.BXParamsBag<object>{};
 
        foreach (string key in tlist.Keys)
        {
			res[key] = new Bitrix.DataTypes.BXParamsBag<object>{
				{"show", (Request.Params["tskbrset[" + key + "][show]"] == "true")},
				{"set", ((Request.Params["tskbrset[" + key + "][set]"] == "2") ? "2" : "3")},
				{"active", Request.Params["tskbrset[" + key + "][active]"] == "true"}
			};
        }

		BXOptionManager.SetOptionString(edname, "taskbar_settings_" + edname, res.ToString());

		Dictionary<string, string> tbslist = getBaseList("tskbrsetset");		
		res = new Bitrix.DataTypes.BXParamsBag<object>{};
        foreach (string key in tbslist .Keys)
        {
			res[key] = new Bitrix.DataTypes.BXParamsBag<object>{
				{"show", Request.Params["tskbrsetset[" + key + "][show]"] == "true"},
				{"size", Request.Params["tskbrsetset[" + key + "][size]"]}
			};
        }

		BXOptionManager.SetOptionString(edname, "taskbarset_settings_" + edname, res.ToString());
    }
    
</script>
