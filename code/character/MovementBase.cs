using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class MovementBase : CharacterBody3D, Interfaces.IMovement
	{
		[ExportGroup("Speed")]
		[Export] protected float _defaultSpeed = 5.0f;
		[Export] protected float _speedMultiplierSprint = 2f;
		[Export] protected float _speedMultiplierMax = 1.5f;
		[Export] protected float _speedMultiplierMin = 0.5f;
		protected float _speedMultiplier = 1f;
		protected float _cachedSpeedMultiplier;
		protected float _currentSpeed;

		[ExportGroup("Jumping")]
		[Export] protected float _jumpVelocity = 4.5f;

		[ExportGroup("Stance")]
		[Export] protected float _characterHeight = 2f;
		[Export] protected float _stanceMin = 0.4f;
		[Export] protected float _stanceProne = 0.2f;
		protected float _stanceMultiplier = 1f;
		protected float _crouchTimer = -1f;

		[ExportGroup("Lean")]
		[Export] protected float _leanMax = 0.25f;
		[Export] protected float _leanMaxAngle = 15f;
		protected float _peekHelper = 0f;
		protected float _leanCurrent = 0f;
		protected float _leanTimer;
		protected bool _resetLean = true;

		protected float _characterVelocity;
		protected float _colliderVelocity = 0f;

		[ExportGroup("References")]
		protected Stance _stance = Stance.Idle;

		protected AnimationTree _animator;
		AnimationNodeStateMachinePlayback _playbackBase;
		AnimationNodeStateMachinePlayback _playbackActions;
		protected Node3D _playerModel;

		// protected CharacterBase _character;
		protected CollisionShape3D _collider;
		protected ShapeCast3D _stanceCast;
		protected ShapeCast3D _movementCast;

		public float Speed
		{
			get { return _currentSpeed; }
		}

		public Stance ActiveStance
		{
			get { return _stance; }
		}

		public AnimationTree Animator
		{
			get { return _animator; }
		}

		public override void _Ready()
		{
			_collider = GetNode<CollisionShape3D>("Collider");
			_animator = GetNode<AnimationTree>("CharacterModel/AnimationTree");
			_animator.AdvanceExpressionBaseNode = GetPath();
			_playbackBase = (AnimationNodeStateMachinePlayback)_animator.Get("parameters/playback");
			_playbackActions = (AnimationNodeStateMachinePlayback)_animator.Get("parameters/Interactions/playback");

			_playerModel = GetNode<Node3D>("CharacterModel");
			SetupShapeCastComponents();
			InitializeAnimationParameters();
		}

		public override void _Process(double delta)
		{
			UpdatedAnimationParameters();
		}

		public virtual void ChangeStance(Stance newStance)
		{

		}

		public void ChangePosition(Vector3 newPosition)
		{
			GlobalPosition = newPosition;
		}

		public void ChangeRotation(Vector3 newRotation)
		{
			GlobalRotation = newRotation;
		}

		public void RestoreSavedState(GameData.CharacterSaveState savedState)
		{
			GlobalPosition = savedState.Position + Vector3.Up * 0.01f;
			GlobalRotation = savedState.Rotation;
			Velocity = savedState.Velocity;
		}

		protected virtual void SetupShapeCastComponents()
		{
			SetupMovementCast();
		}

		protected void AdjustForElevation(Vector2 inputDirection)
		{
			if (!IsOnFloor())
			{
				return;
			}

			_movementCast.Position = new Vector3(0, _collider.Position.Y / 2, 0) + ((_movementCast.Transform.Basis * new Vector3(inputDirection.X, 0, inputDirection.Y)).Normalized() / 3f);

			if (_movementCast.GetCollisionCount() > 0)
			{
				float elevationChange = HelperMethods.RoundFloat(_movementCast.GetCollisionPoint(0).Y - (GlobalPosition.Y - 0.01f));

				if (elevationChange > 0 && elevationChange <= StaticValues.MaxStepHeight)
				{
					Translate(new Vector3(0, elevationChange, 0)); // smooth this somehow
				}
			}
		}

		private void SetupMovementCast()
		{
			_movementCast = new ShapeCast3D
			{
				Position = new Vector3(0, 0.4f, -0.4f),
				Shape = new CylinderShape3D(),//BoxShape3D(),
				TargetPosition = Vector3.Down,// * 0.4f,
				MaxResults = 4
			};

			// ((BoxShape3D)movementCast.Shape).Size = new Vector3(0.2f, 0.1f, 0.1f);
			((CylinderShape3D)_movementCast.Shape).Height = 0.05f;
			((CylinderShape3D)_movementCast.Shape).Radius = 0.05f;

			CallDeferred(Node.MethodName.AddChild, _movementCast);
		}

		private void InitializeAnimationParameters()
		{
			_animator.Set($"parameters/conditions/IsNotInteracting", true);
			_animator.Set($"parameters/conditions/IsSittingMid", false);
		}

		private string _action = "idle";

		public string Action
		{
			// get { return (int)_animator.Get("action"); }
			// set { _animator.Set("action", value); }
			get { return _action; }
			set
			{
				_action = value;
				((AnimationNodeStateMachinePlayback)_animator.Get("parameters/playback")).Travel(_action);
			}
		}

		public void TriggerAction(string action)
		{
			if (action.Contains("action_"))
			{
				PlayInteractionAction(action.Replace("action_", string.Empty));
				return;
			}

			PlayMovementAction(action);
		}

		private void PlayMovementAction(string action)
		{
			TogglePositionFreeze(true);
			_playbackActions.Travel("End");
			_playbackBase.Travel(action);
		}

		private void PlayInteractionAction(string action)
		{
			TogglePositionFreeze(false);
			_playbackBase.Travel("Interactions");
			_playbackActions.Travel(action);
		}

		protected void UpdatedAnimationParameters()
		{
			bool isMoving = Velocity.X != 0 || Velocity.Z != 0;

			_animator.Set("parameters/conditions/IsRunning", (isMoving && _speedMultiplier > 1f));
			_animator.Set("parameters/conditions/IsNotRunning", (isMoving && _speedMultiplier <= 1f));
			_animator.Set("parameters/conditions/IsWalking", (isMoving && _speedMultiplier <= 1f));
			_animator.Set("parameters/conditions/IsNotWalking", !isMoving);
			_animator.Set("parameters/conditions/IsOnFloor", IsOnFloor());
			_animator.Set("parameters/conditions/IsInAir", !IsOnFloor());
		}

		private void TogglePositionFreeze(bool enable)
		{
			AxisLockAngularX = enable;
			AxisLockAngularY = enable;
			AxisLockAngularZ = enable;
		}
	}
}