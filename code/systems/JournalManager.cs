using System.Collections.Generic;
using ImmersiveSim.GameData;

namespace ImmersiveSim.Systems
{
	public class JournalManager
	{
		private Dictionary<System.Guid, PlayerNote> _playerNotes = new Dictionary<System.Guid, PlayerNote>();

		public Dictionary<System.Guid, PlayerNote> PlayerNotes
		{
			get { return _playerNotes; }
		}

		public System.Guid CreateNote(string title, string content, string id = "")
		{
			id = (id == "") ? string.Empty : id;
			PlayerNote newNote = new PlayerNote(id, title, content);
			PlayerNotes.Add(newNote.ID, newNote);
			return newNote.ID;
		}

		public void UpdateNote(System.Guid id, string title, string content)
		{
			if (id != System.Guid.Empty && PlayerNotes.ContainsKey(id))
			{
				PlayerNotes[id].UpdateNote(title, content);
			}
			else
			{
				CreateNote(title, content);
			}

			foreach (PlayerNote nextNote in PlayerNotes.Values)
			{
				Godot.GD.Print("== note > " + nextNote.Title);
			}
		}

		public void DeleteNote(System.Guid id)
		{
			PlayerNotes.Remove(id);
		}
	}
}