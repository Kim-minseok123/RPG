using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Quest
    {
        public int TemplateId { get; private set; }
        public QuestType QuestType { get; private set; }
        public int DemandLevel { get; private set; }
        public int DemandQuest { get; private set; }
        public bool IsRepeated { get; private set; }
        public QuestReward Reward { get; private set; }
        public bool IsClear { get; protected set; }
        public Quest(QuestType questType)
        {
            QuestType = questType;
        }
        public static Quest MakeQuest(int id)
        {
            Quest quest = null;
            if (DataManager.QuestDict.TryGetValue(id, out QuestData questData) == false)
                return null;
            switch (questData.questType)
            {
                case QuestType.Battle:
                    quest = new BattleQuest(questData);
                    break;
                case QuestType.Collection:
                    quest = new CollectionQuest(questData);
                    break;
                case QuestType.Enter:
                    quest = new EnterQuest(questData);
                    break;
            }
            if(quest != null)
            {
                quest.TemplateId = questData.id;
                quest.IsRepeated = questData.isRepeated;
                quest.DemandQuest = questData.demandQuest;
                quest.DemandLevel = questData.demandLevel;
                quest.Reward = questData.reward;
                quest.IsClear = false;
            }
            return quest;
        }
    }
    public class BattleQuest : Quest
    {
        public List<BattleQuestGoals> goals;
        Dictionary<int, int> countDict = new Dictionary<int, int>();
        public BattleQuest(QuestData questData) : base(QuestType.Battle)
        {
            if (questData == null)
                return;
            countDict.Clear();
            goals = ((BattleQuestData)questData).goals;
            foreach (var goal in goals)
                countDict.Add(goal.enemyId, 0);
        }
        public void Update(BattleQuestGoals questGoals)
        {
            foreach (var goal in goals)
            {
                if(goal.enemyId == questGoals.enemyId)
                {
                    if (!countDict.ContainsKey(goal.enemyId)) return;
                    countDict[goal.enemyId] += questGoals.count;
                }
            }
            CheckQuestClear();
        }
        public void CheckQuestClear()
        {
            foreach (var goal in goals)
            {
                if (!countDict.ContainsKey(goal.enemyId)) return;
                if (goal.count > countDict[goal.enemyId]) 
                { 
                    IsClear = false;
                    return;
                }
            }
            IsClear = true;
        } 
    }
    public class CollectionQuest : Quest
    {
        public List<CollectionQuestGoals> goals;
        Dictionary<int, int> countDict = new Dictionary<int, int>();
        public CollectionQuest(QuestData questData) : base(QuestType.Collection)
        {
            if (questData == null)
                return;
            goals = ((CollectionQuestData)questData).goals;
            countDict.Clear();
            foreach (var goal in goals)
                countDict.Add(goal.collectionId, 0);
        }
        public void Update(CollectionQuestGoals questGoals)
        {
            foreach (var goal in goals)
            {
                if (goal.collectionId == questGoals.collectionId)
                {
                    if (!countDict.ContainsKey(goal.collectionId)) return;
                    countDict[goal.collectionId] += questGoals.count;
                }
            }
            CheckQuestClear();
        }
        public void CheckQuestClear()
        {
            foreach (var goal in goals)
            {
                if (!countDict.ContainsKey(goal.collectionId)) return;
                if (goal.count > countDict[goal.collectionId])
                {
                    IsClear = false;
                    return;
                }
            }
            IsClear = true;
        }
    }
    public class EnterQuest : Quest
    {
        public int goals;
        public int cur = -1;
        public EnterQuest(QuestData questData) : base(QuestType.Enter)
        {
            if (questData == null)
                return;
            goals = ((EnterQuestData)questData).goals;
        }
        public void Update(int goal)
        {
            cur = goal;
        }
        public void CheckQuestClear()
        {
            if (cur == goals)
                IsClear = true;
            else
                IsClear = false;
        }
    }
}
