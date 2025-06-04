using Godot;

namespace ImmersiveSim.UI
{
	public partial class Notification : Control
	{
		private float _timeLeft;

		private Label _content;

		public override void _Ready()
		{
			_content = GetNode<Label>("Content");
		}

		public void Display(string contentString, float displayTime)
		{
			_content.Text = contentString;
			_timeLeft = displayTime;
		}

		public void TickDown()
		{
			_timeLeft--;

			if (_timeLeft <= 0)
			{
				QueueFree();
			}
		}
	}
}