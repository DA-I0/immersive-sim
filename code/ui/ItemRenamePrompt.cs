using System;
using Godot;
using ImmersiveSim.Gameplay;


namespace ImmersiveSim.UI
{
	public partial class ItemRenamePrompt : Control
	{
		private LineEdit _newNameBox;
		private BaseItem _targetItem;

		public override void _Ready()
		{
			_newNameBox = GetNode<LineEdit>("ControlPanel/Controls/NameBox");
			ToggleRenamePrompt(null);
		}

		public override void _Input(InputEvent @event)
		{
			if (@event.IsActionPressed("ui_cancel") && Visible)
			{
				ToggleRenamePrompt(null);
			}
		}

		public void ToggleRenamePrompt(BaseItem targetItem)
		{
			_targetItem = targetItem;

			if (targetItem != null)
			{
				_newNameBox.Text = _targetItem.PublicName;
				Visible = true;
			}
			else
			{
				_newNameBox.Text = string.Empty;
				Visible = false;
			}
		}

		private void ConfirmNameChange()
		{
			_targetItem.Rename(_newNameBox.Text.StripEdges());
			ToggleRenamePrompt(null);
		}

		private void CancelNameChange()
		{
			ToggleRenamePrompt(null);
		}
	}
}