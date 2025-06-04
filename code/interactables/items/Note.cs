namespace ImmersiveSim.Gameplay
{
	public partial class Note : BaseItem
	{
		public override void UseItem(Inventory user)
		{
			base.UseItem(user);
			_game.UI.OpenNote(this);
		}
	}
}