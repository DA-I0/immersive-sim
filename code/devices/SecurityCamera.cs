using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class SecurityCamera : MeshInstance3D
	{
		[Export] private Camera3D _source;
		[Export] private SubViewport _target;

		public override void _Ready()
		{
			((BaseMaterial3D)Mesh.SurfaceGetMaterial(0)).AlbedoTexture = _target.GetTexture();
		}

		public void OnButtonPressed()
		{
			GD.Print("SecurityCamera:: it worked!");
		}
	}
}