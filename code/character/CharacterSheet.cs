using Godot;

namespace ImmersiveSim.Gameplay
{
	public partial class CharacterSheet : Node
	{
		private string _name = "Player name";
		private float _height = 1.8f;
		private float _weight = 70f;
		private float _bmi = 21.7f;
		// possible stats:
		// 0 - strength/body
		// 1 - 
		// 2 - 
		// 3 - charisma?
		// 4 - luck?
		private int[] _stats = { 3, 1, 1, 1, 1 };

		public string CharacterName
		{
			get { return _name; }
		}

		public float Height
		{
			get { return _height; }
		}

		public float Weight
		{
			get { return _weight; }
		}

		public int[] Stats
		{
			get { return _stats; }
		}

		// < 18.5      - underweight
		// 18.5 - 24.9 - normal weight, 
		// >= 25       - overweight
		public float BMI
		{
			get { return _bmi; }
		}

		// take BMI 21.7 as base (1.0) multiplier for affected mechanics?
		// used for: MaxHealth
		public float BMIRatio
		{
			get { return BMI / 21.7f; }
		}

		// take BMI 21.7 as base (1.0) multiplier for affected mechanics?
		// used for: MaxSpeed?
		public float BMIInversedRatio
		{
			get { return 21.7f / BMI; }
		}

		public void SetPlayerCharacter(string name, float height, float weight, int strength)
		{
			_name = name;
			_height = height;
			_weight = weight;

			_stats[0] = strength;
			// _stats[1] = ;

			RecalculateValues();
		}

		public void RecalculateValues()
		{
			// _bmi = HelperMethods.RoundFloat(_weight / Mathf.Pow(_height, 2));          // Classic BMI: weight / height ^ 2
			_bmi = Statics.HelperMethods.RoundFloat(1.3f * (_weight / Mathf.Pow(_height, 2.5f))); // Updated BMI: 1.3 * (weight / height ^ 2.5)
		}
	}
}