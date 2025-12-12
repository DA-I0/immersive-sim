using System;
using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class Radio : Node3D, Interfaces.IDevice
	{
		public event StatusUpdate RadioUpdated;

		[Export] private AudioStream[] _audioContent; // move it into a separate "database" and call straight from there? No need to set up the audio list for each prefab individually.

		[Export] private int _initialState;
		private int _currentState;
		private float _volume = 1f;
		private float _baseVolumeDb;
		private int _currentStation = 0;
		private AudioStreamPlayer3D _audioSource;
		private MeshInstance3D _display;

		public string Station
		{
			get { return HelperMethods.RemoveExtention(_audioSource.Stream.ResourcePath.Split('/')[^1]); }
		}

		public float Volume
		{
			get { return _volume; }
		}

		public override void _Ready()
		{
			_audioSource = GetNode<AudioStreamPlayer3D>("AudioSource");
			_baseVolumeDb = _audioSource.VolumeDb;
			_display = GetNode<MeshInstance3D>("Screen/Display");
			SetDeviceState(_initialState);
			RandomizeSetup();
		}

		public void SetDeviceState(int newState)
		{
			_currentState = newState;
			ChangeToStation(_currentStation);
			_display.Visible = (_currentState > 0);
		}

		public void ToggleDeviceState()
		{
			int newState = (_currentState > 0) ? -1 : 1;
			SetDeviceState(newState);
		}

		public void ChangeStation(bool cycleForward)
		{
			if (cycleForward)
			{
				ChangeToStation(_currentStation + 1);
			}
			else
			{
				ChangeToStation(_currentStation - 1);
			}
		}

		public void AdjustVolume(float newVolume)
		{
			_volume = Math.Clamp(newVolume, StaticValues.VolumeRadioMin, StaticValues.VolumeRadioMax);
			ApplyVolume();
		}

		private void RandomizeSetup()
		{
			int randomValue = GD.RandRange(StaticValues.VolumeRadioMin, StaticValues.VolumeRadioMax);
			AdjustVolume(randomValue);
			randomValue = GD.RandRange(0, _audioContent.Length - 1);
			ChangeToStation(randomValue);
		}

		private void ChangeToStation(int index)
		{
			_currentStation = index;

			if (index < 0)
			{
				_currentStation = _audioContent.Length - 1;
			}

			if (index > _audioContent.Length - 1)
			{
				_currentStation = 0;
			}

			float playbackPosition = (_audioSource.Stream != null) ? _audioSource.GetPlaybackPosition() / (float)_audioSource.Stream.GetLength() : 0;
			_audioSource.Stream = _audioContent[_currentStation];
			float newSourceOffset = (float)_audioSource.Stream.GetLength() * playbackPosition;
			_audioSource.Play(newSourceOffset);
			ApplyVolume();
			RadioUpdated?.Invoke();
		}

		private void ApplyVolume()
		{
			if (_audioSource == null)
			{
				return;
			}

			_audioSource.VolumeDb = (_currentState > 0) ? _volume : -100; // might have add baseVolume variable and multiply by _volume
		}
	}
}