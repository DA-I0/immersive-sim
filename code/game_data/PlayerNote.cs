namespace ImmersiveSim.GameData
{
	public class PlayerNote
	{
		private readonly System.Guid _id;
		private string _title;
		private string _content;

		public System.Guid ID
		{
			get { return _id; }
		}

		public string Title
		{
			get { return _title; }
		}

		public string Content
		{
			get { return _content; }
		}

		public PlayerNote(string id, string title, string content)
		{
			_id = (id == string.Empty) ? System.Guid.NewGuid() : System.Guid.Parse(id);
			_title = title;
			_content = content;
		}

		public void UpdateNote(string newTitle, string newContent)
		{
			Godot.GD.Print(">> updating note: " + Title);
			_title = newTitle;
			_content = newContent;
			Godot.GD.Print(">>> new title: " + Title);
			Godot.GD.Print(">>> new content: " + Content);
		}
	}
}