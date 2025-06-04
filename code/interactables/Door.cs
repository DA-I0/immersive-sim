using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class Door : Node3D, Interfaces.IDevice, Interfaces.IInteractable
	{
		public event DoorStatusHandler StateUpdated;

		[Export] private DoorAxis _doorAxis;
		[Export] private float _openRange;
		[Export] private float _speed = 0.025f;
		[Export] private bool _open = false;
		[Export] private bool _locked = false;
		[Export] private bool _useInNPCNavigation = false;

		private Vector3 _closePosition;
		private Vector3 _closeAngle;
		private bool _inUse = false;

		private float _openProgress = 0f;

		private Timer _autoResetTimer = new Timer();

		public override void _Ready()
		{
			_closePosition = Position;
			_closeAngle = Rotation;
			AddChild(_autoResetTimer);
			_autoResetTimer.Timeout += TurnOff;
			SetDeviceState(ChangeStateToInt(_open));
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_inUse)
			{
				if (!_open)
				{
					ManipulateEntity(-_speed);
				}
				else
				{
					ManipulateEntity(_speed);
				}
			}
		}

		public void SetDeviceState(int newState)
		{
			if (_inUse)
			{
				return;
			}

			if (!_open && _locked)
			{
				return;
			}

			_open = (newState > 0);
			_inUse = true;
		}

		public void ToggleDeviceState()
		{
			SetDeviceState(ChangeStateToInt(!_open));
		}

		public void ToggleNPCInteraction()
		{
			if (!_useInNPCNavigation)
			{
				return;
			}

			SetDeviceState(ChangeStateToInt(true));
			_autoResetTimer.Start(3f);
		}

		public void SetLockState(int newState)
		{
			if (!_open)
			{
				_locked = (newState > 0);
			}

			BroadcastDoorStatus();
		}

		public void ToggleLockStatus()
		{
			SetLockState(ChangeStateToInt(!_locked));
		}

		private void CheckProgress()
		{
			if (_openProgress <= 0f)
			{
				_inUse = false;
				_openProgress = 0;
				BroadcastDoorStatus();
			}

			if (_openProgress >= 1f)
			{
				_inUse = false;
				_openProgress = 1;
				BroadcastDoorStatus();
			}
		}

		private void ManipulateEntity(float amount)
		{
			_openProgress += amount;
			CheckProgress();

			if ((int)_doorAxis < 3)
				SmoothRotate();
			else
				SmoothMove();
		}

		private void SmoothRotate()
		{
			Vector3 temp = Rotation;

			switch (_doorAxis)
			{
				case DoorAxis.rotateX:
					temp.X = Mathf.DegToRad(_closeAngle.X + _openRange * _openProgress);
					break;

				case DoorAxis.rotateY:
					temp.Y = Mathf.DegToRad(_closeAngle.Y + _openRange * _openProgress);
					break;

				case DoorAxis.rotateZ:
					temp.Z = Mathf.DegToRad(_closeAngle.Z + _openRange * _openProgress);
					break;
			}

			Rotation = temp;
		}

		private void SmoothMove()
		{
			Vector3 temp = Position;

			switch (_doorAxis)
			{
				case DoorAxis.moveX:
					temp.X = _closePosition.X + _openRange * _openProgress;
					break;

				case DoorAxis.moveY:
					temp.Y = _closePosition.Y + _openRange * _openProgress;
					break;

				case DoorAxis.moveZ:
					temp.Z = _closePosition.Z + _openRange * _openProgress;
					break;
			}

			Position = temp;
		}

		private void BroadcastDoorStatus()
		{
			StateUpdated?.Invoke((int)_openProgress, ChangeStateToInt(_locked));
		}

		private int ChangeStateToInt(bool newState)
		{
			return newState ? 1 : 0;
		}

		private void TurnOff()
		{
			SetDeviceState(ChangeStateToInt(false));
		}

		public void TriggerPrimaryInteraction(Inventory user)
		{
			ToggleDeviceState();
		}

		public void TriggerSecondaryInteraction(Inventory user)
		{
			ToggleDeviceState(); // change to locking?
		}
	}
}