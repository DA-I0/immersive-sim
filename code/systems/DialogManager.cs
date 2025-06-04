using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Systems
{
	public class DialogManager
	{
		public event DialogNodeData DisplayDialogNode;

		private string _activeDialogNode;
		private CharacterBase _targetNPC;

		private readonly GameSystem _game;

		public DialogManager(GameSystem gameSystem)
		{
			_game = gameSystem;
		}

		internal void SubscribeToEvents()
		{
			_game.UI.DialogUI.TriggeredPlayerChoice += TriggerPlayerChoice;
		}

		internal void DisplayDialog(string dialogID, CharacterBase targetNPC = null)
		{
			if (dialogID == "end" || dialogID == string.Empty)
			{
				CloseDialog();
				return;
			}

			if (_game.UI.ActiveUIState != UIState.Dialog)
			{
				_game.UI.SetUIState(UIState.Dialog);
			}

			_activeDialogNode = dialogID;

			string speaker = string.Empty;
			string content = string.Empty;
			string[] playerChoices = new string[0];

			if (dialogID.Trim() != string.Empty)
			{
				if (targetNPC != null)
				{
					_targetNPC = targetNPC;
					speaker = targetNPC.Name;
				}

				string dialogContentID = _game.GameDatabase.DialogNodes[_activeDialogNode].GenericTextID != string.Empty ? _game.GameDatabase.DialogNodes[_activeDialogNode].GenericTextID : _activeDialogNode;
				content = Godot.TranslationServer.Translate(dialogContentID.ToUpper());

				playerChoices = new string[_game.GameDatabase.DialogNodes[_activeDialogNode].PlayerReplyIDs.Length];

				for (int index = 0; index < playerChoices.Length; index++)
				{
					string choiceNodeID = _game.GameDatabase.DialogNodes[_activeDialogNode].PlayerReplyIDs[index];

					if (ParseChoiceRequirements(_game.GameDatabase.DialogNodes[choiceNodeID].Requirements))
					{
						if (_game.GameDatabase.DialogNodes[choiceNodeID].GenericTextID != string.Empty)
						{
							choiceNodeID = _game.GameDatabase.DialogNodes[choiceNodeID].GenericTextID;
						}

						playerChoices[index] = Godot.TranslationServer.Translate(choiceNodeID.ToUpper());
					}
					else
					{
						playerChoices[index] = string.Empty;
					}
				}
			}

			DisplayDialogNode?.Invoke(speaker, content, playerChoices);
		}

		internal void CloseDialog()
		{
			if (_targetNPC != null)
			{
				((NPCMovement)_targetNPC.BaseMovement).RestorePreviousState();
				_targetNPC = null;
			}

			_game.UI.SetUIState(UIState.None);
			DisplayDialogNode?.Invoke("end", string.Empty, new string[0]);
		}

		private void TriggerPlayerChoice(float index)
		{
			if (index < 0)
			{
				AdvanceDialog();
				return;
			}

			if (ParseChoiceConsequence(_game.GameDatabase.DialogNodes[_game.GameDatabase.DialogNodes[_activeDialogNode].PlayerReplyIDs[(int)index]].Effects))
			{
				string nodeToDisplay = _game.GameDatabase.DialogNodes[_activeDialogNode].PlayerReplyIDs[(int)index];
				DisplayDialog(_game.GameDatabase.DialogNodes[nodeToDisplay].NextNodeID, _targetNPC);
			}
		}

		private bool ParseChoiceRequirements(string[] requirements)
		{
			foreach (string requirement in requirements)
			{
				if (_game.ParseTriggeredAction(requirement, ParseActionTarget(requirement.Split(':'))) == false)
				{
					return false;
				}
			}

			return true;
		}

		private bool ParseChoiceConsequence(string[] choiceEffects)
		{
			bool continueDialog = true;

			foreach (string action in choiceEffects)
			{
				if (_game.ParseTriggeredAction(action, ParseActionTarget(action.Split(':'))) == false)
				{
					continueDialog = false;
				}
			}

			return continueDialog;
		}

		private CharacterBase ParseActionTarget(string[] actionParameters)
		{
			if (actionParameters[^1] == "player")
			{
				return _game.Player;
			}
			else
			{
				return _targetNPC;
			}
		}

		private void AdvanceDialog()
		{
			DisplayDialog(_game.GameDatabase.DialogNodes[_activeDialogNode].NextNodeID);
		}
	}
}