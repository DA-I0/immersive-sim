using Godot;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class SaveMenu : Control
	{
		[Export] private bool _inMenu = false;
		[Export] private PackedScene _saveEntryPrefab;

		private int _selectedSaveStateIndex;

		private Node _saveList;
		private Node _buttonList;

		private LineEdit _saveName;
		private Label _selectedSaveName;
		private Label _selectedSaveMap;
		private Label _selectedSaveDate;

		private GameSystem _game;

		public override void _Ready()
		{
			_saveList = GetNode("SaveList");
			_buttonList = GetNode("SaveInfo/Buttons");
			_saveName = GetNode<LineEdit>("SaveList/NewSave/SaveName");
			_selectedSaveName = GetNode<Label>("SaveInfo/SaveData/Name");
			_selectedSaveMap = GetNode<Label>("SaveInfo/SaveData/ActiveMap");
			_selectedSaveDate = GetNode<Label>("SaveInfo/SaveData/CurrentDate");

			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.UI.StateUpdated += ToggleVisibility;

			TreeExiting += UnsubscribeFromEvents;

			ToggleSaveControls();
		}

		private void ToggleVisibility(UIState newState)
		{
			if (_inMenu == _game.Level.IsInMenu)
			{
				Visible = (newState == UIState.SaveLoad);
				_saveName.Text = string.Empty;
				SelectSaveState(-1);
				UpdateSaveList();
				return;
			}

			Visible = false;
		}

		private void ToggleSaveControls()
		{
			_saveList.GetChild<Control>(0).Visible = !_inMenu;
			_buttonList.GetChild<Button>(0).Visible = !_inMenu;
		}

		private void ClearSaveList()
		{
			foreach (Node child in _saveList.GetChildren())
			{
				if (child is Button saveEntry)
				{
					saveEntry.QueueFree();
				}
			}
		}

		private void UpdateSaveList()
		{
			if (Visible)
			{
				ClearSaveList();

				for (int index = 0; index < _game.Save.GetSaveList().Length; index++)
				{
					Button saveEntry = CreateSaveEntryButton(_game.Save.GetSaveList()[index], index);
					_saveList.AddChild(saveEntry);
				}
			}
		}

		private Button CreateSaveEntryButton(string saveName, int index)
		{
			Button saveEntryButton = new Button
			{
				Text = saveName.Replace(".save", string.Empty),
				Alignment = HorizontalAlignment.Left,
			};

			saveEntryButton.Pressed += () => SelectSaveState(index + 1);

			return saveEntryButton;
		}

		private void SaveGame()
		{
			if (_selectedSaveStateIndex < 0)
			{
				_game.Save.SaveGame(_saveName.Text);
			}
			else
			{
				string selectedSaveName = _saveList.GetChild<Button>(_selectedSaveStateIndex).Text.Trim();
				_game.Save.SaveGame(selectedSaveName);
			}

			UpdateSaveList();
		}

		private void LoadGame()
		{
			if (_saveList.GetChildCount() > _selectedSaveStateIndex)
			{
				_game.Save.LoadGame(_saveList.GetChild<Button>(_selectedSaveStateIndex).Text);
			}
		}

		private void DeleteSave()
		{
			string selectedSaveName = _saveList.GetChild<Button>(_selectedSaveStateIndex).Text;
			FileOperations.DeleteSave(selectedSaveName);
			UpdateSaveList();
		}

		private void SelectSaveState(int index)
		{
			_selectedSaveStateIndex = index;
			UpdateSaveButtons();
			UpdateSelectedSaveData();
		}

		private void UpdateSaveButtons()
		{
			_buttonList.GetChild<Button>(0).Text = (_selectedSaveStateIndex < 0) ? "BUTTON_SAVE" : "BUTTON_OVERWRITE";
			_buttonList.GetChild<Button>(0).Disabled = (_selectedSaveStateIndex < 0 && _saveName.Text.Trim() == string.Empty) || CheckForExistingName();
			_buttonList.GetChild<Button>(1).Disabled = (_selectedSaveStateIndex < 0);
		}

		private void UpdateSelectedSaveData()
		{
			if (_selectedSaveStateIndex < 0)
			{
				_selectedSaveName.Text = string.Empty;
				_selectedSaveMap.Text = string.Empty;
				_selectedSaveDate.Text = string.Empty;
			}
			else
			{
				string[] saveData = FileOperations.GetSaveData(_saveList.GetChild<Button>(_selectedSaveStateIndex).Text);
				_selectedSaveName.Text = saveData[0];
				_selectedSaveMap.Text = $"{TranslationServer.Translate("HEADER_LOCATION")}: {saveData[1]}";
				_selectedSaveDate.Text = $"{TranslationServer.Translate("HEADER_DATE")}: {saveData[2]}";
			}
		}

		private bool CheckForExistingName()
		{
			string trimmedNewName = _saveName.Text.Trim();

			foreach (Node child in _saveList.GetChildren())
			{
				if (child is Button saveEntry)
				{
					if (trimmedNewName == saveEntry.Text)
					{
						return true;
					}
				}
			}

			return false;
		}

		private void UnsubscribeFromEvents()
		{
			_game.UI.StateUpdated -= ToggleVisibility;
			TreeExiting -= UnsubscribeFromEvents;
		}

		private void Return()
		{
			if (_inMenu)
			{
				_game.UI.SetUIState(UIState.Misc);
			}
			else
			{
				_game.UI.SetUIState(UIState.Pause);
			}
		}
	}
}