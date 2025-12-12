namespace ImmersiveSim.Statics // TODO: rename
{
	// System
	public enum CameraState { Idle, Freelook };
	public enum GameState { Gameplay, Interface, Menu };
	public enum UIState { None, Inventory, Pause, TimeSkip, Misc, Dialog, SaveLoad, Settings, Journal };
	public enum QuestState { Unknown, Mentioned, Accepted, Achieved, Completed, Failed };

	// Interactables
	public enum DoorAxis { rotateX, rotateY, rotateZ, moveX, moveY, moveZ };
	public enum SwitchType { Toggle, State };
	public enum NPCInteractionDuration { ActionBased, TimeBased };
	public enum NPCInteractionType { Feeder, Rest, Work };

	// Equipment
	public enum ItemCategory { Aid, Clothes, Consumables, Containers, Guns, Keys, Melee, Misc, Readables, Tools };
	public enum EquipmentSlot { Item, Head, Torso, Legs, Feet, Backpack, Selected };
	public enum InteractionType { Pickup, Equip, Use, Open, None };

	// Characters
	public enum Stance { Idle, Crouch, Prone, Sitting };
	public enum NPCState { Idle, Active, Dialog };
	public enum NPCActivityType { Static, Simulation, Schedule, Random };

	// Destructibles
	public enum DamageType { Blade, Blunt, Explosion, Collision, Fire, Gun, Healing, Age };
}