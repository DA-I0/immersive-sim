using System;
using Godot;
using ImmersiveSim.GameData;
using ImmersiveSim.Gameplay;

namespace ImmersiveSim.Statics
{
	public static class HelperMethods
	{
		public static float RoundFloat(float number, int decimals = 2)
		{
			int decimalHelper = (int)Math.Pow(10, decimals);

			return Mathf.Round(number * decimalHelper) / decimalHelper;
		}

		public static string GetFormattedTime(DateTime date)
		{
			return date.ToString("HH:mm");
		}

		public static string GetFormattedPrice(int value)
		{
			return $"{value}{TranslationServer.Translate("SHOP_CURRENCY")}";
		}

		public static string ItemIconPath(Gameplay.BaseItem item)
		{
			string filePath = $"{ProjectSettings.GetSetting("global/IconsFolder")}/items/{item.Category}/{item.TemplateID}.png".ToLower();

			if (!ResourceLoader.Exists(filePath))
			{
				filePath = filePath.Replace($"{item.TemplateID}.png", "default_icon.png");

				if (!ResourceLoader.Exists(filePath))
				{
					filePath = $"{ProjectSettings.GetSetting("global/IconsFolder")}/missing_icon.png";
				}
			}

			return filePath;
		}

		public static Vector3 GetShapeDimensions(Shape3D target)
		{
			Vector3 shapeDimensions = Vector3.Zero;

			if (target is BoxShape3D box)
			{
				shapeDimensions = box.Size;
			}

			if (target is CylinderShape3D cylinder)
			{
				shapeDimensions.X = cylinder.Radius;
				shapeDimensions.Y = cylinder.Height;
				shapeDimensions.Z = cylinder.Radius;
			}

			// add custom shape check?

			return shapeDimensions;
		}

		public static string RemoveExtention(string sourceString)
		{
			string fileExtention = $".{sourceString.Split('.')[^1]}";
			return sourceString.Replace(fileExtention, string.Empty);
		}

		public static Vector2 AdjustDisplayPosition(Vector2 objectPosition, Vector2 objectSize)
		{
			Vector2 adjustedPosition = objectPosition;

			float horizontalEndpoint = objectPosition.X + objectSize.X;
			float screenWidth = DisplayServer.ScreenGetSize().X;

			if (horizontalEndpoint > screenWidth)
			{
				adjustedPosition.X = objectPosition.X - (horizontalEndpoint - screenWidth);
			}

			float verticalEndpoint = objectPosition.Y + objectSize.Y;
			float screenHeight = DisplayServer.ScreenGetSize().Y;

			if (verticalEndpoint > screenHeight)
			{
				adjustedPosition.Y = objectPosition.Y - (verticalEndpoint - screenHeight);
			}

			return adjustedPosition;
		}

		public static ItemData ParseBaseItemData(ConfigFile itemConfig)
		{
			return new ItemData(
				(string)itemConfig.GetValue(string.Empty, "id"),
				(int)itemConfig.GetValue(string.Empty, "value", 1),
				(int)itemConfig.GetValue(string.Empty, "primary_interaction", (int)InteractionType.Use),
				(int)itemConfig.GetValue(string.Empty, "secondary_interaction", (int)InteractionType.Use),
				(int)itemConfig.GetValue(string.Empty, "base_size", 1),
				(float)itemConfig.GetValue(string.Empty, "base_weight", 1),
				(int)itemConfig.GetValue(string.Empty, "category", (int)ItemCategory.Misc),
				(int)itemConfig.GetValue(string.Empty, "equipment_slot", (int)EquipmentSlot.Selected),
				(bool)itemConfig.GetValue(string.Empty, "is_pickable", false),
				(bool)itemConfig.GetValue(string.Empty, "is_usable", false),
				(bool)itemConfig.GetValue(string.Empty, "is_stackable", false),
				(bool)itemConfig.GetValue(string.Empty, "can_be_stowed", true),
				(bool)itemConfig.GetValue(string.Empty, "can_be_hanged", false),
				(int)itemConfig.GetValue(string.Empty, "uses", -1),
				(float)itemConfig.GetValue(string.Empty, "use_weight", -1),
				(int)itemConfig.GetValue(string.Empty, "health_recovery", 0),
				(int)itemConfig.GetValue(string.Empty, "stamina_recovery", 0),
				(int)itemConfig.GetValue(string.Empty, "capacity", -1),
				(float)itemConfig.GetValue(string.Empty, "item_weight_multiplier", 1),
				(bool)itemConfig.GetValue(string.Empty, "is_hard", true),
				(string[])itemConfig.GetValue(string.Empty, "effects", string.Empty)
				);
		}

		public static StoreData ParseBaseStoreData(ConfigFile itemConfig)
		{
			return new StoreData(
				(string)itemConfig.GetValue(string.Empty, "id"),
				(string[])itemConfig.GetValue(string.Empty, "products", new string[0])
				);
		}

		public static ConversationNode ParseConversationNodeData(ConfigFile itemConfig)
		{
			return new ConversationNode(
				(string)itemConfig.GetValue(string.Empty, "id"),
				(string)itemConfig.GetValue(string.Empty, "generic_text_id", string.Empty),
				(string[])itemConfig.GetValue(string.Empty, "requirements", string.Empty),
				(string[])itemConfig.GetValue(string.Empty, "effects", string.Empty),
				(string)itemConfig.GetValue(string.Empty, "next_node_id"),
				(string[])itemConfig.GetValue(string.Empty, "player_reply_ids")
				);
		}

		public static CharacterSaveState PrepareCharacterSaveState(CharacterBase character)
		{
			string[] equipmentItems = new string[character.CharInventory._equipmentSlots.Length];

			for (int index = 0; index < equipmentItems.Length; index++)
			{
				equipmentItems[index] = (character.CharInventory._equipmentSlots[index] != null) ? character.CharInventory._equipmentSlots[index].ObjectID : string.Empty;
			}

			return new CharacterSaveState(
				character.GetParent().GetPath(),
				character.ID,
				character.GetParent<MovementBase>().GlobalPosition,
				character.GetParent<MovementBase>().GlobalRotation,
				character.GetParent<MovementBase>().Velocity,
				character.CharStatus.MaxHealth,
				character.CharStatus.Health,
				character.CharStatus.LastRestDate,
				character.CharStatus.Stamina,
				character.CharInventory.Money,
				equipmentItems
			);
		}

		public static CharacterSaveState ParseCharacterSaveState(ConfigFile saveState, string targetSection)
		{
			return new CharacterSaveState(
				(string)saveState.GetValue(targetSection, "node_path"),
				(string)saveState.GetValue(targetSection, "id"),
				(Vector3)saveState.GetValue(targetSection, "position"),
				(Vector3)saveState.GetValue(targetSection, "rotation"),
				(Vector3)saveState.GetValue(targetSection, "velocity"),
				(float)saveState.GetValue(targetSection, "max_health", 100f),
				(float)saveState.GetValue(targetSection, "health", 100f),
				DateTime.Parse((string)saveState.GetValue(targetSection, "last_rest_date")),
				(float)saveState.GetValue(targetSection, "stamina", 100f),
				(int)saveState.GetValue(targetSection, "money", 0),
				(string[])saveState.GetValue(targetSection, "equipment_items", Array.Empty<string>())
			);
		}

		public static ItemSaveState PrepareItemSaveState(BaseItem item)
		{
			string parentContainer = item.ObjectID;//ParentContainerPath;

			int usesLeft = -1;

			if (item is Consumable consumable)
			{
				usesLeft = consumable.UsesLeft;
			}

			string[] itemsWithin = Array.Empty<string>();

			if (item is Gameplay.Container container)
			{
				if (container.ItemsWithin.Count > 0)
				{
					itemsWithin = new string[container.ItemsWithin.Count];

					for (int index = 0; index < container.ItemsWithin.Count; index++)
					{
						itemsWithin[index] = container.ItemsWithin[index]?.ObjectID;
					}
				}
			}

			return new ItemSaveState(
				item.GetPath(),
				item.ObjectID,
				item.TemplateID,
				item.CustomName,
				item.IsInstancedObject,
				false,
				item.GlobalPosition,
				item.GlobalRotation,
				item.LinearVelocity,
				item.IsActive,
				item.Freeze,
				item.CollisionsDisabled,
				item.StackCount,
				parentContainer,
				usesLeft,
				itemsWithin,
				item.IsEquipped
			);
		}

		public static ItemSaveState ParseItemSaveState(ConfigFile saveState, string targetSection)
		{
			return new ItemSaveState(
				(string)saveState.GetValue(targetSection, "current_node_path"),
				(string)saveState.GetValue(targetSection, "object_id"),
				(string)saveState.GetValue(targetSection, "template_id"),
				(string)saveState.GetValue(targetSection, "custom_name", string.Empty),
				(bool)saveState.GetValue(targetSection, "is_instanced_object", true),
				(bool)saveState.GetValue(targetSection, "is_destroyed", false),
				(Vector3)saveState.GetValue(targetSection, "position"),
				(Vector3)saveState.GetValue(targetSection, "rotation"),
				(Vector3)saveState.GetValue(targetSection, "velocity"),
				(bool)saveState.GetValue(targetSection, "is_visible", true),
				(bool)saveState.GetValue(targetSection, "is_frozen", false),
				(bool)saveState.GetValue(targetSection, "collissions_disabled", false),
				(int)saveState.GetValue(targetSection, "stack_count", 1),
				(string)saveState.GetValue(targetSection, "parent_container_node_path"),
				(int)saveState.GetValue(targetSection, "uses_left", -1),
				(string[])saveState.GetValue(targetSection, "items_within", Array.Empty<string>()),
				(bool)saveState.GetValue(targetSection, "is_equipped", false)
			);
		}

		public static string MakeSavePath(string saveName)
		{
			return $"{ProjectSettings.GetSetting("global/SaveFolder")}/{saveName}.save";
		}

		public static string GetLocalizedLanguage(string languageCode)
		{
			return TranslationServer.GetTranslationObject(languageCode).GetMessage("LANGUAGE_NAME");
		}

		public static int FindOptionIndex(OptionButton targetList, string optionToFind)
		{
			for (int ind = 0; ind < targetList.ItemCount; ind++)
			{
				if (targetList.GetItemText(ind).ToLower() == optionToFind.ToLower())
				{
					return ind;
				}
			}

			return -1;
		}

		public static void ParseCustomValues(string content, ref System.Collections.Generic.Dictionary<string, string> customValues)
		{
			string[] splitContent = content.Split(";");

			foreach (string line in splitContent)
			{
				string[] keyValuePair = line.Split(":");

				if (keyValuePair.Length == 2)
				{
					customValues.Add(keyValuePair[0].Trim(), keyValuePair[1].Trim());
				}
			}
		}

		// public static string GetJoypadType(string targetAction)
		// {
		// 	switch (refs.settings.ControllerPrompts)
		// 	{
		// 		case "nintendo":
		// 			return $"ni_{targetAction}";

		// 		case "playstation":
		// 			return $"ps_{targetAction}";

		// 		case "steam deck":
		// 			return $"sd_{targetAction}";

		// 		case "xbox":
		// 			return $"xb_{targetAction}";

		// 		default:
		// 			return targetAction;
		// 	}
		// }

		public static bool CompareValues(string operation, string stringValue1, string stringValue2)
		{
			bool result = true;

			float value1;
			float.TryParse(stringValue1, out value1);
			float value2;
			float.TryParse(stringValue2, out value2);

			switch (operation)
			{
				case "gt":
					result = value1 > value2;
					break;

				case "lt":
					result = value1 < value2;
					break;

				case "eq":
					result = value1 == value2;
					break;

				default:
					break;
			}

			return result;
		}
	}
}