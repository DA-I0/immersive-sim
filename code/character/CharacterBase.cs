using Godot;
using ImmersiveSim.GameData;
using ImmersiveSim.Statics;
using ImmersiveSim.Systems;

namespace ImmersiveSim.Gameplay
{
	public partial class CharacterBase : Node
	{
		public StatusUpdate SetForDestruction;

		[Export] private string _id;
		[Export] private bool _canBeSaved = false;

		protected bool _isModified = false;
		private bool _isReloading = false;

		protected CharacterSheet _sheet;
		protected Inventory _inventory;
		protected MovementBase _baseMovement;
		protected Status _status;

		protected GameSystem _game;

		public string ID
		{
			get { return _id; }
		}

		public bool IsModified
		{
			get { return _isModified; }
			set { _isModified = value; }
		}

		public bool IsReloading
		{
			get { return _isReloading; }
			// set { _isReloading = value; }
		}

		public CharacterSheet CharSheet
		{
			get { return _sheet; }
		}

		public Inventory CharInventory
		{
			get { return _inventory; }
		}

		public MovementBase BaseMovement
		{
			get { return _baseMovement; }
		}

		public Status CharStatus
		{
			get { return _status; }
		}

		internal GameSystem Game
		{
			get { return _game; }
		}

		public override void _Ready()
		{
			InitialBaseSetup();
			CallDeferred("RegisterForSaving");
		}

		protected virtual void InitialBaseSetup()
		{
			_sheet = GetNode<CharacterSheet>("../CharacterSheet");
			_inventory = GetNode<Inventory>("../CharacterEquipment");
			_baseMovement = GetParent<MovementBase>();
			_status = GetNode<Status>("../CharacterStatus");

			_game = GetNode<GameSystem>(ProjectSettings.GetSetting("global/GameSystemPath").ToString());
		}

		public override void _PhysicsProcess(double delta)
		{
			CallDeferred("CharacterModified");
		}

		protected void RegisterForSaving()
		{
			if (_canBeSaved)
			{
				if (!_game.Save.RegisterCharacterForSaving(this))
				{
					_isReloading = true;
					_isModified = false;
					CallDeferred("RestoreSavedState");
				}
			}
		}

		internal async virtual void RestoreSavedState()
		{
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

			CharacterSaveState saveState = _game.Save.GetCharacterSaveState(ID);
			RestoreSavedState(saveState);
		}

		private void RestoreSavedState(CharacterSaveState savedState)
		{
			_baseMovement.RestoreSavedState(savedState);
			_inventory.RestoreSavedState(savedState);
			_status.RestoreSavedState(savedState);
			_isReloading = false;
			_isModified = false;
		}

		public void Destroy()
		{
			_isModified = false;
			_baseMovement.Name = "set_for_destruction";
			GetParent().ProcessMode = ProcessModeEnum.Always;
			GetParent().QueueFree();
			SetForDestruction?.Invoke();
		}

		internal virtual void CharacterModified()
		{
			if (_isModified && !_isReloading)
			{
				_game.Save.UpdateCharacterCache(this);
				_isModified = false;
			}
		}
	}
}