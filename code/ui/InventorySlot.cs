using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class InventorySlot : Control, Interfaces.IDraggableItem
	{
		[Export] private EquipmentSlot _slot;
		[Export] private int _slotIndex;
		private bool _isSelected = false;

		private TextureRect _icon;

		private CharacterInventoryWindow _inventoryUI;
		// private Inventory _game.Player.CharInventory;
		private GameSystem _game;

		public override void _Ready()
		{
			_icon = GetNode<TextureRect>("Icon");
			_inventoryUI = GetNode<CharacterInventoryWindow>($"{ProjectSettings.GetSetting("global/UIHandlerPath")}/CharacterInventory");
			// string playerNodePath = $"{ProjectSettings.GetSetting("global/PlayerNodePath")}/CharacterEquipment";
			// _game.Player.CharInventory = GetNode<Inventory>(playerNodePath);
			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.NewPlayerSpawned += SetPlayerReferences;
			// UpdateWindowInfo();
		}

		private void SetPlayerReferences()
		{
			_game.Player.SetForDestruction += UnsetPlayerReferences;
			_game.Player.CharInventory.InventoryUpdated += UpdateWindowInfo;
			UpdateWindowInfo();
		}

		private void UnsetPlayerReferences()
		{
			_game.Player.CharInventory.InventoryUpdated -= UpdateWindowInfo;
		}

		public override void _Input(InputEvent @event)
		{
			if (!_isSelected || _game.Player.CharInventory._equipmentSlots[_slotIndex] == null)
			{
				return;
			}

			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.ButtonIndex == MouseButton.Right)
				{
					_inventoryUI.ToggleItemInteractionMenu(_game.Player.CharInventory._equipmentSlots[_slotIndex], mouseEvent.Position);
				}

				if (mouseEvent.DoubleClick)
				{
					_game.Player.CharInventory.Unequip(_game.Player.CharInventory._equipmentSlots[_slotIndex], null);
					UpdateWindowInfo();
					return;
				}
			}
		}

		public override Variant _GetDragData(Vector2 atPosition)
		{
			ItemDrag dragItem = new ItemDrag(this, _game.Player.CharInventory._equipmentSlots[_slotIndex]);
			SetDragPreview(dragItem);
			return dragItem;
		}

		public override bool _CanDropData(Vector2 atPosition, Variant data)
		{
			if ((ItemDrag)data != null)
			{
				return ((ItemDrag)data).Item.Slot == _slot;
			}

			return true;
		}

		public override void _DropData(Vector2 atPosition, Variant data)
		{
			if ((ItemDrag)data != null)
			{
				EquipItemToSlot(((ItemDrag)data).Item);
			}
		}

		public void UpdateWindowInfo()
		{
			string iconPath = (_game.Player.CharInventory._equipmentSlots[_slotIndex] != null) ? HelperMethods.ItemIconPath(_game.Player.CharInventory._equipmentSlots[_slotIndex]) : $"res://assets/textures/ui/icons/{_slot.ToString().ToLower()}.png";
			_icon.Texture = ResourceLoader.Load<Texture2D>(iconPath);
		}

		private void EquipItemToSlot(BaseItem item)
		{
			_game.Player.CharInventory.EquipTo(item, _slotIndex);
			UpdateWindowInfo();
		}

		private void ToggleSelection(bool select)
		{
			_isSelected = select;
		}
	}
}