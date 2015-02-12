using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using System.Xml;
using System.Web.Hosting;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Text;
using Bitrix;
using Bitrix.Configuration;
using Bitrix.Services;
using System.Collections.Specialized;

public partial class bitrix_admin_AppOptimization : BXAdminPage
{
    protected override string BackUrl
    {
        get { return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "AppOptimization.aspx"; }
    }
    private string webConfigPath = null;
    private string WebConfigPath
    {
        get { return this.webConfigPath ?? (this.webConfigPath = HostingEnvironment.MapPath(this.webConfigPath)); }
    }
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		btnSwitchComponentAutoCaching.Text = GetMessageRaw(IsComponentAutoCachingEnabled ? "ButtomText.DisableComponentAutoCaching" : "ButtomText.EnableComponentAutoCaching");
		btnSwitchManagedCaching.Text = GetMessageRaw(IsTagCachingEnabled ? "ButtomText.DisableManagedCaching" : "ButtomText.EnableManagedCaching");
	}
    protected override void OnLoad(EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                EnableMemoryCollection.Checked = !Settings.DisableMemoryCollection;
                PrivateBytesLimit.Text = Settings.PrivateBytesLimit.ToString();
                PrivateBytesPollTime.Text = Settings.PrivateBytesPollTime.ToString();
                CompilationEnableDebug.Checked = Settings.CompilationDebug;
                CompilationEnableBatch.Checked = Settings.CompilationBatch;
            }
            else
            {
                Validate();

                if (IsValid)
                {
                    Settings.DisableMemoryCollection = !EnableMemoryCollection.Checked;

                    if (!string.IsNullOrEmpty(PrivateBytesLimit.Text))
                    {
                        long privateBytesLimit;
                        if (long.TryParse(PrivateBytesLimit.Text, out privateBytesLimit))
                            Settings.PrivateBytesLimit = privateBytesLimit;
                    }
                    else
                        PageError |= AppOptimizationError.PrivateBytesLimitParsing;

                    if (!string.IsNullOrEmpty(PrivateBytesPollTime.Text))
                    {
                        int privateBytesPollTime;
                        if (int.TryParse(PrivateBytesPollTime.Text, out privateBytesPollTime))
                            Settings.PrivateBytesPollTime = privateBytesPollTime;
                    }
                    else
                        PageError |= AppOptimizationError.PrivateBytesPollTimeParsing;

                    Settings.CompilationDebug = CompilationEnableDebug.Checked;
                    Settings.CompilationBatch = CompilationEnableBatch.Checked;

                    CompilationAssembly[] supAssemblies = Settings.CompilationAssemblies;
                    if (supAssemblies.Length > 0)
                        for (int i = 0; i < supAssemblies.Length; i++)
                        { 
                            CompilationAssembly asm = supAssemblies[i];
                            CheckBox cb = null;
                            switch (asm.Name)
                            {
                                case "System.EnterpriseServices":
                                    cb = EnableEnterpriseServices;
                                    break;
                                case "System.ServiceModel":
                                    cb = EnableServiceModel;
                                    break;
                                case "System.ServiceModel.Web":
                                    cb = EnableServiceModelWeb;
                                    break;
                                case "System.IdentityModel":
                                    cb = EnableIdentityModel;
                                    break;
                                case "System.Web.Mobile":
                                    cb = EnableWebMobile;
                                    break;
                                case "System.WorkflowServices":
                                    cb = EnableWorkflowServices;
                                    break;
                            }
                            asm.Enabled = cb != null && cb.Checked;  
                        }
                }
            }
            MasterTitle = Page.Title = GetMessage("PageTitle");
        }
        catch (Exception exc)
        {
            PageError |= AppOptimizationError.Custom;
            CustomPageErrorMessage = exc.Message;
        }
        base.OnLoad(e);
    }
    protected override void OnPreRender(EventArgs e)
    {
        Scripts.RequireUtils();
        if (PageError != AppOptimizationError.None)
            errorMessage.AddErrorMessage(HttpUtility.HtmlEncode(GetPageErrorMessage()));
        else if(!IsPostBack)
        {
            EnableEnterpriseServices.Checked = Settings.IsCompilationAssemblyEnabled("System.EnterpriseServices");
            EnableServiceModel.Checked = Settings.IsCompilationAssemblyEnabled("System.ServiceModel");
            EnableServiceModelWeb.Checked = Settings.IsCompilationAssemblyEnabled("System.ServiceModel.Web");
            EnableIdentityModel.Checked = Settings.IsCompilationAssemblyEnabled("System.IdentityModel");
            EnableWebMobile.Checked = Settings.IsCompilationAssemblyEnabled("System.Web.Mobile");
            EnableWorkflowServices.Checked = Settings.IsCompilationAssemblyEnabled("System.WorkflowServices");
        }
        base.OnPreRender(e);
    }
    protected void OnTabControlCommand(object sender, BXTabControlCommandEventArgs e)
    {
        if (PageError != AppOptimizationError.None)
            return;

        try
        {
            switch (e.CommandName)
            {
                case "save":
                    {
                        if (IsValid)
                        {
                            TrySave();
                            GoBack();
                        }
                    }
                    break;
                case "apply":
                    {
                        if (IsValid)
                        {
                            TrySave();
                            Response.Redirect(Request.RawUrl);
                        }
                    }
                    break;
                case "cancel":
                    GoBack();
                    break;
            }
        }
        catch (Exception exc)
        {
            PageError |= AppOptimizationError.Custom;
            CustomPageErrorMessage = exc.Message;
        }
    }
    private OptimizationSettings settings = null;
    private OptimizationSettings Settings
    {
        get 
        {
            if (this.settings != null)
                return this.settings;

            this.settings = new OptimizationSettings();
            this.settings.Load();

            return this.settings;
        }
    }
    private void TrySave()
    {
        Settings.Save();
    }
    private AppOptimizationError pageError = AppOptimizationError.None;
    public AppOptimizationError PageError
    {
        get { return this.pageError; }
        private set { this.pageError = value; }
    }
    private string customPageErrorMsg = string.Empty;
    public string CustomPageErrorMessage
    {
        get { return this.customPageErrorMsg; }
        private set { this.customPageErrorMsg = value ?? string.Empty; }
    }
    public string GetPageErrorMessage()
    {
        if (this.pageError == AppOptimizationError.None)
            return string.Empty;

        StringBuilder sb = new StringBuilder();
        if ((this.pageError & AppOptimizationError.PrivateBytesLimitParsing) != 0)
            sb.AppendLine(GetMessageRaw("ErrorMessage.PrivateBytesLimitParsing"));
        if ((this.pageError & AppOptimizationError.PrivateBytesPollTimeParsing) != 0)
            sb.AppendLine(GetMessageRaw("ErrorMessage.PrivateBytesPollTimeParsing"));

        if (!string.IsNullOrEmpty(this.customPageErrorMsg))
            sb.AppendLine(this.customPageErrorMsg);

        return sb.ToString();
    }
    public bool HasCompilationAssemblyReferencences
    {
        get { return Settings.CompilationAssemblies.Length > 0; }
    }

	private bool? isComponentAutoCachingEnabled = null;
	public bool IsComponentAutoCachingEnabled
	{
		get { return (this.isComponentAutoCachingEnabled ?? (this.isComponentAutoCachingEnabled = BXOptionManager.GetOption("main", "ComponentsAutoCache", true))).Value; }
	}

	protected void SwitchComponentAutoCaching(object sender, CommandEventArgs e)
	{
		BXOptionManager.SetOption("main", "ComponentsAutoCache", !IsComponentAutoCachingEnabled);
		NameValueCollection qs = HttpUtility.ParseQueryString(Request.Url.Query);
		qs["tabindex"] = "1";
		Response.Redirect(string.Concat(Request.Url.AbsolutePath, "?", qs.ToString()), true);
	}

	private bool? isTagCachingEnabled = null;
	public bool IsTagCachingEnabled
	{
		get { return (this.isTagCachingEnabled ?? (this.isTagCachingEnabled = BXTagCachingManager.IsTagCachingEnabled())).Value; }
	}
	
	protected void SwitchManagedCaching(object sender, CommandEventArgs e)
	{
		BXTagCachingManager.EnableTagCaching(!BXTagCachingManager.IsTagCachingEnabled());
		NameValueCollection qs = HttpUtility.ParseQueryString(Request.Url.Query);
		qs["tabindex"] = "2";
		Response.Redirect(string.Concat(Request.Url.AbsolutePath, "?", qs.ToString()), true);
	}

    private class CompilationAssembly
    {
        public CompilationAssembly(string name)
        {
            Name = name;
        }

        private string name = string.Empty;
        public string Name
        {
            get { return this.name; }
            set { this.name = value ?? string.Empty; }
        }

        private string fullName = string.Empty;
        public string FullName
        {
            get { return this.fullName; }
            set { this.fullName = value ?? string.Empty; }
        }

        private bool isReferencedLocaly = false;
        public bool IsReferencedLocaly
        {
            get { return this.isReferencedLocaly; }
            set { this.isReferencedLocaly = value; }
        }

        private bool enabled = true;
        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }
    }
    private class OptimizationSettings
    {

        private bool compilationDebug = false;
        public bool CompilationDebug
        {
            get { return this.compilationDebug; }
            set { this.compilationDebug = value; }
        }

        private bool compilationBatch = false;
        public bool CompilationBatch
        {
            get { return this.compilationBatch; }
            set { this.compilationBatch = value; }
        }

        private bool disableMemoryCollection = true;
        public bool DisableMemoryCollection
        {
            get { return this.disableMemoryCollection; }
            set { this.disableMemoryCollection = value; }
        }

        private long privateBytesLimit = 0L;
        public long PrivateBytesLimit
        {
            get { return this.privateBytesLimit; }
            set { this.privateBytesLimit = value >= 0 ? value : 0; }
        }

        private int privateBytesPollTime = 1;
        public int PrivateBytesPollTime
        {
            get { return this.privateBytesPollTime; }
            set { this.privateBytesPollTime = value > 0 ? value : 1; }
        }

        private CompilationAssembly[] compilationAssemblies = null;
        public CompilationAssembly[] CompilationAssemblies
        {
            get { return this.compilationAssemblies ?? (this.compilationAssemblies = new CompilationAssembly[0]); }
        }

        public bool IsCompilationAssemblyEnabled(string name)
        {
            if (this.compilationAssemblies == null)
                return false;

            for (int i = 0; i < this.compilationAssemblies.Length; i++)
                if (string.Equals(name, this.compilationAssemblies[i].Name, StringComparison.Ordinal))
                    return this.compilationAssemblies[i].Enabled;

            return false;
        }

        private string webConfigPath = null;
        public string WebConfigPath
        {
            get { return this.webConfigPath ?? (this.webConfigPath = HostingEnvironment.MapPath("~/web.config")); }
        }

        private XmlDocument doc = null;
        private XmlDocument InnerDoc
        {
            get 
            {
                if (this.doc == null)
                {
                    this.doc = new XmlDocument();
                    doc.Load(WebConfigPath);
                }
                return this.doc;
            }
        }

        public void Reset()
        {
            this.compilationDebug = false;
            this.compilationBatch = true;
            this.disableMemoryCollection = false;
            this.privateBytesLimit = 0L;
            this.privateBytesPollTime = 1;
            this.compilationAssemblies = null;
        }

        public bool TryGetAttributeBoolean(XmlElement node, string name, out bool val)
        {
            val = false;
            if(!node.HasAttribute(name))
                return false;

            string s = node.GetAttribute(name);
            return bool.TryParse(s, out val);
        }

        public bool TryGetAttributeInt64(XmlElement node, string name, out long val)
        {
            val = 0L;
            if (!node.HasAttribute(name))
                return false;

            string s = node.GetAttribute(name);
            return long.TryParse(s, out val);
        }

        public bool TryGetAttributeInt32(XmlElement node, string name, out int val)
        {
            val = 0;
            if (!node.HasAttribute(name))
                return false;

            string s = node.GetAttribute(name);
            return int.TryParse(s, out val);
        }

        public bool TryGetAttributeString(XmlElement node, string name, out string val)
        {
            val = string.Empty;
            if (!node.HasAttribute(name))
                return false;

            val = node.GetAttribute(name);
            return true;
        }

        public void Load()
        {
            Reset();

            XmlElement cache = XContext.Instance.GetChildNode(InnerDoc.DocumentElement, "system.web/caching/cache", false);
            if (cache != null)
            {
                bool disableMemoryCollection;
                this.disableMemoryCollection = TryGetAttributeBoolean(cache, "disableMemoryCollection", out disableMemoryCollection) ? disableMemoryCollection : false;

                long privateBytesLimit;
                if (TryGetAttributeInt64(cache, "privateBytesLimit", out privateBytesLimit))
                    this.privateBytesLimit = privateBytesLimit;

                string privateBytesPollTime;
                if (TryGetAttributeString(cache, "privateBytesPollTime", out privateBytesPollTime))
                {
                    TimeSpan span = TimeSpan.Parse(privateBytesPollTime);
                    this.privateBytesPollTime = Convert.ToInt32(span.TotalSeconds);
                }
            }

            XmlElement compilation = XContext.Instance.GetChildNode(InnerDoc.DocumentElement, "system.web/compilation", false);
            this.compilationDebug = BXCompilationHelper.IsDebuggingEnabled;
            bool compilationBatch;
            if (TryGetAttributeBoolean(compilation, "batch", out compilationBatch))
                this.compilationBatch = compilationBatch;

            XmlElement compilationAssemblies = XContext.Instance.GetChildNode(InnerDoc.DocumentElement, "system.web/compilation/assemblies", false);

            List<CompilationAssembly> supList = new List<CompilationAssembly>();
            CompilationAssembly sup = null;
            if ((sup = CreateCompilationAssembly("System.EnterpriseServices", "System.EnterpriseServices, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", compilationAssemblies)) != null)
                supList.Add(sup);

            if ((sup = CreateCompilationAssembly("System.ServiceModel", "System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", compilationAssemblies)) != null)
                supList.Add(sup);

            if ((sup = CreateCompilationAssembly("System.ServiceModel.Web", "System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", compilationAssemblies)) != null)
                supList.Add(sup);

            if ((sup = CreateCompilationAssembly("System.Web.Mobile", "System.Web.Mobile, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", compilationAssemblies)) != null)
                supList.Add(sup);

            if ((sup = CreateCompilationAssembly("System.IdentityModel", "System.IdentityModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, processorArchitecture=MSIL", compilationAssemblies)) != null)
                supList.Add(sup);

            if ((sup = CreateCompilationAssembly("System.WorkflowServices", "System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", compilationAssemblies)) != null)
                supList.Add(sup);

            this.compilationAssemblies = supList.ToArray();
        }

        private CompilationAssembly CreateCompilationAssembly(string name, string fullName, XmlElement compilationAssemblies)
        {
            bool enabled = false;
            try
            {
                enabled = BXCompilationHelper.CheckAssemblyReference(name);
            }
            catch
            {
                return null;
            }
                
            CompilationAssembly r = new CompilationAssembly(name);
            r.FullName = fullName;
            r.Enabled = enabled;
            r.IsReferencedLocaly = compilationAssemblies != null ? XContext.Instance.GetChildNode(compilationAssemblies, string.Concat(enabled ? "add" : "remove", "[starts-with(lower-case(string(@assembly)), lower-case('", name, "'))]"), false) != null : false;
            return r; 
        }

        public void Save()
        {
            XmlElement cache = XContext.Instance.GetChildNode(InnerDoc.DocumentElement, "system.web/caching/cache", true);
            cache.SetAttribute("disableMemoryCollection", DisableMemoryCollection.ToString().ToLowerInvariant());

            if (!DisableMemoryCollection && PrivateBytesLimit > 0)
                cache.SetAttribute("privateBytesLimit", PrivateBytesLimit.ToString());
            else if (cache.HasAttribute("privateBytesLimit"))
                cache.RemoveAttribute("privateBytesLimit");

            if (!DisableMemoryCollection && PrivateBytesPollTime > 1)
                cache.SetAttribute("privateBytesPollTime", TimeSpan.FromSeconds(PrivateBytesPollTime).ToString());
            else if (cache.HasAttribute("privateBytesPollTime"))
                cache.RemoveAttribute("privateBytesPollTime");

            XmlElement compilation = XContext.Instance.GetChildNode(InnerDoc.DocumentElement, "system.web/compilation", true);
            compilation.SetAttribute("debug", this.compilationDebug.ToString().ToLowerInvariant());
            compilation.SetAttribute("batch", this.compilationBatch.ToString().ToLowerInvariant());

            if (CompilationAssemblies.Length > 0)
            {
                XmlElement compilationAssemblies = XContext.Instance.GetChildNode(InnerDoc.DocumentElement, "system.web/compilation/assemblies", true);
                for (int i = 0; i < CompilationAssemblies.Length; i++)
                {
                    CompilationAssembly asm = CompilationAssemblies[i];
                    if (!asm.Enabled)
                    {
                        if (asm.IsReferencedLocaly)
                        {
                            XmlElement addAsm = XContext.Instance.GetChildNode(compilationAssemblies, string.Concat("add[starts-with(lower-case(string(@assembly)), lower-case('", asm.Name, "'))]"), false);
                            if (addAsm != null)
                                compilationAssemblies.RemoveChild(addAsm);
                        }
                        XmlElement remAsm = XContext.Instance.GetChildNode(compilationAssemblies, string.Concat("remove[starts-with(lower-case(string(@assembly)), lower-case('", asm.Name, "'))]"), false);
                        if(remAsm == null)
                            remAsm = compilationAssemblies.OwnerDocument.CreateElement("remove");
                        remAsm.SetAttribute("assembly", asm.FullName);
                        compilationAssemblies.AppendChild(remAsm);
                    }
                    else
                    {
                        if (asm.IsReferencedLocaly)
                        {
                            XmlElement remAsm = XContext.Instance.GetChildNode(compilationAssemblies, string.Concat("remove[starts-with(lower-case(string(@assembly)), lower-case('", asm.Name, "'))]"), false);
                            if (remAsm != null)
                                compilationAssemblies.RemoveChild(remAsm);
                        }
                        XmlElement addAsm = XContext.Instance.GetChildNode(compilationAssemblies, string.Concat("add[starts-with(lower-case(string(@assembly)), lower-case('", asm.Name, "'))]"), false);
                        if(addAsm == null)
                            addAsm = compilationAssemblies.OwnerDocument.CreateElement("add");
                        addAsm.SetAttribute("assembly", asm.FullName);
                        compilationAssemblies.AppendChild(addAsm); 
                    }
                }
            }

            InnerDoc.Save(WebConfigPath);
        }
    }
    private class XContext : XsltContext
    {
        private static object sync = new object();
        private static volatile XContext instance = null;
        public static XContext Instance
        {
            get 
            {
                if (instance != null)
                    return instance;

                lock (sync)
                {
                    return instance ?? (instance = new XContext());
                }
            }
        }

        #region
        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
        {
            if (string.Equals(name, "lower-case", StringComparison.Ordinal))
                return new LowerCaseFunction();
            else if (string.Equals(name, "ends-with", StringComparison.Ordinal))
                return new EndsWithFunction();

            return null;
        }

        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            return 0;
        }

        public override bool PreserveWhitespace(XPathNavigator node)
        {
            return true;
        }

        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            return null;
        }

        public override bool Whitespace
        {
            get { return true; }
        }
        #endregion

        public XmlElement GetChildNode(XmlElement parentNode, string path, bool create)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Is not defined", "path");

            string[] nameAry = path.Split(new char[] { '/' });
            XmlElement curnNode = null, curParNode = parentNode;
            for (int i = 0; i < nameAry.Length; i++)
            {
                string name = nameAry[i];
                curnNode = (XmlElement)(curParNode.SelectSingleNode(name, this));
                if (curnNode == null && create)
                    curnNode = (XmlElement)(curParNode.AppendChild(curParNode.OwnerDocument.CreateElement(name)));

                if (curnNode == null)
                    break;

                curParNode = curnNode;
            }
            return curnNode;
        }

        public XmlElement SetNodeAttribute(XmlElement node, string attrName, string attrValue)
        {
            (node.Attributes[attrName] ?? node.Attributes.Append(node.OwnerDocument.CreateAttribute(attrName))).Value = attrValue;
            return node;
        }
    }
    class LowerCaseFunction : System.Xml.Xsl.IXsltContextFunction
    {
        #region IXsltContextFunction Members

        public System.Xml.XPath.XPathResultType[] ArgTypes
        {
            get { return new System.Xml.XPath.XPathResultType[] { System.Xml.XPath.XPathResultType.String }; }
        }

        public object Invoke(System.Xml.Xsl.XsltContext xsltContext, object[] args, System.Xml.XPath.XPathNavigator docContext)
        {
            return (args[0].ToString()).ToLower();
        }

        public int Maxargs
        {
            get { return 1; }
        }

        public int Minargs
        {
            get { return 1; }
        }

        public System.Xml.XPath.XPathResultType ReturnType
        {
            get { return System.Xml.XPath.XPathResultType.Boolean; }
        }

        #endregion
    }
    class EndsWithFunction : System.Xml.Xsl.IXsltContextFunction
    {
        #region IXsltContextFunction Members

        public System.Xml.XPath.XPathResultType[] ArgTypes
        {
            get { return new System.Xml.XPath.XPathResultType[] { System.Xml.XPath.XPathResultType.String, System.Xml.XPath.XPathResultType.String }; }
        }

        public object Invoke(System.Xml.Xsl.XsltContext xsltContext, object[] args, System.Xml.XPath.XPathNavigator docContext)
        {
            return (args[0].ToString()).EndsWith(args[1].ToString());
        }

        public int Maxargs
        {
            get { return 2; }
        }

        public int Minargs
        {
            get { return 2; }
        }

        public System.Xml.XPath.XPathResultType ReturnType
        {
            get { return System.Xml.XPath.XPathResultType.Boolean; }
        }

        #endregion
    }
}

public enum AppOptimizationError
{
    None = 0x0,
    Custom = 0x1,
    PrivateBytesLimitParsing = 0x2,
    PrivateBytesPollTimeParsing = 0x4
}
