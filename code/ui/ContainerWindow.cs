using System.Collections.Generic;
using System.Linq;
using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class ContainerWindow : Control
	{
		public bool isTemporary = true;
		[Export] private PackedScene _itemInfoPrefab;

		private Label _containerName;
		private Label _containerSpace;
		private Label _containerWeight;
		private Control _itemList;

		private int orderType = -1;

		private Gameplay.Container _sourceContainer;

		private GameSystem _game;

		public Gameplay.Container SourceContainer
		{
			get { return _sourceContainer; }
		}

		public override void _Ready()
		{
			_containerName = GetNode<Label>("Panel/VerticalAligner/HeaderContainer/ContainerName");
			_containerSpace = GetNode<Label>("Panel/VerticalAligner/HeaderContainer/ContainerSpace");
			_containerWeight = GetNode<Label>("Panel/VerticalAligner/HeaderContainer/ContainerWeight");
			_itemList = GetNode<Control>("Panel/VerticalAligner/ItemList");

			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());

			// GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).ItemSortChanged += UpdateItemList; // TODO: fix when implemented player settings
			_game.ItemSortChanged += UpdateItemList; // TODO: fix when implemented player settings
		}

		public override bool _CanDropData(Vector2 atPosition, Variant data)
		{
			if ((ItemDrag)data != null)
			{
				if (((ItemDrag)data).Item != _sourceContainer)
				{
					return _sourceContainer.Capacity >= _sourceContainer.UsedSpace + ((ItemDrag)data).Item.Size;
				}
			}

			return false;
		}

		public override void _DropData(Vector2 atPosition, Variant data)
		{
			if ((ItemDrag)data != null)
			{
				// string playerNodePath = $"{ProjectSettings.GetSetting("global/PlayerNodePath")}/CharacterEquipment";
				_game.Player.CharInventory.Unequip(((ItemDrag)data).Item, _sourceContainer);
				UpdateContainerData();
			}
		}

		public void OpenContainer(Gameplay.Container target)
		{
			_sourceContainer = target;
			_sourceContainer.InventoryUpdated += UpdateContainerData;
			UpdateContainerData();
		}

		public void CloseContainer()
		{
			_sourceContainer.InventoryUpdated -= UpdateContainerData;
			ClearItemList();
			GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).ItemSortChanged -= UpdateItemList;
			QueueFree();
		}

		public void UpdateContainerData()
		{
			_containerName.Text = _sourceContainer.PublicName;
			_containerSpace.Text = $"{TranslationServer.Translate("HEADER_CAPACITY")}: {_sourceContainer.UsedSpace}/{_sourceContainer.Capacity}";
			_containerWeight.Text = $"{TranslationServer.Translate("HEADER_WEIGHT")}: {_sourceContainer.Weight}kg";
			UpdateItemList();
		}

		private void UpdateItemList()
		{
			ClearItemList();

			foreach (BaseItem item in PrepareSortedItemList())
			{
				ItemEntry itemEntry = _itemInfoPrefab.Instantiate<ItemEntry>();
				_itemList.AddChild(itemEntry);
				itemEntry.SetTargetItem(item);
			}
		}

		private void ClearItemList()
		{
			foreach (Node itemEntry in _itemList.GetChildren())
			{
				((ItemEntry)itemEntry).Destroy();
			}
		}

		private void SetOrder(int order)
		{
			// -1 - default (type, name)
			// 0 - name
			// 1 - space
			// 2 - weight
			GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).ItemListSortOrder = order; // TODO: get from game settings
			UpdateItemList();
		}

		private List<BaseItem> PrepareSortedItemList()
		{
			switch (GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).ItemListSortOrder) // TODO: get from game settings
			{
				case 0:
					return _sourceContainer.ItemsWithin.OrderBy(i => i.TemplateID).ToList();

				case 1:
					return _sourceContainer.ItemsWithin.OrderBy(i => i.Size).ToList();

				case 2:
					return _sourceContainer.ItemsWithin.OrderBy(i => i.Weight).ToList();

				default:
					return _sourceContainer.ItemsWithin.OrderBy(i => i.Category).ThenByDescending(i => i.TemplateID).ToList();
			}
		}
	}
}