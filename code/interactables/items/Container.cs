using Godot;
using Godot.Collections;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class Container : BaseItem
	{
		public event StatusUpdate InventoryUpdated;

		private int _usedSpace;
		private float _combinedWeight;

		private Node _contents;

		public override int Size
		{
			get
			{
				return _game.GameDatabase.Items[_templateID].IsHard ? _game.GameDatabase.Items[_templateID].BaseSize : _game.GameDatabase.Items[_templateID].BaseSize + UsedSpace;
			}
		}

		public override float Weight
		{
			get { return _game.GameDatabase.Items[_templateID].BaseWeight + _combinedWeight; }
		}

		public int Capacity
		{
			get { return _game.GameDatabase.Items[_templateID].Capacity; }
		}

		public float ItemWeightMultiplier
		{
			get { return _game.GameDatabase.Items[_templateID].ItemWeightMultiplier; }
		}

		public Array<BaseItem> ItemsWithin
		{
			get { return GetItemsWithin(); }
		}

		private Array<BaseItem> GetItemsWithin()
		{
			Array<BaseItem> items = new Array<BaseItem>();

			foreach (Node child in _contents.GetChildren())
			{
				if (child is BaseItem item)
				{
					items.Add(item);
				}
			}

			return items;
		}

		public int UsedSpace
		{
			get { return _usedSpace; }
		}

		public override void _Ready()
		{
			_contents = GetNode("Contents");
			_contents.ChildEnteredTree += (Node child) => MarkAsModified();
			_contents.ChildExitingTree += (Node child) => MarkAsModified();
			base._Ready();

			if (!_isReloading)
			{
				CallDeferred("InitialContainerSetup");
			}
		}

		public override void UseItem(Inventory user)
		{
			base.UseItem(user);
			user.Equip(this);
		}

		public bool AddItem(BaseItem item, bool avoidStack = false)
		{
			if (item == null || !item.CanBeStowed)
			{
				return false;
			}

			if (!ItemsWithin.Contains(item))
			{
				if (item.Size <= Capacity - _usedSpace && item != this)
				{
					bool added = false;

					if (item.IsStackable && !avoidStack)
					{
						added = AddToStack(item);
					}

					if (!added)
					{
						ReparentItem(item);
					}

					return true;
				}
			}
			return false;
		}

		public bool RemoveItem(BaseItem item)
		{
			if (ItemsWithin.Contains(item))
			{
				item.ChangeParent(_game.Level.ActiveScene);
				return true;
			}
			else
			{
				return false;
			}
		}

		protected override void FinishSaveStateRestore()
		{
			base.FinishSaveStateRestore();
			GameData.ItemSaveState savedState = _game.Save.GetItemSaveState(ObjectID);
			CallDeferred("RestoreSavedContent", savedState.ItemsWithin);
		}

		private async void RestoreSavedContent(string[] contentIDs)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

			foreach (string itemID in contentIDs)
			{
				BaseItem item = _game.Save.IsItemTracked(itemID);
				if (item != null)
				{
					AddItem(item);
				}
			}
		}

		private void InitialContainerSetup()
		{
			foreach (BaseItem item in ItemsWithin)
			{
				ReparentItem(item);
			}
		}

		public void RefreshContainer(bool updateChildren = true)
		{
			_usedSpace = 0;
			_combinedWeight = 0;

			foreach (BaseItem item in ItemsWithin)
			{
				_usedSpace += (item.Size * item.StackCount);
				_combinedWeight += HelperMethods.RoundFloat(item.Weight * item.StackCount);

				if (updateChildren)
				{
					item.IsModified = true;
				}
			}

			InventoryUpdated?.Invoke();
		}

		internal override void ItemModified()
		{
			if (_isModified && !_isReloading)
			{
				RefreshContainer();
				UpdateItemInfo();
				base.ItemModified();
				ParentContainer?.RefreshContainer(false);
			}
		}

		private void ReparentItem(Interfaces.IItem item)
		{
			if (item == null)
			{
				return;
			}
			item.PickUp(_contents, setVisible: false);
			item.ParentContainer = this;
		}

		private bool AddToStack(BaseItem newItem)
		{
			foreach (BaseItem item in ItemsWithin)
			{
				if (item.TemplateID == newItem.TemplateID && item.IsStackable)
				{
					if (item.CustomName != newItem.CustomName)
					{
						return false;
					}

					if (item is Consumable consumable)
					{
						if (consumable.UsesLeft != ((Consumable)newItem).UsesLeft)
						{
							continue;
						}
					}
					// if (newItem.GetComponent<Durability>() == null || 
					// 	(newItem.GetComponent<Durability>().currHealth <= item.GetComponent<Durability>().currHealth * 1.1f 
					// 	&& newItem.GetComponent<Durability>().currHealth >= item.GetComponent<Durability>().currHealth * 0.9f))
					// {
					// item.ChangeStackCount += newItem.StackCount;
					item.ChangeStack(newItem.StackCount);
					// newItem.StackCount = 0;

					// if (item.GetComponent<Durability>() != null)
					// 	item.GetComponent<Durability>().currHealth = (item.GetComponent<Durability>().currHealth + newItem.GetComponent<Durability>().currHealth)/item.count;

					// newItem.UpdateObjectProperties();
					// item.UpdateObjectProperties();
					// RefreshContainer();
					newItem.Destroy();
					return true;
					// }


					// item.TryAddingToStack(newItem);
				}
			}
			return false;
		}
	}
}
