using ImmersiveSim.Statics;

namespace ImmersiveSim.GameData
{
	public readonly struct ItemData
	{
		public readonly string ID;
		public readonly int Value;
		public readonly InteractionType PrimaryInteraction;
		public readonly InteractionType SecondaryInteraction;
		public readonly int BaseSize;
		public readonly float BaseWeight;
		public readonly ItemCategory Category;
		public readonly EquipmentSlot Slot;
		public readonly bool IsPickable;
		public readonly bool IsUsable;
		public readonly bool IsStackable;
		public readonly bool CanBeStowed;
		public readonly bool CanBeHanged;
		public readonly int Uses;
		public readonly float UseWeight;
		public readonly int HealthRecovery;
		public readonly int StaminaRecovery;
		public readonly int Capacity;
		public readonly float ItemWeightMultiplier;
		public readonly bool IsHard;
		public readonly string[] Effects;

		public ItemData(
			string id, int value, int primaryInteraction, int secondaryInteraction,
			int baseSize, float weight, int itemType, int equipmentSlot,
			bool setPickable, bool setUsable, bool setStackable, bool canBeStowed, bool canBeHanged,
			int uses, float useWeight, int healthRecovery, int staminaRecovery,
			int capacity, float itemWeightMultiplier, bool isHard, string[] effects)
		{
			ID = id;
			Value = value;
			PrimaryInteraction = (InteractionType)primaryInteraction;
			SecondaryInteraction = (InteractionType)secondaryInteraction;
			BaseSize = baseSize;
			BaseWeight = weight;
			Category = (ItemCategory)itemType;
			Slot = (EquipmentSlot)equipmentSlot;
			IsPickable = setPickable;
			IsUsable = setUsable;
			IsStackable = setStackable;
			CanBeStowed = canBeStowed;
			CanBeHanged = canBeHanged;
			Uses = uses;
			UseWeight = useWeight;
			HealthRecovery = healthRecovery;
			StaminaRecovery = staminaRecovery;
			Capacity = capacity;
			ItemWeightMultiplier = itemWeightMultiplier;
			IsHard = isHard;
			Effects = effects;
		}
	}
}