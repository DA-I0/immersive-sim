namespace ImmersiveSim.GameData
{
	public readonly struct StoreData
	{
		public readonly string ID;
		public readonly string[] Products;
		// add price multipliers?

		public StoreData(string id, string[] products)
		{
			ID = id;
			Products = products;
		}
	}
}