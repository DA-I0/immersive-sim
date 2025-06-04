using System;
using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class StreamingTrigger : Area3D
	{
		private Systems.GameSystem _game;

		public override void _Ready()
		{
			AreaEntered += AreaEnteredRange;
			AreaExited += AreaExitedRange;
			BodyEntered += BodyEnteredRange;
			BodyExited += BodyExitedRange;
			TreeExiting += UnsubscribeFromEvents;

			_game = GetNode<Systems.GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.Settings.SettingsUpdated += UpdateStreamingRange;
			UpdateStreamingRange();
		}

		private void UpdateStreamingRange()
		{
			((SphereShape3D)GetChild<CollisionShape3D>(0).Shape).Radius = _game.Settings.StreamingDistance;
		}

		private void AreaEnteredRange(Area3D target)
		{
			if (target is NPCSpawner npcSpawner)
			{
				npcSpawner.IsActive = true;
				return;
			}

			if (target is LevelSpawner levelSpawner)
			{
				levelSpawner.Enable();
				return;
			}
		}

		private void AreaExitedRange(Area3D target)
		{
			if (target is NPCSpawner npcSpawner)
			{
				npcSpawner.IsActive = false;
				return;
			}

			if (target is LevelSpawner levelSpawner)
			{
				levelSpawner.Disable();
				return;
			}
		}

		private void BodyEnteredRange(Node3D target)
		{
			if (target is CharacterBody3D npc)
			{
				_game.NPCSpawnController.AddNPCToCache(npc);
			}
		}

		private void BodyExitedRange(Node3D target)
		{
			if (target is CharacterBody3D npc)
			{
				_game.NPCSpawnController.RemoveNPCFromCache(npc);
			}
		}

		private void UnsubscribeFromEvents()
		{
			AreaEntered -= AreaEnteredRange;
			AreaExited -= AreaExitedRange;
			BodyEntered -= BodyEnteredRange;
			BodyExited -= BodyExitedRange;
			TreeExiting -= UnsubscribeFromEvents;
		}
	}
}