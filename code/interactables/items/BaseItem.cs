using Godot;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay
{
	public partial class BaseItem : RigidBody3D, Interfaces.IItem, Interfaces.IInteractable, Interfaces.IReparentableEntity
	{
		public event StatusUpdate ItemUpdated;

		[Export] protected string _objectID = string.Empty;
		[Export] protected string _templateID;
		protected string _customName = string.Empty;
		protected bool _isInstancedObject = false;
		protected bool _canBeSaved = true;
		protected bool _collisionsDisabled = false;
		protected int _stackCount = 1;
		protected string _parentContainerID = string.Empty;
		protected Node _parentNode;

		protected bool _isModified = false;
		protected bool _isReloading = false;
		protected bool _isEquipped = false;

		private Timer _parentResetTimer = new Timer();

		protected GameSystem _game;

		public string ObjectID
		{
			get { return _objectID; }
		}

		public string TemplateID
		{
			get { return _templateID; }
		}

		public string CustomName
		{
			get { return _customName; }
		}

		public string PublicName
		{
			get { return (CustomName != string.Empty) ? $"{CustomName} *" : TranslationServer.Translate($"ITEM_{TemplateID.ToUpper()}"); }
		}

		public bool IsInstancedObject
		{
			get { return _isInstancedObject; }
		}

		public bool CollisionsDisabled
		{
			get { return _collisionsDisabled; }
		}

		public virtual int Value
		{
			get { return _game.GameDatabase.Items[_templateID].Value; }
		}

		public virtual int Size
		{
			get { return _game.GameDatabase.Items[_templateID].BaseSize; }
		}

		public virtual float Weight
		{
			get { return _game.GameDatabase.Items[_templateID].BaseWeight; }
		}

		public virtual ItemCategory Category
		{
			get { return _game.GameDatabase.Items[_templateID].Category; }
		}

		public virtual EquipmentSlot Slot
		{
			get { return _game.GameDatabase.Items[_templateID].Slot; }
		}

		public virtual bool IsUsable
		{
			get { return _game.GameDatabase.Items[_templateID].IsUsable; }
		}

		public virtual bool IsStackable
		{
			get { return _game.GameDatabase.Items[_templateID].IsStackable; }
		}

		public virtual bool IsPickable
		{
			get { return _game.GameDatabase.Items[_templateID].IsPickable; }
		}

		public virtual bool CanBeStowed
		{
			get { return _game.GameDatabase.Items[_templateID].CanBeStowed; }
		}

		public virtual bool CanBeHanged
		{
			get { return _game.GameDatabase.Items[_templateID].CanBeHanged; }
		}

		public virtual string[] Effects
		{
			get { return _game.GameDatabase.Items[_templateID].Effects; }
		}

		public virtual int StackCount
		{
			get { return _stackCount; }
			set { _stackCount = value; }
		}

		public bool IsModified
		{
			get { return _isModified; }
			set { _isModified = value; }
		}

		public bool IsEquipped
		{
			get { return _isEquipped; }
			set { _isEquipped = value; }
		}

		public string ParentContainerID
		{
			get { return _parentContainerID; }
			set { _parentContainerID = value; }
		}

		public Container ParentContainer
		{
			get
			{
				if (ParentContainerID != string.Empty)
				{
					Container parentContainer = GetNodeOrNull<Container>(_game.Save.ItemStateCache[ParentContainerID].CurrentNodePath);
					return parentContainer;
				}

				return null;
			}
			set { ParentContainerID = (value != null) ? value.ObjectID : string.Empty; }

		}

		public bool IsActive
		{
			get { return Visible; }
			set { Visible = value; }
		}

		public InteractionType PrimaryInteraction
		{
			get { return _game.GameDatabase.Items[_templateID].PrimaryInteraction; }
		}

		public InteractionType SecondaryInteraction
		{
			get { return _game.GameDatabase.Items[_templateID].SecondaryInteraction; }
		}

		public override void _EnterTree()
		{
			if (ObjectID == string.Empty)
			{
				_objectID = $"{TemplateID}_{GetInstanceId()}";
			}

			Name = ObjectID;
		}

		public override void _Ready()
		{
			InitialBaseSetup();

			if (_canBeSaved && !_game.Save.RegisterItemForSaving(this))
			{
				_isReloading = true;
				CallDeferred("RestoreSavedState");
			}
		}


		public override void _PhysicsProcess(double delta)
		{
			CallDeferred("ItemModified");
		}

		public void SetInstancedState(string originalObjectID, bool savingEnabled = true)
		{
			_objectID = originalObjectID;
			_isInstancedObject = true;
			_canBeSaved = savingEnabled;
		}

		public virtual void TriggerPrimaryInteraction(Inventory user)
		{
			TriggerSelectedInteraction(user, _game.GameDatabase.Items[_templateID].PrimaryInteraction);
		}

		public virtual void TriggerSecondaryInteraction(Inventory user)
		{
			TriggerSelectedInteraction(user, _game.GameDatabase.Items[_templateID].SecondaryInteraction);
		}

		public virtual void UseItem(Inventory user)
		{
			foreach (string effect in Effects)
			{
				_game.ParseTriggeredAction(effect);
			}
		}

		public virtual void PickUp(Node newParent, bool takeSingleItem = false, bool setVisible = true)
		{
			if (!IsPickable)
			{
				return;
			}

			if (ParentContainer != null && StackCount > 1 && takeSingleItem)
			{
				SplitStack(StackCount - 1);
			}

			RemoveFromParentContainer();
			DisableColliders(true);
			ChangeParent(newParent, true, setVisible, false);
		}

		public virtual void SplitStack(int newStackSize) // splitting consumables doesn't update their UI entry until reentering inventory
		{
			BaseItem newItem = this.Duplicate(7) as BaseItem;
			GetParent().AddChild(newItem);
			ChangeStack(-newStackSize);
			newItem.SetStack(newStackSize);
			ParentContainer.AddItem(newItem, true);
		}

		public void Drop()
		{
			RemoveFromParentContainer();
			DisableColliders(false);
			Node newParent = GetNode(_game.Level.ActiveScenePath);
			ChangeParent(newParent);
			LinearVelocity = Vector3.Zero;
		}

		public void ChangeStack(int value)
		{
			_stackCount += value;
			MarkAsModified();
			UpdateItemInfo();
		}

		public void SetStack(int value)
		{
			_stackCount = value;
			MarkAsModified();
			UpdateItemInfo();
		}

		public virtual void Destroy(bool asDuplicate = false)
		{
			RemoveFromParentContainer();

			if (!asDuplicate)
			{
				if (_game.Save.ItemStateCache[ObjectID].IsInstancedObject)
				{
					_game.Save.ItemStateCache.Remove(ObjectID);
				}
				else
				{
					_game.Save.ItemStateCache[ObjectID].MarkAsDestroyed();
				}
			}

			Name = "set_for_destruction";
			QueueFree();
			GetParent().RemoveChild(this);
		}

		public void Rename(string newName)
		{
			_customName = newName;
			UpdateItemInfo();
		}

		public void ReparentToSubscene(Node newParent)
		{
			_parentResetTimer?.Stop();
			ChangeParent(newParent, keepPosition: true);
		}

		public void ResetParentToMainScene()
		{
			ChangeParent(_game.Level.ActiveScene, keepPosition: true);
		}

		public void TriggerParentReset()
		{
			_parentResetTimer?.Start(StaticValues.EntityParentResetDelay);
		}

		internal virtual void RestoreSavedState()
		{
			RestoreSavedState(_game.Save.GetItemSaveState(ObjectID));
		}

		internal async virtual void RestoreSavedState(GameData.ItemSaveState savedState)
		{
			if (!IsInsideTree())
			{
				return;
			}

			if (CheckIfDuplicate(savedState))
			{
				Destroy(true);
				return;
			}

			_objectID = savedState.ObjectID;
			_customName = savedState.CustomName;
			_isInstancedObject = savedState.IsInstancedObject;
			IsEquipped = savedState.IsEquipped;
			SetStack(savedState.StackCount);

			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

			if (savedState.ParentContainer == string.Empty)
			{
				if (savedState.CurrentNodePath != GetPath())
				{
					RemoveFromParentContainer();
					Node newParentNode = GetNodeOrNull(savedState.CurrentNodePath.Replace($"/{Name}", string.Empty));
					ChangeParent(newParentNode);
				}
			}

			CallDeferred("FinishSaveStateRestore");
		}

		protected virtual void FinishSaveStateRestore()
		{
			GameData.ItemSaveState savedState = _game.Save.GetItemSaveState(ObjectID);

			if (!IsEquipped)
			{
				GlobalPosition = savedState.Position;
				GlobalRotation = savedState.Rotation;
				LinearVelocity = savedState.Velocity;
			}

			IsActive = savedState.Visible;
			Freeze = savedState.Frozen;
			DisableColliders(savedState.CollisionsDisabled);
			_isReloading = false;
			_isModified = false;
		}

		protected virtual void InitialBaseSetup()
		{
			SleepingStateChanged += MarkAsModified;

			AddChild(_parentResetTimer);
			_parentResetTimer.Timeout += ResetParentToMainScene;

			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());

			Mass = Weight;
		}

		protected virtual void TriggerSelectedInteraction(Inventory user, InteractionType interaction)
		{
			switch (interaction)
			{
				case InteractionType.Equip:
					user.Equip(this);
					break;

				case InteractionType.Use:
					UseItem(user);
					MarkAsModified();
					break;

				case InteractionType.Open:
					GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).UI.OpenContainer((Container)this);
					break;

				case InteractionType.Pickup:
					user.PickupItem(this);
					break;

				default:
					break;
			}
		}

		private bool CheckIfDuplicate(GameData.ItemSaveState savedState)
		{
			if (savedState.IsDestroyed)
			{
				return true;
			}

			if (!savedState.IsInstancedObject)
			{
				BaseItem existingInstance = GetNodeOrNull<BaseItem>(savedState.CurrentNodePath);

				if (GetPath() != savedState.CurrentNodePath && existingInstance != null)
				{
					return true;
				}
			}

			return false;
		}

		internal virtual void ItemModified()
		{
			if (!IsQueuedForDeletion() && _isModified && !_isReloading)
			{
				_game.Save.UpdateItemCache(this);
				_isModified = false;
			}
		}

		protected void UpdateItemInfo()
		{
			ItemUpdated?.Invoke();
		}

		protected void MarkAsModified()
		{
			_isModified = true;
		}

		public void RemoveFromParentContainer()
		{
			ParentContainer?.RemoveItem(this);
			ParentContainerID = string.Empty;
		}

		internal virtual void ChangeParent(Node newParent, bool freezePhysics = false, bool setVisible = true, bool keepPosition = true)
		{
			if (newParent != GetParent() && newParent != null)
			{
				Vector3 adjustedPosition = GlobalPosition;
				_parentNode = newParent;
				Reparent(newParent, keepPosition);
				// GetParent().RemoveChild(this);
				// newParent.AddChild(this);

				if (!keepPosition)
				{
					Position = Vector3.Zero;
					Rotation = Vector3.Zero;
				}
				else
				{
					GlobalPosition = adjustedPosition;
				}
			}

			Freeze = freezePhysics;
			Visible = setVisible;
			MarkAsModified();
		}

		internal void DisableColliders(bool setDisabled)
		{
			foreach (Node child in GetChildren())
			{
				if (child.Name.ToString().Contains("Collider"))
				{
					((CollisionShape3D)child).Disabled = setDisabled;
				}
			}

			_collisionsDisabled = setDisabled;
		}
	}
}