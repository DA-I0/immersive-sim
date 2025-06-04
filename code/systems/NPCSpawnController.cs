using System.Collections.Generic;
using System.Linq;
using Godot;
using ImmersiveSim.Gameplay;

namespace ImmersiveSim.Systems
{
	public class NPCSpawnController
	{
		private List<NPCSpawner> _npcSpawners = new List<NPCSpawner>();
		// private List<NPCBase> _npcCache = new List<NPCBase>();
		private List<CharacterBody3D> _npcCache = new List<CharacterBody3D>();

		private GameSystem _game;

		public NPCSpawnController(GameSystem game)
		{
			_game = game;
		}

		public void AddNPCSpawner(NPCSpawner target)
		{
			if (!_npcSpawners.Contains(target))
			{
				_npcSpawners.Add(target);
			}
		}

		public void RemoveNPCSpawner(NPCSpawner target)
		{
			if (_npcSpawners.Contains(target))
			{
				_npcSpawners.Remove(target);
			}
		}

		public async void SpawnRandomNPC()
		{
			ClearOrphanedCacheEntries();
			GD.Print($"current NPC count: {_npcCache.Count} -- max NPC count: {Statics.StaticValues.NPCLimit}");

			if (_npcCache.Count >= Statics.StaticValues.NPCLimit || _game.Level.IsInMenu)
			{
				return;
			}

			await _game.ToSignal(_game.GetTree(), SceneTree.SignalName.PhysicsFrame);

			// select random spawn point that's Active and Not In Sight
			// Trigger NPC spawn - select the spawn node, get its presetID, handle the instantiation and reparenting here, change position to the selected spawn position
			// check existing NPC cache for disabled NPC of the selected type, reenable if there's a suitable target, spawn new if not
			NPCSpawner targetSpawner = SelectRandomSpawnPoint();

			if (targetSpawner != null)
			{
				CharacterBody3D newNPC = InstantiateNPC(RandomizeNPCTemplate(targetSpawner));
				targetSpawner.GetParent().GetParent().AddChild(newNPC);
				AddNPCToCache(newNPC);
				newNPC.GlobalPosition = targetSpawner.GlobalPosition;
			}
		}

		public void AddNPCToCache(Node target)
		{
			if (target is NPCMovement npc)
			{
				_npcCache.Add(npc);
				npc.GetNode<NPCBase>("CharacterBase").StopDestructionSequence();
			}
		}

		public void RemoveNPCFromCache(Node target) // There are issues with clearing NPCs from the list
		{
			// when player moves too far from an NPC, remove the NPC
			// disable if cache has free space, destroy the target if max limit reached
			if (target is NPCMovement npc)
			{
				GD.Print($">> triggering removal of NPC: {target.Name}");
				// _npcCache.Remove(npc);
				npc.GetNode<NPCBase>("CharacterBase").StartDestructionSequence();
			}
		}

		public void RemoveNPCOnDestruction(NPCMovement target)
		{
			GD.Print($">> removing NPC during destruction: {target.Name}");
			// if (target.BaseMovement is NPCMovement npc)
			// {
			_npcCache.Remove(target);
			// }
		}

		private NPCSpawner SelectRandomSpawnPoint()
		{
			if (_npcSpawners.Count < 1)
			{
				return null;
			}

			List<NPCSpawner> filteredList = _npcSpawners.Where(spawner => spawner.IsAvailable).ToList();
			int randomIndex = GD.RandRange(0, filteredList.Count - 1);

			return (randomIndex >= 0 && randomIndex < filteredList.Count) ? filteredList.ElementAt(randomIndex) : null;
		}

		private string RandomizeNPCTemplate(NPCSpawner targetSpawner)
		{
			string[] availableTemplates = _game.GameDatabase.NpcSpawnPresets[targetSpawner.NPCPresetID];
			int randomIndex = GD.RandRange(0, availableTemplates.Length - 1);
			return availableTemplates[randomIndex];
		}

		private CharacterBody3D InstantiateNPC(string templateName)
		{
			string prefabPath = $"{ProjectSettings.GetSetting("global/NPCPrefabFolder")}/{templateName}.tscn".ToLower();
			ResourceLoader.LoadThreadedRequest(prefabPath);
			return (CharacterBody3D)(ResourceLoader.LoadThreadedGet(prefabPath) as PackedScene).Instantiate();
		}

		private void ClearOrphanedCacheEntries()
		{
			List<CharacterBody3D> emptyEntries = _npcCache.FindAll(npc => npc == null);

			foreach (CharacterBody3D entry in emptyEntries)
			{
				_npcCache.Remove(entry);
			}
		}
	}
}