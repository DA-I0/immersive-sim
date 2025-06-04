using Godot;

public partial class OwnerController : Area3D
{
	private Node _previousOwner;

	private void TakeOverEntity(Node3D target)
	{
		_previousOwner = target.GetParent();
		GetParent().AddChild(target);
		target.RemoveChild(target);

		// _previousOwner = target.Owner;
		// target.Owner = GetParent();
	}

	private void RestoreEntity(Node3D target)
	{
		_previousOwner.AddChild(target);
		GetParent().RemoveChild(target);

		// target.Owner = _previousOwner;
	}
}
