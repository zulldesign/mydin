using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Services;


/// <summary>
/// Параметр компонента "Поиск на карте Яндекс.Карты"
/// </summary>
public enum MapYandexSearchComponentParameter
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
public enum MapYandexSearchYandexMapType
{
    Map = 1,
    Satellite,
    Hybrid
}

/// <summary>
/// Элемент управления "Поиск на карте Яндекс.Карты"
/// </summary>
[FlagsAttribute()]
public enum MapYandexSearchYandexMapControlType
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
public enum MapYandexSearchYandexMapOptions
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
public enum MapYandexSearchComponentError
{ 
    None = 0x0,
    YandexMapsApiKeyIsNotFound = 0x1
}

/// <summary>
/// Компонент "Поиск на карте Яндекс.Карты"
/// </summary>
public partial class MapYandexSearchComponent : BXComponent
{
    public string GetParameterKey(MapYandexSearchComponentParameter parameter)
    {
        return parameter.ToString("G");
    }

    public string GetParameterTitle(MapYandexSearchComponentParameter parameter)
    {
        return GetMessageRaw(string.Concat("Param.", parameter.ToString("G")));
    }

    public string GetParameterValueTitle(MapYandexSearchComponentParameter parameter, string valueKey)
    {
        return GetMessageRaw(string.Concat("Param.", parameter.ToString("G"), ".", valueKey));
    }

    /// <summary>
    /// Ключ API "Яндекс.Карты"
    /// </summary>
    public string YandexMapsApiKey
    {
        get { return Parameters.Get(GetParameterKey(MapYandexSearchComponentParameter.YandexMapsApiKey)) ?? string.Empty; }
        set { Parameters[GetParameterKey(MapYandexSearchComponentParameter.YandexMapsApiKey)] = value; }
    }

    /// <summary>
    /// Исходное состояние
    /// </summary>
    public string InitialState
    {
        get
        {
            string s = Parameters.Get(GetParameterKey(MapYandexSearchComponentParameter.InitialState), null);
            return !string.IsNullOrEmpty(s) ? s : "{}"; 
        }
        set
        {
            Parameters[GetParameterKey(MapYandexSearchComponentParameter.InitialState)] = !string.IsNullOrEmpty(value) ? value : "{}";
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
        get { return Parameters.GetInt(GetParameterKey(MapYandexSearchComponentParameter.MapWidthInPixels), DefaultMapWidthInPixels); }
        set { Parameters[GetParameterKey(MapYandexSearchComponentParameter.MapWidthInPixels)] = (value > 0 ? value : DefaultMapWidthInPixels).ToString(); }
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
        get { return Parameters.GetInt(GetParameterKey(MapYandexSearchComponentParameter.MapHeightInPixels), DefaultMapHeightInPixels); }
        set { Parameters[GetParameterKey(MapYandexSearchComponentParameter.MapHeightInPixels)] = (value > 0 ? value : DefaultMapHeightInPixels).ToString(); }
    }

    /// <summary>
    /// Набор элементов управления картой
    /// </summary>
    public MapYandexSearchYandexMapControlType MapControls
    {
        get 
        {
            object o;
            if (ComponentCache.TryGetValue("__MapControls", out o))
				return (MapYandexSearchYandexMapControlType)o;

			MapYandexSearchYandexMapControlType r = (MapYandexSearchYandexMapControlType)0;
            string vals = Parameters.Get(GetParameterKey(MapYandexSearchComponentParameter.MapControls));
            if (!string.IsNullOrEmpty(vals))
            {
                vals = vals.Replace("'", string.Empty);
                try
                {
                    foreach (string s in vals.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        r |= (MapYandexSearchYandexMapControlType)Enum.Parse(typeof(MapYandexSearchYandexMapControlType), s, true);
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
            Parameters[GetParameterKey(MapYandexSearchComponentParameter.MapControls)] = value.ToString("F");
            ComponentCache["__MapControls"] = value;
        }
    }

    /// <summary>
    /// Настройки карты
    /// </summary>
	public MapYandexSearchYandexMapOptions MapOptions
    {
        get 
        {
            object o;
            if (ComponentCache.TryGetValue("__MapOptions", out o))
				return (MapYandexSearchYandexMapOptions)o;

			MapYandexSearchYandexMapOptions r = (MapYandexSearchYandexMapOptions)0;

            IList<string> vals = Parameters.GetListString(GetParameterKey(MapYandexSearchComponentParameter.MapOptions));
            if (vals != null && vals.Count > 0)
                try
                {
                    for (int i = 0; i < vals.Count; i++)
						r |= (MapYandexSearchYandexMapOptions)Enum.Parse(typeof(MapYandexSearchYandexMapOptions), vals[i], true);
                }
                catch 
                {
                }
            ComponentCache["__MapOptions"] = r;
            return r;
        }
        set 
        { 
            Parameters[GetParameterKey(MapYandexSearchComponentParameter.MapOptions)] = value.ToString("F");
            ComponentCache["__MapOptions"] = value;
        }
    }

    private MapYandexSearchComponentError errors = MapYandexSearchComponentError.None;
    public MapYandexSearchComponentError ComponentErrors
    {
        get { return this.errors; }
    }

    protected override void OnLoad(EventArgs e)
    {
        if (string.IsNullOrEmpty(YandexMapsApiKey))
            this.errors = MapYandexSearchComponentError.YandexMapsApiKeyIsNotFound;

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
            GetParameterKey(MapYandexSearchComponentParameter.YandexMapsApiKey),
            new BXParamText(
                GetParameterTitle(MapYandexSearchComponentParameter.YandexMapsApiKey),
                string.Empty,
                mainCategory
            )
        );

        ParamsDefinition.Add(
            GetParameterKey(MapYandexSearchComponentParameter.MapWidthInPixels),
            new BXParamText(
                GetParameterTitle(MapYandexSearchComponentParameter.MapWidthInPixels),
                "400",
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapYandexSearchComponentParameter.MapHeightInPixels),
            new BXParamText(
                GetParameterTitle(MapYandexSearchComponentParameter.MapHeightInPixels),
                "300",
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapYandexSearchComponentParameter.MapControls),
            new BXParamMultiSelection(
                GetParameterTitle(MapYandexSearchComponentParameter.MapControls),
                string.Empty,
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapYandexSearchComponentParameter.MapOptions),
            new BXParamMultiSelection(
                GetParameterTitle(MapYandexSearchComponentParameter.MapOptions),
                string.Empty,
                mainCategory
            )
        );

        ParamsDefinition.Add(
            GetParameterKey(MapYandexSearchComponentParameter.InitialState),
            new BXYandexMapsInitialState(
                GetParameterTitle(MapYandexSearchComponentParameter.InitialState),
                string.Empty,
                mainCategory
            )
        );
    }

    protected override void LoadComponentDefinition()
    {
        //MapYandexSearchComponentParameter.MapControls
        IList<BXParamValue> controlValues = ParamsDefinition[GetParameterKey(MapYandexSearchComponentParameter.MapControls)].Values;
        controlValues.Clear();
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapControls, MapYandexSearchYandexMapControlType.ToolBar.ToString("G")), MapYandexSearchYandexMapControlType.ToolBar.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapControls, MapYandexSearchYandexMapControlType.ZoomStandard.ToString("G")), MapYandexSearchYandexMapControlType.ZoomStandard.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapControls, MapYandexSearchYandexMapControlType.ZoomCompact.ToString("G")), MapYandexSearchYandexMapControlType.ZoomCompact.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapControls, MapYandexSearchYandexMapControlType.MiniMap.ToString("G")), MapYandexSearchYandexMapControlType.MiniMap.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapControls, MapYandexSearchYandexMapControlType.TypeControl.ToString("G")), MapYandexSearchYandexMapControlType.TypeControl.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapControls, MapYandexSearchYandexMapControlType.ScaleLine.ToString("G")), MapYandexSearchYandexMapControlType.ScaleLine.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapControls, MapYandexSearchYandexMapControlType.Search.ToString("G")), MapYandexSearchYandexMapControlType.Search.ToString("G")));

        //MapGoogleViewComponentParameter.MapOptions
        IList<BXParamValue> optionValues = ParamsDefinition[GetParameterKey(MapYandexSearchComponentParameter.MapOptions)].Values;
        optionValues.Clear();
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapOptions, MapYandexSearchYandexMapOptions.DisableDragging.ToString("G")), MapYandexSearchYandexMapOptions.DisableDragging.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapOptions, MapYandexSearchYandexMapOptions.DisableDoubleClickZoom.ToString("G")), MapYandexSearchYandexMapOptions.DisableDoubleClickZoom.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapOptions, MapYandexSearchYandexMapOptions.EnableScrollZoom.ToString("G")), MapYandexSearchYandexMapOptions.EnableScrollZoom.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapOptions, MapYandexSearchYandexMapOptions.EnableHotKeys.ToString("G")), MapYandexSearchYandexMapOptions.EnableHotKeys.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapOptions, MapYandexSearchYandexMapOptions.EnableMagnifier.ToString("G")), MapYandexSearchYandexMapOptions.EnableMagnifier.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapOptions, MapYandexSearchYandexMapOptions.EnableRightButtonMagnifier.ToString("G")), MapYandexSearchYandexMapOptions.EnableRightButtonMagnifier.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapYandexSearchComponentParameter.MapOptions, MapYandexSearchYandexMapOptions.EnableRuler.ToString("G")), MapYandexSearchYandexMapOptions.EnableRuler.ToString("G")));
    }
    #endregion
}

/// <summary>
/// Базовый класс для шаблонов компонента "MapYandexViewComponent"
/// </summary>
public abstract class MapYandexSearchComponentTemplate : BXComponentTemplate<MapYandexSearchComponent>
{
    protected override void Render(HtmlTextWriter writer)
    {
        if (IsComponentDesignMode
            && (Component.ComponentErrors & MapYandexSearchComponentError.YandexMapsApiKeyIsNotFound) != 0)
        {
            writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
            return;
        }
        base.Render(writer);
    }
}
