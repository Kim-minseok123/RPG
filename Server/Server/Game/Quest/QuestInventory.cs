using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class QuestInventory
    {
        Dictionary<int, Quest>[] QuestList = new Dictionary<int, Quest>[(int)QuestType.MaxCount];
        public QuestInventory()
        {
            for (int i = 0; i < QuestList.Length; i++)
            {
                QuestList[i] = new Dictionary<int, Quest>();
            }
        }
        public void AddQuest(Quest quest)
        {
            QuestList[(int)quest.QuestType].Add(quest.TemplateId, quest);
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

        public void UpdateQuestProgress<T>(QuestType questType, T questGoals)
        {
            var list = QuestList[(int)questType];

            foreach (var quest in list.Values)
            {
                if (quest.IsClear) continue;

                switch (quest)
                {
                    case BattleQuest battleQuest when questGoals is BattleQuestGoals battleGoals:
                        battleQuest.Update(battleGoals);
                        break;
                    case CollectionQuest collectionQuest when questGoals is CollectionQuestGoals collectionGoals:
                        collectionQuest.Update(collectionGoals);
                        break;
                    case EnterQuest enterQuest when questGoals is int enterGoals:
                        enterQuest.Update(enterGoals);
                        break;
                }
            }
        }
        public bool CheckQuestClear(int id, QuestType questType)
        {
            return QuestList[(int)questType].TryGetValue(id, out Quest quest) && quest.IsClear;
        }
    }
}
