using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI.Settings
{
	public partial class GeneralSettings : Control
	{
		private OptionButton _language;
		private OptionButton _font;
		private Label _fontSizeText;
		private HSlider _fontSize;
		private Label _crosshairScaleText;
		private HSlider _crosshairScale;
		private CheckButton _displayInteractionKeybinds;
		private CheckButton _displayItemInteractions;
		private Label _maxNotificationNumberText;
		private HSlider _maxNotificationNumber;
		private Label _notificationDisplayTimeText;
		private HSlider _notificationDisplayTime;
		private CheckButton _displayFrameCounter;
		private Label _streamingDistanceText;
		private HSlider _streamingDistance;

		private SettingsMenu _settingsMenu;

		public override void _Ready()
		{
			_language = GetNode<OptionButton>("SettingsList/Language/List");
			_font = GetNode<OptionButton>("SettingsList/Font/List");
			_fontSizeText = GetNode<Label>("SettingsList/FontSize/Header");
			_fontSize = GetNode<HSlider>("SettingsList/FontSize/Slider");
			_crosshairScaleText = GetNode<Label>("SettingsList/CrosshairScale/Header");
			_crosshairScale = GetNode<HSlider>("SettingsList/CrosshairScale/Slider");
			_displayInteractionKeybinds = GetNode<CheckButton>("SettingsList/DisplayInteractionKeybinds/CheckButton");
			_displayItemInteractions = GetNode<CheckButton>("SettingsList/DisplayItemInteractions/CheckButton");
			_maxNotificationNumberText = GetNode<Label>("SettingsList/MaxNotificationNumber/Header");
			_maxNotificationNumber = GetNode<HSlider>("SettingsList/MaxNotificationNumber/Slider");
			_notificationDisplayTimeText = GetNode<Label>("SettingsList/NotificationDisplayTime/Header");
			_notificationDisplayTime = GetNode<HSlider>("SettingsList/NotificationDisplayTime/Slider");
			_displayFrameCounter = GetNode<CheckButton>("SettingsList/FrameCounter/CheckButton");
			_streamingDistanceText = GetNode<Label>("SettingsList/StreamingDistance/Header");
			_streamingDistance = GetNode<HSlider>("SettingsList/StreamingDistance/Slider");

			_settingsMenu = GetParent().GetParent<SettingsMenu>();
		}

		internal void InitializeSettings()
		{
			PopulateLanguageList();
			PopulateFontList();
			UpdateTextValues();
		}

		internal void UpdateSettings()
		{
			_language.Selected = HelperMethods.FindOptionIndex(_language, HelperMethods.GetLocalizedLanguage(_settingsMenu.Game.Settings.Language));
			_font.Selected = _settingsMenu.Game.Settings.FontIndex;
			_fontSize.Value = _settingsMenu.Game.Settings.FontSize;
			_crosshairScale.Value = _settingsMenu.Game.Settings.CrosshairScale;
			_displayInteractionKeybinds.ButtonPressed = _settingsMenu.Game.Settings.DisplayInteractionKeybinds;
			_displayItemInteractions.ButtonPressed = _settingsMenu.Game.Settings.DisplayItemInteractions;
			_maxNotificationNumber.Value = _settingsMenu.Game.Settings.MaxNotificationNumber;
			_notificationDisplayTime.Value = _settingsMenu.Game.Settings.NotificationDisplayTime;
			_displayFrameCounter.ButtonPressed = _settingsMenu.Game.Settings.DisplayFrameCounter;
			_streamingDistance.Value = _settingsMenu.Game.Settings.StreamingDistance;
		}

		internal void ApplySettings()
		{
			_settingsMenu.Game.Settings.Language = TranslationServer.GetLoadedLocales()[_language.Selected];
			_settingsMenu.Game.Settings.FontIndex = _font.Selected;
			_settingsMenu.Game.Settings.FontSize = (int)_fontSize.Value;
			_settingsMenu.Game.Settings.CrosshairScale = (float)_crosshairScale.Value;
			_settingsMenu.Game.Settings.DisplayInteractionKeybinds = _displayInteractionKeybinds.ButtonPressed;
			_settingsMenu.Game.Settings.DisplayItemInteractions = _displayItemInteractions.ButtonPressed;
			_settingsMenu.Game.Settings.MaxNotificationNumber = (int)_maxNotificationNumber.Value;
			_settingsMenu.Game.Settings.NotificationDisplayTime = (float)_notificationDisplayTime.Value;
			_settingsMenu.Game.Settings.DisplayFrameCounter = _displayFrameCounter.ButtonPressed;
			_settingsMenu.Game.Settings.StreamingDistance = (int)_streamingDistance.Value;
		}

		private void UpdateTextValues()
		{
			_fontSizeText.Text = $"{TranslationServer.Translate("HEADER_FONT_SIZE")}: {(int)_fontSize.Value}";
			_crosshairScaleText.Text = $"{TranslationServer.Translate("HEADER_CROSSHAIR_SCALE")}: x{(float)_crosshairScale.Value}";
			_maxNotificationNumberText.Text = $"{TranslationServer.Translate("HEADER_MAX_NOTIFICATION_NUMBER")}: {(int)_maxNotificationNumber.Value}";
			_notificationDisplayTimeText.Text = $"{TranslationServer.Translate("HEADER_NOTIFICATION_DISPLAY_TIME")}: {(float)_notificationDisplayTime.Value}s";
			_streamingDistanceText.Text = $"{TranslationServer.Translate("HEADER_STREAMING_DISTANCE")}: {(int)_streamingDistance.Value}m";
			_settingsMenu.FlagSettingsAsUnmodified(false);
		}

		private void PopulateLanguageList()
		{
			System.Collections.Generic.List<string> languageNames = new System.Collections.Generic.List<string>();

			foreach (string languageCode in TranslationServer.GetLoadedLocales())
			{
				if (!languageNames.Contains(languageCode))
				{
					languageNames.Add(HelperMethods.GetLocalizedLanguage(languageCode));
				}
			}

			foreach (string languageName in languageNames)
			{
				_language.AddItem(languageName);
			}
		}

		private void PopulateFontList()
		{
			foreach (string font in _settingsMenu.Game.GameDatabase.Fonts)
			{
				int extentionIndex = font.LastIndexOf('.');
				_font.AddItem(font.Remove(extentionIndex));
			}
		}
	}
}