using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class ElectronicDeviceSwitch : ToggleSwitch
	{
		[Export] private HSlider _slider;

		public override void _Ready()
		{
			_slider.Value = ((Light)_connectedDevices[0]).Brightness;
		}

		public void AdjustDeviceValue(float value)
		{
			foreach (Node3D device in _connectedDevices)
			{
				if (device is Light)
				{
					((Light)device).Brightness = (float)_slider.Value;
				}
			}
		}
	}
}