using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager 
{
    Dictionary<int, Quest>[] QuestList = new Dictionary<int, Quest>[(int)QuestType.MaxCount];
    Dictionary<int, Quest> FinishedQuest = new Dictionary<int, Quest>();

    public void Init()
    {
        for(int i = 0; i < QuestList.Length; i ++)
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
    public bool CheckClearQuest(int id, QuestType questType)
    {
        return QuestList[(int)questType].TryGetValue(id, out Quest quest) && quest.IsFinish;
    }
    public bool CheckIsFinishQuest(int id)
    {
        return FinishedQuest.TryGetValue(id,out Quest value);
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
