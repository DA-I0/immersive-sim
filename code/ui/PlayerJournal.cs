using System.Linq;
using Godot;
using ImmersiveSim.GameData;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class PlayerJournal : Control
	{
		[Export] private Control _noteList;
		[Export] private LineEdit _noteTitle;
		[Export] private TextEdit _noteContent;
		[Export] private TextureButton _deleteButton;

		private System.Guid _activeNoteID = System.Guid.Empty;

		private UIManager _ui;
		private Systems.JournalManager _journal;

		public override void _Ready()
		{
			_ui = GetParent().GetParent<UIManager>();
			_journal = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).Journal;
			SubscribeToEvents();
		}

		public override void _Input(InputEvent @event)
		{
			if (_ui.LockKeybindings)
			{
				return;
			}

			if (@event.IsActionPressed("action_journal"))
			{
				_ui.SetUIState(UIState.Journal);
			}
		}

		private void ToggleJournal()
		{
			_ui.SetUIState(UIState.Journal);
		}

		public void ToggleJournal(UIState currentState)
		{
			Visible = (currentState == UIState.Journal);

			if (Visible)
			{
				UpdateNoteList();
			}
		}

		private void UpdateNoteList()
		{
			ClearNoteList();

			foreach (PlayerNote nextNote in _journal.PlayerNotes.Values.OrderBy(note => note.Title))
			{
				Button noteEntry = CreateNoteEntryButton(nextNote.Title, nextNote.ID);
				_noteList.AddChild(noteEntry);
			}

			ToggleDeleteButton();
		}

		private void ClearNoteList()
		{
			foreach (Node child in _noteList.GetChildren())
			{
				if (child is Button noteEntry)
				{
					noteEntry.QueueFree();
				}
			}
		}

		private Button CreateNoteEntryButton(string saveName, System.Guid id)
		{
			Button noteEntryButton = new Button
			{
				Text = saveName,
				Alignment = HorizontalAlignment.Left,
			};

			noteEntryButton.Pressed += () => OpenNote(id);

			return noteEntryButton;
		}

		private void OpenNote(System.Guid id)
		{
			_activeNoteID = id;

			GameData.PlayerNote activeNote = _journal.PlayerNotes[id];
			_noteTitle.Text = activeNote.Title;
			_noteContent.Text = activeNote.Content;
			ToggleDeleteButton();
		}

		private void CreateNote()
		{
			_activeNoteID = System.Guid.Empty;
			_noteTitle.Text = string.Empty;
			_noteContent.Text = string.Empty;
		}

		private void SaveNote()
		{
			if (_activeNoteID != System.Guid.Empty)
			{
				GD.Print("updating note: " + _activeNoteID + ", title: " + _journal.PlayerNotes[_activeNoteID].Title);
				_journal.UpdateNote(_activeNoteID, _noteTitle.Text, _noteContent.Text);
			}
			else
			{
				_activeNoteID = _journal.CreateNote(_noteTitle.Text, _noteContent.Text);
			}

			UpdateNoteList();
		}

		private void DeleteNote()
		{
			if (_activeNoteID != System.Guid.Empty)
			{
				_journal.DeleteNote(_activeNoteID);
				CreateNote();
				UpdateNoteList();
			}
		}

		private void SubscribeToEvents()
		{
			_ui.StateUpdated += ToggleJournal;
		}

		private void ToggleDeleteButton()
		{
			_deleteButton.Disabled = (_activeNoteID == System.Guid.Empty);
		}
	}
}