using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class Movement : MovementBase, Interfaces.IMovement
	{
		[ExportGroup("References")]
		[Export] protected Camera3D _camera;

		private PlayerBase _character;
		private ShapeCast3D _leanCast;

		public override void _Ready()
		{
			_character = GetNode<PlayerBase>("CharacterBase");
			base._Ready();
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_stance == Stance.Sitting || _character.Game.State == GameState.Menu)
			{
				return;
			}

			UpdateVelocity(delta);
			MoveAndSlide();
			CollideWithRigidbodies();
			CheckCollisionDamage();
			_characterVelocity = Velocity.Length();
			// Peek();
			Lean();
		}

		public override void _Input(InputEvent @event)
		{
			if (_character.Game.State != GameState.Gameplay || _stance == Stance.Sitting)
			{
				return;
			}

			if (@event is InputEventMouseMotion mouseEvent)
			{
				MouseEvents(mouseEvent);
				return;
			}

			ActionEvents(@event);
		}

		public override void ChangeStance(Stance newStance)
		{
			_stance = newStance;

			if (newStance == Stance.Idle)
			{
				_stanceMultiplier = 1f;
				_character.CharInteraction.ResetCamera(true);
			}

			if (newStance == Stance.Sitting)
			{
				_character.CharInteraction.ToggleCameraMode(true);
			}

			UpdateCharacterCollider();
		}

		protected override void SetupShapeCastComponents()
		{
			SetupMovementCast();
			SetupStanceCast();
			SetupLeanCast();
		}

		private void SetupMovementCast()
		{
			_movementCast = new ShapeCast3D
			{
				Position = new Vector3(0, 0.4f, -0.4f),
				Shape = new BoxShape3D(),
				TargetPosition = Vector3.Down,
				MaxResults = 4
			};

			BoxShape3D characterShape = (BoxShape3D)GetNode<CollisionShape3D>("Collider").Shape;
			((BoxShape3D)_movementCast.Shape).Size = new Vector3(characterShape.Size.X, 0.05f, characterShape.Size.Z);

			CallDeferred(Node.MethodName.AddChild, _movementCast);
		}

		private void SetupStanceCast()
		{
			_stanceCast = new ShapeCast3D
			{
				Position = Vector3.Up * (_characterHeight / 2f),
				Shape = new SphereShape3D(),
				MaxResults = 4
			};
			((SphereShape3D)_stanceCast.Shape).Radius = 0.05f;
			AddChild(_stanceCast);
		}

		private void SetupLeanCast()
		{
			_leanCast = new ShapeCast3D
			{
				Position = _camera.Position,
				Shape = new SphereShape3D(),
				TargetPosition = Vector3.Left,
				MaxResults = 4
			};
			((SphereShape3D)_leanCast.Shape).Radius = 0.15f;
			AddChild(_leanCast);
		}

		private void MouseEvents(InputEventMouseMotion mouseEvent)
		{
			if (_character.CharInteraction.CameraMode != CameraState.Idle)
			{
				return;
			}

			if (Input.IsActionPressed("stance_smooth_height"))
			{
				_stanceMultiplier -= mouseEvent.Relative.Y * 0.01f * _character.Game.Settings.StanceSensitivity;
				_stanceMultiplier = Mathf.Clamp(_stanceMultiplier, 0.4f, 1f);
				UpdateCharacterCollider();
			}

			if (Input.IsActionPressed("stance_smooth_lean"))
			{
				_leanCurrent += mouseEvent.Relative.X * 0.01f * _character.Game.Settings.LeanSensitivity;
				_resetLean = false;
				// Lean();
				return;
			}
		}

		private void ActionEvents(InputEvent @event)
		{
			if (@event.IsAction("move_speed_increase"))
			{
				AdjustMovementSpeed(1);
				return;
			}

			if (@event.IsAction("move_speed_decrease"))
			{
				AdjustMovementSpeed(-1);
				return;
			}

			if (@event.IsActionPressed("stance_crouch"))
			{
				_stanceMultiplier = _stanceMin;
				_crouchTimer = Time.GetTicksMsec();
				UpdateCharacterCollider();
				return;
			}

			if (@event.IsActionReleased("stance_crouch"))
			{
				if (_crouchTimer + StaticValues.CrouchToggleDelay > Time.GetTicksMsec())
				{
					if (_stance == Stance.Idle)
					{
						_stance = Stance.Crouch;
						return;
					}
					else
					{
						_stance = Stance.Idle;
					}
				}

				_stanceMultiplier = 1f;
				UpdateCharacterCollider();
				return;
			}

			if (@event.IsActionPressed("stance_prone"))
			{
				if (_stance == Stance.Prone)
				{
					_stance = Stance.Idle;
					_stanceMultiplier = 1f;
				}
				else
				{
					_stance = Stance.Prone;
					_stanceMultiplier = _stanceProne;
				}

				UpdateCharacterCollider();
				return;
			}

			if (@event.IsActionPressed("move_sprint")) // needs to be constantly checked and applied, otherwise sprint stops if one adjusts speed multiplier
			{
				_cachedSpeedMultiplier = _speedMultiplier;
				_speedMultiplier = _speedMultiplierSprint;
				return;
			}

			if (@event.IsActionReleased("move_sprint"))
			{
				_speedMultiplier = _cachedSpeedMultiplier;
				return;
			}
		}

		private void CollideWithRigidbodies()
		{
			for (int i = 0; i < GetSlideCollisionCount(); i++)
			{
				KinematicCollision3D collision = GetSlideCollision(i);

				if (collision.GetCollider() is RigidBody3D rigidbody)
				{
					rigidbody.ApplyImpulse((-collision.GetNormal() * (_characterVelocity * rigidbody.Mass * 0.5f))); // can't use the target mass like this cause it will let you run into 100kg boxes no problem

					if (rigidbody.LinearVelocity.Length() > _colliderVelocity)
					{
						_colliderVelocity = rigidbody.LinearVelocity.Length();
					}
				}

				if (collision.GetCollider() is CharacterBody3D otherCharacter)
				{
					if (otherCharacter.Velocity.Length() > _colliderVelocity)
					{
						_colliderVelocity = otherCharacter.Velocity.Length();
					}
				}
			}
		}

		private void CheckCollisionDamage()
		{
			float collisionSpeed = _characterVelocity + _colliderVelocity;
			if (collisionSpeed > StaticValues.CollisionVelocityDamageThreshold)
			{
				_character.CharStatus.TakeDamage(collisionSpeed * StaticValues.CollisionDamageMultiplier, DamageType.Collision);
			}

			_colliderVelocity = 0;
		}

		private void UpdateVelocity(double delta)
		{
			Vector3 newVelocity = Velocity;

			// Add the gravity.
			if (!IsOnFloor())
			{
				newVelocity += GetGravity() * (float)delta;
			}
			else
			{
				if (_character.Game.State == GameState.Gameplay)
				{
					float equipmentWeightMultiplier = Mathf.Clamp(_character.CharInventory.CarryWeightRatio - 1, 1f, 3f);

					// Handle Jump.
					if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
					{
						newVelocity.Y = _jumpVelocity / equipmentWeightMultiplier;
					}

					Vector2 inputDirection = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
					Vector3 movementDirection = (Transform.Basis * new Vector3(inputDirection.X, 0, inputDirection.Y)).Normalized();
					AdjustForElevation(inputDirection);

					// move to a new method and recalculate only on carryweight change, speed and stance change, cache the result?
					float stanceSpeedMultiplier = (_stanceMultiplier < 0.75f) ? _stanceMultiplier + 0.2f : 1f;
					float combinedSpeedMultiplier = (_speedMultiplier * stanceSpeedMultiplier) / equipmentWeightMultiplier;
					//

					combinedSpeedMultiplier *= (inputDirection.Y > 0) ? StaticValues.MoveBackwardMultiplier : 1f;
					_currentSpeed = _defaultSpeed * combinedSpeedMultiplier;

					if (movementDirection != Vector3.Zero)
					{
						// velocity.X = movementDirection.X * _currentSpeed;
						// velocity.Z = movementDirection.Z * _currentSpeed;

						newVelocity.X = Mathf.Lerp(Velocity.X, movementDirection.X * _currentSpeed, StaticValues.Momentum);
						newVelocity.Z = Mathf.Lerp(Velocity.Z, movementDirection.Z * _currentSpeed, StaticValues.Momentum);

						if (_speedMultiplier > _speedMultiplierMax)
						{
							_character.CharStatus.ReduceStamina(-StaticValues.MovementCost * combinedSpeedMultiplier);
						}
					}
					else
					{
						newVelocity.X = Mathf.MoveToward(Velocity.X, 0, _currentSpeed);
						newVelocity.Z = Mathf.MoveToward(Velocity.Z, 0, _currentSpeed);
					}
				}
			}

			Velocity = newVelocity;
		}

		private void UpdateCharacterCollider()
		{
			CheckHeightLimits();

			float newHeight = (_characterHeight * _stanceMultiplier) + _peekHelper; // TODO: remove peek?
			float heightChange = newHeight - ((BoxShape3D)_collider.Shape).Size.Y;

			if (heightChange > 0 && IsOnFloor())
			{
				Vector3 adjustedCharacterPosition = Position;
				adjustedCharacterPosition.Y += (heightChange / 2) + 0.01f;
				Position = adjustedCharacterPosition;
			}

			Vector3 newSize = ((BoxShape3D)_collider.Shape).Size;
			newSize.Y = newHeight;
			((BoxShape3D)_collider.Shape).Size = newSize;
			UpdateCameraPosition();
			AdjustForElevation(Vector2.Zero);
		}

		private void CheckHeightLimits()
		{
			float collisionCheckedHeight = CheckHeightCollisions();

			if (_stanceMultiplier > collisionCheckedHeight)
			{
				_stanceMultiplier = collisionCheckedHeight + 0.015f;
			}

			if (_stance == Stance.Prone)
			{
				_stanceMultiplier = _stanceProne;
			}
			else
			{
				if (_stanceMultiplier < _stanceMin)
				{
					if (_stanceMultiplier < _stanceMin - 0.1f)
					{
						_stanceMultiplier = _stanceProne;
						_stance = Stance.Prone;
					}
					else
					{
						_stanceMultiplier = _stanceMin;
					}
				}
			}

			if (_stance == Stance.Sitting)
			{
				_stanceMultiplier = _stanceProne; // needed?
			}

			_stanceMultiplier = HelperMethods.RoundFloat(_stanceMultiplier, 3);
		}

		private void UpdateCameraPosition()
		{
			Vector3 adjustedCameraPosition = _camera.Position;
			adjustedCameraPosition.Y = _collider.Position.Y + (((BoxShape3D)_collider.Shape).Size.Y / 2) - 0.1f;
			_camera.Position = adjustedCameraPosition;
		}

		private float CheckHeightCollisions()
		{
			_stanceCast.TargetPosition = Vector3.Up * (_characterHeight - (((BoxShape3D)_collider.Shape).Size.Y / 2));
			return _stanceCast.GetClosestCollisionSafeFraction();
		}

		private void AdjustMovementSpeed(int direction)
		{
			if (_character.CharInteraction.IsItemPlacementMode)
			{
				return;
			}

			_speedMultiplier += StaticValues.SpeedChangeStep * direction;
			_speedMultiplier = Mathf.Clamp(_speedMultiplier, _speedMultiplierMin, _speedMultiplierMax);
		}

		private void Peek()
		{
			if (Input.IsActionPressed("stance_lean_left") && Input.IsActionPressed("stance_lean_right"))
			{
				_peekHelper = 0.25f;
			}
			else
			{
				_peekHelper = 0;
			}

			UpdateCharacterCollider();
		}

		private void Lean()
		{
			if (_character.Game.State == GameState.Gameplay && IsOnFloor())
			{
				if (Input.IsActionJustPressed("stance_smooth_lean"))
				{
					_leanTimer = Time.GetTicksMsec();
				}

				if (!Input.IsActionPressed("stance_smooth_lean"))
				{
					if (Input.IsActionPressed("stance_lean_left"))
					{
						_leanCurrent = Mathf.Lerp(_leanCurrent, -1, 0.1f);
						_resetLean = true;
					}

					if (Input.IsActionPressed("stance_lean_right"))
					{
						_leanCurrent = Mathf.Lerp(_leanCurrent, 1, 0.1f);
						_resetLean = true;
					}
				}

				if (Input.IsActionJustReleased("stance_smooth_lean") && Time.GetTicksMsec() < (_leanTimer + 150))
				{
					_resetLean = true;
				}

				CheckLeanLimits();
			}

			ResetLean();
			ApplyLean();
		}

		private void ResetLean()
		{
			if (!IsLeaning())
			{
				_leanCurrent = Mathf.Lerp(_leanCurrent, 0.0f, 0.1f);

				if (_leanCurrent < 0.01f && _leanCurrent > -0.01f)
				{
					_leanCurrent = 0;
				}
			}
		}

		private void CheckLeanLimits()
		{
			if (IsLeaning() && _leanCurrent < 0f)
			{
				float collisionLeanMax = CheckLeanCollisions(Vector3.Left);
				GD.Print($"Movement:: lean collision: {collisionLeanMax}");

				if (_leanMax > collisionLeanMax)
				{
					_leanCurrent = -(collisionLeanMax / _leanMax);
				}
			}

			if (IsLeaning() && _leanCurrent > 0f)
			{
				float collisionLeanMax = CheckLeanCollisions(Vector3.Right);
				GD.Print($"Movement:: lean collision: {collisionLeanMax}");

				if (_leanMax > collisionLeanMax)
				{
					_leanCurrent = (collisionLeanMax / _leanMax);
				}
			}

			_leanCurrent = Mathf.Clamp(_leanCurrent, -1, 1);
		}

		private float CheckLeanCollisions(Vector3 direction)
		{
			// stanceCast.Position = _camera.Position;
			_leanCast.TargetPosition = direction;
			return _leanCast.GetClosestCollisionSafeFraction();
		}

		private void ApplyLean()
		{
			_leanCurrent = HelperMethods.RoundFloat(_leanCurrent, 3);
			Vector3 temp = Vector3.Zero;

			temp.X = _leanCurrent * _leanMax;
			temp.Y = _camera.Position.Y;
			temp.Z = _camera.Position.Z;

			_camera.Position = temp;

			temp.X = _camera.Rotation.X;
			temp.Y = _camera.Rotation.Y;
			temp.Z = Mathf.DegToRad(-_leanCurrent * _leanMaxAngle);
			_camera.Rotation = temp;
		}

		private bool IsLeaning()
		{
			return (IsOnFloor()
				&& (!_resetLean || (Input.IsActionPressed("stance_lean_left") || Input.IsActionPressed("stance_lean_right")))
				&& !(Input.IsActionPressed("stance_lean_left") && Input.IsActionPressed("stance_lean_right")));
		}
	}
}