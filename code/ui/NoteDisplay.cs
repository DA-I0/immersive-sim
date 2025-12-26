using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class NoteDisplay : Control
	{
		public event StatusUpdate NoteClosed;

		[Export] private RichTextLabel _noteContent;
		[Export] private Label _notePage;
		[Export] private Button _previousPage;
		[Export] private Button _nextPage;

		private string _activeNoteID;
		private int _page = 0;

		public override void _Ready()
		{
			GetNode<UIHandler>(ProjectSettings.GetSetting("global/UIHandlerPath").ToString()).StateUpdated += CloseNoteFromUIChange;
			GetNode<UIHandler>(ProjectSettings.GetSetting("global/UIHandlerPath").ToString()).OpenNoteUI += OpenNote;
			Visible = false;
		}

		public override void _Input(InputEvent @event)
		{
			if (@event.IsActionPressed("ui_cancel") && Visible)
			{
				CloseNote();
			}
		}

		public void OpenNote(string noteID)
		{
			if (noteID.Trim() == string.Empty)
			{
				return;
			}

			_activeNoteID = noteID;
			TogglePageButtons();
			_notePage.Text = (!_previousPage.Disabled || !_nextPage.Disabled) ? $"{_page + 1}" : string.Empty;
			_noteContent.Text = TranslationServer.Translate($"NOTE_{noteID.ToUpper()}_CONTENT_{_page}");
			Visible = true;
		}

		private void CloseNote()
		{
			HideNoteDisplay();
			NoteClosed?.Invoke();
		}

		private void HideNoteDisplay()
		{
			_activeNoteID = string.Empty;
			_page = 0;
			Visible = false;
		}

		private void ChangePage(int direction)
		{
			_page += direction;

			if (_page < 0)
			{
				_page = 0;
			}

			OpenNote(_activeNoteID);
		}

		private void TogglePageButtons()
		{
			string pageString = $"NOTE_{_activeNoteID.ToUpper()}_CONTENT_{_page - 1}";
			_previousPage.Visible = (TranslationServer.Translate(pageString) != pageString);
			_previousPage.Disabled = !_previousPage.Visible;

			pageString = $"NOTE_{_activeNoteID.ToUpper()}_CONTENT_{_page + 1}";
			_nextPage.Visible = (TranslationServer.Translate(pageString) != pageString);
			_nextPage.Disabled = !_nextPage.Visible;
		}

		private void CloseNoteFromUIChange(UIState newState)
		{
			HideNoteDisplay();
		}
	}
}
