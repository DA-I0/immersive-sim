using System.Collections.Generic;
using System.Linq;
using Godot;
using ImmersiveSim.Statics;

public enum InputType { Joypad, Keyboard, Mouse };

namespace ImmersiveSim.Systems
{
	public class Settings
	{
		private bool _firstLaunch = false;

		private ConfigFile _defaultConfig;
		private ConfigFile _config;

		private Dictionary<string, Godot.Collections.Array<InputEvent>> DefaultKeybindings = new Dictionary<string, Godot.Collections.Array<InputEvent>>();
		private InputType _activeInputType = InputType.Keyboard;
		private int _activeControllerId = -1;

		private readonly GameSystem _game;

		public event StatusUpdate SettingsUpdated;
		public event StatusUpdate ActiveInputTypeChanged;

		public bool FirstLaunch
		{
			get { return _firstLaunch; }
		}

		// Default settings
		public string DefaultLanguage
		{
			get { return (string)_defaultConfig.GetValue("general", "language"); }
		}

		public int DefaultFont
		{
			get { return (int)_defaultConfig.GetValue("general", "font"); }
		}

		public int DefaultFontSize
		{
			get { return (int)_defaultConfig.GetValue("general", "font_size"); }
		}

		public float DefaultCrosshairScale
		{
			get { return (float)_defaultConfig.GetValue("general", "crosshair_scale"); }
		}

		public bool DefaultDisplayInteractionKeybinds
		{
			get { return (bool)_defaultConfig.GetValue("general", "display_interaction_keybinds"); }
		}

		public bool DefaultDisplayItemInteractions
		{
			get { return (bool)_defaultConfig.GetValue("general", "display_item_interactions"); }
		}

		public string DefaultControlerPrompts
		{
			get { return (string)_defaultConfig.GetValue("general", "controler_prompts"); }
		}

		public int DefaultMaxNotificationNumber
		{
			get { return (int)_defaultConfig.GetValue("general", "max_notification_number"); }
		}

		public float DefaultNotificationDisplayTime
		{
			get { return (float)_defaultConfig.GetValue("general", "notification_display_time"); }
		}

		public bool DefaultDisplayFrameCounter
		{
			get { return (bool)_defaultConfig.GetValue("general", "display_frame_counter"); }
		}

		public int DefaultStreamingDistance
		{
			get { return (int)_defaultConfig.GetValue("general", "streaming_distance"); }
		}

		public int DefaultScreenMode
		{
			get { return (int)_defaultConfig.GetValue("display", "screen_mode"); }
		}

		public int DefaultResolutionScalingMode
		{
			get { return (int)_defaultConfig.GetValue("display", "resolution_scaling_mode"); }
		}

		public float DefaultResolutionScale
		{
			get { return (float)_defaultConfig.GetValue("display", "resolution_scale"); }
		}

		public bool DefaultLimitFrames
		{
			get { return (bool)_defaultConfig.GetValue("display", "limit_frames"); }
		}

		public static int DefaultMaxFPS
		{
			get { return (int)DisplayServer.ScreenGetRefreshRate((int)DisplayServer.ScreenOfMainWindow) + 1; }
		}

		public int DefaultFieldOfView
		{
			get { return (int)_defaultConfig.GetValue("display", "field_of_view"); }
		}

		public int DefaultAntiAliasing
		{
			get { return (int)_defaultConfig.GetValue("display", "anti-aliasing"); }
		}

		public bool DefaultSSAO
		{
			get { return (bool)_defaultConfig.GetValue("display", "enable_ssao"); }
		}

		public bool DefaultSDFGI
		{
			get { return (bool)_defaultConfig.GetValue("display", "enable_sdfgi"); }
		}

		public float DefaultMasterVolume
		{
			get { return (float)_defaultConfig.GetValue("audio", "master_volume"); }
		}

		public float DefaultMusicVolume
		{
			get { return (float)_defaultConfig.GetValue("audio", "music_volume"); }
		}

		public float DefaultEffectsVolume
		{
			get { return (float)_defaultConfig.GetValue("audio", "effects_volume"); }
		}

		public float DefaultCameraSensitivity
		{
			get { return (float)_defaultConfig.GetValue("controls", "camera_sensitivity"); }
		}

		public int DefaultCameraVerticalDirection
		{
			get { return (int)_defaultConfig.GetValue("controls", "camera_vertical_direction"); }
		}

		public float DefaultStanceSensitivity
		{
			get { return (float)_defaultConfig.GetValue("controls", "stance_sensitivity"); }
		}

		public float DefaultLeanSensitivity
		{
			get { return (float)_defaultConfig.GetValue("controls", "lean_sensitivity"); }
		}

		public bool DefaultControllerVibrations
		{
			get { return (bool)_defaultConfig.GetValue("controls", "controller_vibrations"); }
		}

		public int DefaultInputType
		{
			get { return (int)_defaultConfig.GetValue("controls", "input_type"); }
		}

		// Custom setting
		public string Language
		{
			get { return (string)_config.GetValue("general", "language", DefaultLanguage); }
			set { _config.SetValue("general", "language", value); }
		}

		public int FontIndex
		{
			get { return (int)_config.GetValue("general", "font", DefaultFont); }
			set { _config.SetValue("general", "font", value); }
		}

		public int FontSize
		{
			get { return (int)_config.GetValue("general", "font_size", DefaultFontSize); }
			set { _config.SetValue("general", "font_size", value); }
		}

		public float CrosshairScale
		{
			get { return (float)_config.GetValue("general", "crosshair_scale", DefaultCrosshairScale); }
			set { _config.SetValue("general", "crosshair_scale", value); }
		}

		public bool DisplayInteractionKeybinds
		{
			get { return (bool)_config.GetValue("display", "display_interaction_keybinds", DefaultDisplayInteractionKeybinds); }
			set { _config.SetValue("display", "display_interaction_keybinds", value); }
		}

		public bool DisplayItemInteractions
		{
			get { return (bool)_config.GetValue("display", "display_item_interactions", DefaultDisplayItemInteractions); }
			set { _config.SetValue("display", "display_item_interactions", value); }
		}

		public string ControlerPrompts
		{
			get { return (string)_config.GetValue("general", "controler_prompts", DefaultControlerPrompts); }
			set { _config.SetValue("general", "controler_prompts", value); }
		}

		public int MaxNotificationNumber
		{
			get { return (int)_config.GetValue("general", "max_notification_number", DefaultMaxNotificationNumber); }
			set { _config.SetValue("general", "max_notification_number", value); }
		}

		public float NotificationDisplayTime
		{
			get { return (float)_config.GetValue("general", "notification_display_time", DefaultNotificationDisplayTime); }
			set { _config.SetValue("general", "notification_display_time", value); }
		}

		public bool DisplayFrameCounter
		{
			get { return (bool)_config.GetValue("display", "display_frame_counter", DefaultDisplayFrameCounter); }
			set { _config.SetValue("display", "display_frame_counter", value); }
		}

		public int StreamingDistance
		{
			get { return (int)_config.GetValue("general", "streaming_distance", DefaultStreamingDistance); }
			set { _config.SetValue("general", "streaming_distance", value); }
		}

		public int ScreenMode
		{
			get { return (int)_config.GetValue("display", "screen_mode", DefaultScreenMode); }
			set { _config.SetValue("display", "screen_mode", value); }
		}

		public int ResolutionScalingMode
		{
			get { return (int)_config.GetValue("display", "resolution_scaling_mode", DefaultResolutionScalingMode); }
			set { _config.SetValue("display", "resolution_scaling_mode", value); }
		}

		public float ResolutionScale
		{
			get { return (float)_config.GetValue("display", "resolution_scale", DefaultResolutionScale); }
			set { _config.SetValue("display", "resolution_scale", value); }
		}

		public bool LimitFrames
		{
			get { return (bool)_config.GetValue("display", "limit_frames", DefaultLimitFrames); }
			set { _config.SetValue("display", "limit_frames", value); }
		}

		public int MaxFPS
		{
			get { return (int)_config.GetValue("display", "max_fps", DefaultMaxFPS); }
			set { _config.SetValue("display", "max_fps", value); }
		}

		public int FieldOfView
		{
			get { return (int)_config.GetValue("display", "field_of_view", DefaultFieldOfView); }
			set { _config.SetValue("display", "field_of_view", value); }
		}

		public int AntiAliasing
		{
			get { return (int)_config.GetValue("display", "anti-aliasing", DefaultAntiAliasing); }
			set { _config.SetValue("display", "anti-aliasing", value); }
		}

		public bool EnableSSAO
		{
			get { return (bool)_config.GetValue("display", "enable_ssao", DefaultSSAO); }
			set { _config.SetValue("display", "enable_ssao", value); }
		}

		public bool EnableSDFGI
		{
			get { return (bool)_config.GetValue("display", "enable_sdfgi", DefaultSDFGI); }
			set { _config.SetValue("display", "enable_sdfgi", value); }
		}

		public float MasterVolume
		{
			get { return (float)_config.GetValue("audio", "master_volume", DefaultMasterVolume); }
			set { _config.SetValue("audio", "master_volume", value); }
		}

		public float MusicVolume
		{
			get { return (float)_config.GetValue("audio", "music_volume", DefaultMusicVolume); }
			set { _config.SetValue("audio", "music_volume", value); }
		}

		public float EffectsVolume
		{
			get { return (float)_config.GetValue("audio", "effects_volume", DefaultEffectsVolume); }
			set { _config.SetValue("audio", "effects_volume", value); }
		}

		public float CameraSensitivity
		{
			get { return (float)_config.GetValue("controls", "camera_sensitivity", DefaultCameraSensitivity); }
			set { _config.SetValue("controls", "camera_sensitivity", value); }
		}

		public int CameraVerticalDirection
		{
			get { return (int)_config.GetValue("controls", "camera_vertical_direction", DefaultCameraVerticalDirection); }
			set { _config.SetValue("controls", "camera_vertical_direction", value); }
		}

		public float StanceSensitivity
		{
			get { return (float)_config.GetValue("controls", "stance_sensitivity", DefaultStanceSensitivity); }
			set { _config.SetValue("controls", "stance_sensitivity", value); }
		}

		public float LeanSensitivity
		{
			get { return (float)_config.GetValue("controls", "lean_sensitivity", DefaultLeanSensitivity); }
			set { _config.SetValue("controls", "lean_sensitivity", value); }
		}

		public bool ControllerVibrations
		{
			get { return (bool)_config.GetValue("controls", "controller_vibrations", DefaultControllerVibrations); }
			set { _config.SetValue("controls", "controller_vibrations", value); }
		}

		public InputType ActiveInputType
		{
			get { return _activeInputType; }
			set
			{
				if (_activeInputType != value)
				{
					_activeInputType = value;
					_config.SetValue("controls", "input_type", (int)value);
					ActiveInputTypeChanged?.Invoke();
				}
			}
		}

		public string ActiveController
		{
			get { return (string)_config.GetValue("controls", "active_controller", string.Empty); }
			set { _config.SetValue("controls", "active_controller", value); }
		}

		public int ActiveControllerID
		{
			get { return _activeControllerId; }
			set { _activeControllerId = value; }
		}

		public Settings(GameSystem game)
		{
			_game = game;
		}

		public void InitializeSettings()
		{
			PrepareDefaultKeybindings();
			LoadDefaultSettings();
			LoadSettings();
		}

		private void PrepareDefaultKeybindings()
		{
			var gameplayInputActions = InputMap.GetActions().Where(a => !a.ToString().StartsWith("ui_"));

			foreach (string action in gameplayInputActions)
			{
				var inputEvents = InputMap.ActionGetEvents(action);

				if (inputEvents.Count > 0)
				{
					DefaultKeybindings[action] = inputEvents;
				}
			}
		}

		public void SetDefaultValues()
		{
			SetDefaultGeneralValues();
			SetDefaultVideoValues();
			SetDefaultAudioValues();
			SetDefaultControlValues();
		}

		public void SetDefaultGeneralValues()
		{
			Language = DefaultLanguage;
			FontIndex = DefaultFont;
			FontSize = DefaultFontSize;
			DisplayInteractionKeybinds = DefaultDisplayInteractionKeybinds;
			DisplayItemInteractions = DefaultDisplayItemInteractions;
			ActiveInputType = (InputType)DefaultInputType;
			ControlerPrompts = DefaultControlerPrompts;
			MaxNotificationNumber = DefaultMaxNotificationNumber;
			NotificationDisplayTime = DefaultNotificationDisplayTime;
			DisplayFrameCounter = DefaultDisplayFrameCounter;
			StreamingDistance = DefaultStreamingDistance;
		}

		public void SetDefaultVideoValues()
		{
			ScreenMode = DefaultScreenMode;
			ResolutionScalingMode = DefaultResolutionScalingMode;
			ResolutionScale = DefaultResolutionScale;
			LimitFrames = DefaultLimitFrames;
			MaxFPS = DefaultMaxFPS;
			FieldOfView = DefaultFieldOfView;
			AntiAliasing = DefaultAntiAliasing;
			EnableSSAO = DefaultSSAO;
			EnableSDFGI = DefaultSDFGI;
		}

		public void SetDefaultAudioValues()
		{
			MasterVolume = DefaultMasterVolume;
			MusicVolume = DefaultMusicVolume;
			EffectsVolume = DefaultEffectsVolume;
		}

		public void SetDefaultControlValues()
		{
			CameraSensitivity = DefaultCameraSensitivity;
			CameraVerticalDirection = DefaultCameraVerticalDirection;
			StanceSensitivity = DefaultStanceSensitivity;
			LeanSensitivity = DefaultLeanSensitivity;
			ActiveController = string.Empty;
			SetDefaultKeybindings();
		}

		private void SetDefaultKeybindings()
		{
			var gameplayInputActions = InputMap.GetActions().Where(a => !a.ToString().StartsWith("ui_"));

			foreach (string action in DefaultKeybindings.Keys)
			{
				var inputEvents = InputMap.ActionGetEvents(action);

				InputMap.ActionEraseEvents(action);

				foreach (InputEvent inputEvent in DefaultKeybindings[action])
				{
					InputMap.ActionAddEvent(action, inputEvent);
				}
			}

			SaveKeybindings();
		}

		private void LoadDefaultSettings()
		{
			_defaultConfig = new ConfigFile();
			Error error = _defaultConfig.Load(ProjectSettings.GetSetting("global/DefaultConfigFilePath").ToString());

			if (error != Error.Ok)
			{
				GD.PrintErr("Failed to read default config file.");
			}
		}

		public void LoadSettings()
		{
			_config = new ConfigFile();
			Error error = _config.Load(ProjectSettings.GetSetting("global/ConfigFilePath").ToString());

			if (error != Error.Ok)
			{
				SetDefaultValues();
				_firstLaunch = true;
			}

			ApplySettings();
		}

		public void SaveSettings()
		{
			_config.Save(ProjectSettings.GetSetting("global/ConfigFilePath").ToString());
			ApplySettings();
			_firstLaunch = false;
		}

		public void ChangeKeybinding(string actionToChange, string inputValueToChange, InputEvent newInputValue)
		{
			if (inputValueToChange != string.Empty)
			{
				string oldInputValue = _game.GameDatabase.GetInputKey(inputValueToChange);
				var inputEvents = InputMap.ActionGetEvents(actionToChange).Where(a => a.AsText().Contains(oldInputValue));

				InputMap.ActionEraseEvent(actionToChange, inputEvents.ElementAt(0));
			}

			InputMap.ActionAddEvent(actionToChange, newInputValue);
			SaveKeybindings();
		}

		private void SaveKeybindings()
		{
			var gameplayInputActions = InputMap.GetActions().Where(a => !a.ToString().StartsWith("ui_"));

			foreach (string action in gameplayInputActions)
			{
				var inputEvents = InputMap.ActionGetEvents(action);
				_config.SetValue("controls", action, inputEvents);
			}
		}

		private void ApplyKeybindings()
		{
			var gameplayInputActions = InputMap.GetActions().Where(a => !a.ToString().StartsWith("ui_"));

			foreach (string action in gameplayInputActions)
			{
				Godot.Collections.Array<InputEvent> customKeybinds = (Godot.Collections.Array<InputEvent>)_config.GetValue("controls", action);
				InputMap.ActionEraseEvents(action);

				foreach (InputEvent inputEvent in customKeybinds)
				{
					InputMap.ActionAddEvent(action, inputEvent);
				}
			}
		}

		private void ApplySettings()
		{
			TranslationServer.SetLocale(Language);
			ApplyFont();

			DisplayServer.WindowSetMode((DisplayServer.WindowMode)ScreenMode, 0);
			ApplyResolutionScaling();
			Engine.MaxFps = LimitFrames ? MaxFPS : 0;
			ApplyAASettings();
			_game.World.Environment.SsaoEnabled = EnableSSAO;
			_game.World.Environment.SdfgiEnabled = EnableSDFGI;

			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), MasterVolume);
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), MusicVolume);
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Effects"), EffectsVolume);

			ActiveControllerID = (ActiveController != string.Empty) ? FindConnectedController() : -1;
			ApplyKeybindings();
			SettingsUpdated?.Invoke();
		}

		private void ApplyResolutionScaling()
		{
			switch (ResolutionScalingMode)
			{
				case 1:
					_game.GetViewport().Scaling3DMode = Viewport.Scaling3DModeEnum.Fsr;
					break;

				case 2:
					_game.GetViewport().Scaling3DMode = Viewport.Scaling3DModeEnum.Fsr2;
					break;

				default:
					_game.GetViewport().Scaling3DMode = Viewport.Scaling3DModeEnum.Bilinear;
					break;
			}

			_game.GetViewport().Scaling3DScale = ResolutionScale;
		}

		private void DisableAA()
		{
			_game.GetViewport().Msaa3D = Viewport.Msaa.Disabled;
			_game.GetViewport().UseTaa = false;
			_game.GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Disabled;
		}

		private void ApplyAASettings()
		{
			DisableAA();

			switch (AntiAliasing)
			{
				case 1:
					_game.GetViewport().Msaa3D = Viewport.Msaa.Msaa2X;
					break;

				case 2:
					_game.GetViewport().Msaa3D = Viewport.Msaa.Msaa4X;
					break;

				case 3:
					_game.GetViewport().Msaa3D = Viewport.Msaa.Msaa8X;
					break;

				case 4:
					_game.GetViewport().UseTaa = true;
					break;

				case 5:
					_game.GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Fxaa;
					break;

				default:
					break;
			}
		}

		private int FindConnectedController()
		{
			foreach (int index in Input.GetConnectedJoypads())
			{
				if (Input.GetJoyName(index) == ActiveController)
				{
					return index;
				}
			}

			return -1;
		}

		private void ApplyFont()
		{
			Theme currentTheme = ThemeDB.GetProjectTheme();
			currentTheme.DefaultFont = ResourceLoader.Load<Font>($"res://assets/fonts/{_game.GameDatabase.Fonts[FontIndex]}");
			currentTheme.DefaultFontSize = FontSize;
		}
	}
}