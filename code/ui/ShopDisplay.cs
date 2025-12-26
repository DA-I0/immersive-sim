using System.Linq;
using Godot;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	// TODO: move logic to ShopManager class
	public partial class ShopDisplay : Control
	{
		[Export] private PackedScene _productInfoPrefab;
		private Label _shopHeader;
		private Label _cartTotal;
		private Node _productList;
		private Button _buyButton;

		private Gameplay.Shop _activeShop;

		private GameSystem _game;

		public override void _Ready()
		{
			_shopHeader = GetNode<Label>("Background/ControlList/ShopHeader");
			_cartTotal = GetNode<Label>("Background/ControlList/CartPriceTotal");
			_productList = GetNode("Background/ControlList/ProductList");
			_buyButton = GetNode<Button>("Background/ControlList/ShopOperations/Confirm");
			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			SubscribeToEvents();
		}

		public override void _Input(InputEvent @event)
		{
			if (@event.IsActionPressed("ui_cancel") && Visible)
			{
				ToggleShopDisplay(null);
			}
		}

		public void ToggleShopDisplay(Gameplay.Shop shop)
		{
			if (shop == null)
			{
				_game.UI.SetUIState(UIState.None);
			}
			else
			{
				_shopHeader.Text = TranslationServer.Translate($"SHOP_{shop.ShopID.ToUpper()}");
				_activeShop = shop;
				PopulateProductList();
				UpdateCartTotal();
				Visible = true;
				_game.UI.SetUIState(UIState.Misc);
			}
		}

		public void UpdateCartTotal()
		{
			int totalPrice = _activeShop.GetTotalPrice(GetProductAmounts());
			_cartTotal.Text = $"{TranslationServer.Translate("HEADER_TOTAL_PRICE")} {HelperMethods.GetFormattedPrice(totalPrice)}\nAvailable money: {HelperMethods.GetFormattedPrice(_game.Player.CharInventory.Money)}";

			_buyButton.Disabled = (totalPrice > _game.Player.CharInventory.Money);
		}

		private void SubscribeToEvents()
		{
			_game.UI.StateUpdated += CloseOnUIStateChange;
			_game.UI.OpenShopUI += ToggleShopDisplay;
		}

		private void ConfirmPurchase()
		{
			if (_activeShop.PurchaseCart(_game.Player.CharInventory, GetProductAmounts()))
			{
				ToggleShopDisplay(null);
			}
		}

		private int[] GetProductAmounts()
		{
			int[] purchaseAmounts = new int[_productList.GetChildCount()];

			for (int index = 0; index < _productList.GetChildCount(); index++)
			{
				purchaseAmounts[index] = _productList.GetChild<ShopItemEntry>(index).ItemsToBuy;
			}

			return purchaseAmounts;
		}

		private void CancelPurchase()
		{
			ToggleShopDisplay(null);
		}

		private void ResetShopState()
		{
			foreach (ShopItemEntry productEntry in _productList.GetChildren().Cast<ShopItemEntry>())
			{
				productEntry.Reset();
			}
		}

		private void PopulateProductList()
		{
			ClearProductList();
			foreach (string itemID in _activeShop.ProductList)
			{
				ShopItemEntry productEntry = _productInfoPrefab.Instantiate<ShopItemEntry>();
				_productList.AddChild(productEntry);
				productEntry.InitializeItemDetails(_game.GameDatabase.Items[itemID]);
				productEntry.ItemAmountChanged += UpdateCartTotal;
			}
		}

		private void ClearProductList()
		{
			foreach (ShopItemEntry productEntry in _productList.GetChildren().Cast<ShopItemEntry>())
			{
				productEntry.ItemAmountChanged -= UpdateCartTotal;
				productEntry.Destroy();
			}
		}

		private void CloseOnUIStateChange(UIState activeState)
		{
			if (activeState != UIState.Misc)
			{
				Visible = false;
			}
		}
	}
}