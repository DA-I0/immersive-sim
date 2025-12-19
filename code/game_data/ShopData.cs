namespace ImmersiveSim.GameData
{
	public readonly struct ShopData
	{
		public readonly string ID;
		public readonly string[] Products;
		// add price multipliers?

		public ShopData(string id, string[] products)
		{
			ID = id;
			Products = products;
		}
	}
}