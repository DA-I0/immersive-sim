using Godot;

namespace ImmersiveSim.UI.Settings
{
	public partial class AudioSettings : Control
	{
		private Label _masterVolumeText;
		private HSlider _masterVolume;
		private Label _musicVolumeText;
		private HSlider _musicVolume;
		private Label _effectsVolumeText;
		private HSlider _effectsVolume;

		private SettingsMenu _settingsMenu;

		public override void _Ready()
		{
			_masterVolumeText = GetNode<Label>("SettingsList/MasterVolume/Header");
			_masterVolume = GetNode<HSlider>("SettingsList/MasterVolume/Slider");
			_musicVolumeText = GetNode<Label>("SettingsList/MusicVolume/Header");
			_musicVolume = GetNode<HSlider>("SettingsList/MusicVolume/Slider");
			_effectsVolumeText = GetNode<Label>("SettingsList/EffectsVolume/Header");
			_effectsVolume = GetNode<HSlider>("SettingsList/EffectsVolume/Slider");

			_settingsMenu = GetParent().GetParent<SettingsMenu>();
		}

		internal void InitializeSettings()
		{
			UpdateTextValues();
		}

		internal void UpdateSettings()
		{
			_masterVolume.Value = _settingsMenu.Game.Settings.MasterVolume;
			_musicVolume.Value = _settingsMenu.Game.Settings.MusicVolume;
			_effectsVolume.Value = _settingsMenu.Game.Settings.EffectsVolume;
		}

		internal void ApplySettings()
		{
			_settingsMenu.Game.Settings.MasterVolume = (float)_masterVolume.Value;
			_settingsMenu.Game.Settings.MasterVolume = (float)_masterVolume.Value;
			_settingsMenu.Game.Settings.MusicVolume = (float)_musicVolume.Value;
			_settingsMenu.Game.Settings.EffectsVolume = (float)_effectsVolume.Value;
		}

		private void UpdateTextValues()
		{
			_masterVolumeText.Text = $"{TranslationServer.Translate("SETTING_VOLUME_MASTER")}: {System.MathF.Round((float)_masterVolume.Ratio * 100, 0)}%";
			_musicVolumeText.Text = $"{TranslationServer.Translate("SETTING_VOLUME_MUSIC")}: {System.MathF.Round((float)_musicVolume.Ratio * 100, 0)}%";
			_effectsVolumeText.Text = $"{TranslationServer.Translate("SETTING_VOLUME_EFFECTS")}: {System.MathF.Round((float)_effectsVolume.Ratio * 100, 0)}%";
			_settingsMenu.FlagSettingsAsUnmodified(false);
		}
	}
}