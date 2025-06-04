using Godot;
using ImmersiveSim.Interfaces;

namespace ImmersiveSim.Gameplay
{
	public partial class VendingMachine : Shop, IMerchantDevice
	{
		private bool _isActive = false;

		public override void _Ready()
		{
			base._Ready();
			SetUpProductButtons();
			PopulateProductDisplay();
		}

		public bool IsActive()
		{
			return _isActive;
		}

		public void ChangePowerState(bool isPowered)
		{
			_isActive = isPowered;
		}

		public void SetDeviceState(Inventory user, int itemIndex)
		{
			if (_isActive)
			{
				PurchaseProduct(user, itemIndex, 1, true);
			}
		}

		public void ToggleDeviceState(Inventory user)
		{
			if (_isActive)
			{
				PurchaseProduct(user, 0);
			}
		}

		private void SetUpProductButtons()
		{
			Node buttonParent = GetNode("ProductButtons");

			for (int index = 0; index < buttonParent.GetChildCount(); index++)
			{
				if (index < ProductList.Length)
				{
					buttonParent.GetChild<SelectorSwitch>(index).AssignTarget(this, index);
				}
				else
				{
					buttonParent.GetChild<SelectorSwitch>(index).ToggleActive(false);
				}

			}
		}

		private void PopulateProductDisplay()
		{
			Node productSlots = GetNode("ProductSlots");

			for (int index = 0; index < productSlots.GetChildCount(); index++)
			{
				ClearProductSlot(productSlots.GetChild(index));

				if (index < ProductList.Length)
				{
					BaseItem newProduct = _game.SpawnItem(ProductList[index], canBeSaved: false, targetParent: productSlots.GetChild(index));
					newProduct.Freeze = true;
					newProduct.ProcessMode = ProcessModeEnum.Disabled;
					newProduct.Position = AdjustProductPosition(ref newProduct);
					newProduct.RotationDegrees = RandomizeProductRotation();
				}
			}
		}

		private static void ClearProductSlot(Node slotNode)
		{
			if (slotNode.GetChildCount() > 0)
			{
				for (int childIndex = 0; childIndex < slotNode.GetChildCount(); childIndex++)
				{
					slotNode.GetChild(childIndex).ProcessMode = ProcessModeEnum.Always;
					slotNode.GetChild(childIndex).QueueFree();
				}
			}
		}

		private static Vector3 AdjustProductPosition(ref BaseItem product)
		{
			Vector3 productDimensions = Statics.HelperMethods.GetShapeDimensions(product.GetNode<CollisionShape3D>("Collider").Shape);
			return new Vector3(0, productDimensions.Y / 2, 0);
		}

		private static Vector3 RandomizeProductRotation()
		{
			Vector3 newRotation = Vector3.Zero;
			newRotation.Y = (float)GD.RandRange(-Statics.StaticValues.ProductRotationRange, Statics.StaticValues.ProductRotationRange);
			return newRotation;
		}
	}
}