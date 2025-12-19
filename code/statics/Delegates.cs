namespace ImmersiveSim.Statics
{
	public delegate void DateChange(System.DateTime newDate);
	public delegate void DoorStatusHandler(int openProgress, int lockStatus);
	public delegate void StatusUpdate();
	public delegate void StatusValueUpdate(string value);
	public delegate void ValueChanged(float value);
	public delegate void UIStateUpdated(UIState currentState);
	public delegate void InteractableTargeted(object interactable);
	public delegate void ConversationNodeData(string source, string content, string[] playerChoices);
}