using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class Flashlight : BaseItem
	{
		private Light3D _lightSource;

		public override void _Ready()
		{
			base._Ready();
			FlashlightSetup();
		}

		public override void UseItem(Inventory user)
		{
			base.UseItem(user);
			_lightSource.Visible = !_lightSource.Visible;
		}

		private void FlashlightSetup()
		{
			_lightSource = GetNode<Light3D>("LightSource");
			_lightSource.Visible = false;
		}
	}
}