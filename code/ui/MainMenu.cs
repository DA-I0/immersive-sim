using Godot;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class MainMenu : Control
	{
		private GameSystem _game;

		public override void _Ready()
		{
			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.UI.StateUpdated += ToggleActiveScreen;
			TreeExiting += UnsubscribeFromEvents;
		}

		private void ToggleActiveScreen(UIState newState)
		{
			GetChild<Control>(0).Visible = false;

			switch (newState)
			{
				case UIState.SaveLoad:
					break;

				case UIState.Settings:
					break;

				default:
					GetChild<Control>(0).Visible = true;
					break;
			}
		}

		private void StartGame()
		{
			_game.Level.ChangeLevel("city", "SpawnPoint1");
		}

		private void ShowLoadMenu()
		{
			_game.UI.SetUIState(UIState.SaveLoad);
		}

		private void ShowOptionsMenu()
		{
			_game.UI.SetUIState(UIState.Settings);
		}

		private void QuitGame()
		{
			_game.QuitGame();
		}

		private void UnsubscribeFromEvents()
		{
			_game.UI.StateUpdated -= ToggleActiveScreen;
			TreeExiting -= UnsubscribeFromEvents;
		}
	}
}
