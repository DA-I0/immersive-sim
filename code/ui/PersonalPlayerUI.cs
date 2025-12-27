using Godot;
using ImmersiveSim.Statics;

namespace ImmersiveSim.UI
{
	public partial class PersonalPlayerUI : Control
	{
		private UIManager _ui;

		public override void _Ready()
		{
			_ui = GetParent<UIManager>();
			SubscribeToEvents();
			SetReferences();
		}

		private void SubscribeToEvents()
		{
			_ui.StateUpdated += ToggleVisibility;
		}

		private void SetReferences()
		{
			GetNode<Button>("CategoryButtons/PlayerInventory").Pressed += () => _ui.ChangeUIState(UIState.Inventory);
			GetNode<Button>("CategoryButtons/PlayerJournal").Pressed += () => _ui.ChangeUIState(UIState.Journal);
			GetNode<Button>("CategoryButtons/ClosePersonalUI").Pressed += () => _ui.ChangeUIState(UIState.None);
		}

		private void ToggleVisibility(UIState currentState)
		{
			Visible = (currentState == UIState.Inventory) || (currentState == UIState.Journal);
		}
	}
}