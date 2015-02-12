using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Services;


/// <summary>
/// Параметр компонента "Карта Google Maps с формой поиска"
/// </summary>
public enum MapGoogleSearchComponentParameter
{
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
    DraggableCursor,
    DraggingCursor,
    InitialState
}

/// <summary>
/// Тип карты "Google Maps"
/// </summary>
public enum MapGoogleSearchGoogleMapType
{
    RoadMap = 1,
    Satellite,
    Hybrid,
    Terrain
}

/// <summary>
/// Элемент управления карты "GoogleMaps"
/// </summary>
[FlagsAttribute()]
public enum MapGoogleSearchGoogleMapControlType
{
    LargeMap             = 0x1,
    SmallMap             = 0x2,
    HorBarMapType        = 0x4,
    DropDownMenuMapType  = 0x8,
    Scale                = 0x10
}

/// <summary>
/// Опции карты "GoogleMaps"
/// </summary>
[FlagsAttribute()]
public enum MapGoogleSearchGoogleMapOptions
{
    DisableDefaultUI         = 0x1,
    DisableDoubleClickZoom   = 0x2,
    DisableDragging          = 0x4,
    DisableScrollWheel       = 0x8,
    DisableKeyboardShortcuts = 0x10
}

[FlagsAttribute()]
public enum MapGoogleSearchComponentError
{ 
    None = 0x0
}

/// <summary>
/// Компонент "Карта Google Maps с формой поиска"
/// </summary>
public partial class MapGoogleSearchComponent : BXComponent
{
    public string GetParameterKey(MapGoogleSearchComponentParameter parameter)
    {
        return parameter.ToString("G");
    }

    public string GetParameterTitle(MapGoogleSearchComponentParameter parameter)
    {
        return GetMessageRaw(string.Concat("Param.", parameter.ToString("G")));
    }

    public string GetParameterValueTitle(MapGoogleSearchComponentParameter parameter, string valueKey)
    {
        return GetMessageRaw(string.Concat("Param.", parameter.ToString("G"), ".", valueKey));
    }

    /// <summary>
    /// Исходное состояние
    /// </summary>
    public string InitialState
    {
        get
        {
            string s = Parameters.Get(GetParameterKey(MapGoogleSearchComponentParameter.InitialState), null);
            return !string.IsNullOrEmpty(s) ? s : "{}"; 
        }
        set
        {
            Parameters[GetParameterKey(MapGoogleSearchComponentParameter.InitialState)] = !string.IsNullOrEmpty(value) ? value : "{}";
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
        get { return Parameters.GetInt(GetParameterKey(MapGoogleSearchComponentParameter.MapWidthInPixels), DefaultMapWidthInPixels); }
        set { Parameters[GetParameterKey(MapGoogleSearchComponentParameter.MapWidthInPixels)] = (value > 0 ? value : DefaultMapWidthInPixels).ToString(); }
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
        get { return Parameters.GetInt(GetParameterKey(MapGoogleSearchComponentParameter.MapHeightInPixels), DefaultMapHeightInPixels); }
        set { Parameters[GetParameterKey(MapGoogleSearchComponentParameter.MapHeightInPixels)] = (value > 0 ? value : DefaultMapHeightInPixels).ToString(); }
    }

    /// <summary>
    /// Набор элементов управления картой
    /// </summary>
    public MapGoogleSearchGoogleMapControlType MapControls
    {
        get 
        {
            object o;
            if (ComponentCache.TryGetValue("__MapControls", out o))
                return (MapGoogleSearchGoogleMapControlType)o;

            MapGoogleSearchGoogleMapControlType r = (MapGoogleSearchGoogleMapControlType)0;
            string vals = Parameters.Get(GetParameterKey(MapGoogleSearchComponentParameter.MapControls));
            if (!string.IsNullOrEmpty(vals))
            {
                vals = vals.Replace("'", string.Empty);
                try
                {
                    foreach (string s in vals.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        r |= (MapGoogleSearchGoogleMapControlType)Enum.Parse(typeof(MapGoogleSearchGoogleMapControlType), s, true);
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
            Parameters[GetParameterKey(MapGoogleSearchComponentParameter.MapControls)] = value.ToString("F");
            ComponentCache["__MapControls"] = value;
        }
    }

    /// <summary>
    /// Настройки карты
    /// </summary>
    public MapGoogleSearchGoogleMapOptions MapOptions
    {
        get 
        {
            object o;
            if (ComponentCache.TryGetValue("__MapOptions", out o))
				return (MapGoogleSearchGoogleMapOptions)o;

			MapGoogleSearchGoogleMapOptions r = (MapGoogleSearchGoogleMapOptions)0;
            string vals = Parameters.Get(GetParameterKey(MapGoogleSearchComponentParameter.MapOptions));
            if (!string.IsNullOrEmpty(vals))
            {
                vals = vals.Replace("'", string.Empty);
                try
                {
                    foreach (string s in vals.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        r |= (MapGoogleSearchGoogleMapOptions)Enum.Parse(typeof(MapGoogleSearchGoogleMapOptions), s, true);
                }
                catch
                {
                }
            }
            ComponentCache["__MapOptions"] = r;
            return r;
        }
        set 
        {
            Parameters[GetParameterKey(MapGoogleSearchComponentParameter.MapOptions)] = value.ToString("F");
            ComponentCache["__MapOptions"] = value;
        }
    }

    public string DraggableCursor
    {
        get { return Parameters.Get(GetParameterKey(MapGoogleSearchComponentParameter.DraggableCursor)) ?? string.Empty; }
        set { Parameters[GetParameterKey(MapGoogleSearchComponentParameter.DraggableCursor)] = value ?? string.Empty; }
    }

    public string DraggingCursor
    {
        get { return Parameters.Get(GetParameterKey(MapGoogleSearchComponentParameter.DraggingCursor)) ?? string.Empty; }
        set { Parameters[GetParameterKey(MapGoogleSearchComponentParameter.DraggingCursor)] = value ?? string.Empty; }
    }

	private MapGoogleSearchComponentError errors = MapGoogleSearchComponentError.None;
	public MapGoogleSearchComponentError ComponentErrors
    {
        get { return this.errors; }
    }

    protected override void OnLoad(EventArgs e)
    {
        IncludeComponentTemplate();
        base.OnLoad(e);
    }

    #region BXComponent
    protected override void PreLoadComponentDefinition()
    {
        Title = GetMessageRaw("Title");
        Description = GetMessageRaw("Description");
        Icon = "images/map_search.gif";

        Group = new BXComponentGroup("googleMaps", GetMessageRaw("Group"), 100, BXComponentGroup.Content);
        BXCategory mainCategory = BXCategory.Main;

        ParamsDefinition.Add(
            GetParameterKey(MapGoogleSearchComponentParameter.MapWidthInPixels),
            new BXParamText(
                GetParameterTitle(MapGoogleSearchComponentParameter.MapWidthInPixels),
                "400",
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleSearchComponentParameter.MapHeightInPixels),
            new BXParamText(
                GetParameterTitle(MapGoogleSearchComponentParameter.MapHeightInPixels),
                "300",
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleSearchComponentParameter.MapControls),
            new BXParamMultiSelection(
                GetParameterTitle(MapGoogleSearchComponentParameter.MapControls),
                string.Empty,
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleSearchComponentParameter.MapOptions),
            new BXParamMultiSelection(
                GetParameterTitle(MapGoogleSearchComponentParameter.MapOptions),
                string.Empty,
                mainCategory
            )
        );

        ParamsDefinition.Add(
            GetParameterKey(MapGoogleSearchComponentParameter.DraggableCursor),
            new BXParamText(
                GetParameterTitle(MapGoogleSearchComponentParameter.DraggableCursor),
                string.Empty,
                mainCategory
            )
        );

        ParamsDefinition.Add(
            GetParameterKey(MapGoogleSearchComponentParameter.DraggingCursor),
            new BXParamText(
                GetParameterTitle(MapGoogleSearchComponentParameter.DraggingCursor),
                string.Empty,
                mainCategory
            )
        );

        ParamsDefinition.Add(
            GetParameterKey(MapGoogleSearchComponentParameter.InitialState),
            new BXGoogleMapsInitialState(
                GetParameterTitle(MapGoogleSearchComponentParameter.InitialState),
                string.Empty,
                mainCategory
            )
        );
    }

    protected override void LoadComponentDefinition()
    {
        //MapGoogleSearchComponentParameter.MapControls
        IList<BXParamValue> controlValues = ParamsDefinition[GetParameterKey(MapGoogleSearchComponentParameter.MapControls)].Values;
        controlValues.Clear();
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapControls, MapGoogleSearchGoogleMapControlType.LargeMap.ToString("G")), MapGoogleSearchGoogleMapControlType.LargeMap.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapControls, MapGoogleSearchGoogleMapControlType.SmallMap.ToString("G")), MapGoogleSearchGoogleMapControlType.SmallMap.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapControls, MapGoogleSearchGoogleMapControlType.Scale.ToString("G")), MapGoogleSearchGoogleMapControlType.Scale.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapControls, MapGoogleSearchGoogleMapControlType.HorBarMapType.ToString("G")), MapGoogleSearchGoogleMapControlType.HorBarMapType.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapControls, MapGoogleSearchGoogleMapControlType.DropDownMenuMapType.ToString("G")), MapGoogleSearchGoogleMapControlType.DropDownMenuMapType.ToString("G")));

        //MapGoogleSearchComponentParameter.MapOptions
        IList<BXParamValue> optionValues = ParamsDefinition[GetParameterKey(MapGoogleSearchComponentParameter.MapOptions)].Values;
        optionValues.Clear();
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapOptions, MapGoogleSearchGoogleMapOptions.DisableDefaultUI.ToString("G")), MapGoogleSearchGoogleMapOptions.DisableDefaultUI.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapOptions, MapGoogleSearchGoogleMapOptions.DisableScrollWheel.ToString("G")), MapGoogleSearchGoogleMapOptions.DisableScrollWheel.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapOptions, MapGoogleSearchGoogleMapOptions.DisableDoubleClickZoom.ToString("G")), MapGoogleSearchGoogleMapOptions.DisableDoubleClickZoom.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapOptions, MapGoogleSearchGoogleMapOptions.DisableKeyboardShortcuts.ToString("G")), MapGoogleSearchGoogleMapOptions.DisableKeyboardShortcuts.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleSearchComponentParameter.MapOptions, MapGoogleSearchGoogleMapOptions.DisableDragging.ToString("G")), MapGoogleSearchGoogleMapOptions.DisableDragging.ToString("G")));
    }
    #endregion
}

/// <summary>
/// Базовый класс для шаблонов компонента "MapGoogleSearchComponent"
/// </summary>
public abstract class MapGoogleSearchComponentTemplate : BXComponentTemplate<MapGoogleSearchComponent>
{
}
