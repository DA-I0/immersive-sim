using Godot;
using ImmersiveSim.Interfaces;

namespace ImmersiveSim.Gameplay
{
	public partial class PowerPlug : BaseItem
	{
		[Export] private Node3D _parentDevice;

		public override void PickUp(Node newParent, bool takeSingleItem = false, bool setVisible = true)
		{
			DisconnectFromPowerPoint();
			base.PickUp(newParent, takeSingleItem, setVisible);
		}

		public void ConnectToPowerPoint(Node3D target)
		{
			GD.Print($"plugged: {_parentDevice.Name} to {target.GetParent().Name}");
			Freeze = true;
			GlobalPosition = target.GlobalPosition;
			GlobalRotation = target.GlobalRotation;

			if (_parentDevice is IMerchantDevice device)
			{
				device.ChangePowerState(true);
			}
		}

		private void DisconnectFromPowerPoint()
		{
			GD.Print($"{_parentDevice.Name} disconnected from power point");
			if (_parentDevice is IMerchantDevice device)
			{
				device.ChangePowerState(false);
			}
		}

		internal override void ChangeParent(Node newParent, bool freezePhysics = false, bool setVisible = true, bool keepPosition = true)
		{
			if (newParent != GetParent() && newParent != null)
			{
				if (!newParent.Name.ToString().Contains("ItemHoldPosition"))
				{
					newParent = _parentDevice;
				}

				Vector3 adjustedPosition = GlobalPosition;
				_parentNode = newParent;
				Reparent(newParent, keepPosition);

				if (!keepPosition)
				{
					Position = Vector3.Zero;
					Rotation = Vector3.Zero;
				}
				else
				{
					GlobalPosition = adjustedPosition;
				}
			}

			Freeze = freezePhysics;
			Visible = setVisible;
			MarkAsModified();
		}
	}
}