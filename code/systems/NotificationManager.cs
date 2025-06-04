namespace ImmersiveSim.Systems
{
	public class NotificationManager
	{
		private GameSystem _game;

		public NotificationManager(GameSystem game)
		{
			_game = game;
		}

		public void TriggerNotification(string type, string parameter = "")
		{
			string notificationContent = string.Empty;

			switch (type)
			{
				case "game_save":
					notificationContent = Godot.TranslationServer.Translate("NOTIFICATION_SAVE");
					break;

				case "game_load":
					notificationContent = Godot.TranslationServer.Translate("NOTIFICATION_LOAD");
					break;

				case "item_pickup":
					notificationContent = $"{Godot.TranslationServer.Translate("NOTIFICATION_ITEM_PICKUP")} {parameter}";
					break;

				case "item_drop":
					notificationContent = $"{Godot.TranslationServer.Translate("NOTIFICATION_ITEM_DROP")} {parameter}";
					break;

				case "item_equip":
					notificationContent = $"{Godot.TranslationServer.Translate("NOTIFICATION_ITEM_EQUIP")} {parameter}";
					break;

				case "item_unequip":
					notificationContent = $"{Godot.TranslationServer.Translate("NOTIFICATION_ITEM_UNEQUIP")} {parameter}";
					break;

				case "money_change":
					notificationContent = $"{Godot.TranslationServer.Translate("NOTIFICATION_MONEY_CHANGE")} {Godot.TranslationServer.Translate("SHOP_CURRENCY")}{parameter}";
					break;

				default:
					break;
			}

			_game.UI.DisplayNotification(notificationContent);
		}
	}
}