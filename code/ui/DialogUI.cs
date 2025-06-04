using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Systems;

namespace ImmersiveSim.UI
{
	public partial class DialogUI : Control
	{
		public event Statics.ValueChanged TriggeredPlayerChoice;

		private Control _dialogWindow;
		private Control _dialogChoiceWindow;
		private Control _dialogChoiceList;
		private RichTextLabel _dialogContent;
		private Button _continueButton;

		private string _activeDialogNode;
		private NPCMovement _targetNPC;

		public override void _Ready()
		{
			_dialogWindow = GetNode<Control>("NPCBox");
			_dialogChoiceWindow = GetNode<Control>("PlayerBox");
			_dialogChoiceList = GetNode<Control>("PlayerBox/DialogChoices/Choices");
			_dialogContent = _dialogWindow.GetNode<RichTextLabel>("DialogDisplay/DialogContent");
			_continueButton = _dialogChoiceWindow.GetChild(0).GetChild(0).GetChild<Button>(0);

			GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).Dialog.DisplayDialogNode += DisplayDialog;
			ToggleVisibility(false);
		}

		public void DisplayDialog(string speaker, string content, string[] playerChoices)
		{
			if (speaker == "end")
			{
				CloseDialog();
				return;
			}

			_dialogContent.Text = content;
			ToggleVisibility(true);
			ClearDialogChoices();
			DisplayDialogChoices(playerChoices);
		}

		public void CloseDialog()
		{
			ToggleVisibility(false);
		}

		private void ToggleVisibility(bool setActive)
		{
			Visible = setActive;
		}

		private void DisplayDialogChoices(string[] choices)
		{
			_continueButton.Visible = (choices == null || choices.Length < 1);

			for (int index = 0; index < choices.Length; index++)
			{
				Button choiceButton = CreateDialogChoiceButton(choices[index], index);

				if (choiceButton != null)
				{
					_dialogChoiceList.AddChild(choiceButton);
				}
			}
		}

		private void ClearDialogChoices()
		{
			for (int index = 1; index < _dialogChoiceList.GetChildCount(); index++)
			{
				Button dialogChoice = (Button)_dialogChoiceList.GetChild(index);
				dialogChoice.QueueFree();
			}
		}

		private Button CreateDialogChoiceButton(string choiceContent, int index)
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