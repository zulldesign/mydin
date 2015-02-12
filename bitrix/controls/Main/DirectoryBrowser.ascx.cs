using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Bitrix.Services;
using Bitrix.UI;
using Bitrix.IO;
using Bitrix.Services.Js;
using Bitrix.Configuration;

public partial class DirectoryBrowser : BXControl, IBXDirectoryBrowser
{
	string _JavaScriptObject;
	string _ExtensionsList = string.Empty;
	bool _ShowFiles = true;
	BXDirectoryBrowserSelectionType _ItemsToSelect = BXDirectoryBrowserSelectionType.FilesAndFolders;
	Unit _Width = Unit.Percentage(100);
	Unit _Height = Unit.Percentage(100);
	bool showExtras = true;
	bool enableExtras = true;
	string defaultUploadDirectory = string.Empty;
    string defaultPath = string.Empty;
	bool appendSlashToFolders;

	[Category("Appearance")]
	[DefaultValue(false)]
	public bool ShowDescription
	{
		get { return browser.ShowDescription; }
		set { browser.ShowDescription = value; }
	}

	[Category("Appearance")]
	[DefaultValue("")]
	public string DescriptionText
	{
		get { return browser.DescriptionText; }
		set { browser.DescriptionText = value; }
	}

	[Category("Appearance")]
	[DefaultValue("")]
	public string DescriptionTitle
	{
		get { return browser.DescriptionTitle; }
		set { browser.DescriptionTitle = value; }
	}

	[Category("Appearance")]
	[DefaultValue("")]
	public string WindowTitle
	{
		get { return browser.WindowTitle; }
		set { browser.WindowTitle = value; }
	}

	[Category("Behavior")]
	[DefaultValue("")]
	public string OKClientScript
	{
		get { return browser.OnSaveClientClick; }
		set { browser.OnSaveClientClick = value; }
	}

	[Category("Behavior")]
	[DefaultValue("")]
	public string CancelClientScript
	{
		get { return browser.OnCancelClientClick; }
		set { browser.OnCancelClientClick = value; }
	}

	[Category("Behavior")]
	[DefaultValue(true)]
	public bool ShowFiles
	{
		get { return _ShowFiles; }
		set { _ShowFiles = value; }
	}

	[Category("Behavior")]
	[DefaultValue(true)]
	public bool ShowExtras
	{
		get { return showExtras; }
		set { showExtras = value; }
	}

	[Category("Behavior")]
	[DefaultValue(true)]
	public bool EnableExtras
	{
		get { return enableExtras; }
		set { enableExtras = value; }
	}

	[Category("Behavior")]
	[DefaultValue(BXDirectoryBrowserSelectionType.FilesAndFolders)]
	public BXDirectoryBrowserSelectionType ItemsToSelect
	{
		get { return _ItemsToSelect; }
		set { _ItemsToSelect = value; }
	}

	[Category("Behavior")]
	[DefaultValue("")]
	public string DefaultUploadDirectory
	{
		get { return defaultUploadDirectory; }
		set { defaultUploadDirectory = value; }
	}


    [Category("Behavior")]
    [DefaultValue("")]
    public string DefaultPath
    {
        get { return defaultPath; }
        set { defaultPath = value; }
    }

	[Category("Behavior")]
	[DefaultValue("")]
	public string ExtensionsList
	{
		get { return _ExtensionsList; }
		set { _ExtensionsList = value; }
	}

	[Category("Behavior")]
	[DefaultValue(false)]
	public bool AppendSlashToFolders
	{
		get { return appendSlashToFolders; }
		set { appendSlashToFolders = value; }
	}

	[Browsable(false)]
	public string JavaScriptObject
	{
		get
		{
			/*if (_JavaScriptObject == null)*/
			_JavaScriptObject = this.ClientID + "_db";
			return _JavaScriptObject;
		}
	}

	[Browsable(false)]
	public BXPopupDialog ContainerDialog
	{
		get { return browser; }
	}

	protected void Page_PreRender(object sender, EventArgs e)
	{
		Dictionary<string, object> images = new Dictionary<string, object>();
        images.Add("empty", VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/img/DirectoryBrowser/empty.gif"));
        images.Add("dot", VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/img/DirectoryBrowser/dot.gif"));
        images.Add("plus", VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/img/DirectoryBrowser/plus.gif"));
        images.Add("minus", VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/img/DirectoryBrowser/minus.gif"));
        images.Add("opened", VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/img/DirectoryBrowser/opened.gif"));
        images.Add("closed", VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/img/DirectoryBrowser/closed.gif"));
        images.Add("file", VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/img/DirectoryBrowser/file.gif"));
        images.Add("root", VirtualPathUtility.ToAbsolute("~/bitrix/controls/Main/img/DirectoryBrowser/root.gif"));
        //images.Add("wait", BXConfigurationUtility.Constants.AdminThemeRoot + "images/wait.gif");
        //zg, 25.04.2008
        images.Add("wait", VirtualPathUtility.ToAbsolute(Bitrix.UI.BXThemeHelper.SimpleCombineWithCurrentThemePath("images/wait.gif")));

		Dictionary<string, object> strings = new Dictionary<string, object>();
		strings.Add("root", GetMessageRaw("RootTitle"));
		strings.Add("loading", GetMessageRaw("Loading"));
		strings.Add("wait", GetMessageRaw("Wait"));

		Dictionary<string, object> properties = new Dictionary<string, object>();
		properties.Add("ShowFiles", this.ShowFiles);
		properties.Add("ItemsToSelect", this.ItemsToSelect);
		properties.Add("ExtensionsList", this.ExtensionsList ?? string.Empty);
		properties.Add("EnableExtras", this.EnableExtras);
		properties.Add("ShowExtras", this.ShowExtras);
		properties.Add("DefaultUploadDirectory", this.DefaultUploadDirectory);
		properties.Add("AppendSlashToFolders", this.AppendSlashToFolders);

        string path = BXPath.ToRelativePath(this.DefaultPath);
        //if (!string.IsNullOrEmpty(path))
        //{
        //path = BXPath.NormalizePath(path);
        //if (path.Length > 1)
        //    path = path.Substring(2);
        //else
        //    path = "";
        //    if (!path.StartsWith("\\"))
        //        path = "\\" + path;
        //}
        properties.Add("DefaultPath", path);

		Dictionary<string, object> controls = new Dictionary<string, object>();
		controls.Add("selector", this.selector);
		controls.Add("target", this.selectedName);
		controls.Add("preview", this.preview);
		controls.Add("extras", this.extras);
		controls.Add("extrasImage", this.switcher);
		controls.Add("container", this.browser);
		controls.Add("uploadFrame", this.up);

		Dictionary<string, object> parameters = new Dictionary<string, object>();
		parameters.Add("self", this.JavaScriptObject);

        parameters.Add("handler", VirtualPathUtility.ToAbsolute(Bitrix.Configuration.BXConfigurationUtility.Constants.DirectoryBrowserHandler));
        parameters.Add("resizer", VirtualPathUtility.ToAbsolute(Bitrix.Configuration.BXConfigurationUtility.Constants.ImageResizerHandler));
        parameters.Add("uploader",
            VirtualPathUtility.ToAbsolute(AppRelativeTemplateSourceDirectory + "DirectoryUploader.aspx")
            );

		parameters.Add("appPath", VirtualPathUtility.ToAbsolute("~"));
		parameters.Add("containerWidth", (int) browser.Width.Value);
		parameters.Add("extrasWidth", 276);
		parameters.Add("dialogObj", browser.GetJSObjectName());
		parameters.Add("images", images);
		parameters.Add("strings", strings);
		parameters.Add("controls", controls);

        BXPage.RegisterScriptInclude("~/bitrix/js/Main/DirectoryBrowser.js");

        ScriptManager.RegisterStartupScript(
            Page,
            GetType(),
            ClientID,
            String.Format("var {0} = new DirectoryBrowser({1}, {2});", this.JavaScriptObject, BXJSUtility.BuildJSON(parameters),
                          BXJSUtility.BuildJSON(properties)),
            true
            );

		BXPage.RegisterThemeStyle("DirectoryBrowser.css");
    }

	protected override void Render(HtmlTextWriter writer)
	{
		PrepareControlsForRender();
		base.Render(writer);
	}

	void PrepareControlsForRender()
	{
		preview.Style[HtmlTextWriterStyle.BackgroundImage] = "url(" +
		                                                     VirtualPathUtility.ToAbsolute(
		                                                     	AppRelativeTemplateSourceDirectory +
		                                                     	"/img/DirectoryBrowser/imagebg.png") + ")";
		//up.Attributes["src"] = VirtualPathUtility.ToAbsolute(AppRelativeTemplateSourceDirectory + "/DirectoryUploader.aspx");
	}

	//NESTED CLASSES
	[Flags]
	public enum SelectionType
	{
		None = 0,
		Folders = 1,
		Files = 2,
		FilesAndFolders = Files | Folders
	}
}
