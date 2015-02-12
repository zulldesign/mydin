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
using Bitrix.Components.Editor;
using System.IO;
using System.Text;
using Bitrix.Services;

namespace Bitrix.Main.Components
{

	/// <summary>
	/// Компонент "Медиа проигрыватель"
	/// </summary>
	public partial class MediaPlayerComponent : Bitrix.UI.BXComponent
	{
		public MediaPlayerComponent()
		{
			EnableViewState = false;
			EnableTheming = false; //Нельзя определять в разметке из-за реализации BXParser.GetAllComponentsSummary()
		}
		/// <summary>
		/// Получить ключ параметра
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public static string GetParameterKey(MediaPlayerComponentParameter parameter)
		{
			return parameter.ToString().ToLowerInvariant();
		}

		/// <summary>
		/// Получить папку по умолчанию для тем "Flash Player"
		/// </summary>
		/// <returns></returns>
		public string GetFlvDefaultSkinFolderVirtualPath()
		{
			return "~/bitrix/controls/main/media.player/flv/skins";
		}

		/*
		/// <summary>
		/// Получить текущий тип проигрывателя
		/// Позволяет разрешить пользовательскую установку "Auto" в определённый тип проигрывателя исходя из расширения файла.
		/// </summary>
		/// <returns></returns>
		public PlayerComponentPlayerType GetCurrentPlayerType() 
		{
			PlayerComponentPlayerType r = PlayerType;
			if (r == PlayerComponentPlayerType.Auto)
			{
				string filePath = SourceFilePath;
				int extStartIndex = -1;
				if (filePath != null && filePath.Length > 4 && (extStartIndex = filePath.LastIndexOf('.')) > 0)
				{
					string ext = filePath.Substring(extStartIndex);
					r = string.Equals(ext, ".wma", StringComparison.InvariantCultureIgnoreCase) || 
						string.Equals(ext, ".wmv", StringComparison.InvariantCultureIgnoreCase) ? 
						PlayerComponentPlayerType.Wmv : PlayerComponentPlayerType.Flv;
				}
				else 
					r = PlayerComponentPlayerType.Flv;
			}
			return r;
		}
		*/
		/// <summary>
		/// Получить строку настроек проигрывателя "Flash Player"
		/// </summary>
		/// <returns></returns>

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            if (IsCached())
                return;

			IncludeComponentTemplate();
		}

		private string PrepareAbsoluteUrl(string url)
		{
			if(string.IsNullOrEmpty(url))
				return string.Empty;

			UriBuilder bldr = new UriBuilder(Request.Url);
			if(!url.StartsWith("~", StringComparison.Ordinal))
			{
				bldr.Path = bldr.Query = string.Empty;
				Uri uri = new Uri(bldr.Uri, url);
				return uri.AbsoluteUri;		
			}

			int whatInd = url.IndexOf('?');
			bldr.Path = VirtualPathUtility.ToAbsolute(whatInd >= 0 ? url.Substring(0, whatInd) : url);
			bldr.Query = whatInd >= 0 ? url.Substring(whatInd + 1) : string.Empty;

			return bldr.Uri.AbsoluteUri;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if(IsRenderedFromCache)
				return;

			SourceFilePath = PrepareAbsoluteUrl(SourceFilePath);
			LinkForDownloadSourceFileUrl = PrepareAbsoluteUrl(LinkForDownloadSourceFileUrl);
			PreviewImageFilePath = PrepareAbsoluteUrl(PreviewImageFilePath);
		}

		#region Parameters
		/// <summary>
		/// Путь к файлу
		/// </summary>
		public string SourceFilePath
		{
			get
			{
				return Parameters.Get(GetParameterKey(MediaPlayerComponentParameter.SourceFilePath), string.Empty);
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.SourceFilePath)] = value;
			}
		}


		/*
		/// <summary>
		/// Путь к списку воспроизведения 
		/// </summary>
        
		public string PlayListFilePath
		{
			get 
			{
				return Parameters.Get(GetParameterKey(PlayerComponentParameter.PlayListFilePath), string.Empty);
			}
			set 
			{
				Parameters[GetParameterKey(PlayerComponentParameter.PlayListFilePath)] = value;
			}
		}
		*/

		/// <summary>
		/// Тип проигрывателя
		/// </summary>
		public MediaPlayerComponentPlayerType PlayerType
		{
			get
			{
				string s = Parameters.Get(GetParameterKey(MediaPlayerComponentParameter.PlayerType), string.Empty);
				if (string.IsNullOrEmpty(s))
					return MediaPlayerComponentPlayerType.Auto;
				MediaPlayerComponentPlayerType? r = null;
				try
				{
					r = Enum.Parse(typeof(MediaPlayerComponentPlayerType), s) as MediaPlayerComponentPlayerType?;
				}
				catch (ArgumentException /*e*/)
				{
					r = MediaPlayerComponentPlayerType.Auto;
				}
				return r.Value;
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.PlayerType)] = value.ToString();
			}
		}

		/// <summary>
		/// Ширина
		/// </summary>
		public int Width
		{
			get
			{
				return Parameters.Get<int>(GetParameterKey(MediaPlayerComponentParameter.Width), 400);
			}
			set
			{
				if (value < 0)
					value = 400;
				Parameters[GetParameterKey(MediaPlayerComponentParameter.Width)] = value.ToString();
			}
		}

		/// <summary>
		/// Высота
		/// </summary>
		public int Height
		{
			get
			{
				return Parameters.Get<int>(GetParameterKey(MediaPlayerComponentParameter.Height), 300);
			}
			set
			{
				if (value < 0)
					value = 300;
				Parameters[GetParameterKey(MediaPlayerComponentParameter.Height)] = value.ToString();
			}
		}

		/// <summary>
		/// Путь к рисунку для предварительного просмотра
		/// </summary>
		public string PreviewImageFilePath
		{
			get
			{
				return Parameters.Get(GetParameterKey(MediaPlayerComponentParameter.PreviewImageFilePath), string.Empty);
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.PreviewImageFilePath)] = value;
			}
		}

		/// <summary>
		/// Путь к изображению авторского знака
		/// </summary>
		public string LogoImageFilePath
		{
			get
			{
				return Parameters.Get(GetParameterKey(MediaPlayerComponentParameter.LogoImageFilePath), string.Empty);
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.LogoImageFilePath)] = value;
			}
		}

		/// <summary>
		/// Разрешить полноэкранный режим
		/// </summary>
		public bool EnableFullScreenModeSwitch
		{
			get
			{
				return Parameters.Get<bool>(GetParameterKey(MediaPlayerComponentParameter.EnableFullScreenModeSwitch), true);
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.EnableFullScreenModeSwitch)] = value.ToString();
			}
		}

		/// <summary>
		/// Автоматически начать проигрывать
		/// </summary>
		public bool EnableAutoStart
		{
			get
			{
				return Parameters.Get<bool>(GetParameterKey(MediaPlayerComponentParameter.EnableAutoStart), false);
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.EnableAutoStart)] = value.ToString();
			}
		}


		/// <summary>
		/// Повторять композицию или список воспроизведения
		/// </summary>
		public bool EnableRepeatMode
		{
			get
			{
				return Parameters.Get<bool>(GetParameterKey(MediaPlayerComponentParameter.EnableRepeatMode), false);
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.EnableRepeatMode)] = value.ToString();
			}
		}

		/// <summary>
		/// Уровень громкости в процентах от максимального
		/// </summary>
		public int VolumeLevelInPercents
		{
			get
			{
				return Parameters.Get<int>(GetParameterKey(MediaPlayerComponentParameter.VolumeLevelInPercents), 90);
			}
			set
			{
				if (value < 0)
					value = 90;
				else if (value > 100)
					value = 100;
				Parameters[GetParameterKey(MediaPlayerComponentParameter.VolumeLevelInPercents)] = value.ToString();
			}
		}

		/// <summary>
		/// Начать проигрывать с элемента в списке
		/// </summary>
		public int PlayListStartAtPosition
		{
			get
			{
				return Parameters.Get<int>(GetParameterKey(MediaPlayerComponentParameter.PlayListStartAtPosition), 0);
			}
			set
			{
				if (value < 0)
					value = 0;
				Parameters[GetParameterKey(MediaPlayerComponentParameter.PlayListStartAtPosition)] = value.ToString();
			}
		}

		/// <summary>
		/// Размер буфера в секундах
		/// </summary>
		public int BufferLengthInSeconds
		{
			get
			{
				return Parameters.GetInt(GetParameterKey(MediaPlayerComponentParameter.BufferLengthInSeconds), 10);
			}
			set
			{
				if (value < 0)
					value = 10;
				Parameters[GetParameterKey(MediaPlayerComponentParameter.BufferLengthInSeconds)] = value.ToString();
			}
		}

		/// <summary>
		/// Ссылка для скачивания ролика
		/// </summary>
		public string LinkForDownloadSourceFileUrl
		{
			get
			{
				return Parameters.Get(GetParameterKey(MediaPlayerComponentParameter.LinkForDownloadSourceFileUrl), string.Empty);
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.LinkForDownloadSourceFileUrl)] = value;
			}
		}

		/// <summary>
		/// Открывать ссылку в
		/// </summary>
		public string LinkForDownloadSourceFileTargetWindow
		{
			get
			{
				return Parameters.Get(GetParameterKey(MediaPlayerComponentParameter.LinkForDownloadSourceFileTargetWindow), string.Empty);
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.LinkForDownloadSourceFileTargetWindow)] = value;
			}
		}


		/// <summary>
		/// Скрыть контекстное меню проигрывателя "Flash Player"
		/// </summary>
		public bool FlvHideContextMenu
		{
			get
			{
				return Parameters.Get<bool>(GetParameterKey(MediaPlayerComponentParameter.FlvHideContextMenu), false);
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.FlvHideContextMenu)] = value.ToString();
			}
		}

		/// <summary>
		/// Режим окна (WMode)
		/// </summary>
		public MediaPlayerComponentFlvWindowMode FlvWMode
		{
			get
			{
				string s = Parameters.Get(GetParameterKey(MediaPlayerComponentParameter.FlvWMode), string.Empty);
				if (string.IsNullOrEmpty(s))
					return MediaPlayerComponentFlvWindowMode.Window;
				MediaPlayerComponentFlvWindowMode? r = null;
				try
				{
					r = Enum.Parse(typeof(MediaPlayerComponentFlvWindowMode), s) as MediaPlayerComponentFlvWindowMode?;
				}
				catch (ArgumentException /*e*/)
				{
					r = MediaPlayerComponentFlvWindowMode.Window;
				}
				return r.Value;
			}
			set
			{
				Parameters[GetParameterKey(MediaPlayerComponentParameter.FlvWMode)] = value.ToString();
			}
		}

		/// <summary>
		/// Тескст о владельце проигрывателя
		/// </summary>
		public string AboutText
		{
			get
			{
				return GetMessageRaw("AboutText");
			}
		}

		/// <summary>
		/// Ссылка на сайт владельца проигрывателя
		/// </summary>
		public string AboutLink
		{
			get
			{
				return GetMessageRaw("AboutLink");
			}
		}

		#endregion

		#region Parameter definitions
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/player.gif";

			Group = new BXComponentGroup("media", GetMessageRaw("Group"), 100, BXComponentGroup.Content);

			string legendSuffix = "Legend",
				clientSideActionGroupViewId = ClientID;

			string addvancedSettingsGroupName = "advancedSettings";
			//singleFilePlaybackGroupName = "singleFlePlaybackSettings";
			//playListSettingsGroupName = "playListSettings";

			string playerTypeParamKey = GetParameterKey(MediaPlayerComponentParameter.PlayerType);
			ParamsDefinition.Add(
				playerTypeParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(playerTypeParamKey, legendSuffix)),
					MediaPlayerComponentPlayerType.Auto.ToString(),
					BXCategory.Main,
					null
				)
			);

			string sourceFilePathParamKey = GetParameterKey(MediaPlayerComponentParameter.SourceFilePath);
			ParamsDefinition.Add(
				sourceFilePathParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(sourceFilePathParamKey, legendSuffix)),
					string.Empty,
					BXCategory.Main
				)
			);


			/*
			string playListFilePathParamKey = GetParameterKey(PlayerComponentParameter.PlayListFilePath);
			ParamsDefinition.Add(
				playListFilePathParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(playListFilePathParamKey, legendSuffix)),
					string.Empty,
					BXCategory.Main,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, playListFilePathParamKey, new string[] { playListSettingsGroupName })
				)
			);

			string enablePlayListParamKey = GetParameterKey(PlayerComponentParameter.EnablePlayList);
			ParamsDefinition.Add(
				enablePlayListParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(enablePlayListParamKey, legendSuffix)),
					false,
					BXCategory.Main,
					new ParamClientSideActionGroupViewSwitch(clientSideActionGroupViewId, enablePlayListParamKey, playListSettingsGroupName, singleFilePlaybackGroupName)
				)
			 );
			 */

			string widthParamKey = GetParameterKey(MediaPlayerComponentParameter.Width);
			ParamsDefinition.Add(
				widthParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(widthParamKey, legendSuffix)),
					"425",
					BXCategory.Main
					)
			);

			string heightParamKey = GetParameterKey(MediaPlayerComponentParameter.Height);
			ParamsDefinition.Add(
				heightParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(heightParamKey, legendSuffix)),
					"344",
					BXCategory.Main
					)
			);

			string previewImageFilePathParamKey = GetParameterKey(MediaPlayerComponentParameter.PreviewImageFilePath);
			ParamsDefinition.Add(
				previewImageFilePathParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(previewImageFilePathParamKey, legendSuffix)),
					string.Empty,
					BXCategory.Main,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, previewImageFilePathParamKey, new string[] { addvancedSettingsGroupName })
					)
			);

            string enableAdvancedSettingsParamKey = GetParameterKey(MediaPlayerComponentParameter.EnableAdvancedSettings);
            ParamsDefinition.Add(
                enableAdvancedSettingsParamKey,
                new BXParamYesNo(
                    GetMessageRaw(string.Concat(enableAdvancedSettingsParamKey, legendSuffix)),
                    false,
                    BXCategory.Main,
                    new ParamClientSideActionGroupViewSwitch(
                        clientSideActionGroupViewId,
                        enableAdvancedSettingsParamKey,
                        addvancedSettingsGroupName,
                        string.Empty
                        )
                    )
                );

			BXCategory appearance = new BXCategory(GetMessageRaw("Category.AppearanceCommon.Title"), "PLAYER_APPEARANCE", 151);

			string logoImageFilePathParamKey = GetParameterKey(MediaPlayerComponentParameter.LogoImageFilePath);
			ParamsDefinition.Add(
				logoImageFilePathParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(logoImageFilePathParamKey, legendSuffix)),
					string.Empty,
					appearance,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, logoImageFilePathParamKey, new string[] { addvancedSettingsGroupName })
					)
			);

			string enableFullScreenModeSwitch = GetParameterKey(MediaPlayerComponentParameter.EnableFullScreenModeSwitch);
			ParamsDefinition.Add(
				enableFullScreenModeSwitch,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(enableFullScreenModeSwitch, legendSuffix)),
					true,
					appearance,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						enableFullScreenModeSwitch,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);


			string stretchingParamKey = GetParameterKey(MediaPlayerComponentParameter.Stretching);
			ParamsDefinition.Add(
				stretchingParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(stretchingParamKey, legendSuffix)),
					Enum.GetName(typeof(MediaPlayerStretchingMode), MediaPlayerStretchingMode.Proportionally),
					appearance,
					null,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						stretchingParamKey, new string[] { addvancedSettingsGroupName }
						)
					)
			);

			string controlPanelBackgroundColorParamKey = GetParameterKey(MediaPlayerComponentParameter.ControlPanelBackgroundColor);
			ParamsDefinition.Add(
				controlPanelBackgroundColorParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(controlPanelBackgroundColorParamKey, legendSuffix)),
					"FFFFFF",
					appearance,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						controlPanelBackgroundColorParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

			string controlsColorParamKey = GetParameterKey(MediaPlayerComponentParameter.ControlsColor);
			ParamsDefinition.Add(
				controlsColorParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(controlsColorParamKey, legendSuffix)),
					"000000",
					appearance,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						controlsColorParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);


			string controlsOverColorParamKey = GetParameterKey(MediaPlayerComponentParameter.ControlsOverColor);
			ParamsDefinition.Add(
				controlsOverColorParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(controlsOverColorParamKey, legendSuffix)),
					"000000",
					appearance,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						controlsOverColorParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

			string screenColorParamKey = GetParameterKey(MediaPlayerComponentParameter.ScreenColor);
			ParamsDefinition.Add(
				screenColorParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(screenColorParamKey, legendSuffix)),
					"000000",
					appearance,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						screenColorParamKey, new string[] { addvancedSettingsGroupName }
						)
					)
			);

			BXCategory appearanceFlv = new BXCategory(GetMessageRaw("Category.AppearanceFlv.Title"), "PLAYER_APPEARANCE_FLV", 155);
			string flvSkinFolderPathParamKey = GetParameterKey(MediaPlayerComponentParameter.FlvSkinFolderPath);
			ParamsDefinition.Add(
				flvSkinFolderPathParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(flvSkinFolderPathParamKey, legendSuffix)),
					GetFlvDefaultSkinFolderVirtualPath(),
					appearanceFlv,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, flvSkinFolderPathParamKey, new string[] { addvancedSettingsGroupName })
					)
			);

			ParamsDefinition[flvSkinFolderPathParamKey].RefreshOnDirty = true;

			string flvSkinNameParamKey = GetParameterKey(MediaPlayerComponentParameter.FlvSkinName);
			ParamsDefinition.Add(
				flvSkinNameParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(flvSkinNameParamKey, legendSuffix)),
					"bitrix",
					appearanceFlv,
					null,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, flvSkinNameParamKey, new string[] { addvancedSettingsGroupName })
					)
			);

			string flvControlBarLocationParamKey = GetParameterKey(MediaPlayerComponentParameter.FlvControlBarLocation);
			ParamsDefinition.Add(
				flvControlBarLocationParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(flvControlBarLocationParamKey, legendSuffix)),
					"bottom",
					appearanceFlv,
					null,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, flvControlBarLocationParamKey, new string[] { addvancedSettingsGroupName })
					)
			);


			string flvWmodeParamKey = GetParameterKey(MediaPlayerComponentParameter.FlvWMode);
			ParamsDefinition.Add(
				flvWmodeParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(flvWmodeParamKey, legendSuffix)),
                    "opaque",
					appearanceFlv,
					null,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, flvWmodeParamKey, new string[] { addvancedSettingsGroupName })
					)
			);

			string flvHideContextMenuParamKey = GetParameterKey(MediaPlayerComponentParameter.FlvHideContextMenu);
			ParamsDefinition.Add(
				flvHideContextMenuParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(flvHideContextMenuParamKey, legendSuffix)),
					false,
					appearanceFlv,
					new ParamClientSideActionGroupViewMember(clientSideActionGroupViewId, flvHideContextMenuParamKey, new string[] { addvancedSettingsGroupName })
					)
			);

			BXCategory appearanceWmv = new BXCategory(GetMessageRaw("Category.AppearanceWmv.Title"), "PLAYER_APPEARANCE_WMV", 155);
			/*string wmvPlayListLocationParamKey = GetParameterKey(PlayerComponentParameter.WmvPlayListLocation);
			ParamsDefinition.Add(
				wmvPlayListLocationParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(wmvPlayListLocationParamKey, legendSuffix)),
					string.Empty,
					appearanceWmv,
					null,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId, 
						wmvPlayListLocationParamKey, 
						new string[] { addvancedSettingsGroupName, playListSettingsGroupName },
						ParamClientSideActionGroupViewMemberDisplayCondition.And
						)
					)
			);

			string wmvPlayListSizeParamKey = GetParameterKey(PlayerComponentParameter.WmvPlayListSize);
			ParamsDefinition.Add(
				wmvPlayListSizeParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(wmvPlayListSizeParamKey, legendSuffix)),
					"180",
					appearanceWmv,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId, 
						wmvPlayListSizeParamKey, 
						new string[] { addvancedSettingsGroupName, playListSettingsGroupName },
						ParamClientSideActionGroupViewMemberDisplayCondition.And
						)
					)
			);

			string wmvPlayListTypeParamKey = GetParameterKey(PlayerComponentParameter.WmvPlayListType);
			ParamsDefinition.Add(
				wmvPlayListTypeParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(wmvPlayListTypeParamKey, legendSuffix)),
					string.Empty,
					appearanceWmv,
					null,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId, 
						wmvPlayListTypeParamKey, 
						new string[] { addvancedSettingsGroupName, playListSettingsGroupName },
						ParamClientSideActionGroupViewMemberDisplayCondition.And
						)
					)
			);

			string wmvPlayListPreviewImageWidthParamKey = GetParameterKey(PlayerComponentParameter.WmvPlayListPreviewImageWidth);
			ParamsDefinition.Add(
				wmvPlayListPreviewImageWidthParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(wmvPlayListPreviewImageWidthParamKey, legendSuffix)),
					"64",
					appearanceWmv,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId, 
						wmvPlayListPreviewImageWidthParamKey, 
						new string[] { addvancedSettingsGroupName, playListSettingsGroupName },
						ParamClientSideActionGroupViewMemberDisplayCondition.And
						)
					)
			);

			string wmvPlayListPreviewImageHeightParamKey = GetParameterKey(PlayerComponentParameter.WmvPlayListPreviewImageHeight);
			ParamsDefinition.Add(
				wmvPlayListPreviewImageHeightParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(wmvPlayListPreviewImageHeightParamKey, legendSuffix)),
					"48",
					appearanceWmv,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId, 
						wmvPlayListPreviewImageHeightParamKey, 
						new string[] { addvancedSettingsGroupName, playListSettingsGroupName },
						ParamClientSideActionGroupViewMemberDisplayCondition.And
						)
					)
			);*/

			string wmvShowControlPanelParamKey = GetParameterKey(MediaPlayerComponentParameter.WmvShowControlPanel);
			ParamsDefinition.Add(
				wmvShowControlPanelParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(wmvShowControlPanelParamKey, legendSuffix)),
					true,
					appearanceWmv,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						wmvShowControlPanelParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

			string wmvShowStopButtonParamKey = GetParameterKey(MediaPlayerComponentParameter.WmvShowStopButton);
			ParamsDefinition.Add(
				wmvShowStopButtonParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(wmvShowStopButtonParamKey, legendSuffix)),
					false,
					appearanceWmv,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						wmvShowStopButtonParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

			string wmvShowDigitsParamKey = GetParameterKey(MediaPlayerComponentParameter.WmvShowDigits);
			ParamsDefinition.Add(
				wmvShowDigitsParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(wmvShowDigitsParamKey, legendSuffix)),
					false,
					appearanceWmv,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						wmvShowDigitsParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

			string wmvWModeParamKey = GetParameterKey(MediaPlayerComponentParameter.WmvWMode);
			ParamsDefinition.Add(
				wmvWModeParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(wmvWModeParamKey, legendSuffix)),
                    "windowless",
					appearanceWmv,
					null,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId, wmvWModeParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

			string enableAutoStartParamKey = GetParameterKey(MediaPlayerComponentParameter.EnableAutoStart);
			ParamsDefinition.Add(
				enableAutoStartParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(enableAutoStartParamKey, legendSuffix)),
					false,
					BXCategory.PlaybackSettings
					)
			);

			string enableRepeatModeParamKey = GetParameterKey(MediaPlayerComponentParameter.EnableRepeatMode);
			ParamsDefinition.Add(
				enableRepeatModeParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(enableRepeatModeParamKey, legendSuffix)),
					false,
					BXCategory.PlaybackSettings
					)
			);

			string volumeLevelInPercentsParamKey = GetParameterKey(MediaPlayerComponentParameter.VolumeLevelInPercents);
			ParamsDefinition.Add(
				volumeLevelInPercentsParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(volumeLevelInPercentsParamKey, legendSuffix)),
					"90",
					BXCategory.PlaybackSettings
					)
			);

			BXCategory additionalSettingsFlv = new BXCategory(GetMessageRaw("Category.AdditionalSettingsFlv.Title"), "PLAYER_ADDITIONAL_SETTINGS_FLV", 151);

			string flvClientClickActionNameParamKey = GetParameterKey(MediaPlayerComponentParameter.FlvClientClickActionName);
			ParamsDefinition.Add(
				flvClientClickActionNameParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(flvClientClickActionNameParamKey, legendSuffix)),
					string.Empty,
					additionalSettingsFlv,
					null,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						flvClientClickActionNameParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

			string flvStartInMuteModeParamKey = GetParameterKey(MediaPlayerComponentParameter.FlvStartInMuteMode);
			ParamsDefinition.Add(
				flvStartInMuteModeParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(flvStartInMuteModeParamKey, legendSuffix)),
					false,
					additionalSettingsFlv,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						flvStartInMuteModeParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

			string flvEnableHighQualityParamKey = GetParameterKey(MediaPlayerComponentParameter.FlvEnableHighQuality);
			ParamsDefinition.Add(
				flvEnableHighQualityParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(flvEnableHighQualityParamKey, legendSuffix)),
					true,
					additionalSettingsFlv,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						flvEnableHighQualityParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

			/*string flvEnablePlayListShuffleModeParamKey = GetParameterKey(PlayerComponentParameter.FlvEnablePlayListShuffleMode);
			ParamsDefinition.Add(
				flvEnablePlayListShuffleModeParamKey,
				new BXParamYesNo(
					GetMessageRaw(string.Concat(flvEnablePlayListShuffleModeParamKey, legendSuffix)),
					false,
					additionalSettingsFlv,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						flvEnablePlayListShuffleModeParamKey, 
						new string[] { addvancedSettingsGroupName, playListSettingsGroupName },
						ParamClientSideActionGroupViewMemberDisplayCondition.And
						)
					)
				);

			string playListStartAtPositionParamKey = GetParameterKey(PlayerComponentParameter.PlayListStartAtPosition);
			ParamsDefinition.Add(
				playListStartAtPositionParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(playListStartAtPositionParamKey, legendSuffix)),
					"0",
					BXCategory.AdditionalSettings,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						playListStartAtPositionParamKey,
						new string[] { addvancedSettingsGroupName, playListSettingsGroupName },
						ParamClientSideActionGroupViewMemberDisplayCondition.And
						)
					)
			);*/

            //string enableAdvancedSettingsParamKey = GetParameterKey(MediaPlayerComponentParameter.EnableAdvancedSettings);
            //ParamsDefinition.Add(
            //    enableAdvancedSettingsParamKey,
            //    new BXParamYesNo(
            //        GetMessageRaw(string.Concat(enableAdvancedSettingsParamKey, legendSuffix)),
            //        false,
            //        BXCategory.AdditionalSettings,
            //        new ParamClientSideActionGroupViewSwitch(
            //            clientSideActionGroupViewId,
            //            enableAdvancedSettingsParamKey,
            //            addvancedSettingsGroupName,
            //            string.Empty
            //            )
            //        )
            //    );

			string bufferLengthInSecondsParamKey = GetParameterKey(MediaPlayerComponentParameter.BufferLengthInSeconds);
			ParamsDefinition.Add(
				bufferLengthInSecondsParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(bufferLengthInSecondsParamKey, legendSuffix)),
					"10",
					BXCategory.AdditionalSettings,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						bufferLengthInSecondsParamKey,
						new string[] { addvancedSettingsGroupName }
						)
					)
			);

            string enableDownloading = GetParameterKey(MediaPlayerComponentParameter.EnableDownloading);
            string groupDownloading = "downloading";
            ParamsDefinition.Add(
                enableDownloading,
                new BXParamYesNo(
                    GetMessageRaw(string.Concat(enableDownloading, legendSuffix)),
                    false,
                    BXCategory.AdditionalSettings,
                    new ParamClientSideActionGroupViewSwitch(ClientID, "PlayerEnableDownloading", groupDownloading, string.Empty)
                    )
            );

			string linkForDownloadSourceFileUrlParamKey = GetParameterKey(MediaPlayerComponentParameter.LinkForDownloadSourceFileUrl);
			ParamsDefinition.Add(
				linkForDownloadSourceFileUrlParamKey,
				new BXParamText(
					GetMessageRaw(string.Concat(linkForDownloadSourceFileUrlParamKey, legendSuffix)),
					string.Empty,
					BXCategory.AdditionalSettings,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						linkForDownloadSourceFileUrlParamKey,
                        new string[] { groupDownloading }
						)
					)
			);

			string linkForDownloadSourceFileTargetWindowParamKey = GetParameterKey(MediaPlayerComponentParameter.LinkForDownloadSourceFileTargetWindow);
			ParamsDefinition.Add(
				linkForDownloadSourceFileTargetWindowParamKey,
				new BXParamSingleSelection(
					GetMessageRaw(string.Concat(linkForDownloadSourceFileTargetWindowParamKey, legendSuffix)),
					"_self",
					BXCategory.AdditionalSettings,
					null,
					new ParamClientSideActionGroupViewMember(
						clientSideActionGroupViewId,
						linkForDownloadSourceFileTargetWindowParamKey,
                        new string[] { groupDownloading }
						)
					)
			);
		}

		protected override void LoadComponentDefinition()
		{
			#region PlayerType
			string[] playerTypeNameArr = Enum.GetNames(typeof(MediaPlayerComponentPlayerType));
			int playerTypeNamesCount = playerTypeNameArr != null ? playerTypeNameArr.Length : 0;
			if (playerTypeNamesCount > 0)
			{
				BXParamValue[] paramValueArr = new BXParamValue[playerTypeNamesCount];
				for (int i = 0; i < playerTypeNamesCount; i++)
				{
					string curPlayerTypeName = playerTypeNameArr[i];
					paramValueArr[i] = new BXParamValue(GetMessageRaw(string.Concat("PlayerTypeTitle", curPlayerTypeName)), curPlayerTypeName);
				}
				List<BXParamValue> playerTypeValues = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.PlayerType)].Values;
				if (playerTypeValues != null)
				{
					if (playerTypeValues.Count > 0)
						playerTypeValues.Clear();
					playerTypeValues.AddRange(paramValueArr);
				}
				else
					ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.PlayerType)].Values = new List<BXParamValue>(paramValueArr);
			}
			else
			{
				List<BXParamValue> playerTypeValues = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.PlayerType)].Values;
				if (playerTypeValues != null && playerTypeValues.Count > 0)
					playerTypeValues.Clear();
			}
			#endregion

			#region FlvSkinName
			string flvSkinFolderPath = Parameters.Get(GetParameterKey(MediaPlayerComponentParameter.FlvSkinFolderPath), GetFlvDefaultSkinFolderVirtualPath());
			List<BXParamValue> templateNameLst = new List<BXParamValue>();
			templateNameLst.Add(new BXParamValue(GetMessageRaw("ParamValueTitle.Standard"), "default"));
			try
			{
				if (!string.IsNullOrEmpty(flvSkinFolderPath))
				{
					if (!VirtualPathUtility.IsAppRelative(flvSkinFolderPath))
						flvSkinFolderPath = VirtualPathUtility.ToAppRelative(flvSkinFolderPath);
					string flvSkinFolderPhysicalPath = System.Web.Hosting.HostingEnvironment.MapPath(flvSkinFolderPath);
					string[] fileList = null;
					if (Directory.Exists(flvSkinFolderPhysicalPath))
						fileList = Directory.GetFiles(flvSkinFolderPhysicalPath, "*.swf", SearchOption.TopDirectoryOnly);
					int filesCount = fileList != null ? fileList.Length : 0;
					for (int i = 0; i < filesCount; i++)
					{
						FileInfo fi = new FileInfo(fileList[i]);

						string fileName = fi.Name;
						if (fileName.Length <= 4)
							continue;

						if (templateNameLst == null)
							templateNameLst = new List<BXParamValue>();

						templateNameLst.Add(new BXParamValue(fileName.Substring(0, fileName.Length - 4), fileName));
					}
				}
			}
			catch (Exception e)
			{
			}

			List<BXParamValue> flvSkinNameValues = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.FlvSkinName)].Values;
			if (flvSkinNameValues != null && flvSkinNameValues.Count > 0)
				flvSkinNameValues.Clear();

			if (templateNameLst != null && templateNameLst.Count > 0)
				flvSkinNameValues.AddRange(templateNameLst);
			#endregion

			#region FlvControlBarLocation
			IList<BXParamValue> flvCtrlBarLocVals = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.FlvControlBarLocation)].Values;
			flvCtrlBarLocVals.Add(new BXParamValue(GetMessageRaw("FlvControlBarLocationBottom"), "bottom"));
			flvCtrlBarLocVals.Add(new BXParamValue(GetMessageRaw("FlvControlBarLocationOver"), "over"));
			flvCtrlBarLocVals.Add(new BXParamValue(GetMessageRaw("FlvControlBarLocationNone"), "none"));
			#endregion

			#region FlvWMode
			IList<BXParamValue> flvWModeVals = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.FlvWMode)].Values;
			flvWModeVals.Add(new BXParamValue(GetMessageRaw("FlvWModeWindow"), "window"));
			flvWModeVals.Add(new BXParamValue(GetMessageRaw("FlvWModeOpaque"), "opaque"));
			flvWModeVals.Add(new BXParamValue(GetMessageRaw("FlvWModeTransparent"), "transparent"));
			#endregion

			#region WmvWMode
			IList<BXParamValue> wmvWModeVals = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.WmvWMode)].Values;
			wmvWModeVals.Add(new BXParamValue(GetMessageRaw("WmvWModeWindow"), "window"));
			wmvWModeVals.Add(new BXParamValue(GetMessageRaw("WmvWModeWindowless"), "windowless"));
			#endregion

			#region FlvClientClickActionName
			IList<BXParamValue> flvClientClickActionNameVals = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.FlvClientClickActionName)].Values;
			flvClientClickActionNameVals.Add(new BXParamValue(GetMessageRaw("FlvClientClickActionNamePlay"), "play"));
			flvClientClickActionNameVals.Add(new BXParamValue(GetMessageRaw("FlvClientClickActionNameLink"), "link"));
			flvClientClickActionNameVals.Add(new BXParamValue(GetMessageRaw("FlvClientClickActionNameFullScreen"), "fullscreen"));
			flvClientClickActionNameVals.Add(new BXParamValue(GetMessageRaw("FlvClientClickActionNameNone"), "none"));
			flvClientClickActionNameVals.Add(new BXParamValue(GetMessageRaw("FlvClientClickActionNameMute"), "mute"));
			#endregion

			#region Stretching

			string[] playerStretchingModeNameArr = Enum.GetNames(typeof(MediaPlayerStretchingMode));
			int playerStretchingModeNameArrCount = playerStretchingModeNameArr != null ? playerStretchingModeNameArr.Length : 0;
			if (playerStretchingModeNameArrCount > 0)
			{
				BXParamValue[] paramValueArr = new BXParamValue[playerStretchingModeNameArrCount];
				for (int i = 0; i < playerStretchingModeNameArrCount; i++)
				{
					string playerStretchingModeName = playerStretchingModeNameArr[i];
					paramValueArr[i] = new BXParamValue(GetMessageRaw(string.Concat("MediaPlayerStretchingMode", playerStretchingModeName)), playerStretchingModeName);
				}
				BXParam palyerStretchingModeParam = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.Stretching)];
				IList<BXParamValue> palyerStretchingModeParamVals = palyerStretchingModeParam.Values;
				if (palyerStretchingModeParamVals != null)
				{
					if (palyerStretchingModeParamVals.Count > 0)
						palyerStretchingModeParamVals.Clear();
					for (int j = 0; j < playerStretchingModeNameArrCount; j++)
						palyerStretchingModeParamVals.Add(paramValueArr[j]);
				}
				else
					palyerStretchingModeParam.Values = new List<BXParamValue>(palyerStretchingModeParamVals);
			}
			else
			{
				IList<BXParamValue> palyerStretchingModeParamVals = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.Stretching)].Values;
				if (palyerStretchingModeParamVals != null && palyerStretchingModeParamVals.Count > 0)
					palyerStretchingModeParamVals.Clear();
			}
			#endregion

			#region LinkForDownloadSourceFileTargetWindow
			IList<BXParamValue> linkForDownloadSourceFileTargetWindowVals = ParamsDefinition[GetParameterKey(MediaPlayerComponentParameter.LinkForDownloadSourceFileTargetWindow)].Values;
			linkForDownloadSourceFileTargetWindowVals.Add(new BXParamValue(GetMessageRaw("LinkForDownloadSourceFileTargetSelf"), "_self"));
			linkForDownloadSourceFileTargetWindowVals.Add(new BXParamValue(GetMessageRaw("LinkForDownloadSourceFileTargetBlank"), "_blank"));
			#endregion

		}
		#endregion


		/// <summary>
		/// Параметр компонента "Медиа проигрыватель"
		/// </summary>
		public enum MediaPlayerComponentParameter
		{
			/// <summary>
			/// Путь к файлу 
			/// [PATH]
			/// </summary>
			SourceFilePath = 1,

			/// <summary>
			/// Ширина
			/// [WIDTH]
			/// </summary>
			Width,

			/// <summary>
			/// Высота
			/// [HEIGHT]
			/// </summary>
			Height,

			/// <summary>
			/// Автоматически начать проигрывать
			/// [AUTOSTART]
			/// </summary>
			EnableAutoStart,

			/// <summary>
			/// Повторять композицию или список воспроизведения
			/// [REPEAT]
			/// </summary>
			EnableRepeatMode,

			/// <summary>
			/// Уровень громкости в процентах от максимального
			/// [VOLUME]
			/// </summary>
			VolumeLevelInPercents,

			/// <summary>
			/// Расширенный режим настройки компонента
			/// [ADVANCED_MODE_SETTINGS]
			/// </summary>
			EnableAdvancedSettings,

			/// <summary>
			/// Тип проигрывателя
			/// [PLAYER_TYPE]
			/// </summary>
			PlayerType,

			/// <summary>
			/// Использовать список воспроизведения
			/// [USE_PLAYLIST]
			/// </summary>
			//EnablePlayList,

			/// <summary>
			/// Путь к списку воспроизведения 
			/// [PATH]
			/// </summary>
			//PlayListFilePath,

			/// <summary>
			/// Путь к рисунку для предварительного просмотра
			/// [PREVIEW]
			/// </summary>
			PreviewImageFilePath,

			/// <summary>
			/// Путь к изображению авторского знака
			/// [LOGO]
			/// </summary>
			LogoImageFilePath,

			/// <summary>
			/// Разрешить полноэкранный режим
			/// [FULLSCREEN]
			/// </summary>
			EnableFullScreenModeSwitch,

			/// <summary>
			/// Растягивание
			/// </summary>
			Stretching,

			/// <summary>
			/// Цвет фона панели управления
			/// [CONTROLS_BGCOLOR]
			/// </summary>
			ControlPanelBackgroundColor,

			/// <summary>
			/// Цвет элементов управления
			/// [CONTROLS_COLOR]
			/// </summary>
			ControlsColor,

			/// <summary>
			/// Цвет элементов управления при наведении указателя мыши
			/// [CONTROLS_OVER_COLOR]
			/// </summary>
			ControlsOverColor,

			/// <summary>
			/// Цвет экрана
			/// [SCREEN_COLOR]
			/// </summary>
			ScreenColor,

			/// <summary>
			/// Путь к папке со темами
			/// [SKIN_PATH]        
			/// </summary>
			FlvSkinFolderPath,

			/// <summary>
			/// Тема
			/// [SKIN]        
			/// </summary>
			FlvSkinName,

			/// <summary>
			/// Расположение панели управления
			/// [CONTROLBAR]
			/// </summary>
			FlvControlBarLocation,

			/// <summary>
			/// Режим окна (WMode)
			/// [WMODE]
			/// </summary>
			FlvWMode,

			/// <summary>
			/// Скрыть контекстное меню проигрывателя
			/// [HIDE_MENU]
			/// </summary>
			FlvHideContextMenu,

			/// <summary>
			/// Показывать панель управления
			/// [SHOW_CONTROLS]
			/// </summary>
			WmvShowControlPanel,

			/// <summary>
			/// Показывать кнопку остановки ролика
			/// [SHOW_STOP]
			/// </summary>
			WmvShowStopButton,

			/// <summary>
			/// Показывать текущее время воспроизведения и оставшееся время
			/// [SHOW_DIGITS]
			/// </summary>
			WmvShowDigits,

			/// <summary>
			/// Режим окна
			/// [WMODE_WMV]
			/// обычный (standard)
			/// безоконный (windowless)
			/// </summary>
			WmvWMode,

			/// <summary>
			/// Расположение списка воспроизведения
			/// [PLAYLIST]
			/// </summary>
			WmvPlayListLocation,

			/// <summary>
			/// Размер списка воспроизведения в пикселях
			/// [PLAYLIST_SIZE]
			/// </summary>
			WmvPlayListSize,

			/// <summary>
			/// Формат списка воспроизведения
			/// [PLAYLIST_TYPE]
			/// </summary>
			WmvPlayListType,

			/// <summary>
			/// Ширина картинки в списке воспроизведения в пикселях
			/// [PLAYLIST_PREVIEW_WIDTH]
			/// </summary>
			WmvPlayListPreviewImageWidth,

			/// <summary>
			/// Высота картинки в списке воспроизведения в пикселях
			/// [PLAYLIST_PREVIEW_HEIGHT]
			/// </summary>
			WmvPlayListPreviewImageHeight,

			/// <summary>
			/// Действие при клике по экрану:
			/// [DISPLAY_CLICK]
			/// </summary>
			FlvClientClickActionName,

			/// <summary>
			/// Отключать звук при старте
			/// [MUTE]
			/// </summary>
			FlvStartInMuteMode,

			/// <summary>
			/// Высококачественное проигрывание
			/// [HIGH_QUALITY]
			/// </summary>
			FlvEnableHighQuality,

			/// <summary>
			/// Перемешать список воспроизведения
			/// [SHUFFLE]
			/// </summary>
			//FlvEnablePlayListShuffleMode,

			/// <summary>
			/// Начать проигрывать с элемента в списке
			/// [START_ITEM]
			/// </summary>
			PlayListStartAtPosition,

			/// <summary>
			/// Размер буфера в секундах
			/// [BUFFER_LENGTH]
			/// </summary>
			BufferLengthInSeconds,

            /// <summary>
            /// Разрешить скачивание
            /// </summary>
            EnableDownloading,

			/// <summary>
			/// Ссылка для скачивания ролика
			/// [DOWNLOAD_LINK]
			/// </summary>
			LinkForDownloadSourceFileUrl,

			/// <summary>
			/// Открывать ссылку в
			/// [DOWNLOAD_LINK_TARGET]
			/// </summary>
			LinkForDownloadSourceFileTargetWindow
		}


		/// <summary>
		/// Тип проигрывателя
		/// </summary>
		public enum MediaPlayerComponentPlayerType
		{
			/// <summary>
			/// Автоопределение
			/// В этом режиме запрещена установка плей листа
			/// </summary>
			Auto = 1,
			/// <summary>
			/// Adobe Flash Player
			/// </summary>
			Flv,
			/// <summary>
			/// Windows Media Player
			/// </summary>
			Wmv
		}


		/// <summary>
		/// Режим окна проигрывателя "Flash Player"
		/// </summary>
		public enum MediaPlayerComponentFlvWindowMode
		{
			/// <summary>
			/// Обычный (с собственным окном)
			/// </summary>
			Window = 1,
			/// <summary>
			/// Непрозрачный (без собственного окна)
			/// </summary>
			Opaque,
			/// <summary>
			/// Прозрачный (без собственного окна)
			/// </summary>
			Transparent
		}

		/// <summary>
		/// Режим растягивания медиа плеера
		/// </summary>
		public enum MediaPlayerStretchingMode
		{
			None = 0,
			Proportionally = 1,
			Disproportionally = 2,
			Fill = 3
		}
	}

	public abstract class MediaPlayerComponentTemplate : BXComponentTemplate<MediaPlayerComponent>
	{
        protected override void Render(HtmlTextWriter writer)
        {
            StartWidth = "100%";
            if (IsComponentDesignMode)
                writer.Write(GetMessage("ComponentTitle"));
            else
                base.Render(writer);
        }
	}
}

