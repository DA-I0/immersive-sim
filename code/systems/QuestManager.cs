namespace ImmersiveSim.Systems
{
	public class QuestManager
	{
		private GameSystem _game;

		public QuestManager(GameSystem gameSystem)
		{
			_game = gameSystem;
		}

		public void LoadQuestData(string questID)
		{
			// params and body TDB
		}

		public void SaveQuestData(string questID)
		{
			// params and body TDB
		}

		public void ChangeQuestState(string questID, string operation, string newState)
		{
			// find targeted quest

			switch (operation)
			{
				case "set":
					// set quest state
					// ParseQuestState(newState)
					break;

				case "setObjective":
					// set objective state
					break;

				case "changeObjective":
					// change objective state by newState?
					break;

				default:
					return;
			}
		}

		private Statics.QuestState ParseQuestState(string newState)
		{
			_ = int.TryParse(newState, out int parsedState);
			Statics.QuestState variable = (Statics.QuestState)parsedState;

			return variable;//Statics.QuestState.Unknown;
		}
	}
}