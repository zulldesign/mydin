using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using Bitrix.UI;
using Bitrix.Configuration;
using Bitrix;
using Bitrix.IO;
using System.Web.Compilation;
using Bitrix.Components;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using Bitrix.Security;
using Bitrix.DataTypes;
using Bitrix.Services.Undo;
using System.Text;

public partial class bitrix_dialogs_CopyComponentTemplate : BXDialogPage
{
    /// <summary>
    /// Типы шаблонов
    /// </summary>
    protected enum ComponentTemplateType
    {
        System = 1,
        Site
    }

    private string _targetFileAppRelPath = null;
    /// <summary>
    /// Путь к целевому файлу (содержащему компонент)
    /// </summary>
    protected string TargetFileAppRelPath
    {
        get 
        {
            if (_targetFileAppRelPath == null)
            {
                _targetFileAppRelPath = Request.QueryString["path"] ?? string.Empty;

                if (string.IsNullOrEmpty(_targetFileAppRelPath))
                    throw new InvalidOperationException(GetMessageRaw("ERROR_COMPONENT_ELEMENT_FILE_PATH_IS_NOT_DEFINED"));
            }
            return _targetFileAppRelPath; 
        }
    }

    /// <summary>
    /// ИД целевого эл-та (содержащего параметры компонента)
    /// </summary>
    private string _targetElementID = null;
    protected string TargetElementID
    {
        get 
        {
            if (_targetElementID == null)
            {
                _targetElementID = Request.QueryString["id"] ?? string.Empty;

                if (string.IsNullOrEmpty(_targetElementID))
                    throw new InvalidOperationException(GetMessageRaw("ERROR_COMPONENT_ELEMENT_ID_IS_NOT_DEFINED"));
            }
            return _targetElementID;  
        }
    }

    /// <summary>
    /// URL возврата. Передаётся при вызове страниц административной части.
    /// </summary>
    protected string BackUrl 
    {
        get { return Request.QueryString[BXConfigurationUtility.Constants.BackUrl]; }
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        if (!CheckUserAuthorization())
            return;
		if (Request["siteTemplate"] != null)
			BXSite.CurrentTemplate = Request["siteTemplate"];

		_name = Request["name"];
		_templateName = Request["templateName"];
		_templatePath = Request["templatePath"];

        try
        {
            Title = GetMessage("DlgTitle.CopyingOfComponentTemplate");

            Behaviour.Settings.MinWidth = 560;
            Behaviour.Settings.MinHeight = 370;
            Behaviour.Settings.Width = 560;
            Behaviour.Settings.Height = 370;
            Behaviour.Settings.Resizeable = true;
            Behaviour.ValidationGroup = MainValidationGroupName;

            BXComponent component = Component;

            DescriptionParagraphs.Add(string.Format("<b>{0}</b>", HttpUtility.HtmlEncode(component.Title)));
            DescriptionParagraphs.Add(HttpUtility.HtmlEncode(component.Description));
            string componentPath = VirtualPathUtility.GetDirectory(BXComponentManager.GetPath(component.Name));

            DescriptionParagraphs.Add(string.Format("<a href=\"{0}?path={1}\" title=\"{3}\">{2}</a>", VirtualPathUtility.ToAbsolute("~/bitrix/admin/FileMan.aspx"), HttpUtility.UrlEncode(componentPath), HttpUtility.HtmlEncode(component.Name), HttpUtility.HtmlAttributeEncode(GetMessageRaw("Tooltip.ComponentFolder"))));
            DescriptionIconImageUrl = component.Icon;

            rbtnDefaultSiteTemplate.Name = SiteTemplateNameForNewItemElementName;
            rbtnActiveSiteTemplate.Name = SiteTemplateNameForNewItemElementName;
            rbtnAnotherSiteTemplate.Name = SiteTemplateNameForNewItemElementName;

            foreach (System.Collections.Generic.KeyValuePair<string, string> siteTemplate in SiteTemplates)
                ddlSiteTemplateName.Items.Add(new ListItem(HttpUtility.HtmlEncode(siteTemplate.Value), HttpUtility.HtmlAttributeEncode(siteTemplate.Key)));

            vldrRequiredNewTemplateName.Text = GetMessage("VALIDATOR_TEXT_NEW_TEMPLATE_NAME_IS_EMPTY");
            vldrRequiredNewTemplateName.ValidationGroup = MainValidationGroupName;

            vldrNewTemplateName.ValidationExpression = BXPath.NameValidationRegexString;
            vldrNewTemplateName.Text = GetMessage("ILLEGAL_CHARACTERS_IS_DETECTED_IN_NEW_TEMPLATE_NAME");
            vldrNewTemplateName.ValidationGroup = MainValidationGroupName;

            if (!IsPostBack)
                SetupByDefault();
        }
        catch (System.Threading.ThreadAbortException /*exc*/)
        {
            //...игнорируем, вызвано Close();
        }
        catch (Exception exc)
        {
            Close(exc.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!CheckUserAuthorization())
            return;
        try
        {
			if(string.IsNullOrEmpty(_name) 
				|| string.IsNullOrEmpty(_templateName)  
				|| string.IsNullOrEmpty(_templatePath))
			{
				Component.Visible = false;
				Component.Load += 
					delegate(object sender, EventArgs eventArgs)
					{
						_name = Component.Name;
						_templateName = Component.Template;
						_templatePath = Component.TemplatePath;
					};

				Controls.Add(Component);
			}

            if (IsPostBack)
            {
                NameForNewItem = tbxNewTemplateName.Text;
                if (string.IsNullOrEmpty(NameForNewItem))
                    ShowError(GetMessageRaw("ERROR_NEW_TEMPLATE_NAME_IS_NOT_DEFINED"));

                SetSiteTemplateNameForNewItemFromUserInput();

                if (string.IsNullOrEmpty(SiteTemplateNameForNewItem))
                {
                    ShowError(GetMessageRaw("ERROR_DESTINATION_SITE_TEMPLATE_NAME_IS_NOT_RESOLVED"));
                    SetupByDefault();
                }
            }

            Validate();
        }
        catch (System.Threading.ThreadAbortException /*exc*/)
        {
            //...игнорируем, вызвано Close();
        }
        catch (Exception exc)
        {
            Close(exc.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
        }
    }

	private void Component_Load(object sender, EventArgs e)
	{

	}

    protected override void OnError(EventArgs e)
    {
        base.OnError(e);
        Close(GetMessageRaw("ERROR_GENERAL"), BXDialogGoodbyeWindow.LayoutType.Error, -1);
    }


    /// <summary>
    /// Проверить авторизацию пользователя для работы с диалогом
    /// </summary>
    /// <returns></returns>
    protected override bool CheckUserAuthorization()
    {
        return BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
    }

    /// <summary>
    /// GetParamsBagName
    /// Имя хранилища параметров
    /// Помещается в сессию
    /// </summary>
    /// <returns></returns>
    protected override string GetParametersBagName()
    {
        return "BitrixDialogCopyComponentTemplateParamsBag";
    }

    /// <summary>
    /// ExternalizeParameters
    /// Выгрузить параметры
    /// </summary>
    /// <param name="paramsBag"></param>
    protected override void ExternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        paramsBag.Add(tbxNewTemplateName.ID, tbxNewTemplateName.Text);

        if(rbtnDefaultSiteTemplate.Checked)
            paramsBag.Add(rbtnDefaultSiteTemplate.ID, string.Empty);

        if (rbtnActiveSiteTemplate.Checked)
            paramsBag.Add(rbtnActiveSiteTemplate.ID, string.Empty);

        if (rbtnAnotherSiteTemplate.Checked)
        {
            paramsBag.Add(rbtnAnotherSiteTemplate.ID, string.Empty);
            paramsBag.Add(ddlSiteTemplateName.ID, ddlSiteTemplateName.Value);
        }

        paramsBag.Add(chbxApply.ID, chbxApply.Checked.ToString());
        paramsBag.Add(chbxGo2Modification.ID, chbxGo2Modification.Checked.ToString());
    }

    /// <summary>
    /// InternalizeParameters
    /// Загрузить параметры
    /// </summary>
    /// <param name="paramsBag"></param>
    protected override void InternalizeParameters(BXParamsBag<string> paramsBag)
    {
        if (paramsBag == null)
            throw new ArgumentNullException("paramsBag");

        if (paramsBag.ContainsKey(tbxNewTemplateName.ID))
            tbxNewTemplateName.Text = paramsBag[tbxNewTemplateName.ID];

        if (paramsBag.ContainsKey(rbtnDefaultSiteTemplate.ID))
        {
            rbtnDefaultSiteTemplate.Checked = true;
            rbtnActiveSiteTemplate.Checked = false;
            rbtnAnotherSiteTemplate.Checked = false;
            ddlSiteTemplateName.Disabled = true;
            ddlSiteTemplateName.SelectedIndex = 0;
        }
        else if (paramsBag.ContainsKey(rbtnActiveSiteTemplate.ID))
        {
            rbtnDefaultSiteTemplate.Checked = false;
            rbtnActiveSiteTemplate.Checked = true;
            rbtnAnotherSiteTemplate.Checked = false;
            ddlSiteTemplateName.Disabled = true;
            ddlSiteTemplateName.SelectedIndex = 0;
        }
        else if (paramsBag.ContainsKey(rbtnAnotherSiteTemplate.ID))
        {
            rbtnDefaultSiteTemplate.Checked = false;
            rbtnActiveSiteTemplate.Checked = false;
            rbtnAnotherSiteTemplate.Checked = true;
            ddlSiteTemplateName.Disabled = false;
            if (paramsBag.ContainsKey(ddlSiteTemplateName.ID)) 
            {
                string siteTemplateName = paramsBag[ddlSiteTemplateName.ID];
                int siteTemplateNamesCount = ddlSiteTemplateName.Items.Count, 
                    siteTemplateNameIndex = -1;
                for (int i = 0; i < siteTemplateNamesCount; i++)
                {
                    if (!string.Equals(ddlSiteTemplateName.Items[i].Value, siteTemplateName, StringComparison.InvariantCulture))
                        continue;
                    siteTemplateNameIndex = i;
                    break;
                }

                if (siteTemplateNameIndex >= 0)
                    ddlSiteTemplateName.SelectedIndex = siteTemplateNameIndex;
                else if(siteTemplateNamesCount > 0)
                    ddlSiteTemplateName.SelectedIndex = 0;
            }
            else if (ddlSiteTemplateName.Items.Count > 0)
                ddlSiteTemplateName.SelectedIndex = 0;
        }

        SetSiteTemplateNameForNewItemFromUserInput();

        try
        {
            if (paramsBag.ContainsKey(chbxApply.ID))
                chbxApply.Checked = Convert.ToBoolean(paramsBag[chbxApply.ID]);
            if (paramsBag.ContainsKey(chbxGo2Modification.ID))
                chbxGo2Modification.Checked = Convert.ToBoolean(paramsBag[chbxGo2Modification.ID]);
        }
        catch (FormatException /*exc*/) { } //подавление исключений при конвертации
    }


    private void SetupByDefault()
    {
        rbtnDefaultSiteTemplate.Checked = true;
        rbtnActiveSiteTemplate.Checked = false;
        rbtnAnotherSiteTemplate.Checked = false;
        ddlSiteTemplateName.Disabled = true;
        ddlSiteTemplateName.SelectedIndex = 0;

        NameForNewItem = CreateDefaultNameForNewItem(null);
        tbxNewTemplateName.Text = NameForNewItem;
        SiteTemplateNameForNewItem = DefaultSiteTemplateName;

        chbxApply.Checked = true;
        chbxGo2Modification.Checked = true;
    }

    protected void Behaviour_Save(object sender, EventArgs e)
    {
        BXUser.DemandOperations(BXRoleOperation.Operations.ProductSettingsManage);

        if(!IsValid || ErrorsCount > 0)
            return;

        try
        {
            #region Processing of the template
            string siteTemplateName = GetOwnerSiteTemplateName();

            string sourcePath = string.IsNullOrEmpty(siteTemplateName)
                ? BXPath.Combine(
                    GetComponentTemplatesPath(null),
                    TemplateName
                  )
                : BXPath.Combine(
                    GetSiteComponentTemplatesPath(siteTemplateName, null),
                    TemplateName
                  );

            string destinationSiteTemplateName = SiteTemplateNameForNewItem;
            string destinationSiteTemplatePath = GetSiteTemplatePath(destinationSiteTemplateName);


            if (!BXSecureIO.DirectoryExists(destinationSiteTemplatePath))
            {
                ShowError(string.Format(GetMessageRaw("ERROR_FORMATTED_SITE_TEMPLATE_IS_NOT_FOUND"), destinationSiteTemplateName));
                return;
            }
            string destinationPath = BXPath.Combine(
                GetSiteComponentTemplatesPath(destinationSiteTemplateName, null),
                NameForNewItem
                );

            if (BXSecureIO.DirectoryExists(destinationPath))
            {
                ShowError(string.Format(GetMessageRaw("ERROR_FORMATTED_COMPONENT_TEMPLATE_ALREADY_EXISTS"), NameForNewItem, destinationSiteTemplateName));
                return;
            }

            BXSecureIO.DirectoryCopy(sourcePath, destinationPath);
            #endregion

			BXUndoCTCopyOperation undo = new BXUndoCTCopyOperation();
			undo.TemplateVirtualPath = VirtualPathUtility.AppendTrailingSlash(destinationPath);

            #region Processing of dependencies
            string dependenciesFileName = "dependencies.config";
            string dependenciesFilePath = VirtualPathUtility.Combine(
                    VirtualPathUtility.AppendTrailingSlash(destinationPath),
                    dependenciesFileName
                    );

            if (BXSecureIO.FileExists(dependenciesFilePath))
            {
                List<Dictionary<string, string>> dependencyList = null;
                using (TextReader tr = BXSecureIO.FileReadToTextReader(dependenciesFilePath))
                {
                    XmlReaderSettings xrSettings = new XmlReaderSettings();
                    xrSettings.IgnoreComments = true;
                    xrSettings.IgnoreProcessingInstructions = true;
                    xrSettings.IgnoreWhitespace = true;
                    using (XmlReader xr = XmlReader.Create(tr, xrSettings))
                    {
                        string name = null,
                            template = null;
                        while (xr.Read())
                        {
                            if (xr.NodeType != XmlNodeType.Element
                                || !string.Equals(xr.Name, "component", StringComparison.InvariantCultureIgnoreCase))
                                continue;

                            name = xr.MoveToAttribute("name") ? xr.Value : null;
                            template = xr.MoveToAttribute("template") ? xr.Value : null;

                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(template))
                            {
                                Dictionary<string, string> dependencyDic = new Dictionary<string, string>();
                                dependencyDic.Add("name", name);
                                dependencyDic.Add("template", template);

                                if (dependencyList == null)
                                    dependencyList = new List<Dictionary<string, string>>();
                                dependencyList.Add(dependencyDic);
                            }
                            xr.MoveToElement();
                        }
                    }
                }
                if (dependencyList != null)
                {
                    string parentMacroParameter = "%PARENT%";
                    foreach (Dictionary<string, string> dependency in dependencyList)
                    {
                        string name = dependency["name"],
                            template = dependency["template"];
                        string sourceDir = BXComponentManager.GetTemplateVirtualDir(name, template.Replace(parentMacroParameter, TemplateName), siteTemplateName, null);
                        if (string.IsNullOrEmpty(sourceDir))
                            continue;


                        //string destinationDir = BXPath.Combine(GetSiteComponentTemplatesPath(destinationSiteTemplateName, name), template.Replace(parentMacroParameter, NameForNewItem));
                        string destinationDir = BXPath.Combine(destinationPath, GetComponentTemplatePartialPath(name, template.Replace(parentMacroParameter, NameForNewItem)));

                        if (BXSecureIO.DirectoryExists(destinationDir))
                            continue;

                        BXSecureIO.DirectoryEnsureExists(destinationDir);
                        BXSecureIO.DirectoryCopy(sourceDir, destinationDir);
                    }
                }

                if (BXSecureIO.FileExists(dependenciesFilePath))
                    BXSecureIO.FileDelete(dependenciesFilePath);
            }

            #endregion

            #region Pocessing of the target file


            if (chbxApply.Checked
                && (IsDefaultSiteActive ? rbtnDefaultSiteTemplate.Checked : rbtnActiveSiteTemplate.Checked))
            {
				undo.PageUndo = new BXUndoPageModificationOperation();
				IList<BXSite> sites = BXSite.GetSitesForPath(TargetFileAppRelPath);
				undo.PageUndo.SiteId = (sites.Count > 0 ? sites[0]  : BXSite.DefaultSite).Id;
				undo.PageUndo.FileVirtualPath = TargetFileAppRelPath;
				undo.PageUndo.FileEncodingName = BXConfigurationUtility.DefaultEncoding.WebName;
				undo.PageUndo.FileContent = BXSecureIO.FileReadAllText(undo.PageUndo.FileVirtualPath, BXConfigurationUtility.DefaultEncoding);

                BXPageProxy page = BXPageProxy.Create(TargetFileAppRelPath);
                BXComponentProxy proxy = page.ResolveControlId(TargetElementID);
                proxy.Parameters["Template"].SelectedValue = NameForNewItem;

                if (page.IsDirty)
                    page.Save();
            }
            #endregion

			BXUndoInfo undoInfo = new BXUndoInfo();
			undoInfo.Operation = undo;
			undoInfo.Save();

			BXDialogGoodbyeWindow goodbye = new BXDialogGoodbyeWindow(string.Format(
				GetMessageRaw("NewTemplateIsSuccessfullyCreated"), 
				string.Concat(undoInfo.GetClientScript(), " return false;"), 
				"#"), -1, BXDialogGoodbyeWindow.LayoutType.Success);
			BXDialogGoodbyeWindow.SetCurrent(goodbye);

            #region Redirect to new template modification and completion
            if (chbxGo2Modification.Checked)
            {
                string componentTemplateFileName = VirtualPathUtility.GetFileName(TemplatePath);
                string destinationTemplateFilePath = VirtualPathUtility.Combine(VirtualPathUtility.AppendTrailingSlash(destinationPath), componentTemplateFileName);

                if (!BXSecureIO.FileExists(destinationTemplateFilePath))
                {
                    ShowError(string.Format(GetMessageRaw("ERROR_FORMATTED_COMPONENT_TEMPLATE_IS_NOT_FOUND_IN_SITE_TEMPLATE"), NameForNewItem, destinationSiteTemplateName));
                    return;
                }

                /*string redirectionUrl = string.Format(
                    "{0}?path={1}&{2}={3}",
                    VirtualPathUtility.ToAbsolute("~/bitrix/admin/FileManEdit.aspx"),
                    HttpUtility.UrlEncode(destinationTemplateFilePath),
                    BXConfigurationUtility.Constants.BackUrl,
                    HttpUtility.UrlEncode(BackUrl));

                Redirect(redirectionUrl, string.Format("{0}. {1}...", GetMessageRaw("OPERATION_COMPLETED_SUCCESSFULLY"), GetMessageRaw("EDITOR_OPENING")), BXDialogGoodbyeWindow.LayoutType.Success, 800);
				*/

				BXDialogSettings dlgSetting = new BXDialogSettings();
				dlgSetting.Height = 604;
				dlgSetting.Width = 968;
				dlgSetting.MinHeight = 400;
				dlgSetting.MinWidth = 780;

				SwitchToDialog(
					string.Format(
						"{0}?path={1}&clientType=WindowManager&vpe_mode=PlainText&noundo=", 
						VirtualPathUtility.ToAbsolute("~/bitrix/dialogs/VisualPageEditor.aspx"), 
						HttpUtility.UrlEncode(destinationTemplateFilePath)), 
						dlgSetting, 
						GetMessage("NewTemplateIsSuccessfullyCreatedShowEditDialog"), 
						BXDialogGoodbyeWindow.LayoutType.Success, 2000);				
            }

			Refresh(string.Empty, BXDialogGoodbyeWindow.LayoutType.Success, -1);
            #endregion
        }
        catch (System.Threading.ThreadAbortException)
        {
            //...игнорируем, вызвано Close();
        }
        catch (Exception exc)
        {
            Close(exc.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
        }
    }

    /// <summary>
    /// Проверка является ли текущим сайт по умолчанию
    /// </summary>
    protected bool IsDefaultSiteActive
    {
        get 
        {
            return string.Equals(GetActiveSiteTemplateName(), BXConfigurationUtility.Constants.DefaultTemplate, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    /// <summary>
    /// Отображение опции "Копировать в текущий шаблон сайта"
    /// </summary>
    protected bool AboutCopy2ActiveSiteTemplateOptionDisplay
    {
        get { return !IsDefaultSiteActive; }
    }
    /// <summary>
    /// Запрос имени шаблона сайта задействованного в данном контексте (др. словами текущего шаблона сайта).
    /// </summary>
    /// <returns></returns>
    protected string GetActiveSiteTemplateName()
    { 
        return BXSite.CurrentTemplate; 
    }

    /// <summary>
    /// Запрос имени шаблона сайта владельца текущего шаблона компонента.
    /// Возвращается часть пути к шаблону компонента "SITE_TEMPLATE" из "~/bitrix/templates/[SITE_TEMPLATE]/"
    /// Если путь к шаблону компонента не принадлежит "~/bitrix/templates/", то возвращается string.Empty.
    /// </summary>
    /// <returns></returns>
    protected string GetOwnerSiteTemplateName() 
    {
        string componentTemplatePath = TemplatePath;
        if (string.IsNullOrEmpty(componentTemplatePath))
            throw new InvalidOperationException(GetMessageRaw("ERROR_COMPONENT_TEMPLATE_IS_NOT_DEFINED"));

        if (!VirtualPathUtility.IsAppRelative(componentTemplatePath))
            throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_PATH_IS_NOT_APP_RELATIVE"), componentTemplatePath));

        string templatesFolderPath = BXConfigurationUtility.Constants.TemplatesFolderPath;
        if (string.IsNullOrEmpty(templatesFolderPath))
            throw new InvalidOperationException(GetMessageRaw("ERROR_SYSTEM_TEMPLATE_FOLDER_PATH_IS_NOT_ASSIGNED"));

        if (!VirtualPathUtility.IsAppRelative(templatesFolderPath))
            throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_PATH_IS_NOT_APP_RELATIVE"), templatesFolderPath));

        templatesFolderPath = VirtualPathUtility.AppendTrailingSlash(templatesFolderPath);

        if (componentTemplatePath.Equals(VirtualPathUtility.RemoveTrailingSlash(templatesFolderPath), StringComparison.InvariantCultureIgnoreCase))
            throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_COMPONENT_TEMPLATE_PATH_DOESNT_CONTAINS_SITE_TEMPLATE_NAME"), componentTemplatePath));

        if (!componentTemplatePath.StartsWith(templatesFolderPath, StringComparison.InvariantCultureIgnoreCase))
            return string.Empty;

        if (componentTemplatePath.Length == templatesFolderPath.Length)
            throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_COMPONENT_TEMPLATE_PATH_DOESNT_CONTAINS_SITE_TEMPLATE_NAME"), componentTemplatePath));

        int resultStartIndex = templatesFolderPath.Length, 
            resultEndIndex = componentTemplatePath.IndexOf('/', templatesFolderPath.Length);

        return resultEndIndex >= 0 ? 
            componentTemplatePath.Substring(resultStartIndex, resultEndIndex - resultStartIndex) : 
            componentTemplatePath.Substring(resultStartIndex);
    }

    private Dictionary<string, string> _siteTemplates = null;
    protected IDictionary<string, string> SiteTemplates 
    {
        get 
        {
            if (_siteTemplates == null)
            {
                _siteTemplates = new Dictionary<string, string>();

                string templatesFolderPath = BXConfigurationUtility.Constants.TemplatesFolderPath;
                if (string.IsNullOrEmpty(templatesFolderPath))
                    throw new InvalidOperationException(GetMessageRaw("ERROR_SYSTEM_TEMPLATE_FOLDER_PATH_IS_NOT_ASSIGNED"));

                if (!VirtualPathUtility.IsAppRelative(templatesFolderPath))
                    throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_PATH_IS_NOT_APP_RELATIVE"), templatesFolderPath));

                templatesFolderPath = VirtualPathUtility.AppendTrailingSlash(templatesFolderPath);

                foreach(string siteTemplateFolder in BXSecureIO.DirectoryGetDirectories(templatesFolderPath))
                {
                    if (!BXSecureIO.FileExists(VirtualPathUtility.Combine(VirtualPathUtility.Combine(templatesFolderPath, VirtualPathUtility.AppendTrailingSlash(siteTemplateFolder)), BXConfigurationUtility.Constants.TemplateFileName))
                        || string.Equals(siteTemplateFolder, BXConfigurationUtility.Constants.DefaultTemplate, StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(siteTemplateFolder, GetActiveSiteTemplateName(), StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    _siteTemplates.Add(siteTemplateFolder, siteTemplateFolder);
                }
            }
            return _siteTemplates;
        }
    }

    private bool _isComponentLoaded = false;
    private BXComponent _component = null;

	private void EnsureComponentLoaded()
	{
		if (_isComponentLoaded)
			return;

		try
		{
			_component = BXIncludeComponentHelper.ReadFromFile(TargetFileAppRelPath, TargetElementID, null);//new string[] { "Template" });
		}
		catch (FileIsNotFoundException exc)
		{
			throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_SOURCE_FILE_IS_NOT_FOUND"), TargetFileAppRelPath), exc);
		}
		catch (FileIsEmptyException exc)
		{
			throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_SOURCE_FILE_IS_EMPTY"), TargetFileAppRelPath), exc);
		}
		catch (ElementIsNotFoundException exc)
		{
			throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_COMPONENT_ELEMENT_IS_NOT_FOUND"), TargetElementID, TargetFileAppRelPath), exc);
		}
		catch (ElementAttributesParsingException exc)
		{
			throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_COMPONENT_ELEMENT_COULD_NOT_PARSE_ATTRIBUTES"), TargetElementID, TargetFileAppRelPath), exc);
		}
		catch (ElementComponentNameIsNotFoundException exc)
		{
			throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_COMPONENT_ELEMENT_NAME_ATTR_IS_NOT_FOUND"), TargetElementID, TargetFileAppRelPath), exc);
		}
		catch (ComponentPathIsNotFoundException exc)
		{
			throw new InvalidOperationException(string.Format(GetMessageRaw("ERROR_FORMATTED_COMPONENT_PATH_IS_NOT_FOUND"), exc.Name), exc);
		}
		catch (ComponentCreationFailedException exc)
		{
			throw new InvalidOperationException(GetMessageRaw("ERROR_COMPONENT_IS_NOT_FOUND"), exc);
		}

		/*
		 * теперь параметры получаются из запроса
		//Нужно для прохождения компонентом "жизненного цикла" в странице. Иначе TemplatePath получить несудьба :(
		_component.Visible = false;
		Controls.Add(_component);
		//Controls.Remove(_component);
		 */

		_isComponentLoaded = true;
	}

    /// <summary>
    /// Компонент
    /// </summary> 
    protected BXComponent Component 
    { 
        get 
        {
            EnsureComponentLoaded();
            return _component; 
        } 
    }

    /// <summary>
    /// Системное имя компонента
    /// </summary>
    private string _name = null;
    protected string Name
    {
        get { return _name ?? string.Empty; }
    }

	private string _templatePath = null;
	protected string TemplatePath
	{
		get { return _templatePath ?? string.Empty; }
	}

    private string _templateName = string.Empty;
    /// <summary>
    /// Имя шаблона
    /// </summary>
    protected string TemplateName 
    {
        get { return _templateName ?? string.Empty; }
    }

    private ComponentTemplateType? _templateType = null;
    /// <summary>
    /// Тип шаблона.
    /// Если путь к шаблону компонента принадлежит "~/bitrix/templates/", 
    /// то ComponentTemplateType.Site иначе ComponentTemplateType.System.
    /// </summary>
    protected ComponentTemplateType TemplateType 
    { 
        get 
        {
            if (!_templateType.HasValue)
                _templateType = string.IsNullOrEmpty(GetOwnerSiteTemplateName()) ? ComponentTemplateType.System : ComponentTemplateType.Site; 
            return _templateType.Value;
        } 
    }

    /// <summary>
    /// Создать имя для нового шаблона компонента по умолчанию
    /// </summary>
    /// <returns></returns>
    private string CreateDefaultNameForNewItem(string siteTemplateName)
    {
        if (string.IsNullOrEmpty(siteTemplateName))
            siteTemplateName = BXConfigurationUtility.Constants.DefaultTemplate;
   
        string templateName = TemplateName;

        if (string.Equals(templateName, BXConfigurationUtility.Constants.DefaultTemplate, StringComparison.InvariantCulture))
            templateName = "template";

        int num = 0,
            numDigits = 1,
            digitPos = templateName.Length;

        while (digitPos > 0 && Char.IsDigit(templateName, digitPos - 1))
            digitPos--;

        if (digitPos < templateName.Length)
        {
            string numStr = templateName.Substring(digitPos);
            numDigits = numStr.Length;
            num = int.Parse(numStr);
            templateName = templateName.Remove(digitPos);
        }
        string newTemplate = string.Empty,
            newTemplatePath = string.Empty,
            siteComponentTemplatesPath = string.Empty;

        siteComponentTemplatesPath = GetSiteComponentTemplatesPath(siteTemplateName, null);

        do
        {
            num++;
            newTemplate = templateName + num.ToString().PadLeft(numDigits, '0');
            newTemplatePath = BXPath.Combine(siteComponentTemplatesPath, newTemplate);
        } while (BXSecureIO.FileOrDirectoryExists(newTemplatePath));
        return newTemplate;
    }


    /// <summary>
    /// Получить путь к шаблону сайта
    /// </summary>
    /// <param name="siteTemplateName">Имя шаблона сайта</param>
    /// <returns></returns>
    private string GetSiteTemplatePath(string siteTemplateName)
    {
        if (string.IsNullOrEmpty(siteTemplateName))
            throw new ArgumentException("Is not defined!", "siteTemplateName");

        return BXPath.Combine(
            BXConfigurationUtility.Constants.TemplatesFolderPath,
            siteTemplateName
            );
    }

    /// <summary>
    /// Получить путь к шаблонам компонентов шаблона сайта
    /// </summary>
    /// <param name="siteTemplateName"></param>
    /// <returns></returns>
    private string GetSiteComponentTemplatesPath(string siteTemplateName, string componentName)
    {
        if(string.IsNullOrEmpty(siteTemplateName))
            throw new ArgumentException("Is not defined!", "siteTemplateName");

        if (string.IsNullOrEmpty(componentName))
            componentName = Name;

        return  BXPath.Combine(
            GetSiteTemplatePath(siteTemplateName),
            "components",
            GetComponentPartialPath(componentName)
            );
    }

    /// <summary>
    /// Получить "частичный" путь к компоненту
    /// </summary>
    /// <returns></returns>
    protected string GetComponentPartialPath(string componentName)
    {
        if (string.IsNullOrEmpty(componentName))
            componentName = Name;
        return componentName.Replace(':', System.IO.Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Получить "частичный" путь к шаблону компонента
    /// </summary>
    /// <returns></returns>
    protected string GetComponentTemplatePartialPath(string componentName, string templateName)
    {
        if (string.IsNullOrEmpty(componentName))
            componentName = Name;

        if (string.IsNullOrEmpty(templateName))
            templateName = TemplateName;

        return string.Concat(VirtualPathUtility.AppendTrailingSlash(GetComponentPartialPath(componentName)), templateName);
    }

    /// <summary>
    /// Получить путь к системным шаблонам компонентов
    /// </summary>
    /// <returns></returns>
    protected string GetComponentTemplatesPath(string componentName)
    {
        if (string.IsNullOrEmpty(componentName))
            componentName = Name;
        return BXPath.Combine(
                BXConfigurationUtility.Constants.ComponentsFolder,
                GetComponentPartialPath(componentName),
                "templates"
              );
    }

    private string _nameForNewItem = string.Empty;
    /// <summary>
    /// Имя для копии шаблона компонента
    /// </summary>
    protected string NameForNewItem
    {
        get { return _nameForNewItem; }
        set { _nameForNewItem = value; }
    }
    /// <summary>
    /// Имя эл-та "Название нового шаблона компонента"
    /// </summary>
    //protected string NameForNewItemElementName
    //{
    //    get { return "TEMPLATE_NAME"; }
    //}

    /// <summary>
    /// Имя эл-та "Копировать в шаблон сайта".
    /// </summary>
    protected string SiteTemplateNameForNewItemElementName
    {
        get { return "SITE_TEMPLATE_NAME"; }
    }

    private string _siteTemplateNameForNewItem = string.Empty;
    /// <summary>
    /// Имя шаблона сайта, в который будет осуществляться копирование
    /// </summary>
    protected string SiteTemplateNameForNewItem
    {
        get { return _siteTemplateNameForNewItem; }
        set { _siteTemplateNameForNewItem = value; }
    }

    /// <summary>
    /// Установить имя шаблона сайта по пользовательскому выбору
    /// </summary>
    protected void SetSiteTemplateNameForNewItemFromUserInput()
    {
        if (rbtnDefaultSiteTemplate.Checked)
            _siteTemplateNameForNewItem = DefaultSiteTemplateName;
        else if (rbtnActiveSiteTemplate.Checked)
            _siteTemplateNameForNewItem = GetActiveSiteTemplateName();
        else if (rbtnAnotherSiteTemplate.Checked)
            _siteTemplateNameForNewItem = ddlSiteTemplateName.Value;
        else
            _siteTemplateNameForNewItem = string.Empty;
    }

    protected string DefaultSiteTemplateName
    {
        get { return BXConfigurationUtility.Constants.DefaultTemplate; }
    }

    /// <summary>
    /// Имя главной группы валидации
    /// </summary>
    protected string MainValidationGroupName
    {
        get{ return "CopyComponentTemplateDialog"; }
    }
}
