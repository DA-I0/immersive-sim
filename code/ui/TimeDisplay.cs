using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class TimeDisplay : Control
	{
		private Label _date;
		private Label _time;

		public override void _Ready()
		{
			_date = GetNode<Label>("TimeControls/Date");
			_time = GetNode<Label>("TimeControls/Time");
			GetNode<Gameplay.TimeController>(ProjectSettings.GetSetting("global/TimeControllerPath").ToString()).DateUpdated += UpdateDateTimeInfo;
			GetParent<UIHandler>().StateUpdated += ToggleDisplay;
		}

		private void UpdateDateTimeInfo(System.DateTime newDate)
		{
			_date.Text = $"{newDate.DayOfWeek}";
			_time.Text = $"{HelperMethods.GetFormattedTime(newDate)}";
		}

		private void ToggleDisplay(UIState activeState)
		{
			Visible = (activeState == UIState.None);
		}
	}
}