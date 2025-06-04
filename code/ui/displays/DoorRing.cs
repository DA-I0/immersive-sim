using Godot;

namespace ImmersiveSim.UI
{
	public partial class DoorRing : Control
	{
		[Export] private string _doorKeyId; // use it to check if player has the key when equipment is a thing?
		[Export] private Gameplay.Door _targetDoor;
		[Export] private AudioStreamPlayer3D _audioSource;

		private void ToggleDoor()
		{
			_targetDoor.ToggleDeviceState();
		}

		private void TriggerRing()
		{
			_audioSource.Play();
		}
	}
}