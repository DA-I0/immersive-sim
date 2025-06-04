using System;
using Godot;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay
{
	public partial class NPCInteractionTarget : NPCSpawner, Interfaces.INPCUsable
	{
		public event StatusUpdate InteractionComplete;

		[Export] private NPCInteractionType _interactionType;
		[Export] private NPCInteractionDuration _interactionDuration;
		[Export] private float _duration; // temp, complete the interaction after finishing an animation or after x amound of in-game time in the future
		[Export] private string _conditionName;
		[Export] private Node3D _interactionPosition;

		[ExportCategory("Status changes")]
		[Export] private int _healthChange = 0;
		[Export] private int _staminaChange = 0;
		[Export] private int _maxStaminaChange = 0;

		private Timer _interactionTimer;
		private DateTime _interactionStart;

		private NPCBase _user;

		private GameSystem _game;

		public /*Vector3*/Node3D InteractionPointPosition
		{
			get { return _interactionPosition != null ? _interactionPosition : this; }
		}

		public NPCInteractionType InteractionType
		{
			get { return _interactionType; }
		}

		public int HealthChange
		{
			get { return _healthChange; }
		}

		public int StaminaChange
		{
			get { return _staminaChange; }
		}

		public NPCBase ActiveUser
		{
			get { return _user; }
		}

		public override void _Ready()
		{
			_interactionTimer = new Timer();
			_interactionTimer.Timeout += CompleteNPCInteraction;
			AddChild(_interactionTimer);

			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());

			base._Ready();
		}

		public void NPCInteract(NPCBase user)
		{
			_user = user;
			user.CharMovement.AdjustToTargetPosition(InteractionPointPosition);
			_interactionStart = _game.Time.CurrentDate;

			if (_interactionDuration == NPCInteractionDuration.ActionBased)
			{
				if (_conditionName != string.Empty)
				{
					_user.CharMovement.TriggerAction(_conditionName);
				}

				float randomisedDuration = (float)GD.RandRange(_duration * 0.5f, _duration);
				_interactionTimer.Start(randomisedDuration);
			}
		}

		protected override void CharacterEntered(Node3D target)
		{
			base.CharacterEntered(target);

			if (target is NPCMovement)
			{
				NPCBase targetNPC = target.GetNodeOrNull<NPCBase>("CharacterBase");

				if (targetNPC != null && targetNPC.CharActions.CurrentInteractionTarget == null)
				{
					targetNPC.CharActions.ForceInteractionTarget(this, true);
				}
			}
		}

		private void ApplyStatusChanges(NPCBase user)
		{
			if (user == null)
			{
				return;
			}

			user.CharStatus.ChangeHealth(_healthChange);
			user.CharStatus.ChangeStamina(_staminaChange);

			if (_maxStaminaChange != 0)
			{
				DateTime newRestTime = _interactionStart.AddHours(_maxStaminaChange);
				user.CharStatus.Rest(newRestTime);
			}
			else
			{
				user.CharStatus.Rest(_game.Time.CurrentDate);
			}
		}

		private void CompleteNPCInteraction()
		{
			if (_user == null)
			{
				return;
			}

			if (_interactionDuration == NPCInteractionDuration.TimeBased)
			{
				if (_game.Time.CurrentDate < _interactionStart.AddHours(_duration))
				{
					_interactionTimer.Start(_duration);
					return;
				}
			}

			_user.CharMovement.TriggerAction("Idle");

			if (_interactionPosition != null)
			{
				_user.CharMovement.AdjustToTargetPosition(this);
			}
			ApplyStatusChanges(_user);
			InteractionComplete?.Invoke();
		}

		public void ClearUser(Node3D user)
		{
			if (user != null && _user != null && _user.CharMovement == user)
			{
				_user = null;
			}
		}
	}
}