using Godot;
using ImmersiveSim.Gameplay;

namespace ImmersiveSim.GameData
{
	public class CharacterSaveState
	{
		public string NodePath;
		public string CharacterID;
		public Vector3 Position;
		public Vector3 Rotation;
		public Vector3 Velocity;
		public float MaxHealth;
		public float Health;
		public System.DateTime LastRestDate;
		public float Stamina;
		public int Money;
		public string[] EquipmentItems = new string[9];

		public CharacterSaveState(
			string nodePath, string characterID, Vector3 position, Vector3 rotation, Vector3 velocity,
			float maxHealth, float health, System.DateTime lastRestDate, float stamina,
			int money, string[] equipmentItems
			)
		{
			NodePath = nodePath;
			CharacterID = characterID;
			Position = position;
			Rotation = rotation;
			Velocity = velocity;
			MaxHealth = maxHealth;
			Health = health;
			LastRestDate = lastRestDate;
			Stamina = stamina;
			Money = money;
			EquipmentItems = equipmentItems;
		}

		public void UpdateCharacterData(CharacterBase target)
		{
			if (target != null)
			{
				NodePath = target.GetPath();
				CharacterID = target.ID;
				Position = target.BaseMovement.GlobalPosition;
				Rotation = target.BaseMovement.GlobalRotation;
				Velocity = target.BaseMovement.Velocity;
				MaxHealth = target.CharStatus.MaxHealth;
				Health = target.CharStatus.Health;
				LastRestDate = target.CharStatus.LastRestDate;
				Stamina = target.CharStatus.Stamina;
				Money = target.CharInventory.Money;

				for (int index = 0; index < target.CharInventory._equipmentSlots.Length; index++)
				{

					if (target.CharInventory._equipmentSlots[index] != null)
					{
						EquipmentItems[index] = target.CharInventory._equipmentSlots[index].ObjectID;
					}
					else
					{
						EquipmentItems[index] = string.Empty;
					}
				}
			}
		}

		public override string ToString()
		{
			return $"{CharacterID} :: {NodePath}\n--> {EquipmentItems[0]}\n--> {EquipmentItems[1]}\n--> {EquipmentItems[2]}\n--> {EquipmentItems[3]}\n--> {EquipmentItems[4]}\n--> {EquipmentItems[5]}\n--> {EquipmentItems[6]}\n--> {EquipmentItems[7]}\n--> {EquipmentItems[8]}";
		}
	}
}
