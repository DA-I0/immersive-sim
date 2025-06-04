using Godot;
using ImmersiveSim.GameData;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class ShopItemEntry : Control
	{
		public event StatusUpdate ItemAmountChanged;

		private Label _itemName;
		private Label _itemCategory;
		private Label _itemPrice;
		private LineEdit _itemAmount;

		private int _itemsToBuy = 0;

		public int ItemsToBuy
		{
			get { return _itemsToBuy; }
		}

		public override void _Ready()
		{
			_itemName = GetNode<Label>("ItemName");
			_itemCategory = GetNode<Label>("ItemCategory");
			_itemPrice = GetNode<Label>("ItemPrice");
			_itemAmount = GetNode<LineEdit>("ItemsToBuy");
		}

		public void Destroy()
		{
			QueueFree();
		}

		public void InitializeItemDetails(ItemData itemData)
		{
			string displayName = TranslationServer.Translate($"ITEM_{itemData.ID.ToUpper()}");

			_itemName.Text = $"{displayName}";
			_itemCategory.Text = $"{itemData.Category}";
			_itemPrice.Text = HelperMethods.GetFormattedPrice(itemData.Value);
		}

		public void Reset()
		{
			SetItemAmount(0);
		}

		private void ChangeItemAmount(int value, bool updateTextBox = true)
		{
			SetItemAmount(_itemsToBuy + value, updateTextBox);
		}

		private void SetItemAmount(int value, bool updateTextBox = true)
		{
			_itemsToBuy = value;

			if (_itemsToBuy < 0)
			{
				_itemsToBuy = 0;
			}

			if (updateTextBox)
			{
				UpdateAmountTextBox();
			}

			ItemAmountChanged?.Invoke();
		}

		private void SetItemAmount(string value)
		{
			if (value.IsValidInt())
			{
				int numericValue = int.Parse(value);

				if (numericValue >= 0)
				{
					SetItemAmount(numericValue, false);
					return;
				}
			}

			SetItemAmount(0);
		}

		private void UpdateAmountTextBox()
		{
			_itemAmount.Text = _itemsToBuy.ToString();
		}
	}
}