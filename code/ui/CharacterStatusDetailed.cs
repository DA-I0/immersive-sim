using Godot;
using ImmersiveSim.Gameplay;

namespace ImmersiveSim.UI
{
	public partial class CharacterStatusDetailed : CharacterStatus
	{
		private Label _characterName;
		private Label _equipmentWeight;

		private Systems.GameSystem _game;
		// private Inventory _inventory;

		public override void _Ready()
		{
			SetupReferences();
		}

		private async void SetupReferences()
		{
			_characterHealth = GetNode<ProgressBar>("InfoControls/CharacterHealth/HealthBar");
			_characterStamina = GetNode<ProgressBar>("InfoControls/CharacterStamina/StaminaBar");
			_characterName = GetNode<Label>("InfoControls/CharacterName");
			_equipmentWeight = GetNode<Label>("InfoControls/CharacterWeight/WeightValue");

			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

			_game = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.NewPlayerSpawned += SetPlayerReferences;
		}


		private void SetPlayerReferences()
		{
			_game.Player.SetForDestruction += UnsetPlayerReferences;
			_characterName.Text = _game.Player.CharSheet.CharacterName;
			_game.Player.CharInventory.InventoryUpdated += UpdateEquipmentWeight;
		}

		private void UnsetPlayerReferences()
		{
			_game.Player.CharInventory.InventoryUpdated -= UpdateEquipmentWeight;

		}

		private void UpdateEquipmentWeight()
		{
			_equipmentWeight.Text = $"{_game.Player.CharInventory.EquipmentWeight}/{_game.Player.CharInventory.MaxCarryWeight}kg";
		}
	}
}