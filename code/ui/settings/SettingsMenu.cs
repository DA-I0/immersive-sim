using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI.Settings
{
	internal partial class SettingsMenu : Control
	{
		[Export] private bool _inMenu = false;

		private GeneralSettings _generalSettings;
		private VideoSettings _videoSettings;
		private AudioSettings _audioSettings;
		private ControlSettings _controlSettings;

		private Button _confirm;
		private Button _return;

		private Systems.GameSystem _game;

		internal Systems.GameSystem Game
		{
			get { return _game; }
		}

		public override void _Ready()
		{
			FindControls();

			_game = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.UI.StateUpdated += ToggleVisibility;
			_game.Settings.SettingsUpdated += UpdateSettings;

			TreeExiting += UnsubscribeFromEvents;

			_generalSettings.InitializeSettings();
			_videoSettings.InitializeSettings();
			_controlSettings.InitializeSettings();
		}

		private void FindControls()
		{
			_generalSettings = GetNode<GeneralSettings>("SettingsContainer/HEADER_GENERAL");
			_videoSettings = GetNode<VideoSettings>("SettingsContainer/HEADER_VIDEO");
			_audioSettings = GetNode<AudioSettings>("SettingsContainer/HEADER_AUDIO");
			_controlSettings = GetNode<ControlSettings>("SettingsContainer/HEADER_CONTROLS");

			_confirm = GetNode<Button>("Buttons/Confirm");
			_return = GetNode<Button>("Buttons/Return");
		}

		private void ToggleVisibility(UIState newState)
		{
			if (_inMenu == _game.Level.IsInMenu)
			{
				if (newState == UIState.Settings)
				{
					Visible = true;
					UpdateSettings();
					FlagSettingsAsUnmodified(true);
					_controlSettings.UpdateOnToggle();
					GetChild<TabContainer>(0).CurrentTab = 0;
					return;
				}
			}

			Visible = false;
		}

		private void UpdateSettings()
		{
			_generalSettings.UpdateSettings();
			_videoSettings.UpdateSettings();
			_audioSettings.UpdateSettings();
			_controlSettings.UpdateSettings();
		}

		private void ApplySettings()
		{
			_generalSettings.ApplySettings();
			_videoSettings.ApplySettings();
			_audioSettings.ApplySettings();
			_controlSettings.ApplySettings();

			_game.Settings.SaveSettings();
			FlagSettingsAsUnmodified(true);
		}

		private void RestoreDefaultSettings()
		{
			_game.Settings.SetDefaultValues();
			UpdateSettings();
		}

		internal void Return()
		{
			if (!Visible)
			{
				return;
			}

			if (_inMenu)
			{
				_game.UI.SetUIState(UIState.Misc);
			}
			else
			{
				_game.UI.SetUIState(UIState.Pause);
			}
		}

		internal void FlagSettingsAsUnmodifiedAlt(int value)
		{
			FlagSettingsAsUnmodified(false);
		}

		internal void FlagSettingsAsUnmodified(bool disable)
		{
			_confirm.Disabled = disable;
		}

		private void UnsubscribeFromEvents()
		{
			_game.UI.StateUpdated -= ToggleVisibility;
			_game.Settings.SettingsUpdated -= UpdateSettings;
			TreeExiting -= UnsubscribeFromEvents;
		}
	}
}