namespace ImmersiveSim.Statics
{
	public static class StaticValues
	{
		// character movement
		public static float SpeedChangeStep = 0.05f;
		public static float Momentum = 0.075f;
		public static float MoveBackwardMultiplier = 0.7f;
		public static float CrouchToggleDelay = 150f;
		public static float MovementCost = 0.25f;
		public static float MaxStepHeight = 0.3f;
		public static float CollisionVelocityDamageThreshold = 9.0f;
		public static float CollisionDamageMultiplier = 2.0f;
		// character interaction
		public static float ItemPlacementRange = 1.5f;
		// character status
		public static int BaseMaxHealth = 100;
		public static int BaseMaxStamina = 100;
		public static int RegenDelay = 1000;
		public static int StaminaAwakeCost = 3;
		// world
		public static int NPCLimit = 10;
		public static float NPCDestructionDelay = 10.0f;
		public static float EntityParentResetDelay = 1.0f;
		public static float ProductRotationRange = 6f;
		public static int VolumeRadioMin = -70;
		public static int VolumeRadioMax = -35;
		// UI
		public static int CrosshairBaseSize = 10;
	}
}