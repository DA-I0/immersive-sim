using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class CharacterInventoryWindow : Control
	{
		[Export] PackedScene _containerWindowPrefab;

		private Control _equipmentSlotList;
		private Control _containerWindowList;
		private ItemInteractionMenu _itemInteractionMenu;
		private Systems.GameSystem _game;

		private UIManager _ui;

		public override void _Ready()
		{
			_equipmentSlotList = GetNode<Control>("PanelContainer/VBoxContainer");
			_containerWindowList = GetNode<Control>("ContainerWindows/WindowList");
			_game = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.NewPlayerSpawned += SetPlayerReferences;
			_itemInteractionMenu = GetNode<ItemInteractionMenu>("ItemInteractionMenu");
			_ui = GetParent().GetParent<UIManager>();
			SubscribeToEvents();
		}

		public override void _Input(InputEvent @event)
		{
			if (_ui.LockKeybindings)
			{
				return;
			}

			if (@event.IsActionPressed("action_inventory"))
			{
				_ui.SetUIState(UIState.Inventory);
			}

			// if (@event.IsActionPressed("ui_cancel") && Visible)
			// {
			// 	_ui.ChangeUIState(UIState.None);
			// }
		}

		public void ToggleInventory(UIState currentState)
		{
			Visible = (currentState == UIState.Inventory);

			if (Visible)
			{
				RefreshInventory();
			}
			else
			{
				ClearAllContainerWindows();
			}
		}

		public void RefreshInventory()
		{
			if (!Visible)
			{
				return;
			}

			ClearUnusedContainerWindows();
			PopulateContainerWindowsForEquipped();
		}

		public void OpenContainer(Gameplay.Container target, bool isTemporary = true)
		{
			if (CheckIfOpened(target))
			{
				return;
			}

			Control containerWindow = _containerWindowPrefab.Instantiate<Control>();
			_containerWindowList.AddChild(containerWindow);

			if (!isTemporary)
			{
				_containerWindowList.MoveChild(containerWindow, _containerWindowList.GetChildCount() - 2);
			}

			((ContainerWindow)containerWindow).isTemporary = isTemporary;
			((ContainerWindow)containerWindow).OpenContainer(target);
		}

		public void CloseContainer(Gameplay.Container target)
		{
			int windowIndex = FindContainerWindowIndex(target);

			if (windowIndex > -1)
			{
				((ContainerWindow)_containerWindowList.GetChild(windowIndex)).CloseContainer();
			}
		}

		public void ToggleItemInteractionMenu(BaseItem targetItem, Vector2 position)
		{
			_itemInteractionMenu.ToggleInteractionMenu(targetItem, position);
		}

		private void SubscribeToEvents()
		{
			_ui.StateUpdated += ToggleInventory;
			_ui.OpenContainerUI += OpenContainer;
		}

		private void SetPlayerReferences()
		{
			_game.Player.SetForDestruction += UnsetPlayerReferences;
			_game.Player.CharInventory.InventoryUpdated += RefreshInventory;
		}

		private void UnsetPlayerReferences()
		{
			_game.Player.CharInventory.InventoryUpdated -= RefreshInventory;
		}

		private void ClearAllContainerWindows()
		{
			foreach (Control window in _containerWindowList.GetChildren())
			{
				((ContainerWindow)window).CloseContainer();
			}
		}

		private void ClearUnusedContainerWindows()
		{
			foreach (Control window in _containerWindowList.GetChildren())
			{
				if (!((ContainerWindow)window).isTemporary)
				{
					if (!_game.Player.CharInventory.CheckIfEquipped(((ContainerWindow)window).SourceContainer, false))
					{
						((ContainerWindow)window).CloseContainer();
					}
				}
			}
		}

		private void PopulateContainerWindowsForEquipped()
		{
			foreach (BaseItem item in _game.Player.CharInventory._equipmentSlots)
			{
				if (item is Gameplay.Container itemContainer)
				{
					OpenContainer(itemContainer, false);
				}
			}
		}

		public bool CheckIfOpened(Gameplay.Container item)
		{
			return FindContainerWindowIndex(item) >= 0;
		}

		private int FindContainerWindowIndex(Gameplay.Container item)
		{
			for (int index = 0; index < _containerWindowList.GetChildCount(); index++)
			{
				if (((ContainerWindow)_containerWindowList.GetChild(index)).SourceContainer == item)
				{
					return index;
				}
			}

			return -1;
		}
	}
}