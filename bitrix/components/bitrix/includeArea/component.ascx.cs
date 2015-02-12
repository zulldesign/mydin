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
using Bitrix.UI;
using Bitrix.Components;
using System.Collections.Generic;
using System.Text;
using Bitrix.Components.Editor;
using Bitrix.Configuration;
using System.IO;
using Bitrix.Security;
using Bitrix.IO;
using Bitrix.Services;
using Bitrix.Services.Js;
using Bitrix.UI.Hermitage;

/// <summary>
/// Параметр компонента "Включаемая область"
/// </summary>
public enum IncludeAreaComponentParameter
{
	/// <summary>
	/// Режим (из IncludeAreaComponentMode)
	/// </summary>
	Mode = 1,
	/// <summary>
	///Путь к файлу (работает в режиме IncludeAreaComponentMode.File)
	/// </summary>
	FilePath,
	/// <summary>
	///Кодировка файла(работает в режиме IncludeAreaComponentMode.File)
	/// </summary>
	FileEncoding,
	/// <summary>
	/// Имя файла (работает в режиме IncludeAreaComponentMode.Section)
	/// </summary>
	FileName,
	/// <summary>
	/// Суффикс для имени файла (работает в режиме IncludeAreaComponentMode.Page)
	/// </summary>
	FileNameSuffix,
	/// <summary>
	/// Рекурсивный поиск подключаемого файла (работает в режиме IncludeAreaComponentMode.Section - если файл не найден в "текущем" разделе, то поиск продолжается в родительском и т.д. пока файл с нужным именем не будет найден или не будет достигнут корень сайта)
	/// </summary>
	RecursiveFileSearch,
	/// <summary>
	/// Разрешать редактирование подключаемого файла через публичный интерфейс
	/// </summary>
	AllowEditing
}

/// <summary>
/// Режим компонента "Включаемая область"
/// </summary>
public enum IncludeAreaComponentMode
{
	//для сраницы
	Page = 1,
	//для раздела
	Section,
	//из файла
	File
}


/// <summary>
/// Компонент "Включаемая область"
/// </summary>
public partial class IncludeAreaComponent : BXComponent
{
	/// <summary>
	/// Режим
	/// </summary>
	public IncludeAreaComponentMode Mode
	{
		get
		{
			string modeStr = Parameters.Get(GetParameterKey(IncludeAreaComponentParameter.Mode), string.Empty);
			return !string.IsNullOrEmpty(modeStr) ? (IncludeAreaComponentMode)Enum.Parse(typeof(IncludeAreaComponentMode), modeStr, true) : IncludeAreaComponentMode.Page;
		}
		set
		{
			Parameters[GetParameterKey(IncludeAreaComponentParameter.Mode)] = Enum.GetName(typeof(IncludeAreaComponentMode), value);
		}
	}
	/// <summary>
	/// Путь к файлу (работает в режиме IncludeAreaComponentMode.File)
	/// </summary>
	public string FilePath
	{
		get
		{
			return Parameters.Get(GetParameterKey(IncludeAreaComponentParameter.FilePath), string.Empty);
		}
		set
		{
			Parameters[GetParameterKey(IncludeAreaComponentParameter.FilePath)] = value;
		}
	}

	/// <summary>
	/// Получить виртуальный путь к файлу (работает в режиме IncludeAreaComponentMode.File)
	/// FilePath приведённое к в AppRelative, при неудаче - пустатя строка.
	/// </summary>
	/// <returns></returns>
	public string GetVirtualFilePath()
	{
		string result = FilePath;
		//считаем все пути начинающиеся с '/' относительно корня приложения
		if(!string.IsNullOrEmpty(result) && result[0] == '/')
			result = string.Concat("~", result);

		if (!string.IsNullOrEmpty(result) && !VirtualPathUtility.IsAppRelative(result))
			result = VirtualPathUtility.ToAppRelative(this.Page.ResolveUrl(result));

		if (!string.IsNullOrEmpty(result) && !VirtualPathUtility.IsAppRelative(result))
			result = string.Empty;

		return result;
	}


	/// <summary>
	/// Проверка поддержки расширения
	/// </summary>
	/// <param name="ext"></param>
	/// <returns></returns>
	public bool IsFilePathExtensionAllowed(string ext)
	{
		return string.Equals(ext, ".ascx")
			|| string.Equals(ext, ".htm")
			|| string.Equals(ext, ".html")
			|| string.Equals(ext, ".txt");
	}

	/// <summary>
	/// Кодировка файла(работает в режиме IncludeAreaComponentMode.File)
	/// </summary>
	public Encoding FileEncoding
	{
		get
		{
			Encoding result = null;
			string encodingWebName = Parameters.Get(GetParameterKey(IncludeAreaComponentParameter.FileEncoding));
			if (!string.IsNullOrEmpty(encodingWebName))
			{
				try
				{
					result = Encoding.GetEncoding(encodingWebName);
				}
				catch (ArgumentException /*exc*/)
				{
				}
			}

			//if (result == null)
			//	result = BXConfigurationUtility.DefaultEncoding;

			return result;
		}
		set
		{
			Parameters[GetParameterKey(IncludeAreaComponentParameter.FileEncoding)] = value.WebName;
		}
	}

	/// <summary>
	/// Имя файла (работает в режиме IncludeAreaComponentMode.Section)
	/// </summary>
	public string FileName
	{
		get
		{
			return Parameters.Get(GetParameterKey(IncludeAreaComponentParameter.FileName), string.Empty);
		}
		set
		{
			Parameters[GetParameterKey(IncludeAreaComponentParameter.FileName)] = value;
		}
	}

	/// <summary>
	/// Суффикс для имени файла (работает в режиме IncludeAreaComponentMode.Page)
	/// </summary>
	public string FileNameSuffix
	{
		get
		{
			return Parameters.Get(GetParameterKey(IncludeAreaComponentParameter.FileNameSuffix), string.Empty);
		}
		set
		{
			Parameters[GetParameterKey(IncludeAreaComponentParameter.FileNameSuffix)] = value;
		}
	}

	/// <summary>
	/// Рекурсивный поиск подключаемого файла (работает в режиме IncludeAreaComponentMode.Section - если файл не найден в "текущем" разделе, то поиск продолжается в родительском и т.д. пока файл с нужным именем не будет найден или не будет достигнут корень сайта)
	/// </summary>
	public bool RecursiveFileSearch
	{
		get
		{
			return Parameters.Get<bool>(GetParameterKey(IncludeAreaComponentParameter.RecursiveFileSearch), true);
		}
		set
		{
			Parameters[GetParameterKey(IncludeAreaComponentParameter.RecursiveFileSearch)] = value.ToString();
		}
	}

	/// <summary>
	/// Разрешать редактирование включаемого файла
	/// </summary>
	public bool AllowEditing
	{
		get
		{
			return Parameters.GetBool(GetParameterKey(IncludeAreaComponentParameter.AllowEditing), true);
		}
		set
		{
			Parameters[GetParameterKey(IncludeAreaComponentParameter.AllowEditing)] = value.ToString();
		}
	}

	/// <summary>
	/// Получить ключ параметра
	/// </summary>
	/// <param name="parameter"></param>
	/// <returns></returns>
	public static string GetParameterKey(IncludeAreaComponentParameter parameter)
	{
		return Enum.GetName(typeof(IncludeAreaComponentParameter), parameter);
		#region old
		//switch (parameter)
		//{
		//    case IncludeAreaComponentParameter.Mode:
		//        return "Mode";
		//    case IncludeAreaComponentParameter.FilePath:
		//        return "FilePath";
		//    case IncludeAreaComponentParameter.FileName:
		//        return "FileName";
		//    case IncludeAreaComponentParameter.FileNameSuffix:
		//        return "FileNameSuffix";
		//    default:
		//        throw new InvalidOperationException(string.Format("Value '{0}' is unknown in current context!", Enum.GetName(typeof(IncludeAreaComponentParameter), parameter)));
		//}
		#endregion
	}
	protected override void OnLoad(EventArgs e)
	{
		IncludeComponentTemplate();
		base.OnLoad(e);
	}

	protected override void PreLoadComponentDefinition()
	{
		Title = GetMessageRaw("Title");
		Description = GetMessageRaw("Description");
		Icon = "images/include.gif";

		Group = new BXComponentGroup("includedArea", GetMessageRaw("Group"), 100, BXComponentGroup.Utility);

		string legendStr = "Legend",
			clientSideActionGroupViewId = ClientID;


		//AREA_FILE_SHOW
		string modeParamKey = GetParameterKey(IncludeAreaComponentParameter.Mode);
		ParamsDefinition.Add(
			modeParamKey,
			new BXParamSingleSelection(
				GetMessageRaw(string.Concat(modeParamKey, legendStr)),
				Enum.GetName(typeof(IncludeAreaComponentMode), IncludeAreaComponentMode.Page),
				BXCategory.Main,
				null,
				new ParamClientSideActionGroupViewSelector(clientSideActionGroupViewId, modeParamKey)
			)
		);
		//ParamsDefinition[modeParamKey].RefreshOnDirty = true; 
		//PATH
		string filePathParamKey = GetParameterKey(IncludeAreaComponentParameter.FilePath);
		ParamsDefinition.Add(
			filePathParamKey,
			new BXParamText(
				GetMessageRaw(string.Concat(filePathParamKey, legendStr)),
				string.Empty,
				BXCategory.Main,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, filePathParamKey, new string[] { Enum.GetName(typeof(IncludeAreaComponentMode), IncludeAreaComponentMode.File) })
			)
		);

		//...
		string fileNameParamKey = GetParameterKey(IncludeAreaComponentParameter.FileName);
		ParamsDefinition.Add(
			fileNameParamKey,
			new BXParamText(
				GetMessageRaw(string.Concat(fileNameParamKey, legendStr)),
				string.Empty,
				BXCategory.Main,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, fileNameParamKey, new string[] { Enum.GetName(typeof(IncludeAreaComponentMode), IncludeAreaComponentMode.Section) })
			)
		);

		//AREA_FILE_SUFFIX
		string fileNameSuffixParamKey = GetParameterKey(IncludeAreaComponentParameter.FileNameSuffix);
		ParamsDefinition.Add(
			fileNameSuffixParamKey,
			new BXParamText(
				GetMessageRaw(string.Concat(fileNameSuffixParamKey, legendStr)),
				string.Empty,
				BXCategory.Main,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, fileNameSuffixParamKey, new string[] { Enum.GetName(typeof(IncludeAreaComponentMode), IncludeAreaComponentMode.Page) })
			)
		);

		//AREA_FILE_RECURSIVE
		string recursiveFileSearchParamKey = GetParameterKey(IncludeAreaComponentParameter.RecursiveFileSearch);
		ParamsDefinition.Add(
			recursiveFileSearchParamKey,
			new BXParamYesNo(
				GetMessageRaw(string.Concat(recursiveFileSearchParamKey, legendStr)),
				true,
				BXCategory.Main,
				new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, recursiveFileSearchParamKey, new string[] { Enum.GetName(typeof(IncludeAreaComponentMode), IncludeAreaComponentMode.Section) })
			)
		);

		string allowEditingParamKey = GetParameterKey(IncludeAreaComponentParameter.AllowEditing);
		ParamsDefinition.Add(
			allowEditingParamKey,
			new BXParamYesNo(
				GetMessageRaw(string.Concat(allowEditingParamKey, legendStr)),
				true,
				BXCategory.Main
			)
		);
	}

	protected override void LoadComponentDefinition()
	{
		string[] modeNameArr = Enum.GetNames(typeof(IncludeAreaComponentMode));
		int modeNamesCount = modeNameArr != null ? modeNameArr.Length : 0;
		if (modeNamesCount > 0)
		{
			BXParamValue[] paramValueArr = new BXParamValue[modeNamesCount];
			for (int i = 0; i < modeNamesCount; i++)
			{
				string curModeName = modeNameArr[i];
				paramValueArr[i] = new BXParamValue(GetMessageRaw(string.Concat("ModeTitle", curModeName)), curModeName);
			}
			List<BXParamValue> modeValues = ParamsDefinition[GetParameterKey(IncludeAreaComponentParameter.Mode)].Values;
			if (modeValues != null)
			{
				if (modeValues.Count > 0)
					modeValues.Clear();
				modeValues.AddRange(paramValueArr);
			}
			else
				ParamsDefinition[GetParameterKey(IncludeAreaComponentParameter.Mode)].Values = new List<BXParamValue>(paramValueArr);
		}
	}

	/// <summary>
	/// Получить имя включаемого файла
	/// </summary>
	/// <param name="virtualDirPath"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	protected string GetIncludeFileName(string virtualDirPath, string name)
	{
		if (string.IsNullOrEmpty(virtualDirPath))
			throw new ArgumentException("virtualDirPath", "Is not specified!");

		if (string.IsNullOrEmpty(name))
			throw new ArgumentException("Is not specified!", "name");

		string physicalDirPath = Request.MapPath(virtualDirPath);
		
		if(!Directory.Exists(physicalDirPath))
			return string.Empty;

		string result = string.Empty;
		string[] filePathArr = Directory.GetFiles(physicalDirPath, string.Concat(name, ".*"));
		if (filePathArr.Length > 0)
		{
			int filePathInd = Array.FindIndex<string>(filePathArr,
				delegate(string filePath)
				{
					return filePath.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase);
				});

			if (filePathInd < 0)
				filePathInd = Array.FindIndex<string>(filePathArr,
					delegate(string filePath)
					{
						return filePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".htm", StringComparison.OrdinalIgnoreCase);
					}
					);

			if (filePathInd < 0)
				filePathInd = Array.FindIndex<string>(filePathArr,
					delegate(string filePath)
					{
						return filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
					}
					);

			if (filePathInd >= 0)
			{
				FileInfo fi = new FileInfo(filePathArr[filePathInd]);
				result = fi.Name;
			}
		}

		return result;
	}

	private string _includeFileVirtualPath = null;
	/// <summary>
	/// Получить путь к включаемому файлу
	/// </summary>
	/// <returns></returns>
	public string GetIncludeFileVirtualPath()
	{
		if (_includeFileVirtualPath != null)
			return _includeFileVirtualPath;

		string result = string.Empty;

		switch (Mode)
		{
			case IncludeAreaComponentMode.File:
				{
					result = GetVirtualFilePath();
					break;
				}
			case IncludeAreaComponentMode.Page:
				{
					string pageVirtualPath = Page != null ? Page.AppRelativeVirtualPath : string.Empty;

					if (!string.IsNullOrEmpty(pageVirtualPath))
					{
						string virtualDirPath = VirtualPathUtility.GetDirectory(pageVirtualPath);
						string pageFileName = VirtualPathUtility.GetFileName(pageVirtualPath);
						if (pageFileName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
						{
							string includeFileName = pageFileName.Substring(0, pageFileName.Length - 5);
							string includeFileNameSuffix = FileNameSuffix;
							if (!string.IsNullOrEmpty(includeFileNameSuffix))
								includeFileName = string.Concat(includeFileName, includeFileNameSuffix);

							string resultIncludeFileName = GetIncludeFileName(virtualDirPath, includeFileName);
							//if (string.IsNullOrEmpty(resultIncludeFileName) && !ensureExists)
							//    resultIncludeFileName = GetDefaultIncludeFileName(includeFileName);
							if (!string.IsNullOrEmpty(resultIncludeFileName))
								result = VirtualPathUtility.Combine(virtualDirPath, resultIncludeFileName);
						}
					}
					break;
				}
			case IncludeAreaComponentMode.Section:
				{
					string pageVirtualPath = Page != null ? Page.AppRelativeVirtualPath : string.Empty;
					if (!string.IsNullOrEmpty(pageVirtualPath))
					{
						string virtualDirPath = VirtualPathUtility.GetDirectory(pageVirtualPath);
						string includeFileName = string.Empty;
						string includeFileNameFirstPart = FileName;
						bool aboutRecursiveSearch = RecursiveFileSearch;
						if (!string.IsNullOrEmpty(includeFileNameFirstPart))
							while (string.IsNullOrEmpty(includeFileName))
							{
								includeFileName = GetIncludeFileName(virtualDirPath, includeFileNameFirstPart);
								if (!aboutRecursiveSearch || string.Equals(virtualDirPath, "~/", StringComparison.OrdinalIgnoreCase))
									break;
								if (string.IsNullOrEmpty(includeFileName))
									virtualDirPath = VirtualPathUtility.GetDirectory(VirtualPathUtility.RemoveTrailingSlash(virtualDirPath));
							}

						//if (string.IsNullOrEmpty(includeFileName) && !ensureExists)
						//    includeFileName = GetDefaultIncludeFileName(includeFileNameFirstPart);

						if (!string.IsNullOrEmpty(includeFileName))
							result = VirtualPathUtility.Combine(virtualDirPath, includeFileName);
					}
					break;
				}
		}

		_includeFileVirtualPath =
			!string.IsNullOrEmpty(result) && VirtualPathUtility.IsAppRelative(result) ? result : string.Empty;

		return _includeFileVirtualPath;
	}

	private string _defaultIncludeFileVirtualPath = null;
	/// <summary>
	/// Получить путь к включаемому файлу по умолчанию (без проверки на существование и выборки из существующих альтернатив)
	/// </summary>
	/// <returns></returns>
	public string GetDefaultIncludeFileVirtualPath()
	{
		if (_defaultIncludeFileVirtualPath != null)
			return _defaultIncludeFileVirtualPath;

		string result = string.Empty;

		switch (Mode)
		{
			case IncludeAreaComponentMode.File:
				{
					result = GetVirtualFilePath();
					break;
				}
			case IncludeAreaComponentMode.Page:
				{
					string pageVirtualPath = Page != null ? Page.AppRelativeVirtualPath : string.Empty;

					if (!string.IsNullOrEmpty(pageVirtualPath))
					{
						string virtualDirPath = VirtualPathUtility.GetDirectory(pageVirtualPath);
						string pageFileName = VirtualPathUtility.GetFileName(pageVirtualPath);
						if (pageFileName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
						{
							string includeFileName = pageFileName.Substring(0, pageFileName.Length - 5);
							string includeFileNameSuffix = FileNameSuffix;
							if (!string.IsNullOrEmpty(includeFileNameSuffix))
								includeFileName = string.Concat(includeFileName, includeFileNameSuffix);

							result = VirtualPathUtility.Combine(virtualDirPath, string.Concat(includeFileName, ".ascx"));
						}
					}
					break;
				}
			case IncludeAreaComponentMode.Section:
				{
					string pageVirtualPath = Page != null ? Page.AppRelativeVirtualPath : string.Empty;
					if (!string.IsNullOrEmpty(pageVirtualPath))
					{
						string virtualDirPath = VirtualPathUtility.GetDirectory(pageVirtualPath);
						string includeFileNameFirstPart = FileName;
						if (!string.IsNullOrEmpty(includeFileNameFirstPart))
							result = VirtualPathUtility.Combine(virtualDirPath, string.Concat(includeFileNameFirstPart, ".ascx"));
					}
					break;
				}
		}

		_defaultIncludeFileVirtualPath =
			!string.IsNullOrEmpty(result) && VirtualPathUtility.IsAppRelative(result) ? result : string.Empty;

		return _defaultIncludeFileVirtualPath;
	}

	public override BXComponentPopupMenuInfo PopupMenuInfo
	{
		get
		{
			if(this.popupMenuInfo != null)
				return this.popupMenuInfo;

			BXComponentPopupMenuInfo info = base.PopupMenuInfo;

			if (!AllowEditing)
				return info;

			info.CreateComponentContentMenuItems = delegate(BXShowMode showMode)
			{

				if (showMode == BXShowMode.View)
					return new BXHermitagePopupMenuBaseItem[0];

				string includeFileVirtualPath = GetIncludeFileVirtualPath();

				if (string.IsNullOrEmpty(includeFileVirtualPath))
					includeFileVirtualPath = GetDefaultIncludeFileVirtualPath();

				if (string.IsNullOrEmpty(includeFileVirtualPath))
					return new BXHermitagePopupMenuBaseItem[0];

				string ext = VirtualPathUtility.GetExtension(includeFileVirtualPath);
				if (!IsFilePathExtensionAllowed(ext))
					return new BXHermitagePopupMenuBaseItem[0];

				if (!BXSecureIO.CheckWrite(includeFileVirtualPath))
					return new BXHermitagePopupMenuBaseItem[0];

				if (BXSecureIO.FileExists(includeFileVirtualPath))
				{
					bool createSourceCodeModification = !string.Equals(ext, ".txt", StringComparison.InvariantCultureIgnoreCase);

					BXHermitagePopupMenuBaseItem[] result = new BXHermitagePopupMenuBaseItem[1];
					
					BXHermitagePopupMenuItemContainer modifyInEditor = new BXHermitagePopupMenuItemContainer();
					result[0] = modifyInEditor;

					modifyInEditor.Id = string.Concat("INCLUDE_AREA_EDIT_IN_EDITOR_", ID);
					string modifyInEditorText = string.Empty;
					switch (Mode)
					{
						case IncludeAreaComponentMode.File:
							modifyInEditorText = GetMessageRaw("ModifyInclAreaFile");
							break;
						case IncludeAreaComponentMode.Page:
							modifyInEditorText = GetMessageRaw("ModifyInclAreaCurrentPage");
							break;
						case IncludeAreaComponentMode.Section:
							modifyInEditorText = GetMessageRaw("ModifyInclAreaSection");
							break;
						default:
							modifyInEditorText = GetMessageRaw("ModifyInclAreaGeneral");
							break;
					}
					modifyInEditor.Text = modifyInEditorText;
					Encoding fileEnc = FileEncoding;

					modifyInEditor.ClientClickScript = string.Format(
						"(new BX.CDialogNet({{ 'content_url':'{0}?path={1}{2}&lang={3}&clientType=WindowManager', 'min_width':'780', 'min_height':'400', 'width':'968', 'height':'604' }})).Show();",
						BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/dialogs/VisualPageEditor.aspx")), 
						BXJSUtility.Encode(HttpUtility.UrlEncode(includeFileVirtualPath)), 
						fileEnc != null ? string.Concat("&encoding=", BXJSUtility.Encode(HttpUtility.UrlEncode(fileEnc.WebName))) : string.Empty,
                        BXJSUtility.Encode(HttpUtility.UrlEncode(BXLoc.CurrentLocale)));
					modifyInEditor.IconCssClass = "bx-context-toolbar-edit-icon";
					modifyInEditor.Sort = 10;

					if (createSourceCodeModification)
					{
						BXHermitagePopupMenuItem modifySourceCode = modifyInEditor.CreateItem();

						modifySourceCode.Id = string.Concat("INCLUDE_AREA_EDIT_SOURCE_CODE_", ID);
						string modifySourceCodeText = string.Empty;
						switch (Mode)
						{
							case IncludeAreaComponentMode.File:
								modifySourceCodeText = GetMessageRaw("ModifySourceCodeInclAreaFile");
								break;
							case IncludeAreaComponentMode.Page:
								modifySourceCodeText = GetMessageRaw("ModifySourceCodeInclAreaCurrentPage");
								break;
							case IncludeAreaComponentMode.Section:
								modifySourceCodeText = GetMessageRaw("ModifySourceCodeInclAreaSection");
								break;
							default:
								modifySourceCodeText = GetMessageRaw("ModifySourceCodeInclAreaGeneral");
								break;
						}
						modifySourceCode.Text = modifySourceCodeText;
						modifySourceCode.ClientClickScript = string.Format(
                            "(new BX.CDialogNet({{ 'content_url':'{0}?path={1}{2}&lang={3}&vpe_mode=PlainText&clientType=WindowManager', 'min_width':'780', 'min_height':'400', 'width':'968', 'height':'604' }})).Show();",
							BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/dialogs/VisualPageEditor.aspx")), 
							BXJSUtility.Encode(HttpUtility.UrlEncode(includeFileVirtualPath)), 
							fileEnc != null ? string.Concat("&encoding=", BXJSUtility.Encode(HttpUtility.UrlEncode(fileEnc.WebName))) : string.Empty,
                            BXJSUtility.Encode(HttpUtility.UrlEncode(BXLoc.CurrentLocale)));
						modifySourceCode.IconCssClass = "bx-context-toolbar-edit-icon";
						modifySourceCode.Sort = 10;
					}
					return result;
				}
				else
				{
					BXHermitagePopupMenuItem[] result = new BXHermitagePopupMenuItem[1];

					BXHermitagePopupMenuItem create = new BXHermitagePopupMenuItem();
					result[0] = create;

					create.Id = string.Concat("INCLUDE_AREA_CREATE_FILE_", ID);
					string createText = string.Empty;
					switch (Mode)
					{
						case IncludeAreaComponentMode.File:
							createText = GetMessageRaw("CreateInclAreaFile");
							break;
						case IncludeAreaComponentMode.Page:
							createText = GetMessageRaw("CreateInclAreaCurrentPage");
							break;
						case IncludeAreaComponentMode.Section:
							createText = GetMessageRaw("CreateInclAreaSection");
							break;
						default:
							createText = GetMessageRaw("CreateInclAreaGeneral");
							break;
					}
					create.Text = createText;
					Encoding fileEnc = FileEncoding;
					create.ClientClickScript = string.Format(
						"(new BX.CDialogNet({{ 'content_url':'{0}?clientType=WindowManager&new&path={1}&encoding={2}&lang={3}', 'min_width':'775', 'min_height':'570', 'width':'775', 'height':'570' }})).Show();",
						BXJSUtility.Encode(VirtualPathUtility.ToAbsolute("~/bitrix/dialogs/VisualPageEditor.aspx")), 
						BXJSUtility.Encode(HttpUtility.UrlEncode(includeFileVirtualPath)), 
						fileEnc != null ? BXJSUtility.Encode(HttpUtility.UrlEncode(fileEnc.WebName)) : string.Empty,
                        BXJSUtility.Encode(HttpUtility.UrlEncode(BXLoc.CurrentLocale)));
					create.IconCssClass = "bx-context-toolbar-create-icon";
					create.Sort = 10;
					return result;
				}
			};

			return info;
		}
	}
}

/// <summary>
/// Базовый класс для шаблонов компонента "IncludedAreaComponent"
/// </summary>
public abstract class IncludeAreaComponentTemplate : BXComponentTemplate<IncludeAreaComponent>
{
	protected bool TryMapPath(string virtualPath, out string physicalPath)
	{
		if (string.IsNullOrEmpty(virtualPath))
		{
			physicalPath = string.Empty;
			return false;
		}

		try
		{
			physicalPath = Request.MapPath(virtualPath);
		}
		catch (HttpException /*exc*/)
		{
			physicalPath = string.Empty;
			return false;
		}

		return true;
	}

	protected bool TryGetFileInfo(string physicalPath, out FileInfo fi)
	{
		if (string.IsNullOrEmpty(physicalPath))
		{
			fi = null;
			return false;
		}

		try
		{
			fi = new System.IO.FileInfo(physicalPath);
		}
		catch (Exception /*exc*/)
		{
			fi = null;
			return false;
		}
		#region exc
		//catch (System.Security.SecurityException /*exc*/) { }
		//catch (ArgumentException /*exc*/) { }
		//catch (UnauthorizedAccessException /*exc*/) { }
		//catch (System.IO.PathTooLongException /*exc*/) { }
		//catch (NotSupportedException /*exc*/) { }
		#endregion

		return true;
	}

	public Control LoadUserControl(string virtualPath)
	{
		if (string.IsNullOrEmpty(virtualPath))
			throw new ArgumentException("Is not specified!", virtualPath);

		string physicalPath;
		if (!TryMapPath(virtualPath, out physicalPath))
			return null;

		FileInfo fileInfo;
		if (!TryGetFileInfo(physicalPath, out fileInfo) || !fileInfo.Exists)
			return null;

		return (System.Web.UI.Control)System.Web.Compilation.BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(System.Web.UI.Control));
	}

	public Control LoadLiteralControl(string virtualPath, Encoding encoding, bool htmlEncode)
	{
		if (string.IsNullOrEmpty(virtualPath))
			throw new ArgumentException("Is not specified!", virtualPath);

		string physicalPath;
		if (!TryMapPath(virtualPath, out physicalPath))
			return null;

		FileInfo fileInfo;
		if (!TryGetFileInfo(physicalPath, out fileInfo) || !fileInfo.Exists)
			return null;

		string sourceText = File.ReadAllText(physicalPath, encoding);
		if (htmlEncode)
			sourceText = HttpUtility.HtmlEncode(sourceText);
		System.Web.UI.LiteralControl litCtrl = new System.Web.UI.LiteralControl(sourceText);
		return litCtrl;
	}

	private Control _contentControl = null;
	private bool _isContentControlLoaded = false;
	/// <summary>
	/// Получить эл-т управления с содержанием
	/// </summary>
	/// <returns></returns>
	protected Control GetContentControl()
	{
		if (_isContentControlLoaded)
			return _contentControl;

		IncludeAreaComponent component = Component;
		if (component == null)
			throw new InvalidOperationException("Could not find component!");

		try
		{
			string sourceFilePath = component.GetIncludeFileVirtualPath();
			if (!string.IsNullOrEmpty(sourceFilePath))
			{

				string ext = VirtualPathUtility.GetExtension(sourceFilePath);
				if (!Component.IsFilePathExtensionAllowed(ext))
				{
					System.Web.UI.LiteralControl litCtrl = new System.Web.UI.LiteralControl(HttpUtility.HtmlEncode(string.Format(Component.GetMessageRaw("FileExtensionIsNotAllowed"), ext)));
					_contentControl = litCtrl;
				}
				else
				{
					if (string.Equals(ext, ".ascx", StringComparison.OrdinalIgnoreCase))
						_contentControl = LoadUserControl(sourceFilePath);
					else if (string.Equals(ext, ".txt", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(ext, ".htm", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(ext, ".html", StringComparison.OrdinalIgnoreCase))
					{
						bool htmlEncode = string.Equals(ext, ".txt", StringComparison.OrdinalIgnoreCase);
						Encoding encoding = component.FileEncoding;
						_contentControl = LoadLiteralControl(sourceFilePath, encoding != null ? encoding : BXConfigurationUtility.DefaultEncoding, htmlEncode);
					}
				}
			}
		}
		catch (Exception /*exc*/)
		{
			//_contentControl = new LiteralControl(BXLoc.GetMessage(Component, "ErrorLodadingIncludeFile"));
		}


		if (_contentControl != null)
			_contentControl.ID = "container";

		_isContentControlLoaded = true;
		return _contentControl;
	}

	protected override void Render(HtmlTextWriter writer)
	{
		Control contentControl = GetContentControl();
		if (contentControl == null && IsComponentDesignMode)
		{
			writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
			return;
		}
		base.Render(writer);
	}
}
