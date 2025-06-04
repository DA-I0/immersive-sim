using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class SelectorSwitch : ToggleSwitch
	{
		public int TargetState
		{
			get { return _targetState; }
		}

		public override void TriggerPrimaryInteraction(Inventory user)
		{
			foreach (Interfaces.IMerchantDevice device in _connectedDevices)
			{
				if (device != null)
				{
					if (_switchType == SwitchType.Toggle)
					{
						device.ToggleDeviceState(user);
					}
					else
					{
						device.SetDeviceState(user, _targetState);
					}
				}
			}
		}

		public override void TriggerSecondaryInteraction(Inventory user)
		{
			// throw new System.NotImplementedException();
		}
	}
}