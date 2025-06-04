namespace ImmersiveSim.Gameplay
{
	public partial class PlayerBase : CharacterBase
	{
		protected Interaction _interaction;
		protected Movement _movement;

		public Interaction CharInteraction
		{
			get { return _interaction; }
		}

		public Movement CharMovement
		{
			get { return _movement; }
		}

		protected override void InitialBaseSetup()
		{
			base.InitialBaseSetup();
			_interaction = GetNode<Interaction>("../CharacterInteraction");
			_movement = GetParent<Movement>();
		}
	}
}