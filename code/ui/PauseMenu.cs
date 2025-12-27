using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class PauseMenu : Control
	{
		private UIManager _ui;

		public override void _Ready()
		{
			_ui = GetParent<UIManager>();
			_ui.StateUpdated += ActOnUIStateChange;
		}

		// public override void _Input(InputEvent @event)
		// {
		// 	if (@event.IsActionPressed("ui_cancel"))
		// 	{
		// 		ReturnToGame();
		// 	}
		// }

		private void ReturnToGame()
		{
			if (Visible)
			{
				_ui.SetUIState(UIState.None);
			}
		}

		private void ShowSaveMenu()
		{
			_ui.SetUIState(UIState.SaveLoad);
		}

		private void ShowOptionsMenu()
		{
			_ui.SetUIState(UIState.Settings);
		}

		private void ReturnToMenu()
		{
			_ui.Game.Level.LoadMenu();
		}

		private void QuitGame()
		{
			_ui.Game.QuitGame();
		}

		private void ActOnUIStateChange(UIState newState)
		{
			Visible = (newState == UIState.Pause);
			GetTree().Paused = ShouldPauseGame();
			// GetTree().Paused = Visible;
		}

		private bool ShouldPauseGame()
		{
			if (_ui.ActiveUIState == UIState.Pause)
			{
				return true;
			}

			if (_ui.ActiveUIState == UIState.Settings)
			{
				return true;
			}

			if (_ui.ActiveUIState == UIState.SaveLoad)
			{
				return true;
			}

			return false;
		}
	}
}