using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class CharacterStatus : Control
	{
		protected ProgressBar _characterHealth;
		protected ProgressBar _characterStamina;
		protected ProgressBar _characterStaminaPenalty;

		private Systems.GameSystem _game;

		public override void _Ready()
		{
			Initialize();
		}

		protected virtual void Initialize()
		{
			_characterHealth = GetNode<ProgressBar>("InfoControls/HealthBar");
			_characterStamina = GetNode<ProgressBar>("InfoControls/Stamina/StaminaBar");
			_characterStaminaPenalty = GetNode<ProgressBar>("InfoControls/Stamina/StaminaPenaltyBar");

			_game = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.NewPlayerSpawned += SetPlayerReferences;

			GetParent<UIHandler>().StateUpdated += ToggleVisibility;
		}

		private void SetPlayerReferences()
		{
			_game.Player.SetForDestruction += UnsetPlayerReferences;
			_game.Player.CharStatus.HealthChanged += UpdateHealthDisplay;
			_game.Player.CharStatus.StaminaChanged += UpdateStaminaDisplay;
			_game.Player.CharStatus.StaminaPenaltyChanged += UpdateStaminaPenaltyDisplay;
		}

		private void UnsetPlayerReferences()
		{
			_game.Player.CharStatus.HealthChanged -= UpdateHealthDisplay;
			_game.Player.CharStatus.StaminaChanged -= UpdateStaminaDisplay;
			_game.Player.CharStatus.StaminaPenaltyChanged -= UpdateStaminaPenaltyDisplay;
		}

		protected void UpdateHealthDisplay(float value)
		{
			_characterHealth.Value = value;
		}

		protected void UpdateStaminaDisplay(float value)
		{
			_characterStamina.Value = value;
		}

		protected void UpdateStaminaPenaltyDisplay(float value)
		{
			_characterStaminaPenalty.Value = value;
		}

		private void ToggleVisibility(UIState activeState)
		{
			Visible = (activeState == UIState.None);
		}
	}
}