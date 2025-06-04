namespace ImmersiveSim.Gameplay
{
	public partial class Consumable : BaseItem
	{
		private int _usesLeft;

		private int Uses
		{
			get { return _game.GameDatabase.Items[_templateID].Uses; }
		}

		private float UseWeight
		{
			get { return _game.GameDatabase.Items[_templateID].UseWeight; }
		}

		private int HealthRecovery
		{
			get { return _game.GameDatabase.Items[_templateID].HealthRecovery; }
		}

		private int StaminaRecovery
		{
			get { return _game.GameDatabase.Items[_templateID].StaminaRecovery; }
		}

		public int UsesLeft
		{
			get { return _usesLeft; }
		}

		public int FillingLeft
		{
			get { return (int)(((float)_usesLeft / (float)Uses) * 100); }
		}

		protected override void InitialBaseSetup()
		{
			base.InitialBaseSetup();
			_usesLeft = Uses;
		}

		public override void UseItem(Inventory user)
		{
			base.UseItem(user);

			if (_usesLeft < 1)
			{
				return;
			}

			if (_stackCount > 1)
			{
				SplitStack(_stackCount - 1);
			}

			user.GetNode<Status>("../CharacterStatus").ChangeHealth(HealthRecovery);
			user.GetNode<Status>("../CharacterStatus").ChangeStamina(StaminaRecovery);
			_usesLeft--;
			UpdateItemInfo();
			CheckIfEmpty(user); // let player get rid of their own trash?
			MarkAsModified();
			// _isModified = true;
		}

		// internal override void RestoreSavedState(GameData.ItemSaveState savedState)
		// {
		// 	base.RestoreSavedState(savedState);
		// 	_usesLeft = savedState.UsesLeft;
		// }

		protected override void FinishSaveStateRestore()
		{
			base.FinishSaveStateRestore();
			GameData.ItemSaveState savedState = _game.Save.GetItemSaveState(ObjectID);
			_usesLeft = savedState.UsesLeft;
		}

		private void CheckIfEmpty(Inventory user)
		{
			if (_usesLeft > 0)
			{
				return;
			}

			user.ClearSelectedItem();
			Destroy();
		}
	}
}