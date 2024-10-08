﻿using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    #region Skill
    [Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public float cooldown;
        public SkillType skillType;
        public int masterLevel;
        public bool isContinual;
        public string description;
        public int mpConsume;
    }
    [Serializable]
    public class AttackSkill : Skill
    {
        public List<SkillDataInfo> skillDatas;
    }
    [Serializable]
    public class BuffSkill : Skill
    {
        public int duration;
        public int skillLevelInc;
    }
    public class SkillDataInfo
    {
        public float attackTime;
        public int damage;
        public int skillLevelInc;
        public SkillRange range;
    }
    public class SkillRange
    {
        public float width;
        public float depth;
        public float nonDepth;
        public float height;
    }

    [Serializable]
    public class SkillData : ILoader<int, Skill>
    {
        public List<AttackSkill> attackSkills = new List<AttackSkill>();
        public List<BuffSkill> buffSkills = new List<BuffSkill>();

        public Dictionary<int, Skill> MakeDict()
        {
            Dictionary<int, Skill> dict = new Dictionary<int, Skill>();
            foreach (Skill skill in attackSkills)
            {
                dict.Add(skill.id, skill);
            }
            foreach (Skill skill in buffSkills)
            {
                dict.Add(skill.id, skill);
            }
            return dict;
        }
    }
    #endregion
    #region Item
    [Serializable]
	public class ItemData
	{
		public int id;
		public string name;
		public ItemType itemType;
		public string iconPath;
        public int sellGold;
        public string description;
    }

    [Serializable]
	public class WeaponData : ItemData
	{
		public WeaponType weaponType;
		public int damage;
        public int requirementLevel;
        public string requirementClass;
    }

    [Serializable]
	public class ArmorData : ItemData
	{
		public ArmorType armorType;
		public int defence;
        public int requirementLevel;
        public string requirementClass;
    }

    [Serializable]
	public class ConsumableData : ItemData
	{
		public ConsumableType consumableType;
		public int maxCount;
        public int healVal;
    }


	[Serializable]
	public class ItemLoader : ILoader<int, ItemData>
	{
		public List<WeaponData> weapons = new List<WeaponData>();
		public List<ArmorData> armors = new List<ArmorData>();
		public List<ConsumableData> consumables = new List<ConsumableData>();

		public Dictionary<int, ItemData> MakeDict()
		{
			Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
			foreach (ItemData item in weapons)
			{
				item.itemType = ItemType.Weapon;
				dict.Add(item.id, item);
			}
			foreach (ItemData item in armors)
			{
				item.itemType = ItemType.Armor;
				dict.Add(item.id, item);
			}
			foreach (ItemData item in consumables)
			{
				item.itemType = ItemType.Consumable;
				dict.Add(item.id, item);
			}
			return dict;
		}
	}
	#endregion
    #region Monster
    [Serializable]
    public class RewardData
    {
        public int probability; // 100분율
        public int itemId;
        public int count;
    }

    [Serializable]
    public class Monster
    {
        public int id;
        public string name;
        public StatInfo stat;
        public List<RewardData> rewards;
        //public string prefabPath;
    }

    [Serializable]
    public class MonsterData : ILoader<int, Monster>
    {
        public List<Monster> monsters = new List<Monster>();

        public Dictionary<int, Monster> MakeDict()
        {
            Dictionary<int, Monster> dict = new Dictionary<int, Monster>();
            foreach (Monster monster in monsters)
            {
                dict.Add(monster.id, monster);
            }
            return dict;
        }
    }

    #endregion
    #region Exp
    [Serializable]
    public class ExpData
    {
        public int level;
        public int requiredExp;
    }
    [Serializable]
    public class ExpLoader : ILoader<int, ExpData>
    {
        public List<ExpData> exps = new List<ExpData>();

        public Dictionary<int, ExpData> MakeDict()
        {
            Dictionary<int, ExpData> dict = new Dictionary<int, ExpData>();
            foreach (ExpData exp in exps)
            {
                dict.Add(exp.level, exp);
            }
            return dict;
        }
    }
    #endregion
    #region Npc
    [Serializable]
    public class NpcData
    {
        public int id;
        public string name;
        public List<NpcSellList> npcSellLists;
        public List<int> npcQuestLists;
        public string questNonString;
    }

    [Serializable]
    public class NpcLoader : ILoader<int, NpcData>
    {
        public List<NpcData> npcs = new List<NpcData>();

        public Dictionary<int, NpcData> MakeDict()
        {
            Dictionary<int, NpcData> dict = new Dictionary<int, NpcData>();
            foreach (NpcData npc in npcs)
            {
                dict.Add(npc.id, npc);
            }
            return dict;
        }
    }
    #endregion
    #region Quest
    [Serializable]
    public class QuestData
    {
        public int id;
        public QuestType questType;
        public string questTitle;
        public int demandLevel;
        public int demandQuest;
        public string questSummary;
        public bool isRepeated;
        public QuestReward reward;
        public List<string> questDescription;
        public string questAcceptString;
        public string questRefuseString;
        public string questClearString;
        public string questDetailString;
        public string questNonClearString;
        public string questNpcIconPath;
        public string questItemIconPath;
        public string questLocationString;
        public string goalText;
    }
    [Serializable]
    public class QuestReward
    {
        public int exp;
        public int money;
        public int itemId;
    }
    [Serializable]
    public class BattleQuestData : QuestData 
    {
        public List<BattleQuestGoals> goals;
    }
    [Serializable]
    public class BattleQuestGoals
    {
        public int enemyId;
        public int count;
    }
    [Serializable]
    public class CollectionQuestData : QuestData
    {
        public List<CollectionQuestGoals> goals;
    }
    [Serializable]
    public class CollectionQuestGoals
    {
        public int collectionId;
        public int count;
    }
    [Serializable]
    public class EnterQuestData : QuestData
    {
        public int goals;
    }
    [Serializable]
    public class QuestLoader : ILoader<int, QuestData>
    {
        public List<BattleQuestData> battles = new List<BattleQuestData>();
        public List<CollectionQuestData> collections = new List<CollectionQuestData>();
        public List<EnterQuestData> enters = new List<EnterQuestData>();

        public Dictionary<int, QuestData> MakeDict()
        {
            Dictionary<int, QuestData> dict = new Dictionary<int, QuestData>();
            foreach (QuestData quest in battles)
            {
                quest.questType = QuestType.Battle;
                dict.Add(quest.id, quest);
            }
            foreach (QuestData quest in collections)
            {
                quest.questType = QuestType.Collection;
                dict.Add(quest.id, quest);
            }
            foreach (QuestData quest in enters)
            {
                quest.questType = QuestType.Enter;
                dict.Add(quest.id, quest);
            }
            return dict;
        }
    }
    #endregion
}