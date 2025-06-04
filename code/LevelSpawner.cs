using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class LevelSpawner : Area3D
	{
		[Export] private string _levelID;
		private bool _isActive;

		private Node3D _unloadedDetails;
		private Systems.LevelManager _level;

		public override void _Ready()
		{
			_unloadedDetails = GetNodeOrNull<Node3D>("../UnloadedDetails");
			_level = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).Level;
		}

		public void Enable()
		{
			_level.LoadSubLevel(_levelID, GetParent<Node3D>());
			ToggleUnloadedDetails(false);
		}

		public void Disable()
		{
			_level.RemoveSubLevel(_levelID.Split('/')[^1], GetParent<Node3D>());
			ToggleUnloadedDetails(true);
		}

		private void ToggleUnloadedDetails(bool enable)
		{
			if (_unloadedDetails != null)
			{
				_unloadedDetails.Visible = enable;
			}
		}
	}
}