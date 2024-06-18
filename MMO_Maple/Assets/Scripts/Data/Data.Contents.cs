using Google.Protobuf.Protocol;
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
        public SKillRange range;
    }
    public class SKillRange
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
        public string description;
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
}