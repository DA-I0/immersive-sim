using System.Linq;
using Godot;

namespace ImmersiveSim.UI.Settings
{
	public partial class ControlSettings : Control
	{
		private Label _cameraSensitivityText;
		private HSlider _cameraSensitivity;
		private CheckButton _cameraDirectionVertical;

		private Node _controlDevices;
		private string _inputType = string.Empty;
		private string _inputActionToChange = string.Empty;
		private string _keybindToChange = string.Empty;

		private Control _keyRebindPopup;

		private SettingsMenu _settingsMenu;

		public override void _Ready()
		{
			_cameraSensitivityText = GetNode<Label>("SettingsList/CameraSensitivity/Header");
			_cameraSensitivity = GetNode<HSlider>("SettingsList/CameraSensitivity/Slider");
			_cameraDirectionVertical = GetNode<CheckButton>("SettingsList/CameraDirectionVertical/CheckButton");
			_controlDevices = GetNode("SettingsList/KeybindList");

			_settingsMenu = GetParent().GetParent<SettingsMenu>();
			_keyRebindPopup = _settingsMenu.GetNode<Control>("KeyRebindPopup");
		}

		internal void InitializeSettings()
		{
			PopulateKeybindControls();
			ToggleKeyRebindPopup(false);
			UpdateTextValues();
		}

		public override void _Input(InputEvent @event)
		{
			if (_inputActionToChange == string.Empty)
			{
				if (@event.IsActionPressed("ui_cancel") && !_keyRebindPopup.Visible)
				{
					_settingsMenu.Return();
				}

				return;
			}

			if (@event is InputEventJoypadButton || @event is InputEventJoypadMotion
				|| (@event is InputEventKey key && key.IsReleased()) || @event is InputEventMouseButton)
			{
				SaveNewKeybinding(@event);
			}
		}

		internal void UpdateOnToggle()
		{
			UpdateKeybindings();
			_controlDevices.GetNode<Control>("KeyboardMouse").Visible = false;
			_controlDevices.GetNode<Control>("Joypad").Visible = false;
		}

		internal void UpdateSettings()
		{
			_cameraSensitivity.Value = _settingsMenu.Game.Settings.CameraSensitivity;
			_cameraDirectionVertical.ButtonPressed = (_settingsMenu.Game.Settings.CameraVerticalDirection < 0);
			UpdateKeybindings();
		}

		internal void ApplySettings()
		{
			_settingsMenu.Game.Settings.CameraSensitivity = (float)_cameraSensitivity.Value;
			_settingsMenu.Game.Settings.CameraVerticalDirection = _cameraDirectionVertical.ButtonPressed ? -1 : 1;
		}

		private void UpdateTextValues()
		{
			_cameraSensitivityText.Text = $"{TranslationServer.Translate("HEADER_CAMERA_SENSITIVITY")}: {(float)_cameraSensitivity.Value}";
			_settingsMenu.FlagSettingsAsUnmodified(false);
		}

		private void ToggleKeyboardMouseInputs()
		{
			_controlDevices.GetNode<Control>("KeyboardMouse").Visible = !_controlDevices.GetNode<Control>("KeyboardMouse").Visible;
		}

		private void ToggleJoypadInputs()
		{
			_controlDevices.GetNode<Control>("Joypad").Visible = !_controlDevices.GetNode<Control>("Joypad").Visible;
		}

		private void PopulateKeybindControls()
		{
			var inputActions = InputMap.GetActions().Where(a => !a.ToString().StartsWith("ui_"));

			foreach (Control inputType in _controlDevices.GetChildren())
			{
				if (inputType is Button button)
				{
					continue;
				}

				foreach (var action in inputActions)
				{
					HBoxContainer actionContainer = new HBoxContainer
					{
						Name = action
					};

					Control spacer = new Control
					{
						CustomMinimumSize = new Vector2(8, 0)
					};
					actionContainer.AddChild(spacer);

					Label inputHeader = new Label
					{
						Text = $"INPUT_{action.ToString().ToUpper()}",
						SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand,
						SizeFlagsStretchRatio = 0.45f
					};

					actionContainer.AddChild(inputHeader);
					actionContainer.AddChild(CreateKeybindButton(inputType.Name.ToString(), action));
					actionContainer.AddChild(CreateKeybindButton(inputType.Name.ToString(), action));

					spacer = new Control
					{
						CustomMinimumSize = new Vector2(1, 0)
					};
					actionContainer.AddChild(spacer);

					inputType.AddChild(actionContainer);
				}
			}
		}

		private Button CreateKeybindButton(string inputType, string inputAction)
		{
			Button newButton = new Button
			{
				CustomMinimumSize = new Vector2(0, 30),
				SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand,
				SizeFlagsStretchRatio = 0.25f
			};

			newButton.Pressed += () => ToggleKeybindChange(inputType, inputAction, newButton.Text);
			return newButton;
		}

		private void UpdateKeybindings()
		{
			foreach (Node inputType in _controlDevices.GetChildren())
			{
				foreach (Node inputAction in inputType.GetChildren())
				{
					string[] inputEvents = _settingsMenu.Game.GameDatabase.GetInputSymbol(inputAction.Name, inputAction.GetParent().Name);
					inputAction.GetChild<Button>(2).Text = (inputEvents.Length > 0) ? inputEvents[0] : string.Empty;
					inputAction.GetChild<Button>(3).Text = (inputEvents.Length > 1) ? inputEvents[1] : string.Empty;
				}
			}

			ToggleKeybindChange(string.Empty, string.Empty, string.Empty);
		}

		private void ToggleKeybindChange(string inputType, string inputActionName, string inputValue) // TODO: rename
		{
			_inputActionToChange = inputActionName;
			_keybindToChange = inputValue;
			ToggleKeyRebindPopup(_inputActionToChange != string.Empty);
		}

		private void SaveNewKeybinding(InputEvent @event)
		{
			if (_inputActionToChange == string.Empty || @event.IsAction("ui_cancel"))
			{
				ToggleKeybindChange(string.Empty, string.Empty, string.Empty);
				return;
			}

			if (
				(_inputType == "Joypad" && (@event is not InputEventJoypadButton || @event is not InputEventJoypadMotion))
				|| (_inputType == "KeyboardMouse" && (@event is InputEventJoypadButton || @event is InputEventJoypadMotion))
			)
			{
				return;
			}

			_settingsMenu.Game.Settings.ChangeKeybinding(_inputActionToChange, _keybindToChange, @event);
			UpdateKeybindings();
			_settingsMenu.FlagSettingsAsUnmodified(false);
		}

		private void ToggleKeyRebindPopup(bool enable)
		{
			// TODO: replace statically provided "Keyboard" with active controller type keyboard/joypad
			// TODO: move dynamic value replacement to a more generic place
			_keyRebindPopup.GetNode<Label>("Background/Content").Text = TranslationServer.Translate("INFO_KEY_REBIND").ToString().Replace("<key_ui_cancel>", _settingsMenu.Game.GameDatabase.GetInputSymbol("ui_cancel", "Keyboard")[0]);
			_keyRebindPopup.Visible = enable;
		}
	}
}