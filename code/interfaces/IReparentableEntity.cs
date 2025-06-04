namespace ImmersiveSim.Interfaces
{
	public interface IReparentableEntity
	{
		public void ReparentToSubscene(Godot.Node newParent);
		public void ResetParentToMainScene();
		public void TriggerParentReset();
	}
}