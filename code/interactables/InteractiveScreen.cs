using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class InteractiveScreen : Node3D
	{
		[Export] private Viewport _sourceViewport;
		[Export] private MeshInstance3D _screenModel;
		[Export] private Area3D _screenCollider;

		private QuadMesh _screen;
		private bool _isMouseInside = false;
		private Vector2 _lastEventPos2D;
		private float _lastEventTime = -1f;

		public override void _Ready()
		{
			_screen = (QuadMesh)_screenModel.Mesh;

			_screenCollider.MouseEntered += MouseEnteredArea;
			_screenCollider.MouseExited += MouseExitedArea;
			_screenCollider.InputEvent += MouseInputEvent;

			((BaseMaterial3D)_screen.SurfaceGetMaterial(0)).AlbedoTexture = _sourceViewport.GetTexture();
			((BaseMaterial3D)_screen.SurfaceGetMaterial(0)).EmissionTexture = _sourceViewport.GetTexture();
		}

		private void MouseEnteredArea()
		{
			_isMouseInside = true;
		}

		private void MouseExitedArea()
		{
			_isMouseInside = false;
		}

		private void MouseInputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIdx)
		{
			// Get mesh size to detect edges and make conversions. This code only support PlaneMesh and QuadMesh.
			Vector2 screenSize = _screen.Size;

			// Event position in Area3D in world coordinate space.
			Vector3 eventPosition3D = eventPosition;

			// Current time in seconds since engine start.
			float now = Time.GetTicksMsec() / 1000.0f;

			// Convert position to a coordinate space relative to the Area3D node.
			// NOTE: affine_inverse accounts for the Area3D node's scale, rotation, and position in the scene!
			eventPosition3D = GetGlobalTransform().AffineInverse() * eventPosition3D;

			Vector2 eventPosition2D = new Vector2();

			if (_isMouseInside)
			{
				// Convert the relative event position from 3D to 2D.
				eventPosition2D = new Vector2(eventPosition3D.X, -eventPosition3D.Y);

				// Right now the event position's range is the following: (-quad_size/2) -> (quad_size/2)
				// We need to convert it into the following range: -0.5 -> 0.5
				eventPosition2D.X /= screenSize.X;
				eventPosition2D.Y /= screenSize.Y;

				// Then we need to convert it into the following range: 0 -> 1
				eventPosition2D.X += 0.5f;
				eventPosition2D.Y += 0.5f;

				// Finally, we convert the position to the following range: 0 -> viewport.size
				eventPosition2D.X *= ((SubViewport)_sourceViewport).Size.X;
				eventPosition2D.Y *= ((SubViewport)_sourceViewport).Size.Y;
				// We need to do these conversions so the event's position is in the viewport's coordinate system.
			}
			else
			{
				if (_lastEventPos2D != null) // Vector2 is never null
				{
					// Fall back to the last known event position.
					eventPosition2D = _lastEventPos2D;
				}
			}

			// Set the event's position and global position.
			((InputEventMouse)@event).Position = eventPosition2D;

			if (@event is InputEventMouse mouseEvent)
			{
				mouseEvent.GlobalPosition = eventPosition2D;
			}

			// Calculate the relative event distance.
			if (@event is InputEventMouseMotion || @event is InputEventScreenDrag)
			{
				// If there is not a stored previous position, then we'll assume there is no relative motion.
				if (_lastEventPos2D == null)
				{
					((InputEventMouseMotion)@event).Relative = new Vector2(0, 0);
				}
				else
				{
					// If there is a stored previous position, then we'll calculate the relative position by subtracting
					// the previous position from the new position. This will give us the distance the event traveled from prev_pos.
					((InputEventMouseMotion)@event).Relative = eventPosition2D - _lastEventPos2D;
					((InputEventMouseMotion)@event).Velocity = ((InputEventMouseMotion)@event).Relative / (now - _lastEventTime);
				}
			}

			// Update last_event_pos2D with the position we just calculated.
			_lastEventPos2D = eventPosition2D;

			// Update last_event_time to current time.
			_lastEventTime = now;

			// Finally, send the processed input event to the viewport.
			_sourceViewport.PushInput(@event);
		}
		/*
			private void UnhandledInput(event)
			{/*
				// Check if the event is a non-mouse/non-touch event
				for mouse_event in [InputEventMouseButton, InputEventMouseMotion, InputEventScreenDrag, InputEventScreenTouch]:
					if is_instance_of(event, mouse_event):
						// If the event is a mouse/touch event, then we can ignore it here, because it will be
						// handled via Physics Picking.
						return
				node_viewport.push_input(event)*/
	}
}