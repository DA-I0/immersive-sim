using Godot;
using ImmersiveSim.Interfaces;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay
{
	public partial class ConversationTrigger : Node, IInteractable
	{
		[Export] private string _startingConversationNode;

		private CharacterBase _character;
		private NPCMovement _charMovement;
		private GameSystem _game;

		public override void _Ready()
		{
			_character = GetNode<CharacterBase>("../CharacterBase");
			_charMovement = GetParent<NPCMovement>();
			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
		}

		public void TriggerPrimaryInteraction(Inventory user)
		{
			StartConversation(user);
		}

		public void TriggerSecondaryInteraction(Inventory user)
		{
			StartConversation(user);
		}

		private void StartConversation(Inventory user)
		{
			_charMovement.ChangeActiveState(Statics.NPCState.Conversation);
			_charMovement.LookAt(((Node3D)user.GetParent()).GlobalPosition);
			_game.Conversation.DisplayConversation(_startingConversationNode, _character);
		}
	}
}
