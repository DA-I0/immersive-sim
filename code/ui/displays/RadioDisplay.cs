using Godot;
using ImmersiveSim.Gameplay;

namespace ImmersiveSim.UI
{
	public partial class RadioDisplay : Control
	{
		private Label _stationName;
		private Slider _volume;

		private Radio _targetDevice;

		public override void _Ready()
		{
			_stationName = GetNode<Label>("StationName");
			_volume = GetNode<Slider>("VolumeSlider");
			_targetDevice = GetNode<Radio>("../..");
			_targetDevice.RadioUpdated += UpdateDisplay;

			_volume.MinValue = Statics.StaticValues.VolumeRadioMin;
			_volume.MaxValue = Statics.StaticValues.VolumeRadioMax;
		}

		private void UpdateDisplay()
		{
			_stationName.Text = TranslationServer.Translate($"AUDIO_{_targetDevice.Station.ToUpper()}");
			_volume.SetValueNoSignal(_targetDevice.Volume);
		}

		private void ChangeStation(bool cycleForward)
		{
			_targetDevice.ChangeStation(cycleForward);
		}

		private void AdjustVolume(float newVolume)
		{
			_targetDevice.AdjustVolume(newVolume);
		}
	}
}