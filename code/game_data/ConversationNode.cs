namespace ImmersiveSim.GameData
{
	public struct ConversationNode
	{
		public readonly string ID;
		public readonly string GenericTextID;
		public readonly string[] Requirements; // conditions for displaying this dialog node, kept as a string to parse - "strength>2" or something like that
		public readonly string[] Effects; // changes caused by getting to this dialog node (setting a variable, starting a quest etc), kept as a string to parse - "var;var_name;operation;value" (var;quest2;set;3) or something like that
		public readonly string NextNodeID;
		public readonly string[] PlayerReplyIDs;

		public ConversationNode(string id, string genericTextID, string[] requirements, string[] effects, string nextNodeID, string[] playerReplyIDs)
		{
			ID = id;
			GenericTextID = genericTextID;
			Requirements = requirements;
			Effects = effects;
			NextNodeID = nextNodeID;
			PlayerReplyIDs = playerReplyIDs;
		}
	}
}