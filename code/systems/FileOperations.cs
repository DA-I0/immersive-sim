using System.Collections.Generic;
using System.Linq;
using Godot;
using ImmersiveSim.GameData;

namespace ImmersiveSim.Systems
{
	public static class FileOperations
	{
		public static string[] GetFolderList(string path)
		{
			if (!DirAccess.DirExistsAbsolute(path))
			{
				return System.Array.Empty<string>();
			}

			return DirAccess.GetDirectoriesAt(path);
		}

		public static string[] GetFileList(string path)
		{
			if (!DirAccess.DirExistsAbsolute(path))
			{
				return System.Array.Empty<string>();
			}

			return DirAccess.GetFilesAt(path);
		}

		public static Dictionary<string, ItemData> LoadBaseItemData()
		{
			string itemConfigFolder = ProjectSettings.GetSetting("global/ItemDataFolder").ToString();
			string[] configFolders = GetFolderList(itemConfigFolder);

			Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();

			foreach (string folder in configFolders)
			{
				string[] itemFiles = GetFileList($"{itemConfigFolder}/{folder}");

				for (int index = 0; index < itemFiles.Length; index++)
				{
					ConfigFile itemConfig = new ConfigFile();
					Error error = itemConfig.Load($"{itemConfigFolder}/{folder}/{itemFiles[index]}");

					if (error == Error.Ok)
					{
						items.Add((string)itemConfig.GetValue(string.Empty, "id"), Statics.HelperMethods.ParseBaseItemData(itemConfig));
					}
				}
			}

			return items;
		}

		public static Dictionary<string, ShopData> LoadBaseShopData()
		{
			Dictionary<string, ShopData> shops = new Dictionary<string, ShopData>();

			string[] configFiles = GetFileList(ProjectSettings.GetSetting("global/ShopDataFolder").ToString());

			for (int index = 0; index < configFiles.Length; index++)
			{
				ConfigFile shopConfig = new ConfigFile();
				Error error = shopConfig.Load($"{ProjectSettings.GetSetting("global/ShopDataFolder")}/{configFiles[index]}");

				if (error == Error.Ok)
				{
					shops.Add((string)shopConfig.GetValue(string.Empty, "id"), Statics.HelperMethods.ParseBaseShopData(shopConfig));
				}
			}

			return shops;
		}

		public static Dictionary<string, ConversationNode> LoadConversationData()
		{
			string fileFolder = $"{ProjectSettings.GetSetting("global/ConversationNodeFolder")}";
			string[] configFiles = GetFileList(fileFolder);
			Dictionary<string, ConversationNode> nodes = new Dictionary<string, ConversationNode>();

			for (int index = 0; index < configFiles.Length; index++)
			{
				ConfigFile itemConfig = new ConfigFile();
				Error error = itemConfig.Load($"{fileFolder}/{configFiles[index]}");

				if (error == Error.Ok)
				{
					nodes.Add((string)itemConfig.GetValue(string.Empty, "id"), Statics.HelperMethods.ParseConversationNodeData(itemConfig));
				}
			}

			return nodes;
		}

		public static Dictionary<string, string[]> LoadNPCSpawnPresets()
		{
			string fileFolder = $"{ProjectSettings.GetSetting("global/NPCSpawnPresetFolder")}";
			string[] configFiles = GetFileList(fileFolder);
			Dictionary<string, string[]> presets = new Dictionary<string, string[]>();

			for (int index = 0; index < configFiles.Length; index++)
			{
				ConfigFile itemConfig = new ConfigFile();
				Error error = itemConfig.Load($"{fileFolder}/{configFiles[index]}");

				if (error == Error.Ok)
				{
					presets.Add((string)itemConfig.GetValue(string.Empty, "id"), (string[])itemConfig.GetValue(string.Empty, "npc_templates"));
				}
			}

			return presets;
		}

		public static string[] GetFontData()
		{
			List<string> fontList = new List<string>();

			foreach (string fileName in GetFileList(ProjectSettings.GetSetting("global/DefaultFontsFolder").ToString()))
			{
				if (fileName.EndsWith(".ttf") || fileName.EndsWith(".otf"))
				{
					fontList.Add(fileName);
				}
			}

			// do the same as above for eventual custom font folder

			return fontList.ToArray<string>();
		}

		public static string[] GetSaveList(string path)
		{
			if (!DirAccess.DirExistsAbsolute(path))
			{
				return System.Array.Empty<string>();
			}

			path = ProjectSettings.GlobalizePath(path);
			IOrderedEnumerable<System.IO.FileSystemInfo> fileSystemInfos = new System.IO.DirectoryInfo(path).GetFiles().OrderByDescending(file => file.LastWriteTimeUtc);
			string[] saveList = new string[fileSystemInfos.Count()];

			for (int index = 0; index < saveList.Length; index++)
			{
				saveList[index] = fileSystemInfos.ElementAt(index).Name;
			}

			return saveList;
		}

		public static string[] GetSaveData(string saveID)
		{
			string[] saveData = new string[] { string.Empty, string.Empty, string.Empty };
			string filePath = Statics.HelperMethods.MakeSavePath(saveID);

			ConfigFile saveState = new ConfigFile();
			Error error = saveState.Load(filePath);

			if (error == Error.Ok)
			{
				saveData[0] = saveID;
				saveData[1] = (string)saveState.GetValue("World", "active_level", string.Empty); // replace with localized level name
				saveData[2] = (string)saveState.GetValue("World", "current_date", string.Empty);
			}

			return saveData;
		}

		public static void SaveGame(GameSystem gameSystem, string saveID)
		{
			FindOrCreateDirectory(ProjectSettings.GetSetting("global/SaveFolder").ToString());

			ConfigFile newSaveState = new ConfigFile();

			InsertWorldDataToSave(gameSystem, ref newSaveState);
			InsertCharactersToSave(gameSystem, ref newSaveState);
			InsertItemsToSave(gameSystem, ref newSaveState);
			InsertPlayerNotesToSave(gameSystem, ref newSaveState);

			newSaveState.Save(Statics.HelperMethods.MakeSavePath(saveID));
		}

		public static ConfigFile LoadGame(string saveID)
		{
			ConfigFile saveState = new ConfigFile();
			Error error = saveState.Load(Statics.HelperMethods.MakeSavePath(saveID));

			if (error == Error.Ok)
			{
				return saveState;
			}

			return null;
		}

		// public static Difficulty[] LoadDifficulties(string path)
		// {
		// 	if (!DirAccess.DirExistsAbsolute(path))
		// 	{
		// 		return System.Array.Empty<Difficulty>();
		// 	}

		// 	string[] difficultyFiles = DirAccess.GetFilesAt(path);
		// 	System.Collections.Generic.List<Difficulty> difficulties = new System.Collections.Generic.List<Difficulty>();

		// 	foreach (string filePath in difficultyFiles)
		// 	{
		// 		ConfigFile nextDifficulty = new ConfigFile();
		// 		Error error = nextDifficulty.Load($"{path}/{filePath}");

		// 		if (error == Error.Ok)
		// 		{
		// 			difficulties.Add(HelperMethods.DifficultyFromConfig(nextDifficulty));
		// 		}
		// 	}

		// 	return difficulties.ToArray();
		// }

		// public static void SaveDifficulty(string oldDifficultyName, Difficulty newDifficulty)
		// {
		// 	if (oldDifficultyName != string.Empty && oldDifficultyName != newDifficulty.DifficultyName)
		// 	{
		// 		DeleteDifficulty(oldDifficultyName);
		// 	}

		// 	FindOrCreateDirectory(ProjectSettings.GetSetting("global/CustomDifficultyFolder").ToString());

		// 	string fileName = $"diff_{newDifficulty.DifficultyName}";
		// 	ConfigFile parsedDifficulty = HelperMethods.DifficultyToConfig(newDifficulty);
		// 	parsedDifficulty.Save($"{ProjectSettings.GetSetting("global/CustomDifficultyFolder")}/{fileName}.diff");
		// }

		// public static void DeleteDifficulty(string difficultyName)
		// {
		// 	string filePath = $"{ProjectSettings.GetSetting("global/CustomDifficultyFolder")}/diff_{difficultyName}.diff";

		// 	if (FileAccess.FileExists(filePath))
		// 	{
		// 		DirAccess.RemoveAbsolute(filePath);
		// 	}
		// }

		public static string LoadTextFile(string filePath)
		{
			string fileContent = string.Empty;

			if (FileAccess.FileExists(filePath))
			{
				FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
				fileContent = file.GetAsText();
			}

			return fileContent;
		}

		private static void FindOrCreateDirectory(string path)
		{
			if (DirAccess.DirExistsAbsolute(path))
			{
				return;
			}

			DirAccess.MakeDirAbsolute(path);
		}

		public static void DeleteSave(string saveID)
		{
			string filePath = Statics.HelperMethods.MakeSavePath(saveID);

			if (FileAccess.FileExists(filePath))
			{
				DirAccess.RemoveAbsolute(filePath);
			}
		}

		private static void InsertWorldDataToSave(GameSystem gameSystem, ref ConfigFile newSaveState)
		{
			newSaveState.SetValue($"World", "active_level", gameSystem.Level.ActiveSceneName);//GetNode("../ActiveLevel").GetChild(0).Name); // replace with a call to a LevelManager class
			newSaveState.SetValue($"World", "current_date", gameSystem.Time.CurrentDate.ToString());
		}

		private static void InsertCharactersToSave(GameSystem gameSystem, ref ConfigFile newSaveState)
		{
			foreach (CharacterSaveState character in gameSystem.Save.CharacterStateCache.Values)
			{
				newSaveState.SetValue($"Character - {character.CharacterID}", "node_path", character.NodePath);
				newSaveState.SetValue($"Character - {character.CharacterID}", "id", character.CharacterID);
				newSaveState.SetValue($"Character - {character.CharacterID}", "position", character.Position);
				newSaveState.SetValue($"Character - {character.CharacterID}", "rotation", character.Rotation);
				newSaveState.SetValue($"Character - {character.CharacterID}", "velocity", character.Velocity);
				newSaveState.SetValue($"Character - {character.CharacterID}", "max_health", character.MaxHealth);
				newSaveState.SetValue($"Character - {character.CharacterID}", "health", character.Health);
				newSaveState.SetValue($"Character - {character.CharacterID}", "last_rest_date", character.LastRestDate.ToString());
				newSaveState.SetValue($"Character - {character.CharacterID}", "stamina", character.Stamina);
				newSaveState.SetValue($"Character - {character.CharacterID}", "money", character.Money);
				newSaveState.SetValue($"Character - {character.CharacterID}", "equipment_items", character.EquipmentItems);
			}
		}

		private static void InsertItemsToSave(GameSystem gameSystem, ref ConfigFile newSaveState)
		{
			foreach (ItemSaveState item in gameSystem.Save.ItemStateCache.Values)
			{
				if (item.NeedsSaving)
				{
					newSaveState.SetValue($"Item - {item.ObjectID}", "current_node_path", item.CurrentNodePath);
					newSaveState.SetValue($"Item - {item.ObjectID}", "object_id", item.ObjectID);
					newSaveState.SetValue($"Item - {item.ObjectID}", "template_id", item.TemplateID);
					newSaveState.SetValue($"Item - {item.ObjectID}", "custom_name", item.CustomName);
					newSaveState.SetValue($"Item - {item.ObjectID}", "is_instanced_object", item.IsInstancedObject);
					newSaveState.SetValue($"Item - {item.ObjectID}", "is_destroyed", item.IsDestroyed);
					newSaveState.SetValue($"Item - {item.ObjectID}", "position", item.Position);
					newSaveState.SetValue($"Item - {item.ObjectID}", "rotation", item.Rotation);
					newSaveState.SetValue($"Item - {item.ObjectID}", "velocity", item.Velocity);
					newSaveState.SetValue($"Item - {item.ObjectID}", "is_visible", item.Visible);
					newSaveState.SetValue($"Item - {item.ObjectID}", "is_frozen", item.Frozen);
					newSaveState.SetValue($"Item - {item.ObjectID}", "collissions_disabled", item.CollisionsDisabled);
					newSaveState.SetValue($"Item - {item.ObjectID}", "stack_count", item.StackCount);
					newSaveState.SetValue($"Item - {item.ObjectID}", "parent_container_node_path", item.ParentContainer);
					newSaveState.SetValue($"Item - {item.ObjectID}", "uses_left", item.UsesLeft);
					newSaveState.SetValue($"Item - {item.ObjectID}", "items_within", item.ItemsWithin);
					newSaveState.SetValue($"Item - {item.ObjectID}", "is_equipped", item.IsEquipped);
				}
			}
		}

		private static void InsertPlayerNotesToSave(GameSystem gameSystem, ref ConfigFile newSaveState)
		{
			foreach (PlayerNote note in gameSystem.Journal.PlayerNotes.Values)
			{
				newSaveState.SetValue($"Note - {note.ID}", "id", note.ID.ToString());
				newSaveState.SetValue($"Note - {note.ID}", "title", note.Title);
				newSaveState.SetValue($"Note - {note.ID}", "content", note.Content);
			}
		}
	}
}