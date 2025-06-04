using Godot;

namespace ImmersiveSim.GameData
{
	public struct TimeOfDayPreset
	{
		[Export] public readonly Gradient _sunColor;
		[Export] Curve _sunIntensity;
		[Export] private Gradient _moonColor;
		[Export] Curve _moonIntensity;
		[Export] Curve _shadowStrength;

	}
}