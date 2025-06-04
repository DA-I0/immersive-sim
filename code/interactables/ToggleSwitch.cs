using Godot;
using ImmersiveSim.Interfaces;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class ToggleSwitch : Node3D, IInteractable
	{
		[Export] protected SwitchType _switchType;
		[Export] protected int _targetState;
		[Export] protected Node3D[] _connectedDevices;

		public void AssignTarget(Node3D[] devices, int newTargetState)
		{
			_connectedDevices = devices;
			_targetState = newTargetState;
		}

		public void AssignTarget(Node3D devices, int newTargetState)
		{
			_connectedDevices = new Node3D[] { devices };
			_targetState = newTargetState;
		}

		public void ToggleActive(bool setActive)
		{
			GetNode<CollisionShape3D>("Collider/CollisionShape").Disabled = !setActive;
		}

		public virtual void TriggerPrimaryInteraction(Inventory user)
		{
			foreach (IDevice device in _connectedDevices)
			{
				if (device != null)
				{
					if (_switchType == SwitchType.Toggle)
					{
						device.ToggleDeviceState();
					}
					else
					{
						device.SetDeviceState(_targetState);
					}
				}
			}
		}

		public virtual void TriggerSecondaryInteraction(Inventory user)
		{
			TriggerPrimaryInteraction(user);
		}
	}
}