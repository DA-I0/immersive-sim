using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Systems
{
	public partial class GameSystem : Node
	{
		public StatusUpdate NewPlayerSpawned;
		public StatusUpdate ItemSortChanged;

		public GameState _state = GameState.Gameplay;

		private Database _database;
		private Settings _settings;
		private TimeController _time;
		private UI.UIHandler _ui;
		private PlayerBase _player;
		private WorldEnvironment _world;
		private DialogManager _dialog;
		private SaveManager _save;
		private LevelManager _level;
		private JournalManager _journal;
		private NPCSpawnController _npcSpawnController;
		private Timer _npcSpawnTimer;
		private NotificationManager _notifications;

		// temp
		private int _itemListSortOrder = -1;  // TODO: move to game settings
		public int ItemListSortOrder
		{
			get { return _itemListSortOrder; }
			set
			{
				_itemListSortOrder = value;
				ItemSortChanged?.Invoke();
			}
		}
		// temp

		public Database GameDatabase
		{
			get { return _database; }
		}

		public Settings Settings
		{
			get { return _settings; }
		}

		public GameState State
		{
			get { return _state; }
		}

		public TimeController Time
		{
			get { return _time; }
		}

		public UI.UIHandler UI
		{
			get { return _ui; }
		}

		public PlayerBase Player
		{
			get { return _player; }
		}

		public WorldEnvironment World
		{
			get { return _world; }
		}

		public DialogManager Dialog
		{
			get { return _dialog; }
		}

		public SaveManager Save
		{
			get { return _save; }
		}

		public LevelManager Level
		{
			get { return _level; }
		}

		public JournalManager Journal
		{
			get { return _journal; }
		}

		public NPCSpawnController NPCSpawnController
		{
			get { return _npcSpawnController; }
		}

		public NotificationManager Notifications
		{
			get { return _notifications; }
		}

		public override void _EnterTree()
		{
			_database = new Database();
			_settings = new Settings(this);
			_dialog = new DialogManager(this);
			_save = new SaveManager(this);
			_journal = new JournalManager();
			_npcSpawnController = new NPCSpawnController(this);
			_npcSpawnTimer = new Timer
			{
				OneShot = false
			};
			CallDeferred(MethodName.AddChild, _npcSpawnTimer);
			_npcSpawnTimer.Timeout += _npcSpawnController.SpawnRandomNPC;
			_notifications = new NotificationManager(this);
		}

		public override void _Ready()
		{
			SetupReferences();

			_settings.InitializeSettings();
			_level.LoadMenu();
		}

		public override void _Input(InputEvent @event)
		{
			if (_state != GameState.Gameplay)
			{
				return;
			}

			if (@event.IsActionPressed("quick_save"))
			{
				Save.SaveGame("quicksave");
				return;
			}

			if (@event.IsActionPressed("quick_load"))
			{
				Save.LoadGame("quicksave");
				return;
			}

			if (@event.IsActionPressed("ui_cancel") && !_level.IsInMenu && _ui.ActiveUIState == UIState.None)
			{
				_ui.SetUIState(UIState.Pause);
				return;
			}

			// if (@event.IsActionPressed("ui_cancel"))
			// {
			// 	switch (_state)
			// 	{
			// 		case GameState.Gameplay:
			// 			SetState(GameState.Menu);
			// 			break;

			// 		default:
			// 			SetState(GameState.Gameplay);
			// 			break;
			// 	}
			// }
		}

		public void SetState(GameState newState)
		{
			_state = newState;
			GetTree().Paused = (_state == GameState.Menu);

			if (_state == GameState.Gameplay)
			{
				Input.SetMouseMode(Input.MouseModeEnum.Captured);
			}
			else
			{
				Input.SetMouseMode(Input.MouseModeEnum.Visible);
			}
		}

		public void QuitGame()
		{
			GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
			GetTree().Quit();
		}

		private async void SetupReferences()
		{
			_time = GetNode<TimeController>(ProjectSettings.GetSetting("global/TimeControllerPath").ToString());
			_ui = GetNode<UI.UIHandler>(ProjectSettings.GetSetting("global/UIHandlerPath").ToString());
			_world = GetNode<WorldEnvironment>("../WorldEnvironment");
			_level = GetNode<LevelManager>("../ActiveLevel");

			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
			SubscribeToEvents();
		}

		private void SubscribeToEvents()
		{
			_ui.StateUpdated += UpdateBackgroundEffect;
			_level.LevelLoaded += HandleLevelChange;
			Dialog.SubscribeToEvents();
			Save.SubscribeToEvents();
		}

		private void HandleLevelChange()
		{
			_ui.SetUIState(UIState.None);

			if (_level.IsInMenu)
			{
				DestroyPlayer();
				SetState(GameState.Menu);
				_npcSpawnTimer.Stop();
			}
			else
			{
				SpawnNewPlayer();
				_npcSpawnTimer.Start();
			}
		}

		public void DestroyPlayer()
		{
			if (_player != null)
			{
				_player.Destroy();
				_player = null;
			}
		}

		private async void SpawnNewPlayer()
		{
			if (_player == null)
			{
				await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
				await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
				await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

				ResourceLoader.LoadThreadedRequest(ProjectSettings.GetSetting("global/PlayerPrefabPath").ToString());
				Node newPlayer = (ResourceLoader.LoadThreadedGet(ProjectSettings.GetSetting("global/PlayerPrefabPath").ToString()) as PackedScene).Instantiate();
				GetNode("/root/Game").AddChild(newPlayer);
				_player = newPlayer.GetNode<PlayerBase>("CharacterBase");
				NewPlayerSpawned?.Invoke();
			}

			SetState(GameState.Gameplay);

			if (_player != null)
			{
				Vector3 spawnPosition = _level.GetSpawnPosition();
				_player.CharMovement.GlobalPosition = spawnPosition;
			}
		}

		public BaseItem SpawnItem(string templateID, string objectID = "", bool canBeSaved = true, Node targetParent = null)
		{
			string category = GameDatabase.Items[templateID].Category.ToString();
			string prefabPath = $"{ProjectSettings.GetSetting("global/ItemPrefabFolder")}/{category}/{templateID}.tscn".ToLower();
			ResourceLoader.LoadThreadedRequest(prefabPath);
			BaseItem newItem = (BaseItem)(ResourceLoader.LoadThreadedGet(prefabPath) as PackedScene).Instantiate();
			newItem.SetInstancedState(objectID, canBeSaved);

			if (targetParent == null)
			{
				GetNode("/root/Game/ActiveLevel").GetChild(0).AddChild(newItem);
			}
			else
			{
				targetParent.AddChild(newItem);
			}

			return newItem;
		}

		private void UpdateBackgroundEffect(UIState newState)
		{
			if (newState == UIState.None || newState == UIState.Dialog)
			{
				_world.Environment.GlowBlendMode = Environment.GlowBlendModeEnum.Softlight; // add background blur in menus
			}
			else
			{
				_world.Environment.GlowBlendMode = Environment.GlowBlendModeEnum.Replace;
			}
		}

		internal void RestoreSavedState(ConfigFile saveState)
		{
			Time.RestoreSavedState(saveState);
		}

		internal bool ParseChoiceRequirements(string action, CharacterBase targetCharacter = null)
		{
			string[] actionParameters = action.Split(':');

			switch (actionParameters[0])
			{
				case "var":
					return GameDatabase.CompareVariable(actionParameters[1], actionParameters[2], actionParameters[3]);

				case "quest":
					// check mission state
					break;

				case "time":
					return Time.CompareTime(actionParameters[2], actionParameters[3], actionParameters[4]);

				case "stamina":
					return HelperMethods.CompareValues(actionParameters[2], targetCharacter.CharStatus.Stamina.ToString(), actionParameters[3]);

				case "stat":
					// check character's stat (char_id:stat_to_check:comparator:value)
					break;

				case "money":
					return HelperMethods.CompareValues(actionParameters[1], targetCharacter.CharInventory.Money.ToString(), actionParameters[2]);

				case "inv":
					// check inventory
					break;

				default:
					break;
			}

			return true;
		}

		internal bool ParseTriggeredAction(string action, CharacterBase targetCharacter = null)
		{
			string[] actionParameters = action.Split(':');

			switch (actionParameters[0])
			{
				case "shop":
					Shop _targetStore = targetCharacter.GetNode<Shop>($"../{actionParameters[1]}"); // replace targetNPC with path to the shop node?
					Dialog.CloseDialog();
					UI.OpenShopDialog(_targetStore);
					return false;

				case "var":
					if (actionParameters[2] == "set" || actionParameters[2] == "change")
					{
						GameDatabase.ModifyVariable(actionParameters[1], actionParameters[2], actionParameters[3]);
					}
					break;

				case "quest":
					// affect mission state
					break;

				case "time":
					if (actionParameters[1] == "set")
					{
						Time.TimeSet(actionParameters[2], actionParameters[3]);
					}
					else
					{
						Time.TimeSkip(actionParameters[2], actionParameters[3]);
					}
					break;

				case "stamina":
					float changeValue = 0;
					float.TryParse(actionParameters[2], out changeValue);
					if (actionParameters[1] == "set")
					{
						targetCharacter.CharStatus.SetStamina(changeValue);
					}
					else
					{
						targetCharacter.CharStatus.ChangeStamina(changeValue);
					}
					break;

				case "stat":
					// change character's stat (char_id:stat_to_change:operation:value)
					break;

				case "money":
					int moneyAmount = 0;
					int.TryParse(actionParameters[2], out moneyAmount);
					if (actionParameters[1] == "set")
					{
						targetCharacter.CharInventory.SetMoney(moneyAmount);
					}
					else
					{
						targetCharacter.CharInventory.ChangeMoney(moneyAmount);
					}
					break;

				case "inv":
					// change inventory state
					break;

				case "move":
					// teleport character (based on ID?) to another location
					break;

				case "move_random":
					// teleport character (based on ID?) to another location (randomise based on some kind of array of possibilities, kept as prefabs in the database?)
					break;

				default:
					break;
			}

			return true;
		}
	}
}