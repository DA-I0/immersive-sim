using Godot;
using ImmersiveSim.Interfaces;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay
{
	public partial class Interaction : Node
	{
		public event InteractableTargeted InteractionTarget;

		[Export] private float _zoomMultiplier = 0.3f; // temp, move to user settings
		[Export] private float _maxInteractionRange = 1.0f;

		private CameraState _cameraMode = CameraState.Idle;
		private float _activeFoV;

		[Export] private float _cameraMaxVerticalAngle = 80.0f; // move camera controls to separate class
		[Export] private float _cameraMaxHorizontalAngle = 110.0f; // use for freelook, move camera controls to separate class

		private float _cameraAngle = 0;

		private bool _itemPlacementMode = false;
		private ShapeCast3D _placementCast;
		private Vector3 _placementPosition;
		private Node3D _targetItem;

		private int _placementRotationAxis = 1;
		private Vector3 _placementRotation = Vector3.Zero;
		private Vector3 _placementNormal = Vector3.Zero;
		private Node3D _placementTarget;
		private Node3D _placeholderPosition;

		private GameSystem _game;
		private PlayerBase _character;
		private Camera3D _camera;
		private Vector2 _screenCenter;

		public CameraState CameraMode
		{
			get { return _cameraMode; }
		}

		public bool IsItemPlacementMode
		{
			get { return _itemPlacementMode; }
		}

		private int FoV
		{
			get { return _game.Settings.FieldOfView; }
		}

		public override void _Ready()
		{
			_character = GetNode<PlayerBase>("../CharacterBase");
			_camera = GetNode<Camera3D>("../Camera");
			_placeholderPosition = _camera.GetChild<Node3D>(2);

			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());

			_activeFoV = FoV;

			_game.Settings.SettingsUpdated += BaseFoVChanged;
			_character.SetForDestruction += UnsubscribeFromEvents;

			SetupPlacementCast();
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_game.State != GameState.Gameplay)
			{
				return;
			}

			UpdateFoV(); // there are some issues with interactive screens when zoomed in, check if some kind of refresh needed
			UpdateScreenCenterPosition();

			if (_itemPlacementMode)
			{
				// CalculateItemPlacement(); // shape casting version
				CalculateItemPlacementAlt(); // raycast version
			}
			else
			{
				DetectInteractionOptions();
			}

			ResetCamera();
		}

		public override void _Input(InputEvent @event)
		{
			if (_game.State != GameState.Gameplay)
			{
				return;
			}

			if (@event is InputEventMouseMotion mouseEvent)
			{
				MouseEvents(mouseEvent);
			}

			ActionEvents(@event);
		}

		public void ResetCamera(bool resetFreeLook = false)
		{
			if (resetFreeLook)
			{
				ToggleCameraMode(false);
			}

			if (_cameraMode == CameraState.Freelook)
			{
				return;
			}

			Vector3 defaultRotation = _camera.RotationDegrees;
			defaultRotation.Y = 0;
			_camera.RotationDegrees = defaultRotation;
		}

		public void ToggleCameraMode(bool isFreelook)
		{
			if (isFreelook || _character.CharMovement.ActiveStance == Stance.Sitting)
			{
				_cameraMode = CameraState.Freelook;
			}
			else
			{
				_cameraMode = CameraState.Idle;
			}
		}

		private void MouseEvents(InputEventMouseMotion mouseEvent)
		{
			float changeValue;

			if (!Input.IsActionPressed("stance_smooth_lean"))
			{
				if (CameraMode == CameraState.Idle && _character.CharMovement.ActiveStance != Stance.Sitting)
				{
					_character.CharMovement.RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * _game.Settings.CameraSensitivity));
				}
				else
				{
					changeValue = -mouseEvent.Relative.X * _game.Settings.CameraSensitivity;
					float newRotation = LimitRotationY(changeValue);
					_camera.RotateY(Mathf.DegToRad(newRotation));
				}
			}

			if (Input.IsActionPressed("stance_smooth_height"))
			{
				return;
			}

			changeValue = -mouseEvent.Relative.Y * _game.Settings.CameraSensitivity * _game.Settings.CameraVerticalDirection;

			if (_cameraAngle + changeValue > -_cameraMaxVerticalAngle && _cameraAngle + changeValue < _cameraMaxVerticalAngle)
			{
				_cameraAngle += changeValue;
				_camera.RotateObjectLocal(Vector3.Left, Mathf.DegToRad(-changeValue));
			}
		}

		private void ActionEvents(InputEvent @event)
		{
			if (@event.IsActionPressed("action_interact") && !@event.IsActionPressed("action_interact_secondary", exactMatch: true))
			{
				if (_itemPlacementMode)
				{
					PlaceItem();
				}
				else
				{
					object target = SearchForInteractables();

					if (target != null && target is IInteractable device)
					{
						device.TriggerPrimaryInteraction(_character.CharInventory);
					}
				}
			}

			if (@event.IsActionPressed("action_interact_secondary") && !@event.IsActionPressed("action_interact", exactMatch: true))
			{
				object target = SearchForInteractables();

				if (target != null && target is IInteractable device)
				{
					device.TriggerSecondaryInteraction(_character.CharInventory);
				}
			}

			if (@event.IsActionPressed("action_place_item"))
			{
				ToggleItemPlacement(!_itemPlacementMode);
			}

			if (_itemPlacementMode && @event.IsActionPressed("action_placement_rotation_axis"))
			{
				_placementRotationAxis = (_placementRotationAxis < 2) ? _placementRotationAxis + 1 : 0;
			}

			if (@event.IsActionPressed("action_zoom_in"))
			{
				ToggleCameraZoom(true);
			}

			if (@event.IsActionReleased("action_zoom_in"))
			{
				ToggleCameraZoom(false);
			}

			if (@event.IsActionPressed("action_free_look"))
			{
				ToggleCameraMode(true);
			}

			if (@event.IsActionReleased("action_free_look"))
			{
				ToggleCameraMode(false);
			}

			if (@event.IsAction("move_speed_increase"))
			{
				AdjustPlacementRotation(0.1f);
				return;
			}

			if (@event.IsAction("move_speed_decrease"))
			{
				AdjustPlacementRotation(-0.1f);
				return;
			}
		}

		private void AdjustPlacementRotation(float direction)
		{
			if (!_itemPlacementMode)
			{
				return;
			}

			switch (_placementRotationAxis)
			{
				case 1:
					_placementRotation.Y += direction;
					break;

				case 2:
					_placementRotation.Z += direction;
					break;

				default:
					_placementRotation.X += direction;
					break;
			}

			if (_placementRotation.X > 359 || _placementRotation.X < -359)
			{
				_placementRotation.X = 0;
				return;
			}

			if (_placementRotation.Y > 359 || _placementRotation.Y < -359)
			{
				_placementRotation.Y = 0;
				return;
			}

			if (_placementRotation.Z > 359 || _placementRotation.Z < -359)
			{
				_placementRotation.Z = 0;
				return;
			}
		}

		private float LimitRotationY(float rotation)
		{
			float rotationResult = _camera.RotationDegrees.Y + rotation;
			if (rotationResult < -_cameraMaxHorizontalAngle || rotationResult > _cameraMaxHorizontalAngle)
			{
				return 0;
			}

			return rotation;
		}

		private void UpdateScreenCenterPosition()
		{
			_screenCenter = GetViewport().GetVisibleRect().Size / 2;
		}

		private void DetectInteractionOptions()
		{
			object target = SearchForInteractables();
			InteractionTarget?.Invoke(target);
		}

		private object SearchForInteractables()
		{
			if (_itemPlacementMode)
			{
				return null;
			}

			var spaceState = _camera.GetWorld3D().DirectSpaceState;
			Vector3 from = _camera.ProjectRayOrigin(_screenCenter);
			Vector3 to = from + _camera.ProjectRayNormal(_screenCenter) * 1.5f;

			var query = PhysicsRayQueryParameters3D.Create(from, to);
			query.CollideWithAreas = false;//true;

			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				if (result["collider"].Obj is IInteractable && result["collider"].Obj is not NPCInteractionTarget)
				{
					GD.Print($"-> returning: {((Node)result["collider"].Obj).Name}");
					return (Node)result["collider"].Obj;
				}

				if (result["collider"].Obj is NPCMovement)
				{
					return ((Node)result["collider"].Obj).GetNode("CharacterConversation");
				}

				GD.Print($"-> returning: {((Node)result["collider"].Obj).GetParent().Name}");
				return ((Node)result["collider"].Obj).GetParent();
			}

			return null;
		}

		private void SetupPlacementCast()
		{
			_placementCast = new ShapeCast3D
			{
				Position = _camera.Position,
				Shape = new BoxShape3D(),
				MaxResults = 4
			};
			((BoxShape3D)_placementCast.Shape).Size = new Vector3(0.01f, 0.01f, 0.01f);
			GetParent().CallDeferred(Node.MethodName.AddChild, _placementCast);
		}

		private void ToggleItemPlacement(bool enable)
		{
			if (_character.CharInventory.SelectedItem == null)
			{
				_itemPlacementMode = false;
				return;
			}

			_itemPlacementMode = enable;
			_placementCast.Enabled = _itemPlacementMode;

			if (!_itemPlacementMode)
			{
				_targetItem.Position = Vector3.Zero;
				_targetItem.Rotation = Vector3.Zero;
				_targetItem = null;
			}
			else
			{
				_placementRotationAxis = 1;
				_placementRotation = Vector3.Zero;
				_targetItem = _character.CharInventory.SelectedItem;
				_placementCast.Shape = _targetItem.GetNode<CollisionShape3D>("Collider").Shape;
				InteractionTarget?.Invoke(null);
			}
		}

		private void CalculateItemPlacement()
		{
			// current system, sends out a proper shape, can be finnicky with smaller spaces

			if (_character.CharInventory.SelectedItem == null)
			{
				ToggleItemPlacement(false);
				return;
			}

			Vector3 globalPlacementRotation = GetParent<Node3D>().GlobalRotation + _placementRotation;

			_placementCast.Position = _camera.Position;
			_placementCast.TargetPosition = _placementCast.ToLocal(_placeholderPosition.GlobalPosition);
			_placementCast.GlobalRotation = globalPlacementRotation;

			if (_placementCast.GetCollisionCount() < 1)
			{
				_targetItem.Position = Vector3.Zero;
				return;
			}

			_placementPosition = _placementCast.GetCollisionPoint(0) + ((HelperMethods.GetShapeDimensions(_placementCast.Shape) / 2) * _placementCast.GetCollisionNormal(0));
			_targetItem.GlobalPosition = _placementPosition;
			_targetItem.GlobalRotation = globalPlacementRotation;
		}

		private void CalculateItemPlacementAlt()
		{
			// rough version of an alternative approach
			// less problems with putting things into smaller spaces like drawers but has some other issues that would have to be resolved
			// for example it allows players to put big objects into places they don't belong (clipping is usually resolved but sometimes placed object can get stuck)

			if (_character.CharInventory.SelectedItem == null)
			{
				ToggleItemPlacement(false);
				return;
			}

			var spaceState = _camera.GetWorld3D().DirectSpaceState;
			Vector3 raycastOrigin = _camera.ProjectRayOrigin(_screenCenter);
			Vector3 raycastEnd = raycastOrigin + _camera.ProjectRayNormal(_screenCenter) * StaticValues.ItemPlacementRange;

			var query = PhysicsRayQueryParameters3D.Create(raycastOrigin, raycastEnd);
			query.CollideWithAreas = true;

			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				_placementTarget = (Node3D)result["collider"];
				_placementNormal = (Vector3)result["normal"];

				if (((BaseItem)_targetItem).CanBeHanged && IsHangingPosition())
				{
					_placementPosition = (Vector3)result["position"] + new Vector3(0.01f, 0f, 0.01f) * _placementNormal;
					_targetItem.GlobalPosition = _placementPosition;
					_targetItem.LookAt((Vector3)result["position"] - new Vector3(0.01f, 0f, 0.01f) * _placementNormal);
					_placementRotation = _targetItem.GlobalRotation;
				}
				else
				{
					_placementPosition = (Vector3)result["position"] + ((HelperMethods.GetShapeDimensions(_placementCast.Shape) / 2) * _placementNormal);
					_targetItem.GlobalPosition = _placementPosition;

					_targetItem.GlobalRotation = GetParent<Node3D>().GlobalRotation + _placementRotation;
				}
			}
			else
			{
				_targetItem.Position = Vector3.Zero;
				_targetItem.RotationDegrees = Vector3.Zero;
				_placementNormal = Vector3.Zero;
			}
		}

		private async void PlaceItem()
		{
			_character.CharInventory.DropSelectedItem();

			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

			if (((BaseItem)_targetItem).CanBeHanged && IsHangingPosition())
			{
				((BaseItem)_targetItem).ChangeParent(_placementTarget, true, true);
				_targetItem.GlobalPosition = _placementPosition;
				_targetItem.GlobalRotation = _placementRotation;
			}
			else
			{
				_targetItem.GlobalPosition = _placementPosition + Vector3.Zero * 0.01f;
				_targetItem.GlobalRotation = GetParent<Node3D>().GlobalRotation + _placementRotation;
			}

			ToggleItemPlacement(false);
		}

		private bool IsHangingPosition()
		{
			return _placementNormal.X == 1 || _placementNormal.X == -1 || _placementNormal.Z == 1 || _placementNormal.Z == -1;
		}

		private void ToggleCameraZoom(bool zoomIn)
		{
			_activeFoV = zoomIn ? FoV * (1f - _zoomMultiplier) : FoV;
		}

		private void UpdateFoV()
		{
			float newFoV = _activeFoV;

			if (Mathf.Abs(_camera.Fov - _activeFoV) > 0.1f)
			{
				newFoV = Mathf.Lerp(_camera.Fov, _activeFoV, 0.1f);
			}

			_camera.Fov = newFoV;
		}

		private void BaseFoVChanged()
		{
			ToggleCameraZoom(false);
		}

		private void UnsubscribeFromEvents()
		{
			_game.Settings.SettingsUpdated -= BaseFoVChanged;
			_character.SetForDestruction -= UnsubscribeFromEvents;
		}
	}
}