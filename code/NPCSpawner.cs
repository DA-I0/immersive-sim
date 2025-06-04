using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class NPCSpawner : Area3D
	{
		[Export] private string _npcPresetID;

		private bool _isActive;
		protected bool _isOccupied;

		private System.Collections.Generic.List<CharacterBody3D> _charactersOnPoint = new();

		public string NPCPresetID
		{
			get { return _npcPresetID; }
		}

		public bool IsAvailable
		{
			get { return _isActive && _charactersOnPoint.Count < 1; }
		}

		public bool IsActive
		{
			set { _isActive = value; }
		}

		public override void _Ready()
		{
			BodyEntered += CharacterEntered;
			BodyExited += CharacterExited;
			TreeExiting += RemoveFromSpawnerList;
			GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).NPCSpawnController.AddNPCSpawner(this);
		}

		public void Enable()
		{
			_isActive = true;
		}

		public void Disable()
		{
			_isActive = false;
		}

		protected virtual void CharacterEntered(Node3D target)
		{
			if (target is CharacterBody3D character)
			{
				_charactersOnPoint.Add(character);
				UpdateOccupiedVisual();
			}
		}

		private void CharacterExited(Node3D target)
		{
			if (target is CharacterBody3D character)
			{
				_charactersOnPoint.Remove(character);
				UpdateOccupiedVisual();
			}
		}

		private void RemoveFromSpawnerList()
		{
			BodyEntered -= CharacterEntered;
			BodyExited -= CharacterExited;
			TreeExiting -= RemoveFromSpawnerList;
			GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).NPCSpawnController.RemoveNPCSpawner(this);
		}

		private void UpdateOccupiedVisual()
		{
			GetNode<MeshInstance3D>("TriggerMarker").Visible = IsAvailable;
		}
	}
}