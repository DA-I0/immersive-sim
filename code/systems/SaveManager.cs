using System.Collections.Generic;
using System.Linq;
using ImmersiveSim.GameData;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Systems
{
	public class SaveManager
	{
		public StatusUpdate GameLoaded;

		private Dictionary<string, CharacterSaveState> _characterStateCache = new Dictionary<string, CharacterSaveState>();
		private Dictionary<string, ItemSaveState> _itemStateCache = new Dictionary<string, ItemSaveState>();

		private string _saveFolderPath;

		private bool _isLoading = false;
		private Godot.ConfigFile _loadedSaveFile;

		private readonly GameSystem _game;

		public Dictionary<string, CharacterSaveState> CharacterStateCache
		{
			get { return _characterStateCache; }
		}

		public Dictionary<string, ItemSaveState> ItemStateCache
		{
			get { return _itemStateCache; }
		}

		public SaveManager(GameSystem gameSystem)
		{
			_game = gameSystem;
			_saveFolderPath = Godot.ProjectSettings.GetSetting("global/SaveFolder").ToString();
		}

		public void SubscribeToEvents()
		{
			_game.Level.LevelLoaded += ActOnLevelChange;
			_game.Level.SubLevelLoaded += RestoreInstancedItems;
		}

		public void SaveGame(string fileName)
		{
			FileOperations.SaveGame(_game, fileName);
			_game.Notifications.TriggerNotification("game_save");
		}

		public void LoadGame(string fileName)
		{
			_isLoading = true;
			_game.DestroyPlayer();
			ClearStateCache();
			RestoreSavedState(FileOperations.LoadGame(fileName));
		}

		public string[] GetSaveList()
		{
			string[] saveFileList = FileOperations.GetSaveList(_saveFolderPath);//GetFileList(_saveFolderPath);
			return saveFileList;
		}

		public bool RegisterCharacterForSaving(CharacterBase target)
		{
			if (_characterStateCache.ContainsKey(target.ID))
			{
				return false;
			}

			_characterStateCache.Add(target.ID, HelperMethods.PrepareCharacterSaveState(target));
			return true;
		}

		private bool RegisterCharacterForSaving(CharacterSaveState target)
		{
			if (_characterStateCache.ContainsKey(target.CharacterID))
			{
				return false;
			}

			_characterStateCache.Add(target.CharacterID, target);
			return true;
		}

		public void UpdateCharacterCache(CharacterBase target)
		{
			if (target == null || target.ID == null)
			{
				return;
			}

			if (_characterStateCache.TryGetValue(target.ID, out CharacterSaveState value))
			{
				value.UpdateCharacterData(target);
			}
		}

		public bool RegisterItemForSaving(BaseItem target)
		{
			if (_itemStateCache.ContainsKey(target.ObjectID))
			{
				return false;
			}

			_itemStateCache.Add(target.ObjectID, HelperMethods.PrepareItemSaveState(target));
			return true;
		}

		private bool RegisterItemForSaving(ItemSaveState target)
		{
			if (_itemStateCache.ContainsKey(target.ObjectID))
			{
				return false;
			}

			_itemStateCache.Add(target.ObjectID, target);
			return true;
		}

		public void UpdateItemCache(BaseItem target)
		{
			if (target == null || _game.Level.LevelChangeInProgress)
			{
				return;
			}

			_itemStateCache[target.ObjectID]?.UpdateItemData(target);
		}

		public CharacterSaveState GetCharacterSaveState(string characterID)
		{
			return _characterStateCache[characterID];
		}

		public ItemSaveState GetItemSaveState(string objectID)
		{
			return _itemStateCache[objectID];
		}

		public BaseItem IsItemTracked(string objectID)
		{
			BaseItem target = _game.GetNodeOrNull<BaseItem>(_itemStateCache[objectID].CurrentNodePath);

			if (target != null)
			{
				return target;
			}

			ItemSaveState targetItem = GetItemSaveState(objectID);
			return _game.SpawnItem(targetItem.TemplateID, objectID);
		}

		public void RestoreSavedState(Godot.ConfigFile saveState)
		{
			if (saveState == null)
			{
				return;
			}

			_loadedSaveFile = saveState;

			// parse character and item state from config file and put them into _*StateCache dictionaries
			string[] sections = saveState.GetSections();

			foreach (string section in sections)
			{
				if (section.Contains("Character - "))
				{
					RegisterCharacterForSaving(HelperMethods.ParseCharacterSaveState(saveState, section));
				}

				if (section.Contains("Item - "))
				{
					RegisterItemForSaving(HelperMethods.ParseItemSaveState(saveState, section));
				}

				if (section.Contains("Note - "))
				{
					_game.Journal.CreateNote(
						(string)saveState.GetValue(section, "title"),
						(string)saveState.GetValue(section, "content"),
						(string)saveState.GetValue(section, "id")
					);
				}
			}

			string activeLevel = (string)saveState.GetValue("World", "active_level", "test_map");
			_game.Level.ChangeLevel(activeLevel);
		}

		private void ActOnLevelChange()
		{
			if (_game.Level.IsInMenu)
			{
				ClearStateCache();
				return;
			}

			if (_isLoading)
			{
				_game.RestoreSavedState(_loadedSaveFile);
				GameLoaded?.Invoke();
				_game.Notifications.TriggerNotification("game_load");
			}

			RestoreInstancedItems(_game.Level.ActiveScenePath);
		}

		private void RestoreInstancedItems(string levelPath)
		{
			if (levelPath == string.Empty)
			{
				return;
			}

			foreach (ItemSaveState item in _itemStateCache.Values.Where(i => i.CurrentNodePath.Contains(levelPath)))
			{
				if (item.IsInstancedObject)
				{
					_game.SpawnItem(item.TemplateID, item.ObjectID);
				}
			}
		}

		private void ClearStateCache()
		{
			_characterStateCache.Clear();
			_itemStateCache.Clear();
		}
	}
}