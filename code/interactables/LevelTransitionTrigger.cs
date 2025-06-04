using Godot;
using ImmersiveSim.Interfaces;

namespace ImmersiveSim.Gameplay
{
	public partial class LevelTransitionTrigger : Node, IInteractable
	{
		[Export] private string _targetLevel;
		[Export] private string _targetSpawnPoint;

		private Systems.GameSystem _game;

		public override void _Ready()
		{
			_game = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
		}

		public void TriggerPrimaryInteraction(Inventory user)
		{
			_game.Level.ChangeLevel(_targetLevel, _targetSpawnPoint);
		}

		public void TriggerSecondaryInteraction(Inventory user)
		{
			TriggerPrimaryInteraction(user);
		}
	}
}
