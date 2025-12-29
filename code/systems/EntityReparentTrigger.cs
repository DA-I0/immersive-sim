using Godot;
using ImmersiveSim.Gameplay;

namespace ImmersiveSim.Systems
{
	public partial class EntityReparentTrigger : Area3D
	{
		[Export] private bool _topSceneParent;
		private LevelManager _levelManager;
		private System.Collections.Generic.List<Node3D> _npcsInside = new System.Collections.Generic.List<Node3D>();

		private Node TargetParent
		{
			get { return _topSceneParent ? _levelManager.ActiveScene : GetParent().GetParent(); }
		}

		public override void _Ready()
		{
			BodyEntered += CheckEnteringBody;
			BodyExited += CheckExitingBody;
			TreeExiting += DestroyNPCsInside;
			_levelManager = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString()).Level;
		}

		private void CheckEnteringBody(Node3D body)
		{
			if (body is NPCMovement)
			{
				_npcsInside.Add(body);
				return;
			}

			if (body is Interfaces.IReparentableEntity reparentableBody)
			{
				if (body.GetParent() == TargetParent || body.GetPath().ToString().Contains(TargetParent.GetPath()) || !body.GetPath().ToString().Contains(_levelManager.ActiveScenePath))
				{
					return;
				}

				reparentableBody.ReparentToSubscene(TargetParent);
				GD.Print($"reparenting entity: {body.Name} to sublevel");
			}
		}

		private void CheckExitingBody(Node3D body)
		{
			if (body is NPCMovement)
			{
				_npcsInside.Remove(body);
				return;
			}

			if (body is Interfaces.IReparentableEntity reparentableBody)
			{
				reparentableBody.TriggerParentReset();
				GD.Print($"reparenting entity: {body.Name} to the main active level scene");
			}
		}

		private void DestroyNPCsInside()
		{
			foreach (Node3D npc in _npcsInside)
			{
				npc.GetNode<CharacterBase>("CharacterBase").Destroy();
			}
		}
	}
}