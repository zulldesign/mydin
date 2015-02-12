using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Services;


/// <summary>
/// Параметр компонента "Карта Google Maps"
/// </summary>
public enum MapGoogleViewComponentParameter
{
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
    DraggableCursor,
    DraggingCursor,
    InitialState
}

/// <summary>
/// Тип карты "Google Maps"
/// </summary>
public enum MapGoogleViewGoogleMapType
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
public enum MapGoogleViewGoogleMapControlType
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
public enum MapGoogleViewGoogleMapOptions
{
    DisableDefaultUI         = 0x1,
    DisableDoubleClickZoom   = 0x2,
    DisableDragging          = 0x4,
    DisableScrollWheel       = 0x8,
    DisableKeyboardShortcuts = 0x10
}

[FlagsAttribute()]
public enum MapGoogleViewComponentError
{ 
    None = 0x0
    //GoogleMapsApiKeyIsNotFound = 0x1
}

/// <summary>
/// Компонент "Карта Google Maps"
/// </summary>
public partial class MapGoogleViewComponent : BXComponent
{
    public string GetParameterKey(MapGoogleViewComponentParameter parameter)
    {
        return parameter.ToString("G");
    }

    public string GetParameterTitle(MapGoogleViewComponentParameter parameter)
    {
        return GetMessageRaw(string.Concat("Param.", parameter.ToString("G")));
    }

    public string GetParameterValueTitle(MapGoogleViewComponentParameter parameter, string valueKey)
    {
        return GetMessageRaw(string.Concat("Param.", parameter.ToString("G"), ".", valueKey));
    }

    /// <summary>
    /// Ключ "Google Maps"
    /// </summary>
    //public string GoogleMapsApiKey
    //{
    //    get { return Parameters.Get(GetParameterKey(MapGoogleViewComponentParameter.GoogleMapsApiKey)) ?? string.Empty; }
    //    set { Parameters[GetParameterKey(MapGoogleViewComponentParameter.GoogleMapsApiKey)] = value; }
    //}

    /// <summary>
    ///  Начальный тип карты
    /// </summary>
    //public GoogleMapType InitialMapType
    //{
    //    get 
    //    {
    //        string v = Parameters.Get(GetParameterKey(MapGoogleViewComponentParameter.InitialMapType));
    //        if (string.IsNullOrEmpty(v))
    //            return GoogleMapType.RoadMap;

    //        try
    //        {
    //           return (GoogleMapType)Enum.Parse(typeof(GoogleMapType), v, true);
    //        }
    //        catch
    //        {
    //        }
    //        return GoogleMapType.RoadMap;
    //    }
    //    set 
    //    {
    //        Parameters[GetParameterKey(MapGoogleViewComponentParameter.InitialMapType)] = value.ToString("G");
    //    }
    //}

    /// <summary>
    /// Исходное состояние
    /// </summary>
    public string InitialState
    {
        get
        {
            string s = Parameters.Get(GetParameterKey(MapGoogleViewComponentParameter.InitialState), null);
            return !string.IsNullOrEmpty(s) ? s : "{}"; 
        }
        set
        {
            Parameters[GetParameterKey(MapGoogleViewComponentParameter.InitialState)] = !string.IsNullOrEmpty(value) ? value : "{}";
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
        get { return Parameters.GetInt(GetParameterKey(MapGoogleViewComponentParameter.MapWidthInPixels), DefaultMapWidthInPixels); }
        set { Parameters[GetParameterKey(MapGoogleViewComponentParameter.MapWidthInPixels)] = (value > 0 ? value : DefaultMapWidthInPixels).ToString(); }
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
        get { return Parameters.GetInt(GetParameterKey(MapGoogleViewComponentParameter.MapHeightInPixels), DefaultMapHeightInPixels); }
        set { Parameters[GetParameterKey(MapGoogleViewComponentParameter.MapHeightInPixels)] = (value > 0 ? value : DefaultMapHeightInPixels).ToString(); }
    }

    /// <summary>
    /// Набор элементов управления картой
    /// </summary>
    public MapGoogleViewGoogleMapControlType MapControls
    {
        get 
        {
            object o;
            if (ComponentCache.TryGetValue("__MapControls", out o))
				return (MapGoogleViewGoogleMapControlType)o;

			MapGoogleViewGoogleMapControlType r = (MapGoogleViewGoogleMapControlType)0;
            string vals = Parameters.Get(GetParameterKey(MapGoogleViewComponentParameter.MapControls));
            if (!string.IsNullOrEmpty(vals))
            {
                vals = vals.Replace("'", string.Empty);
                try
                {
                    foreach (string s in vals.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        r |= (MapGoogleViewGoogleMapControlType)Enum.Parse(typeof(MapGoogleViewGoogleMapControlType), s, true);
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
            Parameters[GetParameterKey(MapGoogleViewComponentParameter.MapControls)] = value.ToString("F");
            ComponentCache["__MapControls"] = value;
        }
    }

    /// <summary>
    /// Настройки карты
    /// </summary>
	public MapGoogleViewGoogleMapOptions MapOptions
    {
        get 
        {
            object o;
            if (ComponentCache.TryGetValue("__MapOptions", out o))
				return (MapGoogleViewGoogleMapOptions)o;

			MapGoogleViewGoogleMapOptions r = (MapGoogleViewGoogleMapOptions)0;
            string vals = Parameters.Get(GetParameterKey(MapGoogleViewComponentParameter.MapOptions));
            if (!string.IsNullOrEmpty(vals))
            {
                vals = vals.Replace("'", string.Empty);
                try
                {
                    foreach (string s in vals.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        r |= (MapGoogleViewGoogleMapOptions)Enum.Parse(typeof(MapGoogleViewGoogleMapOptions), s, true);
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
            Parameters[GetParameterKey(MapGoogleViewComponentParameter.MapOptions)] = value.ToString("F");
            ComponentCache["__MapOptions"] = value;
        }
    }

    /*
    /// <summary>
    /// Ид пользовательской карты Google Maps
    /// </summary>
    public string MapId
    {
        get { return Parameters.Get(GetParameterKey(MapGoogleViewComponentParameter.MapId)) ?? string.Empty; }
        set { Parameters[GetParameterKey(MapGoogleViewComponentParameter.MapId)] = value; }
    }
    */

    public string DraggableCursor
    {
        get { return Parameters.Get(GetParameterKey(MapGoogleViewComponentParameter.DraggableCursor)) ?? string.Empty; }
        set { Parameters[GetParameterKey(MapGoogleViewComponentParameter.DraggableCursor)] = value ?? string.Empty; }
    }

    public string DraggingCursor
    {
        get { return Parameters.Get(GetParameterKey(MapGoogleViewComponentParameter.DraggingCursor)) ?? string.Empty; }
        set { Parameters[GetParameterKey(MapGoogleViewComponentParameter.DraggingCursor)] = value ?? string.Empty; }
    }

    private MapGoogleViewComponentError errors = MapGoogleViewComponentError.None;
    public MapGoogleViewComponentError ComponentErrors
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
        Icon = "images/map_view.gif";

        Group = new BXComponentGroup("googleMaps", GetMessageRaw("Group"), 100, BXComponentGroup.Content);
        BXCategory mainCategory = BXCategory.Main;

        /*
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.GoogleMapsApiKey),
            new BXParamText(
                GetParameterTitle(MapGoogleViewComponentParameter.GoogleMapsApiKey),
                string.Empty,
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.InitialMapType),
            new BXParamSingleSelection(
                GetParameterTitle(MapGoogleViewComponentParameter.InitialMapType),
                GoogleMapType.RoadMap.ToString("G"),
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.MapData),
            new BXParamText(
                GetParameterTitle(MapGoogleViewComponentParameter.MapData),
                string.Empty,
                mainCategory
            )
        );
        */
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.MapWidthInPixels),
            new BXParamText(
                GetParameterTitle(MapGoogleViewComponentParameter.MapWidthInPixels),
                "400",
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.MapHeightInPixels),
            new BXParamText(
                GetParameterTitle(MapGoogleViewComponentParameter.MapHeightInPixels),
                "300",
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.MapControls),
            new BXParamMultiSelection(
                GetParameterTitle(MapGoogleViewComponentParameter.MapControls),
                string.Empty,
                mainCategory
            )
        );
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.MapOptions),
            new BXParamMultiSelection(
                GetParameterTitle(MapGoogleViewComponentParameter.MapOptions),
                string.Empty,
                mainCategory
            )
        );

        /*
        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.MapId),
            new BXParamMultiSelection(
                GetParameterTitle(MapGoogleViewComponentParameter.MapId),
                string.Empty,
                mainCategory
            )
        );
        */

        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.DraggableCursor),
            new BXParamText(
                GetParameterTitle(MapGoogleViewComponentParameter.DraggableCursor),
                string.Empty,
                mainCategory
            )
        );

        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.DraggingCursor),
            new BXParamText(
                GetParameterTitle(MapGoogleViewComponentParameter.DraggingCursor),
                string.Empty,
                mainCategory
            )
        );

        ParamsDefinition.Add(
            GetParameterKey(MapGoogleViewComponentParameter.InitialState),
            new BXGoogleMapsInitialState(
                GetParameterTitle(MapGoogleViewComponentParameter.InitialState),
                string.Empty,
                mainCategory
            )
        );
    }

    protected override void LoadComponentDefinition()
    {
        //MapGoogleViewComponentParameter.InitialMapType
        //IList<BXParamValue> mapTypeValues = ParamsDefinition[GetParameterKey(MapGoogleViewComponentParameter.InitialMapType)].Values;
        //mapTypeValues.Clear();
        //mapTypeValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.InitialMapType, GoogleMapType.RoadMap.ToString("G")), GoogleMapType.RoadMap.ToString("G")));
        //mapTypeValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.InitialMapType, GoogleMapType.Satellite.ToString("G")), GoogleMapType.Satellite.ToString("G")));
        //mapTypeValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.InitialMapType, GoogleMapType.Hybrid.ToString("G")), GoogleMapType.Hybrid.ToString("G")));
        //mapTypeValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.InitialMapType, GoogleMapType.Terrain.ToString("G")), GoogleMapType.Terrain.ToString("G")));

        //MapGoogleViewComponentParameter.MapControls
        IList<BXParamValue> controlValues = ParamsDefinition[GetParameterKey(MapGoogleViewComponentParameter.MapControls)].Values;
        controlValues.Clear();
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapControls, MapGoogleViewGoogleMapControlType.LargeMap.ToString("G")), MapGoogleViewGoogleMapControlType.LargeMap.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapControls, MapGoogleViewGoogleMapControlType.SmallMap.ToString("G")), MapGoogleViewGoogleMapControlType.SmallMap.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapControls, MapGoogleViewGoogleMapControlType.Scale.ToString("G")), MapGoogleViewGoogleMapControlType.Scale.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapControls, MapGoogleViewGoogleMapControlType.HorBarMapType.ToString("G")), MapGoogleViewGoogleMapControlType.HorBarMapType.ToString("G")));
		controlValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapControls, MapGoogleViewGoogleMapControlType.DropDownMenuMapType.ToString("G")), MapGoogleViewGoogleMapControlType.DropDownMenuMapType.ToString("G")));

        //MapGoogleViewComponentParameter.MapOptions
        IList<BXParamValue> optionValues = ParamsDefinition[GetParameterKey(MapGoogleViewComponentParameter.MapOptions)].Values;
        optionValues.Clear();
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapOptions, MapGoogleViewGoogleMapOptions.DisableDefaultUI.ToString("G")), MapGoogleViewGoogleMapOptions.DisableDefaultUI.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapOptions, MapGoogleViewGoogleMapOptions.DisableScrollWheel.ToString("G")), MapGoogleViewGoogleMapOptions.DisableScrollWheel.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapOptions, MapGoogleViewGoogleMapOptions.DisableDoubleClickZoom.ToString("G")), MapGoogleViewGoogleMapOptions.DisableDoubleClickZoom.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapOptions, MapGoogleViewGoogleMapOptions.DisableKeyboardShortcuts.ToString("G")), MapGoogleViewGoogleMapOptions.DisableKeyboardShortcuts.ToString("G")));
		optionValues.Add(new BXParamValue(GetParameterValueTitle(MapGoogleViewComponentParameter.MapOptions, MapGoogleViewGoogleMapOptions.DisableDragging.ToString("G")), MapGoogleViewGoogleMapOptions.DisableDragging.ToString("G")));
    }
    #endregion
}

/// <summary>
/// Базовый класс для шаблонов компонента "MapGoogleViewComponent"
/// </summary>
public abstract class MapGoogleViewComponentTemplate : BXComponentTemplate<MapGoogleViewComponent>
{
    /*
    protected override void Render(HtmlTextWriter writer)
    {
        if (IsComponentDesignMode
            && (Component.ComponentErrors & MapGoogleViewComponentError.GoogleMapsApiKeyIsNotFound) != 0)
        {
            writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
            return;
        }
        base.Render(writer);
    }
    */
}
