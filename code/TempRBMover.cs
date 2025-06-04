using Godot;

public partial class TempRBMover : RigidBody3D
{
	[Export] private Vector3 _movementVelocity;

	private int _direction = 0;

	public override void _PhysicsProcess(double delta)
	{
		ApplyForce(_movementVelocity * _direction);
	}

	private void Reverse()
	{
		_direction = 1;
	}
}