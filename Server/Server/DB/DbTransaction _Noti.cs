using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.DB
{
	public partial class DbTransaction : JobSerializer
	{
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped,
                Slot = item.Slot,
            };

            Instance.Push(() => 
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Equipped)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(ItemDb.Slot)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        // 실패했다면 유저를 Kick
                    }
                }
            });
        }
        public static void RemoveItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ItemDb itemDb = db.Items.SingleOrDefault(i => i.ItemDbId == item.ItemDbId && i.OwnerDbId == player.PlayerDbId);
                    if (itemDb != null)
                    {
                        db.Items.Remove(itemDb);
                        bool success = db.SaveChangesEx();
                        if (!success)
                        {
                            // 실패 시 로직 추가 (로그 작성 등)
                        }
                    }
                }
            });
        }
        public static void UseItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Count = item.Count,
            };

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        // 실패 시 로직 추가 (로그 작성 등)
                    }
                }
            });
        }
        public static void ExpNoti(Player player, bool levelUp = false)
        {
            if (player == null)
                return;

            PlayerDb playerDb = new PlayerDb()
            {
                PlayerDbId = player.PlayerDbId,
                Exp = player.Stat.Exp,
            };
            if (levelUp)
            {
                playerDb.Level = player.Stat.Level;
                playerDb.MaxMp = player.Stat.MaxMp;
                playerDb.MaxHp = player.Stat.MaxHp;
                playerDb.StatPoint = player.Stat.StatPoint;
                playerDb.SkillPoint = player.Stat.SkillPoint;
            }

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Exp)).IsModified = true;
                    if (levelUp)
                    {
                        db.Entry(playerDb).Property(nameof(PlayerDb.Level)).IsModified = true;
                        db.Entry(playerDb).Property(nameof(PlayerDb.MaxHp)).IsModified = true;
                        db.Entry(playerDb).Property(nameof(PlayerDb.MaxMp)).IsModified = true;
                        db.Entry(playerDb).Property(nameof(PlayerDb.StatPoint)).IsModified = true;
                        db.Entry(playerDb).Property(nameof(PlayerDb.SkillPoint)).IsModified = true;
                    }
                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        
                    }
                }
            });
        }
        public static void SkillLevelNoti(Player player, SkillInfo skillInfo)
        {
            if (player == null || skillInfo == null || skillInfo.Level <= 0) return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    bool success = false;

                    if (skillInfo.Level == 1)
                    {
                        SkillDb skillDb = new SkillDb
                        {
                            PlayerDbId = player.PlayerDbId,
                            TemplateId = skillInfo.SkillId,
                            SkillLevel = skillInfo.Level
                        };
                        db.Skills.Add(skillDb);
                    }
                    else
                    {
                        SkillDb skillDb = db.Skills.SingleOrDefault(s => s.PlayerDbId == player.PlayerDbId && s.TemplateId == skillInfo.SkillId);
                        if (skillDb != null)
                        {
                            skillDb.SkillLevel = skillInfo.Level;
                            db.Entry(skillDb).State = EntityState.Modified;
                        }
                    }
                    success = db.SaveChangesEx();

                    if (!success)
                    {
                        // 실패 시 로깅 또는 예외 처리
                        Console.WriteLine("Skill level update failed for PlayerDbId: " + player.PlayerDbId);
                    }
                }
            });
        }
        public static void SaveQuickSlotNoti(Player player, C_SaveQuickSlot saveQuickSlot)
        {
            if (player == null) return;
            foreach (var info in saveQuickSlot.Info)
            {
                Instance.Push(() =>
                {
                    using (AppDbContext db = new AppDbContext()) 
                    {
                        QuickSlotDb existQuickSlot = db.QuickSlots.SingleOrDefault(q => q.PlayerDbId == player.PlayerDbId && q.TemplateId == info.TemplateId && q.Slot != info.SlotName);
                        if (existQuickSlot != null)
                        {
                            db.QuickSlots.Remove(existQuickSlot);
                        }

                        QuickSlotDb quickSlotDb = db.QuickSlots.SingleOrDefault(q => q.PlayerDbId == player.PlayerDbId && q.Slot == info.SlotName);
                        if(quickSlotDb != null)
                        {
                            quickSlotDb.TemplateId = info.TemplateId;
                            db.Entry(quickSlotDb).State = EntityState.Modified;
                        }
                        else
                        {
                            quickSlotDb = new QuickSlotDb()
                            {
                                PlayerDbId = player.PlayerDbId,
                                Slot = info.SlotName,
                                TemplateId = info.TemplateId
                            };
                            db.QuickSlots.Add(quickSlotDb);
                        }
                        bool success = db.SaveChangesEx();
                        if (!success)
                        {

                        }
                    }
                });
            }
        }
        public static void ChangeItemSlotNoti(Player player, Item curItem, Item pointItem = null)
        {
            if (player == null) return;

            ItemDb item1 = new ItemDb()
            {
                ItemDbId = curItem.ItemDbId,
                Count = curItem.Count,
                Slot = curItem.Slot,
            };
            ItemDb item2 = null;
            if(pointItem != null)
            {
                item2 = new ItemDb()
                {
                    ItemDbId = pointItem.ItemDbId,
                    Count = pointItem.Count,
                    Slot = pointItem.Slot,
                };
            }

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                {
                    if(curItem.Slot == -1)
                    {
                        ItemDb itemDb = db.Items.SingleOrDefault(i => i.ItemDbId == curItem.ItemDbId);
                        db.Items.Remove(itemDb);
                    }
                    else
                    {
                        db.Entry(item1).State = EntityState.Unchanged;
                        db.Entry(item1).Property(nameof(ItemDb.Slot)).IsModified = true;
                        db.Entry(item1).Property(nameof(ItemDb.Count)).IsModified = true;
                    }
                    if(item2 != null)
                    {
                        db.Entry(item2).State = EntityState.Unchanged;
                        db.Entry(item2).Property(nameof(ItemDb.Slot)).IsModified = true;
                        db.Entry(item2).Property(nameof(ItemDb.Count)).IsModified = true;
                    }
                    bool success = db.SaveChangesEx();
                    if (!success)
                    {

                    }
                }
            });
        }
        public static void AddQuestNoti(Player player, Quest quest)
        {
            if (quest == null || player == null)
                return;
            var questGoals = new List<QuestGoalDb>();
            
            switch (quest.QuestType)
            {
                case QuestType.Battle:
                    BattleQuest battleQuest = (BattleQuest)quest;
                    foreach (var key in battleQuest.countDict.Keys)
                        questGoals.Add(new QuestGoalDb { TemplateId = key, Count = battleQuest.countDict[key] });
                    break;
                case QuestType.Collection:
                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                    foreach (var key in collectionQuest.countDict.Keys)
                        questGoals.Add(new QuestGoalDb { TemplateId = key, Count = collectionQuest.countDict[key] });
                    break;
                case QuestType.Enter:
                    EnterQuest enterQuest = (EnterQuest)quest;
                    questGoals.Add(new QuestGoalDb { TemplateId = enterQuest.cur, Count = 1 });
                    break;
            }

            QuestDb questDb = new QuestDb()
            {
                TemplateId = quest.TemplateId,
                QuestType = (int)quest.QuestType,
                IsFinish = quest.IsFinish,
                PlayerDbId = player.PlayerDbId,
                IsCleard = false,
                Goals = questGoals
            };
            foreach (var goal in questGoals)
            {
                goal.Quest = questDb;
            }
            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Quests.Add(questDb);
                    bool success = db.SaveChangesEx();
                    if (!success)
                    {

                    }
                }
            });
        }
        public static void ClearQuestNoti(Player player, Quest quest)
        {
            if (quest == null || player == null)
                return;
            var questGoals = new List<QuestGoalDb>();

            switch (quest.QuestType)
            {
                case QuestType.Battle:
                    BattleQuest battleQuest = (BattleQuest)quest;
                    foreach (var key in battleQuest.countDict.Keys)
                        questGoals.Add(new QuestGoalDb { TemplateId = key, Count = battleQuest.countDict[key] });
                    break;
                case QuestType.Collection:
                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                    foreach (var key in collectionQuest.countDict.Keys)
                        questGoals.Add(new QuestGoalDb { TemplateId = key, Count = collectionQuest.countDict[key] });
                    break;
                case QuestType.Enter:
                    EnterQuest enterQuest = (EnterQuest)quest;
                    questGoals.Add(new QuestGoalDb { TemplateId = enterQuest.cur, Count = 1 });
                    break;
            }

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                {
                    QuestDb qdb = db.Quests
                    .Include(q => q.Goals)
                    .Where(q => q.TemplateId == quest.TemplateId && !q.IsCleard && q.PlayerDbId == player.PlayerDbId)
                    .FirstOrDefault();

                    if (qdb != null)
                    {
                        qdb.IsCleard = true;
                        qdb.IsFinish = quest.IsFinish;

                        db.QuestGoals.RemoveRange(qdb.Goals);

                        foreach (var goal in questGoals)
                        {
                            goal.OnwerQuestDbId = qdb.QuestDbId;  
                            db.QuestGoals.Add(goal);
                        }
                        db.Entry(qdb).State = EntityState.Modified;

                        bool success = db.SaveChangesEx();
                        if (!success)
                        {
                        }
                    }
                }
            });
        }
        public static void UpdateQuest(Player player, Quest quest)
        {
            if (quest == null || player == null)
                return;
            var questGoals = new List<QuestGoalDb>();

            switch (quest.QuestType)
            {
                case QuestType.Battle:
                    BattleQuest battleQuest = (BattleQuest)quest;
                    foreach (var key in battleQuest.countDict.Keys)
                        questGoals.Add(new QuestGoalDb { TemplateId = key, Count = battleQuest.countDict[key] });
                    break;
                case QuestType.Collection:
                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                    foreach (var key in collectionQuest.countDict.Keys)
                        questGoals.Add(new QuestGoalDb { TemplateId = key, Count = collectionQuest.countDict[key] });
                    break;
                case QuestType.Enter:
                    EnterQuest enterQuest = (EnterQuest)quest;
                    questGoals.Add(new QuestGoalDb { TemplateId = enterQuest.cur, Count = 1 });
                    break;
            }
            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                {
                    QuestDb qdb = db.Quests
                    .Include(q => q.Goals)
                    .Where(q => q.TemplateId == quest.TemplateId && !q.IsCleard && q.PlayerDbId == player.PlayerDbId)
                    .FirstOrDefault();

                    if (qdb != null)
                    {
                        qdb.IsFinish = quest.IsFinish;

                        db.QuestGoals.RemoveRange(qdb.Goals);

                        foreach (var goal in questGoals)
                        {
                            goal.OnwerQuestDbId = qdb.QuestDbId;
                        }

                        db.QuestGoals.AddRange(questGoals); 
                        db.Entry(qdb).State = EntityState.Modified;

                        bool success = db.SaveChangesEx();
                        if (!success)
                        {
                        }
                    }
                }
            });
        }
    }
}
