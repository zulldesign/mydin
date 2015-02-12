using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Bitrix.UI;
using System.Xml;
using System.Collections.Generic;

public partial class bitrix_admin_UpdateSystemUpdateFramework : BXAdminPage
{
	const string AsmOld = "System.Web.Extensions, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
	const string AsmNew = "System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	protected void ContextMenu_CommandClick(object sender, CommandEventArgs e)
	{
		if (e.CommandName == "ViewUpdates")
			Response.Redirect("UpdateSystem.aspx");
	}

	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = GetMessageRaw("MasterTitle");
	}

	protected void Update35_Click(object sender, EventArgs e)
	{
		XmlDocument doc = new XmlDocument();
		doc.Load(MapPath("~/web.config"));

		XmlElement root = doc.DocumentElement;
		SetConfigSections(Node(root, "configSections"));

		XmlElement systemWeb = Node(root, "system.web");

		XmlElement pages = Node(systemWeb, "pages");
		SetPageControls(Node(pages, "controls"));
		SetPageNamespaces(Node(pages, "namespaces"));

		SetAssemblies(Node(Node(systemWeb, "compilation"), "assemblies"));

		XmlNode n;
		if ((n = systemWeb.SelectSingleNode("httpHandlers")) != null)
			PatchTypes(n);
		if ((n = systemWeb.SelectSingleNode("httpModules")) != null)
			PatchTypes(n);

		if ((n = root.SelectSingleNode("system.webServer/modules")) != null)
			PatchTypes(n);
		if ((n = root.SelectSingleNode("system.webServer/handlers")) != null)
			PatchTypes(n);

		SetCompilers(Node(Node(root, "system.codedom"), "compilers"));
		SetDependentAssemblies(NodeNamespaceLocal(Node(root, "runtime"), "urn:schemas-microsoft-com:asm.v1", "assemblyBinding", null));

		doc.Save(MapPath("~/web.config"));

		Response.Redirect("UpdateSystemUpdateFramework.aspx");
	}

	private void SetDependentAssemblies(XmlElement binding)
	{
		XmlElement t;
		XmlElement d1 = NodeNamespaceLocal(binding, "urn:schemas-microsoft-com:asm.v1", "dependentAssembly", "[*[local-name()='assemblyIdentity' and @name='System.Web.Extensions' and @publicKeyToken='31bf3856ad364e35']]");

		t = NodeNamespaceLocal(d1, "urn:schemas-microsoft-com:asm.v1", "assemblyIdentity", "[@name='System.Web.Extensions' and @publicKeyToken='31bf3856ad364e35']");
		Set(t, "name", "System.Web.Extensions");
		Set(t, "publicKeyToken", "31bf3856ad364e35");


		t = NodeNamespaceLocal(d1, "urn:schemas-microsoft-com:asm.v1", "bindingRedirect", "[@oldVersion='1.0.0.0-1.1.0.0']");
		Set(t, "oldVersion", "1.0.0.0-1.1.0.0");
		Set(t, "newVersion", "3.5.0.0");

		XmlElement d2 = NodeNamespaceLocal(binding, "urn:schemas-microsoft-com:asm.v1", "dependentAssembly", "[*[local-name()='assemblyIdentity' and @name='System.Web.Extensions.Design' and @publicKeyToken='31bf3856ad364e35']]");

		t = NodeNamespaceLocal(d2, "urn:schemas-microsoft-com:asm.v1", "assemblyIdentity", "[@name='System.Web.Extensions.Design' and @publicKeyToken='31bf3856ad364e35']");
		Set(t, "name", "System.Web.Extensions.Design");
		Set(t, "publicKeyToken", "31bf3856ad364e35");

		t = NodeNamespaceLocal(d2, "urn:schemas-microsoft-com:asm.v1", "bindingRedirect", "[@oldVersion='1.0.0.0-1.1.0.0']");
		Set(t, "oldVersion", "1.0.0.0-1.1.0.0");
		Set(t, "newVersion", "3.5.0.0");
	}

	private void SetCompilers(XmlElement compilers)
	{
		XmlElement cs = Node(compilers, "compiler", "extension", ".cs");
		Set(cs, "language", "c#;cs;csharp");
		Set(cs, "warningLevel", "4");
		Set(cs, "type", "Microsoft.CSharp.CSharpCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

		Set(Node(cs, "providerOption", "name", "CompilerVersion"), "value", "v3.5");
		Set(Node(cs, "providerOption", "name", "WarnAsError"), "value", "false");

		XmlElement vb = Node(compilers, "compiler", "extension", ".vb");
		Set(vb, "language", "vb;vbs;visualbasic;vbscript");
		Set(vb, "warningLevel", "4");
		Set(vb, "type", "Microsoft.VisualBasic.VBCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

		Set(Node(vb, "providerOption", "name", "CompilerVersion"), "value", "v3.5");
		Set(Node(vb, "providerOption", "name", "OptionInfer"), "value", "true");
		Set(Node(vb, "providerOption", "name", "WarnAsError"), "value", "false");
	}


	private void PatchTypes(XmlNode n)
	{
		foreach (XmlNode node in n.SelectNodes("*[ends-with(lower-case(string(@type)), '" + AsmOld.ToLower() + "')]", new XContext()))
		{
			string old = node.Attributes["type"].Value;
			node.Attributes["type"].Value = old.Remove(old.Length - AsmOld.Length) + AsmNew;
		}
	}

	private void SetAssemblies(XmlElement assemblies)
	{
		Set(
			Node(assemblies, "add", "[starts-with(lower-case(string(@assembly)), lower-case('System.Core,'))]"),
			"assembly",
			"System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089"
		);

		Set(
			Node(assemblies, "add", "[starts-with(lower-case(string(@assembly)), lower-case('System.Web.Extensions,'))]"),
			"assembly",
			"System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
		);

		Set(
			Node(assemblies, "add", "[starts-with(lower-case(string(@assembly)), lower-case('System.Xml.Linq,'))]"),
			"assembly",
			"System.Xml.Linq, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089"
		);

		Set(
			Node(assemblies, "add", "[starts-with(lower-case(string(@assembly)), lower-case('System.Data.DataSetExtensions,'))]"),
			"assembly",
			"System.Data.DataSetExtensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089"
		);
	}

	private void SetPageNamespaces(XmlElement namespaces)
	{
		XmlNode clear = namespaces.SelectSingleNode("clear");
		while (clear != null)
		{
			List<XmlNode> delete = new List<XmlNode>();
			foreach (XmlNode node in namespaces.ChildNodes)
			{
				delete.Add(node);
				if (node == clear)
					break;
			}
			foreach (XmlNode node in delete)
				namespaces.RemoveChild(node);

			clear = namespaces.SelectSingleNode("clear");
		}

		Node(namespaces, 0, "clear");
		Node(namespaces, "add", "namespace", "System");
		Node(namespaces, "add", "namespace", "System.Collections");
		Node(namespaces, "add", "namespace", "System.Collections.Generic");
		Node(namespaces, "add", "namespace", "System.Collections.Specialized");
		Node(namespaces, "add", "namespace", "System.Configuration");
		Node(namespaces, "add", "namespace", "System.Text");
		Node(namespaces, "add", "namespace", "System.Text.RegularExpressions");
		Node(namespaces, "add", "namespace", "System.Linq");
		Node(namespaces, "add", "namespace", "System.Xml.Linq");
		Node(namespaces, "add", "namespace", "System.Web");
		Node(namespaces, "add", "namespace", "System.Web.Caching");
		Node(namespaces, "add", "namespace", "System.Web.SessionState");
		Node(namespaces, "add", "namespace", "System.Web.Security");
		Node(namespaces, "add", "namespace", "System.Web.Profile");
		Node(namespaces, "add", "namespace", "System.Web.UI");
		Node(namespaces, "add", "namespace", "System.Web.UI.WebControls");
		Node(namespaces, "add", "namespace", "System.Web.UI.WebControls.WebParts");
		Node(namespaces, "add", "namespace", "System.Web.UI.HtmlControls");
	}

	private void SetPageControls(XmlElement controls)
	{
		foreach (XmlNode n in controls.SelectNodes("add[starts-with(@assembly, 'System.Web.Extensions,')]"))
			n.Attributes["assembly"].Value = AsmNew;

		XmlElement t = Node(controls, "add", "[@tagPrefix='asp' and @namespace='System.Web.UI' and starts-with(@assembly, 'System.Web.Extensions,')]");
		Set(t, "tagPrefix", "asp");
		Set(t, "namespace", "System.Web.UI");
		Set(t, "assembly", AsmNew);
	}

	private static void SetConfigSections(XmlElement configSections)
	{
		XmlElement t;

		XmlNode oldSections = configSections.SelectSingleNode("sectionGroup[@name='system.web.extensions' and contains(lower-case(string(@type)), 'version=1.0.61025.0')]",  new XContext());
		if (oldSections != null)
			configSections.RemoveChild(oldSections);


		t = Node(configSections, "sectionGroup", "name", "system.web.extensions");
		Set(t, "type", "System.Web.Configuration.SystemWebExtensionsSectionGroup, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

		XmlElement scripting = Node(t, "sectionGroup", "name", "scripting");
		Set(scripting, "type", "System.Web.Configuration.ScriptingSectionGroup, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

		t = Node(scripting, "section", "name", "scriptResourceHandler");
		Set(t, "type", "System.Web.Configuration.ScriptingScriptResourceHandlerSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
		Set(t, "requirePermission", "false");
		Set(t, "allowDefinition", "MachineToApplication");

		XmlElement webServices = Node(scripting, "sectionGroup", "name", "webServices");
		Set(webServices, "type", "System.Web.Configuration.ScriptingWebServicesSectionGroup, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

		t = Node(webServices, "section", "name", "jsonSerialization");
		Set(t, "type", "System.Web.Configuration.ScriptingJsonSerializationSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
		Set(t, "requirePermission", "false");
		Set(t, "allowDefinition", "Everywhere");

		t = Node(webServices, "section", "name", "profileService");
		Set(t, "type", "System.Web.Configuration.ScriptingProfileServiceSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
		Set(t, "requirePermission", "false");
		Set(t, "allowDefinition", "MachineToApplication");

		t = Node(webServices, "section", "name", "authenticationService");
		Set(t, "type", "System.Web.Configuration.ScriptingAuthenticationServiceSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
		Set(t, "requirePermission", "false");
		Set(t, "allowDefinition", "MachineToApplication");

		t = Node(webServices, "section", "name", "roleService");
		Set(t, "type", "System.Web.Configuration.ScriptingRoleServiceSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
		Set(t, "requirePermission", "false");
		Set(t, "allowDefinition", "MachineToApplication");
	}



	public static XmlElement Node(XmlElement node, string name)
	{
		return Node(node, name, null);
	}

	public static XmlElement Add(XmlElement node, string name)
	{
		return (XmlElement)node.AppendChild(node.OwnerDocument.CreateElement(name));
	}

	public static XmlElement Node(XmlElement node, string name, string suffix)
	{
		return (XmlElement)(node.SelectSingleNode(name + suffix, new XContext()) ?? node.AppendChild(node.OwnerDocument.CreateElement(name)));
	}

	public static XmlElement NodeNamespaceLocal(XmlElement node, string uri, string name, string suffix)
	{
		return (XmlElement)(node.SelectSingleNode("*[local-name()='" + name + "']" + suffix, new XContext()) ?? node.AppendChild(node.OwnerDocument.CreateElement("", name, uri)));
	}

	public static XmlElement Node(XmlElement node, string name, string attr, string value)
	{
		XmlElement n = (XmlElement)node.SelectSingleNode(name + "[@" + attr + "='" + value + "']");
		if (n == null)
		{
			n = (XmlElement)node.AppendChild(node.OwnerDocument.CreateElement(name));
			n.Attributes.Append(node.OwnerDocument.CreateAttribute(attr)).Value = value;
		}
		return n;
	}

	public static XmlElement Node(XmlElement node, int index, string name)
	{
		return Node(node, index, name, null);
	}

	public static XmlElement Node(XmlElement node, int index, string name, string suffix)
	{
		XmlElement n = (XmlElement)node.SelectSingleNode(name + suffix,  new XContext());
		if (n != null)
			node.RemoveChild(n);
		else
			n = (XmlElement)node.AppendChild(node.OwnerDocument.CreateElement(name));

		if (index >= node.ChildNodes.Count || node.ChildNodes.Count == 0)
			node.AppendChild(n);
		else
			node.InsertBefore(n, node.ChildNodes[index]);

		return n;
	}

	public static XmlElement Node(XmlElement node, int index, string name, string attr, string value)
	{
		XmlElement n = (XmlElement)node.SelectSingleNode(name + "[@" + attr + "='" + value + "']");
		if (n != null)
			node.RemoveChild(n);
		else
		{
			n = (XmlElement)node.AppendChild(node.OwnerDocument.CreateElement(name));
			n.Attributes.Append(node.OwnerDocument.CreateAttribute(attr)).Value = value;
		}

		if (index >= node.ChildNodes.Count || node.ChildNodes.Count == 0)
			node.AppendChild(n);
		else
			node.InsertBefore(n, node.ChildNodes[index]);

		return n;
	}

	public static XmlElement Set(XmlElement node, string name, string value)
	{
		(node.Attributes[name] ?? node.Attributes.Append(node.OwnerDocument.CreateAttribute(name))).Value = value;
		return node;
	}

	class XContext : System.Xml.Xsl.XsltContext
	{
		public override System.Xml.Xsl.IXsltContextFunction ResolveFunction(string prefix, string name, System.Xml.XPath.XPathResultType[] ArgTypes)
		{
			if (name == "lower-case")
				return new LowerCaseFunction();
			else if (name == "ends-with")
				return new EndsWithFunction();
			return null;
		}

		public override int CompareDocument(string baseUri, string nextbaseUri)
		{
			return 0;
		}

		public override bool PreserveWhitespace(System.Xml.XPath.XPathNavigator node)
		{
			return true;
		}

		public override System.Xml.Xsl.IXsltContextVariable ResolveVariable(string prefix, string name)
		{
			return null;
		}

		public override bool Whitespace
		{
			get { return true; }
		}
	}
	class LowerCaseFunction : System.Xml.Xsl.IXsltContextFunction
	{
		#region IXsltContextFunction Members

		public System.Xml.XPath.XPathResultType[] ArgTypes
		{
			get { return new System.Xml.XPath.XPathResultType[] { System.Xml.XPath.XPathResultType.String };  }
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
			get { return new System.Xml.XPath.XPathResultType[] { System.Xml.XPath.XPathResultType.String, System.Xml.XPath.XPathResultType.String };  }
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

