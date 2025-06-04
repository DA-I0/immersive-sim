using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Interfaces
{
	public interface IItem
	{
		public string ObjectID { get; }
		public string TemplateID { get; }
		public abstract string CustomName { get; }
		public abstract int Value { get; }
		public abstract int Size { get; }
		public abstract float Weight { get; }
		public abstract ItemCategory Category { get; }
		public abstract EquipmentSlot Slot { get; }
		public abstract bool IsPickable { get; }
		public abstract bool IsUsable { get; }
		public abstract bool IsStackable { get; }
		public abstract Container ParentContainer { get; set; }
		public abstract bool IsActive { get; set; }

		public virtual void TriggerPrimaryInteraction(Inventory user) { }
		public virtual void TriggerSecondaryInteraction(Inventory user) { }
		public virtual void UseItem(Inventory user) { }
		public void PickUp(Godot.Node newParent, bool takeSingleItem = false, bool setVisible = true) { }
		public virtual void SplitStack(int newStackSize) { }
		public void Drop() { }
		public virtual bool TryAddingToStack(BaseItem itemToAdd) { return true; }
		public void ChangeStack(int value) { }
		public void SetStack(int value) { }
		public virtual void Destroy() { }
		public void Rename(string newName) { }
	}
}