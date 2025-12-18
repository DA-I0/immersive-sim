using System;
using Godot;
using ImmersiveSim.GameData;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay // move from Gameplay to Character?
{
	public partial class Inventory : Node
	{
		public event StatusUpdate InventoryUpdated;

		private const float StrengthCarryWeightMultiplier = 10f; // temp, replace with difficulty/game setting

		private int _money = 100;
		// 0-3 - quick access/"belt" items, 4 - head, 5 - torso, 6 - legs, 7 - feet, 8 - backpack
		public BaseItem[] _equipmentSlots = { null, null, null, null, null, null, null, null, null };
		public int[] _equipmentSlotSelected = { -1, -1, -1, -1, -1, -1, -1, -1, -1 };

		private float _maxCarryWeight = 0f;
		private float _equipmentWeight = 0f;
		private float _carryWeightRatio = 0f;

		private BaseItem _selectedItem;
		private Node3D _lastSelectedItem;

		private Node3D _itemHoldPosition;

		private GameSystem _game;
		private CharacterBase _character;

		public int Money
		{
			get { return _money; }
			// set { _money = value; }
		}

		public float EquipmentWeight
		{
			get { return _equipmentWeight; }
		}

		public float MaxCarryWeight
		{
			get { return _maxCarryWeight; }
		}

		public float CarryWeightRatio
		{
			get { return _carryWeightRatio; }
		}

		public BaseItem SelectedItem
		{
			get { return _selectedItem; }
		}

		public override void _Ready()
		{
			_itemHoldPosition = GetNode<Node3D>("../Camera/ItemHoldPosition");
			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_character = GetNode<CharacterBase>("../CharacterBase");
			_maxCarryWeight = _character.CharSheet.Stats[0] * StrengthCarryWeightMultiplier; // TODO: move to a method after implementing status effects (for recalculations on addition/removal)
																							 // CallDeferred("UpdateEquipmentWeight");
			UpdateEquipmentWeight();
		}

		public override void _Input(InputEvent @event)
		{
			if (_game.State != GameState.Gameplay)
			{
				return;
			}

			if (@event.IsActionPressed("action_use_item"))
			{
				if (_character is PlayerBase player && !player.CharInteraction.IsItemPlacementMode)
				{
					UseSelecteItem();
				}
			}

			if (@event.IsActionPressed("action_drop_selected"))
			{
				DropSelectedItem();
			}

			if (@event.IsActionPressed("inventory_quick_item_1"))
			{
				QuickSelectFromEquipped(0);
			}

			if (@event.IsActionPressed("inventory_quick_item_2"))
			{
				QuickSelectFromEquipped(1);
			}

			if (@event.IsActionPressed("inventory_quick_item_3"))
			{
				QuickSelectFromEquipped(2);
			}

			if (@event.IsActionPressed("inventory_quick_item_4"))
			{
				QuickSelectFromEquipped(3);
			}

			if (@event.IsActionPressed("inventory_quick_torso"))
			{
				QuickSelectFromEquipped(5);
			}

			if (@event.IsActionPressed("inventory_quick_legs"))
			{
				QuickSelectFromEquipped(6);
			}

			if (@event.IsActionPressed("inventory_hide_selected"))
			{
				DeselectItem(_selectedItem);
			}
		}

		public void PickupItem(BaseItem target)
		{
			if (PickupIntoEquipped(target))
			{
				return;
			}

			target.PickUp(this);
			SelectItem(target);
			UpdateEquipmentWeight();
			TriggerPickupNotification(target);
		}

		public void DropItem(BaseItem target)
		{
			if (target == null)
			{
				return;
			}

			CheckIfEquipped(_selectedItem, true);
			target.GlobalPosition = _itemHoldPosition.GlobalPosition;
			target?.Drop();
			UpdateEquipmentWeight();
			TriggerDropNotification(target);
		}

		public void DropSelectedItem()
		{
			DropItem(_selectedItem);
			_selectedItem = null;
		}

		//Item equip
		public void Equip(BaseItem item)
		{
			switch (item.Slot)
			{
				case EquipmentSlot.Item:
					for (int i = 0; i < 3; i++)
					{
						if (_equipmentSlots[i] == null)
						{
							EquipTo(item, i);
							break;
						}
					}

					break;

				case EquipmentSlot.Head:
					EquipTo(item, 4);
					break;

				case EquipmentSlot.Torso:
					EquipTo(item, 5);
					break;

				case EquipmentSlot.Legs:
					EquipTo(item, 6);
					break;

				case EquipmentSlot.Feet:
					EquipTo(item, 7);
					break;

				case EquipmentSlot.Backpack:
					EquipTo(item, 8);
					break;
			}

			FinalizeItemEquip(null, item);
		}

		public void EquipTo(BaseItem item, int slotIndex)
		{
			BaseItem temp = PackOldItem(item, slotIndex);
			_equipmentSlots[slotIndex] = item;
			item.DisableColliders(true);
			FinalizeItemEquip(temp, item);
			TriggerEquipNotification(item);
		}

		private void FinalizeItemEquip(BaseItem oldItem, BaseItem newItem)
		{
			if (oldItem != null)
			{
				PickupItem(oldItem);
			}

			if (_selectedItem == newItem)
			{
				_selectedItem = null;
			}

			DisplayEquippedItem(newItem);
			UpdateEquipmentWeight();
			newItem.IsEquipped = true;
			newItem.IsModified = true;
		}

		private /*async*/ void DisplayEquippedItem(BaseItem newItem)
		{
			newItem.RemoveFromParentContainer();
			// await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); // TODO: replace this with a check in BaseItem whether item is equipped or not (skip position restoration if true) 
			// await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
			// await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

			// placeholder way to attach equipped item to the player
			newItem.ChangeParent(GetNode("../Collider").GetChild(0), true, true, false); // temp placement of equipped items
																						 // newItem.Position = Vector3.Zero;
			newItem.RotationDegrees = new Vector3(0, 180f, 0);
		}

		private BaseItem PackOldItem(BaseItem item, int slotIndex)
		{
			if (_equipmentSlots[slotIndex] != null)
			{
				if (item.ParentContainer != null && item.ParentContainer != _equipmentSlots[slotIndex])
				{
					item.ParentContainer.AddItem(_equipmentSlots[slotIndex]);
					return null;
				}
			}

			return _equipmentSlots[slotIndex];
		}

		public void Unequip(BaseItem item, Container container)
		{
			if (item != null)
			{
				CheckIfEquipped(item, true);

				if (container != null && container != item)
				{
					if (!container.AddItem(item))
					{
						PickupItem(item);
					}
				}
				else
				{
					PickupItem(item);
				}
			}
			UpdateEquipmentWeight();
		}

		public bool CheckIfEquipped(BaseItem item, bool unequip) // change since this unequips and Unequip also selects; "UnequipIfPossible" or something?
		{
			if (item == null)
			{
				return false;
			}

			for (int i = 0; i < _equipmentSlots.Length; i++)
			{
				if (_equipmentSlots[i] == item)
				{
					if (unequip)
					{
						_equipmentSlots[i] = null;
						item.IsEquipped = false;
						TriggerUnequipNotification(item);
					}

					return true;
				}
			}

			return false;
		}

		private bool PickupIntoEquipped(BaseItem target)
		{
			foreach (BaseItem equippedItem in _equipmentSlots)
			{
				if (equippedItem is Container container)
				{
					if (container.AddItem(target))
					{
						UpdateEquipmentWeight();
						TriggerPickupNotification(target);
						return true;
					}
				}
			}

			return false;
		}

		public void SelectItem(BaseItem target)
		{
			if (target == _selectedItem)
			{
				return;
			}

			DeselectItem(_selectedItem);

			_selectedItem = target;
			_selectedItem.PickUp(_itemHoldPosition, true, true);
		}

		public void ClearSelectedItem()
		{
			_selectedItem = null;
		}

		private void DeselectItem(BaseItem target)
		{
			if (target != null && !target.IsQueuedForDeletion())
			{
				PackItemIfPossible(target);
			}

			ClearSelectedItem();
		}

		private void PackItemIfPossible(BaseItem item)
		{
			if (item == null)
			{
				return;
			}

			if (CheckIfEquipped(item, false))
			{
				item.GetParent().RemoveChild(item);
				AddChild(item);
				item.Visible = false;
				return;
			}

			if (item.ParentContainer != null)
			{
				item.GetParent().RemoveChild(item);
				item.ParentContainer.AddChild(item);
				item.Visible = false;
				return;
			}

			if (PickupIntoEquipped(item))
			{
				return;
			}

			DropItem(item);
		}

		private void QuickSelectFromEquipped(int slotButton)
		{
			if (_equipmentSlots[slotButton] != null)
			{
				if (_equipmentSlots[slotButton] is Container searchedContainer)
				{
					if (searchedContainer.ItemsWithin.Count > 0)
					{
						_equipmentSlotSelected[slotButton]++;

						if (_equipmentSlotSelected[slotButton] > searchedContainer.ItemsWithin.Count - 1)
						{
							_equipmentSlotSelected[slotButton] = 0;
						}

						SelectItem(searchedContainer.ItemsWithin[_equipmentSlotSelected[slotButton]]);
					}
				}
				else
				{
					SelectItem(_equipmentSlots[slotButton]);
				}
			}
		}

		private void UseSelecteItem()
		{
			if (_selectedItem != null && Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				_selectedItem.UseItem(this);
			}
		}

		private void UpdateEquipmentWeight()
		{
			_equipmentWeight = 0;

			foreach (BaseItem item in _equipmentSlots)
			{
				_equipmentWeight += (item != null) ? item.Weight : 0;
			}

			_carryWeightRatio = _equipmentWeight / MaxCarryWeight;
			InventoryUpdated?.Invoke();
			_character.IsModified = true;
		}

		internal void RestoreSavedState(CharacterSaveState savedState)
		{
			_money = savedState.Money;
			CallDeferred("RestoreSavedEquipment", savedState.EquipmentItems);
		}

		private void RestoreSavedEquipment(string[] contentIDs)
		{
			for (int index = 0; index < _equipmentSlots.Length; index++)
			{
				if (contentIDs[index] != string.Empty)
				{
					BaseItem targetItem = _game.Save.IsItemTracked(contentIDs[index]);
					EquipTo(targetItem, index);
				}
			}
		}

		private void TriggerPickupNotification(BaseItem target)
		{
			if (_character.ID == "player" && target != null)
			{
				_game.Notifications.TriggerNotification("item_pickup", target.PublicName);
			}
		}

		private void TriggerDropNotification(BaseItem target)
		{
			if (_character.ID == "player" && target != null)
			{
				_game.Notifications.TriggerNotification("item_drop", target.PublicName);
			}
		}

		private void TriggerEquipNotification(BaseItem target)
		{
			if (_character.ID == "player" && target != null)
			{
				_game.Notifications.TriggerNotification("item_equip", target.PublicName);
			}
		}

		private void TriggerUnequipNotification(BaseItem target)
		{
			if (_character.ID == "player" && target != null)
			{
				_game.Notifications.TriggerNotification("item_unequip", target.PublicName);
			}
		}

		public void ChangeMoney(int value)
		{
			value = (value < -_money) ? -_money : value;
			_money += value;

			if (value != 0)
			{
				_game.Notifications.TriggerNotification("money_change", value.ToString());
			}
		}

		public void SetMoney(int value)
		{
			_money = (value >= 0) ? value : 0;
		}
	}
}