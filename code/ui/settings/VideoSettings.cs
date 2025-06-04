using Godot;

namespace ImmersiveSim.UI.Settings
{
	public partial class VideoSettings : Control
	{
		private CheckButton _fullscreen;
		private OptionButton _resolutionScalingMode;
		private Label _resolutionScaleText;
		private HSlider _resolutionScale;
		private CheckButton _limitFramerate;
		private Label _maxFPSText;
		private HSlider _maxFPS;
		private Label _fieldOfViewText;
		private HSlider _fieldOfView;
		private OptionButton _antiAliasing;
		private CheckButton _ssao;
		private CheckButton _sdfgi;

		private SettingsMenu _settingsMenu;

		public override void _Ready()
		{
			_fullscreen = GetNode<CheckButton>("SettingsList/FullScreen/CheckButton");
			_resolutionScalingMode = GetNode<OptionButton>("SettingsList/ResolutionScalingMode/List");
			_resolutionScaleText = GetNode<Label>("SettingsList/ResolutionScale/Header");
			_resolutionScale = GetNode<HSlider>("SettingsList/ResolutionScale/Slider");
			_limitFramerate = GetNode<CheckButton>("SettingsList/FrameLimit/CheckButton");
			_maxFPSText = GetNode<Label>("SettingsList/MaxFPS/Header");
			_maxFPS = GetNode<HSlider>("SettingsList/MaxFPS/Slider");
			_fieldOfViewText = GetNode<Label>("SettingsList/FoV/Header");
			_fieldOfView = GetNode<HSlider>("SettingsList/FoV/Slider");
			_antiAliasing = GetNode<OptionButton>("SettingsList/AntiAliasing/List");
			_ssao = GetNode<CheckButton>("SettingsList/SSAO/CheckButton");
			_sdfgi = GetNode<CheckButton>("SettingsList/SDFGI/CheckButton");

			_settingsMenu = GetParent().GetParent<SettingsMenu>();
		}

		internal void InitializeSettings()
		{
			ToggleFramelimitSlider();
			UpdateTextValues();
		}

		internal void UpdateSettings()
		{
			_fullscreen.ButtonPressed = (_settingsMenu.Game.Settings.ScreenMode > 0);
			_resolutionScalingMode.Selected = _settingsMenu.Game.Settings.ResolutionScalingMode;
			_resolutionScale.Value = _settingsMenu.Game.Settings.ResolutionScale;
			_limitFramerate.ButtonPressed = _settingsMenu.Game.Settings.LimitFrames;
			_maxFPS.Value = _settingsMenu.Game.Settings.MaxFPS;
			_fieldOfView.Value = _settingsMenu.Game.Settings.FieldOfView;
			_antiAliasing.Selected = _settingsMenu.Game.Settings.AntiAliasing;
			_ssao.ButtonPressed = _settingsMenu.Game.Settings.EnableSSAO;
			_sdfgi.ButtonPressed = _settingsMenu.Game.Settings.EnableSDFGI;
		}

		internal void ApplySettings()
		{
			_settingsMenu.Game.Settings.ScreenMode = _fullscreen.ButtonPressed ? 3 : 0;
			_settingsMenu.Game.Settings.ResolutionScalingMode = _resolutionScalingMode.Selected;
			_settingsMenu.Game.Settings.ResolutionScale = (float)_resolutionScale.Value;
			_settingsMenu.Game.Settings.LimitFrames = _limitFramerate.ButtonPressed;
			_settingsMenu.Game.Settings.MaxFPS = (int)_maxFPS.Value;
			_settingsMenu.Game.Settings.FieldOfView = (int)_fieldOfView.Value;
			_settingsMenu.Game.Settings.AntiAliasing = _antiAliasing.Selected;
			_settingsMenu.Game.Settings.EnableSSAO = _ssao.ButtonPressed;
			_settingsMenu.Game.Settings.EnableSDFGI = _sdfgi.ButtonPressed;
		}

		private void UpdateTextValues()
		{
			_resolutionScaleText.Text = $"{TranslationServer.Translate("SETTING_VIDEO_RESOLUTION_SCALE")}: x{(float)_resolutionScale.Value}";
			_maxFPSText.Text = $"{TranslationServer.Translate("SETTING_VIDEO_MAX_FPS")}: {(int)_maxFPS.Value}";
			_fieldOfViewText.Text = $"{TranslationServer.Translate("SETTING_VIDEO_FOV")}: {(int)_fieldOfView.Value}";
			_settingsMenu.FlagSettingsAsUnmodified(false);
		}

		private void AdjustScalingRange(int value)
		{
			if (value == 0)
			{
				_resolutionScale.MaxValue = 2f;
			}
			else
			{
				_resolutionScale.MaxValue = 1f;
			}

			_settingsMenu.FlagSettingsAsUnmodified(false);
		}

		private void ToggleFramelimitSlider()
		{
			_maxFPS.GetParent<Control>().Visible = _limitFramerate.ButtonPressed;
			_settingsMenu.FlagSettingsAsUnmodified(false);
		}
	}
}