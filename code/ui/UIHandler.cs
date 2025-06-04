using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class UIHandler : Control
	{
		public event UIStateUpdated StateUpdated;

		private UIState _uiState;

		private CharacterInventoryWindow _inventoryWindow;
		private ItemStackSplit _itemStackSplitPrompt;
		private ItemRenamePrompt _itemRenamePrompt;
		private ItemDetails _itemDetails;
		private NoteDisplay _noteDisplay;
		private DialogUI _dialogUI;
		private ShopDisplay _shop;
		private NotificationDisplay _notifications;
		private Control _loadingScreen;

		private Label _playerStatus; // temp stuff, move to separate class
		private GameSystem _game;

		public UIState ActiveUIState
		{
			get { return _uiState; }
		}

		public bool LockKeybindings
		{
			get { return _itemRenamePrompt.Visible; }
		}

		public DialogUI DialogUI
		{
			get { return _dialogUI; }
		}

		internal GameSystem Game
		{
			get { return _game; }
		}

		public override void _Ready()
		{
			_inventoryWindow = GetNode<CharacterInventoryWindow>("CharacterInventory");
			_itemStackSplitPrompt = GetNode<ItemStackSplit>("ItemStackSplit");
			_itemRenamePrompt = GetNode<ItemRenamePrompt>("ItemRenamePrompt");
			_itemDetails = GetNode<ItemDetails>("ItemDetails");
			_playerStatus = GetNode<Label>("PlayerStatusInfo");
			_noteDisplay = GetNode<NoteDisplay>("NoteDisplay");
			_dialogUI = GetNode<DialogUI>("DialogDisplay");
			_shop = GetNode<ShopDisplay>("ShopDisplay");
			_notifications = GetNode<NotificationDisplay>("NotificationDisplay");
			_loadingScreen = GetNode<Control>("LoadingScreen");

			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.NewPlayerSpawned += SetPlayerReferences;
			SubscribeToEvents();
			CallDeferred("ResetUIState");
		}

		public void SetUIState(UIState newState)
		{
			if (_uiState == newState || newState == UIState.None)
			{
				_uiState = UIState.None;
				_game.SetState(GameState.Gameplay);
			}
			else
			{
				_game.SetState(GameState.Interface);
				_uiState = newState;
			}

			StateUpdated?.Invoke(_uiState);
		}

		public void OpenContainer(Gameplay.Container target)
		{
			if (_uiState != UIState.Inventory)
			{
				SetUIState(UIState.Inventory);
			}

			_inventoryWindow.OpenContainer(target);
		}

		public void OpenItemStackSplitPrompt(BaseItem item)
		{
			_itemStackSplitPrompt.ToggleSplitPrompt(item);
		}

		public void OpenItemRenamePrompt(BaseItem item)
		{
			_itemRenamePrompt.ToggleRenamePrompt(item);
		}

		public void ToggleItemDetails(BaseItem item)
		{
			_itemDetails.ToggleItemDetails(item);
		}

		public void OpenNote(BaseItem item)
		{
			if (_uiState == UIState.None)
			{
				SetUIState(UIState.Misc);
			}

			_noteDisplay.OpenNote(item.TemplateID);
		}

		public void OpenShopDialog(Shop shop)
		{
			_shop.ToggleShopDisplay(shop);
		}

		public void DisplayNotification(string content)
		{
			_notifications.TriggerNotification(content);
		}

		private void SubscribeToEvents()
		{
			_noteDisplay.NoteClosed += RestoreUIState;
			_game.Level.UpcomingLevelChange += () => ToggleLoadingScreen(true);
			_game.Level.LevelLoaded += () => ToggleLoadingScreen(false);
		}

		private void SetPlayerReferences()
		{
			_game.Player.SetForDestruction += UnsetPlayerReferences;
			_game.Player.CharStatus.StaminaChanged += UpdatePlayerStatus;
		}

		private void UnsetPlayerReferences()
		{
			_game.Player.CharStatus.StaminaChanged -= UpdatePlayerStatus;
		}

		private void ResetUIState()
		{
			SetUIState(UIState.None);
		}

		private void RestoreUIState()
		{
			if (_uiState == UIState.Misc)
			{
				SetUIState(UIState.None);
			}
		}

		private void UpdatePlayerStatus(float value) // temp
		{
			_playerStatus.Text = $"Stamina: {value}";
		}

		private void ToggleLoadingScreen(bool enable)
		{
			_loadingScreen.Visible = enable;
		}
	}
}