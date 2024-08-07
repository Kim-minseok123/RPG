using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game
{
    public class QuestInventory
    {
        Dictionary<int, Quest>[] QuestList = new Dictionary<int, Quest>[(int)QuestType.MaxCount];
        Dictionary<int, Quest> FinishedQuest = new Dictionary<int, Quest>();
        Player player = null;
        public QuestInventory(Player player)
        {
            for (int i = 0; i < QuestList.Length; i++)
            {
                QuestList[i] = new Dictionary<int, Quest>();
            }
            this.player = player;
        }
        public void AddQuest(Quest quest)
        {
            QuestList[(int)quest.QuestType].TryAdd(quest.TemplateId, quest);
        }

        public void RemoveQuest(Quest quest)
        {
            QuestList[(int)quest.QuestType].Remove(quest.TemplateId);
        }

        public Quest GetQuest(int id, QuestType questType)
        {
            QuestList[(int)questType].TryGetValue(id, out Quest quest);
            return quest;
        }
        public List<Quest> GetAllQuest()
        {
            List<Quest> quests = new List<Quest>();
            foreach (var quest in QuestList)
            {
                quests.AddRange(quest.Values.ToList());
            }
            return quests;
        }
        public List<Quest> GetAllFinishQuest()
        {
            List<Quest> quests = FinishedQuest.Values.ToList();

            return quests;
        }
        public void UpdateQuestProgress<T>(QuestType questType, T questGoals)
        {
            var list = QuestList[(int)questType];

            foreach (var quest in list.Values)
            {
                if (quest.IsFinish) continue;

                switch (quest)
                {
                    case BattleQuest battleQuest when questGoals is BattleQuestGoals battleGoals:
                        if (battleQuest.Update(battleGoals))
                        {
                            S_QuestChangeValue changeValue = new S_QuestChangeValue();
                            changeValue.QuestId = battleQuest.TemplateId;
                            changeValue.QuestType = QuestType.Battle;
                            changeValue.TemplateId = battleGoals.enemyId;
                            changeValue.Count = battleGoals.count;
                            changeValue.IsFinish = battleQuest.IsFinish;
                            player?.Session.Send(changeValue);
                        }
                        break;
                    case CollectionQuest collectionQuest when questGoals is CollectionQuestGoals collectionGoals:
                        if (collectionQuest.Update(collectionGoals))
                        {
                            S_QuestChangeValue changeValue = new S_QuestChangeValue();
                            changeValue.QuestId = collectionQuest.TemplateId;
                            changeValue.QuestType = QuestType.Collection;
                            changeValue.TemplateId = collectionGoals.collectionId;
                            changeValue.Count = collectionGoals.count;
                            changeValue.IsFinish = collectionQuest.IsFinish;
                            player?.Session.Send(changeValue);
                        }
                        break;
                    case EnterQuest enterQuest when questGoals is int enterGoals:
                        if (enterQuest.Update(enterGoals))
                        {
                            S_QuestChangeValue changeValue = new S_QuestChangeValue();
                            changeValue.QuestId = enterQuest.TemplateId;
                            changeValue.QuestType = QuestType.Enter;
                            changeValue.IsFinish = enterQuest.IsFinish;
                            player?.Session.Send(changeValue);
                        }
                        break;
                }
            }
        }
        public bool CheckQuestClear(int id, QuestType questType)
        {
            return QuestList[(int)questType].TryGetValue(id, out Quest quest) && quest.IsFinish;
        }
        public bool CheckIsFinishQuest(int id)
        {
            return FinishedQuest.TryGetValue(id, out Quest value);
        }
        public void Clear()
        {
            foreach (var quest in QuestList)
            {
                quest.Clear();
            }
        }
        public void FinishQuest(Quest quest)
        {
            FinishedQuest.TryAdd(quest.TemplateId, quest);
        }
    }
}
