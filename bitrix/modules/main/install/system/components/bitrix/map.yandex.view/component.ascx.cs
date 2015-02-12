using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Services;


/// <summary>
/// Параметр компонента "Карта Яндекс.Карты"
/// </summary>
public enum MapYandexViewComponentParameter
{
    /// <summary>
    /// Ключ API
    /// </summary>
    YandexMapsApiKey,
    /// <summary>
    /// Ширина карты в пикселях
    /// </summary>
    MapWidthInPixels,
    /// <summary>
    /// Высота карты в пикселях
    /// </summary>
    MapHeightInPixels,
    /// <summary>
    /// Эл-ты управления картой
    /// </summary>
    MapControls,
    /// <summary>
    /// Настройки карты
    /// </summary>
    MapOptions,
    /// <summary>
    /// Начальное состояние
    /// </summary>
    InitialState
}

/// <summary>
/// Тип карты "Яндекс.Карты"
/// </summary>
public enum MapYandexViewYandexMapType
{
    Map = 1,
    Satellite,
    Hybrid
}

/// <summary>
/// Элемент управления карты "Яндекс.Карты"
/// </summary>
[FlagsAttribute()]
public enum MapYandexViewYandexMapControlType
{
    /// <summary>
    /// Панель инструментов
    /// (в стандартном варианте включает Dragger, Magnifier, Ruller)
    /// </summary>
    ToolBar         = 0x1,
    /// <summary>
    /// Управление масштабированием
    /// </summary>
    ZoomStandard    = 0x2,
    /// <summary>
    /// Компактное управление масштабирования
    /// </summary>
    ZoomCompact     = 0x4,
    /// <summary>
    ///  Обзорная карта
    /// </summary>
    MiniMap         = 0x8,
    /// <summary>
    /// Переключатель типа карты
    /// </summary>
    TypeControl     = 0x10,
    /// <summary>
    /// Масштабная линейка
    /// </summary>
    ScaleLine       = 0x20,
    /// <summary>
    /// Поиск по карте
    /// </summary>
    Search          = 0x40
}

/// <summary>
/// Опции карты "Яндекс.Карты"
/// </summary>
[FlagsAttribute()]
public enum MapYandexViewYandexMapOptions
{
    DisableDragging             = 0x1,
    DisableDoubleClickZoom      = 0x2,
    EnableScrollZoom            = 0x4,
    EnableHotKeys               = 0x8,
    EnableMagnifier             = 0x10,
    EnableRightButtonMagnifier  = 0x20,
    EnableRuler                 = 0x40 
}

[FlagsAttribute()]
public enum MapYandexViewComponentError
{ 
    None = 0x0,
    YandexMapsApiKeyIsNotFound = 0x1
}

/// <summary>
/// Компонент "Карта Яндекс.Карты"
/// </summary>
public partial class MapYandexViewComponent : BXComponent
{
    public string GetParameterKey(MapYandexViewComponentParameter parameter)
    {
        return parameter.ToString("G");
    }

    public string GetParameterTitle(MapYandexViewComponentParameter parameter)
    {
        return GetMessageRaw(string.Concat("Param.", parameter.ToString("G")));
    }

    public string GetParameterValueTitle(MapYandexViewComponentParameter parameter, string valueKey)
    {
        return GetMessageRaw(string.Concat("Param.", parameter.ToString("G"), ".", valueKey));
    }

    /// <summary>
    /// Ключ API "Яндекс.Карты"
    /// </summary>
    public string YandexMapsApiKey
    {
        get { return Parameters.Get(GetParameterKey(MapYandexViewComponentParameter.YandexMapsApiKey)) ?? string.Empty; }
        set { Parameters[GetParameterKey(MapYandexViewComponentParameter.YandexMapsApiKey)] = value; }
    }

    /// <summary>
    /// Исходное состояние
    /// </summary>
    public string InitialState
    {
        get
        {
            string s = Parameters.Get(GetParameterKey(MapYandexViewComponentParameter.InitialState), null);
            return !string.IsNullOrEmpty(s) ? s : "{}"; 
        }
        set
        {
            Parameters[GetParameterKey(MapYandexViewComponentParameter.InitialState)] = !string.IsNullOrEmpty(value) ? value : "{}";
        }
    }
    /// <summary>
    /// Ширина карты в пикселях по умолчанию
    /// </summary>
    public static int DefaultMapWidthInPixels
    {
        get { return 400; }
    }

    /// <summary>
    /// Ширина карты в пикселях
    /// </summary>
    public int MapWidthInPixels
    {
        get { return Parameters.GetInt(GetParameterKey(MapYandexViewComponentParameter.MapWidthInPixels), DefaultMapWidthInPixels); }
        set { Parameters[GetParameterKey(MapYandexViewComponentParameter.MapWidthInPixels)] = (value > 0 ? value : DefaultMapWidthInPixels).ToString(); }
    }

    /// <summary>
    /// Высота карты в пикселях по умолчанию
    /// </summary>
    public static int DefaultMapHeightInPixels
    {
        get { return 300; }
    }

    /// <summary>
    /// Высота карты в пикселях
    /// </summary>
    public int MapHeightInPixels
    {
        get { return Parameters.GetInt(GetParameterKey(MapYandexViewComponentParameter.MapHeightInPixels), DefaultMapHeightInPixels); }
        set { Parameters[GetParameterKey(MapYandexViewComponentParameter.MapHeightInPixels)] = (value > 0 ? value : DefaultMapHeightInPixels).ToString(); }
    }

    /// <summary>
    /// Набор элементов управления картой
    /// </summary>
	public MapYandexViewYandexMapControlType MapControls
    {
        get 
        {
            object o;
            if (ComponentCache.TryGetValue("__MapControls", out o))
				return (MapYandexViewYandexMapControlType)o;

			MapYandexViewYandexMapControlType r = (MapYandexViewYandexMapControlType)0;
            string vals = Parameters.Get(GetParameterKey(MapYandexViewComponentParameter.MapControls));
            if (!string.IsNullOrEmpty(vals))
            {
                vals = vals.Replace("'", string.Empty);
                try
                {
                    foreach (string s in vals.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        r |= (MapYandexViewYandexMapControlType)Enum.Parse(typeof(MapYandexViewYandexMapControlType), s, true);
                }
                catch
                {
                }
            }
            ComponentCache["__MapControls"] = r;
            return r;
        }
        set 
        { 
            Parameters[GetParameterKey(MapYandexViewComponentParameter.MapControls)] = value.ToString("F");
            ComponentCache["__MapControls"] = value;
        }
    }

    /// <summary>
    /// Настройки карты
    /// </summary>
	public MapYandexViewYandexMapOptions MapOptions
    {
        get 
        {
            object o;
            if (ComponentCache.TryGetValue("__MapOptions", out o))
				return (MapYandexViewYandexMapOptions)o;

			MapYandexViewYandexMapOptions r = (MapYandexViewYandexMapOptions)0;

            IList<string> vals = Parameters.GetListString(GetParameterKey(MapYandexViewComponentParameter.MapOptions));
            if (vals != null && vals.Count > 0)
                try
                {
                    for (int i = 0; i < vals.Count; i++)
						r |= (MapYandexViewYandexMapOptions)Enum.Parse(typeof(MapYandexViewYandexMapOptions), vals[i], true);
                }
                catch 
                {
                }
            ComponentCache["__MapOptions"] = r;
            return r;
        }
        set 
        { 
            Parameters[GetParameterKey(MapYandexViewComponentParameter.MapOptions)] = value.ToString("F");
            ComponentCache["__MapOptions"] = value;
        }
    }

    private MapYandexViewComponentError errors = MapYandexViewComponentError.None;
    public MapYandexViewComponentError ComponentErrors
    {
        get { return this.errors; }
    }

    protected override void OnLoad(EventArgs e)
    {
        if (string.IsNullOrEmpty(YandexMapsApiKey))
            this.errors = MapYandexViewComponentError.YandexMapsApiKeyIsNotFound;

        IncludeComponentTemplate();
        base.OnLoad(e);
    }

    #region BXComponent
    protected override void PreLoadComponentDefinition()
    {
        Title = GetMessageRaw("Title");
        Description = GetMessageRaw("Description");
        Icon = "images/map_view.gif";

        Group = new BXComponentGroup("yandexMaps", GetMessageRaw("Group"), 100, BXComponentGroup.Content);
        BXCategory mainCategory = BXCategory.Main;

        ParamsDefinition.Add(
            GetParameterKey(MapYandexViewComponentParameter.YandexMapsApiKey),
            new BXParamText(
                GetParameterTitle(MapYandexViewComponentParameter.YandexMapsApiKey),
                string.Empty,
                mainCategory
            )
        );

        ParamsDefinition.Add(
            GetParameterKey(MapYandexViewComponentParameter.MapWidthInPixels),
            new BXParamText(
                GetParameterTitle(MapYandexViewComponentParameter.MapWidthInPixels),
                "400",
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapYandexViewComponentParameter.MapHeightInPixels),
            new BXParamText(
                GetParameterTitle(MapYandexViewComponentParameter.MapHeightInPixels),
                "300",
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapYandexViewComponentParameter.MapControls),
            new BXParamMultiSelection(
                GetParameterTitle(MapYandexViewComponentParameter.MapControls),
                string.Empty,
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapYandexViewComponentParameter.MapOptions),
            new BXParamMultiSelection(
                GetParameterTitle(MapYandexViewComponentParameter.MapOptions),
                string.Empty,
                mainCategory
            )
        );

        ParamsDefinition.Add(
            GetParameterKey(MapYandexViewComponentParameter.InitialState),
            new BXYandexMapsInitialState(
                GetParameterTitle(MapYandexViewComponentParameter.InitialState),
                string.Empty,
                mainCategory
            )
        );
    }

    protected override void LoadComponentDefinition()
    {
        //MapYandexViewComponentParameter.MapControls
        IList<BXParamValue> controlValues = ParamsDefinition[GetParameterKey(MapYandexViewComponentParameter.MapControls)].Values;
        controlValues.Clear();
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapControls, MapYandexViewYandexMapControlType.ToolBar.ToString("G")), MapYandexViewYandexMapControlType.ToolBar.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapControls, MapYandexViewYandexMapControlType.ZoomStandard.ToString("G")), MapYandexViewYandexMapControlType.ZoomStandard.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapControls, MapYandexViewYandexMapControlType.ZoomCompact.ToString("G")), MapYandexViewYandexMapControlType.ZoomCompact.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapControls, MapYandexViewYandexMapControlType.MiniMap.ToString("G")), MapYandexViewYandexMapControlType.MiniMap.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapControls, MapYandexViewYandexMapControlType.TypeControl.ToString("G")), MapYandexViewYandexMapControlType.TypeControl.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapControls, MapYandexViewYandexMapControlType.ScaleLine.ToString("G")), MapYandexViewYandexMapControlType.ScaleLine.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapControls, MapYandexViewYandexMapControlType.Search.ToString("G")), MapYandexViewYandexMapControlType.Search.ToString("G")));

        //MapGoogleViewComponentParameter.MapOptions
        IList<BXParamValue> optionValues = ParamsDefinition[GetParameterKey(MapYandexViewComponentParameter.MapOptions)].Values;
        optionValues.Clear();
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapOptions, MapYandexViewYandexMapOptions.DisableDragging.ToString("G")), MapYandexViewYandexMapOptions.DisableDragging.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapOptions, MapYandexViewYandexMapOptions.DisableDoubleClickZoom.ToString("G")), MapYandexViewYandexMapOptions.DisableDoubleClickZoom.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapOptions, MapYandexViewYandexMapOptions.EnableScrollZoom.ToString("G")), MapYandexViewYandexMapOptions.EnableScrollZoom.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapOptions, MapYandexViewYandexMapOptions.EnableHotKeys.ToString("G")), MapYandexViewYandexMapOptions.EnableHotKeys.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapOptions, MapYandexViewYandexMapOptions.EnableMagnifier.ToString("G")), MapYandexViewYandexMapOptions.EnableMagnifier.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapOptions, MapYandexViewYandexMapOptions.EnableRightButtonMagnifier.ToString("G")), MapYandexViewYandexMapOptions.EnableRightButtonMagnifier.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexViewComponentParameter.MapOptions, MapYandexViewYandexMapOptions.EnableRuler.ToString("G")), MapYandexViewYandexMapOptions.EnableRuler.ToString("G")));
    }
    #endregion
}

/// <summary>
/// Базовый класс для шаблонов компонента "MapYandexViewComponent"
/// </summary>
public abstract class MapYandexViewComponentTemplate : BXComponentTemplate<MapYandexViewComponent>
{
    protected override void Render(HtmlTextWriter writer)
    {
        if (IsComponentDesignMode
            && (Component.ComponentErrors & MapYandexViewComponentError.YandexMapsApiKeyIsNotFound) != 0)
        {
            writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
            return;
        }
        base.Render(writer);
    }
}
