using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class Mirror : Node3D
	{
		[Export] private SubViewport _source;
		[Export] private MeshInstance3D _mirrorModel;

		public override void _Ready()
		{
			((BaseMaterial3D)_mirrorModel.Mesh.SurfaceGetMaterial(0)).AlbedoTexture = _source.GetTexture();
		}
	}
}