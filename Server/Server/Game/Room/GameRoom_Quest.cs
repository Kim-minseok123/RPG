using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleAddQuest(Player player, C_AddQuest addQuestPacket)
        {
            if (player == null) return;
            if (_npc.TryGetValue(addQuestPacket.NpcId, out Npc npc) == false) return;
            QuestNpc questNpc = npc as QuestNpc;

            if (questNpc == null || questNpc.QuestList == null) return;
            if(questNpc.QuestList.Contains(addQuestPacket.QuestId) == false)
            {
                Console.WriteLine($"잘못된 npc 접근입니다. 접근자 : {player.Info.Name}");
            }
            Quest quest = Quest.MakeQuest(addQuestPacket.QuestId);
            if(quest == null)
            {
                Console.WriteLine("존재하지 않는 퀘스트 입니다.");
            }
            player.QuestInven.AddQuest(quest);
            // DB에도 저장해야함.
            
            // 클라 통보
            S_AddQuest addQuestOk = new S_AddQuest();
            addQuestOk.QuestId = addQuestPacket.QuestId;
            player.Session.Send(addQuestOk);
        }
        public void HandleClearQuest(Player player, C_ClearQuest clearQuestPacket)
        {
            if (player == null) return;
            if (_npc.TryGetValue(clearQuestPacket.NpcId, out Npc npc) == false) return;
            QuestNpc questNpc = npc as QuestNpc;

            if (questNpc == null || questNpc.QuestList == null) return;
            if (questNpc.QuestList.Contains(clearQuestPacket.QuestId) == false)
            {
                Console.WriteLine($"잘못된 npc 접근입니다. 접근자 : {player.Info.Name}");
            }
            Quest quest = player.QuestInven.GetQuest(clearQuestPacket.QuestId, clearQuestPacket.QuestType);
            if (quest == null)
            {
                Console.WriteLine("존재하지 않는 퀘스트 입니다.");
                return;
            }
            if (player.QuestInven.CheckQuestClear(quest.TemplateId, quest.QuestType) == true)
            {
                player.QuestInven.RemoveQuest(quest);
                player.QuestInven.FinishQuest(quest);
                // 보상 지급
                QuestReward questReward = quest.Reward;
                if (questReward != null)
                {
                    if(questReward.exp > 0)
                        player.RewardExp(questReward.exp);
                    if(questReward.money > 0)
                    {
                        C_AddItem addItem = new C_AddItem();
                        addItem.TemplateId = 1000;
                        addItem.Count = questReward.money;
                        addItem.IsBuy = false;
                        HandleAddItem(player, addItem);
                    }
                    if(questReward.itemId != -1)
                    {
                        C_AddItem addItem = new C_AddItem();
                        addItem.TemplateId = questReward.itemId;
                        addItem.Count = 1;
                        addItem.IsBuy = false;
                        HandleAddItem(player, addItem);
                    }
                }
                S_ClearQuest clearQuestOk = new S_ClearQuest();
                clearQuestOk.QuestId = quest.TemplateId;
                clearQuestOk.QuestType = quest.QuestType;
                player.Session.Send(clearQuestOk);
            }
            else
            {
                Console.WriteLine("퀘스트가 완료되지 않았습니다");
            }
        }
    }
}
