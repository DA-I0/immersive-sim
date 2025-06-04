using ImmersiveSim.Gameplay;

namespace ImmersiveSim.Interfaces
{
	public interface IInteractable
	{
		public void TriggerPrimaryInteraction(Inventory user);
		public void TriggerSecondaryInteraction(Inventory user);
	}
}