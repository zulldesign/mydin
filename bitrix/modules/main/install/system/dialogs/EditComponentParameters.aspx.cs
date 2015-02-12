using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Bitrix;
using Bitrix.Components;
using Bitrix.Components.Editor;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.UI;
using Bitrix.Services.Undo;
using System.Text;

public partial class bitrix_dialogs_EditComponentParameters : BXDialogPage
{
    Dictionary<string, BXParamDynamicExp> dParams;
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        if (!CheckUserAuthorization())
            return;
        try 
        {
			if (Request["siteTemplate"] != null)
				BXSite.CurrentTemplate = Request["siteTemplate"];
            EnsureChildControls();

            Title = GetMessage("DIALOG_TITLE_EDIT_COMPONENT_PARAMETERS");

            Behaviour.Settings.MinWidth = 500;
            Behaviour.Settings.MinHeight = 300;
            Behaviour.Settings.Width = 700;
            Behaviour.Settings.Height = 400;
            Behaviour.Settings.Resizeable = true;
            Behaviour.ValidationGroup = MainValidationGroupName;

            BXComponent component = Component;

			DescriptionTitle = HttpUtility.HtmlEncode(component.Title);
			DescriptionContent = HttpUtility.HtmlEncode(component.Description);
            string componentPath = BXComponentManager.GetPath(component.Name);
            if (string.IsNullOrEmpty(componentPath) || !VirtualPathUtility.IsAppRelative(componentPath))
                throw new InvalidOperationException(string.Format("Component path is not valid: \"{0}\"!", componentPath));
            string directoryPath = VirtualPathUtility.ToAbsolute(VirtualPathUtility.RemoveTrailingSlash(VirtualPathUtility.GetDirectory(componentPath)));
			DescriptionNotes = string.Format("<a href=\"{0}?path={1}\">{2}</a>", VirtualPathUtility.ToAbsolute("~/bitrix/admin/FileMan.aspx"), HttpUtility.UrlEncode(directoryPath), HttpUtility.HtmlEncode(component.Name));
            DescriptionIconImageUrl = component.Icon;


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


    //protected override void OnError(EventArgs e)
    //{
    //    base.OnError(e);
    //    Close(GetMessageRaw("ERROR_GENERAL"), BXDialogGoodbyeWindow.LayoutType.Error, -1);
    //}

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ScriptManager.RegisterClientScriptBlock(this,
            GetType(),
            "ParamClientSideActionGroupViewGetElementContainerDeclaration",
            @"window.ParamClientSideActionGroupViewGetElementContainer = function(elId){
			    var el = Bitrix.TypeUtility.isNotEmptyString(elId) ? window.document.getElementById(elId) : null;
				if(el == null) return null;
				var parent = el.parentNode;
				while(parent){
				    if(typeof(parent.className) != 'undefined' && parent.className == 'paramcontainer')
					    return parent;
						parent = parent.parentNode;
					}
					return null;
			}
            Bitrix.ComponentParametersEditor.getInstance().reset(); 
            Bitrix.ComponentParametersEditor.getInstance().setViewContainerElementByViewIdFunction(window.ParamClientSideActionGroupViewGetElementContainer);
			",
            true);
    }


    private List<string> _parameterControlIds = new List<string>();
    protected override void CreateChildControls()
    {
		if (!CheckUserAuthorization())
			return;

        dParams = new Dictionary<string, BXParamDynamicExp>();
        string prefixText, expressionText;
		BXDynamicExpressionCalculator calculator = new BXDynamicExpressionCalculator();
		calculator.SiteId = Request["site"];
        foreach (string paramKey in Component.Parameters.Keys)
        {
            string param = Component.Parameters[paramKey];
            if (BXDynamicExpressionHelper.ParseExpressionText(param, false, out prefixText, out expressionText))
            {
               BXParamDynamicExp dynExObj = new BXParamDynamicExp(param, prefixText, expressionText, calculator);
               dParams.Add(paramKey, dynExObj);
            }
        }

        foreach (string paramKey in dParams.Keys)
            Component.SetParameterValue(paramKey, dParams[paramKey].Result, false);

        Component.ForceLoadComponentDefinition();

        foreach (string paramKey in dParams.Keys)
            Component.SetParameterValue(paramKey, dParams[paramKey].TextValue, false);

		string currentTemplate = null;
		Dictionary<string, object> loaded = new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, BXParam> parameterKeyValuePair in Component.ParamsDefinition)
        {
            string key = parameterKeyValuePair.Key;
            BXParam parameter = parameterKeyValuePair.Value;
            if (parameter == null)
                throw new InvalidOperationException(string.Format("Param for key '{0}' is not assigned!", key));

			
            if (dParams.ContainsKey(key))
				parameter.SelectedValue = dParams[key].Result;

			BXComponentParameterView templateView = null;
			if (string.Equals(key, "Template", StringComparison.OrdinalIgnoreCase))
				templateView = new BXComponentParameterViewSingleSelection(Component, key);
			BXComponentParameterView control = new BXComponentParameterViewComboExpressionEditor(
				templateView ?? BXComponentParameterViewFactory.Create(Component, key, parameter)
			);

            componentParameters.Controls.Add(control);
            control.ClientScript = ClientScript;
            _parameterControlIds.Add(control.UniqueID);
			loaded[key] = null;

			if (templateView != null)
				currentTemplate = Request.Form[templateView.UniqueID];	
        }
		if (currentTemplate != null)
			Component.Parameters["Template"] = currentTemplate;


		//Append template parameters
		Component.ForcePreLoadTemplateDefinition();
		foreach (KeyValuePair<string, BXParam> kv in Component.ParamsDefinition)
		{
			if (!Component.Parameters.ContainsKey(kv.Key))
				Component.Parameters.Add(kv.Key, kv.Value.DefaultValue);
		}
		Component.ForceLoadTemplateDefinition();

		foreach (KeyValuePair<string, BXParam> parameterKeyValuePair in Component.ParamsDefinition)
        {
            string key = parameterKeyValuePair.Key;
            if (loaded.ContainsKey(key))
				continue;
			BXParam parameter = parameterKeyValuePair.Value;
            if (parameter == null)
                throw new InvalidOperationException(string.Format("Param for key '{0}' is not assigned!", key));

			BXComponentParameterView control = new BXComponentParameterViewComboExpressionEditor(
				BXComponentParameterViewFactory.Create(Component, key, parameter)
			);

            componentParameters.Controls.Add(control);
            control.ClientScript = ClientScript;
            _parameterControlIds.Add(control.UniqueID);
        }
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
    /// Имя главной группы валидации
    /// </summary>
    protected string MainValidationGroupName
    {
        get { return "EditComponentParametersDialog"; }
    }
    private bool _isComponentLoaded = false;
    private BXComponent _component = null;
    private void EnsureComponentLoaded()
    {
        if (_isComponentLoaded)
            return;

		try
		{
			_component = BXIncludeComponentHelper.ReadFromFile(TargetFileAppRelPath, TargetElementID, null, false, Request.QueryString["site"]);
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

    //кэшируемые категории параметров
    BXCategory[] _componentParameterCategories = null;
    /// <summary>
    /// Категории параметров компонента
    /// </summary>
    /// <returns></returns>
    protected BXCategory[] ComponentParameterCategories
    {
        get
        {
            if(_componentParameterCategories == null)
            {
                IList<BXCategory> resultList = Component.ParamsDefinition.SortedCategories;
                int resultItemsCount = resultList != null ? resultList.Count : 0;
                _componentParameterCategories = new BXCategory[resultItemsCount];
                for (int i = 0; i < resultItemsCount; i++)
                    _componentParameterCategories[i] = resultList[i];
            }

            return _componentParameterCategories;
        }
    }

    /// <summary>
    /// Получить определения параметров компонента по категории
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    protected BXParametersDefinition GetComponentParametersDefinitionByCategory(BXCategory category)
    {
        if (category == null)
            throw new ArgumentNullException("category");

        return Component.ParamsDefinition.GetByCategory(category);
    }

    /// <summary>
    /// Сгенерировать разметку для параметра компонента
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    protected string RenderComponentRow(BXParam parameter, string key)
    {
        if (parameter == null)
            throw new ArgumentNullException("parameter");
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Is not assigned!", "key");

        HtmlTableRow row = new HtmlTableRow();
        row.ID = string.Concat("paramcontainer_", HttpUtility.HtmlAttributeEncode(key));
        row.Attributes.Add("class", "paramcontainer");
        if (!parameter.Visible)
            row.Attributes.Add("style", "display:none;");
        HtmlTableCell legendCell = new HtmlTableCell();
        row.Cells.Add(legendCell);
        legendCell.Attributes.Add("class", "bx-popup-label bx-width50");
        legendCell.InnerHtml = string.Concat(HttpUtility.HtmlEncode(parameter.Title), ":");

        HtmlTableCell dataCell = new HtmlTableCell();
        row.Cells.Add(dataCell);

        BXComponentParameterView parameterControl = (BXComponentParameterView)componentParameters.FindControl(BXComponentParameterView.GetDefaultID(Component, key));
		if (parameterControl == null)
			throw new InvalidOperationException(string.Format("Could not find control for component parameter key '{0}'!", key));

        HtmlGenericControl dataContainer = new HtmlGenericControl("DIV");
        dataContainer.Attributes.CssStyle["float"] = "left !important";
        dataContainer.Controls.Add(parameterControl);
        dataCell.Controls.Add(dataContainer);

        string refreshButtonContainerID = string.Concat(parameterControl.ClientID, ClientIDSeparator, "refreshButtonContainer");
        HtmlGenericControl refreshButtonContainer = new HtmlGenericControl("DIV");
        refreshButtonContainer.ID = refreshButtonContainerID;
        refreshButtonContainer.Attributes.Add("class", "parameter-buttons-container");
        dataCell.Controls.Add(refreshButtonContainer);

        string switchModeButtonContainerID = string.Concat(parameterControl.ClientID, ClientIDSeparator, "switchModeButtonContainer");
        HtmlGenericControl switchModeButtonContainer = new HtmlGenericControl("DIV");
        switchModeButtonContainer.Attributes.Add("class", "parameter-buttons-container");
        switchModeButtonContainer.ID = switchModeButtonContainerID;
        dataCell.Controls.Add(switchModeButtonContainer);

        dataCell.Attributes.Add("onmouseover", string.Format("if(typeof(this.setVisibleTask) != 'undefined' && this.setVisibleTask != null){{ window.clearTimeout(this.setVisibleTask); this.setVisibleTask = null; }} if(typeof(this.setNotVisibleTask) != 'undefined' && this.setNotVisibleTask != null){{ window.clearTimeout(this.setNotVisibleTask); this.setNotVisibleTask = null; }} this.setVisibleTask = window.setTimeout(function(){{ var el = window.document.getElementById('{0}'); if(el) el.style.visibility = 'visible';}}, 500);", switchModeButtonContainer.ClientID));
        dataCell.Attributes.Add("onmouseout", string.Format("if(typeof(this.setNotVisibleTask) != 'undefined' && this.setNotVisibleTask != null){{ window.clearTimeout(this.setNotVisibleTask); this.setNotVisibleTask = null; }} if(typeof(this.setVisibleTask) != 'undefined' && this.setVisibleTask != null){{ window.clearTimeout(this.setVisibleTask); this.setVisibleTask = null; }} this.setNotVisibleTask = window.setTimeout(function(){{var view = Bitrix.ComponentParametersEditor.getInstance().getView('{0}'); if(view.getModificationMode().getCurrentID() == {1}) return; var el = window.document.getElementById('{2}'); if(el) el.style.visibility = 'hidden';}}, 75);", parameterControl.ClientID, Convert.ToInt32(BXComponentParameterModificationMode.Expression), switchModeButtonContainer.ClientID));

        BXComponentParameterViewComboExpressionEditor parameterComboExpEd = parameterControl as BXComponentParameterViewComboExpressionEditor;
        if (parameterComboExpEd != null)
        {
            string switchModeImgID = string.Concat(parameterComboExpEd.ClientID, ClientIDSeparator, "imgSwitchModificationMode"),
            //string switchModeImgID = "imgSwitchModificationMode",
                urlToModificationModeTextImg = BXThemeHelper.AddAbsoluteThemePath("images/components/exp_off.gif"),
                urlToModificationModeExprImg = BXThemeHelper.AddAbsoluteThemePath("images/components/exp_on.gif");
            HtmlImage switchModeImg = new HtmlImage();
            switchModeImg.ID = switchModeImgID;
            switchModeImg.Attributes.Add("class", "parameter-button mode");

            string switchModeImgAlt2Expr = GetMessage("IMG_TITLE_SWITCH_TO_DYN_EXP_ED"),
                switchModeImgAlt2Text = GetMessage("IMG_TITLE_SWITCH_TO_STD_ED");

            
            if (parameterComboExpEd.GetModificationMode() == BXComponentParameterModificationMode.Standard)
            {
                switchModeImg.Alt = switchModeImgAlt2Expr;
                switchModeImg.Attributes.Add("title", switchModeImgAlt2Expr);
                switchModeImg.Src = urlToModificationModeTextImg;
                switchModeButtonContainer.Attributes.CssStyle[HtmlTextWriterStyle.Visibility] = "hidden";
            }
            else 
            {
                switchModeImg.Alt = switchModeImgAlt2Text;
                switchModeImg.Attributes.Add("title", switchModeImgAlt2Text);
                switchModeImg.Src = urlToModificationModeExprImg;
                switchModeButtonContainer.Attributes.CssStyle[HtmlTextWriterStyle.Visibility] = "visible";
            }

            switchModeImg.Attributes.Add("onclick", string.Format("var view = Bitrix.ComponentParametersEditor.getInstance().getView('{0}'); view.switchModificationMode(); if(view.getModificationMode().getCurrentID() == {1}){{ this.src = '{3}'; this.alt = '{4}'; this.title = '{4}'; }}else {{ this.src = '{2}'; this.alt = '{5}'; this.title = '{5}'; }}",
                parameterComboExpEd.ClientID,
                Convert.ToInt32(BXComponentParameterModificationMode.Standard),
				urlToModificationModeExprImg,
				urlToModificationModeTextImg,
                switchModeImgAlt2Expr,
                switchModeImgAlt2Text));

            parameterComboExpEd.ShowModificationModeSwitchButton = false;
            parameterComboExpEd.DisplayExpressionTags = false;
        
            //dataCell.Controls.Add(switchModeImg);
            switchModeButtonContainer.Controls.Add(switchModeImg);
        }

        if (!parameter.RefreshOnDirty)
            dataCell.Controls.Remove(refreshButtonContainer);
        else
        {
            BXComponentParameterView parameterView = (BXComponentParameterView)parameterControl;
            HtmlImage refreshImg = new HtmlImage();
            string refreshImgID = string.Concat(parameterView.ClientID, ClientIDSeparator, "imgRefreshComponentParameters");
            refreshImg.ID = refreshImgID;
            refreshImg.Attributes.Add("class", "parameter-button refresh");
            
            refreshImg.Src = BXThemeHelper.AddAbsoluteThemePath("images/refresh_blue.gif");

            string refreshImgAlt = GetMessage("IMG_TITLE_REFRESH");

            refreshImg.Alt = refreshImgAlt;
            refreshImg.Attributes.Add("title", refreshImgAlt);

            refreshImg.Attributes.Add("onclick", string.Format("this.src='{0}'; window.setTimeout(function(){{ {1}; }}, 300);", 
                BXThemeHelper.AddAbsoluteThemePath("images/refresh_blue_anim.gif"),
                parameterView.GetPostBackEventReference(BXComponentParameterViewPostBackEventType.RefreshComponentParameters)));
            //dataCell.Controls.Add(refreshImg);
            refreshButtonContainer.Controls.Add(refreshImg);

            parameterView.SetPostBackEventTrigger(
                BXComponentParameterViewPostBackEventType.RefreshComponentParameters,
                string.Format("window.document.getElementById('{0}').onclick();", refreshImg.ClientID));
        }

		ParamClientSideAction action = parameter.ClientSideAction;
		if (action != null)
		{
			action.AdjustControls(new Control[] { row }, this);
			action.RegisterClientScripts(this);
			ParamClientSideActionGroupView groupViewAction = action as ParamClientSideActionGroupView;
			if (groupViewAction != null)
			{
				ScriptManager.RegisterStartupScript(this,
					GetType(),
					"ParamClientSideActionGroupViewVisibilityBinding",
                    string.Format("if(typeof(Bitrix) != 'undefined' && typeof(Bitrix.ParamClientSideActionGroupView) != 'undefined'){{ Bitrix.ParamClientSideActionGroupView.ensureEntryCreated('{0}'); }}", groupViewAction.ViewID), 
					true);

				if (groupViewAction is ParamClientSideActionGroupViewMember)
					row.Attributes.CssStyle[HtmlTextWriterStyle.Display] = "none";
			}

		}
        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter htmlw = new HtmlTextWriter(sw))
                row.RenderControl(htmlw);
            return sw.ToString();
        }
    }


    protected override void Render(HtmlTextWriter writer)
    {
        foreach (string id in _parameterControlIds)
            ClientScript.RegisterForEventValidation(id);
        base.Render(writer);
    }
    /// <summary>
    /// Проверить авторизацию пользователя для работы с диалогом
    /// </summary>
    /// <returns></returns>
    protected override bool CheckUserAuthorization()
    {
        return BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage) && BXSecureIO.CheckView(TargetFileAppRelPath);
    }

    /// <summary>
    /// GetParamsBagName
    /// Имя хранилища параметров
    /// Помещается в сессию
    /// </summary>
    /// <returns></returns>
    protected override string GetParametersBagName()
    {
        return "BitrixDialogEditComponentParametersParamsBag";
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

		if (!IsPostBack)
			return;

		string targetElementID = TargetElementID;
		paramsBag["TargetElementID"] = targetElementID;
		paramsBag["TargetFileAppRelPath"] = TargetFileAppRelPath;

		NameValueCollection postData = Request.Form;
		string keyPrefix = string.Concat(targetElementID, "_");
		foreach (string postDataKey in postData.Keys)
		{
			if (!postDataKey.StartsWith(keyPrefix))
				continue;
			paramsBag[postDataKey] = postData[postDataKey];
		}
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

		string targetElementID = paramsBag.ContainsKey("TargetElementID") ? paramsBag["TargetElementID"] : string.Empty;
		string targetFileAppRelPath = paramsBag.ContainsKey("TargetFileAppRelPath") ? paramsBag["TargetFileAppRelPath"] : string.Empty;
		if (string.IsNullOrEmpty(targetElementID) 
			|| !string.Equals(targetElementID, TargetElementID, StringComparison.InvariantCulture)
			|| string.IsNullOrEmpty(targetFileAppRelPath)
			|| !string.Equals(targetFileAppRelPath, TargetFileAppRelPath, StringComparison.InvariantCulture))
			return;

		foreach (Control control in componentParameters.Controls)
		{
			BXComponentParameterView componentParameterView = control as BXComponentParameterView;
			if (componentParameterView == null)
				continue;
			if (componentParameterView.LoadData(paramsBag))
				componentParameterView.SaveSettings();
		}
    }

    protected void Behaviour_Save(object sender, EventArgs e)
    {
        BXUser.DemandOperations(BXRoleOperation.Operations.FileManage);
		BXSecureIO.DemandWrite(TargetFileAppRelPath);
        try 
        {
			BXUndoPageModificationOperation undoOperation = new BXUndoPageModificationOperation();
			undoOperation.FileVirtualPath = TargetFileAppRelPath;
			undoOperation.FileEncodingName = Encoding.UTF8.WebName;
			undoOperation.FileContent = BXSecureIO.FileReadAllText(TargetFileAppRelPath, Encoding.UTF8);

			BXUndoInfo undo = new BXUndoInfo();
			undo.Operation = undoOperation;
			undo.Save();

            BXPageProxy pageProxy = BXPageProxy.Create(TargetFileAppRelPath);
            BXComponent component = Component;
            BXComponentProxy componentProxy = pageProxy.ResolveControlId(component.ID);
            foreach (KeyValuePair<string, BXParam> kv in component.ParamsDefinition)
            {
                if (componentProxy.Parameters.ContainsKey(kv.Key))
                    componentProxy.Parameters.Remove(kv.Key);
                
				BXParam param = kv.Value;
				componentProxy.Parameters.Add(kv.Key, param);
                
				string val;
				if (component.Parameters.TryGetValue(kv.Key, out val))
                {
                    param.SelectedValue = val;
                    param.IsDirty = true;
                }
            }

            if (pageProxy.IsDirty)
                pageProxy.Save();

			BXDialogGoodbyeWindow goodbye = new BXDialogGoodbyeWindow(string.Format(
				GetMessageRaw("OPERATION_HAS_BEEN_COMPLETED_SUCCESSFULLY_UNDO"), 
				string.Concat(undo.GetClientScript(), " return false;"), 
				"#"), -1, BXDialogGoodbyeWindow.LayoutType.Success);
			BXDialogGoodbyeWindow.SetCurrent(goodbye);

            Refresh(string.Empty, BXDialogGoodbyeWindow.LayoutType.Success);
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

}

