using ImmersiveSim.Statics;

namespace ImmersiveSim.Interfaces
{
	public interface INPCUsable
	{
		public event StatusUpdate InteractionComplete;
		public Godot.Node3D InteractionPointPosition { get; }
		public NPCInteractionType InteractionType { get; }
		public int HealthChange { get; }
		public int StaminaChange { get; }
		public Gameplay.NPCBase ActiveUser { get; }

		public void NPCInteract(Gameplay.NPCBase user);
		public void ClearUser(Godot.Node3D user);
	}
}