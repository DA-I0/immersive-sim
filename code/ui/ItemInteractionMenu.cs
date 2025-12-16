using Godot;
using Godot.Collections;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class ItemInteractionMenu : Control
	{
		private BaseItem _targetItem;
		private Array<Node> _options;
		private bool _isSelected = false;

		private UIHandler _ui;
		private Systems.GameSystem _game;
		private CharacterInventoryWindow _inventoryWindow;

		public override void _Ready()
		{
			_options = GetNode("InteractionList").GetChildren();
			_ui = GetNode<UIHandler>(ProjectSettings.GetSetting("global/UIHandlerPath").ToString());
			_inventoryWindow = GetParent<CharacterInventoryWindow>();
			_game = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			GetNode<Control>("InteractionList").Resized += RefreshMenuSize;
			ToggleInteractionMenu(null, Vector2.Zero);
		}
		public override void _Input(InputEvent @event)
		{
			if (!Visible || _isSelected)
			{
				return;
			}

			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.ButtonIndex == MouseButton.Left || mouseEvent.ButtonIndex == MouseButton.Right)
				{
					ToggleInteractionMenu(null, Vector2.Zero);
				}
			}
		}

		public void ToggleInteractionMenu(BaseItem targetItem, Vector2 position)
		{
			if (targetItem == null)
			{
				_targetItem = null;
				Visible = false;
			}
			else
			{
				_targetItem = targetItem;
				Position = position;
				ToggleAvailableInteractions();
				Visible = true;
			}
		}

		private void ToogleSelection(bool isMouseOnList)
		{
			_isSelected = isMouseOnList;
		}

		private void ToggleAvailableInteractions()
		{
			((Control)_options[1]).Visible = (_targetItem.Slot != EquipmentSlot.Selected) && !_game.Player.CharInventory.CheckIfEquipped(_targetItem, false);
			((Control)_options[2]).Visible = _game.Player.CharInventory.CheckIfEquipped(_targetItem, false);
			((Control)_options[3]).Visible = _targetItem.IsUsable;

			if (_targetItem is Gameplay.Container container)
			{
				((Control)_options[4]).Visible = !_inventoryWindow.CheckIfOpened(container);
				((Control)_options[5]).Visible = _inventoryWindow.CheckIfOpened(container);
			}
			else
			{
				((Control)_options[4]).Visible = false;
				((Control)_options[5]).Visible = false;
			}

			((Control)_options[6]).Visible = (_targetItem.IsStackable && _targetItem.StackCount > 1);
		}

		private void RefreshMenuSize()
		{
			Size = GetNode<Control>("InteractionList").Size;
			Position = HelperMethods.AdjustDisplayPosition(Position, Size);
		}

		// private Vector2 AdjustDisplayPosition(Vector2 objectPosition, Vector2 objectSize)
		// {
		// 	Vector2 adjustedPosition = Position;

		// 	float horizontalEndpoint = Position.X + Size.X;
		// 	float screenWidth = DisplayServer.ScreenGetSize().X;

		// 	if (horizontalEndpoint > screenWidth)
		// 	{
		// 		adjustedPosition.X = Position.X - (horizontalEndpoint - screenWidth);
		// 	}

		// 	float verticalEndpoint = Position.Y + Size.Y;
		// 	float screenHeight = DisplayServer.ScreenGetSize().Y;

		// 	if (verticalEndpoint > screenHeight)
		// 	{
		// 		adjustedPosition.Y = Position.Y - (verticalEndpoint - screenHeight);
		// 	}

		// 	return adjustedPosition;
		// }

		private void SelectItem()
		{
			_game.Player.CharInventory.SelectItem(_targetItem);
			ToggleInteractionMenu(null, Vector2.Zero);
		}

		private void EquipItem()
		{
			_game.Player.CharInventory.Equip(_targetItem);
			ToggleInteractionMenu(null, Vector2.Zero);
		}

		private void UnequipItem()
		{
			_game.Player.CharInventory.Unequip(_targetItem, null);
			ToggleInteractionMenu(null, Vector2.Zero);
		}

		private void UseItem()
		{
			_targetItem.UseItem(_game.Player.CharInventory);
			ToggleInteractionMenu(null, Vector2.Zero);
		}

		private void OpenItem()
		{
			_inventoryWindow.OpenContainer((Gameplay.Container)_targetItem);
			ToggleInteractionMenu(null, Vector2.Zero);
		}

		private void CloseItem()
		{
			_inventoryWindow.CloseContainer((Gameplay.Container)_targetItem);
			ToggleInteractionMenu(null, Vector2.Zero);
		}

		private void SplitItem()
		{
			_ui.OpenItemStackSplitPrompt(_targetItem);
			ToggleInteractionMenu(null, Vector2.Zero);
		}

		private void RenameItem()
		{
			_ui.OpenItemRenamePrompt(_targetItem);
			ToggleInteractionMenu(null, Vector2.Zero);
		}

		private void DropItem()
		{
			_game.Player.CharInventory.CheckIfEquipped(_targetItem, true);
			_game.Player.CharInventory.DropItem(_targetItem);
			ToggleInteractionMenu(null, Vector2.Zero);
		}
	}
}