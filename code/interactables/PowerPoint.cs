using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class PowerPoint : Area3D
	{
		private void ItemBodyEntered(Node3D collider)
		{
			GD.Print($"collider entered: {collider.Name}");

			if (collider is PowerPlug item)
			{
				item.ConnectToPowerPoint(this);
			}
		}
	}
}