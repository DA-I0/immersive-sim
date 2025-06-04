using Godot;
using ImmersiveSim.Interfaces;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay
{
	public partial class DialogTrigger : Node, IInteractable
	{
		[Export] private string _startingDialogNode;

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
			StartDialog(user);
		}

		public void TriggerSecondaryInteraction(Inventory user)
		{
			StartDialog(user);
		}

		private void StartDialog(Inventory user)
		{
			_charMovement.ChangeActiveState(Statics.NPCState.Dialog);
			_charMovement.LookAt(((Node3D)user.GetParent()).GlobalPosition);
			_game.Dialog.DisplayDialog(_startingDialogNode, _character);
		}
	}
}
