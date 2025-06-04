using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class Elevator : AnimatableBody3D, Interfaces.IDevice
	{
		[Export] private float _speed = 0.025f;
		[Export] private int _startingStop = 0;
		[Export] private Node3D[] _stops;

		private bool _inUse = false;
		private int _nextStop;

		public override void _PhysicsProcess(double delta)
		{
			if (_inUse)
			{
				Move();
			}
		}

		public void SetDeviceState(int newState)
		{
			if (_inUse)
			{
				return;
			}

			if (newState >= 0 && newState < _stops.Length)
			{
				_nextStop = newState;
				_inUse = true;
			}
		}

		public void ToggleDeviceState()
		{
			int newState = _nextStop++;

			if (newState > _stops.Length)
			{
				newState = 0;
			}

			SetDeviceState(newState);
		}

		private void CheckIfArrived()
		{
			if (Position.DistanceTo(_stops[_nextStop].Position) < 0.1f)
			{
				_inUse = false;
			}
		}

		private void Move()
		{
			CheckIfArrived();

			Vector3 temp = Position;
			temp = temp.Lerp(_stops[_nextStop].Position, _speed);

			Position = temp;
		}
	}
}