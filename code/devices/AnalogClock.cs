using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class AnalogClock : Node3D
	{
		private Node3D _hourHand;
		private Node3D _minuteHand;

		public override void _Ready()
		{
			_hourHand = GetNode<Node3D>("HourHand");
			_minuteHand = GetNode<Node3D>("MinuteHand");
			GetNode<TimeController>(ProjectSettings.GetSetting("global/TimeControllerPath").ToString()).DateUpdated += UpdateTimeDisplay;
			TreeExiting += UnsubscribeFromEvents;
		}

		private void UpdateTimeDisplay(System.DateTime newTime)
		{
			float newAngle = (newTime.Hour / 24f) * 360f;

			Vector3 rotationHelper = _hourHand.RotationDegrees;
			rotationHelper.X = -(newAngle + 90f);
			_hourHand.RotationDegrees = rotationHelper;

			newAngle = (newTime.Minute / 60f) * 360f;

			rotationHelper = _minuteHand.RotationDegrees;
			rotationHelper.X = -(newAngle + 270f);
			_minuteHand.RotationDegrees = rotationHelper;
		}

		private void UnsubscribeFromEvents()
		{
			GetNode<TimeController>(ProjectSettings.GetSetting("global/TimeControllerPath").ToString()).DateUpdated -= UpdateTimeDisplay;
		}
	}
}