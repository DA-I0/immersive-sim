using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class ItemDetails : Control
	{
		private Label _itemName;
		private Label _itemCategory;
		private Label _itemSize;
		private Label _itemWeight;
		private TextureRect _itemIcon;
		private RichTextLabel _itemDescription;

		public override void _Ready()
		{
			_itemName = GetNode<Label>("VerticalContainer/MainInfoContainer/TextDetails/ItemName");
			_itemCategory = GetNode<Label>("VerticalContainer/MainInfoContainer/TextDetails/ItemType");
			_itemSize = GetNode<Label>("VerticalContainer/MainInfoContainer/TextDetails/ItemSize");
			_itemWeight = GetNode<Label>("VerticalContainer/MainInfoContainer/TextDetails/ItemWeight");
			_itemIcon = GetNode<TextureRect>("VerticalContainer/MainInfoContainer/ItemIcon");
			_itemDescription = GetNode<RichTextLabel>("VerticalContainer/ItemDescription");

			GetNode<UIManager>(ProjectSettings.GetSetting("global/UIHandlerPath").ToString()).StateUpdated += HideOutsideInventory;
		}

		public void ToggleItemDetails(BaseItem sourceItem)
		{
			if (sourceItem == null)
			{
				Visible = false;
				return;
			}

			Visible = true;
			Position = HelperMethods.AdjustDisplayPosition(GetViewport().GetMousePosition() + new Vector2(10, 10), Size);

			string usesLeft = string.Empty;

			if (sourceItem is Consumable consumable)
			{
				usesLeft = $"[{consumable.FillingLeft}%]";
			}

			string stackAmount = (sourceItem.StackCount > 1) ? $"({sourceItem.StackCount})" : string.Empty;

			_itemName.Text = $"{sourceItem.PublicName} {usesLeft} {stackAmount}";
			_itemCategory.Text = TranslationServer.Translate($"ITEM_TYPE_{sourceItem.Category.ToString().ToUpper()}");
			_itemSize.Text = $"{TranslationServer.Translate($"HEADER_SIZE")}: {sourceItem.Size * sourceItem.StackCount}";
			_itemWeight.Text = $"{TranslationServer.Translate($"HEADER_WEIGHT")}: {HelperMethods.RoundFloat(sourceItem.Weight * sourceItem.StackCount)}kg";
			string iconPath = HelperMethods.ItemIconPath(sourceItem);
			_itemIcon.Texture = ResourceLoader.Load<Texture2D>(iconPath);
			_itemDescription.Text = TranslationServer.Translate($"ITEM_{sourceItem.TemplateID.ToUpper()}_DESC");

			if (sourceItem.CanBeHanged)
			{
				_itemDescription.Text += $"\n\n{TranslationServer.Translate($"INFO_ITEM_CAN_HANG")}";
			}
		}

		private void HideOutsideInventory(UIState uiState)
		{
			if (uiState != UIState.Inventory)
			{
				ToggleItemDetails(null);
			}
		}
	}
}