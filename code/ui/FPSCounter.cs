using Godot;

namespace ImmersiveSim.UI
{
	public partial class FPSCounter : Control
	{
		private Label _content;

		private Systems.Settings _settings;

		public override void _Ready()
		{
			_content = GetNode<Label>("MarginContainer/Content");
			_settings = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).Settings;
			_settings.SettingsUpdated += ToggleVisibility;
		}

		public override void _Process(double delta)
		{
			_content.Text = $"FPS: {Engine.GetFramesPerSecond()}";
		}

		private void ToggleVisibility()
		{
			Visible = _settings.DisplayFrameCounter;
		}
	}
}
