using System.Collections.Generic;
using System.Linq;
using Godot;
using ImmersiveSim.Interfaces;
using ImmersiveSim.Statics;

namespace ImmersiveSim.Gameplay
{
	public partial class NPCActionControler : Node
	{
		[Export] private NPCActivityType _activityType;
		private NPCActivityType _lastActivityType;
		[Export] private bool isStatic = false; // temp
		private int _currentActivity = -1; // temp?
		private INPCUsable _interactionTarget;

		private bool _interactionStarted = false;
		private bool _subscribedToInteractionDelegate = false;

		private List<INPCUsable> _usablesInRange = new List<INPCUsable>();

		private Timer _interactionTimer;
		private NPCBase _character;

		internal NPCActivityType ActivityType
		{
			get { return _activityType; }
		}

		public INPCUsable CurrentInteractionTarget
		{
			get { return _interactionTarget; }
		}

		public override void _Ready()
		{
			base._Ready();
			_character = GetNode<NPCBase>("../CharacterBase");
			_interactionTimer = GetNode<Timer>("Timer");

			_character.CharMovement.TargetReached += InteractWithTarget;
			_character.SetForDestruction += UnsubscribeFromEvents;
		}

		public bool CheckIfInteractionIsPossible()
		{
			if (_interactionTarget == null || (_interactionTarget.ActiveUser != null && _interactionTarget.ActiveUser != _character))
			{
				InteractionComplete();
				return false;
			}

			return true;
		}

		public void ForceInteractionTarget(NPCInteractionTarget newTarget, bool immediate = false)
		{
			SetInteractionTarget(newTarget);

			if (immediate)
			{
				InteractWithTarget();
			}
		}

		private void FindInteractable()
		{
			if (_activityType == NPCActivityType.Static)
			{
				return;
			}

			if (_usablesInRange.Count < 1)
			{
				GD.Print("No NPC usables in range!");
				return;
			}

			IEnumerable<INPCUsable> filteredUsables = null;

			// scan the environment to do stuff
			switch (_currentActivity)
			{
				case 0:
					filteredUsables = _usablesInRange.Where(t => t.InteractionType == NPCInteractionType.Feeder);
					break;

				case 1:
					filteredUsables = _usablesInRange.Where(t => t.InteractionType == NPCInteractionType.Rest);
					break;

				case 2:
					filteredUsables = _usablesInRange.Where(t => t.InteractionType == NPCInteractionType.Work);
					break;

				default:
					filteredUsables = _usablesInRange;
					break;
			}

			int randomIndex = GD.RandRange(0, filteredUsables.Count() - 1);

			if (randomIndex < 0)
			{
				_interactionTimer.Start();
				return;
			}

			SetInteractionTarget(filteredUsables.ElementAtOrDefault(randomIndex));
		}

		private void SetInteractionTarget(INPCUsable newTarget)
		{
			_interactionTarget = newTarget;

			if (_interactionTarget != null)
			{
				_character.CharMovement.SetupTargetDestination(_interactionTarget.InteractionPointPosition.GlobalPosition);
			}
		}

		private void SelectNewActivity()
		{
			if (_interactionTarget != null || _activityType == NPCActivityType.Static)
			{
				return;
			}

			if (_activityType == NPCActivityType.Simulation)
			{
				SelectSimulationActivity();
				return;
			}

			if (_activityType == NPCActivityType.Schedule)
			{
				SelectScheduleActivity();
				return;
			}

			_currentActivity = -1; // laze around
			FindInteractable();
		}

		private void SelectSimulationActivity()
		{
			if (_character.CharStatus.CurrentMaxStamina <= _character.CharStatus.MaxStamina * 0.5f)
			{
				_currentActivity = 1; // eat go to sleep
				FindInteractable();
				return;
			}

			if (_character.CharStatus.Stamina < _character.CharStatus.CurrentMaxStamina * 0.5f)
			{
				_currentActivity = 0; // eat food
				FindInteractable();
				return;
			}

			_currentActivity = 2; // temp? decide what causes triggering work search
			FindInteractable();
		}

		private void SelectScheduleActivity()
		{
			// TODO: handle schedule actions
		}

		private void InteractWithTarget()
		{
			if (_interactionTarget != null && !_interactionStarted)
			{
				if (CheckIfInteractionIsPossible())
				{
					_lastActivityType = _activityType;
					_activityType = NPCActivityType.Static;
					_interactionTarget.InteractionComplete += InteractionComplete;
					_subscribedToInteractionDelegate = true;
					_interactionTarget.NPCInteract(_character);
					_interactionStarted = true;
				}
			}
		}

		private void InteractionComplete()
		{
			_character.CharMovement.SetupTargetDestination(_character.CharMovement.GlobalPosition);
			_activityType = _lastActivityType;

			if (_interactionTarget != null)
			{
				_interactionTarget.InteractionComplete -= InteractionComplete;
				_subscribedToInteractionDelegate = false;
				_interactionTarget = null;
			}

			_interactionStarted = false;
			_interactionTimer.Start();
		}

		private void AddUsableToList(NPCInteractionTarget entity)
		{
			if (entity is INPCUsable usable)
			{
				_usablesInRange.Add(usable);
			}
		}

		private void RemoveUsableFromList(NPCInteractionTarget entity)
		{
			if (entity is INPCUsable usable)
			{
				_usablesInRange.Remove(usable);
			}
		}

		private void ClearAsInteractionTarget()
		{
			if (_interactionTarget != null)
			{
				_interactionTarget.ClearUser(_character.BaseMovement);
			}
		}

		private void UnsubscribeFromEvents()
		{
			_character.CharMovement.TargetReached -= InteractWithTarget;
			_character.SetForDestruction -= UnsubscribeFromEvents;

			if (_subscribedToInteractionDelegate)
			{
				_interactionTarget.InteractionComplete -= InteractionComplete;
			}
		}
	}
}