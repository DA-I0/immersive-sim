using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class ConversationUI : Control
	{
		public event Statics.ValueChanged TriggeredPlayerChoice;

		private Control _conversationWindow;
		private Control _conversationChoiceWindow;
		private Control _conversationChoiceList;
		private RichTextLabel _conversationContent;
		private Button _continueButton;

		private string _activeConversationNode;
		private NPCMovement _targetNPC;

		public override void _Ready()
		{
			_conversationWindow = GetNode<Control>("NPCBox");
			_conversationChoiceWindow = GetNode<Control>("PlayerBox");
			_conversationChoiceList = GetNode<Control>("PlayerBox/ConversationChoices/Choices");
			_conversationContent = _conversationWindow.GetNode<RichTextLabel>("ConversationDisplay/ConversationContent");
			_continueButton = _conversationChoiceWindow.GetChild(0).GetChild(0).GetChild<Button>(0);
			_continueButton.Pressed += () => TriggerPlayerChoice(-1);

			GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).Conversation.DisplayConversationNode += DisplayConversation;
			ToggleVisibility(false);
		}

		public void DisplayConversation(string speaker, string content, string[] playerChoices)
		{
			if (speaker == "end")
			{
				CloseConversation();
				return;
			}

			_conversationContent.Text = content;
			ToggleVisibility(true);
			ClearConversationChoices();
			DisplayConversationChoices(playerChoices);
		}

		public void CloseConversation()
		{
			ToggleVisibility(false);
		}

		private void ToggleVisibility(bool setActive)
		{
			Visible = setActive;
		}

		private void DisplayConversationChoices(string[] choices)
		{
			_continueButton.Visible = (choices == null || choices.Length < 1);

			for (int index = 0; index < choices.Length; index++)
			{
				Button choiceButton = CreateConversationChoiceButton(choices[index], index);

				if (choiceButton != null)
				{
					_conversationChoiceList.AddChild(choiceButton);
				}
			}
		}

		private void ClearConversationChoices()
		{
			for (int index = 1; index < _conversationChoiceList.GetChildCount(); index++)
			{
				Button conversationChoice = (Button)_conversationChoiceList.GetChild(index);
				conversationChoice.QueueFree();
			}
		}

		private Button CreateConversationChoiceButton(string choiceContent, int index)
		{
			if (choiceContent == string.Empty)
			{
				return null;
			}

			Button newButton = new Button
			{
				Text = choiceContent,
				SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand
			};

			newButton.Pressed += () => TriggerPlayerChoice(index);
			return newButton;
		}

		private void TriggerPlayerChoice(int index)
		{
			TriggeredPlayerChoice?.Invoke(index);
		}
	}
}