using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class TrashBin : Node3D
	{
		private Area3D _deathZone;

		public override void _Ready()
		{
			_deathZone = GetNode<Area3D>("DeathZone");
			_deathZone.BodyEntered += DestroyObject;
		}

		private void DestroyObject(Node3D target)
		{
			if (target is BaseItem item)
			{
				item.Destroy();
			}
		}
	}
}