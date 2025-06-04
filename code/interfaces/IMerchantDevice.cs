using ImmersiveSim.Gameplay;

namespace ImmersiveSim.Interfaces
{
	public interface IMerchantDevice
	{
		public bool IsActive();
		public void ChangePowerState(bool isPowered);
		public void ToggleDeviceState(Inventory user);
		public void SetDeviceState(Inventory user, int newState);
	}
}