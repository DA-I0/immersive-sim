namespace ImmersiveSim.GameData
{
	public class Objective
	{
		private bool _isActive;
		private bool _isOptional;
		private int _order;

		public bool IsActive
		{
			get { return _isActive; }
		}

		public bool IsOptional
		{
			get { return _isOptional; }
		}

		public Objective(bool isOptional, int order)
		{
			_isOptional = isOptional;
			_order = order;
		}

		public void SetActive(bool enable)
		{
			_isActive = enable;
		}
	}
}