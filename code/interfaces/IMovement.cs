using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Interfaces
{
	public interface IMovement
	{
		public Stance ActiveStance { get; }
		public void ChangeStance(Stance newStance);
		public void ChangePosition(Vector3 newPosition);
		public void ChangeRotation(Vector3 newRotation);
	}
}