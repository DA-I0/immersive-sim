using Godot;
using ImmersiveSim.Gameplay;

namespace ImmersiveSim.GameData
{
	public class ItemSaveState
	{
		private bool _needsSaving = false;

		public bool NeedsSaving
		{
			get { return _needsSaving; }
		}

		public string CurrentNodePath;
		public string ObjectID;
		public string TemplateID;
		public string CustomName;
		public bool IsInstancedObject;
		public Vector3 Position;
		public Vector3 Rotation;
		public Vector3 Velocity;
		public bool Visible;
		public bool Frozen;
		public bool CollisionsDisabled;
		public int StackCount;
		public string ParentContainer;
		public int UsesLeft;
		public string[] ItemsWithin;
		public bool IsDestroyed;
		public bool IsEquipped;

		public ItemSaveState(
			string currentNodePath, string objectID, string templateID,
			string customName, bool isInstancedObject, bool isDestroyed,
			Vector3 position, Vector3 rotation, Vector3 velocity,
			bool visible, bool frozen, bool collisions,
			int stackCount, string parentContainer, int usesLeft, string[] itemsWithin,
			bool isEquipped
			)
		{
			CurrentNodePath = currentNodePath;
			ObjectID = objectID;
			TemplateID = templateID;
			CustomName = customName;
			IsInstancedObject = isInstancedObject;
			IsDestroyed = isDestroyed;
			Position = position;
			Rotation = rotation;
			Velocity = velocity;
			Visible = visible;
			Frozen = frozen;
			CollisionsDisabled = collisions;
			StackCount = stackCount;
			ParentContainer = parentContainer;
			UsesLeft = usesLeft;
			ItemsWithin = itemsWithin;
			IsEquipped = isEquipped;
		}

		public void UpdateItemData(BaseItem target)
		{
			if (target != null)
			{
				string parentContainerPath = target.ParentContainerID;

				CurrentNodePath = target.GetPath();
				CustomName = target.CustomName;
				Position = target.GlobalPosition;
				Rotation = target.GlobalRotation;
				Velocity = target.LinearVelocity;
				Visible = target.IsActive;
				Frozen = target.Freeze;
				CollisionsDisabled = target.CollisionsDisabled;
				StackCount = target.StackCount;
				ParentContainer = parentContainerPath;

				if (target is Consumable consumable)
				{
					UsesLeft = consumable.UsesLeft;
				}

				if (target is Gameplay.Container container)
				{
					ItemsWithin = new string[container.ItemsWithin.Count];

					for (int index = 0; index < container.ItemsWithin.Count; index++)
					{
						ItemsWithin[index] = container.ItemsWithin[index].ObjectID;
					}
				}

				IsEquipped = target.IsEquipped;
				_needsSaving = true;
			}
		}

		public void MarkAsDestroyed()
		{
			IsDestroyed = true;
		}
	}
}