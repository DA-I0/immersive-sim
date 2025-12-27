using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class UIManager : Control
	{
		public event UIStateUpdated StateUpdated;
		public event OpenContainer OpenContainerUI;
		public event OpenNote OpenNoteUI;
		public event OpenShop OpenShopUI;

		private UIState _uiState;

		private ItemStackSplit _itemStackSplitPrompt;
		private ItemRenamePrompt _itemRenamePrompt;
		private ItemDetails _itemDetails;
		private ConversationUI _conversationUI;
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
			get { return ShouldLockKeybinds(); }
		}

		public ConversationUI ConversationUI
		{
			get { return _conversationUI; }
		}

		internal GameSystem Game
		{
			get { return _game; }
		}

		private bool ShouldLockKeybinds()
		{
			Control focusedControl = GetViewport().GuiGetFocusOwner();

			if (focusedControl is LineEdit || focusedControl is TextEdit)
			{
				return true;
			}

			return false;
		}

		public override void _Ready()
		{
			_itemStackSplitPrompt = GetNode<ItemStackSplit>("ItemStackSplit");
			_itemRenamePrompt = GetNode<ItemRenamePrompt>("ItemRenamePrompt");
			_itemDetails = GetNode<ItemDetails>("ItemDetails");
			_playerStatus = GetNode<Label>("PlayerStatusInfo");
			_conversationUI = GetNode<ConversationUI>("ConversationDisplay");
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

		internal void ChangeUIState(UIState newState)
		{
			if (_uiState != newState)
			{
				SetUIState(newState);
			}
		}

		public void OpenContainer(Gameplay.Container target)
		{
			if (_uiState != UIState.Inventory)
			{
				SetUIState(UIState.Inventory);
			}

			OpenContainerUI?.Invoke(target, true);
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

			OpenNoteUI?.Invoke(item.TemplateID);
		}

		public void OpenShopDialog(Shop shop)
		{
			OpenShopUI?.Invoke(shop);
		}

		public void DisplayNotification(string content)
		{
			_notifications.TriggerNotification(content);
		}

		private void SubscribeToEvents()
		{
			GetNode<NoteDisplay>("NoteDisplay").NoteClosed += RestoreUIState;
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
