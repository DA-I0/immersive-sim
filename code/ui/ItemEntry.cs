using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class ItemEntry : Control, Interfaces.IDraggableItem
	{
		private Label _itemName;
		private Label _itemCategory;
		private Label _itemSize;
		private Label _itemWeight;

		private BaseItem _sourceItem;
		private bool _isLifted = false;
		private bool _isSelected = false;

		private GameSystem _game;
		private CharacterInventoryWindow _inventoryUI;

		public override void _Ready()
		{
			_itemName = GetNode<Label>("ItemName");
			_itemCategory = GetNode<Label>("ItemCategory");
			_itemSize = GetNode<Label>("ItemSize");
			_itemWeight = GetNode<Label>("ItemWeight");

			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_inventoryUI = GetNode<CharacterInventoryWindow>($"{ProjectSettings.GetSetting("global/UIHandlerPath")}/CharacterInventory");
		}

		public override void _Input(InputEvent @event)
		{
			if (!_isSelected || _isLifted)
			{
				return;
			}

			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.ButtonIndex == MouseButton.Right)
				{
					_inventoryUI.ToggleItemInteractionMenu(_sourceItem, mouseEvent.Position);
				}

				if (mouseEvent.DoubleClick)
				{
					_sourceItem.UseItem(_game.Player.CharInventory);
					UpdateItemDetails();
					UpdateWindowInfo();
					return;
				}
			}
		}

		public override Variant _GetDragData(Vector2 atPosition)
		{
			ItemDrag dragItem = new ItemDrag(this, _sourceItem);
			SetDragPreview(dragItem);
			return dragItem;
		}

		public void SetTargetItem(BaseItem item)
		{
			_sourceItem = item;
			_sourceItem.ItemUpdated += UpdateItemDetails;
			UpdateItemDetails();
		}

		public void Destroy()
		{
			_sourceItem.ItemUpdated -= UpdateItemDetails;
			QueueFree();
		}

		private void UpdateItemDetails()
		{
			if (_sourceItem == null)
			{
				GD.PrintErr("ItemEntry:: item is empty");
				Destroy();
				return;
			}

			string usesLeft = string.Empty;

			if (_sourceItem is Consumable consumable)
			{
				usesLeft = $"[{consumable.FillingLeft}%]";
			}

			string stackAmount = (_sourceItem.StackCount > 1) ? $"({_sourceItem.StackCount})" : string.Empty;

			_itemName.Text = $"{_sourceItem.PublicName} {usesLeft} {stackAmount}";
			_itemCategory.Text = TranslationServer.Translate($"ITEM_TYPE_{_sourceItem.Category.ToString().ToUpper()}");
			_itemSize.Text = (_sourceItem.Size * _sourceItem.StackCount).ToString();
			_itemWeight.Text = $"{Statics.HelperMethods.RoundFloat(_sourceItem.Weight * _sourceItem.StackCount)}kg";
		}

		public void UpdateWindowInfo()
		{
			GetNode<ContainerWindow>("../../../..").UpdateContainerData();
		}

		private void ToggleSelection(bool select)
		{
			_isSelected = select;

			if (_isSelected)
			{
				_game.UI.ToggleItemDetails(_sourceItem);
			}
			else
			{
				_game.UI.ToggleItemDetails(null);
			}
		}
	}
}