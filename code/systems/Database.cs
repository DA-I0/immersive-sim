using System.Collections.Generic;
using System.Linq;
using Godot;
using ImmersiveSim.GameData;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Systems
{
	public class Database
	{
		private readonly Dictionary<string, ItemData> _itemData;
		private readonly Dictionary<string, StoreData> _storeData;
		private readonly Dictionary<string, ConversationNode> _conversationNodes;
		private Dictionary<string, string[]> _npcSpawnPresets = new Dictionary<string, string[]>();
		private readonly string[] _fonts;
		private Dictionary<string, string> _customValues = new Dictionary<string, string>();
		private Dictionary<string, string> _gameVariables = new Dictionary<string, string>(); // move to SaveManager

		public Dictionary<string, ItemData> Items
		{
			get { return _itemData; }
		}

		public Dictionary<string, StoreData> Stores
		{
			get { return _storeData; }
		}

		public Dictionary<string, ConversationNode> ConversationNodes
		{
			get { return _conversationNodes; }
		}

		public Dictionary<string, string[]> NpcSpawnPresets
		{
			get { return _npcSpawnPresets; }
		}

		public string[] Fonts
		{
			get { return _fonts; }
		}

		public Dictionary<string, string> GameVariables
		{
			get { return _gameVariables; }
		}

		public Database()
		{
			_itemData = FileOperations.LoadBaseItemData();
			_storeData = FileOperations.LoadBaseStoreData();
			_conversationNodes = FileOperations.LoadConversationData();
			_npcSpawnPresets = FileOperations.LoadNPCSpawnPresets();
			_fonts = FileOperations.GetFontData();
			LoadCustomValues();
		}

		// MOVE TO SaveManager
		public void ModifyVariable(string variable, string operation, string value)
		{
			_gameVariables.TryAdd(variable, string.Empty);

			if (operation == "set")
			{
				SetVariable(variable, value);
				return;
			}

			if (operation == "change")
			{
				ChangeVariable(variable, value);
			}
		}

		public bool CompareVariable(string variable, string operation, string targetValue)
		{
			bool result = false;

			if (_gameVariables.TryGetValue(variable, out string value))
			{
				return HelperMethods.CompareValues(operation, value, targetValue);
			}

			return result;
		}

		public void SetVariable(string variable, string value)
		{
			_gameVariables[variable] = value;
			GD.Print($"variable {variable} set to {_gameVariables[variable]}");
		}

		public void ChangeVariable(string variable, string value)
		{
			_ = int.TryParse(_gameVariables[variable], out int varValue);
			_ = int.TryParse(value, out int changeValue);

			varValue += changeValue;
			SetVariable(variable, varValue.ToString());
		}
		// END OF MOVE TO SaveManager

		private void LoadCustomValues()
		{
			_customValues.Clear();
			string content = FileOperations.LoadTextFile("res://assets/text/custom_text_values.txt");
			HelperMethods.ParseCustomValues(content, ref _customValues);
		}

		public string[] GetInputSymbol(string inputEvent, string device)
		{
			var inputEvents = GetFilteredInputEvents(inputEvent, device);

			// InputMap.ActionGetEvents(inputEvent).Where(a => a.AsText().Contains(device));

			List<string> inputSymbols = new List<string>();

			// if (device.Contains("Mouse") && (inputEvent == "game_left" || inputEvent == "game_right"))
			// {
			// 	inputSymbols.Add(GetCustomValue("Mouse"));
			// }

			for (int index = 0; index < inputEvents.Count(); index++)
			{
				string targetAction = inputEvents.ElementAt(index).AsText();

				if (targetAction.Find(" (") != -1)
				{
					targetAction = targetAction.Substring(0, targetAction.Find(" ("));
					// targetAction = SetJoypadType(targetAction);
				}

				string keybind = GetCustomValue(targetAction);

				if (keybind.Length > 1 && keybind.Contains('+'))
				{
					string[] keys = keybind.Split('+');
					keybind = string.Empty;

					foreach (string key in keys)
					{
						keybind += $"{GetCustomValue(key)} + ";
					}

					keybind = keybind.Remove(keybind.Length - 3);
				}

				inputSymbols.Add(keybind);
			}

			return inputSymbols.ToArray<string>();
		}

		private IEnumerable<InputEvent> GetFilteredInputEvents(string inputEvent, string device)
		{
			// if (device.Contains("Mouse"))
			// {
			// 	return InputMap.ActionGetEvents(inputEvent).Where(a => a.AsText().Contains("Mouse"));
			// }

			// if (device.Contains("Keyboard"))
			// {
			// 	return InputMap.ActionGetEvents(inputEvent).Where(a => !a.AsText().Contains("Joypad") && !a.AsText().Contains("Mouse"));
			// }

			if (device.Contains("Joypad"))
			{
				return InputMap.ActionGetEvents(inputEvent).Where(a => a.AsText().Contains("Joypad"));
			}

			return InputMap.ActionGetEvents(inputEvent).Where(a => !a.AsText().Contains("Joypad"));
			// return new Godot.Collections.Array<InputEvent>();
		}

		private string GetCustomValue(string key)
		{
			try
			{
				return _customValues[key];
			}
			catch
			{
				// GD.PrintErr("Replacement value not found");
			}

			return key.Contains("_") ? key[3..] : key;
		}

		public string GetInputKey(string targetValue)
		{
			foreach (var entry in _customValues)
			{
				if (entry.Value == targetValue)
				{
					return entry.Key;
				}
			}

			return targetValue;
		}

	}
}