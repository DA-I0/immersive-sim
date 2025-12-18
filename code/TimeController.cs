using System;
using Godot;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay
{
	public partial class TimeController : Node
	{
		public event DateChange DateUpdated;

		[Export] private DirectionalLight3D _sun;
		[Export] private DirectionalLight3D _moon;

		[ExportGroup("Date")]
		[Export] private int _startingYear = 2020;
		[Export] private int _startingMonth = 1;
		[Export] private int _startingDay = 1;
		[Export] private int _startingHour = 7;
		[Export] private int _timeScale = 8;

		[ExportGroup("Time of day")]
		// [Export] private TimeOfDayPreset[] _tods;

		[Export] private Gradient _sunColor;
		[Export] Curve _sunIntensity;
		[Export] private Gradient _moonColor;
		[Export] Curve _moonIntensity;
		[Export] Curve _shadowStrength;

		private System.DateTime _gameDate;
		private GameSystem _game;

		public System.DateTime CurrentDate
		{
			get { return _gameDate; }
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());

			SetDate(new DateTime(_startingYear, _startingMonth, _startingDay, _startingHour, 0, 0));
		}

		public void UpdateWorldTime()
		{
			if (_game.State != GameState.Gameplay)
			{
				return;
			}

			_gameDate = _gameDate.AddSeconds(_timeScale);

			MoveSun();
			UpdateSunParameters();
			DateUpdated?.Invoke(_gameDate);
		}

		public void AdvanceTime(int hours, int minutes)
		{
			_gameDate = _gameDate.AddHours(hours);
			_gameDate = _gameDate.AddMinutes(minutes);
			UpdateSunPosition();
			DateUpdated?.Invoke(_gameDate);
		}

		public void TimeSet(string hours, string minutes)
		{
			int newHour = 0;
			int.TryParse(hours, out newHour);
			int newMinutes = 0;
			int.TryParse(minutes, out newMinutes);

			TimeSet(newHour, newMinutes);
		}

		public void TimeSet(int hour, int minutes)
		{
			// dim the screen
			SetDate(new DateTime(_gameDate.Year, _gameDate.Month, _gameDate.Day, hour, minutes, 0));
			// show the screen
		}

		public void TimeSkip(string hours, string minutes)
		{
			int hoursDuration = 0;
			int.TryParse(hours, out hoursDuration);
			int minutesDuration = 0;
			int.TryParse(minutes, out minutesDuration);

			TimeSkip(hoursDuration, minutesDuration);
		}

		public void TimeSkip(int hours, int minutes)
		{
			// dim the screen
			AdvanceTime(hours, minutes);
			// show the screen
		}

		public bool CompareTime(string operation, string targetHours, string targetMinutes)
		{
			float targetHoursValue;
			float.TryParse(targetHours, out targetHoursValue);
			float targetMinutesValue;
			float.TryParse(targetMinutes, out targetMinutesValue);

			bool result = false;

			switch (operation)
			{
				case "gt":
					if (_gameDate.Hour > targetHoursValue)
					{
						return true;
					}
					else
					{
						return (_gameDate.Hour == targetHoursValue && _gameDate.Minute > targetMinutesValue);
					}

				case "lt":
					if (_gameDate.Hour < targetHoursValue)
					{
						return true;
					}
					else
					{
						return (_gameDate.Hour == targetHoursValue && _gameDate.Minute < targetMinutesValue);
					}

				case "eq":
					return (_gameDate.Hour == targetHoursValue && _gameDate.Minute == targetMinutesValue);

				default:
					break;
			}

			return result;
		}

		public void RestoreSavedState(ConfigFile saveState)
		{
			SetDate(DateTime.Parse((string)saveState.GetValue("World", "current_date", _gameDate.ToString())));
		}

		private void SetDate(DateTime newDate)
		{
			_gameDate = newDate;
			UpdateSunPosition();
			DateUpdated?.Invoke(_gameDate);
		}

		private void UpdateSunPosition()
		{
			float fixedAngle = (15 * _gameDate.Hour) + 90;

			_sun.RotationDegrees = new Vector3(fixedAngle, 0f, 0f);
			_moon.RotationDegrees = new Vector3(-fixedAngle, 0f, 0f);
		}

		private void MoveSun()
		{
			_sun.Rotate(new Vector3(1, 0, 0), Mathf.DegToRad(0.0041f));
			_moon.Rotate(new Vector3(1, 0, 0), Mathf.DegToRad(0.0041f));
		}

		private void UpdateSunParameters()
		{
			float dayProgress = (_gameDate.Hour + _gameDate.Minute / 60f) / 24f;

			_sun.LightColor = _sunColor.Sample(dayProgress);
			_sun.LightEnergy = _sunIntensity.Sample(dayProgress);
			_sun.ShadowOpacity = _shadowStrength.Sample(dayProgress);

			_moon.LightColor = _moonColor.Sample(dayProgress);
			_moon.LightEnergy = _moonIntensity.Sample(dayProgress);
			_sun.ShadowOpacity = _shadowStrength.Sample(dayProgress);

			_sun.Visible = _sun.LightEnergy > _moon.LightEnergy;
			_moon.Visible = _moon.LightEnergy > _sun.LightEnergy;
		}
	}
}