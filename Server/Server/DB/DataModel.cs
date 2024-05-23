using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server.DB
{
	[Table("Account")]
	public class AccountDb
	{
		public int AccountDbId { get; set; }
		public int AccountLoginId { get; set; }
		public ICollection<PlayerDb> Players { get; set; }
	}

	[Table("Player")]
	public class PlayerDb
	{
		public int PlayerDbId { get; set; }
		public string PlayerName { get; set; }
		public bool IsMale { get; set; }

		[ForeignKey("Account")]
		public int AccountDbId { get; set; }
		public AccountDb Account { get; set; }
		public ClassTypes PlayerClass { get; set; }
		public ICollection<ItemDb> Items { get; set; }
		public int Level { get; set; }
		public int Hp { get; set; }
		public int Mp { get; set; }
		public int MaxMp { get; set; }
		public int MaxHp { get; set; }
		public int MaxAttack { get; set; }
		public int MinAttack { get; set; }
		public int Defense { get; set; }
		public float Speed { get; set; }
		public int Str { get; set; }
		public int Dex { get; set; }
		public int Int { get; set; }
		public int Luk { get; set; }
		public int Exp { get; set; }
		public int StatPoint { get; set; }
		public float posX { get; set; }
		public float posY { get; set; }
		public float posZ { get; set; }
		public float rotateY { get; set; }
		public int Money { get; set; }
    }

    [Table("Item")]
	public class ItemDb
	{
		public int ItemDbId { get; set; }
		public int TemplateId { get; set; }
		public int Count { get; set; }
		public int Slot { get; set; }
		public bool Equipped { get; set; } = false;

		[ForeignKey("Owner")]
		public int? OwnerDbId { get; set; }
		public PlayerDb Owner { get; set; }
	}
}
