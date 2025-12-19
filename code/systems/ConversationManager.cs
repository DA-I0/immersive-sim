using ImmersiveSim.Gameplay;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Systems
{
	public class ConversationManager
	{
		public event ConversationNodeData DisplayConversationNode;

		private string _activeConversationNode;
		private CharacterBase _targetNPC;

		private readonly GameSystem _game;

		public ConversationManager(GameSystem gameSystem)
		{
			_game = gameSystem;
		}

		internal void SubscribeToEvents()
		{
			_game.UI.ConversationUI.TriggeredPlayerChoice += TriggerPlayerChoice;
		}

		internal void DisplayConversation(string conversationID, CharacterBase targetNPC = null)
		{
			if (conversationID == "end" || conversationID == string.Empty)
			{
				CloseConversation();
				return;
			}

			if (_game.UI.ActiveUIState != UIState.Conversation)
			{
				_game.UI.SetUIState(UIState.Conversation);
			}

			_activeConversationNode = conversationID;

			string speaker = string.Empty;
			string content = string.Empty;
			string[] playerChoices = new string[0];

			if (conversationID.Trim() != string.Empty)
			{
				if (targetNPC != null)
				{
					_targetNPC = targetNPC;
					speaker = targetNPC.Name;
				}

				string conversationContentID = _game.GameDatabase.ConversationNodes[_activeConversationNode].GenericTextID != string.Empty ? _game.GameDatabase.ConversationNodes[_activeConversationNode].GenericTextID : _activeConversationNode;
				content = Godot.TranslationServer.Translate(conversationContentID.ToUpper());

				playerChoices = new string[_game.GameDatabase.ConversationNodes[_activeConversationNode].PlayerReplyIDs.Length];

				for (int index = 0; index < playerChoices.Length; index++)
				{
					string choiceNodeID = _game.GameDatabase.ConversationNodes[_activeConversationNode].PlayerReplyIDs[index];

					if (ParseChoiceRequirements(_game.GameDatabase.ConversationNodes[choiceNodeID].Requirements))
					{
						if (_game.GameDatabase.ConversationNodes[choiceNodeID].GenericTextID != string.Empty)
						{
							choiceNodeID = _game.GameDatabase.ConversationNodes[choiceNodeID].GenericTextID;
						}

						playerChoices[index] = Godot.TranslationServer.Translate(choiceNodeID.ToUpper());
					}
					else
					{
						playerChoices[index] = string.Empty;
					}
				}
			}

			DisplayConversationNode?.Invoke(speaker, content, playerChoices);
		}

		internal void CloseConversation()
		{
			if (_targetNPC != null)
			{
				((NPCMovement)_targetNPC.BaseMovement).RestorePreviousState();
				_targetNPC = null;
			}

			_game.UI.SetUIState(UIState.None);
			DisplayConversationNode?.Invoke("end", string.Empty, new string[0]);
		}

		private void TriggerPlayerChoice(float index)
		{
			if (index < 0)
			{
				AdvanceConversation();
				return;
			}

			if (ParseChoiceConsequence(_game.GameDatabase.ConversationNodes[_game.GameDatabase.ConversationNodes[_activeConversationNode].PlayerReplyIDs[(int)index]].Effects))
			{
				string nodeToDisplay = _game.GameDatabase.ConversationNodes[_activeConversationNode].PlayerReplyIDs[(int)index];
				DisplayConversation(_game.GameDatabase.ConversationNodes[nodeToDisplay].NextNodeID, _targetNPC);
			}
		}

		private bool ParseChoiceRequirements(string[] requirements)
		{
			foreach (string requirement in requirements)
			{
				if (_game.ParseChoiceRequirements(requirement, ParseActionTarget(requirement.Split(':'))) == false)
				{
					return false;
				}
			}

			return true;
		}

		private bool ParseChoiceConsequence(string[] choiceEffects)
		{
			bool continueConversation = true;

			foreach (string action in choiceEffects)
			{
				if (_game.ParseTriggeredAction(action, ParseActionTarget(action.Split(':'))) == false)
				{
					continueConversation = false;
				}
			}

			return continueConversation;
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

		private void AdvanceConversation()
		{
			DisplayConversation(_game.GameDatabase.ConversationNodes[_activeConversationNode].NextNodeID);
		}
	}
}