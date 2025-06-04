using System.Collections;
using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class Crosshair : Control
	{
		[Export] private Texture2D _defaultIcon;
		[Export] private Color _defaultColor;
		[Export] private Texture2D _interactableIcon;
		[Export] private Color _interactableColor;

		private TextureRect _texture;
		private Control _textDisplay;
		private Label _interactableText;

		private Systems.GameSystem _game;

		public override void _Ready()
		{
			FindControls();
			_game = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.NewPlayerSpawned += SetPlayerReferences;
			_game.UI.StateUpdated += ToggleVisibility;
			_game.Settings.SettingsUpdated += UpdateCrosshairSize;
		}

		private void FindControls()
		{
			_texture = GetNode<TextureRect>("Controls/CrosshairTexture");
			_textDisplay = GetNode<Control>("Controls/InteractableText");
			_interactableText = GetNode<Label>("Controls/InteractableText/Content");
		}

		private void SetPlayerReferences()
		{
			_game.Player.SetForDestruction += UnsetPlayerReferences;
			_game.Player.CharInteraction.InteractionTarget += UpdateCrosshairStatus;
		}

		private void UnsetPlayerReferences()
		{
			_game.Player.CharInteraction.InteractionTarget -= UpdateCrosshairStatus;
		}

		private void UpdateCrosshairStatus(object target)
		{
			if (target is null or not Interfaces.IInteractable)
			{
				_textDisplay.Visible = false;

				if (target is InteractiveScreen)
				{
					Input.SetMouseMode(Input.MouseModeEnum.Confined);
					_texture.Visible = false;
					return;
				}

				Input.SetMouseMode(Input.MouseModeEnum.Captured);
				_texture.Texture = _defaultIcon;
				_texture.Modulate = _defaultColor;
				_texture.Visible = true;
				return;
			}

			Input.SetMouseMode(Input.MouseModeEnum.Captured);
			UpdateInteractionPrompt(target);
			_texture.Texture = _interactableIcon;
			_texture.Modulate = _interactableColor;
			_texture.Visible = true;
		}

		bool showInteractionPrompts = true;

		private void UpdateInteractionPrompt(object target)
		{
			if (target != null)
			{
				string itemInfo = string.Empty;
				string interactionInfo = string.Empty;
				string[] interactionKeybinds = GetInteractionKeybinds();

				InsertItemInfo(ref target, ref itemInfo, ref interactionInfo, ref interactionKeybinds);
				InsertProductInfo(ref target, ref itemInfo);

				if (_game.Settings.DisplayItemInteractions && target is BaseItem)
				{
					_interactableText.Text = itemInfo + interactionInfo;
				}
				else
				{
					_interactableText.Text = $"{interactionKeybinds[0]} {itemInfo} {interactionInfo}";
				}

				_interactableText.Text = _interactableText.Text.Trim();
				_textDisplay.Visible = _interactableText.Text != string.Empty;
			}
		}

		private string[] GetInteractionKeybinds()
		{
			string[] interactionKeybinds = new string[] { string.Empty, string.Empty };

			if (_game.Settings.DisplayInteractionKeybinds)
			{
				interactionKeybinds[0] = $"[{_game.GameDatabase.GetInputSymbol("action_interact", "Keyboard")[0]}]";
				interactionKeybinds[1] = $"[{_game.GameDatabase.GetInputSymbol("action_interact_secondary", "Keyboard")[0]}]";
			}

			return interactionKeybinds;
		}

		private void InsertItemInfo(ref object target, ref string itemInfo, ref string interactionInfo, ref string[] interactionKeybinds)
		{
			if (target is BaseItem item)
			{
				string stackCount = string.Empty;

				itemInfo += item.PublicName;
				stackCount += item.StackCount > 1 ? $" ({item.StackCount})" : string.Empty;

				if (_game.Settings.DisplayItemInteractions)
				{
					interactionInfo = $"\n{interactionKeybinds[0]} {GetInteractionString(item.PrimaryInteraction)}";

					if (item.PrimaryInteraction != item.SecondaryInteraction)
					{
						interactionInfo += $"\n{interactionKeybinds[1]} {GetInteractionString(item.SecondaryInteraction)}";
					}
				}

				if (target is Consumable consumable)
				{
					itemInfo += $" [{consumable.FillingLeft}%]";
				}

				itemInfo += stackCount;
			}
		}

		private void InsertProductInfo(ref object target, ref string itemInfo)
		{
			if (target is SelectorSwitch button && ((Node)target).GetParent().GetParent() is Shop shop)
			{
				GameData.ItemData item = _game.GameDatabase.Items[shop.ProductList[button.TargetState]];
				itemInfo += $"{TranslationServer.Translate(item.ID)} - {HelperMethods.GetFormattedPrice(item.Value)}";
			}
		}

		private void ToggleVisibility(UIState newState)
		{
			Visible = (newState == UIState.None);
		}

		private static string GetInteractionString(InteractionType targetInteraction)
		{
			switch (targetInteraction)
			{
				case InteractionType.Equip:
					return TranslationServer.Translate($"HEADER_ITEM_EQUIP");

				case InteractionType.Open:
					return TranslationServer.Translate($"HEADER_ITEM_OPEN");

				case InteractionType.Pickup:
					return TranslationServer.Translate($"HEADER_ITEM_PICKUP");

				case InteractionType.Use:
					return TranslationServer.Translate($"HEADER_ITEM_USE");

				default:
					return string.Empty;
			}
		}

		private void UpdateCrosshairSize()
		{
			float newCrosshairSize = StaticValues.CrosshairBaseSize * _game.Settings.CrosshairScale;
			_texture.CustomMinimumSize = new Vector2(newCrosshairSize, newCrosshairSize);
		}
	}
}