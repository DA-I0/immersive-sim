namespace ImmersiveSim.Gameplay
{
	public partial class NPCBase : CharacterBase
	{
		protected NPCMovement _movement;
		protected NPCActionControler _actionControler;
		protected Godot.Timer _destructionTimer;

		public NPCMovement CharMovement
		{
			get { return _movement; }
		}

		public NPCActionControler CharActions
		{
			get { return _actionControler; }
		}

		public override void _EnterTree()
		{
			_destructionTimer = new Godot.Timer();
			AddChild(_destructionTimer);
			_destructionTimer.Timeout += Destroy;
		}

		public override void _Ready()
		{
			base._Ready();
			_movement = GetParent<NPCMovement>();
			_actionControler = GetNode<NPCActionControler>("../CharacterActionControler");

			TreeExiting += DestructionCleanup;
		}

		public void StartDestructionSequence()
		{
			if (IsInsideTree() && !IsQueuedForDeletion())
			{
				_destructionTimer?.Start(Statics.StaticValues.NPCDestructionDelay);
			}
		}

		public void StopDestructionSequence()
		{
			if (IsInsideTree())
			{
				_destructionTimer?.Stop();
			}
		}

		private void DestructionCleanup()
		{
			// if (IsQueuedForDeletion())
			// {
			// 	_game.NPCSpawnController.RemoveNPCOnDestruction(this);
			// }
		}

		public override void _Notification(int what)
		{
			if (what == NotificationPredelete)
			{
				_game.NPCSpawnController.RemoveNPCOnDestruction(CharMovement);
			}

			base._Notification(what);
		}
	}
}