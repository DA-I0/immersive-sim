using Godot;
using ImmersiveSim.GameData;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay
{
	public partial class Shop : Node3D, Interfaces.IInteractable
	{
		public event ValueChanged CartTotalUpdated;

		[Export] private string _storeID;
		[Export] private Vector2 _maxSpawnOffset;

		private Node3D _spawnPoint;
		private Vector3 _defaultSpawnPointPosition;

		protected GameSystem _game;

		public string StoreID
		{
			get { return _storeID; }
		}

		public string[] ProductList
		{
			get { return _game.GameDatabase.Stores[_storeID].Products; }
		}

		public override void _Ready()
		{
			_spawnPoint = GetNode<Node3D>("SpawnPoint");
			_defaultSpawnPointPosition = _spawnPoint.Position;

			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
		}

		public bool PurchaseCart(Inventory user, int[] _productAmounts)
		{
			if (user.Money < GetTotalPrice(_productAmounts))
			{
				return false;
			}

			for (int index = 0; index < _productAmounts.Length; index++)
			{
				if (_productAmounts[index] > 0)
				{
					PurchaseProduct(user, index, _productAmounts[index]);
				}
			}

			user.ChangeMoney(-GetTotalPrice(_productAmounts));
			return true;
		}

		protected virtual void PurchaseProduct(Inventory user, int itemIndex, int amount = 1, bool singleProduct = false)
		{
			ItemData product = _game.GameDatabase.Items[ProductList[itemIndex]];

			if (user.Money >= product.Value * amount)
			{
				SpawnItem(product, amount);

				if (singleProduct)
				{
					user.ChangeMoney(-product.Value * amount);
				}
			}
		}

		public int GetTotalPrice(int[] _productAmounts)
		{
			int totalPrice = 0;

			for (int index = 0; index < ProductList.Length; index++)
			{
				totalPrice += _productAmounts[index] * _game.GameDatabase.Items[ProductList[index]].Value;
			}

			return totalPrice;
		}

		private void SpawnItem(ItemData product, int amount)
		{
			int itemInstances = product.IsStackable ? 1 : amount;

			for (int index = 0; index < itemInstances; index++)
			{
				BaseItem newItem = _game.SpawnItem(product.ID);

				if (product.IsStackable)
				{
					newItem.SetStack(amount);
				}

				AdjustItemPosition(newItem);
			}
		}

		private void AdjustItemPosition(BaseItem newItem)
		{
			newItem.GetParent()?.RemoveChild(newItem);

			GetParent().AddChild(newItem); // TODO: change to active scene (level)
			Vector3 randomizedSpawnPosition = _defaultSpawnPointPosition;
			randomizedSpawnPosition.X += (float)GD.RandRange(-_maxSpawnOffset.X, _maxSpawnOffset.X);
			randomizedSpawnPosition.Z += (float)GD.RandRange(-_maxSpawnOffset.Y, _maxSpawnOffset.Y);
			_spawnPoint.Position = randomizedSpawnPosition;
			newItem.GlobalPosition = _spawnPoint.GlobalPosition;
		}

		public void TriggerPrimaryInteraction(Inventory user)
		{
			_game.UI.OpenShopDialog(this);
		}

		public void TriggerSecondaryInteraction(Inventory user)
		{
			throw new System.NotImplementedException();
		}
	}
}