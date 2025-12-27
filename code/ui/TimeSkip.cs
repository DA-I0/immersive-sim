using Godot;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class TimeSkip : Control
	{
		private HSlider _skipDuration;
		private Label _targetDate;
		private Button _confirmationButton;

		private UIManager _ui;
		private GameSystem _game;

		public override void _Ready()
		{
			_skipDuration = GetNode<HSlider>("Window/ControlPositioner/SkipDuration");
			_targetDate = GetNode<Label>("Window/ControlPositioner/TargetDate");
			_confirmationButton = GetNode<Button>("Window/ControlPositioner/ButtonPositioner/Confirm");
			_ui = GetParent<UIManager>();
			_ui.StateUpdated += ToggleWindow;
			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
		}

		public override void _Input(InputEvent @event)
		{
			if (_ui.LockKeybindings)
			{
				return;
			}

			if (@event.IsActionPressed("action_time_skip"))
			{
				_ui.SetUIState(UIState.TimeSkip);
			}

			if (@event.IsActionPressed("ui_cancel") && Visible)
			{
				CancelTimeSkip();
			}
		}

		private void ConfirmTimeSkip()
		{
			_game.Time.AdvanceTime((int)_skipDuration.Value, 0);

			if (_game.Player.CharMovement.ActiveStance == Stance.Prone)
			{
				_game.Player.CharStatus.Rest(_game.Time.CurrentDate);
			}

			if (_game.Player.CharMovement.ActiveStance == Stance.Sitting)
			{
				_game.Player.CharStatus.Rest(_game.Time.CurrentDate.AddHours(-(int)_skipDuration.Value / 2));
			}

			_ui.SetUIState(UIState.None);
		}

		private void CancelTimeSkip()
		{
			_ui.SetUIState(UIState.None);
		}

		private void UpdateTimeSkipDuration(float value)
		{
			System.DateTime newDate = _game.Time.CurrentDate.AddHours((int)value);
			_targetDate.Text = HelperMethods.GetFormattedTime(newDate);
		}

		private void ToggleWindow(UIState currentState)
		{
			if (currentState != UIState.TimeSkip)
			{
				Visible = false;
				return;
			}

			switch (_game.Player.CharMovement.ActiveStance)
			{
				case Stance.Prone:
					_skipDuration.MaxValue = 24;
					_confirmationButton.Text = TranslationServer.Translate("BUTTON_REST");
					break;

				case Stance.Sitting:
					_skipDuration.MaxValue = 24;
					_confirmationButton.Text = TranslationServer.Translate("BUTTON_REST");
					break;

				default:
					_skipDuration.MaxValue = 12;
					_confirmationButton.Text = TranslationServer.Translate("BUTTON_WAIT");
					break;
			}

			_skipDuration.Value = 1f;
			UpdateTimeSkipDuration((float)_skipDuration.Value);
			Visible = true;
		}
	}
}