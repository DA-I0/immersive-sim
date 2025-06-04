using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class NPCMovement : MovementBase, Interfaces.IMovement, Interfaces.IReparentableEntity
	{
		public event StatusUpdate TargetReached;

		private NPCState _activeState;
		private NPCState _previousState;

		private float _speedModifier;
		private NavigationAgent3D _navigationAgent;

		private NPCActionControler _actionControler;

		private Timer _doorTimer = new Timer();
		private Timer _parentResetTimer = new Timer();

		private ShapeCast3D _doorCast;

		private NPCBase _character;

		public Vector3 MovementTarget
		{
			get { return _navigationAgent.TargetPosition; }
			set { _navigationAgent.TargetPosition = value; }
		}

		public override void _Ready()
		{
			_character = GetNode<NPCBase>("CharacterBase");
			_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
			_actionControler = GetNode<NPCActionControler>("CharacterActionControler");
			SetupTimers();
			SetupShapeCastComponents();

			base._Ready();

			_speedModifier = 1;//Mathf.Clamp(GD.Randf() + 0.25f, 0.3f, 1f);
			_navigationAgent.VelocityComputed += MoveCharacter;

			ChangeActiveState(NPCState.Active);
		}

		public void ChangeActiveState(NPCState newState)
		{
			if (_activeState != newState)
			{
				_previousState = _activeState;
			}

			_activeState = newState;
		}

		public void RestorePreviousState()
		{
			_activeState = _previousState;
		}

		protected override void SetupShapeCastComponents()
		{
			base.SetupShapeCastComponents();
			SetupDoorCast();
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_actionControler.ActivityType == NPCActivityType.Static || _activeState != NPCState.Active || _navigationAgent.IsNavigationFinished())
			{
				Velocity = Vector3.Zero; // TODO: change to prevent NPC floating
				return;
			}

			if (_navigationAgent.DistanceToTarget() < 3.5f)
			{
				_actionControler.CheckIfInteractionIsPossible();
			}

			Vector3 currentAgentPosition = GlobalTransform.Origin;
			Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
			Vector3 inputDirection = currentAgentPosition.DirectionTo(nextPathPosition) * _defaultSpeed;

			if (!IsOnFloor()) // NOTE: is kinda wonky
			{
				inputDirection += GetGravity() * (float)delta;
			}

			_navigationAgent.Velocity = inputDirection;
			SearchForDoors();
		}

		public override void ChangeStance(Stance newStance)
		{
			// _collider.Disabled = (newStance == Stance.Sitting);
			_stance = newStance;

			// if (newStance == Stance.Idle)
			// {
			// 	_stanceMultiplier = 1f;
			// 	_character.CharInteraction.ResetCamera();
			// }

			// UpdateCharacterCollider();
		}

		public void SetupTargetDestination(Vector3 targetPosition)
		{
			MovementTarget = targetPosition;
		}

		public void AdjustToTargetPosition(Node3D target)//Vector3 targetPosition)
		{
			// targetPosition.Y += ((CapsuleShape3D)_collider.Shape).Height / 2;
			GlobalPosition = target.GlobalPosition;//targetPosition;
			GlobalRotation = target.GlobalRotation;
		}

		private void MoveCharacter(Vector3 safeVelocity)
		{
			if (_activeState != NPCState.Active || _actionControler.ActivityType == NPCActivityType.Static)
			{
				return;
			}

			Velocity = safeVelocity * _speedModifier;

			RotateTowardsTarget();
			AdjustForElevation(Vector2.Up);
			MoveAndSlide();
		}

		private void RotateTowardsTarget()
		{
			Vector3 newLookAtPosition = GlobalPosition + Velocity;
			newLookAtPosition.Y = GlobalPosition.Y;

			if (newLookAtPosition != GlobalPosition)
			{
				LookAt(newLookAtPosition);
			}
		}

		private void TargetDestinationReached()
		{
			TargetReached?.Invoke();
		}

		private void SetupTimers()
		{
			AddChild(_doorTimer);
			_doorTimer.Timeout += ResetDoorCollisions;

			AddChild(_parentResetTimer);
			_parentResetTimer.OneShot = true;
			_parentResetTimer.Timeout += ResetParentToMainScene;
		}

		private void SetupDoorCast()
		{
			_doorCast = new ShapeCast3D
			{
				Position = new Vector3(0, 1f, 0),
				Shape = new BoxShape3D(),
				TargetPosition = Vector3.Forward,
				MaxResults = 4
			};

			((BoxShape3D)_doorCast.Shape).Size = new Vector3(0.2f, 0.1f, 0.2f);
			CallDeferred(Node.MethodName.AddChild, _doorCast);
		}

		private void SearchForDoors()
		{
			if (_doorCast.GetCollisionCount() > 0)
			{
				if (((Node)_doorCast.GetCollider(0)).GetParent() is Door door)
				{
					if (door != _lastUsedDoor)
					{
						AddDoorCollisionException((Node)_doorCast.GetCollider(0));
						door.ToggleNPCInteraction();
					}
				}
			}
		}

		private Node _lastUsedDoor;

		private void AddDoorCollisionException(Node targetDoor)
		{
			if (_lastUsedDoor != targetDoor)
			{
				ResetDoorCollisions();
				AddCollisionExceptionWith(targetDoor);
				_lastUsedDoor = targetDoor;
				_doorTimer.Start(3f);
			}
		}

		private void ResetDoorCollisions()
		{
			if (_lastUsedDoor != null)
			{
				RemoveCollisionExceptionWith(_lastUsedDoor);
				_lastUsedDoor = null;
			}
		}

		public void ReparentToSubscene(Node newParent)
		{
			_parentResetTimer?.Stop();
			Reparent(newParent, true);
		}

		public void ResetParentToMainScene()
		{
			GD.Print($"parent reset in progress for {Name}");
			Reparent(_character.Game.Level.ActiveScene, true);
		}

		public void TriggerParentReset()
		{
			if (_parentResetTimer.IsInsideTree())
			{
				GD.Print($"triggering parent reset for {Name}");
				_parentResetTimer?.Start(StaticValues.EntityParentResetDelay);
			}
		}
	}
}
