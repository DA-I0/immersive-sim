using System;
using Godot;
using ImmersiveSim.GameData;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay
{
	public partial class Status : Node
	{
		public event ValueChanged HealthChanged;
		public event ValueChanged StaminaChanged;
		public event ValueChanged StaminaPenaltyChanged;

		[ExportGroup("Health")]
		[Export] private bool _invulnerable;
		private float _currentMaxHealth;
		private float _currentHealth;

		[ExportGroup("Stamina")]
		private float _currentMaxStamina;
		private float _currentStamina;
		private float _staminaPenalty = 0;
		private float _staminaRegen = 0.1f;
		private float _staminaModifier = 1f;
		private float _lastStaminaReduction = -1f;
		private DateTime _lastRestDate;

		// add variables for various damage resistances, updated on status and equipment changes
		private float _bladeResistance = 0;
		private float _bluntResistance = 0;
		private float _explosionResistance = 0;
		private float _fallResistance = 0;
		private float _fireResistance = 0;
		private float _gunResistance = 0;

		private CharacterBase _character;
		private GameSystem _game;

		public int MaxHealth
		{
			get { return (int)(StaticValues.BaseMaxHealth * _character.CharSheet.BMIRatio); }
		}

		public float Health
		{
			get { return _currentHealth; }
		}

		public int MaxStamina
		{
			get { return (int)(StaticValues.BaseMaxStamina * _character.CharSheet.BMIInversedRatio); }
		}

		public float CurrentMaxStamina
		{
			get { return _currentMaxStamina; }
		}

		public float Stamina
		{
			get { return _currentStamina; }
		}

		public DateTime LastRestDate
		{
			get { return _lastRestDate; }
		}

		public override void _Ready()
		{
			_character = GetNode<CharacterBase>("../CharacterBase");
			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
			_game.Time.DateUpdated += ApplyRestChanges;
			CallDeferred("InitializeHealth");
			CallDeferred("InitializeStamina");
		}

		public void SetMaxHealth(float value)
		{
			_currentMaxHealth = Math.Clamp(value, 1, MaxHealth);
			_character.IsModified = true;
		}

		public void SetHealth(float value)
		{
			_currentHealth = value;
			_currentHealth = Mathf.Clamp(_currentHealth, 0, _currentMaxHealth);
			HealthChanged?.Invoke(_currentHealth);
			_character.IsModified = true;
		}

		private void UpdateActiveMaxHealth()
		{
			// go through all the status effects and modify the variable value based on that
			_currentMaxHealth = MaxHealth;
			_character.IsModified = true;
		}

		public void ChangeHealth(float value)
		{
			_currentHealth += HelperMethods.RoundFloat(value);

			if (_currentHealth > MaxHealth)
			{
				_currentHealth = MaxHealth;
			}

			if (_currentHealth <= 0)
			{
				// if (gameObject.tag != "Player")
				// 	Die();
				// else
				// 	PassOut();
				GD.Print($"Movement:: object: {GetParent().Name} is dead");
			}

			_character.IsModified = true;
			HealthChanged?.Invoke(_currentHealth);
		}

		public void TakeDamage(float value, DamageType type) // change into "ChangeHealth" and merge with Heal?
		{
			if (!_invulnerable)
			{
				// if (GetParent().IsInGroup("Player"))
				// {
				ChangeHealth(-1 * ReduceDamage(value, type));// * refs.diff.damageToPlayer);
				return;
				// }

				// if (GetParent().IsInGroup("NPC"))
				// {
				// 	HealDamage(-1 * ReduceDamage(value, type));// * refs.diff.damageToPlayer);
				// return;
				// }

				// HealDamage(-1 * ReduceDamage(value, type));
			}
		}

		public float ReduceDamage(float value, DamageType type)
		{
			float reducedAmount;

			switch (type)
			{
				case DamageType.Blade:
					reducedAmount = value - value * _bladeResistance;
					break;

				case DamageType.Blunt:
					reducedAmount = value - value * _bluntResistance;
					break;

				case DamageType.Explosion:
					reducedAmount = value - value * _explosionResistance;
					break;

				case DamageType.Collision:
					reducedAmount = value - value * _fallResistance;
					break;

				case DamageType.Fire:
					reducedAmount = value - value * _fireResistance;
					break;

				case DamageType.Gun:
					reducedAmount = value - value * _gunResistance;
					break;

				default:
					reducedAmount = value;
					break;
			}

			if (reducedAmount < 0)
			{
				return 0;
			}
			else
			{
				return reducedAmount;
			}
		}

		public void Die()
		{
			// Destroy(gameObject);
		}

		public void PassOut() // make it into a status effect?
		{
			// SetHealth((float)((_maxHealth * 15) / 100));
		}


		public void ChangeStamina(float value)
		{
			UpdateStaminaModifier();
			float updatedValue = Mathf.Clamp(_currentStamina + value, 0, _currentMaxStamina);
			SetStamina(updatedValue);
		}

		public void ReduceStamina(float value)
		{
			ChangeStamina(value * _staminaModifier);
			_lastStaminaReduction = Time.GetTicksMsec();
		}

		public void SetStamina(float value)
		{
			_currentStamina = value;

			if (_currentStamina > _currentMaxStamina)
			{
				SetMaxStamina(_currentStamina);
			}

			CheckStaminaLimits();
			StaminaChanged?.Invoke(_currentStamina);
		}

		public void SetCurrMaxStamina(float value)
		{
			_currentMaxStamina = (value > 0) ? value : MaxStamina;
			CheckStaminaLimits();
			StaminaChanged?.Invoke(_currentStamina);
			StaminaPenaltyChanged?.Invoke(MaxStamina - _currentMaxStamina);
		}

		public void AddMaxStamina(float value)
		{
			SetMaxStamina(_currentMaxStamina + value);
		}

		public void SetMaxStamina(float value)
		{
			_currentMaxStamina = value;
			CheckStaminaLimits();
		}

		public void ChangeStaminaPenalty(float value)
		{
			_staminaPenalty += value;
			_staminaPenalty = Mathf.Clamp(_staminaPenalty, 0, MaxStamina * 0.3f);
		}

		public void SetStaminaPenalty(float value)
		{
			_staminaPenalty = Mathf.Clamp(value, 0, MaxStamina * 0.3f);
		}

		public void Rest(DateTime wakeUpDate)
		{
			_lastRestDate = wakeUpDate;
			_character.IsModified = true;
		}

		private void InitializeHealth()
		{
			SetMaxHealth(MaxHealth);
			SetHealth(_currentMaxHealth);
		}

		private void InitializeStamina()
		{
			SetMaxStamina(MaxStamina);
			SetStamina(_currentMaxStamina);
			_lastRestDate = _game.Time.CurrentDate;
		}

		private void RegenStamina()
		{
			if (_lastStaminaReduction + StaticValues.RegenDelay > Time.GetTicksMsec())
			{
				return;
			}

			ChangeStamina(_staminaRegen);
			_lastStaminaReduction = -1f;
		}

		private void UpdateActiveMaxStamina()
		{
			// go through all the status effects and modify the variable value based on that
			_currentMaxStamina = MaxStamina;
		}

		private void CheckStaminaLimits()
		{
			_currentMaxStamina = Mathf.Clamp(_currentMaxStamina, MaxStamina * 0.5f, MaxStamina);
			_currentStamina = Mathf.Clamp(_currentStamina, 0, _currentMaxStamina);
			_staminaRegen = Mathf.Clamp(_staminaRegen, 0.5f, 1f);
			_character.IsModified = true;
		}

		private void UpdateStaminaModifier()
		{
			_staminaModifier = 0.01f;
			// staminaModifier += _character.CharInventory.Weight / 100;
			_staminaModifier += (_character.CharInventory.CarryWeightRatio > 1) ? (_character.CharInventory.CarryWeightRatio - 1) / 10 : 0;

			switch (_character.BaseMovement.ActiveStance)
			{
				case Stance.Prone:
					_staminaModifier *= 0.35f;
					break;

				case Stance.Crouch:
					_staminaModifier *= 1.25f;
					break;

				// case Stance.Hanging:
				// 	staminaMod *= 5f;
				// 	break;

				// case Stance.Climbing:
				// 	staminaMod *= 10f;
				// 	break;

				// case Stance.Rest:
				// 	staminaMod *= 0.1f;
				// 	break;

				default:
					break;
			}
		}

		private void ApplyRestChanges(DateTime newDate)
		{
			TimeSpan change = _lastRestDate - newDate;
			float tirednessCost = (float)change.TotalHours * StaticValues.StaminaAwakeCost;
			float reducedStamina = Mathf.Clamp((MaxStamina + tirednessCost) - _staminaPenalty, MaxStamina * 0.5f, MaxStamina);
			SetCurrMaxStamina(HelperMethods.RoundFloat(reducedStamina, 3));
			// SetCurrMaxStamina(HelperMethods.RoundFloat(_currentMaxStamina + tirednessCost, 3));
		}

		internal void RestoreSavedState(CharacterSaveState savedState)
		{
			SetMaxHealth(savedState.MaxHealth);
			SetHealth(savedState.Health);
			Rest(savedState.LastRestDate);
			SetStamina(savedState.Stamina);
		}
	}
}