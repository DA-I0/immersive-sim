using Godot;

namespace ImmersiveSim.UI
{
	public partial class ElectronicLock : Control
	{
		[Export] private Texture2D[] _textures;
		[Export] private Color[] _textureColors;
		[Export] private TextureButton _mainButton;
		[Export] private TextureButton _lockButton;
		[Export] private TextureButton _cameraButton;
		[Export] private Gameplay.Door _targetDoor;

		[Export] private Camera3D _cameraSource;
		[Export] private TextureRect _cameraFeed;

		public override void _Ready()
		{
			_targetDoor = GetNode<Gameplay.Door>("../../../../door_entity");
			_targetDoor.StateUpdated += UpdateDoorStatus;

			if (_cameraSource != null)
			{
				_cameraFeed.Texture = _cameraSource.GetParent<Viewport>().GetTexture();
				SetCameraView(false);
			}
		}

		private void UpdateDoorStatus(int isDoorOpen, int isDoorLocked)
		{
			_mainButton.TextureNormal = _textures[isDoorOpen];
			_mainButton.Modulate = _textureColors[isDoorOpen];

			int colorIndex = (isDoorLocked > 0) ? 0 : 1; // temp solution
			_lockButton.Modulate = _textureColors[colorIndex];
		}

		private void ToggleDoor()
		{
			_targetDoor.ToggleDeviceState();
		}

		private void ToggleLock()
		{
			_targetDoor.ToggleLockStatus();
		}

		private void ToggleCameraView()
		{
			SetCameraView(!_cameraSource.Visible);
		}

		private void SetCameraView(bool state)
		{
			if (_cameraSource != null)
			{
				_cameraSource.Visible = state;
			}

			_cameraFeed.Visible = state;
			_mainButton.Visible = !state;
		}
	}
}