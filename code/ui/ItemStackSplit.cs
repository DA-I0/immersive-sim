using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class ItemStackSplit : Control
	{
		private Label _header;
		private HSlider _splitSlider;
		private Label _oldStackValue;
		private Label _newStackValue;
		private int _splitAmount;

		private BaseItem _targetItem;

		private UIHandler _ui;
		// private GameSystem _game;

		public override void _Ready()
		{
			_header = GetNode<Label>("Window/ControlPositioner/Header");
			_splitSlider = GetNode<HSlider>("Window/ControlPositioner/SplitControls/SplitSlider");
			_oldStackValue = GetNode<Label>("Window/ControlPositioner/SplitControls/OldStackValue");
			_newStackValue = GetNode<Label>("Window/ControlPositioner/SplitControls/NewStackValue");

			_ui = GetParent<UIHandler>();
			// _game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			ToggleSplitPrompt(null);
		}

		public override void _Input(InputEvent @event)
		{
			if (_ui.LockKeybindings)
			{
				return;
			}
		}

		private void ConfirmSplit()
		{
			_targetItem.SplitStack((int)_splitSlider.Value);
			ToggleSplitPrompt(null);
		}

		private void CancelSplit()
		{
			ToggleSplitPrompt(null);
		}

		private void UpdateSplitAmount(float value)
		{
			_splitAmount = (int)value;
			_oldStackValue.Text = _splitAmount.ToString();
			_newStackValue.Text = (_targetItem.StackCount - _splitAmount).ToString();
		}

		public void ToggleSplitPrompt(BaseItem targetItem)
		{
			_targetItem = targetItem;

			if (_targetItem != null)
			{
				Visible = true;
				_header.Text = $"{TranslationServer.Translate($"HEADER_ITEM_SPLIT")} - {TranslationServer.Translate($"ITEM_{_targetItem.TemplateID.ToUpper()}")}";
				_splitSlider.Value = 1f;
				_splitSlider.MaxValue = _targetItem.StackCount - 1;
				_splitAmount = 1;
				UpdateSplitAmount((float)_splitSlider.Value);
			}
			else
			{
				Visible = false;
			}
		}
	}
}