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
		public int SkillPoint { get; set; }

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
	[Table("Skill")]
	public class SkillDb
	{
		public int SkillDbId { get; set; }
		public int TemplateId { get; set; }
		public int SkillLevel { get; set; }
        [ForeignKey("Player")]
        public int PlayerDbId { get; set; }
        public PlayerDb Player { get; set; }
    }
    [Table("QuickSlot")]
    public class QuickSlotDb
    {
        public int QuickSlotDbId { get; set; }
        public int TemplateId { get; set; }
        public string Slot { get; set; }
        [ForeignKey("Player")]
        public int PlayerDbId { get; set; }
        public PlayerDb Player { get; set; }
    }
	[Table("Quest")]
	public class QuestDb
	{
		public int QuestDbId { get; set; }
		public int TemplateId { get; set; }
		public bool IsFinish { get; set; }
		public bool IsCleard{ get; set; }
		public int QuestType { get; set; }
		public ICollection<QuestGoalDb> Goals { get; set; }
        [ForeignKey("Player")]
        public int PlayerDbId { get; set; }
        public PlayerDb Player { get; set; }
    }
	[Table("QuestGoal")]
	public class QuestGoalDb
	{
		public int QuestGoalDbId { get; set; }
		[ForeignKey("Quest")]
		public int OnwerQuestDbId { get; set; }
		public QuestDb Quest { get; set; }

		public int TemplateId { get; set; }
		public int Count { get; set; }
	}
}
