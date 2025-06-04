using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class Light : Node3D, Interfaces.IDevice // add health system, light malfunctions when health low
	{
		[Export] private int _initialState = -1;
		[Export] private Light3D _lightSource;

		private int _currentState;
		private float _initialEnergy;

		public float Brightness
		{
			get { return (_lightSource.LightEnergy / _initialEnergy); }
			set { _lightSource.LightEnergy = _initialEnergy * value; }
		}

		public override void _Ready()
		{
			_initialEnergy = _lightSource.LightEnergy;
			InitializeDeviceState();
		}

		private void InitializeDeviceState()
		{
			SetDeviceState(_initialState);
		}

		public void SetDeviceState(int newState)
		{
			_currentState = newState;
			_lightSource.Visible = (_currentState > 0);
		}

		public void ToggleDeviceState()
		{
			SetDeviceState(-_currentState);
		}
	}
}