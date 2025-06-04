using ImmersiveSim.Statics;

namespace ImmersiveSim.GameData
{
	public class Quest
	{
		private string _id;
		private QuestState _state;
		private bool _failed = false; // use this instead of a QuestState?
		private Objective[] _objectives;

		public void SetState(QuestState newState)
		{

		}

		public QuestState GetState()
		{
			// if (_failed)
			// {
			// 	return _failed;
			// }
			// else
			// {
			return _state;
			// }
		}

		public void SetFailedState(bool isTrue)
		{
			// use only with the "_failed" flag, can't be used if quest is complete 
		}
	}
}