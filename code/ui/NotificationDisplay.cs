using System.Linq;
using Godot;

namespace ImmersiveSim.UI
{
	public partial class NotificationDisplay : Control
	{
		[Export] private PackedScene _notificationPrefab;

		private Timer _timer;
		private Node _notificationList;

		private Systems.Settings _settings;

		public override void _Ready()
		{
			_notificationList = GetNode("NotificationList");
			_timer = new Timer();
			_timer.Timeout += HandleTimeout;
			AddChild(_timer);

			_settings = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).Settings;
		}

		public void TriggerNotification(string notificationContent)
		{
			Notification newNotification = _notificationPrefab.Instantiate<Notification>();
			_notificationList.AddChild(newNotification);
			newNotification.Display(notificationContent, _settings.NotificationDisplayTime);

			ClearExcessNotifications();
			TriggerTimer();
		}

		private void ClearExcessNotifications()
		{
			if (_notificationList.GetChildCount() > _settings.MaxNotificationNumber)
			{
				_notificationList.GetChild(0).QueueFree();
			}
		}

		private void HandleTimeout()
		{
			TickDownNotification();
			TriggerTimer();
		}

		private void TriggerTimer()
		{
			if (_notificationList.GetChildCount() > 0)
			{
				_timer.Start();
			}
		}

		private void TickDownNotification()
		{
			foreach (Notification notification in _notificationList.GetChildren().Cast<Notification>())
			{
				notification.TickDown();
			}
		}
	}
}