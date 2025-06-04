using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class DigitalClock : Node3D
	{
		private Label _timeLabel;

		public override void _Ready()
		{
			_timeLabel = GetNode<Label>("DisplayViewport/TimeDisplay");

			Mesh displayMesh = GetNode<MeshInstance3D>("Display").Mesh;
			Texture2D displayTexture = GetNode<SubViewport>("DisplayViewport").GetTexture();
			((BaseMaterial3D)displayMesh.SurfaceGetMaterial(0)).AlbedoTexture = displayTexture;
			((BaseMaterial3D)displayMesh.SurfaceGetMaterial(0)).EmissionTexture = displayTexture;

			GetNode<TimeController>(ProjectSettings.GetSetting("global/TimeControllerPath").ToString()).DateUpdated += UpdateTimeDisplay;
			TreeExiting += UnsubscribeFromEvents;
		}

		private void UpdateTimeDisplay(System.DateTime newTime)
		{
			_timeLabel.Text = Statics.HelperMethods.GetFormattedTime(newTime);
		}

		private void UnsubscribeFromEvents()
		{
			GetNode<TimeController>(ProjectSettings.GetSetting("global/TimeControllerPath").ToString()).DateUpdated -= UpdateTimeDisplay;
		}
	}
}