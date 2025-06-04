using Godot;
using ImmersiveSim.Interfaces;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class Chair : Node3D, IInteractable//, INPCUsable
	{
		public event StatusUpdate InteractionComplete;

		private Node3D _seatPosition;
		private Node3D _exitPosition;
		private Node3D _npcInteractionPoint;
		private IMovement _currentUser;

		// [Export] private float _npcInteractionDuration;
		// private Timer _npcInteractionTimer;

		// public Node3D NPCInteractionPoint
		// {
		// 	get { return _npcInteractionPoint; }
		// }

		// public int HealthChange
		// {
		// 	get { return 0; }//_healthChange; }
		// }

		// public int StaminaChange
		// {
		// 	get { return 0; }//_staminaChange; }
		// }

		public override void _Ready()
		{
			_seatPosition = GetNode<Node3D>("SeatPositionMarker");
			_exitPosition = GetNode<Node3D>("ExitPositionMarker");
			// _npcInteractionTimer = new Timer();
			// _npcInteractionTimer.Timeout += CompleteNPCInteraction;
			// AddChild(_npcInteractionTimer);
		}

		public void TriggerPrimaryInteraction(Inventory user)
		{
			if (_currentUser != null)
			{
				UnseatUser();
			}
			else
			{
				IMovement newUser = user.GetParent<IMovement>();

				if (newUser.ActiveStance == Stance.Sitting)
				{
					return;
				}

				_currentUser = newUser;
				SeatUser();
			}
		}

		public void TriggerSecondaryInteraction(Inventory user)
		{
			throw new System.NotImplementedException();
		}

		private void SeatUser()
		{
			_currentUser.ChangeStance(Stance.Sitting);
			_currentUser.ChangePosition(_seatPosition.GlobalPosition);
			_currentUser.ChangeRotation(_seatPosition.GlobalRotation);
		}

		private void UnseatUser()
		{
			_currentUser.ChangePosition(_exitPosition.GlobalPosition);
			_currentUser.ChangeRotation(_exitPosition.GlobalRotation);
			_currentUser.ChangeStance(Stance.Idle);
			_currentUser = null;
		}

		// public void NPCInteract(CharacterBase user)
		// {
		// 	GD.Print("npc interaction");
		// 	Vector3 lookPosition = GlobalPosition;
		// 	lookPosition.Y = user.CharMovement.GlobalPosition.Y;
		// 	user.CharMovement.LookAt(lookPosition);
		// 	_npcInteractionTimer.Start(_npcInteractionDuration);
		// }

		// private void CompleteNPCInteraction()
		// {
		// 	InteractionComplete?.Invoke();
		// }
	}
}